using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Zeta.Game.Internals.Actors;
using Zeta.Game.Internals.SNO;
using Logger = Trinity.Technicals.Logger;

namespace Trinity.LazyCache
{
    /// <summary>
    /// An container object to be attached to a cache object
    /// </summary>
    public class CacheAttachment
    {
        /// <summary>
        /// A collection of cached fields indexed by MemberName
        /// </summary>
        public Dictionary<string, CacheField> UpdateFields = new Dictionary<string, CacheField>();
    }
}
