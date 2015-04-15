using System;
using System.Linq;
using System.Runtime.Serialization;
using Trinity.Cache;
using Trinity.Combat.Abilities;
using Trinity.Config.Combat;
using Trinity.DbProvider;
using Zeta.Common;
using Zeta.Game;
using Zeta.Game.Internals.Actors;
using Zeta.Game.Internals.SNO;

namespace Trinity
{
    // TrinityCacheObject type used to cache all data
    // Let's me create an object list with ONLY the data I need read from D3 memory, and then read from this while
    // Handling movement and interaction with the target - whether the target is a shrine, an item or a monster
    // Completely minimizing the D3 memory reads to the bare minimum
    [DataContract]
    public class TrinityCacheObject
    {
        public TrinityCacheObject(DiaObject diaObject = null)
        {
            if (diaObject != null)
                RActorGuid = diaObject.RActorGuid;
        }

        [NoCopy]
        public DiaObject Object
        {
            get
            {
                if (_object == null || (_object != null && !_object.IsValid))
                {
                    _object = ZetaDia.Actors.GetActorsOfType<DiaObject>(true, true).FirstOrDefault(o => o.RActorGuid == RActorGuid);
                }
                return _object;
            }
            set { _object = value; }
        }

        [NoCopy]
        public DiaUnit Unit
        {
            get
            {
                if (Object != null && Object.IsValid && Object is DiaUnit)
                    return Object as DiaUnit;
                return default(DiaUnit);
            }
        }

        [NoCopy]
        public DiaGizmo Gizmo
        {
            get
            {
                if (Object != null && Object.IsValid && Object is DiaGizmo)
                    return Object as DiaGizmo;
                return default(DiaGizmo);
            }
        }

        [NoCopy]
        public DiaItem Item
        {
            get
            {
                if (Object != null && Object.IsValid && Object is DiaItem)
                    return Object as DiaItem;
                return default(DiaItem);
            }
        }

        [NoCopy]
        public ACD CommonData
        {
            get
            {
                if (Object == null) return null;

                return Object.CommonData;
            }
            set { throw new NotImplementedException(); }
        }

        [NoCopy]
        public bool CommonDataIsValid
        {
            get
            {
                return CommonData != null && CommonData.IsValid;
            }
        }

        [DataMember]
        public int ACDGuid { get; set; }

        [DataMember]
        public int RActorGuid { get; set; }

        [DataMember]
        public int ActorSNO { get; set; }

        [DataMember]
        public GObjectType Type { get; set; }

        [DataMember]
        public ActorType ActorType { get; set; }

        [DataMember]
        public GizmoType GizmoType { get; set; }

        [DataMember]
        public double Weight { get; set; }

        [DataMember]
        public string WeightInfo { get; set; }

        [DataMember]
        public double SpecialWeight { get; set; }

        [DataMember]
        public Vector3 Position { get; set; }

        [DataMember]
        public float ZDiff { get; set; }

        [DataMember]
        public AABB AABBBounds { get; set; }

        [NoCopy]
        public float Distance
        {
            get
            {
                if (_distance >= 0f)
                    return _distance;
                _distance = Trinity.Player.Position.Distance2D(Position);
                return Distance;
            }
            set { _distance = value; }
        }

        [NoCopy]
        public float RadiusDistance { get { return Math.Max(Distance - Radius, 0f); } }

        [DataMember]
        public float RequiredRange { get; set; }

        [DataMember]
        public string InternalName { get; set; }

        [DataMember]
        public SNOAnim Animation { get; set; }

        [DataMember]
        public int DynamicID { get; set; }

        [DataMember]
        public int GameBalanceID { get; set; }

        [DataMember]
        public int ItemLevel { get; set; }

        [DataMember]
        public string ItemLink { get; set; }

        [DataMember]
        public int GoldAmount { get; set; }

        [DataMember]
        public bool OneHanded { get; set; }

        [DataMember]
        public bool TwoHanded { get; set; }

        [DataMember]
        public ItemQuality ItemQuality { get; set; }

        [DataMember]
        public ItemBaseType DBItemBaseType { get; set; }

        [DataMember]
        public ItemType DBItemType { get; set; }

        [DataMember]
        public FollowerType FollowerType { get; set; }

        [DataMember]
        public GItemType TrinityItemType { get; set; }

        [DataMember]
        public DateTime LastSeenTime { get; set; }

        [DataMember]
        public bool IsElite { get; set; }

        [DataMember]
        public bool IsRare { get; set; }

        [DataMember]
        public bool IsUnique { get; set; }

        [DataMember]
        public bool IsMinion { get; set; }

        [DataMember]
        public MonsterAffixes MonsterAffixes { get; set; }

        [DataMember]
        public bool IsTreasureGoblin { get; set; }

        [DataMember]
        public bool IsEliteRareUnique { get; set; }

        [DataMember]
        public bool IsBoss { get; set; }

        [DataMember]
        public bool IsAncient { get; set; }

        [DataMember]
        public bool HasAffixShielded { get; set; }

        [DataMember]
        public bool IsAttackable { get; set; }

        [DataMember]
        public bool HasDotDPS { get; set; }

        [DataMember]
        public double HitPointsPct { get; set; }

        [DataMember]
        public double HitPoints { get; set; }

        [DataMember]
        public float Radius { get; set; }

        [DataMember]
        public float Rotation { get; set; }

        [DataMember]
        public Vector2 DirectionVector { get; set; }

        [DataMember]
        public bool IsFacingPlayer { get; set; }

        [DataMember]
        public bool ForceLeapAgainst { get; set; }

        [DataMember]
        public bool HasBeenPrimaryTarget { get; set; }

        [DataMember]
        public int TimesBeenPrimaryTarget { get; set; }

        [DataMember]
        public DateTime FirstTargetAssignmentTime { get; set; }

        [DataMember]
        public string ObjectHash { get; set; }

        [DataMember]
        public double KillRange { get; set; }

        [DataMember]
        public MonsterSize MonsterSize { get; set; }

        [NoCopy]
        public bool IsBossOrEliteRareUnique { get { return (this.IsUnit && (IsEliteRareUnique || IsBoss)); } }

        [NoCopy]
        public bool IsTrashMob { get { return (this.IsUnit && !(IsEliteRareUnique || IsBoss || IsTreasureGoblin)); } }

        [NoCopy]
        public bool IsMe { get { return RActorGuid == Trinity.Player.RActorGuid; } }

        [DataMember]
        public bool IsSummonedByPlayer { get; set; }

        [DataMember]
        public bool IsSummoner { get; set; }

        [DataMember]
        public bool IsAlly { get; set; }

        [DataMember]
        public int SummonedByACDId { get; set; }

        [NoCopy]
        public bool IsUnit { get { return this.Type == GObjectType.Unit; } }

        [NoCopy]
        public bool IsAvoidance { get { return this.Type == GObjectType.Avoidance; } }

        [DataMember]
        public bool IsKite { get; set; }

        [DataMember]
        public bool IsNPC { get; set; }

        [DataMember]
        public bool NPCIsOperable { get; set; }

        [DataMember]
        public bool IsQuestMonster { get; set; }

        [DataMember]
        public bool IsMinimapActive { get; set; }

        [DataMember]
        public bool IsBountyObjective { get; set; }

        [DataMember]
        public bool IsQuestGiver { get; set; }

        [DataMember]
        public bool IsMarker { get; set; }

        [NoCopy]
        public bool IsCursedChest { get { return Type == GObjectType.CursedChest; } }

        [NoCopy]
        public bool IsCursedShrine { get { return Type == GObjectType.CursedShrine; } }

        [NoCopy]
        public bool IsEventObject { get { return IsCursedChest || IsCursedShrine; } }

        public bool IsInLineOfSight { get; set; }

        [NoCopy]
        public bool IsInLineOfSightOfPoint(Vector3 origin)
        {
            Vector3 target = Position;

            if (MathUtil.GetDiff(origin.Z, target.Z) > 11f)
                return false;

            float dist = origin.Distance2D(target);
            if (dist > 95f)
                return false;

            if (dist >= 1f && dist <= 6f)
                return true;

            if (origin.Distance2D(Trinity.Player.Position) <= 6f)
                return IsInLineOfSight;

            bool isInLoS;

            if (CacheData.HasBeenInLoS.TryGetValue(RActorGuid, out isInLoS))
                return isInLoS;

            // RayCast check
            isInLoS = NavHelper.CanRayCast(origin, target, (Trinity.Player.IsRanged || (Distance <= 16f && Distance > 1f)));

            if (!CacheData.HasBeenInLoS.ContainsKey(RActorGuid))
                CacheData.HasBeenInLoS.Add(RActorGuid, isInLoS);

            return isInLoS;
        }

        [NoCopy]
        public bool IsFacing(Vector3 targetPosition, float arcDegrees = 70f)
        {
            if (DirectionVector != Vector2.Zero)
            {
                Vector3 u = targetPosition - Position;
                u.Z = 0f;
                Vector3 v = new Vector3(DirectionVector.X, DirectionVector.Y, 0f);

                return ((MathEx.ToDegrees(Vector3.AngleBetween(u, v)) <= arcDegrees) ? 1 : 0) != 0;
            }

            return false;
        }

        [NoCopy]
        public bool IsPlayerFacing(float arc)
        {
            return Trinity.Player.IsFacing(Position, arc);
        }

        [NoCopy]
        public bool IsStandingInAvoidance { get { return CacheData.AvoidanceObstacles.Any(a => a.Position.Distance2D(Position) <= a.Radius); } }

        [NoCopy]
        public AvoidanceType AvoidanceType { get { return AvoidanceManager.GetAvoidanceType(ActorSNO); } }

        [NoCopy]
        public Vector3 ClusterPosition(float range = 20f)
        {
            var cluster = GridMap.GetBestClusterNode(Position, maxRange: range, useDefault: false);
            if (cluster != null)
                return cluster.Position;

            return Position;
        }

        [NoCopy]
        public double UnitsWeightsWithinDistance(float range = 5f)
        {
            double weight;
            if (CacheData.UnitsWeightsWithinDistanceRecorded.TryGetValue(new Tuple<int, int>(RActorGuid, (int)range), out weight))
            {
                return weight;
            }

            weight = Weight;
            foreach (var u in Trinity.ObjectCache)
            {
                if (!u.IsUnit)
                    continue;
                if (u.RActorGuid == RActorGuid)
                    continue;

                var dist = u.Position.Distance2D(Position);
                if (dist <= range)
                    weight += Math.Max(u.Weight, 100) * (range - dist);
            }

            if (!CacheData.UnitsWeightsWithinDistanceRecorded.ContainsKey(new Tuple<int, int>(RActorGuid, (int)range)))
                CacheData.UnitsWeightsWithinDistanceRecorded.Add(new Tuple<int, int>(RActorGuid, (int)range), weight);

            return weight;
        }

        [NoCopy]
        public int NearbyUnitsWithinDistance(float range = 5f)
        {
            if (range == 5f)
                return NearbyUnits;

            int count;
            if (CacheData.NearbyUnitsWithinDistanceRecorded.TryGetValue(new Tuple<int, int>(RActorGuid, (int)range), out count))
            {
                return count;
            }

            foreach (var u in CacheData.MonsterObstacles)
            {
                if (u.RActorGUID == RActorGuid)
                    continue;
                if (u.Position.Distance2D(Position) > range)
                    continue;

                count++;
            }

            if (!CacheData.NearbyUnitsWithinDistanceRecorded.ContainsKey(new Tuple<int, int>(RActorGuid, (int)range)))
                CacheData.NearbyUnitsWithinDistanceRecorded.Add(new Tuple<int, int>(RActorGuid, (int)range), count);

            return count;
        }

        [NoCopy]
        public int NearbyUnits
        {
            get { return NearbyUnitsWithinDistance(Trinity.Settings.Combat.Misc.TrashPackClusterRadius); }
        }

        [NoCopy]
        public int CountUnitsBehind(float range)
        {
            return
                (from u in Trinity.ObjectCache
                 where u.RActorGuid != RActorGuid &&
                 u.IsUnit &&
                 MathUtil.IntersectsPath(Position, Radius, Trinity.Player.Position, u.Position)
                 select u).Count();
        }

        [NoCopy]
        public int CountUnitsInFront
        {
            get
            {
                if (_countUnitsInFront >= 0)
                    return _countUnitsInFront;

                _countUnitsInFront =
                    (from u in Trinity.ObjectCache
                     where u.RActorGuid != RActorGuid &&
                     u.IsUnit &&
                     MathUtil.IntersectsPath(u.Position, u.Radius, Trinity.Player.Position, Position)
                     select u).Count();

                return _countUnitsInFront;
            }
        }

        [NoCopy]
        public int CountFCObjectsInFront
        {
            get
            {
                if (_countFcObjectsInFront >= 0)
                    return _countFcObjectsInFront;

                int count = 0;
                Vector3 locAway = MathEx.CalculatePointFrom(Position, Trinity.Player.Position, 40f);
                string dir = MathUtil.GetHeadingToPoint(Position);

                foreach (var o in Trinity.ObjectCache)
                {
                    if (o.Type == GObjectType.Destructible || o.IsUnit)
                    {
                        if (o.Position.Distance2D(Position) > 40f)
                            continue;

                        if (o.IsUnit && (!o.CommonDataIsValid || o.HitPointsPct <= 0f))
                            continue;

                        if (!dir.Equals(MathUtil.GetHeadingToPoint(o.Position)))
                            continue;

                        float radius = Math.Min(Math.Max(o.Radius, 5f), 8f);
                        if (NavHelper.CanRayCast(o.Position, Trinity.Player.Position) && MathUtil.IntersectsPath(o.Position, radius, Trinity.Player.Position, locAway))
                        {
                            if (o.IsBoss || (o.IsTreasureGoblin && Trinity.Settings.Combat.Misc.GoblinPriority == GoblinPriority.Kamikaze))
                                count += 3;
                            else if (o.Type == GObjectType.Destructible || o.IsEliteRareUnique || (o.IsTreasureGoblin && Trinity.Settings.Combat.Misc.GoblinPriority == GoblinPriority.Prioritize))
                                count += 2;
                            else
                                count++;

                            if (TownRun.IsTryingToTownPortal())
                                count++;
                        }
                    }
                }

                _countFcObjectsInFront = count;
                return _countFcObjectsInFront;
            }
        }

        [NoCopy]
        public bool IsInTrashPackCluster
        {
            get
            {
                return NearbyUnits <= Trinity.Settings.Combat.Misc.TrashPackSize;
            }
        }

        [NoCopy]
        public bool IsTrashPackOrBossEliteRareUnique
        {
            get
            {
                return IsBossOrEliteRareUnique || IsInTrashPackCluster;
            }
        }

        [NoCopy]
        public bool HasDebuff(SNOPower debuffSNO)
        {
            try
            {
                if (CommonData.GetAttribute<int>(((int)debuffSNO << 12) + ((int)ActorAttributeType.PowerBuff0VisualEffect & 0xFFF)) == 1)
                    return true;
                if (CommonData.GetAttribute<int>(((int)debuffSNO << 12) + ((int)ActorAttributeType.PowerBuff0VisualEffectA & 0xFFF)) == 1)
                    return true;
                if (CommonData.GetAttribute<int>(((int)debuffSNO << 12) + ((int)ActorAttributeType.PowerBuff0VisualEffectB & 0xFFF)) == 1)
                    return true;
                if (CommonData.GetAttribute<int>(((int)debuffSNO << 12) + ((int)ActorAttributeType.PowerBuff0VisualEffectC & 0xFFF)) == 1)
                    return true;
                if (CommonData.GetAttribute<int>(((int)debuffSNO << 12) + ((int)ActorAttributeType.PowerBuff0VisualEffectD & 0xFFF)) == 1)
                    return true;
                if (CommonData.GetAttribute<int>(((int)debuffSNO << 12) + ((int)ActorAttributeType.PowerBuff0VisualEffectE & 0xFFF)) == 1)
                    return true;

            }
            catch (Exception) { }
            return false;
        }

        [NoCopy]
        public bool IsBlacklisted
        {
            get
            {
                return
                    GenericBlacklist.ContainsKey(ObjectHash) ||
                    Trinity.Blacklist90Seconds.Contains(RActorGuid) ||
                    Trinity.Blacklist60Seconds.Contains(RActorGuid) ||
                    Trinity.Blacklist15Seconds.Contains(RActorGuid) ||
                    Trinity.Blacklist3Seconds.Contains(RActorGuid) ||
                    Trinity.Blacklist1Second.Contains(RActorGuid) ||
                    DataDictionary.BlackListIds.Contains(RActorGuid);
            }
        }

        [NoCopy]
        public override string ToString()
        {
            return string.Format("{0}, Type={1} Dist={2} IsBossOrEliteRareUnique={3} IsAttackable={4}", InternalName, Type, RadiusDistance, IsBossOrEliteRareUnique, IsAttackable);
        }

        [NoCopy]
        public string Infos
        {
            get
            {
                if (IsUnit)
                    return String.Format("{0} {1:0}/{2} yds Power={3} Type={4} Elite={5} LoS={6} HP={7:0.00} Weight={8:0}",
                        InternalName,
                        RadiusDistance,
                        RequiredRange,
                        CombatBase.CurrentPower.SNOPower,
                        Type,
                        IsBossOrEliteRareUnique,
                        IsInLineOfSight,
                        HitPointsPct,
                        Weight);

                return String.Format("{0} {1:0}/{2} yds Type={3} Weight={4:0}",
                    InternalName,
                    RadiusDistance,
                    RequiredRange,
                    Type,
                    Weight);
            }
        }

        [NoCopy]
        public bool IsDestroyable { get { return Type == GObjectType.Barricade || Type == GObjectType.Destructible; } }

        /* private member */

        [NoCopy]
        private DiaObject _object;

        private bool? _isInLineOfSight;

        private float _distance = -1;

        private int _countUnitsInFront = -1;

        private int _countFcObjectsInFront = -1;
    }
}
