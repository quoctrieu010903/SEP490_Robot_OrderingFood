
using SEP490_Robot_FoodOrdering.Core.Base;
using SEP490_Robot_FoodOrdering.Domain.Entities.SEP490_Robot_FoodOrdering.Domain.Entities;

namespace SEP490_Robot_FoodOrdering.Domain.Entities
{
    public class Customer : BaseEntity
    {
        public string PhoneNumber { get; set; } = null!;
        public string? Name { get; set; }

        public int TotalPoints { get; set; } = 0;
        public int LifetimePoints { get; set; } = 0;

        public virtual ICollection<TableSession> TableSessions { get; set; } = new List<TableSession>();
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
        public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
    }
}
