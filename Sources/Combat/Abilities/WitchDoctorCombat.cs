using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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


    }
}
