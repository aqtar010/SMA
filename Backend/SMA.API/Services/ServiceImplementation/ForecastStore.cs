using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Options;
using SMA.API.Data;
using SMA.API.Entities;
using SMA.InventoryForecasting;

namespace SMA.API.Services.ServiceImplementation;

public sealed class ForecastStore(
    AppDbContext context,
    IDistributedCache cache,
    IOptions<ForecastOptions> options,
    ILogger<ForecastStore> logger) : IForecastStore
{
    private const string CacheKey = "sma:inventory-forecast:v2";
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<InventoryForecast?> GetLatestAsync(CancellationToken cancellationToken)
    {
        try
        {
            var cached = await cache.GetStringAsync(CacheKey, cancellationToken);
            if (!string.IsNullOrWhiteSpace(cached))
            {
                var forecast = JsonSerializer.Deserialize<InventoryForecast>(cached, JsonOptions);
                if (forecast is not null && !(forecast.Status == "Unavailable" && forecast.Recommendations.Count == 0))
                {
                    return forecast;
                }
            }
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Unable to read inventory forecast from Redis.");
        }

        var snapshot = await context.InventoryForecastSnapshots
            .AsNoTracking()
            .Where(item => item.Status != "Unavailable" || item.RecommendationsJson != "[]")
            .OrderByDescending(item => item.GeneratedAt)
            .FirstOrDefaultAsync(cancellationToken);
        return snapshot is null ? null : Map(snapshot);
    }

    public async Task SaveAsync(InventoryForecast forecast, CancellationToken cancellationToken)
    {
        var entity = await context.InventoryForecastSnapshots.FindAsync([forecast.Id], cancellationToken);
        var snapshot = MapToEntity(forecast);

        if (entity is null)
        {
            context.InventoryForecastSnapshots.Add(snapshot);
        }
        else
        {
            context.Entry(entity).CurrentValues.SetValues(snapshot);
        }

        await context.SaveChangesAsync(cancellationToken);

        try
        {
            await cache.SetStringAsync(CacheKey, JsonSerializer.Serialize(forecast, JsonOptions), new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(options.Value.CacheHours)
            }, cancellationToken);
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Unable to cache inventory forecast in Redis.");
        }
    }

    private static InventoryForecastSnapshot MapToEntity(InventoryForecast forecast) => new()
    {
        Id = forecast.Id,
        GeneratedAt = forecast.GeneratedAt,
        HistorySince = forecast.HistorySince,
        HistoryUntil = forecast.HistoryUntil,
        NextRefreshAt = forecast.NextRefreshAt,
        Status = forecast.Status,
        Error = forecast.Error,
        RecommendationsJson = JsonSerializer.Serialize(forecast.Recommendations, JsonOptions)
    };

    private static InventoryForecast Map(InventoryForecastSnapshot snapshot) => new(
        snapshot.Id,
        snapshot.GeneratedAt,
        snapshot.HistorySince,
        snapshot.HistoryUntil,
        snapshot.NextRefreshAt,
        snapshot.Status,
        snapshot.Error,
        JsonSerializer.Deserialize<IReadOnlyList<ForecastRecommendation>>(snapshot.RecommendationsJson, JsonOptions) ?? []);
}
