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
            if (!MainGrid.MapAsList.Any())
                return new Dictionary<Vector3, float>();

            return MainGrid.MapAsList.Select(p => new {p.Position, Weight = p.DynamicWeight}).ToDictionary(p => p.Position, p => (float) p.Weight);
        }

        /// <summary>
        /// return true if any grid point has a cluster weight > 0
        /// </summary>
        public static bool ClusterNodeExist
        {
            get
            {
                bool exist = false;
                if (GridResults.HasTickValue_ClusterExist(out exist))
                {
                    return exist;
                }

                GridResults.TickValue_ClusterExist = MainGrid.MapAsList.Any(g => g.ClusterWeight > 0);
                GridResults.IsTickRecorded_ClusterExist = true;

                return GridResults.TickValue_ClusterExist;
            }
        }

        /// <summary>
        /// Search the grid point with higher cluster weight
        /// </summary>
        public static GridNode GetBestClusterNode(Vector3 loc = new Vector3(), float radius = 15f, float range = 65f, float minRange = 0f, int size = 1, bool useWeights = true, bool useDefault = true)
        {
            using (new MemorySpy("GridMap.GetBestClusterNode()"))
            {
                if (loc == new Vector3())
                    loc = Trinity.Player.Position;
                bool atPlayer = loc == Trinity.Player.Position;

                GridNode cluster;
                if (GridResults.HasRecordedValue_GetBestClusterNode(out cluster, range, loc))
                {
                    /* Keep last safe point */
                    var point = GetPointAt(cluster.Position);
                    if (point != null && cluster.ClusterWeight >= (point.ClusterWeight*0.9))
                        return cluster;

                    GridResults.RecordedValues_GetBestClusterNode.RemoveWhere(p => p != null && p.GridLocation != null && p.GridLocation.Equals(cluster));
                }

                if (ClusterNodeExist)
                {
                    cluster = (
                        from g in MainGrid.MapAsList
                        where
                            g.ClusterWeight > 0 &&
                            (atPlayer && g.Distance >= minRange ||
                             !atPlayer && g.Position.Distance2D(loc) >= minRange) &&
                            (atPlayer && g.Distance < range ||
                             !atPlayer && g.Position.Distance2D(loc) < range)
                        orderby
                            g.ClusterWeight descending
                        select g).ToList().FirstOrDefault();

                    if (cluster != null && cluster.ClusterWeight > 0f)
                    {
                        GridResults.RecordedValues_GetBestClusterNode.Add(new GetBestClusterNodeResult(cluster, range, loc));
                        return cluster;
                    }
                }

                if (useDefault)
                    return new GridNode(TargetUtil.GetBestClusterPoint(radius, range, size, useWeights, loc));

                return null;
            }
        }

        /// <summary>
        /// Search closest grid point to player and return his weight
        /// </summary>
        public static float GetWeightAtPlayer
        {
            get
            {
                float weightAtPlayer = 0f;
                if (GridResults.HasTickValue_GetWeightAtPlayer(out weightAtPlayer))
                {
                    return weightAtPlayer;
                }

                GridResults.TickValue_GetWeightAtPlayer = GetWeightAt(Trinity.Player.Position);

                return GridResults.TickValue_GetWeightAtPlayer;
            }
        }

        /// <summary>
        /// Search closest grid point to point and return his weight
        /// </summary>
        public static float GetWeightAt(Vector3 loc)
        {
            if (!MainGrid.MapAsList.Any())
                return 0f;

            float weightAtPoint = 0f;
            if (GridResults.HasTickValue_GetWeightAtPoint(out weightAtPoint, loc))
            {
                return weightAtPoint;
            }

            GridNode gPoint = GetPointAt(loc);
            if (gPoint != null)
            {
                weightAtPoint = (float) gPoint.DynamicWeight;
                GridResults.TickValues_GetWeightAtPoint.Add(new GetWeightResult(weightAtPoint, loc));
            }

            return weightAtPoint;
        }

        /// <summary>
        /// Search closest grid point to point
        /// </summary>
        public static GridNode GetPointAt(Vector3 loc)
        {
            if (!MainGrid.MapAsList.Any())
                return null;

            return MainGrid.MapAsList.OrderBy(g => g.Position.Distance2D(loc)).FirstOrDefault();
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
                if (GridResults.HasRecordedValue_GetBestNode(out result, minRange, maxRange, loc))
                {
                    /* Keep last safe point */
                    if (result.Weight >= (GetWeightAt(result.Position)*0.9))
                        return result;

                    GridResults.RecordedValues_GetBestNode.RemoveWhere(p => p != null && p.GridLocation != null && p.GridLocation.Equals(result));
                }

                var results =
                    (from p in MainGrid.MapAsList
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

                result = results.FirstOrDefault();

                if (result != null && result.Position != Vector3.Zero && result.Position != MainGrid.LastResult.Position)
                {
                    MainGrid.LastResult = result;
                    Logger.Log(TrinityLogLevel.Info, LogCategory.Movement, "Best safe gPoint : Loc={0} Dist={1:1} Weight={2} Infos={3}",
                        result.Position, result.Position.Distance2D(loc).ToString("F0"),
                        result.Weight.ToString("F0"), result.WeightInfos
                        );
                }

                GridResults.RecordedValues_GetBestNode.Add(new GetBestNodeResult(result, minRange, maxRange, loc));
                return result;
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