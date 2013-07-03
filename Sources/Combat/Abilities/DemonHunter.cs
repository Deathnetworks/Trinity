using System;
using System.Linq;
using Trinity.Combat.Abilities;
using Zeta;
using Zeta.Common;
using Zeta.Common.Plugins;
using Zeta.CommonBot;
using Zeta.Internals.Actors;
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
            // Shadow Power
            if (!UseOOCBuff && Hotbar.Contains(SNOPower.DemonHunter_ShadowPower) && !Player.IsIncapacitated &&
                Player.SecondaryResource >= 14 &&
                (Player.CurrentHealthPct <= 0.99 || Player.IsRooted || ElitesWithinRange[RANGE_25] >= 1 || AnythingWithinRange[RANGE_15] >= 3) &&
                SNOPowerUseTimer(SNOPower.DemonHunter_ShadowPower))
            {
                return new TrinityPower(SNOPower.DemonHunter_ShadowPower, 0f, Vector3.Zero, CurrentWorldDynamicId, -1, 1, 1, WAIT_FOR_ANIM);
            }
            // Smoke Screen
            if ((!UseOOCBuff || Settings.Combat.DemonHunter.SpamSmokeScreen) && Hotbar.Contains(SNOPower.DemonHunter_SmokeScreen) &&
                !GetHasBuff(SNOPower.DemonHunter_ShadowPower) && Player.SecondaryResource >= 14 &&
                (
                 ( Player.CurrentHealthPct <= 0.90 || Player.IsRooted || ElitesWithinRange[RANGE_20] >= 1 || AnythingWithinRange[RANGE_15] >= 3 || Player.IsIncapacitated ) ||
                 Settings.Combat.DemonHunter.SpamSmokeScreen 
                ) &&
                SNOPowerUseTimer(SNOPower.DemonHunter_SmokeScreen))
            {
                return new TrinityPower(SNOPower.DemonHunter_SmokeScreen, 0f, Vector3.Zero, CurrentWorldDynamicId, -1, 1, 1, WAIT_FOR_ANIM);
            }
            // Preparation

            if (
                (
                (( !UseOOCBuff && !Player.IsIncapacitated && AnythingWithinRange[RANGE_40] >= 1 ) 
                 || Settings.Combat.DemonHunter.SpamPreparation ) 
                ) && 
                Hotbar.Contains(SNOPower.DemonHunter_Preparation) &&
                Player.SecondaryResource <= 10 &&

                TrinityPowerManager.CanUse(SNOPower.DemonHunter_Preparation)
                )
            {
                return new TrinityPower(SNOPower.DemonHunter_Preparation, 0f, Vector3.Zero, CurrentWorldDynamicId, -1, 1, 1, WAIT_FOR_ANIM);
            }
            // Evasive Fire
            if ( !UseOOCBuff && Hotbar.Contains(SNOPower.DemonHunter_EvasiveFire) && !Player.IsIncapacitated &&
                  (((AnythingWithinRange[RANGE_20] >= 1 || CurrentTarget.RadiusDistance <= 20f) && SNOPowerUseTimer(SNOPower.DemonHunter_EvasiveFire)) ||
                DHHasNoPrimary()))
            {
                float range = DHHasNoPrimary() ? 70f : 0f;

                return new TrinityPower(SNOPower.DemonHunter_EvasiveFire, range, Vector3.Zero, -1, CurrentTarget.ACDGuid, 1, 1, WAIT_FOR_ANIM);
            }
            // Companion
            if (!Player.IsIncapacitated && Hotbar.Contains(SNOPower.DemonHunter_Companion) && iPlayerOwnedDHPets == 0 &&
                Player.SecondaryResource >= 10 && SNOPowerUseTimer(SNOPower.DemonHunter_Companion))
            {
                return new TrinityPower(SNOPower.DemonHunter_Companion, 0f, Vector3.Zero, CurrentWorldDynamicId, -1, 2, 1, WAIT_FOR_ANIM);
            }
            // Sentry Turret
            if (!UseOOCBuff && !Player.IsIncapacitated && Hotbar.Contains(SNOPower.DemonHunter_Sentry) &&
                (TargetUtil.AnyElitesInRange(50) || TargetUtil.AnyMobsInRange(50, 2) || TargetUtil.IsEliteTargetInRange(50)) &&
                Player.PrimaryResource >= 30 && PowerManager.CanCast(SNOPower.DemonHunter_Sentry))
            {

                return new TrinityPower(SNOPower.DemonHunter_Sentry, 0f, Player.Position, CurrentWorldDynamicId, -1, 0, 0, NO_WAIT_ANIM);
            }
            // Marked for Death
            if (!UseOOCBuff && !IsCurrentlyAvoiding && Hotbar.Contains(SNOPower.DemonHunter_MarkedForDeath) &&
                Player.SecondaryResource >= 3 && 
                (ElitesWithinRange[RANGE_40] >= 1 || AnythingWithinRange[RANGE_40] >= 3 ||
                
                ((CurrentTarget.IsEliteRareUnique || CurrentTarget.IsTreasureGoblin || CurrentTarget.IsBoss) &&
                CurrentTarget.Radius <= 40 && CurrentTarget.RadiusDistance <= 40f)) &&
                SNOPowerUseTimer(SNOPower.DemonHunter_MarkedForDeath))
            {
                return new TrinityPower(SNOPower.DemonHunter_MarkedForDeath, 40f, Vector3.Zero, CurrentWorldDynamicId, CurrentTarget.ACDGuid, 1, 1, WAIT_FOR_ANIM);
            }
            // Vault
            if (!UseOOCBuff && !IsCurrentlyAvoiding && Hotbar.Contains(SNOPower.DemonHunter_Vault) && !Player.IsRooted && !Player.IsIncapacitated &&
                // Only use vault to retreat if < level 60, or if in inferno difficulty for level 60's
                (Player.Level < 60 || iCurrentGameDifficulty == GameDifficulty.Inferno) &&
                (CurrentTarget.RadiusDistance <= 10f || AnythingWithinRange[RANGE_6] >= 1) &&
                ((!Hotbar.Contains(SNOPower.DemonHunter_ShadowPower) && Player.SecondaryResource >= 16) ||
                 (Hotbar.Contains(SNOPower.DemonHunter_ShadowPower) && Player.SecondaryResource >= 22)) && 
                    TimeSinceUse(SNOPower.DemonHunter_Vault) >= Trinity.Settings.Combat.DemonHunter.VaultMovementDelay &&
                    PowerManager.CanCast(SNOPower.DemonHunter_Vault))
            {
                Vector3 vNewTarget = MathEx.CalculatePointFrom(CurrentTarget.Position, Player.Position, -15f);
                return new TrinityPower(SNOPower.DemonHunter_Vault, 20f, vNewTarget, CurrentWorldDynamicId, -1, 1, 2, WAIT_FOR_ANIM);
            }
            // Rain of Vengeance
            if (!UseOOCBuff && Hotbar.Contains(SNOPower.DemonHunter_RainOfVengeance) && !Player.IsIncapacitated &&
                (AnythingWithinRange[RANGE_25] >= 3 || ElitesWithinRange[RANGE_25] >= 1) &&
                SNOPowerUseTimer(SNOPower.DemonHunter_RainOfVengeance) && PowerManager.CanCast(SNOPower.DemonHunter_RainOfVengeance))
            {
                return new TrinityPower(SNOPower.DemonHunter_RainOfVengeance, 0f, Vector3.Zero, CurrentWorldDynamicId, -1, 1, 1, WAIT_FOR_ANIM);
            }
            // Cluster Arrow
            if (!UseOOCBuff && !IsCurrentlyAvoiding && Hotbar.Contains(SNOPower.DemonHunter_ClusterArrow) && !Player.IsIncapacitated &&
                Player.PrimaryResource >= 50 &&
               (ElitesWithinRange[RANGE_50] >= 1 || AnythingWithinRange[RANGE_50] >= 5 || ((CurrentTarget.IsEliteRareUnique || CurrentTarget.IsTreasureGoblin || CurrentTarget.IsBoss) && CurrentTarget.RadiusDistance <= 69f)) &&
                SNOPowerUseTimer(SNOPower.DemonHunter_ClusterArrow))
            {
                return new TrinityPower(SNOPower.DemonHunter_ClusterArrow, 69f, Vector3.Zero, CurrentWorldDynamicId, CurrentTarget.ACDGuid, 1, 1, WAIT_FOR_ANIM);
            }
            // Multi Shot
            if (!UseOOCBuff && !IsCurrentlyAvoiding && Hotbar.Contains(SNOPower.DemonHunter_Multishot) && !Player.IsIncapacitated &&
                Player.PrimaryResource >= 30 &&
                (ElitesWithinRange[RANGE_40] >= 1 || AnythingWithinRange[RANGE_40] >= 2 || ((CurrentTarget.IsEliteRareUnique || CurrentTarget.IsTreasureGoblin || CurrentTarget.IsBoss) && 
                CurrentTarget.RadiusDistance <= 30f)))
            {
                return new TrinityPower(SNOPower.DemonHunter_Multishot, 40f, CurrentTarget.Position, CurrentWorldDynamicId, -1, 1, 1, WAIT_FOR_ANIM);
            }
            // Fan of Knives
            if (!UseOOCBuff && Hotbar.Contains(SNOPower.DemonHunter_FanOfKnives) && !Player.IsIncapacitated &&
                Player.PrimaryResource >= 20 &&
                (AnythingWithinRange[RANGE_15] >= 4 || ElitesWithinRange[RANGE_15] >= 1) &&
                PowerManager.CanCast(SNOPower.DemonHunter_FanOfKnives))
            {
                return new TrinityPower(SNOPower.DemonHunter_FanOfKnives, 0f, Vector3.Zero, CurrentWorldDynamicId, -1, 1, 1, WAIT_FOR_ANIM);
            }
            // Strafe spam - similar to barbarian whirlwind routine
            if (!UseOOCBuff && !IsCurrentlyAvoiding && Hotbar.Contains(SNOPower.DemonHunter_Strafe) && !Player.IsIncapacitated && !Player.IsRooted &&
                // Only if there's 3 guys in 25 yds
                AnythingWithinRange[RANGE_25] >= 3 &&
                // Check for energy reservation amounts
                ((Player.PrimaryResource >= 15 && !Player.WaitingForReserveEnergy) || Player.PrimaryResource >= MinEnergyReserve))
            {
                bool bGenerateNewZigZag = (DateTime.Now.Subtract(LastChangedZigZag).TotalMilliseconds >= 1500 ||
                    (vPositionLastZigZagCheck != Vector3.Zero && Player.Position == vPositionLastZigZagCheck && DateTime.Now.Subtract(LastChangedZigZag).TotalMilliseconds >= 200) ||
                    Vector3.Distance(Player.Position, CombatBase.ZigZagPosition) <= 4f ||
                    CurrentTarget.ACDGuid != LastZigZagUnitAcdGuid);
                vPositionLastZigZagCheck = Player.Position;
                if (bGenerateNewZigZag)
                {
                    float fExtraDistance = CurrentTarget.CentreDistance <= 10f ? 10f : 5f;
                    //vSideToSideTarget = FindZigZagTargetLocation(CurrentTarget.vPosition, CurrentTarget.fCentreDist + fExtraDistance);
                    CombatBase.ZigZagPosition = NavHelper.FindSafeZone(false, 1, CurrentTarget.Position, false);
                    // Resetting this to ensure the "no-spam" is reset since we changed our target location
                    LastPowerUsed = SNOPower.None;
                    LastZigZagUnitAcdGuid = CurrentTarget.ACDGuid;
                    LastChangedZigZag = DateTime.Now;
                }
                return new TrinityPower(SNOPower.DemonHunter_Strafe, 25f, CombatBase.ZigZagPosition, CurrentWorldDynamicId, -1, 0, 0, WAIT_FOR_ANIM);
            }
            // Spike Trap
            if (!UseOOCBuff && !Player.IsIncapacitated && Hotbar.Contains(SNOPower.DemonHunter_SpikeTrap) &&
                LastPowerUsed != SNOPower.DemonHunter_SpikeTrap &&
                (ElitesWithinRange[RANGE_30] >= 1 || AnythingWithinRange[RANGE_25] > 4 || ((CurrentTarget.IsEliteRareUnique || CurrentTarget.IsTreasureGoblin || CurrentTarget.IsBoss) && CurrentTarget.RadiusDistance <= 35f)) &&
                Player.PrimaryResource >= 30 && SNOPowerUseTimer(SNOPower.DemonHunter_SpikeTrap))
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
                return new TrinityPower(SNOPower.DemonHunter_SpikeTrap, 40f, vNewTarget, CurrentWorldDynamicId, -1, 1, 1, WAIT_FOR_ANIM);
            }
            // Caltrops
            if (!UseOOCBuff && Hotbar.Contains(SNOPower.DemonHunter_Caltrops) && !Player.IsIncapacitated &&
                Player.SecondaryResource >= 6 && (AnythingWithinRange[RANGE_30] >= 2 || ElitesWithinRange[RANGE_40] >= 1) &&
                SNOPowerUseTimer(SNOPower.DemonHunter_Caltrops))
            {
                return new TrinityPower(SNOPower.DemonHunter_Caltrops, 0f, Vector3.Zero, CurrentWorldDynamicId, -1, 1, 1, WAIT_FOR_ANIM);
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
                ((Player.PrimaryResource >= 10 && !Player.WaitingForReserveEnergy) || Player.PrimaryResource >= MinEnergyReserve))
            {
                // Players with grenades *AND* elemental arrow should spam grenades at close-range instead
                if (Hotbar.Contains(SNOPower.DemonHunter_Grenades) && CurrentTarget.RadiusDistance <= 18f)
                    return new TrinityPower(SNOPower.DemonHunter_Grenades, 18f, Vector3.Zero, -1, CurrentTarget.ACDGuid, 0, 1, WAIT_FOR_ANIM);
                // Now return elemental arrow, if not sending grenades instead
                return new TrinityPower(SNOPower.DemonHunter_ElementalArrow, 65f, Vector3.Zero, -1, CurrentTarget.ACDGuid, 0, 1, WAIT_FOR_ANIM);
            }
            // Chakram
            if (!UseOOCBuff && !IsCurrentlyAvoiding && Hotbar.Contains(SNOPower.DemonHunter_Chakram) && !Player.IsIncapacitated &&
                // If we have elemental arrow or rapid fire, then use chakram as a 110 second buff, instead
                ((!Hotbar.Contains(SNOPower.DemonHunter_ClusterArrow)) ||
                TimeSinceUse(SNOPower.DemonHunter_Chakram) >= 110000) &&
                ((Player.PrimaryResource >= 10 && !Player.WaitingForReserveEnergy) || Player.PrimaryResource >= MinEnergyReserve))
            {
                return new TrinityPower(SNOPower.DemonHunter_Chakram, 50f, Vector3.Zero, -1, CurrentTarget.ACDGuid, 0, 1, WAIT_FOR_ANIM);
            }
            // Rapid Fire
            if (!UseOOCBuff && !IsCurrentlyAvoiding && Hotbar.Contains(SNOPower.DemonHunter_RapidFire) && !Player.IsIncapacitated &&
                ((Player.PrimaryResource >= 20 && !Player.WaitingForReserveEnergy) || Player.PrimaryResource >= MinEnergyReserve))
            {
                // Players with grenades *AND* rapid fire should spam grenades at close-range instead
                if (Hotbar.Contains(SNOPower.DemonHunter_Grenades) && CurrentTarget.RadiusDistance <= 18f)
                    return new TrinityPower(SNOPower.DemonHunter_Grenades, 18f, Vector3.Zero, -1, CurrentTarget.ACDGuid, 0, 0, WAIT_FOR_ANIM);
                // Now return rapid fire, if not sending grenades instead
                return new TrinityPower(SNOPower.DemonHunter_RapidFire, 50f, Vector3.Zero, -1, CurrentTarget.ACDGuid, 0, 0, NO_WAIT_ANIM);
            }
            // Impale
            if (!UseOOCBuff && !IsCurrentlyAvoiding && Hotbar.Contains(SNOPower.DemonHunter_Impale) && !Player.IsIncapacitated &&
                (AnythingWithinRange[RANGE_12] <= 3) &&
                ((Player.PrimaryResource >= 25 && !Player.WaitingForReserveEnergy) || Player.PrimaryResource >= MinEnergyReserve) &&
                CurrentTarget.RadiusDistance <= 50f)
            {
                return new TrinityPower(SNOPower.DemonHunter_Impale, 50f, Vector3.Zero, -1, CurrentTarget.ACDGuid, 0, 1, WAIT_FOR_ANIM);
            }
            // Hungering Arrow
            if (!UseOOCBuff && !IsCurrentlyAvoiding && Hotbar.Contains(SNOPower.DemonHunter_HungeringArrow) && !Player.IsIncapacitated)
            {
                return new TrinityPower(SNOPower.DemonHunter_HungeringArrow, 50f, Vector3.Zero, -1, CurrentTarget.ACDGuid, 0, 0, WAIT_FOR_ANIM);
            }
            // Entangling shot
            if (!UseOOCBuff && !IsCurrentlyAvoiding && Hotbar.Contains(SNOPower.DemonHunter_EntanglingShot) && !Player.IsIncapacitated)
            {
                return new TrinityPower(SNOPower.DemonHunter_EntanglingShot, 50f, Vector3.Zero, -1, CurrentTarget.ACDGuid, 0, 0, WAIT_FOR_ANIM);
            }
            // Bola Shot
            if (!UseOOCBuff && !IsCurrentlyAvoiding && Hotbar.Contains(SNOPower.DemonHunter_BolaShot) && !Player.IsIncapacitated)
            {
                return new TrinityPower(SNOPower.DemonHunter_BolaShot, 50f, Vector3.Zero, -1, CurrentTarget.ACDGuid, 0, 1, WAIT_FOR_ANIM);
            }
            // Grenades
            if (!UseOOCBuff && !IsCurrentlyAvoiding && Hotbar.Contains(SNOPower.DemonHunter_Grenades) && !Player.IsIncapacitated)
            {
                return new TrinityPower(SNOPower.DemonHunter_Grenades, 40f, Vector3.Zero, -1, CurrentTarget.ACDGuid, 0, 1, WAIT_FOR_ANIM);
            }

            // Default attacks
            return CombatBase.DefaultPower;
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
                return new TrinityPower(SNOPower.DemonHunter_HungeringArrow, 40f, Vector3.Zero, -1, -1, 0, 0, WAIT_FOR_ANIM);
            if (Hotbar.Contains(SNOPower.DemonHunter_EntanglingShot))
                return new TrinityPower(SNOPower.DemonHunter_EntanglingShot, 40f, Vector3.Zero, -1, -1, 0, 0, WAIT_FOR_ANIM);
            if (Hotbar.Contains(SNOPower.DemonHunter_BolaShot))
                return new TrinityPower(SNOPower.DemonHunter_BolaShot, 40f, Vector3.Zero, -1, -1, 0, 0, WAIT_FOR_ANIM);
            if (Hotbar.Contains(SNOPower.DemonHunter_Grenades))
                return new TrinityPower(SNOPower.DemonHunter_Grenades, 15f, Vector3.Zero, -1, -1, 0, 0, WAIT_FOR_ANIM);
            if (Hotbar.Contains(SNOPower.DemonHunter_ElementalArrow) && Player.PrimaryResource >= 10)
                return new TrinityPower(SNOPower.DemonHunter_ElementalArrow, 40f, Vector3.Zero, -1, -1, 0, 0, WAIT_FOR_ANIM);
            if (Hotbar.Contains(SNOPower.DemonHunter_RapidFire) && Player.PrimaryResource >= 10)
                return new TrinityPower(SNOPower.DemonHunter_RapidFire, 40f, Vector3.Zero, -1, -1, 0, 0, WAIT_FOR_ANIM);
            if (Hotbar.Contains(SNOPower.DemonHunter_Chakram) && Player.PrimaryResource >= 20)
                return new TrinityPower(SNOPower.DemonHunter_Chakram, 15f, Vector3.Zero, -1, -1, 0, 0, WAIT_FOR_ANIM);
            if (Hotbar.Contains(SNOPower.DemonHunter_EvasiveFire) && Player.PrimaryResource >= 20)
                return new TrinityPower(SNOPower.DemonHunter_EvasiveFire, 40f, Vector3.Zero, -1, -1, 0, 0, WAIT_FOR_ANIM);
            return CombatBase.DefaultPower;
        }
    }
}
