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
        private static GilesPower GetDemonHunterPower(bool bCurrentlyAvoiding, bool bOOCBuff, bool bDestructiblePower)
        {
            // Pick the best destructible power available
            if (bDestructiblePower)
            {
                return GetDemonHunterDestroyPower();
            }
            iWaitingReservedAmount = 25;
            // Shadow Power
            if (!bOOCBuff && hashPowerHotbarAbilities.Contains(SNOPower.DemonHunter_ShadowPower) && !playerStatus.bIsIncapacitated &&
                playerStatus.dDiscipline >= 14 &&
                (playerStatus.dCurrentHealthPct <= 0.99 || playerStatus.bIsRooted || iElitesWithinRange[RANGE_25] >= 1 || iAnythingWithinRange[RANGE_15] >= 3) &&
                GilesUseTimer(SNOPower.DemonHunter_ShadowPower))
            {
                return new GilesPower(SNOPower.DemonHunter_ShadowPower, 0f, vNullLocation, iCurrentWorldID, -1, 1, 1, USE_SLOWLY);
            }
            // Smoke Screen
            if (!bOOCBuff && hashPowerHotbarAbilities.Contains(SNOPower.DemonHunter_SmokeScreen) &&
                !GilesHasBuff(SNOPower.DemonHunter_ShadowPower) && playerStatus.dDiscipline >= 14 &&
                (playerStatus.dCurrentHealthPct <= 0.90 || playerStatus.bIsRooted || iElitesWithinRange[RANGE_20] >= 1 || iAnythingWithinRange[RANGE_15] >= 3 || playerStatus.bIsIncapacitated) &&
                GilesUseTimer(SNOPower.DemonHunter_SmokeScreen))
            {
                return new GilesPower(SNOPower.DemonHunter_SmokeScreen, 0f, vNullLocation, iCurrentWorldID, -1, 1, 1, USE_SLOWLY);
            }
            // Preparation
            if (!bOOCBuff && hashPowerHotbarAbilities.Contains(SNOPower.DemonHunter_Preparation) && !playerStatus.bIsIncapacitated &&
                playerStatus.dDiscipline <= 9 && iAnythingWithinRange[RANGE_40] >= 1 &&
                GilesUseTimer(SNOPower.DemonHunter_Preparation) && PowerManager.CanCast(SNOPower.DemonHunter_Preparation))
            {
                return new GilesPower(SNOPower.DemonHunter_Preparation, 0f, vNullLocation, iCurrentWorldID, -1, 1, 1, USE_SLOWLY);
            }
            // Evasive Fire
            if (!bOOCBuff && hashPowerHotbarAbilities.Contains(SNOPower.DemonHunter_EvasiveFire) && !playerStatus.bIsIncapacitated &&
                (iAnythingWithinRange[RANGE_20] >= 1 || CurrentTarget.fRadiusDistance <= 20f) &&
                GilesUseTimer(SNOPower.DemonHunter_EvasiveFire))
            {
                return new GilesPower(SNOPower.DemonHunter_EvasiveFire, 0f, vNullLocation, -1, CurrentTarget.iACDGuid, 1, 1, USE_SLOWLY);
            }
            // Companion
            if (!playerStatus.bIsIncapacitated && hashPowerHotbarAbilities.Contains(SNOPower.DemonHunter_Companion) && iPlayerOwnedDHPets == 0 &&
                playerStatus.dDiscipline >= 10 && GilesUseTimer(SNOPower.DemonHunter_Companion))
            {
                return new GilesPower(SNOPower.DemonHunter_Companion, 0f, vNullLocation, iCurrentWorldID, -1, 2, 1, USE_SLOWLY);
            }
            // Sentry Turret
            if (!bOOCBuff && !playerStatus.bIsIncapacitated && hashPowerHotbarAbilities.Contains(SNOPower.DemonHunter_Sentry) &&
                powerLastSnoPowerUsed != SNOPower.DemonHunter_Sentry &&
                (iElitesWithinRange[RANGE_50] >= 1 || iAnythingWithinRange[RANGE_50] >= 2 ||
                (CurrentTarget.IsEliteRareUnique || CurrentTarget.bIsTreasureGoblin || CurrentTarget.IsBoss)) && CurrentTarget.fRadiusDistance <= 50f &&
                playerStatus.dCurrentEnergy >= 30 && GilesUseTimer(SNOPower.DemonHunter_Sentry))
            {
                return new GilesPower(SNOPower.DemonHunter_Sentry, 0f, vNullLocation, iCurrentWorldID, -1, 0, 0, SIGNATURE_SPAM);
            }
            // Marked for Death
            if (!bOOCBuff && !bCurrentlyAvoiding && hashPowerHotbarAbilities.Contains(SNOPower.DemonHunter_MarkedForDeath) &&
                playerStatus.dDiscipline >= 3 &&
                (iElitesWithinRange[RANGE_40] >= 1 || iAnythingWithinRange[RANGE_40] >= 3 ||
                ((CurrentTarget.IsEliteRareUnique || CurrentTarget.bIsTreasureGoblin || CurrentTarget.IsBoss) &&
                CurrentTarget.fRadiusDistance <= 40f)) &&
                GilesUseTimer(SNOPower.DemonHunter_MarkedForDeath))
            {
                return new GilesPower(SNOPower.DemonHunter_MarkedForDeath, 40f, vNullLocation, iCurrentWorldID, CurrentTarget.iACDGuid, 1, 1, USE_SLOWLY);
            }
            // Vault
            if (!bOOCBuff && !bCurrentlyAvoiding && hashPowerHotbarAbilities.Contains(SNOPower.DemonHunter_Vault) && !playerStatus.bIsRooted && !playerStatus.bIsIncapacitated &&
                // Only use vault to retreat if < level 60, or if in inferno difficulty for level 60's
                (playerStatus.iMyLevel < 60 || iCurrentGameDifficulty == GameDifficulty.Inferno) &&
                (CurrentTarget.fRadiusDistance <= 10f || iAnythingWithinRange[RANGE_6] >= 1) &&
                playerStatus.dDiscipline >= 8 && GilesUseTimer(SNOPower.DemonHunter_Vault) && PowerManager.CanCast(SNOPower.DemonHunter_Vault))
            {
                Vector3 vNewTarget = MathEx.CalculatePointFrom(CurrentTarget.vPosition, playerStatus.vCurrentPosition, -15f);
                return new GilesPower(SNOPower.DemonHunter_Vault, 20f, vNewTarget, iCurrentWorldID, -1, 1, 2, USE_SLOWLY);
            }
            // Rain of Vengeance
            if (!bOOCBuff && hashPowerHotbarAbilities.Contains(SNOPower.DemonHunter_RainOfVengeance) && !playerStatus.bIsIncapacitated &&
                (iAnythingWithinRange[RANGE_25] >= 3 || iElitesWithinRange[RANGE_25] >= 1) &&
                GilesUseTimer(SNOPower.DemonHunter_RainOfVengeance) && PowerManager.CanCast(SNOPower.DemonHunter_RainOfVengeance))
            {
                return new GilesPower(SNOPower.DemonHunter_RainOfVengeance, 0f, vNullLocation, iCurrentWorldID, -1, 1, 1, USE_SLOWLY);
            }
            // Cluster Arrow
            if (!bOOCBuff && !bCurrentlyAvoiding && hashPowerHotbarAbilities.Contains(SNOPower.DemonHunter_ClusterArrow) && !playerStatus.bIsIncapacitated &&
                playerStatus.dCurrentEnergy >= 50 &&
               (iElitesWithinRange[RANGE_50] >= 1 || iAnythingWithinRange[RANGE_50] >= 5 || ((CurrentTarget.IsEliteRareUnique || CurrentTarget.bIsTreasureGoblin || CurrentTarget.IsBoss) && CurrentTarget.fRadiusDistance <= 69f)) &&
                GilesUseTimer(SNOPower.DemonHunter_ClusterArrow))
            {
                return new GilesPower(SNOPower.DemonHunter_ClusterArrow, 69f, vNullLocation, iCurrentWorldID, CurrentTarget.iACDGuid, 1, 1, USE_SLOWLY);
            }
            // Multi Shot
            if (!bOOCBuff && !bCurrentlyAvoiding && hashPowerHotbarAbilities.Contains(SNOPower.DemonHunter_Multishot) && !playerStatus.bIsIncapacitated &&
                playerStatus.dCurrentEnergy >= 30 &&
                (iElitesWithinRange[RANGE_40] >= 1 || iAnythingWithinRange[RANGE_40] >= 2 || ((CurrentTarget.IsEliteRareUnique || CurrentTarget.bIsTreasureGoblin || CurrentTarget.IsBoss) && CurrentTarget.fRadiusDistance <= 40f)))
            {
                return new GilesPower(SNOPower.DemonHunter_Multishot, 40f, CurrentTarget.vPosition, iCurrentWorldID, -1, 1, 1, USE_SLOWLY);
            }
            // Fan of Knives
            if (!bOOCBuff && hashPowerHotbarAbilities.Contains(SNOPower.DemonHunter_FanOfKnives) && !playerStatus.bIsIncapacitated &&
                playerStatus.dCurrentEnergy >= 20 &&
                (iAnythingWithinRange[RANGE_15] >= 4 || iElitesWithinRange[RANGE_15] >= 1) &&
                GilesUseTimer(SNOPower.DemonHunter_FanOfKnives))
            {
                return new GilesPower(SNOPower.DemonHunter_FanOfKnives, 0f, vNullLocation, iCurrentWorldID, -1, 1, 1, USE_SLOWLY);
            }
            // Strafe spam - similar to barbarian whirlwind routine
            if (!bOOCBuff && !bCurrentlyAvoiding && hashPowerHotbarAbilities.Contains(SNOPower.DemonHunter_Strafe) && !playerStatus.bIsIncapacitated && !playerStatus.bIsRooted &&
                // Only if there's 3 guys in 25 yds
                iAnythingWithinRange[RANGE_25] >= 3 &&
                // Check for energy reservation amounts
                ((playerStatus.dCurrentEnergy >= 15 && !playerStatus.bWaitingForReserveEnergy) || playerStatus.dCurrentEnergy >= iWaitingReservedAmount))
            {
                bool bGenerateNewZigZag = (DateTime.Now.Subtract(lastChangedZigZag).TotalMilliseconds >= 1500 ||
                    (vPositionLastZigZagCheck != vNullLocation && playerStatus.vCurrentPosition == vPositionLastZigZagCheck && DateTime.Now.Subtract(lastChangedZigZag).TotalMilliseconds >= 200) ||
                    Vector3.Distance(playerStatus.vCurrentPosition, vSideToSideTarget) <= 4f ||
                    CurrentTarget.iACDGuid != iACDGUIDLastWhirlwind);
                vPositionLastZigZagCheck = playerStatus.vCurrentPosition;
                if (bGenerateNewZigZag)
                {
                    float fExtraDistance = CurrentTarget.fCentreDist <= 10f ? 10f : 5f;
                    //vSideToSideTarget = FindZigZagTargetLocation(CurrentTarget.vPosition, CurrentTarget.fCentreDist + fExtraDistance);
                    vSideToSideTarget = FindSafeZone(false, 1, CurrentTarget.vPosition, false);
                    // Resetting this to ensure the "no-spam" is reset since we changed our target location
                    powerLastSnoPowerUsed = SNOPower.None;
                    iACDGUIDLastWhirlwind = CurrentTarget.iACDGuid;
                    lastChangedZigZag = DateTime.Now;
                }
                return new GilesPower(SNOPower.DemonHunter_Strafe, 25f, vSideToSideTarget, iCurrentWorldID, -1, 0, 0, USE_SLOWLY);
            }
            // Spike Trap
            if (!bOOCBuff && !playerStatus.bIsIncapacitated && hashPowerHotbarAbilities.Contains(SNOPower.DemonHunter_SpikeTrap) &&
                powerLastSnoPowerUsed != SNOPower.DemonHunter_SpikeTrap &&
                (iElitesWithinRange[RANGE_30] >= 1 || iAnythingWithinRange[RANGE_25] > 4 || ((CurrentTarget.IsEliteRareUnique || CurrentTarget.bIsTreasureGoblin || CurrentTarget.IsBoss) && CurrentTarget.fRadiusDistance <= 35f)) &&
                playerStatus.dCurrentEnergy >= 30 && GilesUseTimer(SNOPower.DemonHunter_SpikeTrap))
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
                return new GilesPower(SNOPower.DemonHunter_SpikeTrap, 40f, vNewTarget, iCurrentWorldID, -1, 1, 1, USE_SLOWLY);
            }
            // Caltrops
            if (!bOOCBuff && hashPowerHotbarAbilities.Contains(SNOPower.DemonHunter_Caltrops) && !playerStatus.bIsIncapacitated &&
                playerStatus.dDiscipline >= 6 && (iAnythingWithinRange[RANGE_30] >= 2 || iElitesWithinRange[RANGE_40] >= 1) &&
                GilesUseTimer(SNOPower.DemonHunter_Caltrops))
            {
                return new GilesPower(SNOPower.DemonHunter_Caltrops, 0f, vNullLocation, iCurrentWorldID, -1, 1, 1, USE_SLOWLY);
            }
            // Elemental Arrow
            if (!bOOCBuff && !bCurrentlyAvoiding && hashPowerHotbarAbilities.Contains(SNOPower.DemonHunter_ElementalArrow) && !playerStatus.bIsIncapacitated &&
                ((playerStatus.dCurrentEnergy >= 10 && !playerStatus.bWaitingForReserveEnergy) || playerStatus.dCurrentEnergy >= iWaitingReservedAmount))
            {
                // Players with grenades *AND* elemental arrow should spam grenades at close-range instead
                if (hashPowerHotbarAbilities.Contains(SNOPower.DemonHunter_Grenades) && CurrentTarget.fRadiusDistance <= 18f)
                    return new GilesPower(SNOPower.DemonHunter_Grenades, 18f, vNullLocation, -1, CurrentTarget.iACDGuid, 0, 1, USE_SLOWLY);
                // Now return elemental arrow, if not sending grenades instead
                return new GilesPower(SNOPower.DemonHunter_ElementalArrow, 50f, vNullLocation, -1, CurrentTarget.iACDGuid, 0, 1, USE_SLOWLY);
            }
            // Chakram
            if (!bOOCBuff && !bCurrentlyAvoiding && hashPowerHotbarAbilities.Contains(SNOPower.DemonHunter_Chakram) && !playerStatus.bIsIncapacitated &&
                // If we have elemental arrow or rapid fire, then use chakram as a 110 second buff, instead
                ((!hashPowerHotbarAbilities.Contains(SNOPower.DemonHunter_ClusterArrow)) ||
                DateTime.Now.Subtract(dictAbilityLastUse[SNOPower.DemonHunter_Chakram]).TotalMilliseconds >= 110000) &&
                ((playerStatus.dCurrentEnergy >= 10 && !playerStatus.bWaitingForReserveEnergy) || playerStatus.dCurrentEnergy >= iWaitingReservedAmount))
            {
                return new GilesPower(SNOPower.DemonHunter_Chakram, 50f, vNullLocation, -1, CurrentTarget.iACDGuid, 0, 1, USE_SLOWLY);
            }
            // Rapid Fire
            if (!bOOCBuff && !bCurrentlyAvoiding && hashPowerHotbarAbilities.Contains(SNOPower.DemonHunter_RapidFire) && !playerStatus.bIsIncapacitated &&
                ((playerStatus.dCurrentEnergy >= 20 && !playerStatus.bWaitingForReserveEnergy) || playerStatus.dCurrentEnergy >= iWaitingReservedAmount))
            {
                // Players with grenades *AND* rapid fire should spam grenades at close-range instead
                if (hashPowerHotbarAbilities.Contains(SNOPower.DemonHunter_Grenades) && CurrentTarget.fRadiusDistance <= 18f)
                    return new GilesPower(SNOPower.DemonHunter_Grenades, 18f, vNullLocation, -1, CurrentTarget.iACDGuid, 0, 0, USE_SLOWLY);
                // Now return rapid fire, if not sending grenades instead
                return new GilesPower(SNOPower.DemonHunter_RapidFire, 50f, vNullLocation, -1, CurrentTarget.iACDGuid, 0, 0, SIGNATURE_SPAM);
            }
            // Impale
            if (!bOOCBuff && !bCurrentlyAvoiding && hashPowerHotbarAbilities.Contains(SNOPower.DemonHunter_Impale) && !playerStatus.bIsIncapacitated &&
                ((playerStatus.dCurrentEnergy >= 25 && !playerStatus.bWaitingForReserveEnergy) || playerStatus.dCurrentEnergy >= iWaitingReservedAmount) &&
                CurrentTarget.fRadiusDistance <= 50f)
            {
                return new GilesPower(SNOPower.DemonHunter_Impale, 50f, vNullLocation, -1, CurrentTarget.iACDGuid, 0, 1, USE_SLOWLY);
            }
            // Hungering Arrow
            if (!bOOCBuff && !bCurrentlyAvoiding && hashPowerHotbarAbilities.Contains(SNOPower.DemonHunter_HungeringArrow) && !playerStatus.bIsIncapacitated)
            {
                return new GilesPower(SNOPower.DemonHunter_HungeringArrow, 50f, vNullLocation, -1, CurrentTarget.iACDGuid, 0, 0, USE_SLOWLY);
            }
            // Entangling shot
            if (!bOOCBuff && !bCurrentlyAvoiding && hashPowerHotbarAbilities.Contains(SNOPower.DemonHunter_EntanglingShot) && !playerStatus.bIsIncapacitated)
            {
                return new GilesPower(SNOPower.DemonHunter_EntanglingShot, 50f, vNullLocation, -1, CurrentTarget.iACDGuid, 0, 0, USE_SLOWLY);
            }
            // Bola Shot
            if (!bOOCBuff && !bCurrentlyAvoiding && hashPowerHotbarAbilities.Contains(SNOPower.DemonHunter_BolaShot) && !playerStatus.bIsIncapacitated)
            {
                return new GilesPower(SNOPower.DemonHunter_BolaShot, 50f, vNullLocation, -1, CurrentTarget.iACDGuid, 0, 1, USE_SLOWLY);
            }
            // Grenades
            if (!bOOCBuff && !bCurrentlyAvoiding && hashPowerHotbarAbilities.Contains(SNOPower.DemonHunter_Grenades) && !playerStatus.bIsIncapacitated)
            {
                return new GilesPower(SNOPower.DemonHunter_Grenades, 40f, vNullLocation, -1, CurrentTarget.iACDGuid, 0, 1, USE_SLOWLY);
            }
            // Default attacks
            if (!bOOCBuff && !bCurrentlyAvoiding && !playerStatus.bIsIncapacitated)
            {
                return new GilesPower(SNOPower.Weapon_Ranged_Projectile, 40f, vNullLocation, -1, CurrentTarget.iACDGuid, 1, 1, USE_SLOWLY);
            }
            return defaultPower;
        }

        private static GilesPower GetDemonHunterDestroyPower()
        {
            if (hashPowerHotbarAbilities.Contains(SNOPower.DemonHunter_HungeringArrow))
                return new GilesPower(SNOPower.DemonHunter_HungeringArrow, 15f, vNullLocation, -1, -1, 0, 0, USE_SLOWLY);
            if (hashPowerHotbarAbilities.Contains(SNOPower.DemonHunter_EntanglingShot))
                return new GilesPower(SNOPower.DemonHunter_EntanglingShot, 15f, vNullLocation, -1, -1, 0, 0, USE_SLOWLY);
            if (hashPowerHotbarAbilities.Contains(SNOPower.DemonHunter_BolaShot))
                return new GilesPower(SNOPower.DemonHunter_BolaShot, 13f, vNullLocation, -1, -1, 0, 0, USE_SLOWLY);
            if (hashPowerHotbarAbilities.Contains(SNOPower.DemonHunter_Grenades))
                return new GilesPower(SNOPower.DemonHunter_Grenades, 12f, vNullLocation, -1, -1, 0, 0, USE_SLOWLY);
            if (hashPowerHotbarAbilities.Contains(SNOPower.DemonHunter_ElementalArrow) && playerStatus.dCurrentEnergy >= 10)
                return new GilesPower(SNOPower.DemonHunter_ElementalArrow, 15f, vNullLocation, -1, -1, 0, 0, USE_SLOWLY);
            if (hashPowerHotbarAbilities.Contains(SNOPower.DemonHunter_RapidFire) && playerStatus.dCurrentEnergy >= 10)
                return new GilesPower(SNOPower.DemonHunter_RapidFire, 15f, vNullLocation, -1, -1, 0, 0, USE_SLOWLY);
            if (hashPowerHotbarAbilities.Contains(SNOPower.DemonHunter_Chakram) && playerStatus.dCurrentEnergy >= 20)
                return new GilesPower(SNOPower.DemonHunter_Chakram, 15f, vNullLocation, -1, -1, 0, 0, USE_SLOWLY);
            return new GilesPower(SNOPower.Weapon_Ranged_Instant, 20f, vNullLocation, -1, CurrentTarget.iACDGuid, 0, 0, USE_SLOWLY);
        }
    }
}
