using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SEP490_Robot_FoodOrdering.API.Extensions;
using SEP490_Robot_FoodOrdering.API.Extentions;
using SEP490_Robot_FoodOrdering.API.Hubs;
using SEP490_Robot_FoodOrdering.API.Middleware;
using SEP490_Robot_FoodOrdering.Application.Extentions;
using SEP490_Robot_FoodOrdering.Domain.Interface;
using SEP490_Robot_FoodOrdering.Infrastructure.DependencyInjection.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Load .env and bind to configuration via extension
builder.LoadDotEnv();

// Add PayOS PayOS SDK client
var payOS = new Net.payOS.PayOS(
    builder.Configuration["Environment:PAYOS_CLIENT_ID"] ?? throw new Exception("Missing PAYOS_CLIENT_ID"),
    builder.Configuration["Environment:PAYOS_API_KEY"] ?? throw new Exception("Missing PAYOS_API_KEY"),
    builder.Configuration["Environment:PAYOS_CHECKSUM_KEY"] ?? throw new Exception("Missing PAYOS_CHECKSUM_KEY")
);
builder.Services.AddSingleton(payOS);

// Needed if you build return/cancel URLs from the current request
builder.Services.AddHttpContextAccessor();

builder.Services.AddControllers();


builder.Services.AddInfrastructure(builder.Configuration)
                .AddApplication(builder.Configuration)
                .AddJwtAuthentication(builder.Configuration)
                .AddSwagger();
builder.Services.AddSingleton<IConfigureOptions<JsonOptions>, JsonOptionsConfigurator>();

// Add SignalR
builder.Services.AddSignalR();

// Add SignalR notification services
builder.Services.AddSignalRNotifications();

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
    .WithOrigins("http://192.168.110.46:3000", "http://localhost:5235", "https://localhost:5235")
    .AllowCredentials());

app.UseAuthentication();
app.UseAuthorization();

app.UseStaticFiles();

app.UseMiddleware<CustomExceptionHandlerMiddleware>();

// Map SignalR Hub
app.MapHub<OrderNotificationHub>("/orderNotificationHub");

app.MapControllers();
app.Logger.LogInformation("ContentRootPath: {ContentRoot}", app.Environment.ContentRootPath);
app.Run();
