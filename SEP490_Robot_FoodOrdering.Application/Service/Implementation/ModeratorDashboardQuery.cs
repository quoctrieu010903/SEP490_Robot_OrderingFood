using SEP490_Robot_FoodOrdering.Application.Abstractions.Hubs;
using SEP490_Robot_FoodOrdering.Application.DTO.Response;
using SEP490_Robot_FoodOrdering.Application.DTO.Response.Complain;
using SEP490_Robot_FoodOrdering.Application.Service.Interface;
using SEP490_Robot_FoodOrdering.Core.CustomExceptions;
using SEP490_Robot_FoodOrdering.Domain;
using SEP490_Robot_FoodOrdering.Domain.Entities;
using SEP490_Robot_FoodOrdering.Domain.Enums;
using SEP490_Robot_FoodOrdering.Domain.Interface;
using SEP490_Robot_FoodOrdering.Domain.Specifications;

namespace SEP490_Robot_FoodOrdering.Application.Service.Implementation
{
    public class ModeratorDashboardQuery : IModeratorDashboardQuery
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IOrderStatsQuery _orderStatsQuery;

        public ModeratorDashboardQuery(IUnitOfWork unitOfWork, IOrderStatsQuery orderStatsQuery)
        {
            _unitOfWork = unitOfWork;
            _orderStatsQuery = orderStatsQuery;
        }

        public async Task<Dictionary<string, ComplainPeedingInfo>> BuildSnapshotAsync()
        {
            var tables = await _unitOfWork.Repository<Table, Guid>()
                .GetAllWithIncludeAsync(true, t => t.Orders, t => t.Sessions);

            if (tables == null || !tables.Any())
                throw new ErrorException(404, "No tables found");

            var complains = await _unitOfWork.Repository<Complain, Guid>()
                .GetAllWithSpecAsync(new BaseSpecification<Complain>(x => x.isPending));

            var pendingCountByTable = complains
                .GroupBy(c => c.TableId)
                .ToDictionary(g => g.Key, g => g.Count());

            var statsDict = await _orderStatsQuery.GetOrderStatsByTableIdsAsync(tables.Select(t => t.Id));

            var result = tables.Select(table =>
            {
                pendingCountByTable.TryGetValue(table.Id, out var pendingCount);
                return BuildInfo(table, pendingCount, statsDict);
            }).ToDictionary(x => x.Id.ToString(), x => x);

            return result;
        }

        public async Task<ComplainPeedingInfo?> BuildForTableAsync(Guid tableId)
        {
            // ✅ ưu tiên query đúng 1 table (nếu repo/spec support)
            var table = (await _unitOfWork.Repository<Table, Guid>()
                .GetAllWithSpecAsync(new TableByIdWithOrdersSessionsSpecification(tableId), true))
                .FirstOrDefault();

            if (table == null) return null;

            var pendingComplains = await _unitOfWork.Repository<Complain, Guid>()
                .GetAllWithSpecAsync(new BaseSpecification<Complain>(x => x.isPending && x.TableId == tableId));

            var pendingCount = pendingComplains.Count();

            var statsDict = await _orderStatsQuery.GetOrderStatsByTableIdsAsync(new[] { tableId });

            return BuildInfo(table, pendingCount, statsDict);
        }

        private static OrderStaticsResponse DefaultStats() => new()
        {
            PaymentStatus = 0,
            DeliveredCount = 0,
            ServedCount = 0,
            PaidCount = 0,
            TotalOrderItems = 0
        };

        private static ComplainPeedingInfo BuildInfo(
            Table table,
            int pendingCount,
            Dictionary<Guid, OrderStaticsResponse> statsDict)
        {
            var activeSession = table.Sessions
                .Where(s => s.Status == TableSessionStatus.Active)
                .OrderByDescending(s => s.CheckIn)
                .FirstOrDefault();

            var sessionId = activeSession?.Id.ToString() ?? string.Empty;

            DateTime? lastOrderUpdatedTime = table.Orders != null && table.Orders.Any()
                ? table.Orders
                    .OrderByDescending(o => o.LastUpdatedTime)
                    .Select(o => (DateTime?)o.LastUpdatedTime)
                    .FirstOrDefault()
                : null;

            var stats = DefaultStats();

            if (activeSession != null && statsDict.TryGetValue(table.Id, out var s))
                stats = s;

            if (table.Status == (int)TableEnums.Available && activeSession == null)
            {
                stats = DefaultStats();
                lastOrderUpdatedTime = null;
            }

            var pendingItems = Math.Max(0, stats.TotalOrderItems - stats.ServedCount);

            // ✅ FIX so sánh int enum
            bool isWaitingDish = pendingItems > 0 && table.Status == TableEnums.Occupied;

            int? waitingDurationInMinutes = null;
            if (isWaitingDish && lastOrderUpdatedTime.HasValue)
            {
                waitingDurationInMinutes = (int)Math.Floor((DateTime.UtcNow - lastOrderUpdatedTime.Value).TotalMinutes);
            }

            return new ComplainPeedingInfo(
                Id: table.Id,
                SessionId: sessionId,
                TableName: table.Name,
                tableStatus: table.Status,
                paymentStatus: stats.PaymentStatus,
                Counter: pendingCount,
                DeliveredCount: stats.DeliveredCount,
                ServeredCount: stats.ServedCount,
                PaidCount: stats.PaidCount,
                TotalItems: stats.TotalOrderItems,
                LastOrderUpdatedTime: lastOrderUpdatedTime,
                PendingItems: pendingItems,
                IsWaitingDish: isWaitingDish,
                WaitingDurationInMinutes: waitingDurationInMinutes
            );
        }
    }
}
