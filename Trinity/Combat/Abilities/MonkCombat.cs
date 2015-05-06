using System;
using System.Linq;
using Trinity.Config.Combat;
using Trinity.DbProvider;
using Trinity.Reference;
using Trinity.Technicals;
using Zeta.Bot;
using Zeta.Bot.Logic;
using Zeta.Bot.Profile.Common;
using Zeta.Common;
using Zeta.Game;
using Zeta.Game.Internals.Actors;
using Logger = Trinity.Technicals.Logger;

namespace Trinity.Combat.Abilities
{
    public class MonkCombat : CombatBase
    {
        private const float MaxDashingStrikeRange = 55f;
        internal static Vector3 LastTempestRushLocation = Vector3.Zero;
        private static DateTime _lastTargetChange = DateTime.MinValue;
        internal static DateTime LastSweepingWindRefresh = DateTime.MinValue;

        static int _swMinTime = 4000;
        const int SwMaxTime = 5400;
        const int SwMaxTaegukTime = 2950;

        static bool _hasInnaSet;
        static bool _hasSwk;
        static float _minSweepingWindSpirit;

        public static MonkSetting MonkSettings
        {
            get { return Trinity.Settings.Combat.Monk; }
        }

        public static TrinityPower GetPower()
        {
            TrinityPower power;

            ParameterSetup();

            // Sweeping Winds Refresh
            if (ShouldRefreshSweepingWinds())
            {
                // LastSweepingWindRefresh = DateTime.UtcNow; // Now set through HandleTarget primary spell usage
                return new TrinityPower(SNOPower.Monk_SweepingWind);
            }

            // Destructible objects
            if (UseDestructiblePower)
            {
                return GetMonkDestroyPower();
            }

            if (UseOOCBuff)
            {
                // Mystic ally
                if (CanCastMysticAlly())
                {
                    return new TrinityPower(SNOPower.X1_Monk_MysticAlly_v2);
                }
                // Air Ally
                if (CanCastMysticAirAlly())
                {
                    return new TrinityPower(SNOPower.X1_Monk_MysticAlly_v2);
                }
                // Sweeping Wind for Transcendance Health Regen
                if (CanCastSweepingWindsForTranscendence())
                {
                    // LastSweepingWindRefresh = DateTime.UtcNow; // Now set through HandleTarget primary spell usage
                    return new TrinityPower(SNOPower.Monk_SweepingWind);
                }
                // Start Sweeping Wind if not up 
                if (ShouldStartSweepingWinds())
                {
                    // LastSweepingWindRefresh = DateTime.UtcNow; // Now set through HandleTarget primary spell usage
                    return new TrinityPower(SNOPower.Monk_SweepingWind);
                }

                // No buffs, do nothing
                return new TrinityPower();
            }

            if (IsCurrentlyAvoiding)
            {
                // Epiphany: spirit regen, dash to targets
                if (CanCastEpiphany())
                {
                    return new TrinityPower(SNOPower.X1_Monk_Epiphany);
                }

                // Serenity if health is low
                if (CanCastSerenityOnLowHealth())
                {
                    return new TrinityPower(SNOPower.Monk_Serenity);
                }

                // Mystic ally
                if (CanCastMysticAlly())
                {
                    return new TrinityPower(SNOPower.X1_Monk_MysticAlly_v2);
                }
                // Air Ally
                if (CanCastMysticAirAlly())
                {
                    return new TrinityPower(SNOPower.X1_Monk_MysticAlly_v2);
                }
                // Sweeping Wind for Transcendance Health Regen
                if (CanCastSweepingWindsForTranscendence())
                {
                    // LastSweepingWindRefresh = DateTime.UtcNow; // Now set through HandleTarget primary spell usage
                    return new TrinityPower(SNOPower.Monk_SweepingWind);
                }

                // Start Sweeping Wind if not up 
                if (ShouldStartSweepingWinds())
                {
                    // LastSweepingWindRefresh = DateTime.UtcNow; // Now set through HandleTarget primary spell usage
                    return new TrinityPower(SNOPower.Monk_SweepingWind);
                }

                // No buffs, do nothing
                return default(TrinityPower);
            }

            // Combat Section 

            if (ShouldRefreshBastiansGeneratorBuff)
            {
                power = GetPrimaryPower();
                if (power != null)
                    return power;
            }

            // Epiphany: spirit regen, dash to targets
            if (CanCastEpiphany())
            {
                return new TrinityPower(SNOPower.X1_Monk_Epiphany);
            }

            // Serenity if health is low
            if (CanCastSerenityOnLowHealth())
            {
                return new TrinityPower(SNOPower.Monk_Serenity);
            }

            // Mystic ally
            if (CanCastMysticAlly())
            {
                return new TrinityPower(SNOPower.X1_Monk_MysticAlly_v2);
            }

            // Air Ally
            if (CanCastMysticAirAlly())
            {
                return new TrinityPower(SNOPower.X1_Monk_MysticAlly_v2);
            }

            // InnerSanctuary 
            if (CanCastInnerSanctuary())
            {
                return new TrinityPower(SNOPower.X1_Monk_InnerSanctuary);
            }

            // Blinding Flash
            if (CanCastBlindingFlash())
            {
                return new TrinityPower(SNOPower.Monk_BlindingFlash);
            }

            // Blinding Flash as a DEFENSE
            if (CanCastBlindingFlashDefensively())
            {
                return new TrinityPower(SNOPower.Monk_BlindingFlash);
            }

            // Breath of Heaven Section

            // Breath of Heaven when needing healing or the buff
            if (CanCastBreathOfHeavenForHealing())
            {
                return new TrinityPower(SNOPower.Monk_BreathOfHeaven);
            }

            // Breath of Heaven for spirit - Infused with Light
            if (CanCastBreathOfHeavenInfusedWithLight())
            {
                return new TrinityPower(SNOPower.Monk_BreathOfHeaven);
            }

            // Seven Sided Strike
            if (CanCastSevenSidedStrike())
            {
                RefreshSweepingWind(true);
                return new TrinityPower(SNOPower.Monk_SevenSidedStrike, 16f, CurrentTarget.Position);
            }

            // Sweeping Winds Refresh
            if (ShouldRefreshSweepingWinds())
            {
                // LastSweepingWindRefresh = DateTime.UtcNow; // Now set through HandleTarget primary spell usage
                return new TrinityPower(SNOPower.Monk_SweepingWind);
            }

            // Start Sweeping Wind if not up 
            if (ShouldStartSweepingWinds())
            {
                // LastSweepingWindRefresh = DateTime.UtcNow; // Now set through HandleTarget primary spell usage
                return new TrinityPower(SNOPower.Monk_SweepingWind);
            }

            // Sweeping Wind for Transcendance Health Regen
            if (CanCastSweepingWindsForTranscendence())
            {
                // LastSweepingWindRefresh = DateTime.UtcNow; // Now set through HandleTarget primary spell usage
                return new TrinityPower(SNOPower.Monk_SweepingWind);
            }

            var cycloneStrikeRange = Runes.Monk.Implosion.IsActive ? 34f : 24f;
            var cycloneStrikeSpirit = Runes.Monk.EyeOfTheStorm.IsActive ? 30 : 50;

            // Cyclone Strike
            if (CanCastCycloneStrike(cycloneStrikeRange, cycloneStrikeSpirit))
            {
                RefreshSweepingWind(true);
                return new TrinityPower(SNOPower.Monk_CycloneStrike);
            }

            // Dashing Strike
            if (CanCastDashingStrike)
            {
                if (Legendary.Jawbreaker.IsEquipped)
                {
                    return JawBreakerDashingStrike();
                }

                // Raiment set, dash costs 75 spirit and refunds a charge when it's used
                if (Sets.ThousandStorms.IsSecondBonusActive && ((Player.PrimaryResource >= 75 && Skills.Monk.DashingStrike.Charges >= 1) || CacheData.Buffs.HasCastingShrine))
                {
                    RefreshSweepingWind(true);
                    if (CurrentTarget.IsBossOrEliteRareUnique)
                        return new TrinityPower(SNOPower.X1_Monk_DashingStrike, MaxDashingStrikeRange, CurrentTarget.Position);
                    return new TrinityPower(SNOPower.X1_Monk_DashingStrike, MaxDashingStrikeRange, TargetUtil.GetBestPierceTarget(50f, true).Position);
                }

                if (!Sets.ThousandStorms.IsSecondBonusActive)
                {
                    // We get a charge every 8 seconds. If we have 2 charges, be dashing
                    if (Skills.Monk.DashingStrike.Charges > 1)
                    {
                        return new TrinityPower(SNOPower.X1_Monk_DashingStrike, MaxDashingStrikeRange, CurrentTarget.Position);
                    }

                    if (Skills.Monk.DashingStrike.Charges > 1 && (CurrentTarget.IsEliteRareUnique || TargetUtil.ClusterExists(15f, 3)) &&
                        TargetUtil.IsUnitWithDebuffInRangeOfPosition(15f, TargetUtil.GetBestClusterPoint(), SNOPower.Monk_ExplodingPalm) ||
                        TargetUtil.AnyMobsInRangeOfPosition(CurrentTarget.Position, 20f, 3) && Skills.Monk.ExplodingPalm.IsTrackedOnUnit(CurrentTarget))
                    {
                        RefreshSweepingWind(true);
                        return new TrinityPower(SNOPower.X1_Monk_DashingStrike, MaxDashingStrikeRange, CurrentTarget.Position);
                    }
                }
            }

            // Make a mega-splosion
            if (ShouldSpreadExplodingPalm())
            {
                ChangeTarget();
            }

            // Exploding Palm
            if (CanCastExplodingPalm())
            {
                return new TrinityPower(SNOPower.Monk_ExplodingPalm, 10f, CurrentTarget.ACDGuid);
            }

            // Wave of light
            if (CanCastWaveOfLight(WaveOfLightRange))
            {
                RefreshSweepingWind(true);
                return new TrinityPower(SNOPower.Monk_WaveOfLight, WaveOfLightRange, TargetUtil.GetBestClusterPoint());
            }

            // Lashing Tail Kick
            if (CanCastLashingTailKick())
            {
                RefreshSweepingWind(true);
                return new TrinityPower(SNOPower.Monk_LashingTailKick, 10f, CurrentTarget.ACDGuid);
            }

            // For tempest rush re-use
            if (CanRecastTempestRush())
            {
                GenerateMonkZigZag();
                Trinity.MaintainTempestRush = true;
                const string trUse = "Continuing Tempest Rush for Combat";
                LogTempestRushStatus(trUse);
                return new TrinityPower(SNOPower.Monk_TempestRush, 23f, ZigZagPosition, Trinity.CurrentWorldDynamicId, -1, 0, 0);
            }

            // Tempest rush at elites or groups of mobs
            if (CanCastTempestRushAsAttack())
            {
                GenerateMonkZigZag();
                Trinity.MaintainTempestRush = true;
                const string trUse = "Starting Tempest Rush for Combat";
                LogTempestRushStatus(trUse);
                return new TrinityPower(SNOPower.Monk_TempestRush, 23f, ZigZagPosition, Trinity.CurrentWorldDynamicId, -1, 0, 0);
            }

            // 4 Mantra spam for the 4 second buff
            if (!Settings.Combat.Monk.DisableMantraSpam)
            {
                if (CanCastMantra(SNOPower.X1_Monk_MantraOfConviction_v2))
                {
                    return new TrinityPower(SNOPower.X1_Monk_MantraOfConviction_v2, 3);
                }

                if (CanCastMantra(SNOPower.X1_Monk_MantraOfRetribution_v2))
                {
                    return new TrinityPower(SNOPower.X1_Monk_MantraOfRetribution_v2, 3);
                }
                if (CanCastMantra(SNOPower.X1_Monk_MantraOfHealing_v2))
                {
                    return new TrinityPower(SNOPower.X1_Monk_MantraOfHealing_v2, 3);
                }
                if (CanCastMantra(SNOPower.X1_Monk_MantraOfEvasion_v2))
                {
                    return new TrinityPower(SNOPower.X1_Monk_MantraOfEvasion_v2, 3);
                }
            }

            /*
             * Dual/Trigen Monk section
             * 
             * Cycle through Deadly Reach, Way of the Hundred Fists, and Fists of Thunder every 3 seconds to keep 8% passive buff up if we have Combination Strike
             *  - or - 
             * Keep Foresight and Blazing Fists buffs up every 30/5 seconds
             */
            bool hasCombinationStrike = Passives.Monk.CombinationStrike.IsActive;
            bool isDualOrTriGen = CacheData.Hotbar.ActiveSkills.Count(s =>
                s.Power == SNOPower.Monk_DeadlyReach ||
                s.Power == SNOPower.Monk_WayOfTheHundredFists ||
                s.Power == SNOPower.Monk_FistsofThunder ||
                s.Power == SNOPower.Monk_CripplingWave) >= 2 && hasCombinationStrike;

            // interval in milliseconds for Generators
            int deadlyReachInterval = 0;
            if (hasCombinationStrike)
                deadlyReachInterval = 2500;
            else if (Runes.Monk.Foresight.IsActive)
                deadlyReachInterval = 29000;

            int wayOfTheHundredFistsInterval = 0;
            if (hasCombinationStrike)
                wayOfTheHundredFistsInterval = 2500;
            else if (Runes.Monk.BlazingFists.IsActive)
                wayOfTheHundredFistsInterval = 4500;

            int cripplingWaveInterval = 0;
            if (hasCombinationStrike)
                cripplingWaveInterval = 2500;

            // Fists of Thunder:Thunder Clap - Fly to Target
            if (!IsCurrentlyAvoiding && CanCast(SNOPower.Monk_FistsofThunder) && Runes.Monk.Thunderclap.IsActive && CurrentTarget.Distance > 16f)
            {
                RefreshSweepingWind();
                return new TrinityPower(SNOPower.Monk_FistsofThunder, 30f, CurrentTarget.ACDGuid);
            }

            // Deadly Reach: Foresight, every 27 seconds or 2.7 seconds with combo strike
            if (!IsCurrentlyAvoiding && CanCast(SNOPower.Monk_DeadlyReach) && (isDualOrTriGen || Runes.Monk.Foresight.IsActive) &&
                (SpellHistory.TimeSinceUse(SNOPower.Monk_DeadlyReach) > TimeSpan.FromMilliseconds(deadlyReachInterval) ||
                (SpellHistory.SpellUseCountInTime(SNOPower.Monk_DeadlyReach, TimeSpan.FromMilliseconds(27000)) < 3) && Runes.Monk.Foresight.IsActive))
            {
                RefreshSweepingWind();
                return new TrinityPower(SNOPower.Monk_DeadlyReach, 16f, CurrentTarget.ACDGuid);
            }

            // Way of the Hundred Fists: Blazing Fists, every 4-5ish seconds or if we don't have 3 stacks of the buff or or 2.7 seconds with combo strike
            if (!IsCurrentlyAvoiding && CanCast(SNOPower.Monk_WayOfTheHundredFists) && (isDualOrTriGen || Runes.Monk.BlazingFists.IsActive) &&
                (GetBuffStacks(SNOPower.Monk_WayOfTheHundredFists) < 3 ||
                SpellHistory.TimeSinceUse(SNOPower.Monk_WayOfTheHundredFists) > TimeSpan.FromMilliseconds(wayOfTheHundredFistsInterval)))
            {
                RefreshSweepingWind();
                return new TrinityPower(SNOPower.Monk_WayOfTheHundredFists, 16f, CurrentTarget.ACDGuid);
            }

            // Crippling Wave
            if (!IsCurrentlyAvoiding && CanCast(SNOPower.Monk_CripplingWave) &&
                SpellHistory.TimeSinceUse(SNOPower.Monk_CripplingWave) > TimeSpan.FromMilliseconds(cripplingWaveInterval))
            {
                RefreshSweepingWind();
                return new TrinityPower(SNOPower.Monk_CripplingWave, 20f, CurrentTarget.ACDGuid);
            }

            power = GetPrimaryPower();
            if (power != null)
                return power;

            // Wave of light as primary 
            if (!IsCurrentlyAvoiding && CanCast(SNOPower.Monk_WaveOfLight))
            {
                RefreshSweepingWind(true);
                return new TrinityPower(SNOPower.Monk_WaveOfLight, 16f, TargetUtil.GetBestClusterPoint());
            }

            // Default attacks
            return DefaultPower;
        }

        private static bool CanRecastTempestRush()
        {
            return Player.PrimaryResource >= 15 && CanCast(SNOPower.Monk_TempestRush) &&
                    Trinity.TimeSinceUse(SNOPower.Monk_TempestRush) <= 150 &&
                    ((Settings.Combat.Monk.TROption != TempestRushOption.MovementOnly) &&
                    !(Settings.Combat.Monk.TROption == TempestRushOption.TrashOnly && TargetUtil.AnyElitesInRange(40f)));
        }

        private static void ParameterSetup()
        {
            _hasInnaSet = Sets.Innas.IsThirdBonusActive;
            _hasSwk = Sets.MonkeyKingsGarb.IsSecondBonusActive;
            _minSweepingWindSpirit = _hasInnaSet ? 5f : 75f;

            // Locally scoped dynamic variables
            if (IsTaegukEquipped()) // Taeguk gem refresh (3 seconds)
            {
                if (_hasInnaSet)
                    _swMinTime = 500;
                _swMinTime = 1800;
            }
        }

        private static TrinityPower GetPrimaryPower()
        {
            // Fists of Thunder
            if (!IsCurrentlyAvoiding && CanCast(SNOPower.Monk_FistsofThunder))
            {
                RefreshSweepingWind();
                return new TrinityPower(SNOPower.Monk_FistsofThunder, 45f, CurrentTarget.ACDGuid);
            }

            // Deadly Reach normal
            if (!IsCurrentlyAvoiding && CanCast(SNOPower.Monk_DeadlyReach))
            {
                RefreshSweepingWind();
                return new TrinityPower(SNOPower.Monk_DeadlyReach, 16f, CurrentTarget.ACDGuid);
            }

            // Way of the Hundred Fists normal
            if (!IsCurrentlyAvoiding && CanCast(SNOPower.Monk_WayOfTheHundredFists))
            {
                RefreshSweepingWind();
                return new TrinityPower(SNOPower.Monk_WayOfTheHundredFists, 16f, CurrentTarget.ACDGuid);
            }

            // Crippling Wave Normal
            if (!IsCurrentlyAvoiding && CanCast(SNOPower.Monk_CripplingWave))
            {
                RefreshSweepingWind();
                return new TrinityPower(SNOPower.Monk_CripplingWave, 30f, CurrentTarget.ACDGuid);
            }
            return null;
        }

        private static bool CanCastMantra(SNOPower mantraPower, int timeSpan = 2950, float combatRange = 10f)
        {
            return CanCast(mantraPower) && Player.PrimaryResource >= 50 && TimeSincePowerUse(mantraPower) > timeSpan &&
                        (!GetHasBuff(mantraPower) || (_hasSwk && TargetUtil.AnyMobsInRange(combatRange)));
        }

        private static bool CanCastTempestRushAsAttack()
        {
            return !IsCurrentlyAvoiding && !Player.IsRooted && CanCast(SNOPower.Monk_TempestRush) &&
                   ((Player.PrimaryResource >= Settings.Combat.Monk.TR_MinSpirit && !IsWaitingForSpecial) || Player.PrimaryResource >= MinEnergyReserve) &&
                   (Settings.Combat.Monk.TROption == TempestRushOption.Always ||
                    Settings.Combat.Monk.TROption == TempestRushOption.CombatOnly ||
                    (Settings.Combat.Monk.TROption == TempestRushOption.ElitesGroupsOnly && (TargetUtil.AnyElitesInRange(25) || TargetUtil.AnyMobsInRange(25, 2))) ||
                    (Settings.Combat.Monk.TROption == TempestRushOption.TrashOnly && !TargetUtil.AnyElitesInRange(90f) && TargetUtil.AnyMobsInRange(40f)));
        }

        private static bool CanCastLashingTailKick()
        {
            return !IsCurrentlyAvoiding && CanCast(SNOPower.Monk_LashingTailKick) &&
                   ((Player.PrimaryResource >= 50 && !IsWaitingForSpecial) || Player.PrimaryResource >= MinEnergyReserve);
        }

        private static bool CanCastWaveOfLight(float wolRange)
        {
            return !IsCurrentlyAvoiding && CanCast(SNOPower.Monk_WaveOfLight) && !MonkHasNoPrimary &&
                   (TargetUtil.AnyMobsInRange(wolRange, Settings.Combat.Monk.MinWoLTrashCount) || TargetUtil.IsEliteTargetInRange(wolRange)) &&
                   (Player.PrimaryResource >= 75 && !IsWaitingForSpecial || Player.PrimaryResource > MinEnergyReserve) &&
                // optional check for SW stacks
                   (Settings.Combat.Monk.SWBeforeWoL && (CheckAbilityAndBuff(SNOPower.Monk_SweepingWind) && GetBuffStacks(SNOPower.Monk_SweepingWind) == 3) || !Settings.Combat.Monk.SWBeforeWoL) &&
                   HasMantraAbilityAndBuff();
        }

        private static bool CanCastExplodingPalm()
        {
            return !IsCurrentlyAvoiding &&
                   CanCast(SNOPower.Monk_ExplodingPalm, CanCastFlags.NoTimer) &&
                   (Runes.Monk.EssenceBurn.IsActive ? !SpellTracker.IsUnitTracked(CurrentTarget, SNOPower.Monk_ExplodingPalm) : !Skills.Monk.ExplodingPalm.IsTrackedOnUnit(CurrentTarget));
        }

        private static bool CanCastCycloneStrike(float cycloneStrikeRange, int cycloneStrikeSpirit)
        {
            return !IsCurrentlyAvoiding && CanCast(SNOPower.Monk_CycloneStrike) &&
                   (
                       TargetUtil.AnyElitesInRange(cycloneStrikeRange, 1) ||
                       TargetUtil.AnyMobsInRange(cycloneStrikeRange, Settings.Combat.Monk.MinCycloneTrashCount) ||
                       (CurrentTarget.RadiusDistance >= 15f && CurrentTarget.RadiusDistance <= cycloneStrikeRange) // pull the current target into attack range
                       ) && TimeSincePowerUse(SNOPower.Monk_CycloneStrike) > 3000 &&

                   // Cyclone if more than 25% of monsters within cyclone range are at least 10f away
                   TargetUtil.IsPercentUnitsWithinBand(10f, cycloneStrikeRange, 0.25) &&

                   (Player.PrimaryResource >= (cycloneStrikeSpirit + MinEnergyReserve));
        }

        private static bool CanCastSweepingWindsForTranscendence()
        {
            return CanCast(SNOPower.Monk_SweepingWind, CanCastFlags.NoTimer) &&
                   Player.PrimaryResource >= _minSweepingWindSpirit &&
                   Passives.Monk.Transcendence.IsActive && Settings.Combat.Monk.SpamSweepingWindOnLowHP &&
                   Player.CurrentHealthPct <= V.F("Monk.SweepingWind.SpamOnLowHealthPct") &&
                   Trinity.TimeSinceUse(SNOPower.Monk_SweepingWind) > 500;
        }

        private static bool ShouldStartSweepingWinds()
        {
            return CanCast(SNOPower.Monk_SweepingWind, CanCastFlags.Timer) && !GetHasBuff(SNOPower.Monk_SweepingWind) &&
                   (TargetUtil.AnyMobsInRange(30, 1) || _hasInnaSet || IsTaegukEquipped()) && Player.PrimaryResource >= _minSweepingWindSpirit;
        }

        private static bool ShouldRefreshSweepingWinds()
        {
            return Player.PrimaryResource >= _minSweepingWindSpirit &&
                   CanCast(SNOPower.Monk_SweepingWind, CanCastFlags.NoTimer) && (GetHasBuff(SNOPower.Monk_SweepingWind) || IsTaegukEquipped()) &&
                   TimeSinceLastSweepingWind >= _swMinTime &&
                // First one is for Taeguk                       This one is for regular SW Stacks
                   (TimeSinceLastSweepingWind <= SwMaxTaegukTime || TimeSinceLastSweepingWind <= SwMaxTime);
        }

        private static bool CanCastSevenSidedStrike()
        {
            return !IsCurrentlyAvoiding &&
                   (TargetUtil.AnyElitesInRange(15, 1) || Player.CurrentHealthPct <= 0.55 || Legendary.Madstone.IsEquipped) &&
                   CanCast(SNOPower.Monk_SevenSidedStrike, CanCastFlags.NoTimer) &&
                   ((Player.PrimaryResource >= 50 && !IsWaitingForSpecial) || Player.PrimaryResource >= MinEnergyReserve);
        }

        private static bool CanCastBreathOfHeavenInfusedWithLight()
        {
            return CanCast(SNOPower.Monk_BreathOfHeaven, CanCastFlags.NoTimer) &&
                   !GetHasBuff(SNOPower.Monk_BreathOfHeaven) && Runes.Monk.InfusedWithLight.IsActive &&
                   (TargetUtil.AnyMobsInRange(20) || TargetUtil.IsEliteTargetInRange(20) || Player.PrimaryResource < 75);
        }

        private static bool CanCastBreathOfHeavenForHealing()
        {
            return (Player.CurrentHealthPct <= 0.6 || !GetHasBuff(SNOPower.Monk_BreathOfHeaven)) && CanCast(SNOPower.Monk_BreathOfHeaven) &&
                   (Player.PrimaryResource >= 35 || (!CanCast(SNOPower.Monk_Serenity) && Player.PrimaryResource >= 25));
        }

        private static bool CanCastBlindingFlashDefensively()
        {
            return Player.PrimaryResource >= 10 && CanCast(SNOPower.Monk_BlindingFlash) &&
                   Player.CurrentHealthPct <= 0.75 && TargetUtil.AnyMobsInRange(15, 1);
        }

        private static bool CanCastBlindingFlash()
        {
            return Player.PrimaryResource >= 20 && CanCast(SNOPower.Monk_BlindingFlash) &&
                   (
                       TargetUtil.AnyElitesInRange(15, 1) ||
                       Player.CurrentHealthPct <= 0.4 ||
                       (TargetUtil.AnyMobsInRange(15, 3)) ||
                       (CurrentTarget.IsBossOrEliteRareUnique && CurrentTarget.RadiusDistance <= 15f) ||
                // as pre-sweeping wind buff
                       (TargetUtil.AnyMobsInRange(15, 1) && CanCast(SNOPower.Monk_SweepingWind) && !GetHasBuff(SNOPower.Monk_SweepingWind) && _hasInnaSet)
                       ) &&
                // Check if either we don't have sweeping winds, or we do and it's ready to cast in a moment
                   (CheckAbilityAndBuff(SNOPower.Monk_SweepingWind) ||
                    (!GetHasBuff(SNOPower.Monk_SweepingWind) &&
                     (CanCast(SNOPower.Monk_SweepingWind, CanCastFlags.NoTimer))) ||
                    Player.CurrentHealthPct <= 0.25);
        }

        private static bool CanCastInnerSanctuary()
        {
            return TargetUtil.EliteOrTrashInRange(16f) && CanCast(SNOPower.X1_Monk_InnerSanctuary);
        }

        private static bool CanCastMysticAirAlly()
        {
            return CanCast(SNOPower.X1_Monk_MysticAlly_v2) && Runes.Monk.AirAlly.IsActive && Player.PrimaryResource < 150;
        }

        private static bool CanCastMysticAlly()
        {
            return CanCast(SNOPower.X1_Monk_MysticAlly_v2) && TargetUtil.EliteOrTrashInRange(30f) && !Runes.Monk.AirAlly.IsActive;
        }

        private static bool CanCastSerenityOnLowHealth()
        {
            return (Player.CurrentHealthPct <= 0.50 || (Player.IsIncapacitated && Player.CurrentHealthPct <= 0.90)) && CanCast(SNOPower.Monk_Serenity);
        }

        private static bool CanCastEpiphany()
        {
            return !IsCurrentlyAvoiding && CanCast(SNOPower.X1_Monk_Epiphany, CanCastFlags.NoTimer) && (Settings.Combat.Monk.EpiphanyOffCD ||
                            (TargetUtil.EliteOrTrashInRange(15f) || TargetUtil.AnyMobsInRange(15f, 5)) &&
                            (Player.PrimaryResourcePct < 0.50 || ((Runes.Monk.DesertShroud.IsActive || Runes.Monk.SoothingMist.IsActive) && Player.CurrentHealthPct < 0.50)));
        }


        /// <summary>
        /// Gets the time since last sweeping wind.
        /// </summary>
        /// <value>The time since last sweeping wind.</value>
        private static double TimeSinceLastSweepingWind
        {
            get
            {
                return DateTime.UtcNow.Subtract(LastSweepingWindRefresh).TotalMilliseconds;
            }
        }

        /// <summary>
        /// Determines whether [is taeguk equipped].
        /// </summary>
        /// <returns><c>true</c> if [is taeguk equipped]; otherwise, <c>false</c>.</returns>
        private static bool IsTaegukEquipped()
        {
            return CacheData.Inventory.EquippedIds.Contains(405804);
        }

        /// <summary>
        /// Determines if we should change targets to apply Exploading Palm to another target
        /// </summary>
        internal static bool ShouldSpreadExplodingPalm()
        {
            return CurrentTarget != null && Skills.Monk.ExplodingPalm.IsActive &&

                // Current target is valid
                 CurrentTarget.IsUnit && !CurrentTarget.IsTreasureGoblin &&

                // Avoid rapidly changing targets
                DateTime.UtcNow.Subtract(_lastTargetChange).TotalMilliseconds > 500 &&

                // enough resources and mobs nearby
                Player.PrimaryResource > 40 && TargetUtil.AnyMobsInRange(15f, 4) &&

                // Don't bother if X or more targets already have EP
                !TargetUtil.IsUnitWithDebuffInRangeOfPosition(15f, TargetUtil.GetBestClusterPoint(), SNOPower.Monk_ExplodingPalm, Settings.Combat.Monk.ExploadingPalmMaxMobCount);

        }

        /// <summary>
        /// Blacklist the current target for 3 seconds and attempt to find a new target one
        /// </summary>
        internal static void ChangeTarget()
        {
            _lastTargetChange = DateTime.UtcNow;

            var currentTarget = CurrentTarget;
            var lowestHealthTarget = TargetUtil.LowestHealthTarget(15f, Trinity.Me.Position, Skills.Monk.ExplodingPalm.SNOPower);

            //Logger.LogNormal("Blacklisting {0} {1} - Changing Target", CurrentTarget.InternalName, CurrentTarget.CommonData.ACDGuid);
            Trinity.Blacklist3Seconds.Add(CurrentTarget.RActorGuid);

            // Would like the new target to be different than the one we just blacklisted, or be very close to dead.
            if (lowestHealthTarget.ACDGuid == currentTarget.ACDGuid && lowestHealthTarget.HitPointsPct < 0.2) return;

            Trinity.CurrentTarget = lowestHealthTarget;
            //Logger.LogNormal("Found lowest health target {0} {1} ({2:0.##}%)", CurrentTarget.InternalName, CurrentTarget.CommonData.ACDGuid, lowestHealthTarget.HitPointsPct * 100);
        }

        private static bool CanCastDashingStrike
        {
            get { return CanCast(SNOPower.X1_Monk_DashingStrike, CanCastFlags.NoTimer) && !IsCurrentlyAvoiding; }
        }

        /// <summary>
        /// blahblah999's Dashing Strike with JawBreaker Item
        /// https://www.thebuddyforum.com/demonbuddy-forum/plugins/trinity/167966-monk-trinity-mod-dash-strike-spam-rots-set-bonus-jawbreaker.html
        /// </summary>
        internal static TrinityPower JawBreakerDashingStrike()
        {
            const float procDistance = 33f;
            var farthestTarget = TargetUtil.GetDashStrikeFarthestTarget(49f);

            // able to cast
            if (Skills.Monk.DashingStrike.Charges > 1 && TargetUtil.AnyMobsInRange(25f, 3) || TargetUtil.IsEliteTargetInRange(70f)) // surround by mobs or elite engaged.
            {
                if (farthestTarget != null) // found a target within 33-49 yards.
                {
                    RefreshSweepingWind();
                    return new TrinityPower(SNOPower.X1_Monk_DashingStrike, MaxDashingStrikeRange, farthestTarget.Position, Trinity.CurrentWorldDynamicId, -1, 2, 2);
                }
                // no free target found, get a nearby cluster point instead.
                var bestClusterPoint = TargetUtil.GetBestClusterPoint(15f, procDistance);
                RefreshSweepingWind();
                return new TrinityPower(SNOPower.X1_Monk_DashingStrike, MaxDashingStrikeRange, bestClusterPoint, Trinity.CurrentWorldDynamicId, -1, 2, 2);
            }

            //usually this trigger after dash to the farthest target. dash a single mobs >30 yards, trying to dash back the cluster
            if (Skills.Monk.DashingStrike.Charges > 1 && TargetUtil.ClusterExists(20, 50, 3))
            {
                var dashStrikeBestClusterPoint = TargetUtil.GetDashStrikeBestClusterPoint(20f, 50f);
                if (dashStrikeBestClusterPoint != Trinity.Player.Position)
                {
                    RefreshSweepingWind();
                    return new TrinityPower(SNOPower.X1_Monk_DashingStrike, MaxDashingStrikeRange, dashStrikeBestClusterPoint, Trinity.CurrentWorldDynamicId, -1, 2, 2);
                }
            }

            // dash to anything which is free.               
            if (Skills.Monk.DashingStrike.Charges > 1 && farthestTarget != null)
            {
                RefreshSweepingWind();
                return new TrinityPower(SNOPower.X1_Monk_DashingStrike, MaxDashingStrikeRange, farthestTarget.Position, Trinity.CurrentWorldDynamicId, -1, 2, 2);
            }

            return new TrinityPower(SNOPower.X1_Monk_DashingStrike, MaxDashingStrikeRange, CurrentTarget.Position, Trinity.CurrentWorldDynamicId, -1, 2, 2);
        }

        internal static void RefreshSweepingWind(bool spendsSpirit = false)
        {
            // Don't refresh timer if we have Taeguk Legendary Gem equipped and not spending spirit
            if (!spendsSpirit && IsTaegukEquipped())
                return;

            if (GetHasBuff(SNOPower.Monk_SweepingWind))
                LastSweepingWindRefresh = DateTime.UtcNow;
        }

        private static Vector3 _zigZagPosition = Vector3.Zero;
        /// <summary>
        /// The last "ZigZag" position, used with Barb Whirlwind, Monk Tempest Rush, etc.
        /// </summary>
        public static Vector3 ZigZagPosition
        {
            get { return _zigZagPosition; }
            internal set { _zigZagPosition = value; }
        }

        internal static void GenerateMonkZigZag()
        {
            float extraDistance = CurrentTarget.RadiusDistance <= 20f ? 15f : 20f;
            ZigZagPosition = TargetUtil.GetZigZagTarget(CurrentTarget.Position, extraDistance);
            double direction = MathUtil.FindDirectionRadian(Player.Position, ZigZagPosition);
            ZigZagPosition = MathEx.GetPointAt(Player.Position, 40f, (float)direction);
            Logger.Log(TrinityLogLevel.Debug, LogCategory.Behavior, "Generated ZigZag {0} distance {1:0}", ZigZagPosition, ZigZagPosition.Distance2D(Player.Position));
        }

        internal static TrinityPower GetMonkDestroyPower()
        {
            if (Skills.Monk.DashingStrike.Charges > 1 && CanCast(SNOPower.X1_Monk_DashingStrike) && Player.PrimaryResource > 75)
                return new TrinityPower(SNOPower.X1_Monk_DashingStrike, MaxDashingStrikeRange);

            if (CanCast(SNOPower.Monk_FistsofThunder))
                return new TrinityPower(SNOPower.Monk_FistsofThunder, 6f);

            if (IsTempestRushReady())
                return new TrinityPower(SNOPower.Monk_TempestRush, 6f);

            if (Skills.Monk.DashingStrike.Charges > 1 && CanCast(SNOPower.X1_Monk_DashingStrike))
                return new TrinityPower(SNOPower.X1_Monk_DashingStrike, MaxDashingStrikeRange);

            if (CanCast(SNOPower.Monk_DeadlyReach))
                return new TrinityPower(SNOPower.Monk_DeadlyReach, 6f);

            if (CanCast(SNOPower.Monk_CripplingWave))
                return new TrinityPower(SNOPower.Monk_CripplingWave, 6f);

            if (CanCast(SNOPower.Monk_WayOfTheHundredFists))
                return new TrinityPower(SNOPower.Monk_WayOfTheHundredFists, 5f);
            return DefaultPower;
        }

        /// <summary>
        /// Returns true if we have a mantra and it's up, or if we don't have a Mantra at all
        /// </summary>
        /// <returns></returns>
        internal static bool HasMantraAbilityAndBuff()
        {
            return
                (CheckAbilityAndBuff(SNOPower.X1_Monk_MantraOfConviction_v2) ||
                CheckAbilityAndBuff(SNOPower.X1_Monk_MantraOfEvasion_v2) ||
                CheckAbilityAndBuff(SNOPower.X1_Monk_MantraOfHealing_v2) ||
                CheckAbilityAndBuff(SNOPower.X1_Monk_MantraOfRetribution_v2));
        }

        internal static bool IsTempestRushReady()
        {
            if (!CanCastRecurringPower())
                return false;

            if (!Hotbar.Contains(SNOPower.Monk_TempestRush))
                return false;

            if (!Hotbar.Contains(SNOPower.Monk_TempestRush))
                return false;

            if (!HasMantraAbilityAndBuff())
                return false;

            double currentSpirit = ZetaDia.Me.CurrentPrimaryResource;

            // Minimum 10 spirit to continue channeling tempest rush
            if (Trinity.TimeSinceUse(SNOPower.Monk_TempestRush) < 150 && currentSpirit > 10f)
                return true;

            // Minimum 25 Spirit to start Tempest Rush
            if (PowerManager.CanCast(SNOPower.Monk_TempestRush) && currentSpirit > Settings.Combat.Monk.TR_MinSpirit && Trinity.TimeSinceUse(SNOPower.Monk_TempestRush) > 550)
                return true;

            return false;
        }

        internal static bool CanCastRecurringPower()
        {
            if (Player.ActorClass != ActorClass.Monk)
                return false;

            if (BrainBehavior.IsVendoring)
                return false;

            if (Player.IsInTown)
                return false;

            if (ProfileManager.CurrentProfileBehavior != null)
            {
                Type profileBehaviorType = ProfileManager.CurrentProfileBehavior.GetType();
                if (profileBehaviorType == typeof(UseObjectTag) ||
                    profileBehaviorType == typeof(UsePortalTag) ||
                    profileBehaviorType == typeof(UseWaypointTag) ||
                    profileBehaviorType == typeof(UseTownPortalTag))
                    return false;
            }

            if (ZetaDia.Me.LoopingAnimationEndTime > 0)
                return false;

            return true;
        }

        internal static void RunOngoingPowers()
        {
            MaintainTempestRush();

            MaintainSweepingWind();
        }

        private static void MaintainSweepingWind()
        {
            if (!CanCastRecurringPower())
                return;

            // Sweeping Winds Refresh
            if (Player.PrimaryResource >= _minSweepingWindSpirit &&
                CanCast(SNOPower.Monk_SweepingWind, CanCastFlags.NoTimer) && (GetHasBuff(SNOPower.Monk_SweepingWind) || IsTaegukEquipped()) &&
                TimeSinceLastSweepingWind >= _swMinTime &&
                // First one is for Taeguk                       This one is for regular SW Stacks
                (TimeSinceLastSweepingWind <= SwMaxTaegukTime || TimeSinceLastSweepingWind <= SwMaxTime))
            {
                var usePowerResult = ZetaDia.Me.UsePower(SNOPower.Monk_SweepingWind, Vector3.Zero, ZetaDia.CurrentWorldDynamicId);
                Logger.Log("Sweeping Wind Out of Band Refresh {0}", usePowerResult ? "succeeded" : "failed");
                if (usePowerResult)
                {
                    LastSweepingWindRefresh = DateTime.UtcNow;
                    CacheData.AbilityLastUsed[SNOPower.Monk_SweepingWind] = DateTime.UtcNow;
                }
            }
        }

        internal static void MaintainTempestRush()
        {
            if (!IsTempestRushReady())
                return;

            if (Player.IsInTown || BrainBehavior.IsVendoring)
                return;

            if (TownRun.IsTryingToTownPortal())
                return;

            if (Trinity.TimeSinceUse(SNOPower.Monk_TempestRush) > 150)
                return;

            bool shouldMaintain = false;
            bool nullTarget = CurrentTarget == null;
            if (!nullTarget)
            {
                // maintain for everything except items, doors, interactables... stuff we have to "click" on
                switch (CurrentTarget.Type)
                {
                    case TrinityObjectType.Unit:
                    case TrinityObjectType.Gold:
                    case TrinityObjectType.Avoidance:
                    case TrinityObjectType.Barricade:
                    case TrinityObjectType.Destructible:
                    case TrinityObjectType.HealthGlobe:
                    case TrinityObjectType.PowerGlobe:
                    case TrinityObjectType.ProgressionGlobe:
                        {
                            if (Settings.Combat.Monk.TROption == TempestRushOption.TrashOnly &&
                                    (TargetUtil.AnyElitesInRange(40f) || CurrentTarget.IsBossOrEliteRareUnique))
                                break;
                            shouldMaintain = true;
                        }
                        break;
                }
            }
            else
            {
                shouldMaintain = true;
            }

            if (Settings.Combat.Monk.TROption != TempestRushOption.MovementOnly && SNOPowerUseTimer(SNOPower.Monk_TempestRush) && shouldMaintain)
            {
                Vector3 target = LastTempestRushLocation;

                const string locationSource = "LastLocation";

                if (target.Distance2D(ZetaDia.Me.Position) <= 1f)
                {
                    // rrrix edit: we can't maintain here
                    return;
                }

                if (target == Vector3.Zero)
                    return;

                float destinationDistance = target.Distance2D(ZetaDia.Me.Position);

                target = TargetUtil.FindTempestRushTarget();

                if (destinationDistance > 10f && NavHelper.CanRayCast(ZetaDia.Me.Position, target))
                {
                    LogTempestRushStatus(String.Format("Using Tempest Rush to maintain channeling, source={0}, V3={1} dist={2:0}", locationSource, target, destinationDistance));

                    var usePowerResult = ZetaDia.Me.UsePower(SNOPower.Monk_TempestRush, target, Trinity.CurrentWorldDynamicId);
                    if (usePowerResult)
                    {
                        CacheData.AbilityLastUsed[SNOPower.Monk_TempestRush] = DateTime.UtcNow;
                    }
                }
            }
        }

        internal static void LogTempestRushStatus(string trUse)
        {

            Logger.Log(TrinityLogLevel.Debug, LogCategory.Behavior, "{0}, xyz={4} spirit={1:0} cd={2} lastUse={3:0}",
                trUse,
                Trinity.Player.PrimaryResource, PowerManager.CanCast(SNOPower.Monk_TempestRush),
                Trinity.TimeSinceUse(SNOPower.Monk_TempestRush), ZigZagPosition);
        }

        public static bool HasSweepingWindBuffUp()
        {
            return (GetHasBuff(SNOPower.Monk_SweepingWind) || !Hotbar.Contains(SNOPower.Monk_SweepingWind));
        }

        private static bool MonkHasNoPrimary
        {
            get
            {
                return !(Hotbar.Contains(SNOPower.Monk_CripplingWave) ||
                    Hotbar.Contains(SNOPower.Monk_FistsofThunder) ||
                    Hotbar.Contains(SNOPower.Monk_DeadlyReach) ||
                    Hotbar.Contains(SNOPower.Monk_WayOfTheHundredFists));
            }
        }
        private static float WaveOfLightRange { get { return Legendary.TzoKrinsGaze.IsEquipped ? 55f : 16f; } }


    }
}
