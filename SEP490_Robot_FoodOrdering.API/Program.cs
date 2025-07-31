using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SEP490_Robot_FoodOrdering.API.Extentions;
using SEP490_Robot_FoodOrdering.API.Middleware;
using SEP490_Robot_FoodOrdering.Application.Extentions;
using SEP490_Robot_FoodOrdering.Domain.Interface;
using SEP490_Robot_FoodOrdering.Infrastructure.DependencyInjection.Extensions;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();


builder.Services.AddInfrastructure(builder.Configuration)
                .AddApplication(builder.Configuration)
                .AddSwagger();
builder.Services.AddSingleton<IConfigureOptions<JsonOptions>, JsonOptionsConfigurator>();


// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();


     var app = builder.Build();

    var scope = app.Services.CreateScope();
    var seeder = scope.ServiceProvider.GetRequiredService<IRobotFoodSeeder>();
    await seeder.Seed();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
 
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseHttpsRedirection();

app.UseCors(x => x
    .AllowAnyMethod()
    .AllowAnyHeader()
    .SetIsOriginAllowed(origin => true)
    .WithOrigins("http://192.168.110.46:3000")
    .AllowCredentials());

app.UseAuthorization();
app.UseStaticFiles();

app.UseMiddleware<CustomExceptionHandlerMiddleware>();


app.MapControllers();

app.Run();
