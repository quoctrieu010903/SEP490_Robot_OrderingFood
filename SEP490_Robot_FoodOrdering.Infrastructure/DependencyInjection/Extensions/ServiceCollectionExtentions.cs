using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SEP490_Robot_FoodOrdering.Application.Abstractions.Cloudinary;
using SEP490_Robot_FoodOrdering.Application.Abstractions.Email;
using SEP490_Robot_FoodOrdering.Application.Abstractions.JWT;
using SEP490_Robot_FoodOrdering.Application.Abstractions.Options;
using SEP490_Robot_FoodOrdering.Application.Abstractions.Utils;
using SEP490_Robot_FoodOrdering.Application.Service.Implementation;
using SEP490_Robot_FoodOrdering.Application.Service.Interface;
using SEP490_Robot_FoodOrdering.Application.Utils;
using SEP490_Robot_FoodOrdering.Domain.Interface;
using SEP490_Robot_FoodOrdering.Infrastructure.Cloudinary;
using SEP490_Robot_FoodOrdering.Infrastructure.Data.Persistence;
using SEP490_Robot_FoodOrdering.Infrastructure.DependencyInjection.Options;
using SEP490_Robot_FoodOrdering.Infrastructure.Email;
using SEP490_Robot_FoodOrdering.Infrastructure.Jwt;
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
            services.AddScoped<IComplainService, ComplainService>();
            services.AddSingleton<FeedbackMemoryStore>();

            // services.AddSingleton<IFeedbackService, FeedbackService>();

            services.AddScoped<IUnitOfWork, UnitOfWork>();
            // Add Scope for the third 's  Service
            services.AddScoped<IRobotFoodSeeder, RobotFoodSeeder>();
            services.AddScoped<IJwtService, JwtService>();

            services.AddScoped<IUtilsService, UtilService>();


            services.Configure<CloudinaryOptions>(configuration.GetSection(nameof(CloudinaryOptions)));
            services.Configure<EmailOptions>(configuration.GetSection(nameof(EmailOptions)));
            services.Configure<VNPayOptions>(configuration.GetSection("VNPay"));
            services.AddScoped<IPaymentService, PaymentService>();
            services.AddScoped<ISettingsService, SettingsService>();
           


                services.AddScoped<IEmailService, EmailService>();
                services.AddSingleton<ICloudinaryService, CloudinaryService>();



            // Add Auto Mapper


            return services;
        }
    }
}