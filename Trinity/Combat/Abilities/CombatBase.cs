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
using Zeta.Game.Internals.SNO;
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
        private static bool _isCombatAllowed = true;
        private static KiteMode _kiteMode = KiteMode.Never;

        public static CombatMovementManager CombatMovement = new CombatMovementManager();
        internal static Vector3 PositionLastZigZagCheck { get; set; }

        public enum CanCastFlags
        {
            All = 2,
            NoTimer = 4,
            NoPowerManager = 8,
            Timer = 16
        }

        internal virtual void CombatSettings() { }

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
                    EnergyReserve = Sets.EmbodimentOfTheMarauder.IsFullyEquipped ? 20 : 25;
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
        /// EnergReserve for using "Big" spells/powers        
        /// </summary>        
        [Obsolete("Use EnergyReserve, Set in Class's CombatSettings() override")]
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
        /// Minimum energy reserve for using "Big" spells/powers     
        /// </summary>
        public static int EnergyReserve { get; set; }
        

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

                if (CurrentTarget.Type == TrinityObjectType.Avoidance)
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
                    case TrinityObjectType.Destructible:
                    case TrinityObjectType.Barricade:
                        return true;
                    default:
                        return false;
                }
            }
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
        internal static bool ShouldWaitForConventionElement(Skill skill)
        {
            if (!Settings.Combat.Misc.UseConventionElementOnly)
                return false;

            return Legendary.ConventionOfElements.IsEquipped && CacheData.Buffs.ConventionElement != skill.Element;
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

                // Some Generators take a while to actually hit something (chakram for example)
                return SpellHistory.TimeSinceGeneratorCast >= 4000;
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

                return  SpellHistory.TimeSinceSpenderCast >= 4750;
            }
        }

        /// <summary>
        /// Select an attacking skill that is primary or a generator
        /// </summary>
        /// <returns></returns>
        internal static TrinityPower GetAttackGenerator()
        {
            return FindSkill("Generator", s => s.IsGeneratorOrPrimary && s.CanCast());
        }

        /// <summary>
        /// Select an attacking skill that spends resource
        /// </summary>
        internal static TrinityPower GetAttackSpender()
        {
            return FindSkill("Spender", s => s.IsAttackSpender && s.CanCast());
        }

        /// <summary>
        /// Select a skill for breaking urns and wooden carts etc.
        /// </summary>
        internal static TrinityPower GetDestructablesSkill()
        {
            return FindSkill("Destructable", s => (s.Meta.IsDestructableSkill || s.IsGeneratorOrPrimary) && s.CanCast());
        }

        /// <summary>
        /// Select a skill for moving places fast
        /// </summary>
        internal static TrinityPower GetMovementSkill()
        {
            return FindSkill("Movement", s => (s.Meta.IsMovementSkill) && s.CanCast());
        }

        /// <summary>
        /// Select a skill for noping the hell out of there, includes skills that provide immunity etc.
        /// </summary>
        internal static TrinityPower GetAvoidanceSkill()
        {
            return FindSkill("Avoidance", s => (s.Meta.IsAvoidanceSkill || s.Meta.IsMovementSkill) && s.CanCast());
        }

        /// <summary>
        /// Select a skill that is a buff
        /// </summary>
        internal static TrinityPower GetBuffSkill()
        {
            return FindSkill("Buff", s => s.Meta.IsBuffingSkill && s.CanCast());
        }

        /// <summary>
        /// Gets the best power for combat
        /// </summary>
        /// <returns></returns>
        public static TrinityPower GetCombatPower(List<Skill> skills)
        {
            return FindSkill("Combat", s => s.CanCast(), skills);
        }

        /// <summary>
        /// Searches for a skill matching some criteria
        /// </summary>
        /// <param name="typeName">name for the type of skill, used in logging</param>
        /// <param name="condition">condition to be applied to skill list FirstOrDefault lamda</param>
        /// <param name="skillCollection">colleciton of skills to search against, defaults to all Active skills</param>
        /// <returns>a TrinityPower</returns>
        internal static TrinityPower FindSkill(string typeName, Func<Skill,bool> condition, List<Skill> skillCollection = null)
        {
            Logger.Log(TrinityLogLevel.Verbose, LogCategory.SkillSelection, "Finding {0} Skill", typeName);

            skillCollection = skillCollection ?? SkillUtils.Active;

            if (condition == null)
                return null;

            var skill = skillCollection.FirstOrDefault(condition);
            if (skill == null)
                return null;

            var power = GetTrinityPower(skill);

            Logger.Log(TrinityLogLevel.Verbose, LogCategory.SkillSelection, "   >>   Selected {0} Skill: {1} ({2}) Target={3}",
                typeName, power.SNOPower, (int)power.SNOPower, (CurrentTarget == null) ? "None" : CurrentTarget.InternalName);

            return power;
        }

        /// <summary>
        /// Checks if a skill can and should be cast.
        /// </summary>
        /// <param name="setting">combat data to use</param>
        public static bool CanCast(SkillMeta setting)
        {
            return setting.Skill != null && CanCast(setting.Skill);
        }

        /// <summary>
        /// Checks if a skill can and should be cast.
        /// </summary>
        /// <param name="skill">the Skill to check</param>
        /// <param name="condition">function to test against</param>
        //public static bool CanCast(Skill skill, Func<SkillMeta, bool> condition)
        //{
        //    return CanCast(skill, null, condition);
        //}

        /// <summary>
        /// Checks if a skill can and should be cast.
        /// </summary>
        /// <param name="skill">the Skill to check</param>
        /// <param name="changes">action to modify existing skill data</param>
        //public static bool CanCast(Skill skill, Action<SkillMeta> changes)
        //{
        //    return CanCast(skill, null, c => { changes(c); return true; });
        //}

        /// <summary>
        /// Checks if a skill can and should be cast.
        /// </summary>
        /// <param name="skill">the Skill to check</param>
        /// <param name="cd">Optional combat data to use</param>
        /// <param name="adhocCondition">Optional function to test against</param>
        public static bool CanCast(Skill skill, SkillMeta sm = null)
        {
            try
            {
                var meta = (sm != null) ? skill.Meta.Apply(sm) : skill.Meta;

                Func<string> check = () =>
                {
                    if (!Hotbar.Contains(skill.SNOPower))
                        return "NotOnHotbar";

                    if (Player.IsIncapacitated)
                        return "IsIncapacitated";

                    //var adhocConditionResult = (adhocCondition == null) || adhocCondition(meta);
                    var metaConditionResult = (meta.CastCondition == null) || meta.CastCondition(meta);

                    if (!meta.CastFlags.HasFlag(CanCastFlags.NoTimer) && !SNOPowerUseTimer(skill.SNOPower))
                        return "PowerUseTimer";

                    if (!meta.CastFlags.HasFlag(CanCastFlags.NoPowerManager) && !PowerManager.CanCast(skill.SNOPower))
                        return "PowerManager";

                    // Note: ZetaDia.Me.IsInCombat is unrealiable and only kicks in after an ability has hit a monster
                    if (meta.IsCombatOnly && CurrentTarget == null)
                        return "IsInCombat";

                    // This is already checked above...?
                    //if (meta.ReUseDelay > 0 && TimeSincePowerUse(skill.SNOPower) < meta.ReUseDelay)
                    //    return "ReUseDelay";

                    //if (meta.IsEliteOnly && Enemies.Nearby.EliteCount == 0)
                    //    return false;

                    //if (meta.MaxTargetDistance > CurrentTarget.Distance)
                    //    return false;

                    var resourceCost = (meta.RequiredResource > 0) ? meta.RequiredResource : skill.Cost;
                    if (resourceCost > 0 && !skill.IsGeneratorOrPrimary)
                    {
                        var actualResource = (skill.Resource == Resource.Discipline) ? Player.SecondaryResource : Player.PrimaryResource;
                        if (actualResource < resourceCost)
                            return string.Format("NotEnoughResource({0}/{1})", Math.Round(actualResource), resourceCost);
                    }

                    //if (meta.IsEliteOnly && !CurrentTarget.IsBossOrEliteRareUnique)
                    //    return false;

                    //if (!adhocConditionResult)
                    //    return "AdHocConditionFailure";

                    if (!metaConditionResult)
                        return "ConditionFailure";

                    return string.Empty;
                };

                var failReason = check();

                if (!string.IsNullOrEmpty(failReason))
                {
                    Logger.Log(TrinityLogLevel.Verbose, LogCategory.SkillSelection, "   >>   CanCast Failed: {0} ({1}) Reason={2}",
                        skill.Name, (int)skill.SNOPower, failReason);
                    
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                Logger.Log("Exception in CanCast for {0}. {1} {2}", skill.Name, ex.Message, ex.InnerException);
            }

            return false;
        }

        /// <summary>
        /// If the players build currently has no primary/generator ability
        /// </summary>
        public static bool HasNoPrimary
        {
            get { return CacheData.Hotbar.ActiveSkills.All(s => s.Skill.IsGeneratorOrPrimary); }
        }

        /// <summary>
        /// Some sugar for Null/Invalid checking on given power;
        /// </summary>
        public static bool TryGetPower(TrinityPower powerToCheck, out TrinityPower power)
        {
            power = powerToCheck;
            return (power != null && power.SNOPower != SNOPower.None && power != DefaultPower);
        }

        /// <summary>
        /// Converts a skill into a TrinityPower for casting
        /// </summary>
        /// <returns></returns>
        public static TrinityPower GetTrinityPower(Skill skill)
        {
            var ticksBefore = skill.Meta.BeforeUseDelay == 0 ? 0 : (int)Math.Round(BotMain.TicksPerSecond * (skill.Meta.BeforeUseDelay / 1000));
            var ticksAfter = skill.Meta.AfterUseDelay == 0 ? 0 : (int)Math.Round(BotMain.TicksPerSecond * (skill.Meta.AfterUseDelay / 1000));

            if (skill.Meta.IsCastOnSelf)
            {
                Logger.Log(LogCategory.Targetting, "Calculating TargetPosition for {0} as Self. CurrentTarget={1}", skill.Name, CurrentTarget != null ? CurrentTarget.InternalName : "");
                return skill.ToPower(ticksBefore, ticksAfter);
            }

            var castRange = (skill.Meta.CastRange <= 0) ? (int)Math.Round(skill.Meta.MaxTargetDistance * 0.5) : skill.Meta.CastRange;

            if (skill.Meta.TargetPositionSelector != null)
            {                
                var targetPosition = skill.Meta.TargetPositionSelector(skill.Meta);

                Logger.Log(LogCategory.Targetting, "Calculating TargetPosition for {0} using TargetPositionSelector at {1} Dist={2} PlayerIsFacing(CastPosition={3} CurrentTarget={4}) CurrentTarget={5}", 
                    skill.Name,
                    targetPosition, 
                    Player.Position.Distance(targetPosition), 
                    Player.IsFacing(targetPosition),
                    Player.IsFacing(CurrentTarget.Position),
                    CurrentTarget.InternalName
                    );

                return skill.ToPower(castRange, targetPosition, ticksBefore, ticksAfter);
            }

            if (skill.Meta.TargetUnitSelector != null)
            {
                var targetUnit = skill.Meta.TargetUnitSelector(skill.Meta);

                Logger.Log(LogCategory.Targetting, "Calculating TargetPosition for {0} using TargetUnitSelector at {1} Dist={2} PlayerIsFacing(CastPosition={3} CurrentTarget={4}) CurrentTarget={5}",
                    skill.Name,
                    targetUnit.Position,
                    Player.Position.Distance(targetUnit.Position),
                    Player.IsFacing(targetUnit.Position),
                    Player.IsFacing(CurrentTarget.Position),
                    CurrentTarget.InternalName
                    );

                return skill.ToPower(castRange, targetUnit.Position, targetUnit.ACDGuid, ticksBefore, ticksAfter);
            }


            if (skill.Meta.IsAreaEffectSkill)
            {
                var target = GetBestAreaEffectTarget(skill);

                Logger.Log(LogCategory.Targetting, "Calculating TargetPosition for {0} using AreaEffectTargetting at {1} Dist={2} PlayerIsFacing(CastPosition={3} CurrentTarget={4}) CurrentTarget={5} AreaShape={6} AreaRadius={7} ",
                    skill.Name,
                    target,
                    Player.Position.Distance(target.Position),
                    Player.IsFacing(target.Position),
                    Player.IsFacing(CurrentTarget.Position),
                    CurrentTarget.InternalName,
                    skill.Meta.AreaEffectShape,
                    skill.AreaEffectRadius
                    );

                return skill.ToPower(castRange, target.Position, target.ACDGuid, ticksBefore, ticksAfter);
            }

            return skill.ToPower(castRange, CurrentTarget.Position);
        }

        /// <summary>
        /// Finds a target location using skill metadata.
        /// </summary>
        /// <param name="skill">skill to be used</param>
        /// <returns>target position</returns>
        public static TrinityCacheObject GetBestAreaEffectTarget(Skill skill)
        {
            // Avoid bot choosing a target that is too far away (and potentially running towards it) when there is danger close by.
            var searchRange = (float)(skill.IsGeneratorOrPrimary && Enemies.CloseNearby.Units.Any() ? skill.Meta.CastRange * 0.5 : skill.Meta.CastRange);

            TrinityCacheObject target;
            switch (skill.Meta.AreaEffectShape)
            {
                case AreaEffectShapeType.Beam:
                    target = TargetUtil.GetBestPierceTarget(searchRange);
                    break;
                case AreaEffectShapeType.Circle:
                    target = TargetUtil.GetBestClusterUnit(skill.AreaEffectRadius, searchRange);
                    break;
                case AreaEffectShapeType.Cone:
                    target = TargetUtil.GetBestArcTarget(searchRange, skill.AreaEffectRadius);
                    break;
                default:
                    target = TargetUtil.GetBestClusterUnit(skill.AreaEffectRadius, searchRange);
                    break;
            }

            return target ?? CurrentTarget;
        }

        public static bool IsInCombat
        {
            get { return CurrentTarget != null && CurrentTarget.ActorType == ActorType.Monster || ZetaDia.Me.IsInCombat; }
        }
        
    }

}
