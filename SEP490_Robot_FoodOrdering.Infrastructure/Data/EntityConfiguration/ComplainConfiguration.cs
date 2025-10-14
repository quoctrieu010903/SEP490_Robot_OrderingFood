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
    public class ComplainConfiguration : IEntityTypeConfiguration<Complain>
    {
        public void Configure(EntityTypeBuilder<Complain> builder)
        {
            builder.HasOne(c => c.Table)
                   .WithMany(t => t.Complains)
                   .HasForeignKey(c => c.TableId);

            builder.HasOne(c => c.OrderItem)
                   .WithMany(oi => oi.Complains)
                   .HasForeignKey(c => c.OrderItemId);
        }

    }
}


