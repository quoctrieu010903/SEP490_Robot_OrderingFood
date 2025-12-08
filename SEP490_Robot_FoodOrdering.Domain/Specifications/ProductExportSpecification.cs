using Microsoft.EntityFrameworkCore;
using SEP490_Robot_FoodOrdering.Domain.Entities;
using SEP490_Robot_FoodOrdering.Domain;

public sealed class ProductExportSpecification : BaseSpecification<Product>
{
    public ProductExportSpecification()
    {
        // Nếu bạn có soft-delete thì mở dòng này (tuỳ entity của bạn)
        // AddFilter(p => !p.IsDeleted);

        ApplyInclude(q => q
            .Include(p => p.ProductCategories)
                .ThenInclude(pc => pc.Category)
            .Include(p => p.Sizes)
            .Include(p => p.AvailableToppings)
                .ThenInclude(pt => pt.Topping)
        );

        // Nếu BaseSpecification của bạn có hỗ trợ:
        // ApplyAsNoTracking();   // export thường không cần tracking
        // ApplySplitQuery();     // tránh cartesian explosion khi nhiều include
    }
}
