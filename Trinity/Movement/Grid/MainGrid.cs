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

        public static Stopwatch[] Timers = Enumerable.Range(0, 21).Select(i => new Stopwatch()).ToArray();
        public static HashSet<GridNode> MapAsList = new HashSet<GridNode>();
        public static Vector3 NavZonePosition = new Vector3();

        public static GridNode LastResult = new GridNode();

        public const float GridRange = 85f;
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
            using (new PerformanceLogger("GridNavigation.RefreshGridMap"))
            {
                Timers[0].Start();
                #region Fields

                Timers[14].Start();
                /* Repick, increase performance if GridSquareSize != 1 */
                var _centerToGrid = GridMap.GetPointAt(_center);
                if (_centerToGrid != null) _center = _centerToGrid.Position;

                /* corner 1 */
                Vector2 _minWorld = new Vector2(_center.X - GridRange, _center.Y - GridRange);
                Point _minPoint = MainGridProvider.WorldToGrid(_minWorld);
                _minPoint.X = Math.Max(_minPoint.X, 0);
                _minPoint.Y = Math.Max(_minPoint.Y, 0);

                /* corner 2 */
                Vector2 _maxWorld = new Vector2(_center.X + GridRange, _center.Y + GridRange);
                Point _maxPoint = MainGridProvider.WorldToGrid(_maxWorld);
                _maxPoint.X = Math.Min(_maxPoint.X, MainGridProvider.Width - 1);
                _maxPoint.Y = Math.Min(_maxPoint.Y, MainGridProvider.Height - 1);

                Point _centerPos = MainGridProvider.WorldToGrid(_center.ToVector2());
                GridNode _bestNode = new GridNode(new Vector3());
                Timers[14].Stop();
                #endregion

                MapAsList.Clear();
                for (int _y = _minPoint.Y; _y <= _maxPoint.Y; _y = _y + (int)GridSquareSize)
                {
                    int _searchAreaBasis = _y * MainGridProvider.Width;
                    for (int _x = _minPoint.X; _x <= _maxPoint.X; _x = _x + (int)GridSquareSize)
                    {
                        Timers[15].Start();
                        int _dx = _centerPos.X - _x;
                        int _dy = _centerPos.Y - _y;

                        /* Out of range */
                        if (_dx * _dx + _dy * _dy > (GridRange / 2.5f) * (GridRange / 2.5f))
                        {
                            Timers[15].Stop();
                            continue;
                        }

                        /* Cant stand at */
                        if (!MainGridProvider.SearchArea[_searchAreaBasis + _x])
                        {
                            Timers[15].Stop();
                            continue;
                        }
                        Timers[15].Stop();

                        /* THIS IS A NODE */

                        Timers[1].Start();
                        Vector2 _xy = MainGridProvider.GridToWorld(new Point(_x, _y));
                        Vector3 _xyz = new Vector3(_xy.X, _xy.Y, MainGridProvider.GetHeight(_xy));

                        GridNode _g = new GridNode(_xyz);
                        GridNode _nodeRecorded;
                        if (MainGrid.NodesRecorded.TryGetValue(MainGrid.VectorToTuple(_g.Position), out _nodeRecorded))
                        {
                            /* switch to recorded point */
                            Timers[1].Stop();

                            /* Take back recorded values */
                            Timers[2].Start();
                            /* Reset value */
                            _nodeRecorded.ResetTickValues();

                            _g.DynamicWeight = _nodeRecorded.DynamicWeight;
                            _g.DynamicWeightInfos = _nodeRecorded.DynamicWeightInfos;

                            _g.UnchangeableWeight = _nodeRecorded.UnchangeableWeight;
                            _g.UnchangeableWeightInfos = _nodeRecorded.UnchangeableWeightInfos;

                            _g.NearbyExitsCount = _nodeRecorded.NearbyExitsCount;
                            _g.NearbyGridPointsCount = _nodeRecorded.NearbyGridPointsCount;

                            _g.ObjectsLastWeightValues = _nodeRecorded.ObjectsLastWeightValues;

                            /* Then remove it */
                            MainGrid.NodesRecorded.Remove(MainGrid.VectorToTuple(_g.Position));
                            Timers[2].Stop();
                        }
                        else
                        {
                            /* Create new point */
                            Timers[1].Stop();

                            Timers[3].Start();
                            /* Reset value */
                            _g.ResetTickValues();

                            /* This weight never change */
                            _g.SetUnchangeableWeight();
                            Timers[3].Stop();
                        }

                        /* Everytime */
                        _g.OperateDynamicWeight("BaseDistanceWeight", (MainGrid.GridRange - _g.Distance) * 5f);

                        Timers[4].Start();
                        /* Set weight related to unit target */
                        _g.SetTargetWeights();
                        Timers[4].Stop();

                        Timers[5].Start();
                        /* Set weight related to avoidances */
                        _g.SetAvoidancesWeights();
                        Timers[5].Stop();

                        Timers[6].Start();
                        /* Set weight related to cache objects */
                        _g.SetCacheObjectsWeights();
                        Timers[6].Stop();

                        Timers[7].Start();
                        /* try catch fastest to check key in collection */
                        try { MainGrid.NodesRecorded.Add(MainGrid.VectorToTuple(_xyz), _g); }
                        catch { }
                        Timers[7].Stop();

                        MapAsList.Add(_g);

                        /* Check best nav location */
                        if (_g.Weight > _bestNode.Weight ||
                            (_g.Weight == _bestNode.Weight && _g.Distance < _bestNode.Distance))
                        {
                            _bestNode = _g;
                        }
                    }
                }

                Timers[8].Start();
                /* low, so reduce list to minimum with dist & weight limit */
                foreach (var _g in MapAsList)
                {
                    /* Something to do */
                    _g.FinalCheck();

                    if (_g.DynamicWeight <= _bestNode.Weight * 0.7)
                        continue;

                    if (_g.Distance > 40f)
                        continue;

                    Timers[9].Start();
                    /* Ray cast */
                    _g.SetNavWeight();
                    Timers[9].Stop();

                    /* Count ray casted points within distance */
                    if (_g.NearbyExitsCount < 0)
                    {
                        _g.NearbyExitsCount = _g.NearbyExitsWithinDistance((float)(_bestNode.Weight * 0.7));

                        if (_g.NearbyExitsCount > 0)
                            _g.OperateUnchangeableWeight(String.Format("HasExits[{0}]", _g.NearbyExitsCount), MainGrid.BaseWeight * _g.NearbyExitsCount);

                        if (_g.NearbyGridPointsCount > 0)
                            _g.OperateUnchangeableWeight(String.Format("CloseToOtherPoints[{0}]", _g.NearbyGridPointsCount), MainGrid.BaseWeight * _g.NearbyGridPointsCount);
                    }
                }
                Timers[8].Stop();
            }

            Timers[0].Stop();
            return true;
        }

        private static int Tick = 0;
        internal static void ResetTickValues()
        {
            if (Tick > 10) { Tick = 0; }
            Tick++;

            /* at every tick */
            GridResults.ResetTickValues();

            tickValue_MinRangeToTarget = -1f;
            isTickRecorded_PlayerShouldKite = false;
            isTickRecorded_ShouldAvoidAoE = false;
            isTickRecorded_ShouldCollectHealthGlobe = false;
            tickValue_HealthGlobeWeightPct = -1f;

            ObjectCacheIsEmpty = !ObjectCache.Any();
            AvoidancesCacheIsEmpty = !CacheData.Avoidances.Any();
            PositionsCacheIsEmpty = !CacheData.VisitedZones.Any();
            UnSafeZonesCacheIsEmpty = !CacheData.UnSafeZones.Any();

            ShouldKiteBosses = CombatBase.KiteMode != KiteMode.Never || Trinity.Player.AvoidDeath;
            ShouldKiteElites = CombatBase.KiteMode == KiteMode.Elites || CombatBase.KiteMode == KiteMode.Always || Trinity.Player.AvoidDeath;
            ShouldKiteTrashs = CombatBase.KiteMode == KiteMode.Always || Trinity.Player.AvoidDeath;
            ShouldFlee = (Trinity.Settings.Combat.Misc.FleeInGhostMode && Trinity.Player.IsGhosted) || Trinity.Player.AvoidDeath;

            ShouldBeAwayFromAoE = Player.IsRanged || (PlayerShouldKite && Player.NeedToKite) || ShouldFlee || ShouldAvoidAoE;

            PlayerIsInTrialRift = Player.LevelAreaId.Equals(DataDictionary.RiftTrialLevelAreaId);

            /* at multiple of 2 */
            if (Tick % 2 == 0)
            {
                Timers[10].Start();
                List<Tuple<int, int>> _itemToRemove = new List<Tuple<int, int>>();

                foreach (var _g in NodesRecorded) { if (_g.Value.ObjectOOR(_g.Value.Position, GridRange + 30)) { _itemToRemove.Add(_g.Key); } }
                foreach (var _i in _itemToRemove) { NodesRecorded.Remove(_i); }
                Timers[10].Stop();

                if (NavZonePosition == new Vector3() ||
                NavZonePosition.Distance2D(Player.Position) >= 5f)
                {
                    NavZonePosition = Player.Position;
                    NavZones.Clear();
                }
            }

            /* at multiple of 3 */
            if (Tick % 3 == 0)
            {
                if (MainGridProvider.Width == 0 || MainGridProvider.Height == 0)
                    GridSegmentation.Reset();
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
