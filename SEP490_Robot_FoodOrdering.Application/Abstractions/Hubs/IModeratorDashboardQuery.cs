using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SEP490_Robot_FoodOrdering.Application.DTO.Response.Complain;

namespace SEP490_Robot_FoodOrdering.Application.Abstractions.Hubs
{
    public interface IModeratorDashboardQuery
    {
        Task<Dictionary<string, ComplainPeedingInfo>> BuildSnapshotAsync();
        Task<ComplainPeedingInfo?> BuildForTableAsync(Guid tableId);

    }
}
