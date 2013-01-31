﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GilesTrinity.ItemRules;
using GilesTrinity.Settings.Loot;
using GilesTrinity.Technicals;
using Zeta.CommonBot;
using Zeta.CommonBot.Items;
using Zeta.Internals.Actors;

namespace GilesTrinity
{
    public class TrinityItemManager : ItemManager
    {
        public TrinityItemManager()
        {

        }

        private RuleTypePriority _priority = null;
        public override RuleTypePriority Priority
        {
            get
            {
                if (_priority == null)
                {
                    _priority = new RuleTypePriority()
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

            switch (evaluationType)
            {
                case ItemEvaluationType.Keep:
                    return ShouldStashItem(item);
                case ItemEvaluationType.PickUp:
                    return ShouldPickUpItem(item);
                case ItemEvaluationType.Salvage:
                    return ShouldSalvageItem(item);
                case ItemEvaluationType.Sell:
                    return ShouldSellItem(item);
            }
            return false;
        }
        //public override bool ItemIsProtected(ACDItem item)
        //{
        //    return false;
        //}
        //public override bool ShouldPickUpItem(ACDItem item)
        //{
        //    return false;
        //}

        public override bool ShouldSalvageItem(ACDItem item)
        {
            if (ItemManager.Current.ItemIsProtected(item))
            {
                return false;
            }

            GilesCachedACDItem cItem = GilesCachedACDItem.GetCachedItem(item);

            GItemType trinityItemType = GilesTrinity.DetermineItemType(cItem.InternalName, cItem.DBItemType, cItem.FollowerType);
            GItemBaseType trinityItemBaseType = GilesTrinity.DetermineBaseType(trinityItemType);

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
        public override bool ShouldSellItem(ACDItem item)
        {
            GilesCachedACDItem cItem = GilesCachedACDItem.GetCachedItem(item);

            if (ShouldStashItem(item))
                return false;

            if (ShouldSalvageItem(item))
                return false;

            if (ItemManager.Current.ItemIsProtected(cItem.AcdItem))
            {
                return false;
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
                    if (cItem.TrinityItemType == GItemType.CraftingPlan)
                        return true;
                    else
                        return false;
                case GItemBaseType.Unknown:
                    return false;
            }

            // Switch giles base item type
            return false;
        }
        public override bool ShouldStashItem(ACDItem item)
        {
            if (ItemManager.Current.ItemIsProtected(item))
            {
                return false;
            }

            GilesCachedACDItem cItem = GilesCachedACDItem.GetCachedItem(item);

            // Now look for Misc items we might want to keep
            GItemType trinityItemType = cItem.TrinityItemType; // DetermineItemType(cItem.InternalName, cItem.DBItemType, cItem.FollowerType);
            GItemBaseType trinityBaseType = cItem.TrinityItemBaseType; // DetermineBaseType(trinityItemType);

            if (trinityItemType == GItemType.StaffOfHerding)
            {
                DbHelper.Log(TrinityLogLevel.Normal, LogCategory.ItemValuation, "{0} [{1}] [{2}] = (autokeep staff of herding)", cItem.RealName, cItem.InternalName, trinityItemType);
                return true;
            }
            if (trinityItemType == GItemType.CraftingMaterial)
            {
                DbHelper.Log(TrinityLogLevel.Normal, LogCategory.ItemValuation, "{0} [{1}] [{2}] = (autokeep craft materials)", cItem.RealName, cItem.InternalName, trinityItemType);
                return true;
            }

            if (trinityItemType == GItemType.Emerald)
            {
                DbHelper.Log(TrinityLogLevel.Normal, LogCategory.ItemValuation, "{0} [{1}] [{2}] = (autokeep gems)", cItem.RealName, cItem.InternalName, trinityItemType);
                return true;
            }
            if (trinityItemType == GItemType.Amethyst)
            {
                DbHelper.Log(TrinityLogLevel.Normal, LogCategory.ItemValuation, "{0} [{1}] [{2}] = (autokeep gems)", cItem.RealName, cItem.InternalName, trinityItemType);
                return true;
            }
            if (trinityItemType == GItemType.Topaz)
            {
                DbHelper.Log(TrinityLogLevel.Normal, LogCategory.ItemValuation, "{0} [{1}] [{2}] = (autokeep gems)", cItem.RealName, cItem.InternalName, trinityItemType);
                return true;
            }
            if (trinityItemType == GItemType.Ruby)
            {
                DbHelper.Log(TrinityLogLevel.Normal, LogCategory.ItemValuation, "{0} [{1}] [{2}] = (autokeep gems)", cItem.RealName, cItem.InternalName, trinityItemType);
                return true;
            }
            if (trinityItemType == GItemType.CraftTome)
            {
                DbHelper.Log(TrinityLogLevel.Normal, LogCategory.ItemValuation, "{0} [{1}] [{2}] = (autokeep tomes)", cItem.RealName, cItem.InternalName, trinityItemType);
                return true;
            }
            if (trinityItemType == GItemType.InfernalKey)
            {
                DbHelper.Log(TrinityLogLevel.Normal, LogCategory.UserInformation, "{0} [{1}] [{2}] = (autokeep infernal key)", cItem.RealName, cItem.InternalName, trinityItemType);
                return true;
            }
            if (trinityItemType == GItemType.HealthPotion)
            {
                DbHelper.Log(TrinityLogLevel.Normal, LogCategory.ItemValuation, "{0} [{1}] [{2}] = (ignoring potions)", cItem.RealName, cItem.InternalName, trinityItemType);
                return false;
            }

            // Stash all unidentified items - assume we want to keep them since we are using an identifier over-ride
            if (cItem.IsUnidentified)
            {
                DbHelper.Log(TrinityLogLevel.Normal, LogCategory.UserInformation, "{0} [{1}] = (autokeep unidentified items)", cItem.RealName, cItem.InternalName);
                return true;
            }

            if (GilesTrinity.Settings.Loot.ItemFilterMode == ItemFilterMode.TrinityWithItemRules)
            {
                Interpreter.InterpreterAction action = GilesTrinity.StashRule.checkItem(item);
                DbHelper.Log(TrinityLogLevel.Normal, LogCategory.UserInformation, "{0} [{1}] [{2}] = (" + action + ")", cItem.AcdItem.Name, cItem.AcdItem.InternalName, cItem.AcdItem.ItemType);
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
                DbHelper.Log(TrinityLogLevel.Normal, LogCategory.UserInformation, "{0} [{1}] [{2}] = (autokeep legendaries)", cItem.RealName, cItem.InternalName, trinityItemType);
                return true;
            }

            if (trinityItemType == GItemType.CraftingPlan)
            {
                DbHelper.Log(TrinityLogLevel.Normal, LogCategory.UserInformation, "{0} [{1}] [{2}] = (autokeep plans)", cItem.RealName, cItem.InternalName, trinityItemType);
                return true;
            }

            // Ok now try to do some decent item scoring based on item types
            double iNeedScore = GilesTrinity.ScoreNeeded(item.ItemBaseType);
            double iMyScore = GilesTrinity.ValueThisItem(cItem, trinityItemType);

            DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.ItemValuation, "{0} [{1}] [{2}] = {3}", cItem.RealName, cItem.InternalName, trinityItemType, iMyScore);
            if (iMyScore >= iNeedScore) return true;

            // If we reached this point, then we found no reason to keep the item!
            return false;

        }

        //internal static bool IsWeaponArmorJewlery(GilesCachedACDItem thisitem)
        //{
        //    return (thisitem.DBBaseType == Zeta.Internals.Actors.ItemBaseType.Armor || thisitem.DBBaseType == Zeta.Internals.Actors.ItemBaseType.Jewelry || thisitem.DBBaseType == Zeta.Internals.Actors.ItemBaseType.Weapon);
        //}

        internal static SalvageOption GetSalvageOption(ItemQuality quality)
        {
            if (quality >= ItemQuality.Magic1 && quality <= ItemQuality.Magic3)
            {
                return GilesTrinity.Settings.Loot.TownRun.SalvageBlueItemOption;
            }
            else if (quality >= ItemQuality.Rare4 && quality <= ItemQuality.Rare6)
            {
                return GilesTrinity.Settings.Loot.TownRun.SalvageYellowItemOption;
            }
            else if (quality >= ItemQuality.Legendary)
            {
                return GilesTrinity.Settings.Loot.TownRun.SalvageLegendaryItemOption;
            }
            return SalvageOption.None;
        }

        internal static void OnItemSalvaged(object sender, EventArgs e)
        {

        }
        internal static void OnItemSold(object sender, EventArgs e)
        {

        }
        internal static void OnItemStashed(object sender, EventArgs e)
        {

        }


    }
}
