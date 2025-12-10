using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using SEP490_Robot_FoodOrdering.Application.Abstractions.Hubs;
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

                        _logger.LogInformation(
                            "Released table {TableName} (tableId={TableId}) by closing session {SessionId}",
                            t.Name, t.Id, activeSession.Id);
                    }
                  

                    await unitOfWork.SaveChangesAsync(stoppingToken);
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
    }
}
