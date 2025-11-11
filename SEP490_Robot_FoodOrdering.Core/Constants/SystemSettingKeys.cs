using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEP490_Robot_FoodOrdering.Core.Constants
{
    public class SystemSettingKeys
    {
        public const string TableAutoReleaseMinutes = "TableAutoReleaseMinutes";
        public const string PaymentTimeoutMinutes = "PaymentTimeoutMinutes";
        public const string AllowMultiDeviceLogin = "AllowMultiDeviceLogin";
        public const string MaxOrderPerTable = "MaxOrderPerTable";
        public const string EnableOrderNotifications = "EnableOrderNotifications";
        public const string PaymentPolicy = "PaymentPolicy";
        public const string TableAccessTimeoutWithoutOrderMinutes = "TableAccessTimeoutWithoutOrderMinutes";
        public const string OrderCleanupAfterDays = "OrderCleanupAfterDays";

    }
}
