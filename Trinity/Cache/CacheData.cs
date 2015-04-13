using System;
using System.Collections.Generic;
using System.Linq;
using Trinity.Cache;
using Trinity.Technicals;
using Zeta.Common;
using Zeta.Game.Internals.Actors;
using Zeta.Game.Internals.SNO;
using Logger = Trinity.Technicals.Logger;

namespace Trinity
{
    public partial class CacheData
    {
        /* 
         * This set of dictionaries are used for performance increases throughout, and a minimization of DB mis-read/null exception errors
         * Uses a little more ram - but for a massive CPU gain. And ram is cheap, CPU is not!
         */

        /// <summary>
        /// Contains the time we last used a spell
        /// </summary>
        public static Dictionary<SNOPower, DateTime> AbilityLastUsed { get { return abilityLastUsedCache; } internal set { abilityLastUsedCache = value; } }
        private static Dictionary<SNOPower, DateTime> abilityLastUsedCache = new Dictionary<SNOPower, DateTime>();

        /// <summary>
        /// Special cache for monster types {ActorSNO, MonsterType}
        /// </summary>
        internal static Dictionary<int, string> ObjectsIgnored = new Dictionary<int, string>();
        /// <summary>
        /// Special cache for monster types {ActorSNO, MonsterType}
        /// </summary>
        internal static Dictionary<int, MonsterType> MonsterTypes = new Dictionary<int, MonsterType>();

        /// <summary>
        /// Special cache for Monster sizes {ActorSNO, MonsterSize}
        /// </summary>
        internal static Dictionary<int, MonsterSize> MonsterSizes = new Dictionary<int, MonsterSize>();

        /// <summary>
        /// Caches monster affixes for the monster ID, as this value can be a pain to get and slow (RactorGUID based)
        /// </summary>
        internal static Dictionary<int, MonsterAffixes> UnitMonsterAffix = new Dictionary<int, MonsterAffixes>();

        /// <summary>
        /// Caches each monster's max-health, since this never changes (RactorGUID based)
        /// </summary>
        internal static Dictionary<int, double> UnitMaxHealth = new Dictionary<int, double>();

        /// <summary>
        /// Caches each monster's current health for brief periods  (RactorGUID based)
        /// </summary>
        internal static Dictionary<int, double> CurrentUnitHealth = new Dictionary<int, double>();

        /// <summary>
        /// Stores when we last checked a units health (we don't check too fast, to avoid hitting game memory)
        /// </summary>
        internal static Dictionary<int, int> LastCheckedUnitHealth = new Dictionary<int, int>();

        /// <summary>
        /// Store a "not-burrowed" value for monsters that we have already checked a burrowed-status of and found false (RactorGUID based)
        /// </summary>
        internal static Dictionary<int, bool> UnitIsBurrowed = new Dictionary<int, bool>();

        /// <summary>
        /// Caches the position for each object (only used for non-units, as this data is static so can be cached) (RactorGUID based)
        /// </summary>
        internal static Dictionary<int, Vector3> Position = new Dictionary<int, Vector3>();

        /// <summary>
        /// Same as above but for gold-amount of pile (RactorGUID based)
        /// </summary>
        internal static Dictionary<int, int> GoldStack = new Dictionary<int, int>();
        ///// <summary>
        ///// Same as above but for quality of item, we check this twice to make bloody sure we don't miss a legendary from a mis-read though (RactorGUID based)
        ///// </summary>
        internal static Dictionary<int, ItemQuality> ItemQuality = new Dictionary<int, ItemQuality>();

        /// <summary>
        /// Same as above but for whether we want to pick it up or not (RactorGUID based)
        /// </summary>
        internal static Dictionary<int, bool> PickupItem = new Dictionary<int, bool>();

        /// <summary>
        /// How many times the player tried to interact with this object in total
        /// </summary>
        internal static Dictionary<int, int> InteractAttempts = new Dictionary<int, int>();
        /// <summary>
        /// Summoned-by ID for units (RactorGUID based)
        /// </summary>
        internal static Dictionary<int, int> SummonedByACDId = new Dictionary<int, int>();

        /// <summary>
        /// If a unit, item, or other object has been raycastable before, this will contain true value and will be considered for targetting, otherwise we will continue to check
        /// </summary>
        internal static Dictionary<int, Tuple<bool, int>> RayCastResultsFromObjects = new Dictionary<int, Tuple<bool, int>>();

        /// <summary>
        /// Record of items that have been on the ground
        /// </summary>
        internal static HashSet<PickupItem> DroppedItems = new HashSet<PickupItem>();

        /// <summary>
        /// Stores the computed ItemQuality from an ACDItem.ItemLink (ACDGuid based)
        /// </summary>
        internal static Dictionary<int, ItemQuality> ItemLinkQuality = new Dictionary<int, ItemQuality>();

        /// <summary>
        /// Stores if a unit/monster is a Summoner (spawns other units) (ACDGuid based)
        /// </summary>
        internal static Dictionary<int, bool> IsSummoner = new Dictionary<int, bool>();

        /// <summary>
        /// A list of sentry on area
        /// </summary>
        internal static HashSet<CacheObstacleObject> SentryTurret = new HashSet<CacheObstacleObject>();

        /// <summary>
        /// A list of bbv on area
        /// </summary>
        internal static HashSet<CacheObstacleObject> Voodoo = new HashSet<CacheObstacleObject>();

        /// <summary>
        /// Obstacle cache, things we can't or shouldn't move through
        /// </summary>
        internal static HashSet<CacheObstacleObject> NavigationObstacles = new HashSet<CacheObstacleObject>();
        internal static Dictionary<int, float> DictionaryObstacles = new Dictionary<int, float>();

        internal static Dictionary<Vector3, float> NavRayCastObstacles = new Dictionary<Vector3, float>();

        /// <summary>
        /// A list of all monsters and their positions, so we don't try to walk through them during avoidance
        /// </summary>
        internal static HashSet<CacheObstacleObject> MonsterObstacles = new HashSet<CacheObstacleObject>();

        /// <summary>
        /// A list of all current obstacles, to help avoid running through them when picking targets
        /// </summary>
        internal static HashSet<CacheObstacleObject> AvoidanceObstacles = new HashSet<CacheObstacleObject>();

        /// <summary>
        /// Contains an RActorGUID and count of the number of times we've switched to this target
        /// </summary>
        internal static Dictionary<string, int> PrimaryTargetCount = new Dictionary<string, int>();

        /// <summary>
        /// Events that have expired without being completed
        /// </summary>
        internal static HashSet<int> BlacklistedEvents = new HashSet<int>();

        /// <summary>
        /// Cache for low weight/priority objects, so we dont have to refresh them every tick.
        /// </summary>
        internal static Dictionary<int, TrinityCacheObject> LowPriorityObjectCache = new Dictionary<int, TrinityCacheObject>();

        /// <summary>
        /// Dictionary of PositionCache.Cache
        /// </summary>
        internal static Dictionary<Vector3, int> VisitedZones = new Dictionary<Vector3, int>();

        // <summary>
        /// Unsafe boss area
        /// </summary>
        internal static Dictionary<Vector3, float> UnSafeZones = new Dictionary<Vector3, float>();

        /// <summary>
        /// To set avoidance at player just one time
        /// </summary>
        internal static HashSet<int> ObsoleteAvoidancesAtPlayer = new HashSet<int>();

        internal static Dictionary<Tuple<int, int>, int> NearbyUnitsWithinDistanceRecorded = new Dictionary<Tuple<int, int>, int>();
        internal static Dictionary<Tuple<int, int>, double> UnitsWeightsWithinDistanceRecorded = new Dictionary<Tuple<int, int>, double>();

        public static InventoryCache Inventory
        {
            get { return InventoryCache.Instance; }            
        }

        public static PlayerCache Player
        {
            get { return PlayerCache.Instance; }
        }

        public static HotbarCache Hotbar
        {
            get { return HotbarCache.Instance; }
        }

        public static BuffsCache Buffs
        {
            get { return BuffsCache.Instance; }
        }

        internal static void SetDictionary()
        {
            if (!Trinity.Player.IsInRift)
            {
                foreach (UnSafeZone a in UnSafeZone.UnsafeKiteAreas)
                {
                    if (!CacheData.UnSafeZones.ContainsKey(a.Position) &&
                        a.WorldId == Trinity.Player.WorldID && a.Position.Distance2D(Trinity.Player.Position) <= a.Radius + 40f)
                    {
                        CacheData.UnSafeZones.Add(MainGrid.VectorToGrid(a.Position), a.Radius);
                    }
                }
            }
        }

        internal static bool AddToNavigationObstacles(CacheObstacleObject cacheObject)
        {
            try
            {
                if (cacheObject.Distance < 150f && !DictionaryObstacles.ContainsKey(cacheObject.RActorGUID))
            {
                NavigationObstacles.Add(cacheObject);
                DictionaryObstacles.Add(cacheObject.RActorGUID, cacheObject.ActorSNO);
                NavRayCastObstacles.Add(cacheObject.Position, cacheObject.Radius);

                return true;
            }
            }
            catch { }
            return false;
        }

        internal static void ClearNavigationObstacles()
        {
            foreach (CacheObstacleObject cacheObject in NavigationObstacles.Where(o => o.Distance >= 150).ToList())
            {
                NavigationObstacles.Remove(cacheObject);
                DictionaryObstacles.Remove(cacheObject.RActorGUID);
                NavRayCastObstacles.Remove(cacheObject.Position);
            }
        }

        private static int Tick = 0;
        /// <summary>
        /// Called every cache-refresh
        /// </summary>
        internal static void Clear()
        {
            if (Tick > 100) Tick = 0;
            Tick++;

            /* Every ticks */
            MonsterObstacles.Clear();
            AvoidanceObstacles.Clear();

            /* FORK ADD - Every ticks */
            ClearNavigationObstacles();
            UnSafeZones.Clear();
            SentryTurret.Clear();
            Voodoo.Clear();
            NearbyUnitsWithinDistanceRecorded.Clear();
            UnitsWeightsWithinDistanceRecorded.Clear();

            /* Every 10 ticks */
            if (Tick % 10 == 0)
            {
                /* Obsolet trinity cache does not use them 
                * CurrentUnitHealth.Clear();
                * LastCheckedUnitHealth.Clear();
                * MonsterTypes.Clear();
                * Position.Clear();*/

                ObjectsIgnored.Clear();
            }
        }

        /// <summary>
        /// Called on bot stop, new game, join game, etc
        /// </summary>
        internal static void FullClear()
        {
            Clear();
            ClearObsolete();
            WorldChangedClear();
            DroppedItems.Clear();
        }

        internal static void ClearObsolete()
        {
            if (Tick % 3 == 0 && Trinity.ObjectCache != null)
            {
                ObsoleteAvoidancesAtPlayer.RemoveWhere(a => Trinity.ObjectCache.All(o => o.ActorSNO != a));
                RayCastResultsFromObjects.ToList()
                    .Where(i => Trinity.ObjectCache.All(o => o.RActorGuid != i.Key))
                    .ToList()
                    .ForEach(i => RayCastResultsFromObjects.Remove(i.Key));
            }
        }

        internal static void WorldChangedClear()
        {
            AbilityLastUsed = new Dictionary<SNOPower, DateTime>(DataDictionary.LastUseAbilityTimeDefaults);
            AvoidanceObstacles.Clear();
            GoldStack.Clear();
            InteractAttempts.Clear();
            IsSummoner.Clear();
            ItemLinkQuality.Clear();
            NavigationObstacles.Clear();
            PickupItem.Clear();
            PrimaryTargetCount.Clear();
            AvoidanceObstacles.Clear();
            BlacklistedEvents.Clear();
            LowPriorityObjectCache.Clear();
            ObsoleteAvoidancesAtPlayer.Clear();
            Player.ForceUpdates();
            MonsterSizes.Clear();
            UnitIsBurrowed.Clear();
            UnitMaxHealth.Clear();
            UnitMonsterAffix.Clear();
            SummonedByACDId.Clear();
            IsSummoner.Clear();
        }
    }
}
