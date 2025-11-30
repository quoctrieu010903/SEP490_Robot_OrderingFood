using Microsoft.Extensions.DependencyInjection;
using Quartz;
using SEP490_Robot_FoodOrdering.Infrastructure.BackgroundJob;

namespace SEP490_Robot_FoodOrdering.Infrastructure.DependencyInjection.Extensions
{
    public static class BackgroundJobsExtensions
    {
        public static IServiceCollection AddRobotFoodOrderingBackgroundJobs(
            this IServiceCollection services)
        {

            services.AddQuartz(q =>
            {

                var jobKey = new JobKey("DailyCleanupJob");
                q.AddJob<DailyCleanupJob>(opts => opts.WithIdentity(jobKey));

                var vnTz = TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh");

                q.AddTrigger(t => t
                    .ForJob(jobKey)
                    .WithIdentity("DailyCleanupJob-trigger")
                    .WithCronSchedule("0 5 00 * * ?", x => x
                        .InTimeZone(vnTz)
                        .WithMisfireHandlingInstructionDoNothing()
                    )
                );
            });

            services.AddQuartzHostedService(options =>
            {
                options.WaitForJobsToComplete = true;
            });

            services.AddHostedService<TableReleaseBackgroundService>();

            return services;
        }
    }
}
