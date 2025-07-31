using System.Reflection;
using Microsoft.OpenApi.Models;

namespace SEP490_Robot_FoodOrdering.API.Extentions
{
    public static class SwaggerFeatureExtensions
    {
        public static IServiceCollection AddSwagger(this IServiceCollection services)
        {
            services.AddSwaggerGen( c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "FPTU_Robot_FoodOrdering_API", Version = "v1" });

                // Configure JWT Authentication in Swagger
                c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
                {
                    Name = "Authorization",
                    Type = SecuritySchemeType.Http,  // Correct type for Bearer token
                    Scheme = "Bearer", // Capital 'B' to match JWT standard
                    BearerFormat = "JWT",
                    In = ParameterLocation.Header,
                    Description = "JWT Authorization header using the Bearer scheme. \r\n\r\n" +
                                  "Enter 'Bearer' [space] and then your token in the text input below.\r\n\r\n" +
                                  "Example: \"Bearer 1safsfsdfdfd\"",
                });

                c.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>() // Explicit empty array for scopes
                    }

                });

                // Include XML comments safely
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

                // Only include if file exists
                if (File.Exists(xmlPath))
                {
                    c.IncludeXmlComments(xmlPath);
                }
            });

            return services;
        }

        public static WebApplication WithSwagger(this WebApplication app)
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json",
                    "FPTU_Robot_FoodOrdering_API API V1");
                c.RoutePrefix = "swagger";
            });

            return app;
        }
    }

}
