using DotNetEnv;
using Microsoft.AspNetCore.Builder;

namespace SEP490_Robot_FoodOrdering.API.Extentions
{
    public static class EnvironmentExtensions
    {
        public static void LoadDotEnv(this WebApplicationBuilder builder)
        {
            try
            {
                Env.Load();
            }
            catch
            {
                // Ignore if .env is missing; runtime env vars will still be used
            }

            // Ensure values loaded from .env are available to the configuration
            builder.Configuration.AddEnvironmentVariables();
        }
    }
}


