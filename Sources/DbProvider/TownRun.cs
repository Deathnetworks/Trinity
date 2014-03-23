using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Windows.Documents;
using Trinity.Config.Loot;
using Trinity.Notifications;
using Trinity.Technicals;
using Zeta.Bot;
using Zeta.Bot.Logic;
using Zeta.Bot.Profile;
using Zeta.Bot.Profile.Common;
using Zeta.Common;
using Zeta.Game;
using Zeta.Game.Internals.Actors;
using Zeta.TreeSharp;
using Action = Zeta.TreeSharp.Action;
using Logger = Trinity.Technicals.Logger;
using NotificationManager = Trinity.Notifications.NotificationManager;

namespace Trinity
{
    internal class TownRun
    {
        static TownRun()
        {
            PreTownRunWorldId = -1;
            PreTownRunPosition = Vector3.Zero;
            WasVendoring = false;
        }

        // Whether salvage/sell run should go to a middle-waypoint first to help prevent stucks

        internal static bool LastTownRunCheckResult = false;
        // Random variables used during item handling and town-runs

        private static bool _loggedAnythingThisStash = false;

        private static bool _loggedJunkThisStash = false;
        internal static string ValueItemStatString = "";
        internal static string JunkItemStatString = "";
        internal static bool TestingBackpack = false;


        // Stash mapper - it's an array representing every slot in your stash, true or false dictating if the slot is free or not

        private static bool[,] StashSlotBlocked = new bool[7, 30];

        // DateTime check to prevent inventory-check spam when looking for repairs being needed
        internal static DateTime LastCheckBackpackDurability = DateTime.UtcNow;
        private static DateTime _LastCompletedTownRun = DateTime.MinValue;


        internal static Vector3 PreTownRunPosition { get; set; }
        internal static int PreTownRunWorldId { get; set; }
        internal static bool WasVendoring { get; set; }

        /// <summary>
        /// Called from Plugin.Pulse
        /// </summary>
        internal static void VendorRunPulseCheck()
        {
            // If we're in town and vendoring
            if (ZetaDia.IsInTown && BrainBehavior.IsVendoring)
            {
                WasVendoring = true;
                Trinity.ForceVendorRunASAP = true;
            }
            if (!ZetaDia.IsInTown && !BrainBehavior.IsVendoring && WasVendoring)
            {

            }
        }

        /// <summary>
        /// Records the position when we first run out of bag space, so we can return to that same position after a town run
        /// </summary>
        internal static void SetPreTownRunPosition()
        {
            if (PreTownRunPosition == Vector3.Zero && PreTownRunWorldId == -1 && !Trinity.Player.IsInTown)
            {
                PreTownRunPosition = Trinity.Player.Position;
                PreTownRunWorldId = Trinity.Player.WorldID;
            }
        }

        /// <summary>
        /// TownRunCheckOverlord - determine if we should do a town-run or not
        /// </summary>
        /// <param name="ret"></param>
        /// <returns></returns>
        internal static bool TownRunCanRun(object ret)
        {
            using (new PerformanceLogger("TownRunOverlord"))
            {
                Trinity.IsReadyToTownRun = false;

                if (Trinity.Player.IsDead)
                    return false;

                if (DataDictionary.BossLevelAreaIDs.Contains(Trinity.Player.LevelAreaId))
                    return false;

                if (ZetaDia.IsInTown && DbProvider.DeathHandler.EquipmentNeedsEmergencyRepair())
                {
                    Logger.Log(TrinityLogLevel.Debug, LogCategory.GlobalHandler, "EquipmentNeedsEmergencyRepair!");
                    return true;
                }
                if (Trinity.IsReadyToTownRun && Trinity.CurrentTarget != null)
                {
                    TownRunCheckTimer.Reset();
                    return false;
                }

                // Check if we should be forcing a town-run
                if (Trinity.ForceVendorRunASAP || BrainBehavior.IsVendoring)
                {
                    if (!LastTownRunCheckResult)
                    {
                        if (BrainBehavior.IsVendoring)
                        {
                            Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "Looks like we are being asked to force a town-run by a profile/plugin/new DB feature, now doing so.");
                        }
                    }
                    SetPreTownRunPosition();
                    Trinity.IsReadyToTownRun = true;
                }

                // Time safety switch for more advanced town-run checking to prevent CPU spam
                if (DateTime.UtcNow.Subtract(LastCheckBackpackDurability).TotalSeconds > 6)
                {
                    LastCheckBackpackDurability = DateTime.UtcNow;

                    // Check for no space in backpack
                    Vector2 validLocation = Trinity.FindValidBackpackLocation(true);
                    if (validLocation.X < 0 || validLocation.Y < 0)
                    {
                        Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "No more space to pickup a 2-slot item, now running town-run routine.");
                        if (!LastTownRunCheckResult)
                        {
                            LastTownRunCheckResult = true;
                        }
                        Trinity.IsReadyToTownRun = true;

                        Trinity.ForceVendorRunASAP = true;
                        // Record the first position when we run out of bag space, so we can return later
                        TownRun.SetPreTownRunPosition();
                    }

                    var equippedItems = ZetaDia.Me.Inventory.Equipped.Where(i => i.DurabilityCurrent != i.DurabilityMax);
                    if (!equippedItems.Any())
                        return false;
                    double avg = equippedItems.Average(i => i.DurabilityPercent);

                    float threshold = Trinity.Player.IsInTown ? 0.50f : Zeta.Bot.Settings.CharacterSettings.Instance.RepairWhenDurabilityBelow;
                    bool needsRepair = avg <= threshold;

                    if (needsRepair)
                    {
                        Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "Items may need repair, now running town-run routine.");

                        Trinity.IsReadyToTownRun = true;
                        Trinity.ForceVendorRunASAP = true;
                        TownRun.SetPreTownRunPosition();
                    }         
                    
                }

                if (ErrorDialog.IsVisible)
                {
                    Trinity.IsReadyToTownRun = false;
                }

                LastTownRunCheckResult = Trinity.IsReadyToTownRun;

                // Clear blacklists to triple check any potential targets
                if (Trinity.IsReadyToTownRun)
                {
                    Trinity.hashRGUIDBlacklist3 = new HashSet<int>();
                    Trinity.hashRGUIDBlacklist15 = new HashSet<int>();
                    Trinity.hashRGUIDBlacklist60 = new HashSet<int>();
                    Trinity.hashRGUIDBlacklist90 = new HashSet<int>();
                }

                // Fix for A1 new game with bags full
                if (Trinity.Player.LevelAreaId == 19947 && ZetaDia.CurrentQuest.QuestSNO == 87700 && (ZetaDia.CurrentQuest.StepId == -1 || ZetaDia.CurrentQuest.StepId == 42))
                {
                    Trinity.IsReadyToTownRun = false;
                }

                // check for navigation obstacles (never TP near demonic forges, etc)
                if (CacheData.NavigationObstacles.Any(o => Vector3.Distance(o.Position, Trinity.Player.Position) < 90f))
                {
                    Trinity.IsReadyToTownRun = false;
                }

                if (Trinity.IsReadyToTownRun && !(BrainBehavior.IsVendoring || Trinity.Player.IsInTown))
                {
                    string cantUseTPreason;
                    if (!ZetaDia.Me.CanUseTownPortal(out cantUseTPreason) && !ZetaDia.IsInTown)
                    {
                        Logger.Log(TrinityLogLevel.Verbose, LogCategory.UserInformation, "It appears we need to town run but can't: {0}", cantUseTPreason);
                        Trinity.IsReadyToTownRun = false;
                    }
                }


                if ((Trinity.IsReadyToTownRun && TownRunTimerFinished()) || BrainBehavior.IsVendoring)
                {
                    return true;
                }
                if (Trinity.IsReadyToTownRun && !TownRunCheckTimer.IsRunning)
                {
                    TownRunCheckTimer.Start();
                    _loggedAnythingThisStash = false;
                    _loggedJunkThisStash = false;
                }
                return false;
            }
        }

        public static Composite TownRunWrapper(Composite original)
        {
            return
            new Sequence(
                original,
                new Action(delegate
                {
                    if (!BrainBehavior.IsVendoring)
                    {
                        Logger.Log("TownRun complete");
                        Trinity.IsReadyToTownRun = false;
                        Trinity.ForceVendorRunASAP = false;
                        TownRunCheckTimer.Reset();
                        SendEmailNotification();
                        SendMobileNotifications();
                    }
                    return RunStatus.Success;
                })
            );
        }

        internal static bool TownRunTimerFinished()
        {
            return ZetaDia.IsInTown || (TownRunCheckTimer.IsRunning && TownRunCheckTimer.ElapsedMilliseconds > 2000);
        }

        internal static bool TownRunTimerRunning()
        {
            return TownRunCheckTimer.IsRunning && TownRunCheckTimer.ElapsedMilliseconds < 2000;
        }

        private static bool lastTownPortalCheckResult = false;
        private static DateTime lastTownPortalCheckTime = DateTime.MinValue;

        /// <summary>
        /// Returns if we're trying to TownRun or if profile tag is UseTownPortalTag
        /// </summary>
        /// <returns></returns>
        internal static bool IsTryingToTownPortal()
        {
            if (DateTime.UtcNow.Subtract(lastTownPortalCheckTime).TotalMilliseconds < Trinity.Settings.Advanced.CacheRefreshRate)
                return lastTownPortalCheckResult;

            bool result = false;

            if (Trinity.IsReadyToTownRun)
                result = true;

            if (Trinity.ForceVendorRunASAP)
                result = true;

            if (TownRunCheckTimer.IsRunning)
                result = true;

            if (XmlTags.TrinityTownPortal.ForceClearArea)
                result = true;

            ProfileBehavior CurrentProfileBehavior = null;

            try
            {
                if (ProfileManager.CurrentProfileBehavior != null)
                    CurrentProfileBehavior = ProfileManager.CurrentProfileBehavior;
            }
            catch (Exception ex)
            {
                Logger.Log(LogCategory.UserInformation, "Exception while checking for TownPortal!");
                Logger.Log(LogCategory.GlobalHandler, ex.ToString());
            }
            if (ProfileManager.CurrentProfileBehavior != null)
            {
                Type profileBehaviortype = CurrentProfileBehavior.GetType();
                if (profileBehaviortype != null && (profileBehaviortype == typeof(UseTownPortalTag) || profileBehaviortype == typeof(WaitTimerTag) || profileBehaviortype == typeof(XmlTags.TrinityTownRun) || profileBehaviortype == typeof(XmlTags.TrinityTownPortal)))
                {
                    result = true;
                }
            }

            if (Zeta.Bot.Logic.BrainBehavior.IsVendoring)
                result = true;


            lastTownPortalCheckTime = DateTime.UtcNow;
            lastTownPortalCheckResult = result;
            return result;
        }

        internal static Stopwatch randomTimer = new Stopwatch();
        internal static Random timerRandomizer = new Random();
        internal static int randomTimerVal = -1;

        internal static void SetStartRandomTimer()
        {
            if (!randomTimer.IsRunning)
            {
                randomTimerVal = timerRandomizer.Next(500, 1500);
                randomTimer.Start();
            }
        }

        internal static void StopRandomTimer()
        {
            randomTimer.Reset();
        }

        internal static bool RandomTimerIsDone()
        {
            return (randomTimer.IsRunning && randomTimer.ElapsedMilliseconds >= randomTimerVal);
        }

        internal static bool RandomTimerIsNotDone()
        {
            return (randomTimer.IsRunning && randomTimer.ElapsedMilliseconds < randomTimerVal);
        }
                internal static SalvageOption GetSalvageOption(ItemQuality thisquality)
        {
            if (thisquality >= ItemQuality.Magic1 && thisquality <= ItemQuality.Magic3)
            {
                return Trinity.Settings.Loot.TownRun.SalvageBlueItemOption;
            }
            else if (thisquality >= ItemQuality.Rare4 && thisquality <= ItemQuality.Rare6)
            {
                return Trinity.Settings.Loot.TownRun.SalvageYellowItemOption;
            }
            else if (thisquality >= ItemQuality.Legendary)
            {
                return Trinity.Settings.Loot.TownRun.SalvageLegendaryItemOption;
            }
            return SalvageOption.None;
        }

        internal static Stopwatch TownRunCheckTimer = new Stopwatch();


        internal static void SendEmailNotification()
        {
            if (Trinity.Settings.Notification.MailEnabled && NotificationManager.EmailMessage.Length > 0)
                NotificationManager.SendEmail(
                    Trinity.Settings.Notification.EmailAddress,
                    Trinity.Settings.Notification.EmailAddress,
                    "New DB stash loot - " + FileManager.BattleTagName,
                    NotificationManager.EmailMessage.ToString(),
                    NotificationManager.SmtpServer,
                    Trinity.Settings.Notification.EmailPassword);
            NotificationManager.EmailMessage.Clear();
        }

        internal static void SendMobileNotifications()
        {
            while (NotificationManager.pushQueue.Count > 0)
            {
                NotificationManager.SendNotification(NotificationManager.pushQueue.Dequeue());
            }
        }


        /// <summary>
        /// Log the nice items we found and stashed
        /// </summary>
        internal static void LogGoodItems(CachedACDItem acdItem, GItemBaseType itemBaseType, GItemType itemType, double itemValue)
        {
            FileStream LogStream = null;
            try
            {
                string filePath = Path.Combine(FileManager.LoggingPath, "StashLog - " + Trinity.Player.ActorClass.ToString() + ".log");
                LogStream = File.Open(filePath, FileMode.Append, FileAccess.Write, FileShare.Read);

                //TODO : Change File Log writing
                using (StreamWriter LogWriter = new StreamWriter(LogStream))
                {
                    if (!_loggedAnythingThisStash)
                    {
                        _loggedAnythingThisStash = true;
                        LogWriter.WriteLine(DateTime.Now.ToString() + ":");
                        LogWriter.WriteLine("====================");
                    }
                    string sLegendaryString = "";
                    bool shouldSendNotifications = false;
                    if (acdItem.Quality >= ItemQuality.Legendary)
                    {
                        if (!Trinity.Settings.Notification.LegendaryScoring)
                            shouldSendNotifications = true;
                        else if (Trinity.Settings.Notification.LegendaryScoring && Trinity.CheckScoreForNotification(itemBaseType, itemValue))
                            shouldSendNotifications = true;
                        if (shouldSendNotifications)
                            NotificationManager.AddNotificationToQueue(acdItem.RealName + " [" + itemType.ToString() +
                                "] (Score=" + itemValue.ToString() + ". " + ValueItemStatString + ")",
                                ZetaDia.Service.Hero.Name + " new legendary!", ProwlNotificationPriority.Emergency);
                        sLegendaryString = " {legendary item}";

                        // Change made by bombastic
                        Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "+=+=+=+=+=+=+=+=+ LEGENDARY FOUND +=+=+=+=+=+=+=+=+");
                        Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "+  Name:       {0} ({1})", acdItem.RealName, itemType);
                        Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "+  Score:       {0:0}", itemValue);
                        Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "+  Attributes: {0}", ValueItemStatString);
                        Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+");
                    }
                    else
                    {

                        // Check for non-legendary notifications
                        shouldSendNotifications = Trinity.CheckScoreForNotification(itemBaseType, itemValue);
                        if (shouldSendNotifications)
                            NotificationManager.AddNotificationToQueue(acdItem.RealName + " [" + itemType.ToString() + "] (Score=" + itemValue.ToString() + ". " + ValueItemStatString + ")", ZetaDia.Service.Hero.Name + " new item!", ProwlNotificationPriority.Emergency);
                    }
                    if (shouldSendNotifications)
                    {
                        NotificationManager.EmailMessage.AppendLine(itemBaseType.ToString() + " - " + itemType.ToString() + " '" + acdItem.RealName + "'. Score = " + Math.Round(itemValue).ToString() + sLegendaryString)
                            .AppendLine("  " + ValueItemStatString)
                            .AppendLine();
                    }
                    LogWriter.WriteLine(itemBaseType.ToString() + " - " + itemType.ToString() + " '" + acdItem.RealName + "'. Score = " + Math.Round(itemValue).ToString() + sLegendaryString);
                    LogWriter.WriteLine("  " + ValueItemStatString);
                    LogWriter.WriteLine("");
                }
                LogStream.Close();
            }
            catch (IOException)
            {
                Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "Fatal Error: File access error for stash log file.");
                if (LogStream != null)
                    LogStream.Close();
            }
        }

        /// <summary>
        /// Log the rubbish junk items we salvaged or sold
        /// </summary>
        internal static void LogJunkItems(CachedACDItem acdItem, GItemBaseType itemBaseType, GItemType itemType, double itemValue)
        {
            FileStream LogStream = null;
            try
            {
                string filePath = Path.Combine(FileManager.LoggingPath, "JunkLog - " + Trinity.Player.ActorClass.ToString() + ".log");
                LogStream = File.Open(filePath, FileMode.Append, FileAccess.Write, FileShare.Read);
                using (StreamWriter LogWriter = new StreamWriter(LogStream))
                {
                    if (!_loggedJunkThisStash)
                    {
                        _loggedJunkThisStash = true;
                        LogWriter.WriteLine(DateTime.Now.ToString() + ":");
                        LogWriter.WriteLine("====================");
                    }
                    string isLegendaryItem = "";
                    if (acdItem.Quality >= ItemQuality.Legendary)
                        isLegendaryItem = " {legendary item}";
                    LogWriter.WriteLine(itemBaseType.ToString() + " - " + itemType.ToString() + " '" + acdItem.RealName + "'. Score = " + itemValue.ToString("0") + isLegendaryItem);
                    if (JunkItemStatString != "")
                        LogWriter.WriteLine("  " + JunkItemStatString);
                    else
                        LogWriter.WriteLine("  (no scorable attributes)");
                    LogWriter.WriteLine("");
                }
                LogStream.Close();
            }
            catch (IOException)
            {
                Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "Fatal Error: File access error for junk log file.");
                if (LogStream != null)
                    LogStream.Close();
            }
        }


        internal static Vector2 SortingFindLocationStash(bool isOriginalTwoSlot, bool endOfStash = false)
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
            //ZetaDia.Actors.Update();

            // Check we can get the player dynamic ID
            int iPlayerDynamicID = -1;
            try
            {
                iPlayerDynamicID = Trinity.Player.MyDynamicID;
            }
            catch
            {
                Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "Failure getting your player data from DemonBuddy, abandoning the sort!");
                return;
            }
            if (iPlayerDynamicID == -1)
            {
                Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "Failure getting your player data, abandoning the sort!");
                return;
            }

            // List used for all the sorting
            List<StashSortItem> listSortMyStash = new List<StashSortItem>();

            // Map out the backpack free slots
            for (int row = 0; row <= 5; row++)
                for (int col = 0; col <= 9; col++)
                    Trinity.BackpackSlotBlocked[col, row] = false;

            foreach (ACDItem item in ZetaDia.Me.Inventory.Backpack)
            {
                int inventoryRow = item.InventoryRow;
                int inventoryColumn = item.InventoryColumn;

                // Mark this slot as not-free
                Trinity.BackpackSlotBlocked[inventoryColumn, inventoryRow] = true;

                // Try and reliably find out if this is a two slot item or not
                if (item.IsTwoSquareItem && inventoryRow < 5)
                {
                    Trinity.BackpackSlotBlocked[inventoryColumn, inventoryRow + 1] = true;
                }
            }

            // Map out the stash free slots
            for (int iRow = 0; iRow <= 29; iRow++)
                for (int iColumn = 0; iColumn <= 6; iColumn++)
                    StashSlotBlocked[iColumn, iRow] = false;

            // Block off the entire of any "protected stash pages"
            foreach (int iProtPage in Zeta.Bot.Settings.CharacterSettings.Instance.ProtectedStashPages)
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
                GItemType itemType = Trinity.DetermineItemType(item.InternalName, item.ItemType, item.FollowerSpecialType);

                if (item.IsTwoSquareItem && inventoryRow != 19 && inventoryRow != 9 && inventoryRow != 29)
                {
                    StashSlotBlocked[inventoryColumn, inventoryRow + 1] = true;
                }
                else if (item.IsTwoSquareItem && (inventoryRow == 19 || inventoryRow == 9 || inventoryRow == 29))
                {
                    Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "WARNING: There was an error reading your stash, abandoning the process.");
                    Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "Always make sure you empty your backpack, open the stash, then RESTART DEMONBUDDY before sorting!");
                    return;
                }
                CachedACDItem thiscacheditem = new CachedACDItem(item, item.InternalName, item.Name, item.Level, item.ItemQualityLevel, item.Gold, item.GameBalanceId,
                    item.DynamicId, item.Stats.WeaponDamagePerSecond, item.IsOneHand, item.IsTwoHand, item.DyeType, item.ItemType, item.ItemBaseType, item.FollowerSpecialType,
                    item.IsUnidentified, item.ItemStackQuantity, item.Stats);

                double ItemValue = ItemValuation.ValueThisItem(thiscacheditem, itemType);
                double NeedScore = Trinity.ScoreNeeded(item.ItemBaseType);


                // Ignore stackable items
                // TODO check if item.MaxStackCount is 0 on non stackable items or 1
                if (!(item.MaxStackCount > 1) && itemType != GItemType.StaffOfHerding)
                {
                    listSortMyStash.Add(new StashSortItem(((ItemValue / NeedScore) * 1000), 1, inventoryColumn, inventoryRow, item.DynamicId, item.IsTwoSquareItem));
                }
            }


            // Sort the items in the stash by their row number, lowest to highest
            listSortMyStash.Sort((p1, p2) => p1.InventoryRow.CompareTo(p2.InventoryRow));

            // Now move items into your backpack until full, then into the END of the stash
            Vector2 vFreeSlot;

            // Loop through all stash items
            foreach (StashSortItem thisstashsort in listSortMyStash)
            {
                vFreeSlot = Trinity.SortingFindLocationBackpack(thisstashsort.IsTwoSlot);
                int iStashOrPack = 1;
                if (vFreeSlot.X == -1 || vFreeSlot.Y == -1)
                {
                    vFreeSlot = SortingFindLocationStash(thisstashsort.IsTwoSlot, true);
                    if (vFreeSlot.X == -1 || vFreeSlot.Y == -1)
                        continue;
                    iStashOrPack = 2;
                }
                if (iStashOrPack == 1)
                {
                    ZetaDia.Me.Inventory.MoveItem(thisstashsort.DynamicID, iPlayerDynamicID, InventorySlot.BackpackItems, (int)vFreeSlot.X, (int)vFreeSlot.Y);
                    StashSlotBlocked[thisstashsort.InventoryColumn, thisstashsort.InventoryRow] = false;
                    if (thisstashsort.IsTwoSlot)
                        StashSlotBlocked[thisstashsort.InventoryColumn, thisstashsort.InventoryRow + 1] = false;
                    Trinity.BackpackSlotBlocked[(int)vFreeSlot.X, (int)vFreeSlot.Y] = true;
                    if (thisstashsort.IsTwoSlot)
                        Trinity.BackpackSlotBlocked[(int)vFreeSlot.X, (int)vFreeSlot.Y + 1] = true;
                    thisstashsort.InventoryColumn = (int)vFreeSlot.X;
                    thisstashsort.InventoryRow = (int)vFreeSlot.Y;
                    thisstashsort.StashOrPack = 2;
                }
                else
                {
                    ZetaDia.Me.Inventory.MoveItem(thisstashsort.DynamicID, iPlayerDynamicID, InventorySlot.SharedStash, (int)vFreeSlot.X, (int)vFreeSlot.Y);
                    StashSlotBlocked[thisstashsort.InventoryColumn, thisstashsort.InventoryRow] = false;
                    if (thisstashsort.IsTwoSlot)
                        StashSlotBlocked[thisstashsort.InventoryColumn, thisstashsort.InventoryRow + 1] = false;
                    StashSlotBlocked[(int)vFreeSlot.X, (int)vFreeSlot.Y] = true;
                    if (thisstashsort.IsTwoSlot)
                        StashSlotBlocked[(int)vFreeSlot.X, (int)vFreeSlot.Y + 1] = true;
                    thisstashsort.InventoryColumn = (int)vFreeSlot.X;
                    thisstashsort.InventoryRow = (int)vFreeSlot.Y;
                    thisstashsort.StashOrPack = 1;
                }
                Thread.Sleep(150);
            }

            // Now sort the items by their score, highest to lowest
            listSortMyStash.Sort((p1, p2) => p1.Score.CompareTo(p2.Score));
            listSortMyStash.Reverse();

            // Now fill the stash in ordered-order
            foreach (StashSortItem thisstashsort in listSortMyStash)
            {
                vFreeSlot = SortingFindLocationStash(thisstashsort.IsTwoSlot, false);
                if (vFreeSlot.X == -1 || vFreeSlot.Y == -1)
                {
                    Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "Failure trying to put things back into stash, no stash slots free? Abandoning...");
                    return;
                }
                ZetaDia.Me.Inventory.MoveItem(thisstashsort.DynamicID, iPlayerDynamicID, InventorySlot.SharedStash, (int)vFreeSlot.X, (int)vFreeSlot.Y);
                if (thisstashsort.StashOrPack == 1)
                {
                    StashSlotBlocked[thisstashsort.InventoryColumn, thisstashsort.InventoryRow] = false;
                    if (thisstashsort.IsTwoSlot)
                        StashSlotBlocked[thisstashsort.InventoryColumn, thisstashsort.InventoryRow + 1] = false;
                }
                else
                {
                    Trinity.BackpackSlotBlocked[thisstashsort.InventoryColumn, thisstashsort.InventoryRow] = false;
                    if (thisstashsort.IsTwoSlot)
                        Trinity.BackpackSlotBlocked[thisstashsort.InventoryColumn, thisstashsort.InventoryRow + 1] = false;
                }
                StashSlotBlocked[(int)vFreeSlot.X, (int)vFreeSlot.Y] = true;
                if (thisstashsort.IsTwoSlot)
                    StashSlotBlocked[(int)vFreeSlot.X, (int)vFreeSlot.Y + 1] = true;
                thisstashsort.StashOrPack = 1;
                thisstashsort.InventoryRow = (int)vFreeSlot.Y;
                thisstashsort.InventoryColumn = (int)vFreeSlot.X;
                Thread.Sleep(150);
            }
            Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "Stash sorted!");
        }

    }
}
