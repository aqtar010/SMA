using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SMA.API.Data;
using SMA.API.DTOs;
using SMA.API.Entities;
using SMA.API.Hubs;
using SMA.API.Services.ServiceContracts;

namespace SMA.API.Services.ServiceImplementation
{
    public class ProductService : IProductService
    {
        private readonly AppDbContext _context;
        private readonly IHubContext<ProductHub> _hubContext;
        private readonly IProductCache _productCache;

        public ProductService(AppDbContext context, IHubContext<ProductHub> hubContext, IProductCache productCache)
        {
            _context = context;
            _hubContext = hubContext;
            _productCache = productCache;
        }

        public async Task<IReadOnlyList<ProductResponseDto>> GetProductsAsync(CancellationToken cancellationToken = default)
        {
            var cachedProducts = await _productCache.GetActiveProductsAsync(cancellationToken);
            if (cachedProducts != null)
                return cachedProducts;

            var products = await _context.Products.Where(product => product.IsActive)
                .Select(product => new ProductResponseDto
                {
                    Id = product.Id, Sku = product.Sku, Name = product.Name,
                    Description = product.Description, Price = product.Price,
                    QuantityAvailable = product.Inventory == null ? 0 : product.Inventory.QuantityAvailable
                }).ToListAsync(cancellationToken);
            await _productCache.SetActiveProductsAsync(products, cancellationToken);
            return products;
        }

        public async Task<ProductResponseDto?> CreateProductAsync(CreateProductDto request, CancellationToken cancellationToken = default)
        {
            if (await _context.Products.AnyAsync(product => product.Sku == request.Sku, cancellationToken))
                return null;

            var product = new Product
            {
                Sku = request.Sku, Name = request.Name, Description = request.Description, Price = request.Price,
                Inventory = new Inventory { QuantityAvailable = request.InitialStock, QuantityReserved = 0 }
            };
            _context.Products.Add(product);
            await _context.SaveChangesAsync(cancellationToken);
            await _productCache.InvalidateActiveProductsAsync(cancellationToken);
            return MapProduct(product);
        }

        public async Task<IReadOnlyList<AdminProductResponseDto>> GetAdminProductsAsync(CancellationToken cancellationToken = default)
        {
            var cachedProducts = await _productCache.GetAdminProductsAsync(cancellationToken);
            if (cachedProducts != null)
                return cachedProducts;

            var products = await _context.Products.OrderByDescending(product => product.UpdatedAt)
                .Select(product => new AdminProductResponseDto
                {
                    Id = product.Id, Sku = product.Sku, Name = product.Name,
                    Description = product.Description, Price = product.Price, IsActive = product.IsActive,
                    QuantityAvailable = product.Inventory == null ? 0 : product.Inventory.QuantityAvailable,
                    QuantityReserved = product.Inventory == null ? 0 : product.Inventory.QuantityReserved,
                    CreatedAt = product.CreatedAt, UpdatedAt = product.UpdatedAt
                }).ToListAsync(cancellationToken);
            await _productCache.SetAdminProductsAsync(products, cancellationToken);
            return products;
        }

        public async Task<AdminProductResponseDto?> UpdateProductAsync(Guid id, UpdateProductDto request, CancellationToken cancellationToken = default)
        {
            var product = await _context.Products.Include(item => item.Inventory).FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
            if (product == null) return null;
            if (!string.IsNullOrWhiteSpace(request.Sku) && request.Sku != product.Sku)
            {
                if (await _context.Products.AnyAsync(item => item.Sku == request.Sku && item.Id != id, cancellationToken))
                    throw new InvalidOperationException("A product with this SKU already exists.");
                product.Sku = request.Sku;
            }
            if (!string.IsNullOrWhiteSpace(request.Name)) product.Name = request.Name;
            if (request.Description != null) product.Description = request.Description;
            if (request.Price.HasValue) product.Price = request.Price.Value;
            if (request.IsActive.HasValue) product.IsActive = request.IsActive.Value;
            product.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
            await _productCache.InvalidateActiveProductsAsync(cancellationToken);
            await PublishUpdateAsync(product);
            return MapAdminProduct(product);
        }

        public async Task<AdminProductResponseDto?> UpdateStockAsync(Guid id, UpdateStockDto request, CancellationToken cancellationToken = default)
        {
            var product = await _context.Products.Include(item => item.Inventory).FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
            if (product == null) return null;
            if (product.Inventory == null)
            {
                product.Inventory = new Inventory { ProductId = product.Id, QuantityReserved = 0 };
                _context.Inventory.Add(product.Inventory);
            }
            if (request.QuantityAvailable < product.Inventory.QuantityReserved)
                throw new InvalidOperationException($"Stock cannot be lower than reserved quantity ({product.Inventory.QuantityReserved}).");
            product.Inventory.QuantityAvailable = request.QuantityAvailable;
            product.Inventory.UpdatedAt = DateTime.UtcNow;
            product.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
            await _productCache.InvalidateActiveProductsAsync(cancellationToken);
            await PublishUpdateAsync(product);
            return MapAdminProduct(product);
        }

        private async Task PublishUpdateAsync(Product product)
        {
            var update = new
            {
                id = product.Id, productId = product.Id, product.Sku, product.Name, product.Description,
                product.Price, product.IsActive, quantityAvailable = product.Inventory?.QuantityAvailable ?? 0,
                updatedAt = product.UpdatedAt
            };
            await _hubContext.Clients.Group(product.Id.ToString()).SendAsync("ProductUpdated", update);
            await _hubContext.Clients.Group("products").SendAsync("ProductUpdated", update);
        }

        private static ProductResponseDto MapProduct(Product product) => new()
        {
            Id = product.Id, Sku = product.Sku, Name = product.Name, Description = product.Description,
            Price = product.Price, QuantityAvailable = product.Inventory?.QuantityAvailable ?? 0
        };

        private static AdminProductResponseDto MapAdminProduct(Product product) => new()
        {
            Id = product.Id, Sku = product.Sku, Name = product.Name, Description = product.Description,
            Price = product.Price, IsActive = product.IsActive,
            QuantityAvailable = product.Inventory?.QuantityAvailable ?? 0,
            QuantityReserved = product.Inventory?.QuantityReserved ?? 0,
            CreatedAt = product.CreatedAt, UpdatedAt = product.UpdatedAt
        };
    }
}
