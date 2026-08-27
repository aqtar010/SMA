using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
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

        [HttpGet("{id:guid}/ratings")]
        public async Task<ActionResult<ProductRatingSummaryDto>> GetRatings(Guid id, CancellationToken cancellationToken)
        {
            var result = await _productService.GetRatingSummaryAsync(id, GetUserIdFromClaims(), cancellationToken);
            return result == null ? NotFound() : Ok(result);
        }

        [HttpPost("{id:guid}/ratings")]
        public async Task<ActionResult<ProductRatingSummaryDto>> SaveRating(Guid id, CreateProductRatingDto request, CancellationToken cancellationToken)
        {
            try { return Ok(await _productService.SaveRatingAsync(id, GetUserIdFromClaims(), request, cancellationToken)); }
            catch (UnauthorizedAccessException ex) { return StatusCode(StatusCodes.Status403Forbidden, ex.Message); }
            catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
        }

        private Guid GetUserIdFromClaims()
        {
            var value = User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? User.FindFirst(ClaimTypes.Name)?.Value;
            return Guid.TryParse(value, out var userId) ? userId : Guid.Empty;
        }

    }
}
