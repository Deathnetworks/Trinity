using System;
using Zeta;
using Zeta.Common;
using Zeta.Internals.Actors;
using Zeta.Internals.SNO;

namespace GilesTrinity.Technicals
{
    public enum CacheType
    {
        Object,
        Unit,
        Item,
        Gizmo,
        Other
    }

    internal abstract class CacheObject : IEquatable<CacheObject>
    {
        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="CacheObject" /> class.
        /// </summary>
        /// <param name="rActorGuid">The Actor GUID.</param>
        public CacheObject(int rActorGuid)
        {
            RActorGuid = rActorGuid;
            Type = ObjectType.Unknown;
            LastAccessDate = DateTime.UtcNow;

            CacheType = Technicals.CacheType.Other;

        }
        public CacheObject(ACD acd)
        {
            RActorGuid = acd.AsRActor.RActorGuid;
            
            Type = ObjectType.Unknown;
            CacheType = Technicals.CacheType.Other;
            LastAccessDate = DateTime.UtcNow;

        }
        #endregion Constructors

        #region Properties
        /// <summary>
        /// Gets the RActorGUID.
        /// </summary>
        /// <value>The RActorGUID.</value>
        public int RActorGuid
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets or sets the last access date.
        /// </summary>
        /// <value>The last access date.</value>
        public DateTime LastAccessDate
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the object type.
        /// </summary>
        /// <value>The object type.</value>
        public ObjectType Type
        {
            get;
            set;
        }
        #endregion Properties

        #region Methods
        /// <summary>
        /// Clones this instance.
        /// </summary>
        /// <returns>Cloned instance of <see cref="CacheObject"/>.</returns>
        public abstract CacheObject Clone();
        #endregion Methods

        /// <summary>
        /// Gets the ACDGUID
        /// </summary>
        public int ACDGUID { get; set; }

        /// <summary>
        /// Gets the DynamicID
        /// </summary>
        public int DynamicID { get; set; }

        /// <summary>
        /// Gets the BalanceID
        /// </summary>
        public int BalanceID { get; set; }

        /// <summary>
        /// Gets the ActorSNO
        /// </summary>
        public int ActorSNO { get; set; }

        /// <summary>
        /// The name of the object
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets the Position
        /// </summary>
        public virtual Vector3 Position { get; set; }
        /// <summary>
        /// Gets the Trinity ObjectType
        /// </summary>
        public ObjectType ObjectType { get; set; }

        /// <summary>
        /// Gets the Distance to the center of the object
        /// </summary>
        public float CentreDistance { get; set; }

        /// <summary>
        /// Gets the distance to the radius of the object
        /// </summary>
        public float RadiusDistance { get; set; }

        /// <summary>
        /// Gets the Radius of the object
        /// </summary>
        public float Radius { get; set; }

        /// <summary>
        /// Gets the difference in Z axis of the object in relation to the Player
        /// </summary>
        public float ZDiff { get; set; }

        /// <summary>
        /// A reason, if any, that this was not further processed in the caching mechanism
        /// </summary>
        public virtual string IgnoreReason { get; set; }

        /// <summary>
        /// A more detailed reason, if any, that this was not processed in the caching mechanism
        /// </summary>
        public virtual string IgnoreSubStep { get; set; }

        /// <summary>
        /// Gets the original DiaObject
        /// </summary>
        /// <remarks>Accessing this will hit DB API/D3 memory</remarks>
        public DiaObject InternalObject { get; set; }

        /// <summary>
        /// Gets the ACD (CommonData) of the Object
        /// </summary>
        /// <remarks>Accessing this will hit DB API/D3 memory</remarks>
        public virtual ACD CommonData { get; set; }

        /// <summary>
        /// Gets the type of the cached object
        /// </summary>
        public CacheType CacheType { get; set; }

        public bool Equals(CacheObject other)
        {
            return this.RActorGuid == other.RActorGuid;
        }

    }


    internal class CacheOther : CacheObject
    {
        public CacheOther(int rActorGUID)
            : base(rActorGUID)
        {

        }

        public override CacheObject Clone()
        {
            return (CacheObject)this.MemberwiseClone();
        }
    }

    internal class CacheUnit : CacheObject
    {
        public CacheUnit(int rActorGUID)
            : base(rActorGUID)
        {
            InternalUnit = (DiaUnit)InternalObject;
        }

        /// <summary>
        /// Gets a DiaUnit
        /// </summary>
        public DiaUnit InternalUnit { get; set; }

        /// <summary>
        /// Gets the HitPoints of the Unit
        /// </summary>
        public double HitointsCurrent { get; set; }
        public double HitpointsCurrentPct { get; set; }
        public double HitpointsMax { get; set; }
        public double HitpointsMaxTotal { get; set; }

        /// <summary>
        /// The Current Animation as SNOAnim
        /// </summary>
        public SNOAnim CurrentAnimation { get; set; }
        /// <summary>
        /// MonsterSize 
        /// </summary>
        public MonsterSize MonsterSize { get; set; }

        public bool IsEliteRareUnique { get; set; }
        public bool IsElite { get; set; }
        public bool IsRare { get; set; }
        public bool IsUnique { get; set; }
        /// <summary>
        /// A minion of an Elite, Rare, or Unique Unit
        /// </summary>
        public bool IsMinion { get; set; }
        public bool IsBoss { get; set; }

        public bool IsTreasureGoblin { get; set; }

        public bool IsAttackable { get; set; }
        public bool IsHireling { get; set; }
        public bool IsBurrowed { get; set; }
        public bool IsDead { get; set; }
        public bool IsNPC { get; set; }
        public bool IsRooted { get; set; }
        public bool IsTownVendor { get; set; }
        public bool IsUninterruptible { get; set; }
        public bool IsUntargetable { get; set; }
        public int SummonedByACDId { get; set; }
        public int SummonedBySNO { get; set; }
        public int SummonerId { get; set; }

        /// <summary>
        /// intellq special sauce
        /// </summary>
        public bool ForceLeapAgainst { get; set; }

        public override CacheObject Clone()
        {
            return (CacheObject)this.MemberwiseClone();
        }

    }

    /// <summary>
    /// Cached Items
    /// </summary>
    internal class CacheItem : CacheObject
    {
        public CacheItem(int rActorGUID)
            : base(rActorGUID)
        {

        }
        /// <summary>
        /// Gets the DiaItem
        /// </summary>
        public DiaItem InternalItem { get; set; }
        public ItemType ItemType { get; set; }
        public ItemQuality ItemQuality { get; set; }
        public ItemType InternalItemType { get; set; }
        public FollowerType FollowerType { get; set; }
        public int ItemLevel { get; set; }
        public int GoldStackSize { get; set; }
        public bool IsOneHandedItem { get; set; }

        public override CacheObject Clone()
        {
            return (CacheObject)this.MemberwiseClone();
        }
    }

    /// <summary>
    /// Cached Gizmos
    /// </summary>
    internal class CacheGizmo : CacheObject
    {
        public CacheGizmo(int rActorGUID)
            : base(rActorGUID)
        {

        }
        /// <summary>
        /// Gets the DiaGizmo
        /// </summary>
        public DiaGizmo InternalGizmo { get; set; }
        public bool IsObstacle { get; set; }
        public GizmoType Gizmotype { get; set; }
        public bool IsBarricade { get; set; }
        public bool Isdestructible { get; set; }
        public bool IsGizmoDisabledByScript { get; set; }
        public bool IsDoorOpen { get; set; }
        public bool IsHealthWellReady { get; set; }
        public bool IsLootContainerOpen { get; set; }

        public override CacheObject Clone()
        {
            return (CacheObject)this.MemberwiseClone();
        }

    }

}
