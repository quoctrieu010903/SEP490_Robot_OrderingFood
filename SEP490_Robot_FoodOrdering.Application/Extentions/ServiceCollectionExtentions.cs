using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SEP490_Robot_FoodOrdering.Application.Abstractions.Email;

namespace SEP490_Robot_FoodOrdering.Application.Extentions
{
    public static class ServiceCollectionExtentions
    {
        public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
        {
           
           
          
            return services;
        }
    } 
}
