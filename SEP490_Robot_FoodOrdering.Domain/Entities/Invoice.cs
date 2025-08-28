using SEP490_Robot_FoodOrdering.Core.Base;
using SEP490_Robot_FoodOrdering.Domain.Enums;

namespace SEP490_Robot_FoodOrdering.Domain.Entities;

public class Invoice: BaseEntity
{
    public Table Table { get; set; }
    public decimal totalMoney { get; set; }        
    public PaymentStatusEnums status { get; set; }        
    public PaymentMethodEnums PhuongThucThanhToan { get; set; }
    public ICollection<InvoiceDetail>  Details { get; set; }
    
}

public class InvoiceDetail : BaseEntity
{
    // neu ma khach chon thi set OrderStatus thanh da huy va totol monney = 0 OK 
    public OrderItem OrderItem { get; set; }
    public decimal totalMoney { get; set; }
    public OrderStatus Status { get; set; }
    public Invoice Invoice { get; set; }
}