using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Trinity.Combat.Abilities;
using Trinity.DbProvider;
using Trinity.Technicals;
using Zeta.Bot.Navigation;
using Zeta.Common;
using Zeta.Game;
using Zeta.Game.Internals.Actors;
using Logger = Trinity.Technicals.Logger;

namespace Trinity
{
    class TargetUtil
    {
        #region Helper fields

        private static List<TrinityCacheObject> ObjectCache
        {
            get
            {
                return Trinity.ObjectCache;
            }
        }
        private static PlayerInfoCache Player
        {
            get
            {
                return Trinity.Player;
            }
        }
        private static bool AnyTreasureGoblinsPresent
        {
            get
            {
                if (ObjectCache != null)
                    return ObjectCache.Any(u => u.IsTreasureGoblin);
                else
                    return false;
            }
        }
        private static TrinityCacheObject CurrentTarget
        {
            get
            {
                return Trinity.CurrentTarget;
            }
        }
        private static List<SNOPower> Hotbar
        {
            get
            {
                return Trinity.Hotbar;
            }
        }
        #endregion

        /// <summary>
        /// Gets the number of units facing player
        /// </summary>
        /// <param name="range"></param>
        /// <returns></returns>
        internal static int UnitsFacingPlayer(float range)
        {
            return
                (from u in ObjectCache
                 where u.IsUnit &&
                 u.IsFacingPlayer
                 select u).Count();
        }

        /// <summary>
        /// Gets the number of units player is facing
        /// </summary>
        /// <param name="range"></param>
        /// <returns></returns>
        internal static int UnitsPlayerFacing(float range, float arcDegrees = 70f)
        {
            return
                (from u in ObjectCache
                 where u.IsUnit &&
                 u.IsPlayerFacing(arcDegrees)
                 select u).Count();
        }

        /// <summary>
        /// If ignoring elites, checks to see if enough trash trash pack are around
        /// </summary>
        /// <param name="range"></param>
        /// <returns></returns>
        internal static bool EliteOrTrashInRange(float range)
        {
            if (CombatBase.IgnoringElites)
            {
                return
                    (from u in ObjectCache
                     where u.IsUnit &&
                     !u.IsEliteRareUnique &&
                     u.Weight > 0 &&
                     u.RadiusDistance <= Trinity.Settings.Combat.Misc.TrashPackClusterRadius
                     select u).Count() >= Trinity.Settings.Combat.Misc.TrashPackSize;
            }
            else
            {
                return
                    (from u in ObjectCache
                     where u.IsUnit &&
                     u.Weight > 0 &&
                     u.IsBossOrEliteRareUnique &&
                     u.RadiusDistance <= range
                     select u).Any();
            }

        }


        /// <summary>
        /// Checks to make sure there's at least one valid cluster with the minimum monster count
        /// </summary>
        /// <param name="radius"></param>
        /// <param name="maxRange"></param>
        /// <param name="minCount"></param>
        /// <param name="forceElites"></param>
        /// <returns></returns>
        internal static bool ClusterExists(float radius = 15f, int minCount = 2)
        {
            return ClusterExists(radius, 300f, minCount, false);
        }
        /// <summary>
        /// Checks to make sure there's at least one valid cluster with the minimum monster count
        /// </summary>
        internal static bool ClusterExists(float radius = 15f, float maxRange = 90f, int minCount = 2, bool forceElites = true)
        {
            if (radius < 5f)
                radius = 5f;
            if (maxRange > 300f)
                maxRange = 300f;
            if (minCount < 1)
                minCount = 1;

            if (forceElites && ObjectCache.Any(u => u.IsUnit && u.IsBossOrEliteRareUnique && u.RadiusDistance < maxRange))
                return true;

            var clusterCheck =
                (from u in ObjectCache
                 where u.IsUnit &&
                 u.RadiusDistance <= maxRange &&
                 u.NearbyUnitsWithinDistance(radius) >= minCount
                 select u).Any();

            return clusterCheck;
        }
        /// <summary>
        /// Return a cluster of specified size and radius
        /// </summary>
        internal static Vector3 GetClusterPoint(float clusterRadius = 15f, int minCount = 2)
        {
            if (clusterRadius < 5f)
                clusterRadius = 5f;
            if (minCount < 1)
                minCount = 1;

            if (CurrentTarget == null)
                return Player.Position;

            if (ObjectCache.Any(u => u.IsUnit && u.IsBossOrEliteRareUnique && u.RadiusDistance < 200))
                return CurrentTarget.Position;

            var clusterUnit =
                (from u in ObjectCache
                 where u.IsUnit && u.CommonData!= null && u.CommonData.IsValid &&
                 u.RadiusDistance <= 200 &&
                 u.NearbyUnitsWithinDistance(clusterRadius) >= minCount
                 orderby u.NearbyUnitsWithinDistance(clusterRadius)
                 select u).FirstOrDefault();

            if (clusterUnit == null)
                return CurrentTarget.Position;

            return clusterUnit.Position;
        }

        internal static TrinityCacheObject GetBestPierceTarget(float maxRange, int arcDegrees = 0)
        {
            var result =
                (from u in ObjectCache
                 where u.IsUnit &&
                 u.RadiusDistance <= maxRange
                 orderby u.IsEliteRareUnique descending
                 orderby u.CountUnitsInFront() descending
                 select u).FirstOrDefault();
            if (result == null && CurrentTarget != null)
                result = CurrentTarget;
            else
                result = GetBestClusterUnit(15f, maxRange);
            return result;
        }

        internal static TrinityCacheObject GetBestArcTarget(float maxRange, float arcDegrees)
        {
            var result =
                (from u in ObjectCache
                 where u.IsUnit &&
                 u.RadiusDistance <= maxRange
                 orderby u.IsEliteRareUnique descending
                 orderby u.CountUnitsInFront() descending
                 select u).FirstOrDefault();

            if (result != null) return result;

            if (CurrentTarget != null)
                return CurrentTarget;
            return GetBestClusterUnit(15f, maxRange);
        }

        private static Vector3 GetBestAoEMovementPosition()
        {
            Vector3 _bestMovementPosition = Vector3.Zero;

            if (HealthGlobeExists(25) && Player.CurrentHealthPct < Trinity.Settings.Combat.Barbarian.HealthGlobeLevel)
                _bestMovementPosition = GetBestHealthGlobeClusterPoint(7, 25);
            else if (PowerGlobeExists(25))
                _bestMovementPosition = GetBestPowerGlobeClusterPoint(7, 25);
            else if (GetFarthestClusterUnit(7, 25, 4) != null && !CurrentTarget.IsEliteRareUnique && !CurrentTarget.IsTreasureGoblin)
                _bestMovementPosition = GetFarthestClusterUnit(7, 25).Position;
            else if (_bestMovementPosition == Vector3.Zero)
                _bestMovementPosition = CurrentTarget.Position;

            return _bestMovementPosition;
        }

        internal static TrinityCacheObject GetFarthestClusterUnit(float aoe_radius = 25f, float maxRange = 65f, int count = 1, bool useWeights = true, bool includeUnitsInAoe = true)
        {
            if (aoe_radius < 1f)
                aoe_radius = 1f;
            if (maxRange > 300f)
                maxRange = 300f;

            using (new PerformanceLogger("TargetUtil.GetFarthestClusterUnit"))
            {
                TrinityCacheObject bestClusterUnit;
                var clusterUnits =
                    (from u in ObjectCache
                     where ((useWeights && u.Weight > 0) || !useWeights) &&
                     (includeUnitsInAoe || !UnitOrPathInAoE(u)) &&
                     u.RadiusDistance <= maxRange &&
                     u.NearbyUnitsWithinDistance(aoe_radius) >= count
                     orderby u.Type != GObjectType.HealthGlobe && u.Type != GObjectType.PowerGlobe
                     orderby u.NearbyUnitsWithinDistance(aoe_radius)
                     orderby u.Distance descending
                     select u).ToList();

                if (clusterUnits.Any())
                    bestClusterUnit = clusterUnits.FirstOrDefault();
                else if (Trinity.CurrentTarget != null)
                    bestClusterUnit = Trinity.CurrentTarget;
                else
                    bestClusterUnit = default(TrinityCacheObject);

                return bestClusterUnit;
            }
        }
        /// <summary>
        /// Finds the optimal cluster position, works regardless if there is a cluster or not (will return single unit position if not). This is not a K-Means cluster, but rather a psuedo cluster based
        /// on the number of other monsters within a radius of any given unit
        /// </summary>
        /// <param name="radius">The maximum distance between monsters to be considered part of a cluster</param>
        /// <param name="maxRange">The maximum unit range to include, units further than this will not be checked as a cluster center but may be included in a cluster</param>
        /// <param name="useWeights">Whether or not to included un-weighted (ignored) targets in the cluster finding</param>
        /// <param name="includeUnitsInAoe">Checks the cluster point for AoE effects</param>
        /// <returns>The Vector3 position of the unit that is the ideal "center" of a cluster</returns>
        internal static Vector3 GetBestHealthGlobeClusterPoint(float radius = 15f, float maxRange = 65f, bool useWeights = true, bool includeUnitsInAoe = true)
        {
            if (radius < 5f)
                radius = 5f;
            if (maxRange > 30f)
                maxRange = 30f;

            Vector3 bestClusterPoint;
            var clusterUnits =
                (from u in ObjectCache
                 where u.Type == GObjectType.HealthGlobe &&
                 (includeUnitsInAoe || !UnitOrPathInAoE(u)) &&
                 u.RadiusDistance <= maxRange
                 orderby u.NearbyUnitsWithinDistance(radius)
                 orderby u.Distance descending
                 select u.Position).ToList();

            if (clusterUnits.Any())
                bestClusterPoint = clusterUnits.FirstOrDefault();
            else
                bestClusterPoint = Trinity.Player.Position;

            return bestClusterPoint;

        }
        /// <summary>
        /// Finds the optimal cluster position, works regardless if there is a cluster or not (will return single unit position if not). This is not a K-Means cluster, but rather a psuedo cluster based
        /// on the number of other monsters within a radius of any given unit
        /// </summary>
        /// <param name="radius">The maximum distance between monsters to be considered part of a cluster</param>
        /// <param name="maxRange">The maximum unit range to include, units further than this will not be checked as a cluster center but may be included in a cluster</param>
        /// <param name="useWeights">Whether or not to included un-weighted (ignored) targets in the cluster finding</param>
        /// <param name="includeUnitsInAoe">Checks the cluster point for AoE effects</param>
        /// <returns>The Vector3 position of the unit that is the ideal "center" of a cluster</returns>
        internal static Vector3 GetBestPowerGlobeClusterPoint(float radius = 15f, float maxRange = 65f, bool useWeights = true, bool includeUnitsInAoe = true)
        {
            if (radius < 5f)
                radius = 5f;
            if (maxRange > 30f)
                maxRange = 30f;

            Vector3 bestClusterPoint;
            var clusterUnits =
                (from u in ObjectCache
                 where u.Type == GObjectType.PowerGlobe &&
                 (includeUnitsInAoe || !UnitOrPathInAoE(u)) &&
                 u.RadiusDistance <= maxRange
                 orderby u.NearbyUnitsWithinDistance(radius)
                 orderby u.Distance descending
                 select u.Position).ToList();

            if (clusterUnits.Any())
                bestClusterPoint = clusterUnits.FirstOrDefault();
            else
                bestClusterPoint = Trinity.Player.Position;

            return bestClusterPoint;
        }
        /// <summary>
        /// Checks to see if there is a health globe around to grab
        /// </summary>
        /// <param name="radius"></param>
        /// <returns></returns>
        internal static bool HealthGlobeExists(float radius = 15f)
        {
            var clusterCheck =
                (from u in ObjectCache
                 where u.Type == GObjectType.HealthGlobe && !UnitOrPathInAoE(u) &&
                 u.RadiusDistance <= radius
                 select u).Any();

            return clusterCheck;
        }

        /// <summary>
        /// Checks to see if there is a health globe around to grab
        /// </summary>
        /// <param name="radius"></param>
        /// <returns></returns>
        internal static bool PowerGlobeExists(float radius = 15f)
        {
            var clusterCheck =
                (from u in ObjectCache
                 where u.Type == GObjectType.PowerGlobe && !UnitOrPathInAoE(u) &&
                 u.RadiusDistance <= radius
                 select u).Any();

            return clusterCheck;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="radius"></param>
        /// <param name="maxRange"></param>
        /// <param name="count"></param>
        /// <param name="useWeights"></param>
        /// <param name="includeUnitsInAoe"></param>
        /// <returns></returns>
        internal static TrinityCacheObject GetBestClusterUnit(float radius = 15f, float maxRange = 65f, int count = 1, bool useWeights = true, bool includeUnitsInAoe = true)
        {
            if (radius < 1f)
                radius = 1f;
            if (maxRange > 300f)
                maxRange = 300f;

            TrinityCacheObject bestClusterUnit;
            var clusterUnits =
                (from u in ObjectCache
                 where u.IsUnit &&
                 ((useWeights && u.Weight > 0) || !useWeights) &&
                 (includeUnitsInAoe || !UnitOrPathInAoE(u)) &&
                 u.RadiusDistance <= maxRange
                 orderby !u.IsBossOrEliteRareUnique
                 orderby u.NearbyUnitsWithinDistance(radius) descending
                 orderby u.Distance
                 orderby u.HitPointsPct descending
                 select u).ToList();

            if (clusterUnits.Any())
                bestClusterUnit = clusterUnits.FirstOrDefault();
            else if (Trinity.CurrentTarget != null)
                bestClusterUnit = Trinity.CurrentTarget;
            else
                bestClusterUnit = default(TrinityCacheObject);

            return bestClusterUnit;
        }

        /// <summary>
        /// Finds the optimal cluster position, works regardless if there is a cluster or not (will return single unit position if not). This is not a K-Means cluster, but rather a psuedo cluster based
        /// on the number of other monsters within a radius of any given unit
        /// </summary>
        /// <param name="radius">The maximum distance between monsters to be considered part of a cluster</param>
        /// <param name="maxRange">The maximum unit range to include, units further than this will not be checked as a cluster center but may be included in a cluster</param>
        /// <param name="useWeights">Whether or not to included un-weighted (ignored) targets in the cluster finding</param>
        /// <param name="includeUnitsInAoe">Checks the cluster point for AoE effects</param>
        /// <returns>The Vector3 position of the unit that is the ideal "center" of a cluster</returns>
        internal static Vector3 GetBestClusterPoint(float radius = 15f, float maxRange = 65f, bool useWeights = true, bool includeUnitsInAoe = true)
        {
            if (radius < 5f)
                radius = 5f;
            if (maxRange > 300f)
                maxRange = 300f;

            bool includeHealthGlobes = false;
            switch (Trinity.Player.ActorClass)
            {
                case ActorClass.Barbarian:
                    includeHealthGlobes = CombatBase.Hotbar.Contains(SNOPower.Barbarian_Whirlwind) &&
                                          Trinity.Settings.Combat.Misc.CollectHealthGlobe &&
                                          ObjectCache.Any(g => g.Type == GObjectType.HealthGlobe && g.Weight > 0);
                    break;
            }

            Vector3 bestClusterPoint;
            var clusterUnits =
                (from u in ObjectCache
                 where (u.IsUnit || (includeHealthGlobes && u.Type == GObjectType.HealthGlobe)) &&
                 ((useWeights && u.Weight > 0) || !useWeights) &&
                 (includeUnitsInAoe || !UnitOrPathInAoE(u)) &&
                 u.RadiusDistance <= maxRange
                 orderby u.Type != GObjectType.HealthGlobe // if it's a globe this will be false and sorted at the top
                 orderby !u.IsBossOrEliteRareUnique
                 orderby u.NearbyUnitsWithinDistance(radius) descending
                 orderby u.Distance
                 orderby u.HitPointsPct descending
                 select u.Position).ToList();

            if (clusterUnits.Any())
                bestClusterPoint = clusterUnits.FirstOrDefault();
            else if (Trinity.CurrentTarget != null)
                bestClusterPoint = Trinity.CurrentTarget.Position;
            else
                bestClusterPoint = Trinity.Player.Position;

            return bestClusterPoint;
        }
        /// <summary>
        /// Fast check to see if there are any attackable units within a certain distance
        /// </summary>
        /// <param name="range"></param>
        /// <returns></returns>
        internal static bool AnyMobsInRange(float range = 10f)
        {
            return AnyMobsInRange(range, 1);
        }
        /// <summary>
        /// Fast check to see if there are any attackable units within a certain distance
        /// </summary>
        /// <param name="range"></param>
        /// <returns></returns>
        internal static bool AnyMobsInRange(float range = 10f, bool useWeights = true)
        {
            return AnyMobsInRange(range, 1, useWeights);
        }
        /// <summary>
        /// Fast check to see if there are any attackable units within a certain distance
        /// </summary>
        /// <param name="range"></param>
        /// <returns></returns>
        internal static bool AnyMobsInRange(float range = 10f, int minCount = 1, bool useWeights = true)
        {
            if (range < 5f)
                range = 5f;
            if (minCount < 1)
                minCount = 1;
            return (from o in ObjectCache
                    where o.IsUnit &&
                     ((useWeights && o.Weight > 0) || !useWeights) &&
                    o.RadiusDistance <= range
                    select o).Count() >= minCount;
        }
        /// <summary>
        /// Checks if there are any mobs in range of the specified position
        /// </summary>
        internal static bool AnyMobsInRangeOfPosition(Vector3 position, float range = 15f, int unitsRequired = 1)
        {
            var inRangeCount = (from u in ObjectCache
                                where u.IsUnit &&
                                        u.Weight > 0 &&
                                        u.CommonData != null && u.CommonData.IsValid &&
                                        u.Position.Distance2D(position) <= range
                                select u).Count();

            return inRangeCount >= unitsRequired;
        }
        /// <summary>
        /// Checks if there are any mobs in range of the specified position
        /// </summary>
        internal static int NumMobsInRangeOfPosition(Vector3 position, float range = 15f)
        {
            return (from u in ObjectCache
                                where u.IsUnit &&
                                        u.Weight > 0 &&
                                        u.CommonData != null && u.CommonData.IsValid &&
                                        u.Position.Distance2D(position) <= range
                                select u).Count();
        }
        /// <summary>
        /// Checks if there are any bosses in range of the specified position
        /// </summary>
        internal static int NumBossInRangeOfPosition(Vector3 position, float range = 15f)
        {
            return (from u in ObjectCache
                    where u.IsUnit &&
                            u.Weight > 0 &&
                            u.IsBoss &&
                            u.CommonData != null && u.CommonData.IsValid &&
                            u.Position.Distance2D(position) <= range
                    select u).Count();
        }
        /// <summary>
        /// Returns list of units within the specified range
        /// </summary>
        internal static List<TrinityCacheObject> ListUnitsInRangeOfPosition(Vector3 position, float range = 15f)
        {
            return (from u in ObjectCache
                where u.IsUnit &&
                    u.Weight > 0 &&
                    u.CommonData != null && u.CommonData.IsValid &&
                    u.Position.Distance2D(position) <= range
                select u).ToList();
        }

        internal static bool AnyTrashInRange(float range = 10f, int minCount = 1, bool useWeights = true)
        {
            if (range < 5f)
                range = 5f;
            if (minCount < 1)
                minCount = 1;
            return (from o in ObjectCache
                    where o.IsUnit && o.IsTrashMob &&
                     ((useWeights && o.Weight > 0) || !useWeights) &&
                    o.RadiusDistance <= range
                    select o).Count() >= minCount;
        }
        /// <summary>
        /// Fast check to see if there are any attackable Elite units within a certain distance
        /// </summary>
        /// <param name="range"></param>
        /// <returns></returns>
        internal static bool AnyElitesInRange(float range = 10f)
        {
            if (CombatBase.IgnoringElites)
                return false;

            if (range < 5f)
                range = 5f;
            return (from o in ObjectCache
                    where o.IsUnit &&
                    o.IsBossOrEliteRareUnique &&
                    o.RadiusDistance <= range
                    select o).Any();
        }
        /// <summary>
        /// Fast check to see if there are any attackable Elite units within a certain distance
        /// </summary>
        /// <param name="range"></param>
        /// <returns></returns>
        internal static bool AnyElitesInRange(float range = 10f, int minCount = 1)
        {
            if (CombatBase.IgnoringElites)
                return false;

            if (range < 5f)
                range = 5f;
            if (minCount < 1)
                minCount = 1;
            return (from o in ObjectCache
                    where o.IsUnit &&
                    o.IsBossOrEliteRareUnique &&
                    o.RadiusDistance <= range
                    select o).Count() >= minCount;
        }
        /// <summary>
        /// Checks if there are any mobs in range of the specified position
        /// </summary>
        internal static bool AnyElitesInRangeOfPosition(Vector3 position, float range = 15f, int unitsRequired = 1)
        {
            var inRangeCount = (from u in ObjectCache
                                where u.IsUnit &&
                                        u.Weight > 0 &&
                                        u.IsBossOrEliteRareUnique &&
                                        u.CommonData != null && u.CommonData.IsValid &&
                                        u.Position.Distance2D(position) <= range
                                select u).Count();

            return inRangeCount >= unitsRequired;
        }
        /// <summary>
        /// Count of elites within range of position
        /// </summary>
        internal static int NumElitesInRangeOfPosition(Vector3 position, float range = 15f)
        {
            return (from u in ObjectCache
                                where u.IsUnit &&
                                        u.Weight > 0 &&
                                        u.IsBossOrEliteRareUnique &&
                                        u.CommonData != null && u.CommonData.IsValid &&
                                        u.Position.Distance2D(position) <= range
                                select u).Count();
        }
        /// <summary>
        /// Returns true if there is any elite units within the given range
        /// </summary>
        /// <param name="range"></param>
        /// <returns></returns>
        internal static bool IsEliteTargetInRange(float range = 10f)
        {
            if (range < 5f)
                range = 5f;
            return Trinity.CurrentTarget != null && Trinity.CurrentTarget.IsBossOrEliteRareUnique && Trinity.CurrentTarget.RadiusDistance <= range;
        }

        /// <summary>
        /// Finds an optimal position for using Monk Tempest Rush out of combat
        /// </summary>
        /// <returns></returns>
        internal static Vector3 FindTempestRushTarget()
        {
            Vector3 target = PlayerMover.LastMoveToTarget;
            Vector3 myPos = ZetaDia.Me.Position;

            if (Trinity.CurrentTarget != null && NavHelper.CanRayCast(myPos, target))
            {
                target = Trinity.CurrentTarget.Position;
            }

            float distance = target.Distance2D(myPos);

            if (distance < 30f)
            {
                double direction = MathUtil.FindDirectionRadian(myPos, target);
                target = MathEx.GetPointAt(myPos, 40f, (float)direction);
            }

            return target;
        }

        // Special Zig-Zag movement for whirlwind/tempest
        /// <summary>
        /// Finds an optimal position for Barbarian Whirlwind, Monk Tempest Rush, or Demon Hunter Strafe
        /// </summary>
        /// <param name="origin"></param>
        /// <param name="ringDistance"></param>
        /// <param name="randomizeDistance"></param>
        /// <returns></returns>
        internal static Vector3 GetZigZagTarget(Vector3 origin, float ringDistance, bool randomizeDistance = false)
        {
            var minDistance = 20f;
            Vector3 myPos = Player.Position;
            float distanceToTarget = origin.Distance2D(myPos);

            Vector3 zigZagPoint = origin;

            bool useTargetBasedZigZag = false;
            float maxDistance = 25f;
            int minTargets = 2;

            if (Trinity.Player.ActorClass == ActorClass.Monk)
            {
                maxDistance = 20f;
                minTargets = 3;
                useTargetBasedZigZag = Trinity.Settings.Combat.Monk.TargetBasedZigZag;
            }
            if (Trinity.Player.ActorClass == ActorClass.Barbarian)
            {
                useTargetBasedZigZag = Trinity.Settings.Combat.Barbarian.TargetBasedZigZag;
            }

            int eliteCount = ObjectCache.Count(u => u.IsUnit && u.IsBossOrEliteRareUnique);
            bool shouldZigZagElites = ((Trinity.CurrentTarget.IsBossOrEliteRareUnique && eliteCount > 1) || eliteCount == 0);

            if (useTargetBasedZigZag && shouldZigZagElites && !AnyTreasureGoblinsPresent && ObjectCache.Count(o => o.IsUnit) >= minTargets)
            {
                bool attackInAoe = Trinity.Settings.Combat.Misc.KillMonstersInAoE;
                var clusterPoint = TargetUtil.GetBestClusterPoint(ringDistance, ringDistance, false, attackInAoe);
                if (clusterPoint.Distance2D(Player.Position) >= minDistance)
                {
                    Logger.Log(LogCategory.Movement, "Returning ZigZag: BestClusterPoint {0} r-dist={1} t-dist={2}", clusterPoint, ringDistance, clusterPoint.Distance2D(Player.Position));
                    return clusterPoint;
                }


                var zigZagTargetList = new List<TrinityCacheObject>();
                if (attackInAoe)
                {
                    zigZagTargetList =
                        (from u in ObjectCache
                         where u.IsUnit && u.Distance < maxDistance
                         select u).ToList();
                }
                else
                {
                    zigZagTargetList =
                        (from u in ObjectCache
                         where u.IsUnit && u.Distance < maxDistance && !UnitOrPathInAoE(u)
                         select u).ToList();
                }

                if (zigZagTargetList.Count() >= minTargets)
                {
                    zigZagPoint = zigZagTargetList.OrderByDescending(u => u.Distance).FirstOrDefault().Position;
                    if (NavHelper.CanRayCast(zigZagPoint) && zigZagPoint.Distance2D(Player.Position) >= minDistance)
                    {
                        Logger.Log(LogCategory.Movement, "Returning ZigZag: TargetBased {0} r-dist={1} t-dist={2}", zigZagPoint, ringDistance, zigZagPoint.Distance2D(Player.Position));
                        return zigZagPoint;
                    }
                }
            }

            Random rndNum = new Random(int.Parse(Guid.NewGuid().ToString().Substring(0, 8), NumberStyles.HexNumber));
            float highestWeightFound = float.NegativeInfinity;
            Vector3 bestLocation = origin;

            // the unit circle always starts at 0 :)
            double min = 0;
            // the maximum size of a unit circle
            double max = 2 * Math.PI;
            // the number of times we will iterate around the circle to find points
            double piSlices = 16;

            // We will do several "passes" to make sure we can get a point that we can least zig-zag to
            // The total number of points tested will be piSlices * distancePasses.Count
            List<float> distancePasses = new List<float>();
            distancePasses.Add(ringDistance * 1 / 2); // Do one loop at 1/2 distance
            distancePasses.Add(ringDistance * 3 / 4); // Do one loop at 3/4 distance
            distancePasses.Add(ringDistance);         // Do one loop at exact distance

            foreach (float distance in distancePasses)
            {
                for (double direction = min; direction < max; direction += (Math.PI / piSlices))
                {
                    // Starting weight is 1
                    float pointWeight = 1f;

                    // Find a new XY
                    zigZagPoint = MathEx.GetPointAt(origin, distance, (float)direction);
                    // Get the Z
                    zigZagPoint.Z = Trinity.MainGridProvider.GetHeight(zigZagPoint.ToVector2());

                    // Make sure we're actually zig-zagging our target, except if we're kiting

                    float targetCircle = CurrentTarget.Radius;
                    if (targetCircle <= 5f)
                        targetCircle = 5f;
                    if (targetCircle > 10f)
                        targetCircle = 10f;

                    bool intersectsPath = MathUtil.IntersectsPath(CurrentTarget.Position, targetCircle, myPos, zigZagPoint);
                    if (CombatBase.PlayerKiteDistance <= 0 && !intersectsPath)
                        continue;

                    // if we're kiting, lets not actualy run through monsters
                    if (CombatBase.PlayerKiteDistance > 0 && CacheData.MonsterObstacles.Any(m => m.Position.Distance(zigZagPoint) <= CombatBase.PlayerKiteDistance))
                        continue;

                    // Ignore point if any AoE in this point position
                    if (CacheData.TimeBoundAvoidance.Any(m => m.Position.Distance(zigZagPoint) <= m.Radius && Player.CurrentHealthPct <= AvoidanceManager.GetAvoidanceHealthBySNO(m.ActorSNO, 1)))
                        continue;

                    // Make sure this point is in LoS/walkable (not around corners or into a wall)
                    bool canRayCast = !Navigator.Raycast(Player.Position, zigZagPoint);
                    if (!canRayCast)
                        continue;

                    float distanceToPoint = zigZagPoint.Distance2D(myPos);
                    float distanceFromTargetToPoint = zigZagPoint.Distance2D(origin);

                    // Lots of weight for points further away from us (e.g. behind our CurrentTarget)
                    pointWeight *= distanceToPoint;

                    // Add weight for any units in this point
                    int monsterCount = ObjectCache.Count(u => u.IsUnit && u.Position.Distance2D(zigZagPoint) <= Math.Max(u.Radius, 10f));
                    if (monsterCount > 0)
                        pointWeight *= monsterCount;

                    //Logger.Log(LogCategory.Movement, "ZigZag Point: {0} distance={1:0} distaceFromTarget={2:0} intersectsPath={3} weight={4:0} monsterCount={5}",
                    //    zigZagPoint, distanceToPoint, distanceFromTargetToPoint, intersectsPath, pointWeight, monsterCount);

                    // Use this one if it's more weight, or we haven't even found one yet, or if same weight as another with a random chance
                    if (pointWeight > highestWeightFound)
                    {
                        highestWeightFound = pointWeight;

                        if (Trinity.Settings.Combat.Misc.UseNavMeshTargeting)
                        {
                            bestLocation = new Vector3(zigZagPoint.X, zigZagPoint.Y, Trinity.MainGridProvider.GetHeight(zigZagPoint.ToVector2()));
                        }
                        else
                        {
                            bestLocation = new Vector3(zigZagPoint.X, zigZagPoint.Y, zigZagPoint.Z + 4);
                        }
                    }
                }
            }
            Logger.Log(LogCategory.Movement, "Returning ZigZag: RandomXY {0} r-dist={1} t-dist={2}", bestLocation, ringDistance, bestLocation.Distance2D(Player.Position));
            return bestLocation;
        }

        /// <summary>
        /// Checks to see if a given Unit is standing in AoE, or if the direct paht-line to the unit goes through AoE
        /// </summary>
        /// <param name="u"></param>
        /// <returns></returns>
        internal static bool UnitOrPathInAoE(TrinityCacheObject u)
        {
            return UnitInAoe(u) && PathToObjectIntersectsAoe(u);
        }

        /// <summary>
        /// Checks to see if a given Unit is standing in AoE
        /// </summary>
        /// <param name="u"></param>
        /// <returns></returns>
        internal static bool UnitInAoe(TrinityCacheObject u)
        {
            return CacheData.TimeBoundAvoidance.Any(aoe => aoe.Position.Distance2D(u.Position) <= aoe.Radius);
        }

        /// <summary>
        /// Checks to see if the path-line to a unit goes through AoE
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        internal static bool PathToObjectIntersectsAoe(TrinityCacheObject obj)
        {
            return CacheData.TimeBoundAvoidance.Any(aoe =>
                MathUtil.IntersectsPath(aoe.Position, aoe.Radius, obj.Position, Player.Position));
        }

        /// <summary>
        /// Checks if spell is tracked on any unit within range of specified position
        /// </summary>
        internal static bool IsUnitWithDebuffInRangeOfPosition(float range, Vector3 position, SNOPower power, int unitsRequiredWithDebuff = 1)
        {
            var unitsWithDebuff = (from u in ObjectCache

                                   where u.IsUnit &&
                                          u.Weight > 0 && 
                                          u.CommonData != null && u.CommonData.IsValid &&
                                          u.Position.Distance2D(position) <= range &&
                                          SpellTracker.IsUnitTracked(u.ACDGuid, power)

                                   select u).ToList();

            // Make sure units exist
            unitsWithDebuff.RemoveAll(u =>
            {
                var acd = ZetaDia.Actors.GetACDByGuid(u.ACDGuid);
                return acd == null || !acd.IsValid;
            });

            return unitsWithDebuff.Count >= unitsRequiredWithDebuff;
        }

        /// <summary>
        /// Creates a circular band or donut shape around player between min and max range and calculates the number of monsters inside.
        /// </summary>
        /// <param name="bandMinRange">Starting range for the band - monsters outside this value</param>
        /// <param name="bandMaxRange">Ending range for the band - monsters inside this value</param>
        /// <param name="percentage">Percentrage of monsters within bandMaxRange that must be within the band</param>
        /// <returns>True if at least specified percentage of monsters are within the band</returns>
        internal static bool IsPercentUnitsWithinBand(float bandMinRange = 10f, float bandMaxRange = 10f, double percentage = 50)
        {
            if (bandMinRange > bandMaxRange) bandMinRange = bandMaxRange;
            if (bandMaxRange < bandMinRange) bandMaxRange = bandMinRange;
            if (percentage < 0 || percentage > 100) percentage = 75;
            if (percentage < 1) percentage = percentage * 100;

            var totalWithinMaxRange = (from o in ObjectCache
                                       where o.IsUnit && o.Weight > 0 &&
                                       o.RadiusDistance <= bandMaxRange
                                       select o).Count();

            var totalWithinMinRange = (from o in ObjectCache
                                       where o.IsUnit && o.Weight > 0 &&
                                       o.RadiusDistance <= bandMinRange
                                       select o).Count();

            var totalWithinBand = totalWithinMaxRange - totalWithinMinRange;

            double percentWithinBand = ((double)totalWithinBand / (double)totalWithinMaxRange) * 100;

            //Logger.LogDebug("{0} of {6} mobs between {1} and {2} yards ({3:f2}%), needed={4}% result={5}", totalWithinBand, bandMinRange, bandMaxRange, percentWithinBand, percentage, percentWithinBand >= percentage, totalWithinMaxRange);

            return percentWithinBand >= percentage;
        }


        internal static TrinityCacheObject GetBestHarvestTarget(float skillRange, float maxRange = 30f)
        {
            TrinityCacheObject harvestTarget =
            (from u in ObjectCache
             where u.IsUnit &&
             u.RadiusDistance <= maxRange &&
             u.IsBossOrEliteRareUnique &&
             u.CommonData.GetAttribute<int>(ActorAttributeType.BuffVisualEffect) != 0
             orderby u.NearbyUnitsWithinDistance(skillRange) descending
             select u).FirstOrDefault();
            if (harvestTarget != null)
                return harvestTarget;

            return (from u in ObjectCache
                    where u.IsUnit &&
                    u.RadiusDistance <= maxRange &&
                    u.CommonData.GetAttribute<int>(ActorAttributeType.BuffVisualEffect) != 0
                    orderby u.NearbyUnitsWithinDistance(skillRange) descending
                    select u).FirstOrDefault();
        }

        internal static bool PercentOfMobsDebuffed(float maxRange = 30f, float minPercent = 0.5f)
        {  
            int debuffed = (from u in ObjectCache
                            where u.IsUnit &&
                            u.RadiusDistance <= maxRange &&
                            u.CommonData.GetAttribute<int>(ActorAttributeType.BuffVisualEffect) != 0
                            select u).Count();

            int all = (from u in ObjectCache
                       where u.IsUnit &&
                       u.RadiusDistance <= maxRange
                       select u).Count();

            if (debuffed / all >= minPercent)
                return true;

            return false;
        }

        internal static int MobsWithDebuff(SNOPower power, float maxRange = 30f)
        {
            return (from u in ObjectCache
                    where u.IsUnit &&
                    u.RadiusDistance <= maxRange &&
                    u.HasDebuff(power)
                    select u).Count();
        }

        internal static int MobsWithDebuff(IEnumerable<SNOPower> powers, float maxRange = 30f)
        {
            return (from u in ObjectCache
                    where u.IsUnit &&
                    u.RadiusDistance <= maxRange &&
                    powers.Any(u.HasDebuff)
                    select u).Count();
        }

        internal static int MobsWithDebuff(IEnumerable<SNOPower> powers, IEnumerable<TrinityCacheObject> units)
        {
            return (from u in units
                    where u.IsUnit &&
                    powers.Any(u.HasDebuff)
                    select u).Count();
        }

        internal static int DebuffCount(IEnumerable<SNOPower> powers, float maxRange = 30f)
        {
            return (from u in ObjectCache
                    where u.IsUnit &&
                    u.RadiusDistance <= maxRange &&
                    powers.Any(u.HasDebuff)
                    select powers.Count(u.HasDebuff)
                    ).Sum();
        }

        internal static int DebuffCount(IEnumerable<SNOPower> powers, IEnumerable<TrinityCacheObject> units)
        {
            return (from u in units
                    where u.IsUnit &&
                    powers.Any(u.HasDebuff)
                    select powers.Count(u.HasDebuff)
                    ).Sum();
        }


        internal static TrinityCacheObject LowestHealthTarget(float range, Vector3 position = new Vector3(), SNOPower debuff = SNOPower.None)
        {
            if (position == new Vector3())
                position = Player.Position;

            TrinityCacheObject lowestHealthTarget;
            var unitsByHealth = (from u in ObjectCache

                                 where u.IsUnit &&
                                        u.Weight > 0 &&
                                        u.CommonData != null && u.CommonData.IsValid &&
                                        u.Position.Distance2D(position) <= range &&
                                        (debuff == SNOPower.None || !SpellTracker.IsUnitTracked(u.ACDGuid, debuff))
                                 orderby u.HitPoints ascending
                                 select u).ToList();

            if (unitsByHealth.Any())
                lowestHealthTarget = unitsByHealth.FirstOrDefault();
            else if (Trinity.CurrentTarget != null)
                lowestHealthTarget = Trinity.CurrentTarget;
            else
                lowestHealthTarget = default(TrinityCacheObject);

            return lowestHealthTarget;
        }

        internal static TrinityCacheObject BestTargetWithoutDebuffs(float range, IEnumerable<SNOPower> debuffs, Vector3 position = new Vector3())
        {
            if (position == new Vector3())
                position = Player.Position;

            TrinityCacheObject target;
            var unitsByWeight = (from u in ObjectCache

                                 where u.IsUnit &&
                                        u.Weight > 0 &&
                                        u.CommonData != null && u.CommonData.IsValid &&
                                        u.Position.Distance2D(position) <= range &&
                                        !debuffs.All(u.HasDebuff)
                                 orderby u.Weight descending 
                                 select u).ToList();

            if (unitsByWeight.Any())
                target = unitsByWeight.FirstOrDefault();

            else if (Trinity.CurrentTarget != null)
                target = Trinity.CurrentTarget;
            else
                target = default(TrinityCacheObject);

            return target;
        }

        internal static TrinityCacheObject GetDashStrikeFarthestTarget(float maxRange, float procDistance = 33f, int arcDegrees = 0)
        {
            var result =
                (from u in ObjectCache
                 where u.IsUnit && u.Distance >= procDistance &&
                 u.RadiusDistance <= maxRange
                 orderby u.RadiusDistance descending
                 select u).FirstOrDefault();
            return result;
        }

        internal static Vector3 GetDashStrikeBestClusterPoint(float radius = 15f, float maxRange = 65f, float procDistance = 33f, bool useWeights = true, bool includeUnitsInAoe = true)
        {
            if (radius < 5f)
                radius = 5f;
            if (maxRange > 300f)
                maxRange = 300f;

            bool includeHealthGlobes = false;
            switch (Trinity.Player.ActorClass)
            {
                case ActorClass.Barbarian:
                    includeHealthGlobes = CombatBase.Hotbar.Contains(SNOPower.Barbarian_Whirlwind) &&
                                          Trinity.Settings.Combat.Misc.CollectHealthGlobe &&
                                          ObjectCache.Any(g => g.Type == GObjectType.HealthGlobe && g.Weight > 0);
                    break;
            }

            Vector3 bestClusterPoint;
            var clusterUnits =
                (from u in ObjectCache
                 where (u.IsUnit || (includeHealthGlobes && u.Type == GObjectType.HealthGlobe)) &&
                 ((useWeights && u.Weight > 0) || !useWeights) &&
                 (includeUnitsInAoe || !UnitOrPathInAoE(u)) &&
                 u.RadiusDistance <= maxRange && u.Distance >= procDistance
                 orderby u.Type != GObjectType.HealthGlobe // if it's a globe this will be false and sorted at the top
                 orderby !u.IsBossOrEliteRareUnique
                 orderby u.NearbyUnitsWithinDistance(radius) descending
                 orderby u.Distance
                 orderby u.HitPointsPct descending
                 select u.Position).ToList();

            if (clusterUnits.Any())
                bestClusterPoint = clusterUnits.FirstOrDefault();
            else
                bestClusterPoint = Trinity.Player.Position;

            return bestClusterPoint;
        }



    }
}
