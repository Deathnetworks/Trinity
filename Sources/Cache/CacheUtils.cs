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
            return Trinity.hashBossSNO.Contains(actorSNO);
        }

        internal static bool IsAvoidanceSNO(int actorSNO)
        {
            return Trinity.hashAvoidanceSNOList.Contains(actorSNO) || Trinity.hashAvoidanceBuffSNOList.Contains(actorSNO) || Trinity.hashAvoidanceSNOProjectiles.Contains(actorSNO);
        }

        internal static float GetZDiff(Vector3 Position)
        {
            if (Position != Vector3.Zero)
                return Math.Abs(Trinity.PlayerStatus.CurrentPosition.Z - Position.Z);
            else
                return 0f;
        }
    }
}
