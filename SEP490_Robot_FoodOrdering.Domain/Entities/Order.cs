
using SEP490_Robot_FoodOrdering.Core.Base;
using SEP490_Robot_FoodOrdering.Domain.Enums;

namespace SEP490_Robot_FoodOrdering.Domain.Entities
{
    public class Order : BaseEntity
    {
        public Guid? TableId { get; set; } // nếu đặt tại quán
        public virtual Table? Table { get; set; } // nếu đặt tại quán
        public OrderStatus Status { get; set; }  // Automatically derived from order items' statuses
        public decimal TotalPrice { get; set; }
       
        public PaymentMethodEnums paymentMethod { get; set; }  // 0: Cash, 1: Card, 2: Online
        public PaymentStatusEnums PaymentStatus { get; set; } // Track payment status at order level
        public virtual ICollection<OrderItem> OrderItems { get; set; }
        public virtual ICollection<Invoice> Invoices { get; set; }
            
        public decimal TotalPaid => Invoices?.Sum(i => i.TotalMoney) ?? 0;

        public bool IsFullyPaid => OrderItems
              .Where(x => x.Status != OrderItemStatus.Cancelled)
              .All(x => x.InvoiceDetails.Any());
        public virtual Payment Payment { get; set; }

    }

}
