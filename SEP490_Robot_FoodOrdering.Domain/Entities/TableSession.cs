using SEP490_Robot_FoodOrdering.Core.Base;
using SEP490_Robot_FoodOrdering.Domain.Enums;
namespace SEP490_Robot_FoodOrdering.Domain.Entities
{
        /// Một lần nhóm khách ngồi 1 bàn
        public class TableSession : BaseEntity
        {
            public Guid TableId { get; set; }
            public virtual Table Table { get; set; } = null!;

            public string SessionToken { get; set; } = null!;
            public TableSessionStatus Status { get; set; } = TableSessionStatus.Active;

            // 1 bàn trong 1 session chỉ có 1 device giữ quyền
            public string? DeviceId { get; set; } = null!;
            public string? DeviceName { get; set; }

            // nếu khách có tài khoản / nhập SĐT
            public Guid? CustomerId { get; set; }
            public virtual Customer? Customer { get; set; }
            public string? SessionCode  { get; set; }
            public DateTime CheckIn { get; set; } = DateTime.UtcNow;
            public DateTime? CheckOut { get; set; }
            public DateTime? LastActivityAt { get; set; }

            public virtual ICollection<TableActivity> Activities { get; set; } = new List<TableActivity>();
            public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
            
        }
    }

