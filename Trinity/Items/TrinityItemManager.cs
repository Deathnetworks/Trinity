using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
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
            return TrinitySell(cItem);
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

            // Always stash ancients setting
            if (Trinity.Settings.Loot.TownRun.AlwaysStashAncients && item.AncientRank > 0)
                return true;

            CachedACDItem cItem = CachedACDItem.GetCachedItem(item);
            // Now look for Misc items we might want to keep
            TrinityItemType tItemType = cItem.TrinityItemType; // DetermineItemType(cItem.InternalName, cItem.DBItemType, cItem.FollowerType);
            TrinityItemBaseType tBaseType = cItem.TrinityItemBaseType; // DetermineBaseType(trinityItemType);

            bool isEquipment = (tBaseType == TrinityItemBaseType.Armor ||
                tBaseType == TrinityItemBaseType.Jewelry ||
                tBaseType == TrinityItemBaseType.Offhand ||
                tBaseType == TrinityItemBaseType.WeaponOneHand ||
                tBaseType == TrinityItemBaseType.WeaponRange ||
                tBaseType == TrinityItemBaseType.WeaponTwoHand);

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

            if (cItem.TrinityItemType == TrinityItemType.HoradricCache && Trinity.Settings.Loot.TownRun.OpenHoradricCaches)
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
            if (tItemType == TrinityItemType.StaffOfHerding)
            {
                if (evaluationType == ItemEvaluationType.Keep)
                    Logger.Log(TrinityLogLevel.Info, LogCategory.ItemValuation, "{0} [{1}] [{2}] = (autokeep staff of herding)", cItem.RealName, cItem.InternalName, tItemType);
                return true;
            }
            if (tItemType == TrinityItemType.CraftingMaterial)
            {
                var craftMaterialType = GetCraftingMaterialType(cItem);
                if (evaluationType == ItemEvaluationType.Keep && craftMaterialType != CraftingMaterialType.Unknown)
                {
                    var stackCount = GetItemStackCount(cItem, InventorySlot.SharedStash);
                    if (craftMaterialType == CraftingMaterialType.ArcaneDust && stackCount >= Trinity.Settings.Loot.TownRun.MaxStackArcaneDust)
                    {
                        Logger.Log("Already have {0} of {1}, max {2} (TRASH)", stackCount, craftMaterialType, Trinity.Settings.Loot.TownRun.MaxStackArcaneDust);
                        return false;
                    }

                    if (craftMaterialType == CraftingMaterialType.DeathsBreath && stackCount >= Trinity.Settings.Loot.TownRun.MaxStackDeathsBreath)
                    {
                        Logger.Log("Already have {0} of {1}, max {2} (TRASH)", stackCount, craftMaterialType, Trinity.Settings.Loot.TownRun.MaxStackDeathsBreath);
                        return false;
                    }

                    if (craftMaterialType == CraftingMaterialType.ForgottonSoul && stackCount >= Trinity.Settings.Loot.TownRun.MaxStackForgottonSoul)
                    {
                        Logger.Log("Already have {0} of {1}, max {2} (TRASH)", stackCount, craftMaterialType, Trinity.Settings.Loot.TownRun.MaxStackForgottonSoul);
                        return false;
                    }

                    if (craftMaterialType == CraftingMaterialType.ReusableParts && stackCount >= Trinity.Settings.Loot.TownRun.MaxStackReusableParts)
                    {
                        Logger.Log("Already have {0} of {1}, max {2} (TRASH)", stackCount, craftMaterialType, Trinity.Settings.Loot.TownRun.MaxStackReusableParts);
                        return false;
                    }

                    if (craftMaterialType == CraftingMaterialType.VeiledCrystal && stackCount >= Trinity.Settings.Loot.TownRun.MaxStackVeiledCrystal)
                    {
                        Logger.Log("Already have {0} of {1}, max {2} (TRASH)", stackCount, craftMaterialType, Trinity.Settings.Loot.TownRun.MaxStackVeiledCrystal);
                        return false;
                    }
                }

                if (evaluationType == ItemEvaluationType.Keep)
                    Logger.Log(TrinityLogLevel.Info, LogCategory.ItemValuation, "{0} [{1}] [{2}] = (autokeep craft materials)", cItem.RealName, cItem.InternalName, tItemType);
                return true;
            }

            if (tItemType == TrinityItemType.Emerald || tItemType == TrinityItemType.Amethyst || tItemType == TrinityItemType.Topaz || tItemType == TrinityItemType.Ruby || tItemType == TrinityItemType.Diamond)
            {
                if (evaluationType == ItemEvaluationType.Keep)
                    Logger.Log(TrinityLogLevel.Info, LogCategory.ItemValuation, "{0} [{1}] [{2}] = (autokeep gems)", cItem.RealName, cItem.InternalName, tItemType);
                return true;
            }
            if (tItemType == TrinityItemType.CraftTome)
            {
                if (evaluationType == ItemEvaluationType.Keep)
                    Logger.Log(TrinityLogLevel.Info, LogCategory.ItemValuation, "{0} [{1}] [{2}] = (autokeep tomes)", cItem.RealName, cItem.InternalName, tItemType);
                return true;
            }
            if (tItemType == TrinityItemType.InfernalKey)
            {
                if (evaluationType == ItemEvaluationType.Keep)
                    Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "{0} [{1}] [{2}] = (autokeep infernal key)", cItem.RealName, cItem.InternalName, tItemType);
                return true;
            }

            if (tItemType == TrinityItemType.HealthPotion && item.ItemQualityLevel >= ItemQuality.Legendary)
            {
                var shouldStash = Trinity.Settings.Loot.TownRun.StashLegendaryPotions;
                if (evaluationType == ItemEvaluationType.Keep)
                    Logger.Log(TrinityLogLevel.Info, LogCategory.ItemValuation, "{0} [{1}] [{2}] = ({3} legendary potions)", cItem.RealName, cItem.InternalName, tItemType,
                        shouldStash ? "stashing" : "ignoring");
                return shouldStash;
            }

            if (tItemType == TrinityItemType.HealthPotion && item.ItemQualityLevel < ItemQuality.Legendary)
            {
                if (evaluationType == ItemEvaluationType.Keep)
                    Logger.Log(TrinityLogLevel.Info, LogCategory.ItemValuation, "{0} [{1}] [{2}] = (ignoring potions)", cItem.RealName, cItem.InternalName, tItemType);
                return false;
            }

            if (tItemType == TrinityItemType.CraftingPlan && cItem.Quality >= ItemQuality.Legendary)
            {
                if (evaluationType == ItemEvaluationType.Keep)
                    Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "{0} [{1}] [{2}] = (autokeep legendary plans)", cItem.RealName, cItem.InternalName, tItemType);
                return true;
            }

            if (tItemType == TrinityItemType.ConsumableAddSockets)
            {
                if (evaluationType == ItemEvaluationType.Keep)
                    Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "{0} [{1}] [{2}] = (autokeep Ramaladni's Gift)", cItem.RealName, cItem.InternalName, tItemType);
                return true;
            }

            if (tItemType == TrinityItemType.TieredLootrunKey)
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

            if (tItemType == TrinityItemType.CraftingPlan)
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
            if (cItem.Quality <= ItemQuality.Superior && (isEquipment || cItem.TrinityItemBaseType == TrinityItemBaseType.FollowerItem))
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
            if (cItem.Quality >= ItemQuality.Magic1 && cItem.Quality <= ItemQuality.Magic3 && (isEquipment || cItem.TrinityItemBaseType == TrinityItemBaseType.FollowerItem))
            {
                Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "{0} [{1}] [{2}] = (trashing blues)", cItem.RealName, cItem.InternalName, tItemType);
                return false;
            }

            // Force salvage Rares
            if (Trinity.Settings.Loot.TownRun.ForceSalvageRares && cItem.Quality >= ItemQuality.Rare4 && cItem.Quality <= ItemQuality.Rare6 && (isEquipment || cItem.TrinityItemBaseType == TrinityItemBaseType.FollowerItem))
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

            if (!cItem.IsSalvageable)
                return false;

            // Vanity Items
            if (DataDictionary.VanityItems.Any(i => item.InternalName.StartsWith(i)))
                return false;

            if (item.ItemType == ItemType.KeystoneFragment && item.TieredLootRunKeyLevel >= 0)
                return false;

            // Stashing Whites
            if (Trinity.Settings.Loot.TownRun.StashWhites && cItem.Quality < ItemQuality.Magic1)
                return false;

            // Stashing Blues
            if (Trinity.Settings.Loot.TownRun.StashBlues && cItem.Quality > ItemQuality.Superior && cItem.Quality < ItemQuality.Rare4)
                return false;

            // Take Salvage Option corresponding to ItemLevel
            SalvageOption salvageOption = GetSalvageOption(cItem.Quality);

            if (salvageOption == SalvageOption.Salvage)
                return true;

            switch (cItem.TrinityItemBaseType)
            {
                case TrinityItemBaseType.WeaponRange:
                case TrinityItemBaseType.WeaponOneHand:
                case TrinityItemBaseType.WeaponTwoHand:
                case TrinityItemBaseType.Armor:
                case TrinityItemBaseType.Offhand:
                    return salvageOption == SalvageOption.Salvage;
                case TrinityItemBaseType.Jewelry:
                    return salvageOption == SalvageOption.Salvage;
                case TrinityItemBaseType.FollowerItem:
                    return salvageOption == SalvageOption.Salvage;
                case TrinityItemBaseType.Gem:
                case TrinityItemBaseType.Misc:
                case TrinityItemBaseType.Unknown:
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
        internal static bool TrinitySell(ACDItem item)
        {
            return TrinitySell(CachedACDItem.GetCachedItem(item));
        }

        /// <summary>
        /// Determines if we should Sell an Item
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        internal static bool TrinitySell(CachedACDItem item)
        {

            // Vanity Items
            if (DataDictionary.VanityItems.Any(i => item.InternalName.StartsWith(i)))
                return false;

            if (item.DBItemType == ItemType.KeystoneFragment && item.AcdItem.TieredLootRunKeyLevel >= 0)
                return false;

            if (item.DBItemType == ItemType.HoradricCache)
                return false;

            if (Trinity.Settings.Loot.TownRun.ApplyPickupValidationToStashing)
            {
                var pItem = new PickupItem(item.AcdItem, item.TrinityItemBaseType, item.TrinityItemType);
                var pickupCheck = PickupItemValidation(pItem);
                if (!pickupCheck)
                    return true;
            }

            switch (item.TrinityItemBaseType)
            {
                case TrinityItemBaseType.WeaponRange:
                case TrinityItemBaseType.WeaponOneHand:
                case TrinityItemBaseType.WeaponTwoHand:
                case TrinityItemBaseType.Armor:
                case TrinityItemBaseType.Offhand:
                case TrinityItemBaseType.Jewelry:
                case TrinityItemBaseType.FollowerItem:
                    return true;
                case TrinityItemBaseType.Gem:
                case TrinityItemBaseType.Misc:
                    if (item.TrinityItemType == TrinityItemType.CraftingPlan)
                        return true;
                    if (item.TrinityItemType == TrinityItemType.CraftingMaterial)
                        return true;
                    return false;
                case TrinityItemBaseType.Unknown:
                    return false;
            }

            return false;
        }
        private static SalvageOption GetSalvageOption(ItemQuality quality)
        {
            if (quality >= ItemQuality.Inferior && quality <= ItemQuality.Superior)
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

                List<ItemWrapper> itemList = new List<ItemWrapper>();

                switch (location)
                {
                    case DumpItemLocation.All:
                        itemList = ZetaDia.Actors.GetActorsOfType<ACDItem>(true).Select(i => new ItemWrapper(i)).OrderBy(i => i.InventorySlot).ThenBy(i => i.Name).ToList();
                        break;
                    case DumpItemLocation.Backpack:
                        itemList = ZetaDia.Me.Inventory.Backpack.Select(i => new ItemWrapper(i)).ToList();
                        break;
                    case DumpItemLocation.Merchant:
                        itemList = ZetaDia.Me.Inventory.MerchantItems.Select(i => new ItemWrapper(i)).ToList();
                        break;
                    case DumpItemLocation.Ground:
                        itemList = ZetaDia.Actors.GetActorsOfType<DiaItem>(true).Select(i => new ItemWrapper(i.CommonData)).ToList();
                        break;
                    case DumpItemLocation.Equipped:
                        itemList = ZetaDia.Me.Inventory.Equipped.Select(i => new ItemWrapper(i)).ToList();
                        break;
                    case DumpItemLocation.Stash:
                        if (UIElements.StashWindow.IsVisible)
                        {
                            itemList = ZetaDia.Me.Inventory.StashItems.Select(i => new ItemWrapper(i)).ToList();
                        }
                        else
                        {
                            Logger.Log("Stash window not open!");
                        }
                        break;
                }

                itemList.RemoveAll(i => i == null);
                //itemList.RemoveAll(i => !i.IsValid);

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
                            int iVal = item.Item.GetAttribute<int>((ActorAttributeType)val);
                            float fVal = item.Item.GetAttribute<float>((ActorAttributeType)val);

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
                        Logger.Log("Link Color ItemQuality=" + item.Item.ItemLinkColorQuality());
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
                    if (_lastBackPackLocation != new Vector2(-2, -2) &&
                        _lastBackPackCount == CacheData.Inventory.Backpack.Count &&
                        _lastProtectedSlotsCount == CharacterSettings.Instance.ProtectedBagSlots.Count)
                    {
                        return _lastBackPackLocation;
                    }

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
            TrinityItemType itemType = DetermineItemType(item.InternalName, item.DBItemType, item.ItemFollowerType);
            TrinityItemBaseType baseType = DetermineBaseType(itemType);

            // Pickup Ramaladni's Gift
            if (itemType == TrinityItemType.ConsumableAddSockets)
            {
                return Trinity.Settings.Loot.Pickup.RamadalinisGift;
            }

            // Tiered Rift Keys
            if (itemType == TrinityItemType.TieredLootrunKey)
            {
                return Trinity.Settings.Loot.Pickup.LootRunKey;
            }

            // Pickup Legendary potions
            if (itemType == TrinityItemType.HealthPotion && item.Quality >= ItemQuality.Legendary)
            {
                return Trinity.Settings.Loot.Pickup.LegendaryPotions;
            }

            if (itemType == TrinityItemType.InfernalKey)
            {
                return Trinity.Settings.Loot.Pickup.InfernalKeys;
            }

            // Rift Keystone Fragments == LootRunkey
            if (itemType == TrinityItemType.LootRunKey)
            {
                return Trinity.Settings.Loot.Pickup.LootRunKey;
            }

            // Blood Shards == HoradricRelic
            if (itemType == TrinityItemType.HoradricRelic && ZetaDia.CPlayer.BloodshardCount < Trinity.Player.MaxBloodShards)
            {
                return Trinity.Settings.Loot.Pickup.BloodShards;
            }

            if (itemType == TrinityItemType.CraftingMaterial && (item.ACDItem.GetTrinityItemQuality() < Trinity.Settings.Loot.Pickup.MiscItemQuality || !Trinity.Settings.Loot.Pickup.CraftMaterials))
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

            if (itemType == TrinityItemType.CraftingPlan && item.Quality >= ItemQuality.Legendary && Trinity.Settings.Loot.Pickup.LegendaryPlans)
            {
                return true;
            }

            if (item.IsUpgrade && Trinity.Settings.Loot.Pickup.PickupUpgrades)
            {
                return true;
            }

            switch (baseType)
            {
                case TrinityItemBaseType.WeaponTwoHand:
                case TrinityItemBaseType.WeaponOneHand:
                case TrinityItemBaseType.WeaponRange:
                    if (item.Quality >= ItemQuality.Legendary)
                        return Trinity.Settings.Loot.Pickup.PickupLegendaries;

                    return CheckLevelRequirements(item.Level, item.Quality, Trinity.Settings.Loot.Pickup.PickupBlueWeapons, Trinity.Settings.Loot.Pickup.PickupYellowWeapons);
                case TrinityItemBaseType.Armor:
                case TrinityItemBaseType.Offhand:
                    if (item.Quality >= ItemQuality.Legendary)
                        return Trinity.Settings.Loot.Pickup.PickupLegendaries;

                    return CheckLevelRequirements(item.Level, item.Quality, Trinity.Settings.Loot.Pickup.PickupBlueArmor, Trinity.Settings.Loot.Pickup.PickupYellowArmor);
                case TrinityItemBaseType.Jewelry:
                    if (item.Quality >= ItemQuality.Legendary)
                        return Trinity.Settings.Loot.Pickup.PickupLegendaries;

                    return CheckLevelRequirements(item.Level, item.Quality, Trinity.Settings.Loot.Pickup.PickupBlueJewlery, Trinity.Settings.Loot.Pickup.PickupYellowJewlery);
                case TrinityItemBaseType.FollowerItem:
                    if (item.Quality >= ItemQuality.Legendary)
                        return Trinity.Settings.Loot.Pickup.PickupLegendaryFollowerItems;

                    if (item.Quality >= ItemQuality.Magic1 && item.Quality <= ItemQuality.Magic3)
                        return Trinity.Settings.Loot.Pickup.PickupBlueFollowerItems;

                    if (item.Quality >= ItemQuality.Rare4 && item.Quality <= ItemQuality.Rare6)
                        return Trinity.Settings.Loot.Pickup.PickupYellowFollowerItems;
                    // not matched above, ignore it
                    return false;
                case TrinityItemBaseType.Gem:
                    if (item.Level < Trinity.Settings.Loot.Pickup.GemLevel ||
                        (itemType == TrinityItemType.Ruby && !Trinity.Settings.Loot.Pickup.GemType.HasFlag(TrinityGemType.Ruby)) ||
                        (itemType == TrinityItemType.Emerald && !Trinity.Settings.Loot.Pickup.GemType.HasFlag(TrinityGemType.Emerald)) ||
                        (itemType == TrinityItemType.Amethyst && !Trinity.Settings.Loot.Pickup.GemType.HasFlag(TrinityGemType.Amethyst)) ||
                        (itemType == TrinityItemType.Topaz && !Trinity.Settings.Loot.Pickup.GemType.HasFlag(TrinityGemType.Topaz)) ||
                        (itemType == TrinityItemType.Diamond && !Trinity.Settings.Loot.Pickup.GemType.HasFlag(TrinityGemType.Diamond)))
                    {
                        return false;
                    }
                    break;
                case TrinityItemBaseType.Misc:
                    if (item.ACDItem.GetTrinityItemQuality() < Trinity.Settings.Loot.Pickup.MiscItemQuality)
                        return false;

                    // Potion filtering
                    if (itemType == TrinityItemType.HealthPotion && item.Quality < ItemQuality.Legendary)
                    {
                        long potionsInBackPack = ZetaDia.Me.Inventory.Backpack.Where(p => p.ItemType == ItemType.Potion).Sum(p => p.ItemStackQuantity);

                        if (potionsInBackPack >= Trinity.Settings.Loot.Pickup.PotionCount)
                            return false;
                        return true;
                    }
                    break;
                case TrinityItemBaseType.HealthGlobe:
                    return true;
                case TrinityItemBaseType.ProgressionGlobe:
                    return true;
                case TrinityItemBaseType.Unknown:
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

        internal static TrinityItemType DetermineItemType(ACDItem item)
        {
            return DetermineItemType(item.InternalName, item.ItemType);
        }

        private static readonly Regex ItemExpansionRegex = new Regex(@"^[xp]\d_", RegexOptions.Compiled);

        /// <summary>
        ///     DetermineItemType - Calculates what kind of item it is from D3 internalnames
        /// </summary>
        /// <param name="name"></param>
        /// <param name="dbItemType"></param>
        /// <param name="dbFollowerType"></param>
        /// <returns></returns>
        internal static TrinityItemType DetermineItemType(string name, ItemType dbItemType, FollowerType dbFollowerType = FollowerType.None)
        {
            name = name.ToLower();
            if (name.StartsWith("x1_")) name = name.Substring(3, name.Length - 3);
            if (name.StartsWith("p1_")) name = name.Substring(3, name.Length - 3);
            if (name.StartsWith("p2_")) name = name.Substring(3, name.Length - 3);
            if (ItemExpansionRegex.IsMatch(name)) name = name.Substring(3, name.Length - 3);

            if (name.StartsWith("a1_")) return TrinityItemType.SpecialItem;
            if (name.StartsWith("amethyst")) return TrinityItemType.Amethyst;
            if (name.StartsWith("amulet_")) return TrinityItemType.Amulet;
            if (name.StartsWith("axe_")) return TrinityItemType.Axe;
            if (name.StartsWith("barbbelt_")) return TrinityItemType.MightyBelt;
            if (name.StartsWith("blacksmithstome")) return TrinityItemType.CraftTome;
            if (name.StartsWith("boots_")) return TrinityItemType.Boots;
            if (name.StartsWith("bow_")) return TrinityItemType.TwoHandBow;
            if (name.StartsWith("bracers_")) return TrinityItemType.Bracer;
            if (name.StartsWith("ceremonialdagger_")) return TrinityItemType.CeremonialKnife;
            if (name.StartsWith("cloak_")) return TrinityItemType.Cloak;
            if (name.StartsWith("combatstaff_")) return TrinityItemType.TwoHandDaibo;
            if (name.StartsWith("crafting_")) return TrinityItemType.CraftingMaterial;
            if (name.StartsWith("craftingmaterials_")) return TrinityItemType.CraftingMaterial;
            if (name.StartsWith("craftingplan_")) return TrinityItemType.CraftingPlan;
            if (name.StartsWith("craftingreagent_legendary_")) return TrinityItemType.CraftingMaterial;
            if (name.StartsWith("crushield_")) return TrinityItemType.CrusaderShield;
            if (name.StartsWith("dagger_")) return TrinityItemType.Dagger;
            if (name.StartsWith("diamond_")) return TrinityItemType.Diamond;
            if (name.StartsWith("dye_")) return TrinityItemType.Dye;
            if (name.StartsWith("emerald_")) return TrinityItemType.Emerald;
            if (name.StartsWith("fistweapon_")) return TrinityItemType.FistWeapon;
            if (name.StartsWith("flail1h_")) return TrinityItemType.Flail;
            if (name.StartsWith("flail2h_")) return TrinityItemType.TwoHandFlail;
            if (name.StartsWith("followeritem_enchantress_") || dbFollowerType == FollowerType.Enchantress) return TrinityItemType.FollowerEnchantress;
            if (name.StartsWith("followeritem_scoundrel_") || dbFollowerType == FollowerType.Scoundrel) return TrinityItemType.FollowerScoundrel;
            if (name.StartsWith("followeritem_templar_") || dbFollowerType == FollowerType.Templar) return TrinityItemType.FollowerTemplar;
            if (name.StartsWith("gloves_")) return TrinityItemType.Gloves;
            if (name.StartsWith("handxbow_")) return TrinityItemType.HandCrossbow;
            if (name.StartsWith("healthglobe")) return TrinityItemType.HealthGlobe;
            if (name.StartsWith("healthpotion")) return TrinityItemType.HealthPotion;
            if (name.StartsWith("horadriccache")) return TrinityItemType.HoradricCache;
            if (name.StartsWith("lore_book_")) return TrinityItemType.CraftTome;
            if (name.StartsWith("lootrunkey")) return TrinityItemType.LootRunKey;
            if (name.StartsWith("mace_")) return TrinityItemType.Mace;
            if (name.StartsWith("mightyweapon_1h_")) return TrinityItemType.MightyWeapon;
            if (name.StartsWith("mightyweapon_2h_")) return TrinityItemType.TwoHandMighty;
            if (name.StartsWith("mojo_")) return TrinityItemType.Mojo;
            if (name.StartsWith("orb_")) return TrinityItemType.Orb;
            if (name.StartsWith("page_of_")) return TrinityItemType.CraftTome;
            if (name.StartsWith("pants_")) return TrinityItemType.Legs;
            if (name.StartsWith("polearm_") || dbItemType == ItemType.Polearm) return TrinityItemType.TwoHandPolearm;
            if (name.StartsWith("quiver_")) return TrinityItemType.Quiver;
            if (name.StartsWith("ring_")) return TrinityItemType.Ring;
            if (name.StartsWith("ruby_")) return TrinityItemType.Ruby;
            if (name.StartsWith("shield_")) return TrinityItemType.Shield;
            if (name.StartsWith("shoulderpads_")) return TrinityItemType.Shoulder;
            if (name.StartsWith("spear_")) return TrinityItemType.Spear;
            if (name.StartsWith("spiritstone_")) return TrinityItemType.SpiritStone;
            if (name.StartsWith("staff_")) return TrinityItemType.TwoHandStaff;
            if (name.StartsWith("staffofcow")) return TrinityItemType.StaffOfHerding;
            if (name.StartsWith("sword_")) return TrinityItemType.Sword;
            if (name.StartsWith("topaz_")) return TrinityItemType.Topaz;
            if (name.StartsWith("twohandedaxe_")) return TrinityItemType.TwoHandAxe;
            if (name.StartsWith("twohandedmace_")) return TrinityItemType.TwoHandMace;
            if (name.StartsWith("twohandedsword_")) return TrinityItemType.TwoHandSword;
            if (name.StartsWith("voodoomask_")) return TrinityItemType.VoodooMask;
            if (name.StartsWith("wand_")) return TrinityItemType.Wand;
            if (name.StartsWith("wizardhat_")) return TrinityItemType.WizardHat;
            if (name.StartsWith("xbow_")) return TrinityItemType.TwoHandCrossbow;
            if (name.StartsWith("console_powerglobe")) return TrinityItemType.PowerGlobe;
            if (name.StartsWith("tiered_rifts_orb")) return TrinityItemType.ProgressionGlobe;
            if (name.StartsWith("consumable_add_sockets")) return TrinityItemType.ConsumableAddSockets; // Ramaladni's Gift
            if (name.StartsWith("tieredlootrunkey_")) return TrinityItemType.TieredLootrunKey;
            if (name.StartsWith("demonkey_") || name.StartsWith("demontrebuchetkey") || name.StartsWith("quest_")) return TrinityItemType.InfernalKey;
            if (name.StartsWith("offhand_")) return TrinityItemType.Mojo;
            if (name.StartsWith("horadricrelic")) return TrinityItemType.HoradricRelic;


            // Follower item types
            if (name.StartsWith("jewelbox_") || dbItemType == ItemType.FollowerSpecial)
            {
                if (dbFollowerType == FollowerType.Scoundrel)
                    return TrinityItemType.FollowerScoundrel;
                if (dbFollowerType == FollowerType.Templar)
                    return TrinityItemType.FollowerTemplar;
                if (dbFollowerType == FollowerType.Enchantress)
                    return TrinityItemType.FollowerEnchantress;
            }

            // Fall back on some partial DB item type checking 
            if (name.StartsWith("crafting_"))
            {
                if (dbItemType == ItemType.CraftingPage)
                    return TrinityItemType.CraftTome;
                return TrinityItemType.CraftingMaterial;
            }
            if (name.StartsWith("chestarmor_"))
            {
                if (dbItemType == ItemType.Cloak)
                    return TrinityItemType.Cloak;
                return TrinityItemType.Chest;
            }
            if (name.StartsWith("helm_"))
            {
                if (dbItemType == ItemType.SpiritStone)
                    return TrinityItemType.SpiritStone;
                if (dbItemType == ItemType.VoodooMask)
                    return TrinityItemType.VoodooMask;
                if (dbItemType == ItemType.WizardHat)
                    return TrinityItemType.WizardHat;
                return TrinityItemType.Helm;
            }
            if (name.StartsWith("helmcloth_"))
            {
                if (dbItemType == ItemType.SpiritStone)
                    return TrinityItemType.SpiritStone;
                if (dbItemType == ItemType.VoodooMask)
                    return TrinityItemType.VoodooMask;
                if (dbItemType == ItemType.WizardHat)
                    return TrinityItemType.WizardHat;
                return TrinityItemType.Helm;
            }
            if (name.StartsWith("belt_"))
            {
                if (dbItemType == ItemType.MightyBelt)
                    return TrinityItemType.MightyBelt;
                return TrinityItemType.Belt;
            }
            return TrinityItemType.Unknown;
        }

        /// <summary>
        ///     DetermineBaseType - Calculates a more generic, "basic" type of item
        /// </summary>
        /// <param name="itemType"></param>
        /// <returns></returns>
        internal static TrinityItemBaseType DetermineBaseType(TrinityItemType itemType)
        {
            var itemBaseType = TrinityItemBaseType.Misc;

            // One Handed Weapons
            switch (itemType)
            {
                case TrinityItemType.Axe:
                case TrinityItemType.CeremonialKnife:
                case TrinityItemType.Dagger:
                case TrinityItemType.Flail:
                case TrinityItemType.FistWeapon:
                case TrinityItemType.Mace:
                case TrinityItemType.MightyWeapon:
                case TrinityItemType.Spear:
                case TrinityItemType.Sword:
                case TrinityItemType.Wand:
                    {

                        itemBaseType = TrinityItemBaseType.WeaponOneHand;
                        break;
                    }
                // Two Handed Weapons
                case TrinityItemType.TwoHandDaibo:
                case TrinityItemType.TwoHandMace:
                case TrinityItemType.TwoHandFlail:
                case TrinityItemType.TwoHandMighty:
                case TrinityItemType.TwoHandPolearm:
                case TrinityItemType.TwoHandStaff:
                case TrinityItemType.TwoHandSword:
                case TrinityItemType.TwoHandAxe:
                    {
                        itemBaseType = TrinityItemBaseType.WeaponTwoHand;
                        break;
                    }
                // Ranged Weapons
                case TrinityItemType.TwoHandCrossbow:
                case TrinityItemType.HandCrossbow:
                case TrinityItemType.TwoHandBow:
                    {
                        itemBaseType = TrinityItemBaseType.WeaponRange;
                        break;
                    }
                // Off-hands
                case TrinityItemType.Mojo:
                case TrinityItemType.Orb:
                case TrinityItemType.CrusaderShield:
                case TrinityItemType.Quiver:
                case TrinityItemType.Shield:
                    {
                        itemBaseType = TrinityItemBaseType.Offhand;
                        break;
                    }
                // Armors
                case TrinityItemType.Boots:
                case TrinityItemType.Bracer:
                case TrinityItemType.Chest:
                case TrinityItemType.Cloak:
                case TrinityItemType.Gloves:
                case TrinityItemType.Helm:
                case TrinityItemType.Legs:
                case TrinityItemType.Shoulder:
                case TrinityItemType.SpiritStone:
                case TrinityItemType.VoodooMask:
                case TrinityItemType.WizardHat:
                case TrinityItemType.Belt:
                case TrinityItemType.MightyBelt:
                    {
                        itemBaseType = TrinityItemBaseType.Armor;
                        break;
                    }
                // Jewlery
                case TrinityItemType.Amulet:
                case TrinityItemType.Ring:
                    {
                        itemBaseType = TrinityItemBaseType.Jewelry;
                        break;
                    }
                // Follower Items
                case TrinityItemType.FollowerEnchantress:
                case TrinityItemType.FollowerScoundrel:
                case TrinityItemType.FollowerTemplar:
                    {
                        itemBaseType = TrinityItemBaseType.FollowerItem;
                        break;
                    }
                // Misc Items
                case TrinityItemType.CraftingMaterial:
                case TrinityItemType.CraftTome:
                case TrinityItemType.LootRunKey:
                case TrinityItemType.HoradricRelic:
                case TrinityItemType.SpecialItem:
                case TrinityItemType.CraftingPlan:
                case TrinityItemType.HealthPotion:
                case TrinityItemType.HoradricCache:
                case TrinityItemType.Dye:
                case TrinityItemType.StaffOfHerding:
                case TrinityItemType.InfernalKey:
                case TrinityItemType.ConsumableAddSockets:
                case TrinityItemType.TieredLootrunKey:
                    {
                        itemBaseType = TrinityItemBaseType.Misc;
                        break;
                    }
                // Gems
                case TrinityItemType.Ruby:
                case TrinityItemType.Emerald:
                case TrinityItemType.Topaz:
                case TrinityItemType.Amethyst:
                case TrinityItemType.Diamond:
                    {
                        itemBaseType = TrinityItemBaseType.Gem;
                        break;
                    }
                // Globes
                case TrinityItemType.HealthGlobe:
                    {
                        itemBaseType = TrinityItemBaseType.HealthGlobe;
                        break;
                    }
                case TrinityItemType.PowerGlobe:
                    {
                        itemBaseType = TrinityItemBaseType.PowerGlobe;
                        break;
                    }
                case TrinityItemType.ProgressionGlobe:
                    {
                        itemBaseType = TrinityItemBaseType.ProgressionGlobe;
                        break;
                    }
            }
            return itemBaseType;
        }

        internal static ItemType GItemTypeToItemType(TrinityItemType itemType)
        {
            switch (itemType)
            {
                case TrinityItemType.Axe:
                    return ItemType.Axe;

                case TrinityItemType.Dagger:
                    return ItemType.Dagger;

                case TrinityItemType.Flail:
                    return ItemType.Flail;

                case TrinityItemType.FistWeapon:
                    return ItemType.FistWeapon;

                case TrinityItemType.Mace:
                    return ItemType.Mace;

                case TrinityItemType.MightyWeapon:
                    return ItemType.MightyWeapon;

                case TrinityItemType.Spear:
                    return ItemType.Spear;

                case TrinityItemType.Sword:
                    return ItemType.Sword;

                case TrinityItemType.Wand:
                    return ItemType.Wand;

                case TrinityItemType.HandCrossbow:
                    return ItemType.HandCrossbow;

                case TrinityItemType.CeremonialKnife:
                    return ItemType.CeremonialDagger;

                case TrinityItemType.TwoHandDaibo:
                    return ItemType.Daibo;

                case TrinityItemType.TwoHandMace:
                    return ItemType.Mace;

                case TrinityItemType.TwoHandFlail:
                    return ItemType.Flail;

                case TrinityItemType.TwoHandMighty:
                    return ItemType.MightyWeapon;

                case TrinityItemType.TwoHandPolearm:
                    return ItemType.Polearm;

                case TrinityItemType.TwoHandStaff:
                    return ItemType.Staff;

                case TrinityItemType.TwoHandSword:
                    return ItemType.Sword;

                case TrinityItemType.TwoHandAxe:
                    return ItemType.Axe;

                case TrinityItemType.TwoHandCrossbow:
                    return ItemType.Crossbow;

                case TrinityItemType.TwoHandBow:
                    return ItemType.Bow;

                case TrinityItemType.FollowerEnchantress:
                case TrinityItemType.FollowerScoundrel:
                case TrinityItemType.FollowerTemplar:
                    return ItemType.FollowerSpecial;

                case TrinityItemType.CraftingMaterial:
                    return ItemType.CraftingReagent;

                case TrinityItemType.CraftTome:
                    return ItemType.CraftingPlan;

                case TrinityItemType.HealthPotion:
                case TrinityItemType.Dye:
                case TrinityItemType.ConsumableAddSockets:
                case TrinityItemType.ProgressionGlobe:
                case TrinityItemType.PowerGlobe:
                case TrinityItemType.HealthGlobe:
                    return ItemType.Consumable;

                case TrinityItemType.Ruby:
                case TrinityItemType.Emerald:
                case TrinityItemType.Topaz:
                case TrinityItemType.Amethyst:
                case TrinityItemType.Diamond:
                    return ItemType.Gem;

                case TrinityItemType.LootRunKey:
                case TrinityItemType.HoradricRelic:
                case TrinityItemType.SpecialItem:
                case TrinityItemType.CraftingPlan:
                case TrinityItemType.HoradricCache:
                case TrinityItemType.StaffOfHerding:
                case TrinityItemType.InfernalKey:
                case TrinityItemType.TieredLootrunKey:
                    return ItemType.Unknown;
            }

            ItemType newType;
            if (Enum.TryParse(itemType.ToString(), true, out newType))
                return newType;

            return ItemType.Unknown;
        }

        internal static bool IsEquipment(CachedACDItem i)
        {
            return (i.DBBaseType == ItemBaseType.Armor || i.DBBaseType == ItemBaseType.Jewelry || i.DBBaseType == ItemBaseType.Weapon);
        }

        internal static CraftingMaterialType GetCraftingMaterialType(CachedACDItem item)
        {
            return (CraftingMaterialType)item.ActorSNO;
        }

        internal static Func<ACDItem, CachedACDItem, bool> StackItemMatchFunc = (item, cItem) => item.IsValid && item.ActorSNO == cItem.ActorSNO;
        /// <summary>
        /// Gets the number of items combined in all stacks
        /// </summary>
        /// <param name="cItem">The c item.</param>
        /// <param name="inventorySlot">The inventory slot.</param>
        /// <returns>System.Int32.</returns>
        /// <exception cref="System.ArgumentException">InventorySlot  + inventorySlot +  is not supported for GetStackCount method</exception>
        internal static int GetItemStackCount(CachedACDItem cItem, InventorySlot inventorySlot)
        {
            try
            {
                switch (inventorySlot)
                {
                    case InventorySlot.BackpackItems:
                        return ZetaDia.Me.Inventory.Backpack.Where(i => StackItemMatchFunc(i, cItem)).Sum(i => i.GetItemStackQuantity());
                    case InventorySlot.SharedStash:
                        return ZetaDia.Me.Inventory.StashItems.Where(i => StackItemMatchFunc(i, cItem)).Sum(i => i.GetItemStackQuantity());
                }
                throw new ArgumentException("InventorySlot " + inventorySlot + " is not supported for GetStackCount method");
            }
            catch (Exception ex)
            {
                Logger.LogDebug("Error Getting ItemStackQuantity {0}", ex);
                return -1;
            }
        }
    }
}