using System;
using System.Collections.Generic;
using System.Linq;
using Trinity.Technicals;
using Zeta;
using Zeta.Common;
using Zeta.Common.Plugins;
using Zeta.CommonBot;
using Zeta.Internals;
using Zeta.Internals.Actors;
using Trinity.Config.Combat;
using Trinity.Combat.Abilities;

namespace Trinity
{

    public partial class Trinity : IPlugin
    {
        private static TrinityPower GetWizardPower(bool IsCurrentlyAvoiding, bool UseOOCBuff, bool UseDestructiblePower)
        {
            // TODO
            //- AI so Trinity knows that it is already in the densest monster area and not to teleport.
            //- AI to use teleport when ranged attacks are approaching (succubus slow moving torpedo-orb)
            //- AI to avoid AoE's (plague, molten, desecrator)
            //- For Trinity to cast one or two Twisters before moving into melee range or teleporting closer. 

            // Pick the best destructible power available
            if (UseDestructiblePower)
            {
                if (!GetHasBuff(SNOPower.Wizard_Archon))
                {
                    return GetWizardDestructablePower();
                }
                else
                {
                    if (CurrentTarget.RadiusDistance <= 10f)
                        return new TrinityPower(SNOPower.Wizard_Archon_ArcaneStrike, 20f, Vector3.Zero, -1, CurrentTarget.ACDGuid, 0, 0, WAIT_FOR_ANIM);
                    return new TrinityPower(SNOPower.Wizard_Archon_DisintegrationWave, 19f, Vector3.Zero, -1, CurrentTarget.ACDGuid, 0, 0, WAIT_FOR_ANIM);
                }
            }
            // Wizards want to save up to a reserve of 65+ energy
            MinEnergyReserve = 65;

            bool hasCriticalMass = ZetaDia.CPlayer.PassiveSkills.Contains(SNOPower.Wizard_Passive_CriticalMass);

            if (!GetHasBuff(SNOPower.Wizard_Archon))
            {

                // Slow Time for in combat
                if (!UseOOCBuff && !PlayerStatus.IsIncapacitated && Hotbar.Contains(SNOPower.Wizard_SlowTime) &&
                    (ElitesWithinRange[RANGE_25] > 0 || AnythingWithinRange[RANGE_25] > 1 || PlayerStatus.CurrentHealthPct <= 0.7 || ((CurrentTarget.IsEliteRareUnique || CurrentTarget.IsTreasureGoblin || CurrentTarget.IsBoss) && CurrentTarget.RadiusDistance <= 35f)) &&
                    PowerManager.CanCast(SNOPower.Wizard_SlowTime))
                {
                    return new TrinityPower(SNOPower.Wizard_SlowTime, 0f, Vector3.Zero, CurrentWorldDynamicId, -1, 1, 1, WAIT_FOR_ANIM);
                }
                // Wave of force
                if (!UseOOCBuff && !PlayerStatus.IsIncapacitated && PlayerStatus.PrimaryResource >= 25 &&
                    (
                    // Check this isn't a critical mass wizard, cos they won't want to use this except for low health unless they don't have nova/blast in which case go for it
                    (hasCriticalMass && ((!Hotbar.Contains(SNOPower.Wizard_FrostNova) && !Hotbar.Contains(SNOPower.Wizard_ExplosiveBlast)) ||
                        (PlayerStatus.CurrentHealthPct <= 0.7 && (TargetUtil.AnyMobsInRange(15f) || TargetUtil.IsEliteTargetInRange(23f)))))
                    // Else normal wizard in which case check standard stuff
                    || (!hasCriticalMass && ElitesWithinRange[RANGE_15] > 0 || AnythingWithinRange[RANGE_15] > 3 || PlayerStatus.CurrentHealthPct <= 0.7 || (CurrentTarget.IsBossOrEliteRareUnique && CurrentTarget.RadiusDistance <= 23f))
                    ) &&
                    Hotbar.Contains(SNOPower.Wizard_WaveOfForce) &&
                    SNOPowerUseTimer(SNOPower.Wizard_WaveOfForce, true) && PowerManager.CanCast(SNOPower.Wizard_WaveOfForce))
                {
                    return new TrinityPower(SNOPower.Wizard_WaveOfForce, 0f, Vector3.Zero, CurrentWorldDynamicId, -1, 1, 2, WAIT_FOR_ANIM);
                }

                //SkillDict.Add("Blizzard", SNOPower.Wizard_Blizzard);
                //RuneDict.Add("GraspingChill", 2);
                //RuneDict.Add("FrozenSolid", 4);
                //RuneDict.Add("Snowbound", 3);
                //RuneDict.Add("StarkWinter", 1);
                //RuneDict.Add("UnrelentingStorm", 0);

                bool hasSnowBoundRune = HotbarSkills.AssignedSkills.Any(s => s.Power == SNOPower.Wizard_Blizzard && s.RuneIndex == 3);

                // Blizzard
                if (!UseOOCBuff && !PlayerStatus.IsIncapacitated && Hotbar.Contains(SNOPower.Wizard_Blizzard) &&
                    (TargetUtil.ClusterExists(45f, 2) || TargetUtil.AnyElitesInRange(40f) || TargetUtil.IsEliteTargetInRange(45f)) &&
                    (PlayerStatus.PrimaryResource >= 40 || (hasSnowBoundRune && PlayerStatus.PrimaryResource >= 20)) && SNOPowerUseTimer(SNOPower.Wizard_Blizzard))
                {
                    var bestClusterPoint = TargetUtil.GetBestClusterPoint(18f, 45f);
                    return new TrinityPower(SNOPower.Wizard_Blizzard, 45f, bestClusterPoint, CurrentWorldDynamicId, -1, 1, 1, WAIT_FOR_ANIM);
                }
                // Meteor
                if (!UseOOCBuff && !PlayerStatus.IsIncapacitated && Hotbar.Contains(SNOPower.Wizard_Meteor) &&
                    (ElitesWithinRange[RANGE_25] > 0 || AnythingWithinRange[RANGE_25] > 2 || CurrentTarget.IsEliteRareUnique || CurrentTarget.IsBoss || CurrentTarget.IsTreasureGoblin) &&
                    PlayerStatus.PrimaryResource >= 50 && PowerManager.CanCast(SNOPower.Wizard_Meteor))
                {
                    return new TrinityPower(SNOPower.Wizard_Meteor, 21f, new Vector3(CurrentTarget.Position.X, CurrentTarget.Position.Y, CurrentTarget.Position.Z), CurrentWorldDynamicId, -1, 1, 2, WAIT_FOR_ANIM);
                }
                // Teleport in combat for critical-mass wizards
                if (!UseOOCBuff && !IsCurrentlyAvoiding && !PlayerStatus.IsIncapacitated && Hotbar.Contains(SNOPower.Wizard_Teleport) && hasCriticalMass &&
                    LastPowerUsed != SNOPower.Wizard_Teleport &&
                    PlayerStatus.PrimaryResource >= 15 && CurrentTarget.CentreDistance <= 35f &&
                    PowerManager.CanCast(SNOPower.Wizard_Teleport))
                {
                    vSideToSideTarget = TargetUtil.GetBestClusterPoint(15f, 35f);
                    return new TrinityPower(SNOPower.Wizard_Teleport, 35f, vSideToSideTarget, CurrentWorldDynamicId, -1, 1, 2, WAIT_FOR_ANIM);
                }

                // Diamond Skin SPAM
                if (Hotbar.Contains(SNOPower.Wizard_DiamondSkin) && LastPowerUsed != SNOPower.Wizard_DiamondSkin &&
                    (ElitesWithinRange[RANGE_25] > 0 || AnythingWithinRange[RANGE_25] > 0 || PlayerStatus.CurrentHealthPct <= 0.90 || PlayerStatus.IsIncapacitated || PlayerStatus.IsRooted || (!UseOOCBuff && CurrentTarget.RadiusDistance <= 40f)) &&
                    ((hasCriticalMass && !UseOOCBuff) || !GetHasBuff(SNOPower.Wizard_DiamondSkin)) &&
                    PowerManager.CanCast(SNOPower.Wizard_DiamondSkin))
                {
                    return new TrinityPower(SNOPower.Wizard_DiamondSkin, 0f, Vector3.Zero, CurrentWorldDynamicId, -1, 0, 1, WAIT_FOR_ANIM);
                }
                // Familiar
                if (!PlayerStatus.IsIncapacitated && Hotbar.Contains(SNOPower.Wizard_Familiar) &&
                    PlayerStatus.PrimaryResource >= 20 && SNOPowerUseTimer(SNOPower.Wizard_Familiar))
                {
                    return new TrinityPower(SNOPower.Wizard_Familiar, 0f, Vector3.Zero, CurrentWorldDynamicId, -1, 1, 2, WAIT_FOR_ANIM);
                }

                // The three wizard armors, done in an else-if loop so it doesn't keep replacing one with the other
                if (!PlayerStatus.IsIncapacitated && PlayerStatus.PrimaryResource >= 25)
                {
                    // Energy armor as priority cast if available and not buffed
                    if (Hotbar.Contains(SNOPower.Wizard_EnergyArmor))
                    {
                        if ((!GetHasBuff(SNOPower.Wizard_EnergyArmor) && PowerManager.CanCast(SNOPower.Wizard_EnergyArmor)) || (Hotbar.Contains(SNOPower.Wizard_Archon) && (!GetHasBuff(SNOPower.Wizard_EnergyArmor) || SNOPowerUseTimer(SNOPower.Wizard_EnergyArmor))))
                        {
                            return new TrinityPower(SNOPower.Wizard_EnergyArmor, 0f, Vector3.Zero, CurrentWorldDynamicId, -1, 1, 2, WAIT_FOR_ANIM);
                        }
                    }
                    // Ice Armor
                    else if (Hotbar.Contains(SNOPower.Wizard_IceArmor))
                    {
                        if (!GetHasBuff(SNOPower.Wizard_IceArmor) && PowerManager.CanCast(SNOPower.Wizard_IceArmor))
                        {
                            return new TrinityPower(SNOPower.Wizard_IceArmor, 0f, Vector3.Zero, CurrentWorldDynamicId, -1, 1, 2, WAIT_FOR_ANIM);
                        }
                    }
                    // Storm Armor
                    else if (Hotbar.Contains(SNOPower.Wizard_StormArmor))
                    {
                        if (!GetHasBuff(SNOPower.Wizard_StormArmor) || ((DateTime.Now.Subtract(AbilityLastUsedCache[SNOPower.Wizard_StormArmor]).TotalMilliseconds >= 15000) && PowerManager.CanCast(SNOPower.Wizard_Archon)))
                        {
                            return new TrinityPower(SNOPower.Wizard_StormArmor, 0f, Vector3.Zero, CurrentWorldDynamicId, -1, 1, 2, WAIT_FOR_ANIM);
                        }
                    }
                }
                // Magic Weapon                        
                if (!PlayerStatus.IsIncapacitated && Hotbar.Contains(SNOPower.Wizard_MagicWeapon) && PowerManager.CanCast(SNOPower.Wizard_MagicWeapon) &&
                    (!GetHasBuff(SNOPower.Wizard_MagicWeapon) || ((DateTime.Now.Subtract(AbilityLastUsedCache[SNOPower.Wizard_MagicWeapon]).TotalMilliseconds >= 10000) && PowerManager.CanCast(SNOPower.Wizard_Archon))))
                {
                    return new TrinityPower(SNOPower.Wizard_MagicWeapon, 0f, Vector3.Zero, CurrentWorldDynamicId, -1, 1, 2, WAIT_FOR_ANIM);
                }
                // Magic Weapon
                if (!PlayerStatus.IsIncapacitated && Hotbar.Contains(SNOPower.Wizard_MagicWeapon) &&
                    PlayerStatus.PrimaryResource >= 25 && (SNOPowerUseTimer(SNOPower.Wizard_MagicWeapon) || !GetHasBuff(SNOPower.Wizard_MagicWeapon)))
                {
                    return new TrinityPower(SNOPower.Wizard_MagicWeapon, 0f, Vector3.Zero, CurrentWorldDynamicId, -1, 1, 2, WAIT_FOR_ANIM);
                }
                // Hydra
                if (!UseOOCBuff && !PlayerStatus.IsIncapacitated &&
                    LastPowerUsed != SNOPower.Wizard_Hydra &&
                    (ElitesWithinRange[RANGE_15] > 0 || AnythingWithinRange[RANGE_15] > 4 || PlayerStatus.CurrentHealthPct <= 0.7 || ((CurrentTarget.IsEliteRareUnique || CurrentTarget.IsBoss || CurrentTarget.IsTreasureGoblin) && CurrentTarget.RadiusDistance <= 15f)) &&
                    Hotbar.Contains(SNOPower.Wizard_Hydra) &&
                    PlayerStatus.PrimaryResource >= 15 && SNOPowerUseTimer(SNOPower.Wizard_Hydra))
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
                    Vector3 vNewTarget = MathEx.CalculatePointFrom(CurrentTarget.Position, PlayerStatus.CurrentPosition, CurrentTarget.CentreDistance - fExtraDistance);
                    return new TrinityPower(SNOPower.Wizard_Hydra, 30f, vNewTarget, CurrentWorldDynamicId, -1, 1, 2, WAIT_FOR_ANIM);
                }
                // Mirror Image  @ half health or 5+ monsters or rooted/incapacitated or last elite left @25% health
                if (!UseOOCBuff && Hotbar.Contains(SNOPower.Wizard_MirrorImage) &&
                    (PlayerStatus.CurrentHealthPct <= 0.50 || AnythingWithinRange[RANGE_30] >= 5 || PlayerStatus.IsIncapacitated || PlayerStatus.IsRooted || (ElitesWithinRange[RANGE_30] == 1 && CurrentTarget.IsEliteRareUnique && !CurrentTarget.IsBoss && CurrentTarget.HitPointsPct <= 0.35)) &&
                    PowerManager.CanCast(SNOPower.Wizard_MirrorImage))
                {
                    return new TrinityPower(SNOPower.Wizard_MirrorImage, 0f, Vector3.Zero, CurrentWorldDynamicId, -1, 1, 1, WAIT_FOR_ANIM);
                }
                // Archon
                if (!UseOOCBuff && !IsCurrentlyAvoiding && Hotbar.Contains(SNOPower.Wizard_Archon) && Wizard_ShouldStartArchon() &&
                    PowerManager.CanCast(SNOPower.Wizard_Archon))
                {
                    // Familiar has been removed for now. Uncomment the three comments below relating to familiars to force re-buffing them
                    bool bHasBuffAbilities = (Hotbar.Contains(SNOPower.Wizard_MagicWeapon) ||
                        //hashPowerHotbarAbilities.Contains(SNOPower.Wizard_Familiar) ||
                        Hotbar.Contains(SNOPower.Wizard_EnergyArmor) || Hotbar.Contains(SNOPower.Wizard_IceArmor) ||
                        Hotbar.Contains(SNOPower.Wizard_StormArmor));
                    int iExtraEnergyNeeded = 25;
                    if (Hotbar.Contains(SNOPower.Wizard_MagicWeapon)) iExtraEnergyNeeded += 25;
                    //if (hashPowerHotbarAbilities.Contains(SNOPower.Wizard_Familiar)) iExtraEnergyNeeded += 25;
                    if (Hotbar.Contains(SNOPower.Wizard_EnergyArmor) || Hotbar.Contains(SNOPower.Wizard_IceArmor) ||
                        Hotbar.Contains(SNOPower.Wizard_StormArmor)) iExtraEnergyNeeded += 25;
                    if (!bHasBuffAbilities || PlayerStatus.PrimaryResource <= iExtraEnergyNeeded)
                        CanCastArchon = true;
                    if (!CanCastArchon)
                    {
                        AbilityLastUsedCache[SNOPower.Wizard_MagicWeapon] = DateTime.Today;
                        //dictAbilityLastUse[SNOPower.Wizard_Familiar] = DateTime.Today;
                        AbilityLastUsedCache[SNOPower.Wizard_EnergyArmor] = DateTime.Today;
                        AbilityLastUsedCache[SNOPower.Wizard_IceArmor] = DateTime.Today;
                        AbilityLastUsedCache[SNOPower.Wizard_StormArmor] = DateTime.Today;
                        CanCastArchon = true;
                    }
                    else
                    {
                        CanCastArchon = false;
                        return new TrinityPower(SNOPower.Wizard_Archon, 0f, Vector3.Zero, CurrentWorldDynamicId, -1, 4, 5, WAIT_FOR_ANIM);
                    }
                }

                //SkillDict.Add("FrostNova", SNOPower.Wizard_FrostNova);
                //RuneDict.Add("Shatter", 1);
                //RuneDict.Add("ColdSnap", 3);
                //RuneDict.Add("FrozenMist", 2);
                //RuneDict.Add("DeepFreeze", 4);
                //RuneDict.Add("BoneChill", 0);

                bool hasDeepFreeze = HotbarSkills.AssignedSkills.Any(s => s.Power == SNOPower.Wizard_FrostNova && s.RuneIndex == 4);

                // Frost Nova
                if (!UseOOCBuff && Hotbar.Contains(SNOPower.Wizard_FrostNova) && !PlayerStatus.IsIncapacitated &&
                    ((hasDeepFreeze && TargetUtil.AnyMobsInRange(25, 5)) || (!hasDeepFreeze && (TargetUtil.AnyMobsInRange(25, 1) || PlayerStatus.CurrentHealthPct <= 0.7)) &&
                    CurrentTarget.RadiusDistance <= 25f) &&
                    PowerManager.CanCast(SNOPower.Wizard_FrostNova))
                {
                    return new TrinityPower(SNOPower.Wizard_FrostNova, 20f, Vector3.Zero, CurrentWorldDynamicId, -1, 0, 0, WAIT_FOR_ANIM);
                }

                // Frost Nova for Critical Mass builds
                if (!UseOOCBuff && Hotbar.Contains(SNOPower.Wizard_FrostNova) && !PlayerStatus.IsIncapacitated &&
                    hasCriticalMass && TargetUtil.AnyMobsInRange(20, 1) && PowerManager.CanCast(SNOPower.Wizard_FrostNova))
                {
                    return new TrinityPower(SNOPower.Wizard_FrostNova, 15f, Vector3.Zero, CurrentWorldDynamicId, -1, 0, 0, WAIT_FOR_ANIM);
                }

                // Explosive Blast SPAM when enough AP, blow erry thing up, nah mean
                if (!UseOOCBuff && Hotbar.Contains(SNOPower.Wizard_ExplosiveBlast) && !PlayerStatus.IsIncapacitated && PlayerStatus.PrimaryResource >= 20 &&
                    (TargetUtil.AnyMobsInRange(25) && CurrentTarget.RadiusDistance <= 25f) &&
                    PowerManager.CanCast(SNOPower.Wizard_ExplosiveBlast))
                {
                    float fThisRange = 11f;
                    if (hasCriticalMass)
                        fThisRange = 9f;
                    return new TrinityPower(SNOPower.Wizard_ExplosiveBlast, fThisRange, Vector3.Zero, CurrentWorldDynamicId, -1, 0, 0, WAIT_FOR_ANIM);
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
                if (!UseOOCBuff && !PlayerStatus.IsIncapacitated && Hotbar.Contains(SNOPower.Wizard_EnergyTwister) &&
                    PlayerStatus.PrimaryResource >= 35 &&
                    // If using storm chaser, then force a signature spell every 1 stack of the buff, if we have a signature spell
                    (!bHasSignatureSpell || GetBuffStacks(SNOPower.Wizard_EnergyTwister) < 1) &&
                    ((!hasWickedWindRune && CurrentTarget.RadiusDistance <= 25f) ||
                    (hasWickedWindRune && CurrentTarget.RadiusDistance <= 60f)) &&
                    (!Hotbar.Contains(SNOPower.Wizard_Electrocute) || !DataDictionary.FastMovingMonsterIds.Contains(CurrentTarget.ActorSNO)) &&
                    ((hasCriticalMass && !bHasSignatureSpell) || !hasCriticalMass))
                {
                    Vector3 targetDirection = MathEx.CalculatePointFrom(PlayerStatus.CurrentPosition, CurrentTarget.Position, 3f);

                    Vector3 bestClusterPoint = TargetUtil.GetBestClusterPoint(10f, 15f);

                    float twisterRange = 28f;
                    if (hasCriticalMass)
                        twisterRange = 9f;
                    return new TrinityPower(SNOPower.Wizard_EnergyTwister, twisterRange, bestClusterPoint, CurrentWorldDynamicId, -1, 0, 0, WAIT_FOR_ANIM);
                }

                // Disintegrate
                if (!UseOOCBuff && !PlayerStatus.IsIncapacitated && Hotbar.Contains(SNOPower.Wizard_Disintegrate) &&
                    ((PlayerStatus.PrimaryResource >= 20 && !PlayerStatus.WaitingForReserveEnergy) || PlayerStatus.PrimaryResource >= MinEnergyReserve))
                {
                    float fThisRange = 35f;
                    if (hasCriticalMass)
                        fThisRange = 20f;
                    return new TrinityPower(SNOPower.Wizard_Disintegrate, fThisRange, Vector3.Zero, -1, CurrentTarget.ACDGuid, 0, 0, NO_WAIT_ANIM);
                }
                // Arcane Orb
                if (!UseOOCBuff && !PlayerStatus.IsIncapacitated && Hotbar.Contains(SNOPower.Wizard_ArcaneOrb) &&
                    ((PlayerStatus.PrimaryResource >= 35 && !PlayerStatus.WaitingForReserveEnergy) || PlayerStatus.PrimaryResource >= MinEnergyReserve) &&
                    SNOPowerUseTimer(SNOPower.Wizard_ArcaneOrb))
                {
                    float fThisRange = 40f;
                    if (hasCriticalMass)
                        fThisRange = 20f;
                    return new TrinityPower(SNOPower.Wizard_ArcaneOrb, fThisRange, Vector3.Zero, -1, CurrentTarget.ACDGuid, 1, 1, WAIT_FOR_ANIM);
                }
                // Arcane Torrent
                if (!UseOOCBuff && !PlayerStatus.IsIncapacitated && Hotbar.Contains(SNOPower.Wizard_ArcaneTorrent) &&
                    ((PlayerStatus.PrimaryResource >= 16 && !PlayerStatus.WaitingForReserveEnergy) || PlayerStatus.PrimaryResource >= MinEnergyReserve) &&
                    SNOPowerUseTimer(SNOPower.Wizard_ArcaneTorrent))
                {
                    float fThisRange = 40f;
                    /*if (hasCriticalMass)
                        fThisRange = 20f;*/
                    return new TrinityPower(SNOPower.Wizard_ArcaneTorrent, fThisRange, Vector3.Zero, -1, CurrentTarget.ACDGuid, 0, 0, WAIT_FOR_ANIM);
                }
                // Ray of Frost
                if (!UseOOCBuff && !IsCurrentlyAvoiding && !PlayerStatus.IsIncapacitated && Hotbar.Contains(SNOPower.Wizard_RayOfFrost) &&
                    PlayerStatus.PrimaryResource >= 12)
                {
                    float fThisRange = 35f;
                    if (hasCriticalMass)
                        fThisRange = 20f;
                    return new TrinityPower(SNOPower.Wizard_RayOfFrost, fThisRange, Vector3.Zero, -1, CurrentTarget.ACDGuid, 0, 0, NO_WAIT_ANIM);
                }
                // Magic Missile
                if (!UseOOCBuff && !IsCurrentlyAvoiding && Hotbar.Contains(SNOPower.Wizard_MagicMissile))
                {
                    float fThisRange = 35f;
                    if (hasCriticalMass)
                        fThisRange = 20f;
                    return new TrinityPower(SNOPower.Wizard_MagicMissile, fThisRange, Vector3.Zero, -1, CurrentTarget.ACDGuid, 0, 0, WAIT_FOR_ANIM);
                }
                // Shock Pulse
                if (!UseOOCBuff && !IsCurrentlyAvoiding && Hotbar.Contains(SNOPower.Wizard_ShockPulse))
                {
                    return new TrinityPower(SNOPower.Wizard_ShockPulse, 15f, Vector3.Zero, -1, CurrentTarget.ACDGuid, 0, 1, WAIT_FOR_ANIM);
                }
                // Spectral Blade
                if (!UseOOCBuff && !IsCurrentlyAvoiding && Hotbar.Contains(SNOPower.Wizard_SpectralBlade))
                {
                    return new TrinityPower(SNOPower.Wizard_SpectralBlade, 14f, Vector3.Zero, -1, CurrentTarget.ACDGuid, 0, 1, WAIT_FOR_ANIM);
                }
                // Electrocute
                if (!UseOOCBuff && !IsCurrentlyAvoiding && Hotbar.Contains(SNOPower.Wizard_Electrocute))
                {
                    return new TrinityPower(SNOPower.Wizard_Electrocute, 40f, Vector3.Zero, -1, CurrentTarget.ACDGuid, 0, 0, WAIT_FOR_ANIM);
                }
                // Default attacks
                return CombatBase.DefaultPower;

            }
            else
            {
                bool cancelArchon = false;

                if (Settings.Combat.Wizard.ArchonCancelOption == WizardArchonCancelOption.RebuffArmor && !Wizard_HasWizardArmor())
                    cancelArchon = true;

                if (Settings.Combat.Wizard.ArchonCancelOption == WizardArchonCancelOption.RebuffMagicWeaponFamiliar &&
                    (!CheckAbilityAndBuff(SNOPower.Wizard_MagicWeapon) || !CheckAbilityAndBuff(SNOPower.Wizard_Familiar)))
                    cancelArchon = true;

                if (Settings.Combat.Wizard.ArchonCancelOption == WizardArchonCancelOption.Timer &&
                    DateTime.Now.Subtract(AbilityLastUsedCache[SNOPower.Wizard_Archon]).TotalSeconds >= Settings.Combat.Wizard.ArchonCancelSeconds)
                    cancelArchon = true;

                if (cancelArchon && Wizard_ShouldStartArchon())
                {


                    var archonBuff = ZetaDia.Me.GetBuff(SNOPower.Wizard_Archon);
                    if (archonBuff != null && archonBuff.IsCancelable)
                    {
                        // this actually cancels Archon
                        archonBuff.Cancel();

                        // this SNOPower is fake - it isn't actually used, we're just putting it here to force a BehaviorTree return/recheck
                        return new TrinityPower(SNOPower.Wizard_Archon_Cancel, 0f, Vector3.Zero, -1, -1, -1, -1, false);
                    }
                }

                // Archon form
                // Archon Slow Time for in combat
                if (!UseOOCBuff && !PlayerStatus.IsIncapacitated &&
                    (ElitesWithinRange[RANGE_25] > 0 || AnythingWithinRange[RANGE_25] > 1 || PlayerStatus.CurrentHealthPct <= 0.7 || ((CurrentTarget.IsEliteRareUnique || CurrentTarget.IsTreasureGoblin || CurrentTarget.IsBoss) && CurrentTarget.RadiusDistance <= 35f)) &&
                    Hotbar.Contains(SNOPower.Wizard_Archon_SlowTime) &&
                    SNOPowerUseTimer(SNOPower.Wizard_Archon_SlowTime, true) && PowerManager.CanCast(SNOPower.Wizard_Archon_SlowTime))
                {
                    return new TrinityPower(SNOPower.Wizard_Archon_SlowTime, 0f, Vector3.Zero, CurrentWorldDynamicId, -1, 1, 1, WAIT_FOR_ANIM);
                }
                // Archon Teleport in combat
                if (!UseOOCBuff && !IsCurrentlyAvoiding && !PlayerStatus.IsIncapacitated && Hotbar.Contains(SNOPower.Wizard_Archon_Teleport) &&
                    // Try and teleport-retreat from 1 elite or 3+ greys or a boss at 15 foot range
                    (ElitesWithinRange[RANGE_15] >= 1 || AnythingWithinRange[RANGE_15] >= 3 || (CurrentTarget.IsBoss && CurrentTarget.RadiusDistance <= 15f)) &&
                    SNOPowerUseTimer(SNOPower.Wizard_Archon_Teleport) && PowerManager.CanCast(SNOPower.Wizard_Archon_Teleport))
                {
                    Vector3 vNewTarget = MathEx.CalculatePointFrom(CurrentTarget.Position, PlayerStatus.CurrentPosition, -20f);
                    return new TrinityPower(SNOPower.Wizard_Archon_Teleport, 35f, vNewTarget, CurrentWorldDynamicId, -1, 1, 1, WAIT_FOR_ANIM);
                }
                // Arcane Blast
                if (!UseOOCBuff && !PlayerStatus.IsIncapacitated &&
                    (ElitesWithinRange[RANGE_15] >= 1 || AnythingWithinRange[RANGE_15] >= 1 ||
                     (CurrentTarget.IsBossOrEliteRareUnique && CurrentTarget.RadiusDistance <= 15f)) &&
                    SNOPowerUseTimer(SNOPower.Wizard_Archon_ArcaneBlast) && PowerManager.CanCast(SNOPower.Wizard_Archon_ArcaneBlast))
                {
                    return new TrinityPower(SNOPower.Wizard_Archon_ArcaneBlast, 0f, Vector3.Zero, CurrentWorldDynamicId, -1, 1, 1, WAIT_FOR_ANIM);
                }
                // Arcane Strike (Arcane Strike) Rapid Spam at close-range only
                if (!UseOOCBuff && !PlayerStatus.IsIncapacitated && CurrentTarget.RadiusDistance <= 5f && TargetUtil.AnyMobsInRange(7f, 2) &&
                    CurrentTarget.IsBossOrEliteRareUnique && !Settings.Combat.Wizard.NoArcaneStrike)
                {
                    return new TrinityPower(SNOPower.Wizard_Archon_ArcaneStrike, 7f, Vector3.Zero, -1, CurrentTarget.ACDGuid, 1, 1, WAIT_FOR_ANIM);
                }
                // Disintegrate
                if (!UseOOCBuff && !IsCurrentlyAvoiding && !PlayerStatus.IsIncapacitated)
                {
                    return new TrinityPower(SNOPower.Wizard_Archon_DisintegrationWave, 49f, Vector3.Zero, -1, CurrentTarget.ACDGuid, 0, 0, NO_WAIT_ANIM);
                }
                return new TrinityPower(SNOPower.None, -1, Vector3.Zero, -1, -1, 0, 0, WAIT_FOR_ANIM);
            }
        }

        private static bool Wizard_ShouldStartArchon()
        {
            return (TargetUtil.AnyElitesInRange(30, 1) || TargetUtil.AnyMobsInRange(Settings.Combat.Wizard.ArchonMobDistance, Settings.Combat.Wizard.ArchonMobCount) ||
                    TargetUtil.IsEliteTargetInRange(30f)) &&
                    PlayerStatus.PrimaryResource >= 25 && PlayerStatus.CurrentHealthPct >= 0.10;
        }

        private static bool Wizard_HasWizardArmor()
        {
            return (GetHasBuff(SNOPower.Wizard_EnergyArmor) || GetHasBuff(SNOPower.Wizard_IceArmor) || GetHasBuff(SNOPower.Wizard_StormArmor));
        }

        private static TrinityPower GetWizardDestructablePower()
        {
            if (Hotbar.Contains(SNOPower.Wizard_EnergyTwister) && PlayerStatus.PrimaryResource >= 35)
                return new TrinityPower(SNOPower.Wizard_EnergyTwister, 9f, Vector3.Zero, -1, -1, 0, 0, WAIT_FOR_ANIM);

            if (Hotbar.Contains(SNOPower.Wizard_ArcaneOrb))
                return new TrinityPower(SNOPower.Wizard_ArcaneOrb, 40f, Vector3.Zero, -1, -1, 0, 0, WAIT_FOR_ANIM);

            if (Hotbar.Contains(SNOPower.Wizard_MagicMissile))
                return new TrinityPower(SNOPower.Wizard_MagicMissile, 15f, Vector3.Zero, -1, -1, 0, 0, WAIT_FOR_ANIM);

            if (Hotbar.Contains(SNOPower.Wizard_ShockPulse))
                return new TrinityPower(SNOPower.Wizard_ShockPulse, 10f, Vector3.Zero, -1, -1, 0, 0, WAIT_FOR_ANIM);

            if (Hotbar.Contains(SNOPower.Wizard_SpectralBlade))
                return new TrinityPower(SNOPower.Wizard_SpectralBlade, 9f, Vector3.Zero, -1, -1, 0, 0, WAIT_FOR_ANIM);

            if (Hotbar.Contains(SNOPower.Wizard_Electrocute))
                return new TrinityPower(SNOPower.Wizard_Electrocute, 9f, Vector3.Zero, -1, -1, 0, 0, WAIT_FOR_ANIM);

            return CombatBase.DefaultPower;
        }



    }
}
