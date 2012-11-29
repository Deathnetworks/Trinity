using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GilesTrinity.Technicals
{
    /// <summary>
    /// Enumerate all log categories
    /// </summary>
    [Flags]
    public enum LogCategory
    {
        UserInformation = 0,
        ItemValuation,
        CacheManagement,
        ScriptRule,
        Configuration,
        UI,
        WeaponSwap,
        Behavior,
        Performance

    }
}
