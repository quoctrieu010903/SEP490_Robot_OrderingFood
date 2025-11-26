

namespace SEP490_Robot_FoodOrdering.Application.Service.Interface
{
    public interface ICustomerPointService
    {

        Task AwardPointsForInvoiceAsync(Guid invoiceId);
    }
}
