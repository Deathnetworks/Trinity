using System;
using System.IO;
using Trinity.Cache;
using Trinity.Config.Loot;
using Trinity.Helpers;
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
            bool logNewItem = false;
            bool AddToCache = false;

            if (c_BalanceID == -1)
            {
                AddToCache = false;
                c_IgnoreSubStep = "InvalidBalanceID";
            }

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
            c_ItemQualityLevelIdentified = ((DiaItem)c_diaObject).CommonData.GetAttribute<int>(ActorAttributeType.ItemQualityLevelIdentified);
            c_ItemDisplayName = diaItem.CommonData.Name;
            c_GameBalanceID = diaItem.CommonData.GameBalanceId;


            c_ItemLevel = diaItem.CommonData.Level;
            c_DBItemBaseType = diaItem.CommonData.ItemBaseType;
            c_DBItemType = diaItem.CommonData.ItemType;
            c_IsOneHandedItem = diaItem.CommonData.IsOneHand;
            c_IsTwoHandedItem = diaItem.CommonData.IsTwoHand;
            c_item_tFollowerType = diaItem.CommonData.FollowerSpecialType;
            // Calculate item type
            c_item_GItemType = DetermineItemType(c_InternalName, c_DBItemType, c_item_tFollowerType);

            // And temporarily store the base type
            GItemBaseType itemBaseType = DetermineBaseType(c_item_GItemType);

            // Compute item quality from item link for Crafting Plans (Blacksmith or Jewler)
            if (itemBaseType == GItemBaseType.Misc || itemBaseType == GItemBaseType.Unknown)
            {
                if (!CacheData.ItemLinkQuality.TryGetValue(c_ACDGUID, out c_ItemQuality))
                {
                    c_ItemQuality = diaItem.CommonData.ItemLinkColorQuality();
                    CacheData.ItemLinkQuality.Add(c_ACDGUID, c_ItemQuality);
                }
            }

            // Gem quality level hax
            if (itemBaseType == GItemBaseType.Gem)
                c_ItemLevel = diaItem.CommonData.GetGemQualityLevel();

            float fExtraRange = 0f;

            if (c_ItemQuality >= ItemQuality.Legendary)
            {
                // always pickup
                AddToCache = true;
            }
            else
            {
                if (c_ItemQuality >= ItemQuality.Rare4)
                    fExtraRange = CurrentBotLootRange;

                if (iKeepLootRadiusExtendedFor > 0)
                    fExtraRange += 90f;

                if (c_CentreDistance > (CurrentBotLootRange + fExtraRange))
                {
                    c_IgnoreSubStep = "OutOfRange";
                    AddToCache = false;
                    // return here to save CPU on reading unncessary attributes for out of range items;
                    if (!AddToCache)
                        return AddToCache;
                }
            }

            float damage, toughness, healing = 0;
            bool isUpgrade = false;
            diaItem.CommonData.GetStatChanges(out damage, out healing, out toughness);

            if (damage > 0 && toughness > 0)
                isUpgrade = true;

            var pickupItem = new PickupItem
            {
                Name = c_ItemDisplayName,
                InternalName = c_InternalName,
                Level = c_ItemLevel,
                Quality = c_ItemQuality,
                BalanceID = c_BalanceID,
                DBBaseType = c_DBItemBaseType,
                DBItemType = c_DBItemType,
                IsOneHand = c_IsOneHandedItem,
                IsTwoHand = c_IsTwoHandedItem,
                ItemFollowerType = c_item_tFollowerType,
                DynamicID = c_GameDynamicID,
                Position = CurrentCacheObject.Position,
                ActorSNO = c_ActorSNO,
                ACDGuid = c_ACDGUID,
                IsUpgrade = isUpgrade,
                UpgradeDamage = damage,
                UpgradeToughness = toughness,
                UpgradeHealing = healing
            };


            // Treat all globes as a yes
            if (c_item_GItemType == GItemType.HealthGlobe)
            {
                c_ObjectType = GObjectType.HealthGlobe;
                // Create or alter this cached object type
                CacheData.ObjectType[c_RActorGuid] = c_ObjectType;
                AddToCache = true;
                return AddToCache;
            }

            // Treat all globes as a yes
            if (c_item_GItemType == GItemType.PowerGlobe)
            {
                c_ObjectType = GObjectType.PowerGlobe;
                // Create or alter this cached object type
                CacheData.ObjectType[c_RActorGuid] = c_ObjectType;
                AddToCache = true;
                return AddToCache;
            }

            // Item stats
            logNewItem = RefreshItemStats(itemBaseType);

            // Get whether or not we want this item, cached if possible
            if (!CacheData.PickupItem.TryGetValue(c_RActorGuid, out AddToCache))
            {
                if (pickupItem.IsTwoHand && Settings.Loot.Pickup.IgnoreTwoHandedWeapons && c_ItemQuality < ItemQuality.Legendary)
                {
                    AddToCache = false;
                }
                else if (Settings.Loot.ItemFilterMode == ItemFilterMode.DemonBuddy)
                {
                    AddToCache = ItemManager.Current.ShouldPickUpItem((ACDItem)c_CommonData);
                }
                else if (Settings.Loot.ItemFilterMode == ItemFilterMode.TrinityWithItemRules)
                {
                    AddToCache = ItemRulesPickupValidation(pickupItem);
                }
                else // Trinity Scoring Only
                {
                    AddToCache = PickupItemValidation(pickupItem);
                }

                // Pickup low level enabled, and we're a low level
                if (!AddToCache && Settings.Loot.Pickup.PickupLowLevel && Player.Level <= 10)
                {
                    AddToCache = PickupItemValidation(pickupItem);
                }

                CacheData.PickupItem.Add(c_RActorGuid, AddToCache);
            }

            // Using DB built-in item rules
            if (AddToCache && ForceVendorRunASAP)
                c_IgnoreSubStep = "ForcedVendoring";

            // Didn't pass pickup rules, so ignore it
            if (!AddToCache && c_IgnoreSubStep == String.Empty)
                c_IgnoreSubStep = "NoMatchingRule";

            if (Settings.Advanced.LogDroppedItems && logNewItem && c_item_GItemType != GItemType.HealthGlobe && c_item_GItemType != GItemType.HealthPotion && c_item_GItemType != GItemType.PowerGlobe)
                LogDroppedItem();

            return AddToCache;
        }

        private static void LogDroppedItem()
        {
            string droppedItemLogPath = Path.Combine(FileManager.TrinityLogsPath, String.Format("ItemsDropped.csv"));

            bool pickupItem = false;
            CacheData.PickupItem.TryGetValue(c_RActorGuid, out pickupItem);

            bool writeHeader = !File.Exists(droppedItemLogPath);
            using (var LogWriter = new StreamWriter(droppedItemLogPath, true))
            {
                if (writeHeader)
                {
                    LogWriter.WriteLine("ActorSNO,GameBalanceID,Name,InternalName,DBBaseType,DBItemType,TBaseType,TItemType,Quality,Level,Pickup");
                }
                LogWriter.Write(FormatCSVField(c_ActorSNO));
                LogWriter.Write(FormatCSVField(c_GameBalanceID));
                LogWriter.Write(FormatCSVField(c_ItemDisplayName));
                LogWriter.Write(FormatCSVField(c_InternalName));
                LogWriter.Write(FormatCSVField(c_DBItemBaseType.ToString()));
                LogWriter.Write(FormatCSVField(c_DBItemType.ToString()));
                LogWriter.Write(FormatCSVField(DetermineBaseType(c_item_GItemType).ToString()));
                LogWriter.Write(FormatCSVField(c_item_GItemType.ToString()));
                LogWriter.Write(FormatCSVField(c_ItemQuality.ToString()));
                LogWriter.Write(FormatCSVField(c_ItemLevel));
                LogWriter.Write(FormatCSVField(pickupItem));
                LogWriter.Write("\n");
            }
        }

        private static bool RefreshGold(bool AddToCache)
        {
            //int rangedMinimumStackSize = 0;
            AddToCache = true;

            if (Player.ActorClass == ActorClass.Barbarian && Settings.Combat.Barbarian.IgnoreGoldInWOTB && Hotbar.Contains(SNOPower.Barbarian_WrathOfTheBerserker) &&
                GetHasBuff(SNOPower.Barbarian_WrathOfTheBerserker))
            {
                AddToCache = false;
                c_IgnoreSubStep = "IgnoreGoldInWOTB";
                return AddToCache;
            }

            // Get the gold amount of this pile, cached if possible
            if (!CacheData.GoldStack.TryGetValue(c_RActorGuid, out c_GoldStackSize))
            {
                try
                {
                    c_GoldStackSize = ((ACDItem)c_CommonData).Gold;
                }
                catch
                {
                    Logger.Log(TrinityLogLevel.Debug, LogCategory.CacheManagement, "Safely handled exception getting gold pile amount for item {0} [{1}]", c_InternalName, c_ActorSNO);
                    AddToCache = false;
                    c_IgnoreSubStep = "GetAttributeException";
                }
                CacheData.GoldStack.Add(c_RActorGuid, c_GoldStackSize);
            }

            if (c_GoldStackSize < Settings.Loot.Pickup.MinimumGoldStack)
            {
                AddToCache = false;
                c_IgnoreSubStep = "NotEnoughGold";
                return AddToCache;
            }

            if (c_CentreDistance <= Player.GoldPickupRadius)
            {
                AddToCache = false;
                c_IgnoreSubStep = "WithinPickupRadius";
                return AddToCache;
            }

            //if (!AddToCache)
            //    LogSkippedGold();

            //DbHelper.Log(TrinityLogLevel.Debug, LogCategory.CacheManagement, "Gold Stack {0} has iPercentage {1} with rangeMinimumStackSize: {2} Distance: {3} MininumGoldStack: {4} PickupRadius: {5} AddToCache: {6}",
            //    c_GoldStackSize, iPercentage, rangedMinimumStackSize, c_CentreDistance, Settings.Loot.Pickup.MinimumGoldStack, ZetaDia.Me.GoldPickUpRadius, AddToCache);

            return AddToCache;
        }
        private static bool RefreshItemStats(GItemBaseType tempbasetype)
        {
            bool isNewLogItem = false;

            c_ItemMd5Hash = HashGenerator.GenerateItemHash(CurrentCacheObject.Position, c_ActorSNO, c_InternalName, CurrentWorldDynamicId, c_ItemQuality, c_ItemLevel);

            if (!GenericCache.ContainsKey(c_ItemMd5Hash))
            {
                GenericCache.AddToCache(new GenericCacheObject(c_ItemMd5Hash, null, new TimeSpan(1, 0, 0)));

                try
                {
                    isNewLogItem = true;
                    if (tempbasetype == GItemBaseType.Armor || tempbasetype == GItemBaseType.WeaponOneHand || tempbasetype == GItemBaseType.WeaponTwoHand ||
                        tempbasetype == GItemBaseType.WeaponRange || tempbasetype == GItemBaseType.Jewelry || tempbasetype == GItemBaseType.FollowerItem ||
                        tempbasetype == GItemBaseType.Offhand)
                    {
                        try
                        {
                            int iThisQuality;
                            ItemsDroppedStats.Total++;
                            if (c_ItemQuality >= ItemQuality.Legendary)
                                iThisQuality = QUALITYORANGE;
                            else if (c_ItemQuality >= ItemQuality.Rare4)
                                iThisQuality = QUALITYYELLOW;
                            else if (c_ItemQuality >= ItemQuality.Magic1)
                                iThisQuality = QUALITYBLUE;
                            else
                                iThisQuality = QUALITYWHITE;
                            ItemsDroppedStats.TotalPerQuality[iThisQuality]++;
                            ItemsDroppedStats.TotalPerLevel[c_ItemLevel]++;
                            ItemsDroppedStats.TotalPerQPerL[iThisQuality, c_ItemLevel]++;
                        }
                        catch (Exception ex)
                        {
                            Logger.LogError("Error Refreshing Item Stats for Equippable Item: " + ex.ToString());
                        }
                    }
                    else if (tempbasetype == GItemBaseType.Gem)
                    {
                        try
                        {
                            int iThisGemType = 0;
                            ItemsDroppedStats.TotalGems++;
                            if (c_item_GItemType == GItemType.Topaz)
                                iThisGemType = GEMTOPAZ;
                            if (c_item_GItemType == GItemType.Ruby)
                                iThisGemType = GEMRUBY;
                            if (c_item_GItemType == GItemType.Emerald)
                                iThisGemType = GEMEMERALD;
                            if (c_item_GItemType == GItemType.Amethyst)
                                iThisGemType = GEMAMETHYST;
                            if (c_item_GItemType == GItemType.Diamond)
                                iThisGemType = GEMDIAMOND;
                            ItemsDroppedStats.GemsPerType[iThisGemType]++;
                            ItemsDroppedStats.GemsPerLevel[c_ItemLevel]++;
                            ItemsDroppedStats.GemsPerTPerL[iThisGemType, c_ItemLevel]++;
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
                            ItemsDroppedStats.TotalInfernalKeys++;
                        }
                        catch (Exception ex)
                        {
                            Logger.LogError("Error refreshing item stats for InfernalKey: " + ex.ToString());
                        }
                    }
                    // See if we should update the stats file
                    if (DateTime.UtcNow.Subtract(ItemStatsLastPostedReport).TotalSeconds > 10)
                    {
                        try
                        {
                            ItemStatsLastPostedReport = DateTime.UtcNow;
                            OutputReport();
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
                LogWriter.Write(FormatCSVField(c_ActorSNO));
                LogWriter.Write(FormatCSVField(c_RActorGuid));
                LogWriter.Write(FormatCSVField(c_GameDynamicID));
                LogWriter.Write(FormatCSVField(c_ACDGUID));
                LogWriter.Write(FormatCSVField(c_InternalName));
                LogWriter.Write(FormatCSVField(c_GoldStackSize));
                LogWriter.Write(FormatCSVField(c_IgnoreSubStep));
                LogWriter.Write(FormatCSVField(c_CentreDistance));
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