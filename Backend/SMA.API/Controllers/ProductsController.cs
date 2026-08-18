using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SMA.API.Data;
using SMA.API.DTOs;
using SMA.API.Entities;

namespace SMA.API.Controllers
{
    [ApiController]
    [Authorize(Roles = "Admin,Customer")]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ILogger<ProductsController> _logger;

        // Dependency Injection: The framework automatically provides the database context
        public ProductsController(AppDbContext context, ILogger<ProductsController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // GET: api/products
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductResponseDto>>> GetProducts()
        {
            // .Include() is crucial here. It tells EF Core to perform a SQL JOIN
            // to fetch the related Inventory data alongside the Product.
            var products = await _context.Products
                .Include(p => p.Inventory)
                .Where(p => p.IsActive)
                .Select(p => new ProductResponseDto
                {
                    Id = p.Id,
                    Sku = p.Sku,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    QuantityAvailable = p.Inventory != null ? p.Inventory.QuantityAvailable : 0
                })
                .ToListAsync();

            return Ok(products);
        }

        // POST: api/products
        [Authorize(Roles = "Admin")] // Only Admins can create products
        [HttpPost]
        public async Task<ActionResult<ProductResponseDto>> CreateProduct(CreateProductDto dto)
        {
            _logger.LogInformation("Using connection: {cs}", _context.Database.GetDbConnection().ConnectionString);

            // 1. Check if SKU already exists to prevent database errors
            if (await _context.Products.AnyAsync(p => p.Sku == dto.Sku))
            {
                return BadRequest("A product with this SKU already exists.");
            }

            // 2. Create the Database Entities
            var newProduct = new Product
            {
                Sku = dto.Sku,
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price,
                // We create the linked Inventory record at the exact same time
                Inventory = new Inventory
                {
                    QuantityAvailable = dto.InitialStock,
                    QuantityReserved = 0
                }
            };

            // 3. Save to PostgreSQL
            _context.Products.Add(newProduct);
            var result = await _context.SaveChangesAsync();
            _logger.LogInformation("SaveChanges returned {count}; new product id={id}", result, newProduct.Id);

            // 4. Return the newly created resource
            var responseDto = new ProductResponseDto
            {
                Id = newProduct.Id,
                Sku = newProduct.Sku,
                Name = newProduct.Name,
                Description = newProduct.Description,
                Price = newProduct.Price,
                QuantityAvailable = newProduct.Inventory.QuantityAvailable
            };

            return CreatedAtAction(nameof(GetProducts), new { id = responseDto.Id }, responseDto);
        }

        // GET: api/products/admin
        [Authorize(Roles = "Admin,SuperAdmin")]
        [HttpGet("admin")]
        public async Task<ActionResult<IEnumerable<AdminProductResponseDto>>> GetAdminProducts()
        {
            var products = await _context.Products
                .Include(p => p.Inventory)
                .OrderByDescending(p => p.UpdatedAt)
                .Select(p => new AdminProductResponseDto
                {
                    Id = p.Id,
                    Sku = p.Sku,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    IsActive = p.IsActive,
                    QuantityAvailable = p.Inventory != null ? p.Inventory.QuantityAvailable : 0,
                    QuantityReserved = p.Inventory != null ? p.Inventory.QuantityReserved : 0,
                    CreatedAt = p.CreatedAt,
                    UpdatedAt = p.UpdatedAt
                })
                .ToListAsync();

            return Ok(products);
        }

        // PUT: api/products/{id}
        [Authorize(Roles = "Admin,SuperAdmin")]
        [HttpPut("{id:guid}")]
        public async Task<ActionResult<AdminProductResponseDto>> UpdateProduct(Guid id, UpdateProductDto dto)
        {
            var product = await _context.Products
                .Include(p => p.Inventory)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return NotFound("Product not found.");
            }

            if (!string.IsNullOrWhiteSpace(dto.Sku) && dto.Sku != product.Sku)
            {
                if (await _context.Products.AnyAsync(p => p.Sku == dto.Sku && p.Id != id))
                {
                    return BadRequest("A product with this SKU already exists.");
                }

                product.Sku = dto.Sku;
            }

            if (!string.IsNullOrWhiteSpace(dto.Name))
            {
                product.Name = dto.Name;
            }

            if (dto.Description != null)
            {
                product.Description = dto.Description;
            }

            if (dto.Price.HasValue)
            {
                product.Price = dto.Price.Value;
            }

            if (dto.IsActive.HasValue)
            {
                product.IsActive = dto.IsActive.Value;
            }

            product.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();

            return Ok(MapToAdminDto(product));
        }

        // PATCH: api/products/{id}/stock
        [Authorize(Roles = "Admin,SuperAdmin")]
        [HttpPatch("{id:guid}/stock")]
        public async Task<ActionResult<AdminProductResponseDto>> UpdateStock(Guid id, UpdateStockDto dto)
        {
            var product = await _context.Products
                .Include(p => p.Inventory)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return NotFound("Product not found.");
            }

            if (product.Inventory == null)
            {
                product.Inventory = new Inventory
                {
                    ProductId = product.Id,
                    QuantityReserved = 0
                };
                _context.Inventory.Add(product.Inventory);
            }

            if (dto.QuantityAvailable < product.Inventory.QuantityReserved)
            {
                return BadRequest(
                    $"Stock cannot be lower than reserved quantity ({product.Inventory.QuantityReserved}).");
            }

            product.Inventory.QuantityAvailable = dto.QuantityAvailable;
            product.Inventory.UpdatedAt = DateTime.UtcNow;
            product.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return Ok(MapToAdminDto(product));
        }

        private static AdminProductResponseDto MapToAdminDto(Product product)
        {
            return new AdminProductResponseDto
            {
                Id = product.Id,
                Sku = product.Sku,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                IsActive = product.IsActive,
                QuantityAvailable = product.Inventory?.QuantityAvailable ?? 0,
                QuantityReserved = product.Inventory?.QuantityReserved ?? 0,
                CreatedAt = product.CreatedAt,
                UpdatedAt = product.UpdatedAt
            };
        }
    }
}
