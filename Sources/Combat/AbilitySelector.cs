using System;
using System.Linq;
using Trinity.Combat.Abilities;
using Trinity.Technicals;
using Zeta.Common;
using Zeta.Common.Plugins;
using Zeta.Game;
using Zeta.Game.Internals.Actors;
using Logger = Trinity.Technicals.Logger;

namespace Trinity
{
    public partial class Trinity : IPlugin
    {


        /// <summary>
        /// Check if a particular buff is present
        /// </summary>
        /// <param name="power"></param>
        /// <returns></returns>
        public static bool GetHasBuff(SNOPower power)
        {
            int id = (int)power;
            return listCachedBuffs.Any(u => u.SNOId == id);
        }

        /// <summary>
        /// Returns how many stacks of a particular buff there are
        /// </summary>
        /// <param name="power"></param>
        /// <returns></returns>
        public static int GetBuffStacks(SNOPower power)
        {
            int stacks;
            if (dictCachedBuffs.TryGetValue((int)power, out stacks))
            {
                return stacks;
            }
            return 0;
        }

        /// <summary>
        /// Check re-use timers on skills, returns true if we can use the power
        /// </summary>
        /// <param name="power">The power.</param>
        /// <param name="recheck">if set to <c>true</c> check again.</param>
        /// <returns>
        /// Returns whether or not we can use a skill, or if it's on our own internal Trinity cooldown timer
        /// </returns>
        public static bool SNOPowerUseTimer(SNOPower power, bool recheck = false)
        {
            if (TimeSinceUse(power) >= CombatBase.GetSNOPowerUseDelay(power))
                return true;
            if (recheck && TimeSinceUse(power) >= 150 && TimeSinceUse(power) <= 600)
                return true;
            return false;
        }

        /// <summary>
        /// Quick and Dirty routine just to force a wait until the character is "free"
        /// </summary>
        /// <param name="maxSafetyLoops">The max safety loops.</param>
        /// <param name="waitForAttacking">if set to <c>true</c> wait for attacking.</param>
        public static void WaitWhileAnimating(int maxSafetyLoops = 10, bool waitForAttacking = false)
        {
            bool bKeepLooping = true;
            int iSafetyLoops = 0;
            ACDAnimationInfo myAnimationState = ZetaDia.Me.CommonData.AnimationInfo;
            while (bKeepLooping)
            {
                iSafetyLoops++;
                if (iSafetyLoops > maxSafetyLoops)
                    bKeepLooping = false;
                bool bIsAnimating = false;
                try
                {
                    myAnimationState = ZetaDia.Me.CommonData.AnimationInfo;
                    if (myAnimationState == null || myAnimationState.State == AnimationState.Casting || myAnimationState.State == AnimationState.Channeling)
                        bIsAnimating = true;
                    if (waitForAttacking && (myAnimationState == null || myAnimationState.State == AnimationState.Attacking))
                        bIsAnimating = true;
                }
                catch
                {
                    bIsAnimating = true;
                }
                if (!bIsAnimating)
                    bKeepLooping = false;
                //DbHelper.Log(TrinityLogLevel.Debug, LogCategory.Behavior, "Waiting for animation, maxLoops={0} waitForAttacking={1} anim={2}", maxSafetyLoops, waitForAttacking, myAnimationState.State);

            }
        }


        /// <summary>
        /// A default power in case we can't use anything else
        /// </summary>
        private static TrinityPower defaultPower = new TrinityPower(SNOPower.None, 10f, Vector3.Zero, -1, -1, 0, 0, WAIT_FOR_ANIM);

        /// <summary>
        /// Returns an appropriately selected TrinityPower and related information
        /// </summary>
        /// <param name="IsCurrentlyAvoiding">Are we currently avoiding?</param>
        /// <param name="UseOOCBuff">Buff Out Of Combat</param>
        /// <param name="UseDestructiblePower">Is this for breaking destructables?</param>
        /// <returns></returns>
        internal static TrinityPower AbilitySelector(bool IsCurrentlyAvoiding = false, bool UseOOCBuff = false, bool UseDestructiblePower = false)
        {
            using (new PerformanceLogger("AbilitySelector"))
            {
                // See if archon just appeared/disappeared, so update the hotbar
                if (ShouldRefreshHotbarAbilities)
                    PlayerInfoCache.RefreshHotbar();

                // Switch based on the cached character class

                TrinityPower power = CombatBase.CurrentPower;

                using (new PerformanceLogger("AbilitySelector.ClassAbility"))
                {
                    switch (Player.ActorClass)
                    {
                        // Barbs
                        case ActorClass.Barbarian:
                            //power = GetBarbarianPower(IsCurrentlyAvoiding, UseOOCBuff, UseDestructiblePower);
                            power = BarbarianCombat.GetPower();
                            break;
                        // Monks
                        case ActorClass.Monk:
                            power = GetMonkPower(IsCurrentlyAvoiding, UseOOCBuff, UseDestructiblePower);
                            break;
                        // Wizards
                        case ActorClass.Wizard:
                            power = GetWizardPower(IsCurrentlyAvoiding, UseOOCBuff, UseDestructiblePower);
                            break;
                        // Witch Doctors
                        case ActorClass.Witchdoctor:
                            power = GetWitchDoctorPower(IsCurrentlyAvoiding, UseOOCBuff, UseDestructiblePower);
                            break;
                        // Demon Hunters
                        case ActorClass.DemonHunter:
                            power = GetDemonHunterPower(IsCurrentlyAvoiding, UseOOCBuff, UseDestructiblePower);
                            break;
                    }
                }
                // use IEquatable to check if they're equal
                if (CombatBase.CurrentPower == power)
                {
                    Logger.Log(LogCategory.Behavior, "Keeping {0}", CombatBase.CurrentPower.ToString());
                    return CombatBase.CurrentPower;
                }
                else if (power != null && power.SNOPower != SNOPower.None)
                {
                    Logger.Log(LogCategory.Behavior, "Selected new {0}", power.ToString());
                    return power;
                }
                else
                    return defaultPower;
            }
        }

        /// <summary>
        /// Returns true if we have the ability and the buff is up, or true if we don't have the ability in our hotbar
        /// </summary>
        /// <param name="snoPower"></param>
        /// <returns></returns>
        internal static bool CheckAbilityAndBuff(SNOPower snoPower)
        {
            return
                (!Hotbar.Contains(snoPower) || (Hotbar.Contains(snoPower) && GetHasBuff(snoPower)));

        }

        /// <summary>
        /// Gets the time in Millseconds since we've used the specified power
        /// </summary>
        /// <param name="power"></param>
        /// <returns></returns>
        internal static double TimeSinceUse(SNOPower power)
        {
            if (AbilityLastUsedCache.ContainsKey(power))
                return DateTime.Now.Subtract(AbilityLastUsedCache[power]).TotalMilliseconds;
            else
                return -1;
        }


    }

}
