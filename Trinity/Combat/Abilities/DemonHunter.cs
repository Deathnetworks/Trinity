using System;
using System.Linq;
using Trinity.Combat.Abilities;
using Zeta.Common;
using Zeta.Common.Plugins;
using Zeta.Game;
using Zeta.Game.Internals.Actors;


namespace Trinity
{
    public partial class Trinity : IPlugin
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
            bool hasKridershot = ZetaDia.Me.Inventory.Equipped.Any(i => i.GameBalanceId == 1999595351);

            bool hasPreparation = Hotbar.Contains(SNOPower.DemonHunter_Preparation);

            // Shadow Power
            if (!UseOOCBuff && CombatBase.CanCast(SNOPower.DemonHunter_ShadowPower) && !Player.IsIncapacitated &&
                (!GetHasBuff(SNOPower.DemonHunter_ShadowPower) || Trinity.Player.CurrentHealthPct <= 0.5) && // if we don't have the buff or our health is low
                ((!hasPreparation && Player.SecondaryResource >= 14) || (hasPreparation && Player.SecondaryResource >= 39)) && // Save some Discipline for Preparation
                (Player.CurrentHealthPct <= 0.99 || Player.IsRooted || TargetUtil.AnyMobsInRange(15)))
            {
                return new TrinityPower(SNOPower.DemonHunter_ShadowPower, 0f, Vector3.Zero, CurrentWorldDynamicId, -1, 1, 1, WAIT_FOR_ANIM);
            }

            // Vengeance
            if (!IsCurrentlyAvoiding && CombatBase.CanCast(SNOPower.X1_DemonHunter_Vengeance) && (TargetUtil.EliteOrTrashInRange(60) || TargetUtil.AnyMobsInRange(60, 6)))
            {
                return new TrinityPower(SNOPower.X1_DemonHunter_Vengeance, 60f, TargetUtil.GetBestClusterUnit(60f, 60f, 1, false, true).Position);
            }

            // Smoke Screen
            if ((!UseOOCBuff || Settings.Combat.DemonHunter.SpamSmokeScreen) && CombatBase.CanCast(SNOPower.DemonHunter_SmokeScreen) &&
                !GetHasBuff(SNOPower.DemonHunter_ShadowPower) && Player.SecondaryResource >= 14 &&
                (
                 (Player.CurrentHealthPct <= 0.50 || Player.IsRooted || TargetUtil.AnyMobsInRange(15) || Player.IsIncapacitated) ||
                 Settings.Combat.DemonHunter.SpamSmokeScreen
                ))
            {
                return new TrinityPower(SNOPower.DemonHunter_SmokeScreen, 0f, Vector3.Zero, CurrentWorldDynamicId, -1, 1, 1, WAIT_FOR_ANIM);
            }

            // Sentry Turret
            if (!UseOOCBuff && !Player.IsIncapacitated && CombatBase.CanCast(SNOPower.DemonHunter_Sentry, CombatBase.CanCastFlags.NoTimer) &&
                (TargetUtil.AnyElitesInRange(50) || TargetUtil.AnyMobsInRange(50, 2) || TargetUtil.IsEliteTargetInRange(50)) &&
                Player.PrimaryResource >= 30 &&
                (SpellHistory.TimeSinceUse(SNOPower.DemonHunter_Sentry) > TimeSpan.FromSeconds(10) || SpellHistory.DistanceFromLastUsePosition(SNOPower.DemonHunter_Sentry) > 7.5))
            {

                return new TrinityPower(SNOPower.DemonHunter_Sentry, 0f, Player.Position, CurrentWorldDynamicId, -1, 0, 0, NO_WAIT_ANIM);
            }

            // Caltrops
            if (!UseOOCBuff && !Player.IsIncapacitated && CombatBase.CanCast(SNOPower.DemonHunter_Caltrops) &&
                Player.SecondaryResource >= 6 && TargetUtil.AnyMobsInRange(40))
            {
                return new TrinityPower(SNOPower.DemonHunter_Caltrops, 0f, Vector3.Zero, CurrentWorldDynamicId, -1, 1, 1, WAIT_FOR_ANIM);
            }


            // Preperation: restore 30 disc / 45 sec cd
            // Invigoration = 1 : passive + 15 disc / 45 sec cd
            // Punishment   = 0 : Cost: 25 disc, restore 75 hatred / NO COOLDOWN
            // Battle Scars = 3 : instantly restore 40% health + restore 30 disc / 45 sec cd
            // Focused Mind = 2 : restore 45 disc over 15 sec / 45 sec cd
            // Backup plan  = 4 : 30% chance Prep has no cooldown (remove cast timer)

            // Restore 75 hatred for 25 disc - NO COOLDOWN!
            bool hasPunishment = HotbarSkills.AssignedSkills.Any(s => s.Power == SNOPower.DemonHunter_Preparation && s.RuneIndex == 0);
            bool hasInvigoration = HotbarSkills.AssignedSkills.Any(s => s.Power == SNOPower.DemonHunter_Preparation && s.RuneIndex == 1);
            bool hasBattleScars = HotbarSkills.AssignedSkills.Any(s => s.Power == SNOPower.DemonHunter_Preparation && s.RuneIndex == 3);
            bool hasFocusedMind = HotbarSkills.AssignedSkills.Any(s => s.Power == SNOPower.DemonHunter_Preparation && s.RuneIndex == 2);
            bool hasBackupPlan = HotbarSkills.AssignedSkills.Any(s => s.Power == SNOPower.DemonHunter_Preparation && s.RuneIndex == 4);

            float preperationTriggerRange = V.F("DemonHunter.PreperationTriggerRange");
            if (((!UseOOCBuff && !Player.IsIncapacitated &&
                (TargetUtil.AnyMobsInRange(preperationTriggerRange))) || Settings.Combat.DemonHunter.SpamPreparation || hasPunishment) &&
                Hotbar.Contains(SNOPower.DemonHunter_Preparation))
            {
                // Preperation w/ Punishment
                if (hasPunishment && CombatBase.CanCast(SNOPower.DemonHunter_Preparation, CombatBase.CanCastFlags.NoTimer) && Player.SecondaryResource >= 25 && Player.PrimaryResourceMissing >= 75)
                {
                    return new TrinityPower(SNOPower.DemonHunter_Preparation, 0f, Vector3.Zero, CurrentWorldDynamicId, -1, 1, 1, WAIT_FOR_ANIM);
                }

                // Preperation w/ Battle Scars - check for health only
                if (hasBattleScars && CombatBase.CanCast(SNOPower.DemonHunter_Preparation, CombatBase.CanCastFlags.NoTimer) && Player.CurrentHealthPct < 0.6)
                {
                    return new TrinityPower(SNOPower.DemonHunter_Preparation, 0f, Vector3.Zero, CurrentWorldDynamicId, -1, 1, 1, WAIT_FOR_ANIM);
                }

                // no rune || invigoration || focused mind || Backup Plan || Battle Scars (need Disc)
                if ((!hasPunishment) && CombatBase.CanCast(SNOPower.DemonHunter_Preparation, CombatBase.CanCastFlags.NoTimer) && Player.SecondaryResourceMissing >= 30)
                {
                    return new TrinityPower(SNOPower.DemonHunter_Preparation, 0f, Vector3.Zero, CurrentWorldDynamicId, -1, 1, 1, WAIT_FOR_ANIM);
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
                if (!UseOOCBuff && hasBoar && CombatBase.CanCast(SNOPower.X1_DemonHunter_Companion) && ((TargetUtil.ClusterExists(20f, 4) && TargetUtil.EliteOrTrashInRange(20f)) ||
                    (CurrentTarget.IsEliteRareUnique && CurrentTarget.CentreDistance <= 20f)))
                {
                    return new TrinityPower(SNOPower.X1_DemonHunter_Companion);
                }

                // Placeholder for Ferrets Logic - Unsure if utilities Exist to Determine Whether Gold and Health Globes are Within a Certain Range - Consider Interaction with Blood Vengeance for Optimized Results

                // Use Wolf Howl on Unique/Elite/Champion - Would help for farming trash, but trash farming should not need this - Used on Elites to reduce Deaths per hour
                if (hasWolf && CombatBase.CanCast(SNOPower.X1_DemonHunter_Companion) && CurrentTarget.IsEliteRareUnique)
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
                return new TrinityPower(SNOPower.X1_DemonHunter_Companion, 0f, Vector3.Zero, CurrentWorldDynamicId, -1, 2, 1, WAIT_FOR_ANIM);
            }

            // Marked for Death
            if (!UseOOCBuff && !IsCurrentlyAvoiding && CombatBase.CanCast(SNOPower.DemonHunter_MarkedForDeath) &&
                Player.SecondaryResource >= 3 &&
                (TargetUtil.AnyElitesInRange(40) || TargetUtil.AnyMobsInRange(40, 3) ||

                ((CurrentTarget.IsEliteRareUnique || CurrentTarget.IsTreasureGoblin || CurrentTarget.IsBoss) &&
                CurrentTarget.Radius <= 40 && CurrentTarget.RadiusDistance <= 40f)))
            {
                return new TrinityPower(SNOPower.DemonHunter_MarkedForDeath, 40f, Vector3.Zero, CurrentWorldDynamicId, CurrentTarget.ACDGuid, 1, 1, WAIT_FOR_ANIM);
            }

            // Vault
            if (!UseOOCBuff && !IsCurrentlyAvoiding && CombatBase.CanCast(SNOPower.DemonHunter_Vault) && !Player.IsRooted && !Player.IsIncapacitated &&
                Settings.Combat.DemonHunter.VaultMode != Config.Combat.DemonHunterVaultMode.MovementOnly &&
                // Only use vault to retreat if < level 60, or if in inferno difficulty for level 60's
                (Player.Level < 60 || iCurrentGameDifficulty > GameDifficulty.Master) &&
                (CurrentTarget.RadiusDistance <= 10f || TargetUtil.AnyMobsInRange(10)) &&
                // if we have ShadowPower and Disicpline is >= 16
                // or if we don't have ShadoWpower and Discipline is >= 22
                (Player.SecondaryResource >= (Hotbar.Contains(SNOPower.DemonHunter_ShadowPower) ? 22 : 16)) &&
                    TimeSinceUse(SNOPower.DemonHunter_Vault) >= Trinity.Settings.Combat.DemonHunter.VaultMovementDelay)
            {
                //Vector3 vNewTarget = MathEx.CalculatePointFrom(CurrentTarget.Position, Player.Position, -15f);
                // Lets find a smarter Vault position instead of just "backwards"
                Vector3 vNewTarget = NavHelper.FindSafeZone(Trinity.Player.Position, true, false, null, false);

                return new TrinityPower(SNOPower.DemonHunter_Vault, 20f, vNewTarget, CurrentWorldDynamicId, -1, 1, 2, WAIT_FOR_ANIM);
            }

            // Rain of Vengeance
            if (!UseOOCBuff && CombatBase.CanCast(SNOPower.DemonHunter_RainOfVengeance) && !Player.IsIncapacitated &&
                (TargetUtil.ClusterExists(45f, 3) || TargetUtil.EliteOrTrashInRange(45f)))
            {
                var bestClusterPoint = TargetUtil.GetBestClusterPoint(45f, 65f, false, true);

                return new TrinityPower(SNOPower.DemonHunter_RainOfVengeance, 0f, bestClusterPoint, CurrentWorldDynamicId, -1, 1, 1, WAIT_FOR_ANIM);
            }

            // Cluster Arrow
            if (!UseOOCBuff && !IsCurrentlyAvoiding && CombatBase.CanCast(SNOPower.DemonHunter_ClusterArrow) && !Player.IsIncapacitated &&
                Player.PrimaryResource >= 50)
            {
                return new TrinityPower(SNOPower.DemonHunter_ClusterArrow, 69f, Vector3.Zero, CurrentWorldDynamicId, CurrentTarget.ACDGuid, 1, 1, WAIT_FOR_ANIM);
            }

            // Multi Shot
            if (!UseOOCBuff && !IsCurrentlyAvoiding && CombatBase.CanCast(SNOPower.DemonHunter_Multishot) && !Player.IsIncapacitated &&
                Player.PrimaryResource >= 30 &&
                (TargetUtil.AnyMobsInRange(40, 2) || CurrentTarget.IsBossOrEliteRareUnique || CurrentTarget.IsTreasureGoblin))
            {
                return new TrinityPower(SNOPower.DemonHunter_Multishot, 30f, CurrentTarget.Position, CurrentWorldDynamicId, -1, 1, 1, WAIT_FOR_ANIM);
            }

            // Fan of Knives
            if (!UseOOCBuff && CombatBase.CanCast(SNOPower.DemonHunter_FanOfKnives) && !Player.IsIncapacitated &&
                (TargetUtil.EliteOrTrashInRange(15) || TargetUtil.AnyTrashInRange(15f, 5, false)))
            {
                return new TrinityPower(SNOPower.DemonHunter_FanOfKnives, 0f, Vector3.Zero, CurrentWorldDynamicId, -1, 1, 1, WAIT_FOR_ANIM);
            }

            // Strafe spam - similar to barbarian whirlwind routine
            if (!UseOOCBuff && !IsCurrentlyAvoiding && CombatBase.CanCast(SNOPower.DemonHunter_Strafe, CombatBase.CanCastFlags.NoTimer) &&
                !Player.IsIncapacitated && !Player.IsRooted && Player.PrimaryResource >= Settings.Combat.DemonHunter.StrafeMinHatred)
            {
                bool shouldGetNewZigZag =
                    (DateTime.UtcNow.Subtract(Trinity.LastChangedZigZag).TotalMilliseconds >= V.I("Barbarian.Whirlwind.ZigZagMaxTime") ||
                    CurrentTarget.ACDGuid != Trinity.LastZigZagUnitAcdGuid ||
                    CombatBase.ZigZagPosition.Distance2D(Player.Position) <= 5f);

                if (shouldGetNewZigZag)
                {
                    var wwdist = V.F("Barbarian.Whirlwind.ZigZagDistance");

                    CombatBase.ZigZagPosition = TargetUtil.GetZigZagTarget(CurrentTarget.Position, wwdist);

                    Trinity.LastZigZagUnitAcdGuid = CurrentTarget.ACDGuid;
                    Trinity.LastChangedZigZag = DateTime.UtcNow;
                }

                int postCastTickDelay = TrinityPower.MillisecondsToTickDelay(250);

                return new TrinityPower(SNOPower.DemonHunter_Strafe, 15f, CombatBase.ZigZagPosition, CurrentWorldDynamicId, -1, 0, postCastTickDelay, WAIT_FOR_ANIM);
            }

            // Spike Trap
            if (!UseOOCBuff && !Player.IsIncapacitated && CombatBase.CanCast(SNOPower.DemonHunter_SpikeTrap) &&
                LastPowerUsed != SNOPower.DemonHunter_SpikeTrap && Player.PrimaryResource >= 30)
            {
                // For distant monsters, try to target a little bit in-front of them (as they run towards us), if it's not a treasure goblin
                float fExtraDistance = 0f;
                if (CurrentTarget.CentreDistance > 17f && !CurrentTarget.IsTreasureGoblin)
                {
                    fExtraDistance = CurrentTarget.CentreDistance - 17f;
                    if (fExtraDistance > 5f)
                        fExtraDistance = 5f;
                    if (CurrentTarget.CentreDistance - fExtraDistance < 15f)
                        fExtraDistance -= 2;
                }
                Vector3 vNewTarget = MathEx.CalculatePointFrom(CurrentTarget.Position, Player.Position, CurrentTarget.CentreDistance - fExtraDistance);
                return new TrinityPower(SNOPower.DemonHunter_SpikeTrap, 35f, vNewTarget, CurrentWorldDynamicId, -1, 1, 1, WAIT_FOR_ANIM);
            }

            //skillDict.Add("ElementalArrow", SNOPower.DemonHunter_ElementalArrow);
            //runeDict.Add("BallLightning", 1);
            //runeDict.Add("FrostArrow", 0);
            //runeDict.Add("ScreamingSkull", 2);
            //runeDict.Add("LightningBolts", 4);
            //runeDict.Add("NetherTentacles", 3);

            var hasBallLightning = HotbarSkills.AssignedSkills.Any(s => s.Power == SNOPower.DemonHunter_ElementalArrow && s.RuneIndex == 1);
            var hasFrostArrow = HotbarSkills.AssignedSkills.Any(s => s.Power == SNOPower.DemonHunter_ElementalArrow && s.RuneIndex == 0);
            var hasScreamingSkull = HotbarSkills.AssignedSkills.Any(s => s.Power == SNOPower.DemonHunter_ElementalArrow && s.RuneIndex == 2);
            var hasLightningBolts = HotbarSkills.AssignedSkills.Any(s => s.Power == SNOPower.DemonHunter_ElementalArrow && s.RuneIndex == 4);
            var hasNetherTentacles = HotbarSkills.AssignedSkills.Any(s => s.Power == SNOPower.DemonHunter_ElementalArrow && s.RuneIndex == 3);

            // Elemental Arrow
            if (!UseOOCBuff && !IsCurrentlyAvoiding && Hotbar.Contains(SNOPower.DemonHunter_ElementalArrow) &&
                SNOPowerUseTimer(SNOPower.DemonHunter_ElementalArrow) && !Player.IsIncapacitated &&
                ((Player.PrimaryResource >= 10 && !Player.WaitingForReserveEnergy) || Player.PrimaryResource >= MinEnergyReserve || hasKridershot))
            {
                return new TrinityPower(SNOPower.DemonHunter_ElementalArrow, 65f, Vector3.Zero, -1, CurrentTarget.ACDGuid, 0, 1, WAIT_FOR_ANIM);
            }

            //skillDict.Add("Chakram", SNOPower.DemonHunter_Chakram);
            //runeDict.Add("TwinChakrams", 0);
            //runeDict.Add("Serpentine", 2);
            //runeDict.Add("RazorDisk", 3);
            //runeDict.Add("Boomerang", 1);
            //runeDict.Add("ShurikenCloud", 4);

            bool hasShurikenCloud = HotbarSkills.AssignedSkills.Any(s => s.Power == SNOPower.DemonHunter_Chakram && s.RuneIndex == 4);

            // Chakram normal attack
            if (!UseOOCBuff && !IsCurrentlyAvoiding && Hotbar.Contains(SNOPower.DemonHunter_Chakram) && !Player.IsIncapacitated &&
                !hasShurikenCloud &&
                ((Player.PrimaryResource >= 10 && !Player.WaitingForReserveEnergy) || Player.PrimaryResource >= MinEnergyReserve))
            {
                return new TrinityPower(SNOPower.DemonHunter_Chakram, 50f, Vector3.Zero, -1, CurrentTarget.ACDGuid, 0, 1, WAIT_FOR_ANIM);
            }

            // Chakram:Shuriken Cloud
            if (!Player.IsInTown && !IsCurrentlyAvoiding && Hotbar.Contains(SNOPower.DemonHunter_Chakram) && !Player.IsIncapacitated &&
                hasShurikenCloud && TimeSinceUse(SNOPower.DemonHunter_Chakram) >= 110000 &&
                ((Player.PrimaryResource >= 10 && !Player.WaitingForReserveEnergy) || Player.PrimaryResource >= MinEnergyReserve))
            {
                return new TrinityPower(SNOPower.DemonHunter_Chakram, 0f, Vector3.Zero, CurrentWorldDynamicId, -1, 2, 2, WAIT_FOR_ANIM);
            }

            // Rapid Fire
            if (!UseOOCBuff && !IsCurrentlyAvoiding && CombatBase.CanCast(SNOPower.DemonHunter_RapidFire, CombatBase.CanCastFlags.NoTimer) && !Player.IsIncapacitated &&
                Player.PrimaryResource >= 20)
            {
                // Players with grenades *AND* rapid fire should spam grenades at close-range instead
                if (Hotbar.Contains(SNOPower.DemonHunter_Grenades) && CurrentTarget.RadiusDistance <= 18f)
                {
                    return new TrinityPower(SNOPower.DemonHunter_Grenades, 18f, Vector3.Zero, -1, CurrentTarget.ACDGuid, 0, 0, WAIT_FOR_ANIM);
                }
                // Now return rapid fire, if not sending grenades instead
                return new TrinityPower(SNOPower.DemonHunter_RapidFire, 50f, CurrentTarget.Position);
            }

            // Impale
            if (!UseOOCBuff && !IsCurrentlyAvoiding && Hotbar.Contains(SNOPower.DemonHunter_Impale) && !Player.IsIncapacitated &&
                (!TargetUtil.AnyMobsInRange(12, 4)) &&
                ((Player.PrimaryResource >= 25 && !Player.WaitingForReserveEnergy) || Player.PrimaryResource >= MinEnergyReserve) &&
                CurrentTarget.RadiusDistance <= 50f)
            {
                return new TrinityPower(SNOPower.DemonHunter_Impale, 50f, Vector3.Zero, -1, CurrentTarget.ACDGuid, 0, 1, WAIT_FOR_ANIM);
            }

            // Evasive Fire
            if (!UseOOCBuff && CombatBase.CanCast(SNOPower.X1_DemonHunter_EvasiveFire) && !Player.IsIncapacitated &&
                  (TargetUtil.AnyMobsInRange(10f) || DemonHunter_HasNoPrimary()))
            {
                float range = DemonHunter_HasNoPrimary() ? 70f : 0f;

                return new TrinityPower(SNOPower.X1_DemonHunter_EvasiveFire, range, Vector3.Zero, -1, CurrentTarget.ACDGuid, 1, 1, WAIT_FOR_ANIM);
            }

            // Hungering Arrow
            if (!UseOOCBuff && !IsCurrentlyAvoiding && Hotbar.Contains(SNOPower.DemonHunter_HungeringArrow) && !Player.IsIncapacitated)
            {
                return new TrinityPower(SNOPower.DemonHunter_HungeringArrow, 50f, Vector3.Zero, -1, CurrentTarget.ACDGuid, 0, 0, WAIT_FOR_ANIM);
            }

            // Entangling shot
            if (!UseOOCBuff && !IsCurrentlyAvoiding && Hotbar.Contains(SNOPower.X1_DemonHunter_EntanglingShot) && !Player.IsIncapacitated)
            {
                return new TrinityPower(SNOPower.X1_DemonHunter_EntanglingShot, 50f, Vector3.Zero, -1, CurrentTarget.ACDGuid, 0, 0, WAIT_FOR_ANIM);
            }

            // Bola Shot
            if (!UseOOCBuff && !IsCurrentlyAvoiding && Hotbar.Contains(SNOPower.DemonHunter_Bolas) && !Player.IsIncapacitated)
            {
                return new TrinityPower(SNOPower.DemonHunter_Bolas, 50f, Vector3.Zero, -1, CurrentTarget.ACDGuid, 0, 1, WAIT_FOR_ANIM);
            }

            // Grenades
            if (!UseOOCBuff && !IsCurrentlyAvoiding && Hotbar.Contains(SNOPower.DemonHunter_Grenades) && !Player.IsIncapacitated)
            {
                return new TrinityPower(SNOPower.DemonHunter_Grenades, 40f, Vector3.Zero, -1, CurrentTarget.ACDGuid, 5, 5, WAIT_FOR_ANIM);
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
                return new TrinityPower(SNOPower.DemonHunter_HungeringArrow, 40f, CurrentTarget.ACDGuid);

            if (Hotbar.Contains(SNOPower.X1_DemonHunter_EntanglingShot))
                return new TrinityPower(SNOPower.X1_DemonHunter_EntanglingShot, 40f, CurrentTarget.ACDGuid);

            if (Hotbar.Contains(SNOPower.DemonHunter_Bolas))
                return new TrinityPower(SNOPower.DemonHunter_Bolas, 40f, CurrentTarget.ACDGuid);

            if (Hotbar.Contains(SNOPower.DemonHunter_Grenades))
                return new TrinityPower(SNOPower.DemonHunter_Grenades, 15f, CurrentTarget.ACDGuid);

            if (Hotbar.Contains(SNOPower.DemonHunter_ElementalArrow) && Player.PrimaryResource >= 10)
                return new TrinityPower(SNOPower.DemonHunter_ElementalArrow, 40f, CurrentTarget.ACDGuid);

            if (Hotbar.Contains(SNOPower.X1_DemonHunter_EvasiveFire) && Player.PrimaryResource >= 10)
                return new TrinityPower(SNOPower.X1_DemonHunter_EvasiveFire, 40f, CurrentTarget.ACDGuid);

            if (Hotbar.Contains(SNOPower.DemonHunter_RapidFire) && Player.PrimaryResource >= 10)
                return new TrinityPower(SNOPower.DemonHunter_RapidFire, 40f, CurrentTarget.Position);

            if (Hotbar.Contains(SNOPower.DemonHunter_Chakram) && Player.PrimaryResource >= 20)
                return new TrinityPower(SNOPower.DemonHunter_Chakram, 0f, CurrentTarget.ACDGuid);

            return CombatBase.DefaultPower;
        }
    }
}
