using SMA.API.DTOs;

namespace SMA.API.Services.ServiceContracts
{
    public interface IOrderService
    {
        Task<CheckoutResponseDto> CreateOrderAsync(Guid userId, CreateOrderRequestDto request, string? idempotencyKey, CancellationToken cancellationToken = default);
        Task<PagedOrderResponseDto> GetOrdersAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken = default);
        Task<OrderResponseDto?> GetOrderByIdAsync(Guid userId, Guid orderId, CancellationToken cancellationToken = default);
        Task<PagedAdminOrderResponseDto> GetAllOrdersAsync(int page, int pageSize, CancellationToken cancellationToken = default);
        Task<AdminAnalyticsDto> GetAdminAnalyticsAsync(int days, CancellationToken cancellationToken = default);

    }
}
