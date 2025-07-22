using SEP490_Robot_FoodOrdering.API.Extentions;
using SEP490_Robot_FoodOrdering.Application.Extentions;
using SEP490_Robot_FoodOrdering.Domain.Specifications.Interface;
using SEP490_Robot_FoodOrdering.Infrastructure.DependencyInjection.Extensions;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.


builder.Services.AddControllers();


builder.Services.AddInfrastructure(builder.Configuration)
                .AddApplication(builder.Configuration)
                .AddSwagger();



// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

var scope = app.Services.CreateScope();
var seeder = scope.ServiceProvider.GetRequiredService<IRobotFoodOrderingSeeder>();
await seeder.Seed();


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
 
    app.UseSwagger();
    app.UseSwaggerUI();
}


app.UseHttpsRedirection();

app.UseAuthorization();
app.UseStaticFiles();

app.MapControllers();

app.Run();
