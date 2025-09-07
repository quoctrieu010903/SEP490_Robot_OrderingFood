namespace SEP490_Robot_FoodOrdering.Application.DTO.Response.Invouce;

public class InvoiceDetailResponse
{
    public Guid OrderItemId { get; set; }
    public string ProductName { get; set; }
    public decimal UnitPrice { get; set; }
    public List<string> Toppings { get; set; }
    public decimal TotalMoney { get; set; }
}

public class InvoiceResponse
{
    public Guid Id { get; set; }
    public string TableName { get; set; }
    public DateTime CreatedTime { get; set; }
    public decimal TotalMoney { get; set; }
    public string PaymentStatus { get; set; }
    public List<InvoiceDetailResponse> Details { get; set; }
}
