using System;
using System.Linq;
using Zeta.CommonBot.Profile;
using Zeta.TreeSharp;
using Zeta.XmlEngine;

namespace Trinity.XmlTags
{
    // * TrinityUseOnce ensures a sequence of tags is only ever used once during this profile
    [XmlElement("TrinityUseOnce")]
    public class TrinityUseOnce : BaseComplexNodeTag
    {
        private Func<bool> funcConditionalProcess;
        private int iUniqueID;
        private int iMaxRedo;
        private string sDisablePrevious;

        protected override Composite CreateBehavior()
        {
            return new
                Decorator(ret => CheckNotAlreadyDone(ret),
                    new PrioritySelector(base.GetNodes().Select(b => b.Behavior).ToArray())
            );
        }

        public override bool GetConditionExec()
        {
            // See if we've EVER hit this ID before
            if (Trinity.hashUseOnceID.Contains(ID))
            {

                // See if we've hit it more than or equal to the max times before
                if (Trinity.dictUseOnceID[ID] >= Max || Trinity.dictUseOnceID[ID] < 0)
                    return false;

                // Add 1 to our hit count, and let it run this time
                Trinity.dictUseOnceID[ID]++;
                return true;
            }

            // Never hit this before, so create the entry and let it run

            // First see if we should disable all other ID's currently hit to prevent them ever being run again this run
            if (DisablePrevious != null && DisablePrevious.ToLower() == "true")
            {
                foreach (int thisid in Trinity.hashUseOnceID)
                {
                    if (thisid != ID)
                    {
                        Trinity.dictUseOnceID[thisid] = -1;
                    }
                }
            }

            // Now store the fact we have hit this ID and set up the dictionary entry for it
            Trinity.hashUseOnceID.Add(ID);
            Trinity.dictUseOnceID.Add(ID, 1);
            return true;
        }

        private bool CheckNotAlreadyDone(object object_0)
        {
            return !IsDone;
        }

        [XmlAttribute("id")]
        public int ID
        {
            get
            {
                return iUniqueID;
            }
            set
            {
                iUniqueID = value;
            }
        }

        [XmlAttribute("disableprevious")]
        public string DisablePrevious
        {
            get
            {
                return sDisablePrevious;
            }
            set
            {
                sDisablePrevious = value;
            }
        }

        [XmlAttribute("max")]
        public int Max
        {
            get
            {
                return iMaxRedo;
            }
            set
            {
                iMaxRedo = value;
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
