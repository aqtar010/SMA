using Microsoft.Extensions.Options;
using SMA.API.Configuration;
using SMA.API.Entities;
using SMA.API.Services.ServiceContracts;
using Stripe;
using Stripe.Checkout;

namespace SMA.API.Services.ServiceImplementation
{
    public class StripePaymentService : IPaymentService
    {
        private readonly StripeOptions _options;
        private readonly SessionService _sessions;

        public StripePaymentService(IOptions<StripeOptions> options)
        {
            _options = options.Value;
            _sessions = new SessionService();
        }

        public async Task<CheckoutSessionResult> CreateCheckoutSessionAsync(Order order, CancellationToken cancellationToken = default)
        {
            var sessionOptions = new SessionCreateOptions
            {
                Mode = "payment",
                SuccessUrl = string.Format(_options.SuccessUrl, order.Id),
                CancelUrl = string.Format(_options.CancelUrl, order.Id),
                ClientReferenceId = order.Id.ToString(),
                Metadata = new Dictionary<string, string>
                {
                    ["order_id"] = order.Id.ToString(),
                    ["user_id"] = order.UserId.ToString()
                },
                LineItems = order.OrderItems.Select(item => new SessionLineItemOptions
                {
                    Quantity = item.Quantity,
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        Currency = "usd",
                        UnitAmount = (long)(item.UnitPrice * 100m),
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = item.Product?.Name ?? "SMA product"
                        }
                    }
                }).ToList()
            };

            var requestOptions = new RequestOptions
            {
                IdempotencyKey = $"checkout-order-{order.Id}"
            };

            var session = await _sessions.CreateAsync(sessionOptions, requestOptions, cancellationToken);
            return new CheckoutSessionResult(session.Id, session.Url);
        }
    }
}