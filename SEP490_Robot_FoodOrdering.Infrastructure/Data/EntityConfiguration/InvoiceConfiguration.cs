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
    public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
    {
        public void Configure(EntityTypeBuilder<Invoice> builder)
        {

            builder.HasOne(i => i.Order)
           .WithOne(o => o.Invoices)
           .HasForeignKey<Invoice>(i => i.OrderId)
           .OnDelete(DeleteBehavior.Cascade); // xoá Order sẽ xoá Invoice

            builder.HasOne(i => i.Table)
               .WithMany()
               .HasForeignKey(i => i.TableId)
               .OnDelete(DeleteBehavior.SetNull);

        }
    }
}
