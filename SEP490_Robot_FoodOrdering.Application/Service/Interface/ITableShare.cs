using System;
using System.Threading.Tasks;
using SEP490_Robot_FoodOrdering.Application.DTO.Request;
using SEP490_Robot_FoodOrdering.Application.DTO.Response.Table;
using SEP490_Robot_FoodOrdering.Core.Response;

namespace SEP490_Robot_FoodOrdering.Application.Service.Interface
{
    public interface ITableShare
    {
        /// <summary>
        /// Moderators manually attach a device to an occupied table.
        /// </summary>
        Task<BaseResponseModel<TableResponse>> AttachDeviceAsync(Guid tableId, AttachDeviceRequest request);

        /// <summary>
        /// Generates a QR code for sharing a table session.
        /// </summary>
        Task<BaseResponseModel<QrShareResponse>> ShareTableAsync(Guid tableId, string currentDeviceId);

        /// <summary>
        /// Accepts a shared table session using a token.
        /// </summary>
        Task<BaseResponseModel<TableResponse>> AcceptSharedTableAsync(Guid tableId, string shareToken, string newDeviceId);
        
        /// <summary>
        /// Transfer table ownership (Placeholder for future implementation)
        /// </summary>
        Task<BaseResponseModel<TableResponse>> TransferTableAsync(Guid tableId, Guid transferToUserId, string? reason = null, string transferredBy = "System");
    }
}
