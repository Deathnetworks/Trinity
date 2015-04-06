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
            var min = item.Stats.MinDamageElemental;
            return (min != 0) ? (int) min : (int) item.Stats.MinDamage;
        }

        public static int GetMaxBaseDamage(ACDItem item)
        {
            var max = item.Stats.MaxDamageElemental;
            return (max != 0) ? (int)max : (int)item.Stats.MaxDamage;
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
                var kvp = new KeyValuePair<GItemType, ActorClass>(itemType, actorClass);

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

        public static List<Skill> GetSkillsForItemType(GItemType itemType, ActorClass actorClass = ActorClass.Invalid)
        {
            var result = new List<Skill>();
            if (actorClass != ActorClass.Invalid)
            {
                var kvp = new KeyValuePair<GItemType, ActorClass>(itemType, actorClass);
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
                    var kvp = new KeyValuePair<GItemType, ActorClass>(itemType, ac);
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

            if (ItemPropertyLimitsByItemType.TryGetValue(new KeyValuePair<GItemType, ItemProperty>(item.GItemType,prop), out statRange))
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

            if (ItemPropertyLimitsByItemType.TryGetValue(new KeyValuePair<GItemType, ItemProperty>(item.GItemType, prop), out statRange))
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
            var props = ItemPropertyLimitsByItemType.Where(pair => pair.Key.Key == item.GItemType).Select(pair => pair.Key.Value).ToList();
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
        public static List<object> GetItemPropertyVariants(ItemProperty prop, GItemType itemType)
        {
            var result = new List<object>();
            switch (prop)
            {
                case ItemProperty.SkillDamage:
                    var classRestriction = (Item.GetClassRestriction(itemType));
                    result = GetSkillsForItemType(itemType, classRestriction).Cast<object>().ToList();
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
            { new Tuple<Item, ItemProperty>(Legendary.AndarielsVisage, ItemProperty.FireSkills), new ItemStatRange { Max = 20, Min = 15 }},
            { new Tuple<Item, ItemProperty>(Legendary.AndarielsVisage, ItemProperty.AttackSpeed), new ItemStatRange { Max = 7, Min = 5 }},
            { new Tuple<Item, ItemProperty>(Legendary.SunKeeper, ItemProperty.DamageAgainstElites), new ItemStatRange { Max = 30, Min = 15 }},
            { new Tuple<Item, ItemProperty>(Legendary.Frostburn, ItemProperty.ColdSkills), new ItemStatRange { Max = 15, Min = 10 }},
            { new Tuple<Item, ItemProperty>(Legendary.ThundergodsVigor, ItemProperty.LightningSkills), new ItemStatRange { Max = 15, Min = 10 }},
            { new Tuple<Item, ItemProperty>(Legendary.SashOfKnives, ItemProperty.ResourceCost), new ItemStatRange { Max = 8, Min = 5 }},
            { new Tuple<Item, ItemProperty>(Legendary.StoneOfJordan, ItemProperty.DamageAgainstElites), new ItemStatRange { Max = 30, Min = 25 }},
            { new Tuple<Item, ItemProperty>(Legendary.Unity, ItemProperty.ResourceCost), new ItemStatRange { Max = 15, Min = 12 }},
            { new Tuple<Item, ItemProperty>(Legendary.HellcatWaistguard, ItemProperty.ResourceCost), new ItemStatRange { Max = 6, Min = 3 }},
            { new Tuple<Item, ItemProperty>(Legendary.SunKeeper, ItemProperty.ResourceCost), new ItemStatRange { Max = 30, Min = 15 }},
        };

        /// <summary>
        /// Properties that are ALWAYS available to the ItemType are listed here.  
        /// Determines if Property will be available in ItemList rules dropdown.     
        /// </summary>
        public static readonly Dictionary<KeyValuePair<GItemType, ItemProperty>, ItemStatRange> ItemPropertyLimitsByItemType = new Dictionary<KeyValuePair<GItemType, ItemProperty>, ItemStatRange>
        {

            // PrimaryStat

            {new KeyValuePair<GItemType, ItemProperty>(GItemType.Ring, ItemProperty.PrimaryStat), new ItemStatRange {AncientMax = 650, AncientMin = 550, Max = 500, Min = 416 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.Amulet, ItemProperty.PrimaryStat), new ItemStatRange {AncientMax = 1000, AncientMin = 825, Max = 750, Min = 626 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.Helm, ItemProperty.PrimaryStat), new ItemStatRange {AncientMax = 1000, AncientMin = 750, Max = 750, Min = 626 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.VoodooMask, ItemProperty.PrimaryStat), new ItemStatRange {AncientMax = 1000, AncientMin = 750, Max = 750, Min = 626 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.WizardHat, ItemProperty.PrimaryStat), new ItemStatRange {AncientMax = 1000, AncientMin = 750, Max = 750, Min = 626 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.SpiritStone, ItemProperty.PrimaryStat), new ItemStatRange {AncientMax = 1000, AncientMin = 750, Max = 750, Min = 626 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.Gloves, ItemProperty.PrimaryStat), new ItemStatRange {AncientMax = 1000, AncientMin = 825, Max = 750, Min = 626 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.Quiver, ItemProperty.PrimaryStat), new ItemStatRange {AncientMax = 1000, AncientMin = 825, Max = 750, Min = 650 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.Orb, ItemProperty.PrimaryStat), new ItemStatRange {AncientMax = 1000, AncientMin = 825, Max = 750, Min = 650 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.Mojo, ItemProperty.PrimaryStat), new ItemStatRange {AncientMax = 1000, AncientMin = 825, Max = 750, Min = 650 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.Shoulder, ItemProperty.PrimaryStat), new ItemStatRange {AncientMax = 650, AncientMin = 550, Max = 500, Min = 416 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.Chest, ItemProperty.PrimaryStat), new ItemStatRange {AncientMax = 650, AncientMin = 550, Max = 500, Min = 416 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.Cloak, ItemProperty.PrimaryStat), new ItemStatRange {AncientMax = 650, AncientMin = 550, Max = 500, Min = 416 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.Belt, ItemProperty.PrimaryStat), new ItemStatRange {AncientMax = 650, AncientMin = 550, Max = 500, Min = 416 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.MightyBelt, ItemProperty.PrimaryStat), new ItemStatRange {AncientMax = 650, AncientMin = 550, Max = 500, Min = 416 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.Bracer, ItemProperty.PrimaryStat), new ItemStatRange {AncientMax = 650, AncientMin = 550, Max = 500, Min = 416 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.Legs, ItemProperty.PrimaryStat), new ItemStatRange {AncientMax = 650, AncientMin = 550, Max = 500, Min = 416 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.Boots, ItemProperty.PrimaryStat), new ItemStatRange {AncientMax = 650, AncientMin = 550, Max = 500, Min = 416 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.MightyWeapon, ItemProperty.PrimaryStat), new ItemStatRange {AncientMax = 1000, AncientMin = 825, Max = 750, Min = 626 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.Wand, ItemProperty.PrimaryStat), new ItemStatRange {AncientMax = 1000, AncientMin = 825, Max = 750, Min = 626 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.Flail, ItemProperty.PrimaryStat), new ItemStatRange {AncientMax = 1000, AncientMin = 825, Max = 750, Min = 626 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.Axe, ItemProperty.PrimaryStat), new ItemStatRange {AncientMax = 1000, AncientMin = 825, Max = 750, Min = 626 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.HandCrossbow, ItemProperty.PrimaryStat), new ItemStatRange {AncientMax = 1000, AncientMin = 825, Max = 750, Min = 626 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.CeremonialKnife, ItemProperty.PrimaryStat), new ItemStatRange {AncientMax = 1000, AncientMin = 825, Max = 750, Min = 626 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.FistWeapon, ItemProperty.PrimaryStat), new ItemStatRange {AncientMax = 1000, AncientMin = 825, Max = 750, Min = 626 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.Mace, ItemProperty.PrimaryStat), new ItemStatRange {AncientMax = 1000, AncientMin = 825, Max = 750, Min = 626 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.Sword, ItemProperty.PrimaryStat), new ItemStatRange {AncientMax = 1000, AncientMin = 825, Max = 750, Min = 626 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.TwoHandDaibo, ItemProperty.PrimaryStat), new ItemStatRange {AncientMax = 1465, AncientMin = 1237, Max = 1125, Min = 946 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.TwoHandCrossbow, ItemProperty.PrimaryStat), new ItemStatRange {AncientMax = 1000, AncientMin = 825, Max = 500, Min = 416 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.TwoHandAxe, ItemProperty.PrimaryStat), new ItemStatRange {AncientMax = 1465, AncientMin = 1237, Max = 1125, Min = 946 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.TwoHandBow, ItemProperty.PrimaryStat), new ItemStatRange {AncientMax = 1000, AncientMin = 825, Max = 500, Min = 416 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.TwoHandFlail, ItemProperty.PrimaryStat), new ItemStatRange {AncientMax = 1465, AncientMin = 1237, Max = 1125, Min = 946 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.TwoHandMace, ItemProperty.PrimaryStat), new ItemStatRange {AncientMax = 1465, AncientMin = 1237, Max = 1125, Min = 946 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.TwoHandMighty, ItemProperty.PrimaryStat), new ItemStatRange {AncientMax = 1465, AncientMin = 1237, Max = 1125, Min = 946 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.TwoHandPolearm, ItemProperty.PrimaryStat), new ItemStatRange {AncientMax = 1465, AncientMin = 1237, Max = 1125, Min = 946 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.TwoHandStaff, ItemProperty.PrimaryStat), new ItemStatRange {AncientMax = 1465, AncientMin = 1237, Max = 1125, Min = 946 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.TwoHandSword, ItemProperty.PrimaryStat), new ItemStatRange {AncientMax = 1465, AncientMin = 1237, Max = 1125, Min = 946 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.Spear, ItemProperty.PrimaryStat), new ItemStatRange {AncientMax = 1000, AncientMin = 825, Max = 750, Min = 626 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.Shield, ItemProperty.PrimaryStat), new ItemStatRange {AncientMax = 650, AncientMin = 550, Max = 500, Min = 416 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.Dagger, ItemProperty.PrimaryStat), new ItemStatRange {AncientMax = 1000, AncientMin = 825, Max = 750, Min = 626 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.CrusaderShield, ItemProperty.PrimaryStat), new ItemStatRange {AncientMax = 650, AncientMin = 550, Max = 500, Min = 416 }},

            // CriticalDamage

            {new KeyValuePair<GItemType, ItemProperty>(GItemType.Ring, ItemProperty.CriticalHitDamage), new ItemStatRange { Max = 50, Min = 26 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.Amulet, ItemProperty.CriticalHitDamage), new ItemStatRange { Max = 100, Min = 51 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.Gloves, ItemProperty.CriticalHitDamage), new ItemStatRange { Max = 50, Min = 26 }},

            // CriticalChance

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

            // IAS

            {new KeyValuePair<GItemType, ItemProperty>(GItemType.Ring, ItemProperty.AttackSpeed), new ItemStatRange { Max = 7, Min = 5 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.Amulet, ItemProperty.AttackSpeed), new ItemStatRange { Max = 7, Min = 5 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.Gloves, ItemProperty.AttackSpeed), new ItemStatRange { Max = 7, Min = 5 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.Quiver, ItemProperty.AttackSpeed), new ItemStatRange { Max = 7, Min = 5 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.MightyWeapon, ItemProperty.AttackSpeed), new ItemStatRange { Max = 7, Min = 5 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.Wand, ItemProperty.AttackSpeed), new ItemStatRange { Max = 7, Min = 5 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.Flail, ItemProperty.AttackSpeed), new ItemStatRange { Max = 7, Min = 5 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.Axe, ItemProperty.AttackSpeed), new ItemStatRange { Max = 7, Min = 5 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.HandCrossbow, ItemProperty.AttackSpeed), new ItemStatRange { Max = 7, Min = 5 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.CeremonialKnife, ItemProperty.AttackSpeed), new ItemStatRange { Max = 7, Min = 5 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.FistWeapon, ItemProperty.AttackSpeed), new ItemStatRange { Max = 7, Min = 5 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.Mace, ItemProperty.AttackSpeed), new ItemStatRange { Max = 7, Min = 5 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.Sword, ItemProperty.AttackSpeed), new ItemStatRange { Max = 7, Min = 5 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.TwoHandDaibo, ItemProperty.AttackSpeed), new ItemStatRange { Max = 7, Min = 5 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.TwoHandCrossbow, ItemProperty.AttackSpeed), new ItemStatRange { Max = 7, Min = 5 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.TwoHandAxe, ItemProperty.AttackSpeed), new ItemStatRange { Max = 7, Min = 5 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.TwoHandBow, ItemProperty.AttackSpeed), new ItemStatRange { Max = 7, Min = 5 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.TwoHandFlail, ItemProperty.AttackSpeed), new ItemStatRange { Max = 7, Min = 5 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.TwoHandMace, ItemProperty.AttackSpeed), new ItemStatRange { Max = 7, Min = 5 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.TwoHandMighty, ItemProperty.AttackSpeed), new ItemStatRange { Max = 7, Min = 5 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.TwoHandPolearm, ItemProperty.AttackSpeed), new ItemStatRange { Max = 7, Min = 5 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.TwoHandStaff, ItemProperty.AttackSpeed), new ItemStatRange { Max = 7, Min = 5 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.TwoHandSword, ItemProperty.AttackSpeed), new ItemStatRange { Max = 7, Min = 5 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.Spear, ItemProperty.AttackSpeed), new ItemStatRange { Max = 7, Min = 5 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.Dagger, ItemProperty.AttackSpeed), new ItemStatRange { Max = 7, Min = 5 }},

            // Skill Damage acdItem.GetSkillDamageIncrease() method is fixed.
            // DISABLED until DB's 
            //{new KeyValuePair<GItemType, ItemProperty>(GItemType.Helm, ItemProperty.SkillDamage), new ItemStatRange { Max = 15, Min = 10 }},
            //{new KeyValuePair<GItemType, ItemProperty>(GItemType.VoodooMask, ItemProperty.SkillDamage), new ItemStatRange { Max = 15, Min = 10 }},
            //{new KeyValuePair<GItemType, ItemProperty>(GItemType.WizardHat, ItemProperty.SkillDamage), new ItemStatRange { Max = 15, Min = 10 }},
            //{new KeyValuePair<GItemType, ItemProperty>(GItemType.SpiritStone, ItemProperty.SkillDamage), new ItemStatRange { Max = 15, Min = 10 }},
            //{new KeyValuePair<GItemType, ItemProperty>(GItemType.Quiver, ItemProperty.SkillDamage), new ItemStatRange { Max = 15, Min = 10 }},
            //{new KeyValuePair<GItemType, ItemProperty>(GItemType.Orb, ItemProperty.SkillDamage), new ItemStatRange { Max = 15, Min = 10 }},
            //{new KeyValuePair<GItemType, ItemProperty>(GItemType.Mojo, ItemProperty.SkillDamage), new ItemStatRange { Max = 15, Min = 10 }},
            //{new KeyValuePair<GItemType, ItemProperty>(GItemType.Shoulder, ItemProperty.SkillDamage), new ItemStatRange { Max = 15, Min = 10 }},
            //{new KeyValuePair<GItemType, ItemProperty>(GItemType.Chest, ItemProperty.SkillDamage), new ItemStatRange { Max = 15, Min = 10 }},
            //{new KeyValuePair<GItemType, ItemProperty>(GItemType.Cloak, ItemProperty.SkillDamage), new ItemStatRange { Max = 15, Min = 10 }},
            //{new KeyValuePair<GItemType, ItemProperty>(GItemType.Belt, ItemProperty.SkillDamage), new ItemStatRange { Max = 15, Min = 10 }},
            //{new KeyValuePair<GItemType, ItemProperty>(GItemType.MightyBelt, ItemProperty.SkillDamage), new ItemStatRange { Max = 15, Min = 10 }},
            //{new KeyValuePair<GItemType, ItemProperty>(GItemType.Legs, ItemProperty.SkillDamage), new ItemStatRange { Max = 15, Min = 10 }},
            //{new KeyValuePair<GItemType, ItemProperty>(GItemType.Boots, ItemProperty.SkillDamage), new ItemStatRange { Max = 15, Min = 10 }},
            //{new KeyValuePair<GItemType, ItemProperty>(GItemType.MightyWeapon, ItemProperty.SkillDamage), new ItemStatRange { Max = 15, Min = 10 }},
            //{new KeyValuePair<GItemType, ItemProperty>(GItemType.Wand, ItemProperty.SkillDamage), new ItemStatRange { Max = 15, Min = 10 }},
            //{new KeyValuePair<GItemType, ItemProperty>(GItemType.Flail, ItemProperty.SkillDamage), new ItemStatRange { Max = 15, Min = 10 }},
            //{new KeyValuePair<GItemType, ItemProperty>(GItemType.HandCrossbow, ItemProperty.SkillDamage), new ItemStatRange { Max = 15, Min = 10 }},
            //{new KeyValuePair<GItemType, ItemProperty>(GItemType.FistWeapon, ItemProperty.SkillDamage), new ItemStatRange { Max = 15, Min = 10 }},
            //{new KeyValuePair<GItemType, ItemProperty>(GItemType.TwoHandDaibo, ItemProperty.SkillDamage), new ItemStatRange { Max = 15, Min = 10 }},
            //{new KeyValuePair<GItemType, ItemProperty>(GItemType.TwoHandMighty, ItemProperty.SkillDamage), new ItemStatRange { Max = 15, Min = 10 }},

            // Base Damage

            {new KeyValuePair<GItemType, ItemProperty>(GItemType.Orb, ItemProperty.BaseMaxDamage), new ItemStatRange { AncientMax = 600, AncientMin = 400, Max = 500, Min = 340 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.Mojo, ItemProperty.BaseMaxDamage), new ItemStatRange { AncientMax = 600, AncientMin = 400, Max = 500, Min = 340 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.MightyWeapon, ItemProperty.BaseMaxDamage), new ItemStatRange { AncientMax = 1940, AncientMin = 1318, Max = 1490, Min = 981 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.Wand, ItemProperty.BaseMaxDamage), new ItemStatRange { AncientMax = 1940, AncientMin = 1318, Max = 1490, Min = 981 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.Flail, ItemProperty.BaseMaxDamage), new ItemStatRange { AncientMax = 1940, AncientMin = 1318, Max = 1490, Min = 981 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.Axe, ItemProperty.BaseMaxDamage), new ItemStatRange { AncientMax = 1940, AncientMin = 1318, Max = 1490, Min = 981 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.HandCrossbow, ItemProperty.BaseMaxDamage), new ItemStatRange { AncientMax = 1940, AncientMin = 1318, Max = 1304, Min = 856 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.CeremonialKnife, ItemProperty.BaseMaxDamage), new ItemStatRange { AncientMax = 1940, AncientMin = 1318, Max = 1490, Min = 981 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.FistWeapon, ItemProperty.BaseMaxDamage), new ItemStatRange { AncientMax = 1940, AncientMin = 1318, Max = 1490, Min = 981 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.Mace, ItemProperty.BaseMaxDamage), new ItemStatRange { AncientMax = 1940, AncientMin = 1318, Max = 1490, Min = 981 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.Sword, ItemProperty.BaseMaxDamage), new ItemStatRange { AncientMax = 1940, AncientMin = 1318, Max = 1490, Min = 981 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.Spear, ItemProperty.BaseMaxDamage), new ItemStatRange { AncientMax = 1940, AncientMin = 1318, Max = 1490, Min = 981 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.Dagger, ItemProperty.BaseMaxDamage), new ItemStatRange { AncientMax = 1940, AncientMin = 1318, Max = 1490, Min = 981 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.TwoHandDaibo, ItemProperty.BaseMaxDamage), new ItemStatRange { AncientMax = 2325, AncientMin = 1582, Max = 1788, Min = 1177 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.TwoHandCrossbow, ItemProperty.BaseMaxDamage), new ItemStatRange { AncientMax = 1940, AncientMin = 1318, Max = 1788, Min = 1177 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.TwoHandAxe, ItemProperty.BaseMaxDamage), new ItemStatRange { AncientMax = 2325, AncientMin = 1582, Max = 1788, Min = 1177 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.TwoHandBow, ItemProperty.BaseMaxDamage), new ItemStatRange { AncientMax = 1940, AncientMin = 1318, Max = 1788, Min = 1177 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.TwoHandFlail, ItemProperty.BaseMaxDamage), new ItemStatRange { AncientMax = 2325, AncientMin = 1582, Max = 1788, Min = 1177 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.TwoHandMace, ItemProperty.BaseMaxDamage), new ItemStatRange { AncientMax = 2325, AncientMin = 1582, Max = 1788, Min = 1177 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.TwoHandMighty, ItemProperty.BaseMaxDamage), new ItemStatRange { AncientMax = 2325, AncientMin = 1582, Max = 1788, Min = 1177 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.TwoHandPolearm, ItemProperty.BaseMaxDamage), new ItemStatRange { AncientMax = 2325, AncientMin = 1582, Max = 1788, Min = 1177 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.TwoHandStaff, ItemProperty.BaseMaxDamage), new ItemStatRange { AncientMax = 2325, AncientMin = 1582, Max = 1788, Min = 1177 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.TwoHandSword, ItemProperty.BaseMaxDamage), new ItemStatRange { AncientMax = 2325, AncientMin = 1582, Max = 1788, Min = 1177 }},

            // Percent Damage

            {new KeyValuePair<GItemType, ItemProperty>(GItemType.MightyWeapon, ItemProperty.PercentDamage), new ItemStatRange {  Max = 10, Min = 6 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.Wand, ItemProperty.PercentDamage), new ItemStatRange {  Max = 10, Min = 6 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.Flail, ItemProperty.PercentDamage), new ItemStatRange {  Max = 10, Min = 6 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.Axe, ItemProperty.PercentDamage), new ItemStatRange {  Max = 10, Min = 6 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.HandCrossbow, ItemProperty.PercentDamage), new ItemStatRange {  Max = 10, Min = 6 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.CeremonialKnife, ItemProperty.PercentDamage), new ItemStatRange {  Max = 10, Min = 6 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.FistWeapon, ItemProperty.PercentDamage), new ItemStatRange {  Max = 10, Min = 6 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.Mace, ItemProperty.PercentDamage), new ItemStatRange {  Max = 10, Min = 6 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.Sword, ItemProperty.PercentDamage), new ItemStatRange {  Max = 10, Min = 6 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.Spear, ItemProperty.PercentDamage), new ItemStatRange {  Max = 10, Min = 6 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.Dagger, ItemProperty.PercentDamage), new ItemStatRange {  Max = 10, Min = 6 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.TwoHandDaibo, ItemProperty.PercentDamage), new ItemStatRange {  Max = 10, Min = 6 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.TwoHandCrossbow, ItemProperty.PercentDamage), new ItemStatRange { Max = 10, Min = 6 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.TwoHandAxe, ItemProperty.PercentDamage), new ItemStatRange {  Max = 10, Min = 6 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.TwoHandBow, ItemProperty.PercentDamage), new ItemStatRange { Max = 10, Min = 6 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.TwoHandFlail, ItemProperty.PercentDamage), new ItemStatRange {  Max = 10, Min = 6 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.TwoHandMace, ItemProperty.PercentDamage), new ItemStatRange {  Max = 10, Min = 6 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.TwoHandMighty, ItemProperty.PercentDamage), new ItemStatRange {  Max = 10, Min = 6 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.TwoHandPolearm, ItemProperty.PercentDamage), new ItemStatRange {  Max = 10, Min = 6 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.TwoHandStaff, ItemProperty.PercentDamage), new ItemStatRange {  Max = 10, Min = 6 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.TwoHandSword, ItemProperty.PercentDamage), new ItemStatRange {  Max = 10, Min = 6 }},

            // Sockets

            {new KeyValuePair<GItemType, ItemProperty>(GItemType.Ring, ItemProperty.Sockets), new ItemStatRange { Max = 1, Min = 0 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.Amulet, ItemProperty.Sockets), new ItemStatRange { Max = 1, Min = 0 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.Helm, ItemProperty.Sockets), new ItemStatRange { Max = 1, Min = 0 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.VoodooMask, ItemProperty.Sockets), new ItemStatRange { Max = 1, Min = 0 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.WizardHat, ItemProperty.Sockets), new ItemStatRange { Max = 1, Min = 0 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.SpiritStone, ItemProperty.Sockets), new ItemStatRange { Max = 1, Min = 0 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.Quiver, ItemProperty.Sockets), new ItemStatRange { Max = 1, Min = 0 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.Orb, ItemProperty.Sockets), new ItemStatRange { Max = 1, Min = 0 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.Mojo, ItemProperty.Sockets), new ItemStatRange { Max = 1, Min = 0 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.Chest, ItemProperty.Sockets), new ItemStatRange { Max = 3, Min = 0 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.Cloak, ItemProperty.Sockets), new ItemStatRange { Max = 3, Min = 0 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.Legs, ItemProperty.Sockets), new ItemStatRange { Max = 2, Min = 0 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.MightyWeapon, ItemProperty.Sockets), new ItemStatRange { Max = 1, Min = 0 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.Wand, ItemProperty.Sockets), new ItemStatRange { Max = 1, Min = 0 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.Flail, ItemProperty.Sockets), new ItemStatRange { Max = 1, Min = 0 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.Axe, ItemProperty.Sockets), new ItemStatRange { Max = 1, Min = 0 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.HandCrossbow, ItemProperty.Sockets), new ItemStatRange { Max = 1, Min = 0 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.CeremonialKnife, ItemProperty.Sockets), new ItemStatRange { Max = 1, Min = 0 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.FistWeapon, ItemProperty.Sockets), new ItemStatRange { Max = 1, Min = 0 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.Mace, ItemProperty.Sockets), new ItemStatRange { Max = 1, Min = 0 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.Sword, ItemProperty.Sockets), new ItemStatRange { Max = 1, Min = 0 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.TwoHandDaibo, ItemProperty.Sockets), new ItemStatRange { Max = 1, Min = 0 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.TwoHandCrossbow, ItemProperty.Sockets), new ItemStatRange { Max = 1, Min = 0 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.TwoHandAxe, ItemProperty.Sockets), new ItemStatRange { Max = 1, Min = 0 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.TwoHandBow, ItemProperty.Sockets), new ItemStatRange { Max = 1, Min = 0 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.TwoHandFlail, ItemProperty.Sockets), new ItemStatRange { Max = 1, Min = 0 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.TwoHandMace, ItemProperty.Sockets), new ItemStatRange { Max = 1, Min = 0 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.TwoHandMighty, ItemProperty.Sockets), new ItemStatRange { Max = 1, Min = 0 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.TwoHandPolearm, ItemProperty.Sockets), new ItemStatRange { Max = 1, Min = 0 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.TwoHandStaff, ItemProperty.Sockets), new ItemStatRange { Max = 1, Min = 0 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.TwoHandSword, ItemProperty.Sockets), new ItemStatRange { Max = 1, Min = 0 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.Spear, ItemProperty.Sockets), new ItemStatRange { Max = 1, Min = 0 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.Shield, ItemProperty.Sockets), new ItemStatRange { Max = 1, Min = 0 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.Dagger, ItemProperty.Sockets), new ItemStatRange { Max = 1, Min = 0 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.CrusaderShield, ItemProperty.Sockets), new ItemStatRange { Max = 1, Min = 0 }},
            
            // Resource Cost Reduction

            {new KeyValuePair<GItemType, ItemProperty>(GItemType.Ring, ItemProperty.ResourceCost), new ItemStatRange { Max = 8, Min = 5 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.Amulet, ItemProperty.ResourceCost), new ItemStatRange { Max = 8, Min = 5 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.Gloves, ItemProperty.ResourceCost), new ItemStatRange { Max = 8, Min = 5 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.Quiver, ItemProperty.ResourceCost), new ItemStatRange { Max = 8, Min = 5 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.Orb, ItemProperty.ResourceCost), new ItemStatRange { Max = 8, Min = 5 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.Mojo, ItemProperty.ResourceCost), new ItemStatRange {Max = 8, Min = 5 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.Shoulder, ItemProperty.ResourceCost), new ItemStatRange { Max = 8, Min = 5 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.MightyWeapon, ItemProperty.ResourceCost), new ItemStatRange { Max = 10, Min = 8 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.Wand, ItemProperty.ResourceCost), new ItemStatRange { Max = 10, Min = 8 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.Flail, ItemProperty.ResourceCost), new ItemStatRange { Max = 10, Min = 8 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.Axe, ItemProperty.ResourceCost), new ItemStatRange { Max = 10, Min = 8 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.HandCrossbow, ItemProperty.ResourceCost), new ItemStatRange { Max = 10, Min = 8 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.CeremonialKnife, ItemProperty.ResourceCost), new ItemStatRange { Max = 10, Min = 8 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.FistWeapon, ItemProperty.ResourceCost), new ItemStatRange { Max = 10, Min = 8 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.Mace, ItemProperty.ResourceCost), new ItemStatRange { Max = 10, Min = 8 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.Sword, ItemProperty.ResourceCost), new ItemStatRange { Max = 10, Min = 8 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.TwoHandDaibo, ItemProperty.ResourceCost), new ItemStatRange { Max = 10, Min = 8 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.TwoHandCrossbow, ItemProperty.ResourceCost), new ItemStatRange { Max = 10, Min = 8 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.TwoHandAxe, ItemProperty.ResourceCost), new ItemStatRange { Max = 10, Min = 8 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.TwoHandBow, ItemProperty.ResourceCost), new ItemStatRange { Max = 10, Min = 8 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.TwoHandFlail, ItemProperty.ResourceCost), new ItemStatRange { Max = 10, Min = 8 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.TwoHandMace, ItemProperty.ResourceCost), new ItemStatRange { Max = 10, Min = 8 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.TwoHandMighty, ItemProperty.ResourceCost), new ItemStatRange { Max = 10, Min = 8 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.TwoHandPolearm, ItemProperty.ResourceCost), new ItemStatRange { Max = 10, Min = 8 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.TwoHandStaff, ItemProperty.ResourceCost), new ItemStatRange { Max = 10, Min = 8 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.TwoHandSword, ItemProperty.ResourceCost), new ItemStatRange { Max = 10, Min = 8 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.Spear, ItemProperty.ResourceCost), new ItemStatRange { Max = 10, Min = 8 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.Dagger, ItemProperty.ResourceCost), new ItemStatRange { Max = 10, Min = 8 }},

            // Cooldown Reduction

            {new KeyValuePair<GItemType, ItemProperty>(GItemType.Ring, ItemProperty.Cooldown), new ItemStatRange { Max = 8, Min = 5 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.Amulet, ItemProperty.Cooldown), new ItemStatRange { Max = 8, Min = 5 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.Gloves, ItemProperty.Cooldown), new ItemStatRange { Max = 8, Min = 5 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.Quiver, ItemProperty.Cooldown), new ItemStatRange { Max = 8, Min = 5 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.Orb, ItemProperty.Cooldown), new ItemStatRange { Max = 8, Min = 5 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.Mojo, ItemProperty.Cooldown), new ItemStatRange {Max = 8, Min = 5 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.Shoulder, ItemProperty.Cooldown), new ItemStatRange { Max = 8, Min = 5 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.MightyWeapon, ItemProperty.Cooldown), new ItemStatRange { Max = 10, Min = 6 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.Wand, ItemProperty.Cooldown), new ItemStatRange { Max = 10, Min = 6 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.Flail, ItemProperty.Cooldown), new ItemStatRange { Max = 10, Min = 6 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.Axe, ItemProperty.Cooldown), new ItemStatRange { Max = 10, Min = 6 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.HandCrossbow, ItemProperty.Cooldown), new ItemStatRange { Max = 10, Min = 6 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.CeremonialKnife, ItemProperty.Cooldown), new ItemStatRange { Max = 10, Min = 6 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.FistWeapon, ItemProperty.Cooldown), new ItemStatRange { Max = 10, Min = 6 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.Mace, ItemProperty.Cooldown), new ItemStatRange { Max = 10, Min = 6 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.Sword, ItemProperty.Cooldown), new ItemStatRange { Max = 10, Min = 6 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.TwoHandDaibo, ItemProperty.Cooldown), new ItemStatRange { Max = 10, Min = 6 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.TwoHandCrossbow, ItemProperty.Cooldown), new ItemStatRange { Max = 10, Min = 6 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.TwoHandAxe, ItemProperty.Cooldown), new ItemStatRange { Max = 10, Min = 6 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.TwoHandBow, ItemProperty.Cooldown), new ItemStatRange { Max = 10, Min = 6 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.TwoHandFlail, ItemProperty.Cooldown), new ItemStatRange { Max = 10, Min = 6 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.TwoHandMace, ItemProperty.Cooldown), new ItemStatRange { Max = 10, Min = 6 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.TwoHandMighty, ItemProperty.Cooldown), new ItemStatRange { Max = 10, Min = 6 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.TwoHandPolearm, ItemProperty.Cooldown), new ItemStatRange { Max = 10, Min = 6 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.TwoHandStaff, ItemProperty.Cooldown), new ItemStatRange { Max = 10, Min = 6 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.TwoHandSword, ItemProperty.Cooldown), new ItemStatRange { Max = 10, Min = 6 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.Spear, ItemProperty.Cooldown), new ItemStatRange { Max = 10, Min = 6 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.Dagger, ItemProperty.Cooldown), new ItemStatRange { Max = 10, Min = 6 }},

            // Damage Against Elites

            {new KeyValuePair<GItemType, ItemProperty>(GItemType.MightyWeapon, ItemProperty.DamageAgainstElites), new ItemStatRange { Max = 8, Min = 5 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.Wand, ItemProperty.DamageAgainstElites), new ItemStatRange { Max = 8, Min = 5 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.Flail, ItemProperty.DamageAgainstElites), new ItemStatRange { Max = 8, Min = 5 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.Axe, ItemProperty.DamageAgainstElites), new ItemStatRange { Max = 8, Min = 5 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.HandCrossbow, ItemProperty.DamageAgainstElites), new ItemStatRange { Max = 8, Min = 5 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.CeremonialKnife, ItemProperty.DamageAgainstElites), new ItemStatRange { Max = 8, Min = 5 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.FistWeapon, ItemProperty.DamageAgainstElites), new ItemStatRange { Max = 8, Min = 5 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.Mace, ItemProperty.DamageAgainstElites), new ItemStatRange { Max = 8, Min = 5 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.Sword, ItemProperty.DamageAgainstElites), new ItemStatRange { Max = 8, Min = 5 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.TwoHandDaibo, ItemProperty.DamageAgainstElites), new ItemStatRange { Max = 10, Min = 9 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.TwoHandCrossbow, ItemProperty.DamageAgainstElites), new ItemStatRange { Max = 10, Min = 9 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.TwoHandAxe, ItemProperty.DamageAgainstElites), new ItemStatRange { Max = 10, Min = 9 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.TwoHandBow, ItemProperty.DamageAgainstElites), new ItemStatRange { Max = 10, Min = 9 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.TwoHandFlail, ItemProperty.DamageAgainstElites), new ItemStatRange { Max = 10, Min = 9 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.TwoHandMace, ItemProperty.DamageAgainstElites), new ItemStatRange { Max = 10, Min = 9 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.TwoHandMighty, ItemProperty.DamageAgainstElites), new ItemStatRange { Max = 10, Min = 9 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.TwoHandPolearm, ItemProperty.DamageAgainstElites), new ItemStatRange { Max = 10, Min = 9 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.TwoHandStaff, ItemProperty.DamageAgainstElites), new ItemStatRange { Max = 10, Min = 9 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.TwoHandSword, ItemProperty.DamageAgainstElites), new ItemStatRange { Max = 10, Min = 9 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.Spear, ItemProperty.DamageAgainstElites), new ItemStatRange { Max = 8, Min = 5 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.Shield, ItemProperty.DamageAgainstElites), new ItemStatRange { Max = 8, Min = 5 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.Dagger, ItemProperty.DamageAgainstElites), new ItemStatRange { Max = 8, Min = 5 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.CrusaderShield, ItemProperty.DamageAgainstElites), new ItemStatRange { Max = 8, Min = 5 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.Quiver, ItemProperty.DamageAgainstElites), new ItemStatRange { Max = 10, Min = 8 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.Orb, ItemProperty.DamageAgainstElites), new ItemStatRange { Max = 10, Min = 8 }},
            {new KeyValuePair<GItemType, ItemProperty>(GItemType.Mojo, ItemProperty.DamageAgainstElites), new ItemStatRange { Max = 10, Min = 8 }},


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

        public static readonly LookupList<KeyValuePair<GItemType, ActorClass>, Skill> SkillDamageByItemTypeAndClass = new LookupList<KeyValuePair<GItemType, ActorClass>, Skill>
        {
            // Head Slot
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Helm, ActorClass.DemonHunter), Skills.DemonHunter.ClusterArrow},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Helm, ActorClass.DemonHunter), Skills.DemonHunter.Multishot},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Helm, ActorClass.DemonHunter), Skills.DemonHunter.ElementalArrow},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Helm, ActorClass.DemonHunter), Skills.DemonHunter.Strafe},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Helm, ActorClass.DemonHunter), Skills.DemonHunter.Chakram},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Helm, ActorClass.DemonHunter), Skills.DemonHunter.RapidFire},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Helm, ActorClass.DemonHunter), Skills.DemonHunter.Impale},

            {new KeyValuePair<GItemType, ActorClass>(GItemType.Helm, ActorClass.Barbarian), Skills.Barbarian.Whirlwind},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Helm, ActorClass.Barbarian), Skills.Barbarian.SeismicSlam},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Helm, ActorClass.Barbarian), Skills.Barbarian.AncientSpear},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Helm, ActorClass.Barbarian), Skills.Barbarian.HammerOfTheAncients},

            {new KeyValuePair<GItemType, ActorClass>(GItemType.Helm, ActorClass.Wizard), Skills.Wizard.Meteor},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Helm, ActorClass.Wizard), Skills.Wizard.Disintegrate},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Helm, ActorClass.Wizard), Skills.Wizard.EnergyTwister},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Helm, ActorClass.Wizard), Skills.Wizard.ArcaneTorrent},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Helm, ActorClass.Wizard), Skills.Wizard.WaveOfForce},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Helm, ActorClass.Wizard), Skills.Wizard.ArcaneOrb},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Helm, ActorClass.Wizard), Skills.Wizard.RayOfFrost},

            {new KeyValuePair<GItemType, ActorClass>(GItemType.Helm, ActorClass.Witchdoctor), Skills.WitchDoctor.AcidCloud},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Helm, ActorClass.Witchdoctor), Skills.WitchDoctor.SpiritBarrage},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Helm, ActorClass.Witchdoctor), Skills.WitchDoctor.ZombieCharger},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Helm, ActorClass.Witchdoctor), Skills.WitchDoctor.Sacrifice},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Helm, ActorClass.Witchdoctor), Skills.WitchDoctor.Firebats},

            {new KeyValuePair<GItemType, ActorClass>(GItemType.VoodooMask, ActorClass.Witchdoctor), Skills.WitchDoctor.AcidCloud},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.VoodooMask, ActorClass.Witchdoctor), Skills.WitchDoctor.SpiritBarrage},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.VoodooMask, ActorClass.Witchdoctor), Skills.WitchDoctor.ZombieCharger},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.VoodooMask, ActorClass.Witchdoctor), Skills.WitchDoctor.Sacrifice},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.VoodooMask, ActorClass.Witchdoctor), Skills.WitchDoctor.Firebats},

            {new KeyValuePair<GItemType, ActorClass>(GItemType.Helm, ActorClass.Monk), Skills.Monk.TempestRush},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Helm, ActorClass.Monk), Skills.Monk.ExplodingPalm},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Helm, ActorClass.Monk), Skills.Monk.WaveOfLight},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Helm, ActorClass.Monk), Skills.Monk.LashingTailKick},

            {new KeyValuePair<GItemType, ActorClass>(GItemType.SpiritStone, ActorClass.Monk), Skills.Monk.TempestRush},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.SpiritStone, ActorClass.Monk), Skills.Monk.ExplodingPalm},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.SpiritStone, ActorClass.Monk), Skills.Monk.WaveOfLight},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.SpiritStone, ActorClass.Monk), Skills.Monk.LashingTailKick},  

            // Shoulders

            {new KeyValuePair<GItemType, ActorClass>(GItemType.Shoulder, ActorClass.DemonHunter), Skills.DemonHunter.Companion},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Shoulder, ActorClass.DemonHunter), Skills.DemonHunter.RainOfVengeance},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Shoulder, ActorClass.DemonHunter), Skills.DemonHunter.Sentry},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Shoulder, ActorClass.DemonHunter), Skills.DemonHunter.SpikeTrap},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Shoulder, ActorClass.DemonHunter), Skills.DemonHunter.FanOfKnives},

            {new KeyValuePair<GItemType, ActorClass>(GItemType.Shoulder, ActorClass.Barbarian), Skills.Barbarian.Avalanche},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Shoulder, ActorClass.Barbarian), Skills.Barbarian.Earthquake},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Shoulder, ActorClass.Barbarian), Skills.Barbarian.Overpower},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Shoulder, ActorClass.Barbarian), Skills.Barbarian.Rend},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Shoulder, ActorClass.Barbarian), Skills.Barbarian.Revenge},

            {new KeyValuePair<GItemType, ActorClass>(GItemType.Shoulder, ActorClass.Wizard), Skills.Wizard.BlackHole},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Shoulder, ActorClass.Wizard), Skills.Wizard.Blizzard},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Shoulder, ActorClass.Wizard), Skills.Wizard.Familiar},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Shoulder, ActorClass.Wizard), Skills.Wizard.ExplosiveBlast},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Shoulder, ActorClass.Wizard), Skills.Wizard.Hydra},

            {new KeyValuePair<GItemType, ActorClass>(GItemType.Shoulder, ActorClass.Witchdoctor), Skills.WitchDoctor.FetishArmy},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Shoulder, ActorClass.Witchdoctor), Skills.WitchDoctor.Gargantuan},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Shoulder, ActorClass.Witchdoctor), Skills.WitchDoctor.WallOfZombies},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Shoulder, ActorClass.Witchdoctor), Skills.WitchDoctor.GraspOfTheDead},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Shoulder, ActorClass.Witchdoctor), Skills.WitchDoctor.Haunt},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Shoulder, ActorClass.Witchdoctor), Skills.WitchDoctor.LocustSwarm},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Shoulder, ActorClass.Witchdoctor), Skills.WitchDoctor.Piranhas},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Shoulder, ActorClass.Witchdoctor), Skills.WitchDoctor.SummonZombieDogs},

            {new KeyValuePair<GItemType, ActorClass>(GItemType.Shoulder, ActorClass.Monk), Skills.Monk.DashingStrike},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Shoulder, ActorClass.Monk), Skills.Monk.SevensidedStrike},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Shoulder, ActorClass.Monk), Skills.Monk.MysticAlly},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Shoulder, ActorClass.Monk), Skills.Monk.SweepingWind},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Shoulder, ActorClass.Monk), Skills.Monk.CycloneStrike},

            {new KeyValuePair<GItemType, ActorClass>(GItemType.Shoulder, ActorClass.Crusader), Skills.Crusader.FallingSword},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Shoulder, ActorClass.Crusader), Skills.Crusader.HeavensFury},

            // Chest

            {new KeyValuePair<GItemType, ActorClass>(GItemType.Chest, ActorClass.DemonHunter), Skills.DemonHunter.Companion},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Chest, ActorClass.DemonHunter), Skills.DemonHunter.RainOfVengeance},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Chest, ActorClass.DemonHunter), Skills.DemonHunter.Sentry},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Chest, ActorClass.DemonHunter), Skills.DemonHunter.SpikeTrap},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Chest, ActorClass.DemonHunter), Skills.DemonHunter.FanOfKnives},

            {new KeyValuePair<GItemType, ActorClass>(GItemType.Cloak, ActorClass.DemonHunter), Skills.DemonHunter.Companion},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Cloak, ActorClass.DemonHunter), Skills.DemonHunter.RainOfVengeance},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Cloak, ActorClass.DemonHunter), Skills.DemonHunter.Sentry},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Cloak, ActorClass.DemonHunter), Skills.DemonHunter.SpikeTrap},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Cloak, ActorClass.DemonHunter), Skills.DemonHunter.FanOfKnives},

            {new KeyValuePair<GItemType, ActorClass>(GItemType.Chest, ActorClass.Barbarian), Skills.Barbarian.Avalanche},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Chest, ActorClass.Barbarian), Skills.Barbarian.Earthquake},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Chest, ActorClass.Barbarian), Skills.Barbarian.Overpower},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Chest, ActorClass.Barbarian), Skills.Barbarian.Rend},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Chest, ActorClass.Barbarian), Skills.Barbarian.Revenge},

            {new KeyValuePair<GItemType, ActorClass>(GItemType.Chest, ActorClass.Wizard), Skills.Wizard.BlackHole},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Chest, ActorClass.Wizard), Skills.Wizard.Blizzard},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Chest, ActorClass.Wizard), Skills.Wizard.Familiar},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Chest, ActorClass.Wizard), Skills.Wizard.ExplosiveBlast},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Chest, ActorClass.Wizard), Skills.Wizard.Hydra},

            {new KeyValuePair<GItemType, ActorClass>(GItemType.Chest, ActorClass.Witchdoctor), Skills.WitchDoctor.FetishArmy},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Chest, ActorClass.Witchdoctor), Skills.WitchDoctor.Gargantuan},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Chest, ActorClass.Witchdoctor), Skills.WitchDoctor.WallOfZombies},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Chest, ActorClass.Witchdoctor), Skills.WitchDoctor.GraspOfTheDead},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Chest, ActorClass.Witchdoctor), Skills.WitchDoctor.Haunt},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Chest, ActorClass.Witchdoctor), Skills.WitchDoctor.LocustSwarm},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Chest, ActorClass.Witchdoctor), Skills.WitchDoctor.Piranhas},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Chest, ActorClass.Witchdoctor), Skills.WitchDoctor.SummonZombieDogs},

            {new KeyValuePair<GItemType, ActorClass>(GItemType.Chest, ActorClass.Monk), Skills.Monk.DashingStrike},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Chest, ActorClass.Monk), Skills.Monk.SevensidedStrike},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Chest, ActorClass.Monk), Skills.Monk.MysticAlly},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Chest, ActorClass.Monk), Skills.Monk.SweepingWind},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Chest, ActorClass.Monk), Skills.Monk.CycloneStrike},

            {new KeyValuePair<GItemType, ActorClass>(GItemType.Chest, ActorClass.Crusader), Skills.Crusader.FallingSword},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Chest, ActorClass.Crusader), Skills.Crusader.HeavensFury},

            // Belt

            {new KeyValuePair<GItemType, ActorClass>(GItemType.Belt, ActorClass.DemonHunter), Skills.DemonHunter.Grenade},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Belt, ActorClass.DemonHunter), Skills.DemonHunter.EvasiveFire},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Belt, ActorClass.DemonHunter), Skills.DemonHunter.Bolas},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Belt, ActorClass.DemonHunter), Skills.DemonHunter.EntanglingShot},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Belt, ActorClass.DemonHunter), Skills.DemonHunter.HungeringArrow},

            {new KeyValuePair<GItemType, ActorClass>(GItemType.Belt, ActorClass.Barbarian), Skills.Barbarian.Frenzy},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Belt, ActorClass.Barbarian), Skills.Barbarian.Cleave},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Belt, ActorClass.Barbarian), Skills.Barbarian.Bash},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Belt, ActorClass.Barbarian), Skills.Barbarian.WeaponThrow},

            {new KeyValuePair<GItemType, ActorClass>(GItemType.MightyBelt, ActorClass.Barbarian), Skills.Barbarian.Frenzy},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.MightyBelt, ActorClass.Barbarian), Skills.Barbarian.Cleave},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.MightyBelt, ActorClass.Barbarian), Skills.Barbarian.Bash},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.MightyBelt, ActorClass.Barbarian), Skills.Barbarian.WeaponThrow},

            {new KeyValuePair<GItemType, ActorClass>(GItemType.Belt, ActorClass.Wizard), Skills.Wizard.Electrocute},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Belt, ActorClass.Wizard), Skills.Wizard.SpectralBlade},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Belt, ActorClass.Wizard), Skills.Wizard.ShockPulse},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Belt, ActorClass.Wizard), Skills.Wizard.MagicMissile},

            {new KeyValuePair<GItemType, ActorClass>(GItemType.Belt, ActorClass.Witchdoctor), Skills.WitchDoctor.Firebomb},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Belt, ActorClass.Witchdoctor), Skills.WitchDoctor.PlagueOfToads},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Belt, ActorClass.Witchdoctor), Skills.WitchDoctor.CorpseSpiders},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Belt, ActorClass.Witchdoctor), Skills.WitchDoctor.PoisonDart},

            {new KeyValuePair<GItemType, ActorClass>(GItemType.Belt, ActorClass.Monk), Skills.Monk.WayOfTheHundredFists},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Belt, ActorClass.Monk), Skills.Monk.CripplingWave},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Belt, ActorClass.Monk), Skills.Monk.DeadlyReach},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Belt, ActorClass.Monk), Skills.Monk.FistsOfThunder},

            {new KeyValuePair<GItemType, ActorClass>(GItemType.Belt, ActorClass.Crusader), Skills.Crusader.Punish},

            // Pants

            {new KeyValuePair<GItemType, ActorClass>(GItemType.Legs, ActorClass.DemonHunter), Skills.DemonHunter.Grenade},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Legs, ActorClass.DemonHunter), Skills.DemonHunter.EvasiveFire},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Legs, ActorClass.DemonHunter), Skills.DemonHunter.Bolas},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Legs, ActorClass.DemonHunter), Skills.DemonHunter.EntanglingShot},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Legs, ActorClass.DemonHunter), Skills.DemonHunter.HungeringArrow},

            {new KeyValuePair<GItemType, ActorClass>(GItemType.Legs, ActorClass.Barbarian), Skills.Barbarian.Frenzy},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Legs, ActorClass.Barbarian), Skills.Barbarian.Cleave},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Legs, ActorClass.Barbarian), Skills.Barbarian.Bash},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Legs, ActorClass.Barbarian), Skills.Barbarian.WeaponThrow},

            {new KeyValuePair<GItemType, ActorClass>(GItemType.Legs, ActorClass.Wizard), Skills.Wizard.Electrocute},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Legs, ActorClass.Wizard), Skills.Wizard.SpectralBlade},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Legs, ActorClass.Wizard), Skills.Wizard.ShockPulse},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Legs, ActorClass.Wizard), Skills.Wizard.MagicMissile},

            {new KeyValuePair<GItemType, ActorClass>(GItemType.Legs, ActorClass.Witchdoctor), Skills.WitchDoctor.Firebomb},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Legs, ActorClass.Witchdoctor), Skills.WitchDoctor.PlagueOfToads},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Legs, ActorClass.Witchdoctor), Skills.WitchDoctor.CorpseSpiders},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Legs, ActorClass.Witchdoctor), Skills.WitchDoctor.PoisonDart},

            {new KeyValuePair<GItemType, ActorClass>(GItemType.Legs, ActorClass.Monk), Skills.Monk.WayOfTheHundredFists},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Legs, ActorClass.Monk), Skills.Monk.CripplingWave},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Legs, ActorClass.Monk), Skills.Monk.DeadlyReach},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Legs, ActorClass.Monk), Skills.Monk.FistsOfThunder},

            {new KeyValuePair<GItemType, ActorClass>(GItemType.Legs, ActorClass.Crusader), Skills.Crusader.Punish},
            
            // Boots

            {new KeyValuePair<GItemType, ActorClass>(GItemType.Helm, ActorClass.DemonHunter), Skills.DemonHunter.ClusterArrow},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Helm, ActorClass.DemonHunter), Skills.DemonHunter.Multishot},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Helm, ActorClass.DemonHunter), Skills.DemonHunter.ElementalArrow},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Helm, ActorClass.DemonHunter), Skills.DemonHunter.Strafe},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Helm, ActorClass.DemonHunter), Skills.DemonHunter.Chakram},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Helm, ActorClass.DemonHunter), Skills.DemonHunter.RapidFire},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Helm, ActorClass.DemonHunter), Skills.DemonHunter.Impale},

            {new KeyValuePair<GItemType, ActorClass>(GItemType.Helm, ActorClass.Barbarian), Skills.Barbarian.Whirlwind},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Helm, ActorClass.Barbarian), Skills.Barbarian.SeismicSlam},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Helm, ActorClass.Barbarian), Skills.Barbarian.AncientSpear},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Helm, ActorClass.Barbarian), Skills.Barbarian.HammerOfTheAncients},

            {new KeyValuePair<GItemType, ActorClass>(GItemType.Helm, ActorClass.Wizard), Skills.Wizard.Meteor},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Helm, ActorClass.Wizard), Skills.Wizard.Disintegrate},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Helm, ActorClass.Wizard), Skills.Wizard.EnergyTwister},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Helm, ActorClass.Wizard), Skills.Wizard.ArcaneTorrent},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Helm, ActorClass.Wizard), Skills.Wizard.WaveOfForce},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Helm, ActorClass.Wizard), Skills.Wizard.ArcaneOrb},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Helm, ActorClass.Wizard), Skills.Wizard.RayOfFrost},

            {new KeyValuePair<GItemType, ActorClass>(GItemType.Helm, ActorClass.Witchdoctor), Skills.WitchDoctor.AcidCloud},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Helm, ActorClass.Witchdoctor), Skills.WitchDoctor.SpiritBarrage},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Helm, ActorClass.Witchdoctor), Skills.WitchDoctor.ZombieCharger},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Helm, ActorClass.Witchdoctor), Skills.WitchDoctor.Sacrifice},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Helm, ActorClass.Witchdoctor), Skills.WitchDoctor.Firebats},

            {new KeyValuePair<GItemType, ActorClass>(GItemType.Helm, ActorClass.Monk), Skills.Monk.TempestRush},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Helm, ActorClass.Monk), Skills.Monk.ExplodingPalm},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Helm, ActorClass.Monk), Skills.Monk.WaveOfLight},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Helm, ActorClass.Monk), Skills.Monk.LashingTailKick},  

            // Offhand

            {new KeyValuePair<GItemType, ActorClass>(GItemType.Quiver, ActorClass.DemonHunter), Skills.DemonHunter.ClusterArrow},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Quiver, ActorClass.DemonHunter), Skills.DemonHunter.Multishot},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Quiver, ActorClass.DemonHunter), Skills.DemonHunter.ElementalArrow},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Quiver, ActorClass.DemonHunter), Skills.DemonHunter.Strafe},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Quiver, ActorClass.DemonHunter), Skills.DemonHunter.Chakram},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Quiver, ActorClass.DemonHunter), Skills.DemonHunter.RapidFire},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Quiver, ActorClass.DemonHunter), Skills.DemonHunter.Impale},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Quiver, ActorClass.DemonHunter), Skills.DemonHunter.Sentry},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Quiver, ActorClass.DemonHunter), Skills.DemonHunter.RainOfVengeance},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Quiver, ActorClass.DemonHunter), Skills.DemonHunter.FanOfKnives},

            {new KeyValuePair<GItemType, ActorClass>(GItemType.Orb, ActorClass.Wizard), Skills.Wizard.EnergyTwister},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Orb, ActorClass.Wizard), Skills.Wizard.Electrocute},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Orb, ActorClass.Wizard), Skills.Wizard.ArcaneOrb},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Orb, ActorClass.Wizard), Skills.Wizard.Hydra},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Orb, ActorClass.Wizard), Skills.Wizard.MagicMissile},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Orb, ActorClass.Wizard), Skills.Wizard.Familiar},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Orb, ActorClass.Wizard), Skills.Wizard.RayOfFrost},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Orb, ActorClass.Wizard), Skills.Wizard.Meteor},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Orb, ActorClass.Wizard), Skills.Wizard.SpectralBlade},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Orb, ActorClass.Wizard), Skills.Wizard.WaveOfForce},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Orb, ActorClass.Wizard), Skills.Wizard.Disintegrate},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Orb, ActorClass.Wizard), Skills.Wizard.ArcaneTorrent},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Orb, ActorClass.Wizard), Skills.Wizard.ExplosiveBlast},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Orb, ActorClass.Wizard), Skills.Wizard.Blizzard},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Orb, ActorClass.Wizard), Skills.Wizard.BlackHole},

            {new KeyValuePair<GItemType, ActorClass>(GItemType.Mojo, ActorClass.Witchdoctor), Skills.WitchDoctor.SpiritBarrage},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Mojo, ActorClass.Witchdoctor), Skills.WitchDoctor.LocustSwarm},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Mojo, ActorClass.Witchdoctor), Skills.WitchDoctor.PlagueOfToads},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Mojo, ActorClass.Witchdoctor), Skills.WitchDoctor.Haunt},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Mojo, ActorClass.Witchdoctor), Skills.WitchDoctor.Firebats},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Mojo, ActorClass.Witchdoctor), Skills.WitchDoctor.GraspOfTheDead},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Mojo, ActorClass.Witchdoctor), Skills.WitchDoctor.Sacrifice},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Mojo, ActorClass.Witchdoctor), Skills.WitchDoctor.FetishArmy},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Mojo, ActorClass.Witchdoctor), Skills.WitchDoctor.ZombieCharger},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Mojo, ActorClass.Witchdoctor), Skills.WitchDoctor.CorpseSpiders},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Mojo, ActorClass.Witchdoctor), Skills.WitchDoctor.AcidCloud},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Mojo, ActorClass.Witchdoctor), Skills.WitchDoctor.PoisonDart},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Mojo, ActorClass.Witchdoctor), Skills.WitchDoctor.WallOfZombies},

            // One Hand Weapon

            {new KeyValuePair<GItemType, ActorClass>(GItemType.MightyWeapon, ActorClass.Barbarian), Skills.Barbarian.WeaponThrow},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.MightyWeapon, ActorClass.Barbarian), Skills.Barbarian.AncientSpear},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Wand, ActorClass.Wizard), Skills.Wizard.Disintegrate},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Wand, ActorClass.Wizard), Skills.Wizard.SpectralBlade},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.Flail, ActorClass.Crusader), Skills.Crusader.BlessedHammer},

            // Two Hand Weapon

            {new KeyValuePair<GItemType, ActorClass>(GItemType.TwoHandMighty, ActorClass.Barbarian), Skills.Barbarian.Whirlwind},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.TwoHandMighty, ActorClass.Barbarian), Skills.Barbarian.SeismicSlam},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.TwoHandMighty, ActorClass.Barbarian), Skills.Barbarian.AncientSpear},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.TwoHandMighty, ActorClass.Barbarian), Skills.Barbarian.HammerOfTheAncients},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.TwoHandMighty, ActorClass.Barbarian), Skills.Barbarian.Avalanche},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.TwoHandMighty, ActorClass.Barbarian), Skills.Barbarian.Earthquake},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.TwoHandMighty, ActorClass.Barbarian), Skills.Barbarian.Overpower},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.TwoHandMighty, ActorClass.Barbarian), Skills.Barbarian.Rend},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.TwoHandMighty, ActorClass.Barbarian), Skills.Barbarian.Revenge},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.TwoHandMighty, ActorClass.Barbarian), Skills.Barbarian.Frenzy},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.TwoHandMighty, ActorClass.Barbarian), Skills.Barbarian.Cleave},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.TwoHandMighty, ActorClass.Barbarian), Skills.Barbarian.Bash},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.TwoHandMighty, ActorClass.Barbarian), Skills.Barbarian.WeaponThrow},

            {new KeyValuePair<GItemType, ActorClass>(GItemType.TwoHandDaibo, ActorClass.Monk), Skills.Monk.SevensidedStrike},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.TwoHandDaibo, ActorClass.Monk), Skills.Monk.LashingTailKick},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.TwoHandDaibo, ActorClass.Monk), Skills.Monk.WaveOfLight},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.TwoHandDaibo, ActorClass.Monk), Skills.Monk.ExplodingPalm},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.TwoHandDaibo, ActorClass.Monk), Skills.Monk.TempestRush},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.TwoHandDaibo, ActorClass.Monk), Skills.Monk.FistsOfThunder},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.TwoHandDaibo, ActorClass.Monk), Skills.Monk.CripplingWave},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.TwoHandDaibo, ActorClass.Monk), Skills.Monk.WayOfTheHundredFists},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.TwoHandDaibo, ActorClass.Monk), Skills.Monk.DeadlyReach},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.TwoHandDaibo, ActorClass.Monk), Skills.Monk.DashingStrike},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.TwoHandDaibo, ActorClass.Monk), Skills.Monk.MysticAlly},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.TwoHandDaibo, ActorClass.Monk), Skills.Monk.SweepingWind},
            {new KeyValuePair<GItemType, ActorClass>(GItemType.TwoHandDaibo, ActorClass.Monk), Skills.Monk.CycloneStrike},

        };

    }
}
