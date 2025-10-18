using SEP490_Robot_FoodOrdering.Domain.Enums;

namespace SEP490_Robot_FoodOrdering.Application.DTO.Request.Complain;

public class OrderItemDTO
{
    public Guid OrderItemId { get; set; }
    public string OrderItemName { get; set; }
    public string ImageUrl { get; set; }

    public OrderItemStatus Status { get; set; }


    public OrderItemDTO(Guid orderItemId, string orderItemName, string imageUrl, OrderItemStatus status)
    {
        OrderItemId = orderItemId;
        OrderItemName = orderItemName;
        ImageUrl = imageUrl;
        Status = status;
    }
}