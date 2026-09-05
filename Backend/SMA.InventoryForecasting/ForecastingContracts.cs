namespace SMA.InventoryForecasting;

public sealed record ProductDemandInput(
    Guid ProductId,
    string ProductName,
    int CurrentStock,
    int ReservedStock,
    IReadOnlyList<DailyDemand> DailyDemand);

public sealed record DailyDemand(DateTime Date, int Quantity);

public sealed record ForecastInput(
    DateTime GeneratedAt,
    DateTime HistorySince,
    DateTime HistoryUntil,
    int ForecastHorizonDays,
    IReadOnlyList<ProductDemandInput> Products);

public sealed record ForecastRecommendation(
    Guid ProductId,
    int PredictedDemand,
    int ReorderPoint,
    bool ReorderRecommended,
    double TrendPercent,
    string Insight);

public sealed record InventoryForecast(
    Guid Id,
    DateTime GeneratedAt,
    DateTime HistorySince,
    DateTime HistoryUntil,
    DateTime NextRefreshAt,
    string Status,
    string? Error,
    IReadOnlyList<ForecastRecommendation> Recommendations);

public interface IForecastDataSource
{
    Task<ForecastInput> GetInputAsync(DateTime now, ForecastOptions options, CancellationToken cancellationToken);
}

public interface IForecastStore
{
    Task<InventoryForecast?> GetLatestAsync(CancellationToken cancellationToken);
    Task SaveAsync(InventoryForecast forecast, CancellationToken cancellationToken);
}

public interface IForecastRefreshLock
{
    Task<IAsyncDisposable?> TryAcquireAsync(TimeSpan duration, CancellationToken cancellationToken);
}

public interface IInventoryForecastService
{
    Task<InventoryForecast?> GetLatestAsync(CancellationToken cancellationToken);
    Task<InventoryForecast> RefreshAsync(CancellationToken cancellationToken);
}
