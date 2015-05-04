using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Trinity.DbProvider;
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
        #region Constructors

        public TrinityObject() { }
        public TrinityObject(int acdGuid) : base(acdGuid) { }
        public TrinityObject(ACD acd) : base(acd) { }

        #endregion

        #region Fields

        private readonly CacheField<string> _internalName = new CacheField<string>();
        private readonly CacheField<int> _rActorGuid = new CacheField<int>();
        private readonly CacheField<Vector3> _position = new CacheField<Vector3>(UpdateSpeed.Ultra);
        private readonly CacheField<float> _distance = new CacheField<float>(UpdateSpeed.Fast);
        private readonly CacheField<int> _actorSNO = new CacheField<int>();
        private readonly CacheField<SNOAnim> _currentAnimation = new CacheField<SNOAnim>(UpdateSpeed.Fast);
        private readonly CacheField<bool> _isInLineOfSight = new CacheField<bool>(UpdateSpeed.Fast);
        private readonly CacheField<bool> _isNavigationObstacle = new CacheField<bool>();
        private readonly CacheField<float> _radiusDistance = new CacheField<float>();
        private readonly CacheField<float> _radius = new CacheField<float>();
        private readonly CacheField<Sphere> _collisionSphere = new CacheField<Sphere>();
        private readonly CacheField<MonsterType> _monsterType = new CacheField<MonsterType>();
        private readonly CacheField<int> _affixId = new CacheField<int>();
        private readonly CacheField<int> _dynamicId = new CacheField<int>();
        private readonly CacheField<int> _balanceId = new CacheField<int>();
        private readonly CacheField<int> _minimapVisibilityFlags = new CacheField<int>();
        private readonly CacheField<int> _worldDynamicId = new CacheField<int>();
        private readonly CacheField<SNORecordActor> _actorInfo = new CacheField<SNORecordActor>();
        private readonly CacheField<ACDAnimationInfo> _animationInfo = new CacheField<ACDAnimationInfo>();
        private readonly CacheField<AnimationState> _animationState = new CacheField<AnimationState>(UpdateSpeed.Fast);
        private readonly CacheField<GameBalanceType> _gameBalanceType = new CacheField<GameBalanceType>();
        private readonly CacheField<GizmoType> _gizmoType = new CacheField<GizmoType>();
        private readonly CacheField<AvoidanceType> _avoidanceType = new CacheField<AvoidanceType>();
        private readonly CacheField<MarkerType> _markerType = new CacheField<MarkerType>();
        private readonly CacheField<bool> _isEventObject = new CacheField<bool>();
        private readonly CacheField<bool> _isCursedChest = new CacheField<bool>();
        private readonly CacheField<bool> _isCursedShrine = new CacheField<bool>();
        private readonly CacheField<bool> _isBountyObjective = new CacheField<bool>();
        private readonly CacheField<bool> _isMinimapActive = new CacheField<bool>(UpdateSpeed.Slow);
        private readonly CacheField<bool> _isUnit = new CacheField<bool>();
        private readonly CacheField<bool> _isGlobe = new CacheField<bool>();
        private readonly CacheField<string> _objectHash = new CacheField<string>();
        private readonly CacheField<bool> _isPickupNoClick = new CacheField<bool>();
        private readonly CacheField<bool> _isInteractable = new CacheField<bool>();
        private readonly CacheField<bool> _isDetroyable = new CacheField<bool>();
        private readonly CacheField<bool> _isUntargetable = new CacheField<bool>();
        private readonly CacheField<bool> _isInvulnerable = new CacheField<bool>();
        private readonly CacheField<bool> _isBlocking = new CacheField<bool>(UpdateSpeed.Fast);
        private readonly CacheField<bool> _isNoDamage = new CacheField<bool>();
        private readonly CacheField<float> _weight = new CacheField<float>();
        private readonly CacheField<SNORecordMonster> _monsterInfo = new CacheField<SNORecordMonster>();

        #endregion

        #region Properties

        /// <summary>
        /// The user-friendly name shown in game
        /// </summary>
        public string Name
        {
            get { return _internalName.IsCacheValid ? _internalName.CachedValue : (_internalName.CachedValue = GetName(this)); }
            set { _internalName.SetValueOverride(value); }
        }

        /// <summary>
        /// Diablo's internal name
        /// </summary>
        public string InternalName
        {
            get { return _internalName.IsCacheValid ? _internalName.CachedValue : (_internalName.CachedValue = Trinity.NameNumberTrimRegex.Replace(Source.Name, "")); }
            set { _internalName.SetValueOverride(value); }
        }

        /// <summary>
        /// Unique identifier for the RActor list
        /// </summary>
        public int RActorGuid
        {
            get { return _rActorGuid.IsCacheValid ? _rActorGuid.CachedValue : (_rActorGuid.CachedValue = GetObjectProperty(x => x.RActorGuid)); }
            set { _rActorGuid.SetValueOverride(value); }
        }

        /// <summary>
        /// Current location
        /// </summary>
        public Vector3 Position
        {
            get { return _position.IsCacheValid ? _position.CachedValue : (_position.CachedValue = Source.Position); }
            set { _position.SetValueOverride(value); }
        }

        /// <summary>
        /// Distance from player
        /// </summary>
        public float Distance
        {
            get { return _distance.IsCacheValid ? _distance.CachedValue : (_distance.CachedValue = GetDistance(this)); }
            set { _distance.SetValueOverride(value); }
        }

        /// <summary>
        /// Unique identifier for the actor
        /// </summary>
        public int ActorSNO
        {
            get { return _actorSNO.IsCacheValid ? _actorSNO.CachedValue : (_actorSNO.CachedValue = Source.ActorSNO); }
            set { _actorSNO.SetValueOverride(value); }
        }

        /// <summary>
        /// Current animation being used by actor
        /// </summary>
        public SNOAnim CurrentAnimation
        {
            get { return _currentAnimation.IsCacheValid ? _currentAnimation.CachedValue : (_currentAnimation.CachedValue = Source.CurrentAnimation); }
            set { _currentAnimation.SetValueOverride(value); }
        }

        /// <summary>
        /// If player can see the actor (Navigator-Based Raycast)
        /// </summary>
        public bool IsInLineOfSight
        {
            get { return _isInLineOfSight.IsCacheValid ? _isInLineOfSight.CachedValue : (_isInLineOfSight.CachedValue = NavHelper.CanRayCast(Position)); }
            set { _isInLineOfSight.SetValueOverride(value); }
        }

        /// <summary>
        /// Distance from center of the actor to the widest point of its model
        /// </summary>
        public float Radius
        {
            get { return _radius.IsCacheValid ? _radius.CachedValue : (_radius.CachedValue = GetRadius(this)); }
            set { _radius.SetValueOverride(value); }
        }

        /// <summary>
        /// Distance from player to the edge of the actor's model.
        /// </summary>
        public float RadiusDistance
        {
            get { return _radiusDistance.IsCacheValid ? _radiusDistance.CachedValue : (_radiusDistance.CachedValue = GetRadiusDistance(this)); }
            set { _radiusDistance.SetValueOverride(value); }
        }

        /// <summary>
        /// Is actor listed as something that blocks navigation/movement
        /// </summary>
        public bool IsNavigationObstacle
        {
            get { return _isNavigationObstacle.IsCacheValid ? _isNavigationObstacle.CachedValue : (_isNavigationObstacle.CachedValue = DataDictionary.NavigationObstacleIds.Contains(ActorSNO)); }
            set { _isNavigationObstacle.SetValueOverride(value); }
        }

        /// <summary>
        /// Sphere around an actor that is used for collision detection by diablo3 (accessing properties reads directly from Diablo memory)
        /// </summary>
        public Sphere CollisionSphere
        {
            get { return _collisionSphere.IsCacheValid ? _collisionSphere.CachedValue : (_collisionSphere.CachedValue = Object.ActorInfo.Sphere); }
            set { _collisionSphere.SetValueOverride(value); }
        }

        /// <summary>
        /// DBs category of monster type, Human, Beast, Undead etc
        /// </summary>
        public MonsterType MonsterType
        {
            get { return _monsterType.IsCacheValid ? _monsterType.CachedValue : (_monsterType.CachedValue = GetMonsterType(this)); }
            set { _monsterType.SetValueOverride(value); }
        }

        /// <summary>
        /// No idea what this is [Update Me]
        /// </summary>
        public int AffixId
        {
            get { return _affixId.IsCacheValid ? _affixId.CachedValue : (_affixId.CachedValue = GetSourceProperty(x => x.AffixId)); }
            set { _affixId.SetValueOverride(value); }
        }

        /// <summary>
        /// A temporary Id required certain methods, ie. buying items from vendor.
        /// </summary>
        public int DynamicId
        {
            get { return _dynamicId.IsCacheValid ? _dynamicId.CachedValue : (_dynamicId.CachedValue = GetSourceProperty(x => x.DynamicId)); }
            set { _dynamicId.SetValueOverride(value); }
        }

        /// <summary>
        /// No idea what this is [Update Me]
        /// </summary>
        public int BalanceId
        {
            get { return _balanceId.IsCacheValid ? _balanceId.CachedValue : (_balanceId.CachedValue = GetSourceProperty(x => x.GameBalanceId)); }
            set { _balanceId.SetValueOverride(value); }
        }

        /// <summary>
        /// No idea what this is [Update Me]
        /// </summary>
        public int MinimapVisibilityFlags
        {
            get { return _minimapVisibilityFlags.IsCacheValid ? _minimapVisibilityFlags.CachedValue : (_minimapVisibilityFlags.CachedValue = GetSourceProperty(x => x.MinimapVisibilityFlags)); }
            set { _minimapVisibilityFlags.SetValueOverride(value); }
        }

        /// <summary>
        /// Temporary Id for the world actor is in (not to be confused with WorldId)
        /// </summary>
        public int WorldDynamicId
        {
            get { return _worldDynamicId.IsCacheValid ? _worldDynamicId.CachedValue : (_worldDynamicId.CachedValue = GetSourceProperty(x => x.WorldDynamicId)); }
            set { _worldDynamicId.SetValueOverride(value); }
        }

        /// <summary>
        /// Source for detailed actor info like collision Sphere, Bounds, GizmoType, ArtisanType etc (accessing properties reads directly from Diablo memory)
        /// </summary>
        public SNORecordActor ActorInfo
        {
            get { return _actorInfo.IsCacheValid ? _actorInfo.CachedValue : (_actorInfo.CachedValue = GetSourceProperty(x => x.ActorInfo)); }
            set { _actorInfo.SetValueOverride(value); }
        }

        /// <summary>
        /// Source for animation info like current animation, animation state (accessing properties reads directly from Diablo memory)
        /// </summary>
        public ACDAnimationInfo AnimationInfo
        {
            get { return _animationInfo.IsCacheValid ? _animationInfo.CachedValue : (_animationInfo.CachedValue = GetSourceProperty(x => x.AnimationInfo)); }
            set { _animationInfo.SetValueOverride(value); }
        }

        /// <summary>
        /// The current animation's category, eg. Idle, TakingDamage, Attacking, Casting etc.
        /// </summary>
        public AnimationState AnimationState
        {
            get { return _animationState.IsCacheValid ? _animationState.CachedValue : (_animationState.CachedValue = GetSourceProperty(x => x.AnimationState)); }
            set { _animationState.SetValueOverride(value); }
        }

        /// <summary>
        /// No idea what this is [Update Me]
        /// </summary>
        public GameBalanceType GameBalanceType
        {
            get { return _gameBalanceType.IsCacheValid ? _gameBalanceType.CachedValue : (_gameBalanceType.CachedValue = GetSourceProperty(x => x.GameBalanceType)); }
            set { _gameBalanceType.SetValueOverride(value); }
        }

        /// <summary>
        /// The kind of interactable - Chest, Portal, Switch, PlacedLoot etc
        /// </summary>
        public GizmoType GizmoType 
        {
            get { return _gizmoType.IsCacheValid ? _gizmoType.CachedValue : (_gizmoType.CachedValue = GetSourceProperty(x => x.GizmoType)); }
            set { _gizmoType.SetValueOverride(value); }
        }

        /// <summary>
        /// The kind of avoidance / elite affix - eg. Molten Ball, Plague, Frozen Pulse etc.
        /// </summary>
        public AvoidanceType AvoidanceType
        {
            get { return _avoidanceType.IsCacheValid ? _avoidanceType.CachedValue : (_avoidanceType.CachedValue = AvoidanceManager.GetAvoidanceType(ActorSNO)); }
            set { _avoidanceType.SetValueOverride(value); }
        }

        /// <summary>
        /// The minimap marker shape - eg. Asterisk, Exclamation, Question etc.
        /// </summary>
        public MarkerType MarkerType
        {
            get { return _markerType.IsCacheValid ? _markerType.CachedValue : (_markerType.CachedValue = GetSourceProperty(x => x.MarkerType)); }
            set { _markerType.SetValueOverride(value); }
        }

        /// <summary>
        /// The minimap marker shape - eg. Asterisk, Exclamation, Question etc.
        /// </summary>
        public bool IsEventObject
        {
            get { return _isEventObject.IsCacheValid ? _isEventObject.CachedValue : (_isEventObject.CachedValue = IsCursedChest || IsCursedShrine); }
            set { _isEventObject.SetValueOverride(value); }
        }

        /// <summary>
        /// Is the actor a cursed chest
        /// </summary>
        public bool IsCursedChest
        {
            get { return _isCursedChest.IsCacheValid ? _isCursedChest.CachedValue : (_isCursedChest.CachedValue = TrinityType == TrinityObjectType.CursedChest); }
            set { _isCursedChest.SetValueOverride(value); }
        }

        /// <summary>
        /// Is the actor a cursed shrine
        /// </summary>
        public bool IsCursedShrine
        {
            get { return _isCursedShrine.IsCacheValid ? _isCursedShrine.CachedValue : (_isCursedShrine.CachedValue = TrinityType == TrinityObjectType.CursedShrine); }
            set { _isCursedShrine.SetValueOverride(value); }
        }

        /// <summary>
        /// If the actor the objective for the current bounty quest
        /// </summary>
        public bool IsBountyObjective
        {
            get { return _isBountyObjective.IsCacheValid ? _isBountyObjective.CachedValue : (_isCursedShrine.CachedValue = Source.GetAttributeOrDefault<int>(ActorAttributeType.BountyObjective) > 0); }
            set { _isBountyObjective.SetValueOverride(value); }
        }

        /// <summary>
        /// Is the actor currently visible as a minimap marker
        /// </summary>
        public bool IsMinimapActive
        {
            get { return _isMinimapActive.IsCacheValid ? _isMinimapActive.CachedValue : (_isMinimapActive.CachedValue = ActorType == ActorType.Monster && Source.GetAttributeOrDefault<int>(ActorAttributeType.MinimapActive) > 0); }
            set { _isMinimapActive.SetValueOverride(value); }
        }

        /// <summary>
        /// Is the actor a unit
        /// </summary>
        public bool IsUnit
        {
            get { return _isUnit.IsCacheValid ? _isUnit.CachedValue : (_isUnit.CachedValue = TrinityType == TrinityObjectType.Unit); }
            set { _isUnit.SetValueOverride(value); }
        }

        /// <summary>
        /// Is the actor a globe of any variety
        /// </summary>
        public bool IsGlobe
        {
            get { return _isGlobe.IsCacheValid ? _isGlobe.CachedValue : (_isGlobe.CachedValue = TrinityType == TrinityObjectType.HealthGlobe || TrinityType == TrinityObjectType.ProgressionGlobe || TrinityType == TrinityObjectType.PowerGlobe); }
            set { _isGlobe.SetValueOverride(value); }
        }

        /// <summary>
        /// A unique identifier caclulated from the actors position and other properties
        /// </summary>
        public string ObjectHash
        {
            get { return _objectHash.IsCacheValid ? _objectHash.CachedValue : (_objectHash.CachedValue = GenerateObjectHash(this)); }
            set { _objectHash.SetValueOverride(value); }
        }

        /// <summary>
        /// If can be picked up just by walking near to it
        /// </summary>
        public bool IsPickupNoClick
        {
            get { return _isPickupNoClick.IsCacheValid ? _isPickupNoClick.CachedValue : (_isPickupNoClick.CachedValue = DataDictionary.NoPickupClickTypes.Any(t => t == TrinityType)); }
            set { _isPickupNoClick.SetValueOverride(value); }
        }

        /// <summary>
        /// If can be interacted with
        /// </summary>
        public bool IsInteractable
        {
            get { return _isInteractable.IsCacheValid ? _isInteractable.CachedValue : (_isInteractable.CachedValue = DataDictionary.InteractableTypes.Any(t => t == TrinityType)); }
            set { _isInteractable.SetValueOverride(value); }
        }

        /// <summary>
        /// If can be destroyed
        /// </summary>
        public bool IsDestroyable
        {
            get { return _isDetroyable.IsCacheValid ? _isDetroyable.CachedValue : (_isDetroyable.CachedValue = DataDictionary.DestroyableTypes.Any(t => t == TrinityType)); }
            set { _isDetroyable.SetValueOverride(value); }
        }

        /// <summary>
        /// If can't be targetted with spells etc
        /// </summary>
        public bool IsUntargetable
        {
            get { return _isUntargetable.IsCacheValid ? _isUntargetable.CachedValue : (_isUntargetable.CachedValue = Source.GetAttributeOrDefault<int>(ActorAttributeType.Untargetable) > 0); }
            set { _isUntargetable.SetValueOverride(value); }
        }

        /// <summary>
        /// If can't be damaged
        /// </summary>
        public bool IsInvulnerable
        {
            get { return _isInvulnerable.IsCacheValid ? _isInvulnerable.CachedValue : (_isInvulnerable.CachedValue = Source.GetAttributeOrDefault<int>(ActorAttributeType.Invulnerable) > 0); }
            set { _isInvulnerable.SetValueOverride(value); }
        }

        /// <summary>
        /// If the actor is right in front of us and we can't move
        /// </summary>
        public bool IsBlocking
        {
            get { return _isBlocking.IsCacheValid ? _isBlocking.CachedValue : (_isBlocking.CachedValue = RadiusDistance <= 3f && PlayerMover.GetMovementSpeed() <= 0); }
            set { _isBlocking.SetValueOverride(value); }
        }

        /// <summary>
        /// If can't deal any damage
        /// </summary>
        public bool IsNoDamage
        {
            get { return _isNoDamage.IsCacheValid ? _isNoDamage.CachedValue : (_isNoDamage.CachedValue = Source.GetAttributeOrDefault<int>(ActorAttributeType.NoDamage) > 0); }
            set { _isNoDamage.SetValueOverride(value); }
        }

        /// <summary>
        /// For targetting logic, reason(s) why actor should be ignored
        /// </summary>
        public List<IgnoreReason> IgnoreReasons = new List<IgnoreReason>();

        /// <summary>
        /// If there are any ignore reasons presently
        /// </summary>
        public bool IsIgnored
        {
            get { return IgnoreReasons.Any(); }
        }

        /// <summary>
        /// For targetting, degree of importance given the current combat situation
        /// </summary>
        public float Weight
        {
            get { return _weight.IsCacheValid ? _weight.CachedValue : (_weight.CachedValue = Weighting.CalculateWeight(this, out WeightFactors)); }
            set { _weight.SetValueOverride(value); }
        }

        /// <summary>
        /// The individual factors that contribute to a final weight
        /// </summary>
        public List<Weighting.Weight> WeightFactors = new List<Weighting.Weight>();

        #endregion

        #region Methods

        /// <summary>
        /// Generates an SHA1 hash of a particular CacheObject
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public static string GenerateObjectHash(TrinityObject obj)
        {
            using (MD5 md5 = MD5.Create())
            {
                string objHashBase;
                if (obj.TrinityType == TrinityObjectType.Unit)
                    objHashBase = obj.ActorSNO + obj.InternalName + obj.Position + obj.TrinityType + Trinity.CurrentWorldDynamicId;
                else if (obj.TrinityType == TrinityObjectType.Item && obj is TrinityItem)
                {
                    var objItem = (TrinityItem)obj;
                    return HashGenerator.GenerateItemHash(obj.Position, obj.ActorSNO, obj.InternalName, Trinity.CurrentWorldId, objItem.ItemQuality, objItem.Level);
                }
                else
                    objHashBase = String.Format("{0}{1}{2}{3}", obj.ActorSNO, obj.Position, obj.TrinityType, Trinity.CurrentWorldDynamicId);

                string objHash = HashGenerator.GetMd5Hash(md5, objHashBase);
                return objHash;
            }
        }

        /// <summary>
        /// Trinity's actor type.
        /// </summary>
        internal static TrinityObjectType GetTrinityObjectType(ACD acd)
        {
            var id = acd.ActorSNO;
            var snoActor = (SNOActor)id;
            var actorType = acd.ActorType;
            var internalName = acd.Name;
            var gizmoType = acd.GizmoType;

            if (actorType == ActorType.Item || DataDictionary.ForceToItemOverrideIds.Contains(id))
                return TrinityObjectType.Item;

            if (actorType == ActorType.Monster)
                return TrinityObjectType.Unit;

            if (internalName.Contains("CursedChest"))
                return TrinityObjectType.CursedChest;

            if (internalName.Contains("CursedShrine"))
                return TrinityObjectType.CursedShrine;

            if (DataDictionary.Shrines.Any(s => s == snoActor))
                return TrinityObjectType.Shrine;

            if (internalName.ToLower().StartsWith("gold"))
                return TrinityObjectType.Gold;

            if (DataDictionary.InteractWhiteListIds.Contains(id))
                return TrinityObjectType.Interactable;

            if (AvoidanceManager.GetAvoidanceType(id) != AvoidanceType.None)
                return TrinityObjectType.Avoidance;

            if (actorType == ActorType.Gizmo)
            {
                switch (gizmoType)
                {
                    case GizmoType.HealingWell:
                        return TrinityObjectType.HealthWell;

                    case GizmoType.Door:
                        return TrinityObjectType.Door;

                    case GizmoType.BreakableDoor:
                        return TrinityObjectType.Barricade;

                    case GizmoType.PoolOfReflection:
                    case GizmoType.PowerUp:
                        return TrinityObjectType.Shrine;

                    case GizmoType.Chest:
                        return TrinityObjectType.Container;

                    case GizmoType.DestroyableObject:
                    case GizmoType.BreakableChest:
                        return TrinityObjectType.Destructible;

                    case GizmoType.PlacedLoot:
                    case GizmoType.Switch:
                    case GizmoType.Headstone:
                        return TrinityObjectType.Interactable;

                    case GizmoType.Portal:
                        return TrinityObjectType.Portal;
                }
            }

            if (acd.ActorType == ActorType.Player)
                return TrinityObjectType.Player;

            if (internalName.StartsWith("Banner_Player"))
                return TrinityObjectType.Banner;

            if (internalName.StartsWith("Waypoint-"))
                return TrinityObjectType.Waypoint;

            return TrinityObjectType.Unknown;
        }

        /// <summary>
        /// If name in our list of actors we never care about.
        /// </summary>
        internal static bool IsIgnoredName(string name)
        {
            name = name.ToLower();
            return DataDictionary.ActorIgnoreNameParts.Any(ignoreName => name.Contains(ignoreName));
        }

        /// <summary>
        /// Is ActorSNO in our DataDictionary lists of avoidances.
        /// </summary>
        internal static bool IsAvoidanceSNO(int actorSNO)
        {
            return DataDictionary.Avoidances.Contains(actorSNO) || DataDictionary.ButcherFloorPanels.Contains(actorSNO) || DataDictionary.AvoidanceProjectiles.Contains(actorSNO);
        }

        /// <summary>
        /// Faster distance call than ACD.Distance due to being 2d and calling cached player position.
        /// </summary>
        internal static float GetDistance(TrinityObject o)
        {
            var pos = o.Position;

            // Distance on Ground Items must be called on the ACDItem/DiaItem (not the ACD)
            if (o.ActorType == ActorType.Item && o.Source is ACDItem)
                pos = o.GetDiaItemProperty(x => x.Position);

            return pos == Vector3.Zero ? 0f : pos.Distance2D(CacheManager.Me.Position);
        }

        /// <summary>
        /// The radius, checking for overrides in DataDictionary.
        /// </summary>
        internal static float GetRadius(TrinityObject o)
        {
            float radius;
            if (o.IsNavigationObstacle && DataDictionary.CustomObjectRadius.TryGetValue(o.ActorSNO, out radius))
                return radius;

            return o.CollisionSphere.Radius;
        }

        /// <summary>
        /// The monster type, checking for overrides in DataDictionary.
        /// </summary>
        internal static MonsterType GetMonsterType(TrinityObject o)
        {
            MonsterType monsterTypeOverride;
            return DataDictionary.MonsterTypeOverrides.TryGetValue(o.ActorSNO, out monsterTypeOverride) ? monsterTypeOverride : o.GetSourceProperty(x => x.MonsterInfo.MonsterType);
        }

        /// <summary>
        /// The radius distance, checking for overrides in DataDictionary.
        /// </summary>
        public static float GetRadiusDistance(TrinityObject o)
        {
            if (o.TrinityType != TrinityObjectType.Destructible)
                return Math.Max(o.Distance - o.Radius, 0f);

            float maxRadiusDistance;
            return DataDictionary.DestructableObjectRadius.TryGetValue(o.ActorSNO, out maxRadiusDistance) ? maxRadiusDistance : Trinity.Settings.WorldObject.DestructibleRange;
        }

        /// <summary>
        /// The type of player-summoned actor this is.
        /// </summary>
        internal static SummonType GetSummonType(TrinityObject o)
        {
            if (DataDictionary.MysticAllyIds.Contains(o.ActorSNO))
                return SummonType.MysticAlly;

            if (DataDictionary.DemonHunterPetIds.Contains(o.ActorSNO))
                return SummonType.Companion;

            if (DataDictionary.DemonHunterSentryIds.Contains(o.ActorSNO))
                return SummonType.Sentry;

            if (DataDictionary.WizardHydraIds.Contains(o.ActorSNO))
                return SummonType.Hydra;

            if (DataDictionary.GargantuanIds.Contains(o.ActorSNO))
                return SummonType.Gargantuan;

            if (DataDictionary.ZombieDogIds.Contains(o.ActorSNO))
                return SummonType.ZombieDog;

            return SummonType.Unknown;
        }

        /// <summary>
        /// The difference in height (Z-Axis) of two positions
        /// </summary>
        internal static float GetZDiff(Vector3 position)
        {
            return position != Vector3.Zero ? Math.Abs(CacheManager.Me.Position.Z - position.Z) : 0f;
        }

        /// <summary>
        /// The friendly name (the name you see in the Diablo interface)
        /// </summary>
        internal static string GetName(TrinityObject o)
        {
            if (o.Source is ACDItem && o.Item != null)
                return o.Item.Name;

            return o.InternalName;
        }

        /// <summary>
        /// Soruce for monster information
        /// </summary>
        public SNORecordMonster MonsterInfo
        {
            get { return _monsterInfo.IsCacheValid ? _monsterInfo.CachedValue : (_monsterInfo.CachedValue = GetSourceProperty(x => x.MonsterInfo)); }
            set { _monsterInfo.SetValueOverride(value); }
        }

        #endregion

        #region Cast Conversions

        public static implicit operator TrinityObject(ACD x)
        {
            return CacheFactory.CreateObject<TrinityObject>(x);
        }

        #endregion
    }
}
