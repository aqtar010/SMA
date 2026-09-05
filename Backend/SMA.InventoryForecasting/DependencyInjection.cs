using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace SMA.InventoryForecasting;

public static class DependencyInjection
{
    public static IServiceCollection AddInventoryForecasting(this IServiceCollection services, Action<ForecastOptions>? configure = null)
    {
        if (configure is not null)
        {
            services.Configure(configure);
        }

        services.AddOptions<ForecastOptions>()
            .BindConfiguration(ForecastOptions.SectionName)
            .ValidateDataAnnotations()
            .ValidateOnStart();
        services.AddHttpClient<OllamaForecastProvider>((serviceProvider, client) =>
        {
            var settings = serviceProvider.GetRequiredService<IOptions<ForecastOptions>>().Value;
            client.BaseAddress = new Uri(settings.OllamaBaseUrl.TrimEnd('/') + "/");
            client.Timeout = TimeSpan.FromSeconds(settings.RequestTimeoutSeconds);
        });
        services.AddScoped<IInventoryForecastService, InventoryForecastService>();
        services.AddHostedService<ForecastRefreshWorker>();
        return services;
    }
}
