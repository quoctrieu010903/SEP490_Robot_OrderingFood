using SEP490_Robot_FoodOrdering.Application.DTO.Request.fedback;

namespace SEP490_Robot_FoodOrdering.Application.DTO.Response.Feedback;

public class FeedbackModole
{
    public string Feedback { get; set; }
    public bool IsPeeding { get; set; }
    public DateTime CreatedTime { get; set; }
    public Guid IDFeedback { get; set; }

    public List<OrderItemDTO>? OrderItemDto { get; set; }

    public FeedbackModole(string feedback = null, bool isPeeding = default, DateTime createdTime = default,
        Guid idFeedback = default, List<OrderItemDTO>? orderItemDto = default)
    {
        Feedback = feedback;
        IsPeeding = isPeeding;
        CreatedTime = createdTime;
        IDFeedback = idFeedback;
        OrderItemDto = orderItemDto;
    }
    
    
}