
using SEP490_Robot_FoodOrdering.Core.Base;
using SEP490_Robot_FoodOrdering.Domain.Entities.SEP490_Robot_FoodOrdering.Domain.Entities;
using SEP490_Robot_FoodOrdering.Domain.Enums;

namespace SEP490_Robot_FoodOrdering.Domain.Entities
{
        public class OrderItem : BaseEntity
        {
            public Guid OrderId { get; set; }
            public virtual Order Order { get; set; }

            public Guid ProductId { get; set; }
            public virtual Product Product { get; set; }

            public Guid ProductSizeId { get; set; }
            public virtual ProductSize ProductSize { get; set; }

            public PaymentStatusEnums PaymentStatus { get; set; } = PaymentStatusEnums.Pending;
              public OrderItemStatus Status { get; set; } // Pending, Preparing, Ready, Served, Completed, Cancelled , Returned
            public string? Note { get; set; }
            public string? RemakeNote { get; set; }
            public bool IsUrgent { get; set; } = false;
            public decimal ? Price { get; set; }
            public decimal ? TotalPrice { get; set; } // tong tien giua size va OrderItemTopping
            public DateTime? ReadyTime { get; set; } // Thời gian chuyển sang trạng thái Ready (Preparing → Ready)
            public DateTime? ServedTime { get; set; } // Thời gian chuyển sang trạng thái Served (Ready → Served)
            public DateTime? CancelledTime { get; set; } // Thời gian hủy món
            public DateTime? RemakedTime { get; set; } // Thời gian làm lại món (chuyển sang Remark)
            



        public virtual ICollection<OrderItemTopping> OrderItemTopping { get; set; }
        public virtual ICollection<RemakeOrderItem> RemakeOrderItems { get; set; }
        public virtual ICollection<CancelledOrderItem> CancelledOrderItems { get; set; }
        public virtual ICollection<InvoiceDetail> InvoiceDetails { get; set; }
        public bool IsPaid => (PaymentStatus == PaymentStatusEnums.Paid)
                         || (InvoiceDetails != null && InvoiceDetails.Any());







    }
}
