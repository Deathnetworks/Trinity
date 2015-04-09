using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using Trinity.Combat.Abilities;
using Trinity.Config.Combat;
using Trinity.DbProvider;
using Trinity.Reference;
using Trinity.Technicals;
using Zeta.Bot.Navigation;
using Zeta.Common;
using Zeta.Game;
using Zeta.Game.Internals.Actors;
using Logger = Trinity.Technicals.Logger;

namespace Trinity
{
    class GridMap
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

            return MainGrid.MapAsList.Select(p => new { p.Position, Weight = p.DynamicWeight }).ToDictionary(p => p.Position, p => (float)p.Weight);
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
        public static GridNode GetBestClusterNode(Vector3 _loc = new Vector3(), float _radius = 15f, float _range = 65f, float _minRange = 0f, int _size = 1, bool _useWeights = true, bool _useDefault = true)
        {
            using (new MemorySpy("GridMap.GetBestClusterNode()"))
            {
                if (_loc == new Vector3())
                    _loc = Trinity.Player.Position;
                bool _atPlayer = _loc == Trinity.Player.Position;

                GridNode _cluster;
                if (GridResults.HasRecordedValue_GetBestClusterNode(out _cluster, _range, _loc))
                {
                    /* Keep last safe point */
                    if (_cluster.ClusterWeight >= (GetPointAt(_cluster.Position).ClusterWeight * 0.9))
                        return _cluster;

                    GridResults.RecordedValues_GetBestClusterNode.RemoveWhere(p => p.GridLocation.Equals(_cluster));
                }

                if (ClusterNodeExist)
                {
                    _cluster = (
                        from _g in MainGrid.MapAsList
                        where
                            _g.ClusterWeight > 0 &&
                            (_atPlayer && _g.Distance >= _minRange ||
                            !_atPlayer && _g.Position.Distance2D(_loc) >= _minRange) &&
                            (_atPlayer && _g.Distance < _range ||
                            !_atPlayer && _g.Position.Distance2D(_loc) < _range)
                        orderby
                            _g.ClusterWeight descending
                        select _g).ToList().FirstOrDefault();

                    if (_cluster != null && _cluster.ClusterWeight > 0f)
                    {
                        GridResults.RecordedValues_GetBestClusterNode.Add(new GetBestClusterNodeResult(_cluster, _range, _loc));
                        return _cluster;
                    }
                }

                if (_useDefault)
                    return new GridNode(TargetUtil.GetBestClusterPoint(_radius, _range, _size, _useWeights, _loc));

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
                weightAtPoint = (float)gPoint.DynamicWeight;
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

            return MainGrid.MapAsList.OrderBy(g => g.Position.Distance2D(loc)).FirstOrDefault(); ;
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
                    if (result.Weight >= (GetWeightAt(result.Position) * 0.9))
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
