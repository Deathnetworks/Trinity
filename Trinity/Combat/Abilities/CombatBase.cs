using System;
using System.Collections.Generic;
using System.Linq;
using Trinity.Config;
using Trinity.Config.Combat;
using Trinity.Objects;
using Trinity.Reference;
using Trinity.Technicals;
using Zeta.Bot;
using Zeta.Common;
using Zeta.Game;
using Zeta.Game.Internals.Actors;
using Logger = Trinity.Technicals.Logger;

namespace Trinity.Combat.Abilities
{
    public class CombatBase
    {
        static CombatBase()
		{
            GameEvents.OnGameJoined += (sender, args) => LoadCombatSettings();
            LoadCombatSettings();
		}
        
        private static TrinityPower _currentPower = new TrinityPower();
        private static Vector3 _lastZigZagLocation = Vector3.Zero;
        private static Vector3 _zigZagPosition = Vector3.Zero;
        private static bool _isCombatAllowed = true;
        private static KiteMode _kiteMode = KiteMode.Never;

        public static CombatMovementManager CombatMovement = new CombatMovementManager();
        internal static DateTime LastChangedZigZag { get; set; }
        internal static Vector3 PositionLastZigZagCheck { get; set; }
        // Unique ID of mob last targetting when using whirlwind/strafe
        internal static int LastZigZagUnitAcdGuid { get; set; }

        public enum CanCastFlags
        {
            All = 2,
            NoTimer = 4,
            NoPowerManager = 8,
            Timer = 16
        }

        internal static void LoadCombatSettings()
        {
            switch (Player.ActorClass)
            {
                case ActorClass.Barbarian:
                    EmergencyHealthPotionLimit = Settings.Combat.Barbarian.PotionLevel;
                    EmergencyHealthGlobeLimit = Settings.Combat.Barbarian.HealthGlobeLevel;
                    HealthGlobeResource = Settings.Combat.Barbarian.HealthGlobeLevelResource;
                    KiteDistance = Settings.Combat.Barbarian.KiteLimit;
                    KiteMode = KiteMode.Never;
                    break;

                case ActorClass.Crusader:
                    EmergencyHealthPotionLimit = Settings.Combat.Crusader.PotionLevel;
                    EmergencyHealthGlobeLimit = Settings.Combat.Crusader.HealthGlobeLevel;
                    HealthGlobeResource = Settings.Combat.Barbarian.HealthGlobeLevelResource;
                    KiteDistance = 0;
                    KiteMode = KiteMode.Never;
                    break;

                case ActorClass.Monk:
                    EmergencyHealthPotionLimit = Settings.Combat.Monk.PotionLevel;
                    EmergencyHealthGlobeLimit = Settings.Combat.Monk.HealthGlobeLevel;
                    HealthGlobeResource = Settings.Combat.Barbarian.HealthGlobeLevelResource;
                    KiteDistance = 0;
                    KiteMode = KiteMode.Never;
                    break;

                case ActorClass.Wizard:
                    EmergencyHealthPotionLimit = Settings.Combat.Wizard.PotionLevel;
                    EmergencyHealthGlobeLimit = Settings.Combat.Wizard.HealthGlobeLevel;
                    HealthGlobeResource = Settings.Combat.Barbarian.HealthGlobeLevelResource;
                    KiteDistance = Settings.Combat.Wizard.KiteLimit;
                    KiteMode = KiteMode.Always;
                    break;

                case ActorClass.Witchdoctor:
                    EmergencyHealthPotionLimit = Settings.Combat.WitchDoctor.PotionLevel;
                    EmergencyHealthGlobeLimit = Settings.Combat.WitchDoctor.HealthGlobeLevel;
                    HealthGlobeResource = Settings.Combat.Barbarian.HealthGlobeLevelResource;
                    KiteDistance = Settings.Combat.WitchDoctor.KiteLimit;
                    KiteMode = KiteMode.Always;
                    break;

                case ActorClass.DemonHunter:
                    EmergencyHealthPotionLimit = Settings.Combat.DemonHunter.PotionLevel;
                    EmergencyHealthGlobeLimit = Settings.Combat.DemonHunter.HealthGlobeLevel;
                    HealthGlobeResource = Settings.Combat.Barbarian.HealthGlobeLevelResource;
                    KiteDistance = Settings.Combat.DemonHunter.KiteLimit;
                    KiteMode = Settings.Combat.DemonHunter.KiteMode;
                    break;
            }

            // Monk Seven Sided Strike: Sustained Attack
            if (Player.ActorClass == ActorClass.Monk && CacheData.Hotbar.ActiveSkills.Any(s => s.Power == SNOPower.Monk_SevenSidedStrike && s.RuneIndex == 3))
                SetSNOPowerUseDelay(SNOPower.Monk_SevenSidedStrike, 17000);

            if (Player.ActorClass == ActorClass.Witchdoctor && Passives.WitchDoctor.GraveInjustice.IsActive)
            {
                SetSNOPowerUseDelay(SNOPower.Witchdoctor_SoulHarvest, 1000);
                SetSNOPowerUseDelay(SNOPower.Witchdoctor_SpiritWalk, 1000);
                SetSNOPowerUseDelay(SNOPower.Witchdoctor_Horrify, 1000);
                SetSNOPowerUseDelay(SNOPower.Witchdoctor_Gargantuan, 20000);
                SetSNOPowerUseDelay(SNOPower.Witchdoctor_SummonZombieDog, 20000);
                SetSNOPowerUseDelay(SNOPower.Witchdoctor_GraspOfTheDead, 500);
                SetSNOPowerUseDelay(SNOPower.Witchdoctor_SpiritBarrage, 2000);
                SetSNOPowerUseDelay(SNOPower.Witchdoctor_Locust_Swarm, 2000);
                SetSNOPowerUseDelay(SNOPower.Witchdoctor_Haunt, 2000);
                SetSNOPowerUseDelay(SNOPower.Witchdoctor_Hex, 3000);
                SetSNOPowerUseDelay(SNOPower.Witchdoctor_MassConfusion, 15000);
                SetSNOPowerUseDelay(SNOPower.Witchdoctor_FetishArmy, 20000);
                SetSNOPowerUseDelay(SNOPower.Witchdoctor_BigBadVoodoo, 20000);
            }
            if (Player.ActorClass == ActorClass.Barbarian && CacheData.Hotbar.PassiveSkills.Contains(SNOPower.Barbarian_Passive_BoonOfBulKathos))
            {
                SetSNOPowerUseDelay(SNOPower.Barbarian_Earthquake, 90500);
                SetSNOPowerUseDelay(SNOPower.Barbarian_CallOfTheAncients, 90500);
                SetSNOPowerUseDelay(SNOPower.Barbarian_WrathOfTheBerserker, 90500);
            }
        }

        private static int _kiteDistance;
        /// <summary>
        /// Distance to kite, read settings (class independant)
        /// </summary>
        public static int KiteDistance
        {
            get
            {
                // Conduit Pylon buff is active, no kite distance
                if (CacheData.Buffs.HasConduitPylon)
                    return 0;

                return _kiteDistance;
            }
            set { _kiteDistance = value; }
        }

        public static float EmergencyHealthPotionLimit { get; set; }
        public static float EmergencyHealthGlobeLimit { get; set; }
        public static float HealthGlobeResource { get; set; }

        // When to Kite
        public static KiteMode KiteMode
        {
            get { return _kiteMode; }
            set { _kiteMode = value; }
        }

        /// <summary>
        /// Allows for completely disabling combat. Settable through API only. 
        /// </summary>
        public static bool IsCombatAllowed
        {
            get
            {
                // if disabled in the profile, or disabled through api
                if (!CombatTargeting.Instance.AllowedToKillMonsters)
                    return false;

                if (!_isCombatAllowed)
                    return false;
                return true;
            }
            set { _isCombatAllowed = value; }
        }

        public static bool IsQuestingMode { get; set; }

        /// <summary>
        /// The last "ZigZag" position, used with Barb Whirlwind, Monk Tempest Rush, etc.
        /// </summary>
        public static Vector3 ZigZagPosition
        {
            get { return _zigZagPosition; }
            internal set { _zigZagPosition = value; }
        }

        /// <summary>
        /// A dictionary containing the date time we last used a specific spell
        /// </summary>
        public static Dictionary<SNOPower, DateTime> AbilityLastUsedCache
        {
            get
            {
                return CacheData.AbilityLastUsed;
            }
            set
            {
                CacheData.AbilityLastUsed = value;
            }
        }

        /// <summary>
        /// Always contains the last power used
        /// </summary>
        public static SNOPower LastPowerUsed
        {
            get
            {
                return Trinity.LastPowerUsed;
            }
        }

        /// <summary>
        /// Gets/sets whether we are building up energy for a big spell
        /// </summary>
        public static bool IsWaitingForSpecial { get; set; }

        /// <summary>
        /// Minimum energy reserve for using "Big" spells/powers
        /// </summary>
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
                    case ActorClass.Witchdoctor:
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
                   Trinity.ObjectCache.Any(o => o.IsEliteRareUnique &&
                          o.MonsterAffixes.HasFlag(MonsterAffixes.ArcaneEnchanted | MonsterAffixes.Frozen | MonsterAffixes.Jailer | MonsterAffixes.Molten) ||
                          (o.MonsterAffixes.HasFlag(MonsterAffixes.Electrified) && o.MonsterAffixes.HasFlag(MonsterAffixes.ReflectsDamage))) ||
                        Trinity.ObjectCache.Any(o => o.IsBoss);
            }
        }

        public static bool IgnoringElites
        {
            get
            {
                return !IsQuestingMode && Settings.Combat.Misc.IgnoreElites;
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
                return false;
            }
        }

        public static bool IsCurrentlyAvoiding
        {
            get
            {
                if (CurrentTarget == null)
                    return false;

                if (CurrentTarget.Type == GObjectType.Avoidance)
                    return true;
                return false;
            }
        }

        public static bool UseDestructiblePower
        {
            get
            {
                if (CurrentTarget == null)
                    return false;

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
            get { return _lastZigZagLocation; }
            set { _lastZigZagLocation = value; }
        }

        public static TrinityPower CurrentPower
        {
            get { return _currentPower; }
            set { _currentPower = value; }
        }

        public static HashSet<SNOPower> Hotbar
        {
            get
            {
                return CacheData.Hotbar.ActivePowers;
            }
        }
        public static CacheData.PlayerCache Player
        {
            get
            {
                return CacheData.Player;
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
                        MonkCombat.RefreshSweepingWind();
                    }

                    return new TrinityPower
                    {
                        SNOPower = DefaultWeaponPower,
                        MinimumRange = DefaultWeaponDistance,
                        TargetACDGUID = CurrentTarget.ACDGuid,
                    };
                }
                return new TrinityPower();
            }
        }

        /// <summary>
        /// Gets the default weapon power based on the current equipped primary weapon
        /// </summary>
        /// <returns></returns>
        public static SNOPower DefaultWeaponPower
        {
            get
            {
                ACDItem lhItem = CacheData.Inventory.Equipped.FirstOrDefault(i => i.InventorySlot == InventorySlot.LeftHand);
                if (lhItem == null)
                    return SNOPower.None;

                switch (lhItem.ItemType)
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
                    case ItemType.MightyWeapon:
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
        public static float DefaultWeaponDistance
        {
            get
            {
                switch (DefaultWeaponPower)
                {
                    case SNOPower.Weapon_Ranged_Instant:
                    case SNOPower.Weapon_Ranged_Projectile:
                        return 65f;
                    case SNOPower.Weapon_Ranged_Wand:
                        return 55f;
                    default:
                        return 10f;
                }
            }
        }

        /// <summary>
        /// Performs basic checks to see if we have and can cast a power (hotbar, power manager). Checks use timer for Wiz, DH
        /// </summary>
        /// <param name="power"></param>
        /// <param name="flags"></param>
        /// <returns></returns>
        public static bool CanCast(SNOPower power, CanCastFlags flags = CanCastFlags.All)
        {
            bool hasPower = Hotbar.Contains(power);
            if (!hasPower)
                return false;

            // Skip this for Barb, Crusader, WD, Monk, except when specifically requested
            if (Player.ActorClass == ActorClass.Wizard || Player.ActorClass == ActorClass.DemonHunter || flags.HasFlag(CanCastFlags.Timer))
            {
                bool timer = flags.HasFlag(CanCastFlags.NoTimer) || SNOPowerUseTimer(power);

                if (!timer)
                    return false;
            }

            bool powerManager = flags.HasFlag(CanCastFlags.NoPowerManager) || PowerManager.CanCast(power);

            if (!powerManager)
                return false;

            return true;
        }

        /// <summary>
        /// Check if a particular buff is present
        /// </summary>
        /// <param name="power"></param>
        /// <returns></returns>
        public static bool GetHasBuff(SNOPower power)
        {
            return CacheData.Buffs.HasBuff(power);
        }

        /// <summary>
        /// Returns how many stacks of a particular buff there are
        /// </summary>
        /// <param name="power"></param>
        /// <returns></returns>
        public static int GetBuffStacks(SNOPower power)
        {
            return CacheData.Buffs.GetBuffStacks(power);
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
            if (TimeSincePowerUse(power) >= GetSNOPowerUseDelay(power))
                return true;
            if (recheck && TimeSincePowerUse(power) >= 150 && TimeSincePowerUse(power) <= 600)
                return true;
            return false;
        }

        public static void SetSNOPowerUseDelay(SNOPower power, double delay)
        {
            string key = "SpellDelay." + power.ToString();
            TVar v = V.Data[key];

            bool hasDefaultValue = v.Value == v.DefaultValue;

            if (hasDefaultValue)
            {
                // Create a new TVar (changes the default value)
                V.Set(new TVar(v.Name, delay, v.Description));
            }
        }

        public static double GetSNOPowerUseDelay(SNOPower power)
        {
            double delay = V.D("SpellDelay." + power);

            if (GetHasBuff(SNOPower.Pages_Buff_Infinite_Casting))
            {
                delay = delay * 0.25d;
            }

            return delay;
        }

        /// <summary>
        /// Returns true if we have the ability and the buff is up, or true if we don't have the ability in our hotbar
        /// </summary>
        /// <param name="snoPower"></param>
        /// <returns></returns>
        internal static bool CheckAbilityAndBuff(SNOPower snoPower)
        {
            return
                (!CacheData.Hotbar.ActivePowers.Contains(snoPower) || (CacheData.Hotbar.ActivePowers.Contains(snoPower) && GetHasBuff(snoPower)));
        }

        /// <summary>
        /// Returns how many stacks of a particular skill there are
        /// </summary>
        /// <param name="power"></param>
        /// <returns></returns>
        public static int GetSkillCharges(SNOPower power)
        {
            return CacheData.Hotbar.GetSkillCharges(power);
        }

        /// <summary>
        /// Gets the time in Millseconds since we've used the specified power
        /// </summary>
        /// <param name="power"></param>
        /// <returns></returns>
        internal static double TimeSincePowerUse(SNOPower power)
        {
            if (CacheData.AbilityLastUsed.ContainsKey(power))
                return DateTime.UtcNow.Subtract(CacheData.AbilityLastUsed[power]).TotalMilliseconds;
            return -1;
        }

        /// <summary>
        /// Gets the time in Millseconds since we've used the specified power
        /// </summary>
        /// <param name="power"></param>
        /// <returns></returns>
        internal static TimeSpan TimeSpanSincePowerUse(SNOPower power)
        {
            if (CacheData.AbilityLastUsed.ContainsKey(power))
                return DateTime.UtcNow.Subtract(CacheData.AbilityLastUsed[power]);
            return TimeSpan.MinValue;
        }

        /// <summary>
        /// Check if a power is null
        /// </summary>
        /// <param name="power"></param>
        protected static bool IsNull(TrinityPower power)
        {
            return power == null;
        }

        /// <summary>
        /// Checks a skill against the convention of elements ring
        /// </summary>
        internal static bool CheckConventionElement(Skill skill)
        {
            return !Settings.Combat.Misc.UseConventionElementOnly || !Legendary.ConventionOfElements.IsEquipped || CacheData.Buffs.ConventionElement == skill.Element;
        }

        /// <summary>
        /// If we should attack to aquire the Bastians generator buff
        /// </summary>
        internal static bool ShouldRefreshBastiansGeneratorBuff
        {
            get
            {
                if (Sets.BastionsOfWill.IsFullyEquipped && !CacheData.Buffs.HasBastiansWillGeneratorBuff)
                    return true;

                return SpellHistory.TimeSinceGeneratorCast >= 3500;
            }
        }

        /// <summary>
        /// If we should attack to aquire the Bastians spender buff
        /// </summary>
        internal static bool ShouldRefreshBastiansSpenderBuff
        {
            get
            {
                if (Sets.BastionsOfWill.IsFullyEquipped && !CacheData.Buffs.HasBastiansWillSpenderBuff)
                    return true;

                return  SpellHistory.TimeSinceSpenderCast >= 3500;
            }
        }

        /// <summary>
        /// Select an attacking skill that is primary or a generator
        /// </summary>
        /// <returns></returns>
        internal static TrinityPower CastAttackGenerator()
        {
            var generator = SkillUtils.Active.FirstOrDefault(s => s.IsGeneratorOrPrimary);
            if (generator == null)
                return DefaultPower;

            return generator.ToPower(50f, Enemies.BestCluster.Position);
        }

        /// <summary>
        /// Select an attacking skill that spends resource
        /// </summary>
        internal static TrinityPower CastAttackSpender()
        {
            var spender = SkillUtils.Active.FirstOrDefault(s => s.IsAttackSpender);
            if (spender == null)
                return DefaultPower;

            return spender.ToPower(80f, Enemies.BestCluster.Position);
        }

    }

}
