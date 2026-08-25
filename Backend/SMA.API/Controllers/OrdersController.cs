using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMA.API.Data;
using SMA.API.DTOs;
using SMA.API.Entities;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using SMA.API.Services.ServiceContracts;

namespace SMA.API.Controllers
{
    [Authorize] // Locks down all endpoints in this controller to authenticated users
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IPaymentService _paymentService;

        public OrdersController(AppDbContext context, IPaymentService paymentService)
        {
            _context = context;
            _paymentService = paymentService;
        }
        /// <summary>
        /// POST: api/orders/checkout
        /// Secure transactional endpoint for placing an order and reserving inventory.
        /// </summary>
        [HttpPost("checkout")]
        public async Task<ActionResult<CheckoutResponseDto>> CreateOrder([FromBody] CreateOrderRequestDto request)
        {
            if (request == null || !request.Items.Any() || request.Items.Any(item => item.Quantity <= 0))
            {
                return BadRequest("Order must contain at least one item.");
            }

            // 1. Securely extract the User ID from the active JWT claims
            var userId = GetUserIdFromClaims();
            if (userId == Guid.Empty)
            {
                return Unauthorized("Unable to resolve user identity from authorization token.");
            }

            // 2. Begin an explicit database transaction
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                decimal totalOrderAmount = 0;
                var orderItemsToCreate = new List<OrderItem>();

                // 3. Process each item in the order
                foreach (var itemDto in request.Items)
                {
                    // Fetch product catalog details
                    var product = await _context.Products.FindAsync(itemDto.ProductId);
                    if (product == null || !product.IsActive)
                    {
                        return BadRequest($"Product with ID {itemDto.ProductId} is unavailable.");
                    }

                    // Fetch inventory stock - Pessimistic lock should be applied here in production
                    var inventory = await _context.Inventory
                        .FirstOrDefaultAsync(i => i.ProductId == itemDto.ProductId);

                    if (inventory == null)
                    {
                        return BadRequest($"Inventory records for product {product.Name} could not be resolved.");
                    }

                    // 4. Validate and update Stock Levels
                    if (inventory.QuantityAvailable < itemDto.Quantity)
                    {
                        await transaction.RollbackAsync(); // Immediately cancel everything
                        return BadRequest($"Insufficient stock for '{product.Name}'. Available: {inventory.QuantityAvailable}. Requested: {itemDto.Quantity}.");
                    }

                    // Deduct stock levels and reserve them
                    inventory.QuantityAvailable -= itemDto.Quantity;
                    inventory.QuantityReserved += itemDto.Quantity;
                    inventory.UpdatedAt = DateTime.UtcNow;

                    // Calculate totals
                    decimal itemTotal = product.Price * itemDto.Quantity;
                    totalOrderAmount += itemTotal;

                    // Build line item configuration
                    orderItemsToCreate.Add(new OrderItem
                    {
                        ProductId = product.Id,
                        Quantity = itemDto.Quantity,
                        UnitPrice = product.Price,
                        Product = product
                    });
                }

                // 5. Create Order record
                var order = new Order
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    TotalAmount = totalOrderAmount,
                    Status = "Payment_Pending",
                    ShippingAddress = request.ShippingAddress,
                    OrderItems = orderItemsToCreate,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _context.Orders.Add(order);

                var checkoutSession = await _paymentService.CreateCheckoutSessionAsync(order);
                order.StripeCheckoutSessionId = checkoutSession.Id;

                // 6. Save changes to PostgreSQL
                await _context.SaveChangesAsync();

                // 7. Commit transaction to make the changes permanent
                await transaction.CommitAsync();

                var response = new CheckoutResponseDto
                {
                    OrderId = order.Id,
                    TotalAmount = order.TotalAmount,
                    Status = order.Status,
                    ShippingAddress = order.ShippingAddress,
                    CreatedAt = order.CreatedAt,
                    CheckoutUrl = checkoutSession.Url
                };

                return CreatedAtAction(nameof(GetOrderById), new { id = order.Id }, response);

            }
            catch (Exception ex)
            {
                // Safety net: Rollback the transaction to prevent corrupt database states
                await transaction.RollbackAsync();
                return StatusCode(500, $"An error occurred processing your checkout: {ex.Message}");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<OrderResponseDto>> GetOrderById(Guid id)
        {
            var userId = GetUserIdFromClaims();
            var order = await _context.Orders
                .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId);

            if (order == null)
            {
                return NotFound();
            }

            return Ok(new OrderResponseDto
            {
                OrderId = order.Id,
                TotalAmount = order.TotalAmount,
                Status = order.Status,
                ShippingAddress = order.ShippingAddress,
                CreatedAt = order.CreatedAt
            });
        }

        private Guid GetUserIdFromClaims()
        {
            var userIdString = User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                            ?? User.FindFirst(ClaimTypes.Name)?.Value;

            if (Guid.TryParse(userIdString, out Guid parsedGuid))
            {
                return parsedGuid;
            }

            return Guid.Empty;
        }
    }
}
