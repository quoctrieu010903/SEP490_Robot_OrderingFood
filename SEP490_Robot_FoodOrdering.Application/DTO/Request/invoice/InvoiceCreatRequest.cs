using SEP490_Robot_FoodOrdering.Domain.Enums;

namespace SEP490_Robot_FoodOrdering.Application.DTO.Request.invoice;

public class InvoiceCreatRequest
{
    public Guid tableId { get; set; }
    public StatusInvoice status { get; set; }
    
    public PaymentMethodEnums MethodEnums {get; set;}
}