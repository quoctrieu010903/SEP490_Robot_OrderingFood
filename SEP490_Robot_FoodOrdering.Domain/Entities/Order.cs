
using SEP490_Robot_FoodOrdering.Core.Base;
using SEP490_Robot_FoodOrdering.Domain.Enums;

namespace SEP490_Robot_FoodOrdering.Domain.Entities
{
    public class Order : BaseEntity
    {
        public Guid? TableId { get; set; } // nếu đặt tại quán
        public virtual Table? Table { get; set; } // nếu đặt tại quán
        public OrderStatus Status { get; set; }  // 0: Pending, 1: InProgress, 2: Completed, 3: Cancelled
        public decimal TotalPrice { get; set; }
        public PaymentMethodEnums paymentMethod { get; set; }  // 0: Cash, 1: Card, 2: Online
        public virtual ICollection<OrderItem> OrderItems { get; set; }
        public virtual Payment Payment { get; set; }

    }
}
