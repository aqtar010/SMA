using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SMA.API.DTOs;
using SMA.API.Services.ServiceContracts;

namespace SMA.API.Controllers.Admin
{
    [ApiController]
    [Authorize(Roles = "Admin,SuperAdmin")]
    [Route("api/admin/analytics")]
    public class AdminAnalyticsController : ControllerBase
    {
        private readonly IOrderService _orderService;

        public AdminAnalyticsController(IOrderService orderService) => _orderService = orderService;

        [HttpGet]
        public async Task<ActionResult<AdminAnalyticsDto>> Get([FromQuery] int days = 7, CancellationToken cancellationToken = default)
        {
            return Ok(await _orderService.GetAdminAnalyticsAsync(Math.Clamp(days, 1, 90), cancellationToken));
        }
    }
}
