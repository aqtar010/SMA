using SMA.API.Models;

namespace SMA.API.Services.ServiceContracts
{
    public sealed record RegistrationResult(Guid UserId, string Email);
    public sealed record LoginResult(string AccessToken, string RefreshToken, string Role, Guid UserId, string Email);

    public interface IAuthService
    {
        Task<RegistrationResult?> RegisterAsync(CreateUserRequest request, CancellationToken cancellationToken = default);
        Task<LoginResult?> LoginAsync(LoginRequest request, string ipAddress, CancellationToken cancellationToken = default);
    }
}
