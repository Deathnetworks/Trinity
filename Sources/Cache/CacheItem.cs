using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Zeta;
using Zeta.Common;
using Db = Zeta.Internals.Actors;
using GilesTrinity;
using Zeta.Internals.Actors;
using Zeta.CommonBot;

namespace GilesTrinity.Cache
{
    internal class CacheItem : CacheObject
    {
        #region Fields
        private float? _Score;
        #endregion Fields

        #region Constructors
        /// <summary>
        /// Initializes a new instance of the <see cref="CacheItem" /> class.
        /// </summary>
        /// <param name="rActorGuid">The RActorGUID.</param>
        /// <param name="item">The item.</param>
        public CacheItem(ACD acd)
            : base(acd)
        {
            CacheType = CacheType.Item;
            ACDItem = (ACDItem)acd;
            InternalName = CacheObject.NameNumberRemover.Replace(ACDItem.InternalName, string.Empty);
            try
            {
                Quality = DetermineQuality(ACDItem.ItemQualityLevel);
            }
            catch { }
            try
            {
                ItemType = DetermineItemType(InternalName, ACDItem.ItemType);
            }
            catch { }
            BaseType = DetermineBaseType(ItemType);

            ComputeItemProperty(this);
        }
        #endregion Constructors

        #region Properties
        private Db.ACDItem ACDItem
        {
            get;
            set;
        }

        public GItemType ItemType
        {
            get;
            set;
        }

        public GItemBaseType BaseType
        {
            get;
            set;
        }

        public float Dexterity
        {
            get;
            set;
        }

        public float Intelligence
        {
            get;
            set;
        }

        public float Strength
        {
            get;
            set;
        }

        public int Vitality
        {
            get;
            set;
        }

        public int Level
        {
            get;
            set;
        }

        public int RequiredLevel
        {
            get;
            set;
        }

        public int LevelReduction
        {
            get;
            set;
        }

        public int ArcaneOnCrit
        {
            get;
            set;
        }

        public int Armor
        {
            get;
            set;
        }

        public int ArmorBonus
        {
            get;
            set;
        }

        public int ArmorTotal
        {
            get;
            set;
        }

        public float AttackSpeedPercent
        {
            get;
            set;
        }

        public int BlockChance
        {
            get;
            set;
        }

        public int CritDamagePercent
        {
            get;
            set;
        }

        public float CritPercent 
        {
            get;
            set;
        }

        public int DamageReductionPhysicalPercent
        {
            get;
            set;
        }

        public int GoldFind
        {
            get;
            set;
        }

        public int HatredRegen
        {
            get;
            set;
        }

        public int HealthGlobeBonus
        {
            get;
            set;
        }

        public int HealthPerSecond
        {
            get;
            set;
        }

        public int HealthPerSpiritSpent
        {
            get;
            set;
        }

        public int LifeOnHit
        {
            get;
            set;
        }

        public int LifePercent
        {
            get;
            set;
        }

        public float LifeSteal
        {
            get;
            set;
        }

        public int MagicFind
        {
            get;
            set;
        }

        public int ManaRegen
        {
            get;
            set;
        }

        public int MaxArcanePower
        {
            get;
            set;
        }

        public int MaxDamage
        {
            get;
            set;
        }

        public int MaxDiscipline
        {
            get;
            set;
        }

        public int MaxFury
        {
            get;
            set;
        }

        public int MaxMana
        {
            get;
            set;
        }

        public int MaxSpirit
        {
            get;
            set;
        }

        public int MinDamage
        {
            get;
            set;
        }

        public int MovementSpeed
        {
            get;
            set;
        }

        public int PickUpRadius
        {
            get;
            set;
        }

        public TrinityItemQuality Quality
        {
            get;
            set;
        }

        public int ResistAll
        {
            get;
            set;
        }

        public int ResistArcane
        {
            get;
            set;
        }

        public int ResistCold
        {
            get;
            set;
        }

        public int ResistFire
        {
            get;
            set;
        }

        public int ResistHoly
        {
            get;
            set;
        }

        public int ResistLightning
        {
            get;
            set;
        }

        public int ResistPhysical
        {
            get;
            set;
        }

        public int ResistPoison
        {
            get;
            set;
        }

        public int Sockets
        {
            get;
            set;
        }

        public int SpiritRegen
        {
            get;
            set;
        }

        public int Thorns
        {
            get;
            set;
        }

        public int WeaponAttacksPerSecond
        {
            get;
            set;
        }

        public int WeaponDamagePerSecond
        {
            get;
            set;
        }

        public DamageType WeaponDamageType
        {
            get;
            set;
        }

        public int WeaponMaxDamage
        {
            get;
            set;
        }

        public int WeaponMinDamage
        {
            get;
            set;
        }

        public Db.FollowerType FollowerSpecialType
        {
            get;
            set;
        }

        public int Gold
        {
            get;
            set;
        }

        public int IdentifyCost
        {
            get;
            set;
        }

        public int InventoryColumn
        {
            get;
            set;
        }

        public int InventoryRow
        {
            get;
            set;
        }

        public bool IsTwoHand
        {
            get;
            set;
        }

        public bool IsTwoSquareItem
        {
            get;
            set;
        }

        public bool IsUnidentified
        {
            get;
            set;
        }

        public int ItemStackQuantity
        {
            get;
            set;
        }

        public int MaxStackCount
        {
            get;
            set;
        }

        public bool ShouldPickup
        {
            get;
            set;
        }

        public override float RadiusDistance
        {
            get;
            set;
        }

        public override float Radius
        {
            get;
            set;

        }

        public override string IgnoreReason
        {
            get;
            set;

        }

        public override string IgnoreSubStep
        {
            get;
            set;

        }

        public float Distance 
        {
            get;
            set;
        }

        public float Score
        {
            get
            {
                if (!_Score.HasValue && !IsUnidentified)
                {
                    _Score = CalculateScore(this);
                }
                return _Score.GetValueOrDefault(-1);
            }
        }
        #endregion Properties

        #region Methods
        /// <summary>
        /// Clones this instance.
        /// </summary>
        /// <returns>Cloned instance of <see cref="CacheObject" />.</returns>
        public override CacheObject Clone()
        {
            return (CacheObject)this.MemberwiseClone();
        }

        /// <summary>
        /// DetermineItemType - Calculates what kind of item it is from D3 internalnames
        /// </summary>
        /// <param name="name"></param>
        /// <param name="dbItemType"></param>
        /// <param name="dbFollowerType"></param>
        /// <returns></returns>
        internal static GItemType DetermineItemType(string name, Db.ItemType dbItemType, Db.FollowerType dbFollowerType = Db.FollowerType.None)
        {
            name = name.ToLower();
            if (name.StartsWith("axe_")) return GItemType.Axe;
            if (name.StartsWith("ceremonialdagger_")) return GItemType.CeremonialKnife;
            if (name.StartsWith("handxbow_")) return GItemType.HandCrossbow;
            if (name.StartsWith("dagger_")) return GItemType.Dagger;
            if (name.StartsWith("fistweapon_")) return GItemType.FistWeapon;
            if (name.StartsWith("mace_")) return GItemType.Mace;
            if (name.StartsWith("mightyweapon_1h_")) return GItemType.MightyWeapon;
            if (name.StartsWith("spear_")) return GItemType.Spear;
            if (name.StartsWith("sword_")) return GItemType.Sword;
            if (name.StartsWith("wand_")) return GItemType.Wand;
            if (name.StartsWith("twohandedaxe_")) return GItemType.TwoHandAxe;
            if (name.StartsWith("bow_")) return GItemType.TwoHandBow;
            if (name.StartsWith("combatstaff_")) return GItemType.TwoHandDaibo;
            if (name.StartsWith("xbow_")) return GItemType.TwoHandCrossbow;
            if (name.StartsWith("twohandedmace_")) return GItemType.TwoHandMace;
            if (name.StartsWith("mightyweapon_2h_")) return GItemType.TwoHandMighty;
            if (name.StartsWith("polearm_")) return GItemType.TwoHandPolearm;
            if (name.StartsWith("staff_")) return GItemType.TwoHandStaff;
            if (name.StartsWith("twohandedsword_")) return GItemType.TwoHandSword;
            if (name.StartsWith("staffofcow")) return GItemType.StaffOfHerding;
            if (name.StartsWith("mojo_")) return GItemType.Mojo;
            if (name.StartsWith("orb_")) return GItemType.Orb;
            if (name.StartsWith("quiver_")) return GItemType.Quiver;
            if (name.StartsWith("shield_")) return GItemType.Shield;
            if (name.StartsWith("amulet_")) return GItemType.Amulet;
            if (name.StartsWith("ring_")) return GItemType.Ring;
            if (name.StartsWith("boots_")) return GItemType.Boots;
            if (name.StartsWith("bracers_")) return GItemType.Bracer;
            if (name.StartsWith("cloak_")) return GItemType.Cloak;
            if (name.StartsWith("gloves_")) return GItemType.Gloves;
            if (name.StartsWith("pants_")) return GItemType.Legs;
            if (name.StartsWith("barbbelt_")) return GItemType.MightyBelt;
            if (name.StartsWith("shoulderpads_")) return GItemType.Shoulder;
            if (name.StartsWith("spiritstone_")) return GItemType.SpiritStone;
            if (name.StartsWith("voodoomask_")) return GItemType.VoodooMask;
            if (name.StartsWith("wizardhat_")) return GItemType.WizardHat;
            if (name.StartsWith("lore_book_")) return GItemType.CraftTome;
            if (name.StartsWith("page_of_")) return GItemType.CraftTome;
            if (name.StartsWith("blacksmithstome")) return GItemType.CraftTome;
            if (name.StartsWith("ruby_")) return GItemType.Ruby;
            if (name.StartsWith("emerald_")) return GItemType.Emerald;
            if (name.StartsWith("topaz_")) return GItemType.Topaz;
            if (name.StartsWith("amethyst")) return GItemType.Amethyst;
            if (name.StartsWith("healthpotion_")) return GItemType.HealthPotion;
            if (name.StartsWith("followeritem_enchantress_")) return GItemType.FollowerEnchantress;
            if (name.StartsWith("followeritem_scoundrel_")) return GItemType.FollowerScoundrel;
            if (name.StartsWith("followeritem_templar_")) return GItemType.FollowerTemplar;
            if (name.StartsWith("craftingplan_")) return GItemType.CraftingPlan;
            if (name.StartsWith("dye_")) return GItemType.Dye;
            if (name.StartsWith("a1_")) return GItemType.SpecialItem;
            if (name.StartsWith("healthglobe")) return GItemType.HealthGlobe;

            // Follower item types
            if (name.StartsWith("jewelbox_") || dbItemType == Db.ItemType.FollowerSpecial)
            {
                if (dbFollowerType == Db.FollowerType.Scoundrel)
                    return GItemType.FollowerScoundrel;
                if (dbFollowerType == Db.FollowerType.Templar)
                    return GItemType.FollowerTemplar;
                if (dbFollowerType == Db.FollowerType.Enchantress)
                    return GItemType.FollowerEnchantress;
            }

            // Fall back on some partial DB item type checking 
            if (name.StartsWith("crafting_"))
            {
                if (dbItemType == Db.ItemType.CraftingPage) return GItemType.CraftTome;
                return GItemType.CraftingMaterial;
            }
            if (name.StartsWith("chestarmor_"))
            {
                if (dbItemType == Db.ItemType.Cloak) return GItemType.Cloak;
                return GItemType.Chest;
            }
            if (name.StartsWith("helm_"))
            {
                if (dbItemType == Db.ItemType.SpiritStone) return GItemType.SpiritStone;
                if (dbItemType == Db.ItemType.VoodooMask) return GItemType.VoodooMask;
                if (dbItemType == Db.ItemType.WizardHat) return GItemType.WizardHat;
                return GItemType.Helm;
            }
            if (name.StartsWith("helmcloth_"))
            {
                if (dbItemType == Db.ItemType.SpiritStone) return GItemType.SpiritStone;
                if (dbItemType == Db.ItemType.VoodooMask) return GItemType.VoodooMask;
                if (dbItemType == Db.ItemType.WizardHat) return GItemType.WizardHat;
                return GItemType.Helm;
            }
            if (name.StartsWith("belt_"))
            {
                if (dbItemType == Db.ItemType.MightyBelt) return GItemType.MightyBelt;
                return GItemType.Belt;
            }
            if (name.StartsWith("demonkey_") || name.StartsWith("demontrebuchetkey"))
            {
                return GItemType.InfernalKey;
            }

            // hax for fuimusbruce's horadric hamburger
            if (name.StartsWith("offHand_"))
            {
                return GItemType.Dagger;
            }

            // ORGANS QUICK HACK IN
            if (name.StartsWith("quest_"))
            {
                return GItemType.InfernalKey;
            }
            return GItemType.Unknown;
        }

        /// <summary>
        /// DetermineBaseType - Calculates a more generic, "basic" type of item
        /// </summary>
        /// <param name="itemType"></param>
        /// <returns></returns>
        internal static GItemBaseType DetermineBaseType(GItemType itemType)
        {
            GItemBaseType baseType = GItemBaseType.Unknown;
            if (itemType == GItemType.Axe || itemType == GItemType.CeremonialKnife || itemType == GItemType.Dagger ||
                itemType == GItemType.FistWeapon || itemType == GItemType.Mace || itemType == GItemType.MightyWeapon ||
                itemType == GItemType.Spear || itemType == GItemType.Sword || itemType == GItemType.Wand)
            {
                baseType = GItemBaseType.WeaponOneHand;
            }
            else if (itemType == GItemType.TwoHandDaibo || itemType == GItemType.TwoHandMace ||
                itemType == GItemType.TwoHandMighty || itemType == GItemType.TwoHandPolearm || itemType == GItemType.TwoHandStaff ||
                itemType == GItemType.TwoHandSword || itemType == GItemType.TwoHandAxe)
            {
                baseType = GItemBaseType.WeaponTwoHand;
            }
            else if (itemType == GItemType.TwoHandCrossbow || itemType == GItemType.HandCrossbow || itemType == GItemType.TwoHandBow)
            {
                baseType = GItemBaseType.WeaponRange;
            }
            else if (itemType == GItemType.Mojo || itemType == GItemType.Orb ||
                itemType == GItemType.Quiver || itemType == GItemType.Shield)
            {
                baseType = GItemBaseType.Offhand;
            }
            else if (itemType == GItemType.Boots || itemType == GItemType.Bracer || itemType == GItemType.Chest ||
                itemType == GItemType.Cloak || itemType == GItemType.Gloves || itemType == GItemType.Helm ||
                itemType == GItemType.Legs || itemType == GItemType.Shoulder || itemType == GItemType.SpiritStone ||
                itemType == GItemType.VoodooMask || itemType == GItemType.WizardHat || itemType == GItemType.Belt ||
                itemType == GItemType.MightyBelt)
            {
                baseType = GItemBaseType.Armor;
            }
            else if (itemType == GItemType.Amulet || itemType == GItemType.Ring)
            {
                baseType = GItemBaseType.Jewelry;
            }
            else if (itemType == GItemType.FollowerEnchantress || itemType == GItemType.FollowerScoundrel ||
                itemType == GItemType.FollowerTemplar)
            {
                baseType = GItemBaseType.FollowerItem;
            }
            else if (itemType == GItemType.CraftingMaterial || itemType == GItemType.CraftTome ||
                itemType == GItemType.SpecialItem || itemType == GItemType.CraftingPlan || itemType == GItemType.HealthPotion ||
                itemType == GItemType.Dye || itemType == GItemType.StaffOfHerding || itemType == GItemType.InfernalKey)
            {
                baseType = GItemBaseType.Misc;
            }
            else if (itemType == GItemType.Ruby || itemType == GItemType.Emerald || itemType == GItemType.Topaz ||
                itemType == GItemType.Amethyst)
            {
                baseType = GItemBaseType.Gem;
            }
            else if (itemType == GItemType.HealthGlobe)
            {
                baseType = GItemBaseType.HealthGlobe;
            }
            return baseType;
        }

        internal static TrinityItemQuality DetermineQuality(Db.ItemQuality itemQuality)
        {
            if (itemQuality < Db.ItemQuality.Magic1)
            {
                return TrinityItemQuality.Common;
            }
            else if (itemQuality < Db.ItemQuality.Rare4)
            {
                return TrinityItemQuality.Magic;
            }
            else if (itemQuality < Db.ItemQuality.Legendary)
            {
                return TrinityItemQuality.Rare;
            }
            else if (itemQuality < Db.ItemQuality.Special)
            {
                return TrinityItemQuality.Legendary;
            }
            else
            {
                return TrinityItemQuality.Set;
            }
        }

        internal static float CalculateScore(CacheItem cacheItem)
        {
        
            //TODO : Call Scoring method 
            throw new NotImplementedException();
        }

        internal static bool ShouldPickupGold(int GoldStackSize, float distance)
        {
            return GilesTrinity.Settings.Loot.Pickup.MinimumGoldStack == 0 || (GoldStackSize * 100f / GilesTrinity.Settings.Loot.Pickup.MinimumGoldStack) / (distance * 100 / 2000) >= 1;
            //return GoldStackSize >= GilesTrinity.Settings.Loot.Pickup.MinimumGoldStack;
        }

        internal static bool ShouldPickupItem(CacheItem item)
        {
            if (GilesTrinity.Settings.Loot.ItemFilterMode == global::GilesTrinity.Settings.Loot.ItemFilterMode.DemonBuddy && item.BaseType != GItemBaseType.HealthGlobe)
            {
                return ItemManager.EvaluateItem(item.ACDItem, ItemManager.RuleType.PickUp);
            }
            else if (GilesTrinity.Settings.Loot.ItemFilterMode == Settings.Loot.ItemFilterMode.TrinityWithItemRules && item.BaseType != GItemBaseType.Misc && item.BaseType != GItemBaseType.Gem && item.BaseType != GItemBaseType.HealthGlobe)
            {
                return ScriptedRules.RulesManager.ShouldPickup(item);
            }
            else
            {
                return GilesTrinity.GilesPickupItemValidation(item.ACDItem.InternalName, item.ACDItem.Level, item.ACDItem.ItemQualityLevel, item.ACDItem.GameBalanceId, item.ACDItem.ItemBaseType, item.ACDItem.ItemType, item.ACDItem.IsOneHand, item.ACDItem.IsTwoHand, item.ACDItem.FollowerSpecialType, item.ACDItem.DynamicId);
            }
        }

        internal static void ComputeItemProperty(CacheItem item)
        {
            try
            {
                ACDItem acd = item.ACDItem;
                int standardRequiredLevel = (acd.Stats.Level > 60) ? 60 : acd.Stats.Level - 1;
                item.LevelReduction = standardRequiredLevel - acd.Stats.RequiredLevel;

                item.Vitality = (int)Math.Floor(acd.Stats.Vitality);
                item.Strength = (int)Math.Floor(acd.Stats.Strength);
                item.Intelligence = (int)Math.Floor(acd.Stats.Intelligence);
                item.Dexterity = (int)Math.Floor(acd.Stats.Dexterity);
                item.ArcaneOnCrit = (int)Math.Floor(acd.Stats.ArcaneOnCrit);
                item.Armor = (int)Math.Floor(acd.Stats.Armor);
                item.ArmorBonus = (int)Math.Floor(acd.Stats.ArmorBonus);
                item.ArmorTotal = (int)Math.Floor(acd.Stats.ArmorTotal);
                item.AttackSpeedPercent = acd.Stats.AttackSpeedPercent;
                item.BlockChance = (int)Math.Floor(acd.Stats.BlockChance);
                item.CritDamagePercent = (int)Math.Floor(acd.Stats.CritDamagePercent);
                item.CritPercent = acd.Stats.CritPercent;
                item.DamageReductionPhysicalPercent = (int)Math.Floor(acd.Stats.DamageReductionPhysicalPercent);
                item.GoldFind = (int)Math.Floor(acd.Stats.GoldFind);
                item.HatredRegen = (int)Math.Floor(acd.Stats.HatredRegen);
                item.HealthGlobeBonus = (int)Math.Floor(acd.Stats.HealthGlobeBonus);
                item.HealthPerSecond = (int)Math.Floor(acd.Stats.HealthPerSecond);
                item.HealthPerSpiritSpent = (int)Math.Floor(acd.Stats.HealthPerSpiritSpent);
                item.LifeOnHit = (int)Math.Floor(acd.Stats.LifeOnHit);
                item.LifePercent = (int)Math.Floor(acd.Stats.LifePercent);
                item.LifeSteal = (int)Math.Floor(acd.Stats.LifeSteal);
                item.MagicFind = (int)Math.Floor(acd.Stats.MagicFind);
                item.ManaRegen = (int)Math.Floor(acd.Stats.ManaRegen);
                item.MaxArcanePower = (int)Math.Floor(acd.Stats.MaxArcanePower);
                item.MaxDamage = (int)Math.Floor(acd.Stats.MaxDamage);
                item.MaxDiscipline = (int)Math.Floor(acd.Stats.MaxDiscipline);
                item.MaxFury = (int)Math.Floor(acd.Stats.MaxFury);
                item.MaxMana = (int)Math.Floor(acd.Stats.MaxMana);
                item.MaxSpirit = (int)Math.Floor(acd.Stats.MaxSpirit);
                item.MinDamage = (int)Math.Floor(acd.Stats.MinDamage);
                item.MovementSpeed = (int)Math.Floor(acd.Stats.MovementSpeed);
                item.PickUpRadius = (int)Math.Floor(acd.Stats.PickUpRadius);
                item.ResistAll = (int)Math.Floor(acd.Stats.ResistAll);
                item.ResistArcane = (int)Math.Floor(acd.Stats.ResistArcane);
                item.ResistCold = (int)Math.Floor(acd.Stats.ResistCold);
                item.ResistFire = (int)Math.Floor(acd.Stats.ResistFire);
                item.ResistHoly = (int)Math.Floor(acd.Stats.ResistHoly);
                item.ResistLightning = (int)Math.Floor(acd.Stats.ResistLightning);
                item.ResistPhysical = (int)Math.Floor(acd.Stats.ResistPhysical);
                item.ResistPoison = (int)Math.Floor(acd.Stats.ResistPoison);
                item.SpiritRegen = (int)Math.Floor(acd.Stats.SpiritRegen);
                item.Thorns = (int)Math.Floor(acd.Stats.Thorns);
                item.WeaponAttacksPerSecond = (int)Math.Floor(acd.Stats.WeaponAttacksPerSecond);
                item.WeaponDamagePerSecond = (int)Math.Floor(acd.Stats.WeaponDamagePerSecond);
                item.WeaponDamageType = acd.Stats.WeaponDamageType;
                item.WeaponMaxDamage = (int)Math.Floor(acd.Stats.WeaponMaxDamage);
                item.WeaponMinDamage = (int)Math.Floor(acd.Stats.WeaponMinDamage);

                item.LifeSteal = acd.Stats.LifeSteal;

                item.Sockets = acd.Stats.Sockets;
                item.FollowerSpecialType = acd.FollowerSpecialType;
                item.Gold = acd.Gold;
                item.IdentifyCost = acd.IdentifyCost;
                item.InventoryColumn = acd.InventoryColumn;
                item.InventoryRow = acd.InventoryRow;
                item.IsTwoHand = acd.IsTwoHand;
                item.IsTwoSquareItem = acd.IsTwoSquareItem;
                item.IsUnidentified = acd.IsUnidentified;
                item.ItemStackQuantity = acd.ItemStackQuantity;
                item.MaxStackCount = acd.MaxStackCount;
                item.Name = acd.Name;
                item.Position = acd.Position;
                item.Level = acd.Stats.Level;
                item.Distance = acd.Position.Distance(ZetaDia.Me.Position);

                if (item.Gold > 0)
                    item.ShouldPickup = ShouldPickupGold(item.Gold, item.Distance);
                else
                    item.ShouldPickup = ShouldPickupItem(item);
            }
            catch { }
        }
        #endregion Methods
    }
}
