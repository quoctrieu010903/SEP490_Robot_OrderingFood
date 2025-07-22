
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SEP490_Robot_FoodOrdering.Domain.Entities;

namespace SEP490_Robot_FoodOrdering.Infrastructure.Data.EntityConfiguration
{
    public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
    {
        public void Configure(EntityTypeBuilder<OrderItem> builder)
        {
            builder.HasOne(oi => oi.Order)
               .WithMany(o => o.OrderItems)
               .HasForeignKey(oi => oi.OrderId)
               .OnDelete(DeleteBehavior.Cascade); // ✅ Delete OrderItem when Order is deleted

            builder.HasOne(oi => oi.Product)
                   .WithMany()
                   .HasForeignKey(oi => oi.ProductId)
                   .OnDelete(DeleteBehavior.Restrict); // ❗ Prevent accidental Product deletion

            builder.HasOne(oi => oi.ProductSize)
                   .WithMany()
                   .HasForeignKey(oi => oi.ProductSizeId)
                   .OnDelete(DeleteBehavior.Restrict); // ❗ Same reason as above

            builder.HasMany(oi => oi.OrderItemTopping)
                   .WithOne(oit => oit.OrderItem)
                   .HasForeignKey(oit => oit.OrderItemId)
                   .OnDelete(DeleteBehavior.Cascade); // ✅ Delete Toppings when OrderItem is deleted
        }
    }
}
