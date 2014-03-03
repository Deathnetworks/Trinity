using System;
using System.Linq;
using Trinity.Config.Combat;
using Zeta.Bot;
using Zeta.Common;
using Zeta.Game.Internals.Actors;
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

            if (UseOOCBuff)
            {
                // Sprint OOC
                if (IsNull(power) && CanCastSprintOOC)
                    power = PowerSprint;

                // Threatening Shout OOC

            }

            // Ignore Pain when low on health
            if (IsNull(power) && CanCastIgnorePain)
                power = PowerIgnorePain;

            IsWaitingForSpecial = false;

            // Check if we should conserve Fury for specials
            if (IsNull(power) && Player.PrimaryResource < MinEnergyReserve)
            {
                if (ShouldWaitForWrathOfTheBerserker)
                {
                    //Logger.LogNormal("Waiting for Barbarian_WrathOfTheBerserker 1!");
                    IsWaitingForSpecial = true;
                }
                if (ShouldWaitForEarthquake)
                {
                    //Logger.LogNormal("Waiting for Barbarian_Earthquake!");
                    IsWaitingForSpecial = true;
                }
                if (ShouldWaitForCallOfTheAncients)
                {
                    //Logger.LogNormal("Waiting for Barbarian_CallOfTheAncients!");
                    IsWaitingForSpecial = true;
                }
            }

            // WOTB
            if (IsNull(power) && CanCastWrathOfTheBerserker)
            {
                //Logger.Log(TrinityLogLevel.Verbose, LogCategory.UserInformation, "Barbarian_WrathOfTheBerserker being used! ({0})", CurrentTarget.InternalName);
                power = PowerWrathOfTheBerserker;
            }

            // Earthquake
            if (IsNull(power) && CanCastEarthquake)
                power = PowerEarthquake;

            // Call of the Ancients
            if (IsNull(power) && CanCastCallOfTheAncients)
                power = PowerCallOfTheAncients;

            // War Cry
            if (IsNull(power) && CanUseWarCry)
                power = PowerWarCry;

            // Battle Rage
            if (IsNull(power) && CanCastBattleRage || CanUseBattleRage)
                power = PowerBattleRage;

            // Leap
            if (IsNull(power) && CanUseLeap)
                power = PowerLeap;

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

            // Furious Charge
            if (IsNull(power) && CanUseFuriousCharge)
                power = PowerFuriousCharge;

            // Seismic Slam
            if (IsNull(power) && CanUseSeismicSlam)
                power = PowerSeismicSlam;

            // Ancient Spear
            if (IsNull(power) && CanUseAncientSpear)
                power = PowerAncientSpear;

            // Sprint
            if (IsNull(power) && CanUseSprint)
                power = PowerSprint;

            // Bash to 3 stacks (Punish)
            if (IsNull(power) && CanUseBashTo3)
                power = PowerBash;

            // Frenzy to 5 stacks (Maniac)
            if (IsNull(power) && CanUseFrenzyTo5)
                power = PowerFrenzy;

            // Weapon Throw: Dreadbomb
            if (IsNull(power) && CanUseDreadbomb)
                power = PowerWeaponThrow;

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
                    Player.CurrentHealthPct <= V.F("Barbarian.IgnorePain.MinHealth");
            }
        }
        public static bool ShouldWaitForEarthquake
        {
            get
            {
                return
                    Hotbar.Contains(SNOPower.Barbarian_Earthquake) &&
                    !UseOOCBuff &&
                    !IsCurrentlyAvoiding &&
                    !CanCast(SNOPower.Barbarian_Earthquake) &&
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
                    TargetUtil.IsEliteTargetInRange(6f) &&
                    Player.PrimaryResource > 50;
            }
        }
        public static bool ShouldWaitForWrathOfTheBerserker
        {
            get
            {
                if (UseOOCBuff || IsCurrentlyAvoiding)
                    return false;

                // WOTB with elites
                bool wotbElites =
                    (WOTBGoblins || WOTBElitesPresent);

                return
                    Hotbar.Contains(SNOPower.Barbarian_WrathOfTheBerserker) &&
                    !UseOOCBuff &&
                    !IsCurrentlyAvoiding &&
                    Player.PrimaryResource <= 50 &&
                    !CanCast(SNOPower.Barbarian_WrathOfTheBerserker) &&
                    !GetHasBuff(SNOPower.Barbarian_WrathOfTheBerserker) &&
                    (WOTBIgnoreElites || wotbElites || (Settings.Combat.Barbarian.WOTBMode == BarbarianWOTBMode.WhenReady));
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

                bool hardElitesOnly = Settings.Combat.Barbarian.WOTBMode == Config.Combat.BarbarianWOTBMode.HardElitesOnly;

                return
                    (!hardElitesOnly && TargetUtil.AnyElitesInRange(V.F("Barbarian.WOTB.MinRange"), V.I("Barbarian.WOTB.MinCount")))
                    || (hardElitesOnly && HardElitesPresent);
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
                    CombatBase.IgnoringElites &&
                    (TargetUtil.AnyMobsInRange(V.F("Barbarian.WOTB.RangeNear"), V.I("Barbarian.WOTB.CountNear")) ||
                    TargetUtil.AnyMobsInRange(V.F("Barbarian.WOTB.RangeFar"), V.I("Barbarian.WOTB.CountFar")) ||
                    TargetUtil.AnyMobsInRange(Settings.Combat.Misc.TrashPackClusterRadius, Settings.Combat.Misc.TrashPackSize));
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

                bool anyTime = (Settings.Combat.Barbarian.WOTBMode == Config.Combat.BarbarianWOTBMode.WhenReady && !Player.IsInTown);

                return
                    (!UseOOCBuff || anyTime) &&
                    Player.PrimaryResource >= V.I("Barbarian.WOTB.MinFury") &&
                    // Don't still have the buff
                    !GetHasBuff(SNOPower.Barbarian_WrathOfTheBerserker) &&
                    CanCast(SNOPower.Barbarian_WrathOfTheBerserker, CanCastFlags.NoTimer) &&
                    (WOTBGoblins || WOTBIgnoreElites || WOTBElitesPresent || anyTime ||
                    //Or if our health is low
                     Player.CurrentHealthPct <= V.F("Barbarian.WOTB.EmergencyHealth"));
            }
        }
        public static bool ShouldWaitForCallOfTheAncients
        {
            get
            {
                return
                    Hotbar.Contains(SNOPower.Barbarian_CallOfTheAncients) &&
                    !UseOOCBuff &&
                    !IsCurrentlyAvoiding &&
                    !CanCast(SNOPower.Barbarian_CallOfTheAncients) &&
                    TargetUtil.AnyElitesInRange(V.F("Barbarian.CallOfTheAncients.MinEliteRange")) &&
                    !GetHasBuff(SNOPower.Barbarian_CallOfTheAncients) &&
                    Player.PrimaryResource <= V.F("Barbarian.CallOfTheAncients.MinFury");
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
                    TargetUtil.AnyElitesInRange(V.F("Barbarian.CallOfTheAncients.MinEliteRange"));
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
                    (
                        !GetHasBuff(SNOPower.Barbarian_BattleRage) ||
                        SNOPowerUseTimer(SNOPower.Barbarian_BattleRage) || 
                        (Settings.Combat.Barbarian.FuryDumpWOTB && Player.PrimaryResourcePct >= V.F("Barbarian.WOTB.FuryDumpMin") && GetHasBuff(SNOPower.Barbarian_WrathOfTheBerserker)) ||
                        Settings.Combat.Barbarian.FuryDumpAlways && Player.PrimaryResourcePct >= V.F("Barbarian.WOTB.FuryDumpMin")
                    ) &&
                    Player.PrimaryResource >= V.F("Barbarian.BattleRage.MinFury");
            }
        }
        public static bool CanCastSprintOOC
        {
            get
            {
                return
                Settings.Combat.Barbarian.UseSprintOOC &&
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
                return
                    !UseOOCBuff &&
                    CanCast(SNOPower.Barbarian_FuriousCharge) &&
                    (TargetUtil.AnyElitesInRange(V.F("Barbarian.FuriousCharge.EliteRange"), V.I("Barbarian.FuriousCharge.EliteCount")) ||
                    TargetUtil.AnyElitesInRange(V.F("Barbarian.FuriousCharge.TrashRange"), V.I("Barbarian.FuriousCharge.TrashCount")));
            }
        }
        public static bool CanUseLeap
        {
            get
            {
                return
                    !UseOOCBuff &&
                    !Player.IsIncapacitated &&
                    CanCast(SNOPower.Barbarian_Leap) &&
                    (TargetUtil.AnyMobsInRange(V.F("Barbarian.Leap.TrashRange"), V.I("Barbarian.Leap.TrashCount")) ||
                    TargetUtil.AnyElitesInRange(V.F("Barbarian.Leap.EliteRange"), V.I("Barbarian.Leap.EliteCount")));
            }
        }
        public static bool CanUseRend
        {
            //skillDict.Add("Rend", SNOPower.Barbarian_Rend);
            //runeDict.Add("Ravage", 1);
            //runeDict.Add("BloodLust", 3);
            //runeDict.Add("Lacerate", 0);
            //runeDict.Add("Mutilate", 2);
            //runeDict.Add("Bloodbath", 4);
            get
            {
                bool hasReserveEnergy = (!IsWaitingForSpecial && Player.PrimaryResource >= V.I("Barbarian.Rend.MinFury")) || (IsWaitingForSpecial && Player.PrimaryResource > MinEnergyReserve);

                return
                    !UseOOCBuff &&
                    !Player.IsIncapacitated &&
                    hasReserveEnergy &&
                    ((CanCast(SNOPower.Barbarian_Rend)) &&
                        (Trinity.ObjectCache.Count(o => o.Type == GObjectType.Unit &&
                            !o.HasDotDPS && o.RadiusDistance <= V.F("Barbarian.Rend.MaxRange")) >= V.I("Barbarian.Rend.MinNonBleedMobCount"))
                     ||
                    // Spam with Bloodlust
                    (CanCast(SNOPower.Barbarian_Rend) &&
                     Player.CurrentHealthPct <= V.F("Barbarian.Rend.SpamBelowHealthPct") &&
                     HotbarSkills.AssignedSkills.Any(s => s.Power == SNOPower.Barbarian_Rend && s.RuneIndex == 3) &&
                     TargetUtil.AnyMobsInRange(V.F("Barbarian.Rend.MaxRange"), false)
                    ))
                    ;
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
                        (CurrentTarget.IsTreasureGoblin && CurrentTarget.CentreDistance <= V.F("Barbarian.OverPower.MaxRange")) || Hotbar.Contains(SNOPower.Barbarian_SeismicSlam))
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
                    Player.PrimaryResource >= V.I("Barbarian.SeismicSlam.MinFury") && CurrentTarget.CentreDistance <= V.F("Barbarian.SeismicSlam.CurrentTargetRange") &&
                    (TargetUtil.AnyMobsInRange(V.F("Barbarian.SeismicSlam.TrashRange")) ||
                     TargetUtil.IsEliteTargetInRange(V.F("Barbarian.SeismicSlam.EliteRange")));
            }
        }
        public static bool CanUseAncientSpear
        {
            get
            {
                return !UseOOCBuff && !IsCurrentlyAvoiding && CanCast(SNOPower.X1_Barbarian_AncientSpear) && Player.PrimaryResource >= 25 &&
                    CurrentTarget.HitPointsPct >= V.F("Barbarian.AncientSpear.MinHealthPct");
            }
        }
        public static bool CanUseSprint
        {
            get
            {
                return !UseOOCBuff && CanCast(SNOPower.Barbarian_Sprint, CanCastFlags.NoTimer) && !Player.IsIncapacitated &&
                    (
                    // last power used was whirlwind and we don't have sprint up
                        (LastPowerUsed == SNOPower.Barbarian_Whirlwind && !GetHasBuff(SNOPower.Barbarian_Sprint)) ||
                    // Fury Dump Options for sprint: use at max energy constantly
                        (Settings.Combat.Barbarian.FuryDumpWOTB && Player.PrimaryResourcePct >= V.F("Barbarian.WOTB.FuryDumpMin") && GetHasBuff(SNOPower.Barbarian_WrathOfTheBerserker)) ||
                        (Settings.Combat.Barbarian.FuryDumpAlways && Player.PrimaryResourcePct >= V.F("Barbarian.WOTB.FuryDumpMin")) ||
                    // or on a timer
                        (
                         (SNOPowerUseTimer(SNOPower.Barbarian_Sprint) && !GetHasBuff(SNOPower.Barbarian_Sprint)) &&
                    // Always keep up if we are whirlwinding, if the target is a goblin, or if we are more than 16 feet away from the target
                         (Hotbar.Contains(SNOPower.Barbarian_Whirlwind) || CurrentTarget.IsTreasureGoblin ||
                          (CurrentTarget.CentreDistance >= V.F("Barbarian.Sprint.SingleTargetRange") && Player.PrimaryResource >= V.F("Barbarian.Sprint.SingleTargetMinFury"))
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

                bool hasPunish = HotbarSkills.AssignedSkills.Any(s => s.Power == SNOPower.Barbarian_Bash && s.RuneIndex == 1);

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
                return !UseOOCBuff && !IsCurrentlyAvoiding && Hotbar.Contains(SNOPower.Barbarian_Whirlwind) && !Player.IsIncapacitated && !Player.IsRooted && Player.PrimaryResource >= 10 &&
                    (!IsWaitingForSpecial || (IsWaitingForSpecial && Player.PrimaryResource > MinEnergyReserve)) &&
                    //(!IsWaitingForSpecial || (IsWaitingForSpecial && !(TargetUtil.AnyMobsInRange(3, 15) || ForceCloseRangeTarget))) && // make sure we're not surrounded if waiting for special
                    // Don't WW against goblins, units in the special SNO list
                    (!Settings.Combat.Barbarian.SelectiveWhirlwind || (Settings.Combat.Barbarian.SelectiveWhirlwind && !DataDictionary.WhirlwindIgnoreSNOIds.Contains(CurrentTarget.ActorSNO))) &&
                    // Only if within 25 yards of main target
                    ((CurrentTarget.RadiusDistance <= 25f || TargetUtil.AnyMobsInRange(V.F("Barbarian.Whirlwind.TrashRange"), V.I("Barbarian.Whirlwind.TrashCount")))) &&
                    (TargetUtil.AnyMobsInRange(50, 2) || CurrentTarget.HitPointsPct >= 0.30 || CurrentTarget.IsBossOrEliteRareUnique || Player.CurrentHealthPct <= 0.60) &&
                    // Check for energy reservation amounts
                    //((playerStatus.dCurrentEnergy >= 20 && !playerStatus.bWaitingForReserveEnergy) || playerStatus.dCurrentEnergy >= iWaitingReservedAmount) &&
                    Player.PrimaryResource >= V.D("Barbarian.Whirlwind.MinFury") &&
                    // If they have battle-rage, make sure it's up
                    (!Hotbar.Contains(SNOPower.Barbarian_BattleRage) || (Hotbar.Contains(SNOPower.Barbarian_BattleRage) && GetHasBuff(SNOPower.Barbarian_BattleRage)));
            }
        }
        public static bool CanUseBattleRage
        {
            get
            {
                return !UseOOCBuff && Hotbar.Contains(SNOPower.Barbarian_BattleRage) && !Player.IsIncapacitated &&
                    // Fury Dump Options for battle rage IF they don't have sprint 
                        (
                        (Settings.Combat.Barbarian.FuryDumpWOTB && Player.PrimaryResourcePct >= V.F("Barbarian.WOTB.FuryDumpMin") && GetHasBuff(SNOPower.Barbarian_WrathOfTheBerserker)) ||
                        (Settings.Combat.Barbarian.FuryDumpAlways && Player.PrimaryResourcePct >= V.F("Barbarian.WOTB.FuryDumpMin")) || !GetHasBuff(SNOPower.Barbarian_BattleRage)
                        ) &&
                        Player.PrimaryResource >= 20 && PowerManager.CanCast(SNOPower.Barbarian_BattleRage);
            }
        }
        // Dreadbomb build support
        public static bool CanUseDreadbomb
        {
            get
            {
                return !UseOOCBuff && !IsCurrentlyAvoiding && Hotbar.Contains(SNOPower.X1_Barbarian_WeaponThrow)
                    && Player.PrimaryResourcePct >= 0.99 && (HotbarSkills.AssignedSkills.Any(s => s.Power == SNOPower.X1_Barbarian_WeaponThrow && s.RuneIndex == 3));
            }
        }
        public static bool CanUseHammerOfTheAncients
        {
            get
            {
                return !UseOOCBuff && !IsCurrentlyAvoiding && !Player.IsIncapacitated && !IsWaitingForSpecial && CanCast(SNOPower.Barbarian_HammerOfTheAncients) &&
                    Player.PrimaryResource >= 20 && Player.CurrentHealthPct >= Settings.Combat.Barbarian.MinHotaHealth;
            }
        }
        public static bool CanUseHammerOfTheAncientsElitesOnly
        {
            get
            {
                bool canUseHota = CanUseHammerOfTheAncients;

                if (canUseHota)
                {
                    bool hotaElites = (CurrentTarget.IsBossOrEliteRareUnique || CurrentTarget.IsTreasureGoblin) && !TargetUtil.AnyMobsInRange(10f, 3, false);

                    bool hotaTrash = CombatBase.IgnoringElites && CurrentTarget.IsTrashMob &&
                        (Trinity.ObjectCache.Count(u => u.Position.Distance(CurrentTarget.Position) <= 6f) >= 3 || CurrentTarget.MonsterSize == Zeta.Game.Internals.SNO.MonsterSize.Big);

                    return canUseHota && (hotaElites || hotaTrash);
                }
                return false;
            }
        }
        public static bool CanUseWeaponThrow
        {
            get
            {
                return !UseOOCBuff && !IsCurrentlyAvoiding && Hotbar.Contains(SNOPower.X1_Barbarian_WeaponThrow);
            }
        }
        public static bool CanUseFrenzy { get { return !UseOOCBuff && !IsCurrentlyAvoiding && Hotbar.Contains(SNOPower.Barbarian_Frenzy) && PowerManager.CanCast(SNOPower.Barbarian_Frenzy); } }
        public static bool CanUseBash { get { return !UseOOCBuff && !IsCurrentlyAvoiding && Hotbar.Contains(SNOPower.Barbarian_Bash) && PowerManager.CanCast(SNOPower.Barbarian_Bash); } }
        public static bool CanUseCleave { get { return !UseOOCBuff && !IsCurrentlyAvoiding && Hotbar.Contains(SNOPower.Barbarian_Cleave) && PowerManager.CanCast(SNOPower.Barbarian_Cleave); } }

        public static TrinityPower PowerIgnorePain { get { return new TrinityPower(SNOPower.Barbarian_IgnorePain); } }
        public static TrinityPower PowerEarthquake { get { return new TrinityPower(SNOPower.Barbarian_Earthquake); } }
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
                float extraDistance;
                if (CurrentTarget.CentreDistance <= 25)
                    extraDistance = 30;
                else
                    extraDistance = (25 - CurrentTarget.CentreDistance);
                if (extraDistance < V.F("Barbarian.FuriousCharge.MinExtraTargetDistance"))
                    extraDistance = V.F("Barbarian.FuriousCharge.MinExtraTargetDistance");
                Vector3 vNewTarget = MathEx.CalculatePointFrom(CurrentTarget.Position, Player.Position, CurrentTarget.CentreDistance + extraDistance);
                return new TrinityPower(SNOPower.Barbarian_FuriousCharge, V.F("Barbarian.FuriousCharge.UseRange"), vNewTarget);
            }
        }
        public static TrinityPower PowerLeap
        {
            get
            {
                float extraDistance = CurrentTarget.Radius;
                if (extraDistance <= V.F("Barbarian.Leap.MinExtraDistance"))
                    extraDistance = V.F("Barbarian.Leap.MinExtraDistance");
                if (CurrentTarget.CentreDistance + extraDistance > 35f)
                    extraDistance = 35 - CurrentTarget.CentreDistance;

                Vector3 vNewTarget = MathEx.CalculatePointFrom(CurrentTarget.Position, Player.Position, CurrentTarget.CentreDistance + extraDistance);
                return new TrinityPower(SNOPower.Barbarian_Leap, V.F("Barbarian.Leap.UseRange"), vNewTarget);
            }
        }
        public static TrinityPower PowerRend { get { return new TrinityPower(SNOPower.Barbarian_Rend, V.I("Barbarian.Rend.TickDelay"), V.I("Barbarian.Rend.TickDelay")); } }
        public static TrinityPower PowerOverpower { get { return new TrinityPower(SNOPower.Barbarian_Overpower); } }
        public static TrinityPower PowerSeismicSlam { get { return new TrinityPower(SNOPower.Barbarian_SeismicSlam, V.F("Barbarian.SeismicSlam.UseRange"), CurrentTarget.ACDGuid); } }
        public static TrinityPower PowerAncientSpear { get { return new TrinityPower(SNOPower.X1_Barbarian_AncientSpear, V.F("Barbarian.AncientSpear.UseRange"), CurrentTarget.Position); } }
        public static TrinityPower PowerWhirlwind
        {
            get
            {
                bool shouldGetNewZigZag =
                    (DateTime.Now.Subtract(Trinity.LastChangedZigZag).TotalMilliseconds >= V.I("Barbarian.Whirlwind.ZigZagMaxTime") ||
                    CurrentTarget.ACDGuid != Trinity.LastZigZagUnitAcdGuid ||
                    ZigZagPosition.Distance2D(Player.Position) <= 5f);

                if (shouldGetNewZigZag)
                {
                    var wwdist = V.F("Barbarian.Whirlwind.ZigZagDistance");

                    ZigZagPosition = TargetUtil.GetZigZagTarget(CurrentTarget.Position, wwdist);

                    Trinity.LastZigZagUnitAcdGuid = CurrentTarget.ACDGuid;
                    Trinity.LastChangedZigZag = DateTime.Now;
                }
                return new TrinityPower(SNOPower.Barbarian_Whirlwind, V.F("Barbarian.Whirlwind.UseRange"), ZigZagPosition, Trinity.CurrentWorldDynamicId, -1, 0, 1, false);
            }
        }
        public static TrinityPower PowerHammerOfTheAncients { get { return new TrinityPower(SNOPower.Barbarian_HammerOfTheAncients, V.F("Barbarian.HammerOfTheAncients.UseRange"), CurrentTarget.ACDGuid); } }
        public static TrinityPower PowerWeaponThrow { get { return new TrinityPower(SNOPower.X1_Barbarian_WeaponThrow, V.F("Barbarian.WeaponThrow.UseRange"), CurrentTarget.ACDGuid); } }
        public static TrinityPower PowerFrenzy { get { return new TrinityPower(SNOPower.Barbarian_Frenzy, V.F("Barbarian.Frenzy.UseRange"), CurrentTarget.ACDGuid); } }
        public static TrinityPower PowerBash { get { return new TrinityPower(SNOPower.Barbarian_Bash, V.F("Barbarian.Bash.UseRange"), Vector3.Zero, -1, CurrentTarget.ACDGuid, 2, 2, true); } }
        public static TrinityPower PowerCleave { get { return new TrinityPower(SNOPower.Barbarian_Cleave, V.F("Barbarian.Cleave.UseRange"), CurrentTarget.ACDGuid); } }

        private static TrinityPower DestroyObjectPower
        {
            get
            {
                if (Hotbar.Contains(SNOPower.Barbarian_Whirlwind) && Player.PrimaryResource > MinEnergyReserve)
                    return new TrinityPower(SNOPower.Barbarian_Whirlwind, V.F("Barbarian.Whirlwind.UseRange"), LastZigZagLocation);

                if (Hotbar.Contains(SNOPower.Barbarian_Frenzy))
                    return new TrinityPower(SNOPower.Barbarian_Frenzy, V.F("Barbarian.Frenzy.UseRange"));

                if (Hotbar.Contains(SNOPower.Barbarian_Bash))
                    return new TrinityPower(SNOPower.Barbarian_Bash, V.F("Barbarian.Bash.UseRange"));

                if (Hotbar.Contains(SNOPower.Barbarian_Cleave))
                    return new TrinityPower(SNOPower.Barbarian_Cleave, V.F("Barbarian.Cleave.UseRange"));

                if (Hotbar.Contains(SNOPower.Barbarian_Rend) && Player.PrimaryResourcePct >= 0.65)
                    return new TrinityPower(SNOPower.Barbarian_Rend, V.F("Barbarian.Rend.UseRange"));

                if (Hotbar.Contains(SNOPower.X1_Barbarian_WeaponThrow) && Player.PrimaryResource >= 20)
                    return new TrinityPower(SNOPower.X1_Barbarian_WeaponThrow, V.F("Barbarian.WeaponThrow.UseRange"));

                return CombatBase.DefaultPower;
            }
        }

        public static bool AllowSprintOOC
        {
            get { return allowSprintOOC; }
            set { allowSprintOOC = value; }
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

    }
}
