namespace SMA.API.Entities;

public sealed class InventoryForecastSnapshot
{
    public Guid Id { get; set; }
    public DateTime GeneratedAt { get; set; }
    public DateTime HistorySince { get; set; }
    public DateTime HistoryUntil { get; set; }
    public DateTime NextRefreshAt { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Error { get; set; }
    public string RecommendationsJson { get; set; } = "[]";
}
