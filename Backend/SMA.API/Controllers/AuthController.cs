using Microsoft.AspNetCore.Mvc;
using SMA.API.Models;
using SMA.API.Services.ServiceContracts;

namespace SMA.API.Controllers
{
    [ApiController]    
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private const string RefreshCookieName = "sma_refresh_token";
        private readonly ILogger<AuthController> _logger;
        private readonly IAuthService _authService;
        private readonly ITokenService _tokenService;

        public AuthController(ILogger<AuthController> logger, IAuthService authService, ITokenService tokenService)
        {
            _logger = logger;
            _authService = authService;
            _tokenService = tokenService;
        }

        /// <summary>
        /// POST: api/auth/register
        /// Creates a new user account with credentials.
        /// </summary>
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] CreateUserRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var result = await _authService.RegisterAsync(request);
                if (result == null)
                    return BadRequest("User with this email already exists.");

                _logger.LogInformation("New user registered: {Email}", request.Email);

                return Ok(new { 
                    message = "User registered successfully.",
                    userId = result.UserId,
                    email = result.Email
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error registering user");
                return StatusCode(500, "An error occurred during registration.");
            }
        }

        /// <summary>
        /// POST: api/auth/login
        /// Authenticates user and issues a JWT token.
        /// </summary>
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
                var result = await _authService.LoginAsync(request, ip);
                if (result == null)
                {
                    _logger.LogWarning("Login attempt with invalid credentials for email: {Email}", request.Email);
                    return Unauthorized("Invalid credentials.");
                }

                _logger.LogInformation("User logged in: {Email}", result.Email);

                SetRefreshCookie(result.RefreshToken);
                return Ok(new { 
                    token = result.AccessToken,
                    role = result.Role,
                    userId = result.UserId,
                    email = result.Email
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login");
                return StatusCode(500, "An error occurred during login.");
            }
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh()
        {
            var refreshToken = Request.Cookies[RefreshCookieName];
            if (string.IsNullOrEmpty(refreshToken)) return BadRequest("RefreshToken is required.");
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

            try
            {
                var (accessToken, newRefreshToken, user) = await _tokenService.RotateRefreshTokenAsync(refreshToken, ip);
                SetRefreshCookie(newRefreshToken);
                return Ok(new { token = accessToken, role = user.Role, email = user.Email });
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogWarning(ex, "Invalid refresh attempt");
                return Unauthorized("Invalid refresh token.");
            }
        }

        [HttpPost("revoke")]
        public async Task<IActionResult> Revoke()
        {
            var refreshToken = Request.Cookies[RefreshCookieName];
            if (string.IsNullOrEmpty(refreshToken)) return BadRequest("RefreshToken is required.");
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

            var success = await _tokenService.RevokeRefreshTokenAsync(refreshToken, ip);
            if (!success) return NotFound("Token not found or already revoked.");
            Response.Cookies.Delete(RefreshCookieName, new CookieOptions { Path = "/api/auth" });
            return Ok(new { message = "Token revoked." });
        }

        private void SetRefreshCookie(string refreshToken)
        {
            Response.Cookies.Append(RefreshCookieName, refreshToken, new CookieOptions
            {
                HttpOnly = true,
                Secure = !HttpContext.RequestServices.GetRequiredService<IWebHostEnvironment>().IsDevelopment(),
                SameSite = SameSiteMode.Lax,
                Expires = DateTimeOffset.UtcNow.AddDays(30),
                Path = "/api/auth"
            });
        }
    }
}
