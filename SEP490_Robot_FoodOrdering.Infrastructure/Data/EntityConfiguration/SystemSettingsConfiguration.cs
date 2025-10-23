using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SEP490_Robot_FoodOrdering.Domain.Entities;

namespace SEP490_Robot_FoodOrdering.Infrastructure.Data.EntityConfiguration
{
    internal class SystemSettingsConfiguration : IEntityTypeConfiguration<SystemSettings>
    {
        public void Configure(EntityTypeBuilder<SystemSettings> builder)
        {
            builder.Property(s => s.Key)
                   .IsRequired()
                   .HasMaxLength(100);

            builder.Property(s => s.Value)
                   .IsRequired();

            builder.Property(s => s.Type)
                   .IsRequired()
                   .HasConversion<int>();

            builder.HasIndex(s => s.Key)
                   .IsUnique();
        }
    }
}


