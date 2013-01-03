using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GilesTrinity.Settings.Loot;
using Zeta.Internals.Actors;
using GilesTrinity;
using GilesTrinity.Technicals;
using Zeta.TreeSharp;
using Zeta.Common;
using Zeta;
using Zeta.CommonBot;
using System.IO;
using System.Globalization;
using GilesTrinity.Notifications;
using Zeta.Internals.Actors.Gizmos;
using Zeta.Internals;
using Action = Zeta.TreeSharp.Action;
using System.Threading;
using System.Diagnostics;

namespace GilesTrinity
{
    internal class TownRun
    {
        // Whether salvage/sell run should go to a middle-waypoint first to help prevent stucks
        private static bool bGoToSafetyPointFirst = false;
        private static bool bGoToSafetyPointSecond = false;
        private static bool bReachedSafety = false;
        internal static bool bLastTownRunCheckResult = false;
        // Random variables used during item handling and town-runs
        private static int itemDelayLoopLimit = 0;
        private static int currentItemLoops = 0;
        private static bool loggedAnythingThisStash = false;
        private static bool updatedStashMap = false;
        private static bool loggedJunkThisStash = false;
        internal static string ValueItemStatString = "";
        internal static string junkItemStatString = "";
        internal static bool testingBackpack = false;
        // Safety pauses to make sure we aren't still coming through the portal or selling
        internal static bool bPreStashPauseDone = false;
        internal static double iPreStashLoops = 0;
        internal static bool GilesPreStashPauseOverlord(object ret)
        {
            return (!bPreStashPauseDone);
        }
        internal static RunStatus GilesStashPrePause(object ret)
        {
            bPreStashPauseDone = true;
            iPreStashLoops = 0;
            return RunStatus.Success;
        }
        internal static RunStatus GilesStashPause(object ret)
        {
            iPreStashLoops++;
            if (iPreStashLoops < 30)
                return RunStatus.Running;
            return RunStatus.Success;
        }
        internal static HashSet<GilesCachedACDItem> hashGilesCachedKeepItems = new HashSet<GilesCachedACDItem>();
        internal static HashSet<GilesCachedACDItem> hashGilesCachedSalvageItems = new HashSet<GilesCachedACDItem>();
        internal static HashSet<GilesCachedACDItem> hashGilesCachedSellItems = new HashSet<GilesCachedACDItem>();
        // Stash mapper - it's an array representing every slot in your stash, true or false dictating if the slot is free or not
        private static bool[,] StashSlotBlocked = new bool[7, 30];
        internal static float iLowestDurabilityFound = -1;
        internal static bool bNeedsEquipmentRepairs = false;
        // DateTime check to prevent inventory-check spam when looking for repairs being needed
        internal static DateTime timeLastAttemptedTownRun = DateTime.Now;
        internal static bool bCurrentlyMoving = false;
        internal static bool bReachedDestination = false;
        // The distance last loop, so we can compare to current distance to work out if we moved
        internal static float lastDistance = 0f;
        // This dictionary stores attempted stash counts on items, to help detect any stash stucks on the same item etc.
        internal static Dictionary<int, int> _dictItemStashAttempted = new Dictionary<int, int>();

        /// <summary>
        /// TownRunCheckOverlord - determine if we should do a town-run or not
        /// </summary>
        /// <param name="ret"></param>
        /// <returns></returns>
        internal static bool TownRunCanRun(object ret)
        {
            using (new PerformanceLogger("TownRunOverlord"))
            {
                GilesTrinity.bWantToTownRun = false;

                if (GilesTrinity.BossLevelAreaIDs.Contains(GilesTrinity.playerStatus.LevelAreaId))
                    return false;

                if (GilesTrinity.bWantToTownRun && GilesTrinity.CurrentTarget != null)
                {
                    TownRunCheckTimer.Reset();
                    return false;
                }

                // Check if we should be forcing a town-run
                if (GilesTrinity.ForceVendorRunASAP || Zeta.CommonBot.Logic.BrainBehavior.IsVendoring)
                {
                    if (!TownRun.bLastTownRunCheckResult)
                    {
                        bPreStashPauseDone = false;
                        if (Zeta.CommonBot.Logic.BrainBehavior.IsVendoring)
                        {
                            DbHelper.Log(TrinityLogLevel.Normal, LogCategory.UserInformation, "Looks like we are being asked to force a town-run by a profile/plugin/new DB feature, now doing so.");
                        }
                    }
                    GilesTrinity.bWantToTownRun = true;
                }

                // Time safety switch for more advanced town-run checking to prevent CPU spam
                else if (DateTime.Now.Subtract(timeLastAttemptedTownRun).TotalSeconds > 6)
                {
                    timeLastAttemptedTownRun = DateTime.Now;

                    // Check for no space in backpack
                    Vector2 ValidLocation = GilesTrinity.FindValidBackpackLocation(true);
                    if (ValidLocation.X < 0 || ValidLocation.Y < 0)
                    {
                        DbHelper.Log(TrinityLogLevel.Normal, LogCategory.UserInformation, "No more space to pickup a 2-slot item, now running town-run routine.");
                        if (!bLastTownRunCheckResult)
                        {
                            bPreStashPauseDone = false;
                            bLastTownRunCheckResult = true;
                        }
                        GilesTrinity.bWantToTownRun = true;
                    }

                    // Check durability percentages
                    foreach (ACDItem tempitem in ZetaDia.Me.Inventory.Equipped)
                    {
                        if (tempitem.BaseAddress != IntPtr.Zero)
                        {
                            if (tempitem.DurabilityPercent <= Zeta.CommonBot.Settings.CharacterSettings.Instance.RepairWhenDurabilityBelow)
                            {
                                DbHelper.Log(TrinityLogLevel.Normal, LogCategory.UserInformation, "Items may need repair, now running town-run routine.");
                                if (!bLastTownRunCheckResult)
                                {
                                    bPreStashPauseDone = false;
                                }
                                GilesTrinity.bWantToTownRun = true;
                            }
                        }
                    }
                }

                if (Zeta.CommonBot.ErrorDialog.IsVisible)
                {
                    GilesTrinity.bWantToTownRun = false;
                }

                bLastTownRunCheckResult = GilesTrinity.bWantToTownRun;

                // Clear blacklists to triple check any potential targets
                if (GilesTrinity.bWantToTownRun)
                {
                    GilesTrinity.hashRGUIDBlacklist3 = new HashSet<int>();
                    GilesTrinity.hashRGUIDBlacklist15 = new HashSet<int>();
                    GilesTrinity.hashRGUIDBlacklist60 = new HashSet<int>();
                    GilesTrinity.hashRGUIDBlacklist90 = new HashSet<int>();
                }

                // Fix for A1 new game with bags full
                if (GilesTrinity.playerStatus.LevelAreaId == 19947 && ZetaDia.CurrentQuest.QuestSNO == 87700 && (ZetaDia.CurrentQuest.StepId == -1 || ZetaDia.CurrentQuest.StepId == 42))
                {
                    GilesTrinity.bWantToTownRun = false;
                }

                // check for navigation obstacles (never TP near demonic forges, etc)
                if (GilesTrinity.hashNavigationObstacleCache.Any(o => Vector3.Distance(o.Location, GilesTrinity.playerStatus.CurrentPosition) < 40f))
                {
                    GilesTrinity.bWantToTownRun = false;
                }

                if ((GilesTrinity.bWantToTownRun && TownRunCheckTimer.IsRunning && TownRunCheckTimer.ElapsedMilliseconds > 2000) || Zeta.CommonBot.Logic.BrainBehavior.IsVendoring)
                    return true;
                else if (GilesTrinity.bWantToTownRun && !TownRunCheckTimer.IsRunning)
                    TownRunCheckTimer.Start();

                return false;
            }
        }


        /// <summary>
        /// Randomize the timer between stashing/salvaging etc.
        /// </summary>
        internal static void RandomizeTheTimer()
        {
            Random rndNum = new Random(int.Parse(Guid.NewGuid().ToString().Substring(0, 8), NumberStyles.HexNumber));
            int rnd = rndNum.Next(7);
            itemDelayLoopLimit = 4 + rnd + ((int)Math.Floor(((double)(BotMain.TicksPerSecond / 2))));
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

        /// <summary>
        /// Sell Validation - Determines what should or should not be sold to vendor
        /// </summary>
        /// <param name="thisinternalname"></param>
        /// <param name="thislevel"></param>
        /// <param name="thisquality"></param>
        /// <param name="thisdbitemtype"></param>
        /// <param name="thisfollowertype"></param>
        /// <returns></returns>
        internal static bool GilesSellValidation(string thisinternalname, int thislevel, ItemQuality thisquality, ItemType thisdbitemtype, FollowerType thisfollowertype)
        {

            // Check this isn't something we want to salvage
            if (GilesSalvageValidation(thisinternalname, thislevel, thisquality, thisdbitemtype, thisfollowertype))
                return false;

            GItemType thisGilesItemType = GilesTrinity.DetermineItemType(thisinternalname, thisdbitemtype, thisfollowertype);
            GItemBaseType thisGilesBaseType = GilesTrinity.DetermineBaseType(thisGilesItemType);
            switch (thisGilesBaseType)
            {
                case GItemBaseType.WeaponRange:
                case GItemBaseType.WeaponOneHand:
                case GItemBaseType.WeaponTwoHand:
                case GItemBaseType.Armor:
                case GItemBaseType.Offhand:
                case GItemBaseType.Jewelry:
                case GItemBaseType.FollowerItem:
                    return true;
                case GItemBaseType.Gem:
                case GItemBaseType.Misc:
                    if (thisGilesItemType == GItemType.CraftingPlan)
                        return true;
                    else
                        return false;
                case GItemBaseType.Unknown:
                    return false;
            }

            // Switch giles base item type
            return false;
        }

        /// <summary>
        /// Salvage Validation - Determines what should or should not be salvaged
        /// </summary>
        /// <param name="thisinternalname"></param>
        /// <param name="thislevel"></param>
        /// <param name="thisquality"></param>
        /// <param name="thisdbitemtype"></param>
        /// <param name="thisfollowertype"></param>
        /// <returns></returns>
        internal static bool GilesSalvageValidation(string thisinternalname, int thislevel, ItemQuality thisquality, ItemType thisdbitemtype, FollowerType thisfollowertype)
        {
            GItemType thisGilesItemType = GilesTrinity.DetermineItemType(thisinternalname, thisdbitemtype, thisfollowertype);
            GItemBaseType thisGilesBaseType = GilesTrinity.DetermineBaseType(thisGilesItemType);

            // Take Salvage Option corresponding to ItemLevel
            SalvageOption salvageOption = GetSalvageOption(thisquality);

            if (thisquality >= ItemQuality.Legendary && salvageOption == SalvageOption.InfernoOnly && thislevel >= 60)
                return true;

            switch (thisGilesBaseType)
            {
                case GItemBaseType.WeaponRange:
                case GItemBaseType.WeaponOneHand:
                case GItemBaseType.WeaponTwoHand:
                case GItemBaseType.Armor:
                case GItemBaseType.Offhand:
                    return ((thislevel >= 61 && salvageOption == SalvageOption.InfernoOnly) || salvageOption == SalvageOption.All);
                case GItemBaseType.Jewelry:
                    return ((thislevel >= 59 && salvageOption == SalvageOption.InfernoOnly) || salvageOption == SalvageOption.All);
                case GItemBaseType.FollowerItem:
                    return ((thislevel >= 60 && salvageOption == SalvageOption.InfernoOnly) || salvageOption == SalvageOption.All);
                case GItemBaseType.Gem:
                case GItemBaseType.Misc:
                case GItemBaseType.Unknown:
                    return false;
            }

            // Switch giles base item type
            return false;
        }

        internal static SalvageOption GetSalvageOption(ItemQuality thisquality)
        {
            if (thisquality >= ItemQuality.Magic1 && thisquality <= ItemQuality.Magic3)
            {
                return GilesTrinity.Settings.Loot.TownRun.SalvageBlueItemOption;
            }
            else if (thisquality >= ItemQuality.Rare4 && thisquality <= ItemQuality.Rare6)
            {
                return GilesTrinity.Settings.Loot.TownRun.SalvageYellowItemOption;
            }
            else if (thisquality >= ItemQuality.Legendary)
            {
                return GilesTrinity.Settings.Loot.TownRun.SalvageLegendaryItemOption;
            }
            return SalvageOption.None;
        }

        internal static Stopwatch TownRunCheckTimer = new Stopwatch();

        /// <summary>
        /// Stash Overlord values all items and checks if we have anything to stash
        /// </summary>
        /// <param name="ret"></param>
        /// <returns></returns>
        internal static bool GilesStashOverlord(object ret)
        {
            hashGilesCachedKeepItems = new HashSet<GilesCachedACDItem>();
            bNeedsEquipmentRepairs = false;
            GilesTrinity.ForceVendorRunASAP = false;
            bool bShouldVisitStash = false;

            TownRunCheckTimer.Reset();

            foreach (ACDItem thisitem in ZetaDia.Me.Inventory.Backpack)
            {
                if (thisitem.BaseAddress != IntPtr.Zero)
                {

                    // Find out if this item's in a protected bag slot
                    if (!ItemManager.ItemIsProtected(thisitem) && !GilesTrinity.weaponSwap.SwapperUsing(thisitem))
                    {
                        // test
                        DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.ScriptRule, "DEBUG: {0},{1},{2}", thisitem.InternalName, thisitem.Name, thisitem.Level);
                        GilesCachedACDItem thiscacheditem = new GilesCachedACDItem(thisitem, thisitem.InternalName, thisitem.Name, thisitem.Level, thisitem.ItemQualityLevel, thisitem.Gold, thisitem.GameBalanceId,
                            thisitem.DynamicId, thisitem.Stats.WeaponDamagePerSecond, thisitem.IsOneHand, thisitem.IsTwoHand, thisitem.DyeType, thisitem.ItemType, thisitem.ItemBaseType, thisitem.FollowerSpecialType,
                            thisitem.IsUnidentified, thisitem.ItemStackQuantity, thisitem.Stats);

                        bool bShouldStashThis = GilesTrinity.Settings.Loot.ItemFilterMode != ItemFilterMode.DemonBuddy ? GilesTrinity.ShouldWeStashThis(thiscacheditem) : ItemManager.ShouldStashItem(thisitem);

                        if (bShouldStashThis)
                        {
                            hashGilesCachedKeepItems.Add(thiscacheditem);
                            bShouldVisitStash = true;
                        }
                    }
                }
                else
                {
                    DbHelper.Log(TrinityLogLevel.Normal, LogCategory.UserInformation, "GSError: Diablo 3 memory read error, or item became invalid [StashOver-1]", true);
                }
            }
            return bShouldVisitStash;
        }

        /// <summary>
        /// Pre Stash prepares stuff for our stash run
        /// </summary>
        /// <param name="ret"></param>
        /// <returns></returns>
        internal static RunStatus GilesOptimisedPreStash(object ret)
        {
            if (GilesTrinity.Settings.Advanced.DebugInStatusBar)
                BotMain.StatusText = "Town run: Stash routine started";
            DbHelper.Log(TrinityLogLevel.Debug, LogCategory.UserInformation, "GSDebug: Stash routine started.");
            loggedAnythingThisStash = false;
            updatedStashMap = false;
            currentItemLoops = 0;
            RandomizeTheTimer();
            return RunStatus.Success;
        }

        /// <summary>
        /// Post Stash tidies up and signs off log file after a stash run
        /// </summary>
        /// <param name="ret"></param>
        /// <returns></returns>
        internal static RunStatus GilesOptimisedPostStash(object ret)
        {
            DbHelper.Log(TrinityLogLevel.Debug, LogCategory.UserInformation, "GSDebug: Stash routine ending sequence...");

            // Lock memory (probably not actually necessary anymore, since we handle all item stuff ourselves!?)
            //using (ZetaDia.Memory.AcquireFrame())
            //{
            //    ZetaDia.Actors.Update();
            //}
            if (loggedAnythingThisStash)
            {
                FileStream LogStream = null;
                try
                {
                    LogStream = File.Open(Path.Combine(FileManager.LoggingPath, "StashLog - " + ZetaDia.Actors.Me.ActorClass.ToString() + ".log"), FileMode.Append, FileAccess.Write, FileShare.Read);
                    using (StreamWriter LogWriter = new StreamWriter(LogStream))
                        LogWriter.WriteLine("");
                    LogStream.Close();
                    if (GilesTrinity.Settings.Notification.MailEnabled && NotificationManager.EmailMessage.Length > 0)
                        NotificationManager.SendEmail(
                            GilesTrinity.Settings.Notification.EmailAddress,
                            GilesTrinity.Settings.Notification.EmailAddress,
                            "New DB stash loot - " + ZetaDia.Service.CurrentHero.BattleTagName,
                            NotificationManager.EmailMessage.ToString(),
                            NotificationManager.SmtpServer,
                            GilesTrinity.Settings.Notification.EmailPassword);
                    NotificationManager.EmailMessage.Clear();

                    // Send notification to IPhone & Android
                    while (NotificationManager.pushQueue.Count > 0)
                    {
                        NotificationManager.SendNotification(NotificationManager.pushQueue.Dequeue());
                    }
                }
                catch (IOException)
                {
                    DbHelper.Log(TrinityLogLevel.Error, LogCategory.UserInformation, "Fatal Error: File access error for signing off the stash log file.");
                    if (LogStream != null)
                        LogStream.Close();
                }
                loggedAnythingThisStash = false;
            }
            DbHelper.Log(TrinityLogLevel.Debug, LogCategory.UserInformation, "GSDebug: Stash routine finished.");
            return RunStatus.Success;
        }

        /// <summary>
        /// Lovely smooth one-at-a-time stashing routine
        /// </summary>
        /// <param name="ret"></param>
        /// <returns></returns>
        internal static RunStatus GilesOptimisedStash(object ret)
        {
            /*
             *  Move to Stash
             */
            //ZetaDia.Actors.Update();
            if (ZetaDia.Actors.Me == null)
            {
                DbHelper.Log(TrinityLogLevel.Debug, LogCategory.UserInformation, "GSError: Diablo 3 memory read error, or item became invalid [CoreStash-1]");
                return RunStatus.Failure;
            }
            Vector3 vectorPlayerPosition = ZetaDia.Me.Position;
            Vector3 vectorStashLocation = new Vector3(0f, 0f, 0f);
            DiaObject objPlayStash = ZetaDia.Actors.GetActorsOfType<GizmoPlayerSharedStash>(true).FirstOrDefault<GizmoPlayerSharedStash>();

            if (objPlayStash != null)
            {
                vectorStashLocation = objPlayStash.Position;
            }
            else
                switch (ZetaDia.CurrentAct)
                {
                    case Act.A1:
                        vectorStashLocation = new Vector3(2971.285f, 2798.801f, 24.04533f); break;
                    case Act.A2:
                        vectorStashLocation = new Vector3(323.4543f, 228.5806f, 0.1f); break;
                    case Act.A3:
                    case Act.A4:
                        vectorStashLocation = new Vector3(389.3798f, 390.7143f, 0.3321428f); break;
                }
            float iDistanceFromStash = Vector3.Distance(vectorPlayerPosition, vectorStashLocation);
            if (iDistanceFromStash > 120f)
                return RunStatus.Failure;


            if (iDistanceFromStash > 7f)
            {
                ZetaDia.Me.UsePower(SNOPower.Walk, vectorStashLocation, ZetaDia.Me.WorldDynamicId);
                return RunStatus.Running;
            }
            if (objPlayStash == null)
                return RunStatus.Failure;
            /*
             *  Interact with Stash
             */
            if (!UIElements.StashWindow.IsVisible)
            {
                objPlayStash.Interact();
                return RunStatus.Running;
            }

            /*
             *  Get XY Map of used/blocked stash cells
             */
            if (!updatedStashMap)
            {

                // Array for what blocks are or are not blocked
                for (int iRow = 0; iRow <= 29; iRow++)
                {
                    for (int iColumn = 0; iColumn <= 6; iColumn++)
                    {
                        StashSlotBlocked[iColumn, iRow] = false;
                    }
                }
                // Block off the entire of any "protected stash pages"
                foreach (int iProtPage in Zeta.CommonBot.Settings.CharacterSettings.Instance.ProtectedStashPages)
                {
                    for (int iProtRow = 0; iProtRow <= 9; iProtRow++)
                    {
                        for (int iProtColumn = 0; iProtColumn <= 6; iProtColumn++)
                        {
                            StashSlotBlocked[iProtColumn, iProtRow + (iProtPage * 10)] = true;
                        }
                    }
                }
                // Remove rows we don't have
                for (int iRow = (ZetaDia.Me.NumSharedStashSlots / 7); iRow <= 29; iRow++)
                {
                    for (int iColumn = 0; iColumn <= 6; iColumn++)
                    {
                        StashSlotBlocked[iColumn, iRow] = true;
                    }
                }
                // Map out all the items already in the stash
                foreach (ACDItem tempitem in ZetaDia.Me.Inventory.StashItems)
                {
                    if (tempitem.BaseAddress != IntPtr.Zero)
                    {
                        int inventoryRow = tempitem.InventoryRow;
                        int inventoryColumn = tempitem.InventoryColumn;

                        // Mark this slot as not-free
                        StashSlotBlocked[inventoryColumn, inventoryRow] = true;

                        // Try and reliably find out if this is a two slot item or not
                        GItemType tempItemType = GilesTrinity.DetermineItemType(tempitem.InternalName, tempitem.ItemType, tempitem.FollowerSpecialType);
                        if (GilesTrinity.DetermineIsTwoSlot(tempItemType) && inventoryRow != 9 && inventoryRow != 19 && inventoryRow != 29)
                        {
                            StashSlotBlocked[inventoryColumn, inventoryRow + 1] = true;
                        }
                        else if (GilesTrinity.DetermineIsTwoSlot(tempItemType) && (inventoryRow == 9 || inventoryRow == 19 || inventoryRow == 29))
                        {
                            DbHelper.Log(TrinityLogLevel.Debug, LogCategory.UserInformation,
                                "GSError: DemonBuddy thinks this item is 2 slot even though it's at bottom row of a stash page: {0} [{1}] type={2} @ slot {3}/{4}",
                                tempitem.Name,
                                tempitem.InternalName,
                                tempItemType,
                                (inventoryRow + 1),
                                (inventoryColumn + 1));
                        }
                    }
                }

                // Loop through all stash items
                updatedStashMap = true;
            }

            // Need to update the stash map?
            if (hashGilesCachedKeepItems.Count > 0)
            {
                currentItemLoops++;
                if (currentItemLoops < itemDelayLoopLimit)
                    return RunStatus.Running;
                currentItemLoops = 0;
                RandomizeTheTimer();
                GilesCachedACDItem thisitem = hashGilesCachedKeepItems.FirstOrDefault();

                bool bDidStashSucceed = GilesStashAttempt(thisitem);

                if (!bDidStashSucceed)
                    DbHelper.Log(TrinityLogLevel.Debug, LogCategory.UserInformation, "There was an unknown error stashing an item.");
                if (thisitem != null)
                    hashGilesCachedKeepItems.Remove(thisitem);
                if (hashGilesCachedKeepItems.Count > 0)
                    return RunStatus.Running;
            }
            return RunStatus.Success;
        }

        /// <summary>
        /// Sell Overlord - determines if we should visit the vendor for repairs or selling
        /// </summary>
        /// <param name="ret"></param>
        /// <returns></returns>
        internal static bool GilesSellOverlord(object ret)
        {
            GilesTrinity.ForceVendorRunASAP = false;
            hashGilesCachedSellItems = new HashSet<GilesCachedACDItem>();
            bool bShouldVisitVendor = false;

            // Check durability percentages
            iLowestDurabilityFound = -1;
            foreach (ACDItem tempitem in ZetaDia.Me.Inventory.Equipped)
            {
                if (tempitem.BaseAddress != IntPtr.Zero)
                {
                    if (tempitem.DurabilityPercent <= Zeta.CommonBot.Settings.CharacterSettings.Instance.RepairWhenDurabilityBelow)
                    {
                        iLowestDurabilityFound = tempitem.DurabilityPercent;
                        bNeedsEquipmentRepairs = true;
                        bShouldVisitVendor = true;
                    }
                }
            }

            ACDItem thisBestPotion = ZetaDia.Me.Inventory.Backpack.Where(i => i.IsPotion).OrderByDescending(p => p.HitpointsGranted).FirstOrDefault();
            // Check for anything to sell
            foreach (ACDItem thisitem in ZetaDia.Me.Inventory.Backpack)
            {
                if (thisitem.BaseAddress != IntPtr.Zero)
                {
                    if (!ItemManager.ItemIsProtected(thisitem) && !GilesTrinity.weaponSwap.SwapperUsing(thisitem))
                    {
                        GilesCachedACDItem thiscacheditem = new GilesCachedACDItem(thisitem, thisitem.InternalName, thisitem.Name, thisitem.Level, thisitem.ItemQualityLevel, thisitem.Gold, thisitem.GameBalanceId,
                            thisitem.DynamicId, thisitem.Stats.WeaponDamagePerSecond, thisitem.IsOneHand, thisitem.IsTwoHand, thisitem.DyeType, thisitem.ItemType, thisitem.ItemBaseType, thisitem.FollowerSpecialType,
                            thisitem.IsUnidentified, thisitem.ItemStackQuantity, thisitem.Stats);
                        thiscacheditem.Row = thisitem.InventoryRow;
                        thiscacheditem.Column = thisitem.InventoryColumn;
                        bool bShouldSellThis = GilesTrinity.Settings.Loot.ItemFilterMode != ItemFilterMode.DemonBuddy
                            ? GilesSellValidation(thiscacheditem.InternalName, thiscacheditem.Level, thiscacheditem.Quality, thiscacheditem.DBItemType, thiscacheditem.FollowerType)
                            : ItemManager.ShouldSellItem(thisitem);

                        // if it has gems, always salvage
                        if (thisitem.NumSocketsFilled > 0)
                        {
                            bShouldSellThis = false;
                        }

                        // Don't sell stuff that we want to salvage, if using custom loot-rules
                        if (GilesTrinity.Settings.Loot.ItemFilterMode == ItemFilterMode.DemonBuddy && ItemManager.ShouldSalvageItem(thisitem))
                        {
                            bShouldSellThis = false;
                        }

                        // Sell potions that aren't best quality
                        if (thisitem.IsPotion && thisitem.GameBalanceId != thisBestPotion.GameBalanceId)
                        {
                            bShouldSellThis = true;
                        }

                        if (bShouldSellThis)
                        {
                            hashGilesCachedSellItems.Add(thiscacheditem);
                            bShouldVisitVendor = true;
                        }
                    }
                }
                else
                {
                    DbHelper.Log(TrinityLogLevel.Debug, LogCategory.UserInformation, "GSError: Diablo 3 memory read error, or item became invalid [SellOver-1]");
                }
            }
            return bShouldVisitVendor;
        }

        /// <summary>
        /// Pre Sell sets everything up ready for running to vendor
        /// </summary>
        /// <param name="ret"></param>
        /// <returns></returns>
        internal static RunStatus GilesOptimisedPreSell(object ret)
        {
            if (GilesTrinity.Settings.Advanced.DebugInStatusBar)
                BotMain.StatusText = "Town run: Sell routine started";
            DbHelper.Log(TrinityLogLevel.Debug, LogCategory.UserInformation, "GSDebug: Sell routine started.");
            //ZetaDia.Actors.Update();
            if (ZetaDia.Actors.Me == null)
            {
                DbHelper.Log(TrinityLogLevel.Debug, LogCategory.UserInformation, "GSError: Diablo 3 memory read error, or item became invalid [PreSell-1]");
                return RunStatus.Failure;
            }
            bGoToSafetyPointFirst = true;
            bGoToSafetyPointSecond = false;
            loggedJunkThisStash = false;
            bCurrentlyMoving = false;
            bReachedDestination = false;
            currentItemLoops = 0;
            RandomizeTheTimer();
            return RunStatus.Success;
        }

        /// <summary>
        /// Sell Routine replacement for smooth one-at-a-time item selling and handling
        /// </summary>
        /// <param name="ret"></param>
        /// <returns></returns>
        internal static RunStatus GilesOptimisedSell(object ret)
        {
            string VendorName = GetTownVendorName();
            if (bGoToSafetyPointFirst)
            {
                Vector3 vectorPlayerPosition = ZetaDia.Me.Position;
                Vector3 vectorSellLocation = new Vector3(0f, 0f, 0f);
                switch (ZetaDia.CurrentAct)
                {
                    case Act.A1:
                        vectorSellLocation = new Vector3(2941.904f, 2812.825f, 24.04533f); break;
                    case Act.A2:
                        vectorSellLocation = new Vector3(295.2101f, 265.1436f, 0.1000002f); break;
                    case Act.A3:
                    case Act.A4:
                        vectorSellLocation = new Vector3(410.6073f, 355.8762f, 0.1000005f); break;
                }
                float iDistanceFromSell = Vector3.Distance(vectorPlayerPosition, vectorSellLocation);
                if (bCurrentlyMoving)
                {
                    if (iDistanceFromSell <= 8f)
                    {
                        bGoToSafetyPointFirst = false;
                        /*if (ZetaDia.CurrentAct == Act.A2)
                            bGoToSafetyPointSecond = true;*/
                        bCurrentlyMoving = false;
                    }
                    else if (lastDistance == iDistanceFromSell)
                    {
                        ZetaDia.Me.UsePower(SNOPower.Walk, vectorSellLocation, ZetaDia.Me.WorldDynamicId);
                    }
                    DbHelper.Log(TrinityLogLevel.Debug, LogCategory.UserInformation, "GSDebug: Moving to vendor pt1.");
                    return RunStatus.Running;
                }
                lastDistance = iDistanceFromSell;
                if (iDistanceFromSell > 120f)
                    return RunStatus.Failure;
                if (iDistanceFromSell > 8f)
                {
                    ZetaDia.Me.UsePower(SNOPower.Walk, vectorSellLocation, ZetaDia.Me.WorldDynamicId);
                    bCurrentlyMoving = true;
                    DbHelper.Log(TrinityLogLevel.Debug, LogCategory.UserInformation, "GSDebug: Moving to vendor pt2.");
                    return RunStatus.Running;
                }
                bCurrentlyMoving = false;
                bGoToSafetyPointFirst = false;

                DbHelper.Log(TrinityLogLevel.Debug, LogCategory.UserInformation, "GSDebug: Moving to vendor pt3.");
                return RunStatus.Running;
            }


            if (!bReachedDestination)
            {
                Vector3 vectorPlayerPosition = ZetaDia.Me.Position;
                Vector3 vectorSellLocation = new Vector3(0f, 0f, 0f);
                switch (ZetaDia.CurrentAct)
                {
                    case Act.A1:
                        vectorSellLocation = new Vector3(2896.159f, 2779.443f, 24.04532f); break;
                    case Act.A2:
                        vectorSellLocation = new Vector3(286.0302f, 280.2442f, 0.1000038f); break;
                    case Act.A3:
                    case Act.A4:
                        vectorSellLocation = new Vector3(447.8373f, 324.1446f, 0.1000005f); break;
                }
                DiaUnit townVendor = ZetaDia.Actors.GetActorsOfType<DiaUnit>(true).FirstOrDefault<DiaUnit>(u => u.Name.ToLower().StartsWith(VendorName));
                if (townVendor != null)
                    vectorSellLocation = townVendor.Position;
                float iDistanceFromSell = Vector3.Distance(vectorPlayerPosition, vectorSellLocation);
                if (bCurrentlyMoving)
                {
                    if (iDistanceFromSell <= 9.5f)
                    {
                        bReachedDestination = true;
                        if (townVendor == null)
                            return RunStatus.Failure;
                        townVendor.Interact();
                    }
                    else if (lastDistance == iDistanceFromSell)
                    {
                        ZetaDia.Me.UsePower(SNOPower.Walk, vectorSellLocation, ZetaDia.Me.WorldDynamicId);
                    }
                    DbHelper.Log(TrinityLogLevel.Debug, LogCategory.UserInformation, "GSDebug: Moving to vendor pt4.");
                    return RunStatus.Running;
                }
                lastDistance = iDistanceFromSell;
                if (iDistanceFromSell > 120f)
                    return RunStatus.Failure;
                if (iDistanceFromSell > 9.5f)
                {
                    bCurrentlyMoving = true;
                    ZetaDia.Me.UsePower(SNOPower.Walk, vectorSellLocation, ZetaDia.Me.WorldDynamicId);
                    DbHelper.Log(TrinityLogLevel.Debug, LogCategory.UserInformation, "GSDebug: Moving to vendor pt5.");
                    return RunStatus.Running;
                }
                bReachedDestination = true;
                if (townVendor == null)
                    return RunStatus.Failure;
                townVendor.Interact();
            }
            if (!Zeta.Internals.UIElement.IsValidElement(12123456831356216535L) || !Zeta.Internals.UIElement.FromHash(12123456831356216535L).IsVisible)
            {
                DiaUnit townVendor = ZetaDia.Actors.GetActorsOfType<DiaUnit>(true).FirstOrDefault<DiaUnit>(u => u.Name.ToLower().StartsWith(VendorName));
                if (townVendor == null)
                    return RunStatus.Failure;
                townVendor.Interact();
                DbHelper.Log(TrinityLogLevel.Debug, LogCategory.UserInformation, "GSDebug: Vendor interact pt1.");
                return RunStatus.Running;
            }
            if (hashGilesCachedSellItems.Count > 0)
            {
                currentItemLoops++;
                if (currentItemLoops < itemDelayLoopLimit)
                    return RunStatus.Running;
                currentItemLoops = 0;
                RandomizeTheTimer();
                GilesCachedACDItem thisitem = hashGilesCachedSellItems.OrderBy(i => i.Row).ThenBy(i => i.Column).FirstOrDefault();

                // Item log for cool stuff sold
                if (thisitem != null)
                {
                    GItemType OriginalGilesItemType = GilesTrinity.DetermineItemType(thisitem.InternalName, thisitem.DBItemType, thisitem.FollowerType);
                    GItemBaseType thisGilesBaseType = GilesTrinity.DetermineBaseType(OriginalGilesItemType);
                    if (thisGilesBaseType == GItemBaseType.WeaponTwoHand || thisGilesBaseType == GItemBaseType.WeaponOneHand || thisGilesBaseType == GItemBaseType.WeaponRange ||
                        thisGilesBaseType == GItemBaseType.Armor || thisGilesBaseType == GItemBaseType.Jewelry || thisGilesBaseType == GItemBaseType.Offhand ||
                        thisGilesBaseType == GItemBaseType.FollowerItem)
                    {
                        double iThisItemValue = GilesTrinity.ValueThisItem(thisitem, OriginalGilesItemType);
                        LogJunkItems(thisitem, thisGilesBaseType, OriginalGilesItemType, iThisItemValue);
                    }
                    ZetaDia.Me.Inventory.SellItem(thisitem.item);
                }
                if (thisitem != null)
                    hashGilesCachedSellItems.Remove(thisitem);
                if (hashGilesCachedSellItems.Count > 0)
                    return RunStatus.Running;
            }
            bCurrentlyMoving = false;
            bReachedSafety = false;
            return RunStatus.Success;
        }

        private static string GetTownVendorName()
        {
            string sVendorName = "";
            switch (ZetaDia.CurrentAct)
            {
                case Act.A1:
                    sVendorName = "a1_uniquevendor_miner"; break;
                case Act.A2:
                    sVendorName = "a2_uniquevendor_peddler"; break;
                case Act.A3:
                    sVendorName = "a3_uniquevendor_collector"; break;
                case Act.A4:
                    sVendorName = "a4_uniquevendor_collector"; break;
            }
            return sVendorName;
        }

        /// <summary>
        /// Post Sell tidies everything up and signs off junk log after selling
        /// </summary>
        /// <param name="ret"></param>
        /// <returns></returns>
        internal static RunStatus GilesOptimisedPostSell(object ret)
        {
            DbHelper.Log(TrinityLogLevel.Debug, LogCategory.UserInformation, "GSDebug: Sell routine ending sequence...");

            // Always repair, but only if we have enough money
            if (bNeedsEquipmentRepairs && iLowestDurabilityFound < 20 && iLowestDurabilityFound > -1 && ZetaDia.Me.Inventory.Coinage < 5000)
            {
                DbHelper.Log(TrinityLogLevel.Normal, LogCategory.UserInformation, "*");
                DbHelper.Log(TrinityLogLevel.Normal, LogCategory.UserInformation, "Emergency Stop: You need repairs but don't have enough money. Stopping the bot to prevent infinite death loop.");
                BotMain.Stop();
            }
            string VendorName = GetTownVendorName();

            if (!Zeta.Internals.UIElement.IsValidElement(12123456831356216535L) || !Zeta.Internals.UIElement.FromHash(12123456831356216535L).IsVisible)
            {
                DiaUnit townVendor = ZetaDia.Actors.GetActorsOfType<DiaUnit>(true).FirstOrDefault<DiaUnit>(u => u.Name.ToLower().StartsWith(VendorName));
                if (townVendor == null)
                    return RunStatus.Failure;
                townVendor.Interact();
                DbHelper.Log(TrinityLogLevel.Debug, LogCategory.UserInformation, "GSDebug: Vendor interact pt2.");
                return RunStatus.Running;
            }

            ZetaDia.Me.Inventory.RepairAllItems();

            /*
             * Per Nesox, 2013 jan 02, clicking repair tab doesn't activate (same issue as stash tabs)
             */
            //UIElement repairTab = UIElement.FromHash(0x95EFA3BFC7BD25BC);
            //UIElement repairAllButton = UIElement.FromHash(0x80F5D06A035848A5);

            //SetStartRandomTimer();
            //if (RandomTimerIsNotDone())
            //{
            //    return RunStatus.Running;
            //}
            //else
            //{
            //    StopRandomTimer();
            //}

            //Logging.Write("Repair tab isValid: {0} isVisible: {1}", repairTab.IsValid, repairTab.IsVisible);
            //Logging.Write("Repair all button isValid: {0} isVisible: {1}", repairAllButton.IsValid, repairAllButton.IsVisible);

            //if (repairTab.IsValid && repairTab.IsVisible && !(repairAllButton.IsValid && repairAllButton.IsVisible) )
            //{
            //    DbHelper.Log(TrinityLogLevel.Normal, LogCategory.UserInformation, "Clicking Repair tab");
            //    repairTab.Click();
            //    return RunStatus.Running;
            //}

            //if (repairAllButton.IsValid && repairAllButton.IsVisible)
            //{
            //    UITextObject repairButtonText = null;
            //    if (repairAllButton.HasText)
            //        repairButtonText = repairAllButton.TextObject;

            //    DbHelper.Log(TrinityLogLevel.Normal, LogCategory.UserInformation, "Clicking Repair All button. Text: {0}", repairButtonText.Text);
            //    repairAllButton.Click();
            //}


            bNeedsEquipmentRepairs = false;
            if (loggedJunkThisStash)
            {
                FileStream LogStream = null;
                try
                {
                    LogStream = File.Open(Path.Combine(FileManager.LoggingPath, "JunkLog - " + ZetaDia.Actors.Me.ActorClass.ToString() + ".log"), FileMode.Append, FileAccess.Write, FileShare.Read);
                    using (StreamWriter LogWriter = new StreamWriter(LogStream))
                        LogWriter.WriteLine("");
                    LogStream.Close();
                }
                catch (IOException)
                {
                    DbHelper.Log(TrinityLogLevel.Normal, LogCategory.UserInformation, "Fatal Error: File access error for signing off the junk log file.");
                    if (LogStream != null)
                        LogStream.Close();
                }
                loggedJunkThisStash = false;
            }

            // See if we can close the inventory window
            if (Zeta.Internals.UIElement.IsValidElement(0x368FF8C552241695))
            {
                try
                {
                    var el = Zeta.Internals.UIElement.FromHash(0x368FF8C552241695);
                    if (el != null && el.IsValid && el.IsVisible && el.IsEnabled)
                        el.Click();
                }
                catch
                {

                    // Do nothing if it fails, just catching to prevent any big errors/plugin crashes from this
                }
            }

            DbHelper.Log(TrinityLogLevel.Debug, LogCategory.UserInformation, "GSDebug: Sell routine finished.");
            return RunStatus.Success;
        }

        /// <summary>
        /// Salvage Overlord determines if we should visit the blacksmith or not
        /// </summary>
        /// <param name="ret"></param>
        /// <returns></returns>
        internal static bool GilesSalvageOverlord(object ret)
        {
            GilesTrinity.ForceVendorRunASAP = false;
            hashGilesCachedSalvageItems = new HashSet<GilesCachedACDItem>();
            bool bShouldVisitSmith = false;

            // Check for anything to salvage
            foreach (ACDItem thisitem in ZetaDia.Me.Inventory.Backpack)
            {
                if (thisitem.BaseAddress != IntPtr.Zero)
                {
                    if (!ItemManager.ItemIsProtected(thisitem) && !GilesTrinity.weaponSwap.SwapperUsing(thisitem))
                    {
                        GilesCachedACDItem thiscacheditem = new GilesCachedACDItem(thisitem, thisitem.InternalName, thisitem.Name, thisitem.Level, thisitem.ItemQualityLevel, thisitem.Gold, thisitem.GameBalanceId,
                            thisitem.DynamicId, thisitem.Stats.WeaponDamagePerSecond, thisitem.IsOneHand, thisitem.IsTwoHand, thisitem.DyeType, thisitem.ItemType, thisitem.ItemBaseType, thisitem.FollowerSpecialType,
                            thisitem.IsUnidentified, thisitem.ItemStackQuantity, thisitem.Stats);
                        bool bShouldSalvageThis = GilesTrinity.Settings.Loot.ItemFilterMode != ItemFilterMode.DemonBuddy ? GilesSalvageValidation(thiscacheditem.InternalName, thiscacheditem.Level, thiscacheditem.Quality, thiscacheditem.DBItemType, thiscacheditem.FollowerType) : ItemManager.ShouldSalvageItem(thisitem);
                        if (bShouldSalvageThis)
                        {
                            hashGilesCachedSalvageItems.Add(thiscacheditem);
                            bShouldVisitSmith = true;
                        }
                    }
                }
                else
                {
                    DbHelper.Log(TrinityLogLevel.Debug, LogCategory.UserInformation, "GSError: Diablo 3 memory read error, or item became invalid [SalvageOver-1]");
                }
            }
            return bShouldVisitSmith;
        }

        /// <summary>
        /// Pre Salvage sets everything up ready for our blacksmith run
        /// </summary>
        /// <param name="ret"></param>
        /// <returns></returns>
        internal static RunStatus GilesOptimisedPreSalvage(object ret)
        {
            if (GilesTrinity.Settings.Advanced.DebugInStatusBar)
                BotMain.StatusText = "Town run: Salvage routine started";
            DbHelper.Log(TrinityLogLevel.Debug, LogCategory.UserInformation, "GSDebug: Salvage routine started.");
            //ZetaDia.Actors.Update();
            if (ZetaDia.Actors.Me == null)
            {
                DbHelper.Log(TrinityLogLevel.Debug, LogCategory.UserInformation, "GSError: Diablo 3 memory read error, or item became invalid [PreSalvage-1]");
                return RunStatus.Failure;
            }
            bGoToSafetyPointFirst = true;
            bGoToSafetyPointSecond = false;
            loggedJunkThisStash = false;
            bCurrentlyMoving = false;
            bReachedDestination = false;
            currentItemLoops = 0;
            RandomizeTheTimer();
            return RunStatus.Success;
        }

        /// <summary>
        /// Nice smooth one-at-a-time salvaging replacement
        /// </summary>
        /// <param name="ret"></param>
        /// <returns></returns>
        internal static RunStatus GilesOptimisedSalvage(object ret)
        {
            if (bGoToSafetyPointFirst)
            {
                Vector3 vectorPlayerPosition = ZetaDia.Me.Position;
                Vector3 vectorSalvageLocation = new Vector3(0f, 0f, 0f);
                switch (ZetaDia.CurrentAct)
                {
                    case Act.A1:
                        vectorSalvageLocation = new Vector3(2949.626f, 2815.065f, 24.04389f); break;
                    case Act.A2:
                        vectorSalvageLocation = new Vector3(289.6358f, 232.1146f, 0.1f); break;
                    case Act.A3:
                    case Act.A4:
                        vectorSalvageLocation = new Vector3(379.6096f, 415.6198f, 0.3321424f); break;
                }
                float iDistanceFromSalvage = Vector3.Distance(vectorPlayerPosition, vectorSalvageLocation);
                if (bCurrentlyMoving)
                {
                    if (iDistanceFromSalvage <= 8f)
                    {
                        bGoToSafetyPointFirst = false;
                        if (ZetaDia.CurrentAct == Act.A3)
                            bGoToSafetyPointSecond = true;
                        bCurrentlyMoving = false;
                    }
                    else if (lastDistance == iDistanceFromSalvage)
                    {
                        ZetaDia.Me.UsePower(SNOPower.Walk, vectorSalvageLocation, ZetaDia.Me.WorldDynamicId);
                    }
                    return RunStatus.Running;
                }
                lastDistance = iDistanceFromSalvage;
                if (iDistanceFromSalvage > 120f)
                    return RunStatus.Failure;
                if (iDistanceFromSalvage > 8f)
                {
                    ZetaDia.Me.UsePower(SNOPower.Walk, vectorSalvageLocation, ZetaDia.Me.WorldDynamicId);
                    bCurrentlyMoving = true;
                    return RunStatus.Running;
                }
                bCurrentlyMoving = false;
                bGoToSafetyPointFirst = false;
                if (ZetaDia.CurrentAct == Act.A3)
                    bGoToSafetyPointSecond = true;
                return RunStatus.Running;
            }
            if (bGoToSafetyPointSecond)
            {
                Vector3 vectorPlayerPosition = ZetaDia.Me.Position;
                Vector3 vectorSalvageLocation = new Vector3(0f, 0f, 0f);
                switch (ZetaDia.CurrentAct)
                {
                    case Act.A1:
                        vectorSalvageLocation = new Vector3(2949.626f, 2815.065f, 24.04389f); break;
                    case Act.A2:
                        vectorSalvageLocation = new Vector3(289.6358f, 232.1146f, 0.1f); break;
                    case Act.A3:
                    case Act.A4:
                        vectorSalvageLocation = new Vector3(328.6024f, 425.4113f, 0.2758033f); break;
                }
                float iDistanceFromSalvage = Vector3.Distance(vectorPlayerPosition, vectorSalvageLocation);
                if (bCurrentlyMoving)
                {
                    if (iDistanceFromSalvage <= 8f)
                    {
                        bGoToSafetyPointSecond = false;
                        bCurrentlyMoving = false;
                    }
                    else if (lastDistance == iDistanceFromSalvage)
                    {
                        ZetaDia.Me.UsePower(SNOPower.Walk, vectorSalvageLocation, ZetaDia.Me.WorldDynamicId);
                    }
                    return RunStatus.Running;
                }
                lastDistance = iDistanceFromSalvage;
                if (iDistanceFromSalvage > 120f)
                    return RunStatus.Failure;
                if (iDistanceFromSalvage > 8f)
                {
                    ZetaDia.Me.UsePower(SNOPower.Walk, vectorSalvageLocation, ZetaDia.Me.WorldDynamicId);
                    bCurrentlyMoving = true;
                    return RunStatus.Running;
                }
                bCurrentlyMoving = false;
                bGoToSafetyPointSecond = false;
                return RunStatus.Running;
            }
            if (!bReachedDestination)
            {
                Vector3 vectorPlayerPosition = ZetaDia.Me.Position;
                Vector3 vectorSalvageLocation = new Vector3(0f, 0f, 0f);
                DiaUnit objSalvageNavigation = ZetaDia.Actors.GetActorsOfType<DiaUnit>(true).FirstOrDefault<DiaUnit>(u => u.IsSalvageShortcut);
                if (objSalvageNavigation != null)
                    vectorSalvageLocation = objSalvageNavigation.Position;
                else
                    switch (ZetaDia.CurrentAct)
                    {
                        case Act.A1:
                            vectorSalvageLocation = new Vector3(2942.137f, 2854.078f, 24.04533f); break;
                        case Act.A2:
                            vectorSalvageLocation = new Vector3(275.6705f, 221.1727f, 0.1f); break;
                        case Act.A3:
                        case Act.A4:
                            vectorSalvageLocation = new Vector3(328.6024f, 425.4113f, 0.2758033f); break;
                    }
                float iDistanceFromSalvage = Vector3.Distance(vectorPlayerPosition, vectorSalvageLocation);
                if (bCurrentlyMoving)
                {
                    if (iDistanceFromSalvage <= 9.5f)
                    {
                        bReachedDestination = true;
                        if (objSalvageNavigation == null)
                            return RunStatus.Failure;
                        objSalvageNavigation.Interact();
                    }
                    else if (lastDistance == iDistanceFromSalvage)
                    {
                        ZetaDia.Me.UsePower(SNOPower.Walk, vectorSalvageLocation, ZetaDia.Me.WorldDynamicId);
                    }
                    return RunStatus.Running;
                }
                lastDistance = iDistanceFromSalvage;
                if (iDistanceFromSalvage > 120f)
                    return RunStatus.Failure;
                if (iDistanceFromSalvage > 9.5f)
                {
                    bCurrentlyMoving = true;
                    ZetaDia.Me.UsePower(SNOPower.Walk, vectorSalvageLocation, ZetaDia.Me.WorldDynamicId);
                    return RunStatus.Running;
                }
                bReachedDestination = true;
                if (objSalvageNavigation == null)
                    return RunStatus.Failure;
                objSalvageNavigation.Interact();
            }
            if (!Zeta.Internals.UIElement.IsValidElement(0x359867fd497d2ff3L) || !Zeta.Internals.UIElement.FromHash(0x359867fd497d2ff3L).IsVisible)
            {
                DiaUnit objSalvageNavigation = ZetaDia.Actors.GetActorsOfType<DiaUnit>(true).FirstOrDefault<DiaUnit>(u => u.IsSalvageShortcut);
                if (objSalvageNavigation == null)
                    return RunStatus.Failure;
                objSalvageNavigation.Interact();
                return RunStatus.Running;
            }
            if (hashGilesCachedSalvageItems.Count > 0)
            {
                currentItemLoops++;
                if (currentItemLoops < itemDelayLoopLimit)
                    return RunStatus.Running;
                currentItemLoops = 0;
                RandomizeTheTimer();
                GilesCachedACDItem thisitem = hashGilesCachedSalvageItems.FirstOrDefault();
                if (thisitem != null)
                {

                    // Item log for cool stuff stashed
                    GItemType OriginalGilesItemType = GilesTrinity.DetermineItemType(thisitem.InternalName, thisitem.DBItemType, thisitem.FollowerType);
                    GItemBaseType thisGilesBaseType = GilesTrinity.DetermineBaseType(OriginalGilesItemType);
                    if (thisGilesBaseType == GItemBaseType.WeaponTwoHand || thisGilesBaseType == GItemBaseType.WeaponOneHand || thisGilesBaseType == GItemBaseType.WeaponRange ||
                        thisGilesBaseType == GItemBaseType.Armor || thisGilesBaseType == GItemBaseType.Jewelry || thisGilesBaseType == GItemBaseType.Offhand ||
                        thisGilesBaseType == GItemBaseType.FollowerItem)
                    {
                        double iThisItemValue = GilesTrinity.ValueThisItem(thisitem, OriginalGilesItemType);
                        LogJunkItems(thisitem, thisGilesBaseType, OriginalGilesItemType, iThisItemValue);
                    }
                    ZetaDia.Me.Inventory.SalvageItem(thisitem.DynamicID);
                }
                if (thisitem != null)
                    hashGilesCachedSalvageItems.Remove(thisitem);
                if (hashGilesCachedSalvageItems.Count > 0)
                    return RunStatus.Running;
            }
            bReachedSafety = false;
            bCurrentlyMoving = false;
            return RunStatus.Success;
        }

        /// <summary>
        /// Post salvage cleans up and signs off junk log file after salvaging
        /// </summary>
        /// <param name="ret"></param>
        /// <returns></returns>
        internal static RunStatus GilesOptimisedPostSalvage(object ret)
        {
            DbHelper.Log(TrinityLogLevel.Debug, LogCategory.UserInformation, "GSDebug: Salvage routine ending sequence...");
            //using (ZetaDia.Memory.AcquireFrame())
            //{
            //    ZetaDia.Actors.Update();
            //}
            if (loggedJunkThisStash)
            {
                FileStream LogStream = null;
                try
                {
                    LogStream = File.Open(Path.Combine(FileManager.LoggingPath, "JunkLog - " + ZetaDia.Actors.Me.ActorClass.ToString() + ".log"), FileMode.Append, FileAccess.Write, FileShare.Read);
                    using (StreamWriter LogWriter = new StreamWriter(LogStream))
                        LogWriter.WriteLine("");
                    LogStream.Close();
                }
                catch (IOException)
                {
                    DbHelper.Log(TrinityLogLevel.Normal, LogCategory.UserInformation, "Fatal Error: File access error for signing off the junk log file.");
                    if (LogStream != null)
                        LogStream.Close();
                }
                loggedJunkThisStash = false;
            }
            if (!bReachedSafety && ZetaDia.CurrentAct == Act.A3)
            {
                Vector3 vectorPlayerPosition = ZetaDia.Me.Position;
                Vector3 vectorSafeLocation = new Vector3(379.6096f, 415.6198f, 0.3321424f);
                float iDistanceFromSafety = Vector3.Distance(vectorPlayerPosition, vectorSafeLocation);
                if (bCurrentlyMoving)
                {
                    if (iDistanceFromSafety <= 8f)
                    {
                        bGoToSafetyPointSecond = false;
                        bCurrentlyMoving = false;
                    }
                    else if (lastDistance == iDistanceFromSafety)
                    {
                        ZetaDia.Me.UsePower(SNOPower.Walk, vectorSafeLocation, ZetaDia.Me.WorldDynamicId);
                    }
                    return RunStatus.Running;
                }
                lastDistance = iDistanceFromSafety;
                if (iDistanceFromSafety > 120f)
                    return RunStatus.Failure;
                if (iDistanceFromSafety > 8f)
                {
                    ZetaDia.Me.UsePower(SNOPower.Walk, vectorSafeLocation, ZetaDia.Me.WorldDynamicId);
                    bCurrentlyMoving = true;
                    return RunStatus.Running;
                }
                bCurrentlyMoving = false;
                bReachedSafety = true;
            }
            DbHelper.Log(TrinityLogLevel.Debug, LogCategory.UserInformation, "GSDebug: Salvage routine finished.");
            return RunStatus.Success;
        }

        /// <summary>
        /// Log the nice items we found and stashed
        /// </summary>
        /// <param name="thisgooditem"></param>
        /// <param name="thisgilesbaseitemtype"></param>
        /// <param name="thisgilesitemtype"></param>
        /// <param name="ithisitemvalue"></param>
        internal static void LogGoodItems(GilesCachedACDItem thisgooditem, GItemBaseType thisgilesbaseitemtype, GItemType thisgilesitemtype, double ithisitemvalue)
        {
            FileStream LogStream = null;
            try
            {
                LogStream = File.Open(Path.Combine(FileManager.LoggingPath, "StashLog - " + ZetaDia.Actors.Me.ActorClass.ToString() + ".log"), FileMode.Append, FileAccess.Write, FileShare.Read);

                //TODO : Change File Log writing
                using (StreamWriter LogWriter = new StreamWriter(LogStream))
                {
                    if (!loggedAnythingThisStash)
                    {
                        loggedAnythingThisStash = true;
                        LogWriter.WriteLine(DateTime.Now.ToString() + ":");
                        LogWriter.WriteLine("====================");
                    }
                    string sLegendaryString = "";
                    bool bShouldNotify = false;
                    if (thisgooditem.Quality >= ItemQuality.Legendary)
                    {
                        if (!GilesTrinity.Settings.Notification.LegendaryScoring)
                            bShouldNotify = true;
                        else if (GilesTrinity.Settings.Notification.LegendaryScoring && GilesTrinity.CheckScoreForNotification(thisgilesbaseitemtype, ithisitemvalue))
                            bShouldNotify = true;
                        if (bShouldNotify)
                            NotificationManager.AddNotificationToQueue(thisgooditem.RealName + " [" + thisgilesitemtype.ToString() +
                                "] (Score=" + ithisitemvalue.ToString() + ". " + ValueItemStatString + ")",
                                ZetaDia.Service.CurrentHero.Name + " new legendary!", ProwlNotificationPriority.Emergency);
                        sLegendaryString = " {legendary item}";

                        // Change made by bombastic
                        DbHelper.Log(TrinityLogLevel.Normal, LogCategory.UserInformation, "+=+=+=+=+=+=+=+=+ LEGENDARY FOUND +=+=+=+=+=+=+=+=+");
                        DbHelper.Log(TrinityLogLevel.Normal, LogCategory.UserInformation, "+  Name:       {0} ({1})", thisgooditem.RealName, thisgilesitemtype);
                        DbHelper.Log(TrinityLogLevel.Normal, LogCategory.UserInformation, "+  Score:       {0:0}", ithisitemvalue);
                        DbHelper.Log(TrinityLogLevel.Normal, LogCategory.UserInformation, "+  Attributes: {0}", ValueItemStatString);
                        DbHelper.Log(TrinityLogLevel.Normal, LogCategory.UserInformation, "+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+");
                    }
                    else
                    {

                        // Check for non-legendary notifications
                        bShouldNotify = GilesTrinity.CheckScoreForNotification(thisgilesbaseitemtype, ithisitemvalue);
                        if (bShouldNotify)
                            NotificationManager.AddNotificationToQueue(thisgooditem.RealName + " [" + thisgilesitemtype.ToString() + "] (Score=" + ithisitemvalue.ToString() + ". " + ValueItemStatString + ")", ZetaDia.Service.CurrentHero.Name + " new item!", ProwlNotificationPriority.Emergency);
                    }
                    if (bShouldNotify)
                    {
                        NotificationManager.EmailMessage.AppendLine(thisgilesbaseitemtype.ToString() + " - " + thisgilesitemtype.ToString() + " '" + thisgooditem.RealName + "'. Score = " + Math.Round(ithisitemvalue).ToString() + sLegendaryString)
                            .AppendLine("  " + ValueItemStatString)
                            .AppendLine();
                    }
                    LogWriter.WriteLine(thisgilesbaseitemtype.ToString() + " - " + thisgilesitemtype.ToString() + " '" + thisgooditem.RealName + "'. Score = " + Math.Round(ithisitemvalue).ToString() + sLegendaryString);
                    LogWriter.WriteLine("  " + ValueItemStatString);
                    LogWriter.WriteLine("");
                }
                LogStream.Close();
            }
            catch (IOException)
            {
                DbHelper.Log(TrinityLogLevel.Normal, LogCategory.UserInformation, "Fatal Error: File access error for stash log file.");
                if (LogStream != null)
                    LogStream.Close();
            }
        }

        /// <summary>
        /// Log the rubbish junk items we salvaged or sold
        /// </summary>
        /// <param name="thisgooditem"></param>
        /// <param name="thisgilesbaseitemtype"></param>
        /// <param name="thisgilesitemtype"></param>
        /// <param name="ithisitemvalue"></param>
        internal static void LogJunkItems(GilesCachedACDItem thisgooditem, GItemBaseType thisgilesbaseitemtype, GItemType thisgilesitemtype, double ithisitemvalue)
        {
            FileStream LogStream = null;
            try
            {
                LogStream = File.Open(Path.Combine(FileManager.LoggingPath, "JunkLog - " + ZetaDia.Actors.Me.ActorClass.ToString() + ".log"), FileMode.Append, FileAccess.Write, FileShare.Read);
                using (StreamWriter LogWriter = new StreamWriter(LogStream))
                {
                    if (!loggedJunkThisStash)
                    {
                        loggedJunkThisStash = true;
                        LogWriter.WriteLine(DateTime.Now.ToString() + ":");
                        LogWriter.WriteLine("====================");
                    }
                    string sLegendaryString = "";
                    if (thisgooditem.Quality >= ItemQuality.Legendary)
                        sLegendaryString = " {legendary item}";
                    LogWriter.WriteLine(thisgilesbaseitemtype.ToString() + " - " + thisgilesitemtype.ToString() + " '" + thisgooditem.RealName + "'. Score = " + ithisitemvalue.ToString("0") + sLegendaryString);
                    if (junkItemStatString != "")
                        LogWriter.WriteLine("  " + junkItemStatString);
                    else
                        LogWriter.WriteLine("  (no scorable attributes)");
                    LogWriter.WriteLine("");
                }
                LogStream.Close();
            }
            catch (IOException)
            {
                DbHelper.Log(TrinityLogLevel.Normal, LogCategory.UserInformation, "Fatal Error: File access error for junk log file.");
                if (LogStream != null)
                    LogStream.Close();
            }
        }

        /// <summary>
        /// Stash replacement accurately and neatly finds a free stash location
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        internal static bool GilesStashAttempt(GilesCachedACDItem item)
        {
            int iPlayerDynamicID = ZetaDia.Me.CommonData.DynamicId;
            int iOriginalGameBalanceId = item.BalanceID;
            int iOriginalDynamicID = item.DynamicID;
            int iOriginalStackQuantity = item.ItemStackQuantity;
            string sOriginalItemName = item.RealName;
            string sOriginalInternalName = item.InternalName;
            GItemType OriginalGilesItemType = GilesTrinity.DetermineItemType(item.InternalName, item.DBItemType, item.FollowerType);
            GItemBaseType thisGilesBaseType = GilesTrinity.DetermineBaseType(OriginalGilesItemType);
            bool bOriginalTwoSlot = GilesTrinity.DetermineIsTwoSlot(OriginalGilesItemType);
            bool bOriginalIsStackable = GilesTrinity.DetermineIsStackable(OriginalGilesItemType);
            int iAttempts;
            if (_dictItemStashAttempted.TryGetValue(iOriginalDynamicID, out iAttempts))
            {
                DbHelper.Log(TrinityLogLevel.Normal, LogCategory.UserInformation, "GSError: Detected a duplicate stash attempt, DB item mis-read error, now forcing this item as a 2-slot item");
                _dictItemStashAttempted[iOriginalDynamicID] = iAttempts + 1;
                bOriginalTwoSlot = true;
                bOriginalIsStackable = false;
                if (iAttempts > 6)
                {
                    DbHelper.Log(TrinityLogLevel.Normal, LogCategory.UserInformation, "GSError: Detected an item stash loop risk, now re-mapping stash treating everything as 2-slot and re-attempting");

                    // Array for what blocks are or are not blocked
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

                    // Map out all the items already in the stash
                    foreach (ACDItem tempitem in ZetaDia.Me.Inventory.StashItems)
                    {
                        if (tempitem.BaseAddress != IntPtr.Zero)
                        {
                            int inventoryRow = tempitem.InventoryRow;
                            int inventoryColumn = tempitem.InventoryColumn;

                            // Mark this slot as not-free
                            StashSlotBlocked[inventoryColumn, inventoryRow] = true;

                            // Try and reliably find out if this is a two slot item or not
                            StashSlotBlocked[inventoryColumn, inventoryRow + 1] = true;
                            if (inventoryRow != 19 && inventoryRow != 9 && inventoryRow != 29)
                            {
                                StashSlotBlocked[inventoryColumn, inventoryRow + 1] = true;
                            }
                        }
                    }
                }
                if (iAttempts > 15)
                {
                    DbHelper.Log(TrinityLogLevel.Normal, LogCategory.UserInformation, "*");
                    DbHelper.Log(TrinityLogLevel.Normal, LogCategory.UserInformation, "GSError: Emergency Stop: No matter what we tried, we couldn't prevent an infinite stash loop. Sorry. Now stopping the bot.");
                    BotMain.Stop();
                    return false;
                }
            }
            else
            {
                _dictItemStashAttempted.Add(iOriginalDynamicID, 1);
            }

            // Safety incase it's not actually in the backpack anymore
            /*if (item.InventorySlot != InventorySlot.PlayerBackpack)
            {
                Log("GSError: Diablo 3 memory read error, or item became invalid [StashAttempt-4]", true);
                return false;
            }*/
            int iLeftoverStackQuantity = 0;

            // Item log for cool stuff stashed
            if (thisGilesBaseType == GItemBaseType.WeaponTwoHand || thisGilesBaseType == GItemBaseType.WeaponOneHand || thisGilesBaseType == GItemBaseType.WeaponRange ||
                thisGilesBaseType == GItemBaseType.Armor || thisGilesBaseType == GItemBaseType.Jewelry || thisGilesBaseType == GItemBaseType.Offhand ||
                thisGilesBaseType == GItemBaseType.FollowerItem)
            {
                double iThisItemValue = GilesTrinity.ValueThisItem(item, OriginalGilesItemType);
                LogGoodItems(item, thisGilesBaseType, OriginalGilesItemType, iThisItemValue);
            }
            int iPointX = -1;
            int iPointY = -1;

            // First check if we can top-up any already-existing stacks in the stash
            if (bOriginalIsStackable)
            {
                foreach (ACDItem tempitem in ZetaDia.Me.Inventory.StashItems)
                {
                    if (tempitem.BaseAddress == IntPtr.Zero)
                    {
                        DbHelper.Log(TrinityLogLevel.Debug, LogCategory.UserInformation, "GSError: Diablo 3 memory read error, or stash item became invalid [StashAttempt-5]");
                        return false;
                    }

                    // Check if we combine the stacks, we won't overfill them
                    if ((tempitem.GameBalanceId == iOriginalGameBalanceId) && (tempitem.ItemStackQuantity < tempitem.MaxStackCount))
                    {

                        // Will we have leftovers?
                        if ((tempitem.ItemStackQuantity + iOriginalStackQuantity) > tempitem.MaxStackCount)
                        {
                            iLeftoverStackQuantity = (tempitem.ItemStackQuantity + iOriginalStackQuantity) - tempitem.MaxStackCount;
                        }
                        iPointX = tempitem.InventoryColumn;
                        iPointY = tempitem.InventoryRow;
                        goto HandleStackMovement;
                    }
                }
            HandleStackMovement:
                if ((iPointX >= 0) && (iPointY >= 0))
                {
                    ZetaDia.Me.Inventory.MoveItem(iOriginalDynamicID, iPlayerDynamicID, InventorySlot.PlayerSharedStash, iPointX, iPointY);

                    // Only return if we have emptied this stack
                    if (iLeftoverStackQuantity <= 0)
                    {
                        return true;
                    }
                }
            }
            iPointX = -1;
            iPointY = -1;

            // If it's a 2-square item, find a double-slot free
            if (bOriginalTwoSlot)
            {
                for (int iRow = 0; iRow <= 29; iRow++)
                {
                    bool bBottomPageRow = (iRow == 9 || iRow == 19 || iRow == 29);
                    for (int iColumn = 0; iColumn <= 6; iColumn++)
                    {

                        // If nothing in the 1st row 
                        if (!StashSlotBlocked[iColumn, iRow])
                        {
                            bool bNotEnoughSpace = false;

                            // Bottom row of a page = no room
                            if (bBottomPageRow)
                                bNotEnoughSpace = true;

                            // Already something in the stash in the 2nd row)
                            else if (StashSlotBlocked[iColumn, iRow + 1])
                                bNotEnoughSpace = true;
                            if (!bNotEnoughSpace)
                            {
                                iPointX = iColumn;
                                iPointY = iRow;
                                goto FoundStashLocation;
                            }
                        }
                    }
                }
            }

 // 2 slot item?

            // Now deal with any leftover 1-slot items
            else
            {

                // First we try and find somewhere "sensible"
                for (int iRow = 0; iRow <= 29; iRow++)
                {
                    bool bTopPageRow = (iRow == 0 || iRow == 10 || iRow == 20);
                    bool bBottomPageRow = (iRow == 9 || iRow == 19 || iRow == 29);
                    for (int iColumn = 0; iColumn <= 6; iColumn++)
                    {

                        // Nothing in this slot
                        if (!StashSlotBlocked[iColumn, iRow])
                        {
                            bool bSensibleLocation = false;
                            if (!bTopPageRow && !bBottomPageRow)
                            {

                                // Something above and below this slot, or an odd-numbered row, so put something here
                                if ((StashSlotBlocked[iColumn, iRow + 1] && StashSlotBlocked[iColumn, iRow - 1]) ||
                                    (iRow) % 2 != 0)
                                    bSensibleLocation = true;
                            }

                            // Top page row with something directly underneath already blocking
                            else if (bTopPageRow)
                            {
                                if (StashSlotBlocked[iColumn, iRow + 1])
                                    bSensibleLocation = true;
                            }

                            // Bottom page row with something directly over already blocking
                            else
                            {
                                bSensibleLocation = true;
                            }

                            // Sensible location? Yay, stash it here!
                            if (bSensibleLocation)
                            {
                                iPointX = iColumn;
                                iPointY = iRow;

                                // Keep looking for places if it's a stackable to try to stick it at the end
                                if (!bOriginalIsStackable)
                                    goto FoundStashLocation;
                            }
                        }
                    }
                }

                // Didn't find a "sensible" place, let's try and force it in absolutely anywhere
                if ((iPointX < 0) || (iPointY < 0))
                {
                    for (int iRow = 0; iRow <= 29; iRow++)
                    {
                        for (int iColumn = 0; iColumn <= 6; iColumn++)
                        {

                            // Nothing in this spot, we're good!
                            if (!StashSlotBlocked[iColumn, iRow])
                            {
                                iPointX = iColumn;
                                iPointY = iRow;

                                // Keep looking for places if it's a stackable to try to stick it at the end
                                if (!bOriginalIsStackable)
                                    goto FoundStashLocation;
                            }
                        }
                    }
                }
            }
        FoundStashLocation:
            if ((iPointX < 0) || (iPointY < 0))
            {
                DbHelper.Log(TrinityLogLevel.Debug, LogCategory.UserInformation, "Fatal Error: No valid stash location found for '{0}' [{1} - {2}]", sOriginalItemName, sOriginalInternalName, OriginalGilesItemType);
                DbHelper.Log(TrinityLogLevel.Normal, LogCategory.UserInformation, "*");
                DbHelper.Log(TrinityLogLevel.Normal, LogCategory.UserInformation, "GSError: Emergency Stop: You need to stash an item but no valid space could be found. Stash is full? Stopping the bot to prevent infinite town-run loop.");
                BotMain.Stop();
                return false;
            }

            // We have two valid points that are empty, move the object here!
            StashSlotBlocked[iPointX, iPointY] = true;
            if (bOriginalTwoSlot)
                StashSlotBlocked[iPointX, iPointY + 1] = true;
            ZetaDia.Me.Inventory.MoveItem(iOriginalDynamicID, iPlayerDynamicID, InventorySlot.PlayerSharedStash, iPointX, iPointY);
            return true;
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

        internal class Decorators
        {
            internal static Decorator GetPreStashDecorator()
            {
                return new Decorator(ctx => GilesPreStashPauseOverlord(ctx),
                    new Sequence(
                        new Action(ctx => GilesStashPrePause(ctx)),
                        new Action(ctx => GilesStashPause(ctx))
                    )
                );
            }

            internal static Decorator GetStashDecorator()
            {
                return new Decorator(ctx => GilesStashOverlord(ctx),
                    new Sequence(
                        new Action(ctx => GilesOptimisedPreStash(ctx)),
                        new Action(ctx => GilesOptimisedStash(ctx)),
                        new Action(ctx => GilesOptimisedPostStash(ctx)),
                        new Sequence(
                            new Action(ctx => GilesStashPrePause(ctx)),
                            new Action(ctx => GilesStashPause(ctx))
                        )
                    )
                );
            }

            internal static Decorator GetSellDecorator()
            {
                return new Decorator(ctx => GilesSellOverlord(ctx),
                    new Sequence(
                        new Action(ctx => GilesOptimisedPreSell(ctx)),
                        new Action(ctx => GilesOptimisedSell(ctx)),
                        new Action(ctx => GilesOptimisedPostSell(ctx)),
                        new Sequence(
                            new Action(ctx => GilesStashPrePause(ctx)),
                            new Action(ctx => GilesStashPause(ctx))
                        )
                    )
                );
            }

            internal static Decorator GetSalvageDecorator()
            {
                return new Decorator(ctx => GilesSalvageOverlord(ctx),
                    new Sequence(
                        new Action(ctx => GilesOptimisedPreSalvage(ctx)),
                        new Action(ctx => GilesOptimisedSalvage(ctx)),
                        new Action(ctx => GilesOptimisedPostSalvage(ctx)),
                        new Sequence(
                            new Action(ctx => GilesStashPrePause(ctx)),
                            new Action(ctx => GilesStashPause(ctx))
                        )
                    )
                );
            }
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
                    GilesTrinity.BackpackSlotBlocked[iColumn, iRow] = false;

            foreach (ACDItem item in ZetaDia.Me.Inventory.Backpack)
            {
                int inventoryRow = item.InventoryRow;
                int inventoryColumn = item.InventoryColumn;

                // Mark this slot as not-free
                GilesTrinity.BackpackSlotBlocked[inventoryColumn, inventoryRow] = true;

                // Try and reliably find out if this is a two slot item or not
                GItemType tempItemType = GilesTrinity.DetermineItemType(item.InternalName, item.ItemType, item.FollowerSpecialType);
                if (GilesTrinity.DetermineIsTwoSlot(tempItemType) && inventoryRow < 5)
                {
                    GilesTrinity.BackpackSlotBlocked[inventoryColumn, inventoryRow + 1] = true;
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
                GItemType itemType = GilesTrinity.DetermineItemType(item.InternalName, item.ItemType, item.FollowerSpecialType);

                bool isTwoSlot = GilesTrinity.DetermineIsTwoSlot(itemType);
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

                double ItemValue = GilesTrinity.ValueThisItem(thiscacheditem, itemType);
                double NeedScore = GilesTrinity.ScoreNeeded(itemType);

                // Ignore stackable items
                if (!GilesTrinity.DetermineIsStackable(itemType) && itemType != GItemType.StaffOfHerding)
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
                vFreeSlot = GilesTrinity.SortingFindLocationBackpack(thisstashsort.bIsTwoSlot);
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
                    GilesTrinity.BackpackSlotBlocked[(int)vFreeSlot.X, (int)vFreeSlot.Y] = true;
                    if (thisstashsort.bIsTwoSlot)
                        GilesTrinity.BackpackSlotBlocked[(int)vFreeSlot.X, (int)vFreeSlot.Y + 1] = true;
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
                    GilesTrinity.BackpackSlotBlocked[thisstashsort.iInventoryColumn, thisstashsort.InventoryRow] = false;
                    if (thisstashsort.bIsTwoSlot)
                        GilesTrinity.BackpackSlotBlocked[thisstashsort.iInventoryColumn, thisstashsort.InventoryRow + 1] = false;
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

    }
}
