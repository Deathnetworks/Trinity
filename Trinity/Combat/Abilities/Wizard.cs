using System;
using System.Linq;
using Trinity.Combat.Abilities;
using Trinity.Config.Combat;
using Trinity.Technicals;
using Zeta.Bot;
using Zeta.Common;
using Zeta.Game;
using Zeta.Game.Internals.Actors;
using Logger = Trinity.Technicals.Logger;

namespace Trinity
{
    public partial class Trinity
    {
        private static TrinityPower GetWizardPower(bool isCurrentlyAvoiding, bool useOocBuff, bool useDestructiblePower)
        {
            // Pick the best destructible power available
            if (useDestructiblePower)
            {
                if (!GetHasBuff(SNOPower.Wizard_Archon))
                {
                    return GetWizardDestructablePower();
                }
                if (CurrentTarget.RadiusDistance <= 10f)
                    return new TrinityPower(SNOPower.Wizard_Archon_ArcaneStrike, 20f, Vector3.Zero, -1, CurrentTarget.ACDGuid, 0, 0);
                return new TrinityPower(SNOPower.Wizard_Archon_DisintegrationWave, 19f, Vector3.Zero, -1, CurrentTarget.ACDGuid, 0, 0);
            }
            // Wizards want to save up to a reserve of 65+ energy
            MinEnergyReserve = 45;

            if (!GetHasBuff(SNOPower.Wizard_Archon))
            {
                bool hasIllusionist = ZetaDia.CPlayer.PassiveSkills.Any(p => p == SNOPower.Wizard_Passive_Illusionist);

                // Illusionist speed boost
                if (hasIllusionist && useOocBuff)
                {
                    // Slow Time on self for speed boost
                    if (CombatBase.CanCast(SNOPower.Wizard_SlowTime))
                        return new TrinityPower(SNOPower.Wizard_SlowTime);

                    // Mirror Image for speed boost
                    if (CombatBase.CanCast(SNOPower.Wizard_MirrorImage))
                        return new TrinityPower(SNOPower.Wizard_MirrorImage);

                    // Teleport already called from PlayerMover, not here (since it's a "movement" spell, not a buff)
                }


                bool hasCalamity = HotbarSkills.AssignedSkills.Any(s => s.Power == SNOPower.Wizard_Teleport && s.RuneIndex == 0);
                // Offensive Teleport: Calamity
                if (CombatBase.CanCast(SNOPower.Wizard_Teleport) && hasCalamity && (TargetUtil.ClusterExists(15f, 4) || TargetUtil.IsEliteTargetInRange(55f)))
                {
                    var bestClusterPoint = TargetUtil.GetBestClusterPoint();
                    return new TrinityPower(SNOPower.Wizard_Teleport, 55f, bestClusterPoint);
                }

                bool hasSafePassage = HotbarSkills.AssignedSkills.Any(s => s.Power == SNOPower.Wizard_Teleport && s.RuneIndex == 1);
                // Defensive Teleport: SafePassage
                if (CombatBase.CanCast(SNOPower.Wizard_Teleport) && hasSafePassage &&
                    TimeSinceUse(SNOPower.Wizard_Teleport) >= 5000 && Player.CurrentHealthPct <= 0.75 &&
                    (CurrentTarget.IsBossOrEliteRareUnique || TargetUtil.IsEliteTargetInRange(30f)))
                {
                    return new TrinityPower(SNOPower.Wizard_Teleport, 1f, Player.Position);
                }

                // Black Hole experiment
                //spell steal
                //[Trinity] Hotbar Skills (Skill/RuneIndex/Slot): Weapon_Ranged_Wand/-1/HotbarMouseLeft X1_Wizard_Wormhole/3/HotbarSlot1 None/-1/HotbarSlot2
                //blazar
                //[Trinity] Hotbar Skills (Skill/RuneIndex/Slot): Weapon_Ranged_Wand/-1/HotbarMouseLeft X1_Wizard_Wormhole/2/HotbarSlot1 None/-1/HotbarSlot2
                //event horizon
                //[Trinity] Hotbar Skills (Skill/RuneIndex/Slot): Weapon_Ranged_Wand/-1/HotbarMouseLeft X1_Wizard_Wormhole/1/HotbarSlot1 None/-1/HotbarSlot2
                //absolute zero
                //[Trinity] Hotbar Skills (Skill/RuneIndex/Slot): Weapon_Ranged_Wand/-1/HotbarMouseLeft X1_Wizard_Wormhole/4/HotbarSlot1 None/-1/HotbarSlot2
                //super massive
                //[Trinity] Hotbar Skills (Skill/RuneIndex/Slot): Weapon_Ranged_Wand/-1/HotbarMouseLeft X1_Wizard_Wormhole/0/HotbarSlot1 None/-1/HotbarSlot2
                //no rune
                //[Trinity] Hotbar Skills (Skill/RuneIndex/Slot): Weapon_Ranged_Wand/-1/HotbarMouseLeft X1_Wizard_Wormhole/-1/HotbarSlot1 None/-1/HotbarSlot2

                bool hasSupermassive = HotbarSkills.AssignedSkills.Any(s => s.Power == SNOPower.X1_Wizard_Wormhole && s.RuneIndex == 0);
                float blackholeRadius = hasSupermassive ? 20f : 15f;
                if (!useOocBuff && !isCurrentlyAvoiding && CombatBase.CanCast(SNOPower.X1_Wizard_Wormhole, CombatBase.CanCastFlags.NoTimer) &&
                    TargetUtil.ClusterExists(blackholeRadius, 45f, 4))
                {
                    return new TrinityPower(SNOPower.X1_Wizard_Wormhole, 45f, TargetUtil.GetBestClusterUnit(blackholeRadius, 45f, 1, false).Position);
                }

                bool arcaneDynamoPassiveReady =
                 (ZetaDia.CPlayer.PassiveSkills.Any(s => s == SNOPower.Wizard_Passive_ArcaneDynamo) && GetBuffStacks(SNOPower.Wizard_Passive_ArcaneDynamo) == 5);

                var bestMeteorClusterUnit = TargetUtil.GetBestClusterUnit();
                // Meteor: Arcane Dynamo
                if (!useOocBuff && !Player.IsIncapacitated && !arcaneDynamoPassiveReady && CombatBase.CanCast(SNOPower.Wizard_Meteor, CombatBase.CanCastFlags.NoTimer) &&
                    (TargetUtil.EliteOrTrashInRange(65) || TargetUtil.ClusterExists(15f, 65, 2)))
                {
                    return new TrinityPower(SNOPower.Wizard_Meteor, 65f, bestMeteorClusterUnit.Position);
                }

                // Diamond Skin SPAM
                if (!useOocBuff && CombatBase.CanCast(SNOPower.Wizard_DiamondSkin) && LastPowerUsed != SNOPower.Wizard_DiamondSkin && !GetHasBuff(SNOPower.Wizard_DiamondSkin) &&
                    (TargetUtil.AnyElitesInRange(25, 1) || TargetUtil.AnyMobsInRange(25, 1) || Player.CurrentHealthPct <= 0.90 || Player.IsIncapacitated || Player.IsRooted || CurrentTarget.RadiusDistance <= 40f))
                {
                    return new TrinityPower(SNOPower.Wizard_DiamondSkin);
                }

                // Diamond Skin off CD
                bool hasSleekShell = HotbarSkills.AssignedSkills.Any(s => s.Power == SNOPower.Wizard_DiamondSkin && s.RuneIndex == 0);
                if (hasSleekShell && CombatBase.CanCast(SNOPower.Wizard_DiamondSkin))
                {
                    return new TrinityPower(SNOPower.Wizard_DiamondSkin);
                }

                // Slow Time for in combat
                if (!useOocBuff && !Player.IsIncapacitated && CombatBase.CanCast(SNOPower.Wizard_SlowTime, CombatBase.CanCastFlags.NoTimer) &&
                    (TargetUtil.AnyElitesInRange(25, 1) || TargetUtil.AnyMobsInRange(25, 2) || (CurrentTarget.IsBossOrEliteRareUnique && CurrentTarget.RadiusDistance <= 40f)) &&
                    (CombatBase.TimeSpanSincePowerUse(SNOPower.Wizard_SlowTime) > TimeSpan.FromSeconds(15) || SpellHistory.DistanceFromLastTarget(SNOPower.Wizard_SlowTime) > 30f))
                {
                    if (TargetUtil.AnyMobsInRange(20f))
                        return new TrinityPower(SNOPower.Wizard_SlowTime); // cast of Self
                    return new TrinityPower(SNOPower.Wizard_SlowTime, 55f, TargetUtil.GetBestClusterUnit(20f).Position);
                }

                // Mirror Image  @ half health or 5+ monsters or rooted/incapacitated or last elite left @25% health
                if (!useOocBuff && CombatBase.CanCast(SNOPower.Wizard_MirrorImage, CombatBase.CanCastFlags.NoTimer) &&
                    (Player.CurrentHealthPct <= PlayerEmergencyHealthPotionLimit || TargetUtil.AnyMobsInRange(30, 4) || Player.IsIncapacitated || Player.IsRooted ||
                    TargetUtil.AnyElitesInRange(30) || CurrentTarget.IsBossOrEliteRareUnique))
                {
                    return new TrinityPower(SNOPower.Wizard_MirrorImage, 0f, Vector3.Zero, CurrentWorldDynamicId, -1, 1, 1);
                }

                // Familiar
                if (!Player.IsIncapacitated && CombatBase.CanCast(SNOPower.Wizard_Familiar) && Player.PrimaryResource >= 20 && !Wizard_HasFamiliar())
                {
                    return new TrinityPower(SNOPower.Wizard_Familiar, 0f, Vector3.Zero, CurrentWorldDynamicId, -1, 1, 2);
                }

                // The three wizard armors, done in an else-if loop so it doesn't keep replacing one with the other
                if (!Player.IsIncapacitated && Player.PrimaryResource >= 25)
                {
                    // Energy armor as priority cast if available and not buffed
                    if (Hotbar.Contains(SNOPower.Wizard_EnergyArmor))
                    {
                        if ((!GetHasBuff(SNOPower.Wizard_EnergyArmor) && PowerManager.CanCast(SNOPower.Wizard_EnergyArmor)) || (Hotbar.Contains(SNOPower.Wizard_Archon) && (!GetHasBuff(SNOPower.Wizard_EnergyArmor) || SNOPowerUseTimer(SNOPower.Wizard_EnergyArmor))))
                        {
                            return new TrinityPower(SNOPower.Wizard_EnergyArmor, 0f, Vector3.Zero, CurrentWorldDynamicId, -1, 1, 2);
                        }
                    }
                    // Ice Armor
                    else if (Hotbar.Contains(SNOPower.Wizard_IceArmor))
                    {
                        if (!GetHasBuff(SNOPower.Wizard_IceArmor) && PowerManager.CanCast(SNOPower.Wizard_IceArmor))
                        {
                            return new TrinityPower(SNOPower.Wizard_IceArmor, 0f, Vector3.Zero, CurrentWorldDynamicId, -1, 1, 2);
                        }
                    }
                    // Storm Armor
                    else if (Hotbar.Contains(SNOPower.Wizard_StormArmor))
                    {
                        if (!GetHasBuff(SNOPower.Wizard_StormArmor) && PowerManager.CanCast(SNOPower.Wizard_StormArmor))
                        {
                            return new TrinityPower(SNOPower.Wizard_StormArmor, 0f, Vector3.Zero, CurrentWorldDynamicId, -1, 1, 2);
                        }
                    }
                }
                // Magic Weapon (10 minutes)                 
                if (!Player.IsIncapacitated && Player.PrimaryResource >= 25 && CombatBase.CanCast(SNOPower.Wizard_MagicWeapon) && !GetHasBuff(SNOPower.Wizard_MagicWeapon))
                {
                    return new TrinityPower(SNOPower.Wizard_MagicWeapon, 0f, Vector3.Zero, CurrentWorldDynamicId, -1, 1, 2);
                }

                // Hydra
                if (!useOocBuff && !Player.IsIncapacitated && CombatBase.CanCast(SNOPower.Wizard_Hydra, CombatBase.CanCastFlags.NoTimer) &&
                    (CombatBase.TimeSpanSincePowerUse(SNOPower.Wizard_Hydra) > TimeSpan.FromSeconds(15) && SpellHistory.DistanceFromLastTarget(SNOPower.Wizard_Hydra) > 30f) && //LastPowerUsed != SNOPower.Wizard_Hydra &&
                    (TargetUtil.AnyElitesInRange(15, 1) || TargetUtil.AnyMobsInRange(15, 4) || (CurrentTarget.IsBossOrEliteRareUnique && CurrentTarget.RadiusDistance <= 15f)) &&
                    Player.PrimaryResource >= 15)
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
                    return new TrinityPower(SNOPower.Wizard_Hydra, 30f, vNewTarget, CurrentWorldDynamicId, -1, 1, 2);
                }

                // Archon
                if (!useOocBuff && !isCurrentlyAvoiding && CombatBase.CanCast(SNOPower.Wizard_Archon, CombatBase.CanCastFlags.NoTimer) && Wizard_ShouldStartArchon())
                {
                    //CanCastArchon = false;
                    //return new TrinityPower(SNOPower.Wizard_Archon, 0f, Vector3.Zero, CurrentWorldDynamicId, -1, 4, 5);

                    // Familiar has been removed for now. Uncomment the three comments below relating to familiars to force re-buffing them
                    int reserveArcanePower = 0;
                    if (Hotbar.Contains(SNOPower.Wizard_MagicWeapon)) reserveArcanePower += 25;
                    if (Hotbar.Contains(SNOPower.Wizard_Familiar)) reserveArcanePower += 25;
                    if (Hotbar.Contains(SNOPower.Wizard_EnergyArmor) || Hotbar.Contains(SNOPower.Wizard_IceArmor) ||
                        Hotbar.Contains(SNOPower.Wizard_StormArmor)) reserveArcanePower += 25;

                    bool hasBuffSpells =
                        (Hotbar.Contains(SNOPower.Wizard_MagicWeapon) ||
                        Hotbar.Contains(SNOPower.Wizard_Familiar) ||
                        Hotbar.Contains(SNOPower.Wizard_EnergyArmor) ||
                        Hotbar.Contains(SNOPower.Wizard_IceArmor) ||
                        Hotbar.Contains(SNOPower.Wizard_StormArmor));

                    CanCastArchon = //Player.PrimaryResource >= reserveArcanePower || 
                        (
                        //hasBuffSpells &&
                        CheckAbilityAndBuff(SNOPower.Wizard_MagicWeapon) &&
                        (!Hotbar.Contains(SNOPower.Wizard_Familiar) || Wizard_HasFamiliar()) &&
                        CheckAbilityAndBuff(SNOPower.Wizard_EnergyArmor) &&
                        CheckAbilityAndBuff(SNOPower.Wizard_IceArmor) &&
                        CheckAbilityAndBuff(SNOPower.Wizard_StormArmor));

                    if (CanCastArchon)
                    {
                        Player.WaitingForReserveEnergy = false;
                        CanCastArchon = false;
                        ShouldRefreshHotbarAbilities = true;
                        return new TrinityPower(SNOPower.Wizard_Archon, 0f, Vector3.Zero, CurrentWorldDynamicId, -1, 4, 5);
                    }
                    Player.WaitingForReserveEnergy = true;
                }
                // Explosive Blast
                if (!useOocBuff && !Player.IsIncapacitated && CombatBase.CanCast(SNOPower.Wizard_ExplosiveBlast, CombatBase.CanCastFlags.NoTimer) && Player.PrimaryResource >= 20)
                {
                    return new TrinityPower(SNOPower.Wizard_ExplosiveBlast, 12f, CurrentTarget.Position);
                }

                //SkillDict.Add("Blizzard", SNOPower.Wizard_Blizzard);
                //RuneDict.Add("GraspingChill", 2);
                //RuneDict.Add("FrozenSolid", 4);
                //RuneDict.Add("Snowbound", 3);
                //RuneDict.Add("StarkWinter", 1);
                //RuneDict.Add("UnrelentingStorm", 0);

                bool hasSnowBoundRune = HotbarSkills.AssignedSkills.Any(s => s.Power == SNOPower.Wizard_Blizzard && s.RuneIndex == 3);

                // Blizzard
                if (!useOocBuff && !Player.IsIncapacitated && Hotbar.Contains(SNOPower.Wizard_Blizzard) &&
                    (TargetUtil.ClusterExists(18f, 90f, 2, false) || TargetUtil.AnyElitesInRange(40f) || TargetUtil.IsEliteTargetInRange(45f)) &&
                    (Player.PrimaryResource >= 40 || (hasSnowBoundRune && Player.PrimaryResource >= 20)) && SNOPowerUseTimer(SNOPower.Wizard_Blizzard))
                {
                    var bestClusterPoint = TargetUtil.GetBestClusterPoint(18f, 45f, false);
                    return new TrinityPower(SNOPower.Wizard_Blizzard, 45f, bestClusterPoint, CurrentWorldDynamicId, -1, 1, 1);
                }

                bool hasArcaneDynamo = ZetaDia.CPlayer.PassiveSkills.Any(s => s == SNOPower.Wizard_Passive_ArcaneDynamo);

                // Meteor - no arcane dynamo
                if (!useOocBuff && !Player.IsIncapacitated && !hasArcaneDynamo && CombatBase.CanCast(SNOPower.Wizard_Meteor, CombatBase.CanCastFlags.NoTimer) &&
                    (TargetUtil.EliteOrTrashInRange(65) || TargetUtil.ClusterExists(15f, 65, 2)))
                {
                    return new TrinityPower(SNOPower.Wizard_Meteor, 65f, bestMeteorClusterUnit.Position);
                }

                //SkillDict.Add("FrostNova", SNOPower.Wizard_FrostNova);
                //RuneDict.Add("Shatter", 1);
                //RuneDict.Add("ColdSnap", 3);
                //RuneDict.Add("FrozenMist", 2);
                //RuneDict.Add("DeepFreeze", 4);
                //RuneDict.Add("BoneChill", 0);

                bool hasDeepFreeze = HotbarSkills.AssignedSkills.Any(s => s.Power == SNOPower.Wizard_FrostNova && s.RuneIndex == 4);

                // Frost Nova
                if (!useOocBuff && CombatBase.CanCast(SNOPower.Wizard_FrostNova) && !Player.IsIncapacitated &&
                    ((hasDeepFreeze && TargetUtil.AnyMobsInRange(25, 5)) || (!hasDeepFreeze && (TargetUtil.AnyMobsInRange(25, 1) || Player.CurrentHealthPct <= 0.7)) &&
                    CurrentTarget.RadiusDistance <= 25f))
                {
                    return new TrinityPower(SNOPower.Wizard_FrostNova, 20f, Vector3.Zero, CurrentWorldDynamicId, -1, 0, 2);
                }

                // Check to see if we have a signature spell on our hotbar, for energy twister check
                bool bHasSignatureSpell = (Hotbar.Contains(SNOPower.Wizard_MagicMissile) || Hotbar.Contains(SNOPower.Wizard_ShockPulse) ||
                    Hotbar.Contains(SNOPower.Wizard_SpectralBlade) || Hotbar.Contains(SNOPower.Wizard_Electrocute));

                //SkillDict.Add("EnergyTwister", SNOPower.Wizard_EnergyTwister);
                //RuneDict.Add("MistralBreeze", 3);
                //RuneDict.Add("GaleForce", 0);
                //RuneDict.Add("RagingStorm", 1);
                //RuneDict.Add("WickedWind", 4);
                //RuneDict.Add("StromChaser", 2);

                bool hasWickedWindRune = HotbarSkills.AssignedSkills.Any(s => s.Power == SNOPower.Wizard_EnergyTwister && s.RuneIndex == 4);

                // Energy Twister SPAMS whenever 35 or more ap to generate Arcane Power
                if (!useOocBuff && !Player.IsIncapacitated && CombatBase.CanCast(SNOPower.Wizard_EnergyTwister) &&
                    Player.PrimaryResource >= 35 &&
                    // If using storm chaser, then force a signature spell every 1 stack of the buff, if we have a signature spell
                    (!bHasSignatureSpell || GetBuffStacks(SNOPower.Wizard_EnergyTwister) < 1) &&
                    ((!hasWickedWindRune && CurrentTarget.RadiusDistance <= 25f) ||
                    (hasWickedWindRune && CurrentTarget.RadiusDistance <= 60f)) &&
                    (!Hotbar.Contains(SNOPower.Wizard_Electrocute) || !DataDictionary.FastMovingMonsterIds.Contains(CurrentTarget.ActorSNO)))
                {
                    Vector3 bestClusterPoint = TargetUtil.GetBestClusterPoint(10f, 15f);

                    const float twisterRange = 28f;
                    return new TrinityPower(SNOPower.Wizard_EnergyTwister, twisterRange, bestClusterPoint, CurrentWorldDynamicId, -1, 0, 0);
                }

                // Wave of force
                if (!useOocBuff && !Player.IsIncapacitated && Player.PrimaryResource >= 25 && CombatBase.CanCast(SNOPower.Wizard_WaveOfForce, CombatBase.CanCastFlags.NoTimer))
                {
                    return new TrinityPower(SNOPower.Wizard_WaveOfForce, 5f, CurrentTarget.Position, CurrentWorldDynamicId, -1, 1, 2);
                }

                bool hasEntropy = HotbarSkills.AssignedSkills.Any(s => s.Power == SNOPower.Wizard_Disintegrate && s.RuneIndex == 2);

                float disintegrateRange = hasEntropy ? 10f : 35f;
                // Disintegrate
                if (!useOocBuff && !Player.IsIncapacitated && CombatBase.CanCast(SNOPower.Wizard_Disintegrate) &&
                    ((Player.PrimaryResource >= 20 && !Player.WaitingForReserveEnergy) || Player.PrimaryResource >= MinEnergyReserve))
                {
                    return new TrinityPower(SNOPower.Wizard_Disintegrate, disintegrateRange, Vector3.Zero, -1, CurrentTarget.ACDGuid, 0, 0);
                }
                // Arcane Orb
                if (!useOocBuff && !Player.IsIncapacitated && CombatBase.CanCast(SNOPower.Wizard_ArcaneOrb) &&
                    ((Player.PrimaryResource >= 30 && !Player.WaitingForReserveEnergy) || Player.PrimaryResource >= MinEnergyReserve))
                {
                    return new TrinityPower(SNOPower.Wizard_ArcaneOrb, 35f, CurrentTarget.ACDGuid);
                }
                // Arcane Torrent
                if (!useOocBuff && !Player.IsIncapacitated && CombatBase.CanCast(SNOPower.Wizard_ArcaneTorrent) &&
                    ((Player.PrimaryResource >= 16 && !Player.WaitingForReserveEnergy) || Player.PrimaryResource >= MinEnergyReserve))
                {
                    return new TrinityPower(SNOPower.Wizard_ArcaneTorrent, 40f, Vector3.Zero, -1, CurrentTarget.ACDGuid, 0, 0);
                }

                //skillDict.Add("RayOfFrost", SNOPower.Wizard_RayOfFrost);
                //runeDict.Add("Numb", 2);
                //runeDict.Add("SnowBlast", 0);
                //runeDict.Add("ColdBlood", 3);
                //runeDict.Add("SleetStorm", 1);
                //runeDict.Add("BlackIce", 4);

                bool hasSleetStorm = HotbarSkills.AssignedSkills.Any(s => s.Power == SNOPower.Wizard_RayOfFrost && s.RuneIndex == 1);

                // Ray of Frost
                if (!useOocBuff && !isCurrentlyAvoiding && !Player.IsIncapacitated && Hotbar.Contains(SNOPower.Wizard_RayOfFrost) &&
                    Player.PrimaryResource >= 12 && !Player.WaitingForReserveEnergy)
                {
                    float range = 50f;
                    if (hasSleetStorm)
                        range = 5f;

                    return new TrinityPower(SNOPower.Wizard_RayOfFrost, range, Vector3.Zero, -1, CurrentTarget.ACDGuid, 0, 1);
                }

                bool hasConflagrate = HotbarSkills.AssignedSkills.Any(s => s.Power == SNOPower.Wizard_MagicMissile && s.RuneIndex == 2);

                // Magic Missile
                if (!useOocBuff && !isCurrentlyAvoiding && Hotbar.Contains(SNOPower.Wizard_MagicMissile))
                {
                    var bestPierceTarget = TargetUtil.GetBestPierceTarget(45f);
                    int targetId;

                    if (bestPierceTarget != null)
                        targetId = hasConflagrate ?
                            bestPierceTarget.ACDGuid :
                            CurrentTarget.ACDGuid;
                    else
                        targetId = CurrentTarget.ACDGuid;

                    return new TrinityPower(SNOPower.Wizard_MagicMissile, 45f, targetId);
                }
                // Shock Pulse
                if (!useOocBuff && !isCurrentlyAvoiding && Hotbar.Contains(SNOPower.Wizard_ShockPulse))
                {
                    return new TrinityPower(SNOPower.Wizard_ShockPulse, 15f, Vector3.Zero, -1, CurrentTarget.ACDGuid, 0, 1);
                }
                // Spectral Blade
                if (!useOocBuff && !isCurrentlyAvoiding && Hotbar.Contains(SNOPower.Wizard_SpectralBlade))
                {
                    return new TrinityPower(SNOPower.Wizard_SpectralBlade, 14f, Vector3.Zero, -1, CurrentTarget.ACDGuid, 0, 1);
                }
                // Electrocute
                if (!useOocBuff && !isCurrentlyAvoiding && Hotbar.Contains(SNOPower.Wizard_Electrocute))
                {
                    return new TrinityPower(SNOPower.Wizard_Electrocute, 40f, Vector3.Zero, -1, CurrentTarget.ACDGuid, 0, 0);
                }
                // Default attacks
                return CombatBase.DefaultPower;

            }
            else
            {
                bool cancelArchon = false;
                string reason = "";

                if (Settings.Combat.Wizard.ArchonCancelOption == WizardArchonCancelOption.RebuffArmor && !Wizard_HasWizardArmor())
                {
                    reason += "Rebuff Armor ";
                    cancelArchon = true;
                }
                if (Settings.Combat.Wizard.ArchonCancelOption == WizardArchonCancelOption.RebuffMagicWeaponFamiliar &&
                    (!CheckAbilityAndBuff(SNOPower.Wizard_MagicWeapon) || !Wizard_HasFamiliar()))
                {
                    if (!CheckAbilityAndBuff(SNOPower.Wizard_MagicWeapon))
                        reason += "Rebuff Magic Weapon ";
                    if (!Wizard_HasFamiliar())
                        reason += "Rebuff Familiar ";
                    cancelArchon = true;
                }

                if (Settings.Combat.Wizard.ArchonCancelOption == WizardArchonCancelOption.Timer &&
                    DateTime.UtcNow.Subtract(CacheData.AbilityLastUsed[SNOPower.Wizard_Archon]).TotalSeconds >= Settings.Combat.Wizard.ArchonCancelSeconds)
                {
                    reason += "Timer";
                    cancelArchon = true;
                }

                if (cancelArchon && Wizard_ShouldStartArchon())
                {
                    var archonBuff = ZetaDia.Me.GetBuff(SNOPower.Wizard_Archon);
                    if (archonBuff != null && archonBuff.IsCancelable)
                    {
                        Logger.Log(TrinityLogLevel.Debug, LogCategory.Behavior, "Canceling Archon: {0}", reason);
                        // this actually cancels Archon
                        archonBuff.Cancel();

                        // this SNOPower is fake - it isn't actually used, we're just putting it here to force a BehaviorTree return/recheck
                        return new TrinityPower(SNOPower.Wizard_Archon_Cancel, 0f, Vector3.Zero, -1, -1, -1, -1);
                    }
                }

                // Archon form
                // Archon Slow Time for in combat
                if (!useOocBuff && !Player.IsIncapacitated &&
                    (TargetUtil.AnyElitesInRange(25, 1) || TargetUtil.EliteOrTrashInRange(25f) || (CurrentTarget.IsBossOrEliteRareUnique && CurrentTarget.RadiusDistance <= 35f)) &&
                    CombatBase.CanCast(SNOPower.Wizard_Archon_SlowTime, CombatBase.CanCastFlags.NoTimer) &&
                    (CombatBase.TimeSpanSincePowerUse(SNOPower.Wizard_Archon_SlowTime) > TimeSpan.FromSeconds(15)))
                {
                    return new TrinityPower(SNOPower.Wizard_Archon_SlowTime, 0f, Vector3.Zero, CurrentWorldDynamicId, -1, 1, 1);
                }

                // Archon Teleport in combat for kiting
                if (!useOocBuff && !isCurrentlyAvoiding && !Player.IsIncapacitated && CombatBase.CanCast(SNOPower.Wizard_Archon_Teleport, CombatBase.CanCastFlags.NoTimer) &&
                    Settings.Combat.Wizard.KiteLimit > 0 &&
                    // Try and teleport-retreat from 1 elite or 3+ greys or a boss at 15 foot range
                    (TargetUtil.AnyElitesInRange(15, 1) || TargetUtil.AnyMobsInRange(15, 3) || (CurrentTarget.IsBoss && CurrentTarget.RadiusDistance <= 15f)))
                {
                    Vector3 vNewTarget = MathEx.CalculatePointFrom(CurrentTarget.Position, Player.Position, -20f);
                    return new TrinityPower(SNOPower.Wizard_Archon_Teleport, 35f, vNewTarget);
                }

                // Archon teleport in combat for no-kite
                if (!useOocBuff && !isCurrentlyAvoiding && !Player.IsIncapacitated && CombatBase.CanCast(SNOPower.Wizard_Archon_Teleport, CombatBase.CanCastFlags.NoTimer) &&
                    Settings.Combat.Wizard.KiteLimit == 0 && CurrentTarget.RadiusDistance >= 10f)
                {
                    return new TrinityPower(SNOPower.Wizard_Archon_Teleport, 35f, CurrentTarget.Position);
                }
                // Arcane Blast - 2 second cooldown, big AoE
                if (!useOocBuff && !Player.IsIncapacitated && CombatBase.CanCast(SNOPower.Wizard_Archon_ArcaneBlast, CombatBase.CanCastFlags.NoTimer) && TargetUtil.AnyMobsInRange(15, 1) && CurrentTarget.IsFacingPlayer)
                {
                    return new TrinityPower(SNOPower.Wizard_Archon_ArcaneBlast, 0f, Vector3.Zero, CurrentWorldDynamicId, -1, 1, 1);
                }

                // Disintegrate
                if (!useOocBuff && !isCurrentlyAvoiding && !Player.IsIncapacitated && (CurrentTarget.CountUnitsBehind(25f) > 2 || Settings.Combat.Wizard.NoArcaneStrike || Settings.Combat.Wizard.KiteLimit > 0))
                {
                    return new TrinityPower(SNOPower.Wizard_Archon_DisintegrationWave, 49f, Vector3.Zero, -1, CurrentTarget.ACDGuid, 0, 0);
                }

                // Arcane Strike Rapid Spam at close-range only
                if (!useOocBuff && !Player.IsIncapacitated && !Settings.Combat.Wizard.NoArcaneStrike)
                {
                    return new TrinityPower(SNOPower.Wizard_Archon_ArcaneStrike, 7f, Vector3.Zero, -1, CurrentTarget.ACDGuid, 1, 1);
                }

                // Disintegrate as final option just in case
                if (!useOocBuff && !isCurrentlyAvoiding && !Player.IsIncapacitated)
                {
                    return new TrinityPower(SNOPower.Wizard_Archon_DisintegrationWave, 49f, Vector3.Zero, -1, CurrentTarget.ACDGuid, 0, 0);
                }

                return new TrinityPower(SNOPower.None, -1, Vector3.Zero, -1, -1, 0, 0);
            }
        }

        private static bool Wizard_ShouldStartArchon()
        {
            bool elitesOnly = Settings.Combat.Wizard.ArchonElitesOnly && TargetUtil.AnyElitesInRange(Settings.Combat.Wizard.ArchonEliteDistance);
            bool trashInRange = !Settings.Combat.Wizard.ArchonElitesOnly && TargetUtil.AnyMobsInRange(Settings.Combat.Wizard.ArchonMobDistance, Settings.Combat.Wizard.ArchonMobCount);

            return elitesOnly || trashInRange;
        }

        private static bool Wizard_HasWizardArmor()
        {
            return (GetHasBuff(SNOPower.Wizard_EnergyArmor) || GetHasBuff(SNOPower.Wizard_IceArmor) || GetHasBuff(SNOPower.Wizard_StormArmor));
        }

        private static bool Wizard_HasFamiliar()
        {
            double timeSinceDeath = DateTime.UtcNow.Subtract(LastDeathTime).TotalMilliseconds;

            // We've died, no longer have familiar
            if (timeSinceDeath < CombatBase.TimeSincePowerUse(SNOPower.Wizard_Familiar))
                return false;

            // we've used it within the last 5 minutes, we should still have it
            if (CombatBase.TimeSincePowerUse(SNOPower.Wizard_Familiar) < (5 * 60 * 1000))
                return true;

            return false;
        }

        private static TrinityPower GetWizardDestructablePower()
        {
            if (Hotbar.Contains(SNOPower.Wizard_WaveOfForce) && Player.PrimaryResource >= 25)
                return new TrinityPower(SNOPower.Wizard_WaveOfForce, 9f);

            if (Hotbar.Contains(SNOPower.Wizard_EnergyTwister) && Player.PrimaryResource >= 35)
                return new TrinityPower(SNOPower.Wizard_EnergyTwister, 9f);

            if (Hotbar.Contains(SNOPower.Wizard_ArcaneOrb))
                return new TrinityPower(SNOPower.Wizard_ArcaneOrb, 35f);

            if (Hotbar.Contains(SNOPower.Wizard_MagicMissile))
                return new TrinityPower(SNOPower.Wizard_MagicMissile, 15f);

            if (Hotbar.Contains(SNOPower.Wizard_ShockPulse))
                return new TrinityPower(SNOPower.Wizard_ShockPulse, 10f);

            if (Hotbar.Contains(SNOPower.Wizard_SpectralBlade))
                return new TrinityPower(SNOPower.Wizard_SpectralBlade, 5f);

            if (Hotbar.Contains(SNOPower.Wizard_Electrocute))
                return new TrinityPower(SNOPower.Wizard_Electrocute, 9f);

            if (Hotbar.Contains(SNOPower.Wizard_ArcaneTorrent))
                return new TrinityPower(SNOPower.Wizard_ArcaneTorrent, 9f);

            if (Hotbar.Contains(SNOPower.Wizard_Blizzard))
                return new TrinityPower(SNOPower.Wizard_Blizzard, 9f);

            return CombatBase.DefaultPower;
        }



    }
}
