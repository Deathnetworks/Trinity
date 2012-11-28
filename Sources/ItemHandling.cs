using GilesTrinity.DbProvider;
using GilesTrinity.Settings.Loot;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading;
using Zeta;
using Zeta.Common;
using Zeta.Common.Plugins;
using Zeta.CommonBot;
using Zeta.Internals.Actors;
using GilesTrinity.ItemRules;
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
        private static bool GilesPickupItemValidation(string name, int level, ItemQuality quality, int balanceId, ItemType dbItemType, FollowerType followerType, int dynamicID = 0)
        {

            // If it's legendary, we always want it *IF* it's level is right
            if (quality >= ItemQuality.Legendary)
            {
                if (Settings.Loot.Pickup.LegendaryLevel > 0 && (level >= Settings.Loot.Pickup.LegendaryLevel || Settings.Loot.Pickup.LegendaryLevel == 1))
                    return true;
                return false;
            }

            // Calculate giles item types and base types etc.
            GItemType itemType = DetermineItemType(name, dbItemType, followerType);
            GBaseItemType baseType = DetermineBaseType(itemType);

            // Error logging for DemonBuddy item mis-reading
            ItemType gilesDBItemType = GilesToDBItemType(itemType);
            if (gilesDBItemType != dbItemType)
            {
                Log("GSError: Item type mis-match detected: Item Internal=" + name + ". DemonBuddy ItemType thinks item type is=" + dbItemType.ToString() + ". Giles thinks item type is=" +
                    gilesDBItemType.ToString() + ". [pickup]", true);
            }
            switch (baseType)
            {
                case GBaseItemType.WeaponTwoHand:
                case GBaseItemType.WeaponOneHand:
                case GBaseItemType.WeaponRange:

                    // Not enough DPS, so analyse for possibility to blacklist
                    if (quality < ItemQuality.Magic1)
                    {
                        
                        // White item, blacklist
                        return false;
                    }
                    if (quality >= ItemQuality.Magic1 && quality < ItemQuality.Rare4)
                    {
                        if (Settings.Loot.Pickup.WeaponBlueLevel == 0 || (Settings.Loot.Pickup.WeaponBlueLevel != 0 && level < Settings.Loot.Pickup.WeaponBlueLevel))
                        {

                            // Between magic and rare, and either we want no blues, or this level is higher than the blue level we want
                            return false;
                        }
                    }
                    else
                    {
                        if (Settings.Loot.Pickup.WeaponYellowLevel == 0 || (Settings.Loot.Pickup.WeaponYellowLevel != 0 && level < Settings.Loot.Pickup.WeaponYellowLevel))
                        {

                            // Between magic and rare, and either we want no blues, or this level is higher than the blue level we want
                            return false;
                        }
                    }
                    break;
                case GBaseItemType.Armor:
                case GBaseItemType.Offhand:
                    if (quality < ItemQuality.Magic1)
                    {

                        // White item, blacklist
                        return false;
                    }
                    if (quality >= ItemQuality.Magic1 && quality < ItemQuality.Rare4)
                    {
                        if (Settings.Loot.Pickup.ArmorBlueLevel == 0 || (Settings.Loot.Pickup.ArmorBlueLevel != 0 && level < Settings.Loot.Pickup.ArmorBlueLevel))
                        {

                            // Between magic and rare, and either we want no blues, or this level is higher than the blue level we want
                            return false;
                        }
                    }
                    else
                    {
                        if (Settings.Loot.Pickup.ArmorYellowLevel == 0 || (Settings.Loot.Pickup.ArmorYellowLevel != 0 && level < Settings.Loot.Pickup.ArmorYellowLevel))
                        {

                            // Between magic and rare, and either we want no blues, or this level is higher than the blue level we want
                            return false;
                        }
                    }
                    break;
                case GBaseItemType.Jewelry:
                    if (quality < ItemQuality.Magic1)
                    {

                        // White item, blacklist
                        return false;
                    }
                    if (quality >= ItemQuality.Magic1 && quality < ItemQuality.Rare4)
                    {
                        if (Settings.Loot.Pickup.JewelryBlueLevel == 0 || (Settings.Loot.Pickup.JewelryBlueLevel != 0 && level < Settings.Loot.Pickup.JewelryBlueLevel))
                        {

                            // Between magic and rare, and either we want no blues, or this level is higher than the blue level we want
                            return false;
                        }
                    }
                    else
                    {
                        if (Settings.Loot.Pickup.JewelryYellowLevel == 0 || (Settings.Loot.Pickup.JewelryYellowLevel != 0 && level < Settings.Loot.Pickup.JewelryYellowLevel))
                        {

                            // Between magic and rare, and either we want no blues, or this level is higher than the blue level we want
                            return false;
                        }
                    }
                    break;
                case GBaseItemType.FollowerItem:
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
                case GBaseItemType.Gem:
                    if (level < Settings.Loot.Pickup.GemLevel || 
                        (itemType == GItemType.Ruby && (Settings.Loot.Pickup.GemType & TrinityGemType.Ruby) != TrinityGemType.Ruby) || 
                        (itemType == GItemType.Emerald && (Settings.Loot.Pickup.GemType & TrinityGemType.Emerald) != TrinityGemType.Emerald) ||
                        (itemType == GItemType.Amethyst && (Settings.Loot.Pickup.GemType & TrinityGemType.Amethys) != TrinityGemType.Amethys) ||
                        (itemType == GItemType.Topaz && (Settings.Loot.Pickup.GemType & TrinityGemType.Topaz) != TrinityGemType.Topaz))
                    {
                        return false;
                    }
                    break;
                case GBaseItemType.Misc:

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
                        if (Settings.Loot.Pickup.PotionMode == PotionMode.Ignore || level < Settings.Loot.Pickup.Potionlevel)
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
                case GBaseItemType.HealthGlobe:
                    return false;
                case GBaseItemType.Unknown:
                    return false;
                default:
                    DbHelper.Log("default case");
                    return false;
            }

            // Switch giles base item type

            // Didn't cancel it, so default to true!
            return true;
        }

        /// <summary>
        /// DetermineItemType - Calculates what kind of item it is from D3 internalnames
        /// </summary>
        /// <param name="name"></param>
        /// <param name="dbItemType"></param>
        /// <param name="dbFollowerType"></param>
        /// <returns></returns>
        private static GItemType DetermineItemType(string name, ItemType dbItemType, FollowerType dbFollowerType = FollowerType.None)
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
        private static GBaseItemType DetermineBaseType(GItemType itemType)
        {
            GBaseItemType thisGilesBaseType = GBaseItemType.Unknown;
            if (itemType == GItemType.Axe || itemType == GItemType.CeremonialKnife || itemType == GItemType.Dagger ||
                itemType == GItemType.FistWeapon || itemType == GItemType.Mace || itemType == GItemType.MightyWeapon ||
                itemType == GItemType.Spear || itemType == GItemType.Sword || itemType == GItemType.Wand)
            {
                thisGilesBaseType = GBaseItemType.WeaponOneHand;
            }
            else if (itemType == GItemType.TwoHandDaibo || itemType == GItemType.TwoHandMace ||
                itemType == GItemType.TwoHandMighty || itemType == GItemType.TwoHandPolearm || itemType == GItemType.TwoHandStaff ||
                itemType == GItemType.TwoHandSword || itemType == GItemType.TwoHandAxe)
            {
                thisGilesBaseType = GBaseItemType.WeaponTwoHand;
            }
            else if (itemType == GItemType.TwoHandCrossbow || itemType == GItemType.HandCrossbow || itemType == GItemType.TwoHandBow)
            {
                thisGilesBaseType = GBaseItemType.WeaponRange;
            }
            else if (itemType == GItemType.Mojo || itemType == GItemType.Orb ||
                itemType == GItemType.Quiver || itemType == GItemType.Shield)
            {
                thisGilesBaseType = GBaseItemType.Offhand;
            }
            else if (itemType == GItemType.Boots || itemType == GItemType.Bracer || itemType == GItemType.Chest ||
                itemType == GItemType.Cloak || itemType == GItemType.Gloves || itemType == GItemType.Helm ||
                itemType == GItemType.Legs || itemType == GItemType.Shoulder || itemType == GItemType.SpiritStone ||
                itemType == GItemType.VoodooMask || itemType == GItemType.WizardHat || itemType == GItemType.Belt ||
                itemType == GItemType.MightyBelt)
            {
                thisGilesBaseType = GBaseItemType.Armor;
            }
            else if (itemType == GItemType.Amulet || itemType == GItemType.Ring)
            {
                thisGilesBaseType = GBaseItemType.Jewelry;
            }
            else if (itemType == GItemType.FollowerEnchantress || itemType == GItemType.FollowerScoundrel ||
                itemType == GItemType.FollowerTemplar)
            {
                thisGilesBaseType = GBaseItemType.FollowerItem;
            }
            else if (itemType == GItemType.CraftingMaterial || itemType == GItemType.CraftTome ||
                itemType == GItemType.SpecialItem || itemType == GItemType.CraftingPlan || itemType == GItemType.HealthPotion ||
                itemType == GItemType.Dye || itemType == GItemType.StaffOfHerding || itemType == GItemType.InfernalKey)
            {
                thisGilesBaseType = GBaseItemType.Misc;
            }
            else if (itemType == GItemType.Ruby || itemType == GItemType.Emerald || itemType == GItemType.Topaz ||
                itemType == GItemType.Amethyst)
            {
                thisGilesBaseType = GBaseItemType.Gem;
            }
            else if (itemType == GItemType.HealthGlobe)
            {
                thisGilesBaseType = GBaseItemType.HealthGlobe;
            }
            return thisGilesBaseType;
        }

        /// <summary>
        /// DetermineIsStackable - Calculates what items can be stacked up
        /// </summary>
        /// <param name="itemType"></param>
        /// <returns></returns>
        private static bool DetermineIsStackable(GItemType itemType)
        {
            bool bIsStackable = itemType == GItemType.CraftingMaterial || itemType == GItemType.CraftTome || itemType == GItemType.Ruby ||
                                itemType == GItemType.Emerald || itemType == GItemType.Topaz || itemType == GItemType.Amethyst ||
                                itemType == GItemType.HealthPotion || itemType == GItemType.CraftingPlan || itemType == GItemType.Dye ||
                                itemType == GItemType.InfernalKey;
            return bIsStackable;
        }

        /// <summary>
        /// DetermineIsTwoSlot - Tries to calculate what items take up 2 slots or 1
        /// </summary>
        /// <param name="itemType"></param>
        /// <returns></returns>
        private static bool DetermineIsTwoSlot(GItemType itemType)
        {
            if (itemType == GItemType.Axe || itemType == GItemType.CeremonialKnife || itemType == GItemType.Dagger ||
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
                itemType == GItemType.VoodooMask || itemType == GItemType.WizardHat || itemType == GItemType.StaffOfHerding)
                return true;
            return false;
        }

        /// <summary>
        /// This is for DemonBuddy error checking - see what sort of item DB THINKS it is
        /// </summary>
        /// <param name="itemType"></param>
        /// <returns></returns>
        private static ItemType GilesToDBItemType(GItemType itemType)
        {
            switch (itemType)
            {
                case GItemType.Axe: return ItemType.Axe;
                case GItemType.CeremonialKnife: return ItemType.CeremonialDagger;
                case GItemType.HandCrossbow: return ItemType.HandCrossbow;
                case GItemType.Dagger: return ItemType.Dagger;
                case GItemType.FistWeapon: return ItemType.FistWeapon;
                case GItemType.Mace: return ItemType.Mace;
                case GItemType.MightyWeapon: return ItemType.MightyWeapon;
                case GItemType.Spear: return ItemType.Spear;
                case GItemType.Sword: return ItemType.Sword;
                case GItemType.Wand: return ItemType.Wand;
                case GItemType.TwoHandAxe: return ItemType.Axe;
                case GItemType.TwoHandBow: return ItemType.Bow;
                case GItemType.TwoHandDaibo: return ItemType.Daibo;
                case GItemType.TwoHandCrossbow: return ItemType.Crossbow;
                case GItemType.TwoHandMace: return ItemType.Mace;
                case GItemType.TwoHandMighty: return ItemType.MightyWeapon;
                case GItemType.TwoHandPolearm: return ItemType.Polearm;
                case GItemType.TwoHandStaff: return ItemType.Staff;
                case GItemType.TwoHandSword: return ItemType.Sword;
                case GItemType.StaffOfHerding: return ItemType.Staff;
                case GItemType.Mojo: return ItemType.Mojo;
                case GItemType.Orb: return ItemType.Orb;
                case GItemType.Quiver: return ItemType.Quiver;
                case GItemType.Shield: return ItemType.Shield;
                case GItemType.Amulet: return ItemType.Amulet;
                case GItemType.Ring: return ItemType.Ring;
                case GItemType.Belt: return ItemType.Belt;
                case GItemType.Boots: return ItemType.Boots;
                case GItemType.Bracer: return ItemType.Bracer;
                case GItemType.Chest: return ItemType.Chest;
                case GItemType.Cloak: return ItemType.Cloak;
                case GItemType.Gloves: return ItemType.Gloves;
                case GItemType.Helm: return ItemType.Helm;
                case GItemType.Legs: return ItemType.Legs;
                case GItemType.MightyBelt: return ItemType.MightyBelt;
                case GItemType.Shoulder: return ItemType.Shoulder;
                case GItemType.SpiritStone: return ItemType.SpiritStone;
                case GItemType.VoodooMask: return ItemType.VoodooMask;
                case GItemType.WizardHat: return ItemType.WizardHat;
                case GItemType.FollowerEnchantress: return ItemType.FollowerSpecial;
                case GItemType.FollowerScoundrel: return ItemType.FollowerSpecial;
                case GItemType.FollowerTemplar: return ItemType.FollowerSpecial;
                case GItemType.CraftingMaterial: return ItemType.CraftingReagent;
                case GItemType.CraftTome: return ItemType.CraftingPage;
                case GItemType.Ruby: return ItemType.Gem;
                case GItemType.Emerald: return ItemType.Gem;
                case GItemType.Topaz: return ItemType.Gem;
                case GItemType.Amethyst: return ItemType.Gem;
                case GItemType.SpecialItem: return ItemType.Unknown;
                case GItemType.CraftingPlan: return ItemType.CraftingPlan;
                case GItemType.HealthPotion: return ItemType.Potion;
                case GItemType.Dye: return ItemType.Unknown;
                case GItemType.InfernalKey: return ItemType.Unknown;
            }
            return ItemType.Unknown;
        }

        /// <summary>
        /// Arrange your stash by highest to lowest scoring items
        /// </summary>
        public class GilesStashSort
        {
            public double dStashScore { get; set; }
            public int iStashOrPack { get; set; }
            public int iInventoryColumn { get; set; }
            public int InventoryRow { get; set; }
            public int iDynamicID { get; set; }
            public bool bIsTwoSlot { get; set; }
            public GilesStashSort(double stashscore, int stashorpack, int icolumn, int irow, int dynamicid, bool twoslot)
            {
                dStashScore = stashscore;
                iStashOrPack = stashorpack;
                iInventoryColumn = icolumn;
                InventoryRow = irow;
                iDynamicID = dynamicid;
                bIsTwoSlot = twoslot;
            }
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
        private static void SortStash()
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
                Log("Failure getting your player data from DemonBuddy, abandoning the sort!");
                return;
            }
            if (iPlayerDynamicID == -1)
            {
                Log("Failure getting your player data, abandoning the sort!");
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
                GItemType tempItemType = DetermineItemType(item.InternalName, item.ItemType, item.FollowerSpecialType);
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
                GItemType itemType = DetermineItemType(item.InternalName, item.ItemType, item.FollowerSpecialType);

                bool isTwoSlot = DetermineIsTwoSlot(itemType);
                if (isTwoSlot && inventoryRow != 19 && inventoryRow != 9 && inventoryRow != 29)
                {
                    StashSlotBlocked[inventoryColumn, inventoryRow + 1] = true;
                }
                else if (isTwoSlot && (inventoryRow == 19 || inventoryRow == 9 || inventoryRow == 29))
                {
                    Log("WARNING: There was an error reading your stash, abandoning the process.");
                    Log("Always make sure you empty your backpack, open the stash, then RESTART DEMONBUDDY before sorting!");
                    return;
                }
                GilesCachedACDItem thiscacheditem = new GilesCachedACDItem(item, item.InternalName, item.Name, item.Level, item.ItemQualityLevel, item.Gold, item.GameBalanceId,
                    item.DynamicId, item.Stats.WeaponDamagePerSecond, item.IsOneHand, item.IsTwoHand, item.DyeType, item.ItemType, item.ItemBaseType, item.FollowerSpecialType,
                    item.IsUnidentified, item.ItemStackQuantity, item.Stats);

                double ItemValue = ValueThisItem(thiscacheditem, itemType);
                double NeedScore = ScoreNeeded(itemType);

                // Ignore stackable items
                if (!DetermineIsStackable(itemType) && itemType != GItemType.StaffOfHerding)
                {
                    listSortMyStash.Add(new GilesStashSort(((ItemValue / NeedScore) * 1000), 1, inventoryColumn, inventoryRow, item.DynamicId, isTwoSlot));
                }
            }

            // Loop through all stash items

            // Sort the items in the stash by their row number, lowest to highest
            listSortMyStash.Sort((p1, p2) => p1.InventoryRow.CompareTo(p2.InventoryRow));

            // Now move items into your backpack until full, then into the END of the stash
            Vector2 vFreeSlot;
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
                    Log("Failure trying to put things back into stash, no stash slots free? Abandoning...");
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
            Log("Stash sorted!");
        }

        /// <summary>
        /// Output test scores for everything in the backpack
        /// </summary>
        private static void TestScoring()
        {
            if (testingBackpack) return;
            testingBackpack = true;
            ZetaDia.Actors.Update();
            if (ZetaDia.Actors.Me == null)
            {
                Logging.Write("Error testing scores - not in game world?");
                return;
            }
            if (ZetaDia.IsInGame && !ZetaDia.IsLoadingWorld)
            {
                bOutputItemScores = true;
                Logging.Write("===== Outputting Test Scores =====");
                foreach (ACDItem item in ZetaDia.Actors.Me.Inventory.Backpack)
                {
                    if (item.BaseAddress == IntPtr.Zero)
                    {
                        Logging.Write("GSError: Diablo 3 memory read error, or item became invalid [TestScore-1]");
                    }
                    else
                    {
                        GilesCachedACDItem thiscacheditem = new GilesCachedACDItem(item, item.InternalName, item.Name, item.Level, item.ItemQualityLevel, item.Gold, item.GameBalanceId, item.DynamicId,
                            item.Stats.WeaponDamagePerSecond, item.IsOneHand, item.IsTwoHand, item.DyeType, item.ItemType, item.ItemBaseType, item.FollowerSpecialType, item.IsUnidentified, item.ItemStackQuantity,
                            item.Stats);
                        bool bShouldStashTest = ShouldWeStashThis(thiscacheditem);
                        Logging.Write(bShouldStashTest ? "* KEEP *" : "-- TRASH --");
                    }
                }
                Logging.Write("===== Finished Test Score Outputs =====");
                Logging.Write("Note: See bad scores? Wrong item types? Known DB bug - restart DB before using the test button!");
                bOutputItemScores = false;
            }
            else
            {
                Logging.Write("Error testing scores - not in game world?");
            }
            testingBackpack = false;
        }

        /// <summary>
        /// Determine if we should stash this item or not based on item type and score
        /// </summary>
        /// <param name="thisitem"></param>
        /// <returns></returns>
        private static bool ShouldWeStashThis(GilesCachedACDItem thisitem)
        {
            // auto trash blue items
            if ((thisitem.DBBaseType == ItemBaseType.Armor || thisitem.DBBaseType == ItemBaseType.Jewelry || thisitem.DBBaseType == ItemBaseType.Weapon) && thisitem.Quality < ItemQuality.Rare4)
            {
                return false;
            }

            // Stash all unidentified items - assume we want to keep them since we are using an identifier over-ride
            if (thisitem.IsUnidentified)
            {
                if (bOutputItemScores) Log(thisitem.RealName + " [" + thisitem.InternalName + "] = (autokeep unidentified items)");
                return true;
            }

            // Now look for Misc items we might want to keep
            GItemType TrueItemType = DetermineItemType(thisitem.InternalName, thisitem.DBItemType, thisitem.FollowerType);
            GBaseItemType thisGilesBaseType = DetermineBaseType(TrueItemType);

            if (TrueItemType == GItemType.StaffOfHerding)
            {
                if (bOutputItemScores) Log(thisitem.RealName + " [" + thisitem.InternalName + "] [" + TrueItemType.ToString() + "] = (autokeep staff of herding)");
                return true;
            }
            if (TrueItemType == GItemType.CraftingMaterial)
            {
                if (bOutputItemScores) Log(thisitem.RealName + " [" + thisitem.InternalName + "] [" + TrueItemType.ToString() + "] = (autokeep craft materials)");
                return true;
            }
            if (TrueItemType == GItemType.CraftingPlan)
            {
                if (bOutputItemScores) Log(thisitem.RealName + " [" + thisitem.InternalName + "] [" + TrueItemType.ToString() + "] = (autokeep plans)");
                return true;
            }
            if (TrueItemType == GItemType.Emerald)
            {
                if (bOutputItemScores) Log(thisitem.RealName + " [" + thisitem.InternalName + "] [" + TrueItemType.ToString() + "] = (autokeep gems)");
                return true;
            }
            if (TrueItemType == GItemType.Amethyst)
            {
                if (bOutputItemScores) Log(thisitem.RealName + " [" + thisitem.InternalName + "] [" + TrueItemType.ToString() + "] = (autokeep gems)");
                return true;
            }
            if (TrueItemType == GItemType.Topaz)
            {
                if (bOutputItemScores) Log(thisitem.RealName + " [" + thisitem.InternalName + "] [" + TrueItemType.ToString() + "] = (autokeep gems)");
                return true;
            }
            if (TrueItemType == GItemType.Ruby)
            {
                if (bOutputItemScores) Log(thisitem.RealName + " [" + thisitem.InternalName + "] [" + TrueItemType.ToString() + "] = (autokeep gems)");
                return true;
            }
            if (TrueItemType == GItemType.CraftTome)
            {
                if (bOutputItemScores) Log(thisitem.RealName + " [" + thisitem.InternalName + "] [" + TrueItemType.ToString() + "] = (autokeep tomes)");
                return true;
            }
            if (TrueItemType == GItemType.InfernalKey)
            {
                if (bOutputItemScores) Log(thisitem.RealName + " [" + thisitem.InternalName + "] [" + TrueItemType.ToString() + "] = (autokeep infernal key)");
                return true;
            }
            if (TrueItemType == GItemType.HealthPotion)
            {
                if (bOutputItemScores) Log(thisitem.RealName + " [" + thisitem.InternalName + "] [" + TrueItemType.ToString() + "] = (ignoring potions)");
                return false;
            }

            if (thisitem.Quality >= ItemQuality.Legendary)
            {
                if (bOutputItemScores) Log(thisitem.RealName + " [" + thisitem.InternalName + "] [" + TrueItemType.ToString() + "] = (autokeep legendaries)");
                return true;
            }

            if (Settings.Loot.ItemFilterMode == ItemFilterMode.TrinityWithItemRules)
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


            // Ok now try to do some decent item scoring based on item types
            double iNeedScore = ScoreNeeded(TrueItemType);
            double iMyScore = ValueThisItem(thisitem, TrueItemType);

            if (bOutputItemScores) Log(thisitem.RealName + " [" + thisitem.InternalName + "] [" + TrueItemType.ToString() + "] = " + iMyScore.ToString());
            if (iMyScore >= iNeedScore) return true;

            // If we reached this point, then we found no reason to keep the item!
            return false;
        }

        /// <summary>
        /// Return the score needed to keep something by the item type
        /// </summary>
        /// <param name="thisGilesItemType"></param>
        /// <returns></returns>
        private static double ScoreNeeded(GItemType thisGilesItemType)
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

        public static bool EvaluateItemScoreForNotification(GBaseItemType thisgilesbaseitemtype, double ithisitemvalue)
        {
            switch (thisgilesbaseitemtype)
            {
                case GBaseItemType.WeaponOneHand:
                case GBaseItemType.WeaponRange:
                case GBaseItemType.WeaponTwoHand:
                    if (ithisitemvalue >= Settings.Notification.WeaponScore)
                        return true;
                    break;
                case GBaseItemType.Armor:
                case GBaseItemType.Offhand:
                    if (ithisitemvalue >= Settings.Notification.ArmorScore)
                        return true;
                    break;
                case GBaseItemType.Jewelry:
                    if (ithisitemvalue >= Settings.Notification.JewelryScore)
                        return true;
                    break;
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
