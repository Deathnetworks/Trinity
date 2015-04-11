using System;
using System.Collections.Generic;
using System.Linq;
using Trinity.Technicals;
using Zeta.Common;
using Logger = Trinity.Technicals.Logger;

namespace Trinity
{
    internal class GridMap
    {
        /// <summary>
        /// Return _map to a dictionary with Key > Position & Value > Weight
        /// </summary>
        /// <param name="_map"></param>
        /// <returns></returns>
        public static Dictionary<Vector3, float> ToDictionary()
        {
            if (!MainGrid.Map.Any())
                return new Dictionary<Vector3, float>();

            return MainGrid.Map.Select(p => new {p.Position, Weight = p.DynamicWeight}).ToDictionary(p => p.Position, p => (float) p.Weight);
        }

        /// <summary>
        /// return true if any grid point has a cluster weight > 0
        /// </summary>
        public static bool ClusterNodeExist
        {
            get
            {
                return MainGrid.Map.Any(g => g.ClusterWeight > 0);
            }
        }

        /// <summary>
        /// Search closest grid point to player and return his weight
        /// </summary>
        public static float GetWeightAtPlayer
        {
            get
            {
                return GetWeightAt(Trinity.Player.Position);
            }
        }

        /// <summary>
        /// Search closest grid point to point and return his weight
        /// </summary>
        public static float GetWeightAt(Vector3 loc)
        {
            float weight = 0f;

            if (MainGrid.Map.Any())
            {
                GridNode node = GetNodeAt(loc);
                if (node != null)
                    weight = (float)node.Weight;
            }

            return weight;
        }

        /// <summary>
        /// Search closest grid point to point
        /// </summary>
        public static GridNode GetNodeAt(Vector3 loc)
        {
            if (MainGrid.Map.Any())
            {
                GridNode result;
                if (GridResults.RecordedValues_GetNodeAt.TryGetValue(loc, out result))
                    return result;

                result = MainGrid.Map.OrderBy(g => g.Position.Distance2D(loc)).First();
                if (result != null && result.Position != Vector3.Zero)
                {
                    GridResults.RecordedValues_GetNodeAt.Add(loc, result);
                    return result;
                }
            }

            return null;
        }

        /// <summary>
        /// Search in grid map the best safe grid point by weight
        /// </summary>
        /// <param name="minRange">minimum search range of point</param>
        /// <param name="maxRange">maximum search range of point</param>
        /// <param name="loc">origin search location</param>
        /// <param name="prioritizeDist"></param>
        /// <returns></returns>
        public static GridNode GetBestMoveNode(float minRange = 6f, float maxRange = 100f, Vector3 loc = new Vector3(), bool prioritizeDist = false)
        {
            using (new MemorySpy("GridMap.GetBestMoveNode()"))
            {
                if (loc == Vector3.Zero)
                    loc = Trinity.Player.Position;

                bool atPlayer = loc == Trinity.Player.Position;

                GridNode result;
                var key = new Tuple<float, float, Vector3>(minRange, maxRange, loc);

                if (GridResults.RecordedValues_GetBestSafeNode.TryGetValue(key, out result))
                {
                    /* Keep last safe node */
                    if (result.Weight >= (GetWeightAt(result.Position)*0.9))
                        return result;

                    GridResults.RecordedValues_GetBestSafeNode.Remove(key);
                }

                var results =
                    (from p in MainGrid.Map
                        where
                            (atPlayer && p.Distance >= minRange ||
                             !atPlayer && p.Position.Distance2D(loc) >= minRange) &&
                            (atPlayer && p.Distance < maxRange ||
                             !atPlayer && p.Position.Distance2D(loc) < maxRange)
                        select p).ToList();

                if (prioritizeDist)
                    results =
                        (from p in results
                            orderby
                                p.Weight descending,
                                p.Position.Distance2D(loc)
                            select p).ToList();
                else
                    results =
                        (from p in results
                            orderby p.Weight descending
                            select p).ToList();

                if (results != null && results.Count() > 0)
                {
                    result = results.First();
                    GridResults.RecordedValues_GetBestSafeNode.Add(key, result);

                    if (result.Position != MainGrid.LastResult.Position)
                    {
                        MainGrid.LastResult = result;
                        Logger.Log(TrinityLogLevel.Info, LogCategory.Movement, "Best safe node : Loc={0} Dist={1:0} Weight={2:0} Infos={3}",
                            result.Position, result.Position.Distance2D(loc),
                            result.Weight, result.WeightInfos
                            );
                    }
                }

                return result;
            }
        }

        /// <summary>
        /// Search the grid point with higher cluster weight
        /// </summary>
        public static GridNode GetBestClusterNode(Vector3 loc = new Vector3(), float radius = 15f, float maxRange = 65f, float minRange = 0f, int size = 1, bool useWeights = true, bool useDefault = true)
        {
            using (new MemorySpy("GridMap.GetBestClusterNode()"))
            {
                if (loc == new Vector3())
                    loc = Trinity.Player.Position;

                bool atPlayer = loc == Trinity.Player.Position;

                GridNode result;
                var key = new Tuple<float, float, Vector3>(minRange, maxRange, loc);

                if (GridResults.RecordedValues_GetBestClusterNode.TryGetValue(key, out result))
                {
                    /* Keep last cluster */
                    var point = GetNodeAt(result.Position);
                    if (point != null && result.ClusterWeight >= (point.ClusterWeight * 0.9))
                        return result;

                    GridResults.RecordedValues_GetBestClusterNode.Remove(key);
                }

                if (ClusterNodeExist)
                {
                    var results = (
                        from g in MainGrid.Map
                        where
                            g.ClusterWeight > 0 &&
                            (atPlayer && g.Distance >= minRange ||
                             !atPlayer && g.Position.Distance2D(loc) >= minRange) &&
                            (atPlayer && g.Distance < maxRange ||
                             !atPlayer && g.Position.Distance2D(loc) < maxRange)
                        orderby
                            g.ClusterWeight descending
                        select g).ToList();

                    if (results != null && results.Count() > 0)
                    {
                        result = results.First();
                        GridResults.RecordedValues_GetBestClusterNode.Add(key, result);
                        return result;
                    }
                }

                if (useDefault)
                    return new GridNode(TargetUtil.GetBestClusterPoint(radius, maxRange, size, useWeights, loc));

                return null;
            }
        }

        /// <summary>
        /// Search point and set weight
        /// </summary>
        public static void RefreshGridMainLoop()
        {
            MainGrid.ResetTickValues();
            MainGrid.Refresh();
        }
    }
}