
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.SignalR;
using SEP490_Robot_FoodOrdering.Application.DTO.Response.Order;
using SEP490_Robot_FoodOrdering.Domain.Entities;
using static Microsoft.AspNetCore.Hosting.Internal.HostingApplication;

namespace SEP490_Robot_FoodOrdering.Application.Abstractions.Hub
{
    public interface IOrderItemNotificationClient
    {
        Task ReceiveOrderItemUpdated(OrderItemResponse item);
        Task ReceiveOrderItemListUpdated(IEnumerable<OrderResponse> items);
    }
    public sealed class OrderItemNotificationClient : Hub<IOrderItemNotificationClient>
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
