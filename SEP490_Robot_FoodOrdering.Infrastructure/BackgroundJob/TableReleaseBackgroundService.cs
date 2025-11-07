using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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
            ILogger<TableReleaseBackgroundService> logger)
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
                    using (var scope = _serviceScopeFactory.CreateScope())
                    {
                        var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                        var settingsService = scope.ServiceProvider.GetRequiredService<ISettingsService>();

                        // 1️⃣ Lấy setting: thời gian auto release
                        var response = await settingsService.GetByKeyAsync(SystemSettingKeys.TableAccessTimeoutWithoutOrderMinutes);
                        int autoReleaseMinutes = 15;
                        if (response.Data != null && int.TryParse(response.Data.Value, out var val))
                        {
                            autoReleaseMinutes = val;
                        }

                        // 2️⃣ Lấy danh sách bàn đang occupied nhưng chưa có order
                        var spec = new TablesToReleaseSpecification(autoReleaseMinutes);
                        var tables = await unitOfWork.Repository<Table, Guid>().GetAllWithSpecAsync(spec);

                        // 3️⃣ Release bàn
                        foreach (var t in tables)
                        {
                            if (!t.Orders.Any())
                            {
                                t.Status = TableEnums.Available;
                                t.LastUpdatedTime = DateTime.UtcNow;
                                t.LockedAt = null;
                                t.DeviceId = null;
                                t.LockedAt = null;
                                t.IsQrLocked = false;
                                unitOfWork.Repository<Table, Guid>().Update(t);
                                _logger.LogInformation("Released table {TableId} due to inactivity", t.Id);
                            }
                        }

                        await unitOfWork.SaveChangesAsync();
                    }

                    // 4️⃣ Chạy lại sau 1 phút
                    await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in table release background job");
                }
            }
        }
    }
}
