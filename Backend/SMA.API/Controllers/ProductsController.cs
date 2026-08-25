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

    }
}
