using SEP490_Robot_FoodOrdering.Application.DTO.Response.Topping;

public class RemakeOrderItemsResponse
{
    public Guid RemakeOrderItemId { get; set; } // ID bản ghi remake
    public Guid OriginalOrderItemId { get; set; } // ID món gốc

    // ===== Thông tin món ăn chính =====
    public string ProductName { get; set; } = string.Empty;
    public string ProductSize { get; set; } = string.Empty;
    public decimal ProductPrice { get; set; } // Giá món chính

    // ===== Topping =====
    public List<ToppingResponse> Toppings { get; set; } = new(); // Danh sách topping đi kèm

    // ===== Thông tin remake =====
    public int RemakeCount { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string? PreviousStatus { get; set; }
    public string CurrentStatus { get; set; } = string.Empty;

    // ===== Thông tin hệ thống =====
    public string? CreatedByName { get; set; } // Ai yêu cầu remake
    public DateTime CreatedTime { get; set; }

    public string? LastUpdatedByName { get; set; }
    public DateTime? LastUpdatedTime { get; set; }

    // ===== Thông tin đơn hàng =====
    public Guid OrderId { get; set; }
    public string TableName { get; set; } = string.Empty;
    public DateTime OrderCreatedTime { get; set; }
}
