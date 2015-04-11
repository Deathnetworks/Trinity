using System;
using System.Collections.Generic;
using System.Linq;
using Trinity.DbProvider;
using Trinity.Reference;
using Zeta.Common;
using Zeta.Game.Internals.Actors;
using Logger = Trinity.Technicals.Logger;

namespace Trinity.Combat.Abilities
{
    public class WitchDoctorCombat : CombatBase
    {
        private static int MaxFetishArmyCount
        {
            get
            {
                if (Runes.WitchDoctor.LegionOfDaggers.IsActive)
                    return 8;
                if (Runes.WitchDoctor.TikiTorchers.IsActive || Runes.WitchDoctor.HeadHunters.IsActive)
                    return 7;

                return 5;
            }
        }
        private static readonly HashSet<SNOPower> HarvesterDebuffs = new HashSet<SNOPower>
        {
            SNOPower.Witchdoctor_Haunt,
            SNOPower.Witchdoctor_Locust_Swarm,
            SNOPower.Witchdoctor_Piranhas,
            SNOPower.Witchdoctor_AcidCloud
        };
        private static readonly HashSet<SNOPower> HarvesterCoreDebuffs = new HashSet<SNOPower>
        {
            SNOPower.Witchdoctor_Haunt,
            SNOPower.Witchdoctor_Locust_Swarm,
        };
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

            // Spirit walk
            power = GetSpiritWalkPower();
            if (!IsNull(power)) { return power; }

            // Skills off CD
            power = GetSpamPower();
            if (!IsNull(power)) { return power; }

            // Avoidance
            if (IsCurrentlyAvoiding || Trinity.Player.StandingInAvoidance)
            {
                power = GetAvoidancePower();
                if (!IsNull(power)) { return power; }
            }

            // Destruclible
            if (UseDestructiblePower)
            {
                return DestroyObjectPower;
            }

            // Bastions of will
            if (!UseOOCBuff && !IsCurrentlyAvoiding && IsBastionsPrimaryBuffWillExpire)
            {
                power = GetPrimaryPower();
                if (!IsNull(power)) { return power; }
            }

            // Jade Harvester special logic
            if (Player.IsInCombat && Sets.RaimentOfTheJadeHarvester.IsMaxBonusActive)
            {
                power = RunJadeHarvesterRoutine();
                if (!IsNull(power)) { return power; }
            }

            if (Player.IsInCombat && Legendary.TiklandianVisage.IsEquipped)
            {
                power = RunTiklandianRoutine();
                if (!IsNull(power)) { return power; }
            }

            // Combat
            if (!UseOOCBuff && !IsCurrentlyAvoiding && CurrentTarget != null)
            {
                power = GetCombatPower();
                if (!IsNull(power)) { return power; }

                power = GetPrimaryPower();
                if (!IsNull(power)) { return power; }
            }

            // Buffs
            if (UseOOCBuff)
            {
                power = GetOOCPower();
                if (!IsNull(power)) { return power; }
            }

            // Default Attacks
            if (IsNull(power)) { return DefaultPower; }

            return power;
        }

        private static TrinityPower GetSpiritWalkPower()
        {
            if (!Skills.WitchDoctor.SpiritWalk.CanCast(CanCastFlags.NoTimer))
                return null;

            // Spirit walk OOC Movement
            if (!Player.IsInCombat && Settings.Combat.Misc.AllowOOCMovement && PlayerMover.GetMovementSpeed() > 1)
                return new TrinityPower(SNOPower.Witchdoctor_SpiritWalk);

            // Spirit walk Avoidance
            if (Trinity.Player.StandingInAvoidance || IsCurrentlyAvoiding)
                return new TrinityPower(SNOPower.Witchdoctor_SpiritWalk);

            // Spirit walk Incapacited or Rooted
            if (Player.IsIncapacitated || Player.IsRooted)
                return new TrinityPower(SNOPower.Witchdoctor_SpiritWalk);

            // Spirit walk Mana purchase
            if (Runes.WitchDoctor.HonoredGuest.IsActive && Player.PrimaryResourcePct <= 0.5)
                return new TrinityPower(SNOPower.Witchdoctor_SpiritWalk);

            // Spirit walk Life purchase
            if (Runes.WitchDoctor.HealingJourney.IsActive && Player.CurrentHealthPct <= 0.65)
                return new TrinityPower(SNOPower.Witchdoctor_SpiritWalk);

            // Avoid death
            if (Player.CurrentHealthPct <= 0.5)
                return new TrinityPower(SNOPower.Witchdoctor_SpiritWalk);

            // Spirit walk Goblin track
            if (!Settings.Combat.WitchDoctor.UseSpiritWalkOnlyAvoidance && CurrentTarget != null &&
                CurrentTarget.IsTreasureGoblin && CurrentTarget.HitPointsPct < 0.90 && CurrentTarget.RadiusDistance <= 40f)
            {
                return new TrinityPower(SNOPower.Witchdoctor_SpiritWalk);
            }

            // Spam in combat
            if (Player.IsInCombat && !Settings.Combat.WitchDoctor.UseSpiritWalkOnlyAvoidance)
            {
                return new TrinityPower(SNOPower.Witchdoctor_SpiritWalk);
            }

            return null;
        }

        private static TrinityPower GetAvoidancePower()
        {
            // Spirit walk
            TrinityPower spiritWalkPower = GetSpiritWalkPower();
            if (!IsNull(spiritWalkPower)) { return spiritWalkPower; }

            // Horrify
            if (Skills.WitchDoctor.Horrify.CanCast())
            {
                TrinityPower horrifyPower = GetHorrifyPower();
                if (!IsNull(horrifyPower)) { return horrifyPower; }
            }

            return null;
        }

        private static TrinityPower GetSpamPower()
        {
            // Horrify Off CD
            if (Settings.Combat.WitchDoctor.SpamHorrify && CanCast(SNOPower.Witchdoctor_Horrify, CanCastFlags.NoTimer))
            {
                TrinityPower power = GetHorrifyPower();
                if (!IsNull(power)) { return power; }
            }

            if (TargetUtil.AnyMobsInRange(40f))
            {
                // Fetish Army off CD
                if (Settings.Combat.WitchDoctor.UseFetishArmyOffCooldown && CanCast(SNOPower.Witchdoctor_FetishArmy, CanCastFlags.NoTimer) && Trinity.PlayerOwnedFetishCount <= MaxFetishArmyCount * 0.6)
                    return new TrinityPower(SNOPower.Witchdoctor_FetishArmy);

                // BigBadVoodoo off CD at Player
                if (Settings.Combat.WitchDoctor.UseBigBadVoodooOffCooldown && CanCast(SNOPower.Witchdoctor_BigBadVoodoo, CanCastFlags.NoTimer) && !CacheData.Voodoo.Any(bbv => bbv.Position.Distance2D(Player.Position) <= 27f))
                    return new TrinityPower(SNOPower.Witchdoctor_BigBadVoodoo);

                // BigBadVoodoo off CD close to AoE
                if (Settings.Combat.WitchDoctor.UseBigBadVoodooOffCooldown && CanCast(SNOPower.Witchdoctor_BigBadVoodoo, CanCastFlags.NoTimer) && CacheData.Voodoo.Any(bbv => bbv.Position.Distance2D(Player.Position) <= 27f) &&
                    CurrentTarget != null && CurrentTarget.IsTrashPackOrBossEliteRareUnique)
                {
                    return new TrinityPower(SNOPower.Witchdoctor_BigBadVoodoo, 8f, TargetUtil.GetBestClusterPoint(30f, RangedAttackRange));
                }
            }

            return null;
        }

        private static TrinityPower GetOOCPower()
        {
            // Horrify increase move speed by 20%
            if (CanCast(SNOPower.Witchdoctor_Horrify) && Runes.WitchDoctor.Stalker.IsActive && PlayerMover.GetMovementSpeed() > 1)
                return new TrinityPower(SNOPower.Witchdoctor_Horrify);

            // Zombie Dogs non-sacrifice build
            if (CanCast(SNOPower.Witchdoctor_SummonZombieDog) &&
                ((Legendary.TheTallMansFinger.IsEquipped && Trinity.PlayerOwnedZombieDogCount < 1) ||
                (!Legendary.TheTallMansFinger.IsEquipped && Trinity.PlayerOwnedZombieDogCount <= 2)))
            {
                return new TrinityPower(SNOPower.Witchdoctor_SummonZombieDog);
            }

            // Gargantuan
            if (CanCast(SNOPower.Witchdoctor_Gargantuan) && Trinity.PlayerOwnedGargantuanCount == 0 &&
                !Runes.WitchDoctor.RestlessGiant.IsActive && !Runes.WitchDoctor.WrathfulProtector.IsActive)
            {
                return new TrinityPower(SNOPower.Witchdoctor_Gargantuan);
            }

            // Spirit Barrage Manitou
            if (CanCast(SNOPower.Witchdoctor_SpiritBarrage) && Runes.WitchDoctor.Manitou.IsActive &&
                TimeSincePowerUse(SNOPower.Witchdoctor_SpiritBarrage) > 18000)
            {
                return new TrinityPower(SNOPower.Witchdoctor_SpiritBarrage);
            }

            return null;
        }

        private static TrinityPower GetCombatPower()
        {
            // Vision Quest Passive
            if (Passives.WitchDoctor.VisionQuest.IsActive &&
                (!GetHasBuff(SNOPower.Witchdoctor_Passive_VisionQuest) || GetTimeSinceLastVisionQuestRefresh() > 3800))
            {
                TrinityPower primaryPower = GetPrimaryPower();
                if (!IsNull(primaryPower)) { return primaryPower; }
            }

            // Sacrifice
            if (CanCast(SNOPower.Witchdoctor_Sacrifice) && Trinity.PlayerOwnedZombieDogCount > 0)
            {
                if (TargetUtil.AnyElitesInRange(15f, 1) || (CurrentTarget.IsBossOrEliteRareUnique && CurrentTarget.RadiusDistance <= 15f))
                {
                    return new TrinityPower(SNOPower.Witchdoctor_Sacrifice);
                }

                // Sacrifice for Circle of Life
                if (Passives.WitchDoctor.CircleOfLife.IsActive && TargetUtil.AnyMobsInRange(15f))
                {
                    return new TrinityPower(SNOPower.Witchdoctor_Sacrifice);
                }

                // Sacrifice for 0 Dogs
                if (TargetUtil.AnyMobsInRange(25f) && (Settings.Combat.WitchDoctor.ZeroDogs || !WitchDoctorHasPrimaryAttack))
                {
                    return new TrinityPower(SNOPower.Witchdoctor_Sacrifice);
                }
            }

            // Gargantuan
            if (CanCast(SNOPower.Witchdoctor_Gargantuan) && Trinity.PlayerOwnedGargantuanCount == 0)
            {
                // Gargantuan, Recast on Elites or Bosses to trigger Restless Giant
                if (Runes.WitchDoctor.RestlessGiant.IsActive && (TargetUtil.IsEliteTargetInRange(30f) || Trinity.PlayerOwnedGargantuanCount == 0))
                {
                    return new TrinityPower(SNOPower.Witchdoctor_Gargantuan);
                }

                // Gargantuan Wrathful Protector, 15 seconds of smash, use sparingly!
                if (Runes.WitchDoctor.WrathfulProtector.IsActive && TargetUtil.IsEliteTargetInRange(30f))
                {
                    return new TrinityPower(SNOPower.Witchdoctor_Gargantuan);
                }

                // Gargantuan regular
                if (!Runes.WitchDoctor.RestlessGiant.IsActive && !Runes.WitchDoctor.WrathfulProtector.IsActive)
                {
                    return new TrinityPower(SNOPower.Witchdoctor_Gargantuan);
                }
            }

            // Zombie Dogs
            if (CanCast(SNOPower.Witchdoctor_SummonZombieDog))
            {
                // Zombie Dogs for Sacrifice
                if (Skills.WitchDoctor.Sacrifice.IsActive &&
                    (LastPowerUsed == SNOPower.Witchdoctor_Sacrifice || Trinity.PlayerOwnedZombieDogCount <= 2))
                {
                    return new TrinityPower(SNOPower.Witchdoctor_SummonZombieDog);
                }

                // Zombie Dogs non-sacrifice build
                if (!Skills.WitchDoctor.Sacrifice.IsActive &&
                    ((Legendary.TheTallMansFinger.IsEquipped && Trinity.PlayerOwnedZombieDogCount < 1) ||
                    (!Legendary.TheTallMansFinger.IsEquipped && Trinity.PlayerOwnedZombieDogCount <= 2)))
                {
                    return new TrinityPower(SNOPower.Witchdoctor_SummonZombieDog);
                }
            }

            // Hex with angry chicken, is chicken, explode!
            if (CanCast(SNOPower.Witchdoctor_Hex_Explode) && Skills.WitchDoctor.Hex.IsActive && Player.IsHidden &&
                (TargetUtil.AnyMobsInRange(12f) || CurrentTarget.RadiusDistance <= 12f))
            {
                return new TrinityPower(SNOPower.Witchdoctor_Hex_Explode);
            }

            // Hex
            if (CanCast(SNOPower.Witchdoctor_Hex) &&
                (TargetUtil.ClusterExists(RangedAttackRange, 2) || TargetUtil.EliteOrTrashInRange(RangedAttackRange)))
            {
                return new TrinityPower(SNOPower.Witchdoctor_Hex);
            }

            // Mass Confuse, elites only or big mobs or to escape on low health
            if (CanCast(SNOPower.Witchdoctor_MassConfusion))
            {
                if (TargetUtil.AnyElitesInRange(MeleeAttackRange) || TargetUtil.AnyMobsInRange(MeleeAttackRange, 5))
                {
                    return new TrinityPower(SNOPower.Witchdoctor_MassConfusion);
                }

                if (Player.CurrentHealthPct <= 0.25 && TargetUtil.AnyMobsInRange(MeleeAttackRange, 2))
                {
                    return new TrinityPower(SNOPower.Witchdoctor_MassConfusion);
                }

                if (CurrentTarget.IsBossOrEliteRareUnique && CurrentTarget.RadiusDistance <= MeleeAttackRange)
                {
                    return new TrinityPower(SNOPower.Witchdoctor_MassConfusion);
                }
            }

            // Fetish Army
            if (CanCast(SNOPower.Witchdoctor_FetishArmy) && Trinity.PlayerOwnedFetishCount <= MaxFetishArmyCount * 0.6 &&
                (TargetUtil.EliteOrTrashInRange(RangedAttackRange) || (CurrentTarget.IsBoss && CurrentTarget.Distance <= RangedAttackRange)))
            {
                return new TrinityPower(SNOPower.Witchdoctor_FetishArmy);
            }

            // Big Bad Voodoo
            if (CanCast(SNOPower.Witchdoctor_BigBadVoodoo))
            {
                // At player
                if (!Trinity.Player.HasDebuff(SNOPower.Witchdoctor_BigBadVoodoo) && (TargetUtil.EliteOrTrashInRange(RangedAttackRange) || (CurrentTarget.IsBoss && CurrentTarget.Distance <= RangedAttackRange)))
                {
                    return new TrinityPower(SNOPower.Witchdoctor_BigBadVoodoo);
                }

                // Close to AoE
                if (Trinity.Player.HasDebuff(SNOPower.Witchdoctor_BigBadVoodoo) && CurrentTarget.IsTrashPackOrBossEliteRareUnique)
                {
                    return new TrinityPower(SNOPower.Witchdoctor_BigBadVoodoo, 8f, TargetUtil.GetBestClusterPoint(30f, RangedAttackRange));
                }
            }

            // Grasp of the Dead
            if (CanCast(SNOPower.Witchdoctor_GraspOfTheDead) &&
                (TargetUtil.ClusterExists(RangedAttackRange, 2) || TargetUtil.EliteOrTrashInRange(RangedAttackRange)))
            {
                return new TrinityPower(SNOPower.Witchdoctor_GraspOfTheDead, RangedAttackRange, TargetUtil.GetBestClusterPoint(15f, RangedAttackRange));
            }

            // Piranhas
            if (Skills.WitchDoctor.Piranhas.CanCast() &&
                (TargetUtil.ClusterExists(20f, 3) || TargetUtil.EliteOrTrashInRange(RangedAttackRange)))
            {
                return new TrinityPower(SNOPower.Witchdoctor_Piranhas, RangedAttackRange, TargetUtil.GetBestClusterPoint(20f));
            }

            // Wall of Zombies
            if (CanCast(SNOPower.Witchdoctor_WallOfZombies) &&
                (TargetUtil.AnyElitesInRange(RangedAttackRange, 1) || TargetUtil.AnyMobsInRange(RangedAttackRange, 4) ||
                (CurrentTarget.IsEliteRareUnique || CurrentTarget.IsTreasureGoblin || CurrentTarget.IsBoss)))
            {
                return new TrinityPower(SNOPower.Witchdoctor_WallOfZombies, RangedAttackRange, CurrentTarget.Position);
            }

            // Horrify
            if (CanCast(SNOPower.Witchdoctor_Horrify))
            {
                TrinityPower horrifyPower = GetHorrifyPower();
                if (!IsNull(horrifyPower)) { return horrifyPower; }
            }

            // Haunt 
            if (CanCast(SNOPower.Witchdoctor_Haunt) && LastPowerUsed != SNOPower.Witchdoctor_Haunt &&
                !CurrentTarget.HasDebuff(SNOPower.Witchdoctor_Haunt))
            {
                return new TrinityPower(SNOPower.Witchdoctor_Haunt, RangedAttackRange, CurrentTarget.Position);
            }

            // Locust Swarm
            if (CanCast(SNOPower.Witchdoctor_Locust_Swarm) &&
                !CurrentTarget.HasDebuff(SNOPower.Witchdoctor_Locust_Swarm))
            {
                return new TrinityPower(SNOPower.Witchdoctor_Locust_Swarm, MeleeAttackRange, CurrentTarget.Position);
            }

            // Zombie Charger
            if (CanCast(SNOPower.Witchdoctor_ZombieCharger))
            {
                return new TrinityPower(SNOPower.Witchdoctor_ZombieCharger, MeleeAttackRange, CurrentTarget.Position);
            }

            if (!Sets.RaimentOfTheJadeHarvester.IsMaxBonusActive && CanCast(SNOPower.Witchdoctor_SoulHarvest))
            {
                // Soul Harvest Any Elites or to increase buff stacks
                if (TargetUtil.AnyMobsInRange(16f, GetBuffStacks(SNOPower.Witchdoctor_SoulHarvest) + 1, false) ||
                    TargetUtil.IsEliteTargetInRange(16f))
                {
                    return new TrinityPower(SNOPower.Witchdoctor_SoulHarvest);
                }

                // Soul Harvest with VengefulSpirit
                if (Runes.WitchDoctor.VengefulSpirit.IsActive && TargetUtil.AnyMobsInRange(16, 3))
                {
                    return new TrinityPower(SNOPower.Witchdoctor_SoulHarvest);
                }

                // Soul Harvest with SwallowYourSoul
                if (Runes.WitchDoctor.SwallowYourSoul.IsActive && Player.PrimaryResourcePct <= 0.50)
                {
                    return new TrinityPower(SNOPower.Witchdoctor_SoulHarvest);
                }
            }

            // Firebats
            if (CanCast(SNOPower.Witchdoctor_Firebats))
            {
                int fireBatsChannelCost = Runes.WitchDoctor.VampireBats.IsActive ? 0 : 75;
                int fireBatsMana = TimeSincePowerUse(SNOPower.Witchdoctor_Firebats) < 125 ? fireBatsChannelCost : 225;

                bool firebatsMaintain =
                  Trinity.ObjectCache.Any(u => u.IsUnit &&
                      u.IsPlayerFacing(70f) && u.Weight > 0 &&
                      u.Distance <= V.F("WitchDoctor.Firebats.MaintainRange") &&
                      SpellHistory.TimeSinceUse(SNOPower.Witchdoctor_Firebats) <= TimeSpan.FromMilliseconds(250d));

                // Fire Bats:Cloud of bats 
                if (Player.PrimaryResource >= fireBatsMana &&
                    Runes.WitchDoctor.CloudOfBats.IsActive && (TargetUtil.AnyMobsInRange(8f) || firebatsMaintain))
                {
                    var range = Settings.Combat.WitchDoctor.FirebatsRange > 12f ? 12f : Settings.Combat.WitchDoctor.FirebatsRange;
                    return new TrinityPower(SNOPower.Witchdoctor_Firebats, range, CurrentTarget.Position);
                }

                // Fire Bats fast-attack
                if (Player.PrimaryResource >= fireBatsMana &&
                     (TargetUtil.AnyMobsInRange(Settings.Combat.WitchDoctor.FirebatsRange) || firebatsMaintain) && !Runes.WitchDoctor.CloudOfBats.IsActive)
                {
                    return new TrinityPower(SNOPower.Witchdoctor_Firebats, Settings.Combat.WitchDoctor.FirebatsRange, CurrentTarget.Position);
                }
            }

            // Acid Cloud
            if (CanCast(SNOPower.Witchdoctor_AcidCloud))
            {
                Vector3 bestClusterPoint;
                if (Passives.WitchDoctor.GraveInjustice.IsActive)
                    bestClusterPoint = TargetUtil.GetBestClusterPoint(15f, Math.Min(Player.GoldPickupRadius + 8f, 30f));
                else
                    bestClusterPoint = TargetUtil.GetBestClusterPoint(15f, 30f);

                return new TrinityPower(SNOPower.Witchdoctor_AcidCloud, RangedAttackRange, bestClusterPoint);
            }

            if (CanCast(SNOPower.Witchdoctor_SpiritBarrage))
            {
                // Spirit Barrage + Rush of Essence
                if (Passives.WitchDoctor.RushOfEssence.IsActive && !Runes.WitchDoctor.Manitou.IsActive)
                {
                    return new TrinityPower(SNOPower.Witchdoctor_SpiritBarrage, MeleeAttackRange, CurrentTarget.Position);
                }

                // Spirit Barrage Manitou
                if (Runes.WitchDoctor.Manitou.IsActive && TimeSincePowerUse(SNOPower.Witchdoctor_SpiritBarrage) > 18000)
                {
                    return new TrinityPower(SNOPower.Witchdoctor_SpiritBarrage);
                }

                // Regular spirit barage
                if (!Runes.WitchDoctor.Manitou.IsActive)
                {
                    return new TrinityPower(SNOPower.Witchdoctor_SpiritBarrage, MeleeAttackRange, CurrentTarget.Position);
                }
            }



            return null;
        }

        public static TrinityPower GetHorrifyPower()
        {
            // Horrify FrighteningAspect
            if (!CombatBase.PlayerIsImmune && Runes.WitchDoctor.FrighteningAspect.IsActive &&
                (Player.CurrentHealthPct <= 0.25 ||
                (Player.CurrentHealthPct <= 0.5 && TargetUtil.AnyMobsInRange(18f, 1)) ||
                (Player.CurrentHealthPct <= 0.75 && TargetUtil.AnyMobsInRange(18f, 2))))
            {
                return new TrinityPower(SNOPower.Witchdoctor_Horrify);
            }

            // Horrify increase move speed by 20%
            if (Runes.WitchDoctor.Stalker.IsActive && PlayerMover.GetMovementSpeed() > 1)
                return new TrinityPower(SNOPower.Witchdoctor_Horrify);

            // Horrify 24 yards
            if (!Runes.WitchDoctor.Stalker.IsActive && Runes.WitchDoctor.FaceOfDeath.IsActive &&
                TargetUtil.AnyMobsInRange(24f))
            {
                return new TrinityPower(SNOPower.Witchdoctor_Horrify);
            }

            // Horrify 18 yards
            if (!Runes.WitchDoctor.Stalker.IsActive && !Runes.WitchDoctor.FaceOfDeath.IsActive &&
                TargetUtil.AnyMobsInRange(18f))
            {
                return new TrinityPower(SNOPower.Witchdoctor_Horrify);
            }

            return null;
        }

        public static TrinityPower GetPrimaryPower()
        {
            // Poison Darts
            if (CanCast(SNOPower.Witchdoctor_PoisonDart))
            {
                VisionQuestRefreshTimer.Restart();
                return new TrinityPower(SNOPower.Witchdoctor_PoisonDart, RangedAttackRange, CurrentTarget.Position);
            }
            // Corpse Spiders
            if (CanCast(SNOPower.Witchdoctor_CorpseSpider))
            {
                VisionQuestRefreshTimer.Restart();
                return new TrinityPower(SNOPower.Witchdoctor_CorpseSpider, RangedAttackRange, CurrentTarget.Position);
            }
            // Plague Of Toads
            if (CanCast(SNOPower.Witchdoctor_PlagueOfToads))
            {
                VisionQuestRefreshTimer.Restart();
                return new TrinityPower(SNOPower.Witchdoctor_PlagueOfToads, RangedAttackRange, CurrentTarget.Position);
            }
            // Fire Bomb
            if (CanCast(SNOPower.Witchdoctor_Firebomb))
            {
                VisionQuestRefreshTimer.Restart();
                return new TrinityPower(SNOPower.Witchdoctor_Firebomb, RangedAttackRange, CurrentTarget.Position);
            }

            return null;
        }

        public static TrinityPower RunTiklandianRoutine()
        {
            // Piranhas
            if (Skills.WitchDoctor.Piranhas.CanCast() &&
                (TargetUtil.ClusterExists(20f, 3) || TargetUtil.EliteOrTrashInRange(RangedAttackRange)))
            {
                return new TrinityPower(SNOPower.Witchdoctor_Piranhas, RangedAttackRange, TargetUtil.GetBestClusterPoint(20f));
            }

            float horrifyMinRange = Runes.WitchDoctor.FaceOfDeath.IsActive ? 24f : 18f;

            //Cast Horrify before we go into the fray
            if (Skills.WitchDoctor.Horrify.CanCast() && TikHorrifyCriteria(new TargetArea(horrifyMinRange)))
            {
                Skills.WitchDoctor.Horrify.Cast();
            }

            // Should we move to a better position to fear people
            if (TikHorrifyCriteria(new TargetCluster(horrifyMinRange)))
            {
                MoveToHorrifyPoint(new TargetCluster(horrifyMinRange));
            }

            return null;
        }

        private static TargetCluster SoulHarvestBestCluster = null;
        private static TargetCluster GetSoulHarvestBestCluster
        {
            get
            {
                if (Enemies.BestLargeCluster.Exists && IdealSoulHarvestCriteria(Enemies.BestLargeCluster))
                    return Enemies.BestLargeCluster;

                if (Enemies.BestCluster.Exists && IdealSoulHarvestCriteria(Enemies.BestCluster))
                    return Enemies.BestCluster;

                if (Enemies.BestLargeCluster.Exists && MinimumSoulHarvestCriteria(Enemies.BestLargeCluster))
                    return Enemies.BestLargeCluster;

                if (Enemies.BestCluster.Exists && MinimumSoulHarvestCriteria(Enemies.BestCluster))
                    return Enemies.BestCluster;

                if (Enemies.BestLargeCluster.Exists)
                    return Enemies.BestLargeCluster;

                if (Enemies.BestCluster.Exists)
                    return Enemies.BestCluster;

                return null;
            }
        }

        public static TrinityPower RunJadeHarvesterRoutine()
        {
            // Piranhas
            if (Skills.WitchDoctor.Piranhas.CanCast() &&
                (TargetUtil.ClusterExists(20f, 3) || TargetUtil.EliteOrTrashInRange(RangedAttackRange)))
            {
                return new TrinityPower(SNOPower.Witchdoctor_Piranhas, RangedAttackRange, TargetUtil.GetBestClusterPoint(20f));
            }

            SoulHarvestBestCluster = GetSoulHarvestBestCluster;
            // Should we move to cluster for harvest
            if (SoulHarvestBestCluster != null)
            {
                MoveToSoulHarvestPoint(SoulHarvestBestCluster);
                if (CurrentTarget == null)
                {
                    CombatBase.SwitchToTarget(TargetUtil.GetClosestTarget(150f));
                    Trinity.CurrentTarget.Position = SoulHarvestBestCluster.Position;
                }
            }


            // Should we harvest right here?
            if (Skills.WitchDoctor.SoulHarvest.CanCast() &&
                (IdealSoulHarvestCriteria(Enemies.CloseNearby) || MinimumSoulHarvestCriteria(Enemies.CloseNearby)))
            {
                Skills.WitchDoctor.SoulHarvest.Cast();
            }

            if (CurrentTarget != null && CurrentTarget.IsUnit)
            {
                TrinityCacheObject getTargetWithoutDebuffs = Enemies.BestCluster.GetTargetWithoutDebuffs(HarvesterCoreDebuffs);
                if (getTargetWithoutDebuffs != null && getTargetWithoutDebuffs != default(TrinityCacheObject))
                {
                    CombatBase.SwitchToTarget(getTargetWithoutDebuffs);
                }

                // Locust Swarm
                if (CanCast(SNOPower.Witchdoctor_Locust_Swarm) &&
                    !CurrentTarget.HasDebuff(SNOPower.Witchdoctor_Locust_Swarm))
                {
                    return new TrinityPower(SNOPower.Witchdoctor_Locust_Swarm, MeleeAttackRange, CurrentTarget.ClusterPosition(MeleeAttackRange), CurrentTarget.ACDGuid);
                }

                // Haunt 
                if (CanCast(SNOPower.Witchdoctor_Haunt) &&
                    !CurrentTarget.HasDebuff(SNOPower.Witchdoctor_Haunt))
                {
                    return new TrinityPower(SNOPower.Witchdoctor_Haunt, MeleeAttackRange, CurrentTarget.ClusterPosition(MeleeAttackRange), CurrentTarget.ACDGuid);
                }

                // Acid Cloud
                if (Skills.WitchDoctor.AcidCloud.CanCast() && Player.PrimaryResource >= 325 &&
                    LastPowerUsed != SNOPower.Witchdoctor_AcidCloud)
                {
                    return new TrinityPower(SNOPower.Witchdoctor_AcidCloud, MeleeAttackRange, CurrentTarget.ClusterPosition(MeleeAttackRange));
                }
            }

            return null;
        }

        private static float RangedAttackRange
        {
            get
            {
                float range = 30f;
                if (CombatBase.KiteDistance >= 20f)
                    range = CombatBase.KiteDistance + 8f;

                if (Passives.WitchDoctor.GraveInjustice.IsActive)
                    return Math.Min(Player.GoldPickupRadius + 20f, range);

                return range;
            }
        }
        private static float MeleeAttackRange
        {
            get
            {
                if (CanCast(SNOPower.Witchdoctor_ZombieCharger))
                    return 30f;

                if (Legendary.TiklandianVisage.IsEquipped && !TikHorrifyCriteria(Enemies.BestLargeCluster))
                    return 25f;

                if (Legendary.TiklandianVisage.IsEquipped)
                    return 1f;

                return 16f;
            }
        }

        private static readonly Func<TargetArea, bool> MinimumSoulHarvestCriteria = area =>

            //Harvest is off cooldown AND at least 2 debuffs exists && at least 40% of the units have a havestable debuff
            //area.TotalDebuffCount(HarvesterCoreDebuffs) >= 2 && 
            area.DebuffedCount(HarvesterCoreDebuffs) >= area.UnitCount * 0.6 &&

            // AND there's an elite, boss or more than 3 units or greater 35% of the units within sight are within this cluster
            (area.EliteCount > 0 || area.BossCount > 0 || area.UnitCount >= 2);// || area.UnitCount >= (float)Enemies.Nearby.UnitCount * 0.35);


        private static readonly Func<TargetArea, bool> IdealSoulHarvestCriteria = area =>

            // Harvest is off cooldown AND at least 7 debuffs are present (can be more than 1 per unit)
            //area.TotalDebuffCount(HarvesterDebuffs) > 7 && 
            area.DebuffedCount(HarvesterCoreDebuffs) >= area.UnitCount * 0.8 &&

            // AND average health accross units in area is more than 30%
            area.AverageHealthPct > 0.3f &&

            // AND at least 2 Elites, a boss or more than 5 units or 80% of the nearby units are within this area
            (area.EliteCount >= 1 || area.BossCount > 0 || area.UnitCount >= 5 || area.UnitCount >= (float)Enemies.Nearby.UnitCount * 0.80);

        private static readonly Func<TargetArea, bool> TikHorrifyCriteria = area =>

            //at least 2 Elites, a boss or more than 5 units or 80% of the nearby units are within this area
            (area.EliteCount >= 2 || area.UnitCount >= 5);// || area.UnitCount >= (float)Enemies.Nearby.UnitCount * 0.80);



        private static readonly Action<string, TargetArea> LogTargetArea = (message, area) =>
        {
            Logger.LogDebug(message + " Units={0} Elites={1} DebuffedUnits={2} TotalDebuffs={4} AvgHealth={3:#.#} ---",
                area.UnitCount,
                area.EliteCount,
                area.DebuffedCount(HarvesterDebuffs),
                area.AverageHealthPct * 100,
                area.TotalDebuffCount(HarvesterDebuffs));
        };

        private static void MoveToSoulHarvestPoint(TargetArea area)
        {
            QueuedMovement.Queue(new QueuedMovement
            {
                Name = "Jade Harvest Position",
                Destination = area.Position,
                OnUpdate = m =>
                {
                    TargetCluster bestCluster = SoulHarvestBestCluster;
                    if (bestCluster != null)
                    {
                        m.Destination = bestCluster.Position;
                    }

                    if (Skills.WitchDoctor.SoulHarvest.CanCast() && IdealSoulHarvestCriteria(Enemies.CloseNearby))
                    {
                        Skills.WitchDoctor.SoulHarvest.Cast();
                    }
                },
                OnFinished = m =>
                {
                    if (Skills.WitchDoctor.SoulHarvest.CanCast() && MinimumSoulHarvestCriteria(Enemies.CloseNearby))
                    {
                        Skills.WitchDoctor.SoulHarvest.Cast();
                    }
                },
                StopCondition = m =>
                    SoulHarvestBestCluster == null || CurrentTarget == null || !CurrentTarget.IsUnit
                ,
                Options = new QueuedMovementOptions
                {
                    Logging = LogLevel.Verbose,
                    AcceptableDistance = 1f,
                    Type = MoveType.SpecialCombat,
                }
            });
        }

        private static void MoveToHorrifyPoint(TargetArea area)
        {
            QueuedMovement.Queue(new QueuedMovement
            {
                Name = "Horrify Position",
                Destination = area.Position,
                OnUpdate = m =>
                {
                    // Only change destination if the new target is way better
                    if (TikHorrifyCriteria(Enemies.BestLargeCluster) &&
                        Enemies.BestLargeCluster.Position.Distance(m.Destination) > 15f)
                        m.Destination = Enemies.BestLargeCluster.Position;
                },
                Options = new QueuedMovementOptions
                {
                    AcceptableDistance = 12f,
                    Logging = LogLevel.Verbose,
                    ChangeInDistanceLimit = 2f,
                    SuccessBlacklistSeconds = 3,
                    FailureBlacklistSeconds = 7,
                    TimeBeforeBlocked = 500
                }

            });
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

        private static TrinityPower DestroyObjectPower
        {
            get
            {
                if (Hotbar.Contains(SNOPower.Witchdoctor_Firebomb))
                    return new TrinityPower(SNOPower.Witchdoctor_Firebomb, 12f, CurrentTarget.Position);
                if (Hotbar.Contains(SNOPower.Witchdoctor_PoisonDart))
                    return new TrinityPower(SNOPower.Witchdoctor_PoisonDart, 15f, CurrentTarget.Position);
                if (CanCast(SNOPower.Witchdoctor_ZombieCharger))
                    return new TrinityPower(SNOPower.Witchdoctor_ZombieCharger, 12f, CurrentTarget.Position);
                if (Hotbar.Contains(SNOPower.Witchdoctor_CorpseSpider))
                    return new TrinityPower(SNOPower.Witchdoctor_CorpseSpider, 12f, CurrentTarget.Position);
                if (Hotbar.Contains(SNOPower.Witchdoctor_PlagueOfToads))
                    return new TrinityPower(SNOPower.Witchdoctor_PlagueOfToads, 12f, CurrentTarget.Position);
                if (CanCast(SNOPower.Witchdoctor_AcidCloud))
                    return new TrinityPower(SNOPower.Witchdoctor_AcidCloud, 12f, CurrentTarget.Position);

                if (Hotbar.Contains(SNOPower.Witchdoctor_Sacrifice) && Hotbar.Contains(SNOPower.Witchdoctor_SummonZombieDog) &&
                    Trinity.PlayerOwnedZombieDogCount > 0 && Settings.Combat.WitchDoctor.ZeroDogs)
                    return new TrinityPower(SNOPower.Witchdoctor_Sacrifice, 12f, CurrentTarget.Position);

                if (CanCast(SNOPower.Witchdoctor_SpiritBarrage))
                    return new TrinityPower(SNOPower.Witchdoctor_SpiritBarrage, 12f, CurrentTarget.Position);

                return DefaultPower;
            }
        }

    }
}
