using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using SEP490_Robot_FoodOrdering.Application.DTO.Request;
using SEP490_Robot_FoodOrdering.Application.DTO.Response.Table;
using SEP490_Robot_FoodOrdering.Core.Response;
using SEP490_Robot_FoodOrdering.Domain;
using SEP490_Robot_FoodOrdering.Domain.Enums;

namespace SEP490_Robot_FoodOrdering.Application.Service.Interface
{
    public interface ITableService
    {
        Task<BaseResponseModel> Create(CreateTableRequest request);
        Task<BaseResponseModel> Delete(Guid id);
        Task<PaginatedList<TableResponse>> GetAll(PagingRequestModel paging, TableEnums? status, string? tableName);
        Task<TableResponse> GetById(Guid id);
        Task<BaseResponseModel> Update(UpdateStatusTable request, Guid id);
        Task<BaseResponseModel<TableResponse>> ScanQrCode(Guid id, string DevidedId);
        Task<TableResponse> ChangeTableStatus(Guid tableId, TableEnums newStatus, string? reason = null, string updatedBy = "System");
    }
} 