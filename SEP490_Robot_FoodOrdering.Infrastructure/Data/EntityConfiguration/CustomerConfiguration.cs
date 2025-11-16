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
    public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
    {
        public void Configure(EntityTypeBuilder<Customer> builder)
        {
            // Customer 1 - n TableSession
            builder.HasMany(x => x.TableSessions)
                .WithOne(ts => ts.Customer)
                .HasForeignKey(ts => ts.CustomerId)
                .OnDelete(DeleteBehavior.SetNull);

            // Customer 1 - n Order
            builder.HasMany(x => x.Orders)
                .WithOne(o => o.Customer)
                .HasForeignKey(o => o.CustomerId)
                .OnDelete(DeleteBehavior.SetNull);

            // Customer 1 - n Invoice
            builder.HasMany(x => x.Invoices)
                .WithOne(i => i.Customer)
                .HasForeignKey(i => i.CustomerId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}