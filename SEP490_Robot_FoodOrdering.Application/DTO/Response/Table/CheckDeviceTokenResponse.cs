using SEP490_Robot_FoodOrdering.Domain.Enums;

namespace SEP490_Robot_FoodOrdering.Application.DTO.Response.Table
{
    /// <summary>
    /// Response for checking if a device token matches a table's current device
    /// </summary>
    public class CheckDeviceTokenResponse
    {
        /// <summary>
        /// Indicates whether the provided device token matches the table's current device
        /// </summary>
        public bool IsMatch { get; set; }

        /// <summary>
        /// The ID of the table
        /// </summary>
        public Guid TableId { get; set; }

        /// <summary>
        /// The name of the table
        /// </summary>
        public string TableName { get; set; } = string.Empty;

        /// <summary>
        /// The current device ID associated with the table (null if no device)
        /// </summary>
        public string? CurrentDeviceId { get; set; }

        /// <summary>
        /// The current status of the table (Available, Occupied, Reserved)
        /// </summary>
        public TableEnums Status { get; set; }

        /// <summary>
        /// Indicates whether the table's QR code is locked
        /// </summary>
        public bool IsQrLocked { get; set; }

        /// <summary>
        /// The last time the table was accessed (null if never accessed)
        /// </summary>
        public DateTime? LastAccessedAt { get; set; }
    }
}

