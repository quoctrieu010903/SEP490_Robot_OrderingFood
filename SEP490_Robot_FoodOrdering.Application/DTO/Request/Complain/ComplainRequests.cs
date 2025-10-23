using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEP490_Robot_FoodOrdering.Application.DTO.Request.Complain
{
    public class ComplainRequests
    {
        public Guid TableId { get; set; }
        public string Title { get; set; }
        public string ComplainNote { get; set; }
        public List<Guid>?  OrderItemIds { get; set; }
    }
}
