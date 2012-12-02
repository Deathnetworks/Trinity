using GilesTrinity.Technicals;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Zeta.Internals.Actors;

namespace GilesTrinity.Cache
{
    internal static class CacheManager
    {
        #region Delegates
        public delegate CacheObject CacheObjectGetterDelegate(int acdGuid, ACD acdObject);
        public delegate void CacheObjectRefresherDelegate(int acdGuid, ACD acdObject, CacheObject cacheObject);
        #endregion Delegates

        #region Fields
        private static CacheObjectGetterDelegate _CacheObjectGetter;
        private static IDictionary<int, CacheObject> _Cache = new Dictionary<int, CacheObject>();
        private static CacheObjectRefresherDelegate _CacheObjectRefresher;
        private static readonly object _Synchronizer = new object();
        private static Thread _CacheCleaner;
        private static readonly IDictionary<CacheType, uint> _CacheTimeout = new Dictionary<CacheType, uint>();
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
        public static IDictionary<CacheType, uint> CacheTimeout
        {
            get
            {
                return _CacheTimeout;
            }
        }

        public static IDictionary<int, CacheObject> Cache
        {
            get
            {
                return _Cache;
            }
            set
            {
                if (_Cache != null)
                {
                    _Cache = value;
                }
            }
        }

        public static bool ContainsKey(int acdGuid)
        {
            using (new PerformanceLogger("CacheManager.PutObject"))
            {
                lock (_Synchronizer)
                {
                    return _Cache.ContainsKey(acdGuid);
                }
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

                _CacheTimeout.Add(CacheType.Avoidance, 60000);
                _CacheTimeout.Add(CacheType.Gizmo, 60000);
                _CacheTimeout.Add(CacheType.Item, 60000);
                _CacheTimeout.Add(CacheType.Object, 60000);
                _CacheTimeout.Add(CacheType.Other, 60000);
                _CacheTimeout.Add(CacheType.Unit, 60000);
                
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
            using (new PerformanceLogger("CacheManager.Destroy"))
            {
                _CacheCleaner.Abort();
                lock (_Synchronizer)
                {
                    _Cache.Clear();
                }
            }
        }

        /// <summary>Gets an object from cache.</summary>
        /// <param name="acdObject">The acd object.</param>
        /// <returns>
        /// Cached object corresponding to the <paramref name="acdObject" />
        /// </returns>
        public static CacheObject GetObject(ACD acdObject)
        {
            if (acdObject == null)
            {
                return null;
            }

            return GetObject(acdObject.ACDGuid, acdObject);
        }

        /// <summary>Gets an object from cache.</summary>
        /// <param name="acdObject">The acd object.</param>
        /// <returns>
        /// Cached object corresponding to the <paramref name="acdObject" />
        /// </returns>
        public static T GetObject<T>(ACD acdObject)
            where T : CacheObject
        {
            if (acdObject == null)
            {
                return null;
            }

            return (T)GetObject(acdObject.ACDGuid, acdObject);
        }

        /// <summary>Gets the object from cache.</summary>
        /// <param name="acdGuid">The ACDGuid.</param>
        /// <returns>Cached object corresponding to the <paramref name="acdGuid" /></returns>
        public static CacheObject GetObject(int acdGuid)
        {
            return GetObject(acdGuid, null);
        }

        private static CacheObject GetObject(int acdGuid, ACD acdObject)
        {
            using (new PerformanceLogger("CacheManager.GetObject"))
            {
                CacheObject cacheObject;
                lock (_Synchronizer)
                {
                    if (_Cache.ContainsKey(acdGuid))
                    {
                        cacheObject = _Cache[acdGuid];
                        if (CacheObjectRefresher == null)
                        {
                            DbHelper.Log(TrinityLogLevel.Error, LogCategory.CacheManagement, "You haven't defined CacheObjectRefresher before calling GetObject");
                        }
                        else
                        {
                            if (DateTime.UtcNow.Subtract(cacheObject.LastRefreshDate).TotalMilliseconds >= MaxRefreshRate)
                            {
                                CacheObjectRefresher.Invoke(acdGuid, acdObject, cacheObject);
                                cacheObject.LastRefreshDate = DateTime.UtcNow;
                            }
                        }
                    }
                    else
                    {
                        if (_CacheObjectGetter == null)
                        {
                            throw new InvalidOperationException("You must set CacheObjectGetter property before calling GetObject");
                        }
                        cacheObject = _CacheObjectGetter.Invoke(acdGuid, acdObject);
                        if (cacheObject != null)
                        {
                            _Cache.Add(acdGuid, cacheObject);
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

        /// <summary>Gets all cached object corresponding to the type.</summary>
        /// <typeparam name="T">Type of cached object</typeparam>
        /// <param name="type">The type.</param>
        /// <returns><see cref="IEnumerable"/> corresponding to all objects of this type in cache.</returns>
        public static IEnumerable<T> GetAllObjectByType<T>(CacheType type)
            where T : CacheObject
        {
            foreach (CacheObject obj in _Cache.Values.Where(o => o.CacheType == type).ToList())
            {
                if (obj is T)
                {
                    yield return (T)obj.Clone();
                }
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

                    // Find and delete useless CacheObject
                    foreach (KeyValuePair<int, CacheObject> keyPair in _Cache.ToList())
                    {
                        if (DateTime.UtcNow.Subtract(keyPair.Value.LastAccessDate).TotalSeconds > _CacheTimeout[keyPair.Value.CacheType])
                        {
                            lock (_Synchronizer)
                            {
                                _Cache.Remove(keyPair.Key);
                            }
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
