using System;
using Trinity.Technicals;
using Zeta;
using Zeta.CommonBot.Profile;
using Zeta.TreeSharp;
using Zeta.XmlEngine;

namespace Trinity.XmlTags
{
    // * TrinityUseReset - Resets a UseOnce tag as if it has never been used
    [XmlElement("TrinityCastSweepingWinds")]
    public class TrinityCastSweepingWinds : ProfileBehavior
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
                if (ZetaDia.Me.ActorClass == Zeta.Internals.Actors.ActorClass.Monk && Trinity.Hotbar.Contains(Zeta.Internals.Actors.SNOPower.Monk_SweepingWind)
                    && Trinity.Settings.Combat.Monk.HasInnaSet && Trinity.Player.PrimaryResource > 10)
                {
                    if (DateTime.Now.Subtract(Trinity.SweepWindSpam).TotalMilliseconds >= 1500)
                    {
                        if (Trinity.GetHasBuff(Zeta.Internals.Actors.SNOPower.Monk_SweepingWind))
                        {
                            ZetaDia.Me.UsePower(Zeta.Internals.Actors.SNOPower.Monk_SweepingWind, Trinity.Player.Position, Trinity.CurrentWorldDynamicId, -1);
                            Trinity.SweepWindSpam = DateTime.Now;
                            Logger.Log(TrinityLogLevel.Normal, LogCategory.ProfileTag, "Cast Sweeping Winds.");
                        }
                        else
                        {
                            Logger.Log(TrinityLogLevel.Normal, LogCategory.ProfileTag, "Sweeping winds buff is down - not casting.");
                        }
                    }
                    else
                    {
                        Logger.Log(TrinityLogLevel.Normal, LogCategory.ProfileTag, " Too soon to cast SW again, avoiding spam.");
                    }
                }
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