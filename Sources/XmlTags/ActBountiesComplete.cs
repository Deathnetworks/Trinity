using System.Linq;
using Trinity.Technicals;
using Zeta.Bot.Profile;
using Zeta.Game;
using Zeta.Game.Internals;
using Zeta.TreeSharp;
using Zeta.XmlEngine;

namespace Trinity.XmlTags
{
    [XmlElement("ActBountiesComplete")]
    public class ActBountiesComplete : BaseComplexNodeTag
    {

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
            var b = ZetaDia.ActInfo.Bounties.Where(bounty => bounty.Act.ToString().Equals(Act) && bounty.Info.State == QuestState.Completed);
            if (b.FirstOrDefault() != null) Logger.Log("Bounties Complete count:" + b.Count());
            else Logger.Log("Bounties complete returned null.");

            foreach (var c in ZetaDia.ActInfo.Bounties.Where(bounty => bounty.Act.ToString().Equals(Act) && bounty.Info.State != QuestState.Completed))
            {
                Logger.Log("Bounty " + c.Info.Quest.ToString() + " (" + c.Info.QuestSNO + ") unsupported or invalid.");
            }

            return b.FirstOrDefault() != null && b.Count() == 5;
        }

        private bool CheckNotAlreadyDone(object obj)
        {
            return !IsDone;
        }

        [XmlAttribute("act", true)]
        public string Act
        {
            get;
            set;
        }
    }
}
