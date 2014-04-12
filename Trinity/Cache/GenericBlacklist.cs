using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Trinity.Technicals;

namespace Trinity
{
    internal class GenericBlacklist
    {
        private static HashSet<GenericCacheObject> Blacklist = new HashSet<GenericCacheObject>();

        private static Dictionary<string, GenericCacheObject> _dataCache = new Dictionary<string, GenericCacheObject>();
        private static Dictionary<DateTime, string> _expireCache = new Dictionary<DateTime, string>();

        private static readonly object _Synchronizer = new object();

        private static Thread Manager;

        public static bool AddToBlacklist(GenericCacheObject obj)
        {
            if (obj.Key == "")
                return false;

            lock (_Synchronizer)
            {
                if (!ContainsKey(obj.Key))
                {
                    _dataCache.Add(obj.Key, obj);
                    _expireCache.Add(obj.Expires, obj.Key);
                    return true;
                }
                return false;
            }
        }

        public static bool UpdateObject(GenericCacheObject obj)
        {
            if (obj.Key == "")
                return false;

            lock (_Synchronizer)
            {
                RemoveObject(obj.Key);

                _dataCache.Add(obj.Key, obj);
                _expireCache.Add(obj.Expires, obj.Key);

                return true;
            }
        }

        public static bool RemoveObject(string key)
        {
            if (key == "")
                return false;

            lock (_Synchronizer)
            {
                if (ContainsKey(key))
                {
                    GenericCacheObject oldObj = _dataCache[key];
                    _dataCache.Remove(key);
                    _expireCache.Remove(oldObj.Expires);
                    return true;
                }
                return false;
            }
        }
        
        public static bool ContainsKey(string key)
        {
            if (key == "")
                return false;

            lock (_Synchronizer)
            {
                return _dataCache.ContainsKey(key);
            }
        }

        public static bool Contains(GenericCacheObject obj)
        {
            if (obj.Key == "")
                return false;

            lock (_Synchronizer)
            {
                return ContainsKey(obj.Key);
            }
        }

        public static GenericCacheObject GetObject(string key)
        {
            lock (_Synchronizer)
            {
                if (ContainsKey(key))
                {
                    return _dataCache[key];
                }
                else
                    return new GenericCacheObject();
            }
        }

        public static void MaintainBlacklist()
        {
            using (new PerformanceLogger("GenericBlacklist.MaintainBlacklist"))
            {
                try
                {
                    if (Manager == null || (Manager != null && !Manager.IsAlive))
                    {
                        Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "Starting up Generic Blacklist Manager thread");
                        Manager = new Thread(Manage)
                        {
                            Name = "Trinity Generic Blacklist",
                            IsBackground = true,
                            Priority = ThreadPriority.Lowest
                        };
                        Manager.Start();
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "Exception in Generic Blacklist Manager");
                    Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, ex.ToString());
                }
            }
        }

        private static void Manage()
        {
            while (true)
            {
                long NowTicks = DateTime.UtcNow.Ticks;

                lock (_Synchronizer)
                {
                    foreach (KeyValuePair<DateTime, string> kv in _expireCache.Where(o => o.Key.Ticks < NowTicks).ToList())
                    {
                        if (kv.Key.Ticks < NowTicks)
                        {
                            _expireCache.Remove(kv.Key);
                            _dataCache.Remove(kv.Value);
                        }
                    }
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

        public static void ClearBlacklist()
        {
            lock (_Synchronizer)
            {
                _dataCache.Clear();
                _expireCache.Clear();
            }
        }
    }

}
