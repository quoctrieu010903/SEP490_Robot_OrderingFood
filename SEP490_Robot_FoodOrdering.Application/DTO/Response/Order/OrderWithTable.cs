using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEP490_Robot_FoodOrdering.Application.DTO.Response.Order
{
    public class OrderWithTable
    {
        public Guid Id  { get; set; }
        public Guid TableId { get; set; }
        public string TableName { get; set; }
        public List<OrderResponse> Orders { get; set; }

    }
}
        