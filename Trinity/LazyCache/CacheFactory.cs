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
        public static TrinityObject CreateTypedTrinityObject(ACD acd, ActorType actorType, int acdGuid)
        {
            var gizmoType = acd.GizmoType;
            var actorSNO = acd.ActorSNO;
            var internalName = acd.Name;
            var trinityType = TrinityObject.GetTrinityObjectType(acd, actorType, actorSNO, gizmoType, internalName);

            TrinityObject result;

            if (actorType == ActorType.Monster && trinityType == TrinityObjectType.Unit)
                result = new TrinityUnit(acd, acdGuid);

            else if (actorType == ActorType.Gizmo)
                result = new TrinityGizmo(acd, acdGuid);

            else if(actorType == ActorType.Item && trinityType == TrinityObjectType.Item)
                result = new TrinityItem(acd, acdGuid);

            else if(actorType == ActorType.Player && trinityType == TrinityObjectType.Player)
                result = new TrinityPlayer(acd, acdGuid);

            else if (trinityType == TrinityObjectType.Avoidance)
                result = new TrinityAvoidance(acd, acdGuid);

            else
                result = new TrinityObject(acd, acdGuid);

            // Assign the properties we've used so far so they're only called once
            result.ActorType = actorType;
            result.TrinityType = trinityType;
            result.ActorSNO = actorSNO;
            result.InternalName = internalName;
            result.GizmoType = gizmoType;

            return result;
        }
    }
}
