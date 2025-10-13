using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SEP490_Robot_FoodOrdering.Application.DTO.Response.Order;
using SEP490_Robot_FoodOrdering.Core.Response;
using SEP490_Robot_FoodOrdering.Domain;

namespace SEP490_Robot_FoodOrdering.Application.Service.Interface
{
    public interface IRemakeItemService
    {
        Task<bool> CreateRemakeItemAsync(Guid orderItemId, string? remakeNote, Guid remakedByUserId);
        Task<PaginatedList<RemakeOrderItemsResponse>> GetAllRemakeItemsAsync();

    }
}
