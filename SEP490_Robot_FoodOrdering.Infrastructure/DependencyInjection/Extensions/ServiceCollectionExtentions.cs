

using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SEP490_Robot_FoodOrdering.Application.Abstractions.Email;
using SEP490_Robot_FoodOrdering.Application.Abstractions.Utils;
using SEP490_Robot_FoodOrdering.Application.Utils;
using SEP490_Robot_FoodOrdering.Domain.Interface;
using SEP490_Robot_FoodOrdering.Infrastructure.Data.Persistence;
using SEP490_Robot_FoodOrdering.Infrastructure.Email;
using SEP490_Robot_FoodOrdering.Infrastructure.Repository;

namespace SEP490_Robot_FoodOrdering.Infrastructure.DependencyInjection.Extensions
{
    public static class ServiceCollectionExtentions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {

            var connectionString = configuration.GetConnectionString("DefaultConnection");
            services.AddDbContext<RobotFoodOrderingDBContext>(options =>
                     options.UseNpgsql(configuration.GetConnectionString("ConnectionStrings.DefaultConnection")));

            // Dependency Injection 
            services.AddScoped(typeof(IGenericRepository<,>), typeof(GenericRepository<,>));

            services.AddScoped<IUnitOfWork, UnitOfWork>();
            // Add Scope for the third 's  Service

            services.AddScoped<IUtilsService, UtilService>();

            services.AddScoped<IEmailService , EmailService>();



            // Add Auto Mapper




            return services;

        }
    }
}
