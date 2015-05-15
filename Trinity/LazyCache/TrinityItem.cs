using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Trinity.Items;
using Zeta.Game;
using Zeta.Game.Internals;
using Zeta.Game.Internals.Actors;
using Logger = Trinity.Technicals.Logger;

namespace Trinity.LazyCache
{
    /// <summary>
    /// Trinity Item
    /// </summary>
    public class TrinityItem : TrinityObject
    {
        public TrinityItem() { }

        public TrinityItem(DiaObject rActor) : base(rActor) { }

        #region Fields

        private readonly CacheField<int> _ancientRank = new CacheField<int>();
        private readonly CacheField<int> _boundToACD = new CacheField<int>();
        private readonly CacheField<int> _durabilityCurrent = new CacheField<int>();
        private readonly CacheField<int> _durabilityMax = new CacheField<int>();
        private readonly CacheField<float> _durabilityPct = new CacheField<float>();
        private readonly CacheField<InventorySlot> _inventorySlot = new CacheField<InventorySlot>();
        private readonly CacheField<FollowerType> _followerSpecialType = new CacheField<FollowerType>();
        private readonly CacheField<GemQuality> _gemQuality = new CacheField<GemQuality>();
        private readonly CacheField<int> _gold = new CacheField<int>();
        private readonly CacheField<int> _inventoryColumn = new CacheField<int>();
        private readonly CacheField<int> _inventoryRow = new CacheField<int>();
        private readonly CacheField<int> _itemLegendaryItemLevelOverride = new CacheField<int>();
        private readonly CacheField<int> _itemLevelRequirementReduction = new CacheField<int>();
        private readonly CacheField<long> _itemStackQuantity = new CacheField<long>();
        private readonly CacheField<int> _jewelRank = new CacheField<int>();
        private readonly CacheField<int> _level = new CacheField<int>();
        private readonly CacheField<int> _lockedToACD = new CacheField<int>();
        private readonly CacheField<int> _maxStackCount = new CacheField<int>();
        private readonly CacheField<int> _numSockets = new CacheField<int>();
        private readonly CacheField<int> _numSocketsFilled = new CacheField<int>();
        private readonly CacheField<int> _requiredLevel = new CacheField<int>();
        private readonly CacheField<int> _tieredLootRunKeyLevel = new CacheField<int>();
        private readonly CacheField<bool> _isAccountBound = new CacheField<bool>();
        private readonly CacheField<bool> _isArmor = new CacheField<bool>();
        private readonly CacheField<bool> _isCrafted = new CacheField<bool>();
        private readonly CacheField<bool> _isCraftingPage = new CacheField<bool>();
        private readonly CacheField<bool> _isEquipped = new CacheField<bool>(UpdateSpeed.VerySlow);
        private readonly CacheField<bool> _isGem = new CacheField<bool>();
        private readonly CacheField<bool> _isMisc = new CacheField<bool>();
        private readonly CacheField<bool> _isOneHanded = new CacheField<bool>();
        private readonly CacheField<bool> _isTwoHanded = new CacheField<bool>();
        private readonly CacheField<bool> _isPotion = new CacheField<bool>();
        private readonly CacheField<bool> _isTwoSquareItem = new CacheField<bool>();
        private readonly CacheField<bool> _isUnidentified = new CacheField<bool>();
        private readonly CacheField<bool> _isOnGround = new CacheField<bool>(UpdateSpeed.Ultra);       
        private readonly CacheField<bool> _isVendorBought = new CacheField<bool>();
        private readonly CacheField<bool> _isAutoPickup = new CacheField<bool>();
        private readonly CacheField<ItemQuality> _itemQuality = new CacheField<ItemQuality>();
        private readonly CacheField<ItemStats> _stats = new CacheField<ItemStats>();
        private readonly CacheField<HashSet<InventorySlot>> _validInventorySlots = new CacheField<HashSet<InventorySlot>>();
        private readonly CacheField<bool> _isAncient = new CacheField<bool>();
        private readonly CacheField<double> _weaponDamagePercent = new CacheField<double>();
        private readonly CacheField<double> _weaponBaseMaxPhysicalDamage = new CacheField<double>();
        private readonly CacheField<double> _weaponBaseMinPhysicalDamage = new CacheField<double>();
        private readonly CacheField<ItemType> _itemType = new CacheField<ItemType>();
        private readonly CacheField<ItemBaseType> _itemBaseType = new CacheField<ItemBaseType>();
        private readonly CacheField<TrinityItemType> _trinityItemType = new CacheField<TrinityItemType>();
        private readonly CacheField<TrinityItemBaseType> _trinityItemBaseType = new CacheField<TrinityItemBaseType>();


        #endregion

        #region Properties

        /// <summary>
        /// DB's ItemType classification
        /// </summary>
        public ItemType ItemType
        {
            get { return _itemType.IsCacheValid ? _itemType.CachedValue : (_itemType.CachedValue = GetACDItemProperty(x => x.ItemType)); }
            set { _itemType.SetValueOverride(value); }
        }

        /// <summary>
        /// DB's ItemBaseType classification
        /// </summary>
        public ItemBaseType ItemBaseType
        {
            get { return _itemBaseType.IsCacheValid ? _itemBaseType.CachedValue : (_itemBaseType.CachedValue = GetACDItemProperty(x => x.ItemBaseType)); }
            set { _itemBaseType.SetValueOverride(value); }
        }

        /// <summary>
        /// Trinity's Item Type
        /// </summary>
        public TrinityItemType TrinityItemType
        {
            get { return _trinityItemType.IsCacheValid ? _trinityItemType.CachedValue : (_trinityItemType.CachedValue = TrinityItemManager.DetermineItemType(InternalName, ItemType, FollowerSpecialType)); }
            set { _trinityItemType.SetValueOverride(value); }
        }

        /// <summary>
        /// If item is ancient
        /// </summary>
        public TrinityItemBaseType TrinityItemBaseType
        {
            get { return _trinityItemBaseType.IsCacheValid ? _trinityItemBaseType.CachedValue : (_trinityItemBaseType.CachedValue = TrinityItemManager.DetermineBaseType(TrinityItemType)); }
            set { _trinityItemBaseType.SetValueOverride(value); }
        }

        /// <summary>
        /// ACD of account item is bound to
        /// </summary>
        public int BoundToACD
        {
            get { return _boundToACD.IsCacheValid ? _boundToACD.CachedValue : (_boundToACD.CachedValue = GetACDItemProperty(x => x.BoundToACD)); }
            set { _boundToACD.SetValueOverride(value); }
        }

        /// <summary>
        /// Current amount of durability
        /// </summary>
        public int DurabilityCurrent
        {
            get { return _durabilityCurrent.IsCacheValid ? _durabilityCurrent.CachedValue : (_durabilityCurrent.CachedValue = GetACDItemProperty(x => x.DurabilityCurrent)); }
            set { _durabilityCurrent.SetValueOverride(value); }
        }

        /// <summary>
        /// Max possible durability
        /// </summary>
        public int DurabilityMax
        {
            get { return _durabilityMax.IsCacheValid ? _durabilityMax.CachedValue : (_durabilityMax.CachedValue = GetACDItemProperty(x => x.DurabilityMax)); }
            set { _durabilityMax.SetValueOverride(value); }
        }

        /// <summary>
        /// Current durability as a percentage of maximum
        /// </summary>
        public float DurabilityPct
        {
            get { return _durabilityPct.IsCacheValid ? _durabilityPct.CachedValue : (_durabilityPct.CachedValue = GetACDItemProperty(x => x.DurabilityPercent)); }
            set { _durabilityPct.SetValueOverride(value); }
        }

        /// <summary>
        /// Inventory Slot (Hand, Feet etc. None = item is on the ground)
        /// </summary>
        public InventorySlot InventorySlot
        {
            get { return _inventorySlot.IsCacheValid ? _inventorySlot.CachedValue : (_inventorySlot.CachedValue = GetACDItemProperty(x => x.InventorySlot)); }
            set { _inventorySlot.SetValueOverride(value); }
        }

        /// <summary>
        /// Scoundrel, Templar etc.
        /// </summary>
        public FollowerType FollowerSpecialType
        {
            get { return _followerSpecialType.IsCacheValid ? _followerSpecialType.CachedValue : (_followerSpecialType.CachedValue = GetACDItemProperty(x => x.FollowerSpecialType)); }
            set { _followerSpecialType.SetValueOverride(value); }
        }

        /// <summary>
        /// Chipped, Flawed, Square etc.
        /// </summary>
        public GemQuality GemQuality
        {
            get { return _gemQuality.IsCacheValid ? _gemQuality.CachedValue : (_gemQuality.CachedValue = GetACDItemProperty(x => x.GemQuality)); }
            set { _gemQuality.SetValueOverride(value); }
        }

        /// <summary>
        /// Gold amount
        /// </summary>
        public int Gold
        {
            get { return _gold.IsCacheValid ? _gold.CachedValue : (_gold.CachedValue = GetACDItemProperty(x => x.Gold)); }
            set { _gold.SetValueOverride(value); }
        }

        /// <summary>
        /// Number of slots away from left (inventory)
        /// </summary>
        public int InventoryColumn
        {
            get { return _inventoryColumn.IsCacheValid ? _inventoryColumn.CachedValue : (_inventoryColumn.CachedValue = GetACDItemProperty(x => x.InventoryColumn)); }
            set { _inventoryColumn.SetValueOverride(value); }
        }

        /// <summary>
        /// Number of slots away from top (inventory)
        /// </summary>
        public int InventoryRow
        {
            get { return _inventoryRow.IsCacheValid ? _inventoryRow.CachedValue : (_inventoryRow.CachedValue = GetACDItemProperty(x => x.InventoryRow)); }
            set { _inventoryRow.SetValueOverride(value); }
        }

        /// <summary>
        /// 
        /// </summary>
        public int ItemLegendaryItemLevelOverride
        {
            get { return _itemLegendaryItemLevelOverride.IsCacheValid ? _itemLegendaryItemLevelOverride.CachedValue : (_itemLegendaryItemLevelOverride.CachedValue = GetACDItemProperty(x => x.ItemLegendaryItemLevelOverride)); }
            set { _itemLegendaryItemLevelOverride.SetValueOverride(value); }
        }

        /// <summary>
        /// Amount of item level requirement reduction
        /// </summary>
        public int ItemLevelRequirementReduction
        {
            get { return _itemLevelRequirementReduction.IsCacheValid ? _itemLevelRequirementReduction.CachedValue : (_itemLevelRequirementReduction.CachedValue = GetACDItemProperty(x => x.ItemLevelRequirementReduction)); }
            set { _itemLevelRequirementReduction.SetValueOverride(value); }
        }

        /// <summary>
        /// Amount of item level requirement reduction
        /// </summary>
        public long ItemStackQuantity
        {
            get { return _itemStackQuantity.IsCacheValid ? _itemStackQuantity.CachedValue : (_itemStackQuantity.CachedValue = GetACDItemProperty(x => x.ItemStackQuantity)); }
            set { _itemStackQuantity.SetValueOverride(value); }
        }

        /// <summary>
        /// 
        /// </summary>
        public int JewelRank
        {
            get { return _jewelRank.IsCacheValid ? _jewelRank.CachedValue : (_jewelRank.CachedValue = GetACDItemProperty(x => x.JewelRank)); }
            set { _jewelRank.SetValueOverride(value); }
        }

        /// <summary>
        /// Level of this item
        /// </summary>
        public int Level
        {
            get { return _level.IsCacheValid ? _level.CachedValue : (_level.CachedValue = GetACDItemProperty(x => x.Level)); }
            set { _level.SetValueOverride(value); }
        }

        /// <summary>
        /// ACD of account this item is locked to
        /// </summary>
        public int LockedToACD
        {
            get { return _lockedToACD.IsCacheValid ? _lockedToACD.CachedValue : (_lockedToACD.CachedValue = GetACDItemProperty(x => x.LockedToACD)); }
            set { _lockedToACD.SetValueOverride(value); }
        }

        /// <summary>
        /// Maximum stack count
        /// </summary>
        public int MaxStackCount
        {
            get { return _maxStackCount.IsCacheValid ? _maxStackCount.CachedValue : (_maxStackCount.CachedValue = GetACDItemProperty(x => x.MaxStackCount)); }
            set { _maxStackCount.SetValueOverride(value); }
        }

        /// <summary>
        /// Current Number of Sockets
        /// </summary>
        public int NumSockets
        {
            get { return _numSockets.IsCacheValid ? _numSockets.CachedValue : (_numSockets.CachedValue = GetACDItemProperty(x => x.NumSockets)); }
            set { _numSockets.SetValueOverride(value); }
        }

        /// <summary>
        /// Number of sockets with gems in them
        /// </summary>
        public int NumSocketsFilled
        {
            get { return _numSocketsFilled.IsCacheValid ? _numSocketsFilled.CachedValue : (_numSocketsFilled.CachedValue = GetACDItemProperty(x => x.NumSocketsFilled)); }
            set { _numSocketsFilled.SetValueOverride(value); }
        }

        /// <summary>
        /// Level required to use item
        /// </summary>
        public int RequiredLevel
        {
            get { return _requiredLevel.IsCacheValid ? _requiredLevel.CachedValue : (_requiredLevel.CachedValue = GetACDItemProperty(x => x.RequiredLevel)); }
            set { _requiredLevel.SetValueOverride(value); }
        }

        /// <summary>
        /// Greater rift key level
        /// </summary>
        public int TieredLootRunKeyLevel
        {
            get { return _tieredLootRunKeyLevel.IsCacheValid ? _tieredLootRunKeyLevel.CachedValue : (_tieredLootRunKeyLevel.CachedValue = GetACDItemProperty(x => x.TieredLootRunKeyLevel)); }
            set { _tieredLootRunKeyLevel.SetValueOverride(value); }
        }

        /// <summary>
        /// If the item is locked to a specific account
        /// </summary>
        public bool IsAccountBound
        {
            get { return _isAccountBound.IsCacheValid ? _isAccountBound.CachedValue : (_isAccountBound.CachedValue = GetACDItemProperty(x => x.IsAccountBound)); }
            set { _isAccountBound.SetValueOverride(value); }
        }

        /// <summary>
        /// If armor
        /// </summary>
        public bool IsArmor
        {
            get { return _isArmor.IsCacheValid ? _isArmor.CachedValue : (_isArmor.CachedValue = GetACDItemProperty(x => x.IsArmor)); }
            set { _isArmor.SetValueOverride(value); }
        }

        /// <summary>
        /// If armor
        /// </summary>
        public bool IsCrafted
        {
            get { return _isCrafted.IsCacheValid ? _isCrafted.CachedValue : (_isCrafted.CachedValue = GetACDItemProperty(x => x.IsCrafted)); }
            set { _isCrafted.SetValueOverride(value); }
        }

        /// <summary>
        /// If a recipe to craft an item
        /// </summary>
        public bool IsCraftingPage
        {
            get { return _isCraftingPage.IsCacheValid ? _isCraftingPage.CachedValue : (_isCraftingPage.CachedValue = GetACDItemProperty(x => x.IsCraftingPage)); }
            set { _isCraftingPage.SetValueOverride(value); }
        }

        /// <summary>
        /// Is currently equipped
        /// </summary>
        public bool IsEquipped
        {
            get { return _isEquipped.IsCacheValid ? _isEquipped.CachedValue : (_isEquipped.CachedValue = GetACDItemProperty(x => x.IsEquipped)); }
            set { _isEquipped.SetValueOverride(value); }
        }

        /// <summary>
        /// Is a gem
        /// </summary>
        public bool IsGem
        {
            get { return _isGem.IsCacheValid ? _isGem.CachedValue : (_isGem.CachedValue = GetACDItemProperty(x => x.IsGem)); }
            set { _isGem.SetValueOverride(value); }
        }

        /// <summary>
        /// Is a misc item
        /// </summary>
        public bool IsMiscItem
        {
            get { return _isMisc.IsCacheValid ? _isMisc.CachedValue : (_isMisc.CachedValue = GetACDItemProperty(x => x.IsMiscItem)); }
            set { _isMisc.SetValueOverride(value); }
        }

        /// <summary>
        /// Is one handed item
        /// </summary>
        public bool IsOneHand
        {
            get { return _isOneHanded.IsCacheValid ? _isOneHanded.CachedValue : (_isOneHanded.CachedValue = GetACDItemProperty(x => x.IsOneHand)); }
            set { _isOneHanded.SetValueOverride(value); }
        }

        /// <summary>
        /// Is two handed item
        /// </summary>
        public bool IsTwoHand
        {
            get { return _isTwoHanded.IsCacheValid ? _isTwoHanded.CachedValue : (_isTwoHanded.CachedValue = GetACDItemProperty(x => x.IsTwoHand)); }
            set { _isTwoHanded.SetValueOverride(value); }
        }

        /// <summary>
        /// Is a potion
        /// </summary>
        public bool IsPotion
        {
            get { return _isPotion.IsCacheValid ? _isPotion.CachedValue : (_isPotion.CachedValue = GetACDItemProperty(x => x.IsPotion)); }
            set { _isPotion.SetValueOverride(value); }
        }

        /// <summary>
        /// Takes up two slots in your backpack/stash
        /// </summary>
        public bool IsTwoSquareItem
        {
            get { return _isTwoSquareItem.IsCacheValid ? _isTwoSquareItem.CachedValue : (_isTwoSquareItem.CachedValue = GetACDItemProperty(x => x.IsTwoSquareItem)); }
            set { _isTwoSquareItem.SetValueOverride(value); }
        }

        /// <summary>
        /// Takes up two slots in your backpack/stash
        /// </summary>
        public bool IsUnidentified
        {
            get { return _isUnidentified.IsCacheValid ? _isUnidentified.CachedValue : (_isUnidentified.CachedValue = GetACDItemProperty(x => x.IsUnidentified)); }
            set { _isUnidentified.SetValueOverride(value); }
        }

        /// <summary>
        /// Takes up two slots in your backpack/stash
        /// </summary>
        public bool IsVendorBought
        {
            get { return _isVendorBought.IsCacheValid ? _isVendorBought.CachedValue : (_isVendorBought.CachedValue = GetACDItemProperty(x => x.IsVendorBought)); }
            set { _isVendorBought.SetValueOverride(value); }
        }

        /// <summary>
        /// If item can be picked up automatically
        /// </summary>
        public bool IsAutoPickUp
        {
            get { return _isAutoPickup.IsCacheValid ? _isAutoPickup.CachedValue : (_isAutoPickup.CachedValue = GetACDItemProperty(x => !x.NoAutoPickUp)); }
            set { _isAutoPickup.SetValueOverride(value); }
        }

        /// <summary>
        /// Quality of the item (Legendary, Rare etc)
        /// </summary>
        public ItemQuality ItemQuality
        {
            get { return _itemQuality.IsCacheValid ? _itemQuality.CachedValue : (_itemQuality.CachedValue = GetItemQuality(this)); }
            set { _itemQuality.SetValueOverride(value); }
        }

        /// <summary>
        /// Source for stat bonuses and detailed info (accessing properties reads directly from Diablo memory)
        /// </summary>
        public ItemStats Stats
        {
            get { return _stats.IsCacheValid ? _stats.CachedValue : (_stats.CachedValue = GetACDItemProperty(x => x.Stats)); }
            set { _stats.SetValueOverride(value); }
        }

        /// <summary>
        /// Inventory slots this item can be used for
        /// </summary>
        public HashSet<InventorySlot> ValidInventorySlots
        {
            get { return _validInventorySlots.IsCacheValid ? _validInventorySlots.CachedValue : (_validInventorySlots.CachedValue = GetACDItemProperty(x => new HashSet<InventorySlot>(x.ValidInventorySlots))); }
            set { _validInventorySlots.SetValueOverride(value); }
        }

        /// <summary>
        /// If item is ancient
        /// </summary>
        public bool IsAncient
        {
            get { return _isAncient.IsCacheValid ? _isAncient.CachedValue : (_isAncient.CachedValue = GetACDItemProperty(x => x.AncientRank) > 0); }
            set { _isAncient.SetValueOverride(value); }
        }

        /// <summary>
        /// The +% damage property on weapons.
        /// </summary>
        public double WeaponDamagePercent
        {
            get { return _weaponDamagePercent.IsCacheValid ? _weaponDamagePercent.CachedValue : (_weaponDamagePercent.CachedValue = Math.Round(Source.GetAttributeOrDefault<float>(ActorAttributeType.DamageWeaponPercentAll) * 100, MidpointRounding.AwayFromZero)); }
            set { _weaponDamagePercent.SetValueOverride(value); }
        }

        /// <summary>
        /// The MAXIMUM end of the damage range listed on a weapon e.g. 1450-1790 fire damage
        /// </summary>
        public double WeaponBaseMaxPhysicalDamage
        {
            get { return _weaponBaseMaxPhysicalDamage.IsCacheValid ? _weaponBaseMaxPhysicalDamage.CachedValue : (_weaponBaseMaxPhysicalDamage.CachedValue = Math.Round(Source.GetAttributeOrDefault<float>(ActorAttributeType.DamageMaxWeaponBonusPhysical) + Source.GetAttributeOrDefault<float>(ActorAttributeType.DamageMinWeaponBonusPhysical))); }
            set { _weaponBaseMaxPhysicalDamage.SetValueOverride(value); }
        }

        /// <summary>
        /// Is on the ground at the moment
        /// </summary>
        public bool IsOnGround
        {
            get { return _isOnGround.IsCacheValid ? _isOnGround.CachedValue : (_isOnGround.CachedValue = InventorySlot == InventorySlot.None); }
            set { _isOnGround.SetValueOverride(value); }
        }

        /// <summary>
        /// The MINIMUM end of the damage range listed on a weapon e.g. 1450-1790 fire damage
        /// </summary>
        public double WeaponBaseMinPhysicalDamage
        {
            get { return _weaponBaseMinPhysicalDamage.IsCacheValid ? _weaponBaseMinPhysicalDamage.CachedValue : (_weaponBaseMinPhysicalDamage.CachedValue = Math.Round(Source.GetAttributeOrDefault<float>(ActorAttributeType.DamageMaxWeaponBonusPhysical))); }
            set { _weaponBaseMinPhysicalDamage.SetValueOverride(value); }
        }


        #endregion

        #region Methods

        public bool GetStatChanges(out float toughness, out float healing, out float damage)
        {
            return ACDItem.GetStatChanges(out toughness, out healing, out damage);
        }

        public void Socket(ACDItem gem)
        {
            ACDItem.Socket(gem);
        }

        public static double SkillDamagePercent(ACDItem acdItem, SNOPower power)
        {
            return Math.Round(acdItem.GetAttribute<float>(((int)power << 12) + ((int)ActorAttributeType.PowerDamagePercentBonus & 0xFFF)) * 100, MidpointRounding.AwayFromZero);
        }

        public static ItemQuality GetItemQuality(TrinityItem item)
        {
            if (item.IsOnGround)
                return item.GetDiaItemProperty(x => x.CommonData.ItemQualityLevel);

            return item.GetACDItemProperty(x => x.ItemQualityLevel);
        }

        public override string ToString()
        {
            return String.Format("{0}, Type={1} TrinityItemType={2} TrinityBaseType={3} InventorySlot={4} Dist={5}", Name, TrinityType, TrinityItemType, TrinityItemBaseType, InventorySlot, RadiusDistance);
        }

        #endregion


    }
}