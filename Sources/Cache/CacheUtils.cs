using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GilesTrinity.Cache
{
    internal static class CacheUtils
    {
        internal static bool IsBossSNO(int actorSNO)
        {
            return GilesTrinity.hashBossSNO.Contains(actorSNO);
        }

        internal static bool IsAvoidanceSNO(int actorSNO)
        {
            return GilesTrinity.hashAvoidanceSNOList.Contains(actorSNO) || GilesTrinity.hashAvoidanceBuffSNOList.Contains(actorSNO) || GilesTrinity.hashAvoidanceSNOProjectiles.Contains(actorSNO);
        }
    }
}
