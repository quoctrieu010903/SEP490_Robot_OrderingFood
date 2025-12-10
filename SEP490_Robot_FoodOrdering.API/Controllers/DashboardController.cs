using Microsoft.AspNetCore.Mvc;
using SEP490_Robot_FoodOrdering.Application.DTO.Request;
using SEP490_Robot_FoodOrdering.Application.Service.Interface;
using SEP490_Robot_FoodOrdering.Core.Response;

namespace SEP490_Robot_FoodOrdering.API.Controllers
{
    /// <summary>
    /// Dashboard API endpoints for retrieving system statistics
    /// </summary>
    [Route("api/[controller]")]
    [ApiController]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        /// <summary>
        /// Get dashboard statistics with optional date filtering
        /// </summary>
        /// <remarks>
        /// Returns dashboard statistics including:
        /// - Total number of user accounts
        /// - Total number of products
        /// - Most ordered product with count
        /// - Least ordered product with count
        /// - Total cancelled order items
        /// - Top 5 most ordered products
        /// 
        /// Date filtering:
        /// - If no parameters provided: defaults to current month and year
        /// - If only Year provided: filters by entire year
        /// - If Year and Month provided: filters by that month
        /// - If Year, Month, and Day provided: filters by that specific day
        /// 
        /// Sample requests:
        /// GET /api/dashboard (current month/year)
        /// GET /api/dashboard?year=2024 (entire year 2024)
        /// GET /api/dashboard?year=2024&month=12 (December 2024)
        /// GET /api/dashboard?year=2024&month=12&day=25 (December 25, 2024)
        /// </remarks>
        /// <param name="request">Optional filter parameters (year, month, day)</param>
        /// <returns>Dashboard statistics response</returns>
        /// <response code="200">Successfully retrieved dashboard statistics</response>
        /// <response code="500">Internal server error occurred</response>
        [HttpGet]
        public async Task<ActionResult<BaseResponseModel>> GetDashboard([FromQuery] DashboardRequest? request)
        {
            var result = await _dashboardService.GetDashboardAsync(request);
            return Ok(result);
        }
    }
}
