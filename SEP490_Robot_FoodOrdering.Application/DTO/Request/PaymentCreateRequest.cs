using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEP490_Robot_FoodOrdering.Application.DTO.Request
{
    public class PaymentCreateRequest
    {
        public string MoneyUnit { get; set; }
        public Guid OrderId { get; set; }
        public string PaymentContent { get; set; } = "";
        public float TotalAmount { get; set; } = 0;
    }
}
