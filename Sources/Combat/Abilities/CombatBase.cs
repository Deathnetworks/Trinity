using Trinity.DbProvider;
using Trinity.Technicals;
using System;
using System.Linq;
using Zeta.Common;
using Zeta.Common.Plugins;
using Zeta.CommonBot;
using Zeta.Internals.Actors;
using System.Collections.Generic;
using Zeta;
using Trinity.Config;

namespace Trinity.Combat.Abilities
{
    public class CombatBase
    {
        private static bool isWaitingForSpecial = false;
        private static TrinityPower currentPower = new TrinityPower();
        private static Vector3 lastZigZagLocation = Vector3.Zero;
        private static Vector3 zigZagPosition = Vector3.Zero;

        public static Vector3 ZigZagPosition
        {
            get { return CombatBase.zigZagPosition; }
            internal set { CombatBase.zigZagPosition = value; }
        }

        public enum AnimWait
        {
            NO_WAIT = 0,
            WAIT = 1
        }
        public enum CanCastFlags
        {
            All = 2,
            NoTimer = 4,
            NoPowerManager = 8
        }

        /// <summary>
        /// Returns an appropriately selected TrinityPower and related information
        /// </summary>
        /// <param name="IsCurrentlyAvoiding">Are we currently avoiding?</param>
        /// <param name="UseOOCBuff">Buff Out Of Combat</param>
        /// <param name="UseDestructiblePower">Is this for breaking destructables?</param>
        /// <returns></returns>
        internal static TrinityPower AbilitySelector()
        {
            using (new PerformanceLogger("AbilitySelector"))
            {
                // See if archon just appeared/disappeared, so update the hotbar
                if (Trinity.ShouldRefreshHotbarAbilities)
                    PlayerInfoCache.RefreshHotbar();

                // Switch based on the cached character class

                TrinityPower power = CombatBase.CurrentPower;

                using (new PerformanceLogger("AbilitySelector.ClassAbility"))
                {
                    switch (Player.ActorClass)
                    {
                        // Barbs
                        case ActorClass.Barbarian:
                            power = BarbarianCombat.GetPower();
                            break;
                        // Monks
                        //case ActorClass.Monk:
                        //    power = GetMonkPower(IsCurrentlyAvoiding, UseOOCBuff, UseDestructiblePower);
                        //    break;
                        //// Wizards
                        //case ActorClass.Wizard:
                        //    power = GetWizardPower(IsCurrentlyAvoiding, UseOOCBuff, UseDestructiblePower);
                        //    break;
                        //// Witch Doctors
                        //case ActorClass.WitchDoctor:
                        //    power = GetWitchDoctorPower(IsCurrentlyAvoiding, UseOOCBuff, UseDestructiblePower);
                        //    break;
                        //// Demon Hunters
                        //case ActorClass.DemonHunter:
                        //    power = GetDemonHunterPower(IsCurrentlyAvoiding, UseOOCBuff, UseDestructiblePower);
                        //    break;
                    }
                }
                // use IEquatable to check if they're equal
                if (CombatBase.CurrentPower == power)
                {
                    Logger.Log(LogCategory.Behavior, "Keeping {0}", CombatBase.CurrentPower.ToString());
                    return CombatBase.CurrentPower;
                }
                else if (power != null)
                {
                    Logger.Log(LogCategory.Behavior, "Selected new {0}", power.ToString());
                    return power;
                }
                else
                    return DefaultPower;
            }
        }

        public static Dictionary<SNOPower, DateTime> AbilityLastUsedCache
        {
            get
            {
                return Trinity.AbilityLastUsedCache;
            }
            set
            {
                Trinity.AbilityLastUsedCache = value;
            }
        }

        public static bool IsWaitingForSpecial
        {
            get { return CombatBase.isWaitingForSpecial; }
            set { CombatBase.isWaitingForSpecial = value; }
        }

        public static int MinEnergyReserve
        {
            get
            {
                switch (Player.ActorClass)
                {
                    case ActorClass.Barbarian:
                        return V.I("Barbarian.MinEnergyReserve");
                    case ActorClass.DemonHunter:
                        return V.I("DemonHunter.MinEnergyReserve");
                    case ActorClass.Monk:
                        return V.I("Monk.MinEnergyReserve");
                    case ActorClass.WitchDoctor:
                        return V.I("WitchDoctor.MinEnergyReserve");
                    case ActorClass.Wizard:
                        return V.I("Wizard.MinEnergyReserve");
                    default:
                        return 0;
                }
            }
        }

        /// <summary>
        /// Arcane, Frozen, Jailer, Molten, Electrified+Reflect Damage elites
        /// </summary>
        public static bool HardElitesPresent
        {
            get
            {
                return
                   Trinity.ObjectCache.Any(o =>
                          o.MonsterAffixes.HasFlag(MonsterAffixes.ArcaneEnchanted | MonsterAffixes.Frozen | MonsterAffixes.Jailer | MonsterAffixes.Molten) ||
                          (o.MonsterAffixes.HasFlag(MonsterAffixes.Electrified) && o.MonsterAffixes.HasFlag(MonsterAffixes.ReflectsDamage))) ||
                        Trinity.ObjectCache.Any(o => o.IsBoss);
            }
        }

        public static TrinitySetting Settings
        {
            get { return Trinity.Settings; }
        }

        public static bool UseOOCBuff
        {
            get
            {
                if (CurrentTarget == null)
                    return true;
                else
                    return false;
            }
        }

        public static bool IsCurrentlyAvoiding
        {
            get
            {
                if (CurrentTarget.Type == GObjectType.Avoidance)
                    return true;
                else
                    return false;
            }
        }

        public static bool UseDestructiblePower
        {
            get
            {
                switch (CurrentTarget.Type)
                {
                    case GObjectType.Destructible:
                    case GObjectType.Barricade:
                        return true;
                    default:
                        return false;
                }
            }
        }

        public static Vector3 LastZigZagLocation
        {
            get { return lastZigZagLocation; }
            set { lastZigZagLocation = value; }
        }

        public static TrinityPower CurrentPower
        {
            get { return currentPower; }
            set { currentPower = value; }
        }

        public static HashSet<SNOPower> Hotbar
        {
            get
            {
                return Trinity.Hotbar;
            }
        }
        public static PlayerInfoCache Player
        {
            get
            {
                return Trinity.Player;
            }
        }

        public static TrinityCacheObject CurrentTarget
        {
            get
            {
                return Trinity.CurrentTarget;
            }
        }
        public static TrinityPower DefaultPower
        {
            get
            {
                // Default attacks
                if (!UseOOCBuff && !IsCurrentlyAvoiding)
                {
                    if (Trinity.Player.ActorClass == ActorClass.Monk && Hotbar.Contains(SNOPower.Monk_SweepingWind))
                    {
                        Trinity.Monk_TickSweepingWindSpam();
                    }

                    return new TrinityPower()
                    {
                        SNOPower = GetDefaultWeaponPower,
                        MinimumRange = GetDefaultWeaponDistance,
                        TargetRActorGUID = CurrentTarget.ACDGuid,
                        WaitForAnimationFinished = true
                    };
                }
                return new TrinityPower();
            }
        }

        /// <summary>
        /// Gets the default weapon power based on the current equipped primary weapon
        /// </summary>
        /// <returns></returns>
        public static SNOPower GetDefaultWeaponPower
        {
            get
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
        }
        /// <summary>
        /// Gets the default weapon distance based on the current equipped primary weapon
        /// </summary>
        /// <returns></returns>
        public static float GetDefaultWeaponDistance
        {
            get
            {
                switch (GetDefaultWeaponPower)
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
        }

        /// <summary>
        /// Performs basic checks to see if we have and can cast a power (hotbar, use timer, power manager)
        /// </summary>
        /// <param name="power"></param>
        /// <returns></returns>
        public static bool CanCast(SNOPower power, CanCastFlags flags = CanCastFlags.All)
        {
            return Hotbar.Contains(power) && 
                flags.HasFlag(CanCastFlags.NoTimer) ? SNOPowerUseTimer(power) : true &&
                flags.HasFlag(CanCastFlags.NoPowerManager) ? PowerManager.CanCast(power) : true;
        }

        /// <summary>
        /// Check if a particular buff is present
        /// </summary>
        /// <param name="power"></param>
        /// <returns></returns>
        public static bool GetHasBuff(SNOPower power)
        {
            int id = (int)power;
            return Trinity.listCachedBuffs.Any(u => u.SNOId == id);
        }

        /// <summary>
        /// Returns how many stacks of a particular buff there are
        /// </summary>
        /// <param name="power"></param>
        /// <returns></returns>
        public static int GetBuffStacks(SNOPower power)
        {
            int stacks;
            if (Trinity.dictCachedBuffs.TryGetValue((int)power, out stacks))
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
            if (TimeSinceUse(power) >= DataDictionary.AbilityRepeatDelays[power])
                return true;
            if (recheck && TimeSinceUse(power) >= 150 && TimeSinceUse(power) <= 600)
                return true;
            return false;
        }

        /// <summary>
        /// Returns true if we have the ability and the buff is up, or true if we don't have the ability in our hotbar
        /// </summary>
        /// <param name="snoPower"></param>
        /// <returns></returns>
        internal static bool CheckAbilityAndBuff(SNOPower snoPower)
        {
            return
                (!Trinity.Hotbar.Contains(snoPower) || (Trinity.Hotbar.Contains(snoPower) && GetHasBuff(snoPower)));

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

        /// <summary>
        /// Check if a power is null
        /// </summary>
        /// <param name="power"></param>
        protected static bool IsNull(TrinityPower power)
        {
            return power.Equals(null);
        }
    }
}
