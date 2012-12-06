﻿using System;
using System.Collections.Generic;
using System.Linq;
using Zeta;
using Zeta.Common.Plugins;
using Zeta.CommonBot;
using Zeta.Internals.Actors;
namespace GilesTrinity
{
    public partial class GilesTrinity : IPlugin
    {
        // Refresh the skills in our hotbar
        // Also caches the values after - but ONLY if we aren't in archon mode (or if this function is told NOT to cache this)
        public static void GilesRefreshHotbar(bool dontCacheThis = false)
        {
            bMappedPlayerAbilities = true;
            hashPowerHotbarAbilities = new HashSet<SNOPower>();
            for (int i = 0; i <= 5; i++)
                hashPowerHotbarAbilities.Add(ZetaDia.Me.GetHotbarPowerId((HotbarSlot)i));
            bRefreshHotbarAbilities = false;
            if (!dontCacheThis)
                hashCachedPowerHotbarAbilities = new HashSet<SNOPower>(hashPowerHotbarAbilities);
        }
        /// <summary>
        /// Check if a particular buff is present
        /// </summary>
        /// <param name="power"></param>
        /// <returns></returns>
        public static bool GilesHasBuff(SNOPower power)
        {
            int id = (int)power;
            return listCachedBuffs.Any(u => u.SNOId == id);
        }

        /// <summary>
        /// Returns how many stacks of a particular buff there are
        /// </summary>
        /// <param name="power"></param>
        /// <returns></returns>
        public static int GilesBuffStacks(SNOPower power)
        {
            int stacks;
            if (dictCachedBuffs.TryGetValue((int)power, out stacks))
            {
                return stacks;
            }
            return 0;
        }
        /// <summary>
        /// Check re-use timers on skills
        /// </summary>
        /// <param name="power">The power.</param>
        /// <param name="recheck">if set to <c>true</c> check again.</param>
        /// <returns>
        /// Returns whether or not we can use a skill, or if it's on our own internal Trinity cooldown timer
        /// </returns>
        private static bool GilesUseTimer(SNOPower power, bool recheck = false)
        {
            if (DateTime.Now.Subtract(dictAbilityLastUse[power]).TotalMilliseconds >= dictAbilityRepeatDelay[power])
                return true;
            if (recheck && DateTime.Now.Subtract(dictAbilityLastUse[power]).TotalMilliseconds >= 150 && DateTime.Now.Subtract(dictAbilityLastUse[power]).TotalMilliseconds <= 600)
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
            while (bKeepLooping)
            {
                iSafetyLoops++;
                if (iSafetyLoops > maxSafetyLoops)
                    bKeepLooping = false;
                bool bIsAnimating = false;
                try
                {
                    ACDAnimationInfo myAnimationState = ZetaDia.Me.CommonData.AnimationInfo;
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
            }
        }

        // This function checks when the spell last failed (according to D3 memory, which isn't always reliable)
        // To prevent Trinity getting stuck re-trying the same spell over and over and doing nothing else
        // No longer used but keeping this here incase I re-use it
        private static bool GilesCanRecastAfterFailure(SNOPower power, int maxRecheckTime = 250)
        {
            if (DateTime.Now.Subtract(dictAbilityLastFailed[power]).TotalMilliseconds <= maxRecheckTime)
                return false;
            return true;
        }

        // When last hit the power-manager for this - not currently used, saved here incase I use it again in the future!
        // This is a safety function to prevent spam of the CPU and time-intensive "PowerManager.CanCast" function in DB
        // No longer used but keeping this here incase I re-use it
        private static bool GilesPowerManager(SNOPower power, int maxRecheckTime)
        {
            if (DateTime.Now.Subtract(dictAbilityLastPowerChecked[power]).TotalMilliseconds <= maxRecheckTime)
                return false;
            dictAbilityLastPowerChecked[power] = DateTime.Now;
            if (PowerManager.CanCast(power))
                return true;
            return false;
        }

        // Checking for buffs and caching the buff list
        // Cache all current buffs on character
        public static void GilesRefreshBuffs()
        {
            listCachedBuffs = new List<Buff>();
            dictCachedBuffs = new Dictionary<int, int>();
            listCachedBuffs = ZetaDia.Me.GetAllBuffs().ToList();
            // Special flag for detecting the activation and de-activation of archon
            bool bThisArchonBuff = false;
            int iTempStackCount;
            // Store how many stacks of each buff we have
            foreach (Buff thisbuff in listCachedBuffs)
            {
                // Store the stack count of this buff
                if (!dictCachedBuffs.TryGetValue(thisbuff.SNOId, out iTempStackCount))
                    dictCachedBuffs.Add(thisbuff.SNOId, thisbuff.StackCount);
                // Check for archon stuff
                if (thisbuff.SNOId == (int)SNOPower.Wizard_Archon)
                    bThisArchonBuff = true;
            }
            // Archon stuff
            if (bThisArchonBuff)
            {
                if (!bHasHadArchonbuff)
                    bRefreshHotbarAbilities = true;
                bHasHadArchonbuff = true;
            }
            else
            {
                if (bHasHadArchonbuff)
                {
                    hashPowerHotbarAbilities = new HashSet<SNOPower>(hashCachedPowerHotbarAbilities);
                }
                bHasHadArchonbuff = false;
            }
            //"g_killElitePack : 1, snoid=230745" <- Noting this here incase I ever want to monitor NV stacks, this is the SNO ID code for it!
        }

        /// <summary>
        /// A default power in case we can't use anything else
        /// </summary>
        private static GilesPower defaultPower = new GilesPower(SNOPower.None, 0, vNullLocation, -1, -1, 0, 0, false);

        /// <summary>
        /// Returns an appropriately selected GilesPower and related information
        /// </summary>
        /// <param name="bCurrentlyAvoiding">Are we currently avoiding?</param>
        /// <param name="bOOCBuff">Buff Out Of Combat</param>
        /// <param name="bDestructiblePower">Is this for breaking destructables?</param>
        /// <returns></returns>
        internal static GilesPower GilesAbilitySelector(bool bCurrentlyAvoiding = false, bool bOOCBuff = false, bool bDestructiblePower = false)
        {
            // Refresh buffs once to save buff-check-spam
            GilesRefreshBuffs();
            // See if archon just appeared/disappeared, so update the hotbar
            if (bRefreshHotbarAbilities)
                GilesRefreshHotbar(GilesHasBuff(SNOPower.Wizard_Archon));
            // Extra height thingy, not REALLY used as it was originally going to be, will probably get phased out...
            float iThisHeight = iExtraHeight;
            // Switch based on the cached character class
            switch (iMyCachedActorClass)
            {
                // Barbs
                case ActorClass.Barbarian: 
                    return GetBarbarianPower(bCurrentlyAvoiding, bOOCBuff, bDestructiblePower);

                // Monks
                case ActorClass.Monk: 
                    return GetMonkPower(bCurrentlyAvoiding, bOOCBuff, bDestructiblePower);

                // Wizards
                case ActorClass.Wizard: 
                    return GetWizardPower(bCurrentlyAvoiding, bOOCBuff, bDestructiblePower);

                // Witch Doctors
                case ActorClass.WitchDoctor:
                    return GetWitchDoctorPower(bCurrentlyAvoiding, bOOCBuff, bDestructiblePower, iThisHeight);

                // Demon Hunters
                case ActorClass.DemonHunter:
                    return GetDemonHunterPower(bCurrentlyAvoiding, bOOCBuff, bDestructiblePower);
            }
            return defaultPower;
        }

        /// <summary>
        /// Returns true if we have the ability and the buff is up, or if we don't have the ability in our hotbar
        /// </summary>
        /// <param name="snoPower"></param>
        /// <returns></returns>
        internal static bool CheckAbilityAndBuff(SNOPower snoPower)
        {
            return
                (!hashPowerHotbarAbilities.Contains(snoPower) || (hashPowerHotbarAbilities.Contains(snoPower) && GilesHasBuff(snoPower)));

        }

    }
}
