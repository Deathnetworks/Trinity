using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Trinity.LazyCache;

namespace Trinity.Combat.Weighting
{
    public class WeightingBase
    {
        protected static List<Weight> GetBaseWeight()
        {
            var weightFactors = new List<Weight>();

            return weightFactors;            
        }

        /// <summary>
        /// Weighting for Shrines
        /// </summary>
        public static IEnumerable<Weight> GetPreWeightChecks(TrinityObject cacheObject)
        {
            var weightFactors = new List<Weight>();

            if (cacheObject.IsOwnedByPlayer)
                weightFactors.Add(new Weight(0, WeightMethod.Set, WeightReason.OwnedByPlayer));

            return weightFactors;
        }

    }
}
