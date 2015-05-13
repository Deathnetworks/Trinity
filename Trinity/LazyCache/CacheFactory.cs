using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Trinity.Technicals;
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
        public static TrinityObject CreateTypedTrinityObject(ACD acd, int guid, int actorSNO)
        {
            CacheBase baseObj;

            using (new PerformanceLogger(string.Format("BaseCtor {0} ActorType={1} SNO={2}", acd.Name, guid, actorSNO)))
            {
                baseObj = new CacheBase(acd, guid)
                {
                    ActorSNO = actorSNO                    
                };
            }

            TrinityObject result;

            var actorType = baseObj.ActorType;
            var trinityType = baseObj.TrinityType;

            if (actorType == ActorType.Player && trinityType == TrinityObjectType.Player)
                result = baseObj.ToTrinityPlayer();

            else if (actorType == ActorType.Monster && trinityType == TrinityObjectType.Unit)
                result = baseObj.ToTrinityUnit();

            else if (actorType == ActorType.Gizmo)
                result = baseObj.ToTrinityGizmo();

            else if (actorType == ActorType.Item && trinityType == TrinityObjectType.Item)
                result = baseObj.ToTrinityItem();

            else if (trinityType == TrinityObjectType.Avoidance)
                result = baseObj.ToTrinityAvoidance();

            else
                result = baseObj.ToTrinityObject();

            return result;
        }

        /// <summary>
        /// Converts an ACD into the appropriate DiaObject
        /// </summary>
        public static DiaObject CreateTypedDiaObject(ACD acd, ActorType actorType)
        {
            switch (actorType)
            {
                case ActorType.Monster:
                    return CacheUtilities.New<DiaUnit>(acd.BaseAddress);

                case ActorType.Gizmo:
                    return CacheUtilities.New<DiaGizmo>(acd.BaseAddress);

                case ActorType.Player:

                    if(acd.ACDGuid == ZetaDia.ActivePlayerACDGuid)
                        return CacheUtilities.New<DiaActivePlayer>(acd.BaseAddress);

                    return CacheUtilities.New<DiaPlayer>(acd.BaseAddress);

                case ActorType.Item:
                    return CacheUtilities.New<DiaItem>(acd.BaseAddress);
            }

            return CacheUtilities.New<DiaObject>(acd.BaseAddress);
        }
    }
}
