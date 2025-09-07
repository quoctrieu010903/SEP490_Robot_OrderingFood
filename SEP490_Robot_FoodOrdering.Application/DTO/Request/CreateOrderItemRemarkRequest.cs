

namespace SEP490_Robot_FoodOrdering.Application.DTO.Request
{
    public class CreateOrderItemRemarkRequest
    {
        public Guid OrderItemId { get; set; }
        public string RemarkNote { get; set; } = string.Empty;

    }
}
