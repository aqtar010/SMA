namespace SMA.API.Models
{
    public class LoginRequest
    {
        public Guid UserId { get; set; } = Guid.Empty;

        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty; // In a real app, this is verified against a hashed password
    }
}
