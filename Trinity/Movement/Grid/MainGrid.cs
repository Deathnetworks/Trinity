using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using Trinity.Combat.Abilities;
using Trinity.Config.Combat;
using Trinity.Reference;
using Trinity.Technicals;
using Zeta.Bot.Dungeons;
using Zeta.Bot.Navigation;
using Zeta.Common;
using Zeta.Game;
using Zeta.Game.Internals.Actors;

namespace Trinity
{
    class MainGrid
    {
        #region Helper fields

        public static Vector3 VectorToGrid(Vector3 _loc)
        {
            return new Vector3((int)Math.Round(_loc.X), (int)Math.Round(_loc.Y), (int)Math.Round(_loc.Z));
        }
        public static Tuple<int, int> VectorToTuple(Vector3 _loc)
        {
            return new Tuple<int, int>((int)_loc.X, (int)_loc.Y);
        }

        private static List<TrinityCacheObject> ObjectCache
        {
            get
            {
                return Trinity.ObjectCache;
            }
        }
        private static CacheData.PlayerCache Player
        {
            get
            {
                return CacheData.Player;
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
                return CacheData.Hotbar.ActivePowers;
            }
        }
        private static Zeta.Bot.Navigation.MainGridProvider MainGridProvider
        {
            get
            {
                return Trinity.MainGridProvider;
            }
        }
        #endregion
        #region Fields

        public static Dictionary<Tuple<int,int>,GridNode> NodesRecorded = new Dictionary<Tuple<int,int>,GridNode>();
        public static Dictionary<Vector3, float> NavZones = new Dictionary<Vector3, float>();

        public static HashSet<GridNode> MapAsList = new HashSet<GridNode>();
        public static Vector3 NavZonePosition = new Vector3();

        public static GridNode LastResult = new GridNode();

        public const float GridRange = 75f;
        public const float GridSquareSize = 2f;
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
        /// <param name="_center"></param>
        /// <returns>Return true if grid has been refresh</returns>
        public static bool Refresh(Vector3 _center)
        {
            using (new MemorySpy("MainGrid.Refresh()"))
            {
                #region Fields

                Vector2 _minWorld;
                Point _minPoint;
                Vector2 _maxWorld;
                Point _maxPoint;
                Point _centerPos;
                GridNode _bestNode;

                using (new MemorySpy("MainGrid.Refresh().SetInit"))
                {
                    /* corner 1 */
                    _minWorld = new Vector2(_center.X - GridRange, _center.Y - GridRange);
                    _minPoint = MainGridProvider.WorldToGrid(_minWorld);
                    _minPoint.X = Math.Max(_minPoint.X, 0);
                    _minPoint.Y = Math.Max(_minPoint.Y, 0);

                    /* corner 2 */
                    _maxWorld = new Vector2(_center.X + GridRange, _center.Y + GridRange);
                    _maxPoint = MainGridProvider.WorldToGrid(_maxWorld);
                    _maxPoint.X = Math.Min(_maxPoint.X, MainGridProvider.Width - 1);
                    _maxPoint.Y = Math.Min(_maxPoint.Y, MainGridProvider.Height - 1);

                    _centerPos = MainGridProvider.WorldToGrid(_center.ToVector2());
                    _bestNode = new GridNode(new Vector3());
                }
                #endregion

                MapAsList.Clear();
                for (int _y = _minPoint.Y; _y <= _maxPoint.Y; _y = _y + (int)GridSquareSize)
                {
                    int _searchAreaBasis = _y * MainGridProvider.Width;
                    for (int _x = _minPoint.X; _x <= _maxPoint.X; _x = _x + (int)GridSquareSize)
                    {

                        int _dx = _centerPos.X - _x;
                        int _dy = _centerPos.Y - _y;

                        /* Out of range */
                        using (new MemorySpy("MainGrid.Refresh().OutOfRangeCheck"))
                        {
                            if (_dx * _dx + _dy * _dy > (GridRange / 2f) * (GridRange / 2f))
                                continue;
                        }

                        /* Cant stand at */
                        using (new MemorySpy("MainGrid.Refresh().CantStandAtCheck"))
                        {
                            if (!MainGridProvider.SearchArea[_searchAreaBasis + _x])
                                continue;
                        }

                        Vector2 _xy;
                        Vector3 _xyz;
                        GridNode _g;

                        using (new MemorySpy("MainGrid.Refresh().SetPosition"))
                        {
                            _xy = MainGridProvider.GridToWorld(new Point(_x, _y));

                            /* Round to int pair sup, require to no change GridSquareSize value (2) */
                            _xy.X = ((int)_xy.X & 1) == 1 ? (int)_xy.X + 1 : (int)_xy.X;
                            _xy.Y = ((int)_xy.Y & 1) == 1 ? (int)_xy.Y + 1 : (int)_xy.Y;

                            _xyz = new Vector3(_xy.X, _xy.Y, MainGridProvider.GetHeight(_xy));
                            _g = new GridNode(_xyz);
                        }

                        GridNode _nodeRecorded;
                        bool _recorded = false;

                        using (new MemorySpy("MainGrid.Refresh().CheckRecorded"))
                        { _recorded = MainGrid.NodesRecorded.TryGetValue(MainGrid.VectorToTuple(_g.Position), out _nodeRecorded); }

                        if (_recorded)
                        {
                            using (new MemorySpy("MainGrid.Refresh().GetRecordedValues"))
                            {
                                _nodeRecorded.ResetTickValues();

                                _g.UnchangeableWeight = _nodeRecorded.UnchangeableWeight;
                                _g.UnchangeableWeightInfos = _nodeRecorded.UnchangeableWeightInfos;

                                _g.NearbyExitsCount = _nodeRecorded.NearbyExitsCount;
                                _g.NearbyGridPointsCount = _nodeRecorded.NearbyGridPointsCount;

                                _g.LastDynamicWeightValues = _nodeRecorded.LastDynamicWeightValues;
                                _g.LastTargetWeightValues = _nodeRecorded.LastTargetWeightValues;
                                _g.LastClusterWeightValues = _nodeRecorded.LastClusterWeightValues;
                                _g.LastMonsterWeightValues = _nodeRecorded.LastMonsterWeightValues;

                                MainGrid.NodesRecorded.Remove(MainGrid.VectorToTuple(_g.Position));
                            }
                        }
                        else
                        {
                            using (new MemorySpy("MainGrid.Refresh().GetNewValues"))
                            {
                                _g.ResetTickValues();
                                _g.SetUnchangeableWeight();
                            }
                        }

                        _g.OperateWeight(WeightType.Dynamic, "BaseDistanceWeight", (MainGrid.GridRange - _g.Distance) * 5f);

                        using (new MemorySpy("MainGrid.Refresh().SetTargetWeights"))
                        {
                            _g.SetTargetWeights();
                        }

                        using (new MemorySpy("MainGrid.Refresh().SetAvoidancesWeights"))
                        {
                            _g.SetAvoidancesWeights();
                        }

                        using (new MemorySpy("MainGrid.Refresh().SetCacheObjectsWeights"))
                        {
                            _g.SetCacheObjectsWeights();
                        }

                        MapAsList.Add(_g);

                        /* Check best nav location */
                        if (_g.Weight > _bestNode.Weight ||
                            (_g.Weight == _bestNode.Weight && _g.Distance < _bestNode.Distance))
                        {
                            _bestNode = _g;
                        }
                    }
                }

                /* low, so reduce list to minimum with dist & weight limit */
                using (new MemorySpy("MainGrid.Refresh().LowWeighting"))
                {
                    foreach (var _g in MapAsList)
                    {
                        /* Something to do */
                        _g.FinalCheck();

                        if (_g.Weight <= _bestNode.Weight * 0.75)
                        {
                            using (new MemorySpy("MainGrid.Refresh().DictionaryAdd"))
                            {
                                if (!MainGrid.NodesRecorded.ContainsKey(MainGrid.VectorToTuple(_g.Position)))
                                    MainGrid.NodesRecorded.Add(MainGrid.VectorToTuple(_g.Position), _g);
                            }

                            continue;
                        }

                        if (_g.Distance > 40f)
                        {
                            using (new MemorySpy("MainGrid.Refresh().DictionaryAdd"))
                            {
                                if (!MainGrid.NodesRecorded.ContainsKey(MainGrid.VectorToTuple(_g.Position)))
                                    MainGrid.NodesRecorded.Add(MainGrid.VectorToTuple(_g.Position), _g);
                            }

                            continue;
                        }

                        /* Ray cast */
                        using (new MemorySpy("MainGrid.Refresh().SetNavWeight"))
                        {
                            _g.SetNavWeight();
                        }

                        /* Count ray casted points within distance */
                        using (new MemorySpy("MainGrid.Refresh().SetSafeZoneWeight"))
                        {
                            if (_g.NearbyExitsCount < 0)
                            {
                                _g.NearbyExitsCount = _g.NearbyExitsWithinDistance((float)(_bestNode.Weight * 0.75), 30f);

                                if (_g.NearbyExitsCount > 0)
                                    _g.OperateWeight(WeightType.Unchangeable, String.Format("HasExits[{0}]", _g.NearbyExitsCount), MainGrid.BaseWeight * _g.NearbyExitsCount);

                                if (_g.NearbyGridPointsCount > 0)
                                    _g.OperateWeight(WeightType.Unchangeable, String.Format("CloseToOtherPoints[{0}]", _g.NearbyGridPointsCount), MainGrid.BaseWeight * _g.NearbyGridPointsCount);
                            }
                        }

                        /* try catch fastest to check key in collection */
                        using (new MemorySpy("MainGrid.Refresh().AddToDictionary"))
                        {
                            if (!MainGrid.NodesRecorded.ContainsKey(MainGrid.VectorToTuple(_g.Position)))
                                MainGrid.NodesRecorded.Add(MainGrid.VectorToTuple(_g.Position), _g);
                        }
                    }
                }
            }

            return true;
        }

        internal static void ResetTickValues()
        {
            using (new MemorySpy("MainGrid.ResetTickValues()"))
            {
                /* at every tick */
                GridResults.ResetTickValues();

                tickValue_MinRangeToTarget = -1f;
                isTickRecorded_PlayerShouldKite = false;
                isTickRecorded_ShouldAvoidAoE = false;
                isTickRecorded_ShouldCollectHealthGlobe = false;
                tickValue_HealthGlobeWeightPct = -1f;

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

        private static float tickValue_MinRangeToTarget = -1f;
        public static float MinRangeToTarget
        {
            get
            {
                if (tickValue_MinRangeToTarget >= 0)
                    return tickValue_MinRangeToTarget;

                tickValue_MinRangeToTarget = 30f;

                if (CombatBase.CurrentPower.MinimumRange > 0)
                    tickValue_MinRangeToTarget = CombatBase.CurrentPower.MinimumRange;
                else if (CombatBase.LastPowerRange > 0)
                    tickValue_MinRangeToTarget = CombatBase.LastPowerRange;

                if (Player.ActorClass == ActorClass.DemonHunter && Trinity.Settings.Combat.DemonHunter.RangedAttackRange > 0)
                    tickValue_MinRangeToTarget = Math.Min(Trinity.Settings.Combat.DemonHunter.RangedAttackRange, tickValue_MinRangeToTarget);

                return tickValue_MinRangeToTarget;
            }
        }
        private static bool isTickRecorded_PlayerShouldKite = false;
        private static bool tickValue_PlayerShouldKite { get; set; }
        public static bool PlayerShouldKite
        {
            get
            {
                if (isTickRecorded_PlayerShouldKite)
                    return tickValue_PlayerShouldKite;

                tickValue_PlayerShouldKite = Player.AvoidDeath ||
                    (CombatBase.CurrentPower.SNOPower != SNOPower.None &&
                    CombatBase.KiteDistance > 0 &&
                    MinRangeToTarget > 1f &&
                    MinRangeToTarget > CombatBase.KiteDistance);

                isTickRecorded_PlayerShouldKite = true;
                return tickValue_PlayerShouldKite;
            }
        }
        private static bool isTickRecorded_ShouldAvoidAoE = false;
        private static bool tickValue_ShouldAvoidAoE { get; set; }
        public static bool ShouldAvoidAoE
        {
            get
            {
                if (isTickRecorded_ShouldAvoidAoE)
                    return tickValue_ShouldAvoidAoE;
                
                tickValue_ShouldAvoidAoE = Player.AvoidDeath || Player.CurrentHealthPct <= 0.3 || Player.StandingInAvoidance ||
                    (CurrentTarget != null && CurrentTarget.IsAvoidance);

                isTickRecorded_ShouldAvoidAoE = true;
                return tickValue_ShouldAvoidAoE;
            }
        }
        private static bool isTickRecorded_ShouldCollectHealthGlobe = false;
        private static bool tickValue_ShouldCollectHealthGlobe { get; set; }
        public static bool ShouldCollectHealthGlobe
        {
            get
            {
                if (isTickRecorded_ShouldCollectHealthGlobe)
                    return tickValue_ShouldCollectHealthGlobe;

                if (Trinity.Settings.Combat.Misc.HiPriorityHG)
                    tickValue_ShouldCollectHealthGlobe = true;

                else if (Player.CurrentHealthPct < CombatBase.EmergencyHealthGlobeLimit)
                    tickValue_ShouldCollectHealthGlobe = true;

                else
                    tickValue_ShouldCollectHealthGlobe = Player.PrimaryResourcePct < CombatBase.HealthGlobeResource &&
                        (Legendary.ReapersWraps.IsEquipped ||
                        (Player.ActorClass == ActorClass.Witchdoctor && CacheData.Hotbar.PassiveSkills.Contains(SNOPower.Witchdoctor_Passive_GruesomeFeast)) ||
                        (Player.ActorClass == ActorClass.DemonHunter && CacheData.Hotbar.PassiveSkills.Contains(SNOPower.DemonHunter_Passive_Vengeance)));

                isTickRecorded_ShouldCollectHealthGlobe = true;
                return tickValue_ShouldCollectHealthGlobe;
            }
        }
        public static float tickValue_HealthGlobeWeightPct = -1f;
        public static float HealthGlobeWeightPct
        {
            get
            {
                if (tickValue_HealthGlobeWeightPct >= 0)
                    return tickValue_HealthGlobeWeightPct;

                tickValue_HealthGlobeWeightPct = (float)(1f - Player.CurrentHealthPct) * 10f;
                if (Player.CurrentHealthPct > 1 && Player.PrimaryResourcePct > 1)
                {
                    tickValue_HealthGlobeWeightPct += (float)(1f - Player.PrimaryResourcePct) * 10f;
                    tickValue_HealthGlobeWeightPct = (float)tickValue_HealthGlobeWeightPct * 0.5f;
                }

                return tickValue_HealthGlobeWeightPct;
            }
        }
    }
}
