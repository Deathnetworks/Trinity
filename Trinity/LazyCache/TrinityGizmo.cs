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
        public TrinityGizmo(ACD acd) : base(acd) { }

        #region Properties

        public bool HasBeenOperated
        {
            get { return CacheManager.GetCacheValue(this, o => o.Gizmo.HasBeenOperated, 200); }
        }

        public bool IsGizmoUsed
        {
            get { return CacheManager.GetCacheValue(this, CacheUtilities.IsGizmoUsed, 200); }
        }

        public bool IsBarricade
        {
            get { return CacheManager.GetCacheValue(this, o => o.Gizmo.IsBarricade); }
        }

        public bool IsDestructibleObject
        {
            get { return CacheManager.GetCacheValue(this, o => o.Gizmo.IsDestructibleObject); }
        }

        public bool IsGizmoDisabledByScript
        {
            get { return CacheManager.GetCacheValue(this, o => o.Gizmo.IsGizmoDisabledByScript); }
        }

        public bool IsNephalemAltar
        {
            get { return CacheManager.GetCacheValue(this, o => o.Gizmo.IsNephalemAltar); }
        }

        public bool IsPortal
        {
            get { return CacheManager.GetCacheValue(this, o => o.Gizmo.IsPortal); }
        }

        public bool IsTownPortal
        {
            get { return CacheManager.GetCacheValue(this, o => o.Gizmo.IsTownPortal); }
        }

        public bool Operatable
        {
            get { return CacheManager.GetCacheValue(this, o => o.Gizmo.Operatable); }
        }

        public int BannerACDId
        {
            get { return CacheManager.GetCacheValue(this, o => o.Gizmo.BannerACDId); }
        }

        public int DropsNoLoot
        {
            get { return CacheManager.GetCacheValue(this, o => o.Gizmo.DropsNoLoot); }
        }

        public int GizmoCharges
        {
            get { return CacheManager.GetCacheValue(this, o => o.Gizmo.GizmoCharges, 200); }
        }

        public int GizmoOperatorACDID
        {
            get { return CacheManager.GetCacheValue(this, o => o.Gizmo.GizmoOperatorACDID); }
        }

        public int GizmoState
        {
            get { return CacheManager.GetCacheValue(this, o => o.Gizmo.GizmoState, 200); }
        }

        public bool IsChestOpen
        {
            get { return CacheManager.GetCacheValue(this, o => o.Source.GetAttributeOrDefault<int>(ActorAttributeType.ChestOpen) > 0); }
        }

        public bool IsCorpse
        {
            get { return CacheManager.GetCacheValue(this, o => o.InternalName.ToLower().Contains("corpse")); }
        }

        public bool IsWeaponRack
        {
            get { return CacheManager.GetCacheValue(this, o => o.InternalName.ToLower().Contains("rack")); }
        }

        public bool IsGroundClicky
        {
            get { return CacheManager.GetCacheValue(this, o => o.InternalName.ToLower().Contains("ground_clicky")); }
        }

        public bool IsChest
        {
            get
            {
                return CacheManager.GetCacheValue(this, o => (!IsRareChest && o.InternalName.ToLower().Contains("chest")) ||
                            DataDictionary.ContainerWhiteListIds.Contains(o.ActorSNO)); 
            }
        }

        public bool IsRareChest
        {
            get 
            { 
                return CacheManager.GetCacheValue(this, o => o.InternalName.ToLower().Contains("chest_rare") || 
                DataDictionary.ResplendentChestIds.Contains(o.ActorSNO)); 
            }
        }

        public bool IsCloseDestructable
        {
            get { return CacheManager.GetCacheValue(this, o => IsDestructibleObject && Distance < 6f); }
        }

        public bool IsCloseLargeDestructable
        {
            get { return CacheManager.GetCacheValue(this, o => IsCloseDestructable && o.Radius >= 10f); }
        }

        public bool IsWithinDestroyRange
        {
            get { return CacheManager.GetCacheValue(this, o => DestroyRange <= Distance); }
        }

        public bool IsShrine
        {
            get { return CacheManager.GetCacheValue(this, o => Type == TrinityObjectType.Shrine || Type == TrinityObjectType.CursedShrine); }
        }

        public float DestroyRange
        {
            get
            {
                return CacheManager.GetCacheValue(this, o =>
                {
                    float maxRadiusDistance;
                    return DataDictionary.DestructableObjectRadius.TryGetValue(o.ActorSNO, out maxRadiusDistance) ? maxRadiusDistance : Trinity.Settings.WorldObject.DestructibleRange;
                });
            }
        }

        public int GrantsNoXp
        {
            get { return CacheManager.GetCacheValue(this, o => o.Gizmo.GrantsNoXp); }
        }

        #endregion

        #region Methods

        public bool Interact()
        {
            return Object.Interact();
        }

        #endregion

        public static implicit operator TrinityGizmo(ACD x)
        {
            return CacheFactory.CreateObject<TrinityGizmo>(x);
        }

    }
}
