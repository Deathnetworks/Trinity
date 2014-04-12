using Trinity.Combat.Abilities;
using Trinity.Technicals;
using Zeta.Bot.Profile;
using Zeta.TreeSharp;
using Zeta.XmlEngine;

namespace Trinity.XmlTags
{
    // Trinity Log lets profiles send log messages to DB
    [XmlElement("TrinitySetQuesting")]
    public class TrinitySetQuesting : ProfileBehavior
    {
        private bool isDone = false;

        public override bool IsDone
        {
            get { return isDone; }
        }

        protected override Composite CreateBehavior()
        {
            return new Zeta.TreeSharp.Action(ret =>
            {
                CombatBase.IsQuestingMode = true;
                Logger.Log("Setting Trinity Combat mode as QUESTING for the current profile.");
                isDone = true;   
            });
        }

        public override void ResetCachedDone()
        {
            isDone = false;
            base.ResetCachedDone();
        }
    }
}
