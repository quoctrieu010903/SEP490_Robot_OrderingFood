using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using SEP490_Robot_FoodOrdering.Application.DTO.Request;
using SEP490_Robot_FoodOrdering.Application.DTO.Response.Product;
using SEP490_Robot_FoodOrdering.Core.Response;
using SEP490_Robot_FoodOrdering.Domain;

namespace SEP490_Robot_FoodOrdering.Application.Service.Interface
{
    public interface IProductSizeService
    {
        Task<BaseResponseModel> Create(CreateProductSizeRequest request);
        Task<BaseResponseModel> Delete(Guid id);
        Task<PaginatedList<ProductSizeResponse>> GetAll(PagingRequestModel paging);
        Task<ProductSizeResponse> GetById(Guid id);
        Task<BaseResponseModel> Update(CreateProductSizeRequest request, Guid id);
    }
} 