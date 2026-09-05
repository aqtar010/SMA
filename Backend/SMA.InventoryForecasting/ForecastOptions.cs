using System.ComponentModel.DataAnnotations;

namespace SMA.InventoryForecasting;

public sealed class ForecastOptions
{
    public const string SectionName = "InventoryForecast";

    [Required]
    public string OllamaBaseUrl { get; set; } = "http://host.docker.internal:11434";

    [Required]
    public string Model { get; set; } = "qwen3.5:4b";

    [Range(7, 365)]
    public int HistoryDays { get; set; } = 90;

    [Range(1, 30)]
    public int ForecastHorizonDays { get; set; } = 7;

    [Range(1, 168)]
    public int RefreshIntervalHours { get; set; } = 6;

    [Range(5, 300)]
    public int RequestTimeoutSeconds { get; set; } = 60;

    [Range(1, 168)]
    public int CacheHours { get; set; } = 12;
}
