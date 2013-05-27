using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Trinity.Technicals;
using Zeta.Common;
using Zeta.CommonBot;
using Zeta.Internals.Actors;

namespace Trinity.Combat.Abilities
{
    class BarbarianCombat : CombatBase
    {
        private static bool allowSprintOOC = true;

        public static TrinityPower GetPower()
        {
            TrinityPower power = null;

            if (UseDestructiblePower)
                power = DestroyObjectPower;

            // Ignore Pain when low on health
            if (IsNull(power) && CanCastIgnorePain)
                power = PowerIgnorePain;

            IsWaitingForSpecial = false;

            // Check if we should conserve Fury for specials
            if (IsNull(power) && Player.PrimaryResource < MinEnergyReserve)
            {
                if (ShouldWaitForEarthquake)
                {
                    Logger.LogNormal("Waiting for Barbarian_Earthquake!");
                    IsWaitingForSpecial = true;
                }
                if (ShouldWaitForWrathOfTheBerserker)
                {
                    Logger.LogNormal("Waiting for Barbarian_WrathOfTheBerserker 1!");
                    IsWaitingForSpecial = true;
                }
                if (ShouldWaitForCallOfTheAncients)
                {
                    Logger.LogNormal("Waiting for Barbarian_CallOfTheAncients!");
                    IsWaitingForSpecial = true;
                }
            }

            // Earthquake
            if (IsNull(power) && CanCastEarthquake)
                power = PowerEarthquake;

            // WOTB
            if (IsNull(power) && CanCastWrathOfTheBerserker)
            {
                Logger.Log(TrinityLogLevel.Verbose, LogCategory.UserInformation, "Barbarian_WrathOfTheBerserker being used!({0})", CurrentTarget.InternalName);
                power = PowerWrathOfTheBerserker;
            }

            // Call of the Ancients
            if (IsNull(power) && CanCastCallOfTheAncients)
                power = PowerCallOfTheAncients;

            // Battle Rage
            if (IsNull(power) && CanCastBattleRage)
                power = PowerBattleRage;

            if (IsNull(power) && CanCastSprintOOC)
                power = PowerSprint;

            // Default Attacks
            if (IsNull(power))
                power = CombatBase.DefaultPower;

            return power;

        }

        public static bool CanCastIgnorePain
        {
            get
            {
                return
                    !UseOOCBuff &&
                    CanCast(SNOPower.Barbarian_IgnorePain) &&
                    Player.CurrentHealthPct <= 0.45;
            }
        }
        public static bool ShouldWaitForEarthquake
        {
            get
            {
                return
                    !UseOOCBuff &&
                    !IsCurrentlyAvoiding &&
                    CanCast(SNOPower.Barbarian_Earthquake) &&
                    TargetUtil.AnyElitesInRange(25) &&
                    !GetHasBuff(SNOPower.Barbarian_Earthquake) &&
                    Player.PrimaryResource <= 50; ;
            }
        }
        public static bool CanCastEarthquake
        {
            get
            {
                return
                    !UseOOCBuff &&
                    !IsCurrentlyAvoiding &&
                    !Player.IsIncapacitated &&
                    CanCast(SNOPower.Barbarian_Earthquake) &&
                    !GetHasBuff(SNOPower.Barbarian_Earthquake) &&
                    TargetUtil.IsEliteTargetInRange(13f) &&
                    Player.PrimaryResource > 50;
            }
        }
        public static bool ShouldWaitForWrathOfTheBerserker
        {
            get
            {
                // WOTB with elites
                bool wotbElites =
                    (WOTBGoblins || WOTBElitesPresent);

                return
                    !UseOOCBuff &&
                    !IsCurrentlyAvoiding &&
                    Player.PrimaryResource <= 50 &&
                    CanCast(SNOPower.Barbarian_WrathOfTheBerserker) &&
                    !GetHasBuff(SNOPower.Barbarian_WrathOfTheBerserker) &&
                    (WOTBIgnoreElites || wotbElites);
            }
        }

        /// <summary>
        /// If using WOTB on all elites, or if we should only use on "hard" affixes
        /// </summary>
        public static bool WOTBElitesPresent
        {
            get
            {
                // WotB only used on Arcane, Frozen, Jailer, Molten, Electrified+Reflect Damage elites, or bosses and ubers, or when more than 4 elites are present
                bool wotbHardElitesPresent = HardElitesPresent ||
                    Trinity.ObjectCache.Any(o => DataDictionary.ForceUseWOTBIds.Contains(o.ActorSNO)) ||
                        TargetUtil.AnyElitesInRange(50, 4);

                return
                    (!Settings.Combat.Barbarian.WOTBHardOnly && TargetUtil.AnyElitesInRange(20, 1))
                    || (wotbHardElitesPresent && Settings.Combat.Barbarian.WOTBHardOnly);
            }
        }

        /// <summary>
        /// Make sure we are allowed to use wrath on goblins, else make sure this isn't a goblin
        /// </summary>
        public static bool WOTBGoblins
        {
            get
            {
                return !Settings.Combat.Barbarian.UseWOTBGoblin || (Settings.Combat.Barbarian.UseWOTBGoblin && CurrentTarget.IsTreasureGoblin);
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
                    Settings.Combat.Misc.IgnoreElites &&
                    (TargetUtil.AnyMobsInRange(25, 3) || TargetUtil.AnyMobsInRange(50, 10) || TargetUtil.AnyMobsInRange(Settings.Combat.Misc.TrashPackClusterRadius, Settings.Combat.Misc.TrashPackSize)) ||
                    !Settings.Combat.Misc.IgnoreElites;
            }
        }

        public static bool CanCastWrathOfTheBerserker
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

                return
                    !UseOOCBuff &&
                    Player.PrimaryResource >= 50 &&
                    // Don't still have the buff
                    !GetHasBuff(SNOPower.Barbarian_WrathOfTheBerserker) &&
                    CanCast(SNOPower.Barbarian_WrathOfTheBerserker) &&
                    // Not on heart of sin after Cydaea
                    CurrentTarget.ActorSNO != 193077 &&
                    (WOTBGoblins || WOTBIgnoreElites || WOTBElitesPresent ||
                    //Or if our health is low
                    Player.CurrentHealthPct <= Settings.Combat.Barbarian.PotionLevel);
            }
        }
        public static bool ShouldWaitForCallOfTheAncients
        {
            get
            {
                return
                    !UseOOCBuff &&
                    !IsCurrentlyAvoiding &&
                    CanCast(SNOPower.Barbarian_CallOfTheAncients) &&
                    TargetUtil.AnyElitesInRange(25) &&
                    !GetHasBuff(SNOPower.Barbarian_CallOfTheAncients) &&
                    Player.PrimaryResource <= 50; ;
            }
        }
        public static bool CanCastCallOfTheAncients
        {
            get
            {
                return
                    !UseOOCBuff &&
                    !IsCurrentlyAvoiding &&
                    CanCast(SNOPower.Barbarian_CallOfTheAncients) &&
                    !Player.IsIncapacitated &&
                    TargetUtil.AnyElitesInRange(25f);
            }
        }
        public static bool CanCastBattleRage
        {
            get
            {
                return
                    !UseOOCBuff &&
                    !Player.IsIncapacitated &&
                    CanCast(SNOPower.Barbarian_BattleRage, CanCastFlags.NoTimer) &&
                    (SNOPowerUseTimer(SNOPower.Barbarian_BattleRage) || !GetHasBuff(SNOPower.Barbarian_BattleRage)) &&
                    Player.PrimaryResource >= 20;
            }
        }
        public static bool CanCastSprintOOC
        {
            get
            {
                return
                UseOOCBuff &&
                AllowSprintOOC &&
                !Player.IsIncapacitated &&
                CanCast(SNOPower.Barbarian_Sprint) &&
                (Settings.Combat.Misc.AllowOOCMovement || GetHasBuff(SNOPower.Barbarian_WrathOfTheBerserker)) &&
                !GetHasBuff(SNOPower.Barbarian_Sprint) &&
                Player.PrimaryResource >= 20;
            }
        }

        public static TrinityPower PowerIgnorePain { get { return new TrinityPower(SNOPower.Barbarian_IgnorePain); } }
        public static TrinityPower PowerEarthquake { get { return new TrinityPower(SNOPower.Barbarian_Earthquake); } }
        public static TrinityPower PowerWrathOfTheBerserker { get { return new TrinityPower(SNOPower.Barbarian_WrathOfTheBerserker); } }
        public static TrinityPower PowerCallOfTheAncients { get { return new TrinityPower(SNOPower.Barbarian_CallOfTheAncients, 4, 4); } }
        public static TrinityPower PowerBattleRage { get { return new TrinityPower(SNOPower.Barbarian_BattleRage); } }
        public static TrinityPower PowerSprint { get { return new TrinityPower(SNOPower.Barbarian_Sprint); } }
        public static TrinityPower PowerWarCry { get { return new TrinityPower(SNOPower.Barbarian_WarCry); } }
        public static TrinityPower PowerThreateningShout { get { return new TrinityPower(SNOPower.Barbarian_ThreateningShout); } }
        public static TrinityPower PowerGroundStomp { get { return new TrinityPower(SNOPower.Barbarian_GroundStomp); } }
        public static TrinityPower PowerRevenge { get { return new TrinityPower(SNOPower.Barbarian_Revenge); } }

        private static TrinityPower DestroyObjectPower
        {
            get
            {
                if (Hotbar.Contains(SNOPower.Barbarian_Whirlwind) && Player.PrimaryResource > MinEnergyReserve)
                    return new TrinityPower(SNOPower.Barbarian_Whirlwind, 10f, LastZigZagLocation);

                if (Hotbar.Contains(SNOPower.Barbarian_Frenzy))
                    return new TrinityPower(SNOPower.Barbarian_Frenzy, 10f);

                if (Hotbar.Contains(SNOPower.Barbarian_Bash))
                    return new TrinityPower(SNOPower.Barbarian_Bash, 6f);

                if (Hotbar.Contains(SNOPower.Barbarian_Cleave))
                    return new TrinityPower(SNOPower.Barbarian_Cleave, 6f);

                if (Hotbar.Contains(SNOPower.Barbarian_Rend) && Player.PrimaryResourcePct >= 0.65)
                    return new TrinityPower(SNOPower.Barbarian_Rend, 10f);

                if (Hotbar.Contains(SNOPower.Barbarian_WeaponThrow) && Player.PrimaryResource >= 20)
                    return new TrinityPower(SNOPower.Barbarian_WeaponThrow, 15f);

                return CombatBase.DefaultPower;
            }
        }

        public static bool AllowSprintOOC
        {
            get { return allowSprintOOC; }
            set { allowSprintOOC = value; }
        }

    }
}
