using GilesTrinity.Technicals;
using Zeta.Common;
using Zeta.CommonBot.Profile;
using Zeta.TreeSharp;
using Zeta.XmlEngine;

namespace GilesTrinity.XmlTags
{
    // Trinity Log lets profiles send log messages to DB
    [XmlElement("TrinityLog")]
    public class TrinityLog : ProfileBehavior
    {
        private bool m_IsDone = false;
        private string sLogOutput;
        private string sLogLevel;

        public override bool IsDone
        {
            get { return m_IsDone; }
        }

        protected override Composite CreateBehavior()
        {
            return new Zeta.TreeSharp.Action(ret =>
            {
                DbHelper.Log((Level != null && Level.ToLower() == "diagnostic") ? TrinityLogLevel.Debug : TrinityLogLevel.Normal, LogCategory.UserInformation, Output);
                m_IsDone = true;
            });
        }

        [XmlAttribute("level", true)]
        public string Level
        {
            get
            {
                return sLogLevel;
            }
            set
            {
                sLogLevel = value;
            }
        }

        [XmlAttribute("output", true)]
        public string Output
        {
            get
            {
                return sLogOutput;
            }
            set
            {
                sLogOutput = value;
            }
        }

        public override void ResetCachedDone()
        {
            m_IsDone = false;
            base.ResetCachedDone();
        }
    }
}
