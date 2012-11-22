using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Zeta.CommonBot.Profile;
using Zeta.TreeSharp;
using Zeta.XmlEngine;

namespace GilesTrinity.XmlTags
{
    // * TrinityUseReset - Resets a UseOnce tag as if it has never been used
    [XmlElement("TrinityUseReset")]
    public class TrinityUseReset : ProfileBehavior
    {
        private bool m_IsDone = false;
        private int iID;

        public override bool IsDone
        {
            get { return m_IsDone; }
        }

        protected override Composite CreateBehavior()
        {
            return new Zeta.TreeSharp.Action(ret =>
            {

                // See if we've EVER hit this ID before

                // If so, delete it, if not, do nothing
                if (GilesTrinity.hashUseOnceID.Contains(ID))
                {
                    GilesTrinity.hashUseOnceID.Remove(ID);
                    GilesTrinity.dictUseOnceID.Remove(ID);
                }
                m_IsDone = true;
            });
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

        public override void ResetCachedDone()
        {
            m_IsDone = false;
            base.ResetCachedDone();
        }
    }
}
