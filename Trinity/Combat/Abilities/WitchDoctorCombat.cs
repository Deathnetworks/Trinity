
using System.Collections.Generic;
namespace Trinity.Combat.Abilities
{
    public class WitchDoctorCombat : CombatBase
    {
        public static System.Diagnostics.Stopwatch VisionQuestRefreshTimer = new System.Diagnostics.Stopwatch();
        public static long GetTimeSinceLastVisionQuestRefresh()
        {
            if (!VisionQuestRefreshTimer.IsRunning)
                VisionQuestRefreshTimer.Start();

            return VisionQuestRefreshTimer.ElapsedMilliseconds;
        }
        
        public static readonly HashSet<int> ZunimasaItemIds = new HashSet<int>()
        {
            -960430780, // Zunimassa's String of Skulls
            -1187722720, // Zunimassa's Pox
            1941359608, // Zunimassa's Trail
        };

    }
}
