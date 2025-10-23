using System;

namespace SEP490_Robot_FoodOrdering.Application.DTO.Request.Feedback
{
    public class UpdateFeedbackRequest
    {
        public int? Rating { get; set; }
        public string? Comment { get; set; }
        public int? Type { get; set; }
    }
}


