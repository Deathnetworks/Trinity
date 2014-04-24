using System;
using Trinity.Technicals;
using Zeta.Bot.Navigation;
using Zeta.Common.Plugins;
using Zeta.Game;
using Zeta.Game.Internals.Actors;

namespace Trinity
{
    public partial class Trinity : IPlugin
    {
        private static bool RefreshAvoidance(bool AddToCache)
        {
            AddToCache = true;
            
            try
            {
                CurrentCacheObject.Animation = CurrentCacheObject.Object.CommonData.CurrentAnimation;
            }
            catch (Exception ex)
            {
                Logger.LogDebug(LogCategory.CacheManagement, "Error reading CurrentAnimation for AoE sno:{0} raGuid:{1} name:{2} ex:{3}",
                  CurrentCacheObject.ActorSNO, CurrentCacheObject.RActorGuid, CurrentCacheObject.InternalName, ex.Message);
            }
            try
            {
                CurrentCacheObject.DirectionVector = CurrentCacheObject.Object.Movement.DirectionVector;
            }
            catch (Exception ex)
            {
                Logger.LogDebug(LogCategory.CacheManagement, "Error reading DirectionVector for AoE sno:{0} raGuid:{1} name:{2} ex:{3}",
                  CurrentCacheObject.ActorSNO, CurrentCacheObject.RActorGuid, CurrentCacheObject.InternalName, ex.Message);
            }
            try
            {
                CurrentCacheObject.Rotation = CurrentCacheObject.Object.Movement.Rotation;
            }
            catch (Exception ex)
            {
                Logger.LogDebug(LogCategory.CacheManagement, "Error reading Rotation for AoE sno:{0} raGuid:{1} name:{2} ex:{3}",
                  CurrentCacheObject.ActorSNO, CurrentCacheObject.RActorGuid, CurrentCacheObject.InternalName, ex.Message);
            }
            try
            {
                CurrentCacheObject.AABBBounds = CurrentCacheObject.Object.ActorInfo.AABBBounds;
            }
            catch (Exception ex)
            {
                Logger.LogDebug(LogCategory.CacheManagement, "Error reading AABBBounds for AoE sno:{0} raGuid:{1} name:{2} ex:{3}",
                  CurrentCacheObject.ActorSNO, CurrentCacheObject.RActorGuid, CurrentCacheObject.InternalName, ex.Message);
            }

            double minAvoidanceHealth = GetAvoidanceHealth(CurrentCacheObject.ActorSNO);
            double minAvoidanceRadius = GetAvoidanceRadius(CurrentCacheObject.ActorSNO, CurrentCacheObject.Radius);

            // cap avoidance to 30f maximum
            minAvoidanceRadius = Math.Min(30f, minAvoidanceRadius);

            // Are we allowed to path around avoidance?
            if (Settings.Combat.Misc.AvoidanceNavigation)
            {
                ((MainGridProvider)MainGridProvider).AddCellWeightingObstacle(CurrentCacheObject.ActorSNO, (float)minAvoidanceRadius);
            }

            AvoidanceType avoidanceType = AvoidanceManager.GetAvoidanceType(CurrentCacheObject.ActorSNO);

            // Beast Charge should set aoe position as players current position!
            if (avoidanceType == AvoidanceType.BeastCharge)
                CurrentCacheObject.Position = Trinity.Player.Position;

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
                    default:
                        minAvoidanceHealth *= V.F("Barbarian.Avoidance.WOTB.Other");
                        break;
                }
            }

            if (minAvoidanceHealth == 0)
            {
                AddToCache = false;
                Logger.Log(TrinityLogLevel.Verbose, LogCategory.Avoidance, "Ignoring Avoidance! Name={0} SNO={1} radius={2:0} health={3:0.00} dist={4:0}",
                       CurrentCacheObject.InternalName, CurrentCacheObject.ActorSNO, minAvoidanceRadius, minAvoidanceHealth, CurrentCacheObject.Distance);
                return AddToCache;

            }

            // Add it to the list of known avoidance objects, *IF* our health is lower than this avoidance health limit
            if (minAvoidanceHealth >= Player.CurrentHealthPct)
            {
                float avoidanceRadius = (float)GetAvoidanceRadius(CurrentCacheObject.ActorSNO, CurrentCacheObject.Radius);

                TimeSpan aoeExpiration;
                DataDictionary.AvoidanceSpawnerDuration.TryGetValue(CurrentCacheObject.ActorSNO, out aoeExpiration);

                CacheData.TimeBoundAvoidance.Add(new CacheObstacleObject(CurrentCacheObject.Position, avoidanceRadius, CurrentCacheObject.ActorSNO, CurrentCacheObject.InternalName)
                {
                    Expires = DateTime.UtcNow.Add(aoeExpiration),
                    ObjectType = GObjectType.Avoidance,
                    Rotation = CurrentCacheObject.Rotation
                });

                // Is this one under our feet? If so flag it up so we can find an avoidance spot
                if (CurrentCacheObject.Distance <= minAvoidanceRadius)
                {
                    StandingInAvoidance = true;

                    // Note if this is a travelling projectile or not so we can constantly update our safe points
                    if (DataDictionary.AvoidanceProjectiles.Contains(CurrentCacheObject.ActorSNO))
                    {
                        IsAvoidingProjectiles = true;
                        Logger.Log(TrinityLogLevel.Verbose, LogCategory.Avoidance, "Is standing in avoidance for projectile Name={0} SNO={1} radius={2:0} health={3:0.00} dist={4:0}",
                           CurrentCacheObject.InternalName, CurrentCacheObject.ActorSNO, minAvoidanceRadius, minAvoidanceHealth, CurrentCacheObject.Distance);
                    }
                    else
                    {
                        Logger.Log(TrinityLogLevel.Verbose, LogCategory.Avoidance, "Is standing in avoidance Name={0} SNO={1} radius={2:0} health={3:0.00} dist={4:0}",
                            CurrentCacheObject.InternalName, CurrentCacheObject.ActorSNO, minAvoidanceRadius, minAvoidanceHealth, CurrentCacheObject.Distance);
                    }
                }
                else
                {
                    // Logger.Log(TrinityLogLevel.Verbose, LogCategory.Avoidance, "NOT standing in Avoidance Name={0} SNO={1} radius={2:0} health={3:0.00} dist={4:0}",
                    //    CurrentCacheObject.InternalName, CurrentCacheObject.ActorSNO, minAvoidanceRadius, minAvoidanceHealth, c_CentreDistance);
                }
            }
            else
            {
                //Logger.Log(TrinityLogLevel.Verbose, LogCategory.Avoidance, "Enough health for avoidance, ignoring Name={0} SNO={1} radius={2:0} health={3:0.00} dist={4:0}",
                //CurrentCacheObject.InternalName, CurrentCacheObject.ActorSNO, minAvoidanceRadius, minAvoidanceHealth, c_CentreDistance);
            }

            return AddToCache;
        }
        private static double GetAvoidanceHealth(int actorSNO = -1)
        {
            // snag our SNO from cache variable if not provided
            if (actorSNO == -1)
                actorSNO = CurrentCacheObject.ActorSNO;
            try
            {
                if (actorSNO != -1)
                    return AvoidanceManager.GetAvoidanceHealthBySNO(CurrentCacheObject.ActorSNO, 1);
                else
                    return AvoidanceManager.GetAvoidanceHealthBySNO(actorSNO, 1);
            }
            catch (Exception ex)
            {
                Logger.Log(TrinityLogLevel.Info, LogCategory.Avoidance, "Exception getting avoidance radius for sno={0}", actorSNO);
                Logger.Log(TrinityLogLevel.Info, LogCategory.Avoidance, ex.ToString());
                // 100% unless specified
                return 1;
            }
        }
        private static double GetAvoidanceRadius(int actorSNO = -1, float radius = -1f)
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
