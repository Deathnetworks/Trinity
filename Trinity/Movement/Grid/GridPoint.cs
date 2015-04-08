using System;
using System.Collections.Generic;
using System.Linq;
using Trinity.Combat.Abilities;
using Trinity.Config.Combat;
using Trinity.Reference;
using Trinity.Technicals;
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
            HasMonsterRelated = false;

            /* Weight */
            DynamicWeight = 0f;
            DynamicWeightInfos = string.Empty;

            /* Special weight */
            ClusterWeight = 0;
            ClusterWeightInfos = string.Empty;

            MonsterWeight = 0;
            MonsterWeightInfos = string.Empty;

            TargetWeight = 0;
            TargetWeightInfos = string.Empty;

            SpecialWeight = 0;
            SpecialCount = 0;
        }

        public void FinalCheck()
        {
            // Some stuff
        }

        public Vector3 Position { get; set; }
        private float LastTickValue_Distance = -1f;
        public float Distance
        {
            get
            {
                if (LastTickValue_Distance >= 0)
                    return LastTickValue_Distance;

                LastTickValue_Distance = this.Position.Distance2D(Trinity.Player.Position);
                return LastTickValue_Distance;
            }
        }

        /* Infos fields */
        public bool HasAvoidanceRelated = false;
        public bool HasMonsterRelated = false;

        /* Weighting */
        public Dictionary<int, DynamicWeight> LastDynamicWeightValues = new Dictionary<int, DynamicWeight>();
        public Dictionary<int, DynamicWeight> LastClusterWeightValues = new Dictionary<int, DynamicWeight>();
        public Dictionary<int, DynamicWeight> LastMonsterWeightValues = new Dictionary<int, DynamicWeight>();
        public Dictionary<int, DynamicWeight> LastTargetWeightValues = new Dictionary<int, DynamicWeight>();

        public double DynamicWeight = 0;
        public string DynamicWeightInfos = string.Empty;

        public double UnchangeableWeight = 0;
        public string UnchangeableWeightInfos = string.Empty;

        /* Does not set by grid generationcan be used for other thing */
        public double SpecialWeight = 0;
        public int SpecialCount = 0;

        public double ClusterWeight = 0;
        public string ClusterWeightInfos = string.Empty;

        public double MonsterWeight = 0;
        public string MonsterWeightInfos = string.Empty;

        public double TargetWeight = 0;
        public string TargetWeightInfos = string.Empty;

        public double Weight 
        { 
            get 
            {
                using (new MemorySpy("GridNode.Weight"))
                {
                    if (Distance >= MainGrid.GridRange)
                        return UnchangeableWeight;

                    double _w = DynamicWeight + UnchangeableWeight;
                    if (!HasAvoidanceRelated && !Trinity.Player.NeedToKite) _w += ClusterWeight;
                    if (Trinity.Player.NeedToKite || MainGrid.ShouldBeAwayFromAoE) _w += MonsterWeight;
                    if (!Trinity.Player.NeedToKite && !MainGrid.ShouldAvoidAoE) _w += TargetWeight;

                    return _w;
                }
            } 
        }
        public string WeightInfos 
        { 
            get 
            {
                using (new MemorySpy("GridNode.WeightInfos"))
                {
                    if (Distance >= MainGrid.GridRange)
                        return UnchangeableWeightInfos;

                    string _wi = DynamicWeightInfos + UnchangeableWeightInfos;
                    if (!HasAvoidanceRelated && !Trinity.Player.NeedToKite) _wi += ClusterWeightInfos;
                    if (Trinity.Player.NeedToKite || MainGrid.ShouldBeAwayFromAoE) _wi += MonsterWeightInfos;
                    if (!Trinity.Player.NeedToKite && !MainGrid.ShouldAvoidAoE) _wi += TargetWeightInfos;

                    return _wi;
                }
            } 
        }

        public void OperateWeight(WeightType _type, string _weightInfos, float _weight, int _saveAsKey = 0, int _keepDuringLoop = 5)
        {
            using (new MemorySpy("GridNode.OperateWeight()"))
            {
                switch (_type)
                {
                    case WeightType.Target:
                        {
                            if (_weight != 0)
                            {
                                TargetWeight += _weight;
                                TargetWeightInfos += " " + _weightInfos + "(" + _weight.ToString("F0") + ")";
                            }

                            if (_saveAsKey != 0 && !LastTargetWeightValues.ContainsKey(_saveAsKey))
                                LastTargetWeightValues.Add(_saveAsKey, new DynamicWeight(_weight, _weightInfos, _keepDuringLoop));
                        }
                        break;
                    case WeightType.Cluster:
                        {
                            if (_weight != 0)
                            {
                                ClusterWeight += _weight;
                                ClusterWeightInfos += " " + _weightInfos + "(" + _weight.ToString("F0") + ")"; 
                            }

                            if (_saveAsKey != 0 && !LastClusterWeightValues.ContainsKey(_saveAsKey))
                                LastClusterWeightValues.Add(_saveAsKey, new DynamicWeight(_weight, _weightInfos, _keepDuringLoop));
                        } 
                        break;
                    case WeightType.Monster:
                        {
                            if (_weight != 0)
                            {
                                MonsterWeight += _weight;
                                MonsterWeightInfos += " " + _weightInfos + "(" + _weight.ToString("F0") + ")"; 
                            }

                            if (_saveAsKey != 0 && !LastMonsterWeightValues.ContainsKey(_saveAsKey))
                                LastMonsterWeightValues.Add(_saveAsKey, new DynamicWeight(_weight, _weightInfos, _keepDuringLoop));
                        } 
                        break;
                    case WeightType.Unchangeable:
                        {
                            if (_weight != 0)
                            {
                                UnchangeableWeight += _weight;
                                UnchangeableWeightInfos += " " + _weightInfos + "(" + _weight.ToString("F0") + ")";
                            }
                        }
                        break;
                    case WeightType.Dynamic:
                        {
                            if (_weight != 0)
                            {
                                DynamicWeight += _weight;
                                DynamicWeightInfos += " " + _weightInfos + "(" + _weight.ToString("F0") + ")"; 
                            }

                            if (_saveAsKey != 0 && !LastDynamicWeightValues.ContainsKey(_saveAsKey))
                                LastDynamicWeightValues.Add(_saveAsKey, new DynamicWeight(_weight, _weightInfos, _keepDuringLoop));
                        }
                        break;
                    default:
                        break;
                }              
            }
        }

        public int NearbyGridPointsCount = -1;
        public int NearbyExitsCount = -1;
        public int NearbyExitsWithinDistance(float _minWeight = 0f, float _exitRange = 35f)
        {
            using (new MemorySpy("GridNode.NearbyExitsWithinDistance()"))
            {
                int _count = 0;
                int _nodesCount = 0;
                foreach (var _i in MainGrid.MapAsList)
                {
                    if (_count >= 10)
                        break;

                    if (_i.Distance > 35f)
                        continue;

                    if (_i.Weight <= _minWeight)
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
                OperateWeight(WeightType.Unchangeable, String.Format("HasExits[{0}]", NearbyExitsCount), MainGrid.BaseWeight * NearbyExitsCount);

            /* Nearby recorded exits */
            if (NearbyGridPointsCount > 0)
                OperateWeight(WeightType.Unchangeable, String.Format("CloseToOtherPoints[{0}]", NearbyGridPointsCount), MainGrid.BaseWeight * NearbyGridPointsCount);

            /* Unsafe kite zones (NavHelper.cs)*/
            if (!MainGrid.UnSafeZonesCacheIsEmpty && MainGrid.ShouldBeAwayFromAoE)
            {
                foreach (var _a in CacheData.UnSafeZones)
                {
                    if (ObjectOOR(_a.Key, _a.Value))
                        continue;

                    if (_a.Key.Distance2D(Position) <= _a.Value)
                        OperateWeight(WeightType.Unchangeable, "IsInUnsafeKiteAreas", (MainGrid.BaseWeight - _a.Key.Distance2D(Position)) * -5f);
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
                        OperateWeight(WeightType.Unchangeable, "IsInVisitedZone", (MainGrid.BaseWeight - _p.Key.Distance2D(Position)) * 4f);
                }
            }
        }
        public void SetNavWeight()
        {
            DynamicWeight _w;
            bool isNavigable = false;

            if (Distance < 10f)
                isNavigable = true;
            else if (MainGrid.NavZones.ContainsKey(MainGrid.VectorToGrid(Position)))
                isNavigable = true;
            else if (LastDynamicWeightValues.TryGetValue(1, out _w))
            {
                _w.IncreaseLoopCount();
                if (_w.KeepObject)
                {
                    OperateWeight(WeightType.Dynamic, _w.ObjectWeightInfo + "[D]", _w.ObjectWeight);
                    return;
                }

                LastDynamicWeightValues.Remove(1);
            }
            else if (MainGrid.NavZones.Any(i => i.Key.Distance2D(Position) <= i.Value))
                isNavigable = true;
            else if (NavHelper.CanRayCast(Position))
            {
                isNavigable = true;
                MainGrid.NavZones.Add(MainGrid.VectorToGrid(Position), 5f);
            }

            if (isNavigable)
                OperateWeight(WeightType.Dynamic, "IsNavigable", (MainGrid.BaseWeight - Distance) * 5f, 1, 3);
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
            if (LastTargetWeightValues.TryGetValue(Trinity.CurrentTarget.RActorGuid + 99999, out _w))
            {
                _w.IncreaseLoopCount();
                if (_w.KeepObject)
                {
                    OperateWeight(WeightType.Target, _w.ObjectWeightInfo + "[D]", _w.ObjectWeight);
                    return;
                }

                LastTargetWeightValues.Remove(Trinity.CurrentTarget.RActorGuid + 99999);
            }

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

            OperateWeight(WeightType.Target, _weightInfos, _weight, Trinity.CurrentTarget.RActorGuid + 99999, 3);
        }
        public void SetAvoidancesWeights()
        {
            if (MainGrid.AvoidancesCacheIsEmpty)
                return;

            foreach (var _a in CacheData.AvoidanceObstacles)
            {
                int _key = _a.IsAvoidanceAnimations ? (int)_a.Animation : _a.RActorGUID;
                _key += (int)_a.Position.X + (int)_a.Position.Y;

                DynamicWeight _w;
                if (LastDynamicWeightValues.TryGetValue(_key, out _w))
                {
                    _w.IncreaseLoopCount();
                    if (_w.KeepObject)
                    {
                        if (_w.ObjectWeight < 0)
                            HasAvoidanceRelated = true;

                        OperateWeight(WeightType.Dynamic, _w.ObjectWeightInfo + "[D]", _w.ObjectWeight);
                        continue;
                    }

                    LastDynamicWeightValues.Remove(_key);
                }

                if (ObjectOOR(_a.Position, 60f))
                    continue;

                float _weight = 0f;
                string _weightInfo = string.Empty;
                float _dstFromObj = Position.Distance2D(_a.Position);

                if (_dstFromObj <= _a.Radius)
                {
                    _weight += (MainGrid.BaseWeight - _dstFromObj + _a.Radius) * -30f;
                    _weightInfo += "IsStandingInAvoidance";
                }
                else if (_a.AvoidType != AvoidType.Projectile)
                {
                    if (_dstFromObj <= _a.Radius * 1.2)
                    {
                        _weight += (MainGrid.BaseWeight - _dstFromObj + _a.Radius) * -28f;
                        _weightInfo += "IsCloseToAvoidance";
                    }
                    else
                    {
                        using (new MemorySpy("GridNode.SetAvoidancesWeights().GetIntersect"))
                        {
                            if (Trinity.Player.Position.Distance2D(_a.Position) < Distance &&
                            MathUtil.IntersectsPath(_a.Position, _a.Radius + 2f, Trinity.Player.Position, Position, true, true))
                            {
                                _weight += (MainGrid.BaseWeight - _dstFromObj + _a.Radius) * -26f;
                                _weightInfo += "IsIntersectAvoidanceRadius";
                            }
                        }
                    }
                }

                if (_weight < 0)
                    HasAvoidanceRelated = true;

                OperateWeight(WeightType.Dynamic, _weightInfo, _weight, _key, 3);
            }
        }
        public void SetCacheObjectsWeights()
        {
            if (MainGrid.ObjectCacheIsEmpty)
                return;

            foreach (TrinityCacheObject _o in Trinity.ObjectCache)
            {
                if (_o.Distance > 55f)
                    continue;

                if (ObjectOOR(_o.Position, 55f))
                    continue;

                DynamicWeight _w;
                if (LastDynamicWeightValues.TryGetValue(_o.RActorGuid, out _w))
                {
                    _w.IncreaseLoopCount();
                    if (_w.KeepObject)
                    {
                        OperateWeight(WeightType.Dynamic, _w.ObjectWeightInfo + "[D]", _w.ObjectWeight);
                        continue;
                    }

                    LastDynamicWeightValues.Remove(_o.RActorGuid);
                }

                switch (_o.Type)
                {
                    case GObjectType.Unit:
                        #region Unit
                        {
                            if (LastMonsterWeightValues.TryGetValue(_o.RActorGuid, out _w))
                            {
                                _w.IncreaseLoopCount();
                                if (_w.KeepObject)
                                {
                                    if (_w.ObjectWeight < 0)
                                        HasMonsterRelated = true;

                                    OperateWeight(WeightType.Monster, _w.ObjectWeightInfo + "[D]", _w.ObjectWeight);
                                    continue;
                                }

                                LastMonsterWeightValues.Remove(_o.RActorGuid);
                            }

                            int _dstFromObj = (int)Position.Distance2D(_o.Position);

                            if (!MainGrid.ShouldBeAwayFromAoE)
                            {
                                if (LastClusterWeightValues.TryGetValue(_o.RActorGuid + (int)_o.Position.X + (int)_o.Position.Y, out _w))
                                {
                                    _w.IncreaseLoopCount();
                                    if (_w.KeepObject)
                                    {
                                        if (_dstFromObj <= _o.Radius)
                                            HasMonsterRelated = true;

                                        OperateWeight(WeightType.Cluster, _w.ObjectWeightInfo + "[D]", _w.ObjectWeight);
                                        continue;
                                    }

                                    LastClusterWeightValues.Remove(_o.RActorGuid + (int)_o.Position.X + (int)_o.Position.Y);
                                }

                                float _clusterWeight = 0f;
                                if (!HasAvoidanceRelated && _dstFromObj <= Trinity.Settings.Combat.Misc.TrashPackClusterRadius && _o.IsTrashPackOrBossEliteRareUnique)
                                {
                                    _clusterWeight = (float)(((Trinity.Settings.Combat.Misc.TrashPackClusterRadius - _dstFromObj) * Trinity.Settings.Combat.Misc.TrashPackClusterRadius) + _o.Radius + ((_o.Weight * 100) / 50000)) * _o.NearbyUnits * 10f;
                                    OperateWeight(WeightType.Cluster, "Clustering", _clusterWeight, _o.RActorGuid + (int)_o.Position.X + (int)_o.Position.Y, 3);
                                }

                                if (_dstFromObj <= _o.Radius)
                                    HasMonsterRelated = true;
                            }
                            else if (_dstFromObj <= _o.Radius)
                            {
                                HasMonsterRelated = true;
                                OperateWeight(WeightType.Monster, "IsInMonsterRadius", (MainGrid.BaseWeight - _dstFromObj + _o.Radius) * -13f, _o.RActorGuid, 3);
                            }
                            else if (MainGrid.PlayerShouldKite && !_o.IsTreasureGoblin && _dstFromObj <= CombatBase.KiteDistance)
                            {
                                if (_o.IsBoss && MainGrid.ShouldKiteBosses)
                                {
                                    OperateWeight(WeightType.Monster, "IsInBossKiteRange", (MainGrid.BaseWeight - _dstFromObj + CombatBase.KiteDistance + _o.Radius) * -11f, _o.RActorGuid, 3);
                                }
                                else if (_o.IsBossOrEliteRareUnique && MainGrid.ShouldKiteElites)
                                {
                                    OperateWeight(WeightType.Monster, "IsInEliteKiteRange", (MainGrid.BaseWeight - _dstFromObj + CombatBase.KiteDistance + _o.Radius) * -9f, _o.RActorGuid, 3);
                                }
                                else if (MainGrid.ShouldKiteTrashs)
                                {
                                    OperateWeight(WeightType.Monster, "IsInMobKiteRange", (MainGrid.BaseWeight - _dstFromObj + CombatBase.KiteDistance + _o.Radius) * -7f, _o.RActorGuid, 3);
                                }
                            }
                            else if (_o.Distance < Distance && MathUtil.IntersectsPath(_o.Position, _o.Radius + 2f, Trinity.Player.Position, Position))
                            {
                                OperateWeight(WeightType.Monster, "IsIntersectMonsterRadius", (MainGrid.BaseWeight - _dstFromObj + _o.Radius) * -6f, _o.RActorGuid, 3);
                            }
                            else if (MainGrid.ShouldFlee)
                            {
                                OperateWeight(WeightType.Monster, "AvoidMonster", (MainGrid.BaseWeight + _dstFromObj) * 10f, _o.RActorGuid, 3);
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
                                OperateWeight(WeightType.Dynamic, "IsInHealthWellRequiredRange", (MainGrid.BaseWeight - Distance) * 4f, _o.RActorGuid);
                            }
                            else if (_o.Distance < Distance && MathUtil.IntersectsPath(_o.Position, 5f, Trinity.Player.Position, Position))
                            {
                                OperateWeight(WeightType.Dynamic, "IsIntersectHealthWellRequiredRange", (MainGrid.BaseWeight - Distance) * 4f, _o.RActorGuid);
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
                                    OperateWeight(WeightType.Dynamic, "IsInHealthGlobePickUpRadius&HighPririty", (float)((MainGrid.BaseWeight - Distance) * MainGrid.HealthGlobeWeightPct * 8f), _o.RActorGuid);
                                }
                                else if (MainGrid.ShouldAvoidAoE)
                                {
                                    OperateWeight(WeightType.Dynamic, "IsInHealthGlobePickUpRadius&LowHealth", (float)((MainGrid.BaseWeight - Distance) * MainGrid.HealthGlobeWeightPct * 7f), _o.RActorGuid);
                                }
                                else
                                {
                                    OperateWeight(WeightType.Dynamic, "IsInHealthGlobePickUpRadius", (float)((MainGrid.BaseWeight - Distance) * MainGrid.HealthGlobeWeightPct * 6f), _o.RActorGuid);
                                }
                            }
                            else if (_o.Distance < Distance && MathUtil.IntersectsPath(_o.Position, Trinity.Player.GoldPickupRadius + 2f, Trinity.Player.Position, Position))
                            {
                                if (Trinity.Settings.Combat.Misc.HiPriorityHG)
                                {
                                    OperateWeight(WeightType.Dynamic, "IsIntersectHealthGlobePickUpRadius&HiPriority", (float)((MainGrid.BaseWeight - Distance) * MainGrid.HealthGlobeWeightPct * 8f), _o.RActorGuid);
                                }
                                else if (MainGrid.ShouldAvoidAoE)
                                {
                                    OperateWeight(WeightType.Dynamic, "IsIntersectHealthGlobePickUpRadius&LowHealth", (float)((MainGrid.BaseWeight - Distance) * MainGrid.HealthGlobeWeightPct * 7f), _o.RActorGuid);
                                }
                                else
                                {
                                    OperateWeight(WeightType.Dynamic, "IsIntersectHealthGlobePickUpRadius", (float)((MainGrid.BaseWeight - Distance) * MainGrid.HealthGlobeWeightPct * 6f), _o.RActorGuid);
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
                                    OperateWeight(WeightType.Dynamic, "IsInProgressionGlobePickUpRadius", (MainGrid.BaseWeight - Distance) * 3f, _o.RActorGuid);
                                }
                                else if (_o.Distance < Distance && MathUtil.IntersectsPath(_o.Position, Trinity.Player.GoldPickupRadius + 2f, Trinity.Player.Position, Position))
                                {
                                    OperateWeight(WeightType.Dynamic, "IsIntersectProgressionGlobePickUpRadius", (MainGrid.BaseWeight - Distance) * 3f, _o.RActorGuid);
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
                                    OperateWeight(WeightType.Dynamic, "IsInPowerGlobePickUpRadius", (MainGrid.BaseWeight - Distance) * 2f, _o.RActorGuid);
                                }
                                else if (_o.Distance < Distance && MathUtil.IntersectsPath(_o.Position, Trinity.Player.GoldPickupRadius + 2f, Trinity.Player.Position, Position))
                                {
                                    OperateWeight(WeightType.Dynamic, "IsIntersectPowerGlobePickUpRadius", (MainGrid.BaseWeight - Distance) * 2f, _o.RActorGuid);
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
                                        OperateWeight(WeightType.Dynamic, "IsInGoldPickUpRadius&Goldwrap", (MainGrid.BaseWeight - Distance) * 10f, _o.RActorGuid);
                                    }
                                    else if (Legendary.KymbosGold.IsEquipped && Trinity.Player.CurrentHealthPct < 0.8)
                                    {
                                        OperateWeight(WeightType.Dynamic, "IsInGoldPickUpRadius&KymboGold", (MainGrid.BaseWeight - Distance) * 5f, _o.RActorGuid);
                                    }
                                    else
                                    {
                                        OperateWeight(WeightType.Dynamic, "IsInGoldPickUpRadius", (MainGrid.BaseWeight - Distance * 2f), _o.RActorGuid);
                                    }
                                }
                                else if (_o.Distance < Distance && MathUtil.IntersectsPath(_o.Position, Trinity.Player.GoldPickupRadius + 2f, Trinity.Player.Position, Position))
                                {
                                    if (Legendary.Goldwrap.IsEquipped)
                                    {
                                        OperateWeight(WeightType.Dynamic, "IsIntersectGoldPickUpRadius&Goldwrap", (MainGrid.BaseWeight - Distance) * 10f, _o.RActorGuid);
                                    }
                                    else if (Legendary.KymbosGold.IsEquipped && Trinity.Player.CurrentHealthPct < 0.8)
                                    {
                                        OperateWeight(WeightType.Dynamic, "IsIntersectGoldPickUpRadius&KymboGold", (MainGrid.BaseWeight - Distance) * 5f, _o.RActorGuid);
                                    }
                                    else
                                    {
                                        OperateWeight(WeightType.Dynamic, "IsIntersectGoldPickUpRadius", (MainGrid.BaseWeight - Distance) * 1.5f, _o.RActorGuid);
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
                                    OperateWeight(WeightType.Dynamic, "IsInShrineRequiredRange&HiPriority", (MainGrid.BaseWeight - Distance) * 6f, _o.RActorGuid);
                                }
                                else
                                {
                                    OperateWeight(WeightType.Dynamic, "IsInShrineRequiredRange", (MainGrid.BaseWeight - Distance) * 3f, _o.RActorGuid);
                                }
                            }
                            else if (_o.Distance < Distance && MathUtil.IntersectsPath(_o.Position, Trinity.Player.GoldPickupRadius + 2f, Trinity.Player.Position, Position))
                            {
                                if (Trinity.Settings.WorldObject.HiPriorityShrines)
                                {
                                    OperateWeight(WeightType.Dynamic, "IsIntersectShrineRequiredRange&HiPriority", (MainGrid.BaseWeight - Distance) * 6f, _o.RActorGuid);
                                }
                                else
                                {
                                    OperateWeight(WeightType.Dynamic, "IsIntersectShrineRequiredRange", (MainGrid.BaseWeight - Distance) * 3f, _o.RActorGuid);
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
                                    OperateWeight(WeightType.Dynamic, "IsInContainerRequiredRange&HiPriority", (MainGrid.BaseWeight - Distance) * 10f, _o.RActorGuid);
                                }
                                else if (_dstFromObj <= _o.Radius + 5f)
                                {
                                    OperateWeight(WeightType.Dynamic, "IsInContainerRequiredRange&HiPriority", (MainGrid.BaseWeight - Distance) * -2f, _o.RActorGuid);
                                }
                                else if (_o.Distance < Distance && MathUtil.IntersectsPath(_o.Position, _o.Radius, Trinity.Player.Position, Position, true, true))
                                {
                                    OperateWeight(WeightType.Dynamic, "IntersectsPathObstacles", (MainGrid.BaseWeight - _dstFromObj + _o.Radius) * -2f, _o.RActorGuid);
                                } 
                            }
                            break;
                        }
                        #endregion
                    default:
                        break;
                }
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
