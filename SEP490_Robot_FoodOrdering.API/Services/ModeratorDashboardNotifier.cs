using Microsoft.AspNetCore.SignalR;
using SEP490_Robot_FoodOrdering.API.Hubs;
using SEP490_Robot_FoodOrdering.Application.Abstractions.Hubs;
using SEP490_Robot_FoodOrdering.Application.DTO.Response.Complain;

namespace SEP490_Robot_FoodOrdering.API.Services
{
    public static class ModeratorDashboardHubs
    {
        public const string ModeratorsGroup = "Moderators";
    }

    public class ModeratorDashboardNotifier : IModeratorDashboardNotifier
    {
        private readonly IHubContext<ModeratorDashboardHub> _hub;
        private readonly ILogger<ModeratorDashboardNotifier> _logger;

        public ModeratorDashboardNotifier(
            IHubContext<ModeratorDashboardHub> hub,
            ILogger<ModeratorDashboardNotifier> logger)
        {
            _hub = hub;
            _logger = logger;
        }

        public Task BroadcastPendingComplainsSnapshotAsync(Dictionary<string, ComplainPeedingInfo> snapshot)
            => _hub.Clients.Group(ModeratorDashboardHubs.ModeratorsGroup)
                .SendAsync("PendingComplainsSnapshotUpdated", snapshot);

        public Task BroadcastTableUpdatedAsync(string tableId, ComplainPeedingInfo info)
            => _hub.Clients.Group(ModeratorDashboardHubs.ModeratorsGroup)
                // ✅ khuyến nghị: 1 object payload
                .SendAsync("DashboardTableUpdated", new { tableId, info });

     
    }
}
