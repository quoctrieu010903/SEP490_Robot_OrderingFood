using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SEP490_Robot_FoodOrdering.Domain.Entities;

namespace SEP490_Robot_FoodOrdering.Infrastructure.Data.EntityConfiguration
{
    public class OrderItemToppingConfiguration : IEntityTypeConfiguration<OrderItemTopping>
    {
        public void Configure(EntityTypeBuilder<OrderItemTopping> builder)
        {
            builder.HasOne(oit => oit.OrderItem)
                   .WithMany(oi => oi.OrderItemTopping) 
                   .HasForeignKey(oit => oit.OrderItemId)
                   .OnDelete(DeleteBehavior.Cascade);   

            // Topping -> OrderItemTopping (1-n)
            builder.HasOne(oit => oit.Topping)
                   .WithMany()
                   .HasForeignKey(oit => oit.ToppingId)
                   .OnDelete(DeleteBehavior.Restrict);  // optional: tránh xóa topping nếu bị dùng
        }
    }
}
