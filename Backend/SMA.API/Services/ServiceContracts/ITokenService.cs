using SMA.API.Entities;

namespace SMA.API.Services.ServiceContracts
{
    public interface ITokenService
    {
        string GenerateAccessToken(User user);
        (string Token, string TokenHash) GenerateRefreshTokenStringAndHash();
        Task<string> CreateRefreshTokenForUserAsync(User user, string createdByIp, int ttlDays = 30);
        Task<(string AccessToken, string RefreshToken)> RotateRefreshTokenAsync(string refreshToken, string ipAddress);
        Task<bool> RevokeRefreshTokenAsync(string refreshToken, string ipAddress);

    }
}
