
using SEP490_Robot_FoodOrdering.Core.Base;
using SEP490_Robot_FoodOrdering.Domain.Enums;

namespace SEP490_Robot_FoodOrdering.Domain.Entities
{
    public class ProductSize : BaseEntity
    {
        public SizeEnums SizeName { get; set; }
        public decimal Price { get; set; }

        public Guid ProductId { get; set; }
        public virtual Product Product { get; set; }
    }
}
