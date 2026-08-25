using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SMA.API.Configuration;
using SMA.API.Services.ServiceContracts;
using Stripe;

namespace SMA.API.Controllers
{
    [ApiController]
    [AllowAnonymous]
    [Route("api/stripe")]
    public class StripeWebhookController : ControllerBase
    {
        private readonly StripeOptions _options;
        private readonly IStripeWebhookService _webhookService;
        private readonly ILogger<StripeWebhookController> _logger;

        public StripeWebhookController(IOptions<StripeOptions> options, IStripeWebhookService webhookService, ILogger<StripeWebhookController> logger)
        {
            _options = options.Value;
            _webhookService = webhookService;
            _logger = logger;
        }

        [HttpPost("webhook")]
        public async Task<IActionResult> Handle(CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(_options.WebhookSecret)) return StatusCode(500, "Stripe webhook is not configured.");
            using var reader = new StreamReader(Request.Body);
            var payload = await reader.ReadToEndAsync(cancellationToken);
            var signature = Request.Headers["Stripe-Signature"].ToString();
            if (string.IsNullOrWhiteSpace(signature)) return BadRequest("Missing Stripe-Signature header.");
            Event stripeEvent;
            try
            {
                stripeEvent = EventUtility.ConstructEvent(payload, signature, _options.WebhookSecret, tolerance: 300, throwOnApiVersionMismatch: false);
            }
            catch (Exception ex) when (ex is StripeException or JsonException)
            {
                _logger.LogWarning(ex, "Invalid Stripe webhook signature or payload.");
                return BadRequest();
            }
            await _webhookService.ProcessAsync(stripeEvent, payload, cancellationToken);
            return Ok();
        }
    }
}
