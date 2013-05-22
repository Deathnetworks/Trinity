using System.Collections.Generic;
using Zeta.CommonBot;
using Zeta.Internals.Actors;

namespace Trinity.DbProvider
{
    /// <summary>
    /// Combat Targeting Provider 
    /// </summary>
    /// <remarks>
    /// This class is injected to DemonBuddy
    /// </remarks>
    public class BlankCombatProvider : ITargetingProvider
    {
        private static readonly List<DiaObject> listEmptyList = new List<DiaObject>();

        /// <summary>
        /// Gets list of target in range by weight.
        /// </summary>
        /// <returns>Blank list of target, GilesTrinity don't use this Db process.</returns>
        public List<DiaObject> GetObjectsByWeight()
        {
            if (!Trinity.bDontMoveMeIAmDoingShit || Trinity.FakeObject == null)
                return listEmptyList;

            return new List<DiaObject>() 
                        { 
                            Trinity.FakeObject 
                        };
        }
    }
}
