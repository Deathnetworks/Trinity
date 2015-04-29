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
        public TrinityItem(ACD acd) : base(acd) { }

        #region Properties

        public float AncientRank
        {
            get { return CacheManager.GetCacheValue(this, o => o.Item.AncientRank); }
        }

        public int BoundToACD
        {
            get { return CacheManager.GetCacheValue(this, o => o.Item.BoundToACD); }
        }

        public float DurabilityCurrent
        {
            get { return CacheManager.GetCacheValue(this, o => o.Item.DurabilityPercent); }
        }

        public InventorySlot InventorySlot
        {
            get { return CacheManager.GetCacheValue(this, o => o.Item.InventorySlot); }
        }

        public float DurabilityMax
        {
            get { return CacheManager.GetCacheValue(this, o => o.Item.DurabilityPercent); }
        }

        public float DurabilityPercent
        {
            get { return CacheManager.GetCacheValue(this, o => o.Item.DurabilityPercent); }
        }

        public DyeType DyeType
        {
            get { return CacheManager.GetCacheValue(this, o => o.Item.DyeType); }
        }

        public FollowerType FollowerSpecialType
        {
            get { return CacheManager.GetCacheValue(this, o => o.Item.FollowerSpecialType); }
        }

        public GemQuality GemQuality
        {
            get { return CacheManager.GetCacheValue(this, o => o.Item.GemQuality); }
        }

        public int Gold
        {
            get { return CacheManager.GetCacheValue(this, o => o.Item.Gold); }
        }

        public int HitpointsGranted
        {
            get { return CacheManager.GetCacheValue(this, o => o.Item.HitpointsGranted); }
        }

        public int IdentifyCost
        {
            get { return CacheManager.GetCacheValue(this, o => o.Item.IdentifyCost); }
        }

        public int InventoryColumn
        {
            get { return CacheManager.GetCacheValue(this, o => o.Item.InventoryColumn); }
        }

        public int InventoryRow
        {
            get { return CacheManager.GetCacheValue(this, o => o.Item.InventoryRow); }
        }

        public int ItemLegendaryItemLevelOverride
        {
            get { return CacheManager.GetCacheValue(this, o => o.Item.ItemLegendaryItemLevelOverride); }
        }

        public int ItemLevelRequirementReduction
        {
            get { return CacheManager.GetCacheValue(this, o => o.Item.ItemLevelRequirementReduction); }
        }

        public int ItemStackQuantity
        {
            get { return CacheManager.GetCacheValue(this, o => o.Item.ItemStackQuantity); }
        }

        public int ItemTimeSold
        {
            get { return CacheManager.GetCacheValue(this, o => o.Item.ItemTimeSold); }
        }

        public int ItemUnlockTime
        {
            get { return CacheManager.GetCacheValue(this, o => o.Item.ItemUnlockTime); }
        }

        public int JewelRank
        {
            get { return CacheManager.GetCacheValue(this, o => o.Item.JewelRank); }
        }

        public int Level
        {
            get { return CacheManager.GetCacheValue(this, o => o.Item.Level); }
        }

        public int LockedToACD
        {
            get { return CacheManager.GetCacheValue(this, o => o.Item.LockedToACD); }
        }

        public int MaxDurability
        {
            get { return CacheManager.GetCacheValue(this, o => o.Item.MaxDurability); }
        }

        public int MaxStackCount
        {
            get { return CacheManager.GetCacheValue(this, o => o.Item.MaxStackCount); }
        }

        public int NumSockets
        {
            get { return CacheManager.GetCacheValue(this, o => o.Item.NumSockets); }
        }

        public int NumSocketsFilled
        {
            get { return CacheManager.GetCacheValue(this, o => o.Item.NumSocketsFilled); }
        }

        public int RequiredLevel
        {
            get { return CacheManager.GetCacheValue(this, o => o.Item.RequiredLevel); }
        }

        public int TieredLootRunKeyLevel
        {
            get { return CacheManager.GetCacheValue(this, o => o.Item.TieredLootRunKeyLevel); }
        }

        public bool IsAccountBound
        {
            get { return CacheManager.GetCacheValue(this, o => o.Item.IsAccountBound); }
        }

        public bool IsArmor
        {
            get { return CacheManager.GetCacheValue(this, o => o.Item.IsArmor); }
        }

        public bool IsCrafted
        {
            get { return CacheManager.GetCacheValue(this, o => o.Item.IsCrafted); }
        }

        public bool IsCraftingPage
        {
            get { return CacheManager.GetCacheValue(this, o => o.Item.IsCraftingPage); }
        }

        public bool IsCraftingReagent
        {
            get { return CacheManager.GetCacheValue(this, o => o.Item.IsCraftingReagent); }
        }

        public bool IsEquipped
        {
            get { return CacheManager.GetCacheValue(this, o => o.Item.IsEquipped); }
        }

        public bool IsGem
        {
            get { return CacheManager.GetCacheValue(this, o => o.Item.IsGem); }
        }

        public bool IsMiscItem
        {
            get { return CacheManager.GetCacheValue(this, o => o.Item.IsMiscItem); }
        }

        public bool IsOneHand
        {
            get { return CacheManager.GetCacheValue(this, o => o.Item.IsOneHand); }
        }

        public bool IsPotion
        {
            get { return CacheManager.GetCacheValue(this, o => o.Item.IsPotion); }
        }

        public bool IsTwoHand
        {
            get { return CacheManager.GetCacheValue(this, o => o.Item.IsTwoHand); }
        }

        public bool IsTwoSquareItem
        {
            get { return CacheManager.GetCacheValue(this, o => o.Item.IsTwoSquareItem); }
        }

        public bool IsUnidentified
        {
            get { return CacheManager.GetCacheValue(this, o => o.Item.IsUnidentified); }
        }

        public bool IsVendorBought
        {
            get { return CacheManager.GetCacheValue(this, o => o.Item.IsVendorBought); }
        }

        public bool NoAutoPickUp
        {
            get { return CacheManager.GetCacheValue(this, o => o.Item.NoAutoPickUp); }
        }

        public ItemQuality ItemQuality
        {
            get { return CacheManager.GetCacheValue(this, o => o.Item.ItemQualityLevel); }
        }

        public int ItemLevel
        {
            get { return CacheManager.GetCacheValue(this, o => o.Item.Level); }
        }

        public ItemStats Stats
        {
            get { return CacheManager.GetCacheValue(this, o => o.Item.Stats); }
        }

        public List<InventorySlot> ValidInventorySlots
        {
            get { return CacheManager.GetCacheValue(this, o => o.Item.ValidInventorySlots).ToList(); }
        }

        public bool IsAncient
        {
            get { return CacheManager.GetCacheValue(this, o => AncientRank > 0); }          
        }

        public double WeaponDamagePercent
        {
            get { return CacheManager.GetCacheValue(this, o => Math.Round(o.Source.GetAttributeOrDefault<float>(ActorAttributeType.DamageWeaponPercentAll)*100, MidpointRounding.AwayFromZero)); }          
        }

        public double WeaponBaseMaxPhysicalDamage
        {
            get { return CacheManager.GetCacheValue(this, o => Math.Round(o.Source.GetAttributeOrDefault<float>(ActorAttributeType.DamageMaxWeaponBonusPhysical) + o.Source.GetAttributeOrDefault<float>(ActorAttributeType.DamageMinWeaponBonusPhysical))); }
        }

        public double WeaponBaseMinPhysicalDamage
        {
            get { return CacheManager.GetCacheValue(this, o => Math.Round(o.Source.GetAttributeOrDefault<float>(ActorAttributeType.DamageMaxWeaponBonusPhysical))); }
        }

        public ItemType ItemType
        {
            get { return CacheManager.GetCacheValue(this, o => o.Item.ItemType); }
        }

        public ItemBaseType ItemBaseType
        {
            get { return CacheManager.GetCacheValue(this, o => o.Item.ItemBaseType); }
        }
        
        public TinityItemType TrinityItemType
        {
            get { return CacheManager.GetCacheValue(this, o => TrinityItemManager.DetermineItemType(o.InternalName, o.ItemType, o.FollowerSpecialType)); }            
        }

        public TrinityItemBaseType TrinityItemBaseType
        {
            get { return CacheManager.GetCacheValue(this, o => TrinityItemManager.DetermineBaseType(o.TrinityItemType)); }
        }

        public int GoldAmount
        {
            get { return CacheManager.GetCacheValue(this, o => o.Item.Gold); }
        }

        #endregion

        #region Methods

        public bool GetStatChanges(out float toughness, out float healing, out float damage)
        {
            return Item.GetStatChanges(out toughness, out healing, out damage);
        }

        public void Socket(ACDItem gem)
        {
            Item.Socket(gem);
        }

        public static double SkillDamagePercent(ACDItem acdItem, SNOPower power)
        {
            return Math.Round(acdItem.GetAttribute<float>(((int)power << 12) + ((int)ActorAttributeType.PowerDamagePercentBonus & 0xFFF)) * 100, MidpointRounding.AwayFromZero);
        }

        #endregion

        public static implicit operator TrinityItem(ACD x)
        {
            return CacheFactory.CreateObject<TrinityItem>(x);
        }

        public static implicit operator ACDItem(TrinityItem x)
        {
            return x.Item;
        }

    }
}