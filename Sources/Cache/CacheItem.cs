﻿using System;
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
            DiaItem item = ((DiaItem)acd.AsRActor);
            CachedObject = item;
            Quality = DetermineQuality(ACDItem.ItemQualityLevel);
            ItemType = DetermineItemType(InternalName, item.CommonData.ItemType);
            BaseType = DetermineBaseType(ItemType);

            int standardRequiredLevel = (ACDItem.Stats.Level > 60) ? 60 : ACDItem.Stats.Level - 1;
            LevelReduction = standardRequiredLevel - ACDItem.Stats.RequiredLevel;

            Vitality = (int)Math.Floor(ACDItem.Stats.Vitality);
            Strength = (int)Math.Floor(ACDItem.Stats.Strength);
            Intelligence = (int)Math.Floor(ACDItem.Stats.Intelligence);
            Dexterity = (int)Math.Floor(ACDItem.Stats.Dexterity);
            ArcaneOnCrit = (int)Math.Floor(ACDItem.Stats.ArcaneOnCrit);
            Armor = (int)Math.Floor(ACDItem.Stats.Armor);
            ArmorBonus = (int)Math.Floor(ACDItem.Stats.ArmorBonus);
            ArmorTotal = (int)Math.Floor(ACDItem.Stats.ArmorTotal);
            AttackSpeedPercent = (int)Math.Floor(ACDItem.Stats.AttackSpeedPercent);
            BlockChance = (int)Math.Floor(ACDItem.Stats.BlockChance);
            CritDamagePercent = (int)Math.Floor(ACDItem.Stats.CritDamagePercent);
            CritPercent = ACDItem.Stats.CritPercent;
            DamageReductionPhysicalPercent = (int)Math.Floor(ACDItem.Stats.DamageReductionPhysicalPercent);
            GoldFind = (int)Math.Floor(ACDItem.Stats.GoldFind);
            HatredRegen = (int)Math.Floor(ACDItem.Stats.HatredRegen);
            HealthGlobeBonus = (int)Math.Floor(ACDItem.Stats.HealthGlobeBonus);
            HealthPerSecond = (int)Math.Floor(ACDItem.Stats.HealthPerSecond);
            HealthPerSpiritSpent = (int)Math.Floor(ACDItem.Stats.HealthPerSpiritSpent);
            LifeOnHit = (int)Math.Floor(ACDItem.Stats.LifeOnHit);
            LifePercent = (int)Math.Floor(ACDItem.Stats.LifePercent);
            LifeSteal = (int)Math.Floor(ACDItem.Stats.LifeSteal);
            MagicFind = (int)Math.Floor(ACDItem.Stats.MagicFind);
            ManaRegen = (int)Math.Floor(ACDItem.Stats.ManaRegen);
            MaxArcanePower = (int)Math.Floor(ACDItem.Stats.MaxArcanePower);
            MaxDamage = (int)Math.Floor(ACDItem.Stats.MaxDamage);
            MaxDiscipline = (int)Math.Floor(ACDItem.Stats.MaxDiscipline);
            MaxFury = (int)Math.Floor(ACDItem.Stats.MaxFury);
            MaxMana = (int)Math.Floor(ACDItem.Stats.MaxMana);
            MaxSpirit = (int)Math.Floor(ACDItem.Stats.MaxSpirit);
            MinDamage = (int)Math.Floor(ACDItem.Stats.MinDamage);
            MovementSpeed = (int)Math.Floor(ACDItem.Stats.MovementSpeed);
            PickUpRadius = (int)Math.Floor(ACDItem.Stats.PickUpRadius);
            ResistAll = (int)Math.Floor(ACDItem.Stats.ResistAll);
            ResistArcane = (int)Math.Floor(ACDItem.Stats.ResistArcane);
            ResistCold = (int)Math.Floor(ACDItem.Stats.ResistCold);
            ResistFire = (int)Math.Floor(ACDItem.Stats.ResistFire);
            ResistHoly = (int)Math.Floor(ACDItem.Stats.ResistHoly);
            ResistLightning = (int)Math.Floor(ACDItem.Stats.ResistLightning);
            ResistPhysical = (int)Math.Floor(ACDItem.Stats.ResistPhysical);
            ResistPoison = (int)Math.Floor(ACDItem.Stats.ResistPoison);
            SpiritRegen = (int)Math.Floor(ACDItem.Stats.SpiritRegen);
            Thorns = (int)Math.Floor(ACDItem.Stats.Thorns);
            WeaponAttacksPerSecond = (int)Math.Floor(ACDItem.Stats.WeaponAttacksPerSecond);
            WeaponDamagePerSecond = (int)Math.Floor(ACDItem.Stats.WeaponDamagePerSecond);
            WeaponDamageType = ACDItem.Stats.WeaponDamageType;
            WeaponMaxDamage = (int)Math.Floor(ACDItem.Stats.WeaponMaxDamage);
            WeaponMinDamage = (int)Math.Floor(ACDItem.Stats.WeaponMinDamage);

            LifeSteal = ACDItem.Stats.LifeSteal;

            Sockets = ACDItem.Stats.Sockets;
            FollowerSpecialType = ACDItem.FollowerSpecialType;
            Gold = ACDItem.Gold;
            IdentifyCost = ACDItem.IdentifyCost;
            InternalName = ACDItem.InternalName;
            InventoryColumn = ACDItem.InventoryColumn;
            InventoryRow = ACDItem.InventoryRow;
            IsTwoHand = ACDItem.IsTwoHand;
            IsTwoSquareItem = ACDItem.IsTwoSquareItem;
            IsUnidentified = ACDItem.IsUnidentified;
            ItemStackQuantity = ACDItem.ItemStackQuantity;
            MaxStackCount = ACDItem.MaxStackCount;
            Name = ACDItem.Name;
            Position = ACDItem.Position;
            Level = ACDItem.Stats.Level;

            if (Gold > 0)
                ShouldPickup = ShouldPickupGold(Gold);
            else
                ShouldPickup = ShouldPickupItem(ACDItem);

        }
        #endregion Constructors

        #region Properties
        private Db.ACDItem ACDItem
        {
            get
            {
                return CachedObject.CommonData;
            }
        }

        public Db.DiaItem CachedObject
        {
            get;
            private set;
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

        public int AttackSpeedPercent
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
        /// <summary>Clones this instance.</summary>
        /// <returns>Cloned instance of <see cref="CacheObject" />.</returns>
        public override CacheObject Clone()
        {
            CacheItem item = new CacheItem(CachedObject.CommonData);
            item.LastAccessDate = LastAccessDate;
            item.Type = Type;
            item.ItemType = ItemType;

            return item;
        }

        /// <summary>
        /// DetermineItemType - Calculates what kind of item it is from D3 internalnames
        /// </summary>
        /// <param name="name"></param>
        /// <param name="dbItemType"></param>
        /// <param name="dbFollowerType"></param>
        /// <returns></returns>
        private static GItemType DetermineItemType(string name, Db.ItemType dbItemType, Db.FollowerType dbFollowerType = Db.FollowerType.None)
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
        private static GItemBaseType DetermineBaseType(GItemType itemType)
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

        private static TrinityItemQuality DetermineQuality(Db.ItemQuality itemQuality)
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

        private static float CalculateScore(CacheItem cacheItem)
        {
            throw new NotImplementedException();
        }

        private static bool ShouldPickupGold(int GoldStackSize)
        {
            return GoldStackSize >= GilesTrinity.Settings.Loot.Pickup.MinimumGoldStack;
        }

        private static bool ShouldPickupItem(ACDItem item)
        {

            if (GilesTrinity.Settings.Loot.ItemFilterMode == global::GilesTrinity.Settings.Loot.ItemFilterMode.DemonBuddy)
            {
                return ItemManager.EvaluateItem(item, ItemManager.RuleType.PickUp);
            }
            /*
             * Add darkfriend77 pickup filter here
             */
            else
            {
                return GilesTrinity.GilesPickupItemValidation(item.Name, item.Level, item.ItemQualityLevel, item.GameBalanceId, item.ItemType, item.FollowerSpecialType, item.DynamicId);
            }

        }
        #endregion Methods
    }
}
