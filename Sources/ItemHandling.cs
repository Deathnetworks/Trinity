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
namespace GilesTrinity
{
    public partial class GilesTrinity : IPlugin
    {

        // Item handling Stuff

        // Randomize the timer between stashing/salvaging etc.
        private static void RandomizeTheTimer()
        {
            Random rndNum = new Random(int.Parse(Guid.NewGuid().ToString().Substring(0, 8), NumberStyles.HexNumber));
            int rnd = rndNum.Next(7);
            iItemDelayLoopLimit = 4 + rnd;
        }

        // Pickup Validation - Determines what should or should not be picked up
        private static bool GilesPickupItemValidation(string tempname, int templevel, ItemQuality tempquality, int tempbalanceid, ItemType thisdbitemtype, FollowerType thisfollowertype, int iDynamicID = 0)
        {

            // If it's legendary, we always want it *IF* it's level is right
            if (tempquality >= ItemQuality.Legendary)
            {
                if (settings.iFilterLegendary > 0 && (templevel >= settings.iFilterLegendary || settings.iFilterLegendary == 1))
                    return true;
                return false;
            }

            // Calculate giles item types and base types etc.
            GilesItemType thisGilesItemType = DetermineItemType(tempname, thisdbitemtype, thisfollowertype);
            GilesBaseItemType thisGilesBaseType = DetermineBaseType(thisGilesItemType);

            // Error logging for DemonBuddy item mis-reading
            ItemType gilesDBItemType = GilesToDBItemType(thisGilesItemType);
            if (gilesDBItemType != thisdbitemtype)
            {
                Log("GSError: Item type mis-match detected: Item Internal=" + tempname + ". DemonBuddy ItemType thinks item type is=" + thisdbitemtype.ToString() + ". Giles thinks item type is=" +
                    gilesDBItemType.ToString() + ". [pickup]", true);
            }
            switch (thisGilesBaseType)
            {
                case GilesBaseItemType.WeaponTwoHand:
                case GilesBaseItemType.WeaponOneHand:
                case GilesBaseItemType.WeaponRange:

                    // Not enough DPS, so analyse for possibility to blacklist
                    if (tempquality < ItemQuality.Magic1)
                    {

                        // White item, blacklist
                        return false;
                    }
                    if (tempquality >= ItemQuality.Magic1 && tempquality < ItemQuality.Rare4)
                    {
                        if (settings.iFilterBlueWeapons == 0 || (settings.iFilterBlueWeapons != 0 && templevel < settings.iFilterBlueWeapons))
                        {

                            // Between magic and rare, and either we want no blues, or this level is higher than the blue level we want
                            return false;
                        }
                    }
                    else
                    {
                        if (settings.iFilterYellowWeapons == 0 || (settings.iFilterYellowWeapons != 0 && templevel < settings.iFilterYellowWeapons))
                        {

                            // Between magic and rare, and either we want no blues, or this level is higher than the blue level we want
                            return false;
                        }
                    }
                    break;
                case GilesBaseItemType.Armor:
                case GilesBaseItemType.Offhand:
                    if (tempquality < ItemQuality.Magic1)
                    {

                        // White item, blacklist
                        return false;
                    }
                    if (tempquality >= ItemQuality.Magic1 && tempquality < ItemQuality.Rare4)
                    {
                        if (settings.iFilterBlueArmor == 0 || (settings.iFilterBlueArmor != 0 && templevel < settings.iFilterBlueArmor))
                        {

                            // Between magic and rare, and either we want no blues, or this level is higher than the blue level we want
                            return false;
                        }
                    }
                    else
                    {
                        if (settings.iFilterYellowArmor == 0 || (settings.iFilterYellowArmor != 0 && templevel < settings.iFilterYellowArmor))
                        {

                            // Between magic and rare, and either we want no blues, or this level is higher than the blue level we want
                            return false;
                        }
                    }
                    break;
                case GilesBaseItemType.Jewelry:
                    if (tempquality < ItemQuality.Magic1)
                    {

                        // White item, blacklist
                        return false;
                    }
                    if (tempquality >= ItemQuality.Magic1 && tempquality < ItemQuality.Rare4)
                    {
                        if (settings.iFilterBlueJewelry == 0 || (settings.iFilterBlueJewelry != 0 && templevel < settings.iFilterBlueJewelry))
                        {

                            // Between magic and rare, and either we want no blues, or this level is higher than the blue level we want
                            return false;
                        }
                    }
                    else
                    {
                        if (settings.iFilterYellowJewelry == 0 || (settings.iFilterYellowJewelry != 0 && templevel < settings.iFilterYellowJewelry))
                        {

                            // Between magic and rare, and either we want no blues, or this level is higher than the blue level we want
                            return false;
                        }
                    }
                    break;
                case GilesBaseItemType.FollowerItem:
                    if (templevel < 60 || !settings.bPickupFollower || tempquality < ItemQuality.Rare4)
                    {
                        if (!_hashsetItemFollowersIgnored.Contains(iDynamicID))
                        {
                            _hashsetItemFollowersIgnored.Add(iDynamicID);
                            iTotalFollowerItemsIgnored++;
                        }
                        return false;
                    }
                    break;
                case GilesBaseItemType.Gem:
                    if (templevel < settings.iFilterGems || (thisGilesItemType == GilesItemType.Ruby && !settings.bGemsRuby) || (thisGilesItemType == GilesItemType.Emerald && !settings.bGemsEmerald) ||
                        (thisGilesItemType == GilesItemType.Amethyst && !settings.bGemsAmethyst) || (thisGilesItemType == GilesItemType.Topaz && !settings.bGemsTopaz))
                    {
                        return false;
                    }
                    break;
                case GilesBaseItemType.Misc:

                    // Note; Infernal keys are misc, so should be picked up here - we aren't filtering them out, so should default to true at the end of this function
                    if (thisGilesItemType == GilesItemType.CraftingMaterial && templevel < settings.iFilterMisc)
                    {
                        return false;
                    }
                    if (thisGilesItemType == GilesItemType.CraftTome && (templevel < settings.iFilterMisc || !settings.bPickupCraftTomes))
                    {
                        return false;
                    }
                    if (thisGilesItemType == GilesItemType.CraftingPlan && !settings.bPickupPlans)
                    {
                        return false;
                    }

                    // Potion filtering
                    if (thisGilesItemType == GilesItemType.HealthPotion)
                    {
                        if (settings.iFilterPotions == 1 || templevel < settings.iFilterPotionLevel)
                        {
                            return false;
                        }
                        if (settings.iFilterPotions == 2)
                        {

                            // Map out all the items already in the backpack
                            int iTotalPotions =
                                (from tempitem in ZetaDia.Me.Inventory.Backpack where tempitem.BaseAddress != IntPtr.Zero where tempitem.GameBalanceId == tempbalanceid select tempitem.ItemStackQuantity).Sum();
                            if (iTotalPotions > 98)
                            {
                                return false;
                            }
                        }
                    }
                    break;
                case GilesBaseItemType.HealthGlobe:
                    return false;
                case GilesBaseItemType.Unknown:
                    return false;
                default:
                    return false;
            }

            // Switch giles base item type

            // Didn't cancel it, so default to true!
            return true;
        }

        // * DetermineItemType - Calculates what kind of item it is from D3 internalnames
        private static GilesItemType DetermineItemType(string sThisInternalName, ItemType DBItemType, FollowerType dbFollowerType = FollowerType.None)
        {
            sThisInternalName = sThisInternalName.ToLower();
            if (sThisInternalName.StartsWith("axe_")) return GilesItemType.Axe;
            if (sThisInternalName.StartsWith("ceremonialdagger_")) return GilesItemType.CeremonialKnife;
            if (sThisInternalName.StartsWith("handxbow_")) return GilesItemType.HandCrossbow;
            if (sThisInternalName.StartsWith("dagger_")) return GilesItemType.Dagger;
            if (sThisInternalName.StartsWith("fistweapon_")) return GilesItemType.FistWeapon;
            if (sThisInternalName.StartsWith("mace_")) return GilesItemType.Mace;
            if (sThisInternalName.StartsWith("mightyweapon_1h_")) return GilesItemType.MightyWeapon;
            if (sThisInternalName.StartsWith("spear_")) return GilesItemType.Spear;
            if (sThisInternalName.StartsWith("sword_")) return GilesItemType.Sword;
            if (sThisInternalName.StartsWith("wand_")) return GilesItemType.Wand;
            if (sThisInternalName.StartsWith("twohandedaxe_")) return GilesItemType.TwoHandAxe;
            if (sThisInternalName.StartsWith("bow_")) return GilesItemType.TwoHandBow;
            if (sThisInternalName.StartsWith("combatstaff_")) return GilesItemType.TwoHandDaibo;
            if (sThisInternalName.StartsWith("xbow_")) return GilesItemType.TwoHandCrossbow;
            if (sThisInternalName.StartsWith("twohandedmace_")) return GilesItemType.TwoHandMace;
            if (sThisInternalName.StartsWith("mightyweapon_2h_")) return GilesItemType.TwoHandMighty;
            if (sThisInternalName.StartsWith("polearm_")) return GilesItemType.TwoHandPolearm;
            if (sThisInternalName.StartsWith("staff_")) return GilesItemType.TwoHandStaff;
            if (sThisInternalName.StartsWith("twohandedsword_")) return GilesItemType.TwoHandSword;
            if (sThisInternalName.StartsWith("staffofcow")) return GilesItemType.StaffOfHerding;
            if (sThisInternalName.StartsWith("mojo_")) return GilesItemType.Mojo;
            if (sThisInternalName.StartsWith("orb_")) return GilesItemType.Orb;
            if (sThisInternalName.StartsWith("quiver_")) return GilesItemType.Quiver;
            if (sThisInternalName.StartsWith("shield_")) return GilesItemType.Shield;
            if (sThisInternalName.StartsWith("amulet_")) return GilesItemType.Amulet;
            if (sThisInternalName.StartsWith("ring_")) return GilesItemType.Ring;
            if (sThisInternalName.StartsWith("boots_")) return GilesItemType.Boots;
            if (sThisInternalName.StartsWith("bracers_")) return GilesItemType.Bracer;
            if (sThisInternalName.StartsWith("cloak_")) return GilesItemType.Cloak;
            if (sThisInternalName.StartsWith("gloves_")) return GilesItemType.Gloves;
            if (sThisInternalName.StartsWith("pants_")) return GilesItemType.Legs;
            if (sThisInternalName.StartsWith("barbbelt_")) return GilesItemType.MightyBelt;
            if (sThisInternalName.StartsWith("shoulderpads_")) return GilesItemType.Shoulder;
            if (sThisInternalName.StartsWith("spiritstone_")) return GilesItemType.SpiritStone;
            if (sThisInternalName.StartsWith("voodoomask_")) return GilesItemType.VoodooMask;
            if (sThisInternalName.StartsWith("wizardhat_")) return GilesItemType.WizardHat;
            if (sThisInternalName.StartsWith("lore_book_")) return GilesItemType.CraftTome;
            if (sThisInternalName.StartsWith("page_of_")) return GilesItemType.CraftTome;
            if (sThisInternalName.StartsWith("blacksmithstome")) return GilesItemType.CraftTome;
            if (sThisInternalName.StartsWith("ruby_")) return GilesItemType.Ruby;
            if (sThisInternalName.StartsWith("emerald_")) return GilesItemType.Emerald;
            if (sThisInternalName.StartsWith("topaz_")) return GilesItemType.Topaz;
            if (sThisInternalName.StartsWith("amethyst")) return GilesItemType.Amethyst;
            if (sThisInternalName.StartsWith("healthpotion_")) return GilesItemType.HealthPotion;
            if (sThisInternalName.StartsWith("followeritem_enchantress_")) return GilesItemType.FollowerEnchantress;
            if (sThisInternalName.StartsWith("followeritem_scoundrel_")) return GilesItemType.FollowerScoundrel;
            if (sThisInternalName.StartsWith("followeritem_templar_")) return GilesItemType.FollowerTemplar;
            if (sThisInternalName.StartsWith("craftingplan_")) return GilesItemType.CraftingPlan;
            if (sThisInternalName.StartsWith("dye_")) return GilesItemType.Dye;
            if (sThisInternalName.StartsWith("a1_")) return GilesItemType.SpecialItem;
            if (sThisInternalName.StartsWith("healthglobe")) return GilesItemType.HealthGlobe;

            // Follower item types
            if (sThisInternalName.StartsWith("jewelbox_") || DBItemType == ItemType.FollowerSpecial)
            {
                if (dbFollowerType == FollowerType.Scoundrel)
                    return GilesItemType.FollowerScoundrel;
                if (dbFollowerType == FollowerType.Templar)
                    return GilesItemType.FollowerTemplar;
                if (dbFollowerType == FollowerType.Enchantress)
                    return GilesItemType.FollowerEnchantress;
            }

            // Fall back on some partial DB item type checking 
            if (sThisInternalName.StartsWith("crafting_"))
            {
                if (DBItemType == ItemType.CraftingPage) return GilesItemType.CraftTome;
                return GilesItemType.CraftingMaterial;
            }
            if (sThisInternalName.StartsWith("chestarmor_"))
            {
                if (DBItemType == ItemType.Cloak) return GilesItemType.Cloak;
                return GilesItemType.Chest;
            }
            if (sThisInternalName.StartsWith("helm_"))
            {
                if (DBItemType == ItemType.SpiritStone) return GilesItemType.SpiritStone;
                if (DBItemType == ItemType.VoodooMask) return GilesItemType.VoodooMask;
                if (DBItemType == ItemType.WizardHat) return GilesItemType.WizardHat;
                return GilesItemType.Helm;
            }
            if (sThisInternalName.StartsWith("helmcloth_"))
            {
                if (DBItemType == ItemType.SpiritStone) return GilesItemType.SpiritStone;
                if (DBItemType == ItemType.VoodooMask) return GilesItemType.VoodooMask;
                if (DBItemType == ItemType.WizardHat) return GilesItemType.WizardHat;
                return GilesItemType.Helm;
            }
            if (sThisInternalName.StartsWith("belt_"))
            {
                if (DBItemType == ItemType.MightyBelt) return GilesItemType.MightyBelt;
                return GilesItemType.Belt;
            }
            if (sThisInternalName.StartsWith("demonkey_") || sThisInternalName.StartsWith("demontrebuchetkey"))
            {
                return GilesItemType.InfernalKey;
            }

            // ORGANS QUICK HACK IN
            if (sThisInternalName.StartsWith("quest_"))
            {
                return GilesItemType.InfernalKey;
            }
            return GilesItemType.Unknown;
        }

        // DetermineBaseType - Calculates a more generic, "basic" type of item
        private static GilesBaseItemType DetermineBaseType(GilesItemType thisGilesItemType)
        {
            GilesBaseItemType thisGilesBaseType = GilesBaseItemType.Unknown;
            if (thisGilesItemType == GilesItemType.Axe || thisGilesItemType == GilesItemType.CeremonialKnife || thisGilesItemType == GilesItemType.Dagger ||
                thisGilesItemType == GilesItemType.FistWeapon || thisGilesItemType == GilesItemType.Mace || thisGilesItemType == GilesItemType.MightyWeapon ||
                thisGilesItemType == GilesItemType.Spear || thisGilesItemType == GilesItemType.Sword || thisGilesItemType == GilesItemType.Wand)
            {
                thisGilesBaseType = GilesBaseItemType.WeaponOneHand;
            }
            else if (thisGilesItemType == GilesItemType.TwoHandDaibo || thisGilesItemType == GilesItemType.TwoHandMace ||
                thisGilesItemType == GilesItemType.TwoHandMighty || thisGilesItemType == GilesItemType.TwoHandPolearm || thisGilesItemType == GilesItemType.TwoHandStaff ||
                thisGilesItemType == GilesItemType.TwoHandSword || thisGilesItemType == GilesItemType.TwoHandAxe)
            {
                thisGilesBaseType = GilesBaseItemType.WeaponTwoHand;
            }
            else if (thisGilesItemType == GilesItemType.TwoHandCrossbow || thisGilesItemType == GilesItemType.HandCrossbow || thisGilesItemType == GilesItemType.TwoHandBow)
            {
                thisGilesBaseType = GilesBaseItemType.WeaponRange;
            }
            else if (thisGilesItemType == GilesItemType.Mojo || thisGilesItemType == GilesItemType.Orb ||
                thisGilesItemType == GilesItemType.Quiver || thisGilesItemType == GilesItemType.Shield)
            {
                thisGilesBaseType = GilesBaseItemType.Offhand;
            }
            else if (thisGilesItemType == GilesItemType.Boots || thisGilesItemType == GilesItemType.Bracer || thisGilesItemType == GilesItemType.Chest ||
                thisGilesItemType == GilesItemType.Cloak || thisGilesItemType == GilesItemType.Gloves || thisGilesItemType == GilesItemType.Helm ||
                thisGilesItemType == GilesItemType.Legs || thisGilesItemType == GilesItemType.Shoulder || thisGilesItemType == GilesItemType.SpiritStone ||
                thisGilesItemType == GilesItemType.VoodooMask || thisGilesItemType == GilesItemType.WizardHat || thisGilesItemType == GilesItemType.Belt ||
                thisGilesItemType == GilesItemType.MightyBelt)
            {
                thisGilesBaseType = GilesBaseItemType.Armor;
            }
            else if (thisGilesItemType == GilesItemType.Amulet || thisGilesItemType == GilesItemType.Ring)
            {
                thisGilesBaseType = GilesBaseItemType.Jewelry;
            }
            else if (thisGilesItemType == GilesItemType.FollowerEnchantress || thisGilesItemType == GilesItemType.FollowerScoundrel ||
                thisGilesItemType == GilesItemType.FollowerTemplar)
            {
                thisGilesBaseType = GilesBaseItemType.FollowerItem;
            }
            else if (thisGilesItemType == GilesItemType.CraftingMaterial || thisGilesItemType == GilesItemType.CraftTome ||
                thisGilesItemType == GilesItemType.SpecialItem || thisGilesItemType == GilesItemType.CraftingPlan || thisGilesItemType == GilesItemType.HealthPotion ||
                thisGilesItemType == GilesItemType.Dye || thisGilesItemType == GilesItemType.StaffOfHerding || thisGilesItemType == GilesItemType.InfernalKey)
            {
                thisGilesBaseType = GilesBaseItemType.Misc;
            }
            else if (thisGilesItemType == GilesItemType.Ruby || thisGilesItemType == GilesItemType.Emerald || thisGilesItemType == GilesItemType.Topaz ||
                thisGilesItemType == GilesItemType.Amethyst)
            {
                thisGilesBaseType = GilesBaseItemType.Gem;
            }
            else if (thisGilesItemType == GilesItemType.HealthGlobe)
            {
                thisGilesBaseType = GilesBaseItemType.HealthGlobe;
            }
            return thisGilesBaseType;
        }

        // DetermineIsStackable - Calculates what items can be stacked up
        private static bool DetermineIsStackable(GilesItemType thisGilesItemType)
        {
            bool bIsStackable = thisGilesItemType == GilesItemType.CraftingMaterial || thisGilesItemType == GilesItemType.CraftTome || thisGilesItemType == GilesItemType.Ruby ||
                                thisGilesItemType == GilesItemType.Emerald || thisGilesItemType == GilesItemType.Topaz || thisGilesItemType == GilesItemType.Amethyst ||
                                thisGilesItemType == GilesItemType.HealthPotion || thisGilesItemType == GilesItemType.CraftingPlan || thisGilesItemType == GilesItemType.Dye ||
                                thisGilesItemType == GilesItemType.InfernalKey;
            return bIsStackable;
        }

        // DetermineIsTwoSlot - Tries to calculate what items take up 2 slots or 1
        private static bool DetermineIsTwoSlot(GilesItemType thisGilesItemType)
        {
            if (thisGilesItemType == GilesItemType.Axe || thisGilesItemType == GilesItemType.CeremonialKnife || thisGilesItemType == GilesItemType.Dagger ||
                thisGilesItemType == GilesItemType.FistWeapon || thisGilesItemType == GilesItemType.Mace || thisGilesItemType == GilesItemType.MightyWeapon ||
                thisGilesItemType == GilesItemType.Spear || thisGilesItemType == GilesItemType.Sword || thisGilesItemType == GilesItemType.Wand ||
                thisGilesItemType == GilesItemType.TwoHandDaibo || thisGilesItemType == GilesItemType.TwoHandCrossbow || thisGilesItemType == GilesItemType.TwoHandMace ||
                thisGilesItemType == GilesItemType.TwoHandMighty || thisGilesItemType == GilesItemType.TwoHandPolearm || thisGilesItemType == GilesItemType.TwoHandStaff ||
                thisGilesItemType == GilesItemType.TwoHandSword || thisGilesItemType == GilesItemType.TwoHandAxe || thisGilesItemType == GilesItemType.HandCrossbow ||
                thisGilesItemType == GilesItemType.TwoHandBow || thisGilesItemType == GilesItemType.Mojo || thisGilesItemType == GilesItemType.Orb ||
                thisGilesItemType == GilesItemType.Quiver || thisGilesItemType == GilesItemType.Shield || thisGilesItemType == GilesItemType.Boots ||
                thisGilesItemType == GilesItemType.Bracer || thisGilesItemType == GilesItemType.Chest || thisGilesItemType == GilesItemType.Cloak ||
                thisGilesItemType == GilesItemType.Gloves || thisGilesItemType == GilesItemType.Helm || thisGilesItemType == GilesItemType.Legs ||
                thisGilesItemType == GilesItemType.Shoulder || thisGilesItemType == GilesItemType.SpiritStone ||
                thisGilesItemType == GilesItemType.VoodooMask || thisGilesItemType == GilesItemType.WizardHat || thisGilesItemType == GilesItemType.StaffOfHerding)
                return true;
            return false;
        }

        // This is for DemonBuddy error checking - see what sort of item DB THINKS it is
        private static ItemType GilesToDBItemType(GilesItemType thisgilesitemtype)
        {
            switch (thisgilesitemtype)
            {
                case GilesItemType.Axe: return ItemType.Axe;
                case GilesItemType.CeremonialKnife: return ItemType.CeremonialDagger;
                case GilesItemType.HandCrossbow: return ItemType.HandCrossbow;
                case GilesItemType.Dagger: return ItemType.Dagger;
                case GilesItemType.FistWeapon: return ItemType.FistWeapon;
                case GilesItemType.Mace: return ItemType.Mace;
                case GilesItemType.MightyWeapon: return ItemType.MightyWeapon;
                case GilesItemType.Spear: return ItemType.Spear;
                case GilesItemType.Sword: return ItemType.Sword;
                case GilesItemType.Wand: return ItemType.Wand;
                case GilesItemType.TwoHandAxe: return ItemType.Axe;
                case GilesItemType.TwoHandBow: return ItemType.Bow;
                case GilesItemType.TwoHandDaibo: return ItemType.Daibo;
                case GilesItemType.TwoHandCrossbow: return ItemType.Crossbow;
                case GilesItemType.TwoHandMace: return ItemType.Mace;
                case GilesItemType.TwoHandMighty: return ItemType.MightyWeapon;
                case GilesItemType.TwoHandPolearm: return ItemType.Polearm;
                case GilesItemType.TwoHandStaff: return ItemType.Staff;
                case GilesItemType.TwoHandSword: return ItemType.Sword;
                case GilesItemType.StaffOfHerding: return ItemType.Staff;
                case GilesItemType.Mojo: return ItemType.Mojo;
                case GilesItemType.Orb: return ItemType.Orb;
                case GilesItemType.Quiver: return ItemType.Quiver;
                case GilesItemType.Shield: return ItemType.Shield;
                case GilesItemType.Amulet: return ItemType.Amulet;
                case GilesItemType.Ring: return ItemType.Ring;
                case GilesItemType.Belt: return ItemType.Belt;
                case GilesItemType.Boots: return ItemType.Boots;
                case GilesItemType.Bracer: return ItemType.Bracer;
                case GilesItemType.Chest: return ItemType.Chest;
                case GilesItemType.Cloak: return ItemType.Cloak;
                case GilesItemType.Gloves: return ItemType.Gloves;
                case GilesItemType.Helm: return ItemType.Helm;
                case GilesItemType.Legs: return ItemType.Legs;
                case GilesItemType.MightyBelt: return ItemType.MightyBelt;
                case GilesItemType.Shoulder: return ItemType.Shoulder;
                case GilesItemType.SpiritStone: return ItemType.SpiritStone;
                case GilesItemType.VoodooMask: return ItemType.VoodooMask;
                case GilesItemType.WizardHat: return ItemType.WizardHat;
                case GilesItemType.FollowerEnchantress: return ItemType.FollowerSpecial;
                case GilesItemType.FollowerScoundrel: return ItemType.FollowerSpecial;
                case GilesItemType.FollowerTemplar: return ItemType.FollowerSpecial;
                case GilesItemType.CraftingMaterial: return ItemType.CraftingReagent;
                case GilesItemType.CraftTome: return ItemType.CraftingPage;
                case GilesItemType.Ruby: return ItemType.Gem;
                case GilesItemType.Emerald: return ItemType.Gem;
                case GilesItemType.Topaz: return ItemType.Gem;
                case GilesItemType.Amethyst: return ItemType.Gem;
                case GilesItemType.SpecialItem: return ItemType.Unknown;
                case GilesItemType.CraftingPlan: return ItemType.CraftingPlan;
                case GilesItemType.HealthPotion: return ItemType.Potion;
                case GilesItemType.Dye: return ItemType.Unknown;
                case GilesItemType.InfernalKey: return ItemType.Unknown;
            }
            return ItemType.Unknown;
        }

        // Arrange your stash by highest to lowest scoring items
        public class GilesStashSort
        {
            public double dStashScore { get; set; }
            public int iStashOrPack { get; set; }
            public int iInventoryColumn { get; set; }
            public int iInventoryRow { get; set; }
            public int iDynamicID { get; set; }
            public bool bIsTwoSlot { get; set; }
            public GilesStashSort(double stashscore, int stashorpack, int icolumn, int irow, int dynamicid, bool twoslot)
            {
                dStashScore = stashscore;
                iStashOrPack = stashorpack;
                iInventoryColumn = icolumn;
                iInventoryRow = irow;
                iDynamicID = dynamicid;
                bIsTwoSlot = twoslot;
            }
        }

        // Search backpack to see if we have room for a 2-slot item anywhere
        private static bool[,] GilesBackpackSlotBlocked = new bool[10, 6];
        private static Vector2 SortingFindLocationBackpack(bool bOriginalTwoSlot)
        {
            int iPointX = -1;
            int iPointY = -1;
            for (int iRow = 0; iRow <= 5; iRow++)
            {
                for (int iColumn = 0; iColumn <= 9; iColumn++)
                {
                    if (!GilesBackpackSlotBlocked[iColumn, iRow])
                    {
                        bool bNotEnoughSpace = false;
                        if (iRow < 5)
                        {
                            bNotEnoughSpace = (bOriginalTwoSlot && GilesBackpackSlotBlocked[iColumn, iRow + 1]);
                        }
                        else
                        {
                            if (bOriginalTwoSlot)
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
        private static Vector2 SortingFindLocationStash(bool bOriginalTwoSlot, bool bEndOfStash = false)
        {
            int iPointX = -1;
            int iPointY = -1;
            for (int iRow = 0; iRow <= 29; iRow++)
            {
                for (int iColumn = 0; iColumn <= 6; iColumn++)
                {
                    if (!GilesStashSlotBlocked[iColumn, iRow])
                    {
                        bool bNotEnoughSpace = false;
                        if (iRow != 9 && iRow != 19 && iRow != 29)
                        {
                            bNotEnoughSpace = (bOriginalTwoSlot && GilesStashSlotBlocked[iColumn, iRow + 1]);
                        }
                        else
                        {
                            if (bOriginalTwoSlot)
                                bNotEnoughSpace = true;
                        }
                        if (!bNotEnoughSpace)
                        {
                            iPointX = iColumn;
                            iPointY = iRow;
                            if (!bEndOfStash)
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
                    GilesBackpackSlotBlocked[iColumn, iRow] = false;
            foreach (ACDItem tempitem in ZetaDia.Me.Inventory.Backpack)
            {
                int inventoryRow = tempitem.InventoryRow;
                int inventoryColumn = tempitem.InventoryColumn;

                // Mark this slot as not-free
                GilesBackpackSlotBlocked[inventoryColumn, inventoryRow] = true;

                // Try and reliably find out if this is a two slot item or not
                GilesItemType tempItemType = DetermineItemType(tempitem.InternalName, tempitem.ItemType, tempitem.FollowerSpecialType);
                if (DetermineIsTwoSlot(tempItemType) && inventoryRow < 5)
                {
                    GilesBackpackSlotBlocked[inventoryColumn, inventoryRow + 1] = true;
                }
            }

            // Map out the stash free slots
            for (int iRow = 0; iRow <= 29; iRow++)
                for (int iColumn = 0; iColumn <= 6; iColumn++)
                    GilesStashSlotBlocked[iColumn, iRow] = false;

            // Block off the entire of any "protected stash pages"
            foreach (int iProtPage in Zeta.CommonBot.Settings.CharacterSettings.Instance.ProtectedStashPages)
                for (int iProtRow = 0; iProtRow <= 9; iProtRow++)
                    for (int iProtColumn = 0; iProtColumn <= 6; iProtColumn++)
                        GilesStashSlotBlocked[iProtColumn, iProtRow + (iProtPage * 10)] = true;

            // Remove rows we don't have
            for (int iRow = (ZetaDia.Me.NumSharedStashSlots / 7); iRow <= 29; iRow++)
                for (int iColumn = 0; iColumn <= 6; iColumn++)
                    GilesStashSlotBlocked[iColumn, iRow] = true;

            // Map out all the items already in the stash and store their scores if appropriate
            foreach (ACDItem thisitem in ZetaDia.Me.Inventory.StashItems)
            {
                int inventoryRow = thisitem.InventoryRow;
                int inventoryColumn = thisitem.InventoryColumn;

                // Mark this slot as not-free
                GilesStashSlotBlocked[inventoryColumn, inventoryRow] = true;

                // Try and reliably find out if this is a two slot item or not
                GilesItemType tempItemType = DetermineItemType(thisitem.InternalName, thisitem.ItemType, thisitem.FollowerSpecialType);
                bool bIsTwoSlot = DetermineIsTwoSlot(tempItemType);
                if (bIsTwoSlot && inventoryRow != 19 && inventoryRow != 9 && inventoryRow != 29)
                {
                    GilesStashSlotBlocked[inventoryColumn, inventoryRow + 1] = true;
                }
                else if (bIsTwoSlot && (inventoryRow == 19 || inventoryRow == 9 || inventoryRow == 29))
                {
                    Log("WARNING: There was an error reading your stash, abandoning the process.");
                    Log("Always make sure you empty your backpack, open the stash, then RESTART DEMONBUDDY before sorting!");
                    return;
                }
                GilesCachedACDItem thiscacheditem = new GilesCachedACDItem(thisitem.InternalName, thisitem.Name, thisitem.Level, thisitem.ItemQualityLevel, thisitem.Gold, thisitem.GameBalanceId,
                    thisitem.DynamicId, thisitem.Stats.WeaponDamagePerSecond, thisitem.IsOneHand, thisitem.IsTwoHand, thisitem.DyeType, thisitem.ItemType, thisitem.ItemBaseType, thisitem.FollowerSpecialType,
                    thisitem.IsUnidentified, thisitem.ItemStackQuantity, thisitem.Stats);
                double iThisItemValue = ValueThisItem(thiscacheditem, tempItemType);
                double iNeedScore = ScoreNeeded(tempItemType);

                // Ignore stackable items
                if (!DetermineIsStackable(tempItemType) && tempItemType != GilesItemType.StaffOfHerding)
                {
                    listSortMyStash.Add(new GilesStashSort(((iThisItemValue / iNeedScore) * 1000), 1, inventoryColumn, inventoryRow, thisitem.DynamicId, bIsTwoSlot));
                }
            }

            // Loop through all stash items

            // Sort the items in the stash by their row number, lowest to highest
            listSortMyStash.Sort((p1, p2) => p1.iInventoryRow.CompareTo(p2.iInventoryRow));

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
                    GilesStashSlotBlocked[thisstashsort.iInventoryColumn, thisstashsort.iInventoryRow] = false;
                    if (thisstashsort.bIsTwoSlot)
                        GilesStashSlotBlocked[thisstashsort.iInventoryColumn, thisstashsort.iInventoryRow + 1] = false;
                    GilesBackpackSlotBlocked[(int)vFreeSlot.X, (int)vFreeSlot.Y] = true;
                    if (thisstashsort.bIsTwoSlot)
                        GilesBackpackSlotBlocked[(int)vFreeSlot.X, (int)vFreeSlot.Y + 1] = true;
                    thisstashsort.iInventoryColumn = (int)vFreeSlot.X;
                    thisstashsort.iInventoryRow = (int)vFreeSlot.Y;
                    thisstashsort.iStashOrPack = 2;
                }
                else
                {
                    ZetaDia.Me.Inventory.MoveItem(thisstashsort.iDynamicID, iPlayerDynamicID, InventorySlot.PlayerSharedStash, (int)vFreeSlot.X, (int)vFreeSlot.Y);
                    GilesStashSlotBlocked[thisstashsort.iInventoryColumn, thisstashsort.iInventoryRow] = false;
                    if (thisstashsort.bIsTwoSlot)
                        GilesStashSlotBlocked[thisstashsort.iInventoryColumn, thisstashsort.iInventoryRow + 1] = false;
                    GilesStashSlotBlocked[(int)vFreeSlot.X, (int)vFreeSlot.Y] = true;
                    if (thisstashsort.bIsTwoSlot)
                        GilesStashSlotBlocked[(int)vFreeSlot.X, (int)vFreeSlot.Y + 1] = true;
                    thisstashsort.iInventoryColumn = (int)vFreeSlot.X;
                    thisstashsort.iInventoryRow = (int)vFreeSlot.Y;
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
                    GilesStashSlotBlocked[thisstashsort.iInventoryColumn, thisstashsort.iInventoryRow] = false;
                    if (thisstashsort.bIsTwoSlot)
                        GilesStashSlotBlocked[thisstashsort.iInventoryColumn, thisstashsort.iInventoryRow + 1] = false;
                }
                else
                {
                    GilesBackpackSlotBlocked[thisstashsort.iInventoryColumn, thisstashsort.iInventoryRow] = false;
                    if (thisstashsort.bIsTwoSlot)
                        GilesBackpackSlotBlocked[thisstashsort.iInventoryColumn, thisstashsort.iInventoryRow + 1] = false;
                }
                GilesStashSlotBlocked[(int)vFreeSlot.X, (int)vFreeSlot.Y] = true;
                if (thisstashsort.bIsTwoSlot)
                    GilesStashSlotBlocked[(int)vFreeSlot.X, (int)vFreeSlot.Y + 1] = true;
                thisstashsort.iStashOrPack = 1;
                thisstashsort.iInventoryRow = (int)vFreeSlot.Y;
                thisstashsort.iInventoryColumn = (int)vFreeSlot.X;
                Thread.Sleep(150);
            }
            Log("Stash sorted!");
        }

        // Output test scores for everything in the backpack
        private static void TestScoring()
        {
            if (bTestingBackpack) return;
            bTestingBackpack = true;
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
                        GilesCachedACDItem thiscacheditem = new GilesCachedACDItem(item.InternalName, item.Name, item.Level, item.ItemQualityLevel, item.Gold, item.GameBalanceId, item.DynamicId,
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
            bTestingBackpack = false;
        }

        // Determine if we should stash this item or not based on item type and score
        private static bool ShouldWeStashThis(GilesCachedACDItem thisitem)
        {

            // Stash all unidentified items - assume we want to keep them since we are using an identifier over-ride
            if (thisitem.IsUnidentified)
            {
                if (bOutputItemScores) Log(thisitem.RealName + " [" + thisitem.InternalName + "] = (autokeep unidentified items)");
                return true;
            }

            // Now look for Misc items we might want to keep
            GilesItemType TrueItemType = DetermineItemType(thisitem.InternalName, thisitem.DBItemType, thisitem.FollowerType);
            GilesBaseItemType thisGilesBaseType = DetermineBaseType(TrueItemType);

            switch (StashRule.checkItem(thisitem, TrueItemType, thisGilesBaseType))
            {
                case Interpreter.InterpreterAction.KEEP:
                    return true;
                case Interpreter.InterpreterAction.TRASH:
                    return false;
                default:
                    break;
            }

            if (TrueItemType == GilesItemType.StaffOfHerding)
            {
                if (bOutputItemScores) Log(thisitem.RealName + " [" + thisitem.InternalName + "] [" + TrueItemType.ToString() + "] = (autokeep staff of herding)");
                return true;
            }
            if (TrueItemType == GilesItemType.CraftingMaterial)
            {
                if (bOutputItemScores) Log(thisitem.RealName + " [" + thisitem.InternalName + "] [" + TrueItemType.ToString() + "] = (autokeep craft materials)");
                return true;
            }
            if (TrueItemType == GilesItemType.CraftingPlan)
            {
                if (bOutputItemScores) Log(thisitem.RealName + " [" + thisitem.InternalName + "] [" + TrueItemType.ToString() + "] = (autokeep plans)");
                return true;
            }
            if (TrueItemType == GilesItemType.Emerald)
            {
                if (bOutputItemScores) Log(thisitem.RealName + " [" + thisitem.InternalName + "] [" + TrueItemType.ToString() + "] = (autokeep gems)");
                return true;
            }
            if (TrueItemType == GilesItemType.Amethyst)
            {
                if (bOutputItemScores) Log(thisitem.RealName + " [" + thisitem.InternalName + "] [" + TrueItemType.ToString() + "] = (autokeep gems)");
                return true;
            }
            if (TrueItemType == GilesItemType.Topaz)
            {
                if (bOutputItemScores) Log(thisitem.RealName + " [" + thisitem.InternalName + "] [" + TrueItemType.ToString() + "] = (autokeep gems)");
                return true;
            }
            if (TrueItemType == GilesItemType.Ruby)
            {
                if (bOutputItemScores) Log(thisitem.RealName + " [" + thisitem.InternalName + "] [" + TrueItemType.ToString() + "] = (autokeep gems)");
                return true;
            }
            if (TrueItemType == GilesItemType.CraftTome)
            {
                if (bOutputItemScores) Log(thisitem.RealName + " [" + thisitem.InternalName + "] [" + TrueItemType.ToString() + "] = (autokeep tomes)");
                return true;
            }
            if (TrueItemType == GilesItemType.InfernalKey)
            {
                if (bOutputItemScores) Log(thisitem.RealName + " [" + thisitem.InternalName + "] [" + TrueItemType.ToString() + "] = (autokeep infernal key)");
                return true;
            }
            if (TrueItemType == GilesItemType.HealthPotion)
            {
                if (bOutputItemScores) Log(thisitem.RealName + " [" + thisitem.InternalName + "] [" + TrueItemType.ToString() + "] = (ignoring potions)");
                return false;
            }

            

            if (thisitem.Quality >= ItemQuality.Legendary)
            {
                Log(thisitem.RealName + " [" + thisitem.InternalName + "] [" + TrueItemType.ToString() + "] = (autokeep legendaries)");
                return true;
            }

            // Ok now try to do some decent item scoring based on item types
            double iNeedScore = ScoreNeeded(TrueItemType);
            double iMyScore = ValueThisItem(thisitem, TrueItemType);
            if (bOutputItemScores) Log(thisitem.RealName + " [" + thisitem.InternalName + "] [" + TrueItemType.ToString() + "] = " + iMyScore.ToString());
            if (iMyScore >= iNeedScore) return true;

            // If we reached this point, then we found no reason to keep the item!
            return false;
        }

        // Return the score needed to keep something by the item type
        private static double ScoreNeeded(GilesItemType thisGilesItemType)
        {
            double iThisNeedScore = 0;

            // Weapons
            if (thisGilesItemType == GilesItemType.Axe || thisGilesItemType == GilesItemType.CeremonialKnife || thisGilesItemType == GilesItemType.Dagger ||
                thisGilesItemType == GilesItemType.FistWeapon || thisGilesItemType == GilesItemType.Mace || thisGilesItemType == GilesItemType.MightyWeapon ||
                thisGilesItemType == GilesItemType.Spear || thisGilesItemType == GilesItemType.Sword || thisGilesItemType == GilesItemType.Wand ||
                thisGilesItemType == GilesItemType.TwoHandDaibo || thisGilesItemType == GilesItemType.TwoHandCrossbow || thisGilesItemType == GilesItemType.TwoHandMace ||
                thisGilesItemType == GilesItemType.TwoHandMighty || thisGilesItemType == GilesItemType.TwoHandPolearm || thisGilesItemType == GilesItemType.TwoHandStaff ||
                thisGilesItemType == GilesItemType.TwoHandSword || thisGilesItemType == GilesItemType.TwoHandAxe || thisGilesItemType == GilesItemType.HandCrossbow || thisGilesItemType == GilesItemType.TwoHandBow)
                iThisNeedScore = settings.iNeedPointsToKeepWeapon;

            // Jewelry
            if (thisGilesItemType == GilesItemType.Ring || thisGilesItemType == GilesItemType.Amulet || thisGilesItemType == GilesItemType.FollowerEnchantress ||
                thisGilesItemType == GilesItemType.FollowerScoundrel || thisGilesItemType == GilesItemType.FollowerTemplar)
                iThisNeedScore = settings.iNeedPointsToKeepJewelry;

            // Armor
            if (thisGilesItemType == GilesItemType.Mojo || thisGilesItemType == GilesItemType.Orb || thisGilesItemType == GilesItemType.Quiver ||
                thisGilesItemType == GilesItemType.Shield || thisGilesItemType == GilesItemType.Belt || thisGilesItemType == GilesItemType.Boots ||
                thisGilesItemType == GilesItemType.Bracer || thisGilesItemType == GilesItemType.Chest || thisGilesItemType == GilesItemType.Cloak ||
                thisGilesItemType == GilesItemType.Gloves || thisGilesItemType == GilesItemType.Helm || thisGilesItemType == GilesItemType.Legs ||
                thisGilesItemType == GilesItemType.MightyBelt || thisGilesItemType == GilesItemType.Shoulder || thisGilesItemType == GilesItemType.SpiritStone ||
                thisGilesItemType == GilesItemType.VoodooMask || thisGilesItemType == GilesItemType.WizardHat)
                iThisNeedScore = settings.iNeedPointsToKeepArmor;
            return Math.Round(iThisNeedScore);
        }

        // The bizarre mystery function to score your lovely items!
        private static double ValueThisItem(GilesCachedACDItem thisitem, GilesItemType thisGilesItemType)
        {
            double iTotalPoints = 0;
            bool bAbandonShip = true;
            double[] iThisItemsMaxStats = new double[TOTALSTATS];
            double[] iThisItemsMaxPoints = new double[TOTALSTATS];
            GilesBaseItemType thisGilesBaseType = DetermineBaseType(thisGilesItemType);

            // One Handed Weapons 
            if (thisGilesItemType == GilesItemType.Axe || thisGilesItemType == GilesItemType.CeremonialKnife || thisGilesItemType == GilesItemType.Dagger ||
                 thisGilesItemType == GilesItemType.FistWeapon || thisGilesItemType == GilesItemType.Mace || thisGilesItemType == GilesItemType.MightyWeapon ||
                 thisGilesItemType == GilesItemType.Spear || thisGilesItemType == GilesItemType.Sword || thisGilesItemType == GilesItemType.Wand)
            {
                Array.Copy(iMaxWeaponOneHand, iThisItemsMaxStats, TOTALSTATS);
                Array.Copy(iWeaponPointsAtMax, iThisItemsMaxPoints, TOTALSTATS);
                bAbandonShip = false;
            }

            // Two Handed Weapons
            if (thisGilesItemType == GilesItemType.TwoHandAxe || thisGilesItemType == GilesItemType.TwoHandDaibo || thisGilesItemType == GilesItemType.TwoHandMace ||
                thisGilesItemType == GilesItemType.TwoHandMighty || thisGilesItemType == GilesItemType.TwoHandPolearm || thisGilesItemType == GilesItemType.TwoHandStaff ||
                thisGilesItemType == GilesItemType.TwoHandSword)
            {
                Array.Copy(iMaxWeaponTwoHand, iThisItemsMaxStats, TOTALSTATS);
                Array.Copy(iWeaponPointsAtMax, iThisItemsMaxPoints, TOTALSTATS);
                bAbandonShip = false;
            }

            // Ranged Weapons
            if (thisGilesItemType == GilesItemType.TwoHandCrossbow || thisGilesItemType == GilesItemType.TwoHandBow || thisGilesItemType == GilesItemType.HandCrossbow)
            {
                Array.Copy(iMaxWeaponRanged, iThisItemsMaxStats, TOTALSTATS);
                Array.Copy(iWeaponPointsAtMax, iThisItemsMaxPoints, TOTALSTATS);
                if (thisGilesItemType == GilesItemType.HandCrossbow)
                {
                    iThisItemsMaxStats[TOTALDPS] -= 150;
                }
                bAbandonShip = false;
            }

            // Off-handed stuff

            // Mojo, Source, Quiver
            if (thisGilesItemType == GilesItemType.Mojo || thisGilesItemType == GilesItemType.Orb || thisGilesItemType == GilesItemType.Quiver)
            {
                Array.Copy(iMaxOffHand, iThisItemsMaxStats, TOTALSTATS);
                Array.Copy(iArmorPointsAtMax, iThisItemsMaxPoints, TOTALSTATS);
                bAbandonShip = false;
            }

            // Shields
            if (thisGilesItemType == GilesItemType.Shield)
            {
                Array.Copy(iMaxShield, iThisItemsMaxStats, TOTALSTATS);
                Array.Copy(iArmorPointsAtMax, iThisItemsMaxPoints, TOTALSTATS);
                bAbandonShip = false;
            }

            // Jewelry

            // Ring
            if (thisGilesItemType == GilesItemType.Amulet)
            {
                Array.Copy(iMaxAmulet, iThisItemsMaxStats, TOTALSTATS);
                Array.Copy(iJewelryPointsAtMax, iThisItemsMaxPoints, TOTALSTATS);
                bAbandonShip = false;
            }

            // Ring
            if (thisGilesItemType == GilesItemType.Ring)
            {
                Array.Copy(iMaxRing, iThisItemsMaxStats, TOTALSTATS);
                Array.Copy(iJewelryPointsAtMax, iThisItemsMaxPoints, TOTALSTATS);
                bAbandonShip = false;
            }

            // Armor

            // Belt
            if (thisGilesItemType == GilesItemType.Belt)
            {
                Array.Copy(iMaxBelt, iThisItemsMaxStats, TOTALSTATS);
                Array.Copy(iArmorPointsAtMax, iThisItemsMaxPoints, TOTALSTATS);
                bAbandonShip = false;
            }

            // Boots
            if (thisGilesItemType == GilesItemType.Boots)
            {
                Array.Copy(iMaxBoots, iThisItemsMaxStats, TOTALSTATS);
                Array.Copy(iArmorPointsAtMax, iThisItemsMaxPoints, TOTALSTATS);
                bAbandonShip = false;
            }

            // Bracers
            if (thisGilesItemType == GilesItemType.Bracer)
            {
                Array.Copy(iMaxBracer, iThisItemsMaxStats, TOTALSTATS);
                Array.Copy(iArmorPointsAtMax, iThisItemsMaxPoints, TOTALSTATS);
                bAbandonShip = false;
            }

            // Chest
            if (thisGilesItemType == GilesItemType.Chest)
            {
                Array.Copy(iMaxChest, iThisItemsMaxStats, TOTALSTATS);
                Array.Copy(iArmorPointsAtMax, iThisItemsMaxPoints, TOTALSTATS);
                bAbandonShip = false;
            }
            if (thisGilesItemType == GilesItemType.Cloak)
            {
                Array.Copy(iMaxCloak, iThisItemsMaxStats, TOTALSTATS);
                Array.Copy(iArmorPointsAtMax, iThisItemsMaxPoints, TOTALSTATS);
                bAbandonShip = false;
            }

            // Gloves
            if (thisGilesItemType == GilesItemType.Gloves)
            {
                Array.Copy(iMaxGloves, iThisItemsMaxStats, TOTALSTATS);
                Array.Copy(iArmorPointsAtMax, iThisItemsMaxPoints, TOTALSTATS);
                bAbandonShip = false;
            }

            // Helm
            if (thisGilesItemType == GilesItemType.Helm)
            {
                Array.Copy(iMaxHelm, iThisItemsMaxStats, TOTALSTATS);
                Array.Copy(iArmorPointsAtMax, iThisItemsMaxPoints, TOTALSTATS);
                bAbandonShip = false;
            }

            // Pants
            if (thisGilesItemType == GilesItemType.Legs)
            {
                Array.Copy(iMaxPants, iThisItemsMaxStats, TOTALSTATS);
                Array.Copy(iArmorPointsAtMax, iThisItemsMaxPoints, TOTALSTATS);
                bAbandonShip = false;
            }
            if (thisGilesItemType == GilesItemType.MightyBelt)
            {
                Array.Copy(iMaxMightyBelt, iThisItemsMaxStats, TOTALSTATS);
                Array.Copy(iArmorPointsAtMax, iThisItemsMaxPoints, TOTALSTATS);
                bAbandonShip = false;
            }

            // Shoulders
            if (thisGilesItemType == GilesItemType.Shoulder)
            {
                Array.Copy(iMaxShoulders, iThisItemsMaxStats, TOTALSTATS);
                Array.Copy(iArmorPointsAtMax, iThisItemsMaxPoints, TOTALSTATS);
                bAbandonShip = false;
            }
            if (thisGilesItemType == GilesItemType.SpiritStone)
            {
                Array.Copy(iMaxSpiritStone, iThisItemsMaxStats, TOTALSTATS);
                Array.Copy(iArmorPointsAtMax, iThisItemsMaxPoints, TOTALSTATS);
                bAbandonShip = false;
            }
            if (thisGilesItemType == GilesItemType.VoodooMask)
            {
                Array.Copy(iMaxVoodooMask, iThisItemsMaxStats, TOTALSTATS);
                Array.Copy(iArmorPointsAtMax, iThisItemsMaxPoints, TOTALSTATS);
                bAbandonShip = false;
            }

            // Wizard Hat
            if (thisGilesItemType == GilesItemType.WizardHat)
            {
                Array.Copy(iMaxWizardHat, iThisItemsMaxStats, TOTALSTATS);
                Array.Copy(iArmorPointsAtMax, iThisItemsMaxPoints, TOTALSTATS);
                bAbandonShip = false;
            }

            // Follower Items
            if (thisGilesItemType == GilesItemType.FollowerEnchantress || thisGilesItemType == GilesItemType.FollowerScoundrel || thisGilesItemType == GilesItemType.FollowerTemplar)
            {
                Array.Copy(iMaxFollower, iThisItemsMaxStats, TOTALSTATS);
                Array.Copy(iJewelryPointsAtMax, iThisItemsMaxPoints, TOTALSTATS);
                bAbandonShip = false;
            }

            // Constants for convenient stat names
            double[] iHadStat = new double[TOTALSTATS] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            double[] iHadPoints = new double[TOTALSTATS] { 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
            double iSafeLifePercentage = 0;
            bool bSocketsCanReplacePrimaries = false;
            double iHighestScoringPrimary = 0;
            int iWhichPrimaryIsHighest = 0;
            double iAmountHighestScoringPrimary = 0;

            // Double safety check for unidentified items
            if (thisitem.IsUnidentified) bAbandonShip = true;

            // Make sure we got a valid item here, otherwise score it a big fat 0
            if (bAbandonShip)
            {
                if (bFullAnalysis) Log("-- Invalid Item Type or Unidentified?");
                return 0;
            }
            if (bFullAnalysis) Log("NEXT ITEM= " + thisitem.RealName + " - " + thisitem.InternalName + " [" + thisGilesBaseType.ToString() + " - " + thisGilesItemType.ToString() + "]");
            double iGlobalMultiplier = 1;
            sValueItemStatString = "";
            sJunkItemStatString = "";

            // We loop through all of the stats, in a particular order. The order *IS* important, because it pulls up primary stats first, BEFORE other stats
            for (int i = 0; i <= (TOTALSTATS - 1); i++)
            {
                double iTempStatistic = 0;

                // Now we lookup each stat on this item we are scoring, and store it in the variable "iTempStatistic" - which is used for calculations further down
                switch (i)
                {
                    case DEXTERITY: iTempStatistic = thisitem.Dexterity; break;
                    case INTELLIGENCE: iTempStatistic = thisitem.Intelligence; break;
                    case STRENGTH: iTempStatistic = thisitem.Strength; break;
                    case VITALITY: iTempStatistic = thisitem.Vitality; break;
                    case LIFEPERCENT: iTempStatistic = thisitem.LifePercent; break;
                    case LIFEONHIT: iTempStatistic = thisitem.LifeOnHit; break;
                    case LIFESTEAL: iTempStatistic = thisitem.LifeSteal; break;
                    case LIFEREGEN: iTempStatistic = thisitem.HealthPerSecond; break;
                    case MAGICFIND: iTempStatistic = thisitem.MagicFind; break;
                    case GOLDFIND: iTempStatistic = thisitem.GoldFind; break;
                    case MOVEMENTSPEED: iTempStatistic = thisitem.MovementSpeed; break;
                    case PICKUPRADIUS: iTempStatistic = thisitem.PickUpRadius; break;
                    case SOCKETS: iTempStatistic = thisitem.Sockets; break;
                    case CRITCHANCE: iTempStatistic = thisitem.CritPercent; break;
                    case CRITDAMAGE: iTempStatistic = thisitem.CritDamagePercent; break;
                    case ATTACKSPEED: iTempStatistic = thisitem.AttackSpeedPercent; break;
                    case MINDAMAGE: iTempStatistic = thisitem.MinDamage; break;
                    case MAXDAMAGE: iTempStatistic = thisitem.MaxDamage; break;
                    case BLOCKCHANCE: iTempStatistic = thisitem.BlockChance; break;
                    case THORNS: iTempStatistic = thisitem.Thorns; break;
                    case ALLRESIST: iTempStatistic = thisitem.ResistAll; break;
                    case RANDOMRESIST:
                        //intell -- sugerir
                        if (thisitem.ResistArcane > iTempStatistic) iTempStatistic = thisitem.ResistArcane;
                        if (thisitem.ResistCold > iTempStatistic) iTempStatistic = 0;
                        //thisitem.ResistCold;
                        if (thisitem.ResistFire > iTempStatistic) iTempStatistic = thisitem.ResistFire;
                        if (thisitem.ResistHoly > iTempStatistic) iTempStatistic = thisitem.ResistHoly;
                        if (thisitem.ResistLightning > iTempStatistic) iTempStatistic = 0;
                        //thisitem.ResistLightning;
                        if (thisitem.ResistPhysical > iTempStatistic) iTempStatistic = thisitem.ResistPhysical;
                        if (thisitem.ResistPoison > iTempStatistic) iTempStatistic = 0;
                        //thisitem.ResistPoison;
                        break;
                    case TOTALDPS: iTempStatistic = thisitem.WeaponDamagePerSecond; break;
                    case ARMOR: iTempStatistic = thisitem.ArmorBonus; break;
                    case MAXDISCIPLINE: iTempStatistic = thisitem.MaxDiscipline; break;
                    case MAXMANA: iTempStatistic = thisitem.MaxMana; break;
                    case ARCANECRIT: iTempStatistic = thisitem.ArcaneOnCrit; break;
                    case MANAREGEN: iTempStatistic = thisitem.ManaRegen; break;
                    case GLOBEBONUS: iTempStatistic = thisitem.GlobeBonus; break;
                }
                iHadStat[i] = iTempStatistic;
                iHadPoints[i] = 0;

                // Now we check that the current statistic in the "for" loop, actually exists on this item, and is a stat we are measuring (has >0 in the "max stats" array)
                if (iThisItemsMaxStats[i] > 0 && iTempStatistic > 0)
                {

                    // Final bonus granted is an end-of-score multiplier. 1 = 100%, so all items start off with 100%, of course!
                    double iFinalBonusGranted = 1;

                    // Temp percent is what PERCENTAGE of the *MAXIMUM POSSIBLE STAT*, this stat is at.

                    // Note that stats OVER the max will get a natural score boost, since this value will be over 1!
                    double iTempPercent = iTempStatistic / iThisItemsMaxStats[i];

                    // Now multiply the "max points" value, by that percentage, as the start/basis of the scoring for this statistic
                    double iTempPoints = iThisItemsMaxPoints[i] * iTempPercent;
                    if (bFullAnalysis) Log("--- " + StatNames[i] + ": " + iTempStatistic.ToString() + " out of " + iThisItemsMaxStats[i].ToString() + " (" + iThisItemsMaxPoints[i].ToString() + " * " + iTempPercent.ToString() + " = " + iTempPoints.ToString() + ")");

                    // Check if this statistic is over the "bonus threshold" array value for this stat - if it is, then it gets a score bonus when over a certain % of max-stat
                    if (iTempPercent > iBonusThreshold[i] && iBonusThreshold[i] > 0f)
                    {
                        iFinalBonusGranted += ((iTempPercent - iBonusThreshold[i]) * 0.9);
                    }

                    // We're going to store the life % stat here for quick-calculations against other stats. Don't edit this bit!
                    if (i == LIFEPERCENT)
                    {
                        if (iThisItemsMaxStats[LIFEPERCENT] > 0)
                        {
                            iSafeLifePercentage = (iTempStatistic / iThisItemsMaxStats[LIFEPERCENT]);
                        }
                        else
                        {
                            iSafeLifePercentage = 0;
                        }
                    }

                    // This *REMOVES* score from follower items for stats that followers don't care about
                    if (thisGilesBaseType == GilesBaseItemType.FollowerItem && (i == CRITDAMAGE || i == LIFEONHIT || i == ALLRESIST))
                        iFinalBonusGranted -= 0.9;

                    // Bonus 15% for being *at* the stat cap (ie - completely maxed out, or very very close to), but not for the socket stat (since sockets are usually 0 or 1!)
                    if (i != SOCKETS)
                    {
                        if ((iTempStatistic / iThisItemsMaxStats[i]) >= 0.99)
                            iFinalBonusGranted += 0.15;

                        // Else bonus 10% for being in final 95%
                        else if ((iTempStatistic / iThisItemsMaxStats[i]) >= 0.95)
                            iFinalBonusGranted += 0.10;
                    }

                    // Socket handling

                    // Sockets give special bonuses for certain items, depending how close to the max-socket-count it is for that item

                    // It also enables bonus scoring for stats which usually rely on a high primary stat - since a socket can make up for a lack of a high primary (you can socket a +primary stat!)
                    if (i == SOCKETS)
                    {

                        // Off-handers get less value from sockets
                        if (thisGilesBaseType == GilesBaseItemType.Offhand)
                        {
                            iFinalBonusGranted -= 0.35;
                        }

                        // Chest
                        if (thisGilesItemType == GilesItemType.Chest || thisGilesItemType == GilesItemType.Cloak)
                        {
                            if (iTempStatistic >= 2)
                            {
                                bSocketsCanReplacePrimaries = true;
                                if (iTempStatistic >= 3)
                                    iFinalBonusGranted += 0.25;
                            }
                        }

                        // Pants
                        if (thisGilesItemType == GilesItemType.Legs)
                        {
                            if (iTempStatistic >= 2)
                            {
                                bSocketsCanReplacePrimaries = true;
                                iFinalBonusGranted += 0.25;
                            }
                        }

                        // Helmets can have a bonus for a socket since it gives amazing MF/GF
                        if (iTempStatistic >= 1 && (thisGilesItemType == GilesItemType.Helm || thisGilesItemType == GilesItemType.WizardHat || thisGilesItemType == GilesItemType.VoodooMask ||
                            thisGilesItemType == GilesItemType.SpiritStone))
                        {
                            bSocketsCanReplacePrimaries = true;
                        }

                        // And rings and amulets too
                        if (iTempStatistic >= 1 && (thisGilesItemType == GilesItemType.Ring || thisGilesItemType == GilesItemType.Amulet))
                        {
                            bSocketsCanReplacePrimaries = true;
                        }
                    }

                    // Right, here's quite a long bit of code, but this is basically all about granting all sorts of bonuses based on primary stat values of all different ranges

                    // For all item types *EXCEPT* weapons
                    if (thisGilesBaseType != GilesBaseItemType.WeaponRange && thisGilesBaseType != GilesBaseItemType.WeaponOneHand && thisGilesBaseType != GilesBaseItemType.WeaponTwoHand)
                    {
                        double iSpecialBonus = 0;
                        if (i > LIFEPERCENT)
                        {

                            // Knock off points for being particularly low
                            if ((iTempStatistic / iThisItemsMaxStats[i]) < 0.2 && (iBonusThreshold[i] <= 0f || iBonusThreshold[i] >= 0.2))
                                iFinalBonusGranted -= 0.35;
                            else if ((iTempStatistic / iThisItemsMaxStats[i]) < 0.4 && (iBonusThreshold[i] <= 0f || iBonusThreshold[i] >= 0.4))
                                iFinalBonusGranted -= 0.15;

                            // Remove 80% if below minimum threshold
                            if ((iTempStatistic / iThisItemsMaxStats[i]) < iMinimumThreshold[i] && iMinimumThreshold[i] > 0f)
                                iFinalBonusGranted -= 0.8;

                            // Primary stat/vitality minimums or zero-check reductions on other stats
                            if (iStatMinimumPrimary[i] > 0)
                            {

                                // Remove 40% from all stats if there is no prime stat present or vitality/life present and this is below 90% of max
                                if (((iTempStatistic / iThisItemsMaxStats[i]) < .90) && ((iHadStat[DEXTERITY] / iThisItemsMaxStats[DEXTERITY]) < iStatMinimumPrimary[i]) &&
                                    ((iHadStat[STRENGTH] / iThisItemsMaxStats[STRENGTH]) < (iStatMinimumPrimary[i] + 0.1)) && ((iHadStat[INTELLIGENCE] / iThisItemsMaxStats[INTELLIGENCE]) < iStatMinimumPrimary[i]) &&
                                    ((iHadStat[VITALITY] / iThisItemsMaxStats[VITALITY]) < iStatMinimumPrimary[i]) && (iSafeLifePercentage < (iStatMinimumPrimary[i] * 2.5)) && !bSocketsCanReplacePrimaries)
                                {
                                    if (thisGilesItemType != GilesItemType.Ring && thisGilesItemType != GilesItemType.Amulet)
                                        iFinalBonusGranted -= 0.4;
                                    else
                                        iFinalBonusGranted -= 0.3;

                                    // And another 25% off for armor and all resist which are more useful with primaries, as long as not jewelry
                                    if ((i == ARMOR || i == ALLRESIST || i == RANDOMRESIST) && thisGilesItemType != GilesItemType.Ring && thisGilesItemType != GilesItemType.Amulet && !bSocketsCanReplacePrimaries)
                                        iFinalBonusGranted -= 0.15;
                                }
                            }
                            else
                            {

                                // Almost no primary stats or health at all
                                if (iHadStat[DEXTERITY] <= 60 && iHadStat[STRENGTH] <= 60 && iHadStat[INTELLIGENCE] <= 60 && iHadStat[VITALITY] <= 60 && iSafeLifePercentage < 0.9 && !bSocketsCanReplacePrimaries)
                                {

                                    // So 35% off for all items except jewelry which is 20% off
                                    if (thisGilesItemType != GilesItemType.Ring && thisGilesItemType != GilesItemType.Amulet)
                                    {
                                        iFinalBonusGranted -= 0.35;

                                        // And another 25% off for armor and all resist which are more useful with primaries
                                        if (i == ARMOR || i == ALLRESIST)
                                            iFinalBonusGranted -= 0.15;
                                    }
                                    else
                                    {
                                        iFinalBonusGranted -= 0.20;
                                    }
                                }
                            }
                            if (thisGilesBaseType == GilesBaseItemType.Armor || thisGilesBaseType == GilesBaseItemType.Jewelry)
                            {

                                // Grant a 50% bonus to stats if a primary is above 200 AND (vitality above 200 or life% within 90% max)
                                if ((iHadStat[DEXTERITY] > 200 || iHadStat[STRENGTH] > 200 || iHadStat[INTELLIGENCE] > 200) && (iHadStat[VITALITY] > 200 || iSafeLifePercentage > .97))
                                {
                                    if (0.5 > iSpecialBonus) iSpecialBonus = 0.5;
                                }

                                // Else grant a 40% bonus to stats if a primary is above 200
                                if (iHadStat[DEXTERITY] > 200 || iHadStat[STRENGTH] > 200 || iHadStat[INTELLIGENCE] > 200)
                                {
                                    if (0.4 > iSpecialBonus) iSpecialBonus = 0.4;
                                }

                                // Grant a 30% bonus if vitality > 200 or life percent within 90% of max
                                if (iHadStat[VITALITY] > 200 || iSafeLifePercentage > .97)
                                {
                                    if (0.3 > iSpecialBonus) iSpecialBonus = 0.3;
                                }
                            }

                            // Checks for various primary & health levels
                            if ((iHadStat[DEXTERITY] / iThisItemsMaxStats[DEXTERITY]) > .85 || (iHadStat[STRENGTH] / iThisItemsMaxStats[STRENGTH]) > .85 || (iHadStat[INTELLIGENCE] / iThisItemsMaxStats[INTELLIGENCE]) > .85)
                            {
                                if ((iHadStat[VITALITY] / iThisItemsMaxStats[VITALITY]) > .6 || iSafeLifePercentage > .90)
                                {
                                    if (0.5 > iSpecialBonus) iSpecialBonus = 0.5;
                                }
                                else if ((iHadStat[VITALITY] / iThisItemsMaxStats[VITALITY]) > .35 || iSafeLifePercentage > .85)
                                {
                                    if (0.4 > iSpecialBonus) iSpecialBonus = 0.4;
                                }
                                else
                                {
                                    if (0.2 > iSpecialBonus) iSpecialBonus = 0.2;
                                }
                            }
                            if ((iHadStat[DEXTERITY] / iThisItemsMaxStats[DEXTERITY]) > .75 || (iHadStat[STRENGTH] / iThisItemsMaxStats[STRENGTH]) > .75 || (iHadStat[INTELLIGENCE] / iThisItemsMaxStats[INTELLIGENCE]) > .75)
                            {
                                if ((iHadStat[VITALITY] / iThisItemsMaxStats[VITALITY]) > .6 || iSafeLifePercentage > .90)
                                {
                                    if (0.35 > iSpecialBonus) iSpecialBonus = 0.35;
                                }
                                else if ((iHadStat[VITALITY] / iThisItemsMaxStats[VITALITY]) > .35 || iSafeLifePercentage > .85)
                                {
                                    if (0.30 > iSpecialBonus) iSpecialBonus = 0.30;
                                }
                                else
                                {
                                    if (0.15 > iSpecialBonus) iSpecialBonus = 0.15;
                                }
                            }
                            if ((iHadStat[DEXTERITY] / iThisItemsMaxStats[DEXTERITY]) > .65 || (iHadStat[STRENGTH] / iThisItemsMaxStats[STRENGTH]) > .65 || (iHadStat[INTELLIGENCE] / iThisItemsMaxStats[INTELLIGENCE]) > .65)
                            {
                                if ((iHadStat[VITALITY] / iThisItemsMaxStats[VITALITY]) > .6 || iSafeLifePercentage > .90)
                                {
                                    if (0.26 > iSpecialBonus) iSpecialBonus = 0.26;
                                }
                                else if ((iHadStat[VITALITY] / iThisItemsMaxStats[VITALITY]) > .35 || iSafeLifePercentage > .85)
                                {
                                    if (0.22 > iSpecialBonus) iSpecialBonus = 0.22;
                                }
                                else
                                {
                                    if (0.11 > iSpecialBonus) iSpecialBonus = 0.11;
                                }
                            }
                            if ((iHadStat[DEXTERITY] / iThisItemsMaxStats[DEXTERITY]) > .55 || (iHadStat[STRENGTH] / iThisItemsMaxStats[STRENGTH]) > .55 || (iHadStat[INTELLIGENCE] / iThisItemsMaxStats[INTELLIGENCE]) > .55)
                            {
                                if ((iHadStat[VITALITY] / iThisItemsMaxStats[VITALITY]) > .6 || iSafeLifePercentage > .90)
                                {
                                    if (0.18 > iSpecialBonus) iSpecialBonus = 0.18;
                                }
                                else if ((iHadStat[VITALITY] / iThisItemsMaxStats[VITALITY]) > .35 || iSafeLifePercentage > .85)
                                {
                                    if (0.14 > iSpecialBonus) iSpecialBonus = 0.14;
                                }
                                else
                                {
                                    if (0.08 > iSpecialBonus) iSpecialBonus = 0.08;
                                }
                            }
                            if ((iHadStat[DEXTERITY] / iThisItemsMaxStats[DEXTERITY]) > .5 || (iHadStat[STRENGTH] / iThisItemsMaxStats[STRENGTH]) > .5 || (iHadStat[INTELLIGENCE] / iThisItemsMaxStats[INTELLIGENCE]) > .5)
                            {
                                if ((iHadStat[VITALITY] / iThisItemsMaxStats[VITALITY]) > .6 || iSafeLifePercentage > .90)
                                {
                                    if (0.12 > iSpecialBonus) iSpecialBonus = 0.12;
                                }
                                else if ((iHadStat[VITALITY] / iThisItemsMaxStats[VITALITY]) > .35 || iSafeLifePercentage > .85)
                                {
                                    if (0.05 > iSpecialBonus) iSpecialBonus = 0.05;
                                }
                                else
                                {
                                    if (0.03 > iSpecialBonus) iSpecialBonus = 0.03;
                                }
                            }
                            if (thisGilesItemType == GilesItemType.Ring || thisGilesItemType == GilesItemType.Amulet)
                            {
                                if ((iHadStat[DEXTERITY] / iThisItemsMaxStats[DEXTERITY]) > .4 || (iHadStat[STRENGTH] / iThisItemsMaxStats[STRENGTH]) > .4 || (iHadStat[INTELLIGENCE] / iThisItemsMaxStats[INTELLIGENCE]) > .4)
                                {
                                    if ((iHadStat[VITALITY] / iThisItemsMaxStats[VITALITY]) > .6 || iSafeLifePercentage > .90)
                                    {
                                        if (0.10 > iSpecialBonus) iSpecialBonus = 0.10;
                                    }
                                    else if ((iHadStat[VITALITY] / iThisItemsMaxStats[VITALITY]) > .35 || iSafeLifePercentage > .85)
                                    {
                                        if (0.08 > iSpecialBonus) iSpecialBonus = 0.08;
                                    }
                                    else
                                    {
                                        if (0.05 > iSpecialBonus) iSpecialBonus = 0.05;
                                    }
                                }
                            }
                            if ((iHadStat[VITALITY] / iThisItemsMaxStats[VITALITY]) > .8 || iSafeLifePercentage > .98)
                            {
                                if (0.20 > iSpecialBonus) iSpecialBonus = 0.20;
                            }
                            if ((iHadStat[VITALITY] / iThisItemsMaxStats[VITALITY]) > .7 || iSafeLifePercentage > .95)
                            {
                                if (0.16 > iSpecialBonus) iSpecialBonus = 0.16;
                            }
                            if ((iHadStat[VITALITY] / iThisItemsMaxStats[VITALITY]) > .6 || iSafeLifePercentage > .92)
                            {
                                if (0.12 > iSpecialBonus) iSpecialBonus = 0.12;
                            }
                            if ((iHadStat[VITALITY] / iThisItemsMaxStats[VITALITY]) > .55 || iSafeLifePercentage > .89)
                            {
                                if (0.07 > iSpecialBonus) iSpecialBonus = 0.07;
                            }
                            else if ((iHadStat[VITALITY] / iThisItemsMaxStats[VITALITY]) > .5 || iSafeLifePercentage > .87)
                            {
                                if (0.05 > iSpecialBonus) iSpecialBonus = 0.05;
                            }
                            else if ((iHadStat[VITALITY] / iThisItemsMaxStats[VITALITY]) > .45 || iSafeLifePercentage > .86)
                            {
                                if (0.02 > iSpecialBonus) iSpecialBonus = 0.02;
                            }
                        }

                        // This stat is one after life percent stat

                        // Shields get less of a special bonus from high prime stats
                        if (thisGilesItemType == GilesItemType.Shield)
                            iSpecialBonus *= 0.7;
                        if (bFullAnalysis) Log("------- special bonus =" + iSpecialBonus.ToString());
                        iFinalBonusGranted += iSpecialBonus;
                    }

                    // NOT A WEAPON!?
                    //intell -- sugerir
                    if (i == LIFESTEAL && thisGilesItemType == GilesItemType.MightyBelt)
                        iFinalBonusGranted += 0.3;

                    // Knock off points for being particularly low
                    if ((iTempStatistic / iThisItemsMaxStats[i]) < iMinimumThreshold[i] && iMinimumThreshold[i] > 0f)
                        iFinalBonusGranted -= 0.35;

                    // Grant a 20% bonus to vitality or Life%, for being paired with any prime stat above minimum threshold +.1
                    if (((i == VITALITY && (iTempStatistic / iThisItemsMaxStats[VITALITY]) > iMinimumThreshold[VITALITY]) ||
                          i == LIFEPERCENT && (iTempStatistic / iThisItemsMaxStats[LIFEPERCENT]) > iMinimumThreshold[LIFEPERCENT]) &&
                        ((iHadStat[DEXTERITY] / iThisItemsMaxStats[DEXTERITY]) > (iMinimumThreshold[DEXTERITY] + 0.1) || (iHadStat[STRENGTH] / iThisItemsMaxStats[STRENGTH]) > (iMinimumThreshold[STRENGTH] + 0.1) ||
                         (iHadStat[INTELLIGENCE] / iThisItemsMaxStats[INTELLIGENCE]) > (iMinimumThreshold[INTELLIGENCE] + 0.1)))
                        iFinalBonusGranted += 0.2;

                    // Blue item point reduction for non-weapons
                    if (thisitem.Quality < ItemQuality.Rare4 && (thisGilesBaseType == GilesBaseItemType.Armor || thisGilesBaseType == GilesBaseItemType.Offhand ||
                        thisGilesBaseType == GilesBaseItemType.Jewelry || thisGilesBaseType == GilesBaseItemType.FollowerItem) && ((iTempStatistic / iThisItemsMaxStats[i]) < 0.88))
                        iFinalBonusGranted -= 0.9;

                    // Special all-resist bonuses
                    if (i == ALLRESIST)
                    {

                        // Shields with < 60% max all resist, lost some all resist score
                        if (thisGilesItemType == GilesItemType.Shield && (iTempStatistic / iThisItemsMaxStats[i]) <= 0.6)
                            iFinalBonusGranted -= 0.30;
                        double iSpecialBonus = 0;

                        // All resist gets a special bonus if paired with good strength and some vitality
                        if ((iHadStat[STRENGTH] / iThisItemsMaxStats[STRENGTH]) > 0.7 && (iHadStat[VITALITY] / iThisItemsMaxStats[VITALITY]) > 0.3)
                            if (0.45 > iSpecialBonus) iSpecialBonus = 0.45;

                        // All resist gets a smaller special bonus if paired with good dexterity and some vitality
                        if ((iHadStat[DEXTERITY] / iThisItemsMaxStats[DEXTERITY]) > 0.7 && (iHadStat[VITALITY] / iThisItemsMaxStats[VITALITY]) > 0.3)
                            if (0.35 > iSpecialBonus) iSpecialBonus = 0.35;

                        // All resist gets a slight special bonus if paired with good intelligence and some vitality
                        if ((iHadStat[INTELLIGENCE] / iThisItemsMaxStats[INTELLIGENCE]) > 0.7 && (iHadStat[INTELLIGENCE] / iThisItemsMaxStats[INTELLIGENCE]) > 0.3)
                            if (0.25 > iSpecialBonus) iSpecialBonus = 0.25;

                        // Smaller bonuses for smaller stats

                        // All resist gets a special bonus if paired with good strength and some vitality
                        if ((iHadStat[STRENGTH] / iThisItemsMaxStats[STRENGTH]) > 0.55 && (iHadStat[VITALITY] / iThisItemsMaxStats[VITALITY]) > 0.3)
                            if (0.45 > iSpecialBonus) iSpecialBonus = 0.20;

                        // All resist gets a smaller special bonus if paired with good dexterity and some vitality
                        if ((iHadStat[DEXTERITY] / iThisItemsMaxStats[DEXTERITY]) > 0.55 && (iHadStat[VITALITY] / iThisItemsMaxStats[VITALITY]) > 0.3)
                            if (0.35 > iSpecialBonus) iSpecialBonus = 0.15;

                        // All resist gets a slight special bonus if paired with good intelligence and some vitality
                        if ((iHadStat[INTELLIGENCE] / iThisItemsMaxStats[INTELLIGENCE]) > 0.55 && (iHadStat[INTELLIGENCE] / iThisItemsMaxStats[INTELLIGENCE]) > 0.3)
                            if (0.25 > iSpecialBonus) iSpecialBonus = 0.10;

                        // This stat is one after life percent stat
                        iFinalBonusGranted += iSpecialBonus;

                        // Global bonus to everything
                        if ((iThisItemsMaxStats[i] - iTempStatistic) < 10.2f)
                            iGlobalMultiplier += 0.05;
                    }

                    // All resist special bonuses
                    if (thisGilesItemType != GilesItemType.Ring && thisGilesItemType != GilesItemType.Amulet)
                    {

                        // Shields get 10% less on everything
                        if (thisGilesItemType == GilesItemType.Shield)
                            iFinalBonusGranted -= 0.10;

                        // Prime stat gets a 20% bonus if 50 from max possible
                        if ((i == DEXTERITY || i == STRENGTH || i == INTELLIGENCE || i == VITALITY) && (iThisItemsMaxStats[i] - iTempStatistic) < 50.5f)
                            iFinalBonusGranted += 0.25;

                        // Reduce a prime stat by 75% if less than 100 *OR* less than 50% max
                        if ((i == DEXTERITY || i == STRENGTH || i == INTELLIGENCE) && (iTempStatistic < 100 || ((iTempStatistic / iThisItemsMaxStats[i]) < 0.5)))
                            iFinalBonusGranted -= 0.75;

                        // Reduce a vitality/life% stat by 60% if less than 80 vitality/less than 60% max possible life%
                        if ((i == VITALITY && iTempStatistic < 80) || (i == LIFEPERCENT && ((iTempStatistic / iThisItemsMaxStats[LIFEPERCENT]) < 0.6)))
                            iFinalBonusGranted -= 0.6;

                        // Grant 10% to any 4 main stat above 200
                        if ((i == DEXTERITY || i == STRENGTH || i == INTELLIGENCE || i == VITALITY) && iTempStatistic > 200)
                            iFinalBonusGranted += 0.1;

                        // Special stat handling stuff for non-jewelry types

                        // Within 2 block chance
                        if (i == BLOCKCHANCE && (iThisItemsMaxStats[i] - iTempStatistic) < 2.3f)
                            iFinalBonusGranted += 1;

                        // Within final 5 gold find
                        if (i == GOLDFIND && (iThisItemsMaxStats[i] - iTempStatistic) < 5.3f)
                        {
                            iFinalBonusGranted += 0.04;

                            // Even bigger bonus if got prime stat & vit
                            if (((iHadStat[DEXTERITY] / iThisItemsMaxStats[DEXTERITY]) > iMinimumThreshold[DEXTERITY] || (iHadStat[STRENGTH] / iThisItemsMaxStats[STRENGTH]) > iMinimumThreshold[STRENGTH] ||
                                (iHadStat[INTELLIGENCE] / iThisItemsMaxStats[INTELLIGENCE]) > iMinimumThreshold[INTELLIGENCE]) && (iHadStat[VITALITY] / iThisItemsMaxStats[VITALITY]) > iMinimumThreshold[VITALITY])
                                iFinalBonusGranted += 0.02;
                        }

                        // Within final 3 gold find
                        if (i == GOLDFIND && (iThisItemsMaxStats[i] - iTempStatistic) < 3.3f)
                        {
                            iFinalBonusGranted += 0.04;
                        }

                        // Within final 2 gold find
                        if (i == GOLDFIND && (iThisItemsMaxStats[i] - iTempStatistic) < 2.3f)
                        {
                            iFinalBonusGranted += 0.05;
                        }

                        // Within final 3 magic find
                        if (i == MAGICFIND && (iThisItemsMaxStats[i] - iTempStatistic) < 3.3f)
                            iFinalBonusGranted += 0.08;

                        // Within final 2 magic find
                        if (i == MAGICFIND && (iThisItemsMaxStats[i] - iTempStatistic) < 2.3f)
                        {
                            iFinalBonusGranted += 0.04;

                            // Even bigger bonus if got prime stat & vit
                            if (((iHadStat[DEXTERITY] / iThisItemsMaxStats[DEXTERITY]) > iMinimumThreshold[DEXTERITY] || (iHadStat[STRENGTH] / iThisItemsMaxStats[STRENGTH]) > iMinimumThreshold[STRENGTH] ||
                                (iHadStat[INTELLIGENCE] / iThisItemsMaxStats[INTELLIGENCE]) > iMinimumThreshold[INTELLIGENCE]) && (iHadStat[VITALITY] / iThisItemsMaxStats[VITALITY]) > iMinimumThreshold[VITALITY])
                                iFinalBonusGranted += 0.03;
                        }

                        // Within final magic find
                        if (i == MAGICFIND && (iThisItemsMaxStats[i] - iTempStatistic) < 1.3f)
                        {
                            iFinalBonusGranted += 0.05;
                        }

                        // Within final 10 all resist
                        if (i == ALLRESIST && (iThisItemsMaxStats[i] - iTempStatistic) < 10.2f)
                        {
                            iFinalBonusGranted += 0.05;

                            // Even bigger bonus if got prime stat & vit
                            if (((iHadStat[DEXTERITY] / iThisItemsMaxStats[DEXTERITY]) > iMinimumThreshold[DEXTERITY] || (iHadStat[STRENGTH] / iThisItemsMaxStats[STRENGTH]) > iMinimumThreshold[STRENGTH] ||
                                (iHadStat[INTELLIGENCE] / iThisItemsMaxStats[INTELLIGENCE]) > iMinimumThreshold[INTELLIGENCE]) && (iHadStat[VITALITY] / iThisItemsMaxStats[VITALITY]) > iMinimumThreshold[VITALITY])
                                iFinalBonusGranted += 0.20;
                        }

                        // Within final 50 armor
                        if (i == ARMOR && (iThisItemsMaxStats[i] - iTempStatistic) < 50.2f)
                        {
                            iFinalBonusGranted += 0.10;
                            if ((iHadStat[STRENGTH] / iThisItemsMaxStats[STRENGTH]) > iMinimumThreshold[STRENGTH])
                                iFinalBonusGranted += 0.10;
                        }

                        // Within final 15 armor
                        if (i == ARMOR && (iThisItemsMaxStats[i] - iTempStatistic) < 15.2f)
                            iFinalBonusGranted += 0.15;

                        // Within final 5 critical hit damage
                        if (i == CRITDAMAGE && (iThisItemsMaxStats[i] - iTempStatistic) < 5.2f)
                            iFinalBonusGranted += 0.25;

                        // More than 2.5 crit chance out
                        if (i == CRITCHANCE && (iThisItemsMaxStats[i] - iTempStatistic) > 2.45f)
                            iFinalBonusGranted -= 0.35;

                        // More than 20 crit damage out
                        if (i == CRITDAMAGE && (iThisItemsMaxStats[i] - iTempStatistic) > 19.95f)
                            iFinalBonusGranted -= 0.35;

                        // More than 2 attack speed out
                        if (i == ATTACKSPEED && (iThisItemsMaxStats[i] - iTempStatistic) > 1.95f)
                            iFinalBonusGranted -= 0.35;

                        // More than 2 move speed
                        if (i == MOVEMENTSPEED && (iThisItemsMaxStats[i] - iTempStatistic) > 1.95f)
                            iFinalBonusGranted -= 0.35;

                        // More than 5 gold find out
                        if (i == GOLDFIND && (iThisItemsMaxStats[i] - iTempStatistic) > 5.2f)
                            iFinalBonusGranted -= 0.40;

                        // More than 8 gold find out
                        if (i == GOLDFIND && (iThisItemsMaxStats[i] - iTempStatistic) > 8.2f)
                            iFinalBonusGranted -= 0.1;

                        // More than 5 magic find out
                        if (i == MAGICFIND && (iThisItemsMaxStats[i] - iTempStatistic) > 5.2f)
                            iFinalBonusGranted -= 0.40;

                        // More than 7 magic find out
                        if (i == MAGICFIND && (iThisItemsMaxStats[i] - iTempStatistic) > 7.2f)
                            iFinalBonusGranted -= 0.1;

                        // More than 20 all resist out
                        if (i == ALLRESIST && (iThisItemsMaxStats[i] - iTempStatistic) > 20.2f)
                            iFinalBonusGranted -= 0.50;

                        // More than 30 all resist out
                        if (i == ALLRESIST && (iThisItemsMaxStats[i] - iTempStatistic) > 30.2f)
                            iFinalBonusGranted -= 0.20;
                    }

                    // And now for jewelry checks...
                    else
                    {

                        // Global bonus to everything if jewelry has an all resist above 50%
                        if (i == ALLRESIST && (iTempStatistic / iThisItemsMaxStats[i]) > 0.5)
                            iGlobalMultiplier += 0.08;

                        // Within final 10 all resist
                        if (i == ALLRESIST && (iThisItemsMaxStats[i] - iTempStatistic) < 10.2f)
                            iFinalBonusGranted += 0.10;

                        // Within final 5 critical hit damage
                        if (i == CRITDAMAGE && (iThisItemsMaxStats[i] - iTempStatistic) < 5.2f)
                            iFinalBonusGranted += 0.25;

                        // Within 3 block chance
                        if (i == BLOCKCHANCE && (iThisItemsMaxStats[i] - iTempStatistic) < 3.3f)
                            iFinalBonusGranted += 0.15;

                        // Reduce a prime stat by 60% if less than 60
                        if ((i == DEXTERITY || i == STRENGTH || i == INTELLIGENCE) && (iTempStatistic < 60 || ((iTempStatistic / iThisItemsMaxStats[i]) < 0.3)))
                            iFinalBonusGranted -= 0.6;

                        // Reduce a vitality/life% stat by 50% if less than 50 vitality/less than 40% max possible life%
                        if ((i == VITALITY && iTempStatistic < 50) || (i == LIFEPERCENT && ((iTempStatistic / iThisItemsMaxStats[LIFEPERCENT]) < 0.4)))
                            iFinalBonusGranted -= 0.5;

                        // Grant 20% to any 4 main stat above 150
                        if ((i == DEXTERITY || i == STRENGTH || i == INTELLIGENCE || i == VITALITY) && iTempStatistic > 150)
                            iFinalBonusGranted += 0.2;

                        // Special stat handling stuff for jewelry
                        if (thisGilesItemType == GilesItemType.Ring)
                        {

                            // Prime stat gets a 25% bonus if 30 from max possible
                            if ((i == DEXTERITY || i == STRENGTH || i == INTELLIGENCE || i == VITALITY) && (iThisItemsMaxStats[i] - iTempStatistic) < 30.5f)
                                iFinalBonusGranted += 0.25;

                            // Within final 5 magic find
                            if (i == MAGICFIND && (iThisItemsMaxStats[i] - iTempStatistic) < 5.2f)
                                iFinalBonusGranted += 0.4;

                            // Within final 5 gold find
                            if (i == GOLDFIND && (iThisItemsMaxStats[i] - iTempStatistic) < 5.2f)
                                iFinalBonusGranted += 0.35;

                            // Within final 45 life on hit
                            if (i == LIFEONHIT && (iThisItemsMaxStats[i] - iTempStatistic) < 45.2f)
                                iFinalBonusGranted += 1.2;

                        }
                        else
                        {

                            // Prime stat gets a 25% bonus if 60 from max possible
                            if ((i == DEXTERITY || i == STRENGTH || i == INTELLIGENCE || i == VITALITY) && (iThisItemsMaxStats[i] - iTempStatistic) < 60.5f)
                                iFinalBonusGranted += 0.25;

                            // Within final 10 magic find
                            if (i == MAGICFIND && (iThisItemsMaxStats[i] - iTempStatistic) < 10.2f)
                                iFinalBonusGranted += 0.4;

                            // Within final 10 gold find
                            if (i == GOLDFIND && (iThisItemsMaxStats[i] - iTempStatistic) < 10.2f)
                                iFinalBonusGranted += 0.35;

                            // Within final 40 life on hit
                            if (i == LIFEONHIT && (iThisItemsMaxStats[i] - iTempStatistic) < 40.2f)
                                iFinalBonusGranted += 1.2;

                        }

                        // Within final 50 armor
                        if (i == ARMOR && (iThisItemsMaxStats[i] - iTempStatistic) < 50.2f)
                        {
                            iFinalBonusGranted += 0.30;
                            if ((iHadStat[STRENGTH] / iThisItemsMaxStats[STRENGTH]) > iMinimumThreshold[STRENGTH])
                                iFinalBonusGranted += 0.30;
                        }

                        // Within final 15 armor
                        if (i == ARMOR && (iThisItemsMaxStats[i] - iTempStatistic) < 15.2f)
                            iFinalBonusGranted += 0.20;

                        // More than 2.5 crit chance out
                        if (i == CRITCHANCE && (iThisItemsMaxStats[i] - iTempStatistic) > 5.55f)
                            iFinalBonusGranted -= 0.20;

                        // More than 20 crit damage out
                        if (i == CRITDAMAGE && (iThisItemsMaxStats[i] - iTempStatistic) > 19.95f)
                            iFinalBonusGranted -= 0.20;

                        // More than 2 attack speed out
                        if (i == ATTACKSPEED && (iThisItemsMaxStats[i] - iTempStatistic) > 1.95f)
                            iFinalBonusGranted -= 0.20;

                        // More than 15 gold find out
                        if (i == GOLDFIND && (iThisItemsMaxStats[i] - iTempStatistic) > 15.2f)
                            iFinalBonusGranted -= 0.1;

                        // More than 15 magic find out
                        if (i == MAGICFIND && (iThisItemsMaxStats[i] - iTempStatistic) > 15.2f)
                            iFinalBonusGranted -= 0.1;

                        // More than 30 all resist out
                        if (i == ALLRESIST && (iThisItemsMaxStats[i] - iTempStatistic) > 20.2f)
                            iFinalBonusGranted -= 0.1;

                        // More than 40 all resist out
                        if (i == ALLRESIST && (iThisItemsMaxStats[i] - iTempStatistic) > 30.2f)
                            iFinalBonusGranted -= 0.1;
                    }

                    // All the "set to 0" checks now

                    // Disable specific primary stat scoring for certain class-specific item types
                    if ((thisGilesItemType == GilesItemType.VoodooMask || thisGilesItemType == GilesItemType.WizardHat || thisGilesItemType == GilesItemType.Wand ||
                        thisGilesItemType == GilesItemType.CeremonialKnife || thisGilesItemType == GilesItemType.Mojo || thisGilesItemType == GilesItemType.Orb)
                        && (i == STRENGTH || i == DEXTERITY))
                        iFinalBonusGranted = 0;
                    if ((thisGilesItemType == GilesItemType.Quiver || thisGilesItemType == GilesItemType.HandCrossbow || thisGilesItemType == GilesItemType.Cloak ||
                        thisGilesItemType == GilesItemType.SpiritStone || thisGilesItemType == GilesItemType.TwoHandDaibo || thisGilesItemType == GilesItemType.FistWeapon)
                        && (i == STRENGTH || i == INTELLIGENCE))
                        iFinalBonusGranted = 0;
                    if ((thisGilesItemType == GilesItemType.MightyBelt || thisGilesItemType == GilesItemType.MightyWeapon || thisGilesItemType == GilesItemType.TwoHandMighty)
                        && (i == DEXTERITY || i == INTELLIGENCE))
                        iFinalBonusGranted = 0;

                    // Remove unwanted follower stats for specific follower types
                    if (thisGilesItemType == GilesItemType.FollowerEnchantress && (i == STRENGTH || i == DEXTERITY))
                        iFinalBonusGranted = 0;
                    if (thisGilesItemType == GilesItemType.FollowerEnchantress && (i == INTELLIGENCE || i == VITALITY))
                        iFinalBonusGranted -= 0.4;
                    if (thisGilesItemType == GilesItemType.FollowerScoundrel && (i == STRENGTH || i == INTELLIGENCE))
                        iFinalBonusGranted = 0;
                    if (thisGilesItemType == GilesItemType.FollowerScoundrel && (i == DEXTERITY || i == VITALITY))
                        iFinalBonusGranted -= 0.4;
                    if (thisGilesItemType == GilesItemType.FollowerTemplar && (i == DEXTERITY || i == INTELLIGENCE))
                        iFinalBonusGranted = 0;
                    if (thisGilesItemType == GilesItemType.FollowerTemplar && (i == STRENGTH || i == VITALITY))
                        iFinalBonusGranted -= 0.4;

                    // Attack speed is always on a quiver so forget it
                    if ((thisGilesItemType == GilesItemType.Quiver) && (i == ATTACKSPEED))
                        iFinalBonusGranted = 0;

                    // Single resists worth nothing without all-resist
                    if (i == RANDOMRESIST && (iHadStat[ALLRESIST] / iThisItemsMaxStats[ALLRESIST]) < iMinimumThreshold[ALLRESIST])
                        iFinalBonusGranted = 0;
                    if (iFinalBonusGranted < 0)
                        iFinalBonusGranted = 0;

                    // Grant the final bonus total
                    iTempPoints *= iFinalBonusGranted;

                    // If it's a primary stat, log the highest scoring primary... else add these points to the running total
                    if (i == DEXTERITY || i == STRENGTH || i == INTELLIGENCE)
                    {
                        if (bFullAnalysis) Log("---- +" + iTempPoints.ToString() + " (*" + iFinalBonusGranted.ToString() + " multiplier) [MUST BE MAX STAT SCORE TO COUNT]");
                        if (iTempPoints > iHighestScoringPrimary)
                        {
                            iHighestScoringPrimary = iTempPoints;
                            iWhichPrimaryIsHighest = i;
                            iAmountHighestScoringPrimary = iTempStatistic;
                        }
                    }
                    else
                    {
                        if (bFullAnalysis) Log("---- +" + iTempPoints.ToString() + " score (*" + iFinalBonusGranted.ToString() + " multiplier)");
                        iTotalPoints += iTempPoints;
                    }
                    iHadPoints[i] = iTempPoints;

                    // For item logs
                    if (i != DEXTERITY && i != STRENGTH && i != INTELLIGENCE)
                    {
                        if (sValueItemStatString != "")
                            sValueItemStatString += ". ";
                        sValueItemStatString += StatNames[i] + "=" + Math.Round(iTempStatistic).ToString();
                        if (sJunkItemStatString != "")
                            sJunkItemStatString += ". ";
                        sJunkItemStatString += StatNames[i] + "=" + Math.Round(iTempStatistic).ToString();
                    }
                }
            }

            // End of main 0-TOTALSTATS stat loop
            int iTotalRequirements;

            // Now add on one of the three primary stat scores, whichever was higher
            if (iHighestScoringPrimary > 0)
            {

                // Give a 30% of primary-stat-score-possible bonus to the primary scoring if paired with a good amount of life % or vitality
                if ((iHadStat[VITALITY] / iThisItemsMaxStats[VITALITY] > (iMinimumThreshold[VITALITY] + 0.1)) || iSafeLifePercentage > 0.85)
                    iHighestScoringPrimary += iThisItemsMaxPoints[iWhichPrimaryIsHighest] * 0.3;

                // Reduce a primary a little if there is no vitality or life
                if ((iHadStat[VITALITY] < 40) || iSafeLifePercentage < 0.7)
                    iHighestScoringPrimary *= 0.8;
                iTotalPoints += iHighestScoringPrimary;
                sValueItemStatString = StatNames[iWhichPrimaryIsHighest] + "=" + Math.Round(iAmountHighestScoringPrimary).ToString() + ". " + sValueItemStatString;
                sJunkItemStatString = StatNames[iWhichPrimaryIsHighest] + "=" + Math.Round(iAmountHighestScoringPrimary).ToString() + ". " + sJunkItemStatString;
            }
            if (bFullAnalysis) Log("--- +" + iTotalPoints.ToString() + " total score pre-special reductions. (GM=" + iGlobalMultiplier.ToString() + ")");

            // Global multiplier
            iTotalPoints *= iGlobalMultiplier;

            // 2 handed weapons and ranged weapons lose a large score for low DPS
            if (thisGilesBaseType == GilesBaseItemType.WeaponRange || thisGilesBaseType == GilesBaseItemType.WeaponTwoHand)
            {
                if ((iHadStat[TOTALDPS] / iThisItemsMaxStats[TOTALDPS]) <= 0.7)
                    iTotalPoints *= 0.75;
            }

            // Weapons should get a nice 15% bonus score for having very high primaries
            if (thisGilesBaseType == GilesBaseItemType.WeaponRange || thisGilesBaseType == GilesBaseItemType.WeaponOneHand || thisGilesBaseType == GilesBaseItemType.WeaponTwoHand)
            {
                if (iHighestScoringPrimary > 0 && (iHighestScoringPrimary >= iThisItemsMaxPoints[iWhichPrimaryIsHighest] * 0.9))
                {
                    iTotalPoints *= 1.15;
                }

                // And an extra 15% for a very high vitality
                if (iHadStat[VITALITY] > 0 && (iHadStat[VITALITY] >= iThisItemsMaxPoints[VITALITY] * 0.9))
                {
                    iTotalPoints *= 1.15;
                }

                // And an extra 15% for a very high life-on-hit
                if (iHadStat[LIFEONHIT] > 0 && (iHadStat[LIFEONHIT] >= iThisItemsMaxPoints[LIFEONHIT] * 0.9))
                {
                    iTotalPoints *= 1.15;
                }
            }

            // Shields 
            if (thisGilesItemType == GilesItemType.Shield)
            {

                // Strength/Dex based shield calculations
                if (iWhichPrimaryIsHighest == STRENGTH || iWhichPrimaryIsHighest == DEXTERITY)
                {
                    if (iHadStat[BLOCKCHANCE] < 20)
                    {
                        iTotalPoints *= 0.7;
                    }
                    else if (iHadStat[BLOCKCHANCE] < 25)
                    {
                        iTotalPoints *= 0.9;
                    }
                }

                // Intelligence/no primary based shields
                else
                {
                    if (iHadStat[BLOCKCHANCE] < 28)
                        iTotalPoints -= iHadPoints[BLOCKCHANCE];
                }
            }

            // Quivers
            if (thisGilesItemType == GilesItemType.Quiver)
            {
                iTotalRequirements = 0;
                if (iHadStat[DEXTERITY] >= 100)
                    iTotalRequirements++;
                else
                    iTotalRequirements -= 3;
                if (iHadStat[DEXTERITY] >= 160)
                    iTotalRequirements++;
                if (iHadStat[DEXTERITY] >= 250)
                    iTotalRequirements++;
                if (iHadStat[ATTACKSPEED] < 14)
                    iTotalRequirements -= 2;
                if (iHadStat[VITALITY] >= 70 || iSafeLifePercentage >= 0.85)
                    iTotalRequirements++;
                else
                    iTotalRequirements--;
                if (iHadStat[VITALITY] >= 260)
                    iTotalRequirements++;
                if (iHadStat[MAXDISCIPLINE] >= 8)
                    iTotalRequirements++;
                if (iHadStat[MAXDISCIPLINE] >= 10)
                    iTotalRequirements++;
                if (iHadStat[SOCKETS] >= 1)
                    iTotalRequirements++;
                if (iHadStat[CRITCHANCE] >= 6)
                    iTotalRequirements++;
                if (iHadStat[CRITCHANCE] >= 8)
                    iTotalRequirements++;
                if (iHadStat[LIFEPERCENT] >= 8)
                    iTotalRequirements++;
                if (iHadStat[MAGICFIND] >= 18)
                    iTotalRequirements++;
                if (iTotalRequirements < 4)
                    iTotalPoints *= 0.4;
                else if (iTotalRequirements < 5)
                    iTotalPoints *= 0.5;
                if (iTotalRequirements >= 7)
                    iTotalPoints *= 1.2;
            }

            // Mojos and Sources
            if (thisGilesItemType == GilesItemType.Orb || thisGilesItemType == GilesItemType.Mojo)
            {
                iTotalRequirements = 0;
                if (iHadStat[INTELLIGENCE] >= 100)
                    iTotalRequirements++;
                else if (iHadStat[INTELLIGENCE] < 80)
                    iTotalRequirements -= 3;
                else if (iHadStat[INTELLIGENCE] < 100)
                    iTotalRequirements -= 1;
                if (iHadStat[INTELLIGENCE] >= 160)
                    iTotalRequirements++;
                if (iHadStat[MAXDAMAGE] >= 250)
                    iTotalRequirements++;
                else
                    iTotalRequirements -= 2;
                if (iHadStat[MAXDAMAGE] >= 340)
                    iTotalRequirements++;
                if (iHadStat[MINDAMAGE] >= 50)
                    iTotalRequirements++;
                else
                    iTotalRequirements--;
                if (iHadStat[MINDAMAGE] >= 85)
                    iTotalRequirements++;
                if (iHadStat[VITALITY] >= 70)
                    iTotalRequirements++;
                if (iHadStat[SOCKETS] >= 1)
                    iTotalRequirements++;
                if (iHadStat[CRITCHANCE] >= 6)
                    iTotalRequirements++;
                if (iHadStat[CRITCHANCE] >= 8)
                    iTotalRequirements++;
                if (iHadStat[LIFEPERCENT] >= 8)
                    iTotalRequirements++;
                if (iHadStat[MAGICFIND] >= 15)
                    iTotalRequirements++;
                if (iHadStat[MAXMANA] >= 60)
                    iTotalRequirements++;
                if (iHadStat[ARCANECRIT] >= 8)
                    iTotalRequirements++;
                if (iHadStat[ARCANECRIT] >= 10)
                    iTotalRequirements++;
                if (iTotalRequirements < 4)
                    iTotalPoints *= 0.4;
                else if (iTotalRequirements < 5)
                    iTotalPoints *= 0.5;
                if (iTotalRequirements >= 8)
                    iTotalPoints *= 1.2;
            }

            // Chests/cloaks/pants without a socket lose 17% of total score
            if ((thisGilesItemType == GilesItemType.Chest || thisGilesItemType == GilesItemType.Cloak || thisGilesItemType == GilesItemType.Legs) && iHadStat[SOCKETS] == 0)
                iTotalPoints *= 0.83;

            // Boots with no movement speed get reduced score
            if ((thisGilesItemType == GilesItemType.Boots) && iHadStat[MOVEMENTSPEED] <= 6)
                iTotalPoints *= 0.75;

            // Helmets
            if (thisGilesItemType == GilesItemType.Helm || thisGilesItemType == GilesItemType.WizardHat || thisGilesItemType == GilesItemType.VoodooMask || thisGilesItemType == GilesItemType.SpiritStone)
            {
                // Helmets without a socket lose 20% of total score, and most of any MF/GF bonus
                if (iHadStat[SOCKETS] == 0)
                {
                    iTotalPoints *= 0.8;
                    if (iHadStat[MAGICFIND] > 0 || iHadStat[GOLDFIND] > 0)
                    {
                        if (iHadStat[MAGICFIND] > 0 && iHadStat[GOLDFIND] > 0)
                            iTotalPoints -= ((iHadPoints[MAGICFIND] * 0.25) + (iHadPoints[GOLDFIND] * 0.25));
                        else
                            iTotalPoints -= ((iHadPoints[MAGICFIND] * 0.65) + (iHadPoints[GOLDFIND] * 0.65));
                    }
                }
            }

            // Gold-find and pickup radius combined
            if ((iHadStat[GOLDFIND] / iThisItemsMaxStats[GOLDFIND] > 0.55) && (iHadStat[PICKUPRADIUS] / iThisItemsMaxStats[PICKUPRADIUS] > 0.5))
                iTotalPoints += (((iThisItemsMaxPoints[PICKUPRADIUS] + iThisItemsMaxPoints[GOLDFIND]) / 2) * 0.25);

            // All-resist and pickup radius combined
            if ((iHadStat[ALLRESIST] / iThisItemsMaxStats[ALLRESIST] > 0.55) && (iHadStat[PICKUPRADIUS] > 0))
                iTotalPoints += (((iThisItemsMaxPoints[PICKUPRADIUS] + iThisItemsMaxPoints[ALLRESIST]) / 2) * 0.65);

            // Special crit hit/crit chance/attack speed combos
            double dBestFinalBonus = 1d;
            if ((iHadStat[CRITCHANCE] > (iThisItemsMaxStats[CRITCHANCE] * 0.8)) && (iHadStat[CRITDAMAGE] > (iThisItemsMaxStats[CRITDAMAGE] * 0.8)) && (iHadStat[ATTACKSPEED] > (iThisItemsMaxStats[ATTACKSPEED] * 0.8)))
            {
                if (dBestFinalBonus < 3.2 && thisGilesItemType != GilesItemType.Quiver) dBestFinalBonus = 3.2;
            }
            if ((iHadStat[CRITCHANCE] > (iThisItemsMaxStats[CRITCHANCE] * 0.8)) && (iHadStat[CRITDAMAGE] > (iThisItemsMaxStats[CRITDAMAGE] * 0.8)))
            {
                if (dBestFinalBonus < 2.3) dBestFinalBonus = 2.3;
            }
            if ((iHadStat[CRITCHANCE] > (iThisItemsMaxStats[CRITCHANCE] * 0.8)) && (iHadStat[ATTACKSPEED] > (iThisItemsMaxStats[ATTACKSPEED] * 0.8)))
            {
                if (dBestFinalBonus < 2.1 && thisGilesItemType != GilesItemType.Quiver) dBestFinalBonus = 2.1;
            }
            if ((iHadStat[CRITDAMAGE] > (iThisItemsMaxStats[CRITDAMAGE] * 0.8)) && (iHadStat[ATTACKSPEED] > (iThisItemsMaxStats[ATTACKSPEED] * 0.8)))
            {
                if (dBestFinalBonus < 1.8 && thisGilesItemType != GilesItemType.Quiver) dBestFinalBonus = 1.8;
            }
            if ((iHadStat[CRITCHANCE] > (iThisItemsMaxStats[CRITCHANCE] * 0.65)) && (iHadStat[CRITDAMAGE] > (iThisItemsMaxStats[CRITDAMAGE] * 0.65)) && (iHadStat[ATTACKSPEED] > (iThisItemsMaxStats[ATTACKSPEED] * 0.65)))
            {
                if (dBestFinalBonus < 2.1 && thisGilesItemType != GilesItemType.Quiver) dBestFinalBonus = 2.1;
            }
            if ((iHadStat[CRITCHANCE] > (iThisItemsMaxStats[CRITCHANCE] * 0.65)) && (iHadStat[CRITDAMAGE] > (iThisItemsMaxStats[CRITDAMAGE] * 0.65)))
            {
                if (dBestFinalBonus < 1.9) dBestFinalBonus = 1.9;
            }
            if ((iHadStat[CRITCHANCE] > (iThisItemsMaxStats[CRITCHANCE] * 0.65)) && (iHadStat[ATTACKSPEED] > (iThisItemsMaxStats[ATTACKSPEED] * 0.65)))
            {
                if (dBestFinalBonus < 1.7 && thisGilesItemType != GilesItemType.Quiver) dBestFinalBonus = 1.7;
            }
            if ((iHadStat[CRITDAMAGE] > (iThisItemsMaxStats[CRITDAMAGE] * 0.65)) && (iHadStat[ATTACKSPEED] > (iThisItemsMaxStats[ATTACKSPEED] * 0.65)))
            {
                if (dBestFinalBonus < 1.5 && thisGilesItemType != GilesItemType.Quiver) dBestFinalBonus = 1.5;
            }
            if ((iHadStat[CRITCHANCE] > (iThisItemsMaxStats[CRITCHANCE] * 0.45)) && (iHadStat[CRITDAMAGE] > (iThisItemsMaxStats[CRITDAMAGE] * 0.45)) && (iHadStat[ATTACKSPEED] > (iThisItemsMaxStats[ATTACKSPEED] * 0.45)))
            {
                if (dBestFinalBonus < 1.7 && thisGilesItemType != GilesItemType.Quiver) dBestFinalBonus = 1.7;
            }
            if ((iHadStat[CRITCHANCE] > (iThisItemsMaxStats[CRITCHANCE] * 0.45)) && (iHadStat[CRITDAMAGE] > (iThisItemsMaxStats[CRITDAMAGE] * 0.45)))
            {
                if (dBestFinalBonus < 1.4) dBestFinalBonus = 1.4;
            }
            if ((iHadStat[CRITCHANCE] > (iThisItemsMaxStats[CRITCHANCE] * 0.45)) && (iHadStat[ATTACKSPEED] > (iThisItemsMaxStats[ATTACKSPEED] * 0.45)))
            {
                if (dBestFinalBonus < 1.3 && thisGilesItemType != GilesItemType.Quiver) dBestFinalBonus = 1.3;
            }
            if ((iHadStat[CRITDAMAGE] > (iThisItemsMaxStats[CRITDAMAGE] * 0.45)) && (iHadStat[ATTACKSPEED] > (iThisItemsMaxStats[ATTACKSPEED] * 0.45)))
            {
                if (dBestFinalBonus < 1.1 && thisGilesItemType != GilesItemType.Quiver) dBestFinalBonus = 1.1;
            }
            iTotalPoints *= dBestFinalBonus;
            if (bFullAnalysis) Log("TOTAL: " + iTotalPoints.ToString());
            if (bFullAnalysis) Log("");
            return Math.Round(iTotalPoints);
        }
        
        public static bool EvaluateItemScoreForNotification(GilesBaseItemType thisgilesbaseitemtype, double ithisitemvalue)
        {
            switch (thisgilesbaseitemtype)
            {
                case GilesBaseItemType.WeaponOneHand:
                case GilesBaseItemType.WeaponRange:
                case GilesBaseItemType.WeaponTwoHand:
                    if (ithisitemvalue >= settings.iNeedPointsToNotifyWeapon)
                        return true;
                    break;
                case GilesBaseItemType.Armor:
                case GilesBaseItemType.Offhand:
                    if (ithisitemvalue >= settings.iNeedPointsToNotifyArmor)
                        return true;
                    break;
                case GilesBaseItemType.Jewelry:
                    if (ithisitemvalue >= settings.iNeedPointsToNotifyJewelry)
                        return true;
                    break;
            }
            return false;
        }

        // Full Output Of Item Stats
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
                    for (int iThisLevel = 1; iThisLevel <= 63; iThisLevel++)
                        if (ItemsDroppedStats.TotalPerLevel[iThisLevel] > 0)
                            LogWriter.WriteLine("- ilvl" + iThisLevel.ToString() + ": " + ItemsDroppedStats.TotalPerLevel[iThisLevel].ToString() + " [" +
                                Math.Round(ItemsDroppedStats.TotalPerLevel[iThisLevel] / TotalRunningTime.TotalHours, 2).ToString() + " per hour] {" +
                                Math.Round((ItemsDroppedStats.TotalPerLevel[iThisLevel] / ItemsDroppedStats.Total) * 100, 2).ToString() + " %}");
                    LogWriter.WriteLine("");
                    LogWriter.WriteLine("Items dropped by quality: ");
                    for (int iThisQuality = 0; iThisQuality <= 3; iThisQuality++)
                    {
                        if (ItemsDroppedStats.TotalPerQuality[iThisQuality] > 0)
                        {
                            LogWriter.WriteLine("- " + sQualityString[iThisQuality] + ": " + ItemsDroppedStats.TotalPerQuality[iThisQuality].ToString() + " [" + Math.Round(ItemsDroppedStats.TotalPerQuality[iThisQuality] / TotalRunningTime.TotalHours, 2).ToString() + " per hour] {" + Math.Round((ItemsDroppedStats.TotalPerQuality[iThisQuality] / ItemsDroppedStats.Total) * 100, 2).ToString() + " %}");
                            for (int iThisLevel = 1; iThisLevel <= 63; iThisLevel++)
                                if (ItemsDroppedStats.TotalPerQPerL[iThisQuality, iThisLevel] > 0)
                                    LogWriter.WriteLine("--- ilvl " + iThisLevel.ToString() + " " + sQualityString[iThisQuality] + ": " + ItemsDroppedStats.TotalPerQPerL[iThisQuality, iThisLevel].ToString() + " [" + Math.Round(ItemsDroppedStats.TotalPerQPerL[iThisQuality, iThisLevel] / TotalRunningTime.TotalHours, 2).ToString() + " per hour] {" + Math.Round((ItemsDroppedStats.TotalPerQPerL[iThisQuality, iThisLevel] / ItemsDroppedStats.Total) * 100, 2).ToString() + " %}");
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
                    for (int iThisLevel = 1; iThisLevel <= 63; iThisLevel++) if (ItemsDroppedStats.PotionsPerLevel[iThisLevel] > 0)
                            LogWriter.WriteLine("- ilvl " + iThisLevel.ToString() + ": " + ItemsDroppedStats.PotionsPerLevel[iThisLevel].ToString() + " [" + Math.Round(ItemsDroppedStats.PotionsPerLevel[iThisLevel] / TotalRunningTime.TotalHours, 2).ToString() + " per hour] {" + Math.Round((ItemsDroppedStats.PotionsPerLevel[iThisLevel] / ItemsDroppedStats.TotalPotions) * 100, 2).ToString() + " %}");
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
                            for (int iThisLevel = 1; iThisLevel <= 63; iThisLevel++)
                                if (ItemsDroppedStats.GemsPerTPerL[iThisGemType, iThisLevel] > 0)
                                    LogWriter.WriteLine("--- ilvl " + iThisLevel.ToString() + " " + sGemString[iThisGemType] + ": " + ItemsDroppedStats.GemsPerTPerL[iThisGemType, iThisLevel].ToString() + " [" + Math.Round(ItemsDroppedStats.GemsPerTPerL[iThisGemType, iThisLevel] / TotalRunningTime.TotalHours, 2).ToString() + " per hour] {" + Math.Round((ItemsDroppedStats.GemsPerTPerL[iThisGemType, iThisLevel] / ItemsDroppedStats.TotalGems) * 100, 2).ToString() + " %}");
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
                    for (int iThisLevel = 1; iThisLevel <= 63; iThisLevel++)
                        if (ItemsPickedStats.TotalPerLevel[iThisLevel] > 0)
                            LogWriter.WriteLine("- ilvl" + iThisLevel.ToString() + ": " + ItemsPickedStats.TotalPerLevel[iThisLevel].ToString() + " [" + Math.Round(ItemsPickedStats.TotalPerLevel[iThisLevel] / TotalRunningTime.TotalHours, 2).ToString() + " per hour] {" + Math.Round((ItemsPickedStats.TotalPerLevel[iThisLevel] / ItemsPickedStats.Total) * 100, 2).ToString() + " %}");
                    LogWriter.WriteLine("");
                    LogWriter.WriteLine("Items picked up by quality: ");
                    for (int iThisQuality = 0; iThisQuality <= 3; iThisQuality++)
                    {
                        if (ItemsPickedStats.TotalPerQuality[iThisQuality] > 0)
                        {
                            LogWriter.WriteLine("- " + sQualityString[iThisQuality] + ": " + ItemsPickedStats.TotalPerQuality[iThisQuality].ToString() + " [" + Math.Round(ItemsPickedStats.TotalPerQuality[iThisQuality] / TotalRunningTime.TotalHours, 2).ToString() + " per hour] {" + Math.Round((ItemsPickedStats.TotalPerQuality[iThisQuality] / ItemsPickedStats.Total) * 100, 2).ToString() + " %}");
                            for (int iThisLevel = 1; iThisLevel <= 63; iThisLevel++)
                                if (ItemsPickedStats.TotalPerQPerL[iThisQuality, iThisLevel] > 0)
                                    LogWriter.WriteLine("--- ilvl " + iThisLevel.ToString() + " " + sQualityString[iThisQuality] + ": " + ItemsPickedStats.TotalPerQPerL[iThisQuality, iThisLevel].ToString() + " [" + Math.Round(ItemsPickedStats.TotalPerQPerL[iThisQuality, iThisLevel] / TotalRunningTime.TotalHours, 2).ToString() + " per hour] {" + Math.Round((ItemsPickedStats.TotalPerQPerL[iThisQuality, iThisLevel] / ItemsPickedStats.Total) * 100, 2).ToString() + " %}");
                        }

                        // Any at all this quality?
                    }

                    // For loop on quality
                    LogWriter.WriteLine("");
                    if (iTotalFollowerItemsIgnored > 0)
                    {
                        LogWriter.WriteLine("  (note: " + iTotalFollowerItemsIgnored.ToString() + " follower items ignored for being ilvl <60 or blue)");
                    }
                }

                // End of item stats

                // Potion stats
                if (ItemsPickedStats.TotalPotions > 0)
                {
                    LogWriter.WriteLine("Potion Pickups:");
                    LogWriter.WriteLine("Total potions: " + ItemsPickedStats.TotalPotions.ToString() + " [" + Math.Round(ItemsPickedStats.TotalPotions / TotalRunningTime.TotalHours, 2).ToString() + " per hour]");
                    for (int iThisLevel = 1; iThisLevel <= 63; iThisLevel++) if (ItemsPickedStats.PotionsPerLevel[iThisLevel] > 0)
                            LogWriter.WriteLine("- ilvl " + iThisLevel.ToString() + ": " + ItemsPickedStats.PotionsPerLevel[iThisLevel].ToString() + " [" + Math.Round(ItemsPickedStats.PotionsPerLevel[iThisLevel] / TotalRunningTime.TotalHours, 2).ToString() + " per hour] {" + Math.Round((ItemsPickedStats.PotionsPerLevel[iThisLevel] / ItemsPickedStats.TotalPotions) * 100, 2).ToString() + " %}");
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
                            for (int iThisLevel = 1; iThisLevel <= 63; iThisLevel++)
                                if (ItemsPickedStats.GemsPerTPerL[iThisGemType, iThisLevel] > 0)
                                    LogWriter.WriteLine("--- ilvl " + iThisLevel.ToString() + " " + sGemString[iThisGemType] + ": " + ItemsPickedStats.GemsPerTPerL[iThisGemType, iThisLevel].ToString() + " [" + Math.Round(ItemsPickedStats.GemsPerTPerL[iThisGemType, iThisLevel] / TotalRunningTime.TotalHours, 2).ToString() + " per hour] {" + Math.Round((ItemsPickedStats.GemsPerTPerL[iThisGemType, iThisLevel] / ItemsPickedStats.TotalGems) * 100, 2).ToString() + " %}");
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

        // Search backpack to see if we have room for a 2-slot item anywhere
        private static Vector2 FindValidBackpackLocation(bool bOriginalTwoSlot)
        {
            bool[,] GilesBackpackSlotBlocked = new bool[10, 6];

            // Block off the entire of any "protected bag slots"
            foreach (InventorySquare thissquare in Zeta.CommonBot.Settings.CharacterSettings.Instance.ProtectedBagSlots)
                GilesBackpackSlotBlocked[thissquare.Column, thissquare.Row] = true;

            // Map out all the items already in the backpack
            foreach (ACDItem tempitem in ZetaDia.Me.Inventory.Backpack)
            {
                if (tempitem.BaseAddress == IntPtr.Zero)
                {
                    return new Vector2(-1, -1);
                }
                int inventoryRow = tempitem.InventoryRow;
                int inventoryColumn = tempitem.InventoryColumn;

                // Mark this slot as not-free
                GilesBackpackSlotBlocked[inventoryColumn, inventoryRow] = true;

                // Try and reliably find out if this is a two slot item or not
                GilesItemType tempItemType = DetermineItemType(tempitem.InternalName, tempitem.ItemType, tempitem.FollowerSpecialType);
                if (DetermineIsTwoSlot(tempItemType) && inventoryRow < 5)
                {
                    GilesBackpackSlotBlocked[inventoryColumn, inventoryRow + 1] = true;
                }
            }
            int iPointX = -1;
            int iPointY = -1;
            for (int iRow = 0; iRow <= 5; iRow++)
            {
                for (int iColumn = 0; iColumn <= 9; iColumn++)
                {
                    if (!GilesBackpackSlotBlocked[iColumn, iRow])
                    {
                        bool bNotEnoughSpace = false;
                        if (iRow < 5)
                        {
                            bNotEnoughSpace = (bOriginalTwoSlot && GilesBackpackSlotBlocked[iColumn, iRow + 1]);
                        }
                        else
                        {
                            if (bOriginalTwoSlot)
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
    }
}
