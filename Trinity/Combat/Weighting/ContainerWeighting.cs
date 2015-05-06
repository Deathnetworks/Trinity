using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trinity.LazyCache;
using Trinity.Reference;
using Zeta.Game;

namespace Trinity.Combat.Weighting
{
    public class ContainerWeighting : WeightingBase
    {
        /// <summary>
        /// Weighting for Containers
        /// </summary>
        public static IEnumerable<Weight> GetWeight(TrinityObject cacheObject)
        {
            var weightFactors = new List<Weight>();

            if (weightFactors.TryAddWeight(cacheObject, WeightReason.CloseRangePriority))
                return weightFactors;

            if (weightFactors.TryAddWeight(cacheObject, WeightReason.NavBlocking))
                return weightFactors;

            // Dont open any containers while damage buff is active
            if (Legendary.HarringtonWaistguard.IsBuffActive)
            {
                weightFactors.Add(new Weight(0, WeightMethod.Set, WeightReason.HarringtonBuff));
                return weightFactors;
            }

            // Weight Containers
            float maxOpenRange = cacheObject.InternalName.ToLower().Contains("chest_rare") ? 250 : Trinity.Settings.WorldObject.ContainerOpenRange;
            weightFactors.Add(new Weight((maxOpenRange - cacheObject.Distance) / maxOpenRange * 100d, WeightMethod.Set, WeightReason.StartingWeight));

            weightFactors.TryAddWeight(cacheObject, WeightReason.CloseProximity, 600);

            // Open container for the damage buff
            if (Legendary.HarringtonWaistguard.IsEquipped && !Legendary.HarringtonWaistguard.IsBuffActive &&
                ZetaDia.Me.IsInCombat && cacheObject.Distance < 80f || CombatContext.ShouldPrioritizeContainers)
                weightFactors.Add(new Weight(20000, WeightMethod.Add, WeightReason.StartingWeight));

            weightFactors.TryAddWeight(cacheObject, WeightReason.PreviousTarget);
            weightFactors.TryAddWeight(cacheObject, WeightReason.MonsterInLoS, 0.5, WeightMethod.Multiply);
            weightFactors.TryAddWeight(cacheObject, WeightReason.AvoidanceInLoS);

            return weightFactors;
        }
    }
}
