using Microsoft.AspNetCore.SignalR;
using SEP490_Robot_FoodOrdering.Domain.Enums;

namespace SEP490_Robot_FoodOrdering.API.Hubs
{
    /// <summary>
    /// SignalR Hub for real-time order and order item status notifications
    /// </summary>
    public class OrderNotificationHub : Hub
    {
        /// <summary>
        /// Joins a group based on table ID for table-specific notifications
        /// </summary>
        /// <param name="tableId">The table ID to join</param>
        public async Task JoinTableGroup(string tableId)
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"Table_{tableId}");
        }

        /// <summary>
        /// Leaves a table group
        /// </summary>
        /// <param name="tableId">The table ID to leave</param>
        public async Task LeaveTableGroup(string tableId)
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"Table_{tableId}");
        }

        /// <summary>
        /// Joins a group for kitchen staff to receive all order updates
        /// </summary>
        public async Task JoinKitchenGroup()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "Kitchen");
        }

        /// <summary>
        /// Leaves the kitchen group
        /// </summary>
        public async Task LeaveKitchenGroup()
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "Kitchen");
        }

        /// <summary>
        /// Joins a group for waiter staff to receive serving notifications
        /// </summary>
        public async Task JoinWaiterGroup()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "Waiters");
        }

        /// <summary>
        /// Leaves the waiter group
        /// </summary>
        public async Task LeaveWaiterGroup()
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "Waiters");
        }

        /// <summary>
        /// Joins a group for moderators to receive all system notifications
        /// </summary>
        public async Task JoinModeratorGroup()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "Moderators");
        }

        /// <summary>
        /// Leaves the moderator group
        /// </summary>
        public async Task LeaveModeratorGroup()
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "Moderators");
        }

        /// <summary>
        /// Called when a client connects
        /// </summary>
        public override async Task OnConnectedAsync()
        {
            await base.OnConnectedAsync();
            Console.WriteLine($"Client {Context.ConnectionId} connected to OrderNotificationHub");
        }

        /// <summary>
        /// Called when a client disconnects
        /// </summary>
        /// <param name="exception">The exception that caused the disconnection, if any</param>
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            await base.OnDisconnectedAsync(exception);
            Console.WriteLine($"Client {Context.ConnectionId} disconnected from OrderNotificationHub");
        }
    }
}
