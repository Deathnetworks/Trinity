using System.Collections.Generic;
using Zeta.CommonBot;
using Zeta.Internals.Actors;

namespace GilesTrinity.DbProvider
{
    /// <summary>
    /// Combat Targeting Provider 
    /// </summary>
    /// <remarks>
    /// This class is injected to DemonBuddy
    /// </remarks>
    public class GilesCombatTargetingReplacer : ITargetingProvider
    {
        private static readonly List<DiaObject> listEmptyList = new List<DiaObject>();

        /// <summary>
        /// Gets list of target in range by weight.
        /// </summary>
        /// <returns>Blank list of target, GilesTrinity don't use this Db process.</returns>
        public List<DiaObject> GetObjectsByWeight()
        {
            if (!GilesTrinity.bDontMoveMeIAmDoingShit || GilesTrinity.FakeObject == null)
                return listEmptyList;

            return new List<DiaObject>() 
                        { 
                            GilesTrinity.FakeObject 
                        };
        }
    }
}
