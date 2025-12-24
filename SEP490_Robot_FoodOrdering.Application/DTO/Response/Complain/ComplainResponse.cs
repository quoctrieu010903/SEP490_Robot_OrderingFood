using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SEP490_Robot_FoodOrdering.Application.DTO.Request.Complain;

namespace SEP490_Robot_FoodOrdering.Application.DTO.Response.Complain
{
    public class ComplainResponse
    {
       public Guid ComplainId { get; set; }
        public Guid IdTable { get; set; }
        public string FeedBack { get; set; }    
        public bool IsPending { get; set; }
        public DateTime CreateData { get; set; }
        public DateTime? LastOrderUpdateTime { get; set; }

        public int KitchenItemCount { get; set; }   // Pending + Preparing + Remark
        public int WaiterItemCount { get; set; }    // Ready + Served
        public int CancelledItemCount { get; set; } // Cancelled

        public string? ResolutionNote { get; set; } // Ghi chú xử lý từ moderator (chứa "Yêu cầu nhanh:" khi được gửi phục vụ nhanh)
        public string? HandledBy { get; set; } // tên moderator xử lý
    }
}
