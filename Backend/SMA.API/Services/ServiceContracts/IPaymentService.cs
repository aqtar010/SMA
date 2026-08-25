using SMA.API.Entities;

namespace SMA.API.Services.ServiceContracts
{
    public sealed record CheckoutSessionResult(string Id, string Url);

    public interface IPaymentService
    {
        Task<CheckoutSessionResult> CreateCheckoutSessionAsync(Order order, CancellationToken cancellationToken = default);
    }
}