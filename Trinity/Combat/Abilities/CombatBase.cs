using System;
using System.Collections.Generic;
using System.Linq;
using Trinity.Config;
using Trinity.Config.Combat;
using Trinity.Objects;
using Trinity.Reference;
using Trinity.Technicals;
using Zeta.Bot;
using Zeta.Bot.Navigation;
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
            GameEvents.OnWorldChanged += (sender, args) => LoadCombatSettings();
            Pulsator.OnPulse += (sender, args) => MonkCombat.RunOngoingPowers();
            LoadCombatSettings();
		}
        
        private static TrinityPower _currentPower = new TrinityPower();
        private static Vector3 _lastZigZagLocation = Vector3.Zero;
        private static Vector3 _zigZagPosition = Vector3.Zero;
        private static bool _isCombatAllowed = true;
        private static KiteMode _kiteMode = KiteMode.Never;

        public static QueuedMovementManager QueuedMovement = new QueuedMovementManager();
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

        internal static bool Cast(TrinityPower power)
        {
            if (IsNull(power))
                return false;

            if (power.SNOPower == SNOPower.Walk && power.TargetPosition == Vector3.Zero)
            {
                Navigator.PlayerMover.MoveStop();
                return true;
            }

            if (power.SNOPower == SNOPower.Walk)
                return false;

            if (ZetaDia.Me.UsePower(power.SNOPower, power.TargetPosition, power.TargetDynamicWorldId, power.TargetACDGUID))
            {
                SpellTracker.TrackSpellOnUnit(power.TargetACDGUID, power.SNOPower);
                SpellHistory.RecordSpell(power);

                Trinity.lastGlobalCooldownUse = DateTime.UtcNow;
                Trinity.IsWaitingAfterPower = power.ShouldWaitAfterUse;

                Vector3 target = power.TargetPosition != Vector3.Zero ? power.TargetPosition : CurrentTarget != null && power.TargetACDGUID != -1 ? CurrentTarget.Position : Vector3.Zero;
                float distance = target != Vector3.Zero ? target.Distance2D(Player.Position) : 0f;

                string info = CurrentTarget != null ? " TargetType=" + CurrentTarget.Type : "";
                info += target != Vector3.Zero ? " at " + NavHelper.PrettyPrintVector3(target) : "";
                info += power.TargetACDGUID != -1 ? " on " + power.TargetACDGUID : "";
                info += (int)distance > 0 ? " Dist=" + (int)distance : "";
                info += " Range=" + power.MinimumRange;

                Skill skill = SkillUtils.ById(power.SNOPower);
                if (skill != null)
                {
                    if (skill.IsSpender)
                    {
                        LastSpenderUseTime = DateTime.UtcNow;
                        info += " Cost=" + skill.Cost;
                    }

                    else if (skill.IsAttackGenerator)
                    {
                        LastGeneratorUseTime = DateTime.UtcNow;
                        info += " IsAttackGenerator";
                    }

                    if (skill.Charges > 0)
                        info += " Charges=" + skill.Charges;
                }

                info += String.Format(" TimeSincePrimaryUse={0} TimeSinceSpendSkillUse={1}", 
                    DateTime.UtcNow.Subtract(LastGeneratorUseTime).TotalMilliseconds, 
                    DateTime.UtcNow.Subtract(LastSpenderUseTime).TotalMilliseconds);

                Logger.Log(TrinityLogLevel.Verbose, LogCategory.Targetting, "Used Power {0}" + info, power.SNOPower);

                return true;
            }

            return false;
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
                    HealthGlobeResource = Settings.Combat.Crusader.HealthGlobeLevelResource;
                    KiteDistance = 0;
                    KiteMode = KiteMode.Never;
                    break;

                case ActorClass.Monk:
                    EmergencyHealthPotionLimit = Settings.Combat.Monk.PotionLevel;
                    EmergencyHealthGlobeLimit = Settings.Combat.Monk.HealthGlobeLevel;
                    HealthGlobeResource = Settings.Combat.Monk.HealthGlobeLevelResource;
                    KiteDistance = 0;
                    KiteMode = KiteMode.Never;
                    break;

                case ActorClass.Wizard:
                    EmergencyHealthPotionLimit = Settings.Combat.Wizard.PotionLevel;
                    EmergencyHealthGlobeLimit = Settings.Combat.Wizard.HealthGlobeLevel;
                    HealthGlobeResource = Settings.Combat.Wizard.HealthGlobeLevelResource;
                    KiteDistance = Settings.Combat.Wizard.KiteLimit;
                    KiteMode = KiteMode.Always;
                    break;

                case ActorClass.Witchdoctor:
                    EmergencyHealthPotionLimit = Settings.Combat.WitchDoctor.PotionLevel;
                    EmergencyHealthGlobeLimit = Settings.Combat.WitchDoctor.HealthGlobeLevel;
                    HealthGlobeResource = Settings.Combat.WitchDoctor.HealthGlobeLevelResource;
                    KiteDistance = Settings.Combat.WitchDoctor.KiteLimit;
                    KiteMode = KiteMode.Always;
                    break;

                case ActorClass.DemonHunter:
                    EmergencyHealthPotionLimit = Settings.Combat.DemonHunter.PotionLevel;
                    EmergencyHealthGlobeLimit = Settings.Combat.DemonHunter.HealthGlobeLevel;
                    HealthGlobeResource = Settings.Combat.DemonHunter.HealthGlobeLevelResource;
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

        internal static bool SwitchToTarget(TrinityCacheObject _target)
        {
            if (_target == null || _target == default(TrinityCacheObject))
                return false;

            // Change target
            if (_target.RActorGuid != CurrentTarget.RActorGuid)
                Trinity.CurrentTarget = _target;

            if (CurrentTarget != null)
            {
                if (CurrentTarget.IsUnit)
                    Trinity.lastHadUnitInSights = DateTime.UtcNow;

                if (CurrentTarget.IsBossOrEliteRareUnique)
                    Trinity.lastHadEliteUnitInSights = DateTime.UtcNow;

                if (CurrentTarget.IsBoss || CurrentTarget.IsBountyObjective)
                    Trinity.lastHadBossUnitInSights = DateTime.UtcNow;

                if (CurrentTarget.Type == GObjectType.Container)
                    Trinity.lastHadContainerInSights = DateTime.UtcNow;

                // Record the last time our target changed
                if (Trinity.LastTargetRactorGUID != CurrentTarget.RActorGuid)
                {
                    Trinity.RecordTargetHistory();

                    Logger.Log(TrinityLogLevel.Info, LogCategory.Weight,
                        "Found New Target {0} dist={1:0} IsElite={2} Radius={3:0.0} Weight={4:0} ActorSNO={5} " +
                        "Anim={6} TargetedCount={7} Type={8} ",
                        CurrentTarget.InternalName,
                        CurrentTarget.Distance,
                        CurrentTarget.IsEliteRareUnique,
                        CurrentTarget.Radius,
                        CurrentTarget.Weight,
                        CurrentTarget.ActorSNO,
                        CurrentTarget.Animation,
                        CurrentTarget.TimesBeenPrimaryTarget,
                        CurrentTarget.Type
                        );

                    Trinity.LastPickedTargetTime = DateTime.UtcNow;
                    Trinity.TargetLastHealth = 0f;
                    Trinity.LastTargetRactorGUID = CurrentTarget.RActorGuid;

                    return true;
                }

                // We're sticking to the same target, so update the target's health cache to check for stucks
                if (CurrentTarget.IsUnit)
                {
                    // Check if the health has changed, if so update the target-pick time before we blacklist them again
                    if (CurrentTarget.HitPointsPct != Trinity.TargetLastHealth)
                    {
                        Logger.Log(TrinityLogLevel.Info, LogCategory.Weight, "Keeping Target {0} - CurrentTarget.HitPoints: {1:0.00} TargetLastHealth: {2:0.00} ",
                                        CurrentTarget.RActorGuid, CurrentTarget.HitPointsPct, Trinity.TargetLastHealth);
                        Trinity.LastPickedTargetTime = DateTime.UtcNow;
                    }
                    // Now store the target's last-known health
                    Trinity.TargetLastHealth = CurrentTarget.HitPointsPct;
                }
            }

            return false;
        }

        internal static void RefreshValues()
        {
            TargetUtil.ResetTickValues();
            DemonHunterCombat.ResetArea();

            PlayerIsSurrounded = Trinity.ObjectCache != null && Trinity.ObjectCache.Count(o => o.IsUnit && o.Weight > 0 && o.RadiusDistance < 16f) > 4;
            PlayerShouldNotFight = Player.StandingInAvoidance || Player.AvoidDeath || Player.NeedToKite;
            PlayerIsImmune = false;// GetHasBuff(SNOPower.Witchdoctor_SpiritWalk) || GetHasBuff(SNOPower.DemonHunter_SmokeScreen);
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

        public static bool PlayerShouldNotFight { get; set; }
        public static bool PlayerIsSurrounded { get; set; }
        public static bool PlayerIsImmune { get; set; }

        public static float EmergencyHealthPotionLimit { get; set; }
        public static float EmergencyHealthGlobeLimit { get; set; }
        public static float HealthGlobeResource { get; set; }

        public static float LastPowerRange = 0f;

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

        public static DateTime LastSpenderUseTime = DateTime.MinValue;
        public static DateTime LastGeneratorUseTime = DateTime.MinValue;

        /// <summary>
        /// Determines whether [is ZeisOfStone equipped].
        /// </summary>
        public static bool IsBaneOfTrappedEquipped
        {
            get { return CacheData.Inventory.EquippedIds.Contains(405781); }
        }

        /// <summary>
        /// Determines whether [is ZeisOfStone equipped].
        /// </summary>
        public static bool IsZeisOfStoneEquipped
        {
            get { return CacheData.Inventory.EquippedIds.Contains(405801); }
        }

        /// <summary>
        /// Determines whether [is taeguk equipped].
        /// </summary>
        public static bool IsTaegukEquipped
        {
            get { return CacheData.Inventory.EquippedIds.Contains(405804); }
        }

        /// <summary>
        /// Retrun sets equipped and time sup 2500
        /// </summary>
        public static bool IsTaegukBuffWillExpire
        {
            get { return IsTaegukEquipped && DateTime.UtcNow.Subtract(LastSpenderUseTime).TotalMilliseconds >= 2250; }
        }

        /// <summary>
        /// Retrun sets equipped and time sup 4500
        /// </summary>
        public static bool IsBastionsPrimaryBuffWillExpired
        {
            get
            {
                if (!Sets.BastionsOfWill.IsFullyEquipped)
                    return false;

                var stacks = Legendary.Restraint.BuffStacks;
                var timeSinceGenerator = DateTime.UtcNow.Subtract(LastGeneratorUseTime).TotalMilliseconds;

                return stacks == 0 || timeSinceGenerator >= 4500;
            }
        }

        /// <summary>
        /// Retrun sets equipped and time sup 4500
        /// </summary>
        public static bool IsBastionsSpendingBuffWillExpired
        {
            get
            {
                if (!Sets.BastionsOfWill.IsFullyEquipped)
                    return false;

                var stacks = Legendary.Restraint.BuffStacks;
                var timeSinceSpender = DateTime.UtcNow.Subtract(LastSpenderUseTime).TotalMilliseconds;

                return stacks == 0 || timeSinceSpender >= 4500;
            }
        }

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
                        if (Player.ActorClass == ActorClass.DemonHunter)
                            return Trinity.Settings.Combat.DemonHunter.RangedAttackRange;
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
        /// Returns how many stacks of a particular skill there are
        /// </summary>
        /// <param name="power"></param>
        /// <returns></returns>
        public static int GetSkillCharges(SNOPower power)
        {
            return CacheData.Hotbar.GetSkillCharges(power);
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

        internal static double TimeSincePrimaryUse
        {
            get { return DateTime.UtcNow.Subtract(LastGeneratorUseTime).TotalMilliseconds;  }
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
        public static bool IsNull(TrinityPower power)
        {
            return power == null || power.SNOPower == SNOPower.None;
        }
    }
}
