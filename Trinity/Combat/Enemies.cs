using System;
using System.Collections.Generic;
using System.Linq;
using Trinity.Configuration;
using Trinity.Config.Combat;
using Zeta.Common;
using Zeta.Game;
using Zeta.Game.Internals.Actors;

namespace Trinity.Combat
{
    public static class Enemies
    {
        public static List<TrinityCacheObject> Alive = new List<TrinityCacheObject>();
        public static List<TrinityCacheObject> Dead = new List<TrinityCacheObject>();
        public static HashSet<int> DeadGuids = new HashSet<int>();
        public static HashSet<int> AliveGuids = new HashSet<int>();
        public static TargetArea Nearby = new TargetArea(70f);
        public static TargetArea CloseNearby = new TargetArea(15f);
        public static TargetArea AtPlayerNearby = new TargetArea(10f);
        public static TargetCluster BestCluster = new TargetCluster(20f);
        public static TargetCluster BestLargeCluster = new TargetCluster(24f, 8);

        public static void Update()
        {
            if (!ZetaDia.IsInGame || !ZetaDia.Me.IsValid || Trinity.ObjectCache == null || !Trinity.ObjectCache.Any())
                return;

            List<TrinityCacheObject> units = Trinity.ObjectCache.Where(o => o.IsUnit && o.CommonDataIsValid || o.IsBossOrEliteRareUnique).ToList();
            var unitsGuids = new HashSet<int>(units.Select(e => e.ACDGuid));

            // Find Newly Dead Units
            List<TrinityCacheObject> newlyDead = Alive.Where(a => !unitsGuids.Contains(a.ACDGuid) && !DeadGuids.Contains(a.ACDGuid)).ToList();
            newlyDead.ForEach(u => Events.OnUnitAliveHandler.Invoke(u));
            Dead.AddRange(newlyDead);
            Dead.RemoveAll(e => DateTime.UtcNow.Subtract(e.LastSeenTime).TotalSeconds > 60);
            DeadGuids = new HashSet<int>(Dead.Select(e => e.ACDGuid));

            // Find Newly Alive Units
            var newlyAliveGuids = new HashSet<int>(units.Where(a => !AliveGuids.Contains(a.ACDGuid)).Select(a => a.ACDGuid));
            Alive = units;
            AliveGuids = unitsGuids;
            Alive.Where(u => newlyAliveGuids.Contains(u.ACDGuid)).ForEach(u => Events.OnUnitDeathHandler.Invoke(u));

            Nearby.Update();
            CloseNearby.Update();
            AtPlayerNearby.Update();
            BestCluster.Update();
            BestLargeCluster.Update();

        }
    }

    public class TargetArea
    {
        public TargetArea (float range = 20f, Vector3 position = new Vector3())
        {
            if (position == Vector3.Zero)
                NearMe = true;

            Units = new List<TrinityCacheObject>();
            UnitsACDGuid = new HashSet<int>();
            Position = position;
            Range = range;
            Update();
        }

        public Vector3 Position { get; set; }
        public float Range { get; set; }
        public int EliteCount { get; set; }
        public int BossCount { get; set; }
        public int UnitCount { get; set; }
        public int UnitInLosCount { get; set; }
        public bool NearMe { get; set; }
        public List<TrinityCacheObject> Units { get; set; }
        public HashSet<int> UnitsACDGuid { get; set; }

        public double AverageHealthPct
        {
            get { return Units.Any() ? Units.Average(u => u.HitPointsPct) : 0; }
        }

        public void Update()
        {
            if (NearMe)
                Position = Trinity.Player.Position;

            if (Position == Vector3.Zero)
                return;

            if (Trinity.ObjectCache == null || !Trinity.ObjectCache.Any())
                return;

            Units = TargetUtil.ListUnitsInRangeOfPosition(Position, Range);
            UnitsACDGuid = new HashSet<int>(Units.Select(u => u.ACDGuid));
            EliteCount = TargetUtil.NumElitesInRangeOfPosition(Position, Range);
            UnitCount = TargetUtil.NumMobsInRangeOfPosition(Position, Range);
            UnitInLosCount = TargetUtil.NumMobsInLosInRangeOfPosition(Position, Range);
            BossCount = TargetUtil.NumBossInRangeOfPosition(Position, Range);
        }

        public int TotalDebuffCount (IEnumerable<SNOPower> powers)
        {
            return Units.Any() ? TargetUtil.DebuffCount(powers, Units) : 0;
        }

        public int DebuffedCount (IEnumerable<SNOPower> powers)
        {
            return Units.Any() ? TargetUtil.MobsWithDebuff(powers, Units) : 0;
        }

        public float DebuffedPercent (IEnumerable<SNOPower> powers)
        {
            return Units.Any() ? DebuffedCount(powers)/Units.Count : 0;
        }

        internal TrinityCacheObject GetTargetWithoutDebuffs(IEnumerable<SNOPower> debuffs)
        {
            return TargetUtil.BestTargetWithoutDebuffs(Range, debuffs, Position);
        }
    }

    public class TargetCluster : TargetArea
    {
        public TargetCluster (float radiusOfCluster = 50f, int minUnitsInCluster = 1)
        {
            Radius = Math.Min(Trinity.Settings.Combat.Misc.TrashPackClusterRadius, radiusOfCluster > 5 ? radiusOfCluster : 5);
            Size = Math.Max(Trinity.Settings.Combat.Misc.TrashPackSize, minUnitsInCluster < 1 ? minUnitsInCluster : 1);
            Update();
        }

        public float Radius { get; set; }
        public int Size { get; set; }
        public TargetArea TargetArea { get; set; }

        public bool Exists
        {
            get { return TargetUtil.ClusterExists(Radius, Size); }
        }

        public bool GridLocExists
        {
            get { return GridMap.ClusterNodeExist;  }
        }

        public new void Update()
        {
            Position = GridMap.GetBestClusterNode(new Vector3(), Radius, Size).Position;
            NearMe = false;
            Range = Radius;
            base.Update();

            TargetArea = new TargetArea(Radius, Position);
        }

        internal TrinityCacheObject GetTargetWithoutDebuffs(IEnumerable<SNOPower> debuffs)
        {
            return TargetUtil.BestTargetWithoutDebuffs(Range, debuffs, Position);
        }
    }
}