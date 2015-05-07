using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trinity.LazyCache;

namespace Trinity.Combat.Weighting
{
    public class HotSpotWeighting : WeightingBase
    {
        /// <summary>
        /// Weighting for Hotspots
        /// </summary>
        public static IEnumerable<Weight> GetWeight(TrinityObject cacheObject)
        {
            var weightFactors = new List<Weight>();

            //// If there's monsters in our face, ignore
            //if (CombatContext.PrioritizeCloseRangeUnits)
            //{
            //    weightFactors.Add(new Weight(0, WeightMethod.Set, WeightReason.MonstersNearPlayer));
            //    return weightFactors;
            //}

            //// if we started cache refresh with a target already
            //if (Trinity.LastTargetRactorGUID != -1)
            //{
            //    weightFactors.Add(new Weight(0, WeightMethod.Set, WeightReason.HasTarget));
            //    return weightFactors;
            //}

            //// If it's very close, ignore
            //if (cacheObject.Distance <= V.F("Cache.HotSpot.MinDistance"))
            //{
            //    weightFactors.Add(new Weight(0, WeightMethod.Set, WeightReason.AlreadyCloseEnough));
            //    return weightFactors;
            //}

            //// Avoidance near position
            //if (!CacheData.TimeBoundAvoidance.Any(aoe => aoe.Position.Distance2D(cacheObject.Position) <= aoe.Radius))
            //{
            //    float maxDist = V.F("Cache.HotSpot.MaxDistance");
            //    weightFactors.Add(new Weight((maxDist - cacheObject.Distance) / maxDist * 50000d, WeightMethod.Set, WeightReason.AvoidanceAtPosition));
            //}

            return weightFactors;
        }
    }
}
