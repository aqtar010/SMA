namespace SMA.API.Configuration
{
    public class StripeOptions
    {
        public string SecretKey { get; set; } = string.Empty;
        public string WebhookSecret { get; set; } = string.Empty;
        public string SuccessUrl { get; set; } = "http://localhost:3000/orders/{0}";
        public string CancelUrl { get; set; } = "http://localhost:3000/checkout?cancelled=true&orderId={0}";
    }
}