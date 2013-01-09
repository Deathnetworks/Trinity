using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GilesTrinity
{
    internal class GenericCache
    {
        private static HashSet<GenericCacheObject> CacheList = new HashSet<GenericCacheObject>();

        private static readonly object _Synchronizer = new object();

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

        public static bool ContainsKey(string key)
        {
            lock (_Synchronizer)
            {
                return CacheList.Any(o => o.Key == key);
            }
        }

        public static GenericCacheObject GetObject(string key)
        {
            lock (_Synchronizer)
            {
                if (ContainsKey(key))
                    return CacheList.FirstOrDefault(o => o.Key == key);
                else
                    return new GenericCacheObject();
            }
        }

        public static void MaintainCache()
        {
            lock (_Synchronizer)
            {
                foreach (GenericCacheObject obj in CacheList)
                {
                    if (obj.IsExpired())
                        CacheList.Remove(obj);
                }
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

        public bool IsExpired()
        {
            return DateTime.Now.Subtract(Expires).TotalMilliseconds > 0;
        }
    }
}
