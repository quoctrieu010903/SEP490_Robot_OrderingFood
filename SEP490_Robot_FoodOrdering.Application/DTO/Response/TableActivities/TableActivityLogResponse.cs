using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SEP490_Robot_FoodOrdering.Domain.Enums;

namespace SEP490_Robot_FoodOrdering.Application.DTO.Response.TableActivities
{
    public class TableActivityLogResponse
    {
        public Guid TableSessionId { get; set; }
        public string? DeviceId { get; set; }
        public string? Type { get; set; }
        public string? ActivityCode { get; set; }

        // Cho phép kiểu object để bind JSON con
        public object? Data { get; set; }

        public DateTime CreatedTime { get; set; }
    }
}
