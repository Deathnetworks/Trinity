using System;
using System.Linq;
using Zeta;
using Zeta.Common;
using Zeta.CommonBot.Profile;
using Zeta.Internals.Actors;
using Zeta.TreeSharp;
using Zeta.XmlEngine;

namespace GilesTrinity.XmlTags
{
    // TrinityIfWithinRange checks an SNO is in range and processes child nodes
    [XmlElement("TrinityIfSNOInRange")]
    public class TrinityIfSNOInRange : BaseComplexNodeTag
    {
        private Func<bool> funcConditionalProcess;
        private int iSNOID;
        private float fRadius;
        private string sType;

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
            bool flag;
            Vector3 vMyLocation = ZetaDia.Me.Position;
            if (sType != null && sType == "reverse")
                flag = ZetaDia.Actors.GetActorsOfType<DiaObject>(true, false).FirstOrDefault<DiaObject>(a => a.ActorSNO == SNOID && a.Position.Distance(vMyLocation) <= Range) == null;
            else
                flag = (ZetaDia.Actors.GetActorsOfType<DiaObject>(true, false).FirstOrDefault<DiaObject>(a => a.ActorSNO == SNOID && a.Position.Distance(vMyLocation) <= Range) != null);
            return flag;
        }

        private bool CheckNotAlreadyDone(object object_0)
        {
            return !IsDone;
        }

        [XmlAttribute("snoid")]
        public int SNOID
        {
            get
            {
                return iSNOID;
            }
            set
            {
                iSNOID = value;
            }
        }

        [XmlAttribute("range")]
        public float Range
        {
            get
            {
                return fRadius;
            }
            set
            {
                fRadius = value;
            }
        }

        [XmlAttribute("type")]
        public string Type
        {
            get
            {
                return sType;
            }
            set
            {
                sType = value;
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
