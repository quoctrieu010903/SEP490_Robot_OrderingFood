using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SEP490_Robot_FoodOrdering.Core.Response;

namespace SEP490_Robot_FoodOrdering.Application.Service.Interface
{
    public interface IProductCategoryService
    {
        
        Task<BaseResponseModel> CreateProductCategoryAsync(Guid productId, Guid categoryId);
       
        Task<BaseResponseModel> DeleteProductCategoryAsync(Guid id);
    }
}
