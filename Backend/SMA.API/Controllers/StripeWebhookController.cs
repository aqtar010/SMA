using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SMA.API.Configuration;
using SMA.API.Data;
using SMA.API.Entities;
using Stripe;
using Stripe.Checkout;

namespace SMA.API.Controllers
{
    [ApiController]
    [AllowAnonymous]
    [Route("api/stripe")]
    public class StripeWebhookController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly StripeOptions _options;
        private readonly ILogger<StripeWebhookController> _logger;

        public StripeWebhookController(AppDbContext context, IOptions<StripeOptions> options, ILogger<StripeWebhookController> logger)
        {
            _context = context;
            _options = options.Value;
            _logger = logger;
        }

        [HttpPost("webhook")]
        public async Task<IActionResult> Handle(CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(_options.WebhookSecret))
                return StatusCode(500, "Stripe webhook is not configured.");

            using var reader = new StreamReader(Request.Body);
            var payload = await reader.ReadToEndAsync(cancellationToken);
            var signature = Request.Headers["Stripe-Signature"].ToString();
            if (string.IsNullOrWhiteSpace(signature))
            {
                _logger.LogWarning("Missing Stripe-Signature header.");
                return BadRequest("Missing Stripe-Signature header.");
            }

            Event stripeEvent;
            try
            {
                stripeEvent = EventUtility.ConstructEvent(
                    payload, 
                    signature, 
                    _options.WebhookSecret,
                    tolerance: 300,
                    throwOnApiVersionMismatch: false
                );
            }
            catch (Exception ex) when (ex is StripeException or JsonException)
            {
                _logger.LogWarning(ex, "Invalid Stripe webhook signature or payload.");
                return BadRequest();
            }

            if (await _context.StripeWebhookEvents.AnyAsync(item => item.Id == stripeEvent.Id, cancellationToken))
                return Ok();

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
                    await HandlePaymentActionRequiredAsync(stripeEvent, payload, cancellationToken);
                    break;
                default:
                    _logger.LogInformation("Ignoring unsupported Stripe event {EventId} of type {EventType}.", stripeEvent.Id, stripeEvent.Type);
                    break;
            }

            _context.StripeWebhookEvents.Add(new StripeWebhookEvent { Id = stripeEvent.Id, Type = stripeEvent.Type });
            await _context.SaveChangesAsync(cancellationToken);
            return Ok();
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
                var inventory = await _context.Inventory.SingleAsync(i => i.ProductId == item.ProductId, cancellationToken);
                inventory.QuantityReserved = Math.Max(0, inventory.QuantityReserved - item.Quantity);
                inventory.UpdatedAt = DateTime.UtcNow;
            }

            order.Status = "Paid";
            order.StripePaymentIntentId = session.PaymentIntentId;
            order.UpdatedAt = DateTime.UtcNow;
            _context.Transactions.Add(new Transaction
            {
                OrderId = order.Id,
                GatewayTransactionId = session.PaymentIntentId ?? session.Id,
                PaymentGateway = "Stripe",
                Amount = order.TotalAmount,
                Currency = "INR",
                Status = "Succeeded",
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
                var inventory = await _context.Inventory.SingleAsync(i => i.ProductId == item.ProductId, cancellationToken);
                inventory.QuantityAvailable += item.Quantity;
                inventory.QuantityReserved = Math.Max(0, inventory.QuantityReserved - item.Quantity);
                inventory.UpdatedAt = DateTime.UtcNow;
            }

            order.Status = stripeEvent.Type == EventTypes.CheckoutSessionExpired ? "Expired" : "Failed";
            order.UpdatedAt = DateTime.UtcNow;
            _context.Transactions.Add(new Transaction
            {
                OrderId = order.Id,
                GatewayTransactionId = session.Id,
                PaymentGateway = "Stripe",
                Amount = order.TotalAmount,
                Currency = "INR",
                Status = "Failed",
                RawGatewayResponse = JsonDocument.Parse(payload)
            });
        }

        private async Task HandlePaymentActionRequiredAsync(Event stripeEvent, string payload, CancellationToken cancellationToken)
        {
            // Extract PaymentIntent from the event data
            var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
            if (paymentIntent == null) return;

            _logger.LogWarning(
                "Payment intent {PaymentIntentId} requires action. " +
                "Customer must complete authentication at: {ClientSecret}",
                paymentIntent.Id,
                paymentIntent.ClientSecret
            );

            // Log the event for debugging/auditing
            _logger.LogInformation(
                "Payment action required event: {EventId} for PaymentIntent: {PaymentIntentId}",
                stripeEvent.Id,
                paymentIntent.Id
            );

            // NOTE: In a production system, you would typically:
            // 1. Send customer an email with authentication link
            // 2. Update order status to "Awaiting Payment Confirmation"
            // 3. Wait for payment_intent.succeeded or payment_intent.payment_failed event
            await Task.CompletedTask;
        }

        private Task<Order?> FindOrderAsync(Session session, CancellationToken cancellationToken)
        {
            var orderId = session.Metadata.TryGetValue("order_id", out var metadataId)
                ? metadataId
                : session.ClientReferenceId;

            return Guid.TryParse(orderId, out var id)
                ? _context.Orders.Include(order => order.OrderItems).SingleOrDefaultAsync(order => order.Id == id, cancellationToken)
                : Task.FromResult<Order?>(null);
        }
    }
}