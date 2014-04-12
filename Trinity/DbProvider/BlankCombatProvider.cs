using System.Collections.Generic;
using Zeta.Bot;
using Zeta.Game.Internals.Actors;
namespace Trinity.DbProvider
{
    /// <summary>Combat Targeting Provider </summary>
    public class BlankCombatProvider : ITargetingProvider
    {
        private static readonly List<DiaObject> targetList = new List<DiaObject>();

        /// <summary> Gets list of target in range by weight.</summary>
        /// <returns>Blank list of target, Trinity don't use this Db process.</returns>
        public List<DiaObject> GetObjectsByWeight()
        {
            var list = new List<DiaObject>();
            if (Trinity.CurrentTarget != null && Trinity.CurrentTarget.Object != null && Trinity.CurrentTarget.Object.IsValid)
                list.Add(Trinity.CurrentTarget.Object);
            return list;
        }

    }
}
