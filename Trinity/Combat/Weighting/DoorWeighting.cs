using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trinity.LazyCache;

namespace Trinity.Combat.Weighting
{
    public class DoorWeighting : WeightingBase
    {
        /// <summary>
        /// Weighting for Doors
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

            if (weightFactors.TryAddWeight(cacheObject, WeightReason.MonsterInLoS))
                return weightFactors;

            weightFactors.TryAddWeight(cacheObject, WeightReason.UnitsBehind, 15000);
            weightFactors.TryAddWeight(cacheObject, WeightReason.PreviousTarget);
            weightFactors.TryAddWeight(cacheObject, WeightReason.CloseProximity, WeightManager.MaxWeight);

            return weightFactors;
        }
    }
}
