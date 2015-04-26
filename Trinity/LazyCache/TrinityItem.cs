using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Zeta.Game.Internals.Actors;
using Logger = Trinity.Technicals.Logger;

namespace Trinity.LazyCache
{
    /// <summary>
    /// Trinity Item
    /// </summary>
    public class TrinityItem : TrinityObject
    {
        public TrinityItem(ACD acd) : base(acd)
        {
            SourceItem = acd as ACDItem;
        }

        public ACDItem SourceItem { get; private set; }        

        #region Properties

        public float DurabilityCurrent
        {
            get { return CacheManager.GetCacheValue(this, parent => SourceItem.DurabilityPercent); }
        }

        public float DurabilityMax
        {
            get { return CacheManager.GetCacheValue(this, parent => SourceItem.DurabilityPercent); }
        }

        public float DurabilityPercent
        {
            get { return CacheManager.GetCacheValue(this, parent => SourceItem.DurabilityPercent); }
        }

        public float ArcaneOnCrit
        {
            get { return CacheManager.GetCacheValue(this, parent => SourceItem.Stats.ArcaneOnCrit); }
        }

        #endregion

        #region Methods



        #endregion

        public static implicit operator TrinityItem(ACD x)
        {
            return CacheFactory.CreateObject<TrinityItem>(x);
        }

    }
}