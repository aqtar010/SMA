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
