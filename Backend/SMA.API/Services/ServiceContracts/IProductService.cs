using SMA.API.DTOs;

namespace SMA.API.Services.ServiceContracts
{
    public interface IProductService
    {
        Task<IReadOnlyList<ProductResponseDto>> GetProductsAsync(CancellationToken cancellationToken = default);
        Task<ProductResponseDto?> CreateProductAsync(CreateProductDto request, CancellationToken cancellationToken = default);
        Task<IReadOnlyList<AdminProductResponseDto>> GetAdminProductsAsync(CancellationToken cancellationToken = default);
        Task<AdminProductResponseDto?> UpdateProductAsync(Guid id, UpdateProductDto request, CancellationToken cancellationToken = default);
        Task<AdminProductResponseDto?> UpdateStockAsync(Guid id, UpdateStockDto request, CancellationToken cancellationToken = default);
        Task<ProductRatingSummaryDto?> GetRatingSummaryAsync(Guid productId, Guid userId, CancellationToken cancellationToken = default);
        Task<ProductRatingSummaryDto> SaveRatingAsync(Guid productId, Guid userId, CreateProductRatingDto request, CancellationToken cancellationToken = default);
        Task<PagedProductRatingResponseDto?> GetAdminRatingsAsync(Guid productId, int page, int pageSize, CancellationToken cancellationToken = default);
    }
}
