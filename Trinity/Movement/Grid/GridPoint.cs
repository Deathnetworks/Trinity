using System;
using System.Collections.Generic;
using System.Linq;
using Trinity.Combat.Abilities;
using Trinity.Config.Combat;
using Trinity.Reference;
using Zeta.Bot.Navigation;
using Zeta.Common;
using Zeta.Game;

namespace Trinity
{
    class GridNode : IEquatable<GridNode>
    {
        public GridNode(Vector3 _position = new Vector3(), float _weight = 0f)
        {
            Position = MainGrid.VectorToGrid(_position);
        }

        public void ResetTickValues()
        {
            LastTickValue_Distance = -1f;

            /* Infos fields */
            HasAvoidanceRelated = false;
            IsInMonsterRadius = false;

            /* Weight */
            DynamicWeight = 0f;
            DynamicWeightInfos = string.Empty;
            SafeWeightMonsterRelated = 0;

            /* Special weight */
            ClusterWeight = 0f;
            SpecialWeight = 0;
            SpecialCount = 0;

            if (ObjectsLastWeightValues.Any())
            {
                List<int> _itemToRemove = new List<int>();
                foreach (var _o in ObjectsLastWeightValues)
                {
                    _o.Value.IncreaseLoopCount();
                    if (!_o.Value.KeepObject)
                        _itemToRemove.Add(_o.Key);
                }

                foreach (int _i in _itemToRemove)
                {
                    ObjectsLastWeightValues.Remove(_i);
                }
            }
        }

        public void FinalCheck()
        {
        }

        public Vector3 Position { get; set; }
        private float LastTickValue_Distance = -1f;
        public float Distance
        {
            get
            {
                if (LastTickValue_Distance >= 0)
                    return LastTickValue_Distance;

                MainGrid.Timers[11].Start();
                LastTickValue_Distance = this.Position.Distance2D(Trinity.Player.Position);
                MainGrid.Timers[11].Stop();

                return LastTickValue_Distance;
            }
        }

        /* Infos fields */
        public bool HasAvoidanceRelated = false;
        public bool IsInMonsterRadius = false;

        /* Weighting */
        public Dictionary<int, DynamicWeight> ObjectsLastWeightValues = new Dictionary<int, DynamicWeight>();
        public double ClusterWeight = 0;
        public double SpecialWeight = 0;
        public int SpecialCount = 0;

        public double SafeWeightMonsterRelated = 0;
        public double SafeWeight
        {
            get
            {
                return DynamicWeight + UnchangeableWeight + SafeWeightMonsterRelated;
            }
        }
        public double Weight 
        { 
            get 
            {
                if (HasAvoidanceRelated)
                    return DynamicWeight + UnchangeableWeight; 
                return DynamicWeight + UnchangeableWeight + ClusterWeight; 
            } 
        }
        public string WeightInfos 
        { 
            get 
            {
                return DynamicWeightInfos + UnchangeableWeightInfos; 
            } 
        }

        public double DynamicWeight = 0;
        public string DynamicWeightInfos { get; set; }
        public void OperateDynamicWeight(string _weightInfos, float _weight, int _saveAsKey = 0, int _keepDuringLoop = 3, bool _addToCluster = false, bool _addToSafeWeight = false)
        {
            MainGrid.Timers[12].Start();
            if (_weight != 0f)
            {
                if (_addToSafeWeight && !MainGrid.ShouldBeAwayFromAoE)
                    SafeWeightMonsterRelated += _weight;
                else
                {
                    DynamicWeight += _weight;
                    DynamicWeightInfos += " " + _weightInfos + "(" + _weight.ToString("F0") + ")";
                }

                if (_addToCluster) { ClusterWeight += Weight; }
            }

            if (_saveAsKey != 0)
            {
                // try catch fastest to check keys in collection
                try { ObjectsLastWeightValues.Add(_saveAsKey, new DynamicWeight(_weight, _weightInfos, _keepDuringLoop)); }
                catch { }
            }
            MainGrid.Timers[12].Stop(); 
        }

        public double UnchangeableWeight = 0;
        public string UnchangeableWeightInfos { get; set; }
        public void OperateUnchangeableWeight(string weightInfos, float weight)
        {
            MainGrid.Timers[13].Start();
            UnchangeableWeight += weight;
            UnchangeableWeightInfos += " " + weightInfos + "(" + weight.ToString("F0") + ")";
            MainGrid.Timers[13].Stop();
        }

        public int NearbyGridPointsCount = -1;
        public int NearbyExitsCount = -1;
        public int NearbyExitsWithinDistance(float _minWeight = 0f, float _exitRange = 35f)
        {
            using (new Technicals.PerformanceLogger("GridPoint.GridPointsNearExits"))
            {
                int _count = 0;
                int _nodesCount = 0;
                foreach (var _i in MainGrid.MapAsList)
                {
                    if (_count >= 10)
                        break;

                    if (_i.Distance > 35f)
                        continue;

                    if (_i.DynamicWeight <= GridMap.GetBestMoveNode().Weight * 0.7)
                        continue;

                    if (ObjectOOR(_i.Position, _exitRange))
                        continue;

                    if (_i.Position.Distance2D(Position) > _exitRange)
                        continue;

                    if (_i.Position.Distance2D(Position) <= 15f)
                    {
                        _nodesCount++;
                    }

                    if (NavHelper.CanRayCast(Position, _i.Position))
                    {
                        _count++;
                    }
                }

                NearbyGridPointsCount = _nodesCount;
                return _count;
            }
        }

        public bool Equals(GridNode other)
        {
            return Equals(Position.X, other.Position.X) && Equals(Position.Y, other.Position.Y);
        }

        public bool ObjectOOR(Vector3 _loc, float _r)
        {
            return (Math.Max(_loc.X, Position.X) - Math.Min(_loc.X, Position.X)) > _r || (Math.Max(_loc.Y, Position.Y) - Math.Max(_loc.Y, Position.Y)) > _r;
        }

        public void SetUnchangeableWeight()
        {
            /* Nearby recorded points */
            if (NearbyExitsCount > 0)
                OperateUnchangeableWeight(String.Format("HasExits[{0}]", NearbyExitsCount), MainGrid.BaseWeight * NearbyExitsCount);

            /* Nearby recorded exits */
            if (NearbyGridPointsCount > 0)
                OperateUnchangeableWeight(String.Format("CloseToOtherPoints[{0}]", NearbyGridPointsCount), MainGrid.BaseWeight * NearbyGridPointsCount);

            /* Unsafe kite zones (NavHelper.cs)*/
            if (!MainGrid.UnSafeZonesCacheIsEmpty)
            {
                foreach (var _a in CacheData.UnSafeZones)
                {
                    if (ObjectOOR(_a.Key, _a.Value))
                        continue;

                    if (_a.Key.Distance2D(Position) <= _a.Value)
                        OperateUnchangeableWeight("IsInUnsafeKiteAreas", (MainGrid.BaseWeight - _a.Key.Distance2D(Position)) * -5f);
                }
            }

            /* All visited zones*/
            if (!MainGrid.PlayerIsInTrialRift && !MainGrid.PositionsCacheIsEmpty)
            {
                foreach (var _p in CacheData.VisitedZones)
                {
                    if (ObjectOOR(_p.Key, 7f))
                        continue;

                    if (Position.Distance2D(_p.Key) <= 7f)
                        OperateUnchangeableWeight("IsInVisitedZone", (MainGrid.BaseWeight - _p.Key.Distance2D(Position)) * 4f);
                }
            }
        }
        public void SetNavWeight()
        {
            bool isNavigable = false;
            if (Distance < 10f)
                isNavigable = true;
            else if (MainGrid.NavZones.ContainsKey(MainGrid.VectorToGrid(Position)))
                isNavigable = true;
            else if (MainGrid.NavZones.Any(i => i.Key.Distance2D(Position) <= i.Value))
                isNavigable = true;
            else if (NavHelper.CanRayCast(Position))
            {
                isNavigable = true;
                MainGrid.NavZones.Add(MainGrid.VectorToGrid(Position), 5f);
            }

            if (isNavigable)
                OperateDynamicWeight("IsNavigable", (MainGrid.BaseWeight - Distance) * 5f, 1, 3);
        }
        public void SetTargetWeights()
        {
            if (MainGrid.ShouldAvoidAoE)
                return;

            if (!MainGrid.ShouldBeAwayFromAoE)
                return;

            if (Trinity.CurrentTarget == null)
                return;

            if (!Trinity.CurrentTarget.IsUnit)
                return;

            DynamicWeight _w;
            if (ObjectsLastWeightValues.TryGetValue(Trinity.CurrentTarget.RActorGuid + 99999, out _w))
            {
                OperateDynamicWeight(_w.ObjectWeightInfo + "[D]", _w.ObjectWeight);
            }
            else
            {
                float _weight = 0f;
                string _weightInfos = string.Empty;

                int _dstFromObj = (int)Position.Distance2D(Trinity.CurrentTarget.Position) - (int)Trinity.CurrentTarget.Radius;
                if (_dstFromObj <= MainGrid.MinRangeToTarget - 3f)
                {
                    if (Trinity.CurrentTarget.IsInLineOfSightOfPoint(Position))
                    {
                        _weight = MainGrid.ShouldBeAwayFromAoE ? (MainGrid.BaseWeight + _dstFromObj) * 10f : (MainGrid.BaseWeight - _dstFromObj) * 10f;
                        _weightInfos = "IsInLoSOfTarget" + "(" + _weight.ToString("F0") + ")";

                        if (MainGrid.PlayerShouldKite && _dstFromObj > CombatBase.KiteDistance)
                        {
                            _weight = MainGrid.ShouldBeAwayFromAoE ? (MainGrid.BaseWeight + _dstFromObj) * 8f : (MainGrid.BaseWeight - _dstFromObj) * 10f;
                            _weightInfos += " IsInTargetRequiredRangeInKiteRange";
                        }
                        else
                        {
                            _weight = MainGrid.ShouldBeAwayFromAoE ? (MainGrid.BaseWeight + _dstFromObj) * 6f : (MainGrid.BaseWeight - _dstFromObj) * 10f;
                            _weightInfos += " IsInTargetRequiredRange";
                        }
                    }
                }

                OperateDynamicWeight(_weightInfos, _weight, Trinity.CurrentTarget.RActorGuid + 99999);
            }
        }
        public void SetAvoidancesWeights()
        {
            if (MainGrid.AvoidancesCacheIsEmpty)
                return;

            foreach (var _a in CacheData.Avoidances)
            {
                DynamicWeight _w;
                if (ObjectsLastWeightValues.TryGetValue(_a.Key, out _w))
                {
                    HasAvoidanceRelated = true;
                    OperateDynamicWeight(_w.ObjectWeightInfo + "[D]", _w.ObjectWeight);
                }
                else
                {
                    if (ObjectOOR(_a.Value.Item1, 60f))
                        continue;

                    float _weight = 0f;
                    string _weightInfo = string.Empty;
                    float _dstFromObj = Position.Distance2D(_a.Value.Item1);

                    if (_dstFromObj <= _a.Value.Item2)
                    {
                        _weight += (MainGrid.BaseWeight - _dstFromObj + _a.Value.Item2) * -30f;
                        _weightInfo += "IsStandingInAvoidance";
                    }
                    else if (_dstFromObj <= _a.Value.Item2 * 1.2)
                    {
                        _weight += (MainGrid.BaseWeight - _dstFromObj + _a.Value.Item2) * -28f;
                        _weightInfo += "IsCloseToAvoidance";
                    }
                    else if (MathUtil.IntersectsPath(_a.Value.Item1, _a.Value.Item2, Trinity.Player.Position, Position, true, true))
                    {
                        _weight += (MainGrid.BaseWeight - _dstFromObj + _a.Value.Item2) * -26f;
                        _weightInfo += "IsIntersectAvoidanceRadius";
                    }

                    if (_weight > 0)
                        HasAvoidanceRelated = true;

                    OperateDynamicWeight(_weightInfo, _weight, _a.Key);
                }
            }
        }
        public void SetCacheObjectsWeights()
        {
            if (MainGrid.ObjectCacheIsEmpty)
                return;

            foreach (TrinityCacheObject _o in Trinity.ObjectCache)
            {
                if (_o.Distance > 45f)
                    continue;

                if (ObjectOOR(_o.Position, 45f))
                    continue;

                MainGrid.Timers[20].Start();
                DynamicWeight _w;
                if (ObjectsLastWeightValues.TryGetValue(_o.RActorGuid, out _w))
                {
                    if (_w.ObjectWeightInfo.Contains("IsInMonsterRadius"))
                        IsInMonsterRadius = true;

                    bool _isSafeWeight = _w.ObjectWeightInfo.Contains("Monster") || _w.ObjectWeightInfo.Contains("Kite");

                    OperateDynamicWeight(_w.ObjectWeightInfo + "[D]", _w.ObjectWeight, _addToSafeWeight: _isSafeWeight);
                    MainGrid.Timers[20].Stop();
                    continue;
                }
                MainGrid.Timers[20].Stop();

                MainGrid.Timers[19].Start();
                switch (_o.Type)
                {
                    case GObjectType.Unit:
                        #region Unit
                        {
                            int _dstFromObj = (int)Position.Distance2D(_o.Position);
                            if (!MainGrid.ShouldBeAwayFromAoE)
                            {
                                float _clusterWeight = 0f;
                                if (!HasAvoidanceRelated && _dstFromObj <= Trinity.Settings.Combat.Misc.TrashPackClusterRadius && _o.IsTrashPackOrBossEliteRareUnique)
                                {
                                    _clusterWeight = (float)(MainGrid.BaseWeight - (_dstFromObj * 2) + _o.Radius + ((_o.Weight * 100) / 50000)) * _o.NearbyUnits * 10f;
                                    OperateDynamicWeight("Clustering", _clusterWeight, _addToCluster: true);
                                }

                                if (_dstFromObj <= _o.Radius)
                                    IsInMonsterRadius = true;
                            }
                            
                            if (_dstFromObj <= _o.Radius)
                            {
                                IsInMonsterRadius = true;
                                OperateDynamicWeight("IsInMonsterRadius", (MainGrid.BaseWeight - _dstFromObj + _o.Radius) * -13f, _o.RActorGuid, 10, _addToSafeWeight: true);
                            }
                            else if (MainGrid.PlayerShouldKite && !_o.IsTreasureGoblin && _dstFromObj <= CombatBase.KiteDistance)
                            {
                                if (_o.IsBoss && MainGrid.ShouldKiteBosses)
                                {
                                    OperateDynamicWeight("IsInBossKiteRange", (MainGrid.BaseWeight - _dstFromObj + CombatBase.KiteDistance + _o.Radius) * -11f, _o.RActorGuid, 10, _addToSafeWeight: true);
                                }
                                else if (_o.IsBossOrEliteRareUnique && MainGrid.ShouldKiteElites)
                                {
                                    OperateDynamicWeight("IsInEliteKiteRange", (MainGrid.BaseWeight - _dstFromObj + CombatBase.KiteDistance + _o.Radius) * -9f, _o.RActorGuid, 10, _addToSafeWeight: true);
                                }
                                else if (MainGrid.ShouldKiteTrashs)
                                {
                                    OperateDynamicWeight("IsInMobKiteRange", (MainGrid.BaseWeight - _dstFromObj + CombatBase.KiteDistance + _o.Radius) * -7f, _o.RActorGuid, 10, _addToSafeWeight: true);
                                }
                            }
                            else if (_o.Distance < Distance && MathUtil.IntersectsPath(_o.Position, _o.Radius, Trinity.Player.Position, Position, true, true))
                            {
                                OperateDynamicWeight("IsIntersectMonsterRadius", (MainGrid.BaseWeight - _dstFromObj + _o.Radius) * -6f, _o.RActorGuid, 10, _addToSafeWeight: true);
                            }
                            else if (MainGrid.ShouldFlee)
                            {
                                OperateDynamicWeight("AvoidMonster", (MainGrid.BaseWeight + _dstFromObj) * 10f, _o.RActorGuid, 10, _addToSafeWeight: true);
                            }
                            break;
                        }
                        #endregion
                    case GObjectType.HealthWell:
                        #region HealthWell
                        {
                            int _dstFromObj = (int)Position.Distance2D(_o.Position);
                            if (_dstFromObj <= 5f && Trinity.Player.CurrentHealthPct < 0.3)
                            {
                                OperateDynamicWeight("IsInHealthWellRequiredRange", (MainGrid.BaseWeight - Distance) * 4f, _o.RActorGuid, _addToCluster: true);
                            }
                            else if (_o.Distance < Distance && MathUtil.IntersectsPath(_o.Position, 5f, Trinity.Player.Position, Position))
                            {
                                OperateDynamicWeight("IsIntersectHealthWellRequiredRange", (MainGrid.BaseWeight - Distance) * 4f, _o.RActorGuid, _addToCluster: true);
                            }
                            break;
                        }
                        #endregion
                    case GObjectType.HealthGlobe:
                        #region HealthGlobe
                        {
                            int _dstFromObj = (int)Position.Distance2D(_o.Position);
                            if (_dstFromObj <= Trinity.Player.GoldPickupRadius + 2f && MainGrid.ShouldCollectHealthGlobe)
                            {
                                if (Trinity.Settings.Combat.Misc.HiPriorityHG)
                                {
                                    OperateDynamicWeight("IsInHealthGlobePickUpRadius&HighPririty", (float)((MainGrid.BaseWeight - Distance) * MainGrid.HealthGlobeWeightPct * 8f), _o.RActorGuid, _addToCluster: true);
                                }
                                else if (MainGrid.ShouldAvoidAoE)
                                {
                                    OperateDynamicWeight("IsInHealthGlobePickUpRadius&LowHealth", (float)((MainGrid.BaseWeight - Distance) * MainGrid.HealthGlobeWeightPct * 7f), _o.RActorGuid, _addToCluster: true);
                                }
                                else
                                {
                                    OperateDynamicWeight("IsInHealthGlobePickUpRadius", (float)((MainGrid.BaseWeight - Distance) * MainGrid.HealthGlobeWeightPct * 6f), _o.RActorGuid, _addToCluster: true);
                                }
                            }
                            else if (_o.Distance < Distance && MathUtil.IntersectsPath(_o.Position, Trinity.Player.GoldPickupRadius + 2f, Trinity.Player.Position, Position))
                            {
                                if (Trinity.Settings.Combat.Misc.HiPriorityHG)
                                {
                                    OperateDynamicWeight("IsIntersectHealthGlobePickUpRadius&HiPriority", (float)((MainGrid.BaseWeight - Distance) * MainGrid.HealthGlobeWeightPct * 8f), _o.RActorGuid, _addToCluster: true);
                                }
                                else if (MainGrid.ShouldAvoidAoE)
                                {
                                    OperateDynamicWeight("IsIntersectHealthGlobePickUpRadius&LowHealth", (float)((MainGrid.BaseWeight - Distance) * MainGrid.HealthGlobeWeightPct * 7f), _o.RActorGuid, _addToCluster: true);
                                }
                                else
                                {
                                    OperateDynamicWeight("IsIntersectHealthGlobePickUpRadius", (float)((MainGrid.BaseWeight - Distance) * MainGrid.HealthGlobeWeightPct * 6f), _o.RActorGuid, _addToCluster: true);
                                }
                            }
                            break;
                        }
                        #endregion
                    case GObjectType.ProgressionGlobe:
                        #region ProgressionGlobe
                        {
                            if (!MainGrid.ShouldAvoidAoE)
                            {
                                int _dstFromObj = (int)Position.Distance2D(_o.Position);
                                if (_dstFromObj <= Trinity.Player.GoldPickupRadius + 2f)
                                {
                                    OperateDynamicWeight("IsInProgressionGlobePickUpRadius", (MainGrid.BaseWeight - Distance) * 3f, _o.RActorGuid, _addToCluster: true);
                                }
                                else if (_o.Distance < Distance && MathUtil.IntersectsPath(_o.Position, Trinity.Player.GoldPickupRadius + 2f, Trinity.Player.Position, Position))
                                {
                                    OperateDynamicWeight("IsIntersectProgressionGlobePickUpRadius", (MainGrid.BaseWeight - Distance) * 3f, _o.RActorGuid, _addToCluster: true);
                                }
                            }
                            break;
                        }
                        #endregion
                    case GObjectType.PowerGlobe:
                        #region PowerGlobe
                        {
                            if (!MainGrid.ShouldAvoidAoE)
                            {
                                int _dstFromObj = (int)Position.Distance2D(_o.Position);
                                if (_dstFromObj <= Trinity.Player.GoldPickupRadius + 2f)
                                {
                                    OperateDynamicWeight("IsInPowerGlobePickUpRadius", (MainGrid.BaseWeight - Distance) * 2f, _o.RActorGuid, _addToCluster: true);
                                }
                                else if (_o.Distance < Distance && MathUtil.IntersectsPath(_o.Position, Trinity.Player.GoldPickupRadius + 2f, Trinity.Player.Position, Position))
                                {
                                    OperateDynamicWeight("IsIntersectPowerGlobePickUpRadius", (MainGrid.BaseWeight - Distance) * 2f, _o.RActorGuid, _addToCluster: true);
                                } 
                            }
                            break;
                        }
                        #endregion
                    case GObjectType.Gold:
                        #region Gold
                        {
                            if (!MainGrid.ShouldAvoidAoE || (MainGrid.ShouldAvoidAoE && Legendary.Goldwrap.IsEquipped || Legendary.KymbosGold.IsEquipped))
                            {
                                int _dstFromObj = (int)Position.Distance2D(_o.Position);
                                if (_dstFromObj <= Trinity.Player.GoldPickupRadius + 2f)
                                {
                                    if (Legendary.Goldwrap.IsEquipped)
                                    {
                                        OperateDynamicWeight("IsInGoldPickUpRadius&Goldwrap", (MainGrid.BaseWeight - Distance) * 10f, _o.RActorGuid, _addToCluster: true);
                                    }
                                    else if (Legendary.KymbosGold.IsEquipped && Trinity.Player.CurrentHealthPct < 0.8)
                                    {
                                        OperateDynamicWeight("IsInGoldPickUpRadius&KymboGold", (MainGrid.BaseWeight - Distance) * 5f, _o.RActorGuid, _addToCluster: true);
                                    }
                                    else
                                    {
                                        OperateDynamicWeight("IsInGoldPickUpRadius", (MainGrid.BaseWeight - Distance * 2f), _o.RActorGuid, _addToCluster: true);
                                    }
                                }
                                else if (_o.Distance < Distance && MathUtil.IntersectsPath(_o.Position, Trinity.Player.GoldPickupRadius + 2f, Trinity.Player.Position, Position))
                                {
                                    if (Legendary.Goldwrap.IsEquipped)
                                    {
                                        OperateDynamicWeight("IsIntersectGoldPickUpRadius&Goldwrap", (MainGrid.BaseWeight - Distance) * 10f, _o.RActorGuid, _addToCluster: true);
                                    }
                                    else if (Legendary.KymbosGold.IsEquipped && Trinity.Player.CurrentHealthPct < 0.8)
                                    {
                                        OperateDynamicWeight("IsIntersectGoldPickUpRadius&KymboGold", (MainGrid.BaseWeight - Distance) * 5f, _o.RActorGuid, _addToCluster: true);
                                    }
                                    else
                                    {
                                        OperateDynamicWeight("IsIntersectGoldPickUpRadius", (MainGrid.BaseWeight - Distance) * 1.5f, _o.RActorGuid, _addToCluster: true);
                                    }
                                } 
                            }
                            break;
                        }
                        #endregion
                    case GObjectType.Shrine:
                        #region Shrine
                        {
                            int _dstFromObj = (int)Position.Distance2D(_o.Position);
                            if (_dstFromObj <= 5f)
                            {
                                if (Trinity.Settings.WorldObject.HiPriorityShrines)
                                {
                                    OperateDynamicWeight("IsInShrineRequiredRange&HiPriority", (MainGrid.BaseWeight - Distance) * 6f, _o.RActorGuid, _addToCluster: true);
                                }
                                else
                                {
                                    OperateDynamicWeight("IsInShrineRequiredRange", (MainGrid.BaseWeight - Distance) * 3f, _o.RActorGuid, _addToCluster: true);
                                }
                            }
                            else if (_o.Distance < Distance && MathUtil.IntersectsPath(_o.Position, Trinity.Player.GoldPickupRadius + 2f, Trinity.Player.Position, Position))
                            {
                                if (Trinity.Settings.WorldObject.HiPriorityShrines)
                                {
                                    OperateDynamicWeight("IsIntersectShrineRequiredRange&HiPriority", (MainGrid.BaseWeight - Distance) * 6f, _o.RActorGuid, _addToCluster: true);
                                }
                                else
                                {
                                    OperateDynamicWeight("IsIntersectShrineRequiredRange", (MainGrid.BaseWeight - Distance) * 3f, _o.RActorGuid, _addToCluster: true);
                                }
                            }
                            break;
                        }
                        #endregion
                    case GObjectType.Destructible:
                    case GObjectType.Interactable:
                    case GObjectType.Container:
                        #region Container
                        {
                            if (!MainGrid.ShouldAvoidAoE)
                            {
                                int _dstFromObj = (int)Position.Distance2D(_o.Position);
                                if (_dstFromObj <= _o.Radius + 5f && !MainGrid.ShouldAvoidAoE && !_o.IsNavBlocking() &&
                                    (Trinity.Settings.WorldObject.HiPriorityContainers ||
                                    ((Legendary.HarringtonWaistguard.IsEquipped && !Legendary.HarringtonWaistguard.IsBuffActive))))
                                {
                                    OperateDynamicWeight("IsInContainerRequiredRange&HiPriority", (MainGrid.BaseWeight - Distance) * 10f, _o.RActorGuid, _addToCluster: true);
                                }
                                else if (_dstFromObj <= _o.Radius + 5f)
                                {
                                    OperateDynamicWeight("IsInContainerRequiredRange&HiPriority", (MainGrid.BaseWeight - Distance) * -2f, _o.RActorGuid);
                                }
                                else if (_o.Distance < Distance && MathUtil.IntersectsPath(_o.Position, _o.Radius, Trinity.Player.Position, Position, true, true))
                                {
                                    OperateDynamicWeight("IntersectsPathObstacles", (MainGrid.BaseWeight - _dstFromObj + _o.Radius) * -2f, _o.RActorGuid);
                                } 
                            }
                            break;
                        }
                        #endregion
                    default:
                        break;
                }
                MainGrid.Timers[19].Stop();
            }
        }
    }

    class DynamicWeight : IEquatable<DynamicWeight>
    {
        public DynamicWeight(float _weight, string _objectWeightInfo, int _keepDuringLoop = 3)
        {
            ObjectWeight = _weight;
            ObjectWeightInfo = _objectWeightInfo;
            KeepDuringLoop = _keepDuringLoop;
            CurrentLoopCount = 1;
        }

        private int KeepDuringLoop { get; set; }
        private int CurrentLoopCount { get; set; }
        public float ObjectWeight { get; set; }
        public string ObjectWeightInfo { get; set; }
        public bool KeepObject { get { return CurrentLoopCount < KeepDuringLoop; } }
        public void IncreaseLoopCount()
        {
            CurrentLoopCount++;
        }

        public bool Equals(DynamicWeight other)
        {
            return false;
        }
    }
}
