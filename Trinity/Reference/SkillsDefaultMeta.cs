using System;
using System.Collections.Generic;
using Trinity.Helpers;
using Trinity.Objects;

namespace Trinity.Reference
{
    /// <summary>
    /// Additional information about skills and how they are used in combat
    /// Store values here that are fixed / dont change based on items, build or combat situational factors.
    /// </summary>
    public static class SkillsDefaultMeta
    {
        static SkillsDefaultMeta()
        {
            LoadDefaults();
        }

        public static void LoadDefaults()
        {
            var all = new List<SkillMeta>();
            all.AddRange(Monk.ToList());
            all.AddRange(WitchDoctor.ToList());
            all.AddRange(Crusader.ToList());
            all.AddRange(Barbarian.ToList());
            all.AddRange(DemonHunter.ToList());
            all.AddRange(Wizard.ToList());
            all.ForEach(si => si.Overrides(si.Skill, si));
            SkillUtils.SetSkillMeta(all);            
        }

        #region Monk Default Skill Settings

        public class Monk : FieldCollection<Monk, SkillMeta>
        {

            public static SkillMeta FistsOfThunder = new SkillMeta
            {

            };

            public static SkillMeta LashingTailKick = new SkillMeta
            {

            };

            public static SkillMeta DeadlyReach = new SkillMeta
            {

            };

            public static SkillMeta BlindingFlash = new SkillMeta
            {

            };

            public static SkillMeta TempestRush = new SkillMeta
            {

            };

            public static SkillMeta BreathOfHeaven = new SkillMeta
            {

            };

            public static SkillMeta DashingStrike = new SkillMeta
            {

            };

            public static SkillMeta CripplingWave = new SkillMeta
            {

            };

            public static SkillMeta WaveOfLight = new SkillMeta
            {

            };

            public static SkillMeta ExplodingPalm = new SkillMeta
            {

            };

            public static SkillMeta CycloneStrike = new SkillMeta
            {

            };

            public static SkillMeta WayOfTheHundredFists = new SkillMeta
            {

            };

            public static SkillMeta Serenity = new SkillMeta
            {

            };

            public static SkillMeta SevensidedStrike = new SkillMeta
            {

            };

            public static SkillMeta MantraOfSalvation = new SkillMeta
            {

            };

            public static SkillMeta SweepingWind = new SkillMeta
            {

            };

            public static SkillMeta MantraOfRetribution = new SkillMeta
            {

            };

            public static SkillMeta InnerSanctuary = new SkillMeta
            {

            };

            public static SkillMeta MysticAlly = new SkillMeta
            {

            };

            public static SkillMeta MantraOfHealing = new SkillMeta
            {

            };

            public static SkillMeta MantraOfConviction = new SkillMeta
            {

            };

            public static SkillMeta Epiphany = new SkillMeta
            {

            };
        }

        #endregion

        #region WitchDoctor Default Skill Settings

        public class WitchDoctor : FieldCollection<WitchDoctor, SkillMeta>
        {

            public static SkillMeta PoisonDart = new SkillMeta
            {

            };

            public static SkillMeta GraspOfTheDead = new SkillMeta
            {

            };

            public static SkillMeta CorpseSpiders = new SkillMeta
            {

            };

            public static SkillMeta SummonZombieDogs = new SkillMeta
            {

            };

            public static SkillMeta Firebats = new SkillMeta
            {

            };

            public static SkillMeta Horrify = new SkillMeta
            {

            };

            public static SkillMeta SoulHarvest = new SkillMeta
            {

            };

            public static SkillMeta PlagueOfToads = new SkillMeta
            {

            };

            public static SkillMeta Haunt = new SkillMeta
            {

            };

            public static SkillMeta Sacrifice = new SkillMeta
            {

            };

            public static SkillMeta ZombieCharger = new SkillMeta
            {

            };

            public static SkillMeta SpiritWalk = new SkillMeta
            {

            };

            public static SkillMeta SpiritBarrage = new SkillMeta
            {

            };

            public static SkillMeta Gargantuan = new SkillMeta
            {

            };

            public static SkillMeta LocustSwarm = new SkillMeta
            {

            };

            public static SkillMeta Firebomb = new SkillMeta
            {

            };

            public static SkillMeta Hex = new SkillMeta
            {

            };

            public static SkillMeta AcidCloud = new SkillMeta
            {

            };

            public static SkillMeta MassConfusion = new SkillMeta
            {

            };

            public static SkillMeta BigBadVoodoo = new SkillMeta
            {

            };

            public static SkillMeta WallOfZombies = new SkillMeta
            {

            };

            public static SkillMeta FetishArmy = new SkillMeta
            {

            };

            public static SkillMeta Piranhas = new SkillMeta
            {

            };

            
        }

        #endregion

        #region Crusader Default Skill Settings

        public class Crusader : FieldCollection<Crusader, SkillMeta>
        {

            public static SkillMeta Punish = new SkillMeta
            {

            };

            public static SkillMeta ShieldBash = new SkillMeta
            {

            };

            public static SkillMeta Slash = new SkillMeta
            {

            };

            public static SkillMeta ShieldGlare = new SkillMeta
            {

            };

            public static SkillMeta SweepAttack = new SkillMeta
            {

            };

            public static SkillMeta IronSkin = new SkillMeta
            {

            };

            public static SkillMeta Provoke = new SkillMeta
            {

            };

            public static SkillMeta Smite = new SkillMeta
            {

            };

            public static SkillMeta BlessedHammer = new SkillMeta
            {

            };

            public static SkillMeta SteedCharge = new SkillMeta
            {

            };

            public static SkillMeta LawsOfValor = new SkillMeta
            {

            };

            public static SkillMeta Justice = new SkillMeta
            {

            };

            public static SkillMeta Consecration = new SkillMeta
            {

            };

            public static SkillMeta LawsOfJustice = new SkillMeta
            {

            };

            public static SkillMeta FallingSword = new SkillMeta
            {

            };

            public static SkillMeta BlessedShield = new SkillMeta
            {

            };

            public static SkillMeta Condemn = new SkillMeta
            {

            };

            public static SkillMeta Judgment = new SkillMeta
            {

            };

            public static SkillMeta LawsOfHope = new SkillMeta
            {

            };

            public static SkillMeta AkaratsChampion = new SkillMeta
            {

            };

            public static SkillMeta FistOfTheHeavens = new SkillMeta
            {

            };

            public static SkillMeta Phalanx = new SkillMeta
            {

            };

            public static SkillMeta HeavensFury = new SkillMeta
            {

            };

            public static SkillMeta Bombardment = new SkillMeta
            {

            };

        }

        #endregion

        #region Barbarian Default Skill Settings

        public class Barbarian : FieldCollection<Barbarian, SkillMeta>
        {


            public static SkillMeta Bash = new SkillMeta
            {

            };

            public static SkillMeta HammerOfTheAncients = new SkillMeta
            {

            };

            public static SkillMeta Cleave = new SkillMeta
            {

            };

            public static SkillMeta GroundStomp = new SkillMeta
            {

            };

            public static SkillMeta Rend = new SkillMeta
            {

            };

            public static SkillMeta Leap = new SkillMeta
            {

            };

            public static SkillMeta Overpower = new SkillMeta
            {

            };

            public static SkillMeta Frenzy = new SkillMeta
            {

            };

            public static SkillMeta SeismicSlam = new SkillMeta
            {

            };

            public static SkillMeta Revenge = new SkillMeta
            {

            };

            public static SkillMeta ThreateningShout = new SkillMeta
            {

            };

            public static SkillMeta Sprint = new SkillMeta
            {

            };

            public static SkillMeta WeaponThrow = new SkillMeta
            {

            };

            public static SkillMeta Earthquake = new SkillMeta
            {

            };

            public static SkillMeta Whirlwind = new SkillMeta
            {

            };

            public static SkillMeta FuriousCharge = new SkillMeta
            {

            };

            public static SkillMeta IgnorePain = new SkillMeta
            {

            };

            public static SkillMeta BattleRage = new SkillMeta
            {

            };

            public static SkillMeta CallOfTheAncients = new SkillMeta
            {

            };

            public static SkillMeta AncientSpear = new SkillMeta
            {

            };

            public static SkillMeta WarCry = new SkillMeta
            {

            };

            public static SkillMeta WrathOfTheBerserker = new SkillMeta
            {

            };

            public static SkillMeta Avalanche = new SkillMeta
            {

            };

        }

        #endregion

        #region DemonHunter Default Skill Settings

        public class DemonHunter : FieldCollection<DemonHunter, SkillMeta>
        {

            public static SkillMeta HungeringArrow = new SkillMeta(Skills.DemonHunter.HungeringArrow)
            {
                IsCombatOnly = true,
                IsOffensiveSkill = true,
                CastRange = 50f,
                MaxTargetDistance = 100,
                IsAreaEffectSkill = true,
                AreaEffectType = AreaEffectShape.Beam,

                Overrides = (skill, info) =>
                {
                    if (skill.CurrentRune == Runes.DemonHunter.ShatterShot)
                        info.AreaEffectType = AreaEffectShape.Cone;

                    if (skill.CurrentRune == Runes.DemonHunter.SprayOfTeeth)
                        info.AreaEffectType = AreaEffectShape.Circle;                                                
                }
            };

            public static SkillMeta Impale = new SkillMeta(Skills.DemonHunter.Impale)
            {
                IsCombatOnly = true,
                IsOffensiveSkill = true,
                CastRange = 80f,

                Overrides = (skill, info) =>
                {
                    if (skill.CurrentRune == Runes.DemonHunter.Overpenetration)
                    {
                        info.IsAreaEffectSkill = true;
                        info.AreaEffectType = AreaEffectShape.Beam;
                    }
                }
            };

            public static SkillMeta EntanglingShot = new SkillMeta(Skills.DemonHunter.EntanglingShot)
            {
                IsCombatOnly = true,
                IsOffensiveSkill = true,
                CastRange = 50f,
                MaxTargetDistance = 100f,
                IsAreaEffectSkill = true,
                AreaEffectType = AreaEffectShape.Circle,

                Overrides = (skill,info) =>
                {
                    skill.AreaEffectRadius = 12f;
                }
            };

            public static SkillMeta Caltrops = new SkillMeta(Skills.DemonHunter.Caltrops)
            {
                IsDefensiveSkill = true,
                IsCastOnSelf = true,
                IsAreaEffectSkill = true,
                AreaEffectType = AreaEffectShape.Circle
            };

            public static SkillMeta RapidFire = new SkillMeta(Skills.DemonHunter.RapidFire)
            {
                IsCombatOnly = true,
                IsOffensiveSkill = true,
                CastRange = 45f,
                MaxTargetDistance = 100f,

                Overrides = (skill, info) =>
                {
                    if (skill.CurrentRune == Runes.DemonHunter.HighVelocity)
                    {
                        info.IsAreaEffectSkill = true;
                        info.AreaEffectType = AreaEffectShape.Beam;
                    }

                    if (skill.CurrentRune == Runes.DemonHunter.Bombardment)
                    {
                        info.IsAreaEffectSkill = true;
                        info.AreaEffectType = AreaEffectShape.Circle;
                    }                        
                }
            };

            public static SkillMeta SmokeScreen = new SkillMeta(Skills.DemonHunter.SmokeScreen)
            {
                IsCombatOnly = true,
                IsDefensiveSkill = true,
                IsCastOnSelf = true
            };

            public static SkillMeta Vault = new SkillMeta(Skills.DemonHunter.Vault)
            {
                IsMovementSkill = true,
                IsDefensiveSkill = true,
                IsAvoidanceSkill = true,
                CastRange = 20f,
            };

            public static SkillMeta Bolas = new SkillMeta(Skills.DemonHunter.Bolas)
            {
                IsCombatOnly = true,
                IsOffensiveSkill = true,
                CastRange = 50f,
                MaxTargetDistance = 85f,
                AreaEffectType = AreaEffectShape.Circle,

                Overrides = (skill, info) =>
                {
                    if (skill.CurrentRune == Runes.DemonHunter.FreezingStrike)
                    {
                        info.IsAreaEffectSkill = false;
                        info.AreaEffectType = AreaEffectShape.None;
                    }
                }
            };

            public static SkillMeta Chakram = new SkillMeta(Skills.DemonHunter.Chakram)
            {
                IsCombatOnly = true,
                IsOffensiveSkill = true,
                IsAreaEffectSkill = true,
                CastRange = 50f,
                MaxTargetDistance = 100f,

                Overrides = (skill, info) =>
                {
                    if (skill.CurrentRune == Runes.DemonHunter.TwinChakrams)
                    {
                        skill.AreaEffectRadius = 45f;                        
                        info.AreaEffectType = AreaEffectShape.Beam;                        
                    }

                    if (skill.CurrentRune == Runes.DemonHunter.Serpentine || skill.CurrentRune == Runes.DemonHunter.Boomerang)
                    {
                        skill.AreaEffectRadius = 20f;
                        info.AreaEffectType = AreaEffectShape.Beam;
                    }

                    if (skill.CurrentRune == Runes.DemonHunter.ShurikenCloud)
                    {
                        skill.AreaEffectRadius = 20f;
                        info.IsCastOnSelf = true;                        
                        info.AreaEffectType = AreaEffectShape.Circle;
                    }

                    if (skill.CurrentRune == Runes.DemonHunter.RazorDisk)
                    {
                        skill.AreaEffectRadius = 50f;
                        info.AreaEffectType = AreaEffectShape.Circle;
                    }
                }
            };

            public static SkillMeta Preparation = new SkillMeta(Skills.DemonHunter.Preparation)
            {
                IsCombatOnly = true,
                IsBuffingSkill = true,
                IsCastOnSelf = true,
            };

            public static SkillMeta FanOfKnives = new SkillMeta(Skills.DemonHunter.FanOfKnives)
            {
                IsCombatOnly = true,
                IsOffensiveSkill = true,
                IsCastOnSelf = true,
                CastRange = 25f
            };

            public static SkillMeta EvasiveFire = new SkillMeta(Skills.DemonHunter.EvasiveFire)
            {
                IsCombatOnly = true,
                IsAvoidanceSkill = true,
                IsOffensiveSkill = true,
                IsDefensiveSkill = true,
                IsAreaEffectSkill = true,
                AreaEffectType = AreaEffectShape.Cone,
                CastRange = 20f,
                MaxTargetDistance = 30f,

                Overrides = (skill, info) =>
                {
                    if (skill.CurrentRune == Runes.DemonHunter.Focus)
                    {
                        info.IsAvoidanceSkill = false;
                        info.IsDefensiveSkill = false;
                    }
                }
            };

            public static SkillMeta Grenade = new SkillMeta(Skills.DemonHunter.Grenade)
            {
                IsCombatOnly = true,
                IsOffensiveSkill = true,
                IsAreaEffectSkill = true,
                AreaEffectType = AreaEffectShape.Circle,
                CastRange = 40f,
                MaxTargetDistance = 100f,

                Overrides = (skill, info) =>
                {
                    if (skill.CurrentRune == Runes.DemonHunter.GrenadeCache)
                    {
                        info.AreaEffectType = AreaEffectShape.Cone;
                    }
                }
            };

            public static SkillMeta ShadowPower = new SkillMeta(Skills.DemonHunter.ShadowPower)
            {
                IsCombatOnly = true,
                IsCastOnSelf = true,
                IsBuffingSkill = true,
                IsDefensiveSkill = true,
            };

            public static SkillMeta SpikeTrap = new SkillMeta(Skills.DemonHunter.SpikeTrap)
            {
                IsCombatOnly = true,
                IsDefensiveSkill = true,
                IsCastOnSelf = true,
                IsAreaEffectSkill = true,
                AreaEffectType = AreaEffectShape.Circle
            };

            public static SkillMeta Companion = new SkillMeta(Skills.DemonHunter.Companion)
            {
                IsBuffingSkill = true,
                IsCastOnSelf = true,
                IsOffensiveSkill = true,
            };

            public static SkillMeta Strafe = new SkillMeta(Skills.DemonHunter.Strafe)
            {
                IsCombatOnly = true,
                IsOffensiveSkill = true,
                CastRange = 65f,

                Overrides = (skill, info) =>
                {
                    if (skill.CurrentRune == Runes.DemonHunter.Demolition)
                    {
                        info.IsAreaEffectSkill = true;
                        info.AreaEffectType = AreaEffectShape.Circle;
                    }
                }
            };

            public static SkillMeta ElementalArrow = new SkillMeta(Skills.DemonHunter.ElementalArrow)
            {
                IsCombatOnly = true,
                IsOffensiveSkill = true,
                CastRange = 100f,
                IsAreaEffectSkill = true,
                AreaEffectType = AreaEffectShape.Beam,

                Overrides = (skill, info) =>
                {
                    skill.AreaEffectRadius = 8f;

                    if (skill.CurrentRune == Runes.DemonHunter.BallLightning)
                    {
                        skill.AreaEffectRadius = 17f;
                    }
                        
                    if (skill.CurrentRune == Runes.DemonHunter.FrostArrow)
                        info.AreaEffectType = AreaEffectShape.Cone;
                }
            };

            public static SkillMeta MarkedForDeath = new SkillMeta(Skills.DemonHunter.MarkedForDeath)
            {
                IsCombatOnly = true,
                IsOffensiveSkill = true,
                IsDebuffingSkill = true,
                CastRange = 100f,
            };

            public static SkillMeta Multishot = new SkillMeta(Skills.DemonHunter.Multishot)
            {
                IsCombatOnly = true,
                IsOffensiveSkill = true,
                IsAreaEffectSkill = true,
                AreaEffectType = AreaEffectShape.Cone,                
                CastRange = 70f,
                MaxTargetDistance = 90f,

                Overrides = (skill, info) =>
                {
                    skill.AreaEffectRadius = 90f;
                }
            };

            public static SkillMeta Sentry = new SkillMeta(Skills.DemonHunter.Sentry)
            {
                IsCombatOnly = true,
                IsOffensiveSkill = true,
                IsSummoningSkill = true,
                CastRange = 80f,
            };

            public static SkillMeta ClusterArrow = new SkillMeta(Skills.DemonHunter.ClusterArrow)
            {
                IsCombatOnly = true,
                IsOffensiveSkill = true,
                CastRange = 45f,
                MaxTargetDistance = 90f,
            };

            public static SkillMeta RainOfVengeance = new SkillMeta(Skills.DemonHunter.RainOfVengeance)
            {
                IsCombatOnly = true,
                IsOffensiveSkill = true,
                CastRange = 90f,
            };

            public static SkillMeta Vengeance = new SkillMeta(Skills.DemonHunter.Vengeance)
            {
                IsCombatOnly = true,
                IsBuffingSkill = true,
                IsCastOnSelf = true
            };

        }

        #endregion

        #region Wizard Default Skill Settings

        public class Wizard : FieldCollection<Wizard, SkillMeta>
        {

            public static SkillMeta MagicMissile = new SkillMeta
            {

            };

            public static SkillMeta RayOfFrost = new SkillMeta
            {

            };

            public static SkillMeta ShockPulse = new SkillMeta
            {

            };

            public static SkillMeta FrostNova = new SkillMeta
            {

            };

            public static SkillMeta ArcaneOrb = new SkillMeta
            {

            };

            public static SkillMeta DiamondSkin = new SkillMeta
            {

            };

            public static SkillMeta WaveOfForce = new SkillMeta
            {

            };

            public static SkillMeta SpectralBlade = new SkillMeta
            {

            };

            public static SkillMeta ArcaneTorrent = new SkillMeta
            {

            };

            public static SkillMeta EnergyTwister = new SkillMeta
            {

            };

            public static SkillMeta IceArmor = new SkillMeta
            {

            };

            public static SkillMeta Electrocute = new SkillMeta
            {

            };

            public static SkillMeta SlowTime = new SkillMeta
            {

            };

            public static SkillMeta StormArmor = new SkillMeta
            {

            };

            public static SkillMeta ExplosiveBlast = new SkillMeta
            {

            };

            public static SkillMeta MagicWeapon = new SkillMeta
            {

            };

            public static SkillMeta Hydra = new SkillMeta
            {

            };

            public static SkillMeta Disintegrate = new SkillMeta
            {

            };

            public static SkillMeta Familiar = new SkillMeta
            {

            };

            public static SkillMeta Teleport = new SkillMeta
            {

            };

            public static SkillMeta MirrorImage = new SkillMeta
            {

            };

            public static SkillMeta Meteor = new SkillMeta
            {

            };

            public static SkillMeta Blizzard = new SkillMeta
            {

            };

            public static SkillMeta EnergyArmor = new SkillMeta
            {

            };

            public static SkillMeta Archon = new SkillMeta
            {

            };

            public static SkillMeta BlackHole = new SkillMeta
            {


            };

        }

        #endregion

    }
}