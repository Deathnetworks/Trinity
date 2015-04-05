using System;
using System.Collections.Generic;
using System.Linq;
using Trinity.Combat.Abilities;
using Trinity.Reference;
using Trinity.Technicals;
using Zeta.Common;
using Zeta.Game;
using Zeta.Game.Internals.Actors;
using Logger = Trinity.Technicals.Logger;

namespace Trinity
{
    public partial class Trinity
    {
        private static bool RefreshAvoidanceAnimations()
        {
            if (CurrentCacheObject.IsSummonedByPlayer || CurrentCacheObject.IsAlly ||
                CurrentCacheObject.ActorSNO == Player.ActorSNO || CurrentCacheObject.RActorGuid == Player.RActorGuid)
            {
                return true;
            }

            try
            {
                CurrentCacheObject.Animation = c_diaObject.CommonData.CurrentAnimation;
            }
            catch (Exception ex)
            {
                Logger.Log(TrinityLogLevel.Debug, LogCategory.Avoidance, "Error reading CurrentAnimation of Unit sno:{0} raGuid:{1} name:{2} ex:{3}",
                  (int)CurrentCacheObject.Animation, CurrentCacheObject.RActorGuid, CurrentCacheObject.InternalName, ex.Message);

                return true;
            }

            if (DataDictionary.AnimationsObsoleteIds.Any(n => (int)CurrentCacheObject.Animation == n) ||
                DataDictionary.AnimationsObsoleteIds.Any(n => CurrentCacheObject.ActorSNO == n))
            {
                return true;
            }

            try
            {
                CurrentCacheObject.Rotation = c_diaObject.Movement.Rotation;
            }
            catch { }

            string animationType = "null";

            float customRadius;
            if (DataDictionary.DefaultAvoidanceCustomRadius.TryGetValue((int)CurrentCacheObject.Animation, out customRadius) ||
                DataDictionary.DefaultAvoidanceCustomRadius.TryGetValue(CurrentCacheObject.ActorSNO, out customRadius))
            {
                CurrentCacheObject.Radius = customRadius;
            }

            if (CurrentCacheObject.InternalName.Contains("wall") ||
                CurrentCacheObject.Animation.ToString().Contains("wall"))
            {
                AddObjectToNavigationObstacleCache();
            }

            if (!Trinity.Settings.Combat.Misc.AvoidAOE)
                return true;

            TimeSpan aoeExpiration = TimeSpan.FromMilliseconds(500);
            double minAvoidanceHealth = GetAvoidanceHealth(CurrentCacheObject.ActorSNO);

            if ((int)CurrentCacheObject.Animation == 137577) // Demonic forge animation
            {
                /*Vector3 endPoint = MathEx.GetPointAt(CurrentCacheObject.Position, 50f, CurrentCacheObject.Rotation); 
                for (float i = 0; i <= CurrentCacheObject.Position.Distance2D(endPoint); i += 5f)
                {
                    Vector3 pathSpot = MathEx.CalculatePointFrom(CurrentCacheObject.Position, endPoint, i);

                    if (Player.Position.Distance2D(pathSpot) <= 20f)
                    {
                        Trinity.Player.StandingInAvoidance = true;
                        animationType = "Demonic forge animation";
                    }

                    CacheData.TimeBoundAvoidance.Add(new CacheObstacleObject(pathSpot, 40f, CurrentCacheObject.ActorSNO, CurrentCacheObject.Animation.ToString())
                    {
                        Expires = DateTime.UtcNow.Add(aoeExpiration),
                        ObjectType = GObjectType.Avoidance,
                        Rotation = CurrentCacheObject.Rotation,
                        Animation = CurrentCacheObject.Animation,
                        IsAvoidanceAnimations = true,
                    });

                    Logger.Log(TrinityLogLevel.Verbose, LogCategory.Avoidance, "Add {0} to CacheData SNO={2} SNOAnim={3} ({1})", CurrentCacheObject.Animation.ToString(), animationType, CurrentCacheObject.ActorSNO, (int)CurrentCacheObject.Animation);
                }*/
            }
            else if (Settings.Combat.Misc.UseExperimentalSavageBeastAvoidance && CurrentCacheObject.IsCharging)
            {
                var minWidth = Math.Max(10f, CurrentCacheObject.Radius) + 5f;
                Vector3 endPoint = MathEx.GetPointAt(CurrentCacheObject.Position, 70f, CurrentCacheObject.Rotation);
                for (float i = 0; i <= CurrentCacheObject.Position.Distance2D(endPoint); i += (float)(minWidth * 0.5))
                {
                    Vector3 pathSpot = MathEx.CalculatePointFrom(CurrentCacheObject.Position, endPoint, i);

                    CacheData.TimeBoundAvoidance.Add(new CacheObstacleObject(pathSpot, minWidth, CurrentCacheObject.ActorSNO, CurrentCacheObject.Animation.ToString())
                    {
                        Expires = DateTime.UtcNow.Add(aoeExpiration),
                        ObjectType = GObjectType.Avoidance,
                        Rotation = CurrentCacheObject.Rotation,
                        Animation = CurrentCacheObject.Animation,
                        IsAvoidanceAnimations = true,
                    });

                    if (Player.Position.Distance2D(pathSpot) <= minWidth * 0.5)
                    {
                        Trinity.Player.StandingInAvoidance = Player.CurrentHealthPct <= minAvoidanceHealth;
                        animationType = "Charger animation";
                    }

                    Logger.Log(TrinityLogLevel.Debug, LogCategory.Avoidance, "Add {0} to CacheData SNO={2} SNOAnim={3} ({1})", CurrentCacheObject.Animation.ToString(), animationType, CurrentCacheObject.ActorSNO, (int)CurrentCacheObject.Animation);
                }
            }
            else if (CurrentCacheObject.HasAnimationToAvoidAtPlayer)
            {
                if (!CacheData.TimeBoundAvoidance.Any(a => a.ActorSNO == (int)CurrentCacheObject.Animation))
                {
                    Logger.Log(TrinityLogLevel.Debug, LogCategory.Avoidance, "Add {0} to CacheData SNO={2} SNOAnim={3} ({1})", CurrentCacheObject.Animation.ToString(), animationType, CurrentCacheObject.ActorSNO, (int)CurrentCacheObject.Animation);

                    CacheData.TimeBoundAvoidance.Add(new CacheObstacleObject(Trinity.Player.Position, CurrentCacheObject.Radius, CurrentCacheObject.ActorSNO, CurrentCacheObject.Animation.ToString())
                    {
                        Expires = DateTime.UtcNow.Add(TimeSpan.FromMilliseconds(1500)),
                        ObjectType = GObjectType.Avoidance,
                        Rotation = CurrentCacheObject.Rotation,
                        Animation = CurrentCacheObject.Animation,
                        IsAvoidanceAnimations = true,
                    });

                    Trinity.Player.StandingInAvoidance = Player.CurrentHealthPct <= minAvoidanceHealth;
                    animationType = "At player animation";
                }
                else if (CacheData.TimeBoundAvoidance.Any(a =>
                    a.ActorSNO == (int)CurrentCacheObject.Animation &&
                    a.Position.Distance2D(Trinity.Player.Position) <= CurrentCacheObject.Radius))
                {
                    Trinity.Player.StandingInAvoidance = Player.CurrentHealthPct <= minAvoidanceHealth;
                    animationType = "At player animation";
                }
            }
            else if (CombatBase.KiteDistance > 5 && CurrentCacheObject.HasBasicAttackAnimation)
            {
                CacheData.TimeBoundAvoidance.Add(new CacheObstacleObject(CurrentCacheObject.Position, CurrentCacheObject.Radius, CurrentCacheObject.ActorSNO, CurrentCacheObject.Animation.ToString())
                {
                    Expires = DateTime.UtcNow.Add(aoeExpiration),
                    ObjectType = GObjectType.Avoidance,
                    Rotation = CurrentCacheObject.Rotation,
                    Animation = CurrentCacheObject.Animation,
                    IsAvoidanceAnimations = true,
                });

                if (CurrentCacheObject.IsFacingPlayer && CurrentCacheObject.Distance <= CurrentCacheObject.Radius)
                {
                    Trinity.Player.StandingInAvoidance = Player.CurrentHealthPct <= minAvoidanceHealth;
                    animationType = "Basic attack animation";
                }

                Logger.Log(TrinityLogLevel.Debug, LogCategory.Avoidance, "Add {0} to CacheData SNO={2} SNOAnim={3} ({1})", CurrentCacheObject.Animation.ToString(), animationType, CurrentCacheObject.ActorSNO, (int)CurrentCacheObject.Animation);
            }
            else if (CurrentCacheObject.HasAnimationToAvoid && !CurrentCacheObject.HasBasicAttackAnimation)
            {
                CacheData.TimeBoundAvoidance.Add(new CacheObstacleObject(CurrentCacheObject.Position, CurrentCacheObject.Radius, CurrentCacheObject.ActorSNO, CurrentCacheObject.Animation.ToString())
                {
                    Expires = DateTime.UtcNow.Add(aoeExpiration),
                    ObjectType = GObjectType.Avoidance,
                    Rotation = CurrentCacheObject.Rotation,
                    Animation = CurrentCacheObject.Animation,
                    IsAvoidanceAnimations = true,
                });

                if (CurrentCacheObject.Distance <= CurrentCacheObject.Radius)
                {
                    Trinity.Player.StandingInAvoidance = Player.CurrentHealthPct <= minAvoidanceHealth;
                    animationType = "Animation";
                }

                Logger.Log(TrinityLogLevel.Debug, LogCategory.Avoidance, "Add {0} to CacheData SNO={2} SNOAnim={3} ({1})", CurrentCacheObject.Animation.ToString(), animationType, CurrentCacheObject.ActorSNO, (int)CurrentCacheObject.Animation);
            }

            if (Trinity.Player.StandingInAvoidance && animationType != "null")
            {
                Logger.Log(TrinityLogLevel.Info, LogCategory.Avoidance, "Is standing in avoidance {5} Name={0} SNO={1} radius={2:0} health={3:0.00} dist={4:0}",
                    CurrentCacheObject.Animation.ToString(),
                    (int)CurrentCacheObject.Animation,
                    CurrentCacheObject.Radius,
                    Player.CurrentHealthPct,
                    CurrentCacheObject.Distance,
                    animationType);
            }

            return true;
        }

        private static bool RefreshAvoidance()
        {
            try
            {
                CurrentCacheObject.Animation = c_diaObject.CommonData.CurrentAnimation;
                CurrentCacheObject.Rotation = c_diaObject.Movement.Rotation;
            }
            catch (Exception ex)
            {
                Logger.LogDebug(LogCategory.CacheManagement, "Error reading CurrentAnimation for AoE sno:{0} raGuid:{1} name:{2} ex:{3}",
                  CurrentCacheObject.ActorSNO, CurrentCacheObject.RActorGuid, CurrentCacheObject.InternalName, ex.Message);
            }

            double minAvoidanceHealth = GetAvoidanceHealth(CurrentCacheObject.ActorSNO);

            float customRadius;
            if (DataDictionary.DefaultAvoidanceCustomRadius.TryGetValue(CurrentCacheObject.ActorSNO, out customRadius))
            {
                CurrentCacheObject.Radius = customRadius;
            }
            else
            {
                CurrentCacheObject.Radius = (float)GetAvoidanceRadius(CurrentCacheObject.ActorSNO, (float)(CurrentCacheObject.Radius * 1.5));
            }

            // Add Navigation cell weights to path around avoidance
            MainGridProvider.AddCellWeightingObstacle(CurrentCacheObject.ActorSNO, (float)CurrentCacheObject.Radius);

            AvoidanceType avoidanceType = AvoidanceManager.GetAvoidanceType(CurrentCacheObject.ActorSNO);

            if (CurrentCacheObject.InternalName.Contains("wall"))
            {
                AddObjectToNavigationObstacleCache();
                return true;
            }

            if ((DataDictionary.AvoidancesAtPlayer.Contains(CurrentCacheObject.ActorSNO) || CurrentCacheObject.IsAvoidanceAtPlayer) &&
                !CacheData.ObsoleteAvoidancesAtPlayer.Contains(CurrentCacheObject.ActorSNO))
            {
                CurrentCacheObject.Position = Trinity.Player.Position;
                CurrentCacheObject.Distance = 0f;

                CacheData.ObsoleteAvoidancesAtPlayer.Add(CurrentCacheObject.ActorSNO);
            }

            // Monks with Serenity up ignore all AOE's
            if (Player.ActorClass == ActorClass.Monk && Hotbar.Contains(SNOPower.Monk_Serenity) && GetHasBuff(SNOPower.Monk_Serenity))
            {
                // Monks with serenity are immune
                minAvoidanceHealth *= V.F("Monk.Avoidance.Serenity");
                Logger.Log(TrinityLogLevel.Debug, LogCategory.Avoidance, "Ignoring avoidance as a Monk with Serenity");
            }
            // Witch doctors with spirit walk available and not currently Spirit Walking will subtly ignore ice balls, arcane, desecrator & plague cloud
            if (Player.ActorClass == ActorClass.Witchdoctor && Hotbar.Contains(SNOPower.Witchdoctor_SpiritWalk) && GetHasBuff(SNOPower.Witchdoctor_SpiritWalk))
            {
                if (avoidanceType == AvoidanceType.IceBall || avoidanceType == AvoidanceType.Arcane || avoidanceType == AvoidanceType.Desecrator || avoidanceType == AvoidanceType.PlagueCloud)
                {
                    // Ignore ICE/Arcane/Desc/PlagueCloud altogether with spirit walk up or available
                    minAvoidanceHealth *= V.F("WitchDoctor.Avoidance.SpiritWalk");
                    Logger.Log(TrinityLogLevel.Debug, LogCategory.Avoidance, "Ignoring avoidance as a WitchDoctor with Spirit Walk");
                }
            }
            // Remove ice balls if the barbarian has wrath of the berserker up, and reduce health from most other SNO avoidances
            if (Player.ActorClass == ActorClass.Barbarian &&
                Settings.Combat.Barbarian.IgnoreAvoidanceInWOTB &&
                Hotbar.Contains(SNOPower.Barbarian_WrathOfTheBerserker) &&
                GetHasBuff(SNOPower.Barbarian_WrathOfTheBerserker))
            {
                switch (avoidanceType)
                {
                    case AvoidanceType.IceBall:
                        minAvoidanceHealth *= V.F("Barbarian.Avoidance.WOTB.IceBall");
                        break;
                    case AvoidanceType.Arcane:
                        minAvoidanceHealth *= V.F("Barbarian.Avoidance.WOTB.Arcane");
                        break;
                    case AvoidanceType.Desecrator:
                        minAvoidanceHealth *= V.F("Barbarian.Avoidance.WOTB.Desecrator");
                        break;
                    case AvoidanceType.Belial:
                        minAvoidanceHealth = V.F("Barbarian.Avoidance.WOTB.Belial");
                        break;
                    case AvoidanceType.PoisonTree:
                        minAvoidanceHealth = V.F("Barbarian.Avoidance.WOTB.PoisonTree");
                        break;
                    case AvoidanceType.BeastCharge:
                        minAvoidanceHealth = V.F("Barbarian.Avoidance.WOTB.BeastCharge");
                        break;
                    case AvoidanceType.MoltenCore:
                        minAvoidanceHealth = V.F("Barbarian.Avoidance.WOTB.MoltenCore");
                        break;
                    default:
                        minAvoidanceHealth *= V.F("Barbarian.Avoidance.WOTB.Other");
                        break;
                }
            }

            // Item based immunity
            switch (avoidanceType)
            {
                case AvoidanceType.PoisonTree:
                case AvoidanceType.PlagueCloud:
                case AvoidanceType.PoisonEnchanted:
                case AvoidanceType.PlagueHand:

                    if (Legendary.MarasKaleidoscope.IsEquipped)
                    {
                        Logger.Log(TrinityLogLevel.Debug, LogCategory.Avoidance, "Ignoring Avoidance {0} because MarasKaleidoscope is equipped", avoidanceType);
                        minAvoidanceHealth = 0;
                    }
                    break;

                case AvoidanceType.AzmoFireball:
                case AvoidanceType.DiabloRingOfFire:
                case AvoidanceType.DiabloMeteor:
                case AvoidanceType.ButcherFloorPanel:
                case AvoidanceType.Mortar:
                case AvoidanceType.MageFire:
                case AvoidanceType.MoltenTrail:
                case AvoidanceType.MoltenBall:
                case AvoidanceType.ShamanFire:

                    if (Legendary.TheStarOfAzkaranth.IsEquipped)
                    {
                        Logger.Log(TrinityLogLevel.Debug, LogCategory.Avoidance, "Ignoring Avoidance {0} because TheStarofAzkaranth is equipped", avoidanceType);
                        minAvoidanceHealth = 0;
                    }
                    break;

                case AvoidanceType.FrozenPulse:
                case AvoidanceType.IceBall:
                case AvoidanceType.IceTrail:

                    // Ignore if both items are equipped
                    if (Legendary.TalismanOfAranoch.IsEquipped)
                    {
                        Logger.Log(TrinityLogLevel.Debug, LogCategory.Avoidance, "Ignoring Avoidance {0} because TalismanofAranoch is equipped", avoidanceType);
                        minAvoidanceHealth = 0;
                    }
                    break;

                case AvoidanceType.Orbiter:
                case AvoidanceType.Thunderstorm:

                    if (Legendary.XephirianAmulet.IsEquipped)
                    {
                        Logger.Log(TrinityLogLevel.Debug, LogCategory.Avoidance, "Ignoring Avoidance {0} because XephirianAmulet is equipped", avoidanceType);
                        minAvoidanceHealth = 0;
                    }
                    break;

                case AvoidanceType.Arcane:
                    if (Legendary.CountessJuliasCameo.IsEquipped)
                    {
                        Logger.Log(TrinityLogLevel.Debug, LogCategory.Avoidance, "Ignoring Avoidance {0} because CountessJuliasCameo is equipped", avoidanceType);
                        minAvoidanceHealth = 0;
                    }
                    break;
            }

            // Set based immunity
            if (Sets.BlackthornesBattlegear.IsMaxBonusActive)
            {
                var blackthornsImmunity = new HashSet<AvoidanceType>
                {
                    AvoidanceType.Desecrator,
                    AvoidanceType.MoltenBall,
                    AvoidanceType.MoltenCore,
                    AvoidanceType.MoltenTrail,
                    AvoidanceType.PlagueHand
                };

                if (blackthornsImmunity.Contains(avoidanceType))
                {
                    Logger.Log(TrinityLogLevel.Debug, LogCategory.Avoidance, "Ignoring Avoidance {0} because BlackthornesBattlegear is equipped", avoidanceType);
                    minAvoidanceHealth = 0;
                }
            }

            if (minAvoidanceHealth == 0)
            {
                Logger.Log(TrinityLogLevel.Info, LogCategory.Avoidance, "Ignoring Avoidance! Name={0} SNO={1} radius={2:0} health={3:0.00} dist={4:0}",
                       CurrentCacheObject.InternalName, CurrentCacheObject.ActorSNO, CurrentCacheObject.Radius, minAvoidanceHealth, CurrentCacheObject.Distance);
                return false;
            }

            TimeSpan aoeExpiration = TimeSpan.FromMilliseconds(500);

            if (DataDictionary.AvoidanceProjectiles.Contains(CurrentCacheObject.ActorSNO))
            {
                var minWidth = Math.Max(8f, CurrentCacheObject.Radius);

                if (Player.Position.Distance2D(CurrentCacheObject.Position) > minWidth)
                {
                    Vector3 endPoint = MathEx.GetPointAt(CurrentCacheObject.Position, 70f, CurrentCacheObject.Rotation);
                    for (float i = 0; i <= CurrentCacheObject.Position.Distance2D(endPoint); i += (float)(minWidth * 0.5))
                    {
                        Vector3 pathSpot = MathEx.CalculatePointFrom(CurrentCacheObject.Position, endPoint, i);

                        if (Player.Position.Distance2D(pathSpot) <= (minWidth * 0.5))
                        {
                            Trinity.Player.TryToAvoidProjectile = Player.CurrentHealthPct <= minAvoidanceHealth;

                            Logger.Log(TrinityLogLevel.Info, LogCategory.Avoidance, "Is standing in experimental avoidance of projectile Name={0} SNO={1} radius={2:0} health={3:0.00} dist={4:0}",
                               CurrentCacheObject.InternalName, CurrentCacheObject.ActorSNO, minWidth, minAvoidanceHealth, CurrentCacheObject.Distance);
            }

                        CacheData.TimeBoundAvoidance.Add(new CacheObstacleObject(pathSpot, minWidth, CurrentCacheObject.ActorSNO, CurrentCacheObject.InternalName)
                        {
                            Expires = DateTime.UtcNow.Add(aoeExpiration),
                            ObjectType = GObjectType.Avoidance,
                            Rotation = CurrentCacheObject.Rotation,
                        });
                    }
                }
                else
                {
                    Logger.Log(TrinityLogLevel.Info, LogCategory.Avoidance, "Is standing in avoidance of projectile impact Name={0} SNO={1} radius={2:0} health={3:0.00} dist={4:0}",
                        CurrentCacheObject.InternalName, CurrentCacheObject.ActorSNO, minWidth, minAvoidanceHealth, CurrentCacheObject.Distance);

                    Trinity.Player.StandingInAvoidance = minAvoidanceHealth >= Player.CurrentHealthPct;

                    CacheData.TimeBoundAvoidance.Add(new CacheObstacleObject(CurrentCacheObject.Position, CurrentCacheObject.Radius, CurrentCacheObject.ActorSNO, CurrentCacheObject.InternalName)
                    {
                        Expires = DateTime.UtcNow.Add(aoeExpiration),
                        ObjectType = GObjectType.Avoidance,
                        Rotation = CurrentCacheObject.Rotation,
                    });
                }
            }
            else
            {
                DataDictionary.AvoidanceSpawnerDuration.TryGetValue(CurrentCacheObject.ActorSNO, out aoeExpiration);

                CacheData.TimeBoundAvoidance.Add(new CacheObstacleObject(CurrentCacheObject.Position, CurrentCacheObject.Radius, CurrentCacheObject.ActorSNO, CurrentCacheObject.InternalName)
                {
                    Expires = DateTime.UtcNow.Add(aoeExpiration),
                    ObjectType = GObjectType.Avoidance,
                    Rotation = CurrentCacheObject.Rotation,
                });

                // Is this one under our feet? If so flag it up so we can find an avoidance spot
                if (CurrentCacheObject.Distance <= CurrentCacheObject.Radius)
                {
                    Trinity.Player.StandingInAvoidance = minAvoidanceHealth >= Player.CurrentHealthPct;

                    // Note if this is a travelling projectile or not so we can constantly update our safe points
                    if (DataDictionary.AvoidanceProjectiles.Contains(CurrentCacheObject.ActorSNO))
                    {
                        Logger.Log(TrinityLogLevel.Info, LogCategory.Avoidance, "Is standing in avoidance for projectile Name={0} SNO={1} radius={2:0} health={3:0.00} dist={4:0}",
                            CurrentCacheObject.InternalName, CurrentCacheObject.ActorSNO, CurrentCacheObject.Radius, minAvoidanceHealth, CurrentCacheObject.Distance);
                    }
                    else
                    {
                        Logger.Log(TrinityLogLevel.Info, LogCategory.Avoidance, "Is standing in avoidance Name={0} SNO={1} radius={2:0} health={3:0.00} dist={4:0}",
                            CurrentCacheObject.InternalName, CurrentCacheObject.ActorSNO, CurrentCacheObject.Radius, minAvoidanceHealth, CurrentCacheObject.Distance);
                    }
                }
            }

            return true;
        }

        private static double GetAvoidanceHealth(int actorSNO = -1)
        {
            // snag our SNO from cache variable if not provided
            if (actorSNO == -1)
                actorSNO = CurrentCacheObject.ActorSNO;
            float hlthDefault = Player.IsRanged ? 0.98f : 0.75f;
            try
            {
                if (actorSNO != -1)
                    return AvoidanceManager.GetAvoidanceHealthBySNO(CurrentCacheObject.ActorSNO, hlthDefault);
                return AvoidanceManager.GetAvoidanceHealthBySNO(actorSNO, hlthDefault);
            }
            catch (Exception ex)
            {
                Logger.Log(TrinityLogLevel.Info, LogCategory.Avoidance, "Exception getting avoidance radius for sno={0}", actorSNO);
                Logger.Log(TrinityLogLevel.Info, LogCategory.Avoidance, ex.ToString());
                // 100% unless specified
                return hlthDefault;
            }
        }
        public static double GetAvoidanceRadius(int actorSNO = -1, float radius = -1f)
        {
            if (actorSNO == -1)
                actorSNO = CurrentCacheObject.ActorSNO;

            if (radius == -1f)
                radius = 20f;

            try
            {
                return AvoidanceManager.GetAvoidanceRadiusBySNO(actorSNO, radius);
            }
            catch (Exception ex)
            {
                Logger.Log(TrinityLogLevel.Info, LogCategory.Avoidance, "Exception getting avoidance radius for sno={0} radius={1}", actorSNO, radius);
                Logger.Log(TrinityLogLevel.Info, LogCategory.Avoidance, ex.ToString());
                return radius;
            }

        }
    }
}
