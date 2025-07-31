namespace SEP490_Robot_FoodOrdering.Application.DTO.Response.Feedback;

public record FeedbackGet(Guid IdFeedback, Guid IdTable, string FeedBack, bool IsPeeding,DateTime CreateData);