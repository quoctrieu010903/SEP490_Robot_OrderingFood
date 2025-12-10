using SEP490_Robot_FoodOrdering.Application.DTO.Request;
using SEP490_Robot_FoodOrdering.Application.DTO.Response.Dashboard;
using SEP490_Robot_FoodOrdering.Core.Response;

namespace SEP490_Robot_FoodOrdering.Application.Service.Interface
{
    public interface IDashboardService
    {
        /// <summary>
        /// Get dashboard statistics with optional date filtering
        /// </summary>
        /// <param name="request">Filter request with year, month, day (defaults to current month/year if not provided)</param>
        /// <returns>Dashboard statistics including total users, products, most/least ordered products, cancelled items, and top 5 products</returns>
        Task<BaseResponseModel<DashboardResponse>> GetDashboardAsync(DashboardRequest? request = null);
    }
}
