
using SEP490_Robot_FoodOrdering.Core.Base;
using SEP490_Robot_FoodOrdering.Domain.Enums;

namespace SEP490_Robot_FoodOrdering.Domain.Entities
{
    public class Table : BaseEntity
    {
        public Guid Name { get; set; }
        public TableEnums Status { get; set; }  
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
