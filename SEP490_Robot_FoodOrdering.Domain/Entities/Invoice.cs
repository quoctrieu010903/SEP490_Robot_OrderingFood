﻿using SEP490_Robot_FoodOrdering.Core.Base;
using SEP490_Robot_FoodOrdering.Domain.Enums;

namespace SEP490_Robot_FoodOrdering.Domain.Entities;

public class Invoice: BaseEntity
{
    public Guid TableId { get; set; }
    public virtual Table Table { get; set; }
    public decimal TotalMoney { get; set; }        
    public PaymentStatusEnums Status { get; set; }        
    public PaymentMethodEnums PaymentMethod { get; set; }
    public ICollection<InvoiceDetail>  Details { get; set; }
    
}

public class InvoiceDetail : BaseEntity
{
    // neu ma khach chon thi set OrderStatus thanh da huy va totol monney = 0 OK 
   public Guid OrderItemId { get; set; }
    public virtual OrderItem OrderItem { get; set; }
    public decimal TotalMoney { get; set; }
    public OrderStatus Status { get; set; }
    public Invoice Invoice { get; set; }
}