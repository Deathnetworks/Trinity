using System;

namespace Trinity.Technicals
{
    /// <summary>
    /// Enumerate all log categories
    /// </summary>
    [Flags]
    public enum LogCategory
    {
        UserInformation = 0,
        ItemValuation = 1,
        CacheManagement = 2,
        ScriptRule = 4,
        Configuration = 8,
        UI = 16,
        Behavior = 32,
        Performance = 64,
        Targetting = 128,
        Weight = 256,
        ProfileTag = 512,
        Movement = 1024,
        GlobalHandler = 2048,
        Navigator = 4096,
        Avoidance = 8192,
        SpellTrack = 16384,
        Animation = 32768,
        UnknownObjects = 65536,
        ActiveBuffs = 131072,
        SkillSelection = 262144
    }
}
