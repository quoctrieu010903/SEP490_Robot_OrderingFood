using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SEP490_Robot_FoodOrdering.Domain.Entities;

namespace SEP490_Robot_FoodOrdering.Domain.Interface
{
    public interface IOrderItemReposotory : IGenericRepository<OrderItem, Guid>
    {
        Task<List<OrderItem>> GetAllOrderItemList(List<Guid> idOrderItemList);
    }
}