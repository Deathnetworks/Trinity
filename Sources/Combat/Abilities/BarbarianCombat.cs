using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Trinity.Combat.Abilities
{
    class BarbarianCombat : CombatBase
    {
        public static TrinityPower GetPower(CombatContext ctx)
        {
            return CombatBase.GetDefaultPower(ctx);
        }
    }
}
