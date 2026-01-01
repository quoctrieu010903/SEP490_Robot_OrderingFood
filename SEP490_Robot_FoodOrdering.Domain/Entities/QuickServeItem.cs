using SEP490_Robot_FoodOrdering.Core.Base;

namespace SEP490_Robot_FoodOrdering.Domain.Entities
{
    public class QuickServeItem : BaseEntity
    {
        public Guid ComplainId { get; set; }
        public virtual Complain Complain { get; set; }

        public string ItemName { get; set; } = string.Empty; // Ví dụ: "Nước mắm", "Nước tương"
    }
}

