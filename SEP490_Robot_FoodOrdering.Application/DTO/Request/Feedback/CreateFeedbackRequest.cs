using System;
using SEP490_Robot_FoodOrdering.Domain.Entities.SEP490_Robot_FoodOrdering.Domain.Entities;

namespace SEP490_Robot_FoodOrdering.Application.DTO.Request.Feedback
{
    public class CreateFeedbackRequest
    {
        public Guid TableId { get; set; }
        public Guid OrderItemId { get; set; } // bor
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public FeedbackTypeEnum Type { get; set; }
        public FeedbackAction Action { get; set; } 
    }
}


