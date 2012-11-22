using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Zeta.Common;
using Zeta.CommonBot.Profile;
using Zeta.TreeSharp;
using Zeta.XmlEngine;

namespace GilesTrinity.XmlTags
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
                Logging.Write("[Trinity] Town-run request received, will town-run at next possible moment.");
                GilesTrinity.bGilesForcedVendoring = true;
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
