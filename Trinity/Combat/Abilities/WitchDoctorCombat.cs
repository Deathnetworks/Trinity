
using System;
using System.Collections.Generic;
using System.Linq;
using Zeta.Common;
using Zeta.Game;
using Zeta.Game.Internals.Actors;

namespace Trinity.Combat.Abilities
{
    public class WitchDoctorCombat : CombatBase
    {
        public static System.Diagnostics.Stopwatch VisionQuestRefreshTimer = new System.Diagnostics.Stopwatch();
        public static long GetTimeSinceLastVisionQuestRefresh()
        {
            if (!VisionQuestRefreshTimer.IsRunning)
                VisionQuestRefreshTimer.Start();

            return VisionQuestRefreshTimer.ElapsedMilliseconds;
        }


        public static TrinityPower GetPower()
        {
            TrinityPower power = null;


            // Spirit Walk, always!
            if (CanCast(SNOPower.Witchdoctor_SpiritWalk))
            {
                return new TrinityPower(SNOPower.Witchdoctor_SpiritWalk);
            }


            // Combat Avoidance Spells
            if (!UseOOCBuff && IsCurrentlyAvoiding)
            {
            }

            // Incapacitated or Rooted
            if (!UseOOCBuff && (Player.IsIncapacitated || Player.IsRooted))
            {
                // Spirit Walk
                if (CanCast(SNOPower.Witchdoctor_SpiritWalk))
                {
                    return new TrinityPower(SNOPower.Witchdoctor_SpiritWalk);
                }
            }

            // Combat Spells with a Target
            if (!UseOOCBuff && !IsCurrentlyAvoiding && CurrentTarget != null)
            {
                bool hasGraveInjustice = ZetaDia.CPlayer.PassiveSkills.Contains(SNOPower.Witchdoctor_Passive_GraveInjustice);

                bool hasAngryChicken = HotbarSkills.AssignedSkills.Any(s => s.Power == SNOPower.Witchdoctor_Hex && s.RuneIndex == 1);
                bool isChicken = hasAngryChicken && Player.IsHidden;

                bool hasVisionQuest = ZetaDia.CPlayer.PassiveSkills.Any(s => s == SNOPower.Witchdoctor_Passive_VisionQuest);

                // Set max ranged attack range, based on Grave Injustice, and current target NOT standing in avoidance, and health > 25%
                float rangedAttackMaxRange = 30f;
                if (hasGraveInjustice && !CurrentTarget.IsStandingInAvoidance && Player.CurrentHealthPct > 0.25)
                    rangedAttackMaxRange = Math.Min(Player.GoldPickupRadius + 8f, 30f);

                // Set basic attack range, depending on whether or not we have Bears
                float basicAttackRange = 35f;
                if (hasGraveInjustice)
                    basicAttackRange = rangedAttackMaxRange;
                else if (Hotbar.Contains(SNOPower.Witchdoctor_ZombieCharger) && Player.PrimaryResource >= 150)
                    basicAttackRange = 30f;


                // Hex with angry chicken, is chicken, explode!
                if (isChicken && (TargetUtil.AnyMobsInRange(12f, 1, false) || CurrentTarget.RadiusDistance <= 10f || UseDestructiblePower) &&
                    CanCast(SNOPower.Witchdoctor_Hex_Explode))
                {
                    Trinity.ShouldRefreshHotbarAbilities = true;
                    return new TrinityPower(SNOPower.Witchdoctor_Hex_Explode);
                }
                else if (hasAngryChicken)
                {
                    Trinity.ShouldRefreshHotbarAbilities = true;
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

                // Spirit Walk for Goblins chasing
                if (CanCast(SNOPower.Witchdoctor_SpiritWalk) &&
                    CurrentTarget.IsTreasureGoblin && CurrentTarget.HitPointsPct < 0.90 && CurrentTarget.RadiusDistance <= 40f)
                {
                    return new TrinityPower(SNOPower.Witchdoctor_SpiritWalk);
                }

                // Spirit Walk < 65% Health: Healing Journey
                if (CanCast(SNOPower.Witchdoctor_SpiritWalk) && hasHealingJourney &&
                    Player.CurrentHealthPct <= V.F("WitchDoctor.SpiritWalk.HealingJourneyHealth"))
                {
                    return new TrinityPower(SNOPower.Witchdoctor_SpiritWalk);
                }

                // Spirit Walk < 50% Mana: Honored Guest
                if (CanCast(SNOPower.Witchdoctor_SpiritWalk) && hasHonoredGuest &&
                    Player.PrimaryResourcePct <= V.F("WitchDoctor.SpiritWalk.HonoredGuestMana"))
                {
                    return new TrinityPower(SNOPower.Witchdoctor_SpiritWalk);
                }

                //bool shouldRefreshVisionQuest = WitchDoctorCombat.GetTimeSinceLastVisionQuestRefresh() > 4000;
                bool shouldRefreshVisionQuest = !GetHasBuff(SNOPower.Witchdoctor_Passive_VisionQuest) || GetTimeSinceLastVisionQuestRefresh() > 3800;

                // Vision Quest Passive
                if (hasVisionQuest && shouldRefreshVisionQuest)
                {
                    // Poison Darts 
                    if (CanCast(SNOPower.Witchdoctor_PoisonDart))
                    {
                        WitchDoctorCombat.VisionQuestRefreshTimer.Restart();
                        return new TrinityPower(SNOPower.Witchdoctor_PoisonDart, basicAttackRange, CurrentTarget.ACDGuid);
                    }
                    // Corpse Spiders
                    if (CanCast(SNOPower.Witchdoctor_CorpseSpider))
                    {
                        WitchDoctorCombat.VisionQuestRefreshTimer.Restart();
                        return new TrinityPower(SNOPower.Witchdoctor_CorpseSpider, basicAttackRange, CurrentTarget.ACDGuid);
                    }
                    // Plague Of Toads 
                    if (CanCast(SNOPower.Witchdoctor_PlagueOfToads))
                    {
                        WitchDoctorCombat.VisionQuestRefreshTimer.Restart();
                        return new TrinityPower(SNOPower.Witchdoctor_PlagueOfToads, basicAttackRange, CurrentTarget.ACDGuid);
                    }
                    // Fire Bomb 
                    if (CanCast(SNOPower.Witchdoctor_Firebomb))
                    {
                        WitchDoctorCombat.VisionQuestRefreshTimer.Restart();
                        return new TrinityPower(SNOPower.Witchdoctor_Firebomb, basicAttackRange, CurrentTarget.ACDGuid); ;
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

                // Soul Harvest Any Elites or to increase buff stacks
                if (CanCast(SNOPower.Witchdoctor_SoulHarvest) &&
                    (TargetUtil.AnyMobsInRange(16f, GetBuffStacks(SNOPower.Witchdoctor_SoulHarvest) + 1, false) || (hasSwallowYourSoul && Player.PrimaryResourcePct <= 0.50) || TargetUtil.IsEliteTargetInRange(16f)))
                {
                    return new TrinityPower(SNOPower.Witchdoctor_SoulHarvest);
                }

                // Soul Harvest with VengefulSpirit
                if (CanCast(SNOPower.Witchdoctor_SoulHarvest) && hasVengefulSpirit &&
                    TargetUtil.AnyMobsInRange(16, 3))
                {
                    return new TrinityPower(SNOPower.Witchdoctor_SoulHarvest);
                }

                // Sacrifice
                if (CanCast(SNOPower.Witchdoctor_Sacrifice) && Trinity.PlayerOwnedZombieDog > 0 &&
                    (TargetUtil.AnyElitesInRange(15, 1) || (CurrentTarget.IsBossOrEliteRareUnique && CurrentTarget.RadiusDistance <= 9f)))
                {
                    return new TrinityPower(SNOPower.Witchdoctor_Sacrifice);
                }

                // Sacrifice for Circle of Life
                bool hasCircleofLife = ZetaDia.CPlayer.PassiveSkills.Any(s => s == SNOPower.Witchdoctor_Passive_CircleOfLife);
                if (CanCast(SNOPower.Witchdoctor_Sacrifice) && Trinity.PlayerOwnedZombieDog > 0 && hasCircleofLife && TargetUtil.AnyMobsInRange(15f))
                {
                    return new TrinityPower(SNOPower.Witchdoctor_Sacrifice);
                }

                bool hasRestlessGiant = HotbarSkills.AssignedSkills.Any(s => s.Power == SNOPower.Witchdoctor_Gargantuan && s.RuneIndex == 0);
                bool hasWrathfulProtector = HotbarSkills.AssignedSkills.Any(s => s.Power == SNOPower.Witchdoctor_Gargantuan && s.RuneIndex == 3);

                if (CanCast(SNOPower.Witchdoctor_Gargantuan))
                {
                    // Gargantuan, Recast on Elites or Bosses to trigger Restless Giant
                    if (hasRestlessGiant && (TargetUtil.IsEliteTargetInRange(30f) || Trinity.PlayerOwnedGargantuan == 0))
                    {
                        return new TrinityPower(SNOPower.Witchdoctor_Gargantuan);
                    }

                    // Gargantuan Wrathful Protector, 15 seconds of smash, use sparingly!
                    if (hasWrathfulProtector && TargetUtil.IsEliteTargetInRange(30f))
                    {
                        return new TrinityPower(SNOPower.Witchdoctor_Gargantuan);
                    }

                    // Gargantuan regular
                    if (!hasRestlessGiant && !hasWrathfulProtector && Trinity.PlayerOwnedGargantuan == 0)
                    {
                        return new TrinityPower(SNOPower.Witchdoctor_Gargantuan);
                    }
                }

                bool hasSacrifice = Hotbar.Contains(SNOPower.Witchdoctor_Sacrifice);

                // Zombie Dogs non-sacrifice build
                if (!hasSacrifice && CanCast(SNOPower.Witchdoctor_SummonZombieDog) && Trinity.PlayerOwnedZombieDog <= 2)
                {
                    return new TrinityPower(SNOPower.Witchdoctor_SummonZombieDog);
                }

                // Zombie Dogs for Sacrifice
                if (hasSacrifice && CanCast(SNOPower.Witchdoctor_SummonZombieDog) &&
                    (LastPowerUsed == SNOPower.Witchdoctor_Sacrifice || Trinity.PlayerOwnedZombieDog <= 2) &&
                    CombatBase.LastPowerUsed != SNOPower.Witchdoctor_SummonZombieDog)
                {
                    return new TrinityPower(SNOPower.Witchdoctor_SummonZombieDog);
                }

                // Hex with angry chicken, check if we want to shape shift and explode
                if (CanCast(SNOPower.Witchdoctor_Hex) && (TargetUtil.AnyMobsInRange(12f, 1, false) || CurrentTarget.RadiusDistance <= 10f) &&
                    hasAngryChicken)
                {
                    Trinity.ShouldRefreshHotbarAbilities = true;
                    return new TrinityPower(SNOPower.Witchdoctor_Hex);
                }

                // Hex Spam Cast without angry chicken
                if (CanCast(SNOPower.Witchdoctor_Hex) && !hasAngryChicken &&
                   (TargetUtil.AnyElitesInRange(12) || TargetUtil.AnyMobsInRange(12, 2) || TargetUtil.IsEliteTargetInRange(18f)))
                {
                    return new TrinityPower(SNOPower.Witchdoctor_Hex);
                }
                // Mass Confuse, elites only or big mobs or to escape on low health
                if (CanCast(SNOPower.Witchdoctor_MassConfusion) &&
                    (TargetUtil.AnyElitesInRange(12, 1) || TargetUtil.AnyMobsInRange(12, 6) || Player.CurrentHealthPct <= 0.25 || (CurrentTarget.IsBossOrEliteRareUnique && CurrentTarget.RadiusDistance <= 12f)) &&
                    !CurrentTarget.IsTreasureGoblin)
                {
                    return new TrinityPower(SNOPower.Witchdoctor_MassConfusion, 0f, CurrentTarget.ACDGuid);
                }
                // Big Bad Voodoo, elites and bosses only
                if (CanCast(SNOPower.Witchdoctor_BigBadVoodoo) &&
                    (TargetUtil.EliteOrTrashInRange(25f) || (CurrentTarget.IsBoss && CurrentTarget.Distance <= 30f)))
                {
                    return new TrinityPower(SNOPower.Witchdoctor_BigBadVoodoo);
                }

                // Grasp of the Dead
                if (CanCast(SNOPower.Witchdoctor_GraspOfTheDead) &&
                    (TargetUtil.AnyMobsInRange(30, 2) || TargetUtil.EliteOrTrashInRange(30f)) &&
                    Player.PrimaryResource >= 150)
                {
                    var bestClusterPoint = TargetUtil.GetBestClusterPoint(15);

                    return new TrinityPower(SNOPower.Witchdoctor_GraspOfTheDead, 25f, bestClusterPoint);
                }

                // Piranhas
                if (CanCast(SNOPower.Witchdoctor_Piranhas) && Player.PrimaryResource >= 250 &&
                    (TargetUtil.ClusterExists(15f, 45f, 2, true) || TargetUtil.AnyElitesInRange(45f)) &&
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

                // Horrify when low on health
                if (CanCast(SNOPower.Witchdoctor_Horrify) && Player.CurrentHealthPct <= Trinity.PlayerEmergencyHealthPotionLimit && TargetUtil.AnyMobsInRange(horrifyRadius, 3))
                {
                    return new TrinityPower(SNOPower.Witchdoctor_Horrify);
                }

                // Horrify Buff at 35% health -- Freightening Aspect
                if (CanCast(SNOPower.Witchdoctor_Horrify) && Player.CurrentHealthPct <= 0.35 && hasFrighteningAspect)
                {
                    return new TrinityPower(SNOPower.Witchdoctor_Horrify);
                }

                // Fetish Army, elites only
                if (CanCast(SNOPower.Witchdoctor_FetishArmy) &&
                    (TargetUtil.EliteOrTrashInRange(30f) || TargetUtil.IsEliteTargetInRange(30f) || Settings.Combat.WitchDoctor.UseFetishArmyOffCooldown))
                {
                    return new TrinityPower(SNOPower.Witchdoctor_FetishArmy);
                }

                //skillDict.Add("SpiritBarage", SNOPower.Witchdoctor_SpiritBarrage);
                //runeDict.Add("TheSpiritIsWilling", 3);
                //runeDict.Add("WellOfSouls", 1);
                //runeDict.Add("Phantasm", 2);
                //runeDict.Add("Phlebotomize", 0);
                //runeDict.Add("Manitou", 4);

                bool hasManitou = HotbarSkills.AssignedSkills.Any(s => s.Power == SNOPower.Witchdoctor_SpiritBarrage && s.RuneIndex == 4);

                // Spirit Barrage Manitou
                if (CanCast(SNOPower.Witchdoctor_SpiritBarrage) && Player.PrimaryResource >= 100 &&
                    TimeSincePowerUse(SNOPower.Witchdoctor_SpiritBarrage) > 18000 && hasManitou)
                {
                    return new TrinityPower(SNOPower.Witchdoctor_SpiritBarrage);
                }

                //skillDict.Add("Haunt", SNOPower.Witchdoctor_Haunt);
                //runeDict.Add("ConsumingSpirit", 0);
                //runeDict.Add("ResentfulSpirit", 4);
                //runeDict.Add("LingeringSpirit", 1);
                //runeDict.Add("GraspingSpirit", 2);
                //runeDict.Add("DrainingSpirit", 3);

                bool hasResentfulSpirit = HotbarSkills.AssignedSkills.Any(s => s.Power == SNOPower.Witchdoctor_Haunt && s.RuneIndex == 4);

                // Haunt 
                if (CanCast(SNOPower.Witchdoctor_Haunt) &&
                    Player.PrimaryResource >= 50 &&
                    !SpellTracker.IsUnitTracked(CurrentTarget, SNOPower.Witchdoctor_Haunt))
                {
                    return new TrinityPower(SNOPower.Witchdoctor_Haunt, 21f, CurrentTarget.ACDGuid);
                }

                //skillDict.Add("LocustSwarm", SNOPower.Witchdoctor_Locust_Swarm);

                // Locust Swarm
                if (CanCast(SNOPower.Witchdoctor_Locust_Swarm) && Player.PrimaryResource >= 300 &&
                    !SpellTracker.IsUnitTracked(CurrentTarget, SNOPower.Witchdoctor_Locust_Swarm) && LastPowerUsed != SNOPower.Witchdoctor_Locust_Swarm)
                {
                    return new TrinityPower(SNOPower.Witchdoctor_Locust_Swarm, 12f, CurrentTarget.ACDGuid);
                }

                // Sacrifice for 0 Dogs
                if (CanCast(SNOPower.Witchdoctor_Sacrifice) &&
                    (Settings.Combat.WitchDoctor.ZeroDogs || !WitchDoctorHasPrimaryAttack))
                {
                    return new TrinityPower(SNOPower.Witchdoctor_Sacrifice, 9f);
                }

                // Wall of Zombies
                if (CanCast(SNOPower.Witchdoctor_WallOfZombies) &&
                    (TargetUtil.AnyElitesInRange(15, 1) || TargetUtil.AnyMobsInRange(15, 4) ||
                    ((CurrentTarget.IsEliteRareUnique || CurrentTarget.IsTreasureGoblin || CurrentTarget.IsBoss) && CurrentTarget.RadiusDistance <= 25f)))
                {
                    return new TrinityPower(SNOPower.Witchdoctor_WallOfZombies, 25f, CurrentTarget.Position);
                }

                var zombieChargerRange = hasGraveInjustice ? Math.Min(Player.GoldPickupRadius + 8f, 11f) : 11f;

                // Zombie Charger aka Zombie bears Spams Bears @ Everything from 11feet away
                if (CanCast(SNOPower.Witchdoctor_ZombieCharger) && Player.PrimaryResource >= 150)
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

                int fireBatsChannelCost = hasVampireBats ? 0 : 75;
                int fireBatsMana = CombatBase.TimeSincePowerUse(SNOPower.Witchdoctor_Firebats) < 125 ? fireBatsChannelCost : 225;

                bool firebatsMaintain =
                  Trinity.ObjectCache.Any(u => u.IsUnit &&
                      u.IsPlayerFacing(70f) && u.Weight > 0 &&
                      u.Distance <= V.F("WitchDoctor.Firebats.MaintainRange") &&
                      SpellHistory.TimeSinceUse(SNOPower.Witchdoctor_Firebats) <= TimeSpan.FromMilliseconds(250d));

                // Fire Bats:Cloud of bats 
                if (hasCloudOfBats && (TargetUtil.AnyMobsInRange(8f) || firebatsMaintain) &&
                    CanCast(SNOPower.Witchdoctor_Firebats) && Player.PrimaryResource >= fireBatsMana)
                {
                    var range = Settings.Combat.WitchDoctor.FirebatsRange > 12f ? 12f : Settings.Combat.WitchDoctor.FirebatsRange;

                    return new TrinityPower(SNOPower.Witchdoctor_Firebats, range, CurrentTarget.ACDGuid);
                }

                // Fire Bats fast-attack
                if (CanCast(SNOPower.Witchdoctor_Firebats) && Player.PrimaryResource >= fireBatsMana &&
                     (TargetUtil.AnyMobsInRange(Settings.Combat.WitchDoctor.FirebatsRange) || firebatsMaintain) && !hasCloudOfBats)
                {
                    float range = firebatsMaintain ? Settings.Combat.WitchDoctor.FirebatsRange : V.F("WitchDoctor.Firebats.MaintainRange");
                    return new TrinityPower(SNOPower.Witchdoctor_Firebats, Settings.Combat.WitchDoctor.FirebatsRange, CurrentTarget.Position);
                }

                // Acid Cloud
                if (CanCast(SNOPower.Witchdoctor_AcidCloud) && Player.PrimaryResource >= 175)
                {
                    Vector3 bestClusterPoint;
                    if (hasGraveInjustice)
                        bestClusterPoint = TargetUtil.GetBestClusterPoint(15f, Math.Min(Player.GoldPickupRadius + 8f, 30f));
                    else
                        bestClusterPoint = TargetUtil.GetBestClusterPoint(15f, 30f);

                    return new TrinityPower(SNOPower.Witchdoctor_AcidCloud, rangedAttackMaxRange, bestClusterPoint);
                }

                bool hasWellOfSouls = HotbarSkills.AssignedSkills.Any(s => s.Power == SNOPower.Witchdoctor_SpiritBarrage && s.RuneIndex == 1);
                bool hasRushOfEssence = ZetaDia.CPlayer.PassiveSkills.Any(s => s == SNOPower.Witchdoctor_Passive_RushOfEssence);

                // Spirit Barrage + Rush of Essence
                if (CanCast(SNOPower.Witchdoctor_SpiritBarrage) && Player.PrimaryResource >= 100 &&
                    hasRushOfEssence && !hasManitou)
                {
                    if (hasWellOfSouls)
                        return new TrinityPower(SNOPower.Witchdoctor_SpiritBarrage, 2, 2);

                    return new TrinityPower(SNOPower.Witchdoctor_SpiritBarrage, 21f, CurrentTarget.ACDGuid);
                }

                // Zombie Charger backup
                if (CanCast(SNOPower.Witchdoctor_ZombieCharger) && Player.PrimaryResource >= 150)
                {
                    return new TrinityPower(SNOPower.Witchdoctor_ZombieCharger, zombieChargerRange, CurrentTarget.Position);
                }

                // Regular spirit barage
                if (CanCast(SNOPower.Witchdoctor_SpiritBarrage) && Player.PrimaryResource >= 100 && !hasManitou)
                {
                    return new TrinityPower(SNOPower.Witchdoctor_SpiritBarrage, basicAttackRange, CurrentTarget.ACDGuid);
                }

                // Poison Darts fast-attack Spams Darts when mana is too low (to cast bears) @12yds or @10yds if Bears avialable
                if (CanCast(SNOPower.Witchdoctor_PoisonDart))
                {
                    WitchDoctorCombat.VisionQuestRefreshTimer.Restart();
                    return new TrinityPower(SNOPower.Witchdoctor_PoisonDart, basicAttackRange, CurrentTarget.ACDGuid);
                }
                // Corpse Spiders fast-attacks Spams Spiders when mana is too low (to cast bears) @12yds or @10yds if Bears avialable
                if (CanCast(SNOPower.Witchdoctor_CorpseSpider))
                {
                    WitchDoctorCombat.VisionQuestRefreshTimer.Restart();
                    return new TrinityPower(SNOPower.Witchdoctor_CorpseSpider, basicAttackRange, CurrentTarget.ACDGuid);
                }
                // Toads fast-attacks Spams Toads when mana is too low (to cast bears) @12yds or @10yds if Bears avialable
                if (CanCast(SNOPower.Witchdoctor_PlagueOfToads))
                {
                    WitchDoctorCombat.VisionQuestRefreshTimer.Restart();
                    return new TrinityPower(SNOPower.Witchdoctor_PlagueOfToads, basicAttackRange, CurrentTarget.ACDGuid);
                }
                // Fire Bomb fast-attacks Spams Bomb when mana is too low (to cast bears) @12yds or @10yds if Bears avialable
                if (CanCast(SNOPower.Witchdoctor_Firebomb))
                {
                    WitchDoctorCombat.VisionQuestRefreshTimer.Restart();
                    return new TrinityPower(SNOPower.Witchdoctor_Firebomb, basicAttackRange, CurrentTarget.ACDGuid);
                }

            }

            // Buffs
            if (UseOOCBuff)
            {
                // Spirit Walk OOC 
                if (CanCast(SNOPower.Witchdoctor_SpiritWalk) && Settings.Combat.Misc.AllowOOCMovement)
                {
                    return new TrinityPower(SNOPower.Witchdoctor_SpiritWalk);
                }

                bool hasStalker = HotbarSkills.AssignedSkills.Any(s => s.Power == SNOPower.Witchdoctor_Horrify && s.RuneIndex == 4);
                // Horrify Buff When not in combat for movement speed -- Stalker
                if (CanCast(SNOPower.Witchdoctor_Horrify) && hasStalker)
                {
                    return new TrinityPower(SNOPower.Witchdoctor_Horrify);
                }

                // Zombie Dogs non-sacrifice build
                if (CanCast(SNOPower.Witchdoctor_SummonZombieDog) && Trinity.PlayerOwnedZombieDog <= 2)
                {
                    return new TrinityPower(SNOPower.Witchdoctor_SummonZombieDog);
                }


                bool hasRestlessGiant = HotbarSkills.AssignedSkills.Any(s => s.Power == SNOPower.Witchdoctor_Gargantuan && s.RuneIndex == 0);
                bool hasWrathfulProtector = HotbarSkills.AssignedSkills.Any(s => s.Power == SNOPower.Witchdoctor_Gargantuan && s.RuneIndex == 3);

                if (CanCast(SNOPower.Witchdoctor_Gargantuan) && !hasRestlessGiant && !hasWrathfulProtector && Trinity.PlayerOwnedGargantuan == 0)
                {
                    return new TrinityPower(SNOPower.Witchdoctor_Gargantuan);
                }
            }

            // Default Attacks
            if (IsNull(power))
                power = CombatBase.DefaultPower;

            return power;
        }

        public static readonly HashSet<int> ZunimasaItemIds = new HashSet<int>()
        {
            -960430780, // Zunimassa's String of Skulls
            -1187722720, // Zunimassa's Pox
            1941359608, // Zunimassa's Trail
        };

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


        private static TrinityPower DestroyObjectPower
        {
            get
            {

                if (Hotbar.Contains(SNOPower.Witchdoctor_Firebomb))
                    return new TrinityPower(SNOPower.Witchdoctor_Firebomb, 12f, CurrentTarget.Position);
                if (Hotbar.Contains(SNOPower.Witchdoctor_PoisonDart))
                    return new TrinityPower(SNOPower.Witchdoctor_PoisonDart, 15f, CurrentTarget.Position);
                if (Hotbar.Contains(SNOPower.Witchdoctor_ZombieCharger) && Player.PrimaryResource >= 150)
                    return new TrinityPower(SNOPower.Witchdoctor_ZombieCharger, 12f, CurrentTarget.Position);
                if (Hotbar.Contains(SNOPower.Witchdoctor_CorpseSpider))
                    return new TrinityPower(SNOPower.Witchdoctor_CorpseSpider, 12f, CurrentTarget.Position);
                if (Hotbar.Contains(SNOPower.Witchdoctor_PlagueOfToads))
                    return new TrinityPower(SNOPower.Witchdoctor_PlagueOfToads, 12f, CurrentTarget.Position);
                if (Hotbar.Contains(SNOPower.Witchdoctor_AcidCloud) && Player.PrimaryResource >= 175)
                    return new TrinityPower(SNOPower.Witchdoctor_AcidCloud, 12f, CurrentTarget.Position);

                if (Hotbar.Contains(SNOPower.Witchdoctor_Sacrifice) && Hotbar.Contains(SNOPower.Witchdoctor_SummonZombieDog) &&
                    Trinity.PlayerOwnedZombieDog > 0 && Settings.Combat.WitchDoctor.ZeroDogs)
                    return new TrinityPower(SNOPower.Witchdoctor_Sacrifice, 12f, CurrentTarget.Position);

                if (Hotbar.Contains(SNOPower.Witchdoctor_SpiritBarrage) && Player.PrimaryResource > 100)
                    return new TrinityPower(SNOPower.Witchdoctor_SpiritBarrage, 12f, CurrentTarget.ACDGuid);

                return CombatBase.DefaultPower;
            }
        }

    }
}
