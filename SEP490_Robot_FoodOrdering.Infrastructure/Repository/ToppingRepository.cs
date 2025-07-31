using Microsoft.EntityFrameworkCore;
using SEP490_Robot_FoodOrdering.Domain.Entities;
using SEP490_Robot_FoodOrdering.Domain.Interface;
using SEP490_Robot_FoodOrdering.Infrastructure.Data.Persistence;

namespace SEP490_Robot_FoodOrdering.Infrastructure.Repository;

public class ToppingRepository : GenericRepository<ProductTopping, Guid>, IToppingRepository
{
    private readonly RobotFoodOrderingDBContext _context;
    private readonly DbSet<ProductTopping> _dbSet;

    public ToppingRepository(RobotFoodOrderingDBContext context) : base(context)
    {
        _context = context;
        _dbSet = context.Set<ProductTopping>();
    }

    public async Task<List<Topping>> getToppingbyProductionId(Guid id)
    {
        var temp = await _dbSet.Include(topping => topping.Topping).
            Where(topping => topping.ProductId.Equals(id)).ToListAsync();

        return temp.Select(topping => topping.Topping).ToList();
    }
}