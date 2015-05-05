using System;
using System.Collections.Generic;
using System.Linq;
using Trinity.Config.Combat;
using Trinity.Helpers;
using Trinity.Objects;
using Trinity.Reference;
using Zeta.Common;
using Zeta.Game.Internals.Actors;
using Trinity.Technicals;
using Logger = Trinity.Technicals.Logger;

namespace Trinity.Combat.Abilities
{
    public class DemonHunterCombat : CombatBase
    {
        static DemonHunterCombat()
        {
            SkillUtils.SetSkillMeta(SkillsDefaultMeta.DemonHunter.ToList());
            SetConditions();
        }

        internal override void CombatSettings()
        {
        }

        /// <summary>
        /// Main method for selecting a power
        /// </summary>
        public static TrinityPower GetPower()
        {
            TrinityPower power;

            if (UseDestructiblePower && TryGetPower(GetDestructablesSkill(), out power))
                return power;

            if (IsCurrentlyAvoiding && TryGetPower(GetAvoidanceSkill(), out power))
                return power;
   
            if (CurrentTarget == null)
            {
                // Out of Combat Buffs
                if (!IsCurrentlyAvoiding && !Player.IsInTown && TryGetPower(GetBuffSkill(), out power))
                    return power;  
            }
            else
            {
                // Use Generator for the Bastians Ring Set buff
                if (ShouldRefreshBastiansGeneratorBuff && TryGetPower(GetAttackGenerator(), out power))
                    return power;

                // Use Spender for the Bastians Ring Set buff
                if (ShouldRefreshBastiansSpenderBuff && TryGetPower(GetAttackSpender(), out power))
                    return power;

                // Main ability selection 
                if (TryGetPower(GetCombatPower(CombatSkillOrder), out power))
                    return power;                
            }

            Logger.Log(TrinityLogLevel.Verbose, LogCategory.SkillSelection, Player.ActorClass + " GetPower() Returning DefaultPower Target={0}",
                (CurrentTarget == null) ? "Null" : CurrentTarget.InternalName);

            return DefaultPower;
        }

        /// <summary>
        /// The combat skills and the order they should be evaluated in
        /// </summary>
        private static List<Skill> CombatSkillOrder
        {
            get
            {
                return new List<Skill>
                {
                    //Buffs
                    Skills.DemonHunter.Vengeance,
                    Skills.DemonHunter.ShadowPower,
                    Skills.DemonHunter.SmokeScreen,
                    Skills.DemonHunter.Preparation,
                    Skills.DemonHunter.Companion,

                    // Cooldown only
                    Skills.DemonHunter.RainOfVengeance,

                    // Spenders
                    Skills.DemonHunter.Sentry,
                    Skills.DemonHunter.Caltrops,
                    Skills.DemonHunter.MarkedForDeath,
                    Skills.DemonHunter.Vault,
                    Skills.DemonHunter.FanOfKnives,
                    Skills.DemonHunter.Multishot,
                    Skills.DemonHunter.Strafe,
                    Skills.DemonHunter.SpikeTrap,
                    Skills.DemonHunter.ClusterArrow,
                    Skills.DemonHunter.RapidFire,
                    Skills.DemonHunter.Impale,
                    Skills.DemonHunter.Chakram,
                    Skills.DemonHunter.ElementalArrow,

                    // Generators
                    Skills.DemonHunter.EvasiveFire,
                    Skills.DemonHunter.HungeringArrow,
                    Skills.DemonHunter.EntanglingShot,
                    Skills.DemonHunter.Bolas,
                    Skills.DemonHunter.Grenade,
                };
            }
        }

        /// <summary>
        /// When skills should be cast, evaluated by CanCast() calls.
        /// </summary>
        public static void SetConditions()
        {
            Skills.DemonHunter.RainOfVengeance.Meta.CastCondition = RainOfVengeanceCondition;
            Skills.DemonHunter.Vengeance.Meta.CastCondition = VengeanceCondition;
            Skills.DemonHunter.ShadowPower.Meta.CastCondition = ShadowPowerCondition;
            Skills.DemonHunter.SmokeScreen.Meta.CastCondition = SmokeScreenCondition;
            Skills.DemonHunter.Preparation.Meta.CastCondition = PreperationCondition;
            Skills.DemonHunter.Sentry.Meta.CastCondition = SentryCondition;
            Skills.DemonHunter.Caltrops.Meta.CastCondition = CaltropsCondition;
            Skills.DemonHunter.Companion.Meta.CastCondition = CompanionCondition;
            Skills.DemonHunter.MarkedForDeath.Meta.CastCondition = MarkedForDeathCondition;
            Skills.DemonHunter.Vault.Meta.CastCondition = VaultCondition;
            Skills.DemonHunter.FanOfKnives.Meta.CastCondition = FanOfKnivesCondition;
            Skills.DemonHunter.Multishot.Meta.CastCondition = MultiShotCondition;
            Skills.DemonHunter.Strafe.Meta.CastCondition = StrafeCondition;
            Skills.DemonHunter.SpikeTrap.Meta.CastCondition = SpikeTrapCondition;
            Skills.DemonHunter.ElementalArrow.Meta.CastCondition = ElementalArrowCondition;
            Skills.DemonHunter.ClusterArrow.Meta.CastCondition = ClusterArrowCondition;
            Skills.DemonHunter.Chakram.Meta.CastCondition = ChakramCondition;
            Skills.DemonHunter.RapidFire.Meta.CastCondition = RapidFireCondition;
            Skills.DemonHunter.Impale.Meta.CastCondition = ImpaleCondition;
            Skills.DemonHunter.EvasiveFire.Meta.CastCondition = EvasiveFireCondition;
            Skills.DemonHunter.HungeringArrow.Meta.CastCondition = HungeringArrowCondition;
            Skills.DemonHunter.EntanglingShot.Meta.CastCondition = EntanglingShotCondition;
            Skills.DemonHunter.Bolas.Meta.CastCondition = BolasCondition;
            Skills.DemonHunter.Grenade.Meta.CastCondition = GrenadeCondition;
        }

        /// <summary>
        /// When Grenade should be cast
        /// </summary>
        private static bool GrenadeCondition(SkillMeta meta)
        {
            meta.CastRange = 40f;
            return true;
        }

        /// <summary>
        /// When Bolas should be cast
        /// </summary>
        private static bool BolasCondition(SkillMeta meta)
        {
            meta.CastRange = 50f;
            return true;
        }

        /// <summary>
        /// When Entangling Shot should be cast
        /// </summary>
        private static bool EntanglingShotCondition(SkillMeta meta)
        {
            meta.CastRange = 50f;
            return true;
        }

        /// <summary>
        /// When Hungering Arrow should be cast
        /// </summary>
        private static bool HungeringArrowCondition(SkillMeta meta)
        {
            meta.CastRange = 50f;
            return true;
        }

        /// <summary>
        /// When Evasive fire should be cast
        /// </summary>
        /// <param name="meta"></param>
        /// <returns></returns>
        private static bool EvasiveFireCondition(SkillMeta meta)
        {
            meta.CastRange = 16f;
            return TargetUtil.AnyMobsInRange(50f);
        }

        /// <summary>
        /// When Impale should be cast
        /// </summary>
        private static bool ImpaleCondition(SkillMeta meta)
        {
            meta.CastRange = 80f;

            // Not enough resource
            if (Player.PrimaryResource <= EnergyReserve)
                return false;

            if (!TargetUtil.AnyMobsInRange(12, 4) && CurrentTarget.RadiusDistance <= 75f)
                return true;
            
            return false;
        }

        /// <summary>
        /// When Rapid Fire should be cast
        /// </summary>
        /// <param name="meta"></param>
        /// <returns></returns>
        private static bool RapidFireCondition(SkillMeta meta)
        {
            meta.CastFlags = CanCastFlags.NoTimer;
            meta.CastRange = 45f;

            // Stay above minimum resource level
            if (Player.PrimaryResource < EnergyReserve || Player.PrimaryResource < Settings.Combat.DemonHunter.RapidFireMinHatred)
                return false;

            // Never use it twice in a row
            if (LastPowerUsed == SNOPower.DemonHunter_RapidFire)
                return false;

            return true;
        }

        /// <summary>
        /// When Chakram should be cast.
        /// </summary>
        private static bool ChakramCondition(SkillMeta meta)
        {
            meta.CastRange = 50f;

            // Spam it for Shuriken Cloud buff
            if (Runes.DemonHunter.ShurikenCloud.IsActive && TimeSincePowerUse(SNOPower.DemonHunter_Chakram) >= 110000 &&
                ((Player.PrimaryResource >= 10 && !IsWaitingForSpecial) || Player.PrimaryResource >= MinEnergyReserve))
                return true;

            // Always cast with Spines of Seething Hatred rune, grants 4 hatred
            if (Legendary.SpinesOfSeethingHatred.IsEquipped)
                return true;

            // Monsters nearby
            if (TargetUtil.ClusterExists(45f,4))
                return true;       

            return false;
        }

        /// <summary>
        /// When Cluster Arrow should be cast
        /// </summary>
        private static bool ClusterArrowCondition(SkillMeta meta)
        {
            meta.CastRange = 85f;

            // Natalyas - Wait for damage buff
            if (Sets.NatalyasVengeance.IsFullyEquipped && Player.PrimaryResource < 100 && !CacheData.Buffs.HasBuff(SNOPower.P2_ItemPassive_Unique_Ring_053))
                return false;

            // Stay above minimum resource level
            if (Player.PrimaryResource < EnergyReserve)
                return false;

            return true;
        }

        /// <summary>
        /// When Elemental Arrow should be cast
        /// </summary>
        private static bool ElementalArrowCondition(SkillMeta meta)
        {
            meta.CastRange = 100f;

            // Stay above minimum resource level
            if (Player.PrimaryResource < EnergyReserve && !Legendary.Kridershot.IsEquipped)
                return false;

            // Lightning DH
            if (Runes.DemonHunter.BallLightning.IsActive && Legendary.MeticulousBolts.IsEquipped)
                meta.CastRange = 15f;

            // Kridershot
            if (Legendary.Kridershot.IsEquipped)
                meta.CastRange = 65f;

            return true;
        }

        /// <summary>
        /// When spike trap should be cast
        /// </summary>
        private static bool SpikeTrapCondition(SkillMeta meta)
        {
            meta.TargetPositionSelector = SpikeTrapTargetSelector;

            if (LastPowerUsed != SNOPower.DemonHunter_SpikeTrap)
                return true;

            return false;
        }

        /// <summary>
        /// Where Spike trap should be cast
        /// </summary>
        private static Vector3 SpikeTrapTargetSelector(SkillMeta skillMeta)
        {
            // For distant monsters, try to target a little bit in-front of them (as they run towards us), if it's not a treasure goblin
            float reducedDistance = 0f;
            if (CurrentTarget.Distance > 17f && !CurrentTarget.IsTreasureGoblin)
            {
                reducedDistance = CurrentTarget.Distance - 17f;
                if (reducedDistance > 5f)
                    reducedDistance = 5f;
            }
            return MathEx.CalculatePointFrom(CurrentTarget.Position, Player.Position, CurrentTarget.Distance - reducedDistance);
        }

        /// <summary>
        /// When Strafe should be cast
        /// </summary>
        private static bool StrafeCondition(SkillMeta meta)
        {
            meta.CastRange = 65f;
            meta.ReUseDelay = 250;
            //meta.TargetPositionSelector = ret => TargetUtil.GetZigZagTarget(CurrentTarget.Position, V.F("Barbarian.Whirlwind.ZigZagDistance"));
            meta.TargetPositionSelector = ret => NavHelper.FindSafeZone(false, 0, CurrentTarget.Position, true, Trinity.ObjectCache, false);
            meta.CastFlags = CanCastFlags.NoTimer;
            meta.RequiredResource = Settings.Combat.DemonHunter.StrafeMinHatred;

            if (!Player.IsRooted)
                return true;

            return false;
        }

        /// <summary>
        /// When Multishot should be cast
        /// </summary>
        private static bool MultiShotCondition(SkillMeta meta)
        {
            meta.CastFlags = CanCastFlags.NoPowerManager;

            // Natalyas - Wait for damage buff
            if (Sets.NatalyasVengeance.IsFullyEquipped && Player.PrimaryResource < 100 && !CacheData.Buffs.HasBuff(SNOPower.P2_ItemPassive_Unique_Ring_053))
                return false;

            if (Sets.UnhallowedEssence.IsMaxBonusActive || TargetUtil.ClusterExists(45, 3))
                return true;
            
            return true;
        }

        /// <summary>
        /// When Fan of Knives should be cast
        /// </summary>
        private static bool FanOfKnivesCondition(SkillMeta meta)
        {
            meta.CastRange = 25f;

            if (TargetUtil.EliteOrTrashInRange(15) || TargetUtil.AnyTrashInRange(15f, 5, false))
                return true;

            return false;
        }

        /// <summary>
        /// When Vault should be cast
        /// </summary>
        private static bool VaultCondition(SkillMeta meta)
        {
            meta.CastRange = 20f;
            meta.TargetPositionSelector = ret => NavHelper.MainFindSafeZone(Player.Position, true);            
            meta.RequiredResource = Hotbar.Contains(SNOPower.DemonHunter_ShadowPower) ? 22 : 16;
            meta.ReUseDelay = Settings.Combat.DemonHunter.VaultMovementDelay;

            if (Settings.Combat.DemonHunter.VaultMode == DemonHunterVaultMode.MovementOnly && IsInCombat)
                return false;

            if (Settings.Combat.DemonHunter.VaultMode == DemonHunterVaultMode.CombatOnly && !IsInCombat)
                return false;

            if (!Player.IsRooted && (TargetUtil.AnyMobsInRange(7f, 6) || Player.CurrentHealthPct <= 0.7))
                return true;

            return false;
        }

        /// <summary>
        /// When Marked for Death should be cast
        /// </summary>
        private static bool MarkedForDeathCondition(SkillMeta meta)
        {
            meta.CastRange = 100f;
            meta.CastFlags = CanCastFlags.NoTimer;

            if (!CurrentTarget.HasDebuff(SNOPower.DemonHunter_MarkedForDeath) && !SpellTracker.IsUnitTracked(CurrentTarget, SNOPower.DemonHunter_MarkedForDeath))                
                return true;

            return false;
        }

        /// <summary>
        /// When Companion should be cast
        /// </summary>
        private static bool CompanionCondition(SkillMeta meta)
        {
            meta.CastFlags = CanCastFlags.NoTimer;

            // Use Spider Slow on 4 or more trash mobs in an area or on Unique/Elite/Champion
            if (Runes.DemonHunter.SpiderCompanion.IsActive && TargetUtil.ClusterExists(25f, 4) && TargetUtil.EliteOrTrashInRange(25f))
                return true;

            //Use Bat when Hatred is Needed
            if (Runes.DemonHunter.BatCompanion.IsActive && Player.PrimaryResourceMissing >= 60)
                return true;

            // Use Boar Taunt on 3 or more trash mobs in an area or on Unique/Elite/Champion
            if (Runes.DemonHunter.BoarCompanion.IsActive && ((TargetUtil.ClusterExists(20f, 4) && TargetUtil.EliteOrTrashInRange(20f)) || (CurrentTarget.IsBossOrEliteRareUnique && CurrentTarget.Distance <= 20f)))
                return true;

            // Ferrets used for picking up Health Globes when low on Health
            if (Runes.DemonHunter.FerretCompanion.IsActive && Trinity.ObjectCache.Any(o => o.Type == TrinityObjectType.HealthGlobe && o.Distance < 60f) && Player.CurrentHealthPct < EmergencyHealthPotionLimit)
                return true;

            // Use Wolf Howl on Unique/Elite/Champion - Would help for farming trash, but trash farming should not need this - Used on Elites to reduce Deaths per hour
            if (Runes.DemonHunter.WolfCompanion.IsActive && (TargetUtil.AnyElitesInRange(100f) || TargetUtil.AnyMobsInRange(40, 8)))            
                return true;

            // Companion off CD
            if (Settings.Combat.DemonHunter.CompanionOffCooldown && TargetUtil.AnyMobsInRange(60))
                return true;
            
            return false;
        }

        /// <summary>
        /// When Caltrops should be cast
        /// </summary>
        private static bool CaltropsCondition(SkillMeta meta)
        {        
            return TargetUtil.AnyMobsInRange(40) && !GetHasBuff(SNOPower.DemonHunter_Caltrops);
        }

        /// <summary>
        /// When Sentry should be cast
        /// </summary>
        /// <param name="meta"></param>
        /// <returns></returns>
        private static bool SentryCondition(SkillMeta meta)
        {
            meta.CastRange = 80f;
            meta.CastFlags = CanCastFlags.NoTimer;

            if (TargetUtil.AnyMobsInRange(65) && Trinity.PlayerOwnedDHSentryCount < MaxSentryCount)
                return true;

            return false;
        }

        /// <summary>
        /// When Rain of Vengeance should be cast
        /// </summary>
        private static bool RainOfVengeanceCondition(SkillMeta meta)
        {
            meta.CastRange = 90f;            
            meta.CastFlags = CanCastFlags.NoTimer;

            if (Legendary.CrashingRain.IsEquipped)
                meta.TargetPositionSelector = skillMeta => TargetUtil.GetBestClusterPoint(30f, 80f); 

            if (Settings.Combat.DemonHunter.RainOfVengeanceOffCD || Sets.NatalyasVengeance.IsEquipped)
                return true;

            if (TargetUtil.ClusterExists(45f, 4) || TargetUtil.AnyElitesInRange(90f))
                return true;

            return false;
        }

        /// <summary>
        /// When Preperation should be cast
        /// </summary>
        private static bool PreperationCondition(SkillMeta meta)
        {
            meta.ReUseDelay = Runes.DemonHunter.FocusedMind.IsActive ? 15000 : 500;
            meta.CastFlags = CanCastFlags.NoTimer;
            
            if (!Runes.DemonHunter.Punishment.IsActive && Player.SecondaryResource <= V.F("DemonHunter.MinPreparationDiscipline"))
                return true;

            if (Runes.DemonHunter.Punishment.IsActive && Player.PrimaryResource <= 75 && (TargetUtil.AnyElitesInRange(50f) || Enemies.Nearby.UnitCount > 5))
                return true;

            return false;
        }

        /// <summary>
        /// When Smoke Screen should be cast
        /// </summary>
        private static bool SmokeScreenCondition(SkillMeta meta)
        {
            meta.CastFlags = CanCastFlags.NoTimer;

            // Buff Already Active
            if (GetHasBuff(SNOPower.DemonHunter_ShadowPower))
                return false;

            // Mobs in range
            if (TargetUtil.AnyMobsInRange(15) || (Legendary.MeticulousBolts.IsEquipped && TargetUtil.AnyMobsInRange(60)))
                return true;

            // Defensive Cast
            if((Player.CurrentHealthPct <= 0.50 || Player.IsRooted || Player.IsIncapacitated))
                return true;

            // Spam Setting
            if (Settings.Combat.DemonHunter.SpamSmokeScreen)
                return true;

            return false;
        }

        /// <summary>
        /// When Shadow Power should be cast
        /// </summary>
        private static bool ShadowPowerCondition(SkillMeta meta)
        {
            // Buff Already Active
            if(GetHasBuff(SNOPower.DemonHunter_ShadowPower))
                return false;

            // Not Enough Discipline
            if (Player.SecondaryResource < 14)
                return false;

            // Used Recently
            if (TimeSincePowerUse(SNOPower.DemonHunter_ShadowPower) < 4500)
                return false;

            // Low Health
            if(Player.CurrentHealthPct <= Trinity.PlayerEmergencyHealthPotionLimit && Player.SecondaryResource >= 14)
                return true;

            // Defensive Cast
            if(Player.IsRooted || TargetUtil.AnyMobsInRange(15))
                return true;

            // Spam Setting
            if(Settings.Combat.DemonHunter.SpamShadowPower)
                return true;

            return false;
        }

        /// <summary>
        /// When Vengeance should be cast
        /// </summary>
        private static bool VengeanceCondition(SkillMeta meta)
        {
            meta.CastFlags = CanCastFlags.NoTimer;

            if (!Settings.Combat.DemonHunter.VengeanceElitesOnly && TargetUtil.AnyMobsInRange(60f, 6))
                return true;

            if (TargetUtil.IsEliteTargetInRange(100f))
                return true;

            return false;
        }

        /// <summary>
        /// Maximum number of sentries allowed from Equipped items and Passives
        /// </summary>
        public static int MaxSentryCount 
        {
            get { return 2 + (Legendary.BombardiersRucksack.IsEquipped ? 2 : 0) + (Passives.DemonHunter.CustomEngineering.IsActive ? 1 : 0); }
        }

    }
}
