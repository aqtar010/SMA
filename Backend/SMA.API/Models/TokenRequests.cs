namespace SMA.API.Models
{
    public class RefreshRequest
    {
        public string RefreshToken { get; set; } = string.Empty;
    }
    public class RevokeRequest
    {
        public string RefreshToken { get; set; } = string.Empty;
    }
}
