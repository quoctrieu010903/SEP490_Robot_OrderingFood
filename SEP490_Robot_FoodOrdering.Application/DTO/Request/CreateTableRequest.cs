using System;
using SEP490_Robot_FoodOrdering.Domain.Enums;

namespace SEP490_Robot_FoodOrdering.Application.DTO.Request
{
    public class CreateTableRequest
    {
        public string Name { get; set; }
        public TableEnums Status { get; set; } = TableEnums.Available; // M?c ??nh tr?ng thái là Available
    }
} 