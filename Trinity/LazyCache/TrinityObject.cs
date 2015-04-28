using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Zeta.Bot.Navigation;
using Zeta.Common;
using Zeta.Game;
using Zeta.Game.Internals.Actors;
using Zeta.Game.Internals.SNO;
using Logger = Trinity.Technicals.Logger;

namespace Trinity.LazyCache
{
    /// <summary>
    /// Trinity's Cached version of DiaObject
    /// </summary>
    public class TrinityObject : CacheBase
    {
        public TrinityObject(ACD acd) : base(acd) { }

        #region Properties

        public string Name
        {
            get { return CacheManager.GetCacheValue(this, o => o.Object.Name); }
        }

        public string InternalName
        {
            get { return CacheManager.GetCacheValue(this, o => Trinity.NameNumberTrimRegex.Replace(o.Source.Name, "")); }
        }

        public float Distance
        {
            get { return CacheManager.GetCacheValue(this, o => o.Source.Distance, 50); }
        }

        public float RadiusDistance
        {
            get { return CacheManager.GetCacheValue(this, o => Math.Max(Distance - Radius, 0f), 50); }
        }

        public int ActorId
        {
            get { return CacheManager.GetCacheValue(this, o => o.Source.ActorSNO); }
        }

        public int ActorSNO
        {
            get { return CacheManager.GetCacheValue(this, o => ActorId); }
        }

        public Vector3 Position
        {
            get { return CacheManager.GetCacheValue(this, o => o.Unit.Position, 50); }
        }

        public float Radius
        {
            get { return CacheManager.GetCacheValue(this, o => o.Unit.CollisionSphere.Radius); }
        }

        public bool InLineOfSight
        {
            get { return CacheManager.GetCacheValue(this, o => IsInLineOfSight(), 200); }
        }

        public Sphere CollisionSphere
        {
            get { return CacheManager.GetCacheValue(this, o => o.Object.CollisionSphere); }
        }

        public int AffixId
        {
            get { return CacheManager.GetCacheValue(this, o => o.Source.AffixId); }
        }

        public int DynamicId
        {
            get { return CacheManager.GetCacheValue(this, o => o.Source.DynamicId); }
        }

        public int BalanceId
        {
            get { return CacheManager.GetCacheValue(this, o => o.Source.GameBalanceId); }
        }

        public int MinimapVisibilityFlags
        {
            get { return CacheManager.GetCacheValue(this, o => o.Source.MinimapVisibilityFlags); }
        }

        public int WorldDynamicId
        {
            get { return CacheManager.GetCacheValue(this, o => o.Source.WorldDynamicId); }
        }

        public SNORecordActor ActorInfo
        {
            get { return CacheManager.GetCacheValue(this, o => o.Source.ActorInfo); }
        }

        public ACDAnimationInfo AnimationInfo
        {
            get { return CacheManager.GetCacheValue(this, o => o.Source.AnimationInfo); }
        }

        public AnimationState AnimationState
        {
            get { return CacheManager.GetCacheValue(this, o => o.Source.AnimationState); }
        }

        public SNOAnim CurrentAnimation
        {
            get { return CacheManager.GetCacheValue(this, o => o.Source.CurrentAnimation); }
        }

        public GameBalanceType GameBalanceType
        {
            get { return CacheManager.GetCacheValue(this, o => o.Source.GameBalanceType); }
        }

        public GizmoType GizmoType
        {
            get { return CacheManager.GetCacheValue(this, o => o.Source.GizmoType); }
        }

        public TrinityObjectType Type
        {
            get { return CacheManager.GetCacheValue(this, o => CacheUtilities.GetTrinityObjectType(this)); }
        }

        public AvoidanceType AvoidanceType
        {
            get { return CacheManager.GetCacheValue(this, o => AvoidanceManager.GetAvoidanceType(ActorId)); }
        }

        public MarkerType MarkerType
        {
            get { return CacheManager.GetCacheValue(this, o => o.Source.MarkerType); }
        }

        public bool IsEventObject
        {
            get { return CacheManager.GetCacheValue(this, o => IsCursedChest || IsCursedShrine); }
        }

        public bool IsCursedChest
        {
            get { return CacheManager.GetCacheValue(this, o => Type == TrinityObjectType.CursedChest); }
        }  
      
        public bool IsCursedShrine
        {
            get { return CacheManager.GetCacheValue(this, o => Type == TrinityObjectType.CursedShrine); }
        }

        public bool IsBountyObjective
        {
            get { return CacheManager.GetCacheValue(this, o => o.Source.GetAttributeOrDefault<int>(ActorAttributeType.BountyObjective) != 0); }
        }

        public bool IsMinimapActive
        {
            get { return CacheManager.GetCacheValue(this, o => o.Source.GetAttributeOrDefault<int>(ActorAttributeType.MinimapActive) > 0); }
        }

        public bool IsUnit
        {
            get { return CacheManager.GetCacheValue(this, o => o.Type == TrinityObjectType.Unit); }
        }

        public bool IsGlobe
        {
            get { return CacheManager.GetCacheValue(this, o => o.Type == TrinityObjectType.HealthGlobe || o.Type == TrinityObjectType.ProgressionGlobe || o.Type == TrinityObjectType.PowerGlobe); }
        }
        
        public string ObjectHash
        {
            get { return CacheManager.GetCacheValue(this, CacheUtilities.GenerateObjectHash); }

        }

        public bool IsPickupNoClick
        {
            get { return CacheManager.GetCacheValue(this, o => DataDictionary.NoPickupClickTypes.Any(t => t == Type)); }
        }

        public bool IsInteractable
        {
            get { return CacheManager.GetCacheValue(this, o => DataDictionary.InteractableTypes.Any(t => t == Type)); }
        }

        public bool IsDestroyable
        {
            get { return CacheManager.GetCacheValue(this, o => DataDictionary.DestroyableTypes.Any(t => t == Type)); }
        }

        public bool ShouldIgnore { get; set; }

        public IgnoreReasonFlags IgnoreReasons { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Determines whether [is in line of sight].
        /// </summary>
        /// <returns><c>true</c> if [is in line of sight]; otherwise, <c>false</c>.</returns>
        public bool IsInLineOfSight()
        {
            return Navigator.Raycast(Trinity.Player.Position, Position);
        }

        #endregion

        public static implicit operator TrinityObject(ACD x)
        {
            return CacheFactory.CreateObject<TrinityObject>(x);
        }

    }
}
