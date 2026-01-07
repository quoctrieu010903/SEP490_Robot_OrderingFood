namespace SEP490_Robot_FoodOrdering.Application.DTO.Response.Complain
{
    public class QuickServeItemResponse
    {
        public Guid Id { get; set; }
        public Guid ComplainId { get; set; }
        public string ItemName { get; set; } = string.Empty; // Ví dụ: "Nước mắm", "Nước tương"
        public bool IsServed { get; set; }
        public DateTime CreatedTime { get; set; }
        public DateTime LastUpdatedTime { get; set; }
    }
}

