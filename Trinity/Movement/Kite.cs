using System;
using System.Collections.Generic;
using System.Linq;
using Trinity.Combat;
using Trinity.Combat.Abilities;
using Trinity.Config.Combat;
using Trinity.Technicals;
using Trinity.DbProvider;
using Zeta.Common;
using Zeta.Common.Plugins;
using Zeta.Game;
using Zeta.Game.Internals.Actors;
using Zeta.Game.Internals.SNO;
using Logger = Trinity.Technicals.Logger;

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

        internal static void RefreshKiteValue()
        {
            if (Trinity.ObjectCache == null)
                return;

            foreach (var u in Trinity.ObjectCache.Where(u => u.IsUnit))
            {
                // Kite (can make it in navHelper, avoid to re loop twice)
                if (u.RadiusDistance < CombatBase.KiteDistance * 0.75 && u.IsInLineOfSight)
                {
                    if (u.IsBoss && CombatBase.KiteMode != KiteMode.Never)
                    {
                        Trinity.Player.NeedToKite = true;
                    }
                    else if (u.IsBossOrEliteRareUnique && (CombatBase.KiteMode == KiteMode.Elites || CombatBase.KiteMode == KiteMode.Always))
                    {
                        Trinity.Player.NeedToKite = true;
                    }
                    else if (CombatBase.KiteMode == KiteMode.Always)
                    {
                        Trinity.Player.NeedToKite = true;
                    }
                }
            }
        }

        private static void RefreshSetKiting(ref Vector3 vKitePointAvoid, bool needToKite)
        {
            using (new PerformanceLogger("RefreshDiaObjectCache.Kiting"))
            {
                if (Trinity.Settings.Combat.Misc.KeepMovingInCombat)
                    return;

                bool TryToKite = false;

                List<TrinityCacheObject> kiteMonsterList = new List<TrinityCacheObject>();

                if (CurrentTarget != null && CurrentTarget.IsUnit)
                {
                    switch (CombatBase.KiteMode)
                    {
                        case KiteMode.Never:
                            break;
                        case KiteMode.Elites:
                            kiteMonsterList = (from m in ObjectCache
                                               where m.IsUnit &&
                                               m.RadiusDistance > 0 &&
                                               m.RadiusDistance <= CombatBase.KiteDistance &&
                                               m.IsBossOrEliteRareUnique
                                               select m).ToList();
                            break;
                        case KiteMode.Bosses:
                            kiteMonsterList = (from m in ObjectCache
                                               where m.IsUnit &&
                                               m.RadiusDistance > 0 &&
                                               m.RadiusDistance <= CombatBase.KiteDistance &&
                                               m.IsBoss
                                               select m).ToList();
                            break;
                        case KiteMode.Always:
                            kiteMonsterList = (from m in ObjectCache
                                               where m.IsUnit &&
                                               m.Weight > 0 &&
                                               m.RadiusDistance > 0 &&
                                               m.RadiusDistance <= CombatBase.KiteDistance &&
                                               (m.IsBossOrEliteRareUnique ||
                                                ((m.HitPointsPct >= .15 || m.MonsterSize != MonsterSize.Swarm) && !m.IsBossOrEliteRareUnique))
                                               select m).ToList();
                            break;
                    }
                }
                if (kiteMonsterList.Any())
                {
                    TryToKite = true;
                    vKitePointAvoid = Player.Position;
                }

                if (CombatBase.KiteDistance > 0 && kiteMonsterList.Count() > 0 && IsWizardShouldKite())
                {
                    TryToKite = true;
                    vKitePointAvoid = Player.Position;
                }

                // Avoid Death
                if (Trinity.Player.AvoidDeath)
                {
                    Trinity.Player.NeedToKite = true;
                    kiteMonsterList = (from m in ObjectCache
                                       where m.IsUnit
                                       select m).ToList();
                }

                if (!Trinity.Player.AvoidDeath && CombatBase.KiteDistance <= 0)
                    return;

                // Note that if treasure goblin level is set to kamikaze, even avoidance moves are disabled to reach the goblin!
                bool shouldKamikazeTreasureGoblins = (!AnyTreasureGoblinsPresent || Settings.Combat.Misc.GoblinPriority <= GoblinPriority.Prioritize);

                double msCancelledEmergency = DateTime.UtcNow.Subtract(timeCancelledEmergencyMove).TotalMilliseconds;
                bool shouldEmergencyMove = msCancelledEmergency >= cancelledEmergencyMoveForMilliseconds && Trinity.Player.NeedToKite;

                double msCancelledKite = DateTime.UtcNow.Subtract(timeCancelledKiteMove).TotalMilliseconds;
                bool shouldKite = msCancelledKite >= cancelledKiteMoveForMilliseconds && TryToKite;

                if (shouldKamikazeTreasureGoblins && (shouldEmergencyMove || shouldKite) && !Combat.QueuedMovementManager.Stuck.IsStuck())
                {
                    Vector3 vAnySafePoint = GridMap.GetBestMoveNode().Position;

                    if (LastKitePosition == null)
                    {
                        LastKitePosition = new KitePosition()
                        {
                            PositionFoundTime = DateTime.UtcNow,
                            Position = vAnySafePoint,
                            Distance = vAnySafePoint.Distance(Player.Position)
                        };
                    }

                    if (vAnySafePoint != Vector3.Zero && vAnySafePoint.Distance(Player.Position) >= 1)
                    {
                        PlayerMover.UsedSpecialMovement(vAnySafePoint);

                        if ((DateTime.UtcNow.Subtract(LastKitePosition.PositionFoundTime).TotalMilliseconds > 3000 && LastKitePosition.Position == vAnySafePoint) ||
                            (CurrentTarget != null && DateTime.UtcNow.Subtract(lastGlobalCooldownUse).TotalMilliseconds > 1500 && TryToKite))
                        {
                            timeCancelledKiteMove = DateTime.UtcNow;
                            cancelledKiteMoveForMilliseconds = 1500;
                            Logger.Log(TrinityLogLevel.Debug, LogCategory.UserInformation, "Kite movement failed, cancelling for {0:0}ms", cancelledKiteMoveForMilliseconds);
                            return;
                        }
                        else
                        {
                            LastKitePosition = new KitePosition()
                            {
                                PositionFoundTime = DateTime.UtcNow,
                                Position = vAnySafePoint,
                                Distance = vAnySafePoint.Distance(Player.Position)
                            };
                        }

                        if (Settings.Advanced.LogCategories.HasFlag(LogCategory.Movement))
                        {
                            Logger.Log(TrinityLogLevel.Verbose, LogCategory.Movement, "Kiting to: {0} Distance: {1:0} Direction: {2:0}, Health%={3:0.00}, KiteDistance: {4:0}, Nearby Monsters: {5:0} Trinity.Player.NeedToKite: {6} TryToKite: {7}",
                                vAnySafePoint, vAnySafePoint.Distance(Player.Position), MathUtil.GetHeading(MathUtil.FindDirectionDegree(Player.Position, vAnySafePoint)),
                                Player.CurrentHealthPct, CombatBase.KiteDistance, kiteMonsterList.Count(),
                                Trinity.Player.NeedToKite, TryToKite);
                        }
                        CurrentTarget = new TrinityCacheObject()
                        {
                            Position = vAnySafePoint,
                            Type = GObjectType.Avoidance,
                            Weight = 90000,
                            Radius = 2f,
                            InternalName = "KitePoint",
                            IsKite = true
                        };
                    }
                }
                else if (!shouldEmergencyMove && Trinity.Player.NeedToKite)
                {
                    Logger.Log(TrinityLogLevel.Debug, LogCategory.UserInformation, "Emergency movement cancelled for {0:0}ms", DateTime.UtcNow.Subtract(timeCancelledEmergencyMove).TotalMilliseconds - cancelledKiteMoveForMilliseconds);
                }
                else if (!shouldKite && TryToKite)
                {
                    Logger.Log(TrinityLogLevel.Debug, LogCategory.UserInformation, "Kite movement cancelled for {0:0}ms", DateTime.UtcNow.Subtract(timeCancelledKiteMove).TotalMilliseconds - cancelledKiteMoveForMilliseconds);
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
