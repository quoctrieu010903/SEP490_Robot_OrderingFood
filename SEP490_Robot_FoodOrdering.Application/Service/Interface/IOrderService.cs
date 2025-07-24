using SEP490_Robot_FoodOrdering.Application.DTO.Response.Order;
using SEP490_Robot_FoodOrdering.Core.Response;
using SEP490_Robot_FoodOrdering.Domain.Entities;

namespace SEP490_Robot_FoodOrdering.Application.Service.Implementation;

public interface IOrderService
{
    Task<BaseResponseModel<InforBill>> CreateBill(Guid idOrder);
}