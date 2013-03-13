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
        private static TrinityPower GetWitchDoctorPower(bool IsCurrentlyAvoiding, bool UseOOCBuff, bool UseDestructiblePower)
        {
            bool hasGraveInjustice = ZetaDia.CPlayer.PassiveSkills.Contains(SNOPower.Witchdoctor_Passive_GraveInjustice);

            bool hasAngryChicken = HotbarSkills.AssignedSkills.Any(s => s.Power == SNOPower.Witchdoctor_Hex && s.RuneIndex == 1);
            bool isChicken = hasAngryChicken && GetHasBuff(SNOPower.Witchdoctor_Hex);

            // Hex with angry chicken, is chicken, explode!
            if (!UseOOCBuff && isChicken && TargetUtil.AnyMobsInRange(12f, 1))
            {
                return new TrinityPower(SNOPower.Witchdoctor_Hex_Explode, 0f, vNullLocation, CurrentWorldDynamicId, -1, 0, 0, WAIT_FOR_ANIM);
            }
            else if (isChicken) // we can't use any powers if we're a chicken
            {
                return new TrinityPower(SNOPower.None, 0f, vNullLocation, -1, -1, 0, 0, NO_WAIT_ANIM);
            }


            // Pick the best destructible power available
            if (UseDestructiblePower)
            {
                return GetWitchDoctorDestroyPower();
            }
            // Witch doctors have no reserve requirements?
            MinEnergyReserve = 0;
            // Spirit Walk Cast on 65% health or while avoiding anything but molten core or incapacitated or Chasing Goblins
            if (Hotbar.Contains(SNOPower.Witchdoctor_SpiritWalk) && PlayerStatus.PrimaryResource >= 49 &&
                (
                 PlayerStatus.CurrentHealthPct <= 0.65 || PlayerStatus.IsIncapacitated || PlayerStatus.IsRooted || (Settings.Combat.Misc.AllowOOCMovement && UseOOCBuff) ||
                 (!UseOOCBuff && CurrentTarget.IsTreasureGoblin && CurrentTarget.HitPointsPct < 0.90 && CurrentTarget.RadiusDistance <= 40f)
                ) &&
                PowerManager.CanCast(SNOPower.Witchdoctor_SpiritWalk))
            {
                return new TrinityPower(SNOPower.Witchdoctor_SpiritWalk, 0f, vNullLocation, CurrentWorldDynamicId, -1, 0, 0, WAIT_FOR_ANIM);
            }

            // Witch Doctor - Terror
            //skillDict.Add("SoulHarvest", SNOPower.Witchdoctor_SoulHarvest);
            //runeDict.Add("SwallowYourSoul", 3);
            //runeDict.Add("Siphon", 0);
            //runeDict.Add("Languish", 2);
            //runeDict.Add("SoulToWaste", 1);
            //runeDict.Add("VengefulSpirit", 4);

            bool hasVengefulSpirit = HotbarSkills.AssignedSkills.Any(s => s.Power == SNOPower.Witchdoctor_SoulHarvest && s.RuneIndex == 4);

            // Soul Harvest Any Elites or 2+ Norms and baby it's harvest season
            if (!UseOOCBuff && Hotbar.Contains(SNOPower.Witchdoctor_SoulHarvest) && !PlayerStatus.IsIncapacitated && PlayerStatus.PrimaryResource >= 59 && GetBuffStacks(SNOPower.Witchdoctor_SoulHarvest) <= 4 &&
                (TargetUtil.AnyMobsInRange(16f, 2) || TargetUtil.IsEliteTargetInRange(16f)) && PowerManager.CanCast(SNOPower.Witchdoctor_SoulHarvest))
            {
                return new TrinityPower(SNOPower.Witchdoctor_SoulHarvest, 0f, vNullLocation, CurrentWorldDynamicId, -1, 0, 0, WAIT_FOR_ANIM);
            }

            // Soul Harvest with VengefulSpirit
            if (!UseOOCBuff && !IsCurrentlyAvoiding && !PlayerStatus.IsIncapacitated && Hotbar.Contains(SNOPower.Witchdoctor_SoulHarvest) && hasVengefulSpirit && PlayerStatus.PrimaryResource >= 59
                && TargetUtil.AnyMobsInRange(16, 3) && GetBuffStacks(SNOPower.Witchdoctor_SoulHarvest) <= 4 && PowerManager.CanCast(SNOPower.Witchdoctor_SoulHarvest))
            {
                return new TrinityPower(SNOPower.Witchdoctor_SoulHarvest, 0f, vNullLocation, CurrentWorldDynamicId, -1, 0, 0, WAIT_FOR_ANIM);
            }

            // Sacrifice AKA Zombie Dog Jihad, use on Elites Only or to try and Save yourself
            if (!UseOOCBuff && Hotbar.Contains(SNOPower.Witchdoctor_Sacrifice) &&
                (ElitesWithinRange[RANGE_15] > 0 || ((CurrentTarget.IsEliteRareUnique || CurrentTarget.IsBoss || CurrentTarget.IsTreasureGoblin) && CurrentTarget.RadiusDistance <= 15f)) &&
                PowerManager.CanCast(SNOPower.Witchdoctor_Sacrifice))
            {
                return new TrinityPower(SNOPower.Witchdoctor_Sacrifice, 0f, vNullLocation, CurrentWorldDynamicId, -1, 1, 0, WAIT_FOR_ANIM);
            }
            // Gargantuan, Recast on 1+ Elites or Bosses to trigger Restless Giant
            if (!UseOOCBuff && !IsCurrentlyAvoiding && Hotbar.Contains(SNOPower.Witchdoctor_Gargantuan) && !PlayerStatus.IsIncapacitated && PlayerStatus.PrimaryResource >= 147 &&
                (ElitesWithinRange[RANGE_15] >= 1 ||
                 (CurrentTarget != null && (CurrentTarget.IsBossOrEliteRareUnique && CurrentTarget.RadiusDistance <= 15f)) || iPlayerOwnedGargantuan == 0) &&
                PowerManager.CanCast(SNOPower.Witchdoctor_Gargantuan))
            {
                return new TrinityPower(SNOPower.Witchdoctor_Gargantuan, 0f, vNullLocation, CurrentWorldDynamicId, -1, 2, 1, WAIT_FOR_ANIM);
            }
            // Zombie dogs Woof Woof, good for being blown up, cast when less than or equal to 2 Dogs or Not Blowing them up and cast when less than 4
            if (!IsCurrentlyAvoiding && Hotbar.Contains(SNOPower.Witchdoctor_SummonZombieDog) && !PlayerStatus.IsIncapacitated &&
                PlayerStatus.PrimaryResource >= 49 && (ElitesWithinRange[RANGE_20] >= 2 || AnythingWithinRange[RANGE_20] >= 5 ||
                 (CurrentTarget != null && ((CurrentTarget.IsEliteRareUnique || CurrentTarget.IsTreasureGoblin) && CurrentTarget.RadiusDistance <= 30f)) || iPlayerOwnedZombieDog <= 2) &&
                PowerManager.CanCast(SNOPower.Witchdoctor_SummonZombieDog))
            {
                return new TrinityPower(SNOPower.Witchdoctor_SummonZombieDog, 0f, vNullLocation, CurrentWorldDynamicId, -1, 0, 0, WAIT_FOR_ANIM);
            }


            // Hex with angry chicken, check if we want to shape shift and explode
            if (Hotbar.Contains(SNOPower.Witchdoctor_Hex) && hasAngryChicken && PowerManager.CanCast(SNOPower.Witchdoctor_Hex) && PlayerStatus.PrimaryResource >= 49)
            {
                return new TrinityPower(SNOPower.Witchdoctor_Hex, 0f, vNullLocation, CurrentWorldDynamicId, -1, 0, 0, NO_WAIT_ANIM);
            }

            // Hex Spam Cast without angry chicken
            if (!UseOOCBuff && Hotbar.Contains(SNOPower.Witchdoctor_Hex) && !PlayerStatus.IsIncapacitated && PlayerStatus.PrimaryResource >= 49 && !hasAngryChicken &&
               (TargetUtil.AnyElitesInRange(12) || TargetUtil.AnyMobsInRange(12, 2) || TargetUtil.IsEliteTargetInRange(18f)) && PowerManager.CanCast(SNOPower.Witchdoctor_Hex))
            {
                return new TrinityPower(SNOPower.Witchdoctor_Hex, 0f, vNullLocation, CurrentWorldDynamicId, -1, 0, 0, NO_WAIT_ANIM);
            }
            // Mass Confuse, elites only or big mobs or to escape on low health
            if (!UseOOCBuff && Hotbar.Contains(SNOPower.Witchdoctor_MassConfusion) && !PlayerStatus.IsIncapacitated && PlayerStatus.PrimaryResource >= 74 &&
                (ElitesWithinRange[RANGE_12] >= 1 || AnythingWithinRange[RANGE_12] >= 6 || PlayerStatus.CurrentHealthPct <= 0.25 || (CurrentTarget.IsBossOrEliteRareUnique && CurrentTarget.RadiusDistance <= 12f)) &&
                !CurrentTarget.IsTreasureGoblin && PowerManager.CanCast(SNOPower.Witchdoctor_MassConfusion))
            {
                return new TrinityPower(SNOPower.Witchdoctor_MassConfusion, 0f, vNullLocation, -1, CurrentTarget.ACDGuid, 1, 1, WAIT_FOR_ANIM);
            }
            // Big Bad Voodoo, elites and bosses only
            if (!UseOOCBuff && Hotbar.Contains(SNOPower.Witchdoctor_BigBadVoodoo) && !PlayerStatus.IsIncapacitated &&
                !CurrentTarget.IsTreasureGoblin &&
                (ElitesWithinRange[RANGE_6] > 0 || (CurrentTarget.IsBossOrEliteRareUnique && CurrentTarget.RadiusDistance <= 12f)) &&
                PowerManager.CanCast(SNOPower.Witchdoctor_BigBadVoodoo))
            {
                return new TrinityPower(SNOPower.Witchdoctor_BigBadVoodoo, 0f, vNullLocation, CurrentWorldDynamicId, -1, 0, 0, WAIT_FOR_ANIM);
            }

            // Grasp of the Dead, look below, droping globes and dogs when using it on elites and 3 norms
            if (!UseOOCBuff && !IsCurrentlyAvoiding && Hotbar.Contains(SNOPower.Witchdoctor_GraspOfTheDead) && !PlayerStatus.IsIncapacitated &&
                (TargetUtil.AnyMobsInRange(30, 2)) &&
                PlayerStatus.PrimaryResource >= 78 && PowerManager.CanCast(SNOPower.Witchdoctor_GraspOfTheDead))
            {
                var bestClusterPoint = TargetUtil.GetBestClusterPoint(15);

                return new TrinityPower(SNOPower.Witchdoctor_GraspOfTheDead, 25f, bestClusterPoint, CurrentWorldDynamicId, -1, 0, 0, WAIT_FOR_ANIM);
            }
            // Horrify Buff When not in combat for movement speed
            if (UseOOCBuff && Settings.Combat.WitchDoctor.GraveInjustice == true && Hotbar.Contains(SNOPower.Witchdoctor_Horrify) && !PlayerStatus.IsIncapacitated && PlayerStatus.PrimaryResource >= 37 &&
                PowerManager.CanCast(SNOPower.Witchdoctor_Horrify))
            {
                return new TrinityPower(SNOPower.Witchdoctor_Horrify, 0f, vNullLocation, CurrentWorldDynamicId, -1, 0, 0, WAIT_FOR_ANIM);
            }
            // Horrify Buff at 35% health
            if (!UseOOCBuff && Hotbar.Contains(SNOPower.Witchdoctor_Horrify) && !PlayerStatus.IsIncapacitated && PlayerStatus.PrimaryResource >= 37 &&
                PlayerStatus.CurrentHealthPct <= 0.35 &&
                PowerManager.CanCast(SNOPower.Witchdoctor_Horrify))
            {
                return new TrinityPower(SNOPower.Witchdoctor_Horrify, 0f, vNullLocation, CurrentWorldDynamicId, -1, 0, 0, WAIT_FOR_ANIM);
            }
            // Fetish Army, elites only
            if (!UseOOCBuff && Hotbar.Contains(SNOPower.Witchdoctor_FetishArmy) && !PlayerStatus.IsIncapacitated &&
                (ElitesWithinRange[RANGE_25] > 0 || ((CurrentTarget.IsEliteRareUnique || CurrentTarget.IsTreasureGoblin || CurrentTarget.IsBoss) && CurrentTarget.RadiusDistance <= 16f)) &&
                PowerManager.CanCast(SNOPower.Witchdoctor_FetishArmy))
            {
                return new TrinityPower(SNOPower.Witchdoctor_FetishArmy, 0f, vNullLocation, CurrentWorldDynamicId, -1, 1, 1, WAIT_FOR_ANIM);
            }

            //skillDict.Add("SpiritBarage", SNOPower.Witchdoctor_SpiritBarrage);
            //runeDict.Add("TheSpiritIsWilling", 3);
            //runeDict.Add("WellOfSouls", 1);
            //runeDict.Add("Phantasm", 2);
            //runeDict.Add("Phlebotomize", 0);
            //runeDict.Add("Manitou", 4);

            bool hasManitou = HotbarSkills.AssignedSkills.Any(s => s.Power == SNOPower.Witchdoctor_SpiritBarrage && s.RuneIndex == 4);

            // Spirit Barrage
            if (!UseOOCBuff && !IsCurrentlyAvoiding && Hotbar.Contains(SNOPower.Witchdoctor_SpiritBarrage) && !PlayerStatus.IsIncapacitated && PlayerStatus.PrimaryResource >= 108 &&
                PowerManager.CanCast(SNOPower.Witchdoctor_SpiritBarrage) && !hasManitou)
            {
                return new TrinityPower(SNOPower.Witchdoctor_SpiritBarrage, 21f, vNullLocation, -1, CurrentTarget.ACDGuid, 2, 2, WAIT_FOR_ANIM);
            }

            // Spirit Barrage Manitou
            if (Hotbar.Contains(SNOPower.Witchdoctor_SpiritBarrage) && PlayerStatus.PrimaryResource >= 108 && TimeSinceUse(SNOPower.Witchdoctor_SpiritBarrage) > 18000 && hasManitou)
            {
                return new TrinityPower(SNOPower.Witchdoctor_SpiritBarrage, 0f, vNullLocation, CurrentWorldDynamicId, -1, 2, 2, WAIT_FOR_ANIM);
            }

            // Haunt the shit out of monster and maybe they will give you treats
            if (!UseOOCBuff && !IsCurrentlyAvoiding && Hotbar.Contains(SNOPower.Witchdoctor_Haunt) && !PlayerStatus.IsIncapacitated && PlayerStatus.PrimaryResource >= 98 &&
                PowerManager.CanCast(SNOPower.Witchdoctor_Haunt))
            {
                return new TrinityPower(SNOPower.Witchdoctor_Haunt, 21f, vNullLocation, -1, CurrentTarget.ACDGuid, 1, 1, WAIT_FOR_ANIM);
            }

            // Locust
            if (!UseOOCBuff && !IsCurrentlyAvoiding && Hotbar.Contains(SNOPower.Witchdoctor_Locust_Swarm) && !PlayerStatus.IsIncapacitated && PlayerStatus.PrimaryResource >= 196 &&
                PowerManager.CanCast(SNOPower.Witchdoctor_Locust_Swarm) && !CurrentTarget.HasDotDPS && LastPowerUsed != SNOPower.Witchdoctor_Locust_Swarm)
            {
                return new TrinityPower(SNOPower.Witchdoctor_Locust_Swarm, 12f, vNullLocation, -1, CurrentTarget.ACDGuid, 1, 1, WAIT_FOR_ANIM);
            }
            // Wall of Zombies
            if (!UseOOCBuff && !IsCurrentlyAvoiding && Hotbar.Contains(SNOPower.Witchdoctor_WallOfZombies) && !PlayerStatus.IsIncapacitated &&
                (ElitesWithinRange[RANGE_15] > 0 || AnythingWithinRange[RANGE_15] > 3 || ((CurrentTarget.IsEliteRareUnique || CurrentTarget.IsTreasureGoblin || CurrentTarget.IsBoss) && CurrentTarget.RadiusDistance <= 25f)) &&
                PlayerStatus.PrimaryResource >= 103 && PowerManager.CanCast(SNOPower.Witchdoctor_WallOfZombies))
            {
                return new TrinityPower(SNOPower.Witchdoctor_WallOfZombies, 25f, CurrentTarget.Position, CurrentWorldDynamicId, -1, 1, 1, WAIT_FOR_ANIM);
            }
            // Zombie Charger aka Zombie bears Spams Bears @ Everything from 11feet away
            if (!UseOOCBuff && Hotbar.Contains(SNOPower.Witchdoctor_ZombieCharger) && !PlayerStatus.IsIncapacitated && PlayerStatus.PrimaryResource >= 134 &&
                TargetUtil.AnyMobsInRange(11f) &&
                PowerManager.CanCast(SNOPower.Witchdoctor_ZombieCharger))
            {
                return new TrinityPower(SNOPower.Witchdoctor_ZombieCharger, 11f, CurrentTarget.Position, CurrentWorldDynamicId, -1, 0, 0, WAIT_FOR_ANIM);
            }
            // Acid Cloud
            if (!UseOOCBuff && !IsCurrentlyAvoiding && Hotbar.Contains(SNOPower.Witchdoctor_AcidCloud) && !PlayerStatus.IsIncapacitated &&
                PlayerStatus.PrimaryResource >= 172 && PowerManager.CanCast(SNOPower.Witchdoctor_AcidCloud))
            {
                var bestClusterPoint = TargetUtil.GetBestClusterPoint(15f, 30f);
                return new TrinityPower(SNOPower.Witchdoctor_AcidCloud, 30f, bestClusterPoint, CurrentWorldDynamicId, -1, 1, 1, WAIT_FOR_ANIM);
            }
            // Fire Bats fast-attack
            if (!UseOOCBuff && !IsCurrentlyAvoiding && Hotbar.Contains(SNOPower.Witchdoctor_Firebats) && !PlayerStatus.IsIncapacitated && PlayerStatus.PrimaryResource >= 98)
            {
                return new TrinityPower(SNOPower.Witchdoctor_Firebats, 20f, vNullLocation, -1, CurrentTarget.ACDGuid, 0, 1, WAIT_FOR_ANIM);
            }
            // Poison Darts fast-attack Spams Darts when mana is too low (to cast bears) @12yds or @10yds if Bears avialable
            if (!UseOOCBuff && !IsCurrentlyAvoiding && Hotbar.Contains(SNOPower.Witchdoctor_PoisonDart) && !PlayerStatus.IsIncapacitated)
            {
                float fUseThisRange = 35f;
                if (Hotbar.Contains(SNOPower.Witchdoctor_ZombieCharger) && PlayerStatus.PrimaryResource >= 150)
                    fUseThisRange = 30f;
                return new TrinityPower(SNOPower.Witchdoctor_PoisonDart, fUseThisRange, vNullLocation, -1, CurrentTarget.ACDGuid, 0, 2, WAIT_FOR_ANIM);
            }
            // Corpse Spiders fast-attacks Spams Spiders when mana is too low (to cast bears) @12yds or @10yds if Bears avialable
            if (!UseOOCBuff && !IsCurrentlyAvoiding && Hotbar.Contains(SNOPower.Witchdoctor_CorpseSpider) && !PlayerStatus.IsIncapacitated)
            {
                float fUseThisRange = 35f;
                if (Hotbar.Contains(SNOPower.Witchdoctor_ZombieCharger) && PlayerStatus.PrimaryResource >= 150)
                    fUseThisRange = 30f;
                return new TrinityPower(SNOPower.Witchdoctor_CorpseSpider, fUseThisRange, vNullLocation, -1, CurrentTarget.ACDGuid, 0, 1, WAIT_FOR_ANIM);
            }
            // Toads fast-attacks Spams Toads when mana is too low (to cast bears) @12yds or @10yds if Bears avialable
            if (!UseOOCBuff && !IsCurrentlyAvoiding && Hotbar.Contains(SNOPower.Witchdoctor_PlagueOfToads) && !PlayerStatus.IsIncapacitated)
            {
                float fUseThisRange = 35f;
                if (Hotbar.Contains(SNOPower.Witchdoctor_ZombieCharger) && PlayerStatus.PrimaryResource >= 150)
                    fUseThisRange = 30f;
                return new TrinityPower(SNOPower.Witchdoctor_PlagueOfToads, fUseThisRange, vNullLocation, -1, CurrentTarget.ACDGuid, 0, 1, WAIT_FOR_ANIM);
            }
            // Fire Bomb fast-attacks Spams Bomb when mana is too low (to cast bears) @12yds or @10yds if Bears avialable
            if (!UseOOCBuff && !IsCurrentlyAvoiding && Hotbar.Contains(SNOPower.Witchdoctor_Firebomb) && !PlayerStatus.IsIncapacitated)
            {
                float fUseThisRange = 35f;
                if (Hotbar.Contains(SNOPower.Witchdoctor_ZombieCharger) && PlayerStatus.PrimaryResource >= 150)
                    fUseThisRange = 30f;
                return new TrinityPower(SNOPower.Witchdoctor_Firebomb, fUseThisRange, vNullLocation, -1, CurrentTarget.ACDGuid, 0, 1, WAIT_FOR_ANIM);
            }
            // Default attacks
            if (!UseOOCBuff && !IsCurrentlyAvoiding)
            {
                return new TrinityPower(GetDefaultWeaponPower(), GetDefaultWeaponDistance(), vNullLocation, -1, CurrentTarget.ACDGuid, 0, 0, WAIT_FOR_ANIM);
            }
            return new TrinityPower(SNOPower.None, -1, vNullLocation, -1, -1, 0, 0, WAIT_FOR_ANIM);
        }

        private static TrinityPower GetWitchDoctorDestroyPower()
        {
            if (Hotbar.Contains(SNOPower.Witchdoctor_Firebats))
                return new TrinityPower(SNOPower.Witchdoctor_Firebats, 12f, vNullLocation, -1, -1, 0, 0, WAIT_FOR_ANIM);
            if (Hotbar.Contains(SNOPower.Witchdoctor_Firebomb))
                return new TrinityPower(SNOPower.Witchdoctor_Firebomb, 12f, vNullLocation, -1, -1, 0, 0, WAIT_FOR_ANIM);
            if (Hotbar.Contains(SNOPower.Witchdoctor_PoisonDart))
                return new TrinityPower(SNOPower.Witchdoctor_PoisonDart, 15f, vNullLocation, -1, -1, 0, 0, WAIT_FOR_ANIM);
            if (Hotbar.Contains(SNOPower.Witchdoctor_ZombieCharger) && PlayerStatus.PrimaryResource >= 140)
                return new TrinityPower(SNOPower.Witchdoctor_ZombieCharger, 12f, vNullLocation, -1, -1, 0, 0, WAIT_FOR_ANIM);
            if (Hotbar.Contains(SNOPower.Witchdoctor_CorpseSpider))
                return new TrinityPower(SNOPower.Witchdoctor_CorpseSpider, 12f, vNullLocation, -1, -1, 0, 0, WAIT_FOR_ANIM);
            if (Hotbar.Contains(SNOPower.Witchdoctor_PlagueOfToads))
                return new TrinityPower(SNOPower.Witchdoctor_PlagueOfToads, 12f, vNullLocation, -1, -1, 0, 0, WAIT_FOR_ANIM);
            if (Hotbar.Contains(SNOPower.Witchdoctor_AcidCloud) && PlayerStatus.PrimaryResource >= 172)
                return new TrinityPower(SNOPower.Witchdoctor_AcidCloud, 12f, vNullLocation, -1, -1, 0, 0, WAIT_FOR_ANIM);
            return new TrinityPower(SNOPower.Weapon_Melee_Instant, 10f, vNullLocation, -1, -1, 0, 0, WAIT_FOR_ANIM);
        }

    }
}
