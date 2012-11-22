using Zeta.Common;
using Zeta.Common.Plugins;
using Zeta.Internals.Actors;
namespace GilesTrinity
{
    public partial class GilesTrinity : IPlugin
    {
// Giles Power - used when picking a power to use to cache where/how to use it
        public class GilesPower
        {
            public SNOPower SNOPower { get; set; }
            public float iMinimumRange { get; set; }
            public Vector3 vTargetLocation { get; set; }
            public int iTargetWorldID { get; set; }
            public int iTargetGUID { get; set; }
            public int iForceWaitLoopsBefore { get; set; }
            public int iForceWaitLoopsAfter { get; set; }
            public bool bWaitWhileAnimating { get; set; }
            public GilesPower(SNOPower snoPower, float fRange, Vector3 vPosition, int iWorldId, int iGuid, int iWaitLoops, int iAfterLoops, bool bRepeat)
            {
                SNOPower = snoPower;
                iMinimumRange = fRange;
                vTargetLocation = vPosition;
                iTargetWorldID = iWorldId;
                iTargetGUID = iGuid;
                iForceWaitLoopsBefore = iWaitLoops;
                iForceWaitLoopsAfter = iAfterLoops;
                bWaitWhileAnimating = bRepeat;
            }
        }
    }
}
