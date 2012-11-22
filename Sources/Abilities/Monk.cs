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
        private static GilesPower GetMonkPower(bool bCurrentlyAvoiding, bool bOOCBuff, bool bDestructiblePower)
        {
            // Pick the best destructible power available
            if (bDestructiblePower)
            {
                return GetMonkDestroyPower();
            }
            // Monks need 80 for special spam like tempest rushing
            iWaitingReservedAmount = 80;
            // 4 Mantras for the initial buff (slow-use)
            if (hashPowerHotbarAbilities.Contains(SNOPower.Monk_MantraOfEvasion) && !GilesHasBuff(SNOPower.Monk_MantraOfEvasion) &&
                playerStatus.dCurrentEnergy >= 50 && GilesUseTimer(SNOPower.Monk_MantraOfEvasion, true))
            {
                return new GilesPower(SNOPower.Monk_MantraOfEvasion, 0f, vNullLocation, iCurrentWorldID, -1, 0, 1, USE_SLOWLY);
            }
            if (hashPowerHotbarAbilities.Contains(SNOPower.Monk_MantraOfConviction) && !GilesHasBuff(SNOPower.Monk_MantraOfConviction) &&
                playerStatus.dCurrentEnergy >= 50 && GilesUseTimer(SNOPower.Monk_MantraOfConviction, true))
            {
                return new GilesPower(SNOPower.Monk_MantraOfConviction, 0f, vNullLocation, iCurrentWorldID, -1, 0, 1, USE_SLOWLY);
            }
            if (hashPowerHotbarAbilities.Contains(SNOPower.Monk_MantraOfHealing) && !GilesHasBuff(SNOPower.Monk_MantraOfHealing) &&
                playerStatus.dCurrentEnergy >= 50 && GilesUseTimer(SNOPower.Monk_MantraOfHealing, true))
            {
                return new GilesPower(SNOPower.Monk_MantraOfHealing, 0f, vNullLocation, iCurrentWorldID, -1, 0, 1, USE_SLOWLY);
            }
            if (hashPowerHotbarAbilities.Contains(SNOPower.Monk_MantraOfRetribution) && !GilesHasBuff(SNOPower.Monk_MantraOfRetribution) &&
                playerStatus.dCurrentEnergy >= 50 && GilesUseTimer(SNOPower.Monk_MantraOfRetribution, true))
            {
                return new GilesPower(SNOPower.Monk_MantraOfRetribution, 0f, vNullLocation, iCurrentWorldID, -1, 0, 1, USE_SLOWLY);
            }
            // Mystic ally
            if (hashPowerHotbarAbilities.Contains(SNOPower.Monk_MysticAlly) && playerStatus.dCurrentEnergy >= 90 && iPlayerOwnedMysticAlly == 0 &&
                GilesUseTimer(SNOPower.Monk_MysticAlly) && PowerManager.CanCast(SNOPower.Monk_MysticAlly))
            {
                return new GilesPower(SNOPower.Monk_MysticAlly, 0f, vNullLocation, iCurrentWorldID, -1, 2, 2, USE_SLOWLY);
            }
            // InnerSanctuary
            if (!bOOCBuff && playerStatus.dCurrentHealthPct <= 0.45 && hashPowerHotbarAbilities.Contains(SNOPower.Monk_InnerSanctuary) &&
                GilesUseTimer(SNOPower.Monk_InnerSanctuary, true) &&
                playerStatus.dCurrentEnergy >= 30 && PowerManager.CanCast(SNOPower.Monk_InnerSanctuary))
            {
                return new GilesPower(SNOPower.Monk_InnerSanctuary, 0f, vNullLocation, iCurrentWorldID, -1, 1, 1, USE_SLOWLY);
            }
            // Serenity if health is low
            if ((playerStatus.dCurrentHealthPct <= 0.50 || (playerStatus.bIsIncapacitated && playerStatus.dCurrentHealthPct <= 0.90)) && hashPowerHotbarAbilities.Contains(SNOPower.Monk_Serenity) &&
                GilesUseTimer(SNOPower.Monk_Serenity, true) &&
                playerStatus.dCurrentEnergy >= 10 && PowerManager.CanCast(SNOPower.Monk_Serenity))
            {
                return new GilesPower(SNOPower.Monk_Serenity, 0f, vNullLocation, iCurrentWorldID, -1, 1, 1, USE_SLOWLY);
            }
            // Breath of heaven when needing healing or the buff
            if (!bOOCBuff && (playerStatus.dCurrentHealthPct <= 0.6 || !GilesHasBuff(SNOPower.Monk_BreathOfHeaven)) && hashPowerHotbarAbilities.Contains(SNOPower.Monk_BreathOfHeaven) &&
                (playerStatus.dCurrentEnergy >= 35 || (!hashPowerHotbarAbilities.Contains(SNOPower.Monk_Serenity) && playerStatus.dCurrentEnergy >= 25)) &&
                GilesUseTimer(SNOPower.Monk_BreathOfHeaven) && PowerManager.CanCast(SNOPower.Monk_BreathOfHeaven))
            {
                return new GilesPower(SNOPower.Monk_BreathOfHeaven, 0f, vNullLocation, iCurrentWorldID, -1, 1, 1, USE_SLOWLY);
            }
            // Blinding Flash
            if (!bOOCBuff && playerStatus.dCurrentEnergy >= 20 && hashPowerHotbarAbilities.Contains(SNOPower.Monk_BlindingFlash) &&
                (
                    iElitesWithinRange[RANGE_15] >= 1 || playerStatus.dCurrentHealthPct <= 0.4 || (iAnythingWithinRange[RANGE_20] >= 5 && iElitesWithinRange[RANGE_50] == 0) ||
                    (iAnythingWithinRange[RANGE_15] >= 3 && playerStatus.dCurrentEnergyPct <= 0.5) || (CurrentTarget.IsBoss && CurrentTarget.fRadiusDistance <= 15f) ||
                    (iAnythingWithinRange[RANGE_15] >= 1 && hashPowerHotbarAbilities.Contains(SNOPower.Monk_SweepingWind) && !GilesHasBuff(SNOPower.Monk_SweepingWind) && settings.bMonkInnaSet)
                ) &&
                // Check if we don't have breath of heaven
                (!hashPowerHotbarAbilities.Contains(SNOPower.Monk_BreathOfHeaven) ||
                (hashPowerHotbarAbilities.Contains(SNOPower.Monk_BreathOfHeaven) && (!settings.bMonkInnaSet || GilesHasBuff(SNOPower.Monk_BreathOfHeaven)))) &&
                // Check if either we don't have sweeping winds, or we do and it's ready to cast in a moment
                (!hashPowerHotbarAbilities.Contains(SNOPower.Monk_SweepingWind) ||
                (hashPowerHotbarAbilities.Contains(SNOPower.Monk_SweepingWind) && (playerStatus.dCurrentEnergy >= 95 ||
                (settings.bMonkInnaSet && playerStatus.dCurrentEnergy >= 25) || GilesHasBuff(SNOPower.Monk_SweepingWind))) ||
                playerStatus.dCurrentHealthPct <= 0.4) &&
                GilesUseTimer(SNOPower.Monk_BlindingFlash) && PowerManager.CanCast(SNOPower.Monk_BlindingFlash))
            {
                return new GilesPower(SNOPower.Monk_BlindingFlash, 0f, vNullLocation, iCurrentWorldID, -1, 0, 1, USE_SLOWLY); //intell -- 11f -- 1, 2
            }
            // Blinding Flash as a DEFENSE
            if (!bOOCBuff && playerStatus.dCurrentEnergy >= 10 && hashPowerHotbarAbilities.Contains(SNOPower.Monk_BlindingFlash) &&
                playerStatus.dCurrentHealthPct <= 0.25 && iAnythingWithinRange[RANGE_15] >= 1 &&
                GilesUseTimer(SNOPower.Monk_BlindingFlash) && PowerManager.CanCast(SNOPower.Monk_BlindingFlash))
            {
                return new GilesPower(SNOPower.Monk_BlindingFlash, 0f, vNullLocation, iCurrentWorldID, -1, 0, 1, USE_SLOWLY); //intell -- 11f -- 1, 2
            }
            // Sweeping wind
            //intell -- inna
            if (!bOOCBuff && hashPowerHotbarAbilities.Contains(SNOPower.Monk_SweepingWind) && !GilesHasBuff(SNOPower.Monk_SweepingWind) &&
                (iElitesWithinRange[RANGE_25] > 0 || iAnythingWithinRange[RANGE_20] >= 3 || (iAnythingWithinRange[RANGE_20] >= 1 && settings.bMonkInnaSet) || ((CurrentTarget.IsEliteRareUnique || CurrentTarget.IsBoss) && CurrentTarget.fRadiusDistance <= 25f)) &&
                // Check if either we don't have blinding flash, or we do and it's been cast in the last 6000ms
                //DateTime.Now.Subtract(dictAbilityLastUse[SNOPower.Monk_BlindingFlash]).TotalMilliseconds <= 6000)) &&
                (!hashPowerHotbarAbilities.Contains(SNOPower.Monk_BlindingFlash) ||
                (hashPowerHotbarAbilities.Contains(SNOPower.Monk_BlindingFlash) &&
                ((!settings.bMonkInnaSet && iElitesWithinRange[RANGE_50] == 0 && !CurrentTarget.IsEliteRareUnique && !CurrentTarget.IsBoss) || GilesHasBuff(SNOPower.Monk_BlindingFlash)))) &&
                // Check our mantras, if we have them, are up first
                (!hashPowerHotbarAbilities.Contains(SNOPower.Monk_MantraOfEvasion) || (hashPowerHotbarAbilities.Contains(SNOPower.Monk_MantraOfEvasion) && GilesHasBuff(SNOPower.Monk_MantraOfEvasion))) &&
                (!hashPowerHotbarAbilities.Contains(SNOPower.Monk_MantraOfConviction) || (hashPowerHotbarAbilities.Contains(SNOPower.Monk_MantraOfConviction) && GilesHasBuff(SNOPower.Monk_MantraOfConviction))) &&
                (!hashPowerHotbarAbilities.Contains(SNOPower.Monk_MantraOfRetribution) || (hashPowerHotbarAbilities.Contains(SNOPower.Monk_MantraOfRetribution) && GilesHasBuff(SNOPower.Monk_MantraOfRetribution))) &&
                // Check the re-use timer and energy costs
                (playerStatus.dCurrentEnergy >= 75 || (settings.bMonkInnaSet && playerStatus.dCurrentEnergy >= 5)))
            {
                SweepWindSpam = DateTime.Now;
                return new GilesPower(SNOPower.Monk_SweepingWind, 0f, vNullLocation, iCurrentWorldID, -1, 0, 1, USE_SLOWLY); //intell -- 2,2
            }
            // Sweeping wind: spam it if inna set
            //intell -- inna
            if (hashPowerHotbarAbilities.Contains(SNOPower.Monk_SweepingWind) && settings.bMonkInnaSet && GilesHasBuff(SNOPower.Monk_SweepingWind) &&
                playerStatus.dCurrentEnergy >= 5 && DateTime.Now.Subtract(SweepWindSpam).TotalMilliseconds >= 5000)
            {
                SweepWindSpam = DateTime.Now;
                return new GilesPower(SNOPower.Monk_SweepingWind, 0f, vNullLocation, iCurrentWorldID, -1, 0, 0, USE_SLOWLY);
            }
            // Seven Sided Strike
            if (!bOOCBuff && !bCurrentlyAvoiding && !playerStatus.bIsIncapacitated &&
                (iElitesWithinRange[RANGE_15] >= 1 || ((CurrentTarget.IsEliteRareUnique || CurrentTarget.IsBoss) && CurrentTarget.fRadiusDistance <= 15f) || playerStatus.dCurrentHealthPct <= 0.55) &&
                hashPowerHotbarAbilities.Contains(SNOPower.Monk_SevenSidedStrike) && ((playerStatus.dCurrentEnergy >= 50 && !playerStatus.bWaitingForReserveEnergy) || playerStatus.dCurrentEnergy >= iWaitingReservedAmount) &&
                GilesUseTimer(SNOPower.Monk_SevenSidedStrike, true) && PowerManager.CanCast(SNOPower.Monk_SevenSidedStrike))
            {
                return new GilesPower(SNOPower.Monk_SevenSidedStrike, 16f, CurrentTarget.vPosition, iCurrentWorldID, -1, 2, 3, USE_SLOWLY);
            }
            // Exploding Palm
            if (!bOOCBuff && !bCurrentlyAvoiding && !playerStatus.bIsIncapacitated &&
                (iElitesWithinRange[RANGE_25] > 0 || iAnythingWithinRange[RANGE_15] >= 3 || ((CurrentTarget.IsEliteRareUnique || CurrentTarget.IsBoss) && CurrentTarget.fRadiusDistance <= 14f)) &&
                hashPowerHotbarAbilities.Contains(SNOPower.Monk_ExplodingPalm) &&
                ((playerStatus.dCurrentEnergy >= 40 && !playerStatus.bWaitingForReserveEnergy) || playerStatus.dCurrentEnergy >= iWaitingReservedAmount) &&
                GilesUseTimer(SNOPower.Monk_ExplodingPalm) && PowerManager.CanCast(SNOPower.Monk_ExplodingPalm))
            {
                return new GilesPower(SNOPower.Monk_ExplodingPalm, 14f, vNullLocation, -1, CurrentTarget.iACDGuid, 1, 1, USE_SLOWLY);
            }
            // Cyclone Strike
            if (!bOOCBuff && !bCurrentlyAvoiding && !playerStatus.bIsIncapacitated &&
                (iElitesWithinRange[RANGE_20] >= 1 || iAnythingWithinRange[RANGE_20] >= 2 || playerStatus.dCurrentEnergyPct >= 0.5 || ((CurrentTarget.IsEliteRareUnique || CurrentTarget.IsBoss) && CurrentTarget.fRadiusDistance <= 18f)) &&
                hashPowerHotbarAbilities.Contains(SNOPower.Monk_CycloneStrike) &&
                ((playerStatus.dCurrentEnergy >= 70 && !playerStatus.bWaitingForReserveEnergy) || playerStatus.dCurrentEnergy >= iWaitingReservedAmount) &&
                GilesUseTimer(SNOPower.Monk_CycloneStrike) && PowerManager.CanCast(SNOPower.Monk_CycloneStrike))
            {
                return new GilesPower(SNOPower.Monk_CycloneStrike, 0f, vNullLocation, iCurrentWorldID, -1, 2, 2, USE_SLOWLY);
            }
            // 4 Mantra spam for the 4 second buff
            if (!bOOCBuff && (!hashPowerHotbarAbilities.Contains(SNOPower.Monk_TempestRush) || playerStatus.dCurrentEnergy >= 98 ||
                (playerStatus.dCurrentHealthPct <= 0.55 && playerStatus.dCurrentEnergy >= 75) || CurrentTarget.IsBoss) &&
                    (playerStatus.dCurrentEnergy >= 135 ||
                    (GilesHasBuff(SNOPower.Monk_SweepingWind) && (playerStatus.dCurrentEnergy >= 60 ||
                    (playerStatus.dCurrentEnergy >= 50 && playerStatus.dCurrentHealthPct >= 0.6)) &&
                // Checking we have no expensive finishers
                    !hashPowerHotbarAbilities.Contains(SNOPower.Monk_SevenSidedStrike) && !hashPowerHotbarAbilities.Contains(SNOPower.Monk_LashingTailKick) &&
                    !hashPowerHotbarAbilities.Contains(SNOPower.Monk_WaveOfLight) && !hashPowerHotbarAbilities.Contains(SNOPower.Monk_CycloneStrike) &&
                    !hashPowerHotbarAbilities.Contains(SNOPower.Monk_ExplodingPalm))) &&
                (iElitesWithinRange[RANGE_15] >= 1 || iAnythingWithinRange[RANGE_15] >= 3 ||
                (iAnythingWithinRange[RANGE_15] >= 1 && (settings.bMonkInnaSet && playerStatus.dCurrentEnergy >= 70)))) //intell -- inna
            {
                if (hashPowerHotbarAbilities.Contains(SNOPower.Monk_MantraOfEvasion) && GilesUseTimer(SNOPower.Monk_MantraOfEvasion))
                {
                    return new GilesPower(SNOPower.Monk_MantraOfEvasion, 0f, vNullLocation, iCurrentWorldID, -1, 1, 1, USE_SLOWLY);
                }
                if (hashPowerHotbarAbilities.Contains(SNOPower.Monk_MantraOfConviction) && GilesUseTimer(SNOPower.Monk_MantraOfConviction))
                {
                    return new GilesPower(SNOPower.Monk_MantraOfConviction, 0f, vNullLocation, iCurrentWorldID, -1, 1, 1, USE_SLOWLY);
                }
                if (hashPowerHotbarAbilities.Contains(SNOPower.Monk_MantraOfRetribution) && GilesUseTimer(SNOPower.Monk_MantraOfRetribution))
                {
                    return new GilesPower(SNOPower.Monk_MantraOfRetribution, 0f, vNullLocation, iCurrentWorldID, -1, 1, 1, USE_SLOWLY);
                }
                if (hashPowerHotbarAbilities.Contains(SNOPower.Monk_MantraOfHealing) && GilesUseTimer(SNOPower.Monk_MantraOfHealing))
                {
                    return new GilesPower(SNOPower.Monk_MantraOfHealing, 0f, vNullLocation, iCurrentWorldID, -1, 1, 1, USE_SLOWLY);
                }
            }
            // Lashing Tail Kick
            if (!bOOCBuff && !bCurrentlyAvoiding && hashPowerHotbarAbilities.Contains(SNOPower.Monk_LashingTailKick) && !playerStatus.bIsIncapacitated &&
                (iElitesWithinRange[RANGE_15] > 0 || iAnythingWithinRange[RANGE_15] > 4 || ((CurrentTarget.IsEliteRareUnique || CurrentTarget.IsBoss) && CurrentTarget.fRadiusDistance <= 10f)) &&
                // Either doesn't have sweeping wind, or does but the buff is already up
                (!hashPowerHotbarAbilities.Contains(SNOPower.Monk_SweepingWind) || (hashPowerHotbarAbilities.Contains(SNOPower.Monk_SweepingWind) && GilesHasBuff(SNOPower.Monk_SweepingWind))) &&
                GilesUseTimer(SNOPower.Monk_LashingTailKick) &&
                ((playerStatus.dCurrentEnergy >= 65 && !playerStatus.bWaitingForReserveEnergy) || playerStatus.dCurrentEnergy >= iWaitingReservedAmount))
            {
                return new GilesPower(SNOPower.Monk_LashingTailKick, 10f, vNullLocation, -1, CurrentTarget.iACDGuid, 1, 1, USE_SLOWLY);
            }
            // Wave of light
            if (!bOOCBuff && !bCurrentlyAvoiding && !playerStatus.bIsIncapacitated &&
                (iElitesWithinRange[RANGE_25] > 0 ||
                ((CurrentTarget.IsEliteRareUnique || CurrentTarget.IsBoss) && CurrentTarget.fRadiusDistance <= 14f) ||
                iAnythingWithinRange[RANGE_15] > 2) &&
                hashPowerHotbarAbilities.Contains(SNOPower.Monk_WaveOfLight) &&
                GilesUseTimer(SNOPower.Monk_WaveOfLight) &&
                (playerStatus.dCurrentEnergy >= 90 || playerStatus.dCurrentEnergyPct >= 0.85) && HasMonkMantraAbilityAndBuff())
            {
                return new GilesPower(SNOPower.Monk_WaveOfLight, 16f, vNullLocation, -1, CurrentTarget.iACDGuid, 1, 1, USE_SLOWLY);
            }
            // Tempest rush at elites or groups of mobs
            if (!bOOCBuff && !bCurrentlyAvoiding && !playerStatus.bIsIncapacitated && !playerStatus.bIsRooted &&
                (iElitesWithinRange[RANGE_25] > 0 || ((CurrentTarget.IsEliteRareUnique || CurrentTarget.IsBoss) && CurrentTarget.fRadiusDistance <= 14f) || iAnythingWithinRange[RANGE_15] > 2) &&
                hashPowerHotbarAbilities.Contains(SNOPower.Monk_TempestRush) && ((playerStatus.dCurrentEnergy >= 20 && !playerStatus.bWaitingForReserveEnergy) || playerStatus.dCurrentEnergy >= iWaitingReservedAmount) &&
                PowerManager.CanCast(SNOPower.Monk_TempestRush))
            {
                bool bGenerateNewZigZag = (DateTime.Now.Subtract(lastChangedZigZag).TotalMilliseconds >= 1500 ||
                    (vPositionLastZigZagCheck != vNullLocation && playerStatus.vCurrentPosition == vPositionLastZigZagCheck && DateTime.Now.Subtract(lastChangedZigZag).TotalMilliseconds >= 200) ||
                    Vector3.Distance(playerStatus.vCurrentPosition, vSideToSideTarget) <= 4f ||
                    CurrentTarget.iACDGuid != iACDGUIDLastWhirlwind);
                vPositionLastZigZagCheck = playerStatus.vCurrentPosition;
                if (bGenerateNewZigZag)
                {
                    float fExtraDistance = CurrentTarget.fCentreDist <= 20f ? 15f : 5f;
                    vSideToSideTarget = FindZigZagTargetLocation(CurrentTarget.vPosition, CurrentTarget.fCentreDist + fExtraDistance);
                    // Resetting this to ensure the "no-spam" is reset since we changed our target location
                    powerLastSnoPowerUsed = SNOPower.None;
                    iACDGUIDLastWhirlwind = CurrentTarget.iACDGuid;
                    lastChangedZigZag = DateTime.Now;
                }
                return new GilesPower(SNOPower.Monk_TempestRush, 23f, vSideToSideTarget, iCurrentWorldID, -1, 0, 0, USE_SLOWLY);
            }
            // Dashing Strike
            if (!bOOCBuff && !bCurrentlyAvoiding && !playerStatus.bIsIncapacitated &&
                (iElitesWithinRange[RANGE_25] > 0 || ((CurrentTarget.IsEliteRareUnique || CurrentTarget.IsBoss) && CurrentTarget.fRadiusDistance <= 14f) || iAnythingWithinRange[RANGE_15] > 2) &&
                hashPowerHotbarAbilities.Contains(SNOPower.Monk_DashingStrike) && ((playerStatus.dCurrentEnergy >= 30 && !playerStatus.bWaitingForReserveEnergy) || playerStatus.dCurrentEnergy >= iWaitingReservedAmount))
            {
                return new GilesPower(SNOPower.Monk_DashingStrike, 14f, vNullLocation, -1, CurrentTarget.iACDGuid, 0, 1, USE_SLOWLY);
            }
            // Fists of thunder as the primary, repeatable attack
            if (!bOOCBuff && !bCurrentlyAvoiding && hashPowerHotbarAbilities.Contains(SNOPower.Monk_FistsofThunder))
            {
                if (GilesHasBuff(SNOPower.Monk_SweepingWind))
                    SweepWindSpam = DateTime.Now; //intell -- inna
                return new GilesPower(SNOPower.Monk_FistsofThunder, 30f, vNullLocation, -1, CurrentTarget.iACDGuid, 0, 0, SIGNATURE_SPAM);
            }
            // Deadly reach
            if (!bOOCBuff && !bCurrentlyAvoiding && hashPowerHotbarAbilities.Contains(SNOPower.Monk_DeadlyReach))
            {
                if (GilesHasBuff(SNOPower.Monk_SweepingWind))
                    SweepWindSpam = DateTime.Now; //intell -- inna
                return new GilesPower(SNOPower.Monk_DeadlyReach, 16f, vNullLocation, -1, CurrentTarget.iACDGuid, 0, 1, USE_SLOWLY);
            }
            // Crippling wave
            if (!bOOCBuff && !bCurrentlyAvoiding && hashPowerHotbarAbilities.Contains(SNOPower.Monk_CripplingWave))
            {
                if (GilesHasBuff(SNOPower.Monk_SweepingWind))
                    SweepWindSpam = DateTime.Now; //intell -- inna
                return new GilesPower(SNOPower.Monk_CripplingWave, 14f, vNullLocation, -1, CurrentTarget.iACDGuid, 0, 1, USE_SLOWLY);
            }
            // Way of hundred fists
            if (!bOOCBuff && !bCurrentlyAvoiding && hashPowerHotbarAbilities.Contains(SNOPower.Monk_WayOfTheHundredFists))
            {
                if (GilesHasBuff(SNOPower.Monk_SweepingWind))
                    SweepWindSpam = DateTime.Now; //intell -- inna
                return new GilesPower(SNOPower.Monk_WayOfTheHundredFists, 14f, vNullLocation, -1, CurrentTarget.iACDGuid, 0, 1, SIGNATURE_SPAM);
            }
            // Default attacks
            if (!bOOCBuff && !bCurrentlyAvoiding && !playerStatus.bIsIncapacitated)
            {
                if (GilesHasBuff(SNOPower.Monk_SweepingWind))
                    SweepWindSpam = DateTime.Now; //intell -- inna
                return new GilesPower(SNOPower.Weapon_Melee_Instant, 10f, vNullLocation, -1, CurrentTarget.iACDGuid, 1, 1, USE_SLOWLY);
            }
            return defaultPower;
        }

        private static GilesPower GetMonkDestroyPower()
        {
            if (hashPowerHotbarAbilities.Contains(SNOPower.Monk_FistsofThunder))
                return new GilesPower(SNOPower.Monk_FistsofThunder, 15f, vNullLocation, -1, -1, 0, 0, USE_SLOWLY);
            if (hashPowerHotbarAbilities.Contains(SNOPower.Monk_DeadlyReach))
                return new GilesPower(SNOPower.Monk_DeadlyReach, 10f, vNullLocation, -1, -1, 0, 0, USE_SLOWLY);
            if (hashPowerHotbarAbilities.Contains(SNOPower.Monk_CripplingWave))
                return new GilesPower(SNOPower.Monk_CripplingWave, 10f, vNullLocation, -1, -1, 0, 0, USE_SLOWLY);
            if (hashPowerHotbarAbilities.Contains(SNOPower.Monk_WayOfTheHundredFists))
                return new GilesPower(SNOPower.Monk_WayOfTheHundredFists, 10f, vNullLocation, -1, -1, 0, 0, USE_SLOWLY);
            return new GilesPower(SNOPower.Weapon_Melee_Instant, 10f, vNullLocation, -1, -1, 0, 0, USE_SLOWLY);
        }
        private static bool HasMonkMantraAbilityAndBuff()
        {
            return
                (hashPowerHotbarAbilities.Contains(SNOPower.Monk_MantraOfConviction) ||
                hashPowerHotbarAbilities.Contains(SNOPower.Monk_MantraOfEvasion) ||
                hashPowerHotbarAbilities.Contains(SNOPower.Monk_MantraOfHealing) ||
                hashPowerHotbarAbilities.Contains(SNOPower.Monk_MantraOfRetribution))
                &&
                (GilesHasBuff(SNOPower.Monk_MantraOfConviction) ||
                GilesHasBuff(SNOPower.Monk_MantraOfEvasion) ||
                GilesHasBuff(SNOPower.Monk_MantraOfHealing) ||
                GilesHasBuff(SNOPower.Monk_MantraOfRetribution));
        }

    }
}
