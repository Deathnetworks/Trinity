using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Navigation;
using Trinity.DbProvider;
using Zeta.Bot.Navigation;
using Zeta.Game;
using Zeta.Game.Internals.Actors;
using Zeta.Game.Internals.SNO;
using Logger = Trinity.Technicals.Logger;

namespace Trinity.LazyCache
{
    /// <summary>
    /// Trinity Gizmo Object
    /// </summary>
    public class TrinityGizmo : TrinityObject
    {
        public TrinityGizmo() { }

        public TrinityGizmo(DiaObject rActor) : base(rActor) { }

        #region Fields

        private readonly CacheField<bool> _hasBeenOperated = new CacheField<bool>(UpdateSpeed.Normal);
        private readonly CacheField<bool> _isGizmoUsed = new CacheField<bool>(UpdateSpeed.Fast);
        private readonly CacheField<bool> _isBarricade = new CacheField<bool>();
        private readonly CacheField<bool> _isDestructible = new CacheField<bool>();
        private readonly CacheField<bool> _isDisabledByScript = new CacheField<bool>(UpdateSpeed.Normal);
        private readonly CacheField<bool> _isNephalemAltar = new CacheField<bool>();
        private readonly CacheField<bool> _isPortal = new CacheField<bool>();
        private readonly CacheField<bool> _isTownPortal = new CacheField<bool>();
        private readonly CacheField<bool> _isOperatable = new CacheField<bool>(UpdateSpeed.Normal);
        private readonly CacheField<int> _bannerACDId = new CacheField<int>();
        private readonly CacheField<bool> _isDropsNoLoot = new CacheField<bool>();
        private readonly CacheField<int> _gizmoCharges = new CacheField<int>();
        private readonly CacheField<int> _gizmoOperatorACDId = new CacheField<int>();
        private readonly CacheField<int> _gizmoState = new CacheField<int>();
        private readonly CacheField<bool> _isChestOpen = new CacheField<bool>();
        private readonly CacheField<bool> _isCorpse = new CacheField<bool>();
        private readonly CacheField<bool> _isWeaponRack = new CacheField<bool>();
        private readonly CacheField<bool> _isGroundClicky = new CacheField<bool>();
        private readonly CacheField<bool> _isChest = new CacheField<bool>();
        private readonly CacheField<bool> _isRareChest = new CacheField<bool>();
        private readonly CacheField<bool> _isCloseDestructable = new CacheField<bool>();
        private readonly CacheField<bool> _isCloseLargeDestructable = new CacheField<bool>();
        private readonly CacheField<bool> _isWithinDestroyRange = new CacheField<bool>();
        private readonly CacheField<bool> _isShrine = new CacheField<bool>();
        private readonly CacheField<float> _destroyRange = new CacheField<float>();
        private readonly CacheField<bool> _grantsNoXP = new CacheField<bool>();

        #endregion

        #region Properties

        /// <summary>
        /// If HasBeenOperated attribute is set to true
        /// </summary>
        public bool HasBeenOperated
        {
            get { return _hasBeenOperated.IsCacheValid ? _hasBeenOperated.CachedValue : (_hasBeenOperated.CachedValue = GetGizmoProperty(x => x.HasBeenOperated)); }
            set { _hasBeenOperated.SetValueOverride(value); }
        }

        /// <summary>
        /// If the gizmo has been used (factoring in many types of used attributes)
        /// </summary>
        public bool IsGizmoUsed
        {
            get { return _isGizmoUsed.IsCacheValid ? _isGizmoUsed.CachedValue : (_isGizmoUsed.CachedValue = GetIsGizmoUsed(this)); }
            set { _isGizmoUsed.SetValueOverride(value); }
        }

        /// <summary>
        /// If its a barricade gizmo
        /// </summary>
        public bool IsBarricade
        {
            get { return _isBarricade.IsCacheValid ? _isBarricade.CachedValue : (_isBarricade.CachedValue = ActorMeta.IsBarracade); }
            set { _isBarricade.SetValueOverride(value); }
        }

        /// <summary>
        /// If can be destroyed
        /// </summary>
        public bool IsDestructible
        {
            get { return _isDestructible.IsCacheValid ? _isDestructible.CachedValue : (_isDestructible.CachedValue = ActorMeta.GizmoIsDestructible); }
            set { _isDestructible.SetValueOverride(value); }
        }

        /// <summary>
        /// If gizmo is currently disabled by a script
        /// </summary>
        public bool IsDisabledByScript
        {
            get { return _isDisabledByScript.IsCacheValid ? _isDisabledByScript.CachedValue : (_isDisabledByScript.CachedValue = ActorMeta.GizmoIsDisabledByScript); }
            set { _isDisabledByScript.SetValueOverride(value); }
        }

        /// <summary>
        /// If gizmo is currently disabled by a script
        /// </summary>
        public bool IsNephalemAltar
        {
            get { return _isNephalemAltar.IsCacheValid ? _isNephalemAltar.CachedValue : (_isNephalemAltar.CachedValue = GetGizmoProperty(x => x.IsNephalemAltar)); }
            set { _isNephalemAltar.SetValueOverride(value); }
        }

        /// <summary>
        /// If gizmo is a portal
        /// </summary>
        public bool IsPortal
        {
            get { return _isPortal.IsCacheValid ? _isPortal.CachedValue : (_isPortal.CachedValue = ActorMeta.GizmoIsPortal); }
            set { _isPortal.SetValueOverride(value); }
        }

        /// <summary>
        /// If gizmo is a portal that goes to town
        /// </summary>
        public bool IsTownPortal
        {
            get { return _isTownPortal.IsCacheValid ? _isTownPortal.CachedValue : (_isTownPortal.CachedValue = ActorMeta.GizmoIsTownPortal); }
            set { _isTownPortal.SetValueOverride(value); }
        }

        /// <summary>
        /// If Operatable attribute is true
        /// </summary>
        public bool IsOperatable
        {
            get { return _isOperatable.IsCacheValid ? _isOperatable.CachedValue : (_isOperatable.CachedValue = GetGizmoProperty(x => x.Operatable)); }
            set { _isOperatable.SetValueOverride(value); }
        }

        /// <summary>
        /// 
        /// </summary>
        public int BannerACDId
        {
            get { return _bannerACDId.IsCacheValid ? _bannerACDId.CachedValue : (_bannerACDId.CachedValue = GetGizmoProperty(x => x.BannerACDId)); }
            set { _bannerACDId.SetValueOverride(value); }
        }

        /// <summary>
        /// If a gizmo that doesn't drop loot
        /// </summary>
        public bool IsDropsNoLoot
        {
            get { return _isDropsNoLoot.IsCacheValid ? _isDropsNoLoot.CachedValue : (_isDropsNoLoot.CachedValue = ActorMeta.GizmoDropNoLoot > 0); }
            set { _isDropsNoLoot.SetValueOverride(value); }
        }

        /// <summary>
        /// If a gizmo that doesn't drop loot
        /// </summary>
        public int GizmoCharges
        {
            get { return _gizmoCharges.IsCacheValid ? _gizmoCharges.CachedValue : (_gizmoCharges.CachedValue = GetGizmoProperty(x => x.GizmoCharges)); }
            set { _gizmoCharges.SetValueOverride(value); }
        }

        /// <summary>
        /// 
        /// </summary>
        public int GizmoOperatorACDId
        {
            get { return _gizmoOperatorACDId.IsCacheValid ? _gizmoOperatorACDId.CachedValue : (_gizmoOperatorACDId.CachedValue = GetGizmoProperty(x => x.GizmoOperatorACDID)); }
            set { _gizmoOperatorACDId.SetValueOverride(value); }
        }

        /// <summary>
        /// 
        /// </summary>
        public int GizmoState
        {
            get { return _gizmoState.IsCacheValid ? _gizmoState.CachedValue : (_gizmoState.CachedValue = GetGizmoProperty(x => x.GizmoState)); }
            set { _gizmoState.SetValueOverride(value); }
        }

        /// <summary>
        /// If chest is open
        /// </summary>
        public bool IsChestOpen
        {
            get { return _isChestOpen.IsCacheValid ? _isChestOpen.CachedValue : (_isChestOpen.CachedValue = Source.GetAttributeOrDefault<int>(ActorAttributeType.ChestOpen) > 0); }
            set { _isChestOpen.SetValueOverride(value); }
        }

        /// <summary>
        /// If is a corpse container
        /// </summary>
        public bool IsCorpse
        {
            get { return _isCorpse.IsCacheValid ? _isCorpse.CachedValue : (_isCorpse.CachedValue = InternalNameLowerCase.Contains("corpse")); }
            set { _isCorpse.SetValueOverride(value); }
        }

        /// <summary>
        /// If is a weapon rack container
        /// </summary>
        public bool IsWeaponRack
        {
            get { return _isWeaponRack.IsCacheValid ? _isWeaponRack.CachedValue : (_isWeaponRack.CachedValue = InternalNameLowerCase.Contains("rack")); }
            set { _isWeaponRack.SetValueOverride(value); }
        }

        /// <summary>
        /// If is a clickable thing on the ground, tile/rock etc.
        /// </summary>
        public bool IsGroundClicky
        {
            get { return _isGroundClicky.IsCacheValid ? _isGroundClicky.CachedValue : (_isGroundClicky.CachedValue = InternalNameLowerCase.Contains("ground_clicky")); }
            set { _isGroundClicky.SetValueOverride(value); }
        }

        /// <summary>
        /// If is a chest container
        /// </summary>
        public bool IsChest
        {
            get { return _isChest.IsCacheValid ? _isChest.CachedValue : (_isChest.CachedValue = (!IsRareChest && InternalNameLowerCase.Contains("chest")) || DataDictionary.ContainerWhiteListIds.Contains(ActorSNO)); }
            set { _isChest.SetValueOverride(value); }
        }

        /// <summary>
        /// If is a rare chest container
        /// </summary>
        public bool IsRareChest
        {
            get { return _isRareChest.IsCacheValid ? _isRareChest.CachedValue : (_isRareChest.CachedValue = InternalNameLowerCase.Contains("chest_rare") || DataDictionary.ResplendentChestIds.Contains(ActorSNO)); }
            set { _isRareChest.SetValueOverride(value); }
        }

        /// <summary>
        /// If a destructable and within 6f
        /// </summary>
        public bool IsCloseDestructable
        {
            get { return _isCloseDestructable.IsCacheValid ? _isCloseDestructable.CachedValue : (_isCloseDestructable.CachedValue = IsDestructible && Distance < 6f); }
            set { _isCloseDestructable.SetValueOverride(value); }
        }

        /// <summary>
        /// If a large destructable that is close
        /// </summary>
        public bool IsCloseLargeDestructable
        {
            get { return _isCloseLargeDestructable.IsCacheValid ? _isCloseLargeDestructable.CachedValue : (_isCloseLargeDestructable.CachedValue = IsDestructible && Radius >= 10f && RadiusDistance < 12f); }
            set { _isCloseLargeDestructable.SetValueOverride(value); }
        }

        /// <summary>
        /// If within range to be destroyed
        /// </summary>
        public bool IsWithinDestroyRange
        {
            get { return _isWithinDestroyRange.IsCacheValid ? _isWithinDestroyRange.CachedValue : (_isWithinDestroyRange.CachedValue = DestroyRange <= Distance); }
            set { _isWithinDestroyRange.SetValueOverride(value); }
        }

        /// <summary>
        /// If its a shrine
        /// </summary>
        public bool IsShrine
        {
            get { return _isShrine.IsCacheValid ? _isShrine.CachedValue : (_isShrine.CachedValue = TrinityType == TrinityObjectType.Shrine || TrinityType == TrinityObjectType.CursedShrine); }
            set { _isShrine.SetValueOverride(value); }
        }

        /// <summary>
        /// Range this object can be destroyed within
        /// </summary>
        public float DestroyRange
        {
            get { return _destroyRange.IsCacheValid ? _destroyRange.CachedValue : (_destroyRange.CachedValue = GetRadiusDistance(this)); }
            set { _destroyRange.SetValueOverride(value); }
        }

        /// <summary>
        /// If this gizmo grants no experience
        /// </summary>
        public bool GrantsNoXp
        {
            get { return _grantsNoXP.IsCacheValid ? _grantsNoXP.CachedValue : (_grantsNoXP.CachedValue = ActorMeta.GizmoGrantsNoXp > 0); }
            set { _grantsNoXP.SetValueOverride(value); }
        }

        #endregion

        #region Methods

        public bool Interact()
        {
            return Object.Interact();
        }

        /// <summary>
        /// If interactable is currently in its used state.
        /// </summary>
        internal static bool GetIsGizmoUsed(TrinityGizmo gizmo)
        {
            int endAnimation;
            if (gizmo.TrinityType == TrinityObjectType.Interactable &&
                DataDictionary.InteractEndAnimations.TryGetValue(gizmo.ActorSNO, out endAnimation) &&
                endAnimation == (int)gizmo.CurrentAnimation)
                return true;

            if (gizmo.HasBeenOperated || gizmo.IsChestOpen)
                return true;

            if (gizmo.GizmoState == 1)
                return true;

            if (gizmo.TrinityType == TrinityObjectType.Door)
            {
                string currentAnimation = gizmo.CurrentAnimation.ToString().ToLower();

                // special hax for A3 Iron Gates
                if (currentAnimation.Contains("irongate") && currentAnimation.Contains("open"))
                    return false;

                if (currentAnimation.Contains("irongate") && currentAnimation.Contains("idle"))
                    return true;

                if (currentAnimation.EndsWith("open") || currentAnimation.EndsWith("opening"))
                    return true;
            }

            return false;
        }

        public override string ToString()
        {
            return String.Format("{0}, Type={1} GizmoType={2} Dist={3} RActorGuid={4} ACDGuid={5} ActorSNO={6} WeightReasons={7}", 
                Name, TrinityType, GizmoType, RadiusDistance, RActorGuid, ACDGuid, ActorSNO, WeightReasons);
        }

        #endregion

    }
}
