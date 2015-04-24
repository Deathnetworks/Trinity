using System;
using System.Collections.Generic;
using System.Linq;
using Trinity.Helpers;
using Trinity.Items;
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

        public static int GetMinBaseDamage(ACDItem item)
        {
            var min = Math.Min(item.Stats.MinDamageElemental, item.Stats.MaxDamageElemental);
            return (min != 0) ? (int)min : (int)item.WeaponBaseMinPhysicalDamage();
        }

        public static int GetMaxBaseDamage(ACDItem item)
        {
            var max = Math.Max(item.Stats.MinDamageElemental, item.Stats.MaxDamageElemental);
            return (max != 0) ? (int)max : (int)item.WeaponBaseMaxPhysicalDamage();
        }

        internal static double GetAttackSpeed(ACDItem acdItem)
        {
            return Math.Round(Math.Max(acdItem.Stats.AttackSpeedPercent, acdItem.Stats.AttackSpeedPercentBonus), MidpointRounding.AwayFromZero);
        }

        public static double GetElementalDamage(ACDItem item, Element element)
        {
            switch (element)
            {
                case Element.Fire:
                    return item.Stats.FireSkillDamagePercentBonus;
                case Element.Cold:
                    return item.Stats.ColdSkillDamagePercentBonus;
                case Element.Lightning:
                    return item.Stats.LightningSkillDamagePercentBonus;
                case Element.Poison:
                    return item.Stats.PosionSkillDamagePercentBonus;
                case Element.Arcane:
                    return item.Stats.ArcaneSkillDamagePercentBonus;
                case Element.Holy:
                    return item.Stats.HolySkillDamagePercentBonus;
                case Element.Physical:
                    return item.Stats.PhysicalSkillDamagePercentBonus;
            }
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
            var itemType = TrinityItemManager.DetermineItemType(item);

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
                var kvp = new KeyValuePair<TinityItemType, ActorClass>(itemType, actorClass);

                foreach (var skill in SkillDamageByItemTypeAndClass[kvp])
                {
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

        public static List<Skill> GetSkillsForItemType(TinityItemType itemType, ActorClass actorClass = ActorClass.Invalid)
        {
            var result = new List<Skill>();
            if (actorClass != ActorClass.Invalid)
            {
                var kvp = new KeyValuePair<TinityItemType, ActorClass>(itemType, actorClass);
                result.AddRange(SkillDamageByItemTypeAndClass[kvp]);
            }
            else
            {
                var actorClasses = new List<ActorClass>
                {
                        ActorClass.Monk,
                        ActorClass.DemonHunter,
                        ActorClass.Witchdoctor,
                        ActorClass.Wizard,
                        ActorClass.Crusader,
                        ActorClass.Barbarian               
                };
                foreach (var ac in actorClasses)
                {
                    var kvp = new KeyValuePair<TinityItemType, ActorClass>(itemType, ac);
                    result.AddRange(SkillDamageByItemTypeAndClass[kvp]);
                }             
            }
            return result;
        }

        /// <summary>
        /// Returns an object with the Min and Max values for a particular property and item
        /// Eg. Fire Damage 15-20%
        /// </summary>
        public static ItemStatRange GetItemStatRange(Item item, ItemProperty prop)
        {
            ItemStatRange statRange;

            var result = new ItemStatRange();

            if(prop == ItemProperty.Ancient)
                return new ItemStatRange { Max = 1, Min = 0};

            if (ItemPropertyLimitsByItemType.TryGetValue(new KeyValuePair<TinityItemType, ItemProperty>(item.TinityItemType,prop), out statRange))
                result = statRange;

            if (SpecialItemsPropertyCases.TryGetValue(new Tuple<Item, ItemProperty>(item, prop), out statRange))
                result = statRange;

            return result;
        }

        /// <summary>
        /// Determine if an item can have a given property
        /// </summary>
        /// <param name="item"></param>
        /// <param name="prop"></param>
        /// <returns></returns>
        public static bool IsValidPropertyForItem(Item item, ItemProperty prop)
        {
            ItemStatRange statRange;

            if (prop == ItemProperty.Ancient)
                return true;

            if (ItemPropertyLimitsByItemType.TryGetValue(new KeyValuePair<TinityItemType, ItemProperty>(item.TinityItemType, prop), out statRange))
                return true;

            if (SpecialItemsPropertyCases.ContainsKey(new Tuple<Item, ItemProperty>(item, prop)))
                return true;

            return false;
        }

        /// <summary>
        /// Returns all the possible properties for a given item.
        /// </summary>
        public static List<ItemProperty> GetPropertiesForItem(Item item)
        {
            var props = ItemPropertyLimitsByItemType.Where(pair => pair.Key.Key == item.TinityItemType).Select(pair => pair.Key.Value).ToList();
            var specialProps = SpecialItemsPropertyCases.Where(pair => pair.Key.Item1 == item).Select(pair => pair.Key.Item2).ToList();
            props = props.Concat(specialProps).Distinct().ToList();
            props.Add(ItemProperty.Ancient);
            props.Sort();
            return props;
        }

        /// <summary>
        /// Get all the possible options for multi-value item properties. 
        /// For example Skill Damage for Quiver can be for the Sentry, Cluster Arrow, Multishot etc.
        /// </summary>
        public static List<object> GetItemPropertyVariants(ItemProperty prop, TinityItemType itemType)
        {
            var result = new List<object>();
            switch (prop)
            {
                case ItemProperty.SkillDamage:
                    var classRestriction = (Item.GetClassRestriction(itemType));
                    result = GetSkillsForItemType(itemType, classRestriction).Cast<object>().ToList();
                    break;

                case ItemProperty.ElementalDamage:
                    result = new List<object>
                    {
                        Element.Poison.ToEnumValue(),
                        Element.Holy.ToEnumValue(),
                        Element.Cold.ToEnumValue(),
                        Element.Arcane.ToEnumValue(),
                        Element.Fire.ToEnumValue(),
                        Element.Physical.ToEnumValue(),
                        Element.Lightning.ToEnumValue(),                       
                    };
                    break;
            }
            return result;
        }

        /// <summary>
        /// Items with unusual properties are listed here
        /// Determines if Property will be available in ItemList rules dropdown.
        /// </summary>
        public static readonly Dictionary<Tuple<Item,ItemProperty>, ItemStatRange> SpecialItemsPropertyCases = new Dictionary<Tuple<Item,ItemProperty>, ItemStatRange>
        {
            { new Tuple<Item, ItemProperty>(Legendary.HellcatWaistguard, ItemProperty.DamageAgainstElites), new ItemStatRange { Max = 6, Min = 3}},
            { new Tuple<Item, ItemProperty>(Legendary.HellcatWaistguard, ItemProperty.AttackSpeed), new ItemStatRange { Max = 7, Min = 5 }},
            { new Tuple<Item, ItemProperty>(Legendary.TheWitchingHour, ItemProperty.CriticalHitDamage), new ItemStatRange { Max = 50, Min = 26 }},
            { new Tuple<Item, ItemProperty>(Legendary.TheWitchingHour, ItemProperty.AttackSpeed), new ItemStatRange { Max = 7, Min = 5 }},
            { new Tuple<Item, ItemProperty>(Legendary.Magefist, ItemProperty.FireSkills), new ItemStatRange { Max = 20, Min = 15 }},
            { new Tuple<Item, ItemProperty>(Legendary.Cindercoat, ItemProperty.FireSkills), new ItemStatRange { Max = 20, Min = 15 }},
            { new Tuple<Item, ItemProperty>(Legendary.UnboundBolt, ItemProperty.CriticalHitDamage), new ItemStatRange { Max = 35, Min = 31 }},
            { new Tuple<Item, ItemProperty>(Legendary.LacuniProwlers, ItemProperty.AttackSpeed), new ItemStatRange { Max = 7, Min = 5 }},
            { new Tuple<Item, ItemProperty>(Legendary.SteadyStrikers, ItemProperty.AttackSpeed), new ItemStatRange { Max = 7, Min = 5 }},
            { new Tuple<Item, ItemProperty>(Legendary.MempoOfTwilight, ItemProperty.AttackSpeed), new ItemStatRange { Max = 7, Min = 5 }},
            { new Tuple<Item, ItemProperty>(Legendary.AndarielsVisage, ItemProperty.ElementalDamage), new ItemStatRange { Max = 20, Min = 15 }},
            { new Tuple<Item, ItemProperty>(Legendary.AndarielsVisage, ItemProperty.AttackSpeed), new ItemStatRange { Max = 7, Min = 5 }},
            { new Tuple<Item, ItemProperty>(Legendary.SunKeeper, ItemProperty.DamageAgainstElites), new ItemStatRange { Max = 30, Min = 15 }},
            { new Tuple<Item, ItemProperty>(Legendary.Frostburn, ItemProperty.ColdSkills), new ItemStatRange { Max = 15, Min = 10 }},
            { new Tuple<Item, ItemProperty>(Legendary.ThundergodsVigor, ItemProperty.LightningSkills), new ItemStatRange { Max = 15, Min = 10 }},
            { new Tuple<Item, ItemProperty>(Legendary.SashOfKnives, ItemProperty.ResourceCost), new ItemStatRange { Max = 8, Min = 5 }},
            { new Tuple<Item, ItemProperty>(Legendary.StoneOfJordan, ItemProperty.DamageAgainstElites), new ItemStatRange { Max = 30, Min = 25 }},
            { new Tuple<Item, ItemProperty>(Legendary.StoneOfJordan, ItemProperty.ElementalDamage), new ItemStatRange { Max = 20, Min = 15 }},
            { new Tuple<Item, ItemProperty>(Legendary.Unity, ItemProperty.DamageAgainstElites), new ItemStatRange { Max = 15, Min = 12 }},
            { new Tuple<Item, ItemProperty>(Legendary.Etrayu, ItemProperty.ColdSkills), new ItemStatRange { Max = 20, Min = 15 }},
            { new Tuple<Item, ItemProperty>(Legendary.Uskang, ItemProperty.LightningSkills), new ItemStatRange { Max = 20, Min = 15 }},
            { new Tuple<Item, ItemProperty>(Legendary.Triumvirate, ItemProperty.LightningSkills), new ItemStatRange { Max = 10, Min = 7 }},
            { new Tuple<Item, ItemProperty>(Legendary.Triumvirate, ItemProperty.FireSkills), new ItemStatRange { Max = 10, Min = 7 }},
            { new Tuple<Item, ItemProperty>(Legendary.Triumvirate, ItemProperty.ArcaneSkills), new ItemStatRange { Max = 10, Min = 7 }},
            { new Tuple<Item, ItemProperty>(Legendary.WinterFlurry, ItemProperty.ColdSkills), new ItemStatRange { Max = 20, Min = 15 }},
        };

        /// <summary>
        /// Properties that are ALWAYS available to the ItemType are listed here.  
        /// Determines if Property will be available in ItemList rules dropdown.     
        /// </summary>
        public static readonly Dictionary<KeyValuePair<TinityItemType, ItemProperty>, ItemStatRange> ItemPropertyLimitsByItemType = new Dictionary<KeyValuePair<TinityItemType, ItemProperty>, ItemStatRange>
        {

            // PrimaryStat

            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.Ring, ItemProperty.PrimaryStat), new ItemStatRange {AncientMax = 650, AncientMin = 550, Max = 500, Min = 416 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.Amulet, ItemProperty.PrimaryStat), new ItemStatRange {AncientMax = 1000, AncientMin = 825, Max = 750, Min = 626 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.Helm, ItemProperty.PrimaryStat), new ItemStatRange {AncientMax = 1000, AncientMin = 750, Max = 750, Min = 626 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.VoodooMask, ItemProperty.PrimaryStat), new ItemStatRange {AncientMax = 1000, AncientMin = 750, Max = 750, Min = 626 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.WizardHat, ItemProperty.PrimaryStat), new ItemStatRange {AncientMax = 1000, AncientMin = 750, Max = 750, Min = 626 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.SpiritStone, ItemProperty.PrimaryStat), new ItemStatRange {AncientMax = 1000, AncientMin = 750, Max = 750, Min = 626 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.Gloves, ItemProperty.PrimaryStat), new ItemStatRange {AncientMax = 1000, AncientMin = 825, Max = 750, Min = 626 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.Quiver, ItemProperty.PrimaryStat), new ItemStatRange {AncientMax = 1000, AncientMin = 825, Max = 750, Min = 650 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.Orb, ItemProperty.PrimaryStat), new ItemStatRange {AncientMax = 1000, AncientMin = 825, Max = 750, Min = 650 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.Mojo, ItemProperty.PrimaryStat), new ItemStatRange {AncientMax = 1000, AncientMin = 825, Max = 750, Min = 650 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.Shoulder, ItemProperty.PrimaryStat), new ItemStatRange {AncientMax = 650, AncientMin = 550, Max = 500, Min = 416 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.Chest, ItemProperty.PrimaryStat), new ItemStatRange {AncientMax = 650, AncientMin = 550, Max = 500, Min = 416 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.Cloak, ItemProperty.PrimaryStat), new ItemStatRange {AncientMax = 650, AncientMin = 550, Max = 500, Min = 416 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.Belt, ItemProperty.PrimaryStat), new ItemStatRange {AncientMax = 650, AncientMin = 550, Max = 500, Min = 416 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.MightyBelt, ItemProperty.PrimaryStat), new ItemStatRange {AncientMax = 650, AncientMin = 550, Max = 500, Min = 416 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.Bracer, ItemProperty.PrimaryStat), new ItemStatRange {AncientMax = 650, AncientMin = 550, Max = 500, Min = 416 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.Legs, ItemProperty.PrimaryStat), new ItemStatRange {AncientMax = 650, AncientMin = 550, Max = 500, Min = 416 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.Boots, ItemProperty.PrimaryStat), new ItemStatRange {AncientMax = 650, AncientMin = 550, Max = 500, Min = 416 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.MightyWeapon, ItemProperty.PrimaryStat), new ItemStatRange {AncientMax = 1000, AncientMin = 825, Max = 750, Min = 626 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.Wand, ItemProperty.PrimaryStat), new ItemStatRange {AncientMax = 1000, AncientMin = 825, Max = 750, Min = 626 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.Flail, ItemProperty.PrimaryStat), new ItemStatRange {AncientMax = 1000, AncientMin = 825, Max = 750, Min = 626 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.Axe, ItemProperty.PrimaryStat), new ItemStatRange {AncientMax = 1000, AncientMin = 825, Max = 750, Min = 626 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.HandCrossbow, ItemProperty.PrimaryStat), new ItemStatRange {AncientMax = 1000, AncientMin = 825, Max = 750, Min = 626 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.CeremonialKnife, ItemProperty.PrimaryStat), new ItemStatRange {AncientMax = 1000, AncientMin = 825, Max = 750, Min = 626 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.FistWeapon, ItemProperty.PrimaryStat), new ItemStatRange {AncientMax = 1000, AncientMin = 825, Max = 750, Min = 626 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.Mace, ItemProperty.PrimaryStat), new ItemStatRange {AncientMax = 1000, AncientMin = 825, Max = 750, Min = 626 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.Sword, ItemProperty.PrimaryStat), new ItemStatRange {AncientMax = 1000, AncientMin = 825, Max = 750, Min = 626 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.TwoHandDaibo, ItemProperty.PrimaryStat), new ItemStatRange {AncientMax = 1465, AncientMin = 1237, Max = 1125, Min = 946 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.TwoHandCrossbow, ItemProperty.PrimaryStat), new ItemStatRange {AncientMax = 1000, AncientMin = 825, Max = 500, Min = 416 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.TwoHandAxe, ItemProperty.PrimaryStat), new ItemStatRange {AncientMax = 1465, AncientMin = 1237, Max = 1125, Min = 946 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.TwoHandBow, ItemProperty.PrimaryStat), new ItemStatRange {AncientMax = 1000, AncientMin = 825, Max = 500, Min = 416 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.TwoHandFlail, ItemProperty.PrimaryStat), new ItemStatRange {AncientMax = 1465, AncientMin = 1237, Max = 1125, Min = 946 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.TwoHandMace, ItemProperty.PrimaryStat), new ItemStatRange {AncientMax = 1465, AncientMin = 1237, Max = 1125, Min = 946 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.TwoHandMighty, ItemProperty.PrimaryStat), new ItemStatRange {AncientMax = 1465, AncientMin = 1237, Max = 1125, Min = 946 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.TwoHandPolearm, ItemProperty.PrimaryStat), new ItemStatRange {AncientMax = 1465, AncientMin = 1237, Max = 1125, Min = 946 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.TwoHandStaff, ItemProperty.PrimaryStat), new ItemStatRange {AncientMax = 1465, AncientMin = 1237, Max = 1125, Min = 946 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.TwoHandSword, ItemProperty.PrimaryStat), new ItemStatRange {AncientMax = 1465, AncientMin = 1237, Max = 1125, Min = 946 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.Spear, ItemProperty.PrimaryStat), new ItemStatRange {AncientMax = 1000, AncientMin = 825, Max = 750, Min = 626 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.Shield, ItemProperty.PrimaryStat), new ItemStatRange {AncientMax = 650, AncientMin = 550, Max = 500, Min = 416 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.Dagger, ItemProperty.PrimaryStat), new ItemStatRange {AncientMax = 1000, AncientMin = 825, Max = 750, Min = 626 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.CrusaderShield, ItemProperty.PrimaryStat), new ItemStatRange {AncientMax = 650, AncientMin = 550, Max = 500, Min = 416 }},

            // CriticalDamage

            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.Ring, ItemProperty.CriticalHitDamage), new ItemStatRange { Max = 50, Min = 26 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.Amulet, ItemProperty.CriticalHitDamage), new ItemStatRange { Max = 100, Min = 51 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.Gloves, ItemProperty.CriticalHitDamage), new ItemStatRange { Max = 50, Min = 26 }},

            // CriticalChance

            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.Ring, ItemProperty.CriticalHitChance), new ItemStatRange { Max = 6, Min = 4.5 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.Amulet, ItemProperty.CriticalHitChance), new ItemStatRange { Max = 10, Min = 8 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.Helm, ItemProperty.CriticalHitChance), new ItemStatRange { Max = 6, Min = 4.5 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.VoodooMask, ItemProperty.CriticalHitChance), new ItemStatRange { Max = 6, Min = 4.5 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.WizardHat, ItemProperty.CriticalHitChance), new ItemStatRange { Max = 6, Min = 4.5 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.SpiritStone, ItemProperty.CriticalHitChance), new ItemStatRange { Max = 6, Min = 4.5 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.Gloves, ItemProperty.CriticalHitChance), new ItemStatRange { Max = 10, Min = 8 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.Quiver, ItemProperty.CriticalHitChance), new ItemStatRange { Max = 10, Min = 8 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.Orb, ItemProperty.CriticalHitChance), new ItemStatRange { Max = 10, Min = 8 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.Mojo, ItemProperty.CriticalHitChance), new ItemStatRange { Max = 10, Min = 8 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.Bracer, ItemProperty.CriticalHitChance), new ItemStatRange { Max = 6, Min = 4.5 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.Shield, ItemProperty.CriticalHitChance), new ItemStatRange { Max = 10, Min = 8 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.CrusaderShield, ItemProperty.CriticalHitChance), new ItemStatRange { Max = 10, Min = 8 }},

            // IAS

            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.Ring, ItemProperty.AttackSpeed), new ItemStatRange { Max = 7, Min = 5 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.Amulet, ItemProperty.AttackSpeed), new ItemStatRange { Max = 7, Min = 5 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.Gloves, ItemProperty.AttackSpeed), new ItemStatRange { Max = 7, Min = 5 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.Quiver, ItemProperty.AttackSpeed), new ItemStatRange { Max = 7, Min = 5 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.MightyWeapon, ItemProperty.AttackSpeed), new ItemStatRange { Max = 7, Min = 5 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.Wand, ItemProperty.AttackSpeed), new ItemStatRange { Max = 7, Min = 5 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.Flail, ItemProperty.AttackSpeed), new ItemStatRange { Max = 7, Min = 5 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.Axe, ItemProperty.AttackSpeed), new ItemStatRange { Max = 7, Min = 5 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.HandCrossbow, ItemProperty.AttackSpeed), new ItemStatRange { Max = 7, Min = 5 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.CeremonialKnife, ItemProperty.AttackSpeed), new ItemStatRange { Max = 7, Min = 5 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.FistWeapon, ItemProperty.AttackSpeed), new ItemStatRange { Max = 7, Min = 5 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.Mace, ItemProperty.AttackSpeed), new ItemStatRange { Max = 7, Min = 5 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.Sword, ItemProperty.AttackSpeed), new ItemStatRange { Max = 7, Min = 5 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.TwoHandDaibo, ItemProperty.AttackSpeed), new ItemStatRange { Max = 7, Min = 5 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.TwoHandCrossbow, ItemProperty.AttackSpeed), new ItemStatRange { Max = 7, Min = 5 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.TwoHandAxe, ItemProperty.AttackSpeed), new ItemStatRange { Max = 7, Min = 5 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.TwoHandBow, ItemProperty.AttackSpeed), new ItemStatRange { Max = 7, Min = 5 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.TwoHandFlail, ItemProperty.AttackSpeed), new ItemStatRange { Max = 7, Min = 5 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.TwoHandMace, ItemProperty.AttackSpeed), new ItemStatRange { Max = 7, Min = 5 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.TwoHandMighty, ItemProperty.AttackSpeed), new ItemStatRange { Max = 7, Min = 5 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.TwoHandPolearm, ItemProperty.AttackSpeed), new ItemStatRange { Max = 7, Min = 5 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.TwoHandStaff, ItemProperty.AttackSpeed), new ItemStatRange { Max = 7, Min = 5 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.TwoHandSword, ItemProperty.AttackSpeed), new ItemStatRange { Max = 7, Min = 5 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.Spear, ItemProperty.AttackSpeed), new ItemStatRange { Max = 7, Min = 5 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.Dagger, ItemProperty.AttackSpeed), new ItemStatRange { Max = 7, Min = 5 }},

            // Skill Damage acdItem.GetSkillDamageIncrease() method is fixed.

            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.Helm, ItemProperty.SkillDamage), new ItemStatRange { Max = 15, Min = 10 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.VoodooMask, ItemProperty.SkillDamage), new ItemStatRange { Max = 15, Min = 10 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.WizardHat, ItemProperty.SkillDamage), new ItemStatRange { Max = 15, Min = 10 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.SpiritStone, ItemProperty.SkillDamage), new ItemStatRange { Max = 15, Min = 10 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.Quiver, ItemProperty.SkillDamage), new ItemStatRange { Max = 15, Min = 10 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.Orb, ItemProperty.SkillDamage), new ItemStatRange { Max = 15, Min = 10 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.Mojo, ItemProperty.SkillDamage), new ItemStatRange { Max = 15, Min = 10 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.Shoulder, ItemProperty.SkillDamage), new ItemStatRange { Max = 15, Min = 10 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.Chest, ItemProperty.SkillDamage), new ItemStatRange { Max = 15, Min = 10 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.Cloak, ItemProperty.SkillDamage), new ItemStatRange { Max = 15, Min = 10 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.Belt, ItemProperty.SkillDamage), new ItemStatRange { Max = 15, Min = 10 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.MightyBelt, ItemProperty.SkillDamage), new ItemStatRange { Max = 15, Min = 10 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.Legs, ItemProperty.SkillDamage), new ItemStatRange { Max = 15, Min = 10 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.Boots, ItemProperty.SkillDamage), new ItemStatRange { Max = 15, Min = 10 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.MightyWeapon, ItemProperty.SkillDamage), new ItemStatRange { Max = 15, Min = 10 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.Wand, ItemProperty.SkillDamage), new ItemStatRange { Max = 15, Min = 10 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.Flail, ItemProperty.SkillDamage), new ItemStatRange { Max = 15, Min = 10 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.HandCrossbow, ItemProperty.SkillDamage), new ItemStatRange { Max = 15, Min = 10 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.FistWeapon, ItemProperty.SkillDamage), new ItemStatRange { Max = 15, Min = 10 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.TwoHandDaibo, ItemProperty.SkillDamage), new ItemStatRange { Max = 15, Min = 10 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.TwoHandMighty, ItemProperty.SkillDamage), new ItemStatRange { Max = 15, Min = 10 }},

            // Base Damage

            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.Orb, ItemProperty.BaseMaxDamage), new ItemStatRange { AncientMax = 600, AncientMin = 400, Max = 500, Min = 340 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.Mojo, ItemProperty.BaseMaxDamage), new ItemStatRange { AncientMax = 600, AncientMin = 400, Max = 500, Min = 340 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.MightyWeapon, ItemProperty.BaseMaxDamage), new ItemStatRange { AncientMax = 1940, AncientMin = 1318, Max = 1490, Min = 981 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.Wand, ItemProperty.BaseMaxDamage), new ItemStatRange { AncientMax = 1940, AncientMin = 1318, Max = 1490, Min = 981 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.Flail, ItemProperty.BaseMaxDamage), new ItemStatRange { AncientMax = 1940, AncientMin = 1318, Max = 1490, Min = 981 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.Axe, ItemProperty.BaseMaxDamage), new ItemStatRange { AncientMax = 1940, AncientMin = 1318, Max = 1490, Min = 981 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.HandCrossbow, ItemProperty.BaseMaxDamage), new ItemStatRange { AncientMax = 1940, AncientMin = 1318, Max = 1304, Min = 856 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.CeremonialKnife, ItemProperty.BaseMaxDamage), new ItemStatRange { AncientMax = 1940, AncientMin = 1318, Max = 1490, Min = 981 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.FistWeapon, ItemProperty.BaseMaxDamage), new ItemStatRange { AncientMax = 1940, AncientMin = 1318, Max = 1490, Min = 981 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.Mace, ItemProperty.BaseMaxDamage), new ItemStatRange { AncientMax = 1940, AncientMin = 1318, Max = 1490, Min = 981 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.Sword, ItemProperty.BaseMaxDamage), new ItemStatRange { AncientMax = 1940, AncientMin = 1318, Max = 1490, Min = 981 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.Spear, ItemProperty.BaseMaxDamage), new ItemStatRange { AncientMax = 1940, AncientMin = 1318, Max = 1490, Min = 981 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.Dagger, ItemProperty.BaseMaxDamage), new ItemStatRange { AncientMax = 1940, AncientMin = 1318, Max = 1490, Min = 981 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.TwoHandDaibo, ItemProperty.BaseMaxDamage), new ItemStatRange { AncientMax = 2325, AncientMin = 1582, Max = 1788, Min = 1177 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.TwoHandCrossbow, ItemProperty.BaseMaxDamage), new ItemStatRange { AncientMax = 1940, AncientMin = 1318, Max = 1788, Min = 1177 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.TwoHandAxe, ItemProperty.BaseMaxDamage), new ItemStatRange { AncientMax = 2325, AncientMin = 1582, Max = 1788, Min = 1177 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.TwoHandBow, ItemProperty.BaseMaxDamage), new ItemStatRange { AncientMax = 1940, AncientMin = 1318, Max = 1788, Min = 1177 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.TwoHandFlail, ItemProperty.BaseMaxDamage), new ItemStatRange { AncientMax = 2325, AncientMin = 1582, Max = 1788, Min = 1177 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.TwoHandMace, ItemProperty.BaseMaxDamage), new ItemStatRange { AncientMax = 2325, AncientMin = 1582, Max = 1788, Min = 1177 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.TwoHandMighty, ItemProperty.BaseMaxDamage), new ItemStatRange { AncientMax = 2325, AncientMin = 1582, Max = 1788, Min = 1177 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.TwoHandPolearm, ItemProperty.BaseMaxDamage), new ItemStatRange { AncientMax = 2325, AncientMin = 1582, Max = 1788, Min = 1177 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.TwoHandStaff, ItemProperty.BaseMaxDamage), new ItemStatRange { AncientMax = 2325, AncientMin = 1582, Max = 1788, Min = 1177 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.TwoHandSword, ItemProperty.BaseMaxDamage), new ItemStatRange { AncientMax = 2325, AncientMin = 1582, Max = 1788, Min = 1177 }},

            // Percent Damage

            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.MightyWeapon, ItemProperty.PercentDamage), new ItemStatRange {  Max = 10, Min = 6 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.Wand, ItemProperty.PercentDamage), new ItemStatRange {  Max = 10, Min = 6 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.Flail, ItemProperty.PercentDamage), new ItemStatRange {  Max = 10, Min = 6 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.Axe, ItemProperty.PercentDamage), new ItemStatRange {  Max = 10, Min = 6 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.HandCrossbow, ItemProperty.PercentDamage), new ItemStatRange {  Max = 10, Min = 6 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.CeremonialKnife, ItemProperty.PercentDamage), new ItemStatRange {  Max = 10, Min = 6 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.FistWeapon, ItemProperty.PercentDamage), new ItemStatRange {  Max = 10, Min = 6 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.Mace, ItemProperty.PercentDamage), new ItemStatRange {  Max = 10, Min = 6 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.Sword, ItemProperty.PercentDamage), new ItemStatRange {  Max = 10, Min = 6 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.Spear, ItemProperty.PercentDamage), new ItemStatRange {  Max = 10, Min = 6 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.Dagger, ItemProperty.PercentDamage), new ItemStatRange {  Max = 10, Min = 6 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.TwoHandDaibo, ItemProperty.PercentDamage), new ItemStatRange {  Max = 10, Min = 6 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.TwoHandCrossbow, ItemProperty.PercentDamage), new ItemStatRange { Max = 10, Min = 6 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.TwoHandAxe, ItemProperty.PercentDamage), new ItemStatRange {  Max = 10, Min = 6 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.TwoHandBow, ItemProperty.PercentDamage), new ItemStatRange { Max = 10, Min = 6 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.TwoHandFlail, ItemProperty.PercentDamage), new ItemStatRange {  Max = 10, Min = 6 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.TwoHandMace, ItemProperty.PercentDamage), new ItemStatRange {  Max = 10, Min = 6 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.TwoHandMighty, ItemProperty.PercentDamage), new ItemStatRange {  Max = 10, Min = 6 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.TwoHandPolearm, ItemProperty.PercentDamage), new ItemStatRange {  Max = 10, Min = 6 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.TwoHandStaff, ItemProperty.PercentDamage), new ItemStatRange {  Max = 10, Min = 6 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.TwoHandSword, ItemProperty.PercentDamage), new ItemStatRange {  Max = 10, Min = 6 }},

            // Sockets

            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.Ring, ItemProperty.Sockets), new ItemStatRange { Max = 1, Min = 0 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.Amulet, ItemProperty.Sockets), new ItemStatRange { Max = 1, Min = 0 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.Helm, ItemProperty.Sockets), new ItemStatRange { Max = 1, Min = 0 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.VoodooMask, ItemProperty.Sockets), new ItemStatRange { Max = 1, Min = 0 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.WizardHat, ItemProperty.Sockets), new ItemStatRange { Max = 1, Min = 0 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.SpiritStone, ItemProperty.Sockets), new ItemStatRange { Max = 1, Min = 0 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.Quiver, ItemProperty.Sockets), new ItemStatRange { Max = 1, Min = 0 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.Orb, ItemProperty.Sockets), new ItemStatRange { Max = 1, Min = 0 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.Mojo, ItemProperty.Sockets), new ItemStatRange { Max = 1, Min = 0 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.Chest, ItemProperty.Sockets), new ItemStatRange { Max = 3, Min = 0 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.Cloak, ItemProperty.Sockets), new ItemStatRange { Max = 3, Min = 0 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.Legs, ItemProperty.Sockets), new ItemStatRange { Max = 2, Min = 0 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.MightyWeapon, ItemProperty.Sockets), new ItemStatRange { Max = 1, Min = 0 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.Wand, ItemProperty.Sockets), new ItemStatRange { Max = 1, Min = 0 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.Flail, ItemProperty.Sockets), new ItemStatRange { Max = 1, Min = 0 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.Axe, ItemProperty.Sockets), new ItemStatRange { Max = 1, Min = 0 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.HandCrossbow, ItemProperty.Sockets), new ItemStatRange { Max = 1, Min = 0 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.CeremonialKnife, ItemProperty.Sockets), new ItemStatRange { Max = 1, Min = 0 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.FistWeapon, ItemProperty.Sockets), new ItemStatRange { Max = 1, Min = 0 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.Mace, ItemProperty.Sockets), new ItemStatRange { Max = 1, Min = 0 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.Sword, ItemProperty.Sockets), new ItemStatRange { Max = 1, Min = 0 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.TwoHandDaibo, ItemProperty.Sockets), new ItemStatRange { Max = 1, Min = 0 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.TwoHandCrossbow, ItemProperty.Sockets), new ItemStatRange { Max = 1, Min = 0 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.TwoHandAxe, ItemProperty.Sockets), new ItemStatRange { Max = 1, Min = 0 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.TwoHandBow, ItemProperty.Sockets), new ItemStatRange { Max = 1, Min = 0 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.TwoHandFlail, ItemProperty.Sockets), new ItemStatRange { Max = 1, Min = 0 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.TwoHandMace, ItemProperty.Sockets), new ItemStatRange { Max = 1, Min = 0 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.TwoHandMighty, ItemProperty.Sockets), new ItemStatRange { Max = 1, Min = 0 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.TwoHandPolearm, ItemProperty.Sockets), new ItemStatRange { Max = 1, Min = 0 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.TwoHandStaff, ItemProperty.Sockets), new ItemStatRange { Max = 1, Min = 0 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.TwoHandSword, ItemProperty.Sockets), new ItemStatRange { Max = 1, Min = 0 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.Spear, ItemProperty.Sockets), new ItemStatRange { Max = 1, Min = 0 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.Shield, ItemProperty.Sockets), new ItemStatRange { Max = 1, Min = 0 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.Dagger, ItemProperty.Sockets), new ItemStatRange { Max = 1, Min = 0 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.CrusaderShield, ItemProperty.Sockets), new ItemStatRange { Max = 1, Min = 0 }},
            
            // Resource Cost Reduction

            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.Ring, ItemProperty.ResourceCost), new ItemStatRange { Max = 8, Min = 5 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.Amulet, ItemProperty.ResourceCost), new ItemStatRange { Max = 8, Min = 5 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.Gloves, ItemProperty.ResourceCost), new ItemStatRange { Max = 8, Min = 5 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.Quiver, ItemProperty.ResourceCost), new ItemStatRange { Max = 8, Min = 5 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.Orb, ItemProperty.ResourceCost), new ItemStatRange { Max = 8, Min = 5 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.Mojo, ItemProperty.ResourceCost), new ItemStatRange {Max = 8, Min = 5 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.Shoulder, ItemProperty.ResourceCost), new ItemStatRange { Max = 8, Min = 5 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.MightyWeapon, ItemProperty.ResourceCost), new ItemStatRange { Max = 10, Min = 8 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.Wand, ItemProperty.ResourceCost), new ItemStatRange { Max = 10, Min = 8 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.Flail, ItemProperty.ResourceCost), new ItemStatRange { Max = 10, Min = 8 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.Axe, ItemProperty.ResourceCost), new ItemStatRange { Max = 10, Min = 8 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.HandCrossbow, ItemProperty.ResourceCost), new ItemStatRange { Max = 10, Min = 8 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.CeremonialKnife, ItemProperty.ResourceCost), new ItemStatRange { Max = 10, Min = 8 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.FistWeapon, ItemProperty.ResourceCost), new ItemStatRange { Max = 10, Min = 8 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.Mace, ItemProperty.ResourceCost), new ItemStatRange { Max = 10, Min = 8 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.Sword, ItemProperty.ResourceCost), new ItemStatRange { Max = 10, Min = 8 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.TwoHandDaibo, ItemProperty.ResourceCost), new ItemStatRange { Max = 10, Min = 8 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.TwoHandCrossbow, ItemProperty.ResourceCost), new ItemStatRange { Max = 10, Min = 8 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.TwoHandAxe, ItemProperty.ResourceCost), new ItemStatRange { Max = 10, Min = 8 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.TwoHandBow, ItemProperty.ResourceCost), new ItemStatRange { Max = 10, Min = 8 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.TwoHandFlail, ItemProperty.ResourceCost), new ItemStatRange { Max = 10, Min = 8 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.TwoHandMace, ItemProperty.ResourceCost), new ItemStatRange { Max = 10, Min = 8 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.TwoHandMighty, ItemProperty.ResourceCost), new ItemStatRange { Max = 10, Min = 8 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.TwoHandPolearm, ItemProperty.ResourceCost), new ItemStatRange { Max = 10, Min = 8 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.TwoHandStaff, ItemProperty.ResourceCost), new ItemStatRange { Max = 10, Min = 8 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.TwoHandSword, ItemProperty.ResourceCost), new ItemStatRange { Max = 10, Min = 8 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.Spear, ItemProperty.ResourceCost), new ItemStatRange { Max = 10, Min = 8 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.Dagger, ItemProperty.ResourceCost), new ItemStatRange { Max = 10, Min = 8 }},

            // Cooldown Reduction

            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.Ring, ItemProperty.Cooldown), new ItemStatRange { Max = 8, Min = 5 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.Amulet, ItemProperty.Cooldown), new ItemStatRange { Max = 8, Min = 5 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.Gloves, ItemProperty.Cooldown), new ItemStatRange { Max = 8, Min = 5 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.Quiver, ItemProperty.Cooldown), new ItemStatRange { Max = 8, Min = 5 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.Orb, ItemProperty.Cooldown), new ItemStatRange { Max = 8, Min = 5 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.Mojo, ItemProperty.Cooldown), new ItemStatRange {Max = 8, Min = 5 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.Shoulder, ItemProperty.Cooldown), new ItemStatRange { Max = 8, Min = 5 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.MightyWeapon, ItemProperty.Cooldown), new ItemStatRange { Max = 10, Min = 6 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.Wand, ItemProperty.Cooldown), new ItemStatRange { Max = 10, Min = 6 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.Flail, ItemProperty.Cooldown), new ItemStatRange { Max = 10, Min = 6 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.Axe, ItemProperty.Cooldown), new ItemStatRange { Max = 10, Min = 6 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.HandCrossbow, ItemProperty.Cooldown), new ItemStatRange { Max = 10, Min = 6 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.CeremonialKnife, ItemProperty.Cooldown), new ItemStatRange { Max = 10, Min = 6 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.FistWeapon, ItemProperty.Cooldown), new ItemStatRange { Max = 10, Min = 6 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.Mace, ItemProperty.Cooldown), new ItemStatRange { Max = 10, Min = 6 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.Sword, ItemProperty.Cooldown), new ItemStatRange { Max = 10, Min = 6 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.TwoHandDaibo, ItemProperty.Cooldown), new ItemStatRange { Max = 10, Min = 6 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.TwoHandCrossbow, ItemProperty.Cooldown), new ItemStatRange { Max = 10, Min = 6 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.TwoHandAxe, ItemProperty.Cooldown), new ItemStatRange { Max = 10, Min = 6 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.TwoHandBow, ItemProperty.Cooldown), new ItemStatRange { Max = 10, Min = 6 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.TwoHandFlail, ItemProperty.Cooldown), new ItemStatRange { Max = 10, Min = 6 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.TwoHandMace, ItemProperty.Cooldown), new ItemStatRange { Max = 10, Min = 6 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.TwoHandMighty, ItemProperty.Cooldown), new ItemStatRange { Max = 10, Min = 6 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.TwoHandPolearm, ItemProperty.Cooldown), new ItemStatRange { Max = 10, Min = 6 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.TwoHandStaff, ItemProperty.Cooldown), new ItemStatRange { Max = 10, Min = 6 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.TwoHandSword, ItemProperty.Cooldown), new ItemStatRange { Max = 10, Min = 6 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.Spear, ItemProperty.Cooldown), new ItemStatRange { Max = 10, Min = 6 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.Dagger, ItemProperty.Cooldown), new ItemStatRange { Max = 10, Min = 6 }},

            // Damage Against Elites

            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.MightyWeapon, ItemProperty.DamageAgainstElites), new ItemStatRange { Max = 8, Min = 5 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.Wand, ItemProperty.DamageAgainstElites), new ItemStatRange { Max = 8, Min = 5 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.Flail, ItemProperty.DamageAgainstElites), new ItemStatRange { Max = 8, Min = 5 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.Axe, ItemProperty.DamageAgainstElites), new ItemStatRange { Max = 8, Min = 5 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.HandCrossbow, ItemProperty.DamageAgainstElites), new ItemStatRange { Max = 8, Min = 5 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.CeremonialKnife, ItemProperty.DamageAgainstElites), new ItemStatRange { Max = 8, Min = 5 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.FistWeapon, ItemProperty.DamageAgainstElites), new ItemStatRange { Max = 8, Min = 5 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.Mace, ItemProperty.DamageAgainstElites), new ItemStatRange { Max = 8, Min = 5 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.Sword, ItemProperty.DamageAgainstElites), new ItemStatRange { Max = 8, Min = 5 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.TwoHandDaibo, ItemProperty.DamageAgainstElites), new ItemStatRange { Max = 10, Min = 9 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.TwoHandCrossbow, ItemProperty.DamageAgainstElites), new ItemStatRange { Max = 10, Min = 9 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.TwoHandAxe, ItemProperty.DamageAgainstElites), new ItemStatRange { Max = 10, Min = 9 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.TwoHandBow, ItemProperty.DamageAgainstElites), new ItemStatRange { Max = 10, Min = 9 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.TwoHandFlail, ItemProperty.DamageAgainstElites), new ItemStatRange { Max = 10, Min = 9 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.TwoHandMace, ItemProperty.DamageAgainstElites), new ItemStatRange { Max = 10, Min = 9 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.TwoHandMighty, ItemProperty.DamageAgainstElites), new ItemStatRange { Max = 10, Min = 9 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.TwoHandPolearm, ItemProperty.DamageAgainstElites), new ItemStatRange { Max = 10, Min = 9 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.TwoHandStaff, ItemProperty.DamageAgainstElites), new ItemStatRange { Max = 10, Min = 9 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.TwoHandSword, ItemProperty.DamageAgainstElites), new ItemStatRange { Max = 10, Min = 9 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.Spear, ItemProperty.DamageAgainstElites), new ItemStatRange { Max = 8, Min = 5 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.Shield, ItemProperty.DamageAgainstElites), new ItemStatRange { Max = 8, Min = 5 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.Dagger, ItemProperty.DamageAgainstElites), new ItemStatRange { Max = 8, Min = 5 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.CrusaderShield, ItemProperty.DamageAgainstElites), new ItemStatRange { Max = 8, Min = 5 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.Quiver, ItemProperty.DamageAgainstElites), new ItemStatRange { Max = 10, Min = 8 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.Orb, ItemProperty.DamageAgainstElites), new ItemStatRange { Max = 10, Min = 8 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.Mojo, ItemProperty.DamageAgainstElites), new ItemStatRange { Max = 10, Min = 8 }},

            // ElementalDamage

            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.Amulet, ItemProperty.ElementalDamage), new ItemStatRange { Max = 20, Min = 15 }},
            {new KeyValuePair<TinityItemType, ItemProperty>(TinityItemType.Bracer, ItemProperty.ElementalDamage), new ItemStatRange { Max = 20, Min = 15 }},


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

        public static readonly LookupList<KeyValuePair<TinityItemType, ActorClass>, Skill> SkillDamageByItemTypeAndClass = new LookupList<KeyValuePair<TinityItemType, ActorClass>, Skill>
        {
            // Head Slot
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Helm, ActorClass.DemonHunter), Skills.DemonHunter.ClusterArrow},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Helm, ActorClass.DemonHunter), Skills.DemonHunter.Multishot},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Helm, ActorClass.DemonHunter), Skills.DemonHunter.ElementalArrow},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Helm, ActorClass.DemonHunter), Skills.DemonHunter.Strafe},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Helm, ActorClass.DemonHunter), Skills.DemonHunter.Chakram},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Helm, ActorClass.DemonHunter), Skills.DemonHunter.RapidFire},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Helm, ActorClass.DemonHunter), Skills.DemonHunter.Impale},

            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Helm, ActorClass.Barbarian), Skills.Barbarian.Whirlwind},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Helm, ActorClass.Barbarian), Skills.Barbarian.SeismicSlam},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Helm, ActorClass.Barbarian), Skills.Barbarian.AncientSpear},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Helm, ActorClass.Barbarian), Skills.Barbarian.HammerOfTheAncients},

            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Helm, ActorClass.Wizard), Skills.Wizard.Meteor},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Helm, ActorClass.Wizard), Skills.Wizard.Disintegrate},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Helm, ActorClass.Wizard), Skills.Wizard.EnergyTwister},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Helm, ActorClass.Wizard), Skills.Wizard.ArcaneTorrent},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Helm, ActorClass.Wizard), Skills.Wizard.WaveOfForce},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Helm, ActorClass.Wizard), Skills.Wizard.ArcaneOrb},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Helm, ActorClass.Wizard), Skills.Wizard.RayOfFrost},

            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Helm, ActorClass.Witchdoctor), Skills.WitchDoctor.AcidCloud},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Helm, ActorClass.Witchdoctor), Skills.WitchDoctor.SpiritBarrage},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Helm, ActorClass.Witchdoctor), Skills.WitchDoctor.ZombieCharger},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Helm, ActorClass.Witchdoctor), Skills.WitchDoctor.Sacrifice},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Helm, ActorClass.Witchdoctor), Skills.WitchDoctor.Firebats},

            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.VoodooMask, ActorClass.Witchdoctor), Skills.WitchDoctor.AcidCloud},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.VoodooMask, ActorClass.Witchdoctor), Skills.WitchDoctor.SpiritBarrage},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.VoodooMask, ActorClass.Witchdoctor), Skills.WitchDoctor.ZombieCharger},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.VoodooMask, ActorClass.Witchdoctor), Skills.WitchDoctor.Sacrifice},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.VoodooMask, ActorClass.Witchdoctor), Skills.WitchDoctor.Firebats},

            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Helm, ActorClass.Monk), Skills.Monk.TempestRush},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Helm, ActorClass.Monk), Skills.Monk.ExplodingPalm},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Helm, ActorClass.Monk), Skills.Monk.WaveOfLight},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Helm, ActorClass.Monk), Skills.Monk.LashingTailKick},

            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.SpiritStone, ActorClass.Monk), Skills.Monk.TempestRush},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.SpiritStone, ActorClass.Monk), Skills.Monk.ExplodingPalm},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.SpiritStone, ActorClass.Monk), Skills.Monk.WaveOfLight},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.SpiritStone, ActorClass.Monk), Skills.Monk.LashingTailKick},  

            // Shoulders

            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Shoulder, ActorClass.DemonHunter), Skills.DemonHunter.Companion},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Shoulder, ActorClass.DemonHunter), Skills.DemonHunter.RainOfVengeance},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Shoulder, ActorClass.DemonHunter), Skills.DemonHunter.Sentry},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Shoulder, ActorClass.DemonHunter), Skills.DemonHunter.SpikeTrap},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Shoulder, ActorClass.DemonHunter), Skills.DemonHunter.FanOfKnives},

            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Shoulder, ActorClass.Barbarian), Skills.Barbarian.Avalanche},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Shoulder, ActorClass.Barbarian), Skills.Barbarian.Earthquake},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Shoulder, ActorClass.Barbarian), Skills.Barbarian.Overpower},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Shoulder, ActorClass.Barbarian), Skills.Barbarian.Rend},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Shoulder, ActorClass.Barbarian), Skills.Barbarian.Revenge},

            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Shoulder, ActorClass.Wizard), Skills.Wizard.BlackHole},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Shoulder, ActorClass.Wizard), Skills.Wizard.Blizzard},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Shoulder, ActorClass.Wizard), Skills.Wizard.Familiar},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Shoulder, ActorClass.Wizard), Skills.Wizard.ExplosiveBlast},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Shoulder, ActorClass.Wizard), Skills.Wizard.Hydra},

            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Shoulder, ActorClass.Witchdoctor), Skills.WitchDoctor.FetishArmy},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Shoulder, ActorClass.Witchdoctor), Skills.WitchDoctor.Gargantuan},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Shoulder, ActorClass.Witchdoctor), Skills.WitchDoctor.WallOfZombies},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Shoulder, ActorClass.Witchdoctor), Skills.WitchDoctor.GraspOfTheDead},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Shoulder, ActorClass.Witchdoctor), Skills.WitchDoctor.Haunt},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Shoulder, ActorClass.Witchdoctor), Skills.WitchDoctor.LocustSwarm},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Shoulder, ActorClass.Witchdoctor), Skills.WitchDoctor.Piranhas},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Shoulder, ActorClass.Witchdoctor), Skills.WitchDoctor.SummonZombieDogs},

            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Shoulder, ActorClass.Monk), Skills.Monk.DashingStrike},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Shoulder, ActorClass.Monk), Skills.Monk.SevensidedStrike},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Shoulder, ActorClass.Monk), Skills.Monk.MysticAlly},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Shoulder, ActorClass.Monk), Skills.Monk.SweepingWind},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Shoulder, ActorClass.Monk), Skills.Monk.CycloneStrike},

            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Shoulder, ActorClass.Crusader), Skills.Crusader.FallingSword},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Shoulder, ActorClass.Crusader), Skills.Crusader.HeavensFury},

            // Chest

            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Chest, ActorClass.DemonHunter), Skills.DemonHunter.Companion},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Chest, ActorClass.DemonHunter), Skills.DemonHunter.RainOfVengeance},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Chest, ActorClass.DemonHunter), Skills.DemonHunter.Sentry},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Chest, ActorClass.DemonHunter), Skills.DemonHunter.SpikeTrap},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Chest, ActorClass.DemonHunter), Skills.DemonHunter.FanOfKnives},

            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Cloak, ActorClass.DemonHunter), Skills.DemonHunter.Companion},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Cloak, ActorClass.DemonHunter), Skills.DemonHunter.RainOfVengeance},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Cloak, ActorClass.DemonHunter), Skills.DemonHunter.Sentry},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Cloak, ActorClass.DemonHunter), Skills.DemonHunter.SpikeTrap},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Cloak, ActorClass.DemonHunter), Skills.DemonHunter.FanOfKnives},

            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Chest, ActorClass.Barbarian), Skills.Barbarian.Avalanche},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Chest, ActorClass.Barbarian), Skills.Barbarian.Earthquake},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Chest, ActorClass.Barbarian), Skills.Barbarian.Overpower},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Chest, ActorClass.Barbarian), Skills.Barbarian.Rend},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Chest, ActorClass.Barbarian), Skills.Barbarian.Revenge},

            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Chest, ActorClass.Wizard), Skills.Wizard.BlackHole},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Chest, ActorClass.Wizard), Skills.Wizard.Blizzard},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Chest, ActorClass.Wizard), Skills.Wizard.Familiar},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Chest, ActorClass.Wizard), Skills.Wizard.ExplosiveBlast},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Chest, ActorClass.Wizard), Skills.Wizard.Hydra},

            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Chest, ActorClass.Witchdoctor), Skills.WitchDoctor.FetishArmy},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Chest, ActorClass.Witchdoctor), Skills.WitchDoctor.Gargantuan},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Chest, ActorClass.Witchdoctor), Skills.WitchDoctor.WallOfZombies},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Chest, ActorClass.Witchdoctor), Skills.WitchDoctor.GraspOfTheDead},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Chest, ActorClass.Witchdoctor), Skills.WitchDoctor.Haunt},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Chest, ActorClass.Witchdoctor), Skills.WitchDoctor.LocustSwarm},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Chest, ActorClass.Witchdoctor), Skills.WitchDoctor.Piranhas},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Chest, ActorClass.Witchdoctor), Skills.WitchDoctor.SummonZombieDogs},

            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Chest, ActorClass.Monk), Skills.Monk.DashingStrike},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Chest, ActorClass.Monk), Skills.Monk.SevensidedStrike},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Chest, ActorClass.Monk), Skills.Monk.MysticAlly},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Chest, ActorClass.Monk), Skills.Monk.SweepingWind},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Chest, ActorClass.Monk), Skills.Monk.CycloneStrike},

            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Chest, ActorClass.Crusader), Skills.Crusader.FallingSword},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Chest, ActorClass.Crusader), Skills.Crusader.HeavensFury},

            // Belt

            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Belt, ActorClass.DemonHunter), Skills.DemonHunter.Grenade},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Belt, ActorClass.DemonHunter), Skills.DemonHunter.EvasiveFire},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Belt, ActorClass.DemonHunter), Skills.DemonHunter.Bolas},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Belt, ActorClass.DemonHunter), Skills.DemonHunter.EntanglingShot},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Belt, ActorClass.DemonHunter), Skills.DemonHunter.HungeringArrow},

            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Belt, ActorClass.Barbarian), Skills.Barbarian.Frenzy},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Belt, ActorClass.Barbarian), Skills.Barbarian.Cleave},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Belt, ActorClass.Barbarian), Skills.Barbarian.Bash},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Belt, ActorClass.Barbarian), Skills.Barbarian.WeaponThrow},

            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.MightyBelt, ActorClass.Barbarian), Skills.Barbarian.Frenzy},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.MightyBelt, ActorClass.Barbarian), Skills.Barbarian.Cleave},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.MightyBelt, ActorClass.Barbarian), Skills.Barbarian.Bash},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.MightyBelt, ActorClass.Barbarian), Skills.Barbarian.WeaponThrow},

            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Belt, ActorClass.Wizard), Skills.Wizard.Electrocute},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Belt, ActorClass.Wizard), Skills.Wizard.SpectralBlade},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Belt, ActorClass.Wizard), Skills.Wizard.ShockPulse},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Belt, ActorClass.Wizard), Skills.Wizard.MagicMissile},

            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Belt, ActorClass.Witchdoctor), Skills.WitchDoctor.Firebomb},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Belt, ActorClass.Witchdoctor), Skills.WitchDoctor.PlagueOfToads},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Belt, ActorClass.Witchdoctor), Skills.WitchDoctor.CorpseSpiders},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Belt, ActorClass.Witchdoctor), Skills.WitchDoctor.PoisonDart},

            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Belt, ActorClass.Monk), Skills.Monk.WayOfTheHundredFists},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Belt, ActorClass.Monk), Skills.Monk.CripplingWave},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Belt, ActorClass.Monk), Skills.Monk.DeadlyReach},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Belt, ActorClass.Monk), Skills.Monk.FistsOfThunder},

            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Belt, ActorClass.Crusader), Skills.Crusader.Punish},

            // Pants

            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Legs, ActorClass.DemonHunter), Skills.DemonHunter.Grenade},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Legs, ActorClass.DemonHunter), Skills.DemonHunter.EvasiveFire},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Legs, ActorClass.DemonHunter), Skills.DemonHunter.Bolas},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Legs, ActorClass.DemonHunter), Skills.DemonHunter.EntanglingShot},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Legs, ActorClass.DemonHunter), Skills.DemonHunter.HungeringArrow},

            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Legs, ActorClass.Barbarian), Skills.Barbarian.Frenzy},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Legs, ActorClass.Barbarian), Skills.Barbarian.Cleave},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Legs, ActorClass.Barbarian), Skills.Barbarian.Bash},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Legs, ActorClass.Barbarian), Skills.Barbarian.WeaponThrow},

            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Legs, ActorClass.Wizard), Skills.Wizard.Electrocute},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Legs, ActorClass.Wizard), Skills.Wizard.SpectralBlade},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Legs, ActorClass.Wizard), Skills.Wizard.ShockPulse},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Legs, ActorClass.Wizard), Skills.Wizard.MagicMissile},

            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Legs, ActorClass.Witchdoctor), Skills.WitchDoctor.Firebomb},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Legs, ActorClass.Witchdoctor), Skills.WitchDoctor.PlagueOfToads},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Legs, ActorClass.Witchdoctor), Skills.WitchDoctor.CorpseSpiders},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Legs, ActorClass.Witchdoctor), Skills.WitchDoctor.PoisonDart},

            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Legs, ActorClass.Monk), Skills.Monk.WayOfTheHundredFists},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Legs, ActorClass.Monk), Skills.Monk.CripplingWave},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Legs, ActorClass.Monk), Skills.Monk.DeadlyReach},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Legs, ActorClass.Monk), Skills.Monk.FistsOfThunder},

            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Legs, ActorClass.Crusader), Skills.Crusader.Punish},
            
            // Boots

            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Helm, ActorClass.DemonHunter), Skills.DemonHunter.ClusterArrow},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Helm, ActorClass.DemonHunter), Skills.DemonHunter.Multishot},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Helm, ActorClass.DemonHunter), Skills.DemonHunter.ElementalArrow},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Helm, ActorClass.DemonHunter), Skills.DemonHunter.Strafe},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Helm, ActorClass.DemonHunter), Skills.DemonHunter.Chakram},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Helm, ActorClass.DemonHunter), Skills.DemonHunter.RapidFire},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Helm, ActorClass.DemonHunter), Skills.DemonHunter.Impale},

            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Helm, ActorClass.Barbarian), Skills.Barbarian.Whirlwind},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Helm, ActorClass.Barbarian), Skills.Barbarian.SeismicSlam},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Helm, ActorClass.Barbarian), Skills.Barbarian.AncientSpear},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Helm, ActorClass.Barbarian), Skills.Barbarian.HammerOfTheAncients},

            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Helm, ActorClass.Wizard), Skills.Wizard.Meteor},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Helm, ActorClass.Wizard), Skills.Wizard.Disintegrate},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Helm, ActorClass.Wizard), Skills.Wizard.EnergyTwister},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Helm, ActorClass.Wizard), Skills.Wizard.ArcaneTorrent},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Helm, ActorClass.Wizard), Skills.Wizard.WaveOfForce},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Helm, ActorClass.Wizard), Skills.Wizard.ArcaneOrb},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Helm, ActorClass.Wizard), Skills.Wizard.RayOfFrost},

            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Helm, ActorClass.Witchdoctor), Skills.WitchDoctor.AcidCloud},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Helm, ActorClass.Witchdoctor), Skills.WitchDoctor.SpiritBarrage},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Helm, ActorClass.Witchdoctor), Skills.WitchDoctor.ZombieCharger},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Helm, ActorClass.Witchdoctor), Skills.WitchDoctor.Sacrifice},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Helm, ActorClass.Witchdoctor), Skills.WitchDoctor.Firebats},

            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Helm, ActorClass.Monk), Skills.Monk.TempestRush},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Helm, ActorClass.Monk), Skills.Monk.ExplodingPalm},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Helm, ActorClass.Monk), Skills.Monk.WaveOfLight},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Helm, ActorClass.Monk), Skills.Monk.LashingTailKick},  

            // Offhand

            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Quiver, ActorClass.DemonHunter), Skills.DemonHunter.ClusterArrow},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Quiver, ActorClass.DemonHunter), Skills.DemonHunter.Multishot},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Quiver, ActorClass.DemonHunter), Skills.DemonHunter.ElementalArrow},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Quiver, ActorClass.DemonHunter), Skills.DemonHunter.Strafe},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Quiver, ActorClass.DemonHunter), Skills.DemonHunter.Chakram},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Quiver, ActorClass.DemonHunter), Skills.DemonHunter.RapidFire},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Quiver, ActorClass.DemonHunter), Skills.DemonHunter.Impale},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Quiver, ActorClass.DemonHunter), Skills.DemonHunter.Sentry},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Quiver, ActorClass.DemonHunter), Skills.DemonHunter.RainOfVengeance},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Quiver, ActorClass.DemonHunter), Skills.DemonHunter.FanOfKnives},

            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Orb, ActorClass.Wizard), Skills.Wizard.EnergyTwister},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Orb, ActorClass.Wizard), Skills.Wizard.Electrocute},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Orb, ActorClass.Wizard), Skills.Wizard.ArcaneOrb},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Orb, ActorClass.Wizard), Skills.Wizard.Hydra},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Orb, ActorClass.Wizard), Skills.Wizard.MagicMissile},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Orb, ActorClass.Wizard), Skills.Wizard.Familiar},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Orb, ActorClass.Wizard), Skills.Wizard.RayOfFrost},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Orb, ActorClass.Wizard), Skills.Wizard.Meteor},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Orb, ActorClass.Wizard), Skills.Wizard.SpectralBlade},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Orb, ActorClass.Wizard), Skills.Wizard.WaveOfForce},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Orb, ActorClass.Wizard), Skills.Wizard.Disintegrate},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Orb, ActorClass.Wizard), Skills.Wizard.ArcaneTorrent},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Orb, ActorClass.Wizard), Skills.Wizard.ExplosiveBlast},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Orb, ActorClass.Wizard), Skills.Wizard.Blizzard},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Orb, ActorClass.Wizard), Skills.Wizard.BlackHole},

            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Mojo, ActorClass.Witchdoctor), Skills.WitchDoctor.SpiritBarrage},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Mojo, ActorClass.Witchdoctor), Skills.WitchDoctor.LocustSwarm},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Mojo, ActorClass.Witchdoctor), Skills.WitchDoctor.PlagueOfToads},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Mojo, ActorClass.Witchdoctor), Skills.WitchDoctor.Haunt},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Mojo, ActorClass.Witchdoctor), Skills.WitchDoctor.Firebats},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Mojo, ActorClass.Witchdoctor), Skills.WitchDoctor.GraspOfTheDead},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Mojo, ActorClass.Witchdoctor), Skills.WitchDoctor.Sacrifice},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Mojo, ActorClass.Witchdoctor), Skills.WitchDoctor.FetishArmy},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Mojo, ActorClass.Witchdoctor), Skills.WitchDoctor.ZombieCharger},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Mojo, ActorClass.Witchdoctor), Skills.WitchDoctor.CorpseSpiders},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Mojo, ActorClass.Witchdoctor), Skills.WitchDoctor.AcidCloud},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Mojo, ActorClass.Witchdoctor), Skills.WitchDoctor.PoisonDart},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Mojo, ActorClass.Witchdoctor), Skills.WitchDoctor.WallOfZombies},

            // One Hand Weapon

            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.MightyWeapon, ActorClass.Barbarian), Skills.Barbarian.WeaponThrow},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.MightyWeapon, ActorClass.Barbarian), Skills.Barbarian.AncientSpear},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Wand, ActorClass.Wizard), Skills.Wizard.Disintegrate},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Wand, ActorClass.Wizard), Skills.Wizard.SpectralBlade},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.Flail, ActorClass.Crusader), Skills.Crusader.BlessedHammer},

            // Two Hand Weapon

            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.TwoHandMighty, ActorClass.Barbarian), Skills.Barbarian.Whirlwind},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.TwoHandMighty, ActorClass.Barbarian), Skills.Barbarian.SeismicSlam},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.TwoHandMighty, ActorClass.Barbarian), Skills.Barbarian.AncientSpear},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.TwoHandMighty, ActorClass.Barbarian), Skills.Barbarian.HammerOfTheAncients},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.TwoHandMighty, ActorClass.Barbarian), Skills.Barbarian.Avalanche},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.TwoHandMighty, ActorClass.Barbarian), Skills.Barbarian.Earthquake},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.TwoHandMighty, ActorClass.Barbarian), Skills.Barbarian.Overpower},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.TwoHandMighty, ActorClass.Barbarian), Skills.Barbarian.Rend},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.TwoHandMighty, ActorClass.Barbarian), Skills.Barbarian.Revenge},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.TwoHandMighty, ActorClass.Barbarian), Skills.Barbarian.Frenzy},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.TwoHandMighty, ActorClass.Barbarian), Skills.Barbarian.Cleave},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.TwoHandMighty, ActorClass.Barbarian), Skills.Barbarian.Bash},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.TwoHandMighty, ActorClass.Barbarian), Skills.Barbarian.WeaponThrow},

            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.TwoHandDaibo, ActorClass.Monk), Skills.Monk.SevensidedStrike},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.TwoHandDaibo, ActorClass.Monk), Skills.Monk.LashingTailKick},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.TwoHandDaibo, ActorClass.Monk), Skills.Monk.WaveOfLight},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.TwoHandDaibo, ActorClass.Monk), Skills.Monk.ExplodingPalm},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.TwoHandDaibo, ActorClass.Monk), Skills.Monk.TempestRush},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.TwoHandDaibo, ActorClass.Monk), Skills.Monk.FistsOfThunder},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.TwoHandDaibo, ActorClass.Monk), Skills.Monk.CripplingWave},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.TwoHandDaibo, ActorClass.Monk), Skills.Monk.WayOfTheHundredFists},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.TwoHandDaibo, ActorClass.Monk), Skills.Monk.DeadlyReach},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.TwoHandDaibo, ActorClass.Monk), Skills.Monk.DashingStrike},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.TwoHandDaibo, ActorClass.Monk), Skills.Monk.MysticAlly},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.TwoHandDaibo, ActorClass.Monk), Skills.Monk.SweepingWind},
            {new KeyValuePair<TinityItemType, ActorClass>(TinityItemType.TwoHandDaibo, ActorClass.Monk), Skills.Monk.CycloneStrike},

        };



    }
}
