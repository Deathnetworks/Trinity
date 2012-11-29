using System.Collections.Generic;
using Zeta.CommonBot;
using Zeta.Internals.Actors;

namespace GilesTrinity.DbProvider
{
    /// <summary>
    /// Obstacle Targeting Provider 
    /// </summary>
    /// <remarks>
    /// This class is injected to DemonBuddy. 
    /// Leave blank, process is bypassed by plugin
    /// </remarks>
    public class GilesObstacleTargetingProvider : ITargetingProvider
    {
        private static readonly List<DiaObject> listEmptyList = new List<DiaObject>();

        /// <summary>
        /// Gets list of obstacle in range by weight.
        /// </summary>
        /// <returns>Blank list of target, GilesTrinity don't use this Db process.</returns>
        public List<DiaObject> GetObjectsByWeight()
        {
            return listEmptyList;
        }
    }
}
