using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SEP490_Robot_FoodOrdering.Application.Abstractions.Hubs;
using SEP490_Robot_FoodOrdering.Application.DTO.Request.User;
using SEP490_Robot_FoodOrdering.Application.Service.Implementation;
using SEP490_Robot_FoodOrdering.Application.Service.Interface;
using SEP490_Robot_FoodOrdering.Core.Constants;
using SEP490_Robot_FoodOrdering.Domain;
using SEP490_Robot_FoodOrdering.Domain.Entities;
using SEP490_Robot_FoodOrdering.Domain.Enums;
using SEP490_Robot_FoodOrdering.Domain.Interface;
using SEP490_Robot_FoodOrdering.Domain.Specifications;

namespace SEP490_Robot_FoodOrdering.Infrastructure.BackgroundJob
{
    public class TableReleaseBackgroundService : BackgroundService
    {
        private readonly ILogger<TableReleaseBackgroundService> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public TableReleaseBackgroundService(
            IServiceScopeFactory serviceScopeFactory,
            ILogger<TableReleaseBackgroundService> logger )
        {
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Table release background service started.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using var scope = _serviceScopeFactory.CreateScope();

                    var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                    var settingsService = scope.ServiceProvider.GetRequiredService<ISettingsService>();
                    var tableSessionService = scope.ServiceProvider.GetRequiredService<ITableSessionService>();
                    var tableActivityService = scope.ServiceProvider.GetRequiredService<ITableActivityService>();
                    var moderatorHub = scope.ServiceProvider.GetRequiredService<IModeratorDashboardRefresher>();

                    var response = await settingsService.GetByKeyAsync(SystemSettingKeys.TableAccessTimeoutWithoutOrderMinutes);
                    var autoReleaseMinutes = 15;
                    var raw = response?.Data?.Value;
                    if (!string.IsNullOrWhiteSpace(raw) && int.TryParse(raw, out var val) && val > 0)
                        autoReleaseMinutes = val;

                    var spec = new TablesToReleaseSpecification(autoReleaseMinutes);
                    var tables = await unitOfWork.Repository<Table, Guid>().GetAllWithSpecAsync(spec);

                    foreach (var t in tables)
                    {
                        stoppingToken.ThrowIfCancellationRequested();

                        // 2) Lấy session ACTIVE mới nhất của bàn (mới nhất lên đầu)
                        var sessionSpec = new BaseSpecification<TableSession>(s =>
                            s.TableId == t.Id &&
                            s.Status == TableSessionStatus.Active &&
                            s.CheckOut == null &&
                            !s.DeletedTime.HasValue);

                        sessionSpec.AddOrderByDescending(s => s.CheckIn); // ✅ mới nhất

                        var activeSession = await unitOfWork.Repository<TableSession, Guid>()
                            .GetWithSpecAsync(sessionSpec);

                        if (activeSession == null) continue;

                        // 3) Check “không có order” theo session này
                        // Nếu bạn muốn “không có order ACTIVE”, dùng điều kiện status như dưới
                        var hasAnyOrderForSession = await unitOfWork.Repository<Order, Guid>()
                            .AnyAsync(o =>
                                o.TableSessionId == activeSession.Id &&
                                !o.DeletedTime.HasValue &&
                                o.Status != OrderStatus.Completed &&
                                o.Status != OrderStatus.Cancelled);

                        if (hasAnyOrderForSession) continue;

                        // (optional) chặn complain pending
                        var hasPendingComplains = await unitOfWork.Repository<Complain, Guid>()
                            .AnyAsync(c => c.TableId == t.Id && c.isPending && !c.DeletedTime.HasValue);

                        if (hasPendingComplains) continue;

                        // 4) Log activity + CloseSession
                        var reason = "AUTO_RELEASE_NO_ORDER_TIMEOUT";

                        var payload = new
                        {
                            reason,
                            autoReleaseMinutes,
                            thresholdUtc = DateTime.UtcNow.AddMinutes(-autoReleaseMinutes),
                            table = new { id = t.Id, name = t.Name, deviceId = t.DeviceId, lockedAtUtc = t.LockedAt },
                            session = new
                            {
                                id = activeSession.Id,
                                checkInUtc = activeSession.CheckIn,
                                lastActivityAtUtc = activeSession.LastActivityAt
                            }
                        };

                        await tableActivityService.LogAsync(
                            activeSession,
                            deviceId: null, // System
                            TableActivityType.AutoReleaseNoOrderTimeout,
                            payload);

                        await tableSessionService.CloseSessionAsync(
                            activeSession,
                            reason: reason,
                            invoiceId: null,
                            invoiceCode: null,
                            actorDeviceId: null); // System

                        await moderatorHub.PushTableAsync(t.Id,stoppingToken);

                        _logger.LogInformation(
                            "Released table {TableName} (tableId={TableId}) by closing session {SessionId}",
                            t.Name, t.Id, activeSession.Id);
                    }
                  

                    await unitOfWork.SaveChangesAsync(stoppingToken);

                    // ===== AUTO CHECKOUT OVERTIME: Bàn đã thanh toán xong, món đã giao hết, nhưng không gọi thêm món mới trong X phút =====
                    await AutoCheckoutOvertimeTablesAsync(
                        unitOfWork,
                        settingsService,
                        tableSessionService,
                        tableActivityService,
                        moderatorHub,
                        scope.ServiceProvider.GetRequiredService<ITableService>(),
                        stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;  
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in table release background job");
                }

                await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
            }
        }

        /// <summary>
        /// Tự động checkout các bàn có session active, tất cả order đã thanh toán, 
        /// tất cả order items đã giao xong (Served/Completed), và không có món mới trong X phút.
        /// </summary>
        private async Task AutoCheckoutOvertimeTablesAsync(
            IUnitOfWork unitOfWork,
            ISettingsService settingsService,
            ITableSessionService tableSessionService,
            ITableActivityService tableActivityService,
            IModeratorDashboardRefresher moderatorHub,
            ITableService tableService,
            CancellationToken stoppingToken)
        {
            try
            {
                // Đọc setting MinuteOvertime (mặc định 120 phút)
                var response = await settingsService.GetByKeyAsync(SystemSettingKeys.MinuteOvertime);
                var minuteOvertime = 120;
                var raw = response?.Data?.Value;
                if (!string.IsNullOrWhiteSpace(raw) && int.TryParse(raw, out var val) && val > 0)
                    minuteOvertime = val;

                var thresholdUtc = DateTime.UtcNow.AddMinutes(-minuteOvertime);

                // Lấy tất cả active sessions
                var activeSessionsSpec = new BaseSpecification<TableSession>(s =>
                    s.Status == TableSessionStatus.Active &&
                    s.CheckOut == null &&
                    !s.DeletedTime.HasValue);

                var activeSessions = await unitOfWork.Repository<TableSession, Guid>()
                    .GetAllWithSpecAsync(activeSessionsSpec);

                foreach (var session in activeSessions)
                {
                    stoppingToken.ThrowIfCancellationRequested();

                    // Lấy thông tin bàn
                    var table = await unitOfWork.Repository<Table, Guid>().GetByIdAsync(session.TableId);
                    if (table == null || table.Status != TableEnums.Occupied)
                        continue;

                    // Lấy tất cả orders của session này (include OrderItems)
                    var ordersSpec = new BaseSpecification<Order>(o =>
                        o.TableSessionId == session.Id &&
                        !o.DeletedTime.HasValue);

                    var orders = await unitOfWork.Repository<Order, Guid>()
                        .GetAllWithSpecWithInclueAsync(ordersSpec, true, o => o.OrderItems);

                    // Nếu không có order nào -> bỏ qua (case này đã xử lý ở AutoReleaseNoOrderTimeout)
                    if (!orders.Any())
                        continue;

                    // Kiểm tra: Tất cả orders phải đã thanh toán (Paid)
                    var allOrdersPaid = orders.All(o => o.PaymentStatus == PaymentStatusEnums.Paid);
                    if (!allOrdersPaid)
                        continue;

                    // Kiểm tra: Tất cả order items phải đã giao xong (Served, Completed, hoặc Cancelled)
                    var allOrderItems = orders.SelectMany(o => o.OrderItems).ToList();
                    if (!allOrderItems.Any())
                        continue;

                    var deliveredStatuses = new[]
                    {
                        OrderItemStatus.Served,
                        OrderItemStatus.Completed,
                        OrderItemStatus.Cancelled
                    };

                    var allItemsDelivered = allOrderItems.All(oi => deliveredStatuses.Contains(oi.Status));
                    if (!allItemsDelivered)
                        continue;

                    // Kiểm tra: OrderItem/Order mới nhất phải được tạo trước thresholdUtc (quá X phút không gọi thêm món)
                    var lastOrderItemCreatedTime = allOrderItems.Max(oi => oi.CreatedTime);
                    var lastOrderCreatedTime = orders.Max(o => o.CreatedTime);
                    var lastActivityTime = lastOrderItemCreatedTime > lastOrderCreatedTime ? lastOrderItemCreatedTime : lastOrderCreatedTime;

                    if (lastActivityTime >= thresholdUtc)
                        continue; // Vẫn còn hoạt động trong thời gian cho phép

                    // Kiểm tra có pending complains không
                    var hasPendingComplains = await unitOfWork.Repository<Complain, Guid>()
                        .AnyAsync(c => c.TableId == table.Id && c.isPending && !c.DeletedTime.HasValue);

                    if (hasPendingComplains)
                        continue;

                    // ===== Tất cả điều kiện thỏa mãn -> Auto Checkout =====
                    var reason = "AUTO_CHECKOUT_OVERTIME_NO_NEW_ORDER";

                    var payload = new
                    {
                        reason,
                        minuteOvertime,
                        thresholdUtc = thresholdUtc.ToString("O"),
                        lastActivityTimeUtc = lastActivityTime.ToString("O"),
                        table = new
                        {
                            id = table.Id,
                            name = table.Name,
                            deviceId = table.DeviceId
                        },
                        session = new
                        {
                            id = session.Id,
                            checkInUtc = session.CheckIn.ToString("O"),
                            lastActivityAtUtc = session.LastActivityAt?.ToString("O")
                        },
                        orders = orders.Select(o => new
                        {
                            id = o.Id,
                            orderCode = o.OrderCode,
                            paymentStatus = o.PaymentStatus.ToString(),
                            itemCount = o.OrderItems.Count
                        }).ToList()
                    };

                    // Log activity
                    await tableActivityService.LogAsync(
                        session,
                        deviceId: null, // System
                        TableActivityType.AutoCheckoutOvertimeNoNewOrder,
                        payload);

                    // Gọi API Checkout cho bàn
                    try
                    {
                        var checkoutRequest = new CheckoutTableRequest();
                        await tableService.CheckoutTable(table.Id, checkoutRequest);

                        _logger.LogInformation(
                            "AutoCheckoutOvertime: Successfully checked out table {TableName} (tableId={TableId}), " +
                            "session {SessionId}. Last activity was at {LastActivityTime} UTC, " +
                            "overtime threshold: {MinuteOvertime} minutes.",
                            table.Name, table.Id, session.Id, lastActivityTime, minuteOvertime);
                    }
                    catch (Exception checkoutEx)
                    {
                        // Nếu checkout thất bại (ví dụ: order chưa đủ điều kiện), log và bỏ qua
                        _logger.LogWarning(checkoutEx,
                            "AutoCheckoutOvertime: Failed to checkout table {TableName} (tableId={TableId}). " +
                            "Will close session instead.",
                            table.Name, table.Id);

                        // Fallback: Close session nếu không checkout được
                        await tableSessionService.CloseSessionAsync(
                            session,
                            reason: reason,
                            invoiceId: null,
                            invoiceCode: null,
                            actorDeviceId: null);
                    }

                    await moderatorHub.PushTableAsync(table.Id, stoppingToken);
                }

                await unitOfWork.SaveChangesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in AutoCheckoutOvertimeTablesAsync");
            }
        }
    }
}
