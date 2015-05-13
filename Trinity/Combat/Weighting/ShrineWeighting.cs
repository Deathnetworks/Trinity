using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trinity.LazyCache;

namespace Trinity.Combat.Weighting
{
    public class ShrineWeighting : WeightingBase
    {
        /// <summary>
        /// Weighting for Shrines
        /// </summary>
        public static IEnumerable<Weight> GetWeight(TrinityObject cacheObject)
        {
            var weightFactors = new List<Weight>();
            var gizmo = cacheObject as TrinityGizmo;
            if (gizmo == null)
            {
                weightFactors.Add(new Weight(0, WeightMethod.Set, WeightReason.InvalidType));
                return weightFactors;
            }

            if (cacheObject.TrinityType == TrinityObjectType.CursedShrine)
            {
                weightFactors.Add(new Weight(5000, WeightMethod.Add, WeightReason.Event));
                return weightFactors;
            }

            if (weightFactors.TryAddWeight(cacheObject, WeightReason.DisableForQuest))
                return weightFactors;

            var maxRange = CacheManager.Me.IsInRift ? 300f : 75f;
            var maxWeight = CacheManager.Me.IsInRift ? WeightManager.MaxWeight * 0.75d : 100d;
            var priorityShrines = Trinity.Settings.WorldObject.HiPriorityShrines;
            var startingWeight = priorityShrines ? WeightManager.MaxWeight * 0.75 : Math.Max(((maxRange - cacheObject.RadiusDistance) / maxRange * 15000d), 100d);

            weightFactors.Add(new Weight(startingWeight, WeightMethod.Set, WeightReason.Start));

            // Very close shrines get a weight increase
            if (cacheObject.Distance <= 30f)
                weightFactors.Add(new Weight(10000, WeightMethod.Add, WeightReason.CloseProximity));

            // Disable safety checks for Rift Pylons
            if (!CacheManager.Me.IsInRift)
            {
                weightFactors.TryAddWeight(cacheObject, WeightReason.PreviousTarget);
                weightFactors.TryAddWeight(cacheObject, WeightReason.MonsterInLoS);
                weightFactors.TryAddWeight(cacheObject, WeightReason.AvoidanceInLoS);
                weightFactors.TryAddWeight(cacheObject, WeightReason.DangerClose);
            }

            return weightFactors;
        }
    }
}
