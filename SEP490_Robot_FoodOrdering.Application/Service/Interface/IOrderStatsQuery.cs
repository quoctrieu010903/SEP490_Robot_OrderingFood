using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SEP490_Robot_FoodOrdering.Application.DTO.Response;

namespace SEP490_Robot_FoodOrdering.Application.Service.Interface
{
    public interface IOrderStatsQuery
    {
        Task<Dictionary<Guid, OrderStaticsResponse>> GetOrderStatsByTableIdsAsync(IEnumerable<Guid> tableIds);

    }
}
