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
        public delegate CacheObject CacheObjectGetterDelegate(DiaObject acdObject);
        public delegate void CacheObjectRefresherDelegate(DiaObject acdObject, CacheObject cacheObject);
        #endregion Delegates

        #region Fields
        private static CacheObjectGetterDelegate _CacheObjectGetter;
        private static IDictionary<int, CacheObject> _Cache = new Dictionary<int, CacheObject>();
        private static CacheObjectRefresherDelegate _CacheObjectRefresher;
        private static readonly object _Synchronizer = new object();
        private static Thread _CacheCleaner;
        private static readonly IDictionary<CacheType, uint> _CacheTimeout = new Dictionary<CacheType, uint>();
        private static int _MaxRefreshRate = 300;
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

        public static void DefineStaleFlag()
        {
            lock (_Synchronizer)
            {
                foreach (CacheObject obj in _Cache.Values)
                {
                    obj.Stale = true;
                }
            }
        }

        /// <summary>
        /// Gets or sets the max refresh rate.
        /// </summary>
        /// <value>The max refresh rate.</value>
        public static int MaxRefreshRate
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
                    DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.CacheManagement, "Your refresh rate on cache have been set to {0}.", value);
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
                if (_CacheCleaner != null)
                {
                    _CacheCleaner.Abort();
                    _CacheCleaner = null;
                }
                lock (_Synchronizer)
                {
                    _Cache.Clear();
                    _CacheTimeout.Clear();
                }
            }
        }

        /// <summary>
        /// Put object to cache or refresh existing object.
        /// </summary>
        /// <param name="diaObject">The dia object.</param>
        /// <exception cref="System.InvalidOperationException">You haven't defined CacheObjectRefresher before calling GetObject</exception>
        /// <exception cref="System.InvalidOperationException">You must set CacheObjectGetter property before calling GetObject</exception>
        public static void RefreshObject(DiaObject diaObject)
        {
            using (new PerformanceLogger("CacheManager.GetObject"))
            {
                CacheObject cacheObject;
                int acdGuid = diaObject.ACDGuid;
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
                                CacheObjectRefresher.Invoke(diaObject, cacheObject);
                            }
                        }
                    }
                    else
                    {
                        if (_CacheObjectGetter == null)
                        {
                            throw new InvalidOperationException("You must set CacheObjectGetter property before calling GetObject");
                        }
                        cacheObject = _CacheObjectGetter.Invoke(diaObject);
                        if (cacheObject != null)
                        {
                            _Cache.Add(acdGuid, cacheObject);
                        }
                    }

                    cacheObject.LastRefreshDate = DateTime.UtcNow;
                    cacheObject.Stale = false;
                }
            }
        }

        /// <summary>Gets all cached object corresponding to the type.</summary>
        /// <typeparam name="T">Type of cached object</typeparam>
        /// <param name="type">The type.</param>
        /// <returns><see cref="IEnumerable"/> corresponding to all objects of this type in cache.</returns>
        public static IEnumerable<T> GetAllObjectByType<T>(CacheType type)
            where T : CacheObject
        {
            lock (_Synchronizer)
            {
                foreach (CacheObject obj in _Cache.Values.Where(o => o.CacheType == type && !o.Stale).ToList())
                {
                    if (obj is T)
                    {
                        yield return (T)obj.Clone();
                    }
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

                    lock (_Synchronizer)
                    {
                        // Find and delete useless CacheObject
                        foreach (KeyValuePair<int, CacheObject> keyPair in _Cache.ToList())
                        {
                            if (DateTime.UtcNow.Subtract(keyPair.Value.LastAccessDate).TotalSeconds > _CacheTimeout[keyPair.Value.CacheType])
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
                    DbHelper.Log(TrinityLogLevel.Debug, LogCategory.CacheManagement, "An error occured in Cache Maintenance system: {0}", ex);
                }
            }
        }
        #endregion Methods
    }
}
