

using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SEP490_Robot_FoodOrdering.Domain.Entities;

namespace SEP490_Robot_FoodOrdering.Infrastructure.Data.EntityConfiguration
{
    public class TableConfiguration : IEntityTypeConfiguration<Table>
    {
        public void Configure(EntityTypeBuilder<Table> builder)
        {
            builder.Property(t => t.Status)
                  .IsRequired()
                  .HasConversion<int>();
         builder.HasMany(t => t.Sessions)
                 .WithOne(s => s.Table)
                 .HasForeignKey(s => s.TableId)
                 .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(t => t.Orders)
                 .WithOne(o => o.Table)
                 .HasForeignKey(o => o.TableId)
                 .OnDelete(DeleteBehavior.SetNull); // Nếu xóa bàn, order giữ lại

            // 1 - N với Feedback
            builder.HasMany(t => t.Feedbacks)
                  .WithOne(f => f.Table)
                  .HasForeignKey(f => f.TableId)
                  .OnDelete(DeleteBehavior.Cascade);

            // 1 - N với Complain
            builder.HasMany(t => t.Complains)
                .WithOne(c => c.Table)
                .HasForeignKey(c => c.TableId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
