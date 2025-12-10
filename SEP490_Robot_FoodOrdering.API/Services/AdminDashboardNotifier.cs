using Microsoft.AspNetCore.SignalR;
using SEP490_Robot_FoodOrdering.API.Hubs;
using SEP490_Robot_FoodOrdering.Application.Abstractions.Hubs;
using SEP490_Robot_FoodOrdering.Application.DTO.Response.Dashboard;

namespace SEP490_Robot_FoodOrdering.API.Services
{
    public static class AdminDashboardHubs
    {
        public const string AdminsGroup = "Admins";
    }

    public class AdminDashboardNotifier : IAdminDashboardNotifier
    {
        private readonly IHubContext<AdminDashboardHub> _hub;
        private readonly ILogger<AdminDashboardNotifier> _logger;

        public AdminDashboardNotifier(
            IHubContext<AdminDashboardHub> hub,
            ILogger<AdminDashboardNotifier> logger)
        {
            _hub = hub;
            _logger = logger;
        }

        public Task BroadcastDashboardUpdatedAsync(DashboardResponse payload)
            => _hub.Clients.Group(AdminDashboardHubs.AdminsGroup)
                .SendAsync("DashboardStatsUpdated", payload);
    }
}
