using System;
using System.Collections.Generic;
using System.Linq;
using Trinity.Technicals;
using Zeta.Common;
using Zeta.Game;

namespace Trinity
{
    public class PositionCache : IEquatable<PositionCache>
    {
        public Vector3 Position { get; set; }
        public DateTime RecordedAt { get; set; }
        public int WorldId { get; set; }

        public PositionCache()
        {
            if (ZetaDia.Me != null && ZetaDia.Me.IsValid)
            {
                Vector3 playerLoc = MainGrid.VectorToGrid(Trinity.Player.Position);
                if (!CacheData.VisitedZones.ContainsKey(playerLoc))
                {
                    CacheData.VisitedZones.Add(playerLoc, Trinity.Player.WorldID);
                    Position = playerLoc;
                    RecordedAt = DateTime.UtcNow;
                    WorldId = Trinity.Player.WorldID;
                }
            }
        }

        public static HashSet<PositionCache> Cache = new HashSet<PositionCache>();

        /// <summary>
        /// Adds the current position as needed and maintains the cache
        /// </summary>
        /// <param name="distance"></param>
        public static void AddPosition(float distance = 5f)
        {
            MaintainCache();

            if (Cache.Any(p => DateTime.UtcNow.Subtract(p.RecordedAt).TotalMilliseconds < 100))
                return;

            foreach (PositionCache p in Cache.Where(p => p.Position.Distance2D(Trinity.Player.Position) < distance).ToList())
            {
                CacheData.VisitedZones.Remove(p.Position);
                Cache.Remove(p);
            }
            Cache.Add(new PositionCache());
        }

        /// <summary>
        /// Removes stale objects from the cache
        /// </summary>
        public static void MaintainCache()
        {
            int worldId = ZetaDia.CurrentWorldId;
            foreach (PositionCache p in Cache.Where(p => p.WorldId != worldId))
            {
                CacheData.VisitedZones.Remove(p.Position);
                Cache.Remove(p);
            }

        }

        public bool Equals(PositionCache other)
        {
            return this.Position == other.Position && this.WorldId == other.WorldId;
        }
    }
}
