
using SEP490_Robot_FoodOrdering.Core.Base;
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

            public OrderItemStatus Status { get; set; } // Pending, Preparing, Ready, Served, Completed, Cancelled , Returned
            public string? Note { get; set; }
            public string? RemakeNote { get; set; }
            public bool IsUrgent { get; set; } = false; 


        public virtual ICollection<OrderItemTopping> OrderItemTopping { get; set; }
        public virtual ICollection<RemakeOrderItem> RemakeOrderItems { get; set; }
        public virtual ICollection<CancelledOrderItem> CancelledOrderItems { get; set; }









    }
}
