using System;
using System.IO;
using System.Linq;
using Trinity.Cache;
using Trinity.ItemRules;
using Trinity.Config.Loot;
using Trinity.Technicals;
using Zeta;
using Zeta.Common;
using Zeta.Common.Plugins;
using Zeta.CommonBot;
using Zeta.Internals.Actors;

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
                Logger.Log(TrinityLogLevel.Normal, LogCategory.UserInformation, "Item Rules doesn't handle {0} of type {1} and quality {2}!", item.Name, item.DBItemType, item.Quality);

            return PickupItemValidation(item);
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        internal static bool PickupItemValidation(PickupItem item)
        {

            // If it's legendary, we always want it *IF* it's level is right
            if (item.Quality >= ItemQuality.Legendary)
            {
                return (Settings.Loot.Pickup.LegendaryLevel > 0 && (item.Level >= Settings.Loot.Pickup.LegendaryLevel || Settings.Loot.Pickup.LegendaryLevel == 1));
            }

            // Calculate item types and base types etc.
            GItemType itemType = DetermineItemType(item.InternalName, item.DBItemType, item.ItemFollowerType);
            GItemBaseType baseType = DetermineBaseType(itemType);

            string itemSha1Hash = HashGenerator.GenerateItemHash(item.Position, item.ActorSNO, item.Name, CurrentWorldDynamicId, item.Quality, item.Level);

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
                    if (item.Level < 60 || !Settings.Loot.Pickup.FollowerItem || item.Quality < ItemQuality.Rare4)
                    {
                        if (!_hashsetItemFollowersIgnored.Contains(itemSha1Hash))
                        {
                            _hashsetItemFollowersIgnored.Add(itemSha1Hash);
                            totalFollowerItemsIgnored++;
                        }
                        return false;
                    }
                    break;
                case GItemBaseType.Gem:
                    if (item.Level < Settings.Loot.Pickup.GemLevel ||
                        (itemType == GItemType.Ruby && !Settings.Loot.Pickup.GemType.HasFlag(TrinityGemType.Ruby)) ||
                        (itemType == GItemType.Emerald && !Settings.Loot.Pickup.GemType.HasFlag(TrinityGemType.Emerald)) ||
                        (itemType == GItemType.Amethyst && !Settings.Loot.Pickup.GemType.HasFlag(TrinityGemType.Amethyst)) ||
                        (itemType == GItemType.Topaz && !Settings.Loot.Pickup.GemType.HasFlag(TrinityGemType.Topaz)))
                    {
                        return false;
                    }
                    break;
                case GItemBaseType.Misc:

                    if (itemType == GItemType.CraftingMaterial && (item.Level < Settings.Loot.Pickup.MiscItemLevel || !Settings.Loot.Pickup.CraftMaterials))
                    {
                        return false;
                    }

                    if (itemType == GItemType.CraftTome && !Settings.Loot.Pickup.CraftTomes)
                    {
                        return false;
                    }

                    // Plans
                    if (item.InternalName.ToLower().StartsWith("craftingplan_smith") && (item.Level < Settings.Loot.Pickup.MiscItemLevel || !Settings.Loot.Pickup.Plans))
                    {
                        return false;
                    }

                    // Designs
                    if (item.InternalName.ToLower().StartsWith("craftingplan_jeweler") && (item.Level < Settings.Loot.Pickup.MiscItemLevel || !Settings.Loot.Pickup.Designs))
                    {
                        return false;
                    }

                    // Always pickup Legendary plans
                    if (itemType == GItemType.CraftingPlan && item.Quality >= ItemQuality.Legendary && Settings.Loot.Pickup.LegendaryPlans)
                    {
                        return true;
                    }

                    if (itemType == GItemType.InfernalKey && !Settings.Loot.Pickup.InfernalKeys)
                    {
                        return false;
                    }

                    // Potion filtering
                    if (itemType == GItemType.HealthPotion)
                    {
                        if (Settings.Loot.Pickup.PotionMode == PotionMode.Ignore || item.Level < Settings.Loot.Pickup.PotionLevel)
                        {
                            return false;
                        }
                        if (Settings.Loot.Pickup.PotionMode == PotionMode.Cap)
                        {
                            // Map out all the items already in the backpack
                            int iTotalPotions =
                                (from tempitem in ZetaDia.Me.Inventory.Backpack
                                 where tempitem.BaseAddress != IntPtr.Zero &&
                                 tempitem.GameBalanceId == item.BalanceID
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
                                       item.DynamicId);

            Logger.Log(TrinityLogLevel.Normal, LogCategory.UserInformation,
                "Incoming Identification Request: {0}, {1}, {2}, {3}, {4}",
                pickupItem.Quality, pickupItem.Level, pickupItem.DBBaseType,
                pickupItem.DBItemType, pickupItem.IsOneHand ? "1H" : pickupItem.IsTwoHand ? "2H" : "NH");

            // using ItemEvaluationType.Identify isn't available so we are abusing Sell for that manner
            Interpreter.InterpreterAction action = Trinity.StashRule.checkPickUpItem(pickupItem, ItemEvaluationType.Sell);

            Logger.Log(TrinityLogLevel.Normal, LogCategory.UserInformation, "Action is: {0}", action);

            switch (action)
            {
                case Interpreter.InterpreterAction.IDENTIFY:
                    return true;
                case Interpreter.InterpreterAction.UNIDENT:
                    return false;
                default:
                    Logger.Log(TrinityLogLevel.Normal, LogCategory.ScriptRule, "Trinity, item is unhandled by ItemRules (Identification)!");
                    return IdentifyItemValidation(pickupItem);
            }

        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        internal static bool IdentifyItemValidation(PickupItem item)
        {
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
            if (quality < ItemQuality.Normal && Player.Level > 5 && Settings.Loot.Pickup.PickupLowLevel)
            {
                // Grey item, ignore if we're over level 5
                return false;
            }

            if (quality < ItemQuality.Magic1 && Player.Level > 10 && Settings.Loot.Pickup.PickupLowLevel)
            {
                // White item, ignore if we're over level 10
                return false;
            }
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
            if (name.StartsWith("polearm_") || dbItemType == ItemType.Polearm) return GItemType.TwoHandPolearm;
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
            if (name.StartsWith("healthpotion")) return GItemType.HealthPotion;
            if (name.StartsWith("followeritem_enchantress_") || dbFollowerType == FollowerType.Enchantress) return GItemType.FollowerEnchantress;
            if (name.StartsWith("followeritem_scoundrel_") || dbFollowerType == FollowerType.Scoundrel) return GItemType.FollowerScoundrel;
            if (name.StartsWith("followeritem_templar_") || dbFollowerType == FollowerType.Templar) return GItemType.FollowerTemplar;
            if (name.StartsWith("craftingplan_")) return GItemType.CraftingPlan;
            if (name.StartsWith("craftingmaterials_")) return GItemType.CraftingMaterial;
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
            GItemBaseType itemBaseType = GItemBaseType.Unknown;
            if (itemType == GItemType.Axe || itemType == GItemType.CeremonialKnife || itemType == GItemType.Dagger ||
                itemType == GItemType.FistWeapon || itemType == GItemType.Mace || itemType == GItemType.MightyWeapon ||
                itemType == GItemType.Spear || itemType == GItemType.Sword || itemType == GItemType.Wand)
            {
                itemBaseType = GItemBaseType.WeaponOneHand;
            }
            else if (itemType == GItemType.TwoHandDaibo || itemType == GItemType.TwoHandMace ||
                itemType == GItemType.TwoHandMighty || itemType == GItemType.TwoHandPolearm || itemType == GItemType.TwoHandStaff ||
                itemType == GItemType.TwoHandSword || itemType == GItemType.TwoHandAxe)
            {
                itemBaseType = GItemBaseType.WeaponTwoHand;
            }
            else if (itemType == GItemType.TwoHandCrossbow || itemType == GItemType.HandCrossbow || itemType == GItemType.TwoHandBow)
            {
                itemBaseType = GItemBaseType.WeaponRange;
            }
            else if (itemType == GItemType.Mojo || itemType == GItemType.Orb ||
                itemType == GItemType.Quiver || itemType == GItemType.Shield)
            {
                itemBaseType = GItemBaseType.Offhand;
            }
            else if (itemType == GItemType.Boots || itemType == GItemType.Bracer || itemType == GItemType.Chest ||
                itemType == GItemType.Cloak || itemType == GItemType.Gloves || itemType == GItemType.Helm ||
                itemType == GItemType.Legs || itemType == GItemType.Shoulder || itemType == GItemType.SpiritStone ||
                itemType == GItemType.VoodooMask || itemType == GItemType.WizardHat || itemType == GItemType.Belt ||
                itemType == GItemType.MightyBelt)
            {
                itemBaseType = GItemBaseType.Armor;
            }
            else if (itemType == GItemType.Amulet || itemType == GItemType.Ring)
            {
                itemBaseType = GItemBaseType.Jewelry;
            }
            else if (itemType == GItemType.FollowerEnchantress || itemType == GItemType.FollowerScoundrel ||
                itemType == GItemType.FollowerTemplar)
            {
                itemBaseType = GItemBaseType.FollowerItem;
            }
            else if (itemType == GItemType.CraftingMaterial || itemType == GItemType.CraftTome ||
                itemType == GItemType.SpecialItem || itemType == GItemType.CraftingPlan || itemType == GItemType.HealthPotion ||
                itemType == GItemType.Dye || itemType == GItemType.StaffOfHerding || itemType == GItemType.InfernalKey)
            {
                itemBaseType = GItemBaseType.Misc;
            }
            else if (itemType == GItemType.Ruby || itemType == GItemType.Emerald || itemType == GItemType.Topaz ||
                itemType == GItemType.Amethyst)
            {
                itemBaseType = GItemBaseType.Gem;
            }
            else if (itemType == GItemType.HealthGlobe)
            {
                itemBaseType = GItemBaseType.HealthGlobe;
            }
            return itemBaseType;
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
            //ZetaDia.Actors.Update();
            if (ZetaDia.Actors.Me == null)
            {
                Logger.Log(TrinityLogLevel.Normal, LogCategory.UserInformation, "Error testing scores - not in game world?");
                return;
            }
            if (ZetaDia.IsInGame && !ZetaDia.IsLoadingWorld)
            {
                Logger.Log(TrinityLogLevel.Normal, LogCategory.UserInformation, "===== Outputting Test Scores =====");
                foreach (ACDItem item in ZetaDia.Me.Inventory.Backpack)
                {
                    if (item.BaseAddress == IntPtr.Zero)
                    {
                        Logger.Log(TrinityLogLevel.Normal, LogCategory.UserInformation, "GSError: Diablo 3 memory read error, or item became invalid [TestScore-1]");
                    }
                    else
                    {
                        CachedACDItem thiscacheditem = new CachedACDItem(item, item.InternalName, item.Name, item.Level, item.ItemQualityLevel, item.Gold, item.GameBalanceId, item.DynamicId,
                            item.Stats.WeaponDamagePerSecond, item.IsOneHand, item.IsTwoHand, item.DyeType, item.ItemType, item.ItemBaseType, item.FollowerSpecialType, item.IsUnidentified, item.ItemStackQuantity,
                            item.Stats);
                        bool bShouldStashTest = ShouldWeStashThis(thiscacheditem);
                        Logger.Log(TrinityLogLevel.Normal, LogCategory.UserInformation, bShouldStashTest ? "* KEEP *" : "-- TRASH --");
                    }
                }
                Logger.Log(TrinityLogLevel.Normal, LogCategory.UserInformation, "===== Finished Test Score Outputs =====");
                Logger.Log(TrinityLogLevel.Normal, LogCategory.UserInformation, "Note: See bad scores? Wrong item types? Known DB bug - restart DB before using the test button!");
            }
            else
            {
                Logger.Log(TrinityLogLevel.Normal, LogCategory.UserInformation, "Error testing scores - not in game world?");
            }
            TownRun.testingBackpack = false;
        }

        /// <summary>
        /// Determine if we should stash this item or not based on item type and score, and/or loot rule scripting
        /// </summary>
        /// <param name="cItem"></param>
        /// <returns></returns>
        internal static bool ShouldWeStashThis(CachedACDItem cItem, ACDItem acdItem = null)
        {
            // Now look for Misc items we might want to keep
            GItemType itemType = DetermineItemType(cItem.InternalName, cItem.DBItemType, cItem.FollowerType);
            GItemBaseType itemBaseType = DetermineBaseType(itemType);

            if (itemType == GItemType.StaffOfHerding)
            {
                Logger.Log(TrinityLogLevel.Normal, LogCategory.ItemValuation, "{0} [{1}] [{2}] = (autokeep staff of herding)", cItem.RealName, cItem.InternalName, itemType);
                return true;
            }
            if (itemType == GItemType.CraftingMaterial)
            {
                Logger.Log(TrinityLogLevel.Normal, LogCategory.ItemValuation, "{0} [{1}] [{2}] = (autokeep craft materials)", cItem.RealName, cItem.InternalName, itemType);
                return true;
            }

            if (itemType == GItemType.Emerald)
            {
                Logger.Log(TrinityLogLevel.Normal, LogCategory.ItemValuation, "{0} [{1}] [{2}] = (autokeep gems)", cItem.RealName, cItem.InternalName, itemType);
                return true;
            }
            if (itemType == GItemType.Amethyst)
            {
                Logger.Log(TrinityLogLevel.Normal, LogCategory.ItemValuation, "{0} [{1}] [{2}] = (autokeep gems)", cItem.RealName, cItem.InternalName, itemType);
                return true;
            }
            if (itemType == GItemType.Topaz)
            {
                Logger.Log(TrinityLogLevel.Normal, LogCategory.ItemValuation, "{0} [{1}] [{2}] = (autokeep gems)", cItem.RealName, cItem.InternalName, itemType);
                return true;
            }
            if (itemType == GItemType.Ruby)
            {
                Logger.Log(TrinityLogLevel.Normal, LogCategory.ItemValuation, "{0} [{1}] [{2}] = (autokeep gems)", cItem.RealName, cItem.InternalName, itemType);
                return true;
            }
            if (itemType == GItemType.CraftTome)
            {
                Logger.Log(TrinityLogLevel.Normal, LogCategory.ItemValuation, "{0} [{1}] [{2}] = (autokeep tomes)", cItem.RealName, cItem.InternalName, itemType);
                return true;
            }
            if (itemType == GItemType.InfernalKey)
            {
                Logger.Log(TrinityLogLevel.Normal, LogCategory.UserInformation, "{0} [{1}] [{2}] = (autokeep infernal key)", cItem.RealName, cItem.InternalName, itemType);
                return true;
            }
            if (itemType == GItemType.HealthPotion)
            {
                Logger.Log(TrinityLogLevel.Normal, LogCategory.ItemValuation, "{0} [{1}] [{2}] = (ignoring potions)", cItem.RealName, cItem.InternalName, itemType);
                return false;
            }

            // Stash all unidentified items - assume we want to keep them since we are using an identifier over-ride
            if (cItem.IsUnidentified)
            {
                Logger.Log(TrinityLogLevel.Normal, LogCategory.UserInformation, "{0} [{1}] = (autokeep unidentified items)", cItem.RealName, cItem.InternalName);
                return true;
            }

            if (Settings.Loot.ItemFilterMode == ItemFilterMode.TrinityWithItemRules)
            {
                Interpreter.InterpreterAction action = StashRule.checkItem(acdItem, ItemEvaluationType.Keep);
                Logger.Log(TrinityLogLevel.Normal, LogCategory.UserInformation, "{0} [{1}] [{2}] = (" + action + ")", cItem.AcdItem.Name, cItem.AcdItem.InternalName, cItem.AcdItem.ItemType);
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
            if ((acdItem.ItemBaseType == ItemBaseType.Armor
               || acdItem.ItemBaseType == ItemBaseType.Weapon
               || acdItem.ItemBaseType == ItemBaseType.Jewelry)
                  && cItem.Quality < ItemQuality.Rare4)
            {
                return false;
            }


            if (cItem.Quality >= ItemQuality.Legendary)
            {
                Logger.Log(TrinityLogLevel.Normal, LogCategory.UserInformation, "{0} [{1}] [{2}] = (autokeep legendaries)", cItem.RealName, cItem.InternalName, itemType);
                return true;
            }

            if (itemType == GItemType.CraftingPlan)
            {
                Logger.Log(TrinityLogLevel.Normal, LogCategory.UserInformation, "{0} [{1}] [{2}] = (autokeep plans)", cItem.RealName, cItem.InternalName, itemType);
                return true;
            }

            // Ok now try to do some decent item scoring based on item types
            double iNeedScore = ScoreNeeded(acdItem.ItemBaseType);
            double iMyScore = ItemValuation.ValueThisItem(cItem, itemType);

            Logger.Log(TrinityLogLevel.Verbose, LogCategory.ItemValuation, "{0} [{1}] [{2}] = {3}", cItem.RealName, cItem.InternalName, itemType, iMyScore);
            if (iMyScore >= iNeedScore) return true;

            // If we reached this point, then we found no reason to keep the item!
            return false;
        }

        internal static bool IsWeaponArmorJewlery(CachedACDItem thisitem)
        {
            return (thisitem.DBBaseType == Zeta.Internals.Actors.ItemBaseType.Armor || thisitem.DBBaseType == Zeta.Internals.Actors.ItemBaseType.Jewelry || thisitem.DBBaseType == Zeta.Internals.Actors.ItemBaseType.Weapon);
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
            if (ZetaDia.Me == null || !ZetaDia.Me.IsValid)
                return;

            if (!Settings.Advanced.OutputReports)
            {
                return;
            }

            /*
              Check is Lv 60 or not
             * If lv 60 use Paragon
             * If not lv 60 use normal xp/hr
             */
            try
            {
                if (Player.Level < 60)
                {
                    if (!(iTotalXp == 0 && iLastXp == 0 && iNextLvXp == 0))
                    {
                        if (iLastXp > ZetaDia.Me.CurrentExperience)
                        {
                            iTotalXp += iNextLvXp;
                        }
                        else
                        {
                            iTotalXp += ZetaDia.Me.CurrentExperience - iLastXp;
                        }
                    }
                    iLastXp = ZetaDia.Me.CurrentExperience;
                    iNextLvXp = ZetaDia.Me.ExperienceNextLevel;
                }
                else
                {
                    if (!(iTotalXp == 0 && iLastXp == 0 && iNextLvXp == 0))
                    {
                        if (iLastXp > ZetaDia.Me.ParagonCurrentExperience)
                        {
                            iTotalXp += iNextLvXp;
                        }
                        else
                        {
                            iTotalXp += ZetaDia.Me.ParagonCurrentExperience - iLastXp;
                        }
                    }
                    iLastXp = ZetaDia.Me.ParagonCurrentExperience;
                    iNextLvXp = ZetaDia.Me.ParagonExperienceNextLevel;
                }


                PersistentOutputReport();
                TimeSpan TotalRunningTime = DateTime.Now.Subtract(ItemStatsWhenStartedBot);

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

                        LogWriter.WriteLine("Total XP gained: " + Math.Round(iTotalXp / (float)1000000, 2).ToString() + " million [" + Math.Round(iTotalXp / TotalRunningTime.TotalHours / 1000000, 2).ToString() + " million per hour]");
                        if (iLastGold == 0)
                        {
                            iLastGold = Player.Coinage;
                        }
                        if (Player.Coinage - iLastGold >= 500000)
                        {
                            iLastGold = Player.Coinage;
                        }
                        else
                        {
                            iTotalGold += Player.Coinage - iLastGold;
                            iLastGold = Player.Coinage;
                        }
                        LogWriter.WriteLine("Total Gold gained: " + Math.Round(iTotalGold / (float)1000, 2).ToString() + " Thousand [" + Math.Round(iTotalGold / TotalRunningTime.TotalHours / 1000, 2).ToString() + " Thousand per hour]");
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

                        LogWriter.Flush();
                        LogStream.Flush();
                    }
                }
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
            if (ratioCellsFilled > .5 && ZetaDia.Me.IsInTown)
                return true;

            return false;
        }

        /// <summary>
        /// Search backpack to see if we have room for a 2-slot item anywhere
        /// </summary>
        /// <param name="IsOriginalTwoSlot"></param>
        /// <returns></returns>
        internal static Vector2 FindValidBackpackLocation(bool IsOriginalTwoSlot)
        {
            try
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
                    if (item.IsTwoSquareItem && inventoryRow < 5)
                    {
                        BackpackSlotBlocked[inventoryColumn, inventoryRow + 1] = true;
                    }
                }

                int iPointX = -1;
                int iPointY = -1;

                // 6 rows
                for (int iRow = 0; iRow <= 5; iRow++)
                {
                    // 10 columns
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
            catch (Exception ex)
            {
                Logger.Log(LogCategory.UserInformation, "Error in finding backpack slot");
                Logger.Log(LogCategory.UserInformation, "{0}", ex.ToString());
                return new Vector2(1, 1);
            }
        }
    }
}
