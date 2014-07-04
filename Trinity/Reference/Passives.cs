using System;
using System.Collections.Generic;
using System.Linq;
using Trinity.Helpers;
using Trinity.Objects;
using Zeta.Game;
using Zeta.Game.Internals.Actors;

// AUTO-GENERATED on Sat, 28 Jun 2014 09:34:14 GMT

namespace Trinity.Reference
{
    public static class Passives
    {
        public class DemonHunter : FieldCollection<DemonHunter, Passive>
        {

            /// <summary>
            /// Every 6 seconds, your next skill that costs Hatred will immobilize all enemies hit for 2 seconds. 
            /// </summary>
            public static Passive ThrillOfTheHunt = new Passive
            {
                Index = 1,
                Name = "Thrill of the Hunt",
                SNOPower = SNOPower.DemonHunter_Passive_ThrillOfTheHunt,
                RequiredLevel = 10,
                Description = " Every 6 seconds, your next skill that costs Hatred will immobilize all enemies hit for 2 seconds. ",
                Tooltip = "skill/demon-hunter/thrill-of-the-hunt",
                Class = ActorClass.DemonHunter
            };

            /// <summary>
            /// Whenever you use Vault, Smoke Screen, or backflip with Evasive Fire you gain 60% movement speed for 2 seconds. 
            /// </summary>
            public static Passive TacticalAdvantage = new Passive
            {
                Index = 2,
                Name = "Tactical Advantage",
                SNOPower = SNOPower.DemonHunter_Passive_TacticalAdvantage,
                RequiredLevel = 10,
                Description = " Whenever you use Vault, Smoke Screen, or backflip with Evasive Fire you gain 60% movement speed for 2 seconds. ",
                Tooltip = "skill/demon-hunter/tactical-advantage",
                Class = ActorClass.DemonHunter
            };

            /// <summary>
            /// Your maximum Hatred is increased by 25. In addition, gain 30 Hatred and 3 Discipline when you are healed by a health globe. 
            /// </summary>
            public static Passive BloodVengeance = new Passive
            {
                Index = 3,
                Name = "Blood Vengeance",
                SNOPower = SNOPower.DemonHunter_Passive_Vengeance,
                RequiredLevel = 13,
                Description = " Your maximum Hatred is increased by 25. In addition, gain 30 Hatred and 3 Discipline when you are healed by a health globe. ",
                Tooltip = "skill/demon-hunter/blood-vengeance",
                Class = ActorClass.DemonHunter
            };

            /// <summary>
            /// As long as there are no enemies within 10 yards, all damage is increased by 20%. 
            /// </summary>
            public static Passive SteadyAim = new Passive
            {
                Index = 4,
                Name = "Steady Aim",
                SNOPower = SNOPower.DemonHunter_Passive_SteadyAim,
                RequiredLevel = 16,
                Description = " As long as there are no enemies within 10 yards, all damage is increased by 20%. ",
                Tooltip = "skill/demon-hunter/steady-aim",
                Class = ActorClass.DemonHunter
            };

            /// <summary>
            /// Increase damage against slowed enemies by 20%. 
            /// </summary>
            public static Passive CullTheWeak = new Passive
            {
                Index = 5,
                Name = "Cull the Weak",
                SNOPower = SNOPower.DemonHunter_Passive_CullTheWeak,
                RequiredLevel = 20,
                Description = " Increase damage against slowed enemies by 20%. ",
                Tooltip = "skill/demon-hunter/cull-the-weak",
                Class = ActorClass.DemonHunter
            };

            /// <summary>
            /// Critical Hits have a chance to restore 1 Discipline. Discipline is used to fuel many of your tactical and defensive skills. 
            /// </summary>
            public static Passive NightStalker = new Passive
            {
                Index = 6,
                Name = "Night Stalker",
                SNOPower = SNOPower.DemonHunter_Passive_NightStalker,
                RequiredLevel = 20,
                Description = " Critical Hits have a chance to restore 1 Discipline. Discipline is used to fuel many of your tactical and defensive skills. ",
                Tooltip = "skill/demon-hunter/night-stalker",
                Class = ActorClass.DemonHunter
            };

            /// <summary>
            /// Gain 1.5% Life regeneration per second for every second you remain stationary, stacking up to 3 times. This bonus is reset 5 seconds after you move. 
            /// </summary>
            public static Passive Brooding = new Passive
            {
                Index = 7,
                Name = "Brooding",
                SNOPower = SNOPower.DemonHunter_Passive_Brooding,
                RequiredLevel = 25,
                Description = " Gain 1.5% Life regeneration per second for every second you remain stationary, stacking up to 3 times. This bonus is reset 5 seconds after you move. ",
                Tooltip = "skill/demon-hunter/brooding",
                Class = ActorClass.DemonHunter
            };

            /// <summary>
            /// Increase movement speed by 20% for 2 seconds when you hit an enemy. 
            /// </summary>
            public static Passive HotPursuit = new Passive
            {
                Index = 8,
                Name = "Hot Pursuit",
                SNOPower = SNOPower.DemonHunter_Passive_HotPursuit,
                RequiredLevel = 27,
                Description = " Increase movement speed by 20% for 2 seconds when you hit an enemy. ",
                Tooltip = "skill/demon-hunter/hot-pursuit",
                Class = ActorClass.DemonHunter
            };

            /// <summary>
            /// Gain a bonus based on your weapon type: Bow: 8% increased damage Crossbow: 50% Critical Hit Damage Hand Crossbow: 5% Critical Hit Chance 2nd Hand Crossbow: 1 Hatred per Second 
            /// </summary>
            public static Passive Archery = new Passive
            {
                Index = 9,
                Name = "Archery",
                SNOPower = SNOPower.DemonHunter_Passive_Archery,
                RequiredLevel = 30,
                Description = " Gain a bonus based on your weapon type: Bow: 8% increased damage Crossbow: 50% Critical Hit Damage Hand Crossbow: 5% Critical Hit Chance 2nd Hand Crossbow: 1 Hatred per Second ",
                Tooltip = "skill/demon-hunter/archery",
                Class = ActorClass.DemonHunter
            };

            /// <summary>
            /// Enemies you Slow or hit with Fan of Knives, Spike Trap, Caltrops, Grenades, and Sentry fire have their damage reduced by 25% for 3 seconds. 
            /// </summary>
            public static Passive NumbingTraps = new Passive
            {
                Index = 10,
                Name = "Numbing Traps",
                SNOPower = SNOPower.DemonHunter_Passive_NumbingTraps,
                RequiredLevel = 30,
                Description = " Enemies you Slow or hit with Fan of Knives, Spike Trap, Caltrops, Grenades, and Sentry fire have their damage reduced by 25% for 3 seconds. ",
                Tooltip = "skill/demon-hunter/numbing-traps",
                Class = ActorClass.DemonHunter
            };

            /// <summary>
            /// Reduce the Discipline cost of all skills by 10%. Increase your Life, Armor, and resistance to all elements by 10%. Discipline is used to fuel many of your tactical and defensive skills. 
            /// </summary>
            public static Passive Perfectionist = new Passive
            {
                Index = 11,
                Name = "Perfectionist",
                SNOPower = SNOPower.DemonHunter_Passive_Perfectionist,
                RequiredLevel = 35,
                Description = " Reduce the Discipline cost of all skills by 10%. Increase your Life, Armor, and resistance to all elements by 10%. Discipline is used to fuel many of your tactical and defensive skills. ",
                Tooltip = "skill/demon-hunter/perfectionist",
                Class = ActorClass.DemonHunter
            };

            /// <summary>
            /// Increase the duration of your Caltrops, Marked for Death, Spike Trap, and Sentry by 100%. Increase the maximum number of Sentries to 3 and Spike Traps to 6. 
            /// </summary>
            public static Passive CustomEngineering = new Passive
            {
                Index = 12,
                Name = "Custom Engineering",
                SNOPower = SNOPower.DemonHunter_Passive_CustomEngineering,
                RequiredLevel = 40,
                Description = " Increase the duration of your Caltrops, Marked for Death, Spike Trap, and Sentry by 100%. Increase the maximum number of Sentries to 3 and Spike Traps to 6. ",
                Tooltip = "skill/demon-hunter/custom-engineering",
                Class = ActorClass.DemonHunter
            };

            /// <summary>
            /// Increase the damage of grenades by 10%. Increase the explosion size of grenades by 20%. Upon death, you drop a giant grenade that explodes for 1000% weapon damage as Fire. 
            /// </summary>
            public static Passive Grenadier = new Passive
            {
                Index = 13,
                Name = "Grenadier",
                SNOPower = SNOPower.DemonHunter_Passive_Grenadier,
                RequiredLevel = 45,
                Description = " Increase the damage of grenades by 10%. Increase the explosion size of grenades by 20%. Upon death, you drop a giant grenade that explodes for 1000% weapon damage as Fire. ",
                Tooltip = "skill/demon-hunter/grenadier",
                Class = ActorClass.DemonHunter
            };

            /// <summary>
            /// Gain 4% Critical Hit Chance every second. This bonus is reset 1 seconds after you successfully critically hit. 
            /// </summary>
            public static Passive Sharpshooter = new Passive
            {
                Index = 14,
                Name = "Sharpshooter",
                SNOPower = SNOPower.DemonHunter_Passive_Sharpshooter,
                RequiredLevel = 50,
                Description = " Gain 4% Critical Hit Chance every second. This bonus is reset 1 seconds after you successfully critically hit. ",
                Tooltip = "skill/demon-hunter/sharpshooter",
                Class = ActorClass.DemonHunter
            };

            /// <summary>
            /// Increase damage of rockets by 100%. In addition, you have a 20% chance to fire a homing rocket for 150% weapon damage when you attack. 
            /// </summary>
            public static Passive Ballistics = new Passive
            {
                Index = 15,
                Name = "Ballistics",
                SNOPower = SNOPower.DemonHunter_Passive_Ballistics,
                RequiredLevel = 55,
                Description = " Increase damage of rockets by 100%. In addition, you have a 20% chance to fire a homing rocket for 150% weapon damage when you attack. ",
                Tooltip = "skill/demon-hunter/ballistics",
                Class = ActorClass.DemonHunter
            };

            /// <summary>
            /// You deal 40% additional damage to enemies above 75% health. 
            /// </summary>
            public static Passive Ambush = new Passive
            {
                Index = 16,
                Name = "Ambush",
                SNOPower = SNOPower.X1_DemonHunter_Passive_Ambush,
                RequiredLevel = 64,
                Description = " You deal 40% additional damage to enemies above 75% health. ",
                Tooltip = "skill/demon-hunter/ambush",
                Class = ActorClass.DemonHunter
            };

            /// <summary>
            /// Your Armor is increased by 30% of your Dexterity. 
            /// </summary>
            public static Passive Awareness = new Passive
            {
                Index = 17,
                Name = "Awareness",
                SNOPower = SNOPower.X1_DemonHunter_Passive_Awareness,
                RequiredLevel = 66,
                Description = " Your Armor is increased by 30% of your Dexterity. ",
                Tooltip = "skill/demon-hunter/awareness",
                Class = ActorClass.DemonHunter
            };

            /// <summary>
            /// Gain 25% Critical Hit Chance against enemies who are more than 20 yards away from any other enemies. 
            /// </summary>
            public static Passive SingleOut = new Passive
            {
                Index = 18,
                Name = "Single Out",
                SNOPower = SNOPower.X1_DemonHunter_Passive_SingleOut,
                RequiredLevel = 68,
                Description = " Gain 25% Critical Hit Chance against enemies who are more than 20 yards away from any other enemies. ",
                Tooltip = "skill/demon-hunter/single-out",
                Class = ActorClass.DemonHunter
            };
        }
        public class WitchDoctor : FieldCollection<WitchDoctor, Passive>
        {

            /// <summary>
            /// Reduce all damage taken by you and your pets by 15%. 
            /// </summary>
            public static Passive JungleFortitude = new Passive
            {
                Index = 1,
                Name = "Jungle Fortitude",
                SNOPower = SNOPower.Witchdoctor_Passive_JungleFortitude,
                RequiredLevel = 10,
                Description = " Reduce all damage taken by you and your pets by 15%. ",
                Tooltip = "skill/witch-doctor/jungle-fortitude",
                Class = ActorClass.Witchdoctor
            };

            /// <summary>
            /// When an enemy dies within 20 yards, there is a 30% chance that a Zombie Dog will automatically emerge. The range of this effect is increased by your gold pickup radius. 
            /// </summary>
            public static Passive CircleOfLife = new Passive
            {
                Index = 2,
                Name = "Circle of Life",
                SNOPower = SNOPower.Witchdoctor_Passive_CircleOfLife,
                RequiredLevel = 10,
                Description = " When an enemy dies within 20 yards, there is a 30% chance that a Zombie Dog will automatically emerge. The range of this effect is increased by your gold pickup radius. ",
                Tooltip = "skill/witch-doctor/circle-of-life",
                Class = ActorClass.Witchdoctor
            };

            /// <summary>
            /// Maximum Mana is increased by 10%. Regenerate 1% of your maximum Mana per second. Mana fuels your offensive and defensive skills. 
            /// </summary>
            public static Passive SpiritualAttunement = new Passive
            {
                Index = 3,
                Name = "Spiritual Attunement",
                SNOPower = SNOPower.Witchdoctor_Passive_SpiritualAttunement,
                RequiredLevel = 13,
                Description = " Maximum Mana is increased by 10%. Regenerate 1% of your maximum Mana per second. Mana fuels your offensive and defensive skills. ",
                Tooltip = "skill/witch-doctor/spiritual-attunement",
                Class = ActorClass.Witchdoctor
            };

            /// <summary>
            /// When you are healed by a health globe, gain 10% of your maximum Mana and 10% Intelligence for 15 seconds. The Intelligence bonus stacks up to 5 times. 
            /// </summary>
            public static Passive GruesomeFeast = new Passive
            {
                Index = 4,
                Name = "Gruesome Feast",
                SNOPower = SNOPower.Witchdoctor_Passive_GruesomeFeast,
                RequiredLevel = 16,
                Description = " When you are healed by a health globe, gain 10% of your maximum Mana and 10% Intelligence for 15 seconds. The Intelligence bonus stacks up to 5 times. ",
                Tooltip = "skill/witch-doctor/gruesome-feast",
                Class = ActorClass.Witchdoctor
            };

            /// <summary>
            /// 10% of Mana costs are paid with Life. In addition, you regenerate 1% of your maximum Life per second. 
            /// </summary>
            public static Passive BloodRitual = new Passive
            {
                Index = 5,
                Name = "Blood Ritual",
                SNOPower = SNOPower.Witchdoctor_Passive_BloodRitual,
                RequiredLevel = 20,
                Description = " 10% of Mana costs are paid with Life. In addition, you regenerate 1% of your maximum Life per second. ",
                Tooltip = "skill/witch-doctor/blood-ritual",
                Class = ActorClass.Witchdoctor
            };

            /// <summary>
            /// When you deal Poison damage to an enemy, its damage is reduced by 20% for 3 seconds. 
            /// </summary>
            public static Passive BadMedicine = new Passive
            {
                Index = 6,
                Name = "Bad Medicine",
                SNOPower = SNOPower.Witchdoctor_Passive_BadMedicine,
                RequiredLevel = 20,
                Description = " When you deal Poison damage to an enemy, its damage is reduced by 20% for 3 seconds. ",
                Tooltip = "skill/witch-doctor/bad-medicine",
                Class = ActorClass.Witchdoctor
            };

            /// <summary>
            /// You can have 1 additional Zombie Dog summoned at one time. The healths of you, your Zombie Dogs and Gargantuan are increased by 20%. 
            /// </summary>
            public static Passive ZombieHandler = new Passive
            {
                Index = 7,
                Name = "Zombie Handler",
                SNOPower = SNOPower.Witchdoctor_Passive_ZombieHandler,
                RequiredLevel = 24,
                Description = " You can have 1 additional Zombie Dog summoned at one time. The healths of you, your Zombie Dogs and Gargantuan are increased by 20%. ",
                Tooltip = "skill/witch-doctor/zombie-handler",
                Class = ActorClass.Witchdoctor
            };

            /// <summary>
            /// All of your damage is increased by 20%, but your Mana costs are increased by 30%. 
            /// </summary>
            public static Passive PierceTheVeil = new Passive
            {
                Index = 8,
                Name = "Pierce the Veil",
                SNOPower = SNOPower.Witchdoctor_Passive_PierceTheVeil,
                RequiredLevel = 27,
                Description = " All of your damage is increased by 20%, but your Mana costs are increased by 30%. ",
                Tooltip = "skill/witch-doctor/pierce-the-veil",
                Class = ActorClass.Witchdoctor
            };

            /// <summary>
            /// Reduce the cooldown of your Horrify, Spirit Walk, and Soul Harvest spells by 2 seconds. In addition, the next time you receive fatal damage, you automatically enter the spirit realm for 2 seconds and heal to 15% of your maximum Life. This effect may occur once every 90 seconds. 
            /// </summary>
            public static Passive SpiritVessel = new Passive
            {
                Index = 9,
                Name = "Spirit Vessel",
                SNOPower = SNOPower.Witchdoctor_Passive_SpiritVessel,
                RequiredLevel = 30,
                Description = " Reduce the cooldown of your Horrify, Spirit Walk, and Soul Harvest spells by 2 seconds. In addition, the next time you receive fatal damage, you automatically enter the spirit realm for 2 seconds and heal to 15% of your maximum Life. This effect may occur once every 90 seconds. ",
                Tooltip = "skill/witch-doctor/spirit-vessel",
                Class = ActorClass.Witchdoctor
            };

            /// <summary>
            /// When you hit enemies with your spells, you have up to a 10% chance to summon a dagger-wielding Fetish to fight by your side for 60 seconds. 
            /// </summary>
            public static Passive FetishSycophants = new Passive
            {
                Index = 10,
                Name = "Fetish Sycophants",
                SNOPower = SNOPower.Witchdoctor_Passive_FetishSycophants,
                RequiredLevel = 30,
                Description = " When you hit enemies with your spells, you have up to a 10% chance to summon a dagger-wielding Fetish to fight by your side for 60 seconds. ",
                Tooltip = "skill/witch-doctor/fetish-sycophants",
                Class = ActorClass.Witchdoctor
            };

            /// <summary>
            /// Spirit spells return 100 Mana over 10 seconds. Spirit spells are: Haunt Horrify Mass Confusion Soul Harvest Spirit Barrage Spirit Walk 
            /// </summary>
            public static Passive RushOfEssence = new Passive
            {
                Index = 11,
                Name = "Rush of Essence",
                SNOPower = SNOPower.Witchdoctor_Passive_RushOfEssence,
                RequiredLevel = 36,
                Description = " Spirit spells return 100 Mana over 10 seconds. Spirit spells are: Haunt Horrify Mass Confusion Soul Harvest Spirit Barrage Spirit Walk ",
                Tooltip = "skill/witch-doctor/rush-of-essence",
                Class = ActorClass.Witchdoctor
            };

            /// <summary>
            /// When you deal damage with Corpse Spiders, Firebomb, Plague of Toads, or Poison Dart, your Mana regeneration is increased by 30% for 5 seconds. 
            /// </summary>
            public static Passive VisionQuest = new Passive
            {
                Index = 12,
                Name = "Vision Quest",
                SNOPower = SNOPower.Witchdoctor_Passive_VisionQuest,
                RequiredLevel = 40,
                Description = " When you deal damage with Corpse Spiders, Firebomb, Plague of Toads, or Poison Dart, your Mana regeneration is increased by 30% for 5 seconds. ",
                Tooltip = "skill/witch-doctor/vision-quest",
                Class = ActorClass.Witchdoctor
            };

            /// <summary>
            /// You can have 1 additional Zombie Dog summoned at one time. While you have a Zombie Dog, Gargantuan, or Fetish following you and not in combat, your movement speed is increased by 30%. 
            /// </summary>
            public static Passive FierceLoyalty = new Passive
            {
                Index = 13,
                Name = "Fierce Loyalty",
                SNOPower = SNOPower.Witchdoctor_Passive_FierceLoyalty,
                RequiredLevel = 45,
                Description = " You can have 1 additional Zombie Dog summoned at one time. While you have a Zombie Dog, Gargantuan, or Fetish following you and not in combat, your movement speed is increased by 30%. ",
                Tooltip = "skill/witch-doctor/fierce-loyalty",
                Class = ActorClass.Witchdoctor
            };

            /// <summary>
            /// Gain 1% of your maximum Life and Mana and reduce the cooldown of all of your skills by 1 second when an enemy dies within 20 yards. The range is extended by items that increase your gold pickup radius. 
            /// </summary>
            public static Passive GraveInjustice = new Passive
            {
                Index = 14,
                Name = "Grave Injustice",
                SNOPower = SNOPower.Witchdoctor_Passive_GraveInjustice,
                RequiredLevel = 50,
                Description = " Gain 1% of your maximum Life and Mana and reduce the cooldown of all of your skills by 1 second when an enemy dies within 20 yards. The range is extended by items that increase your gold pickup radius. ",
                Tooltip = "skill/witch-doctor/grave-injustice",
                Class = ActorClass.Witchdoctor
            };

            /// <summary>
            /// Reduce the cooldowns of your Fetish Army, Big Bad Voodoo, Hex, Gargantuan, Summon Zombie Dogs and Mass Confusion skills by 25%. 
            /// </summary>
            public static Passive TribalRites = new Passive
            {
                Index = 15,
                Name = "Tribal Rites",
                SNOPower = SNOPower.Witchdoctor_Passive_TribalRites,
                RequiredLevel = 55,
                Description = " Reduce the cooldowns of your Fetish Army, Big Bad Voodoo, Hex, Gargantuan, Summon Zombie Dogs and Mass Confusion skills by 25%. ",
                Tooltip = "skill/witch-doctor/tribal-rites",
                Class = ActorClass.Witchdoctor
            };

            /// <summary>
            /// Your Haunt, Locust Swarm and the damage amplification from Piranhas last almost forever. 
            /// </summary>
            public static Passive CreepingDeath = new Passive
            {
                Index = 16,
                Name = "Creeping Death",
                SNOPower = SNOPower.Witchdoctor_Passive_CreepingDeath,
                RequiredLevel = 64,
                Description = " Your Haunt, Locust Swarm and the damage amplification from Piranhas last almost forever. ",
                Tooltip = "skill/witch-doctor/creeping-death",
                Class = ActorClass.Witchdoctor
            };

            /// <summary>
            /// You gain 70 Physical Resistance for every enemy within 20 yards. The range of this effect is increased by your gold pickup radius. 
            /// </summary>
            public static Passive PhysicalAttunement = new Passive
            {
                Index = 17,
                Name = "Physical Attunement",
                SNOPower = SNOPower.Witchdoctor_Passive_PhysicalAttunement,
                RequiredLevel = 66,
                Description = " You gain 70 Physical Resistance for every enemy within 20 yards. The range of this effect is increased by your gold pickup radius. ",
                Tooltip = "skill/witch-doctor/physical-attunement",
                Class = ActorClass.Witchdoctor
            };

            /// <summary>
            /// You can have 1 additional Zombie Dog summoned at one time. The damage of your Zombie Dogs and Gargantuan is increased 50%. 
            /// </summary>
            public static Passive MidnightFeast = new Passive
            {
                Index = 18,
                Name = "Midnight Feast",
                SNOPower = SNOPower.Witchdoctor_Passive_MidnightFeast,
                RequiredLevel = 68,
                Description = " You can have 1 additional Zombie Dog summoned at one time. The damage of your Zombie Dogs and Gargantuan is increased 50%. ",
                Tooltip = "skill/witch-doctor/midnight-feast",
                Class = ActorClass.Witchdoctor
            };
        }
        public class Monk : FieldCollection<Monk, Passive>
        {

            /// <summary>
            /// Damage you deal reduces enemy damage by 20% for 2.5 seconds. 
            /// </summary>
            public static Passive Resolve = new Passive
            {
                Index = 1,
                Name = "Resolve",
                SNOPower = SNOPower.Monk_Passive_Resolve,
                RequiredLevel = 10,
                Description = " Damage you deal reduces enemy damage by 20% for 2.5 seconds. ",
                Tooltip = "skill/monk/resolve",
                Class = ActorClass.Monk
            };

            /// <summary>
            /// Increase movement speed by 10%. 
            /// </summary>
            public static Passive FleetFooted = new Passive
            {
                Index = 2,
                Name = "Fleet Footed",
                SNOPower = SNOPower.Monk_Passive_FleetFooted,
                RequiredLevel = 10,
                Description = " Increase movement speed by 10%. ",
                Tooltip = "skill/monk/fleet-footed",
                Class = ActorClass.Monk
            };

            /// <summary>
            /// Increase maximum Spirit by 100 and increase Spirit Regeneration by 2 per second. Spirit fuels your defensive and offensive abilities. 
            /// </summary>
            public static Passive ExaltedSoul = new Passive
            {
                Index = 3,
                Name = "Exalted Soul",
                SNOPower = SNOPower.Monk_Passive_ExaltedSoul,
                RequiredLevel = 13,
                Description = " Increase maximum Spirit by 100 and increase Spirit Regeneration by 2 per second. Spirit fuels your defensive and offensive abilities. ",
                Tooltip = "skill/monk/exalted-soul",
                Class = ActorClass.Monk
            };

            /// <summary>
            /// Every point of Spirit spent heals you for 248 Life. Heal amount is increased by 0.4% of your Health Globe Healing Bonus. 
            /// </summary>
            public static Passive Transcendence = new Passive
            {
                Index = 4,
                Name = "Transcendence",
                SNOPower = SNOPower.Monk_Passive_Transcendence,
                RequiredLevel = 16,
                Description = " Every point of Spirit spent heals you for 248 Life. Heal amount is increased by 0.4% of your Health Globe Healing Bonus. ",
                Tooltip = "skill/monk/transcendence",
                Class = ActorClass.Monk
            };

            /// <summary>
            /// The Spirit costs of Mantra activation effects are reduced by 50% and you gain 2 Spirit every second when you have a Mantra learned. 
            /// </summary>
            public static Passive ChantOfResonance = new Passive
            {
                Index = 5,
                Name = "Chant of Resonance",
                SNOPower = SNOPower.Monk_Passive_ChantOfResonance,
                RequiredLevel = 20,
                Description = " The Spirit costs of Mantra activation effects are reduced by 50% and you gain 2 Spirit every second when you have a Mantra learned. ",
                Tooltip = "skill/monk/chant-of-resonance",
                Class = ActorClass.Monk
            };

            /// <summary>
            /// Your Armor is increased by 30% of your Dexterity. 
            /// </summary>
            public static Passive SeizeTheInitiative = new Passive
            {
                Index = 6,
                Name = "Seize the Initiative",
                SNOPower = SNOPower.Monk_Passive_SeizeTheInitiative,
                RequiredLevel = 20,
                Description = " Your Armor is increased by 30% of your Dexterity. ",
                Tooltip = "skill/monk/seize-the-initiative",
                Class = ActorClass.Monk
            };

            /// <summary>
            /// While dual-wielding, you gain a 15% chance to dodge incoming attacks. While using a two-handed weapon, all Spirit generation is increased by 35%. 
            /// </summary>
            public static Passive TheGuardiansPath = new Passive
            {
                Index = 7,
                Name = "The Guardian's Path",
                SNOPower = SNOPower.Monk_Passive_TheGuardiansPath,
                RequiredLevel = 24,
                Description = " While dual-wielding, you gain a 15% chance to dodge incoming attacks. While using a two-handed weapon, all Spirit generation is increased by 35%. ",
                Tooltip = "skill/monk/the-guardians-path",
                Class = ActorClass.Monk
            };

            /// <summary>
            /// Your dodge chance is increased by an amount equal to 42.5% of your Critical Hit Chance. 
            /// </summary>
            public static Passive SixthSense = new Passive
            {
                Index = 8,
                Name = "Sixth Sense",
                SNOPower = SNOPower.Monk_Passive_SixthSense,
                RequiredLevel = 27,
                Description = " Your dodge chance is increased by an amount equal to 42.5% of your Critical Hit Chance. ",
                Tooltip = "skill/monk/sixth-sense",
                Class = ActorClass.Monk
            };

            /// <summary>
            /// The duration of control-impairing effects on you are reduced by 25%. Whenever you are hit by a Stun, Freeze, Fear, Immobilize or Charm, you gain 15% increased damage for 10 seconds. 
            /// </summary>
            public static Passive Provocation = new Passive
            {
                Index = 9,
                Name = "Provocation",
                SNOPower = SNOPower.Monk_Passive_Pacifism,
                RequiredLevel = 30,
                Description = " The duration of control-impairing effects on you are reduced by 25%. Whenever you are hit by a Stun, Freeze, Fear, Immobilize or Charm, you gain 15% increased damage for 10 seconds. ",
                Tooltip = "skill/monk/provocation",
                Class = ActorClass.Monk
            };

            /// <summary>
            /// Reduce all cooldowns by 20%. 
            /// </summary>
            public static Passive BeaconOfYtar = new Passive
            {
                Index = 10,
                Name = "Beacon of Ytar",
                SNOPower = SNOPower.Monk_Passive_BeaconOfYtar,
                RequiredLevel = 35,
                Description = " Reduce all cooldowns by 20%. ",
                Tooltip = "skill/monk/beacon-of-ytar",
                Class = ActorClass.Monk
            };

            /// <summary>
            /// Your heals and shields grant increased damage equal to the percentage of Life missing, up to a maximum of 30%, for 10 seconds. 
            /// </summary>
            public static Passive GuidingLight = new Passive
            {
                Index = 11,
                Name = "Guiding Light",
                SNOPower = SNOPower.Monk_Passive_GuidingLight,
                RequiredLevel = 40,
                Description = " Your heals and shields grant increased damage equal to the percentage of Life missing, up to a maximum of 30%, for 10 seconds. ",
                Tooltip = "skill/monk/guiding-light",
                Class = ActorClass.Monk
            };

            /// <summary>
            /// Your resistance to all elements is equal to your highest elemental resistance. 
            /// </summary>
            public static Passive OneWithEverything = new Passive
            {
                Index = 12,
                Name = "One With Everything",
                SNOPower = SNOPower.Monk_Passive_OneWithEverything,
                RequiredLevel = 45,
                Description = " Your resistance to all elements is equal to your highest elemental resistance. ",
                Tooltip = "skill/monk/one-with-everything",
                Class = ActorClass.Monk
            };

            /// <summary>
            /// Each different Spirit Generator you use increases your damage by 10% for 3 seconds. 
            /// </summary>
            public static Passive CombinationStrike = new Passive
            {
                Index = 13,
                Name = "Combination Strike",
                SNOPower = SNOPower.Monk_Passive_CombinationStrike,
                RequiredLevel = 50,
                Description = " Each different Spirit Generator you use increases your damage by 10% for 3 seconds. ",
                Tooltip = "skill/monk/combination-strike",
                Class = ActorClass.Monk
            };

            /// <summary>
            /// When receiving fatal damage, you are instead restored to 35% of maximum Life and 35% Spirit. This effect may occur once every 60 seconds. When Near Death Experience is on cooldown, your Health Globe Healing Bonus, Life per Second and Life per Hit are increased by 35%. 
            /// </summary>
            public static Passive NearDeathExperience = new Passive
            {
                Index = 14,
                Name = "Near Death Experience",
                SNOPower = SNOPower.Monk_Passive_NearDeathExperience,
                RequiredLevel = 58,
                Description = " When receiving fatal damage, you are instead restored to 35% of maximum Life and 35% Spirit. This effect may occur once every 60 seconds. When Near Death Experience is on cooldown, your Health Globe Healing Bonus, Life per Second and Life per Hit are increased by 35%. ",
                Tooltip = "skill/monk/near-death-experience",
                Class = ActorClass.Monk
            };

            /// <summary>
            /// Each ally affected by your Mantras increases your damage by 5%, up to a maximum of 20%, and has 5% increased damage. 
            /// </summary>
            public static Passive Unity = new Passive
            {
                Index = 15,
                Name = "Unity",
                SNOPower = SNOPower.X1_Monk_Passive_Unity,
                RequiredLevel = 64,
                Description = " Each ally affected by your Mantras increases your damage by 5%, up to a maximum of 20%, and has 5% increased damage. ",
                Tooltip = "skill/monk/unity",
                Class = ActorClass.Monk
            };

            /// <summary>
            /// Moving 25 yards increases your damage by 20% for 6 seconds. 
            /// </summary>
            public static Passive Momentum = new Passive
            {
                Index = 16,
                Name = "Momentum",
                SNOPower = SNOPower.X1_Monk_Passive_Momentum,
                RequiredLevel = 66,
                Description = " Moving 25 yards increases your damage by 20% for 6 seconds. ",
                Tooltip = "skill/monk/momentum",
                Class = ActorClass.Monk
            };

            /// <summary>
            /// Every third hit from a Spirit Generator increases the damage of your next damaging Spirit Spender by 40%. 
            /// </summary>
            public static Passive MythicRhythm = new Passive
            {
                Index = 17,
                Name = "Mythic Rhythm",
                SNOPower = SNOPower.X1_Monk_Passive_MythicRhythm,
                RequiredLevel = 68,
                Description = " Every third hit from a Spirit Generator increases the damage of your next damaging Spirit Spender by 40%. ",
                Tooltip = "skill/monk/mythic-rhythm",
                Class = ActorClass.Monk
            };
        }
        public class Barbarian : FieldCollection<Barbarian, Passive>
        {

            /// <summary>
            /// Gain 50% additional Life from health globes. 
            /// </summary>
            public static Passive PoundOfFlesh = new Passive
            {
                Index = 1,
                Name = "Pound of Flesh",
                SNOPower = SNOPower.Barbarian_Passive_PoundOfFlesh,
                RequiredLevel = 10,
                Description = " Gain 50% additional Life from health globes. ",
                Tooltip = "skill/barbarian/pound-of-flesh",
                Class = ActorClass.Barbarian
            };

            /// <summary>
            /// You deal 40% additional damage to enemies below 30% health. 
            /// </summary>
            public static Passive Ruthless = new Passive
            {
                Index = 2,
                Name = "Ruthless",
                SNOPower = SNOPower.Barbarian_Passive_Ruthless,
                RequiredLevel = 10,
                Description = " You deal 40% additional damage to enemies below 30% health. ",
                Tooltip = "skill/barbarian/ruthless",
                Class = ActorClass.Barbarian
            };

            /// <summary>
            /// Increase your Armor by 50% of your Vitality. 
            /// </summary>
            public static Passive NervesOfSteel = new Passive
            {
                Index = 3,
                Name = "Nerves of Steel",
                SNOPower = SNOPower.Barbarian_Passive_NervesOfSteel,
                RequiredLevel = 13,
                Description = " Increase your Armor by 50% of your Vitality. ",
                Tooltip = "skill/barbarian/nerves-of-steel",
                Class = ActorClass.Barbarian
            };

            /// <summary>
            /// Gain a bonus based on the weapon type of your main hand weapon: Swords/Daggers: 8% increased damage Maces/Axes: 5% Critical Hit Chance Polearms/Spears: 8% attack speed Mighty Weapons: 1 Fury per hit 
            /// </summary>
            public static Passive WeaponsMaster = new Passive
            {
                Index = 4,
                Name = "Weapons Master",
                SNOPower = SNOPower.Barbarian_Passive_WeaponsMaster,
                RequiredLevel = 16,
                Description = " Gain a bonus based on the weapon type of your main hand weapon: Swords/Daggers: 8% increased damage Maces/Axes: 5% Critical Hit Chance Polearms/Spears: 8% attack speed Mighty Weapons: 1 Fury per hit ",
                Tooltip = "skill/barbarian/weapons-master",
                Class = ActorClass.Barbarian
            };

            /// <summary>
            /// The duration of your shouts is doubled. After using a shout you and all allies within 100 yards regenerate 1% of maximum Life per second for 60 seconds. Your shouts are: Battle Rage Threatening Shout War Cry 
            /// </summary>
            public static Passive InspiringPresence = new Passive
            {
                Index = 5,
                Name = "Inspiring Presence",
                SNOPower = SNOPower.Barbarian_Passive_InspiringPresence,
                RequiredLevel = 20,
                Description = " The duration of your shouts is doubled. After using a shout you and all allies within 100 yards regenerate 1% of maximum Life per second for 60 seconds. Your shouts are: Battle Rage Threatening Shout War Cry ",
                Tooltip = "skill/barbarian/inspiring-presence",
                Class = ActorClass.Barbarian
            };

            /// <summary>
            /// You deal 25% additional damage while at maximum Fury. 
            /// </summary>
            public static Passive BerserkerRage = new Passive
            {
                Index = 6,
                Name = "Berserker Rage",
                SNOPower = SNOPower.Barbarian_Passive_BerserkerRage,
                RequiredLevel = 20,
                Description = " You deal 25% additional damage while at maximum Fury. ",
                Tooltip = "skill/barbarian/berserker-rage",
                Class = ActorClass.Barbarian
            };

            /// <summary>
            /// Each point of Fury spent heals you for 578 Life. Heal amount is increased by 1% of your Health Globe Healing Bonus. 
            /// </summary>
            public static Passive Bloodthirst = new Passive
            {
                Index = 7,
                Name = "Bloodthirst",
                SNOPower = SNOPower.Barbarian_Passive_Bloodthirst,
                RequiredLevel = 24,
                Description = " Each point of Fury spent heals you for 578 Life. Heal amount is increased by 1% of your Health Globe Healing Bonus. ",
                Tooltip = "skill/barbarian/bloodthirst",
                Class = ActorClass.Barbarian
            };

            /// <summary>
            /// Increase all Fury generation by 10%. Increase maximum Fury by 20. Fury is used to fuel your most powerful attacks. 
            /// </summary>
            public static Passive Animosity = new Passive
            {
                Index = 8,
                Name = "Animosity",
                SNOPower = SNOPower.Barbarian_Passive_Animosity,
                RequiredLevel = 27,
                Description = " Increase all Fury generation by 10%. Increase maximum Fury by 20. Fury is used to fuel your most powerful attacks. ",
                Tooltip = "skill/barbarian/animosity",
                Class = ActorClass.Barbarian
            };

            /// <summary>
            /// Reduce all non-Physical damage by 20%. When you take damage from a ranged or elemental attack, you have a chance to gain 2 Fury. 
            /// </summary>
            public static Passive Superstition = new Passive
            {
                Index = 9,
                Name = "Superstition",
                SNOPower = SNOPower.Barbarian_Passive_Superstition,
                RequiredLevel = 30,
                Description = " Reduce all non-Physical damage by 20%. When you take damage from a ranged or elemental attack, you have a chance to gain 2 Fury. ",
                Tooltip = "skill/barbarian/superstition",
                Class = ActorClass.Barbarian
            };

            /// <summary>
            /// Increase Armor by 25%. Increase Thorns damage dealt by 50%. 
            /// </summary>
            public static Passive ToughAsNails = new Passive
            {
                Index = 10,
                Name = "Tough as Nails",
                SNOPower = SNOPower.Barbarian_Passive_ToughAsNails,
                RequiredLevel = 30,
                Description = " Increase Armor by 25%. Increase Thorns damage dealt by 50%. ",
                Tooltip = "skill/barbarian/tough-as-nails",
                Class = ActorClass.Barbarian
            };

            /// <summary>
            /// Increase the damage of Weapon Throw and Ancient Spear by 25% against enemies more than 20 yards away from you. 
            /// </summary>
            public static Passive NoEscape = new Passive
            {
                Index = 11,
                Name = "No Escape",
                SNOPower = SNOPower.Barbarian_Passive_NoEscape,
                RequiredLevel = 35,
                Description = " Increase the damage of Weapon Throw and Ancient Spear by 25% against enemies more than 20 yards away from you. ",
                Tooltip = "skill/barbarian/no-escape",
                Class = ActorClass.Barbarian
            };

            /// <summary>
            /// While below 35% Life, all skills cost 75% less Fury and all damage taken is reduced by 50%. 
            /// </summary>
            public static Passive Relentless = new Passive
            {
                Index = 12,
                Name = "Relentless",
                SNOPower = SNOPower.Barbarian_Passive_Relentless,
                RequiredLevel = 40,
                Description = " While below 35% Life, all skills cost 75% less Fury and all damage taken is reduced by 50%. ",
                Tooltip = "skill/barbarian/relentless",
                Class = ActorClass.Barbarian
            };

            /// <summary>
            /// As long as there are 3 enemies within 12 yards, all of your damage is increased by 20%. 
            /// </summary>
            public static Passive Brawler = new Passive
            {
                Index = 13,
                Name = "Brawler",
                SNOPower = SNOPower.Barbarian_Passive_Brawler,
                RequiredLevel = 45,
                Description = " As long as there are 3 enemies within 12 yards, all of your damage is increased by 20%. ",
                Tooltip = "skill/barbarian/brawler",
                Class = ActorClass.Barbarian
            };

            /// <summary>
            /// The duration of control-impairing effects on you are reduced by 30%. In addition, whenever a Stun, Fear, Immobilize or Charm is cast on you, you have a chance to recover 20% of your maximum Life. 
            /// </summary>
            public static Passive Juggernaut = new Passive
            {
                Index = 14,
                Name = "Juggernaut",
                SNOPower = SNOPower.Barbarian_Passive_Juggernaut,
                RequiredLevel = 50,
                Description = " The duration of control-impairing effects on you are reduced by 30%. In addition, whenever a Stun, Fear, Immobilize or Charm is cast on you, you have a chance to recover 20% of your maximum Life. ",
                Tooltip = "skill/barbarian/juggernaut",
                Class = ActorClass.Barbarian
            };

            /// <summary>
            /// You no longer degenerate Fury. Instead, you generate 2 Fury every 1 seconds. 
            /// </summary>
            public static Passive Unforgiving = new Passive
            {
                Index = 15,
                Name = "Unforgiving",
                SNOPower = SNOPower.Barbarian_Passive_Unforgiving,
                RequiredLevel = 55,
                Description = " You no longer degenerate Fury. Instead, you generate 2 Fury every 1 seconds. ",
                Tooltip = "skill/barbarian/unforgiving",
                Class = ActorClass.Barbarian
            };

            /// <summary>
            /// Reduce the cooldowns of your: Earthquake by 15 seconds. Call of the Ancients by 30 seconds. Wrath of the Berserker by 30 seconds. 
            /// </summary>
            public static Passive BoonOfBulkathos = new Passive
            {
                Index = 16,
                Name = "Boon of Bul-Kathos",
                SNOPower = SNOPower.Barbarian_Passive_BoonOfBulKathos,
                RequiredLevel = 60,
                Description = " Reduce the cooldowns of your: Earthquake by 15 seconds. Call of the Ancients by 30 seconds. Wrath of the Berserker by 30 seconds. ",
                Tooltip = "skill/barbarian/boon-of-bulkathos",
                Class = ActorClass.Barbarian
            };

            /// <summary>
            /// Gain 30 Fury when activating Avalanche or Earthquake. 
            /// </summary>
            public static Passive EarthenMight = new Passive
            {
                Index = 17,
                Name = "Earthen Might",
                SNOPower = SNOPower.X1_Barbarian_Passive_EarthenMight,
                RequiredLevel = 64,
                Description = " Gain 30 Fury when activating Avalanche or Earthquake. ",
                Tooltip = "skill/barbarian/earthen-might",
                Class = ActorClass.Barbarian
            };

            /// <summary>
            /// Blocking an attack generates 6 Fury. 
            /// </summary>
            public static Passive SwordAndBoard = new Passive
            {
                Index = 18,
                Name = "Sword and Board",
                SNOPower = SNOPower.X1_Barbarian_Passive_SwordAndBoard,
                RequiredLevel = 66,
                Description = " Blocking an attack generates 6 Fury. ",
                Tooltip = "skill/barbarian/sword-and-board",
                Class = ActorClass.Barbarian
            };

            /// <summary>
            /// Increase Strength by 1% for 8 seconds after killing or assisting in killing an enemy. This effect stacks up to 25 times. 
            /// </summary>
            public static Passive Rampage = new Passive
            {
                Index = 19,
                Name = "Rampage",
                SNOPower = SNOPower.X1_Barbarian_Passive_Rampage,
                RequiredLevel = 68,
                Description = " Increase Strength by 1% for 8 seconds after killing or assisting in killing an enemy. This effect stacks up to 25 times. ",
                Tooltip = "skill/barbarian/rampage",
                Class = ActorClass.Barbarian
            };
        }
        public class Crusader : FieldCollection<Crusader, Passive>
        {

            /// <summary>
            /// You can wield a two-handed weapon in your main hand while bearing a shield in your off hand. 
            /// </summary>
            public static Passive HeavenlyStrength = new Passive
            {
                Index = 1,
                Name = "Heavenly Strength",
                SNOPower = SNOPower.X1_Crusader_Passive_HeavenlyStrength,
                RequiredLevel = 10,
                Description = " You can wield a two-handed weapon in your main hand while bearing a shield in your off hand. ",
                Tooltip = "skill/crusader/heavenly-strength",
                Class = ActorClass.Crusader
            };

            /// <summary>
            /// While wielding a one-handed weapon, your attack speed is increased by 15% and all cooldowns are reduced by 15%. 
            /// </summary>
            public static Passive Fervor = new Passive
            {
                Index = 2,
                Name = "Fervor",
                SNOPower = SNOPower.X1_Crusader_Passive_Fervor,
                RequiredLevel = 10,
                Description = " While wielding a one-handed weapon, your attack speed is increased by 15% and all cooldowns are reduced by 15%. ",
                Tooltip = "skill/crusader/fervor",
                Class = ActorClass.Crusader
            };

            /// <summary>
            /// Increase Life regeneration by 2063. Reduce all non-Physical damage taken by 20%. 
            /// </summary>
            public static Passive Vigilant = new Passive
            {
                Index = 3,
                Name = "Vigilant",
                SNOPower = SNOPower.X1_Crusader_Passive_Vigilant,
                RequiredLevel = 13,
                Description = " Increase Life regeneration by 2063. Reduce all non-Physical damage taken by 20%. ",
                Tooltip = "skill/crusader/vigilant",
                Class = ActorClass.Crusader
            };

            /// <summary>
            /// Your primary skills generate an additional 3 Wrath. Increase maximum Wrath by 30. 
            /// </summary>
            public static Passive Righteousness = new Passive
            {
                Index = 4,
                Name = "Righteousness",
                SNOPower = SNOPower.X1_Crusader_Passive_Righteousness,
                RequiredLevel = 16,
                Description = " Your primary skills generate an additional 3 Wrath. Increase maximum Wrath by 30. ",
                Tooltip = "skill/crusader/righteousness",
                Class = ActorClass.Crusader
            };

            /// <summary>
            /// Blocking an attack generates 6 Wrath. 
            /// </summary>
            public static Passive Insurmountable = new Passive
            {
                Index = 5,
                Name = "Insurmountable",
                SNOPower = SNOPower.X1_Crusader_Passive_Insurmountable,
                RequiredLevel = 20,
                Description = " Blocking an attack generates 6 Wrath. ",
                Tooltip = "skill/crusader/insurmountable",
                Class = ActorClass.Crusader
            };

            /// <summary>
            /// Increase the attack speed of Punish, Slash, Smite and Justice by 15%. 
            /// </summary>
            public static Passive Fanaticism = new Passive
            {
                Index = 6,
                Name = "Fanaticism",
                SNOPower = SNOPower.X1_Crusader_Passive_NephalemMajesty,
                RequiredLevel = 20,
                Description = " Increase the attack speed of Punish, Slash, Smite and Justice by 15%. ",
                Tooltip = "skill/crusader/fanaticism",
                Class = ActorClass.Crusader
            };

            /// <summary>
            /// When you receive fatal damage, you instead become immune to damage, gain 35% increased damage and gain 82526 Life per Kill for 5 seconds. This effect may occur once every 60 seconds. 
            /// </summary>
            public static Passive Indestructible = new Passive
            {
                Index = 7,
                Name = "Indestructible",
                SNOPower = SNOPower.X1_Crusader_Passive_Indestructible,
                RequiredLevel = 25,
                Description = " When you receive fatal damage, you instead become immune to damage, gain 35% increased damage and gain 82526 Life per Kill for 5 seconds. This effect may occur once every 60 seconds. ",
                Tooltip = "skill/crusader/indestructible",
                Class = ActorClass.Crusader
            };

            /// <summary>
            /// The amount of damage dealt by your weapon is increased by 10%. Whenever you deal Holy damage, you heal 1% of your total Life. 
            /// </summary>
            public static Passive HolyCause = new Passive
            {
                Index = 8,
                Name = "Holy Cause",
                SNOPower = SNOPower.X1_Crusader_Passive_HolyCause,
                RequiredLevel = 27,
                Description = " The amount of damage dealt by your weapon is increased by 10%. Whenever you deal Holy damage, you heal 1% of your total Life. ",
                Tooltip = "skill/crusader/holy-cause",
                Class = ActorClass.Crusader
            };

            /// <summary>
            /// Each point of Wrath spent heals you for 825 Life. Heal amount is increased by 1% of your Health Globe Healing Bonus. 
            /// </summary>
            public static Passive Wrathful = new Passive
            {
                Index = 9,
                Name = "Wrathful",
                SNOPower = SNOPower.X1_Crusader_Passive_Wrathful,
                RequiredLevel = 30,
                Description = " Each point of Wrath spent heals you for 825 Life. Heal amount is increased by 1% of your Health Globe Healing Bonus. ",
                Tooltip = "skill/crusader/wrathful",
                Class = ActorClass.Crusader
            };

            /// <summary>
            /// Your Armor is increased by a percent equal to your shield's Block Chance. 
            /// </summary>
            public static Passive DivineFortress = new Passive
            {
                Index = 10,
                Name = "Divine Fortress",
                SNOPower = SNOPower.X1_Crusader_Passive_DivineFortress,
                RequiredLevel = 30,
                Description = " Your Armor is increased by a percent equal to your shield's Block Chance. ",
                Tooltip = "skill/crusader/divine-fortress",
                Class = ActorClass.Crusader
            };

            /// <summary>
            /// The cooldown of Steed Charge is reduced by 25% and Bombardment by 35%. Damage dealt by Phalanx is increased 20%. 
            /// </summary>
            public static Passive LordCommander = new Passive
            {
                Index = 11,
                Name = "Lord Commander",
                SNOPower = SNOPower.X1_Crusader_Passive_LordCommander,
                RequiredLevel = 35,
                Description = " The cooldown of Steed Charge is reduced by 25% and Bombardment by 35%. Damage dealt by Phalanx is increased 20%. ",
                Tooltip = "skill/crusader/lord-commander",
                Class = ActorClass.Crusader
            };

            /// <summary>
            /// You can no longer Dodge, but your Block Chance is increased by 15%. 
            /// </summary>
            public static Passive HoldYourGround = new Passive
            {
                Index = 12,
                Name = "Hold Your Ground",
                SNOPower = SNOPower.X1_Crusader_Passive_HoldYourGround,
                RequiredLevel = 40,
                Description = " You can no longer Dodge, but your Block Chance is increased by 15%. ",
                Tooltip = "skill/crusader/hold-your-ground",
                Class = ActorClass.Crusader
            };

            /// <summary>
            /// Increase the duration of the Active effect of all Laws by 5 seconds. 
            /// </summary>
            public static Passive LongArmOfTheLaw = new Passive
            {
                Index = 13,
                Name = "Long Arm of the Law",
                SNOPower = SNOPower.X1_Crusader_Passive_LongArmOfTheLaw,
                RequiredLevel = 45,
                Description = " Increase the duration of the Active effect of all Laws by 5 seconds. ",
                Tooltip = "skill/crusader/long-arm-of-the-law",
                Class = ActorClass.Crusader
            };

            /// <summary>
            /// Your Thorns is increased by 50%. 
            /// </summary>
            public static Passive IronMaiden = new Passive
            {
                Index = 14,
                Name = "Iron Maiden",
                SNOPower = SNOPower.X1_Crusader_Passive_IronMaiden,
                RequiredLevel = 50,
                Description = " Your Thorns is increased by 50%. ",
                Tooltip = "skill/crusader/iron-maiden",
                Class = ActorClass.Crusader
            };

            /// <summary>
            /// Whenever you successfully block, you gain 12379 Life. 
            /// </summary>
            public static Passive Renewal = new Passive
            {
                Index = 15,
                Name = "Renewal",
                SNOPower = SNOPower.X1_Crusader_Passive_Renewal,
                RequiredLevel = 55,
                Description = " Whenever you successfully block, you gain 12379 Life. ",
                Tooltip = "skill/crusader/renewal",
                Class = ActorClass.Crusader
            };

            /// <summary>
            /// Gain 1.5% Strength for every gem socketed into your gear. 
            /// </summary>
            public static Passive Finery = new Passive
            {
                Index = 16,
                Name = "Finery",
                SNOPower = SNOPower.X1_Crusader_Passive_Finery,
                RequiredLevel = 60,
                Description = " Gain 1.5% Strength for every gem socketed into your gear. ",
                Tooltip = "skill/crusader/finery",
                Class = ActorClass.Crusader
            };

            /// <summary>
            /// Increase the damage of Justice and Blessed Hammer by 20%. 
            /// </summary>
            public static Passive Blunt = new Passive
            {
                Index = 17,
                Name = "Blunt",
                SNOPower = SNOPower.X1_Crusader_Passive_Blunt,
                RequiredLevel = 65,
                Description = " Increase the damage of Justice and Blessed Hammer by 20%. ",
                Tooltip = "skill/crusader/blunt",
                Class = ActorClass.Crusader
            };

            /// <summary>
            /// Increase the damage of Punish, Shield Bash and Blessed Shield by 20%. Reduce the cooldown of Shield Glare by 30%. 
            /// </summary>
            public static Passive ToweringShield = new Passive
            {
                Index = 18,
                Name = "Towering Shield",
                SNOPower = SNOPower.X1_Crusader_Passive_ToweringShield,
                RequiredLevel = 70,
                Description = " Increase the damage of Punish, Shield Bash and Blessed Shield by 20%. Reduce the cooldown of Shield Glare by 30%. ",
                Tooltip = "skill/crusader/towering-shield",
                Class = ActorClass.Crusader
            };
        }
        public class Wizard : FieldCollection<Wizard, Passive>
        {

            /// <summary>
            /// Being healed by a health globe causes the next Arcane Power Spender you cast to be cast for free. You can have up to 10 charges of Power Hungry. 
            /// </summary>
            public static Passive PowerHungry = new Passive
            {
                Index = 1,
                Name = "Power Hungry",
                SNOPower = SNOPower.Wizard_Passive_PowerHungry,
                RequiredLevel = 10,
                Description = " Being healed by a health globe causes the next Arcane Power Spender you cast to be cast for free. You can have up to 10 charges of Power Hungry. ",
                Tooltip = "skill/wizard/power-hungry",
                Class = ActorClass.Wizard
            };

            /// <summary>
            /// Decrease damage taken by 17%. 
            /// </summary>
            public static Passive Blur = new Passive
            {
                Index = 2,
                Name = "Blur",
                SNOPower = SNOPower.Wizard_Passive_Blur,
                RequiredLevel = 10,
                Description = " Decrease damage taken by 17%. ",
                Tooltip = "skill/wizard/blur",
                Class = ActorClass.Wizard
            };

            /// <summary>
            /// Reduce all cooldowns by 20%. 
            /// </summary>
            public static Passive Evocation = new Passive
            {
                Index = 3,
                Name = "Evocation",
                SNOPower = SNOPower.Wizard_Passive_Evocation,
                RequiredLevel = 13,
                Description = " Reduce all cooldowns by 20%. ",
                Tooltip = "skill/wizard/evocation",
                Class = ActorClass.Wizard
            };

            /// <summary>
            /// Increase all damage done by 15%, but decrease Armor and resistances by 10%. 
            /// </summary>
            public static Passive GlassCannon = new Passive
            {
                Index = 4,
                Name = "Glass Cannon",
                SNOPower = SNOPower.Wizard_Passive_GlassCannon,
                RequiredLevel = 16,
                Description = " Increase all damage done by 15%, but decrease Armor and resistances by 10%. ",
                Tooltip = "skill/wizard/glass-cannon",
                Class = ActorClass.Wizard
            };

            /// <summary>
            /// When you cast a Signature spell, you gain 5 Arcane Power. The following skills are Signature spells: Magic Missile Shock Pulse Spectral Blade Electrocute 
            /// </summary>
            public static Passive Prodigy = new Passive
            {
                Index = 5,
                Name = "Prodigy",
                SNOPower = SNOPower.Wizard_Passive_Prodigy,
                RequiredLevel = 20,
                Description = " When you cast a Signature spell, you gain 5 Arcane Power. The following skills are Signature spells: Magic Missile Shock Pulse Spectral Blade Electrocute ",
                Tooltip = "skill/wizard/prodigy",
                Class = ActorClass.Wizard
            };

            /// <summary>
            /// Increase your maximum Arcane Power by 20 and Arcane Power regeneration by 2 per second. 
            /// </summary>
            public static Passive AstralPresence = new Passive
            {
                Index = 6,
                Name = "Astral Presence",
                SNOPower = SNOPower.Wizard_Passive_AstralPresence,
                RequiredLevel = 24,
                Description = " Increase your maximum Arcane Power by 20 and Arcane Power regeneration by 2 per second. ",
                Tooltip = "skill/wizard/astral-presence",
                Class = ActorClass.Wizard
            };

            /// <summary>
            /// When you take more than 15% of your maximum Life in damage within 1 second, the cooldowns on Mirror Image, Slow Time, and Teleport are reset. When you use Mirror Image, Slow Time, or Teleport, your movement speed is increased by 30% for 3 seconds. 
            /// </summary>
            public static Passive Illusionist = new Passive
            {
                Index = 7,
                Name = "Illusionist",
                SNOPower = SNOPower.Wizard_Passive_Illusionist,
                RequiredLevel = 27,
                Description = " When you take more than 15% of your maximum Life in damage within 1 second, the cooldowns on Mirror Image, Slow Time, and Teleport are reset. When you use Mirror Image, Slow Time, or Teleport, your movement speed is increased by 30% for 3 seconds. ",
                Tooltip = "skill/wizard/illusionist",
                Class = ActorClass.Wizard
            };

            /// <summary>
            /// Enemies chilled or frozen by you take 10% more damage from all sources for the duration of the chill or Freeze. 
            /// </summary>
            public static Passive ColdBlooded = new Passive
            {
                Index = 8,
                Name = "Cold Blooded",
                SNOPower = SNOPower.Wizard_Passive_ColdBlooded,
                RequiredLevel = 30,
                Description = " Enemies chilled or frozen by you take 10% more damage from all sources for the duration of the chill or Freeze. ",
                Tooltip = "skill/wizard/cold-blooded",
                Class = ActorClass.Wizard
            };

            /// <summary>
            /// Fire damage dealt to enemies applies a burning effect, increasing their chance to be critically hit by 6% for 3 seconds. 
            /// </summary>
            public static Passive Conflagration = new Passive
            {
                Index = 9,
                Name = "Conflagration",
                SNOPower = SNOPower.Wizard_Passive_Conflagration,
                RequiredLevel = 34,
                Description = " Fire damage dealt to enemies applies a burning effect, increasing their chance to be critically hit by 6% for 3 seconds. ",
                Tooltip = "skill/wizard/conflagration",
                Class = ActorClass.Wizard
            };

            /// <summary>
            /// Lightning spells have a 15% chance to Stun all targets hit for 1.5 seconds. 
            /// </summary>
            public static Passive Paralysis = new Passive
            {
                Index = 10,
                Name = "Paralysis",
                SNOPower = SNOPower.Wizard_Passive_Paralysis,
                RequiredLevel = 37,
                Description = " Lightning spells have a 15% chance to Stun all targets hit for 1.5 seconds. ",
                Tooltip = "skill/wizard/paralysis",
                Class = ActorClass.Wizard
            };

            /// <summary>
            /// As long as you have not taken damage in the last 5 seconds you gain a protective shield that absorbs the next 62720 damage. 
            /// </summary>
            public static Passive GalvanizingWard = new Passive
            {
                Index = 11,
                Name = "Galvanizing Ward",
                SNOPower = SNOPower.Wizard_Passive_GalvanizingWard,
                RequiredLevel = 40,
                Description = " As long as you have not taken damage in the last 5 seconds you gain a protective shield that absorbs the next 62720 damage. ",
                Tooltip = "skill/wizard/galvanizing-ward",
                Class = ActorClass.Wizard
            };

            /// <summary>
            /// Enemies that take Arcane damage are slowed by 80% for 2 seconds. 
            /// </summary>
            public static Passive TemporalFlux = new Passive
            {
                Index = 12,
                Name = "Temporal Flux",
                SNOPower = SNOPower.Wizard_Passive_TemporalFlux,
                RequiredLevel = 45,
                Description = " Enemies that take Arcane damage are slowed by 80% for 2 seconds. ",
                Tooltip = "skill/wizard/temporal-flux",
                Class = ActorClass.Wizard
            };

            /// <summary>
            /// Killing an enemy grants a shield that absorbs 12379 damage for 3 seconds. This effect can stack up to 10 times. Refreshing Dominance will set the shield to its maximum possible potency and each stack will increase its total duration by 0.5 seconds. 
            /// </summary>
            public static Passive Dominance = new Passive
            {
                Index = 13,
                Name = "Dominance",
                SNOPower = SNOPower.x1_Wizard_Passive_ArcaneAegis,
                RequiredLevel = 50,
                Description = " Killing an enemy grants a shield that absorbs 12379 damage for 3 seconds. This effect can stack up to 10 times. Refreshing Dominance will set the shield to its maximum possible potency and each stack will increase its total duration by 0.5 seconds. ",
                Tooltip = "skill/wizard/dominance",
                Class = ActorClass.Wizard
            };

            /// <summary>
            /// When you cast a Signature spell, you gain a Flash of Insight. After 5 Flashes of Insight, your next non-Signature spell deals 60% additional damage. The following skills are Signature spells: Magic Missile Shock Pulse Spectral Blade Electrocute 
            /// </summary>
            public static Passive ArcaneDynamo = new Passive
            {
                Index = 14,
                Name = "Arcane Dynamo",
                SNOPower = SNOPower.Wizard_Passive_ArcaneDynamo,
                RequiredLevel = 55,
                Description = " When you cast a Signature spell, you gain a Flash of Insight. After 5 Flashes of Insight, your next non-Signature spell deals 60% additional damage. The following skills are Signature spells: Magic Missile Shock Pulse Spectral Blade Electrocute ",
                Tooltip = "skill/wizard/arcane-dynamo",
                Class = ActorClass.Wizard
            };

            /// <summary>
            /// When you receive fatal damage, you heal to 45% of your maximum Life and release a shockwave that knocks enemies back and slows them by 60% for 3 seconds. This effect may occur once every 60 seconds. 
            /// </summary>
            public static Passive UnstableAnomaly = new Passive
            {
                Index = 15,
                Name = "Unstable Anomaly",
                SNOPower = SNOPower.Wizard_Passive_UnstableAnomaly,
                RequiredLevel = 60,
                Description = " When you receive fatal damage, you heal to 45% of your maximum Life and release a shockwave that knocks enemies back and slows them by 60% for 3 seconds. This effect may occur once every 60 seconds. ",
                Tooltip = "skill/wizard/unstable-anomaly",
                Class = ActorClass.Wizard
            };

            /// <summary>
            /// Standing still for 1.5 seconds increases the following attributes: Armor: 20% All Resistances: 20% Damage: 10% 
            /// </summary>
            public static Passive UnwaveringWill = new Passive
            {
                Index = 16,
                Name = "Unwavering Will",
                SNOPower = SNOPower.X1_Wizard_Passive_UnwaveringWill,
                RequiredLevel = 64,
                Description = " Standing still for 1.5 seconds increases the following attributes: Armor: 20% All Resistances: 20% Damage: 10% ",
                Tooltip = "skill/wizard/unwavering-will",
                Class = ActorClass.Wizard
            };

            /// <summary>
            /// You deal 15% additional damage to enemies within 15 yards. 
            /// </summary>
            public static Passive Audacity = new Passive
            {
                Index = 17,
                Name = "Audacity",
                SNOPower = SNOPower.X1_Wizard_Passive_Audacity,
                RequiredLevel = 66,
                Description = " You deal 15% additional damage to enemies within 15 yards. ",
                Tooltip = "skill/wizard/audacity",
                Class = ActorClass.Wizard
            };

            /// <summary>
            /// Damaging enemies with Arcane, Cold, Fire or Lightning will cause them to take 5% more damage from all sources for 5 seconds. Each different damage type applies a stack, stacking up to 4 times. Elemental damage from your weapon contributes to Elemental Exposure. 
            /// </summary>
            public static Passive ElementalExposure = new Passive
            {
                Index = 18,
                Name = "Elemental Exposure",
                SNOPower = SNOPower.X1_Wizard_Passive_ElementalExposure,
                RequiredLevel = 68,
                Description = " Damaging enemies with Arcane, Cold, Fire or Lightning will cause them to take 5% more damage from all sources for 5 seconds. Each different damage type applies a stack, stacking up to 4 times. Elemental damage from your weapon contributes to Elemental Exposure. ",
                Tooltip = "skill/wizard/elemental-exposure",
                Class = ActorClass.Wizard
            };
        }

        /// <summary>
        /// All passives that are currently active
        /// </summary>
        public static List<Passive> Active
        {
            get
            {
                if (ZetaDia.CPlayer.IsValid && ZetaDia.IsInGame && (!_active.Any() || DateTime.UtcNow.Subtract(_lastUpdatedActivePassives) > TimeSpan.FromSeconds(3)))
                {
                    _lastUpdatedActivePassives = DateTime.UtcNow;
                    _active.Clear();
                    _active = CurrentClass.Where(p => p.IsActive).ToList();
                }
                return _active;
            }
        }
        private static List<Passive> _active = new List<Passive>();
        private static DateTime _lastUpdatedActivePassives = DateTime.MinValue;

        /// <summary>
        /// All passives
        /// </summary>        
        public static List<Passive> All
        {
            get
            {
                if (!_all.Any())
                {
                    _all.AddRange(Barbarian.ToList());
                    _all.AddRange(WitchDoctor.ToList());
                    _all.AddRange(DemonHunter.ToList());
                    _all.AddRange(Wizard.ToList());
                    _all.AddRange(Crusader.ToList());
                    _all.AddRange(Monk.ToList());
                }
                return _all;
            }
        }
        private static List<Passive> _all = new List<Passive>();


        /// <summary>
        /// All passives for the specified class
        /// </summary>
        public static List<Passive> ByActorClass(ActorClass Class)
        {
            if (ZetaDia.Me.IsValid)
            {
                switch (ZetaDia.Me.ActorClass)
                {
                    case ActorClass.Barbarian:
                        return Barbarian.ToList();
                    case ActorClass.Crusader:
                        return Crusader.ToList();
                    case ActorClass.DemonHunter:
                        return DemonHunter.ToList();
                    case ActorClass.Monk:
                        return Monk.ToList();
                    case ActorClass.Witchdoctor:
                        return WitchDoctor.ToList();
                    case ActorClass.Wizard:
                        return Wizard.ToList();
                }
            }
            return new List<Passive>();
        }

        /// <summary>
        /// Passives for the current class
        /// </summary>
        public static IEnumerable<Passive> CurrentClass
        {
            get { return ZetaDia.Me.IsValid ? ByActorClass(ZetaDia.Me.ActorClass) : new List<Passive>(); }
        }

    
    }
}