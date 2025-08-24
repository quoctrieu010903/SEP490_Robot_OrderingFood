using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SEP490_Robot_FoodOrdering.Domain.Enums;

namespace SEP490_Robot_FoodOrdering.Application.DTO.Request
{
    public class UpdateStatusTable
    {
        public TableEnums Status { get; set; } // Trạng thái mới của bàn
        public string? Reason { get; set; } 
    }
}
