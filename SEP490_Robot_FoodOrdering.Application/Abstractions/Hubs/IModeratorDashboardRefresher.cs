using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEP490_Robot_FoodOrdering.Application.Abstractions.Hubs
{
    public interface IModeratorDashboardRefresher
    {
        Task PushTableAsync(Guid tableId);
        Task PushTableAsync(Guid tableId, CancellationToken ct = default);
        Task PushSnapshotAsync(); // optional
    }
}
