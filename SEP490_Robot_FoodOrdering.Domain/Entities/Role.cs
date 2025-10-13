
using SEP490_Robot_FoodOrdering.Core.Base;
using SEP490_Robot_FoodOrdering.Domain.Enums;

namespace SEP490_Robot_FoodOrdering.Domain.Entities
{
    public class Role : BaseEntity
    {
        public RoleNameEnums Name { get; set; }
        public string Description { get; set; }
        public virtual ICollection<User> Users { get; set; }


    }
}
