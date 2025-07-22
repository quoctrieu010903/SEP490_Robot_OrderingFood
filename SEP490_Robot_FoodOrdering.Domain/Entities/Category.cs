using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SEP490_Robot_FoodOrdering.Core.Base;

namespace SEP490_Robot_FoodOrdering.Domain.Entities
{
    public class Category : BaseEntity
    {
        public string Name { get; set; }

        public virtual ICollection<ProductCategory>? ProductCategories { get; set; }
    }
}
