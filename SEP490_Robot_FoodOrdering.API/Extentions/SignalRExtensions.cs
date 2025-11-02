using Microsoft.AspNetCore.SignalR;
using SEP490_Robot_FoodOrdering.API.Hubs;
using SEP490_Robot_FoodOrdering.Application.Abstractions.Hub;
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
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", builder =>
                {
                    builder
                        .WithOrigins("http://localhost:3000") // ?? frontend port c?a b?n
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .AllowCredentials(); // ?? b?t bu?c cho SignalR
                });
            });

            services.AddSignalR();

            return services;
        }
        public static IApplicationBuilder UseAppSignalR(this IApplicationBuilder app)
        {
            app.UseCors("AllowAll");

            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                // Map các Hub t?i ?ây
                endpoints.MapHub<OrderItemNotificationHub>("/api/orderNotificationHub");
            });

            return app;
        }
    }
}
