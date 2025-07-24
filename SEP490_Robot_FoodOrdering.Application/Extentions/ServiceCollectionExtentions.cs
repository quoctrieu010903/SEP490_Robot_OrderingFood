using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SEP490_Robot_FoodOrdering.Application.Abstractions.Email;
using SEP490_Robot_FoodOrdering.Application.Service.Implementation;
using SEP490_Robot_FoodOrdering.Application.Service.Interface;

namespace SEP490_Robot_FoodOrdering.Application.Extentions
{
    public static class ServiceCollectionExtentions
    {
        public static IServiceCollection AddApplication(this IServiceCollection services, IConfiguration configuration)
        {
           
             services.AddScoped<IProductService, ProductService>();
            services.AddScoped<ITableService, TableService>();
            services.AddScoped<IProductCategoryService, ProductCategoryService>();
            services.AddScoped<ICategoryService, CategoryService>();
            services.AddScoped<IProductCategoryService, ProductCategoryService>();

            services.AddScoped<IProductToppingService, ProductToppingService>();
            
             services.AddScoped<IProductSizeService, ProductSizeService>();
            services.AddScoped<IToppingService, ToppingService>();

            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());



            return services;
        }
    } 
}
