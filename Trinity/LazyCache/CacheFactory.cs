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
    /// Utility for creating cache objects
    /// </summary>
    public static class CacheFactory
    {
        /// <summary>
        /// Convert CacheObjectBase into a derived class
        /// </summary>
        /// <typeparam name="T">a class derived from CacheBase</typeparam>
        /// <param name="cacheBase">an instance of CacheBase</param>
        /// <returns>baseObject converted to an instance of T</returns>
        public static T CreateObject<T>(CacheBase cacheBase)
        {            
            return CacheUtilities.New<T>(cacheBase.Source);
        }

        /// <summary>
        /// Converts an ACD into the appropriate Trinity object
        /// </summary>
        public static TrinityObject CreateTypedTrinityObject(ACD acd)
        {
            var trinityType = TrinityObject.GetTrinityObjectType(acd);
            var actorType = acd.ActorType;

            if (acd.ActorType == ActorType.Monster && trinityType == TrinityObjectType.Unit)
                return new TrinityUnit(acd);

            if (actorType == ActorType.Gizmo)
                return new TrinityGizmo(acd);

            if(actorType == ActorType.Item && trinityType == TrinityObjectType.Item)
                return new TrinityItem(acd);

            if(actorType == ActorType.Player && trinityType == TrinityObjectType.Player)
                return new TrinityPlayer(acd);

            if (trinityType == TrinityObjectType.Avoidance)
                return new TrinityAvoidance(acd);

            return new TrinityObject(acd);
        }
    }
}
