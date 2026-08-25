using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMA.API.DTOs;
using SMA.API.Services.ServiceContracts;

namespace SMA.API.Controllers.Admin
{
    [ApiController]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [Route("api/admin/products")]
    public class AdminProductController : ControllerBase
    {
        private readonly IProductService _productService;

        public AdminProductController(IProductService productService) => _productService = productService;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<AdminProductResponseDto>>> GetProducts(CancellationToken cancellationToken) =>
            Ok(await _productService.GetAdminProductsAsync(cancellationToken));

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<ProductResponseDto>> CreateProduct(CreateProductDto request, CancellationToken cancellationToken)
        {
            var product = await _productService.CreateProductAsync(request, cancellationToken);
            return product == null
                ? BadRequest("A product with this SKU already exists.")
                : CreatedAtAction(nameof(GetProducts), new { id = product.Id }, product);
        }

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
