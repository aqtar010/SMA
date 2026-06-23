using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using SMA.API.Data;
using SMA.API.Models;
using SMA.API.Utilities;
using SMA.API.Enums;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using System.Text;

namespace SMA.API.Controllers
{
    [ApiController]    
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly ILogger<AuthController> _logger;
        private readonly IConfiguration _configuration;
        private readonly AppDbContext _dbContext;

        public AuthController(ILogger<AuthController> logger, IConfiguration configuration, AppDbContext dbContext)
        {
            _logger = logger;
            _configuration = configuration;
            _dbContext = dbContext;
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

            // Check if email already exists
            if (_dbContext.Users.Any(u => u.Email == request.Email))
                return BadRequest("User with this email already exists.");

            // Prevent creation of SuperAdmin via API
            if (!string.IsNullOrEmpty(request.Role) &&
                string.Equals(request.Role, UserRoles.SuperAdmin.ToString(), StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest("Cannot create SuperAdmin via API. Use the CLI tool.");
            }

            try
            {
                var newUser = new Entities.User
                {
                    Id = Guid.NewGuid(),
                    Email = request.Email,
                    FirstName = request.FirstName,
                    LastName = request.LastName,
                    PasswordHash = PasswordHasher.HashPassword(request.Password),
                    Role = request.Role ?? "Customer",
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };

                _dbContext.Users.Add(newUser);
                await _dbContext.SaveChangesAsync();

                _logger.LogInformation("New user registered: {Email}", request.Email);

                return Ok(new { 
                    message = "User registered successfully.",
                    userId = newUser.Id,
                    email = newUser.Email 
                });
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
                // Retrieve user from database
                var user = await _dbContext.Users.FirstOrDefaultAsync(u => u.Email == request.Email);

                if (user == null || !user.IsActive)
                {
                    _logger.LogWarning("Login attempt with invalid email: {Email}", request.Email);
                    return Unauthorized("Invalid credentials.");
                }

                // Verify password
                if (!PasswordHasher.VerifyPassword(request.Password, user.PasswordHash))
                {
                    _logger.LogWarning("Login attempt with invalid password for email: {Email}", request.Email);
                    return Unauthorized("Invalid credentials.");
                }

                // Update last login timestamp
                user.LastLogin = DateTime.UtcNow;
                user.UpdatedAt = DateTime.UtcNow;
                _dbContext.Users.Update(user);
                await _dbContext.SaveChangesAsync();

                // Generate JWT token
                var token = GenerateJwtToken(user.Id, user.Email, user.Role);

                _logger.LogInformation("User logged in: {Email}", user.Email);

                return Ok(new { 
                    token = token,
                    role = user.Role,
                    userId = user.Id,
                    email = user.Email
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login");
                return StatusCode(500, "An error occurred during login.");
            }
        }

        private string GenerateJwtToken(Guid userId, string email, string role)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var secretKey = _configuration["JwtSettings:SecretKey"]
                ?? "default_secret_key_that_is_at_least_32_characters_long";
            var key = Encoding.ASCII.GetBytes(secretKey);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
                    new Claim(ClaimTypes.Name, email),
                    new Claim(ClaimTypes.Role, role),
                    new Claim("tenant_id", "my-ecommerce-app")
                }),
                Expires = DateTime.UtcNow.AddHours(2),
                Issuer = _configuration["JwtSettings:Issuer"],
                Audience = _configuration["JwtSettings:Audience"],
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256Signature
                )
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
    }
}
