using Trinity.Technicals;
using Zeta.CommonBot.Profile;
using Zeta.TreeSharp;
using Zeta.XmlEngine;

namespace Trinity.XmlTags
{
    // TrinityTownRun forces a town-run request
    [XmlElement("TrinityTownRun")]
    public class TrinityTownRun : ProfileBehavior
    {
        private bool m_IsDone = false;

        public override bool IsDone
        {
            get { return m_IsDone; }
        }

        protected override Composite CreateBehavior()
        {
            return new Zeta.TreeSharp.Action(ret =>
            {
                Logger.Log(TrinityLogLevel.Normal, LogCategory.ProfileTag, "Town-run request received, will town-run at next possible moment.");
                Trinity.ForceVendorRunASAP = true;
                m_IsDone = true;
            });
        }

        public override void ResetCachedDone()
        {
            m_IsDone = false;
            base.ResetCachedDone();
        }
    }
}
