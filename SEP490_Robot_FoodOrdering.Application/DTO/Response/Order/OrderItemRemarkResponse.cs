using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEP490_Robot_FoodOrdering.Application.DTO.Response.Order
{
    public class OrderItemRemarkResponse
    {
        public Guid Id { get; set; }
        public Guid ParentOrderId { get; set; }
        public string RemarkNote { get; set; } = string.Empty;
        public DateTime CreatedTime { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending"; // Default status
    }
}
