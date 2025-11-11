using SEP490_Robot_FoodOrdering.Core.Base;
using SEP490_Robot_FoodOrdering.Domain.Enums;

namespace SEP490_Robot_FoodOrdering.Domain.Entities
{
    public class SystemSettings : BaseEntity
    {
        //public PaymentPolicy PaymentPolicy { get; set; }
        public string Key { get; set; }
        public string Value { get; set; }
        public SettingType Type { get; set; }
        public string DisplayName { get; set; } = default!; 
        public string? Description { get; set; }            
    }
}


