using System;
using System.Linq;
using Trinity.Config.Combat;
using Trinity.DbProvider;
using Trinity.Reference;
using Trinity.Technicals;
using Zeta.Common;
using Zeta.Game;
using Zeta.Game.Internals.Actors;

namespace Trinity.Combat.Abilities
{
    class BarbarianCombat : CombatBase
    {
        public static bool CurrentlyUseFuriousCharge
        {
            get
            {
                return Player.ActorClass == ActorClass.Barbarian && Hotbar.Contains(SNOPower.Barbarian_FuriousCharge) &&
                    CombatBase.TimeSincePowerUse(SNOPower.Barbarian_FuriousCharge) < 350;
            }
        }

        private static bool _allowSprintOoc = true;
        private const float maxFuriousChargeDistance = 40f;

        public static TrinityPower GetPower()
        {
            TrinityPower power = null;

            if (Sets.ImmortalKingsCall.IsMaxBonusActive && Skills.Barbarian.Whirlwind.IsActive)
            {
                power = SpamPowerWhirlwind;
            }

            if (IsNull(power) && UseDestructiblePower)
                power = DestroyObjectPower;

            if (UseOOCBuff)
            {
                // Call of The Ancients
                if (IsNull(power) && CanUseCallOfTheAncients && Sets.ImmortalKingsCall.IsFullyEquipped)
                    power = PowerCallOfTheAncients;

                // Sprint OOC
                if (IsNull(power) && CanUseSprintOOC)
                    power = PowerSprint;
                // Threatening Shout OOC
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

            IsWaitingForSpecial = false;

            // Check if we should conserve Fury for specials
            if (IsNull(power) && Player.PrimaryResource < MinEnergyReserve)
            {
                if (ShouldWaitForCallOfTheAncients)
                {
                    //Logger.LogNormal("Waiting for Barbarian_CallOfTheAncients!");
                    IsWaitingForSpecial = true;
                }
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
            }

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

            // Furious Charge
            if (IsNull(power))
                power = PowerFuriousCharge;

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

            // The Legacy Of Raekor Kite Logic
            if (IsNull(power) && CurrentTarget != null && 
                Sets.TheLegacyOfRaekor.IsMaxBonusActive && Skills.Barbarian.FuriousCharge.IsActive)
            {
                var _kiteNode = GridMap.GetBestMoveNode(20f, prioritizeDist: true);

                if (IsNull(power) && _kiteNode != null && TargetUtil.NumMobsInLosInRangeOfPosition(CurrentTarget.Position, 35f) == 1 &&
                    !(CurrentTarget.IsTreasureGoblin && Trinity.Settings.Combat.Misc.GoblinPriority >= GoblinPriority.Prioritize) &&
                    !CurrentTarget.IsBossOrEliteRareUnique)
                {
                    Trinity.Blacklist15Seconds.Add(CurrentTarget.RActorGuid);
                    power = new TrinityPower(SNOPower.Walk, 0f, _kiteNode.Position);
                }

                if (IsNull(power) && _kiteNode != null && (TargetUtil.AnyMobsInRange(30f, 2) || (CurrentTarget.IsBoss && CurrentTarget.Distance <= 35f)))
                    power = new TrinityPower(SNOPower.Walk, 0f, _kiteNode.Position);

                if (!IsNull(power))
                {
                    Trinity.Player.NeedToKite = true;
                    CombatBase.QueuedMovement.Queue(new QueuedMovement
                    {
                        Name = CurrentTarget.InternalName,
                        Infos = "(Barbarian Kite) " + CurrentTarget.Infos + " WeightInfos: " + GridMap.GetBestMoveNode(20f, prioritizeDist: true).WeightInfos,
                        Destination = power.TargetPosition,
                        StopCondition = m =>
                            m.Destination == Vector3.Zero ||
                            m.Destination.Distance2D(Trinity.Player.Position) <= 1f ||
                            CurrentTarget == null ||
                            CombatBase.IsNull(CombatBase.CurrentPower) || CurrentTarget == null || CurrentTarget.IsAvoidance ||
                            !CurrentTarget.IsUnit || CombatBase.CurrentPower.SNOPower != SNOPower.Walk
                        ,
                        Options = new QueuedMovementOptions
                        {
                            Logging = LogLevel.Info,
                            AcceptableDistance = 3f,
                            Type = MoveType.SpecialCombat,
                        }
                    });
                }
            }

            // Default Attacks
            if (IsNull(power) && !IsCurrentlyAvoiding)
                power = DefaultPower;

            return power;
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

                return CanCast(SNOPower.Barbarian_IgnorePain, CanCastFlags.NoTimer) && Sets.TheLegacyOfRaekor.IsFullyEquipped && ShouldFuryDump;
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
                    TargetUtil.AnyMobsInRange(V.F("Barbarian.CallOfTheAncients.MinEliteRange"), 3) &&
                    !GetHasBuff(SNOPower.Barbarian_CallOfTheAncients) &&
                    Player.PrimaryResource <= V.F("Barbarian.CallOfTheAncients.MinFury");
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
                    CurrentTarget.IsBossOrEliteRareUnique && CurrentTarget.Distance <= 15f ||
                    TargetUtil.EliteOrTrashInRange(15f) ||
                    TargetUtil.AnyMobsInRange(15f, 4) ||
                    TargetUtil.AnyElitesInRange(15f));
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
                bool canCast = CanCast(SNOPower.Barbarian_WrathOfTheBerserker, CanCastFlags.NoTimer);

                bool emergencyHealth = Player.CurrentHealthPct <= V.F("Barbarian.WOTB.EmergencyHealth");

                bool result =
                    (!UseOOCBuff || anyTime) &&
                    //Player.PrimaryResource >= V.I("Barbarian.WOTB.MinFury") && // WOTB is "free" !
                    // Don't still have the buff
                    !hasBuff &&
                    canCast &&
                    (WOTBGoblins || WOTBIgnoreElites || WOTBElitesPresent || anyTime || emergencyHealth || hasInfiniteCasting);

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

                float _range = CanCast(SNOPower.Barbarian_FuriousCharge) ? 40f : 15f;
                bool hardElitesOnly = Settings.Combat.Barbarian.WOTBMode == BarbarianWOTBMode.HardElitesOnly;

                bool elitesPresent = TargetUtil.AnyElitesInRange(_range, V.I("Barbarian.WOTB.MinCount"));

                bool _bigAoePresent = TargetUtil.AnyMobsInRange(15f, 6) || TargetUtil.AnyMobsInRange(45f, 15);

                return ((!hardElitesOnly && elitesPresent) || (hardElitesOnly && HardElitesPresent) || _bigAoePresent);
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

        public static bool ShouldWaitForEarthquake
        {
            get
            {
                return
                    Hotbar.Contains(SNOPower.Barbarian_Earthquake) &&
                    !UseOOCBuff &&
                    !IsCurrentlyAvoiding &&
                    !CanCast(SNOPower.Barbarian_Earthquake) &&
                    TargetUtil.EliteOrTrashInRange(45) &&
                    Player.PrimaryResource <= 50;
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
                    (!GetHasBuff(SNOPower.Barbarian_BattleRage) || ShouldFuryDump);
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
                    (!GetHasBuff(SNOPower.X1_Barbarian_WarCry_v2) || (Legendary.ChilaniksChain.IsEquipped && TimeSincePowerUse(SNOPower.X1_Barbarian_WarCry_v2) >= 9500));
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
                        TargetUtil.AnyElitesInRange(V.F("Barbarian.GroundStomp.EliteRange"), V.I("Barbarian.GroundStomp.EliteCount")) || CurrentTarget.IsBossOrEliteRareUnique ||
                        TargetUtil.AnyMobsInRange(V.F("Barbarian.GroundStomp.TrashRange"), V.I("Barbarian.GroundStomp.TrashCount")) || CurrentTarget.NearbyUnits >= 3 || 
                        TargetUtil.ClusterExists(14f, 3) ||
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
        public static TrinityPower PowerFuriousCharge
        {
            get
            {
                if (CombatBase.IsCombatAllowed && (CanCast(SNOPower.Barbarian_FuriousCharge) || 
                    (Skills.Barbarian.FuriousCharge.IsActive && Sets.TheLegacyOfRaekor.IsMaxBonusActive)))
                {
                    var _moveNode = TargetUtil.GetBestFuriousChargeMoveNode(maxFuriousChargeDistance);
                    var _castNode = TargetUtil.GetBestFuriousChargeNode(maxFuriousChargeDistance);

                    if (CanCast(SNOPower.Barbarian_FuriousCharge))
                    {
                        // Cast without moving
                        if (_castNode != null && (_moveNode == null || _moveNode.SpecialWeight <= _castNode.SpecialWeight ||
                            !(_moveNode.SpecialWeight > _castNode.SpecialWeight && _moveNode.Distance <= 7f && _moveNode.Distance > 3f)))
                            return new TrinityPower(SNOPower.Barbarian_FuriousCharge, 0f, _castNode.Position);

                        _castNode = _moveNode != null ? TargetUtil.GetBestFuriousChargeNode(maxFuriousChargeDistance, _moveNode.Position) : null;

                        // New trinity power with a move position & a target position
                        if (_moveNode != null && _castNode != null)
                            return new TrinityPower(SNOPower.Barbarian_FuriousCharge, 4f, _moveNode.Position, _castNode.Position);
                    }

                    var _closestTarget = TargetUtil.GetClosestTarget(maxFuriousChargeDistance, _useWeights: false);
                    _moveNode = _closestTarget != default(TrinityCacheObject) ? TargetUtil.GetBestFuriousChargeMoveNode(maxFuriousChargeDistance, _closestTarget.Position) : null;
                    _castNode = _moveNode != null ? TargetUtil.GetBestFuriousChargeNode(maxFuriousChargeDistance, _moveNode.Position) : null;

                    // New trinity power with a move position & a target position
                    if (_moveNode != null && _castNode != null)
                        return new TrinityPower(SNOPower.Barbarian_FuriousCharge, 4f, _moveNode.Position, _castNode.Position);

                    /*_moveNode = TargetUtil.GetBestFuriousChargeMoveNode(maxFuriousChargeDistance, _useFcWeights: false);
                    _castNode = _moveNode != null && _moveNode.SpecialWeight > 0 ? TargetUtil.GetBestFuriousChargeNode(maxFuriousChargeDistance, _moveNode.Position) : null;

                    // New trinity power with a move position & a target position
                    if (_moveNode != null && _castNode != null)
                        return new TrinityPower(SNOPower.Barbarian_FuriousCharge, 4f, _moveNode.Position, _castNode.Position);*/

                    if (CanCast(SNOPower.Barbarian_FuriousCharge) && CurrentTarget != null && CurrentTarget.IsPlayerFacing(20f) && CurrentTarget.Distance < 10f)
                        return new TrinityPower(SNOPower.Barbarian_FuriousCharge, 4f, CurrentTarget.Position);

                    var _kiteNode = GridMap.GetBestMoveNode(15f, 40f);
                    if (IsCurrentlyAvoiding && CanCast(SNOPower.Barbarian_FuriousCharge) && Player.CurrentHealthPct <= 0.6 && _kiteNode != null)
                        return new TrinityPower(SNOPower.Barbarian_FuriousCharge, 0f, _kiteNode.Position);
                }

                return null;
            }
        }
        public static bool CanUseLeap
        {
            get
            {
                bool leapresult = !UseOOCBuff &&
                        !Player.IsIncapacitated &&
                        CanCast(SNOPower.Barbarian_Leap);
                if (Legendary.LutSocks.IsEquipped) // This will now cast whenever leap is available and an enemy is around. Disable Leap OOC option. The last line will prevent you from leaping on destructibles
                {
                    return leapresult && TargetUtil.AnyMobsInRange(15f, 1);
                }
                else
                {
                    return leapresult && (TargetUtil.ClusterExists(15f, 35f, V.I("Barbarian.Leap.TrashCount")) || CurrentTarget.IsBossOrEliteRareUnique);
                }
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
                        (Trinity.ObjectCache.Count(o => o.IsUnit &&
                            !o.HasDotDPS && o.RadiusDistance <= V.F("Barbarian.Rend.MaxRange")) >= V.I("Barbarian.Rend.MinNonBleedMobCount"))
                     ||
                    // Spam with Bloodlust
                    (CanCast(SNOPower.Barbarian_Rend) &&
                     Player.CurrentHealthPct <= V.F("Barbarian.Rend.SpamBelowHealthPct") &&
                     CacheData.Hotbar.ActiveSkills.Any(s => s.Power == SNOPower.Barbarian_Rend && s.RuneIndex == 3) &&
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
                if (CanCast(SNOPower.Barbarian_Sprint, CanCastFlags.NoTimer) && IsCurrentlyAvoiding)
                    return true;

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
                    TargetUtil.AnyMobsInRange(15f) && GetBuffStacks(SNOPower.Barbarian_Frenzy) < 5;
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
                return !UseOOCBuff && !IsCurrentlyAvoiding && Hotbar.Contains(SNOPower.Barbarian_Whirlwind) && !Player.IsIncapacitated && !Player.IsRooted && Player.PrimaryResource >= 10 &&
                    (!IsWaitingForSpecial || (IsWaitingForSpecial && Player.PrimaryResource > MinEnergyReserve)) &&
                    //(!IsWaitingForSpecial || (IsWaitingForSpecial && !(TargetUtil.AnyMobsInRange(3, 15) || ForceCloseRangeTarget))) && // make sure we're not surrounded if waiting for special
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
        // Dreadbomb build support
        public static bool CanUseDreadbomb
        {
            get
            {
                return !UseOOCBuff && !IsCurrentlyAvoiding && Hotbar.Contains(SNOPower.X1_Barbarian_WeaponThrow)
                    && Player.PrimaryResourcePct >= 0.99 && (CacheData.Hotbar.ActiveSkills.Any(s => s.Power == SNOPower.X1_Barbarian_WeaponThrow && s.RuneIndex == 3));
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


        public static TrinityPower PowerAvalanche { get { return new TrinityPower(SNOPower.X1_Barbarian_Avalanche_v2, 15f, TargetUtil.GetBestClusterPoint(15f, 45f)); } }
        public static TrinityPower PowerIgnorePain { get { return new TrinityPower(SNOPower.Barbarian_IgnorePain); } }
        public static TrinityPower PowerEarthquake { get { return new TrinityPower(SNOPower.Barbarian_Earthquake); } }
        public static TrinityPower PowerWrathOfTheBerserker { get { return new TrinityPower(SNOPower.Barbarian_WrathOfTheBerserker); } }
        public static TrinityPower PowerCallOfTheAncients { get { return new TrinityPower(SNOPower.Barbarian_CallOfTheAncients, V.I("Barbarian.CallOfTheAncients.TickDelay"), V.I("Barbarian.CallOfTheAncients.TickDelay")); } }
        public static TrinityPower PowerBattleRage { get { return new TrinityPower(SNOPower.Barbarian_BattleRage); } }
        public static TrinityPower PowerSprint { get { return new TrinityPower(SNOPower.Barbarian_Sprint); } }
        public static TrinityPower PowerWarCry { get { return new TrinityPower(SNOPower.X1_Barbarian_WarCry_v2); } }
        public static TrinityPower PowerThreateningShout { get { return new TrinityPower(SNOPower.Barbarian_ThreateningShout); } }
        public static TrinityPower PowerRevenge { get { return new TrinityPower(SNOPower.Barbarian_Revenge); } }

        public static TrinityPower PowerGroundStomp 
        { 
            get 
            { 
                if (CurrentTarget.IsBossOrEliteRareUnique || CurrentTarget.NearbyUnits >= 3)
                    return new TrinityPower(SNOPower.Barbarian_GroundStomp, 12f, CurrentTarget.ClusterPosition(7f), CurrentTarget.ACDGuid); 

                return new TrinityPower(SNOPower.Barbarian_GroundStomp, 12f, TargetUtil.GetBestClusterPoint(9f)); 
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
                    Vector3 aoeTarget = TargetUtil.GetBestClusterPoint(7f, 9f);
                    return new TrinityPower(SNOPower.Barbarian_Leap, V.F("Barbarian.Leap.UseRange"), aoeTarget);
                }
                else
                {
                    Vector3 aoeTarget = TargetUtil.GetBestClusterPoint(15f, 35f);
                    return new TrinityPower(SNOPower.Barbarian_Leap, V.F("Barbarian.Leap.UseRange"), aoeTarget);
                }
            }
        }
        public static TrinityPower PowerRend { get { return new TrinityPower(SNOPower.Barbarian_Rend, V.I("Barbarian.Rend.TickDelay"), V.I("Barbarian.Rend.TickDelay")); } }
        public static TrinityPower PowerOverpower { get { return new TrinityPower(SNOPower.Barbarian_Overpower); } }
        public static TrinityPower PowerSeismicSlam { get { return new TrinityPower(SNOPower.Barbarian_SeismicSlam, V.F("Barbarian.SeismicSlam.UseRange"), CurrentTarget.ClusterPosition(V.F("Barbarian.SeismicSlam.UseRange") -3f), CurrentTarget.ACDGuid); } }
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
                bool shouldGetNewZigZag =
                    (DateTime.UtcNow.Subtract(LastChangedZigZag).TotalMilliseconds >= V.I("Barbarian.Whirlwind.ZigZagMaxTime") ||
                    CurrentTarget.ACDGuid != LastZigZagUnitAcdGuid ||
                    ZigZagPosition.Distance2D(Player.Position) <= 5f);
                bool hasRLTW = CacheData.Hotbar.ActiveSkills.Any(s => s.Power == SNOPower.Barbarian_Sprint && s.RuneIndex == 2);

                if (shouldGetNewZigZag)
                {
                    LastZigZagUnitAcdGuid = CurrentTarget.ACDGuid;
                    LastChangedZigZag = DateTime.UtcNow;

                    if (hasRLTW)
                    {
                        var wwdist = V.F("Barbarian.Whirlwind.RLTWZigZag");
                        ZigZagPosition = TargetUtil.GetZigZagTarget(CurrentTarget.Position, wwdist);
                        return new TrinityPower(SNOPower.Barbarian_Whirlwind, V.F("Barbarian.Whirlwind.UseRange"), ZigZagPosition);
                    }
                    else
                    {
                        var wwdist = V.F("Barbarian.Whirlwind.ZigZagDistance");
                        ZigZagPosition = TargetUtil.GetZigZagTarget(CurrentTarget.Position, wwdist);
                        return new TrinityPower(SNOPower.Barbarian_Whirlwind, V.F("Barbarian.Whirlwind.UseRange"), ZigZagPosition);
                    }

                   
                }
                return new TrinityPower(SNOPower.Barbarian_Whirlwind, V.F("Barbarian.Whirlwind.UseRange"), ZigZagPosition);
            }
        }
        public static TrinityPower SpamPowerWhirlwind
        {
            get
            {
                if (CanCast(SNOPower.Barbarian_Whirlwind, CanCastFlags.NoTimer) && 
                    !GetHasBuff(SNOPower.Barbarian_Whirlwind) && 
                    !Skills.Barbarian.Whirlwind.IsBuffActive && !Player.HasDebuff(SNOPower.Barbarian_Whirlwind))
                {
                    if (CurrentTarget != null && CurrentTarget.IsUnit && TargetUtil.AnyMobsInRange(15f, false))
                    {
                        bool shouldGetNewZigZag =
                        DateTime.UtcNow.Subtract(LastChangedZigZag).TotalMilliseconds >= 1000 ||
                        ZigZagPosition.Distance2D(Player.Position) <= 2f;

                        if (shouldGetNewZigZag)
                        {
                            LastChangedZigZag = DateTime.UtcNow;

                            ZigZagPosition = TargetUtil.GetZigZagTarget(CurrentTarget.Position, 15f);
                            return new TrinityPower(SNOPower.Barbarian_Whirlwind, 0f, ZigZagPosition);
                        }

                        return new TrinityPower(SNOPower.Barbarian_Whirlwind, 0f, ZigZagPosition);
                    }

                    if (TargetUtil.AnyMobsInRange(10f, false) && Player.MovementSpeed > 0 && PlayerMover.LastMoveToTarget != Vector3.Zero)
                        return new TrinityPower(SNOPower.Barbarian_Whirlwind, 0f, MathEx.GetPointAt(Player.Position, 15f, Player.Rotation));
                }

                return null;
            }
        }
        public static TrinityPower PowerHammerOfTheAncients 
        { 
            get { return new TrinityPower(SNOPower.Barbarian_HammerOfTheAncients, V.F("Barbarian.HammerOfTheAncients.UseRange"), CurrentTarget.ClusterPosition(V.F("Barbarian.HammerOfTheAncients.UseRange") -3f), CurrentTarget.ACDGuid); } 
        }
        public static TrinityPower PowerWeaponThrow 
        {
            get { return new TrinityPower(SNOPower.X1_Barbarian_WeaponThrow, V.F("Barbarian.WeaponThrow.UseRange"), CurrentTarget.ClusterPosition(V.F("Barbarian.WeaponThrow.UseRange") - 3f), CurrentTarget.ACDGuid); } 
        }
        public static TrinityPower PowerFrenzy 
        {
            get { return new TrinityPower(SNOPower.Barbarian_Frenzy, V.F("Barbarian.Frenzy.UseRange"), CurrentTarget.ClusterPosition(V.F("Barbarian.Frenzy.UseRange") - 3f), CurrentTarget.ACDGuid); } 
        }
        public static TrinityPower PowerBash 
        {
            get { return new TrinityPower(SNOPower.Barbarian_Bash, V.F("Barbarian.Bash.UseRange"), CurrentTarget.ClusterPosition(V.F("Barbarian.Bash.UseRange") - 3f), CurrentTarget.ACDGuid); } 
        }
        public static TrinityPower PowerCleave 
        {
            get { return new TrinityPower(SNOPower.Barbarian_Cleave, V.F("Barbarian.Cleave.UseRange"), CurrentTarget.ClusterPosition(V.F("Barbarian.Cleave.UseRange") - 3f), CurrentTarget.ACDGuid); } 
        }

        private static TrinityPower DestroyObjectPower
        {
            get
            {
                if (CanCast(SNOPower.Barbarian_FuriousCharge, CanCastFlags.NoTimer))
                    return new TrinityPower(SNOPower.Barbarian_FuriousCharge, maxFuriousChargeDistance, CurrentTarget.ACDGuid);

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
                    return new TrinityPower(SNOPower.Barbarian_Whirlwind, 10f, CurrentTarget.ACDGuid);

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
                bool ignoranceIsBliss = Runes.Barbarian.IgnoranceIsBliss.IsActive && 
                    GetHasBuff(SNOPower.Barbarian_IgnorePain) && Trinity.Player.CurrentHealthPct <= 1;

                return ((berserkerRage || ignoranceIsBliss) && GetHasBuff(SNOPower.Barbarian_WrathOfTheBerserker) &&
                        ((Settings.Combat.Barbarian.FuryDumpWOTB && Player.PrimaryResourcePct >= V.F("Barbarian.WOTB.FuryDumpMin")) ||
                         Settings.Combat.Barbarian.FuryDumpAlways && Player.PrimaryResourcePct >= V.F("Barbarian.WOTB.FuryDumpMin")));
            }
        }


    }
}
