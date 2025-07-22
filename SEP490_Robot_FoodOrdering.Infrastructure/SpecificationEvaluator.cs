using Microsoft.EntityFrameworkCore;
using SEP490_Robot_FoodOrdering.Domain.Specifications.Interface;

namespace SEP490_Robot_FoodOrdering.Infrastructure
{
    public class SpecificationEvaluator<TEntity>
     where TEntity : class
    {
        public static IQueryable<TEntity> GetQuery(
            IQueryable<TEntity> inputQuery,
            ISpecification<TEntity> spec)
        {
            // Initialize queryable
            var query = inputQuery.AsQueryable();

            // Apply split query if enabled
            if (spec.AsSplitQuery)
            {
                query = query.AsSplitQuery();
            }

            // Apply criteria
            if (spec.Criteria != null!) query = query.Where(spec.Criteria);

            // Apply additional filters
            if (spec.Filters != null! && spec.Filters.Any())
            {
                foreach (var filter in spec.Filters)
                {
                    query = query.Where(filter);
                }
            }

            // Apply grouping
            if (spec.GroupBy != null!) query = query.GroupBy(spec.GroupBy).Select(g => g.FirstOrDefault())!;

            // Apply ordering
            if (spec.OrderBy != null!) query = query?.OrderBy(spec.OrderBy);
            if (spec.OrderByDescending != null!) query = query?.OrderByDescending(spec.OrderByDescending);

            // Apply pagination
            if (spec.IsPagingEnabled) query = query?.Skip(spec.Skip).Take(spec.Take);

            // Apply includes (with support for ThenInclude)
            if (spec.Includes != null!)
            {
                foreach (var include in spec.Includes)
                {
                    if (query != null) query = include(query);
                }
            }

            return query!;
        }
    }

}
