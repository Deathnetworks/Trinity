using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
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
                case Element.Any:

                    var fire = GetElementalDamage(item, Element.Fire);
                    if (fire > 0)
                        return fire;

                    var cold = GetElementalDamage(item, Element.Cold);
                    if (cold > 0)
                        return cold;

                    var lightning = GetElementalDamage(item, Element.Lightning);
                    if (lightning > 0)
                        return lightning;

                    var arcane = GetElementalDamage(item, Element.Arcane);
                    if (arcane > 0)
                        return arcane;

                    var poison = GetElementalDamage(item, Element.Poison);
                    if (poison > 0)
                        return poison;

                    var holy = GetElementalDamage(item, Element.Holy);
                    if (holy > 0)
                        return holy;

                    var physical = GetElementalDamage(item, Element.Physical);
                    if (physical > 0)
                        return physical;

                    break;
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
                var kvp = new KeyValuePair<TrinityItemType, ActorClass>(itemType, actorClass);

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

        public static List<Skill> GetSkillsForItemType(TrinityItemType itemType, ActorClass actorClass = ActorClass.Invalid)
        {
            var result = new List<Skill>();
            if (actorClass != ActorClass.Invalid)
            {
                var kvp = new KeyValuePair<TrinityItemType, ActorClass>(itemType, actorClass);
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
                    var kvp = new KeyValuePair<TrinityItemType, ActorClass>(itemType, ac);
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

            if (ItemPropertyLimitsByItemType.TryGetValue(new KeyValuePair<TrinityItemType, ItemProperty>(item.TrinityItemType,prop), out statRange))
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

            if (ItemPropertyLimitsByItemType.TryGetValue(new KeyValuePair<TrinityItemType, ItemProperty>(item.TrinityItemType, prop), out statRange))
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
            var props = ItemPropertyLimitsByItemType.Where(pair => pair.Key.Key == item.TrinityItemType).Select(pair => pair.Key.Value).ToList();
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
        public static List<object> GetItemPropertyVariants(ItemProperty prop, TrinityItemType itemType)
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
                        Element.Any.ToEnumValue()
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
        public static readonly Dictionary<KeyValuePair<TrinityItemType, ItemProperty>, ItemStatRange> ItemPropertyLimitsByItemType = new Dictionary<KeyValuePair<TrinityItemType, ItemProperty>, ItemStatRange>
        {

            // PrimaryStat

            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.Ring, ItemProperty.PrimaryStat), new ItemStatRange {AncientMax = 650, AncientMin = 550, Max = 500, Min = 416 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.Amulet, ItemProperty.PrimaryStat), new ItemStatRange {AncientMax = 1000, AncientMin = 825, Max = 750, Min = 626 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.Helm, ItemProperty.PrimaryStat), new ItemStatRange {AncientMax = 1000, AncientMin = 750, Max = 750, Min = 626 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.VoodooMask, ItemProperty.PrimaryStat), new ItemStatRange {AncientMax = 1000, AncientMin = 750, Max = 750, Min = 626 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.WizardHat, ItemProperty.PrimaryStat), new ItemStatRange {AncientMax = 1000, AncientMin = 750, Max = 750, Min = 626 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.SpiritStone, ItemProperty.PrimaryStat), new ItemStatRange {AncientMax = 1000, AncientMin = 750, Max = 750, Min = 626 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.Gloves, ItemProperty.PrimaryStat), new ItemStatRange {AncientMax = 1000, AncientMin = 825, Max = 750, Min = 626 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.Quiver, ItemProperty.PrimaryStat), new ItemStatRange {AncientMax = 1000, AncientMin = 825, Max = 750, Min = 650 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.Orb, ItemProperty.PrimaryStat), new ItemStatRange {AncientMax = 1000, AncientMin = 825, Max = 750, Min = 650 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.Mojo, ItemProperty.PrimaryStat), new ItemStatRange {AncientMax = 1000, AncientMin = 825, Max = 750, Min = 650 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.Shoulder, ItemProperty.PrimaryStat), new ItemStatRange {AncientMax = 650, AncientMin = 550, Max = 500, Min = 416 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.Chest, ItemProperty.PrimaryStat), new ItemStatRange {AncientMax = 650, AncientMin = 550, Max = 500, Min = 416 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.Cloak, ItemProperty.PrimaryStat), new ItemStatRange {AncientMax = 650, AncientMin = 550, Max = 500, Min = 416 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.Belt, ItemProperty.PrimaryStat), new ItemStatRange {AncientMax = 650, AncientMin = 550, Max = 500, Min = 416 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.MightyBelt, ItemProperty.PrimaryStat), new ItemStatRange {AncientMax = 650, AncientMin = 550, Max = 500, Min = 416 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.Bracer, ItemProperty.PrimaryStat), new ItemStatRange {AncientMax = 650, AncientMin = 550, Max = 500, Min = 416 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.Legs, ItemProperty.PrimaryStat), new ItemStatRange {AncientMax = 650, AncientMin = 550, Max = 500, Min = 416 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.Boots, ItemProperty.PrimaryStat), new ItemStatRange {AncientMax = 650, AncientMin = 550, Max = 500, Min = 416 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.MightyWeapon, ItemProperty.PrimaryStat), new ItemStatRange {AncientMax = 1000, AncientMin = 825, Max = 750, Min = 626 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.Wand, ItemProperty.PrimaryStat), new ItemStatRange {AncientMax = 1000, AncientMin = 825, Max = 750, Min = 626 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.Flail, ItemProperty.PrimaryStat), new ItemStatRange {AncientMax = 1000, AncientMin = 825, Max = 750, Min = 626 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.Axe, ItemProperty.PrimaryStat), new ItemStatRange {AncientMax = 1000, AncientMin = 825, Max = 750, Min = 626 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.HandCrossbow, ItemProperty.PrimaryStat), new ItemStatRange {AncientMax = 1000, AncientMin = 825, Max = 750, Min = 626 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.CeremonialKnife, ItemProperty.PrimaryStat), new ItemStatRange {AncientMax = 1000, AncientMin = 825, Max = 750, Min = 626 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.FistWeapon, ItemProperty.PrimaryStat), new ItemStatRange {AncientMax = 1000, AncientMin = 825, Max = 750, Min = 626 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.Mace, ItemProperty.PrimaryStat), new ItemStatRange {AncientMax = 1000, AncientMin = 825, Max = 750, Min = 626 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.Sword, ItemProperty.PrimaryStat), new ItemStatRange {AncientMax = 1000, AncientMin = 825, Max = 750, Min = 626 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.TwoHandDaibo, ItemProperty.PrimaryStat), new ItemStatRange {AncientMax = 1465, AncientMin = 1237, Max = 1125, Min = 946 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.TwoHandCrossbow, ItemProperty.PrimaryStat), new ItemStatRange {AncientMax = 1000, AncientMin = 825, Max = 500, Min = 416 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.TwoHandAxe, ItemProperty.PrimaryStat), new ItemStatRange {AncientMax = 1465, AncientMin = 1237, Max = 1125, Min = 946 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.TwoHandBow, ItemProperty.PrimaryStat), new ItemStatRange {AncientMax = 1000, AncientMin = 825, Max = 500, Min = 416 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.TwoHandFlail, ItemProperty.PrimaryStat), new ItemStatRange {AncientMax = 1465, AncientMin = 1237, Max = 1125, Min = 946 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.TwoHandMace, ItemProperty.PrimaryStat), new ItemStatRange {AncientMax = 1465, AncientMin = 1237, Max = 1125, Min = 946 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.TwoHandMighty, ItemProperty.PrimaryStat), new ItemStatRange {AncientMax = 1465, AncientMin = 1237, Max = 1125, Min = 946 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.TwoHandPolearm, ItemProperty.PrimaryStat), new ItemStatRange {AncientMax = 1465, AncientMin = 1237, Max = 1125, Min = 946 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.TwoHandStaff, ItemProperty.PrimaryStat), new ItemStatRange {AncientMax = 1465, AncientMin = 1237, Max = 1125, Min = 946 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.TwoHandSword, ItemProperty.PrimaryStat), new ItemStatRange {AncientMax = 1465, AncientMin = 1237, Max = 1125, Min = 946 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.Spear, ItemProperty.PrimaryStat), new ItemStatRange {AncientMax = 1000, AncientMin = 825, Max = 750, Min = 626 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.Shield, ItemProperty.PrimaryStat), new ItemStatRange {AncientMax = 650, AncientMin = 550, Max = 500, Min = 416 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.Dagger, ItemProperty.PrimaryStat), new ItemStatRange {AncientMax = 1000, AncientMin = 825, Max = 750, Min = 626 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.CrusaderShield, ItemProperty.PrimaryStat), new ItemStatRange {AncientMax = 650, AncientMin = 550, Max = 500, Min = 416 }},

            // CriticalDamage

            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.Ring, ItemProperty.CriticalHitDamage), new ItemStatRange { Max = 50, Min = 26 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.Amulet, ItemProperty.CriticalHitDamage), new ItemStatRange { Max = 100, Min = 51 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.Gloves, ItemProperty.CriticalHitDamage), new ItemStatRange { Max = 50, Min = 26 }},

            // CriticalChance

            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.Ring, ItemProperty.CriticalHitChance), new ItemStatRange { Max = 6, Min = 4.5 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.Amulet, ItemProperty.CriticalHitChance), new ItemStatRange { Max = 10, Min = 8 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.Helm, ItemProperty.CriticalHitChance), new ItemStatRange { Max = 6, Min = 4.5 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.VoodooMask, ItemProperty.CriticalHitChance), new ItemStatRange { Max = 6, Min = 4.5 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.WizardHat, ItemProperty.CriticalHitChance), new ItemStatRange { Max = 6, Min = 4.5 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.SpiritStone, ItemProperty.CriticalHitChance), new ItemStatRange { Max = 6, Min = 4.5 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.Gloves, ItemProperty.CriticalHitChance), new ItemStatRange { Max = 10, Min = 8 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.Quiver, ItemProperty.CriticalHitChance), new ItemStatRange { Max = 10, Min = 8 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.Orb, ItemProperty.CriticalHitChance), new ItemStatRange { Max = 10, Min = 8 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.Mojo, ItemProperty.CriticalHitChance), new ItemStatRange { Max = 10, Min = 8 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.Bracer, ItemProperty.CriticalHitChance), new ItemStatRange { Max = 6, Min = 4.5 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.Shield, ItemProperty.CriticalHitChance), new ItemStatRange { Max = 10, Min = 8 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.CrusaderShield, ItemProperty.CriticalHitChance), new ItemStatRange { Max = 10, Min = 8 }},

            // IAS

            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.Ring, ItemProperty.AttackSpeed), new ItemStatRange { Max = 7, Min = 5 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.Amulet, ItemProperty.AttackSpeed), new ItemStatRange { Max = 7, Min = 5 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.Gloves, ItemProperty.AttackSpeed), new ItemStatRange { Max = 7, Min = 5 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.Quiver, ItemProperty.AttackSpeed), new ItemStatRange { Max = 7, Min = 5 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.MightyWeapon, ItemProperty.AttackSpeed), new ItemStatRange { Max = 7, Min = 5 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.Wand, ItemProperty.AttackSpeed), new ItemStatRange { Max = 7, Min = 5 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.Flail, ItemProperty.AttackSpeed), new ItemStatRange { Max = 7, Min = 5 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.Axe, ItemProperty.AttackSpeed), new ItemStatRange { Max = 7, Min = 5 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.HandCrossbow, ItemProperty.AttackSpeed), new ItemStatRange { Max = 7, Min = 5 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.CeremonialKnife, ItemProperty.AttackSpeed), new ItemStatRange { Max = 7, Min = 5 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.FistWeapon, ItemProperty.AttackSpeed), new ItemStatRange { Max = 7, Min = 5 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.Mace, ItemProperty.AttackSpeed), new ItemStatRange { Max = 7, Min = 5 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.Sword, ItemProperty.AttackSpeed), new ItemStatRange { Max = 7, Min = 5 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.TwoHandDaibo, ItemProperty.AttackSpeed), new ItemStatRange { Max = 7, Min = 5 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.TwoHandCrossbow, ItemProperty.AttackSpeed), new ItemStatRange { Max = 7, Min = 5 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.TwoHandAxe, ItemProperty.AttackSpeed), new ItemStatRange { Max = 7, Min = 5 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.TwoHandBow, ItemProperty.AttackSpeed), new ItemStatRange { Max = 7, Min = 5 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.TwoHandFlail, ItemProperty.AttackSpeed), new ItemStatRange { Max = 7, Min = 5 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.TwoHandMace, ItemProperty.AttackSpeed), new ItemStatRange { Max = 7, Min = 5 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.TwoHandMighty, ItemProperty.AttackSpeed), new ItemStatRange { Max = 7, Min = 5 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.TwoHandPolearm, ItemProperty.AttackSpeed), new ItemStatRange { Max = 7, Min = 5 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.TwoHandStaff, ItemProperty.AttackSpeed), new ItemStatRange { Max = 7, Min = 5 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.TwoHandSword, ItemProperty.AttackSpeed), new ItemStatRange { Max = 7, Min = 5 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.Spear, ItemProperty.AttackSpeed), new ItemStatRange { Max = 7, Min = 5 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.Dagger, ItemProperty.AttackSpeed), new ItemStatRange { Max = 7, Min = 5 }},

            // Skill Damage acdItem.GetSkillDamageIncrease() method is fixed.

            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.Helm, ItemProperty.SkillDamage), new ItemStatRange { Max = 15, Min = 10 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.VoodooMask, ItemProperty.SkillDamage), new ItemStatRange { Max = 15, Min = 10 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.WizardHat, ItemProperty.SkillDamage), new ItemStatRange { Max = 15, Min = 10 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.SpiritStone, ItemProperty.SkillDamage), new ItemStatRange { Max = 15, Min = 10 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.Quiver, ItemProperty.SkillDamage), new ItemStatRange { Max = 15, Min = 10 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.Orb, ItemProperty.SkillDamage), new ItemStatRange { Max = 15, Min = 10 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.Mojo, ItemProperty.SkillDamage), new ItemStatRange { Max = 15, Min = 10 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.Shoulder, ItemProperty.SkillDamage), new ItemStatRange { Max = 15, Min = 10 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.Chest, ItemProperty.SkillDamage), new ItemStatRange { Max = 15, Min = 10 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.Cloak, ItemProperty.SkillDamage), new ItemStatRange { Max = 15, Min = 10 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.Belt, ItemProperty.SkillDamage), new ItemStatRange { Max = 15, Min = 10 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.MightyBelt, ItemProperty.SkillDamage), new ItemStatRange { Max = 15, Min = 10 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.Legs, ItemProperty.SkillDamage), new ItemStatRange { Max = 15, Min = 10 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.Boots, ItemProperty.SkillDamage), new ItemStatRange { Max = 15, Min = 10 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.MightyWeapon, ItemProperty.SkillDamage), new ItemStatRange { Max = 15, Min = 10 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.Wand, ItemProperty.SkillDamage), new ItemStatRange { Max = 15, Min = 10 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.Flail, ItemProperty.SkillDamage), new ItemStatRange { Max = 15, Min = 10 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.HandCrossbow, ItemProperty.SkillDamage), new ItemStatRange { Max = 15, Min = 10 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.FistWeapon, ItemProperty.SkillDamage), new ItemStatRange { Max = 15, Min = 10 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.TwoHandDaibo, ItemProperty.SkillDamage), new ItemStatRange { Max = 15, Min = 10 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.TwoHandMighty, ItemProperty.SkillDamage), new ItemStatRange { Max = 15, Min = 10 }},

            // Base Damage

            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.Orb, ItemProperty.BaseMaxDamage), new ItemStatRange { AncientMax = 600, AncientMin = 400, Max = 500, Min = 340 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.Mojo, ItemProperty.BaseMaxDamage), new ItemStatRange { AncientMax = 600, AncientMin = 400, Max = 500, Min = 340 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.MightyWeapon, ItemProperty.BaseMaxDamage), new ItemStatRange { AncientMax = 1940, AncientMin = 1318, Max = 1490, Min = 981 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.Wand, ItemProperty.BaseMaxDamage), new ItemStatRange { AncientMax = 1940, AncientMin = 1318, Max = 1490, Min = 981 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.Flail, ItemProperty.BaseMaxDamage), new ItemStatRange { AncientMax = 1940, AncientMin = 1318, Max = 1490, Min = 981 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.Axe, ItemProperty.BaseMaxDamage), new ItemStatRange { AncientMax = 1940, AncientMin = 1318, Max = 1490, Min = 981 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.HandCrossbow, ItemProperty.BaseMaxDamage), new ItemStatRange { AncientMax = 1940, AncientMin = 1318, Max = 1304, Min = 856 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.CeremonialKnife, ItemProperty.BaseMaxDamage), new ItemStatRange { AncientMax = 1940, AncientMin = 1318, Max = 1490, Min = 981 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.FistWeapon, ItemProperty.BaseMaxDamage), new ItemStatRange { AncientMax = 1940, AncientMin = 1318, Max = 1490, Min = 981 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.Mace, ItemProperty.BaseMaxDamage), new ItemStatRange { AncientMax = 1940, AncientMin = 1318, Max = 1490, Min = 981 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.Sword, ItemProperty.BaseMaxDamage), new ItemStatRange { AncientMax = 1940, AncientMin = 1318, Max = 1490, Min = 981 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.Spear, ItemProperty.BaseMaxDamage), new ItemStatRange { AncientMax = 1940, AncientMin = 1318, Max = 1490, Min = 981 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.Dagger, ItemProperty.BaseMaxDamage), new ItemStatRange { AncientMax = 1940, AncientMin = 1318, Max = 1490, Min = 981 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.TwoHandDaibo, ItemProperty.BaseMaxDamage), new ItemStatRange { AncientMax = 2325, AncientMin = 1582, Max = 1788, Min = 1177 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.TwoHandCrossbow, ItemProperty.BaseMaxDamage), new ItemStatRange { AncientMax = 1940, AncientMin = 1318, Max = 1788, Min = 1177 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.TwoHandAxe, ItemProperty.BaseMaxDamage), new ItemStatRange { AncientMax = 2325, AncientMin = 1582, Max = 1788, Min = 1177 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.TwoHandBow, ItemProperty.BaseMaxDamage), new ItemStatRange { AncientMax = 1940, AncientMin = 1318, Max = 1788, Min = 1177 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.TwoHandFlail, ItemProperty.BaseMaxDamage), new ItemStatRange { AncientMax = 2325, AncientMin = 1582, Max = 1788, Min = 1177 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.TwoHandMace, ItemProperty.BaseMaxDamage), new ItemStatRange { AncientMax = 2325, AncientMin = 1582, Max = 1788, Min = 1177 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.TwoHandMighty, ItemProperty.BaseMaxDamage), new ItemStatRange { AncientMax = 2325, AncientMin = 1582, Max = 1788, Min = 1177 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.TwoHandPolearm, ItemProperty.BaseMaxDamage), new ItemStatRange { AncientMax = 2325, AncientMin = 1582, Max = 1788, Min = 1177 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.TwoHandStaff, ItemProperty.BaseMaxDamage), new ItemStatRange { AncientMax = 2325, AncientMin = 1582, Max = 1788, Min = 1177 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.TwoHandSword, ItemProperty.BaseMaxDamage), new ItemStatRange { AncientMax = 2325, AncientMin = 1582, Max = 1788, Min = 1177 }},

            // Percent Damage

            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.MightyWeapon, ItemProperty.PercentDamage), new ItemStatRange {  Max = 10, Min = 6 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.Wand, ItemProperty.PercentDamage), new ItemStatRange {  Max = 10, Min = 6 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.Flail, ItemProperty.PercentDamage), new ItemStatRange {  Max = 10, Min = 6 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.Axe, ItemProperty.PercentDamage), new ItemStatRange {  Max = 10, Min = 6 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.HandCrossbow, ItemProperty.PercentDamage), new ItemStatRange {  Max = 10, Min = 6 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.CeremonialKnife, ItemProperty.PercentDamage), new ItemStatRange {  Max = 10, Min = 6 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.FistWeapon, ItemProperty.PercentDamage), new ItemStatRange {  Max = 10, Min = 6 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.Mace, ItemProperty.PercentDamage), new ItemStatRange {  Max = 10, Min = 6 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.Sword, ItemProperty.PercentDamage), new ItemStatRange {  Max = 10, Min = 6 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.Spear, ItemProperty.PercentDamage), new ItemStatRange {  Max = 10, Min = 6 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.Dagger, ItemProperty.PercentDamage), new ItemStatRange {  Max = 10, Min = 6 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.TwoHandDaibo, ItemProperty.PercentDamage), new ItemStatRange {  Max = 10, Min = 6 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.TwoHandCrossbow, ItemProperty.PercentDamage), new ItemStatRange { Max = 10, Min = 6 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.TwoHandAxe, ItemProperty.PercentDamage), new ItemStatRange {  Max = 10, Min = 6 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.TwoHandBow, ItemProperty.PercentDamage), new ItemStatRange { Max = 10, Min = 6 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.TwoHandFlail, ItemProperty.PercentDamage), new ItemStatRange {  Max = 10, Min = 6 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.TwoHandMace, ItemProperty.PercentDamage), new ItemStatRange {  Max = 10, Min = 6 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.TwoHandMighty, ItemProperty.PercentDamage), new ItemStatRange {  Max = 10, Min = 6 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.TwoHandPolearm, ItemProperty.PercentDamage), new ItemStatRange {  Max = 10, Min = 6 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.TwoHandStaff, ItemProperty.PercentDamage), new ItemStatRange {  Max = 10, Min = 6 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.TwoHandSword, ItemProperty.PercentDamage), new ItemStatRange {  Max = 10, Min = 6 }},

            // Sockets

            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.Ring, ItemProperty.Sockets), new ItemStatRange { Max = 1, Min = 0 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.Amulet, ItemProperty.Sockets), new ItemStatRange { Max = 1, Min = 0 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.Helm, ItemProperty.Sockets), new ItemStatRange { Max = 1, Min = 0 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.VoodooMask, ItemProperty.Sockets), new ItemStatRange { Max = 1, Min = 0 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.WizardHat, ItemProperty.Sockets), new ItemStatRange { Max = 1, Min = 0 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.SpiritStone, ItemProperty.Sockets), new ItemStatRange { Max = 1, Min = 0 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.Quiver, ItemProperty.Sockets), new ItemStatRange { Max = 1, Min = 0 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.Orb, ItemProperty.Sockets), new ItemStatRange { Max = 1, Min = 0 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.Mojo, ItemProperty.Sockets), new ItemStatRange { Max = 1, Min = 0 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.Chest, ItemProperty.Sockets), new ItemStatRange { Max = 3, Min = 0 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.Cloak, ItemProperty.Sockets), new ItemStatRange { Max = 3, Min = 0 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.Legs, ItemProperty.Sockets), new ItemStatRange { Max = 2, Min = 0 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.MightyWeapon, ItemProperty.Sockets), new ItemStatRange { Max = 1, Min = 0 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.Wand, ItemProperty.Sockets), new ItemStatRange { Max = 1, Min = 0 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.Flail, ItemProperty.Sockets), new ItemStatRange { Max = 1, Min = 0 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.Axe, ItemProperty.Sockets), new ItemStatRange { Max = 1, Min = 0 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.HandCrossbow, ItemProperty.Sockets), new ItemStatRange { Max = 1, Min = 0 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.CeremonialKnife, ItemProperty.Sockets), new ItemStatRange { Max = 1, Min = 0 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.FistWeapon, ItemProperty.Sockets), new ItemStatRange { Max = 1, Min = 0 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.Mace, ItemProperty.Sockets), new ItemStatRange { Max = 1, Min = 0 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.Sword, ItemProperty.Sockets), new ItemStatRange { Max = 1, Min = 0 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.TwoHandDaibo, ItemProperty.Sockets), new ItemStatRange { Max = 1, Min = 0 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.TwoHandCrossbow, ItemProperty.Sockets), new ItemStatRange { Max = 1, Min = 0 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.TwoHandAxe, ItemProperty.Sockets), new ItemStatRange { Max = 1, Min = 0 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.TwoHandBow, ItemProperty.Sockets), new ItemStatRange { Max = 1, Min = 0 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.TwoHandFlail, ItemProperty.Sockets), new ItemStatRange { Max = 1, Min = 0 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.TwoHandMace, ItemProperty.Sockets), new ItemStatRange { Max = 1, Min = 0 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.TwoHandMighty, ItemProperty.Sockets), new ItemStatRange { Max = 1, Min = 0 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.TwoHandPolearm, ItemProperty.Sockets), new ItemStatRange { Max = 1, Min = 0 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.TwoHandStaff, ItemProperty.Sockets), new ItemStatRange { Max = 1, Min = 0 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.TwoHandSword, ItemProperty.Sockets), new ItemStatRange { Max = 1, Min = 0 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.Spear, ItemProperty.Sockets), new ItemStatRange { Max = 1, Min = 0 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.Shield, ItemProperty.Sockets), new ItemStatRange { Max = 1, Min = 0 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.Dagger, ItemProperty.Sockets), new ItemStatRange { Max = 1, Min = 0 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.CrusaderShield, ItemProperty.Sockets), new ItemStatRange { Max = 1, Min = 0 }},
            
            // Resource Cost Reduction

            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.Ring, ItemProperty.ResourceCost), new ItemStatRange { Max = 8, Min = 5 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.Amulet, ItemProperty.ResourceCost), new ItemStatRange { Max = 8, Min = 5 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.Gloves, ItemProperty.ResourceCost), new ItemStatRange { Max = 8, Min = 5 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.Quiver, ItemProperty.ResourceCost), new ItemStatRange { Max = 8, Min = 5 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.Orb, ItemProperty.ResourceCost), new ItemStatRange { Max = 8, Min = 5 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.Mojo, ItemProperty.ResourceCost), new ItemStatRange {Max = 8, Min = 5 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.Shoulder, ItemProperty.ResourceCost), new ItemStatRange { Max = 8, Min = 5 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.MightyWeapon, ItemProperty.ResourceCost), new ItemStatRange { Max = 10, Min = 8 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.Wand, ItemProperty.ResourceCost), new ItemStatRange { Max = 10, Min = 8 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.Flail, ItemProperty.ResourceCost), new ItemStatRange { Max = 10, Min = 8 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.Axe, ItemProperty.ResourceCost), new ItemStatRange { Max = 10, Min = 8 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.HandCrossbow, ItemProperty.ResourceCost), new ItemStatRange { Max = 10, Min = 8 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.CeremonialKnife, ItemProperty.ResourceCost), new ItemStatRange { Max = 10, Min = 8 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.FistWeapon, ItemProperty.ResourceCost), new ItemStatRange { Max = 10, Min = 8 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.Mace, ItemProperty.ResourceCost), new ItemStatRange { Max = 10, Min = 8 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.Sword, ItemProperty.ResourceCost), new ItemStatRange { Max = 10, Min = 8 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.TwoHandDaibo, ItemProperty.ResourceCost), new ItemStatRange { Max = 10, Min = 8 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.TwoHandCrossbow, ItemProperty.ResourceCost), new ItemStatRange { Max = 10, Min = 8 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.TwoHandAxe, ItemProperty.ResourceCost), new ItemStatRange { Max = 10, Min = 8 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.TwoHandBow, ItemProperty.ResourceCost), new ItemStatRange { Max = 10, Min = 8 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.TwoHandFlail, ItemProperty.ResourceCost), new ItemStatRange { Max = 10, Min = 8 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.TwoHandMace, ItemProperty.ResourceCost), new ItemStatRange { Max = 10, Min = 8 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.TwoHandMighty, ItemProperty.ResourceCost), new ItemStatRange { Max = 10, Min = 8 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.TwoHandPolearm, ItemProperty.ResourceCost), new ItemStatRange { Max = 10, Min = 8 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.TwoHandStaff, ItemProperty.ResourceCost), new ItemStatRange { Max = 10, Min = 8 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.TwoHandSword, ItemProperty.ResourceCost), new ItemStatRange { Max = 10, Min = 8 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.Spear, ItemProperty.ResourceCost), new ItemStatRange { Max = 10, Min = 8 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.Dagger, ItemProperty.ResourceCost), new ItemStatRange { Max = 10, Min = 8 }},

            // Cooldown Reduction

            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.Ring, ItemProperty.Cooldown), new ItemStatRange { Max = 8, Min = 5 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.Amulet, ItemProperty.Cooldown), new ItemStatRange { Max = 8, Min = 5 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.Gloves, ItemProperty.Cooldown), new ItemStatRange { Max = 8, Min = 5 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.Quiver, ItemProperty.Cooldown), new ItemStatRange { Max = 8, Min = 5 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.Orb, ItemProperty.Cooldown), new ItemStatRange { Max = 8, Min = 5 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.Mojo, ItemProperty.Cooldown), new ItemStatRange {Max = 8, Min = 5 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.Shoulder, ItemProperty.Cooldown), new ItemStatRange { Max = 8, Min = 5 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.MightyWeapon, ItemProperty.Cooldown), new ItemStatRange { Max = 10, Min = 6 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.Wand, ItemProperty.Cooldown), new ItemStatRange { Max = 10, Min = 6 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.Flail, ItemProperty.Cooldown), new ItemStatRange { Max = 10, Min = 6 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.Axe, ItemProperty.Cooldown), new ItemStatRange { Max = 10, Min = 6 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.HandCrossbow, ItemProperty.Cooldown), new ItemStatRange { Max = 10, Min = 6 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.CeremonialKnife, ItemProperty.Cooldown), new ItemStatRange { Max = 10, Min = 6 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.FistWeapon, ItemProperty.Cooldown), new ItemStatRange { Max = 10, Min = 6 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.Mace, ItemProperty.Cooldown), new ItemStatRange { Max = 10, Min = 6 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.Sword, ItemProperty.Cooldown), new ItemStatRange { Max = 10, Min = 6 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.TwoHandDaibo, ItemProperty.Cooldown), new ItemStatRange { Max = 10, Min = 6 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.TwoHandCrossbow, ItemProperty.Cooldown), new ItemStatRange { Max = 10, Min = 6 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.TwoHandAxe, ItemProperty.Cooldown), new ItemStatRange { Max = 10, Min = 6 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.TwoHandBow, ItemProperty.Cooldown), new ItemStatRange { Max = 10, Min = 6 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.TwoHandFlail, ItemProperty.Cooldown), new ItemStatRange { Max = 10, Min = 6 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.TwoHandMace, ItemProperty.Cooldown), new ItemStatRange { Max = 10, Min = 6 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.TwoHandMighty, ItemProperty.Cooldown), new ItemStatRange { Max = 10, Min = 6 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.TwoHandPolearm, ItemProperty.Cooldown), new ItemStatRange { Max = 10, Min = 6 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.TwoHandStaff, ItemProperty.Cooldown), new ItemStatRange { Max = 10, Min = 6 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.TwoHandSword, ItemProperty.Cooldown), new ItemStatRange { Max = 10, Min = 6 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.Spear, ItemProperty.Cooldown), new ItemStatRange { Max = 10, Min = 6 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.Dagger, ItemProperty.Cooldown), new ItemStatRange { Max = 10, Min = 6 }},

            // Damage Against Elites

            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.MightyWeapon, ItemProperty.DamageAgainstElites), new ItemStatRange { Max = 8, Min = 5 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.Wand, ItemProperty.DamageAgainstElites), new ItemStatRange { Max = 8, Min = 5 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.Flail, ItemProperty.DamageAgainstElites), new ItemStatRange { Max = 8, Min = 5 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.Axe, ItemProperty.DamageAgainstElites), new ItemStatRange { Max = 8, Min = 5 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.HandCrossbow, ItemProperty.DamageAgainstElites), new ItemStatRange { Max = 8, Min = 5 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.CeremonialKnife, ItemProperty.DamageAgainstElites), new ItemStatRange { Max = 8, Min = 5 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.FistWeapon, ItemProperty.DamageAgainstElites), new ItemStatRange { Max = 8, Min = 5 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.Mace, ItemProperty.DamageAgainstElites), new ItemStatRange { Max = 8, Min = 5 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.Sword, ItemProperty.DamageAgainstElites), new ItemStatRange { Max = 8, Min = 5 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.TwoHandDaibo, ItemProperty.DamageAgainstElites), new ItemStatRange { Max = 10, Min = 9 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.TwoHandCrossbow, ItemProperty.DamageAgainstElites), new ItemStatRange { Max = 10, Min = 9 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.TwoHandAxe, ItemProperty.DamageAgainstElites), new ItemStatRange { Max = 10, Min = 9 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.TwoHandBow, ItemProperty.DamageAgainstElites), new ItemStatRange { Max = 10, Min = 9 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.TwoHandFlail, ItemProperty.DamageAgainstElites), new ItemStatRange { Max = 10, Min = 9 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.TwoHandMace, ItemProperty.DamageAgainstElites), new ItemStatRange { Max = 10, Min = 9 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.TwoHandMighty, ItemProperty.DamageAgainstElites), new ItemStatRange { Max = 10, Min = 9 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.TwoHandPolearm, ItemProperty.DamageAgainstElites), new ItemStatRange { Max = 10, Min = 9 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.TwoHandStaff, ItemProperty.DamageAgainstElites), new ItemStatRange { Max = 10, Min = 9 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.TwoHandSword, ItemProperty.DamageAgainstElites), new ItemStatRange { Max = 10, Min = 9 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.Spear, ItemProperty.DamageAgainstElites), new ItemStatRange { Max = 8, Min = 5 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.Shield, ItemProperty.DamageAgainstElites), new ItemStatRange { Max = 8, Min = 5 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.Dagger, ItemProperty.DamageAgainstElites), new ItemStatRange { Max = 8, Min = 5 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.CrusaderShield, ItemProperty.DamageAgainstElites), new ItemStatRange { Max = 8, Min = 5 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.Quiver, ItemProperty.DamageAgainstElites), new ItemStatRange { Max = 10, Min = 8 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.Orb, ItemProperty.DamageAgainstElites), new ItemStatRange { Max = 10, Min = 8 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.Mojo, ItemProperty.DamageAgainstElites), new ItemStatRange { Max = 10, Min = 8 }},

            // ElementalDamage

            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.Amulet, ItemProperty.ElementalDamage), new ItemStatRange { Max = 20, Min = 15 }},
            {new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.Bracer, ItemProperty.ElementalDamage), new ItemStatRange { Max = 20, Min = 15 }},


            //{new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.Ring, ItemProperty.CritcalHitDamage), new ItemStatRange { Max = 50, Min = 26 }},
            //{new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.Amulet, ItemProperty.CritcalHitDamage), new ItemStatRange { Max = 100, Min = 51 }},
            //{new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.Helm, ItemProperty.CritcalHitDamage), new ItemStatRange { Max = 500, Min = 416 }},
            //{new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.VoodooMask, ItemProperty.CritcalHitDamage), new ItemStatRange { Max = 500, Min = 416 }},
            //{new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.WizardHat, ItemProperty.CritcalHitDamage), new ItemStatRange { Max = 500, Min = 416 }},
            //{new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.SpiritStone, ItemProperty.CritcalHitDamage), new ItemStatRange { Max = 500, Min = 416 }},
            //{new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.Gloves, ItemProperty.CritcalHitDamage), new ItemStatRange { Max = 50, Min = 26 }},
            //{new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.Quiver, ItemProperty.CritcalHitDamage), new ItemStatRange { Max = 10, Min = 8 }},
            //{new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.Orb, ItemProperty.CritcalHitDamage), new ItemStatRange { Max = 10, Min = 8 }},
            //{new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.Mojo, ItemProperty.CritcalHitDamage), new ItemStatRange { Max = 10, Min = 8 }},
            //{new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.Shoulder, ItemProperty.CritcalHitDamage), new ItemStatRange { Max = 500, Min = 416 }},
            //{new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.Chest, ItemProperty.CritcalHitDamage), new ItemStatRange { Max = 500, Min = 416 }},
            //{new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.Cloak, ItemProperty.CritcalHitDamage), new ItemStatRange { Max = 500, Min = 416 }},
            //{new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.Belt, ItemProperty.CritcalHitDamage), new ItemStatRange { Max = 500, Min = 416 }},
            //{new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.MightyBelt, ItemProperty.CritcalHitDamage), new ItemStatRange { Max = 500, Min = 416 }},
            //{new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.Bracer, ItemProperty.CritcalHitDamage), new ItemStatRange { Max = 500, Min = 416 }},
            //{new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.Legs, ItemProperty.CritcalHitDamage), new ItemStatRange { Max = 500, Min = 416 }},
            //{new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.Boots, ItemProperty.CritcalHitDamage), new ItemStatRange { Max = 500, Min = 416 }},
            //{new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.MightyWeapon, ItemProperty.CritcalHitDamage), new ItemStatRange { Max = 500, Min = 416 }},
            //{new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.Wand, ItemProperty.CritcalHitDamage), new ItemStatRange { Max = 500, Min = 416 }},
            //{new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.Flail, ItemProperty.CritcalHitDamage), new ItemStatRange { Max = 500, Min = 416 }},
            //{new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.Axe, ItemProperty.CritcalHitDamage), new ItemStatRange { Max = 500, Min = 416 }},
            //{new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.HandCrossbow, ItemProperty.CritcalHitDamage), new ItemStatRange { Max = 500, Min = 416 }},
            //{new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.CeremonialKnife, ItemProperty.CritcalHitDamage), new ItemStatRange { Max = 500, Min = 416 }},
            //{new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.FistWeapon, ItemProperty.CritcalHitDamage), new ItemStatRange { Max = 500, Min = 416 }},
            //{new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.Mace, ItemProperty.CritcalHitDamage), new ItemStatRange { Max = 500, Min = 416 }},
            //{new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.Sword, ItemProperty.CritcalHitDamage), new ItemStatRange { Max = 500, Min = 416 }},
            //{new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.TwoHandDaibo, ItemProperty.CritcalHitDamage), new ItemStatRange { Max = 500, Min = 416 }},
            //{new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.TwoHandCrossbow, ItemProperty.CritcalHitDamage), new ItemStatRange { Max = 500, Min = 416 }},
            //{new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.TwoHandAxe, ItemProperty.CritcalHitDamage), new ItemStatRange { Max = 500, Min = 416 }},
            //{new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.TwoHandBow, ItemProperty.CritcalHitDamage), new ItemStatRange { Max = 500, Min = 416 }},
            //{new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.TwoHandFlail, ItemProperty.CritcalHitDamage), new ItemStatRange { Max = 500, Min = 416 }},
            //{new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.TwoHandMace, ItemProperty.CritcalHitDamage), new ItemStatRange { Max = 500, Min = 416 }},
            //{new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.TwoHandMighty, ItemProperty.CritcalHitDamage), new ItemStatRange { Max = 500, Min = 416 }},
            //{new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.TwoHandPolearm, ItemProperty.CritcalHitDamage), new ItemStatRange { Max = 500, Min = 416 }},
            //{new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.TwoHandStaff, ItemProperty.CritcalHitDamage), new ItemStatRange { Max = 500, Min = 416 }},
            //{new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.TwoHandSword, ItemProperty.CritcalHitDamage), new ItemStatRange { Max = 500, Min = 416 }},
            //{new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.Spear, ItemProperty.CritcalHitDamage), new ItemStatRange { Max = 500, Min = 416 }},
            //{new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.Shield, ItemProperty.CritcalHitDamage), new ItemStatRange { Max = 500, Min = 416 }},
            //{new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.Dagger, ItemProperty.CritcalHitDamage), new ItemStatRange { Max = 500, Min = 416 }},
            //{new KeyValuePair<TrinityItemType, ItemProperty>(TrinityItemType.CrusaderShield, ItemProperty.CritcalHitDamage), new ItemStatRange { Max = 500, Min = 416 }},

    
        };

        public static readonly LookupList<KeyValuePair<TrinityItemType, ActorClass>, Skill> SkillDamageByItemTypeAndClass = new LookupList<KeyValuePair<TrinityItemType, ActorClass>, Skill>
        {
            // Head Slot
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Helm, ActorClass.DemonHunter), Skills.DemonHunter.ClusterArrow},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Helm, ActorClass.DemonHunter), Skills.DemonHunter.Multishot},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Helm, ActorClass.DemonHunter), Skills.DemonHunter.ElementalArrow},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Helm, ActorClass.DemonHunter), Skills.DemonHunter.Strafe},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Helm, ActorClass.DemonHunter), Skills.DemonHunter.Chakram},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Helm, ActorClass.DemonHunter), Skills.DemonHunter.RapidFire},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Helm, ActorClass.DemonHunter), Skills.DemonHunter.Impale},

            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Helm, ActorClass.Barbarian), Skills.Barbarian.Whirlwind},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Helm, ActorClass.Barbarian), Skills.Barbarian.SeismicSlam},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Helm, ActorClass.Barbarian), Skills.Barbarian.AncientSpear},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Helm, ActorClass.Barbarian), Skills.Barbarian.HammerOfTheAncients},

            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Helm, ActorClass.Wizard), Skills.Wizard.Meteor},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Helm, ActorClass.Wizard), Skills.Wizard.Disintegrate},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Helm, ActorClass.Wizard), Skills.Wizard.EnergyTwister},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Helm, ActorClass.Wizard), Skills.Wizard.ArcaneTorrent},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Helm, ActorClass.Wizard), Skills.Wizard.WaveOfForce},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Helm, ActorClass.Wizard), Skills.Wizard.ArcaneOrb},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Helm, ActorClass.Wizard), Skills.Wizard.RayOfFrost},

            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Helm, ActorClass.Witchdoctor), Skills.WitchDoctor.AcidCloud},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Helm, ActorClass.Witchdoctor), Skills.WitchDoctor.SpiritBarrage},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Helm, ActorClass.Witchdoctor), Skills.WitchDoctor.ZombieCharger},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Helm, ActorClass.Witchdoctor), Skills.WitchDoctor.Sacrifice},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Helm, ActorClass.Witchdoctor), Skills.WitchDoctor.Firebats},

            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.VoodooMask, ActorClass.Witchdoctor), Skills.WitchDoctor.AcidCloud},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.VoodooMask, ActorClass.Witchdoctor), Skills.WitchDoctor.SpiritBarrage},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.VoodooMask, ActorClass.Witchdoctor), Skills.WitchDoctor.ZombieCharger},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.VoodooMask, ActorClass.Witchdoctor), Skills.WitchDoctor.Sacrifice},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.VoodooMask, ActorClass.Witchdoctor), Skills.WitchDoctor.Firebats},

            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Helm, ActorClass.Monk), Skills.Monk.TempestRush},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Helm, ActorClass.Monk), Skills.Monk.ExplodingPalm},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Helm, ActorClass.Monk), Skills.Monk.WaveOfLight},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Helm, ActorClass.Monk), Skills.Monk.LashingTailKick},

            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.SpiritStone, ActorClass.Monk), Skills.Monk.TempestRush},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.SpiritStone, ActorClass.Monk), Skills.Monk.ExplodingPalm},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.SpiritStone, ActorClass.Monk), Skills.Monk.WaveOfLight},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.SpiritStone, ActorClass.Monk), Skills.Monk.LashingTailKick},  

            // Shoulders

            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Shoulder, ActorClass.DemonHunter), Skills.DemonHunter.Companion},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Shoulder, ActorClass.DemonHunter), Skills.DemonHunter.RainOfVengeance},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Shoulder, ActorClass.DemonHunter), Skills.DemonHunter.Sentry},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Shoulder, ActorClass.DemonHunter), Skills.DemonHunter.SpikeTrap},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Shoulder, ActorClass.DemonHunter), Skills.DemonHunter.FanOfKnives},

            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Shoulder, ActorClass.Barbarian), Skills.Barbarian.Avalanche},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Shoulder, ActorClass.Barbarian), Skills.Barbarian.Earthquake},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Shoulder, ActorClass.Barbarian), Skills.Barbarian.Overpower},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Shoulder, ActorClass.Barbarian), Skills.Barbarian.Rend},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Shoulder, ActorClass.Barbarian), Skills.Barbarian.Revenge},

            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Shoulder, ActorClass.Wizard), Skills.Wizard.BlackHole},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Shoulder, ActorClass.Wizard), Skills.Wizard.Blizzard},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Shoulder, ActorClass.Wizard), Skills.Wizard.Familiar},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Shoulder, ActorClass.Wizard), Skills.Wizard.ExplosiveBlast},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Shoulder, ActorClass.Wizard), Skills.Wizard.Hydra},

            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Shoulder, ActorClass.Witchdoctor), Skills.WitchDoctor.FetishArmy},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Shoulder, ActorClass.Witchdoctor), Skills.WitchDoctor.Gargantuan},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Shoulder, ActorClass.Witchdoctor), Skills.WitchDoctor.WallOfZombies},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Shoulder, ActorClass.Witchdoctor), Skills.WitchDoctor.GraspOfTheDead},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Shoulder, ActorClass.Witchdoctor), Skills.WitchDoctor.Haunt},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Shoulder, ActorClass.Witchdoctor), Skills.WitchDoctor.LocustSwarm},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Shoulder, ActorClass.Witchdoctor), Skills.WitchDoctor.Piranhas},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Shoulder, ActorClass.Witchdoctor), Skills.WitchDoctor.SummonZombieDogs},

            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Shoulder, ActorClass.Monk), Skills.Monk.DashingStrike},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Shoulder, ActorClass.Monk), Skills.Monk.SevensidedStrike},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Shoulder, ActorClass.Monk), Skills.Monk.MysticAlly},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Shoulder, ActorClass.Monk), Skills.Monk.SweepingWind},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Shoulder, ActorClass.Monk), Skills.Monk.CycloneStrike},

            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Shoulder, ActorClass.Crusader), Skills.Crusader.FallingSword},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Shoulder, ActorClass.Crusader), Skills.Crusader.HeavensFury},

            // Chest

            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Chest, ActorClass.DemonHunter), Skills.DemonHunter.Companion},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Chest, ActorClass.DemonHunter), Skills.DemonHunter.RainOfVengeance},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Chest, ActorClass.DemonHunter), Skills.DemonHunter.Sentry},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Chest, ActorClass.DemonHunter), Skills.DemonHunter.SpikeTrap},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Chest, ActorClass.DemonHunter), Skills.DemonHunter.FanOfKnives},

            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Cloak, ActorClass.DemonHunter), Skills.DemonHunter.Companion},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Cloak, ActorClass.DemonHunter), Skills.DemonHunter.RainOfVengeance},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Cloak, ActorClass.DemonHunter), Skills.DemonHunter.Sentry},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Cloak, ActorClass.DemonHunter), Skills.DemonHunter.SpikeTrap},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Cloak, ActorClass.DemonHunter), Skills.DemonHunter.FanOfKnives},

            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Chest, ActorClass.Barbarian), Skills.Barbarian.Avalanche},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Chest, ActorClass.Barbarian), Skills.Barbarian.Earthquake},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Chest, ActorClass.Barbarian), Skills.Barbarian.Overpower},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Chest, ActorClass.Barbarian), Skills.Barbarian.Rend},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Chest, ActorClass.Barbarian), Skills.Barbarian.Revenge},

            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Chest, ActorClass.Wizard), Skills.Wizard.BlackHole},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Chest, ActorClass.Wizard), Skills.Wizard.Blizzard},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Chest, ActorClass.Wizard), Skills.Wizard.Familiar},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Chest, ActorClass.Wizard), Skills.Wizard.ExplosiveBlast},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Chest, ActorClass.Wizard), Skills.Wizard.Hydra},

            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Chest, ActorClass.Witchdoctor), Skills.WitchDoctor.FetishArmy},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Chest, ActorClass.Witchdoctor), Skills.WitchDoctor.Gargantuan},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Chest, ActorClass.Witchdoctor), Skills.WitchDoctor.WallOfZombies},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Chest, ActorClass.Witchdoctor), Skills.WitchDoctor.GraspOfTheDead},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Chest, ActorClass.Witchdoctor), Skills.WitchDoctor.Haunt},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Chest, ActorClass.Witchdoctor), Skills.WitchDoctor.LocustSwarm},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Chest, ActorClass.Witchdoctor), Skills.WitchDoctor.Piranhas},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Chest, ActorClass.Witchdoctor), Skills.WitchDoctor.SummonZombieDogs},

            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Chest, ActorClass.Monk), Skills.Monk.DashingStrike},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Chest, ActorClass.Monk), Skills.Monk.SevensidedStrike},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Chest, ActorClass.Monk), Skills.Monk.MysticAlly},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Chest, ActorClass.Monk), Skills.Monk.SweepingWind},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Chest, ActorClass.Monk), Skills.Monk.CycloneStrike},

            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Chest, ActorClass.Crusader), Skills.Crusader.FallingSword},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Chest, ActorClass.Crusader), Skills.Crusader.HeavensFury},

            // Belt

            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Belt, ActorClass.DemonHunter), Skills.DemonHunter.Grenade},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Belt, ActorClass.DemonHunter), Skills.DemonHunter.EvasiveFire},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Belt, ActorClass.DemonHunter), Skills.DemonHunter.Bolas},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Belt, ActorClass.DemonHunter), Skills.DemonHunter.EntanglingShot},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Belt, ActorClass.DemonHunter), Skills.DemonHunter.HungeringArrow},

            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Belt, ActorClass.Barbarian), Skills.Barbarian.Frenzy},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Belt, ActorClass.Barbarian), Skills.Barbarian.Cleave},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Belt, ActorClass.Barbarian), Skills.Barbarian.Bash},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Belt, ActorClass.Barbarian), Skills.Barbarian.WeaponThrow},

            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.MightyBelt, ActorClass.Barbarian), Skills.Barbarian.Frenzy},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.MightyBelt, ActorClass.Barbarian), Skills.Barbarian.Cleave},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.MightyBelt, ActorClass.Barbarian), Skills.Barbarian.Bash},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.MightyBelt, ActorClass.Barbarian), Skills.Barbarian.WeaponThrow},

            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Belt, ActorClass.Wizard), Skills.Wizard.Electrocute},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Belt, ActorClass.Wizard), Skills.Wizard.SpectralBlade},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Belt, ActorClass.Wizard), Skills.Wizard.ShockPulse},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Belt, ActorClass.Wizard), Skills.Wizard.MagicMissile},

            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Belt, ActorClass.Witchdoctor), Skills.WitchDoctor.Firebomb},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Belt, ActorClass.Witchdoctor), Skills.WitchDoctor.PlagueOfToads},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Belt, ActorClass.Witchdoctor), Skills.WitchDoctor.CorpseSpiders},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Belt, ActorClass.Witchdoctor), Skills.WitchDoctor.PoisonDart},

            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Belt, ActorClass.Monk), Skills.Monk.WayOfTheHundredFists},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Belt, ActorClass.Monk), Skills.Monk.CripplingWave},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Belt, ActorClass.Monk), Skills.Monk.DeadlyReach},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Belt, ActorClass.Monk), Skills.Monk.FistsOfThunder},

            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Belt, ActorClass.Crusader), Skills.Crusader.Punish},

            // Pants

            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Legs, ActorClass.DemonHunter), Skills.DemonHunter.Grenade},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Legs, ActorClass.DemonHunter), Skills.DemonHunter.EvasiveFire},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Legs, ActorClass.DemonHunter), Skills.DemonHunter.Bolas},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Legs, ActorClass.DemonHunter), Skills.DemonHunter.EntanglingShot},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Legs, ActorClass.DemonHunter), Skills.DemonHunter.HungeringArrow},

            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Legs, ActorClass.Barbarian), Skills.Barbarian.Frenzy},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Legs, ActorClass.Barbarian), Skills.Barbarian.Cleave},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Legs, ActorClass.Barbarian), Skills.Barbarian.Bash},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Legs, ActorClass.Barbarian), Skills.Barbarian.WeaponThrow},

            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Legs, ActorClass.Wizard), Skills.Wizard.Electrocute},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Legs, ActorClass.Wizard), Skills.Wizard.SpectralBlade},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Legs, ActorClass.Wizard), Skills.Wizard.ShockPulse},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Legs, ActorClass.Wizard), Skills.Wizard.MagicMissile},

            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Legs, ActorClass.Witchdoctor), Skills.WitchDoctor.Firebomb},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Legs, ActorClass.Witchdoctor), Skills.WitchDoctor.PlagueOfToads},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Legs, ActorClass.Witchdoctor), Skills.WitchDoctor.CorpseSpiders},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Legs, ActorClass.Witchdoctor), Skills.WitchDoctor.PoisonDart},

            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Legs, ActorClass.Monk), Skills.Monk.WayOfTheHundredFists},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Legs, ActorClass.Monk), Skills.Monk.CripplingWave},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Legs, ActorClass.Monk), Skills.Monk.DeadlyReach},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Legs, ActorClass.Monk), Skills.Monk.FistsOfThunder},

            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Legs, ActorClass.Crusader), Skills.Crusader.Punish},
            
            // Boots

            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Helm, ActorClass.DemonHunter), Skills.DemonHunter.ClusterArrow},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Helm, ActorClass.DemonHunter), Skills.DemonHunter.Multishot},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Helm, ActorClass.DemonHunter), Skills.DemonHunter.ElementalArrow},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Helm, ActorClass.DemonHunter), Skills.DemonHunter.Strafe},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Helm, ActorClass.DemonHunter), Skills.DemonHunter.Chakram},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Helm, ActorClass.DemonHunter), Skills.DemonHunter.RapidFire},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Helm, ActorClass.DemonHunter), Skills.DemonHunter.Impale},

            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Helm, ActorClass.Barbarian), Skills.Barbarian.Whirlwind},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Helm, ActorClass.Barbarian), Skills.Barbarian.SeismicSlam},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Helm, ActorClass.Barbarian), Skills.Barbarian.AncientSpear},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Helm, ActorClass.Barbarian), Skills.Barbarian.HammerOfTheAncients},

            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Helm, ActorClass.Wizard), Skills.Wizard.Meteor},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Helm, ActorClass.Wizard), Skills.Wizard.Disintegrate},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Helm, ActorClass.Wizard), Skills.Wizard.EnergyTwister},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Helm, ActorClass.Wizard), Skills.Wizard.ArcaneTorrent},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Helm, ActorClass.Wizard), Skills.Wizard.WaveOfForce},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Helm, ActorClass.Wizard), Skills.Wizard.ArcaneOrb},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Helm, ActorClass.Wizard), Skills.Wizard.RayOfFrost},

            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Helm, ActorClass.Witchdoctor), Skills.WitchDoctor.AcidCloud},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Helm, ActorClass.Witchdoctor), Skills.WitchDoctor.SpiritBarrage},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Helm, ActorClass.Witchdoctor), Skills.WitchDoctor.ZombieCharger},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Helm, ActorClass.Witchdoctor), Skills.WitchDoctor.Sacrifice},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Helm, ActorClass.Witchdoctor), Skills.WitchDoctor.Firebats},

            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Helm, ActorClass.Monk), Skills.Monk.TempestRush},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Helm, ActorClass.Monk), Skills.Monk.ExplodingPalm},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Helm, ActorClass.Monk), Skills.Monk.WaveOfLight},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Helm, ActorClass.Monk), Skills.Monk.LashingTailKick},  

            // Offhand

            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Quiver, ActorClass.DemonHunter), Skills.DemonHunter.ClusterArrow},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Quiver, ActorClass.DemonHunter), Skills.DemonHunter.Multishot},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Quiver, ActorClass.DemonHunter), Skills.DemonHunter.ElementalArrow},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Quiver, ActorClass.DemonHunter), Skills.DemonHunter.Strafe},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Quiver, ActorClass.DemonHunter), Skills.DemonHunter.Chakram},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Quiver, ActorClass.DemonHunter), Skills.DemonHunter.RapidFire},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Quiver, ActorClass.DemonHunter), Skills.DemonHunter.Impale},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Quiver, ActorClass.DemonHunter), Skills.DemonHunter.Sentry},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Quiver, ActorClass.DemonHunter), Skills.DemonHunter.RainOfVengeance},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Quiver, ActorClass.DemonHunter), Skills.DemonHunter.FanOfKnives},

            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Orb, ActorClass.Wizard), Skills.Wizard.EnergyTwister},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Orb, ActorClass.Wizard), Skills.Wizard.Electrocute},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Orb, ActorClass.Wizard), Skills.Wizard.ArcaneOrb},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Orb, ActorClass.Wizard), Skills.Wizard.Hydra},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Orb, ActorClass.Wizard), Skills.Wizard.MagicMissile},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Orb, ActorClass.Wizard), Skills.Wizard.Familiar},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Orb, ActorClass.Wizard), Skills.Wizard.RayOfFrost},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Orb, ActorClass.Wizard), Skills.Wizard.Meteor},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Orb, ActorClass.Wizard), Skills.Wizard.SpectralBlade},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Orb, ActorClass.Wizard), Skills.Wizard.WaveOfForce},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Orb, ActorClass.Wizard), Skills.Wizard.Disintegrate},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Orb, ActorClass.Wizard), Skills.Wizard.ArcaneTorrent},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Orb, ActorClass.Wizard), Skills.Wizard.ExplosiveBlast},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Orb, ActorClass.Wizard), Skills.Wizard.Blizzard},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Orb, ActorClass.Wizard), Skills.Wizard.BlackHole},

            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Mojo, ActorClass.Witchdoctor), Skills.WitchDoctor.SpiritBarrage},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Mojo, ActorClass.Witchdoctor), Skills.WitchDoctor.LocustSwarm},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Mojo, ActorClass.Witchdoctor), Skills.WitchDoctor.PlagueOfToads},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Mojo, ActorClass.Witchdoctor), Skills.WitchDoctor.Haunt},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Mojo, ActorClass.Witchdoctor), Skills.WitchDoctor.Firebats},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Mojo, ActorClass.Witchdoctor), Skills.WitchDoctor.GraspOfTheDead},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Mojo, ActorClass.Witchdoctor), Skills.WitchDoctor.Sacrifice},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Mojo, ActorClass.Witchdoctor), Skills.WitchDoctor.FetishArmy},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Mojo, ActorClass.Witchdoctor), Skills.WitchDoctor.ZombieCharger},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Mojo, ActorClass.Witchdoctor), Skills.WitchDoctor.CorpseSpiders},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Mojo, ActorClass.Witchdoctor), Skills.WitchDoctor.AcidCloud},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Mojo, ActorClass.Witchdoctor), Skills.WitchDoctor.PoisonDart},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Mojo, ActorClass.Witchdoctor), Skills.WitchDoctor.WallOfZombies},

            // One Hand Weapon

            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.MightyWeapon, ActorClass.Barbarian), Skills.Barbarian.WeaponThrow},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.MightyWeapon, ActorClass.Barbarian), Skills.Barbarian.AncientSpear},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Wand, ActorClass.Wizard), Skills.Wizard.Disintegrate},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Wand, ActorClass.Wizard), Skills.Wizard.SpectralBlade},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.Flail, ActorClass.Crusader), Skills.Crusader.BlessedHammer},

            // Two Hand Weapon

            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.TwoHandMighty, ActorClass.Barbarian), Skills.Barbarian.Whirlwind},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.TwoHandMighty, ActorClass.Barbarian), Skills.Barbarian.SeismicSlam},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.TwoHandMighty, ActorClass.Barbarian), Skills.Barbarian.AncientSpear},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.TwoHandMighty, ActorClass.Barbarian), Skills.Barbarian.HammerOfTheAncients},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.TwoHandMighty, ActorClass.Barbarian), Skills.Barbarian.Avalanche},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.TwoHandMighty, ActorClass.Barbarian), Skills.Barbarian.Earthquake},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.TwoHandMighty, ActorClass.Barbarian), Skills.Barbarian.Overpower},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.TwoHandMighty, ActorClass.Barbarian), Skills.Barbarian.Rend},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.TwoHandMighty, ActorClass.Barbarian), Skills.Barbarian.Revenge},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.TwoHandMighty, ActorClass.Barbarian), Skills.Barbarian.Frenzy},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.TwoHandMighty, ActorClass.Barbarian), Skills.Barbarian.Cleave},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.TwoHandMighty, ActorClass.Barbarian), Skills.Barbarian.Bash},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.TwoHandMighty, ActorClass.Barbarian), Skills.Barbarian.WeaponThrow},

            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.TwoHandDaibo, ActorClass.Monk), Skills.Monk.SevensidedStrike},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.TwoHandDaibo, ActorClass.Monk), Skills.Monk.LashingTailKick},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.TwoHandDaibo, ActorClass.Monk), Skills.Monk.WaveOfLight},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.TwoHandDaibo, ActorClass.Monk), Skills.Monk.ExplodingPalm},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.TwoHandDaibo, ActorClass.Monk), Skills.Monk.TempestRush},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.TwoHandDaibo, ActorClass.Monk), Skills.Monk.FistsOfThunder},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.TwoHandDaibo, ActorClass.Monk), Skills.Monk.CripplingWave},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.TwoHandDaibo, ActorClass.Monk), Skills.Monk.WayOfTheHundredFists},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.TwoHandDaibo, ActorClass.Monk), Skills.Monk.DeadlyReach},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.TwoHandDaibo, ActorClass.Monk), Skills.Monk.DashingStrike},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.TwoHandDaibo, ActorClass.Monk), Skills.Monk.MysticAlly},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.TwoHandDaibo, ActorClass.Monk), Skills.Monk.SweepingWind},
            {new KeyValuePair<TrinityItemType, ActorClass>(TrinityItemType.TwoHandDaibo, ActorClass.Monk), Skills.Monk.CycloneStrike},

        };



    }
}
