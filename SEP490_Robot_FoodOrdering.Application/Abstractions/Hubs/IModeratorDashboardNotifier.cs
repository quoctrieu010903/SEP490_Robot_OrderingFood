using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SEP490_Robot_FoodOrdering.Application.DTO.Response.Complain;

namespace SEP490_Robot_FoodOrdering.Application.Abstractions.Hubs
{
    public interface IModeratorDashboardNotifier 
    {
        Task BroadcastPendingComplainsSnapshotAsync(Dictionary<string, ComplainPeedingInfo> snapshot);

       
        Task BroadcastTableUpdatedAsync(string tableId, ComplainPeedingInfo info);



    }
}
