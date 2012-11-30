using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace GilesTrinity.Technicals
{
    internal static class CacheManager
    {
        #region Delegates
        public delegate CacheObject CacheObjectGetterDelegate(int rActorGUID);
        public delegate void CacheObjectRefresherDelegate(int rActorGUID, CacheObject cacheObject);
        #endregion Delegates

        #region Fields
        private static CacheObjectGetterDelegate _CacheObjectGetter;
        private static readonly IDictionary<int, CacheObject> _Cache = new Dictionary<int, CacheObject>();
        private static CacheObjectRefresherDelegate _CacheObjectRefresher;
        private static readonly object _Synchronizer = new object();
        private static Thread _CacheCleaner;
        private static readonly IDictionary<GObjectType, uint> _CacheTimeout = new Dictionary<GObjectType, uint>();
        private static uint _MaxRefreshRate = 300;
        #endregion Fields

        #region Properties
        /// <summary>
        /// Gets or sets the delegate which is called for refreshing object.
        /// </summary>
        /// <value>The delegate which is called for refreshing object.</value>
        public static CacheObjectRefresherDelegate CacheObjectRefresher
        {
            get
            {
                return _CacheObjectRefresher;
            }
            set
            {
                if (_CacheObjectRefresher != value)
                {
                    _CacheObjectRefresher = value;
                }
            }
        }

        /// <summary>
        /// Gets or sets the delegate which is called for getting inexisting object.
        /// </summary>
        /// <value>The delegate which is called for getting inexisting object.</value>
        public static CacheObjectGetterDelegate CacheObjectGetter
        {
            get
            {
                return _CacheObjectGetter;
            }
            set
            {
                if (_CacheObjectGetter != value)
                {
                    _CacheObjectGetter = value;
                }
            }
        }

        /// <summary>
        /// Gets the cache timeout by <see cref="GObjectType"/>.
        /// </summary>
        /// <value>The cache timeout.</value>
        public static IDictionary<GObjectType, uint> CacheTimeout
        {
            get
            {
                return _CacheTimeout;
            }
        }

        /// <summary>
        /// Gets or sets the max refresh rate.
        /// </summary>
        /// <value>The max refresh rate.</value>
        public static uint MaxRefreshRate
        {
            get
            {
                return _MaxRefreshRate;
            }
            set
            {
                if (MaxRefreshRate != value)
                {
                    _MaxRefreshRate = value;
                    DbHelper.Log(TrinityLogLevel.Normal, LogCategory.CacheManagement, "Your refresh rate on cache have been set to {0}.", value);
                }
            }
        }
        #endregion Properties

        #region Methods
        /// <summary>
        /// Initializes <see cref="CacheManager"/> when Bot start.
        /// </summary>
        public static void Initialize()
        {
            using (new PerformanceLogger("CacheManager.Initialize"))
            {
                if (_CacheCleaner != null)
                {
                    throw new InvalidOperationException("CacheManager is already initialized.");
                }

                _CacheTimeout.Add(GObjectType.Avoidance, 60000);
                _CacheTimeout.Add(GObjectType.Backtrack, 60000);
                _CacheTimeout.Add(GObjectType.Barricade, 60000);
                _CacheTimeout.Add(GObjectType.Container, 60000);
                _CacheTimeout.Add(GObjectType.Destructible, 60000);
                _CacheTimeout.Add(GObjectType.Door, 60000);
                _CacheTimeout.Add(GObjectType.Globe, 60000);
                _CacheTimeout.Add(GObjectType.Gold, 60000);
                _CacheTimeout.Add(GObjectType.HealthWell, 60000);
                _CacheTimeout.Add(GObjectType.Interactable, 60000);
                _CacheTimeout.Add(GObjectType.Item, 60000);
                _CacheTimeout.Add(GObjectType.Shrine, 60000);
                _CacheTimeout.Add(GObjectType.Unit, 60000);
                _CacheTimeout.Add(GObjectType.Unknown, 60000);

                _CacheCleaner = new Thread(MaintainCache);
                _CacheCleaner.Priority = ThreadPriority.Lowest;
                _CacheCleaner.IsBackground = true;
                _CacheCleaner.Start();
            }
        }

        /// <summary>
        /// Destroys all objects cached when Bot stop.
        /// </summary>
        public static void Destroy()
        {
            using (new PerformanceLogger("CacheManager.Initialize"))
            {
                _CacheCleaner.Abort();
                lock (_Synchronizer)
                {
                    _Cache.Clear();
                }
            }
        }

        /// <summary>Gets an object from cache.</summary>
        /// <param name="rActorGuid">The RActorGUID.</param>
        /// <returns>Cached object corresponding to the <paramref name="rActorGuid"/></returns>
        /// <exception cref="System.InvalidOperationException">You must set CacheObjectGetter property before calling GetObject</exception>
        public static CacheObject GetObject(int rActorGuid)
        {
            using (new PerformanceLogger("CacheManager.GetObject"))
            {
                CacheObject cacheObject;
                lock (_Synchronizer)
                {
                    if (_Cache.ContainsKey(rActorGuid))
                    {
                        cacheObject = _Cache[rActorGuid];
                        if (CacheObjectRefresher == null)
                        {
                            DbHelper.Log(TrinityLogLevel.Error, LogCategory.CacheManagement, "You don't have define CacheObjectRefresher before colling GetObject");
                        }
                        else
                        {
                            if (DateTime.UtcNow.Subtract(cacheObject.LastAccessDate).TotalMilliseconds >= MaxRefreshRate)
                            {
                                CacheObjectRefresher.Invoke(rActorGuid, cacheObject);
                            }
                        }
                    }
                    else
                    {
                        if (_CacheObjectGetter == null)
                        {
                            throw new InvalidOperationException("You must set CacheObjectGetter property before calling GetObject");
                        }
                        cacheObject = _CacheObjectGetter.Invoke(rActorGuid);
                        if (cacheObject != null)
                        {
                            _Cache.Add(rActorGuid, cacheObject);
                        }
                    }
                }

                if (cacheObject != null)
                {
                    cacheObject.LastAccessDate = DateTime.UtcNow;
                }
                // Return clone of cached object (You can modify returned copy without impact on cache system) 
                return cacheObject.Clone();
            }
        }

        /// <summary>
        /// Thread method which maintains the proper cache data.
        /// </summary>
        private static void MaintainCache()
        {
            while (true)
            {
                try
                {
                    // Search obselete object in cache dictionary
                    IList<int> removableKey = new List<int>();
                    
                    // Find which RActorGuid can be deleted from cache and store it 
                    foreach (KeyValuePair<int, CacheObject> keyPair in _Cache)
                    {
                        if (DateTime.UtcNow.Subtract(keyPair.Value.LastAccessDate).TotalSeconds > _CacheTimeout[keyPair.Value.Type])
                        {
                            removableKey.Add(keyPair.Key);
                        }
                    }

                    // Delete from cache all stored RActorGuid
                    foreach (int rActorGuid in removableKey)
                    {
                        lock (_Synchronizer)
                        {
                            _Cache.Remove(rActorGuid);
                        }
                    }

                    Thread.Sleep(5000);
                }
                catch (ThreadAbortException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    DbHelper.Log(TrinityLogLevel.Error, LogCategory.CacheManagement, "MaintainCache occurs an error : {0}", ex);
                }
            }
        }
        #endregion Methods
    }
}
