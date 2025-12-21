using System.Text.Json;
using Microsoft.Extensions.Logging;
using SEP490_Robot_FoodOrdering.Application.Abstractions.Hubs;
using SEP490_Robot_FoodOrdering.Application.DTO.Request;
using SEP490_Robot_FoodOrdering.Application.DTO.Response.Dashboard;
using SEP490_Robot_FoodOrdering.Application.Service.Interface;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SEP490_Robot_FoodOrdering.Application.Service.Implementation
{
    public class AdminDashboardRefresher : IAdminDashboardRefresher
    {
        private readonly IDashboardService _dashboardService;
        private readonly IAdminDashboardNotifier _notifier;
        private readonly ILogger<AdminDashboardRefresher> _logger;

        public AdminDashboardRefresher(
            IDashboardService dashboardService,
            IAdminDashboardNotifier notifier,
            ILogger<AdminDashboardRefresher> logger)
        {
            _dashboardService = dashboardService;
            _notifier = notifier;
            _logger = logger;
        }

        public async Task<DashboardResponse> PushDashboardAsync(DashboardRequest? request = null)
        {
            var dashboard = await _dashboardService.GetDashboardAsync(request);

            if (dashboard.Data is not null)
            {
                await _notifier.BroadcastDashboardUpdatedAsync(dashboard.Data);

                _logger.LogInformation(
                    "AdminDashboardRefresher: broadcasted dashboard update: {DashboardJson}",
                    JsonSerializer.Serialize(dashboard.Data, new JsonSerializerOptions
                    {
                        WriteIndented = false
                    })
                );
            }
            else
            {
                _logger.LogWarning("AdminDashboardRefresher: dashboard data is null, nothing to broadcast");
            }

            return dashboard.Data ?? new DashboardResponse();
        }
    }
}
