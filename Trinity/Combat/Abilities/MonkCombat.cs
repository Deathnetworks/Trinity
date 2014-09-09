using System;
using System.Linq;
using Trinity.Config.Combat;
using Trinity.Reference;
using Trinity.Technicals;
using Zeta.Bot;
using Zeta.Bot.Profile.Common;
using Zeta.Common;
using Zeta.Game;
using Zeta.Game.Internals.Actors;
using Logger = Trinity.Technicals.Logger;

namespace Trinity.Combat.Abilities
{
    public class MonkCombat : CombatBase
    {

        private static float Monk_MaxDashingStrikeRange = 55f;
        internal static Vector3 LastTempestRushLocation = Vector3.Zero;
        private static DateTime _lastTargetChange = DateTime.MinValue;

        public static MonkSetting MonkSettings
        {
            get { return Trinity.Settings.Combat.Monk; }
        }

        private static bool hasInnaSet = Sets.Innas.IsThirdBonusActive;

        public static TrinityPower GetPower()
        {     

            if (UseDestructiblePower)
                return GetMonkDestroyPower();

            if (!UseOOCBuff && !IsCurrentlyAvoiding && CombatBase.CanCast(SNOPower.X1_Monk_Epiphany, CombatBase.CanCastFlags.NoTimer) &&
                (TargetUtil.EliteOrTrashInRange(15f) || TargetUtil.AnyMobsInRange(15f, 5)) && 
                (Player.PrimaryResourcePct < 0.50 || ((Runes.Monk.DesertShroud.IsActive || Runes.Monk.SoothingMist.IsActive) && Player.CurrentHealthPct < 0.50))
                )
            {
                return new TrinityPower(SNOPower.X1_Monk_Epiphany);
            }

            // Serenity if health is low
            if ((Player.CurrentHealthPct <= 0.50 || (Player.IsIncapacitated && Player.CurrentHealthPct <= 0.90)) && CombatBase.CanCast(SNOPower.Monk_Serenity))
            {
                return new TrinityPower(SNOPower.Monk_Serenity, 0f, Vector3.Zero, Trinity.CurrentWorldDynamicId, -1, 1, 1);
            }

            // Mystic ally
            if (CombatBase.CanCast(SNOPower.X1_Monk_MysticAlly_v2) && TargetUtil.EliteOrTrashInRange(30f)
                && (!Runes.Monk.AirAlly.IsActive | !Runes.Monk.EnduringAlly.IsActive))
            {
                return new TrinityPower(SNOPower.X1_Monk_MysticAlly_v2, 0f, Vector3.Zero, Trinity.CurrentWorldDynamicId, -1, 2, 2);
            }
            else if (CombatBase.CanCast(SNOPower.X1_Monk_MysticAlly_v2) && Runes.Monk.AirAlly.IsActive && Player.PrimaryResourcePct >= 0.10)
            {
                return new TrinityPower(SNOPower.X1_Monk_MysticAlly_v2, 0f, Vector3.Zero, Trinity.CurrentWorldDynamicId, -1, 2, 2);
            }
            else if (CombatBase.CanCast(SNOPower.X1_Monk_MysticAlly_v2) && Runes.Monk.EnduringAlly.IsActive && Player.CurrentHealthPct >= 0.4)
            {
                return new TrinityPower(SNOPower.X1_Monk_MysticAlly_v2, 0f, Vector3.Zero, Trinity.CurrentWorldDynamicId, -1, 2, 2);
            }

            // InnerSanctuary 
            if (!UseOOCBuff && TargetUtil.EliteOrTrashInRange(16f) && CombatBase.CanCast(SNOPower.X1_Monk_InnerSanctuary))
            {
                return new TrinityPower(SNOPower.X1_Monk_InnerSanctuary, 0f, Vector3.Zero, Trinity.CurrentWorldDynamicId, -1, 1, 1);
            }

            // Blinding Flash
            if (!UseOOCBuff && Player.PrimaryResource >= 20 && CombatBase.CanCast(SNOPower.Monk_BlindingFlash) &&
                (
                    TargetUtil.AnyElitesInRange(15, 1) ||
                    Player.CurrentHealthPct <= 0.4 ||
                    (TargetUtil.AnyMobsInRange(15, 3)) ||
                    (CurrentTarget.IsBossOrEliteRareUnique && CurrentTarget.RadiusDistance <= 15f) ||
                    // as pre-sweeping wind buff
                    (TargetUtil.AnyMobsInRange(15, 1) && CombatBase.CanCast(SNOPower.Monk_SweepingWind) && !GetHasBuff(SNOPower.Monk_SweepingWind) && hasInnaSet)
                ) &&
                // Check if either we don't have sweeping winds, or we do and it's ready to cast in a moment
                (CheckAbilityAndBuff(SNOPower.Monk_SweepingWind) ||
                 (!GetHasBuff(SNOPower.Monk_SweepingWind) &&
                 (CombatBase.CanCast(SNOPower.Monk_SweepingWind, CombatBase.CanCastFlags.NoTimer))) ||
                 Player.CurrentHealthPct <= 0.25))
            {
                return new TrinityPower(SNOPower.Monk_BlindingFlash, 0f, Vector3.Zero, Trinity.CurrentWorldDynamicId, -1, 0, 1);
            }

            // Blinding Flash as a DEFENSE
            if (!UseOOCBuff && Player.PrimaryResource >= 10 && CombatBase.CanCast(SNOPower.Monk_BlindingFlash) &&
                Player.CurrentHealthPct <= 0.75 && TargetUtil.AnyMobsInRange(15, 1))
            {
                return new TrinityPower(SNOPower.Monk_BlindingFlash, 0f, Vector3.Zero, Trinity.CurrentWorldDynamicId, -1, 0, 1);
            }

            // Breath of Heaven when needing healing or the buff
            if (!UseOOCBuff && (Player.CurrentHealthPct <= 0.6 || !GetHasBuff(SNOPower.Monk_BreathOfHeaven)) && CombatBase.CanCast(SNOPower.Monk_BreathOfHeaven) &&
                (Player.PrimaryResource >= 35 || (!CombatBase.CanCast(SNOPower.Monk_Serenity) && Player.PrimaryResource >= 25)))
            {
                return new TrinityPower(SNOPower.Monk_BreathOfHeaven, 0f, Vector3.Zero, Trinity.CurrentWorldDynamicId, -1, 1, 1);
            }

            // Breath of Heaven for spirit - Infused with Light
            if (!UseOOCBuff && !Player.IsIncapacitated && CombatBase.CanCast(SNOPower.Monk_BreathOfHeaven) && !GetHasBuff(SNOPower.Monk_BreathOfHeaven) && Runes.Monk.InfusedWithLight.IsActive &&
                (TargetUtil.AnyMobsInRange(3, 20) || TargetUtil.IsEliteTargetInRange(20)) && Player.PrimaryResourcePct < 0.75)
            {
                return new TrinityPower(SNOPower.Monk_BreathOfHeaven, 0f, Vector3.Zero, Trinity.CurrentWorldDynamicId, -1, 1, 1);
            }


            // Seven Sided Strike
            if (!UseOOCBuff && !IsCurrentlyAvoiding && !Player.IsIncapacitated &&
                (TargetUtil.AnyElitesInRange(15, 1) || (CurrentTarget.IsBossOrEliteRareUnique && CurrentTarget.RadiusDistance <= 15f) || Player.CurrentHealthPct <= 0.55) &&
                CombatBase.CanCast(SNOPower.Monk_SevenSidedStrike, CombatBase.CanCastFlags.NoTimer) && 
                ((Player.PrimaryResource >= 50 && !Player.WaitingForReserveEnergy) || Player.PrimaryResource >= MinEnergyReserve))
            {
                Monk_TickSweepingWindSpam();
                return new TrinityPower(SNOPower.Monk_SevenSidedStrike, 16f, CurrentTarget.Position, Trinity.CurrentWorldDynamicId, -1, 2, 3);
            }

            // WayOfTheHundredFists: apply fists of fury DoT if we have Infused with Light buff + WotHF:FoF
            if (!UseOOCBuff && Runes.Monk.InfusedWithLight.IsActive && Runes.Monk.FistsOfFury.IsActive && GetHasBuff(SNOPower.Monk_BreathOfHeaven) && !CurrentTarget.HasDotDPS)
            {
                Monk_TickSweepingWindSpam();
                return new TrinityPower(SNOPower.Monk_WayOfTheHundredFists, 14f, Vector3.Zero, -1, CurrentTarget.ACDGuid, 0, 1);
            }


            // Sweeping winds spam
            if ((Player.PrimaryResource >= 75 || (hasInnaSet && Player.PrimaryResource >= 5)) &&
                CombatBase.CanCast(SNOPower.Monk_SweepingWind, CombatBase.CanCastFlags.NoTimer) && GetHasBuff(SNOPower.Monk_SweepingWind) &&
                DateTime.UtcNow.Subtract(Trinity.SweepWindSpam).TotalMilliseconds >= 4000 && DateTime.UtcNow.Subtract(Trinity.SweepWindSpam).TotalMilliseconds <= 5400)
            {
                Trinity.SweepWindSpam = DateTime.UtcNow;
                return new TrinityPower(SNOPower.Monk_SweepingWind, 0f, Vector3.Zero, Trinity.CurrentWorldDynamicId, -1, 0, 0);
            }

            float minSweepingWindSpirit = hasInnaSet ? 5f : 75f;

            // Sweeping wind
            if (!UseOOCBuff && CombatBase.CanCast(SNOPower.Monk_SweepingWind) && !GetHasBuff(SNOPower.Monk_SweepingWind) &&
                ((TargetUtil.AnyElitesInRange(25, 1) || TargetUtil.AnyMobsInRange(20, 1) || hasInnaSet ||
                (CurrentTarget.IsBossOrEliteRareUnique && CurrentTarget.RadiusDistance <= 25f)) &&
                // Check our mantras, if we have them, are up first
                Monk_HasMantraAbilityAndBuff() &&
                // Check if either we don't have blinding flash, or we do and it's been cast in the last 8000ms
                (Trinity.TimeSinceUse(SNOPower.Monk_BlindingFlash) <= 8000 || CheckAbilityAndBuff(SNOPower.Monk_BlindingFlash) ||
                TargetUtil.AnyElitesInRange(25, 1) && Trinity.TimeSinceUse(SNOPower.Monk_BlindingFlash) <= 12500)) &&
                Player.PrimaryResource >= minSweepingWindSpirit)
            {
                Trinity.SweepWindSpam = DateTime.UtcNow;
                return new TrinityPower(SNOPower.Monk_SweepingWind, 0f, Vector3.Zero, Trinity.CurrentWorldDynamicId, -1, 0, 0);
            }

            // Sweeping Wind for Transcendance Health Regen
            if (CombatBase.CanCast(SNOPower.Monk_SweepingWind, CombatBase.CanCastFlags.NoTimer) &&
                Player.PrimaryResource >= minSweepingWindSpirit &&
                Passives.Monk.Transcendence.IsActive && Settings.Combat.Monk.SpamSweepingWindOnLowHP &&
                Player.CurrentHealthPct <= V.F("Monk.SweepingWind.SpamOnLowHealthPct") &&
                Trinity.TimeSinceUse(SNOPower.Monk_SweepingWind) > 500)
            {
                Trinity.SweepWindSpam = DateTime.UtcNow;
                return new TrinityPower(SNOPower.Monk_SweepingWind, 0f, Vector3.Zero, Trinity.CurrentWorldDynamicId, -1, 0, 0);
            }

            // Exploding Palm
            if (!UseOOCBuff && !IsCurrentlyAvoiding && !Player.IsIncapacitated &&
                CombatBase.CanCast(SNOPower.Monk_ExplodingPalm, CombatBase.CanCastFlags.NoTimer) &&
                !SpellTracker.IsUnitTracked(CurrentTarget, SNOPower.Monk_ExplodingPalm) &&
                Player.PrimaryResource >= 40)
            {
                return new TrinityPower(SNOPower.Monk_ExplodingPalm, 2f, Vector3.Zero, -1, CurrentTarget.ACDGuid, 1, 3);
            }

            // Make a mega-splosion
            if (ShouldSpreadExplodingPalm())
            {
                ChangeTarget();
            }

            // Dashing Strike
            if (CanCastDashingStrike)
            {
                TrinityPower dash = null;
                if (Legendary.Jawbreaker.IsEquipped && (dash = JawBreakerDashingStrike()) != null)
                {
                    return dash;
                }

                var cluster15Y3M = TargetUtil.ClusterExists(15f, 3);
                var cluster15Y3MPosition = TargetUtil.GetBestClusterPoint();

                if(CurrentTarget.IsEliteRareUnique || cluster15Y3M && TargetUtil.IsUnitWithDebuffInRangeOfPosition(15f, cluster15Y3MPosition, SNOPower.Monk_ExplodingPalm) ||
                TargetUtil.AnyMobsInRangeOfPosition(CurrentTarget.Position, 20f, 3) && Skills.Monk.ExplodingPalm.IsTrackedOnUnit(CurrentTarget))                
                {
                    Monk_TickSweepingWindSpam();
                    if (cluster15Y3M && Sets.ThousandStorms.IsMaxBonusActive)
                    {
                        return new TrinityPower(SNOPower.X1_Monk_DashingStrike, Monk_MaxDashingStrikeRange, cluster15Y3MPosition, Trinity.CurrentWorldDynamicId, -1, 2, 2);
                    }
                    return new TrinityPower(SNOPower.X1_Monk_DashingStrike, Monk_MaxDashingStrikeRange, CurrentTarget.Position, Trinity.CurrentWorldDynamicId, -1, 2, 2);
                }
               
            }

            
            var cycloneStrikeRange = Runes.Monk.Implosion.IsActive ? 34f : 24f;
            var cycloneStrikeSpirit = Runes.Monk.EyeOfTheStorm.IsActive ? 30 : 50;
            
            // Cyclone Strike
            if (!UseOOCBuff && !IsCurrentlyAvoiding && !Player.IsIncapacitated && CombatBase.CanCast(SNOPower.Monk_CycloneStrike) &&
                (
                 TargetUtil.AnyElitesInRange(cycloneStrikeRange, 1) ||
                 TargetUtil.AnyMobsInRange(cycloneStrikeRange, Settings.Combat.Monk.MinCycloneTrashCount) ||
                 (CurrentTarget.RadiusDistance >= 15f && CurrentTarget.RadiusDistance <= cycloneStrikeRange) // pull the current target into attack range
                ) &&

                // Cyclone if more than 25% of monsters within cyclone range are at least 10f away
                TargetUtil.IsPercentUnitsWithinBand(10f, cycloneStrikeRange, 0.25) &&

                (Player.PrimaryResource >= (cycloneStrikeSpirit + MinEnergyReserve)))
            {
                Monk_TickSweepingWindSpam();
                return new TrinityPower(SNOPower.Monk_CycloneStrike, 0f, Vector3.Zero, Trinity.CurrentWorldDynamicId, -1, 2, 2);
            }

            var minWoLSpirit = Runes.Monk.EmpoweredWave.IsActive ? 40 : 75;
            // Wave of light
            if (!UseOOCBuff && !IsCurrentlyAvoiding && !Player.IsIncapacitated && CombatBase.CanCast(SNOPower.Monk_WaveOfLight) &&
                (TargetUtil.AnyMobsInRange(16f, Settings.Combat.Monk.MinWoLTrashCount) || TargetUtil.IsEliteTargetInRange(20f)) &&
                (Player.PrimaryResource >= minWoLSpirit && !IsWaitingForSpecial || Player.PrimaryResource > MinEnergyReserve) &&
                // optional check for SW stacks
                (Settings.Combat.Monk.SWBeforeWoL && (CheckAbilityAndBuff(SNOPower.Monk_SweepingWind) && GetBuffStacks(SNOPower.Monk_SweepingWind) == 3) || !Settings.Combat.Monk.SWBeforeWoL) &&
                Monk_HasMantraAbilityAndBuff())
            {
                Monk_TickSweepingWindSpam();
                return new TrinityPower(SNOPower.Monk_WaveOfLight, 16f, TargetUtil.GetBestClusterPoint(), -1, CurrentTarget.ACDGuid, 0, 1);
            }

            // For tempest rush re-use
            if (!UseOOCBuff && Player.PrimaryResource >= 15 && CombatBase.CanCast(SNOPower.Monk_TempestRush) &&
                Trinity.TimeSinceUse(SNOPower.Monk_TempestRush) <= 150 &&
                ((Settings.Combat.Monk.TROption != TempestRushOption.MovementOnly) &&
                !(Settings.Combat.Monk.TROption == TempestRushOption.TrashOnly && TargetUtil.AnyElitesInRange(40f))))
            {
                GenerateMonkZigZag();
                Trinity.MaintainTempestRush = true;
                const string trUse = "Continuing Tempest Rush for Combat";
                Monk_TempestRushStatus(trUse);
                return new TrinityPower(SNOPower.Monk_TempestRush, 23f, CombatBase.ZigZagPosition, Trinity.CurrentWorldDynamicId, -1, 0, 0);
            }

            // Tempest rush at elites or groups of mobs
            if (!UseOOCBuff && !IsCurrentlyAvoiding && !Player.IsIncapacitated && !Player.IsRooted && CombatBase.CanCast(SNOPower.Monk_TempestRush) &&
                ((Player.PrimaryResource >= Settings.Combat.Monk.TR_MinSpirit && !Player.WaitingForReserveEnergy) || Player.PrimaryResource >= MinEnergyReserve) &&
                (Settings.Combat.Monk.TROption == TempestRushOption.Always ||
                Settings.Combat.Monk.TROption == TempestRushOption.CombatOnly ||
                (Settings.Combat.Monk.TROption == TempestRushOption.ElitesGroupsOnly && (TargetUtil.AnyElitesInRange(25) || TargetUtil.AnyMobsInRange(25, 2))) ||
                (Settings.Combat.Monk.TROption == TempestRushOption.TrashOnly && !TargetUtil.AnyElitesInRange(90f) && TargetUtil.AnyMobsInRange(40f))))
            {
                GenerateMonkZigZag();
                Trinity.MaintainTempestRush = true;
                const string trUse = "Starting Tempest Rush for Combat";
                Monk_TempestRushStatus(trUse);
                return new TrinityPower(SNOPower.Monk_TempestRush, 23f, CombatBase.ZigZagPosition, Trinity.CurrentWorldDynamicId, -1, 0, 0);
            }

            // Lashing Tail Kick
            if (!UseOOCBuff && !IsCurrentlyAvoiding && CombatBase.CanCast(SNOPower.Monk_LashingTailKick) && !Player.IsIncapacitated &&
                // Either doesn't have sweeping wind, or does but the buff is already up
                (!Hotbar.Contains(SNOPower.Monk_SweepingWind) || (Hotbar.Contains(SNOPower.Monk_SweepingWind) && GetHasBuff(SNOPower.Monk_SweepingWind))) &&
                ((Player.PrimaryResource >= 65 && !Player.WaitingForReserveEnergy) || Player.PrimaryResource >= MinEnergyReserve))
            {
                Monk_TickSweepingWindSpam();
                return new TrinityPower(SNOPower.Monk_LashingTailKick, 10f, Vector3.Zero, -1, CurrentTarget.ACDGuid, 1, 1);
            }

            // 4 Mantra spam for the 4 second buff
            if (!UseOOCBuff && !IsCurrentlyAvoiding && !Player.IsIncapacitated && !Settings.Combat.Monk.DisableMantraSpam)
            {
                if (CombatBase.CanCast(SNOPower.X1_Monk_MantraOfConviction_v2) && !GetHasBuff(SNOPower.X1_Monk_MantraOfConviction_v2) &&
                    (Player.PrimaryResource >= 50) && CurrentTarget != null)
                {
                    return new TrinityPower(SNOPower.X1_Monk_MantraOfConviction_v2);
                }

                if (CombatBase.CanCast(SNOPower.X1_Monk_MantraOfRetribution_v2) && !GetHasBuff(SNOPower.X1_Monk_MantraOfRetribution_v2) &&
                    (Player.PrimaryResource >= 50) && CurrentTarget != null)
                {
                    return new TrinityPower(SNOPower.X1_Monk_MantraOfRetribution_v2);
                }
            }

            //Use Mantra of Healing active if health is low for shield or spam it if we're using SWK build.
            bool isSWK = Sets.MonkeyKingsGarb.IsSecondBonusActive;
            if (CombatBase.CanCast(SNOPower.X1_Monk_MantraOfHealing_v2) &&
                (!isSWK && Player.CurrentHealthPct <= V.F("Monk.MantraOfHealing.UseHealthPct") || isSWK) &&
                !Player.IsIncapacitated && !GetHasBuff(SNOPower.X1_Monk_MantraOfHealing_v2))
            {
                return new TrinityPower(SNOPower.X1_Monk_MantraOfHealing_v2);
            }

            if (CombatBase.CanCast(SNOPower.X1_Monk_MantraOfEvasion_v2) &&
                (!isSWK && Player.CurrentHealthPct <= V.F("Monk.MantraOfHealing.UseHealthPct") || isSWK) &&
                !GetHasBuff(SNOPower.X1_Monk_MantraOfEvasion_v2) && CurrentTarget != null)
            {
                return new TrinityPower(SNOPower.X1_Monk_MantraOfEvasion_v2);
            }

            
            /*
             * Dual/Trigen Monk section
             * 
             * Cycle through Deadly Reach, Way of the Hundred Fists, and Fists of Thunder every 3 seconds to keep 8% passive buff up if we have Combination Strike
             *  - or - 
             * Keep Foresight and Blazing Fists buffs up every 30/5 seconds
             */
            bool hasCombinationStrike = Passives.Monk.CombinationStrike.IsActive;
            bool isDualOrTriGen = HotbarSkills.AssignedSkills.Count(s =>
                s.Power == SNOPower.Monk_DeadlyReach ||
                s.Power == SNOPower.Monk_WayOfTheHundredFists ||
                s.Power == SNOPower.Monk_FistsofThunder ||
                s.Power == SNOPower.Monk_CripplingWave) >= 2 && hasCombinationStrike;

            // interval in milliseconds for Generators
            int drInterval = 0;
            if (hasCombinationStrike)
                drInterval = 2500;
            else if (Runes.Monk.Foresight.IsActive)
                drInterval = 29000;

            int wothfInterval = 0;
            if (hasCombinationStrike)
                wothfInterval = 2500;
            else if (Runes.Monk.BlazingFists.IsActive)
                wothfInterval = 4500;

            int cwInterval = 0;
            if (hasCombinationStrike)
                cwInterval = 2500;

            // Fists of Thunder:Thunder Clap - Fly to Target
            if (!UseOOCBuff && !IsCurrentlyAvoiding && CombatBase.CanCast(SNOPower.Monk_FistsofThunder) && Runes.Monk.Thunderclap.IsActive && CurrentTarget.Distance > 16f)
            {
                Monk_TickSweepingWindSpam();
                return new TrinityPower(SNOPower.Monk_FistsofThunder, 30f, Vector3.Zero, -1, CurrentTarget.ACDGuid, 0, 3);
            }

            // Deadly Reach: Foresight, every 27 seconds or 2.7 seconds with combo strike
            if (!UseOOCBuff && !IsCurrentlyAvoiding && CombatBase.CanCast(SNOPower.Monk_DeadlyReach) && (isDualOrTriGen || Runes.Monk.Foresight.IsActive) &&
                (SpellHistory.TimeSinceUse(SNOPower.Monk_DeadlyReach) > TimeSpan.FromMilliseconds(drInterval) ||
                (SpellHistory.SpellUseCountInTime(SNOPower.Monk_DeadlyReach, TimeSpan.FromMilliseconds(27000)) < 3) && Runes.Monk.Foresight.IsActive))
            {
                Monk_TickSweepingWindSpam();
                return new TrinityPower(SNOPower.Monk_DeadlyReach, 16f, Vector3.Zero, -1, CurrentTarget.ACDGuid, 0, 3);
            }

            // Way of the Hundred Fists: Blazing Fists, every 4-5ish seconds or if we don't have 3 stacks of the buff or or 2.7 seconds with combo strike
            if (!UseOOCBuff && !IsCurrentlyAvoiding && CombatBase.CanCast(SNOPower.Monk_WayOfTheHundredFists) && (isDualOrTriGen || Runes.Monk.BlazingFists.IsActive) &&
                (GetBuffStacks(SNOPower.Monk_WayOfTheHundredFists) < 3 ||
                SpellHistory.TimeSinceUse(SNOPower.Monk_WayOfTheHundredFists) > TimeSpan.FromMilliseconds(wothfInterval)))
            {
                Monk_TickSweepingWindSpam();
                return new TrinityPower(SNOPower.Monk_WayOfTheHundredFists, 16f, Vector3.Zero, -1, CurrentTarget.ACDGuid, 0, 3);
            }

            // Crippling Wave
            if (!UseOOCBuff && !IsCurrentlyAvoiding && CombatBase.CanCast(SNOPower.Monk_CripplingWave) &&
                SpellHistory.TimeSinceUse(SNOPower.Monk_CripplingWave) > TimeSpan.FromMilliseconds(cwInterval))
            {
                Monk_TickSweepingWindSpam();
                return new TrinityPower(SNOPower.Monk_CripplingWave, 20f, Vector3.Zero, -1, CurrentTarget.ACDGuid, 0, 3);
            }

            // Fists of Thunder
            if (!UseOOCBuff && !IsCurrentlyAvoiding && CombatBase.CanCast(SNOPower.Monk_FistsofThunder))
            {
                Monk_TickSweepingWindSpam();
                return new TrinityPower(SNOPower.Monk_FistsofThunder, 30f, Vector3.Zero, -1, CurrentTarget.ACDGuid, 0, 3);
            }

            // Deadly Reach normal
            if (!UseOOCBuff && !IsCurrentlyAvoiding && CombatBase.CanCast(SNOPower.Monk_DeadlyReach))
            {
                Monk_TickSweepingWindSpam();
                return new TrinityPower(SNOPower.Monk_DeadlyReach, 16f, Vector3.Zero, -1, CurrentTarget.ACDGuid, 0, 3);
            }

            // Way of the Hundred Fists normal
            if (!UseOOCBuff && !IsCurrentlyAvoiding && CombatBase.CanCast(SNOPower.Monk_WayOfTheHundredFists))
            {
                Monk_TickSweepingWindSpam();
                return new TrinityPower(SNOPower.Monk_WayOfTheHundredFists, 16f, Vector3.Zero, -1, CurrentTarget.ACDGuid, 0, 3);
            }

            // Crippling Wave Normal
            if (!UseOOCBuff && !IsCurrentlyAvoiding && CombatBase.CanCast(SNOPower.Monk_CripplingWave))
            {
                Monk_TickSweepingWindSpam();
                return new TrinityPower(SNOPower.Monk_CripplingWave, 30f, Vector3.Zero, -1, CurrentTarget.ACDGuid, 0, 3);
            }

            // Default attacks
            return CombatBase.DefaultPower;
        }

        internal static bool ShouldSpreadExplodingPalm()
        {
            return Skills.Monk.ExplodingPalm.IsActive && 
                
                // enough resources and mobs nearby
                Player.PrimaryResourcePct > 0.30 && TargetUtil.AnyMobsInRange(15f, 4) &&

                // Don't bother if 3 or more targets already have EP
                !TargetUtil.IsUnitWithDebuffInRangeOfPosition(15f, TargetUtil.GetBestClusterPoint(), SNOPower.Monk_ExplodingPalm, 3) &&

                // Avoid rapidly changing targets
                DateTime.UtcNow.Subtract(_lastTargetChange).TotalMilliseconds > 1500 &&

                // Current target is valid
                CurrentTarget != null && CurrentTarget.IsUnit && !CurrentTarget.IsTreasureGoblin;
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
            if (lowestHealthTarget.ACDGuid == currentTarget.ACDGuid && !(lowestHealthTarget.HitPointsPct > 0.2)) return;

            Trinity.CurrentTarget = lowestHealthTarget;
            //Logger.LogNormal("Found lowest health target {0} {1} ({2:0.##}%)", CurrentTarget.InternalName, CurrentTarget.CommonData.ACDGuid, lowestHealthTarget.HitPointsPct * 100);
        }

        private static bool CanCastDashingStrike
        {
            get { return !UseOOCBuff && !Player.IsIncapacitated && CanCast(SNOPower.X1_Monk_DashingStrike, CanCastFlags.NoTimer); }
        }

        /// <summary>
        /// blahblah999's Dashing Strike with JawBreaker Item
        /// https://www.thebuddyforum.com/demonbuddy-forum/plugins/trinity/167966-monk-trinity-mod-dash-strike-spam-rots-set-bonus-jawbreaker.html
        /// </summary>
        internal static TrinityPower JawBreakerDashingStrike()
        {
            float Monk_JawbreakerRange = Convert.ToSingle(Settings.Combat.Monk.MinJawBreakerRange);
            var farthestTarget = TargetUtil.GetDashStrikeFarthestTarget(49f, Monk_JawbreakerRange);

            // able to cast
            if (TargetUtil.AnyMobsInRange(25f, 3) || TargetUtil.IsEliteTargetInRange(70f)) // surround by mobs or elite engaged.
            {
                if (farthestTarget != null) // found a target within 33-49 yards.
                {
                    Monk_TickSweepingWindSpam();
                    return new TrinityPower(SNOPower.X1_Monk_DashingStrike, Monk_MaxDashingStrikeRange, farthestTarget.Position, Trinity.CurrentWorldDynamicId, -1, 2, 2);
                }
                // no free target found, get a nearby cluster point instead.
                var bestClusterPoint = TargetUtil.GetBestClusterPoint(15f, Monk_JawbreakerRange);
                Monk_TickSweepingWindSpam();
                return new TrinityPower(SNOPower.X1_Monk_DashingStrike, Monk_MaxDashingStrikeRange, bestClusterPoint, Trinity.CurrentWorldDynamicId, -1, 2, 2);
            }

            //usually this trigger after dash to the farthest target. dash a single mobs >30 yards, trying to dash back the cluster
            if (TargetUtil.ClusterExists(20, 50, 3)) 
            {
                var dashStrikeBestClusterPoint = TargetUtil.GetDashStrikeBestClusterPoint(20f, 50f, Monk_JawbreakerRange);
                if (dashStrikeBestClusterPoint != Trinity.Player.Position)
                {
                    Monk_TickSweepingWindSpam();
                    return new TrinityPower(SNOPower.X1_Monk_DashingStrike, Monk_MaxDashingStrikeRange, dashStrikeBestClusterPoint, Trinity.CurrentWorldDynamicId, -1, 2, 2);
                }
            }
                
            // dash to anything which is free.               
            if (farthestTarget != null)
            {
                Monk_TickSweepingWindSpam();
                return new TrinityPower(SNOPower.X1_Monk_DashingStrike, Monk_MaxDashingStrikeRange, farthestTarget.Position, Trinity.CurrentWorldDynamicId, -1, 2, 2);
            }

            return new TrinityPower(SNOPower.X1_Monk_DashingStrike, Monk_MaxDashingStrikeRange, CurrentTarget.Position, Trinity.CurrentWorldDynamicId, -1, 2, 2);
        }

        internal static void Monk_TickSweepingWindSpam()
        {
            if (GetHasBuff(SNOPower.Monk_SweepingWind) && DateTime.UtcNow.Subtract(Trinity.SweepWindSpam).TotalMilliseconds < 5500)
                Trinity.SweepWindSpam = DateTime.UtcNow;
        }

        internal static void GenerateMonkZigZag()
        {
            float fExtraDistance = CurrentTarget.RadiusDistance <= 20f ? 15f : 20f;
            CombatBase.ZigZagPosition = TargetUtil.GetZigZagTarget(CurrentTarget.Position, fExtraDistance);
            double direction = MathUtil.FindDirectionRadian(Player.Position, CombatBase.ZigZagPosition);
            CombatBase.ZigZagPosition = MathEx.GetPointAt(Player.Position, 40f, (float)direction);
            Logger.Log(TrinityLogLevel.Debug, LogCategory.Behavior, "Generated ZigZag {0} distance {1:0}", CombatBase.ZigZagPosition, CombatBase.ZigZagPosition.Distance2D(Player.Position));
            Trinity.LastZigZagUnitAcdGuid = CurrentTarget.ACDGuid;
            Trinity.LastChangedZigZag = DateTime.UtcNow;
        }

        internal static TrinityPower GetMonkDestroyPower()
        {
            if (Monk_TempestRushReady())
                return new TrinityPower(SNOPower.Monk_TempestRush, 5f, Vector3.Zero, -1, -1, 0, 0);
            //if (CombatBase.CanCast(SNOPower.X1_Monk_DashingStrike))
            //    return new TrinityPower(SNOPower.X1_Monk_DashingStrike, Monk_MaxDashingStrikeRange, Vector3.Zero, -1, -1, 0, 0);
            if (CombatBase.CanCast(SNOPower.Monk_FistsofThunder))
                return new TrinityPower(SNOPower.Monk_FistsofThunder, 5f, Vector3.Zero, -1, -1, 0, 0);
            if (CombatBase.CanCast(SNOPower.Monk_DeadlyReach))
                return new TrinityPower(SNOPower.Monk_DeadlyReach, 5f, Vector3.Zero, -1, -1, 0, 0);
            if (CombatBase.CanCast(SNOPower.Monk_CripplingWave))
                return new TrinityPower(SNOPower.Monk_CripplingWave, 5f, Vector3.Zero, -1, -1, 0, 0);
            if (CombatBase.CanCast(SNOPower.Monk_WayOfTheHundredFists))
                return new TrinityPower(SNOPower.Monk_WayOfTheHundredFists, 5f, Vector3.Zero, -1, -1, 0, 0);
            return CombatBase.DefaultPower;
        }

        /// <summary>
        /// Returns true if we have a mantra and it's up, or if we don't have a Mantra at all
        /// </summary>
        /// <returns></returns>
        internal static bool Monk_HasMantraAbilityAndBuff()
        {
            return
                (CheckAbilityAndBuff(SNOPower.X1_Monk_MantraOfConviction_v2) ||
                CheckAbilityAndBuff(SNOPower.X1_Monk_MantraOfEvasion_v2) ||
                CheckAbilityAndBuff(SNOPower.X1_Monk_MantraOfHealing_v2) ||
                CheckAbilityAndBuff(SNOPower.X1_Monk_MantraOfRetribution_v2) ||
                DoesNotHaveMonkMantraAbility());
        }
        internal static bool DoesNotHaveMonkMantraAbility()
        {
            return
                (!Hotbar.Contains(SNOPower.X1_Monk_MantraOfConviction_v2) &&
                !Hotbar.Contains(SNOPower.X1_Monk_MantraOfEvasion_v2) &&
                !Hotbar.Contains(SNOPower.X1_Monk_MantraOfHealing_v2) &&
                !Hotbar.Contains(SNOPower.X1_Monk_MantraOfRetribution_v2));
        }

        internal static bool Monk_TempestRushReady()
        {
            if (Player.ActorClass != ActorClass.Monk)
                return false;

            if (!Hotbar.Contains(SNOPower.Monk_TempestRush))
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

            if (!Hotbar.Contains(SNOPower.Monk_TempestRush))
                return false;

            if (!Monk_HasMantraAbilityAndBuff())
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

        internal static void Monk_MaintainTempestRush()
        {
            if (!Monk_TempestRushReady())
                return;

            if (Player.IsInTown || Zeta.Bot.Logic.BrainBehavior.IsVendoring)
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
                    case GObjectType.Unit:
                    case GObjectType.Gold:
                    case GObjectType.Avoidance:
                    case GObjectType.Barricade:
                    case GObjectType.Destructible:
                    case GObjectType.HealthGlobe:
                    case GObjectType.PowerGlobe:
                        {
                            if (Settings.Combat.Monk.TROption == TempestRushOption.TrashOnly &&
                                (TargetUtil.AnyElitesInRange(40f) || CurrentTarget.IsBossOrEliteRareUnique))
                                shouldMaintain = false;
                            else
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

                float DestinationDistance = target.Distance2D(ZetaDia.Me.Position);

                target = TargetUtil.FindTempestRushTarget();

                if (DestinationDistance > 10f && NavHelper.CanRayCast(ZetaDia.Me.Position, target))
                {
                    Monk_TempestRushStatus(String.Format("Using Tempest Rush to maintain channeling, source={0}, V3={1} dist={2:0}", locationSource, target, DestinationDistance));

                    var usePowerResult = ZetaDia.Me.UsePower(SNOPower.Monk_TempestRush, target, Trinity.CurrentWorldDynamicId, -1);
                    if (usePowerResult)
                    {
                        CacheData.AbilityLastUsed[SNOPower.Monk_TempestRush] = DateTime.UtcNow;
                    }
                }
            }
        }

        internal static void Monk_TempestRushStatus(string trUse)
        {

            Logger.Log(TrinityLogLevel.Debug, LogCategory.Behavior, "{0}, xyz={4} spirit={1:0} cd={2} lastUse={3:0}",
                trUse,
                Trinity.Player.PrimaryResource, PowerManager.CanCast(SNOPower.Monk_TempestRush),
                Trinity.TimeSinceUse(SNOPower.Monk_TempestRush), CombatBase.ZigZagPosition);
        }



    }
}
