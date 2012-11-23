using System;
using Zeta.Common;
using Zeta.Common.Plugins;

namespace GilesTrinity
{
    // Current cached player data
    // Just stores the data on YOU, well, your character's current status - for readability of those variables more than anything, but also caching
    public class GilesPlayerCache
    {
        public DateTime LastUpdated { get; set; }
        public bool IsIncapacitated { get; set; }
        public bool IsRooted { get; set; }
        public bool IsInTown { get; set; }
        public double CurrentHealthPct { get; set; }
        public double CurrentEnergy { get; set; }
        public double CurrentEnergyPct { get; set; }
        public double Discipline { get; set; }
        public double DisciplinePct { get; set; }
        public Vector3 CurrentPosition { get; set; }
        public bool WaitingForReserveEnergy { get; set; }
        public int MyDynamicID { get; set; }
        public int Level { get; set; }

        public GilesPlayerCache(DateTime lastUpdated, bool incapacitated, bool isRooted, bool isInTown, double currentHealth, double currentEnergy, double currentEnergyPct,
            double discipline, double disciplinePct, Vector3 currentPosition, bool waitingReserve, int dynamicId, int level)
        {
            LastUpdated = lastUpdated;
            IsIncapacitated = incapacitated;
            IsRooted = isRooted;
            IsInTown = isInTown;
            CurrentHealthPct = currentHealth;
            CurrentEnergy = currentEnergy;
            CurrentEnergyPct = currentEnergyPct;
            Discipline = discipline;
            DisciplinePct = disciplinePct;
            CurrentPosition = currentPosition;
            WaitingForReserveEnergy = waitingReserve;
            MyDynamicID = dynamicId;
            Level = level;
        }
    }
}
