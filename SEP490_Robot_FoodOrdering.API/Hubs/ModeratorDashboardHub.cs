using Microsoft.AspNetCore.SignalR;

namespace SEP490_Robot_FoodOrdering.API.Hubs
{
    public class ModeratorDashboardHub : Hub
    {
        public Task JoinModeratorGroup()
            => Groups.AddToGroupAsync(Context.ConnectionId, "Moderators");

        public Task LeaveModeratorGroup()
            => Groups.RemoveFromGroupAsync(Context.ConnectionId, "Moderators");
    }
}
