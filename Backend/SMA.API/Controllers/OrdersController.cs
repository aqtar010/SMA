using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMA.API.DTOs;
using SMA.API.Services.ServiceContracts;

namespace SMA.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService) => _orderService = orderService;

        [HttpPost("checkout")]
        public async Task<ActionResult<CheckoutResponseDto>> CreateOrder(CreateOrderRequestDto request, CancellationToken cancellationToken)
        {
            if (request == null || request.Items.Count == 0 || request.Items.Any(item => item.Quantity <= 0))
                return BadRequest("Order must contain at least one item.");
            var userId = GetUserIdFromClaims();
            if (userId == Guid.Empty) return Unauthorized("Unable to resolve user identity from authorization token.");

            try
            {
                var idempotencyKey = Request.Headers["Idempotency-Key"].ToString();
                if (idempotencyKey.Length > 100) return BadRequest("Idempotency-Key must be 100 characters or fewer.");
                var result = await _orderService.CreateOrderAsync(userId, request, idempotencyKey, cancellationToken);
                return CreatedAtAction(nameof(GetOrderById), new { id = result.OrderId }, result);
            }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [HttpGet]
        public async Task<ActionResult<PagedOrderResponseDto>> GetOrders([FromQuery] int page = 1, [FromQuery] int pageSize = 10, CancellationToken cancellationToken = default)
        {
            var userId = GetUserIdFromClaims();
            if (userId == Guid.Empty) return Unauthorized("Unable to resolve user identity from authorization token.");
            return Ok(await _orderService.GetOrdersAsync(userId, Math.Max(page, 1), Math.Clamp(pageSize, 1, 50), cancellationToken));
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<OrderResponseDto>> GetOrderById(Guid id, CancellationToken cancellationToken)
        {
            var order = await _orderService.GetOrderByIdAsync(GetUserIdFromClaims(), id, cancellationToken);
            return order == null ? NotFound() : Ok(order);
        }

        private Guid GetUserIdFromClaims()
        {
            var value = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst(ClaimTypes.Name)?.Value;
            return Guid.TryParse(value, out var userId) ? userId : Guid.Empty;
        }
    }
}
