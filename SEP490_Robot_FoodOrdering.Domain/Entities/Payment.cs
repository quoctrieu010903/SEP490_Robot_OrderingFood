
using SEP490_Robot_FoodOrdering.Core.Base;
using SEP490_Robot_FoodOrdering.Domain.Enums;

namespace SEP490_Robot_FoodOrdering.Domain.Entities
{
    public class Payment : BaseEntity
    {
        public DateTime PaymentTime { get; set; } = DateTime.UtcNow;
        public PaymentMethodEnums PaymentMethod { get; set; } // COD, VNPay, Momo
        public PaymentStatusEnums PaymentStatus { get; set; } // Paid, Failed, Pending

        public Guid OrderId { get; set; }
        public virtual Order Order { get; set; }
    }
}
