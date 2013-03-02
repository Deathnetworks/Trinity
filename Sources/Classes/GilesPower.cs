using Zeta.Common;
using Zeta.Internals.Actors;

namespace GilesTrinity
{
    /// <summary>
    /// Giles Power - used when picking a power and where/how to use it
    /// </summary>
    internal class TrinityPower
    {
        public SNOPower SNOPower { get; set; }
        public float MinimumRange { get; set; }
        public Vector3 TargetPosition { get; set; }
        public int TargetDynamicWorldId { get; set; }
        public int TargetRActorGUID { get; set; }
        public int WaitTicksBeforeUse { get; set; }
        public int WaitTicksAfterUse { get; set; }
        public bool WaitForAnimationFinished { get; set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="TrinityPower" /> class.
        /// </summary>
        /// <param name="snoPower">The sno power.</param>
        /// <param name="minimumRange">The f range.</param>
        /// <param name="position">The v position.</param>
        /// <param name="targetDynamicWorldId">The i world id.</param>
        /// <param name="targetRActorGUID">The i GUID.</param>
        /// <param name="waitTicksBeforeUse">The i wait loops.</param>
        /// <param name="waitTicksAfterUse">The i after loops.</param>
        /// <param name="waitForAnimationFinished">if set to <c>true</c> [b repeat].</param>
        public TrinityPower(SNOPower snoPower, float minimumRange, Vector3 position, int targetDynamicWorldId, int targetRActorGUID, int waitTicksBeforeUse, int waitTicksAfterUse, bool waitForAnimationFinished)
        {
            SNOPower = snoPower;
            MinimumRange = minimumRange;
            TargetPosition = position;
            TargetDynamicWorldId = targetDynamicWorldId;
            TargetRActorGUID = targetRActorGUID;
            WaitTicksBeforeUse = waitTicksBeforeUse;
            WaitTicksAfterUse = waitTicksAfterUse;
            WaitForAnimationFinished = waitForAnimationFinished;
        }
    }
}
