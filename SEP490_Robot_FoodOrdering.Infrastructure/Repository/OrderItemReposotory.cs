using Microsoft.EntityFrameworkCore;
using SEP490_Robot_FoodOrdering.Domain.Entities;
using SEP490_Robot_FoodOrdering.Domain.Interface;
using SEP490_Robot_FoodOrdering.Infrastructure.Data.Persistence;

namespace SEP490_Robot_FoodOrdering.Infrastructure.Repository;

public class OrderItemReposotory : GenericRepository<OrderItem, Guid>, IOrderItemReposotory
{
    private readonly RobotFoodOrderingDBContext _context;
    private readonly DbSet<OrderItem> _dbSet;

    public OrderItemReposotory(RobotFoodOrderingDBContext context) : base(context)
    {
        _context = context;
        _dbSet = context.Set<OrderItem>();
    }

    public Task<List<OrderItem>> GetAllOrderItemList(List<Guid> idOrderItemList)
    {
        var res = _dbSet
            .Include(item => item.ProductSize)
            .ThenInclude(size => size.Product)
            .Include(item => item.OrderItemTopping)
            .ThenInclude(pt => pt.Topping)
            .Include(item => item.Order)
            .Where(item => idOrderItemList.Contains(item.Id))
            .ToListAsync();

        return res;
    }
}