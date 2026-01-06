using Quartz;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SEP490_Robot_FoodOrdering.Domain.Enums;
using SEP490_Robot_FoodOrdering.Domain.Interface;
using SEP490_Robot_FoodOrdering.Application.Service.Interface;
using SEP490_Robot_FoodOrdering.Core.Constants;
using SEP490_Robot_FoodOrdering.Domain.Entities;
using SEP490_Robot_FoodOrdering.Domain;
using System.Threading;
using static QRCoder.PayloadGenerator;
using SEP490_Robot_FoodOrdering.Application.Abstractions.Hubs;

public class DailyCleanupJob : IJob
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISettingsService _systemSettingService;
    private readonly ICancelledItemService _cancelledItemService;
    private readonly ILogger<DailyCleanupJob> _logger;
    private readonly ITableSessionService _tableSessionService;
    private readonly ITableActivityService _tableActivityService;
    private readonly IModeratorDashboardRefresher _moderatorHub;
    private const string SystemUserId = "b9abf60c-9c0e-4246-a846-d9ab62303b13";

    public DailyCleanupJob(
        IUnitOfWork unitOfWork,
        ISettingsService systemSettingService,
        ICancelledItemService cancelItemService,
        ITableSessionService tableSessionService,
        ITableActivityService tableActivityService,
        IModeratorDashboardRefresher moderatorHub,
        ILogger<DailyCleanupJob> logger)
    {
        _unitOfWork = unitOfWork;
        _systemSettingService = systemSettingService;
        _logger = logger;
        _cancelledItemService = cancelItemService;
        _tableSessionService = tableSessionService;
        _tableActivityService = tableActivityService;
        _moderatorHub = moderatorHub;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var ct = context.CancellationToken;
        _logger.LogInformation("DailyCleanupJob started at {Time}", DateTimeOffset.UtcNow);

        try
        {
            // 1) Đọc số ngày cleanup từ SystemSettings (OrderCleanupAfterDays)
            var days = await _systemSettingService
                .GetByKeyAsync(SystemSettingKeys.OrderCleanupAfterDays); 
            var cleanupDays = 1; // default = 1 ngày (qua ngày hôm sau)
            if (!string.IsNullOrWhiteSpace(days.Data.Value)
           && int.TryParse(days.Data.Value, out var parsedDays))
            {
                cleanupDays = parsedDays;
            }
        var thresholdLocal = DateTime.Today.AddDays(-cleanupDays);
            var thresholdUtc = thresholdLocal.ToUniversalTime();
            // 2) Auto-cancel các OrderItem đang Pending quá thresholdDate
            await AutoCancelPendingItems(thresholdUtc, ct);

            // 3) Auto xử lý Complain cũ (đánh dấu đã xử lý)
            await AutoProcessOldComplain(thresholdUtc, ct);

            // 4) Auto close feedback cũ
            //await AutoCloseOldFeedback(thresholdDate, ct);

            //5) Giải phóng bàn không còn order/ complain active
             await AutoReleaseTables(thresholdUtc,ct);
            
            //6) Áp dụng PaymentPolicyPending nếu đã đến thời điểm
            await ApplyPendingPaymentPolicy(ct);

            await _unitOfWork.SaveChangesAsync(ct);

            _logger.LogInformation("DailyCleanupJob finished at {Time}", DateTimeOffset.UtcNow);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in DailyCleanupJob");
            throw;
        }
    }

    private async Task AutoReleaseTables(DateTime thresholdDate, CancellationToken ct)
    {
        var tables = await _unitOfWork.Repository<Table, Guid>()
            .GetAllWithSpecWithInclueAsync(
                new BaseSpecification<Table>(t => t.Status == TableEnums.Occupied),
                true,
                t => t.Orders,
                t => t.Complains);

        _logger.LogInformation("DailyCleanupJob: found {Count} occupied tables to check for release", tables.Count());
        var releasedTableIds = new List<Guid>();
        foreach (var table in tables)
        {
            ct.ThrowIfCancellationRequested();

            // 1) orders đang mở
            var openOrders = table.Orders
                .Where(o => o.Status != OrderStatus.Completed && o.Status != OrderStatus.Cancelled)
                .ToList();

            var hasOpenOrders = openOrders.Any();

            // 2) complains pending
            var pendingComplains = table.Complains
                .Where(c => c.isPending)
                .ToList();

            var hasPendingComplains = pendingComplains.Any();

            // 3) activity gần đây (>= thresholdDate)
            var hasRecentActivity = table.Orders.Any(o =>
                o.CreatedTime >= thresholdDate ||
                (o.LastUpdatedTime >= thresholdDate));

            _logger.LogInformation(
                "DailyCleanupJob: table {TableName} - openOrders={HasOpenOrders}, pendingComplains={HasPendingComplains}, recentActivity={HasRecentActivity}",
                table.Name, hasOpenOrders, hasPendingComplains, hasRecentActivity);

            if (hasOpenOrders || hasPendingComplains || hasRecentActivity)
                continue;

            // 4) active session theo TableId
            var activeSession = await _unitOfWork.Repository<TableSession, Guid>()
                .GetWithSpecAsync(new BaseSpecification<TableSession>(
                    ts => ts.TableId == table.Id && ts.Status == TableSessionStatus.Active));

            if (activeSession == null)
            {
                _logger.LogWarning("DailyCleanupJob: table {TableName} has no active session -> skip", table.Name);
                continue;
            }

            // 5) Build payload log activity (theo threshold)
            var payload = BuildAutoReleasePayload(
                table,
                activeSession,
                thresholdDate,
                openOrders,
                pendingComplains);

            // ✅ Khuyên dùng type riêng để khỏi trùng log CloseSession
            await _tableActivityService.LogAsync(
                activeSession,
                deviceId: null, // System
                TableActivityType.AutoReleaseAfterMidnight, // tạo type riêng (khuyên)
                payload);

            // 6) Close session
            await _tableSessionService.CloseSessionAsync(
                activeSession,
                reason: "AUTO_RELEASE_NO_ACTIVITY_OVER_THRESHOLD",
                invoiceId: null,
                invoiceCode: null,
                actorDeviceId: null);
            // 7) Gửi hub refresh dashboard moderator
            releasedTableIds.Add(table.Id);

            _logger.LogInformation(
                "DailyCleanupJob: released table {TableName} by closing session {SessionId}",
                table.Name, activeSession.Id);
        }
        foreach (var tableId in releasedTableIds)
        {
            await _moderatorHub.PushTableAsync(tableId, ct);
        }

    }

    private object BuildAutoReleasePayload(
        Table table,
        TableSession session,
        DateTime thresholdUtc,
        IReadOnlyList<Order> openOrders,
        IReadOnlyList<Complain> pendingComplains)
    {
        return new
        {
            reason = "AUTO_RELEASE_NO_ACTIVITY_OVER_THRESHOLD",
            thresholdUtc = thresholdUtc.ToString("O"),
            table = new
            {
                id = table.Id,
                name = table.Name,
                statusBefore = table.Status.ToString(),
                statusAfter = TableEnums.Available.ToString()
            },
            session = new
            {
                id = session.Id,
                checkInUtc = session.CheckIn.ToString("O"),
                lastActivityAtUtc = session.LastActivityAt?.ToString("O")
            },
            checks = new
            {
                openOrdersCount = openOrders.Count,
                pendingComplainsCount = pendingComplains.Count,
                openOrderIds = openOrders.Select(o => o.Id).ToList(),
                pendingComplainIds = pendingComplains.Select(c => c.Id).ToList()
            },
            job = new
            {
                name = "DailyCleanupJob",
                runAtUtc = DateTime.UtcNow.ToString("O")
            }
        };
    }

    private async Task AutoCancelPendingItems(DateTime thresholdDate, CancellationToken ct)
    {
        var pendingItems = await _unitOfWork.Repository<OrderItem, Guid>()
            .GetAllWithSpecWithInclueAsync(new BaseSpecification<OrderItem>(oi =>
                oi.Status == OrderItemStatus.Pending &&
                oi.CreatedTime <= thresholdDate), true, oi => oi.Order);
                    

        _logger.LogInformation("DailyCleanupJob: found {Count} pending items to auto cancel", pendingItems.Count());

        foreach (var item in pendingItems)
        {
           _cancelledItemService.CreateCancelledItemAsync(
                item.Id,
                "Auto-cancelled by system daily cleanup job",
                Guid.Parse(SystemUserId)).Wait(ct);
        }

        // Recalculate total cho các Order bị ảnh hưởng
        var affectedOrders = pendingItems
            .Select(oi => oi.Order)
            .Where(o => o != null)
            .Distinct()
            .ToList();

        foreach (var order in affectedOrders)
        {
            var activeItems = order.OrderItems
                .Where(oi => oi.Status != OrderItemStatus.Cancelled)
                .ToList();

            if (!activeItems.Any())
            {
                // Nếu không còn món nào active → auto cancel order luôn (tuỳ enum của em)
                order.Status = OrderStatus.Cancelled;
                order.PaymentStatus = PaymentStatusEnums.None; // nếu có
            }
            else
            {
                order.TotalPrice = (decimal) activeItems.Sum(oi => oi.TotalPrice);
            }
        }
    }


    private async Task AutoProcessOldComplain(DateTime thresholdDate, CancellationToken ct)
    {
        var feedbacks = await _unitOfWork.Repository<Complain, Guid>()
            .GetAllWithSpecAsync(new BaseSpecification<Complain>(f =>
                f.CreatedTime <= thresholdDate &&
                f.isPending), true);

        _logger.LogInformation("DailyCleanupJob: found {Count} feedbacks to auto archive", feedbacks.Count());

        foreach (var fb in feedbacks)
        {
            fb.isPending = false;
            fb.ResolutionNote = "Auto-processed by system daily cleanup job";
            fb.LastUpdatedTime = DateTime.UtcNow;
            fb.HandledBy = Guid.Parse(SystemUserId);
            fb.ResolvedAt = DateTime.UtcNow;


        }
      }

        private async Task ApplyPendingPaymentPolicy(CancellationToken ct)
        {
            try
            {
                var pendingSetting = await _unitOfWork.Repository<SystemSettings, Guid>()
                    .GetWithSpecAsync(new BaseSpecification<SystemSettings>(s => 
                        s.Key == SystemSettingKeys.PaymentPolicyPending && !s.DeletedTime.HasValue));

                if (pendingSetting == null)
                {
                    _logger.LogDebug("No pending payment policy to apply");
                    return;
                }

                var effectiveDateSetting = await _unitOfWork.Repository<SystemSettings, Guid>()
                    .GetWithSpecAsync(new BaseSpecification<SystemSettings>(s => 
                        s.Key == SystemSettingKeys.PaymentPolicyEffectiveDate && !s.DeletedTime.HasValue));

                if (effectiveDateSetting == null)
                {
                    _logger.LogWarning("PaymentPolicyPending exists but PaymentPolicyEffectiveDate is missing");
                    return;
                }

                // Parse effective date
                if (!DateTime.TryParse(effectiveDateSetting.Value, out var effectiveDateUtc))
                {
                    _logger.LogWarning("Invalid PaymentPolicyEffectiveDate format: {Value}", effectiveDateSetting.Value);
                    return;
                }

                // Kiểm tra xem đã đến thời điểm áp dụng chưa
                var nowUtc = DateTime.UtcNow;
                if (nowUtc < effectiveDateUtc)
                {
                    var vnTz = TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh");
                    var effectiveDateVn = TimeZoneInfo.ConvertTimeFromUtc(effectiveDateUtc, vnTz);
                    _logger.LogDebug(
                        "PaymentPolicyPending not yet effective. Will apply at {EffectiveDateVn} (VN time). Current: {NowVn}",
                        effectiveDateVn, TimeZoneInfo.ConvertTimeFromUtc(nowUtc, vnTz));
                    return;
                }

                // Áp dụng PaymentPolicyPending vào PaymentPolicy
                var currentPolicySetting = await _unitOfWork.Repository<SystemSettings, Guid>()
                    .GetWithSpecAsync(new BaseSpecification<SystemSettings>(s => 
                        s.Key == SystemSettingKeys.PaymentPolicy && !s.DeletedTime.HasValue));

                if (currentPolicySetting == null)
                {
                    // Tạo mới nếu chưa có
                    currentPolicySetting = new SystemSettings
                    {
                        Id = Guid.NewGuid(),
                        Key = SystemSettingKeys.PaymentPolicy,
                        Value = pendingSetting.Value,
                        Type = SettingType.String,
                        DisplayName = "Chính sách thanh toán",
                        Description = "Prepay = thanh toán trước, Postpay = thanh toán sau",
                        CreatedTime = DateTime.UtcNow,
                        LastUpdatedTime = DateTime.UtcNow
                    };
                    await _unitOfWork.Repository<SystemSettings, Guid>().AddAsync(currentPolicySetting);
                }
                else
                {
                    var oldValue = currentPolicySetting.Value;
                    currentPolicySetting.Value = pendingSetting.Value;
                    currentPolicySetting.LastUpdatedTime = DateTime.UtcNow;
                    _unitOfWork.Repository<SystemSettings, Guid>().Update(currentPolicySetting);
                    
                    _logger.LogInformation(
                        "Payment policy updated from {OldPolicy} to {NewPolicy}",
                        oldValue, pendingSetting.Value);
                }

                // Xóa PaymentPolicyPending và PaymentPolicyEffectiveDate sau khi đã áp dụng
                pendingSetting.DeletedTime = DateTime.UtcNow;
                effectiveDateSetting.DeletedTime = DateTime.UtcNow;
                _unitOfWork.Repository<SystemSettings, Guid>().Update(pendingSetting);
                _unitOfWork.Repository<SystemSettings, Guid>().Update(effectiveDateSetting);

                _logger.LogInformation(
                    "Successfully applied pending payment policy: {Policy}",
                    pendingSetting.Value);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error applying pending payment policy");
                // Không throw để không làm gián đoạn các job khác
            }
        }

   

}
