
using SEP490_Robot_FoodOrdering.Application.Abstractions.Hubs;

namespace SEP490_Robot_FoodOrdering.Application.Service.Implementation
{
    public class ModeratorDashboardRefresher : IModeratorDashboardRefresher
    {
        private readonly IModeratorDashboardQuery _query;
        private readonly IModeratorDashboardNotifier _notifier;

        public ModeratorDashboardRefresher(
            IModeratorDashboardQuery query,
            IModeratorDashboardNotifier notifier)
        {
            _query = query;
            _notifier = notifier;
        }

        public async Task PushTableAsync(Guid tableId)
        {
            var info = await _query.BuildForTableAsync(tableId);
            if (info == null) return;

            await _notifier.BroadcastTableUpdatedAsync(tableId.ToString(), info);
        }

        public async Task PushSnapshotAsync()
        {
            var snap = await _query.BuildSnapshotAsync();
            await _notifier.BroadcastPendingComplainsSnapshotAsync(snap);
        }
    }
}
