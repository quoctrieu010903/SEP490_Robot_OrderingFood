using SEP490_Robot_FoodOrdering.Domain.Entities;

namespace SEP490_Robot_FoodOrdering.Domain.Interface;

public interface IOrderRepository : IGenericRepository<Order, Guid>
{

    Task<List<Order>> GetAllOrderWithPending(Guid idTable);

}