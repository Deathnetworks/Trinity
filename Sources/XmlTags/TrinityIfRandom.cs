using System;
using Zeta.CommonBot.Profile;
using Zeta.TreeSharp;
using Zeta.XmlEngine;

namespace Trinity.XmlTags
{
    // TrinityIfRandom only runs the container stuff if the given id is the given value
    [XmlElement("TrinityIfRandom")]
    public class TrinityIfRandom : BaseComplexNodeTag
    {
        private Func<bool> funcConditionalProcess;
        private int iID;
        private int iResult;

        protected override Composite CreateBehavior()
        {
            PrioritySelector decorated = new PrioritySelector(new Composite[0]);
            foreach (ProfileBehavior behavior in base.GetNodes())
            {
                decorated.AddChild(behavior.Behavior);
            }
            return new Zeta.TreeSharp.Decorator(new CanRunDecoratorDelegate(CheckNotAlreadyDone), decorated);
        }

        public override bool GetConditionExec()
        {
            int iOldValue;

            // If the dictionary value doesn't even exist, FAIL!
            if (!Trinity.dictRandomID.TryGetValue(ID, out iOldValue))
                return false;

            // Ok, do the results match up what we want? then SUCCESS!
            if (iOldValue == Result)
                return true;

            // No? Fail!
            return false;
        }

        private bool CheckNotAlreadyDone(object obj)
        {
            return !IsDone;
        }

        [XmlAttribute("id")]
        public int ID
        {
            get
            {
                return iID;
            }
            set
            {
                iID = value;
            }
        }

        [XmlAttribute("result")]
        public int Result
        {
            get
            {
                return iResult;
            }
            set
            {
                iResult = value;
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
