using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Zeta.Game;
using Zeta.Game.Internals;
using Zeta.Game.Internals.Actors;
using Zeta.Game.Internals.Actors.Gizmos;
using Zeta.Game.Internals.SNO;
using Logger = Trinity.Technicals.Logger;

namespace Trinity.LazyCache
{
    /// <summary>
    /// Base class for cache objects. 
    /// Handles linkage to ACDGuid. Properties for Indexed Lookups etc.
    /// </summary>
    public class CacheBase
    {
        public CacheBase(ACD acd)
        {
            ACDGuid = acd.ACDGuid;     
            UpdateSource(acd);       
        }

        /// <summary>
        /// Check if the underlying ACD is valid. When false, LazyCache will return only 
        /// cached values and this object will be destroyed in the next purge.
        /// </summary>
        public bool IsValid
        {
            get { return Source.IsProperValid(); }
        }

        #region Core Propperties

        public ACD Source { get; private set; }

        public int ACDGuid { get; private set; }

        public DateTime LastUpdated { get; private set; }

        public ActorType ActorType
        {
            get { return CacheManager.GetCacheValue(this, o => o.Source.ActorType); }
        }

        public DiaObject Object
        {
            get { return CacheManager.GetCacheValue(this, o => o.Source.AsRActor); }
        }

        public DiaGizmo Gizmo
        {
            // Todo replace with faster call, ACD Based Obj or ActorManager.GetActorByRActorGuid() when nesox adds it.
            get { return CacheManager.GetCacheValue(this, o => ZetaDia.Actors.GetActorsOfType<DiaGizmo>(true).FirstOrDefault(u => u.ACDGuid == ACDGuid)); }
        }

        public ACDItem Item
        {
            get { return CacheManager.GetCacheValue(this, o => ActorType == ActorType.Item ? o.Source as ACDItem : null); }
        }

        public DiaUnit Unit
        {
            // Todo replace with faster call, ACD Based Obj or ActorManager.GetActorByRActorGuid() when nesox adds it.
            get { return CacheManager.GetCacheValue(this, o => ZetaDia.Actors.GetActorsOfType<DiaUnit>(true).FirstOrDefault(u => u.ACDGuid == ACDGuid)); }
        }

        #endregion

        #region Cast Conversions

        public static implicit operator CacheBase(ACD x)
        {
            return new CacheBase(x);
        }

        public static implicit operator ACD(CacheBase x)
        {
            return x.Source;
        }

        #endregion

        #region Methods

        internal void UpdateSource(ACD acd)
        {
            Source = acd;
            LastUpdated = DateTime.UtcNow;
        }

        public override int GetHashCode()
        {
            return ACDGuid;
        }

        public override bool Equals(object obj)
        {
            return GetHashCode() == obj.GetHashCode();
        }

        #endregion
 
    }
}
