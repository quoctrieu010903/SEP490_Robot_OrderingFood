using SEP490_Robot_FoodOrdering.Core.Base;
using System.ComponentModel.DataAnnotations.Schema;

namespace SEP490_Robot_FoodOrdering.Domain.Entities
{
    public class Complain : BaseEntity
    {
        public Guid TableId { get; set; }
        public virtual Table Table { get; set; }

        public string Title { get; set; } = string.Empty; // ví dụ: “Món nguội”, “Phục vụ chậm”
        public string Description { get; set; } = string.Empty; // chi tiết khiếu nạ
        public bool isPending { get; set; } = false;

        public Guid? HandledBy { get; set; }
        public virtual User? Handler { get; set; }

        public DateTime? ResolvedAt { get; set; } // Thời điểm xử lý xong       
        public string? ResolutionNote { get; set; } // Ghi chú xử lý

        public virtual ICollection<QuickServeItem> QuickServeItems { get; set; } = new List<QuickServeItem>();
    }

    public enum ComplainStatusEnum
    {
        Pending = 0,
        InProgress = 1,
        Resolved = 2,
        Rejected = 3
    }
}
