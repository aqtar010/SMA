using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMA.API.DTOs;
using SMA.API.Services.ServiceContracts;

namespace SMA.API.Controllers
{
    [ApiController]
    [Authorize(Roles = "Admin,Customer")]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IProductService _productService;

        public ProductsController(IProductService productService) => _productService = productService;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductResponseDto>>> GetProducts(CancellationToken cancellationToken) =>
            Ok(await _productService.GetProductsAsync(cancellationToken));

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<ProductResponseDto>> CreateProduct(CreateProductDto request, CancellationToken cancellationToken)
        {
            var product = await _productService.CreateProductAsync(request, cancellationToken);
            return product == null
                ? BadRequest("A product with this SKU already exists.")
                : CreatedAtAction(nameof(GetProducts), new { id = product.Id }, product);
        }

        [Authorize(Roles = "Admin,SuperAdmin")]
        [HttpGet("admin")]
        public async Task<ActionResult<IEnumerable<AdminProductResponseDto>>> GetAdminProducts(CancellationToken cancellationToken) =>
            Ok(await _productService.GetAdminProductsAsync(cancellationToken));

        [Authorize(Roles = "Admin,SuperAdmin")]
        [HttpPut("{id:guid}")]
        public async Task<ActionResult<AdminProductResponseDto>> UpdateProduct(Guid id, UpdateProductDto request, CancellationToken cancellationToken)
        {
            try
            {
                var product = await _productService.UpdateProductAsync(id, request, cancellationToken);
                return product == null ? NotFound("Product not found.") : Ok(product);
            }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }

        [Authorize(Roles = "Admin,SuperAdmin")]
        [HttpPatch("{id:guid}/stock")]
        public async Task<ActionResult<AdminProductResponseDto>> UpdateStock(Guid id, UpdateStockDto request, CancellationToken cancellationToken)
        {
            try
            {
                var product = await _productService.UpdateStockAsync(id, request, cancellationToken);
                return product == null ? NotFound("Product not found.") : Ok(product);
            }
            catch (InvalidOperationException ex) { return BadRequest(ex.Message); }
        }
    }
}
