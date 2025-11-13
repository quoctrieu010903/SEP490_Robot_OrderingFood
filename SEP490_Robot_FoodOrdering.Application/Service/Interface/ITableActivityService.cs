

using SEP490_Robot_FoodOrdering.Application.DTO.Response.TableActivities;
using SEP490_Robot_FoodOrdering.Domain.Entities;
using SEP490_Robot_FoodOrdering.Domain.Enums;

namespace SEP490_Robot_FoodOrdering.Application.Service.Interface
{
    public interface ITableActivityService
    {
        Task LogAsync(TableSession session, string? deviceId,TableActivityType type, object? data = null);
        Task<List<TableActivityLogResponse>> GetLogAsync(Guid sessionId);
    }
}
