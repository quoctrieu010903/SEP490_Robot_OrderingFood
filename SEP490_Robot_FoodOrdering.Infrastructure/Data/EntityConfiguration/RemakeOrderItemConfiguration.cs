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
    internal class RemakeOrderItemConfiguration : IEntityTypeConfiguration<RemakeOrderItem>
    {
        public void Configure(EntityTypeBuilder<RemakeOrderItem> builder)
        {
            builder.ToTable("RemakeOrderItems");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.RemakeNote)
                   .IsRequired()
                   .HasMaxLength(300);

            builder.Property(x => x.IsUrgent)
                   .HasDefaultValue(true);

            builder.HasOne(x => x.OrderItem)
                   .WithMany(x => x.RemakeOrderItems)
                   .HasForeignKey(x => x.OrderItemId)
                   .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(x => x.RemakedByUser)
                   .WithMany(u => u.RemakeItems)
                   .HasForeignKey(x => x.RemakedByUserId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
