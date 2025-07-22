
using SEP490_Robot_FoodOrdering.Core.Base;

namespace SEP490_Robot_FoodOrdering.Domain.Entities
{
    public class Topping : BaseEntity
    {
        public string Name { get; set; }
        public decimal Price { get; set; }

        public virtual ICollection<ProductTopping>? ProductToppings { get; set; }
    }

}

