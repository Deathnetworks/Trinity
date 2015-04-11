using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Trinity.Combat.Abilities;
using Trinity.Config.Combat;
using Trinity.Reference;
using Trinity.Technicals;
using Zeta.Common;
using Zeta.Game;
using Zeta.Game.Internals.Actors;
using Logger = Trinity.Technicals.Logger;

namespace Trinity
{
    internal class MainGrid
    {
        #region Helper fields

        public static Vector3 VectorToGrid(Vector3 loc)
        {
            return new Vector3((int)Math.Round(loc.X), (int)Math.Round(loc.Y), (int)Math.Round(loc.Z));
        }

        public static Tuple<int, int> VectorToTuple(Vector3 loc)
        {
            return new Tuple<int, int>((int)loc.X, (int)loc.Y);
        }
        public static Tuple<int, int> VectorToTuple(Vector2 loc)
        {
            return new Tuple<int, int>((int)loc.X, (int)loc.Y);
        }

        private static List<TrinityCacheObject> ObjectCache
        {
            get { return Trinity.ObjectCache; }
        }

        private static CacheData.PlayerCache Player
        {
            get { return CacheData.Player; }
        }

        private static bool AnyTreasureGoblinsPresent
        {
            get
            {
                if (ObjectCache != null)
                    return ObjectCache.Any(u => u.IsTreasureGoblin);
                return false;
            }
        }

        private static TrinityCacheObject CurrentTarget
        {
            get { return Trinity.CurrentTarget; }
        }

        private static HashSet<SNOPower> Hotbar
        {
            get { return CacheData.Hotbar.ActivePowers; }
        }

        private static Zeta.Bot.Navigation.MainGridProvider MainGridProvider
        {
            get { return Trinity.MainGridProvider; }
        }

        #endregion

        #region Fields

        public static Dictionary<Tuple<int, int>, GridNode> NodesRecorded = new Dictionary<Tuple<int, int>, GridNode>();
        public static Dictionary<Vector3, float> NavZones = new Dictionary<Vector3, float>();

        public static HashSet<GridNode> Map = new HashSet<GridNode>();
        public static Vector3 NavZonePosition = new Vector3();

        public static GridNode LastResult = new GridNode();

        public const int GridRange = 70;
        public const int GridSquareSize = 5;
        public const float BaseWeight = GridRange + 10f;

        #endregion

        /// <summary>
        /// Refresh canStandAt point at player position
        /// </summary>
        /// <returns>Return true if grid has been refresh</returns>
        public static bool Refresh()
        {
            return Refresh(Player.Position);
        }

        /// <summary>
        /// Refresh canStandAt point
        /// </summary>
        /// <param name="center"></param>
        /// <returns>Return true if grid has been refresh</returns>
        public static bool Refresh(Vector3 center)
        {
            using (new MemorySpy("MainGrid.Refresh()"))
            {
                #region Fields

                Point minPoint;
                Point maxPoint;
                Point centerPos;
                GridNode bestNode;

                /* offSet to re-peek recorded */
                var offSet = GridMap.GetNodeAt(center);
                if (offSet != null && offSet.Position.Distance2D(center) <= 5f)
                    center = offSet.Position;

                /* corner 1 */
                Vector2 minWorld = new Vector2(center.X - GridRange, center.Y - GridRange);
                minPoint = MainGridProvider.WorldToGrid(minWorld);
                minPoint.X = Math.Max(minPoint.X, 0);
                minPoint.Y = Math.Max(minPoint.Y, 0);

                /* corner 2 */
                Vector2 maxWorld = new Vector2(center.X + GridRange, center.Y + GridRange);
                maxPoint = MainGridProvider.WorldToGrid(maxWorld);
                maxPoint.X = Math.Min(maxPoint.X, MainGridProvider.Width - 1);
                maxPoint.Y = Math.Min(maxPoint.Y, MainGridProvider.Height - 1);

                centerPos = MainGridProvider.WorldToGrid(center.ToVector2());
                bestNode = new GridNode();

                #endregion

                Map.Clear();
                for (int y = minPoint.Y; y <= maxPoint.Y; y = y + (int)GridSquareSize)
                {
                    int searchAreaBasis = y * MainGridProvider.Width;
                    for (int x = minPoint.X; x <= maxPoint.X; x = x + (int)GridSquareSize)
                    {
                        int dx = centerPos.X - x;
                        int dy = centerPos.Y - y;

                        /* Out of range */
                        if (dx * dx + dy * dy > (GridRange / 2.5f) * (GridRange / 2.5f))
                            continue;

                        /* Cant stand at */
                        if (!MainGridProvider.SearchArea[searchAreaBasis + x])
                            continue;

                        Vector2 xy = MainGridProvider.GridToWorld(new Point(x, y));

                        GridNode gridNode = new GridNode();
                        GridNode nodeRecorded;

                        bool isRecorded = NodesRecorded.TryGetValue(VectorToTuple(xy), out nodeRecorded);

                        if (isRecorded)
                        {
                            nodeRecorded.ResetTickValues();
                            gridNode.Position = nodeRecorded.Position;
                            gridNode.UnchangeableWeight = nodeRecorded.UnchangeableWeight;
                            gridNode.UnchangeableWeightInfos = nodeRecorded.UnchangeableWeightInfos;
                            gridNode.Position = nodeRecorded.Position;

                            gridNode.NearbyExitsCount = nodeRecorded.NearbyExitsCount;
                            gridNode.NearbyGridPointsCount = nodeRecorded.NearbyGridPointsCount;

                            gridNode.LastDynamicWeightValues = nodeRecorded.LastDynamicWeightValues;
                            gridNode.LastTargetWeightValues = nodeRecorded.LastTargetWeightValues;
                            gridNode.LastClusterWeightValues = nodeRecorded.LastClusterWeightValues;
                            gridNode.LastMonsterWeightValues = nodeRecorded.LastMonsterWeightValues;

                            NodesRecorded.Remove(VectorToTuple(gridNode.Position));
                        }
                        else
                        {
                            Vector3 xyz = new Vector3((int)xy.X, (int)xy.Y, Player.Position.Z + 2);
                            gridNode.Position = xyz;
                            gridNode.ResetTickValues();
                            gridNode.SetUnchangeableWeight();
                        }

                        gridNode.OperateWeight(WeightType.Dynamic, "BaseDistanceWeight", (GridRange - gridNode.Distance) * 2f);

                        gridNode.SetTargetWeights();
                        gridNode.SetAvoidancesWeights();
                        gridNode.SetCacheObjectsWeights();

                        Map.Add(gridNode);

                        /* Check best nav location */
                        if (gridNode.Weight > bestNode.Weight ||
                            (gridNode.Weight == bestNode.Weight && gridNode.Distance < bestNode.Distance))
                        {
                            bestNode = gridNode;
                        }
                    }
                }

                /* low, so reduce list to minimum with dist & weight limit */
                foreach (var gridNode in Map)
                {
                    /* Something to do */
                    gridNode.FinalCheck();

                    if (gridNode.Weight <= bestNode.Weight * 0.75)
                    {
                        if (!NodesRecorded.ContainsKey(VectorToTuple(gridNode.Position)))
                            NodesRecorded.Add(VectorToTuple(gridNode.Position), gridNode);

                        continue;
                    }

                    if (gridNode.Distance > 40f)
                    {
                        if (!NodesRecorded.ContainsKey(VectorToTuple(gridNode.Position)))
                            NodesRecorded.Add(VectorToTuple(gridNode.Position), gridNode);

                        continue;
                    }

                    /* Ray cast */
                    gridNode.SetNavWeight();

                    /* Count ray casted points within distance */
                    if (gridNode.NearbyExitsCount < 0)
                    {
                        gridNode.NearbyExitsCount = gridNode.NearbyExitsWithinDistance((float)(bestNode.Weight * 0.75), 30f);

                        if (gridNode.NearbyExitsCount > 0)
                            gridNode.OperateWeight(WeightType.Unchangeable, String.Format("HasExits[{0}]", gridNode.NearbyExitsCount), BaseWeight * gridNode.NearbyExitsCount);

                        if (gridNode.NearbyGridPointsCount > 0)
                            gridNode.OperateWeight(WeightType.Unchangeable, String.Format("CloseToOtherPoints[{0}]", gridNode.NearbyGridPointsCount), BaseWeight * gridNode.NearbyGridPointsCount);
                    }

                    /* try catch fastest to check key in collection */
                    if (!NodesRecorded.ContainsKey(VectorToTuple(gridNode.Position)))
                        NodesRecorded.Add(VectorToTuple(gridNode.Position), gridNode);
                }

                /* test ! */
                using (new MemorySpy("MainGrid.Refresh().Substitue"))
                {
                    /* create substitue */
                    HashSet<GridNode> subList = new HashSet<GridNode>();
                    for (int y = minPoint.Y; y <= maxPoint.Y; y += (int)(GridSquareSize * 0.5))
                    {
                        int searchAreaBasis = y * MainGridProvider.Width;
                        for (int x = minPoint.X; x <= maxPoint.X; x += (int)(GridSquareSize * 0.5))
                        {
                            int dx = centerPos.X - x;
                            int dy = centerPos.Y - y;

                            /* Out of range */
                            if (dx * dx + dy * dy > (GridRange / 2.5f) * (GridRange / 2.5f))
                                continue;

                            /* Cant stand at */
                            if (!MainGridProvider.SearchArea[searchAreaBasis + x])
                                continue;

                            Vector2 xy = MainGridProvider.GridToWorld(new Point(x, y));
                            if (NodesRecorded.ContainsKey(VectorToTuple(xy)))
                                continue;

                            GridNode cornerNE = null;
                            GridNode cornerNW = null;
                            GridNode cornerSW = null;
                            GridNode cornerSE = null;

                            bool goal = false;
                            int cornerCount = 1;


                            for (int _y = (int)(xy.Y - GridSquareSize); _y <= (int)(xy.Y); _y++)
                            {
                                if (goal) break;

                                for (int _x = (int)(xy.X - GridSquareSize); _x <= (int)(xy.X); _x++)
                                {
                                    if (goal) break;

                                    var key = new Tuple<int, int>(_x, _y);

                                    if (!goal && NodesRecorded.ContainsKey(key))
                                    {
                                        cornerNW = NodesRecorded[key];

                                        key = new Tuple<int, int>((int)(_x + GridSquareSize), _y);
                                        if (NodesRecorded.ContainsKey(key))
                                        {
                                            cornerNE = NodesRecorded[key];
                                            cornerCount++;
                                        }

                                        key = new Tuple<int, int>(_x, (int)(_y + GridSquareSize));
                                        if (NodesRecorded.ContainsKey(key))
                                        {
                                            cornerSW = NodesRecorded[key];
                                            cornerCount++;
                                        }

                                        key = new Tuple<int, int>((int)(_x + GridSquareSize), (int)(_y + GridSquareSize));
                                        if (NodesRecorded.ContainsKey(key))
                                        {
                                            cornerSE = NodesRecorded[key];
                                            cornerCount++;
                                        }

                                        goal = true;
                                        break;
                                    }
                                }
                            }

                            if (goal)
                            {
                                GridNode substitue = new GridNode(new Vector3(xy.X, xy.Y, Player.Position.Z + 2));

                                double kNW = cornerNW != null ? cornerNW.Position.Distance2D(substitue.Position) : 0d;
                                double kNE = cornerNE != null ? cornerNE.Position.Distance2D(substitue.Position) : 0d;
                                double kSW = cornerSW != null ? cornerSW.Position.Distance2D(substitue.Position) : 0d;
                                double kSE = cornerSE != null ? cornerSE.Position.Distance2D(substitue.Position) : 0d;

                                kNW = kNW > 0d ? mK - kNW : 0d;
                                kNE = kNE > 0d ? mK - kNE : 0d;
                                kSW = kSW > 0d ? mK - kSW : 0d;
                                kSE = kSE > 0d ? mK - kSE : 0d;

                                double div = kNW + kNE + kSW + kSE;

                                double weight = cornerNW != null ? (kNW * cornerNW.Weight) : 0d;
                                weight += cornerNE != null ? (kNE * cornerNE.Weight) : 0d;
                                weight += cornerSW != null ? (kSW * cornerSW.Weight) : 0d;
                                weight += cornerSE != null ? (kSE * cornerSE.Weight) : 0d;

                                weight = div != 0d ? weight / div : 0d;

                                double clusterWeight = cornerNW != null ? (kNW * cornerNW.ClusterWeight) : 0d;
                                clusterWeight += cornerNE != null ? (kNE * cornerNE.ClusterWeight) : 0d;
                                clusterWeight += cornerSW != null ? (kSW * cornerSW.ClusterWeight) : 0d;
                                clusterWeight += cornerSE != null ? (kSE * cornerSE.ClusterWeight) : 0d;

                                clusterWeight = div != 0d ? clusterWeight / div : 0d;

                                substitue.Weight = weight;
                                substitue.ClusterWeight = clusterWeight;
                                substitue.WeightInfos = String.Format("Fake weighted node generated with {0} corner(s), Weight={1:0} wNW={6:0}-{2:0} wNE={7:0}-{3:0} wSW={8:0}-{4:0} wSE={9:0}-{5:0}", 
                                    cornerCount,
                                    weight,
                                    cornerNW != null ? cornerNW.Weight : 0,
                                    cornerNE != null ? cornerNE.Weight : 0,
                                    cornerSW != null ? cornerSW.Weight : 0,
                                    cornerSE != null ? cornerSE.Weight : 0,
                                    kNW,
                                    kNE,
                                    kSW,
                                    kSE);

                                subList.Add(substitue);
                            }
                        }
                    }

                    if (subList.Any())
                    {
                        subList.ForEach(n => Map.Add(n));
                    }
                }
            }

            return true;
        }

        // test !
        internal static double mK = GridSquareSize * GridSquareSize;

        internal static void ResetTickValues()
        {
            using (new MemorySpy("MainGrid.ResetTickValues()"))
            {
                /* at every tick */
                GridResults.ResetTickValues();

                _tickValueMinRangeToTarget = -1f;
                _isTickRecordedPlayerShouldKite = false;
                _isTickRecordedShouldAvoidAoE = false;
                _isTickRecordedShouldCollectHealthGlobe = false;
                TickValueHealthGlobeWeightPct = -1f;

                ObjectCacheIsEmpty = ObjectCache != null && !ObjectCache.Any();
                AvoidancesCacheIsEmpty = !CacheData.AvoidanceObstacles.Any();
                PositionsCacheIsEmpty = !CacheData.VisitedZones.Any();
                UnSafeZonesCacheIsEmpty = !CacheData.UnSafeZones.Any();

                ShouldKiteBosses = CombatBase.KiteMode != KiteMode.Never || Trinity.Player.AvoidDeath;
                ShouldKiteElites = CombatBase.KiteMode == KiteMode.Elites || CombatBase.KiteMode == KiteMode.Always || Trinity.Player.AvoidDeath;
                ShouldKiteTrashs = CombatBase.KiteMode == KiteMode.Always || Trinity.Player.AvoidDeath;
                ShouldFlee = (Trinity.Settings.Combat.Misc.FleeInGhostMode && Trinity.Player.IsGhosted) || Trinity.Player.AvoidDeath;

                ShouldBeAwayFromAoE = Player.IsRanged || (PlayerShouldKite && Player.NeedToKite) || ShouldFlee || ShouldAvoidAoE;

                PlayerIsInTrialRift = Player.LevelAreaId.Equals(DataDictionary.RiftTrialLevelAreaId);

                using (new MemorySpy("MainGrid.ResetTickValues().RemoveObsoletNode"))
                {
                    //if (NodesRecorded.Any(g => g.Value.ObjectOOR(g.Value.Position, 200)))
                    if (NodesRecorded.Count() > 7500)
                        NodesRecorded.Clear();
                }

                if (NavZonePosition == new Vector3() ||
                    NavZonePosition.Distance2D(Player.Position) >= 5f)
                {
                    NavZonePosition = Player.Position;
                    NavZones.Clear();
                }
            }
        }

        public static bool ObjectCacheIsEmpty { get; set; }
        public static bool AvoidancesCacheIsEmpty { get; set; }
        public static bool PositionsCacheIsEmpty { get; set; }
        public static bool UnSafeZonesCacheIsEmpty { get; set; }

        public static bool ShouldKiteBosses { get; set; }
        public static bool ShouldKiteElites { get; set; }
        public static bool ShouldKiteTrashs { get; set; }
        public static bool ShouldFlee { get; set; }

        public static bool ShouldBeAwayFromAoE { get; set; }

        public static bool PlayerIsInTrialRift { get; set; }

        private static float _tickValueMinRangeToTarget = -1f;

        public static float MinRangeToTarget
        {
            get
            {
                if (_tickValueMinRangeToTarget >= 0)
                    return _tickValueMinRangeToTarget;

                _tickValueMinRangeToTarget = 30f;

                if (CombatBase.CurrentPower.MinimumRange > 0)
                    _tickValueMinRangeToTarget = CombatBase.CurrentPower.MinimumRange;
                else if (CombatBase.LastPowerRange > 0)
                    _tickValueMinRangeToTarget = CombatBase.LastPowerRange;

                if (Player.ActorClass == ActorClass.DemonHunter && Trinity.Settings.Combat.DemonHunter.RangedAttackRange > 0)
                    _tickValueMinRangeToTarget = Math.Min(Trinity.Settings.Combat.DemonHunter.RangedAttackRange, _tickValueMinRangeToTarget);

                return _tickValueMinRangeToTarget;
            }
        }

        private static bool _isTickRecordedPlayerShouldKite;
        private static bool TickValuePlayerShouldKite { get; set; }

        public static bool PlayerShouldKite
        {
            get
            {
                if (_isTickRecordedPlayerShouldKite)
                    return TickValuePlayerShouldKite;

                TickValuePlayerShouldKite = Player.AvoidDeath ||
                                            (CombatBase.CurrentPower.SNOPower != SNOPower.None &&
                                             CombatBase.KiteDistance > 0 &&
                                             MinRangeToTarget > 1f &&
                                             MinRangeToTarget > CombatBase.KiteDistance);

                _isTickRecordedPlayerShouldKite = true;
                return TickValuePlayerShouldKite;
            }
        }

        private static bool _isTickRecordedShouldAvoidAoE;
        private static bool TickValueShouldAvoidAoE { get; set; }

        public static bool ShouldAvoidAoE
        {
            get
            {
                if (_isTickRecordedShouldAvoidAoE)
                    return TickValueShouldAvoidAoE;

                TickValueShouldAvoidAoE = Player.AvoidDeath || Player.CurrentHealthPct <= 0.3 || Player.StandingInAvoidance ||
                                          (CurrentTarget != null && CurrentTarget.IsAvoidance);

                _isTickRecordedShouldAvoidAoE = true;
                return TickValueShouldAvoidAoE;
            }
        }

        private static bool _isTickRecordedShouldCollectHealthGlobe;
        private static bool TickValueShouldCollectHealthGlobe { get; set; }

        public static bool ShouldCollectHealthGlobe
        {
            get
            {
                if (_isTickRecordedShouldCollectHealthGlobe)
                    return TickValueShouldCollectHealthGlobe;

                if (Trinity.Settings.Combat.Misc.HiPriorityHG)
                    TickValueShouldCollectHealthGlobe = true;

                else if (Player.CurrentHealthPct < CombatBase.EmergencyHealthGlobeLimit)
                    TickValueShouldCollectHealthGlobe = true;

                else
                    TickValueShouldCollectHealthGlobe = Player.PrimaryResourcePct < CombatBase.HealthGlobeResource &&
                                                        (Legendary.ReapersWraps.IsEquipped ||
                                                         (Player.ActorClass == ActorClass.Witchdoctor && CacheData.Hotbar.PassiveSkills.Contains(SNOPower.Witchdoctor_Passive_GruesomeFeast)) ||
                                                         (Player.ActorClass == ActorClass.DemonHunter && CacheData.Hotbar.PassiveSkills.Contains(SNOPower.DemonHunter_Passive_Vengeance)));

                _isTickRecordedShouldCollectHealthGlobe = true;
                return TickValueShouldCollectHealthGlobe;
            }
        }

        public static float TickValueHealthGlobeWeightPct = -1f;

        public static float HealthGlobeWeightPct
        {
            get
            {
                if (TickValueHealthGlobeWeightPct >= 0)
                    return TickValueHealthGlobeWeightPct;

                TickValueHealthGlobeWeightPct = (float)(1f - Player.CurrentHealthPct) * 10f;
                if (Player.CurrentHealthPct > 1 && Player.PrimaryResourcePct > 1)
                {
                    TickValueHealthGlobeWeightPct += (float)(1f - Player.PrimaryResourcePct) * 10f;
                    TickValueHealthGlobeWeightPct = TickValueHealthGlobeWeightPct * 0.5f;
                }

                return TickValueHealthGlobeWeightPct;
            }
        }
    }
}