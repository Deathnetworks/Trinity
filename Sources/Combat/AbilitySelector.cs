using System;
using System.Collections.Generic;
using System.Linq;
using Trinity.Technicals;
using Zeta;
using Zeta.Common;
using Zeta.Common.Plugins;
using Zeta.CommonBot;
using Zeta.Internals;
using Zeta.Internals.Actors;

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
        /// Check re-use timers on skills
        /// </summary>
        /// <param name="power">The power.</param>
        /// <param name="recheck">if set to <c>true</c> check again.</param>
        /// <returns>
        /// Returns whether or not we can use a skill, or if it's on our own internal Trinity cooldown timer
        /// </returns>
        public static bool SNOPowerUseTimer(SNOPower power, bool recheck = false)
        {
            if (DateTime.Now.Subtract(AbilityLastUsedCache[power]).TotalMilliseconds >= DataDictionary.AbilityRepeatDelays[power])
                return true;
            if (recheck && DateTime.Now.Subtract(AbilityLastUsedCache[power]).TotalMilliseconds >= 150 && DateTime.Now.Subtract(AbilityLastUsedCache[power]).TotalMilliseconds <= 600)
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

                TrinityPower power = CurrentPower;

                using (new PerformanceLogger("AbilitySelector.ClassAbility"))
                {
                    switch (PlayerStatus.ActorClass)
                    {
                        // Barbs
                        case ActorClass.Barbarian:
                            power = GetBarbarianPower(IsCurrentlyAvoiding, UseOOCBuff, UseDestructiblePower);
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
                        case ActorClass.WitchDoctor:
                            power = GetWitchDoctorPower(IsCurrentlyAvoiding, UseOOCBuff, UseDestructiblePower);
                            break;
                        // Demon Hunters
                        case ActorClass.DemonHunter:
                            power = GetDemonHunterPower(IsCurrentlyAvoiding, UseOOCBuff, UseDestructiblePower);
                            break;
                    }
                }
                // use IEquatable to check if they're equal
                if (CurrentPower == power)
                {
                    DbHelper.Log(LogCategory.Behavior, "Keeping {0}", CurrentPower.ToString());
                    return CurrentPower;
                }
                else if (power != null)
                {
                    DbHelper.Log(LogCategory.Behavior, "Selected new {0}", power.ToString());
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
        /// Gets the default weapon power based on the current equipped primary weapon
        /// </summary>
        /// <returns></returns>
        private static SNOPower GetDefaultWeaponPower()
        {
            ACDItem rhItem = ZetaDia.Me.Inventory.Equipped.Where(i => i.InventorySlot == InventorySlot.PlayerLeftHand).FirstOrDefault();
            if (rhItem == null)
                return SNOPower.None;

            switch (rhItem.ItemType)
            {
                default:
                    return SNOPower.Weapon_Melee_Instant;
                case ItemType.Axe:
                case ItemType.CeremonialDagger:
                case ItemType.Dagger:
                case ItemType.Daibo:
                case ItemType.FistWeapon:
                case ItemType.Mace:
                case ItemType.Polearm:
                case ItemType.Spear:
                case ItemType.Staff:
                case ItemType.Sword:
                    return SNOPower.Weapon_Melee_Instant;
                case ItemType.Wand:
                    return SNOPower.Weapon_Ranged_Wand;
                case ItemType.Bow:
                case ItemType.Crossbow:
                case ItemType.HandCrossbow:
                    return SNOPower.Weapon_Ranged_Projectile;
            }
        }
        /// <summary>
        /// Gets the default weapon distance based on the current equipped primary weapon
        /// </summary>
        /// <returns></returns>
        private static float GetDefaultWeaponDistance()
        {
            switch (GetDefaultWeaponPower())
            {
                case SNOPower.Weapon_Ranged_Instant:
                case SNOPower.Weapon_Ranged_Projectile:
                    return 65f;
                case SNOPower.Weapon_Ranged_Wand:
                    return 35f;
                case SNOPower.Weapon_Melee_Instant:
                case SNOPower.Weapon_Melee_Instant_BothHand:
                default:
                    return 10f;
            }
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
