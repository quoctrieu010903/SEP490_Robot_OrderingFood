using SEP490_Robot_FoodOrdering.Application.DTO.Response.Topping;

namespace SEP490_Robot_FoodOrdering.Application.DTO.Response.Order
{
    public class OrderResponse
    {
        public Guid Id { get; set; }
        public Guid? TableId { get; set; }
        public string TableName { get; set; }
        public string deviderId { get; set; }
        public string Status { get; set; }
        public string OrderCode { get; set; }
        public string PaymentStatus { get; set; }
        public decimal TotalPrice { get; set; }
        public decimal TotalPaid { get; set; }
        

        public DateTime CreatedTime { get; set; }
        public DateTime? LastUpdatedTime { get; set; }
        
        public Guid? DeviceTokenId { get; set; }
        public List<OrderItemResponse> Items { get; set; }
    }

    public class OrderItemResponse
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public string ProductName { get; set; }
        public Guid ProductSizeId { get; set; }
        public string SizeName { get; set; }
        public string Note { get; set; }
        public string RemarkNote { get; set; } = string.Empty;
        public string PaymentStatus { get; set; }
        public decimal Price { get; set; } // Price field for individual item
        public string Status { get; set; }
        public string? ImageUrl { get; set; }
        public DateTime CreatedTime { get; set; }
        public bool IsUrgent { get; set; } 
        public List<ToppingResponse> Toppings { get; set; }
        
        // Thời gian chuyển trạng thái
        public DateTime? ReadyTime { get; set; } // Thời gian chuyển sang trạng thái Ready (Preparing → Ready)
        public DateTime? ServedTime { get; set; } // Thời gian chuyển sang trạng thái Served (Ready → Served)
        public DateTime? CancelledTime { get; set; } // Thời gian hủy món
        public DateTime? RemakedTime { get; set; } // Thời gian làm lại món (chuyển sang Remark)
      
    }
  

}