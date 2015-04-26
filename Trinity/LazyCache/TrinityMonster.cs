using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Zeta.Game.Internals.Actors;
using Logger = Trinity.Technicals.Logger;

namespace Trinity.LazyCache
{
    /// <summary>
    /// Trinity base object
    /// </summary>
    public class TrinityMonster : TrinityObject
    {
        public TrinityMonster(ACD acd) : base(acd) { }

        #region Properties

        public bool IsElite
        {
            get { return CacheManager.GetCacheValue(this, parent => parent.Source.IsElite); }
        }

        public bool IsRare
        {
            get { return CacheManager.GetCacheValue(this, parent => parent.Source.IsRare); }
        }

        public bool IsUnique
        {
            get { return CacheManager.GetCacheValue(this, parent => parent.Source.IsUnique); }
        }

        #endregion

        #region Methods



        #endregion

        public static implicit operator TrinityMonster(ACD x)
        {
            return CacheFactory.CreateObject<TrinityMonster>(x);
        }

    }
}
