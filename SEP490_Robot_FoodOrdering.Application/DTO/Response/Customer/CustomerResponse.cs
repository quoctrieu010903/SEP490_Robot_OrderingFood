using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEP490_Robot_FoodOrdering.Application.DTO.Response.Customer
{
    public sealed class CustomerResponse
    {
        public Guid Id { get; init; }
        public string PhoneNumber { get; init; } = default!;
        public string Name { get; init; } = default!;
        public int TotalPoints { get; init; }
        public int LifetimePoints { get; init; }
        public DateTime? CreatedTime { get; init; }
        public DateTime? LastUpdatedTime { get; init; }
    }
}
