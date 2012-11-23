using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GilesTrinity.Notifications
{
    internal enum ProwlNotificationPriority : sbyte
    {
        VeryLow = -2,
        Moderate = -1,
        Normal = 0,
        High = 1,
        Emergency = 2
    }
}
