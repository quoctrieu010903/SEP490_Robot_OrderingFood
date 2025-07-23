

using Microsoft.EntityFrameworkCore;
using SEP490_Robot_FoodOrdering.Domain.Entities;

namespace SEP490_Robot_FoodOrdering.Infrastructure.Data.Persistence
{
    public class RobotFoodOrderingDBContext : DbContext
    {
        public RobotFoodOrderingDBContext(DbContextOptions<RobotFoodOrderingDBContext> options)
            : base(options) { }

        public DbSet<Category> Categories { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<OrderItemTopping> OrderItemToppings { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductCategory> ProductCategories { get; set; }
        public DbSet<ProductSize> ProductSizes { get; set; }
        public DbSet<ProductTopping> ProductToppings { get; set; }
        public DbSet<Table> Tables { get; set; }
        public DbSet<Topping> Toppings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(RobotFoodOrderingDBContext).Assembly);
            Seed(modelBuilder);
            base.OnModelCreating(modelBuilder);
        }

        public static void Seed(ModelBuilder modelBuilder)
        {
            // Example seed data
            var categoryId = Guid.NewGuid();
            var productId = Guid.NewGuid();
            var sizeId = Guid.NewGuid();
            var toppingId = Guid.NewGuid();
            var tableId = Guid.NewGuid();

            modelBuilder.Entity<Category>().HasData(new Category
            {
                Id = categoryId,
                Name = "Beverages",
                CreatedTime = DateTime.UtcNow,
                LastUpdatedTime = DateTime.UtcNow
            });

            modelBuilder.Entity<Product>().HasData(new Product
            {
                Id = productId,
                Name = "Coca Cola",
                Description = "Refreshing soft drink",
                DurationTime = 1,
                ImageUrl = "cocacola.jpg",
                CreatedTime = DateTime.UtcNow,
                LastUpdatedTime = DateTime.UtcNow
            });

            modelBuilder.Entity<ProductCategory>().HasData(new ProductCategory
            {
                Id = Guid.NewGuid(),
                ProductId = productId,
                CategoryId = categoryId
            });

            modelBuilder.Entity<ProductSize>().HasData(new ProductSize
            {
                Id = sizeId,
                SizeName = SEP490_Robot_FoodOrdering.Domain.Enums.SizeNameEnum.Medium,
                Price = 1.99m,
                ProductId = productId,
                CreatedTime = DateTime.UtcNow,
                LastUpdatedTime = DateTime.UtcNow
            });

            modelBuilder.Entity<Topping>().HasData(new Topping
            {
                Id = toppingId,
                Name = "Ice",
                CreatedTime = DateTime.UtcNow,
                LastUpdatedTime = DateTime.UtcNow
            });

            modelBuilder.Entity<ProductTopping>().HasData(new ProductTopping
            {
                Id = Guid.NewGuid(),
                ProductId = productId,
                ToppingId = toppingId,
                CreatedTime = DateTime.UtcNow,
                LastUpdatedTime = DateTime.UtcNow
            });

            modelBuilder.Entity<Table>().HasData(new Table
            {
                Id = tableId,
                Name = tableId,
                Status = SEP490_Robot_FoodOrdering.Domain.Enums.TableEnums.Available,
                CreatedTime = DateTime.UtcNow,
                LastUpdatedTime = DateTime.UtcNow
            });
        }
    }
}
