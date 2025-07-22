

using SEP490_Robot_FoodOrdering.Core.Base;

namespace SEP490_Robot_FoodOrdering.Domain.Entities
{
    public class ProductTopping : BaseEntity
    {
        public Guid ProductId { get; set; }
        public virtual Product Product { get; set; }

        public Guid ToppingId { get; set; }
        public virtual Topping Topping { get; set; }

    }
}
