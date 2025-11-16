

using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using SEP490_Robot_FoodOrdering.Domain.Entities;

namespace SEP490_Robot_FoodOrdering.Infrastructure.Data.EntityConfiguration
{
    public class TableSessionConfiguration : IEntityTypeConfiguration<TableSession>
    {
        public void Configure(EntityTypeBuilder<TableSession> builder)
        {
            builder.Property(ts => ts.Status)
                .IsRequired()
                .HasConversion<int>();
            // TableSession n - 1 Table
            builder.HasOne(ts => ts.Table)
                .WithMany(t => t.Sessions)
                .HasForeignKey(ts => ts.TableId)
                .OnDelete(DeleteBehavior.Restrict);

            // TableSession n - 1 Customer (optional)
            builder.HasOne(ts => ts.Customer)
                .WithMany(c => c.TableSessions)
                .HasForeignKey(ts => ts.CustomerId)
                .OnDelete(DeleteBehavior.SetNull);

            // TableSession 1 - n TableActivity
            builder.HasMany(ts => ts.Activities)
                .WithOne(a => a.TableSession)
                .HasForeignKey(a => a.TableSessionId)
                .OnDelete(DeleteBehavior.Cascade);

            // TableSession 1 - n Order
            builder.HasMany(ts => ts.Orders)
                .WithOne(o => o.TableSession)
                .HasForeignKey(o => o.TableSessionId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
