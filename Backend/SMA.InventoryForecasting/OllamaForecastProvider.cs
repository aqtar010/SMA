using System.Net.Http.Json;
using System.Text.Json;
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
            You are an inventory planning assistant. Return only valid JSON with this shape:
            {"recommendations":[{"productId":"GUID","predictedDemand":0,"reorderPoint":0,"reorderRecommended":false,"trendPercent":0,"insight":"short advisory sentence"}]}
            Use only the supplied product IDs. Predicted demand is total units for the next {{input.ForecastHorizonDays}} days. Reorder point must be a non-negative integer. Do not invent products.
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

        var payload = await response.Content.ReadFromJsonAsync<OllamaResponse>(JsonOptions, cancellationToken)
            ?? throw new InvalidOperationException("Ollama returned an empty response.");

        if (string.IsNullOrWhiteSpace(payload.Response))
        {
            logger.LogWarning("Ollama returned an empty response body for model {Model}.", options.Model);
            throw new InvalidOperationException("Ollama returned an empty forecast response.");
        }
        var output = JsonSerializer.Deserialize<ForecastOutput>(payload.Response, JsonOptions)
            ?? throw new InvalidOperationException("Ollama returned invalid forecast JSON.");

        var validProducts = input.Products.Select(product => product.ProductId).ToHashSet();
        var recommendations = output.Recommendations ?? [];
        if (recommendations.Any(item => !validProducts.Contains(item.ProductId) || item.PredictedDemand < 0 || item.ReorderPoint < 0))
        {
            throw new InvalidOperationException("Ollama returned an invalid product recommendation.");
        }

        logger.LogInformation("Generated inventory forecast for {ProductCount} products using {Model}.", recommendations.Count, options.Model);
        return recommendations;
    }

    private sealed record OllamaResponse(string Response, string? DoneReason, bool Done);
    private sealed record ForecastOutput(IReadOnlyList<ForecastRecommendation>? Recommendations);
}
