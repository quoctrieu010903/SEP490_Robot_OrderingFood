using Microsoft.EntityFrameworkCore;
using SEP490_Robot_FoodOrdering.API.Extentions;
using SEP490_Robot_FoodOrdering.API.Middleware;
using SEP490_Robot_FoodOrdering.Application.Extentions;
using SEP490_Robot_FoodOrdering.Infrastructure.DependencyInjection.Extensions;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();


builder.Services.AddInfrastructure(builder.Configuration)
                .AddApplication(builder.Configuration)
                .AddSwagger();



// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
 
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseHttpsRedirection();

app.UseAuthorization();
app.UseStaticFiles();

app.UseMiddleware<CustomExceptionHandlerMiddleware>();


app.MapControllers();

app.Run();
