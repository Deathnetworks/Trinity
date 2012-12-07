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
using Zeta.Common;
using GilesTrinity.Technicals;

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
            using (new PerformanceLogger("CacheManagement.RefreshUnit"))
            {
                try
                {
                    ACD acd = unit.CommonData;

                    unit.Position = acd.Position;
                    unit.CentreDistance = Vector3.Distance(GilesTrinity.playerStatus.CurrentPosition, acd.Position);

                    if (unit.InternalUnit != null)
                    {
                        unit.HitpointsCurrent = unit.InternalUnit.HitpointsCurrent;
                        if (unit.HitpointsCurrent <= 0)
                        {
                            unit.IsDead = true;
                        }

                        unit.IsBurrowed = unit.InternalUnit.IsBurrowed;
                        unit.IsUntargetable = unit.InternalUnit.IsUntargetable;
                        unit.IsInvulnerable = unit.InternalUnit.IsInvulnerable;
                    }
                    unit.CurrentAnimation = acd.CurrentAnimation;
                    
                    if (unit.IsBoss)
                    {
                        unit.MonsterType = acd.MonsterInfo.MonsterType;
                    }
                }
                catch
                {
                }
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
                ACDItem acd = (ACDItem)item.CommonData;
                item.Distance = acd.Position.Distance(ZetaDia.Me.Position);
                if (acd.IsUnidentified != item.IsUnidentified || item.Gold>0)
                {
                    CacheItem.ComputeItemProperty(item);
                }
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
            using (ZetaDia.Memory.AcquireFrame())
            {
                try
                {
                    ZetaDia.Actors.Update();
                    CacheManager.CacheObjectGetter = GetCache;
                    CacheManager.CacheObjectRefresher = RefreshCache;
                    CacheManager.MaxRefreshRate = 100;
                    CacheManager.DefineStaleFlag();
                    foreach (DiaObject obj in ZetaDia.Actors.GetActorsOfType<DiaObject>(true, false).Where(o => o.IsACDBased && o.CommonData != null))
                    {
                        CacheManager.RefreshObject(obj);
                    }
                }
                catch (Exception ex) {
                    DbHelper.Log(TrinityLogLevel.Debug, LogCategory.CacheManagement, "Exception occured refreshing cache: {0}", ex.Message);
                    DbHelper.Log(TrinityLogLevel.Debug, LogCategory.CacheManagement, "{0}", ex.StackTrace);
                }
            }
        }

        private static void RefreshCache(DiaObject diaObject, CacheObject cacheObject)
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
        private static CacheObject GetCache(DiaObject diaObject)
        {
            switch (diaObject.ActorType)
            {
                case ActorType.Unit:
                    return new CacheUnit(diaObject);
                case ActorType.Gizmo:
                    return new CacheGizmo(diaObject);
                case ActorType.Item:
                    return new CacheItem(diaObject);
                default:
                    if (CacheUtils.IsAvoidanceSNO(diaObject.ActorSNO))
                    {
                        return new CacheAvoidance(diaObject);
                    }
                    else
                    {
                        return new CacheOther(diaObject);
                    }
            }
        }
    }
}
