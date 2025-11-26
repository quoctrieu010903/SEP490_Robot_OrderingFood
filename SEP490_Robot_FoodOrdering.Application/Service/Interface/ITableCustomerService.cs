using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SEP490_Robot_FoodOrdering.Application.DTO.Request.Customer;
using SEP490_Robot_FoodOrdering.Application.DTO.Response.Customer;

namespace SEP490_Robot_FoodOrdering.Application.Service.Interface
{
    public interface ITableCustomerService
    {
        Task<BindCustomerToTableResult> BindCustomerToActiveSessionAsync(Guid tableId, string deviceId, BindCustomerToTableRequest req);
        Task<CustomerResponse?> GetActiveCustomerByDeviceIdAsync(string deviceId);
        Task EnsureCustomerReadyForCheckoutAsync(Guid tableId);

    }
}
