using System;
using Zeta;
using Zeta.Common;
using Zeta.Internals.Actors;
using Zeta.Internals.SNO;
using System.Text.RegularExpressions;

namespace GilesTrinity.Cache
{
    [Flags]
    public enum CacheType
    {
        Object,
        Unit,
        Item,
        Gizmo,
        Avoidance,
        Other
    }

    internal abstract class CacheObject : IEquatable<CacheObject>
    {
        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="CacheObject" /> class.
        /// </summary>
        /// <param name="acdGuid">The Actor GUID.</param>
        //public CacheObject(int acdGuid)
        //{
        //    ACDGuid = acdGuid;
        //    Type = GObjectType.Unknown;
        //    CacheType = Cache.CacheType.Other;
        //    LastAccessDate = DateTime.UtcNow;
        //    LastRefreshDate = DateTime.Now;
        //}
        public CacheObject(DiaObject dia)
        {
            ACDGuid = dia.ACDGuid;
            CommonData = dia.CommonData;

            Type = GObjectType.Unknown;
            CacheType = Cache.CacheType.Other;
            LastAccessDate = DateTime.UtcNow;
            LastRefreshDate = DateTime.UtcNow;
            Name = dia.Name;
            ActorSNO = dia.ActorSNO;
            DynamicID = dia.CommonData.DynamicId;
            Position = dia.Position;
            CentreDistance = dia.Distance;
            CacheType = Cache.CacheType.Object;

            InternalObject = dia;

            if (InternalObject != null)
            {
                ZDiff = InternalObject.ZDiff;
                RActorGUID = InternalObject.RActorGuid;
            }
            
        }
        #endregion Constructors

        #region Properties

        public bool Stale
        {
            get;
            set;
        }
        /// <summary>
        /// Gets the RActorGUID.
        /// </summary>
        /// <value>The RActorGUID.</value>
        public int ACDGuid
        {
            get;
            private set;
        }

        public string InternalName
        {
            get;
            protected set;
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
        /// Gets or sets the last refreshed date.
        /// </summary>
        /// <value>The last access date.</value>
        public DateTime LastRefreshDate
        {
            get;
            set;
        }

        /// <summary>
        /// Gets or sets the object type.
        /// </summary>
        /// <value>The object type.</value>
        public GObjectType Type
        {
            get;
            set;
        }

        /// <summary>
        /// Gets the RActorGUID
        /// </summary>
        public int RActorGUID { get; set; }

        /// <summary>
        /// Gets the DynamicID
        /// </summary>
        public int DynamicID { get; set; }

        /// <summary>
        /// Gets the BalanceID
        /// </summary>
        public int GameBalanceID { get; set; }

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
        public Vector3 Position { get; set; }

        /// <summary>
        /// Gets the Distance to the center of the object
        /// </summary>
        public float CentreDistance { get; set; }

        /// <summary>
        /// Gets the distance to the radius of the object
        /// </summary>
        public abstract float RadiusDistance { get; set; }

        /// <summary>
        /// Gets the Radius of the object
        /// </summary>
        public abstract float Radius { get; set; }

        /// <summary>
        /// Gets the difference in Z axis of the object in relation to the Player
        /// </summary>
        public float ZDiff { get; set; }

        /// <summary>
        /// A reason, if any, that this was not further processed in the caching mechanism
        /// </summary>
        public abstract string IgnoreReason { get; set; }

        /// <summary>
        /// A more detailed reason, if any, that this was not processed in the caching mechanism
        /// </summary>
        public abstract string IgnoreSubStep { get; set; }

        /// <summary>
        /// Gets the original DiaObject
        /// </summary>
        /// <remarks>Accessing this will hit DB API/D3 memory</remarks>
        public DiaObject InternalObject { get; set; }

        /// <summary>
        /// Gets the ACD (CommonData) of the Object
        /// </summary>
        /// <remarks>Accessing this will hit DB API/D3 memory</remarks>
        public ACD CommonData { get; set; }

        /// <summary>
        /// Gets the type of the cached object
        /// </summary>
        public CacheType CacheType { get; set; }
        #endregion Properties

        protected readonly static Regex NameNumberRemover = new Regex(@"-\d+$", RegexOptions.Compiled);

        #region Methods
        /// <summary>
        /// Clones this instance.
        /// </summary>
        /// <returns>Cloned instance of <see cref="CacheObject"/>.</returns>
        public abstract CacheObject Clone();
        #endregion Methods

        public bool Equals(CacheObject other)
        {
            return this.ACDGuid == other.ACDGuid;
        }

    }


    internal class CacheOther : CacheObject
    {
        public CacheOther(DiaObject dia)
            : base(dia)
        {
            CacheType = CacheType.Other;
        }

        public override CacheObject Clone()
        {
            return (CacheObject)this.MemberwiseClone();
        }

        public override float RadiusDistance
        {
            get;
            set;
        }

        public override float Radius
        {
            get;
            set;
        }

        public override string IgnoreReason
        {
            get;
            set;
        }

        public override string IgnoreSubStep
        {
            get;
            set;
        }
    }


    /// <summary>
    /// Cached Items
    /// </summary>
    //internal class CacheItem2 : CacheObject
    //{
    //    public CacheItem2(ACD acd)
    //        : base(acd)
    //    {

    //    }
    //    /// <summary>
    //    /// Gets the DiaItem
    //    /// </summary>
    //    public DiaItem InternalItem { get; set; }
    //    public GItemType ItemType { get; set; }
    //    public ItemQuality ItemQuality { get; set; }
    //    public GItemType InternalItemType { get; set; }
    //    public FollowerType FollowerType { get; set; }
    //    public int ItemLevel { get; set; }
    //    public int GoldStackSize { get; set; }
    //    public bool IsOneHandedItem { get; set; }

    //    public override CacheObject Clone()
    //    {
    //        return (CacheObject)this.MemberwiseClone();
    //    }
    //}


}
