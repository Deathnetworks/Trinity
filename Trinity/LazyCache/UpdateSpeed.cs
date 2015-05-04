using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Trinity.LazyCache
{
    public enum UpdateSpeed
    {
        /// <summary>
        /// Default, Only Once
        /// </summary>
        Once = -1,

        /// <summary>
        /// Every time
        /// </summary>
        RealTime = 0,

        /// <summary>
        /// Every 50ms
        /// </summary>
        Ultra = 50,

        /// <summary>
        /// Every 200ms
        /// </summary>
        Fast = 200,

        /// <summary>
        /// Every 500ms
        /// </summary>
        Normal = 500,
        
        /// <summary>
        /// Every 2000ms
        /// </summary>
        Slow = 2000
    }
}
