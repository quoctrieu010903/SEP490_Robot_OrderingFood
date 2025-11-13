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
        Task<BaseResponseModel<TableResponse>> CheckoutTable(Guid id); 
        Task<BaseResponseModel<TableResponse>> TransferTableAsync(Guid tableId, Guid transferToUserId, string? reason = null, string transferredBy = "System");
        Task<BaseResponseModel<QrShareResponse>> ShareTableAsync(Guid tableId, string CurrentDevideid);
        Task<BaseResponseModel<TableResponse>> AcceptSharedTableAsync(Guid tableId, string shareToken, string newDeviceId);
        Task<BaseResponseModel<TableResponse>> ScanQrCode01(Guid id, string deviceId);
        Task<BaseResponseModel<TableResponse>> MoveTable(Guid oldTableId, MoveTableRequest request);
        Task<BaseResponseModel<CheckDeviceTokenResponse>> CheckTableAndDeviceToken(Guid tableId, string deviceId);

    }
} 