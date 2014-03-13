using System;
using System.IO;
using Trinity.Cache;
using Trinity.Config.Loot;
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

            // Compute item quality from item link for Crafting Plans (Blacksmith or Jewler)
            if(c_InternalName.StartsWith("CraftingPlan_") || c_InternalName.StartsWith("CraftingReagent_Legendary_Unique_"))
            {
                if (!CacheData.itemLinkQualityCache.TryGetValue(c_ACDGUID, out c_ItemQuality))
                {
                    c_ItemQuality = TrinityItemManager.ItemLinkColorToQuality(diaItem.CommonData.ItemLink, c_InternalName, c_ItemDisplayName, c_GameBalanceID);
                    CacheData.itemLinkQualityCache.Add(c_ACDGUID, c_ItemQuality);
                }
            }

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

            c_ItemLevel = diaItem.CommonData.Level;
            c_DBItemBaseType = diaItem.CommonData.ItemBaseType;
            c_DBItemType = diaItem.CommonData.ItemType;
            c_IsOneHandedItem = diaItem.CommonData.IsOneHand;
            c_IsTwoHandedItem = diaItem.CommonData.IsTwoHand;
            c_item_tFollowerType = diaItem.CommonData.FollowerSpecialType;

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
                Position = c_Position,
                ActorSNO = c_ActorSNO
            };

            // Calculate item type
            c_item_GItemType = DetermineItemType(c_InternalName, c_DBItemType, c_item_tFollowerType);

            // And temporarily store the base type
            GItemBaseType itemBaseType = DetermineBaseType(c_item_GItemType);

            // Treat all globes as a yes
            if (c_item_GItemType == GItemType.HealthGlobe)
            {
                c_ObjectType = GObjectType.HealthGlobe;
                // Create or alter this cached object type
                CacheData.objectTypeCache[c_RActorGuid] = c_ObjectType;
                AddToCache = true;
            }

            // Treat all globes as a yes
            if (c_item_GItemType == GItemType.PowerGlobe)
            {
                c_ObjectType = GObjectType.PowerGlobe;
                // Create or alter this cached object type
                CacheData.objectTypeCache[c_RActorGuid] = c_ObjectType;
                AddToCache = true;
            }



            // Item stats
            logNewItem = RefreshItemStats(itemBaseType);

            // Get whether or not we want this item, cached if possible
            if (!CacheData.pickupItemCache.TryGetValue(c_RActorGuid, out AddToCache))
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

                CacheData.pickupItemCache.Add(c_RActorGuid, AddToCache);
            }

            // Using DB built-in item rules
            if (AddToCache && ForceVendorRunASAP)
                c_IgnoreSubStep = "ForcedVendoring";

            // Didn't pass pickup rules, so ignore it
            if (!AddToCache && c_IgnoreSubStep == String.Empty)
                c_IgnoreSubStep = "NoMatchingRule";

            if (Settings.Advanced.LogDroppedItems && logNewItem)
                LogDroppedItem();

            return AddToCache;
        }

        private static void LogDroppedItem()
        {
            string droppedItemLogPath = Path.Combine(FileManager.TrinityLogsPath, String.Format("ItemsDropped.csv"));

            bool pickupItem = false;
            CacheData.pickupItemCache.TryGetValue(c_RActorGuid, out pickupItem);

            bool writeHeader = !File.Exists(droppedItemLogPath);
            using (var LogWriter = new StreamWriter(droppedItemLogPath, true))
            {
                if (writeHeader)
                {
                    LogWriter.WriteLine("Timestamp,ActorSNO,RActorGUID,DyanmicID,GameBalanceID,ACDGuid,Name,InternalName,DBBaseType,TBaseType,DBItemType,TItemType,Quality,QualityLevelIdentified,Level,IgnoreItemSubStep,Distance,Pickup,SHA1Hash");
                }
                LogWriter.Write(FormatCSVField(DateTime.Now));
                LogWriter.Write(FormatCSVField(c_ActorSNO));
                LogWriter.Write(FormatCSVField(c_RActorGuid));
                LogWriter.Write(FormatCSVField(c_GameDynamicID));
                // GameBalanceID
                LogWriter.Write(FormatCSVField(c_GameBalanceID));
                LogWriter.Write(FormatCSVField(c_ACDGUID));
                LogWriter.Write(FormatCSVField(c_ItemDisplayName));
                LogWriter.Write(FormatCSVField(c_InternalName));
                LogWriter.Write(FormatCSVField(c_DBItemBaseType.ToString()));
                LogWriter.Write(FormatCSVField(DetermineBaseType(c_item_GItemType).ToString()));
                LogWriter.Write(FormatCSVField(c_DBItemType.ToString()));
                LogWriter.Write(FormatCSVField(c_item_GItemType.ToString()));
                LogWriter.Write(FormatCSVField(c_ItemQuality.ToString()));
                LogWriter.Write(FormatCSVField(c_ItemQualityLevelIdentified));
                LogWriter.Write(FormatCSVField(c_ItemLevel));
                LogWriter.Write(FormatCSVField(c_IgnoreSubStep));
                LogWriter.Write(FormatCSVField(c_CentreDistance));
                LogWriter.Write(FormatCSVField(pickupItem));
                LogWriter.Write(FormatCSVField(c_ItemMd5Hash));
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
            if (!CacheData.goldAmountCache.TryGetValue(c_RActorGuid, out c_GoldStackSize))
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
                CacheData.goldAmountCache.Add(c_RActorGuid, c_GoldStackSize);
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

        private static void LogSkippedGold()
        {
            string skippedItemsPath = Path.Combine(FileManager.LoggingPath, String.Format("SkippedGoldStacks_{0}_{1}.csv", Player.ActorClass, DateTime.Now.ToString("yyyy-MM-dd")));

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
            return String.Format("\"{0:yyyy-MM-ddTHH:mm:ss.ffffzzz}\",", time);
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