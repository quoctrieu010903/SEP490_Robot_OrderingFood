using Microsoft.EntityFrameworkCore;
using SEP490_Robot_FoodOrdering.Core.Base;
using SEP490_Robot_FoodOrdering.Domain.Entities;
using SEP490_Robot_FoodOrdering.Domain.Entities.SEP490_Robot_FoodOrdering.Domain.Entities;

namespace SEP490_Robot_FoodOrdering.Infrastructure.Data.Persistence
{
    public class RobotFoodOrderingDBContext : DbContext
    {
        public RobotFoodOrderingDBContext(DbContextOptions<RobotFoodOrderingDBContext> options)
            : base(options)
        {
        }
        public DbSet<Role> Roles { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
        public DbSet<CancelledOrderItem> CancelledOrderItems { get; set; }
        public DbSet<RemakeOrderItem> RemakeOrderItems { get; set; }
        public DbSet<OrderItemTopping> OrderItemToppings { get; set; }
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Product> Products { get; set; }
        public DbSet<ProductCategory> ProductCategories { get; set; }
        public DbSet<ProductSize> ProductSizes { get; set; }
        public DbSet<ProductTopping> ProductToppings { get; set; }
        public DbSet<Table> Tables { get; set; }
        public DbSet<TableSession> TableSessions { get; set; }
        public DbSet<TableActivity> TableActivitys { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Topping> Toppings { get; set; }

        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<SystemSettings> SystemSettings { get; set; }
        public DbSet<Complain> Complains { get; set; }
        public DbSet<QuickServeItem> QuickServeItems { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(RobotFoodOrderingDBContext).Assembly);

            base.OnModelCreating(modelBuilder);
        }
    }
}