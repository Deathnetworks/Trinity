using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Zeta.Common;
using Zeta.Internals.Actors;

namespace Trinity.Combat.Abilities
{
    class SpellHistoryItem
    {
        public TrinityPower Power { get; set; }
        public DateTime UseTime { get; set; }
        public TimeSpan TimeSinceUse
        {
            get
            {
                return DateTime.Now.Subtract(UseTime);
            }
        }

        public TimeSpan TimeDistanceFrom(SpellHistoryItem other)
        {
            if (other.UseTime < this.UseTime)
                return other.UseTime.Subtract(this.UseTime);
            else
                return this.UseTime.Subtract(other.UseTime);
        }

    }
}
