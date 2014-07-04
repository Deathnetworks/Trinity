﻿using System;
using System.Linq;
using Trinity.Combat;
using Trinity.Helpers;
using Trinity.Objects;
using System.Collections.Generic;
using Zeta.Game;
using Zeta.Game.Internals.Actors;

// AUTO-GENERATED on Wed, 02 Jul 2014 07:26:06 GMT

namespace Trinity.Reference
{
    public static class Skills
    {
        public class Crusader : FieldCollection<Crusader, Skill>
        {
            /// <summary>
            /// Generate: 5 Wrath per attack Strike your enemy for 335% weapon damage and gain Hardened Senses, increasing your Block Chance by 15% for 5 seconds. 
            /// </summary>
            public static Skill Punish = new Skill
            {
                Index = 0,
                Name = "Punish",
                SNOPower = SNOPower.X1_Crusader_Punish,
                RequiredLevel = 1,
                Description = " Generate: 5 Wrath per attack Strike your enemy for 335% weapon damage and gain Hardened Senses, increasing your Block Chance by 15% for 5 seconds. ",
                Tooltip = "skill/crusader/punish",
                Category = SpellCategory.Primary,
                IsPrimary = true,
                Class = ActorClass.Crusader,
                Duration = TimeSpan.FromSeconds(5),
                Cost = 0,
                Cooldown = TimeSpan.Zero,
                Element = Element.Physical,
                Resource = Resource.Wrath,
                Runes = new List<Rune>
                {
                    Runes.Crusader.None,
                    Runes.Crusader.Roar,
                    Runes.Crusader.Celerity,
                    Runes.Crusader.Rebirth,
                    Runes.Crusader.Retaliate,
                    Runes.Crusader.Fury,
                }
            };

            /// <summary>
            /// Cost: 30 Wrath Charge at your enemy, bashing him and all nearby foes. Deals 700% weapon damage plus 300% of your shield's Block Chance as Holy damage. 
            /// </summary>
            public static Skill ShieldBash = new Skill
            {
                Index = 1,
                Name = "Shield Bash",
                SNOPower = SNOPower.X1_Crusader_ShieldBash2,
                RequiredLevel = 2,
                Description = " Cost: 30 Wrath Charge at your enemy, bashing him and all nearby foes. Deals 700% weapon damage plus 300% of your shield's Block Chance as Holy damage. ",
                Tooltip = "skill/crusader/shield-bash",
                Category = SpellCategory.Secondary,
                IsPrimary = true,
                Class = ActorClass.Crusader,
                Duration = TimeSpan.Zero,
                Cost = 30,
                Cooldown = TimeSpan.Zero,
                Element = Element.Holy,
                Resource = Resource.Wrath,
                Runes = new List<Rune>
                {
                    Runes.Crusader.None,
                    Runes.Crusader.ShatteredShield,
                    Runes.Crusader.OneOnOne,
                    Runes.Crusader.ShieldCross,
                    Runes.Crusader.Crumble,
                    Runes.Crusader.Pound,
                }
            };

            /// <summary>
            /// Generate: 5 Wrath per attack Ignite the air in front of you, dealing 230% weapon damage as Fire. 
            /// </summary>
            public static Skill Slash = new Skill
            {
                Index = 2,
                Name = "Slash",
                SNOPower = SNOPower.X1_Crusader_Slash,
                RequiredLevel = 3,
                Description = " Generate: 5 Wrath per attack Ignite the air in front of you, dealing 230% weapon damage as Fire. ",
                Tooltip = "skill/crusader/slash",
                Category = SpellCategory.Primary,
                IsPrimary = true,
                Class = ActorClass.Crusader,
                Duration = TimeSpan.Zero,
                Cost = 0,
                Cooldown = TimeSpan.Zero,
                Element = Element.Fire,
                Resource = Resource.Wrath,
                Runes = new List<Rune>
                {
                    Runes.Crusader.None,
                    Runes.Crusader.Electrify,
                    Runes.Crusader.Carve,
                    Runes.Crusader.Crush,
                    Runes.Crusader.Zeal,
                    Runes.Crusader.Guard,
                }
            };

            /// <summary>
            /// Cooldown: 12 seconds Light erupts from your shield, Blinding all enemies up to 30 yards in front of you for 4 seconds. 
            /// </summary>
            public static Skill ShieldGlare = new Skill
            {
                Index = 3,
                Name = "Shield Glare",
                SNOPower = SNOPower.X1_Crusader_ShieldGlare,
                RequiredLevel = 4,
                Description = " Cooldown: 12 seconds Light erupts from your shield, Blinding all enemies up to 30 yards in front of you for 4 seconds. ",
                Tooltip = "skill/crusader/shield-glare",
                Category = SpellCategory.Defensive,
                IsPrimary = true,
                Class = ActorClass.Crusader,
                Duration = TimeSpan.FromSeconds(4),
                Cost = 0,
                Cooldown = TimeSpan.FromSeconds(12),
                Element = Element.Holy,
                Resource = Resource.None,
                Runes = new List<Rune>
                {
                    Runes.Crusader.None,
                    Runes.Crusader.DivineVerdict,
                    Runes.Crusader.Uncertainty,
                    Runes.Crusader.ZealousGlare,
                    Runes.Crusader.EmblazonedShield,
                    Runes.Crusader.Subdue,
                }
            };

            /// <summary>
            /// Cost: 20 Wrath Sweep a mystical flail through enemies up to 18 yards before you, dealing 480% weapon damage. 
            /// </summary>
            public static Skill SweepAttack = new Skill
            {
                Index = 4,
                Name = "Sweep Attack",
                SNOPower = SNOPower.X1_Crusader_SweepAttack,
                RequiredLevel = 5,
                Description = " Cost: 20 Wrath Sweep a mystical flail through enemies up to 18 yards before you, dealing 480% weapon damage. ",
                Tooltip = "skill/crusader/sweep-attack",
                Category = SpellCategory.Secondary,
                IsPrimary = true,
                Class = ActorClass.Crusader,
                Duration = TimeSpan.Zero,
                Cost = 20,
                Cooldown = TimeSpan.Zero,
                Element = Element.Physical,
                Resource = Resource.Wrath,
                Runes = new List<Rune>
                {
                    Runes.Crusader.None,
                    Runes.Crusader.BlazingSweep,
                    Runes.Crusader.TripAttack,
                    Runes.Crusader.HolyShock,
                    Runes.Crusader.GatheringSweep,
                    Runes.Crusader.FrozenSweep,
                }
            };

            /// <summary>
            /// Cooldown: 30 seconds Your skin turns to iron, absorbing 50% of all incoming damage for 4 seconds. 
            /// </summary>
            public static Skill IronSkin = new Skill
            {
                Index = 5,
                Name = "Iron Skin",
                SNOPower = SNOPower.X1_Crusader_IronSkin,
                RequiredLevel = 8,
                Description = " Cooldown: 30 seconds Your skin turns to iron, absorbing 50% of all incoming damage for 4 seconds. ",
                Tooltip = "skill/crusader/iron-skin",
                Category = SpellCategory.Defensive,
                IsPrimary = false,
                Class = ActorClass.Crusader,
                Duration = TimeSpan.FromSeconds(4),
                Cost = 0,
                Cooldown = TimeSpan.FromSeconds(30),
                Element = Element.Physical,
                Resource = Resource.None,
                Runes = new List<Rune>
                {
                    Runes.Crusader.None,
                    Runes.Crusader.ReflectiveSkin,
                    Runes.Crusader.SteelSkin,
                    Runes.Crusader.ExplosiveSkin,
                    Runes.Crusader.ChargedUpIronSkin,
                    Runes.Crusader.Flash,
                }
            };

            /// <summary>
            /// Cooldown: 20 seconds Generate: 30 Wrath Taunt all nearby enemies and instantly generate an additional 5 Wrath for every enemy taunted. Taunted enemies will focus their attention on you for 4 seconds. 
            /// </summary>
            public static Skill Provoke = new Skill
            {
                Index = 6,
                Name = "Provoke",
                SNOPower = SNOPower.X1_Crusader_Provoke,
                RequiredLevel = 9,
                Description = " Cooldown: 20 seconds Generate: 30 Wrath Taunt all nearby enemies and instantly generate an additional 5 Wrath for every enemy taunted. Taunted enemies will focus their attention on you for 4 seconds. ",
                Tooltip = "skill/crusader/provoke",
                Category = SpellCategory.Utility,
                IsPrimary = true,
                Class = ActorClass.Crusader,
                Duration = TimeSpan.FromSeconds(4),
                Cost = 0,
                Cooldown = TimeSpan.FromSeconds(20),
                Element = Element.Physical,
                Resource = Resource.Wrath,
                Runes = new List<Rune>
                {
                    Runes.Crusader.None,
                    Runes.Crusader.Cleanse,
                    Runes.Crusader.FleeFool,
                    Runes.Crusader.TooScaredToRun,
                    Runes.Crusader.ChargedUpProvoke,
                    Runes.Crusader.HitMe,
                }
            };

            /// <summary>
            /// Generate: 5 Wrath per attack Smite enemies up to 30 yards away with holy chains that deal 175% weapon damage as Holy. The chains break off and strike up to 3 additional enemies within 20 yards for 150% weapon damage as Holy. 
            /// </summary>
            public static Skill Smite = new Skill
            {
                Index = 7,
                Name = "Smite",
                SNOPower = SNOPower.X1_Crusader_Smite,
                RequiredLevel = 11,
                Description = " Generate: 5 Wrath per attack Smite enemies up to 30 yards away with holy chains that deal 175% weapon damage as Holy. The chains break off and strike up to 3 additional enemies within 20 yards for 150% weapon damage as Holy. ",
                Tooltip = "skill/crusader/smite",
                Category = SpellCategory.Primary,
                IsPrimary = true,
                Class = ActorClass.Crusader,
                Duration = TimeSpan.Zero,
                Cost = 0,
                Cooldown = TimeSpan.Zero,
                Element = Element.Holy,
                Resource = Resource.Wrath,
                Runes = new List<Rune>
                {
                    Runes.Crusader.None,
                    Runes.Crusader.Shatter,
                    Runes.Crusader.Shackle,
                    Runes.Crusader.Surge,
                    Runes.Crusader.Reaping,
                    Runes.Crusader.SharedFate,
                }
            };

            /// <summary>
            /// Cost: 10 Wrath Summon a blessed hammer that spins around you, dealing 320% weapon damage as Holy to all enemies hit. 
            /// </summary>
            public static Skill BlessedHammer = new Skill
            {
                Index = 8,
                Name = "Blessed Hammer",
                SNOPower = SNOPower.X1_Crusader_BlessedHammer,
                RequiredLevel = 12,
                Description = " Cost: 10 Wrath Summon a blessed hammer that spins around you, dealing 320% weapon damage as Holy to all enemies hit. ",
                Tooltip = "skill/crusader/blessed-hammer",
                Category = SpellCategory.Secondary,
                IsPrimary = true,
                Class = ActorClass.Crusader,
                Duration = TimeSpan.Zero,
                Cost = 10,
                Cooldown = TimeSpan.Zero,
                Element = Element.Holy,
                Resource = Resource.Wrath,
                Runes = new List<Rune>
                {
                    Runes.Crusader.None,
                    Runes.Crusader.BurningWrath,
                    Runes.Crusader.Thunderstruck,
                    Runes.Crusader.Limitless,
                    Runes.Crusader.IceboundHammer,
                    Runes.Crusader.Dominion,
                }
            };

            /// <summary>
            /// Cooldown: 16 seconds Mount a celestial war horse that allows you to ride through enemies unhindered for 2 seconds. 
            /// </summary>
            public static Skill SteedCharge = new Skill
            {
                Index = 9,
                Name = "Steed Charge",
                SNOPower = SNOPower.X1_Crusader_SteedCharge,
                RequiredLevel = 13,
                Description = " Cooldown: 16 seconds Mount a celestial war horse that allows you to ride through enemies unhindered for 2 seconds. ",
                Tooltip = "skill/crusader/steed-charge",
                Category = SpellCategory.Utility,
                IsPrimary = true,
                Class = ActorClass.Crusader,
                Duration = TimeSpan.FromSeconds(2),
                Cost = 0,
                Cooldown = TimeSpan.FromSeconds(16),
                Element = Element.Physical,
                Resource = Resource.None,
                Runes = new List<Rune>
                {
                    Runes.Crusader.None,
                    Runes.Crusader.RammingSpeed,
                    Runes.Crusader.Nightmare,
                    Runes.Crusader.Rejuvenation,
                    Runes.Crusader.Endurance,
                    Runes.Crusader.DrawAndQuarter,
                }
            };

            /// <summary>
            /// Cooldown: 30 seconds Active: Empower the Law, granting you and your allies 15% increased Attack Speed for 5 seconds. Passive: Recite the Law, granting you and your allies 8% increased Attack Speed. Only one Law may be active at a time. 
            /// </summary>
            public static Skill LawsOfValor = new Skill
            {
                Index = 10,
                Name = "Laws of Valor",
                SNOPower = SNOPower.X1_Crusader_LawsOfValor2,
                RequiredLevel = 14,
                Description = " Cooldown: 30 seconds Active: Empower the Law, granting you and your allies 15% increased Attack Speed for 5 seconds. Passive: Recite the Law, granting you and your allies 8% increased Attack Speed. Only one Law may be active at a time. ",
                Tooltip = "skill/crusader/laws-of-valor",
                Category = SpellCategory.Laws,
                IsPrimary = false,
                Class = ActorClass.Crusader,
                Duration = TimeSpan.FromSeconds(5),
                Cost = 0,
                Cooldown = TimeSpan.FromSeconds(30),
                Element = Element.Physical,
                Resource = Resource.None,
                Runes = new List<Rune>
                {
                    Runes.Crusader.None,
                    Runes.Crusader.Invincible,
                    Runes.Crusader.FrozenInTerror,
                    Runes.Crusader.Critical,
                    Runes.Crusader.UnstoppableForce,
                    Runes.Crusader.AnsweredPrayer,
                }
            };

            /// <summary>
            /// Generate: 5 Wrath per attack Hurl a hammer of justice at your enemies, dealing 245% weapon damage. 
            /// </summary>
            public static Skill Justice = new Skill
            {
                Index = 11,
                Name = "Justice",
                SNOPower = SNOPower.X1_Crusader_Justice,
                RequiredLevel = 15,
                Description = " Generate: 5 Wrath per attack Hurl a hammer of justice at your enemies, dealing 245% weapon damage. ",
                Tooltip = "skill/crusader/justice",
                Category = SpellCategory.Primary,
                IsPrimary = true,
                Class = ActorClass.Crusader,
                Duration = TimeSpan.Zero,
                Cost = 0,
                Cooldown = TimeSpan.Zero,
                Element = Element.Holy,
                Resource = Resource.Wrath,
                Runes = new List<Rune>
                {
                    Runes.Crusader.None,
                    Runes.Crusader.Burst,
                    Runes.Crusader.Crack,
                    Runes.Crusader.HammerOfPursuit,
                    Runes.Crusader.SwordOfJustice,
                    Runes.Crusader.HolyBolt,
                }
            };

            /// <summary>
            /// Cooldown: 30 seconds Consecrate the ground 20 yards around you for 10 seconds. You and your allies heal for 8253 Life per second while standing on the consecrated ground. 
            /// </summary>
            public static Skill Consecration = new Skill
            {
                Index = 12,
                Name = "Consecration",
                SNOPower = SNOPower.X1_Crusader_Consecration,
                RequiredLevel = 16,
                Description = " Cooldown: 30 seconds Consecrate the ground 20 yards around you for 10 seconds. You and your allies heal for 8253 Life per second while standing on the consecrated ground. ",
                Tooltip = "skill/crusader/consecration",
                Category = SpellCategory.Defensive,
                IsPrimary = false,
                Class = ActorClass.Crusader,
                Duration = TimeSpan.FromSeconds(10),
                Cost = 0,
                Cooldown = TimeSpan.FromSeconds(30),
                Element = Element.Holy,
                Resource = Resource.None,
                Runes = new List<Rune>
                {
                    Runes.Crusader.None,
                    Runes.Crusader.BathedInLight,
                    Runes.Crusader.FrozenGround,
                    Runes.Crusader.AegisPurgatory,
                    Runes.Crusader.ShatteredGround,
                    Runes.Crusader.Fearful,
                }
            };

            /// <summary>
            /// Cooldown: 30 seconds Active: Empower the Law, granting you and your allies 490 increased resistance to all elements for 5 seconds. Passive: Recite the Law, granting you and your allies 140 increased resistance to all elements. Only one Law may be active at a time. 
            /// </summary>
            public static Skill LawsOfJustice = new Skill
            {
                Index = 13,
                Name = "Laws of Justice",
                SNOPower = SNOPower.X1_Crusader_LawsOfJustice2,
                RequiredLevel = 17,
                Description = " Cooldown: 30 seconds Active: Empower the Law, granting you and your allies 490 increased resistance to all elements for 5 seconds. Passive: Recite the Law, granting you and your allies 140 increased resistance to all elements. Only one Law may be active at a time. ",
                Tooltip = "skill/crusader/laws-of-justice",
                Category = SpellCategory.Laws,
                IsPrimary = false,
                Class = ActorClass.Crusader,
                Duration = TimeSpan.FromSeconds(5),
                Cost = 0,
                Cooldown = TimeSpan.FromSeconds(30),
                Element = Element.Physical,
                Resource = Resource.None,
                Runes = new List<Rune>
                {
                    Runes.Crusader.None,
                    Runes.Crusader.ProtectTheInnocent,
                    Runes.Crusader.ImmovableObject,
                    Runes.Crusader.FaithsArmor,
                    Runes.Crusader.DecayingStrength,
                    Runes.Crusader.Bravery,
                }
            };

            /// <summary>
            /// Cost: 25 Wrath Cooldown: 30 seconds Launch yourself into the heavens and come crashing down on your enemies, dealing 1700% weapon damage to everything within 14 yards of where you land. 
            /// </summary>
            public static Skill FallingSword = new Skill
            {
                Index = 14,
                Name = "Falling Sword",
                SNOPower = SNOPower.X1_Crusader_FallingSword,
                RequiredLevel = 19,
                Description = " Cost: 25 Wrath Cooldown: 30 seconds Launch yourself into the heavens and come crashing down on your enemies, dealing 1700% weapon damage to everything within 14 yards of where you land. ",
                Tooltip = "skill/crusader/falling-sword",
                Category = SpellCategory.Conviction,
                IsPrimary = true,
                Class = ActorClass.Crusader,
                Duration = TimeSpan.Zero,
                Cost = 25,
                Cooldown = TimeSpan.FromSeconds(30),
                Element = Element.Physical,
                Resource = Resource.Wrath,
                Runes = new List<Rune>
                {
                    Runes.Crusader.None,
                    Runes.Crusader.Superheated,
                    Runes.Crusader.PartTheClouds,
                    Runes.Crusader.RiseBrothers,
                    Runes.Crusader.RapidDescent,
                    Runes.Crusader.Flurry,
                }
            };

            /// <summary>
            /// Cost: 20 Wrath Hurl your shield, dealing 430% weapon damage as Holy plus 250% of shield Block Chance as Holy damage. The shield will ricochet to 3 nearby enemies. 
            /// </summary>
            public static Skill BlessedShield = new Skill
            {
                Index = 15,
                Name = "Blessed Shield",
                SNOPower = SNOPower.X1_Crusader_BlessedShield,
                RequiredLevel = 20,
                Description = " Cost: 20 Wrath Hurl your shield, dealing 430% weapon damage as Holy plus 250% of shield Block Chance as Holy damage. The shield will ricochet to 3 nearby enemies. ",
                Tooltip = "skill/crusader/blessed-shield",
                Category = SpellCategory.Secondary,
                IsPrimary = true,
                Class = ActorClass.Crusader,
                Duration = TimeSpan.Zero,
                Cost = 20,
                Cooldown = TimeSpan.Zero,
                Element = Element.Holy,
                Resource = Resource.Wrath,
                Runes = new List<Rune>
                {
                    Runes.Crusader.None,
                    Runes.Crusader.StaggeringShield,
                    Runes.Crusader.Combust,
                    Runes.Crusader.DivineAegis,
                    Runes.Crusader.ShatteringThrow,
                    Runes.Crusader.PiercingShield,
                }
            };

            /// <summary>
            /// Cooldown: 15 seconds Build up a massive explosion, unleashing it after 3 seconds, dealing 1160% weapon damage as Holy to all enemies within 15 yards. 
            /// </summary>
            public static Skill Condemn = new Skill
            {
                Index = 16,
                Name = "Condemn",
                SNOPower = SNOPower.X1_Crusader_Condemn,
                RequiredLevel = 21,
                Description = " Cooldown: 15 seconds Build up a massive explosion, unleashing it after 3 seconds, dealing 1160% weapon damage as Holy to all enemies within 15 yards. ",
                Tooltip = "skill/crusader/condemn",
                Category = SpellCategory.Utility,
                IsPrimary = false,
                Class = ActorClass.Crusader,
                Duration = TimeSpan.Zero,
                Cost = 0,
                Cooldown = TimeSpan.FromSeconds(15),
                Element = Element.Holy,
                Resource = Resource.None,
                Runes = new List<Rune>
                {
                    Runes.Crusader.None,
                    Runes.Crusader.Vacuum,
                    Runes.Crusader.Unleashed,
                    Runes.Crusader.EternalRetaliation,
                    Runes.Crusader.ShatteringExplosion,
                    Runes.Crusader.Reciprocate,
                }
            };

            /// <summary>
            /// Cooldown: 20 seconds Pass judgment on all enemies within 20 yards of the targeted location, Immobilizing them in place for 6 seconds. 
            /// </summary>
            public static Skill Judgment = new Skill
            {
                Index = 17,
                Name = "Judgment",
                SNOPower = SNOPower.X1_Crusader_Judgment,
                RequiredLevel = 22,
                Description = " Cooldown: 20 seconds Pass judgment on all enemies within 20 yards of the targeted location, Immobilizing them in place for 6 seconds. ",
                Tooltip = "skill/crusader/judgment",
                Category = SpellCategory.Defensive,
                IsPrimary = true,
                Class = ActorClass.Crusader,
                Duration = TimeSpan.FromSeconds(6),
                Cost = 0,
                Cooldown = TimeSpan.FromSeconds(20),
                Element = Element.Physical,
                Resource = Resource.None,
                Runes = new List<Rune>
                {
                    Runes.Crusader.None,
                    Runes.Crusader.Penitence,
                    Runes.Crusader.MassVerdict,
                    Runes.Crusader.Deliberation,
                    Runes.Crusader.Resolved,
                    Runes.Crusader.Conversion,
                }
            };

            /// <summary>
            /// Cooldown: 30 seconds Active: Empower the Law, surrounding you and your allies in a shield for 3 seconds that absorbs up to 95483 damage. Passive: Recite the Law, healing you and your allies for 3714 Life per second. Only one Law may be active at a time. 
            /// </summary>
            public static Skill LawsOfHope = new Skill
            {
                Index = 18,
                Name = "Laws of Hope",
                SNOPower = SNOPower.X1_Crusader_LawsOfHope2,
                RequiredLevel = 24,
                Description = " Cooldown: 30 seconds Active: Empower the Law, surrounding you and your allies in a shield for 3 seconds that absorbs up to 95483 damage. Passive: Recite the Law, healing you and your allies for 3714 Life per second. Only one Law may be active at a time. ",
                Tooltip = "skill/crusader/laws-of-hope",
                Category = SpellCategory.Laws,
                IsPrimary = false,
                Class = ActorClass.Crusader,
                Duration = TimeSpan.FromSeconds(3),
                Cost = 0,
                Cooldown = TimeSpan.FromSeconds(30),
                Element = Element.Physical,
                Resource = Resource.None,
                Runes = new List<Rune>
                {
                    Runes.Crusader.None,
                    Runes.Crusader.WingsOfAngels,
                    Runes.Crusader.EternalHope,
                    Runes.Crusader.HopefulCry,
                    Runes.Crusader.FaithsReward,
                    Runes.Crusader.StopTime,
                }
            };

            /// <summary>
            /// Cooldown: 90 seconds Explode with the power of your order, increasing your damage by 35% and increasing your Wrath regeneration by 5 for 20 seconds. 
            /// </summary>
            public static Skill AkaratsChampion = new Skill
            {
                Index = 19,
                Name = "Akarat's Champion",
                SNOPower = SNOPower.X1_Crusader_AkaratsChampion,
                RequiredLevel = 25,
                Description = " Cooldown: 90 seconds Explode with the power of your order, increasing your damage by 35% and increasing your Wrath regeneration by 5 for 20 seconds. ",
                Tooltip = "skill/crusader/akarats-champion",
                Category = SpellCategory.Conviction,
                IsPrimary = false,
                Class = ActorClass.Crusader,
                Duration = TimeSpan.FromSeconds(20),
                Cost = 0,
                Cooldown = TimeSpan.FromSeconds(90),
                Element = Element.Fire,
                Resource = Resource.Wrath,
                Runes = new List<Rune>
                {
                    Runes.Crusader.None,
                    Runes.Crusader.FireStarter,
                    Runes.Crusader.EmbodimentOfPower,
                    Runes.Crusader.Rally,
                    Runes.Crusader.Prophet,
                    Runes.Crusader.Hasteful,
                }
            };

            /// <summary>
            /// Cost: 30 Wrath Call forth a pillar of lightning from the heavens that explodes, dealing 545% weapon damage as Lightning to any enemy within 8 yards. The explosion creates 6 piercing charged bolts that arc outward and deal 255% weapon damage as Lightning. 
            /// </summary>
            public static Skill FistOfTheHeavens = new Skill
            {
                Index = 20,
                Name = "Fist of the Heavens",
                SNOPower = SNOPower.X1_Crusader_FistOfTheHeavens,
                RequiredLevel = 26,
                Description = " Cost: 30 Wrath Call forth a pillar of lightning from the heavens that explodes, dealing 545% weapon damage as Lightning to any enemy within 8 yards. The explosion creates 6 piercing charged bolts that arc outward and deal 255% weapon damage as Lightning. ",
                Tooltip = "skill/crusader/fist-of-the-heavens",
                Category = SpellCategory.Secondary,
                IsPrimary = true,
                Class = ActorClass.Crusader,
                Duration = TimeSpan.Zero,
                Cost = 30,
                Cooldown = TimeSpan.Zero,
                Element = Element.Lightning,
                Resource = Resource.Wrath,
                Runes = new List<Rune>
                {
                    Runes.Crusader.None,
                    Runes.Crusader.DivineWell,
                    Runes.Crusader.HeavensTempest,
                    Runes.Crusader.Fissure,
                    Runes.Crusader.Reverberation,
                    Runes.Crusader.Retribution,
                }
            };

            /// <summary>
            /// Cost: 30 Wrath Summon powerful avatars who charge forward to the targeted destination. Enemies caught in the charge path take 490% weapon damage. 
            /// </summary>
            public static Skill Phalanx = new Skill
            {
                Index = 21,
                Name = "Phalanx",
                SNOPower = SNOPower.x1_Crusader_Phalanx3,
                RequiredLevel = 27,
                Description = " Cost: 30 Wrath Summon powerful avatars who charge forward to the targeted destination. Enemies caught in the charge path take 490% weapon damage. ",
                Tooltip = "skill/crusader/phalanx",
                Category = SpellCategory.Utility,
                IsPrimary = true,
                Class = ActorClass.Crusader,
                Duration = TimeSpan.Zero,
                Cost = 30,
                Cooldown = TimeSpan.Zero,
                Element = Element.Physical,
                Resource = Resource.Wrath,
                Runes = new List<Rune>
                {
                    Runes.Crusader.None,
                    Runes.Crusader.Bowmen,
                    Runes.Crusader.ShieldCharge,
                    Runes.Crusader.Stampede,
                    Runes.Crusader.ShieldBearers,
                    Runes.Crusader.Bodyguard,
                }
            };

            /// <summary>
            /// Cooldown: 20 seconds Call down a furious ray of Holy power that deals 1710% weapon damage as Holy over 6 seconds to all enemies caught within it. 
            /// </summary>
            public static Skill HeavensFury = new Skill
            {
                Index = 22,
                Name = "Heaven's Fury",
                SNOPower = SNOPower.X1_Crusader_HeavensFury3,
                RequiredLevel = 30,
                Description = " Cooldown: 20 seconds Call down a furious ray of Holy power that deals 1710% weapon damage as Holy over 6 seconds to all enemies caught within it. ",
                Tooltip = "skill/crusader/heavens-fury",
                Category = SpellCategory.Conviction,
                IsPrimary = true,
                Class = ActorClass.Crusader,
                Duration = TimeSpan.FromSeconds(6),
                Cost = 0,
                Cooldown = TimeSpan.FromSeconds(20),
                Element = Element.Holy,
                Resource = Resource.None,
                Runes = new List<Rune>
                {
                    Runes.Crusader.None,
                    Runes.Crusader.BlessedGround,
                    Runes.Crusader.Ascendancy,
                    Runes.Crusader.SplitFury,
                    Runes.Crusader.ThouShaltNotPass,
                    Runes.Crusader.FiresOfHeaven,
                }
            };

            /// <summary>
            /// Cooldown: 60 seconds Call in an assault from afar, raining spheres of burning pitch and stone onto enemies around you, dealing 570% weapon damage to enemies within 12 yards of the impact zone. The bombardment continues on randomly targeted enemies nearby for the next 5 seconds. 
            /// </summary>
            public static Skill Bombardment = new Skill
            {
                Index = 23,
                Name = "Bombardment",
                SNOPower = SNOPower.X1_Crusader_Bombardment,
                RequiredLevel = 61,
                Description = " Cooldown: 60 seconds Call in an assault from afar, raining spheres of burning pitch and stone onto enemies around you, dealing 570% weapon damage to enemies within 12 yards of the impact zone. The bombardment continues on randomly targeted enemies nearby for the next 5 seconds. ",
                Tooltip = "skill/crusader/bombardment",
                Category = SpellCategory.Conviction,
                IsPrimary = true,
                Class = ActorClass.Crusader,
                Duration = TimeSpan.Zero,
                Cost = 0,
                Cooldown = TimeSpan.FromSeconds(60),
                Element = Element.Physical,
                Resource = Resource.None,
                Runes = new List<Rune>
                {
                    Runes.Crusader.None,
                    Runes.Crusader.BarrelsOfTar,
                    Runes.Crusader.Annihilate,
                    Runes.Crusader.MineField,
                    Runes.Crusader.ImpactfulBombardment,
                    Runes.Crusader.Targeted,
                }
            };
        }

        public class Wizard : FieldCollection<Wizard, Skill>
        {
            /// <summary>
            /// This is a Signature spell. Signature spells are free to cast. Launch a missile of magic energy, dealing 170% weapon damage as Arcane. 
            /// </summary>
            public static Skill MagicMissile = new Skill
            {
                Index = 0,
                Name = "Magic Missile",
                SNOPower = SNOPower.Wizard_MagicMissile,
                RequiredLevel = 1,
                Description = " This is a Signature spell. Signature spells are free to cast. Launch a missile of magic energy, dealing 170% weapon damage as Arcane. ",
                Tooltip = "skill/wizard/magic-missile",
                Category = SpellCategory.Primary,
                IsPrimary = true,
                Class = ActorClass.Wizard,
                Duration = TimeSpan.Zero,
                Cost = 0,
                Cooldown = TimeSpan.Zero,
                Element = Element.Arcane,
                Resource = Resource.None,
                Runes = new List<Rune>
                {
                    Runes.Wizard.None,
                    Runes.Wizard.ChargedBlast,
                    Runes.Wizard.GlacialSpike,
                    Runes.Wizard.Split,
                    Runes.Wizard.Seeker,
                    Runes.Wizard.Conflagrate,
                }
            };

            /// <summary>
            /// Cost: 16 Arcane Power Project a beam of frozen ice that blasts enemies within 5 yards of the first enemy hit for 510% weapon damage as Cold and Slows their movement by 60% for 3 seconds. 
            /// </summary>
            public static Skill RayOfFrost = new Skill
            {
                Index = 1,
                Name = "Ray of Frost",
                SNOPower = SNOPower.Wizard_RayOfFrost,
                RequiredLevel = 2,
                Description = " Cost: 16 Arcane Power Project a beam of frozen ice that blasts enemies within 5 yards of the first enemy hit for 510% weapon damage as Cold and Slows their movement by 60% for 3 seconds. ",
                Tooltip = "skill/wizard/ray-of-frost",
                Category = SpellCategory.Secondary,
                IsPrimary = true,
                Class = ActorClass.Wizard,
                Duration = TimeSpan.FromSeconds(3),
                Cost = 16,
                Cooldown = TimeSpan.Zero,
                Element = Element.Cold,
                Resource = Resource.Arcane,
                Runes = new List<Rune>
                {
                    Runes.Wizard.None,
                    Runes.Wizard.ColdBlood,
                    Runes.Wizard.Numb,
                    Runes.Wizard.BlackIce,
                    Runes.Wizard.SleetStorm,
                    Runes.Wizard.SnowBlast,
                }
            };

            /// <summary>
            /// This is a Signature spell. Signature spells are free to cast. Release a medium range pulse of 3 unpredictable charges of electricity that deal 194% weapon damage as Lightning. 
            /// </summary>
            public static Skill ShockPulse = new Skill
            {
                Index = 2,
                Name = "Shock Pulse",
                SNOPower = SNOPower.Wizard_ShockPulse,
                RequiredLevel = 3,
                Description = " This is a Signature spell. Signature spells are free to cast. Release a medium range pulse of 3 unpredictable charges of electricity that deal 194% weapon damage as Lightning. ",
                Tooltip = "skill/wizard/shock-pulse",
                Category = SpellCategory.Primary,
                IsPrimary = true,
                Class = ActorClass.Wizard,
                Duration = TimeSpan.Zero,
                Cost = 0,
                Cooldown = TimeSpan.Zero,
                Element = Element.Lightning,
                Resource = Resource.None,
                Runes = new List<Rune>
                {
                    Runes.Wizard.None,
                    Runes.Wizard.ExplosiveBolts,
                    Runes.Wizard.FireBolts,
                    Runes.Wizard.PiercingOrb,
                    Runes.Wizard.PowerAffinity,
                    Runes.Wizard.LivingLightning,
                }
            };

            /// <summary>
            /// Cooldown: 11 seconds Blast nearby enemies with an explosion of ice and freeze them for 2 seconds. 
            /// </summary>
            public static Skill FrostNova = new Skill
            {
                Index = 3,
                Name = "Frost Nova",
                SNOPower = SNOPower.Wizard_FrostNova,
                RequiredLevel = 4,
                Description = " Cooldown: 11 seconds Blast nearby enemies with an explosion of ice and freeze them for 2 seconds. ",
                Tooltip = "skill/wizard/frost-nova",
                Category = SpellCategory.Defensive,
                IsPrimary = false,
                Class = ActorClass.Wizard,
                Duration = TimeSpan.FromSeconds(2),
                Cost = 0,
                Cooldown = TimeSpan.FromSeconds(11),
                Element = Element.Cold,
                Resource = Resource.None,
                Runes = new List<Rune>
                {
                    Runes.Wizard.None,
                    Runes.Wizard.Shatter,
                    Runes.Wizard.ColdSnap,
                    Runes.Wizard.FrozenMist,
                    Runes.Wizard.DeepFreeze,
                    Runes.Wizard.BoneChill,
                }
            };

            /// <summary>
            /// Cost: 30 Arcane Power Hurl an orb of pure energy that explodes on contact, dealing 381% weapon damage as Arcane to all enemies within 15 yards. 
            /// </summary>
            public static Skill ArcaneOrb = new Skill
            {
                Index = 4,
                Name = "Arcane Orb",
                SNOPower = SNOPower.Wizard_ArcaneOrb,
                RequiredLevel = 5,
                Description = " Cost: 30 Arcane Power Hurl an orb of pure energy that explodes on contact, dealing 381% weapon damage as Arcane to all enemies within 15 yards. ",
                Tooltip = "skill/wizard/arcane-orb",
                Category = SpellCategory.Secondary,
                IsPrimary = true,
                Class = ActorClass.Wizard,
                Duration = TimeSpan.Zero,
                Cost = 30,
                Cooldown = TimeSpan.Zero,
                Element = Element.Arcane,
                Resource = Resource.Arcane,
                Runes = new List<Rune>
                {
                    Runes.Wizard.None,
                    Runes.Wizard.Obliteration,
                    Runes.Wizard.ArcaneOrbit,
                    Runes.Wizard.Spark,
                    Runes.Wizard.Scorch,
                    Runes.Wizard.FrozenOrb,
                }
            };

            /// <summary>
            /// Cooldown: 15 seconds Transform your skin to diamond for 3 seconds, absorbing up to 71798 damage from incoming attacks. 
            /// </summary>
            public static Skill DiamondSkin = new Skill
            {
                Index = 5,
                Name = "Diamond Skin",
                SNOPower = SNOPower.Wizard_DiamondSkin,
                RequiredLevel = 8,
                Description = " Cooldown: 15 seconds Transform your skin to diamond for 3 seconds, absorbing up to 71798 damage from incoming attacks. ",
                Tooltip = "skill/wizard/diamond-skin",
                Category = SpellCategory.Defensive,
                IsPrimary = false,
                Class = ActorClass.Wizard,
                Duration = TimeSpan.FromSeconds(3),
                Cost = 0,
                Cooldown = TimeSpan.FromSeconds(15),
                Element = Element.Arcane,
                Resource = Resource.None,
                Runes = new List<Rune>
                {
                    Runes.Wizard.None,
                    Runes.Wizard.CrystalShell,
                    Runes.Wizard.Prism,
                    Runes.Wizard.SleekShell,
                    Runes.Wizard.EnduringSkin,
                    Runes.Wizard.DiamondShards,
                }
            };

            /// <summary>
            /// Cost: 25 Arcane Power Discharge a wave of pure energy that deals 351% weapon damage as Arcane to nearby enemies. 
            /// </summary>
            public static Skill WaveOfForce = new Skill
            {
                Index = 6,
                Name = "Wave of Force",
                SNOPower = SNOPower.Wizard_WaveOfForce,
                RequiredLevel = 9,
                Description = " Cost: 25 Arcane Power Discharge a wave of pure energy that deals 351% weapon damage as Arcane to nearby enemies. ",
                Tooltip = "skill/wizard/wave-of-force",
                Category = SpellCategory.Force,
                IsPrimary = true,
                Class = ActorClass.Wizard,
                Duration = TimeSpan.Zero,
                Cost = 25,
                Cooldown = TimeSpan.Zero,
                Element = Element.Arcane,
                Resource = Resource.Arcane,
                Runes = new List<Rune>
                {
                    Runes.Wizard.None,
                    Runes.Wizard.ImpactfulWave,
                    Runes.Wizard.DebilitatingForce,
                    Runes.Wizard.ArcaneAttunement,
                    Runes.Wizard.StaticPulse,
                    Runes.Wizard.HeatWave,
                }
            };

            /// <summary>
            /// This is a Signature spell. Signature spells are free to cast. Summon a spectral blade that strikes all enemies up to 15 yards in front of you for 168% weapon damage as Arcane. 
            /// </summary>
            public static Skill SpectralBlade = new Skill
            {
                Index = 7,
                Name = "Spectral Blade",
                SNOPower = SNOPower.Wizard_SpectralBlade,
                RequiredLevel = 11,
                Description = " This is a Signature spell. Signature spells are free to cast. Summon a spectral blade that strikes all enemies up to 15 yards in front of you for 168% weapon damage as Arcane. ",
                Tooltip = "skill/wizard/spectral-blade",
                Category = SpellCategory.Primary,
                IsPrimary = true,
                Class = ActorClass.Wizard,
                Duration = TimeSpan.Zero,
                Cost = 0,
                Cooldown = TimeSpan.Zero,
                Element = Element.Arcane,
                Resource = Resource.None,
                Runes = new List<Rune>
                {
                    Runes.Wizard.None,
                    Runes.Wizard.FlameBlades,
                    Runes.Wizard.SiphoningBlade,
                    Runes.Wizard.ThrownBlade,
                    Runes.Wizard.BarrierBlades,
                    Runes.Wizard.IceBlades,
                }
            };

            /// <summary>
            /// Cost: 16 Arcane Power Hurl a barrage of arcane projectiles that deal 573% weapon damage as Arcane to all enemies near the impact location. 
            /// </summary>
            public static Skill ArcaneTorrent = new Skill
            {
                Index = 8,
                Name = "Arcane Torrent",
                SNOPower = SNOPower.Wizard_ArcaneTorrent,
                RequiredLevel = 12,
                Description = " Cost: 16 Arcane Power Hurl a barrage of arcane projectiles that deal 573% weapon damage as Arcane to all enemies near the impact location. ",
                Tooltip = "skill/wizard/arcane-torrent",
                Category = SpellCategory.Secondary,
                IsPrimary = true,
                Class = ActorClass.Wizard,
                Duration = TimeSpan.Zero,
                Cost = 16,
                Cooldown = TimeSpan.Zero,
                Element = Element.Arcane,
                Resource = Resource.Arcane,
                Runes = new List<Rune>
                {
                    Runes.Wizard.None,
                    Runes.Wizard.Disruption,
                    Runes.Wizard.DeathBlossom,
                    Runes.Wizard.ArcaneMines,
                    Runes.Wizard.PowerStone,
                    Runes.Wizard.Cascade,
                }
            };

            /// <summary>
            /// Cost: 35 Arcane Power Unleash a twister of pure energy that deals 1000% weapon damage as Arcane over 6 seconds to everything in its path. 
            /// </summary>
            public static Skill EnergyTwister = new Skill
            {
                Index = 9,
                Name = "Energy Twister",
                SNOPower = SNOPower.Wizard_EnergyTwister,
                RequiredLevel = 13,
                Description = " Cost: 35 Arcane Power Unleash a twister of pure energy that deals 1000% weapon damage as Arcane over 6 seconds to everything in its path. ",
                Tooltip = "skill/wizard/energy-twister",
                Category = SpellCategory.Force,
                IsPrimary = true,
                Class = ActorClass.Wizard,
                Duration = TimeSpan.FromSeconds(6),
                Cost = 35,
                Cooldown = TimeSpan.Zero,
                Element = Element.Arcane,
                Resource = Resource.Arcane,
                Runes = new List<Rune>
                {
                    Runes.Wizard.None,
                    Runes.Wizard.MistralBreeze,
                    Runes.Wizard.GaleForce,
                    Runes.Wizard.RagingStorm,
                    Runes.Wizard.WickedWind,
                    Runes.Wizard.StormChaser,
                }
            };

            /// <summary>
            /// Cost: 25 Arcane Power Surround yourself in a barrier of ice that reduces damage from melee attacks by 12%. Melee attackers are either Chilled or Frozen for 3 seconds. Lasts 10 minutes. Only one Armor may be active at a time. 
            /// </summary>
            public static Skill IceArmor = new Skill
            {
                Index = 10,
                Name = "Ice Armor",
                SNOPower = SNOPower.Wizard_IceArmor,
                RequiredLevel = 14,
                Description = " Cost: 25 Arcane Power Surround yourself in a barrier of ice that reduces damage from melee attacks by 12%. Melee attackers are either Chilled or Frozen for 3 seconds. Lasts 10 minutes. Only one Armor may be active at a time. ",
                Tooltip = "skill/wizard/ice-armor",
                Category = SpellCategory.Conjuration,
                IsPrimary = false,
                Class = ActorClass.Wizard,
                Duration = TimeSpan.FromSeconds(3),
                Cost = 25,
                Cooldown = TimeSpan.FromMinutes(10),
                Element = Element.Cold,
                Resource = Resource.Arcane,
                Runes = new List<Rune>
                {
                    Runes.Wizard.None,
                    Runes.Wizard.ChillingAura,
                    Runes.Wizard.Crystallize,
                    Runes.Wizard.JaggedIce,
                    Runes.Wizard.IceReflect,
                    Runes.Wizard.FrozenStorm,
                }
            };

            /// <summary>
            /// This is a Signature spell. Signature spells are free to cast. Lightning arcs from your fingertips, dealing 138% weapon damage as Lightning. The lightning can jump, hitting up to 2 additional enemies. 
            /// </summary>
            public static Skill Electrocute = new Skill
            {
                Index = 11,
                Name = "Electrocute",
                SNOPower = SNOPower.Wizard_Electrocute,
                RequiredLevel = 15,
                Description = " This is a Signature spell. Signature spells are free to cast. Lightning arcs from your fingertips, dealing 138% weapon damage as Lightning. The lightning can jump, hitting up to 2 additional enemies. ",
                Tooltip = "skill/wizard/electrocute",
                Category = SpellCategory.Primary,
                IsPrimary = true,
                Class = ActorClass.Wizard,
                Duration = TimeSpan.Zero,
                Cost = 0,
                Cooldown = TimeSpan.Zero,
                Element = Element.Lightning,
                Resource = Resource.None,
                Runes = new List<Rune>
                {
                    Runes.Wizard.None,
                    Runes.Wizard.ChainLightning,
                    Runes.Wizard.ForkedLightning,
                    Runes.Wizard.LightningBlast,
                    Runes.Wizard.SurgeOfPower,
                    Runes.Wizard.ArcLightning,
                }
            };

            /// <summary>
            /// Cooldown: 15 seconds Invoke a bubble of warped time and space for 15 seconds, reducing enemy attack speed by 20% and movement speed by 60%. This bubble also slows the speed of enemy projectiles by 90%. 
            /// </summary>
            public static Skill SlowTime = new Skill
            {
                Index = 12,
                Name = "Slow Time",
                SNOPower = SNOPower.Wizard_SlowTime,
                RequiredLevel = 16,
                Description = " Cooldown: 15 seconds Invoke a bubble of warped time and space for 15 seconds, reducing enemy attack speed by 20% and movement speed by 60%. This bubble also slows the speed of enemy projectiles by 90%. ",
                Tooltip = "skill/wizard/slow-time",
                Category = SpellCategory.Defensive,
                IsPrimary = true,
                Class = ActorClass.Wizard,
                Duration = TimeSpan.FromSeconds(15),
                Cost = 0,
                Cooldown = TimeSpan.FromSeconds(15),
                Element = Element.Arcane,
                Resource = Resource.None,
                Runes = new List<Rune>
                {
                    Runes.Wizard.None,
                    Runes.Wizard.TimeShell,
                    Runes.Wizard.TimeAndSpace,
                    Runes.Wizard.TimeWarp,
                    Runes.Wizard.PointOfNoReturn,
                    Runes.Wizard.StretchTime,
                }
            };

            /// <summary>
            /// Cost: 25 Arcane Power Bathe yourself in electrical energy, periodically shocking a nearby enemy for 147% weapon damage as Lightning. Lasts 10 minutes. Only one Armor may be active at a time. 
            /// </summary>
            public static Skill StormArmor = new Skill
            {
                Index = 13,
                Name = "Storm Armor",
                SNOPower = SNOPower.Wizard_StormArmor,
                RequiredLevel = 17,
                Description = " Cost: 25 Arcane Power Bathe yourself in electrical energy, periodically shocking a nearby enemy for 147% weapon damage as Lightning. Lasts 10 minutes. Only one Armor may be active at a time. ",
                Tooltip = "skill/wizard/storm-armor",
                Category = SpellCategory.Conjuration,
                IsPrimary = false,
                Class = ActorClass.Wizard,
                Duration = TimeSpan.FromMinutes(10),
                Cost = 25,
                Cooldown = TimeSpan.FromMinutes(10),
                Element = Element.Lightning,
                Resource = Resource.Arcane,
                Runes = new List<Rune>
                {
                    Runes.Wizard.None,
                    Runes.Wizard.ReactiveArmor,
                    Runes.Wizard.PowerOfTheStorm,
                    Runes.Wizard.ThunderStorm,
                    Runes.Wizard.Scramble,
                    Runes.Wizard.ShockingAspect,
                }
            };

            /// <summary>
            /// Cost: 20 Arcane Power Cooldown: 6 seconds Gather an infusion of energy around you that explodes after 1.5 seconds for 616% weapon damage as Arcane to all enemies within 12 yards. 
            /// </summary>
            public static Skill ExplosiveBlast = new Skill
            {
                Index = 14,
                Name = "Explosive Blast",
                SNOPower = SNOPower.Wizard_ExplosiveBlast,
                RequiredLevel = 19,
                Description = " Cost: 20 Arcane Power Cooldown: 6 seconds Gather an infusion of energy around you that explodes after 1.5 seconds for 616% weapon damage as Arcane to all enemies within 12 yards. ",
                Tooltip = "skill/wizard/explosive-blast",
                Category = SpellCategory.Mastery,
                IsPrimary = false,
                Class = ActorClass.Wizard,
                Duration = TimeSpan.Zero,
                Cost = 20,
                Cooldown = TimeSpan.FromSeconds(6),
                Element = Element.Arcane,
                Resource = Resource.Arcane,
                Runes = new List<Rune>
                {
                    Runes.Wizard.None,
                    Runes.Wizard.Unleashed,
                    Runes.Wizard.TimeBomb,
                    Runes.Wizard.ShortFuse,
                    Runes.Wizard.Obliterate,
                    Runes.Wizard.ChainReaction,
                }
            };

            /// <summary>
            /// Cost: 25 Arcane Power Imbue your weapon with magical energy, granting it 10% increased damage. Lasts 10 minutes. 
            /// </summary>
            public static Skill MagicWeapon = new Skill
            {
                Index = 15,
                Name = "Magic Weapon",
                SNOPower = SNOPower.Wizard_MagicWeapon,
                RequiredLevel = 20,
                Description = " Cost: 25 Arcane Power Imbue your weapon with magical energy, granting it 10% increased damage. Lasts 10 minutes. ",
                Tooltip = "skill/wizard/magic-weapon",
                Category = SpellCategory.Conjuration,
                IsPrimary = false,
                Class = ActorClass.Wizard,
                Duration = TimeSpan.FromMinutes(10),
                Cost = 25,
                Cooldown = TimeSpan.FromMinutes(10),
                Element = Element.Arcane,
                Resource = Resource.Arcane,
                Runes = new List<Rune>
                {
                    Runes.Wizard.None,
                    Runes.Wizard.Electrify,
                    Runes.Wizard.ForceWeapon,
                    Runes.Wizard.Conduit,
                    Runes.Wizard.Ignite,
                    Runes.Wizard.Deflection,
                }
            };

            /// <summary>
            /// Cost: 15 Arcane Power Summon a multi-headed Hydra for 15 seconds that attacks enemies with bolts of fire dealing 66% weapon damage as Fire. Only one Hydra may be active at a time. 
            /// </summary>
            public static Skill Hydra = new Skill
            {
                Index = 16,
                Name = "Hydra",
                SNOPower = SNOPower.Wizard_Hydra,
                RequiredLevel = 21,
                Description = " Cost: 15 Arcane Power Summon a multi-headed Hydra for 15 seconds that attacks enemies with bolts of fire dealing 66% weapon damage as Fire. Only one Hydra may be active at a time. ",
                Tooltip = "skill/wizard/hydra",
                Category = SpellCategory.Force,
                IsPrimary = true,
                Class = ActorClass.Wizard,
                Duration = TimeSpan.FromSeconds(15),
                Cost = 15,
                Cooldown = TimeSpan.Zero,
                Element = Element.Fire,
                Resource = Resource.Arcane,
                Runes = new List<Rune>
                {
                    Runes.Wizard.None,
                    Runes.Wizard.ArcaneHydra,
                    Runes.Wizard.LightningHydra,
                    Runes.Wizard.BlazingHydra,
                    Runes.Wizard.FrostHydra,
                    Runes.Wizard.MammothHydra,
                }
            };

            /// <summary>
            /// Cost: 18 Arcane Power Channel a beam of pure energy forward, dealing 511% weapon damage as Arcane and disintegrating enemies it kills. 
            /// </summary>
            public static Skill Disintegrate = new Skill
            {
                Index = 17,
                Name = "Disintegrate",
                SNOPower = SNOPower.Wizard_Disintegrate,
                RequiredLevel = 21,
                Description = " Cost: 18 Arcane Power Channel a beam of pure energy forward, dealing 511% weapon damage as Arcane and disintegrating enemies it kills. ",
                Tooltip = "skill/wizard/disintegrate",
                Category = SpellCategory.Secondary,
                IsPrimary = true,
                Class = ActorClass.Wizard,
                Duration = TimeSpan.Zero,
                Cost = 18,
                Cooldown = TimeSpan.Zero,
                Element = Element.Arcane,
                Resource = Resource.Arcane,
                Runes = new List<Rune>
                {
                    Runes.Wizard.None,
                    Runes.Wizard.Convergence,
                    Runes.Wizard.Volatility,
                    Runes.Wizard.Entropy,
                    Runes.Wizard.ChaosNexus,
                    Runes.Wizard.Intensify,
                }
            };

            /// <summary>
            /// Cost: 20 Arcane Power Summon a Familiar that attacks your enemies for 179% weapon damage as Arcane. This companion cannot be targeted or damaged by enemies. Lasts 10 minutes. 
            /// </summary>
            public static Skill Familiar = new Skill
            {
                Index = 18,
                Name = "Familiar",
                SNOPower = SNOPower.Wizard_Familiar,
                RequiredLevel = 22,
                Description = " Cost: 20 Arcane Power Summon a Familiar that attacks your enemies for 179% weapon damage as Arcane. This companion cannot be targeted or damaged by enemies. Lasts 10 minutes. ",
                Tooltip = "skill/wizard/familiar",
                Category = SpellCategory.Conjuration,
                IsPrimary = false,
                Class = ActorClass.Wizard,
                Duration = TimeSpan.FromMinutes(10),
                Cost = 20,
                Cooldown = TimeSpan.FromMinutes(10),
                Element = Element.Arcane,
                Resource = Resource.Arcane,
                Runes = new List<Rune>
                {
                    Runes.Wizard.None,
                    Runes.Wizard.Sparkflint,
                    Runes.Wizard.Icicle,
                    Runes.Wizard.AncientGuardian,
                    Runes.Wizard.Arcanot,
                    Runes.Wizard.Cannoneer,
                }
            };

            /// <summary>
            /// Cooldown: 11 seconds Teleport through the ether to the selected location up to 50 yards away. 
            /// </summary>
            public static Skill Teleport = new Skill
            {
                Index = 19,
                Name = "Teleport",
                SNOPower = SNOPower.Wizard_Teleport,
                RequiredLevel = 22,
                Description = " Cooldown: 11 seconds Teleport through the ether to the selected location up to 50 yards away. ",
                Tooltip = "skill/wizard/teleport",
                Category = SpellCategory.Defensive,
                IsPrimary = true,
                Class = ActorClass.Wizard,
                Duration = TimeSpan.Zero,
                Cost = 0,
                Cooldown = TimeSpan.FromSeconds(11),
                Element = Element.Arcane,
                Resource = Resource.None,
                Runes = new List<Rune>
                {
                    Runes.Wizard.None,
                    Runes.Wizard.SafePassage,
                    Runes.Wizard.Wormhole,
                    Runes.Wizard.Reversal,
                    Runes.Wizard.Fracture,
                    Runes.Wizard.Calamity,
                }
            };

            /// <summary>
            /// Cooldown: 15 seconds Summon 2 illusionary duplicates of yourself that taunt nearby enemies for 1 second, last for 7 seconds and have 50% of your Life. Spells cast by your Mirror Images will deal 10% of the damage of your own spells. 
            /// </summary>
            public static Skill MirrorImage = new Skill
            {
                Index = 20,
                Name = "Mirror Image",
                SNOPower = SNOPower.Wizard_MirrorImage,
                RequiredLevel = 25,
                Description = " Cooldown: 15 seconds Summon 2 illusionary duplicates of yourself that taunt nearby enemies for 1 second, last for 7 seconds and have 50% of your Life. Spells cast by your Mirror Images will deal 10% of the damage of your own spells. ",
                Tooltip = "skill/wizard/mirror-image",
                Category = SpellCategory.Mastery,
                IsPrimary = true,
                Class = ActorClass.Wizard,
                Duration = TimeSpan.FromSeconds(1),
                Cost = 0,
                Cooldown = TimeSpan.FromSeconds(15),
                Element = Element.Arcane,
                Resource = Resource.None,
                Runes = new List<Rune>
                {
                    Runes.Wizard.None,
                    Runes.Wizard.Simulacrum,
                    Runes.Wizard.Duplicates,
                    Runes.Wizard.MockingDemise,
                    Runes.Wizard.ExtensionOfWill,
                    Runes.Wizard.MirrorMimics,
                }
            };

            /// <summary>
            /// Cost: 40 Arcane Power Summon an immense Meteor that plummets from the sky, crashing into enemies for 501% weapon damage as Fire. The ground it hits is scorched with molten fire that deals 167% weapon damage as Fire over 3 seconds. 
            /// </summary>
            public static Skill Meteor = new Skill
            {
                Index = 21,
                Name = "Meteor",
                SNOPower = SNOPower.Wizard_Meteor,
                RequiredLevel = 25,
                Description = " Cost: 40 Arcane Power Summon an immense Meteor that plummets from the sky, crashing into enemies for 501% weapon damage as Fire. The ground it hits is scorched with molten fire that deals 167% weapon damage as Fire over 3 seconds. ",
                Tooltip = "skill/wizard/meteor",
                Category = SpellCategory.Force,
                IsPrimary = true,
                Class = ActorClass.Wizard,
                Duration = TimeSpan.FromSeconds(3),
                Cost = 40,
                Cooldown = TimeSpan.Zero,
                Element = Element.Fire,
                Resource = Resource.Arcane,
                Runes = new List<Rune>
                {
                    Runes.Wizard.None,
                    Runes.Wizard.LightningBind,
                    Runes.Wizard.StarPact,
                    Runes.Wizard.Comet,
                    Runes.Wizard.MeteorShower,
                    Runes.Wizard.MoltenImpact,
                }
            };

            /// <summary>
            /// Cost: 40 Arcane Power Call down shards of ice that deal 807% weapon damage as Cold over 6 seconds to enemies in a 12 yard radius. Multiple casts in the same area from the same caster do not stack. 
            /// </summary>
            public static Skill Blizzard = new Skill
            {
                Index = 22,
                Name = "Blizzard",
                SNOPower = SNOPower.Wizard_Blizzard,
                RequiredLevel = 27,
                Description = " Cost: 40 Arcane Power Call down shards of ice that deal 807% weapon damage as Cold over 6 seconds to enemies in a 12 yard radius. Multiple casts in the same area from the same caster do not stack. ",
                Tooltip = "skill/wizard/blizzard",
                Category = SpellCategory.Force,
                IsPrimary = true,
                Class = ActorClass.Wizard,
                Duration = TimeSpan.FromSeconds(6),
                Cost = 40,
                Cooldown = TimeSpan.Zero,
                Element = Element.Cold,
                Resource = Resource.Arcane,
                Runes = new List<Rune>
                {
                    Runes.Wizard.None,
                    Runes.Wizard.GraspingChill,
                    Runes.Wizard.FrozenSolid,
                    Runes.Wizard.Snowbound,
                    Runes.Wizard.StarkWinter,
                    Runes.Wizard.UnrelentingStorm,
                }
            };

            /// <summary>
            /// Cost: 25 Arcane Power Focus your energies, increasing your Armor by 35% but decreasing your maximum Arcane Power by 20. Lasts 10 minutes. Only one Armor may be active at a time. 
            /// </summary>
            public static Skill EnergyArmor = new Skill
            {
                Index = 23,
                Name = "Energy Armor",
                SNOPower = SNOPower.Wizard_EnergyArmor,
                RequiredLevel = 28,
                Description = " Cost: 25 Arcane Power Focus your energies, increasing your Armor by 35% but decreasing your maximum Arcane Power by 20. Lasts 10 minutes. Only one Armor may be active at a time. ",
                Tooltip = "skill/wizard/energy-armor",
                Category = SpellCategory.Conjuration,
                IsPrimary = false,
                Class = ActorClass.Wizard,
                Duration = TimeSpan.FromMinutes(10),
                Cost = 25,
                Cooldown = TimeSpan.FromMinutes(10),
                Element = Element.Arcane,
                Resource = Resource.Arcane,
                Runes = new List<Rune>
                {
                    Runes.Wizard.None,
                    Runes.Wizard.Absorption,
                    Runes.Wizard.PinpointBarrier,
                    Runes.Wizard.EnergyTap,
                    Runes.Wizard.ForceArmor,
                    Runes.Wizard.PrismaticArmor,
                }
            };

            /// <summary>
            /// Cooldown: 120 seconds Transform into a being of pure arcane energy for 20 seconds. While in Archon form, your normal abilities are replaced by powerful Archon abilities and your damage, Armor and resistances are increased by 20%. Each enemy killed while in Archon form increases your damage by 6% for the remaining duration of Archon. 
            /// </summary>
            public static Skill Archon = new Skill
            {
                Index = 24,
                Name = "Archon",
                SNOPower = SNOPower.Wizard_Archon,
                RequiredLevel = 30,
                Description = " Cooldown: 120 seconds Transform into a being of pure arcane energy for 20 seconds. While in Archon form, your normal abilities are replaced by powerful Archon abilities and your damage, Armor and resistances are increased by 20%. Each enemy killed while in Archon form increases your damage by 6% for the remaining duration of Archon. ",
                Tooltip = "skill/wizard/archon",
                Category = SpellCategory.Mastery,
                IsPrimary = true,
                Class = ActorClass.Wizard,
                Duration = TimeSpan.FromSeconds(20),
                Cost = 0,
                Cooldown = TimeSpan.FromSeconds(120),
                Element = Element.Arcane,
                Resource = Resource.Arcane,
                Runes = new List<Rune>
                {
                    Runes.Wizard.None,
                    Runes.Wizard.Combustion,
                    Runes.Wizard.Teleport,
                    Runes.Wizard.PurePower,
                    Runes.Wizard.SlowTime,
                    Runes.Wizard.ImprovedArchon,
                }
            };

            /// <summary>
            /// Cost: 20 Arcane Power Cooldown: 12 seconds Conjure a Black Hole at the target location that draws enemies to it and deals 360% weapon damage as Arcane over 2 seconds to all enemies within 15 yards. 
            /// </summary>
            public static Skill BlackHole = new Skill
            {
                Index = 25,
                Name = "Black Hole",
                SNOPower = SNOPower.X1_Wizard_Wormhole,
                RequiredLevel = 61,
                Description = " Cost: 20 Arcane Power Cooldown: 12 seconds Conjure a Black Hole at the target location that draws enemies to it and deals 360% weapon damage as Arcane over 2 seconds to all enemies within 15 yards. ",
                Tooltip = "skill/wizard/black-hole",
                Category = SpellCategory.Mastery,
                IsPrimary = true,
                Class = ActorClass.Wizard,
                Duration = TimeSpan.FromSeconds(2),
                Cost = 20,
                Cooldown = TimeSpan.FromSeconds(12),
                Element = Element.Arcane,
                Resource = Resource.Arcane,
                Runes = new List<Rune>
                {
                    Runes.Wizard.None,
                    Runes.Wizard.Supermassive,
                    Runes.Wizard.AbsoluteZero,
                    Runes.Wizard.EventHorizon,
                    Runes.Wizard.Blazar,
                    Runes.Wizard.Spellsteal,
                }
            };
        }

        public class DemonHunter : FieldCollection<DemonHunter, Skill>
        {
            /// <summary>
            /// Generate: 3 Hatred Fire a magically imbued arrow that seeks out enemies for 155% weapon damage and has a 35% chance to pierce through them. 
            /// </summary>
            public static Skill HungeringArrow = new Skill
            {
                Index = 0,
                Name = "Hungering Arrow",
                SNOPower = SNOPower.DemonHunter_HungeringArrow,
                RequiredLevel = 1,
                Description = " Generate: 3 Hatred Fire a magically imbued arrow that seeks out enemies for 155% weapon damage and has a 35% chance to pierce through them. ",
                Tooltip = "skill/demon-hunter/hungering-arrow",
                Category = SpellCategory.Primary,
                IsPrimary = true,
                Class = ActorClass.DemonHunter,
                Duration = TimeSpan.Zero,
                Cost = 0,
                Cooldown = TimeSpan.Zero,
                Element = Element.Physical,
                Resource = Resource.Hatred,
                Runes = new List<Rune>
                {
                    Runes.DemonHunter.None,
                    Runes.DemonHunter.PuncturingArrow,
                    Runes.DemonHunter.SerratedArrow,
                    Runes.DemonHunter.ShatterShot,
                    Runes.DemonHunter.DevouringArrow,
                    Runes.DemonHunter.SprayOfTeeth,
                }
            };

            /// <summary>
            /// Cost: 20 Hatred Throw a knife that impales an enemy for 620% weapon damage. 
            /// </summary>
            public static Skill Impale = new Skill
            {
                Index = 1,
                Name = "Impale",
                SNOPower = SNOPower.DemonHunter_Impale,
                RequiredLevel = 2,
                Description = " Cost: 20 Hatred Throw a knife that impales an enemy for 620% weapon damage. ",
                Tooltip = "skill/demon-hunter/impale",
                Category = SpellCategory.Secondary,
                IsPrimary = true,
                Class = ActorClass.DemonHunter,
                Duration = TimeSpan.Zero,
                Cost = 20,
                Cooldown = TimeSpan.Zero,
                Element = Element.Physical,
                Resource = Resource.Hatred,
                Runes = new List<Rune>
                {
                    Runes.DemonHunter.None,
                    Runes.DemonHunter.Impact,
                    Runes.DemonHunter.ChemicalBurn,
                    Runes.DemonHunter.Overpenetration,
                    Runes.DemonHunter.Ricochet,
                    Runes.DemonHunter.GrievousWounds,
                }
            };

            /// <summary>
            /// Generate: 3 Hatred Imbue an arrow with shadow energy that deals 200% weapon damage to the primary enemy and entangles up to 2 enemies, slowing their movement by 60% for 2 seconds. When Entangling Shot hits an enemy, the Slow effect on all entangled enemies is refreshed. 
            /// </summary>
            public static Skill EntanglingShot = new Skill
            {
                Index = 2,
                Name = "Entangling Shot",
                SNOPower = SNOPower.X1_DemonHunter_EntanglingShot,
                RequiredLevel = 3,
                Description = " Generate: 3 Hatred Imbue an arrow with shadow energy that deals 200% weapon damage to the primary enemy and entangles up to 2 enemies, slowing their movement by 60% for 2 seconds. When Entangling Shot hits an enemy, the Slow effect on all entangled enemies is refreshed. ",
                Tooltip = "skill/demon-hunter/entangling-shot",
                Category = SpellCategory.Primary,
                IsPrimary = true,
                Class = ActorClass.DemonHunter,
                Duration = TimeSpan.FromSeconds(2),
                Cost = 0,
                Cooldown = TimeSpan.Zero,
                Element = Element.Physical,
                Resource = Resource.Hatred,
                Runes = new List<Rune>
                {
                    Runes.DemonHunter.None,
                    Runes.DemonHunter.ChainGang,
                    Runes.DemonHunter.ShockCollar,
                    Runes.DemonHunter.HeavyBurden,
                    Runes.DemonHunter.JusticeIsServed,
                    Runes.DemonHunter.BountyHunter,
                }
            };

            /// <summary>
            /// Cost: 6 Discipline Lay a trap of caltrops on the ground that activates when an enemy approaches. Once sprung, the caltrops Slow the movement of enemies within 12 yards by 60%. This trap lasts 6 seconds. 
            /// </summary>
            public static Skill Caltrops = new Skill
            {
                Index = 3,
                Name = "Caltrops",
                SNOPower = SNOPower.DemonHunter_Caltrops,
                RequiredLevel = 4,
                Description = " Cost: 6 Discipline Lay a trap of caltrops on the ground that activates when an enemy approaches. Once sprung, the caltrops Slow the movement of enemies within 12 yards by 60%. This trap lasts 6 seconds. ",
                Tooltip = "skill/demon-hunter/caltrops",
                Category = SpellCategory.Defensive,
                IsPrimary = false,
                Class = ActorClass.DemonHunter,
                Duration = TimeSpan.FromSeconds(6),
                Cost = 6,
                Cooldown = TimeSpan.Zero,
                Element = Element.Physical,
                Resource = Resource.Discipline,
                Runes = new List<Rune>
                {
                    Runes.DemonHunter.None,
                    Runes.DemonHunter.HookedSpines,
                    Runes.DemonHunter.TorturousGround,
                    Runes.DemonHunter.JaggedSpikes,
                    Runes.DemonHunter.CarvedStakes,
                    Runes.DemonHunter.BaitTheTrap,
                }
            };

            /// <summary>
            /// Cost: 20 Hatred initially, and an additional 6 Hatred while channeling Rapidly fire for 525% weapon damage as Physical. 
            /// </summary>
            public static Skill RapidFire = new Skill
            {
                Index = 4,
                Name = "Rapid Fire",
                SNOPower = SNOPower.DemonHunter_RapidFire,
                RequiredLevel = 5,
                Description = " Cost: 20 Hatred initially, and an additional 6 Hatred while channeling Rapidly fire for 525% weapon damage as Physical. ",
                Tooltip = "skill/demon-hunter/rapid-fire",
                Category = SpellCategory.Secondary,
                IsPrimary = true,
                Class = ActorClass.DemonHunter,
                Duration = TimeSpan.Zero,
                Cost = 20,
                Cooldown = TimeSpan.Zero,
                Element = Element.Physical,
                Resource = Resource.Hatred,
                Runes = new List<Rune>
                {
                    Runes.DemonHunter.None,
                    Runes.DemonHunter.WitheringFire,
                    Runes.DemonHunter.WebShot,
                    Runes.DemonHunter.FireSupport,
                    Runes.DemonHunter.HighVelocity,
                    Runes.DemonHunter.Bombardment,
                }
            };

            /// <summary>
            /// Cost: 14 Discipline Cooldown: 2 seconds Vanish behind a wall of smoke, becoming momentarily invisible for 1 seconds. 
            /// </summary>
            public static Skill SmokeScreen = new Skill
            {
                Index = 5,
                Name = "Smoke Screen",
                SNOPower = SNOPower.DemonHunter_SmokeScreen,
                RequiredLevel = 8,
                Description = " Cost: 14 Discipline Cooldown: 2 seconds Vanish behind a wall of smoke, becoming momentarily invisible for 1 seconds. ",
                Tooltip = "skill/demon-hunter/smoke-screen",
                Category = SpellCategory.Defensive,
                IsPrimary = false,
                Class = ActorClass.DemonHunter,
                Duration = TimeSpan.FromSeconds(1),
                Cost = 14,
                Cooldown = TimeSpan.FromSeconds(2),
                Element = Element.Physical,
                Resource = Resource.Discipline,
                Runes = new List<Rune>
                {
                    Runes.DemonHunter.None,
                    Runes.DemonHunter.Displacement,
                    Runes.DemonHunter.LingeringFog,
                    Runes.DemonHunter.HealingVapors,
                    Runes.DemonHunter.SpecialRecipe,
                    Runes.DemonHunter.ChokingGas,
                }
            };

            /// <summary>
            /// Cost: 8 Discipline Tumble acrobatically 35 yards. 
            /// </summary>
            public static Skill Vault = new Skill
            {
                Index = 6,
                Name = "Vault",
                SNOPower = SNOPower.DemonHunter_Vault,
                RequiredLevel = 9,
                Description = " Cost: 8 Discipline Tumble acrobatically 35 yards. ",
                Tooltip = "skill/demon-hunter/vault",
                Category = SpellCategory.Hunting,
                IsPrimary = true,
                Class = ActorClass.DemonHunter,
                Duration = TimeSpan.Zero,
                Cost = 8,
                Cooldown = TimeSpan.Zero,
                Element = Element.Physical,
                Resource = Resource.Discipline,
                Runes = new List<Rune>
                {
                    Runes.DemonHunter.None,
                    Runes.DemonHunter.ActionShot,
                    Runes.DemonHunter.RattlingRoll,
                    Runes.DemonHunter.Tumble,
                    Runes.DemonHunter.Acrobatics,
                    Runes.DemonHunter.TrailOfCinders,
                }
            };

            /// <summary>
            /// Generate: 3 Hatred Shoot out an explosive bola that wraps itself around the enemy. After 1 seconds, the bola explodes dealing 160% weapon damage as Fire to the enemy and an additional 110% weapon damage as Fire to all other enemies within 14 yards. 
            /// </summary>
            public static Skill Bolas = new Skill
            {
                Index = 7,
                Name = "Bolas",
                SNOPower = SNOPower.DemonHunter_Bolas,
                RequiredLevel = 11,
                Description = " Generate: 3 Hatred Shoot out an explosive bola that wraps itself around the enemy. After 1 seconds, the bola explodes dealing 160% weapon damage as Fire to the enemy and an additional 110% weapon damage as Fire to all other enemies within 14 yards. ",
                Tooltip = "skill/demon-hunter/bolas",
                Category = SpellCategory.Primary,
                IsPrimary = true,
                Class = ActorClass.DemonHunter,
                Duration = TimeSpan.Zero,
                Cost = 0,
                Cooldown = TimeSpan.Zero,
                Element = Element.Fire,
                Resource = Resource.Hatred,
                Runes = new List<Rune>
                {
                    Runes.DemonHunter.None,
                    Runes.DemonHunter.VolatileExplosives,
                    Runes.DemonHunter.ThunderBall,
                    Runes.DemonHunter.AcidStrike,
                    Runes.DemonHunter.BitterPill,
                    Runes.DemonHunter.ImminentDoom,
                }
            };

            /// <summary>
            /// Cost: 10 Hatred Fire a swirling Chakram that deals 380% weapon damage as Physical to enemies along its path. 
            /// </summary>
            public static Skill Chakram = new Skill
            {
                Index = 8,
                Name = "Chakram",
                SNOPower = SNOPower.DemonHunter_Chakram,
                RequiredLevel = 12,
                Description = " Cost: 10 Hatred Fire a swirling Chakram that deals 380% weapon damage as Physical to enemies along its path. ",
                Tooltip = "skill/demon-hunter/chakram",
                Category = SpellCategory.Secondary,
                IsPrimary = true,
                Class = ActorClass.DemonHunter,
                Duration = TimeSpan.Zero,
                Cost = 10,
                Cooldown = TimeSpan.Zero,
                Element = Element.Physical,
                Resource = Resource.Hatred,
                Runes = new List<Rune>
                {
                    Runes.DemonHunter.None,
                    Runes.DemonHunter.TwinChakrams,
                    Runes.DemonHunter.Serpentine,
                    Runes.DemonHunter.RazorDisk,
                    Runes.DemonHunter.Boomerang,
                    Runes.DemonHunter.ShurikenCloud,
                }
            };

            /// <summary>
            /// Cooldown: 45 seconds Instantly restore 30 Discipline. 
            /// </summary>
            public static Skill Preparation = new Skill
            {
                Index = 9,
                Name = "Preparation",
                SNOPower = SNOPower.DemonHunter_Preparation,
                RequiredLevel = 13,
                Description = " Cooldown: 45 seconds Instantly restore 30 Discipline. ",
                Tooltip = "skill/demon-hunter/preparation",
                Category = SpellCategory.Hunting,
                IsPrimary = false,
                Class = ActorClass.DemonHunter,
                Duration = TimeSpan.Zero,
                Cost = 0,
                Cooldown = TimeSpan.FromSeconds(45),
                Element = Element.Physical,
                Resource = Resource.Discipline,
                Runes = new List<Rune>
                {
                    Runes.DemonHunter.None,
                    Runes.DemonHunter.Invigoration,
                    Runes.DemonHunter.Punishment,
                    Runes.DemonHunter.BattleScars,
                    Runes.DemonHunter.FocusedMind,
                    Runes.DemonHunter.BackupPlan,
                }
            };

            /// <summary>
            /// Cooldown: 10 seconds Throw knives out in a spiral around you, dealing 450% weapon damage to all enemies within 20 yards. Your knives will also Slow the movement of enemies by 60% for 1 seconds. 
            /// </summary>
            public static Skill FanOfKnives = new Skill
            {
                Index = 10,
                Name = "Fan of Knives",
                SNOPower = SNOPower.DemonHunter_FanOfKnives,
                RequiredLevel = 14,
                Description = " Cooldown: 10 seconds Throw knives out in a spiral around you, dealing 450% weapon damage to all enemies within 20 yards. Your knives will also Slow the movement of enemies by 60% for 1 seconds. ",
                Tooltip = "skill/demon-hunter/fan-of-knives",
                Category = SpellCategory.Devices,
                IsPrimary = false,
                Class = ActorClass.DemonHunter,
                Duration = TimeSpan.FromSeconds(1),
                Cost = 0,
                Cooldown = TimeSpan.FromSeconds(10),
                Element = Element.Physical,
                Resource = Resource.None,
                Runes = new List<Rune>
                {
                    Runes.DemonHunter.None,
                    Runes.DemonHunter.PinpointAccuracy,
                    Runes.DemonHunter.BladedArmor,
                    Runes.DemonHunter.KnivesExpert,
                    Runes.DemonHunter.FanOfDaggers,
                    Runes.DemonHunter.AssassinsKnives,
                }
            };

            /// <summary>
            /// Generate: 4 Hatred Shoot a spread of bolts that hits the primary enemy for 200% weapon damage and two additional enemies for 100% weapon damage. If an enemy is in front of you at close range, you will backflip away 15 yards. You may backflip once per 3 seconds. 
            /// </summary>
            public static Skill EvasiveFire = new Skill
            {
                Index = 11,
                Name = "Evasive Fire",
                SNOPower = SNOPower.X1_DemonHunter_EvasiveFire,
                RequiredLevel = 14,
                Description = " Generate: 4 Hatred Shoot a spread of bolts that hits the primary enemy for 200% weapon damage and two additional enemies for 100% weapon damage. If an enemy is in front of you at close range, you will backflip away 15 yards. You may backflip once per 3 seconds. ",
                Tooltip = "skill/demon-hunter/evasive-fire",
                Category = SpellCategory.Primary,
                IsPrimary = true,
                Class = ActorClass.DemonHunter,
                Duration = TimeSpan.Zero,
                Cost = 0,
                Cooldown = TimeSpan.FromSeconds(3),
                Element = Element.Physical,
                Resource = Resource.Hatred,
                Runes = new List<Rune>
                {
                    Runes.DemonHunter.None,
                    Runes.DemonHunter.Hardened,
                    Runes.DemonHunter.PartingGift,
                    Runes.DemonHunter.CoveringFire,
                    Runes.DemonHunter.Displace,
                    Runes.DemonHunter.Surge,
                }
            };

            /// <summary>
            /// Generate: 3 Hatred Throw a grenade that bounces and explodes for 160% weapon damage as Fire. 
            /// </summary>
            public static Skill Grenade = new Skill
            {
                Index = 12,
                Name = "Grenade",
                SNOPower = SNOPower.DemonHunter_Grenades,
                RequiredLevel = 15,
                Description = " Generate: 3 Hatred Throw a grenade that bounces and explodes for 160% weapon damage as Fire. ",
                Tooltip = "skill/demon-hunter/grenade",
                Category = SpellCategory.Primary,
                IsPrimary = true,
                Class = ActorClass.DemonHunter,
                Duration = TimeSpan.Zero,
                Cost = 0,
                Cooldown = TimeSpan.Zero,
                Element = Element.Fire,
                Resource = Resource.Hatred,
                Runes = new List<Rune>
                {
                    Runes.DemonHunter.None,
                    Runes.DemonHunter.Tinkerer,
                    Runes.DemonHunter.ClusterGrenades,
                    Runes.DemonHunter.GrenadeCache,
                    Runes.DemonHunter.StunGrenade,
                    Runes.DemonHunter.GasGrenades,
                }
            };

            /// <summary>
            /// Cost: 14 Discipline Draw in the power of the shadows, gaining 8253 Life per Hit for 5 seconds. 
            /// </summary>
            public static Skill ShadowPower = new Skill
            {
                Index = 13,
                Name = "Shadow Power",
                SNOPower = SNOPower.DemonHunter_ShadowPower,
                RequiredLevel = 16,
                Description = " Cost: 14 Discipline Draw in the power of the shadows, gaining 8253 Life per Hit for 5 seconds. ",
                Tooltip = "skill/demon-hunter/shadow-power",
                Category = SpellCategory.Defensive,
                IsPrimary = false,
                Class = ActorClass.DemonHunter,
                Duration = TimeSpan.FromSeconds(5),
                Cost = 14,
                Cooldown = TimeSpan.Zero,
                Element = Element.Physical,
                Resource = Resource.Discipline,
                Runes = new List<Rune>
                {
                    Runes.DemonHunter.None,
                    Runes.DemonHunter.NightBane,
                    Runes.DemonHunter.BloodMoon,
                    Runes.DemonHunter.WellOfDarkness,
                    Runes.DemonHunter.Gloom,
                    Runes.DemonHunter.ShadowGlide,
                }
            };

            /// <summary>
            /// Cost: 30 Hatred Lay a trap that arms after 2 seconds and triggers when an enemy approaches. The trap has a 2 second re-arming time and can explode up to 3 times, each time dealing 180% weapon damage as Fire to all enemies within 8 yards. You can have a maximum of 3 Spike Traps active at one time. 
            /// </summary>
            public static Skill SpikeTrap = new Skill
            {
                Index = 14,
                Name = "Spike Trap",
                SNOPower = SNOPower.DemonHunter_SpikeTrap,
                RequiredLevel = 17,
                Description = " Cost: 30 Hatred Lay a trap that arms after 2 seconds and triggers when an enemy approaches. The trap has a 2 second re-arming time and can explode up to 3 times, each time dealing 180% weapon damage as Fire to all enemies within 8 yards. You can have a maximum of 3 Spike Traps active at one time. ",
                Tooltip = "skill/demon-hunter/spike-trap",
                Category = SpellCategory.Devices,
                IsPrimary = true,
                Class = ActorClass.DemonHunter,
                Duration = TimeSpan.Zero,
                Cost = 30,
                Cooldown = TimeSpan.Zero,
                Element = Element.Fire,
                Resource = Resource.Hatred,
                Runes = new List<Rune>
                {
                    Runes.DemonHunter.None,
                    Runes.DemonHunter.EchoingBlast,
                    Runes.DemonHunter.StickyTrap,
                    Runes.DemonHunter.LongFuse,
                    Runes.DemonHunter.LightningRod,
                    Runes.DemonHunter.Scatter,
                }
            };

            /// <summary>
            /// Cooldown: 30 seconds Active: Your raven deals an additional 500% damage on its next attack. Passive: Summons a raven companion that pecks at enemies for 100% of your weapon damage as Physical. 
            /// </summary>
            public static Skill Companion = new Skill
            {
                Index = 15,
                Name = "Companion",
                SNOPower = SNOPower.X1_DemonHunter_Companion,
                RequiredLevel = 17,
                Description = " Cooldown: 30 seconds Active: Your raven deals an additional 500% damage on its next attack. Passive: Summons a raven companion that pecks at enemies for 100% of your weapon damage as Physical. ",
                Tooltip = "skill/demon-hunter/companion",
                Category = SpellCategory.Hunting,
                IsPrimary = false,
                Class = ActorClass.DemonHunter,
                Duration = TimeSpan.Zero,
                Cost = 0,
                Cooldown = TimeSpan.FromSeconds(30),
                Element = Element.Physical,
                Resource = Resource.None,
                Runes = new List<Rune>
                {
                    Runes.DemonHunter.None,
                    Runes.DemonHunter.SpiderCompanion,
                    Runes.DemonHunter.BatCompanion,
                    Runes.DemonHunter.BoarCompanion,
                    Runes.DemonHunter.FerretCompanion,
                    Runes.DemonHunter.WolfCompanion,
                }
            };

            /// <summary>
            /// Cost: 12 Hatred Shoot at random nearby enemies for 400% weapon damage while moving at 75% of normal movement speed. 
            /// </summary>
            public static Skill Strafe = new Skill
            {
                Index = 16,
                Name = "Strafe",
                SNOPower = SNOPower.DemonHunter_Strafe,
                RequiredLevel = 19,
                Description = " Cost: 12 Hatred Shoot at random nearby enemies for 400% weapon damage while moving at 75% of normal movement speed. ",
                Tooltip = "skill/demon-hunter/strafe",
                Category = SpellCategory.Archery,
                IsPrimary = true,
                Class = ActorClass.DemonHunter,
                Duration = TimeSpan.Zero,
                Cost = 12,
                Cooldown = TimeSpan.Zero,
                Element = Element.Physical,
                Resource = Resource.Hatred,
                Runes = new List<Rune>
                {
                    Runes.DemonHunter.None,
                    Runes.DemonHunter.Emberstrafe,
                    Runes.DemonHunter.DriftingShadow,
                    Runes.DemonHunter.StingingSteel,
                    Runes.DemonHunter.RocketStorm,
                    Runes.DemonHunter.Demolition,
                }
            };

            /// <summary>
            /// Cost: 10 Hatred Shoot a fire arrow that deals 300% weapon damage as Fire to all enemies it passes through. 
            /// </summary>
            public static Skill ElementalArrow = new Skill
            {
                Index = 17,
                Name = "Elemental Arrow",
                SNOPower = SNOPower.DemonHunter_ElementalArrow,
                RequiredLevel = 20,
                Description = " Cost: 10 Hatred Shoot a fire arrow that deals 300% weapon damage as Fire to all enemies it passes through. ",
                Tooltip = "skill/demon-hunter/elemental-arrow",
                Category = SpellCategory.Secondary,
                IsPrimary = true,
                Class = ActorClass.DemonHunter,
                Duration = TimeSpan.Zero,
                Cost = 10,
                Cooldown = TimeSpan.Zero,
                Element = Element.Fire,
                Resource = Resource.Hatred,
                Runes = new List<Rune>
                {
                    Runes.DemonHunter.None,
                    Runes.DemonHunter.BallLightning,
                    Runes.DemonHunter.FrostArrow,
                    Runes.DemonHunter.ScreamingSkull,
                    Runes.DemonHunter.LightningBolts,
                    Runes.DemonHunter.NetherTentacles,
                }
            };

            /// <summary>
            /// Cost: 3 Discipline Mark an enemy. The marked enemy takes 20% additional damage for the next 30 seconds. 
            /// </summary>
            public static Skill MarkedForDeath = new Skill
            {
                Index = 18,
                Name = "Marked for Death",
                SNOPower = SNOPower.DemonHunter_MarkedForDeath,
                RequiredLevel = 21,
                Description = " Cost: 3 Discipline Mark an enemy. The marked enemy takes 20% additional damage for the next 30 seconds. ",
                Tooltip = "skill/demon-hunter/marked-for-death",
                Category = SpellCategory.Hunting,
                IsPrimary = true,
                Class = ActorClass.DemonHunter,
                Duration = TimeSpan.Zero,
                Cost = 3,
                Cooldown = TimeSpan.Zero,
                Element = Element.Physical,
                Resource = Resource.Discipline,
                Runes = new List<Rune>
                {
                    Runes.DemonHunter.None,
                    Runes.DemonHunter.Contagion,
                    Runes.DemonHunter.ValleyOfDeath,
                    Runes.DemonHunter.GrimReaper,
                    Runes.DemonHunter.MortalEnemy,
                    Runes.DemonHunter.DeathToll,
                }
            };

            /// <summary>
            /// Cost: 25 Hatred Fire a massive volley of arrows dealing 360% weapon damage to all enemies in the area. 
            /// </summary>
            public static Skill Multishot = new Skill
            {
                Index = 19,
                Name = "Multishot",
                SNOPower = SNOPower.DemonHunter_Multishot,
                RequiredLevel = 22,
                Description = " Cost: 25 Hatred Fire a massive volley of arrows dealing 360% weapon damage to all enemies in the area. ",
                Tooltip = "skill/demon-hunter/multishot",
                Category = SpellCategory.Archery,
                IsPrimary = true,
                Class = ActorClass.DemonHunter,
                Duration = TimeSpan.Zero,
                Cost = 25,
                Cooldown = TimeSpan.Zero,
                Element = Element.Physical,
                Resource = Resource.Hatred,
                Runes = new List<Rune>
                {
                    Runes.DemonHunter.None,
                    Runes.DemonHunter.FireAtWill,
                    Runes.DemonHunter.BurstFire,
                    Runes.DemonHunter.SuppressionFire,
                    Runes.DemonHunter.FullBroadside,
                    Runes.DemonHunter.Arsenal,
                }
            };

            /// <summary>
            /// Cost: 30 Hatred Cooldown: 6 seconds Summon a turret that fires at nearby enemies for 200% weapon damage. Lasts 30 seconds. You may have 2 turrets active at a time. 
            /// </summary>
            public static Skill Sentry = new Skill
            {
                Index = 20,
                Name = "Sentry",
                SNOPower = SNOPower.DemonHunter_Sentry,
                RequiredLevel = 25,
                Description = " Cost: 30 Hatred Cooldown: 6 seconds Summon a turret that fires at nearby enemies for 200% weapon damage. Lasts 30 seconds. You may have 2 turrets active at a time. ",
                Tooltip = "skill/demon-hunter/sentry",
                Category = SpellCategory.Devices,
                IsPrimary = true,
                Class = ActorClass.DemonHunter,
                Duration = TimeSpan.FromSeconds(30),
                Cost = 30,
                Cooldown = TimeSpan.FromSeconds(6),
                Element = Element.Physical,
                Resource = Resource.Hatred,
                Runes = new List<Rune>
                {
                    Runes.DemonHunter.None,
                    Runes.DemonHunter.SpitfireTurret,
                    Runes.DemonHunter.ImpalingBolt,
                    Runes.DemonHunter.ChainOfTorment,
                    Runes.DemonHunter.AidStation,
                    Runes.DemonHunter.GuardianTurret,
                }
            };

            /// <summary>
            /// Cost: 40 Hatred Fire a cluster arrow that explodes for 550% weapon damage as Fire into a series of 4 additional grenades that each explode for 220% weapon damage as Fire. 
            /// </summary>
            public static Skill ClusterArrow = new Skill
            {
                Index = 21,
                Name = "Cluster Arrow",
                SNOPower = SNOPower.DemonHunter_ClusterArrow,
                RequiredLevel = 27,
                Description = " Cost: 40 Hatred Fire a cluster arrow that explodes for 550% weapon damage as Fire into a series of 4 additional grenades that each explode for 220% weapon damage as Fire. ",
                Tooltip = "skill/demon-hunter/cluster-arrow",
                Category = SpellCategory.Archery,
                IsPrimary = true,
                Class = ActorClass.DemonHunter,
                Duration = TimeSpan.Zero,
                Cost = 40,
                Cooldown = TimeSpan.Zero,
                Element = Element.Fire,
                Resource = Resource.Hatred,
                Runes = new List<Rune>
                {
                    Runes.DemonHunter.None,
                    Runes.DemonHunter.DazzlingArrow,
                    Runes.DemonHunter.ShootingStars,
                    Runes.DemonHunter.Maelstrom,
                    Runes.DemonHunter.ClusterBombs,
                    Runes.DemonHunter.LoadedForBear,
                }
            };

            /// <summary>
            /// Cooldown: 30 seconds Fire a massive volley of arrows at a large area. Arrows fall from the sky dealing 1250% weapon damage over 5 seconds to all enemies in the area. 
            /// </summary>
            public static Skill RainOfVengeance = new Skill
            {
                Index = 22,
                Name = "Rain of Vengeance",
                SNOPower = SNOPower.DemonHunter_RainOfVengeance,
                RequiredLevel = 30,
                Description = " Cooldown: 30 seconds Fire a massive volley of arrows at a large area. Arrows fall from the sky dealing 1250% weapon damage over 5 seconds to all enemies in the area. ",
                Tooltip = "skill/demon-hunter/rain-of-vengeance",
                Category = SpellCategory.Archery,
                IsPrimary = true,
                Class = ActorClass.DemonHunter,
                Duration = TimeSpan.FromSeconds(5),
                Cost = 0,
                Cooldown = TimeSpan.FromSeconds(30),
                Element = Element.Physical,
                Resource = Resource.None,
                Runes = new List<Rune>
                {
                    Runes.DemonHunter.None,
                    Runes.DemonHunter.DarkCloud,
                    Runes.DemonHunter.Shade,
                    Runes.DemonHunter.Stampede,
                    Runes.DemonHunter.Anathema,
                    Runes.DemonHunter.FlyingStrike,
                }
            };

            /// <summary>
            /// Cooldown: 90 seconds Turn into the physical embodiment of Vengeance for 15 seconds. Side Guns: Gain 4 additional piercing shots for 60% weapon damage each on every attack. Homing Rockets: Shoot 4 rockets at nearby enemies for 40% weapon damage each on every attack. 
            /// </summary>
            public static Skill Vengeance = new Skill
            {
                Index = 23,
                Name = "Vengeance",
                SNOPower = SNOPower.X1_DemonHunter_Vengeance,
                RequiredLevel = 61,
                Description = " Cooldown: 90 seconds Turn into the physical embodiment of Vengeance for 15 seconds. Side Guns: Gain 4 additional piercing shots for 60% weapon damage each on every attack. Homing Rockets: Shoot 4 rockets at nearby enemies for 40% weapon damage each on every attack. ",
                Tooltip = "skill/demon-hunter/vengeance",
                Category = SpellCategory.Devices,
                IsPrimary = false,
                Class = ActorClass.DemonHunter,
                Duration = TimeSpan.FromSeconds(15),
                Cost = 0,
                Cooldown = TimeSpan.FromSeconds(90),
                Element = Element.Physical,
                Resource = Resource.None,
                Runes = new List<Rune>
                {
                    Runes.DemonHunter.None,
                    Runes.DemonHunter.PersonalMortar,
                    Runes.DemonHunter.DarkHeart,
                    Runes.DemonHunter.SideCannons,
                    Runes.DemonHunter.Seethe,
                    Runes.DemonHunter.FromTheShadows,
                }
            };
        }

        public class Monk : FieldCollection<Monk, Skill>
        {
            /// <summary>
            /// Generate: 14 Spirit per attack Teleport to your target and unleash a series of extremely fast punches that deal 122% weapon damage as Lightning. Every third hit deals 183% weapon damage as Lightning split between all enemies in front of you. 
            /// </summary>
            public static Skill FistsOfThunder = new Skill
            {
                Index = 0,
                Name = "Fists of Thunder",
                SNOPower = SNOPower.Monk_FistsofThunder,
                RequiredLevel = 1,
                Description = " Generate: 14 Spirit per attack Teleport to your target and unleash a series of extremely fast punches that deal 122% weapon damage as Lightning. Every third hit deals 183% weapon damage as Lightning split between all enemies in front of you. ",
                Tooltip = "skill/monk/fists-of-thunder",
                Category = SpellCategory.Primary,
                IsPrimary = true,
                Class = ActorClass.Monk,
                Duration = TimeSpan.Zero,
                Cost = 0,
                Cooldown = TimeSpan.Zero,
                Element = Element.Lightning,
                Resource = Resource.Spirit,
                Runes = new List<Rune>
                {
                    Runes.Monk.None,
                    Runes.Monk.Thunderclap,
                    Runes.Monk.WindBlast,
                    Runes.Monk.StaticCharge,
                    Runes.Monk.Quickening,
                    Runes.Monk.BoundingLight,
                }
            };

            /// <summary>
            /// Cost: 30 Spirit Unleash a deadly roundhouse kick that deals 624% weapon damage as Physical. 
            /// </summary>
            public static Skill LashingTailKick = new Skill
            {
                Index = 1,
                Name = "Lashing Tail Kick",
                SNOPower = SNOPower.Monk_LashingTailKick,
                RequiredLevel = 2,
                Description = " Cost: 30 Spirit Unleash a deadly roundhouse kick that deals 624% weapon damage as Physical. ",
                Tooltip = "skill/monk/lashing-tail-kick",
                Category = SpellCategory.Secondary,
                IsPrimary = true,
                Class = ActorClass.Monk,
                Duration = TimeSpan.Zero,
                Cost = 30,
                Cooldown = TimeSpan.Zero,
                Element = Element.Physical,
                Resource = Resource.Spirit,
                Runes = new List<Rune>
                {
                    Runes.Monk.None,
                    Runes.Monk.VultureClawKick,
                    Runes.Monk.SweepingArmada,
                    Runes.Monk.SpinningFlameKick,
                    Runes.Monk.ScorpionSting,
                    Runes.Monk.HandOfYtar,
                }
            };

            /// <summary>
            /// Generate: 12 Spirit per attack Project lines of pure force over a short distance for 109% weapon damage as Physical. Every third hit has a 50% chance to knock enemies up into the air. 
            /// </summary>
            public static Skill DeadlyReach = new Skill
            {
                Index = 2,
                Name = "Deadly Reach",
                SNOPower = SNOPower.Monk_DeadlyReach,
                RequiredLevel = 3,
                Description = " Generate: 12 Spirit per attack Project lines of pure force over a short distance for 109% weapon damage as Physical. Every third hit has a 50% chance to knock enemies up into the air. ",
                Tooltip = "skill/monk/deadly-reach",
                Category = SpellCategory.Primary,
                IsPrimary = true,
                Class = ActorClass.Monk,
                Duration = TimeSpan.Zero,
                Cost = 0,
                Cooldown = TimeSpan.Zero,
                Element = Element.Physical,
                Resource = Resource.Spirit,
                Runes = new List<Rune>
                {
                    Runes.Monk.None,
                    Runes.Monk.PiercingTrident,
                    Runes.Monk.KeenEye,
                    Runes.Monk.ScatteredBlows,
                    Runes.Monk.StrikeFromBeyond,
                    Runes.Monk.Foresight,
                }
            };

            /// <summary>
            /// Cooldown: 15 seconds Create a flash of light that blinds all enemies within 20 yards for 3 seconds. Elite enemies recover faster, but suffer a 30% chance to miss with attacks. 
            /// </summary>
            public static Skill BlindingFlash = new Skill
            {
                Index = 3,
                Name = "Blinding Flash",
                SNOPower = SNOPower.Monk_BlindingFlash,
                RequiredLevel = 4,
                Description = " Cooldown: 15 seconds Create a flash of light that blinds all enemies within 20 yards for 3 seconds. Elite enemies recover faster, but suffer a 30% chance to miss with attacks. ",
                Tooltip = "skill/monk/blinding-flash",
                Category = SpellCategory.Defensive,
                IsPrimary = false,
                Class = ActorClass.Monk,
                Duration = TimeSpan.FromSeconds(3),
                Cost = 0,
                Cooldown = TimeSpan.FromSeconds(15),
                Element = Element.Holy,
                Resource = Resource.None,
                Runes = new List<Rune>
                {
                    Runes.Monk.None,
                    Runes.Monk.SelfReflection,
                    Runes.Monk.MystifyingLight,
                    Runes.Monk.ReplenishingLight,
                    Runes.Monk.SoothingLight,
                    Runes.Monk.FaithInTheLight,
                }
            };

            /// <summary>
            /// Cost: 15 Spirit plus an additional 10 Spirit while channeling Charge directly through your enemies, dealing 240% weapon damage while running. 
            /// </summary>
            public static Skill TempestRush = new Skill
            {
                Index = 4,
                Name = "Tempest Rush",
                SNOPower = SNOPower.Monk_TempestRush,
                RequiredLevel = 5,
                Description = " Cost: 15 Spirit plus an additional 10 Spirit while channeling Charge directly through your enemies, dealing 240% weapon damage while running. ",
                Tooltip = "skill/monk/tempest-rush",
                Category = SpellCategory.Secondary,
                IsPrimary = true,
                Class = ActorClass.Monk,
                Duration = TimeSpan.Zero,
                Cost = 15,
                Cooldown = TimeSpan.Zero,
                Element = Element.Physical,
                Resource = Resource.Spirit,
                Runes = new List<Rune>
                {
                    Runes.Monk.None,
                    Runes.Monk.NorthernBreeze,
                    Runes.Monk.Tailwind,
                    Runes.Monk.Flurry,
                    Runes.Monk.Slipstream,
                    Runes.Monk.Bluster,
                }
            };

            /// <summary>
            /// Cooldown: 15 seconds A blast of divine energy heals you and all allies within 12 yards for 53642 - 70147 Life. Heal amount is increased by 30% of your Health Globe Healing Bonus. 
            /// </summary>
            public static Skill BreathOfHeaven = new Skill
            {
                Index = 5,
                Name = "Breath of Heaven",
                SNOPower = SNOPower.Monk_BreathOfHeaven,
                RequiredLevel = 8,
                Description = " Cooldown: 15 seconds A blast of divine energy heals you and all allies within 12 yards for 53642 - 70147 Life. Heal amount is increased by 30% of your Health Globe Healing Bonus. ",
                Tooltip = "skill/monk/breath-of-heaven",
                Category = SpellCategory.Defensive,
                IsPrimary = false,
                Class = ActorClass.Monk,
                Duration = TimeSpan.Zero,
                Cost = 0,
                Cooldown = TimeSpan.FromSeconds(15),
                Element = Element.Holy,
                Resource = Resource.None,
                Runes = new List<Rune>
                {
                    Runes.Monk.None,
                    Runes.Monk.CircleOfScorn,
                    Runes.Monk.CircleOfLife,
                    Runes.Monk.BlazingWrath,
                    Runes.Monk.InfusedWithLight,
                    Runes.Monk.Zephyr,
                }
            };

            /// <summary>
            /// Cost: 1 Charge Quickly dash up to 50 yards, striking enemies along the way for 305% weapon damage as Physical. You gain a charge every 6 seconds and can have up to 2 charges stored at a time. 
            /// </summary>
            public static Skill DashingStrike = new Skill
            {
                Index = 6,
                Name = "Dashing Strike",
                SNOPower = SNOPower.X1_Monk_DashingStrike,
                RequiredLevel = 9,
                Description = " Cost: 1 Charge Quickly dash up to 50 yards, striking enemies along the way for 305% weapon damage as Physical. You gain a charge every 6 seconds and can have up to 2 charges stored at a time. ",
                Tooltip = "skill/monk/dashing-strike",
                Category = SpellCategory.Techniques,
                IsPrimary = true,
                Class = ActorClass.Monk,
                Duration = TimeSpan.Zero,
                Cost = 1,
                Cooldown = TimeSpan.Zero,
                Element = Element.Physical,
                Resource = Resource.None,
                Runes = new List<Rune>
                {
                    Runes.Monk.None,
                    Runes.Monk.WayOfTheFallingStar,
                    Runes.Monk.BlindingSpeed,
                    Runes.Monk.Quicksilver,
                    Runes.Monk.FlyingSideKick,
                    Runes.Monk.Barrage,
                }
            };

            /// <summary>
            /// Generate: 12 Spirit per attack Unleash a series of large sweeping attacks that deal 143% weapon damage as Physical to all enemies in front of you. Every third hit also dazes enemies within 11 yards, slowing their movement speed by 30% and attack speed by 20% for 3 seconds. 
            /// </summary>
            public static Skill CripplingWave = new Skill
            {
                Index = 7,
                Name = "Crippling Wave",
                SNOPower = SNOPower.Monk_CripplingWave,
                RequiredLevel = 11,
                Description = " Generate: 12 Spirit per attack Unleash a series of large sweeping attacks that deal 143% weapon damage as Physical to all enemies in front of you. Every third hit also dazes enemies within 11 yards, slowing their movement speed by 30% and attack speed by 20% for 3 seconds. ",
                Tooltip = "skill/monk/crippling-wave",
                Category = SpellCategory.Primary,
                IsPrimary = true,
                Class = ActorClass.Monk,
                Duration = TimeSpan.FromSeconds(3),
                Cost = 0,
                Cooldown = TimeSpan.Zero,
                Element = Element.Physical,
                Resource = Resource.Spirit,
                Runes = new List<Rune>
                {
                    Runes.Monk.None,
                    Runes.Monk.Mangle,
                    Runes.Monk.Concussion,
                    Runes.Monk.RisingTide,
                    Runes.Monk.Tsunami,
                    Runes.Monk.BreakingWave,
                }
            };

            /// <summary>
            /// Cost: 75 Spirit Focus a wave of light that crushes enemies for 605% weapon damage as Holy, followed by an additional 79% weapon damage as Holy to all enemies in a line. 
            /// </summary>
            public static Skill WaveOfLight = new Skill
            {
                Index = 8,
                Name = "Wave of Light",
                SNOPower = SNOPower.Monk_WaveOfLight,
                RequiredLevel = 12,
                Description = " Cost: 75 Spirit Focus a wave of light that crushes enemies for 605% weapon damage as Holy, followed by an additional 79% weapon damage as Holy to all enemies in a line. ",
                Tooltip = "skill/monk/wave-of-light",
                Category = SpellCategory.Secondary,
                IsPrimary = true,
                Class = ActorClass.Monk,
                Duration = TimeSpan.Zero,
                Cost = 75,
                Cooldown = TimeSpan.Zero,
                Element = Element.Holy,
                Resource = Resource.Spirit,
                Runes = new List<Rune>
                {
                    Runes.Monk.None,
                    Runes.Monk.WallOfLight,
                    Runes.Monk.ExplosiveLight,
                    Runes.Monk.EmpoweredWave,
                    Runes.Monk.NumbingLight,
                    Runes.Monk.PillarOfTheAncients,
                }
            };

            /// <summary>
            /// Cost: 40 Spirit Cause a enemy to Bleed for 1179% weapon damage as Physical over 9 seconds. If the enemy dies while bleeding, it explodes and deals 50% of its maximum Life as Physical damage to all nearby enemies. 
            /// </summary>
            public static Skill ExplodingPalm = new Skill
            {
                Index = 9,
                Name = "Exploding Palm",
                SNOPower = SNOPower.Monk_ExplodingPalm,
                RequiredLevel = 13,
                Description = " Cost: 40 Spirit Cause a enemy to Bleed for 1179% weapon damage as Physical over 9 seconds. If the enemy dies while bleeding, it explodes and deals 50% of its maximum Life as Physical damage to all nearby enemies. ",
                Tooltip = "skill/monk/exploding-palm",
                Category = SpellCategory.Techniques,
                IsPrimary = true,
                Class = ActorClass.Monk,
                Duration = TimeSpan.FromSeconds(9),
                Cost = 40,
                Cooldown = TimeSpan.Zero,
                Element = Element.Physical,
                Resource = Resource.Spirit,
                Runes = new List<Rune>
                {
                    Runes.Monk.None,
                    Runes.Monk.TheFleshIsWeak,
                    Runes.Monk.StrongSpirit,
                    Runes.Monk.CreepingDemise,
                    Runes.Monk.ImpendingDoom,
                    Runes.Monk.EssenceBurn,
                }
            };

            /// <summary>
            /// Cost: 50 Spirit Pull up to 16 enemies within 24 yards towards you, followed by a furious blast of energy that deals 261% weapon damage as Holy. 
            /// </summary>
            public static Skill CycloneStrike = new Skill
            {
                Index = 10,
                Name = "Cyclone Strike",
                SNOPower = SNOPower.Monk_CycloneStrike,
                RequiredLevel = 14,
                Description = " Cost: 50 Spirit Pull up to 16 enemies within 24 yards towards you, followed by a furious blast of energy that deals 261% weapon damage as Holy. ",
                Tooltip = "skill/monk/cyclone-strike",
                Category = SpellCategory.Focus,
                IsPrimary = true,
                Class = ActorClass.Monk,
                Duration = TimeSpan.Zero,
                Cost = 50,
                Cooldown = TimeSpan.Zero,
                Element = Element.Holy,
                Resource = Resource.Spirit,
                Runes = new List<Rune>
                {
                    Runes.Monk.None,
                    Runes.Monk.EyeOfTheStorm,
                    Runes.Monk.Implosion,
                    Runes.Monk.Sunburst,
                    Runes.Monk.WallOfWind,
                    Runes.Monk.SoothingBreeze,
                }
            };

            /// <summary>
            /// Generate: 12 Spirit per attack Unleash a rapid series of punches that strike enemies for 192% weapon damage as Physical. 
            /// </summary>
            public static Skill WayOfTheHundredFists = new Skill
            {
                Index = 11,
                Name = "Way of the Hundred Fists",
                SNOPower = SNOPower.Monk_WayOfTheHundredFists,
                RequiredLevel = 15,
                Description = " Generate: 12 Spirit per attack Unleash a rapid series of punches that strike enemies for 192% weapon damage as Physical. ",
                Tooltip = "skill/monk/way-of-the-hundred-fists",
                Category = SpellCategory.Primary,
                IsPrimary = true,
                Class = ActorClass.Monk,
                Duration = TimeSpan.Zero,
                Cost = 0,
                Cooldown = TimeSpan.Zero,
                Element = Element.Physical,
                Resource = Resource.Spirit,
                Runes = new List<Rune>
                {
                    Runes.Monk.None,
                    Runes.Monk.HandsOfLightning,
                    Runes.Monk.BlazingFists,
                    Runes.Monk.FistsOfFury,
                    Runes.Monk.SpiritedSalvo,
                    Runes.Monk.WindforceFlurry,
                }
            };

            /// <summary>
            /// Cooldown: 20 seconds You are enveloped in a protective shield that absorbs all incoming damage for 3 seconds and grants immunity to all control impairing effects. 
            /// </summary>
            public static Skill Serenity = new Skill
            {
                Index = 12,
                Name = "Serenity",
                SNOPower = SNOPower.Monk_Serenity,
                RequiredLevel = 16,
                Description = " Cooldown: 20 seconds You are enveloped in a protective shield that absorbs all incoming damage for 3 seconds and grants immunity to all control impairing effects. ",
                Tooltip = "skill/monk/serenity",
                Category = SpellCategory.Defensive,
                IsPrimary = false,
                Class = ActorClass.Monk,
                Duration = TimeSpan.FromSeconds(3),
                Cost = 0,
                Cooldown = TimeSpan.FromSeconds(20),
                Element = Element.Holy,
                Resource = Resource.None,
                Runes = new List<Rune>
                {
                    Runes.Monk.None,
                    Runes.Monk.PeacefulRepose,
                    Runes.Monk.UnwelcomeDisturbance,
                    Runes.Monk.Tranquility,
                    Runes.Monk.Ascension,
                    Runes.Monk.InstantKarma,
                }
            };

            /// <summary>
            /// Cost: 50 Spirit Cooldown: 30 seconds Dash rapidly between nearby enemies, dealing 5677% weapon damage over 7 strikes. 
            /// </summary>
            public static Skill SevensidedStrike = new Skill
            {
                Index = 13,
                Name = "Seven-Sided Strike",
                SNOPower = SNOPower.Monk_SevenSidedStrike,
                RequiredLevel = 17,
                Description = " Cost: 50 Spirit Cooldown: 30 seconds Dash rapidly between nearby enemies, dealing 5677% weapon damage over 7 strikes. ",
                Tooltip = "skill/monk/sevensided-strike",
                Category = SpellCategory.Focus,
                IsPrimary = true,
                Class = ActorClass.Monk,
                Duration = TimeSpan.Zero,
                Cost = 50,
                Cooldown = TimeSpan.FromSeconds(30),
                Element = Element.Physical,
                Resource = Resource.Spirit,
                Runes = new List<Rune>
                {
                    Runes.Monk.None,
                    Runes.Monk.SuddenAssault,
                    Runes.Monk.SeveralsidedStrike,
                    Runes.Monk.Pandemonium,
                    Runes.Monk.SustainedAttack,
                    Runes.Monk.FulminatingOnslaught,
                }
            };

            /// <summary>
            /// Cost: 50 Spirit Active: You and nearby allies gain an additional 17% increased Dodge Chance for 3 seconds. Passive: You and your allies within 60 yards gain 17% increased Dodge Chance. Only one Mantra may be active at a time. 
            /// </summary>
            public static Skill MantraOfEvasion = new Skill
            {
                Index = 14,
                Name = "Mantra of Evasion",
                SNOPower = SNOPower.X1_Monk_MantraOfEvasion_v2,
                RequiredLevel = 19,
                Description = " Cost: 50 Spirit Active: You and nearby allies gain an additional 17% increased Dodge Chance for 3 seconds. Passive: You and your allies within 60 yards gain 17% increased Dodge Chance. Only one Mantra may be active at a time. ",
                Tooltip = "skill/monk/mantra-of-evasion",
                Category = SpellCategory.Mantras,
                IsPrimary = false,
                Class = ActorClass.Monk,
                Duration = TimeSpan.FromSeconds(3),
                Cost = 50,
                Cooldown = TimeSpan.Zero,
                Element = Element.Physical,
                Resource = Resource.Spirit,
                Runes = new List<Rune>
                {
                    Runes.Monk.None,
                    Runes.Monk.HardTarget,
                    Runes.Monk.DivineProtection,
                    Runes.Monk.WindThroughTheReeds,
                    Runes.Monk.Perseverance,
                    Runes.Monk.Backlash,
                }
            };

            /// <summary>
            /// Cost: 75 Spirit Surround yourself in a vortex that continuously deals 30% weapon damage to all enemies within 10 yards. The vortex lasts 6 seconds and is refreshed each time you strike an enemy with a melee attack. Landing a Critical Hit has a chance to increase the vortex effect up to 3 stacks for a total of 90% weapon damage. 
            /// </summary>
            public static Skill SweepingWind = new Skill
            {
                Index = 15,
                Name = "Sweeping Wind",
                SNOPower = SNOPower.Monk_SweepingWind,
                RequiredLevel = 21,
                Description = " Cost: 75 Spirit Surround yourself in a vortex that continuously deals 30% weapon damage to all enemies within 10 yards. The vortex lasts 6 seconds and is refreshed each time you strike an enemy with a melee attack. Landing a Critical Hit has a chance to increase the vortex effect up to 3 stacks for a total of 90% weapon damage. ",
                Tooltip = "skill/monk/sweeping-wind",
                Category = SpellCategory.Techniques,
                IsPrimary = false,
                Class = ActorClass.Monk,
                Duration = TimeSpan.FromSeconds(6),
                Cost = 75,
                Cooldown = TimeSpan.Zero,
                Element = Element.Physical,
                Resource = Resource.Spirit,
                Runes = new List<Rune>
                {
                    Runes.Monk.None,
                    Runes.Monk.MasterOfWind,
                    Runes.Monk.BladeStorm,
                    Runes.Monk.FireStorm,
                    Runes.Monk.InnerStorm,
                    Runes.Monk.Cyclone,
                }
            };

            /// <summary>
            /// Cost: 50 Spirit Active: Increase the amount of damage dealt to 202% for 3 seconds. Passive: You and your allies within 60 yards deal 101% of your weapon damage as Holy to attackers when blocking, dodging or getting hit. Only one Mantra may be active at a time. 
            /// </summary>
            public static Skill MantraOfRetribution = new Skill
            {
                Index = 16,
                Name = "Mantra of Retribution",
                SNOPower = SNOPower.X1_Monk_MantraOfRetribution_v2,
                RequiredLevel = 21,
                Description = " Cost: 50 Spirit Active: Increase the amount of damage dealt to 202% for 3 seconds. Passive: You and your allies within 60 yards deal 101% of your weapon damage as Holy to attackers when blocking, dodging or getting hit. Only one Mantra may be active at a time. ",
                Tooltip = "skill/monk/mantra-of-retribution",
                Category = SpellCategory.Mantras,
                IsPrimary = false,
                Class = ActorClass.Monk,
                Duration = TimeSpan.FromSeconds(3),
                Cost = 50,
                Cooldown = TimeSpan.Zero,
                Element = Element.Holy,
                Resource = Resource.Spirit,
                Runes = new List<Rune>
                {
                    Runes.Monk.None,
                    Runes.Monk.Retaliation,
                    Runes.Monk.Transgression,
                    Runes.Monk.Indignation,
                    Runes.Monk.AgainstAllOdds,
                    Runes.Monk.CollateralDamage,
                }
            };

            /// <summary>
            /// Cooldown: 20 seconds Create a runic circle of protection on the ground for 6 seconds that reduces all damage taken by 55% for all allies inside. 
            /// </summary>
            public static Skill InnerSanctuary = new Skill
            {
                Index = 17,
                Name = "Inner Sanctuary",
                SNOPower = SNOPower.X1_Monk_InnerSanctuary,
                RequiredLevel = 22,
                Description = " Cooldown: 20 seconds Create a runic circle of protection on the ground for 6 seconds that reduces all damage taken by 55% for all allies inside. ",
                Tooltip = "skill/monk/inner-sanctuary",
                Category = SpellCategory.Defensive,
                IsPrimary = false,
                Class = ActorClass.Monk,
                Duration = TimeSpan.FromSeconds(6),
                Cost = 0,
                Cooldown = TimeSpan.FromSeconds(20),
                Element = Element.Holy,
                Resource = Resource.None,
                Runes = new List<Rune>
                {
                    Runes.Monk.None,
                    Runes.Monk.SanctifiedGround,
                    Runes.Monk.SafeHaven,
                    Runes.Monk.TempleOfProtection,
                    Runes.Monk.Intervene,
                    Runes.Monk.ForbiddenPalace,
                }
            };

            /// <summary>
            /// Cooldown: 30 seconds Active: Your mystic ally has its damage increased by 50% for 10 seconds. Passive: A mystic ally fights by your side. The ally deals 40% of your weapon damage as Physical per swing. When the ally dies, it is reborn after 5 seconds. 
            /// </summary>
            public static Skill MysticAlly = new Skill
            {
                Index = 18,
                Name = "Mystic Ally",
                SNOPower = SNOPower.X1_Monk_MysticAlly_v2,
                RequiredLevel = 22,
                Description = " Cooldown: 30 seconds Active: Your mystic ally has its damage increased by 50% for 10 seconds. Passive: A mystic ally fights by your side. The ally deals 40% of your weapon damage as Physical per swing. When the ally dies, it is reborn after 5 seconds. ",
                Tooltip = "skill/monk/mystic-ally",
                Category = SpellCategory.Focus,
                IsPrimary = false,
                Class = ActorClass.Monk,
                Duration = TimeSpan.FromSeconds(10),
                Cost = 0,
                Cooldown = TimeSpan.FromSeconds(30),
                Element = Element.Physical,
                Resource = Resource.None,
                Runes = new List<Rune>
                {
                    Runes.Monk.None,
                    Runes.Monk.WaterAlly,
                    Runes.Monk.FireAlly,
                    Runes.Monk.AirAlly,
                    Runes.Monk.EnduringAlly,
                    Runes.Monk.EarthAlly,
                }
            };

            /// <summary>
            /// Cost: 50 Spirit Active: Shroud you and your allies with a mystical shield that absorbs up to 47742 damage for 3 seconds. Absorb amount is increased by 15% of your Health Globe Healing Bonus. Passive: You and your allies within 60 yards gain 4126 increased Life regeneration. The heal amount is increased by 30% of your Life per Second. Only one Mantra may be active at a time. 
            /// </summary>
            public static Skill MantraOfHealing = new Skill
            {
                Index = 19,
                Name = "Mantra of Healing",
                SNOPower = SNOPower.X1_Monk_MantraOfHealing_v2,
                RequiredLevel = 26,
                Description = " Cost: 50 Spirit Active: Shroud you and your allies with a mystical shield that absorbs up to 47742 damage for 3 seconds. Absorb amount is increased by 15% of your Health Globe Healing Bonus. Passive: You and your allies within 60 yards gain 4126 increased Life regeneration. The heal amount is increased by 30% of your Life per Second. Only one Mantra may be active at a time. ",
                Tooltip = "skill/monk/mantra-of-healing",
                Category = SpellCategory.Mantras,
                IsPrimary = false,
                Class = ActorClass.Monk,
                Duration = TimeSpan.FromSeconds(3),
                Cost = 50,
                Cooldown = TimeSpan.Zero,
                Element = Element.Holy,
                Resource = Resource.Spirit,
                Runes = new List<Rune>
                {
                    Runes.Monk.None,
                    Runes.Monk.Sustenance,
                    Runes.Monk.CircularBreathing,
                    Runes.Monk.BoonOfInspiration,
                    Runes.Monk.HeavenlyBody,
                    Runes.Monk.TimeOfNeed,
                }
            };

            /// <summary>
            /// Cost: 50 Spirit Active: Damage bonus is increased to 20% for 3 seconds. Passive: Enemies within 30 yards of you take 10% increased damage. Only one Mantra may be active at a time. 
            /// </summary>
            public static Skill MantraOfConviction = new Skill
            {
                Index = 20,
                Name = "Mantra of Conviction",
                SNOPower = SNOPower.X1_Monk_MantraOfConviction_v2,
                RequiredLevel = 30,
                Description = " Cost: 50 Spirit Active: Damage bonus is increased to 20% for 3 seconds. Passive: Enemies within 30 yards of you take 10% increased damage. Only one Mantra may be active at a time. ",
                Tooltip = "skill/monk/mantra-of-conviction",
                Category = SpellCategory.Mantras,
                IsPrimary = false,
                Class = ActorClass.Monk,
                Duration = TimeSpan.FromSeconds(3),
                Cost = 50,
                Cooldown = TimeSpan.Zero,
                Element = Element.Physical,
                Resource = Resource.Spirit,
                Runes = new List<Rune>
                {
                    Runes.Monk.None,
                    Runes.Monk.Overawe,
                    Runes.Monk.Intimidation,
                    Runes.Monk.Dishearten,
                    Runes.Monk.Annihilation,
                    Runes.Monk.Submission,
                }
            };

            /// <summary>
            /// Cooldown: 60 seconds Have an Epiphany, increasing your Spirit Regeneration per Second by 20 and enabling your melee attacks to instantly dash to your target for 15 seconds. 
            /// </summary>
            public static Skill Epiphany = new Skill
            {
                Index = 21,
                Name = "Epiphany",
                SNOPower = SNOPower.X1_Monk_Epiphany,
                RequiredLevel = 61,
                Description = " Cooldown: 60 seconds Have an Epiphany, increasing your Spirit Regeneration per Second by 20 and enabling your melee attacks to instantly dash to your target for 15 seconds. ",
                Tooltip = "skill/monk/epiphany",
                Category = SpellCategory.Focus,
                IsPrimary = false,
                Class = ActorClass.Monk,
                Duration = TimeSpan.FromSeconds(15),
                Cost = 0,
                Cooldown = TimeSpan.FromSeconds(60),
                Element = Element.Holy,
                Resource = Resource.Spirit,
                Runes = new List<Rune>
                {
                    Runes.Monk.None,
                    Runes.Monk.DesertShroud,
                    Runes.Monk.Ascendance,
                    Runes.Monk.SoothingMist,
                    Runes.Monk.Windwalker,
                    Runes.Monk.InnerFire,
                }
            };
        }

        public class WitchDoctor : FieldCollection<WitchDoctor, Skill>
        {
            /// <summary>
            /// This is a Signature spell. Signature spells are free to cast. Shoot a deadly Poison Dart that deals 185% weapon damage as Poison and an additional 40% weapon damage as Poison over 2 seconds. 
            /// </summary>
            public static Skill PoisonDart = new Skill
            {
                Index = 0,
                Name = "Poison Dart",
                SNOPower = SNOPower.Witchdoctor_PoisonDart,
                RequiredLevel = 1,
                Description = " This is a Signature spell. Signature spells are free to cast. Shoot a deadly Poison Dart that deals 185% weapon damage as Poison and an additional 40% weapon damage as Poison over 2 seconds. ",
                Tooltip = "skill/witch-doctor/poison-dart",
                Category = SpellCategory.Primary,
                IsPrimary = true,
                Class = ActorClass.Witchdoctor,
                Duration = TimeSpan.FromSeconds(2),
                Cost = 0,
                Cooldown = TimeSpan.Zero,
                Element = Element.Poison,
                Resource = Resource.None,
                Runes = new List<Rune>
                {
                    Runes.WitchDoctor.None,
                    Runes.WitchDoctor.Splinters,
                    Runes.WitchDoctor.NumbingDart,
                    Runes.WitchDoctor.SpinedDart,
                    Runes.WitchDoctor.FlamingDart,
                    Runes.WitchDoctor.SnakeToTheFace,
                }
            };

            /// <summary>
            /// Cost: 150 Mana Cooldown: 8 seconds Ghoulish hands reach out from the ground, slowing enemy movement by 60% and dealing 560% weapon damage as Physical over 8 seconds. 
            /// </summary>
            public static Skill GraspOfTheDead = new Skill
            {
                Index = 1,
                Name = "Grasp of the Dead",
                SNOPower = SNOPower.Witchdoctor_GraspOfTheDead,
                RequiredLevel = 2,
                Description = " Cost: 150 Mana Cooldown: 8 seconds Ghoulish hands reach out from the ground, slowing enemy movement by 60% and dealing 560% weapon damage as Physical over 8 seconds. ",
                Tooltip = "skill/witch-doctor/grasp-of-the-dead",
                Category = SpellCategory.Secondary,
                IsPrimary = true,
                Class = ActorClass.Witchdoctor,
                Duration = TimeSpan.FromSeconds(8),
                Cost = 150,
                Cooldown = TimeSpan.FromSeconds(8),
                Element = Element.Physical,
                Resource = Resource.Mana,
                Runes = new List<Rune>
                {
                    Runes.WitchDoctor.None,
                    Runes.WitchDoctor.UnbreakableGrasp,
                    Runes.WitchDoctor.GropingEels,
                    Runes.WitchDoctor.DeathIsLife,
                    Runes.WitchDoctor.DesperateGrasp,
                    Runes.WitchDoctor.RainOfCorpses,
                }
            };

            /// <summary>
            /// This is a Signature spell. Signature spells are free to cast. Throw a jar with 4 spiders that attack nearby enemies for a total of 324% weapon damage as Physical before dying. 
            /// </summary>
            public static Skill CorpseSpiders = new Skill
            {
                Index = 2,
                Name = "Corpse Spiders",
                SNOPower = SNOPower.Witchdoctor_CorpseSpider,
                RequiredLevel = 3,
                Description = " This is a Signature spell. Signature spells are free to cast. Throw a jar with 4 spiders that attack nearby enemies for a total of 324% weapon damage as Physical before dying. ",
                Tooltip = "skill/witch-doctor/corpse-spiders",
                Category = SpellCategory.Primary,
                IsPrimary = true,
                Class = ActorClass.Witchdoctor,
                Duration = TimeSpan.Zero,
                Cost = 0,
                Cooldown = TimeSpan.Zero,
                Element = Element.Physical,
                Resource = Resource.None,
                Runes = new List<Rune>
                {
                    Runes.WitchDoctor.None,
                    Runes.WitchDoctor.LeapingSpiders,
                    Runes.WitchDoctor.SpiderQueen,
                    Runes.WitchDoctor.Widowmakers,
                    Runes.WitchDoctor.MedusaSpiders,
                    Runes.WitchDoctor.BlazingSpiders,
                }
            };

            /// <summary>
            /// Cooldown: 45 seconds Summon 3 Zombie Dogs from the depths to fight by your side. Each dog deals 30% of your weapon damage as Physical per hit. 
            /// </summary>
            public static Skill SummonZombieDogs = new Skill
            {
                Index = 3,
                Name = "Summon Zombie Dogs",
                SNOPower = SNOPower.Witchdoctor_SummonZombieDog,
                RequiredLevel = 4,
                Description = " Cooldown: 45 seconds Summon 3 Zombie Dogs from the depths to fight by your side. Each dog deals 30% of your weapon damage as Physical per hit. ",
                Tooltip = "skill/witch-doctor/summon-zombie-dogs",
                Category = SpellCategory.Defensive,
                IsPrimary = true,
                Class = ActorClass.Witchdoctor,
                Duration = TimeSpan.Zero,
                Cost = 0,
                Cooldown = TimeSpan.FromSeconds(45),
                Element = Element.Physical,
                Resource = Resource.None,
                Runes = new List<Rune>
                {
                    Runes.WitchDoctor.None,
                    Runes.WitchDoctor.RabidDogs,
                    Runes.WitchDoctor.FinalGift,
                    Runes.WitchDoctor.LifeLink,
                    Runes.WitchDoctor.BurningDogs,
                    Runes.WitchDoctor.LeechingBeasts,
                }
            };

            /// <summary>
            /// Cost: 150 Mana initially, and an additional 75 Mana while channeling Call forth a swarm of fiery bats to burn enemies in front of you for 425% weapon damage as Fire. 
            /// </summary>
            public static Skill Firebats = new Skill
            {
                Index = 4,
                Name = "Firebats",
                SNOPower = SNOPower.Witchdoctor_Firebats,
                RequiredLevel = 5,
                Description = " Cost: 150 Mana initially, and an additional 75 Mana while channeling Call forth a swarm of fiery bats to burn enemies in front of you for 425% weapon damage as Fire. ",
                Tooltip = "skill/witch-doctor/firebats",
                Category = SpellCategory.Secondary,
                IsPrimary = true,
                Class = ActorClass.Witchdoctor,
                Duration = TimeSpan.Zero,
                Cost = 150,
                Cooldown = TimeSpan.Zero,
                Element = Element.Fire,
                Resource = Resource.Mana,
                Runes = new List<Rune>
                {
                    Runes.WitchDoctor.None,
                    Runes.WitchDoctor.DireBats,
                    Runes.WitchDoctor.VampireBats,
                    Runes.WitchDoctor.PlagueBats,
                    Runes.WitchDoctor.HungryBats,
                    Runes.WitchDoctor.CloudOfBats,
                }
            };

            /// <summary>
            /// Cooldown: 12 seconds Don a spectral mask that horrifies all enemies within 18 yards, causing them to tremor in Fear and be Immobilized for 3 seconds. 
            /// </summary>
            public static Skill Horrify = new Skill
            {
                Index = 5,
                Name = "Horrify",
                SNOPower = SNOPower.Witchdoctor_Horrify,
                RequiredLevel = 8,
                Description = " Cooldown: 12 seconds Don a spectral mask that horrifies all enemies within 18 yards, causing them to tremor in Fear and be Immobilized for 3 seconds. ",
                Tooltip = "skill/witch-doctor/horrify",
                Category = SpellCategory.Defensive,
                IsPrimary = false,
                Class = ActorClass.Witchdoctor,
                Duration = TimeSpan.FromSeconds(3),
                Cost = 0,
                Cooldown = TimeSpan.FromSeconds(12),
                Element = Element.Physical,
                Resource = Resource.None,
                Runes = new List<Rune>
                {
                    Runes.WitchDoctor.None,
                    Runes.WitchDoctor.Phobia,
                    Runes.WitchDoctor.Stalker,
                    Runes.WitchDoctor.FaceOfDeath,
                    Runes.WitchDoctor.FrighteningAspect,
                    Runes.WitchDoctor.RuthlessTerror,
                }
            };

            /// <summary>
            /// Cooldown: 15 seconds Feed on the life force of up to 5 enemies within 16 yards. Gain 2% Intelligence for each affected enemy. This effect lasts 30 seconds. 
            /// </summary>
            public static Skill SoulHarvest = new Skill
            {
                Index = 6,
                Name = "Soul Harvest",
                SNOPower = SNOPower.Witchdoctor_SoulHarvest,
                RequiredLevel = 9,
                Description = " Cooldown: 15 seconds Feed on the life force of up to 5 enemies within 16 yards. Gain 2% Intelligence for each affected enemy. This effect lasts 30 seconds. ",
                Tooltip = "skill/witch-doctor/soul-harvest",
                Category = SpellCategory.Terror,
                IsPrimary = false,
                Class = ActorClass.Witchdoctor,
                Duration = TimeSpan.FromSeconds(30),
                Cost = 0,
                Cooldown = TimeSpan.FromSeconds(15),
                Element = Element.Physical,
                Resource = Resource.None,
                Runes = new List<Rune>
                {
                    Runes.WitchDoctor.None,
                    Runes.WitchDoctor.SwallowYourSoul,
                    Runes.WitchDoctor.Siphon,
                    Runes.WitchDoctor.Languish,
                    Runes.WitchDoctor.SoulToWaste,
                    Runes.WitchDoctor.VengefulSpirit,
                }
            };

            /// <summary>
            /// This is a Signature spell. Signature spells are free to cast. Release a handful of toads that deal 190% weapon damage as Poison to enemies they come in contact with. 
            /// </summary>
            public static Skill PlagueOfToads = new Skill
            {
                Index = 7,
                Name = "Plague of Toads",
                SNOPower = SNOPower.Witchdoctor_PlagueOfToads,
                RequiredLevel = 11,
                Description = " This is a Signature spell. Signature spells are free to cast. Release a handful of toads that deal 190% weapon damage as Poison to enemies they come in contact with. ",
                Tooltip = "skill/witch-doctor/plague-of-toads",
                Category = SpellCategory.Primary,
                IsPrimary = true,
                Class = ActorClass.Witchdoctor,
                Duration = TimeSpan.Zero,
                Cost = 0,
                Cooldown = TimeSpan.Zero,
                Element = Element.Poison,
                Resource = Resource.None,
                Runes = new List<Rune>
                {
                    Runes.WitchDoctor.None,
                    Runes.WitchDoctor.ExplosiveToads,
                    Runes.WitchDoctor.PiercingToads,
                    Runes.WitchDoctor.RainOfToads,
                    Runes.WitchDoctor.AddlingToads,
                    Runes.WitchDoctor.ToadAffinity,
                }
            };

            /// <summary>
            /// Cost: 50 Mana Haunt an enemy with a spirit, dealing 4000% weapon damage as Cold over 12 seconds. If the enemy dies, the spirit will haunt another nearby enemy. An enemy can only be affected by one Haunt at a time. 
            /// </summary>
            public static Skill Haunt = new Skill
            {
                Index = 8,
                Name = "Haunt",
                SNOPower = SNOPower.Witchdoctor_Haunt,
                RequiredLevel = 12,
                Description = " Cost: 50 Mana Haunt an enemy with a spirit, dealing 4000% weapon damage as Cold over 12 seconds. If the enemy dies, the spirit will haunt another nearby enemy. An enemy can only be affected by one Haunt at a time. ",
                Tooltip = "skill/witch-doctor/haunt",
                Category = SpellCategory.Secondary,
                IsPrimary = true,
                Class = ActorClass.Witchdoctor,
                Duration = TimeSpan.FromSeconds(12),
                Cost = 50,
                Cooldown = TimeSpan.Zero,
                Element = Element.Cold,
                Resource = Resource.Mana,
                Runes = new List<Rune>
                {
                    Runes.WitchDoctor.None,
                    Runes.WitchDoctor.ConsumingSpirit,
                    Runes.WitchDoctor.ResentfulSpirits,
                    Runes.WitchDoctor.LingeringSpirit,
                    Runes.WitchDoctor.GraspingSpirit,
                    Runes.WitchDoctor.DrainingSpirit,
                }
            };

            /// <summary>
            /// Banish your Zombie Dogs and cause them to explode, each dealing 185% of your weapon damage as Physical to all enemies within 12 yards. Only summoned Zombie Dogs may be sacrificed. 
            /// </summary>
            public static Skill Sacrifice = new Skill
            {
                Index = 9,
                Name = "Sacrifice",
                SNOPower = SNOPower.Witchdoctor_Sacrifice,
                RequiredLevel = 13,
                Description = " Banish your Zombie Dogs and cause them to explode, each dealing 185% of your weapon damage as Physical to all enemies within 12 yards. Only summoned Zombie Dogs may be sacrificed. ",
                Tooltip = "skill/witch-doctor/sacrifice",
                Category = SpellCategory.Terror,
                IsPrimary = true,
                Class = ActorClass.Witchdoctor,
                Duration = TimeSpan.Zero,
                Cost = 0,
                Cooldown = TimeSpan.Zero,
                Element = Element.Physical,
                Resource = Resource.None,
                Runes = new List<Rune>
                {
                    Runes.WitchDoctor.None,
                    Runes.WitchDoctor.BlackBlood,
                    Runes.WitchDoctor.NextOfKin,
                    Runes.WitchDoctor.Pride,
                    Runes.WitchDoctor.ForTheMaster,
                    Runes.WitchDoctor.ProvokeThePack,
                }
            };

            /// <summary>
            /// Cost: 150 Mana Call forth a reckless, suicidal zombie that deals 560% weapon damage as Poison to all enemies in its path before decomposing. 
            /// </summary>
            public static Skill ZombieCharger = new Skill
            {
                Index = 10,
                Name = "Zombie Charger",
                SNOPower = SNOPower.Witchdoctor_ZombieCharger,
                RequiredLevel = 14,
                Description = " Cost: 150 Mana Call forth a reckless, suicidal zombie that deals 560% weapon damage as Poison to all enemies in its path before decomposing. ",
                Tooltip = "skill/witch-doctor/zombie-charger",
                Category = SpellCategory.Decay,
                IsPrimary = true,
                Class = ActorClass.Witchdoctor,
                Duration = TimeSpan.Zero,
                Cost = 150,
                Cooldown = TimeSpan.Zero,
                Element = Element.Poison,
                Resource = Resource.Mana,
                Runes = new List<Rune>
                {
                    Runes.WitchDoctor.None,
                    Runes.WitchDoctor.PileOn,
                    Runes.WitchDoctor.Undeath,
                    Runes.WitchDoctor.LumberingCold,
                    Runes.WitchDoctor.ExplosiveBeast,
                    Runes.WitchDoctor.ZombieBears,
                }
            };

            /// <summary>
            /// Cooldown: 15 seconds Leave your physical body and enter the spirit realm for 2 seconds. While in the spirit realm, your movement is unhindered. Your link to the spirit realm will end if your physical body sustains 50% of your maximum Life in damage. 
            /// </summary>
            public static Skill SpiritWalk = new Skill
            {
                Index = 11,
                Name = "Spirit Walk",
                SNOPower = SNOPower.Witchdoctor_SpiritWalk,
                RequiredLevel = 16,
                Description = " Cooldown: 15 seconds Leave your physical body and enter the spirit realm for 2 seconds. While in the spirit realm, your movement is unhindered. Your link to the spirit realm will end if your physical body sustains 50% of your maximum Life in damage. ",
                Tooltip = "skill/witch-doctor/spirit-walk",
                Category = SpellCategory.Defensive,
                IsPrimary = false,
                Class = ActorClass.Witchdoctor,
                Duration = TimeSpan.FromSeconds(2),
                Cost = 0,
                Cooldown = TimeSpan.FromSeconds(15),
                Element = Element.Physical,
                Resource = Resource.Spirit,
                Runes = new List<Rune>
                {
                    Runes.WitchDoctor.None,
                    Runes.WitchDoctor.Jaunt,
                    Runes.WitchDoctor.HonoredGuest,
                    Runes.WitchDoctor.UmbralShock,
                    Runes.WitchDoctor.Severance,
                    Runes.WitchDoctor.HealingJourney,
                }
            };

            /// <summary>
            /// Cost: 100 Mana Bombard an enemy with 4 spirit bolts deal a total of 425% weapon damage as Cold. 
            /// </summary>
            public static Skill SpiritBarrage = new Skill
            {
                Index = 12,
                Name = "Spirit Barrage",
                SNOPower = SNOPower.Witchdoctor_SpiritBarrage,
                RequiredLevel = 17,
                Description = " Cost: 100 Mana Bombard an enemy with 4 spirit bolts deal a total of 425% weapon damage as Cold. ",
                Tooltip = "skill/witch-doctor/spirit-barrage",
                Category = SpellCategory.Decay,
                IsPrimary = true,
                Class = ActorClass.Witchdoctor,
                Duration = TimeSpan.Zero,
                Cost = 100,
                Cooldown = TimeSpan.Zero,
                Element = Element.Cold,
                Resource = Resource.Mana,
                Runes = new List<Rune>
                {
                    Runes.WitchDoctor.None,
                    Runes.WitchDoctor.TheSpiritIsWilling,
                    Runes.WitchDoctor.WellOfSouls,
                    Runes.WitchDoctor.Phantasm,
                    Runes.WitchDoctor.Phlebotomize,
                    Runes.WitchDoctor.Manitou,
                }
            };

            /// <summary>
            /// Cooldown: 60 seconds Summon a Gargantuan zombie to fight for you. The Gargantuan attacks for 100% of your weapon damage as Physical. 
            /// </summary>
            public static Skill Gargantuan = new Skill
            {
                Index = 13,
                Name = "Gargantuan",
                SNOPower = SNOPower.Witchdoctor_Gargantuan,
                RequiredLevel = 19,
                Description = " Cooldown: 60 seconds Summon a Gargantuan zombie to fight for you. The Gargantuan attacks for 100% of your weapon damage as Physical. ",
                Tooltip = "skill/witch-doctor/gargantuan",
                Category = SpellCategory.Voodoo,
                IsPrimary = true,
                Class = ActorClass.Witchdoctor,
                Duration = TimeSpan.Zero,
                Cost = 0,
                Cooldown = TimeSpan.FromSeconds(60),
                Element = Element.Physical,
                Resource = Resource.None,
                Runes = new List<Rune>
                {
                    Runes.WitchDoctor.None,
                    Runes.WitchDoctor.Humongoid,
                    Runes.WitchDoctor.RestlessGiant,
                    Runes.WitchDoctor.WrathfulProtector,
                    Runes.WitchDoctor.BigStinker,
                    Runes.WitchDoctor.Bruiser,
                }
            };

            /// <summary>
            /// Cost: 300 Mana Unleash a plague of locusts that swarms an enemy, dealing 1040% weapon damage as Poison over 8 seconds. The locusts will jump to additional nearby enemies. 
            /// </summary>
            public static Skill LocustSwarm = new Skill
            {
                Index = 14,
                Name = "Locust Swarm",
                SNOPower = SNOPower.Witchdoctor_Locust_Swarm,
                RequiredLevel = 21,
                Description = " Cost: 300 Mana Unleash a plague of locusts that swarms an enemy, dealing 1040% weapon damage as Poison over 8 seconds. The locusts will jump to additional nearby enemies. ",
                Tooltip = "skill/witch-doctor/locust-swarm",
                Category = SpellCategory.Secondary,
                IsPrimary = true,
                Class = ActorClass.Witchdoctor,
                Duration = TimeSpan.FromSeconds(8),
                Cost = 300,
                Cooldown = TimeSpan.Zero,
                Element = Element.Poison,
                Resource = Resource.Mana,
                Runes = new List<Rune>
                {
                    Runes.WitchDoctor.None,
                    Runes.WitchDoctor.Pestilence,
                    Runes.WitchDoctor.DevouringSwarm,
                    Runes.WitchDoctor.CloudOfInsects,
                    Runes.WitchDoctor.DiseasedSwarm,
                    Runes.WitchDoctor.SearingLocusts,
                }
            };

            /// <summary>
            /// This is a Signature spell. Signature spells are free to cast. Lob an explosive skull that deals 155% weapon damage as Fire to all enemies within 8 yards. 
            /// </summary>
            public static Skill Firebomb = new Skill
            {
                Index = 15,
                Name = "Firebomb",
                SNOPower = SNOPower.Witchdoctor_Firebomb,
                RequiredLevel = 21,
                Description = " This is a Signature spell. Signature spells are free to cast. Lob an explosive skull that deals 155% weapon damage as Fire to all enemies within 8 yards. ",
                Tooltip = "skill/witch-doctor/firebomb",
                Category = SpellCategory.Primary,
                IsPrimary = true,
                Class = ActorClass.Witchdoctor,
                Duration = TimeSpan.Zero,
                Cost = 0,
                Cooldown = TimeSpan.Zero,
                Element = Element.Fire,
                Resource = Resource.None,
                Runes = new List<Rune>
                {
                    Runes.WitchDoctor.None,
                    Runes.WitchDoctor.FlashFire,
                    Runes.WitchDoctor.RollTheBones,
                    Runes.WitchDoctor.FirePit,
                    Runes.WitchDoctor.Pyrogeist,
                    Runes.WitchDoctor.GhostBomb,
                }
            };

            /// <summary>
            /// Cooldown: 15 seconds Summon a Fetish Shaman for 12 seconds that will hex enemies into chickens. Hexed enemies are unable to perform offensive actions and take 10% additional damage. 
            /// </summary>
            public static Skill Hex = new Skill
            {
                Index = 16,
                Name = "Hex",
                SNOPower = SNOPower.Witchdoctor_Hex,
                RequiredLevel = 22,
                Description = " Cooldown: 15 seconds Summon a Fetish Shaman for 12 seconds that will hex enemies into chickens. Hexed enemies are unable to perform offensive actions and take 10% additional damage. ",
                Tooltip = "skill/witch-doctor/hex",
                Category = SpellCategory.Defensive,
                IsPrimary = false,
                Class = ActorClass.Witchdoctor,
                Duration = TimeSpan.FromSeconds(12),
                Cost = 0,
                Cooldown = TimeSpan.FromSeconds(15),
                Element = Element.Physical,
                Resource = Resource.None,
                Runes = new List<Rune>
                {
                    Runes.WitchDoctor.None,
                    Runes.WitchDoctor.HedgeMagic,
                    Runes.WitchDoctor.Jinx,
                    Runes.WitchDoctor.AngryChicken,
                    Runes.WitchDoctor.ToadOfHugeness,
                    Runes.WitchDoctor.UnstableForm,
                }
            };

            /// <summary>
            /// Cost: 175 Mana Cause acid to rain down, dealing an initial 300% weapon damage as Poison, followed by 360% weapon damage as Poison over 3 seconds to enemies who remain in the area. 
            /// </summary>
            public static Skill AcidCloud = new Skill
            {
                Index = 17,
                Name = "Acid Cloud",
                SNOPower = SNOPower.Witchdoctor_AcidCloud,
                RequiredLevel = 22,
                Description = " Cost: 175 Mana Cause acid to rain down, dealing an initial 300% weapon damage as Poison, followed by 360% weapon damage as Poison over 3 seconds to enemies who remain in the area. ",
                Tooltip = "skill/witch-doctor/acid-cloud",
                Category = SpellCategory.Decay,
                IsPrimary = true,
                Class = ActorClass.Witchdoctor,
                Duration = TimeSpan.FromSeconds(3),
                Cost = 175,
                Cooldown = TimeSpan.Zero,
                Element = Element.Poison,
                Resource = Resource.Mana,
                Runes = new List<Rune>
                {
                    Runes.WitchDoctor.None,
                    Runes.WitchDoctor.AcidRain,
                    Runes.WitchDoctor.LobBlobBomb,
                    Runes.WitchDoctor.SlowBurn,
                    Runes.WitchDoctor.KissOfDeath,
                    Runes.WitchDoctor.CorpseBomb,
                }
            };

            /// <summary>
            /// Cooldown: 60 seconds Incite paranoia in enemies, confusing them and causing some to fight for you for 12 seconds. 
            /// </summary>
            public static Skill MassConfusion = new Skill
            {
                Index = 18,
                Name = "Mass Confusion",
                SNOPower = SNOPower.Witchdoctor_MassConfusion,
                RequiredLevel = 22,
                Description = " Cooldown: 60 seconds Incite paranoia in enemies, confusing them and causing some to fight for you for 12 seconds. ",
                Tooltip = "skill/witch-doctor/mass-confusion",
                Category = SpellCategory.Terror,
                IsPrimary = true,
                Class = ActorClass.Witchdoctor,
                Duration = TimeSpan.FromSeconds(12),
                Cost = 0,
                Cooldown = TimeSpan.FromSeconds(60),
                Element = Element.Physical,
                Resource = Resource.None,
                Runes = new List<Rune>
                {
                    Runes.WitchDoctor.None,
                    Runes.WitchDoctor.UnstableRealm,
                    Runes.WitchDoctor.Devolution,
                    Runes.WitchDoctor.MassHysteria,
                    Runes.WitchDoctor.Paranoia,
                    Runes.WitchDoctor.MassHallucination,
                }
            };

            /// <summary>
            /// Cooldown: 120 seconds Conjure a Fetish that begins a ritual dance that increases the attack speed and movement speed of all nearby allies by 20% for 20 seconds. 
            /// </summary>
            public static Skill BigBadVoodoo = new Skill
            {
                Index = 19,
                Name = "Big Bad Voodoo",
                SNOPower = SNOPower.Witchdoctor_BigBadVoodoo,
                RequiredLevel = 25,
                Description = " Cooldown: 120 seconds Conjure a Fetish that begins a ritual dance that increases the attack speed and movement speed of all nearby allies by 20% for 20 seconds. ",
                Tooltip = "skill/witch-doctor/big-bad-voodoo",
                Category = SpellCategory.Voodoo,
                IsPrimary = true,
                Class = ActorClass.Witchdoctor,
                Duration = TimeSpan.FromSeconds(20),
                Cost = 0,
                Cooldown = TimeSpan.FromSeconds(120),
                Element = Element.Physical,
                Resource = Resource.None,
                Runes = new List<Rune>
                {
                    Runes.WitchDoctor.None,
                    Runes.WitchDoctor.JungleDrums,
                    Runes.WitchDoctor.RainDance,
                    Runes.WitchDoctor.SlamDance,
                    Runes.WitchDoctor.GhostTrance,
                    Runes.WitchDoctor.BoogieMan,
                }
            };

            /// <summary>
            /// Cooldown: 8 seconds Raise a line of zombies 28 yards wide from the ground that attacks nearby enemies for 200% weapon damage as Physical over 4 seconds. 
            /// </summary>
            public static Skill WallOfZombies = new Skill
            {
                Index = 20,
                Name = "Wall of Zombies",
                SNOPower = SNOPower.Witchdoctor_WallOfZombies,
                RequiredLevel = 28,
                Description = " Cooldown: 8 seconds Raise a line of zombies 28 yards wide from the ground that attacks nearby enemies for 200% weapon damage as Physical over 4 seconds. ",
                Tooltip = "skill/witch-doctor/wall-of-zombies",
                Category = SpellCategory.Decay,
                IsPrimary = true,
                Class = ActorClass.Witchdoctor,
                Duration = TimeSpan.FromSeconds(4),
                Cost = 0,
                Cooldown = TimeSpan.FromSeconds(8),
                Element = Element.Physical,
                Resource = Resource.None,
                Runes = new List<Rune>
                {
                    Runes.WitchDoctor.None,
                    Runes.WitchDoctor.Barricade,
                    Runes.WitchDoctor.UnrelentingGrip,
                    Runes.WitchDoctor.Creepers,
                    Runes.WitchDoctor.WreckingCrew,
                    Runes.WitchDoctor.OffensiveLine,
                }
            };

            /// <summary>
            /// Cooldown: 120 seconds Summon an army of 5 dagger-wielding Fetishes to fight by your side for 20 seconds. The Fetishes attack for 180% of your weapon damage as Physical. 
            /// </summary>
            public static Skill FetishArmy = new Skill
            {
                Index = 21,
                Name = "Fetish Army",
                SNOPower = SNOPower.Witchdoctor_FetishArmy,
                RequiredLevel = 30,
                Description = " Cooldown: 120 seconds Summon an army of 5 dagger-wielding Fetishes to fight by your side for 20 seconds. The Fetishes attack for 180% of your weapon damage as Physical. ",
                Tooltip = "skill/witch-doctor/fetish-army",
                Category = SpellCategory.Voodoo,
                IsPrimary = true,
                Class = ActorClass.Witchdoctor,
                Duration = TimeSpan.FromSeconds(20),
                Cost = 0,
                Cooldown = TimeSpan.FromSeconds(120),
                Element = Element.Physical,
                Resource = Resource.None,
                Runes = new List<Rune>
                {
                    Runes.WitchDoctor.None,
                    Runes.WitchDoctor.FetishAmbush,
                    Runes.WitchDoctor.DevotedFollowing,
                    Runes.WitchDoctor.LegionOfDaggers,
                    Runes.WitchDoctor.TikiTorchers,
                    Runes.WitchDoctor.HeadHunters,
                }
            };

            /// <summary>
            /// Cost: 250 Mana Cooldown: 8 seconds Summons a pool of deadly piranhas that deals 400% weapon damage as Poison over 8 seconds. Affected enemies will also take 15% increased damage. 
            /// </summary>
            public static Skill Piranhas = new Skill
            {
                Index = 22,
                Name = "Piranhas",
                SNOPower = SNOPower.Witchdoctor_Piranhas,
                RequiredLevel = 61,
                Description = " Cost: 250 Mana Cooldown: 8 seconds Summons a pool of deadly piranhas that deals 400% weapon damage as Poison over 8 seconds. Affected enemies will also take 15% increased damage. ",
                Tooltip = "skill/witch-doctor/piranhas",
                Category = SpellCategory.Decay,
                IsPrimary = true,
                Class = ActorClass.Witchdoctor,
                Duration = TimeSpan.FromSeconds(8),
                Cost = 250,
                Cooldown = TimeSpan.FromSeconds(8),
                Element = Element.Poison,
                Resource = Resource.Mana,
                Runes = new List<Rune>
                {
                    Runes.WitchDoctor.None,
                    Runes.WitchDoctor.Bogadile,
                    Runes.WitchDoctor.ZombiePiranhas,
                    Runes.WitchDoctor.Piranhado,
                    Runes.WitchDoctor.WaveOfMutilation,
                    Runes.WitchDoctor.FrozenPiranhas,
                }
            };
        }

        public class Barbarian : FieldCollection<Barbarian, Skill>
        {
            /// <summary>
            /// Generate: 6 Fury per attack Brutally smash an enemy for 215% weapon damage. 
            /// </summary>
            public static Skill Bash = new Skill
            {
                Index = 0,
                Name = "Bash",
                SNOPower = SNOPower.Barbarian_Bash,
                RequiredLevel = 1,
                Description = " Generate: 6 Fury per attack Brutally smash an enemy for 215% weapon damage. ",
                Tooltip = "skill/barbarian/bash",
                Category = SpellCategory.Primary,
                IsPrimary = true,
                Class = ActorClass.Barbarian,
                Duration = TimeSpan.Zero,
                Cost = 0,
                Cooldown = TimeSpan.Zero,
                Element = Element.Physical,
                Resource = Resource.Fury,
                Runes = new List<Rune>
                {
                    Runes.Barbarian.None,
                    Runes.Barbarian.Clobber,
                    Runes.Barbarian.Onslaught,
                    Runes.Barbarian.Punish,
                    Runes.Barbarian.Instigation,
                    Runes.Barbarian.Pulverize,
                }
            };

            /// <summary>
            /// Cost: 20 Fury Call forth a massive hammer to smash enemies directly in front of you for 535% weapon damage. Hammer of the Ancients has a 1% increased Critical Hit Chance for every 5 Fury that you have. 
            /// </summary>
            public static Skill HammerOfTheAncients = new Skill
            {
                Index = 1,
                Name = "Hammer of the Ancients",
                SNOPower = SNOPower.Barbarian_HammerOfTheAncients,
                RequiredLevel = 2,
                Description = " Cost: 20 Fury Call forth a massive hammer to smash enemies directly in front of you for 535% weapon damage. Hammer of the Ancients has a 1% increased Critical Hit Chance for every 5 Fury that you have. ",
                Tooltip = "skill/barbarian/hammer-of-the-ancients",
                Category = SpellCategory.Secondary,
                IsPrimary = true,
                Class = ActorClass.Barbarian,
                Duration = TimeSpan.Zero,
                Cost = 20,
                Cooldown = TimeSpan.Zero,
                Element = Element.Physical,
                Resource = Resource.Fury,
                Runes = new List<Rune>
                {
                    Runes.Barbarian.None,
                    Runes.Barbarian.RollingThunder,
                    Runes.Barbarian.Smash,
                    Runes.Barbarian.TheDevilsAnvil,
                    Runes.Barbarian.Thunderstrike,
                    Runes.Barbarian.Birthright,
                }
            };

            /// <summary>
            /// Generate: 5 Fury per attack Swing your weapon in a wide arc to deal 150% weapon damage to all enemies caught in the swing. 
            /// </summary>
            public static Skill Cleave = new Skill
            {
                Index = 2,
                Name = "Cleave",
                SNOPower = SNOPower.Barbarian_Cleave,
                RequiredLevel = 3,
                Description = " Generate: 5 Fury per attack Swing your weapon in a wide arc to deal 150% weapon damage to all enemies caught in the swing. ",
                Tooltip = "skill/barbarian/cleave",
                Category = SpellCategory.Primary,
                IsPrimary = true,
                Class = ActorClass.Barbarian,
                Duration = TimeSpan.Zero,
                Cost = 0,
                Cooldown = TimeSpan.Zero,
                Element = Element.Physical,
                Resource = Resource.Fury,
                Runes = new List<Rune>
                {
                    Runes.Barbarian.None,
                    Runes.Barbarian.Rupture,
                    Runes.Barbarian.ReapingSwing,
                    Runes.Barbarian.ScatteringBlast,
                    Runes.Barbarian.BroadSweep,
                    Runes.Barbarian.GatheringStorm,
                }
            };

            /// <summary>
            /// Generate: 15 Fury Cooldown: 12 seconds Smash the ground, stunning all enemies within 14 yards for 4 seconds. 
            /// </summary>
            public static Skill GroundStomp = new Skill
            {
                Index = 3,
                Name = "Ground Stomp",
                SNOPower = SNOPower.Barbarian_GroundStomp,
                RequiredLevel = 4,
                Description = " Generate: 15 Fury Cooldown: 12 seconds Smash the ground, stunning all enemies within 14 yards for 4 seconds. ",
                Tooltip = "skill/barbarian/ground-stomp",
                Category = SpellCategory.Defensive,
                IsPrimary = true,
                Class = ActorClass.Barbarian,
                Duration = TimeSpan.FromSeconds(4),
                Cost = 0,
                Cooldown = TimeSpan.FromSeconds(12),
                Element = Element.Physical,
                Resource = Resource.Fury,
                Runes = new List<Rune>
                {
                    Runes.Barbarian.None,
                    Runes.Barbarian.DeafeningCrash,
                    Runes.Barbarian.WrenchingSmash,
                    Runes.Barbarian.TremblingStomp,
                    Runes.Barbarian.FootOfTheMountain,
                    Runes.Barbarian.JarringSlam,
                }
            };

            /// <summary>
            /// Cost: 20 Fury A sweeping strike causes all enemies within 12 yards to Bleed for 1000% weapon damage as Physical over 5 seconds. 
            /// </summary>
            public static Skill Rend = new Skill
            {
                Index = 4,
                Name = "Rend",
                SNOPower = SNOPower.Barbarian_Rend,
                RequiredLevel = 5,
                Description = " Cost: 20 Fury A sweeping strike causes all enemies within 12 yards to Bleed for 1000% weapon damage as Physical over 5 seconds. ",
                Tooltip = "skill/barbarian/rend",
                Category = SpellCategory.Secondary,
                IsPrimary = true,
                Class = ActorClass.Barbarian,
                Duration = TimeSpan.FromSeconds(5),
                Cost = 20,
                Cooldown = TimeSpan.Zero,
                Element = Element.Physical,
                Resource = Resource.Fury,
                Runes = new List<Rune>
                {
                    Runes.Barbarian.None,
                    Runes.Barbarian.Ravage,
                    Runes.Barbarian.BloodLust,
                    Runes.Barbarian.Lacerate,
                    Runes.Barbarian.Mutilate,
                    Runes.Barbarian.Bloodbath,
                }
            };

            /// <summary>
            /// Generate: 15 Fury Cooldown: 10 seconds Leap into the air, dealing 180% weapon damage to all enemies within 8 yards of your destination and slowing their movement speed by 60% for 3 seconds. 
            /// </summary>
            public static Skill Leap = new Skill
            {
                Index = 5,
                Name = "Leap",
                SNOPower = SNOPower.Barbarian_Leap,
                RequiredLevel = 8,
                Description = " Generate: 15 Fury Cooldown: 10 seconds Leap into the air, dealing 180% weapon damage to all enemies within 8 yards of your destination and slowing their movement speed by 60% for 3 seconds. ",
                Tooltip = "skill/barbarian/leap",
                Category = SpellCategory.Defensive,
                IsPrimary = true,
                Class = ActorClass.Barbarian,
                Duration = TimeSpan.FromSeconds(3),
                Cost = 0,
                Cooldown = TimeSpan.FromSeconds(10),
                Element = Element.Physical,
                Resource = Resource.Fury,
                Runes = new List<Rune>
                {
                    Runes.Barbarian.None,
                    Runes.Barbarian.IronImpact,
                    Runes.Barbarian.Launch,
                    Runes.Barbarian.TopplingImpact,
                    Runes.Barbarian.CallOfArreat,
                    Runes.Barbarian.DeathFromAbove,
                }
            };

            /// <summary>
            /// Cooldown: 12 seconds Deal 380% weapon damage to all enemies within 9 yards. Critical Hits have a chance to reduce the cooldown of Overpower by 1 second. 
            /// </summary>
            public static Skill Overpower = new Skill
            {
                Index = 6,
                Name = "Overpower",
                SNOPower = SNOPower.Barbarian_Overpower,
                RequiredLevel = 9,
                Description = " Cooldown: 12 seconds Deal 380% weapon damage to all enemies within 9 yards. Critical Hits have a chance to reduce the cooldown of Overpower by 1 second. ",
                Tooltip = "skill/barbarian/overpower",
                Category = SpellCategory.Might,
                IsPrimary = false,
                Class = ActorClass.Barbarian,
                Duration = TimeSpan.Zero,
                Cost = 0,
                Cooldown = TimeSpan.FromSeconds(12),
                Element = Element.Physical,
                Resource = Resource.None,
                Runes = new List<Rune>
                {
                    Runes.Barbarian.None,
                    Runes.Barbarian.StormOfSteel,
                    Runes.Barbarian.KillingSpree,
                    Runes.Barbarian.CrushingAdvance,
                    Runes.Barbarian.Momentum,
                    Runes.Barbarian.Revel,
                }
            };

            /// <summary>
            /// Generate: 4 Fury per attack Swing for 155% weapon damage. Frenzy's attack speed increases by 15% for 4 seconds with each swing. This effect stacks up to 5 times. 
            /// </summary>
            public static Skill Frenzy = new Skill
            {
                Index = 7,
                Name = "Frenzy",
                SNOPower = SNOPower.Barbarian_Frenzy,
                RequiredLevel = 11,
                Description = " Generate: 4 Fury per attack Swing for 155% weapon damage. Frenzy's attack speed increases by 15% for 4 seconds with each swing. This effect stacks up to 5 times. ",
                Tooltip = "skill/barbarian/frenzy",
                Category = SpellCategory.Primary,
                IsPrimary = true,
                Class = ActorClass.Barbarian,
                Duration = TimeSpan.FromSeconds(4),
                Cost = 0,
                Cooldown = TimeSpan.Zero,
                Element = Element.Physical,
                Resource = Resource.Fury,
                Runes = new List<Rune>
                {
                    Runes.Barbarian.None,
                    Runes.Barbarian.Sidearm,
                    Runes.Barbarian.Berserk,
                    Runes.Barbarian.Vanguard,
                    Runes.Barbarian.Smite,
                    Runes.Barbarian.Maniac,
                }
            };

            /// <summary>
            /// Cost: 30 Fury Slam the ground and cause a wave of destruction that deals 620% weapon damage to enemies up to 45 yards in front of you. 
            /// </summary>
            public static Skill SeismicSlam = new Skill
            {
                Index = 8,
                Name = "Seismic Slam",
                SNOPower = SNOPower.Barbarian_SeismicSlam,
                RequiredLevel = 12,
                Description = " Cost: 30 Fury Slam the ground and cause a wave of destruction that deals 620% weapon damage to enemies up to 45 yards in front of you. ",
                Tooltip = "skill/barbarian/seismic-slam",
                Category = SpellCategory.Secondary,
                IsPrimary = true,
                Class = ActorClass.Barbarian,
                Duration = TimeSpan.Zero,
                Cost = 30,
                Cooldown = TimeSpan.Zero,
                Element = Element.Physical,
                Resource = Resource.Fury,
                Runes = new List<Rune>
                {
                    Runes.Barbarian.None,
                    Runes.Barbarian.Stagger,
                    Runes.Barbarian.ShatteredGround,
                    Runes.Barbarian.Rumble,
                    Runes.Barbarian.StrengthFromEarth,
                    Runes.Barbarian.Permafrost,
                }
            };

            /// <summary>
            /// Cost: 1 charge Deal 220% weapon damage to all nearby enemies. You heal 2% of your maximum Life for each enemy hit. Revenge has a 15% chance to gain a charge each time you are hit. Maximum 2 charges. 
            /// </summary>
            public static Skill Revenge = new Skill
            {
                Index = 9,
                Name = "Revenge",
                SNOPower = SNOPower.Barbarian_Revenge,
                RequiredLevel = 13,
                Description = " Cost: 1 charge Deal 220% weapon damage to all nearby enemies. You heal 2% of your maximum Life for each enemy hit. Revenge has a 15% chance to gain a charge each time you are hit. Maximum 2 charges. ",
                Tooltip = "skill/barbarian/revenge",
                Category = SpellCategory.Might,
                IsPrimary = false,
                Class = ActorClass.Barbarian,
                Duration = TimeSpan.Zero,
                Cost = 1,
                Cooldown = TimeSpan.Zero,
                Element = Element.Physical,
                Resource = Resource.None,
                Runes = new List<Rune>
                {
                    Runes.Barbarian.None,
                    Runes.Barbarian.BloodLaw,
                    Runes.Barbarian.BestServedCold,
                    Runes.Barbarian.Retribution,
                    Runes.Barbarian.Grudge,
                    Runes.Barbarian.Provocation,
                }
            };

            /// <summary>
            /// Generate: 15 Fury Cooldown: 10 seconds Shout with great ferocity, reducing damage done by enemies within 25 yards by 20% for 15 seconds. 
            /// </summary>
            public static Skill ThreateningShout = new Skill
            {
                Index = 10,
                Name = "Threatening Shout",
                SNOPower = SNOPower.Barbarian_ThreateningShout,
                RequiredLevel = 14,
                Description = " Generate: 15 Fury Cooldown: 10 seconds Shout with great ferocity, reducing damage done by enemies within 25 yards by 20% for 15 seconds. ",
                Tooltip = "skill/barbarian/threatening-shout",
                Category = SpellCategory.Tactics,
                IsPrimary = false,
                Class = ActorClass.Barbarian,
                Duration = TimeSpan.FromSeconds(15),
                Cost = 0,
                Cooldown = TimeSpan.FromSeconds(10),
                Element = Element.Physical,
                Resource = Resource.Fury,
                Runes = new List<Rune>
                {
                    Runes.Barbarian.None,
                    Runes.Barbarian.Intimidate,
                    Runes.Barbarian.Falter,
                    Runes.Barbarian.GrimHarvest,
                    Runes.Barbarian.Demoralize,
                    Runes.Barbarian.Terrify,
                }
            };

            /// <summary>
            /// Cost: 20 Fury Increase movement speed by 30% for 3 seconds. 
            /// </summary>
            public static Skill Sprint = new Skill
            {
                Index = 11,
                Name = "Sprint",
                SNOPower = SNOPower.Barbarian_Sprint,
                RequiredLevel = 16,
                Description = " Cost: 20 Fury Increase movement speed by 30% for 3 seconds. ",
                Tooltip = "skill/barbarian/sprint",
                Category = SpellCategory.Defensive,
                IsPrimary = false,
                Class = ActorClass.Barbarian,
                Duration = TimeSpan.FromSeconds(3),
                Cost = 20,
                Cooldown = TimeSpan.Zero,
                Element = Element.Physical,
                Resource = Resource.Fury,
                Runes = new List<Rune>
                {
                    Runes.Barbarian.None,
                    Runes.Barbarian.Rush,
                    Runes.Barbarian.RunLikeTheWind,
                    Runes.Barbarian.Marathon,
                    Runes.Barbarian.Gangway,
                    Runes.Barbarian.ForcedMarch,
                }
            };

            /// <summary>
            /// Generate: 6 Fury per attack Hurl a throwing weapon at an enemy dealing 185% weapon damage. 
            /// </summary>
            public static Skill WeaponThrow = new Skill
            {
                Index = 12,
                Name = "Weapon Throw",
                SNOPower = SNOPower.X1_Barbarian_WeaponThrow,
                RequiredLevel = 17,
                Description = " Generate: 6 Fury per attack Hurl a throwing weapon at an enemy dealing 185% weapon damage. ",
                Tooltip = "skill/barbarian/weapon-throw",
                Category = SpellCategory.Primary,
                IsPrimary = true,
                Class = ActorClass.Barbarian,
                Duration = TimeSpan.Zero,
                Cost = 0,
                Cooldown = TimeSpan.Zero,
                Element = Element.Physical,
                Resource = Resource.Fury,
                Runes = new List<Rune>
                {
                    Runes.Barbarian.None,
                    Runes.Barbarian.MightyThrow,
                    Runes.Barbarian.Ricochet,
                    Runes.Barbarian.ThrowingHammer,
                    Runes.Barbarian.Stupefy,
                    Runes.Barbarian.BalancedWeapon,
                }
            };

            /// <summary>
            /// Cost: 50 Fury Cooldown: 60 seconds Shake the ground violently, dealing 2600% weapon damage as Fire over 8 seconds to all enemies within 18 yards. 
            /// </summary>
            public static Skill Earthquake = new Skill
            {
                Index = 13,
                Name = "Earthquake",
                SNOPower = SNOPower.Barbarian_Earthquake,
                RequiredLevel = 19,
                Description = " Cost: 50 Fury Cooldown: 60 seconds Shake the ground violently, dealing 2600% weapon damage as Fire over 8 seconds to all enemies within 18 yards. ",
                Tooltip = "skill/barbarian/earthquake",
                Category = SpellCategory.Rage,
                IsPrimary = true,
                Class = ActorClass.Barbarian,
                Duration = TimeSpan.FromSeconds(8),
                Cost = 50,
                Cooldown = TimeSpan.FromSeconds(60),
                Element = Element.Fire,
                Resource = Resource.Fury,
                Runes = new List<Rune>
                {
                    Runes.Barbarian.None,
                    Runes.Barbarian.GiantsStride,
                    Runes.Barbarian.ChillingEarth,
                    Runes.Barbarian.TheMountainsCall,
                    Runes.Barbarian.MoltenFury,
                    Runes.Barbarian.Cavein,
                }
            };

            /// <summary>
            /// Cost: 10 Fury Deliver multiple attacks to everything in your path for 275% weapon damage. While whirlwinding, you move at 75% movement speed. 
            /// </summary>
            public static Skill Whirlwind = new Skill
            {
                Index = 14,
                Name = "Whirlwind",
                SNOPower = SNOPower.Barbarian_Whirlwind,
                RequiredLevel = 20,
                Description = " Cost: 10 Fury Deliver multiple attacks to everything in your path for 275% weapon damage. While whirlwinding, you move at 75% movement speed. ",
                Tooltip = "skill/barbarian/whirlwind",
                Category = SpellCategory.Secondary,
                IsPrimary = true,
                Class = ActorClass.Barbarian,
                Duration = TimeSpan.Zero,
                Cost = 10,
                Cooldown = TimeSpan.Zero,
                Element = Element.Physical,
                Resource = Resource.Fury,
                Runes = new List<Rune>
                {
                    Runes.Barbarian.None,
                    Runes.Barbarian.DustDevils,
                    Runes.Barbarian.Hurricane,
                    Runes.Barbarian.BloodFunnel,
                    Runes.Barbarian.WindShear,
                    Runes.Barbarian.VolcanicEruption,
                }
            };

            /// <summary>
            /// Generate: 15 Fury Cooldown: 10 seconds Rush forward knocking back and dealing 360% weapon damage to enemies along your path. 
            /// </summary>
            public static Skill FuriousCharge = new Skill
            {
                Index = 15,
                Name = "Furious Charge",
                SNOPower = SNOPower.Barbarian_FuriousCharge,
                RequiredLevel = 21,
                Description = " Generate: 15 Fury Cooldown: 10 seconds Rush forward knocking back and dealing 360% weapon damage to enemies along your path. ",
                Tooltip = "skill/barbarian/furious-charge",
                Category = SpellCategory.Might,
                IsPrimary = true,
                Class = ActorClass.Barbarian,
                Duration = TimeSpan.Zero,
                Cost = 0,
                Cooldown = TimeSpan.FromSeconds(10),
                Element = Element.Physical,
                Resource = Resource.Fury,
                Runes = new List<Rune>
                {
                    Runes.Barbarian.None,
                    Runes.Barbarian.BatteringRam,
                    Runes.Barbarian.MercilessAssault,
                    Runes.Barbarian.Stamina,
                    Runes.Barbarian.BullRush,
                    Runes.Barbarian.Dreadnought,
                }
            };

            /// <summary>
            /// Cooldown: 30 seconds Reduce all damage taken by 50% for 5 seconds. 
            /// </summary>
            public static Skill IgnorePain = new Skill
            {
                Index = 16,
                Name = "Ignore Pain",
                SNOPower = SNOPower.Barbarian_IgnorePain,
                RequiredLevel = 22,
                Description = " Cooldown: 30 seconds Reduce all damage taken by 50% for 5 seconds. ",
                Tooltip = "skill/barbarian/ignore-pain",
                Category = SpellCategory.Defensive,
                IsPrimary = false,
                Class = ActorClass.Barbarian,
                Duration = TimeSpan.FromSeconds(5),
                Cost = 0,
                Cooldown = TimeSpan.FromSeconds(30),
                Element = Element.Physical,
                Resource = Resource.None,
                Runes = new List<Rune>
                {
                    Runes.Barbarian.None,
                    Runes.Barbarian.Bravado,
                    Runes.Barbarian.IronHide,
                    Runes.Barbarian.IgnoranceIsBliss,
                    Runes.Barbarian.MobRule,
                    Runes.Barbarian.ContemptForWeakness,
                }
            };

            /// <summary>
            /// Cost: 20 Fury Enter a rage which increases your damage by 10% and Critical Hit Chance by 3%. Lasts 120 seconds. 
            /// </summary>
            public static Skill BattleRage = new Skill
            {
                Index = 17,
                Name = "Battle Rage",
                SNOPower = SNOPower.Barbarian_BattleRage,
                RequiredLevel = 22,
                Description = " Cost: 20 Fury Enter a rage which increases your damage by 10% and Critical Hit Chance by 3%. Lasts 120 seconds. ",
                Tooltip = "skill/barbarian/battle-rage",
                Category = SpellCategory.Tactics,
                IsPrimary = false,
                Class = ActorClass.Barbarian,
                Duration = TimeSpan.FromSeconds(120),
                Cost = 20,
                Cooldown = TimeSpan.Zero,
                Element = Element.Physical,
                Resource = Resource.Fury,
                Runes = new List<Rune>
                {
                    Runes.Barbarian.None,
                    Runes.Barbarian.MaraudersRage,
                    Runes.Barbarian.Ferocity,
                    Runes.Barbarian.SwordsToPloughshares,
                    Runes.Barbarian.IntoTheFray,
                    Runes.Barbarian.Bloodshed,
                }
            };

            /// <summary>
            /// Cooldown: 120 seconds Summon the ancient Barbarians Talic, Korlic, and Madawc to fight alongside you for 30 seconds. Each deals 180% weapon damage per swing in addition to bonus abilities. Talic wields a sword and shield and uses Whirlwind and Leap Attack. Korlic wields a massive polearm and uses Cleave and Furious Charge. Madawc dual-wields axes and uses Weapon Throw and Seismic Slam. 
            /// </summary>
            public static Skill CallOfTheAncients = new Skill
            {
                Index = 18,
                Name = "Call of the Ancients",
                SNOPower = SNOPower.Barbarian_CallOfTheAncients,
                RequiredLevel = 25,
                Description = " Cooldown: 120 seconds Summon the ancient Barbarians Talic, Korlic, and Madawc to fight alongside you for 30 seconds. Each deals 180% weapon damage per swing in addition to bonus abilities. Talic wields a sword and shield and uses Whirlwind and Leap Attack. Korlic wields a massive polearm and uses Cleave and Furious Charge. Madawc dual-wields axes and uses Weapon Throw and Seismic Slam. ",
                Tooltip = "skill/barbarian/call-of-the-ancients",
                Category = SpellCategory.Rage,
                IsPrimary = true,
                Class = ActorClass.Barbarian,
                Duration = TimeSpan.FromSeconds(30),
                Cost = 0,
                Cooldown = TimeSpan.FromSeconds(120),
                Element = Element.Physical,
                Resource = Resource.None,
                Runes = new List<Rune>
                {
                    Runes.Barbarian.None,
                    Runes.Barbarian.TheCouncilRises,
                    Runes.Barbarian.DutyToTheClan,
                    Runes.Barbarian.AncientsBlessing,
                    Runes.Barbarian.AncientsFury,
                    Runes.Barbarian.TogetherAsOne,
                }
            };

            /// <summary>
            /// Cost: 25 Fury Throw a spear that pierces enemies and deals 500% weapon damage. 
            /// </summary>
            public static Skill AncientSpear = new Skill
            {
                Index = 19,
                Name = "Ancient Spear",
                SNOPower = SNOPower.X1_Barbarian_AncientSpear,
                RequiredLevel = 26,
                Description = " Cost: 25 Fury Throw a spear that pierces enemies and deals 500% weapon damage. ",
                Tooltip = "skill/barbarian/ancient-spear",
                Category = SpellCategory.Secondary,
                IsPrimary = true,
                Class = ActorClass.Barbarian,
                Duration = TimeSpan.Zero,
                Cost = 25,
                Cooldown = TimeSpan.Zero,
                Element = Element.Physical,
                Resource = Resource.Fury,
                Runes = new List<Rune>
                {
                    Runes.Barbarian.None,
                    Runes.Barbarian.Ranseur,
                    Runes.Barbarian.Harpoon,
                    Runes.Barbarian.JaggedEdge,
                    Runes.Barbarian.BoulderToss,
                    Runes.Barbarian.RageFlip,
                }
            };

            /// <summary>
            /// Generate: 20 Fury Cooldown: 20 seconds Unleash a rallying cry to increase Armor for you and all allies within 100 yards by 20% for 60 seconds. 
            /// </summary>
            public static Skill WarCry = new Skill
            {
                Index = 20,
                Name = "War Cry",
                SNOPower = SNOPower.X1_Barbarian_WarCry_v2,
                RequiredLevel = 28,
                Description = " Generate: 20 Fury Cooldown: 20 seconds Unleash a rallying cry to increase Armor for you and all allies within 100 yards by 20% for 60 seconds. ",
                Tooltip = "skill/barbarian/war-cry",
                Category = SpellCategory.Tactics,
                IsPrimary = false,
                Class = ActorClass.Barbarian,
                Duration = TimeSpan.FromSeconds(60),
                Cost = 0,
                Cooldown = TimeSpan.FromSeconds(20),
                Element = Element.Physical,
                Resource = Resource.Fury,
                Runes = new List<Rune>
                {
                    Runes.Barbarian.None,
                    Runes.Barbarian.HardenedWrath,
                    Runes.Barbarian.Charge,
                    Runes.Barbarian.Invigorate,
                    Runes.Barbarian.VeteransWarning,
                    Runes.Barbarian.Impunity,
                }
            };

            /// <summary>
            /// Cooldown: 120 seconds Enter a berserker rage which raises several attributes for 20 seconds. Critical Hit Chance: 10% Attack Speed: 25% Dodge Chance: 20% Movement Speed: 20% 
            /// </summary>
            public static Skill WrathOfTheBerserker = new Skill
            {
                Index = 21,
                Name = "Wrath of the Berserker",
                SNOPower = SNOPower.Barbarian_WrathOfTheBerserker,
                RequiredLevel = 30,
                Description = " Cooldown: 120 seconds Enter a berserker rage which raises several attributes for 20 seconds. Critical Hit Chance: 10% Attack Speed: 25% Dodge Chance: 20% Movement Speed: 20% ",
                Tooltip = "skill/barbarian/wrath-of-the-berserker",
                Category = SpellCategory.Rage,
                IsPrimary = false,
                Class = ActorClass.Barbarian,
                Duration = TimeSpan.FromSeconds(20),
                Cost = 0,
                Cooldown = TimeSpan.FromSeconds(120),
                Element = Element.Physical,
                Resource = Resource.None,
                Runes = new List<Rune>
                {
                    Runes.Barbarian.None,
                    Runes.Barbarian.ArreatsWail,
                    Runes.Barbarian.Insanity,
                    Runes.Barbarian.Slaughter,
                    Runes.Barbarian.StridingGiant,
                    Runes.Barbarian.ThriveOnChaos,
                }
            };

            /// <summary>
            /// Cooldown: 30 seconds Cause a massive avalanche of rocks to fall on an area dealing 1600% weapon damage to all enemies caught in its path. Cooldown is reduced by 1 second for every 25 Fury you spend. 
            /// </summary>
            public static Skill Avalanche = new Skill
            {
                Index = 22,
                Name = "Avalanche",
                SNOPower = SNOPower.X1_Barbarian_Avalanche_v2,
                RequiredLevel = 61,
                Description = " Cooldown: 30 seconds Cause a massive avalanche of rocks to fall on an area dealing 1600% weapon damage to all enemies caught in its path. Cooldown is reduced by 1 second for every 25 Fury you spend. ",
                Tooltip = "skill/barbarian/avalanche",
                Category = SpellCategory.Might,
                IsPrimary = true,
                Class = ActorClass.Barbarian,
                Duration = TimeSpan.Zero,
                Cost = 0,
                Cooldown = TimeSpan.FromSeconds(30),
                Element = Element.Physical,
                Resource = Resource.Fury,
                Runes = new List<Rune>
                {
                    Runes.Barbarian.None,
                    Runes.Barbarian.Volcano,
                    Runes.Barbarian.Lahar,
                    Runes.Barbarian.SnowcappedMountain,
                    Runes.Barbarian.TectonicRift,
                    Runes.Barbarian.Glacier,
                }
            };
        }

        /// <summary>
        /// Fast lookup for a Skill by SNOPower
        /// </summary>
        public static Skill ById (SNOPower power)
        {
            if (!_allSkillBySnoPower.Any())
                _allSkillBySnoPower = _all.ToDictionary(s => s.SNOPower, s => s);
            Skill skill;
            var result = _allSkillBySnoPower.TryGetValue(power, out skill);
            return result ? skill : new Skill();
        }

        private static Dictionary<SNOPower, Skill> _allSkillBySnoPower = new Dictionary<SNOPower, Skill>();

        /// <summary>
        /// All SNOPowers
        /// </summary>        
        public static HashSet<SNOPower> AllIds
        {
            get { return _allSNOPowers ?? (_allSNOPowers = new HashSet<SNOPower>(All.Select(s => s.SNOPower))); }
        }

        private static HashSet<SNOPower> _allSNOPowers;

        /// <summary>
        /// All skills that are currently active
        /// </summary>
        public static List<Skill> Active
        {
            get
            {
                if (!_active.Any() || ShouldUpdateActiveSkills)
                    UpdateActiveSkills();

                return _active;
            }
        }

        private static List<Skill> _active = new List<Skill>();

        /// <summary>
        /// All skills that are currently active, as SNOPower
        /// </summary>
        public static HashSet<SNOPower> ActiveIds
        {
            get
            {
                if (!_activeIds.Any() || ShouldUpdateActiveSkills)
                    UpdateActiveSkills();

                return _activeIds;
            }
        }

        private static HashSet<SNOPower> _activeIds = new HashSet<SNOPower>();

        /// <summary>
        /// Refresh active skills collections with the latest data
        /// </summary>
        private static void UpdateActiveSkills()
        {
            _lastUpdatedActiveSkills = DateTime.UtcNow;
            _active = CurrentClass.Where(s => HotbarSkills.AssignedSNOPowers.Contains(s.SNOPower)).ToList();
            _activeIds = HotbarSkills.AssignedSNOPowers;
        }

        private static DateTime _lastUpdatedActiveSkills = DateTime.MinValue;

        /// <summary>
        /// Check time since last update of active skills
        /// </summary>
        private static bool ShouldUpdateActiveSkills
        {
            get { return DateTime.UtcNow.Subtract(_lastUpdatedActiveSkills) > TimeSpan.FromSeconds(3); }
        }

        /// <summary>
        /// All possible skills, as SNOPower
        /// </summary>        
        public static HashSet<SNOPower> CurrentClassIds
        {
            get { return new HashSet<SNOPower>(CurrentClass.Select(s => s.SNOPower)); }
        }

        /// <summary>
        /// All skills
        /// </summary>        
        public static List<Skill> All
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
        private static List<Skill> _all = new List<Skill>();

        /// <summary>
        /// All skills for the specified class
        /// </summary>
        public static List<Skill> ByActorClass (ActorClass Class)
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
            return new List<Skill>();
        }

        /// <summary>
        /// Skills for the current class
        /// </summary>
        public static IEnumerable<Skill> CurrentClass
        {
            get { return ZetaDia.Me.IsValid ? ByActorClass(ZetaDia.Me.ActorClass) : new List<Skill>(); }
        }

    }
}