using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SEP490_Robot_FoodOrdering.Domain.Entities;

namespace SEP490_Robot_FoodOrdering.Infrastructure.Data.EntityConfiguration
{
    public class QuickServeItemConfiguration : IEntityTypeConfiguration<QuickServeItem>
    {
        public void Configure(EntityTypeBuilder<QuickServeItem> builder)
        {
            builder.HasOne(q => q.Complain)
                   .WithMany(c => c.QuickServeItems)
                   .HasForeignKey(q => q.ComplainId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }
}

