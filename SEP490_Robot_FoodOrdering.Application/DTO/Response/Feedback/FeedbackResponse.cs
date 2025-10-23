using System;

namespace SEP490_Robot_FoodOrdering.Application.DTO.Response.Feedback
{
    public class FeedbackResponse
    {
        public Guid Id { get; set; }
        public Guid TableId { get; set; }
        public Guid OrderItemId { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public int Type { get; set; }
        public DateTime CreatedTime { get; set; }
    }
}


