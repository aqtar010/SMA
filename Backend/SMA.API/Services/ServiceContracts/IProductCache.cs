using SMA.API.DTOs;

namespace SMA.API.Services.ServiceContracts
{
    public interface IProductCache
    {
        Task<IReadOnlyList<ProductResponseDto>?> GetActiveProductsAsync(CancellationToken cancellationToken = default);
        Task SetActiveProductsAsync(IReadOnlyList<ProductResponseDto> products, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<AdminProductResponseDto>?> GetAdminProductsAsync(CancellationToken cancellationToken = default);
        Task SetAdminProductsAsync(IReadOnlyList<AdminProductResponseDto> products, CancellationToken cancellationToken = default);
        Task InvalidateActiveProductsAsync(CancellationToken cancellationToken = default);
    }
}