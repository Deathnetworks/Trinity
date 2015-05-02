using System;
using System.Collections.Generic;
using System.Linq;
using Trinity.Config.Combat;
using Trinity.Objects;
using Trinity.Reference;
using Trinity.Technicals;
using Zeta.Common;
using Zeta.Game.Internals.Actors;
using Logger = Trinity.Technicals.Logger;

namespace Trinity.Combat.Abilities
{
    class BarbarianCombat : CombatBase
    {
        private static bool _allowSprintOoc = true;
        private const float MaxFuriousChargeDistance = 300f;

        static BarbarianCombat()
        {
            SkillUtils.SetSkillMeta(SkillsDefaultMeta.Barbarian.ToList());
            SetConditions();
        }

        internal override void CombatSettings()
        {
            EmergencyHealthPotionLimit = Settings.Combat.Barbarian.PotionLevel;
            EmergencyHealthGlobeLimit = Settings.Combat.Barbarian.HealthGlobeLevel;
            HealthGlobeResource = Settings.Combat.Barbarian.HealthGlobeLevelResource;
            KiteDistance = Settings.Combat.Barbarian.KiteLimit;
            KiteMode = KiteMode.Never;
            EnergyReserve = 10;
        }

        /// <summary>
        /// Main method for selecting a power
        /// </summary>
        public static TrinityPower GetPowerNew()
        {
            TrinityPower power;

            if (UseDestructiblePower && TryGetPower(GetDestructablesSkill(), out power))
                return power;

            if (IsCurrentlyAvoiding && TryGetPower(GetAvoidanceSkill(), out power))
                return power;

            if (CurrentTarget == null)
            {
                // Out of Combat Buffs
                if (!IsCurrentlyAvoiding && !Player.IsInTown && TryGetPower(GetBuffSkill(), out power))
                    return power;
            }
            else
            {
                // Use Generator for the Bastians Ring Set buff
                if (ShouldRefreshBastiansGeneratorBuff && TryGetPower(GetAttackGenerator(), out power))
                    return power;

                // Use Spender for the Bastians Ring Set buff
                if (ShouldRefreshBastiansSpenderBuff && TryGetPower(GetAttackSpender(), out power))
                    return power;

                if (TryGetPower(GetCombatPower(SpecialSkills), out power))
                    return power;

                if (TryGetPower(GetCombatPower(EmergencySkills), out power))
                    return power;

                if (CanCast(Skills.Barbarian.Leap, LeapMightOfTheEarthCondition))
                    return Skills.Barbarian.Leap.ToPower();

                if (CanCast(Skills.Barbarian.Bash, BashPunishCondition))
                    return Skills.Barbarian.Bash.ToPower();

                if (CanCast(Skills.Barbarian.Frenzy, FrenzyManiacCondition))
                    return Skills.Barbarian.Frenzy.ToPower();

                if (TryGetPower(GetCombatPower(FurySpenders), out power))
                    return power;

                if (TryGetPower(GetCombatPower(FuryGenerators), out power))
                    return power;
            }

            Logger.Log(TrinityLogLevel.Verbose, LogCategory.SkillSelection, Player.ActorClass + " GetPower() Returning DefaultPower Target={0}",
                (CurrentTarget == null) ? "Null" : CurrentTarget.InternalName);

            return DefaultPower;
        }

        private static List<Skill> SpecialSkills
        {
            get
            {
                return new List<Skill>
                {
                    // Beast mode
                    Skills.Barbarian.CallOfTheAncients,
                    Skills.Barbarian.WrathOfTheBerserker,
                    Skills.Barbarian.Earthquake,
               };
            }
        }
        private static List<Skill> EmergencySkills
        {
            get
            {
                return new List<Skill>
                {
                    // Buffs
                    Skills.Barbarian.IgnorePain,
                    Skills.Barbarian.WarCry,
                    Skills.Barbarian.Sprint,
                    Skills.Barbarian.BattleRage,
                    Skills.Barbarian.ThreateningShout,
                };
            }
        }
        private static List<Skill> FurySpenders
        {
            get
            {
                return new List<Skill>
                {

                    // Spenders
                    Skills.Barbarian.Avalanche,
                    Skills.Barbarian.Revenge,
                    Skills.Barbarian.Overpower,
                    Skills.Barbarian.HammerOfTheAncients,
                    Skills.Barbarian.Rend,
                    Skills.Barbarian.SeismicSlam,
                    Skills.Barbarian.AncientSpear,
                    Skills.Barbarian.Whirlwind,

                };
            }
        }
        private static List<Skill> SpecialRuneSkills
        {
            get
            {
                return new List<Skill>
                {
                    Skills.Barbarian.Leap,
                    Skills.Barbarian.Bash,
                    Skills.Barbarian.Frenzy,
                };
            }
        }
        private static List<Skill> FuryGenerators
        {
            get
            {
                return new List<Skill>
                {
                    // Generators
                    Skills.Barbarian.FuriousCharge,
                    Skills.Barbarian.GroundStomp,
                    Skills.Barbarian.Leap,
                    Skills.Barbarian.WeaponThrow,
                    Skills.Barbarian.Bash,
                    Skills.Barbarian.Cleave,
                    Skills.Barbarian.Frenzy,
                };
            }
        }

        public static void SetConditions()
        {
            // Buffs
            Skills.Barbarian.IgnorePain.Meta.CastCondition = IgnorePainCondition;
            Skills.Barbarian.WarCry.Meta.CastCondition = WarCryCondition;
            Skills.Barbarian.Sprint.Meta.CastCondition = SprintCondition;
            Skills.Barbarian.BattleRage.Meta.CastCondition = BattleRageCondition;
            Skills.Barbarian.ThreateningShout.Meta.CastCondition = ThreateningShoutCondition;

            // Beast mode
            Skills.Barbarian.CallOfTheAncients.Meta.CastCondition = CallOfTheAncientsCondition;
            Skills.Barbarian.WrathOfTheBerserker.Meta.CastCondition = WrathOfTheBerserkerCondition;

            // Spenders
            Skills.Barbarian.Avalanche.Meta.CastCondition = AvalancheCondition;
            Skills.Barbarian.Earthquake.Meta.CastCondition = EarthquakeCondition;
            Skills.Barbarian.Revenge.Meta.CastCondition = RevengeCondition;
            Skills.Barbarian.Overpower.Meta.CastCondition = OverpowerCondition;
            Skills.Barbarian.HammerOfTheAncients.Meta.CastCondition = HammerOfTheAncientsCondition;
            Skills.Barbarian.Rend.Meta.CastCondition = RendCondition;
            Skills.Barbarian.SeismicSlam.Meta.CastCondition = SeismicSlamCondition;
            Skills.Barbarian.AncientSpear.Meta.CastCondition = AncientSpearCondition;
            Skills.Barbarian.Whirlwind.Meta.CastCondition = WhirlwindCondition;

            // Generators
            Skills.Barbarian.FuriousCharge.Meta.CastCondition = FuriousChargeCondition;
            Skills.Barbarian.GroundStomp.Meta.CastCondition = GroundStompCondition;
            Skills.Barbarian.Leap.Meta.CastCondition = LeapCondition;
            Skills.Barbarian.Bash.Meta.CastCondition = BashCondition;
            Skills.Barbarian.Cleave.Meta.CastCondition = CleaveCondition;
            Skills.Barbarian.Frenzy.Meta.CastCondition = FrenzyCondition;
        }

        public static TrinityPower GetPower()
        {
            TrinityPower power = null;

            if (UseDestructiblePower)
                power = DestroyObjectPower;

            if (UseOOCBuff)
            {
                // Call of The Ancients
                if (IsNull(power) && CanUseCallOfTheAncients && Sets.ImmortalKingsCall.IsFullyEquipped)
                    power = PowerCallOfTheAncients;

                // Sprint OOC
                if (IsNull(power) && CanUseSprintOOC)
                    power = PowerSprint;
            }


            if (!UseOOCBuff)
            {
                // Refresh Frenzy
                if (IsNull(power) && CanCast(SNOPower.Barbarian_Frenzy) && TimeSincePowerUse(SNOPower.Barbarian_Frenzy) > 3000 && TimeSincePowerUse(SNOPower.Barbarian_Frenzy) < 4000)
                    power = PowerFrenzy;
            }
            // Ignore Pain when low on health
            if (IsNull(power) && CanCastIgnorePain)
                power = PowerIgnorePain;

            // WOTB
            if (IsNull(power) && CanUseWrathOfTheBerserker)
                power = PowerWrathOfTheBerserker;

            // Call of the Ancients
            if (IsNull(power) && CanUseCallOfTheAncients)
                power = PowerCallOfTheAncients;

            // Leap with Earth Set.
            if (IsNull(power) && CanUseLeap && Sets.MightOfTheEarth.IsThirdBonusActive)
                power = PowerLeap;

            // Earthquake
            if (IsNull(power) && CanUseEarthquake)
                power = PowerEarthquake;

            // Avalanche
            if (IsNull(power) && CanUseAvalanche)
                power = PowerAvalanche;

            // War Cry
            if (IsNull(power) && CanUseWarCry)
                power = PowerWarCry;

            // Battle Rage
            if (IsNull(power) && CanUseBattleRage)
                power = PowerBattleRage;

            // Rend
            if (IsNull(power) && CanUseRend)
                power = PowerRend;

            // Overpower
            if (IsNull(power) && CanUseOverPower)
                power = PowerOverpower;

            // Threatening Shout
            if (IsNull(power) && CanUseThreatingShout)
                power = PowerThreateningShout;

            // Ground Stomp
            if (IsNull(power) && CanUseGroundStomp)
                power = PowerGroundStomp;

            // Revenge
            if (IsNull(power) && CanUseRevenge)
                power = PowerRevenge;

            // Ancient Spear
            if (IsNull(power) && CanUseAncientSpear)
                power = PowerAncientSpear;

            // Sprint
            if (IsNull(power) && CanUseSprint)
                power = PowerSprint;

            // Furious Charge
            if (IsNull(power) && CanUseFuriousCharge)
                power = PowerFuriousCharge;

            // Leap
            if (IsNull(power) && CanUseLeap)
                power = PowerLeap;

            // Seismic Slam
            if (IsNull(power) && CanUseSeismicSlam)
                power = PowerSeismicSlam;

            // Bash to 3 stacks (Punish)
            if (IsNull(power) && CanUseBashTo3)
                power = PowerBash;

            // Frenzy to 5 stacks (Maniac)
            if (IsNull(power) && CanUseFrenzyTo5)
                power = PowerFrenzy;

            // HOTA Elites
            if (IsNull(power) && CanUseHammerOfTheAncientsElitesOnly)
                power = PowerHammerOfTheAncients;

            // Whirlwind
            if (IsNull(power) && CanUseWhirlwind)
                power = PowerWhirlwind;

            // Hammer of the Ancients
            if (IsNull(power) && CanUseHammerOfTheAncients)
                power = PowerHammerOfTheAncients;

            // Weapon Throw
            if (IsNull(power) && CanUseWeaponThrow)
                power = PowerWeaponThrow;

            // Frenzy Fury Generator
            if (IsNull(power) && CanUseFrenzy)
                power = PowerFrenzy;

            // Bash Fury Generator
            if (IsNull(power) && CanUseBash)
                power = PowerBash;

            // Cleave Fury Generator
            if (IsNull(power) && CanUseCleave)
                power = PowerCleave;

            // Default Attacks
            if (IsNull(power))
                power = DefaultPower;

            return power;

        }

        private static bool CallOfTheAncientsCondition(SkillMeta meta)
        {
            if (GetHasBuff(SNOPower.Barbarian_CallOfTheAncients))
                return false;

            if (Sets.ImmortalKingsCall.IsFullyEquipped)
                return true;

            if (TargetUtil.AnyElitesInRange(V.F("Barbarian.CallOfTheAncients.MinEliteRange")))
                return true;
            return false;
        }
        private static bool WrathOfTheBerserkerCondition(SkillMeta meta)
        {
            /* WOTB should be used when the following conditions are met:
             * If ignoring elites, when 3 monsters in 25 yards or 10 monsters in 50 yards are present, OR
             * If using on hard elites only, when an elite with the required affix is present, OR
             * If normal mode, when any elite is within 20 yards, OR
             * If we have low health (use potion health)
             */

            if (GetHasBuff(SNOPower.Barbarian_WrathOfTheBerserker))
                return false;

            if (CacheData.Buffs.HasCastingShrine)
                return true;

            if (Settings.Combat.Barbarian.WOTBMode == BarbarianWOTBMode.WhenReady && !Player.IsInTown)
                return true;

            if (Player.CurrentHealthPct <= V.F("Barbarian.WOTB.EmergencyHealth"))
                return true;

            if (CurrentTarget.IsTreasureGoblin && Settings.Combat.Barbarian.UseWOTBGoblin)
                return true;

            if (CurrentTarget.IsBoss)
                return true;

            bool ignoreElites = IgnoringElites &&
                    (TargetUtil.AnyMobsInRange(V.F("Barbarian.WOTB.RangeNear"), V.I("Barbarian.WOTB.CountNear")) ||
                    TargetUtil.AnyMobsInRange(V.F("Barbarian.WOTB.RangeFar"), V.I("Barbarian.WOTB.CountFar")) ||
                    TargetUtil.AnyMobsInRange(Settings.Combat.Misc.TrashPackClusterRadius, Settings.Combat.Misc.TrashPackSize));
            if (ignoreElites)
                return true;

            bool hardElitesOnly = Settings.Combat.Barbarian.WOTBMode == BarbarianWOTBMode.HardElitesOnly;

            bool elitesPresent = TargetUtil.AnyElitesInRange(V.F("Barbarian.WOTB.MinRange"), V.I("Barbarian.WOTB.MinCount"));

            if (!hardElitesOnly && elitesPresent)
                return true;

            bool hardElitesPresent = Trinity.ObjectCache.Any(o => o.IsEliteRareUnique &&
                o.MonsterAffixes.HasFlag(MonsterAffixes.ArcaneEnchanted | MonsterAffixes.Frozen | MonsterAffixes.Jailer | MonsterAffixes.Molten) ||
                (o.MonsterAffixes.HasFlag(MonsterAffixes.Electrified) && o.MonsterAffixes.HasFlag(MonsterAffixes.ReflectsDamage)));

            return hardElitesPresent;
        }
        private static bool IgnorePainCondition(SkillMeta meta)
        {
            if (UseOOCBuff)
                return false;

            if (Settings.Combat.Barbarian.IgnorePainOffCooldown && CanCast(SNOPower.Barbarian_IgnorePain))
                return true;

            if (CanCast(SNOPower.Barbarian_IgnorePain) && Player.CurrentHealthPct <= V.F("Barbarian.IgnorePain.MinHealth"))
                return true;

            return Sets.TheLegacyOfRaekor.IsFullyEquipped && ShouldFuryDump;
        }
        private static bool AvalancheCondition(SkillMeta meta)
        {
            bool hasBerserker = CacheData.Hotbar.PassiveSkills.Any(p => p == SNOPower.Barbarian_Passive_BerserkerRage);
            double minFury = hasBerserker ? Player.PrimaryResourceMax * 0.99 : 0f;

            return !UseOOCBuff && !IsCurrentlyAvoiding && CanCast(SNOPower.X1_Barbarian_Avalanche_v2, CanCastFlags.NoTimer) &&
                Player.PrimaryResource >= minFury && (TargetUtil.AnyMobsInRange(3) || TargetUtil.IsEliteTargetInRange());
            return true;
        }
        private static bool EarthquakeCondition(SkillMeta meta)
        {
            double minFury = 50f;
            bool hasCaveIn = CacheData.Hotbar.ActiveSkills.Any(p => p.Power == SNOPower.Barbarian_Earthquake && p.RuneIndex == 4);
            float range = hasCaveIn ? 24f : 14f;

            return
                   !UseOOCBuff &&
                   !IsCurrentlyAvoiding &&
                   !Player.IsIncapacitated &&
                   CanCast(SNOPower.Barbarian_Earthquake) &&
                   Player.PrimaryResource >= minFury &&
                   (TargetUtil.IsEliteTargetInRange(range) || TargetUtil.AnyMobsInRange(range, 10));
            return true;
        }
        private static bool BattleRageCondition(SkillMeta meta)
        {
            return true;
        }
        private static bool SprintCondition(SkillMeta meta)
        {
            return true;
        }
        private static bool ThreateningShoutCondition(SkillMeta meta)
        {
            return true;
        }
        private static bool RevengeCondition(SkillMeta meta)
        {
            return true;
        }
        private static bool WarCryCondition(SkillMeta meta)
        {
            return true;
        }
        private static bool OverpowerCondition(SkillMeta meta)
        {
            return true;
        }
        private static bool FuriousChargeCondition(SkillMeta meta)
        {
            return true;
        }
        private static bool GroundStompCondition(SkillMeta meta)
        {
            return true;
        }
        private static bool HammerOfTheAncientsCondition(SkillMeta meta)
        {
            bool result =
             (Player.PrimaryResource >= V.F("Barbarian.HammerOfTheAncients.MinFury") || LastPowerUsed == SNOPower.Barbarian_HammerOfTheAncients) &&
             (!Hotbar.Contains(SNOPower.Barbarian_Whirlwind) || (Player.CurrentHealthPct >= Settings.Combat.Barbarian.MinHotaHealth && Hotbar.Contains(SNOPower.Barbarian_Whirlwind)));

            if (Legendary.LutSocks.IsEquipped)
            {
                return !CanUseLeap && result;
            }
            return result;
        }
        private static bool LeapCondition(SkillMeta meta)
        {
            return true;
        }
        private static bool LeapMightOfTheEarthCondition(SkillMeta meta)
        {
            return true;
        }
        private static bool RendCondition(SkillMeta meta)
        {
            return true;
        }
        private static bool SeismicSlamCondition(SkillMeta meta)
        {
            return true;
        }
        private static bool AncientSpearCondition(SkillMeta meta)
        {
            return !UseOOCBuff && !IsWaitingForSpecial && !IsCurrentlyAvoiding && CanCast(SNOPower.X1_Barbarian_AncientSpear) && Player.PrimaryResource >= 25 &&
                // Only boulder toss as a rage dump if we have excess resource
             (!Runes.Barbarian.BoulderToss.IsActive || Player.PrimaryResourcePct > 0.8) &&
             CurrentTarget.HitPointsPct >= V.F("Barbarian.AncientSpear.MinHealthPct");
            return true;
        }
        private static bool WhirlwindCondition(SkillMeta meta)
        {
            return true;
        }
        private static bool BashCondition(SkillMeta meta)
        {
            return true;
        }
        private static bool BashPunishCondition(SkillMeta meta)
        {
            return true;
        }
        private static bool CleaveCondition(SkillMeta meta)
        {
            return true;
        }
        private static bool FrenzyCondition(SkillMeta meta)
        {
            return true;
        }
        private static bool FrenzyManiacCondition(SkillMeta meta)
        {
            return true;
        }
        private static bool WeaponThrowCondition(SkillMeta meta)
        {
            return true;
        }


        public static bool CanCastIgnorePain
        {
            get
            {
                if (UseOOCBuff)
                    return false;

                if (Settings.Combat.Barbarian.IgnorePainOffCooldown && CanCast(SNOPower.Barbarian_IgnorePain))
                    return true;

                if (CanCast(SNOPower.Barbarian_IgnorePain) && Player.CurrentHealthPct <= V.F("Barbarian.IgnorePain.MinHealth"))
                    return true;

                return Sets.TheLegacyOfRaekor.IsFullyEquipped && ShouldFuryDump;
            }
        }
        public static bool CanUseCallOfTheAncients
        {
            get
            {
                return !UseOOCBuff &&
                    !IsCurrentlyAvoiding &&
                    CanCast(SNOPower.Barbarian_CallOfTheAncients) &&
                    !Player.IsIncapacitated &&
                    !GetHasBuff(SNOPower.Barbarian_CallOfTheAncients) && (Sets.ImmortalKingsCall.IsFullyEquipped ||
                    CurrentTarget.IsEliteRareUnique ||
                    TargetUtil.EliteOrTrashInRange(V.F("Barbarian.CallOfTheAncients.MinEliteRange")) ||
                    TargetUtil.AnyMobsInRange(V.F("Barbarian.CallOfTheAncients.MinEliteRange"), 3) ||
                    TargetUtil.AnyElitesInRange(V.F("Barbarian.CallOfTheAncients.MinEliteRange")));
            }
        }

        public static bool CanUseWrathOfTheBerserker
        {
            get
            {
                /* WOTB should be used when the following conditions are met:
                 * If ignoring elites, when 3 monsters in 25 yards or 10 monsters in 50 yards are present, OR
                 * If using on hard elites only, when an elite with the required affix is present, OR
                 * If normal mode, when any elite is within 20 yards, OR
                 * If we have low health (use potion health)
                 * And not on the Heart of sin
                 */

                bool anyTime = (Settings.Combat.Barbarian.WOTBMode == BarbarianWOTBMode.WhenReady && !Player.IsInTown);
                bool hasBuff = GetHasBuff(SNOPower.Barbarian_WrathOfTheBerserker);
                bool hasInfiniteCasting = GetHasBuff(SNOPower.Pages_Buff_Infinite_Casting);

                bool emergencyHealth = Player.CurrentHealthPct <= V.F("Barbarian.WOTB.EmergencyHealth");

                bool result =
                    (!UseOOCBuff || anyTime) &&
                    //Player.PrimaryResource >= V.I("Barbarian.WOTB.MinFury") && // WOTB is "free" !
                    // Don't still have the buff
                    !hasBuff &&
                    (WOTBGoblins || WOTBIgnoreElites || WOTBElitesPresent || emergencyHealth || hasInfiniteCasting);

                return result;
            }
        }

        /// <summary>
        /// If using WOTB on all elites, or if we should only use on "hard" affixes
        /// </summary>
        public static bool WOTBElitesPresent
        {
            get
            {
                //bool hardEliteOverride = Trinity.ObjectCache.Any(o => DataDictionary.ForceUseWOTBIds.Contains(o.ActorSNO)) ||
                //        TargetUtil.AnyElitesInRange(V.F("Barbarian.WOTB.HardEliteRangeOverride"), V.I("Barbarian.WOTB.HardEliteCountOverride"));

                //// WotB only used on Arcane, Frozen, Jailer, Molten, Electrified+Reflect Damage elites, or bosses and ubers, or when more than 4 elites are present
                //bool wotbHardElitesPresent = HardElitesPresent || hardEliteOverride;

                bool hardElitesOnly = Settings.Combat.Barbarian.WOTBMode == BarbarianWOTBMode.HardElitesOnly;

                bool elitesPresent = TargetUtil.AnyElitesInRange(V.F("Barbarian.WOTB.MinRange"), V.I("Barbarian.WOTB.MinCount"));

                return ((!hardElitesOnly && elitesPresent) || (hardElitesOnly && HardElitesPresent));
            }
        }

        /// <summary>
        /// Make sure we are allowed to use wrath on goblins, else make sure this isn't a goblin
        /// </summary>
        public static bool WOTBGoblins
        {
            get
            {
                if (CurrentTarget == null)
                    return false;
                return CurrentTarget.IsTreasureGoblin && Settings.Combat.Barbarian.UseWOTBGoblin;
            }
        }

        /// <summary>
        /// If ignoring elites completely, trigger on 3 trash within 25 yards, or 10 trash in 50 yards
        /// </summary>
        public static bool WOTBIgnoreElites
        {
            get
            {
                return
                    IgnoringElites &&
                    (TargetUtil.AnyMobsInRange(V.F("Barbarian.WOTB.RangeNear"), V.I("Barbarian.WOTB.CountNear")) ||
                    TargetUtil.AnyMobsInRange(V.F("Barbarian.WOTB.RangeFar"), V.I("Barbarian.WOTB.CountFar")) ||
                    TargetUtil.AnyMobsInRange(Settings.Combat.Misc.TrashPackClusterRadius, Settings.Combat.Misc.TrashPackSize));
            }
        }

        public static bool CanUseEarthquake
        {
            get
            {
                double minFury = 50f;
                bool hasCaveIn = CacheData.Hotbar.ActiveSkills.Any(p => p.Power == SNOPower.Barbarian_Earthquake && p.RuneIndex == 4);
                float range = hasCaveIn ? 24f : 14f;

                return
                       !UseOOCBuff &&
                       !IsCurrentlyAvoiding &&
                       !Player.IsIncapacitated &&
                       CanCast(SNOPower.Barbarian_Earthquake) &&
                       Player.PrimaryResource >= minFury &&
                       (TargetUtil.IsEliteTargetInRange(range) || TargetUtil.AnyMobsInRange(range, 10));

            }
        }

        public static bool CanUseBattleRage
        {
            get
            {
                return !UseOOCBuff && !Player.IsIncapacitated && CanCast(SNOPower.Barbarian_BattleRage, CanCastFlags.NoTimer) &&
                    (!GetHasBuff(SNOPower.Barbarian_BattleRage) || ShouldFuryDump) &&
                    Player.PrimaryResource >= V.F("Barbarian.BattleRage.MinFury");
            }
        }

        public static bool CanUseSprintOOC
        {
            get
            {
                return
                (Settings.Combat.Barbarian.SprintMode != BarbarianSprintMode.CombatOnly) &&
                AllowSprintOOC &&
                !Player.IsIncapacitated &&
                CanCast(SNOPower.Barbarian_Sprint) &&
                (Settings.Combat.Misc.AllowOOCMovement || GetHasBuff(SNOPower.Barbarian_WrathOfTheBerserker)) &&
                !GetHasBuff(SNOPower.Barbarian_Sprint) &&
                Player.PrimaryResource >= V.F("Barbarian.Sprint.MinFury");
            }
        }
        public static bool CanUseWarCry
        {
            get
            {
                return
                    CanCast(SNOPower.X1_Barbarian_WarCry_v2, CanCastFlags.NoTimer) &&
                    !Player.IsIncapacitated &&
                    (Player.PrimaryResource <= V.F("Barbarian.WarCry.MaxFury") || !GetHasBuff(SNOPower.X1_Barbarian_WarCry_v2));
            }
        }
        public static bool CanUseThreatingShout
        {
            get
            {
                var range = V.F("Barbarian.ThreatShout.Range");

                bool inCombat = !UseOOCBuff &&
                    CanCast(SNOPower.Barbarian_ThreateningShout) &&
                    !Player.IsIncapacitated &&
                    ((TargetUtil.AnyMobsInRange(range, Settings.Combat.Barbarian.MinThreatShoutMobCount, false)) || TargetUtil.IsEliteTargetInRange(range) ||

                        (Hotbar.Contains(SNOPower.Barbarian_Whirlwind) && Player.PrimaryResource <= V.I("Barbarian.Whirlwind.MinFury")) ||
                        (IsWaitingForSpecial && Player.PrimaryResource <= MinEnergyReserve)
                    );

                bool outOfCombat = UseOOCBuff &&
                     !Player.IsIncapacitated &&
                     Settings.Combat.Barbarian.ThreatShoutOOC && CanCast(SNOPower.Barbarian_ThreateningShout) &&
                     Player.PrimaryResource < V.D("Barbarian.ThreatShout.OOCMaxFury");

                return inCombat || outOfCombat;

            }
        }
        public static bool CanUseGroundStomp
        {
            get
            {
                return
                    !UseOOCBuff &&
                    !Player.IsIncapacitated &&
                    CanCast(SNOPower.Barbarian_GroundStomp) &&
                    (
                        TargetUtil.AnyElitesInRange(V.F("Barbarian.GroundStomp.EliteRange"), V.I("Barbarian.GroundStomp.EliteCount")) ||
                        TargetUtil.AnyMobsInRange(V.F("Barbarian.GroundStomp.TrashRange"), V.I("Barbarian.GroundStomp.TrashCount")) ||
                        (Player.CurrentHealthPct <= V.F("Barbarian.GroundStomp.UseBelowHealthPct") && TargetUtil.AnyMobsInRange(V.F("Barbarian.GroundStomp.TrashRange")))
                    );
            }
        }
        public static bool CanUseRevenge
        {
            get
            {
                return
                    !UseOOCBuff &&
                    CanCast(SNOPower.Barbarian_Revenge) &&
                    !Player.IsIncapacitated &&
                    // Don't use revenge on goblins, too slow!
                    (!CurrentTarget.IsTreasureGoblin || TargetUtil.AnyMobsInRange(V.F("Barbarian.Revenge.TrashRange"), V.I("Barbarian.Revenge.TrashCount")));
            }
        }
        public static bool CanUseFuriousCharge
        {
            get
            {
                if (UseOOCBuff)
                    return false;

                var bestTarget = TargetUtil.GetBestPierceTarget(MaxFuriousChargeDistance);
                int unitsInFrontOfBestTarget = 0;

                if (bestTarget != null)
                    unitsInFrontOfBestTarget = bestTarget.CountUnitsInFront();

                bool currentEliteTargetInRange = CurrentTarget.RadiusDistance > 7f && CurrentTarget.IsBossOrEliteRareUnique && CurrentTarget.RadiusDistance <= 35f;

                return CanCast(SNOPower.Barbarian_FuriousCharge, CanCastFlags.NoTimer) && !IsCurrentlyAvoiding &&
                    (currentEliteTargetInRange || unitsInFrontOfBestTarget >= 3 || Sets.TheLegacyOfRaekor.IsFullyEquipped);

            }
        }
        public static bool CanUseLeap
        {
            get
            {
                bool leapresult = !UseOOCBuff && !Player.IsIncapacitated && CanCast(SNOPower.Barbarian_Leap);
                // This will now cast whenever leap is available and an enemy is around. 
                // Disable Leap OOC option. The last line will prevent you from leaping on destructibles
                if (Legendary.LutSocks.IsEquipped)
                {
                    return leapresult && TargetUtil.AnyMobsInRange(15f, 1);
                }
                return leapresult && (TargetUtil.ClusterExists(15f, 35f, V.I("Barbarian.Leap.TrashCount")) || CurrentTarget.IsBossOrEliteRareUnique);
            }
        }
        public static bool CanUseRend
        {
            get
            {
                if (UseOOCBuff || IsCurrentlyAvoiding || Player.IsIncapacitated || !CanCast(SNOPower.Barbarian_Rend))
                    return false;

                if (!CanCast(SNOPower.Barbarian_Rend))
                    return false;

                bool hasReserveEnergy = (!IsWaitingForSpecial && Player.PrimaryResource >= 20) || (IsWaitingForSpecial && Player.PrimaryResource > MinEnergyReserve);

                bool mobCountThreshold = Trinity.ObjectCache.Count(o => o.IsUnit && (!o.HasDotDPS) && o.RadiusDistance <= 12) >= 3 ||
                    CurrentTarget.IsEliteRareUnique;

                if (!mobCountThreshold)
                    return false;

                // Spam with Bloodlust
                if (Runes.Barbarian.BloodLust.IsActive && Player.CurrentHealthPct <= .25)
                    return true;

                return Skills.Barbarian.Rend.TimeSinceUse > Settings.Combat.Barbarian.RendWaitDelay && hasReserveEnergy;
            }
        }
        public static bool CanUseOverPower
        {
            get
            {
                return !UseOOCBuff && CanCast(SNOPower.Barbarian_Overpower) && !Player.IsIncapacitated &&
                    (Hotbar.Contains(SNOPower.Barbarian_BattleRage) && GetHasBuff(SNOPower.Barbarian_BattleRage) || !Hotbar.Contains(SNOPower.Barbarian_BattleRage)) &&
                    (CurrentTarget.RadiusDistance <= V.F("Barbarian.OverPower.MaxRange") ||
                        (
                        TargetUtil.AnyMobsInRange(V.F("Barbarian.OverPower.MaxRange")) &&
                        (CurrentTarget.IsEliteRareUnique || CurrentTarget.IsMinion || CurrentTarget.IsBoss || GetHasBuff(SNOPower.Barbarian_WrathOfTheBerserker) ||
                        (CurrentTarget.IsTreasureGoblin && CurrentTarget.Distance <= V.F("Barbarian.OverPower.MaxRange")) || Hotbar.Contains(SNOPower.Barbarian_SeismicSlam))
                        )
                    );
            }
        }
        public static bool CanUseSeismicSlam
        {
            get
            {
                return !UseOOCBuff && !IsWaitingForSpecial && CanCast(SNOPower.Barbarian_SeismicSlam) && !Player.IsIncapacitated &&
                    (!Hotbar.Contains(SNOPower.Barbarian_BattleRage) || (Hotbar.Contains(SNOPower.Barbarian_BattleRage) && GetHasBuff(SNOPower.Barbarian_BattleRage))) &&
                    Player.PrimaryResource >= V.I("Barbarian.SeismicSlam.MinFury") && CurrentTarget.Distance <= V.F("Barbarian.SeismicSlam.CurrentTargetRange") &&
                    (TargetUtil.AnyMobsInRange(V.F("Barbarian.SeismicSlam.TrashRange")) ||
                     TargetUtil.IsEliteTargetInRange(V.F("Barbarian.SeismicSlam.EliteRange")));
            }
        }
        public static bool CanUseAncientSpear
        {
            get
            {
                return !UseOOCBuff && !IsWaitingForSpecial && !IsCurrentlyAvoiding && CanCast(SNOPower.X1_Barbarian_AncientSpear) && Player.PrimaryResource >= 25 &&
                    // Only boulder toss as a rage dump if we have excess resource
                    (!Runes.Barbarian.BoulderToss.IsActive || Player.PrimaryResourcePct > 0.8) &&
                    CurrentTarget.HitPointsPct >= V.F("Barbarian.AncientSpear.MinHealthPct");
            }
        }
        public static bool CanUseSprint
        {
            get
            {
                return Trinity.Settings.Combat.Barbarian.SprintMode != BarbarianSprintMode.MovementOnly &&
                    !UseOOCBuff && CanCast(SNOPower.Barbarian_Sprint, CanCastFlags.NoTimer) && !Player.IsIncapacitated &&
                    (
                    // last power used was whirlwind and we don't have sprint up
                        (LastPowerUsed == SNOPower.Barbarian_Whirlwind && !GetHasBuff(SNOPower.Barbarian_Sprint)) ||
                    // Fury Dump Options for sprint: use at max energy constantly
                        ShouldFuryDump ||
                    // or on a timer
                        (
                         (SNOPowerUseTimer(SNOPower.Barbarian_Sprint) && !GetHasBuff(SNOPower.Barbarian_Sprint)) &&
                    // Always keep up if we are whirlwinding, if the target is a goblin, or if we are more than 16 feet away from the target
                         (Hotbar.Contains(SNOPower.Barbarian_Whirlwind) || CurrentTarget.IsTreasureGoblin ||
                          (CurrentTarget.Distance >= V.F("Barbarian.Sprint.SingleTargetRange") && Player.PrimaryResource >= V.F("Barbarian.Sprint.SingleTargetMinFury"))
                         )
                        )
                    ) &&
                    // minimum time between uses
                    TimeSincePowerUse(SNOPower.Barbarian_Sprint) >= V.I("Barbarian.Sprint.MinUseDelay") &&
                    // If they have battle-rage, make sure it's up
                    (!Hotbar.Contains(SNOPower.Barbarian_BattleRage) || (Hotbar.Contains(SNOPower.Barbarian_BattleRage) && GetHasBuff(SNOPower.Barbarian_BattleRage))) &&
                    // Check for minimum energy
                    Player.PrimaryResource >= V.F("Barbarian.Sprint.MinFury");
            }
        }
        public static bool CanUseFrenzyTo5
        {
            get
            {
                return !UseOOCBuff && !IsCurrentlyAvoiding && !Player.IsRooted && Hotbar.Contains(SNOPower.Barbarian_Frenzy) &&
                    !TargetUtil.AnyMobsInRange(15f, 3) && GetBuffStacks(SNOPower.Barbarian_Frenzy) < 5;
            }
        }
        public static bool CanUseBashTo3
        {
            get
            {
                // minimum checks
                if (UseOOCBuff)
                    return false;
                if (IsCurrentlyAvoiding)
                    return false;
                if (!Hotbar.Contains(SNOPower.Barbarian_Bash))
                    return false;

                //skillDict.Add("Bash", SNOPower.Barbarian_Bash);
                //runeDict.Add("Clobber", 2);
                //runeDict.Add("Onslaught", 0);
                //runeDict.Add("Punish", 1);
                //runeDict.Add("Instigation", 3);
                //runeDict.Add("Pulverize", 4);

                bool hasPunish = CacheData.Hotbar.ActiveSkills.Any(s => s.Power == SNOPower.Barbarian_Bash && s.RuneIndex == 1);

                // for combo use with Frenzy or Cleave
                if (hasPunish)
                {
                    // haven't bashed, ever
                    if (!SpellHistory.HasUsedSpell(SNOPower.Barbarian_Bash))
                        return true;

                    int timeSinceUse = SpellHistory.TimeSinceUse(SNOPower.Barbarian_Bash).Milliseconds;

                    // been almost 5 seconds since our last bash (keep the Punish buff up)
                    if (timeSinceUse >= 4600)
                        return true;

                    // if it's been less than 5 seconds, check if we have used 2 in 10 seconds
                    if (SpellHistory.HistoryQueue.Count(i => i.TimeSinceUse.TotalMilliseconds < 9600) <= 2)
                        return true;

                    // if it's been less than 5 seconds, check if we have used 3 in 15 seconds (for full stack)
                    if (SpellHistory.HistoryQueue.Count(i => i.TimeSinceUse.TotalSeconds < 14600) <= 3)
                        return true;
                }

                return false;
            }
        }
        public static bool CanUseWhirlwind
        {
            get
            {
                if (UseOOCBuff || IsCurrentlyAvoiding || !CanCast(SNOPower.Barbarian_Whirlwind) || Player.IsIncapacitated || Player.IsRooted || Player.PrimaryResource < 10)
                    return false;

                return (CurrentTarget.RadiusDistance <= 25f || TargetUtil.AnyMobsInRange(V.F("Barbarian.Whirlwind.TrashRange"), V.I("Barbarian.Whirlwind.TrashCount"))) &&
                    // Check for energy reservation amounts
                    Player.PrimaryResource >= V.D("Barbarian.Whirlwind.MinFury") &&
                    // If they have battle-rage, make sure it's up
                    (!Hotbar.Contains(SNOPower.Barbarian_BattleRage) || (Hotbar.Contains(SNOPower.Barbarian_BattleRage) && GetHasBuff(SNOPower.Barbarian_BattleRage)));
            }
        }

        public static bool CanUseHammerOfTheAncients
        {
            get
            {
                bool hotaresult = !UseOOCBuff && !IsCurrentlyAvoiding && !Player.IsIncapacitated && !IsWaitingForSpecial && CanCast(SNOPower.Barbarian_HammerOfTheAncients) &&
                    (Player.PrimaryResource >= V.F("Barbarian.HammerOfTheAncients.MinFury") || LastPowerUsed == SNOPower.Barbarian_HammerOfTheAncients) &&
                    (!Hotbar.Contains(SNOPower.Barbarian_Whirlwind) || (Player.CurrentHealthPct >= Settings.Combat.Barbarian.MinHotaHealth && Hotbar.Contains(SNOPower.Barbarian_Whirlwind)));

                if (Legendary.LutSocks.IsEquipped)
                {
                    return !CanUseLeap && hotaresult;
                }
                else
                {
                    return hotaresult;
                }
            }
        }
        public static bool CanUseHammerOfTheAncientsElitesOnly
        {
            get
            {
                bool canUseHota = CanUseHammerOfTheAncients;

                if (Legendary.LutSocks.IsEquipped)
                {
                    if (canUseHota)
                    {
                        bool hotaElites = (CurrentTarget.IsBossOrEliteRareUnique || CurrentTarget.IsTreasureGoblin) && TargetUtil.EliteOrTrashInRange(10f);

                        bool hotaTrash = IgnoringElites && CurrentTarget.IsTrashMob &&
                            (TargetUtil.EliteOrTrashInRange(6f) || CurrentTarget.MonsterSize == Zeta.Game.Internals.SNO.MonsterSize.Big);

                        return canUseHota && (hotaElites || hotaTrash);
                    }
                }
                else
                {
                    if (canUseHota && !CanUseLeap)
                    {
                        bool hotaElites = (CurrentTarget.IsBossOrEliteRareUnique || CurrentTarget.IsTreasureGoblin) && TargetUtil.EliteOrTrashInRange(10f);

                        bool hotaTrash = IgnoringElites && CurrentTarget.IsTrashMob &&
                            (TargetUtil.EliteOrTrashInRange(6f) || CurrentTarget.MonsterSize == Zeta.Game.Internals.SNO.MonsterSize.Big);

                        return canUseHota && (hotaElites || hotaTrash);
                    }
                }
                return false;
            }
        }
        public static bool CanUseWeaponThrow { get { return !UseOOCBuff && !IsCurrentlyAvoiding && CanCast(SNOPower.X1_Barbarian_WeaponThrow); } }
        public static bool CanUseFrenzy { get { return !UseOOCBuff && !IsCurrentlyAvoiding && CanCast(SNOPower.Barbarian_Frenzy); } }
        public static bool CanUseBash { get { return !UseOOCBuff && !IsCurrentlyAvoiding && CanCast(SNOPower.Barbarian_Bash); } }
        public static bool CanUseCleave { get { return !UseOOCBuff && !IsCurrentlyAvoiding && CanCast(SNOPower.Barbarian_Cleave); } }
        public static bool CanUseAvalanche
        {
            get
            {
                bool hasBerserker = CacheData.Hotbar.PassiveSkills.Any(p => p == SNOPower.Barbarian_Passive_BerserkerRage);
                double minFury = hasBerserker ? Player.PrimaryResourceMax * 0.99 : 0f;

                return !UseOOCBuff && !IsCurrentlyAvoiding && CanCast(SNOPower.X1_Barbarian_Avalanche_v2, CanCastFlags.NoTimer) &&
                    Player.PrimaryResource >= minFury && (TargetUtil.AnyMobsInRange(3) || TargetUtil.IsEliteTargetInRange());

            }
        }


        public static TrinityPower PowerAvalanche { get { return new TrinityPower(SNOPower.X1_Barbarian_Avalanche_v2, 15f, TargetUtil.GetBestClusterUnit(15f, 45f).Position); } }
        public static TrinityPower PowerIgnorePain { get { return new TrinityPower(SNOPower.Barbarian_IgnorePain); } }
        public static TrinityPower PowerEarthquake
        {
            get
            {
                return new TrinityPower(SNOPower.Barbarian_Earthquake);
            }
        }
        public static TrinityPower PowerWrathOfTheBerserker { get { return new TrinityPower(SNOPower.Barbarian_WrathOfTheBerserker); } }
        public static TrinityPower PowerCallOfTheAncients { get { return new TrinityPower(SNOPower.Barbarian_CallOfTheAncients, V.I("Barbarian.CallOfTheAncients.TickDelay"), V.I("Barbarian.CallOfTheAncients.TickDelay")); } }
        public static TrinityPower PowerBattleRage { get { return new TrinityPower(SNOPower.Barbarian_BattleRage); } }
        public static TrinityPower PowerSprint { get { return new TrinityPower(SNOPower.Barbarian_Sprint, 0, 0); } }
        public static TrinityPower PowerWarCry { get { return new TrinityPower(SNOPower.X1_Barbarian_WarCry_v2); } }
        public static TrinityPower PowerThreateningShout { get { return new TrinityPower(SNOPower.Barbarian_ThreateningShout); } }
        public static TrinityPower PowerGroundStomp { get { return new TrinityPower(SNOPower.Barbarian_GroundStomp); } }
        public static TrinityPower PowerRevenge { get { return new TrinityPower(SNOPower.Barbarian_Revenge); } }

        public static TrinityPower PowerFuriousCharge
        {
            get
            {
                var bestTarget = TargetUtil.GetBestPierceTarget(MaxFuriousChargeDistance);

                if (bestTarget != null)
                    return new TrinityPower(SNOPower.Barbarian_FuriousCharge, V.F("Barbarian.FuriousCharge.UseRange"), bestTarget.Position);
                return new TrinityPower(SNOPower.Barbarian_FuriousCharge, V.F("Barbarian.FuriousCharge.UseRange"), CurrentTarget.Position);
            }
        }
        public static TrinityPower PowerLeap
        {
            get
            {
                // For Call of Arreat rune. Will do all quakes on top of each other
                bool hasCallOfArreat = CacheData.Hotbar.ActiveSkills.Any(p => p.Power == SNOPower.Barbarian_Leap && p.RuneIndex == 0);
                if (Legendary.LutSocks.IsEquipped && hasCallOfArreat)
                {
                    Vector3 aoeTarget = TargetUtil.GetBestClusterPoint(7f, 9f, false);
                    return new TrinityPower(SNOPower.Barbarian_Leap, V.F("Barbarian.Leap.UseRange"), aoeTarget);
                }
                else
                {
                    Vector3 aoeTarget = TargetUtil.GetBestClusterPoint(15f, 35f, false);
                    return new TrinityPower(SNOPower.Barbarian_Leap, V.F("Barbarian.Leap.UseRange"), aoeTarget);
                }
            }
        }
        public static TrinityPower PowerRend { get { return new TrinityPower(SNOPower.Barbarian_Rend, V.I("Barbarian.Rend.TickDelay"), V.I("Barbarian.Rend.TickDelay")); } }
        public static TrinityPower PowerOverpower { get { return new TrinityPower(SNOPower.Barbarian_Overpower); } }
        public static TrinityPower PowerSeismicSlam { get { return new TrinityPower(SNOPower.Barbarian_SeismicSlam, V.F("Barbarian.SeismicSlam.UseRange"), CurrentTarget.ACDGuid); } }
        public static TrinityPower PowerAncientSpear
        {
            get
            {
                var bestAoEUnit = TargetUtil.GetBestPierceTarget(35f);

                if (Runes.Barbarian.BoulderToss.IsActive)
                    bestAoEUnit = TargetUtil.GetBestClusterUnit(9f);

                return new TrinityPower(SNOPower.X1_Barbarian_AncientSpear, V.F("Barbarian.AncientSpear.UseRange"), bestAoEUnit.ACDGuid);
            }
        }
        public static TrinityPower PowerWhirlwind
        {
            get
            {
                return new TrinityPower(SNOPower.Barbarian_Whirlwind, 20f, TargetUtil.GetZigZagTarget(CurrentTarget.Position, 20), Trinity.CurrentWorldDynamicId, -1, 0, 1);
            }
        }
        public static TrinityPower PowerHammerOfTheAncients { get { return new TrinityPower(SNOPower.Barbarian_HammerOfTheAncients, V.F("Barbarian.HammerOfTheAncients.UseRange"), CurrentTarget.ACDGuid); } }
        public static TrinityPower PowerWeaponThrow { get { return new TrinityPower(SNOPower.X1_Barbarian_WeaponThrow, V.F("Barbarian.WeaponThrow.UseRange"), CurrentTarget.ACDGuid); } }
        public static TrinityPower PowerFrenzy { get { return new TrinityPower(SNOPower.Barbarian_Frenzy, V.F("Barbarian.Frenzy.UseRange"), CurrentTarget.ACDGuid); } }
        public static TrinityPower PowerBash { get { return new TrinityPower(SNOPower.Barbarian_Bash, V.F("Barbarian.Bash.UseRange"), Vector3.Zero, -1, CurrentTarget.ACDGuid, 2, 2); } }
        public static TrinityPower PowerCleave { get { return new TrinityPower(SNOPower.Barbarian_Cleave, V.F("Barbarian.Cleave.UseRange"), CurrentTarget.ACDGuid); } }

        private static TrinityPower DestroyObjectPower
        {
            get
            {
                if (CanCast(SNOPower.Barbarian_Frenzy))
                    return new TrinityPower(SNOPower.Barbarian_Frenzy, 4f);

                if (CanCast(SNOPower.Barbarian_Bash))
                    return new TrinityPower(SNOPower.Barbarian_Bash, 4f);

                if (CanCast(SNOPower.Barbarian_Cleave))
                    return new TrinityPower(SNOPower.Barbarian_Cleave, 4f);

                if (CanCast(SNOPower.X1_Barbarian_WeaponThrow))
                    return new TrinityPower(SNOPower.X1_Barbarian_WeaponThrow, 4f);

                if (CanCast(SNOPower.Barbarian_Overpower))
                    return new TrinityPower(SNOPower.Barbarian_Overpower, 9);

                if (CanCast(SNOPower.Barbarian_Whirlwind))
                    return new TrinityPower(SNOPower.Barbarian_Whirlwind, V.F("Barbarian.Whirlwind.UseRange"), LastZigZagLocation);

                if (CanCast(SNOPower.Barbarian_Rend) && Player.PrimaryResourcePct >= 0.65)
                    return new TrinityPower(SNOPower.Barbarian_Rend, V.F("Barbarian.Rend.UseRange"));

                return DefaultPower;
            }
        }

        public static bool AllowSprintOOC
        {
            get { return _allowSprintOoc; }
            set { _allowSprintOoc = value; }
        }

        private static bool BarbHasNoPrimary
        {
            get
            {
                return !(Hotbar.Contains(SNOPower.Barbarian_Frenzy) ||
                    Hotbar.Contains(SNOPower.Barbarian_Bash) ||
                    Hotbar.Contains(SNOPower.Barbarian_Cleave));
            }
        }

        private static bool HasMoreThanOnePrimary
        {
            get
            {
                return Hotbar.Count(i => i == SNOPower.Barbarian_Frenzy || i == SNOPower.Barbarian_Bash || i == SNOPower.Barbarian_Cleave) > 1;
            }
        }
        private static bool ShouldFuryDump
        {
            get
            {
                bool berserkerRage = !Passives.Barbarian.BerserkerRage.IsActive;
                bool ignoranceIsBliss = Runes.Barbarian.IgnoranceIsBliss.IsActive && GetHasBuff(SNOPower.Barbarian_IgnorePain) && Trinity.Player.CurrentHealthPct <= 1;

                return ((berserkerRage || ignoranceIsBliss) && GetHasBuff(SNOPower.Barbarian_WrathOfTheBerserker) &&
                        ((Settings.Combat.Barbarian.FuryDumpWOTB && Player.PrimaryResourcePct >= V.F("Barbarian.WOTB.FuryDumpMin")) ||
                         Settings.Combat.Barbarian.FuryDumpAlways && Player.PrimaryResourcePct >= V.F("Barbarian.WOTB.FuryDumpMin")));
            }
        }


    }
}
