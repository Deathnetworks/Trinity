using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using Trinity.Helpers;
using Trinity.Items;
using Trinity.Notifications;
using Trinity.Settings.Loot;
using Trinity.Technicals;
using Zeta.Bot;
using Zeta.Bot.Coroutines;
using Zeta.Bot.Logic;
using Zeta.Bot.Profile;
using Zeta.Bot.Profile.Common;
using Zeta.Bot.Settings;
using Zeta.Common;
using Zeta.Game;
using Zeta.Game.Internals.Actors;
using Zeta.Game.Internals.Actors.Gizmos;
using Zeta.Game.Internals.SNO;
using Zeta.TreeSharp;
using Logger = Trinity.Technicals.Logger;

namespace Trinity.DbProvider
{
    internal class TownRun
    {
        // Whether salvage/sell run should go to a middle-waypoint first to help prevent stucks

        internal static bool LastTownRunCheckResult = false;
        // Random variables used during item handling and town-runs

        private static bool _loggedAnythingThisStash;

        private static bool _loggedJunkThisStash;
        internal static string ValueItemStatString = "";
        internal static string JunkItemStatString = "";
        internal static bool TestingBackpack = false;


        // DateTime check to prevent inventory-check spam when looking for repairs being needed
        internal static DateTime LastCheckBackpackDurability = DateTime.UtcNow;
        private static DateTime _LastCompletedTownRun = DateTime.MinValue;
        private static bool lastTownPortalCheckResult;
        private static DateTime lastTownPortalCheckTime = DateTime.MinValue;
        internal static Stopwatch randomTimer = new Stopwatch();
        internal static Random timerRandomizer = new Random();
        internal static int randomTimerVal = -1;
        internal static Stopwatch TownRunCheckTimer = new Stopwatch();

        static TownRun()
        {
            PreTownRunWorldId = -1;
            PreTownRunPosition = Vector3.Zero;
            WasVendoring = false;
        }


        internal static Vector3 PreTownRunPosition { get; set; }
        internal static int PreTownRunWorldId { get; set; }
        internal static bool WasVendoring { get; set; }

        /// <summary>
        ///     Called from Plugin.Pulse
        /// </summary>
        internal static void VendorRunPulseCheck()
        {
            // If we're in town and vendoring
            if (Trinity.Player.IsInTown && BrainBehavior.IsVendoring)
            {
                WasVendoring = true;
                Trinity.ForceVendorRunASAP = true;
            }
        }

        /// <summary>
        ///     Records the position when we first run out of bag space, so we can return to that same position after a town run
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
        ///     TownRunCheckOverlord - determine if we should do a town-run or not
        /// </summary>
        /// <param name="ret"></param>
        /// <returns></returns>
        internal static bool TownRunCanRun(object ret)
        {
            try
            {
                using (new PerformanceLogger("TownRunOverlord"))
                {
                    if (ZetaDia.Me == null || !ZetaDia.Me.IsValid)
                        return false;

                    Trinity.WantToTownRun = false;

                    if (Trinity.Player.IsDead)
                    {
                        return false;
                    }

                    // Check if we should be forcing a town-run
                    if (Trinity.ForceVendorRunASAP || BrainBehavior.IsVendoring)
                    {
                        if (!LastTownRunCheckResult)
                        {
                            if (BrainBehavior.IsVendoring)
                            {
                                Logger.Log("Looks like we are being asked to force a town-run by a profile/plugin/new DB feature, now doing so.");
                            }
                        }
                        SetPreTownRunPosition();
                        Trinity.WantToTownRun = true;
                    }

                    // Time safety switch for more advanced town-run checking to prevent CPU spam
                    if (DateTime.UtcNow.Subtract(LastCheckBackpackDurability).TotalSeconds > 6)
                    {
                        LastCheckBackpackDurability = DateTime.UtcNow;

                        // Check for no space in backpack
                        if (!Trinity.Player.ParticipatingInTieredLootRun)
                        {
                            Vector2 validLocation = TrinityItemManager.FindValidBackpackLocation(true);
                            if (validLocation.X < 0 || validLocation.Y < 0)
                            {
                                Logger.Log("No more space to pickup a 2-slot item, now running town-run routine. (TownRun)");
                                if (!LastTownRunCheckResult)
                                {
                                    LastTownRunCheckResult = true;
                                }
                                Trinity.WantToTownRun = true;

                                Trinity.ForceVendorRunASAP = true;
                                // Record the first position when we run out of bag space, so we can return later
                                SetPreTownRunPosition();
                            }
                        }
                        if (ZetaDia.Me.IsValid)
                        {
                            List<ACDItem> equippedItems = ZetaDia.Me.Inventory.Equipped.Where(i => i.DurabilityMax > 0 && i.DurabilityCurrent != i.DurabilityMax).ToList();
                            if (equippedItems.Any())
                            {
                                double min = equippedItems.Min(i => i.DurabilityPercent);

                                float threshold = Trinity.Player.IsInTown ? 0.50f : CharacterSettings.Instance.RepairWhenDurabilityBelow;
                                bool needsRepair = min <= threshold;

                                if (needsRepair)
                                {
                                    Logger.Log("Items may need repair, now running town-run routine.");

                                    Trinity.WantToTownRun = true;
                                    Trinity.ForceVendorRunASAP = true;
                                    SetPreTownRunPosition();
                                }
                            }
                        }
                    }

                    if (ErrorDialog.IsVisible)
                    {
                        Trinity.WantToTownRun = false;
                    }

                    LastTownRunCheckResult = Trinity.WantToTownRun;

                    // Clear blacklists to triple check any potential targets
                    if (Trinity.WantToTownRun)
                    {
                        Trinity.Blacklist3Seconds = new HashSet<int>();
                        Trinity.Blacklist15Seconds = new HashSet<int>();
                        Trinity.Blacklist60Seconds = new HashSet<int>();
                        Trinity.Blacklist90Seconds = new HashSet<int>();
                    }

                    // Fix for A1 new game with bags full
                    if (Trinity.Player.LevelAreaId == 19947 && ZetaDia.CurrentQuest.QuestSNO == 87700 && (ZetaDia.CurrentQuest.StepId == -1 || ZetaDia.CurrentQuest.StepId == 42))
                    {
                        Logger.Log(TrinityLogLevel.Verbose, LogCategory.UserInformation, "Can't townrun with the current quest!");
                        Trinity.WantToTownRun = false;
                    }

                    if (Trinity.WantToTownRun && !(BrainBehavior.IsVendoring || Trinity.Player.IsInTown))
                    {
                        string cantUseTPreason;
                        if (!ZetaDia.Me.CanUseTownPortal(out cantUseTPreason) && !ZetaDia.IsInTown)
                        {
                            Logger.Log(TrinityLogLevel.Verbose, LogCategory.UserInformation, "It appears we need to town run but can't: {0}", cantUseTPreason);
                            Trinity.WantToTownRun = false;
                        }
                    }


                    if (Trinity.WantToTownRun && DataDictionary.BossLevelAreaIDs.Contains(Trinity.Player.LevelAreaId))
                    {
                        Logger.Log(TrinityLogLevel.Debug, LogCategory.GlobalHandler, "Unable to Town Portal - Boss Area!");
                        return false;
                    }
                    if (Trinity.WantToTownRun && ZetaDia.IsInTown && DeathHandler.EquipmentNeedsEmergencyRepair())
                    {
                        Logger.Log(TrinityLogLevel.Debug, LogCategory.GlobalHandler, "EquipmentNeedsEmergencyRepair!");
                        return true;
                    }
                    if (Trinity.WantToTownRun && Trinity.CurrentTarget != null)
                    {
                        TownRunCheckTimer.Restart();
                        Logger.Log(TrinityLogLevel.Debug, LogCategory.GlobalHandler, "Restarting TownRunCheckTimer, we have a target!");
                        return false;
                    }

                    if (Trinity.WantToTownRun && DataDictionary.NeverTownPortalLevelAreaIds.Contains(Trinity.Player.LevelAreaId))
                    {
                        Logger.Log(TrinityLogLevel.Debug, LogCategory.GlobalHandler, "Unable to Town Portal in this area!");
                        return false;
                    }
                    if (Trinity.WantToTownRun && (TownRunTimerFinished() || BrainBehavior.IsVendoring))
                    {
                        //Logger.Log(TrinityLogLevel.Verbose, LogCategory.UserInformation, "Town run timer finished {0} or in town {1} or is vendoring {2} (TownRun)",
                        //    TownRunTimerFinished(), Trinity.Player.IsInTown, BrainBehavior.IsVendoring);
                        Trinity.WantToTownRun = false;
                        return true;
                    }
                    if (Trinity.WantToTownRun && !TownRunCheckTimer.IsRunning)
                    {
                        Logger.Log(TrinityLogLevel.Verbose, LogCategory.UserInformation, "Starting town run timer");
                        TownRunCheckTimer.Start();
                        _loggedAnythingThisStash = false;
                        _loggedJunkThisStash = false;
                    }
                    return false;
                }
            }
            catch (Exception ex)
            {
                Logger.LogError("Error Getting TownRun {0}", ex.Message);
                return false;
            }
        }

        public static async Task<bool> TownRunCoroutineWrapper(Decorator original)
        {
            foreach (Composite child in original.Children)
            {
                await child.ExecuteCoroutine();
            }

            if (!BrainBehavior.IsVendoring)
            {
                Logger.Log("TownRun complete");
                Trinity.WantToTownRun = false;
                Trinity.ForceVendorRunASAP = false;
                TownRunCheckTimer.Reset();
                SendEmailNotification();
                SendMobileNotifications();
            }
            return true;
        }

        internal static bool TownRunTimerFinished()
        {
            return ZetaDia.IsInTown || (TownRunCheckTimer.IsRunning && TownRunCheckTimer.ElapsedMilliseconds > 2000);
        }

        internal static bool TownRunTimerRunning()
        {
            return TownRunCheckTimer.IsRunning && TownRunCheckTimer.ElapsedMilliseconds < 2000;
        }

        /// <summary>
        ///     Returns if we're trying to TownRun or if profile tag is UseTownPortalTag
        /// </summary>
        /// <returns></returns>
        internal static bool IsTryingToTownPortal()
        {
            if (DateTime.UtcNow.Subtract(lastTownPortalCheckTime).TotalMilliseconds < Trinity.Settings.Advanced.CacheRefreshRate)
                return lastTownPortalCheckResult;

            bool result = false;

            if (Trinity.WantToTownRun)
                result = true;

            if (Trinity.ForceVendorRunASAP)
                result = true;

            if (TownRunCheckTimer.IsRunning)
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
            if (CurrentProfileBehavior != null)
            {
                Type profileBehaviortype = CurrentProfileBehavior.GetType();
                string behaviorName = profileBehaviortype.Name;
                if ((profileBehaviortype == typeof(UseTownPortalTag) ||
                     profileBehaviortype == typeof(WaitTimerTag) ||
                     behaviorName.ToLower().Contains("townrun") ||
                     behaviorName.ToLower().Contains("townportal")))
                {
                    result = true;
                }
            }

            if (BrainBehavior.IsVendoring)
                result = true;

            lastTownPortalCheckTime = DateTime.UtcNow;
            lastTownPortalCheckResult = result;
            return result;
        }

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

        internal static SalvageOption GetSalvageOption(ItemQuality qualityLevel)
        {
            if (qualityLevel > ItemQuality.Normal && qualityLevel <= ItemQuality.Superior)
            {
                return Trinity.Settings.Loot.TownRun.SalvageWhiteItemOption;
            }

            if (qualityLevel >= ItemQuality.Magic1 && qualityLevel <= ItemQuality.Magic3)
            {
                return Trinity.Settings.Loot.TownRun.SalvageBlueItemOption;
            }

            if (qualityLevel >= ItemQuality.Rare4 && qualityLevel <= ItemQuality.Rare6)
            {
                return Trinity.Settings.Loot.TownRun.SalvageYellowItemOption;
            }

            if (qualityLevel >= ItemQuality.Legendary)
            {
                return Trinity.Settings.Loot.TownRun.SalvageLegendaryItemOption;
            }
            return SalvageOption.Sell;
        }


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
        ///     Log the nice items we found and stashed
        /// </summary>
        internal static void LogGoodItems(CachedACDItem acdItem, GItemBaseType itemBaseType, GItemType itemType, double itemValue)
        {
            FileStream logStream = null;
            try
            {
                string filePath = Path.Combine(FileManager.LoggingPath, "StashLog - " + Trinity.Player.ActorClass + ".log");
                logStream = File.Open(filePath, FileMode.Append, FileAccess.Write, FileShare.Read);

                //TODO : Change File Log writing
                using (var logWriter = new StreamWriter(logStream))
                {
                    if (!_loggedAnythingThisStash)
                    {
                        _loggedAnythingThisStash = true;
                        logWriter.WriteLine(DateTime.Now + ":");
                        logWriter.WriteLine("====================");
                    }
                    string sLegendaryString = "";
                    bool shouldSendNotifications = false;

                    if (acdItem.Quality >= ItemQuality.Legendary)
                    {
                        if (!Trinity.Settings.Notification.LegendaryScoring)
                            shouldSendNotifications = true;
                        else if (Trinity.Settings.Notification.LegendaryScoring && ItemValuation.CheckScoreForNotification(itemBaseType, itemValue))
                            shouldSendNotifications = true;
                        if (shouldSendNotifications)
                            NotificationManager.AddNotificationToQueue(acdItem.RealName + " [" + itemType +
                                                                       "] (Score=" + itemValue + ". " + acdItem.AcdItem.Stats + ")",
                                ZetaDia.Service.Hero.Name + " new legendary!", ProwlNotificationPriority.Emergency);
                        sLegendaryString = " {legendary item}";

                        // Change made by bombastic
                        Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "+=+=+=+=+=+=+=+=+ LEGENDARY FOUND +=+=+=+=+=+=+=+=+");
                        Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "+  Name:       {0} ({1})", acdItem.RealName, itemType);
                        Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "+  Score:       {0:0}", itemValue);
                        Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "+  Attributes: {0}", acdItem.AcdItem.Stats.ToString());
                        Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+");
                    }
                    else
                    {
                        // Check for non-legendary notifications
                        shouldSendNotifications = ItemValuation.CheckScoreForNotification(itemBaseType, itemValue);
                        if (shouldSendNotifications)
                            NotificationManager.AddNotificationToQueue(acdItem.RealName + " [" + itemType + "] (Score=" + itemValue + ". " + acdItem.AcdItem.Stats + ")",
                                ZetaDia.Service.Hero.BattleTagName + " new item!", ProwlNotificationPriority.Normal);
                    }
                    if (shouldSendNotifications)
                    {
                        NotificationManager.EmailMessage.AppendLine(itemBaseType + " - " + itemType + " '" + acdItem.RealName + "'. Score = " + Math.Round(itemValue) + sLegendaryString)
                            .AppendLine("  " + acdItem.AcdItem.Stats)
                            .AppendLine();
                    }
                    logWriter.WriteLine(itemBaseType + " - " + itemType + " '" + acdItem.RealName + "'. Score = " + Math.Round(itemValue) + sLegendaryString);
                    logWriter.WriteLine("  " + acdItem.AcdItem.Stats);
                    logWriter.WriteLine("");
                }
                logStream.Close();
            }
            catch (IOException)
            {
                Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "Fatal Error: File access error for stash log file.");
                if (logStream != null)
                    logStream.Close();
            }
        }

        /// <summary>
        ///     Log the rubbish junk items we salvaged or sold
        /// </summary>
        internal static void LogJunkItems(CachedACDItem acdItem, GItemBaseType itemBaseType, GItemType itemType, double itemValue)
        {
            FileStream logStream = null;
            try
            {
                string filePath = Path.Combine(FileManager.LoggingPath, "JunkLog - " + Trinity.Player.ActorClass + ".log");
                logStream = File.Open(filePath, FileMode.Append, FileAccess.Write, FileShare.Read);
                using (var logWriter = new StreamWriter(logStream))
                {
                    if (!_loggedJunkThisStash)
                    {
                        _loggedJunkThisStash = true;
                        logWriter.WriteLine(DateTime.Now + ":");
                        logWriter.WriteLine("====================");
                    }
                    string isLegendaryItem = "";
                    if (acdItem.Quality >= ItemQuality.Legendary)
                        isLegendaryItem = " {legendary item}";
                    logWriter.WriteLine(itemBaseType + " - " + itemType + " '" + acdItem.RealName + "'. Score = " + itemValue.ToString("0") + isLegendaryItem);
                    if (JunkItemStatString != "")
                        logWriter.WriteLine("  " + JunkItemStatString);
                    else
                        logWriter.WriteLine("  (no scorable attributes)");
                    logWriter.WriteLine("");
                }
                logStream.Close();
            }
            catch (IOException)
            {
                Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "Fatal Error: File access error for junk log file.");
                if (logStream != null)
                    logStream.Close();
            }
        }
        public static Vector3 StashLocation
        {
            get
            {
                switch (Trinity.Player.LevelAreaId)
                {
                    case 19947: // Campaign A1 Hub
                        return new Vector3(2968.16f, 2789.63f, 23.94531f);
                    case 332339: // OpenWorld A1 Hub
                        return new Vector3(388.16f, 509.63f, 23.94531f);
                    case 168314: // A2 Hub
                        return new Vector3(323.0558f, 222.7048f, 0f);
                    case 92945: // A3/A4 Hub
                        return new Vector3(387.6834f, 382.0295f, 0f);
                    case 270011: // A5 Hub
                        return new Vector3(502.8296f, 739.7472f, 2.598635f);
                    default:
                        throw new ValueUnavailableException("Unknown LevelArea Id " + Trinity.Player.LevelAreaId);
                }
            }
        }

        public static DiaGizmo SharedStash
        {
            get
            {
                return ZetaDia.Actors.GetActorsOfType<GizmoPlayerSharedStash>()
                    .FirstOrDefault(o => o.IsFullyValid() && o.ActorInfo.IsValid && o.ActorInfo.GizmoType == GizmoType.SharedStash);
            }
        }


    }
}