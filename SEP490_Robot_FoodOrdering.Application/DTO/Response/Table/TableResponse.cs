using System;
using SEP490_Robot_FoodOrdering.Domain.Enums;

namespace SEP490_Robot_FoodOrdering.Application.DTO.Response.Table
{
    public class TableResponse
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Status { get; set; }
    }
} 