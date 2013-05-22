using Trinity.DbProvider;
using Trinity.Technicals;
using System;
using System.Linq;
using Zeta.Common;
using Zeta.Common.Plugins;
using Zeta.CommonBot;
using Zeta.Internals.Actors;
using System.Collections.Generic;

namespace Trinity
{
    public class CombatAbilities
    {
        public static HashSet<SNOPower> Hotbar
        {
            get
            {
                return Trinity.Hotbar;
            }
        }
        public static PlayerInfoCache PlayerStatus
        {
            get
            {
                return Trinity.PlayerStatus;
            }
        }
    }
}
