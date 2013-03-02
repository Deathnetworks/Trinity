using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using GilesTrinity.Technicals;

namespace GilesTrinity
{
    internal class GenericBlacklist
    {
        private static HashSet<GenericCacheObject> Blacklist = new HashSet<GenericCacheObject>();

        private static readonly object _Synchronizer = new object();

        private static Thread Manager;

        public static bool AddToBlacklist(GenericCacheObject obj)
        {
            lock (_Synchronizer)
            {
                if (!ContainsKey(obj.Key))
                {
                    Blacklist.Add(obj);
                    return true;
                }
                return false;
            }
        }

        public static bool UpdateObject(GenericCacheObject obj)
        {
            lock (_Synchronizer)
            {
                if (Contains(obj))
                {
                    Blacklist.RemoveWhere(o => o.Key == obj.Key);
                }
                Blacklist.Add(obj);
                return true;
            }
        }

        public static bool Contains(GenericCacheObject obj)
        {
            lock (_Synchronizer)
            {
                return Blacklist.Contains(obj);
            }
        }

        public static bool ContainsKey(string key)
        {
            lock (_Synchronizer)
            {
                return Blacklist.Contains(new GenericCacheObject()
                {
                    Key = key
                });
            }
        }

        public static GenericCacheObject GetObject(string key)
        {
            lock (_Synchronizer)
            {
                if (ContainsKey(key))
                    return Blacklist.AsParallel().FirstOrDefault(o => o.Key == key);
                else
                    return new GenericCacheObject();
            }
        }

        public static void MaintainBlacklist()
        {
            try
            {
                if (Manager == null || (Manager != null && !Manager.IsAlive))
                {
                    DbHelper.Log(TrinityLogLevel.Normal, LogCategory.UserInformation, "Starting up Generic Blacklist Manager thread");
                    Manager = new Thread(Manage)
                    {
                        IsBackground = true,
                        Priority = ThreadPriority.Lowest
                    };
                    Manager.Start();
                }
            }
            catch (Exception ex)
            {
                DbHelper.Log(TrinityLogLevel.Normal, LogCategory.UserInformation, "Exception in Generic Blacklist Manager");
                DbHelper.Log(TrinityLogLevel.Normal, LogCategory.UserInformation, ex.ToString());
            }
        }

        private static void Manage()
        {
            while (true)
            {
                long NowTicks = DateTime.Now.Ticks;

                lock (_Synchronizer)
                {
                    Blacklist.RemoveWhere(o => o.Expires.Ticks < NowTicks);
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
                Blacklist.Clear();
            }
        }
    }

}
