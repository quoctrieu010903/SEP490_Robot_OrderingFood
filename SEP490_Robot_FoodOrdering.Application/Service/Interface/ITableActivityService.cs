

using SEP490_Robot_FoodOrdering.Application.DTO.Request;
using SEP490_Robot_FoodOrdering.Application.DTO.Response.TableActivities;
using SEP490_Robot_FoodOrdering.Core.Response;
using SEP490_Robot_FoodOrdering.Domain;
using SEP490_Robot_FoodOrdering.Domain.Entities;
using SEP490_Robot_FoodOrdering.Domain.Enums;

namespace SEP490_Robot_FoodOrdering.Application.Service.Interface
{
    public interface ITableActivityService
    {
        Task LogAsync(TableSession session, string? deviceId,TableActivityType type, object? data = null);
        Task<PaginatedList<TableActivityLogResponse>> GetLogAsync(Guid sessionId , PagingRequestModel pagingrequest);
      }
}

