using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Trinity.Cache;
using Trinity.DbProvider;
using Trinity.Helpers;
using Trinity.ItemRules;
using Trinity.Reference;
using Trinity.Settings.Loot;
using Trinity.Technicals;
using Zeta.Bot;
using Zeta.Bot.Items;
using Zeta.Bot.Settings;
using Zeta.Common;
using Zeta.Game;
using Zeta.Game.Internals;
using Zeta.Game.Internals.Actors;
using Logger = Trinity.Technicals.Logger;

namespace Trinity.Items
{
    public class TrinityItemManager : ItemManager
    {
        private RuleTypePriority _priority;

        public override RuleTypePriority Priority
        {
            get
            {
                if (_priority == null)
                {
                    _priority = new RuleTypePriority
                    {
                        Priority1 = ItemEvaluationType.Keep,
                        Priority2 = ItemEvaluationType.Salvage,
                        Priority3 = ItemEvaluationType.Sell
                    };
                }
                return _priority;
            }
        }

        public override bool EvaluateItem(ACDItem item, ItemEvaluationType evaluationType)
        {
            // Salvage/Sell/Stashing is disabled during greater rifts
            if (Trinity.Player.ParticipatingInTieredLootRun)
                return false;

            if (Trinity.Settings.Loot.ItemFilterMode != ItemFilterMode.DemonBuddy)
            {
                Current.EvaluateItem(item, evaluationType);
            }
            else
            {
                switch (evaluationType)
                {
                    case ItemEvaluationType.Keep:
                        return ShouldStashItem(item);
                    case ItemEvaluationType.Salvage:
                        return ShouldSalvageItem(item);
                    case ItemEvaluationType.Sell:
                        return ShouldSellItem(item);
                }
            }
            return false;
        }

        public override bool ShouldSalvageItem(ACDItem item)
        {
            // Salvage/Sell/Stashing is disabled during greater rifts
            if (Trinity.Player.ParticipatingInTieredLootRun)
                return false;

            bool action = ShouldSalvageItem(item, ItemEvaluationType.Salvage);
            if (action)
                ItemStashSellAppender.Instance.AppendItem(CachedACDItem.GetCachedItem(item), "Salvage");
            return action;
        }

        public bool ShouldSalvageItem(ACDItem item, ItemEvaluationType evaluationType)
        {
            ItemEvents.ResetTownRun();

            if (Current.ItemIsProtected(item))
                return false;

            if (Trinity.Settings.Loot.ItemFilterMode == ItemFilterMode.DemonBuddy)
            {
                return Current.ShouldSalvageItem(item);
            }
            if (Trinity.Settings.Loot.ItemFilterMode == ItemFilterMode.TrinityWithItemRules)
            {
                return ItemRulesSalvageSell(item, evaluationType);
            }
            return TrinitySalvage(item);
        }

        public override bool ShouldSellItem(ACDItem item)
        {
            // Salvage/Sell/Stashing is disabled during greater rifts
            if (Trinity.Player.ParticipatingInTieredLootRun)
                return false;

            bool action = ShouldSellItem(item, ItemEvaluationType.Sell);
            if (action)
                ItemStashSellAppender.Instance.AppendItem(CachedACDItem.GetCachedItem(item), "Sell");
            return action;
        }

        public bool ShouldSellItem(ACDItem item, ItemEvaluationType evaluationType)
        {
            ItemEvents.ResetTownRun();

            CachedACDItem cItem = CachedACDItem.GetCachedItem(item);

            if (Current.ItemIsProtected(cItem.AcdItem))
                return false;

            if (Trinity.Settings.Loot.ItemFilterMode == ItemFilterMode.DemonBuddy)
            {
                return Current.ShouldSellItem(item);
            }
            if (Trinity.Settings.Loot.ItemFilterMode == ItemFilterMode.TrinityWithItemRules)
            {
                return ItemRulesSalvageSell(item, evaluationType);
            }
            return TrinitySell(item);
        }

        public override bool ShouldStashItem(ACDItem item)
        {
            // Salvage/Sell/Stashing is disabled during greater rifts
            if (Trinity.Player.ParticipatingInTieredLootRun)
                return false;

            bool action = ShouldStashItem(item, ItemEvaluationType.Keep);
            if (action)
                ItemStashSellAppender.Instance.AppendItem(CachedACDItem.GetCachedItem(item), "Stash");
            return action;
        }

        /// <summary>
        /// Trinity internal stashing checks
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="evaluationType">Type of the evaluation.</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public bool ShouldStashItem(ACDItem item, ItemEvaluationType evaluationType)
        {
            ItemEvents.ResetTownRun();

            if (Trinity.Settings.Loot.ItemFilterMode == ItemFilterMode.DemonBuddy)
            {
                return Current.ShouldStashItem(item);
            }

            if (Current.ItemIsProtected(item))
                return false;

            // Vanity Items
            if (DataDictionary.VanityItems.Any(i => item.InternalName.StartsWith(i)))
                return Trinity.Settings.Loot.TownRun.StashVanityItems;

            CachedACDItem cItem = CachedACDItem.GetCachedItem(item);
            // Now look for Misc items we might want to keep
            GItemType tItemType = cItem.TrinityItemType; // DetermineItemType(cItem.InternalName, cItem.DBItemType, cItem.FollowerType);
            GItemBaseType tBaseType = cItem.TrinityItemBaseType; // DetermineBaseType(trinityItemType);

            bool isEquipment = (tBaseType == GItemBaseType.Armor ||
                tBaseType == GItemBaseType.Jewelry ||
                tBaseType == GItemBaseType.Offhand ||
                tBaseType == GItemBaseType.WeaponOneHand ||
                tBaseType == GItemBaseType.WeaponRange ||
                tBaseType == GItemBaseType.WeaponTwoHand);

            if (Trinity.Settings.Loot.TownRun.ApplyPickupValidationToStashing)
            {
                // Check pickup (in case we accidentally picked it up)
                var pItem = new PickupItem(item, tBaseType, tItemType);
                var pickupCheck = PickupItemValidation(pItem);
                if (!pickupCheck)
                {
                    Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "{0} [{1}] = (TRASHING: Pickup check failed)", cItem.RealName, cItem.InternalName);
                    return false;
                }
                Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "{0} [{1}] = (Pickup check passed)", cItem.RealName, cItem.InternalName);
            }

            if (item.ItemType == ItemType.KeystoneFragment)
            {
                if ((Trinity.Settings.Loot.TownRun.KeepTieredLootRunKeysInBackpack && item.TieredLootRunKeyLevel >= 1) ||
                (Trinity.Settings.Loot.TownRun.KeepTrialLootRunKeysInBackpack && item.TieredLootRunKeyLevel == 0) ||
                (Trinity.Settings.Loot.TownRun.KeepRiftKeysInBackpack && item.TieredLootRunKeyLevel <= -1))
                    return false;
                return true;
            }

            if (cItem.TrinityItemType == GItemType.HoradricCache && Trinity.Settings.Loot.TownRun.OpenHoradricCaches)
            {
                Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "{0} [{1}] = (ignoring Horadric Cache)", cItem.RealName, cItem.InternalName);
                return false;
            }

            // Stash all unidentified items - assume we want to keep them since we are using an identifier over-ride
            if (cItem.IsUnidentified)
            {
                if (evaluationType == ItemEvaluationType.Keep)
                    Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "{0} [{1}] = (autokeep unidentified items)", cItem.RealName, cItem.InternalName);
                return true;
            }
            if (tItemType == GItemType.StaffOfHerding)
            {
                if (evaluationType == ItemEvaluationType.Keep)
                    Logger.Log(TrinityLogLevel.Info, LogCategory.ItemValuation, "{0} [{1}] [{2}] = (autokeep staff of herding)", cItem.RealName, cItem.InternalName, tItemType);
                return true;
            }
            if (tItemType == GItemType.CraftingMaterial)
            {
                if (evaluationType == ItemEvaluationType.Keep)
                    Logger.Log(TrinityLogLevel.Info, LogCategory.ItemValuation, "{0} [{1}] [{2}] = (autokeep craft materials)", cItem.RealName, cItem.InternalName, tItemType);
                return true;
            }

            if (tItemType == GItemType.Emerald || tItemType == GItemType.Amethyst || tItemType == GItemType.Topaz || tItemType == GItemType.Ruby || tItemType == GItemType.Diamond)
            {
                if (evaluationType == ItemEvaluationType.Keep)
                    Logger.Log(TrinityLogLevel.Info, LogCategory.ItemValuation, "{0} [{1}] [{2}] = (autokeep gems)", cItem.RealName, cItem.InternalName, tItemType);
                return true;
            }
            if (tItemType == GItemType.CraftTome)
            {
                if (evaluationType == ItemEvaluationType.Keep)
                    Logger.Log(TrinityLogLevel.Info, LogCategory.ItemValuation, "{0} [{1}] [{2}] = (autokeep tomes)", cItem.RealName, cItem.InternalName, tItemType);
                return true;
            }
            if (tItemType == GItemType.InfernalKey)
            {
                if (evaluationType == ItemEvaluationType.Keep)
                    Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "{0} [{1}] [{2}] = (autokeep infernal key)", cItem.RealName, cItem.InternalName, tItemType);
                return true;
            }

            if (tItemType == GItemType.HealthPotion && item.ItemQualityLevel >= ItemQuality.Legendary)
            {
                var shouldStash = Trinity.Settings.Loot.TownRun.StashLegendaryPotions;
                if (evaluationType == ItemEvaluationType.Keep)
                    Logger.Log(TrinityLogLevel.Info, LogCategory.ItemValuation, "{0} [{1}] [{2}] = ({3} legendary potions)", cItem.RealName, cItem.InternalName, tItemType,
                        shouldStash ? "stashing" : "ignoring");
                return shouldStash;
            }

            if (tItemType == GItemType.HealthPotion && item.ItemQualityLevel < ItemQuality.Legendary)
            {
                if (evaluationType == ItemEvaluationType.Keep)
                    Logger.Log(TrinityLogLevel.Info, LogCategory.ItemValuation, "{0} [{1}] [{2}] = (ignoring potions)", cItem.RealName, cItem.InternalName, tItemType);
                return false;
            }

            if (tItemType == GItemType.CraftingPlan && cItem.Quality >= ItemQuality.Legendary)
            {
                if (evaluationType == ItemEvaluationType.Keep)
                    Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "{0} [{1}] [{2}] = (autokeep legendary plans)", cItem.RealName, cItem.InternalName, tItemType);
                return true;
            }

            if (tItemType == GItemType.ConsumableAddSockets)
            {
                if (evaluationType == ItemEvaluationType.Keep)
                    Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "{0} [{1}] [{2}] = (autokeep Ramaladni's Gift)", cItem.RealName, cItem.InternalName, tItemType);
                return true;
            }

            if (tItemType == GItemType.TieredLootrunKey)
            {
                if (evaluationType == ItemEvaluationType.Keep)
                    Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "{0} [{1}] [{2}] = (ignoring Tiered Rift Keys)", cItem.RealName, cItem.InternalName, tItemType);
                return false;
            }

            if (Trinity.Settings.Loot.ItemFilterMode == ItemFilterMode.TrinityWithItemRules)
            {
                Interpreter.InterpreterAction action = Trinity.StashRule.checkItem(item, evaluationType);

                if (evaluationType == ItemEvaluationType.Keep)

                    Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "IR2 {0} [{1}] [{2}] = (" + action + ")", cItem.AcdItem.Name, cItem.AcdItem.InternalName, cItem.AcdItem.ItemType);
                switch (action)
                {
                    case Interpreter.InterpreterAction.KEEP:
                        return true;
                    case Interpreter.InterpreterAction.TRASH:
                        return false;
                    case Interpreter.InterpreterAction.SCORE:
                        break;
                }
            }

            if (tItemType == GItemType.CraftingPlan)
            {
                if (evaluationType == ItemEvaluationType.Keep)
                    Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "{0} [{1}] [{2}] = (autokeep plans)", cItem.RealName, cItem.InternalName, tItemType);
                return true;
            }

            // Stashing Whites, auto-keep
            if (Trinity.Settings.Loot.TownRun.StashWhites && isEquipment && cItem.Quality <= ItemQuality.Superior)
            {
                Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "{0} [{1}] [{2}] = (stashing whites)", cItem.RealName, cItem.InternalName, tItemType);
                return true;
            }
            // Else auto-trash
            if (cItem.Quality <= ItemQuality.Superior && (isEquipment || cItem.TrinityItemBaseType == GItemBaseType.FollowerItem))
            {
                Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "{0} [{1}] [{2}] = (trash whites)", cItem.RealName, cItem.InternalName, tItemType);
                return false;
            }

            // Stashing blues, auto-keep
            if (Trinity.Settings.Loot.TownRun.StashBlues && isEquipment && cItem.Quality >= ItemQuality.Magic1 && cItem.Quality <= ItemQuality.Magic3)
            {
                Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "{0} [{1}] [{2}] = (stashing blues)", cItem.RealName, cItem.InternalName, tItemType);
                return true;
            }
            // Else auto trash
            if (cItem.Quality >= ItemQuality.Magic1 && cItem.Quality <= ItemQuality.Magic3 && (isEquipment || cItem.TrinityItemBaseType == GItemBaseType.FollowerItem))
            {
                Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "{0} [{1}] [{2}] = (trashing blues)", cItem.RealName, cItem.InternalName, tItemType);
                return false;
            }

            // Force salvage Rares
            if (Trinity.Settings.Loot.TownRun.ForceSalvageRares && cItem.Quality >= ItemQuality.Rare4 && cItem.Quality <= ItemQuality.Rare6 && (isEquipment || cItem.TrinityItemBaseType == GItemBaseType.FollowerItem))
            {
                Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "{0} [{1}] [{2}] = (force salvage rare)", cItem.RealName, cItem.InternalName, tItemType);
                return false;
            }

            // Item Ranks
            if (cItem.Quality >= ItemQuality.Legendary && Trinity.Settings.Loot.ItemFilterMode == ItemFilterMode.ItemRanks && IsEquipment(cItem))
            {
                if (evaluationType == ItemEvaluationType.Keep)
                    Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "{0} [{1}] [{2}] = (Rank Equipment)", cItem.RealName, cItem.InternalName, tItemType);
                return ItemRanks.ShouldStashItem(cItem);
            }

            // Item List
            if (cItem.Quality >= ItemQuality.Legendary && Trinity.Settings.Loot.ItemFilterMode == ItemFilterMode.ItemList && IsEquipment(cItem))
            {
                var result = ItemList.ShouldStashItem(cItem);
                Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "{0} [{1}] [{2}] = {3}", cItem.RealName, cItem.InternalName, tItemType, "ItemListCheck=" + (result ? "KEEP" : "TRASH"));
                return result;
            }

            if (cItem.Quality >= ItemQuality.Legendary)
            {
                if (evaluationType == ItemEvaluationType.Keep)
                    Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "{0} [{1}] [{2}] = (autokeep legendaries)", cItem.RealName, cItem.InternalName, tItemType);
                return true;
            }

            // Ok now try to do some decent item scoring based on item types
            double iNeedScore = ItemValuation.ScoreNeeded(item.ItemBaseType);
            double iMyScore = ItemValuation.ValueThisItem(cItem, tItemType);

            if (evaluationType == ItemEvaluationType.Keep)
                Logger.Log(TrinityLogLevel.Verbose, LogCategory.ItemValuation, "{0} [{1}] [{2}] = {3}", cItem.RealName, cItem.InternalName, tItemType, iMyScore);
            if (iMyScore >= iNeedScore)
                return true;

            // If we reached this point, then we found no reason to keep the item!
            return false;
        }

        private bool ItemRulesSalvageSell(ACDItem item, ItemEvaluationType evaluationType)
        {
            ItemEvents.ResetTownRun();
            if (!item.IsPotion || item.ItemType != ItemType.Potion)
                Logger.Log(TrinityLogLevel.Info, LogCategory.ItemValuation,
                    "Incoming {0} Request: {1}, {2}, {3}, {4}, {5}",
                    evaluationType, item.ItemQualityLevel, item.Level, item.ItemBaseType,
                    item.ItemType, item.IsOneHand ? "1H" : item.IsTwoHand ? "2H" : "NH");

            Interpreter.InterpreterAction action = Trinity.StashRule.checkItem(item, ItemEvaluationType.Salvage);
            switch (action)
            {
                case Interpreter.InterpreterAction.SALVAGE:
                    Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "{0}: {1}", evaluationType, (evaluationType == ItemEvaluationType.Salvage));
                    return (evaluationType == ItemEvaluationType.Salvage);
                case Interpreter.InterpreterAction.SELL:
                    Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "{0}: {1}", evaluationType, (evaluationType == ItemEvaluationType.Sell));
                    return (evaluationType == ItemEvaluationType.Sell);
                default:
                    Logger.Log(TrinityLogLevel.Info, LogCategory.ScriptRule, "Trinity, item is unhandled by ItemRules (SalvageSell)!");
                    switch (evaluationType)
                    {
                        case ItemEvaluationType.Salvage:
                            return TrinitySalvage(item);
                        default:
                            return TrinitySell(item);
                    }
            }
        }

        /// <summary>
        /// Determines if we should salvage an item
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private static bool TrinitySalvage(ACDItem item)
        {
            CachedACDItem cItem = CachedACDItem.GetCachedItem(item);

            if (cItem.AcdItem.IsVendorBought)
                return false;

            // Vanity Items
            if (DataDictionary.VanityItems.Any(i => item.InternalName.StartsWith(i)))
                return false;

            if (item.ItemType == ItemType.KeystoneFragment && item.TieredLootRunKeyLevel >= 0)
                return false;


            GItemType trinityItemType = DetermineItemType(cItem.InternalName, cItem.DBItemType, cItem.FollowerType);
            GItemBaseType trinityItemBaseType = DetermineBaseType(trinityItemType);

            // Take Salvage Option corresponding to ItemLevel
            SalvageOption salvageOption = GetSalvageOption(cItem.Quality);

            // Stashing Whites
            if (Trinity.Settings.Loot.TownRun.StashWhites && cItem.Quality < ItemQuality.Magic1)
                return false;

            // Stashing Blues
            if (Trinity.Settings.Loot.TownRun.StashBlues && cItem.Quality > ItemQuality.Superior && cItem.Quality < ItemQuality.Rare4)
                return false;

            switch (trinityItemBaseType)
            {
                case GItemBaseType.WeaponRange:
                case GItemBaseType.WeaponOneHand:
                case GItemBaseType.WeaponTwoHand:
                case GItemBaseType.Armor:
                case GItemBaseType.Offhand:
                    return salvageOption == SalvageOption.Salvage;
                case GItemBaseType.Jewelry:
                    return salvageOption == SalvageOption.Salvage;
                case GItemBaseType.FollowerItem:
                    return salvageOption == SalvageOption.Salvage;
                case GItemBaseType.Gem:
                case GItemBaseType.Misc:
                case GItemBaseType.Unknown:
                    return false;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Determines if we should Sell an Item
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private static bool TrinitySell(ACDItem item)
        {
            CachedACDItem cItem = CachedACDItem.GetCachedItem(item);

            // Vanity Items
            if (DataDictionary.VanityItems.Any(i => item.InternalName.StartsWith(i)))
                return false;

            if (item.ItemType == ItemType.KeystoneFragment && item.TieredLootRunKeyLevel >= 0)
                return false;

            if (item.ItemType == ItemType.HoradricCache)
                return false;

            if (Trinity.Settings.Loot.TownRun.ApplyPickupValidationToStashing)
            {
                var pItem = new PickupItem(item, cItem.TrinityItemBaseType, cItem.TrinityItemType);
                var pickupCheck = PickupItemValidation(pItem);
                if (!pickupCheck)
                    return true;
            }

            switch (cItem.TrinityItemBaseType)
            {
                case GItemBaseType.WeaponRange:
                case GItemBaseType.WeaponOneHand:
                case GItemBaseType.WeaponTwoHand:
                case GItemBaseType.Armor:
                case GItemBaseType.Offhand:
                case GItemBaseType.Jewelry:
                case GItemBaseType.FollowerItem:
                    return true;
                case GItemBaseType.Gem:
                case GItemBaseType.Misc:
                    if (cItem.TrinityItemType == GItemType.HealthPotion && Trinity.Settings.Loot.TownRun.SellExtraPotions)
                    {
                        // Never sell our precious legendary potions!
                        if (cItem.AcdItem.ItemQualityLevel >= ItemQuality.Legendary)
                            return false;

                        bool hasLegendaryPotion = ZetaDia.Me.Inventory.Backpack.Any(i => i.ItemType == ItemType.Potion && i.ItemQualityLevel >= ItemQuality.Legendary);

                        // If we have a legendary potion, sell regular potions
                        if (hasLegendaryPotion && cItem.AcdItem.ItemQualityLevel <= ItemQuality.Legendary)
                            return true;

                        // If we have more than 1 stack of potions
                        // Keep the largest stack until we only have 1 stack
                        int potionStacks = ZetaDia.Me.Inventory.Backpack.Count(i => i.ItemType == ItemType.Potion);
                        if (potionStacks > 1)
                        {
                            // Keep only the highest stack
                            ACDItem acdItem = ZetaDia.Me.Inventory.Backpack
                                .Where(i => i.ItemType == ItemType.Potion && i.ItemQualityLevel == ItemQuality.Normal)
                                .OrderBy(i => i.ItemStackQuantity)
                                .FirstOrDefault();

                            if (acdItem != null && cItem.AcdItem.ACDGuid == acdItem.ACDGuid)
                            {
                                return true;
                            }
                        }
                    }
                    if (cItem.TrinityItemType == GItemType.CraftingPlan)
                        return true;
                    return false;
                case GItemBaseType.Unknown:
                    return false;
            }

            return false;
        }

        private static SalvageOption GetSalvageOption(ItemQuality quality)
        {
            if (quality >= ItemQuality.Normal && quality <= ItemQuality.Superior)
            {
                return Trinity.Settings.Loot.TownRun.SalvageWhiteItemOption;
            }
            if (quality >= ItemQuality.Magic1 && quality <= ItemQuality.Magic3)
            {
                return Trinity.Settings.Loot.TownRun.SalvageBlueItemOption;
            }
            if (quality >= ItemQuality.Rare4 && quality <= ItemQuality.Rare6)
            {
                return Trinity.Settings.Loot.TownRun.SalvageYellowItemOption;
            }
            if (quality >= ItemQuality.Legendary)
            {
                return Trinity.Settings.Loot.TownRun.SalvageLegendaryItemOption;
            }
            return SalvageOption.Sell;
        }

        public enum DumpItemLocation
        {
            All,
            Equipped,
            Backpack,
            Ground,
            Stash,
            Merchant,
        }

        public static void DumpQuickItems()
        {
            List<ACDItem> itemList;
            try
            {
                itemList = ZetaDia.Actors.GetActorsOfType<ACDItem>(true).OrderBy(i => i.InventorySlot).ThenBy(i => i.Name).ToList();
            }
            catch
            {
                Logger.LogError("QuickDump: Item Errors Detected!");
                itemList = ZetaDia.Actors.GetActorsOfType<ACDItem>(true).ToList();
            }
            StringBuilder sbTopList = new StringBuilder();
            foreach (var item in itemList)
            {
                try
                {
                    sbTopList.AppendFormat("\nName={0} InternalName={1} ActorSNO={2} DynamicID={3} InventorySlot={4}",
                        item.Name, item.InternalName, item.ActorSNO, item.DynamicId, item.InventorySlot);
                }
                catch (Exception)
                {
                    sbTopList.AppendFormat("Exception reading data from ACDItem ACDGuid={0}", item.ACDGuid);
                }
            }
            Logger.Log(sbTopList.ToString());
        }

#pragma warning disable 1718
        public static void DumpItems(DumpItemLocation location)
        {
            ZetaDia.Actors.Update();
            using (ZetaDia.Memory.SaveCacheState())
            {
                ZetaDia.Memory.TemporaryCacheState(false);

                List<ACDItem> itemList = new List<ACDItem>();

                switch (location)
                {
                    case DumpItemLocation.All:
                        itemList = ZetaDia.Actors.GetActorsOfType<ACDItem>(true).OrderBy(i => i.InventorySlot).ThenBy(i => i.Name).ToList();
                        break;
                    case DumpItemLocation.Backpack:
                        itemList = ZetaDia.Me.Inventory.Backpack.ToList();
                        break;
                    case DumpItemLocation.Merchant:
                        itemList = ZetaDia.Me.Inventory.MerchantItems.ToList();
                        break;
                    case DumpItemLocation.Ground:
                        itemList = ZetaDia.Actors.GetActorsOfType<DiaItem>(true).Select(i => i.CommonData).ToList();
                        break;
                    case DumpItemLocation.Equipped:
                        itemList = ZetaDia.Me.Inventory.Equipped.ToList();
                        break;
                    case DumpItemLocation.Stash:
                        if (UIElements.StashWindow.IsVisible)
                        {
                            itemList = ZetaDia.Me.Inventory.StashItems.ToList();
                        }
                        else
                        {
                            Logger.Log("Stash window not open!");
                        }
                        break;
                }

                itemList.RemoveAll(i => i == null);
                itemList.RemoveAll(i => !i.IsValid);

                foreach (var item in itemList.OrderBy(i => i.InventorySlot).ThenBy(i => i.Name))
                {
                    try
                    {
                        string itemName = String.Format("\nName={0} InternalName={1} ActorSNO={2} DynamicID={3} InventorySlot={4}",
                        item.Name, item.InternalName, item.ActorSNO, item.DynamicId, item.InventorySlot);

                        Logger.Log(itemName);
                    }
                    catch (Exception ex)
                    {
                        Logger.Log("Exception reading Basic Item Info\n{0}", ex.ToString());
                    }
                    try
                    {
                        foreach (object val in Enum.GetValues(typeof(ActorAttributeType)))
                        {
                            int iVal = item.GetAttribute<int>((ActorAttributeType)val);
                            float fVal = item.GetAttribute<float>((ActorAttributeType)val);

                            if (iVal > 0 || fVal > 0)
                                Logger.Log("Attribute: {0}, iVal: {1}, fVal: {2}", val, iVal, (fVal != fVal) ? "" : fVal.ToString());
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Log("Exception reading attributes for {0}\n{1}", item.Name, ex.ToString());
                    }

                    try
                    {
                        foreach (var stat in Enum.GetValues(typeof(ItemStats.Stat)).Cast<ItemStats.Stat>())
                        {
                            float fStatVal = item.Stats.GetStat<float>(stat);
                            int iStatVal = item.Stats.GetStat<int>(stat);
                            if (fStatVal > 0 || iStatVal > 0)
                                Logger.Log("Stat {0}={1}f ({2})", stat, fStatVal, iStatVal);
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Log("Exception reading Item Stats\n{0}", ex.ToString());
                    }

                    try
                    {
                        Logger.Log("Link Color ItemQuality=" + item.ItemLinkColorQuality());
                    }
                    catch (Exception ex)
                    {
                        Logger.Log("Exception reading Item Link\n{0}", ex.ToString());
                    }

                    try
                    {
                        PrintObjectProperties(item);
                    }
                    catch (Exception ex)
                    {
                        Logger.Log("Exception reading Item Properties\n{0}", ex.ToString());
                    }

                }
            }

        }

        private static void PrintObjectProperties<T>(T item)
        {
            PropertyInfo[] properties = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public);
            foreach (PropertyInfo property in properties.ToList().OrderBy(p => p.Name))
            {
                try
                {
                    object val = property.GetValue(item, null);
                    if (val != null)
                    {
                        Logger.Log(typeof(T).Name + "." + property.Name + "=" + val);

                        // Special cases!
                        if (property.Name == "ValidInventorySlots")
                        {
                            foreach (var slot in ((InventorySlot[])val))
                            {
                                Logger.Log(slot.ToString());
                            }
                        }
                    }
                }
                catch
                {
                    Logger.Log("Exception reading {0} from object", property.Name);
                }
            }
        }

        private static int _lastBackPackCount;
        private static int _lastProtectedSlotsCount;
        private static Vector2 _lastBackPackLocation = new Vector2(-2, -2);

        internal static void ResetBackPackCheck()
        {
            _lastBackPackCount = -1;
            _lastProtectedSlotsCount = -1;
            _lastBackPackLocation = new Vector2(-2, -2);
            TownRun.LastCheckBackpackDurability = DateTime.MinValue;
        }

        /// <summary>
        /// Search backpack to see if we have room for a 2-slot item anywhere
        /// </summary>
        /// <param name="isOriginalTwoSlot"></param>
        /// <returns></returns>
        internal static Vector2 FindValidBackpackLocation(bool isOriginalTwoSlot)
        {
            using (new PerformanceLogger("FindValidBackpackLocation"))
            {
                try
                {
                    //if (_lastBackPackLocation != new Vector2(-2, -2) &&
                    //    _lastBackPackCount == CacheData.Inventory.Backpack.Count &&
                    //    _lastProtectedSlotsCount == CharacterSettings.Instance.ProtectedBagSlots.Count)
                    //{
                    //    return _lastBackPackLocation;
                    //}

                    bool[,] backpackSlotBlocked = new bool[10, 6];

                    int freeBagSlots = 60;

                    _lastProtectedSlotsCount = CharacterSettings.Instance.ProtectedBagSlots.Count;
                    _lastBackPackCount = CacheData.Inventory.Backpack.Count;

                    // Block off the entire of any "protected bag slots"
                    foreach (InventorySquare square in CharacterSettings.Instance.ProtectedBagSlots)
                    {
                        backpackSlotBlocked[square.Column, square.Row] = true;
                        freeBagSlots--;
                    }

                    var allItems = ZetaDia.Actors.GetActorsOfType<ACDItem>(true);
                    var backpackItems = allItems.Where(i => i.InventorySlot == InventorySlot.BackpackItems);
                    var blueItems = allItems.Where(i => i.ItemQualityLevel >= ItemQuality.Magic1 && i.ItemQualityLevel <= ItemQuality.Magic3);

                    // Map out all the items already in the backpack
                    foreach (ACDItem item in ZetaDia.Me.Inventory.Backpack)
                    {
                        if (!item.IsValid)
                        {
                            Logger.LogError("Invalid backpack item detetected! marking down two slots!");
                            freeBagSlots -= 2;
                            continue;
                        }
                        int row = item.InventoryRow;
                        int col = item.InventoryColumn;

                        if (row < 0 || row > 5)
                        {
                            Logger.LogError("Item {0} ({1}) is reporting invalid backpack row of {2}!",
                                item.Name, item.InternalName, item.InventoryRow);
                            continue;
                        }

                        if (col < 0 || col > 9)
                        {
                            Logger.LogError("Item {0} ({1}) is reporting invalid backpack column of {2}!",
                                item.Name, item.InternalName, item.InventoryColumn);
                            continue;
                        }

                        // Slot is already protected, don't double count
                        if (!backpackSlotBlocked[col, row])
                        {
                            backpackSlotBlocked[col, row] = true;
                            freeBagSlots--;
                        }

                        if (!item.IsTwoSquareItem)
                            continue;

                        try
                        {
                            // Slot is already protected, don't double count
                            if (backpackSlotBlocked[col, row + 1])
                                continue;
                        }
                        catch (IndexOutOfRangeException)
                        {
                            Logger.LogError("Error checking for next slot on item {0}, row={1} col={2} IsTwoSquare={3} ItemType={4}",
                                item.Name, item.InventoryRow, item.InventoryColumn, item.ItemType);
                            continue;
                        }

                        freeBagSlots--;
                        backpackSlotBlocked[col, row + 1] = true;
                    }

                    bool noFreeSlots = freeBagSlots < 1;
                    int unprotectedSlots = 60 - _lastProtectedSlotsCount;

                    // Use count of Unprotected slots if FreeBagSlots is higher than unprotected slots
                    int minFreeSlots = Trinity.Player.IsInTown ?
                        Math.Min(Trinity.Settings.Loot.TownRun.FreeBagSlotsInTown, unprotectedSlots) :
                        Math.Min(Trinity.Settings.Loot.TownRun.FreeBagSlots, unprotectedSlots);

                    // free bag slots is less than required
                    if (noFreeSlots || freeBagSlots < minFreeSlots)
                    {
                        Logger.LogDebug("Free Bag Slots is less than required. FreeSlots={0}, FreeBagSlots={1} FreeBagSlotsInTown={2} IsInTown={3} Protected={4} BackpackCount={5}",
                            freeBagSlots, Trinity.Settings.Loot.TownRun.FreeBagSlots, Trinity.Settings.Loot.TownRun.FreeBagSlotsInTown, Trinity.Player.IsInTown,
                            _lastProtectedSlotsCount, _lastBackPackCount);
                        _lastBackPackLocation = new Vector2(-1, -1);
                        return _lastBackPackLocation;
                    }
                    // 10 columns
                    for (int col = 0; col <= 9; col++)
                    {
                        // 6 rows
                        for (int row = 0; row <= 5; row++)
                        {
                            // Slot is blocked, skip
                            if (backpackSlotBlocked[col, row])
                                continue;

                            // Not a two slotitem, slot not blocked, use it!
                            if (!isOriginalTwoSlot)
                            {
                                _lastBackPackLocation = new Vector2(col, row);
                                return _lastBackPackLocation;
                            }

                            // Is a Two Slot, Can't check for 2 slot items on last row
                            if (row == 5)
                                continue;

                            // Is a Two Slot, check row below
                            if (backpackSlotBlocked[col, row + 1])
                                continue;

                            _lastBackPackLocation = new Vector2(col, row);
                            return _lastBackPackLocation;
                        }
                    }

                    // no free slot
                    Logger.LogDebug("No Free slots!");
                    _lastBackPackLocation = new Vector2(-1, -1);
                    return _lastBackPackLocation;
                }
                catch (Exception ex)
                {
                    Logger.Log(LogCategory.UserInformation, "Error in finding backpack slot");
                    Logger.Log(LogCategory.UserInformation, "{0}", ex.ToString());
                    return new Vector2(1, 1);
                }
            }
        }

        /// <summary>
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        internal static bool ItemRulesPickupValidation(PickupItem item)
        {
            if (Trinity.StashRule == null)
                Trinity.StashRule = new Interpreter();

            Interpreter.InterpreterAction action = Trinity.StashRule.checkPickUpItem(item, ItemEvaluationType.PickUp);

            switch (action)
            {
                case Interpreter.InterpreterAction.PICKUP:
                    return true;

                case Interpreter.InterpreterAction.IGNORE:
                    return false;
            }
            return PickupItemValidation(item);
        }

        /// <summary>
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        internal static bool PickupItemValidation(PickupItem item)
        {
            // Calculate item types and base types etc.
            GItemType itemType = DetermineItemType(item.InternalName, item.DBItemType, item.ItemFollowerType);
            GItemBaseType baseType = DetermineBaseType(itemType);

            // Pickup Ramaladni's Gift
            if (itemType == GItemType.ConsumableAddSockets)
            {
                return Trinity.Settings.Loot.Pickup.RamadalinisGift;
            }

            // Tiered Rift Keys
            if (itemType == GItemType.TieredLootrunKey)
            {
                return Trinity.Settings.Loot.Pickup.LootRunKey;
            }

            // Pickup Legendary potions
            if (itemType == GItemType.HealthPotion && item.Quality >= ItemQuality.Legendary)
            {
                return Trinity.Settings.Loot.Pickup.LegendaryPotions;
            }

            if (itemType == GItemType.InfernalKey)
            {
                return Trinity.Settings.Loot.Pickup.InfernalKeys;
            }

            // Rift Keystone Fragments == LootRunkey
            if (itemType == GItemType.LootRunKey)
            {
                return Trinity.Settings.Loot.Pickup.LootRunKey;
            }

            // Blood Shards == HoradricRelic
            if (itemType == GItemType.HoradricRelic && ZetaDia.CPlayer.BloodshardCount < 500)
            {
                return Trinity.Settings.Loot.Pickup.BloodShards;
            }

            if (itemType == GItemType.CraftingMaterial && (item.ACDItem.GetTrinityItemQuality() < Trinity.Settings.Loot.Pickup.MiscItemQuality || !Trinity.Settings.Loot.Pickup.CraftMaterials))
            {
                return false;
            }

            // Plans
            if (item.InternalName.ToLower().StartsWith("craftingplan_smith") && (item.ACDItem.GetTrinityItemQuality() < Trinity.Settings.Loot.Pickup.MiscItemQuality || !Trinity.Settings.Loot.Pickup.Plans))
            {
                return false;
            }

            // Designs
            if (item.InternalName.ToLower().StartsWith("craftingplan_jeweler") && (item.ACDItem.GetTrinityItemQuality() < Trinity.Settings.Loot.Pickup.MiscItemQuality || !Trinity.Settings.Loot.Pickup.Designs))
            {
                return false;
            }

            if (itemType == GItemType.CraftingPlan && item.Quality >= ItemQuality.Legendary && Trinity.Settings.Loot.Pickup.LegendaryPlans)
            {
                return true;
            }

            if (item.IsUpgrade && Trinity.Settings.Loot.Pickup.PickupUpgrades)
            {
                return true;
            }

            switch (baseType)
            {
                case GItemBaseType.WeaponTwoHand:
                case GItemBaseType.WeaponOneHand:
                case GItemBaseType.WeaponRange:
                    if (item.Quality >= ItemQuality.Legendary)
                        return Trinity.Settings.Loot.Pickup.PickupLegendaries;

                    return CheckLevelRequirements(item.Level, item.Quality, Trinity.Settings.Loot.Pickup.PickupBlueWeapons, Trinity.Settings.Loot.Pickup.PickupYellowWeapons);
                case GItemBaseType.Armor:
                case GItemBaseType.Offhand:
                    if (item.Quality >= ItemQuality.Legendary)
                        return Trinity.Settings.Loot.Pickup.PickupLegendaries;

                    return CheckLevelRequirements(item.Level, item.Quality, Trinity.Settings.Loot.Pickup.PickupBlueArmor, Trinity.Settings.Loot.Pickup.PickupYellowArmor);
                case GItemBaseType.Jewelry:
                    if (item.Quality >= ItemQuality.Legendary)
                        return Trinity.Settings.Loot.Pickup.PickupLegendaries;

                    return CheckLevelRequirements(item.Level, item.Quality, Trinity.Settings.Loot.Pickup.PickupBlueJewlery, Trinity.Settings.Loot.Pickup.PickupYellowJewlery);
                case GItemBaseType.FollowerItem:
                    if (item.Quality >= ItemQuality.Legendary)
                        return Trinity.Settings.Loot.Pickup.PickupLegendaryFollowerItems;

                    if (item.Quality >= ItemQuality.Magic1 && item.Quality <= ItemQuality.Magic3)
                        return Trinity.Settings.Loot.Pickup.PickupBlueFollowerItems;

                    if (item.Quality >= ItemQuality.Rare4 && item.Quality <= ItemQuality.Rare6)
                        return Trinity.Settings.Loot.Pickup.PickupYellowFollowerItems;
                    // not matched above, ignore it
                    return false;
                case GItemBaseType.Gem:
                    if (item.Level < Trinity.Settings.Loot.Pickup.GemLevel ||
                        (itemType == GItemType.Ruby && !Trinity.Settings.Loot.Pickup.GemType.HasFlag(TrinityGemType.Ruby)) ||
                        (itemType == GItemType.Emerald && !Trinity.Settings.Loot.Pickup.GemType.HasFlag(TrinityGemType.Emerald)) ||
                        (itemType == GItemType.Amethyst && !Trinity.Settings.Loot.Pickup.GemType.HasFlag(TrinityGemType.Amethyst)) ||
                        (itemType == GItemType.Topaz && !Trinity.Settings.Loot.Pickup.GemType.HasFlag(TrinityGemType.Topaz)) ||
                        (itemType == GItemType.Diamond && !Trinity.Settings.Loot.Pickup.GemType.HasFlag(TrinityGemType.Diamond)))
                    {
                        return false;
                    }
                    break;
                case GItemBaseType.Misc:
                    if (item.ACDItem.GetTrinityItemQuality() < Trinity.Settings.Loot.Pickup.MiscItemQuality)
                        return false;

                    // Potion filtering
                    if (itemType == GItemType.HealthPotion && item.Quality < ItemQuality.Legendary)
                    {
                        int potionsInBackPack = ZetaDia.Me.Inventory.Backpack.Where(p => p.ItemType == ItemType.Potion).Sum(p => p.ItemStackQuantity);

                        if (potionsInBackPack >= Trinity.Settings.Loot.Pickup.PotionCount)
                            return false;
                        return true;
                    }
                    break;
                case GItemBaseType.HealthGlobe:
                    return true;
                case GItemBaseType.ProgressionGlobe:
                    return true;
                case GItemBaseType.Unknown:
                    return false;
                default:
                    return false;
            }

            // Didn't cancel it, so default to true!
            return true;
        }

        /// <summary>
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        internal static bool IdentifyItemValidation(PickupItem item)
        {
            if (Trinity.Settings.Loot.TownRun.KeepLegendaryUnid)
                return false;
            return true;
        }

        internal static bool ItemRulesIdentifyValidation(ACDItem item)
        {
            ItemEvents.ResetTownRun();
            var pickupItem = new PickupItem(
                item.Name,
                item.InternalName,
                item.Level,
                item.ItemQualityLevel,
                item.GameBalanceId,
                item.ItemBaseType,
                item.ItemType,
                item.IsOneHand,
                item.IsTwoHand,
                item.FollowerSpecialType,
                item.ACDGuid,
                item.DynamicId);

            Logger.Log(TrinityLogLevel.Info, LogCategory.ItemValuation,
                "Incoming Identification Request: {0}, {1}, {2}, {3}, {4}",
                pickupItem.Quality, pickupItem.Level, pickupItem.DBBaseType,
                pickupItem.DBItemType, pickupItem.IsOneHand ? "1H" : pickupItem.IsTwoHand ? "2H" : "NH");

            if (Trinity.StashRule != null)
            {
                // using ItemEvaluationType.Identify isn't available so we are abusing Sell for that manner
                Interpreter.InterpreterAction action = Trinity.StashRule.checkPickUpItem(pickupItem, ItemEvaluationType.Sell);

                Logger.Log(TrinityLogLevel.Debug, LogCategory.ItemValuation, "Action is: {0}", action);

                switch (action)
                {
                    case Interpreter.InterpreterAction.IDENTIFY:
                        return true;
                    case Interpreter.InterpreterAction.UNIDENT:
                        return false;
                    default:
                        Logger.Log(TrinityLogLevel.Info, LogCategory.ScriptRule, "Trinity, item is unhandled by ItemRules (Identification)!");
                        return IdentifyItemValidation(pickupItem);
                }
            }
            return IdentifyItemValidation(pickupItem);
        }

        /// <summary>
        ///     Checks if current item's level is according to min level for Pickup.
        /// </summary>
        /// <param name="level">The current item's level.</param>
        /// <param name="quality">The item's quality.</param>
        /// <param name="pickupBlue">Pickup Blue items</param>
        /// <param name="pickupYellow">Pikcup Yellow items</param>
        /// <returns></returns>
        internal static bool CheckLevelRequirements(int level, ItemQuality quality, bool pickupBlue, bool pickupYellow)
        {
            // Gray Items
            if (quality < ItemQuality.Normal)
            {
                if (Trinity.Settings.Loot.Pickup.PickupGrayItems)
                    return true;
                return false;
            }

            // White Items
            if (quality == ItemQuality.Normal || quality == ItemQuality.Superior)
            {
                if (Trinity.Settings.Loot.Pickup.PickupWhiteItems)
                    return true;
                return false;
            }

            if (quality < ItemQuality.Normal && Trinity.Player.Level > 5 && !Trinity.Settings.Loot.Pickup.PickupLowLevel)
            {
                // Grey item, ignore if we're over level 5
                return false;
            }

            // Ignore Gray/White if player level is <= 5 and we are picking up white
            if (quality <= ItemQuality.Normal && Trinity.Player.Level <= 5 && !Trinity.Settings.Loot.Pickup.PickupLowLevel)
            {
                return false;
            }

            if (quality < ItemQuality.Magic1 && Trinity.Player.Level > 10)
            {
                // White item, ignore if we're over level 10
                return false;
            }

            // PickupLowLevel setting
            if (quality <= ItemQuality.Magic1 && Trinity.Player.Level <= 10 && !Trinity.Settings.Loot.Pickup.PickupLowLevel)
            {
                // ignore if we don't have the setting enabled
                return false;
            }

            // Blue/Yellow get scored
            if (quality >= ItemQuality.Magic1 && quality < ItemQuality.Rare4 && !pickupBlue)
            {
                return false;
            }
            if (quality >= ItemQuality.Rare4 && quality < ItemQuality.Legendary && !pickupYellow)
            {
                return false;
            }
            return true;
        }

        // private static Regex x1Regex = new Regex("^x1_", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant);

        /// <summary>
        ///     DetermineItemType - Calculates what kind of item it is from D3 internalnames
        /// </summary>
        /// <param name="name"></param>
        /// <param name="dbItemType"></param>
        /// <param name="dbFollowerType"></param>
        /// <returns></returns>
        internal static GItemType DetermineItemType(string name, ItemType dbItemType, FollowerType dbFollowerType = FollowerType.None)
        {
            name = name.ToLower();
            if (name.StartsWith("x1_")) name = name.Substring(3, name.Length - 3);
            if (name.StartsWith("p1_")) name = name.Substring(3, name.Length - 3);

            if (name.StartsWith("a1_")) return GItemType.SpecialItem;
            if (name.StartsWith("amethyst")) return GItemType.Amethyst;
            if (name.StartsWith("amulet_")) return GItemType.Amulet;
            if (name.StartsWith("axe_")) return GItemType.Axe;
            if (name.StartsWith("barbbelt_")) return GItemType.MightyBelt;
            if (name.StartsWith("blacksmithstome")) return GItemType.CraftTome;
            if (name.StartsWith("boots_")) return GItemType.Boots;
            if (name.StartsWith("bow_")) return GItemType.TwoHandBow;
            if (name.StartsWith("bracers_")) return GItemType.Bracer;
            if (name.StartsWith("ceremonialdagger_")) return GItemType.CeremonialKnife;
            if (name.StartsWith("cloak_")) return GItemType.Cloak;
            if (name.StartsWith("combatstaff_")) return GItemType.TwoHandDaibo;
            if (name.StartsWith("crafting_")) return GItemType.CraftingMaterial;
            if (name.StartsWith("craftingmaterials_")) return GItemType.CraftingMaterial;
            if (name.StartsWith("craftingplan_")) return GItemType.CraftingPlan;
            if (name.StartsWith("craftingreagent_legendary_")) return GItemType.CraftingMaterial;
            if (name.StartsWith("crushield_")) return GItemType.CrusaderShield;
            if (name.StartsWith("dagger_")) return GItemType.Dagger;
            if (name.StartsWith("diamond_")) return GItemType.Diamond;
            if (name.StartsWith("dye_")) return GItemType.Dye;
            if (name.StartsWith("emerald_")) return GItemType.Emerald;
            if (name.StartsWith("fistweapon_")) return GItemType.FistWeapon;
            if (name.StartsWith("flail1h_")) return GItemType.Flail;
            if (name.StartsWith("flail2h_")) return GItemType.TwoHandFlail;
            if (name.StartsWith("followeritem_enchantress_") || dbFollowerType == FollowerType.Enchantress) return GItemType.FollowerEnchantress;
            if (name.StartsWith("followeritem_scoundrel_") || dbFollowerType == FollowerType.Scoundrel) return GItemType.FollowerScoundrel;
            if (name.StartsWith("followeritem_templar_") || dbFollowerType == FollowerType.Templar) return GItemType.FollowerTemplar;
            if (name.StartsWith("gloves_")) return GItemType.Gloves;
            if (name.StartsWith("handxbow_")) return GItemType.HandCrossbow;
            if (name.StartsWith("healthglobe")) return GItemType.HealthGlobe;
            if (name.StartsWith("healthpotion")) return GItemType.HealthPotion;
            if (name.StartsWith("horadriccache")) return GItemType.HoradricCache;
            if (name.StartsWith("lore_book_")) return GItemType.CraftTome;
            if (name.StartsWith("lootrunkey")) return GItemType.LootRunKey;
            if (name.StartsWith("mace_")) return GItemType.Mace;
            if (name.StartsWith("mightyweapon_1h_")) return GItemType.MightyWeapon;
            if (name.StartsWith("mightyweapon_2h_")) return GItemType.TwoHandMighty;
            if (name.StartsWith("mojo_")) return GItemType.Mojo;
            if (name.StartsWith("orb_")) return GItemType.Orb;
            if (name.StartsWith("page_of_")) return GItemType.CraftTome;
            if (name.StartsWith("pants_")) return GItemType.Legs;
            if (name.StartsWith("polearm_") || dbItemType == ItemType.Polearm) return GItemType.TwoHandPolearm;
            if (name.StartsWith("quiver_")) return GItemType.Quiver;
            if (name.StartsWith("ring_")) return GItemType.Ring;
            if (name.StartsWith("ruby_")) return GItemType.Ruby;
            if (name.StartsWith("shield_")) return GItemType.Shield;
            if (name.StartsWith("shoulderpads_")) return GItemType.Shoulder;
            if (name.StartsWith("spear_")) return GItemType.Spear;
            if (name.StartsWith("spiritstone_")) return GItemType.SpiritStone;
            if (name.StartsWith("staff_")) return GItemType.TwoHandStaff;
            if (name.StartsWith("staffofcow")) return GItemType.StaffOfHerding;
            if (name.StartsWith("sword_")) return GItemType.Sword;
            if (name.StartsWith("topaz_")) return GItemType.Topaz;
            if (name.StartsWith("twohandedaxe_")) return GItemType.TwoHandAxe;
            if (name.StartsWith("twohandedmace_")) return GItemType.TwoHandMace;
            if (name.StartsWith("twohandedsword_")) return GItemType.TwoHandSword;
            if (name.StartsWith("voodoomask_")) return GItemType.VoodooMask;
            if (name.StartsWith("wand_")) return GItemType.Wand;
            if (name.StartsWith("wizardhat_")) return GItemType.WizardHat;
            if (name.StartsWith("xbow_")) return GItemType.TwoHandCrossbow;
            if (name.StartsWith("console_powerglobe")) return GItemType.PowerGlobe;
            if (name.StartsWith("tiered_rifts_orb")) return GItemType.ProgressionGlobe;
            if (name.StartsWith("consumable_add_sockets")) return GItemType.ConsumableAddSockets; // Ramaladni's Gift
            if (name.StartsWith("tieredlootrunkey_")) return GItemType.TieredLootrunKey;
            if (name.StartsWith("demonkey_") || name.StartsWith("demontrebuchetkey") || name.StartsWith("quest_")) return GItemType.InfernalKey;
            if (name.StartsWith("offhand_")) return GItemType.Mojo;
            if (name.StartsWith("horadricrelic")) return GItemType.HoradricRelic;


            // Follower item types
            if (name.StartsWith("jewelbox_") || dbItemType == ItemType.FollowerSpecial)
            {
                if (dbFollowerType == FollowerType.Scoundrel)
                    return GItemType.FollowerScoundrel;
                if (dbFollowerType == FollowerType.Templar)
                    return GItemType.FollowerTemplar;
                if (dbFollowerType == FollowerType.Enchantress)
                    return GItemType.FollowerEnchantress;
            }

            // Fall back on some partial DB item type checking 
            if (name.StartsWith("crafting_"))
            {
                if (dbItemType == ItemType.CraftingPage)
                    return GItemType.CraftTome;
                return GItemType.CraftingMaterial;
            }
            if (name.StartsWith("chestarmor_"))
            {
                if (dbItemType == ItemType.Cloak)
                    return GItemType.Cloak;
                return GItemType.Chest;
            }
            if (name.StartsWith("helm_"))
            {
                if (dbItemType == ItemType.SpiritStone)
                    return GItemType.SpiritStone;
                if (dbItemType == ItemType.VoodooMask)
                    return GItemType.VoodooMask;
                if (dbItemType == ItemType.WizardHat)
                    return GItemType.WizardHat;
                return GItemType.Helm;
            }
            if (name.StartsWith("helmcloth_"))
            {
                if (dbItemType == ItemType.SpiritStone)
                    return GItemType.SpiritStone;
                if (dbItemType == ItemType.VoodooMask)
                    return GItemType.VoodooMask;
                if (dbItemType == ItemType.WizardHat)
                    return GItemType.WizardHat;
                return GItemType.Helm;
            }
            if (name.StartsWith("belt_"))
            {
                if (dbItemType == ItemType.MightyBelt)
                    return GItemType.MightyBelt;
                return GItemType.Belt;
            }
            return GItemType.Unknown;
        }

        /// <summary>
        ///     DetermineBaseType - Calculates a more generic, "basic" type of item
        /// </summary>
        /// <param name="itemType"></param>
        /// <returns></returns>
        internal static GItemBaseType DetermineBaseType(GItemType itemType)
        {
            var itemBaseType = GItemBaseType.Misc;

            // One Handed Weapons
            switch (itemType)
            {
                case GItemType.Axe:
                case GItemType.CeremonialKnife:
                case GItemType.Dagger:
                case GItemType.Flail:
                case GItemType.FistWeapon:
                case GItemType.Mace:
                case GItemType.MightyWeapon:
                case GItemType.Spear:
                case GItemType.Sword:
                case GItemType.Wand:
                    {

                        itemBaseType = GItemBaseType.WeaponOneHand;
                        break;
                    }
                // Two Handed Weapons
                case GItemType.TwoHandDaibo:
                case GItemType.TwoHandMace:
                case GItemType.TwoHandFlail:
                case GItemType.TwoHandMighty:
                case GItemType.TwoHandPolearm:
                case GItemType.TwoHandStaff:
                case GItemType.TwoHandSword:
                case GItemType.TwoHandAxe:
                    {
                        itemBaseType = GItemBaseType.WeaponTwoHand;
                        break;
                    }
                // Ranged Weapons
                case GItemType.TwoHandCrossbow:
                case GItemType.HandCrossbow:
                case GItemType.TwoHandBow:
                    {
                        itemBaseType = GItemBaseType.WeaponRange;
                        break;
                    }
                // Off-hands
                case GItemType.Mojo:
                case GItemType.Orb:
                case GItemType.CrusaderShield:
                case GItemType.Quiver:
                case GItemType.Shield:
                    {
                        itemBaseType = GItemBaseType.Offhand;
                        break;
                    }
                // Armors
                case GItemType.Boots:
                case GItemType.Bracer:
                case GItemType.Chest:
                case GItemType.Cloak:
                case GItemType.Gloves:
                case GItemType.Helm:
                case GItemType.Legs:
                case GItemType.Shoulder:
                case GItemType.SpiritStone:
                case GItemType.VoodooMask:
                case GItemType.WizardHat:
                case GItemType.Belt:
                case GItemType.MightyBelt:
                    {
                        itemBaseType = GItemBaseType.Armor;
                        break;
                    }
                // Jewlery
                case GItemType.Amulet:
                case GItemType.Ring:
                    {
                        itemBaseType = GItemBaseType.Jewelry;
                        break;
                    }
                // Follower Items
                case GItemType.FollowerEnchantress:
                case GItemType.FollowerScoundrel:
                case GItemType.FollowerTemplar:
                    {
                        itemBaseType = GItemBaseType.FollowerItem;
                        break;
                    }
                // Misc Items
                case GItemType.CraftingMaterial:
                case GItemType.CraftTome:
                case GItemType.LootRunKey:
                case GItemType.HoradricRelic:
                case GItemType.SpecialItem:
                case GItemType.CraftingPlan:
                case GItemType.HealthPotion:
                case GItemType.HoradricCache:
                case GItemType.Dye:
                case GItemType.StaffOfHerding:
                case GItemType.InfernalKey:
                case GItemType.ConsumableAddSockets:
                case GItemType.TieredLootrunKey:
                    {
                        itemBaseType = GItemBaseType.Misc;
                        break;
                    }
                // Gems
                case GItemType.Ruby:
                case GItemType.Emerald:
                case GItemType.Topaz:
                case GItemType.Amethyst:
                case GItemType.Diamond:
                    {
                        itemBaseType = GItemBaseType.Gem;
                        break;
                    }
                // Globes
                case GItemType.HealthGlobe:
                    {
                        itemBaseType = GItemBaseType.HealthGlobe;
                        break;
                    }
                case GItemType.PowerGlobe:
                    {
                        itemBaseType = GItemBaseType.PowerGlobe;
                        break;
                    }
                case GItemType.ProgressionGlobe:
                    {
                        itemBaseType = GItemBaseType.ProgressionGlobe;
                        break;
                    }
            }
            return itemBaseType;
        }

        internal static bool IsEquipment(CachedACDItem i)
        {
            return (i.DBBaseType == ItemBaseType.Armor || i.DBBaseType == ItemBaseType.Jewelry || i.DBBaseType == ItemBaseType.Weapon);
        }
    }
}