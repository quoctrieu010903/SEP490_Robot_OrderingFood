
using SEP490_Robot_FoodOrdering.Core.Base;

namespace SEP490_Robot_FoodOrdering.Domain.Entities
{
    public class Product : BaseEntity  
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int DurationTime { get; set; } // Duration in minutes
        public string ImageUrl { get; set; }

        public virtual ICollection<ProductCategory>? ProductCategories { get; set; }
        public virtual ICollection<ProductSize>? Sizes { get; set; }
        public virtual ICollection<ProductTopping>? AvailableToppings { get; set; }
    }
    
}
