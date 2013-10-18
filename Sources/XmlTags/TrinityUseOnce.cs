using System;
using System.Collections.Generic;
using System.Linq;
using Zeta.TreeSharp;
using Zeta.XmlEngine;

namespace Trinity.XmlTags
{
    // * TrinityUseOnce ensures a sequence of tags is only ever used once during this profile
    [XmlElement("TrinityUseOnce")]
    public class TrinityUseOnce : BaseComplexNodeTag
    {
        // A list of "useonceonly" tags that have been triggered this xml profile
        public static HashSet<int> UseOnceIDs = new HashSet<int>();
        public static Dictionary<int, int> UseOnceCounter = new Dictionary<int, int>();

        private Func<bool> funcConditionalProcess;
        private int uniqueId;
        private int maxReuse;
        private string disablePrevious;

        protected override Composite CreateBehavior()
        {
            return new
                Decorator(ret => CheckNotAlreadyDone(),
                    new PrioritySelector(base.GetNodes().Select(b => b.Behavior).ToArray())
            );
        }

        public override bool GetConditionExec()
        {
            // See if we've EVER hit this ID before
            if (UseOnceIDs.Contains(ID))
            {
                // See if we've hit it more than or equal to the max times before
                if (UseOnceCounter[ID] >= Max || UseOnceCounter[ID] < 0)
                    return false;

                // Add 1 to our hit count, and let it run this time
                UseOnceCounter[ID]++;
                return true;
            }

            // Never hit this before, so create the entry and let it run

            // First see if we should disable all other ID's currently hit to prevent them ever being run again this run
            if (DisablePrevious != null && DisablePrevious.ToLower() == "true")
            {
                foreach (int id in UseOnceIDs)
                {
                    if (id != ID)
                    {
                        UseOnceCounter[id] = -1;
                    }
                }
            }

            // Now store the fact we have hit this ID and set up the dictionary entry for it
            UseOnceIDs.Add(ID);
            UseOnceCounter.Add(ID, 1);
            return true;
        }

        private bool CheckNotAlreadyDone()
        {
            return !IsDone;
        }

        [XmlAttribute("id")]
        public int ID
        {
            get
            {
                return uniqueId;
            }
            set
            {
                uniqueId = value;
            }
        }

        [XmlAttribute("disableprevious")]
        public string DisablePrevious
        {
            get
            {
                return disablePrevious;
            }
            set
            {
                disablePrevious = value;
            }
        }

        [XmlAttribute("max")]
        public int Max
        {
            get
            {
                return maxReuse;
            }
            set
            {
                maxReuse = value;
            }
        }

        public Func<bool> Conditional
        {
            get
            {
                return funcConditionalProcess;
            }
            set
            {
                funcConditionalProcess = value;
            }
        }

    }
}
