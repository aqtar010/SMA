using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMA.InventoryForecasting;

namespace SMA.API.Controllers.Admin;

[ApiController]
[Authorize(Roles = "Admin,SuperAdmin")]
[Route("api/admin/inventory-forecast")]
public sealed class AdminInventoryForecastController(IInventoryForecastService forecastService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<InventoryForecast>> Get(CancellationToken cancellationToken)
    {
        var forecast = await forecastService.GetLatestAsync(cancellationToken);
        return forecast is null ? NoContent() : Ok(forecast);
    }
}
