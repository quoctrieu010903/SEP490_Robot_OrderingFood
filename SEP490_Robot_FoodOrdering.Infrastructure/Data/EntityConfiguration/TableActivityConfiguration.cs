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
    public class TableActivityConfiguration : IEntityTypeConfiguration<TableActivity>
    {
        public void Configure(EntityTypeBuilder<TableActivity> builder)
        {
            // TableActivity n - 1 TableSession
            builder.HasOne(a => a.TableSession)
                .WithMany(ts => ts.Activities)
                .HasForeignKey(a => a.TableSessionId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}