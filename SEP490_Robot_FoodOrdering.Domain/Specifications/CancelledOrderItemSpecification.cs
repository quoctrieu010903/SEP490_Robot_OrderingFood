using System;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using SEP490_Robot_FoodOrdering.Domain;
using SEP490_Robot_FoodOrdering.Domain.Entities;
using SEP490_Robot_FoodOrdering.Domain.Specifications.Params;
using SEP490_Robot_FoodOrdering.Infrastructure.Specifications;



namespace SEP490_Robot_FoodOrdering.Infrastructure.Specifications.CancelledItems
{
    public class CancelledItemSpecification : BaseSpecification<CancelledOrderItem>
    {
        public CancelledItemSpecification(CancelledItemFilterRequestParam filter)
            : base(ci =>
                (!filter.OrderItemId.HasValue || ci.OrderItemId == filter.OrderItemId.Value) &&
                (!filter.CancelledByUserId.HasValue || ci.CancelledByUserId == filter.CancelledByUserId.Value) &&
                (!filter.From.HasValue || ci.CreatedTime >= filter.From.Value) &&
                (!filter.To.HasValue || ci.CreatedTime <= filter.To.Value) &&
                (string.IsNullOrEmpty(filter.Search) ||
                    (ci.Note != null && ci.Note.ToLower().Contains(filter.Search.ToLower())) ||
                    (ci.OrderItem.Product != null && ci.OrderItem.Product.Name.ToLower().Contains(filter.Search.ToLower())))
            )
        {

            AddIncludes();

            // Sorting logic
            if (!string.IsNullOrEmpty(filter.Sort))
            {
                switch (filter.Sort.ToLower())
                {
                    case "createdtime":
                        AddOrderBy(ci => ci.CreatedTime);
                        break;
                    case "-createdtime":
                        AddOrderByDescending(ci => ci.CreatedTime);
                        break;
                    case "lastupdatedtime":
                        AddOrderBy(ci => ci.LastUpdatedTime);
                        break;
                    case "-lastupdatedtime":
                        AddOrderByDescending(ci => ci.LastUpdatedTime);
                        break;
                    default:
                        AddOrderByDescending(ci => ci.CreatedTime);
                        break;
                }
            }
            else
            {
                // Default sort by CreatedTime DESC
                AddOrderByDescending(ci => ci.CreatedTime);
            }

           
        }

        private void AddIncludes()
        {
            ApplyInclude(q => q
                .Include(o => o.OrderItem)
                    .ThenInclude(oi => oi.Product)
                .Include(oi => oi.OrderItem)
                    .ThenInclude(oi => oi.Order)
                .Include(o => o.OrderItem)
                    .ThenInclude(oi => oi.ProductSize) // Ensure ProductSize is included
                .Include(o => o.OrderItem)
                    .ThenInclude(oi => oi.RemakeOrderItems)
                .Include(o => o.CancelledByUser)
                );



        }

    }
}
