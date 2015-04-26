using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
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
    * - Every attribute except ACDGuid is configured with a cache duration.
    * - If the duration hasn't expired on a property, a stored value is returned instead.
    * - Properties don't do any work at all unless the property's getter is accessed.
    * - All objects with the same ACDGuid share cached values regardless of instance.
    */

    /// <summary>
    /// LazyCache creates a caching layer between DB's attribute lookups and Trinity.    
    /// This uses only ACD & ACDItem and does not use ActorManager, DiaObject or Derived types.      
    /// </summary>
    public static class CacheManager
    {
        #region Constants

        /// <summary>
        /// How long after not having seen an object before it is removed from the cache.
        /// </summary>
        private const int PurgeLimitSeconds = 30;

        #endregion

        #region Fields

        /// <summary>
        /// CacheObjects index by ActorType
        /// </summary>
        private static ILookup<ActorType, CacheBase> _cachedObjectsByActorType;

        /// <summary>
        /// CacheObjects indexed by ACDGuid
        /// </summary>
        private static Dictionary<int, CacheBase> _cachedObjectsByACDGuid;

        /// <summary>
        /// The Primary DataStore of CacheObjects
        /// </summary>
        public static readonly ConcurrentDictionary<int, CacheBase> CachedObjects = new ConcurrentDictionary<int, CacheBase>();

        /// <summary>
        /// Stores the cached values for every enabled property in every cached object
        /// </summary>
        private static readonly Dictionary<int, CacheAttachment> CachedData = new Dictionary<int, CacheAttachment>();

        /// <summary>
        /// If execution is within a using(CacheManager.ForceRefresh()) block
        /// </summary>
        private static int _forceRefreshLevel;

        #endregion

        #region Events

        /// <summary>
        /// Arguments for all property cache data related events
        /// </summary>
        public class PropertyEventArgs
        {
            public string PropertyName { get; set; }
            public object OldValue { get; set; }
            public object NewValue { get; set; }
            public Type Type { get; set; }
            public float TimeSincePreviousUpdate { get; set; }
            public int Delay { get; set; }
            public bool IsUpdated { get; set; }
        }

        /// <summary>
        /// Event Delegate for all property cache data related events
        /// </summary>
        public delegate void PropertyEvent(CacheBase sender, PropertyEventArgs args);

        /// <summary>
        /// Event that Fires only when a propertys value has changed
        /// </summary>
        public static event PropertyEvent PropertyChanged = (s, a) => { };

        /// <summary>
        /// Event that fires whenever a property value requested / getter is accessed
        /// </summary>
        public static event PropertyEvent PropertyChecked = (s, a) => { };

        /// <summary>
        /// Arguments for all cache management related events
        /// </summary>
        public class CacheManagementEventArgs
        {
            public int Added { get; set; }
            public int Removed { get; set; }
            public int Total { get; set; }
            public int Updated { get; set; }
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
        /// Update the links between CachedObjects and DiaObjects
        /// This does not access or update any property values.
        /// </summary>
        public static void Update()
        {
            var source = ZetaDia.Actors.ACDList.Cast<ACD>();
            var addedCount = 0;
            var updatedCount = 0;
            var removedCount = 0;
            var tps = BotMain.TicksPerSecond;

            using (new PerformanceLogger("LazyCache.Update"))
            {
                foreach (var actor in source)
                {
                    CachedObjects.AddOrUpdate(actor.ACDGuid, i =>
                    {
                        addedCount++;
                        return actor;

                    }, (key, existingActor) =>
                    {
                        updatedCount++;
                        existingActor.UpdateSource(actor);
                        return existingActor;
                    });
                }
            }

            using (new PerformanceLogger("LazyCache.Purge"))
            {
                CachedObjects.ForEach(o =>
                {
                    var isOld = (tps == 0) || o.Value.UpdateTicks / tps > PurgeLimitSeconds * tps;

                    if ((!UtilityExtensions.IsProperValid(o.Value) || isOld) && CachedObjects.TryRemove(o.Key, o.Value))
                    {
                        if (isOld)
                            CachedData.Remove(o.Key);

                        removedCount++;
                    }
                });
            }

            using (new PerformanceLogger("LazyCache.Lookups"))
            {
                _cachedObjectsByActorType = CachedObjects.ToLookup(k => k.Value.ActorType, v => v.Value);
                _cachedObjectsByACDGuid = CachedObjects.ToDictionary(k => k.Value.ACDGuid, v => v.Value);
            }

            using (new PerformanceLogger("LazyCache.Update.Event"))
            {
                CacheUpdated(new CacheManagementEventArgs
                {
                    Added = addedCount,
                    Removed = removedCount,
                    Updated = updatedCount,
                    Total = CachedObjects.Count
                });
            }
        }

        /// <summary>
        /// Get actors of a particular ActorType
        /// </summary>
        /// <typeparam name="T">the type of CacheObject you want to retreive</typeparam>
        /// <param name="actorType">restrict the search to a certain actorType</param>
        /// <returns>a collection of actors</returns>
        public static List<T> GetActorsOfType<T>(ActorType actorType = ActorType.Invalid) where T : CacheBase
        {
            if (!CachedObjects.Any() || _cachedObjectsByActorType == null)
                return new List<T>();

            if (actorType == ActorType.Invalid)
            {
                return CachedObjects.Values.Select(CacheFactory.CreateObject<T>).ToList();
            }

            var cachedOfType = _cachedObjectsByActorType[actorType].ToList();
            if (cachedOfType.Any())
            {
                return cachedOfType.Select(CacheFactory.CreateObject<T>).ToList();
            }

            return new List<T>();
        }


        /// <summary>
        /// Get a specific actor by ACDGuid
        /// </summary>
        /// <typeparam name="T">the type of CacheObject you want to retreive</typeparam>
        /// <param name="ACDGuid">the ACDGuid of the object you want to find</param>
        /// <returns>the requested actor or null</returns>
        public static T GetActorByACDGuid<T>(int ACDGuid) where T : CacheBase
        {
            if (!CachedObjects.Any() || ACDGuid < 0 || _cachedObjectsByACDGuid == null)
                return null;

            CacheBase obj;
            if (_cachedObjectsByACDGuid.TryGetValue(ACDGuid, out obj))
            {
                return CacheFactory.CreateObject<T>(obj);
            }

            return null;
        }

        /// <summary>
        /// Get a cached controlled value from source using request/converter function
        /// </summary>
        /// <typeparam name="TValue">the Type of the property to be cached</typeparam>
        /// <typeparam name="TParent"></typeparam>
        /// <param name="cacheObject">the instance of the calling class (use .this)</param>
        /// <param name="updateDelegate">function to get new value from source</param>
        /// <param name="refreshDelay">amount of time (in Milliseconds) required before field is refreshed (-1 = only once, 0 = every time)</param>
        /// <param name="propertyName">(Automatic/Optional) the property who's value should be retrieved</param>
        /// <returns>a cached value</returns>
        internal static TValue GetCacheValue<TValue,TParent>(TParent cacheObject, Func<TParent,TValue> updateDelegate, int refreshDelay = -1, [CallerMemberName] string propertyName = "") where TParent : CacheBase
        {
            using (new PerformanceLogger(string.Format("LazyCache.GetCacheValue. {0}", propertyName)))
            {
                if (cacheObject == null)
                    throw new ArgumentNullException("cacheObject");

                if (updateDelegate == null)
                    throw new ArgumentNullException("updateDelegate");
               
                var timeSinceRefreshMs = 0f;
                var updated = false;
                var newValue = default(TValue);

                try
                {
                    var attachedData = CachedData.GetOrCreateValue(cacheObject.ACDGuid);

                    var cacheField = attachedData.UpdateFields.GetOrCreateValue(propertyName, new CacheField
                    {
                        Delay = refreshDelay,
                        LastUpdate = DateTime.MinValue,
                        PropertyName = propertyName
                    });

                    timeSinceRefreshMs = (float)DateTime.UtcNow.Subtract(cacheField.LastUpdate).TotalMilliseconds;

                    var oldValue = cacheField.CachedValue != null ? (TValue)cacheField.CachedValue : default(TValue);

                    cacheField.Delay = refreshDelay;

                    var shouldRefreshValue = cacheObject.IsValid && (!cacheField.IsValueCreated || _forceRefreshLevel > 0 || (cacheField.Delay >= 0 && timeSinceRefreshMs >= cacheField.Delay));

                    if (shouldRefreshValue)
                    {
                        updated = true;
                        cacheField.IsValueCreated = true;
                        cacheField.LastUpdate = DateTime.UtcNow;
                        cacheField.CachedValue = updateDelegate(cacheObject);
                    }

                    newValue = cacheField.CachedValue != null ? (TValue)cacheField.CachedValue : default(TValue);

                    var args = new PropertyEventArgs
                    {
                        Type = typeof(TValue),
                        NewValue = newValue,
                        OldValue = oldValue,
                        PropertyName = propertyName,
                        TimeSincePreviousUpdate = timeSinceRefreshMs,
                        Delay = cacheField.Delay,
                        IsUpdated = updated
                    };

                    if (!Equals(newValue, oldValue))
                    {
                        PropertyChanged(cacheObject, args);
                    }

                    PropertyChecked(cacheObject, args);
                }
                catch (Exception ex)
                {
                    if (ex.Message.StartsWith("Only part of a ReadProcessMemory"))
                    {
                        Logger.LogError("DB Memory Exception Property={0} Age={1} Exception={2}{3}",
                            propertyName, timeSinceRefreshMs, ex.Message, ex.InnerException);
                    }
                    else throw;
                }

                return newValue;
            }
        }

        /// <summary>
        /// using (CacheManager.ForceRefresh())
        /// {
        ///     // properties will be refreshed each access
        /// }
        /// </summary>
        public static IDisposable ForceRefresh()
        {
            return new ForceRefreshHelper();
        }

        #endregion

        #region ForceRefreshHelper

        private class ForceRefreshHelper : IDisposable
        {
            public ForceRefreshHelper()
            {
                ++_forceRefreshLevel;
            }

            public void Dispose()
            {
                --_forceRefreshLevel;
                GC.SuppressFinalize(this);
            }
        }

        #endregion

    }

}
