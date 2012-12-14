using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using GilesTrinity.ItemRules;
using GilesTrinity.Settings.Loot;
using GilesTrinity.Technicals;
using Zeta;
using Zeta.Common;
using Zeta.Common.Plugins;
using Zeta.CommonBot;
using Zeta.Internals.Actors;

namespace GilesTrinity
{
    public partial class GilesTrinity : IPlugin
    {
        /// <summary>
        /// Pickup Validation - Determines what should or should not be picked up
        /// </summary>
        /// <param name="name"></param>
        /// <param name="level"></param>
        /// <param name="quality"></param>
        /// <param name="balanceId"></param>
        /// <param name="dbItemType"></param>
        /// <param name="followerType"></param>
        /// <param name="dynamicID"></param>
        /// <returns></returns>
        internal static bool GilesPickupItemValidation(string name, int level, ItemQuality quality, int balanceId, ItemBaseType dbItemBaseType, ItemType dbItemType, bool isOneHand, bool isTwoHand, FollowerType followerType, int dynamicID = 0)
        {

            if (Settings.Loot.ItemFilterMode == ItemFilterMode.TrinityWithItemRules)
            {
                // try updating fix#3 try for name bug -> doesn't work
                //ACDItem item = ZetaDia.Actors.GetACDItemByGuid(acd.ACDGuid);
                //Interpreter.InterpreterAction action = StashRule.checkItem(ZetaDia.Actors.GetACDItemByGuid(acd.ACDGuid), true);
                
                //Interpreter.InterpreterAction action = StashRule.checkItem(acd as ACDItem, true);

                Interpreter.InterpreterAction action = StashRule.checkPickUpItem(name, level, quality, dbItemBaseType, dbItemType, isOneHand, isTwoHand, balanceId);
                switch (action)
                {
                    case Interpreter.InterpreterAction.KEEP:
                        return true;

                    case Interpreter.InterpreterAction.IGNORE:
                        return false;
                }
            }

            // If it's legendary, we always want it *IF* it's level is right
            if (quality >= ItemQuality.Legendary)
            {
                return (Settings.Loot.Pickup.LegendaryLevel > 0 && (level >= Settings.Loot.Pickup.LegendaryLevel || Settings.Loot.Pickup.LegendaryLevel == 1));
            }

            // Calculate giles item types and base types etc.
            GItemType itemType = DetermineItemType(name, dbItemType, followerType);
            GItemBaseType baseType = DetermineBaseType(itemType);

            switch (baseType)
            {
                case GItemBaseType.WeaponTwoHand:
                case GItemBaseType.WeaponOneHand:
                case GItemBaseType.WeaponRange:
                    return CheckLevelRequirements(level, quality, Settings.Loot.Pickup.WeaponBlueLevel, Settings.Loot.Pickup.WeaponYellowLevel);
                case GItemBaseType.Armor:
                case GItemBaseType.Offhand:
                    return CheckLevelRequirements(level, quality, Settings.Loot.Pickup.ArmorBlueLevel, Settings.Loot.Pickup.ArmorYellowLevel);
                case GItemBaseType.Jewelry:
                    return CheckLevelRequirements(level, quality, Settings.Loot.Pickup.JewelryBlueLevel, Settings.Loot.Pickup.JewelryYellowLevel);
                case GItemBaseType.FollowerItem:
                    if (level < 60 || !Settings.Loot.Pickup.FollowerItem || quality < ItemQuality.Rare4)
                    {
                        if (!_hashsetItemFollowersIgnored.Contains(dynamicID))
                        {
                            _hashsetItemFollowersIgnored.Add(dynamicID);
                            totalFollowerItemsIgnored++;
                        }
                        return false;
                    }
                    break;
                case GItemBaseType.Gem:
                    if (level < Settings.Loot.Pickup.GemLevel ||
                        (itemType == GItemType.Ruby && !Settings.Loot.Pickup.GemType.HasFlag(TrinityGemType.Ruby)) ||
                        (itemType == GItemType.Emerald && !Settings.Loot.Pickup.GemType.HasFlag(TrinityGemType.Emerald)) ||
                        (itemType == GItemType.Amethyst && !Settings.Loot.Pickup.GemType.HasFlag(TrinityGemType.Amethyst)) ||
                        (itemType == GItemType.Topaz && !Settings.Loot.Pickup.GemType.HasFlag(TrinityGemType.Topaz)))
                    {
                        return false;
                    }
                    break;
                case GItemBaseType.Misc:

                    // Note; Infernal keys are misc, so should be picked up here - we aren't filtering them out, so should default to true at the end of this function
                    if (itemType == GItemType.CraftingMaterial && level < Settings.Loot.Pickup.MiscItemLevel)
                    {
                        return false;
                    }
                    if (itemType == GItemType.CraftTome && (level < Settings.Loot.Pickup.MiscItemLevel || !Settings.Loot.Pickup.CraftTomes))
                    {
                        return false;
                    }
                    if (itemType == GItemType.CraftingPlan && !Settings.Loot.Pickup.DesignPlan)
                    {
                        return false;
                    }

                    // Potion filtering
                    if (itemType == GItemType.HealthPotion)
                    {
                        if (Settings.Loot.Pickup.PotionMode == PotionMode.Ignore || level < Settings.Loot.Pickup.PotionLevel)
                        {
                            return false;
                        }
                        if (Settings.Loot.Pickup.PotionMode == PotionMode.Cap)
                        {

                            // Map out all the items already in the backpack
                            int iTotalPotions =
                                (from tempitem in ZetaDia.Me.Inventory.Backpack
                                 where tempitem.BaseAddress != IntPtr.Zero
                                 where tempitem.GameBalanceId == balanceId
                                 select tempitem.ItemStackQuantity).Sum();
                            if (iTotalPotions > 98)
                            {
                                return false;
                            }
                        }
                        // if we're picking up all
                        return true;
                    }
                    break;
                case GItemBaseType.HealthGlobe:
                    return false;
                case GItemBaseType.Unknown:
                    return false;
                default:
                    return false;
            }

            // Switch giles base item type

            // Didn't cancel it, so default to true!
            return true;
        }

        /// <summary>
        /// Checks if current item's level is according to min level for Pickup.
        /// </summary>
        /// <param name="level">The current item's level.</param>
        /// <param name="quality">The item's quality.</param>
        /// <param name="requiredBlueLevel">The blue level required.</param>
        /// <param name="requiredYellowLevel">The yellow level required.</param>
        /// <returns></returns>
        internal static bool CheckLevelRequirements(int level, ItemQuality quality, int requiredBlueLevel, int requiredYellowLevel)
        {
            if (quality < ItemQuality.Magic1)
            {
                // White item, blacklist
                return false;
            }
            if (quality >= ItemQuality.Magic1 && quality < ItemQuality.Rare4)
            {
                if (requiredBlueLevel == 0 || (requiredBlueLevel != 0 && level < requiredBlueLevel))
                {
                    // Between magic and rare, and either we want no blues, or this level is higher than the blue level we want
                    return false;
                }
            }
            else
            {
                if (requiredYellowLevel == 0 || (requiredYellowLevel != 0 && level < requiredYellowLevel))
                {
                    // Between magic and rare, and either we want no blues, or this level is higher than the blue level we want
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// DetermineItemType - Calculates what kind of item it is from D3 internalnames
        /// </summary>
        /// <param name="name"></param>
        /// <param name="dbItemType"></param>
        /// <param name="dbFollowerType"></param>
        /// <returns></returns>
        internal static GItemType DetermineItemType(string name, ItemType dbItemType, FollowerType dbFollowerType = FollowerType.None)
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
                if (dbItemType == ItemType.CraftingPage) return GItemType.CraftTome;
                return GItemType.CraftingMaterial;
            }
            if (name.StartsWith("chestarmor_"))
            {
                if (dbItemType == ItemType.Cloak) return GItemType.Cloak;
                return GItemType.Chest;
            }
            if (name.StartsWith("helm_"))
            {
                if (dbItemType == ItemType.SpiritStone) return GItemType.SpiritStone;
                if (dbItemType == ItemType.VoodooMask) return GItemType.VoodooMask;
                if (dbItemType == ItemType.WizardHat) return GItemType.WizardHat;
                return GItemType.Helm;
            }
            if (name.StartsWith("helmcloth_"))
            {
                if (dbItemType == ItemType.SpiritStone) return GItemType.SpiritStone;
                if (dbItemType == ItemType.VoodooMask) return GItemType.VoodooMask;
                if (dbItemType == ItemType.WizardHat) return GItemType.WizardHat;
                return GItemType.Helm;
            }
            if (name.StartsWith("belt_"))
            {
                if (dbItemType == ItemType.MightyBelt) return GItemType.MightyBelt;
                return GItemType.Belt;
            }
            if (name.StartsWith("demonkey_") || name.StartsWith("demontrebuchetkey"))
            {
                return GItemType.InfernalKey;
            }

            // hax for fuimusbruce's horadric hamburger
            if (name.StartsWith("offhand_"))
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
            GItemBaseType thisGilesBaseType = GItemBaseType.Unknown;
            if (itemType == GItemType.Axe || itemType == GItemType.CeremonialKnife || itemType == GItemType.Dagger ||
                itemType == GItemType.FistWeapon || itemType == GItemType.Mace || itemType == GItemType.MightyWeapon ||
                itemType == GItemType.Spear || itemType == GItemType.Sword || itemType == GItemType.Wand)
            {
                thisGilesBaseType = GItemBaseType.WeaponOneHand;
            }
            else if (itemType == GItemType.TwoHandDaibo || itemType == GItemType.TwoHandMace ||
                itemType == GItemType.TwoHandMighty || itemType == GItemType.TwoHandPolearm || itemType == GItemType.TwoHandStaff ||
                itemType == GItemType.TwoHandSword || itemType == GItemType.TwoHandAxe)
            {
                thisGilesBaseType = GItemBaseType.WeaponTwoHand;
            }
            else if (itemType == GItemType.TwoHandCrossbow || itemType == GItemType.HandCrossbow || itemType == GItemType.TwoHandBow)
            {
                thisGilesBaseType = GItemBaseType.WeaponRange;
            }
            else if (itemType == GItemType.Mojo || itemType == GItemType.Orb ||
                itemType == GItemType.Quiver || itemType == GItemType.Shield)
            {
                thisGilesBaseType = GItemBaseType.Offhand;
            }
            else if (itemType == GItemType.Boots || itemType == GItemType.Bracer || itemType == GItemType.Chest ||
                itemType == GItemType.Cloak || itemType == GItemType.Gloves || itemType == GItemType.Helm ||
                itemType == GItemType.Legs || itemType == GItemType.Shoulder || itemType == GItemType.SpiritStone ||
                itemType == GItemType.VoodooMask || itemType == GItemType.WizardHat || itemType == GItemType.Belt ||
                itemType == GItemType.MightyBelt)
            {
                thisGilesBaseType = GItemBaseType.Armor;
            }
            else if (itemType == GItemType.Amulet || itemType == GItemType.Ring)
            {
                thisGilesBaseType = GItemBaseType.Jewelry;
            }
            else if (itemType == GItemType.FollowerEnchantress || itemType == GItemType.FollowerScoundrel ||
                itemType == GItemType.FollowerTemplar)
            {
                thisGilesBaseType = GItemBaseType.FollowerItem;
            }
            else if (itemType == GItemType.CraftingMaterial || itemType == GItemType.CraftTome ||
                itemType == GItemType.SpecialItem || itemType == GItemType.CraftingPlan || itemType == GItemType.HealthPotion ||
                itemType == GItemType.Dye || itemType == GItemType.StaffOfHerding || itemType == GItemType.InfernalKey)
            {
                thisGilesBaseType = GItemBaseType.Misc;
            }
            else if (itemType == GItemType.Ruby || itemType == GItemType.Emerald || itemType == GItemType.Topaz ||
                itemType == GItemType.Amethyst)
            {
                thisGilesBaseType = GItemBaseType.Gem;
            }
            else if (itemType == GItemType.HealthGlobe)
            {
                thisGilesBaseType = GItemBaseType.HealthGlobe;
            }
            return thisGilesBaseType;
        }

        /// <summary>
        /// DetermineIsStackable - Calculates what items can be stacked up
        /// </summary>
        /// <param name="itemType"></param>
        /// <returns></returns>
        internal static bool DetermineIsStackable(GItemType itemType)
        {
            return itemType == GItemType.CraftingMaterial || itemType == GItemType.CraftTome || itemType == GItemType.Ruby ||
                   itemType == GItemType.Emerald || itemType == GItemType.Topaz || itemType == GItemType.Amethyst ||
                   itemType == GItemType.HealthPotion || itemType == GItemType.CraftingPlan || itemType == GItemType.Dye ||
                   itemType == GItemType.InfernalKey;
        }

        /// <summary>
        /// DetermineIsTwoSlot - Tries to calculate what items take up 2 slots or 1
        /// </summary>
        /// <param name="itemType"></param>
        /// <returns></returns>
        internal static bool DetermineIsTwoSlot(GItemType itemType)
        {
            return (itemType == GItemType.Axe || itemType == GItemType.CeremonialKnife || itemType == GItemType.Dagger ||
                    itemType == GItemType.FistWeapon || itemType == GItemType.Mace || itemType == GItemType.MightyWeapon ||
                    itemType == GItemType.Spear || itemType == GItemType.Sword || itemType == GItemType.Wand ||
                    itemType == GItemType.TwoHandDaibo || itemType == GItemType.TwoHandCrossbow || itemType == GItemType.TwoHandMace ||
                    itemType == GItemType.TwoHandMighty || itemType == GItemType.TwoHandPolearm || itemType == GItemType.TwoHandStaff ||
                    itemType == GItemType.TwoHandSword || itemType == GItemType.TwoHandAxe || itemType == GItemType.HandCrossbow ||
                    itemType == GItemType.TwoHandBow || itemType == GItemType.Mojo || itemType == GItemType.Orb ||
                    itemType == GItemType.Quiver || itemType == GItemType.Shield || itemType == GItemType.Boots ||
                    itemType == GItemType.Bracer || itemType == GItemType.Chest || itemType == GItemType.Cloak ||
                    itemType == GItemType.Gloves || itemType == GItemType.Helm || itemType == GItemType.Legs ||
                    itemType == GItemType.Shoulder || itemType == GItemType.SpiritStone ||
                    itemType == GItemType.VoodooMask || itemType == GItemType.WizardHat || itemType == GItemType.StaffOfHerding);
        }

        /// <summary>
        /// Search backpack to see if we have room for a 2-slot item anywhere
        /// </summary>
        internal static bool[,] BackpackSlotBlocked = new bool[10, 6];
        internal static Vector2 SortingFindLocationBackpack(bool isOriginalTwoSlot)
        {
            int iPointX = -1;
            int iPointY = -1;
            for (int iRow = 0; iRow <= 5; iRow++)
            {
                for (int iColumn = 0; iColumn <= 9; iColumn++)
                {
                    if (!BackpackSlotBlocked[iColumn, iRow])
                    {
                        bool bNotEnoughSpace = false;
                        if (iRow < 5)
                        {
                            bNotEnoughSpace = (isOriginalTwoSlot && BackpackSlotBlocked[iColumn, iRow + 1]);
                        }
                        else
                        {
                            if (isOriginalTwoSlot)
                                bNotEnoughSpace = true;
                        }
                        if (!bNotEnoughSpace)
                        {
                            iPointX = iColumn;
                            iPointY = iRow;
                            goto FoundPackLocation;
                        }
                    }
                }
            }
        FoundPackLocation:
            if ((iPointX < 0) || (iPointY < 0))
            {
                return new Vector2(-1, -1);
            }
            return new Vector2(iPointX, iPointY);
        }

        /// <summary>
        /// Output test scores for everything in the backpack
        /// </summary>
        internal static void TestScoring()
        {
            if (TownRun.testingBackpack) return;
            TownRun.testingBackpack = true;
            ZetaDia.Actors.Update();
            if (ZetaDia.Actors.Me == null)
            {
                DbHelper.Log(TrinityLogLevel.Normal, LogCategory.UserInformation, "Error testing scores - not in game world?");
                return;
            }
            if (ZetaDia.IsInGame && !ZetaDia.IsLoadingWorld)
            {
                DbHelper.Log(TrinityLogLevel.Normal, LogCategory.UserInformation, "===== Outputting Test Scores =====");
                foreach (ACDItem item in ZetaDia.Actors.Me.Inventory.Backpack)
                {
                    if (item.BaseAddress == IntPtr.Zero)
                    {
                        DbHelper.Log(TrinityLogLevel.Normal, LogCategory.UserInformation, "GSError: Diablo 3 memory read error, or item became invalid [TestScore-1]");
                    }
                    else
                    {
                        GilesCachedACDItem thiscacheditem = new GilesCachedACDItem(item, item.InternalName, item.Name, item.Level, item.ItemQualityLevel, item.Gold, item.GameBalanceId, item.DynamicId,
                            item.Stats.WeaponDamagePerSecond, item.IsOneHand, item.IsTwoHand, item.DyeType, item.ItemType, item.ItemBaseType, item.FollowerSpecialType, item.IsUnidentified, item.ItemStackQuantity,
                            item.Stats);
                        bool bShouldStashTest = ShouldWeStashThis(thiscacheditem);
                        DbHelper.Log(TrinityLogLevel.Normal, LogCategory.UserInformation, bShouldStashTest ? "* KEEP *" : "-- TRASH --");
                    }
                }
                DbHelper.Log(TrinityLogLevel.Normal, LogCategory.UserInformation, "===== Finished Test Score Outputs =====");
                DbHelper.Log(TrinityLogLevel.Normal, LogCategory.UserInformation, "Note: See bad scores? Wrong item types? Known DB bug - restart DB before using the test button!");
            }
            else
            {
                DbHelper.Log(TrinityLogLevel.Normal, LogCategory.UserInformation, "Error testing scores - not in game world?");
            }
            TownRun.testingBackpack = false;
        }

        /// <summary>
        /// Determine if we should stash this item or not based on item type and score, and/or loot rule scripting
        /// </summary>
        /// <param name="thisitem"></param>
        /// <returns></returns>
        internal static bool ShouldWeStashThis(GilesCachedACDItem thisitem)
        {
            // Now look for Misc items we might want to keep
            GItemType TrueItemType = DetermineItemType(thisitem.InternalName, thisitem.DBItemType, thisitem.FollowerType);
            GItemBaseType thisGilesBaseType = DetermineBaseType(TrueItemType);

            if (TrueItemType == GItemType.StaffOfHerding)
            {
                DbHelper.Log(TrinityLogLevel.Normal, LogCategory.ItemValuation, "{0} [{1}] [{2}] = (autokeep staff of herding)", thisitem.RealName, thisitem.InternalName, TrueItemType);
                return true;
            }
            if (TrueItemType == GItemType.CraftingMaterial)
            {
                DbHelper.Log(TrinityLogLevel.Normal, LogCategory.ItemValuation, "{0} [{1}] [{2}] = (autokeep craft materials)", thisitem.RealName, thisitem.InternalName, TrueItemType);
                return true;
            }

            if (TrueItemType == GItemType.Emerald)
            {
                DbHelper.Log(TrinityLogLevel.Normal, LogCategory.ItemValuation, "{0} [{1}] [{2}] = (autokeep gems)", thisitem.RealName, thisitem.InternalName, TrueItemType);
                return true;
            }
            if (TrueItemType == GItemType.Amethyst)
            {
                DbHelper.Log(TrinityLogLevel.Normal, LogCategory.ItemValuation, "{0} [{1}] [{2}] = (autokeep gems)", thisitem.RealName, thisitem.InternalName, TrueItemType);
                return true;
            }
            if (TrueItemType == GItemType.Topaz)
            {
                DbHelper.Log(TrinityLogLevel.Normal, LogCategory.ItemValuation, "{0} [{1}] [{2}] = (autokeep gems)", thisitem.RealName, thisitem.InternalName, TrueItemType);
                return true;
            }
            if (TrueItemType == GItemType.Ruby)
            {
                DbHelper.Log(TrinityLogLevel.Normal, LogCategory.ItemValuation, "{0} [{1}] [{2}] = (autokeep gems)", thisitem.RealName, thisitem.InternalName, TrueItemType);
                return true;
            }
            if (TrueItemType == GItemType.CraftTome)
            {
                DbHelper.Log(TrinityLogLevel.Normal, LogCategory.ItemValuation, "{0} [{1}] [{2}] = (autokeep tomes)", thisitem.RealName, thisitem.InternalName, TrueItemType);
                return true;
            }
            if (TrueItemType == GItemType.InfernalKey)
            {
                DbHelper.Log(TrinityLogLevel.Normal, LogCategory.UserInformation, "{0} [{1}] [{2}] = (autokeep infernal key)", thisitem.RealName, thisitem.InternalName, TrueItemType);
                return true;
            }
            if (TrueItemType == GItemType.HealthPotion)
            {
                DbHelper.Log(TrinityLogLevel.Normal, LogCategory.ItemValuation, "{0} [{1}] [{2}] = (ignoring potions)", thisitem.RealName, thisitem.InternalName, TrueItemType);
                return false;
            }

            // Stash all unidentified items - assume we want to keep them since we are using an identifier over-ride
            if (thisitem.IsUnidentified)
            {
                DbHelper.Log(TrinityLogLevel.Normal, LogCategory.UserInformation, "{0} [{1}] = (autokeep unidentified items)", thisitem.RealName, thisitem.InternalName);
                return true;
            }

            if (Settings.Loot.ItemFilterMode == ItemFilterMode.TrinityWithItemRules)
            {
                Interpreter.InterpreterAction action = StashRule.checkItem(thisitem.item);
                DbHelper.Log(TrinityLogLevel.Normal, LogCategory.UserInformation, "{0} [{1}] [{2}] = (" + action + ")", thisitem.item.Name, thisitem.item.InternalName, thisitem.item.ItemType);
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
            if (IsWeaponArmorJewlery(thisitem) && thisitem.Quality < ItemQuality.Rare4)
            {
                return false;
            }


            if (thisitem.Quality >= ItemQuality.Legendary)
            {
                DbHelper.Log(TrinityLogLevel.Normal, LogCategory.UserInformation, "{0} [{1}] [{2}] = (autokeep legendaries)", thisitem.RealName, thisitem.InternalName, TrueItemType);
                return true;
            }

            if (TrueItemType == GItemType.CraftingPlan)
            {
                DbHelper.Log(TrinityLogLevel.Normal, LogCategory.UserInformation, "{0} [{1}] [{2}] = (autokeep plans)", thisitem.RealName, thisitem.InternalName, TrueItemType);
                return true;
            }

            // Ok now try to do some decent item scoring based on item types
            double iNeedScore = ScoreNeeded(TrueItemType);
            double iMyScore = ValueThisItem(thisitem, TrueItemType);

            DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.ItemValuation, "{0} [{1}] [{2}] = {3}", thisitem.RealName, thisitem.InternalName, TrueItemType, iMyScore);
            if (iMyScore >= iNeedScore) return true;

            // If we reached this point, then we found no reason to keep the item!
            return false;
        }

        internal static bool IsWeaponArmorJewlery(GilesCachedACDItem thisitem)
        {
            return (thisitem.DBBaseType == Zeta.Internals.Actors.ItemBaseType.Armor || thisitem.DBBaseType == Zeta.Internals.Actors.ItemBaseType.Jewelry || thisitem.DBBaseType == Zeta.Internals.Actors.ItemBaseType.Weapon);
        }

        /// <summary>
        /// Return the score needed to keep something by the item type
        /// </summary>
        /// <param name="thisGilesItemType"></param>
        /// <returns></returns>
        internal static double ScoreNeeded(GItemType thisGilesItemType)
        {
            double iThisNeedScore = 0;

            // Weapons
            if (thisGilesItemType == GItemType.Axe || thisGilesItemType == GItemType.CeremonialKnife || thisGilesItemType == GItemType.Dagger ||
                thisGilesItemType == GItemType.FistWeapon || thisGilesItemType == GItemType.Mace || thisGilesItemType == GItemType.MightyWeapon ||
                thisGilesItemType == GItemType.Spear || thisGilesItemType == GItemType.Sword || thisGilesItemType == GItemType.Wand ||
                thisGilesItemType == GItemType.TwoHandDaibo || thisGilesItemType == GItemType.TwoHandCrossbow || thisGilesItemType == GItemType.TwoHandMace ||
                thisGilesItemType == GItemType.TwoHandMighty || thisGilesItemType == GItemType.TwoHandPolearm || thisGilesItemType == GItemType.TwoHandStaff ||
                thisGilesItemType == GItemType.TwoHandSword || thisGilesItemType == GItemType.TwoHandAxe || thisGilesItemType == GItemType.HandCrossbow || thisGilesItemType == GItemType.TwoHandBow)
                iThisNeedScore = Settings.Loot.TownRun.WeaponScore;

            // Jewelry
            if (thisGilesItemType == GItemType.Ring || thisGilesItemType == GItemType.Amulet || thisGilesItemType == GItemType.FollowerEnchantress ||
                thisGilesItemType == GItemType.FollowerScoundrel || thisGilesItemType == GItemType.FollowerTemplar)
                iThisNeedScore = Settings.Loot.TownRun.JewelryScore;

            // Armor
            if (thisGilesItemType == GItemType.Mojo || thisGilesItemType == GItemType.Orb || thisGilesItemType == GItemType.Quiver ||
                thisGilesItemType == GItemType.Shield || thisGilesItemType == GItemType.Belt || thisGilesItemType == GItemType.Boots ||
                thisGilesItemType == GItemType.Bracer || thisGilesItemType == GItemType.Chest || thisGilesItemType == GItemType.Cloak ||
                thisGilesItemType == GItemType.Gloves || thisGilesItemType == GItemType.Helm || thisGilesItemType == GItemType.Legs ||
                thisGilesItemType == GItemType.MightyBelt || thisGilesItemType == GItemType.Shoulder || thisGilesItemType == GItemType.SpiritStone ||
                thisGilesItemType == GItemType.VoodooMask || thisGilesItemType == GItemType.WizardHat)
                iThisNeedScore = Settings.Loot.TownRun.ArmorScore;
            return Math.Round(iThisNeedScore);
        }

        /// <summary>
        /// Checks if score of item is suffisant for throw notification.
        /// </summary>
        /// <param name="thisgilesbaseitemtype">The thisgilesbaseitemtype.</param>
        /// <param name="ithisitemvalue">The ithisitemvalue.</param>
        /// <returns></returns>
        public static bool CheckScoreForNotification(GItemBaseType thisgilesbaseitemtype, double ithisitemvalue)
        {
            switch (thisgilesbaseitemtype)
            {
                case GItemBaseType.WeaponOneHand:
                case GItemBaseType.WeaponRange:
                case GItemBaseType.WeaponTwoHand:
                    return (ithisitemvalue >= Settings.Notification.WeaponScore);
                case GItemBaseType.Armor:
                case GItemBaseType.Offhand:
                    return (ithisitemvalue >= Settings.Notification.ArmorScore);
                case GItemBaseType.Jewelry:
                    return (ithisitemvalue >= Settings.Notification.JewelryScore);
            }
            return false;
        }

        /// <summary>
        /// Full Output Of Item Stats
        /// </summary>
        internal static void OutputReport()
        {
            TimeSpan TotalRunningTime = DateTime.Now.Subtract(ItemStatsWhenStartedBot);

            // Create whole new file
            FileStream LogStream = File.Open(Path.Combine(FileManager.LoggingPath, String.Format("Stats - {0}.log", playerStatus.ActorClass)), FileMode.Create, FileAccess.Write, FileShare.Read);
            using (StreamWriter LogWriter = new StreamWriter(LogStream))
            {
                LogWriter.WriteLine("===== Misc Statistics =====");
                LogWriter.WriteLine("Total tracking time: " + TotalRunningTime.Hours.ToString() + "h " + TotalRunningTime.Minutes.ToString() +
                    "m " + TotalRunningTime.Seconds.ToString() + "s");
                LogWriter.WriteLine("Total deaths: " + iTotalDeaths.ToString() + " [" + Math.Round(iTotalDeaths / TotalRunningTime.TotalHours, 2).ToString() + " per hour]");
                LogWriter.WriteLine("Total games (approx): " + TotalLeaveGames.ToString() + " [" + Math.Round(TotalLeaveGames / TotalRunningTime.TotalHours, 2).ToString() + " per hour]");
                if (TotalLeaveGames == 0 && iTotalJoinGames > 0)
                {
                    if (iTotalJoinGames == 1 && TotalProfileRecycles > 1)
                    {
                        LogWriter.WriteLine("(a profile manager/death handler is interfering with join/leave game events, attempting to guess total runs based on profile-loops)");
                        LogWriter.WriteLine("Total full profile cycles: " + TotalProfileRecycles.ToString() + " [" + Math.Round(TotalProfileRecycles / TotalRunningTime.TotalHours, 2).ToString() + " per hour]");
                    }
                    else
                    {
                        LogWriter.WriteLine("(your games left value may be bugged @ 0 due to profile managers/routines etc., now showing games joined instead:)");
                        LogWriter.WriteLine("Total games joined: " + iTotalJoinGames.ToString() + " [" + Math.Round(iTotalJoinGames / TotalRunningTime.TotalHours, 2).ToString() + " per hour]");
                    }
                }
                LogWriter.WriteLine("");
                LogWriter.WriteLine("===== Item DROP Statistics =====");

                // Item stats
                if (ItemsDroppedStats.Total > 0)
                {
                    LogWriter.WriteLine("Items:");
                    LogWriter.WriteLine("Total items dropped: " + ItemsDroppedStats.Total.ToString() + " [" +
                        Math.Round(ItemsDroppedStats.Total / TotalRunningTime.TotalHours, 2).ToString() + " per hour]");
                    LogWriter.WriteLine("Items dropped by ilvl: ");
                    for (int itemLevel = 1; itemLevel <= 63; itemLevel++)
                        if (ItemsDroppedStats.TotalPerLevel[itemLevel] > 0)
                            LogWriter.WriteLine("- ilvl" + itemLevel.ToString() + ": " + ItemsDroppedStats.TotalPerLevel[itemLevel].ToString() + " [" +
                                Math.Round(ItemsDroppedStats.TotalPerLevel[itemLevel] / TotalRunningTime.TotalHours, 2).ToString() + " per hour] {" +
                                Math.Round((ItemsDroppedStats.TotalPerLevel[itemLevel] / ItemsDroppedStats.Total) * 100, 2).ToString() + " %}");
                    LogWriter.WriteLine("");
                    LogWriter.WriteLine("Items dropped by quality: ");
                    for (int iThisQuality = 0; iThisQuality <= 3; iThisQuality++)
                    {
                        if (ItemsDroppedStats.TotalPerQuality[iThisQuality] > 0)
                        {
                            LogWriter.WriteLine("- " + sQualityString[iThisQuality] + ": " + ItemsDroppedStats.TotalPerQuality[iThisQuality].ToString() + " [" + Math.Round(ItemsDroppedStats.TotalPerQuality[iThisQuality] / TotalRunningTime.TotalHours, 2).ToString() + " per hour] {" + Math.Round((ItemsDroppedStats.TotalPerQuality[iThisQuality] / ItemsDroppedStats.Total) * 100, 2).ToString() + " %}");
                            for (int itemLevel = 1; itemLevel <= 63; itemLevel++)
                                if (ItemsDroppedStats.TotalPerQPerL[iThisQuality, itemLevel] > 0)
                                    LogWriter.WriteLine("--- ilvl " + itemLevel.ToString() + " " + sQualityString[iThisQuality] + ": " + ItemsDroppedStats.TotalPerQPerL[iThisQuality, itemLevel].ToString() + " [" + Math.Round(ItemsDroppedStats.TotalPerQPerL[iThisQuality, itemLevel] / TotalRunningTime.TotalHours, 2).ToString() + " per hour] {" + Math.Round((ItemsDroppedStats.TotalPerQPerL[iThisQuality, itemLevel] / ItemsDroppedStats.Total) * 100, 2).ToString() + " %}");
                        }

                        // Any at all this quality?
                    }

                    // For loop on quality
                    LogWriter.WriteLine("");
                }

                // End of item stats

                // Potion stats
                if (ItemsDroppedStats.TotalPotions > 0)
                {
                    LogWriter.WriteLine("Potion Drops:");
                    LogWriter.WriteLine("Total potions: " + ItemsDroppedStats.TotalPotions.ToString() + " [" + Math.Round(ItemsDroppedStats.TotalPotions / TotalRunningTime.TotalHours, 2).ToString() + " per hour]");
                    for (int itemLevel = 1; itemLevel <= 63; itemLevel++) if (ItemsDroppedStats.PotionsPerLevel[itemLevel] > 0)
                            LogWriter.WriteLine("- ilvl " + itemLevel.ToString() + ": " + ItemsDroppedStats.PotionsPerLevel[itemLevel].ToString() + " [" + Math.Round(ItemsDroppedStats.PotionsPerLevel[itemLevel] / TotalRunningTime.TotalHours, 2).ToString() + " per hour] {" + Math.Round((ItemsDroppedStats.PotionsPerLevel[itemLevel] / ItemsDroppedStats.TotalPotions) * 100, 2).ToString() + " %}");
                    LogWriter.WriteLine("");
                }

                // End of potion stats

                // Gem stats
                if (ItemsDroppedStats.TotalGems > 0)
                {
                    LogWriter.WriteLine("Gem Drops:");
                    LogWriter.WriteLine("Total gems: " + ItemsDroppedStats.TotalGems.ToString() + " [" + Math.Round(ItemsDroppedStats.TotalGems / TotalRunningTime.TotalHours, 2).ToString() + " per hour]");
                    for (int iThisGemType = 0; iThisGemType <= 3; iThisGemType++)
                    {
                        if (ItemsDroppedStats.GemsPerType[iThisGemType] > 0)
                        {
                            LogWriter.WriteLine("- " + sGemString[iThisGemType] + ": " + ItemsDroppedStats.GemsPerType[iThisGemType].ToString() + " [" + Math.Round(ItemsDroppedStats.GemsPerType[iThisGemType] / TotalRunningTime.TotalHours, 2).ToString() + " per hour] {" + Math.Round((ItemsDroppedStats.GemsPerType[iThisGemType] / ItemsDroppedStats.TotalGems) * 100, 2).ToString() + " %}");
                            for (int itemLevel = 1; itemLevel <= 63; itemLevel++)
                                if (ItemsDroppedStats.GemsPerTPerL[iThisGemType, itemLevel] > 0)
                                    LogWriter.WriteLine("--- ilvl " + itemLevel.ToString() + " " + sGemString[iThisGemType] + ": " + ItemsDroppedStats.GemsPerTPerL[iThisGemType, itemLevel].ToString() + " [" + Math.Round(ItemsDroppedStats.GemsPerTPerL[iThisGemType, itemLevel] / TotalRunningTime.TotalHours, 2).ToString() + " per hour] {" + Math.Round((ItemsDroppedStats.GemsPerTPerL[iThisGemType, itemLevel] / ItemsDroppedStats.TotalGems) * 100, 2).ToString() + " %}");
                        }

                        // Any at all this quality?
                    }

                    // For loop on quality
                }

                // End of gem stats

                // Key stats
                if (ItemsDroppedStats.TotalInfernalKeys > 0)
                {
                    LogWriter.WriteLine("Infernal Key Drops:");
                    LogWriter.WriteLine("Total Keys: " + ItemsDroppedStats.TotalInfernalKeys.ToString() + " [" + Math.Round(ItemsDroppedStats.TotalInfernalKeys / TotalRunningTime.TotalHours, 2).ToString() + " per hour]");
                }

                // End of key stats
                LogWriter.WriteLine("");
                LogWriter.WriteLine("");
                LogWriter.WriteLine("===== Item PICKUP Statistics =====");

                // Item stats
                if (ItemsPickedStats.Total > 0)
                {
                    LogWriter.WriteLine("Items:");
                    LogWriter.WriteLine("Total items picked up: " + ItemsPickedStats.Total.ToString() + " [" + Math.Round(ItemsPickedStats.Total / TotalRunningTime.TotalHours, 2).ToString() + " per hour]");
                    LogWriter.WriteLine("Item picked up by ilvl: ");
                    for (int itemLevel = 1; itemLevel <= 63; itemLevel++)
                        if (ItemsPickedStats.TotalPerLevel[itemLevel] > 0)
                            LogWriter.WriteLine("- ilvl" + itemLevel.ToString() + ": " + ItemsPickedStats.TotalPerLevel[itemLevel].ToString() + " [" + Math.Round(ItemsPickedStats.TotalPerLevel[itemLevel] / TotalRunningTime.TotalHours, 2).ToString() + " per hour] {" + Math.Round((ItemsPickedStats.TotalPerLevel[itemLevel] / ItemsPickedStats.Total) * 100, 2).ToString() + " %}");
                    LogWriter.WriteLine("");
                    LogWriter.WriteLine("Items picked up by quality: ");
                    for (int iThisQuality = 0; iThisQuality <= 3; iThisQuality++)
                    {
                        if (ItemsPickedStats.TotalPerQuality[iThisQuality] > 0)
                        {
                            LogWriter.WriteLine("- " + sQualityString[iThisQuality] + ": " + ItemsPickedStats.TotalPerQuality[iThisQuality].ToString() + " [" + Math.Round(ItemsPickedStats.TotalPerQuality[iThisQuality] / TotalRunningTime.TotalHours, 2).ToString() + " per hour] {" + Math.Round((ItemsPickedStats.TotalPerQuality[iThisQuality] / ItemsPickedStats.Total) * 100, 2).ToString() + " %}");
                            for (int itemLevel = 1; itemLevel <= 63; itemLevel++)
                                if (ItemsPickedStats.TotalPerQPerL[iThisQuality, itemLevel] > 0)
                                    LogWriter.WriteLine("--- ilvl " + itemLevel.ToString() + " " + sQualityString[iThisQuality] + ": " + ItemsPickedStats.TotalPerQPerL[iThisQuality, itemLevel].ToString() + " [" + Math.Round(ItemsPickedStats.TotalPerQPerL[iThisQuality, itemLevel] / TotalRunningTime.TotalHours, 2).ToString() + " per hour] {" + Math.Round((ItemsPickedStats.TotalPerQPerL[iThisQuality, itemLevel] / ItemsPickedStats.Total) * 100, 2).ToString() + " %}");
                        }

                        // Any at all this quality?
                    }

                    // For loop on quality
                    LogWriter.WriteLine("");
                    if (totalFollowerItemsIgnored > 0)
                    {
                        LogWriter.WriteLine("  (note: " + totalFollowerItemsIgnored.ToString() + " follower items ignored for being ilvl <60 or blue)");
                    }
                }

                // End of item stats

                // Potion stats
                if (ItemsPickedStats.TotalPotions > 0)
                {
                    LogWriter.WriteLine("Potion Pickups:");
                    LogWriter.WriteLine("Total potions: " + ItemsPickedStats.TotalPotions.ToString() + " [" + Math.Round(ItemsPickedStats.TotalPotions / TotalRunningTime.TotalHours, 2).ToString() + " per hour]");
                    for (int itemLevel = 1; itemLevel <= 63; itemLevel++) if (ItemsPickedStats.PotionsPerLevel[itemLevel] > 0)
                            LogWriter.WriteLine("- ilvl " + itemLevel.ToString() + ": " + ItemsPickedStats.PotionsPerLevel[itemLevel].ToString() + " [" + Math.Round(ItemsPickedStats.PotionsPerLevel[itemLevel] / TotalRunningTime.TotalHours, 2).ToString() + " per hour] {" + Math.Round((ItemsPickedStats.PotionsPerLevel[itemLevel] / ItemsPickedStats.TotalPotions) * 100, 2).ToString() + " %}");
                    LogWriter.WriteLine("");
                }

                // End of potion stats

                // Gem stats
                if (ItemsPickedStats.TotalGems > 0)
                {
                    LogWriter.WriteLine("Gem Pickups:");
                    LogWriter.WriteLine("Total gems: " + ItemsPickedStats.TotalGems.ToString() + " [" + Math.Round(ItemsPickedStats.TotalGems / TotalRunningTime.TotalHours, 2).ToString() + " per hour]");
                    for (int iThisGemType = 0; iThisGemType <= 3; iThisGemType++)
                    {
                        if (ItemsPickedStats.GemsPerType[iThisGemType] > 0)
                        {
                            LogWriter.WriteLine("- " + sGemString[iThisGemType] + ": " + ItemsPickedStats.GemsPerType[iThisGemType].ToString() + " [" + Math.Round(ItemsPickedStats.GemsPerType[iThisGemType] / TotalRunningTime.TotalHours, 2).ToString() + " per hour] {" + Math.Round((ItemsPickedStats.GemsPerType[iThisGemType] / ItemsPickedStats.TotalGems) * 100, 2).ToString() + " %}");
                            for (int itemLevel = 1; itemLevel <= 63; itemLevel++)
                                if (ItemsPickedStats.GemsPerTPerL[iThisGemType, itemLevel] > 0)
                                    LogWriter.WriteLine("--- ilvl " + itemLevel.ToString() + " " + sGemString[iThisGemType] + ": " + ItemsPickedStats.GemsPerTPerL[iThisGemType, itemLevel].ToString() + " [" + Math.Round(ItemsPickedStats.GemsPerTPerL[iThisGemType, itemLevel] / TotalRunningTime.TotalHours, 2).ToString() + " per hour] {" + Math.Round((ItemsPickedStats.GemsPerTPerL[iThisGemType, itemLevel] / ItemsPickedStats.TotalGems) * 100, 2).ToString() + " %}");
                        }

                        // Any at all this quality?
                    }

                    // For loop on quality
                }

                // End of gem stats

                // Key stats
                if (ItemsPickedStats.TotalInfernalKeys > 0)
                {
                    LogWriter.WriteLine("Infernal Key Pickups:");
                    LogWriter.WriteLine("Total Keys: " + ItemsPickedStats.TotalInfernalKeys.ToString() + " [" + Math.Round(ItemsPickedStats.TotalInfernalKeys / TotalRunningTime.TotalHours, 2).ToString() + " per hour]");
                }

                // End of key stats
                LogWriter.WriteLine("===== End Of Report =====");
            }
            LogStream.Close();
        }

        /// <summary>
        /// Search backpack to see if we have room for a 2-slot item anywhere
        /// </summary>
        /// <param name="IsOriginalTwoSlot"></param>
        /// <returns></returns>
        internal static Vector2 FindValidBackpackLocation(bool IsOriginalTwoSlot)
        {
            bool[,] BackpackSlotBlocked = new bool[10, 6];

            // Block off the entire of any "protected bag slots"
            foreach (InventorySquare square in Zeta.CommonBot.Settings.CharacterSettings.Instance.ProtectedBagSlots)
            {
                BackpackSlotBlocked[square.Column, square.Row] = true;
            }
            if (playerStatus.ActorClass == ActorClass.Monk && Settings.Combat.Monk.SweepingWindWeaponSwap)
            {
                BackpackSlotBlocked[9, 4] = true;
                BackpackSlotBlocked[9, 5] = true;
            }
            // Map out all the items already in the backpack
            foreach (ACDItem item in ZetaDia.Me.Inventory.Backpack)
            {
                if (item.BaseAddress == IntPtr.Zero)
                {
                    return new Vector2(-1, -1);
                }
                int inventoryRow = item.InventoryRow;
                int inventoryColumn = item.InventoryColumn;

                // Mark this slot as not-free
                BackpackSlotBlocked[inventoryColumn, inventoryRow] = true;

                // Try and reliably find out if this is a two slot item or not
                GItemType tempItemType = DetermineItemType(item.InternalName, item.ItemType, item.FollowerSpecialType);
                if (DetermineIsTwoSlot(tempItemType) && inventoryRow < 5)
                {
                    BackpackSlotBlocked[inventoryColumn, inventoryRow + 1] = true;
                }
            }
            int iPointX = -1;
            int iPointY = -1;
            for (int iRow = 0; iRow <= 5; iRow++)
            {
                for (int iColumn = 0; iColumn <= 9; iColumn++)
                {
                    if (!BackpackSlotBlocked[iColumn, iRow])
                    {
                        bool NotEnoughSpace = false;
                        if (iRow < 5)
                        {
                            NotEnoughSpace = (IsOriginalTwoSlot && BackpackSlotBlocked[iColumn, iRow + 1]);
                        }
                        else
                        {
                            if (IsOriginalTwoSlot)
                                NotEnoughSpace = true;
                        }
                        if (!NotEnoughSpace)
                        {
                            iPointX = iColumn;
                            iPointY = iRow;
                            goto FoundPackLocation;
                        }
                    }
                }
            }
        FoundPackLocation:
            if ((iPointX < 0) || (iPointY < 0))
            {
                return new Vector2(-1, -1);
            }
            return new Vector2(iPointX, iPointY);
        }
    }
}
