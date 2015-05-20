using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Trinity.Combat;
using Trinity.Combat.Weighting;
using Trinity.DbProvider;
using Trinity.Helpers;
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

        public TrinityObject(DiaObject rActor) : base(rActor) {}

        #endregion

        #region Fields

        private readonly CacheField<int> _monsterSNO = new CacheField<int>();
        private readonly CacheField<Vector3> _position = new CacheField<Vector3>(UpdateSpeed.RealTime);
        private readonly CacheField<float> _distance = new CacheField<float>(UpdateSpeed.Fast);
        private readonly CacheField<SNOAnim> _currentAnimation = new CacheField<SNOAnim>(UpdateSpeed.Ultra);
        private readonly CacheField<AnimationState> _currentAnimationState = new CacheField<AnimationState>(UpdateSpeed.Ultra);
        private readonly CacheField<bool> _isInLineOfSight = new CacheField<bool>(UpdateSpeed.Fast);
        private readonly CacheField<bool> _isNavigationObstacle = new CacheField<bool>();
        private readonly CacheField<bool> _isOwnedByPlayer = new CacheField<bool>();
        private readonly CacheField<bool> _isSpawner = new CacheField<bool>();
        private readonly CacheField<float> _radiusDistance = new CacheField<float>(UpdateSpeed.Fast);
        private readonly CacheField<float> _radius = new CacheField<float>();
        private readonly CacheField<Sphere> _collisionSphere = new CacheField<Sphere>();
        private readonly CacheField<MonsterType> _monsterType = new CacheField<MonsterType>();
        private readonly CacheField<int> _affixId = new CacheField<int>();
        private readonly CacheField<int> _dynamicId = new CacheField<int>();
        private readonly CacheField<int> _balanceId = new CacheField<int>();
        private readonly CacheField<int> _minimapVisibilityFlags = new CacheField<int>();
        private readonly CacheField<int> _worldDynamicId = new CacheField<int>();
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
        private readonly CacheField<bool> _isShrine = new CacheField<bool>();
        private readonly CacheField<bool> _isMinimapActive = new CacheField<bool>(UpdateSpeed.VerySlow);
        private readonly CacheField<bool> _isUnit = new CacheField<bool>();
        private readonly CacheField<bool> _isGizmo = new CacheField<bool>();
        private readonly CacheField<bool> _isGlobe = new CacheField<bool>();
        private readonly CacheField<string> _objectHash = new CacheField<string>();
        private readonly CacheField<string> _internalNameLowerCase = new CacheField<string>();
        private readonly CacheField<string> _name = new CacheField<string>();
        private readonly CacheField<bool> _isPickupNoClick = new CacheField<bool>();
        private readonly CacheField<bool> _isInteractable = new CacheField<bool>();
        private readonly CacheField<bool> _isDetroyable = new CacheField<bool>();
        private readonly CacheField<bool> _isUntargetable = new CacheField<bool>();
        private readonly CacheField<bool> _isInvulnerable = new CacheField<bool>();
        private readonly CacheField<bool> _isBlocking = new CacheField<bool>(UpdateSpeed.Fast);
        private readonly CacheField<bool> _isNoDamage = new CacheField<bool>();
        private readonly CacheField<double> _weight = new CacheField<double>(UpdateSpeed.Normal);
        private readonly CacheField<bool> _isMe = new CacheField<bool>();
        private readonly CacheField<bool> _isItem = new CacheField<bool>();
        private double _weightTime;
        private readonly TrinityMovement _movement = new TrinityMovement();

        #endregion

        #region Properties

        /// <summary>
        /// The user-friendly name shown in game
        /// </summary>
        public string Name
        {
            get
            {
                if (_name.IsCacheValid) return _name.CachedValue;
                return _name.CachedValue = GetName(this);
            }
            set { _name.SetValueOverride(value); }
        }

        /// <summary>
        /// Diablo's internal name, converted to lower case
        /// </summary>
        public string InternalNameLowerCase
        {
            get
            {
                if (_internalNameLowerCase.IsCacheValid) return _internalNameLowerCase.CachedValue;
                return _internalNameLowerCase.CachedValue = InternalName.ToLowerInvariant();
            }
            set { _internalNameLowerCase.SetValueOverride(value); }
        }

        /// <summary>
        /// SNOMonster id, used for MonsterInfo record in SNOTable
        /// </summary>
        public int MonsterSNO
        {
            get
            {
                if (_monsterSNO.IsCacheValid) return _monsterSNO.CachedValue;
                return _monsterSNO.CachedValue = ActorMeta.MonsterSNO;
            }
            set { _monsterSNO.SetValueOverride(value); }
        }

        /// <summary>
        /// Current location
        /// </summary>
        public Vector3 Position
        {
            get
            {
                if (_position.IsCacheValid) return _position.CachedValue;
                
                var newPos = GetPosition(this);

                if ((int) newPos.X != (int) _position.CachedValue.X || (int) newPos.Y != (int) _position.CachedValue.Y)
                    OnPropertyChanged();

                return _position.CachedValue = newPos;
            }
            set { _position.SetValueOverride(value); }
        }

        /// <summary>
        /// Distance from player
        /// </summary>
        public float Distance
        {
            get
            {
                if (_distance.IsCacheValid) return _distance.CachedValue;
                return _distance.CachedValue = GetDistance(this);
            }
            set { _distance.SetValueOverride(value); }
        }

        /// <summary>
        /// Current animation being used by actor
        /// </summary>
        public SNOAnim CurrentAnimation
        {
            get
            {
                if (_currentAnimation.IsCacheValid) return _currentAnimation.CachedValue;
                return _currentAnimation.CachedValue = Source.CurrentAnimation;
            }
            set { _currentAnimation.SetValueOverride(value); }
        }

        /// <summary>
        /// Current animation being used by actor
        /// </summary>
        public AnimationState CurrentAnimationState
        {
            get
            {
                if (_currentAnimationState.IsCacheValid) return _currentAnimationState.CachedValue;
                return _currentAnimationState.CachedValue = Source.AnimationState;
            }
            set { _currentAnimationState.SetValueOverride(value); }
        }

        /// <summary>
        /// If player can see the actor (Navigator-Based Raycast)
        /// </summary>
        public bool IsInLineOfSight
        {
            get
            {
                if (_isInLineOfSight.IsCacheValid) return _isInLineOfSight.CachedValue;
                return _isInLineOfSight.CachedValue = NavHelper.CanRayCast(Position);
            }
            set { _isInLineOfSight.SetValueOverride(value); }
        }

        /// <summary>
        /// Source for movement related information
        /// </summary>
        public TrinityMovement Movement
        {
            get { return _movement; }
        }

        /// <summary>
        /// Distance from center of the actor to the widest point of its model
        /// </summary>
        public float Radius
        {
            get
            {
                if (_radius.IsCacheValid) return _radius.CachedValue;
                return _radius.CachedValue = GetRadius(this);
            }
            set { _radius.SetValueOverride(value); }
        }

        /// <summary>
        /// Distance from player to the edge of the actor's model.
        /// </summary>
        public float RadiusDistance
        {
            get
            {
                if (_radiusDistance.IsCacheValid) return _radiusDistance.CachedValue;
                return _radiusDistance.CachedValue = GetRadiusDistance(this);
            }
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
        /// Is actor listed as something that blocks navigation/movement
        /// </summary>
        public bool IsOwnedByPlayer
        {
            get
            {
                if (_isOwnedByPlayer.IsCacheValid) return _isOwnedByPlayer.CachedValue;
                return _isOwnedByPlayer.CachedValue = DataDictionary.PlayerOwnedSNO.Contains(ActorSNO);
            }
            set { _isOwnedByPlayer.SetValueOverride(value); }
        }

        /// <summary>
        /// If actor spawns units
        /// </summary>
        public bool IsSpawner
        {
            get
            {
                if (_isSpawner.IsCacheValid) return _isSpawner.CachedValue;
                return _isSpawner.CachedValue = DataDictionary.SpawnerSNO.Contains(ActorSNO);
            }
            set { _isSpawner.SetValueOverride(value); }
        }

        /// <summary>
        /// DBs category of monster type, Human, Beast, Undead etc
        /// </summary>
        public MonsterType MonsterType
        {
            get
            {
                if (_monsterType.IsCacheValid) return _monsterType.CachedValue;
                return _monsterType.CachedValue = GetMonsterType(this);
            }
            set { _monsterType.SetValueOverride(value); }
        }

        /// <summary>
        /// No idea what this is [Update Me]
        /// </summary>
        public int AffixId
        {
            get
            {
                if (_affixId.IsCacheValid) return _affixId.CachedValue;
                return _affixId.CachedValue = GetSourceProperty(x => x.AffixId);
            }
            set { _affixId.SetValueOverride(value); }
        }

        /// <summary>
        /// A temporary Id required certain methods, ie. buying items from vendor.
        /// </summary>
        public int DynamicId
        {
            get
            {
                if (_dynamicId.IsCacheValid) return _dynamicId.CachedValue;
                return _dynamicId.CachedValue = GetSourceProperty(x => x.DynamicId);
            }
            set { _dynamicId.SetValueOverride(value); }
        }

        /// <summary>
        /// No idea what this is [Update Me]
        /// </summary>
        public int BalanceId
        {
            get
            {
                if (_balanceId.IsCacheValid) return _balanceId.CachedValue;
                return _balanceId.CachedValue = GetSourceProperty(x => x.GameBalanceId);
            }
            set { _balanceId.SetValueOverride(value); }
        }

        /// <summary>
        /// No idea what this is [Update Me]
        /// </summary>
        public int MinimapVisibilityFlags
        {
            get
            {
                if (_minimapVisibilityFlags.IsCacheValid) return _minimapVisibilityFlags.CachedValue;
                return _minimapVisibilityFlags.CachedValue = GetSourceProperty(x => x.MinimapVisibilityFlags);
            }
            set { _minimapVisibilityFlags.SetValueOverride(value); }
        }

        /// <summary>
        /// Temporary Id for the world actor is in (not to be confused with WorldId)
        /// </summary>
        public int WorldDynamicId
        {
            get
            {
                if (_worldDynamicId.IsCacheValid) return _worldDynamicId.CachedValue;
                return _worldDynamicId.CachedValue = GetSourceProperty(x => x.WorldDynamicId);
            }
            set { _worldDynamicId.SetValueOverride(value); }
        }

        /// <summary>
        /// Source for animation info like current animation, animation state (accessing properties reads directly from Diablo memory)
        /// </summary>
        public ACDAnimationInfo AnimationInfo
        {
            get
            {
                if (_animationInfo.IsCacheValid) return _animationInfo.CachedValue;
                return _animationInfo.CachedValue = GetSourceProperty(x => x.AnimationInfo);
            }
            set { _animationInfo.SetValueOverride(value); }
        }

        /// <summary>
        /// The current animation's category, eg. Idle, TakingDamage, Attacking, Casting etc.
        /// </summary>
        public AnimationState AnimationState
        {
            get
            {
                if (_animationState.IsCacheValid) return _animationState.CachedValue;
                return _animationState.CachedValue = GetSourceProperty(x => x.AnimationState);
            }
            set { _animationState.SetValueOverride(value); }
        }

        /// <summary>
        /// No idea what this is [Update Me]
        /// </summary>
        public GameBalanceType GameBalanceType
        {
            get
            {
                if (_gameBalanceType.IsCacheValid) return _gameBalanceType.CachedValue;
                return _gameBalanceType.CachedValue = GetSourceProperty(x => x.GameBalanceType);
            }
            set { _gameBalanceType.SetValueOverride(value); }
        }

        /// <summary>
        /// The kind of interactable - Chest, Portal, Switch, PlacedLoot etc
        /// </summary>
        public GizmoType GizmoType 
        {
            get
            {
                if (_gizmoType.IsCacheValid) return _gizmoType.CachedValue;
                return _gizmoType.CachedValue = ActorMeta.GizmoType;
            }
            set { _gizmoType.SetValueOverride(value); }
        }

        /// <summary>
        /// The kind of avoidance / elite affix - eg. Molten Ball, Plague, Frozen Pulse etc.
        /// </summary>
        public AvoidanceType AvoidanceType
        {
            get
            {
                if (_avoidanceType.IsCacheValid) return _avoidanceType.CachedValue;
                return _avoidanceType.CachedValue = AvoidanceManager.GetAvoidanceType(ActorSNO);
            }
            set { _avoidanceType.SetValueOverride(value); }
        }

        /// <summary>
        /// The minimap marker shape - eg. Asterisk, Exclamation, Question etc.
        /// </summary>
        public MarkerType MarkerType
        {
            get
            {
                if (_markerType.IsCacheValid) return _markerType.CachedValue;
                return _markerType.CachedValue = GetSourceProperty(x => x.MarkerType);
            }
            set { _markerType.SetValueOverride(value); }
        }

        /// <summary>
        /// The minimap marker shape - eg. Asterisk, Exclamation, Question etc.
        /// </summary>
        public bool IsEventObject
        {
            get
            {
                if (_isEventObject.IsCacheValid) return _isEventObject.CachedValue;
                return _isEventObject.CachedValue = IsCursedChest || IsCursedShrine;
            }
            set { _isEventObject.SetValueOverride(value); }
        }

        /// <summary>
        /// Is the actor a cursed chest
        /// </summary>
        public bool IsCursedChest
        {
            get
            {
                if (_isCursedChest.IsCacheValid) return _isCursedChest.CachedValue;
                return _isCursedChest.CachedValue = TrinityType == TrinityObjectType.CursedChest;
            }
            set { _isCursedChest.SetValueOverride(value); }
        }

        /// <summary>
        /// Is the actor a cursed shrine
        /// </summary>
        public bool IsCursedShrine
        {
            get
            {
                if (_isCursedShrine.IsCacheValid) return _isCursedShrine.CachedValue;
                return _isCursedShrine.CachedValue = TrinityType == TrinityObjectType.CursedShrine || TrinityType == TrinityObjectType.HealthWell || TrinityType == TrinityObjectType.Shrine;
            }
            set { _isCursedShrine.SetValueOverride(value); }
        }

        /// <summary>
        /// Is any kind of shrine
        /// </summary>
        public bool IsShrine
        {
            get
            {
                if (_isCursedShrine.IsCacheValid) return _isCursedShrine.CachedValue;
                return _isCursedShrine.CachedValue = TrinityType == TrinityObjectType.CursedShrine;
            }
            set { _isCursedShrine.SetValueOverride(value); }
        }

        /// <summary>
        /// Is any kind of shrine
        /// </summary>
        public bool IsItem
        {
            get
            {
                if (_isItem.IsCacheValid) return _isItem.CachedValue;
                return _isItem.CachedValue = TrinityType == TrinityObjectType.Item;
            }
            set { _isItem.SetValueOverride(value); }
        }

        /// <summary>
        /// If the actor the objective for the current bounty quest
        /// </summary>
        public bool IsBountyObjective
        {
            get
            {
                if (_isBountyObjective.IsCacheValid) return _isBountyObjective.CachedValue;
                return _isBountyObjective.CachedValue = CacheManager.Me.ActiveBounty != null && Source.GetAttributeOrDefault<int>(ActorAttributeType.BountyObjective) > 0;
            }
            set { _isBountyObjective.SetValueOverride(value); }
        }

        /// <summary>
        /// Is the actor currently visible as a minimap marker
        /// </summary>
        public bool IsMinimapActive
        {
            get
            {
                if (_isMinimapActive.IsCacheValid) return _isMinimapActive.CachedValue;
                return _isMinimapActive.CachedValue = ActorType == ActorType.Monster && Source.GetAttributeOrDefault<int>(ActorAttributeType.MinimapActive) > 0;
            }
            set { _isMinimapActive.SetValueOverride(value); }
        }

        /// <summary>
        /// Is the actor a unit
        /// </summary>
        public bool IsUnit
        {
            get
            {
                if (_isUnit.IsCacheValid) return _isUnit.CachedValue;
                return _isUnit.CachedValue = ActorMeta.IsUnit;
            }
            set { _isUnit.SetValueOverride(value); }
        }

        /// <summary>
        /// Is the actor a gizmo
        /// </summary>
        public bool IsGizmo
        {
            get
            {
                if (_isGizmo.IsCacheValid) return _isGizmo.CachedValue;
                return _isGizmo.CachedValue = ActorMeta.IsGizmo;
            }
            set { _isGizmo.SetValueOverride(value); }
        }

        /// <summary>
        /// Is the actor a globe of any variety
        /// </summary>
        public bool IsGlobe
        {
            get
            {
                if (_isGlobe.IsCacheValid) return _isGlobe.CachedValue;
                return _isGlobe.CachedValue = TrinityType == TrinityObjectType.HealthGlobe || TrinityType == TrinityObjectType.ProgressionGlobe || TrinityType == TrinityObjectType.PowerGlobe;
            }
            set { _isGlobe.SetValueOverride(value); }
        }

        /// <summary>
        /// If can be picked up just by walking near to it
        /// </summary>
        public bool IsPickupNoClick
        {
            get
            {
                if (_isPickupNoClick.IsCacheValid) return _isPickupNoClick.CachedValue;
                return _isPickupNoClick.CachedValue = DataDictionary.NoPickupClickTypes.Any(t => t == TrinityType);
            }
            set { _isPickupNoClick.SetValueOverride(value); }
        }

        /// <summary>
        /// If can be interacted with
        /// </summary>
        public bool IsInteractable
        {
            get
            {
                if (_isInteractable.IsCacheValid) return _isInteractable.CachedValue;
                return _isInteractable.CachedValue = DataDictionary.InteractableTypes.Any(t => t == TrinityType);
            }
            set { _isInteractable.SetValueOverride(value); }
        }

        /// <summary>
        /// If can be destroyed
        /// </summary>
        public bool IsDestroyable
        {
            get
            {
                if (_isDetroyable.IsCacheValid) return _isDetroyable.CachedValue;
                return _isDetroyable.CachedValue = DataDictionary.DestroyableTypes.Any(t => t == TrinityType);
            }
            set { _isDetroyable.SetValueOverride(value); }
        }

        /// <summary>
        /// If can't be targetted with spells etc
        /// </summary>
        public bool IsUntargetable
        {
            get
            {
                if (_isUntargetable.IsCacheValid) return _isUntargetable.CachedValue;
                return _isUntargetable.CachedValue = Source.GetAttributeOrDefault<int>(ActorAttributeType.Untargetable) > 0;
            }
            set { _isUntargetable.SetValueOverride(value); }
        }

        /// <summary>
        /// If can't be damaged
        /// </summary>
        public bool IsInvulnerable
        {
            get
            {
                if (_isInvulnerable.IsCacheValid) return _isInvulnerable.CachedValue;
                return _isInvulnerable.CachedValue = Source.GetAttributeOrDefault<int>(ActorAttributeType.Invulnerable) > 0;
            }
            set { _isInvulnerable.SetValueOverride(value); }
        }

        /// <summary>
        /// If the actor is right in front of us and we can't move
        /// </summary>
        public bool IsBlocking
        {
            get
            {
                if (_isBlocking.IsCacheValid) return _isBlocking.CachedValue;
                return _isBlocking.CachedValue = GetIsNavBlocking();
            }
            set { _isBlocking.SetValueOverride(value); }
        }

        /// <summary>
        /// If can't deal any damage
        /// </summary>
        public bool IsNoDamage
        {
            get
            {
                if (_isNoDamage.IsCacheValid) return _isNoDamage.CachedValue;
                return _isNoDamage.CachedValue = Source.GetAttributeOrDefault<int>(ActorAttributeType.NoDamage) > 0;
            }
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
        /// If this was the last target selected
        /// </summary>
        public bool IsLastTarget
        {
            get { return Trinity.LastTargetRactorGUID != ACDGuid; }
        }

        /// <summary>
        /// For targetting, degree of importance given the current combat situation
        /// </summary>
        public double Weight
        {
            get
            {
                if (_weight.IsCacheValid) return _weight.CachedValue;
                return _weight.CachedValue = WeightManager.CalculateWeight(this, out WeightFactors, out _weightTime);
            }
            set { _weight.SetValueOverride(value); }
        }

        /// <summary>
        /// If this TrinityPlayer object is for the current player
        /// </summary>
        public bool IsMe
        {
            get { return _isMe.IsCacheValid ? _isMe.CachedValue : (_isMe.CachedValue = ACDGuid == CacheManager.ActivePlayerGuid); }
            set { _isMe.SetValueOverride(value); }
        }

        /// <summary>
        /// The individual factors that contribute to a final weight
        /// </summary>
        public List<Weight> WeightFactors = new List<Weight>();        

        /// <summary>
        /// How long it took to weight this
        /// </summary>
        public double WeightTime
        {
            get { return _weightTime; }
            set { _weightTime = value; }
        }

        public string WeightReasons 
        {
            get { return string.Join(", ", WeightFactors.Select(x => x.ToString()).ToArray()); }
        }

        #endregion

        #region Methods

        public static TrinityObjectType GetTrinityObjectType(CacheBase cacheBase)
        {
            return GetTrinityType(cacheBase.Source, cacheBase.ActorType, cacheBase.ActorSNO, cacheBase.ActorMeta.GizmoType, cacheBase.ActorMeta.InternalName);
        }

        /// <summary>
        /// Update weights that need updating
        /// </summary>
        internal bool TryCalculateWeight()
        {
            if (!_weight.IsCacheValid && !IsMe)
            {
                _weight.CachedValue = WeightManager.CalculateWeight(this, out WeightFactors, out _weightTime);
                return true;
            }
            return false;
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
            return pos == Vector3.Zero ? 0f : pos.Distance2D(CacheManager.Me.Position);
        }

        /// <summary>
        /// Faster distance call than ACD.Distance due to being 2d and calling cached player position.
        /// </summary>
        internal static Vector3 GetPosition(TrinityObject o)
        {
            // Position/Distance only works for Ground Items if called on DiaItem
            if(o.ActorType == ActorType.Item)
                return o.GetDiaItemProperty(x => x.Position);

            return o.GetSourceProperty(x => x.Position);
        }

        /// <summary>
        /// The radius, checking for overrides in DataDictionary.
        /// </summary>
        internal static float GetRadius(TrinityObject o)
        {
            float radius;
            if (o.IsNavigationObstacle && DataDictionary.CustomObjectRadius.TryGetValue(o.ActorSNO, out radius))
                return radius;

            return (float)o.ActorMeta.Radius;
        }

        /// <summary>
        /// The monster type, checking for overrides in DataDictionary.
        /// </summary>
        internal static MonsterType GetMonsterType(TrinityObject o)
        {
            MonsterType monsterTypeOverride;
            return DataDictionary.MonsterTypeOverrides.TryGetValue(o.ActorSNO, out monsterTypeOverride) ? monsterTypeOverride : o.ActorMeta.MonsterType;
        }

        /// <summary>
        /// The radius distance, checking for overrides in DataDictionary.
        /// </summary>
        public static float GetRadiusDistance(TrinityObject o)
        {
            float customRadiusDistance;
            if (o.TrinityType == TrinityObjectType.Destructible && DataDictionary.DestructableObjectRadius.TryGetValue(o.ActorSNO, out customRadiusDistance))
                return customRadiusDistance;

            return Math.Max(o.Distance - o.Radius - CacheManager.Me.Radius, 0f);
        }

        /// <summary>
        /// The type of player-summoned actor this is.
        /// </summary>
        internal static TrinityPetType GetSummonType(TrinityObject o)
        {
            if (DataDictionary.MysticAllyIds.Contains(o.ActorSNO))
                return TrinityPetType.MysticAlly;

            if (DataDictionary.DemonHunterPetIds.Contains(o.ActorSNO))
                return TrinityPetType.Companion;

            if (DataDictionary.DemonHunterSentryIds.Contains(o.ActorSNO))
                return TrinityPetType.Sentry;

            if (DataDictionary.WizardHydraIds.Contains(o.ActorSNO))
                return TrinityPetType.Hydra;

            if (DataDictionary.GargantuanIds.Contains(o.ActorSNO))
                return TrinityPetType.Gargantuan;

            if (DataDictionary.ZombieDogIds.Contains(o.ActorSNO))
                return TrinityPetType.ZombieDog;

            return TrinityPetType.Unknown;
        }

        /// <summary>
        /// The difference in height (Z-Axis) of two positions
        /// </summary>
        internal static float GetZDiff(Vector3 position)
        {
            return position != Vector3.Zero ? Math.Abs(CacheManager.Me.Position.Z - position.Z) : 0f;
        }

        /// <summary>
        /// If this actor is blocking the bot from moving
        /// </summary>
        private bool GetIsNavBlocking()
        {
            if (RadiusDistance <= 3f && PlayerMover.GetMovementSpeed() <= 0)
                return true;

            if (CacheManager.NavigationObstacles.Any(ob => MathUtil.IntersectsPath(ob.Position, ob.Radius, CacheManager.Me.Position, Position)))
                return true;

            return false;
        }

        /// <summary>
        /// The friendly name (the name you see in the Diablo interface)
        /// </summary>
        internal static string GetName(TrinityObject o)
        {
            if (o.ActorType == ActorType.Item && o.DiaItem != null)
                return o.DiaItem.Name;

            return o.InternalName;
        }

        public override string ToString()
        {
            return String.Format("{0}, Type={1} Dist={2} RActorGuid={3} ACDGuid={4} ActorSNO={5} WeightReasons={6}", InternalName, TrinityType, RadiusDistance, RActorGuid, ACDGuid, ActorSNO, WeightReasons);
        }

        #endregion

    }
}
