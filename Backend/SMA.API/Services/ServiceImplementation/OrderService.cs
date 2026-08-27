using Microsoft.EntityFrameworkCore;
using SMA.API.Data;
using SMA.API.DTOs;
using SMA.API.Entities;
using SMA.API.Services.ServiceContracts;

namespace SMA.API.Services.ServiceImplementation
{
    public class OrderService : IOrderService
    {
        private readonly AppDbContext _context;
        private readonly IPaymentService _paymentService;
        private readonly IProductCache _productCache;

        public OrderService(AppDbContext context, IPaymentService paymentService, IProductCache productCache)
        {
            _context = context;
            _paymentService = paymentService;
            _productCache = productCache;
        }

        public async Task<CheckoutResponseDto> CreateOrderAsync(Guid userId, CreateOrderRequestDto request, string? idempotencyKey, CancellationToken cancellationToken = default)
        {
            if (!string.IsNullOrWhiteSpace(idempotencyKey))
            {
                var existingOrder = await _context.Orders.FirstOrDefaultAsync(order => order.UserId == userId && order.IdempotencyKey == idempotencyKey, cancellationToken);
                if (existingOrder != null)
                {
                    return new CheckoutResponseDto { OrderId = existingOrder.Id, TotalAmount = existingOrder.TotalAmount, Status = existingOrder.Status, ShippingAddress = existingOrder.ShippingAddress, CreatedAt = existingOrder.CreatedAt, CheckoutUrl = existingOrder.StripeCheckoutUrl ?? string.Empty };
                }
            }

            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            decimal totalAmount = 0;
            var orderItems = new List<OrderItem>();

            foreach (var itemRequest in request.Items)
            {
                var product = await _context.Products.FindAsync([itemRequest.ProductId], cancellationToken);
                if (product == null || !product.IsActive)
                    throw new InvalidOperationException($"Product with ID {itemRequest.ProductId} is unavailable.");

                var inventory = await _context.Inventory.FirstOrDefaultAsync(item => item.ProductId == itemRequest.ProductId, cancellationToken);
                if (inventory == null)
                    throw new InvalidOperationException($"Inventory records for product {product.Name} could not be resolved.");
                if (inventory.QuantityAvailable < itemRequest.Quantity)
                    throw new InvalidOperationException($"Insufficient stock for '{product.Name}'. Available: {inventory.QuantityAvailable}. Requested: {itemRequest.Quantity}.");

                inventory.QuantityAvailable -= itemRequest.Quantity;
                inventory.QuantityReserved += itemRequest.Quantity;
                inventory.UpdatedAt = DateTime.UtcNow;
                totalAmount += product.Price * itemRequest.Quantity;
                orderItems.Add(new OrderItem { ProductId = product.Id, Quantity = itemRequest.Quantity, UnitPrice = product.Price, Product = product });
            }

            var order = new Order
            {
                UserId = userId, TotalAmount = totalAmount, ShippingAddress = request.ShippingAddress,
                IdempotencyKey = string.IsNullOrWhiteSpace(idempotencyKey) ? null : idempotencyKey.Trim(),
                OrderItems = orderItems, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow
            };
            _context.Orders.Add(order);
            var checkoutSession = await _paymentService.CreateCheckoutSessionAsync(order, cancellationToken);
            order.StripeCheckoutSessionId = checkoutSession.Id;
            order.StripeCheckoutUrl = checkoutSession.Url;
            await _context.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);
            await _productCache.InvalidateActiveProductsAsync(cancellationToken);

            return new CheckoutResponseDto
            {
                OrderId = order.Id, TotalAmount = order.TotalAmount, Status = order.Status,
                ShippingAddress = order.ShippingAddress, CreatedAt = order.CreatedAt, CheckoutUrl = checkoutSession.Url
            };
        }

        public async Task<PagedOrderResponseDto> GetOrdersAsync(Guid userId, int page, int pageSize, CancellationToken cancellationToken = default)
        {
            var query = _context.Orders.Where(order => order.UserId == userId).OrderByDescending(order => order.CreatedAt);
            var totalCount = await query.CountAsync(cancellationToken);
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).Select(order => MapOrder(order)).ToListAsync(cancellationToken);
            return new PagedOrderResponseDto
            {
                Items = items, Page = page, PageSize = pageSize, TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            };
        }

        public Task<OrderResponseDto?> GetOrderByIdAsync(Guid userId, Guid orderId, CancellationToken cancellationToken = default) =>
            _context.Orders.Where(order => order.Id == orderId && order.UserId == userId)
                .Select(order => MapOrder(order)).FirstOrDefaultAsync(cancellationToken);

        private static OrderResponseDto MapOrder(Order order) => new()
        {
            OrderId = order.Id, TotalAmount = order.TotalAmount, Status = order.Status,
            ShippingAddress = order.ShippingAddress, CreatedAt = order.CreatedAt
        };

        public async Task<PagedAdminOrderResponseDto> GetAllOrdersAsync(int page, int pageSize, CancellationToken cancellationToken = default)
        {
            var query = _context.Orders.OrderByDescending(order => order.CreatedAt);
            var totalCount = await query.CountAsync(cancellationToken);
            var items = await query.Skip((page - 1) * pageSize).Take(pageSize).Select(order => new AdminOrderResponseDto
            {
                OrderId = order.Id,
                UserId = order.UserId,
                CustomerEmail = order.User != null ? order.User.Email : string.Empty,
                CustomerName = order.User != null ? (order.User.FirstName + " " + order.User.LastName).Trim() : "Unknown customer",
                TotalAmount = order.TotalAmount,
                Status = order.Status,
                ShippingAddress = order.ShippingAddress,
                CreatedAt = order.CreatedAt
            }).ToListAsync(cancellationToken);
            return new PagedAdminOrderResponseDto
            {
                Items = items,
                Page = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            };
        }

        public async Task<AdminAnalyticsDto> GetAdminAnalyticsAsync(int days, CancellationToken cancellationToken = default)
        {
            var since = DateTime.UtcNow.Date.AddDays(-(days - 1));
            var paidOrders = _context.Orders.Where(order => order.Status == "Paid" || order.Status == "Placed");
            var recentPaidOrders = paidOrders.Where(order => order.CreatedAt >= since);
            var paidOrderCount = await recentPaidOrders.CountAsync(cancellationToken);
            var grossSales = await recentPaidOrders.Select(order => (decimal?)order.TotalAmount).SumAsync(cancellationToken) ?? 0;
            var statusCounts = await _context.Orders
                .GroupBy(order => order.Status.ToLower())
                .Select(group => new { Status = group.Key, Count = group.Count() })
                .ToDictionaryAsync(item => item.Status, item => item.Count, cancellationToken);
            var dailySales = await recentPaidOrders
                .GroupBy(order => order.CreatedAt.Date)
                .Select(group => new DailySalesDto { Date = group.Key, Amount = group.Sum(order => order.TotalAmount) })
                .OrderBy(item => item.Date)
                .ToListAsync(cancellationToken);
            var activeProducts = _context.Products.Where(product => product.IsActive);
            var inventoryValue = await activeProducts
                .Select(product => (decimal?)(product.Price * (product.Inventory == null ? 0 : product.Inventory.QuantityAvailable)))
                .SumAsync(cancellationToken) ?? 0;
            return new AdminAnalyticsDto
            {
                GrossSales = grossSales,
                PaidOrderCount = paidOrderCount,
                OrderCount = await _context.Orders.CountAsync(cancellationToken),
                AverageOrderValue = paidOrderCount == 0 ? 0 : grossSales / paidOrderCount,
                InventoryValue = inventoryValue,
                ActiveProductCount = await activeProducts.CountAsync(cancellationToken),
                LowStockProductCount = await activeProducts.CountAsync(product => product.Inventory != null && product.Inventory.QuantityAvailable <= 5, cancellationToken),
                OrderStatusCounts = statusCounts,
                DailySales = dailySales
            };
        }
    }
}
