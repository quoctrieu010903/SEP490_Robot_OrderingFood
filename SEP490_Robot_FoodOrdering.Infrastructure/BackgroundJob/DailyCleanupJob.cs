using Quartz;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SEP490_Robot_FoodOrdering.Domain.Enums;
using SEP490_Robot_FoodOrdering.Domain.Interface;
using SEP490_Robot_FoodOrdering.Application.Service.Interface;
using SEP490_Robot_FoodOrdering.Core.Constants;
using SEP490_Robot_FoodOrdering.Domain.Entities;
using SEP490_Robot_FoodOrdering.Domain;

public class DailyCleanupJob : IJob
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ISettingsService _systemSettingService;
    private readonly ICancelledItemService _cancelledItemService;
    private readonly ILogger<DailyCleanupJob> _logger;
    private const string SystemUserId = "44444444-4444-4444-4444-444444444444";

    public DailyCleanupJob(
        IUnitOfWork unitOfWork,
        ISettingsService systemSettingService,
        ICancelledItemService cancelItemService,
        ILogger<DailyCleanupJob> logger)
    {
        _unitOfWork = unitOfWork;
        _systemSettingService = systemSettingService;
        _logger = logger;
        _cancelledItemService = cancelItemService;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        var ct = context.CancellationToken;
        _logger.LogInformation("DailyCleanupJob started at {Time}", DateTimeOffset.UtcNow);

        try
        {
            // 1) Đọc số ngày cleanup từ SystemSettings (OrderCleanupAfterDays)
            var days = await _systemSettingService
                .GetByKeyAsync(SystemSettingKeys.OrderCleanupAfterDays); // default = 1 ngày (ngày hôm sau)
            var cleanupDays = 1; // default = 1 ngày (qua ngày hôm sau)
            if (!string.IsNullOrWhiteSpace(days.Data.Value)
           && int.TryParse(days.Data.Value, out var parsedDays))
            {
                cleanupDays = parsedDays;
            }
            var thresholdDate = DateTime.Today.AddDays(-cleanupDays);

            // 2) Auto-cancel các OrderItem đang Pending quá thresholdDate
            await AutoCancelPendingItems(thresholdDate, ct);

            // 3) Auto xử lý Complain cũ (đánh dấu đã xử lý)
            await AutoProcessOldComplain(thresholdDate, ct);

            // 4) Auto close feedback cũ
            //await AutoCloseOldFeedback(thresholdDate, ct);

            //5) Giải phóng bàn không còn order/ complain active
           await AutoReleaseTables(ct);

            await _unitOfWork.SaveChangesAsync(ct);

            _logger.LogInformation("DailyCleanupJob finished at {Time}", DateTimeOffset.UtcNow);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error in DailyCleanupJob");
            throw;
        }
    }

    private async Task AutoReleaseTables(CancellationToken ct)
    {
        // Lấy tất cả các bàn đang occupied
        var tables = _unitOfWork.Repository<Table, Guid>()
            .GetAllWithSpecWithInclueAsync(
                new BaseSpecification<Table>(t => t.Status == TableEnums.Occupied),
                true,
                t => t.Orders,
                t => t.Complains).Result;
        _logger.LogInformation("DailyCleanupJob: found {Count} occupied tables to check for release", tables.Count());
        foreach (var table in tables)
        {
            var hasActiveOrders = table.Orders.Any(o =>
                o.Status != OrderStatus.Completed &&
                o.Status != OrderStatus.Cancelled);
            var hasActiveComplains = table.Complains.Any(c => c.isPending);
            if (!hasActiveOrders && !hasActiveComplains)
            {
                table.Status = TableEnums.Available;
                table.LastUpdatedTime = DateTime.UtcNow;
                table.LastAccessedAt = null;
                table.LockedAt = null;
                table.DeviceId = null;
                table.IsQrLocked = false;
                _logger.LogInformation("DailyCleanupJob: released table {TableName} as it has no active orders or complains", table.Name);
            }
        }
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

    }
