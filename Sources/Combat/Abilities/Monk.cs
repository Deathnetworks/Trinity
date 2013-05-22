using Trinity.Technicals;
using System;
using System.Linq;
using Zeta.Common;
using Zeta.Common.Plugins;
using Zeta.CommonBot;
using Zeta.Internals.Actors;
using Trinity.Settings.Combat;
using Zeta;
using Trinity.DbProvider;
using Zeta.CommonBot.Profile.Common;

namespace Trinity
{
    public partial class Trinity : IPlugin
    {
        private static float Monk_MaxDashingStrikeRange = 55f;
        internal static Vector3 LastTempestRushLocation = Vector3.Zero;

        private static TrinityPower GetMonkPower(bool IsCurrentlyAvoiding, bool UseOOCBuff, bool UseDestructiblePower)
        {
            if (UseDestructiblePower)
                return GetMonkDestroyPower();

            // Monks need 80 for special spam like tempest rushing
            MinEnergyReserve = 80;

            bool hasInfusedWithLight = HotbarSkills.AssignedSkills.Any(s => s.Power == SNOPower.Monk_BreathOfHeaven && s.RuneIndex == 3);
            bool hasFistsOfFury = HotbarSkills.AssignedSkills.Any(s => s.Power == SNOPower.Monk_WayOfTheHundredFists && s.RuneIndex == 0);

            // apply fists of fury DoT if we have Infused with Light buff + WotHF:FoF
            if (!UseOOCBuff && hasInfusedWithLight && hasFistsOfFury && GetHasBuff(SNOPower.Monk_BreathOfHeaven) && !CurrentTarget.HasDotDPS)
            {
                Monk_TickSweepingWindSpam();
                return new TrinityPower(SNOPower.Monk_WayOfTheHundredFists, 14f, vNullLocation, -1, CurrentTarget.ACDGuid, 0, 1, NO_WAIT_ANIM);
            }

            // 4 Mantras for the initial buff (slow-use)
            if (Hotbar.Contains(SNOPower.Monk_MantraOfEvasion) && !GetHasBuff(SNOPower.Monk_MantraOfEvasion) &&
                PlayerStatus.PrimaryResource >= 50 && GilesUseTimer(SNOPower.Monk_MantraOfEvasion, true))
            {
                return new TrinityPower(SNOPower.Monk_MantraOfEvasion, 0f, vNullLocation, CurrentWorldDynamicId, -1, 0, 1, WAIT_FOR_ANIM);
            }
            if (Hotbar.Contains(SNOPower.Monk_MantraOfConviction) && !GetHasBuff(SNOPower.Monk_MantraOfConviction) &&
                (PlayerStatus.PrimaryResource >= 50 && PlayerStatus.PrimaryResource >= 85) && GilesUseTimer(SNOPower.Monk_MantraOfConviction, true))
            {
                return new TrinityPower(SNOPower.Monk_MantraOfConviction, 0f, vNullLocation, CurrentWorldDynamicId, -1, 0, 1, WAIT_FOR_ANIM);
            }
            if (Hotbar.Contains(SNOPower.Monk_MantraOfHealing) && !GetHasBuff(SNOPower.Monk_MantraOfHealing) &&
                PlayerStatus.PrimaryResource >= 50 && GilesUseTimer(SNOPower.Monk_MantraOfHealing, true))
            {
                return new TrinityPower(SNOPower.Monk_MantraOfHealing, 0f, vNullLocation, CurrentWorldDynamicId, -1, 0, 1, WAIT_FOR_ANIM);
            }
            if (Hotbar.Contains(SNOPower.Monk_MantraOfRetribution) && !GetHasBuff(SNOPower.Monk_MantraOfRetribution) &&
                PlayerStatus.PrimaryResource >= 50 && GilesUseTimer(SNOPower.Monk_MantraOfRetribution, true))
            {
                return new TrinityPower(SNOPower.Monk_MantraOfRetribution, 0f, vNullLocation, CurrentWorldDynamicId, -1, 0, 1, WAIT_FOR_ANIM);
            }
            // Mystic ally
            if (Hotbar.Contains(SNOPower.Monk_MysticAlly) && PlayerStatus.PrimaryResource >= 25 && iPlayerOwnedMysticAlly == 0 &&
                GilesUseTimer(SNOPower.Monk_MysticAlly) && PowerManager.CanCast(SNOPower.Monk_MysticAlly))
            {
                return new TrinityPower(SNOPower.Monk_MysticAlly, 0f, vNullLocation, CurrentWorldDynamicId, -1, 2, 2, WAIT_FOR_ANIM);
            }
            // InnerSanctuary
            if (!UseOOCBuff && PlayerStatus.CurrentHealthPct <= 0.45 && Hotbar.Contains(SNOPower.Monk_InnerSanctuary) &&
                GilesUseTimer(SNOPower.Monk_InnerSanctuary, true) &&
                PlayerStatus.PrimaryResource >= 30 && PowerManager.CanCast(SNOPower.Monk_InnerSanctuary))
            {
                return new TrinityPower(SNOPower.Monk_InnerSanctuary, 0f, vNullLocation, CurrentWorldDynamicId, -1, 1, 1, WAIT_FOR_ANIM);
            }
            // Serenity if health is low
            if ((PlayerStatus.CurrentHealthPct <= 0.50 || (PlayerStatus.IsIncapacitated && PlayerStatus.CurrentHealthPct <= 0.90)) && Hotbar.Contains(SNOPower.Monk_Serenity) &&
                GilesUseTimer(SNOPower.Monk_Serenity, true) &&
                PlayerStatus.PrimaryResource >= 10 && PowerManager.CanCast(SNOPower.Monk_Serenity))
            {
                return new TrinityPower(SNOPower.Monk_Serenity, 0f, vNullLocation, CurrentWorldDynamicId, -1, 1, 1, WAIT_FOR_ANIM);
            }

            // Blinding Flash
            if (!UseOOCBuff && PlayerStatus.PrimaryResource >= 20 && Hotbar.Contains(SNOPower.Monk_BlindingFlash) &&
                (
                    ElitesWithinRange[RANGE_15] >= 1 ||
                    PlayerStatus.CurrentHealthPct <= 0.4 ||
                    (AnythingWithinRange[RANGE_15] >= 3) ||
                    (CurrentTarget.IsBossOrEliteRareUnique && CurrentTarget.RadiusDistance <= 15f) ||
                // as pre-sweeping wind buff
                    (AnythingWithinRange[RANGE_15] >= 1 && Hotbar.Contains(SNOPower.Monk_SweepingWind) && !GetHasBuff(SNOPower.Monk_SweepingWind) && Settings.Combat.Monk.HasInnaSet)
                ) &&
                // Check if either we don't have sweeping winds, or we do and it's ready to cast in a moment
                (CheckAbilityAndBuff(SNOPower.Monk_SweepingWind) ||
                 (Hotbar.Contains(SNOPower.Monk_SweepingWind) && !GetHasBuff(SNOPower.Monk_SweepingWind) &&
                 (Settings.Combat.Monk.HasInnaSet ? PlayerStatus.PrimaryResource >= 15 : PlayerStatus.PrimaryResource >= 85)) ||
                 PlayerStatus.CurrentHealthPct <= 0.25) &&
                GilesUseTimer(SNOPower.Monk_BlindingFlash) && PowerManager.CanCast(SNOPower.Monk_BlindingFlash))
            {
                return new TrinityPower(SNOPower.Monk_BlindingFlash, 0f, vNullLocation, CurrentWorldDynamicId, -1, 0, 1, WAIT_FOR_ANIM);
            }
            // Blinding Flash as a DEFENSE
            if (!UseOOCBuff && PlayerStatus.PrimaryResource >= 10 && Hotbar.Contains(SNOPower.Monk_BlindingFlash) &&
                PlayerStatus.CurrentHealthPct <= 0.25 && AnythingWithinRange[RANGE_15] >= 1 &&
                GilesUseTimer(SNOPower.Monk_BlindingFlash) && PowerManager.CanCast(SNOPower.Monk_BlindingFlash))
            {
                return new TrinityPower(SNOPower.Monk_BlindingFlash, 0f, vNullLocation, CurrentWorldDynamicId, -1, 0, 1, WAIT_FOR_ANIM);
            }

            // Sweeping winds spam
            if ((PlayerStatus.PrimaryResource >= 75 || (Settings.Combat.Monk.HasInnaSet && PlayerStatus.PrimaryResource >= 5)) && GilesUseTimer(SNOPower.Monk_SweepingWind) &&
                Hotbar.Contains(SNOPower.Monk_SweepingWind) && GetHasBuff(SNOPower.Monk_SweepingWind) &&
                DateTime.Now.Subtract(SweepWindSpam).TotalMilliseconds >= 4000 && DateTime.Now.Subtract(SweepWindSpam).TotalMilliseconds <= 5400)
            {
                SweepWindSpam = DateTime.Now;
                return new TrinityPower(SNOPower.Monk_SweepingWind, 0f, vNullLocation, CurrentWorldDynamicId, -1, 0, 0, NO_WAIT_ANIM);
            }

            // Sweeping wind
            if (!UseOOCBuff && Hotbar.Contains(SNOPower.Monk_SweepingWind) && !GetHasBuff(SNOPower.Monk_SweepingWind) && GilesUseTimer(SNOPower.Monk_SweepingWind) &&
                (ElitesWithinRange[RANGE_25] > 0 || AnythingWithinRange[RANGE_20] >= 3 || Settings.Combat.Monk.HasInnaSet ||
                (CurrentTarget.IsBossOrEliteRareUnique && CurrentTarget.RadiusDistance <= 25f)) &&
                // Check our mantras, if we have them, are up first
                Monk_HasMantraAbilityAndBuff() &&
                // Check if either we don't have blinding flash, or we do and it's been cast in the last 8000ms
                (DateTime.Now.Subtract(dictAbilityLastUse[SNOPower.Monk_BlindingFlash]).TotalMilliseconds <= 8000 || CheckAbilityAndBuff(SNOPower.Monk_BlindingFlash) ||
                ElitesWithinRange[RANGE_25] > 0 && DateTime.Now.Subtract(dictAbilityLastUse[SNOPower.Monk_BlindingFlash]).TotalMilliseconds <= 12500) &&
                // Check the re-use timer and energy costs
                (PlayerStatus.PrimaryResource >= 75 || (Settings.Combat.Monk.HasInnaSet && PlayerStatus.PrimaryResource >= 5)))
            {
                SweepWindSpam = DateTime.Now;
                return new TrinityPower(SNOPower.Monk_SweepingWind, 0f, vNullLocation, CurrentWorldDynamicId, -1, 0, 0, NO_WAIT_ANIM);
            }

            //skillDict.Add("BreathOfHeaven", SNOPower.Monk_BreathOfHeaven);
            //runeDict.Add("CircleOfScorn", 0);
            //runeDict.Add("CircleOfLife", 1);
            //runeDict.Add("BlazingWrath", 2);
            //runeDict.Add("InfusedWithLight", 3);
            //runeDict.Add("PenitentFlame", 4);

            // Breath of Heaven when needing healing or the buff
            if (!UseOOCBuff && (PlayerStatus.CurrentHealthPct <= 0.6 || !GetHasBuff(SNOPower.Monk_BreathOfHeaven)) && Hotbar.Contains(SNOPower.Monk_BreathOfHeaven) &&
                (PlayerStatus.PrimaryResource >= 35 || (!Hotbar.Contains(SNOPower.Monk_Serenity) && PlayerStatus.PrimaryResource >= 25)) &&
                GilesUseTimer(SNOPower.Monk_BreathOfHeaven) && PowerManager.CanCast(SNOPower.Monk_BreathOfHeaven))
            {
                return new TrinityPower(SNOPower.Monk_BreathOfHeaven, 0f, vNullLocation, CurrentWorldDynamicId, -1, 1, 1, WAIT_FOR_ANIM);
            }
            // Breath of Heaven for spirit - Infused with Light
            if (!UseOOCBuff && !PlayerStatus.IsIncapacitated && Hotbar.Contains(SNOPower.Monk_BreathOfHeaven) && !GetHasBuff(SNOPower.Monk_BreathOfHeaven) && hasInfusedWithLight &&
                (TargetUtil.AnyMobsInRange(3, 20) || TargetUtil.IsEliteTargetInRange(20)) && PlayerStatus.PrimaryResourcePct < 0.75 &&
                PowerManager.CanCast(SNOPower.Monk_BreathOfHeaven))
            {
                return new TrinityPower(SNOPower.Monk_BreathOfHeaven, 0f, vNullLocation, CurrentWorldDynamicId, -1, 1, 1, WAIT_FOR_ANIM);
            }

            // Seven Sided Strike
            if (!UseOOCBuff && !IsCurrentlyAvoiding && !PlayerStatus.IsIncapacitated &&
                (ElitesWithinRange[RANGE_15] >= 1 || (CurrentTarget.IsBossOrEliteRareUnique && CurrentTarget.RadiusDistance <= 15f) || PlayerStatus.CurrentHealthPct <= 0.55) &&
                Hotbar.Contains(SNOPower.Monk_SevenSidedStrike) && ((PlayerStatus.PrimaryResource >= 50 && !PlayerStatus.WaitingForReserveEnergy) || PlayerStatus.PrimaryResource >= MinEnergyReserve) &&
                GilesUseTimer(SNOPower.Monk_SevenSidedStrike, true) && PowerManager.CanCast(SNOPower.Monk_SevenSidedStrike))
            {
                Monk_TickSweepingWindSpam();
                return new TrinityPower(SNOPower.Monk_SevenSidedStrike, 16f, CurrentTarget.Position, CurrentWorldDynamicId, -1, 2, 3, WAIT_FOR_ANIM);
            }
            // Exploding Palm
            if (!UseOOCBuff && !IsCurrentlyAvoiding && !PlayerStatus.IsIncapacitated &&
                (ElitesWithinRange[RANGE_25] > 0 || AnythingWithinRange[RANGE_15] >= 3 || (CurrentTarget.IsBossOrEliteRareUnique && CurrentTarget.RadiusDistance <= 14f)) &&
                Hotbar.Contains(SNOPower.Monk_ExplodingPalm) &&
                ((PlayerStatus.PrimaryResource >= 40 && !PlayerStatus.WaitingForReserveEnergy) || PlayerStatus.PrimaryResource >= MinEnergyReserve) &&
                GilesUseTimer(SNOPower.Monk_ExplodingPalm) && PowerManager.CanCast(SNOPower.Monk_ExplodingPalm))
            {
                return new TrinityPower(SNOPower.Monk_ExplodingPalm, 14f, vNullLocation, -1, CurrentTarget.ACDGuid, 1, 1, WAIT_FOR_ANIM);
            }

            //SkillDict.Add("WaveOfLight", SNOPower.Monk_WaveOfLight);
            //RuneDict.Add("WallOfLight", 0);
            //RuneDict.Add("ExplosiveLight", 1);
            //RuneDict.Add("EmpoweredWave", 3);
            //RuneDict.Add("BlindingLight", 4);
            //RuneDict.Add("PillarOfTheAncients", 2);
            //bool hasEmpoweredWaveRune = HotbarSkills.AssignedSkills.Any(s => s.Power == SNOPower.Monk_WaveOfLight && s.RuneIndex == 3);

            bool hasEmpoweredWaveRune = HotbarSkills.AssignedSkills.Any(s => s.Power == SNOPower.Monk_WaveOfLight && s.RuneIndex == 3);
            var minWoLSpirit = hasEmpoweredWaveRune ? 40 : 75;
            // Wave of light
            if (!UseOOCBuff && !IsCurrentlyAvoiding && !PlayerStatus.IsIncapacitated && Hotbar.Contains(SNOPower.Monk_WaveOfLight) && GilesUseTimer(SNOPower.Monk_WaveOfLight) &&
                (TargetUtil.AnyMobsInRange(16f, Settings.Combat.Monk.MinWoLTrashCount) || TargetUtil.IsEliteTargetInRange(20f)) &&
                (PlayerStatus.PrimaryResource >= minWoLSpirit && !IsWaitingForSpecial || PlayerStatus.PrimaryResource > MinEnergyReserve) &&
                // optional check for SW stacks
                (Settings.Combat.Monk.SWBeforeWoL && (CheckAbilityAndBuff(SNOPower.Monk_SweepingWind) && GetBuffStacks(SNOPower.Monk_SweepingWind) == 3) || !Settings.Combat.Monk.SWBeforeWoL) && 
                Monk_HasMantraAbilityAndBuff())
            {
                var bestClusterPoint = TargetUtil.GetBestClusterPoint(15f, 15f);
                Monk_TickSweepingWindSpam();
                return new TrinityPower(SNOPower.Monk_WaveOfLight, 16f, bestClusterPoint, -1, CurrentTarget.ACDGuid, 0, 1, WAIT_FOR_ANIM);
            }

            //SkillDict.Add("CycloneStrike", SNOPower.Monk_CycloneStrike);
            //RuneDict.Add("EyeOfTheStorm", 3);
            //RuneDict.Add("Implosion", 1);
            //RuneDict.Add("Sunburst", 0);
            //RuneDict.Add("WallOfWind", 4);
            //RuneDict.Add("SoothingBreeze", 2);

            bool hasCycloneStikeEyeOfTheStorm = HotbarSkills.AssignedSkills.Any(s => s.Power == SNOPower.Monk_CycloneStrike && s.RuneIndex == 3);
            bool hasCycloneStikeImposion = HotbarSkills.AssignedSkills.Any(s => s.Power == SNOPower.Monk_CycloneStrike && s.RuneIndex == 1);

            var cycloneStrikeRange = hasCycloneStikeImposion ? 34f : 24f;
            var cycloneStrikeSpirit = hasCycloneStikeEyeOfTheStorm ? 30 : 50;

            // Cyclone Strike
            if (!UseOOCBuff && !IsCurrentlyAvoiding && !PlayerStatus.IsIncapacitated && Hotbar.Contains(SNOPower.Monk_CycloneStrike) && GilesUseTimer(SNOPower.Monk_CycloneStrike) &&
                (
                 TargetUtil.AnyElitesInRange(cycloneStrikeRange, 2) || 
                 TargetUtil.AnyMobsInRange(cycloneStrikeRange, Settings.Combat.Monk.MinCycloneTrashCount) || 
                 (CurrentTarget.RadiusDistance >= 15f && CurrentTarget.RadiusDistance <= cycloneStrikeRange) // pull the current target into attack range
                ) &&
                ((PlayerStatus.PrimaryResource >= cycloneStrikeSpirit && !PlayerStatus.WaitingForReserveEnergy) || PlayerStatus.PrimaryResource >= MinEnergyReserve) &&
                 PowerManager.CanCast(SNOPower.Monk_CycloneStrike))
            {
                Monk_TickSweepingWindSpam();
                return new TrinityPower(SNOPower.Monk_CycloneStrike, 0f, vNullLocation, CurrentWorldDynamicId, -1, 2, 2, WAIT_FOR_ANIM);
            }

            // For tempest rush re-use
            if (!UseOOCBuff && PlayerStatus.PrimaryResource >= 15 &&
                DateTime.Now.Subtract(dictAbilityLastUse[SNOPower.Monk_TempestRush]).TotalMilliseconds <= 150 &&
                ((Settings.Combat.Monk.TROption != TempestRushOption.MovementOnly) &&
                !(Settings.Combat.Monk.TROption == TempestRushOption.TrashOnly && TargetUtil.AnyElitesInRange(40f))))
            {
                GenerateMonkZigZag();
                MaintainTempestRush = true;
                string trUse = "Continuing Tempest Rush for Combat";
                Monk_TempestRushStatus(trUse);
                return new TrinityPower(SNOPower.Monk_TempestRush, 23f, vSideToSideTarget, CurrentWorldDynamicId, -1, 0, 0, NO_WAIT_ANIM);
            }

            // Tempest rush at elites or groups of mobs
            if (!UseOOCBuff && !IsCurrentlyAvoiding && !PlayerStatus.IsIncapacitated && !PlayerStatus.IsRooted && Hotbar.Contains(SNOPower.Monk_TempestRush) &&
                ((PlayerStatus.PrimaryResource >= Settings.Combat.Monk.TR_MinSpirit && !PlayerStatus.WaitingForReserveEnergy) || PlayerStatus.PrimaryResource >= MinEnergyReserve) &&
                (Settings.Combat.Monk.TROption == TempestRushOption.Always ||
                Settings.Combat.Monk.TROption == TempestRushOption.CombatOnly ||
                (Settings.Combat.Monk.TROption == TempestRushOption.ElitesGroupsOnly && (TargetUtil.AnyElitesInRange(25) || TargetUtil.AnyMobsInRange(25, 2))) ||
                (Settings.Combat.Monk.TROption == TempestRushOption.TrashOnly && !TargetUtil.AnyElitesInRange(90f) && TargetUtil.AnyMobsInRange(40f))))
            {
                GenerateMonkZigZag();
                MaintainTempestRush = true;
                string trUse = "Starting Tempest Rush for Combat";
                Monk_TempestRushStatus(trUse);
                return new TrinityPower(SNOPower.Monk_TempestRush, 23f, vSideToSideTarget, CurrentWorldDynamicId, -1, 0, 0, NO_WAIT_ANIM);
            }

            // 4 Mantra spam for the 4 second buff
            if (!UseOOCBuff && !Settings.Combat.Monk.DisableMantraSpam && (!Hotbar.Contains(SNOPower.Monk_TempestRush) || PlayerStatus.PrimaryResource >= 98 ||
                (PlayerStatus.CurrentHealthPct <= 0.55 && PlayerStatus.PrimaryResource >= 75) || CurrentTarget.IsBoss) &&
                    (PlayerStatus.PrimaryResource >= 135 ||
                    (GetHasBuff(SNOPower.Monk_SweepingWind) && (PlayerStatus.PrimaryResource >= 60 &&
                    PlayerStatus.PrimaryResource >= 110 || (PlayerStatus.PrimaryResource >= 100 && PlayerStatus.CurrentHealthPct >= 0.6) ||
                    (PlayerStatus.PrimaryResource >= 50 && PlayerStatus.CurrentHealthPct >= 0.6)) &&
                // Checking we have no expensive finishers
                    !Hotbar.Contains(SNOPower.Monk_SevenSidedStrike) && !Hotbar.Contains(SNOPower.Monk_LashingTailKick) &&
                    !Hotbar.Contains(SNOPower.Monk_WaveOfLight) && !Hotbar.Contains(SNOPower.Monk_CycloneStrike) &&
                    !Hotbar.Contains(SNOPower.Monk_ExplodingPalm))) &&
                (ElitesWithinRange[RANGE_15] >= 1 || AnythingWithinRange[RANGE_15] >= 3 ||
                (AnythingWithinRange[RANGE_15] >= 1 && (Settings.Combat.Monk.HasInnaSet && PlayerStatus.PrimaryResource >= 70))))
            {
                if (Hotbar.Contains(SNOPower.Monk_MantraOfEvasion) && GilesUseTimer(SNOPower.Monk_MantraOfEvasion))
                {
                    return new TrinityPower(SNOPower.Monk_MantraOfEvasion, 0f, vNullLocation, CurrentWorldDynamicId, -1, 1, 1, WAIT_FOR_ANIM);
                }
                if (Hotbar.Contains(SNOPower.Monk_MantraOfConviction) && GilesUseTimer(SNOPower.Monk_MantraOfConviction))
                {
                    return new TrinityPower(SNOPower.Monk_MantraOfConviction, 0f, vNullLocation, CurrentWorldDynamicId, -1, 1, 1, WAIT_FOR_ANIM);
                }
                if (Hotbar.Contains(SNOPower.Monk_MantraOfRetribution) && GilesUseTimer(SNOPower.Monk_MantraOfRetribution))
                {
                    return new TrinityPower(SNOPower.Monk_MantraOfRetribution, 0f, vNullLocation, CurrentWorldDynamicId, -1, 1, 1, WAIT_FOR_ANIM);
                }
                if (Hotbar.Contains(SNOPower.Monk_MantraOfHealing) && GilesUseTimer(SNOPower.Monk_MantraOfHealing))
                {
                    return new TrinityPower(SNOPower.Monk_MantraOfHealing, 0f, vNullLocation, CurrentWorldDynamicId, -1, 1, 1, WAIT_FOR_ANIM);
                }
            }
            // Lashing Tail Kick
            if (!UseOOCBuff && !IsCurrentlyAvoiding && Hotbar.Contains(SNOPower.Monk_LashingTailKick) && !PlayerStatus.IsIncapacitated &&
                (ElitesWithinRange[RANGE_15] > 0 || AnythingWithinRange[RANGE_15] > 4 || (CurrentTarget.IsBossOrEliteRareUnique && CurrentTarget.RadiusDistance <= 10f)) &&
                // Either doesn't have sweeping wind, or does but the buff is already up
                (!Hotbar.Contains(SNOPower.Monk_SweepingWind) || (Hotbar.Contains(SNOPower.Monk_SweepingWind) && GetHasBuff(SNOPower.Monk_SweepingWind))) &&
                GilesUseTimer(SNOPower.Monk_LashingTailKick) &&
                ((PlayerStatus.PrimaryResource >= 65 && !PlayerStatus.WaitingForReserveEnergy) || PlayerStatus.PrimaryResource >= MinEnergyReserve))
            {
                Monk_TickSweepingWindSpam();
                return new TrinityPower(SNOPower.Monk_LashingTailKick, 10f, vNullLocation, -1, CurrentTarget.ACDGuid, 1, 1, WAIT_FOR_ANIM);
            }

            //skillDict.Add("DashingStrike", SNOPower.Monk_DashingStrike);
            //runeDict.Add("WayOfTheFallingStar", 1);
            //runeDict.Add("FlyingSideKick", 4);
            //runeDict.Add("Quicksilver", 3);
            //runeDict.Add("SoaringSkull", 0);
            //runeDict.Add("BlindingSpeed", 2);

            bool hasWayOfTheFallingStar = HotbarSkills.AssignedSkills.Any(s => s.Power == SNOPower.Monk_DashingStrike && s.RuneIndex == 1);
            bool hasQuicksilver = HotbarSkills.AssignedSkills.Any(s => s.Power == SNOPower.Monk_DashingStrike && s.RuneIndex == 3);
            var dashingStrikeSpirit = hasQuicksilver ? 10 : 25;

            // Dashing Strike, quick move to target out of range
            if (!UseOOCBuff && !IsCurrentlyAvoiding && !PlayerStatus.IsIncapacitated &&
                CurrentTarget.CentreDistance >= 16f &&
                Hotbar.Contains(SNOPower.Monk_DashingStrike) && ((PlayerStatus.PrimaryResource >= dashingStrikeSpirit && !PlayerStatus.WaitingForReserveEnergy) || PlayerStatus.PrimaryResource >= MinEnergyReserve))
            {
                Monk_TickSweepingWindSpam();
                return new TrinityPower(SNOPower.Monk_DashingStrike, Monk_MaxDashingStrikeRange, vNullLocation, -1, CurrentTarget.ACDGuid, 2, 2, WAIT_FOR_ANIM);
            }


            // Dashing strike + way of the fallen Star
            if (!UseOOCBuff && !IsCurrentlyAvoiding && !PlayerStatus.IsIncapacitated && Hotbar.Contains(SNOPower.Monk_DashingStrike) &&
                (TimeSinceUse(SNOPower.Monk_DashingStrike) >= 2800) && hasWayOfTheFallingStar &&
                ((PlayerStatus.PrimaryResource >= 25 && !PlayerStatus.WaitingForReserveEnergy) || PlayerStatus.PrimaryResource >= MinEnergyReserve))
            {
                Monk_TickSweepingWindSpam();
                return new TrinityPower(SNOPower.Monk_DashingStrike, Monk_MaxDashingStrikeRange, vNullLocation, -1, CurrentTarget.ACDGuid, 2, 2, WAIT_FOR_ANIM);
            }

            // Fists of thunder as the primary, repeatable attack
            if (!UseOOCBuff && !IsCurrentlyAvoiding && Hotbar.Contains(SNOPower.Monk_FistsofThunder)
                && (DateTime.Now.Subtract(OtherThanDeadlyReach).TotalMilliseconds < 2700 && DateTime.Now.Subtract(ForeSightFirstHit).TotalMilliseconds < 29000 || !Hotbar.Contains(SNOPower.Monk_DeadlyReach) || CurrentTarget.RadiusDistance > 12f ||
                AnythingWithinRange[RANGE_50] < 5 && ElitesWithinRange[RANGE_50] <= 0 && !WantToSwap))
            {
                if (DateTime.Now.Subtract(OtherThanDeadlyReach).TotalMilliseconds < 2700)
                    OtherThanDeadlyReach = DateTime.Now;
                Monk_TickSweepingWindSpam();
                return new TrinityPower(SNOPower.Monk_FistsofThunder, 30f, vNullLocation, -1, CurrentTarget.ACDGuid, 0, 1, NO_WAIT_ANIM);
            }
            // Crippling wave
            if (!UseOOCBuff && !IsCurrentlyAvoiding && Hotbar.Contains(SNOPower.Monk_CripplingWave)
                && (DateTime.Now.Subtract(OtherThanDeadlyReach).TotalMilliseconds < 2700 && DateTime.Now.Subtract(ForeSightFirstHit).TotalMilliseconds < 29000 || !Hotbar.Contains(SNOPower.Monk_DeadlyReach)
                || AnythingWithinRange[RANGE_50] < 5 && ElitesWithinRange[RANGE_50] <= 0 && !WantToSwap))
            {
                OtherThanDeadlyReach = DateTime.Now;
                Monk_TickSweepingWindSpam();
                return new TrinityPower(SNOPower.Monk_CripplingWave, 14f, vNullLocation, -1, CurrentTarget.ACDGuid, 0, 1, NO_WAIT_ANIM);
            }
            // Way of hundred fists
            if (!UseOOCBuff && !IsCurrentlyAvoiding && Hotbar.Contains(SNOPower.Monk_WayOfTheHundredFists)
                && (DateTime.Now.Subtract(OtherThanDeadlyReach).TotalMilliseconds < 2700 && DateTime.Now.Subtract(ForeSightFirstHit).TotalMilliseconds < 29000 || !Hotbar.Contains(SNOPower.Monk_DeadlyReach)
                || AnythingWithinRange[RANGE_50] < 5 && ElitesWithinRange[RANGE_50] <= 0 && !WantToSwap))
            {
                OtherThanDeadlyReach = DateTime.Now;
                Monk_TickSweepingWindSpam(); 
                return new TrinityPower(SNOPower.Monk_WayOfTheHundredFists, 14f, vNullLocation, -1, CurrentTarget.ACDGuid, 0, 1, NO_WAIT_ANIM);
            }
            // Deadly reach
            if (!UseOOCBuff && !IsCurrentlyAvoiding && Hotbar.Contains(SNOPower.Monk_DeadlyReach))
            {
                if (DateTime.Now.Subtract(ForeSightFirstHit).TotalMilliseconds > 29000)
                {
                    ForeSightFirstHit = DateTime.Now;
                }
                else if (DateTime.Now.Subtract(ForeSight2).TotalMilliseconds > 400 && DateTime.Now.Subtract(ForeSightFirstHit).TotalMilliseconds > 1400)
                {
                    OtherThanDeadlyReach = DateTime.Now;
                }
                if (DateTime.Now.Subtract(ForeSight2).TotalMilliseconds > 2800)
                {
                    ForeSight2 = DateTime.Now;
                }
                Monk_TickSweepingWindSpam();
                return new TrinityPower(SNOPower.Monk_DeadlyReach, 16f, vNullLocation, -1, CurrentTarget.ACDGuid, 0, 1, NO_WAIT_ANIM);
            }
            // Default attacks
            if (!UseOOCBuff && !IsCurrentlyAvoiding)
            {
                Monk_TickSweepingWindSpam();
                return new TrinityPower(GetDefaultWeaponPower(), GetDefaultWeaponDistance(), vNullLocation, -1, CurrentTarget.ACDGuid, 0, 0, WAIT_FOR_ANIM);
            }
            return new TrinityPower(SNOPower.None, -1, vNullLocation, -1, -1, 0, 0, WAIT_FOR_ANIM);
        }

        private static void Monk_TickSweepingWindSpam()
        {
            if (GetHasBuff(SNOPower.Monk_SweepingWind) && DateTime.Now.Subtract(SweepWindSpam).TotalMilliseconds < 5500)
                SweepWindSpam = DateTime.Now;
        }

        private static void GenerateMonkZigZag()
        {
            float fExtraDistance = CurrentTarget.RadiusDistance <= 20f ? 15f : 20f;
            vSideToSideTarget = TargetUtil.GetZigZagTarget(CurrentTarget.Position, fExtraDistance);
            double direction = MathUtil.FindDirectionRadian(PlayerStatus.CurrentPosition, vSideToSideTarget);
            vSideToSideTarget = MathEx.GetPointAt(PlayerStatus.CurrentPosition, 40f, (float)direction);
            DbHelper.Log(TrinityLogLevel.Debug, LogCategory.Behavior, "Generated ZigZag {0} distance {1:0}", vSideToSideTarget, vSideToSideTarget.Distance2D(PlayerStatus.CurrentPosition));
            iACDGUIDLastWhirlwind = CurrentTarget.ACDGuid;
            lastChangedZigZag = DateTime.Now;
        }

        private static TrinityPower GetMonkDestroyPower()
        {
            if (Monk_TempestRushReady())
                return new TrinityPower(SNOPower.Monk_TempestRush, 40f, vNullLocation, -1, -1, 0, 0, WAIT_FOR_ANIM);
            if (Hotbar.Contains(SNOPower.Monk_DashingStrike))
                return new TrinityPower(SNOPower.Monk_DashingStrike, Monk_MaxDashingStrikeRange, vNullLocation, -1, -1, 0, 0, WAIT_FOR_ANIM);
            if (Hotbar.Contains(SNOPower.Monk_FistsofThunder))
                return new TrinityPower(SNOPower.Monk_FistsofThunder, 15f, vNullLocation, -1, -1, 0, 0, WAIT_FOR_ANIM);
            if (Hotbar.Contains(SNOPower.Monk_DeadlyReach))
                return new TrinityPower(SNOPower.Monk_DeadlyReach, 10f, vNullLocation, -1, -1, 0, 0, WAIT_FOR_ANIM);
            if (Hotbar.Contains(SNOPower.Monk_CripplingWave))
                return new TrinityPower(SNOPower.Monk_CripplingWave, 10f, vNullLocation, -1, -1, 0, 0, WAIT_FOR_ANIM);
            if (Hotbar.Contains(SNOPower.Monk_WayOfTheHundredFists))
                return new TrinityPower(SNOPower.Monk_WayOfTheHundredFists, 10f, vNullLocation, -1, -1, 0, 0, WAIT_FOR_ANIM);
            return new TrinityPower(SNOPower.Weapon_Melee_Instant, 10f, vNullLocation, -1, -1, 0, 0, WAIT_FOR_ANIM);
        }
        /// <summary>
        /// Returns true if we have a mantra and it's up, or if we don't have a Mantra at all
        /// </summary>
        /// <returns></returns>
        private static bool Monk_HasMantraAbilityAndBuff()
        {
            return
                (CheckAbilityAndBuff(SNOPower.Monk_MantraOfConviction) ||
                CheckAbilityAndBuff(SNOPower.Monk_MantraOfEvasion) ||
                CheckAbilityAndBuff(SNOPower.Monk_MantraOfHealing) ||
                CheckAbilityAndBuff(SNOPower.Monk_MantraOfRetribution) ||
                DoesNotHaveMonkMantraAbility());
        }
        private static bool DoesNotHaveMonkMantraAbility()
        {
            return
                (!Hotbar.Contains(SNOPower.Monk_MantraOfConviction) &&
                !Hotbar.Contains(SNOPower.Monk_MantraOfEvasion) &&
                !Hotbar.Contains(SNOPower.Monk_MantraOfHealing) &&
                !Hotbar.Contains(SNOPower.Monk_MantraOfRetribution));
        }

        internal static bool Monk_TempestRushReady()
        {
            bool isReady = false;

            if (PlayerStatus.ActorClass != ActorClass.Monk)
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
            if (TimeSinceUse(SNOPower.Monk_TempestRush) < 150 && currentSpirit > 10f)
                return true;

            // Minimum 25 Spirit to start Tempest Rush
            if (PowerManager.CanCast(SNOPower.Monk_TempestRush) && currentSpirit > Settings.Combat.Monk.TR_MinSpirit && TimeSinceUse(SNOPower.Monk_TempestRush) > 550)
                return true;

            return isReady;
        }
        private static void Monk_MaintainTempestRush()
        {
            if (!Monk_TempestRushReady())
                return;

            if (PlayerStatus.IsInTown || Zeta.CommonBot.Logic.BrainBehavior.IsVendoring)
                return;

            if (TownRun.IsTryingToTownPortal())
                return;

            if (TimeSinceUse(SNOPower.Monk_TempestRush) > 150)
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
                    case GObjectType.Globe:
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

            if (Settings.Combat.Monk.TROption != TempestRushOption.MovementOnly && GilesUseTimer(SNOPower.Monk_TempestRush) && shouldMaintain)
            {
                Vector3 target = LastTempestRushLocation;

                string locationSource = "LastLocation";

                //if (CurrentTarget != null && GilesNavHelper.CanRayCast(PlayerStatus.CurrentPosition, CurrentTarget.Position))
                //{
                //    locationSource = "Current Target Position";
                //    target = CurrentTarget.Position;
                //}

                if (target.Distance2D(ZetaDia.Me.Position) <= 1f)
                {
                    // rrrix edit: we can't maintain here
                    return;

                    //locationSource = "ZigZag";
                    //target = FindZigZagTargetLocation(target, 23f);
                }

                if (target == Vector3.Zero)
                    return;

                float DestinationDistance = target.Distance2D(ZetaDia.Me.Position);

                //target = MathEx.CalculatePointFrom(target, PlayerStatus.CurrentPosition, aimPointDistance);
                target = TargetUtil.FindTempestRushTarget();

                if (DestinationDistance > 10f && NavHelper.CanRayCast(ZetaDia.Me.Position, target))
                {
                    Monk_TempestRushStatus(String.Format("Using Tempest Rush to maintain channeling, source={0}, V3={1} dist={2:0}", locationSource, target, DestinationDistance));

                    var usePowerResult = ZetaDia.Me.UsePower(SNOPower.Monk_TempestRush, target, CurrentWorldDynamicId, -1);
                    if (usePowerResult)
                        dictAbilityLastUse[SNOPower.Monk_TempestRush] = DateTime.Now;
                }
            }
        }
        private static void Monk_TempestRushStatus(string trUse)
        {

            DbHelper.Log(TrinityLogLevel.Debug, LogCategory.Behavior, "{0}, xyz={4} spirit={1:0} cd={2} lastUse={3:0}",
                trUse,
                Trinity.PlayerStatus.PrimaryResource, PowerManager.CanCast(SNOPower.Monk_TempestRush),
                TimeSinceUse(SNOPower.Monk_TempestRush), vSideToSideTarget);
        }


    }
}
