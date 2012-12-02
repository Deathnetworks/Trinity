using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Zeta.Internals.Actors;

namespace GilesTrinity
{
    internal class ScheduledPower
    {
        public SNOPower Power
        {
            get;
            set;
        }

        public int Delay
        {
            get;
            set;
        }

        public DateTime LastUsage
        {
            get;
            set;
        }

        public Func<SNOPower, bool> Condition
        {
            get;
            set;
        }

        public override bool Equals(object obj)
        {
            return (obj is ScheduledPower && Power == ((ScheduledPower)obj).Power);            
        }
    }
}
