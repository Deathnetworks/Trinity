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
        private static GilesPower GetDemonHunterPower(bool bCurrentlyAvoiding, bool bOOCBuff, bool bDestructiblePower)
        {
            // Pick the best destructible power available
            if (bDestructiblePower)
            {
                return GetDemonHunterDestroyPower();
            }
            iWaitingReservedAmount = 25;
            // Shadow Power
            if (!bOOCBuff && hashPowerHotbarAbilities.Contains(SNOPower.DemonHunter_ShadowPower) && !playerStatus.IsIncapacitated &&
                playerStatus.Discipline >= 14 &&
                (playerStatus.CurrentHealthPct <= 0.99 || playerStatus.IsRooted || iElitesWithinRange[RANGE_25] >= 1 || iAnythingWithinRange[RANGE_15] >= 3) &&
                GilesUseTimer(SNOPower.DemonHunter_ShadowPower))
            {
                return new GilesPower(SNOPower.DemonHunter_ShadowPower, 0f, vNullLocation, iCurrentWorldID, -1, 1, 1, USE_SLOWLY);
            }
            // Smoke Screen
            if ((!bOOCBuff || Settings.Combat.DemonHunter.SpamSmokeScreen) && hashPowerHotbarAbilities.Contains(SNOPower.DemonHunter_SmokeScreen) &&
                !GilesHasBuff(SNOPower.DemonHunter_ShadowPower) && playerStatus.Discipline >= 14 &&
                (
                 ( playerStatus.CurrentHealthPct <= 0.90 || playerStatus.IsRooted || iElitesWithinRange[RANGE_20] >= 1 || iAnythingWithinRange[RANGE_15] >= 3 || playerStatus.IsIncapacitated ) ||
                 Settings.Combat.DemonHunter.SpamSmokeScreen 
                ) &&
                GilesUseTimer(SNOPower.DemonHunter_SmokeScreen))
            {
                return new GilesPower(SNOPower.DemonHunter_SmokeScreen, 0f, vNullLocation, iCurrentWorldID, -1, 1, 1, USE_SLOWLY);
            }
            // Preparation

            if (
                (
                (( !bOOCBuff && !playerStatus.IsIncapacitated && iAnythingWithinRange[RANGE_40] >= 1 ) 
                 || Settings.Combat.DemonHunter.SpamPreparation ) 
                ) && 
                hashPowerHotbarAbilities.Contains(SNOPower.DemonHunter_Preparation) &&
                playerStatus.Discipline <= 10 &&
                //GilesUseTimer(SNOPower.DemonHunter_Preparation) && 
                //PowerManager.CanCast(SNOPower.DemonHunter_Preparation) 
                TrinityPowerManager.CanUse(SNOPower.DemonHunter_Preparation)
                )
            {
                return new GilesPower(SNOPower.DemonHunter_Preparation, 0f, vNullLocation, iCurrentWorldID, -1, 1, 1, USE_SLOWLY);
            }
            // Evasive Fire
            if ( !bOOCBuff && hashPowerHotbarAbilities.Contains(SNOPower.DemonHunter_EvasiveFire) && !playerStatus.IsIncapacitated &&
                  (((iAnythingWithinRange[RANGE_20] >= 1 || CurrentTarget.RadiusDistance <= 20f) && GilesUseTimer(SNOPower.DemonHunter_EvasiveFire)) ||
                DHHasNoPrimary()))
            {
                float range = DHHasNoPrimary() ? 70f : 0f;

                return new GilesPower(SNOPower.DemonHunter_EvasiveFire, range, vNullLocation, -1, CurrentTarget.ACDGuid, 1, 1, USE_SLOWLY);
            }
            // Companion
            if (!playerStatus.IsIncapacitated && hashPowerHotbarAbilities.Contains(SNOPower.DemonHunter_Companion) && iPlayerOwnedDHPets == 0 &&
                playerStatus.Discipline >= 10 && GilesUseTimer(SNOPower.DemonHunter_Companion))
            {
                return new GilesPower(SNOPower.DemonHunter_Companion, 0f, vNullLocation, iCurrentWorldID, -1, 2, 1, USE_SLOWLY);
            }
            // Sentry Turret
            if (!bOOCBuff && !playerStatus.IsIncapacitated && hashPowerHotbarAbilities.Contains(SNOPower.DemonHunter_Sentry) &&
                (iElitesWithinRange[RANGE_50] >= 1 || iAnythingWithinRange[RANGE_50] >= 2 ||
                (CurrentTarget.IsEliteRareUnique || CurrentTarget.IsTreasureGoblin || CurrentTarget.IsBoss)) && CurrentTarget.RadiusDistance <= 50f &&
                playerStatus.CurrentEnergy >= 30 && GilesUseTimer(SNOPower.DemonHunter_Sentry))
            {
                return new GilesPower(SNOPower.DemonHunter_Sentry, 0f, vNullLocation, iCurrentWorldID, -1, 0, 0, SIGNATURE_SPAM);
            }
            // Marked for Death
            if (!bOOCBuff && !bCurrentlyAvoiding && hashPowerHotbarAbilities.Contains(SNOPower.DemonHunter_MarkedForDeath) &&
                playerStatus.Discipline >= 3 && 
                (iElitesWithinRange[RANGE_40] >= 1 || iAnythingWithinRange[RANGE_40] >= 3 ||
                
                ((CurrentTarget.IsEliteRareUnique || CurrentTarget.IsTreasureGoblin || CurrentTarget.IsBoss) &&
                CurrentTarget.Radius <= 40 && CurrentTarget.RadiusDistance <= 40f)) &&
                GilesUseTimer(SNOPower.DemonHunter_MarkedForDeath))
            {
                return new GilesPower(SNOPower.DemonHunter_MarkedForDeath, 40f, vNullLocation, iCurrentWorldID, CurrentTarget.ACDGuid, 1, 1, USE_SLOWLY);
            }
            // Vault
            if (!bOOCBuff && !bCurrentlyAvoiding && hashPowerHotbarAbilities.Contains(SNOPower.DemonHunter_Vault) && !playerStatus.IsRooted && !playerStatus.IsIncapacitated &&
                // Only use vault to retreat if < level 60, or if in inferno difficulty for level 60's
                (playerStatus.Level < 60 || iCurrentGameDifficulty == GameDifficulty.Inferno) &&
                (CurrentTarget.RadiusDistance <= 10f || iAnythingWithinRange[RANGE_6] >= 1) &&
                ((!hashPowerHotbarAbilities.Contains(SNOPower.DemonHunter_ShadowPower) && playerStatus.Discipline >= 16) ||
                 (hashPowerHotbarAbilities.Contains(SNOPower.DemonHunter_ShadowPower) && playerStatus.Discipline >= 22)) && 
                    //GilesUseTimer(SNOPower.DemonHunter_Vault) && 
                    DateTime.Now.Subtract(GilesTrinity.dictAbilityLastUse[SNOPower.DemonHunter_Vault]).TotalMilliseconds >= GilesTrinity.Settings.Combat.DemonHunter.VaultMovementDelay &&
                    PowerManager.CanCast(SNOPower.DemonHunter_Vault))
            {
                Vector3 vNewTarget = MathEx.CalculatePointFrom(CurrentTarget.Position, playerStatus.CurrentPosition, -15f);
                return new GilesPower(SNOPower.DemonHunter_Vault, 20f, vNewTarget, iCurrentWorldID, -1, 1, 2, USE_SLOWLY);
            }
            // Rain of Vengeance
            if (!bOOCBuff && hashPowerHotbarAbilities.Contains(SNOPower.DemonHunter_RainOfVengeance) && !playerStatus.IsIncapacitated &&
                (iAnythingWithinRange[RANGE_25] >= 3 || iElitesWithinRange[RANGE_25] >= 1) &&
                GilesUseTimer(SNOPower.DemonHunter_RainOfVengeance) && PowerManager.CanCast(SNOPower.DemonHunter_RainOfVengeance))
            {
                return new GilesPower(SNOPower.DemonHunter_RainOfVengeance, 0f, vNullLocation, iCurrentWorldID, -1, 1, 1, USE_SLOWLY);
            }
            // Cluster Arrow
            if (!bOOCBuff && !bCurrentlyAvoiding && hashPowerHotbarAbilities.Contains(SNOPower.DemonHunter_ClusterArrow) && !playerStatus.IsIncapacitated &&
                playerStatus.CurrentEnergy >= 50 &&
               (iElitesWithinRange[RANGE_50] >= 1 || iAnythingWithinRange[RANGE_50] >= 5 || ((CurrentTarget.IsEliteRareUnique || CurrentTarget.IsTreasureGoblin || CurrentTarget.IsBoss) && CurrentTarget.RadiusDistance <= 69f)) &&
                GilesUseTimer(SNOPower.DemonHunter_ClusterArrow))
            {
                return new GilesPower(SNOPower.DemonHunter_ClusterArrow, 69f, vNullLocation, iCurrentWorldID, CurrentTarget.ACDGuid, 1, 1, USE_SLOWLY);
            }
            // Multi Shot
            if (!bOOCBuff && !bCurrentlyAvoiding && hashPowerHotbarAbilities.Contains(SNOPower.DemonHunter_Multishot) && !playerStatus.IsIncapacitated &&
                playerStatus.CurrentEnergy >= 30 &&
                (iElitesWithinRange[RANGE_40] >= 1 || iAnythingWithinRange[RANGE_40] >= 2 || ((CurrentTarget.IsEliteRareUnique || CurrentTarget.IsTreasureGoblin || CurrentTarget.IsBoss) && 
                CurrentTarget.RadiusDistance <= 30f)))
            {
                return new GilesPower(SNOPower.DemonHunter_Multishot, 40f, CurrentTarget.Position, iCurrentWorldID, -1, 1, 1, USE_SLOWLY);
            }
            // Fan of Knives
            if (!bOOCBuff && hashPowerHotbarAbilities.Contains(SNOPower.DemonHunter_FanOfKnives) && !playerStatus.IsIncapacitated &&
                playerStatus.CurrentEnergy >= 20 &&
                (iAnythingWithinRange[RANGE_15] >= 4 || iElitesWithinRange[RANGE_15] >= 1) &&
                GilesUseTimer(SNOPower.DemonHunter_FanOfKnives))
            {
                return new GilesPower(SNOPower.DemonHunter_FanOfKnives, 0f, vNullLocation, iCurrentWorldID, -1, 1, 1, USE_SLOWLY);
            }
            // Strafe spam - similar to barbarian whirlwind routine
            if (!bOOCBuff && !bCurrentlyAvoiding && hashPowerHotbarAbilities.Contains(SNOPower.DemonHunter_Strafe) && !playerStatus.IsIncapacitated && !playerStatus.IsRooted &&
                // Only if there's 3 guys in 25 yds
                iAnythingWithinRange[RANGE_25] >= 3 &&
                // Check for energy reservation amounts
                ((playerStatus.CurrentEnergy >= 15 && !playerStatus.WaitingForReserveEnergy) || playerStatus.CurrentEnergy >= iWaitingReservedAmount))
            {
                bool bGenerateNewZigZag = (DateTime.Now.Subtract(lastChangedZigZag).TotalMilliseconds >= 1500 ||
                    (vPositionLastZigZagCheck != vNullLocation && playerStatus.CurrentPosition == vPositionLastZigZagCheck && DateTime.Now.Subtract(lastChangedZigZag).TotalMilliseconds >= 200) ||
                    Vector3.Distance(playerStatus.CurrentPosition, vSideToSideTarget) <= 4f ||
                    CurrentTarget.ACDGuid != iACDGUIDLastWhirlwind);
                vPositionLastZigZagCheck = playerStatus.CurrentPosition;
                if (bGenerateNewZigZag)
                {
                    float fExtraDistance = CurrentTarget.CentreDistance <= 10f ? 10f : 5f;
                    //vSideToSideTarget = FindZigZagTargetLocation(CurrentTarget.vPosition, CurrentTarget.fCentreDist + fExtraDistance);
                    vSideToSideTarget = FindSafeZone(false, 1, CurrentTarget.Position, false);
                    // Resetting this to ensure the "no-spam" is reset since we changed our target location
                    powerLastSnoPowerUsed = SNOPower.None;
                    iACDGUIDLastWhirlwind = CurrentTarget.ACDGuid;
                    lastChangedZigZag = DateTime.Now;
                }
                return new GilesPower(SNOPower.DemonHunter_Strafe, 25f, vSideToSideTarget, iCurrentWorldID, -1, 0, 0, USE_SLOWLY);
            }
            // Spike Trap
            if (!bOOCBuff && !playerStatus.IsIncapacitated && hashPowerHotbarAbilities.Contains(SNOPower.DemonHunter_SpikeTrap) &&
                powerLastSnoPowerUsed != SNOPower.DemonHunter_SpikeTrap &&
                (iElitesWithinRange[RANGE_30] >= 1 || iAnythingWithinRange[RANGE_25] > 4 || ((CurrentTarget.IsEliteRareUnique || CurrentTarget.IsTreasureGoblin || CurrentTarget.IsBoss) && CurrentTarget.RadiusDistance <= 35f)) &&
                playerStatus.CurrentEnergy >= 30 && GilesUseTimer(SNOPower.DemonHunter_SpikeTrap))
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
                return new GilesPower(SNOPower.DemonHunter_SpikeTrap, 40f, vNewTarget, iCurrentWorldID, -1, 1, 1, USE_SLOWLY);
            }
            // Caltrops
            if (!bOOCBuff && hashPowerHotbarAbilities.Contains(SNOPower.DemonHunter_Caltrops) && !playerStatus.IsIncapacitated &&
                playerStatus.Discipline >= 6 && (iAnythingWithinRange[RANGE_30] >= 2 || iElitesWithinRange[RANGE_40] >= 1) &&
                GilesUseTimer(SNOPower.DemonHunter_Caltrops))
            {
                return new GilesPower(SNOPower.DemonHunter_Caltrops, 0f, vNullLocation, iCurrentWorldID, -1, 1, 1, USE_SLOWLY);
            }
            // Elemental Arrow
            if (!bOOCBuff && !bCurrentlyAvoiding && hashPowerHotbarAbilities.Contains(SNOPower.DemonHunter_ElementalArrow) && !playerStatus.IsIncapacitated &&
                ((playerStatus.CurrentEnergy >= 10 && !playerStatus.WaitingForReserveEnergy) || playerStatus.CurrentEnergy >= iWaitingReservedAmount))
            {
                // Players with grenades *AND* elemental arrow should spam grenades at close-range instead
                if (hashPowerHotbarAbilities.Contains(SNOPower.DemonHunter_Grenades) && CurrentTarget.RadiusDistance <= 18f)
                    return new GilesPower(SNOPower.DemonHunter_Grenades, 18f, vNullLocation, -1, CurrentTarget.ACDGuid, 0, 1, USE_SLOWLY);
                // Now return elemental arrow, if not sending grenades instead
                return new GilesPower(SNOPower.DemonHunter_ElementalArrow, 50f, vNullLocation, -1, CurrentTarget.ACDGuid, 0, 1, USE_SLOWLY);
            }
            // Chakram
            if (!bOOCBuff && !bCurrentlyAvoiding && hashPowerHotbarAbilities.Contains(SNOPower.DemonHunter_Chakram) && !playerStatus.IsIncapacitated &&
                // If we have elemental arrow or rapid fire, then use chakram as a 110 second buff, instead
                ((!hashPowerHotbarAbilities.Contains(SNOPower.DemonHunter_ClusterArrow)) ||
                DateTime.Now.Subtract(dictAbilityLastUse[SNOPower.DemonHunter_Chakram]).TotalMilliseconds >= 110000) &&
                ((playerStatus.CurrentEnergy >= 10 && !playerStatus.WaitingForReserveEnergy) || playerStatus.CurrentEnergy >= iWaitingReservedAmount))
            {
                return new GilesPower(SNOPower.DemonHunter_Chakram, 50f, vNullLocation, -1, CurrentTarget.ACDGuid, 0, 1, USE_SLOWLY);
            }
            // Rapid Fire
            if (!bOOCBuff && !bCurrentlyAvoiding && hashPowerHotbarAbilities.Contains(SNOPower.DemonHunter_RapidFire) && !playerStatus.IsIncapacitated &&
                ((playerStatus.CurrentEnergy >= 20 && !playerStatus.WaitingForReserveEnergy) || playerStatus.CurrentEnergy >= iWaitingReservedAmount))
            {
                // Players with grenades *AND* rapid fire should spam grenades at close-range instead
                if (hashPowerHotbarAbilities.Contains(SNOPower.DemonHunter_Grenades) && CurrentTarget.RadiusDistance <= 18f)
                    return new GilesPower(SNOPower.DemonHunter_Grenades, 18f, vNullLocation, -1, CurrentTarget.ACDGuid, 0, 0, USE_SLOWLY);
                // Now return rapid fire, if not sending grenades instead
                return new GilesPower(SNOPower.DemonHunter_RapidFire, 50f, vNullLocation, -1, CurrentTarget.ACDGuid, 0, 0, SIGNATURE_SPAM);
            }
            // Impale
            if (!bOOCBuff && !bCurrentlyAvoiding && hashPowerHotbarAbilities.Contains(SNOPower.DemonHunter_Impale) && !playerStatus.IsIncapacitated &&
                ((playerStatus.CurrentEnergy >= 25 && !playerStatus.WaitingForReserveEnergy) || playerStatus.CurrentEnergy >= iWaitingReservedAmount) &&
                CurrentTarget.RadiusDistance <= 50f)
            {
                return new GilesPower(SNOPower.DemonHunter_Impale, 50f, vNullLocation, -1, CurrentTarget.ACDGuid, 0, 1, USE_SLOWLY);
            }
            // Hungering Arrow
            if (!bOOCBuff && !bCurrentlyAvoiding && hashPowerHotbarAbilities.Contains(SNOPower.DemonHunter_HungeringArrow) && !playerStatus.IsIncapacitated)
            {
                return new GilesPower(SNOPower.DemonHunter_HungeringArrow, 50f, vNullLocation, -1, CurrentTarget.ACDGuid, 0, 0, USE_SLOWLY);
            }
            // Entangling shot
            if (!bOOCBuff && !bCurrentlyAvoiding && hashPowerHotbarAbilities.Contains(SNOPower.DemonHunter_EntanglingShot) && !playerStatus.IsIncapacitated)
            {
                return new GilesPower(SNOPower.DemonHunter_EntanglingShot, 50f, vNullLocation, -1, CurrentTarget.ACDGuid, 0, 0, USE_SLOWLY);
            }
            // Bola Shot
            if (!bOOCBuff && !bCurrentlyAvoiding && hashPowerHotbarAbilities.Contains(SNOPower.DemonHunter_BolaShot) && !playerStatus.IsIncapacitated)
            {
                return new GilesPower(SNOPower.DemonHunter_BolaShot, 50f, vNullLocation, -1, CurrentTarget.ACDGuid, 0, 1, USE_SLOWLY);
            }
            // Grenades
            if (!bOOCBuff && !bCurrentlyAvoiding && hashPowerHotbarAbilities.Contains(SNOPower.DemonHunter_Grenades) && !playerStatus.IsIncapacitated)
            {
                return new GilesPower(SNOPower.DemonHunter_Grenades, 40f, vNullLocation, -1, CurrentTarget.ACDGuid, 0, 1, USE_SLOWLY);
            }
            // Default attacks
            if (!bOOCBuff && !bCurrentlyAvoiding && !playerStatus.IsIncapacitated)
            {
                return new GilesPower(SNOPower.Weapon_Ranged_Projectile, 40f, vNullLocation, -1, CurrentTarget.ACDGuid, 1, 1, USE_SLOWLY);
            }
            return defaultPower;
        }

        private static bool DHHasNoPrimary()
        {
            return !hashPowerHotbarAbilities.Contains(SNOPower.DemonHunter_BolaShot) ||
                                !hashPowerHotbarAbilities.Contains(SNOPower.DemonHunter_EntanglingShot) ||
                                !hashPowerHotbarAbilities.Contains(SNOPower.DemonHunter_Grenades) ||
                                !hashPowerHotbarAbilities.Contains(SNOPower.DemonHunter_HungeringArrow);
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
            if (hashPowerHotbarAbilities.Contains(SNOPower.DemonHunter_ElementalArrow) && playerStatus.CurrentEnergy >= 10)
                return new GilesPower(SNOPower.DemonHunter_ElementalArrow, 15f, vNullLocation, -1, -1, 0, 0, USE_SLOWLY);
            if (hashPowerHotbarAbilities.Contains(SNOPower.DemonHunter_RapidFire) && playerStatus.CurrentEnergy >= 10)
                return new GilesPower(SNOPower.DemonHunter_RapidFire, 15f, vNullLocation, -1, -1, 0, 0, USE_SLOWLY);
            if (hashPowerHotbarAbilities.Contains(SNOPower.DemonHunter_Chakram) && playerStatus.CurrentEnergy >= 20)
                return new GilesPower(SNOPower.DemonHunter_Chakram, 15f, vNullLocation, -1, -1, 0, 0, USE_SLOWLY);
            if (hashPowerHotbarAbilities.Contains(SNOPower.DemonHunter_EvasiveFire) && playerStatus.CurrentEnergy >= 20)
                return new GilesPower(SNOPower.DemonHunter_EvasiveFire, 15f, vNullLocation, -1, -1, 0, 0, USE_SLOWLY);
            return new GilesPower(SNOPower.Weapon_Ranged_Instant, 20f, vNullLocation, -1, CurrentTarget.ACDGuid, 0, 0, USE_SLOWLY);
        }
    }
}
