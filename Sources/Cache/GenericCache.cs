using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using GilesTrinity.Technicals;

namespace GilesTrinity
{
    internal class GenericCache
    {
        private static HashSet<GenericCacheObject> CacheList = new HashSet<GenericCacheObject>();

        private static readonly object _Synchronizer = new object();

        private static Thread Manager;

        public static bool AddToCache(GenericCacheObject obj)
        {
            lock (_Synchronizer)
            {
                if (!ContainsKey(obj.Key))
                {
                    CacheList.Add(obj);
                    return true;
                }
                return false;
            }
        }

        public static bool UpdateObject(GenericCacheObject obj)
        {
            lock (_Synchronizer)
            {
                if (ContainsKey(obj.Key))
                {
                    CacheList.RemoveWhere(o => o.Key == obj.Key);
                }
                CacheList.Add(obj);
                return true;
            }
        }

        public static bool ContainsKey(string key)
        {
            lock (_Synchronizer)
            {
                return CacheList.AsParallel().Any(o => o.Key == key);
            }
        }

        public static GenericCacheObject GetObject(string key)
        {
            lock (_Synchronizer)
            {
                if (ContainsKey(key))
                    return CacheList.AsParallel().FirstOrDefault(o => o.Key == key);
                else
                    return new GenericCacheObject();
            }
        }

        public static void MaintainCache()
        {
            if (Manager == null || (Manager != null && Manager.ThreadState != System.Threading.ThreadState.Running))
            {
                DbHelper.Log(TrinityLogLevel.Debug, LogCategory.CacheManagement, "Starting up Generic Cache Manage thread");
                Manager = new Thread(Manage)
                {
                    IsBackground = true,
                    Priority = ThreadPriority.Lowest
                };
                Manager.Start();
            }
        }

        private static void Manage()
        {
            while (true)
            {
                long NowTicks = DateTime.Now.Ticks;

                lock (_Synchronizer)
                {
                    CacheList.RemoveWhere(o => o.Expires.Ticks < NowTicks);
                }

                Thread.Sleep(100);
            }
        }

        public static void Shutdown()
        {
            if (Manager != null)
            {
                Manager.Abort();
            }
        }

        public static void ClearCache()
        {
            lock (_Synchronizer)
            {
                CacheList.Clear();
            }
        }
    }

    internal class GenericCacheObject
    {
        public string Key { get; set; }
        public object Value { get; set; }
        public DateTime Expires { get; set; }

        public GenericCacheObject() { }

        public GenericCacheObject(string key, object value, TimeSpan expirationDuration)
        {
            Key = key;
            Value = value;
            Expires = DateTime.Now.Add(expirationDuration);
        }
    }
}
