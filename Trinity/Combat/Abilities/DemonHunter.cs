using System;
using System.Linq;
using Trinity.Combat;
using Trinity.Combat.Abilities;
using Trinity.Reference;
using Zeta.Common;
using Zeta.Common.Plugins;
using Zeta.Game;
using Zeta.Game.Internals.Actors;


namespace Trinity
{
    public partial class Trinity
    {
        private static TrinityPower GetDemonHunterPower(bool IsCurrentlyAvoiding, bool UseOOCBuff, bool UseDestructiblePower)
        {
            // Pick the best destructible power available
            if (UseDestructiblePower)
            {
                return GetDemonHunterDestroyPower();
            }

            MinEnergyReserve = 25;

            //Kridershot InternalName=x1_bow_norm_unique_09-137 GameBalanceID=1999595351 ItemLink: {c:ffff8000}Kridershot{/c}
            bool hasKridershot = Legendary.Kridershot.IsEquipped;

            bool hasPunishment = Runes.DemonHunter.Punishment.IsActive;

            // Spam Shadow Power
            if (Settings.Combat.DemonHunter.SpamShadowPower && CombatBase.CanCast(SNOPower.DemonHunter_ShadowPower) && !Player.IsIncapacitated &&
                (!GetHasBuff(SNOPower.DemonHunter_ShadowPower) || Player.CurrentHealthPct <= PlayerEmergencyHealthPotionLimit) && // if we don't have the buff or our health is low
                ((!hasPunishment && Player.SecondaryResource >= 14) || (hasPunishment && Player.SecondaryResource >= 39)) && // Save some Discipline for Preparation
                (Settings.Combat.DemonHunter.SpamShadowPower && Player.SecondaryResource >= 28)) // When spamming Shadow Power, save some Discipline for emergencies
            {
                return new TrinityPower(SNOPower.DemonHunter_ShadowPower);
            }

            // NotSpam Shadow Power
            if (!UseOOCBuff && !Settings.Combat.DemonHunter.SpamShadowPower && CombatBase.CanCast(SNOPower.DemonHunter_ShadowPower) && !Player.IsIncapacitated &&
                (!GetHasBuff(SNOPower.DemonHunter_ShadowPower) || Player.CurrentHealthPct <= PlayerEmergencyHealthPotionLimit) && // if we don't have the buff or our health is low
                ((!hasPunishment && Player.SecondaryResource >= 14) || (hasPunishment && Player.SecondaryResource >= 39)) && // Save some Discipline for Preparation
                (Player.CurrentHealthPct < 1f || Player.IsRooted || TargetUtil.AnyMobsInRange(15)))
            {
                return new TrinityPower(SNOPower.DemonHunter_ShadowPower);
            }

            // Vengeance
            if (!IsCurrentlyAvoiding && CombatBase.CanCast(SNOPower.X1_DemonHunter_Vengeance, CombatBase.CanCastFlags.NoTimer) &&
                ((!Settings.Combat.DemonHunter.VengeanceElitesOnly && TargetUtil.AnyMobsInRange(60, 6)) || TargetUtil.IsEliteTargetInRange(60f)))
            {
                return new TrinityPower(SNOPower.X1_DemonHunter_Vengeance);
            }

            // Smoke Screen
            if ((!UseOOCBuff || Settings.Combat.DemonHunter.SpamSmokeScreen) && CombatBase.CanCast(SNOPower.DemonHunter_SmokeScreen) &&
                !GetHasBuff(SNOPower.DemonHunter_ShadowPower) && Player.SecondaryResource >= 14 &&
                (
                 (Player.CurrentHealthPct <= 0.50 || Player.IsRooted || TargetUtil.AnyMobsInRange(15) || Player.IsIncapacitated) ||
                 Settings.Combat.DemonHunter.SpamSmokeScreen
                ))
            {
                return new TrinityPower(SNOPower.DemonHunter_SmokeScreen, 0f, Vector3.Zero, CurrentWorldDynamicId, -1, 1, 1);
            }

            // Sentry Turret
            if (!UseOOCBuff && !Player.IsIncapacitated && CombatBase.CanCast(SNOPower.DemonHunter_Sentry, CombatBase.CanCastFlags.NoTimer) &&
               TargetUtil.AnyMobsInRange(65) &&
               Player.PrimaryResource >= 30)
            {
                return new TrinityPower(SNOPower.DemonHunter_Sentry, 65f, TargetUtil.GetBestClusterPoint(35f, 65f, false));
            }

            // Caltrops
            if (!UseOOCBuff && !Player.IsIncapacitated && CombatBase.CanCast(SNOPower.DemonHunter_Caltrops) &&
                Player.SecondaryResource >= 6 && TargetUtil.AnyMobsInRange(40))
            {
                return new TrinityPower(SNOPower.DemonHunter_Caltrops, 0f, Vector3.Zero, CurrentWorldDynamicId, -1, 1, 1);
            }

            bool hasBattleScars = Runes.DemonHunter.BattleScars.IsActive;

            float preperationTriggerRange = V.F("DemonHunter.PreperationTriggerRange");
            if (((!UseOOCBuff && !Player.IsIncapacitated &&
                (TargetUtil.AnyMobsInRange(preperationTriggerRange))) || Settings.Combat.DemonHunter.SpamPreparation || hasPunishment) &&
                Hotbar.Contains(SNOPower.DemonHunter_Preparation))
            {
                // Preperation w/ Punishment
                if (hasPunishment && CombatBase.CanCast(SNOPower.DemonHunter_Preparation, CombatBase.CanCastFlags.NoTimer) &&
                    Player.SecondaryResource >= 25 && Player.PrimaryResourceMissing >= 75 && TimeSinceUse(SNOPower.DemonHunter_Preparation) >= 1000)
                {
                    return new TrinityPower(SNOPower.DemonHunter_Preparation);
                }

                // Preperation w/ Battle Scars - check for health only
                if (hasBattleScars && CombatBase.CanCast(SNOPower.DemonHunter_Preparation) && Player.CurrentHealthPct < 0.6)
                {
                    return new TrinityPower(SNOPower.DemonHunter_Preparation);
                }

                // no rune || invigoration || focused mind || Backup Plan || Battle Scars (need Disc)
                if ((!hasPunishment) && CombatBase.CanCast(SNOPower.DemonHunter_Preparation) && Player.SecondaryResource <= 15 && TimeSinceUse(SNOPower.DemonHunter_Preparation) >= 1000)
                {
                    return new TrinityPower(SNOPower.DemonHunter_Preparation);
                }
            }


            // SOURCE: Stede - http://www.thebuddyforum.com/demonbuddy-forum/plugins/trinity/155398-trinity-dh-companion-use-abilities.html#post1445496

            // Companion On-Use Abilities Added in 2.0
            bool hasSpider = HotbarSkills.AssignedSkills.Any(s => s.Power == SNOPower.X1_DemonHunter_Companion && s.RuneIndex == 0);
            bool hasBat = HotbarSkills.AssignedSkills.Any(s => s.Power == SNOPower.X1_DemonHunter_Companion && s.RuneIndex == 3);
            bool hasBoar = HotbarSkills.AssignedSkills.Any(s => s.Power == SNOPower.X1_DemonHunter_Companion && s.RuneIndex == 1);
            bool hasFerret = HotbarSkills.AssignedSkills.Any(s => s.Power == SNOPower.X1_DemonHunter_Companion && s.RuneIndex == 4);
            bool hasWolf = HotbarSkills.AssignedSkills.Any(s => s.Power == SNOPower.X1_DemonHunter_Companion && s.RuneIndex == 2);

            if (!UseOOCBuff && !Player.IsIncapacitated && Hotbar.Contains(SNOPower.X1_DemonHunter_Companion))
            {
                // Use Spider Slow on 4 or more trash mobs in an area or on Unique/Elite/Champion
                if (hasSpider && CombatBase.CanCast(SNOPower.X1_DemonHunter_Companion) && TargetUtil.ClusterExists(25f, 4) && TargetUtil.EliteOrTrashInRange(25f))
                {
                    return new TrinityPower(SNOPower.X1_DemonHunter_Companion);
                }

                //Use Bat when Hatred is Needed
                if (hasBat && CombatBase.CanCast(SNOPower.X1_DemonHunter_Companion) && Player.PrimaryResourceMissing >= 60)
                {
                    return new TrinityPower(SNOPower.X1_DemonHunter_Companion);
                }

                // Use Boar Taunt on 3 or more trash mobs in an area or on Unique/Elite/Champion
                if (hasBoar && CombatBase.CanCast(SNOPower.X1_DemonHunter_Companion) && ((TargetUtil.ClusterExists(20f, 4) && TargetUtil.EliteOrTrashInRange(20f)) ||
                    (CurrentTarget.IsBossOrEliteRareUnique && CurrentTarget.Distance <= 20f)))
                {
                    return new TrinityPower(SNOPower.X1_DemonHunter_Companion);
                }

                // Ferrets used for picking up Health Globes when low on Health
                if (hasFerret && ObjectCache.Any(o => o.Type == GObjectType.HealthGlobe && o.Distance < 60f) && Player.CurrentHealthPct < PlayerEmergencyHealthPotionLimit)
                {
                    return new TrinityPower(SNOPower.X1_DemonHunter_Companion);
                }

                // Use Wolf Howl on Unique/Elite/Champion - Would help for farming trash, but trash farming should not need this - Used on Elites to reduce Deaths per hour
                if (hasWolf && CombatBase.CanCast(SNOPower.X1_DemonHunter_Companion) &&
                    ((CurrentTarget.IsBossOrEliteRareUnique || TargetUtil.AnyMobsInRange(40, 10)) && CurrentTarget.RadiusDistance < 25f))
                {
                    return new TrinityPower(SNOPower.X1_DemonHunter_Companion);
                }
            }

            //skillDict.Add("EvasiveFire", SNOPower.DemonHunter_EvasiveFire);
            //runeDict.Add("Hardened", 0);
            //runeDict.Add("PartingGift", 2);
            //runeDict.Add("CoveringFire", 1);
            //runeDict.Add("Displace", 4);
            //runeDict.Add("Surge", 3);


            // Companion
            if (!Player.IsIncapacitated && CombatBase.CanCast(SNOPower.X1_DemonHunter_Companion) && TargetUtil.EliteOrTrashInRange(30f) &&
                Player.SecondaryResource >= 10)
            {
                return new TrinityPower(SNOPower.X1_DemonHunter_Companion, 0f, Vector3.Zero, CurrentWorldDynamicId, -1, 2, 1);
            }

            // Marked for Death
            if (!UseOOCBuff && !IsCurrentlyAvoiding && CombatBase.CanCast(SNOPower.DemonHunter_MarkedForDeath, CombatBase.CanCastFlags.NoTimer) &&
                Player.SecondaryResource >= 3 && !SpellTracker.IsUnitTracked(CurrentTarget, SNOPower.DemonHunter_MarkedForDeath))
            {
                return new TrinityPower(SNOPower.DemonHunter_MarkedForDeath, 40f, CurrentTarget.ACDGuid);
            }

            // Vault
            if (!UseOOCBuff && !IsCurrentlyAvoiding && CombatBase.CanCast(SNOPower.DemonHunter_Vault) && !Player.IsRooted && !Player.IsIncapacitated &&
                Settings.Combat.DemonHunter.VaultMode != Config.Combat.DemonHunterVaultMode.MovementOnly &&
                (TargetUtil.AnyMobsInRange(7f, 6) || Player.CurrentHealthPct <= 0.7) &&
                // if we have ShadowPower and Disicpline is >= 16
                // or if we don't have ShadoWpower and Discipline is >= 22
                (Player.SecondaryResource >= (Hotbar.Contains(SNOPower.DemonHunter_ShadowPower) ? 22 : 16)) &&
                    TimeSinceUse(SNOPower.DemonHunter_Vault) >= Settings.Combat.DemonHunter.VaultMovementDelay)
            {
                //Vector3 vNewTarget = MathEx.CalculatePointFrom(CurrentTarget.Position, Player.Position, -15f);
                // Lets find a smarter Vault position instead of just "backwards"
                //Vector3 vNewTarget = NavHelper.FindSafeZone(Trinity.Player.Position, true, false, null, false);
                Vector3 vNewTarget = NavHelper.MainFindSafeZone(Player.Position, true);

                return new TrinityPower(SNOPower.DemonHunter_Vault, 20f, vNewTarget, CurrentWorldDynamicId, -1, 1, 2);
            }

            // Rain of Vengeance
            if (!UseOOCBuff && CombatBase.CanCast(SNOPower.DemonHunter_RainOfVengeance) && !Player.IsIncapacitated &&
                (TargetUtil.ClusterExists(45f, 3) || TargetUtil.EliteOrTrashInRange(45f)))
            {
                var bestClusterPoint = TargetUtil.GetBestClusterPoint(45f, 65f, false);

                return new TrinityPower(SNOPower.DemonHunter_RainOfVengeance, 0f, bestClusterPoint, CurrentWorldDynamicId, -1, 1, 1);
            }

            // Cluster Arrow
            if (!UseOOCBuff && !IsCurrentlyAvoiding && CombatBase.CanCast(SNOPower.DemonHunter_ClusterArrow) && !Player.IsIncapacitated &&
                Player.PrimaryResource >= 50)
            {
                return new TrinityPower(SNOPower.DemonHunter_ClusterArrow, V.F("DemonHunter.ClusterArrow.UseRange"), CurrentTarget.ACDGuid);
            }

            // Multi Shot
            if (!UseOOCBuff && !IsCurrentlyAvoiding && CombatBase.CanCast(SNOPower.DemonHunter_Multishot) && !Player.IsIncapacitated &&
                Player.PrimaryResource >= 30 &&
                (TargetUtil.AnyMobsInRange(40, 2) || CurrentTarget.IsBossOrEliteRareUnique || CurrentTarget.IsTreasureGoblin))
            {
                return new TrinityPower(SNOPower.DemonHunter_Multishot, 30f, CurrentTarget.Position, CurrentWorldDynamicId, -1, 1, 1);
            }

            // Fan of Knives
            if (!UseOOCBuff && CombatBase.CanCast(SNOPower.DemonHunter_FanOfKnives) && !Player.IsIncapacitated &&
                (TargetUtil.EliteOrTrashInRange(15) || TargetUtil.AnyTrashInRange(15f, 5, false)))
            {
                return new TrinityPower(SNOPower.DemonHunter_FanOfKnives, 0f, Vector3.Zero, CurrentWorldDynamicId, -1, 1, 1);
            }

            // Strafe spam - similar to barbarian whirlwind routine
            if (!UseOOCBuff && !IsCurrentlyAvoiding && CombatBase.CanCast(SNOPower.DemonHunter_Strafe, CombatBase.CanCastFlags.NoTimer) &&
                !Player.IsIncapacitated && !Player.IsRooted && Player.PrimaryResource >= Settings.Combat.DemonHunter.StrafeMinHatred)
            {
                bool shouldGetNewZigZag =
                    (DateTime.UtcNow.Subtract(LastChangedZigZag).TotalMilliseconds >= V.I("Barbarian.Whirlwind.ZigZagMaxTime") ||
                    CurrentTarget.ACDGuid != LastZigZagUnitAcdGuid ||
                    CombatBase.ZigZagPosition.Distance2D(Player.Position) <= 5f);

                if (shouldGetNewZigZag)
                {
                    var wwdist = V.F("Barbarian.Whirlwind.ZigZagDistance");

                    CombatBase.ZigZagPosition = TargetUtil.GetZigZagTarget(CurrentTarget.Position, wwdist);

                    LastZigZagUnitAcdGuid = CurrentTarget.ACDGuid;
                    LastChangedZigZag = DateTime.UtcNow;
                }

                int postCastTickDelay = TrinityPower.MillisecondsToTickDelay(250);

                return new TrinityPower(SNOPower.DemonHunter_Strafe, 15f, CombatBase.ZigZagPosition, CurrentWorldDynamicId, -1, 0, postCastTickDelay);
            }

            // Spike Trap
            if (!UseOOCBuff && !Player.IsIncapacitated && CombatBase.CanCast(SNOPower.DemonHunter_SpikeTrap) &&
                LastPowerUsed != SNOPower.DemonHunter_SpikeTrap && Player.PrimaryResource >= 30)
            {
                // For distant monsters, try to target a little bit in-front of them (as they run towards us), if it's not a treasure goblin
                float fExtraDistance = 0f;
                if (CurrentTarget.Distance > 17f && !CurrentTarget.IsTreasureGoblin)
                {
                    fExtraDistance = CurrentTarget.Distance - 17f;
                    if (fExtraDistance > 5f)
                        fExtraDistance = 5f;
                    if (CurrentTarget.Distance - fExtraDistance < 15f)
                        fExtraDistance -= 2;
                }
                Vector3 vNewTarget = MathEx.CalculatePointFrom(CurrentTarget.Position, Player.Position, CurrentTarget.Distance - fExtraDistance);
                return new TrinityPower(SNOPower.DemonHunter_SpikeTrap, 35f, vNewTarget, CurrentWorldDynamicId, -1, 1, 1);
            }

            // Elemental Arrow
            if (!UseOOCBuff && !IsCurrentlyAvoiding && CombatBase.CanCast(SNOPower.DemonHunter_ElementalArrow) && !Player.IsIncapacitated &&
                ((Player.PrimaryResource >= 10 && !Player.WaitingForReserveEnergy) || Player.PrimaryResource >= MinEnergyReserve || hasKridershot))
            {
                return new TrinityPower(SNOPower.DemonHunter_ElementalArrow, 65f, Vector3.Zero, -1, CurrentTarget.ACDGuid, 0, 1);
            }

            // Chakram normal attack
            if (!UseOOCBuff && !IsCurrentlyAvoiding && Hotbar.Contains(SNOPower.DemonHunter_Chakram) && !Player.IsIncapacitated &&
                !Runes.DemonHunter.ShurikenCloud.IsActive &&
                ((Player.PrimaryResource >= 10 && !Player.WaitingForReserveEnergy) || Player.PrimaryResource >= MinEnergyReserve))
            {
                return new TrinityPower(SNOPower.DemonHunter_Chakram, 50f, Vector3.Zero, -1, CurrentTarget.ACDGuid, 0, 1);
            }

            // Chakram:Shuriken Cloud
            if (!Player.IsInTown && !IsCurrentlyAvoiding && Hotbar.Contains(SNOPower.DemonHunter_Chakram) && !Player.IsIncapacitated &&
                Runes.DemonHunter.ShurikenCloud.IsActive && TimeSinceUse(SNOPower.DemonHunter_Chakram) >= 110000 &&
                ((Player.PrimaryResource >= 10 && !Player.WaitingForReserveEnergy) || Player.PrimaryResource >= MinEnergyReserve))
            {
                return new TrinityPower(SNOPower.DemonHunter_Chakram, 0f, Vector3.Zero, CurrentWorldDynamicId, -1, 2, 2);
            }

            // Rapid Fire
            if (!UseOOCBuff && !IsCurrentlyAvoiding && CombatBase.CanCast(SNOPower.DemonHunter_RapidFire, CombatBase.CanCastFlags.NoTimer) &&
                !Player.IsIncapacitated && Player.PrimaryResource >= 16 &&
                (Player.PrimaryResource >= Settings.Combat.DemonHunter.RapidFireMinHatred || CombatBase.LastPowerUsed == SNOPower.DemonHunter_RapidFire))
            {
                // Players with grenades *AND* rapid fire should spam grenades at close-range instead
                if (Hotbar.Contains(SNOPower.DemonHunter_Grenades) && CurrentTarget.RadiusDistance <= 18f)
                {
                    return new TrinityPower(SNOPower.DemonHunter_Grenades, 18f, Vector3.Zero, -1, CurrentTarget.ACDGuid, 0, 0);
                }
                // Now return rapid fire, if not sending grenades instead
                return new TrinityPower(SNOPower.DemonHunter_RapidFire, 40f, CurrentTarget.Position);
            }

            // Impale
            bool hasM6 = Sets.EmbodimentOfTheMarauder.IsThirdBonusActive;
            bool impalecheck = !UseOOCBuff && !IsCurrentlyAvoiding && Hotbar.Contains(SNOPower.DemonHunter_Impale) && !Player.IsIncapacitated && (!TargetUtil.AnyMobsInRange(12, 4));
            if (!hasM6)
            {
                if (impalecheck &&
                    ((Player.PrimaryResource >= 25 && !Player.WaitingForReserveEnergy) || Player.PrimaryResource >= MinEnergyReserve) &&
                    CurrentTarget.RadiusDistance <= 50f)
                {
                    return new TrinityPower(SNOPower.DemonHunter_Impale, 50f, Vector3.Zero, -1, CurrentTarget.ACDGuid, 0, 1);
                }
            }
            //We don't want to spam Impale if we have M6 equipped
            else
            {
                if (impalecheck && ((Player.PrimaryResource >= 75 && !Player.WaitingForReserveEnergy)) &&
                CurrentTarget.RadiusDistance <= 50f)
                {
                    return new TrinityPower(SNOPower.DemonHunter_Impale, 50f, Vector3.Zero, -1, CurrentTarget.ACDGuid, 0, 1);
                }
            }

            // Evasive Fire
            if (!UseOOCBuff && CombatBase.CanCast(SNOPower.X1_DemonHunter_EvasiveFire) && !Player.IsIncapacitated &&
                  (TargetUtil.AnyMobsInRange(10f) || DemonHunter_HasNoPrimary()))
            {
                float range = DemonHunter_HasNoPrimary() ? 70f : 0f;

                return new TrinityPower(SNOPower.X1_DemonHunter_EvasiveFire, range, Vector3.Zero, -1, CurrentTarget.ACDGuid, 1, 1);
            }

            // Hungering Arrow
            if (!UseOOCBuff && !IsCurrentlyAvoiding && Hotbar.Contains(SNOPower.DemonHunter_HungeringArrow) && !Player.IsIncapacitated)
            {
                return new TrinityPower(SNOPower.DemonHunter_HungeringArrow, 50f, Vector3.Zero, -1, CurrentTarget.ACDGuid, 0, 0);
            }

            // Entangling shot
            if (!UseOOCBuff && !IsCurrentlyAvoiding && Hotbar.Contains(SNOPower.X1_DemonHunter_EntanglingShot) && !Player.IsIncapacitated)
            {
                return new TrinityPower(SNOPower.X1_DemonHunter_EntanglingShot, 50f, Vector3.Zero, -1, CurrentTarget.ACDGuid, 0, 0);
            }

            // Bola Shot
            if (!UseOOCBuff && !IsCurrentlyAvoiding && Hotbar.Contains(SNOPower.DemonHunter_Bolas) && !Player.IsIncapacitated)
            {
                return new TrinityPower(SNOPower.DemonHunter_Bolas, 50f, Vector3.Zero, -1, CurrentTarget.ACDGuid, 0, 1);
            }

            // Grenades
            if (!UseOOCBuff && !IsCurrentlyAvoiding && Hotbar.Contains(SNOPower.DemonHunter_Grenades) && !Player.IsIncapacitated)
            {
                return new TrinityPower(SNOPower.DemonHunter_Grenades, 40f, Vector3.Zero, -1, CurrentTarget.ACDGuid, 5, 5);
            }

            //Hexing Pants Mod
            if (!UseOOCBuff && !IsCurrentlyAvoiding && CurrentTarget != null)
            {
                if (Legendary.HexingPantsOfMrYan.IsEquipped && CurrentTarget.IsUnit &&
                CurrentTarget.RadiusDistance > 10f)
                {
                    return new TrinityPower(SNOPower.Walk, 10f, CurrentTarget.Position);
                }

                if (Legendary.HexingPantsOfMrYan.IsEquipped && CurrentTarget.IsUnit &&
                CurrentTarget.RadiusDistance < 10f)
                {
                    Vector3 vNewTarget = MathEx.CalculatePointFrom(CurrentTarget.Position, Player.Position, -10f);
                    return new TrinityPower(SNOPower.Walk, 10f, vNewTarget);
                }
            }

            // Default attacks
            return CombatBase.DefaultPower;
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

            return CombatBase.DefaultPower;
        }
    }
}
