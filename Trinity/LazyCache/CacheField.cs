using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Windows.Media;
using Zeta.Game.Internals.Actors;
using Logger = Trinity.Technicals.Logger;

namespace Trinity.LazyCache
{
    /// <summary>
    /// Information about a field and the tools to refresh it.
    /// </summary>
    public class CacheField<T> 
    {
        private T _cachedValue;

        /// <summary>
        /// Create cache field; specifying an update interval
        /// </summary>
        public CacheField(UpdateSpeed speed)
        {
            Delay = (int)speed;
        }

        /// <summary>
        /// Create cache field; default is to always return cache value after the first refresh.
        /// </summary>
        public CacheField(int delay = -1)
        {
            Delay = delay;
        }

        /// <summary>
        /// Cached value
        /// </summary>
        public T CachedValue
        {
            get { return _cachedValue; }
            set
            {
                if (!IsValueCreated)
                     IsValueCreated = true;

                LastUpdate = CacheManager.LastUpdated;  
                _cachedValue = value;
            }
        }

        /// <summary>
        /// If cache value has ever been set;
        /// </summary>
        public bool IsValueCreated { get; set; }

        /// <summary>
        /// Indicates if a new value should be requested
        /// </summary>
        public bool IsCacheValid 
        {
            get
            {
                // Always use cache if SetValueOverride() was used.
                if (IsValueOverride)
                    return true;

                // Always update the cache if update forcing refresh is turned on
                if (CacheManager.ForceRefreshLevel > 0)
                    return false;

                // Always update the cache if we dont have a value yet
                if (!IsValueCreated)
                    return false;

                // Always use cache value if default -1 delay
                if (Delay == (int) UpdateSpeed.Once)
                    return true;

                // Always update if set to realtime
                if (Delay == (int) UpdateSpeed.RealTime)
                    return false;

                // Always use cache for requests on the same DB pulse
                if (CacheManager.LastUpdated.Ticks == LastUpdate.Ticks)
                    return true;
                
                // Use cache value if delay hasnt passed yet.
                if (CacheManager.LastUpdated.Subtract(LastUpdate).TotalMilliseconds >= Delay)
                    return false;

                // Use cache value
                return true;
            }
        }

        /// <summary>
        /// The amount of time (in Milliseconds) allowed to pass before refreshing
        /// </summary>
        public int Delay { get; set; }

        /// <summary>
        /// Time of the last refresh
        /// </summary>
        public DateTime LastUpdate { get; set; }

        /// <summary>
        /// If true, cache value will always be returned.
        /// </summary>
        public bool IsValueOverride { get; private set; }

        /// <summary>
        /// Set a permanent value, which is always returned.
        /// Intended for manually creating objects that are not linked to DB actors.
        /// </summary>
        internal void SetValueOverride(T value)
        {
            IsValueOverride = true;
            IsValueCreated = true;
            CachedValue = value;            
        }

        /// <summary>
        /// Reset everything except delay property.
        /// </summary>
        public void Clear()
        {
            IsValueOverride = false;
            IsValueCreated = false;
            CachedValue = default(T);
        }
    }

}
