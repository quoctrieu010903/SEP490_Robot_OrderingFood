namespace SEP490_Robot_FoodOrdering.Domain.Entities
{
    using global::SEP490_Robot_FoodOrdering.Core.Base;
   

    namespace SEP490_Robot_FoodOrdering.Domain.Entities
    {
        public class Feedback : BaseEntity
        {
            public Guid TableId { get; set; }
            public virtual Table Table { get; set; }

            public Guid OrderItemId { get; set; }
            public virtual OrderItem? OrderItem { get; set; }

            public int Rating { get; set; } // 1–5 sao
            public string? Comment { get; set; } // “Món ngon, phục vụ tốt”
            public FeedbackTypeEnum Type { get; set; } = FeedbackTypeEnum.Food; // Food, Service, Environment,...
            public FeedbackAction Action { get; set; } = FeedbackAction.None; // Hành động đề xuất từ phản hồi


        }

        public enum FeedbackTypeEnum
        {
            Food = 0, 
            Service = 1,
            Environment = 2,
            Other = 3
        }
        // create 3 vs feedback action 1 -> invoice request checkout  -> thanh toans 
        public enum FeedbackAction
        {
            None = 0,
            RequestCheckOut = 1,
        }
    }

}