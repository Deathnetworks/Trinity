using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
            get { return CacheManager.GetCacheValue(this, o => o.Gizmo.HasBeenOperated); }
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
            get { return CacheManager.GetCacheValue(this, o => o.Gizmo.GizmoCharges); }
        }

        public int GizmoOperatorACDID
        {
            get { return CacheManager.GetCacheValue(this, o => o.Gizmo.GizmoOperatorACDID); }
        }

        public int GizmoState
        {
            get { return CacheManager.GetCacheValue(this, o => o.Gizmo.GizmoState); }
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
