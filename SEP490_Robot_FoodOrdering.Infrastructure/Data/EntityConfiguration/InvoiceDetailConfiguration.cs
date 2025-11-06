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
    internal class InvoiceDetailConfiguration : IEntityTypeConfiguration<InvoiceDetail>

    {
        public void Configure(EntityTypeBuilder<InvoiceDetail> builder)
        {

            builder.HasOne(id => id.Invoices)
              .WithMany(i => i.Details)
              .HasForeignKey(id => id.InvoiceId)
              .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(id => id.OrderItem)
                   .WithMany(oi => oi.InvoiceDetails)
                   .HasForeignKey(id => id.OrderItemId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
