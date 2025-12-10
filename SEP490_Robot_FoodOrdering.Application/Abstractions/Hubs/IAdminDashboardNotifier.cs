using SEP490_Robot_FoodOrdering.Application.DTO.Response.Dashboard;

namespace SEP490_Robot_FoodOrdering.Application.Abstractions.Hubs
{
    public interface IAdminDashboardNotifier
    {
        Task BroadcastDashboardUpdatedAsync(DashboardResponse payload);
    }
}
