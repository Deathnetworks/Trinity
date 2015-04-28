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
    public class TrinityAvoidance : TrinityObject
    {
        public TrinityAvoidance(ACD acd) : base(acd) { }

        #region Properties

        public float AvoidanceHealth
        {
            get { return CacheManager.GetCacheValue(this, o => AvoidanceManager.GetAvoidanceHealthBySNO(ActorId, 100), 30000); }
        }

        public float AvoidanceRadius
        {
            get { return CacheManager.GetCacheValue(this, o => AvoidanceManager.GetAvoidanceRadiusBySNO(ActorId, 100), 30000); }
        }

        #endregion

        #region Methods


        #endregion

        public static implicit operator TrinityAvoidance(ACD x)
        {
            return CacheFactory.CreateObject<TrinityAvoidance>(x);
        }

    }
}
