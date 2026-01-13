using SEP490_Robot_FoodOrdering.Domain.Enums;

namespace SEP490_Robot_FoodOrdering.Application.DTO.Request.invoice;

public class InvoiceCreatRequest
{
    public Guid TableId { get; set; }
    public Guid OrderId { get; set; }
    public Guid? CustomerId { get; set; }
    public string? CustomerPhone { get; set; }
    public string? CustomerName { get; set; }

    public InvoiceCreatRequest(Guid tableId, Guid orderId)
    {
        TableId = tableId;
        OrderId = orderId;
    }
}