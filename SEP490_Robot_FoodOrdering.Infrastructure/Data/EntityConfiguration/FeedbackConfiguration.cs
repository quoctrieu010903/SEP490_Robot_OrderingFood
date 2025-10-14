
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using SEP490_Robot_FoodOrdering.Domain.Entities.SEP490_Robot_FoodOrdering.Domain.Entities;

namespace SEP490_Robot_FoodOrdering.Infrastructure.Data.EntityConfiguration
{
    public class FeedbackConfiguration : IEntityTypeConfiguration<Feedback>
    {
        public void Configure(EntityTypeBuilder<Feedback> builder)
        {
            builder.HasOne(f => f.Table)
                   .WithMany(t => t.Feedbacks)
                   .HasForeignKey(f => f.TableId);

            builder.HasOne(f => f.OrderItem)
                   .WithMany(oi => oi.Feedbacks)
                   .HasForeignKey(f => f.OrderItemId);
        }
    }
}