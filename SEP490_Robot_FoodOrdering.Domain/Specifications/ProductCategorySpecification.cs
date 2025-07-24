
using Microsoft.EntityFrameworkCore;
using SEP490_Robot_FoodOrdering.Domain.Entities;

namespace SEP490_Robot_FoodOrdering.Domain.Specifications
{
    public class ProductCategorySpecification : BaseSpecification<ProductCategory>
    {
        public int PageIndex { get; set; }
        public int PageSize { get; set; }
        public ProductCategorySpecification(int pageIndex, int pageSize)
            : base(p => !p.DeletedTime.HasValue) // No specific filter, just pagination
        {
            PageIndex = pageIndex;
            PageSize = pageSize;
            // Include related entities if necessary
            ApplyInclude(q => q
                .Include(pc => pc.Product)
                .Include(pc => pc.Category));
        }
    }
}
