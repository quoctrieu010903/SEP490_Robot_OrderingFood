using SEP490_Robot_FoodOrdering.Domain.Entities;

namespace SEP490_Robot_FoodOrdering.Domain.Interface;

public interface IToppingRepository : IGenericRepository<ProductTopping,Guid>
{
    Task<List<Topping>> getToppingbyProductionId(Guid id);
}