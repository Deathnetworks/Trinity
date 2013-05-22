using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Trinity.DbProvider;
using Trinity.Technicals;
using Zeta;
using Zeta.Common;
using Zeta.Internals.Actors;
using Zeta.Navigation;
using Zeta.Pathfinding;

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
        private static PlayerInfoCache PlayerStatus
        {
            get
            {
                return Trinity.PlayerStatus;
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
        private static HashSet<SNOPower> Hotbar
        {
            get
            {
                return Trinity.Hotbar;
            }
        }

        #endregion

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
            return ClusterExists(radius, 300f, minCount);
        }
        /// <summary>
        /// Checks to make sure there's at least one valid cluster with the minimum monster count
        /// </summary>
        /// <param name="radius"></param>
        /// <param name="maxRange"></param>
        /// <param name="minCount"></param>
        /// <param name="forceElites"></param>
        /// <returns></returns>
        internal static bool ClusterExists(float radius = 15f, float maxRange = 90f, int minCount = 2, bool forceElites = true)
        {
            if (radius < 5f)
                radius = 5f;
            if (maxRange > 300f)
                maxRange = 300f;
            if (minCount < 2)
                minCount = 2;

            if (forceElites && ObjectCache.Any(u => u.Type == GObjectType.Unit && u.IsBossOrEliteRareUnique && u.RadiusDistance < maxRange))
                return true;

            var clusterCheck =
                (from u in ObjectCache
                 where u.Type == GObjectType.Unit &&
                 u.RadiusDistance <= maxRange &&
                 u.NearbyUnitsWithinDistance(radius) >= minCount
                 select u).Any();

            return clusterCheck;
        }
        /// <summary>
        /// Finds the optimal cluster position, works regardless if there is a cluster or not (will return single unit position if not). This is not a K-Means cluster, but rather a psuedo cluster based
        /// on the number of other monsters within a radius of any given unit
        /// </summary>
        /// <param name="radius">The maximum distance between monsters to be considered part of a cluster</param>
        /// <param name="maxRange">The maximum unit range to include, units further than this will not be checked as a cluster center but may be included in a cluster</param>
        /// <param name="useWeights">Whether or not to included un-weighted (ignored) targets in the cluster finding</param>
        /// <returns>The Vector3 position of the unit that is the ideal "center" of a cluster</returns>
        internal static Vector3 GetBestClusterPoint(float radius = 15f, float maxRange = 65f, bool useWeights = true)
        {
            if (radius < 5f)
                radius = 5f;
            if (maxRange > 300f)
                maxRange = 300f;

            using (new Technicals.PerformanceLogger("TargetUtil.GetBestClusterPoint"))
            {
                Vector3 bestClusterPoint = Vector3.Zero;
                var clusterUnits =
                    (from u in ObjectCache
                     where u.Type == GObjectType.Unit &&
                     ((useWeights && u.Weight > 0) || !useWeights) &&
                     u.RadiusDistance <= maxRange
                     orderby u.IsBossOrEliteRareUnique
                     orderby u.NearbyUnitsWithinDistance(radius) descending
                     orderby u.CentreDistance
                     orderby u.HitPointsPct descending
                     select u.Position).ToList();

                if (clusterUnits.Any())
                    bestClusterPoint = clusterUnits.FirstOrDefault();
                else if (Trinity.CurrentTarget != null)
                    bestClusterPoint = Trinity.CurrentTarget.Position;
                else
                    bestClusterPoint = Trinity.PlayerStatus.CurrentPosition;

                return bestClusterPoint;
            }
        }
        /// <summary>
        /// Fast check to see if there are any attackable units within a certain distance
        /// </summary>
        /// <param name="range"></param>
        /// <returns></returns>
        internal static bool AnyMobsInRange(float range = 10f)
        {
            return AnyMobsInRange(range, 1, true);
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
                    where o.Type == GObjectType.Unit &&
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
            if (Trinity.Settings.Combat.Misc.IgnoreElites)
                return false;

            if (range < 5f)
                range = 5f;
            return (from o in ObjectCache
                    where o.Type == GObjectType.Unit &&
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
            if (Trinity.Settings.Combat.Misc.IgnoreElites)
                return false;

            if (range < 5f)
                range = 5f;
            if (minCount < 1)
                minCount = 1;
            return (from o in ObjectCache
                    where o.Type == GObjectType.Unit &&
                    o.IsBossOrEliteRareUnique &&
                    o.RadiusDistance <= range
                    select o).Count() > minCount;
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
            var minDistance = 9f;
            Vector3 myPos = PlayerStatus.CurrentPosition;
            float distanceToTarget = origin.Distance2D(myPos);

            Vector3 zigZagPoint = origin;

            using (new PerformanceLogger("FindZigZagTargetLocation"))
            {
                using (new PerformanceLogger("FindZigZagTargetLocation.CheckObjectCache"))
                {
                    bool useTargetBasedZigZag = false;
                    float maxDistance = 25f;
                    int minTargets = 2;

                    if (Trinity.PlayerStatus.ActorClass == ActorClass.Monk)
                    {
                        maxDistance = 20f;
                        minTargets = 3;
                        useTargetBasedZigZag = (Trinity.Settings.Combat.Monk.TargetBasedZigZag);
                    }
                    if (Trinity.PlayerStatus.ActorClass == ActorClass.Barbarian)
                    {
                        useTargetBasedZigZag = (Trinity.Settings.Combat.Barbarian.TargetBasedZigZag);
                    }

                    int eliteCount = ObjectCache.Count(u => u.Type == GObjectType.Unit && u.IsBossOrEliteRareUnique);
                    bool shouldZigZagElites = ((Trinity.CurrentTarget.IsBossOrEliteRareUnique && eliteCount > 1) || eliteCount == 0);

                    if (useTargetBasedZigZag && shouldZigZagElites && !AnyTreasureGoblinsPresent && ObjectCache.Where(o => o.Type == GObjectType.Unit).Count() >= minTargets)
                    {
                        var clusterPoint = TargetUtil.GetBestClusterPoint(ringDistance, ringDistance, false);
                        if (clusterPoint.Distance2D(PlayerStatus.CurrentPosition) >= minDistance)
                        {
                            Logger.Log(LogCategory.Movement, "Returning ZigZag: BestClusterPoint {0} r-dist={1} t-dist={2}", clusterPoint, ringDistance, clusterPoint.Distance2D(PlayerStatus.CurrentPosition));
                            return clusterPoint;
                        }
                        IEnumerable<TrinityCacheObject> zigZagTargets =
                            from u in ObjectCache
                            where u.Type == GObjectType.Unit && u.CentreDistance < maxDistance &&
                            !Trinity.hashAvoidanceObstacleCache.Any(a => Vector3.Distance(u.Position, a.Location) < AvoidanceManager.GetAvoidanceRadiusBySNO(a.ActorSNO, a.Radius) && PlayerStatus.CurrentHealthPct <= AvoidanceManager.GetAvoidanceHealthBySNO(a.ActorSNO, 1))
                            select u;
                        if (zigZagTargets.Count() >= minTargets)
                        {
                            zigZagPoint = zigZagTargets.OrderByDescending(u => u.CentreDistance).FirstOrDefault().Position;
                            if (NavHelper.CanRayCast(zigZagPoint) && zigZagPoint.Distance2D(PlayerStatus.CurrentPosition) >= minDistance)
                            {
                                Logger.Log(LogCategory.Movement, "Returning ZigZag: TargetBased {0} r-dist={1} t-dist={2}", zigZagPoint, ringDistance, zigZagPoint.Distance2D(PlayerStatus.CurrentPosition));
                                return zigZagPoint;
                            }
                        }
                    }
                }

                Random rndNum = new Random(int.Parse(Guid.NewGuid().ToString().Substring(0, 8), NumberStyles.HexNumber));

                using (new PerformanceLogger("FindZigZagTargetLocation.RandomZigZagPoint"))
                {
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
                            if (Trinity.PlayerKiteDistance <= 0 && !intersectsPath)
                                continue;

                            // if we're kiting, lets not actualy run through monsters
                            if (Trinity.PlayerKiteDistance > 0 && Trinity.hashMonsterObstacleCache.Any(m => m.Location.Distance(zigZagPoint) <= Trinity.PlayerKiteDistance))
                                continue;

                            // Ignore point if any AoE in this point position
                            if (Trinity.hashAvoidanceObstacleCache.Any(m => m.Location.Distance(zigZagPoint) <= m.Radius && PlayerStatus.CurrentHealthPct <= AvoidanceManager.GetAvoidanceHealthBySNO(m.ActorSNO, 1)))
                                continue;

                            // Make sure this point is in LoS/walkable (not around corners or into a wall)
                            bool canRayCast = !Navigator.Raycast(PlayerStatus.CurrentPosition, zigZagPoint);
                            if (!canRayCast)
                                continue;

                            float distanceToPoint = zigZagPoint.Distance2D(myPos);
                            float distanceFromTargetToPoint = zigZagPoint.Distance2D(origin);

                            // Lots of weight for points further away from us (e.g. behind our CurrentTarget)
                            pointWeight *= distanceToPoint;

                            // Add weight for any units in this point
                            int monsterCount = ObjectCache.Count(u => u.Type == GObjectType.Unit && u.Position.Distance2D(zigZagPoint) <= Math.Max(u.Radius, 10f));
                            if (monsterCount > 0)
                                pointWeight *= monsterCount;

                            Logger.Log(LogCategory.Movement, "ZigZag Point: {0} distance={1:0} distaceFromTarget={2:0} intersectsPath={3} weight={4:0} monsterCount={5}",
                                zigZagPoint, distanceToPoint, distanceFromTargetToPoint, intersectsPath, pointWeight, monsterCount);

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
                    Logger.Log(LogCategory.Movement, "Returning ZigZag: RandomXY {0} r-dist={1} t-dist={2}", bestLocation, ringDistance, bestLocation.Distance2D(PlayerStatus.CurrentPosition));
                    return bestLocation;
                }
            }
        }
    }
}
