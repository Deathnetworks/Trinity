using System;

namespace Trinity.Config.Combat
{
    public enum GoblinPriority
    {
        Ignore = 0,
        Normal = 1,
        Prioritize = 2,
        Kamikaze = 3
    }
    public enum TempestRushOption
    {
        MovementOnly = 0,
        ElitesGroupsOnly = 1,
        CombatOnly = 2,
        Always = 3,
        TrashOnly = 4,
    }
    public enum WizardKiteOption
    {
        Anytime,
        ArchonOnly,
        NormalOnly
    }

    public enum WizardArchonCancelOption
    {
        Never,
        Timer,
        RebuffArmor,
        RebuffMagicWeaponFamiliar,
    }

    public enum DestructibleIgnoreOption
    {
        ForceIgnore,
        OnlyIfStuck,
        DestroyAll
    }

    public enum BarbarianWOTBMode
    {
        HardElitesOnly,
        Normal,
        WhenReady
    }
}
