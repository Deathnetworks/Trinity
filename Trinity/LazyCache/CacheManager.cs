#region Usings

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Threading;
using Trinity.Combat.Weighting;
using Trinity.Helpers;
using Trinity.Technicals;
using Trinity.UIComponents;
using Zeta.Bot;
using Zeta.Common;
using Zeta.Game;
using Zeta.Game.Internals;
using Zeta.Game.Internals.Actors;
using Zeta.Game.Internals.SNO;
using Logger = Trinity.Technicals.Logger;

#endregion

namespace Trinity.LazyCache
{
    /// <summary>
    ///     LazyCache creates a caching layer between DB's memory reading and Trinity to
    ///     avoid wasting time on reading values that don't change very fast or at all.
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

            IsUpdatePending = true;
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

        #region Fields

        private static readonly CacheField<List<TrinityUnit>> _units = new CacheField<List<TrinityUnit>>(UpdateSpeed.Ultra, new List<TrinityUnit>());
        private static readonly CacheField<List<TrinityUnit>> _monsters = new CacheField<List<TrinityUnit>>(UpdateSpeed.Ultra, new List<TrinityUnit>());
        private static readonly CacheField<List<TrinityItem>> _gold = new CacheField<List<TrinityItem>>(UpdateSpeed.Ultra, new List<TrinityItem>());
        private static readonly CacheField<List<TrinityGizmo>> _containers = new CacheField<List<TrinityGizmo>>(UpdateSpeed.Ultra, new List<TrinityGizmo>());
        private static readonly CacheField<List<TrinityGizmo>> _destructibles = new CacheField<List<TrinityGizmo>>(UpdateSpeed.Ultra, new List<TrinityGizmo>());
        private static readonly CacheField<List<TrinityGizmo>> _shrines = new CacheField<List<TrinityGizmo>>(UpdateSpeed.Ultra, new List<TrinityGizmo>());
        private static readonly CacheField<List<TrinityGizmo>> _doors = new CacheField<List<TrinityGizmo>>(UpdateSpeed.Ultra, new List<TrinityGizmo>());
        private static readonly CacheField<List<TrinityPlayer>> _Players = new CacheField<List<TrinityPlayer>>(UpdateSpeed.Ultra, new List<TrinityPlayer>());
        private static readonly CacheField<List<TrinityObject>> _objects = new CacheField<List<TrinityObject>>(UpdateSpeed.Ultra, new List<TrinityObject>());
        private static readonly CacheField<List<TrinityObject>> _hirelings = new CacheField<List<TrinityObject>>(UpdateSpeed.Ultra, new List<TrinityObject>());
        private static readonly CacheField<List<TrinityObject>> _navigationObstacles = new CacheField<List<TrinityObject>>(UpdateSpeed.Ultra, new List<TrinityObject>());
        private static readonly CacheField<List<TrinityUnit>> _eliteRareUniqueBoss = new CacheField<List<TrinityUnit>>(UpdateSpeed.Ultra, new List<TrinityUnit>());
        private static readonly CacheField<List<TrinityUnit>> _trash = new CacheField<List<TrinityUnit>>(UpdateSpeed.Ultra, new List<TrinityUnit>());
        private static readonly CacheField<List<TrinityUnit>> _goblins = new CacheField<List<TrinityUnit>>(UpdateSpeed.Ultra, new List<TrinityUnit>());
        private static readonly CacheField<List<TrinityAvoidance>> _avoidances = new CacheField<List<TrinityAvoidance>>(UpdateSpeed.Ultra, new List<TrinityAvoidance>());
        private static readonly CacheField<List<TrinityObject>> _globes = new CacheField<List<TrinityObject>>(UpdateSpeed.Ultra, new List<TrinityObject>());
        private static readonly CacheField<List<TrinityGizmo>> _gizmos = new CacheField<List<TrinityGizmo>>(UpdateSpeed.Ultra, new List<TrinityGizmo>());
        private static readonly CacheField<List<TrinityItem>> _items = new CacheField<List<TrinityItem>>(UpdateSpeed.Ultra, new List<TrinityItem>());
        private static readonly CacheField<List<TrinityUnit>> _pets = new CacheField<List<TrinityUnit>>(UpdateSpeed.Ultra, new List<TrinityUnit>());
        private static readonly CacheField<List<TrinityUnit>> _summoners = new CacheField<List<TrinityUnit>>(UpdateSpeed.Ultra, new List<TrinityUnit>());

        public static readonly ConcurrentDictionary<int, TrinityObject> CachedObjects = new ConcurrentDictionary<int, TrinityObject>();

        public static readonly ConcurrentDictionary<int, TrinityScene> CachedScenes = new ConcurrentDictionary<int, TrinityScene>();

        internal static int ForceRefreshLevel;
        public static DateTime LastUpdated = DateTime.MinValue;
        public static int ActivePlayerGuid = -1;
        private static Dictionary<int, TrinityObject> _actorsByRActorGuid = new Dictionary<int, TrinityObject>();
        public static int WorldDynamicId;

        #endregion

        #region Properties

        public static List<TrinityUnit> Units
        {
            get
            {
                if (_units.IsCacheValid) return _units.CachedValue;
                return _units.CachedValue = GetActorsOfType<TrinityUnit>().ToList();
            }
        }

        public static List<TrinityUnit> Monsters
        {
            get
            {
                if (_monsters.IsCacheValid) return _monsters.CachedValue;
                return _monsters.CachedValue = Units.Where(i => i.ActorMeta.IsMonster && i.IsHostile && !i.IsDead).ToList();
            }
        }

        public static List<TrinityItem> Gold
        {
            get
            {
                if (_gold.IsCacheValid) return _gold.CachedValue;
                return _gold.CachedValue = GetActorsOfType<TrinityItem>().Where(i => i.TrinityType == TrinityObjectType.Gold).ToList();
            }
        }

        public static List<TrinityGizmo> Containers
        {
            get
            {
                if (_containers.IsCacheValid) return _containers.CachedValue;
                return _containers.CachedValue = GetActorsOfType<TrinityGizmo>().Where(i => i.TrinityType == TrinityObjectType.Container).ToList();
            }
        }

        public static List<TrinityGizmo> Destructibles
        {
            get
            {
                if (_destructibles.IsCacheValid) return _destructibles.CachedValue;
                return _destructibles.CachedValue = GetActorsOfType<TrinityGizmo>().Where(i => i.TrinityType == TrinityObjectType.Destructible).ToList();
            }
        }

        public static List<TrinityGizmo> Shrines
        {
            get
            {
                if (_shrines.IsCacheValid) return _shrines.CachedValue;
                return _shrines.CachedValue = GetActorsOfType<TrinityGizmo>().Where(i => i.IsShrine).ToList();
            }
        }

        public static List<TrinityGizmo> Doors
        {
            get
            {
                if (_doors.IsCacheValid) return _doors.CachedValue;
                return _doors.CachedValue = GetActorsOfType<TrinityGizmo>().Where(i => i.TrinityType == TrinityObjectType.Door).ToList();
            }
        }

        public static List<TrinityPlayer> Players
        {
            get
            {
                if (_Players.IsCacheValid) return _Players.CachedValue;
                return _Players.CachedValue = GetActorsOfType<TrinityPlayer>().ToList();
            }
        }

        public static List<TrinityObject> Objects
        {
            get
            {
                if (_objects.IsCacheValid) return _objects.CachedValue;
                return _objects.CachedValue = GetActorsOfType<TrinityObject>();
            }
        }

        public static List<TrinityObject> Hirelings
        {
            get
            {
                if (_hirelings.IsCacheValid) return _hirelings.CachedValue;
                return _hirelings.CachedValue = GetActorsOfType<TrinityObject>().Where(o => o.ActorMeta.HirelingType != HirelingType.None).ToList();
            }
        }

        public static List<TrinityObject> NavigationObstacles
        {
            get
            {
                if (_navigationObstacles.IsCacheValid) return _navigationObstacles.CachedValue;
                return _navigationObstacles.CachedValue = GetActorsOfType<TrinityObject>().Where(i => i.IsNavigationObstacle).ToList();
            }
        }

        public static List<TrinityUnit> EliteRareUniqueBoss
        {
            get
            {
                if (_eliteRareUniqueBoss.IsCacheValid) return _eliteRareUniqueBoss.CachedValue;
                return _eliteRareUniqueBoss.CachedValue = GetActorsOfType<TrinityUnit>().Where(i => i.IsBossOrEliteRareUnique).ToList();
            }
        }

        public static List<TrinityUnit> Trash
        {
            get
            {
                if (_trash.IsCacheValid) return _trash.CachedValue;
                return _trash.CachedValue = GetActorsOfType<TrinityUnit>().Where(i => i.IsTrash).ToList();
            }
        }

        public static List<TrinityUnit> Goblins
        {
            get
            {
                if (_goblins.IsCacheValid) return _goblins.CachedValue;
                return _goblins.CachedValue = GetActorsOfType<TrinityUnit>().Where(i => i.IsGoblin).ToList();
            }
        }

        public static List<TrinityAvoidance> Avoidances
        {
            get
            {
                if (_avoidances.IsCacheValid) return _avoidances.CachedValue;
                return _avoidances.CachedValue = GetActorsOfType<TrinityAvoidance>().Where(i => i.TrinityType == TrinityObjectType.Avoidance).ToList();
            }
        }

        public static List<TrinityObject> Globes
        {
            get
            {
                if (_globes.IsCacheValid) return _globes.CachedValue;
                return _globes.CachedValue = GetActorsOfType<TrinityObject>().Where(i => i.IsGlobe).ToList();
            }
        }

        public static List<TrinityGizmo> Gizmos
        {
            get
            {
                if (_gizmos.IsCacheValid) return _gizmos.CachedValue;
                return _gizmos.CachedValue = GetActorsOfType<TrinityGizmo>().Where(i => i.IsGizmo).ToList();
            }
        }

        public static List<TrinityItem> Items
        {
            get
            {
                if (_items.IsCacheValid) return _items.CachedValue;
                return _items.CachedValue = GetActorsOfType<TrinityItem>().Where(i => i.IsItem).ToList();
            }
        }

        public static List<TrinityUnit> Pets
        {
            get
            {
                if (_pets.IsCacheValid) return _pets.CachedValue;
                return _pets.CachedValue = GetActorsOfType<TrinityUnit>().Where(i => i.IsSummonedByPlayer).ToList();
            }
        }

        public static List<TrinityUnit> Summoners
        {
            get
            {
                if (_summoners.IsCacheValid) return _summoners.CachedValue;
                return _summoners.CachedValue = GetActorsOfType<TrinityUnit>().Where(i => i.ActorMeta.IsSummoner).ToList();
            }
        }

        /// <summary>
        /// Replacement for ZetaDia.Me
        /// </summary>
        public static TrinityPlayer Me { get; private set; }

        /// <summary>
        /// If CacheManager is turned on
        /// </summary>
        public static bool IsRunning { get; internal set; }

        /// <summary>
        /// Flag that all CacheField object check, while True, they will return a cached value
        /// </summary>
        public static bool IsUpdatePending { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Update the references between CachedObjects and ACDs
        /// </summary>
        public static void Update()
        {
            IsUpdatePending = true;
            var refreshTimer = Stopwatch.StartNew();

            using (new PerformanceLogger("lazyCache.Update.PreUpdate"))
            {               
                if (ZetaDia.Me == null)
                    return;

                LastUpdated = DateTime.UtcNow;

                if (Me == null)
                    Me = new TrinityPlayer(ZetaDia.Me);
                else
                    Me.UpdateSource(ZetaDia.Me);
            }

            using (new PerformanceLogger("lazyCache.Update.Refresh"))
            {
                foreach (var rActor in ZetaDia.Actors.RActorList.OfType<DiaObject>())
                {
                    ACD acd;
                    int rActorGuid;
                    int acdGuid;
                    ActorType actorType;
                    int actorSNO;
                    using (new PerformanceLogger("lazyCache.Update.Refresh.PreFilter"))
                    {
                        if (rActor == null)
                            continue;

                        acd = rActor.CommonData;
                        if (acd == null || !acd.IsValid)
                            continue;

                        actorSNO = acd.ActorSNO;
                        if (DataDictionary.ExcludedActorIds.Contains(actorSNO))
                            continue;

                        actorType = acd.ActorType;
                        if (DataDictionary.ExcludedActorTypes.Contains(actorType))
                            continue;
                       
                        acdGuid = acd.ACDGuid;
                        rActorGuid = rActor.RActorGuid;
                      
                        if (!CacheBase.IsProperValid(rActor, acd, actorType, acdGuid, rActorGuid, actorSNO))
                            continue;                      
                    }

                    using (new PerformanceLogger("lazyCache.Update.Refresh.AddOrUpdate"))
                    {
                        CachedObjects.AddOrUpdate(acdGuid,
                            i => CacheFactory.CreateTrinityObject(rActor, acd, acdGuid, rActorGuid, actorSNO, actorType),
                            (key, actor) => actor.UpdateSource(actor, acd, rActor));
                    }
                }
            }

            IsUpdatePending = false;

            ActivePlayerGuid = Me.ACDGuid;
            WorldDynamicId = Me.WorldDynamicId;

            using (new PerformanceLogger("lazyCache.Update.Purge"))
            {               
                foreach (var o in CachedObjects.Where(o => !o.Value.IsValid)) 
                {
                    CachedObjects.TryRemove(o.Key, o.Value);
                }
            }

            using (new PerformanceLogger("lazyCache.Update.Buffs"))
            {
                CacheBuffs.Update();
            }

            using (new PerformanceLogger("lazyCache.Update.Movement"))
            {
                foreach (var o in Objects)
                {
                    if (o.IsUnit || o.IsProjectile || o.AvoidanceType == AvoidanceType.Arcane)
                    {
                        o.Movement.RecordMovement(o);
                    }
                }
            }

            using (new PerformanceLogger("lazyCache.Update.Scenes"))
            {
                foreach (var scene in ZetaDia.Scenes)
                {
                    var sceneGuid = scene.SceneGuid;
                    if (sceneGuid <= 0)
                        continue;

                    CachedScenes.AddOrUpdate(scene.SceneGuid, i => new TrinityScene(scene, sceneGuid), (i, trinityScene) => trinityScene.Update(scene));
                }
            }

            _actorsByRActorGuid = CachedObjects.Values.DistinctBy(i => i.RActorGuid).ToDictionary(k => k.RActorGuid, v => v);

            refreshTimer.Stop();
            LastUpdateTimeTaken = refreshTimer.Elapsed.TotalMilliseconds;

            var weightTimer = Stopwatch.StartNew();

            using (new PerformanceLogger("lazyCache.Update.Weighting"))
            {
                try
                {
                    CachedObjects.ForEach(cacheObject => cacheObject.Value.TryCalculateWeight());
                }
                catch (Exception)
                {
                    Logger.Log("Exception in Parallel.ForEach Enumeration");
                }
            }

            LastUpdateTimeTaken = refreshTimer.Elapsed.TotalMilliseconds;
            LastWeightingTimeTaken = weightTimer.Elapsed.TotalMilliseconds;
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
        /// Using (CacheManager.ForceRefresh())
        /// </summary>
        public static IDisposable ForceRefresh()
        {
            return new ForceRefreshHelper();
        }

        /// <summary>
        /// IDisposable for ForceRefresh
        /// </summary>
        public class ForceRefreshHelper : IDisposable
        {
            public ForceRefreshHelper()
            {
                ++ForceRefreshLevel;
            }

            public void Dispose()
            {
                --ForceRefreshLevel;
                GC.SuppressFinalize(this);
            }
        }

        #endregion


        public static double LastUpdateTimeTaken { get; set; }
        public static double LastWeightingTimeTaken { get; set; }


        public static List<AABB> WalkableNavCells { get; set; }

        public static List<AABB> Zones { get; set; }
    }
}