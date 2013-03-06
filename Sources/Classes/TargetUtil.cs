﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GilesTrinity.DbProvider;
using Zeta;
using Zeta.Common;

namespace GilesTrinity
{
    public class TargetUtil
    {
        /// <summary>
        /// Checks to make sure there's at least one valid cluster with the minimum monster count
        /// </summary>
        /// <param name="radius"></param>
        /// <param name="maxRange"></param>
        /// <param name="minCount"></param>
        /// <param name="forceElites"></param>
        /// <returns></returns>
        public static bool ClusterExists(float radius = 15f, int minCount = 2)
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
        public static bool ClusterExists(float radius = 15f, float maxRange = 90f, int minCount = 2, bool forceElites = true)
        {
            if (radius < 5f)
                radius = 5f;
            if (maxRange > 300f)
                maxRange = 300f;
            if (minCount < 2)
                minCount = 2;

            if (forceElites && GilesTrinity.GilesObjectCache.Any(u => u.Type == GObjectType.Unit && u.IsBossOrEliteRareUnique && u.RadiusDistance < maxRange))
                return true;

            var clusterCheck =
                (from u in GilesTrinity.GilesObjectCache
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
        public static Vector3 GetBestClusterPoint(float radius = 15f, float maxRange = 65f, bool useWeights = true)
        {
            if (radius < 5f)
                radius = 5f;
            if (maxRange > 300f)
                maxRange = 300f;

            using (new Technicals.PerformanceLogger("TargetUtil.GetBestClusterPoint"))
            {
                Vector3 bestClusterPoint = Vector3.Zero;
                var clusterUnits =
                    (from u in GilesTrinity.GilesObjectCache
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
                else if (GilesTrinity.CurrentTarget != null)
                    bestClusterPoint = GilesTrinity.CurrentTarget.Position;
                else
                    bestClusterPoint = GilesTrinity.PlayerStatus.CurrentPosition;

                return bestClusterPoint;
            }
        }

        /// <summary>
        /// Fast check to see if there are any attackable units within a certain distance
        /// </summary>
        /// <param name="range"></param>
        /// <returns></returns>
        public static bool AnyMobsInRange(float range = 10f)
        {
            if (range < 5f)
                range = 5f;
            return (from o in GilesTrinity.GilesObjectCache
                    where o.Type == GObjectType.Unit &&
                    o.Weight > 0 &&
                    o.RadiusDistance <= range
                    select o).Any();
        }

        /// <summary>
        /// Fast check to see if there are any attackable units within a certain distance
        /// </summary>
        /// <param name="range"></param>
        /// <returns></returns>
        public static bool AnyMobsInRange(float range = 10f, int minCount = 1)
        {
            if (range < 5f)
                range = 5f;
            if (minCount < 1)
                minCount = 1;
            return (from o in GilesTrinity.GilesObjectCache
                    where o.Type == GObjectType.Unit &&
                    o.Weight > 0 &&
                    o.RadiusDistance <= range
                    select o).Count() >= minCount;
        }

        /// <summary>
        /// Fast check to see if there are any attackable Elite units within a certain distance
        /// </summary>
        /// <param name="range"></param>
        /// <returns></returns>
        public static bool AnyElitesInRange(float range = 10f)
        {
            if (range < 5f)
                range = 5f;
            return (from o in GilesTrinity.GilesObjectCache
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
        public static bool AnyElitesInRange(float range = 10f, int minCount = 1)
        {
            if (range < 5f)
                range = 5f;
            if (minCount < 1)
                minCount = 1;
            return (from o in GilesTrinity.GilesObjectCache
                    where o.Type == GObjectType.Unit &&
                    o.IsBossOrEliteRareUnique &&
                    o.RadiusDistance <= range
                    select o).Count() > minCount;
        }

        public static bool IsEliteTargetInRange(float range = 10f)
        {
            if (range < 5f)
                range = 5f;
            return GilesTrinity.CurrentTarget != null && GilesTrinity.CurrentTarget.IsBossOrEliteRareUnique && GilesTrinity.CurrentTarget.RadiusDistance <= range;
        }

        public static Vector3 FindTempestRushTarget()
        {
            Vector3 target = PlayerMover.LastMoveToTarget;
            Vector3 myPos = ZetaDia.Me.Position;

            if (GilesTrinity.CurrentTarget != null && GilesTrinity.CanRayCast(myPos, target))
            {
                target = GilesTrinity.CurrentTarget.Position;
            }

            float distance = target.Distance2D(myPos);

            if (distance < 30f)
            {
                double direction = GilesTrinity.FindDirectionRadian(myPos, target);
                target = MathEx.GetPointAt(myPos, 40f, (float)direction);
            }

            return target;
        }


    }
}
