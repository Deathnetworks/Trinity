using Zeta.CommonBot.Profile;
using Zeta.TreeSharp;
using Zeta.XmlEngine;

using GilesTrinity.Technicals;
using System;
using System.Collections.Generic;
using System.Linq;
using Zeta;
using Zeta.Common;
using Zeta.CommonBot.Profile;
using Zeta.Navigation;
using Zeta.TreeSharp;
using Zeta.XmlEngine;

namespace GilesTrinity.XmlTags
{
    // * TrinityUseReset - Resets a UseOnce tag as if it has never been used
    [XmlElement("TrinityRandomWait")]
    public class TrinityRandomWait : ProfileBehavior
    {
        private bool m_IsDone = false;
        private int iMinDelay;
        private int iMaxDelay;
        private int delay;

        public override bool IsDone
        {
            get { return m_IsDone; }
        }

        protected override Composite CreateBehavior()
        {
            return new Zeta.TreeSharp.Action(ret =>
            {
                delay = new System.Random().Next(iMinDelay, iMaxDelay);
                DbHelper.Log(TrinityLogLevel.Normal, LogCategory.UserInformation, "[XML Tag] Trinity Random Wait - Taking a break for " + Math.Round(delay / (float)1000, 2).ToString() + " seconds.");
                System.Threading.Thread.Sleep(delay);
                m_IsDone = true;
            });
        }

        [XmlAttribute("min")]
        public int min
        {
            get
            {
                return iMinDelay;
            }
            set
            {
                iMinDelay = value;
            }
        }
        [XmlAttribute("max")]
        public int max
        {
            get
            {
                return iMaxDelay;
            }
            set
            {
                iMaxDelay = value;
            }
        }

        public override void ResetCachedDone()
        {
            m_IsDone = false;
            base.ResetCachedDone();
        }
    }
}
