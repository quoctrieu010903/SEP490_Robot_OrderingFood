using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using SEP490_Robot_FoodOrdering.Domain.Entities;

namespace SEP490_Robot_FoodOrdering.Infrastructure.Data.EntityConfiguration
{
    public class CancelledOrderItemConfiguration : IEntityTypeConfiguration<CancelledOrderItem>
    {
        public void Configure(EntityTypeBuilder<CancelledOrderItem> builder)
    {
        builder.ToTable("CancelledOrderItems");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Reason)
               .IsRequired()
               .HasMaxLength(200);

        builder.Property(x => x.Note)
               .HasMaxLength(500);

        builder.Property(x => x.ItemPrice)
               .HasColumnType("decimal(18,2)");

        builder.Property(x => x.OrderTotalBefore)
               .HasColumnType("decimal(18,2)");

        builder.Property(x => x.OrderTotalAfter)
               .HasColumnType("decimal(18,2)");

        builder.HasOne(x => x.OrderItem)
               .WithMany(x => x.CancelledOrderItems)
               .HasForeignKey(x => x.OrderItemId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.CancelledByUser)
               .WithMany(u => u.CancelledItems)
               .HasForeignKey(x => x.CancelledByUserId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
}
