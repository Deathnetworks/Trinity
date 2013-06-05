using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Zeta.Common;

namespace Trinity.Cache
{
    internal static class CacheUtils
    {
        internal static bool IsBossSNO(int actorSNO)
        {
            return DataDictionary.BossIds.Contains(actorSNO);
        }

        internal static bool IsAvoidanceSNO(int actorSNO)
        {
            return DataDictionary.Avoidances.Contains(actorSNO) || DataDictionary.AvoidanceBuffs.Contains(actorSNO) || DataDictionary.AvoidanceProjectiles.Contains(actorSNO);
        }

        internal static float GetZDiff(Vector3 Position)
        {
            if (Position != Vector3.Zero)
                return Math.Abs(Trinity.Player.CurrentPosition.Z - Position.Z);
            else
                return 0f;
        }
    }
}
