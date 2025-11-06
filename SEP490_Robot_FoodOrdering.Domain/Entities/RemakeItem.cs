using SEP490_Robot_FoodOrdering.Core.Base;
using SEP490_Robot_FoodOrdering.Domain.Enums;

namespace SEP490_Robot_FoodOrdering.Domain.Entities
{
    public class RemakeOrderItem : BaseEntity
    {
        public Guid OrderItemId { get; set; }
        public virtual OrderItem OrderItem { get; set; }

        public string RemakeNote { get; set; }
        public OrderItemStatus PreviousStatus { get; set; }
        public OrderItemStatus AfterStatus { get; set; }

        public Guid RemakedByUserId { get; set; }
        public virtual User RemakedByUser { get; set; }
        public bool IsUrgent { get; set; } = true;

    }
}