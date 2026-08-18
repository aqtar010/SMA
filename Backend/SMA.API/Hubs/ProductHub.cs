using Microsoft.AspNetCore.SignalR;

namespace SMA.API.Hubs
{
    public class ProductHub : Hub
    {
        public async Task JoinProductGroup(string productId)
        {
            if (string.IsNullOrWhiteSpace(productId))
            {
                return;
            }

            await Groups.AddToGroupAsync(Context.ConnectionId, productId);
        }

        public async Task LeaveProductGroup(string productId)
        {
            if (string.IsNullOrWhiteSpace(productId))
            {
                return;
            }

            await Groups.RemoveFromGroupAsync(Context.ConnectionId, productId);
        }
    }
}
