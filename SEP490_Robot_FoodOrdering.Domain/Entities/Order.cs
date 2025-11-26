
using SEP490_Robot_FoodOrdering.Core.Base;
using SEP490_Robot_FoodOrdering.Domain.Entities.SEP490_Robot_FoodOrdering.Domain.Entities;
using SEP490_Robot_FoodOrdering.Domain.Enums;

namespace SEP490_Robot_FoodOrdering.Domain.Entities
{
    public class  Order : BaseEntity
    {
        public Guid? TableId { get; set; } // nếu đặt tại quán
        public virtual Table? Table { get; set; } // nếu đặt tại quán
                                                  // Phiên bàn
       
        // Khách hàng – nếu có
        public Guid? CustomerId { get; set; }
        public virtual Customer? Customer { get; set; }
        public String OrderCode { get; set; }  = $"OR{Guid.NewGuid():N}"[..8].ToUpper();

        public OrderStatus Status { get; set; }  // Automatically derived from order items' statuses
        public decimal TotalPrice { get; set; }
       
        public PaymentMethodEnums paymentMethod { get; set; }  // 0: Cash, 1: Card, 2: Online
        public PaymentStatusEnums PaymentStatus { get; set; } // Track payment status at order level
        public virtual ICollection<OrderItem> OrderItems { get; set; }
        public virtual  Invoice Invoices { get; set; }

        public Guid? TableSessionId { get; set; }
        public virtual TableSession? TableSession { get; set; }



        public bool IsFullyPaid => OrderItems
              .Where(x => x.Status != OrderItemStatus.Cancelled)
              .All(x => x.InvoiceDetails.Any());
        public virtual ICollection<Payment> Payments { get; set; }
        //public decimal TotalPaid => OrderItems    //tot
        //    .Where(p => p.PaymentStatus != PaymentStatusEnums.Paid)
        //    .Sum(p => (decimal) p.OrderItems.Sum(x=>x.TotalPrice));
        public decimal TotalPaid =>
         OrderItems
        .Where(x => x.PaymentStatus == PaymentStatusEnums.Paid)
        .Sum(x => (decimal) x.TotalPrice);


    }

}
