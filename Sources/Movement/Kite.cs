using Trinity.Config.Combat;
using Trinity.Technicals;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Zeta;
using Zeta.Common;
using Zeta.Common.Plugins;
using Zeta.CommonBot;
using Zeta.Internals.Actors;
using Zeta.Internals.SNO;
using Trinity.Combat.Abilities;
namespace Trinity
{
    public partial class Trinity : IPlugin
    {
        internal sealed class KitePosition
        {
            public DateTime PositionFoundTime { get; set; }
            public Vector3 Position { get; set; }
            public float Distance { get; set; }
            public KitePosition() { }
        }
        internal static KitePosition LastKitePosition = null;

        private static void RefreshSetKiting(ref Vector3 vKitePointAvoid, bool NeedToKite, ref bool TryToKite)
        {
            using (new PerformanceLogger("RefreshDiaObjectCache.Kiting"))
            {

                TryToKite = false;

                var monsterList = from m in ObjectCache
                                  where m.Type == GObjectType.Unit &&
                                  m.Weight > 0 &&
                                  m.RadiusDistance <= PlayerKiteDistance &&
                                  (m.IsBossOrEliteRareUnique ||
                                   ((m.HitPointsPct >= .15 || m.MonsterStyle != MonsterSize.Swarm) && !m.IsBossOrEliteRareUnique)
                                   )
                                  select m;

                if (CurrentTarget != null && CurrentTarget.Type == GObjectType.Unit && PlayerKiteDistance > 0 && CurrentTarget.RadiusDistance <= PlayerKiteDistance)
                {
                    TryToKite = true;
                    vKitePointAvoid = Player.Position;
                }

                if (monsterList.Count() > 0 && (Player.ActorClass != ActorClass.Wizard || IsWizardShouldKite()))
                {
                    TryToKite = true;
                    vKitePointAvoid = Player.Position;
                }

                if (Settings.Combat.Misc.AvoidDeath && 
                    Player.CurrentHealthPct <= PlayerEmergencyHealthPotionLimit && // h ealth is lower than potion limit
                    !SNOPowerUseTimer(SNOPower.DrinkHealthPotion) && // we can't use a potion anymore
                    TargetUtil.AnyMobsInRange(90f, false))
                {
                    Logger.LogNormal("Attempting to avoid death!");
                    NeedToKite = true;
                }

                // Note that if treasure goblin level is set to kamikaze, even avoidance moves are disabled to reach the goblin!
                bool shouldKamikazeTreasureGoblins = (!AnyTreasureGoblinsPresent || Settings.Combat.Misc.GoblinPriority <= GoblinPriority.Prioritize);

                double msCancelledEmergency = DateTime.Now.Subtract(timeCancelledEmergencyMove).TotalMilliseconds;
                bool shouldEmergencyMove = msCancelledEmergency >= cancelledEmergencyMoveForMilliseconds && NeedToKite;

                double msCancelledKite = DateTime.Now.Subtract(timeCancelledKiteMove).TotalMilliseconds;
                bool shouldKite = msCancelledKite >= cancelledKiteMoveForMilliseconds && TryToKite;

                if (shouldKamikazeTreasureGoblins && (shouldEmergencyMove || shouldKite))
                {
                    Vector3 vAnySafePoint = NavHelper.FindSafeZone(false, 1, vKitePointAvoid, true, monsterList);

                    if (LastKitePosition == null)
                    {
                        LastKitePosition = new KitePosition()
                        {
                            PositionFoundTime = DateTime.Now,
                            Position = vAnySafePoint,
                            Distance = vAnySafePoint.Distance(Player.Position)
                        };
                    }

                    if (vAnySafePoint != Vector3.Zero && vAnySafePoint.Distance(Player.Position) >= 1)
                    {

                        if ((DateTime.Now.Subtract(LastKitePosition.PositionFoundTime).TotalMilliseconds > 3000 && LastKitePosition.Position == vAnySafePoint) ||
                            (CurrentTarget != null && DateTime.Now.Subtract(lastGlobalCooldownUse).TotalMilliseconds > 1500 && TryToKite))
                        {
                            timeCancelledKiteMove = DateTime.Now;
                            cancelledKiteMoveForMilliseconds = 1500;
                            Logger.Log(TrinityLogLevel.Verbose, LogCategory.Movement, "Kite movement failed, cancelling for {0:0}ms", cancelledKiteMoveForMilliseconds);
                            return;
                        }
                        else
                        {
                            LastKitePosition = new KitePosition()
                            {
                                PositionFoundTime = DateTime.Now,
                                Position = vAnySafePoint,
                                Distance = vAnySafePoint.Distance(Player.Position)
                            };
                        }

                        if (Settings.Advanced.LogCategories.HasFlag(LogCategory.Movement))
                        {
                            Logger.Log(TrinityLogLevel.Verbose, LogCategory.Movement, "Kiting to: {0} Distance: {1:0} Direction: {2:0}, Health%={3:0.00}, KiteDistance: {4:0}, Nearby Monsters: {5:0} NeedToKite: {6} TryToKite: {7}",
                                vAnySafePoint, vAnySafePoint.Distance(Player.Position), MathUtil.GetHeading(MathUtil.FindDirectionDegree(Me.Position, vAnySafePoint)),
                                Player.CurrentHealthPct, PlayerKiteDistance, monsterList.Count(),
                                NeedToKite, TryToKite);
                        }
                        CurrentTarget = new TrinityCacheObject()
                        {
                            Position = vAnySafePoint,
                            Type = GObjectType.Avoidance,
                            Weight = 90000,
                            CentreDistance = Vector3.Distance(Player.Position, vAnySafePoint),
                            RadiusDistance = Vector3.Distance(Player.Position, vAnySafePoint),
                            InternalName = "KitePoint"
                        };

                        //timeCancelledKiteMove = DateTime.Now;
                        //cancelledKiteMoveForMilliseconds = 100;

                        // Try forcing a target update with each kiting
                        //bForceTargetUpdate = true;
                    }
                    //else
                    //{
                    //    // Didn't find any kiting we could reach, so don't look for any more kite spots for at least 1.5 seconds
                    //    timeCancelledKiteMove = DateTime.Now;
                    //    cancelledKiteMoveForMilliseconds = 500;
                    //}
                }
                else if (!shouldEmergencyMove && NeedToKite)
                {
                    Logger.Log(TrinityLogLevel.Verbose, LogCategory.Movement, "Emergency movement cancelled for {0:0}ms", DateTime.Now.Subtract(timeCancelledEmergencyMove).TotalMilliseconds - cancelledKiteMoveForMilliseconds);
                }
                else if (!shouldKite && TryToKite)
                {
                    Logger.Log(TrinityLogLevel.Verbose, LogCategory.Movement, "Kite movement cancelled for {0:0}ms", DateTime.Now.Subtract(timeCancelledKiteMove).TotalMilliseconds - cancelledKiteMoveForMilliseconds);
                }
            }
        }

        private static bool IsWizardShouldKite()
        {
            if (Player.ActorClass == ActorClass.Wizard)
            {
                if (Settings.Combat.Wizard.KiteOption == WizardKiteOption.Anytime)
                    return true;

                if (GetHasBuff(SNOPower.Wizard_Archon) && Settings.Combat.Wizard.KiteOption == WizardKiteOption.ArchonOnly)
                    return true;
                if (!GetHasBuff(SNOPower.Wizard_Archon) && Settings.Combat.Wizard.KiteOption == WizardKiteOption.NormalOnly)
                    return true;

                return false;

            }
            else
                return false;
        }
    }
}
