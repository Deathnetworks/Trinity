using System;
using System.Collections.Generic;
using System.Linq;
using Trinity.Combat.Abilities;
using Trinity.Reference;
using Trinity.Technicals;
using Zeta.Common;

namespace Trinity
{
    internal class GridNode : IEquatable<GridNode>
    {
        public GridNode(Vector3 position = new Vector3(), float weight = 0f)
        {
            Position = MainGrid.VectorToGrid(position);
        }

        public void ResetTickValues()
        {
            _lastTickValueDistance = -1f;

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
        private float _lastTickValueDistance = -1f;

        public float Distance
        {
            get
            {
                if (_lastTickValueDistance >= 0)
                    return _lastTickValueDistance;

                _lastTickValueDistance = Position.Distance2D(Trinity.Player.Position);
                return _lastTickValueDistance;
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

        /* Does not set by grid generation, can be used for other thing */
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

                    double w = DynamicWeight + UnchangeableWeight;
                    if (!Trinity.Player.NeedToKite && !HasAvoidanceRelated)
                        w += ClusterWeight;
                    if (Trinity.Player.NeedToKite || MainGrid.ShouldBeAwayFromAoE)
                        w += MonsterWeight;
                    if (!Trinity.Player.NeedToKite && !MainGrid.ShouldAvoidAoE)
                        w += TargetWeight;

                    return w;
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

                    string wi = DynamicWeightInfos + UnchangeableWeightInfos;
                    if (!Trinity.Player.NeedToKite && !HasAvoidanceRelated)
                        wi += ClusterWeightInfos;
                    if (Trinity.Player.NeedToKite || MainGrid.ShouldBeAwayFromAoE)
                        wi += MonsterWeightInfos;
                    if (!Trinity.Player.NeedToKite && !MainGrid.ShouldAvoidAoE)
                        wi += TargetWeightInfos;

                    return wi;
                }
            }
        }

        public void OperateWeight(WeightType type, string weightInfos, float weight, int saveAsKey = 0, int _keepDuringLoop = 5)
        {
            //using (new MemorySpy("GridNode.OperateWeight()"))
            //{
                switch (type)
                {
                    case WeightType.Target:
                    {
                        if (weight != 0)
                        {
                            TargetWeight += weight;
                            TargetWeightInfos += " " + weightInfos + "(" + weight.ToString("F0") + ")";
                        }

                        if (saveAsKey != 0 && !LastTargetWeightValues.ContainsKey(saveAsKey))
                            LastTargetWeightValues.Add(saveAsKey, new DynamicWeight(weight, weightInfos, _keepDuringLoop));
                    }
                        break;
                    case WeightType.Cluster:
                    {
                        if (weight != 0)
                        {
                            ClusterWeight += weight;
                            ClusterWeightInfos += " " + weightInfos + "(" + weight.ToString("F0") + ")";
                        }

                        if (saveAsKey != 0 && !LastClusterWeightValues.ContainsKey(saveAsKey))
                            LastClusterWeightValues.Add(saveAsKey, new DynamicWeight(weight, weightInfos, _keepDuringLoop));
                    }
                        break;
                    case WeightType.Monster:
                    {
                        if (weight != 0)
                        {
                            MonsterWeight += weight;
                            MonsterWeightInfos += " " + weightInfos + "(" + weight.ToString("F0") + ")";
                        }

                        if (saveAsKey != 0 && !LastMonsterWeightValues.ContainsKey(saveAsKey))
                            LastMonsterWeightValues.Add(saveAsKey, new DynamicWeight(weight, weightInfos, _keepDuringLoop));
                    }
                        break;
                    case WeightType.Unchangeable:
                    {
                        if (weight != 0)
                        {
                            UnchangeableWeight += weight;
                            UnchangeableWeightInfos += " " + weightInfos + "(" + weight.ToString("F0") + ")";
                        }
                    }
                        break;
                    case WeightType.Dynamic:
                    {
                        if (weight != 0)
                        {
                            DynamicWeight += weight;
                            DynamicWeightInfos += " " + weightInfos + "(" + weight.ToString("F0") + ")";
                        }

                        if (saveAsKey != 0 && !LastDynamicWeightValues.ContainsKey(saveAsKey))
                            LastDynamicWeightValues.Add(saveAsKey, new DynamicWeight(weight, weightInfos, _keepDuringLoop));
                    }
                        break;
                    default:
                        break;
                }
            //}
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
            return Equals((int) Position.X, (int) other.Position.X) && Equals((int) Position.Y, (int) other.Position.Y);
        }

        public bool ObjectOOR(Vector3 _loc, float _r)
        {
            return (Math.Max(_loc.X, Position.X) - Math.Min(_loc.X, Position.X)) > _r || (Math.Max(_loc.Y, Position.Y) - Math.Max(_loc.Y, Position.Y)) > _r;
        }

        public void SetUnchangeableWeight()
        {
            /* Nearby recorded points */
            if (NearbyExitsCount > 0)
                OperateWeight(WeightType.Unchangeable, String.Format("HasExits[{0}]", NearbyExitsCount), MainGrid.BaseWeight*NearbyExitsCount);

            /* Nearby recorded exits */
            if (NearbyGridPointsCount > 0)
                OperateWeight(WeightType.Unchangeable, String.Format("CloseToOtherPoints[{0}]", NearbyGridPointsCount), MainGrid.BaseWeight*NearbyGridPointsCount);

            /* Unsafe kite zones (NavHelper.cs)*/
            if (!MainGrid.UnSafeZonesCacheIsEmpty && MainGrid.ShouldBeAwayFromAoE)
            {
                foreach (var _a in CacheData.UnSafeZones)
                {
                    if (ObjectOOR(_a.Key, _a.Value))
                        continue;

                    if (_a.Key.Distance2D(Position) <= _a.Value)
                        OperateWeight(WeightType.Unchangeable, "IsInUnsafeKiteAreas", (MainGrid.BaseWeight - _a.Key.Distance2D(Position))*-5f);
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
                        OperateWeight(WeightType.Unchangeable, "IsInVisitedZone", (MainGrid.BaseWeight - _p.Key.Distance2D(Position))*4f);
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
                OperateWeight(WeightType.Dynamic, "IsNavigable", (MainGrid.BaseWeight - Distance)*5f, 1, 3);
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
            if (LastTargetWeightValues.TryGetValue(Trinity.CurrentTarget.RActorGuid, out _w))
            {
                _w.IncreaseLoopCount();
                if (_w.KeepObject)
                {
                    OperateWeight(WeightType.Target, _w.ObjectWeightInfo + "[D]", _w.ObjectWeight);
                    return;
                }

                LastTargetWeightValues.Remove(Trinity.CurrentTarget.RActorGuid);
            }

            float _weight = 0f;
            string _weightInfos = string.Empty;

            int _dstFromObj = (int) Position.Distance2D(Trinity.CurrentTarget.Position) - (int) Trinity.CurrentTarget.Radius;
            if (_dstFromObj <= MainGrid.MinRangeToTarget - 3f)
            {
                if (Trinity.CurrentTarget.IsInLineOfSightOfPoint(Position))
                {
                    _weight = MainGrid.ShouldBeAwayFromAoE ? (MainGrid.BaseWeight + _dstFromObj)*10f : (MainGrid.BaseWeight - _dstFromObj)*10f;
                    _weightInfos = "IsInLoSOfTarget" + "(" + _weight.ToString("F0") + ")";

                    if (MainGrid.PlayerShouldKite && _dstFromObj > CombatBase.KiteDistance)
                    {
                        _weight = MainGrid.ShouldBeAwayFromAoE ? (MainGrid.BaseWeight + _dstFromObj)*8f : (MainGrid.BaseWeight - _dstFromObj)*10f;
                        _weightInfos += " IsInTargetRequiredRangeInKiteRange";
                    }
                    else
                    {
                        _weight = MainGrid.ShouldBeAwayFromAoE ? (MainGrid.BaseWeight + _dstFromObj)*6f : (MainGrid.BaseWeight - _dstFromObj)*10f;
                        _weightInfos += " IsInTargetRequiredRange";
                    }
                }
            }

            OperateWeight(WeightType.Target, _weightInfos, _weight, Trinity.CurrentTarget.RActorGuid, 3);
        }

        public void SetAvoidancesWeights()
        {
            if (MainGrid.AvoidancesCacheIsEmpty)
                return;

            foreach (var a in CacheData.AvoidanceObstacles)
            {
                int key = a.IsAvoidanceAnimations ? (int) a.Animation : a.RActorGUID;
                key += (int) a.Position.X + (int) a.Position.Y;

                DynamicWeight w;
                if (LastDynamicWeightValues.TryGetValue(key, out w))
                {
                    w.IncreaseLoopCount();
                    if (w.KeepObject)
                    {
                        if (w.ObjectWeight < 0)
                            HasAvoidanceRelated = true;

                        OperateWeight(WeightType.Dynamic, w.ObjectWeightInfo + "[D]", w.ObjectWeight);
                        continue;
                    }

                    LastDynamicWeightValues.Remove(key);
                }

                if (ObjectOOR(a.Position, 60f))
                    continue;

                float weight = 0f;
                string weightInfo = string.Empty;
                float dstFromObj = Position.Distance2D(a.Position);

                if (dstFromObj <= a.Radius)
                {
                    weight += (MainGrid.BaseWeight - dstFromObj + a.Radius)*-30f;
                    weightInfo += "IsStandingInAvoidance";
                }
                else if (a.AvoidType != AvoidType.Projectile)
                {
                    if (dstFromObj <= a.Radius*1.2)
                    {
                        weight += (MainGrid.BaseWeight - dstFromObj + a.Radius)*-28f;
                        weightInfo += "IsCloseToAvoidance";
                    }
                    else
                    {
                        //using (new MemorySpy("GridNode.SetAvoidancesWeights().GetIntersect"))
                        //{
                            if (Trinity.Player.Position.Distance2D(a.Position) < Distance &&
                                MathUtil.IntersectsPath(a.Position, a.Radius + 2f, Trinity.Player.Position, Position, true))
                            {
                                weight += (MainGrid.BaseWeight - dstFromObj + a.Radius)*-26f;
                                weightInfo += "IsIntersectAvoidanceRadius";
                            }
                        //}
                    }
                }

                if (weight < 0)
                    HasAvoidanceRelated = true;

                OperateWeight(WeightType.Dynamic, weightInfo, weight, key, 3);
            }
        }

        public void SetCacheObjectsWeights()
        {
            if (MainGrid.ObjectCacheIsEmpty)
                return;

            foreach (TrinityCacheObject o in Trinity.ObjectCache)
            {
                if (o.Distance > 55f)
                    continue;

                if (ObjectOOR(o.Position, 55f))
                    continue;

                DynamicWeight _w;
                if (LastDynamicWeightValues.TryGetValue(o.RActorGuid, out _w))
                {
                    _w.IncreaseLoopCount();
                    if (_w.KeepObject)
                    {
                        OperateWeight(WeightType.Dynamic, _w.ObjectWeightInfo + "[D]", _w.ObjectWeight);
                        continue;
                    }

                    LastDynamicWeightValues.Remove(o.RActorGuid);
                }

                switch (o.Type)
                {
                    case GObjectType.Unit:

                        #region Unit

                    {
                        if (LastMonsterWeightValues.TryGetValue(o.RActorGuid, out _w))
                        {
                            _w.IncreaseLoopCount();
                            if (_w.KeepObject)
                            {
                                if (_w.ObjectWeight < 0)
                                    HasMonsterRelated = true;

                                OperateWeight(WeightType.Monster, _w.ObjectWeightInfo + "[D]", _w.ObjectWeight);
                                continue;
                            }

                            LastMonsterWeightValues.Remove(o.RActorGuid);
                        }

                        int _dstFromObj = (int) Position.Distance2D(o.Position);

                        if (!MainGrid.ShouldBeAwayFromAoE)
                        {
                            if (LastClusterWeightValues.TryGetValue(o.RActorGuid + (int) o.Position.X + (int) o.Position.Y, out _w))
                            {
                                _w.IncreaseLoopCount();
                                if (_w.KeepObject)
                                {
                                    if (_dstFromObj <= o.Radius)
                                        HasMonsterRelated = true;

                                    OperateWeight(WeightType.Cluster, _w.ObjectWeightInfo + "[D]", _w.ObjectWeight);
                                    continue;
                                }

                                LastClusterWeightValues.Remove(o.RActorGuid + (int) o.Position.X + (int) o.Position.Y);
                            }

                            float _clusterWeight = 0f;
                            if (!HasAvoidanceRelated && _dstFromObj <= Trinity.Settings.Combat.Misc.TrashPackClusterRadius && o.IsTrashPackOrBossEliteRareUnique)
                            {
                                _clusterWeight = (float) (((Trinity.Settings.Combat.Misc.TrashPackClusterRadius - _dstFromObj)*Trinity.Settings.Combat.Misc.TrashPackClusterRadius) + o.Radius + ((o.Weight*100)/50000))*o.NearbyUnits*10f;
                                OperateWeight(WeightType.Cluster, "Clustering", _clusterWeight, o.RActorGuid + (int) o.Position.X + (int) o.Position.Y, 3);
                            }

                            if (_dstFromObj <= o.Radius)
                                HasMonsterRelated = true;
                        }
                        else if (_dstFromObj <= o.Radius)
                        {
                            HasMonsterRelated = true;
                            OperateWeight(WeightType.Monster, "IsInMonsterRadius", (MainGrid.BaseWeight - _dstFromObj + o.Radius)*-13f, o.RActorGuid, 3);
                        }
                        else if (MainGrid.PlayerShouldKite && !o.IsTreasureGoblin && _dstFromObj <= CombatBase.KiteDistance)
                        {
                            if (o.IsBoss && MainGrid.ShouldKiteBosses)
                            {
                                OperateWeight(WeightType.Monster, "IsInBossKiteRange", (MainGrid.BaseWeight - _dstFromObj + CombatBase.KiteDistance + o.Radius)*-13f, o.RActorGuid, 3);
                            }
                            else if (o.IsBossOrEliteRareUnique && MainGrid.ShouldKiteElites)
                            {
                                OperateWeight(WeightType.Monster, "IsInEliteKiteRange", (MainGrid.BaseWeight - _dstFromObj + CombatBase.KiteDistance + o.Radius)*-11f, o.RActorGuid, 3);
                            }
                            else if (MainGrid.ShouldKiteTrashs)
                            {
                                OperateWeight(WeightType.Monster, "IsInMobKiteRange", (MainGrid.BaseWeight - _dstFromObj + CombatBase.KiteDistance + o.Radius)*-9f, o.RActorGuid, 3);
                            }
                        }
                        else if (o.Distance < Distance && MathUtil.IntersectsPath(o.Position, o.Radius + 2f, Trinity.Player.Position, Position))
                        {
                            OperateWeight(WeightType.Monster, "IsIntersectMonsterRadius", (MainGrid.BaseWeight - _dstFromObj + o.Radius)*-13f, o.RActorGuid, 3);
                        }
                        else if (MainGrid.ShouldFlee)
                        {
                            OperateWeight(WeightType.Monster, "AvoidMonster", (MainGrid.BaseWeight + _dstFromObj)*10f, o.RActorGuid, 3);
                        }
                        break;
                    }

                        #endregion

                    case GObjectType.HealthWell:

                        #region HealthWell

                    {
                        int _dstFromObj = (int) Position.Distance2D(o.Position);
                        if (_dstFromObj <= 5f && Trinity.Player.CurrentHealthPct < 0.3)
                        {
                            OperateWeight(WeightType.Dynamic, "IsInHealthWellRequiredRange", (MainGrid.BaseWeight - Distance)*4f, o.RActorGuid);
                        }
                        else if (o.Distance < Distance && MathUtil.IntersectsPath(o.Position, 5f, Trinity.Player.Position, Position))
                        {
                            OperateWeight(WeightType.Dynamic, "IsIntersectHealthWellRequiredRange", (MainGrid.BaseWeight - Distance)*4f, o.RActorGuid);
                        }
                        break;
                    }

                        #endregion

                    case GObjectType.HealthGlobe:

                        #region HealthGlobe

                    {
                        int _dstFromObj = (int) Position.Distance2D(o.Position);
                        if (_dstFromObj <= Trinity.Player.GoldPickupRadius + 2f && MainGrid.ShouldCollectHealthGlobe)
                        {
                            if (Trinity.Settings.Combat.Misc.HiPriorityHG)
                            {
                                OperateWeight(WeightType.Dynamic, "IsInHealthGlobePickUpRadius&HighPririty", (MainGrid.BaseWeight - Distance)*MainGrid.HealthGlobeWeightPct*8f, o.RActorGuid);
                            }
                            else if (MainGrid.ShouldAvoidAoE)
                            {
                                OperateWeight(WeightType.Dynamic, "IsInHealthGlobePickUpRadius&LowHealth", (MainGrid.BaseWeight - Distance)*MainGrid.HealthGlobeWeightPct*7f, o.RActorGuid);
                            }
                            else
                            {
                                OperateWeight(WeightType.Dynamic, "IsInHealthGlobePickUpRadius", (MainGrid.BaseWeight - Distance)*MainGrid.HealthGlobeWeightPct*6f, o.RActorGuid);
                            }
                        }
                        else if (o.Distance < Distance && MathUtil.IntersectsPath(o.Position, Trinity.Player.GoldPickupRadius + 2f, Trinity.Player.Position, Position))
                        {
                            if (Trinity.Settings.Combat.Misc.HiPriorityHG)
                            {
                                OperateWeight(WeightType.Dynamic, "IsIntersectHealthGlobePickUpRadius&HiPriority", (MainGrid.BaseWeight - Distance)*MainGrid.HealthGlobeWeightPct*8f, o.RActorGuid);
                            }
                            else if (MainGrid.ShouldAvoidAoE)
                            {
                                OperateWeight(WeightType.Dynamic, "IsIntersectHealthGlobePickUpRadius&LowHealth", (MainGrid.BaseWeight - Distance)*MainGrid.HealthGlobeWeightPct*7f, o.RActorGuid);
                            }
                            else
                            {
                                OperateWeight(WeightType.Dynamic, "IsIntersectHealthGlobePickUpRadius", (MainGrid.BaseWeight - Distance)*MainGrid.HealthGlobeWeightPct*6f, o.RActorGuid);
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
                            int _dstFromObj = (int) Position.Distance2D(o.Position);
                            if (_dstFromObj <= Trinity.Player.GoldPickupRadius + 2f)
                            {
                                OperateWeight(WeightType.Dynamic, "IsInProgressionGlobePickUpRadius", (MainGrid.BaseWeight - Distance)*3f, o.RActorGuid);
                            }
                            else if (o.Distance < Distance && MathUtil.IntersectsPath(o.Position, Trinity.Player.GoldPickupRadius + 2f, Trinity.Player.Position, Position))
                            {
                                OperateWeight(WeightType.Dynamic, "IsIntersectProgressionGlobePickUpRadius", (MainGrid.BaseWeight - Distance)*3f, o.RActorGuid);
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
                            int _dstFromObj = (int) Position.Distance2D(o.Position);
                            if (_dstFromObj <= Trinity.Player.GoldPickupRadius + 2f)
                            {
                                OperateWeight(WeightType.Dynamic, "IsInPowerGlobePickUpRadius", (MainGrid.BaseWeight - Distance)*2f, o.RActorGuid);
                            }
                            else if (o.Distance < Distance && MathUtil.IntersectsPath(o.Position, Trinity.Player.GoldPickupRadius + 2f, Trinity.Player.Position, Position))
                            {
                                OperateWeight(WeightType.Dynamic, "IsIntersectPowerGlobePickUpRadius", (MainGrid.BaseWeight - Distance)*2f, o.RActorGuid);
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
                            int _dstFromObj = (int) Position.Distance2D(o.Position);
                            if (_dstFromObj <= Trinity.Player.GoldPickupRadius + 2f)
                            {
                                if (Legendary.Goldwrap.IsEquipped)
                                {
                                    OperateWeight(WeightType.Dynamic, "IsInGoldPickUpRadius&Goldwrap", (MainGrid.BaseWeight - Distance)*10f, o.RActorGuid);
                                }
                                else if (Legendary.KymbosGold.IsEquipped && Trinity.Player.CurrentHealthPct < 0.8)
                                {
                                    OperateWeight(WeightType.Dynamic, "IsInGoldPickUpRadius&KymboGold", (MainGrid.BaseWeight - Distance)*5f, o.RActorGuid);
                                }
                                else
                                {
                                    OperateWeight(WeightType.Dynamic, "IsInGoldPickUpRadius", (MainGrid.BaseWeight - Distance*2f), o.RActorGuid);
                                }
                            }
                            else if (o.Distance < Distance && MathUtil.IntersectsPath(o.Position, Trinity.Player.GoldPickupRadius + 2f, Trinity.Player.Position, Position))
                            {
                                if (Legendary.Goldwrap.IsEquipped)
                                {
                                    OperateWeight(WeightType.Dynamic, "IsIntersectGoldPickUpRadius&Goldwrap", (MainGrid.BaseWeight - Distance)*10f, o.RActorGuid);
                                }
                                else if (Legendary.KymbosGold.IsEquipped && Trinity.Player.CurrentHealthPct < 0.8)
                                {
                                    OperateWeight(WeightType.Dynamic, "IsIntersectGoldPickUpRadius&KymboGold", (MainGrid.BaseWeight - Distance)*5f, o.RActorGuid);
                                }
                                else
                                {
                                    OperateWeight(WeightType.Dynamic, "IsIntersectGoldPickUpRadius", (MainGrid.BaseWeight - Distance)*1.5f, o.RActorGuid);
                                }
                            }
                        }
                        break;
                    }

                        #endregion

                    case GObjectType.Shrine:

                        #region Shrine

                    {
                        int _dstFromObj = (int) Position.Distance2D(o.Position);
                        if (_dstFromObj <= 5f)
                        {
                            if (Trinity.Settings.WorldObject.HiPriorityShrines)
                            {
                                OperateWeight(WeightType.Dynamic, "IsInShrineRequiredRange&HiPriority", (MainGrid.BaseWeight - Distance)*6f, o.RActorGuid);
                            }
                            else
                            {
                                OperateWeight(WeightType.Dynamic, "IsInShrineRequiredRange", (MainGrid.BaseWeight - Distance)*3f, o.RActorGuid);
                            }
                        }
                        else if (o.Distance < Distance && MathUtil.IntersectsPath(o.Position, Trinity.Player.GoldPickupRadius + 2f, Trinity.Player.Position, Position))
                        {
                            if (Trinity.Settings.WorldObject.HiPriorityShrines)
                            {
                                OperateWeight(WeightType.Dynamic, "IsIntersectShrineRequiredRange&HiPriority", (MainGrid.BaseWeight - Distance)*6f, o.RActorGuid);
                            }
                            else
                            {
                                OperateWeight(WeightType.Dynamic, "IsIntersectShrineRequiredRange", (MainGrid.BaseWeight - Distance)*3f, o.RActorGuid);
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
                            int _dstFromObj = (int) Position.Distance2D(o.Position);
                            if (_dstFromObj <= o.Radius + 5f && !MainGrid.ShouldAvoidAoE && !o.IsNavBlocking() &&
                                (Trinity.Settings.WorldObject.HiPriorityContainers ||
                                 ((Legendary.HarringtonWaistguard.IsEquipped && !Legendary.HarringtonWaistguard.IsBuffActive))))
                            {
                                OperateWeight(WeightType.Dynamic, "IsInContainerRequiredRange&HiPriority", (MainGrid.BaseWeight - Distance)*10f, o.RActorGuid);
                            }
                            else if (_dstFromObj <= o.Radius + 5f)
                            {
                                OperateWeight(WeightType.Dynamic, "IsInContainerRequiredRange&HiPriority", (MainGrid.BaseWeight - Distance)*-2f, o.RActorGuid);
                            }
                            else if (o.Distance < Distance && MathUtil.IntersectsPath(o.Position, o.Radius, Trinity.Player.Position, Position, true, true))
                            {
                                OperateWeight(WeightType.Dynamic, "IntersectsPathObstacles", (MainGrid.BaseWeight - _dstFromObj + o.Radius)*-2f, o.RActorGuid);
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

    internal class DynamicWeight : IEquatable<DynamicWeight>
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

        public bool KeepObject
        {
            get { return CurrentLoopCount < KeepDuringLoop; }
        }

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