using SEP490_Robot_FoodOrdering.Application.DTO.Request;
using SEP490_Robot_FoodOrdering.Application.DTO.Response.Topping;
using SEP490_Robot_FoodOrdering.Core.Response;
using SEP490_Robot_FoodOrdering.Domain;

namespace SEP490_Robot_FoodOrdering.Application.Service.Interface
{
    public interface IToppingService
    {
        Task<BaseResponseModel> Create(CreateToppingRequest request);
        Task<BaseResponseModel> Delete(Guid id);
        Task<PaginatedList<ToppingResponse>> GetAllToppingsAsync(PagingRequestModel paging);
        Task<BaseResponseModel> Update(CreateToppingRequest request, Guid id);
        Task<BaseResponseModel<ToppingResponse>> GetByIdAsync(Guid id);

        Task<BaseResponseModel<List<ToppingResponse>>> GetByIdProduction(Guid id);
    }
}