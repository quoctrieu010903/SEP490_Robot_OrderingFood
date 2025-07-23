using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using SEP490_Robot_FoodOrdering.Application.DTO.Request;
using SEP490_Robot_FoodOrdering.Application.DTO.Response.Product;
using SEP490_Robot_FoodOrdering.Core.Response;
using SEP490_Robot_FoodOrdering.Domain;

namespace SEP490_Robot_FoodOrdering.Application.Service.Interface
{
    public interface IProductToppingService
    {
        Task<BaseResponseModel> Create(CreateProductToppingRequest request);
        Task<BaseResponseModel> Delete(Guid id);
        Task<PaginatedList<ProductToppingResponse>> GetAll(PagingRequestModel paging);
        Task<ProductToppingResponse> GetById(Guid id);
        Task<BaseResponseModel> Update(CreateProductToppingRequest request, Guid id);
    }
} 