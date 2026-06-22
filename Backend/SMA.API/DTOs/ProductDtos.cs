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
}
