using Microsoft.AspNetCore.SignalR;
using SEP490_Robot_FoodOrdering.API.Hubs;
using SEP490_Robot_FoodOrdering.API.Services;
using SEP490_Robot_FoodOrdering.Application.Abstractions.Hubs;
using SEP490_Robot_FoodOrdering.Application.Service.Implementation;
using SEP490_Robot_FoodOrdering.Application.Service.Interface;

namespace SEP490_Robot_FoodOrdering.API.Extentions
{
    /// <summary>
    /// Extension methods for configuring SignalR and notification services
    /// </summary>
    public static class SignalRExtensions
    {
        /// <summary>
        /// Adds SignalR notification services with the proper hub configuration
        /// </summary>
        /// <param name="services">The service collection</param>
        /// <returns>The service collection for chaining</returns>
        public static IServiceCollection AddSignalRNotifications(this IServiceCollection services)
        {
            // Register the notification service with a factory that provides the correct hub context
            services.AddScoped<INotificationService>(serviceProvider =>
            {
                var hubContext = serviceProvider.GetRequiredService<IHubContext<OrderNotificationHub>>();
                var logger = serviceProvider.GetRequiredService<ILogger<OrderNotificationService>>();
                
                // Create the specialized service that works with the OrderNotificationHub
                return new OrderNotificationService(hubContext, logger);
            });
            services.AddScoped<IOrderStatsQuery, OrderStatsQuery>();
            services.AddScoped<IModeratorDashboardRefresher, ModeratorDashboardRefresher>();

            // Notifier (API)
            services.AddScoped<IModeratorDashboardNotifier, ModeratorDashboardNotifier>();

            


            return services;
        }
    }
}
