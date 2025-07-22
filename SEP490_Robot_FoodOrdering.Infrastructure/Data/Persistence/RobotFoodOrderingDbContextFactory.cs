using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace SEP490_Robot_FoodOrdering.Infrastructure.Data.Persistence;

public class RobotFoodOrderingDbContextFactory : IDesignTimeDbContextFactory<RobotFoodOrderingDBContext>
{
    public RobotFoodOrderingDBContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<RobotFoodOrderingDBContext>();
        optionsBuilder.UseNpgsql("Host=turntable.proxy.rlwy.net;Port=46357;Database=railway;Username=postgres;Password=SnADgLOXwxwTMxuvXWQXItKtKNGcGKSp");
        return new RobotFoodOrderingDBContext(optionsBuilder.Options);
    }
}