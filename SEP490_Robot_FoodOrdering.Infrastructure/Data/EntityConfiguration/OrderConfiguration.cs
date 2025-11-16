

using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SEP490_Robot_FoodOrdering.Domain.Entities;

namespace SEP490_Robot_FoodOrdering.Infrastructure.Data.EntityConfiguration
{
    public class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.Property(o => o.Status)
                 .IsRequired()
                 .HasConversion<int>();

            builder.Property(o => o.paymentMethod)
                   .IsRequired()
                   .HasConversion<int>();
            builder.HasOne(o => o.Table)
                       .WithMany()
                       .HasForeignKey(o => o.TableId)
                       .OnDelete(DeleteBehavior.SetNull); // Or .Restrict if you prefer

            builder.HasMany(o => o.OrderItems)
                   .WithOne(oi => oi.Order)
                   .HasForeignKey(oi => oi.OrderId)
                   .OnDelete(DeleteBehavior.Cascade);// ✅ Delete OrderItems when Order is deleted
            
            builder.HasOne(o => o.Customer)
                .WithMany(c => c.Orders)
                .HasForeignKey(o => o.CustomerId)
                .OnDelete(DeleteBehavior.SetNull);
            
            builder.HasOne(o => o.TableSession)
                .WithMany(ts => ts.Orders)
                .HasForeignKey(o => o.TableSessionId)
                .OnDelete(DeleteBehavior.Restrict);


            builder.Ignore(o => o.TotalPaid);
            builder.Ignore(o => o.IsFullyPaid);

        }
    }
}

