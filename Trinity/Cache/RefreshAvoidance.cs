using System;
using System.Collections.Generic;
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
        private static bool RAAvoidances()
        {
            if (!Settings.Combat.Misc.AvoidAOE)
                return GetReturn("AvoidDisabled", value: false);

            Anim _aAvoidance;
            if (DataDictionary.AvoidanceAnimations.TryGetValue(c_CurrentAnimation, out _aAvoidance))
            {
                /* Ignore ground effect */
                if (Sets.BlackthornesBattlegear.IsMaxBonusActive && _aAvoidance.GroundEffect)
                    return GetReturn("Blackthornes is max bonus", _aAvoidance);

                /* See immune */
                switch (_aAvoidance.Element)
                {
                    case Element.Arcane: { if (Legendary.CountessJuliasCameo.IsEquipped) return true; } break;
                    case Element.Poison: { if (Legendary.MarasKaleidoscope.IsEquipped) return true; } break;
                    case Element.Fire: { if (Legendary.TheStarOfAzkaranth.IsEquipped) return true; } break;
                    case Element.Cold: { if (Legendary.TalismanOfAranoch.IsEquipped) return true; } break;
                    case Element.Lightning: { if (Legendary.XephirianAmulet.IsEquipped) return true; } break;
                    default: break;
                }

                _aAvoidance.Radius = (float)GetAvoidanceRadius(_aAvoidance.Id, (float)(c_CacheObject.Radius * 1.5));

                bool isStandingInAvoidance = false;
                bool isFacingPlayer = c_IsFacingPlayer || c_CacheObject.IsFacing(Player.Position, 15f);
                double minAvoidanceHealth = GetAvoidanceHealth(c_CacheObject.ActorSNO);
                bool isBelowHealthThreshold = Player.CurrentHealthPct <= minAvoidanceHealth;

                /* Set avoidance */
                switch (_aAvoidance.Type)
                {
                    case AvoidType.Leap:
                    case AvoidType.Teleport:
                    case AvoidType.Dash:
                        {
                            if (isBelowHealthThreshold)
                            {
                                if (c_TargetACDGuid != -1 && c_TargetACDGuid != Player.ACDGuid)
                                    return GetReturn("targetACDGuid isn't player", _aAvoidance);

                                if (c_TargetACDPosition != Vector3.Zero && c_TargetACDPosition.Distance2D(Player.Position) > _aAvoidance.Radius)
                                    return GetReturn("Anim targetACDPosition isn't player", _aAvoidance);

                                isStandingInAvoidance = isFacingPlayer || c_TargetACDGuid == Player.ACDGuid || c_TargetACDPosition.Distance2D(Player.Position) <= _aAvoidance.Radius; 
                            }

                        } break;
                    case AvoidType.GenericCast:
                    case AvoidType.Attack:
                    case AvoidType.MeleeAttack:
                    case AvoidType.AttackLeft:
                    case AvoidType.AttackRight:
                    case AvoidType.Strafe:
                    case AvoidType.StrafeLeft:
                    case AvoidType.StrafeRight:
                        {
                            if (isBelowHealthThreshold)
                            {
                                if (c_TargetACDGuid != -1 && c_TargetACDGuid != Player.ACDGuid)
                                    return GetReturn("targetACDGuid isn't player", _aAvoidance);

                                if (c_TargetACDPosition != Vector3.Zero && c_TargetACDPosition.Distance2D(Player.Position) > _aAvoidance.Radius)
                                    return GetReturn("Anim targetACDPosition isn't player", _aAvoidance);

                                isStandingInAvoidance = (isFacingPlayer || c_TargetACDGuid == Player.ACDGuid || c_TargetACDPosition.Distance2D(Player.Position) <= _aAvoidance.Radius) &&
                                    c_CacheObject.Distance <= _aAvoidance.Radius; 
                            }

                        } break;
                    case AvoidType.Charge:
                    case AvoidType.Projectile:
                    case AvoidType.RangedAttack:
                        {
                            if (isBelowHealthThreshold)
                            {
                                if (c_TargetACDGuid != -1 && c_TargetACDGuid != Player.ACDGuid)
                                    return GetReturn("Anim target isn't player", _aAvoidance);

                                if (c_TargetACDPosition != new Vector3() && c_TargetACDPosition.Distance2D(Player.Position) > _aAvoidance.Radius)
                                    return GetReturn("Anim targetACDPosition isn't player", _aAvoidance);

                                isStandingInAvoidance = (isFacingPlayer || c_TargetACDGuid == Player.ACDGuid || c_TargetACDPosition.Distance2D(Player.Position) <= _aAvoidance.Radius) &&
                                    c_CacheObject.Distance <= _aAvoidance.Radius; 
                            }

                            Vector3 _targetPoint = c_TargetACDPosition != new Vector3() ? 
                                c_TargetACDPosition : MathEx.GetPointAt(c_CacheObject.Position, 40f, c_CacheObject.Rotation);

                            for (float i = 0; i <= c_CacheObject.Position.Distance2D(_targetPoint); i += 5f)
                            {
                                Vector3 pathSpot = MathEx.CalculatePointFrom(c_CacheObject.Position, _targetPoint, i);

                                CacheData.AvoidanceObstacles.Add(new CacheObstacleObject(pathSpot, _aAvoidance.Radius, c_CacheObject.ActorSNO, c_CurrentAnimation.ToString())
                                {
                                    ObjectType = GObjectType.Avoidance,
                                    Rotation = c_CacheObject.Rotation,
                                    Animation = c_CurrentAnimation,
                                    AvoidType = _aAvoidance.Type,
                                    IsAvoidanceAnimations = true,
                                });

                                /* Player standing in avoidance ? */
                                isStandingInAvoidance = isBelowHealthThreshold && Player.Position.Distance2D(pathSpot) <= _aAvoidance.Radius;
                            }

                        } break;
                    case AvoidType.GroundCircle:
                    case AvoidType.Bomb:
                    case AvoidType.RotateLeft:
                    case AvoidType.RotateRight:
                    default:
                        {
                            isStandingInAvoidance = isBelowHealthThreshold && c_CacheObject.Distance <= _aAvoidance.Radius;
                        }
                        break;
                }

                if (isStandingInAvoidance)
                {
                    Logger.Log(TrinityLogLevel.Info, LogCategory.Avoidance,
                        "Is Standing in avoidance of Anim={0} - {1} Type={2} Elmt={3} Rad={4}",
                        _aAvoidance.Name, _aAvoidance.Id, _aAvoidance.Type, _aAvoidance.Element, _aAvoidance.Radius);

                    Player.StandingInAvoidance = true;

                    //if (!Player.IsRanged && Trinity.KillMonstersInAoE)
                    //    Trinity.KillMonstersInAoE = false;
                }

                CacheData.AvoidanceObstacles.Add(new CacheObstacleObject(c_CacheObject.Position, c_CacheObject.Radius, c_CacheObject.ActorSNO, c_CurrentAnimation.ToString())
                {
                    ObjectType = GObjectType.Avoidance,
                    Rotation = c_CacheObject.Rotation,
                    Animation = c_CurrentAnimation,
                    AvoidType = _aAvoidance.Type,
                    IsAvoidanceAnimations = true,
                });

                return GetReturn("Added to cache", _aAvoidance);
            }

            return GetReturn("NotAAvoidance", value: false);
        }

        private static bool GetReturn(string post, Anim anim = null, bool value = true)
        {
            if (anim != null)
            {
                Logger.Log(TrinityLogLevel.Verbose, LogCategory.Avoidance,
                    "RAAvoidance return {0} reason={1}, Anim={2} - {3} Type={4} Elmt={5} Rad={6}",
                    value, post, anim.Name, anim.Id, anim.Type, anim.Element, anim.Radius);
            }

            if (!value) { c_InfosSubStep += post + " "; }
            return value;
        }

        private static bool RefreshAvoidance()
        {
            double minAvoidanceHealth = GetAvoidanceHealth(c_CacheObject.ActorSNO);

            float customRadius;
            if (DataDictionary.DefaultAvoidanceCustomRadius.TryGetValue(c_CacheObject.ActorSNO, out customRadius))
            {
                c_CacheObject.Radius = customRadius;
            }
            else
            {
                c_CacheObject.Radius = (float)GetAvoidanceRadius(c_CacheObject.ActorSNO, (float)(c_CacheObject.Radius * 1.5));
            }

            // Add Navigation cell weights to path around avoidance
            MainGridProvider.AddCellWeightingObstacle(c_CacheObject.ActorSNO, (float)c_CacheObject.Radius);

            AvoidanceType avoidanceType = AvoidanceManager.GetAvoidanceType(c_CacheObject.ActorSNO);

            if (c_CacheObject.InternalName.Contains("wall"))
            {
                AddObjectToNavigationObstacleCache();
                //return true;
            }

            if (DataDictionary.AvoidancesAtPlayer.Contains(c_CacheObject.ActorSNO) && !CacheData.ObsoleteAvoidancesAtPlayer.Contains(c_CacheObject.ActorSNO))
            {
                c_CacheObject.Position = Trinity.Player.Position;
                c_CacheObject.Distance = 0f;

                CacheData.ObsoleteAvoidancesAtPlayer.Add(c_CacheObject.ActorSNO);
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

            if (c_Avoidance != null)
            {
                switch (c_Avoidance.Element)
                {
                    case Element.Arcane: { if (Legendary.CountessJuliasCameo.IsEquipped) return true; } break;
                    case Element.Poison: { if (Legendary.MarasKaleidoscope.IsEquipped) return true; } break;
                    case Element.Fire: { if (Legendary.TheStarOfAzkaranth.IsEquipped) return true; } break;
                    case Element.Cold: { if (Legendary.TalismanOfAranoch.IsEquipped) return true; } break;
                    case Element.Lightning: { if (Legendary.XephirianAmulet.IsEquipped) return true; } break;
                    default: break;
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
                       c_CacheObject.InternalName, c_CacheObject.ActorSNO, c_CacheObject.Radius, minAvoidanceHealth, c_CacheObject.Distance);
                return false;
            }

            TimeSpan aoeExpiration = TimeSpan.FromMilliseconds(500);
            DataDictionary.AvoidanceSpawnerDuration.TryGetValue(c_CacheObject.ActorSNO, out aoeExpiration);

            CacheData.AvoidanceObstacles.Add(new CacheObstacleObject(c_CacheObject.Position, c_CacheObject.Radius, c_CacheObject.ActorSNO, c_CacheObject.InternalName)
            {
                Expires = DateTime.UtcNow.Add(aoeExpiration),
                ObjectType = GObjectType.Avoidance,
                Rotation = c_CacheObject.Rotation,
                AvoidType = AvoidType.GroundCircle,
            });

            if (c_Avoidance != null && c_Avoidance.Type == AvoidType.Projectile)
            {
                if (Player.Position.Distance2D(c_CacheObject.Position) > c_CacheObject.Radius)
                {
                    Vector3 _targetPoint = c_TargetACDPosition != new Vector3() ?
                        c_TargetACDPosition : MathEx.GetPointAt(c_CacheObject.Position, 40f, c_CacheObject.Rotation);

                    for (float i = 0; i <= c_CacheObject.Position.Distance2D(_targetPoint); i += 5f)
                    {
                        Vector3 _pathSpot = MathEx.CalculatePointFrom(c_CacheObject.Position, _targetPoint, i);

                        CacheData.AvoidanceObstacles.Add(new CacheObstacleObject(c_CacheObject.Position, c_CacheObject.Radius, c_CacheObject.ActorSNO, c_CacheObject.InternalName)
                        {
                            Expires = DateTime.UtcNow.Add(aoeExpiration),
                            ObjectType = GObjectType.Avoidance,
                            Rotation = c_CacheObject.Rotation,
                            AvoidType = AvoidType.Projectile,
                        });

                        /* Player standing in avoidance ? */
                        Trinity.Player.StandingInAvoidance = Player.Position.Distance2D(_pathSpot) <= c_CacheObject.Radius;
                    }
                }
                else
                {
                    Logger.Log(TrinityLogLevel.Info, LogCategory.Avoidance, "Is standing in avoidance of projectile impact Name={0} SNO={1} radius={2:0} health={3:0.00} dist={4:0}",
                        c_CacheObject.InternalName, c_CacheObject.ActorSNO, c_CacheObject.Radius, minAvoidanceHealth, c_CacheObject.Distance);

                    Trinity.Player.StandingInAvoidance = minAvoidanceHealth >= Player.CurrentHealthPct;
                }

                
            }
            else
            {
                // Is this one under our feet? If so flag it up so we can find an avoidance spot
                if (c_CacheObject.Distance <= c_CacheObject.Radius)
                {
                    Trinity.Player.StandingInAvoidance = minAvoidanceHealth >= Player.CurrentHealthPct;

                    Logger.Log(TrinityLogLevel.Info, LogCategory.Avoidance, "Is standing in avoidance Name={0} SNO={1} radius={2:0} health={3:0.00} dist={4:0}",
                        c_CacheObject.InternalName, c_CacheObject.ActorSNO, c_CacheObject.Radius, minAvoidanceHealth, c_CacheObject.Distance);
                } 
            }

            return true;
        }

        private static double GetAvoidanceHealth(int actorSNO = -1)
        {
            // snag our SNO from cache variable if not provided
            if (actorSNO == -1)
                actorSNO = c_CacheObject.ActorSNO;
            float hlthDefault = Player.IsRanged ? 0.98f : 0.75f;
            try
            {
                if (actorSNO != -1)
                    return AvoidanceManager.GetAvoidanceHealthBySNO(c_CacheObject.ActorSNO, hlthDefault);
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
                actorSNO = c_CacheObject.ActorSNO;

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
