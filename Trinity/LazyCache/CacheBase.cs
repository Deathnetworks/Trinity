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
    /// Base class for cache objects. 
    /// Handles linkage to ACDGuid. Properties for Indexed Lookups etc.
    /// </summary>
    public class CacheBase
    {
        public CacheBase(ACD acd)
        {
            UpdateSource(acd);
            ACDGuid = acd.ACDGuid;            
        }

        /// <summary>
        /// Check if the underlying ACD is valid. When false, LazyCache will return only cached values 
        /// and this object will be destroyed in the next purge.
        /// </summary>
        public bool IsValid
        {
            get { return Source.IsProperValid(); }
        }

        #region Core Propperties

        public ACD Source { get; private set; }
        public int ACDGuid { get; private set; }
        public int UpdateTicks { get; private set; }

        public ActorType ActorType
        {
            get { return CacheManager.GetCacheValue(this, parent => parent.Source.ActorType); }
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

        /// <summary>
        /// this should only be called by CacheManager.Update and only once per tick per object.
        /// </summary>
        /// <param name="acd"></param>
        internal void UpdateSource(ACD acd)
        {
            Source = acd;
            ++UpdateTicks;
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
