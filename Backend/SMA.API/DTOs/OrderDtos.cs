namespace SMA.API.DTOs
{
    public class OrderItemRequestDto
    {
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
    }

    public class CreateOrderRequestDto
    {
        public string ShippingAddress { get; set; } = string.Empty;
        public List<OrderItemRequestDto> Items { get; set; } = [];
    }

    // Response response DTO
    public class OrderResponseDto
    {
        public Guid OrderId { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public string ShippingAddress { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class PagedOrderResponseDto
    {
        public List<OrderResponseDto> Items { get; set; } = [];
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
    }

    public class AdminOrderResponseDto : OrderResponseDto
    {
        public Guid UserId { get; set; }
        public string CustomerEmail { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
    }

    public class PagedAdminOrderResponseDto
    {
        public List<AdminOrderResponseDto> Items { get; set; } = [];
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
    }

    public class CheckoutResponseDto : OrderResponseDto
    {
        public string CheckoutUrl { get; set; } = string.Empty;
    }
}
