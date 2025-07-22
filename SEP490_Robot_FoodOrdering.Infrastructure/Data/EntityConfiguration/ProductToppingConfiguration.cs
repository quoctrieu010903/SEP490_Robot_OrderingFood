

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SEP490_Robot_FoodOrdering.Domain.Entities;

namespace SEP490_Robot_FoodOrdering.Infrastructure.Data.EntityConfiguration
{
    public class ProductToppingConfiguration : IEntityTypeConfiguration<ProductTopping>
    {
        public void Configure(EntityTypeBuilder<ProductTopping> builder)
        {
            builder.HasOne(pt => pt.Product)
               .WithMany(p => p.AvailableToppings)
               .HasForeignKey(pt => pt.ProductId);

            builder.HasOne(pt => pt.Topping)
                   .WithMany(t => t.ProductToppings)
                   .HasForeignKey(pt => pt.ToppingId);
        }
    }
}
