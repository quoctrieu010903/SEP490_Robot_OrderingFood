using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SEP490_Robot_FoodOrdering.Application.DTO.Fillter;
using SEP490_Robot_FoodOrdering.Application.DTO.Request;
using SEP490_Robot_FoodOrdering.Application.DTO.Response.Product;
using SEP490_Robot_FoodOrdering.Core.Response;
using SEP490_Robot_FoodOrdering.Domain;

namespace SEP490_Robot_FoodOrdering.Application.Service.Interface
{
    public interface IProductService
    {
        Task<BaseResponseModel> Create(CreateProductRequest request);
        Task<BaseResponseModel<ProductResponse>> Update(CreateProductRequest request, Guid id);
        Task<BaseResponseModel<ProductDetailResponse>> GetById(Guid id);
        Task<PaginatedList<ProductResponse>> GetAll(PagingRequestModel paging, ProductFillterResquest fillter);
        Task<BaseResponseModel> Delete(Guid id);
    
    }
}
