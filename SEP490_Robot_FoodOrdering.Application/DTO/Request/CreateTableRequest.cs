using System;
using SEP490_Robot_FoodOrdering.Domain.Enums;

namespace SEP490_Robot_FoodOrdering.Application.DTO.Request
{
    public class CreateTableRequest
    {
        public TableEnums Status { get; set; }
    }
} 