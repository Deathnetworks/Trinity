using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Web;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using System.Windows.Markup;
using Zeta;
using Zeta.Common;
using Zeta.Common.Plugins;
using Zeta.CommonBot;
using Zeta.CommonBot.Profile;
using Zeta.CommonBot.Profile.Composites;
using Zeta.Internals;
using Zeta.Internals.Actors;
using Zeta.Internals.Actors.Gizmos;
using Zeta.Internals.SNO;
using Zeta.Navigation;
using Zeta.TreeSharp;
using Zeta.XmlEngine;
namespace GilesTrinity
{
    public partial class GilesTrinity : IPlugin
    {
        private static GilesPower GetWizardPower(bool bCurrentlyAvoiding, bool bOOCBuff, bool bDestructiblePower)
        {
            // Pick the best destructible power available
            if (bDestructiblePower)
            {
                if (!GilesHasBuff(SNOPower.Wizard_Archon))
                {
                    return GetWizardDestructablePower();
                }
                else
                {
                    if (CurrentTarget.fRadiusDistance <= 10f)
                        return new GilesPower(SNOPower.Wizard_Archon_ArcaneStrike, 20f, vNullLocation, -1, CurrentTarget.iACDGuid, 0, 0, USE_SLOWLY);
                    return new GilesPower(SNOPower.Wizard_Archon_DisintegrationWave, 19f, vNullLocation, -1, CurrentTarget.iACDGuid, 0, 0, USE_SLOWLY);
                }
            }
            // Wizards want to save up to a reserve of 65+ energy
            iWaitingReservedAmount = 65;
            if (!GilesHasBuff(SNOPower.Wizard_Archon))
            {
                // Slow time, for if being followed
                if (bOOCBuff && !playerStatus.bIsIncapacitated && hashPowerHotbarAbilities.Contains(SNOPower.Wizard_SlowTime) &&
                    GilesUseTimer(SNOPower.Wizard_SlowTime, true) && PowerManager.CanCast(SNOPower.Wizard_SlowTime))
                {
                    return new GilesPower(SNOPower.Wizard_SlowTime, 0f, vNullLocation, iCurrentWorldID, -1, 1, 1, USE_SLOWLY);
                }
                // Slow Time for in combat
                if (!bOOCBuff && !playerStatus.bIsIncapacitated && hashPowerHotbarAbilities.Contains(SNOPower.Wizard_SlowTime) &&
                    (iElitesWithinRange[RANGE_25] > 0 || iAnythingWithinRange[RANGE_25] > 1 || playerStatus.dCurrentHealthPct <= 0.7 || ((CurrentTarget.IsEliteRareUnique || CurrentTarget.bIsTreasureGoblin || CurrentTarget.IsBoss) && CurrentTarget.fRadiusDistance <= 35f)) &&
                    PowerManager.CanCast(SNOPower.Wizard_SlowTime))
                {
                    return new GilesPower(SNOPower.Wizard_SlowTime, 0f, vNullLocation, iCurrentWorldID, -1, 1, 1, USE_SLOWLY);
                }
                // Wave of force
                if (!bOOCBuff && !playerStatus.bIsIncapacitated && playerStatus.dCurrentEnergy >= 25 &&
                    (
                    // Check this isn't a critical mass wizard, cos they won't want to use this except for low health unless they don't have nova/blast in which case go for it
                    (settings.bEnableCriticalMass && ((!hashPowerHotbarAbilities.Contains(SNOPower.Wizard_FrostNova) && !hashPowerHotbarAbilities.Contains(SNOPower.Wizard_ExplosiveBlast)) ||
                        (playerStatus.dCurrentHealthPct <= 0.7 && (iElitesWithinRange[RANGE_15] > 0 || iAnythingWithinRange[RANGE_15] > 0 || ((CurrentTarget.IsEliteRareUnique || CurrentTarget.IsBoss) && CurrentTarget.fRadiusDistance <= 23f)))))
                    // Else normal wizard in which case check standard stuff
                    || (!settings.bEnableCriticalMass && iElitesWithinRange[RANGE_15] > 0 || iAnythingWithinRange[RANGE_15] > 3 || playerStatus.dCurrentHealthPct <= 0.7 || ((CurrentTarget.IsEliteRareUnique || CurrentTarget.IsBoss) && CurrentTarget.fRadiusDistance <= 23f))
                    ) &&
                    hashPowerHotbarAbilities.Contains(SNOPower.Wizard_WaveOfForce) &&
                    GilesUseTimer(SNOPower.Wizard_WaveOfForce, true) && PowerManager.CanCast(SNOPower.Wizard_WaveOfForce))
                {
                    return new GilesPower(SNOPower.Wizard_WaveOfForce, 0f, vNullLocation, iCurrentWorldID, -1, 1, 2, USE_SLOWLY);
                }
                // Blizzard
                if (!bOOCBuff && !playerStatus.bIsIncapacitated && hashPowerHotbarAbilities.Contains(SNOPower.Wizard_Blizzard) &&
                    powerLastSnoPowerUsed != SNOPower.Wizard_Blizzard &&
                    (iElitesWithinRange[RANGE_25] > 0 || iAnythingWithinRange[RANGE_25] > 2 || CurrentTarget.IsEliteRareUnique || CurrentTarget.bIsTreasureGoblin || CurrentTarget.IsBoss || playerStatus.dCurrentHealthPct <= 0.7) &&
                    playerStatus.dCurrentEnergy >= 40 && GilesUseTimer(SNOPower.Wizard_Blizzard))
                {
                    return new GilesPower(SNOPower.Wizard_Blizzard, 40f, new Vector3(CurrentTarget.vPosition.X, CurrentTarget.vPosition.Y, CurrentTarget.vPosition.Z), iCurrentWorldID, -1, 1, 1, USE_SLOWLY);
                }
                // Meteor
                if (!bOOCBuff && !playerStatus.bIsIncapacitated && hashPowerHotbarAbilities.Contains(SNOPower.Wizard_Meteor) &&
                    (iElitesWithinRange[RANGE_25] > 0 || iAnythingWithinRange[RANGE_25] > 2 || CurrentTarget.IsEliteRareUnique || CurrentTarget.IsBoss || CurrentTarget.bIsTreasureGoblin) &&
                    playerStatus.dCurrentEnergy >= 50 && PowerManager.CanCast(SNOPower.Wizard_Meteor))
                {
                    return new GilesPower(SNOPower.Wizard_Meteor, 21f, new Vector3(CurrentTarget.vPosition.X, CurrentTarget.vPosition.Y, CurrentTarget.vPosition.Z), iCurrentWorldID, -1, 1, 2, USE_SLOWLY);
                }
                // Teleport in combat for critical-mass wizards
                if (!bOOCBuff && !bCurrentlyAvoiding && !playerStatus.bIsIncapacitated && hashPowerHotbarAbilities.Contains(SNOPower.Wizard_Teleport) && settings.bEnableCriticalMass &&
                    powerLastSnoPowerUsed != SNOPower.Wizard_Teleport &&
                    playerStatus.dCurrentEnergy >= 15 && CurrentTarget.fCentreDist <= 35f &&
                    PowerManager.CanCast(SNOPower.Wizard_Teleport))
                {
                    vSideToSideTarget = FindZigZagTargetLocation(CurrentTarget.vPosition, CurrentTarget.fCentreDist, true);
                    return new GilesPower(SNOPower.Wizard_Teleport, 35f, vSideToSideTarget, iCurrentWorldID, -1, 1, 2, USE_SLOWLY);
                }
                // Diamond Skin SPAM
                if (hashPowerHotbarAbilities.Contains(SNOPower.Wizard_DiamondSkin) && powerLastSnoPowerUsed != SNOPower.Wizard_DiamondSkin &&
                    (iElitesWithinRange[RANGE_25] > 0 || iAnythingWithinRange[RANGE_25] > 0 || playerStatus.dCurrentHealthPct <= 0.90 || playerStatus.bIsIncapacitated || playerStatus.bIsRooted || (!bOOCBuff && CurrentTarget.fRadiusDistance <= 40f)) &&
                    ((settings.bEnableCriticalMass && !bOOCBuff) || !GilesHasBuff(SNOPower.Wizard_DiamondSkin)) &&
                    PowerManager.CanCast(SNOPower.Wizard_DiamondSkin))
                {
                    return new GilesPower(SNOPower.Wizard_DiamondSkin, 0f, vNullLocation, iCurrentWorldID, -1, 0, 1, USE_SLOWLY);
                }
                // The three wizard armors, done in an else-if loop so it doesn't keep replacing one with the other
                if (!playerStatus.bIsIncapacitated && playerStatus.dCurrentEnergy >= 25)
                {
                    // Energy armor as priority cast if available and not buffed
                    if (hashPowerHotbarAbilities.Contains(SNOPower.Wizard_EnergyArmor))
                    {
                        if ((!GilesHasBuff(SNOPower.Wizard_EnergyArmor) && PowerManager.CanCast(SNOPower.Wizard_EnergyArmor)) || (hashPowerHotbarAbilities.Contains(SNOPower.Wizard_Archon) && (!GilesHasBuff(SNOPower.Wizard_EnergyArmor) || GilesUseTimer(SNOPower.Wizard_EnergyArmor))))
                        {
                            return new GilesPower(SNOPower.Wizard_EnergyArmor, 0f, vNullLocation, iCurrentWorldID, -1, 1, 2, USE_SLOWLY);
                        }
                    }
                    // Ice Armor
                    else if (hashPowerHotbarAbilities.Contains(SNOPower.Wizard_IceArmor))
                    {
                        if (!GilesHasBuff(SNOPower.Wizard_IceArmor) && PowerManager.CanCast(SNOPower.Wizard_IceArmor))
                        {
                            return new GilesPower(SNOPower.Wizard_IceArmor, 0f, vNullLocation, iCurrentWorldID, -1, 1, 2, USE_SLOWLY);
                        }
                    }
                    // Storm Armor
                    else if (hashPowerHotbarAbilities.Contains(SNOPower.Wizard_StormArmor))
                    {
                        if (!GilesHasBuff(SNOPower.Wizard_StormArmor) || ((DateTime.Now.Subtract(dictAbilityLastUse[SNOPower.Wizard_StormArmor]).TotalMilliseconds >= 15000) && PowerManager.CanCast(SNOPower.Wizard_Archon)))
                        {
                            return new GilesPower(SNOPower.Wizard_StormArmor, 0f, vNullLocation, iCurrentWorldID, -1, 1, 2, USE_SLOWLY);
                        }
                    }
                }
                // Magic Weapon                        
                if (!playerStatus.bIsIncapacitated && hashPowerHotbarAbilities.Contains(SNOPower.Wizard_MagicWeapon) && PowerManager.CanCast(SNOPower.Wizard_MagicWeapon) &&
                    (!GilesHasBuff(SNOPower.Wizard_MagicWeapon) || ((DateTime.Now.Subtract(dictAbilityLastUse[SNOPower.Wizard_MagicWeapon]).TotalMilliseconds >= 10000) && PowerManager.CanCast(SNOPower.Wizard_Archon))))
                {
                    return new GilesPower(SNOPower.Wizard_MagicWeapon, 0f, vNullLocation, iCurrentWorldID, -1, 1, 2, USE_SLOWLY);
                }
                // Familiar
                // Magic Weapon
                if (!playerStatus.bIsIncapacitated && hashPowerHotbarAbilities.Contains(SNOPower.Wizard_MagicWeapon) &&
                    playerStatus.dCurrentEnergy >= 25 && (GilesUseTimer(SNOPower.Wizard_MagicWeapon) || !GilesHasBuff(SNOPower.Wizard_MagicWeapon)))
                {
                    return new GilesPower(SNOPower.Wizard_MagicWeapon, 0f, vNullLocation, iCurrentWorldID, -1, 1, 2, USE_SLOWLY);
                }
                // Familiar
                if (!playerStatus.bIsIncapacitated && hashPowerHotbarAbilities.Contains(SNOPower.Wizard_Familiar) &&
                    playerStatus.dCurrentEnergy >= 25 && GilesUseTimer(SNOPower.Wizard_Familiar))
                {
                    return new GilesPower(SNOPower.Wizard_Familiar, 0f, vNullLocation, iCurrentWorldID, -1, 1, 2, USE_SLOWLY);
                }
                // Hydra
                if (!bOOCBuff && !playerStatus.bIsIncapacitated &&
                    powerLastSnoPowerUsed != SNOPower.Wizard_Hydra &&
                    (iElitesWithinRange[RANGE_15] > 0 || iAnythingWithinRange[RANGE_15] > 4 || playerStatus.dCurrentHealthPct <= 0.7 || ((CurrentTarget.IsEliteRareUnique || CurrentTarget.IsBoss || CurrentTarget.bIsTreasureGoblin) && CurrentTarget.fRadiusDistance <= 15f)) &&
                    hashPowerHotbarAbilities.Contains(SNOPower.Wizard_Hydra) &&
                    playerStatus.dCurrentEnergy >= 15 && GilesUseTimer(SNOPower.Wizard_Hydra))
                {
                    // For distant monsters, try to target a little bit in-front of them (as they run towards us), if it's not a treasure goblin
                    float fExtraDistance = 0f;
                    if (CurrentTarget.fCentreDist > 17f && !CurrentTarget.bIsTreasureGoblin)
                    {
                        fExtraDistance = CurrentTarget.fCentreDist - 17f;
                        if (fExtraDistance > 5f)
                            fExtraDistance = 5f;
                        if (CurrentTarget.fCentreDist - fExtraDistance < 15f)
                            fExtraDistance -= 2;
                    }
                    Vector3 vNewTarget = MathEx.CalculatePointFrom(CurrentTarget.vPosition, playerStatus.vCurrentPosition, CurrentTarget.fCentreDist - fExtraDistance);
                    return new GilesPower(SNOPower.Wizard_Hydra, 30f, vNewTarget, iCurrentWorldID, -1, 1, 2, USE_SLOWLY);
                }
                // Mirror Image  @ half health or 5+ monsters or rooted/incapacitated or last elite left @25% health
                if (!bOOCBuff && hashPowerHotbarAbilities.Contains(SNOPower.Wizard_MirrorImage) &&
                    (playerStatus.dCurrentHealthPct <= 0.50 || iAnythingWithinRange[RANGE_30] >= 5 || playerStatus.bIsIncapacitated || playerStatus.bIsRooted || (iElitesWithinRange[RANGE_30] == 1 && CurrentTarget.IsEliteRareUnique && !CurrentTarget.IsBoss && CurrentTarget.iHitPoints <= 0.35)) &&
                    PowerManager.CanCast(SNOPower.Wizard_MirrorImage))
                {
                    return new GilesPower(SNOPower.Wizard_MirrorImage, 0f, vNullLocation, iCurrentWorldID, -1, 1, 1, USE_SLOWLY);
                }
                // Archon
                if (!bOOCBuff && !bCurrentlyAvoiding && hashPowerHotbarAbilities.Contains(SNOPower.Wizard_Archon) &&
                    (iElitesWithinRange[RANGE_30] >= 1 || iAnythingWithinRange[RANGE_25] >= 3 || ((CurrentTarget.IsEliteRareUnique || CurrentTarget.IsBoss) && CurrentTarget.fRadiusDistance <= 30f)) &&
                    playerStatus.dCurrentEnergy >= 25 && playerStatus.dCurrentHealthPct >= 0.10 &&
                    PowerManager.CanCast(SNOPower.Wizard_Archon))
                {
                    // Familiar has been removed for now. Uncomment the three comments below relating to familiars to force re-buffing them
                    bool bHasBuffAbilities = (hashPowerHotbarAbilities.Contains(SNOPower.Wizard_MagicWeapon) ||
                        //hashPowerHotbarAbilities.Contains(SNOPower.Wizard_Familiar) ||
                        hashPowerHotbarAbilities.Contains(SNOPower.Wizard_EnergyArmor) || hashPowerHotbarAbilities.Contains(SNOPower.Wizard_IceArmor) ||
                        hashPowerHotbarAbilities.Contains(SNOPower.Wizard_StormArmor));
                    int iExtraEnergyNeeded = 25;
                    if (hashPowerHotbarAbilities.Contains(SNOPower.Wizard_MagicWeapon)) iExtraEnergyNeeded += 25;
                    //if (hashPowerHotbarAbilities.Contains(SNOPower.Wizard_Familiar)) iExtraEnergyNeeded += 25;
                    if (hashPowerHotbarAbilities.Contains(SNOPower.Wizard_EnergyArmor) || hashPowerHotbarAbilities.Contains(SNOPower.Wizard_IceArmor) ||
                        hashPowerHotbarAbilities.Contains(SNOPower.Wizard_StormArmor)) iExtraEnergyNeeded += 25;
                    if (!bHasBuffAbilities || playerStatus.dCurrentEnergy <= iExtraEnergyNeeded)
                        bCanCastArchon = true;
                    if (!bCanCastArchon)
                    {
                        dictAbilityLastUse[SNOPower.Wizard_MagicWeapon] = DateTime.Today;
                        //dictAbilityLastUse[SNOPower.Wizard_Familiar] = DateTime.Today;
                        dictAbilityLastUse[SNOPower.Wizard_EnergyArmor] = DateTime.Today;
                        dictAbilityLastUse[SNOPower.Wizard_IceArmor] = DateTime.Today;
                        dictAbilityLastUse[SNOPower.Wizard_StormArmor] = DateTime.Today;
                        bCanCastArchon = true;
                    }
                    else
                    {
                        bCanCastArchon = false;
                        return new GilesPower(SNOPower.Wizard_Archon, 0f, vNullLocation, iCurrentWorldID, -1, 4, 5, USE_SLOWLY);
                    }
                }
                // Frost Nova SPAM
                if (!bOOCBuff && hashPowerHotbarAbilities.Contains(SNOPower.Wizard_FrostNova) && !playerStatus.bIsIncapacitated &&
                    ((iElitesWithinRange[RANGE_25] > 0 || iAnythingWithinRange[RANGE_25] > 0 || playerStatus.dCurrentHealthPct <= 0.7) && CurrentTarget.fRadiusDistance <= 12f) &&
                    PowerManager.CanCast(SNOPower.Wizard_FrostNova))
                {
                    float fThisRange = 14f;
                    if (settings.bEnableCriticalMass)
                        fThisRange = 9f;
                    return new GilesPower(SNOPower.Wizard_FrostNova, fThisRange, vNullLocation, iCurrentWorldID, -1, 0, 0, USE_SLOWLY);
                }
                // Explosive Blast SPAM when enough AP, blow erry thing up, nah mean
                if (!bOOCBuff && hashPowerHotbarAbilities.Contains(SNOPower.Wizard_ExplosiveBlast) && !playerStatus.bIsIncapacitated && playerStatus.dCurrentEnergy >= 20 &&
                    ((iElitesWithinRange[RANGE_25] >= 1 || iAnythingWithinRange[RANGE_25] >= 1 || playerStatus.dCurrentHealthPct <= 0.7) && CurrentTarget.fRadiusDistance <= 12f) &&
                    PowerManager.CanCast(SNOPower.Wizard_ExplosiveBlast))
                {
                    float fThisRange = 11f;
                    if (settings.bEnableCriticalMass)
                        fThisRange = 9f;
                    return new GilesPower(SNOPower.Wizard_ExplosiveBlast, fThisRange, vNullLocation, iCurrentWorldID, -1, 0, 0, USE_SLOWLY);
                }
                // Check to see if we have a signature spell on our hotbar, for energy twister check
                bool bHasSignatureSpell = (hashPowerHotbarAbilities.Contains(SNOPower.Wizard_MagicMissile) || hashPowerHotbarAbilities.Contains(SNOPower.Wizard_ShockPulse) ||
                    hashPowerHotbarAbilities.Contains(SNOPower.Wizard_SpectralBlade) || hashPowerHotbarAbilities.Contains(SNOPower.Wizard_Electrocute));
                // Energy Twister SPAMS whenever 35 or more ap to generate Arcane Power
                if (!bOOCBuff && !playerStatus.bIsIncapacitated && hashPowerHotbarAbilities.Contains(SNOPower.Wizard_EnergyTwister) &&
                    // If using storm chaser, then force a signature spell every 1 stack of the buff, if we have a signature spell
                    (!bHasSignatureSpell || GilesBuffStacks(SNOPower.Wizard_EnergyTwister) < 1) &&
                    (iElitesWithinRange[RANGE_30] >= 1 || iAnythingWithinRange[RANGE_25] >= 1 || CurrentTarget.fRadiusDistance <= 12f) &&
                    (!hashPowerHotbarAbilities.Contains(SNOPower.Wizard_Electrocute) || !hashActorSNOFastMobs.Contains(CurrentTarget.iActorSNO)) &&
                    ((settings.bEnableCriticalMass && (!bHasSignatureSpell || playerStatus.dCurrentEnergy >= 35)) || (!settings.bEnableCriticalMass && playerStatus.dCurrentEnergy >= 35)))
                {
                    float fThisRange = 28f;
                    if (settings.bEnableCriticalMass)
                        fThisRange = 9f;
                    return new GilesPower(SNOPower.Wizard_EnergyTwister, fThisRange, new Vector3(CurrentTarget.vPosition.X, CurrentTarget.vPosition.Y, CurrentTarget.vPosition.Z), iCurrentWorldID, -1, 0, 0, USE_SLOWLY);
                }
                // Disintegrate
                if (!bOOCBuff && !playerStatus.bIsIncapacitated && hashPowerHotbarAbilities.Contains(SNOPower.Wizard_Disintegrate) &&
                    ((playerStatus.dCurrentEnergy >= 20 && !playerStatus.bWaitingForReserveEnergy) || playerStatus.dCurrentEnergy >= iWaitingReservedAmount))
                {
                    float fThisRange = 35f;
                    if (settings.bEnableCriticalMass)
                        fThisRange = 20f;
                    return new GilesPower(SNOPower.Wizard_Disintegrate, fThisRange, vNullLocation, -1, CurrentTarget.iACDGuid, 0, 0, SIGNATURE_SPAM);
                }
                // Arcane Orb
                if (!bOOCBuff && !playerStatus.bIsIncapacitated && hashPowerHotbarAbilities.Contains(SNOPower.Wizard_ArcaneOrb) &&
                    ((playerStatus.dCurrentEnergy >= 35 && !playerStatus.bWaitingForReserveEnergy) || playerStatus.dCurrentEnergy >= iWaitingReservedAmount) &&
                    GilesUseTimer(SNOPower.Wizard_ArcaneOrb))
                {
                    float fThisRange = 40f;
                    if (settings.bEnableCriticalMass)
                        fThisRange = 20f;
                    return new GilesPower(SNOPower.Wizard_ArcaneOrb, fThisRange, vNullLocation, -1, CurrentTarget.iACDGuid, 1, 1, USE_SLOWLY);
                }
                // Arcane Torrent
                if (!bOOCBuff && !playerStatus.bIsIncapacitated && hashPowerHotbarAbilities.Contains(SNOPower.Wizard_ArcaneTorrent) &&
                    ((playerStatus.dCurrentEnergy >= 16 && !playerStatus.bWaitingForReserveEnergy) || playerStatus.dCurrentEnergy >= iWaitingReservedAmount) &&
                    GilesUseTimer(SNOPower.Wizard_ArcaneTorrent))
                {
                    float fThisRange = 40f;
                    /*if (settings.bEnableCriticalMass)
                        fThisRange = 20f;*/
                    return new GilesPower(SNOPower.Wizard_ArcaneTorrent, fThisRange, vNullLocation, -1, CurrentTarget.iACDGuid, 0, 0, USE_SLOWLY);
                }
                // Ray of Frost
                if (!bOOCBuff && !bCurrentlyAvoiding && !playerStatus.bIsIncapacitated && hashPowerHotbarAbilities.Contains(SNOPower.Wizard_RayOfFrost) &&
                    playerStatus.dCurrentEnergy >= 12)
                {
                    float fThisRange = 35f;
                    if (settings.bEnableCriticalMass)
                        fThisRange = 20f;
                    return new GilesPower(SNOPower.Wizard_RayOfFrost, fThisRange, vNullLocation, -1, CurrentTarget.iACDGuid, 0, 0, SIGNATURE_SPAM);
                }
                // Magic Missile
                if (!bOOCBuff && !bCurrentlyAvoiding && hashPowerHotbarAbilities.Contains(SNOPower.Wizard_MagicMissile))
                {
                    float fThisRange = 35f;
                    if (settings.bEnableCriticalMass)
                        fThisRange = 20f;
                    return new GilesPower(SNOPower.Wizard_MagicMissile, fThisRange, vNullLocation, -1, CurrentTarget.iACDGuid, 0, 0, USE_SLOWLY);
                }
                // Shock Pulse
                if (!bOOCBuff && !bCurrentlyAvoiding && hashPowerHotbarAbilities.Contains(SNOPower.Wizard_ShockPulse))
                {
                    return new GilesPower(SNOPower.Wizard_ShockPulse, 15f, vNullLocation, -1, CurrentTarget.iACDGuid, 0, 1, USE_SLOWLY);
                }
                // Spectral Blade
                if (!bOOCBuff && !bCurrentlyAvoiding && hashPowerHotbarAbilities.Contains(SNOPower.Wizard_SpectralBlade))
                {
                    return new GilesPower(SNOPower.Wizard_SpectralBlade, 14f, vNullLocation, -1, CurrentTarget.iACDGuid, 0, 1, USE_SLOWLY);
                }
                // Electrocute
                if (!bOOCBuff && !bCurrentlyAvoiding && hashPowerHotbarAbilities.Contains(SNOPower.Wizard_Electrocute))
                {
                    return new GilesPower(SNOPower.Wizard_Electrocute, 40f, vNullLocation, -1, CurrentTarget.iACDGuid, 0, 0, USE_SLOWLY);
                }
                // Default attacks
                if (!bOOCBuff && !bCurrentlyAvoiding && !playerStatus.bIsIncapacitated)
                {
                    return new GilesPower(SNOPower.Weapon_Melee_Instant, 10f, vNullLocation, -1, CurrentTarget.iACDGuid, 1, 1, USE_SLOWLY);
                }
            }
            else
            {
                // Archon form
                // Archon Slow Time for in combat
                if (!bOOCBuff && !playerStatus.bIsIncapacitated &&
                    (iElitesWithinRange[RANGE_25] > 0 || iAnythingWithinRange[RANGE_25] > 1 || playerStatus.dCurrentHealthPct <= 0.7 || ((CurrentTarget.IsEliteRareUnique || CurrentTarget.bIsTreasureGoblin || CurrentTarget.IsBoss) && CurrentTarget.fRadiusDistance <= 35f)) &&
                    hashPowerHotbarAbilities.Contains(SNOPower.Wizard_Archon_SlowTime) &&
                    GilesUseTimer(SNOPower.Wizard_Archon_SlowTime, true) && PowerManager.CanCast(SNOPower.Wizard_Archon_SlowTime))
                {
                    return new GilesPower(SNOPower.Wizard_Archon_SlowTime, 0f, vNullLocation, iCurrentWorldID, -1, 1, 1, USE_SLOWLY);
                }
                // Archon Teleport in combat
                if (!bOOCBuff && !bCurrentlyAvoiding && !playerStatus.bIsIncapacitated && hashPowerHotbarAbilities.Contains(SNOPower.Wizard_Archon_Teleport) &&
                    // Try and teleport-retreat from 1 elite or 3+ greys or a boss at 15 foot range
                    (iElitesWithinRange[RANGE_15] >= 1 || iAnythingWithinRange[RANGE_15] >= 3 || (CurrentTarget.IsBoss && CurrentTarget.fRadiusDistance <= 15f)) &&
                    GilesUseTimer(SNOPower.Wizard_Archon_Teleport) && PowerManager.CanCast(SNOPower.Wizard_Archon_Teleport))
                {
                    Vector3 vNewTarget = MathEx.CalculatePointFrom(CurrentTarget.vPosition, playerStatus.vCurrentPosition, -20f);
                    return new GilesPower(SNOPower.Wizard_Archon_Teleport, 35f, vNewTarget, iCurrentWorldID, -1, 1, 1, USE_SLOWLY);
                }
                // Arcane Blast
                if (!bOOCBuff && !playerStatus.bIsIncapacitated &&
                    (iElitesWithinRange[RANGE_15] >= 1 || iAnythingWithinRange[RANGE_15] >= 1 ||
                     ((CurrentTarget.IsEliteRareUnique || CurrentTarget.IsBoss) && CurrentTarget.fRadiusDistance <= 15f)) &&
                    GilesUseTimer(SNOPower.Wizard_Archon_ArcaneBlast) && PowerManager.CanCast(SNOPower.Wizard_Archon_ArcaneBlast))
                {
                    return new GilesPower(SNOPower.Wizard_Archon_ArcaneBlast, 0f, vNullLocation, iCurrentWorldID, -1, 1, 1, USE_SLOWLY);
                }
                // Arcane Strike (Arcane Strike) Rapid Spam at close-range only
                if (!bOOCBuff && !playerStatus.bIsIncapacitated && CurrentTarget.fRadiusDistance <= 13f &&
                    (CurrentTarget.IsEliteRareUnique || CurrentTarget.IsBoss))
                {
                    return new GilesPower(SNOPower.Wizard_Archon_ArcaneStrike, 11f, vNullLocation, -1, CurrentTarget.iACDGuid, 1, 1, USE_SLOWLY);
                }
                // Disintegrate
                if (!bOOCBuff && !bCurrentlyAvoiding && !playerStatus.bIsIncapacitated)
                {
                    return new GilesPower(SNOPower.Wizard_Archon_DisintegrationWave, 49f, vNullLocation, -1, CurrentTarget.iACDGuid, 0, 0, SIGNATURE_SPAM);
                }
            }
            return defaultPower;
        }

        private static GilesPower GetWizardDestructablePower()
        {
            if (hashPowerHotbarAbilities.Contains(SNOPower.Wizard_EnergyTwister) && playerStatus.dCurrentEnergy >= 35)
                return new GilesPower(SNOPower.Wizard_EnergyTwister, 9f, vNullLocation, -1, -1, 0, 0, USE_SLOWLY);
            if (hashPowerHotbarAbilities.Contains(SNOPower.Wizard_MagicMissile))
                return new GilesPower(SNOPower.Wizard_MagicMissile, 15f, vNullLocation, -1, -1, 0, 0, USE_SLOWLY);
            if (hashPowerHotbarAbilities.Contains(SNOPower.Wizard_ShockPulse))
                return new GilesPower(SNOPower.Wizard_ShockPulse, 10f, vNullLocation, -1, -1, 0, 0, USE_SLOWLY);
            if (hashPowerHotbarAbilities.Contains(SNOPower.Wizard_SpectralBlade))
                return new GilesPower(SNOPower.Wizard_SpectralBlade, 9f, vNullLocation, -1, -1, 0, 0, USE_SLOWLY);
            if (hashPowerHotbarAbilities.Contains(SNOPower.Wizard_Electrocute))
                return new GilesPower(SNOPower.Wizard_Electrocute, 9f, vNullLocation, -1, -1, 0, 0, USE_SLOWLY);
            return new GilesPower(SNOPower.Weapon_Melee_Instant, 10f, vNullLocation, -1, -1, 0, 0, USE_SLOWLY);
        }

    }
}
