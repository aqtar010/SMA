using Microsoft.EntityFrameworkCore;
using SMA.API.Data;
using SMA.API.Entities;
using SMA.API.Enums;
using SMA.API.Models;
using SMA.API.Services.ServiceContracts;
using SMA.API.Utilities;

namespace SMA.API.Services.ServiceImplementation
{
    public class AuthService : IAuthService
    {
        private readonly AppDbContext _dbContext;
        private readonly ITokenService _tokenService;

        public AuthService(AppDbContext dbContext, ITokenService tokenService)
        {
            _dbContext = dbContext;
            _tokenService = tokenService;
        }

        public async Task<RegistrationResult?> RegisterAsync(CreateUserRequest request, CancellationToken cancellationToken = default)
        {
            if (await _dbContext.Users.AnyAsync(user => user.Email == request.Email, cancellationToken))
                return null;

            if (string.Equals(request.Role, UserRoles.SuperAdmin.ToString(), StringComparison.OrdinalIgnoreCase))
                throw new InvalidOperationException("Cannot create SuperAdmin via API. Use the CLI tool.");

            var user = new User
            {
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                PasswordHash = PasswordHasher.HashPassword(request.Password),
                Role = request.Role ?? UserRoles.Customer.ToString(),
                IsActive = true
            };

            _dbContext.Users.Add(user);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return new RegistrationResult(user.Id, user.Email);
        }

        public async Task<LoginResult?> LoginAsync(LoginRequest request, string ipAddress, CancellationToken cancellationToken = default)
        {
            var user = await _dbContext.Users.FirstOrDefaultAsync(item => item.Email == request.Email, cancellationToken);
            if (user == null || !user.IsActive || !PasswordHasher.VerifyPassword(request.Password, user.PasswordHash))
                return null;

            user.LastLogin = DateTime.UtcNow;
            user.UpdatedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync(cancellationToken);

            var accessToken = _tokenService.GenerateAccessToken(user);
            var refreshToken = await _tokenService.CreateRefreshTokenForUserAsync(user, ipAddress);
            return new LoginResult(accessToken, refreshToken, user.Role, user.Id, user.Email);
        }
    }
}
