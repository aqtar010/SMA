using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SMA.API.Data;
using SMA.API.Entities;
using SMA.API.Services.ServiceContracts;
using Stripe;
using Stripe.Checkout;

namespace SMA.API.Services.ServiceImplementation
{
    public class StripeWebhookService : IStripeWebhookService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<StripeWebhookService> _logger;

        public StripeWebhookService(AppDbContext context, ILogger<StripeWebhookService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<bool> ProcessAsync(Event stripeEvent, string payload, CancellationToken cancellationToken = default)
        {
            if (await _context.StripeWebhookEvents.AnyAsync(item => item.Id == stripeEvent.Id, cancellationToken)) return false;
            switch (stripeEvent.Type)
            {
                case EventTypes.CheckoutSessionCompleted:
                case EventTypes.CheckoutSessionAsyncPaymentSucceeded:
                    await CompleteOrderAsync(stripeEvent, payload, cancellationToken);
                    break;
                case EventTypes.CheckoutSessionAsyncPaymentFailed:
                case EventTypes.CheckoutSessionExpired:
                    await ReleaseOrderAsync(stripeEvent, payload, cancellationToken);
                    break;
                case EventTypes.PaymentIntentRequiresAction:
                    LogPaymentActionRequired(stripeEvent);
                    break;
                default:
                    _logger.LogInformation("Ignoring unsupported Stripe event {EventId} of type {EventType}.", stripeEvent.Id, stripeEvent.Type);
                    break;
            }
            _context.StripeWebhookEvents.Add(new StripeWebhookEvent { Id = stripeEvent.Id, Type = stripeEvent.Type });
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }

        private async Task CompleteOrderAsync(Event stripeEvent, string payload, CancellationToken cancellationToken)
        {
            var session = stripeEvent.Data.Object as Session;
            if (session == null || session.PaymentStatus != "paid") return;
            var order = await FindOrderAsync(session, cancellationToken);
            if (order == null || order.Status is "Paid" or "Placed") return;
            if (session.AmountTotal != (long)(order.TotalAmount * 100m) || session.Currency != "usd")
                throw new InvalidOperationException($"Stripe amount or currency mismatch for order {order.Id}.");
            foreach (var item in order.OrderItems)
            {
                var inventory = await _context.Inventory.SingleAsync(stock => stock.ProductId == item.ProductId, cancellationToken);
                inventory.QuantityReserved = Math.Max(0, inventory.QuantityReserved - item.Quantity);
                inventory.UpdatedAt = DateTime.UtcNow;
            }
            order.Status = "Paid";
            order.StripePaymentIntentId = session.PaymentIntentId;
            order.UpdatedAt = DateTime.UtcNow;
            _context.Transactions.Add(new Transaction
            {
                OrderId = order.Id, GatewayTransactionId = session.PaymentIntentId ?? session.Id,
                PaymentGateway = "Stripe", Amount = order.TotalAmount, Currency = "INR", Status = "Succeeded",
                RawGatewayResponse = JsonDocument.Parse(payload)
            });
        }

        private async Task ReleaseOrderAsync(Event stripeEvent, string payload, CancellationToken cancellationToken)
        {
            var session = stripeEvent.Data.Object as Session;
            if (session == null) return;
            var order = await FindOrderAsync(session, cancellationToken);
            if (order == null || order.Status is "Paid" or "Placed" or "Failed" or "Expired") return;
            foreach (var item in order.OrderItems)
            {
                var inventory = await _context.Inventory.SingleAsync(stock => stock.ProductId == item.ProductId, cancellationToken);
                inventory.QuantityAvailable += item.Quantity;
                inventory.QuantityReserved = Math.Max(0, inventory.QuantityReserved - item.Quantity);
                inventory.UpdatedAt = DateTime.UtcNow;
            }
            order.Status = stripeEvent.Type == EventTypes.CheckoutSessionExpired ? "Expired" : "Failed";
            order.UpdatedAt = DateTime.UtcNow;
            _context.Transactions.Add(new Transaction
            {
                OrderId = order.Id, GatewayTransactionId = session.Id, PaymentGateway = "Stripe",
                Amount = order.TotalAmount, Currency = "INR", Status = "Failed", RawGatewayResponse = JsonDocument.Parse(payload)
            });
        }

        private void LogPaymentActionRequired(Event stripeEvent)
        {
            if (stripeEvent.Data.Object is not PaymentIntent paymentIntent) return;
            _logger.LogWarning("Payment intent {PaymentIntentId} requires action. Customer must complete authentication at: {ClientSecret}", paymentIntent.Id, paymentIntent.ClientSecret);
            _logger.LogInformation("Payment action required event: {EventId} for PaymentIntent: {PaymentIntentId}", stripeEvent.Id, paymentIntent.Id);
        }

        private Task<Order?> FindOrderAsync(Session session, CancellationToken cancellationToken)
        {
            var orderId = session.Metadata.TryGetValue("order_id", out var metadataId) ? metadataId : session.ClientReferenceId;
            return Guid.TryParse(orderId, out var id)
                ? _context.Orders.Include(order => order.OrderItems).SingleOrDefaultAsync(order => order.Id == id, cancellationToken)
                : Task.FromResult<Order?>(null);
        }
    }
}
