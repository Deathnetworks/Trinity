using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Trinity.Technicals;
using Zeta.Common;

namespace Trinity
{
    public class GroupHotSpots 
    {
        private const float MaxPositionDistance = 5f;
        private static System.Threading.Thread ManagerThread;

        public class HotSpot : IEquatable<HotSpot>
        {
            public Vector3 Location { get; set; }
            public DateTime ExpirationTime { get; set; }
            public int WorldId { get; set; }
            public HotSpot()
            {

            }
            public HotSpot(Vector3 location, DateTime expirationTime, int worldId)
            {
                Location = location;
                ExpirationTime = expirationTime;
                WorldId = worldId;
            }
            public bool Equals(HotSpot other)
            {
                return this.Location.Distance2DSqr(other.Location) < MaxPositionDistance;
            }
        }

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
            if (!hotSpotList.Any(h => h.Equals(h)))
            {
                hotSpotList.Add(hotSpot);
            }
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
                        foreach (var hotspot in hotSpots)
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

    }
}
