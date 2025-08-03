namespace SEP490_Robot_FoodOrdering.Application.DTO.Response.Feedback;

public record FeedbackPeedingInfo(string TableName , int Counter  , int DeliveredCount,
    int PaidCount,
    int TotalItems);