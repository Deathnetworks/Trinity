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
                if (Player.PrimaryResource >= 50)
                {
                    Logger.Log(TrinityLogLevel.Verbose, LogCategory.UserInformation, "Barbarian_WrathOfTheBerserker being used!({0})", CurrentTarget.InternalName);
                    IsWaitingForSpecial = false;
                    power = PowerWrathOfTheBerserker;
                }
                else
                {
                    Logger.Log(TrinityLogLevel.Verbose, LogCategory.UserInformation, "Barbarian_WrathOfTheBerserker ready, waiting for fury...");
                    IsWaitingForSpecial = true;
                }
            }
            // Call of the Ancients
            if (IsNull(power) && CanCastCallOfTheAncients)
            {
                if (Player.PrimaryResource >= 50)
                {
                    IsWaitingForSpecial = false;
                    power = PowerCallOfTheAncients;
                }
                else
                {
                    Logger.Log(TrinityLogLevel.Verbose, LogCategory.UserInformation, "Call of the Ancients ready, waiting for fury...");
                    IsWaitingForSpecial = true;
                }
            }

            // Battle Rage
            if (IsNull(power) && CanCastBattleRage)
                power = PowerBattleRage;

            if (IsNull(power) && CanCastSprintOOC)

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
                    !GetHasBuff(SNOPower.Barbarian_Earthquake);
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
                // WOTB with ignore elites
                bool test1 =
                    !UseOOCBuff &&
                    !IsCurrentlyAvoiding &&
                    CanCast(SNOPower.Barbarian_WrathOfTheBerserker) &&
                    Settings.Combat.Misc.IgnoreElites &&
                    (TargetUtil.AnyMobsInRange(25, 3) ||
                    TargetUtil.AnyMobsInRange(50, 10) ||
                    TargetUtil.AnyMobsInRange(Settings.Combat.Misc.TrashPackClusterRadius, Settings.Combat.Misc.TrashPackSize)) &&
                    !GetHasBuff(SNOPower.Barbarian_WrathOfTheBerserker);

                // WOTB with elites
                bool test2 =
                    !UseOOCBuff &&
                    !IsCurrentlyAvoiding &&
                    CanCast(SNOPower.Barbarian_WrathOfTheBerserker) &&
                    TargetUtil.AnyElitesInRange(1) &&
                    !GetHasBuff(SNOPower.Barbarian_WrathOfTheBerserker) &&
                    Player.PrimaryResource >= 50;

                return test1 || test2;
            }
        }
        public static bool CanCastWrathOfTheBerserker
        {
            get
            {
                //WotB only used on Arcane, Frozen, Jailer, Molten, Electrified+Reflect Damage elites, or bosses and ubers, or when more than 4 elites are present
                bool hardElitesPresent = HardElitesPresent ||
                    Trinity.ObjectCache.Any(o => DataDictionary.ForceUseWOTBIds.Contains(o.ActorSNO)) ||
                        TargetUtil.AnyElitesInRange(50, 4);

                // If using WOTB on all elites, or if we should only use on "hard" affixes
                bool wotbHardCheck = (!Settings.Combat.Barbarian.WOTBHardOnly || (hardElitesPresent && Settings.Combat.Barbarian.WOTBHardOnly));
                // Make sure we are allowed to use wrath on goblins, else make sure this isn't a goblin
                bool wotbGoblins = (!Settings.Combat.Barbarian.UseWOTBGoblin || (Settings.Combat.Barbarian.UseWOTBGoblin && CurrentTarget.IsTreasureGoblin));
                // If ignoring elites completely, trigger on 3 trash within 25 yards, or 10 trash in 50 yards
                bool wotbIgnoreElites = (Settings.Combat.Misc.IgnoreElites && (TargetUtil.AnyMobsInRange(25, 3) || TargetUtil.AnyMobsInRange(50, 10)) || !Settings.Combat.Misc.IgnoreElites);
                // Otherwise use when Elite target is in 20 yards
                bool wotbEliteCheck = (TargetUtil.AnyElitesInRange(20, 1) || TargetUtil.IsEliteTargetInRange(20f));

                return
                    !UseOOCBuff &&
                    CanCast(SNOPower.Barbarian_WrathOfTheBerserker) &&
                    wotbHardCheck &&
                    // Not on heart of sin after Cydaea
                    CurrentTarget.ActorSNO != 193077 &&
                    (wotbGoblins || wotbIgnoreElites || wotbEliteCheck || //Or if our health is low
                     Player.CurrentHealthPct <= 60) &&
                    // Don't still have the buff
                    !GetHasBuff(SNOPower.Barbarian_WrathOfTheBerserker);
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
                    !GetHasBuff(SNOPower.Barbarian_CallOfTheAncients);
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
