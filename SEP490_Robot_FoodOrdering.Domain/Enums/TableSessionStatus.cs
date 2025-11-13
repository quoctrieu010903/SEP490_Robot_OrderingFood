using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEP490_Robot_FoodOrdering.Domain.Enums
{
    public enum TableSessionStatus
    {
        Active = 0,
        Closed = 1, // checkout / release xong
        Released = 2, // auto giải phóng vì không order
        Expired = 3  // hết hạn do idle lâu
    }
}
