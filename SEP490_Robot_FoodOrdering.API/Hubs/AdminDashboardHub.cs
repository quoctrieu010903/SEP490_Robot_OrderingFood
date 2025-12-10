using Microsoft.AspNetCore.SignalR;
using SEP490_Robot_FoodOrdering.API.Services;

namespace SEP490_Robot_FoodOrdering.API.Hubs
{
    public class AdminDashboardHub : Hub
    {
        public Task JoinAdminGroup()
            => Groups.AddToGroupAsync(Context.ConnectionId, AdminDashboardHubs.AdminsGroup);

        public Task LeaveAdminGroup()
            => Groups.RemoveFromGroupAsync(Context.ConnectionId, AdminDashboardHubs.AdminsGroup);
    }
}
