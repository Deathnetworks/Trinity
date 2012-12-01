using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Zeta;
using Zeta.Internals;
using Zeta.Internals.Actors;
using Zeta.Internals.Actors.Gizmos;
using GilesTrinity;
using Zeta.CommonBot;

namespace GilesTrinity.Technicals
{
    internal static class CacheRefresher
    {
        private static DiaActivePlayer Me = ZetaDia.Me;

        public static void CacheObjectRefresher(int rActorGUID, CacheObject cacheObject)
        {
            ACD rActor = ZetaDia.Actors.GetACDByGuid(rActorGUID);
            CacheObjectRefresher(rActorGUID, cacheObject);
        }

        public static void CacheObjectRefresher(ACD RActor, CacheObject cacheObject)
        {

        }

        public static void RefreshAll()
        {
            foreach (ACD rActor in ZetaDia.Actors.RActorList)
            {
                //int rActorGuid = rActor.AsRActor.RActorGuid;
                //CacheObjectRefresher(rActorGuid, new CacheObject(rActorGuid));
            }
        }
    }
}
