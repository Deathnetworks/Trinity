using Zeta.Common;
using Zeta.CommonBot.Profile;
using Zeta.TreeSharp;
using Zeta.XmlEngine;

namespace GilesTrinity.XmlTags
{
    // TrinityMaxDeaths tells Trinity to handle deaths and exit game after X deaths
    [XmlElement("TrinityMaxDeaths")]
    public class TrinityMaxDeaths : ProfileBehavior
    {
        private bool m_IsDone = false;
        private int iMaxDeaths;
        private string sReset;

        public override bool IsDone
        {
            get { return m_IsDone; }
        }

        protected override Composite CreateBehavior()
        {
            return new Zeta.TreeSharp.Action(ret =>
            {
                if (MaxDeaths != GilesTrinity.iMaxDeathsAllowed)
                    Logging.Write("[Trinity] Max deaths set by profile. Trinity now handling deaths, and will restart the game after " + MaxDeaths.ToString());
                GilesTrinity.iMaxDeathsAllowed = MaxDeaths;
                if (Reset != null && Reset.ToLower() == "true")
                    GilesTrinity.iDeathsThisRun = 0;
                m_IsDone = true;
            });
        }

        [XmlAttribute("reset")]
        public string Reset
        {
            get
            {
                return sReset;
            }
            set
            {
                sReset = value;
            }
        }

        [XmlAttribute("max")]
        public int MaxDeaths
        {
            get
            {
                return iMaxDeaths;
            }
            set
            {
                iMaxDeaths = value;
            }
        }

        public override void ResetCachedDone()
        {
            m_IsDone = false;
            base.ResetCachedDone();
        }
    }
}
