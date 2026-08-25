using Stripe;

namespace SMA.API.Services.ServiceContracts
{
    public interface IStripeWebhookService
    {
        Task<bool> ProcessAsync(Event stripeEvent, string payload, CancellationToken cancellationToken = default);
    }
}
