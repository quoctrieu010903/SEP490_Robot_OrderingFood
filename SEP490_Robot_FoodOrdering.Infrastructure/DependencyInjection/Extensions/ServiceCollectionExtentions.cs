using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SEP490_Robot_FoodOrdering.Application.Abstractions.Email;
using SEP490_Robot_FoodOrdering.Application.Abstractions.Utils;
using SEP490_Robot_FoodOrdering.Application.Service.Implementation;
using SEP490_Robot_FoodOrdering.Application.Service.Interface;
using SEP490_Robot_FoodOrdering.Application.Utils;
using SEP490_Robot_FoodOrdering.Domain.Interface;
using SEP490_Robot_FoodOrdering.Infrastructure.Data.Persistence;
using SEP490_Robot_FoodOrdering.Infrastructure.DependencyInjection.Options;
using SEP490_Robot_FoodOrdering.Infrastructure.Email;
using SEP490_Robot_FoodOrdering.Infrastructure.Repository;
using SEP490_Robot_FoodOrdering.Infrastructure.Seeder;


namespace SEP490_Robot_FoodOrdering.Infrastructure.DependencyInjection.Extensions
{
    public static class ServiceCollectionExtentions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services,
            IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            services.AddDbContext<RobotFoodOrderingDBContext>(options =>
                options.UseNpgsql(connectionString));

            // Dependency Injection 
            services.AddScoped(typeof(IGenericRepository<,>), typeof(GenericRepository<,>));

            services.AddScoped<IToppingRepository, ToppingRepository>();
            services.AddScoped<IOrderItemReposotory, OrderItemReposotory>();
            services.AddScoped<IFeedbackService, FeedbackService>();
            services.AddSingleton<FeedbackMemoryStore>();

            // services.AddSingleton<IFeedbackService, FeedbackService>();

            services.AddScoped<IUnitOfWork, UnitOfWork>();
            // Add Scope for the third 's  Service
            services.AddScoped<IRobotFoodSeeder, RobotFoodSeeder>();

            services.AddScoped<IUtilsService, UtilService>();

<<<<<<< HEAD
            services.AddScoped<IEmailService , EmailService>();
            services.Configure<CloudinaryOptions>(configuration.GetSection(nameof(CloudinaryOptions)));
            services.Configure<EmailOptions>(configuration.GetSection(nameof(EmailOptions)));
           


=======
            services.AddScoped<IEmailService, EmailService>();
>>>>>>> ce20fe3ed229cf05efa6d8de36ac07ab9d16a429


            // Add Auto Mapper


            return services;
        }
    }
}