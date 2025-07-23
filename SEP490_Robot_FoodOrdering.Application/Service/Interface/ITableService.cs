using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using SEP490_Robot_FoodOrdering.Application.DTO.Request;
using SEP490_Robot_FoodOrdering.Application.DTO.Response.Table;
using SEP490_Robot_FoodOrdering.Core.Response;
using SEP490_Robot_FoodOrdering.Domain;

namespace SEP490_Robot_FoodOrdering.Application.Service.Interface
{
    public interface ITableService
    {
        Task<BaseResponseModel> Create(CreateTableRequest request);
        Task<BaseResponseModel> Delete(Guid id);
        Task<PaginatedList<TableResponse>> GetAll(PagingRequestModel paging);
        Task<TableResponse> GetById(Guid id);
        Task<BaseResponseModel> Update(CreateTableRequest request, Guid id);
    }
} 