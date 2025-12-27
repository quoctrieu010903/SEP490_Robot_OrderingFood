using SEP490_Robot_FoodOrdering.Application.DTO.Response.Order;
using SEP490_Robot_FoodOrdering.Core.Response;

namespace SEP490_Robot_FoodOrdering.Application.DTO.Response.Table
{
    /// <summary>
    /// Response for random scan and order operation
    /// </summary>
    public class RandomScanAndOrderResponse
    {
        /// <summary>
        /// The randomly generated device ID
        /// </summary>
        public string DeviceId { get; set; } = string.Empty;

        /// <summary>
        /// The selected table ID (random or specified)
        /// </summary>
        public Guid TableId { get; set; }

        /// <summary>
        /// Result of the table scan operation
        /// </summary>
        public BaseResponseModel<TableResponse> ScanResult { get; set; }

        /// <summary>
        /// Result of the order creation operation
        /// </summary>
        public BaseResponseModel<OrderResponse> OrderResult { get; set; }
    }
}

