using System.Linq;
using Trinity.Config.Combat;
using Trinity.Reference;
using Zeta.Common;
using Zeta.Game.Internals.Actors;

namespace Trinity.Combat.Abilities
{
    class BarbarianCombat : CombatBase
    {
        private static bool _allowSprintOoc = true;
        private const float MaxFuriousChargeDistance = 300f;

        static BarbarianCombat()
        {
            SkillUtils.SetSkillMeta(SkillsDefaultMeta.Barbarian.ToList());
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

                // Refresh Bash - Punish
                if (IsNull(power) && CanCast(SNOPower.Barbarian_Bash) && TimeSincePowerUse(SNOPower.Barbarian_Bash) > 4000 && TimeSincePowerUse(SNOPower.Barbarian_Bash) < 5000)
                    power = PowerBash;
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
        
        public static bool CanCastIgnorePain
        {
            get
            {
                if (UseOOCBuff)
                    return false;

                if (!CanCast(SNOPower.Barbarian_IgnorePain))
                    return false;

                if (Settings.Combat.Barbarian.IgnorePainOffCooldown)
                    return true;

                if (Player.CurrentHealthPct <= V.F("Barbarian.IgnorePain.MinHealth"))
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
                    //Player.PrimaryResource >= V.I("Barbarian.WOTB.MinFury") && // WOTB is "free" !
                    // Don't still have the buff
                    !hasBuff && CanCast(SNOPower.Barbarian_WrathOfTheBerserker) &&
                    (WOTBGoblins || WOTBIgnoreElites || WOTBElitesPresent || emergencyHealth || hasInfiniteCasting || anyTime);

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
                return !UseOOCBuff && !IsCurrentlyAvoiding && !Player.IsRooted && Hotbar.Contains(SNOPower.Barbarian_Bash) &&
                    Runes.Barbarian.Punish.IsActive && !TargetUtil.AnyMobsInRange(15f, 3) && GetBuffStacks(SNOPower.Barbarian_Bash) < 3;
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
        public static TrinityPower PowerBash { get { return new TrinityPower(SNOPower.Barbarian_Bash, V.F("Barbarian.Bash.UseRange"), CurrentTarget.ACDGuid); } }
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
                    return new TrinityPower(SNOPower.Barbarian_Whirlwind, V.F("Barbarian.Whirlwind.UseRange"), CurrentTarget.Position);

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
