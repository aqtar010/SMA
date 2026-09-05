using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;

namespace SMA.InventoryForecasting;

public sealed class OllamaForecastProvider(HttpClient httpClient, ILogger<OllamaForecastProvider> logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<IReadOnlyList<ForecastRecommendation>> GenerateAsync(
        ForecastInput input,
        ForecastOptions options,
        CancellationToken cancellationToken)
    {
        var prompt = $$"""
            You are an inventory planning assistant for a grocery store.
            Return only valid JSON with this exact shape:
            {"recommendations":[{"productId":"GUID","predictedDemand":0,"reorderPoint":0,"reorderRecommended":false,"trendPercent":0,"insight":"short sentence"}]}
            Evaluate only the candidate products in the input. Do not invent products or include extra fields.
            Keep the insight to one short sentence under 16 words.
            Use the provided data only. Predicted demand is total units for the next {{input.ForecastHorizonDays}} days.
            Account for expiry: expired stock must not count as usable supply, and stock expiring within 14 days needs an urgent short-dated inventory insight.
            Input:
            {{JsonSerializer.Serialize(input, JsonOptions)}}
            """;

        using var response = await httpClient.PostAsJsonAsync("api/generate", new
        {
            model = options.Model,
            prompt,
            stream = false,
            format = "json",
            options = new { num_ctx = 16384 }
        }, JsonOptions, cancellationToken);
        
        response.EnsureSuccessStatusCode();

        if (response.Content is null)
        {
            logger.LogWarning("Ollama returned success status {Status} but no content for model {Model}. Request: {Request}",
                response.StatusCode, options.Model, response.RequestMessage);
            throw new InvalidOperationException("Ollama returned an empty HTTP response body.");
        }

        var raw = await response.Content.ReadAsStringAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(raw))
        {
            logger.LogWarning("Ollama returned an empty response body string for model {Model}. Status: {Status}. Headers: {Headers}",
                options.Model, response.StatusCode, response.Headers);
            throw new InvalidOperationException("Ollama returned an empty forecast response.");
        }

        var payload = JsonSerializer.Deserialize<OllamaResponse>(raw, JsonOptions)
            ?? throw new InvalidOperationException("Ollama returned invalid JSON.");
        var generatedJson = string.IsNullOrWhiteSpace(payload.Response) ? payload.Thinking : payload.Response;
        if (string.IsNullOrWhiteSpace(generatedJson))
        {
            throw new InvalidOperationException("Ollama returned an empty forecast response.");
        }

        var output = JsonSerializer.Deserialize<ForecastOutput>(generatedJson, JsonOptions)
            ?? throw new InvalidOperationException("Ollama returned invalid forecast JSON.");

        var validProducts = input.Products.Select(product => product.ProductId).ToHashSet();
        var recommendations = output.Recommendations ?? [];
        if (recommendations.Any(item => !validProducts.Contains(item.ProductId) || item.PredictedDemand < 0 || item.ReorderPoint < 0))
        {
            throw new InvalidOperationException("Ollama returned an invalid product recommendation.");
        }

        logger.LogInformation("Generated inventory forecast with {RecommendationCount} recommendations using {Model} from {Source}.",
            recommendations.Count, options.Model, string.IsNullOrWhiteSpace(payload.Response) ? "thinking" : "response");
        return recommendations;
    }

    private sealed record OllamaResponse(
        [property: JsonPropertyName("response")] string? Response,
        [property: JsonPropertyName("thinking")] string? Thinking,
        [property: JsonPropertyName("done_reason")] string? DoneReason,
        [property: JsonPropertyName("done")] bool Done);

    private sealed record ForecastOutput(
        [property: JsonPropertyName("recommendations")] IReadOnlyList<ForecastRecommendation>? Recommendations);
}
