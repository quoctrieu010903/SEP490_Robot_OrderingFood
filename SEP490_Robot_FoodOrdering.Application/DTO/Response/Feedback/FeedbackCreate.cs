namespace SEP490_Robot_FoodOrdering.Application.DTO.Response.Feedback;

public record FeedbackCreate(DateTime CreateTime, bool IsSuccess, string Message);

