using System;
using System.Linq;
using Trinity.Combat.Abilities;
using Trinity.Config.Combat;
using Trinity.Technicals;
using Zeta.Bot;
using Zeta.Bot.Profile.Common;
using Zeta.Common;
using Zeta.Common.Plugins;
using Zeta.Game;
using Zeta.Game.Internals.Actors;
using Logger = Trinity.Technicals.Logger;

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

            //// Monk - Primary

            bool hasThunderClap = HotbarSkills.AssignedSkills.Any(s => s.Power == SNOPower.Monk_FistsofThunder && s.RuneIndex == 0);
            //bool hasLightningFlash = HotbarSkills.AssignedSkills.Any(s => s.Power == SNOPower.Monk_FistsofThunder && s.RuneIndex == 4);
            //bool hasStaticCharge = HotbarSkills.AssignedSkills.Any(s => s.Power == SNOPower.Monk_FistsofThunder && s.RuneIndex == 3);
            //bool hasQuickening = HotbarSkills.AssignedSkills.Any(s => s.Power == SNOPower.Monk_FistsofThunder && s.RuneIndex == 3);
            //bool hasBoundingLight = HotbarSkills.AssignedSkills.Any(s => s.Power == SNOPower.Monk_FistsofThunder && s.RuneIndex == 1);

            //bool hasPiercingTrident = HotbarSkills.AssignedSkills.Any(s => s.Power == SNOPower.Monk_DeadlyReach && s.RuneIndex == 1);
            //bool hasKeenEye = HotbarSkills.AssignedSkills.Any(s => s.Power == SNOPower.Monk_DeadlyReach && s.RuneIndex == 4);
            //bool hasScatteredBlows = HotbarSkills.AssignedSkills.Any(s => s.Power == SNOPower.Monk_DeadlyReach && s.RuneIndex == 2);
            //bool hasStrikeFromBeyond = HotbarSkills.AssignedSkills.Any(s => s.Power == SNOPower.Monk_DeadlyReach && s.RuneIndex == 3);
            bool hasForesight = HotbarSkills.AssignedSkills.Any(s => s.Power == SNOPower.Monk_DeadlyReach && s.RuneIndex == 0);

            //bool hasMangle = HotbarSkills.AssignedSkills.Any(s => s.Power == SNOPower.Monk_CripplingWave && s.RuneIndex == 0);
            //bool hasConcussion = HotbarSkills.AssignedSkills.Any(s => s.Power == SNOPower.Monk_CripplingWave && s.RuneIndex == 2);
            //bool hasRisingTide = HotbarSkills.AssignedSkills.Any(s => s.Power == SNOPower.Monk_CripplingWave && s.RuneIndex == 3);
            //bool hasTsunami = HotbarSkills.AssignedSkills.Any(s => s.Power == SNOPower.Monk_CripplingWave && s.RuneIndex == 1);
            //bool hasBreakingWave = HotbarSkills.AssignedSkills.Any(s => s.Power == SNOPower.Monk_CripplingWave && s.RuneIndex == 4);

            //bool hasHandsOfLightning = HotbarSkills.AssignedSkills.Any(s => s.Power == SNOPower.Monk_WayOfTheHundredFists && s.RuneIndex == 1);
            bool hasBlazingFists = HotbarSkills.AssignedSkills.Any(s => s.Power == SNOPower.Monk_WayOfTheHundredFists && s.RuneIndex == 2);
            bool hasFistsOfFury = HotbarSkills.AssignedSkills.Any(s => s.Power == SNOPower.Monk_WayOfTheHundredFists && s.RuneIndex == 0);
            //bool hasSpiritedSalvo = HotbarSkills.AssignedSkills.Any(s => s.Power == SNOPower.Monk_WayOfTheHundredFists && s.RuneIndex == 3);
            //bool hasWindforceFlurry = HotbarSkills.AssignedSkills.Any(s => s.Power == SNOPower.Monk_WayOfTheHundredFists && s.RuneIndex == 4);

            // Breath of Heaven Rune
            bool hasInfusedWithLight = HotbarSkills.AssignedSkills.Any(s => s.Power == SNOPower.Monk_BreathOfHeaven && s.RuneIndex == 3);

            // Serenity if health is low
            if ((Player.CurrentHealthPct <= 0.50 || (Player.IsIncapacitated && Player.CurrentHealthPct <= 0.90)) && CombatBase.CanCast(SNOPower.Monk_Serenity))
            {
                return new TrinityPower(SNOPower.Monk_Serenity, 0f, Vector3.Zero, CurrentWorldDynamicId, -1, 1, 1, WAIT_FOR_ANIM);
            }

            //Use Mantra of Healing active if health is low for shield.
            if ((IsCurrentlyAvoiding || Player.CurrentHealthPct <= 0.80) && !Player.IsIncapacitated 
                && CombatBase.CanCast(SNOPower.X1_Monk_MantraOfHealing_v2)
                && !GetHasBuff(SNOPower.X1_Monk_MantraOfHealing_v2)
                && !Player.WaitingForReserveEnergy)
            {
                return new TrinityPower(SNOPower.X1_Monk_MantraOfHealing_v2, 0f, Vector3.Zero, CurrentWorldDynamicId, -1, 1, 1, WAIT_FOR_ANIM);
            }

            // 4 Mantras for the initial buff (slow-use)
            if (CombatBase.CanCast(SNOPower.X1_Monk_MantraOfEvasion_v2) && !GetHasBuff(SNOPower.X1_Monk_MantraOfRetribution_v2_Passive) &&
                Player.PrimaryResource >= 50)
            {
                return new TrinityPower(SNOPower.X1_Monk_MantraOfEvasion_v2, 0f, Vector3.Zero, CurrentWorldDynamicId, -1, 0, 1, WAIT_FOR_ANIM);
            }
            if (CombatBase.CanCast(SNOPower.X1_Monk_MantraOfConviction_v2) && !GetHasBuff(SNOPower.X1_Monk_MantraOfConviction_v2_Passive) &&
                (Player.PrimaryResource >= 50))
            {
                return new TrinityPower(SNOPower.X1_Monk_MantraOfConviction_v2, 0f, Vector3.Zero, CurrentWorldDynamicId, -1, 0, 1, WAIT_FOR_ANIM);
            }
            if (CombatBase.CanCast(SNOPower.X1_Monk_MantraOfHealing_v2) && !GetHasBuff(SNOPower.X1_Monk_MantraOfHealing_v2_Passive) &&
                Player.PrimaryResource >= 50)
            {
                return new TrinityPower(SNOPower.X1_Monk_MantraOfHealing_v2, 0f, Vector3.Zero, CurrentWorldDynamicId, -1, 0, 1, WAIT_FOR_ANIM);
            }
            if (CombatBase.CanCast(SNOPower.X1_Monk_MantraOfRetribution_v2) && !GetHasBuff(SNOPower.X1_Monk_MantraOfRetribution_v2_Passive) &&
                Player.PrimaryResource >= 50)
            {
                return new TrinityPower(SNOPower.X1_Monk_MantraOfRetribution_v2, 0f, Vector3.Zero, CurrentWorldDynamicId, -1, 0, 1, WAIT_FOR_ANIM);
            }

            // Mystic ally
            if (CombatBase.CanCast(SNOPower.X1_Monk_MysticAlly_v2) && TargetUtil.EliteOrTrashInRange(30f))
            {
                return new TrinityPower(SNOPower.X1_Monk_MysticAlly_v2, 0f, Vector3.Zero, CurrentWorldDynamicId, -1, 2, 2, WAIT_FOR_ANIM);
            }

            // InnerSanctuary 
            if (!UseOOCBuff && TargetUtil.EliteOrTrashInRange(16f) && CombatBase.CanCast(SNOPower.X1_Monk_InnerSanctuary))
            {
                return new TrinityPower(SNOPower.X1_Monk_InnerSanctuary, 0f, Vector3.Zero, CurrentWorldDynamicId, -1, 1, 1, WAIT_FOR_ANIM);
            }

            // Blinding Flash
            if (!UseOOCBuff && Player.PrimaryResource >= 20 && CombatBase.CanCast(SNOPower.Monk_BlindingFlash) &&
                (
                    TargetUtil.AnyElitesInRange(15, 1) ||
                    Player.CurrentHealthPct <= 0.4 ||
                    (TargetUtil.AnyMobsInRange(15, 3)) ||
                    (CurrentTarget.IsBossOrEliteRareUnique && CurrentTarget.RadiusDistance <= 15f) ||
                // as pre-sweeping wind buff
                    (TargetUtil.AnyMobsInRange(15, 1) && CombatBase.CanCast(SNOPower.Monk_SweepingWind) && !GetHasBuff(SNOPower.Monk_SweepingWind) && Settings.Combat.Monk.HasInnaSet)
                ) &&
                // Check if either we don't have sweeping winds, or we do and it's ready to cast in a moment
                (CheckAbilityAndBuff(SNOPower.Monk_SweepingWind) ||
                 (!GetHasBuff(SNOPower.Monk_SweepingWind) &&
                 (CombatBase.CanCast(SNOPower.Monk_SweepingWind, CombatBase.CanCastFlags.NoTimer) &&
                 Settings.Combat.Monk.HasInnaSet ? Player.PrimaryResource >= 15 : Player.PrimaryResource >= 85)) ||
                 Player.CurrentHealthPct <= 0.25))
            {
                return new TrinityPower(SNOPower.Monk_BlindingFlash, 0f, Vector3.Zero, CurrentWorldDynamicId, -1, 0, 1, WAIT_FOR_ANIM);
            }
            // Blinding Flash as a DEFENSE
            if (!UseOOCBuff && Player.PrimaryResource >= 10 && CombatBase.CanCast(SNOPower.Monk_BlindingFlash) &&
                Player.CurrentHealthPct <= 0.75 && TargetUtil.AnyMobsInRange(15, 1))
            {
                return new TrinityPower(SNOPower.Monk_BlindingFlash, 0f, Vector3.Zero, CurrentWorldDynamicId, -1, 0, 1, WAIT_FOR_ANIM);
            }

            // Breath of Heaven when needing healing or the buff
            if (!UseOOCBuff && (Player.CurrentHealthPct <= 0.6 || !GetHasBuff(SNOPower.Monk_BreathOfHeaven)) && CombatBase.CanCast(SNOPower.Monk_BreathOfHeaven) &&
                (Player.PrimaryResource >= 35 || (!CombatBase.CanCast(SNOPower.Monk_Serenity) && Player.PrimaryResource >= 25)))
            {
                return new TrinityPower(SNOPower.Monk_BreathOfHeaven, 0f, Vector3.Zero, CurrentWorldDynamicId, -1, 1, 1, WAIT_FOR_ANIM);
            }

            // Breath of Heaven for spirit - Infused with Light
            if (!UseOOCBuff && !Player.IsIncapacitated && CombatBase.CanCast(SNOPower.Monk_BreathOfHeaven) && !GetHasBuff(SNOPower.Monk_BreathOfHeaven) && hasInfusedWithLight &&
                (TargetUtil.AnyMobsInRange(3, 20) || TargetUtil.IsEliteTargetInRange(20)) && Player.PrimaryResourcePct < 0.75)
            {
                return new TrinityPower(SNOPower.Monk_BreathOfHeaven, 0f, Vector3.Zero, CurrentWorldDynamicId, -1, 1, 1, WAIT_FOR_ANIM);
            }


            // Seven Sided Strike
            if (!UseOOCBuff && !IsCurrentlyAvoiding && !Player.IsIncapacitated &&
                (TargetUtil.AnyElitesInRange(15, 1) || (CurrentTarget.IsBossOrEliteRareUnique && CurrentTarget.RadiusDistance <= 15f) || Player.CurrentHealthPct <= 0.55) &&
                CombatBase.CanCast(SNOPower.Monk_SevenSidedStrike) && ((Player.PrimaryResource >= 50 && !Player.WaitingForReserveEnergy) || Player.PrimaryResource >= MinEnergyReserve))
            {
                Monk_TickSweepingWindSpam();
                return new TrinityPower(SNOPower.Monk_SevenSidedStrike, 16f, CurrentTarget.Position, CurrentWorldDynamicId, -1, 2, 3, WAIT_FOR_ANIM);
            }

            // WayOfTheHundredFists: apply fists of fury DoT if we have Infused with Light buff + WotHF:FoF
            if (!UseOOCBuff && hasInfusedWithLight && hasFistsOfFury && GetHasBuff(SNOPower.Monk_BreathOfHeaven) && !CurrentTarget.HasDotDPS)
            {
                Monk_TickSweepingWindSpam();
                return new TrinityPower(SNOPower.Monk_WayOfTheHundredFists, 14f, Vector3.Zero, -1, CurrentTarget.ACDGuid, 0, 1, NO_WAIT_ANIM);
            }


            // Sweeping winds spam
            if ((Player.PrimaryResource >= 75 || (Settings.Combat.Monk.HasInnaSet && Player.PrimaryResource >= 5)) &&
                CombatBase.CanCast(SNOPower.Monk_SweepingWind, CombatBase.CanCastFlags.NoTimer) && GetHasBuff(SNOPower.Monk_SweepingWind) &&
                DateTime.UtcNow.Subtract(SweepWindSpam).TotalMilliseconds >= 4000 && DateTime.UtcNow.Subtract(SweepWindSpam).TotalMilliseconds <= 5400)
            {
                SweepWindSpam = DateTime.UtcNow;
                return new TrinityPower(SNOPower.Monk_SweepingWind, 0f, Vector3.Zero, CurrentWorldDynamicId, -1, 0, 0, NO_WAIT_ANIM);
            }

            bool hasTranscendance = ZetaDia.CPlayer.PassiveSkills.Any(s => s == SNOPower.Monk_Passive_Transcendence);
            float minSweepingWindSpirit = Settings.Combat.Monk.HasInnaSet ? 5f : 75f;

            // Sweeping wind
            if (!UseOOCBuff && CombatBase.CanCast(SNOPower.Monk_SweepingWind) && !GetHasBuff(SNOPower.Monk_SweepingWind) &&
                ((TargetUtil.AnyElitesInRange(25, 1) || TargetUtil.AnyMobsInRange(20, 1) || Settings.Combat.Monk.HasInnaSet ||
                (CurrentTarget.IsBossOrEliteRareUnique && CurrentTarget.RadiusDistance <= 25f)) &&
                // Check our mantras, if we have them, are up first
                Monk_HasMantraAbilityAndBuff() &&
                // Check if either we don't have blinding flash, or we do and it's been cast in the last 8000ms
                (TimeSinceUse(SNOPower.Monk_BlindingFlash) <= 8000 || CheckAbilityAndBuff(SNOPower.Monk_BlindingFlash) ||
                TargetUtil.AnyElitesInRange(25, 1) && TimeSinceUse(SNOPower.Monk_BlindingFlash) <= 12500)) &&
                Player.PrimaryResource >= minSweepingWindSpirit)
            {
                SweepWindSpam = DateTime.UtcNow;
                return new TrinityPower(SNOPower.Monk_SweepingWind, 0f, Vector3.Zero, CurrentWorldDynamicId, -1, 0, 0, NO_WAIT_ANIM);
            }

            // Sweeping Wind for Transcendance Health Regen
            if (CombatBase.CanCast(SNOPower.Monk_SweepingWind, CombatBase.CanCastFlags.NoTimer) &&
                Player.PrimaryResource >= minSweepingWindSpirit &&
                hasTranscendance && Settings.Combat.Monk.SpamSweepingWindOnLowHP &&
                Player.CurrentHealthPct <= V.F("Monk.SweepingWind.SpamOnLowHealthPct") &&
                TimeSinceUse(SNOPower.Monk_SweepingWind) > 500)
            {
                SweepWindSpam = DateTime.UtcNow;
                return new TrinityPower(SNOPower.Monk_SweepingWind, 0f, Vector3.Zero, CurrentWorldDynamicId, -1, 0, 0, NO_WAIT_ANIM);
            }

            //skillDict.Add("BreathOfHeaven", SNOPower.Monk_BreathOfHeaven);
            //runeDict.Add("CircleOfScorn", 0);
            //runeDict.Add("CircleOfLife", 1);
            //runeDict.Add("BlazingWrath", 2);
            //runeDict.Add("InfusedWithLight", 3);
            //runeDict.Add("PenitentFlame", 4);

            // Exploding Palm
            if (!UseOOCBuff && !IsCurrentlyAvoiding && !Player.IsIncapacitated &&
                //(TargetUtil.AnyElitesInRange(25, 0+1)  || TargetUtil.AnyMobsInRange(15, 3)  || (CurrentTarget.IsBossOrEliteRareUnique && CurrentTarget.RadiusDistance <= 14f)) &&
                CombatBase.CanCast(SNOPower.Monk_ExplodingPalm, CombatBase.CanCastFlags.NoTimer) &&
                !SpellTracker.IsUnitTracked(CurrentTarget, SNOPower.Monk_ExplodingPalm) &&
                (Player.PrimaryResource >= (40 + MinEnergyReserve)))
            {
                return new TrinityPower(SNOPower.Monk_ExplodingPalm, 14f, Vector3.Zero, -1, CurrentTarget.ACDGuid, 1, 1, WAIT_FOR_ANIM);
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
            if (!UseOOCBuff && !IsCurrentlyAvoiding && !Player.IsIncapacitated && CombatBase.CanCast(SNOPower.Monk_WaveOfLight) &&
                (TargetUtil.AnyMobsInRange(16f, Settings.Combat.Monk.MinWoLTrashCount) || TargetUtil.IsEliteTargetInRange(20f)) &&
                (Player.PrimaryResource >= minWoLSpirit && !IsWaitingForSpecial || Player.PrimaryResource > MinEnergyReserve) &&
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
            if (!UseOOCBuff && !IsCurrentlyAvoiding && !Player.IsIncapacitated && CombatBase.CanCast(SNOPower.Monk_CycloneStrike) &&
                (
                 TargetUtil.AnyElitesInRange(cycloneStrikeRange, 1) ||
                 TargetUtil.AnyMobsInRange(cycloneStrikeRange, Settings.Combat.Monk.MinCycloneTrashCount) ||
                 (CurrentTarget.RadiusDistance >= 15f && CurrentTarget.RadiusDistance <= cycloneStrikeRange) // pull the current target into attack range
                ) &&
                (Player.PrimaryResource >= (cycloneStrikeSpirit + MinEnergyReserve)))
            {
                Monk_TickSweepingWindSpam();
                return new TrinityPower(SNOPower.Monk_CycloneStrike, 0f, Vector3.Zero, CurrentWorldDynamicId, -1, 2, 2, WAIT_FOR_ANIM);
            }

            // For tempest rush re-use
            if (!UseOOCBuff && Player.PrimaryResource >= 15 && CombatBase.CanCast(SNOPower.Monk_TempestRush) &&
                TimeSinceUse(SNOPower.Monk_TempestRush) <= 150 &&
                ((Settings.Combat.Monk.TROption != TempestRushOption.MovementOnly) &&
                !(Settings.Combat.Monk.TROption == TempestRushOption.TrashOnly && TargetUtil.AnyElitesInRange(40f))))
            {
                GenerateMonkZigZag();
                MaintainTempestRush = true;
                string trUse = "Continuing Tempest Rush for Combat";
                Monk_TempestRushStatus(trUse);
                return new TrinityPower(SNOPower.Monk_TempestRush, 23f, CombatBase.ZigZagPosition, CurrentWorldDynamicId, -1, 0, 0, NO_WAIT_ANIM);
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
                MaintainTempestRush = true;
                string trUse = "Starting Tempest Rush for Combat";
                Monk_TempestRushStatus(trUse);
                return new TrinityPower(SNOPower.Monk_TempestRush, 23f, CombatBase.ZigZagPosition, CurrentWorldDynamicId, -1, 0, 0, NO_WAIT_ANIM);
            }

            // 4 Mantra spam for the 4 second buff
            if (CombatBase.CanCast(SNOPower.X1_Monk_MantraOfEvasion_v2) && !GetHasBuff(SNOPower.X1_Monk_MantraOfRetribution_v2_Passive) &&
                Player.PrimaryResource >= 150 && TargetUtil.EliteOrTrashInRange(30f))
            {
                return new TrinityPower(SNOPower.X1_Monk_MantraOfEvasion_v2, 0f, Vector3.Zero, CurrentWorldDynamicId, -1, 0, 1, WAIT_FOR_ANIM);
            }

            if (CombatBase.CanCast(SNOPower.X1_Monk_MantraOfConviction_v2) && !GetHasBuff(SNOPower.X1_Monk_MantraOfConviction_v2_Passive) &&
                (Player.PrimaryResource >= 150) && TargetUtil.EliteOrTrashInRange(30f))
            {
                return new TrinityPower(SNOPower.X1_Monk_MantraOfConviction_v2, 0f, Vector3.Zero, CurrentWorldDynamicId, -1, 0, 1, WAIT_FOR_ANIM);
            }

            if (CombatBase.CanCast(SNOPower.X1_Monk_MantraOfHealing_v2) && !GetHasBuff(SNOPower.X1_Monk_MantraOfHealing_v2_Passive) &&
                (Player.PrimaryResource >= 150) && TargetUtil.EliteOrTrashInRange(30f))
            {
                return new TrinityPower(SNOPower.X1_Monk_MantraOfHealing_v2, 0f, Vector3.Zero, CurrentWorldDynamicId, -1, 0, 1, WAIT_FOR_ANIM);
            }

            if (CombatBase.CanCast(SNOPower.X1_Monk_MantraOfRetribution_v2) && !GetHasBuff(SNOPower.X1_Monk_MantraOfRetribution_v2_Passive) &&
                (Player.PrimaryResource >= 150) && TargetUtil.EliteOrTrashInRange(30f))
            {
                return new TrinityPower(SNOPower.X1_Monk_MantraOfRetribution_v2, 0f, Vector3.Zero, CurrentWorldDynamicId, -1, 0, 1, WAIT_FOR_ANIM);
            }

            // Lashing Tail Kick
            if (!UseOOCBuff && !IsCurrentlyAvoiding && CombatBase.CanCast(SNOPower.Monk_LashingTailKick) && !Player.IsIncapacitated &&
                // Either doesn't have sweeping wind, or does but the buff is already up
                (!Hotbar.Contains(SNOPower.Monk_SweepingWind) || (Hotbar.Contains(SNOPower.Monk_SweepingWind) && GetHasBuff(SNOPower.Monk_SweepingWind))) &&
                ((Player.PrimaryResource >= 65 && !Player.WaitingForReserveEnergy) || Player.PrimaryResource >= MinEnergyReserve))
            {
                Monk_TickSweepingWindSpam();
                return new TrinityPower(SNOPower.Monk_LashingTailKick, 10f, Vector3.Zero, -1, CurrentTarget.ACDGuid, 1, 1, WAIT_FOR_ANIM);
            }

            bool hasWayOfTheFallingStar = HotbarSkills.AssignedSkills.Any(s => s.Power == SNOPower.Monk_DashingStrike && s.RuneIndex == 1);
            bool hasQuicksilver = HotbarSkills.AssignedSkills.Any(s => s.Power == SNOPower.Monk_DashingStrike && s.RuneIndex == 3);
            var dashingStrikeSpirit = hasQuicksilver ? 10 : 25;

            // Dashing Strike, quick move to target out of range
            if (!UseOOCBuff && !IsCurrentlyAvoiding && !Player.IsIncapacitated &&
                CurrentTarget.CentreDistance >= 16f &&
                CombatBase.CanCast(SNOPower.Monk_DashingStrike) && ((Player.PrimaryResource >= dashingStrikeSpirit && !Player.WaitingForReserveEnergy) || Player.PrimaryResource >= MinEnergyReserve))
            {
                Monk_TickSweepingWindSpam();
                return new TrinityPower(SNOPower.Monk_DashingStrike, Monk_MaxDashingStrikeRange, Vector3.Zero, -1, CurrentTarget.ACDGuid, 2, 2, WAIT_FOR_ANIM);
            }


            // Dashing strike + way of the fallen Star
            if (!UseOOCBuff && !IsCurrentlyAvoiding && !Player.IsIncapacitated && CombatBase.CanCast(SNOPower.Monk_DashingStrike, CombatBase.CanCastFlags.NoTimer) &&
                (TimeSinceUse(SNOPower.Monk_DashingStrike) >= 2800) && hasWayOfTheFallingStar &&
                ((Player.PrimaryResource >= 25 && !Player.WaitingForReserveEnergy) || Player.PrimaryResource >= MinEnergyReserve))
            {
                Monk_TickSweepingWindSpam();
                return new TrinityPower(SNOPower.Monk_DashingStrike, Monk_MaxDashingStrikeRange, Vector3.Zero, -1, CurrentTarget.ACDGuid, 2, 2, WAIT_FOR_ANIM);
            }


            /*
             * Dual/Trigen Monk section
             * 
             * Cycle through Deadly Reach, Way of the Hundred Fists, and Fists of Thunder every 3 seconds to keep 8% passive buff up if we have Combination Strike
             *  - or - 
             * Keep Foresight and Blazing Fists buffs up every 30/5 seconds
             */
            bool hasCombinationStrike = ZetaDia.CPlayer.PassiveSkills.Any(s => s == SNOPower.Monk_Passive_CombinationStrike);
            bool isDualOrTriGen = HotbarSkills.AssignedSkills.Count(s =>
                s.Power == SNOPower.Monk_DeadlyReach ||
                s.Power == SNOPower.Monk_WayOfTheHundredFists ||
                s.Power == SNOPower.Monk_FistsofThunder ||
                s.Power == SNOPower.Monk_CripplingWave) >= 2 && hasCombinationStrike;

            // interval in milliseconds for Generators
            int drInterval = 0;
            if (hasCombinationStrike)
                drInterval = 2500;
            else if (hasForesight)
                drInterval = 29000;

            int wothfInterval = 0;
            if (hasCombinationStrike)
                wothfInterval = 2500;
            else if (hasBlazingFists)
                wothfInterval = 4500;

            int cwInterval = 0;
            if (hasCombinationStrike)
                cwInterval = 2500;

            // Fists of Thunder:Thunder Clap - Fly to Target
            if (!UseOOCBuff && !IsCurrentlyAvoiding && CombatBase.CanCast(SNOPower.Monk_FistsofThunder) && hasThunderClap && CurrentTarget.CentreDistance > 16f)
            {
                Monk_TickSweepingWindSpam();
                return new TrinityPower(SNOPower.Monk_FistsofThunder, 30f, Vector3.Zero, -1, CurrentTarget.ACDGuid, 0, 3, WAIT_FOR_ANIM);
            }

            // Deadly Reach: Foresight, every 27 seconds or 2.7 seconds with combo strike
            if (!UseOOCBuff && !IsCurrentlyAvoiding && CombatBase.CanCast(SNOPower.Monk_DeadlyReach) && (isDualOrTriGen || hasForesight) &&
                (SpellHistory.TimeSinceUse(SNOPower.Monk_DeadlyReach) > TimeSpan.FromMilliseconds(drInterval) ||
                (SpellHistory.SpellUseCountInTime(SNOPower.Monk_DeadlyReach, TimeSpan.FromMilliseconds(27000)) < 3) && hasForesight))
            {
                Monk_TickSweepingWindSpam();
                return new TrinityPower(SNOPower.Monk_DeadlyReach, 16f, Vector3.Zero, -1, CurrentTarget.ACDGuid, 0, 3, WAIT_FOR_ANIM);
            }

            // Way of the Hundred Fists: Blazing Fists, every 4-5ish seconds or if we don't have 3 stacks of the buff or or 2.7 seconds with combo strike
            if (!UseOOCBuff && !IsCurrentlyAvoiding && CombatBase.CanCast(SNOPower.Monk_WayOfTheHundredFists) && (isDualOrTriGen || hasBlazingFists) &&
                (GetBuffStacks(SNOPower.Monk_WayOfTheHundredFists) < 3 ||
                SpellHistory.TimeSinceUse(SNOPower.Monk_WayOfTheHundredFists) > TimeSpan.FromMilliseconds(wothfInterval)))
            {
                Monk_TickSweepingWindSpam();
                return new TrinityPower(SNOPower.Monk_WayOfTheHundredFists, 16f, Vector3.Zero, -1, CurrentTarget.ACDGuid, 0, 3, WAIT_FOR_ANIM);
            }

            // Crippling Wave
            if (!UseOOCBuff && !IsCurrentlyAvoiding && CombatBase.CanCast(SNOPower.Monk_CripplingWave) &&
                SpellHistory.TimeSinceUse(SNOPower.Monk_CripplingWave) > TimeSpan.FromMilliseconds(cwInterval))
            {
                Monk_TickSweepingWindSpam();
                return new TrinityPower(SNOPower.Monk_CripplingWave, 20f, Vector3.Zero, -1, CurrentTarget.ACDGuid, 0, 3, WAIT_FOR_ANIM);
            }

            // Fists of Thunder
            if (!UseOOCBuff && !IsCurrentlyAvoiding && CombatBase.CanCast(SNOPower.Monk_FistsofThunder))
            {
                Monk_TickSweepingWindSpam();
                return new TrinityPower(SNOPower.Monk_FistsofThunder, 30f, Vector3.Zero, -1, CurrentTarget.ACDGuid, 0, 3, WAIT_FOR_ANIM);
            }

            // Deadly Reach normal
            if (!UseOOCBuff && !IsCurrentlyAvoiding && CombatBase.CanCast(SNOPower.Monk_DeadlyReach))
            {
                Monk_TickSweepingWindSpam();
                return new TrinityPower(SNOPower.Monk_DeadlyReach, 16f, Vector3.Zero, -1, CurrentTarget.ACDGuid, 0, 3, WAIT_FOR_ANIM);
            }

            // Way of the Hundred Fists normal
            if (!UseOOCBuff && !IsCurrentlyAvoiding && CombatBase.CanCast(SNOPower.Monk_WayOfTheHundredFists))
            {
                Monk_TickSweepingWindSpam();
                return new TrinityPower(SNOPower.Monk_WayOfTheHundredFists, 16f, Vector3.Zero, -1, CurrentTarget.ACDGuid, 0, 3, WAIT_FOR_ANIM);
            }

            // Crippling Wave Normal
            if (!UseOOCBuff && !IsCurrentlyAvoiding && CombatBase.CanCast(SNOPower.Monk_CripplingWave))
            {
                Monk_TickSweepingWindSpam();
                return new TrinityPower(SNOPower.Monk_CripplingWave, 30f, Vector3.Zero, -1, CurrentTarget.ACDGuid, 0, 3, WAIT_FOR_ANIM);
            }


            //// Fists of thunder as the primary, repeatable attack
            //if (!UseOOCBuff && !IsCurrentlyAvoiding && CombatBase.CanCast(SNOPower.Monk_FistsofThunder)
            //    && (DateTime.UtcNow.Subtract(OtherThanDeadlyReach).TotalMilliseconds < 2700 && DateTime.UtcNow.Subtract(ForeSightFirstHit).TotalMilliseconds < 29000 || !Hotbar.Contains(SNOPower.Monk_DeadlyReach) || CurrentTarget.RadiusDistance > 12f ||
            //    !TargetUtil.AnyMobsInRange(50, 5) && !TargetUtil.AnyElitesInRange(50) && !WantToSwap))
            //{
            //    if (DateTime.UtcNow.Subtract(OtherThanDeadlyReach).TotalMilliseconds < 2700)
            //        OtherThanDeadlyReach = DateTime.UtcNow;
            //    Monk_TickSweepingWindSpam();
            //    return new TrinityPower(SNOPower.Monk_FistsofThunder, 30f, Vector3.Zero, -1, CurrentTarget.ACDGuid, 0, 1, NO_WAIT_ANIM);
            //}
            //// Crippling wave
            //if (!UseOOCBuff && !IsCurrentlyAvoiding && CombatBase.CanCast(SNOPower.Monk_CripplingWave)
            //    && (DateTime.UtcNow.Subtract(OtherThanDeadlyReach).TotalMilliseconds < 2700 && DateTime.UtcNow.Subtract(ForeSightFirstHit).TotalMilliseconds < 29000 || !Hotbar.Contains(SNOPower.Monk_DeadlyReach)
            //    || !TargetUtil.AnyMobsInRange(50, 5) && !TargetUtil.AnyElitesInRange(50) && !WantToSwap))
            //{
            //    OtherThanDeadlyReach = DateTime.UtcNow;
            //    Monk_TickSweepingWindSpam();
            //    return new TrinityPower(SNOPower.Monk_CripplingWave, 14f, Vector3.Zero, -1, CurrentTarget.ACDGuid, 0, 1, NO_WAIT_ANIM);
            //}
            //// Way of hundred fists
            //if (!UseOOCBuff && !IsCurrentlyAvoiding && CombatBase.CanCast(SNOPower.Monk_WayOfTheHundredFists)
            //    && (DateTime.UtcNow.Subtract(OtherThanDeadlyReach).TotalMilliseconds < 2700 && DateTime.UtcNow.Subtract(ForeSightFirstHit).TotalMilliseconds < 29000 || !Hotbar.Contains(SNOPower.Monk_DeadlyReach)
            //    || !TargetUtil.AnyMobsInRange(50, 5) && !TargetUtil.AnyElitesInRange(50) && !WantToSwap))
            //{
            //    OtherThanDeadlyReach = DateTime.UtcNow;
            //    Monk_TickSweepingWindSpam();
            //    return new TrinityPower(SNOPower.Monk_WayOfTheHundredFists, 14f, Vector3.Zero, -1, CurrentTarget.ACDGuid, 0, 1, NO_WAIT_ANIM);
            //}
            //// Deadly reach
            //if (!UseOOCBuff && !IsCurrentlyAvoiding && CombatBase.CanCast(SNOPower.Monk_DeadlyReach))
            //{
            //    if (DateTime.UtcNow.Subtract(ForeSightFirstHit).TotalMilliseconds > 29000)
            //    {
            //        ForeSightFirstHit = DateTime.UtcNow;
            //    }
            //    else if (DateTime.UtcNow.Subtract(ForeSight2).TotalMilliseconds > 400 && DateTime.UtcNow.Subtract(ForeSightFirstHit).TotalMilliseconds > 1400)
            //    {
            //        OtherThanDeadlyReach = DateTime.UtcNow;
            //    }
            //    if (DateTime.UtcNow.Subtract(ForeSight2).TotalMilliseconds > 2800)
            //    {
            //        ForeSight2 = DateTime.UtcNow;
            //    }
            //    Monk_TickSweepingWindSpam();
            //    return new TrinityPower(SNOPower.Monk_DeadlyReach, 16f, Vector3.Zero, -1, CurrentTarget.ACDGuid, 0, 1, NO_WAIT_ANIM);
            //}

            // Default attacks
            return CombatBase.DefaultPower;
        }

        internal static void Monk_TickSweepingWindSpam()
        {
            if (GetHasBuff(SNOPower.Monk_SweepingWind) && DateTime.UtcNow.Subtract(SweepWindSpam).TotalMilliseconds < 5500)
                SweepWindSpam = DateTime.UtcNow;
        }

        private static void GenerateMonkZigZag()
        {
            float fExtraDistance = CurrentTarget.RadiusDistance <= 20f ? 15f : 20f;
            CombatBase.ZigZagPosition = TargetUtil.GetZigZagTarget(CurrentTarget.Position, fExtraDistance);
            double direction = MathUtil.FindDirectionRadian(Player.Position, CombatBase.ZigZagPosition);
            CombatBase.ZigZagPosition = MathEx.GetPointAt(Player.Position, 40f, (float)direction);
            Logger.Log(TrinityLogLevel.Debug, LogCategory.Behavior, "Generated ZigZag {0} distance {1:0}", CombatBase.ZigZagPosition, CombatBase.ZigZagPosition.Distance2D(Player.Position));
            LastZigZagUnitAcdGuid = CurrentTarget.ACDGuid;
            LastChangedZigZag = DateTime.UtcNow;
        }

        private static TrinityPower GetMonkDestroyPower()
        {
            if (Monk_TempestRushReady())
                return new TrinityPower(SNOPower.Monk_TempestRush, 40f, Vector3.Zero, -1, -1, 0, 0, WAIT_FOR_ANIM);
            if (CombatBase.CanCast(SNOPower.Monk_DashingStrike))
                return new TrinityPower(SNOPower.Monk_DashingStrike, Monk_MaxDashingStrikeRange, Vector3.Zero, -1, -1, 0, 0, WAIT_FOR_ANIM);
            if (CombatBase.CanCast(SNOPower.Monk_FistsofThunder))
                return new TrinityPower(SNOPower.Monk_FistsofThunder, 15f, Vector3.Zero, -1, -1, 0, 0, WAIT_FOR_ANIM);
            if (CombatBase.CanCast(SNOPower.Monk_DeadlyReach))
                return new TrinityPower(SNOPower.Monk_DeadlyReach, 10f, Vector3.Zero, -1, -1, 0, 0, WAIT_FOR_ANIM);
            if (CombatBase.CanCast(SNOPower.Monk_CripplingWave))
                return new TrinityPower(SNOPower.Monk_CripplingWave, 10f, Vector3.Zero, -1, -1, 0, 0, WAIT_FOR_ANIM);
            if (CombatBase.CanCast(SNOPower.Monk_WayOfTheHundredFists))
                return new TrinityPower(SNOPower.Monk_WayOfTheHundredFists, 10f, Vector3.Zero, -1, -1, 0, 0, WAIT_FOR_ANIM);
            return CombatBase.DefaultPower;
        }
        /// <summary>
        /// Returns true if we have a mantra and it's up, or if we don't have a Mantra at all
        /// </summary>
        /// <returns></returns>
        private static bool Monk_HasMantraAbilityAndBuff()
        {
            return
                (CheckAbilityAndBuff(SNOPower.X1_Monk_MantraOfConviction_v2) ||
                CheckAbilityAndBuff(SNOPower.X1_Monk_MantraOfEvasion_v2) ||
                CheckAbilityAndBuff(SNOPower.X1_Monk_MantraOfHealing_v2) ||
                CheckAbilityAndBuff(SNOPower.X1_Monk_MantraOfRetribution_v2) ||
                DoesNotHaveMonkMantraAbility());
        }
        private static bool DoesNotHaveMonkMantraAbility()
        {
            return
                (!Hotbar.Contains(SNOPower.X1_Monk_MantraOfConviction_v2) &&
                !Hotbar.Contains(SNOPower.X1_Monk_MantraOfEvasion_v2) &&
                !Hotbar.Contains(SNOPower.X1_Monk_MantraOfHealing_v2) &&
                !Hotbar.Contains(SNOPower.X1_Monk_MantraOfRetribution_v2));
        }

        internal static bool Monk_TempestRushReady()
        {
            bool isReady = false;

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

            if (Player.IsInTown || Zeta.Bot.Logic.BrainBehavior.IsVendoring)
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

                string locationSource = "LastLocation";

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

                    var usePowerResult = ZetaDia.Me.UsePower(SNOPower.Monk_TempestRush, target, CurrentWorldDynamicId, -1);
                    if (usePowerResult)
                    {
                        CacheData.AbilityLastUsedCache[SNOPower.Monk_TempestRush] = DateTime.UtcNow;
                    }
                }
            }
        }
        private static void Monk_TempestRushStatus(string trUse)
        {

            Logger.Log(TrinityLogLevel.Debug, LogCategory.Behavior, "{0}, xyz={4} spirit={1:0} cd={2} lastUse={3:0}",
                trUse,
                Trinity.Player.PrimaryResource, PowerManager.CanCast(SNOPower.Monk_TempestRush),
                TimeSinceUse(SNOPower.Monk_TempestRush), CombatBase.ZigZagPosition);
        }


    }
}
