using System;
using System.Collections.Generic;
using System.Linq;
using Trinity.Config.Combat;
using Trinity.DbProvider;
using Trinity.Objects;
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
        public static bool CurrentlyUseDashingStrike
        {
            get
            {
                return Player.ActorClass == ActorClass.Monk && Hotbar.Contains(SNOPower.X1_Monk_DashingStrike) &&
                    CombatBase.TimeSincePowerUse(SNOPower.X1_Monk_DashingStrike) < 350;
            }
        }

        private const float MaxDashingStrikeRange = 55f;
        private static DateTime _lastTargetChange = DateTime.MinValue;

        internal static Vector3 LastTempestRushLocation
        {
            get { return SpellHistory.GetSpellLastTargetPosition(SNOPower.Monk_TempestRush); }
        }
        internal static DateTime LastSweepingWindRefresh
        {
            get { return CacheData.AbilityLastUsed[SNOPower.Monk_SweepingWind]; }
        }

        static bool _hasInnaSet;
        static bool _hasSwk;
        static float _minSweepingWindSpirit;

        public static MonkSetting MonkSettings
        {
            get { return Trinity.Settings.Combat.Monk; }
        }
        private static readonly HashSet<SNOPower> ExplodingPalmDebuff = new HashSet<SNOPower>
        {
            SNOPower.Monk_ExplodingPalm
        };
        private static readonly Func<TargetArea, bool> MinimumSunwukoCriteria = area =>
            area.DebuffedCount(ExplodingPalmDebuff) >= area.UnitCount * 0.5 &&
            (area.EliteCount > 0 || area.BossCount > 0 || area.UnitCount >= Trinity.Settings.Combat.Misc.TrashPackSize);

        public static TrinityPower GetPower()
        {
            RunOngoingPowers();

            _hasInnaSet = Sets.Innas.IsThirdBonusActive;
            _hasSwk = Sets.MonkeyKingsGarb.IsSecondBonusActive;
            _minSweepingWindSpirit = _hasInnaSet ? 5f : 75f;

            // Destructible objects
            if (UseDestructiblePower)
            {
                // Sweeping Winds Refresh
                if (Player.PrimaryResource >= _minSweepingWindSpirit &&
                   CanCast(SNOPower.Monk_SweepingWind, CanCastFlags.NoTimer) &&
                   !GetHasBuff(SNOPower.Monk_SweepingWind))
                {
                    return new TrinityPower(SNOPower.Monk_SweepingWind);
                }

                return GetMonkDestroyPower();
            }

            // Dashing strike avoidance
            if (IsCurrentlyAvoiding && CanCast(SNOPower.X1_Monk_DashingStrike, CanCastFlags.NoTimer) && GridMap.GetBestMoveNode().Distance >= 15f)
            {
                return new TrinityPower(SNOPower.X1_Monk_DashingStrike, 0f, GridMap.GetBestMoveNode().Position);
            }

            // Epiphany: spirit regen, dash to targets
            if (!UseOOCBuff && !IsCurrentlyAvoiding && CanCast(SNOPower.X1_Monk_Epiphany, CanCastFlags.NoTimer) && (Settings.Combat.Monk.EpiphanyOffCD ||
                (TargetUtil.EliteOrTrashInRange(20f) || TargetUtil.AnyMobsInRange(20f, 5)) &&
                (Player.PrimaryResourcePct < 0.50 || Runes.Monk.DesertShroud.IsActive || Runes.Monk.SoothingMist.IsActive || Player.CurrentHealthPct < 0.50)))
            {
                return new TrinityPower(SNOPower.X1_Monk_Epiphany);
            }

            // Serenity off CD
            if (CanCast(SNOPower.Monk_Serenity, CanCastFlags.NoTimer) && MonkSettings.SerenityOffCD)
            {
                return new TrinityPower(SNOPower.Monk_Serenity);
            }

            // Serenity if health is low
            if (CanCast(SNOPower.Monk_Serenity) && (Player.CurrentHealthPct <= 0.50 || (Player.IsIncapacitated && Player.CurrentHealthPct <= 0.90)))
            {
                return new TrinityPower(SNOPower.Monk_Serenity);
            }

            // Mystic ally
            if (CanCast(SNOPower.X1_Monk_MysticAlly_v2) && TargetUtil.EliteOrTrashInRange(30f) && !Runes.Monk.AirAlly.IsActive)
            {
                return new TrinityPower(SNOPower.X1_Monk_MysticAlly_v2, 0f, Vector3.Zero, Trinity.CurrentWorldDynamicId, -1, 2, 2);
            }

            // Air Ally
            if (CanCast(SNOPower.X1_Monk_MysticAlly_v2) && Runes.Monk.AirAlly.IsActive && Player.PrimaryResource < 150)
            {
                return new TrinityPower(SNOPower.X1_Monk_MysticAlly_v2);
            }

            if (CanCast(SNOPower.X1_Monk_InnerSanctuary))
            {
                // InnerSanctuary ForbiddenPalace
                if (!UseOOCBuff && !IsCurrentlyAvoiding && !Player.IsIncapacitated &&
                    Runes.Monk.ForbiddenPalace.IsActive &&
                    CurrentTarget.IsTrashPackOrBossEliteRareUnique && CurrentTarget.RadiusDistance <= 15f &&
                    (!Skills.Monk.ExplodingPalm.IsActive || CurrentTarget.HasDebuff(SNOPower.Monk_ExplodingPalm) &&
                    (!Skills.Monk.WaveOfLight.IsActive || CanCast(SNOPower.Monk_WaveOfLight))))
                {
                    return new TrinityPower(SNOPower.X1_Monk_InnerSanctuary);
                }
                // InnerSanctuary Intervene
                else if (!UseOOCBuff && !IsCurrentlyAvoiding && !Player.IsIncapacitated &&
                    Runes.Monk.Intervene.IsActive && (!Skills.Monk.DashingStrike.IsActive || !CanCastDashingStrike) &&
                    (CurrentTarget.IsBossOrEliteRareUnique || CurrentTarget.NearbyUnits >= 3) && CurrentTarget.Distance >= 10f)
                {
                    return new TrinityPower(SNOPower.X1_Monk_InnerSanctuary, 50f, CurrentTarget.ACDGuid);
                }
                // InnerSanctuary TempleOfProtection
                else if (Runes.Monk.TempleOfProtection.IsActive && (Player.IsIncapacitated || Player.IsRooted))
                {
                    return new TrinityPower(SNOPower.X1_Monk_InnerSanctuary);
                }

                // InnerSanctuary low health
                if (CanCast(SNOPower.X1_Monk_InnerSanctuary) && Player.CurrentHealthPct <= 0.5)
                {
                    return new TrinityPower(SNOPower.X1_Monk_InnerSanctuary);
                }

                // InnerSanctuary Surrounded
                if (TargetUtil.AnyMobsInRange(16f, 5))
                {
                    return new TrinityPower(SNOPower.X1_Monk_InnerSanctuary);
                }
            }

            // Blinding Flash
            if (!UseOOCBuff && Player.PrimaryResource >= 20 && CanCast(SNOPower.Monk_BlindingFlash) &&
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
                 Player.CurrentHealthPct <= 0.25))
            {
                return new TrinityPower(SNOPower.Monk_BlindingFlash, 0f, Vector3.Zero, Trinity.CurrentWorldDynamicId, -1, 0, 1);
            }

            // Blinding Flash as a DEFENSE
            if (!UseOOCBuff && Player.PrimaryResource >= 10 && CanCast(SNOPower.Monk_BlindingFlash) &&
                Player.CurrentHealthPct <= 0.75 && TargetUtil.AnyMobsInRange(15, 1))
            {
                return new TrinityPower(SNOPower.Monk_BlindingFlash, 0f, Vector3.Zero, Trinity.CurrentWorldDynamicId, -1, 0, 1);
            }

            // Breath of Heaven Section

            // Breath of Heaven when needing healing or the buff
            if (!UseOOCBuff && (Player.CurrentHealthPct <= 0.6 || !GetHasBuff(SNOPower.Monk_BreathOfHeaven)) && CanCast(SNOPower.Monk_BreathOfHeaven) &&
                (Player.PrimaryResource >= 35 || (!CanCast(SNOPower.Monk_Serenity) && Player.PrimaryResource >= 25)))
            {
                return new TrinityPower(SNOPower.Monk_BreathOfHeaven, 0f, Vector3.Zero, Trinity.CurrentWorldDynamicId, -1, 1, 1);
            }

            // Breath of Heaven for spirit - Infused with Light
            if (!UseOOCBuff && !Player.IsIncapacitated && CanCast(SNOPower.Monk_BreathOfHeaven, CanCastFlags.NoTimer) &&
                !GetHasBuff(SNOPower.Monk_BreathOfHeaven) && Runes.Monk.InfusedWithLight.IsActive &&
                (TargetUtil.AnyMobsInRange(20) || TargetUtil.IsEliteTargetInRange(20) || Player.PrimaryResource < 75))
            {
                return new TrinityPower(SNOPower.Monk_BreathOfHeaven);
            }

            // Seven Sided Strike
            if (!UseOOCBuff && !IsCurrentlyAvoiding && !Player.IsIncapacitated &&
                (TargetUtil.AnyElitesInRange(15, 1) || Player.CurrentHealthPct <= 0.55 || Legendary.Madstone.IsEquipped) &&
                CanCast(SNOPower.Monk_SevenSidedStrike, CanCastFlags.NoTimer) &&
                ((Player.PrimaryResource >= 50 && !IsWaitingForSpecial) || Player.PrimaryResource >= MinEnergyReserve))
            {
                // RefreshSweepingWind(true);
                return new TrinityPower(SNOPower.Monk_SevenSidedStrike, 16f, CurrentTarget.ClusterPosition(14f));
            }

            // Sweeping Winds Refresh(wtf the last logic)
            if (Player.PrimaryResource >= _minSweepingWindSpirit &&
               CanCast(SNOPower.Monk_SweepingWind, CanCastFlags.NoTimer) &&
               ((!GetHasBuff(SNOPower.Monk_SweepingWind) && (_hasInnaSet || TargetUtil.AnyMobsInRange(18f))) ||
               CombatBase.IsTaegukBuffWillExpire))
            {
                return new TrinityPower(SNOPower.Monk_SweepingWind);
            }

            // Sweeping Wind for Transcendance Health Regen
            if (CanCast(SNOPower.Monk_SweepingWind, CanCastFlags.NoTimer) &&
                Passives.Monk.Transcendence.IsActive && Settings.Combat.Monk.SpamSweepingWindOnLowHP &&
                Player.CurrentHealthPct <= V.F("Monk.SweepingWind.SpamOnLowHealthPct") &&
                Trinity.TimeSinceUse(SNOPower.Monk_SweepingWind) > 500)
            {
                return new TrinityPower(SNOPower.Monk_SweepingWind);
            }

            var cycloneStrikeRange = Runes.Monk.Implosion.IsActive ? 34f : 24f;
            var cycloneStrikeSpirit = Runes.Monk.EyeOfTheStorm.IsActive ? 30 : 50;

            // Cyclone Strike
            if (!UseOOCBuff && !IsCurrentlyAvoiding && !Player.IsIncapacitated && CanCast(SNOPower.Monk_CycloneStrike) &&
                (
                 TargetUtil.AnyElitesInRange(cycloneStrikeRange, 1) ||
                 TargetUtil.AnyMobsInRange(cycloneStrikeRange, Settings.Combat.Monk.MinCycloneTrashCount) ||
                 (CurrentTarget.RadiusDistance >= 15f && CurrentTarget.RadiusDistance <= cycloneStrikeRange) // pull the current target into attack range
                ) && TimeSincePowerUse(SNOPower.Monk_CycloneStrike) > 3000 &&

                // Cyclone if more than 25% of monsters within cyclone range are at least 10f away
                TargetUtil.IsPercentUnitsWithinBand(10f, cycloneStrikeRange, 0.25) &&

                (Player.PrimaryResource >= (cycloneStrikeSpirit + MinEnergyReserve)))
            {
                // RefreshSweepingWind(true);
                return new TrinityPower(SNOPower.Monk_CycloneStrike, 0f, Vector3.Zero, Trinity.CurrentWorldDynamicId, -1, 2, 2);
            }

            var wolRange = Legendary.TzoKrinsGaze.IsEquipped ? 35f : 10f;
            if (!UseOOCBuff && !Player.IsIncapacitated && CanCast(SNOPower.Monk_WaveOfLight))
            {
                if (MinimumSunwukoCriteria(Enemies.AtPlayerNearby) && Skills.Monk.WaveOfLight.CanCast(CanCastFlags.NoTimer))
                    Skills.Monk.WaveOfLight.Cast(TargetUtil.GetBestClusterPoint(_range: wolRange));
                else if (MinimumSunwukoCriteria(Enemies.AtPlayerNearby) && Skills.Monk.LashingTailKick.CanCast(CanCastFlags.NoTimer))
                    Skills.Monk.LashingTailKick.Cast(TargetUtil.GetBestClusterPoint(8f));
            }

            // Wave of light
            if (!UseOOCBuff && !IsCurrentlyAvoiding && !Player.IsIncapacitated && CanCast(SNOPower.Monk_WaveOfLight) &&
                TargetUtil.AnyMobsInRange(25, Settings.Combat.Monk.MinWoLTrashCount) &&
                (!Settings.Combat.Monk.SWBeforeWoL || (CheckAbilityAndBuff(SNOPower.Monk_SweepingWind) && GetBuffStacks(SNOPower.Monk_SweepingWind) == 3)) &&
                Skills.Monk.ExplodingPalm.IsActive)
            {
                var _target = TargetUtil.BestExploadingPalmDebuffedTarget(wolRange);
                if (_target.IsTrashPackOrBossEliteRareUnique &&
                    TargetUtil.MobsWithDebuff(_target.Position, SNOPower.Monk_ExplodingPalm, 10f) >= 0.5 * TargetUtil.NumMobsInRangeOfPosition(_target.Position, 10f) &&
                    _target.RadiusDistance <= wolRange && _target.HasDebuff(SNOPower.Monk_ExplodingPalm))
                {
                    return new TrinityPower(SNOPower.Monk_WaveOfLight, 0f, _target.ACDGuid);
                }

            }

            // Lashing Tail Kick
            if (!UseOOCBuff && !IsCurrentlyAvoiding && !Player.IsIncapacitated && CanCast(SNOPower.Monk_LashingTailKick) &&
                TargetUtil.AnyMobsInRange(25, 2) &&
                CurrentTarget.IsTrashPackOrBossEliteRareUnique && Skills.Monk.ExplodingPalm.IsActive)
            {
                var _target = TargetUtil.BestExploadingPalmDebuffedTarget(10f);
                if (_target.IsTrashPackOrBossEliteRareUnique &&
                    TargetUtil.MobsWithDebuff(_target.Position, SNOPower.Monk_ExplodingPalm, 10f) >= 0.5 * TargetUtil.NumMobsInRangeOfPosition(_target.Position, 10f) &&
                    _target.RadiusDistance <= 10f && _target.HasDebuff(SNOPower.Monk_ExplodingPalm))
                {
                    return new TrinityPower(SNOPower.Monk_LashingTailKick, 0f, _target.ACDGuid);
                }
            }

            // Make a mega-splosion
            if (ShouldSpreadExplodingPalm())
            {
                ChangeTarget();
            }

            // Dashing Strike
            if (CanCastDashingStrike)
            {
                if (Legendary.Jawbreaker.IsEquipped)
                {
                    return JawBreakerDashingStrike();
                }

                if (CurrentTarget.IsTrashPackOrBossEliteRareUnique && CurrentTarget.Distance >= MaxDashingStrikeRange && CurrentTarget.IsInLineOfSight)
                {
                    return new TrinityPower(SNOPower.X1_Monk_DashingStrike, 0f, CurrentTarget.Position);
                }

                if (CurrentTarget.IsTrashPackOrBossEliteRareUnique &&
                    (CurrentTarget.Distance >= 10f ||
                    !NavHelper.CanRayCast(CurrentTarget.Position) ||
                    CacheData.MonsterObstacles.Any(m => m.RActorGUID != CurrentTarget.RActorGuid && MathUtil.IntersectsPath(m.Position, 5f, CurrentTarget.Position, Player.Position))))
                {
                    // RefreshSweepingWind(true);
                    if (Passives.Monk.Momentum.IsActive && !Passives.Monk.Momentum.IsBuffActive)
                    {
                        Trinity.CurrentTarget = TargetUtil.GetDashStrikeFarthestTarget(MaxDashingStrikeRange, 25f);
                    }

                    return new TrinityPower(SNOPower.X1_Monk_DashingStrike, MaxDashingStrikeRange, CurrentTarget.ClusterPosition(5f));
                }

                if (CombatBase.QueuedMovement.IsQueuedMovement && Combat.QueuedMovementManager.Stuck.IsStuck(2f, 250))
                {
                    return new TrinityPower(SNOPower.X1_Monk_DashingStrike, 0, CombatBase.QueuedMovement.CurrentMovement.Destination);
                }

                GridNode bestCluster = GridMap.GetBestClusterNode(useDefault: false);
                float range = Passives.Monk.Momentum.IsActive && !Passives.Monk.Momentum.IsBuffActive ? 25f : 20f;
                if (bestCluster != null && bestCluster.Distance >= range)
                {
                    return new TrinityPower(SNOPower.X1_Monk_DashingStrike, MaxDashingStrikeRange, bestCluster.Position);
                }

                if (CurrentTarget.IsBoss ||
                    (CurrentTarget.IsBossOrEliteRareUnique && CurrentTarget.NearbyUnitsWithinDistance(25f) <= 1))
                {
                    GridNode gNode = GridMap.GetBestMoveNode(26f);
                    if (gNode.Position != Vector3.Zero)
                    {
                        return new TrinityPower(SNOPower.X1_Monk_DashingStrike, MaxDashingStrikeRange, gNode.Position);
                    }
                }

            }

            // Exploding Palm
            if (!UseOOCBuff && !IsCurrentlyAvoiding && !Player.IsIncapacitated &&
                CanCast(SNOPower.Monk_ExplodingPalm, CanCastFlags.NoTimer) &&
                !CurrentTarget.HasDebuff(SNOPower.Monk_ExplodingPalm))
            {
                return new TrinityPower(SNOPower.Monk_ExplodingPalm, 10f, CurrentTarget.ClusterPosition(8f), CurrentTarget.ACDGuid);
            }

            // Wave of light
            if (!UseOOCBuff && !IsCurrentlyAvoiding && !Player.IsIncapacitated && CanCast(SNOPower.Monk_WaveOfLight) &&
                (TargetUtil.AnyMobsInRange(25, Settings.Combat.Monk.MinWoLTrashCount) || CurrentTarget.IsTrashPackOrBossEliteRareUnique) &&
                (!Settings.Combat.Monk.SWBeforeWoL || (CheckAbilityAndBuff(SNOPower.Monk_SweepingWind) && GetBuffStacks(SNOPower.Monk_SweepingWind) == 3)))
            {
                float range = Skills.Monk.DashingStrike.CanCast() ? 35f : 15f;
                if (CurrentTarget.IsBossOrEliteRareUnique) { range += 10f; }

                var _target = TargetUtil.BestExploadingPalmDebuffedTarget(range);
                if (!Skills.Monk.ExplodingPalm.IsActive || TargetUtil.NumMobsInRangeOfPosition(_target.Position, 12f) <= 1 || _target.HasDebuff(SNOPower.Monk_ExplodingPalm))
                {
                    CombatBase.SwitchToTarget(_target);
                    return new TrinityPower(SNOPower.Monk_WaveOfLight, wolRange, _target.ClusterPosition((float)(wolRange - 2f)), _target.ACDGuid);
                }

            }

            // Lashing Tail Kick
            if (!UseOOCBuff && !IsCurrentlyAvoiding && !Player.IsIncapacitated && CanCast(SNOPower.Monk_LashingTailKick) &&
                (!Skills.Monk.ExplodingPalm.IsActive || TargetUtil.NumMobsInRangeOfPosition(CurrentTarget.Position, 12f) <= 1 || CurrentTarget.HasDebuff(SNOPower.Monk_ExplodingPalm)))
            {
                float range = Skills.Monk.DashingStrike.CanCast() ? 35f : 15f;
                if (CurrentTarget.IsBossOrEliteRareUnique) { range += 10f; }

                var _target = TargetUtil.BestExploadingPalmDebuffedTarget(range);
                if (!Skills.Monk.ExplodingPalm.IsActive || TargetUtil.NumMobsInRangeOfPosition(_target.Position, 12f) <= 1 || _target.HasDebuff(SNOPower.Monk_ExplodingPalm))
                {
                    CombatBase.SwitchToTarget(_target);
                    return new TrinityPower(SNOPower.Monk_LashingTailKick, 10f, _target.ClusterPosition(8f), _target.ACDGuid);
                }
            }

            // For tempest rush re-use
            if (!UseOOCBuff && Player.PrimaryResource >= 15 && CanCast(SNOPower.Monk_TempestRush) &&
                Trinity.TimeSinceUse(SNOPower.Monk_TempestRush) <= 150 &&
                ((Settings.Combat.Monk.TROption != TempestRushOption.MovementOnly) &&
                !(Settings.Combat.Monk.TROption == TempestRushOption.TrashOnly && TargetUtil.AnyElitesInRange(40f))))
            {
                GenerateMonkZigZag();
                Trinity.MaintainTempestRush = true;
                const string trUse = "Continuing Tempest Rush for Combat";
                LogTempestRushStatus(trUse);
                return new TrinityPower(SNOPower.Monk_TempestRush, 23f, ZigZagPosition, Trinity.CurrentWorldDynamicId, -1, 0, 0);
            }

            // Tempest rush at elites or groups of mobs
            if (!UseOOCBuff && !IsCurrentlyAvoiding && !Player.IsIncapacitated && !Player.IsRooted && CanCast(SNOPower.Monk_TempestRush) &&
                ((Player.PrimaryResource >= Settings.Combat.Monk.TR_MinSpirit && !IsWaitingForSpecial) || Player.PrimaryResource >= MinEnergyReserve) &&
                (Settings.Combat.Monk.TROption == TempestRushOption.Always ||
                Settings.Combat.Monk.TROption == TempestRushOption.CombatOnly ||
                (Settings.Combat.Monk.TROption == TempestRushOption.ElitesGroupsOnly && (TargetUtil.AnyElitesInRange(25) || TargetUtil.AnyMobsInRange(25, 2))) ||
                (Settings.Combat.Monk.TROption == TempestRushOption.TrashOnly && !TargetUtil.AnyElitesInRange(90f) && TargetUtil.AnyMobsInRange(40f))))
            {
                GenerateMonkZigZag();
                Trinity.MaintainTempestRush = true;
                const string trUse = "Starting Tempest Rush for Combat";
                LogTempestRushStatus(trUse);
                return new TrinityPower(SNOPower.Monk_TempestRush, 23f, ZigZagPosition, Trinity.CurrentWorldDynamicId, -1, 0, 0);
            }

            // 4 Mantra spam for the 4 second buff
            if (!UseOOCBuff && !IsCurrentlyAvoiding && !Player.IsIncapacitated && !Settings.Combat.Monk.DisableMantraSpam)
            {
                if (CanCast(SNOPower.X1_Monk_MantraOfConviction_v2) && TimeSincePowerUse(SNOPower.X1_Monk_MantraOfConviction_v2) > 1000 && (!GetHasBuff(SNOPower.X1_Monk_MantraOfConviction_v2) ||
                    (_hasSwk && TargetUtil.AnyMobsInRange(10f))) && (Player.PrimaryResource >= 80))
                {
                    return new TrinityPower(SNOPower.X1_Monk_MantraOfConviction_v2, 3);
                }

                if (CanCast(SNOPower.X1_Monk_MantraOfRetribution_v2) && TimeSincePowerUse(SNOPower.X1_Monk_MantraOfRetribution_v2) > 1000 && (!GetHasBuff(SNOPower.X1_Monk_MantraOfRetribution_v2) ||
                    (_hasSwk && TargetUtil.AnyMobsInRange(10f))) && (Player.PrimaryResource >= 80))
                {
                    return new TrinityPower(SNOPower.X1_Monk_MantraOfRetribution_v2, 3);
                }
                if (CanCast(SNOPower.X1_Monk_MantraOfHealing_v2) && TimeSincePowerUse(SNOPower.X1_Monk_MantraOfHealing_v2) > 1000 && (!GetHasBuff(SNOPower.X1_Monk_MantraOfRetribution_v2) ||
                    (_hasSwk && TargetUtil.AnyMobsInRange(10f))) && (Player.PrimaryResource >= 80))
                {
                    return new TrinityPower(SNOPower.X1_Monk_MantraOfHealing_v2, 3);
                }
                if (CanCast(SNOPower.X1_Monk_MantraOfEvasion_v2) && TimeSincePowerUse(SNOPower.X1_Monk_MantraOfEvasion_v2) > 1000 && (!GetHasBuff(SNOPower.X1_Monk_MantraOfRetribution_v2) ||
                    (_hasSwk && TargetUtil.AnyMobsInRange(10f))) && (Player.PrimaryResource >= 80))
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
            if (!UseOOCBuff && !IsCurrentlyAvoiding && CanCast(SNOPower.Monk_FistsofThunder) && Runes.Monk.Thunderclap.IsActive && CurrentTarget.Distance > 16f)
            {
                // RefreshSweepingWind();
                return new TrinityPower(SNOPower.Monk_FistsofThunder, 30f, CurrentTarget.ClusterPosition(28f), CurrentTarget.ACDGuid);
            }

            // Deadly Reach: Foresight, every 27 seconds or 2.7 seconds with combo strike
            if (!UseOOCBuff && !IsCurrentlyAvoiding && CanCast(SNOPower.Monk_DeadlyReach) && (isDualOrTriGen || Runes.Monk.Foresight.IsActive) &&
                (SpellHistory.TimeSinceUse(SNOPower.Monk_DeadlyReach) > TimeSpan.FromMilliseconds(drInterval) ||
                (SpellHistory.SpellUseCountInTime(SNOPower.Monk_DeadlyReach, TimeSpan.FromMilliseconds(27000)) < 3) && Runes.Monk.Foresight.IsActive))
            {
                // RefreshSweepingWind();
                return new TrinityPower(SNOPower.Monk_DeadlyReach, 16f, CurrentTarget.ClusterPosition(14f), CurrentTarget.ACDGuid);
            }

            // Way of the Hundred Fists: Blazing Fists, every 4-5ish seconds or if we don't have 3 stacks of the buff or or 2.7 seconds with combo strike
            if (!UseOOCBuff && !IsCurrentlyAvoiding && CanCast(SNOPower.Monk_WayOfTheHundredFists) && (isDualOrTriGen || Runes.Monk.BlazingFists.IsActive) &&
                (GetBuffStacks(SNOPower.Monk_WayOfTheHundredFists) < 3 ||
                SpellHistory.TimeSinceUse(SNOPower.Monk_WayOfTheHundredFists) > TimeSpan.FromMilliseconds(wothfInterval)))
            {
                // RefreshSweepingWind();
                return new TrinityPower(SNOPower.Monk_WayOfTheHundredFists, 16f, CurrentTarget.ClusterPosition(14f), CurrentTarget.ACDGuid);
            }

            // Crippling Wave
            if (!UseOOCBuff && !IsCurrentlyAvoiding && CanCast(SNOPower.Monk_CripplingWave) &&
                SpellHistory.TimeSinceUse(SNOPower.Monk_CripplingWave) > TimeSpan.FromMilliseconds(cwInterval))
            {
                // RefreshSweepingWind();
                return new TrinityPower(SNOPower.Monk_CripplingWave, 20f, CurrentTarget.ClusterPosition(18f), CurrentTarget.ACDGuid);
            }

            // Fists of Thunder
            if (!UseOOCBuff && !IsCurrentlyAvoiding && CanCast(SNOPower.Monk_FistsofThunder))
            {
                // RefreshSweepingWind();
                return new TrinityPower(SNOPower.Monk_FistsofThunder, 30f, CurrentTarget.ClusterPosition(28f), CurrentTarget.ACDGuid);
            }

            // Deadly Reach normal
            if (!UseOOCBuff && !IsCurrentlyAvoiding && CanCast(SNOPower.Monk_DeadlyReach))
            {
                // RefreshSweepingWind();
                return new TrinityPower(SNOPower.Monk_DeadlyReach, 16f, CurrentTarget.ClusterPosition(14f), CurrentTarget.ACDGuid);
            }

            // Way of the Hundred Fists normal
            if (!UseOOCBuff && !IsCurrentlyAvoiding && CanCast(SNOPower.Monk_WayOfTheHundredFists))
            {
                // RefreshSweepingWind();
                return new TrinityPower(SNOPower.Monk_WayOfTheHundredFists, 16f, CurrentTarget.ClusterPosition(14f), CurrentTarget.ACDGuid);
            }

            // Crippling Wave Normal
            if (!UseOOCBuff && !IsCurrentlyAvoiding && CanCast(SNOPower.Monk_CripplingWave))
            {
                // RefreshSweepingWind();
                return new TrinityPower(SNOPower.Monk_CripplingWave, 30f, CurrentTarget.ClusterPosition(28f), CurrentTarget.ACDGuid);
            }

            // Wave of light as primary 
            if (!UseOOCBuff && !IsCurrentlyAvoiding && !Player.IsIncapacitated && CanCast(SNOPower.Monk_WaveOfLight))
            {
                // RefreshSweepingWind(true);
                return new TrinityPower(SNOPower.Monk_WaveOfLight, wolRange, CurrentTarget.ClusterPosition((float)(wolRange - 2f)), CurrentTarget.ACDGuid);
            }

            // Default attacks
            return DefaultPower;
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
        /// Determines if we should change targets to apply Exploading Palm to another target
        /// </summary>
        internal static bool ShouldSpreadExplodingPalm()
        {
            return CurrentTarget != null && Skills.Monk.ExplodingPalm.IsActive &&

                // Current target is valid
                 CurrentTarget.IsUnit && !CurrentTarget.IsTreasureGoblin &&

                // Avoid rapidly changing targets
                //DateTime.UtcNow.Subtract(_lastTargetChange).TotalMilliseconds > 500 &&

                // enough resources and mobs nearby
                Player.PrimaryResource > 40 && //TargetUtil.AnyMobsInRange(20f, 2) &&

                // Don't bother if X or more targets already have EP
                CurrentTarget.HasDebuff(SNOPower.Monk_ExplodingPalm);

        }

        /// <summary>
        /// Blacklist the current target for 3 seconds and attempt to find a new target one
        /// </summary>
        internal static void ChangeTarget()
        {
            float range = Skills.Monk.DashingStrike.CanCast() ? 35f : 15f;
            if (CurrentTarget.IsBossOrEliteRareUnique) { range += 10f; }

            var bestExplodingPalmTarget = TargetUtil.BestExploadingPalmTarget(range);
            if (bestExplodingPalmTarget != default(TrinityCacheObject) &&
                bestExplodingPalmTarget.RActorGuid != CurrentTarget.RActorGuid)
            {
                //Trinity.Blacklist1Second.Add(CurrentTarget.RActorGuid);
                if (CombatBase.SwitchToTarget(bestExplodingPalmTarget))
                    _lastTargetChange = DateTime.UtcNow;
            }
        }

        private static bool CanCastDashingStrike
        {
            get { return !UseOOCBuff && !Player.IsIncapacitated && CanCast(SNOPower.X1_Monk_DashingStrike, CanCastFlags.NoTimer) && !IsCurrentlyAvoiding; }
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
            if (TargetUtil.AnyMobsInRange(25f, 3) || TargetUtil.IsEliteTargetInRange(70f)) // surround by mobs or elite engaged.
            {
                if (farthestTarget != null) // found a target within 33-49 yards.
                {
                    // RefreshSweepingWind();
                    return new TrinityPower(SNOPower.X1_Monk_DashingStrike, MaxDashingStrikeRange, farthestTarget.Position, Trinity.CurrentWorldDynamicId, -1, 2, 2);
                }
                // no free target found, get a nearby cluster point instead.
                var bestClusterPoint = TargetUtil.GetBestClusterPoint(15f, procDistance);
                // RefreshSweepingWind();
                return new TrinityPower(SNOPower.X1_Monk_DashingStrike, MaxDashingStrikeRange, bestClusterPoint, Trinity.CurrentWorldDynamicId, -1, 2, 2);
            }

            //usually this trigger after dash to the farthest target. dash a single mobs >30 yards, trying to dash back the cluster
            if (TargetUtil.ClusterExists(20, 50, 3))
            {
                var dashStrikeBestClusterPoint = TargetUtil.GetBestClusterPoint(20f, 20f);
                if (dashStrikeBestClusterPoint != Trinity.Player.Position)
                {
                    // RefreshSweepingWind();
                    return new TrinityPower(SNOPower.X1_Monk_DashingStrike, MaxDashingStrikeRange, dashStrikeBestClusterPoint, Trinity.CurrentWorldDynamicId, -1, 2, 2);
                }
            }

            // dash to anything which is free.               
            if (farthestTarget != null)
            {
                // RefreshSweepingWind();
                return new TrinityPower(SNOPower.X1_Monk_DashingStrike, MaxDashingStrikeRange, farthestTarget.Position, Trinity.CurrentWorldDynamicId, -1, 2, 2);
            }

            return new TrinityPower(SNOPower.X1_Monk_DashingStrike, MaxDashingStrikeRange, CurrentTarget.Position, Trinity.CurrentWorldDynamicId, -1, 2, 2);
        }

        internal static void RefreshSweepingWind(bool spendsSpirit = false)
        {
            // Don't refresh timer if we have Taeguk Legendary Gem equipped and not spending spirit
            //if (!spendsSpirit && IsTaegukEquipped)
            //    return;

            //if (GetHasBuff(SNOPower.Monk_SweepingWind))
            //LastSweepingWindRefresh = DateTime.UtcNow;
        }

        internal static void GenerateMonkZigZag()
        {
            float fExtraDistance = CurrentTarget.RadiusDistance <= 20f ? 15f : 20f;
            ZigZagPosition = TargetUtil.GetZigZagTarget(CurrentTarget.Position, fExtraDistance);
            double direction = MathUtil.FindDirectionRadian(Player.Position, ZigZagPosition);
            ZigZagPosition = MathEx.GetPointAt(Player.Position, 40f, (float)direction);
            Logger.Log(TrinityLogLevel.Debug, LogCategory.Behavior, "Generated ZigZag {0} distance {1:0}", ZigZagPosition, ZigZagPosition.Distance2D(Player.Position));
            LastZigZagUnitAcdGuid = CurrentTarget.ACDGuid;
            LastChangedZigZag = DateTime.UtcNow;
        }

        internal static TrinityPower GetMonkDestroyPower()
        {
            if (IsTempestRushReady())
                return new TrinityPower(SNOPower.Monk_TempestRush, 5f, Vector3.Zero, -1, -1, 0, 0);
            if (CanCast(SNOPower.X1_Monk_DashingStrike))
                return new TrinityPower(SNOPower.X1_Monk_DashingStrike, MaxDashingStrikeRange, Vector3.Zero, -1, -1, 0, 0);
            if (CanCast(SNOPower.Monk_FistsofThunder))
                return new TrinityPower(SNOPower.Monk_FistsofThunder, 5f, Vector3.Zero, -1, -1, 0, 0);
            if (CanCast(SNOPower.Monk_DeadlyReach))
                return new TrinityPower(SNOPower.Monk_DeadlyReach, 5f, Vector3.Zero, -1, -1, 0, 0);
            if (CanCast(SNOPower.Monk_CripplingWave))
                return new TrinityPower(SNOPower.Monk_CripplingWave, 5f, Vector3.Zero, -1, -1, 0, 0);
            if (CanCast(SNOPower.Monk_WayOfTheHundredFists))
                return new TrinityPower(SNOPower.Monk_WayOfTheHundredFists, 5f, Vector3.Zero, -1, -1, 0, 0);
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
            if (ZetaDia.Service.Hero == null)
                return;

            if (!ZetaDia.Service.Hero.IsValid)
                return;

            if (!ZetaDia.IsInGame)
                return;

            if (ZetaDia.IsLoadingWorld)
                return;

            if (!ZetaDia.Me.IsValid)
                return;

            if (!ZetaDia.Me.CommonData.IsValid)
                return;

            if (!Player.IsValid)
                return;

            if (Player.IsDead)
                return;

            MaintainTempestRush();
            MaintainSweepingWind();
        }

        private static void MaintainSweepingWind()
        {
            if (!CanCastRecurringPower())
                return;

            //if (Player.IsInTown)
            //return;

            // Sweeping Winds Refresh(wtf the last logic)
            if (Player.PrimaryResource >= _minSweepingWindSpirit &&
               CanCast(SNOPower.Monk_SweepingWind, CanCastFlags.NoTimer) &&
               ((!GetHasBuff(SNOPower.Monk_SweepingWind) && (_hasInnaSet || (Trinity.ObjectCache != null && TargetUtil.AnyMobsInRange(18f)))) ||
               CombatBase.IsTaegukBuffWillExpire))
            {
                Logger.Log("Sweeping Wind Out of Band Refresh {0}",
                    CombatBase.Cast(new TrinityPower(SNOPower.Monk_SweepingWind)) ?
                    "succeeded" : "failed");
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
                    case GObjectType.Unit:
                    case GObjectType.Gold:
                    case GObjectType.Avoidance:
                    case GObjectType.Barricade:
                    case GObjectType.Destructible:
                    case GObjectType.HealthGlobe:
                    case GObjectType.PowerGlobe:
                    case GObjectType.ProgressionGlobe:
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
    }
}
