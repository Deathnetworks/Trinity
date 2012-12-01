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
        /// Randomize the timer between stashing/salvaging etc.
        /// </summary>
        private static void RandomizeTheTimer()
        {
            Random rndNum = new Random(int.Parse(Guid.NewGuid().ToString().Substring(0, 8), NumberStyles.HexNumber));
            int rnd = rndNum.Next(7);
            itemDelayLoopLimit = 4 + rnd;
        }

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
        private static bool GilesPickupItemValidation(string name, int level, ItemQuality quality, int balanceId, Zeta.Internals.Actors.ItemType dbItemType, FollowerType followerType, int dynamicID = 0)
        {

            // If it's legendary, we always want it *IF* it's level is right
            if (quality >= ItemQuality.Legendary)
            {
                return (Settings.Loot.Pickup.LegendaryLevel > 0 && (level >= Settings.Loot.Pickup.LegendaryLevel || Settings.Loot.Pickup.LegendaryLevel == 1));
            }

            // Calculate giles item types and base types etc.
            ItemType itemType = DetermineItemType(name, dbItemType, followerType);
            ItemBaseType baseType = DetermineBaseType(itemType);

            switch (baseType)
            {
                case ItemBaseType.WeaponTwoHand:
                case ItemBaseType.WeaponOneHand:
                case ItemBaseType.WeaponRange:
                    return CheckLevelRequirements(level, quality, Settings.Loot.Pickup.WeaponBlueLevel, Settings.Loot.Pickup.WeaponYellowLevel);
                case ItemBaseType.Armor:
                case ItemBaseType.Offhand:
                    return CheckLevelRequirements(level, quality, Settings.Loot.Pickup.ArmorBlueLevel, Settings.Loot.Pickup.ArmorYellowLevel);
                case ItemBaseType.Jewelry:
                    return CheckLevelRequirements(level, quality, Settings.Loot.Pickup.JewelryBlueLevel, Settings.Loot.Pickup.JewelryYellowLevel);
                case ItemBaseType.FollowerItem:
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
                case ItemBaseType.Gem:
                    if (level < Settings.Loot.Pickup.GemLevel ||
                        (itemType == ItemType.Ruby && !Settings.Loot.Pickup.GemType.HasFlag(TrinityGemType.Ruby)) ||
                        (itemType == ItemType.Emerald && !Settings.Loot.Pickup.GemType.HasFlag(TrinityGemType.Emerald)) ||
                        (itemType == ItemType.Amethyst && !Settings.Loot.Pickup.GemType.HasFlag(TrinityGemType.Amethyst)) ||
                        (itemType == ItemType.Topaz && !Settings.Loot.Pickup.GemType.HasFlag(TrinityGemType.Topaz)))
                    {
                        return false;
                    }
                    break;
                case ItemBaseType.Misc:

                    // Note; Infernal keys are misc, so should be picked up here - we aren't filtering them out, so should default to true at the end of this function
                    if (itemType == ItemType.CraftingMaterial && level < Settings.Loot.Pickup.MiscItemLevel)
                    {
                        return false;
                    }
                    if (itemType == ItemType.CraftTome && (level < Settings.Loot.Pickup.MiscItemLevel || !Settings.Loot.Pickup.CraftTomes))
                    {
                        return false;
                    }
                    if (itemType == ItemType.CraftingPlan && !Settings.Loot.Pickup.DesignPlan)
                    {
                        return false;
                    }

                    // Potion filtering
                    if (itemType == ItemType.HealthPotion)
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
                case ItemBaseType.HealthGlobe:
                    return false;
                case ItemBaseType.Unknown:
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
        private static bool CheckLevelRequirements(int level, ItemQuality quality, int requiredBlueLevel, int requiredYellowLevel)
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
        private static ItemType DetermineItemType(string name, Zeta.Internals.Actors.ItemType dbItemType, FollowerType dbFollowerType = FollowerType.None)
        {
            name = name.ToLower();
            if (name.StartsWith("axe_")) return ItemType.Axe;
            if (name.StartsWith("ceremonialdagger_")) return ItemType.CeremonialKnife;
            if (name.StartsWith("handxbow_")) return ItemType.HandCrossbow;
            if (name.StartsWith("dagger_")) return ItemType.Dagger;
            if (name.StartsWith("fistweapon_")) return ItemType.FistWeapon;
            if (name.StartsWith("mace_")) return ItemType.Mace;
            if (name.StartsWith("mightyweapon_1h_")) return ItemType.MightyWeapon;
            if (name.StartsWith("spear_")) return ItemType.Spear;
            if (name.StartsWith("sword_")) return ItemType.Sword;
            if (name.StartsWith("wand_")) return ItemType.Wand;
            if (name.StartsWith("twohandedaxe_")) return ItemType.TwoHandAxe;
            if (name.StartsWith("bow_")) return ItemType.TwoHandBow;
            if (name.StartsWith("combatstaff_")) return ItemType.TwoHandDaibo;
            if (name.StartsWith("xbow_")) return ItemType.TwoHandCrossbow;
            if (name.StartsWith("twohandedmace_")) return ItemType.TwoHandMace;
            if (name.StartsWith("mightyweapon_2h_")) return ItemType.TwoHandMighty;
            if (name.StartsWith("polearm_")) return ItemType.TwoHandPolearm;
            if (name.StartsWith("staff_")) return ItemType.TwoHandStaff;
            if (name.StartsWith("twohandedsword_")) return ItemType.TwoHandSword;
            if (name.StartsWith("staffofcow")) return ItemType.StaffOfHerding;
            if (name.StartsWith("mojo_")) return ItemType.Mojo;
            if (name.StartsWith("orb_")) return ItemType.Orb;
            if (name.StartsWith("quiver_")) return ItemType.Quiver;
            if (name.StartsWith("shield_")) return ItemType.Shield;
            if (name.StartsWith("amulet_")) return ItemType.Amulet;
            if (name.StartsWith("ring_")) return ItemType.Ring;
            if (name.StartsWith("boots_")) return ItemType.Boots;
            if (name.StartsWith("bracers_")) return ItemType.Bracer;
            if (name.StartsWith("cloak_")) return ItemType.Cloak;
            if (name.StartsWith("gloves_")) return ItemType.Gloves;
            if (name.StartsWith("pants_")) return ItemType.Legs;
            if (name.StartsWith("barbbelt_")) return ItemType.MightyBelt;
            if (name.StartsWith("shoulderpads_")) return ItemType.Shoulder;
            if (name.StartsWith("spiritstone_")) return ItemType.SpiritStone;
            if (name.StartsWith("voodoomask_")) return ItemType.VoodooMask;
            if (name.StartsWith("wizardhat_")) return ItemType.WizardHat;
            if (name.StartsWith("lore_book_")) return ItemType.CraftTome;
            if (name.StartsWith("page_of_")) return ItemType.CraftTome;
            if (name.StartsWith("blacksmithstome")) return ItemType.CraftTome;
            if (name.StartsWith("ruby_")) return ItemType.Ruby;
            if (name.StartsWith("emerald_")) return ItemType.Emerald;
            if (name.StartsWith("topaz_")) return ItemType.Topaz;
            if (name.StartsWith("amethyst")) return ItemType.Amethyst;
            if (name.StartsWith("healthpotion_")) return ItemType.HealthPotion;
            if (name.StartsWith("followeritem_enchantress_")) return ItemType.FollowerEnchantress;
            if (name.StartsWith("followeritem_scoundrel_")) return ItemType.FollowerScoundrel;
            if (name.StartsWith("followeritem_templar_")) return ItemType.FollowerTemplar;
            if (name.StartsWith("craftingplan_")) return ItemType.CraftingPlan;
            if (name.StartsWith("dye_")) return ItemType.Dye;
            if (name.StartsWith("a1_")) return ItemType.SpecialItem;
            if (name.StartsWith("healthglobe")) return ItemType.HealthGlobe;

            // Follower item types
            if (name.StartsWith("jewelbox_") || dbItemType == Zeta.Internals.Actors.ItemType.FollowerSpecial)
            {
                if (dbFollowerType == FollowerType.Scoundrel)
                    return ItemType.FollowerScoundrel;
                if (dbFollowerType == FollowerType.Templar)
                    return ItemType.FollowerTemplar;
                if (dbFollowerType == FollowerType.Enchantress)
                    return ItemType.FollowerEnchantress;
            }

            // Fall back on some partial DB item type checking 
            if (name.StartsWith("crafting_"))
            {
                if (dbItemType == Zeta.Internals.Actors.ItemType.CraftingPage) return ItemType.CraftTome;
                return ItemType.CraftingMaterial;
            }
            if (name.StartsWith("chestarmor_"))
            {
                if (dbItemType == Zeta.Internals.Actors.ItemType.Cloak) return ItemType.Cloak;
                return ItemType.Chest;
            }
            if (name.StartsWith("helm_"))
            {
                if (dbItemType == Zeta.Internals.Actors.ItemType.SpiritStone) return ItemType.SpiritStone;
                if (dbItemType == Zeta.Internals.Actors.ItemType.VoodooMask) return ItemType.VoodooMask;
                if (dbItemType == Zeta.Internals.Actors.ItemType.WizardHat) return ItemType.WizardHat;
                return ItemType.Helm;
            }
            if (name.StartsWith("helmcloth_"))
            {
                if (dbItemType == Zeta.Internals.Actors.ItemType.SpiritStone) return ItemType.SpiritStone;
                if (dbItemType == Zeta.Internals.Actors.ItemType.VoodooMask) return ItemType.VoodooMask;
                if (dbItemType == Zeta.Internals.Actors.ItemType.WizardHat) return ItemType.WizardHat;
                return ItemType.Helm;
            }
            if (name.StartsWith("belt_"))
            {
                if (dbItemType == Zeta.Internals.Actors.ItemType.MightyBelt) return ItemType.MightyBelt;
                return ItemType.Belt;
            }
            if (name.StartsWith("demonkey_") || name.StartsWith("demontrebuchetkey"))
            {
                return ItemType.InfernalKey;
            }

            // hax for fuimusbruce's horadric hamburger
            if (name.StartsWith("offHand_"))
            {
                return ItemType.Dagger;
            }

            // ORGANS QUICK HACK IN
            if (name.StartsWith("quest_"))
            {
                return ItemType.InfernalKey;
            }
            return ItemType.Unknown;
        }

        /// <summary>
        /// DetermineBaseType - Calculates a more generic, "basic" type of item
        /// </summary>
        /// <param name="itemType"></param>
        /// <returns></returns>
        private static ItemBaseType DetermineBaseType(ItemType itemType)
        {
            ItemBaseType thisGilesBaseType = ItemBaseType.Unknown;
            if (itemType == ItemType.Axe || itemType == ItemType.CeremonialKnife || itemType == ItemType.Dagger ||
                itemType == ItemType.FistWeapon || itemType == ItemType.Mace || itemType == ItemType.MightyWeapon ||
                itemType == ItemType.Spear || itemType == ItemType.Sword || itemType == ItemType.Wand)
            {
                thisGilesBaseType = ItemBaseType.WeaponOneHand;
            }
            else if (itemType == ItemType.TwoHandDaibo || itemType == ItemType.TwoHandMace ||
                itemType == ItemType.TwoHandMighty || itemType == ItemType.TwoHandPolearm || itemType == ItemType.TwoHandStaff ||
                itemType == ItemType.TwoHandSword || itemType == ItemType.TwoHandAxe)
            {
                thisGilesBaseType = ItemBaseType.WeaponTwoHand;
            }
            else if (itemType == ItemType.TwoHandCrossbow || itemType == ItemType.HandCrossbow || itemType == ItemType.TwoHandBow)
            {
                thisGilesBaseType = ItemBaseType.WeaponRange;
            }
            else if (itemType == ItemType.Mojo || itemType == ItemType.Orb ||
                itemType == ItemType.Quiver || itemType == ItemType.Shield)
            {
                thisGilesBaseType = ItemBaseType.Offhand;
            }
            else if (itemType == ItemType.Boots || itemType == ItemType.Bracer || itemType == ItemType.Chest ||
                itemType == ItemType.Cloak || itemType == ItemType.Gloves || itemType == ItemType.Helm ||
                itemType == ItemType.Legs || itemType == ItemType.Shoulder || itemType == ItemType.SpiritStone ||
                itemType == ItemType.VoodooMask || itemType == ItemType.WizardHat || itemType == ItemType.Belt ||
                itemType == ItemType.MightyBelt)
            {
                thisGilesBaseType = ItemBaseType.Armor;
            }
            else if (itemType == ItemType.Amulet || itemType == ItemType.Ring)
            {
                thisGilesBaseType = ItemBaseType.Jewelry;
            }
            else if (itemType == ItemType.FollowerEnchantress || itemType == ItemType.FollowerScoundrel ||
                itemType == ItemType.FollowerTemplar)
            {
                thisGilesBaseType = ItemBaseType.FollowerItem;
            }
            else if (itemType == ItemType.CraftingMaterial || itemType == ItemType.CraftTome ||
                itemType == ItemType.SpecialItem || itemType == ItemType.CraftingPlan || itemType == ItemType.HealthPotion ||
                itemType == ItemType.Dye || itemType == ItemType.StaffOfHerding || itemType == ItemType.InfernalKey)
            {
                thisGilesBaseType = ItemBaseType.Misc;
            }
            else if (itemType == ItemType.Ruby || itemType == ItemType.Emerald || itemType == ItemType.Topaz ||
                itemType == ItemType.Amethyst)
            {
                thisGilesBaseType = ItemBaseType.Gem;
            }
            else if (itemType == ItemType.HealthGlobe)
            {
                thisGilesBaseType = ItemBaseType.HealthGlobe;
            }
            return thisGilesBaseType;
        }

        /// <summary>
        /// DetermineIsStackable - Calculates what items can be stacked up
        /// </summary>
        /// <param name="itemType"></param>
        /// <returns></returns>
        private static bool DetermineIsStackable(ItemType itemType)
        {
            return itemType == ItemType.CraftingMaterial || itemType == ItemType.CraftTome || itemType == ItemType.Ruby ||
                   itemType == ItemType.Emerald || itemType == ItemType.Topaz || itemType == ItemType.Amethyst ||
                   itemType == ItemType.HealthPotion || itemType == ItemType.CraftingPlan || itemType == ItemType.Dye ||
                   itemType == ItemType.InfernalKey;
        }

        /// <summary>
        /// DetermineIsTwoSlot - Tries to calculate what items take up 2 slots or 1
        /// </summary>
        /// <param name="itemType"></param>
        /// <returns></returns>
        private static bool DetermineIsTwoSlot(ItemType itemType)
        {
            return (itemType == ItemType.Axe || itemType == ItemType.CeremonialKnife || itemType == ItemType.Dagger ||
                    itemType == ItemType.FistWeapon || itemType == ItemType.Mace || itemType == ItemType.MightyWeapon ||
                    itemType == ItemType.Spear || itemType == ItemType.Sword || itemType == ItemType.Wand ||
                    itemType == ItemType.TwoHandDaibo || itemType == ItemType.TwoHandCrossbow || itemType == ItemType.TwoHandMace ||
                    itemType == ItemType.TwoHandMighty || itemType == ItemType.TwoHandPolearm || itemType == ItemType.TwoHandStaff ||
                    itemType == ItemType.TwoHandSword || itemType == ItemType.TwoHandAxe || itemType == ItemType.HandCrossbow ||
                    itemType == ItemType.TwoHandBow || itemType == ItemType.Mojo || itemType == ItemType.Orb ||
                    itemType == ItemType.Quiver || itemType == ItemType.Shield || itemType == ItemType.Boots ||
                    itemType == ItemType.Bracer || itemType == ItemType.Chest || itemType == ItemType.Cloak ||
                    itemType == ItemType.Gloves || itemType == ItemType.Helm || itemType == ItemType.Legs ||
                    itemType == ItemType.Shoulder || itemType == ItemType.SpiritStone ||
                    itemType == ItemType.VoodooMask || itemType == ItemType.WizardHat || itemType == ItemType.StaffOfHerding);
        }

        /// <summary>
        /// Search backpack to see if we have room for a 2-slot item anywhere
        /// </summary>
        private static bool[,] BackpackSlotBlocked = new bool[10, 6];
        private static Vector2 SortingFindLocationBackpack(bool isOriginalTwoSlot)
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
        private static Vector2 SortingFindLocationStash(bool isOriginalTwoSlot, bool endOfStash = false)
        {
            int iPointX = -1;
            int iPointY = -1;
            for (int iRow = 0; iRow <= 29; iRow++)
            {
                for (int iColumn = 0; iColumn <= 6; iColumn++)
                {
                    if (!StashSlotBlocked[iColumn, iRow])
                    {
                        bool bNotEnoughSpace = false;
                        if (iRow != 9 && iRow != 19 && iRow != 29)
                        {
                            bNotEnoughSpace = (isOriginalTwoSlot && StashSlotBlocked[iColumn, iRow + 1]);
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
                            if (!endOfStash)
                                goto FoundStashLocation;
                        }
                    }
                }
            }
        FoundStashLocation:
            if ((iPointX < 0) || (iPointY < 0))
            {
                return new Vector2(-1, -1);
            }
            return new Vector2(iPointX, iPointY);
        }

        /// <summary>
        /// Sorts the stash
        /// </summary>
        internal static void SortStash()
        {

            // Try and update the player-data
            ZetaDia.Actors.Update();

            // Check we can get the player dynamic ID
            int iPlayerDynamicID = -1;
            try
            {
                iPlayerDynamicID = ZetaDia.Me.CommonData.DynamicId;
            }
            catch
            {
                DbHelper.Log(TrinityLogLevel.Normal, LogCategory.UserInformation, "Failure getting your player data from DemonBuddy, abandoning the sort!");
                return;
            }
            if (iPlayerDynamicID == -1)
            {
                DbHelper.Log(TrinityLogLevel.Normal, LogCategory.UserInformation, "Failure getting your player data, abandoning the sort!");
                return;
            }

            // List used for all the sorting
            List<GilesStashSort> listSortMyStash = new List<GilesStashSort>();

            // Map out the backpack free slots
            for (int iRow = 0; iRow <= 5; iRow++)
                for (int iColumn = 0; iColumn <= 9; iColumn++)
                    BackpackSlotBlocked[iColumn, iRow] = false;

            foreach (ACDItem item in ZetaDia.Me.Inventory.Backpack)
            {
                int inventoryRow = item.InventoryRow;
                int inventoryColumn = item.InventoryColumn;

                // Mark this slot as not-free
                BackpackSlotBlocked[inventoryColumn, inventoryRow] = true;

                // Try and reliably find out if this is a two slot item or not
                ItemType tempItemType = DetermineItemType(item.InternalName, item.ItemType, item.FollowerSpecialType);
                if (DetermineIsTwoSlot(tempItemType) && inventoryRow < 5)
                {
                    BackpackSlotBlocked[inventoryColumn, inventoryRow + 1] = true;
                }
            }

            // Map out the stash free slots
            for (int iRow = 0; iRow <= 29; iRow++)
                for (int iColumn = 0; iColumn <= 6; iColumn++)
                    StashSlotBlocked[iColumn, iRow] = false;

            // Block off the entire of any "protected stash pages"
            foreach (int iProtPage in Zeta.CommonBot.Settings.CharacterSettings.Instance.ProtectedStashPages)
                for (int iProtRow = 0; iProtRow <= 9; iProtRow++)
                    for (int iProtColumn = 0; iProtColumn <= 6; iProtColumn++)
                        StashSlotBlocked[iProtColumn, iProtRow + (iProtPage * 10)] = true;

            // Remove rows we don't have
            for (int iRow = (ZetaDia.Me.NumSharedStashSlots / 7); iRow <= 29; iRow++)
                for (int iColumn = 0; iColumn <= 6; iColumn++)
                    StashSlotBlocked[iColumn, iRow] = true;

            // Map out all the items already in the stash and store their scores if appropriate
            foreach (ACDItem item in ZetaDia.Me.Inventory.StashItems)
            {
                int inventoryRow = item.InventoryRow;
                int inventoryColumn = item.InventoryColumn;

                // Mark this slot as not-free
                StashSlotBlocked[inventoryColumn, inventoryRow] = true;

                // Try and reliably find out if this is a two slot item or not
                ItemType itemType = DetermineItemType(item.InternalName, item.ItemType, item.FollowerSpecialType);

                bool isTwoSlot = DetermineIsTwoSlot(itemType);
                if (isTwoSlot && inventoryRow != 19 && inventoryRow != 9 && inventoryRow != 29)
                {
                    StashSlotBlocked[inventoryColumn, inventoryRow + 1] = true;
                }
                else if (isTwoSlot && (inventoryRow == 19 || inventoryRow == 9 || inventoryRow == 29))
                {
                    DbHelper.Log(TrinityLogLevel.Normal, LogCategory.UserInformation, "WARNING: There was an error reading your stash, abandoning the process.");
                    DbHelper.Log(TrinityLogLevel.Normal, LogCategory.UserInformation, "Always make sure you empty your backpack, open the stash, then RESTART DEMONBUDDY before sorting!");
                    return;
                }
                GilesCachedACDItem thiscacheditem = new GilesCachedACDItem(item, item.InternalName, item.Name, item.Level, item.ItemQualityLevel, item.Gold, item.GameBalanceId,
                    item.DynamicId, item.Stats.WeaponDamagePerSecond, item.IsOneHand, item.IsTwoHand, item.DyeType, item.ItemType, item.ItemBaseType, item.FollowerSpecialType,
                    item.IsUnidentified, item.ItemStackQuantity, item.Stats);

                double ItemValue = ValueThisItem(thiscacheditem, itemType);
                double NeedScore = ScoreNeeded(itemType);

                // Ignore stackable items
                if (!DetermineIsStackable(itemType) && itemType != ItemType.StaffOfHerding)
                {
                    listSortMyStash.Add(new GilesStashSort(((ItemValue / NeedScore) * 1000), 1, inventoryColumn, inventoryRow, item.DynamicId, isTwoSlot));
                }
            }


            // Sort the items in the stash by their row number, lowest to highest
            listSortMyStash.Sort((p1, p2) => p1.InventoryRow.CompareTo(p2.InventoryRow));

            // Now move items into your backpack until full, then into the END of the stash
            Vector2 vFreeSlot;

            // Loop through all stash items
            foreach (GilesStashSort thisstashsort in listSortMyStash)
            {
                vFreeSlot = SortingFindLocationBackpack(thisstashsort.bIsTwoSlot);
                int iStashOrPack = 1;
                if (vFreeSlot.X == -1 || vFreeSlot.Y == -1)
                {
                    vFreeSlot = SortingFindLocationStash(thisstashsort.bIsTwoSlot, true);
                    if (vFreeSlot.X == -1 || vFreeSlot.Y == -1)
                        continue;
                    iStashOrPack = 2;
                }
                if (iStashOrPack == 1)
                {
                    ZetaDia.Me.Inventory.MoveItem(thisstashsort.iDynamicID, iPlayerDynamicID, InventorySlot.PlayerBackpack, (int)vFreeSlot.X, (int)vFreeSlot.Y);
                    StashSlotBlocked[thisstashsort.iInventoryColumn, thisstashsort.InventoryRow] = false;
                    if (thisstashsort.bIsTwoSlot)
                        StashSlotBlocked[thisstashsort.iInventoryColumn, thisstashsort.InventoryRow + 1] = false;
                    BackpackSlotBlocked[(int)vFreeSlot.X, (int)vFreeSlot.Y] = true;
                    if (thisstashsort.bIsTwoSlot)
                        BackpackSlotBlocked[(int)vFreeSlot.X, (int)vFreeSlot.Y + 1] = true;
                    thisstashsort.iInventoryColumn = (int)vFreeSlot.X;
                    thisstashsort.InventoryRow = (int)vFreeSlot.Y;
                    thisstashsort.iStashOrPack = 2;
                }
                else
                {
                    ZetaDia.Me.Inventory.MoveItem(thisstashsort.iDynamicID, iPlayerDynamicID, InventorySlot.PlayerSharedStash, (int)vFreeSlot.X, (int)vFreeSlot.Y);
                    StashSlotBlocked[thisstashsort.iInventoryColumn, thisstashsort.InventoryRow] = false;
                    if (thisstashsort.bIsTwoSlot)
                        StashSlotBlocked[thisstashsort.iInventoryColumn, thisstashsort.InventoryRow + 1] = false;
                    StashSlotBlocked[(int)vFreeSlot.X, (int)vFreeSlot.Y] = true;
                    if (thisstashsort.bIsTwoSlot)
                        StashSlotBlocked[(int)vFreeSlot.X, (int)vFreeSlot.Y + 1] = true;
                    thisstashsort.iInventoryColumn = (int)vFreeSlot.X;
                    thisstashsort.InventoryRow = (int)vFreeSlot.Y;
                    thisstashsort.iStashOrPack = 1;
                }
                Thread.Sleep(150);
            }

            // Now sort the items by their score, highest to lowest
            listSortMyStash.Sort((p1, p2) => p1.dStashScore.CompareTo(p2.dStashScore));
            listSortMyStash.Reverse();

            // Now fill the stash in ordered-order
            foreach (GilesStashSort thisstashsort in listSortMyStash)
            {
                vFreeSlot = SortingFindLocationStash(thisstashsort.bIsTwoSlot, false);
                if (vFreeSlot.X == -1 || vFreeSlot.Y == -1)
                {
                    DbHelper.Log(TrinityLogLevel.Normal, LogCategory.UserInformation, "Failure trying to put things back into stash, no stash slots free? Abandoning...");
                    return;
                }
                ZetaDia.Me.Inventory.MoveItem(thisstashsort.iDynamicID, iPlayerDynamicID, InventorySlot.PlayerSharedStash, (int)vFreeSlot.X, (int)vFreeSlot.Y);
                if (thisstashsort.iStashOrPack == 1)
                {
                    StashSlotBlocked[thisstashsort.iInventoryColumn, thisstashsort.InventoryRow] = false;
                    if (thisstashsort.bIsTwoSlot)
                        StashSlotBlocked[thisstashsort.iInventoryColumn, thisstashsort.InventoryRow + 1] = false;
                }
                else
                {
                    BackpackSlotBlocked[thisstashsort.iInventoryColumn, thisstashsort.InventoryRow] = false;
                    if (thisstashsort.bIsTwoSlot)
                        BackpackSlotBlocked[thisstashsort.iInventoryColumn, thisstashsort.InventoryRow + 1] = false;
                }
                StashSlotBlocked[(int)vFreeSlot.X, (int)vFreeSlot.Y] = true;
                if (thisstashsort.bIsTwoSlot)
                    StashSlotBlocked[(int)vFreeSlot.X, (int)vFreeSlot.Y + 1] = true;
                thisstashsort.iStashOrPack = 1;
                thisstashsort.InventoryRow = (int)vFreeSlot.Y;
                thisstashsort.iInventoryColumn = (int)vFreeSlot.X;
                Thread.Sleep(150);
            }
            DbHelper.Log(TrinityLogLevel.Normal, LogCategory.UserInformation, "Stash sorted!");
        }

        /// <summary>
        /// Output test scores for everything in the backpack
        /// </summary>
        internal static void TestScoring()
        {
            if (testingBackpack) return;
            testingBackpack = true;
            ZetaDia.Actors.Update();
            if (ZetaDia.Actors.Me == null)
            {
                DbHelper.Log(TrinityLogLevel.Normal, LogCategory.UserInformation, "Error testing scores - not in game world?");
                return;
            }
            if (ZetaDia.IsInGame && !ZetaDia.IsLoadingWorld)
            {
                bOutputItemScores = true;
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
                bOutputItemScores = false;
            }
            else
            {
                DbHelper.Log(TrinityLogLevel.Normal, LogCategory.UserInformation, "Error testing scores - not in game world?");
            }
            testingBackpack = false;
        }

        /// <summary>
        /// Determine if we should stash this item or not based on item type and score, and/or loot rule scripting
        /// </summary>
        /// <param name="thisitem"></param>
        /// <returns></returns>
        private static bool ShouldWeStashThis(GilesCachedACDItem thisitem)
        {
            // Now look for Misc items we might want to keep
            ItemType TrueItemType = DetermineItemType(thisitem.InternalName, thisitem.DBItemType, thisitem.FollowerType);
            ItemBaseType thisGilesBaseType = DetermineBaseType(TrueItemType);

            if (TrueItemType == ItemType.StaffOfHerding)
            {
                DbHelper.Log(TrinityLogLevel.Normal, LogCategory.ItemValuation, "{0} [{1}] [{2}] = (autokeep staff of herding)", thisitem.RealName, thisitem.InternalName, TrueItemType);
                return true;
            }
            if (TrueItemType == ItemType.CraftingMaterial)
            {
                DbHelper.Log(TrinityLogLevel.Normal, LogCategory.ItemValuation, "{0} [{1}] [{2}] = (autokeep craft materials)", thisitem.RealName, thisitem.InternalName, TrueItemType);
                return true;
            }
            if (TrueItemType == ItemType.CraftingPlan)
            {
                DbHelper.Log(TrinityLogLevel.Normal, LogCategory.ItemValuation, "{0} [{1}] [{2}] = (autokeep plans)", thisitem.RealName, thisitem.InternalName, TrueItemType);
                return true;
            }
            if (TrueItemType == ItemType.Emerald)
            {
                DbHelper.Log(TrinityLogLevel.Normal, LogCategory.ItemValuation, "{0} [{1}] [{2}] = (autokeep gems)", thisitem.RealName, thisitem.InternalName, TrueItemType);
                return true;
            }
            if (TrueItemType == ItemType.Amethyst)
            {
                DbHelper.Log(TrinityLogLevel.Normal, LogCategory.ItemValuation, "{0} [{1}] [{2}] = (autokeep gems)", thisitem.RealName, thisitem.InternalName, TrueItemType);
                return true;
            }
            if (TrueItemType == ItemType.Topaz)
            {
                DbHelper.Log(TrinityLogLevel.Normal, LogCategory.ItemValuation, "{0} [{1}] [{2}] = (autokeep gems)", thisitem.RealName, thisitem.InternalName, TrueItemType);
                return true;
            }
            if (TrueItemType == ItemType.Ruby)
            {
                DbHelper.Log(TrinityLogLevel.Normal, LogCategory.ItemValuation, "{0} [{1}] [{2}] = (autokeep gems)", thisitem.RealName, thisitem.InternalName, TrueItemType);
                return true;
            }
            if (TrueItemType == ItemType.CraftTome)
            {
                DbHelper.Log(TrinityLogLevel.Normal, LogCategory.ItemValuation, "{0} [{1}] [{2}] = (autokeep tomes)", thisitem.RealName, thisitem.InternalName, TrueItemType);
                return true;
            }
            if (TrueItemType == ItemType.InfernalKey)
            {
                DbHelper.Log(TrinityLogLevel.Normal, LogCategory.ItemValuation, "{0} [{1}] [{2}] = (autokeep infernal key)", thisitem.RealName, thisitem.InternalName, TrueItemType);
                return true;
            }
            if (TrueItemType == ItemType.HealthPotion)
            {
                DbHelper.Log(TrinityLogLevel.Normal, LogCategory.ItemValuation, "{0} [{1}] [{2}] = (ignoring potions)", thisitem.RealName, thisitem.InternalName, TrueItemType);
                return false;
            }

            /*
             * Run Scripted rules for Weapons/Armor/Jewelry
             */
            if (Settings.Loot.ItemFilterMode == ItemFilterMode.TrinityWithItemRules && IsWeaponArmorJewlery(thisitem))
            {

                switch (StashRule.checkItem(thisitem.item))
                {
                    case Interpreter.InterpreterAction.KEEP:
                        return true;
                    case Interpreter.InterpreterAction.TRASH:
                        return false;
                    default:
                        break;
                }
            }

            // auto trash blue weapons/armor/jewlery
            if (IsWeaponArmorJewlery(thisitem) && thisitem.Quality < ItemQuality.Rare4)
            {
                return false;
            }

            // Stash all unidentified items - assume we want to keep them since we are using an identifier over-ride
            if (thisitem.IsUnidentified)
            {
                DbHelper.Log(TrinityLogLevel.Normal, LogCategory.ItemValuation, "{0} [{1}] = (autokeep unidentified items)", thisitem.RealName, thisitem.InternalName);
                return true;
            }

            if (thisitem.Quality >= ItemQuality.Legendary)
            {
                DbHelper.Log(TrinityLogLevel.Normal, LogCategory.ItemValuation, "{0} [{1}] [{2}] = (autokeep legendaries)", thisitem.RealName, thisitem.InternalName, TrueItemType);
                return true;
            }


            // Ok now try to do some decent item scoring based on item types
            double iNeedScore = ScoreNeeded(TrueItemType);
            double iMyScore = ValueThisItem(thisitem, TrueItemType);

            DbHelper.Log(TrinityLogLevel.Normal, LogCategory.ItemValuation, "{0} [{1}] [{2}] = {3}", thisitem.RealName, thisitem.InternalName, TrueItemType, iMyScore);
            if (iMyScore >= iNeedScore) return true;

            // If we reached this point, then we found no reason to keep the item!
            return false;
        }

        private static bool IsWeaponArmorJewlery(GilesCachedACDItem thisitem)
        {
            return (thisitem.DBBaseType == Zeta.Internals.Actors.ItemBaseType.Armor || thisitem.DBBaseType == Zeta.Internals.Actors.ItemBaseType.Jewelry || thisitem.DBBaseType == Zeta.Internals.Actors.ItemBaseType.Weapon);
        }

        /// <summary>
        /// Return the score needed to keep something by the item type
        /// </summary>
        /// <param name="thisGilesItemType"></param>
        /// <returns></returns>
        private static double ScoreNeeded(ItemType thisGilesItemType)
        {
            double iThisNeedScore = 0;

            // Weapons
            if (thisGilesItemType == ItemType.Axe || thisGilesItemType == ItemType.CeremonialKnife || thisGilesItemType == ItemType.Dagger ||
                thisGilesItemType == ItemType.FistWeapon || thisGilesItemType == ItemType.Mace || thisGilesItemType == ItemType.MightyWeapon ||
                thisGilesItemType == ItemType.Spear || thisGilesItemType == ItemType.Sword || thisGilesItemType == ItemType.Wand ||
                thisGilesItemType == ItemType.TwoHandDaibo || thisGilesItemType == ItemType.TwoHandCrossbow || thisGilesItemType == ItemType.TwoHandMace ||
                thisGilesItemType == ItemType.TwoHandMighty || thisGilesItemType == ItemType.TwoHandPolearm || thisGilesItemType == ItemType.TwoHandStaff ||
                thisGilesItemType == ItemType.TwoHandSword || thisGilesItemType == ItemType.TwoHandAxe || thisGilesItemType == ItemType.HandCrossbow || thisGilesItemType == ItemType.TwoHandBow)
                iThisNeedScore = Settings.Loot.TownRun.WeaponScore;

            // Jewelry
            if (thisGilesItemType == ItemType.Ring || thisGilesItemType == ItemType.Amulet || thisGilesItemType == ItemType.FollowerEnchantress ||
                thisGilesItemType == ItemType.FollowerScoundrel || thisGilesItemType == ItemType.FollowerTemplar)
                iThisNeedScore = Settings.Loot.TownRun.JewelryScore;

            // Armor
            if (thisGilesItemType == ItemType.Mojo || thisGilesItemType == ItemType.Orb || thisGilesItemType == ItemType.Quiver ||
                thisGilesItemType == ItemType.Shield || thisGilesItemType == ItemType.Belt || thisGilesItemType == ItemType.Boots ||
                thisGilesItemType == ItemType.Bracer || thisGilesItemType == ItemType.Chest || thisGilesItemType == ItemType.Cloak ||
                thisGilesItemType == ItemType.Gloves || thisGilesItemType == ItemType.Helm || thisGilesItemType == ItemType.Legs ||
                thisGilesItemType == ItemType.MightyBelt || thisGilesItemType == ItemType.Shoulder || thisGilesItemType == ItemType.SpiritStone ||
                thisGilesItemType == ItemType.VoodooMask || thisGilesItemType == ItemType.WizardHat)
                iThisNeedScore = Settings.Loot.TownRun.ArmorScore;
            return Math.Round(iThisNeedScore);
        }

        /// <summary>
        /// Checks if score of item is suffisant for throw notification.
        /// </summary>
        /// <param name="thisgilesbaseitemtype">The thisgilesbaseitemtype.</param>
        /// <param name="ithisitemvalue">The ithisitemvalue.</param>
        /// <returns></returns>
        public static bool CheckScoreForNotification(ItemBaseType thisgilesbaseitemtype, double ithisitemvalue)
        {
            switch (thisgilesbaseitemtype)
            {
                case ItemBaseType.WeaponOneHand:
                case ItemBaseType.WeaponRange:
                case ItemBaseType.WeaponTwoHand:
                    return (ithisitemvalue >= Settings.Notification.WeaponScore);
                case ItemBaseType.Armor:
                case ItemBaseType.Offhand:
                    return (ithisitemvalue >= Settings.Notification.ArmorScore);
                case ItemBaseType.Jewelry:
                    return (ithisitemvalue >= Settings.Notification.JewelryScore);
            }
            return false;
        }

        /// <summary>
        /// Full Output Of Item Stats
        /// </summary>
        private static void OutputReport()
        {
            TimeSpan TotalRunningTime = DateTime.Now.Subtract(ItemStatsWhenStartedBot);
            string sLogFileName = ZetaDia.Service.CurrentHero.BattleTagName + " - Stats - " + ZetaDia.Actors.Me.ActorClass.ToString() + ".log";

            // Create whole new file
            FileStream LogStream = File.Open(sTrinityPluginPath + sLogFileName, FileMode.Create, FileAccess.Write, FileShare.Read);
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
        private static Vector2 FindValidBackpackLocation(bool IsOriginalTwoSlot)
        {
            bool[,] BackpackSlotBlocked = new bool[10, 6];

            // Block off the entire of any "protected bag slots"
            foreach (InventorySquare square in Zeta.CommonBot.Settings.CharacterSettings.Instance.ProtectedBagSlots)
            {
                BackpackSlotBlocked[square.Column, square.Row] = true;
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
                ItemType tempItemType = DetermineItemType(item.InternalName, item.ItemType, item.FollowerSpecialType);
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
