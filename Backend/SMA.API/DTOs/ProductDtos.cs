using System.ComponentModel.DataAnnotations;

namespace SMA.API.DTOs
{
    public class CreateProductDto
    {
        [Required]
        public string Sku { get; set; } = string.Empty;

        [Required]
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        [Range(0.01, 100000)]
        public decimal Price { get; set; }

        [Range(0, 10000)]
        public int InitialStock { get; set; }
    }
    public class ProductResponseDto
    {
        public Guid Id { get; set; }
        public string Sku { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int QuantityAvailable { get; set; } // Flattened from the Inventory table
        public double AverageRating { get; set; }
        public int RatingCount { get; set; }
    }

    public class ProductRatingSummaryDto
    {
        public double AverageRating { get; set; }
        public int RatingCount { get; set; }
        public int? CurrentUserRating { get; set; }
        public bool CanRate { get; set; }
    }

    public class CreateProductRatingDto
    {
        [Range(1, 5)]
        public int Rating { get; set; }

        [MaxLength(2000)]
        public string? Feedback { get; set; }
    }

    public class ProductRatingResponseDto
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string? Feedback { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class PagedProductRatingResponseDto
    {
        public List<ProductRatingResponseDto> Items { get; set; } = [];
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages { get; set; }
    }

    public class AdminProductResponseDto
    {
        public Guid Id { get; set; }
        public string Sku { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public bool IsActive { get; set; }
        public int QuantityAvailable { get; set; }
        public int QuantityReserved { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class UpdateProductDto
    {
        [MaxLength(50)]
        public string? Sku { get; set; }

        [MaxLength(255)]
        public string? Name { get; set; }

        public string? Description { get; set; }

        [Range(0.01, 100000)]
        public decimal? Price { get; set; }

        public bool? IsActive { get; set; }
    }

    public class UpdateStockDto
    {
        [Range(0, 100000)]
        public int QuantityAvailable { get; set; }
    }
}
