using System;
using Trinity.Config.Combat;
using Trinity.DbProvider;
using Trinity.Technicals;
using Zeta.Common.Plugins;
using Zeta.Internals.Actors;
using Zeta.Internals.Actors.Gizmos;
using Zeta.Navigation;

namespace Trinity
{
    public partial class Trinity : IPlugin
    {
        private static bool RefreshAvoidance(bool AddToCache)
        {
            AddToCache = true;

            // Retrieve collision sphere radius, cached if possible
            if (!collisionSphereCache.TryGetValue(c_ActorSNO, out c_Radius))
            {
                try
                {
                    c_Radius = c_diaObject.CollisionSphere.Radius;
                    if (c_Radius <= 5f)
                        c_Radius = 5f;
                }
                catch (Exception ex)
                {
                    Logger.Log(TrinityLogLevel.Debug, LogCategory.CacheManagement, "Safely handled exception getting collisionsphere radius for Avoidance {0} [{1}]", c_InternalName, c_ActorSNO);
                    Logger.Log(TrinityLogLevel.Debug, LogCategory.CacheManagement, "{0}", ex);
                    AddToCache = false;
                    return AddToCache;
                }
                collisionSphereCache.Add(c_ActorSNO, c_Radius);
            }

            try
            {
                c_CurrentAnimation = c_CommonData.CurrentAnimation;
            }
            catch { }

            double minAvoidanceHealth = GetAvoidanceHealth(c_ActorSNO);
            double minAvoidanceRadius = GetAvoidanceRadius(c_ActorSNO, c_Radius);

            // cap avoidance to 30f maximum
            minAvoidanceRadius = Math.Min(30f, minAvoidanceRadius);

            // Are we allowed to path around avoidance?
            if (Settings.Combat.Misc.AvoidanceNavigation)
            {
                ((MainGridProvider)MainGridProvider).AddCellWeightingObstacle(c_ActorSNO, (float)minAvoidanceRadius);
            }

            AvoidanceType avoidanceType = AvoidanceManager.GetAvoidanceType(c_ActorSNO);

            // Monks with Serenity up ignore all AOE's
            if (Player.ActorClass == ActorClass.Monk && Hotbar.Contains(SNOPower.Monk_Serenity) && GetHasBuff(SNOPower.Monk_Serenity))
            {
                // Monks with serenity are immune
                minAvoidanceHealth *= V.F("Monk.Avoidance.Serenity");
                Logger.Log(TrinityLogLevel.Debug, LogCategory.Avoidance, "Ignoring avoidance as a Monk with Serenity");
            }
            // Witch doctors with spirit walk available and not currently Spirit Walking will subtly ignore ice balls, arcane, desecrator & plague cloud
            if (Player.ActorClass == ActorClass.WitchDoctor && Hotbar.Contains(SNOPower.Witchdoctor_SpiritWalk) && GetHasBuff(SNOPower.Witchdoctor_SpiritWalk))
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
                if (avoidanceType == AvoidanceType.IceBall)
                {
                    minAvoidanceHealth *= V.F("Barbarian.Avoidance.WOTB.IceBall");
                }
                else if (avoidanceType == AvoidanceType.Arcane)
                {
                    minAvoidanceHealth *= V.F("Barbarian.Avoidance.WOTB.Arcane");
                }
                else if (avoidanceType == AvoidanceType.Desecrator)
                {
                    minAvoidanceHealth *= V.F("Barbarian.Avoidance.WOTB.Desecrator");
                }
                else if (avoidanceType == AvoidanceType.Belial)
                {
                    minAvoidanceHealth = 1;
                }
                else
                    // Anything else
                    minAvoidanceHealth *= V.F("Barbarian.Avoidance.WOTB.Other");

            }

            if (minAvoidanceHealth == 0)
            {
                AddToCache = false;
                Logger.Log(TrinityLogLevel.Verbose, LogCategory.Avoidance, "Ignoring Avoidance! Name={0} SNO={1} radius={2:0} health={3:0.00} dist={4:0}",
                       c_InternalName, c_ActorSNO, minAvoidanceRadius, minAvoidanceHealth, c_CentreDistance);
                return AddToCache;

            }

            // Add it to the list of known avoidance objects, *IF* our health is lower than this avoidance health limit
            if (minAvoidanceHealth >= Player.CurrentHealthPct)
            {
                // Generate a "weight" for how badly we want to avoid this obstacle, based on a percentage of 100% the avoidance health is, multiplied into a max of 200 weight
                double dThisWeight = (200 * minAvoidanceHealth);

                float avoidanceRadius = (float)GetAvoidanceRadius();

                AvoidanceObstacleCache.Add(new CacheObstacleObject(c_Position, avoidanceRadius, c_ActorSNO, dThisWeight, c_InternalName));

                // Is this one under our feet? If so flag it up so we can find an avoidance spot
                if (c_CentreDistance <= minAvoidanceRadius)
                {
                    StandingInAvoidance = true;

                    // Note if this is a travelling projectile or not so we can constantly update our safe points
                    if (DataDictionary.AvoidanceProjectiles.Contains(c_ActorSNO))
                    {
                        IsAvoidingProjectiles = true;
                        Logger.Log(TrinityLogLevel.Verbose, LogCategory.Avoidance, "Is standing in avoidance for projectile Name={0} SNO={1} radius={2:0} health={3:0.00} dist={4:0}",
                           c_InternalName, c_ActorSNO, minAvoidanceRadius, minAvoidanceHealth, c_CentreDistance);
                    }
                    else
                    {
                        Logger.Log(TrinityLogLevel.Verbose, LogCategory.Avoidance, "Is standing in avoidance Name={0} SNO={1} radius={2:0} health={3:0.00} dist={4:0}",
                       c_InternalName, c_ActorSNO, minAvoidanceRadius, minAvoidanceHealth, c_CentreDistance);
                    }
                }
                else
                {
                    Logger.Log(TrinityLogLevel.Verbose, LogCategory.Avoidance, "NOT standing in Avoidance Name={0} SNO={1} radius={2:0} health={3:0.00} dist={4:0}",
                   c_InternalName, c_ActorSNO, minAvoidanceRadius, minAvoidanceHealth, c_CentreDistance);
                }
            }
            else
            {
                Logger.Log(TrinityLogLevel.Verbose, LogCategory.Avoidance, "Enough health for avoidance, ignoring Name={0} SNO={1} radius={2:0} health={3:0.00} dist={4:0}",
               c_InternalName, c_ActorSNO, minAvoidanceRadius, minAvoidanceHealth, c_CentreDistance);
            }

            return AddToCache;
        }
        private static double GetAvoidanceHealth(int actorSNO = -1)
        {
            // snag our SNO from cache variable if not provided
            if (actorSNO == -1)
                actorSNO = c_ActorSNO;
            try
            {
                if (actorSNO != -1)
                    return AvoidanceManager.GetAvoidanceHealthBySNO(c_ActorSNO, 1);
                else
                    return AvoidanceManager.GetAvoidanceHealthBySNO(actorSNO, 1);
            }
            catch (Exception ex)
            {
                Logger.Log(TrinityLogLevel.Normal, LogCategory.Avoidance, "Exception getting avoidance radius for sno={0}", actorSNO);
                Logger.Log(TrinityLogLevel.Normal, LogCategory.Avoidance, ex.ToString());
                // 100% unless specified
                return 1;
            }
        }
        private static double GetAvoidanceRadius(int actorSNO = -1, float radius = -1f)
        {
            if (actorSNO == -1)
                actorSNO = c_ActorSNO;

            if (radius == -1f)
                radius = 20f;

            try
            {

                return AvoidanceManager.GetAvoidanceRadiusBySNO(actorSNO, radius);
            }
            catch (Exception ex)
            {
                Logger.Log(TrinityLogLevel.Normal, LogCategory.Avoidance, "Exception getting avoidance radius for sno={0} radius={1}", actorSNO, radius);
                Logger.Log(TrinityLogLevel.Normal, LogCategory.Avoidance, ex.ToString());
                return radius;
            }

        }
    }
}
