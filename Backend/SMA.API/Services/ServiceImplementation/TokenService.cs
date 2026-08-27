using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SMA.API.Data;
using SMA.API.Entities;
using SMA.API.Services.ServiceContracts;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace SMA.API.Services.ServiceImplementation
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _config;
        private readonly AppDbContext _db;
        private readonly ILogger<TokenService> _logger;

        public TokenService(IConfiguration config, AppDbContext db, ILogger<TokenService> logger)
        {
            _config = config;
            _db = db;
            _logger = logger;
        }

        public string GenerateAccessToken(User user)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var secretKey = _config["JwtSettings:SecretKey"]
                ?? throw new InvalidOperationException("JwtSettings:SecretKey not configured");
            var key = Encoding.ASCII.GetBytes(secretKey);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[]
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.Email),
                    new Claim(ClaimTypes.Role, user.Role),
                    new Claim("tenant_id", "my-ecommerce-app")
                }),
                Expires = DateTime.UtcNow.AddHours(2),
                Issuer = _config["JwtSettings:Issuer"],
                Audience = _config["JwtSettings:Audience"],
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        public (string Token, string TokenHash) GenerateRefreshTokenStringAndHash()
        {
            // 64 bytes -> 86+ chars base64, enough entropy
            var randomBytes = RandomNumberGenerator.GetBytes(64);
            var token = Convert.ToBase64String(randomBytes);

            using var sha = SHA256.Create();
            var hashBytes = sha.ComputeHash(Encoding.UTF8.GetBytes(token));
            var tokenHash = Convert.ToHexString(hashBytes); // .NET 5+; hex uppercase

            return (token, tokenHash);
        }

        public async Task<string> CreateRefreshTokenForUserAsync(User user, string createdByIp, int ttlDays = 30)
        {
            var (token, tokenHash) = GenerateRefreshTokenStringAndHash();

            var refreshToken = new RefreshToken
            {
                TokenHash = tokenHash,
                UserId = user.Id,
                Expires = DateTime.UtcNow.AddDays(ttlDays),
                CreatedAt = DateTime.UtcNow,
                CreatedByIp = createdByIp
            };

            _db.RefreshTokens.Add(refreshToken);
            await _db.SaveChangesAsync();

            return token; // plain token returned to client
        }

        public async Task<(string AccessToken, string RefreshToken, User User)> RotateRefreshTokenAsync(string refreshToken, string ipAddress)
        {
            if (string.IsNullOrEmpty(refreshToken))
                throw new ArgumentException("Refresh token is required.", nameof(refreshToken));

            var hash = ComputeHash(refreshToken);
            var stored = await _db.RefreshTokens.Include(rt => rt.User).FirstOrDefaultAsync(rt => rt.TokenHash == hash);

            if (stored == null || !stored.IsActive)
                throw new InvalidOperationException("Invalid refresh token.");

            // create new refresh token
            var (newToken, newHash) = GenerateRefreshTokenStringAndHash();

            // revoke old
            stored.IsRevoked = true;
            stored.RevokedAt = DateTime.UtcNow;
            stored.RevokedByIp = ipAddress;
            stored.ReplacedByTokenHash = newHash;

            // add new token record
            var newRefreshEntity = new RefreshToken
            {
                TokenHash = newHash,
                UserId = stored.UserId,
                Expires = DateTime.UtcNow.AddDays(30),
                CreatedAt = DateTime.UtcNow,
                CreatedByIp = ipAddress
            };

            _db.RefreshTokens.Add(newRefreshEntity);
            await _db.SaveChangesAsync();

            // create access token
            var accessToken = GenerateAccessToken(stored.User!);

            return (accessToken, newToken, stored.User!);
        }

        public async Task<bool> RevokeRefreshTokenAsync(string refreshToken, string ipAddress)
        {
            var hash = ComputeHash(refreshToken);
            var stored = await _db.RefreshTokens.FirstOrDefaultAsync(rt => rt.TokenHash == hash);
            if (stored == null || stored.IsRevoked) return false;

            stored.IsRevoked = true;
            stored.RevokedAt = DateTime.UtcNow;
            stored.RevokedByIp = ipAddress;

            _db.RefreshTokens.Update(stored);
            await _db.SaveChangesAsync();
            return true;
        }

        private static string ComputeHash(string token)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(token));
            return Convert.ToHexString(bytes);
        }
    }
}
