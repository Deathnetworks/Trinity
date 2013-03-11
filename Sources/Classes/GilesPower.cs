using System;
using Zeta.Common;
using Zeta.Internals.Actors;

namespace GilesTrinity
{
    /// <summary>
    /// TrinityPower - used when picking a power and where/how to use it
    /// </summary>
    internal class TrinityPower : IEquatable<TrinityPower>
    {
        // 100 == 10 tps or 1/10th a second
        // 66 == 15 tps or 1/15th a second
        // 50 = 20 tps or 1/20th a second
        private const int _tickTimeMs = 20;        

        public SNOPower SNOPower { get; set; }
        /// <summary>
        /// The minimum distance from the target (Position or Unit) we should be in before using this power
        /// </summary>
        public float MinimumRange { get; set; }
        /// <summary>
        /// For position based spells (non-Unit)
        /// </summary>
        public Vector3 TargetPosition { get; set; }
        /// <summary>
        /// Always the CurrentDynamicWorldID
        /// </summary>
        public int TargetDynamicWorldId { get; set; }
        /// <summary>
        /// The Unit RActorGUID that we want to target
        /// </summary>
        public int TargetRActorGUID { get; set; }
        /// <summary>
        /// The number of 1/10th second intervals we should wait before casting this power
        /// </summary>
        public float WaitTicksBeforeUse { get; set; }
        /// <summary>
        /// The number of 1/10th second intervals we should wait after casting this power
        /// </summary>
        public float WaitTicksAfterUse { get; set; }
        /// <summary>
        /// Whether or not we should wait for the player animation to complete after casting this power
        /// </summary>
        public bool WaitForAnimationFinished { get; set; }
        /// <summary>
        /// The DateTime when the power was assigned
        /// </summary>
        public DateTime PowerAssignmentTime { get; set; }
        /// <summary>
        /// Returns the DateTime the power was last used <seealso cref="GilesTrinity.dictAbilityLastUse"/>
        /// </summary>
        public DateTime PowerLastUsedTime
        {
            get
            {
                if (GilesTrinity.dictAbilityLastUse.ContainsKey(this.SNOPower))
                    return GilesTrinity.dictAbilityLastUse[this.SNOPower];
                else
                    return DateTime.MinValue;
            }
        }

        /// <summary>
        /// The minimum delay we should wait before using a power
        /// </summary>
        public double WaitBeforeUseDelay
        {
            get
            {
                return WaitTicksBeforeUse * _tickTimeMs;
            }
        }

        /// <summary>
        /// The minimum delay in millseconds we should wait after using a power
        /// </summary>
        public double WaitAfterUseDelay
        {
            get
            {
                return WaitTicksAfterUse * _tickTimeMs;
            }
        }

        /// <summary>
        /// Gets the milliseconds since the power was assigned
        /// </summary>
        /// <returns></returns>
        public double TimeSinceAssigned
        {
            get
            {
                return DateTime.Now.Subtract(PowerAssignmentTime).TotalMilliseconds;
            }
        }

        /// <summary>
        /// Gets the millseconds since the power was last used
        /// </summary>
        /// <returns></returns>
        public double TimeSinceUse
        {
            get
            {
                return GilesTrinity.TimeSinceUse(this.SNOPower);
            }
        }

        /// <summary>
        /// Returns True when we bot should be waiting before using a power
        /// </summary>
        public bool ShouldWaitBeforeUse
        {
            get
            {
                // if the number of milliseconds since we assigned it is less than the number of ticks*100 we should wait
                return TimeSinceAssigned < WaitBeforeUseDelay;
            }
        }

        /// <summary>
        /// Returns true when the bot should be waiting after using a power
        /// </summary>
        public bool ShouldWaitAfterUse
        {
            get
            {
                // if the number of millseconds since we used it is more than the number of ticks*100 we should wait
                return TimeSinceUse < WaitAfterUseDelay;
            }
        }

        public TrinityPower()
        {
            this.PowerAssignmentTime = DateTime.Now;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TrinityPower" /> class.
        /// </summary>
        /// <param name="snoPower">The SNOPower to be used</param>
        /// <param name="minimumRange">The minimum range required from the Position or Target to be used</param>
        /// <param name="position">The Position to use the power at</param>
        /// <param name="targetDynamicWorldId">Usually the CurrentDynamicWorlID</param>
        /// <param name="targetRActorGUID">The Unit we are targetting</param>
        /// <param name="waitTicksBeforeUse">The number of "ticks" to wait before using a power - logically 1/10th of a second</param>
        /// <param name="waitTicksAfterUse">The number of "ticks" to wait after using a power - logically 1/10th of a second</param>
        /// <param name="waitForAnimationFinished">Force the bot to wait for casting animation to complete after using</param>
        public TrinityPower(SNOPower snoPower, float minimumRange, Vector3 position, int targetDynamicWorldId, int targetRActorGUID, float waitTicksBeforeUse, float waitTicksAfterUse, bool waitForAnimationFinished)
        {
            SNOPower = snoPower;
            MinimumRange = minimumRange;
            TargetPosition = position;
            TargetDynamicWorldId = targetDynamicWorldId;
            TargetRActorGUID = targetRActorGUID;
            WaitTicksBeforeUse = waitTicksBeforeUse;
            WaitTicksAfterUse = waitTicksAfterUse;
            WaitForAnimationFinished = waitForAnimationFinished;
            PowerAssignmentTime = DateTime.Now;
        }


        public bool Equals(TrinityPower other)
        {
            return this.SNOPower == other.SNOPower &&
                this.TargetPosition == other.TargetPosition &&
                this.TargetRActorGUID == other.TargetRActorGUID &&
                this.WaitAfterUseDelay == other.WaitAfterUseDelay &&
                this.TargetDynamicWorldId == other.TargetDynamicWorldId;
        }
    }
}
