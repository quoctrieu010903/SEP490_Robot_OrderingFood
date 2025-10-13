using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using SEP490_Robot_FoodOrdering.Core.Base;

namespace SEP490_Robot_FoodOrdering.Domain.Entities
{
    public class CancelledOrderItem : BaseEntity
    {
        public string Reason { get; set; }
        public string? Note { get; set; }

        public decimal ItemPrice { get; set; }
        public decimal OrderTotalBefore { get; set; }
        public decimal OrderTotalAfter { get; set; }


        public Guid OrderItemId { get; set; }
        public Guid CancelledByUserId { get; set; }

        public virtual OrderItem OrderItem { get; set; }
        public virtual User CancelledByUser { get; set; }
    }



}
