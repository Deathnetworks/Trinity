using System;
using System.Linq;
using Trinity.Combat.Abilities;
using Zeta.Bot;
using Zeta.Common;
using Zeta.Common.Plugins;
using Zeta.Game;
using Zeta.Game.Internals.Actors;
namespace Trinity
{
    public partial class Trinity : IPlugin
    {
        private static TrinityPower GetWitchDoctorPower(bool IsCurrentlyAvoiding, bool UseOOCBuff, bool UseDestructiblePower)
        {
            // Pick the best destructible power available
            if (UseDestructiblePower)
            {
                return GetWitchDoctorDestroyPower();
            }

            bool hasGraveInjustice = ZetaDia.CPlayer.PassiveSkills.Contains(SNOPower.Witchdoctor_Passive_GraveInjustice);

            bool hasAngryChicken = HotbarSkills.AssignedSkills.Any(s => s.Power == SNOPower.Witchdoctor_Hex && s.RuneIndex == 1);
            bool isChicken = hasAngryChicken && Player.IsHidden;

            bool hasVisionQuest = ZetaDia.CPlayer.PassiveSkills.Any(s => s == SNOPower.Witchdoctor_Passive_VisionQuest);

            // Set max ranged attack range, based on Grave Injustice, and current target NOT standing in avoidance, and health > 25%
            float rangedAttackMaxRange = 30f;
            if (!UseOOCBuff && hasGraveInjustice && !CurrentTarget.IsStandingInAvoidance && Player.CurrentHealthPct > 0.25)
                rangedAttackMaxRange = Math.Min(Player.GoldPickupRadius + 8f, 30f);

            // Set basic attack range, depending on whether or not we have Bears
            float basicAttackRange = 35f;
            if (hasGraveInjustice)
                basicAttackRange = rangedAttackMaxRange;
            else if (Hotbar.Contains(SNOPower.Witchdoctor_ZombieCharger) && Player.PrimaryResource >= 150)
                basicAttackRange = 30f;


            // Hex with angry chicken, is chicken, explode!
            if (!UseOOCBuff && isChicken && (TargetUtil.AnyMobsInRange(12f, 1, false) || CurrentTarget.RadiusDistance <= 10f || UseDestructiblePower) && PowerManager.CanCast(SNOPower.Witchdoctor_Hex_Explode))
            {
                ShouldRefreshHotbarAbilities = true;
                return new TrinityPower(SNOPower.Witchdoctor_Hex_Explode, 0f, Vector3.Zero, CurrentWorldDynamicId, -1, 2, 2, WAIT_FOR_ANIM);
            }
            else if (hasAngryChicken)
            {
                ShouldRefreshHotbarAbilities = true;
            }

            //skillDict.Add("SpiritWalk", SNOPower.Witchdoctor_SpiritWalk);
            //runeDict.Add("Jaunt", 1);
            //runeDict.Add("HonoredGuest", 3);
            //runeDict.Add("UmbralShock", 2);
            //runeDict.Add("Severance", 0);
            //runeDict.Add("HealingJourney", 4);

            bool hasJaunt = HotbarSkills.AssignedSkills.Any(s => s.Power == SNOPower.Witchdoctor_SpiritWalk && s.RuneIndex == 1);
            bool hasHonoredGuest = HotbarSkills.AssignedSkills.Any(s => s.Power == SNOPower.Witchdoctor_SpiritWalk && s.RuneIndex == 3);
            bool hasUmbralShock = HotbarSkills.AssignedSkills.Any(s => s.Power == SNOPower.Witchdoctor_SpiritWalk && s.RuneIndex == 2);
            bool hasSeverance = HotbarSkills.AssignedSkills.Any(s => s.Power == SNOPower.Witchdoctor_SpiritWalk && s.RuneIndex == 0);
            bool hasHealingJourney = HotbarSkills.AssignedSkills.Any(s => s.Power == SNOPower.Witchdoctor_SpiritWalk && s.RuneIndex == 4);

            // Witch doctors have no reserve requirements?
            MinEnergyReserve = 0;

            // Spirit Walk OOC 
            if (UseOOCBuff && CombatBase.CanCast(SNOPower.Witchdoctor_SpiritWalk, CombatBase.CanCastFlags.NoTimer) && Player.PrimaryResource >= 49 &&
               Settings.Combat.Misc.AllowOOCMovement)
            {
                return new TrinityPower(SNOPower.Witchdoctor_SpiritWalk, 0f, Vector3.Zero, CurrentWorldDynamicId, -1, 0, 0, WAIT_FOR_ANIM);
            }

            // Spirit Walk While incapacitated or for Goblins
            if (CombatBase.CanCast(SNOPower.Witchdoctor_SpiritWalk, CombatBase.CanCastFlags.NoTimer) && Player.PrimaryResource >= 49 &&
                (Player.IsIncapacitated || Player.IsRooted ||
                 (!UseOOCBuff && CurrentTarget.IsTreasureGoblin && CurrentTarget.HitPointsPct < 0.90 && CurrentTarget.RadiusDistance <= 40f)
                ))
            {
                return new TrinityPower(SNOPower.Witchdoctor_SpiritWalk, 0f, Vector3.Zero, CurrentWorldDynamicId, -1, 0, 0, WAIT_FOR_ANIM);
            }

            // Spirit Walk < 65% Health: Healing Journey
            if (CombatBase.CanCast(SNOPower.Witchdoctor_SpiritWalk) && Player.PrimaryResource >= 49 && hasHealingJourney &&
                Player.CurrentHealthPct <= V.F("WitchDoctor.SpiritWalk.HealingJourneyHealth"))
            {
                return new TrinityPower(SNOPower.Witchdoctor_SpiritWalk, 0f, Vector3.Zero, CurrentWorldDynamicId, -1, 0, 0, WAIT_FOR_ANIM);
            }

            // Spirit Walk < 50% Mana: Honored Guest
            if (CombatBase.CanCast(SNOPower.Witchdoctor_SpiritWalk) && Player.PrimaryResource >= 49 && hasHonoredGuest &&
                Player.PrimaryResourcePct <= V.F("WitchDoctor.SpiritWalk.HonoredGuestMana"))
            {
                return new TrinityPower(SNOPower.Witchdoctor_SpiritWalk, 0f, Vector3.Zero, CurrentWorldDynamicId, -1, 0, 0, WAIT_FOR_ANIM);
            }

            //bool shouldRefreshVisionQuest = WitchDoctorCombat.GetTimeSinceLastVisionQuestRefresh() > 4000;
            bool shouldRefreshVisionQuest = !GetHasBuff(SNOPower.Witchdoctor_Passive_VisionQuest) || WitchDoctorCombat.GetTimeSinceLastVisionQuestRefresh() > 3800;

            // Vision Quest Passive
            if (hasVisionQuest && shouldRefreshVisionQuest)
            {
                // Poison Darts 
                if (!UseOOCBuff && !IsCurrentlyAvoiding && !Player.IsIncapacitated && CombatBase.CanCast(SNOPower.Witchdoctor_PoisonDart, CombatBase.CanCastFlags.NoTimer))
                {
                    WitchDoctorCombat.VisionQuestRefreshTimer.Restart();
                    return new TrinityPower(SNOPower.Witchdoctor_PoisonDart, basicAttackRange, Vector3.Zero, -1, CurrentTarget.ACDGuid, 0, 2, WAIT_FOR_ANIM);
                }
                // Corpse Spiders
                if (!UseOOCBuff && !IsCurrentlyAvoiding && !Player.IsIncapacitated && CombatBase.CanCast(SNOPower.Witchdoctor_CorpseSpider, CombatBase.CanCastFlags.NoTimer))
                {
                    WitchDoctorCombat.VisionQuestRefreshTimer.Restart();
                    return new TrinityPower(SNOPower.Witchdoctor_CorpseSpider, basicAttackRange, Vector3.Zero, -1, CurrentTarget.ACDGuid, 0, 1, WAIT_FOR_ANIM);
                }
                // Plague Of Toads 
                if (!UseOOCBuff && !IsCurrentlyAvoiding && !Player.IsIncapacitated && CombatBase.CanCast(SNOPower.Witchdoctor_PlagueOfToads, CombatBase.CanCastFlags.NoTimer))
                {
                    WitchDoctorCombat.VisionQuestRefreshTimer.Restart();
                    return new TrinityPower(SNOPower.Witchdoctor_PlagueOfToads, basicAttackRange, Vector3.Zero, -1, CurrentTarget.ACDGuid, 0, 1, WAIT_FOR_ANIM);
                }
                // Fire Bomb 
                if (!UseOOCBuff && !IsCurrentlyAvoiding && !Player.IsIncapacitated && CombatBase.CanCast(SNOPower.Witchdoctor_Firebomb, CombatBase.CanCastFlags.NoTimer))
                {
                    WitchDoctorCombat.VisionQuestRefreshTimer.Restart();
                    return new TrinityPower(SNOPower.Witchdoctor_Firebomb, basicAttackRange, Vector3.Zero, -1, CurrentTarget.ACDGuid, 0, 1, WAIT_FOR_ANIM);
                }
            }

            // Witch Doctor - Terror
            //skillDict.Add("SoulHarvest", SNOPower.Witchdoctor_SoulHarvest);
            //runeDict.Add("SwallowYourSoul", 3);
            //runeDict.Add("Siphon", 0);
            //runeDict.Add("Languish", 2);
            //runeDict.Add("SoulToWaste", 1);
            //runeDict.Add("VengefulSpirit", 4);

            bool hasVengefulSpirit = HotbarSkills.AssignedSkills.Any(s => s.Power == SNOPower.Witchdoctor_SoulHarvest && s.RuneIndex == 4);
            bool hasSwallowYourSoul = HotbarSkills.AssignedSkills.Any(s => s.Power == SNOPower.Witchdoctor_SoulHarvest && s.RuneIndex == 3);

            // Soul Harvest Any Elites or 2+ Norms and baby it's harvest season
            if (!UseOOCBuff && CombatBase.CanCast(SNOPower.Witchdoctor_SoulHarvest) && !Player.IsIncapacitated && Player.PrimaryResource >= 59 &&
                (TargetUtil.AnyMobsInRange(16f, GetBuffStacks(SNOPower.Witchdoctor_SoulHarvest) + 1, false) || (hasSwallowYourSoul && Player.PrimaryResourcePct <= 0.50) || TargetUtil.IsEliteTargetInRange(16f)))
            {
                return new TrinityPower(SNOPower.Witchdoctor_SoulHarvest, 0f, Vector3.Zero, CurrentWorldDynamicId, -1, 0, 0, WAIT_FOR_ANIM);
            }

            // Soul Harvest with VengefulSpirit
            if (!UseOOCBuff && !IsCurrentlyAvoiding && !Player.IsIncapacitated && CombatBase.CanCast(SNOPower.Witchdoctor_SoulHarvest) && hasVengefulSpirit && Player.PrimaryResource >= 59
                && TargetUtil.AnyMobsInRange(16, 3) && GetBuffStacks(SNOPower.Witchdoctor_SoulHarvest) <= 4)
            {
                return new TrinityPower(SNOPower.Witchdoctor_SoulHarvest, 0f, Vector3.Zero, CurrentWorldDynamicId, -1, 0, 0, WAIT_FOR_ANIM);
            }

            // Sacrifice
            if (!UseOOCBuff && CombatBase.CanCast(SNOPower.Witchdoctor_Sacrifice) && PlayerOwnedZombieDog > 0 &&
                (TargetUtil.AnyElitesInRange(15, 1) || (CurrentTarget.IsBossOrEliteRareUnique && CurrentTarget.RadiusDistance <= 9f)))
            {
                return new TrinityPower(SNOPower.Witchdoctor_Sacrifice, 0f, Vector3.Zero, CurrentWorldDynamicId, -1, 1, 0, WAIT_FOR_ANIM);
            }

            // Sacrifice for Circle of Life
            bool hasCircleofLife = ZetaDia.CPlayer.PassiveSkills.Any(s => s == SNOPower.Witchdoctor_Passive_CircleOfLife);
            if (!UseOOCBuff && CombatBase.CanCast(SNOPower.Witchdoctor_Sacrifice) && PlayerOwnedZombieDog > 0 && hasCircleofLife && TargetUtil.AnyMobsInRange(15f))
            {
                return new TrinityPower(SNOPower.Witchdoctor_Sacrifice, 0f, Vector3.Zero, CurrentWorldDynamicId, -1, 1, 0, WAIT_FOR_ANIM);
            }

            // Gargantuan, Recast on 1+ Elites or Bosses to trigger Restless Giant
            if (!UseOOCBuff && !IsCurrentlyAvoiding && CombatBase.CanCast(SNOPower.Witchdoctor_Gargantuan) && !Player.IsIncapacitated && Player.PrimaryResource >= 147 &&
                (TargetUtil.AnyElitesInRange(15, 1) ||
                 (CurrentTarget != null && (CurrentTarget.IsBossOrEliteRareUnique && CurrentTarget.RadiusDistance <= 15f)) || iPlayerOwnedGargantuan == 0))
            {
                return new TrinityPower(SNOPower.Witchdoctor_Gargantuan, 0f, Vector3.Zero, CurrentWorldDynamicId, -1, 2, 1, WAIT_FOR_ANIM);
            }

            // Zombie Dogs non-sacrifice build
            if (!IsCurrentlyAvoiding && CombatBase.CanCast(SNOPower.Witchdoctor_SummonZombieDog) && !Hotbar.Contains(SNOPower.Witchdoctor_Sacrifice) && !Player.IsIncapacitated &&
                Player.PrimaryResource >= 49 && (TargetUtil.AnyElitesInRange(20, 2) || !TargetUtil.AnyElitesInRange(20, 5) ||
                 (CurrentTarget != null && ((CurrentTarget.IsEliteRareUnique || CurrentTarget.IsTreasureGoblin) && CurrentTarget.RadiusDistance <= 30f)) || PlayerOwnedZombieDog <= 2))
            {
                return new TrinityPower(SNOPower.Witchdoctor_SummonZombieDog, 0f, Vector3.Zero, CurrentWorldDynamicId, -1, 0, 0, WAIT_FOR_ANIM);
            }

            // Zombie Dogs for Sacrifice
            if (CombatBase.CanCast(SNOPower.Witchdoctor_SummonZombieDog) && Hotbar.Contains(SNOPower.Witchdoctor_Sacrifice) &&
                Player.PrimaryResource >= 49 &&
                (LastPowerUsed == SNOPower.Witchdoctor_Sacrifice || PlayerOwnedZombieDog <= 2) &&
                //((TimeSinceUse(SNOPower.Witchdoctor_SummonZombieDog) > 1000 && TimeSinceUse(SNOPower.Witchdoctor_Sacrifice) < 1000) || TimeSinceUse(SNOPower.Witchdoctor_SummonZombieDog) > 1800) &&
                CombatBase.LastPowerUsed != SNOPower.Witchdoctor_SummonZombieDog)
            {
                return new TrinityPower(SNOPower.Witchdoctor_SummonZombieDog, 0f, Vector3.Zero, CurrentWorldDynamicId, -1, 2, 2, WAIT_FOR_ANIM);
            }

            // Hex with angry chicken, check if we want to shape shift and explode
            if (!UseOOCBuff && !IsCurrentlyAvoiding && (TargetUtil.AnyMobsInRange(12f, 1, false) || CurrentTarget.RadiusDistance <= 10f) &&
                CombatBase.CanCast(SNOPower.Witchdoctor_Hex) && hasAngryChicken && Player.PrimaryResource >= 49)
            {
                ShouldRefreshHotbarAbilities = true;
                return new TrinityPower(SNOPower.Witchdoctor_Hex);
            }

            // Hex Spam Cast without angry chicken
            if (!UseOOCBuff && !IsCurrentlyAvoiding && CombatBase.CanCast(SNOPower.Witchdoctor_Hex) && !Player.IsIncapacitated && Player.PrimaryResource >= 49 && !hasAngryChicken &&
               (TargetUtil.AnyElitesInRange(12) || TargetUtil.AnyMobsInRange(12, 2) || TargetUtil.IsEliteTargetInRange(18f)))
            {
                return new TrinityPower(SNOPower.Witchdoctor_Hex);
            }
            // Mass Confuse, elites only or big mobs or to escape on low health
            if (!UseOOCBuff && CombatBase.CanCast(SNOPower.Witchdoctor_MassConfusion) && !Player.IsIncapacitated && Player.PrimaryResource >= 74 &&
                (TargetUtil.AnyElitesInRange(12, 1) || TargetUtil.AnyMobsInRange(12, 6) || Player.CurrentHealthPct <= 0.25 || (CurrentTarget.IsBossOrEliteRareUnique && CurrentTarget.RadiusDistance <= 12f)) &&
                !CurrentTarget.IsTreasureGoblin)
            {
                return new TrinityPower(SNOPower.Witchdoctor_MassConfusion, 0f, Vector3.Zero, -1, CurrentTarget.ACDGuid, 1, 1, WAIT_FOR_ANIM);
            }
            // Big Bad Voodoo, elites and bosses only
            if (!UseOOCBuff && CombatBase.CanCast(SNOPower.Witchdoctor_BigBadVoodoo) && !Player.IsIncapacitated && (TargetUtil.EliteOrTrashInRange(25f) || (CurrentTarget.IsBoss && CurrentTarget.CentreDistance <= 30f)))
            {
                return new TrinityPower(SNOPower.Witchdoctor_BigBadVoodoo);
            }

            // Grasp of the Dead, look below, droping globes and dogs when using it on elites and 3 norms
            if (!UseOOCBuff && !IsCurrentlyAvoiding && CombatBase.CanCast(SNOPower.Witchdoctor_GraspOfTheDead) && !Player.IsIncapacitated &&
                (TargetUtil.AnyMobsInRange(30, 2) || TargetUtil.EliteOrTrashInRange(30f)) &&
                Player.PrimaryResource >= 78)
            {
                var bestClusterPoint = TargetUtil.GetBestClusterPoint(15);

                return new TrinityPower(SNOPower.Witchdoctor_GraspOfTheDead, 25f, bestClusterPoint);
            }

            // Piranhas
            if (!UseOOCBuff && !IsCurrentlyAvoiding && CombatBase.CanCast(SNOPower.Witchdoctor_Piranhas) && !Player.IsIncapacitated &&
                (TargetUtil.AnyMobsInRange(30, 2) || TargetUtil.ClusterExists(15f, 45f, 2, true) || TargetUtil.AnyElitesInRange(45f)) &&
                Player.PrimaryResource >= 250)
            {
                var bestClusterPoint = TargetUtil.GetBestClusterPoint(15f);

                return new TrinityPower(SNOPower.Witchdoctor_Piranhas, 25f, bestClusterPoint);
            }

            //skillDict.Add("Horrify", SNOPower.Witchdoctor_Horrify);
            //runeDict.Add("Phobia", 2);
            //runeDict.Add("Stalker", 4);
            //runeDict.Add("FaceOfDeath", 1);
            //runeDict.Add("FrighteningAspect", 0);
            //runeDict.Add("RuthlessTerror", 3);

            bool hasPhobia = HotbarSkills.AssignedSkills.Any(s => s.Power == SNOPower.Witchdoctor_Horrify && s.RuneIndex == 2);
            bool hasStalker = HotbarSkills.AssignedSkills.Any(s => s.Power == SNOPower.Witchdoctor_Horrify && s.RuneIndex == 4);
            bool hasFaceOfDeath = HotbarSkills.AssignedSkills.Any(s => s.Power == SNOPower.Witchdoctor_Horrify && s.RuneIndex == 1);
            bool hasFrighteningAspect = HotbarSkills.AssignedSkills.Any(s => s.Power == SNOPower.Witchdoctor_Horrify && s.RuneIndex == 0);
            bool hasRuthlessTerror = HotbarSkills.AssignedSkills.Any(s => s.Power == SNOPower.Witchdoctor_Horrify && s.RuneIndex == 3);

            float horrifyRadius = hasFaceOfDeath ? 24f : 12f;

            // Horrify 
            if (UseOOCBuff && CombatBase.CanCast(SNOPower.Witchdoctor_Horrify) && !Player.IsIncapacitated && Player.PrimaryResource >= 37 &&
                !hasStalker && !hasFrighteningAspect && TargetUtil.AnyMobsInRange(horrifyRadius, 3))
            {
                return new TrinityPower(SNOPower.Witchdoctor_Horrify, 0f, Vector3.Zero, CurrentWorldDynamicId, -1, 0, 2, WAIT_FOR_ANIM);
            }

            // Horrify Buff When not in combat for movement speed -- Stalker
            if (UseOOCBuff && CombatBase.CanCast(SNOPower.Witchdoctor_Horrify) && !Player.IsIncapacitated && Player.PrimaryResource >= 37 &&
                hasStalker)
            {
                return new TrinityPower(SNOPower.Witchdoctor_Horrify, 0f, Vector3.Zero, CurrentWorldDynamicId, -1, 0, 2, WAIT_FOR_ANIM);
            }
            // Horrify Buff at 35% health -- Freightening Aspect
            if (!UseOOCBuff && CombatBase.CanCast(SNOPower.Witchdoctor_Horrify) && !Player.IsIncapacitated && Player.PrimaryResource >= 37 &&
                Player.CurrentHealthPct <= 0.35 && hasFrighteningAspect)
            {
                return new TrinityPower(SNOPower.Witchdoctor_Horrify, 0f, Vector3.Zero, CurrentWorldDynamicId, -1, 0, 2, WAIT_FOR_ANIM);
            }
            // Fetish Army, elites only
            if (!UseOOCBuff && CombatBase.CanCast(SNOPower.Witchdoctor_FetishArmy) && !Player.IsIncapacitated &&
                (TargetUtil.AnyElitesInRange(25, 1) || ((CurrentTarget.IsEliteRareUnique || CurrentTarget.IsTreasureGoblin || CurrentTarget.IsBoss) && CurrentTarget.RadiusDistance <= 16f)))
            {
                return new TrinityPower(SNOPower.Witchdoctor_FetishArmy, 0f, Vector3.Zero, CurrentWorldDynamicId, -1, 1, 1, WAIT_FOR_ANIM);
            }

            //skillDict.Add("SpiritBarage", SNOPower.Witchdoctor_SpiritBarrage);
            //runeDict.Add("TheSpiritIsWilling", 3);
            //runeDict.Add("WellOfSouls", 1);
            //runeDict.Add("Phantasm", 2);
            //runeDict.Add("Phlebotomize", 0);
            //runeDict.Add("Manitou", 4);

            bool hasManitou = HotbarSkills.AssignedSkills.Any(s => s.Power == SNOPower.Witchdoctor_SpiritBarrage && s.RuneIndex == 4);

            // Spirit Barrage Manitou
            if (!UseOOCBuff && CombatBase.CanCast(SNOPower.Witchdoctor_SpiritBarrage) && Player.PrimaryResource >= 100 && TimeSinceUse(SNOPower.Witchdoctor_SpiritBarrage) > 18000 && hasManitou)
            {
                return new TrinityPower(SNOPower.Witchdoctor_SpiritBarrage, 0f, Vector3.Zero, CurrentWorldDynamicId, -1, 2, 2, WAIT_FOR_ANIM);
            }

            //skillDict.Add("Haunt", SNOPower.Witchdoctor_Haunt);
            //runeDict.Add("ConsumingSpirit", 0);
            //runeDict.Add("ResentfulSpirit", 4);
            //runeDict.Add("LingeringSpirit", 1);
            //runeDict.Add("GraspingSpirit", 2);
            //runeDict.Add("DrainingSpirit", 3);

            bool hasResentfulSpirit = HotbarSkills.AssignedSkills.Any(s => s.Power == SNOPower.Witchdoctor_Haunt && s.RuneIndex == 4);

            // Haunt 
            if (!UseOOCBuff && !IsCurrentlyAvoiding && CombatBase.CanCast(SNOPower.Witchdoctor_Haunt) &&
                !Player.IsIncapacitated && Player.PrimaryResource >= 50 &&
                !SpellTracker.IsUnitTracked(CurrentTarget, SNOPower.Witchdoctor_Haunt))
            {
                return new TrinityPower(SNOPower.Witchdoctor_Haunt, 21f, CurrentTarget.ACDGuid);
            }

            //skillDict.Add("LocustSwarm", SNOPower.Witchdoctor_Locust_Swarm);

            // Locust Swarm
            if (!UseOOCBuff && !IsCurrentlyAvoiding && CombatBase.CanCast(SNOPower.Witchdoctor_Locust_Swarm) && !Player.IsIncapacitated && Player.PrimaryResource >= 300 &&
                !SpellTracker.IsUnitTracked(CurrentTarget, SNOPower.Witchdoctor_Locust_Swarm) && LastPowerUsed != SNOPower.Witchdoctor_Locust_Swarm)
            {
                return new TrinityPower(SNOPower.Witchdoctor_Locust_Swarm, 12f, CurrentTarget.ACDGuid);
            }

            // Sacrifice for 0 Dogs
            if (!UseOOCBuff && CombatBase.CanCast(SNOPower.Witchdoctor_Sacrifice, CombatBase.CanCastFlags.NoTimer) &&
                (Settings.Combat.WitchDoctor.ZeroDogs || !WitchDoctorHasPrimaryAttack))
            {
                return new TrinityPower(SNOPower.Witchdoctor_Sacrifice, 9f);
            }

            // Wall of Zombies
            if (!UseOOCBuff && !IsCurrentlyAvoiding && !Player.IsIncapacitated && CombatBase.CanCast(SNOPower.Witchdoctor_WallOfZombies) && !Player.IsIncapacitated &&
                (TargetUtil.AnyElitesInRange(15, 1) || TargetUtil.AnyMobsInRange(15, 4) || ((CurrentTarget.IsEliteRareUnique || CurrentTarget.IsTreasureGoblin || CurrentTarget.IsBoss) && CurrentTarget.RadiusDistance <= 25f)) &&
                Player.PrimaryResource >= 103)
            {
                return new TrinityPower(SNOPower.Witchdoctor_WallOfZombies, 25f, CurrentTarget.Position, CurrentWorldDynamicId, -1, 1, 1, WAIT_FOR_ANIM);
            }

            var zombieChargerRange = hasGraveInjustice ? Math.Min(Player.GoldPickupRadius + 8f, 11f) : 11f;

            // Zombie Charger aka Zombie bears Spams Bears @ Everything from 11feet away
            if (!UseOOCBuff && !IsCurrentlyAvoiding && !Player.IsIncapacitated && CombatBase.CanCast(SNOPower.Witchdoctor_ZombieCharger) && Player.PrimaryResource >= 150)
            {
                return new TrinityPower(SNOPower.Witchdoctor_ZombieCharger, zombieChargerRange, CurrentTarget.Position);
            }

            //skillDict.Add("Firebats", SNOPower.Witchdoctor_Firebats);
            //runeDict.Add("DireBats", 0);
            //runeDict.Add("VampireBats", 3);
            //runeDict.Add("PlagueBats", 2);
            //runeDict.Add("HungryBats", 1);
            //runeDict.Add("CloudOfBats", 4);

            bool hasDireBats = HotbarSkills.AssignedSkills.Any(s => s.Power == SNOPower.Witchdoctor_Firebats && s.RuneIndex == 0);
            bool hasVampireBats = HotbarSkills.AssignedSkills.Any(s => s.Power == SNOPower.Witchdoctor_Firebats && s.RuneIndex == 3);
            bool hasPlagueBats = HotbarSkills.AssignedSkills.Any(s => s.Power == SNOPower.Witchdoctor_Firebats && s.RuneIndex == 2);
            bool hasHungryBats = HotbarSkills.AssignedSkills.Any(s => s.Power == SNOPower.Witchdoctor_Firebats && s.RuneIndex == 1);
            bool hasCloudOfBats = HotbarSkills.AssignedSkills.Any(s => s.Power == SNOPower.Witchdoctor_Firebats && s.RuneIndex == 4);

            int fireBatsMana = TimeSinceUse(SNOPower.Witchdoctor_Firebats) < 125 ? 66 : 220;

            bool firebatsMaintain =
              ObjectCache.Any(u => u.IsUnit &&
                  u.IsPlayerFacing(70f) && u.Weight > 0 &&
                  u.CentreDistance <= V.F("WitchDoctor.Firebats.MaintainRange") &&
                  SpellHistory.TimeSinceUse(SNOPower.Witchdoctor_Firebats) <= TimeSpan.FromMilliseconds(250d));

            // Fire Bats:Cloud of bats 
            if (!UseOOCBuff && !IsCurrentlyAvoiding && !Player.IsIncapacitated && hasCloudOfBats && (TargetUtil.AnyMobsInRange(8f) || firebatsMaintain) &&
                CombatBase.CanCast(SNOPower.Witchdoctor_Firebats) && Player.PrimaryResource >= fireBatsMana)
            {
                return new TrinityPower(SNOPower.Witchdoctor_Firebats, Settings.Combat.WitchDoctor.FirebatsRange, Vector3.Zero, -1, CurrentTarget.ACDGuid, 0, 0, NO_WAIT_ANIM);
            }

            // Fire Bats fast-attack
            if (!UseOOCBuff && !IsCurrentlyAvoiding && !Player.IsIncapacitated && CombatBase.CanCast(SNOPower.Witchdoctor_Firebats) && Player.PrimaryResource >= fireBatsMana &&
                 (TargetUtil.AnyMobsInRange(Settings.Combat.WitchDoctor.FirebatsRange) || firebatsMaintain) && !hasCloudOfBats)
            {
                float range = firebatsMaintain ? Settings.Combat.WitchDoctor.FirebatsRange : V.F("WitchDoctor.Firebats.MaintainRange");
                return new TrinityPower(SNOPower.Witchdoctor_Firebats, Settings.Combat.WitchDoctor.FirebatsRange, CurrentTarget.Position, -1, CurrentTarget.ACDGuid, 0, 0, NO_WAIT_ANIM);
            }

            // Acid Cloud
            if (!UseOOCBuff && !IsCurrentlyAvoiding && !Player.IsIncapacitated && CombatBase.CanCast(SNOPower.Witchdoctor_AcidCloud) && Player.PrimaryResource >= 172)
            {
                Vector3 bestClusterPoint;
                if (hasGraveInjustice)
                    bestClusterPoint = TargetUtil.GetBestClusterPoint(15f, Math.Min(Player.GoldPickupRadius + 8f, 30f));
                else
                    bestClusterPoint = TargetUtil.GetBestClusterPoint(15f, 30f);

                return new TrinityPower(SNOPower.Witchdoctor_AcidCloud, rangedAttackMaxRange, bestClusterPoint, CurrentWorldDynamicId, -1, 1, 1, WAIT_FOR_ANIM);
            }

            bool hasWellOfSouls = HotbarSkills.AssignedSkills.Any(s => s.Power == SNOPower.Witchdoctor_SpiritBarrage && s.RuneIndex == 1);
            bool hasRushOfEssence = ZetaDia.CPlayer.PassiveSkills.Any(s => s == SNOPower.Witchdoctor_Passive_RushOfEssence);

            // Spirit Barrage + Rush of Essence
            if (!UseOOCBuff && !IsCurrentlyAvoiding && !Player.IsIncapacitated && CombatBase.CanCast(SNOPower.Witchdoctor_SpiritBarrage) && Player.PrimaryResource >= 108 &&
                hasRushOfEssence && Player.PrimaryResourcePct <= 0.25 && !hasManitou)
            {
                if (hasWellOfSouls)
                    return new TrinityPower(SNOPower.Witchdoctor_SpiritBarrage, 2, 2);

                return new TrinityPower(SNOPower.Witchdoctor_SpiritBarrage, 21f, Vector3.Zero, -1, CurrentTarget.ACDGuid, 2, 2, WAIT_FOR_ANIM);
            }

            // Zombie Charger backup
            if (!UseOOCBuff && !IsCurrentlyAvoiding && !Player.IsIncapacitated && CombatBase.CanCast(SNOPower.Witchdoctor_ZombieCharger) && Player.PrimaryResource >= 140)
            {
                return new TrinityPower(SNOPower.Witchdoctor_ZombieCharger, zombieChargerRange, CurrentTarget.Position, CurrentWorldDynamicId, -1, 0, 0, WAIT_FOR_ANIM);
            }

            // Regular spirit barage
            if (!UseOOCBuff && CombatBase.CanCast(SNOPower.Witchdoctor_SpiritBarrage) && Player.PrimaryResource >= 100 && !hasManitou)
            {
                return new TrinityPower(SNOPower.Witchdoctor_SpiritBarrage, 12f, CurrentTarget.ACDGuid);
            }

            // Poison Darts fast-attack Spams Darts when mana is too low (to cast bears) @12yds or @10yds if Bears avialable
            if (!UseOOCBuff && !IsCurrentlyAvoiding && !Player.IsIncapacitated && CombatBase.CanCast(SNOPower.Witchdoctor_PoisonDart))
            {
                WitchDoctorCombat.VisionQuestRefreshTimer.Restart();
                return new TrinityPower(SNOPower.Witchdoctor_PoisonDart, basicAttackRange, Vector3.Zero, -1, CurrentTarget.ACDGuid, 0, 2, WAIT_FOR_ANIM);
            }
            // Corpse Spiders fast-attacks Spams Spiders when mana is too low (to cast bears) @12yds or @10yds if Bears avialable
            if (!UseOOCBuff && !IsCurrentlyAvoiding && !Player.IsIncapacitated && CombatBase.CanCast(SNOPower.Witchdoctor_CorpseSpider))
            {
                WitchDoctorCombat.VisionQuestRefreshTimer.Restart();
                return new TrinityPower(SNOPower.Witchdoctor_CorpseSpider, basicAttackRange, Vector3.Zero, -1, CurrentTarget.ACDGuid, 0, 1, WAIT_FOR_ANIM);
            }
            // Toads fast-attacks Spams Toads when mana is too low (to cast bears) @12yds or @10yds if Bears avialable
            if (!UseOOCBuff && !IsCurrentlyAvoiding && !Player.IsIncapacitated && CombatBase.CanCast(SNOPower.Witchdoctor_PlagueOfToads))
            {
                WitchDoctorCombat.VisionQuestRefreshTimer.Restart();
                return new TrinityPower(SNOPower.Witchdoctor_PlagueOfToads, basicAttackRange, Vector3.Zero, -1, CurrentTarget.ACDGuid, 0, 1, WAIT_FOR_ANIM);
            }
            // Fire Bomb fast-attacks Spams Bomb when mana is too low (to cast bears) @12yds or @10yds if Bears avialable
            if (!UseOOCBuff && !IsCurrentlyAvoiding && !Player.IsIncapacitated && CombatBase.CanCast(SNOPower.Witchdoctor_Firebomb))
            {
                WitchDoctorCombat.VisionQuestRefreshTimer.Restart();
                return new TrinityPower(SNOPower.Witchdoctor_Firebomb, basicAttackRange, Vector3.Zero, -1, CurrentTarget.ACDGuid, 0, 1, WAIT_FOR_ANIM);
            }

            // Default attacks
            return CombatBase.DefaultPower;
        }

        private static bool WitchDoctorHasPrimaryAttack
        {
            get
            {
                return
                    Hotbar.Contains(SNOPower.Witchdoctor_WallOfZombies) ||
                    Hotbar.Contains(SNOPower.Witchdoctor_Firebats) ||
                    Hotbar.Contains(SNOPower.Witchdoctor_AcidCloud) ||
                    Hotbar.Contains(SNOPower.Witchdoctor_ZombieCharger) ||
                    Hotbar.Contains(SNOPower.Witchdoctor_PoisonDart) ||
                    Hotbar.Contains(SNOPower.Witchdoctor_CorpseSpider) ||
                    Hotbar.Contains(SNOPower.Witchdoctor_PlagueOfToads) ||
                    Hotbar.Contains(SNOPower.Witchdoctor_Firebomb);
            }
        }

        private static TrinityPower GetWitchDoctorDestroyPower()
        {
            if (Hotbar.Contains(SNOPower.Witchdoctor_Firebomb))
                return new TrinityPower(SNOPower.Witchdoctor_Firebomb, 12f, Vector3.Zero, -1, -1, 0, 0, WAIT_FOR_ANIM);
            if (Hotbar.Contains(SNOPower.Witchdoctor_PoisonDart))
                return new TrinityPower(SNOPower.Witchdoctor_PoisonDart, 15f, Vector3.Zero, -1, -1, 0, 0, WAIT_FOR_ANIM);
            if (Hotbar.Contains(SNOPower.Witchdoctor_ZombieCharger) && Player.PrimaryResource >= 140)
                return new TrinityPower(SNOPower.Witchdoctor_ZombieCharger, 12f, Vector3.Zero, -1, -1, 0, 0, WAIT_FOR_ANIM);
            if (Hotbar.Contains(SNOPower.Witchdoctor_CorpseSpider))
                return new TrinityPower(SNOPower.Witchdoctor_CorpseSpider, 12f, Vector3.Zero, -1, -1, 0, 0, WAIT_FOR_ANIM);
            if (Hotbar.Contains(SNOPower.Witchdoctor_PlagueOfToads))
                return new TrinityPower(SNOPower.Witchdoctor_PlagueOfToads, 12f, Vector3.Zero, -1, -1, 0, 0, WAIT_FOR_ANIM);
            if (Hotbar.Contains(SNOPower.Witchdoctor_AcidCloud) && Player.PrimaryResource >= 172)
                return new TrinityPower(SNOPower.Witchdoctor_AcidCloud, 12f, Vector3.Zero, -1, -1, 0, 0, WAIT_FOR_ANIM);
            if (Hotbar.Contains(SNOPower.Witchdoctor_Sacrifice) && Hotbar.Contains(SNOPower.Witchdoctor_SummonZombieDog) && PlayerOwnedZombieDog > 0 && Settings.Combat.WitchDoctor.ZeroDogs)
                return new TrinityPower(SNOPower.Witchdoctor_Sacrifice, 12f, Vector3.Zero, -1, -1, 1, 2, WAIT_FOR_ANIM);

            if (Hotbar.Contains(SNOPower.Witchdoctor_SpiritBarrage) && Player.PrimaryResource > 100)
                return new TrinityPower(SNOPower.Witchdoctor_SpiritBarrage, 12f, CurrentTarget.ACDGuid);
            return CombatBase.DefaultPower;
        }

    }
}
