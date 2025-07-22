using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEP490_Robot_FoodOrdering.Domain.Enums
{
    public enum PaymentStatusEnums
    {
        Pending = 1,     // Chưa thanh toán
        Paid = 2,        // Thanh toán thành công
        Failed = 3,      // Thất bại
        Refunded = 4     // Đã hoàn tiền
    }
}
