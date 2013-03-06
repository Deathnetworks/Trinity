using System;
using System.Linq;
using Zeta;
using Zeta.Common;
using Zeta.Common.Plugins;
using Zeta.CommonBot;
using Zeta.Internals.Actors;
namespace GilesTrinity
{
    public partial class GilesTrinity : IPlugin
    {
        private static TrinityPower GetDemonHunterPower(bool IsCurrentlyAvoiding, bool UseOOCBuff, bool UseDestructiblePower)
        {
            
            // Pick the best destructible power available
            if (UseDestructiblePower)
            {
                return GetDemonHunterDestroyPower();
            }
            MinEnergyReserve = 25;
            // Shadow Power
            if (!UseOOCBuff && Hotbar.Contains(SNOPower.DemonHunter_ShadowPower) && !PlayerStatus.IsIncapacitated &&
                PlayerStatus.SecondaryResource >= 14 &&
                (PlayerStatus.CurrentHealthPct <= 0.99 || PlayerStatus.IsRooted || ElitesWithinRange[RANGE_25] >= 1 || AnythingWithinRange[RANGE_15] >= 3) &&
                GilesUseTimer(SNOPower.DemonHunter_ShadowPower))
            {
                return new TrinityPower(SNOPower.DemonHunter_ShadowPower, 0f, vNullLocation, CurrentWorldDynamicId, -1, 1, 1, USE_SLOWLY);
            }
            // Smoke Screen
            if ((!UseOOCBuff || Settings.Combat.DemonHunter.SpamSmokeScreen) && Hotbar.Contains(SNOPower.DemonHunter_SmokeScreen) &&
                !GetHasBuff(SNOPower.DemonHunter_ShadowPower) && PlayerStatus.SecondaryResource >= 14 &&
                (
                 ( PlayerStatus.CurrentHealthPct <= 0.90 || PlayerStatus.IsRooted || ElitesWithinRange[RANGE_20] >= 1 || AnythingWithinRange[RANGE_15] >= 3 || PlayerStatus.IsIncapacitated ) ||
                 Settings.Combat.DemonHunter.SpamSmokeScreen 
                ) &&
                GilesUseTimer(SNOPower.DemonHunter_SmokeScreen))
            {
                return new TrinityPower(SNOPower.DemonHunter_SmokeScreen, 0f, vNullLocation, CurrentWorldDynamicId, -1, 1, 1, USE_SLOWLY);
            }
            // Preparation

            if (
                (
                (( !UseOOCBuff && !PlayerStatus.IsIncapacitated && AnythingWithinRange[RANGE_40] >= 1 ) 
                 || Settings.Combat.DemonHunter.SpamPreparation ) 
                ) && 
                Hotbar.Contains(SNOPower.DemonHunter_Preparation) &&
                PlayerStatus.SecondaryResource <= 10 &&
                //GilesUseTimer(SNOPower.DemonHunter_Preparation) && 
                //PowerManager.CanCast(SNOPower.DemonHunter_Preparation) 
                TrinityPowerManager.CanUse(SNOPower.DemonHunter_Preparation)
                )
            {
                return new TrinityPower(SNOPower.DemonHunter_Preparation, 0f, vNullLocation, CurrentWorldDynamicId, -1, 1, 1, USE_SLOWLY);
            }
            // Evasive Fire
            if ( !UseOOCBuff && Hotbar.Contains(SNOPower.DemonHunter_EvasiveFire) && !PlayerStatus.IsIncapacitated &&
                  (((AnythingWithinRange[RANGE_20] >= 1 || CurrentTarget.RadiusDistance <= 20f) && GilesUseTimer(SNOPower.DemonHunter_EvasiveFire)) ||
                DHHasNoPrimary()))
            {
                float range = DHHasNoPrimary() ? 70f : 0f;

                return new TrinityPower(SNOPower.DemonHunter_EvasiveFire, range, vNullLocation, -1, CurrentTarget.ACDGuid, 1, 1, USE_SLOWLY);
            }
            // Companion
            if (!PlayerStatus.IsIncapacitated && Hotbar.Contains(SNOPower.DemonHunter_Companion) && iPlayerOwnedDHPets == 0 &&
                PlayerStatus.SecondaryResource >= 10 && GilesUseTimer(SNOPower.DemonHunter_Companion))
            {
                return new TrinityPower(SNOPower.DemonHunter_Companion, 0f, vNullLocation, CurrentWorldDynamicId, -1, 2, 1, USE_SLOWLY);
            }
            // Sentry Turret
            // Currently BROKEN in demonbuddy :(
            //if (!UseOOCBuff && !PlayerStatus.IsIncapacitated && Hotbar.Contains(SNOPower.DemonHunter_Sentry) &&
            //    (TargetUtil.AnyElitesInRange(50) || TargetUtil.AnyMobsInRange(50,2) || TargetUtil.IsEliteTargetInRange(50)) &&
            //    PlayerStatus.PrimaryResource >= 30 && PowerManager.CanCast(SNOPower.DemonHunter_Sentry))
            //{

            //    return new TrinityPower(SNOPower.DemonHunter_Sentry, 50f, PlayerStatus.CurrentPosition, CurrentWorldDynamicId, CurrentTarget.ACDGuid, 1, 2, USE_SLOWLY);
            //}
            // Marked for Death
            if (!UseOOCBuff && !IsCurrentlyAvoiding && Hotbar.Contains(SNOPower.DemonHunter_MarkedForDeath) &&
                PlayerStatus.SecondaryResource >= 3 && 
                (ElitesWithinRange[RANGE_40] >= 1 || AnythingWithinRange[RANGE_40] >= 3 ||
                
                ((CurrentTarget.IsEliteRareUnique || CurrentTarget.IsTreasureGoblin || CurrentTarget.IsBoss) &&
                CurrentTarget.Radius <= 40 && CurrentTarget.RadiusDistance <= 40f)) &&
                GilesUseTimer(SNOPower.DemonHunter_MarkedForDeath))
            {
                return new TrinityPower(SNOPower.DemonHunter_MarkedForDeath, 40f, vNullLocation, CurrentWorldDynamicId, CurrentTarget.ACDGuid, 1, 1, USE_SLOWLY);
            }
            // Vault
            if (!UseOOCBuff && !IsCurrentlyAvoiding && Hotbar.Contains(SNOPower.DemonHunter_Vault) && !PlayerStatus.IsRooted && !PlayerStatus.IsIncapacitated &&
                // Only use vault to retreat if < level 60, or if in inferno difficulty for level 60's
                (PlayerStatus.Level < 60 || iCurrentGameDifficulty == GameDifficulty.Inferno) &&
                (CurrentTarget.RadiusDistance <= 10f || AnythingWithinRange[RANGE_6] >= 1) &&
                ((!Hotbar.Contains(SNOPower.DemonHunter_ShadowPower) && PlayerStatus.SecondaryResource >= 16) ||
                 (Hotbar.Contains(SNOPower.DemonHunter_ShadowPower) && PlayerStatus.SecondaryResource >= 22)) && 
                    //GilesUseTimer(SNOPower.DemonHunter_Vault) && 
                    DateTime.Now.Subtract(GilesTrinity.dictAbilityLastUse[SNOPower.DemonHunter_Vault]).TotalMilliseconds >= GilesTrinity.Settings.Combat.DemonHunter.VaultMovementDelay &&
                    PowerManager.CanCast(SNOPower.DemonHunter_Vault))
            {
                Vector3 vNewTarget = MathEx.CalculatePointFrom(CurrentTarget.Position, PlayerStatus.CurrentPosition, -15f);
                return new TrinityPower(SNOPower.DemonHunter_Vault, 20f, vNewTarget, CurrentWorldDynamicId, -1, 1, 2, USE_SLOWLY);
            }
            // Rain of Vengeance
            if (!UseOOCBuff && Hotbar.Contains(SNOPower.DemonHunter_RainOfVengeance) && !PlayerStatus.IsIncapacitated &&
                (AnythingWithinRange[RANGE_25] >= 3 || ElitesWithinRange[RANGE_25] >= 1) &&
                GilesUseTimer(SNOPower.DemonHunter_RainOfVengeance) && PowerManager.CanCast(SNOPower.DemonHunter_RainOfVengeance))
            {
                return new TrinityPower(SNOPower.DemonHunter_RainOfVengeance, 0f, vNullLocation, CurrentWorldDynamicId, -1, 1, 1, USE_SLOWLY);
            }
            // Cluster Arrow
            if (!UseOOCBuff && !IsCurrentlyAvoiding && Hotbar.Contains(SNOPower.DemonHunter_ClusterArrow) && !PlayerStatus.IsIncapacitated &&
                PlayerStatus.PrimaryResource >= 50 &&
               (ElitesWithinRange[RANGE_50] >= 1 || AnythingWithinRange[RANGE_50] >= 5 || ((CurrentTarget.IsEliteRareUnique || CurrentTarget.IsTreasureGoblin || CurrentTarget.IsBoss) && CurrentTarget.RadiusDistance <= 69f)) &&
                GilesUseTimer(SNOPower.DemonHunter_ClusterArrow))
            {
                return new TrinityPower(SNOPower.DemonHunter_ClusterArrow, 69f, vNullLocation, CurrentWorldDynamicId, CurrentTarget.ACDGuid, 1, 1, USE_SLOWLY);
            }
            // Multi Shot
            if (!UseOOCBuff && !IsCurrentlyAvoiding && Hotbar.Contains(SNOPower.DemonHunter_Multishot) && !PlayerStatus.IsIncapacitated &&
                PlayerStatus.PrimaryResource >= 30 &&
                (ElitesWithinRange[RANGE_40] >= 1 || AnythingWithinRange[RANGE_40] >= 2 || ((CurrentTarget.IsEliteRareUnique || CurrentTarget.IsTreasureGoblin || CurrentTarget.IsBoss) && 
                CurrentTarget.RadiusDistance <= 30f)))
            {
                return new TrinityPower(SNOPower.DemonHunter_Multishot, 40f, CurrentTarget.Position, CurrentWorldDynamicId, -1, 1, 1, USE_SLOWLY);
            }
            // Fan of Knives
            if (!UseOOCBuff && Hotbar.Contains(SNOPower.DemonHunter_FanOfKnives) && !PlayerStatus.IsIncapacitated &&
                PlayerStatus.PrimaryResource >= 20 &&
                (AnythingWithinRange[RANGE_15] >= 4 || ElitesWithinRange[RANGE_15] >= 1) &&
                PowerManager.CanCast(SNOPower.DemonHunter_FanOfKnives))
            {
                return new TrinityPower(SNOPower.DemonHunter_FanOfKnives, 0f, vNullLocation, CurrentWorldDynamicId, -1, 1, 1, USE_SLOWLY);
            }
            // Strafe spam - similar to barbarian whirlwind routine
            if (!UseOOCBuff && !IsCurrentlyAvoiding && Hotbar.Contains(SNOPower.DemonHunter_Strafe) && !PlayerStatus.IsIncapacitated && !PlayerStatus.IsRooted &&
                // Only if there's 3 guys in 25 yds
                AnythingWithinRange[RANGE_25] >= 3 &&
                // Check for energy reservation amounts
                ((PlayerStatus.PrimaryResource >= 15 && !PlayerStatus.WaitingForReserveEnergy) || PlayerStatus.PrimaryResource >= MinEnergyReserve))
            {
                bool bGenerateNewZigZag = (DateTime.Now.Subtract(lastChangedZigZag).TotalMilliseconds >= 1500 ||
                    (vPositionLastZigZagCheck != vNullLocation && PlayerStatus.CurrentPosition == vPositionLastZigZagCheck && DateTime.Now.Subtract(lastChangedZigZag).TotalMilliseconds >= 200) ||
                    Vector3.Distance(PlayerStatus.CurrentPosition, vSideToSideTarget) <= 4f ||
                    CurrentTarget.ACDGuid != iACDGUIDLastWhirlwind);
                vPositionLastZigZagCheck = PlayerStatus.CurrentPosition;
                if (bGenerateNewZigZag)
                {
                    float fExtraDistance = CurrentTarget.CentreDistance <= 10f ? 10f : 5f;
                    //vSideToSideTarget = FindZigZagTargetLocation(CurrentTarget.vPosition, CurrentTarget.fCentreDist + fExtraDistance);
                    vSideToSideTarget = FindSafeZone(false, 1, CurrentTarget.Position, false);
                    // Resetting this to ensure the "no-spam" is reset since we changed our target location
                    LastPowerUsed = SNOPower.None;
                    iACDGUIDLastWhirlwind = CurrentTarget.ACDGuid;
                    lastChangedZigZag = DateTime.Now;
                }
                return new TrinityPower(SNOPower.DemonHunter_Strafe, 25f, vSideToSideTarget, CurrentWorldDynamicId, -1, 0, 0, USE_SLOWLY);
            }
            // Spike Trap
            if (!UseOOCBuff && !PlayerStatus.IsIncapacitated && Hotbar.Contains(SNOPower.DemonHunter_SpikeTrap) &&
                LastPowerUsed != SNOPower.DemonHunter_SpikeTrap &&
                (ElitesWithinRange[RANGE_30] >= 1 || AnythingWithinRange[RANGE_25] > 4 || ((CurrentTarget.IsEliteRareUnique || CurrentTarget.IsTreasureGoblin || CurrentTarget.IsBoss) && CurrentTarget.RadiusDistance <= 35f)) &&
                PlayerStatus.PrimaryResource >= 30 && GilesUseTimer(SNOPower.DemonHunter_SpikeTrap))
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
                return new TrinityPower(SNOPower.DemonHunter_SpikeTrap, 40f, vNewTarget, CurrentWorldDynamicId, -1, 1, 1, USE_SLOWLY);
            }
            // Caltrops
            if (!UseOOCBuff && Hotbar.Contains(SNOPower.DemonHunter_Caltrops) && !PlayerStatus.IsIncapacitated &&
                PlayerStatus.SecondaryResource >= 6 && (AnythingWithinRange[RANGE_30] >= 2 || ElitesWithinRange[RANGE_40] >= 1) &&
                GilesUseTimer(SNOPower.DemonHunter_Caltrops))
            {
                return new TrinityPower(SNOPower.DemonHunter_Caltrops, 0f, vNullLocation, CurrentWorldDynamicId, -1, 1, 1, USE_SLOWLY);
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
                GilesUseTimer(SNOPower.DemonHunter_ElementalArrow) && !PlayerStatus.IsIncapacitated &&
                ((PlayerStatus.PrimaryResource >= 10 && !PlayerStatus.WaitingForReserveEnergy) || PlayerStatus.PrimaryResource >= MinEnergyReserve))
            {
                // Players with grenades *AND* elemental arrow should spam grenades at close-range instead
                if (Hotbar.Contains(SNOPower.DemonHunter_Grenades) && CurrentTarget.RadiusDistance <= 18f)
                    return new TrinityPower(SNOPower.DemonHunter_Grenades, 18f, vNullLocation, -1, CurrentTarget.ACDGuid, 0, 1, USE_SLOWLY);
                // Now return elemental arrow, if not sending grenades instead
                return new TrinityPower(SNOPower.DemonHunter_ElementalArrow, 65f, vNullLocation, -1, CurrentTarget.ACDGuid, 0, 1, USE_SLOWLY);
            }
            // Chakram
            if (!UseOOCBuff && !IsCurrentlyAvoiding && Hotbar.Contains(SNOPower.DemonHunter_Chakram) && !PlayerStatus.IsIncapacitated &&
                // If we have elemental arrow or rapid fire, then use chakram as a 110 second buff, instead
                ((!Hotbar.Contains(SNOPower.DemonHunter_ClusterArrow)) ||
                DateTime.Now.Subtract(dictAbilityLastUse[SNOPower.DemonHunter_Chakram]).TotalMilliseconds >= 110000) &&
                ((PlayerStatus.PrimaryResource >= 10 && !PlayerStatus.WaitingForReserveEnergy) || PlayerStatus.PrimaryResource >= MinEnergyReserve))
            {
                return new TrinityPower(SNOPower.DemonHunter_Chakram, 50f, vNullLocation, -1, CurrentTarget.ACDGuid, 0, 1, USE_SLOWLY);
            }
            // Rapid Fire
            if (!UseOOCBuff && !IsCurrentlyAvoiding && Hotbar.Contains(SNOPower.DemonHunter_RapidFire) && !PlayerStatus.IsIncapacitated &&
                ((PlayerStatus.PrimaryResource >= 20 && !PlayerStatus.WaitingForReserveEnergy) || PlayerStatus.PrimaryResource >= MinEnergyReserve))
            {
                // Players with grenades *AND* rapid fire should spam grenades at close-range instead
                if (Hotbar.Contains(SNOPower.DemonHunter_Grenades) && CurrentTarget.RadiusDistance <= 18f)
                    return new TrinityPower(SNOPower.DemonHunter_Grenades, 18f, vNullLocation, -1, CurrentTarget.ACDGuid, 0, 0, USE_SLOWLY);
                // Now return rapid fire, if not sending grenades instead
                return new TrinityPower(SNOPower.DemonHunter_RapidFire, 50f, vNullLocation, -1, CurrentTarget.ACDGuid, 0, 0, SIGNATURE_SPAM);
            }
            // Impale
            if (!UseOOCBuff && !IsCurrentlyAvoiding && Hotbar.Contains(SNOPower.DemonHunter_Impale) && !PlayerStatus.IsIncapacitated &&
                (AnythingWithinRange[RANGE_12] <= 3) &&
                ((PlayerStatus.PrimaryResource >= 25 && !PlayerStatus.WaitingForReserveEnergy) || PlayerStatus.PrimaryResource >= MinEnergyReserve) &&
                CurrentTarget.RadiusDistance <= 50f)
            {
                return new TrinityPower(SNOPower.DemonHunter_Impale, 50f, vNullLocation, -1, CurrentTarget.ACDGuid, 0, 1, USE_SLOWLY);
            }
            // Hungering Arrow
            if (!UseOOCBuff && !IsCurrentlyAvoiding && Hotbar.Contains(SNOPower.DemonHunter_HungeringArrow) && !PlayerStatus.IsIncapacitated)
            {
                return new TrinityPower(SNOPower.DemonHunter_HungeringArrow, 50f, vNullLocation, -1, CurrentTarget.ACDGuid, 0, 0, USE_SLOWLY);
            }
            // Entangling shot
            if (!UseOOCBuff && !IsCurrentlyAvoiding && Hotbar.Contains(SNOPower.DemonHunter_EntanglingShot) && !PlayerStatus.IsIncapacitated)
            {
                return new TrinityPower(SNOPower.DemonHunter_EntanglingShot, 50f, vNullLocation, -1, CurrentTarget.ACDGuid, 0, 0, USE_SLOWLY);
            }
            // Bola Shot
            if (!UseOOCBuff && !IsCurrentlyAvoiding && Hotbar.Contains(SNOPower.DemonHunter_BolaShot) && !PlayerStatus.IsIncapacitated)
            {
                return new TrinityPower(SNOPower.DemonHunter_BolaShot, 50f, vNullLocation, -1, CurrentTarget.ACDGuid, 0, 1, USE_SLOWLY);
            }
            // Grenades
            if (!UseOOCBuff && !IsCurrentlyAvoiding && Hotbar.Contains(SNOPower.DemonHunter_Grenades) && !PlayerStatus.IsIncapacitated)
            {
                return new TrinityPower(SNOPower.DemonHunter_Grenades, 40f, vNullLocation, -1, CurrentTarget.ACDGuid, 0, 1, USE_SLOWLY);
            }
            // Default attacks
            if (!UseOOCBuff && !IsCurrentlyAvoiding)
            {
                return new TrinityPower(GetDefaultWeaponPower(), GetDefaultWeaponDistance(), vNullLocation, CurrentWorldDynamicId, CurrentTarget.ACDGuid, 0, 0, USE_SLOWLY);
            }
            return new TrinityPower(SNOPower.None, -1, vNullLocation, -1, -1, 0, 0, USE_SLOWLY);
        }

        private static bool DHHasNoPrimary()
        {
            return !Hotbar.Contains(SNOPower.DemonHunter_BolaShot) ||
                                !Hotbar.Contains(SNOPower.DemonHunter_EntanglingShot) ||
                                !Hotbar.Contains(SNOPower.DemonHunter_Grenades) ||
                                !Hotbar.Contains(SNOPower.DemonHunter_HungeringArrow);
        }

        private static TrinityPower GetDemonHunterDestroyPower()
        {
            if (Hotbar.Contains(SNOPower.DemonHunter_HungeringArrow))
                return new TrinityPower(SNOPower.DemonHunter_HungeringArrow, 40f, vNullLocation, -1, -1, 0, 0, USE_SLOWLY);
            if (Hotbar.Contains(SNOPower.DemonHunter_EntanglingShot))
                return new TrinityPower(SNOPower.DemonHunter_EntanglingShot, 40f, vNullLocation, -1, -1, 0, 0, USE_SLOWLY);
            if (Hotbar.Contains(SNOPower.DemonHunter_BolaShot))
                return new TrinityPower(SNOPower.DemonHunter_BolaShot, 40f, vNullLocation, -1, -1, 0, 0, USE_SLOWLY);
            if (Hotbar.Contains(SNOPower.DemonHunter_Grenades))
                return new TrinityPower(SNOPower.DemonHunter_Grenades, 15f, vNullLocation, -1, -1, 0, 0, USE_SLOWLY);
            if (Hotbar.Contains(SNOPower.DemonHunter_ElementalArrow) && PlayerStatus.PrimaryResource >= 10)
                return new TrinityPower(SNOPower.DemonHunter_ElementalArrow, 40f, vNullLocation, -1, -1, 0, 0, USE_SLOWLY);
            if (Hotbar.Contains(SNOPower.DemonHunter_RapidFire) && PlayerStatus.PrimaryResource >= 10)
                return new TrinityPower(SNOPower.DemonHunter_RapidFire, 40f, vNullLocation, -1, -1, 0, 0, USE_SLOWLY);
            if (Hotbar.Contains(SNOPower.DemonHunter_Chakram) && PlayerStatus.PrimaryResource >= 20)
                return new TrinityPower(SNOPower.DemonHunter_Chakram, 15f, vNullLocation, -1, -1, 0, 0, USE_SLOWLY);
            if (Hotbar.Contains(SNOPower.DemonHunter_EvasiveFire) && PlayerStatus.PrimaryResource >= 20)
                return new TrinityPower(SNOPower.DemonHunter_EvasiveFire, 40f, vNullLocation, -1, -1, 0, 0, USE_SLOWLY);
            return new TrinityPower(SNOPower.Weapon_Ranged_Instant, 40f, vNullLocation, -1, CurrentTarget.ACDGuid, 0, 0, USE_SLOWLY);
        }
    }
}
