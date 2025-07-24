using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SEP490_Robot_FoodOrdering.Application.DTO.Request;
using SEP490_Robot_FoodOrdering.Application.DTO.Response.Product;
using SEP490_Robot_FoodOrdering.Core.Response;
using SEP490_Robot_FoodOrdering.Domain;

namespace SEP490_Robot_FoodOrdering.Application.Service.Interface
{
    public interface IProductCategoryService
    {
        
        Task<BaseResponseModel> CreateProductCategoryAsync(Guid productId, Guid categoryId);

        Task<PaginatedList<ProductCategoryResponse>> GetAllProductCategoriesAsync(PagingRequestModel paging);
        Task<BaseResponseModel> DeleteProductCategoryAsync(Guid id);
    }
}
