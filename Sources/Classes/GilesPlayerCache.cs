using System;
using Zeta.Common;
using Zeta.Common.Plugins;
namespace GilesTrinity
{
    public partial class GilesTrinity : IPlugin
    {
// Current cached player data
// Just stores the data on YOU, well, your character's current status - for readability of those variables more than anything, but also caching
        public class GilesPlayerCache
        {
            public DateTime lastUpdatedPlayer { get; set; }
            public bool bIsIncapacitated { get; set; }
            public bool bIsRooted { get; set; }
            public bool bIsInTown { get; set; }
            public double dCurrentHealthPct { get; set; }
            public double dCurrentEnergy { get; set; }
            public double dCurrentEnergyPct { get; set; }
            public double dDiscipline { get; set; }
            public double dDisciplinePct { get; set; }
            public Vector3 vCurrentPosition { get; set; }
            public bool bWaitingForReserveEnergy { get; set; }
            public int iMyDynamicID { get; set; }
            public int iMyLevel { get; set; }
            public GilesPlayerCache(DateTime lastupdated, bool incapacitated, bool isrooted, bool isintown, double currenthealth, double currentenergy, double currentenergypct,
                double discipline, double disciplinepct, Vector3 currentpos, bool waitingreserve, int dynamicid, int mylevel)
            {
                lastUpdatedPlayer = lastupdated;
                bIsIncapacitated = incapacitated;
                bIsRooted = isrooted;
                bIsInTown = isintown;
                dCurrentHealthPct = currenthealth;
                dCurrentEnergy = currentenergy;
                dCurrentEnergyPct = currentenergypct;
                dDiscipline = discipline;
                dDisciplinePct = disciplinepct;
                vCurrentPosition = currentpos;
                bWaitingForReserveEnergy = waitingreserve;
                iMyDynamicID = dynamicid;
                iMyLevel = mylevel;
            }
        }
    }
}
