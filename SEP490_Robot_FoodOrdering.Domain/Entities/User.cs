using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SEP490_Robot_FoodOrdering.Core.Base;

namespace SEP490_Robot_FoodOrdering.Domain.Entities
{
    public class User : BaseEntity
    {
        public String EmploymentCode { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }
        public string FullName { get; set; }
        public string Avartar { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public bool IsActive { get; set; }
        public Guid RoleId { get; set; }
        public virtual Role Role { get; set; }

        public virtual ICollection<CancelledOrderItem> CancelledItems { get; set; } = new List<CancelledOrderItem>();
        public virtual ICollection<RemakeOrderItem> RemakeItems { get; set; } = new List<RemakeOrderItem>();
        public virtual ICollection<Complain> HandledComplaints { get; set; } = new List<Complain>();
        public virtual ICollection<Complain> CustomerComplaints { get; set; } = new List<Complain>();
        public virtual ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();
    }
}
