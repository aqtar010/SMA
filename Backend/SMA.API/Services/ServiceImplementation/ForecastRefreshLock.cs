using StackExchange.Redis;
using SMA.InventoryForecasting;

namespace SMA.API.Services.ServiceImplementation;

public sealed class ForecastRefreshLock(IConnectionMultiplexer redis) : IForecastRefreshLock
{
    private const string LockKey = "sma:inventory-forecast:refresh-lock:v2";

    public async Task<IAsyncDisposable?> TryAcquireAsync(TimeSpan duration, CancellationToken cancellationToken)
    {
        var database = redis.GetDatabase();
        var token = Guid.NewGuid().ToString("N");
        var acquired = await database.LockTakeAsync(LockKey, token, duration);
        return acquired ? new Lease(database, token) : null;
    }

    private sealed class Lease(IDatabase database, string token) : IAsyncDisposable
    {
        public async ValueTask DisposeAsync() => await database.LockReleaseAsync(LockKey, token);
    }
}
