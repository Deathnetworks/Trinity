using System;
using Trinity.Config.Combat;
using Trinity.Reference;
using Zeta.Common;
using Zeta.Game;
using Zeta.Game.Internals.Actors;
using System.Linq;
using Logger = Trinity.Technicals.Logger;

namespace Trinity.Combat.Abilities
{
    public class DemonHunterCombat : CombatBase
    {
        // ReSharper disable once InconsistentNaming
        public static DemonHunterSetting DHSettings
        {
            get { return Trinity.Settings.Combat.DemonHunter; }
        }

        public static TrinityPower GetPower()
        {
            if (UseDestructiblePower)
                return GetDemonHunterDestroyPower();

            // Buffs
            if ((Player.IsInCombat || UseOOCBuff) && !IsCurrentlyAvoiding)
            {
                var power = GetBuffPower();
                if (power != null && power.SNOPower != SNOPower.None)
                    return power;
            }

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
            // Smoke Screen
            if (CanCast(SNOPower.DemonHunter_SmokeScreen, CanCastFlags.NoTimer) &&
                !GetHasBuff(SNOPower.DemonHunter_ShadowPower) && Player.SecondaryResource >= 14 &&
                (Player.CurrentHealthPct <= 0.50 || Player.IsRooted || Player.IsIncapacitated))
            {
                return new TrinityPower(SNOPower.DemonHunter_SmokeScreen);
            }

            return null;
        }
        /// <summary>
        /// Gets the best combat power for the current conditions
        /// </summary>
        /// <returns></returns>
        private static TrinityPower GetCombatPower()
        {
            int MinEnergyReserve = 25;

            if (Sets.EmbodimentOfTheMarauder.IsFullyEquipped)
                MinEnergyReserve = 70;

            if (Player.PrimaryResource < MinEnergyReserve)
            {
                Player.WaitingForReserveEnergy = IsWaitingForSpecial = true;
            }
            else
            {
                Player.WaitingForReserveEnergy = IsWaitingForSpecial = false;
            }

            // NotSpam Shadow Power
            if (!Settings.Combat.DemonHunter.SpamShadowPower && CanCast(SNOPower.DemonHunter_ShadowPower) && !Player.IsIncapacitated &&
                (!GetHasBuff(SNOPower.DemonHunter_ShadowPower) || Player.CurrentHealthPct <= Trinity.PlayerEmergencyHealthPotionLimit) && // if we don't have the buff or our health is low
                (Player.CurrentHealthPct < 1f || Player.IsRooted || TargetUtil.AnyMobsInRange(15)))
            {
                return new TrinityPower(SNOPower.DemonHunter_ShadowPower);
            }


            // Sentry Turret
            if (!Player.IsIncapacitated && CanCast(SNOPower.DemonHunter_Sentry, CanCastFlags.NoTimer) &&
               TargetUtil.AnyMobsInRange(65) && Player.PrimaryResource >= 30)
            {
                return new TrinityPower(SNOPower.DemonHunter_Sentry, 75f, TargetUtil.GetBestClusterPoint(35f, 75f, false));
            }

            // Caltrops
            if (!Player.IsIncapacitated && CanCast(SNOPower.DemonHunter_Caltrops) &&
                Player.SecondaryResource >= 6 && TargetUtil.AnyMobsInRange(40) && !GetHasBuff(SNOPower.DemonHunter_Caltrops))
            {
                return new TrinityPower(SNOPower.DemonHunter_Caltrops);
            }

            // Companion
            if (!Player.IsIncapacitated && Hotbar.Contains(SNOPower.X1_DemonHunter_Companion))
            {
                // Use Spider Slow on 4 or more trash mobs in an area or on Unique/Elite/Champion
                if (Runes.DemonHunter.SpiderCompanion.IsActive && CanCast(SNOPower.X1_DemonHunter_Companion) && TargetUtil.ClusterExists(25f, 4) && TargetUtil.EliteOrTrashInRange(25f))
                {
                    return new TrinityPower(SNOPower.X1_DemonHunter_Companion);
                }

                //Use Bat when Hatred is Needed
                if (Runes.DemonHunter.BatCompanion.IsActive && CanCast(SNOPower.X1_DemonHunter_Companion) && Player.PrimaryResourceMissing >= 60)
                {
                    return new TrinityPower(SNOPower.X1_DemonHunter_Companion);
                }

                // Use Boar Taunt on 3 or more trash mobs in an area or on Unique/Elite/Champion
                if (Runes.DemonHunter.BoarCompanion.IsActive && CanCast(SNOPower.X1_DemonHunter_Companion) && ((TargetUtil.ClusterExists(20f, 4) && TargetUtil.EliteOrTrashInRange(20f)) ||
                    (CurrentTarget.IsBossOrEliteRareUnique && CurrentTarget.Distance <= 20f)))
                {
                    return new TrinityPower(SNOPower.X1_DemonHunter_Companion);
                }

                // Ferrets used for picking up Health Globes when low on Health
                if (Runes.DemonHunter.FerretCompanion.IsActive && Trinity.ObjectCache.Any(o => o.Type == GObjectType.HealthGlobe && o.Distance < 60f) && Player.CurrentHealthPct < Trinity.PlayerEmergencyHealthPotionLimit)
                {
                    return new TrinityPower(SNOPower.X1_DemonHunter_Companion);
                }

                // Use Wolf Howl on Unique/Elite/Champion - Would help for farming trash, but trash farming should not need this - Used on Elites to reduce Deaths per hour
                if (Runes.DemonHunter.WolfCompanion.IsActive && CanCast(SNOPower.X1_DemonHunter_Companion, CanCastFlags.NoTimer) &&
                    ((CurrentTarget.IsBossOrEliteRareUnique || TargetUtil.AnyMobsInRange(40, 10)) && CurrentTarget.RadiusDistance < 25f))
                {
                    return new TrinityPower(SNOPower.X1_DemonHunter_Companion);
                }
            }

            // Companion active attack on elite
            if (CanCast(SNOPower.X1_DemonHunter_Companion) && CurrentTarget.IsEliteRareUnique &&
                Player.SecondaryResource >= 10)
            {
                return new TrinityPower(SNOPower.X1_DemonHunter_Companion);
            }

            // Smoke Screen
            if (CanCast(SNOPower.DemonHunter_SmokeScreen, CanCastFlags.NoTimer) &&
                !GetHasBuff(SNOPower.DemonHunter_ShadowPower) && Player.SecondaryResource >= 14 &&
                (Player.CurrentHealthPct <= 0.50 || Player.IsRooted || TargetUtil.AnyMobsInRange(15) ||
                (Legendary.MeticulousBolts.IsEquipped && TargetUtil.AnyMobsInRange(60)) || Player.IsIncapacitated))
            {
                return new TrinityPower(SNOPower.DemonHunter_SmokeScreen);
            }

            // Preperation
            bool hasBattleScars = Runes.DemonHunter.BattleScars.IsActive;

            float preperationTriggerRange = V.F("DemonHunter.PreperationTriggerRange");
            if (((!Player.IsIncapacitated &&
                (TargetUtil.AnyMobsInRange(preperationTriggerRange))) || Settings.Combat.DemonHunter.SpamPreparation || Runes.DemonHunter.Punishment.IsActive) &&
                Hotbar.Contains(SNOPower.DemonHunter_Preparation))
            {
                // Preperation w/ Punishment
                if (Runes.DemonHunter.Punishment.IsActive && CanCast(SNOPower.DemonHunter_Preparation, CanCastFlags.NoTimer) &&
                    Player.PrimaryResourceMissing >= 75 && TimeSincePowerUse(SNOPower.DemonHunter_Preparation) >= 1000)
                {
                    return new TrinityPower(SNOPower.DemonHunter_Preparation);
                }

                // Preperation w/ Battle Scars - check for health only
                if (hasBattleScars && CanCast(SNOPower.DemonHunter_Preparation) && Player.CurrentHealthPct < 0.6)
                {
                    return new TrinityPower(SNOPower.DemonHunter_Preparation);
                }

                // no rune || invigoration || focused mind || Backup Plan || Battle Scars (need Disc)
                if ((!Runes.DemonHunter.Punishment.IsActive) && CanCast(SNOPower.DemonHunter_Preparation, CanCastFlags.NoTimer) &&
                    Player.SecondaryResource <= 15 && TimeSincePowerUse(SNOPower.DemonHunter_Preparation) >= 1000)
                {
                    return new TrinityPower(SNOPower.DemonHunter_Preparation);
                }
            }

            // Marked for Death
            if (CanCast(SNOPower.DemonHunter_MarkedForDeath, CanCastFlags.NoTimer) &&
                Player.SecondaryResource >= (Hotbar.Contains(SNOPower.DemonHunter_SmokeScreen) ? 17 : 3) &&
                !CurrentTarget.HasDebuff(SNOPower.DemonHunter_MarkedForDeath) &&
                !SpellTracker.IsUnitTracked(CurrentTarget, SNOPower.DemonHunter_MarkedForDeath))
            {
                return new TrinityPower(SNOPower.DemonHunter_MarkedForDeath, 40f, CurrentTarget.ACDGuid);
            }

            // Vault
            if (CanCast(SNOPower.DemonHunter_Vault) && !Player.IsRooted && !Player.IsIncapacitated &&
                Settings.Combat.DemonHunter.VaultMode != DemonHunterVaultMode.MovementOnly &&
                (TargetUtil.AnyMobsInRange(7f, 6) || Player.CurrentHealthPct <= 0.7) &&
                // if we have ShadowPower and Disicpline is >= 16
                // or if we don't have ShadoWpower and Discipline is >= 22
                (Player.SecondaryResource >= (Hotbar.Contains(SNOPower.DemonHunter_ShadowPower) ? 22 : 16)) &&
                    TimeSincePowerUse(SNOPower.DemonHunter_Vault) >= Settings.Combat.DemonHunter.VaultMovementDelay)
            {
                Vector3 vNewTarget = NavHelper.MainFindSafeZone(Player.Position, true);

                return new TrinityPower(SNOPower.DemonHunter_Vault, 20f, vNewTarget);
            }

            // Rain of Vengeance
            if (CanCast(SNOPower.DemonHunter_RainOfVengeance) && !Player.IsIncapacitated &&
                (TargetUtil.ClusterExists(45f, 3) || TargetUtil.EliteOrTrashInRange(45f)))
            {
                var bestClusterPoint = TargetUtil.GetBestClusterPoint(45f, 65f, false);

                return new TrinityPower(SNOPower.DemonHunter_RainOfVengeance, 0f, bestClusterPoint);
            }

            // Cluster Arrow
            if (CanCast(SNOPower.DemonHunter_ClusterArrow) && !Player.IsIncapacitated &&
                ((Player.PrimaryResource >= 50 && !IsWaitingForSpecial) || Player.PrimaryResource > MinEnergyReserve))
            {
                return new TrinityPower(SNOPower.DemonHunter_ClusterArrow, V.F("DemonHunter.ClusterArrow.UseRange"), CurrentTarget.ACDGuid);
            }

            // Multi Shot
            if (CanCast(SNOPower.DemonHunter_Multishot) && !Player.IsIncapacitated &&
                ((Player.PrimaryResource >= 30 && !IsWaitingForSpecial) || Player.PrimaryResource > MinEnergyReserve) &&
                (TargetUtil.AnyMobsInRange(40, 2) || CurrentTarget.IsBossOrEliteRareUnique || CurrentTarget.IsTreasureGoblin))
            {
                return new TrinityPower(SNOPower.DemonHunter_Multishot, 30f, CurrentTarget.Position);
            }

            // Fan of Knives
            if (CanCast(SNOPower.DemonHunter_FanOfKnives) && !Player.IsIncapacitated &&
                (TargetUtil.EliteOrTrashInRange(15) || TargetUtil.AnyTrashInRange(15f, 5, false)))
            {
                return new TrinityPower(SNOPower.DemonHunter_FanOfKnives);
            }

            // Strafe spam - similar to barbarian whirlwind routine
            if (CanCast(SNOPower.DemonHunter_Strafe, CanCastFlags.NoTimer) &&
                !Player.IsIncapacitated && !Player.IsRooted && Player.PrimaryResource >= Settings.Combat.DemonHunter.StrafeMinHatred)
            {
                bool shouldGetNewZigZag =
                    (DateTime.UtcNow.Subtract(LastChangedZigZag).TotalMilliseconds >= V.I("Barbarian.Whirlwind.ZigZagMaxTime") ||
                    CurrentTarget.ACDGuid != LastZigZagUnitAcdGuid ||
                    ZigZagPosition.Distance2D(Player.Position) <= 5f);

                if (shouldGetNewZigZag)
                {
                    var wwdist = V.F("Barbarian.Whirlwind.ZigZagDistance");

                    ZigZagPosition = TargetUtil.GetZigZagTarget(CurrentTarget.Position, wwdist);

                    LastZigZagUnitAcdGuid = CurrentTarget.ACDGuid;
                    LastChangedZigZag = DateTime.UtcNow;
                }

                int postCastTickDelay = TrinityPower.MillisecondsToTickDelay(250);

                return new TrinityPower(SNOPower.DemonHunter_Strafe, 15f, ZigZagPosition, Trinity.Player.WorldDynamicID, -1, 0, postCastTickDelay);
            }

            // Spike Trap
            if (!Player.IsIncapacitated && CanCast(SNOPower.DemonHunter_SpikeTrap) &&
                LastPowerUsed != SNOPower.DemonHunter_SpikeTrap && Player.PrimaryResource >= 30)
            {
                // For distant monsters, try to target a little bit in-front of them (as they run towards us), if it's not a treasure goblin
                float reducedDistance = 0f;
                if (CurrentTarget.Distance > 17f && !CurrentTarget.IsTreasureGoblin)
                {
                    reducedDistance = CurrentTarget.Distance - 17f;
                    if (reducedDistance > 5f)
                        reducedDistance = 5f;
                }
                Vector3 vNewTarget = MathEx.CalculatePointFrom(CurrentTarget.Position, Player.Position, CurrentTarget.Distance - reducedDistance);
                return new TrinityPower(SNOPower.DemonHunter_SpikeTrap, 35f, vNewTarget, Trinity.Player.WorldDynamicID, -1, 1, 1);
            }

            // Elemental Arrow
            if (CanCast(SNOPower.DemonHunter_ElementalArrow) && !Player.IsIncapacitated &&
                ((Player.PrimaryResource >= 10 && !IsWaitingForSpecial) || Player.PrimaryResource >= MinEnergyReserve || Legendary.Kridershot.IsEquipped))
            {
                return new TrinityPower(SNOPower.DemonHunter_ElementalArrow, 65f, CurrentTarget.ACDGuid);
            }

            // Chakram normal attack
            if (Hotbar.Contains(SNOPower.DemonHunter_Chakram) && !Player.IsIncapacitated &&
                !Runes.DemonHunter.ShurikenCloud.IsActive &&
                ((Player.PrimaryResource >= 10 && !IsWaitingForSpecial) || Player.PrimaryResource >= MinEnergyReserve))
            {
                return new TrinityPower(SNOPower.DemonHunter_Chakram, 50f, CurrentTarget.ACDGuid);
            }

            // Rapid Fire
            if (CanCast(SNOPower.DemonHunter_RapidFire, CanCastFlags.NoTimer) &&
                !Player.IsIncapacitated && ((Player.PrimaryResource >= 16 && !IsWaitingForSpecial) || (Player.PrimaryResource > MinEnergyReserve)) &&
                (Player.PrimaryResource >= Settings.Combat.DemonHunter.RapidFireMinHatred || LastPowerUsed == SNOPower.DemonHunter_RapidFire))
            {
                // Players with grenades *AND* rapid fire should spam grenades at close-range instead
                if (CanCast(SNOPower.DemonHunter_Grenades) && CurrentTarget.RadiusDistance <= 18f)
                {
                    return new TrinityPower(SNOPower.DemonHunter_Grenades, 18f, CurrentTarget.ACDGuid);
                }
                // Now return rapid fire, if not sending grenades instead
                return new TrinityPower(SNOPower.DemonHunter_RapidFire, 40f, CurrentTarget.Position);
            }

            // Impale
            if (CanCast(SNOPower.DemonHunter_Impale) && !TargetUtil.AnyMobsInRange(12, 4) &&
                ((Player.PrimaryResource >= 25 && !IsWaitingForSpecial) || Player.PrimaryResource >= MinEnergyReserve) &&
                CurrentTarget.RadiusDistance <= 75f)
            {
                return new TrinityPower(SNOPower.DemonHunter_Impale, 75f, CurrentTarget.ACDGuid);
            }

            // Evasive Fire
            if (CanCast(SNOPower.X1_DemonHunter_EvasiveFire) && !Player.IsIncapacitated &&
                  (TargetUtil.AnyMobsInRange(10f) || DemonHunter_HasNoPrimary()))
            {
                float range = DemonHunter_HasNoPrimary() ? 70f : 0f;

                return new TrinityPower(SNOPower.X1_DemonHunter_EvasiveFire, range, CurrentTarget.ACDGuid);
            }

            // Spines of Seething Hatred, grants 4 hatred
            if (Legendary.SpinesOfSeethingHatred.IsEquipped && CanCast(SNOPower.DemonHunter_Chakram, CanCastFlags.NoTimer))
            {
                return new TrinityPower(SNOPower.DemonHunter_Chakram, 50f, CurrentTarget.ACDGuid);
            }

            // Hungering Arrow
            if (Hotbar.Contains(SNOPower.DemonHunter_HungeringArrow) && !Player.IsIncapacitated)
            {
                return new TrinityPower(SNOPower.DemonHunter_HungeringArrow, 50f, CurrentTarget.ACDGuid);
            }

            // Entangling shot
            if (Hotbar.Contains(SNOPower.X1_DemonHunter_EntanglingShot) && !Player.IsIncapacitated)
            {
                return new TrinityPower(SNOPower.X1_DemonHunter_EntanglingShot, 50f, CurrentTarget.ACDGuid);
            }

            // Bola Shot
            if (Hotbar.Contains(SNOPower.DemonHunter_Bolas) && !Player.IsIncapacitated)
            {
                return new TrinityPower(SNOPower.DemonHunter_Bolas, 50f, CurrentTarget.ACDGuid);
            }

            // Grenades
            if (Hotbar.Contains(SNOPower.DemonHunter_Grenades) && !Player.IsIncapacitated)
            {
                return new TrinityPower(SNOPower.DemonHunter_Grenades, 40f, CurrentTarget.ACDGuid);
            }

            //Hexing Pants Mod
            if (CurrentTarget != null)
            {
                if (Legendary.HexingPantsOfMrYan.IsEquipped && CurrentTarget.IsUnit && CurrentTarget.RadiusDistance > 10f)
                {
                    return new TrinityPower(SNOPower.Walk, 10f, CurrentTarget.Position);
                }

                if (Legendary.HexingPantsOfMrYan.IsEquipped && CurrentTarget.IsUnit && CurrentTarget.RadiusDistance < 10f)
                {
                    Vector3 vNewTarget = MathEx.CalculatePointFrom(CurrentTarget.Position, Player.Position, -10f);
                    return new TrinityPower(SNOPower.Walk, 10f, vNewTarget);
                }
            }

            return DefaultPower;
        }
        /// <summary>
        /// Checks and casts buffs if needed
        /// </summary>
        /// <returns></returns>
        private static TrinityPower GetBuffPower()
        {
            // Vengeance
            if (CanCast(SNOPower.X1_DemonHunter_Vengeance, CanCastFlags.NoTimer) &&
                ((!Settings.Combat.DemonHunter.VengeanceElitesOnly && TargetUtil.AnyMobsInRange(60, 6)) || TargetUtil.IsEliteTargetInRange(80f)))
            {
                return new TrinityPower(SNOPower.X1_DemonHunter_Vengeance);
            }

            // Spam Shadow Power
            if (Settings.Combat.DemonHunter.SpamShadowPower && CanCast(SNOPower.DemonHunter_ShadowPower) && !Player.IsIncapacitated &&
                (!GetHasBuff(SNOPower.DemonHunter_ShadowPower) || Player.CurrentHealthPct <= Trinity.PlayerEmergencyHealthPotionLimit) && // if we don't have the buff or our health is low
                ((!Runes.DemonHunter.Punishment.IsActive && Player.SecondaryResource >= 14) || (Runes.DemonHunter.Punishment.IsActive && Player.SecondaryResource >= 39)) && // Save some Discipline for Preparation
                (Settings.Combat.DemonHunter.SpamShadowPower && Player.SecondaryResource >= 28)) // When spamming Shadow Power, save some Discipline for emergencies
            {
                return new TrinityPower(SNOPower.DemonHunter_ShadowPower);
            }

            // Smoke Screen spam
            if (Settings.Combat.DemonHunter.SpamSmokeScreen && CanCast(SNOPower.DemonHunter_SmokeScreen) &&
                !GetHasBuff(SNOPower.DemonHunter_ShadowPower) && Player.SecondaryResource >= 14)
            {
                return new TrinityPower(SNOPower.DemonHunter_SmokeScreen);
            }

            // Chakram:Shuriken Cloud
            if (!Player.IsInTown && Hotbar.Contains(SNOPower.DemonHunter_Chakram) && !Player.IsIncapacitated &&
                Runes.DemonHunter.ShurikenCloud.IsActive && TimeSincePowerUse(SNOPower.DemonHunter_Chakram) >= 110000 &&
                ((Player.PrimaryResource >= 10 && !Player.WaitingForReserveEnergy) || Player.PrimaryResource >= MinEnergyReserve))
            {
                return new TrinityPower(SNOPower.DemonHunter_Chakram);
            }

            return null;
        }

        private static bool DemonHunter_HasNoPrimary()
        {
            return !(Hotbar.Contains(SNOPower.DemonHunter_Bolas) ||
                                Hotbar.Contains(SNOPower.X1_DemonHunter_EntanglingShot) ||
                                Hotbar.Contains(SNOPower.DemonHunter_Grenades) ||
                                Hotbar.Contains(SNOPower.DemonHunter_HungeringArrow));
        }
        private static TrinityPower GetDemonHunterDestroyPower()
        {
            if (Hotbar.Contains(SNOPower.DemonHunter_HungeringArrow))
                return new TrinityPower(SNOPower.DemonHunter_HungeringArrow, 10f, CurrentTarget.ACDGuid);

            if (Hotbar.Contains(SNOPower.X1_DemonHunter_EntanglingShot))
                return new TrinityPower(SNOPower.X1_DemonHunter_EntanglingShot, 10f, CurrentTarget.ACDGuid);

            if (Hotbar.Contains(SNOPower.DemonHunter_Bolas))
                return new TrinityPower(SNOPower.DemonHunter_Bolas, 10f, CurrentTarget.ACDGuid);

            if (Hotbar.Contains(SNOPower.DemonHunter_Grenades))
                return new TrinityPower(SNOPower.DemonHunter_Grenades, 10f, CurrentTarget.ACDGuid);

            if (Hotbar.Contains(SNOPower.DemonHunter_ElementalArrow) && Player.PrimaryResource >= 10)
                return new TrinityPower(SNOPower.DemonHunter_ElementalArrow, 10f, CurrentTarget.ACDGuid);

            if (Hotbar.Contains(SNOPower.X1_DemonHunter_EvasiveFire) && Player.PrimaryResource >= 10)
                return new TrinityPower(SNOPower.X1_DemonHunter_EvasiveFire, 10f, CurrentTarget.ACDGuid);

            if (Hotbar.Contains(SNOPower.DemonHunter_RapidFire) && Player.PrimaryResource >= 10)
                return new TrinityPower(SNOPower.DemonHunter_RapidFire, 10f, CurrentTarget.Position);

            if (Hotbar.Contains(SNOPower.DemonHunter_Chakram) && Player.PrimaryResource >= 20)
                return new TrinityPower(SNOPower.DemonHunter_Chakram, 0f, CurrentTarget.ACDGuid);

            return DefaultPower;
        }
    }
}
