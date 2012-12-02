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
using Zeta.Internals.SNO;

namespace GilesTrinity.Cache
{
    internal static class CacheRefresher
    {
        private static DiaActivePlayer Me = ZetaDia.Me;

        public static void CacheObjectRefresher(int? acdGuid = null, ACD acd = null, CacheObject cacheObject = null)
        {
            if (acdGuid == null && acd == null && cacheObject == null)
            {
                throw new ArgumentNullException("All arguments cannot be null");
            }

            if (cacheObject != null)
            {
                acdGuid = cacheObject.ACDGuid;
                acd = cacheObject.CommonData;
            }
            else if (acd != null)
            {
                acdGuid = acd.ACDGuid;
            }
            else if (acdGuid != null)
            {
                acd = ZetaDia.Actors.GetACDByGuid((int)acdGuid);
            }

            // Initialize a new Cache Object
            if (cacheObject == null)
            {
                int actorSNO = acd.ActorSNO;
                if (CacheUtils.IsAvoidanceSNO(actorSNO))
                {
                    cacheObject = new CacheAvoidance(acd);
                }
                else
                {
                    switch (acd.ActorType)
                    {
                        case ActorType.Unit:
                            cacheObject = new CacheUnit(acd);
                            break;
                        case ActorType.Gizmo:
                            cacheObject = new CacheGizmo(acd);
                            break;
                        case ActorType.Item:
                            cacheObject = new CacheItem(acd);
                            break;
                        default:
                            cacheObject = new CacheOther(acd);
                            break;
                    }
                }
            }
            else
            {
                switch (cacheObject.CacheType)
                {
                    case CacheType.Unit:
                        RefreshUnit((CacheUnit)cacheObject);
                        break;
                    case CacheType.Item:
                        RefreshItem((CacheItem)cacheObject);
                        break;
                    case CacheType.Gizmo:
                        RefreshGizmo((CacheGizmo)cacheObject);
                        break;
                    case CacheType.Other:
                        RefreshOther((CacheOther)cacheObject);
                        break;
                }

            }

        }

        /// <summary>
        /// Refreshes a unit
        /// </summary>
        /// <param name="unit"></param>
        private static void RefreshUnit(CacheUnit unit)
        {
            unit.Position = unit.CommonData.Position;



            CacheManager.PutObject(unit);

        }
        /// <summary>
        /// Refreshes an Item
        /// </summary>
        /// <param name="item"></param>
        private static void RefreshItem(CacheItem item)
        {
            CacheManager.PutObject(item);
        }

        /// <summary>
        /// Refreshes a Gizmo
        /// </summary>
        /// <param name="gizmo"></param>
        private static void RefreshGizmo(CacheGizmo gizmo)
        {


            CacheManager.PutObject(gizmo);

        }

        /// <summary>
        /// Refreshes a generic object
        /// </summary>
        /// <param name="other"></param>
        private static void RefreshOther(CacheOther other)
        {
         


            CacheManager.PutObject(other);

        }

        /// <summary>
        /// Refresh all objects from ZetaDia.Actors.ACDList
        /// </summary>
        public static void RefreshAll()
        {
            foreach (ACD acd in ZetaDia.Actors.ACDList)
            {
                CacheObjectRefresher(null, acd, null);
            }
        }
    }
}
