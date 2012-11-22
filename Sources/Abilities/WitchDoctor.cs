using Zeta.Common;
using Zeta.Common.Plugins;
using Zeta.CommonBot;
using Zeta.Internals.Actors;
namespace GilesTrinity
{
    public partial class GilesTrinity : IPlugin
    {
        private static GilesPower GetWitchDoctorPower(bool bCurrentlyAvoiding, bool bOOCBuff, bool bDestructiblePower, float iThisHeight)
        {
            // Pick the best destructible power available
            if (bDestructiblePower)
            {
                return GetWitchDoctorDestroyPower();
            }
            // Witch doctors have no reserve requirements?
            iWaitingReservedAmount = 0;
            // Spirit Walk Cast on 65% health or while avoiding anything but molten core or incapacitated or Chasing Goblins
            if (hashPowerHotbarAbilities.Contains(SNOPower.Witchdoctor_SpiritWalk) && playerStatus.dCurrentEnergy >= 49 &&
                (
                 playerStatus.dCurrentHealthPct <= 0.65 || playerStatus.bIsIncapacitated || playerStatus.bIsRooted || (settings.bOutOfCombatMovementPowers && bOOCBuff) ||
                 (!bOOCBuff && CurrentTarget.bIsTreasureGoblin && CurrentTarget.iHitPoints < 0.90 && CurrentTarget.fRadiusDistance <= 40f)
                ) &&
                PowerManager.CanCast(SNOPower.Witchdoctor_SpiritWalk))
            {
                return new GilesPower(SNOPower.Witchdoctor_SpiritWalk, 0f, vNullLocation, iCurrentWorldID, -1, 0, 0, USE_SLOWLY);
            }
            // Soul Harvest Any Elites or 2+ Norms and baby it's harvest season
            if (!bOOCBuff && hashPowerHotbarAbilities.Contains(SNOPower.Witchdoctor_SoulHarvest) && !playerStatus.bIsIncapacitated && playerStatus.dCurrentEnergy >= 59 && GilesBuffStacks(SNOPower.Witchdoctor_SoulHarvest) < 4 &&
               (iElitesWithinRange[RANGE_6] >= 1 || iAnythingWithinRange[RANGE_6] >= 2 || ((CurrentTarget.IsEliteRareUnique || CurrentTarget.bIsTreasureGoblin) && CurrentTarget.fRadiusDistance <= 7f)) &&
                PowerManager.CanCast(SNOPower.Witchdoctor_SoulHarvest))
            {
                return new GilesPower(SNOPower.Witchdoctor_SoulHarvest, 0f, vNullLocation, iCurrentWorldID, -1, 0, 0, USE_SLOWLY);
            }
            // Sacrifice AKA Zombie Dog Jihad, use on Elites Only or to try and Save yourself
            if (!bOOCBuff && hashPowerHotbarAbilities.Contains(SNOPower.Witchdoctor_Sacrifice) &&
                (iElitesWithinRange[RANGE_15] > 0 || ((CurrentTarget.IsEliteRareUnique || CurrentTarget.IsBoss || CurrentTarget.bIsTreasureGoblin) && CurrentTarget.fRadiusDistance <= 15f)) &&
                PowerManager.CanCast(SNOPower.Witchdoctor_Sacrifice))
            {
                return new GilesPower(SNOPower.Witchdoctor_Sacrifice, 0f, vNullLocation, iCurrentWorldID, -1, 1, 0, USE_SLOWLY);
            }
            // Gargantuan, Recast on 1+ Elites or Bosses to trigger Restless Giant
            if (!bOOCBuff && !bCurrentlyAvoiding && hashPowerHotbarAbilities.Contains(SNOPower.Witchdoctor_Gargantuan) && !playerStatus.bIsIncapacitated && playerStatus.dCurrentEnergy >= 147 &&
                (iElitesWithinRange[RANGE_15] >= 1 ||
                 (CurrentTarget != null && ((CurrentTarget.IsEliteRareUnique || CurrentTarget.IsBoss) && CurrentTarget.fRadiusDistance <= 15f)) || iPlayerOwnedGargantuan == 0) &&
                PowerManager.CanCast(SNOPower.Witchdoctor_Gargantuan))
            {
                return new GilesPower(SNOPower.Witchdoctor_Gargantuan, 0f, vNullLocation, iCurrentWorldID, -1, 2, 1, USE_SLOWLY);
            }
            // Zombie dogs Woof Woof, good for being blown up, cast when less than or equal to 2 Dogs or Not Blowing them up and cast when less than 4
            if (!bCurrentlyAvoiding && hashPowerHotbarAbilities.Contains(SNOPower.Witchdoctor_SummonZombieDog) && !playerStatus.bIsIncapacitated &&
                playerStatus.dCurrentEnergy >= 49 && (iElitesWithinRange[RANGE_20] >= 2 || iAnythingWithinRange[RANGE_20] >= 5 ||
                 (CurrentTarget != null && ((CurrentTarget.IsEliteRareUnique || CurrentTarget.bIsTreasureGoblin) && CurrentTarget.fRadiusDistance <= 30f)) || iPlayerOwnedZombieDog <= 2) &&
                PowerManager.CanCast(SNOPower.Witchdoctor_SummonZombieDog))
            {
                return new GilesPower(SNOPower.Witchdoctor_SummonZombieDog, 0f, vNullLocation, iCurrentWorldID, -1, 0, 0, USE_SLOWLY);
            }
            // Hex Spam Cast on ANYTHING in range, mmm pork and chicken
            if (!bOOCBuff && hashPowerHotbarAbilities.Contains(SNOPower.Witchdoctor_Hex) && !playerStatus.bIsIncapacitated && playerStatus.dCurrentEnergy >= 49 &&
               (iElitesWithinRange[RANGE_12] >= 1 || iAnythingWithinRange[RANGE_12] >= 1 || ((CurrentTarget.IsEliteRareUnique || CurrentTarget.bIsTreasureGoblin || CurrentTarget.IsBoss) && CurrentTarget.fRadiusDistance <= 18f)) &&
               PowerManager.CanCast(SNOPower.Witchdoctor_Hex))
            {
                return new GilesPower(SNOPower.Witchdoctor_Hex, 0f, vNullLocation, iCurrentWorldID, -1, 0, 0, USE_SLOWLY);
            }
            // Mass Confuse, elites only or big mobs or to escape on low health
            if (!bOOCBuff && hashPowerHotbarAbilities.Contains(SNOPower.Witchdoctor_MassConfusion) && !playerStatus.bIsIncapacitated && playerStatus.dCurrentEnergy >= 74 &&
                (iElitesWithinRange[RANGE_12] >= 1 || iAnythingWithinRange[RANGE_12] >= 6 || playerStatus.dCurrentHealthPct <= 0.25 || ((CurrentTarget.IsEliteRareUnique || CurrentTarget.IsBoss) && CurrentTarget.fRadiusDistance <= 12f)) &&
                !CurrentTarget.bIsTreasureGoblin && PowerManager.CanCast(SNOPower.Witchdoctor_MassConfusion))
            {
                return new GilesPower(SNOPower.Witchdoctor_MassConfusion, 0f, vNullLocation, -1, CurrentTarget.iACDGuid, 1, 1, USE_SLOWLY);
            }
            // Big Bad Voodoo, elites and bosses only
            if (!bOOCBuff && hashPowerHotbarAbilities.Contains(SNOPower.Witchdoctor_BigBadVoodoo) && !playerStatus.bIsIncapacitated &&
                !CurrentTarget.bIsTreasureGoblin &&
                (iElitesWithinRange[RANGE_6] > 0 || ((CurrentTarget.IsEliteRareUnique || CurrentTarget.IsBoss) && CurrentTarget.fRadiusDistance <= 12f)) &&
                PowerManager.CanCast(SNOPower.Witchdoctor_BigBadVoodoo))
            {
                return new GilesPower(SNOPower.Witchdoctor_BigBadVoodoo, 0f, vNullLocation, iCurrentWorldID, -1, 0, 0, USE_SLOWLY);
            }
            // Grasp of the Dead, look below, droping globes and dogs when using it on elites and 3 norms
            if (!bOOCBuff && !bCurrentlyAvoiding && hashPowerHotbarAbilities.Contains(SNOPower.Witchdoctor_GraspOfTheDead) && !playerStatus.bIsIncapacitated &&
                (iElitesWithinRange[RANGE_25] > 0 || iAnythingWithinRange[RANGE_25] > 2) &&
                playerStatus.dCurrentEnergy >= 122 && PowerManager.CanCast(SNOPower.Witchdoctor_GraspOfTheDead))
            {
                return new GilesPower(SNOPower.Witchdoctor_GraspOfTheDead, 25f, CurrentTarget.vPosition, iCurrentWorldID, -1, 0, 0, USE_SLOWLY);
            }
            // Horrify Buff When not in combat for movement speed
            if (bOOCBuff && settings.bEnableCriticalMass == true && hashPowerHotbarAbilities.Contains(SNOPower.Witchdoctor_Horrify) && !playerStatus.bIsIncapacitated && playerStatus.dCurrentEnergy >= 37 &&
                PowerManager.CanCast(SNOPower.Witchdoctor_Horrify))
            {
                return new GilesPower(SNOPower.Witchdoctor_Horrify, 0f, vNullLocation, iCurrentWorldID, -1, 0, 0, USE_SLOWLY);
            }
            // Horrify Buff at 35% health
            if (!bOOCBuff && hashPowerHotbarAbilities.Contains(SNOPower.Witchdoctor_Horrify) && !playerStatus.bIsIncapacitated && playerStatus.dCurrentEnergy >= 37 &&
                playerStatus.dCurrentHealthPct <= 0.35 &&
                PowerManager.CanCast(SNOPower.Witchdoctor_Horrify))
            {
                return new GilesPower(SNOPower.Witchdoctor_Horrify, 0f, vNullLocation, iCurrentWorldID, -1, 0, 0, USE_SLOWLY);
            }
            // Fetish Army, elites only
            if (!bOOCBuff && hashPowerHotbarAbilities.Contains(SNOPower.Witchdoctor_FetishArmy) && !playerStatus.bIsIncapacitated &&
                (iElitesWithinRange[RANGE_25] > 0 || ((CurrentTarget.IsEliteRareUnique || CurrentTarget.bIsTreasureGoblin || CurrentTarget.IsBoss) && CurrentTarget.fRadiusDistance <= 16f)) &&
                PowerManager.CanCast(SNOPower.Witchdoctor_FetishArmy))
            {
                return new GilesPower(SNOPower.Witchdoctor_FetishArmy, 0f, vNullLocation, iCurrentWorldID, -1, 1, 1, USE_SLOWLY);
            }
            // Spirit Barrage
            if (!bOOCBuff && !bCurrentlyAvoiding && hashPowerHotbarAbilities.Contains(SNOPower.Witchdoctor_SpiritBarrage) && !playerStatus.bIsIncapacitated && playerStatus.dCurrentEnergy >= 108 &&
                PowerManager.CanCast(SNOPower.Witchdoctor_SpiritBarrage))
            {
                return new GilesPower(SNOPower.Witchdoctor_SpiritBarrage, 21f, vNullLocation, -1, CurrentTarget.iACDGuid, 1, 1, USE_SLOWLY);
            }
            // Haunt the shit out of monster and maybe they will give you treats
            if (!bOOCBuff && !bCurrentlyAvoiding && hashPowerHotbarAbilities.Contains(SNOPower.Witchdoctor_Haunt) && !playerStatus.bIsIncapacitated && playerStatus.dCurrentEnergy >= 98 &&
                PowerManager.CanCast(SNOPower.Witchdoctor_Haunt))
            {
                return new GilesPower(SNOPower.Witchdoctor_Haunt, 21f, vNullLocation, -1, CurrentTarget.iACDGuid, 1, 1, USE_SLOWLY);
            }
            // Locust
            if (!bOOCBuff && !bCurrentlyAvoiding && hashPowerHotbarAbilities.Contains(SNOPower.Witchdoctor_Locust_Swarm) && !playerStatus.bIsIncapacitated && playerStatus.dCurrentEnergy >= 196 &&
                PowerManager.CanCast(SNOPower.Witchdoctor_Locust_Swarm))
            {
                return new GilesPower(SNOPower.Witchdoctor_Locust_Swarm, 12f, vNullLocation, -1, CurrentTarget.iACDGuid, 1, 1, USE_SLOWLY);
            }
            // Wall of Zombies
            if (!bOOCBuff && !bCurrentlyAvoiding && hashPowerHotbarAbilities.Contains(SNOPower.Witchdoctor_WallOfZombies) && !playerStatus.bIsIncapacitated &&
                (iElitesWithinRange[RANGE_15] > 0 || iAnythingWithinRange[RANGE_15] > 3 || ((CurrentTarget.IsEliteRareUnique || CurrentTarget.bIsTreasureGoblin || CurrentTarget.IsBoss) && CurrentTarget.fRadiusDistance <= 25f)) &&
                playerStatus.dCurrentEnergy >= 103 && PowerManager.CanCast(SNOPower.Witchdoctor_WallOfZombies))
            {
                return new GilesPower(SNOPower.Witchdoctor_WallOfZombies, 25f, CurrentTarget.vPosition, iCurrentWorldID, -1, 1, 1, USE_SLOWLY);
            }
            // Zombie Charger aka Zombie bears Spams Bears @ Everything from 11feet away
            if (!bOOCBuff && hashPowerHotbarAbilities.Contains(SNOPower.Witchdoctor_ZombieCharger) && !playerStatus.bIsIncapacitated && playerStatus.dCurrentEnergy >= 134 &&
                (iElitesWithinRange[RANGE_12] > 0 || iAnythingWithinRange[RANGE_12] >= 1 || ((CurrentTarget.IsEliteRareUnique || CurrentTarget.bIsTreasureGoblin || CurrentTarget.IsBoss) && CurrentTarget.fRadiusDistance <= 11f)) &&
                PowerManager.CanCast(SNOPower.Witchdoctor_ZombieCharger))
            {
                return new GilesPower(SNOPower.Witchdoctor_ZombieCharger, 11f, new Vector3(CurrentTarget.vPosition.X, CurrentTarget.vPosition.Y, CurrentTarget.vPosition.Z + iThisHeight), iCurrentWorldID, -1, 0, 0, USE_SLOWLY);
            }
            // Acid Cloud
            if (!bOOCBuff && !bCurrentlyAvoiding && hashPowerHotbarAbilities.Contains(SNOPower.Witchdoctor_AcidCloud) && !playerStatus.bIsIncapacitated &&
                playerStatus.dCurrentEnergy >= 172 && PowerManager.CanCast(SNOPower.Witchdoctor_AcidCloud))
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
                return new GilesPower(SNOPower.Witchdoctor_AcidCloud, 30f, vNewTarget, iCurrentWorldID, -1, 1, 1, USE_SLOWLY);
            }
            // Fire Bats fast-attack
            if (!bOOCBuff && !bCurrentlyAvoiding && hashPowerHotbarAbilities.Contains(SNOPower.Witchdoctor_Firebats) && !playerStatus.bIsIncapacitated && playerStatus.dCurrentEnergy >= 98)
            {
                return new GilesPower(SNOPower.Witchdoctor_Firebats, 40f, vNullLocation, -1, CurrentTarget.iACDGuid, 0, 1, USE_SLOWLY);
            }
            // Poison Darts fast-attack Spams Darts when mana is too low (to cast bears) @12yds or @10yds if Bears avialable
            if (!bOOCBuff && !bCurrentlyAvoiding && hashPowerHotbarAbilities.Contains(SNOPower.Witchdoctor_PoisonDart) && !playerStatus.bIsIncapacitated)
            {
                float fUseThisRange = 35f;
                if (hashPowerHotbarAbilities.Contains(SNOPower.Witchdoctor_ZombieCharger) && playerStatus.dCurrentEnergy >= 150)
                    fUseThisRange = 30f;
                return new GilesPower(SNOPower.Witchdoctor_PoisonDart, fUseThisRange, vNullLocation, -1, CurrentTarget.iACDGuid, 0, 2, USE_SLOWLY);
            }
            // Corpse Spiders fast-attacks Spams Spiders when mana is too low (to cast bears) @12yds or @10yds if Bears avialable
            if (!bOOCBuff && !bCurrentlyAvoiding && hashPowerHotbarAbilities.Contains(SNOPower.Witchdoctor_CorpseSpider) && !playerStatus.bIsIncapacitated)
            {
                float fUseThisRange = 35f;
                if (hashPowerHotbarAbilities.Contains(SNOPower.Witchdoctor_ZombieCharger) && playerStatus.dCurrentEnergy >= 150)
                    fUseThisRange = 30f;
                return new GilesPower(SNOPower.Witchdoctor_CorpseSpider, fUseThisRange, vNullLocation, -1, CurrentTarget.iACDGuid, 0, 1, USE_SLOWLY);
            }
            // Toads fast-attacks Spams Toads when mana is too low (to cast bears) @12yds or @10yds if Bears avialable
            if (!bOOCBuff && !bCurrentlyAvoiding && hashPowerHotbarAbilities.Contains(SNOPower.Witchdoctor_PlagueOfToads) && !playerStatus.bIsIncapacitated)
            {
                float fUseThisRange = 35f;
                if (hashPowerHotbarAbilities.Contains(SNOPower.Witchdoctor_ZombieCharger) && playerStatus.dCurrentEnergy >= 150)
                    fUseThisRange = 30f;
                return new GilesPower(SNOPower.Witchdoctor_PlagueOfToads, fUseThisRange, vNullLocation, -1, CurrentTarget.iACDGuid, 0, 1, USE_SLOWLY);
            }
            // Fire Bomb fast-attacks Spams Bomb when mana is too low (to cast bears) @12yds or @10yds if Bears avialable
            if (!bOOCBuff && !bCurrentlyAvoiding && hashPowerHotbarAbilities.Contains(SNOPower.Witchdoctor_Firebomb) && !playerStatus.bIsIncapacitated)
            {
                float fUseThisRange = 35f;
                if (hashPowerHotbarAbilities.Contains(SNOPower.Witchdoctor_ZombieCharger) && playerStatus.dCurrentEnergy >= 150)
                    fUseThisRange = 30f;
                return new GilesPower(SNOPower.Witchdoctor_Firebomb, fUseThisRange, vNullLocation, -1, CurrentTarget.iACDGuid, 0, 1, USE_SLOWLY);
            }
            // Default attacks
            if (!bOOCBuff && !bCurrentlyAvoiding && !playerStatus.bIsIncapacitated)
            {
                return new GilesPower(SNOPower.Weapon_Melee_Instant, 11f, vNullLocation, -1, CurrentTarget.iACDGuid, 1, 1, USE_SLOWLY);
            }
            return defaultPower;
        }

        private static GilesPower GetWitchDoctorDestroyPower()
        {
            if (hashPowerHotbarAbilities.Contains(SNOPower.Witchdoctor_Firebats))
                return new GilesPower(SNOPower.Witchdoctor_Firebats, 12f, vNullLocation, -1, -1, 0, 0, USE_SLOWLY);
            if (hashPowerHotbarAbilities.Contains(SNOPower.Witchdoctor_Firebomb))
                return new GilesPower(SNOPower.Witchdoctor_Firebomb, 12f, vNullLocation, -1, -1, 0, 0, USE_SLOWLY);
            if (hashPowerHotbarAbilities.Contains(SNOPower.Witchdoctor_PoisonDart))
                return new GilesPower(SNOPower.Witchdoctor_PoisonDart, 15f, vNullLocation, -1, -1, 0, 0, USE_SLOWLY);
            if (hashPowerHotbarAbilities.Contains(SNOPower.Witchdoctor_ZombieCharger) && playerStatus.dCurrentEnergy >= 140)
                return new GilesPower(SNOPower.Witchdoctor_ZombieCharger, 12f, vNullLocation, -1, -1, 0, 0, USE_SLOWLY);
            if (hashPowerHotbarAbilities.Contains(SNOPower.Witchdoctor_CorpseSpider))
                return new GilesPower(SNOPower.Witchdoctor_CorpseSpider, 12f, vNullLocation, -1, -1, 0, 0, USE_SLOWLY);
            if (hashPowerHotbarAbilities.Contains(SNOPower.Witchdoctor_PlagueOfToads))
                return new GilesPower(SNOPower.Witchdoctor_PlagueOfToads, 12f, vNullLocation, -1, -1, 0, 0, USE_SLOWLY);
            return new GilesPower(SNOPower.Weapon_Melee_Instant, 10f, vNullLocation, -1, -1, 0, 0, USE_SLOWLY);
        }

    }
}
