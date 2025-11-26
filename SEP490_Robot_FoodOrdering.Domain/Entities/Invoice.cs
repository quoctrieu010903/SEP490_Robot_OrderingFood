using SEP490_Robot_FoodOrdering.Core.Base;
using SEP490_Robot_FoodOrdering.Domain.Enums;

namespace SEP490_Robot_FoodOrdering.Domain.Entities;

public class Invoice: BaseEntity
{
    public Guid OrderId { get; set; }
    public virtual Order Order { get; set; }
    public Guid TableId { get; set; }
    public virtual Table Table { get; set; }
    public string InvoiceCode { get; set; } = $"INV{Guid.NewGuid():N}"[..8].ToUpper();

    public decimal TotalMoney { get; set; }        
    public PaymentStatusEnums Status { get; set; }        
    public PaymentMethodEnums PaymentMethod { get; set; }
    public Guid? CustomerId { get; set; }
    public virtual Customer? Customer { get; set; }

    public ICollection<InvoiceDetail>  Details { get; set; } = new List<InvoiceDetail>();

}

public class InvoiceDetail : BaseEntity
{
   public Guid InvoiceId { get; set; }
    public virtual Invoice Invoices { get; set; }


    public Guid OrderItemId { get; set; }
    public virtual OrderItem OrderItem { get; set; }
    public decimal TotalMoney { get; set; }
    public OrderStatus Status { get; set; }

}