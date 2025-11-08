using Microsoft.Extensions.DependencyInjection;
using Quartz;

namespace SEP490_Robot_FoodOrdering.Infrastructure.DependencyInjection.Extensions
{
    public static class BackgroundJobsExtensions
    {
        public static IServiceCollection AddRobotFoodOrderingBackgroundJobs(
            this IServiceCollection services)
        {
            
            services.AddQuartz(q =>
            {
                q.UseMicrosoftDependencyInjectionJobFactory();

                var jobKey = new JobKey("DailyCleanupJob");

                q.AddJob<DailyCleanupJob>(opts => opts.WithIdentity(jobKey));

                // chạy mỗi ngày 00:05
                q.AddTrigger(t => t
                    .ForJob(jobKey)
                    .WithIdentity("DailyCleanupJob-trigger")
                    .WithCronSchedule("0 5 0 * * ?"));
            });

            services.AddQuartzHostedService(options =>
            {
                options.WaitForJobsToComplete = true;
            });

            return services;
        }
    }
}
