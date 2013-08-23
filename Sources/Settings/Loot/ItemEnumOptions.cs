using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Trinity.Config.Loot
{
    public enum SalvageOption
    {
        None = 0,
        InfernoOnly = 1,
        All = 2
    }

    public enum PotionMode
    {
        All = 1,
        Cap = 2,
        Ignore = 3
    }

    public enum ItemRuleType
    {
        Config,
        Soft,
        Hard,
        Custom
    }

    public enum ItemFilterMode
    {
        TrinityOnly,
        TrinityWithItemRules,
        DemonBuddy
    }

    [Flags]
    public enum TrinityGemType
    {
        Emerald = 1,
        Topaz = 2,
        Amethyst = 4,
        Ruby = 8,
        None = 16,
        All = Emerald | Topaz | Amethyst | Ruby
    }

    public enum ItemRuleLogLevel
    {
        None = 0,
        Normal = 1,
        Magic = 2,
        Rare = 3,
        Legendary = 4
    }
}
