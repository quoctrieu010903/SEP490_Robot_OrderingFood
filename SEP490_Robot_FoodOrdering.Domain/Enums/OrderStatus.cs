using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEP490_Robot_FoodOrdering.Domain.Enums
{
    public enum OrderStatus
    {
        Pending = 1,       // Đang chờ xác nhận
        Confirmed = 2,     // Đã xác nhận
        Preparing = 3,     // Đang chuẩn bị món
        Delivering = 4,    // Bắt đầu phục phụ
        Completed = 5,     // Đã giao / hoàn thành
        Cancelled = 6      // Đã huỷ

    }
}
