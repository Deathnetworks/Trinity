using System;
using System.Linq;
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
                    _object = ZetaDia.Actors.GetActorsOfType<DiaObject>(true, true).Where(o => o.RActorGuid == this.RActorGuid).FirstOrDefault();
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
                else
                    return null;
            }
        }
        [NoCopy]
        public DiaGizmo Gizmo
        {
            get
            {
                if (Object != null && Object.IsValid && Object is DiaGizmo)
                    return Object as DiaGizmo;
                else
                    return null;
            }
        }

        public int ACDGuid { get; set; }
        public int RActorGuid { get; set; }
        public int ActorSNO { get; set; }

        // Generic stuff applicable to all objects
        public GObjectType Type { get; set; }
        public double Weight { get; set; }
        public Vector3 Position { get; set; }
        public AABB AABBBounds { get; set; }
        public float CentreDistance { get; set; }
        public float RadiusDistance { get; set; }
        public string InternalName { get; set; }
        public SNOAnim Animation { get; set; }
        public int DynamicID { get; set; }
        public int BalanceID { get; set; }
        public int ItemLevel { get; set; }
        public string ItemLink { get; set; }
        public int GoldAmount { get; set; }
        public bool OneHanded { get; set; }
        public bool TwoHanded { get; set; }
        public ItemQuality ItemQuality { get; set; }
        public ItemBaseType DBItemBaseType { get; set; }
        public ItemType DBItemType { get; set; }
        public FollowerType FollowerType { get; set; }
        public GItemType TrinityItemType { get; set; }
        public bool IsElite { get; set; }
        public bool IsRare { get; set; }
        public bool IsUnique { get; set; }
        public bool IsMinion { get; set; }
        public MonsterAffixes MonsterAffixes { get; set; }
        public bool IsTreasureGoblin { get; set; }
        public bool IsEliteRareUnique { get; set; }
        public bool IsBoss { get; set; }
        public bool HasAffixShielded { get; set; }
        public bool IsAttackable { get; set; }
        public bool HasDotDPS { get; set; }
        public double HitPointsPct { get; set; }
        public double HitPoints { get; set; }
        public float Radius { get; set; }
        public float Rotation { get; set; }
        public Vector2 DirectionVector { get; set; }
        /// <summary>
        /// If unit is facing player
        /// </summary>
        public bool IsFacingPlayer { get; set; }
        /// <summary>
        /// If Player is facing unit
        /// </summary>
        public bool ForceLeapAgainst { get; set; }
        public bool HasBeenPrimaryTarget { get; set; }
        public int TimesBeenPrimaryTarget { get; set; }
        public DateTime FirstTargetAssignmentTime { get; set; }
        public string ObjectHash { get; set; }
        public double KillRange { get; set; }
        public MonsterSize MonsterSize { get; set; }
        public bool HasBeenNavigable { get; set; }
        public bool HasBeenRaycastable { get; set; }
        public bool HasBeenInLoS { get; set; }
        [NoCopy]
        public bool IsBossOrEliteRareUnique { get { return (this.IsUnit && (IsEliteRareUnique || IsBoss || IsTreasureGoblin)); } }
        [NoCopy]
        public bool IsTrashMob { get { return (this.IsUnit && !(IsEliteRareUnique || IsBoss || IsTreasureGoblin)); } }
        [NoCopy]
        public bool IsMe { get { return RActorGuid == Trinity.Player.RActorGuid; } }
        public bool IsSummonedByPlayer { get; set; }
        public bool IsSummoner { get; set; }
        [NoCopy]
        public bool IsUnit { get { return this.Type == GObjectType.Unit; } }
        public bool IsNPC { get; set; }
        public bool NPCIsOperable { get; set; }
        public bool IsQuestMonster { get; set; }
        public bool IsMinimapActive { get; set; }
        public bool IsBountyObjective { get; set; }

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
    }
}
