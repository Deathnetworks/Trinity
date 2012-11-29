using System.Collections.Generic;
using Zeta.CommonBot;
using Zeta.Internals.Actors;

namespace GilesTrinity.DbProvider
{
    /// <summary>
    /// Loot Targeting Provider 
    /// </summary>
    /// <remarks>
    /// This class is injected to DemonBuddy. 
    /// Leave blank, process is bypassed by plugin
    /// </remarks>
    public class GilesLootTargetingProvider : ITargetingProvider
    {
        private static readonly List<DiaObject> listEmptyList = new List<DiaObject>();

        /// <summary>
        /// Gets list of items in range by weight.
        /// </summary>
        /// <returns>Blank list of target, GilesTrinity don't use this Db process.</returns>
        public List<DiaObject> GetObjectsByWeight()
        {
            return listEmptyList;
        }
    }
}
