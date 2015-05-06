using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trinity.LazyCache;

namespace Trinity.Combat.Weighting
{
    public class HealthWellWeighting : WeightingBase
    {
        /// <summary>
        /// Weighting for Health Wells
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

            if (!CacheManager.EliteRareUniqueBoss.Any())
            {
                if (!Trinity.Settings.WorldObject.UseShrine)
                {
                    weightFactors.Add(new Weight(1, WeightMethod.Set, WeightReason.DisabledInSettings));
                    return weightFactors;
                }

                weightFactors.TryAddWeight(cacheObject, WeightReason.MonsterInLoS);
                weightFactors.TryAddWeight(cacheObject, WeightReason.AvoidanceInLoS);

                if (cacheObject.IsBlocking)
                {
                    weightFactors.Add(new Weight(0, WeightMethod.Set, WeightReason.NavBlocking));
                    return weightFactors;
                }
            }

            // As a percentage of health with typical maximum weight
            cacheObject.Weight = WeightManager.MaxWeight * (1 - CacheManager.Me.CurrentHealthPct);

            return weightFactors;
        }
    }
}
