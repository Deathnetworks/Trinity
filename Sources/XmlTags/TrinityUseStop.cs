using Zeta.CommonBot.Profile;
using Zeta.TreeSharp;
using Zeta.XmlEngine;

namespace Trinity.XmlTags
{
    //  TrinityUseStop - prevents a useonce tag ID ever being used again                     
    [XmlElement("TrinityUseStop")]
    public class TrinityUseStop : ProfileBehavior
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

                // If so, set it disabled - if not, add it and prevent it
                if (TrinityUseOnce.UseOnceIDs.Contains(ID))
                {
                    TrinityUseOnce.UseOnceCounter[ID] = -1;
                }
                else
                {
                    TrinityUseOnce.UseOnceIDs.Add(ID);
                    TrinityUseOnce.UseOnceCounter.Add(ID, -1);
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
