using SEP490_Robot_FoodOrdering.Application.DTO.Response.Topping;

namespace SEP490_Robot_FoodOrdering.Application.DTO.Response.Invouce;

public class InvoiceDetailResponse
{
    public Guid OrderItemId { get; set; }
    public string ProductName { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalMoney { get; set; }
    public string Status { get; set; }
    public List<ToppingResponse> toppings { get; set; }
}

public class InvoiceResponse
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Guid TableId { get; set; }
    public string TableName { get; set; }
    public string InvoiceCode { get; set; }
    public DateTime CreatedTime { get; set; }
    public string PaymentMethod { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal Discount { get; set; }
    public decimal FinalAmount { get; set; }
    public string CashierName { get; set; }

    public List<InvoiceDetailResponse> Details { get; set; }    
}
