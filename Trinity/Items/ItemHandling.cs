using System;
using System.IO;
using System.Linq;
using Trinity.Cache;
using Trinity.DbProvider;
using Trinity.Helpers;
using Trinity.ItemRules;
using Trinity.Settings.Loot;
using Trinity.Technicals;
using Zeta.Bot;
using Zeta.Game;
using Zeta.Game.Internals.Actors;

namespace Trinity
{
    public partial class Trinity
    {
        /// <summary>
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
                return true;
            }

            // Tiered Rift Keys
            if (itemType == GItemType.TieredLootrunKey)
            {
                return Settings.Loot.Pickup.LootRunKey;
            }

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
                return Settings.Loot.Pickup.PickupLegendaries;
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
                    return CheckLevelRequirements(item.Level, item.Quality, Settings.Loot.Pickup.PickupBlueWeapons, Settings.Loot.Pickup.PickupYellowWeapons);
                case GItemBaseType.Armor:
                case GItemBaseType.Offhand:
                    return CheckLevelRequirements(item.Level, item.Quality, Settings.Loot.Pickup.PickupBlueArmor, Settings.Loot.Pickup.PickupYellowArmor);
                case GItemBaseType.Jewelry:
                    return CheckLevelRequirements(item.Level, item.Quality, Settings.Loot.Pickup.PickupBlueJewlery, Settings.Loot.Pickup.PickupYellowJewlery);
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

            if (StashRule != null)
            {
                // using ItemEvaluationType.Identify isn't available so we are abusing Sell for that manner
                Interpreter.InterpreterAction action = StashRule.checkPickUpItem(pickupItem, ItemEvaluationType.Sell);

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
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        internal static bool IdentifyItemValidation(PickupItem item)
        {
            if (Settings.Loot.TownRun.KeepLegendaryUnid)
                return false;
            return true;
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
            // Always pick legendaries
            if (quality >= ItemQuality.Legendary)
                return true;

            // Gray Items
            if (quality < ItemQuality.Normal)
            {
                if (Settings.Loot.Pickup.PickupGrayItems)
                    return true;
                return false;
            }

            // White Items
            if (quality == ItemQuality.Normal || quality == ItemQuality.Superior)
            {
                if (Settings.Loot.Pickup.PickupWhiteItems)
                    return true;
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
            if (quality >= ItemQuality.Magic1 && quality < ItemQuality.Rare4 && pickupBlue)
            {
                return false;
            }
            if (pickupYellow)
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

        /// <summary>
        ///     Output test scores for everything in the backpack
        /// </summary>
        internal static void TestScoring()
        {
            using (new PerformanceLogger("TestScoring"))
            {
                using (new ZetaCacheHelper())
                {
                    try
                    {
                        if (TownRun.TestingBackpack)
                            return;
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
                                    bool shouldStash = ItemManager.Current.ShouldStashItem(item);
                                    Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, shouldStash ? "* KEEP *" : "-- TRASH --");
                                }
                            }
                            Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "===== Finished Test Score Outputs =====");
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


        internal static bool IsWeaponArmorJewlery(CachedACDItem i)
        {
            return (i.DBBaseType == ItemBaseType.Armor || i.DBBaseType == ItemBaseType.Jewelry || i.DBBaseType == ItemBaseType.Weapon);
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
        ///     Checks if score of item is suffisant for throw notification.
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
        ///     Full Output Of Item Stats
        /// </summary>
        internal static void OutputReport()
        {
            using (new PerformanceLogger("OutputReport"))
            {
                if (!ZetaDia.Service.IsValid)
                    return;

                if (!ZetaDia.Service.Platform.IsConnected)
                    return;

                if (!ZetaDia.IsInGame)
                    return;

                if (ZetaDia.Me.IsFullyValid())
                    return;

                if (!Settings.Advanced.OutputReports)
                    return;

                if (CurrentWorldId <= 0 || Player.ActorClass == ActorClass.Invalid)
                    return;

                /*
                  Check is Lv 60 or not
                 * If lv 60 use Paragon
                 * If not lv 60 use normal xp/hr
                 */
                try
                {
                    Level = Player.Level;
                    ParagonLevel = Player.ParagonLevel;
                    if (Player.Level < 60)
                    {
                        if (!(TotalXP == 0 && LastXP == 0 && NextLevelXP == 0))
                        {
                            if (LastXP > Player.CurrentExperience)
                            {
                                TotalXP += NextLevelXP;
                            }
                            else
                            {
                                TotalXP += ZetaDia.Me.CurrentExperience - LastXP;
                            }
                        }
                        LastXP = Player.CurrentExperience;
                        NextLevelXP = Player.ExperienceNextLevel;
                    }
                    else
                    {
                        if (!(TotalXP == 0 && LastXP == 0 && NextLevelXP == 0))
                        {
                            // We have leveled up
                            if (NextLevelXP < Player.ParagonExperienceNextLevel)
                            {
                                TotalXP += NextLevelXP + Player.ParagonCurrentExperience;
                            }
                            else // We have not leveled up
                            {
                                TotalXP += NextLevelXP - Player.ParagonExperienceNextLevel;
                            }
                        }
                        LastXP = Player.ParagonCurrentExperience;
                        NextLevelXP = Player.ParagonExperienceNextLevel;
                    }


                    PersistentOutputReport();
                    TimeSpan totalRunningTime = DateTime.UtcNow.Subtract(ItemStatsWhenStartedBot);

                    string runStatsPath = Path.Combine(FileManager.LoggingPath, String.Format("RunStats - {0}.log", Player.ActorClass));

                    // Create whole new file
                    using (FileStream logStream =
                        File.Open(runStatsPath, FileMode.Create, FileAccess.Write, FileShare.Read))
                    {
                        using (var logWriter = new StreamWriter(logStream))
                        {
                            logWriter.WriteLine("===== Misc Statistics =====");
                            logWriter.WriteLine("Total tracking time: " + ((int)totalRunningTime.TotalHours) + "h " + totalRunningTime.Minutes +
                                                "m " + totalRunningTime.Seconds + "s");
                            logWriter.WriteLine("Total deaths: " + TotalDeaths + " [" + Math.Round(TotalDeaths / totalRunningTime.TotalHours, 2) + " per hour]");
                            logWriter.WriteLine("Total games (approx): " + TotalLeaveGames + " [" + Math.Round(TotalLeaveGames / totalRunningTime.TotalHours, 2) + " per hour]");
                            logWriter.WriteLine("Total Caches Opened:" + TotalBountyCachesOpened);
                            if (TotalLeaveGames == 0 && TotalGamesJoined > 0)
                            {
                                if (TotalGamesJoined == 1 && TotalProfileRecycles > 1)
                                {
                                    logWriter.WriteLine("(a profile manager/death handler is interfering with join/leave game events, attempting to guess total runs based on profile-loops)");
                                    logWriter.WriteLine("Total full profile cycles: " + TotalProfileRecycles + " [" + Math.Round(TotalProfileRecycles / totalRunningTime.TotalHours, 2) + " per hour]");
                                }
                                else
                                {
                                    logWriter.WriteLine("(your games left value may be bugged @ 0 due to profile managers/routines etc., now showing games joined instead:)");
                                    logWriter.WriteLine("Total games joined: " + TotalGamesJoined + " [" + Math.Round(TotalGamesJoined / totalRunningTime.TotalHours, 2) + " per hour]");
                                }
                            }

                            logWriter.WriteLine("Total XP gained: " + Math.Round(TotalXP / (float)1000000, 2) + " million [" + Math.Round(TotalXP / totalRunningTime.TotalHours / 1000000, 2) + " million per hour]");
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
                            logWriter.WriteLine("Total Gold gained: " + Math.Round(TotalGold / (float)1000, 2) + " Thousand [" + Math.Round(TotalGold / totalRunningTime.TotalHours / 1000, 2) + " Thousand per hour]");
                            logWriter.WriteLine("");
                            logWriter.WriteLine("===== Item DROP Statistics =====");

                            // Item stats
                            if (ItemsDroppedStats.Total > 0)
                            {
                                logWriter.WriteLine("Items:");
                                logWriter.WriteLine("Total items dropped: " + ItemsDroppedStats.Total + " [" +
                                                    Math.Round(ItemsDroppedStats.Total / totalRunningTime.TotalHours, 2) + " per hour]");
                                logWriter.WriteLine("Items dropped by ilvl: ");
                                for (int itemLevel = 1; itemLevel <= 63; itemLevel++)
                                    if (ItemsDroppedStats.TotalPerLevel[itemLevel] > 0)
                                        logWriter.WriteLine("- ilvl" + itemLevel + ": " + ItemsDroppedStats.TotalPerLevel[itemLevel] + " [" +
                                                            Math.Round(ItemsDroppedStats.TotalPerLevel[itemLevel] / totalRunningTime.TotalHours, 2) + " per hour] {" +
                                                            Math.Round((ItemsDroppedStats.TotalPerLevel[itemLevel] / ItemsDroppedStats.Total) * 100, 2) + " %}");
                                logWriter.WriteLine("");
                                logWriter.WriteLine("Items dropped by quality: ");
                                for (int iThisQuality = 0; iThisQuality <= 3; iThisQuality++)
                                {
                                    if (ItemsDroppedStats.TotalPerQuality[iThisQuality] > 0)
                                    {
                                        logWriter.WriteLine("- " + ItemQualityTypeStrings[iThisQuality] + ": " + ItemsDroppedStats.TotalPerQuality[iThisQuality] + " [" + Math.Round(ItemsDroppedStats.TotalPerQuality[iThisQuality] / totalRunningTime.TotalHours, 2) + " per hour] {" + Math.Round((ItemsDroppedStats.TotalPerQuality[iThisQuality] / ItemsDroppedStats.Total) * 100, 2) + " %}");
                                        for (int itemLevel = 1; itemLevel <= 63; itemLevel++)
                                            if (ItemsDroppedStats.TotalPerQPerL[iThisQuality, itemLevel] > 0)
                                                logWriter.WriteLine("--- ilvl " + itemLevel + " " + ItemQualityTypeStrings[iThisQuality] + ": " + ItemsDroppedStats.TotalPerQPerL[iThisQuality, itemLevel] + " [" + Math.Round(ItemsDroppedStats.TotalPerQPerL[iThisQuality, itemLevel] / totalRunningTime.TotalHours, 2) + " per hour] {" + Math.Round((ItemsDroppedStats.TotalPerQPerL[iThisQuality, itemLevel] / ItemsDroppedStats.Total) * 100, 2) + " %}");
                                    }

                                    // Any at all this quality?
                                }

                                // For loop on quality
                                logWriter.WriteLine("");
                            }

                            // End of item stats

                            // Gem stats
                            if (ItemsDroppedStats.TotalGems > 0)
                            {
                                logWriter.WriteLine("Gem Drops:");
                                logWriter.WriteLine("Total gems: " + ItemsDroppedStats.TotalGems + " [" + Math.Round(ItemsDroppedStats.TotalGems / totalRunningTime.TotalHours, 2) + " per hour]");
                                for (int iThisGemType = 0; iThisGemType <= 3; iThisGemType++)
                                {
                                    if (ItemsDroppedStats.GemsPerType[iThisGemType] > 0)
                                    {
                                        logWriter.WriteLine("- " + GemTypeStrings[iThisGemType] + ": " + ItemsDroppedStats.GemsPerType[iThisGemType] + " [" + Math.Round(ItemsDroppedStats.GemsPerType[iThisGemType] / totalRunningTime.TotalHours, 2) + " per hour] {" + Math.Round((ItemsDroppedStats.GemsPerType[iThisGemType] / ItemsDroppedStats.TotalGems) * 100, 2) + " %}");
                                        for (int itemLevel = 1; itemLevel <= 63; itemLevel++)
                                            if (ItemsDroppedStats.GemsPerTPerL[iThisGemType, itemLevel] > 0)
                                                logWriter.WriteLine("--- ilvl " + itemLevel + " " + GemTypeStrings[iThisGemType] + ": " + ItemsDroppedStats.GemsPerTPerL[iThisGemType, itemLevel] + " [" + Math.Round(ItemsDroppedStats.GemsPerTPerL[iThisGemType, itemLevel] / totalRunningTime.TotalHours, 2) + " per hour] {" + Math.Round((ItemsDroppedStats.GemsPerTPerL[iThisGemType, itemLevel] / ItemsDroppedStats.TotalGems) * 100, 2) + " %}");
                                    }

                                    // Any at all this quality?
                                }

                                // For loop on quality
                            }

                            // End of gem stats

                            // Key stats
                            if (ItemsDroppedStats.TotalInfernalKeys > 0)
                            {
                                logWriter.WriteLine("Infernal Key Drops:");
                                logWriter.WriteLine("Total Keys: " + ItemsDroppedStats.TotalInfernalKeys + " [" + Math.Round(ItemsDroppedStats.TotalInfernalKeys / totalRunningTime.TotalHours, 2) + " per hour]");
                            }

                            // End of key stats
                            logWriter.WriteLine("");
                            logWriter.WriteLine("");
                            logWriter.WriteLine("===== Item PICKUP Statistics =====");

                            // Item stats
                            if (ItemsPickedStats.Total > 0)
                            {
                                logWriter.WriteLine("Items:");
                                logWriter.WriteLine("Total items picked up: " + ItemsPickedStats.Total + " [" + Math.Round(ItemsPickedStats.Total / totalRunningTime.TotalHours, 2) + " per hour]");
                                logWriter.WriteLine("Item picked up by ilvl: ");
                                for (int itemLevel = 1; itemLevel <= 63; itemLevel++)
                                    if (ItemsPickedStats.TotalPerLevel[itemLevel] > 0)
                                        logWriter.WriteLine("- ilvl" + itemLevel + ": " + ItemsPickedStats.TotalPerLevel[itemLevel] + " [" + Math.Round(ItemsPickedStats.TotalPerLevel[itemLevel] / totalRunningTime.TotalHours, 2) + " per hour] {" + Math.Round((ItemsPickedStats.TotalPerLevel[itemLevel] / ItemsPickedStats.Total) * 100, 2) + " %}");
                                logWriter.WriteLine("");
                                logWriter.WriteLine("Items picked up by quality: ");
                                for (int iThisQuality = 0; iThisQuality <= 3; iThisQuality++)
                                {
                                    if (ItemsPickedStats.TotalPerQuality[iThisQuality] > 0)
                                    {
                                        logWriter.WriteLine("- " + ItemQualityTypeStrings[iThisQuality] + ": " + ItemsPickedStats.TotalPerQuality[iThisQuality] + " [" + Math.Round(ItemsPickedStats.TotalPerQuality[iThisQuality] / totalRunningTime.TotalHours, 2) + " per hour] {" + Math.Round((ItemsPickedStats.TotalPerQuality[iThisQuality] / ItemsPickedStats.Total) * 100, 2) + " %}");
                                        for (int itemLevel = 1; itemLevel <= 63; itemLevel++)
                                            if (ItemsPickedStats.TotalPerQPerL[iThisQuality, itemLevel] > 0)
                                                logWriter.WriteLine("--- ilvl " + itemLevel + " " + ItemQualityTypeStrings[iThisQuality] + ": " + ItemsPickedStats.TotalPerQPerL[iThisQuality, itemLevel] + " [" + Math.Round(ItemsPickedStats.TotalPerQPerL[iThisQuality, itemLevel] / totalRunningTime.TotalHours, 2) + " per hour] {" + Math.Round((ItemsPickedStats.TotalPerQPerL[iThisQuality, itemLevel] / ItemsPickedStats.Total) * 100, 2) + " %}");
                                    }

                                    // Any at all this quality?
                                }

                                // For loop on quality
                                logWriter.WriteLine("");
                                if (totalFollowerItemsIgnored > 0)
                                {
                                    logWriter.WriteLine("  (note: " + totalFollowerItemsIgnored + " follower items ignored for being ilvl <60 or blue)");
                                }
                            }

                            // End of item stats
                            // Gem stats
                            if (ItemsPickedStats.TotalGems > 0)
                            {
                                logWriter.WriteLine("Gem Pickups:");
                                logWriter.WriteLine("Total gems: " + ItemsPickedStats.TotalGems + " [" + Math.Round(ItemsPickedStats.TotalGems / totalRunningTime.TotalHours, 2) + " per hour]");
                                for (int iThisGemType = 0; iThisGemType <= 3; iThisGemType++)
                                {
                                    if (ItemsPickedStats.GemsPerType[iThisGemType] > 0)
                                    {
                                        logWriter.WriteLine("- " + GemTypeStrings[iThisGemType] + ": " + ItemsPickedStats.GemsPerType[iThisGemType] + " [" + Math.Round(ItemsPickedStats.GemsPerType[iThisGemType] / totalRunningTime.TotalHours, 2) + " per hour] {" + Math.Round((ItemsPickedStats.GemsPerType[iThisGemType] / ItemsPickedStats.TotalGems) * 100, 2) + " %}");
                                        for (int itemLevel = 1; itemLevel <= 63; itemLevel++)
                                            if (ItemsPickedStats.GemsPerTPerL[iThisGemType, itemLevel] > 0)
                                                logWriter.WriteLine("--- ilvl " + itemLevel + " " + GemTypeStrings[iThisGemType] + ": " + ItemsPickedStats.GemsPerTPerL[iThisGemType, itemLevel] + " [" + Math.Round(ItemsPickedStats.GemsPerTPerL[iThisGemType, itemLevel] / totalRunningTime.TotalHours, 2) + " per hour] {" + Math.Round((ItemsPickedStats.GemsPerTPerL[iThisGemType, itemLevel] / ItemsPickedStats.TotalGems) * 100, 2) + " %}");
                                    }

                                    // Any at all this quality?
                                }

                                // For loop on quality
                            }

                            // End of gem stats

                            // Key stats
                            if (ItemsPickedStats.TotalInfernalKeys > 0)
                            {
                                logWriter.WriteLine("Infernal Key Pickups:");
                                logWriter.WriteLine("Total Keys: " + ItemsPickedStats.TotalInfernalKeys + " [" + Math.Round(ItemsPickedStats.TotalInfernalKeys / totalRunningTime.TotalHours, 2) + " per hour]");
                            }

                            // End of key stats
                            logWriter.WriteLine("===== End Of Report =====");

                            logWriter.Flush();
                            logStream.Flush();
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
    }
}