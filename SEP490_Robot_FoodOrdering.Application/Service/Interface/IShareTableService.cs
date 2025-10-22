using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SEP490_Robot_FoodOrdering.Application.DTO.Response.Table;
using SEP490_Robot_FoodOrdering.Core.Response;

namespace SEP490_Robot_FoodOrdering.Application.Service.Interface
{
    public interface IShareTableService
    {
        /// <summary>
        /// Chia sẻ mã QR của bàn để người khác có thể tham gia cùng bàn.
        /// </summary>
        /// <param name="tableId">Id của bàn cần chia sẻ</param>
        /// <param name="currentDeviceId">Device Id hiện tại của người chia sẻ</param>
        Task<BaseResponseModel<QrShareResponse>> ShareTableAsync(
            Guid tableId,
            string currentDeviceId
        );

        /// <summary>
        /// Người khác chấp nhận lời mời chia sẻ bàn (qua token QR).
        /// </summary>
        /// <param name="tableId">Id bàn được chia sẻ</param>
        /// <param name="shareToken">Token được tạo khi chia sẻ QR</param>
        /// <param name="newDeviceId">Thiết bị mới chấp nhận chia sẻ</param>
        Task<BaseResponseModel<TableResponse>> AcceptSharedTableAsync(
            Guid tableId,
            string shareToken,
            string newDeviceId
        );

    }
}
