using GilesTrinity.Notifications;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Zeta;
using Zeta.Common;
using Zeta.Common.Plugins;
using Zeta.CommonBot;
using Zeta.Internals;
using Zeta.Internals.Actors;
using Zeta.Internals.Actors.Gizmos;
using Zeta.TreeSharp;


namespace GilesTrinity
{
    public partial class GilesTrinity : IPlugin
    {
        /// <summary>
        /// Sell Validation - Determines what should or should not be sold to vendor
        /// </summary>
        /// <param name="thisinternalname"></param>
        /// <param name="thislevel"></param>
        /// <param name="thisquality"></param>
        /// <param name="thisdbitemtype"></param>
        /// <param name="thisfollowertype"></param>
        /// <returns></returns>
        private static bool GilesSellValidation(string thisinternalname, int thislevel, ItemQuality thisquality, ItemType thisdbitemtype, FollowerType thisfollowertype)
        {

            // Check this isn't something we want to salvage
            if (settings.bSalvageJunk)
            {
                if (GilesSalvageValidation(thisinternalname, thislevel, thisquality, thisdbitemtype, thisfollowertype))
                    return false;
            }

            // Make sure it's not legendary
            //if (thisquality >= ItemQuality.Legendary)
            //    return false;
            GilesItemType thisGilesItemType = DetermineItemType(thisinternalname, thisdbitemtype, thisfollowertype);
            GilesBaseItemType thisGilesBaseType = DetermineBaseType(thisGilesItemType);
            switch (thisGilesBaseType)
            {
                case GilesBaseItemType.WeaponRange:
                case GilesBaseItemType.WeaponOneHand:
                case GilesBaseItemType.WeaponTwoHand:
                case GilesBaseItemType.Armor:
                case GilesBaseItemType.Offhand:
                case GilesBaseItemType.Jewelry:
                case GilesBaseItemType.FollowerItem:
                    return true;
                case GilesBaseItemType.Gem:
                case GilesBaseItemType.Misc:
                case GilesBaseItemType.Unknown:
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
        private static bool GilesSalvageValidation(string thisinternalname, int thislevel, ItemQuality thisquality, ItemType thisdbitemtype, FollowerType thisfollowertype)
        {
            if (!settings.bSalvageJunk)
                return false;

            // Make sure it's not legendary
            //if (thisquality >= ItemQuality.Legendary)
            //    return false;
            GilesItemType thisGilesItemType = DetermineItemType(thisinternalname, thisdbitemtype, thisfollowertype);
            GilesBaseItemType thisGilesBaseType = DetermineBaseType(thisGilesItemType);
            switch (thisGilesBaseType)
            {
                case GilesBaseItemType.WeaponRange:
                case GilesBaseItemType.WeaponOneHand:
                case GilesBaseItemType.WeaponTwoHand:
                case GilesBaseItemType.Armor:
                case GilesBaseItemType.Offhand:
                    if (thislevel >= 61 && thisquality >= ItemQuality.Magic1)
                    {
                        return true;
                    }
                    return false;
                case GilesBaseItemType.Jewelry:
                    if (thislevel >= 59 && thisquality >= ItemQuality.Magic1)
                    {
                        return true;
                    }
                    return false;
                case GilesBaseItemType.FollowerItem:
                    if (thislevel >= 60 && thisquality >= ItemQuality.Magic1)
                    {
                        return true;
                    }
                    return false;
                case GilesBaseItemType.Gem:
                case GilesBaseItemType.Misc:
                case GilesBaseItemType.Unknown:
                    return false;
            }

            // Switch giles base item type
            return false;
        }

        private static DateTime lastTownRunAttempt = DateTime.Now;

        /// <summary>
        /// TownRunCheckOverlord - determine if we should do a town-run or not
        /// </summary>
        /// <param name="ret"></param>
        /// <returns></returns>
        private static bool GilesTownRunCheckOverlord(object ret)
        {
            bWantToTownRun = false;

            bool CombatCheck = GilesTrinity.GilesGlobalOverlord(null);

            if (CombatCheck)
                return false;

            // Check if we should be forcing a town-run
            if (bGilesForcedVendoring || Zeta.CommonBot.Logic.BrainBehavior.IsVendoring)
            {
                if (!bLastTownRunCheckResult)
                {
                    bPreStashPauseDone = false;
                    if (Zeta.CommonBot.Logic.BrainBehavior.IsVendoring)
                    {
                        Log("Looks like we are being asked to force a town-run by a profile/plugin/new DB feature, now doing so.");
                    }
                }
                bWantToTownRun = true;
            }

            // Time safety switch for more advanced town-run checking to prevent CPU spam
            else if (DateTime.Now.Subtract(timeLastAttemptedTownRun).TotalSeconds > 6)
            {
                timeLastAttemptedTownRun = DateTime.Now;

                // Check for no space in backpack
                Vector2 ValidLocation = FindValidBackpackLocation(true);
                if (ValidLocation.X < 0 || ValidLocation.Y < 0)
                {
                    Log("No more space to pickup a 2-slot item, now running town-run routine.");
                    if (!bLastTownRunCheckResult)
                    {
                        bPreStashPauseDone = false;
                        bLastTownRunCheckResult = true;
                    }
                    bWantToTownRun = true;
                    //return true;
                }

                // Check durability percentages
                foreach (ACDItem tempitem in ZetaDia.Me.Inventory.Equipped)
                {
                    if (tempitem.BaseAddress != IntPtr.Zero)
                    {
                        if (tempitem.DurabilityPercent <= Zeta.CommonBot.Settings.CharacterSettings.Instance.RepairWhenDurabilityBelow)
                        {
                            Log("Items may need repair, now running town-run routine.");
                            if (!bLastTownRunCheckResult)
                            {
                                bPreStashPauseDone = false;
                            }
                            bWantToTownRun = true;
                        }
                    }
                }
            }

            // hax for Town running in Act 2 Soulstone Chamber
            if (ZetaDia.CurrentWorldId == 60193)
            {
                bWantToTownRun = false;
            }

            if (Zeta.CommonBot.ErrorDialog.IsVisible)
            {
                bWantToTownRun = false;
            }

            bLastTownRunCheckResult = bWantToTownRun;

            return bWantToTownRun;
        }

        // Safety pauses to make sure we aren't still coming through the portal or selling
        private static bool bPreStashPauseDone = false;
        private static double iPreStashLoops = 0;
        private static bool GilesPreStashPauseOverlord(object ret)
        {
            return (!bPreStashPauseDone);
        }
        private static RunStatus GilesStashPrePause(object ret)
        {
            bPreStashPauseDone = true;
            iPreStashLoops = 0;
            return RunStatus.Success;
        }
        private static RunStatus GilesStashPause(object ret)
        {
            iPreStashLoops++;
            if (iPreStashLoops < 30)
                return RunStatus.Running;
            return RunStatus.Success;
        }

        /// <summary>
        /// Stash Overlord values all items and checks if we have anything to stash
        /// </summary>
        /// <param name="ret"></param>
        /// <returns></returns>
        private static bool GilesStashOverlord(object ret)
        {
            hashGilesCachedKeepItems = new HashSet<GilesCachedACDItem>();
            bNeedsEquipmentRepairs = false;
            bGilesForcedVendoring = false;
            bool bShouldVisitStash = false;
            foreach (ACDItem thisitem in ZetaDia.Me.Inventory.Backpack)
            {
                if (thisitem.BaseAddress != IntPtr.Zero)
                {

                    // Find out if this item's in a protected bag slot
                    if (!ItemManager.ItemIsProtected(thisitem))
                    {
                        GilesCachedACDItem thiscacheditem = new GilesCachedACDItem(thisitem.InternalName, thisitem.Name, thisitem.Level, thisitem.ItemQualityLevel, thisitem.Gold, thisitem.GameBalanceId,
                            thisitem.DynamicId, thisitem.Stats.WeaponDamagePerSecond, thisitem.IsOneHand, thisitem.IsTwoHand, thisitem.DyeType, thisitem.ItemType, thisitem.ItemBaseType, thisitem.FollowerSpecialType,
                            thisitem.IsUnidentified, thisitem.ItemStackQuantity, thisitem.Stats);
                        bool bShouldStashThis = settings.bUseGilesFilters ? ShouldWeStashThis(thiscacheditem) : ItemManager.ShouldStashItem(thisitem);
                        if (bShouldStashThis)
                        {
                            hashGilesCachedKeepItems.Add(thiscacheditem);
                            bShouldVisitStash = true;
                        }
                    }
                }
                else
                {
                    Log("GSError: Diablo 3 memory read error, or item became invalid [StashOver-1]", true);
                }
            }
            return bShouldVisitStash;
        }

        /// <summary>
        /// Pre Stash prepares stuff for our stash run
        /// </summary>
        /// <param name="ret"></param>
        /// <returns></returns>
        private static RunStatus GilesOptimisedPreStash(object ret)
        {
            if (settings.bDebugInfo)
                BotMain.StatusText = "Town run: Stash routine started";
            Log("GSDebug: Stash routine started.", true);
            bLoggedAnythingThisStash = false;
            bUpdatedStashMap = false;
            iCurrentItemLoops = 0;
            RandomizeTheTimer();
            return RunStatus.Success;
        }

        /// <summary>
        /// Post Stash tidies up and signs off log file after a stash run
        /// </summary>
        /// <param name="ret"></param>
        /// <returns></returns>
        private static RunStatus GilesOptimisedPostStash(object ret)
        {
            Log("GSDebug: Stash routine ending sequence...", true);

            // Lock memory (probably not actually necessary anymore, since we handle all item stuff ourselves!?)
            using (ZetaDia.Memory.AcquireFrame())
            {
                ZetaDia.Actors.Update();
            }
            if (bLoggedAnythingThisStash)
            {
                FileStream LogStream = null;
                try
                {
                    LogStream = File.Open(sTrinityPluginPath + ZetaDia.Service.CurrentHero.BattleTagName + " - StashLog - " + ZetaDia.Actors.Me.ActorClass.ToString() + ".log", FileMode.Append, FileAccess.Write, FileShare.Read);
                    using (StreamWriter LogWriter = new StreamWriter(LogStream))
                        LogWriter.WriteLine("");
                    LogStream.Close();
                    if (settings.bEnableEmail && EmailMessage.Length > 0)
                        NotificationManager.SendEmail(sEmailAddress, sEmailAddress, "New DB stash loot - " + sBotName, EmailMessage.ToString(), SmtpServer, sEmailPassword);
                    EmailMessage.Clear();
                }
                catch (IOException)
                {
                    Log("Fatal Error: File access error for signing off the stash log file.");
                    if (LogStream != null)
                        LogStream.Close();
                }
                bLoggedAnythingThisStash = false;
            }
            Log("GSDebug: Stash routine finished.", true);
            return RunStatus.Success;
        }

        /// <summary>
        /// Lovely smooth one-at-a-time stashing routine
        /// </summary>
        /// <param name="ret"></param>
        /// <returns></returns>
        private static RunStatus GilesOptimisedStash(object ret)
        {
            ZetaDia.Actors.Update();
            if (ZetaDia.Actors.Me == null)
            {
                Log("GSError: Diablo 3 memory read error, or item became invalid [CoreStash-1]", true);
                return RunStatus.Failure;
            }
            Vector3 vectorPlayerPosition = ZetaDia.Me.Position;
            Vector3 vectorStashLocation = new Vector3(0f, 0f, 0f);
            DiaObject objPlayStash = ZetaDia.Actors.GetActorsOfType<GizmoPlayerSharedStash>(true).FirstOrDefault<GizmoPlayerSharedStash>();
            if (objPlayStash != null)
                vectorStashLocation = objPlayStash.Position;
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
            if (!UIElements.StashWindow.IsVisible)
            {
                objPlayStash.Interact();
                return RunStatus.Running;
            }
            if (!bUpdatedStashMap)
            {

                // Array for what blocks are or are not blocked
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

                // Map out all the items already in the stash
                foreach (ACDItem tempitem in ZetaDia.Me.Inventory.StashItems)
                {
                    if (tempitem.BaseAddress != IntPtr.Zero)
                    {
                        int inventoryRow = tempitem.InventoryRow;
                        int inventoryColumn = tempitem.InventoryColumn;

                        // Mark this slot as not-free
                        GilesStashSlotBlocked[inventoryColumn, inventoryRow] = true;

                        // Try and reliably find out if this is a two slot item or not
                        GilesItemType tempItemType = DetermineItemType(tempitem.InternalName, tempitem.ItemType, tempitem.FollowerSpecialType);
                        if (DetermineIsTwoSlot(tempItemType) && inventoryRow != 19 && inventoryRow != 9 && inventoryRow != 29)
                        {
                            GilesStashSlotBlocked[inventoryColumn, inventoryRow + 1] = true;
                        }
                        else if (DetermineIsTwoSlot(tempItemType) && (inventoryRow == 19 || inventoryRow == 9 || inventoryRow == 29))
                        {
                            Log("GSError: DemonBuddy thinks this item is 2 slot even though it's at bottom row of a stash page: " + tempitem.Name + " [" + tempitem.InternalName +
                                "] type=" + tempItemType.ToString() + " @ slot " + (inventoryRow + 1).ToString() + "/" +
                                (inventoryColumn + 1).ToString(), true);
                        }
                    }
                }

                // Loop through all stash items
                bUpdatedStashMap = true;
            }

            // Need to update the stash map?
            if (hashGilesCachedKeepItems.Count > 0)
            {
                iCurrentItemLoops++;
                if (iCurrentItemLoops < iItemDelayLoopLimit)
                    return RunStatus.Running;
                iCurrentItemLoops = 0;
                RandomizeTheTimer();
                GilesCachedACDItem thisitem = hashGilesCachedKeepItems.FirstOrDefault();
                bool bDidStashSucceed = GilesStashAttempt(thisitem);
                if (!bDidStashSucceed)
                    Log("There was an unknown error stashing an item.", true);
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
        private static bool GilesSellOverlord(object ret)
        {
            bGilesForcedVendoring = false;
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

            // Check for anything to sell
            foreach (ACDItem thisitem in ZetaDia.Me.Inventory.Backpack)
            {
                if (thisitem.BaseAddress != IntPtr.Zero)
                {
                    if (!ItemManager.ItemIsProtected(thisitem))
                    {
                        GilesCachedACDItem thiscacheditem = new GilesCachedACDItem(thisitem.InternalName, thisitem.Name, thisitem.Level, thisitem.ItemQualityLevel, thisitem.Gold, thisitem.GameBalanceId,
                            thisitem.DynamicId, thisitem.Stats.WeaponDamagePerSecond, thisitem.IsOneHand, thisitem.IsTwoHand, thisitem.DyeType, thisitem.ItemType, thisitem.ItemBaseType, thisitem.FollowerSpecialType,
                            thisitem.IsUnidentified, thisitem.ItemStackQuantity, thisitem.Stats);
                        bool bShouldSellThis = settings.bUseGilesFilters
                            ? GilesSellValidation(thiscacheditem.InternalName, thiscacheditem.Level, thiscacheditem.Quality, thiscacheditem.DBItemType, thiscacheditem.FollowerType)
                            : ItemManager.ShouldSellItem(thisitem);

                        // if it has gems, always salvage
                        if (thisitem.NumSocketsFilled > 0)
                        {
                            bShouldSellThis = false;
                        }

                        // Don't sell stuff that we want to salvage, if using custom loot-rules
                        if (!settings.bUseGilesFilters && ItemManager.ShouldSalvageItem(thisitem))
                        {
                            bShouldSellThis = false;
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
                    Log("GSError: Diablo 3 memory read error, or item became invalid [SellOver-1]", true);
                }
            }
            return bShouldVisitVendor;
        }

        /// <summary>
        /// Pre Sell sets everything up ready for running to vendor
        /// </summary>
        /// <param name="ret"></param>
        /// <returns></returns>
        private static RunStatus GilesOptimisedPreSell(object ret)
        {
            if (settings.bDebugInfo)
                BotMain.StatusText = "Town run: Sell routine started";
            Log("GSDebug: Sell routine started.", true);
            ZetaDia.Actors.Update();
            if (ZetaDia.Actors.Me == null)
            {
                Log("GSError: Diablo 3 memory read error, or item became invalid [PreSell-1]", true);
                return RunStatus.Failure;
            }
            bGoToSafetyPointFirst = true;
            bGoToSafetyPointSecond = false;
            bLoggedJunkThisStash = false;
            bCurrentlyMoving = false;
            bReachedDestination = false;
            iCurrentItemLoops = 0;
            RandomizeTheTimer();
            return RunStatus.Success;
        }

        /// <summary>
        /// Sell Routine replacement for smooth one-at-a-time item selling and handling
        /// </summary>
        /// <param name="ret"></param>
        /// <returns></returns>
        private static RunStatus GilesOptimisedSell(object ret)
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
                    else if (iLastDistance == iDistanceFromSell)
                    {
                        ZetaDia.Me.UsePower(SNOPower.Walk, vectorSellLocation, ZetaDia.Me.WorldDynamicId);
                    }
                    return RunStatus.Running;
                }
                iLastDistance = iDistanceFromSell;
                if (iDistanceFromSell > 120f)
                    return RunStatus.Failure;
                if (iDistanceFromSell > 8f)
                {
                    ZetaDia.Me.UsePower(SNOPower.Walk, vectorSellLocation, ZetaDia.Me.WorldDynamicId);
                    bCurrentlyMoving = true;
                    return RunStatus.Running;
                }
                bCurrentlyMoving = false;
                bGoToSafetyPointFirst = false;
                /*if (ZetaDia.CurrentAct == Act.A2)
                    bGoToSafetyPointSecond = true;*/
                return RunStatus.Running;
            }
            /*if (bGoToSafetyPointSecond)
            {
                Vector3 vectorPlayerPosition = ZetaDia.Me.Position;
                Vector3 vectorSellLocation = new Vector3(0f, 0f, 0f);
                switch (ZetaDia.CurrentAct)
                {
                    case Act.A1:
                        vectorSellLocation = new Vector3(2941.904f, 2812.825f, 24.04533f); break;
                    case Act.A2:
                        vectorSellLocation = new Vector3(295.0274f, 156.2243f, -1.834799f); break;
                    case Act.A3:
                    case Act.A4:
                        vectorSellLocation = new Vector3(410.6073f, 355.8762f, 0.1000005f); break;
                }
                float iDistanceFromSell = Vector3.Distance(vectorPlayerPosition, vectorSellLocation);
                if (bCurrentlyMoving)
                {
                    if (iDistanceFromSell <= 8f)
                    {
                        bGoToSafetyPointSecond = false;
                        bCurrentlyMoving = false;
                    }
                    else if (iLastDistance == iDistanceFromSell)
                    {
                        try
                        {
                            ZetaDia.Me.UsePower(SNOPower.Walk, vectorSellLocation, ZetaDia.Me.WorldDynamicId);
                        }
                        catch
                        {
                            Log("GSError: Diablo 3 move command error [CoreSell-1: " + thisGilesDiaItem.ThisInternalName + "]", true);
                        }
                    }
                    return RunStatus.Running;
                }
                iLastDistance = iDistanceFromSell;
                if (iDistanceFromSell > 120f)
                    return RunStatus.Failure;
                if (iDistanceFromSell > 8f)
                {
                    ZetaDia.Me.UsePower(SNOPower.Walk, vectorSellLocation, ZetaDia.Me.WorldDynamicId);
                    bCurrentlyMoving = true;
                    return RunStatus.Running;
                }
                bCurrentlyMoving = false;
                bGoToSafetyPointSecond = false;
                return RunStatus.Running;
            }*/
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
                DiaUnit objSellNavigation = ZetaDia.Actors.GetActorsOfType<DiaUnit>(true).FirstOrDefault<DiaUnit>(u => u.Name.ToLower().StartsWith(sVendorName));
                if (objSellNavigation != null)
                    vectorSellLocation = objSellNavigation.Position;
                float iDistanceFromSell = Vector3.Distance(vectorPlayerPosition, vectorSellLocation);
                if (bCurrentlyMoving)
                {
                    if (iDistanceFromSell <= 9.5f)
                    {
                        bReachedDestination = true;
                        if (objSellNavigation == null)
                            return RunStatus.Failure;
                        objSellNavigation.Interact();
                    }
                    else if (iLastDistance == iDistanceFromSell)
                    {
                        ZetaDia.Me.UsePower(SNOPower.Walk, vectorSellLocation, ZetaDia.Me.WorldDynamicId);
                    }
                    return RunStatus.Running;
                }
                iLastDistance = iDistanceFromSell;
                if (iDistanceFromSell > 120f)
                    return RunStatus.Failure;
                if (iDistanceFromSell > 9.5f)
                {
                    bCurrentlyMoving = true;
                    ZetaDia.Me.UsePower(SNOPower.Walk, vectorSellLocation, ZetaDia.Me.WorldDynamicId);
                    return RunStatus.Running;
                }
                bReachedDestination = true;
                if (objSellNavigation == null)
                    return RunStatus.Failure;
                objSellNavigation.Interact();
            }
            if (!Zeta.Internals.UIElement.IsValidElement(12123456831356216535L) || !Zeta.Internals.UIElement.FromHash(12123456831356216535L).IsVisible)
            {
                DiaUnit objSellNavigation = ZetaDia.Actors.GetActorsOfType<DiaUnit>(true).FirstOrDefault<DiaUnit>(u => u.Name.ToLower().StartsWith(sVendorName));
                if (objSellNavigation == null)
                    return RunStatus.Failure;
                objSellNavigation.Interact();
                return RunStatus.Running;
            }
            if (hashGilesCachedSellItems.Count > 0)
            {
                iCurrentItemLoops++;
                if (iCurrentItemLoops < iItemDelayLoopLimit)
                    return RunStatus.Running;
                iCurrentItemLoops = 0;
                RandomizeTheTimer();
                GilesCachedACDItem thisitem = hashGilesCachedSellItems.FirstOrDefault();

                // Item log for cool stuff sold
                if (thisitem != null)
                {
                    GilesItemType OriginalGilesItemType = DetermineItemType(thisitem.InternalName, thisitem.DBItemType, thisitem.FollowerType);
                    GilesBaseItemType thisGilesBaseType = DetermineBaseType(OriginalGilesItemType);
                    if (thisGilesBaseType == GilesBaseItemType.WeaponTwoHand || thisGilesBaseType == GilesBaseItemType.WeaponOneHand || thisGilesBaseType == GilesBaseItemType.WeaponRange ||
                        thisGilesBaseType == GilesBaseItemType.Armor || thisGilesBaseType == GilesBaseItemType.Jewelry || thisGilesBaseType == GilesBaseItemType.Offhand ||
                        thisGilesBaseType == GilesBaseItemType.FollowerItem)
                    {
                        double iThisItemValue = ValueThisItem(thisitem, OriginalGilesItemType);
                        LogJunkItems(thisitem, thisGilesBaseType, OriginalGilesItemType, iThisItemValue);
                    }
                    ZetaDia.Me.Inventory.SellItem(thisitem.DynamicID);
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

        /// <summary>
        /// Post Sell tidies everything up and signs off junk log after selling
        /// </summary>
        /// <param name="ret"></param>
        /// <returns></returns>
        private static RunStatus GilesOptimisedPostSell(object ret)
        {
            Log("GSDebug: Sell routine ending sequence...", true);
            using (ZetaDia.Memory.AcquireFrame())
            {
                ZetaDia.Actors.Update();
            }

            // Always repair, but only if we have enough money
            if (bNeedsEquipmentRepairs && iLowestDurabilityFound < 20 && iLowestDurabilityFound > -1 && ZetaDia.Me.Inventory.Coinage < 40000)
            {
                Log("*");
                Log("Emergency Stop: You need repairs but don't have enough money. Stopping the bot to prevent infinite death loop.");
                BotMain.Stop();
            }
            ZetaDia.Me.Inventory.RepairEquippedItems();
            bNeedsEquipmentRepairs = false;
            if (bLoggedJunkThisStash)
            {
                FileStream LogStream = null;
                try
                {
                    LogStream = File.Open(sTrinityPluginPath + ZetaDia.Service.CurrentHero.BattleTagName + " - JunkLog - " + ZetaDia.Actors.Me.ActorClass.ToString() + ".log", FileMode.Append, FileAccess.Write, FileShare.Read);
                    using (StreamWriter LogWriter = new StreamWriter(LogStream))
                        LogWriter.WriteLine("");
                    LogStream.Close();
                }
                catch (IOException)
                {
                    Log("Fatal Error: File access error for signing off the junk log file.");
                    if (LogStream != null)
                        LogStream.Close();
                }
                bLoggedJunkThisStash = false;
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
            /*if (!bReachedSafety && ZetaDia.CurrentAct == Act.A2)
            {
                Vector3 vectorPlayerPosition = ZetaDia.Me.Position;
                Vector3 vectorSafeLocation = new Vector3(284.3047f, 212.2945f, 0.1f);
                float iDistanceFromSafety = Vector3.Distance(vectorPlayerPosition, vectorSafeLocation);
                if (bCurrentlyMoving)
                {
                    if (iDistanceFromSafety <= 8f)
                    {
                        bReachedSafety = true;
                        bCurrentlyMoving = false;
                    }
                    else if (iLastDistance == iDistanceFromSafety)
                    {
                        try
                        {
                            ZetaDia.Me.UsePower(SNOPower.Walk, vectorSafeLocation, ZetaDia.Me.WorldDynamicId);
                        }
                        catch
                        {
                            Log("GSError: Diablo 3 move command error [PostSell-1: " + thisGilesDiaItem.ThisInternalName + "]", true);
                        }
                    }
                    return RunStatus.Running;
                }
                iLastDistance = iDistanceFromSafety;
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
            }*/
            Log("GSDebug: Sell routine finished.", true);
            return RunStatus.Success;
        }

        /// <summary>
        /// Salvage Overlord determines if we should visit the blacksmith or not
        /// </summary>
        /// <param name="ret"></param>
        /// <returns></returns>
        private static bool GilesSalvageOverlord(object ret)
        {
            bGilesForcedVendoring = false;
            hashGilesCachedSalvageItems = new HashSet<GilesCachedACDItem>();
            bool bShouldVisitSmith = false;

            // Check for anything to salvage
            foreach (ACDItem thisitem in ZetaDia.Me.Inventory.Backpack)
            {
                if (thisitem.BaseAddress != IntPtr.Zero)
                {
                    if (!ItemManager.ItemIsProtected(thisitem))
                    {
                        GilesCachedACDItem thiscacheditem = new GilesCachedACDItem(thisitem.InternalName, thisitem.Name, thisitem.Level, thisitem.ItemQualityLevel, thisitem.Gold, thisitem.GameBalanceId,
                            thisitem.DynamicId, thisitem.Stats.WeaponDamagePerSecond, thisitem.IsOneHand, thisitem.IsTwoHand, thisitem.DyeType, thisitem.ItemType, thisitem.ItemBaseType, thisitem.FollowerSpecialType,
                            thisitem.IsUnidentified, thisitem.ItemStackQuantity, thisitem.Stats);
                        bool bShouldSalvageThis = settings.bUseGilesFilters ? GilesSalvageValidation(thiscacheditem.InternalName, thiscacheditem.Level, thiscacheditem.Quality, thiscacheditem.DBItemType, thiscacheditem.FollowerType) : ItemManager.ShouldSalvageItem(thisitem);
                        if (bShouldSalvageThis)
                        {
                            hashGilesCachedSalvageItems.Add(thiscacheditem);
                            bShouldVisitSmith = true;
                        }
                    }
                }
                else
                {
                    Log("GSError: Diablo 3 memory read error, or item became invalid [SalvageOver-1]", true);
                }
            }
            return bShouldVisitSmith;
        }

        /// <summary>
        /// Pre Salvage sets everything up ready for our blacksmith run
        /// </summary>
        /// <param name="ret"></param>
        /// <returns></returns>
        private static RunStatus GilesOptimisedPreSalvage(object ret)
        {
            if (settings.bDebugInfo)
                BotMain.StatusText = "Town run: Salvage routine started";
            Log("GSDebug: Salvage routine started.", true);
            ZetaDia.Actors.Update();
            if (ZetaDia.Actors.Me == null)
            {
                Log("GSError: Diablo 3 memory read error, or item became invalid [PreSalvage-1]", true);
                return RunStatus.Failure;
            }
            bGoToSafetyPointFirst = true;
            bGoToSafetyPointSecond = false;
            bLoggedJunkThisStash = false;
            bCurrentlyMoving = false;
            bReachedDestination = false;
            iCurrentItemLoops = 0;
            RandomizeTheTimer();
            return RunStatus.Success;
        }

        /// <summary>
        /// Nice smooth one-at-a-time salvaging replacement
        /// </summary>
        /// <param name="ret"></param>
        /// <returns></returns>
        private static RunStatus GilesOptimisedSalvage(object ret)
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
                    else if (iLastDistance == iDistanceFromSalvage)
                    {
                        ZetaDia.Me.UsePower(SNOPower.Walk, vectorSalvageLocation, ZetaDia.Me.WorldDynamicId);
                    }
                    return RunStatus.Running;
                }
                iLastDistance = iDistanceFromSalvage;
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
                    else if (iLastDistance == iDistanceFromSalvage)
                    {
                        ZetaDia.Me.UsePower(SNOPower.Walk, vectorSalvageLocation, ZetaDia.Me.WorldDynamicId);
                    }
                    return RunStatus.Running;
                }
                iLastDistance = iDistanceFromSalvage;
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
                    else if (iLastDistance == iDistanceFromSalvage)
                    {
                        ZetaDia.Me.UsePower(SNOPower.Walk, vectorSalvageLocation, ZetaDia.Me.WorldDynamicId);
                    }
                    return RunStatus.Running;
                }
                iLastDistance = iDistanceFromSalvage;
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
                iCurrentItemLoops++;
                if (iCurrentItemLoops < iItemDelayLoopLimit)
                    return RunStatus.Running;
                iCurrentItemLoops = 0;
                RandomizeTheTimer();
                GilesCachedACDItem thisitem = hashGilesCachedSalvageItems.FirstOrDefault();
                if (thisitem != null)
                {

                    // Item log for cool stuff stashed
                    GilesItemType OriginalGilesItemType = DetermineItemType(thisitem.InternalName, thisitem.DBItemType, thisitem.FollowerType);
                    GilesBaseItemType thisGilesBaseType = DetermineBaseType(OriginalGilesItemType);
                    if (thisGilesBaseType == GilesBaseItemType.WeaponTwoHand || thisGilesBaseType == GilesBaseItemType.WeaponOneHand || thisGilesBaseType == GilesBaseItemType.WeaponRange ||
                        thisGilesBaseType == GilesBaseItemType.Armor || thisGilesBaseType == GilesBaseItemType.Jewelry || thisGilesBaseType == GilesBaseItemType.Offhand ||
                        thisGilesBaseType == GilesBaseItemType.FollowerItem)
                    {
                        double iThisItemValue = ValueThisItem(thisitem, OriginalGilesItemType);
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
        private static RunStatus GilesOptimisedPostSalvage(object ret)
        {
            Log("GSDebug: Salvage routine ending sequence...", true);
            using (ZetaDia.Memory.AcquireFrame())
            {
                ZetaDia.Actors.Update();
            }
            if (bLoggedJunkThisStash)
            {
                FileStream LogStream = null;
                try
                {
                    LogStream = File.Open(sTrinityPluginPath + ZetaDia.Service.CurrentHero.BattleTagName + " - JunkLog - " + ZetaDia.Actors.Me.ActorClass.ToString() + ".log", FileMode.Append, FileAccess.Write, FileShare.Read);
                    using (StreamWriter LogWriter = new StreamWriter(LogStream))
                        LogWriter.WriteLine("");
                    LogStream.Close();
                }
                catch (IOException)
                {
                    Log("Fatal Error: File access error for signing off the junk log file.");
                    if (LogStream != null)
                        LogStream.Close();
                }
                bLoggedJunkThisStash = false;
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
                    else if (iLastDistance == iDistanceFromSafety)
                    {
                        ZetaDia.Me.UsePower(SNOPower.Walk, vectorSafeLocation, ZetaDia.Me.WorldDynamicId);
                    }
                    return RunStatus.Running;
                }
                iLastDistance = iDistanceFromSafety;
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
            Log("GSDebug: Salvage routine finished.", true);
            return RunStatus.Success;
        }

        /// <summary>
        /// Log the nice items we found and stashed
        /// </summary>
        /// <param name="thisgooditem"></param>
        /// <param name="thisgilesbaseitemtype"></param>
        /// <param name="thisgilesitemtype"></param>
        /// <param name="ithisitemvalue"></param>
        internal static void LogGoodItems(GilesCachedACDItem thisgooditem, GilesBaseItemType thisgilesbaseitemtype, GilesItemType thisgilesitemtype, double ithisitemvalue)
        {
            FileStream LogStream = null;
            try
            {
                LogStream = File.Open(sTrinityPluginPath + ZetaDia.Service.CurrentHero.BattleTagName + " - StashLog - " + ZetaDia.Actors.Me.ActorClass.ToString() + ".log", FileMode.Append, FileAccess.Write, FileShare.Read);
                using (StreamWriter LogWriter = new StreamWriter(LogStream))
                {
                    if (!bLoggedAnythingThisStash)
                    {
                        bLoggedAnythingThisStash = true;
                        LogWriter.WriteLine(DateTime.Now.ToString() + ":");
                        LogWriter.WriteLine("====================");
                    }
                    string sLegendaryString = "";
                    bool bShouldNotify = false;
                    if (thisgooditem.Quality >= ItemQuality.Legendary)
                    {
                        if (!settings.bEnableLegendaryNotifyScore)
                            bShouldNotify = true;
                        else if (settings.bEnableLegendaryNotifyScore && EvaluateItemScoreForNotification(thisgilesbaseitemtype, ithisitemvalue))
                            bShouldNotify = true;
                        if (bShouldNotify)
                            NotificationManager.AddNotificationToQueue(thisgooditem.RealName + " [" + thisgilesitemtype.ToString() +
                                "] (Score=" + ithisitemvalue.ToString() + ". " + sValueItemStatString + ")",
                                ZetaDia.Service.CurrentHero.Name + " new legendary!", ProwlNotificationPriority.Emergency);
                        sLegendaryString = " {legendary item}";

                        // Change made by bombastic
                        Logging.Write("+=+=+=+=+=+=+=+=+ LEGENDARY FOUND +=+=+=+=+=+=+=+=+");
                        Logging.Write("+  Name:       " + thisgooditem.RealName + " (" + thisgilesitemtype.ToString() + ")");
                        Logging.Write("+  Score:       " + Math.Round(ithisitemvalue).ToString());
                        Logging.Write("+  Attributes: " + sValueItemStatString);
                        Logging.Write("+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+");
                    }
                    else
                    {

                        // Check for non-legendary notifications
                        bShouldNotify = EvaluateItemScoreForNotification(thisgilesbaseitemtype, ithisitemvalue);
                        if (bShouldNotify)
                            NotificationManager.AddNotificationToQueue(thisgooditem.RealName + " [" + thisgilesitemtype.ToString() + "] (Score=" + ithisitemvalue.ToString() + ". " + sValueItemStatString + ")", ZetaDia.Service.CurrentHero.Name + " new item!", ProwlNotificationPriority.Emergency);
                    }
                    if (bShouldNotify)
                    {
                        EmailMessage.AppendLine(thisgilesbaseitemtype.ToString() + " - " + thisgilesitemtype.ToString() + " '" + thisgooditem.RealName + "'. Score = " + Math.Round(ithisitemvalue).ToString() + sLegendaryString)
                            .AppendLine("  " + sValueItemStatString)
                            .AppendLine();
                    }
                    LogWriter.WriteLine(thisgilesbaseitemtype.ToString() + " - " + thisgilesitemtype.ToString() + " '" + thisgooditem.RealName + "'. Score = " + Math.Round(ithisitemvalue).ToString() + sLegendaryString);
                    LogWriter.WriteLine("  " + sValueItemStatString);
                    LogWriter.WriteLine("");
                }
                LogStream.Close();
            }
            catch (IOException)
            {
                Log("Fatal Error: File access error for stash log file.");
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
        internal static void LogJunkItems(GilesCachedACDItem thisgooditem, GilesBaseItemType thisgilesbaseitemtype, GilesItemType thisgilesitemtype, double ithisitemvalue)
        {
            FileStream LogStream = null;
            try
            {
                LogStream = File.Open(sTrinityPluginPath + ZetaDia.Service.CurrentHero.BattleTagName + " - JunkLog - " + ZetaDia.Actors.Me.ActorClass.ToString() + ".log", FileMode.Append, FileAccess.Write, FileShare.Read);
                using (StreamWriter LogWriter = new StreamWriter(LogStream))
                {
                    if (!bLoggedJunkThisStash)
                    {
                        bLoggedJunkThisStash = true;
                        LogWriter.WriteLine(DateTime.Now.ToString() + ":");
                        LogWriter.WriteLine("====================");
                    }
                    string sLegendaryString = "";
                    if (thisgooditem.Quality >= ItemQuality.Legendary)
                        sLegendaryString = " {legendary item}";
                    LogWriter.WriteLine(thisgilesbaseitemtype.ToString() + " - " + thisgilesitemtype.ToString() + " '" + thisgooditem.RealName + "'. Score = " + Math.Round(ithisitemvalue).ToString() + sLegendaryString);
                    if (sJunkItemStatString != "")
                        LogWriter.WriteLine("  " + sJunkItemStatString);
                    else
                        LogWriter.WriteLine("  (no scorable attributes)");
                    LogWriter.WriteLine("");
                }
                LogStream.Close();
            }
            catch (IOException)
            {
                Log("Fatal Error: File access error for junk log file.");
                if (LogStream != null)
                    LogStream.Close();
            }
        }

        /// <summary>
        /// Stash replacement accurately and neatly finds a free stash location
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private static bool GilesStashAttempt(GilesCachedACDItem item)
        {
            int iPlayerDynamicID = ZetaDia.Me.CommonData.DynamicId;
            int iOriginalGameBalanceId = item.BalanceID;
            int iOriginalDynamicID = item.DynamicID;
            int iOriginalStackQuantity = item.ItemStackQuantity;
            string sOriginalItemName = item.RealName;
            string sOriginalInternalName = item.InternalName;
            GilesItemType OriginalGilesItemType = DetermineItemType(item.InternalName, item.DBItemType, item.FollowerType);
            GilesBaseItemType thisGilesBaseType = DetermineBaseType(OriginalGilesItemType);
            bool bOriginalTwoSlot = DetermineIsTwoSlot(OriginalGilesItemType);
            bool bOriginalIsStackable = DetermineIsStackable(OriginalGilesItemType);
            int iAttempts;
            if (_dictItemStashAttempted.TryGetValue(iOriginalDynamicID, out iAttempts))
            {
                Log("GSError: Detected a duplicate stash attempt, DB item mis-read error, now forcing this item as a 2-slot item");
                _dictItemStashAttempted[iOriginalDynamicID] = iAttempts + 1;
                bOriginalTwoSlot = true;
                bOriginalIsStackable = false;
                if (iAttempts > 6)
                {
                    Log("GSError: Detected an item stash loop risk, now re-mapping stash treating everything as 2-slot and re-attempting");

                    // Array for what blocks are or are not blocked
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

                    // Map out all the items already in the stash
                    foreach (ACDItem tempitem in ZetaDia.Me.Inventory.StashItems)
                    {
                        if (tempitem.BaseAddress != IntPtr.Zero)
                        {
                            int inventoryRow = tempitem.InventoryRow;
                            int inventoryColumn = tempitem.InventoryColumn;

                            // Mark this slot as not-free
                            GilesStashSlotBlocked[inventoryColumn, inventoryRow] = true;

                            // Try and reliably find out if this is a two slot item or not
                            GilesStashSlotBlocked[inventoryColumn, inventoryRow + 1] = true;
                            if (inventoryRow != 19 && inventoryRow != 9 && inventoryRow != 29)
                            {
                                GilesStashSlotBlocked[inventoryColumn, inventoryRow + 1] = true;
                            }
                        }
                    }
                }
                if (iAttempts > 15)
                {
                    Log("*");
                    Log("GSError: Emergency Stop: No matter what we tried, we couldn't prevent an infinite stash loop. Sorry. Now stopping the bot.");
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
            if (thisGilesBaseType == GilesBaseItemType.WeaponTwoHand || thisGilesBaseType == GilesBaseItemType.WeaponOneHand || thisGilesBaseType == GilesBaseItemType.WeaponRange ||
                thisGilesBaseType == GilesBaseItemType.Armor || thisGilesBaseType == GilesBaseItemType.Jewelry || thisGilesBaseType == GilesBaseItemType.Offhand ||
                thisGilesBaseType == GilesBaseItemType.FollowerItem)
            {
                double iThisItemValue = ValueThisItem(item, OriginalGilesItemType);
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
                        Log("GSError: Diablo 3 memory read error, or stash item became invalid [StashAttempt-5]", true);
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
                        if (!GilesStashSlotBlocked[iColumn, iRow])
                        {
                            bool bNotEnoughSpace = false;

                            // Bottom row of a page = no room
                            if (bBottomPageRow)
                                bNotEnoughSpace = true;

                            // Already something in the stash in the 2nd row)
                            else if (GilesStashSlotBlocked[iColumn, iRow + 1])
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
                        if (!GilesStashSlotBlocked[iColumn, iRow])
                        {
                            bool bSensibleLocation = false;
                            if (!bTopPageRow && !bBottomPageRow)
                            {

                                // Something above and below this slot, or an odd-numbered row, so put something here
                                if ((GilesStashSlotBlocked[iColumn, iRow + 1] && GilesStashSlotBlocked[iColumn, iRow - 1]) ||
                                    (iRow) % 2 != 0)
                                    bSensibleLocation = true;
                            }

                            // Top page row with something directly underneath already blocking
                            else if (bTopPageRow)
                            {
                                if (GilesStashSlotBlocked[iColumn, iRow + 1])
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
                            if (!GilesStashSlotBlocked[iColumn, iRow])
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
                Log("Fatal Error: No valid stash location found for '" + sOriginalItemName + "' [" + sOriginalInternalName + " - " + OriginalGilesItemType.ToString() + "]", true);
                Log("*");
                Log("GSError: Emergency Stop: You need to stash an item but no valid space could be found. Stash is full? Stopping the bot to prevent infinite town-run loop.");
                BotMain.Stop();
                return false;
            }

            // We have two valid points that are empty, move the object here!
            GilesStashSlotBlocked[iPointX, iPointY] = true;
            if (bOriginalTwoSlot)
                GilesStashSlotBlocked[iPointX, iPointY + 1] = true;
            ZetaDia.Me.Inventory.MoveItem(iOriginalDynamicID, iPlayerDynamicID, InventorySlot.PlayerSharedStash, iPointX, iPointY);
            return true;
        }

    }
}
