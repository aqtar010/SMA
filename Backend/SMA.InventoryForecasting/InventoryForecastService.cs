using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace SMA.InventoryForecasting;

public sealed class InventoryForecastService(
    IForecastDataSource dataSource,
    IForecastStore store,
    IForecastRefreshLock refreshLock,
    OllamaForecastProvider provider,
    IOptions<ForecastOptions> options,
    ILogger<InventoryForecastService> logger) : IInventoryForecastService
{
    public Task<InventoryForecast?> GetLatestAsync(CancellationToken cancellationToken) =>
        store.GetLatestAsync(cancellationToken);

    public async Task<InventoryForecast> RefreshAsync(CancellationToken cancellationToken)
    {
        var settings = options.Value;
        await using var lease = await refreshLock.TryAcquireAsync(
            TimeSpan.FromHours(settings.RefreshIntervalHours), cancellationToken);
        if (lease is null)
        {
            return await store.GetLatestAsync(cancellationToken)
                ?? EmptyForecast(DateTime.UtcNow, "Waiting for another refresh.");
        }

        var now = DateTime.UtcNow;
        var input = await dataSource.GetInputAsync(now, settings, cancellationToken);
        try
        {
            var recommendations = await provider.GenerateAsync(input, settings, cancellationToken);
            var forecast = new InventoryForecast(
                Guid.NewGuid(), now, input.HistorySince, input.HistoryUntil,
                now.AddHours(settings.RefreshIntervalHours), "Fresh", null, recommendations);
            await store.SaveAsync(forecast, cancellationToken);
            return forecast;
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException or InvalidOperationException)
        {
            var previous = await store.GetLatestAsync(cancellationToken);
            if (previous is not null)
            {
                var stale = previous with { Status = "Stale", Error = exception.Message };
                await store.SaveAsync(stale, cancellationToken);
                logger.LogWarning(exception, "Inventory forecast refresh failed; retaining the previous snapshot.");
                return stale;
            }

            var unavailable = EmptyForecast(now, exception.Message);
            await store.SaveAsync(unavailable, cancellationToken);
            logger.LogWarning(exception, "Inventory forecast is unavailable.");
            return unavailable;
        }
    }

    private static InventoryForecast EmptyForecast(DateTime now, string error) =>
        new(Guid.NewGuid(), now, now, now, now, "Unavailable", error, []);
}
