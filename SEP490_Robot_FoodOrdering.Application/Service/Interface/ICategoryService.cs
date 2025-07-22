
using SEP490_Robot_FoodOrdering.Application.DTO.Request;
using SEP490_Robot_FoodOrdering.Application.DTO.Request.Category;
using SEP490_Robot_FoodOrdering.Application.DTO.Response.Category;
using SEP490_Robot_FoodOrdering.Core.Response;
using SEP490_Robot_FoodOrdering.Domain;

namespace SEP490_Robot_FoodOrdering.Application.Service.Interface
{
    public interface ICategoryService
    {
        Task<PaginatedList<CategoryResponse>> GetAllCategory(PagingRequestModel model);
        Task<BaseResponseModel<CategoryResponse>> GetCategoryById(Guid id);
        Task<BaseResponseModel> Create(CreateCategoryRequest request);
        Task<BaseResponseModel> Update(CreateCategoryRequest request, Guid id);
        Task<BaseResponseModel> Delete(Guid id);
    }
}
