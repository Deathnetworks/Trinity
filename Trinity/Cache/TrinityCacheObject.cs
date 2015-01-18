using System;
using System.Linq;
using System.Runtime.Serialization;
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
        [NoCopy]
        private DiaObject _object;
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
        }

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

        // Generic stuff applicable to all objects

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
        public Vector3 Position { get; set; }

        [DataMember]
        public AABB AABBBounds { get; set; }

        [DataMember]
        public float Distance { get; set; }

        [NoCopy]
        public float RadiusDistance { get { return Math.Max(Distance - Radius, 0f); } }

        [DataMember]
        public string InternalName { get; set; }

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

        /// <summary>
        /// If unit is facing player
        /// </summary>
        [DataMember]
        public bool IsFacingPlayer { get; set; }

        /// <summary>
        /// If Player is facing unit
        /// </summary>
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

        public MonsterSize MonsterSize { get; set; }

        [DataMember]
        public bool HasBeenNavigable { get; set; }

        [DataMember]
        public bool HasBeenRaycastable { get; set; }

        [DataMember]
        public bool HasBeenInLoS { get; set; }

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
        public int SummonedByACDId { get; set; }

        [NoCopy]
        public bool IsUnit { get { return this.Type == GObjectType.Unit; } }

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

        [NoCopy]
        public bool IsCursedChest { get { return Type == GObjectType.CursedChest; } }

        [NoCopy]
        public bool IsCursedShrine { get { return Type == GObjectType.CursedShrine; } }

        [NoCopy]
        public bool IsEventObject { get { return IsCursedChest || IsCursedShrine; } }

        [NoCopy]
        public bool IsFacing(Vector3 targetPosition, float arcDegrees = 70f)
        {
            if (DirectionVector != Vector2.Zero)
            {
                Vector3 u = targetPosition - this.Position;
                u.Z = 0f;
                Vector3 v = new Vector3(DirectionVector.X, DirectionVector.Y, 0f);
                bool result = ((MathEx.ToDegrees(Vector3.AngleBetween(u, v)) <= arcDegrees) ? 1 : 0) != 0;
                return result;
            }
            else
                return false;
        }

        [NoCopy]
        public bool IsPlayerFacing(float arc)
        {
            return Trinity.Player.IsFacing(this.Position, arc);
        }

        [NoCopy]
        public bool IsStandingInAvoidance
        {
            get
            {
                return CacheData.TimeBoundAvoidance.Any(a => a.Position.Distance2D(this.Position) <= a.Radius);
            }
        }

        [NoCopy]
        public AvoidanceType AvoidanceType
        {
            get
            {
                return AvoidanceManager.GetAvoidanceType(this.ActorSNO);
            }
        }

        public TrinityCacheObject(DiaObject _DiaObject = null)
        {
            if (_DiaObject != null)
                this.RActorGuid = _DiaObject.RActorGuid;
        }

        public int NearbyUnitsWithinDistance(float range = 5f)
        {
            using (new Technicals.PerformanceLogger("CacheObject.UnitsNear"))
            {
                if (this.Type != GObjectType.Unit)
                    return 0;

                return Trinity.ObjectCache
                    .Count(u => u.RActorGuid != this.RActorGuid && u.IsUnit && u.Position.Distance2D(this.Position) <= range && u.HasBeenInLoS);
            }
        }

        public int CountUnitsBehind(float range)
        {
            return
                (from u in Trinity.ObjectCache
                 where u.RActorGuid != this.RActorGuid &&
                 u.IsUnit &&
                 MathUtil.IntersectsPath(this.Position, this.Radius, Trinity.Player.Position, u.Position)
                 select u).Count();
        }

        public int CountUnitsInFront()
        {
            return
                (from u in Trinity.ObjectCache
                 where u.RActorGuid != this.RActorGuid &&
                 u.IsUnit &&
                 MathUtil.IntersectsPath(u.Position, u.Radius, Trinity.Player.Position, this.Position)
                 select u).Count();
        }

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

        public override string ToString()
        {
            return string.Format("{0}, Type={1} Dist={2} IsBossOrEliteRareUnique={3} IsAttackable={4}", InternalName, Type, RadiusDistance, IsBossOrEliteRareUnique, IsAttackable);
        }
    }
}
