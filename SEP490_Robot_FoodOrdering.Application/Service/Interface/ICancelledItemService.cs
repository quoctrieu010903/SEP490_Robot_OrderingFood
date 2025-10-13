
using SEP490_Robot_FoodOrdering.Application.DTO.Request;
using SEP490_Robot_FoodOrdering.Application.DTO.Response.CancelledItem;
using SEP490_Robot_FoodOrdering.Core.Response;
using SEP490_Robot_FoodOrdering.Domain;
using SEP490_Robot_FoodOrdering.Domain.Specifications.Params;

namespace SEP490_Robot_FoodOrdering.Application.Service.Interface
{
    public interface ICancelledItemService
    {
        public Task<bool> CreateCancelledItemAsync(Guid orderItemId, string? cancelNote, Guid cancelledByUserId);
        public Task<PaginatedList<CancelledItemResponse>> getAllCancelledItems(CancelledItemFilterRequestParam request);
    }
}
