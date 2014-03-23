using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Trinity.Config.Loot;
using Trinity.ItemRules;
using Trinity.Technicals;
using Zeta.Bot;
using Zeta.Bot.Items;
using Zeta.Game;
using Zeta.Game.Internals;
using Zeta.Game.Internals.Actors;

namespace Trinity
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
            return ShouldSalvageItem(item, ItemEvaluationType.Salvage);
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
            return ShouldSellItem(item, ItemEvaluationType.Sell);
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
            return ShouldStashItem(item, ItemEvaluationType.Keep);
        }

        public bool ShouldStashItem(ACDItem item, ItemEvaluationType evaluationType)
        {
            ItemEvents.ResetTownRun();

            if (Current.ItemIsProtected(item))
                return false;

            // Vanity Items
            if (DataDictionary.VanityItems.Any(i => item.InternalName.StartsWith(i)))
                return false;
            
            CachedACDItem cItem = CachedACDItem.GetCachedItem(item);

            // Now look for Misc items we might want to keep
            GItemType trinityItemType = cItem.TrinityItemType; // DetermineItemType(cItem.InternalName, cItem.DBItemType, cItem.FollowerType);
            GItemBaseType trinityBaseType = cItem.TrinityItemBaseType; // DetermineBaseType(trinityItemType);

            bool isEquipment = (cItem.DBBaseType == ItemBaseType.Armor
               || cItem.DBBaseType == ItemBaseType.Weapon
               || cItem.DBBaseType == ItemBaseType.Jewelry);

            // Stash all unidentified items - assume we want to keep them since we are using an identifier over-ride
            if (cItem.IsUnidentified)
            {
                if (evaluationType == ItemEvaluationType.Keep)
                    Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "{0} [{1}] = (autokeep unidentified items)", cItem.RealName, cItem.InternalName);
                return true;
            }
            if (trinityItemType == GItemType.StaffOfHerding)
            {
                if (evaluationType == ItemEvaluationType.Keep)
                    Logger.Log(TrinityLogLevel.Info, LogCategory.ItemValuation, "{0} [{1}] [{2}] = (autokeep staff of herding)", cItem.RealName, cItem.InternalName, trinityItemType);
                return true;
            }
            if (trinityItemType == GItemType.CraftingMaterial)
            {
                if (evaluationType == ItemEvaluationType.Keep)
                    Logger.Log(TrinityLogLevel.Info, LogCategory.ItemValuation, "{0} [{1}] [{2}] = (autokeep craft materials)", cItem.RealName, cItem.InternalName, trinityItemType);
                return true;
            }

            if (trinityItemType == GItemType.Emerald || trinityItemType == GItemType.Amethyst || trinityItemType == GItemType.Topaz || trinityItemType == GItemType.Ruby || trinityItemType == GItemType.Diamond)
            {
                if (evaluationType == ItemEvaluationType.Keep)
                    Logger.Log(TrinityLogLevel.Info, LogCategory.ItemValuation, "{0} [{1}] [{2}] = (autokeep gems)", cItem.RealName, cItem.InternalName, trinityItemType);
                return true;
            }
            if (trinityItemType == GItemType.CraftTome)
            {
                if (evaluationType == ItemEvaluationType.Keep)
                    Logger.Log(TrinityLogLevel.Info, LogCategory.ItemValuation, "{0} [{1}] [{2}] = (autokeep tomes)", cItem.RealName, cItem.InternalName, trinityItemType);
                return true;
            }
            if (trinityItemType == GItemType.InfernalKey)
            {
                if (evaluationType == ItemEvaluationType.Keep)
                    Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "{0} [{1}] [{2}] = (autokeep infernal key)", cItem.RealName, cItem.InternalName, trinityItemType);
                return true;
            }
            if (trinityItemType == GItemType.HealthPotion)
            {
                if (evaluationType == ItemEvaluationType.Keep)
                    Logger.Log(TrinityLogLevel.Info, LogCategory.ItemValuation, "{0} [{1}] [{2}] = (ignoring potions)", cItem.RealName, cItem.InternalName, trinityItemType);
                return false;
            }

<<<<<<< HEAD
            if (cItem.Quality >= ItemQuality.Legendary)
            {
                if (evaluationType == ItemEvaluationType.Keep)
                    Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "{0} [{1}] [{2}] = (autokeep legendaries)", cItem.RealName, cItem.InternalName, trinityItemType);
                return true;
            }

            if (trinityItemType == GItemType.CraftingPlan)
            {
                if (evaluationType == ItemEvaluationType.Keep)
                    Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "{0} [{1}] [{2}] = (autokeep plans)", cItem.RealName, cItem.InternalName, trinityItemType);
                return true;
            }

            // Stash all unidentified items - assume we want to keep them since we are using an identifier over-ride
            if (cItem.IsUnidentified)
=======
            if (trinityItemType == GItemType.CraftingPlan && cItem.Quality >= ItemQuality.Legendary)
>>>>>>> UnifiedTrinity/master
            {
                if (evaluationType == ItemEvaluationType.Keep)
                    Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "{0} [{1}] [{2}] = (autokeep legendary plans)", cItem.RealName, cItem.InternalName, trinityItemType);
                return true;
            }

            if (Trinity.Settings.Loot.ItemFilterMode == ItemFilterMode.TrinityWithItemRules)
            {
                Interpreter.InterpreterAction action = Trinity.StashRule.checkItem(item, evaluationType);

                if (evaluationType == ItemEvaluationType.Keep)

                    Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "{0} [{1}] [{2}] = (" + action + ")", cItem.AcdItem.Name, cItem.AcdItem.InternalName, cItem.AcdItem.ItemType);
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

            if (trinityItemType == GItemType.CraftingPlan)
            {
                if (evaluationType == ItemEvaluationType.Keep)
                    Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "{0} [{1}] [{2}] = (autokeep plans)", cItem.RealName, cItem.InternalName, trinityItemType);
                return true;
            }

            // Stashing Whites, auto-keep
            if (Trinity.Settings.Loot.TownRun.StashWhites && isEquipment && cItem.Quality <= ItemQuality.Superior)
            {
                Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "{0} [{1}] [{2}] = (stashing whites)", cItem.RealName, cItem.InternalName, trinityItemType);
                return true;
            }
            // Else auto-trash
            if (cItem.Quality <= ItemQuality.Superior && (isEquipment || cItem.TrinityItemBaseType == GItemBaseType.FollowerItem))
            {
                Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "{0} [{1}] [{2}] = (trash whites)", cItem.RealName, cItem.InternalName, trinityItemType);
                return false;
            }

            // Stashing blues, auto-keep
            if (Trinity.Settings.Loot.TownRun.StashBlues && isEquipment && cItem.Quality >= ItemQuality.Magic1 && cItem.Quality <= ItemQuality.Magic3)
            {
                Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "{0} [{1}] [{2}] = (stashing blues)", cItem.RealName, cItem.InternalName, trinityItemType);
                return true;
            }
            // Else auto trash
            if (cItem.Quality >= ItemQuality.Magic1 && cItem.Quality <= ItemQuality.Magic3 && (isEquipment || cItem.TrinityItemBaseType == GItemBaseType.FollowerItem))
            {
                Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "{0} [{1}] [{2}] = (trashing blues)", cItem.RealName, cItem.InternalName, trinityItemType);
                return false;
            }

            // Force salvage Rares
            if (Trinity.Settings.Loot.TownRun.ForceSalvageRares && cItem.Quality >= ItemQuality.Rare4 && cItem.Quality <= ItemQuality.Rare6 && (isEquipment || cItem.TrinityItemBaseType == GItemBaseType.FollowerItem))
            {
                Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "{0} [{1}] [{2}] = (force salvage rare)", cItem.RealName, cItem.InternalName, trinityItemType);
                return false;
            }
<<<<<<< HEAD

            if (Trinity.Settings.Loot.ItemFilterMode == ItemFilterMode.TrinityWithItemRules)
            {
                Interpreter.InterpreterAction action = Trinity.StashRule.checkItem(item, evaluationType);

                if (evaluationType == ItemEvaluationType.Keep)

                    Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "{0} [{1}] [{2}] = (" + action + ")", cItem.AcdItem.Name, cItem.AcdItem.InternalName, cItem.AcdItem.ItemType);
                switch (action)
                {
                    case Interpreter.InterpreterAction.KEEP:
                        return true;
                    case Interpreter.InterpreterAction.TRASH:
                        return false;
                    case Interpreter.InterpreterAction.SCORE:
                        break;
                }
=======
           
            if (cItem.Quality >= ItemQuality.Legendary)
            {
                if (evaluationType == ItemEvaluationType.Keep)
                    Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "{0} [{1}] [{2}] = (autokeep legendaries)", cItem.RealName, cItem.InternalName, trinityItemType);
                return true;
>>>>>>> UnifiedTrinity/master
            }

            // Ok now try to do some decent item scoring based on item types
            double iNeedScore = Trinity.ScoreNeeded(item.ItemBaseType);
            double iMyScore = ItemValuation.ValueThisItem(cItem, trinityItemType);

            if (evaluationType == ItemEvaluationType.Keep)
                Logger.Log(TrinityLogLevel.Verbose, LogCategory.ItemValuation, "{0} [{1}] [{2}] = {3}", cItem.RealName, cItem.InternalName, trinityItemType, iMyScore);
            if (iMyScore >= iNeedScore)
                return true;

            // If we reached this point, then we found no reason to keep the item!
            return false;
        }

        private bool ItemRulesSalvageSell(ACDItem item, ItemEvaluationType evaluationType)
        {
            ItemEvents.ResetTownRun();
            if (!item.IsPotion)
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

        private bool TrinitySalvage(ACDItem item)
        {
            CachedACDItem cItem = CachedACDItem.GetCachedItem(item);

            if (cItem.AcdItem.IsVendorBought)
                return false;

            // Vanity Items
            if (DataDictionary.VanityItems.Any(i => item.InternalName.StartsWith(i)))
                return false;

            GItemType trinityItemType = Trinity.DetermineItemType(cItem.InternalName, cItem.DBItemType, cItem.FollowerType);
            GItemBaseType trinityItemBaseType = Trinity.DetermineBaseType(trinityItemType);

            // Take Salvage Option corresponding to ItemLevel
            SalvageOption salvageOption = GetSalvageOption(cItem.Quality);

            // Stashing Whites
            if (Trinity.Settings.Loot.TownRun.StashWhites && cItem.Quality < ItemQuality.Magic1)
                return false;

            // Stashing Blues
            if (Trinity.Settings.Loot.TownRun.StashBlues && cItem.Quality > ItemQuality.Superior && cItem.Quality < ItemQuality.Rare4)
                return false;
            
            if (cItem.Quality >= ItemQuality.Legendary && salvageOption == SalvageOption.InfernoOnly && cItem.Level >= 60)
                return true;

            switch (trinityItemBaseType)
            {
                case GItemBaseType.WeaponRange:
                case GItemBaseType.WeaponOneHand:
                case GItemBaseType.WeaponTwoHand:
                case GItemBaseType.Armor:
                case GItemBaseType.Offhand:
                    return ((cItem.Level >= 61 && salvageOption == SalvageOption.InfernoOnly) || salvageOption == SalvageOption.All);
                case GItemBaseType.Jewelry:
                    return ((cItem.Level >= 59 && salvageOption == SalvageOption.InfernoOnly) || salvageOption == SalvageOption.All);
                case GItemBaseType.FollowerItem:
                    return ((cItem.Level >= 60 && salvageOption == SalvageOption.InfernoOnly) || salvageOption == SalvageOption.All);
                case GItemBaseType.Gem:
                case GItemBaseType.Misc:
                case GItemBaseType.Unknown:
                    return false;
                default:
                    return false;
            }
        }

        private bool TrinitySell(ACDItem item)
        {
            CachedACDItem cItem = CachedACDItem.GetCachedItem(item);

            // Vanity Items
            if (DataDictionary.VanityItems.Any(i => item.InternalName.StartsWith(i)))
                return false;

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
                    if (cItem.TrinityItemType == GItemType.CraftingPlan)
                        return true;
                    return false;
                case GItemBaseType.Unknown:
                    return false;
            }

            return false;
        }

        private SalvageOption GetSalvageOption(ItemQuality quality)
        {
            if (quality < ItemQuality.Magic1)
            {
                return SalvageOption.All;
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
            return SalvageOption.None;
        }

        public enum DumpItemLocation
        {
            Equipped,
            Backpack,
            Ground,
            Stash
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
                    case DumpItemLocation.Backpack:
                        itemList = ZetaDia.Me.Inventory.Backpack.ToList();
                        break;
                    case DumpItemLocation.Ground:
                        itemList = ZetaDia.Actors.GetActorsOfType<DiaItem>(true, false).Select(i => i.CommonData).ToList();
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

                foreach (var item in itemList)
                {
                    try
                    {
                        string itemName = string.Format("\n\nName={0} InternalName={1} GameBalanceID={2}",
                            item.Name, item.InternalName, item.GameBalanceId) + " ItemLink: " + item.ItemLink.Replace("{", "{{").Replace("}", "}}");

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
                        Logger.Log("Link Color ItemQuality=" + ItemLinkColorToQuality(item.ItemLink, item.InternalName, item.Name, item.GameBalanceId));
                    }
                    catch (Exception ex)
                    {
                        Logger.Log("Exception reading Item Link\n{0}", ex.ToString());
                    }

                    try
                    {
                        PrintObjectProperties<ACDItem>(item);
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
                        Logger.Log(typeof(T).Name + "." + property.Name + "=" + val.ToString());
                }
                catch 
                {
                    Logger.Log("Exception reading {0} from object", property.Name);
                }
            }
        }

        private static Regex ItemQualityRegex = new Regex("{c:[a-zA-Z0-9]{8}}", RegexOptions.Compiled);
        public static ItemQuality ItemLinkColorToQuality(string itemLink, string internalName, string name, int gameBalanceId)
        {
            /*
            {c:ff00ff00} = Set
            {c:ffff8000} = Legendary
            {c:ffffff00} = Rare
            {c:ff6969ff} = Magic
             */

            string linkColor = ItemQualityRegex.Match(itemLink).Value;

            ItemQuality qualityResult;
            string itemLinkLog = itemLink.Replace("{", "{{").Replace("}", "}}");

            switch (linkColor)
            {
                case "{c:ff00ff00}":
                    qualityResult = ItemQuality.Legendary;
                    break;
                case "{c:ffff8000}":
                    qualityResult = ItemQuality.Legendary;
                    break;
                case "{c:ffffff00}":
                    qualityResult = ItemQuality.Rare4;
                    break;
                case "{c:ff6969ff}":
                    qualityResult = ItemQuality.Magic1;
                    break;
                default:
                    Logger.Log("Invalid Item Link color={0} link={1} internalName={2} name={3} gameBalanceId={4}", linkColor, itemLinkLog, internalName, name, gameBalanceId);
                    qualityResult = ItemQuality.Invalid;
                    break;
            }

            return qualityResult;
        }

    }
}