
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

        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
        public virtual ICollection<Feedback> Feedbacks { get; set; }
        = new List<Feedback>();
        public virtual ICollection<Complain> Complains { get; set; } = new List<Complain>();
    }
}
