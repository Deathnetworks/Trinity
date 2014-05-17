using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Trinity.Cache;
using Trinity.Config.Loot;
using Trinity.Helpers;
using Trinity.ItemRules;
using Trinity.Items;
using Trinity.Technicals;
using Zeta.Bot;
using Zeta.Common;
using Zeta.Common.Plugins;
using Zeta.Game;
using Zeta.Game.Internals.Actors;
using Logger = Trinity.Technicals.Logger;

namespace Trinity
{
    public partial class Trinity : IPlugin
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        internal static bool ItemRulesPickupValidation(PickupItem item)
        {
            if (StashRule == null)
                StashRule = new Interpreter();

            Interpreter.InterpreterAction action = StashRule.checkPickUpItem(item, ItemEvaluationType.PickUp);

            switch (action)
            {
                case Interpreter.InterpreterAction.PICKUP:
                    return true;

                case Interpreter.InterpreterAction.IGNORE:
                    return false;
            }

            // TODO: remove if tested
            if (item.Quality > ItemQuality.Superior)
                Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "Item Rules doesn't handle {0} of type {1} and quality {2}!", item.Name, item.DBItemType, item.Quality);

            return PickupItemValidation(item);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        internal static bool PickupItemValidation(PickupItem item)
        {
            // Calculate item types and base types etc.
            GItemType itemType = DetermineItemType(item.InternalName, item.DBItemType, item.ItemFollowerType);
            GItemBaseType baseType = DetermineBaseType(itemType);


            // Pickup Legendary potions
            if (itemType == GItemType.HealthPotion && item.Quality >= ItemQuality.Legendary)
            {
                return true;
            }

            if (itemType == GItemType.InfernalKey)
            {
                return Settings.Loot.Pickup.InfernalKeys;
            }

            // Rift Keystone Fragments == LootRunkey
            if (itemType == GItemType.LootRunKey)
            {
                return Settings.Loot.Pickup.LootRunKey;
            }

            // Blood Shards == HoradricRelic
            if (itemType == GItemType.HoradricRelic && ZetaDia.CPlayer.BloodshardCount < 500)
            {
                return Settings.Loot.Pickup.BloodShards;
            }

            if (itemType == GItemType.CraftingMaterial && (item.ACDItem.GetTrinityItemQuality() < Settings.Loot.Pickup.MiscItemQuality || !Settings.Loot.Pickup.CraftMaterials))
            {
                return false;
            }

            if (itemType == GItemType.CraftTome)
            {
                return Settings.Loot.Pickup.CraftTomes;
            }

            // Plans
            if (item.InternalName.ToLower().StartsWith("craftingplan_smith") && (item.ACDItem.GetTrinityItemQuality() < Settings.Loot.Pickup.MiscItemQuality || !Settings.Loot.Pickup.Plans))
            {
                return false;
            }

            // Designs
            if (item.InternalName.ToLower().StartsWith("craftingplan_jeweler") && (item.ACDItem.GetTrinityItemQuality() < Settings.Loot.Pickup.MiscItemQuality || !Settings.Loot.Pickup.Designs))
            {
                return false;
            }

            // Always pickup Legendary plans
            if (itemType == GItemType.CraftingPlan && item.Quality >= ItemQuality.Legendary && Settings.Loot.Pickup.LegendaryPlans)
            {
                return true;
            }
            // If it's legendary, we always want it *IF* it's level is right
            if (item.Quality >= ItemQuality.Legendary)
            {
                return (Settings.Loot.Pickup.LegendaryLevel > 0 && (item.Level >= Settings.Loot.Pickup.LegendaryLevel || Settings.Loot.Pickup.LegendaryLevel == 1));
            }

            if (item.IsUpgrade && Settings.Loot.Pickup.PickupUpgrades)
            {
                return true;
            }

            switch (baseType)
            {
                case GItemBaseType.WeaponTwoHand:
                case GItemBaseType.WeaponOneHand:
                case GItemBaseType.WeaponRange:
                    return CheckLevelRequirements(item.Level, item.Quality, Settings.Loot.Pickup.WeaponBlueLevel, Settings.Loot.Pickup.WeaponYellowLevel);
                case GItemBaseType.Armor:
                case GItemBaseType.Offhand:
                    return CheckLevelRequirements(item.Level, item.Quality, Settings.Loot.Pickup.ArmorBlueLevel, Settings.Loot.Pickup.ArmorYellowLevel);
                case GItemBaseType.Jewelry:
                    return CheckLevelRequirements(item.Level, item.Quality, Settings.Loot.Pickup.JewelryBlueLevel, Settings.Loot.Pickup.JewelryYellowLevel);
                case GItemBaseType.FollowerItem:
                    if (item.Quality >= ItemQuality.Legendary)
                    {
                        return true;
                    }
                    if (item.Quality >= ItemQuality.Magic1 && item.Quality <= ItemQuality.Magic3 && Settings.Loot.Pickup.PickupBlueFollowerItems)
                        return true;
                    if (item.Quality >= ItemQuality.Rare4 && item.Quality <= ItemQuality.Rare6 && Settings.Loot.Pickup.PickupYellowFollowerItems)
                        return true;
                    // not matched above, ignore it
                    return false;
                case GItemBaseType.Gem:
                    if (item.Level < Settings.Loot.Pickup.GemLevel ||
                        (itemType == GItemType.Ruby && !Settings.Loot.Pickup.GemType.HasFlag(TrinityGemType.Ruby)) ||
                        (itemType == GItemType.Emerald && !Settings.Loot.Pickup.GemType.HasFlag(TrinityGemType.Emerald)) ||
                        (itemType == GItemType.Amethyst && !Settings.Loot.Pickup.GemType.HasFlag(TrinityGemType.Amethyst)) ||
                        (itemType == GItemType.Topaz && !Settings.Loot.Pickup.GemType.HasFlag(TrinityGemType.Topaz)) ||
                        (itemType == GItemType.Diamond && !Settings.Loot.Pickup.GemType.HasFlag(TrinityGemType.Diamond)))
                    {
                        return false;
                    }
                    break;
                case GItemBaseType.Misc:

                    // Potion filtering
                    if (itemType == GItemType.HealthPotion && item.Quality < ItemQuality.Legendary)
                    {
                        int potionsInBackPack = ZetaDia.Me.Inventory.Backpack.Where(p => p.ItemType == ItemType.Potion).Sum(p => p.ItemStackQuantity);

                        if (potionsInBackPack >= Settings.Loot.Pickup.PotionCount)
                            return false;
                        else
                            return true;
                    }
                    break;
                case GItemBaseType.HealthGlobe:
                    return true;
                case GItemBaseType.Unknown:
                    return false;
                default:
                    return false;
            }

            // Didn't cancel it, so default to true!
            return true;
        }

        internal static bool ItemRulesIdentifyValidation(ACDItem item)
        {
            ItemEvents.ResetTownRun();
            PickupItem pickupItem = new PickupItem(
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
            else
                return IdentifyItemValidation(pickupItem);

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        internal static bool IdentifyItemValidation(PickupItem item)
        {
            if (Trinity.Settings.Loot.TownRun.KeepLegendaryUnid)
                return false;
            else
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
            // Always pick legendaries
            if (quality >= ItemQuality.Legendary)
                return true;

            // Gray Items
            if (quality < ItemQuality.Normal)
            {
                if (Settings.Loot.Pickup.PickupGrayItems)
                    return true;
                else
                    return false;
            }

            // White Items
            if (quality == ItemQuality.Normal || quality == ItemQuality.Superior)
            {
                if (Settings.Loot.Pickup.PickupWhiteItems)
                    return true;
                else
                    return false;
            }

            if (quality < ItemQuality.Normal && Player.Level > 5 && !Settings.Loot.Pickup.PickupLowLevel)
            {
                // Grey item, ignore if we're over level 5
                return false;
            }

            // Ignore Gray/White if player level is <= 5 and we are picking up white
            if (quality <= ItemQuality.Normal && Player.Level <= 5 && !Settings.Loot.Pickup.PickupLowLevel)
            {
                return false;
            }

            if (quality < ItemQuality.Magic1 && Player.Level > 10)
            {
                // White item, ignore if we're over level 10
                return false;
            }

            // PickupLowLevel setting
            if (quality <= ItemQuality.Magic1 && Player.Level <= 10 && !Settings.Loot.Pickup.PickupLowLevel)
            {
                // ignore if we don't have the setting enabled
                return false;
            }

            // Blue/Yellow get scored
            if (quality >= ItemQuality.Magic1 && quality < ItemQuality.Rare4)
            {
                if (requiredBlueLevel == 0 || (requiredBlueLevel != 0 && level < requiredBlueLevel))
                {
                    // Between magic and rare, and either we want no blues, or this level is lower than the blue level we want
                    return false;
                }
            }
            else
            {
                if (requiredYellowLevel == 0 || (requiredYellowLevel != 0 && level < requiredYellowLevel))
                {
                    // Either we want no rares or the item is below the level we want
                    return false;
                }
            }
            return true;
        }

        // private static Regex x1Regex = new Regex("^x1_", RegexOptions.IgnoreCase | RegexOptions.Compiled | RegexOptions.CultureInvariant);

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
            if (name.StartsWith("x1_"))
                name = name.Substring(3, name.Length - 3);
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
            if (name == "console_powerglobe") return GItemType.PowerGlobe;

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

            if (name.StartsWith("horadricrelic"))
            {
                return GItemType.HoradricRelic;
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
            GItemBaseType itemBaseType = GItemBaseType.Unknown;

            // One Handed Weapons
            if (itemType == GItemType.Axe || itemType == GItemType.CeremonialKnife || itemType == GItemType.Dagger || itemType == GItemType.Flail ||
                itemType == GItemType.FistWeapon || itemType == GItemType.Mace || itemType == GItemType.MightyWeapon ||
                itemType == GItemType.Spear || itemType == GItemType.Sword || itemType == GItemType.Wand)
            {
                itemBaseType = GItemBaseType.WeaponOneHand;
            }
            // Two Handed Weapons
            else if (itemType == GItemType.TwoHandDaibo || itemType == GItemType.TwoHandMace || itemType == GItemType.TwoHandFlail ||
                itemType == GItemType.TwoHandMighty || itemType == GItemType.TwoHandPolearm || itemType == GItemType.TwoHandStaff ||
                itemType == GItemType.TwoHandSword || itemType == GItemType.TwoHandAxe)
            {
                itemBaseType = GItemBaseType.WeaponTwoHand;
            }
            // Ranged Weapons
            else if (itemType == GItemType.TwoHandCrossbow || itemType == GItemType.HandCrossbow || itemType == GItemType.TwoHandBow)
            {
                itemBaseType = GItemBaseType.WeaponRange;
            }
            // Off-hands
            else if (itemType == GItemType.Mojo || itemType == GItemType.Orb || itemType == GItemType.CrusaderShield ||
                itemType == GItemType.Quiver || itemType == GItemType.Shield)
            {
                itemBaseType = GItemBaseType.Offhand;
            }
            // Armors
            else if (itemType == GItemType.Boots || itemType == GItemType.Bracer || itemType == GItemType.Chest ||
                itemType == GItemType.Cloak || itemType == GItemType.Gloves || itemType == GItemType.Helm ||
                itemType == GItemType.Legs || itemType == GItemType.Shoulder || itemType == GItemType.SpiritStone ||
                itemType == GItemType.VoodooMask || itemType == GItemType.WizardHat || itemType == GItemType.Belt ||
                itemType == GItemType.MightyBelt)
            {
                itemBaseType = GItemBaseType.Armor;
            }
            // Jewlery
            else if (itemType == GItemType.Amulet || itemType == GItemType.Ring)
            {
                itemBaseType = GItemBaseType.Jewelry;
            }
            // Follower Items
            else if (itemType == GItemType.FollowerEnchantress || itemType == GItemType.FollowerScoundrel ||
                itemType == GItemType.FollowerTemplar)
            {
                itemBaseType = GItemBaseType.FollowerItem;
            }
            // Misc Items
            else if (itemType == GItemType.CraftingMaterial || itemType == GItemType.CraftTome || itemType == GItemType.LootRunKey || itemType == GItemType.HoradricRelic ||
                itemType == GItemType.SpecialItem || itemType == GItemType.CraftingPlan || itemType == GItemType.HealthPotion || itemType == GItemType.HoradricCache ||
                itemType == GItemType.Dye || itemType == GItemType.StaffOfHerding || itemType == GItemType.InfernalKey)
            {
                itemBaseType = GItemBaseType.Misc;
            }
            // Gems
            else if (itemType == GItemType.Ruby || itemType == GItemType.Emerald || itemType == GItemType.Topaz ||
                itemType == GItemType.Amethyst || itemType == GItemType.Diamond)
            {
                itemBaseType = GItemBaseType.Gem;
            }
            // Globes
            else if (itemType == GItemType.HealthGlobe)
            {
                itemBaseType = GItemBaseType.HealthGlobe;
            }
            else if (itemType == GItemType.PowerGlobe)
            {
                itemBaseType = GItemBaseType.PowerGlobe;
            }
            return itemBaseType;
        }

        /// <summary>
        /// Search backpack to see if we have room for a 2-slot item anywhere
        /// </summary>
        //internal static Vector2 SortingFindLocationBackpack(bool isOriginalTwoSlot)
        //{
        //    int x = -1;
        //    int y = -1;
        //    for (int row = 0; row <= 5; row++)
        //    {
        //        for (int col = 0; col <= 9; col++)
        //        {
        //            if (!BackpackSlotBlocked[col, row])
        //            {
        //                bool notEnoughSpace = false;
        //                if (row < 5)
        //                {
        //                    notEnoughSpace = (isOriginalTwoSlot && BackpackSlotBlocked[col, row + 1]);
        //                }
        //                else
        //                {
        //                    if (isOriginalTwoSlot)
        //                        notEnoughSpace = true;
        //                }
        //                if (!notEnoughSpace)
        //                {
        //                    x = col;
        //                    y = row;
        //                    goto FoundPackLocation;
        //                }
        //            }
        //        }
        //    }
        //FoundPackLocation:
        //    if ((x < 0) || (y < 0))
        //    {
        //        return new Vector2(-1, -1);
        //    }
        //    return new Vector2(x, y);
        //}

        /// <summary>
        /// Output test scores for everything in the backpack
        /// </summary>
        internal static void TestScoring()
        {
            using (new PerformanceLogger("TestScoring"))
            {
                using (var helper = new Helpers.ZetaCacheHelper())
                {
                    try
                    {
                        if (TownRun.TestingBackpack) return;
                        TownRun.TestingBackpack = true;
                        //ZetaDia.Actors.Update();
                        if (ZetaDia.Actors.Me == null)
                        {
                            Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "Error testing scores - not in game world?");
                            return;
                        }
                        if (ZetaDia.IsInGame && !ZetaDia.IsLoadingWorld)
                        {
                            Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "===== Outputting Test Scores =====");
                            foreach (ACDItem item in ZetaDia.Me.Inventory.Backpack)
                            {
                                if (item.BaseAddress == IntPtr.Zero)
                                {
                                    Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "GSError: Diablo 3 memory read error, or item became invalid [TestScore-1]");
                                }
                                else
                                {
                                    CachedACDItem cItem = new CachedACDItem(item, item.InternalName, item.Name, item.Level, item.ItemQualityLevel, item.Gold, item.GameBalanceId, item.DynamicId,
                                        item.Stats.WeaponDamagePerSecond, item.IsOneHand, item.IsTwoHand, item.DyeType, item.ItemType, item.ItemBaseType, item.FollowerSpecialType, item.IsUnidentified, item.ItemStackQuantity,
                                        item.Stats);
                                    bool bShouldStashTest = TrinityItemManager.Current.ShouldStashItem(item);
                                    Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, bShouldStashTest ? "* KEEP *" : "-- TRASH --");
                                }
                            }
                            Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "===== Finished Test Score Outputs =====");
                            //Logger.Log(TrinityLogLevel.Normal, LogCategory.UserInformation, "Note: See bad scores? Wrong item types? Known DB bug - restart DB before using the test button!");
                        }
                        else
                        {
                            Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "Error testing scores - not in game world?");
                        }
                        TownRun.TestingBackpack = false;
                    }
                    catch (Exception ex)
                    {
                        Logger.LogNormal("Exception in TestScoring(): {0}", ex);
                        TownRun.TestingBackpack = false;
                    }
                }
            }
        }


        internal static bool IsWeaponArmorJewlery(CachedACDItem thisitem)
        {
            return (thisitem.DBBaseType == Zeta.Game.Internals.Actors.ItemBaseType.Armor || thisitem.DBBaseType == Zeta.Game.Internals.Actors.ItemBaseType.Jewelry || thisitem.DBBaseType == Zeta.Game.Internals.Actors.ItemBaseType.Weapon);
        }

        /// <summary>Return the score needed to keep something by the item type</summary>
        internal static double ScoreNeeded(ItemBaseType itemBaseType)
        {
            switch (itemBaseType)
            {
                case ItemBaseType.Weapon:
                    return Math.Round((double)Settings.Loot.TownRun.WeaponScore);
                case ItemBaseType.Armor:
                    return Math.Round((double)Settings.Loot.TownRun.ArmorScore);
                case ItemBaseType.Jewelry:
                    return Math.Round((double)Settings.Loot.TownRun.JewelryScore);
                default:
                    return 0;
            }
        }

        /// <summary>
        /// Checks if score of item is suffisant for throw notification.
        /// </summary>
        public static bool CheckScoreForNotification(GItemBaseType itemBaseType, double itemValue)
        {
            switch (itemBaseType)
            {
                case GItemBaseType.WeaponOneHand:
                case GItemBaseType.WeaponRange:
                case GItemBaseType.WeaponTwoHand:
                    return (itemValue >= Settings.Notification.WeaponScore);
                case GItemBaseType.Armor:
                case GItemBaseType.Offhand:
                    return (itemValue >= Settings.Notification.ArmorScore);
                case GItemBaseType.Jewelry:
                    return (itemValue >= Settings.Notification.JewelryScore);
            }
            return false;
        }

        /// <summary>
        /// Full Output Of Item Stats
        /// </summary>
        internal static void OutputReport()
        {
            using (new PerformanceLogger("OutputReport"))
            {
                if (!ZetaDia.Service.IsValid)
                    return;

                if (!ZetaDia.Service.Platform.IsConnected)
                    return;

                if (ZetaDia.Me == null || !ZetaDia.Me.IsValid)
                    return;

                if (!Settings.Advanced.OutputReports)
                    return;

                if (CurrentWorldId <= 0 || Player.ActorClass == ActorClass.Invalid)
                    return;

                if (!ZetaDia.IsInGame)
                    return;
                /*
                  Check is Lv 60 or not
                 * If lv 60 use Paragon
                 * If not lv 60 use normal xp/hr
                 */
                try
                {
                    Level = Player.Level;
                    ParagonLevel = Trinity.Player.ParagonLevel;
                    if (Player.Level < 60)
                    {
                        if (!(TotalXP == 0 && LastXP == 0 && NextLevelXP == 0))
                        {
                            if (LastXP > Trinity.Player.CurrentExperience)
                            {
                                TotalXP += NextLevelXP;
                            }
                            else
                            {
                                TotalXP += ZetaDia.Me.CurrentExperience - LastXP;
                            }
                        }
                        LastXP = Trinity.Player.CurrentExperience;
                        NextLevelXP = Trinity.Player.ExperienceNextLevel;
                    }
                    else
                    {
                        if (!(TotalXP == 0 && LastXP == 0 && NextLevelXP == 0))
                        {
                            // We have leveled up
                            if (NextLevelXP < Trinity.Player.ParagonExperienceNextLevel)
                            {
                                TotalXP += NextLevelXP + Trinity.Player.ParagonCurrentExperience;
                            }
                            else // We have not leveled up
                            {
                                TotalXP += NextLevelXP - Trinity.Player.ParagonExperienceNextLevel;
                            }
                        }
                        LastXP = Trinity.Player.ParagonCurrentExperience;
                        NextLevelXP = Trinity.Player.ParagonExperienceNextLevel;
                    }


                    PersistentOutputReport();
                    TimeSpan TotalRunningTime = DateTime.UtcNow.Subtract(ItemStatsWhenStartedBot);

                    var runStatsPath = Path.Combine(FileManager.LoggingPath, String.Format("RunStats - {0}.log", Player.ActorClass));

                    // Create whole new file
                    using (FileStream LogStream =
                        File.Open(runStatsPath, FileMode.Create, FileAccess.Write, FileShare.Read))
                    {
                        using (StreamWriter LogWriter = new StreamWriter(LogStream))
                        {
                            LogWriter.WriteLine("===== Misc Statistics =====");
                            LogWriter.WriteLine("Total tracking time: " + ((int)TotalRunningTime.TotalHours).ToString() + "h " + TotalRunningTime.Minutes.ToString() +
                                "m " + TotalRunningTime.Seconds.ToString() + "s");
                            LogWriter.WriteLine("Total deaths: " + TotalDeaths.ToString() + " [" + Math.Round(TotalDeaths / TotalRunningTime.TotalHours, 2).ToString() + " per hour]");
                            LogWriter.WriteLine("Total games (approx): " + TotalLeaveGames.ToString() + " [" + Math.Round(TotalLeaveGames / TotalRunningTime.TotalHours, 2).ToString() + " per hour]");
                            LogWriter.WriteLine("Total Caches Opened:" + Trinity.TotalBountyCachesOpened);
                            if (TotalLeaveGames == 0 && TotalGamesJoined > 0)
                            {
                                if (TotalGamesJoined == 1 && TotalProfileRecycles > 1)
                                {
                                    LogWriter.WriteLine("(a profile manager/death handler is interfering with join/leave game events, attempting to guess total runs based on profile-loops)");
                                    LogWriter.WriteLine("Total full profile cycles: " + TotalProfileRecycles.ToString() + " [" + Math.Round(TotalProfileRecycles / TotalRunningTime.TotalHours, 2).ToString() + " per hour]");
                                }
                                else
                                {
                                    LogWriter.WriteLine("(your games left value may be bugged @ 0 due to profile managers/routines etc., now showing games joined instead:)");
                                    LogWriter.WriteLine("Total games joined: " + TotalGamesJoined.ToString() + " [" + Math.Round(TotalGamesJoined / TotalRunningTime.TotalHours, 2).ToString() + " per hour]");
                                }
                            }

                            LogWriter.WriteLine("Total XP gained: " + Math.Round(TotalXP / (float)1000000, 2).ToString() + " million [" + Math.Round(TotalXP / TotalRunningTime.TotalHours / 1000000, 2).ToString() + " million per hour]");
                            if (LastGold == 0)
                            {
                                LastGold = Player.Coinage;
                            }
                            if (Player.Coinage - LastGold >= 500000)
                            {
                                LastGold = Player.Coinage;
                            }
                            else
                            {
                                TotalGold += Player.Coinage - LastGold;
                                LastGold = Player.Coinage;
                            }
                            LogWriter.WriteLine("Total Gold gained: " + Math.Round(TotalGold / (float)1000, 2).ToString() + " Thousand [" + Math.Round(TotalGold / TotalRunningTime.TotalHours / 1000, 2).ToString() + " Thousand per hour]");
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
                                        LogWriter.WriteLine("- " + ItemQualityTypeStrings[iThisQuality] + ": " + ItemsDroppedStats.TotalPerQuality[iThisQuality].ToString() + " [" + Math.Round(ItemsDroppedStats.TotalPerQuality[iThisQuality] / TotalRunningTime.TotalHours, 2).ToString() + " per hour] {" + Math.Round((ItemsDroppedStats.TotalPerQuality[iThisQuality] / ItemsDroppedStats.Total) * 100, 2).ToString() + " %}");
                                        for (int itemLevel = 1; itemLevel <= 63; itemLevel++)
                                            if (ItemsDroppedStats.TotalPerQPerL[iThisQuality, itemLevel] > 0)
                                                LogWriter.WriteLine("--- ilvl " + itemLevel.ToString() + " " + ItemQualityTypeStrings[iThisQuality] + ": " + ItemsDroppedStats.TotalPerQPerL[iThisQuality, itemLevel].ToString() + " [" + Math.Round(ItemsDroppedStats.TotalPerQPerL[iThisQuality, itemLevel] / TotalRunningTime.TotalHours, 2).ToString() + " per hour] {" + Math.Round((ItemsDroppedStats.TotalPerQPerL[iThisQuality, itemLevel] / ItemsDroppedStats.Total) * 100, 2).ToString() + " %}");
                                    }

                                    // Any at all this quality?
                                }

                                // For loop on quality
                                LogWriter.WriteLine("");
                            }

                            // End of item stats

                            // Gem stats
                            if (ItemsDroppedStats.TotalGems > 0)
                            {
                                LogWriter.WriteLine("Gem Drops:");
                                LogWriter.WriteLine("Total gems: " + ItemsDroppedStats.TotalGems.ToString() + " [" + Math.Round(ItemsDroppedStats.TotalGems / TotalRunningTime.TotalHours, 2).ToString() + " per hour]");
                                for (int iThisGemType = 0; iThisGemType <= 3; iThisGemType++)
                                {
                                    if (ItemsDroppedStats.GemsPerType[iThisGemType] > 0)
                                    {
                                        LogWriter.WriteLine("- " + GemTypeStrings[iThisGemType] + ": " + ItemsDroppedStats.GemsPerType[iThisGemType].ToString() + " [" + Math.Round(ItemsDroppedStats.GemsPerType[iThisGemType] / TotalRunningTime.TotalHours, 2).ToString() + " per hour] {" + Math.Round((ItemsDroppedStats.GemsPerType[iThisGemType] / ItemsDroppedStats.TotalGems) * 100, 2).ToString() + " %}");
                                        for (int itemLevel = 1; itemLevel <= 63; itemLevel++)
                                            if (ItemsDroppedStats.GemsPerTPerL[iThisGemType, itemLevel] > 0)
                                                LogWriter.WriteLine("--- ilvl " + itemLevel.ToString() + " " + GemTypeStrings[iThisGemType] + ": " + ItemsDroppedStats.GemsPerTPerL[iThisGemType, itemLevel].ToString() + " [" + Math.Round(ItemsDroppedStats.GemsPerTPerL[iThisGemType, itemLevel] / TotalRunningTime.TotalHours, 2).ToString() + " per hour] {" + Math.Round((ItemsDroppedStats.GemsPerTPerL[iThisGemType, itemLevel] / ItemsDroppedStats.TotalGems) * 100, 2).ToString() + " %}");
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
                                        LogWriter.WriteLine("- " + ItemQualityTypeStrings[iThisQuality] + ": " + ItemsPickedStats.TotalPerQuality[iThisQuality].ToString() + " [" + Math.Round(ItemsPickedStats.TotalPerQuality[iThisQuality] / TotalRunningTime.TotalHours, 2).ToString() + " per hour] {" + Math.Round((ItemsPickedStats.TotalPerQuality[iThisQuality] / ItemsPickedStats.Total) * 100, 2).ToString() + " %}");
                                        for (int itemLevel = 1; itemLevel <= 63; itemLevel++)
                                            if (ItemsPickedStats.TotalPerQPerL[iThisQuality, itemLevel] > 0)
                                                LogWriter.WriteLine("--- ilvl " + itemLevel.ToString() + " " + ItemQualityTypeStrings[iThisQuality] + ": " + ItemsPickedStats.TotalPerQPerL[iThisQuality, itemLevel].ToString() + " [" + Math.Round(ItemsPickedStats.TotalPerQPerL[iThisQuality, itemLevel] / TotalRunningTime.TotalHours, 2).ToString() + " per hour] {" + Math.Round((ItemsPickedStats.TotalPerQPerL[iThisQuality, itemLevel] / ItemsPickedStats.Total) * 100, 2).ToString() + " %}");
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
                            // Gem stats
                            if (ItemsPickedStats.TotalGems > 0)
                            {
                                LogWriter.WriteLine("Gem Pickups:");
                                LogWriter.WriteLine("Total gems: " + ItemsPickedStats.TotalGems.ToString() + " [" + Math.Round(ItemsPickedStats.TotalGems / TotalRunningTime.TotalHours, 2).ToString() + " per hour]");
                                for (int iThisGemType = 0; iThisGemType <= 3; iThisGemType++)
                                {
                                    if (ItemsPickedStats.GemsPerType[iThisGemType] > 0)
                                    {
                                        LogWriter.WriteLine("- " + GemTypeStrings[iThisGemType] + ": " + ItemsPickedStats.GemsPerType[iThisGemType].ToString() + " [" + Math.Round(ItemsPickedStats.GemsPerType[iThisGemType] / TotalRunningTime.TotalHours, 2).ToString() + " per hour] {" + Math.Round((ItemsPickedStats.GemsPerType[iThisGemType] / ItemsPickedStats.TotalGems) * 100, 2).ToString() + " %}");
                                        for (int itemLevel = 1; itemLevel <= 63; itemLevel++)
                                            if (ItemsPickedStats.GemsPerTPerL[iThisGemType, itemLevel] > 0)
                                                LogWriter.WriteLine("--- ilvl " + itemLevel.ToString() + " " + GemTypeStrings[iThisGemType] + ": " + ItemsPickedStats.GemsPerTPerL[iThisGemType, itemLevel].ToString() + " [" + Math.Round(ItemsPickedStats.GemsPerTPerL[iThisGemType, itemLevel] / TotalRunningTime.TotalHours, 2).ToString() + " per hour] {" + Math.Round((ItemsPickedStats.GemsPerTPerL[iThisGemType, itemLevel] / ItemsPickedStats.TotalGems) * 100, 2).ToString() + " %}");
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

                            LogWriter.Flush();
                            LogStream.Flush();
                        }
                    }
                }
                catch (NullReferenceException)
                {
                    // do nothing... db read error
                }
                catch (AccessViolationException)
                {
                    // do nothing... db read error. 
                }
                catch (Exception ex)
                {
                    Logger.Log(LogCategory.UserInformation, "Error generating item report! Try deleting TrinityLogs directory to fix.");
                    Logger.Log(LogCategory.UserInformation, "{0}", ex.ToString());
                }
            }
        }

        internal static bool TownVisitShouldTownRun()
        {
            double cellsFilled = 0;
            foreach (ACDItem i in ZetaDia.Me.Inventory.Backpack)
            {
                cellsFilled++;
                if (i.IsTwoSquareItem)
                    cellsFilled++;
            }

            double maxCells = 60;
            double ratioCellsFilled = cellsFilled / maxCells;

            // return true if we're already in town and backpack is 1/2 full
            if (ratioCellsFilled > .5 && ZetaDia.IsInTown)
                return true;

            return false;
        }

    }

}
