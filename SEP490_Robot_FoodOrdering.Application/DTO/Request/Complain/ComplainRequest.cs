

namespace SEP490_Robot_FoodOrdering.Application.DTO.Request.Complain
{
    public record class ComplainRequest(Guid IdTable,string Title ,string ComplainNote, List<Guid> OrderItemIds);
}
