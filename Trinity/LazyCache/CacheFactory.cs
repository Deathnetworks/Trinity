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
            return CacheUtilities.New<T>(cacheBase.Object);
        }

        public static TrinityObject CreateTrinityObject(DiaObject rActor)
        {
            return CreateTrinityObject(rActor, rActor.CommonData, rActor.ACDGuid, rActor.RActorGuid, rActor.ActorSNO, rActor.ActorType);
        }

        /// <summary>
        /// Converts an ACD into the appropriate Trinity object
        /// </summary>
        public static TrinityObject CreateTrinityObject(DiaObject rActor, ACD acd, int acdGuid, int rActorGuid, int actorSNO, ActorType actorType)
        {
            TrinityObject result;

            var meta = CacheMeta.GetOrCreateActorMeta(rActor, acd, actorSNO, actorType);

            var trinityType = CacheBase.GetTrinityType(acd, actorType, actorSNO, meta.GizmoType, meta.InternalName);

            //if (actorType == ActorType.Player)
            //    result = new TrinityPlayer();

            if (actorType == ActorType.Monster && trinityType != TrinityObjectType.Player)
                result = new TrinityUnit();

            else if (actorType == ActorType.Gizmo)
                result = new TrinityGizmo();

            else if (actorType == ActorType.Item)
                result = new TrinityItem();

            else if (trinityType == TrinityObjectType.Avoidance)
                result = new TrinityAvoidance();

            else
                result = new TrinityObject();

            result.Object = rActor;
            result.Source = acd;
            result.ACDGuid = acdGuid;
            result.RActorGuid = rActorGuid;
            result.ActorSNO = actorSNO;
            result.ActorMeta = meta;
            result.ActorType = actorType;
            result.InternalName = meta.InternalName;
            result.TrinityType = trinityType;
            result.LastUpdated = CacheManager.LastUpdated;
            result.ACDItem = acd as ACDItem;
            result.DiaGizmo = rActor as DiaGizmo;
            result.DiaItem = rActor as DiaItem;
            result.DiaUnit = rActor as DiaUnit;    

            return result;
        }

    }
}
