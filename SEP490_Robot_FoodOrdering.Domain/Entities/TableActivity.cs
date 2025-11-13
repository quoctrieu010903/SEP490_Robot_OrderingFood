using SEP490_Robot_FoodOrdering.Core.Base;
using SEP490_Robot_FoodOrdering.Domain.Entities.SEP490_Robot_FoodOrdering.Domain.Entities;
using SEP490_Robot_FoodOrdering.Domain.Enums;
using SEP490_Robot_FoodOrdering.Domain.Enums;
namespace SEP490_Robot_FoodOrdering.Domain.Entities
{
    public class TableActivity : BaseEntity
    {
        public Guid TableSessionId { get; set; }
        public virtual TableSession TableSession { get; set; } = null!;

        // device thực hiện action (có thể là device cũ hoặc mới)
        public string? DeviceId { get; set; }

        public TableActivityType Type { get; set; }

        // JSON: chứa info bổ sung tuỳ action
        // vd: { "oldDeviceId":"...", "newDeviceId":"..." }
        //     { "fromTableId":"...", "toTableId":"..." }
        //     { "shareToken":"..." }
        public string? Data { get; set; }
    }
}