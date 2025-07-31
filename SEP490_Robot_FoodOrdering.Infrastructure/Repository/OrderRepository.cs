using Microsoft.EntityFrameworkCore;
using SEP490_Robot_FoodOrdering.Domain.Entities;
using SEP490_Robot_FoodOrdering.Domain.Interface;
using SEP490_Robot_FoodOrdering.Infrastructure.Data.Persistence;

namespace SEP490_Robot_FoodOrdering.Infrastructure.Repository;

public class OrderRepository : GenericRepository<Order, Guid>, IOrderRepository
{
    private readonly RobotFoodOrderingDBContext _context;
    private readonly DbSet<ProductTopping> _dbSet;

    public OrderRepository(RobotFoodOrderingDBContext context) : base(context)
    {
        _context = context;
        _dbSet = context.Set<ProductTopping>();
    }

    public Task<List<Order>> GetAllOrderWithPending(Guid idTable)
    {
        throw new NotImplementedException();
    }
}