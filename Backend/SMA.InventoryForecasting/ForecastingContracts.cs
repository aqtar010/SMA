using System.Text.Json.Serialization;

namespace SMA.InventoryForecasting;

public sealed record ProductDemandInput(
    Guid ProductId,
    string ProductName,
    DateTime? ExpiryDate,
    int DaysUntilExpiry,
    int CurrentStock,
    int ReservedStock,
    int AvgDailySales7,
    int AvgDailySales30,
    double TrendPercent,
    int ReorderPoint,
    double DaysOfCover,
    double UrgencyScore,
    IReadOnlyList<DailyDemand> DailyDemand);

public sealed record DailyDemand(DateTime Date, int Quantity);

public sealed record ForecastInput(
    DateTime GeneratedAt,
    DateTime HistorySince,
    DateTime HistoryUntil,
    int ForecastHorizonDays,
    int MaxProducts,
    IReadOnlyList<ProductDemandInput> Products);

public sealed record ForecastRecommendation(
    [property: JsonPropertyName("productId")] Guid ProductId,
    [property: JsonPropertyName("predictedDemand")] int PredictedDemand,
    [property: JsonPropertyName("reorderPoint")] int ReorderPoint,
    [property: JsonPropertyName("reorderRecommended")] bool ReorderRecommended,
    [property: JsonPropertyName("trendPercent")] double TrendPercent,
    [property: JsonPropertyName("insight")] string Insight);

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
