using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trinity.Config.Combat;
using Trinity.DbProvider;
using Trinity.LazyCache;

namespace Trinity.Combat.Weighting
{
    public class ObstacleWeighting : WeightingBase
    {
        /// <summary>
        /// Weighting for Barricades
        /// </summary>
        public static IEnumerable<Weight> GetBarricadeWeight(TrinityObject cacheObject)
        {
            var weightFactors = new List<Weight>();

            // rrrix added this as a single "weight" source based on the DestructableRange.
            // Calculate the weight based on distance, where a distance = 1 is 5000, 90 = 0
            weightFactors.Add(new Weight((90f - cacheObject.RadiusDistance) / 90f * 5000f, WeightMethod.Set, WeightReason.StartingWeight));

            weightFactors.TryAddWeight(cacheObject, WeightReason.PreviousTarget);
            weightFactors.TryAddWeight(cacheObject, WeightReason.CloseProximity, 30000);

            return weightFactors;
        }

        /// <summary>
        /// Weighting for Destructibles
        /// </summary>
        public static IEnumerable<Weight> GetDestructibleWeight(TrinityObject cacheObject)
        {
            var weightFactors = new List<Weight>();

            if (DataDictionary.ForceDestructibles.Contains(cacheObject.ActorSNO))
            {
                weightFactors.Add(new Weight(100, WeightMethod.Add, WeightReason.SettingForceDestructibles));
                return weightFactors;
            }

            // Not Stuck, skip!
            if (Trinity.Settings.WorldObject.DestructibleOption == DestructibleIgnoreOption.OnlyIfStuck && cacheObject.RadiusDistance > 0 &&
                (DateTime.UtcNow.Subtract(PlayerMover.LastGeneratedStuckPosition).TotalSeconds > 3))
            {
                return weightFactors;
            }

            // rrrix added this as a single "weight" source based on the DestructableRange.
            // Calculate the weight based on distance, where a distance = 1 is 5000, 90 = 0
            weightFactors.Add(new Weight((90f - cacheObject.RadiusDistance) / 90f * 1000f, WeightMethod.Set, WeightReason.StartingWeight));

            weightFactors.TryAddWeight(cacheObject, WeightReason.PreviousTarget);
            weightFactors.TryAddWeight(cacheObject, WeightReason.CloseProximity, 1500);
            weightFactors.TryAddWeight(cacheObject, WeightReason.MonsterInLoS, 0.5, WeightMethod.Multiply);
            weightFactors.TryAddWeight(cacheObject, WeightReason.AvoidanceInLoS);

            // Are we prioritizing close-range stuff atm? If so limit it at a value 3k lower than monster close-range priority
            if (CombatContext.PrioritizeCloseRangeUnits)
                weightFactors.Add(new Weight((15f - cacheObject.Distance) / 15f * 19200d, WeightMethod.Set, WeightReason.CloseRangePriority));

            weightFactors.TryAddWeight(cacheObject, WeightReason.TouchProximity, 40000);

            // Fix for WhimsyShire Pinata
            if (DataDictionary.ResplendentChestIds.Contains(cacheObject.ActorSNO))
                weightFactors.Add(new Weight(100 + cacheObject.RadiusDistance, WeightMethod.Set, WeightReason.DestructableChest));

            return weightFactors;
        }
    }
}
