using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using SEP490_Robot_FoodOrdering.Application.DTO.Request;
using SEP490_Robot_FoodOrdering.Application.DTO.Response.Dashboard;
using SEP490_Robot_FoodOrdering.Application.Service.Interface;
using SEP490_Robot_FoodOrdering.Core.Constants;
using SEP490_Robot_FoodOrdering.Core.Response;
using SEP490_Robot_FoodOrdering.Domain;
using SEP490_Robot_FoodOrdering.Domain.Entities;
using SEP490_Robot_FoodOrdering.Domain.Enums;
using SEP490_Robot_FoodOrdering.Domain.Interface;
using SEP490_Robot_FoodOrdering.Domain.Specifications;

namespace SEP490_Robot_FoodOrdering.Application.Service.Implementation
{
    public class DashboardService : IDashboardService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<DashboardService> _logger;

        public DashboardService(IUnitOfWork unitOfWork, ILogger<DashboardService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<BaseResponseModel<DashboardResponse>> GetDashboardAsync(DashboardRequest? request = null)
        {
            try
            {
                // Set default values: current month and year if not provided
                var now = DateTime.UtcNow;
                var year = request?.Year ?? now.Year;
                var month = request?.Month ?? now.Month;
                var day = request?.Day;

                // Calculate date range based on filter
                DateTime startDate;
                DateTime endDate;

                if (day.HasValue)
                {
                    // Filter by specific day
                    startDate = new DateTime(year, month, day.Value, 0, 0, 0, DateTimeKind.Utc);
                    endDate = startDate.AddDays(1);
                }
                else if (request?.Month.HasValue == true)
                {
                    // Filter by month (when month is explicitly provided)
                    startDate = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
                    endDate = startDate.AddMonths(1);
                }
                else if (request?.Year.HasValue == true)
                {
                    // Filter by entire year (when only year is provided)
                    startDate = new DateTime(year, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                    endDate = startDate.AddYears(1);
                }
                else
                {
                    // Default: current month and year
                    startDate = new DateTime(year, month, 1, 0, 0, 0, DateTimeKind.Utc);
                    endDate = startDate.AddMonths(1);
                }

                _logger.LogInformation(
                    "DashboardService: Getting dashboard statistics from {StartDate} to {EndDate}",
                    startDate, endDate);

                // 1. Get total users count
                var totalUsers = await _unitOfWork.Repository<User, Guid>()
                    .CountAsync();

                // 2. Get total products count
                var totalProducts = await _unitOfWork.Repository<Product, Guid>()
                    .CountAsync();

                // 3. Get all order items within date range, include Product relationship
                var orderItems = await _unitOfWork.Repository<OrderItem, Guid>()
                    .GetAllWithSpecWithInclueAsync(
                        new BaseSpecification<OrderItem>(x =>
                            x.CreatedTime >= startDate && x.CreatedTime < endDate),
                        false, // AsNoTracking for better performance
                        oi => oi.Product);

                // Filter out items without products
                var orderItemsWithProducts = orderItems
                    .Where(oi => oi.Product != null)
                    .ToList();

                // 4. Group by ProductId and count
                var productOrderCounts = orderItemsWithProducts
                    .GroupBy(oi => new { oi.ProductId, oi.Product!.Name })
                    .Select(g => new ProductOrderStat
                    {
                        ProductId = g.Key.ProductId,
                        ProductName = g.Key.Name,
                        OrderCount = g.Count()
                    })
                    .OrderByDescending(x => x.OrderCount)
                    .ToList();

                // 5. Get most ordered product
                var mostOrderedProduct = productOrderCounts.FirstOrDefault();

                // 6. Get least ordered product (only if there are products with orders)
                var leastOrderedProduct = productOrderCounts.Count > 0
                    ? productOrderCounts.LastOrDefault()
                    : null;

                // 7. Get total cancelled items
                var totalCancelledItems = orderItemsWithProducts
                    .Count(oi => oi.Status == OrderItemStatus.Cancelled);

                // 8.1 Get total complains in range
                var totalComplains = await _unitOfWork.Repository<Complain, Guid>()
                    .CountAsync(new BaseSpecification<Complain>(c =>
                        c.CreatedTime >= startDate && c.CreatedTime < endDate));

                var totalComplainsPending = await _unitOfWork.Repository<Complain, Guid>()
                    .CountAsync(new BaseSpecification<Complain>(c =>
                        c.CreatedTime >= startDate && c.CreatedTime < endDate && c.isPending));

                var totalComplainsHandled = await _unitOfWork.Repository<Complain, Guid>()
                    .CountAsync(new BaseSpecification<Complain>(c =>
                        c.CreatedTime >= startDate && c.CreatedTime < endDate && !c.isPending));

                // 8.2 Get total remade order items in range
                var totalRemakeItems = await _unitOfWork.Repository<RemakeOrderItem, Guid>()
                    .CountAsync(new BaseSpecification<RemakeOrderItem>(r =>
                        r.CreatedTime >= startDate && r.CreatedTime < endDate));

                // 8. Get total order items
                var totalOrderItems = orderItemsWithProducts.Count;

                // 8. Get top 5 most ordered products
                var top5MostOrderedProducts = productOrderCounts
                    .Take(5)
                    .ToList();

                // Build response
                var response = new DashboardResponse
                {
                    TotalUsers = totalUsers,
                    TotalProducts = totalProducts,
                    MostOrderedProduct = mostOrderedProduct,
                    LeastOrderedProduct = leastOrderedProduct,
                    TotalCancelledItems = totalCancelledItems,
                    TotalComplains = totalComplains,
                    TotalComplainsPending = totalComplainsPending,
                    TotalComplainsHandled = totalComplainsHandled,
                    TotalRemakeItems = totalRemakeItems,
                    TotalOrderItems = totalOrderItems,
                    Top5MostOrderedProducts = top5MostOrderedProducts
                };

                _logger.LogInformation(
                    "DashboardService: Retrieved dashboard statistics - Users: {TotalUsers}, Products: {TotalProducts}, Cancelled: {Cancelled}",
                    totalUsers, totalProducts, totalCancelledItems);

                return new BaseResponseModel<DashboardResponse>(
                    StatusCodes.Status200OK,
                    ResponseCodeConstants.SUCCESS,
                    response,
                    null,
                    "Lấy thống kê dashboard thành công");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DashboardService: Error occurred while getting dashboard statistics");
                throw;
            }
        }
    }
}
