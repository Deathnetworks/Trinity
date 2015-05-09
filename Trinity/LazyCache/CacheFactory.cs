using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Zeta.Game;
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
        public static TrinityObject CreateTypedTrinityObject(ACD acd, ActorType actorType, int acdGuid, int sno)
        {
            var baseObj = new CacheBase(acd, acdGuid)
            {
                ActorType = actorType,
                ActorSNO = sno                
            };

            TrinityObject result;

            if (actorType == ActorType.Monster && baseObj.TrinityType == TrinityObjectType.Unit)
                result = baseObj.ToTrinityUnit();

            else if (baseObj.ActorType == ActorType.Gizmo)
                result = baseObj.ToTrinityGizmo();

            else if (baseObj.ActorType == ActorType.Item && baseObj.TrinityType == TrinityObjectType.Item)
                result = baseObj.ToTrinityItem();

            else if (baseObj.ActorType == ActorType.Player && baseObj.TrinityType == TrinityObjectType.Player)
                result = new TrinityPlayer(acd, acdGuid);

            else if (baseObj.TrinityType == TrinityObjectType.Avoidance)
                result = baseObj.ToTrinityAvoidance();

            else
                result = baseObj.ToTrinityObject();

            return result;
        }
    }
}
