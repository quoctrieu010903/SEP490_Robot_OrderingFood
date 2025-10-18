

using SEP490_Robot_FoodOrdering.Application.DTO.Request.Complain;

public class ComplainModel
{
    public string Feedback { get; set; }
    public bool IsPeeding { get; set; }
    public DateTime CreatedTime { get; set; }
    public Guid IDFeedback { get; set; }

    public List<OrderItemDTO>? OrderItemDto { get; set; }

    public string content { get; set; }

    public ComplainModel(string feedback = null, bool isPeeding = default, DateTime createdTime = default,
        string content = default,
        Guid idFeedback = default, List<OrderItemDTO>? orderItemDto = default)
    {
        Feedback = feedback;
        IsPeeding = isPeeding;
        CreatedTime = createdTime;
        IDFeedback = idFeedback;
        OrderItemDto = orderItemDto;
        content = content;
    }
}