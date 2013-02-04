using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GilesTrinity
{
    public class TargetUtil
    {
        public static bool AnyTrashMobsInRange(float range = 10f)
        {
            return (from o in GilesTrinity.GilesObjectCache
                    where o.Type == GObjectType.Unit &&
                    o.RadiusDistance <= range
                    select o).Any();
        }

        public static bool AnyTrashMobsInRange(float range = 10f, int minCount = 1)
        {
            return (from o in GilesTrinity.GilesObjectCache
                    where o.Type == GObjectType.Unit &&
                    o.RadiusDistance <= range
                    select o).Count() >= minCount; 
        }

        public static bool AnyElitesInRange(float range = 10f)
        {
            return (from o in GilesTrinity.GilesObjectCache
                    where o.Type == GObjectType.Unit &&
                    o.IsBossOrEliteRareUnique &&
                    o.RadiusDistance <= range
                    select o).Any();
        }

        public static bool AnyElitesInRange(float range = 10f, int minCount = 1)
        {
            return (from o in GilesTrinity.GilesObjectCache
                    where o.Type == GObjectType.Unit &&
                    o.IsBossOrEliteRareUnique &&
                    o.RadiusDistance <= range
                    select o).Count() > minCount;
        }

        public static bool IsEliteTargetInRange(float range = 10f)
        {
            return GilesTrinity.CurrentTarget != null && GilesTrinity.CurrentTarget.IsBossOrEliteRareUnique && GilesTrinity.CurrentTarget.RadiusDistance <= range;
        }

    }
}
