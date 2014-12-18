using System.Collections.Generic;
using System.Linq;
using Trinity.Helpers;
using Trinity.Objects;
using Trinity.Technicals;
using Zeta.Game;
using Zeta.Game.Internals.Actors;

namespace Trinity.Reference
{
    class SkillBySlot
    {

        private static StatType GetMainStatType(ACDItem item)
        {
            if (item.Stats.Strength > 0) return StatType.Strength;
            if (item.Stats.Intelligence > 0) return StatType.Intelligence;
            if (item.Stats.Dexterity > 0) return StatType.Dexterity;
            return StatType.Unknown;
        }

        public enum StatType
        {
            Unknown = 0,
            Strength,
            Dexterity,
            Intelligence,
            Vitality,
        }

        public static int GetSkillDamagePercent(ACDItem item)
        {
            if (!SkillDamageByItemTypeAndClass.Any())
                return 0;

            var statType = GetMainStatType(item);
            var actorClasses = new List<ActorClass>();
            var itemType = item.ItemType;

            switch (statType)
            {
                case StatType.Dexterity:
                    actorClasses.Add(ActorClass.Monk);
                    actorClasses.Add(ActorClass.DemonHunter);
                    break;

                case StatType.Intelligence:
                    actorClasses.Add(ActorClass.Witchdoctor);
                    actorClasses.Add(ActorClass.Wizard);
                    break;

                case StatType.Strength:
                    actorClasses.Add(ActorClass.Crusader);
                    actorClasses.Add(ActorClass.Barbarian);
                    break;
            }

            if (!actorClasses.Any())
                return 0;

            foreach (var actorClass in actorClasses)
            {
                var kvp = new KeyValuePair<ItemType, ActorClass>(itemType, actorClass);

                foreach (var skill in SkillDamageByItemTypeAndClass[kvp])
                {
                    //Logger.Log(string.Format("Skill for {0}/{1} is {2} ({3})", itemType, actorClass, skill.Name, skill.SNOPower));

                    var skillDamageIncrease = item.GetSkillDamageIncrease(skill.SNOPower);

                    if (skillDamageIncrease > 0)
                    {
                        Logger.Log(string.Format("SkillDamage +{0}% {1}", skillDamageIncrease, skill.Name));
                        return (int)skillDamageIncrease;
                    }
                        
                }
            }

            return 0;
        }

        public static readonly LookupList<KeyValuePair<ItemType, ActorClass>, Skill> SkillDamageByItemTypeAndClass = new LookupList<KeyValuePair<ItemType, ActorClass>, Skill>
        {
            // Head Slot
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Helm, ActorClass.DemonHunter), Skills.DemonHunter.ClusterArrow},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Helm, ActorClass.DemonHunter), Skills.DemonHunter.Multishot},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Helm, ActorClass.DemonHunter), Skills.DemonHunter.ElementalArrow},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Helm, ActorClass.DemonHunter), Skills.DemonHunter.Strafe},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Helm, ActorClass.DemonHunter), Skills.DemonHunter.Chakram},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Helm, ActorClass.DemonHunter), Skills.DemonHunter.RapidFire},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Helm, ActorClass.DemonHunter), Skills.DemonHunter.Impale},

            {new KeyValuePair<ItemType, ActorClass>(ItemType.Helm, ActorClass.Barbarian), Skills.Barbarian.Whirlwind},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Helm, ActorClass.Barbarian), Skills.Barbarian.SeismicSlam},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Helm, ActorClass.Barbarian), Skills.Barbarian.AncientSpear},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Helm, ActorClass.Barbarian), Skills.Barbarian.HammerOfTheAncients},

            {new KeyValuePair<ItemType, ActorClass>(ItemType.Helm, ActorClass.Wizard), Skills.Wizard.Meteor},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Helm, ActorClass.Wizard), Skills.Wizard.Disintegrate},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Helm, ActorClass.Wizard), Skills.Wizard.EnergyTwister},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Helm, ActorClass.Wizard), Skills.Wizard.ArcaneTorrent},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Helm, ActorClass.Wizard), Skills.Wizard.WaveOfForce},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Helm, ActorClass.Wizard), Skills.Wizard.ArcaneOrb},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Helm, ActorClass.Wizard), Skills.Wizard.RayOfFrost},

            {new KeyValuePair<ItemType, ActorClass>(ItemType.Helm, ActorClass.Witchdoctor), Skills.WitchDoctor.AcidCloud},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Helm, ActorClass.Witchdoctor), Skills.WitchDoctor.SpiritBarrage},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Helm, ActorClass.Witchdoctor), Skills.WitchDoctor.ZombieCharger},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Helm, ActorClass.Witchdoctor), Skills.WitchDoctor.Sacrifice},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Helm, ActorClass.Witchdoctor), Skills.WitchDoctor.Firebats},

            {new KeyValuePair<ItemType, ActorClass>(ItemType.VoodooMask, ActorClass.Witchdoctor), Skills.WitchDoctor.AcidCloud},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.VoodooMask, ActorClass.Witchdoctor), Skills.WitchDoctor.SpiritBarrage},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.VoodooMask, ActorClass.Witchdoctor), Skills.WitchDoctor.ZombieCharger},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.VoodooMask, ActorClass.Witchdoctor), Skills.WitchDoctor.Sacrifice},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.VoodooMask, ActorClass.Witchdoctor), Skills.WitchDoctor.Firebats},

            {new KeyValuePair<ItemType, ActorClass>(ItemType.Helm, ActorClass.Monk), Skills.Monk.TempestRush},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Helm, ActorClass.Monk), Skills.Monk.ExplodingPalm},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Helm, ActorClass.Monk), Skills.Monk.WaveOfLight},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Helm, ActorClass.Monk), Skills.Monk.LashingTailKick},

            {new KeyValuePair<ItemType, ActorClass>(ItemType.SpiritStone, ActorClass.Monk), Skills.Monk.TempestRush},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.SpiritStone, ActorClass.Monk), Skills.Monk.ExplodingPalm},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.SpiritStone, ActorClass.Monk), Skills.Monk.WaveOfLight},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.SpiritStone, ActorClass.Monk), Skills.Monk.LashingTailKick},  

            // Shoulders

            {new KeyValuePair<ItemType, ActorClass>(ItemType.Shoulder, ActorClass.DemonHunter), Skills.DemonHunter.Companion},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Shoulder, ActorClass.DemonHunter), Skills.DemonHunter.RainOfVengeance},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Shoulder, ActorClass.DemonHunter), Skills.DemonHunter.Sentry},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Shoulder, ActorClass.DemonHunter), Skills.DemonHunter.SpikeTrap},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Shoulder, ActorClass.DemonHunter), Skills.DemonHunter.FanOfKnives},

            {new KeyValuePair<ItemType, ActorClass>(ItemType.Shoulder, ActorClass.Barbarian), Skills.Barbarian.Avalanche},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Shoulder, ActorClass.Barbarian), Skills.Barbarian.Earthquake},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Shoulder, ActorClass.Barbarian), Skills.Barbarian.Overpower},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Shoulder, ActorClass.Barbarian), Skills.Barbarian.Rend},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Shoulder, ActorClass.Barbarian), Skills.Barbarian.Revenge},

            {new KeyValuePair<ItemType, ActorClass>(ItemType.Shoulder, ActorClass.Wizard), Skills.Wizard.BlackHole},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Shoulder, ActorClass.Wizard), Skills.Wizard.Blizzard},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Shoulder, ActorClass.Wizard), Skills.Wizard.Familiar},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Shoulder, ActorClass.Wizard), Skills.Wizard.ExplosiveBlast},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Shoulder, ActorClass.Wizard), Skills.Wizard.Hydra},

            {new KeyValuePair<ItemType, ActorClass>(ItemType.Shoulder, ActorClass.Witchdoctor), Skills.WitchDoctor.FetishArmy},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Shoulder, ActorClass.Witchdoctor), Skills.WitchDoctor.Gargantuan},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Shoulder, ActorClass.Witchdoctor), Skills.WitchDoctor.WallOfZombies},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Shoulder, ActorClass.Witchdoctor), Skills.WitchDoctor.GraspOfTheDead},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Shoulder, ActorClass.Witchdoctor), Skills.WitchDoctor.Haunt},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Shoulder, ActorClass.Witchdoctor), Skills.WitchDoctor.LocustSwarm},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Shoulder, ActorClass.Witchdoctor), Skills.WitchDoctor.Piranhas},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Shoulder, ActorClass.Witchdoctor), Skills.WitchDoctor.SummonZombieDogs},

            {new KeyValuePair<ItemType, ActorClass>(ItemType.Shoulder, ActorClass.Monk), Skills.Monk.DashingStrike},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Shoulder, ActorClass.Monk), Skills.Monk.SevensidedStrike},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Shoulder, ActorClass.Monk), Skills.Monk.MysticAlly},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Shoulder, ActorClass.Monk), Skills.Monk.SweepingWind},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Shoulder, ActorClass.Monk), Skills.Monk.CycloneStrike},

            {new KeyValuePair<ItemType, ActorClass>(ItemType.Shoulder, ActorClass.Crusader), Skills.Crusader.FallingSword},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Shoulder, ActorClass.Crusader), Skills.Crusader.HeavensFury},

            // Chest

            {new KeyValuePair<ItemType, ActorClass>(ItemType.Chest, ActorClass.DemonHunter), Skills.DemonHunter.Companion},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Chest, ActorClass.DemonHunter), Skills.DemonHunter.RainOfVengeance},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Chest, ActorClass.DemonHunter), Skills.DemonHunter.Sentry},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Chest, ActorClass.DemonHunter), Skills.DemonHunter.SpikeTrap},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Chest, ActorClass.DemonHunter), Skills.DemonHunter.FanOfKnives},

            {new KeyValuePair<ItemType, ActorClass>(ItemType.Cloak, ActorClass.DemonHunter), Skills.DemonHunter.Companion},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Cloak, ActorClass.DemonHunter), Skills.DemonHunter.RainOfVengeance},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Cloak, ActorClass.DemonHunter), Skills.DemonHunter.Sentry},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Cloak, ActorClass.DemonHunter), Skills.DemonHunter.SpikeTrap},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Cloak, ActorClass.DemonHunter), Skills.DemonHunter.FanOfKnives},

            {new KeyValuePair<ItemType, ActorClass>(ItemType.Chest, ActorClass.Barbarian), Skills.Barbarian.Avalanche},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Chest, ActorClass.Barbarian), Skills.Barbarian.Earthquake},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Chest, ActorClass.Barbarian), Skills.Barbarian.Overpower},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Chest, ActorClass.Barbarian), Skills.Barbarian.Rend},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Chest, ActorClass.Barbarian), Skills.Barbarian.Revenge},

            {new KeyValuePair<ItemType, ActorClass>(ItemType.Chest, ActorClass.Wizard), Skills.Wizard.BlackHole},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Chest, ActorClass.Wizard), Skills.Wizard.Blizzard},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Chest, ActorClass.Wizard), Skills.Wizard.Familiar},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Chest, ActorClass.Wizard), Skills.Wizard.ExplosiveBlast},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Chest, ActorClass.Wizard), Skills.Wizard.Hydra},

            {new KeyValuePair<ItemType, ActorClass>(ItemType.Chest, ActorClass.Witchdoctor), Skills.WitchDoctor.FetishArmy},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Chest, ActorClass.Witchdoctor), Skills.WitchDoctor.Gargantuan},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Chest, ActorClass.Witchdoctor), Skills.WitchDoctor.WallOfZombies},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Chest, ActorClass.Witchdoctor), Skills.WitchDoctor.GraspOfTheDead},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Chest, ActorClass.Witchdoctor), Skills.WitchDoctor.Haunt},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Chest, ActorClass.Witchdoctor), Skills.WitchDoctor.LocustSwarm},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Chest, ActorClass.Witchdoctor), Skills.WitchDoctor.Piranhas},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Chest, ActorClass.Witchdoctor), Skills.WitchDoctor.SummonZombieDogs},

            {new KeyValuePair<ItemType, ActorClass>(ItemType.Chest, ActorClass.Monk), Skills.Monk.DashingStrike},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Chest, ActorClass.Monk), Skills.Monk.SevensidedStrike},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Chest, ActorClass.Monk), Skills.Monk.MysticAlly},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Chest, ActorClass.Monk), Skills.Monk.SweepingWind},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Chest, ActorClass.Monk), Skills.Monk.CycloneStrike},

            {new KeyValuePair<ItemType, ActorClass>(ItemType.Chest, ActorClass.Crusader), Skills.Crusader.FallingSword},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Chest, ActorClass.Crusader), Skills.Crusader.HeavensFury},

            // Belt

            {new KeyValuePair<ItemType, ActorClass>(ItemType.Belt, ActorClass.DemonHunter), Skills.DemonHunter.Grenade},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Belt, ActorClass.DemonHunter), Skills.DemonHunter.EvasiveFire},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Belt, ActorClass.DemonHunter), Skills.DemonHunter.Bolas},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Belt, ActorClass.DemonHunter), Skills.DemonHunter.EntanglingShot},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Belt, ActorClass.DemonHunter), Skills.DemonHunter.HungeringArrow},

            {new KeyValuePair<ItemType, ActorClass>(ItemType.Belt, ActorClass.Barbarian), Skills.Barbarian.Frenzy},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Belt, ActorClass.Barbarian), Skills.Barbarian.Cleave},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Belt, ActorClass.Barbarian), Skills.Barbarian.Bash},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Belt, ActorClass.Barbarian), Skills.Barbarian.WeaponThrow},

            {new KeyValuePair<ItemType, ActorClass>(ItemType.MightyBelt, ActorClass.Barbarian), Skills.Barbarian.Frenzy},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.MightyBelt, ActorClass.Barbarian), Skills.Barbarian.Cleave},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.MightyBelt, ActorClass.Barbarian), Skills.Barbarian.Bash},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.MightyBelt, ActorClass.Barbarian), Skills.Barbarian.WeaponThrow},

            {new KeyValuePair<ItemType, ActorClass>(ItemType.Belt, ActorClass.Wizard), Skills.Wizard.Electrocute},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Belt, ActorClass.Wizard), Skills.Wizard.SpectralBlade},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Belt, ActorClass.Wizard), Skills.Wizard.ShockPulse},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Belt, ActorClass.Wizard), Skills.Wizard.MagicMissile},

            {new KeyValuePair<ItemType, ActorClass>(ItemType.Belt, ActorClass.Witchdoctor), Skills.WitchDoctor.Firebomb},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Belt, ActorClass.Witchdoctor), Skills.WitchDoctor.PlagueOfToads},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Belt, ActorClass.Witchdoctor), Skills.WitchDoctor.CorpseSpiders},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Belt, ActorClass.Witchdoctor), Skills.WitchDoctor.PoisonDart},

            {new KeyValuePair<ItemType, ActorClass>(ItemType.Belt, ActorClass.Monk), Skills.Monk.WayOfTheHundredFists},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Belt, ActorClass.Monk), Skills.Monk.CripplingWave},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Belt, ActorClass.Monk), Skills.Monk.DeadlyReach},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Belt, ActorClass.Monk), Skills.Monk.FistsOfThunder},

            {new KeyValuePair<ItemType, ActorClass>(ItemType.Belt, ActorClass.Crusader), Skills.Crusader.Punish},

            // Pants

            {new KeyValuePair<ItemType, ActorClass>(ItemType.Legs, ActorClass.DemonHunter), Skills.DemonHunter.Grenade},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Legs, ActorClass.DemonHunter), Skills.DemonHunter.EvasiveFire},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Legs, ActorClass.DemonHunter), Skills.DemonHunter.Bolas},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Legs, ActorClass.DemonHunter), Skills.DemonHunter.EntanglingShot},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Legs, ActorClass.DemonHunter), Skills.DemonHunter.HungeringArrow},

            {new KeyValuePair<ItemType, ActorClass>(ItemType.Legs, ActorClass.Barbarian), Skills.Barbarian.Frenzy},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Legs, ActorClass.Barbarian), Skills.Barbarian.Cleave},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Legs, ActorClass.Barbarian), Skills.Barbarian.Bash},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Legs, ActorClass.Barbarian), Skills.Barbarian.WeaponThrow},

            {new KeyValuePair<ItemType, ActorClass>(ItemType.Legs, ActorClass.Wizard), Skills.Wizard.Electrocute},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Legs, ActorClass.Wizard), Skills.Wizard.SpectralBlade},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Legs, ActorClass.Wizard), Skills.Wizard.ShockPulse},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Legs, ActorClass.Wizard), Skills.Wizard.MagicMissile},

            {new KeyValuePair<ItemType, ActorClass>(ItemType.Legs, ActorClass.Witchdoctor), Skills.WitchDoctor.Firebomb},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Legs, ActorClass.Witchdoctor), Skills.WitchDoctor.PlagueOfToads},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Legs, ActorClass.Witchdoctor), Skills.WitchDoctor.CorpseSpiders},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Legs, ActorClass.Witchdoctor), Skills.WitchDoctor.PoisonDart},

            {new KeyValuePair<ItemType, ActorClass>(ItemType.Legs, ActorClass.Monk), Skills.Monk.WayOfTheHundredFists},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Legs, ActorClass.Monk), Skills.Monk.CripplingWave},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Legs, ActorClass.Monk), Skills.Monk.DeadlyReach},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Legs, ActorClass.Monk), Skills.Monk.FistsOfThunder},

            {new KeyValuePair<ItemType, ActorClass>(ItemType.Legs, ActorClass.Crusader), Skills.Crusader.Punish},
            
            // Boots

            {new KeyValuePair<ItemType, ActorClass>(ItemType.Helm, ActorClass.DemonHunter), Skills.DemonHunter.ClusterArrow},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Helm, ActorClass.DemonHunter), Skills.DemonHunter.Multishot},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Helm, ActorClass.DemonHunter), Skills.DemonHunter.ElementalArrow},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Helm, ActorClass.DemonHunter), Skills.DemonHunter.Strafe},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Helm, ActorClass.DemonHunter), Skills.DemonHunter.Chakram},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Helm, ActorClass.DemonHunter), Skills.DemonHunter.RapidFire},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Helm, ActorClass.DemonHunter), Skills.DemonHunter.Impale},

            {new KeyValuePair<ItemType, ActorClass>(ItemType.Helm, ActorClass.Barbarian), Skills.Barbarian.Whirlwind},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Helm, ActorClass.Barbarian), Skills.Barbarian.SeismicSlam},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Helm, ActorClass.Barbarian), Skills.Barbarian.AncientSpear},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Helm, ActorClass.Barbarian), Skills.Barbarian.HammerOfTheAncients},

            {new KeyValuePair<ItemType, ActorClass>(ItemType.Helm, ActorClass.Wizard), Skills.Wizard.Meteor},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Helm, ActorClass.Wizard), Skills.Wizard.Disintegrate},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Helm, ActorClass.Wizard), Skills.Wizard.EnergyTwister},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Helm, ActorClass.Wizard), Skills.Wizard.ArcaneTorrent},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Helm, ActorClass.Wizard), Skills.Wizard.WaveOfForce},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Helm, ActorClass.Wizard), Skills.Wizard.ArcaneOrb},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Helm, ActorClass.Wizard), Skills.Wizard.RayOfFrost},

            {new KeyValuePair<ItemType, ActorClass>(ItemType.Helm, ActorClass.Witchdoctor), Skills.WitchDoctor.AcidCloud},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Helm, ActorClass.Witchdoctor), Skills.WitchDoctor.SpiritBarrage},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Helm, ActorClass.Witchdoctor), Skills.WitchDoctor.ZombieCharger},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Helm, ActorClass.Witchdoctor), Skills.WitchDoctor.Sacrifice},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Helm, ActorClass.Witchdoctor), Skills.WitchDoctor.Firebats},

            {new KeyValuePair<ItemType, ActorClass>(ItemType.Helm, ActorClass.Monk), Skills.Monk.TempestRush},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Helm, ActorClass.Monk), Skills.Monk.ExplodingPalm},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Helm, ActorClass.Monk), Skills.Monk.WaveOfLight},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Helm, ActorClass.Monk), Skills.Monk.LashingTailKick},  

            // Offhand

            {new KeyValuePair<ItemType, ActorClass>(ItemType.Quiver, ActorClass.DemonHunter), Skills.DemonHunter.ClusterArrow},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Quiver, ActorClass.DemonHunter), Skills.DemonHunter.Multishot},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Quiver, ActorClass.DemonHunter), Skills.DemonHunter.ElementalArrow},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Quiver, ActorClass.DemonHunter), Skills.DemonHunter.Strafe},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Quiver, ActorClass.DemonHunter), Skills.DemonHunter.Chakram},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Quiver, ActorClass.DemonHunter), Skills.DemonHunter.RapidFire},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Quiver, ActorClass.DemonHunter), Skills.DemonHunter.Impale},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Quiver, ActorClass.DemonHunter), Skills.DemonHunter.Sentry},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Quiver, ActorClass.DemonHunter), Skills.DemonHunter.RainOfVengeance},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Quiver, ActorClass.DemonHunter), Skills.DemonHunter.FanOfKnives},

            {new KeyValuePair<ItemType, ActorClass>(ItemType.Orb, ActorClass.Wizard), Skills.Wizard.EnergyTwister},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Orb, ActorClass.Wizard), Skills.Wizard.Electrocute},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Orb, ActorClass.Wizard), Skills.Wizard.ArcaneOrb},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Orb, ActorClass.Wizard), Skills.Wizard.Hydra},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Orb, ActorClass.Wizard), Skills.Wizard.MagicMissile},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Orb, ActorClass.Wizard), Skills.Wizard.Familiar},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Orb, ActorClass.Wizard), Skills.Wizard.RayOfFrost},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Orb, ActorClass.Wizard), Skills.Wizard.Meteor},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Orb, ActorClass.Wizard), Skills.Wizard.SpectralBlade},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Orb, ActorClass.Wizard), Skills.Wizard.WaveOfForce},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Orb, ActorClass.Wizard), Skills.Wizard.Disintegrate},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Orb, ActorClass.Wizard), Skills.Wizard.ArcaneTorrent},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Orb, ActorClass.Wizard), Skills.Wizard.ExplosiveBlast},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Orb, ActorClass.Wizard), Skills.Wizard.Blizzard},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Orb, ActorClass.Wizard), Skills.Wizard.BlackHole},

            {new KeyValuePair<ItemType, ActorClass>(ItemType.Mojo, ActorClass.Witchdoctor), Skills.WitchDoctor.SpiritBarrage},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Mojo, ActorClass.Witchdoctor), Skills.WitchDoctor.LocustSwarm},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Mojo, ActorClass.Witchdoctor), Skills.WitchDoctor.PlagueOfToads},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Mojo, ActorClass.Witchdoctor), Skills.WitchDoctor.Haunt},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Mojo, ActorClass.Witchdoctor), Skills.WitchDoctor.Firebats},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Mojo, ActorClass.Witchdoctor), Skills.WitchDoctor.GraspOfTheDead},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Mojo, ActorClass.Witchdoctor), Skills.WitchDoctor.Sacrifice},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Mojo, ActorClass.Witchdoctor), Skills.WitchDoctor.FetishArmy},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Mojo, ActorClass.Witchdoctor), Skills.WitchDoctor.ZombieCharger},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Mojo, ActorClass.Witchdoctor), Skills.WitchDoctor.CorpseSpiders},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Mojo, ActorClass.Witchdoctor), Skills.WitchDoctor.AcidCloud},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Mojo, ActorClass.Witchdoctor), Skills.WitchDoctor.PoisonDart},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Mojo, ActorClass.Witchdoctor), Skills.WitchDoctor.WallOfZombies},

            // One Hand Weapon

            {new KeyValuePair<ItemType, ActorClass>(ItemType.MightyWeapon, ActorClass.Barbarian), Skills.Barbarian.WeaponThrow},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.MightyWeapon, ActorClass.Barbarian), Skills.Barbarian.AncientSpear},

            {new KeyValuePair<ItemType, ActorClass>(ItemType.Wand, ActorClass.Wizard), Skills.Wizard.Disintegrate},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Wand, ActorClass.Wizard), Skills.Wizard.SpectralBlade},

            {new KeyValuePair<ItemType, ActorClass>(ItemType.Flail, ActorClass.Crusader), Skills.Crusader.BlessedHammer},

            // Two Hand Weapon

            {new KeyValuePair<ItemType, ActorClass>(ItemType.MightyWeapon, ActorClass.Barbarian), Skills.Barbarian.Whirlwind},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.MightyWeapon, ActorClass.Barbarian), Skills.Barbarian.SeismicSlam},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.MightyWeapon, ActorClass.Barbarian), Skills.Barbarian.AncientSpear},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.MightyWeapon, ActorClass.Barbarian), Skills.Barbarian.HammerOfTheAncients},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.MightyWeapon, ActorClass.Barbarian), Skills.Barbarian.Avalanche},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.MightyWeapon, ActorClass.Barbarian), Skills.Barbarian.Earthquake},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.MightyWeapon, ActorClass.Barbarian), Skills.Barbarian.Overpower},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.MightyWeapon, ActorClass.Barbarian), Skills.Barbarian.Rend},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.MightyWeapon, ActorClass.Barbarian), Skills.Barbarian.Revenge},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.MightyWeapon, ActorClass.Barbarian), Skills.Barbarian.Frenzy},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.MightyWeapon, ActorClass.Barbarian), Skills.Barbarian.Cleave},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.MightyWeapon, ActorClass.Barbarian), Skills.Barbarian.Bash},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.MightyWeapon, ActorClass.Barbarian), Skills.Barbarian.WeaponThrow},

            {new KeyValuePair<ItemType, ActorClass>(ItemType.Daibo, ActorClass.Monk), Skills.Monk.SevensidedStrike},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Daibo, ActorClass.Monk), Skills.Monk.LashingTailKick},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Daibo, ActorClass.Monk), Skills.Monk.WaveOfLight},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Daibo, ActorClass.Monk), Skills.Monk.ExplodingPalm},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Daibo, ActorClass.Monk), Skills.Monk.TempestRush},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Daibo, ActorClass.Monk), Skills.Monk.FistsOfThunder},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Daibo, ActorClass.Monk), Skills.Monk.CripplingWave},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Daibo, ActorClass.Monk), Skills.Monk.WayOfTheHundredFists},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Daibo, ActorClass.Monk), Skills.Monk.DeadlyReach},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Daibo, ActorClass.Monk), Skills.Monk.DashingStrike},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Daibo, ActorClass.Monk), Skills.Monk.MysticAlly},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Daibo, ActorClass.Monk), Skills.Monk.SweepingWind},
            {new KeyValuePair<ItemType, ActorClass>(ItemType.Daibo, ActorClass.Monk), Skills.Monk.CycloneStrike},

        };

    }
}
