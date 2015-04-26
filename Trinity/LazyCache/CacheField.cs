using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Zeta.Game.Internals.Actors;
using Logger = Trinity.Technicals.Logger;

namespace Trinity.LazyCache
{
    /// <summary>
    /// Information about a field and the tools to refresh it.
    /// </summary>
    public class CacheField
    {
        /// <summary>
        /// Cached value
        /// </summary>
        public object CachedValue { get; set; }

        /// <summary>
        /// The amount of time (in Milliseconds) allowed to pass before refreshing
        /// </summary>
        public int Delay { get; set; }

        /// <summary>
        /// Time of the last refresh
        /// </summary>
        public DateTime LastUpdate { get; set; }

        /// <summary>
        /// Determines if the initial value has been set
        /// </summary>
        public bool IsValueCreated { get; set; }

        /// <summary>
        /// name of a property
        /// </summary>
        public string PropertyName { get; set; }
    }
}
