using System;
using Trinity.Config.Loot;
using Trinity.ItemRules;
using Trinity.Technicals;
using Zeta.Bot;
using Zeta.Bot.Items;
using Zeta.Game;
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

            CachedACDItem cItem = CachedACDItem.GetCachedItem(item);

            // Now look for Misc items we might want to keep
            GItemType trinityItemType = cItem.TrinityItemType; // DetermineItemType(cItem.InternalName, cItem.DBItemType, cItem.FollowerType);
            GItemBaseType trinityBaseType = cItem.TrinityItemBaseType; // DetermineBaseType(trinityItemType);

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

            if (trinityItemType == GItemType.Emerald)
            {
                if (evaluationType == ItemEvaluationType.Keep)
                    Logger.Log(TrinityLogLevel.Info, LogCategory.ItemValuation, "{0} [{1}] [{2}] = (autokeep gems)", cItem.RealName, cItem.InternalName, trinityItemType);
                return true;
            }
            if (trinityItemType == GItemType.Amethyst)
            {
                if (evaluationType == ItemEvaluationType.Keep)
                    Logger.Log(TrinityLogLevel.Info, LogCategory.ItemValuation, "{0} [{1}] [{2}] = (autokeep gems)", cItem.RealName, cItem.InternalName, trinityItemType);
                return true;
            }
            if (trinityItemType == GItemType.Topaz)
            {
                if (evaluationType == ItemEvaluationType.Keep)
                    Logger.Log(TrinityLogLevel.Info, LogCategory.ItemValuation, "{0} [{1}] [{2}] = (autokeep gems)", cItem.RealName, cItem.InternalName, trinityItemType);
                return true;
            }
            if (trinityItemType == GItemType.Ruby)
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

            // Stash all unidentified items - assume we want to keep them since we are using an identifier over-ride
            if (cItem.IsUnidentified)
            {
                if (evaluationType == ItemEvaluationType.Keep)
                    Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "{0} [{1}] = (autokeep unidentified items)", cItem.RealName, cItem.InternalName);
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

            // auto trash blue weapons/armor/jewlery
            if ((item.ItemBaseType == ItemBaseType.Armor
                 || item.ItemBaseType == ItemBaseType.Weapon
                 || item.ItemBaseType == ItemBaseType.Jewelry)
                && item.ItemQualityLevel < ItemQuality.Rare4)
            {
                return false;
            }

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

            GItemType trinityItemType = Trinity.DetermineItemType(cItem.InternalName, cItem.DBItemType, cItem.FollowerType);
            GItemBaseType trinityItemBaseType = Trinity.DetermineBaseType(trinityItemType);

            // Take Salvage Option corresponding to ItemLevel
            SalvageOption salvageOption = GetSalvageOption(cItem.Quality);

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
#pragma warning disable 1718
        public static void DumpBackpack()
        {
            ZetaDia.Actors.Update();
            using (ZetaDia.Memory.SaveCacheState())
            {
                ZetaDia.Memory.TemporaryCacheState(false);

                foreach (var item in Zeta.Game.ZetaDia.Me.Inventory.Backpack)
                {
                    string itemName = string.Format("Name={0} InternalName={1} DynamicID={2} ", item.Name, item.InternalName, item.DynamicId);
                    try
                    {
                        foreach (object val in Enum.GetValues(typeof(ActorAttributeType)))
                        {
                            int iVal = item.GetAttribute<int>((ActorAttributeType)val);
                            float fVal = item.GetAttribute<float>((ActorAttributeType)val);

                            if (iVal != 0 || fVal != 0)
                                Logger.Log(itemName + "Attribute: {0}, iVal: {1}, fVal: {2}", val, iVal, (fVal != fVal) ? "" : fVal.ToString());
                        }
                    }
                    catch (Exception ex)
                    {
                        Logger.Log("Exception reading attributes for {0}\n{1}", item.Name, ex.ToString());
                    }
                }

            }
        }
    }
}