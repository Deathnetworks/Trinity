using System;
using System.Collections.Generic;
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
                return CacheList.Contains(new GenericCacheObject()
                {
                    Key = key
                });
            }
        }

        public static bool Contains(GenericCacheObject obj)
        {
            lock (_Synchronizer)
            {
                return CacheList.Contains(obj);
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
            if (Manager == null || (Manager != null && !Manager.IsAlive))
            {
                DbHelper.Log(TrinityLogLevel.Normal, LogCategory.UserInformation, "Starting up Generic Cache Manage thread");
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
            try
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
            catch (Exception ex)
            {
                DbHelper.Log(TrinityLogLevel.Normal, LogCategory.UserInformation, "Exception in Generic Cache Manager");
                DbHelper.Log(TrinityLogLevel.Normal, LogCategory.UserInformation, ex.ToString());
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

        public override bool Equals(object obj)
        {
            var other = obj as GenericCacheObject;
            if (other == null)
                return false;

            return this.Key == other.Key;
        }

        public override int GetHashCode()
        {
            return this.Key.GetHashCode();
        }
    }
}
