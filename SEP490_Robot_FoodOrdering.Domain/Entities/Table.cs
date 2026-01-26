
using SEP490_Robot_FoodOrdering.Core.Base;
using SEP490_Robot_FoodOrdering.Domain.Entities.SEP490_Robot_FoodOrdering.Domain.Entities;
using SEP490_Robot_FoodOrdering.Domain.Enums;
namespace SEP490_Robot_FoodOrdering.Domain.Entities 
{
    public class Table : BaseEntity
    {
        public string Name { get; set; }
        public TableEnums Status { get; set; }
        public bool IsQrLocked { get; set; } = false;  
        public DateTime? LockedAt { get; set; } 
        public string? DeviceId { get; set; } 
        public DateTime? LastAccessedAt { get; set; }
        public string? ShareToken { get; set; }
        public bool isShared { get; set; } = false;
        
        /// <summary>
        /// Tọa độ X của bàn trên bản đồ nhà hàng (đơn vị: pixel)
        /// </summary>
        public int PositionX { get; set; } = 0;
        
        /// <summary>
        /// Tọa độ Y của bàn trên bản đồ nhà hàng (đơn vị: pixel)
        /// </summary>
        public int PositionY { get; set; } = 0;
        
        public virtual ICollection<TableSession> Sessions { get; set; } = new List<TableSession>();

        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
        public virtual ICollection<Feedback> Feedbacks { get; set; }
        = new List<Feedback>();
        public virtual ICollection<Complain> Complains { get; set; } = new List<Complain>();
    }
}
