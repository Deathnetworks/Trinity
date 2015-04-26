using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Zeta.Game.Internals.Actors;
using Logger = Trinity.Technicals.Logger;

namespace Trinity.LazyCache
{
    /// <summary>
    /// Utility for creating cache objects
    /// </summary>
    public static class CacheFactory
    {
        /// <summary>
        /// Convert CacheObjectBase into a derived class
        /// </summary>
        /// <typeparam name="T">a class derived from CacheBase</typeparam>
        /// <param name="cacheBase">an instance of CacheBase</param>
        /// <returns>baseObject converted to an instance of T</returns>
        public static T CreateObject<T>(CacheBase cacheBase)
        {
            return (T)Activator.CreateInstance(typeof(T), cacheBase.Source);
        }
    }
}
