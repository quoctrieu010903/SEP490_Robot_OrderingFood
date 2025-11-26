using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEP490_Robot_FoodOrdering.Application.DTO.Response.Customer
{
    public sealed class BindCustomerToTableResult
    {
        public Guid CustomerId { get; init; }
        public string Name { get; init; } = default!;
        public string PhoneNumber { get; init; } = default!;
        public Guid TableId { get; init; }
        public Guid SessionId { get; init; }
        public Guid? OrderId { get; init; }
        public DateTime CreatAt { get; init; }
    }
}
