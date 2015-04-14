using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Trinity.Combat;
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
        #endregion

        private static Dictionary<Tuple<Vector3, float, bool>, List<TrinityCacheObject>> ListObjectResults = new Dictionary<Tuple<Vector3, float, bool>, List<TrinityCacheObject>>();
        public static void ResetTickValues()
        {
            ListObjectResults = new Dictionary<Tuple<Vector3, float, bool>, List<TrinityCacheObject>>();
        }

        /// <summary>
        /// Gets the number of units facing player
        /// </summary>
        /// <param name="_range"></param>
        /// <returns></returns>
        internal static int UnitsFacingPlayer(float _range)
        {
            if (Trinity.ObjectCache == null || !Trinity.ObjectCache.Any()) return 0; 
            return ListUnitsInRangeOfPosition(_range: _range).Count(u => u.IsFacingPlayer);
        }

        /// <summary>
        /// Gets the number of units player is facing
        /// </summary>
        /// <param name="_range"></param>
        /// <returns></returns>
        internal static int UnitsPlayerFacing(float _range, float _arcDegrees = 70f)
        {
            if (Trinity.ObjectCache == null || !Trinity.ObjectCache.Any()) return 0; 
            return ListUnitsInRangeOfPosition(_range: _range).Count(u => u.IsPlayerFacing(_arcDegrees));
        }

        /// <summary>
        /// If ignoring elites, checks to see if enough trash trash pack are around
        /// </summary>
        /// <param name="_range"></param>
        /// <returns></returns>
        internal static bool EliteOrTrashInRange(float _range)
        {
            return AnyElitesInRange(_range) || (CombatBase.IgnoringElites && AnyTrashInRange(_range));
        }


        /// <summary>
        /// Checks to make sure there's at least one valid cluster with the minimum monster count
        /// </summary>
        /// <param name="_radius"></param>
        /// <param name="_range"></param>
        /// <param name="_minCount"></param>
        /// <returns></returns>
        internal static bool ClusterExists(float _radius = 15f)
        { return ClusterExists(_radius, 300f, 2); }
        internal static bool ClusterExists(float _radius = 15f, int _minCount = 2)
        { return ClusterExists(_radius, 300f, _minCount); }
        internal static bool ClusterExists(float _radius = 15f, float _range = 90f, int _minCount = 2)
        {
            if (_radius < 5f) { _radius = 5f; }
            if (Trinity.ObjectCache == null || !Trinity.ObjectCache.Any()) return false; 
            return ListUnitsInRangeOfPosition(_range: _range).Any(u => u.NearbyUnitsWithinDistance(_radius) >= _minCount);
        }

        internal static GridNode GetBestPierceMoveTarget(float _range, Vector3 _loc = new Vector3())
        {
            using (new MemorySpy("TargetUtil.GetBestPierceMoveTarget()"))
            {
                if (!MainGrid.Map.Any())
                    return null;

                if (_loc == new Vector3()) _loc = Player.Position;
                bool _atPlayer = _loc.Distance2D(Player.Position) <= 3f;

                HashSet<GridNode> _listResult = new HashSet<GridNode>();
                var _rnd = new Random();

                var _gridResult = (
                    from _o in MainGrid.Map
                    where
                        _o.Distance > 3f &&
                        _o.MonsterWeight >= 0 &&
                        _o.Weight >= 0 &&
                        !_o.HasMonsterRelated &&
                        !_o.HasAvoidanceRelated &&
                        _o.NearbyGridPointsCount > 0 &&
                        (_atPlayer && _o.Distance <= 30f ||
                        !_atPlayer && _o.Position.Distance2D(_loc) <= 30f)
                    orderby
                        _rnd.Next()
                    select _o).ToList();

                foreach (var _g in _gridResult)
                {
                    if (_listResult.Count() > 35) { break; }
                    _listResult.Add(_g);
                }

                if (_listResult.Any())
                {
                    foreach (var _n in _gridResult)
                    {
                        var _target = GetBestPierceTarget(_range, _n.Position);
                        _n.SpecialCount = _target.CountUnitsInFront;
                    }

                    if (_gridResult.Any(o => o.SpecialCount > 0))
                    {
                        _gridResult = (
                            from _o in _gridResult
                            orderby
                                _o.SpecialCount descending
                            select _o).ToList();

                        return _gridResult.FirstOrDefault();
                    }
                }

                if (CurrentTarget != null)
                    return new GridNode(CurrentTarget.Position);

                return null;
            }
        }
        internal static TrinityCacheObject GetBestPierceTarget(float _range, Vector3 _loc = new Vector3())
        {
            if (_loc == new Vector3()) _loc = Player.Position;
            bool _atPlayer = _loc.Distance2D(Player.Position) <= 3f;

            var _list = ListUnitsInRangeOfPosition(_loc, _range, false);
            if (_list == null)
                return default(TrinityCacheObject);

            var _results =
                (from _u in _list
                 where
                    (Trinity.KillMonstersInAoE || !LocOrPathInAoE(_u)) &&
                    _u.IsInLineOfSight
                 orderby
                    _u.CountUnitsInFront descending,
                    _u.NearbyUnitsWithinDistance(8f) descending
                 select _u).ToList();

            if (_results.Any())
                return _results.FirstOrDefault();

            if (CurrentTarget != null)
                return CurrentTarget;

            return GetBestClusterUnit(15f, _range);
        }
        internal static Vector3 GetBestPiercePoint(float _range)
        {
            var result = GetBestPierceObject(_range);
            if (result != default(TrinityCacheObject))
                return result.ClusterPosition(5f);

            return Vector3.Zero;
        }
        internal static TrinityCacheObject GetBestPierceObject(float _range)
        {
            var _list = ListObjectsInRangeOfPosition(_range: _range, _useWeights: false);
            if (_list == null)
                return default(TrinityCacheObject);

            var _results =
                (from _o in _list
                 where
                    _o.IsInLineOfSight &&
                    (Trinity.KillMonstersInAoE || !LocOrPathInAoE(_o))
                 orderby
                    _o.CountUnitsInFront descending,
                    _o.NearbyUnitsWithinDistance(8f) descending
                 select _o).ToList();

            if (_results.Any())
                return _results.FirstOrDefault();

            if (CurrentTarget != null)
                return CurrentTarget;

            return GetBestClusterObject(15f, _range);
        }

        private static Vector3 GetBestAoEMovementPosition()
        {
            GridNode _moveNode = GridMap.GetBestMoveNode(maxRange: 25f);
            if (_moveNode.Position != Vector3.Zero)
                return _moveNode.Position;

            if (HealthGlobeClusterExists(25f) && Player.CurrentHealthPct < CombatBase.EmergencyHealthGlobeLimit)
                return GetBestHealthGlobeClusterPoint(7, 25);

            if (PowerGlobeClusterExists(25f))
                return GetBestPowerGlobeClusterPoint(7, 25);

            if (ClusterExists(25f))
                return GetBestClusterPoint(7, 25);

            if (CurrentTarget != null)
                return CurrentTarget.Position;

            return Trinity.Player.Position;
        }


        internal static Vector3 GetBestHealthGlobeClusterPoint(float _radius = 15f, float _range = 65f, bool _useWeights = true)
        {
            if (_radius < 5f)
                _radius = 5f;

            var _list = ListObjectsInRangeOfPosition(Player.Position, _range, _useWeights);
            if (_list == null)
                return Vector3.Zero;

            var _results = (
                from u in ListObjectsInRangeOfPosition(Player.Position, _range, _useWeights)
                where
                    u.Type == GObjectType.HealthGlobe &&
                    (Trinity.KillMonstersInAoE || !LocOrPathInAoE(u))
                orderby
                    u.NearbyUnitsWithinDistance(_radius),
                    u.Distance descending
                select u).ToList();

            if (_results.Any())
                return _results.FirstOrDefault().ClusterPosition(Player.GoldPickupRadius - 3f);

            return GetBestClusterPoint(_radius, _range, _useWeights: _useWeights);
        }

        internal static Vector3 GetBestPowerGlobeClusterPoint(float _radius = 15f, float _range = 65f, bool _useWeights = true)
        {
            if (_radius < 5f)
                _radius = 5f;

            var _list = ListObjectsInRangeOfPosition(Player.Position, _range, _useWeights);
            if (_list == null)
                return Vector3.Zero;

            var _results = (
                from u in _list
                where
                    u.Type == GObjectType.PowerGlobe &&
                    (Trinity.KillMonstersInAoE || !LocOrPathInAoE(u))
                orderby
                    u.NearbyUnitsWithinDistance(_radius),
                    u.Distance descending
                select u).ToList();

            if (_results.Any())
                return _results.FirstOrDefault().ClusterPosition(Player.GoldPickupRadius - 3f);

            return GetBestClusterPoint(_radius, _range, _useWeights: _useWeights);
        }

        /// <summary>
        /// Checks to see if there is a health globe around to grab
        /// </summary>
        /// <param name="radius"></param>
        /// <returns></returns>
        internal static bool HealthGlobeClusterExists(float _range = 15f, int _size = 2, bool _useWeights = true)
        {
            if (Trinity.ObjectCache == null || !Trinity.ObjectCache.Any()) return false; 
            return ListObjectsInRangeOfPosition(_range: _range, _useWeights: _useWeights).Any(o => o.Type == GObjectType.HealthGlobe && o.NearbyUnits >= _size);
        }

        /// <summary>
        /// Checks to see if there is a power globe around to grab
        /// </summary>
        /// <param name="radius"></param>
        /// <returns></returns>
        internal static bool PowerGlobeClusterExists(float _range = 15f, int _size = 2, bool _useWeights = true)
        {
            if (Trinity.ObjectCache == null || !Trinity.ObjectCache.Any()) return false; 
            return ListObjectsInRangeOfPosition(_range: _range, _useWeights: _useWeights).Any(o => o.Type == GObjectType.PowerGlobe && o.NearbyUnits >= _size);
        }


        internal static Vector3 GetBestClusterUnitPoint(float _radius = 15f, float _range = 65f, int _size = 1, bool _useWeights = true, Vector3 _loc = new Vector3())
        {
            var result = GetBestClusterUnit(_radius, _range, _size, _useWeights, _loc);
            if (result != default(TrinityCacheObject))
                return result.ClusterPosition(_radius - 3f);

            return Vector3.Zero;
        }

        internal static TrinityCacheObject GetBestClusterUnit(float _radius = 15f, float _range = 65f, int _size = 1, bool _useWeights = true, Vector3 _loc = new Vector3())
        {
            if (_radius < 1f) { _radius = 1f; }
            if (_loc == new Vector3()) { _loc = Player.Position; }

            var _list = ListUnitsInRangeOfPosition(_loc, _range, _useWeights);
            if (_list == null)
                return default(TrinityCacheObject);

            var _results = (
                from _u in _list
                where
                    (Trinity.KillMonstersInAoE || !_u.IsStandingInAvoidance) &&
                    (_size <= 1 || _u.IsBossOrEliteRareUnique || _u.NearbyUnitsWithinDistance(_radius) >= _size)
                orderby
                    _u.NearbyUnitsWithinDistance(_radius) descending,
                    _u.Weight descending,
                    _u.HitPointsPct descending
                select _u).ToList();

            if (_results.Any())
                return _results.FirstOrDefault();

            if (CurrentTarget != null && CurrentTarget.IsUnit)
                return CurrentTarget;

            return default(TrinityCacheObject);
        }

        internal static Vector3 GetBestClusterPoint(float _radius = 15f, float _range = 65f, int _size = 1, bool _useWeights = true, Vector3 _loc = new Vector3())
        {
            if (_loc == new Vector3()) { _loc = Player.Position; }

            var result = GetBestClusterObject(_radius, _range, _size, _useWeights, _loc);
            if (result != default(TrinityCacheObject))
                return result.ClusterPosition(_radius - 3f);

            return Vector3.Zero;
        }

        internal static TrinityCacheObject GetBestClusterObject(float _radius = 15f, float _range = 65f, int _size = 1, bool _useWeights = true, Vector3 _loc = new Vector3())
        {
            if (_radius < 1f) { _radius = 1f; }
            if (_loc == new Vector3()) { _loc = Player.Position; }

            var _list = ListUnitsInRangeOfPosition(_loc, _range, _useWeights);
            if (_list == null)
                return default(TrinityCacheObject);

            var _results = (
                from _o in _list
                where
                    (Trinity.KillMonstersInAoE || !_o.IsStandingInAvoidance || !_o.IsUnit) &&
                    (_size <= 1 || _o.IsBossOrEliteRareUnique || _o.NearbyUnitsWithinDistance(_radius) >= _size)
                orderby
                    _o.NearbyUnitsWithinDistance(_radius) descending,
                    _o.Weight descending
                select _o).ToList();

            if (_results.Any())
                return _results.FirstOrDefault();

            if (CurrentTarget != null && CurrentTarget.IsUnit)
                return CurrentTarget;

            return default(TrinityCacheObject);
        }

        /// <summary>
        /// Check unit by type and return if the actual count in range >= minimum count
        /// </summary>
        /// <param name="_loc">default: Player position</param>
        /// <param name="_range">count only units in range</param>
        /// <param name="_minCount">minimum units count</param>
        /// <param name="_useWeights">count only units with positive weight</param>

        /// MOBS - At player location with min count set to one and use weight active
        internal static bool AnyMobsInRange(float _range = 15f)
        { return AnyMobsInRange(_range, 1, true); }
        /// MOBS - At player location with min count set to one
        internal static bool AnyMobsInRange(float _range = 15f, bool _useWeights = true)
        { return AnyMobsInRange(_range, 1, _useWeights); }
        /// MOBS - At player location
        internal static bool AnyMobsInRange(float _range = 15f, int _minCount = 1, bool _useWeights = true)
        { return AnyMobsInRangeOfPosition(Player.Position, _range, _minCount, _useWeights); }
        /// MOBS - At variable location
        internal static bool AnyMobsInRangeOfPosition(Vector3 _loc = new Vector3(), float _range = 15f, int _minCount = 1, bool _useWeights = true)
        {
            if (_minCount < 1) { _minCount = 1; }
            return NumMobsInRangeOfPosition(_loc, _range, _useWeights) >= _minCount;
        }

        /// TRASH - At player location with min count set to one and use weight active
        internal static bool AnyTrashInRange(float _range = 15f)
        { return AnyTrashInRange(_range, 1, true); }
        /// TRASH - At player location with min count set to one
        internal static bool AnyTrashInRange(float _range = 15f, bool _useWeights = true)
        { return AnyTrashInRange(_range, 1, _useWeights); }
        /// TRASH - At player location
        internal static bool AnyTrashInRange(float _range = 15f, int _minCount = 1, bool _useWeights = true)
        { return AnyTrashInRangeOfPosition(Player.Position, _range, _minCount, _useWeights); }
        /// TRASH - At variable location
        internal static bool AnyTrashInRangeOfPosition(Vector3 _loc = new Vector3(), float _range = 15f, int _minCount = 1, bool _useWeights = true)
        {
            if (_minCount < 1) { _minCount = 1; }
            return NumTrashInRangeOfPosition(_loc, _range, _useWeights) >= _minCount;
        }

        /// ELITES - At player location with min count set to one and use weight active
        internal static bool AnyElitesInRange(float _range = 15f)
        { return AnyElitesInRange(_range, 1, true); }
        /// ELITES - At player location with min count set to one
        internal static bool AnyElitesInRange(float _range = 15f, bool _useWeights = true)
        { return AnyElitesInRange(_range, 1, _useWeights); }
        /// ELITES - At player location
        internal static bool AnyElitesInRange(float _range = 15f, int _minCount = 1, bool _useWeights = true)
        { return AnyElitesInRangeOfPosition(Player.Position, _range, _minCount, _useWeights); }
        /// ELITES - At variable location
        internal static bool AnyElitesInRangeOfPosition(Vector3 _loc = new Vector3(), float _range = 15f, int _minCount = 1, bool _useWeights = true)
        {
            if (CombatBase.IgnoringElites)
                return false;

            if (_minCount < 1) { _minCount = 1; }
            if (_loc == new Vector3()) { _loc = Player.Position; }

            return NumElitesInRangeOfPosition(_loc, _range, _useWeights) >= _minCount;
        }

        /// <summary>
        /// Count unit by type or other fields
        /// </summary>
        /// <param name="_loc">default: Player position</param>
        /// <param name="_range">count only units in range</param>
        /// <param name="_minCount">minimum units count</param>
        /// <param name="_useWeights">count only units with positive weight</param>

        /// MOBS
        internal static int NumMobsInRangeOfPosition(Vector3 _loc = new Vector3(), float _range = 10f, bool _useWeights = true)
        { if (Trinity.ObjectCache == null || !Trinity.ObjectCache.Any()) return 0; return ListUnitsInRangeOfPosition(_loc, _range, _useWeights).Count(); }
        /// TRASH
        internal static int NumTrashInRangeOfPosition(Vector3 _loc = new Vector3(), float _range = 10f, bool _useWeights = true)
        { if (Trinity.ObjectCache == null || !Trinity.ObjectCache.Any()) return 0; return ListUnitsInRangeOfPosition(_loc, _range, _useWeights).Count(u => u.IsTrashMob); }
        /// ELITES
        internal static int NumElitesInRangeOfPosition(Vector3 _loc = new Vector3(), float _range = 10f, bool _useWeights = true)
        { if (Trinity.ObjectCache == null || !Trinity.ObjectCache.Any()) return 0; return ListUnitsInRangeOfPosition(_loc, _range, _useWeights).Count(u => u.IsBoss); }
        /// BOSS
        internal static int NumBossInRangeOfPosition(Vector3 _loc = new Vector3(), float _range = 10f, bool _useWeights = true)
        { if (Trinity.ObjectCache == null || !Trinity.ObjectCache.Any()) return 0; return ListUnitsInRangeOfPosition(_loc, _range, _useWeights).Count(u => u.IsBoss); }
        /// IN LoS
        internal static int NumMobsInLosInRangeOfPosition(Vector3 _loc = new Vector3(), float _range = 10f, bool _useWeights = true)
        { if (Trinity.ObjectCache == null || !Trinity.ObjectCache.Any()) return 0; return ListUnitsInRangeOfPosition(_loc, _range, _useWeights).Count(u => u.IsInLineOfSight); }

        /// <summary>
        /// List all objects by type in range of point
        /// </summary>
        /// <param name="_loc">default: Player position</param>
        /// <param name="_range">count only units in range</param>
        /// <param name="_minCount">minimum units count</param>
        /// <param name="_useWeights">count only units with positive weight</param>

        /// UNITS
        internal static List<TrinityCacheObject> ListUnitsInRangeOfPosition(Vector3 _loc = new Vector3(), float _range = 10f, bool _useWeights = true)
        {
            if (_loc == new Vector3()) { _loc = Player.Position; }

            var _list = ListObjectsInRangeOfPosition(_loc, _range, _useWeights);
            if (_list == null)
                return null;

            return (from _u in _list where _u.IsUnit select _u).ToList(); 
        }

        /// ALL OBJECTS
        internal static List<TrinityCacheObject> ListObjectsInRangeOfPosition(Vector3 _loc = new Vector3(), float _range = 10f, bool _useWeights = true)
        {
            if (_range < 5f) { _range = 5f; }
            if (_loc == new Vector3()) { _loc = Player.Position; }
            bool _atPlayer = _loc == Player.Position;

            if (Trinity.ObjectCache == null || !Trinity.ObjectCache.Any())
                return null;

            Tuple<Vector3, float, bool> _source = new Tuple<Vector3, float, bool>(_loc, _range, _useWeights);

            List<TrinityCacheObject> _result; ;
            if (ListObjectResults.TryGetValue(_source, out _result))
            {
                return _result;
            }

            _result = (
            from _u in ObjectCache
            where
                (!_useWeights || _u.Weight > 0) &&
                (_atPlayer && _u.RadiusDistance <= _range ||
                !_atPlayer && _u.Position.Distance2D(_loc) - _u.Radius <= _range)
            select _u).ToList();

            if (_result.Any() && !ListObjectResults.ContainsKey(_source))
                ListObjectResults.Add(_source, _result);

            return _result;
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
            Vector3 myPos = Player.Position;

            if (CurrentTarget != null && NavHelper.CanRayCast(myPos, target))
            {
                target = CurrentTarget.Position;
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
            var minDistance = 12f;
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

            var bestPierceNode = GetBestPierceNode(maxDistance + 10f);
            if (bestPierceNode != null && bestPierceNode.Distance > minDistance)
            {
                Logger.Log(LogCategory.Movement, "Returning ZigZag: BestPierceNode {0} r-dist={1} t-dist={2} p_weight={3}", bestPierceNode.Position, ringDistance, bestPierceNode.Position.Distance2D(Player.Position), bestPierceNode.SpecialWeight);
                return bestPierceNode.Position;
            }

            int eliteCount = ObjectCache.Count(u => u.IsUnit && u.IsBossOrEliteRareUnique);
            bool shouldZigZagElites = ((Trinity.CurrentTarget.IsBossOrEliteRareUnique && eliteCount > 1) || eliteCount == 0);

            if (useTargetBasedZigZag && shouldZigZagElites && !AnyTreasureGoblinsPresent && ObjectCache.Count(o => o.IsUnit) >= minTargets)
            {
                bool attackInAoe = Trinity.KillMonstersInAoE;

                var clusterPoint = TargetUtil.GetBestClusterPoint(ringDistance, ringDistance);
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
                         where u.IsUnit && 
                         u.Distance < maxDistance && 
                         u.Distance >= minDistance
                         select u).ToList();
                }
                else
                {
                    zigZagTargetList =
                        (from u in ObjectCache
                         where u.IsUnit && 
                         u.Distance < maxDistance && 
                         u.Distance >= minDistance &&
                         !LocOrPathInAoE(u)
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
                    if (CombatBase.KiteDistance <= 0 && !intersectsPath)
                        continue;

                    // if we're kiting, lets not actualy run through monsters
                    if (CombatBase.KiteDistance > 0 && CacheData.MonsterObstacles.Any(m => m.Position.Distance(zigZagPoint) <= CombatBase.KiteDistance))
                        continue;

                    // Ignore point if any AoE in this point position
                    if (CacheData.AvoidanceObstacles.Any(m => m.Position.Distance(zigZagPoint) <= m.Radius && Player.CurrentHealthPct <= AvoidanceManager.GetAvoidanceHealthBySNO(m.ActorSNO, 1)))
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
        internal static bool LocOrPathInAoE(Vector3 _loc)
        {
            return CacheData.AvoidanceObstacles.Any(aoe => aoe.Position.Distance2D(_loc) <= aoe.Radius + 2f) ||
                CacheData.AvoidanceObstacles.Any(aoe => MathUtil.IntersectsPath(aoe.Position, aoe.Radius, _loc, Player.Position));
        }
        internal static bool LocOrPathInAoE(TrinityCacheObject _o)
        {
            return _o.IsStandingInAvoidance || PathToObjectIntersectsAoe(_o);
        }


        /// <summary>
        /// Checks to see if the path-line to a unit goes through AoE
        /// </summary>
        /// <param name="_o"></param>
        /// <returns></returns>
        internal static bool PathToObjectIntersectsAoe(TrinityCacheObject _o)
        {
            return CacheData.AvoidanceObstacles.Any(aoe => MathUtil.IntersectsPath(aoe.Position, aoe.Radius, _o.Position, Player.Position));
        }

        /// <summary>
        /// Checks if spell is tracked on any unit within range of specified position
        /// </summary>
        internal static bool IsUnitWithDebuffInRangeOfPosition(float _range, Vector3 _loc, SNOPower _power, int _minCount = 1)
        {
            if (Trinity.ObjectCache == null || !Trinity.ObjectCache.Any()) return false; 
            return ListUnitsInRangeOfPosition(_loc, _range).Count(u => SpellTracker.IsUnitTracked(u.ACDGuid, _power) || u.HasDebuff(_power)) >= _minCount;
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

        internal static bool PercentOfMobsDebuffed(float maxRange = 30f, float minPercent = 0.5f)
        {
            int debuffed = (from u in ObjectCache
                            where u.IsUnit && u.CommonDataIsValid &&
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
                    where u.IsUnit && u.CommonDataIsValid &&
                    u.RadiusDistance <= maxRange &&
                    u.HasDebuff(power)
                    select u).Count();
        }

        internal static int MobsWithDebuff(Vector3 at, SNOPower power, float maxRange = 30f)
        {
            return (from u in ObjectCache
                    where u.IsUnit && u.CommonDataIsValid &&
                    u.Position.Distance2D(at) <= maxRange &&
                    u.HasDebuff(power)
                    select u).Count();
        }

        internal static int MobsWithDebuff(IEnumerable<SNOPower> powers, float maxRange = 30f)
        {
            return (from u in ObjectCache
                    where u.IsUnit && u.CommonDataIsValid &&
                    u.RadiusDistance <= maxRange &&
                    powers.Any(u.HasDebuff)
                    select u).Count();
        }

        internal static int MobsWithDebuff(IEnumerable<SNOPower> powers, IEnumerable<TrinityCacheObject> units)
        {
            return (from u in units
                    where u.IsUnit && u.CommonDataIsValid &&
                    powers.Any(u.HasDebuff)
                    select u).Count();
        }

        internal static int DebuffCount(IEnumerable<SNOPower> powers, float maxRange = 30f)
        {
            return (from u in ObjectCache
                    where u.IsUnit && u.CommonDataIsValid &&
                    u.RadiusDistance <= maxRange &&
                    powers.Any(u.HasDebuff)
                    select powers.Count(u.HasDebuff)
                    ).Sum();
        }

        internal static int DebuffCount(IEnumerable<SNOPower> powers, IEnumerable<TrinityCacheObject> units)
        {
            return (from u in units
                    where u.IsUnit && u.CommonDataIsValid &&
                    powers.Any(u.HasDebuff)
                    select powers.Count(u.HasDebuff)
                    ).Sum();
        }

        // revised
        internal static TrinityCacheObject LowestHealthTarget(float _range, Vector3 _loc = new Vector3())
        {
            var _results = (
                from u in ListUnitsInRangeOfPosition(_loc, _range)
                orderby u.HitPoints ascending
                select u).ToList();

            if (_results.Any())
                return _results.FirstOrDefault();

            if (Trinity.CurrentTarget != null && Trinity.CurrentTarget.IsUnit)
                return Trinity.CurrentTarget;

            return default(TrinityCacheObject);
        }

        // revised
        internal static TrinityCacheObject BestExploadingPalmTarget(float _range, Vector3 _loc = new Vector3())
        {
            var _list = ListUnitsInRangeOfPosition(_loc, _range);
            if (_list == null)
            {
                if (Trinity.CurrentTarget != null && Trinity.CurrentTarget.IsUnit)
                    return Trinity.CurrentTarget;

                return default(TrinityCacheObject);
            }

            var _results = (
                from u in _list
                where
                    u.CommonDataIsValid &&
                    !u.HasDebuff(SNOPower.Monk_ExplodingPalm) &&
                    u.IsInLineOfSight &&
                    u.IsTrashPackOrBossEliteRareUnique
                select u).ToList();

            if (_range <= 15f)
            {
                _results = (
                    from u in _results
                    orderby
                        u.HitPoints,
                        u.UnitsWeightsWithinDistance(16f) descending,
                        u.CountUnitsInFront
                    select u).ToList();
            }
            else
            {
                _results = (
                   from u in _results
                   orderby
                       u.HitPoints,
                       u.UnitsWeightsWithinDistance(16f) descending
                   select u).ToList();
            }

            if (_results.Any())
                return _results.FirstOrDefault();

            if (Trinity.CurrentTarget != null && Trinity.CurrentTarget.IsUnit)
                return Trinity.CurrentTarget;

            return default(TrinityCacheObject);
        }

        // revised
        internal static TrinityCacheObject BestExploadingPalmDebuffedTarget(float _range, Vector3 _loc = new Vector3())
        {
            var _list = ListUnitsInRangeOfPosition(_loc, _range);
            if (_list == null)
            {
                if (Trinity.CurrentTarget != null && Trinity.CurrentTarget.IsUnit)
                    return Trinity.CurrentTarget;

                return default(TrinityCacheObject);
            }

            var _results = (
                from u in _list
                where
                    u.CommonDataIsValid &&
                    u.HasDebuff(SNOPower.Monk_ExplodingPalm) &&
                    u.IsInLineOfSight &&
                    u.IsTrashPackOrBossEliteRareUnique
                orderby
                    u.HitPoints,
                    u.UnitsWeightsWithinDistance(16f) descending,
                    MobsWithDebuff(u.Position, SNOPower.Monk_ExplodingPalm, 12f) descending
                select u).ToList();

            if (_results.Any())
                return _results.FirstOrDefault();

            if (Trinity.CurrentTarget != null && Trinity.CurrentTarget.IsUnit)
                return Trinity.CurrentTarget;

            return default(TrinityCacheObject);
        }

        // revised
        internal static TrinityCacheObject BestTargetWithoutDebuffs(float _range, IEnumerable<SNOPower> _debuffs, Vector3 _loc = new Vector3())
        {
            if (_loc == new Vector3()) { _loc = Player.Position; }

            var _list = ListUnitsInRangeOfPosition(_loc, _range);
            if (_list == null)
            {
                if (Trinity.CurrentTarget != null && Trinity.CurrentTarget.IsUnit)
                    return Trinity.CurrentTarget;

                return default(TrinityCacheObject);
            }

            var _results = (
                from u in _list
                where
                    u.CommonDataIsValid &&
                    !_debuffs.All(u.HasDebuff) &&
                    u.IsInLineOfSight
                orderby
                    u.Weight descending,
                    u.Position.Distance2D(_loc)
                select u).ToList();

            if (_results.Any())
                return _results.FirstOrDefault();

            if (Trinity.CurrentTarget != null && Trinity.CurrentTarget.IsUnit)
                return Trinity.CurrentTarget;

            return default(TrinityCacheObject);
        }

        // Revised
        internal static TrinityCacheObject GetClosestTarget(float _range, Vector3 _loc = new Vector3(), bool _useWeights = true)
        {
            if (_loc == new Vector3()) { _loc = Player.Position; }

            var _list = ListUnitsInRangeOfPosition(_loc, _range, _useWeights);
            if (_list == null)
            {
                if (Trinity.CurrentTarget != null && Trinity.CurrentTarget.IsUnit)
                    return Trinity.CurrentTarget;

                return default(TrinityCacheObject);
            }

            var _results = (
                from u in _list
                orderby
                    u.Position.Distance2D(_loc) - u.Radius
                select u).ToList();

            if (_results.Any())
                return _results.FirstOrDefault();

            if (Trinity.CurrentTarget != null && Trinity.CurrentTarget.IsUnit)
                return Trinity.CurrentTarget;

            return default(TrinityCacheObject);
        }

        // Revised
        internal static TrinityCacheObject GetClosestDestructible(float _range, Vector3 _loc = new Vector3(), bool _useWeights = true)
        {
            if (_loc == new Vector3()) { _loc = Player.Position; }

            var _list = ListObjectsInRangeOfPosition(_loc, _range, _useWeights);
            if (_list == null)
            {
                if (Trinity.CurrentTarget != null && Trinity.CurrentTarget.IsUnit)
                    return Trinity.CurrentTarget;

                return default(TrinityCacheObject);
            }

            var _results = (
                from u in _list
                where
                    u.Type == GObjectType.Barricade ||
                    u.Type == GObjectType.Destructible ||
                    u.Type == GObjectType.Door ||
                    u.Type == GObjectType.Container
                orderby
                    u.Position.Distance2D(_loc) - u.Radius
                select u).ToList();

            if (_results.Any())
                return _results.FirstOrDefault();

            if (Trinity.CurrentTarget != null && Trinity.CurrentTarget.IsUnit)
                return Trinity.CurrentTarget;

            return default(TrinityCacheObject);
        }

        // Revised
        internal static TrinityCacheObject GetDashStrikeFarthestTarget(float _maxRange, float _minRange = 33f)
        {
            var _list = ListUnitsInRangeOfPosition(Player.Position, _maxRange);
            if (_list == null)
            {
                if (Trinity.CurrentTarget != null && Trinity.CurrentTarget.IsUnit)
                    return Trinity.CurrentTarget;

                return default(TrinityCacheObject);
            }

            var _results = (
                from u in _list
                where
                    u.Distance >= _minRange &&
                    u.Weight > 0 &&
                    u.IsInLineOfSight &&
                    u.IsTrashPackOrBossEliteRareUnique
                orderby
                    u.UnitsWeightsWithinDistance(16f) descending
                select u).ToList();

            if (_results.Any())
                return _results.FirstOrDefault();

            if (Trinity.CurrentTarget != null && Trinity.CurrentTarget.IsUnit)
                return Trinity.CurrentTarget;

            return default(TrinityCacheObject);
        }

        // Revised
        internal static TrinityCacheObject GetDashStrikeThousandStormTarget(float _maxRange, float _minRange = 33f)
        {
            var _list = ListUnitsInRangeOfPosition(Player.Position, _maxRange);
            if (_list == null)
            {
                if (Trinity.CurrentTarget != null && Trinity.CurrentTarget.IsUnit)
                    return Trinity.CurrentTarget;

                return default(TrinityCacheObject);
            }

            var _results = (
                from u in _list
                where
                    u.Distance >= _minRange &&
                    u.IsInLineOfSight &&
                    u.IsTrashPackOrBossEliteRareUnique
                orderby
                    u.CountUnitsInFront descending,
                    u.UnitsWeightsWithinDistance(16f) descending
                select u).ToList();

            if (!_results.Any())
            {
                _results = (
                    from u in _list
                    where
                        u.IsInLineOfSight &&
                        u.IsTrashPackOrBossEliteRareUnique
                    orderby
                        u.CountUnitsInFront descending,
                        u.UnitsWeightsWithinDistance(16f) descending
                    select u).ToList();
            }

            if (_results.Any())
                return _results.FirstOrDefault();

            if (Trinity.CurrentTarget != null && Trinity.CurrentTarget.IsUnit)
                return Trinity.CurrentTarget;

            return default(TrinityCacheObject);
        }

        // new 03 2015
        internal static List<GridNode> GetNodesCircleAroudPosition(Vector3 _loc = new Vector3(), float _radius = 80f, float _arcDegree = 18f)
        {
            using (new MemorySpy("TargetUtil.GetNodesCircleAroudPosition()"))
            {
                if (_loc == new Vector3()) _loc = Player.Position;
                bool _atPlayer = _loc.Distance2D(Player.Position) <= 3f;

                List<GridNode> _result = new List<GridNode>();
                for (float _alpha = 0f; _alpha <= 360f; _alpha = _alpha + _arcDegree)
                {
                    Vector3 _projPoint = MathEx.GetPointAt(_loc, _radius, (float)MathUtil.DegreeToRadian(_alpha));
                    _result.Add(new GridNode(_projPoint));
                }

                if (_result.Any())
                    return _result;

                return null;
            }
        }

        // new 03 2015
        internal static GridNode GetBestPierceNode(float _range, Vector3 _loc = new Vector3())
        {
            using (new MemorySpy("TargetUtil.GetBestPierceChargeNode()"))
            {
                if (_loc == new Vector3()) _loc = Player.Position;
                bool _atPlayer = _loc.Distance2D(Player.Position) <= 2f;

                var _list = ListObjectsInRangeOfPosition(_range: _range + 10f, _useWeights: false);
                if (_list == null)
                    return null;

                List<GridNode> _nodes = GetNodesCircleAroudPosition(_loc, _range);
                if (_nodes != null)
                {
                    foreach (var _n in _nodes)
                    {
                        string _dir = MathUtil.GetHeadingToPoint(_loc, _n.Position);
                        bool _hasMob = false;
                        float farDistance = 0f;
                        Vector3 nodePosition = _n.Position;
                        bool isLastUnit = true;

                        foreach (var _o in _list.OrderByDescending(o => o.Distance))
                        {
                            if ((Skills.Barbarian.FuriousCharge.IsActive && _o.Type == GObjectType.Destructible) || _o.IsUnit)
                            {
                                if (_o.IsUnit && (!_o.CommonDataIsValid || _o.HitPointsPct <= 0f))
                                    continue;

                                if (!_dir.Equals(MathUtil.GetHeadingToPoint(_loc, _o.Position)))
                                    continue;

                                float _radius = Math.Min(Math.Max(_o.Radius, 5f), 8f);
                                if (_o.IsInLineOfSight)
                                {
                                    if (MathUtil.IntersectsPath(_o.Position, _radius, _loc, nodePosition))
                                    {
                                        if (_o.IsUnit) _hasMob = true;

                                        float dist = _o.Position.Distance2D(_loc);
                                        if (dist > farDistance)
                                        {
                                            farDistance = dist;
                                            _n.Position = MathEx.CalculatePointFrom(_loc, _o.Position, -5f);
                                        }

                                        Vector3 _lineProj = MathEx.CalculatePointFrom(nodePosition, _loc, dist);
                                        _n.SpecialWeight += (_radius - _lineProj.Distance2D(_o.Position)) * Math.Max(_o.Weight, 1000f);

                                        if (_o.IsBoss || (_o.IsTreasureGoblin && Trinity.Settings.Combat.Misc.GoblinPriority == GoblinPriority.Kamikaze))
                                            _n.SpecialCount += 3;
                                        else if (_o.IsEliteRareUnique || _o.Type == GObjectType.Destructible || (_o.IsTreasureGoblin && Trinity.Settings.Combat.Misc.GoblinPriority == GoblinPriority.Prioritize))
                                            _n.SpecialCount += 2;
                                        else
                                            _n.SpecialCount++;

                                        if (TownRun.IsTryingToTownPortal() || Trinity.Player.StandingInAvoidance)
                                            _n.SpecialCount++;

                                        if (Skills.Monk.InnerSanctuary.IsActive && _o.HasDebuff(SNOPower.X1_Monk_InnerSanctuary))
                                            _n.SpecialCount++;

                                        if (CombatBase.IsBaneOfTrappedEquipped && Skills.Monk.BlindingFlash.IsActive && _o.HasDebuff(SNOPower.Monk_BlindingFlash))
                                            _n.SpecialCount++;

                                        if (isLastUnit && _o.IsUnit)
                                        {
                                            isLastUnit = false;
                                            if ((!Trinity.KillMonstersInAoE || Trinity.Settings.Combat.Misc.AvoidAOE) && _o.IsStandingInAvoidance)
                                            {
                                                _n.SpecialCount--;
                                            }
                                        }

                                        if (!_atPlayer && dist <= 10)
                                        {
                                            if (_o.IsBoss)
                                                _n.SpecialCount -= 3;
                                            else if (_o.IsEliteRareUnique)
                                                _n.SpecialCount -= 2;
                                        }  
                                    }
                                }
                            }

                            if (Skills.Barbarian.FuriousCharge.Charges > 1)
                                _n.SpecialCount++;

                            _n.SpecialWeight *= _n.SpecialCount;
                            if (!_hasMob) { _n.SpecialWeight = 0; }
                        }
                    }

                    if (_nodes.Any(n => n.SpecialWeight > 0))
                        return _nodes.OrderByDescending(n => n.SpecialWeight).FirstOrDefault();
                }

                return null;
            }
        }

        // new 03 2015
        internal static GridNode GetBestPierceMoveNode(float _range, Vector3 _loc = new Vector3())
        {
            using (new MemorySpy("TargetUtil.GetBestFuriousChargeMoveNode()"))
            {
                if (!MainGrid.Map.Any())
                    return null;

                if (_loc == new Vector3()) _loc = Player.Position;
                bool _atPlayer = _loc.Distance2D(Player.Position) <= 3f;

                HashSet<GridNode> _listResult = new HashSet<GridNode>();
                var _rnd = new Random();

                var _gridResult = (
                    from _o in MainGrid.Map
                    where
                        _o.Distance > 3f &&
                        _o.MonsterWeight >= 0 &&
                        _o.Weight >= 0 &&
                        !_o.HasMonsterRelated &&
                        !_o.HasAvoidanceRelated &&
                        _o.NearbyGridPointsCount > 0 &&
                        (_atPlayer && _o.Distance <= 30f ||
                        !_atPlayer && _o.Position.Distance2D(_loc) <= 30f)
                    orderby
                        _rnd.Next()
                    select _o).ToList();

                foreach (var _g in _gridResult)
                {
                    if (_listResult.Count() > 35) { break; }
                    _listResult.Add(_g);
                }

                if (_listResult.Any())
                {
                    foreach (var _n in _gridResult)
                    {
                        var _node = GetBestPierceNode(_range, _n.Position);
                        if (_node != null)
                        {
                            _n.SpecialWeight = _node.SpecialWeight;
                            _n.SpecialCount = _node.SpecialCount;
                        }
                    }

                    if (_gridResult.Any(o => o.SpecialWeight > 0))
                    {
                        _gridResult = (
                            from _o in _gridResult
                            orderby
                                _o.SpecialWeight descending
                            select _o).ToList();

                        return _gridResult.FirstOrDefault();
                    }
                }

                if (CurrentTarget != null)
                    return new GridNode(CurrentTarget.Position);

                return null;
            }
        }
    }
}
