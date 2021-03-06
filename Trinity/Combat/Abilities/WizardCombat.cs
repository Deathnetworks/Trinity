﻿using System;
using System.Linq;
using Trinity.Reference;
using Trinity.Technicals;
using Zeta.Common;
using Zeta.Game;
using Zeta.Game.Internals.Actors;
using Logger = Trinity.Technicals.Logger;

namespace Trinity.Combat.Abilities
{
    class WizardCombat : CombatBase
    {
        public static int SerpentSparkerId = 272084;

        internal static Config.Combat.WizardSetting WizardSettings
        {
            get { return Trinity.Settings.Combat.Wizard; }
        }

        // Wizards want to save up to a reserve of 45+ energy
        private new const int MinEnergyReserve = 45;

        /// <summary>
        /// Checks and casts Buffs, Avoidance powers, and Combat Powers
        /// </summary>
        /// <returns></returns>
        internal static TrinityPower GetPower()
        {
            // Buffs
            var power = GetBuffPower();
            if (power != null && power.SNOPower != SNOPower.None)
                return power;

            // Destructibles
            if (UseDestructiblePower)
                return DestroyObjectPower();

            // In Combat, Avoiding
            if (IsCurrentlyAvoiding)
            {
                return GetCombatAvoidancePower();
            }
            // In combat, Not Avoiding
            if (CurrentTarget != null)
            {
                return GetCombatPower();
            }

            // Default attacks
            return DefaultPower;
        }

        /// <summary>
        /// Gets the best (non-movement related) avoidance power
        /// </summary>
        /// <returns></returns>
        private static TrinityPower GetCombatAvoidancePower()
        {
            // Defensive Teleport: SafePassage
            if (CanCast(SNOPower.Wizard_Teleport, CanCastFlags.NoTimer) && Runes.Wizard.SafePassage.IsActive && Player.CurrentHealthPct <= 0.75)
            {
                var target = NavHelper.FindSafeZone(false, 1, CurrentTarget.Position, true);
                return new TrinityPower(SNOPower.Wizard_Teleport, 65f, target);
            }

            // Diamond Skin: Tank mode
            if (CanCast(SNOPower.Wizard_DiamondSkin) && LastPowerUsed != SNOPower.Wizard_DiamondSkin && !GetHasBuff(SNOPower.Wizard_DiamondSkin) &&
                (TargetUtil.AnyElitesInRange(25, 1) || TargetUtil.AnyMobsInRange(25, 1) || Player.CurrentHealthPct <= 0.90 || Player.IsIncapacitated || Player.IsRooted || CurrentTarget.RadiusDistance <= 40f))
            {
                return new TrinityPower(SNOPower.Wizard_DiamondSkin);
            }

            // Explosive Blast
            if (CanCast(SNOPower.Wizard_ExplosiveBlast, CanCastFlags.NoTimer) && !Player.IsIncapacitated && !Player.IsInTown)
            {
                return new TrinityPower(SNOPower.Wizard_ExplosiveBlast, 10f);
            }

            // Frost Nova
            if (CanCast(SNOPower.Wizard_FrostNova) && !Player.IsIncapacitated &&
                ((Runes.Wizard.DeepFreeze.IsActive && TargetUtil.AnyMobsInRange(25, 5)) || (!Runes.Wizard.DeepFreeze.IsActive && (TargetUtil.AnyMobsInRange(25, 1) || Player.CurrentHealthPct <= 0.7)) &&
                CurrentTarget.RadiusDistance <= 25f))
            {
                return new TrinityPower(SNOPower.Wizard_FrostNova, 20f);
            }


            return null;
        }

        /// <summary>
        /// Gets the best combat power for the current conditions
        /// </summary>
        /// <returns></returns>
        private static TrinityPower GetCombatPower()
        {
            TrinityPower power = null;
            if (GetHasBuff(SNOPower.Wizard_Archon))
            {
                power = GetArchonPower();
            }

            // Offensive Teleport: Calamity
            if (CanCast(SNOPower.Wizard_Teleport, CanCastFlags.NoTimer) && Runes.Wizard.Calamity.IsActive)
            {
                var bestClusterPoint = TargetUtil.GetBestClusterPoint(5f, 10f);
                return new TrinityPower(SNOPower.Wizard_Teleport, 55f, bestClusterPoint);
            }

            // Defensive Teleport: SafePassage
            if (CanCast(SNOPower.Wizard_Teleport, CanCastFlags.NoTimer) && Runes.Wizard.SafePassage.IsActive &&
                Player.CurrentHealthPct <= 0.50 &&
                (CurrentTarget.IsBossOrEliteRareUnique || TargetUtil.IsEliteTargetInRange(75f)))
            {
                var target = KiteDistance == 0 ? ZetaDia.Me.Position : NavHelper.FindSafeZone(false, 1, CurrentTarget.Position, true);
                return new TrinityPower(SNOPower.Wizard_Teleport, 65f, target);
            }

            // Wormhole / Black hole
            float blackholeRadius = Runes.Wizard.Supermassive.IsActive ? 20f : 15f;
            if (CanCast(SNOPower.X1_Wizard_Wormhole, CanCastFlags.NoTimer) && !ShouldWaitForConventionElement(Skills.Wizard.BlackHole) &&
                (TargetUtil.ClusterExists(blackholeRadius, 45f, Trinity.Settings.Combat.Wizard.BlackHoleAoECount) || CurrentTarget.IsBossOrEliteRareUnique))
            {
                return new TrinityPower(SNOPower.X1_Wizard_Wormhole, 65f, TargetUtil.GetBestClusterUnit(blackholeRadius, 45f, 1, false).Position);
            }

            // Meteor: Arcane Dynamo
            bool arcaneDynamoPassiveReady = (Passives.Wizard.ArcaneDynamo.IsActive && GetBuffStacks(SNOPower.Wizard_Passive_ArcaneDynamo) == 5);
            if (!Player.IsIncapacitated && arcaneDynamoPassiveReady && CanCast(SNOPower.Wizard_Meteor, CanCastFlags.NoTimer) && !ShouldWaitForConventionElement(Skills.Wizard.Meteor) &&
                (TargetUtil.EliteOrTrashInRange(65) || TargetUtil.ClusterExists(15f, 65, 2)))
            {
                var bestMeteorClusterUnit = TargetUtil.GetBestClusterUnit();
                return new TrinityPower(SNOPower.Wizard_Meteor, 65f, bestMeteorClusterUnit.Position);
            }

            // Diamond Skin: Tank mode
            if (CanCast(SNOPower.Wizard_DiamondSkin) && LastPowerUsed != SNOPower.Wizard_DiamondSkin && !GetHasBuff(SNOPower.Wizard_DiamondSkin) &&
                (TargetUtil.AnyElitesInRange(25, 1) || TargetUtil.AnyMobsInRange(25, 1) || Player.CurrentHealthPct <= 0.90 || Player.IsIncapacitated || Player.IsRooted || CurrentTarget.RadiusDistance <= 40f))
            {
                return new TrinityPower(SNOPower.Wizard_DiamondSkin);
            }

            // Slow Time for in combat
            if (!Player.IsIncapacitated && Skills.Wizard.SlowTime.CanCast(CanCastFlags.NoTimer))
            {
                // Defensive Bubble is Priority
                if ((Enemies.Nearby.UnitCount >= 8 || Enemies.Nearby.UnitCount >= 2 || Enemies.CloseNearby.Units.Any()) && Runes.Wizard.PointOfNoReturn.IsActive || Runes.Wizard.StretchTime.IsActive)
                {
                    return new TrinityPower(SNOPower.Wizard_SlowTime, 0f, Player.Position);
                }                

                // Then casting on elites
                if (CurrentTarget.IsBossOrEliteRareUnique && CurrentTarget.Distance < 57f)
                {
                    return new TrinityPower(SNOPower.Wizard_SlowTime, 0f, CurrentTarget.Position);
                }

                // Then big clusters
                var clusterPosition = TargetUtil.GetBestClusterPoint();
                if (TargetUtil.ClusterExists(50f, 5) && clusterPosition.Distance2D(Player.Position) < 57f)
                {
                    return new TrinityPower(SNOPower.Wizard_SlowTime, 0f, clusterPosition);
                }

                // Cooldown isnt an issue with Magnum Opus, so just cast it somewhere.
                if (Sets.DelseresMagnumOpus.IsEquipped)
                {
                    if (Enemies.BestLargeCluster.Exists && Enemies.BestLargeCluster.Position.Distance2D(Player.Position) < 57f)
                    {
                        return new TrinityPower(SNOPower.Wizard_SlowTime, 0f, Enemies.BestLargeCluster.Position);
                    }

                    if (Enemies.Nearby.UnitCount > 2)
                    {
                        return new TrinityPower(SNOPower.Wizard_SlowTime, 0f, Enemies.BestCluster.Position);
                    }

                    return new TrinityPower(SNOPower.Wizard_SlowTime, 0f, Player.Position);           
                }
            }

            // Mirror Image  @ half health or 5+ monsters or rooted/incapacitated or last elite left @25% health
            if (CanCast(SNOPower.Wizard_MirrorImage, CanCastFlags.NoTimer) &&
                (Player.CurrentHealthPct <= CombatBase.EmergencyHealthPotionLimit || TargetUtil.AnyMobsInRange(30, 4) || Player.IsIncapacitated || Player.IsRooted ||
                TargetUtil.AnyElitesInRange(30) || CurrentTarget.IsBossOrEliteRareUnique))
            {
                return new TrinityPower(SNOPower.Wizard_MirrorImage);
            }

            // Hydra
            if (CanCast(SNOPower.Wizard_Hydra, CanCastFlags.NoTimer))
            {
                // ReSharper disable once InconsistentNaming
                var _14s = TimeSpan.FromSeconds(14);
                const float maxHydraDistance = 25f;
                const float castDistance = 65f;
                const float maxHydraDistSqr = maxHydraDistance * maxHydraDistance;

                // This will check if We have the "Serpent Sparker" wand, and attempt to cast a 2nd hydra immediately after the first

                bool serpentSparkerRecast1 = Legendary.SerpentsSparker.IsEquipped && LastPowerUsed == SNOPower.Wizard_Hydra &&
                    SpellHistory.SpellUseCountInTime(SNOPower.Wizard_Hydra, TimeSpan.FromSeconds(2)) < 2;

                int baseRecastDelay = HasPrimarySkill || Player.PrimaryResource < 60 ? 14 : 3;
                bool baseRecast = TimeSpanSincePowerUse(SNOPower.Wizard_Hydra) > TimeSpan.FromSeconds(baseRecastDelay);
                var lastCast = SpellHistory.HistoryQueue
                    .Where(p => p.Power.SNOPower == SNOPower.Wizard_Hydra && p.TimeSinceUse < _14s)
                    .OrderBy(s => s.TimeSinceUse).ThenBy(p => p.Power.TargetPosition.Distance2DSqr(CurrentTarget.Position))
                    .FirstOrDefault();

                bool distanceRecast = lastCast != null && lastCast.TargetPosition.Distance2DSqr(CurrentTarget.Position) > maxHydraDistSqr;

                bool twoAlredyCastIn5Sec = SpellHistory.SpellUseCountInTime(SNOPower.Wizard_Hydra, TimeSpan.FromSeconds(5)) >= 2;

                if (!Player.IsIncapacitated && CanCast(SNOPower.Wizard_Hydra, CanCastFlags.NoTimer) && !ShouldWaitForConventionElement(Skills.Wizard.Hydra) &&
                    (baseRecast || distanceRecast || serpentSparkerRecast1) && !twoAlredyCastIn5Sec &&
                    CurrentTarget.RadiusDistance <= castDistance && Player.PrimaryResource >= 15)
                {
                    var pos = TargetUtil.GetBestClusterPoint(maxHydraDistance);
                    return new TrinityPower(SNOPower.Wizard_Hydra, 55f, pos);
                }

            }

            // Archon
            if (CanCast(SNOPower.Wizard_Archon, CanCastFlags.NoTimer) && ShouldStartArchon())
            {
                IsWaitingForSpecial = false;
                return new TrinityPower(SNOPower.Wizard_Archon, 5, 5);
            }

            if (Hotbar.Contains(SNOPower.Wizard_Archon))
            {
                IsWaitingForSpecial = true;
            }

            // Explosive Blast
            //We should check if Wand of Woh is equipped to define the best routine
            if (Legendary.WandOfWoh.IsEquipped)
            {
                if (!Player.IsIncapacitated && CanCast(SNOPower.Wizard_ExplosiveBlast, CanCastFlags.NoTimer) && !Player.IsInTown && !ShouldWaitForConventionElement(Skills.Wizard.ExplosiveBlast))
                {
                    return new TrinityPower(SNOPower.Wizard_ExplosiveBlast, 10f);
                }
            }
            else
            {
                if (!Player.IsIncapacitated && CanCast(SNOPower.Wizard_ExplosiveBlast, CanCastFlags.NoTimer) && Player.PrimaryResource >= 20 && !ShouldWaitForConventionElement(Skills.Wizard.ExplosiveBlast))
                {
                    return new TrinityPower(SNOPower.Wizard_ExplosiveBlast, 12f, CurrentTarget.Position);
                }
            }

            // Blizzard
            float blizzardRadius = Runes.Wizard.Apocalypse.IsActive ? 30f : 12f;
            if (!Player.IsIncapacitated && CanCast(SNOPower.Wizard_Blizzard, CanCastFlags.NoTimer) && !ShouldWaitForConventionElement(Skills.Wizard.Blizzard) &&
                (TargetUtil.ClusterExists(blizzardRadius, 90f, 2, false) || CurrentTarget.IsBossOrEliteRareUnique || !HasPrimarySkill) &&
                (Player.PrimaryResource >= 40 || (Runes.Wizard.Snowbound.IsActive && Player.PrimaryResource >= 20)))
            {
                var bestClusterPoint = TargetUtil.GetBestClusterPoint(blizzardRadius, 65f, false);
                return new TrinityPower(SNOPower.Wizard_Blizzard, 65f, bestClusterPoint);
            }

            // Meteor - no arcane dynamo
            if (!Player.IsIncapacitated && !Passives.Wizard.ArcaneDynamo.IsActive && CanCast(SNOPower.Wizard_Meteor, CanCastFlags.NoTimer) && !ShouldWaitForConventionElement(Skills.Wizard.Meteor) &&
                (TargetUtil.EliteOrTrashInRange(65) || TargetUtil.ClusterExists(15f, 65, 2)))
            {
                return new TrinityPower(SNOPower.Wizard_Meteor, 65f, TargetUtil.GetBestClusterPoint());
            }

            // Frost Nova
            if (CanCast(SNOPower.Wizard_FrostNova) && !Player.IsIncapacitated && !ShouldWaitForConventionElement(Skills.Wizard.FrostNova) &&
                ((Runes.Wizard.DeepFreeze.IsActive && TargetUtil.AnyMobsInRange(25, 5)) || (!Runes.Wizard.DeepFreeze.IsActive && (TargetUtil.AnyMobsInRange(25, 1) || Player.CurrentHealthPct <= 0.7)) &&
                CurrentTarget.RadiusDistance <= 25f))
            {
                return new TrinityPower(SNOPower.Wizard_FrostNova, 20f);
            }

            // Check to see if we have a signature spell on our hotbar, for energy twister check
            bool hasSignatureSpell = (Hotbar.Contains(SNOPower.Wizard_MagicMissile) || Hotbar.Contains(SNOPower.Wizard_ShockPulse) ||
                Hotbar.Contains(SNOPower.Wizard_SpectralBlade) || Hotbar.Contains(SNOPower.Wizard_Electrocute));

            // Energy Twister SPAMS whenever 35 or more ap to generate Arcane Power
            if (!Player.IsIncapacitated && CanCast(SNOPower.Wizard_EnergyTwister) && !ShouldWaitForConventionElement(Skills.Wizard.EnergyTwister) &&
                Player.PrimaryResource >= 35 &&
                // If using storm chaser, then force a signature spell every 1 stack of the buff, if we have a signature spell
                (!hasSignatureSpell || GetBuffStacks(SNOPower.Wizard_EnergyTwister) < 1) &&
                ((!Runes.Wizard.WickedWind.IsActive && CurrentTarget.RadiusDistance <= 25f) ||
                (Runes.Wizard.WickedWind.IsActive && CurrentTarget.RadiusDistance <= 60f)) &&
                (!Hotbar.Contains(SNOPower.Wizard_Electrocute) || !DataDictionary.FastMovingMonsterIds.Contains(CurrentTarget.ActorSNO)))
            {
                Vector3 bestClusterPoint = TargetUtil.GetBestClusterPoint(10f, 15f);

                const float twisterRange = 28f;
                return new TrinityPower(SNOPower.Wizard_EnergyTwister, twisterRange, bestClusterPoint);
            }

            // Wave of force
            if (!Player.IsIncapacitated && Player.PrimaryResource >= 25 && CanCast(SNOPower.Wizard_WaveOfForce, CanCastFlags.NoTimer) && !ShouldWaitForConventionElement(Skills.Wizard.WaveOfForce))
            {
                return new TrinityPower(SNOPower.Wizard_WaveOfForce, 15f, CurrentTarget.Position);
            }

            float disintegrateRange = Runes.Wizard.Entropy.IsActive ? 10f : 35f;
            // Disintegrate
            if (!Player.IsIncapacitated && CanCast(SNOPower.Wizard_Disintegrate) && !ShouldWaitForConventionElement(Skills.Wizard.Disintegrate) &&
                ((Player.PrimaryResource >= 20 && !IsWaitingForSpecial) || Player.PrimaryResource >= MinEnergyReserve))
            {
                return new TrinityPower(SNOPower.Wizard_Disintegrate, disintegrateRange, Vector3.Zero, -1, CurrentTarget.ACDGuid, 0, 0);
            }
            // Arcane Orb
            if (!Player.IsIncapacitated && CanCast(SNOPower.Wizard_ArcaneOrb) && !ShouldWaitForConventionElement(Skills.Wizard.ArcaneOrb) &&
                ((Player.PrimaryResource >= 30 && !IsWaitingForSpecial) || Player.PrimaryResource >= MinEnergyReserve))
            {
                return new TrinityPower(SNOPower.Wizard_ArcaneOrb, 35f, CurrentTarget.ACDGuid);
            }
            // Arcane Torrent
            if (!Player.IsIncapacitated && CanCast(SNOPower.Wizard_ArcaneTorrent) && !ShouldWaitForConventionElement(Skills.Wizard.ArcaneTorrent) &&
                ((Player.PrimaryResource >= 16 && !IsWaitingForSpecial) || Player.PrimaryResource >= MinEnergyReserve))
            {
                return new TrinityPower(SNOPower.Wizard_ArcaneTorrent, 40f, CurrentTarget.ACDGuid);
            }

            // Ray of Frost
            if (!Player.IsIncapacitated && CanCast(SNOPower.Wizard_RayOfFrost) && !ShouldWaitForConventionElement(Skills.Wizard.RayOfFrost) &&
                (!IsWaitingForSpecial || (Player.PrimaryResource > MinEnergyReserve)))
            {
                float range = 50f;
                if (Runes.Wizard.SleetStorm.IsActive)
                    range = 5f;

                return new TrinityPower(SNOPower.Wizard_RayOfFrost, range, CurrentTarget.ACDGuid);
            }

            // Magic Missile
            if (CanCast(SNOPower.Wizard_MagicMissile))
            {
                var bestPierceTarget = TargetUtil.GetBestPierceTarget(45f);
                int targetId;

                if (bestPierceTarget != null)
                    targetId = Runes.Wizard.Conflagrate.IsActive ?
                        bestPierceTarget.ACDGuid :
                        CurrentTarget.ACDGuid;
                else
                    targetId = CurrentTarget.ACDGuid;

                return new TrinityPower(SNOPower.Wizard_MagicMissile, 45f, targetId);
            }

            // Shock Pulse
            if (CanCast(SNOPower.Wizard_ShockPulse))
            {
                return new TrinityPower(SNOPower.Wizard_ShockPulse, 10f, CurrentTarget.ACDGuid);
            }
            // Spectral Blade
            if (CanCast(SNOPower.Wizard_SpectralBlade))
            {
                return new TrinityPower(SNOPower.Wizard_SpectralBlade, 14f, CurrentTarget.ACDGuid);
            }
            // Electrocute
            if (CanCast(SNOPower.Wizard_Electrocute))
            {
                return new TrinityPower(SNOPower.Wizard_Electrocute, 40f, CurrentTarget.ACDGuid);
            }

            // Default Attacks
            if (IsNull(power))
            {
                // Never use Melee (e.g. Range < 10f), only ranged attacks
                power = DefaultPower.MinimumRange > 11f ? DefaultPower : new TrinityPower(SNOPower.Walk);
            }
            return power;
        }

        /// <summary>
        /// Checks and casts buffs if needed
        /// </summary>
        /// <returns></returns>
        private static TrinityPower GetBuffPower()
        {
            // Illusionist speed boost
            if (Passives.Wizard.Illusionist.IsActive)
            {
                // Slow Time on self for speed boost
                if (CanCast(SNOPower.Wizard_SlowTime))
                    return new TrinityPower(SNOPower.Wizard_SlowTime, 0f, Player.Position);

                // Mirror Image for speed boost
                if (CanCast(SNOPower.Wizard_MirrorImage))
                    return new TrinityPower(SNOPower.Wizard_MirrorImage);

                // Teleport already called from PlayerMover, not here (since it's a "movement" spell, not a buff)
            }
            // Magic Weapon (10 minutes)                 
            if (!Player.IsIncapacitated && Player.PrimaryResource >= 25 && CanCast(SNOPower.Wizard_MagicWeapon) && !GetHasBuff(SNOPower.Wizard_MagicWeapon))
            {
                return new TrinityPower(SNOPower.Wizard_MagicWeapon);
            }
            // Diamond Skin off CD
            bool hasSleekShell = CacheData.Hotbar.ActiveSkills.Any(s => s.Power == SNOPower.Wizard_DiamondSkin && s.RuneIndex == 0);
            if (hasSleekShell && CanCast(SNOPower.Wizard_DiamondSkin))
            {
                return new TrinityPower(SNOPower.Wizard_DiamondSkin);
            }
            // Familiar
            if (!Player.IsIncapacitated && CanCast(SNOPower.Wizard_Familiar) && Player.PrimaryResource >= 20 && !IsFamiliarActive)
            {
                return new TrinityPower(SNOPower.Wizard_Familiar);
            }

            // The three wizard armors, done in an else-if loop so it doesn't keep replacing one with the other
            if (!Player.IsIncapacitated && Player.PrimaryResource >= 25)
            {
                // Energy armor as priority cast if available and not buffed
                if (Hotbar.Contains(SNOPower.Wizard_EnergyArmor))
                {
                    if ((!GetHasBuff(SNOPower.Wizard_EnergyArmor) && CanCast(SNOPower.Wizard_EnergyArmor)) || (Hotbar.Contains(SNOPower.Wizard_Archon) && (!GetHasBuff(SNOPower.Wizard_EnergyArmor) || SNOPowerUseTimer(SNOPower.Wizard_EnergyArmor))))
                    {
                        return new TrinityPower(SNOPower.Wizard_EnergyArmor);
                    }
                }
                // Ice Armor
                else if (Hotbar.Contains(SNOPower.Wizard_IceArmor))
                {
                    if (!GetHasBuff(SNOPower.Wizard_IceArmor) && CanCast(SNOPower.Wizard_IceArmor))
                    {
                        return new TrinityPower(SNOPower.Wizard_IceArmor);
                    }
                }
                // Storm Armor
                else if (Hotbar.Contains(SNOPower.Wizard_StormArmor))
                {
                    if (!GetHasBuff(SNOPower.Wizard_StormArmor) && CanCast(SNOPower.Wizard_StormArmor))
                    {
                        return new TrinityPower(SNOPower.Wizard_StormArmor);
                    }
                }
            }
            // Mirror Image  @ half health or 5+ monsters or rooted/incapacitated or last elite left @25% health
            if (CanCast(SNOPower.Wizard_MirrorImage, CanCastFlags.NoTimer) &&
                (Player.CurrentHealthPct <= CombatBase.EmergencyHealthPotionLimit || TargetUtil.AnyMobsInRange(30, 4) || Player.IsIncapacitated || Player.IsRooted))
            {
                return new TrinityPower(SNOPower.Wizard_MirrorImage);
            }

            return null;
        }

        /// <summary>
        /// Gets the best Archon power for the current conditions
        /// </summary>
        /// <returns></returns>
        private static TrinityPower GetArchonPower()
        {
            // Archon form
            // Archon Slow Time for in combat
            if (!Player.IsIncapacitated &&
                CanCast(SNOPower.Wizard_Archon_SlowTime, CanCastFlags.NoTimer) &&
                (TimeSpanSincePowerUse(SNOPower.Wizard_Archon_SlowTime) > TimeSpan.FromSeconds(30)))
            {
                return new TrinityPower(SNOPower.Wizard_Archon_SlowTime, 0f, Player.Position);
            }

            // Archon Teleport in combat for kiting
            if (!Player.IsIncapacitated && CanCast(SNOPower.Wizard_Archon_Teleport, CanCastFlags.NoTimer) &&
                Settings.Combat.Wizard.KiteLimit > 0 &&
                // Try and teleport-retreat from 1 elite or 3+ greys or a boss at 15 foot range
                (TargetUtil.AnyElitesInRange(15, 1) || TargetUtil.AnyMobsInRange(15, 3) || (CurrentTarget.IsBoss && CurrentTarget.RadiusDistance <= 15f)))
            {
                Vector3 vNewTarget = MathEx.CalculatePointFrom(CurrentTarget.Position, Player.Position, -20f);
                return new TrinityPower(SNOPower.Wizard_Archon_Teleport, 35f, vNewTarget);
            }

            // Archon teleport in combat for no-kite
            if (!Player.IsIncapacitated && CanCast(SNOPower.Wizard_Archon_Teleport, CanCastFlags.NoTimer) &&
                Settings.Combat.Wizard.KiteLimit == 0 && CurrentTarget.RadiusDistance >= 10f)
            {
                return new TrinityPower(SNOPower.Wizard_Archon_Teleport, 35f, CurrentTarget.Position);
            }

            // 2.0.5 Archon elemental runes
            // This needs some checking on range i think

            //392694, 392695, 392696 == Arcane Strike,
            //392697, 392699, 392698 == Disintegration Wave
            //392692, 392693, 392691 == Arcane Blast, Ice Blast 

            SNOPower
                beamPower = SNOPower.Wizard_Archon_ArcaneBlast,
                strikePower = SNOPower.Wizard_Archon_ArcaneStrike,
                blastPower = SNOPower.Wizard_Archon_DisintegrationWave;

            var beamSkill = CacheData.Hotbar.ActiveSkills
                .FirstOrDefault(p => p.Power == SNOPower.Wizard_Archon_DisintegrationWave ||
                    p.Power == SNOPower.Wizard_Archon_DisintegrationWave_Cold ||
                    p.Power == SNOPower.Wizard_Archon_DisintegrationWave_Fire ||
                    p.Power == SNOPower.Wizard_Archon_DisintegrationWave_Lightning);

            var strikeSkill = CacheData.Hotbar.ActiveSkills
                .FirstOrDefault(p => p.Power == SNOPower.Wizard_Archon_ArcaneStrike ||
                    p.Power == SNOPower.Wizard_Archon_ArcaneStrike_Fire ||
                    p.Power == SNOPower.Wizard_Archon_ArcaneStrike_Cold ||
                    p.Power == SNOPower.Wizard_Archon_ArcaneStrike_Lightning);

            var blastSkill = CacheData.Hotbar.ActiveSkills
                .FirstOrDefault(p => p.Power == SNOPower.Wizard_Archon_ArcaneBlast ||
                    p.Power == SNOPower.Wizard_Archon_ArcaneBlast_Cold ||
                    p.Power == SNOPower.Wizard_Archon_ArcaneBlast_Fire ||
                    p.Power == SNOPower.Wizard_Archon_ArcaneBlast_Lightning);

            if (beamSkill != null && beamSkill.Power != default(SNOPower))
                beamPower = beamSkill.Power;

            if (strikeSkill != null && strikeSkill.Power != default(SNOPower))
                strikePower = strikeSkill.Power;

            if (blastSkill != null && blastSkill.Power != default(SNOPower))
                blastPower = blastSkill.Power;

            // Arcane Blast - 2 second cooldown, big AoE
            if (!Player.IsIncapacitated && CanCast(blastPower, CanCastFlags.NoTimer))
            {
                return new TrinityPower(blastPower, 10f, CurrentTarget.Position);
            }

            // Disintegrate
            if (!Player.IsIncapacitated && !Settings.Combat.Wizard.DisableDisintegrationWave && CanCast(beamPower, CanCastFlags.NoTimer) &&
                (CurrentTarget.CountUnitsBehind(25f) > 2 || Settings.Combat.Wizard.NoArcaneStrike || Settings.Combat.Wizard.KiteLimit > 0))
            {
                return new TrinityPower(beamPower, 49f, CurrentTarget.ACDGuid);
            }

            // Arcane Strike Rapid Spam at close-range only, and no AoE inbetween us and target
            if (!Player.IsIncapacitated && !Settings.Combat.Wizard.NoArcaneStrike && CanCast(strikePower, CanCastFlags.NoTimer) &&
                !CacheData.TimeBoundAvoidance.Any(aoe => MathUtil.IntersectsPath(aoe.Position, aoe.Radius, Player.Position, CurrentTarget.Position)))
            {
                return new TrinityPower(strikePower, 7f, CurrentTarget.ACDGuid);
            }

            // Disintegrate as final option just in case
            if (!Player.IsIncapacitated && CanCast(beamPower, CanCastFlags.NoTimer))
            {
                return new TrinityPower(beamPower, 49f, CurrentTarget.ACDGuid);
            }

            return null;
        }

        /// <summary>
        /// Gets the best Wizard object destruction power
        /// </summary>
        private static TrinityPower DestroyObjectPower()
        {
            if (CanCast(SNOPower.Wizard_WaveOfForce) && Player.PrimaryResource >= 25)
                return new TrinityPower(SNOPower.Wizard_WaveOfForce, 9f);

            if (CanCast(SNOPower.Wizard_EnergyTwister) && Player.PrimaryResource >= 35)
                return new TrinityPower(SNOPower.Wizard_EnergyTwister, 9f);

            if (CanCast(SNOPower.Wizard_ArcaneOrb))
                return new TrinityPower(SNOPower.Wizard_ArcaneOrb, 35f);

            if (CanCast(SNOPower.Wizard_MagicMissile))
                return new TrinityPower(SNOPower.Wizard_MagicMissile, 15f);

            if (CanCast(SNOPower.Wizard_ShockPulse))
                return new TrinityPower(SNOPower.Wizard_ShockPulse, 10f);

            if (CanCast(SNOPower.Wizard_SpectralBlade))
                return new TrinityPower(SNOPower.Wizard_SpectralBlade, 5f);

            if (CanCast(SNOPower.Wizard_Electrocute))
                return new TrinityPower(SNOPower.Wizard_Electrocute, 9f);

            if (CanCast(SNOPower.Wizard_ArcaneTorrent))
                return new TrinityPower(SNOPower.Wizard_ArcaneTorrent, 9f);

            if (CanCast(SNOPower.Wizard_Blizzard))
                return new TrinityPower(SNOPower.Wizard_Blizzard, 9f);
            return DefaultPower;
        }

        /// <summary>
        /// Checks for all necessary buffs and combat conditions for starting Archon
        /// </summary>
        /// <returns></returns>
        private static bool ShouldStartArchon()
        {
            bool canCastArchon = (
                 CheckAbilityAndBuff(SNOPower.Wizard_MagicWeapon) &&
                 (!Hotbar.Contains(SNOPower.Wizard_Familiar) || IsFamiliarActive) &&
                 CheckAbilityAndBuff(SNOPower.Wizard_EnergyArmor) &&
                 CheckAbilityAndBuff(SNOPower.Wizard_IceArmor) &&
                 CheckAbilityAndBuff(SNOPower.Wizard_StormArmor)
             );

            bool elitesOnly = Settings.Combat.Wizard.ArchonElitesOnly && TargetUtil.AnyElitesInRange(Settings.Combat.Wizard.ArchonEliteDistance);
            bool trashInRange = !Settings.Combat.Wizard.ArchonElitesOnly && TargetUtil.AnyMobsInRange(Settings.Combat.Wizard.ArchonMobDistance, Settings.Combat.Wizard.ArchonMobCount);

            return canCastArchon && (elitesOnly || trashInRange);
        }

        /// <summary>
        /// Returns true if we have wizard armor in our hotbar and if the buff is active
        /// </summary>
        private static bool IsWizardArmorActive
        {
            get { return (GetHasBuff(SNOPower.Wizard_EnergyArmor) || GetHasBuff(SNOPower.Wizard_IceArmor) || GetHasBuff(SNOPower.Wizard_StormArmor)); }
        }

        /// <summary>
        /// Returns true if Familiar buff is active
        /// </summary>
        private static bool IsFamiliarActive
        {
            get
            {
                double timeSinceDeath = DateTime.UtcNow.Subtract(Trinity.LastDeathTime).TotalMilliseconds;

                // We've died, no longer have familiar
                if (timeSinceDeath < TimeSincePowerUse(SNOPower.Wizard_Familiar))
                    return false;

                // we've used it within the last 5 minutes, we should still have it
                if (TimeSincePowerUse(SNOPower.Wizard_Familiar) < (5 * 60 * 1000))
                    return true;

                return false;
            }
        }

        internal static bool HasPrimarySkill
        {
            get
            {
                return Hotbar.Contains(SNOPower.Wizard_MagicMissile) ||
                    Hotbar.Contains(SNOPower.Wizard_ShockPulse) ||
                    Hotbar.Contains(SNOPower.Wizard_SpectralBlade) ||
                    Hotbar.Contains(SNOPower.Wizard_Electrocute);
            }
        }

    }
}
