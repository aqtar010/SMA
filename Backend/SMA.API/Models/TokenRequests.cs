namespace SMA.API.Models
{
    public class RefreshRequest
    {
        public string? RefreshToken { get; set; }
    }
    public class RevokeRequest
    {
        public string? RefreshToken { get; set; }
    }
}
