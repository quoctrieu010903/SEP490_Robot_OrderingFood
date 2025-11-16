using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SEP490_Robot_FoodOrdering.Domain.Enums
{
    
    public enum TableActivityType
    {
        CheckIn = 0,
        ScanAgain = 1,

        CreateOrder = 10,
        AddOrderItems = 11,



        MoveTable = 30,

        ShareStart = 40,
        ShareJoin = 41,
        ShareStop = 42,

        RequestCheckout = 50,
        CreateInvoice = 50,
        CloseSession = 51,

        AutoRelease = 60,
        AttachDeviceFromModerator = 70,
    }

}

