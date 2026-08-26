using System.Text.Json;
using Microsoft.Extensions.Caching.Distributed;
using SMA.API.DTOs;
using SMA.API.Services.ServiceContracts;

namespace SMA.API.Services.ServiceImplementation
{
    public class ProductCache : IProductCache
    {
        private const string ActiveProductsKey = "sma:products:active:v1";
        private const string AdminProductsKey = "sma:products:admin:v1";
        private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);
        private readonly IDistributedCache _cache;
        private readonly ILogger<ProductCache> _logger;

        public ProductCache(IDistributedCache cache, ILogger<ProductCache> logger)
        {
            _cache = cache;
            _logger = logger;
        }

        public async Task<IReadOnlyList<ProductResponseDto>?> GetActiveProductsAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var cachedProducts = await _cache.GetStringAsync(ActiveProductsKey, cancellationToken);
                return cachedProducts == null
                    ? null
                    : JsonSerializer.Deserialize<List<ProductResponseDto>>(cachedProducts, SerializerOptions);
            }
            catch (Exception exception)
            {
                _logger.LogWarning(exception, "Unable to read active products from Redis.");
                return null;
            }
        }

        public async Task SetActiveProductsAsync(IReadOnlyList<ProductResponseDto> products, CancellationToken cancellationToken = default)
        {
            try
            {
                var cacheOptions = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2)
                };
                var serializedProducts = JsonSerializer.Serialize(products, SerializerOptions);
                await _cache.SetStringAsync(ActiveProductsKey, serializedProducts, cacheOptions, cancellationToken);
            }
            catch (Exception exception)
            {
                _logger.LogWarning(exception, "Unable to write active products to Redis.");
            }
        }

        public async Task<IReadOnlyList<AdminProductResponseDto>?> GetAdminProductsAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var cachedProducts = await _cache.GetStringAsync(AdminProductsKey, cancellationToken);
                return cachedProducts == null
                    ? null
                    : JsonSerializer.Deserialize<List<AdminProductResponseDto>>(cachedProducts, SerializerOptions);
            }
            catch (Exception exception)
            {
                _logger.LogWarning(exception, "Unable to read admin products from Redis.");
                return null;
            }
        }

        public async Task SetAdminProductsAsync(IReadOnlyList<AdminProductResponseDto> products, CancellationToken cancellationToken = default)
        {
            try
            {
                var cacheOptions = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(2)
                };
                var serializedProducts = JsonSerializer.Serialize(products, SerializerOptions);
                await _cache.SetStringAsync(AdminProductsKey, serializedProducts, cacheOptions, cancellationToken);
            }
            catch (Exception exception)
            {
                _logger.LogWarning(exception, "Unable to write admin products to Redis.");
            }
        }

        public async Task InvalidateActiveProductsAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                await _cache.RemoveAsync(ActiveProductsKey, cancellationToken);
                await _cache.RemoveAsync(AdminProductsKey, cancellationToken);
            }
            catch (Exception exception)
            {
                _logger.LogWarning(exception, "Unable to invalidate active products in Redis.");
            }
        }
    }
}