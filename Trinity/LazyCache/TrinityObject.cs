using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Zeta.Game.Internals.Actors;
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

        public string InternalName
        {
            get { return CacheManager.GetCacheValue(this, parent => parent.Source.Name); }
        }

        public float Distance
        {
            get { return CacheManager.GetCacheValue(this, parent => parent.Source.Distance, 50); }
        }

        public int ActorId
        {
            get { return CacheManager.GetCacheValue(this, parent => parent.Source.ActorSNO); }
        }

        #endregion

        #region Methods



        #endregion

        public static implicit operator TrinityObject(ACD x)
        {
            return CacheFactory.CreateObject<TrinityObject>(x);
        }

    }
}
