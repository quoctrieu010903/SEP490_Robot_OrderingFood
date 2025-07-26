using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SEP490_Robot_FoodOrdering.Domain.Entities;

namespace SEP490_Robot_FoodOrdering.Domain.Specifications
{
    public class ProductToppingSpecification : BaseSpecification<ProductTopping>
    {
        public ProductToppingSpecification()
         : base(x => !x.DeletedTime.HasValue)
        {
            ApplyInclude(x => x
                .Include(x => x.Product)
                .Include(p => p.Topping)
            );

        }

        public ProductToppingSpecification(Guid productToppingId)
            : base(x => x.DeletedTime.HasValue && x.Id == productToppingId)
        {
            ApplyInclude(x => x
                .Include(x => x.Product)
                .Include(p => p.Topping)
            );
        }
        public ProductToppingSpecification(Guid productId, bool productwithtopping)
                : base(x => !x.DeletedTime.HasValue && x.ProductId == productId)
        {
            ApplyInclude(x => x
                .Include(x => x.Product)
                    .ThenInclude(x=>x.AvailableToppings)
                        .ThenInclude(x=> x.Topping)
                 .Include(x=>x.Topping)
            );

        }
    }
}
