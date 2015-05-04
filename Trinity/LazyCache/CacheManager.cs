using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.SqlTypes;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security;
using Trinity.Technicals;
using Zeta.Bot;
using Zeta.Common;
using Zeta.Game;
using Zeta.Game.Internals.Actors;
using Zeta.Game.Internals.SNO;
using Logger = Trinity.Technicals.Logger;

namespace Trinity.LazyCache
{
    /*  
    * - Properties can be configured with their own cache duration.
    * - If the duration hasn't expired on a property, a cached value is returned instead.
    * - Things aren't excluded from the cache for the purposes of targetting.
    */

    /// <summary>
    /// LazyCache creates a caching layer between DB's memory reading and Trinity to 
    /// avoid wasting time on reading values that don't change very fast or at all.
    /// </summary>
    public static class CacheManager
    {
        #region Binding

        static CacheManager()
        {
            Start();
        }

        public static void Start()
        {
            if (IsRunning)
                return;

            IsRunning = true;
            Pulsator.OnPulse += PulsatorOnPulse;
        }

        private static void PulsatorOnPulse(object sender, EventArgs eventArgs)
        {
            Update();
        }

        public static void Stop()
        {
            Pulsator.OnPulse -= PulsatorOnPulse;
            IsRunning = false;
        }

        #endregion

        #region Constants

        /// <summary>
        /// How long after not having seen an object before it is removed from the cache.
        /// </summary>
        private const int PurgeLimitSeconds = 5;

        #endregion

        #region Fields

        /// <summary>
        /// The Primary DataStore of CacheObjects
        /// </summary>
        public static readonly ConcurrentDictionary<int, TrinityObject> CachedObjects = new ConcurrentDictionary<int, TrinityObject>();

        /// <summary>
        /// If execution is within a using(CacheManager.ForceRefresh()) block
        /// </summary>
        internal static int ForceRefreshLevel;

        public static DateTime LastUpdated = DateTime.MinValue;

        private static Dictionary<int, DiaObject> _rActorByACDGuid = new Dictionary<int, DiaObject>();

        private static Dictionary<int, TrinityObject> _actorsByRActorGuid = new Dictionary<int, TrinityObject>();

        #endregion

        #region Properties

        public static List<TrinityUnit> Monsters
        {
            get { return GetActorsOfType<TrinityUnit>().Where(i => i.IsHostile).ToList(); }
        }

        public static List<TrinityObject> Gold
        {
            get { return GetActorsOfType<TrinityObject>().Where(i => i.TrinityType == TrinityObjectType.Gold).ToList(); }
        }

        public static List<TrinityObject> Containers
        {
            get { return GetActorsOfType<TrinityObject>().Where(i => i.TrinityType == TrinityObjectType.Container).ToList(); }
        }

        public static List<TrinityObject> Destructibles
        {
            get { return GetActorsOfType<TrinityObject>().Where(i => i.TrinityType == TrinityObjectType.Destructible).ToList(); }
        }

        public static List<TrinityObject> Doors
        {
            get { return GetActorsOfType<TrinityObject>().Where(i => i.TrinityType == TrinityObjectType.Door).ToList(); }
        }

        public static List<TrinityGizmo> Shrines
        {
            get { return GetActorsOfType<TrinityGizmo>().Where(i => i.IsShrine).ToList(); }
        }

        public static List<TrinityPlayer> Players
        {
            get { return GetActorsOfType<TrinityPlayer>(); }
        }

        public static List<TrinityObject> Objects
        {
            get { return GetActorsOfType<TrinityObject>(); }
        }

        public static List<TrinityUnit> Units
        {
            get { return GetActorsOfType<TrinityUnit>(); }
        }

        public static List<TrinityUnit> EliteRareUniqueBoss
        {
            get { return GetActorsOfType<TrinityUnit>().Where(i => i.IsBossOrEliteRareUnique).ToList(); }
        }

        public static List<TrinityUnit> Goblins
        {
            get { return GetActorsOfType<TrinityUnit>().Where(i => i.IsTreasureGoblin).ToList(); }
        }

        public static List<TrinityAvoidance> Avoidances
        {
            get { return GetActorsOfType<TrinityAvoidance>().Where(i => i.TrinityType == TrinityObjectType.Avoidance).ToList(); }
        }

        public static List<TrinityObject> Globes
        {
            get { return GetActorsOfType<TrinityObject>().Where(i => i.IsGlobe).ToList(); }
        }

        public static List<TrinityGizmo> Gizmos
        {
            get { return GetActorsOfType<TrinityGizmo>(); }
        }

        //public static List<TrinityItem> Stash
        //{
        //    get { return GetActorsOfType<TrinityItem>().Where(i => i.InventorySlot == InventorySlot.SharedStash).ToList(); }
        //}

        //public static List<TrinityItem> Backpack
        //{
        //    get { return GetActorsOfType<TrinityItem>().Where(i => i.InventorySlot == InventorySlot.BackpackItems).ToList(); }
        //}

        //public static List<TrinityItem> Equipped
        //{
        //    get { return GetActorsOfType<TrinityItem>().Where(i => i.IsEquipped).ToList(); }
        //}

        public static List<TrinityItem> Items
        {
            get { return GetActorsOfType<TrinityItem>().ToList(); }
        }

        public static List<TrinityUnit> Pets
        {
            get { return GetActorsOfType<TrinityUnit>().Where(i => i.IsSummonedByPlayer).ToList(); }
        }

        public static TrinityPlayer Me { get; private set; }

        public static bool IsRunning { get; private set; }

        #endregion

        #region Events

        /// <summary>
        /// Arguments for all cache management related events
        /// </summary>
        public class CacheManagementEventArgs
        {
            public int Added { get; set; }
            public int Removed { get; set; }
            public int Total { get; set; }
            public int Updated { get; set; }
            public int Excluded { get; set; }
            public long Time { get; set; }
        }

        /// <summary>
        /// Event Delegate for all cache management related events
        /// </summary>
        public delegate void CacheManagementEvent(CacheManagementEventArgs args);

        /// <summary>
        /// Event that fires whenever the cache is updated
        /// </summary> 
        public static event CacheManagementEvent CacheUpdated = args => { };

        #endregion

        #region Methods

        /// <summary>
        /// Update the references between CachedObjects and ACDs
        /// </summary>
        public static void Update()
        {
            int addedCount = 0, updatedCount = 0, removedCount = 0, excludedCount = 0;

            if (!ZetaDia.IsInGame)
                return;

            if (Me == null || !Me.IsValid)
            {
                Me = new TrinityPlayer(ZetaDia.Me.CommonData);
            }
            else
            {
                Me.UpdateSource(ZetaDia.Me.CommonData);
            }

            LastUpdated = DateTime.UtcNow;
                
            var stopwatch = Stopwatch.StartNew();

            _rActorByACDGuid = ZetaDia.Actors.RActorList.OfType<DiaObject>().DistinctBy(i => i.ACDGuid).ToDictionary(k => k.ACDGuid, v => v);

            using (new PerformanceLogger("LazyCache.Update.Objects"))
            {
                foreach (var acd in ZetaDia.Actors.ACDList.OfType<ACD>())
                {
                    if (!acd.IsProperValid() || 
                        DataDictionary.ExcludedActorTypes.Contains(acd.ActorType) ||
                        TrinityObject.IsIgnoredName(acd.Name) ||
                        acd is ACDItem && (acd as ACDItem).InventorySlot != InventorySlot.None) // Ignore Non-Ground Items
                    {
                        excludedCount++;
                        continue;
                    }

                    CachedObjects.AddOrUpdate(acd.ACDGuid, i =>
                    {
                        addedCount++;
                        return CacheFactory.CreateTypedTrinityObject(acd);

                    }, (key, existingActor) =>
                    {
                        updatedCount++;
                        existingActor.UpdateSource(acd);
                        return existingActor;
                    });
                }
            }

            using (new PerformanceLogger("LazyCache.Update.Purge"))
            {
                CachedObjects.ForEach(o =>
                {
                    var isOld = LastUpdated.Subtract(o.Value.LastUpdated).TotalSeconds > PurgeLimitSeconds;

                    if ((!CacheUtilities.IsProperValid(o.Value.Source) || isOld) && CachedObjects.TryRemove(o.Key, o.Value))
                    {   
                        removedCount++;
                    }
                });
            }

            _actorsByRActorGuid = CachedObjects.Values.OfType<TrinityObject>().DistinctBy(i => i.RActorGuid).ToDictionary(k => k.RActorGuid, v => v);

            stopwatch.Stop();

            using (new PerformanceLogger("LazyCache.Update.Event"))
            {
                CacheUpdated(new CacheManagementEventArgs
                {
                    Added = addedCount,
                    Removed = removedCount,
                    Updated = updatedCount,
                    Excluded = excludedCount,
                    Total = CachedObjects.Count,                    
                    Time = stopwatch.ElapsedMilliseconds
                });
            }
        }

        public static T GetRActorOfTypeByACDGuid<T>(int acdGuid) where T : class
        {
            DiaObject rActor;
            return _rActorByACDGuid.TryGetValue(acdGuid, out rActor) ? rActor as T : null;
        }

        /// <summary>
        /// Get actors of a particular TrinityObjectType
        /// </summary>
        public static List<T> GetActorsOfType<T>()
        {
            return CachedObjects.Values.OfType<T>().ToList();
        }

        /// <summary>
        /// Get a specific actor by ACDGuid
        /// </summary>
        public static T GetActorByACDGuid<T>(int ACDGuid) where T : CacheBase
        {
            if (!CachedObjects.Any() || ACDGuid < 0)
                return null;

            TrinityObject obj;
            return CachedObjects.TryGetValue(ACDGuid, out obj) ? CacheFactory.CreateObject<T>(obj) : null;
        }

        /// <summary>
        /// Get a specific actor by RActorGuid
        /// </summary>
        public static T GetActorByRActorGuid<T>(int rActorGuid) where T : CacheBase
        {
            if (!CachedObjects.Any() || rActorGuid < 0)
                return null;

            TrinityObject obj;
            return _actorsByRActorGuid.TryGetValue(rActorGuid, out obj) ? CacheFactory.CreateObject<T>(obj) : null;
        }

        /// <summary>
        /// using (CacheManager.ForceRefresh())
        /// </summary>
        public static IDisposable ForceRefresh()
        {
            return new CacheUtilities.ForceRefreshHelper();
        }

        #endregion

    }

}
