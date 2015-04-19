using System;
using System.Collections.Generic;
using System.Linq;
using Trinity.Helpers;
using Trinity.Objects;
using Trinity.Technicals;
using Zeta.Game;

namespace Trinity.Reference
{
    /// <summary>
    /// This is a store for additional skill information.
    /// Only set default values for things that won't change based or situational factors.
    /// Anything that will change should go in the combat profile CastCondition instead.
    /// </summary>
    public class SkillsDefaultMeta
    {
        /// <summary>
        /// Applies collection of default SkillMeta to their skills
        /// </summary>
        private static void ApplyDefaults(List<SkillMeta> metas)
        {
            metas.ForEach(meta => meta.ApplyDefaults());           
        }

        #region Monk Default Skill Settings

        public class Monk : FieldCollection<Monk, SkillMeta>
        {
            static Monk()
            {
                ApplyDefaults(ToList());
            }

            public static SkillMeta FistsOfThunder = new SkillMeta(Skills.Monk.FistsOfThunder)
            {
                Defaults = (skill, meta) =>
                {

                }
            };

            public static SkillMeta LashingTailKick = new SkillMeta(Skills.Monk.LashingTailKick)
            {
                Defaults = (skill, meta) =>
                {

                }
            };

            public static SkillMeta DeadlyReach = new SkillMeta(Skills.Monk.DeadlyReach)
            {
                Defaults = (skill, meta) =>
                {

                }
            };

            public static SkillMeta BlindingFlash = new SkillMeta(Skills.Monk.BlindingFlash)
            {
                Defaults = (skill, meta) =>
                {

                }
            };

            public static SkillMeta TempestRush = new SkillMeta(Skills.Monk.TempestRush)
            {
                Defaults = (skill, meta) =>
                {

                }
            };

            public static SkillMeta BreathOfHeaven = new SkillMeta(Skills.Monk.BreathOfHeaven)
            {
                Defaults = (skill, meta) =>
                {

                }
            };

            public static SkillMeta DashingStrike = new SkillMeta(Skills.Monk.DashingStrike)
            {
                Defaults = (skill, meta) =>
                {

                }
            };

            public static SkillMeta CripplingWave = new SkillMeta(Skills.Monk.CripplingWave)
            {
                Defaults = (skill, meta) =>
                {

                }
            };

            public static SkillMeta WaveOfLight = new SkillMeta(Skills.Monk.WaveOfLight)
            {
                Defaults = (skill, meta) =>
                {

                }
            };

            public static SkillMeta ExplodingPalm = new SkillMeta(Skills.Monk.ExplodingPalm)
            {
                Defaults = (skill, meta) =>
                {

                }
            };

            public static SkillMeta CycloneStrike = new SkillMeta(Skills.Monk.CycloneStrike)
            {
                Defaults = (skill, meta) =>
                {

                }
            };

            public static SkillMeta WayOfTheHundredFists = new SkillMeta(Skills.Monk.WayOfTheHundredFists)
            {
                Defaults = (skill, meta) =>
                {

                }
            };

            public static SkillMeta Serenity = new SkillMeta(Skills.Monk.Serenity)
            {
                Defaults = (skill, meta) =>
                {

                }
            };

            public static SkillMeta SevensidedStrike = new SkillMeta(Skills.Monk.SevensidedStrike)
            {
                Defaults = (skill, meta) =>
                {

                }
            };

            public static SkillMeta MantraOfSalvation = new SkillMeta(Skills.Monk.MantraOfSalvation)
            {
                Defaults = (skill, meta) =>
                {

                }
            };

            public static SkillMeta SweepingWind = new SkillMeta(Skills.Monk.SweepingWind)
            {
                Defaults = (skill, meta) =>
                {

                }
            };

            public static SkillMeta MantraOfRetribution = new SkillMeta(Skills.Monk.MantraOfRetribution)
            {
                Defaults = (skill, meta) =>
                {

                }
            };

            public static SkillMeta InnerSanctuary = new SkillMeta(Skills.Monk.InnerSanctuary)
            {
                Defaults = (skill, meta) =>
                {

                }
            };

            public static SkillMeta MysticAlly = new SkillMeta(Skills.Monk.MysticAlly)
            {
                Defaults = (skill, meta) =>
                {

                }
            };

            public static SkillMeta MantraOfHealing = new SkillMeta(Skills.Monk.MantraOfHealing)
            {
                Defaults = (skill, meta) =>
                {

                }
            };

            public static SkillMeta MantraOfConviction = new SkillMeta(Skills.Monk.MantraOfConviction)
            {
                Defaults = (skill, meta) =>
                {

                }
            };

            public static SkillMeta Epiphany = new SkillMeta(Skills.Monk.Epiphany)
            {
                Defaults = (skill, meta) =>
                {

                }
            };
        }

        #endregion

        #region WitchDoctor Default Skill Settings

        public class WitchDoctor : FieldCollection<WitchDoctor, SkillMeta>
        {

            static WitchDoctor()
            {
                ApplyDefaults(ToList());
            }

            public static SkillMeta PoisonDart = new SkillMeta(Skills.WitchDoctor.PoisonDart)
            {
                Defaults = (skill, meta) =>
                {

                }
            };

            public static SkillMeta GraspOfTheDead = new SkillMeta(Skills.WitchDoctor.GraspOfTheDead)
            {
                Defaults = (skill, meta) =>
                {

                }
            };

            public static SkillMeta CorpseSpiders = new SkillMeta(Skills.WitchDoctor.CorpseSpiders)
            {
                Defaults = (skill, meta) =>
                {

                }
            };

            public static SkillMeta SummonZombieDogs = new SkillMeta(Skills.WitchDoctor.SummonZombieDogs)
            {
                Defaults = (skill, meta) =>
                {

                }
            };

            public static SkillMeta Firebats = new SkillMeta(Skills.WitchDoctor.Firebats)
            {
                Defaults = (skill, meta) =>
                {

                }
            };

            public static SkillMeta Horrify = new SkillMeta(Skills.WitchDoctor.Horrify)
            {
                Defaults = (skill, meta) =>
                {

                }
            };

            public static SkillMeta SoulHarvest = new SkillMeta(Skills.WitchDoctor.SoulHarvest)
            {
                Defaults = (skill, meta) =>
                {

                }
            };

            public static SkillMeta PlagueOfToads = new SkillMeta(Skills.WitchDoctor.PlagueOfToads)
            {
                Defaults = (skill, meta) =>
                {

                }
            };

            public static SkillMeta Haunt = new SkillMeta(Skills.WitchDoctor.Haunt)
            {
                Defaults = (skill, meta) =>
                {

                }
            };

            public static SkillMeta Sacrifice = new SkillMeta(Skills.WitchDoctor.Sacrifice)
            {
                Defaults = (skill, meta) =>
                {

                }
            };

            public static SkillMeta ZombieCharger = new SkillMeta(Skills.WitchDoctor.ZombieCharger)
            {
                Defaults = (skill, meta) =>
                {

                }
            };

            public static SkillMeta SpiritWalk = new SkillMeta(Skills.WitchDoctor.SpiritWalk)
            {
                Defaults = (skill, meta) =>
                {

                }
            };

            public static SkillMeta SpiritBarrage = new SkillMeta(Skills.WitchDoctor.SpiritBarrage)
            {
                Defaults = (skill, meta) =>
                {

                }
            };

            public static SkillMeta Gargantuan = new SkillMeta(Skills.WitchDoctor.Gargantuan)
            {
                Defaults = (skill, meta) =>
                {

                }
            };

            public static SkillMeta LocustSwarm = new SkillMeta(Skills.WitchDoctor.LocustSwarm)
            {
                Defaults = (skill, meta) =>
                {

                }
            };

            public static SkillMeta Firebomb = new SkillMeta(Skills.WitchDoctor.Firebomb)
            {
                Defaults = (skill, meta) =>
                {

                }
            };

            public static SkillMeta Hex = new SkillMeta(Skills.WitchDoctor.Hex)
            {
                Defaults = (skill, meta) =>
                {

                }
            };

            public static SkillMeta AcidCloud = new SkillMeta(Skills.WitchDoctor.AcidCloud)
            {
                Defaults = (skill, meta) =>
                {

                }
            };

            public static SkillMeta MassConfusion = new SkillMeta(Skills.WitchDoctor.MassConfusion)
            {
                Defaults = (skill, meta) =>
                {

                }
            };

            public static SkillMeta BigBadVoodoo = new SkillMeta(Skills.WitchDoctor.BigBadVoodoo)
            {
                Defaults = (skill, meta) =>
                {

                }
            };

            public static SkillMeta WallOfZombies = new SkillMeta(Skills.WitchDoctor.WallOfZombies)
            {
                Defaults = (skill, meta) =>
                {

                }
            };

            public static SkillMeta FetishArmy = new SkillMeta(Skills.WitchDoctor.FetishArmy)
            {
                Defaults = (skill, meta) =>
                {

                }
            };

            public static SkillMeta Piranhas = new SkillMeta(Skills.WitchDoctor.Piranhas)
            {
                Defaults = (skill, meta) =>
                {

                }
            };


        }

        #endregion

        #region Crusader Default Skill Settings

        public class Crusader : FieldCollection<Crusader, SkillMeta>
        {
            static Crusader()
            {
                ApplyDefaults(ToList());
            }

            public static SkillMeta Punish = new SkillMeta(Skills.Crusader.Punish)
            {
                Defaults = (skill, meta) =>
                {

                }
            };

            public static SkillMeta ShieldBash = new SkillMeta(Skills.Crusader.ShieldBash)
            {
                Defaults = (skill, meta) =>
                {

                }
            };

            public static SkillMeta Slash = new SkillMeta(Skills.Crusader.Slash)
            {
                Defaults = (skill, meta) =>
                {

                }
            };

            public static SkillMeta ShieldGlare = new SkillMeta(Skills.Crusader.ShieldGlare)
            {
                Defaults = (skill, meta) =>
                {

                }
            };

            public static SkillMeta SweepAttack = new SkillMeta(Skills.Crusader.SweepAttack)
            {
                Defaults = (skill, meta) =>
                {

                }
            };

            public static SkillMeta IronSkin = new SkillMeta(Skills.Crusader.IronSkin)
            {
                Defaults = (skill, meta) =>
                {

                }
            };

            public static SkillMeta Provoke = new SkillMeta(Skills.Crusader.Punish)
            {
                Defaults = (skill, meta) =>
                {

                }
            };

            public static SkillMeta Smite = new SkillMeta(Skills.Crusader.Smite)
            {
                Defaults = (skill, meta) =>
                {

                }
            };

            public static SkillMeta BlessedHammer = new SkillMeta(Skills.Crusader.BlessedHammer)
            {
                Defaults = (skill, meta) =>
                {

                }
            };

            public static SkillMeta SteedCharge = new SkillMeta(Skills.Crusader.SteedCharge)
            {
                Defaults = (skill, meta) =>
                {

                }
            };

            public static SkillMeta LawsOfValor = new SkillMeta(Skills.Crusader.LawsOfValor)
            {
                Defaults = (skill, meta) =>
                {

                }
            };

            public static SkillMeta Justice = new SkillMeta(Skills.Crusader.Justice)
            {
                Defaults = (skill, meta) =>
                {

                }
            };

            public static SkillMeta Consecration = new SkillMeta(Skills.Crusader.Consecration)
            {
                Defaults = (skill, meta) =>
                {

                }
            };

            public static SkillMeta LawsOfJustice = new SkillMeta(Skills.Crusader.LawsOfJustice)
            {
                Defaults = (skill, meta) =>
                {

                }
            };

            public static SkillMeta FallingSword = new SkillMeta(Skills.Crusader.FallingSword)
            {
                Defaults = (skill, meta) =>
                {

                }
            };

            public static SkillMeta BlessedShield = new SkillMeta(Skills.Crusader.BlessedShield)
            {
                Defaults = (skill, meta) =>
                {

                }
            };

            public static SkillMeta Condemn = new SkillMeta(Skills.Crusader.Condemn)
            {
                Defaults = (skill, meta) =>
                {

                }
            };

            public static SkillMeta Judgment = new SkillMeta(Skills.Crusader.Judgment)
            {
                Defaults = (skill, meta) =>
                {

                }
            };

            public static SkillMeta LawsOfHope = new SkillMeta(Skills.Crusader.LawsOfHope)
            {
                Defaults = (skill, meta) =>
                {

                }
            };

            public static SkillMeta AkaratsChampion = new SkillMeta(Skills.Crusader.AkaratsChampion)
            {
                Defaults = (skill, meta) =>
                {

                }
            };

            public static SkillMeta FistOfTheHeavens = new SkillMeta(Skills.Crusader.FistOfTheHeavens)
            {
                Defaults = (skill, meta) =>
                {

                }
            };

            public static SkillMeta Phalanx = new SkillMeta(Skills.Crusader.Phalanx)
            {
                Defaults = (skill, meta) =>
                {

                }
            };

            public static SkillMeta HeavensFury = new SkillMeta(Skills.Crusader.HeavensFury)
            {
                Defaults = (skill, meta) =>
                {

                }
            };

            public static SkillMeta Bombardment = new SkillMeta(Skills.Crusader.Bombardment)
            {
                Defaults = (skill, meta) =>
                {

                }
            };

        }

        #endregion

        #region Barbarian Default Skill Settings

        public class Barbarian : FieldCollection<Barbarian, SkillMeta>
        {
            static Barbarian()
            {
                ApplyDefaults(ToList());
            }

            public static SkillMeta Bash = new SkillMeta(Skills.Barbarian.Bash)
            {
                Defaults = (skill, meta) =>
                {

                }
            };

            public static SkillMeta HammerOfTheAncients = new SkillMeta(Skills.Barbarian.Bash)
            {
                Defaults = (skill, meta) =>
                {

                }
            };

            public static SkillMeta Cleave = new SkillMeta(Skills.Barbarian.Cleave)
            {
                Defaults = (skill, meta) =>
                {

                }
            };

            public static SkillMeta GroundStomp = new SkillMeta(Skills.Barbarian.GroundStomp)
            {

            };

            public static SkillMeta Rend = new SkillMeta(Skills.Barbarian.Rend)
            {
                Defaults = (skill, meta) =>
                {

                }
            };

            public static SkillMeta Leap = new SkillMeta(Skills.Barbarian.Leap)
            {
                Defaults = (skill, meta) =>
                {

                }
            };

            public static SkillMeta Overpower = new SkillMeta(Skills.Barbarian.Overpower)
            {
                Defaults = (skill, meta) =>
                {

                }
            };

            public static SkillMeta Frenzy = new SkillMeta(Skills.Barbarian.Frenzy)
            {
                Defaults = (skill, meta) =>
                {

                }
            };

            public static SkillMeta SeismicSlam = new SkillMeta(Skills.Barbarian.FuriousCharge)
            {
                Defaults = (skill, meta) =>
                {

                }
            };

            public static SkillMeta Revenge = new SkillMeta(Skills.Barbarian.Revenge)
            {
                Defaults = (skill, meta) =>
                {

                }
            };

            public static SkillMeta ThreateningShout = new SkillMeta(Skills.Barbarian.ThreateningShout)
            {
                Defaults = (skill, meta) =>
                {

                }
            };

            public static SkillMeta Sprint = new SkillMeta(Skills.Barbarian.Sprint)
            {
                Defaults = (skill, meta) =>
                {

                }
            };

            public static SkillMeta WeaponThrow = new SkillMeta(Skills.Barbarian.WeaponThrow)
            {
                Defaults = (skill, meta) =>
                {

                }
            };

            public static SkillMeta Earthquake = new SkillMeta(Skills.Barbarian.Earthquake)
            {
                Defaults = (skill, meta) =>
                {

                }
            };

            public static SkillMeta Whirlwind = new SkillMeta(Skills.Barbarian.Whirlwind)
            {
                Defaults = (skill, meta) =>
                {

                }
            };

            public static SkillMeta FuriousCharge = new SkillMeta(Skills.Barbarian.FuriousCharge)
            {

            };

            public static SkillMeta IgnorePain = new SkillMeta(Skills.Barbarian.IgnorePain)
            {
                Defaults = (skill, meta) =>
                {

                }
            };

            public static SkillMeta BattleRage = new SkillMeta(Skills.Barbarian.BattleRage)
            {
                Defaults = (skill, meta) =>
                {

                }
            };

            public static SkillMeta CallOfTheAncients = new SkillMeta(Skills.Barbarian.CallOfTheAncients)
            {
                Defaults = (skill, meta) =>
                {

                }
            };

            public static SkillMeta AncientSpear = new SkillMeta(Skills.Barbarian.AncientSpear)
            {
                Defaults = (skill, meta) =>
                {

                }
            };

            public static SkillMeta WarCry = new SkillMeta(Skills.Barbarian.WarCry)
            {
                Defaults = (skill, meta) =>
                {

                }
            };

            public static SkillMeta WrathOfTheBerserker = new SkillMeta(Skills.Barbarian.WrathOfTheBerserker)
            {
                Defaults = (skill, meta) =>
                {

                }
            };

            public static SkillMeta Avalanche = new SkillMeta(Skills.Barbarian.Avalanche)
            {
                Defaults = (skill, meta) =>
                {

                }
            };

        }

        #endregion

        #region DemonHunter Default Skill Settings

        public class DemonHunter : FieldCollection<DemonHunter, SkillMeta>
        {
            static DemonHunter()
            {
                ApplyDefaults(ToList());
            }

            public static SkillMeta HungeringArrow = new SkillMeta(Skills.DemonHunter.HungeringArrow)
            {
                Defaults = (skill, meta) =>
                {
                    meta.IsCombatOnly = true;
                    meta.IsOffensiveSkill = true;
                    meta.MaxTargetDistance = 100;
                    meta.IsAreaEffectSkill = true;
                    meta.AreaEffectShape = AreaEffectShapeType.Beam;

                    if (skill.CurrentRune == Runes.DemonHunter.ShatterShot)
                        meta.AreaEffectShape = AreaEffectShapeType.Cone;

                    if (skill.CurrentRune == Runes.DemonHunter.SprayOfTeeth)
                        meta.AreaEffectShape = AreaEffectShapeType.Circle;                                                
                }
            };

            public static SkillMeta Impale = new SkillMeta(Skills.DemonHunter.Impale)
            {
                Defaults = (skill, meta) =>
                {
                    meta.IsCombatOnly = true;
                    meta.IsOffensiveSkill = true;
                    meta.MaxTargetDistance = 100f;

                    if (skill.CurrentRune == Runes.DemonHunter.Overpenetration)
                    {
                        meta.IsAreaEffectSkill = true;
                        meta.AreaEffectShape = AreaEffectShapeType.Beam;
                    }
                }
            };

            public static SkillMeta EntanglingShot = new SkillMeta(Skills.DemonHunter.EntanglingShot)
            {
                Defaults = (skill, meta) =>
                {
                    meta.IsCombatOnly = true;
                    meta.IsOffensiveSkill = true;
                    meta.MaxTargetDistance = 100f;
                    meta.IsAreaEffectSkill = true;
                    meta.AreaEffectShape = AreaEffectShapeType.Circle;
                    skill.AreaEffectRadius = 12f;
                }
            };

            public static SkillMeta Caltrops = new SkillMeta(Skills.DemonHunter.Caltrops)
            {
                Defaults = (skill, meta) =>
                {
                    meta.IsDefensiveSkill = true;
                    meta.IsCastOnSelf = true;
                    meta.IsAreaEffectSkill = true;
                    meta.AreaEffectShape = AreaEffectShapeType.Circle;
                }
            };

            public static SkillMeta RapidFire = new SkillMeta(Skills.DemonHunter.RapidFire)
            {
                Defaults = (skill, meta) =>
                {
                    meta.IsCombatOnly = true;
                    meta.IsOffensiveSkill = true;
                    meta.MaxTargetDistance = 100f;

                    if (skill.CurrentRune == Runes.DemonHunter.HighVelocity)
                    {
                        meta.IsAreaEffectSkill = true;
                        meta.AreaEffectShape = AreaEffectShapeType.Beam;
                    }

                    if (skill.CurrentRune == Runes.DemonHunter.Bombardment)
                    {
                        meta.IsAreaEffectSkill = true;
                        meta.AreaEffectShape = AreaEffectShapeType.Circle;
                    }                        
                }
            };

            public static SkillMeta SmokeScreen = new SkillMeta(Skills.DemonHunter.SmokeScreen)
            {
                Defaults = (skill, meta) =>
                {
                    meta.IsCombatOnly = true;
                    meta.IsDefensiveSkill = true;
                    meta.IsCastOnSelf = true;
                }
            };

            public static SkillMeta Vault = new SkillMeta(Skills.DemonHunter.Vault)
            {
                Defaults = (skill, meta) =>
                {
                    meta.IsMovementSkill = true;
                    meta.IsDefensiveSkill = true;
                    meta.IsAvoidanceSkill = true;
                }
            };

            public static SkillMeta Bolas = new SkillMeta(Skills.DemonHunter.Bolas)
            {
                Defaults = (skill, meta) =>
                {
                    meta.IsCombatOnly = true;
                    meta.IsOffensiveSkill = true;
                    meta.MaxTargetDistance = 85f;
                    meta.AreaEffectShape = AreaEffectShapeType.Circle;

                    if (skill.CurrentRune == Runes.DemonHunter.FreezingStrike)
                    {
                        meta.IsAreaEffectSkill = false;
                        meta.AreaEffectShape = AreaEffectShapeType.None;
                    }
                }
            };

            public static SkillMeta Chakram = new SkillMeta(Skills.DemonHunter.Chakram)
            {
                Defaults = (skill, meta) =>
                {
                    meta.IsCombatOnly = true;
                    meta.IsOffensiveSkill = true;
                    meta.IsAreaEffectSkill = true;
                    meta.MaxTargetDistance = 100f;

                    if (skill.CurrentRune == Runes.DemonHunter.TwinChakrams)
                    {
                        skill.AreaEffectRadius = 45f;                        
                        meta.AreaEffectShape = AreaEffectShapeType.Beam;                        
                    }

                    if (skill.CurrentRune == Runes.DemonHunter.Serpentine || skill.CurrentRune == Runes.DemonHunter.Boomerang)
                    {
                        skill.AreaEffectRadius = 20f;
                        meta.AreaEffectShape = AreaEffectShapeType.Beam;
                    }

                    if (skill.CurrentRune == Runes.DemonHunter.ShurikenCloud)
                    {
                        skill.AreaEffectRadius = 20f;
                        meta.IsCastOnSelf = true;                        
                        meta.AreaEffectShape = AreaEffectShapeType.Circle;
                    }

                    if (skill.CurrentRune == Runes.DemonHunter.RazorDisk)
                    {
                        skill.AreaEffectRadius = 50f;
                        meta.AreaEffectShape = AreaEffectShapeType.Circle;
                    }
                }
            };

            public static SkillMeta Preparation = new SkillMeta(Skills.DemonHunter.Preparation)
            {
                Defaults = (skill, meta) =>
                {
                    meta.IsCombatOnly = true;
                    meta.IsBuffingSkill = true;
                    meta.IsCastOnSelf = true;
                }
            };

            public static SkillMeta FanOfKnives = new SkillMeta(Skills.DemonHunter.FanOfKnives)
            {
                Defaults = (skill, meta) =>
                {
                    meta.IsCombatOnly = true;
                    meta.IsOffensiveSkill = true;
                    meta.IsCastOnSelf = true;
                }
            };

            public static SkillMeta EvasiveFire = new SkillMeta(Skills.DemonHunter.EvasiveFire)
            {
                Defaults = (skill, meta) =>
                {
                    meta.IsCombatOnly = true;
                    meta.IsAvoidanceSkill = true;
                    meta.IsOffensiveSkill = true;
                    meta.IsDefensiveSkill = true;
                    meta.IsAreaEffectSkill = true;
                    meta.AreaEffectShape = AreaEffectShapeType.Cone;
                    meta.MaxTargetDistance = 30f;

                    if (skill.CurrentRune == Runes.DemonHunter.Focus)
                    {
                        meta.IsAvoidanceSkill = false;
                        meta.IsDefensiveSkill = false;
                    }
                }
            };

            public static SkillMeta Grenade = new SkillMeta(Skills.DemonHunter.Grenade)
            {
                Defaults = (skill, meta) =>
                {
                    meta.IsCombatOnly = true;
                    meta.IsOffensiveSkill = true;
                    meta.IsAreaEffectSkill = true;
                    meta.AreaEffectShape = AreaEffectShapeType.Circle;
                    meta.MaxTargetDistance = 100f;

                    if (skill.CurrentRune == Runes.DemonHunter.GrenadeCache)
                    {
                        meta.AreaEffectShape = AreaEffectShapeType.Cone;
                    }
                }
            };

            public static SkillMeta ShadowPower = new SkillMeta(Skills.DemonHunter.ShadowPower)
            {
                Defaults = (skill, meta) =>
                {
                    meta.IsCombatOnly = true;
                    meta.IsCastOnSelf = true;
                    meta.IsBuffingSkill = true;
                    meta.IsDefensiveSkill = true;
                }
            };

            public static SkillMeta SpikeTrap = new SkillMeta(Skills.DemonHunter.SpikeTrap)
            {
                Defaults = (skill, meta) =>
                {
                    meta.IsCombatOnly = true;
                    meta.IsDefensiveSkill = true;
                    meta.IsCastOnSelf = true;
                    meta.IsAreaEffectSkill = true;
                    meta.AreaEffectShape = AreaEffectShapeType.Circle;
                }
            };

            public static SkillMeta Companion = new SkillMeta(Skills.DemonHunter.Companion)
            {
                Defaults = (skill, meta) =>
                {
                    meta.IsBuffingSkill = true;
                    meta.IsCastOnSelf = true;
                    meta.IsOffensiveSkill = true;
                }
            };

            public static SkillMeta Strafe = new SkillMeta(Skills.DemonHunter.Strafe)
            {
                Defaults = (skill, meta) =>
                {
                    meta.IsCombatOnly = true;
                    meta.IsOffensiveSkill = true;

                    if (skill.CurrentRune == Runes.DemonHunter.Demolition)
                    {
                        meta.IsAreaEffectSkill = true;
                        meta.AreaEffectShape = AreaEffectShapeType.Circle;
                    }
                }             
            };

            public static SkillMeta ElementalArrow = new SkillMeta(Skills.DemonHunter.ElementalArrow)
            {
                Defaults = (skill, meta) =>
                {
                    meta.IsCombatOnly = true;
                    meta.IsOffensiveSkill = true;
                    meta.IsAreaEffectSkill = true;
                    meta.AreaEffectShape = AreaEffectShapeType.Beam;
                    skill.AreaEffectRadius = 8f;

                    if (skill.CurrentRune == Runes.DemonHunter.BallLightning)
                        skill.AreaEffectRadius = 17f;

                    if (skill.CurrentRune == Runes.DemonHunter.FrostArrow)
                        meta.AreaEffectShape = AreaEffectShapeType.Cone;
                }
            };

            public static SkillMeta MarkedForDeath = new SkillMeta(Skills.DemonHunter.MarkedForDeath)
            {
                Defaults = (skill, meta) =>
                {
                    meta.IsCombatOnly = true;
                    meta.IsOffensiveSkill = true;
                    meta.IsDebuffingSkill = true;
                }
            };

            public static SkillMeta Multishot = new SkillMeta(Skills.DemonHunter.Multishot)
            {
                Defaults = (skill, meta) =>
                {
                    meta.IsCombatOnly = true;
                    meta.IsOffensiveSkill = true;
                    meta.IsAreaEffectSkill = true;
                    meta.AreaEffectShape = AreaEffectShapeType.Cone;
                    meta.MaxTargetDistance = 90f;
                    skill.AreaEffectRadius = 90f;
                }
            };

            public static SkillMeta Sentry = new SkillMeta(Skills.DemonHunter.Sentry)
            {
                Defaults = (skill, meta) =>
                {
                    meta.IsCombatOnly = true;
                    meta.IsOffensiveSkill = true;
                    meta.IsSummoningSkill = true;
                }
            };

            public static SkillMeta ClusterArrow = new SkillMeta(Skills.DemonHunter.ClusterArrow)
            {          
                Defaults = (skill, meta) =>
                {
                    meta.IsCombatOnly = true;
                    meta.IsOffensiveSkill = true;
                    meta.MaxTargetDistance = 90f;
                    meta.AreaEffectShape = AreaEffectShapeType.Circle;
                    skill.AreaEffectRadius = 25f;

                    if (skill.CurrentRune == Runes.DemonHunter.ClusterBombs)
                        meta.AreaEffectShape = AreaEffectShapeType.Beam;                        
                }
            };

            public static SkillMeta RainOfVengeance = new SkillMeta(Skills.DemonHunter.RainOfVengeance)
            {               
                Defaults = (skill, meta) =>
                {
                    meta.IsCombatOnly = true;
                    meta.IsOffensiveSkill = true;
                    meta.MaxTargetDistance = 110f;
                    meta.AreaEffectShape = AreaEffectShapeType.Circle;
                    skill.AreaEffectRadius = 45f;

                    if (skill.CurrentRune == Runes.DemonHunter.Stampede)
                        meta.AreaEffectShape = AreaEffectShapeType.Beam;

                    if (skill.CurrentRune == Runes.DemonHunter.Anathema)
                        skill.AreaEffectRadius = 25f;

                    if (skill.CurrentRune == Runes.DemonHunter.Shade)
                        skill.AreaEffectRadius = 65f;
                }
            };

            public static SkillMeta Vengeance = new SkillMeta(Skills.DemonHunter.Vengeance)
            {
                Defaults = (skill, meta) =>
                {
                    meta.IsCombatOnly = true;
                    meta.IsBuffingSkill = true;
                    meta.IsCastOnSelf = true;
                }
            };

        }

        #endregion

        #region Wizard Default Skill Settings

        public class Wizard : FieldCollection<Wizard, SkillMeta>
        {

            static Wizard()
            {
                ApplyDefaults(ToList());
            }

            public static SkillMeta MagicMissile = new SkillMeta(Skills.Wizard.MagicMissile)
            {
                Defaults = (skill, meta) =>
                {

                }
            };

            public static SkillMeta RayOfFrost = new SkillMeta(Skills.Wizard.RayOfFrost)
            {
                Defaults = (skill, meta) =>
                {

                }
            };

            public static SkillMeta ShockPulse = new SkillMeta(Skills.Wizard.ShockPulse)
            {
                Defaults = (skill, meta) =>
                {

                }
            };

            public static SkillMeta FrostNova = new SkillMeta(Skills.Wizard.FrostNova)
            {
                Defaults = (skill, meta) =>
                {

                }
            };

            public static SkillMeta ArcaneOrb = new SkillMeta(Skills.Wizard.ArcaneOrb)
            {
                Defaults = (skill, meta) =>
                {

                }
            };

            public static SkillMeta DiamondSkin = new SkillMeta(Skills.Wizard.DiamondSkin)
            {
                Defaults = (skill, meta) =>
                {

                }
            };

            public static SkillMeta WaveOfForce = new SkillMeta(Skills.Wizard.WaveOfForce)
            {
                Defaults = (skill, meta) =>
                {

                }
            };

            public static SkillMeta SpectralBlade = new SkillMeta(Skills.Wizard.SpectralBlade)
            {
                Defaults = (skill, meta) =>
                {

                }
            };

            public static SkillMeta ArcaneTorrent = new SkillMeta(Skills.Wizard.ArcaneTorrent)
            {
                Defaults = (skill, meta) =>
                {

                }
            };

            public static SkillMeta EnergyTwister = new SkillMeta(Skills.Wizard.EnergyTwister)
            {
                Defaults = (skill, meta) =>
                {

                }
            };

            public static SkillMeta IceArmor = new SkillMeta(Skills.Wizard.IceArmor)
            {
                Defaults = (skill, meta) =>
                {

                }
            };

            public static SkillMeta Electrocute = new SkillMeta(Skills.Wizard.Electrocute)
            {
                Defaults = (skill, meta) =>
                {

                }
            };

            public static SkillMeta SlowTime = new SkillMeta(Skills.Wizard.SlowTime)
            {
                Defaults = (skill, meta) =>
                {

                }
            };

            public static SkillMeta StormArmor = new SkillMeta(Skills.Wizard.StormArmor)
            {
                Defaults = (skill, meta) =>
                {

                }
            };

            public static SkillMeta ExplosiveBlast = new SkillMeta(Skills.Wizard.ExplosiveBlast)
            {
                Defaults = (skill, meta) =>
                {

                }
            };

            public static SkillMeta MagicWeapon = new SkillMeta(Skills.Wizard.MagicWeapon)
            {
                Defaults = (skill, meta) =>
                {

                }
            };

            public static SkillMeta Hydra = new SkillMeta(Skills.Wizard.Hydra)
            {
                Defaults = (skill, meta) =>
                {

                }
            };

            public static SkillMeta Disintegrate = new SkillMeta(Skills.Wizard.Disintegrate)
            {
                Defaults = (skill, meta) =>
                {

                }
            };

            public static SkillMeta Familiar = new SkillMeta(Skills.Wizard.Familiar)
            {
                Defaults = (skill, meta) =>
                {

                }
            };

            public static SkillMeta Teleport = new SkillMeta(Skills.Wizard.Teleport)
            {
                Defaults = (skill, meta) =>
                {

                }
            };

            public static SkillMeta MirrorImage = new SkillMeta(Skills.Wizard.MirrorImage)
            {
                Defaults = (skill, meta) =>
                {

                }
            };

            public static SkillMeta Meteor = new SkillMeta(Skills.Wizard.Meteor)
            {
                Defaults = (skill, meta) =>
                {

                }
            };

            public static SkillMeta Blizzard = new SkillMeta(Skills.Wizard.Blizzard)
            {
                Defaults = (skill, meta) =>
                {

                }
            };

            public static SkillMeta EnergyArmor = new SkillMeta(Skills.Wizard.EnergyArmor)
            {
                Defaults = (skill, meta) =>
                {

                }
            };

            public static SkillMeta Archon = new SkillMeta(Skills.Wizard.Archon)
            {
                Defaults = (skill, meta) =>
                {

                }
            };

            public static SkillMeta BlackHole = new SkillMeta(Skills.Wizard.BlackHole)
            {
                Defaults = (skill, meta) =>
                {

                }
            };

        }

        #endregion

    }
}