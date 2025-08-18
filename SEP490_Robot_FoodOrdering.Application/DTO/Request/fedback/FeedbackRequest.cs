namespace SEP490_Robot_FoodOrdering.Application.DTO.Request.fedback;

public record FeedbackRequest(Guid idTable, List<Guid>? idOrderItem, string note);