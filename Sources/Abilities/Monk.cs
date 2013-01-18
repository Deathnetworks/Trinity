using GilesTrinity.Technicals;
using System;
using Zeta.Common;
using Zeta.Common.Plugins;
using Zeta.CommonBot;
using Zeta.Internals.Actors;

namespace GilesTrinity
{
    public partial class GilesTrinity : IPlugin
    {
        private static TrinityPower GetMonkPower(bool bCurrentlyAvoiding, bool bOOCBuff, bool bDestructiblePower)
        {
            if (!weaponSwap.DpsGearOn() && DateTime.Now.Subtract(WeaponSwapTime).TotalMilliseconds > 3000 && !WantToSwap && DateTime.Now.Subtract(WeaponSwapTime).TotalMilliseconds < 6000)
            {
                weaponSwap.ItemsInPlace();
                if (Zeta.Internals.UIElements.InventoryWindow.IsVisible)
                    Zeta.Internals.UIElements.BackgroundScreenPCButtonInventory.Click();
            }
            if (!bOOCBuff && !bCurrentlyAvoiding && GetHasBuff(SNOPower.Monk_SweepingWind) && DateTime.Now.Subtract(SweepWindSpam).TotalMilliseconds <= 4000 && !PlayerStatus.IsIncapacitated &&
                AnythingWithinRange[RANGE_15] >= 1 && CurrentTarget.RadiusDistance <= 15f)
            {
                SweepWindSpam = DateTime.Now;
            }

            // Do weapon swapping checks
            if (weaponSwap.DpsGearOn() && Settings.Combat.Monk.SweepingWindWeaponSwap && (DateTime.Now.Subtract(WeaponSwapTime).TotalMilliseconds > 1500 ||
                    DateTime.Now.Subtract(WeaponSwapTime).TotalMilliseconds > 800 && GetHasBuff(SNOPower.Monk_SweepingWind)) || WantToSwap &&
                    (DateTime.Now.Subtract(ForeSightFirstHit).TotalMilliseconds >= 1400 && DateTime.Now.Subtract(ForeSightFirstHit).TotalMilliseconds <= 29000 &&
                    DateTime.Now.Subtract(OtherThanDeadlyReach).TotalMilliseconds <= 2700 || !Hotbar.Contains(SNOPower.Monk_DeadlyReach)))
            {
                WantToSwap = false;
                if (!weaponSwap.DpsGearOn())
                {
                    WeaponSwapTime = DateTime.Now;
                }
                weaponSwap.SwapGear();
            }

            // Blinding flash after swap
            if (Settings.Combat.Monk.SweepingWindWeaponSwap && weaponSwap.DpsGearOn() && PowerManager.CanCast(SNOPower.Monk_BlindingFlash) && DateTime.Now.Subtract(WeaponSwapTime).TotalMilliseconds >= 200 &&
                !GetHasBuff(SNOPower.Monk_SweepingWind) && (PlayerStatus.CurrentEnergy >= 85 || (Settings.Combat.Monk.HasInnaSet && PlayerStatus.CurrentEnergy >= 15))
                && Hotbar.Contains(SNOPower.Monk_BlindingFlash))
            {
                return new TrinityPower(SNOPower.Monk_BlindingFlash, 0f, vNullLocation, iCurrentWorldID, -1, 0, 1, USE_SLOWLY);
            }
            // Sweeping winds spam
            if ((PlayerStatus.CurrentEnergy >= 75 || (Settings.Combat.Monk.HasInnaSet && PlayerStatus.CurrentEnergy >= 5))
                && (GetHasBuff(SNOPower.Monk_SweepingWind) && DateTime.Now.Subtract(SweepWindSpam).TotalMilliseconds >= 3700 && DateTime.Now.Subtract(SweepWindSpam).TotalMilliseconds < 5100
                || !GetHasBuff(SNOPower.Monk_SweepingWind) && weaponSwap.DpsGearOn() && DateTime.Now.Subtract(WeaponSwapTime).TotalMilliseconds >= 400)
                && Hotbar.Contains(SNOPower.Monk_SweepingWind))
            {
                SweepWindSpam = DateTime.Now;
                return new TrinityPower(SNOPower.Monk_SweepingWind, 0f, vNullLocation, iCurrentWorldID, -1, 0, 1, USE_SLOWLY);
            }
            // Pick the best destructible power available
            if (bDestructiblePower)
            {
                return GetMonkDestroyPower();
            }






            // Monks need 80 for special spam like tempest rushing
            MinEnergyReserve = 80;
            // 4 Mantras for the initial buff (slow-use)
            if (Hotbar.Contains(SNOPower.Monk_MantraOfEvasion) && !GetHasBuff(SNOPower.Monk_MantraOfEvasion) &&
                PlayerStatus.CurrentEnergy >= 50 && GilesUseTimer(SNOPower.Monk_MantraOfEvasion, true))
            {
                return new TrinityPower(SNOPower.Monk_MantraOfEvasion, 0f, vNullLocation, iCurrentWorldID, -1, 0, 1, USE_SLOWLY);
            }
            if (Hotbar.Contains(SNOPower.Monk_MantraOfConviction) && !GetHasBuff(SNOPower.Monk_MantraOfConviction) &&
                (PlayerStatus.CurrentEnergy >= 50 && !Settings.Combat.Monk.SweepingWindWeaponSwap || PlayerStatus.CurrentEnergy >= 85) && GilesUseTimer(SNOPower.Monk_MantraOfConviction, true))
            {
                return new TrinityPower(SNOPower.Monk_MantraOfConviction, 0f, vNullLocation, iCurrentWorldID, -1, 0, 1, USE_SLOWLY);
            }
            if (Hotbar.Contains(SNOPower.Monk_MantraOfHealing) && !GetHasBuff(SNOPower.Monk_MantraOfHealing) &&
                PlayerStatus.CurrentEnergy >= 50 && GilesUseTimer(SNOPower.Monk_MantraOfHealing, true))
            {
                return new TrinityPower(SNOPower.Monk_MantraOfHealing, 0f, vNullLocation, iCurrentWorldID, -1, 0, 1, USE_SLOWLY);
            }
            if (Hotbar.Contains(SNOPower.Monk_MantraOfRetribution) && !GetHasBuff(SNOPower.Monk_MantraOfRetribution) &&
                PlayerStatus.CurrentEnergy >= 50 && GilesUseTimer(SNOPower.Monk_MantraOfRetribution, true))
            {
                return new TrinityPower(SNOPower.Monk_MantraOfRetribution, 0f, vNullLocation, iCurrentWorldID, -1, 0, 1, USE_SLOWLY);
            }
            // Mystic ally
            if (Hotbar.Contains(SNOPower.Monk_MysticAlly) && PlayerStatus.CurrentEnergy >= 90 && iPlayerOwnedMysticAlly == 0 &&
                GilesUseTimer(SNOPower.Monk_MysticAlly) && PowerManager.CanCast(SNOPower.Monk_MysticAlly))
            {
                return new TrinityPower(SNOPower.Monk_MysticAlly, 0f, vNullLocation, iCurrentWorldID, -1, 2, 2, USE_SLOWLY);
            }
            // InnerSanctuary
            if (!bOOCBuff && PlayerStatus.CurrentHealthPct <= 0.45 && Hotbar.Contains(SNOPower.Monk_InnerSanctuary) &&
                GilesUseTimer(SNOPower.Monk_InnerSanctuary, true) &&
                PlayerStatus.CurrentEnergy >= 30 && PowerManager.CanCast(SNOPower.Monk_InnerSanctuary))
            {
                return new TrinityPower(SNOPower.Monk_InnerSanctuary, 0f, vNullLocation, iCurrentWorldID, -1, 1, 1, USE_SLOWLY);
            }
            // Serenity if health is low
            if ((PlayerStatus.CurrentHealthPct <= 0.50 || (PlayerStatus.IsIncapacitated && PlayerStatus.CurrentHealthPct <= 0.90)) && Hotbar.Contains(SNOPower.Monk_Serenity) &&
                GilesUseTimer(SNOPower.Monk_Serenity, true) &&
                PlayerStatus.CurrentEnergy >= 10 && PowerManager.CanCast(SNOPower.Monk_Serenity))
            {
                return new TrinityPower(SNOPower.Monk_Serenity, 0f, vNullLocation, iCurrentWorldID, -1, 1, 1, USE_SLOWLY);
            }
            // Breath of heaven when needing healing or the buff
            if (!bOOCBuff && (PlayerStatus.CurrentHealthPct <= 0.6 || !GetHasBuff(SNOPower.Monk_BreathOfHeaven)) && Hotbar.Contains(SNOPower.Monk_BreathOfHeaven) &&
                (PlayerStatus.CurrentEnergy >= 35 || (!Hotbar.Contains(SNOPower.Monk_Serenity) && PlayerStatus.CurrentEnergy >= 25)) &&
                GilesUseTimer(SNOPower.Monk_BreathOfHeaven) && PowerManager.CanCast(SNOPower.Monk_BreathOfHeaven))
            {
                return new TrinityPower(SNOPower.Monk_BreathOfHeaven, 0f, vNullLocation, iCurrentWorldID, -1, 1, 1, USE_SLOWLY);
            }
            // Blinding Flash
            if (!bOOCBuff && PlayerStatus.CurrentEnergy >= 20 && Hotbar.Contains(SNOPower.Monk_BlindingFlash) &&
                (
                    ElitesWithinRange[RANGE_15] >= 1 || PlayerStatus.CurrentHealthPct <= 0.4 || (AnythingWithinRange[RANGE_20] >= 5 && ElitesWithinRange[RANGE_50] == 0) ||
                    (AnythingWithinRange[RANGE_15] >= 3 && PlayerStatus.CurrentEnergyPct <= 0.5) || (CurrentTarget.IsBoss && CurrentTarget.RadiusDistance <= 15f) ||
                    (AnythingWithinRange[RANGE_15] >= 1 && Hotbar.Contains(SNOPower.Monk_SweepingWind) && !GetHasBuff(SNOPower.Monk_SweepingWind) && Settings.Combat.Monk.HasInnaSet)
                ) &&
                // Check if we don't have breath of heaven
                (!Hotbar.Contains(SNOPower.Monk_BreathOfHeaven) ||
                (Hotbar.Contains(SNOPower.Monk_BreathOfHeaven) && (!Settings.Combat.Monk.HasInnaSet || GetHasBuff(SNOPower.Monk_BreathOfHeaven)))) &&
                // Check if either we don't have sweeping winds, or we do and it's ready to cast in a moment
                (!Hotbar.Contains(SNOPower.Monk_SweepingWind) ||
                (Hotbar.Contains(SNOPower.Monk_SweepingWind) && (PlayerStatus.CurrentEnergy >= 85 ||
                (Settings.Combat.Monk.HasInnaSet && PlayerStatus.CurrentEnergy >= 15) || GetHasBuff(SNOPower.Monk_SweepingWind))) ||
                PlayerStatus.CurrentHealthPct <= 0.25) &&
                GilesUseTimer(SNOPower.Monk_BlindingFlash) && PowerManager.CanCast(SNOPower.Monk_BlindingFlash))
            {
                if (!weaponSwap.DpsGearOn() && Settings.Combat.Monk.SweepingWindWeaponSwap && !GetHasBuff(SNOPower.Monk_SweepingWind) && weaponSwap.CanSwap())
                {
                    WantToSwap = true;
                }
                if (!Settings.Combat.Monk.SweepingWindWeaponSwap || !weaponSwap.CanSwap() || (weaponSwap.CanSwap() && CheckAbilityAndBuff(SNOPower.Monk_SweepingWind)))
                {
                    return new TrinityPower(SNOPower.Monk_BlindingFlash, 0f, vNullLocation, iCurrentWorldID, -1, 0, 1, USE_SLOWLY); //intell -- 11f -- 1, 2
                }
            }
            // Blinding Flash as a DEFENSE
            if (!bOOCBuff && PlayerStatus.CurrentEnergy >= 10 && Hotbar.Contains(SNOPower.Monk_BlindingFlash) &&
                PlayerStatus.CurrentHealthPct <= 0.25 && AnythingWithinRange[RANGE_15] >= 1 &&
                GilesUseTimer(SNOPower.Monk_BlindingFlash) && PowerManager.CanCast(SNOPower.Monk_BlindingFlash) && (!Settings.Combat.Monk.SweepingWindWeaponSwap || !weaponSwap.CanSwap()))
            {
                return new TrinityPower(SNOPower.Monk_BlindingFlash, 0f, vNullLocation, iCurrentWorldID, -1, 0, 1, USE_SLOWLY); //intell -- 11f -- 1, 2
            }


            // Sweeping wind
            if (!bOOCBuff && Hotbar.Contains(SNOPower.Monk_SweepingWind) && !GetHasBuff(SNOPower.Monk_SweepingWind) &&
                (ElitesWithinRange[RANGE_25] > 0 || AnythingWithinRange[RANGE_20] >= 3 || (AnythingWithinRange[RANGE_20] >= 1 && Settings.Combat.Monk.HasInnaSet) || (CurrentTarget.IsBossOrEliteRareUnique && CurrentTarget.RadiusDistance <= 25f)) &&
                // Check if either we don't have blinding flash, or we do and it's been cast in the last 6000ms
                (DateTime.Now.Subtract(dictAbilityLastUse[SNOPower.Monk_BlindingFlash]).TotalMilliseconds <= 8000 || CheckAbilityAndBuff(SNOPower.Monk_BlindingFlash) ||
                ElitesWithinRange[RANGE_25] > 0 && DateTime.Now.Subtract(dictAbilityLastUse[SNOPower.Monk_BlindingFlash]).TotalMilliseconds <= 12500) &&
                // Check our mantras, if we have them, are up first
                (Settings.Combat.Monk.SweepingWindWeaponSwap ||
                CheckAbilityAndBuff(SNOPower.Monk_MantraOfConviction) &&
                CheckAbilityAndBuff(SNOPower.Monk_MantraOfEvasion) &&
                CheckAbilityAndBuff(SNOPower.Monk_MantraOfRetribution)) &&
                // Check the re-use timer and energy costs
                (PlayerStatus.CurrentEnergy >= 75 || (Settings.Combat.Monk.HasInnaSet && PlayerStatus.CurrentEnergy >= 5)))
            {
                if (!weaponSwap.DpsGearOn() && Settings.Combat.Monk.SweepingWindWeaponSwap && weaponSwap.CanSwap())
                {
                    WantToSwap = true;
                }
                if (!Settings.Combat.Monk.SweepingWindWeaponSwap || !weaponSwap.CanSwap())
                {
                    SweepWindSpam = DateTime.Now;
                    return new TrinityPower(SNOPower.Monk_SweepingWind, 0f, vNullLocation, iCurrentWorldID, -1, 0, 1, USE_SLOWLY); //intell -- 2,2
                }
            }






            // Seven Sided Strike
            if (!bOOCBuff && !bCurrentlyAvoiding && !PlayerStatus.IsIncapacitated &&
                (ElitesWithinRange[RANGE_15] >= 1 || (CurrentTarget.IsBossOrEliteRareUnique && CurrentTarget.RadiusDistance <= 15f) || PlayerStatus.CurrentHealthPct <= 0.55) &&
                Hotbar.Contains(SNOPower.Monk_SevenSidedStrike) && ((PlayerStatus.CurrentEnergy >= 50 && !PlayerStatus.WaitingForReserveEnergy) || PlayerStatus.CurrentEnergy >= MinEnergyReserve) &&
                GilesUseTimer(SNOPower.Monk_SevenSidedStrike, true) && PowerManager.CanCast(SNOPower.Monk_SevenSidedStrike))
            {
                return new TrinityPower(SNOPower.Monk_SevenSidedStrike, 16f, CurrentTarget.Position, iCurrentWorldID, -1, 2, 3, USE_SLOWLY);
            }
            // Exploding Palm
            if (!bOOCBuff && !bCurrentlyAvoiding && !PlayerStatus.IsIncapacitated &&
                (ElitesWithinRange[RANGE_25] > 0 || AnythingWithinRange[RANGE_15] >= 3 || (CurrentTarget.IsBossOrEliteRareUnique && CurrentTarget.RadiusDistance <= 14f)) &&
                Hotbar.Contains(SNOPower.Monk_ExplodingPalm) &&
                ((PlayerStatus.CurrentEnergy >= 40 && !PlayerStatus.WaitingForReserveEnergy) || PlayerStatus.CurrentEnergy >= MinEnergyReserve) &&
                GilesUseTimer(SNOPower.Monk_ExplodingPalm) && PowerManager.CanCast(SNOPower.Monk_ExplodingPalm))
            {
                return new TrinityPower(SNOPower.Monk_ExplodingPalm, 14f, vNullLocation, -1, CurrentTarget.ACDGuid, 1, 1, USE_SLOWLY);
            }
            // Cyclone Strike
            if (!bOOCBuff && !bCurrentlyAvoiding && !PlayerStatus.IsIncapacitated &&
                (ElitesWithinRange[RANGE_20] >= 1 || AnythingWithinRange[RANGE_20] >= 2 || 
                (CurrentTarget.IsBossOrEliteRareUnique && CurrentTarget.RadiusDistance <= 18f)) &&
                Hotbar.Contains(SNOPower.Monk_CycloneStrike) &&
                ((PlayerStatus.CurrentEnergy >= 50 && !PlayerStatus.WaitingForReserveEnergy) || PlayerStatus.CurrentEnergy >= MinEnergyReserve) &&
                GilesUseTimer(SNOPower.Monk_CycloneStrike) && PowerManager.CanCast(SNOPower.Monk_CycloneStrike))
            {
                return new TrinityPower(SNOPower.Monk_CycloneStrike, 0f, vNullLocation, iCurrentWorldID, -1, 2, 2, USE_SLOWLY);
            }
            // 4 Mantra spam for the 4 second buff
            if (!bOOCBuff && !Settings.Combat.Monk.DisableMantraSpam && (!Hotbar.Contains(SNOPower.Monk_TempestRush) || PlayerStatus.CurrentEnergy >= 98 ||
                (PlayerStatus.CurrentHealthPct <= 0.55 && PlayerStatus.CurrentEnergy >= 75) || CurrentTarget.IsBoss) &&
                    (PlayerStatus.CurrentEnergy >= 135 ||
                    (GetHasBuff(SNOPower.Monk_SweepingWind) && (PlayerStatus.CurrentEnergy >= 60 && !Settings.Combat.Monk.SweepingWindWeaponSwap ||
                    PlayerStatus.CurrentEnergy >= 110 || (PlayerStatus.CurrentEnergy >= 100 && PlayerStatus.CurrentHealthPct >= 0.6) ||
                    (PlayerStatus.CurrentEnergy >= 50 && PlayerStatus.CurrentHealthPct >= 0.6 && !Settings.Combat.Monk.SweepingWindWeaponSwap)) &&
                // Checking we have no expensive finishers
                    !Hotbar.Contains(SNOPower.Monk_SevenSidedStrike) && !Hotbar.Contains(SNOPower.Monk_LashingTailKick) &&
                    !Hotbar.Contains(SNOPower.Monk_WaveOfLight) && !Hotbar.Contains(SNOPower.Monk_CycloneStrike) &&
                    !Hotbar.Contains(SNOPower.Monk_ExplodingPalm))) &&
                (ElitesWithinRange[RANGE_15] >= 1 || AnythingWithinRange[RANGE_15] >= 3 ||
                (AnythingWithinRange[RANGE_15] >= 1 && (Settings.Combat.Monk.HasInnaSet && PlayerStatus.CurrentEnergy >= 70)))) //intell -- inna
            {
                if (Hotbar.Contains(SNOPower.Monk_MantraOfEvasion) && GilesUseTimer(SNOPower.Monk_MantraOfEvasion))
                {
                    return new TrinityPower(SNOPower.Monk_MantraOfEvasion, 0f, vNullLocation, iCurrentWorldID, -1, 1, 1, USE_SLOWLY);
                }
                if (Hotbar.Contains(SNOPower.Monk_MantraOfConviction) && GilesUseTimer(SNOPower.Monk_MantraOfConviction))
                {
                    return new TrinityPower(SNOPower.Monk_MantraOfConviction, 0f, vNullLocation, iCurrentWorldID, -1, 1, 1, USE_SLOWLY);
                }
                if (Hotbar.Contains(SNOPower.Monk_MantraOfRetribution) && GilesUseTimer(SNOPower.Monk_MantraOfRetribution))
                {
                    return new TrinityPower(SNOPower.Monk_MantraOfRetribution, 0f, vNullLocation, iCurrentWorldID, -1, 1, 1, USE_SLOWLY);
                }
                if (Hotbar.Contains(SNOPower.Monk_MantraOfHealing) && GilesUseTimer(SNOPower.Monk_MantraOfHealing))
                {
                    return new TrinityPower(SNOPower.Monk_MantraOfHealing, 0f, vNullLocation, iCurrentWorldID, -1, 1, 1, USE_SLOWLY);
                }
            }
            // Lashing Tail Kick
            if (!bOOCBuff && !bCurrentlyAvoiding && Hotbar.Contains(SNOPower.Monk_LashingTailKick) && !PlayerStatus.IsIncapacitated &&
                (ElitesWithinRange[RANGE_15] > 0 || AnythingWithinRange[RANGE_15] > 4 || (CurrentTarget.IsBossOrEliteRareUnique && CurrentTarget.RadiusDistance <= 10f)) &&
                // Either doesn't have sweeping wind, or does but the buff is already up
                (!Hotbar.Contains(SNOPower.Monk_SweepingWind) || (Hotbar.Contains(SNOPower.Monk_SweepingWind) && GetHasBuff(SNOPower.Monk_SweepingWind))) &&
                GilesUseTimer(SNOPower.Monk_LashingTailKick) &&
                ((PlayerStatus.CurrentEnergy >= 65 && !PlayerStatus.WaitingForReserveEnergy) || PlayerStatus.CurrentEnergy >= MinEnergyReserve))
            {
                return new TrinityPower(SNOPower.Monk_LashingTailKick, 10f, vNullLocation, -1, CurrentTarget.ACDGuid, 1, 1, USE_SLOWLY);
            }
            // Wave of light
            if (!bOOCBuff && !bCurrentlyAvoiding && !PlayerStatus.IsIncapacitated &&
                (ElitesWithinRange[RANGE_25] > 0 ||
                (CurrentTarget.IsBossOrEliteRareUnique && CurrentTarget.RadiusDistance <= 14f) ||
                AnythingWithinRange[RANGE_15] > 2) &&
                Hotbar.Contains(SNOPower.Monk_WaveOfLight) &&
                GilesUseTimer(SNOPower.Monk_WaveOfLight) &&
                (PlayerStatus.CurrentEnergy >= 90 || PlayerStatus.CurrentEnergyPct >= 0.85) && HasMonkMantraAbilityAndBuff())
            {
                return new TrinityPower(SNOPower.Monk_WaveOfLight, 16f, vNullLocation, -1, CurrentTarget.ACDGuid, 1, 1, USE_SLOWLY);
            }
            // Tempest rush at elites or groups of mobs
            if (!bOOCBuff && !Settings.Combat.Monk.UseTRMovement && !bCurrentlyAvoiding && !PlayerStatus.IsIncapacitated && !PlayerStatus.IsRooted &&
                (ElitesWithinRange[RANGE_25] > 0 || (CurrentTarget.IsBossOrEliteRareUnique && CurrentTarget.RadiusDistance <= 14f) || AnythingWithinRange[RANGE_15] > 2) &&
                Hotbar.Contains(SNOPower.Monk_TempestRush) && ((PlayerStatus.CurrentEnergy >= 20 && !PlayerStatus.WaitingForReserveEnergy) || PlayerStatus.CurrentEnergy >= MinEnergyReserve) &&
                PowerManager.CanCast(SNOPower.Monk_TempestRush))
            {
                bool bGenerateNewZigZag = (DateTime.Now.Subtract(lastChangedZigZag).TotalMilliseconds >= 1500 ||
                    (vPositionLastZigZagCheck != vNullLocation && PlayerStatus.CurrentPosition == vPositionLastZigZagCheck && DateTime.Now.Subtract(lastChangedZigZag).TotalMilliseconds >= 200) ||
                    Vector3.Distance(PlayerStatus.CurrentPosition, vSideToSideTarget) <= 4f ||
                    CurrentTarget.ACDGuid != iACDGUIDLastWhirlwind);
                vPositionLastZigZagCheck = PlayerStatus.CurrentPosition;
                if (bGenerateNewZigZag)
                {
                    float fExtraDistance = CurrentTarget.CentreDistance <= 20f ? 15f : 5f;
                    vSideToSideTarget = FindZigZagTargetLocation(CurrentTarget.Position, CurrentTarget.CentreDistance + fExtraDistance);
                    // Resetting this to ensure the "no-spam" is reset since we changed our target location
                    powerLastSnoPowerUsed = SNOPower.None;
                    iACDGUIDLastWhirlwind = CurrentTarget.ACDGuid;
                    lastChangedZigZag = DateTime.Now;
                }
                return new TrinityPower(SNOPower.Monk_TempestRush, 23f, vSideToSideTarget, iCurrentWorldID, -1, 0, 0, USE_SLOWLY);
            }
            // Dashing Strike
            if (!bOOCBuff && !bCurrentlyAvoiding && !PlayerStatus.IsIncapacitated &&
                (ElitesWithinRange[RANGE_25] > 0 || (CurrentTarget.IsBossOrEliteRareUnique && CurrentTarget.RadiusDistance <= 14f) || AnythingWithinRange[RANGE_15] > 2) &&
                Hotbar.Contains(SNOPower.Monk_DashingStrike) && ((PlayerStatus.CurrentEnergy >= 30 && !PlayerStatus.WaitingForReserveEnergy) || PlayerStatus.CurrentEnergy >= MinEnergyReserve))
            {
                return new TrinityPower(SNOPower.Monk_DashingStrike, 14f, vNullLocation, -1, CurrentTarget.ACDGuid, 0, 1, USE_SLOWLY);
            }

            // Fists of thunder as the primary, repeatable attack
            if (!bOOCBuff && !bCurrentlyAvoiding && Hotbar.Contains(SNOPower.Monk_FistsofThunder)
                && (DateTime.Now.Subtract(OtherThanDeadlyReach).TotalMilliseconds < 2700 && DateTime.Now.Subtract(ForeSightFirstHit).TotalMilliseconds < 29000 || !Hotbar.Contains(SNOPower.Monk_DeadlyReach) || CurrentTarget.RadiusDistance > 12f ||
                AnythingWithinRange[RANGE_50] < 5 && ElitesWithinRange[RANGE_50] <= 0 && !WantToSwap))
            {
                if (DateTime.Now.Subtract(OtherThanDeadlyReach).TotalMilliseconds < 2700)
                    OtherThanDeadlyReach = DateTime.Now;
                if (GetHasBuff(SNOPower.Monk_SweepingWind) && DateTime.Now.Subtract(SweepWindSpam).TotalMilliseconds < 5500)
                    SweepWindSpam = DateTime.Now; //intell -- inna
                return new TrinityPower(SNOPower.Monk_FistsofThunder, 30f, vNullLocation, -1, CurrentTarget.ACDGuid, 0, 0, SIGNATURE_SPAM);
            }
            // Crippling wave
            if (!bOOCBuff && !bCurrentlyAvoiding && Hotbar.Contains(SNOPower.Monk_CripplingWave)
                && (DateTime.Now.Subtract(OtherThanDeadlyReach).TotalMilliseconds < 2700 && DateTime.Now.Subtract(ForeSightFirstHit).TotalMilliseconds < 29000 || !Hotbar.Contains(SNOPower.Monk_DeadlyReach)
                || AnythingWithinRange[RANGE_50] < 5 && ElitesWithinRange[RANGE_50] <= 0 && !WantToSwap))
            {
                OtherThanDeadlyReach = DateTime.Now;
                if (GetHasBuff(SNOPower.Monk_SweepingWind) && DateTime.Now.Subtract(SweepWindSpam).TotalMilliseconds < 5500)
                    SweepWindSpam = DateTime.Now; //intell -- inna
                return new TrinityPower(SNOPower.Monk_CripplingWave, 14f, vNullLocation, -1, CurrentTarget.ACDGuid, 0, 1, USE_SLOWLY);
            }
            // Way of hundred fists
            if (!bOOCBuff && !bCurrentlyAvoiding && Hotbar.Contains(SNOPower.Monk_WayOfTheHundredFists)
                && (DateTime.Now.Subtract(OtherThanDeadlyReach).TotalMilliseconds < 2700 && DateTime.Now.Subtract(ForeSightFirstHit).TotalMilliseconds < 29000 || !Hotbar.Contains(SNOPower.Monk_DeadlyReach)
                || AnythingWithinRange[RANGE_50] < 5 && ElitesWithinRange[RANGE_50] <= 0 && !WantToSwap))
            {
                OtherThanDeadlyReach = DateTime.Now;
                if (GetHasBuff(SNOPower.Monk_SweepingWind) && DateTime.Now.Subtract(SweepWindSpam).TotalMilliseconds < 5500)
                    SweepWindSpam = DateTime.Now; //intell -- inna
                return new TrinityPower(SNOPower.Monk_WayOfTheHundredFists, 14f, vNullLocation, -1, CurrentTarget.ACDGuid, 0, 1, SIGNATURE_SPAM);
            }
            // Deadly reach
            if (!bOOCBuff && !bCurrentlyAvoiding && Hotbar.Contains(SNOPower.Monk_DeadlyReach))
            {
                if (DateTime.Now.Subtract(ForeSightFirstHit).TotalMilliseconds > 29000)
                {
                    ForeSightFirstHit = DateTime.Now;
                }
                else if (DateTime.Now.Subtract(ForeSight2).TotalMilliseconds > 400 && DateTime.Now.Subtract(ForeSightFirstHit).TotalMilliseconds > 1400)
                {
                    OtherThanDeadlyReach = DateTime.Now;
                }
                if (DateTime.Now.Subtract(ForeSight2).TotalMilliseconds > 2800)
                {
                    ForeSight2 = DateTime.Now;
                }
                if (GetHasBuff(SNOPower.Monk_SweepingWind) && DateTime.Now.Subtract(SweepWindSpam).TotalMilliseconds < 5500)
                    SweepWindSpam = DateTime.Now; //intell -- inna
                return new TrinityPower(SNOPower.Monk_DeadlyReach, 16f, vNullLocation, -1, CurrentTarget.ACDGuid, 0, 1, USE_SLOWLY);
            }
            // Default attacks
            if (!bOOCBuff && !bCurrentlyAvoiding && !PlayerStatus.IsIncapacitated)
            {
                if (GetHasBuff(SNOPower.Monk_SweepingWind) && DateTime.Now.Subtract(SweepWindSpam).TotalMilliseconds < 5500)
                    SweepWindSpam = DateTime.Now; //intell -- inna
                return new TrinityPower(SNOPower.Weapon_Melee_Instant, 10f, vNullLocation, -1, CurrentTarget.ACDGuid, 1, 1, USE_SLOWLY);
            }
            return defaultPower;
        }

        private static TrinityPower GetMonkDestroyPower()
        {
            if (Hotbar.Contains(SNOPower.Monk_FistsofThunder))
                return new TrinityPower(SNOPower.Monk_FistsofThunder, 15f, vNullLocation, -1, -1, 0, 0, USE_SLOWLY);
            if (Hotbar.Contains(SNOPower.Monk_DeadlyReach))
                return new TrinityPower(SNOPower.Monk_DeadlyReach, 10f, vNullLocation, -1, -1, 0, 0, USE_SLOWLY);
            if (Hotbar.Contains(SNOPower.Monk_CripplingWave))
                return new TrinityPower(SNOPower.Monk_CripplingWave, 10f, vNullLocation, -1, -1, 0, 0, USE_SLOWLY);
            if (Hotbar.Contains(SNOPower.Monk_WayOfTheHundredFists))
                return new TrinityPower(SNOPower.Monk_WayOfTheHundredFists, 10f, vNullLocation, -1, -1, 0, 0, USE_SLOWLY);
            return new TrinityPower(SNOPower.Weapon_Melee_Instant, 10f, vNullLocation, -1, -1, 0, 0, USE_SLOWLY);
        }
        /// <summary>
        /// Returns true if we have a mantra and it's up, or if we don't have a Mantra at all
        /// </summary>
        /// <returns></returns>
        private static bool HasMonkMantraAbilityAndBuff()
        {
            return
                (Hotbar.Contains(SNOPower.Monk_MantraOfConviction) ||
                Hotbar.Contains(SNOPower.Monk_MantraOfEvasion) ||
                Hotbar.Contains(SNOPower.Monk_MantraOfHealing) ||
                Hotbar.Contains(SNOPower.Monk_MantraOfRetribution))
                &&
                (GetHasBuff(SNOPower.Monk_MantraOfConviction) ||
                GetHasBuff(SNOPower.Monk_MantraOfEvasion) ||
                GetHasBuff(SNOPower.Monk_MantraOfHealing) ||
                GetHasBuff(SNOPower.Monk_MantraOfRetribution)) ||
                DoesNotHaveMonkMantraAbility();
        }
        private static bool DoesNotHaveMonkMantraAbility()
        {
            return
                (!Hotbar.Contains(SNOPower.Monk_MantraOfConviction) &&
                !Hotbar.Contains(SNOPower.Monk_MantraOfEvasion) &&
                !Hotbar.Contains(SNOPower.Monk_MantraOfHealing) &&
                !Hotbar.Contains(SNOPower.Monk_MantraOfRetribution));
        }


    }
}
