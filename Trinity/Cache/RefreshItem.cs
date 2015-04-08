using System;
using System.IO;
using System.Linq;
using Trinity.Cache;
using Trinity.Helpers;
using Trinity.Items;
using Trinity.Settings.Loot;
using Trinity.Technicals;
using Zeta.Bot;
using Zeta.Game;
using Zeta.Game.Internals.Actors;

namespace Trinity
{
    public partial class Trinity
    {
        private static bool RefreshItem()
        {
            using (new PerformanceLogger("RefreshItem"))
            {
                bool logNewItem;
                bool AddToCache;

                var diaItem = c_diaObject as DiaItem;

                if (diaItem == null)
                    return false;

                if (diaItem.CommonData == null)
                    return false;

                if (!diaItem.IsValid)
                    return false;

                if (!diaItem.CommonData.IsValid)
                    return false;

                c_ItemQuality = diaItem.CommonData.ItemQualityLevel;
                ((DiaItem)c_diaObject).CommonData.GetAttribute<int>(ActorAttributeType.ItemQualityLevelIdentified);
                c_ItemDisplayName = diaItem.CommonData.Name;

                c_CacheObject.DynamicID = c_diaObject.CommonData.DynamicId;
                c_CacheObject.GameBalanceID = c_diaObject.CommonData.GameBalanceId;

                c_ItemLevel = diaItem.CommonData.Level;
                c_DBItemBaseType = diaItem.CommonData.ItemBaseType;
                c_DBItemType = diaItem.CommonData.ItemType;
                c_IsOneHandedItem = diaItem.CommonData.IsOneHand;
                c_IsTwoHandedItem = diaItem.CommonData.IsTwoHand;
                c_item_tFollowerType = diaItem.CommonData.FollowerSpecialType;
                // Calculate item type
                c_item_GItemType = TrinityItemManager.DetermineItemType(c_CacheObject.InternalName, c_DBItemType, c_item_tFollowerType);

                // And temporarily store the base type
                GItemBaseType itemBaseType = TrinityItemManager.DetermineBaseType(c_item_GItemType);

                // Compute item quality from item link 
                if (!CacheData.ItemLinkQuality.TryGetValue(c_CacheObject.ACDGuid, out c_ItemQuality))
                {
                    c_ItemQuality = diaItem.CommonData.ItemLinkColorQuality();
                    CacheData.ItemLinkQuality.Add(c_CacheObject.ACDGuid, c_ItemQuality);
                }

                if (itemBaseType == GItemBaseType.Gem)
                    c_ItemLevel = (int)diaItem.CommonData.GemQuality;

                c_CacheObject.ObjectHash = HashGenerator.GenerateItemHash(
                    c_CacheObject.Position,
                    c_CacheObject.ActorSNO,
                    c_CacheObject.InternalName,
                    Player.WorldID,
                    c_ItemQuality,
                    c_ItemLevel);

                try
                {
                    c_IsAncient = c_ItemQuality == ItemQuality.Legendary && diaItem.CommonData.GetAttribute<int>(ActorAttributeType.AncientRank) > 0;
                }
                catch {}

                float range = 0f;

                // no range check on Legendaries
                if (c_ItemQuality < ItemQuality.Legendary)
                {
                    if (c_ItemQuality >= ItemQuality.Rare4)
                        range = CurrentBotLootRange;

                    if (_keepLootRadiusExtendedForSeconds > 0)
                        range += 90f;

                    if (c_CacheObject.Distance > (CurrentBotLootRange + range))
                    {
                        c_InfosSubStep += "OutOfRange";
                        // return here to save CPU on reading unncessary attributes for out of range items;
                        return false;
                    }
                }

                var pickupItem = new PickupItem
                {
                    Name = c_ItemDisplayName,
                    InternalName = c_CacheObject.InternalName,
                    Level = c_ItemLevel,
                    Quality = c_ItemQuality,
                    BalanceID = c_CacheObject.GameBalanceID,
                    DBBaseType = c_DBItemBaseType,
                    DBItemType = c_DBItemType,
                    TBaseType = itemBaseType,
                    TType = c_item_GItemType,
                    IsOneHand = c_IsOneHandedItem,
                    IsTwoHand = c_IsTwoHandedItem,
                    ItemFollowerType = c_item_tFollowerType,
                    DynamicID = c_CacheObject.DynamicID,
                    Position = c_CacheObject.Position,
                    ActorSNO = c_CacheObject.ActorSNO,
                    ACDGuid = c_CacheObject.ACDGuid,
                    RActorGUID = c_CacheObject.RActorGuid,
                };

                // Blood Shards == HoradricRelic
                if (c_item_GItemType == GItemType.HoradricRelic && ZetaDia.CPlayer.BloodshardCount >= 500)
                {
                    return false;
                }

                // Treat all globes as a yes
                if (c_item_GItemType == GItemType.HealthGlobe)
                {
                    c_CacheObject.Type = GObjectType.HealthGlobe;
                    return true;
                }

                // Treat all globes as a yes
                if (c_item_GItemType == GItemType.PowerGlobe)
                {
                    c_CacheObject.Type = GObjectType.PowerGlobe;
                    return true;
                }

                // Treat all globes as a yes
                if (c_item_GItemType == GItemType.ProgressionGlobe)
                {
                    c_CacheObject.Type = GObjectType.ProgressionGlobe;
                    return true;
                }

                // Item stats
                logNewItem = RefreshItemStats(itemBaseType);

                // Get whether or not we want this item, cached if possible
                if (!CacheData.PickupItem.TryGetValue(c_CacheObject.RActorGuid, out AddToCache))
                {
                    if (pickupItem.IsTwoHand && Settings.Loot.Pickup.IgnoreTwoHandedWeapons && c_ItemQuality < ItemQuality.Legendary)
                    {
                        AddToCache = false;
                    }
                    else if (Settings.Loot.ItemFilterMode == ItemFilterMode.DemonBuddy)
                    {
                        AddToCache = ItemManager.Current.ShouldPickUpItem((ACDItem)c_diaObject.CommonData);
                    }
                    else if (Settings.Loot.ItemFilterMode == ItemFilterMode.TrinityWithItemRules)
                    {
                        AddToCache = TrinityItemManager.ItemRulesPickupValidation(pickupItem);
                    }
                    else // Trinity Scoring Only
                    {
                        AddToCache = TrinityItemManager.PickupItemValidation(pickupItem);
                    }

                    // Pickup low level enabled, and we're a low level
                    if (!AddToCache && Settings.Loot.Pickup.PickupLowLevel && Player.Level <= 10)
                    {
                        AddToCache = TrinityItemManager.PickupItemValidation(pickupItem);
                    }

                    // Ignore if item has existed before in this location
                    if (Settings.Loot.TownRun.DropLegendaryInTown)
                    {
                        if (!CacheData.DroppedItems.Any(i => i.Equals(pickupItem)))
                        {
                            CacheData.DroppedItems.Add(pickupItem);
                            AddToCache = true;
                        }
                        else
                        {
                            Logger.LogDebug("Ignoring Dropped Item = ItemPosition={0} Hashcode={1} DynId={2}", pickupItem.Position, pickupItem.GetHashCode(), pickupItem.DynamicID);
                            AddToCache = false;
                        }
                            
                    }

                    CacheData.PickupItem.Add(c_CacheObject.RActorGuid, AddToCache);
                }

                if (AddToCache && ForceVendorRunASAP)
                    c_InfosSubStep += "ForcedVendoring";

                // Didn't pass pickup rules, so ignore it
                if (!AddToCache && c_InfosSubStep == String.Empty)
                    c_InfosSubStep += "NoMatchingRule";

                if (Settings.Advanced.LogDroppedItems && logNewItem && c_item_GItemType != GItemType.HealthGlobe && c_item_GItemType != GItemType.HealthPotion && c_item_GItemType != GItemType.PowerGlobe && c_item_GItemType != GItemType.ProgressionGlobe)
                    //LogDroppedItem();
                    ItemDroppedAppender.Instance.AppendDroppedItem(pickupItem);

                return AddToCache;
            }
        }

        private static bool RefreshGold()
        {
            if (!Settings.Loot.Pickup.PickupGold)
            {
                c_InfosSubStep += "PickupDisabled";
                return false;
            }

            if (Player.ActorClass == ActorClass.Barbarian && Settings.Combat.Barbarian.IgnoreGoldInWOTB && Hotbar.Contains(SNOPower.Barbarian_WrathOfTheBerserker) &&
                GetHasBuff(SNOPower.Barbarian_WrathOfTheBerserker))
            {
                c_InfosSubStep += "IgnoreGoldInWOTB";
                return false;
            }

            // Get the gold amount of this pile, cached if possible
            if (!CacheData.GoldStack.TryGetValue(c_CacheObject.RActorGuid, out c_GoldStackSize))
            {
                try
                {
                    c_GoldStackSize = ((ACDItem)c_diaObject.CommonData).Gold;
                }
                catch
                {
                    Logger.Log(TrinityLogLevel.Debug, LogCategory.CacheManagement, "Safely handled exception getting gold pile amount for item {0} [{1}]", c_CacheObject.InternalName, c_CacheObject.ActorSNO);
                    c_InfosSubStep += "GetAttributeException";
                    return false;
                }
                CacheData.GoldStack.Add(c_CacheObject.RActorGuid, c_GoldStackSize);
            }

            if (c_GoldStackSize < Settings.Loot.Pickup.MinimumGoldStack)
            {
                c_InfosSubStep += "NotEnoughGold";
                return false;
            }

            return true;
        }
        private static bool RefreshItemStats(GItemBaseType baseType)
        {
            bool isNewLogItem = false;

            c_ItemMd5Hash = HashGenerator.GenerateItemHash(c_CacheObject.Position, c_CacheObject.ActorSNO, c_CacheObject.InternalName, CurrentWorldDynamicId, c_ItemQuality, c_ItemLevel);

            if (!GenericCache.ContainsKey(c_ItemMd5Hash))
            {
                GenericCache.AddToCache(new GenericCacheObject(c_ItemMd5Hash, null, new TimeSpan(1, 0, 0)));

                try
                {
                    isNewLogItem = true;
                    if (baseType == GItemBaseType.Armor || baseType == GItemBaseType.WeaponOneHand || baseType == GItemBaseType.WeaponTwoHand ||
                        baseType == GItemBaseType.WeaponRange || baseType == GItemBaseType.Jewelry || baseType == GItemBaseType.FollowerItem ||
                        baseType == GItemBaseType.Offhand)
                    {
                        try
                        {
                            int iThisQuality;
                            ItemDropStats.ItemsDroppedStats.Total++;
                            if (c_ItemQuality >= ItemQuality.Legendary)
                                iThisQuality = ItemDropStats.QUALITYORANGE;
                            else if (c_ItemQuality >= ItemQuality.Rare4)
                                iThisQuality = ItemDropStats.QUALITYYELLOW;
                            else if (c_ItemQuality >= ItemQuality.Magic1)
                                iThisQuality = ItemDropStats.QUALITYBLUE;
                            else
                                iThisQuality = ItemDropStats.QUALITYWHITE;
                            ItemDropStats.ItemsDroppedStats.TotalPerQuality[iThisQuality]++;
                            ItemDropStats.ItemsDroppedStats.TotalPerLevel[c_ItemLevel]++;
                            ItemDropStats.ItemsDroppedStats.TotalPerQPerL[iThisQuality, c_ItemLevel]++;
                        }
                        catch (Exception ex)
                        {
                            Logger.LogError("Error Refreshing Item Stats for Equippable Item: " + ex.ToString());
                        }
                    }
                    else if (baseType == GItemBaseType.Gem)
                    {
                        try
                        {
                            int iThisGemType = 0;
                            ItemDropStats.ItemsDroppedStats.TotalGems++;
                            if (c_item_GItemType == GItemType.Topaz)
                                iThisGemType = ItemDropStats.GEMTOPAZ;
                            if (c_item_GItemType == GItemType.Ruby)
                                iThisGemType = ItemDropStats.GEMRUBY;
                            if (c_item_GItemType == GItemType.Emerald)
                                iThisGemType = ItemDropStats.GEMEMERALD;
                            if (c_item_GItemType == GItemType.Amethyst)
                                iThisGemType = ItemDropStats.GEMAMETHYST;
                            if (c_item_GItemType == GItemType.Diamond)
                                iThisGemType = ItemDropStats.GEMDIAMOND;
                            ItemDropStats.ItemsDroppedStats.GemsPerType[iThisGemType]++;
                            ItemDropStats.ItemsDroppedStats.GemsPerLevel[c_ItemLevel]++;
                            ItemDropStats.ItemsDroppedStats.GemsPerTPerL[iThisGemType, c_ItemLevel]++;
                        }
                        catch (Exception ex)
                        {
                            Logger.LogError("Error refreshing item stats for Gem: " + ex.ToString());
                        }
                    }
                    else if (c_item_GItemType == GItemType.InfernalKey)
                    {
                        try
                        {
                            ItemDropStats.ItemsDroppedStats.TotalInfernalKeys++;
                        }
                        catch (Exception ex)
                        {
                            Logger.LogError("Error refreshing item stats for InfernalKey: " + ex.ToString());
                        }
                    }
                    // See if we should update the stats file
                    if (DateTime.UtcNow.Subtract(ItemDropStats.ItemStatsLastPostedReport).TotalSeconds > 10)
                    {
                        try
                        {
                            ItemDropStats.ItemStatsLastPostedReport = DateTime.UtcNow;
                            ItemDropStats.OutputReport();
                        }
                        catch (Exception ex)
                        {
                            Logger.LogError("Error Calling OutputReport from RefreshItemStats " + ex.ToString());
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError("Couldn't do Item Stats" + ex.ToString());
                }
            }
            return isNewLogItem;
        }

        private static void LogSkippedGold()
        {
            string skippedItemsPath = Path.Combine(FileManager.LoggingPath, String.Format("SkippedGoldStacks_{0}_{1}.csv", Player.ActorClass, DateTime.UtcNow.ToString("yyyy-MM-dd")));

            bool writeHeader = !File.Exists(skippedItemsPath);
            using (var LogWriter = new StreamWriter(skippedItemsPath, true))
            {
                if (writeHeader)
                {
                    LogWriter.WriteLine("ActorSNO,RActorGUID,DyanmicID,ACDGuid,Name,GoldStackSize,IgnoreItemSubStep,Distance");
                }
                LogWriter.Write(FormatCSVField(c_CacheObject.ActorSNO));
                LogWriter.Write(FormatCSVField(c_CacheObject.RActorGuid));
                LogWriter.Write(FormatCSVField(c_CacheObject.DynamicID));
                LogWriter.Write(FormatCSVField(c_CacheObject.ACDGuid));
                LogWriter.Write(FormatCSVField(c_CacheObject.InternalName));
                LogWriter.Write(FormatCSVField(c_GoldStackSize));
                LogWriter.Write(FormatCSVField(c_InfosSubStep));
                LogWriter.Write(FormatCSVField(c_CacheObject.Distance));
                LogWriter.Write("\n");
            }
        }

        private static string FormatCSVField(DateTime time)
        {
            return String.Format("\"{0:yyyy-MM-ddTHH:mm:ss.ffff}\",", time.ToLocalTime());
        }

        private static string FormatCSVField(string text)
        {
            return String.Format("\"{0}\",", text);
        }

        private static string FormatCSVField(int number)
        {
            return String.Format("\"{0}\",", number);
        }

        private static string FormatCSVField(double number)
        {
            return String.Format("\"{0:0}\",", number);
        }

        private static string FormatCSVField(bool value)
        {
            return String.Format("\"{0}\",", value);
        }
    }
}