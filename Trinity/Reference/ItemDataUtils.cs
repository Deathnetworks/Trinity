using System.Collections.Generic;
using System.Linq;
using Trinity.Helpers;
using Trinity.Objects;
using Trinity.Technicals;
using Trinity.UIComponents;
using Zeta.Game;
using Zeta.Game.Internals.Actors;

namespace Trinity.Reference
{
    class ItemDataUtils
    {

        public static StatType GetMainStatType(ACDItem item)
        {
            if (item.Stats.Strength > 0) return StatType.Strength;
            if (item.Stats.Intelligence > 0) return StatType.Intelligence;
            if (item.Stats.Dexterity > 0) return StatType.Dexterity;
            return StatType.Unknown;
        }

        public static int GetMainStatValue(ACDItem item)
        {
            if (item.Stats.Strength > 0) return (int)item.Stats.Strength;
            if (item.Stats.Intelligence > 0) return (int)item.Stats.Intelligence;
            if (item.Stats.Dexterity > 0) return (int)item.Stats.Dexterity;
            return 0;
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

        public static ItemStatRange GetItemStatRange(GItemType itemType, ItemProperty prop)
        {
            ItemStatRange statRange;
            
            if(ItemPropertyLimitsByItemType.TryGetValue(new KeyValuePair<GItemType, ItemProperty>(itemType,prop), out statRange))
                return statRange;
            
            if (ItemPropertyLimitsByItemType.TryGetValue(new KeyValuePair<GItemType, ItemProperty>(GItemType.Unknown, prop), out statRange))
                return statRange;
            
            return new ItemStatRange();
        }


        public static readonly Dictionary<KeyValuePair<GItemType, ItemProperty>, ItemStatRange> ItemPropertyLimitsByItemType = new Dictionary<KeyValuePair<GItemType, ItemProperty>, ItemStatRange>
        {

            // PrimaryStat by GItemType

            {new KeyValuePair<GItemType, ItemProperty>(GItemType.Ring, ItemProperty.PrimaryStat), 
                new ItemStatRange {AncientMax = 650, AncientMin = 550, Max = 500, Min = 416 }},

            {new KeyValuePair<GItemType, ItemProperty>(GItemType.Amulet, ItemProperty.PrimaryStat), 
                new ItemStatRange {AncientMax = 1000, AncientMin = 825, Max = 750, Min = 626 }},

            {new KeyValuePair<GItemType, ItemProperty>(GItemType.Helm, ItemProperty.PrimaryStat), 
                new ItemStatRange {AncientMax = 1000, AncientMin = 750, Max = 750, Min = 626 }},

            {new KeyValuePair<GItemType, ItemProperty>(GItemType.VoodooMask, ItemProperty.PrimaryStat), 
                new ItemStatRange {AncientMax = 1000, AncientMin = 750, Max = 750, Min = 626 }},

            {new KeyValuePair<GItemType, ItemProperty>(GItemType.WizardHat, ItemProperty.PrimaryStat), 
                new ItemStatRange {AncientMax = 1000, AncientMin = 750, Max = 750, Min = 626 }},

            {new KeyValuePair<GItemType, ItemProperty>(GItemType.SpiritStone, ItemProperty.PrimaryStat), 
                new ItemStatRange {AncientMax = 1000, AncientMin = 750, Max = 750, Min = 626 }},

            {new KeyValuePair<GItemType, ItemProperty>(GItemType.Gloves, ItemProperty.PrimaryStat), 
                new ItemStatRange {AncientMax = 1000, AncientMin = 825, Max = 750, Min = 626 }},

            {new KeyValuePair<GItemType, ItemProperty>(GItemType.Quiver, ItemProperty.PrimaryStat), 
                new ItemStatRange {AncientMax = 1000, AncientMin = 825, Max = 750, Min = 650 }},

            {new KeyValuePair<GItemType, ItemProperty>(GItemType.Orb, ItemProperty.PrimaryStat), 
                new ItemStatRange {AncientMax = 1000, AncientMin = 825, Max = 750, Min = 650 }},

            {new KeyValuePair<GItemType, ItemProperty>(GItemType.Mojo, ItemProperty.PrimaryStat), 
                new ItemStatRange {AncientMax = 1000, AncientMin = 825, Max = 750, Min = 650 }},

            {new KeyValuePair<GItemType, ItemProperty>(GItemType.Shoulder, ItemProperty.PrimaryStat), 
                new ItemStatRange {AncientMax = 650, AncientMin = 550, Max = 500, Min = 416 }},

            {new KeyValuePair<GItemType, ItemProperty>(GItemType.Chest, ItemProperty.PrimaryStat), 
                new ItemStatRange {AncientMax = 650, AncientMin = 550, Max = 500, Min = 416 }},

            {new KeyValuePair<GItemType, ItemProperty>(GItemType.Cloak, ItemProperty.PrimaryStat), 
                new ItemStatRange {AncientMax = 650, AncientMin = 550, Max = 500, Min = 416 }},

            {new KeyValuePair<GItemType, ItemProperty>(GItemType.Belt, ItemProperty.PrimaryStat), 
                new ItemStatRange {AncientMax = 650, AncientMin = 550, Max = 500, Min = 416 }},

            {new KeyValuePair<GItemType, ItemProperty>(GItemType.MightyBelt, ItemProperty.PrimaryStat), 
                new ItemStatRange {AncientMax = 650, AncientMin = 550, Max = 500, Min = 416 }},

            {new KeyValuePair<GItemType, ItemProperty>(GItemType.Bracer, ItemProperty.PrimaryStat), 
                new ItemStatRange {AncientMax = 650, AncientMin = 550, Max = 500, Min = 416 }},

            {new KeyValuePair<GItemType, ItemProperty>(GItemType.Legs, ItemProperty.PrimaryStat), 
                new ItemStatRange {AncientMax = 650, AncientMin = 550, Max = 500, Min = 416 }},

            {new KeyValuePair<GItemType, ItemProperty>(GItemType.Boots, ItemProperty.PrimaryStat), 
                new ItemStatRange {AncientMax = 650, AncientMin = 550, Max = 500, Min = 416 }},

            {new KeyValuePair<GItemType, ItemProperty>(GItemType.MightyWeapon, ItemProperty.PrimaryStat), 
                new ItemStatRange {AncientMax = 1000, AncientMin = 825, Max = 750, Min = 626 }},

            {new KeyValuePair<GItemType, ItemProperty>(GItemType.Wand, ItemProperty.PrimaryStat), 
                new ItemStatRange {AncientMax = 1000, AncientMin = 825, Max = 750, Min = 626 }},

            {new KeyValuePair<GItemType, ItemProperty>(GItemType.Flail, ItemProperty.PrimaryStat), 
                new ItemStatRange {AncientMax = 1000, AncientMin = 825, Max = 750, Min = 626 }},

            {new KeyValuePair<GItemType, ItemProperty>(GItemType.Axe, ItemProperty.PrimaryStat), 
                new ItemStatRange {AncientMax = 1000, AncientMin = 825, Max = 750, Min = 626 }},

            {new KeyValuePair<GItemType, ItemProperty>(GItemType.HandCrossbow, ItemProperty.PrimaryStat), 
                new ItemStatRange {AncientMax = 1000, AncientMin = 825, Max = 750, Min = 626 }},

            {new KeyValuePair<GItemType, ItemProperty>(GItemType.CeremonialKnife, ItemProperty.PrimaryStat), 
                new ItemStatRange {AncientMax = 1000, AncientMin = 825, Max = 750, Min = 626 }},

            {new KeyValuePair<GItemType, ItemProperty>(GItemType.FistWeapon, ItemProperty.PrimaryStat), 
                new ItemStatRange {AncientMax = 61000, AncientMin = 825, Max = 750, Min = 626 }},

            {new KeyValuePair<GItemType, ItemProperty>(GItemType.Mace, ItemProperty.PrimaryStat), 
                new ItemStatRange {AncientMax = 1000, AncientMin = 825, Max = 750, Min = 626 }},

            {new KeyValuePair<GItemType, ItemProperty>(GItemType.Sword, ItemProperty.PrimaryStat), 
                new ItemStatRange {AncientMax = 1000, AncientMin = 825, Max = 750, Min = 626 }},

            {new KeyValuePair<GItemType, ItemProperty>(GItemType.TwoHandDaibo, ItemProperty.PrimaryStat), 
                new ItemStatRange {AncientMax = 1465, AncientMin = 1237, Max = 1125, Min = 946 }},

            {new KeyValuePair<GItemType, ItemProperty>(GItemType.TwoHandCrossbow, ItemProperty.PrimaryStat), 
                new ItemStatRange {AncientMax = 1000, AncientMin = 825, Max = 500, Min = 416 }},

            {new KeyValuePair<GItemType, ItemProperty>(GItemType.TwoHandAxe, ItemProperty.PrimaryStat), 
                new ItemStatRange {AncientMax = 1465, AncientMin = 1237, Max = 1125, Min = 946 }},

            {new KeyValuePair<GItemType, ItemProperty>(GItemType.TwoHandBow, ItemProperty.PrimaryStat), 
                new ItemStatRange {AncientMax = 1000, AncientMin = 825, Max = 500, Min = 416 }},

            {new KeyValuePair<GItemType, ItemProperty>(GItemType.TwoHandFlail, ItemProperty.PrimaryStat), 
                new ItemStatRange {AncientMax = 1465, AncientMin = 1237, Max = 1125, Min = 946 }},

            {new KeyValuePair<GItemType, ItemProperty>(GItemType.TwoHandMace, ItemProperty.PrimaryStat), 
                new ItemStatRange {AncientMax = 1465, AncientMin = 1237, Max = 1125, Min = 946 }},

            {new KeyValuePair<GItemType, ItemProperty>(GItemType.TwoHandMighty, ItemProperty.PrimaryStat), 
                new ItemStatRange {AncientMax = 1465, AncientMin = 1237, Max = 1125, Min = 946 }},

            {new KeyValuePair<GItemType, ItemProperty>(GItemType.TwoHandPolearm, ItemProperty.PrimaryStat), 
                new ItemStatRange {AncientMax = 1465, AncientMin = 1237, Max = 1125, Min = 946 }},

            {new KeyValuePair<GItemType, ItemProperty>(GItemType.TwoHandStaff, ItemProperty.PrimaryStat), 
                new ItemStatRange {AncientMax = 1465, AncientMin = 1237, Max = 1125, Min = 946 }},

            {new KeyValuePair<GItemType, ItemProperty>(GItemType.TwoHandSword, ItemProperty.PrimaryStat), 
                new ItemStatRange {AncientMax = 1465, AncientMin = 1237, Max = 1125, Min = 946 }},

            {new KeyValuePair<GItemType, ItemProperty>(GItemType.Spear, ItemProperty.PrimaryStat), 
                new ItemStatRange {AncientMax = 1000, AncientMin = 825, Max = 750, Min = 626 }},

            {new KeyValuePair<GItemType, ItemProperty>(GItemType.Shield, ItemProperty.PrimaryStat), 
                new ItemStatRange {AncientMax = 650, AncientMin = 550, Max = 500, Min = 416 }},

            {new KeyValuePair<GItemType, ItemProperty>(GItemType.Dagger, ItemProperty.PrimaryStat), 
                new ItemStatRange {AncientMax = 1000, AncientMin = 825, Max = 750, Min = 626 }},

            {new KeyValuePair<GItemType, ItemProperty>(GItemType.CrusaderShield, ItemProperty.PrimaryStat), 
                new ItemStatRange {AncientMax = 650, AncientMin = 550, Max = 500, Min = 416 }},

            {new KeyValuePair<GItemType, ItemProperty>(GItemType.Unknown, ItemProperty.PrimaryStat), 
                new ItemStatRange { Max = 1000, Min = 0 }},


            // CriticalDamage by GItemType

            {new KeyValuePair<GItemType, ItemProperty>(GItemType.Ring, ItemProperty.CriticalHitDamage), new ItemStatRange { Max = 50, Min = 26 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.Amulet, ItemProperty.CriticalHitDamage), new ItemStatRange { Max = 100, Min = 51 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.Gloves, ItemProperty.CriticalHitDamage), new ItemStatRange { Max = 50, Min = 26 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.Belt, ItemProperty.CriticalHitDamage), new ItemStatRange { Max = 50, Min = 25 }}, // WitchingHour
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.Mace, ItemProperty.CriticalHitDamage), new ItemStatRange { Max = 500, Min = 416 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.Sword, ItemProperty.CriticalHitDamage), new ItemStatRange { Max = 500, Min = 416 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.TwoHandBow, ItemProperty.CriticalHitDamage), new ItemStatRange { Max = 35, Min = 31 }}, //UnboundBolt
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.Unknown, ItemProperty.CriticalHitDamage), new ItemStatRange { Max = 100, Min = 0 }},

            // CriticalChance by GItemType

            {new KeyValuePair<GItemType, ItemProperty>(GItemType.Ring, ItemProperty.CriticalHitChance), new ItemStatRange { Max = 6, Min = 4.5 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.Amulet, ItemProperty.CriticalHitChance), new ItemStatRange { Max = 10, Min = 8 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.Helm, ItemProperty.CriticalHitChance), new ItemStatRange { Max = 6, Min = 4.5 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.VoodooMask, ItemProperty.CriticalHitChance), new ItemStatRange { Max = 6, Min = 4.5 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.WizardHat, ItemProperty.CriticalHitChance), new ItemStatRange { Max = 6, Min = 4.5 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.SpiritStone, ItemProperty.CriticalHitChance), new ItemStatRange { Max = 6, Min = 4.5 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.Gloves, ItemProperty.CriticalHitChance), new ItemStatRange { Max = 10, Min = 8 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.Quiver, ItemProperty.CriticalHitChance), new ItemStatRange { Max = 10, Min = 8 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.Orb, ItemProperty.CriticalHitChance), new ItemStatRange { Max = 10, Min = 8 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.Mojo, ItemProperty.CriticalHitChance), new ItemStatRange { Max = 10, Min = 8 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.Bracer, ItemProperty.CriticalHitChance), new ItemStatRange { Max = 6, Min = 4.5 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.Shield, ItemProperty.CriticalHitChance), new ItemStatRange { Max = 10, Min = 8 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.CrusaderShield, ItemProperty.CriticalHitChance), new ItemStatRange { Max = 10, Min = 8 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.Unknown, ItemProperty.CriticalHitChance), new ItemStatRange { Max = 10, Min = 0 }},

            //{new KeyValuePair<GItemType, ItemProperty>(GItemType.Ring, ItemProperty.CritcalHitDamage), new ItemStatRange { Max = 50, Min = 26 }},
            //{new KeyValuePair<GItemType, ItemProperty>(GItemType.Amulet, ItemProperty.CritcalHitDamage), new ItemStatRange { Max = 100, Min = 51 }},
            //{new KeyValuePair<GItemType, ItemProperty>(GItemType.Helm, ItemProperty.CritcalHitDamage), new ItemStatRange { Max = 500, Min = 416 }},
            //{new KeyValuePair<GItemType, ItemProperty>(GItemType.VoodooMask, ItemProperty.CritcalHitDamage), new ItemStatRange { Max = 500, Min = 416 }},
            //{new KeyValuePair<GItemType, ItemProperty>(GItemType.WizardHat, ItemProperty.CritcalHitDamage), new ItemStatRange { Max = 500, Min = 416 }},
            //{new KeyValuePair<GItemType, ItemProperty>(GItemType.SpiritStone, ItemProperty.CritcalHitDamage), new ItemStatRange { Max = 500, Min = 416 }},
            //{new KeyValuePair<GItemType, ItemProperty>(GItemType.Gloves, ItemProperty.CritcalHitDamage), new ItemStatRange { Max = 50, Min = 26 }},
            //{new KeyValuePair<GItemType, ItemProperty>(GItemType.Quiver, ItemProperty.CritcalHitDamage), new ItemStatRange { Max = 10, Min = 8 }},
            //{new KeyValuePair<GItemType, ItemProperty>(GItemType.Orb, ItemProperty.CritcalHitDamage), new ItemStatRange { Max = 10, Min = 8 }},
            //{new KeyValuePair<GItemType, ItemProperty>(GItemType.Mojo, ItemProperty.CritcalHitDamage), new ItemStatRange { Max = 10, Min = 8 }},
            //{new KeyValuePair<GItemType, ItemProperty>(GItemType.Shoulder, ItemProperty.CritcalHitDamage), new ItemStatRange { Max = 500, Min = 416 }},
            //{new KeyValuePair<GItemType, ItemProperty>(GItemType.Chest, ItemProperty.CritcalHitDamage), new ItemStatRange { Max = 500, Min = 416 }},
            //{new KeyValuePair<GItemType, ItemProperty>(GItemType.Cloak, ItemProperty.CritcalHitDamage), new ItemStatRange { Max = 500, Min = 416 }},
            //{new KeyValuePair<GItemType, ItemProperty>(GItemType.Belt, ItemProperty.CritcalHitDamage), new ItemStatRange { Max = 500, Min = 416 }},
            //{new KeyValuePair<GItemType, ItemProperty>(GItemType.MightyBelt, ItemProperty.CritcalHitDamage), new ItemStatRange { Max = 500, Min = 416 }},
            //{new KeyValuePair<GItemType, ItemProperty>(GItemType.Bracer, ItemProperty.CritcalHitDamage), new ItemStatRange { Max = 500, Min = 416 }},
            //{new KeyValuePair<GItemType, ItemProperty>(GItemType.Legs, ItemProperty.CritcalHitDamage), new ItemStatRange { Max = 500, Min = 416 }},
            //{new KeyValuePair<GItemType, ItemProperty>(GItemType.Boots, ItemProperty.CritcalHitDamage), new ItemStatRange { Max = 500, Min = 416 }},
            //{new KeyValuePair<GItemType, ItemProperty>(GItemType.MightyWeapon, ItemProperty.CritcalHitDamage), new ItemStatRange { Max = 500, Min = 416 }},
            //{new KeyValuePair<GItemType, ItemProperty>(GItemType.Wand, ItemProperty.CritcalHitDamage), new ItemStatRange { Max = 500, Min = 416 }},
            //{new KeyValuePair<GItemType, ItemProperty>(GItemType.Flail, ItemProperty.CritcalHitDamage), new ItemStatRange { Max = 500, Min = 416 }},
            //{new KeyValuePair<GItemType, ItemProperty>(GItemType.Axe, ItemProperty.CritcalHitDamage), new ItemStatRange { Max = 500, Min = 416 }},
            //{new KeyValuePair<GItemType, ItemProperty>(GItemType.HandCrossbow, ItemProperty.CritcalHitDamage), new ItemStatRange { Max = 500, Min = 416 }},
            //{new KeyValuePair<GItemType, ItemProperty>(GItemType.CeremonialKnife, ItemProperty.CritcalHitDamage), new ItemStatRange { Max = 500, Min = 416 }},
            //{new KeyValuePair<GItemType, ItemProperty>(GItemType.FistWeapon, ItemProperty.CritcalHitDamage), new ItemStatRange { Max = 500, Min = 416 }},
            //{new KeyValuePair<GItemType, ItemProperty>(GItemType.Mace, ItemProperty.CritcalHitDamage), new ItemStatRange { Max = 500, Min = 416 }},
            //{new KeyValuePair<GItemType, ItemProperty>(GItemType.Sword, ItemProperty.CritcalHitDamage), new ItemStatRange { Max = 500, Min = 416 }},
            //{new KeyValuePair<GItemType, ItemProperty>(GItemType.TwoHandDaibo, ItemProperty.CritcalHitDamage), new ItemStatRange { Max = 500, Min = 416 }},
            //{new KeyValuePair<GItemType, ItemProperty>(GItemType.TwoHandCrossbow, ItemProperty.CritcalHitDamage), new ItemStatRange { Max = 500, Min = 416 }},
            //{new KeyValuePair<GItemType, ItemProperty>(GItemType.TwoHandAxe, ItemProperty.CritcalHitDamage), new ItemStatRange { Max = 500, Min = 416 }},
            //{new KeyValuePair<GItemType, ItemProperty>(GItemType.TwoHandBow, ItemProperty.CritcalHitDamage), new ItemStatRange { Max = 500, Min = 416 }},
            //{new KeyValuePair<GItemType, ItemProperty>(GItemType.TwoHandFlail, ItemProperty.CritcalHitDamage), new ItemStatRange { Max = 500, Min = 416 }},
            //{new KeyValuePair<GItemType, ItemProperty>(GItemType.TwoHandMace, ItemProperty.CritcalHitDamage), new ItemStatRange { Max = 500, Min = 416 }},
            //{new KeyValuePair<GItemType, ItemProperty>(GItemType.TwoHandMighty, ItemProperty.CritcalHitDamage), new ItemStatRange { Max = 500, Min = 416 }},
            //{new KeyValuePair<GItemType, ItemProperty>(GItemType.TwoHandPolearm, ItemProperty.CritcalHitDamage), new ItemStatRange { Max = 500, Min = 416 }},
            //{new KeyValuePair<GItemType, ItemProperty>(GItemType.TwoHandStaff, ItemProperty.CritcalHitDamage), new ItemStatRange { Max = 500, Min = 416 }},
            //{new KeyValuePair<GItemType, ItemProperty>(GItemType.TwoHandSword, ItemProperty.CritcalHitDamage), new ItemStatRange { Max = 500, Min = 416 }},
            //{new KeyValuePair<GItemType, ItemProperty>(GItemType.Spear, ItemProperty.CritcalHitDamage), new ItemStatRange { Max = 500, Min = 416 }},
            //{new KeyValuePair<GItemType, ItemProperty>(GItemType.Shield, ItemProperty.CritcalHitDamage), new ItemStatRange { Max = 500, Min = 416 }},
            //{new KeyValuePair<GItemType, ItemProperty>(GItemType.Dagger, ItemProperty.CritcalHitDamage), new ItemStatRange { Max = 500, Min = 416 }},
            //{new KeyValuePair<GItemType, ItemProperty>(GItemType.CrusaderShield, ItemProperty.CritcalHitDamage), new ItemStatRange { Max = 500, Min = 416 }},

    
        };

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
