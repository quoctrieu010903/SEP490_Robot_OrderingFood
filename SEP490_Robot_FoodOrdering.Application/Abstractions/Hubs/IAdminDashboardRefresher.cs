using SEP490_Robot_FoodOrdering.Application.DTO.Request;
using SEP490_Robot_FoodOrdering.Application.DTO.Response.Dashboard;

namespace SEP490_Robot_FoodOrdering.Application.Abstractions.Hubs
{
    public interface IAdminDashboardRefresher
    {
        Task<DashboardResponse> PushDashboardAsync(DashboardRequest? request = null);
    }
}
