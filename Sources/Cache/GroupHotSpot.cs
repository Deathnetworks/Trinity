﻿using System;
using System.Collections.Generic;
using System.Linq;
using Trinity.Technicals;
using Zeta.Common;

namespace Trinity
{
    public class GroupHotSpots
    {
        /*
         * The follower will read the HotSpot.CurrentTargetHotSpot and store it in the next message
         * The leader will read the HotSpot from the follower message and invoke the AddHotSpot(string xml) method
         * 
         * The leader will do the same, in reverse (so the followers have the leaders hotspots as well)
         */

        private static System.Threading.Thread ManagerThread;

        public static HashSet<HotSpot> HotSpotList
        {
            get { return GroupHotSpots.hotSpotList; }
            set { GroupHotSpots.hotSpotList = value; }
        }
        private static HashSet<HotSpot> hotSpotList = new HashSet<HotSpot>();

        static GroupHotSpots()
        {
            ManagerThread = new System.Threading.Thread(HotSpotManager)
            {
                IsBackground = true,
                Priority = System.Threading.ThreadPriority.Lowest
            };
            ManagerThread.Start();
        }

        public static void AddHotSpot(Vector3 location, DateTime expirationTime, int worldId)
        {
            AddHotSpot(new HotSpot(location, expirationTime, worldId));
        }
        public static void AddHotSpot(HotSpot hotSpot)
        {
            if (hotSpot.IsValid && !hotSpotList.Any(h => h.Equals(hotSpot)))
            {
                Logger.Log(TrinityLogLevel.Debug, LogCategory.CacheManagement, "Added HotSpot {0}", hotSpot);
                hotSpotList.Add(hotSpot);
            }
        }
        public static void AddSerializedHotSpot(string xml)
        {
            if (xml == null)
                return;

            HotSpot hotSpot = HotSpot.Deserialize(xml);

            AddHotSpot(hotSpot);
        }

        private static void HotSpotManager()
        {
            while (true)
            {
                try
                {
                    System.Threading.Thread.Sleep(Trinity.Settings.Advanced.CacheRefreshRate);

                    lock (hotSpotList)
                    {
                        var hotSpots = hotSpotList;
                        foreach (var hotspot in hotSpots.ToList())
                        {
                            if (DateTime.Now.Subtract(hotspot.ExpirationTime).TotalMilliseconds > 0)
                            {
                                hotSpotList.Remove(hotspot);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogNormal("Exception in HotSpotManager: {0}", ex.ToString());
                }
            }
        }

        internal static List<TrinityCacheObject> GetCacheObjectHotSpots()
        {
            List<TrinityCacheObject> list = new List<TrinityCacheObject>();
            foreach (var hotSpot in HotSpotList.Where(s => s.Location.Distance2D(Trinity.Player.Position) <= V.F("Cache.HotSpot.MaxDistance") && s.Location.Distance2D(Trinity.Player.Position) >= V.F("Cache.HotSpot.MinDistance") && s.WorldId == Trinity.Player.WorldID))
            {
                Logger.Log(TrinityLogLevel.Debug, LogCategory.CacheManagement, "Adding HotSpot to Cache: {0}", hotSpot);
                var o = new TrinityCacheObject()
                {
                    Position = hotSpot.Location,
                    InternalName = "HotSpot",
                    Type = GObjectType.HotSpot,
                    CentreDistance = Trinity.Player.Position.Distance2D(hotSpot.Location),
                    RadiusDistance = Math.Max(Trinity.Player.Position.Distance2D(hotSpot.Location) - HotSpot.MaxPositionDistance, 1f),
                    Radius = HotSpot.MaxPositionDistance,
                };

                list.Add(o);
            }

            return list;
        }

        internal static bool LocationIsInHotSpot(Vector3 position, int worldId)
        {
            return HotSpotList.Any(h => h.Equals(new HotSpot(position, worldId)));
        }

        internal static bool CacheObjectIsInHotSpot(TrinityCacheObject cacheObject)
        {
            return LocationIsInHotSpot(cacheObject.Position, Trinity.Player.WorldID);
        }

    }
}