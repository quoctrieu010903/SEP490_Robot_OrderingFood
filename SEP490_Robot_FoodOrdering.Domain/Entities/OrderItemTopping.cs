
using SEP490_Robot_FoodOrdering.Core.Base;

namespace SEP490_Robot_FoodOrdering.Domain.Entities
{
    public class OrderItemTopping : BaseEntity
    {
        public Guid OrderItemId { get; set; }
        public virtual OrderItem OrderItem { get; set; }

        public Guid ToppingId { get; set; }
        public virtual Topping Topping { get; set; }

        public decimal Price { get; set; } 
    }
}
