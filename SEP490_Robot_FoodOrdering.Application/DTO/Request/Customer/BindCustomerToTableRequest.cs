using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEP490_Robot_FoodOrdering.Application.DTO.Request.Customer
{
    public sealed class BindCustomerToTableRequest
    {
        public string PhoneNumber { get; set; } = default!;
        public string Name { get; set; } = default!;
    }
}
