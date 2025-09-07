using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SEP490_Robot_FoodOrdering.Domain.Enums;

namespace SEP490_Robot_FoodOrdering.Application.DTO.Request
{
    public class TableStatusChangeNotification
    {
        public Guid TableId { get; set; }
        public string TableName { get; set; } = string.Empty;
        public TableEnums OldStatus { get; set; }
        public TableEnums NewStatus { get; set; }
        public string? Reason { get; set; }
        public string UpdatedBy { get; set; } = string.Empty;
        public DateTime UpdatedAt { get; set; }
        public string NotificationType { get; set; } = "TableStatusChanged";
    }
}
