using System;
using Zeta;
using Zeta.Common;
using Zeta.Common.Plugins;
using Zeta.CommonBot;
using Zeta.Internals.Actors;
namespace GilesTrinity
{
    public partial class GilesTrinity : IPlugin
    {
        private static TrinityPower GetWizardPower(bool bCurrentlyAvoiding, bool bOOCBuff, bool bDestructiblePower)
        {
            // Pick the best destructible power available
            if (bDestructiblePower)
            {
                if (!GetHasBuff(SNOPower.Wizard_Archon))
                {
                    return GetWizardDestructablePower();
                }
                else
                {
                    if (CurrentTarget.RadiusDistance <= 10f)
                        return new TrinityPower(SNOPower.Wizard_Archon_ArcaneStrike, 20f, vNullLocation, -1, CurrentTarget.ACDGuid, 0, 0, USE_SLOWLY);
                    return new TrinityPower(SNOPower.Wizard_Archon_DisintegrationWave, 19f, vNullLocation, -1, CurrentTarget.ACDGuid, 0, 0, USE_SLOWLY);
                }
            }
            // Wizards want to save up to a reserve of 65+ energy
            MinEnergyReserve = 65;
            if (!GetHasBuff(SNOPower.Wizard_Archon))
            {
                // Slow time, for if being followed
                if (bOOCBuff && !playerStatus.IsIncapacitated && Hotbar.Contains(SNOPower.Wizard_SlowTime) &&
                    GilesUseTimer(SNOPower.Wizard_SlowTime, true) && PowerManager.CanCast(SNOPower.Wizard_SlowTime))
                {
                    return new TrinityPower(SNOPower.Wizard_SlowTime, 0f, vNullLocation, iCurrentWorldID, -1, 1, 1, USE_SLOWLY);
                }
                // Slow Time for in combat
                if (!bOOCBuff && !playerStatus.IsIncapacitated && Hotbar.Contains(SNOPower.Wizard_SlowTime) &&
                    (ElitesWithinRange[RANGE_25] > 0 || iAnythingWithinRange[RANGE_25] > 1 || playerStatus.CurrentHealthPct <= 0.7 || ((CurrentTarget.IsEliteRareUnique || CurrentTarget.IsTreasureGoblin || CurrentTarget.IsBoss) && CurrentTarget.RadiusDistance <= 35f)) &&
                    PowerManager.CanCast(SNOPower.Wizard_SlowTime))
                {
                    return new TrinityPower(SNOPower.Wizard_SlowTime, 0f, vNullLocation, iCurrentWorldID, -1, 1, 1, USE_SLOWLY);
                }
                // Wave of force
                if (!bOOCBuff && !playerStatus.IsIncapacitated && playerStatus.CurrentEnergy >= 25 &&
                    (
                    // Check this isn't a critical mass wizard, cos they won't want to use this except for low health unless they don't have nova/blast in which case go for it
                    (Settings.Combat.Wizard.CriticalMass && ((!Hotbar.Contains(SNOPower.Wizard_FrostNova) && !Hotbar.Contains(SNOPower.Wizard_ExplosiveBlast)) ||
                        (playerStatus.CurrentHealthPct <= 0.7 && (ElitesWithinRange[RANGE_15] > 0 || iAnythingWithinRange[RANGE_15] > 0 || (CurrentTarget.IsBossOrEliteRareUnique && CurrentTarget.RadiusDistance <= 23f)))))
                    // Else normal wizard in which case check standard stuff
                    || (!Settings.Combat.Wizard.CriticalMass && ElitesWithinRange[RANGE_15] > 0 || iAnythingWithinRange[RANGE_15] > 3 || playerStatus.CurrentHealthPct <= 0.7 || (CurrentTarget.IsBossOrEliteRareUnique && CurrentTarget.RadiusDistance <= 23f))
                    ) &&
                    Hotbar.Contains(SNOPower.Wizard_WaveOfForce) &&
                    GilesUseTimer(SNOPower.Wizard_WaveOfForce, true) && PowerManager.CanCast(SNOPower.Wizard_WaveOfForce))
                {
                    return new TrinityPower(SNOPower.Wizard_WaveOfForce, 0f, vNullLocation, iCurrentWorldID, -1, 1, 2, USE_SLOWLY);
                }
                // Blizzard
                if (!bOOCBuff && !playerStatus.IsIncapacitated && Hotbar.Contains(SNOPower.Wizard_Blizzard) &&
                    powerLastSnoPowerUsed != SNOPower.Wizard_Blizzard &&
                    (ElitesWithinRange[RANGE_25] > 0 || iAnythingWithinRange[RANGE_25] > 2 || CurrentTarget.IsEliteRareUnique || CurrentTarget.IsTreasureGoblin || CurrentTarget.IsBoss || playerStatus.CurrentHealthPct <= 0.7) &&
                    playerStatus.CurrentEnergy >= 40 && GilesUseTimer(SNOPower.Wizard_Blizzard))
                {
                    Vector3 targetDirection = MathEx.CalculatePointFrom(playerStatus.CurrentPosition, CurrentTarget.Position, 1f);

                    ZetaDia.Me.UsePower(SNOPower.Walk, targetDirection);
                    return new TrinityPower(SNOPower.Wizard_Blizzard, 40f, new Vector3(CurrentTarget.Position.X, CurrentTarget.Position.Y, CurrentTarget.Position.Z), iCurrentWorldID, -1, 1, 1, USE_SLOWLY);
                }
                // Meteor
                if (!bOOCBuff && !playerStatus.IsIncapacitated && Hotbar.Contains(SNOPower.Wizard_Meteor) &&
                    (ElitesWithinRange[RANGE_25] > 0 || iAnythingWithinRange[RANGE_25] > 2 || CurrentTarget.IsEliteRareUnique || CurrentTarget.IsBoss || CurrentTarget.IsTreasureGoblin) &&
                    playerStatus.CurrentEnergy >= 50 && PowerManager.CanCast(SNOPower.Wizard_Meteor))
                {
                    return new TrinityPower(SNOPower.Wizard_Meteor, 21f, new Vector3(CurrentTarget.Position.X, CurrentTarget.Position.Y, CurrentTarget.Position.Z), iCurrentWorldID, -1, 1, 2, USE_SLOWLY);
                }
                // Teleport in combat for critical-mass wizards
                if (!bOOCBuff && !bCurrentlyAvoiding && !playerStatus.IsIncapacitated && Hotbar.Contains(SNOPower.Wizard_Teleport) && Settings.Combat.Wizard.CriticalMass &&
                    powerLastSnoPowerUsed != SNOPower.Wizard_Teleport &&
                    playerStatus.CurrentEnergy >= 15 && CurrentTarget.CentreDistance <= 35f &&
                    PowerManager.CanCast(SNOPower.Wizard_Teleport))
                {
                    vSideToSideTarget = FindZigZagTargetLocation(CurrentTarget.Position, CurrentTarget.CentreDistance, true);
                    return new TrinityPower(SNOPower.Wizard_Teleport, 35f, vSideToSideTarget, iCurrentWorldID, -1, 1, 2, USE_SLOWLY);
                }
                // Diamond Skin SPAM
                if (Hotbar.Contains(SNOPower.Wizard_DiamondSkin) && powerLastSnoPowerUsed != SNOPower.Wizard_DiamondSkin &&
                    (ElitesWithinRange[RANGE_25] > 0 || iAnythingWithinRange[RANGE_25] > 0 || playerStatus.CurrentHealthPct <= 0.90 || playerStatus.IsIncapacitated || playerStatus.IsRooted || (!bOOCBuff && CurrentTarget.RadiusDistance <= 40f)) &&
                    ((Settings.Combat.Wizard.CriticalMass && !bOOCBuff) || !GetHasBuff(SNOPower.Wizard_DiamondSkin)) &&
                    PowerManager.CanCast(SNOPower.Wizard_DiamondSkin))
                {
                    return new TrinityPower(SNOPower.Wizard_DiamondSkin, 0f, vNullLocation, iCurrentWorldID, -1, 0, 1, USE_SLOWLY);
                }
                // The three wizard armors, done in an else-if loop so it doesn't keep replacing one with the other
                if (!playerStatus.IsIncapacitated && playerStatus.CurrentEnergy >= 25)
                {
                    // Energy armor as priority cast if available and not buffed
                    if (Hotbar.Contains(SNOPower.Wizard_EnergyArmor))
                    {
                        if ((!GetHasBuff(SNOPower.Wizard_EnergyArmor) && PowerManager.CanCast(SNOPower.Wizard_EnergyArmor)) || (Hotbar.Contains(SNOPower.Wizard_Archon) && (!GetHasBuff(SNOPower.Wizard_EnergyArmor) || GilesUseTimer(SNOPower.Wizard_EnergyArmor))))
                        {
                            return new TrinityPower(SNOPower.Wizard_EnergyArmor, 0f, vNullLocation, iCurrentWorldID, -1, 1, 2, USE_SLOWLY);
                        }
                    }
                    // Ice Armor
                    else if (Hotbar.Contains(SNOPower.Wizard_IceArmor))
                    {
                        if (!GetHasBuff(SNOPower.Wizard_IceArmor) && PowerManager.CanCast(SNOPower.Wizard_IceArmor))
                        {
                            return new TrinityPower(SNOPower.Wizard_IceArmor, 0f, vNullLocation, iCurrentWorldID, -1, 1, 2, USE_SLOWLY);
                        }
                    }
                    // Storm Armor
                    else if (Hotbar.Contains(SNOPower.Wizard_StormArmor))
                    {
                        if (!GetHasBuff(SNOPower.Wizard_StormArmor) || ((DateTime.Now.Subtract(dictAbilityLastUse[SNOPower.Wizard_StormArmor]).TotalMilliseconds >= 15000) && PowerManager.CanCast(SNOPower.Wizard_Archon)))
                        {
                            return new TrinityPower(SNOPower.Wizard_StormArmor, 0f, vNullLocation, iCurrentWorldID, -1, 1, 2, USE_SLOWLY);
                        }
                    }
                }
                // Magic Weapon                        
                if (!playerStatus.IsIncapacitated && Hotbar.Contains(SNOPower.Wizard_MagicWeapon) && PowerManager.CanCast(SNOPower.Wizard_MagicWeapon) &&
                    (!GetHasBuff(SNOPower.Wizard_MagicWeapon) || ((DateTime.Now.Subtract(dictAbilityLastUse[SNOPower.Wizard_MagicWeapon]).TotalMilliseconds >= 10000) && PowerManager.CanCast(SNOPower.Wizard_Archon))))
                {
                    return new TrinityPower(SNOPower.Wizard_MagicWeapon, 0f, vNullLocation, iCurrentWorldID, -1, 1, 2, USE_SLOWLY);
                }
                // Familiar
                // Magic Weapon
                if (!playerStatus.IsIncapacitated && Hotbar.Contains(SNOPower.Wizard_MagicWeapon) &&
                    playerStatus.CurrentEnergy >= 25 && (GilesUseTimer(SNOPower.Wizard_MagicWeapon) || !GetHasBuff(SNOPower.Wizard_MagicWeapon)))
                {
                    return new TrinityPower(SNOPower.Wizard_MagicWeapon, 0f, vNullLocation, iCurrentWorldID, -1, 1, 2, USE_SLOWLY);
                }
                // Familiar
                if (!playerStatus.IsIncapacitated && Hotbar.Contains(SNOPower.Wizard_Familiar) &&
                    playerStatus.CurrentEnergy >= 25 && GilesUseTimer(SNOPower.Wizard_Familiar))
                {
                    return new TrinityPower(SNOPower.Wizard_Familiar, 0f, vNullLocation, iCurrentWorldID, -1, 1, 2, USE_SLOWLY);
                }
                // Hydra
                if (!bOOCBuff && !playerStatus.IsIncapacitated &&
                    powerLastSnoPowerUsed != SNOPower.Wizard_Hydra &&
                    (ElitesWithinRange[RANGE_15] > 0 || iAnythingWithinRange[RANGE_15] > 4 || playerStatus.CurrentHealthPct <= 0.7 || ((CurrentTarget.IsEliteRareUnique || CurrentTarget.IsBoss || CurrentTarget.IsTreasureGoblin) && CurrentTarget.RadiusDistance <= 15f)) &&
                    Hotbar.Contains(SNOPower.Wizard_Hydra) &&
                    playerStatus.CurrentEnergy >= 15 && GilesUseTimer(SNOPower.Wizard_Hydra))
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
                    Vector3 vNewTarget = MathEx.CalculatePointFrom(CurrentTarget.Position, playerStatus.CurrentPosition, CurrentTarget.CentreDistance - fExtraDistance);
                    return new TrinityPower(SNOPower.Wizard_Hydra, 30f, vNewTarget, iCurrentWorldID, -1, 1, 2, USE_SLOWLY);
                }
                // Mirror Image  @ half health or 5+ monsters or rooted/incapacitated or last elite left @25% health
                if (!bOOCBuff && Hotbar.Contains(SNOPower.Wizard_MirrorImage) &&
                    (playerStatus.CurrentHealthPct <= 0.50 || iAnythingWithinRange[RANGE_30] >= 5 || playerStatus.IsIncapacitated || playerStatus.IsRooted || (ElitesWithinRange[RANGE_30] == 1 && CurrentTarget.IsEliteRareUnique && !CurrentTarget.IsBoss && CurrentTarget.HitPoints <= 0.35)) &&
                    PowerManager.CanCast(SNOPower.Wizard_MirrorImage))
                {
                    return new TrinityPower(SNOPower.Wizard_MirrorImage, 0f, vNullLocation, iCurrentWorldID, -1, 1, 1, USE_SLOWLY);
                }
                // Archon
                if (!bOOCBuff && !bCurrentlyAvoiding && Hotbar.Contains(SNOPower.Wizard_Archon) &&
                    (ElitesWithinRange[RANGE_30] >= 1 || iAnythingWithinRange[RANGE_25] >= 3 || (CurrentTarget.IsBossOrEliteRareUnique && CurrentTarget.RadiusDistance <= 30f)) &&
                    playerStatus.CurrentEnergy >= 25 && playerStatus.CurrentHealthPct >= 0.10 &&
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
                    if (!bHasBuffAbilities || playerStatus.CurrentEnergy <= iExtraEnergyNeeded)
                        CanCastArchon = true;
                    if (!CanCastArchon)
                    {
                        dictAbilityLastUse[SNOPower.Wizard_MagicWeapon] = DateTime.Today;
                        //dictAbilityLastUse[SNOPower.Wizard_Familiar] = DateTime.Today;
                        dictAbilityLastUse[SNOPower.Wizard_EnergyArmor] = DateTime.Today;
                        dictAbilityLastUse[SNOPower.Wizard_IceArmor] = DateTime.Today;
                        dictAbilityLastUse[SNOPower.Wizard_StormArmor] = DateTime.Today;
                        CanCastArchon = true;
                    }
                    else
                    {
                        CanCastArchon = false;
                        return new TrinityPower(SNOPower.Wizard_Archon, 0f, vNullLocation, iCurrentWorldID, -1, 4, 5, USE_SLOWLY);
                    }
                }
                // Frost Nova SPAM
                if (!bOOCBuff && Hotbar.Contains(SNOPower.Wizard_FrostNova) && !playerStatus.IsIncapacitated &&
                    ((ElitesWithinRange[RANGE_25] > 0 || iAnythingWithinRange[RANGE_25] > 0 || playerStatus.CurrentHealthPct <= 0.7) && CurrentTarget.RadiusDistance <= 12f) &&
                    PowerManager.CanCast(SNOPower.Wizard_FrostNova))
                {
                    float fThisRange = 14f;
                    if (Settings.Combat.Wizard.CriticalMass)
                        fThisRange = 9f;
                    return new TrinityPower(SNOPower.Wizard_FrostNova, fThisRange, vNullLocation, iCurrentWorldID, -1, 0, 0, USE_SLOWLY);
                }
                // Explosive Blast SPAM when enough AP, blow erry thing up, nah mean
                if (!bOOCBuff && Hotbar.Contains(SNOPower.Wizard_ExplosiveBlast) && !playerStatus.IsIncapacitated && playerStatus.CurrentEnergy >= 20 &&
                    ((ElitesWithinRange[RANGE_25] >= 1 || iAnythingWithinRange[RANGE_25] >= 1 || playerStatus.CurrentHealthPct <= 0.7) && CurrentTarget.RadiusDistance <= 12f) &&
                    PowerManager.CanCast(SNOPower.Wizard_ExplosiveBlast))
                {
                    float fThisRange = 11f;
                    if (Settings.Combat.Wizard.CriticalMass)
                        fThisRange = 9f;
                    return new TrinityPower(SNOPower.Wizard_ExplosiveBlast, fThisRange, vNullLocation, iCurrentWorldID, -1, 0, 0, USE_SLOWLY);
                }

                // Check to see if we have a signature spell on our hotbar, for energy twister check
                bool bHasSignatureSpell = (Hotbar.Contains(SNOPower.Wizard_MagicMissile) || Hotbar.Contains(SNOPower.Wizard_ShockPulse) ||
                    Hotbar.Contains(SNOPower.Wizard_SpectralBlade) || Hotbar.Contains(SNOPower.Wizard_Electrocute));


                // Energy Twister SPAMS whenever 35 or more ap to generate Arcane Power
                if (!bOOCBuff && !playerStatus.IsIncapacitated && Hotbar.Contains(SNOPower.Wizard_EnergyTwister) && playerStatus.CurrentEnergy >= 35 &&
                    // If using storm chaser, then force a signature spell every 1 stack of the buff, if we have a signature spell
                    (!bHasSignatureSpell || GetBuffStacks(SNOPower.Wizard_EnergyTwister) < 1) &&
                    (ElitesWithinRange[RANGE_30] >= 1 || iAnythingWithinRange[RANGE_25] >= 1 || CurrentTarget.RadiusDistance <= 12f) &&
                    (!Hotbar.Contains(SNOPower.Wizard_Electrocute) || !hashActorSNOFastMobs.Contains(CurrentTarget.ActorSNO)) &&
                    ((Settings.Combat.Wizard.CriticalMass && !bHasSignatureSpell) || !Settings.Combat.Wizard.CriticalMass))
                {
                    Vector3 targetDirection = MathEx.CalculatePointFrom(playerStatus.CurrentPosition, CurrentTarget.Position, 1f);
                    ZetaDia.Me.UsePower(SNOPower.Walk, targetDirection);
                    float fThisRange = 28f;
                    //if (Settings.Combat.Wizard.CriticalMass)
                    //    fThisRange = 9f;
                    return new TrinityPower(SNOPower.Wizard_EnergyTwister, fThisRange, CurrentTarget.Position, iCurrentWorldID, -1, 0, 0, USE_SLOWLY);
                }



                // Disintegrate
                if (!bOOCBuff && !playerStatus.IsIncapacitated && Hotbar.Contains(SNOPower.Wizard_Disintegrate) &&
                    ((playerStatus.CurrentEnergy >= 20 && !playerStatus.WaitingForReserveEnergy) || playerStatus.CurrentEnergy >= MinEnergyReserve))
                {
                    float fThisRange = 35f;
                    if (Settings.Combat.Wizard.CriticalMass)
                        fThisRange = 20f;
                    return new TrinityPower(SNOPower.Wizard_Disintegrate, fThisRange, vNullLocation, -1, CurrentTarget.ACDGuid, 0, 0, SIGNATURE_SPAM);
                }
                // Arcane Orb
                if (!bOOCBuff && !playerStatus.IsIncapacitated && Hotbar.Contains(SNOPower.Wizard_ArcaneOrb) &&
                    ((playerStatus.CurrentEnergy >= 35 && !playerStatus.WaitingForReserveEnergy) || playerStatus.CurrentEnergy >= MinEnergyReserve) &&
                    GilesUseTimer(SNOPower.Wizard_ArcaneOrb))
                {
                    float fThisRange = 40f;
                    if (Settings.Combat.Wizard.CriticalMass)
                        fThisRange = 20f;
                    return new TrinityPower(SNOPower.Wizard_ArcaneOrb, fThisRange, vNullLocation, -1, CurrentTarget.ACDGuid, 1, 1, USE_SLOWLY);
                }
                // Arcane Torrent
                if (!bOOCBuff && !playerStatus.IsIncapacitated && Hotbar.Contains(SNOPower.Wizard_ArcaneTorrent) &&
                    ((playerStatus.CurrentEnergy >= 16 && !playerStatus.WaitingForReserveEnergy) || playerStatus.CurrentEnergy >= MinEnergyReserve) &&
                    GilesUseTimer(SNOPower.Wizard_ArcaneTorrent))
                {
                    float fThisRange = 40f;
                    /*if (Settings.Combat.Wizard.CriticalMass)
                        fThisRange = 20f;*/
                    return new TrinityPower(SNOPower.Wizard_ArcaneTorrent, fThisRange, vNullLocation, -1, CurrentTarget.ACDGuid, 0, 0, USE_SLOWLY);
                }
                // Ray of Frost
                if (!bOOCBuff && !bCurrentlyAvoiding && !playerStatus.IsIncapacitated && Hotbar.Contains(SNOPower.Wizard_RayOfFrost) &&
                    playerStatus.CurrentEnergy >= 12)
                {
                    float fThisRange = 35f;
                    if (Settings.Combat.Wizard.CriticalMass)
                        fThisRange = 20f;
                    return new TrinityPower(SNOPower.Wizard_RayOfFrost, fThisRange, vNullLocation, -1, CurrentTarget.ACDGuid, 0, 0, SIGNATURE_SPAM);
                }
                // Magic Missile
                if (!bOOCBuff && !bCurrentlyAvoiding && Hotbar.Contains(SNOPower.Wizard_MagicMissile))
                {
                    float fThisRange = 35f;
                    if (Settings.Combat.Wizard.CriticalMass)
                        fThisRange = 20f;
                    return new TrinityPower(SNOPower.Wizard_MagicMissile, fThisRange, vNullLocation, -1, CurrentTarget.ACDGuid, 0, 0, USE_SLOWLY);
                }
                // Shock Pulse
                if (!bOOCBuff && !bCurrentlyAvoiding && Hotbar.Contains(SNOPower.Wizard_ShockPulse))
                {
                    return new TrinityPower(SNOPower.Wizard_ShockPulse, 15f, vNullLocation, -1, CurrentTarget.ACDGuid, 0, 1, USE_SLOWLY);
                }
                // Spectral Blade
                if (!bOOCBuff && !bCurrentlyAvoiding && Hotbar.Contains(SNOPower.Wizard_SpectralBlade))
                {
                    return new TrinityPower(SNOPower.Wizard_SpectralBlade, 14f, vNullLocation, -1, CurrentTarget.ACDGuid, 0, 1, USE_SLOWLY);
                }
                // Electrocute
                if (!bOOCBuff && !bCurrentlyAvoiding && Hotbar.Contains(SNOPower.Wizard_Electrocute))
                {
                    return new TrinityPower(SNOPower.Wizard_Electrocute, 40f, vNullLocation, -1, CurrentTarget.ACDGuid, 0, 0, USE_SLOWLY);
                }
                // Default attacks
                if (!bOOCBuff && !bCurrentlyAvoiding && !playerStatus.IsIncapacitated)
                {
                    return new TrinityPower(SNOPower.Weapon_Melee_Instant, 10f, vNullLocation, -1, CurrentTarget.ACDGuid, 1, 1, USE_SLOWLY);
                }
                return new TrinityPower(SNOPower.Weapon_Melee_Instant, 10f, vNullLocation, -1, -1, 0, 0, USE_SLOWLY);
            }
            else
            {
                // Archon form
                // Archon Slow Time for in combat
                if (!bOOCBuff && !playerStatus.IsIncapacitated &&
                    (ElitesWithinRange[RANGE_25] > 0 || iAnythingWithinRange[RANGE_25] > 1 || playerStatus.CurrentHealthPct <= 0.7 || ((CurrentTarget.IsEliteRareUnique || CurrentTarget.IsTreasureGoblin || CurrentTarget.IsBoss) && CurrentTarget.RadiusDistance <= 35f)) &&
                    Hotbar.Contains(SNOPower.Wizard_Archon_SlowTime) &&
                    GilesUseTimer(SNOPower.Wizard_Archon_SlowTime, true) && PowerManager.CanCast(SNOPower.Wizard_Archon_SlowTime))
                {
                    return new TrinityPower(SNOPower.Wizard_Archon_SlowTime, 0f, vNullLocation, iCurrentWorldID, -1, 1, 1, USE_SLOWLY);
                }
                // Archon Teleport in combat
                if (!bOOCBuff && !bCurrentlyAvoiding && !playerStatus.IsIncapacitated && Hotbar.Contains(SNOPower.Wizard_Archon_Teleport) &&
                    // Try and teleport-retreat from 1 elite or 3+ greys or a boss at 15 foot range
                    (ElitesWithinRange[RANGE_15] >= 1 || iAnythingWithinRange[RANGE_15] >= 3 || (CurrentTarget.IsBoss && CurrentTarget.RadiusDistance <= 15f)) &&
                    GilesUseTimer(SNOPower.Wizard_Archon_Teleport) && PowerManager.CanCast(SNOPower.Wizard_Archon_Teleport))
                {
                    Vector3 vNewTarget = MathEx.CalculatePointFrom(CurrentTarget.Position, playerStatus.CurrentPosition, -20f);
                    return new TrinityPower(SNOPower.Wizard_Archon_Teleport, 35f, vNewTarget, iCurrentWorldID, -1, 1, 1, USE_SLOWLY);
                }
                // Arcane Blast
                if (!bOOCBuff && !playerStatus.IsIncapacitated &&
                    (ElitesWithinRange[RANGE_15] >= 1 || iAnythingWithinRange[RANGE_15] >= 1 ||
                     (CurrentTarget.IsBossOrEliteRareUnique && CurrentTarget.RadiusDistance <= 15f)) &&
                    GilesUseTimer(SNOPower.Wizard_Archon_ArcaneBlast) && PowerManager.CanCast(SNOPower.Wizard_Archon_ArcaneBlast))
                {
                    return new TrinityPower(SNOPower.Wizard_Archon_ArcaneBlast, 0f, vNullLocation, iCurrentWorldID, -1, 1, 1, USE_SLOWLY);
                }
                // Arcane Strike (Arcane Strike) Rapid Spam at close-range only
                if (!bOOCBuff && !playerStatus.IsIncapacitated && CurrentTarget.RadiusDistance <= 7f &&
                    CurrentTarget.IsBossOrEliteRareUnique)
                {
                    return new TrinityPower(SNOPower.Wizard_Archon_ArcaneStrike, 7f, vNullLocation, -1, CurrentTarget.ACDGuid, 1, 1, USE_SLOWLY);
                }
                // Disintegrate
                if (!bOOCBuff && !bCurrentlyAvoiding && !playerStatus.IsIncapacitated)
                {
                    return new TrinityPower(SNOPower.Wizard_Archon_DisintegrationWave, 49f, vNullLocation, -1, CurrentTarget.ACDGuid, 0, 0, SIGNATURE_SPAM);
                }
                return defaultPower;
            }
        }

        private static TrinityPower GetWizardDestructablePower()
        {
            if (Hotbar.Contains(SNOPower.Wizard_EnergyTwister) && playerStatus.CurrentEnergy >= 35)
                return new TrinityPower(SNOPower.Wizard_EnergyTwister, 9f, vNullLocation, -1, -1, 0, 0, USE_SLOWLY);

            if (Hotbar.Contains(SNOPower.Wizard_MagicMissile))
                return new TrinityPower(SNOPower.Wizard_MagicMissile, 15f, vNullLocation, -1, -1, 0, 0, USE_SLOWLY);

            if (Hotbar.Contains(SNOPower.Wizard_ShockPulse))
                return new TrinityPower(SNOPower.Wizard_ShockPulse, 10f, vNullLocation, -1, -1, 0, 0, USE_SLOWLY);

            if (Hotbar.Contains(SNOPower.Wizard_SpectralBlade))
                return new TrinityPower(SNOPower.Wizard_SpectralBlade, 9f, vNullLocation, -1, -1, 0, 0, USE_SLOWLY);

            if (Hotbar.Contains(SNOPower.Wizard_Electrocute))
                return new TrinityPower(SNOPower.Wizard_Electrocute, 9f, vNullLocation, -1, -1, 0, 0, USE_SLOWLY);

            return new TrinityPower(SNOPower.Weapon_Melee_Instant, 10f, vNullLocation, -1, -1, 0, 0, USE_SLOWLY);
        }

    }
}
