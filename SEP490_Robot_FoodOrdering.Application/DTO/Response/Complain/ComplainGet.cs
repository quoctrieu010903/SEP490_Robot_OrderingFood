using SEP490_Robot_FoodOrdering.Application.DTO.Request.Complain;

public record ComplainGet(Guid IdFeedback, Guid IdTable, string FeedBack, bool IsPeeding,DateTime CreateData,List<OrderItemDTO>Dtos);