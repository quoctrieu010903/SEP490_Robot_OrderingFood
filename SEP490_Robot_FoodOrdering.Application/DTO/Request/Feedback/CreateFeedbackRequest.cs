using System;

namespace SEP490_Robot_FoodOrdering.Application.DTO.Request.Feedback
{
    public class CreateFeedbackRequest
    {
        public Guid TableId { get; set; }
        public Guid OrderItemId { get; set; }
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public int Type { get; set; }
    }
}


