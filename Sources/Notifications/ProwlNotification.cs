using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GilesTrinity.Notifications
{
    internal struct ProwlNotification
    {
        public string Event { get; set; }
        public string Description { get; set; }
        public ProwlNotificationPriority Priority { get; set; }
    }
}
