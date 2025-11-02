using Microsoft.AspNetCore.SignalR;
using SEP490_Robot_FoodOrdering.Application.Abstractions.Hub;

namespace SEP490_Robot_FoodOrdering.API.Hubs
{
    public class OrderItemNotificationHub : Hub<IOrderItemNotificationClient>
    {
        public override async Task OnConnectedAsync()
        {
            Console.WriteLine($"Client connected: {Context.ConnectionId}");
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? ex)
        {
            Console.WriteLine($"Client disconnected: {Context.ConnectionId}");
            await base.OnDisconnectedAsync(ex);
        }
    }
}
