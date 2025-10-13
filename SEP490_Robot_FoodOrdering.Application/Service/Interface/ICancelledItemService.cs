using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SEP490_Robot_FoodOrdering.Application.DTO.Fillter;
using SEP490_Robot_FoodOrdering.Application.DTO.Response.CancelledItem;
using SEP490_Robot_FoodOrdering.Core.Response;

namespace SEP490_Robot_FoodOrdering.Application.Service.Interface
{
    public interface ICancelledItemService
    {
        public Task<bool> CreateCancelledItemAsync(Guid orderItemId, string? cancelNote, Guid cancelledByUserId);
        public Task<BaseResponseModel<CancelledItemResponse>> getAllCancelledItems(CancelledItemFilterRequestParam request);
    }
}
