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

        /// <summary>
        /// Refreshes a unit
        /// </summary>
        /// <param name="unit"></param>
        private static void RefreshUnit(CacheUnit unit)
        {
            try
            {
                unit.Position = unit.CommonData.Position;
            }
            catch
            {
            } 
        }
        /// <summary>
        /// Refreshes an Item
        /// </summary>
        /// <param name="item"></param>
        private static void RefreshItem(CacheItem item)
        {
            try
            {

            }
            catch
            {
            }
        }

        /// <summary>
        /// Refreshes a Gizmo
        /// </summary>
        /// <param name="gizmo"></param>
        private static void RefreshGizmo(CacheGizmo gizmo)
        {
            try
            {

            }
            catch
            {
            }
        }

        /// <summary>
        /// Refreshes a generic object
        /// </summary>
        /// <param name="other"></param>
        private static void RefreshOther(CacheOther other)
        {
            try
            {

            }
            catch
            {
            }
        }

        /// <summary>
        /// Refresh all objects from ZetaDia.Actors.ACDList
        /// </summary>
        public static void RefreshAll()
        {
            CacheManager.CacheObjectGetter = GetCache;
            CacheManager.CacheObjectRefresher = RefreshCache;
            CacheManager.MaxRefreshRate = 100;
            foreach (DiaObject obj in ZetaDia.Actors.GetActorsOfType<DiaObject>(true, false).Where( o => o.IsACDBased && o.CommonData != null))
            {
                CacheManager.GetObject(obj.CommonData);
            }
        }

        private static void RefreshCache(int acdGuid, ACD acdObject, CacheObject cacheObject)
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

        private static CacheObject GetCache(int acdGuid, ACD acdObject)
        {
            switch (acdObject.ActorType)
            {
                case ActorType.Unit:
                    return new CacheUnit(acdObject);
                case ActorType.Gizmo:
                    return new CacheGizmo(acdObject);
                case ActorType.Item:
                    return new CacheItem(acdObject);
                default:
                    if (CacheUtils.IsAvoidanceSNO(acdObject.ActorSNO))
                    {
                        return new CacheAvoidance(acdObject);
                    }
                    else
                    {
                        return new CacheOther(acdObject);
                    }
            }
        }
    }
}
