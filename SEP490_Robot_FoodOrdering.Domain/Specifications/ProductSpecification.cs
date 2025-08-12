    using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SEP490_Robot_FoodOrdering.Domain.Entities;
using SEP490_Robot_FoodOrdering.Domain.Specifications.Params;

namespace SEP490_Robot_FoodOrdering.Domain.Specifications
{
    public class ProductSpecification : BaseSpecification<Product>
    {
        public int PageIndex { get; set; }
        public int PageSize { get; set; }

        public ProductSpecification(ProductSpecParams specParams, int pageIndex, int pageSize)
            : base(p => 
                string.IsNullOrEmpty(specParams.Search) ||
                p.Name.ToLower().Contains(specParams.Search.ToLower()) ||
                (!string.IsNullOrEmpty(p.Description) && p.Description.ToLower().Contains(specParams.Search.ToLower()))&& !p.DeletedTime.HasValue)
        {
            PageIndex = pageIndex;
            PageSize = pageSize;

            // Include related entities
            ApplyInclude(q => q
                .Include(p => p.ProductCategories)
                    .ThenInclude(pc => pc.Category)
                .Include(p => p.Sizes)
                .Include(p => p.AvailableToppings)
                    .ThenInclude(pt => pt.Topping)
            );

            // Filter by category name
            if (!string.IsNullOrEmpty(specParams.CategoryName))
            {
                AddFilter(p =>
                    p.ProductCategories.Any(pc =>
                        pc.Category != null && pc.Category.Name.ToLower().Contains(specParams.CategoryName.ToLower())));
            }

            // Filter by duration range
            if (specParams.MinDuration.HasValue)
            {
                AddFilter(p => p.DurationTime >= specParams.MinDuration.Value);
            }

            if (specParams.MaxDuration.HasValue)
            {
                AddFilter(p => p.DurationTime <= specParams.MaxDuration.Value);
            }

            // Sorting
            if (!string.IsNullOrEmpty(specParams.Sort))
            {
                var isDescending = specParams.Sort.StartsWith("-");
                var propertyName = isDescending ? specParams.Sort.Substring(1) : specParams.Sort;

                ApplySorting(propertyName, isDescending);
            }
            else
            {
                AddOrderByDescending(p => p.CreatedTime); // default sort
            }
        }

        private void ApplySorting(string propertyName, bool isDescending)
        {
            var parameter = Expression.Parameter(typeof(Product), "x");
            var property = Expression.Property(parameter, propertyName);

            var sortExpression = Expression.Lambda<Func<Product, object>>(
                Expression.Convert(property, typeof(object)), parameter);

            if (isDescending)
            {
                AddOrderByDescending(sortExpression);
            }
            else
            {
                AddOrderBy(sortExpression);
            }
        }
    }
}
