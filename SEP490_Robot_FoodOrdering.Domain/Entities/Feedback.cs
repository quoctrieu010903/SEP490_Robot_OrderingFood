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
            public virtual OrderItem OrderItem { get; set; }

            public int Rating { get; set; } // 1–5 sao
            public string? Comment { get; set; } // “Món ngon, phục vụ tốt”
            public FeedbackTypeEnum Type { get; set; } = FeedbackTypeEnum.Food; // Food, Service, Environment,...

            public Guid? CreatedBy { get; set; } // Khách nào gửi (nếu có định danh)
            public virtual User? Customer { get; set; }
        }

        public enum FeedbackTypeEnum
        {
            Food = 0,
            Service = 1,
            Environment = 2,
            Other = 3
        }
    }

}