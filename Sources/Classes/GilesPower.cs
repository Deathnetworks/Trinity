using Zeta.Common;
using Zeta.Internals.Actors;

namespace GilesTrinity
{
    /// <summary>
    /// Giles Power - used when picking a power and where/how to use it
    /// </summary>
    internal class GilesPower
    {
        public SNOPower SNOPower { get; set; }
        public float iMinimumRange { get; set; }
        public Vector3 vTargetLocation { get; set; }
        public int iTargetWorldID { get; set; }
        public int iTargetGUID { get; set; }
        public int iForceWaitLoopsBefore { get; set; }
        public int iForceWaitLoopsAfter { get; set; }
        public bool bWaitWhileAnimating { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="GilesPower" /> class.
        /// </summary>
        /// <param name="snoPower">The sno power.</param>
        /// <param name="fRange">The f range.</param>
        /// <param name="vPosition">The v position.</param>
        /// <param name="iWorldId">The i world id.</param>
        /// <param name="iGuid">The i GUID.</param>
        /// <param name="iWaitLoops">The i wait loops.</param>
        /// <param name="iAfterLoops">The i after loops.</param>
        /// <param name="bRepeat">if set to <c>true</c> [b repeat].</param>
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
