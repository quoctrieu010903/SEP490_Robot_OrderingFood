using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEP490_Robot_FoodOrdering.Application.DTO.Response.CancelledItem
{
        public class CancelledItemResponse
        {
            public Guid Id { get; set; }

            // Thông tin liên kết
            public Guid OrderItemId { get; set; }
            public Guid OrderId { get; set; }
            public string OrderCode { get; set; }
            public string ProductName { get; set; }
            public string? SizeName { get; set; }
            public string? ToppingName { get; set; }
            // Thông tin huỷ
            public string Reason { get; set; }
            public string? Note { get; set; }
            public Guid CancelledByUserId { get; set; }
            public string CancelledByUserName { get; set; }
            public DateTime CreatedTime { get; set; }

            // Thông tin giá trị đơn
            public decimal ItemPrice { get; set; }
            public decimal OrderTotalBefore { get; set; }
            public decimal OrderTotalAfter { get; set; }

            // Metadata
            public string CreatedBy { get; set; }
            public string LastUpdatedBy { get; set; }
            public DateTime LastUpdatedTime { get; set; }
        }

    }
}
