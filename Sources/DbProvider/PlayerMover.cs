using GilesTrinity.Technicals;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Zeta;
using Zeta.Common;
using Zeta.CommonBot;
using Zeta.CommonBot.Profile.Common;
using Zeta.Internals.Actors;
using Zeta.Internals.SNO;
using Zeta.Navigation;
using Zeta.TreeSharp;
using GilesTrinity.XmlTags;
using Zeta.Pathfinding;
using System.Drawing;
using GilesTrinity.Settings.Combat;
using Zeta.Internals;

namespace GilesTrinity.DbProvider
{
    // Player Mover Class
    public class PlayerMover : IPlayerMover
    {
        private static readonly HashSet<int> hashAvoidLeapingToSNO = new HashSet<int> { 138989, 176074, 176076, 176077, 176536, 260330 };
        // 138989 = health pool, 176074 = protection, 176076 = fortune, 176077 = frenzied, 176536 = portal in leorics, 260330 = cooldown shrine
        // Exp shrines = ???? Other shrines ????

        private static bool ShrinesInArea(Vector3 targetpos)
        {
            return GilesTrinity.GilesObjectCache.Any(o => hashAvoidLeapingToSNO.Contains(o.ActorSNO) && Vector3.Distance(o.Position, targetpos) <= 10f);
        }

        public void MoveStop()
        {
            ZetaDia.Me.UsePower(SNOPower.Walk, ZetaDia.Me.Position, GilesTrinity.iCurrentWorldID, -1);
        }

        // Anti-stuck variables
        internal static Vector3 vOldMoveToTarget = Vector3.Zero;
        internal static int iTimesReachedStuckPoint = 0;
        internal static int iTotalAntiStuckAttempts = 1;
        internal static Vector3 vSafeMovementLocation = Vector3.Zero;
        internal static DateTime TimeLastRecordedPosition = DateTime.Today;
        internal static Vector3 vOldPosition = Vector3.Zero;
        internal static DateTime timeStartedUnstuckMeasure = DateTime.Today;
        internal static int iTimesReachedMaxUnstucks = 0;
        internal static DateTime timeCancelledUnstuckerFor = DateTime.Today;
        internal static DateTime timeLastReportedAnyStuck = DateTime.Today;
        internal static int iCancelUnstuckerForSeconds = 60;
        internal static DateTime timeLastRestartedGame = DateTime.Today;
        internal static bool UnStuckCheckerLastResult = false;

        // Store player current position
        public static Vector3 vMyCurrentPosition = Vector3.Zero;

        private static int lastKnowCoin;
        private static DateTime lastCheckBag;
        private static DateTime lastRefreshCoin;

        //For Tempest Rush Monks
        private static bool CanChannelTempestRush = false;

        /// <summary>
        /// Resets the gold inactivity timer
        /// </summary>
        internal static void ResetCheckGold()
        {
            lastCheckBag = DateTime.Now;
            lastRefreshCoin = DateTime.Now;
            lastKnowCoin = 0;
        }

        /// <summary>
        /// Determines whether or not to leave the game based on the gold inactivity timer
        /// </summary>
        /// <returns></returns>
        internal static bool GoldInactive()
        {
            if (!GilesTrinity.Settings.Advanced.GoldInactivityEnabled)
            {
                // timer isn't enabled so move along!
                ResetCheckGold();
                return false;
            }
            try
            {
                if (!ZetaDia.IsInGame)
                {
                    ResetCheckGold(); //If not in game, reset the timer
                    return false;
                }
                if (ZetaDia.IsLoadingWorld || lastCheckBag == null)
                    return false;
                if ((DateTime.Now.Subtract(lastCheckBag).TotalSeconds < 5))
                    return false;

                // sometimes bosses take a LONG time
                if (GilesTrinity.CurrentTarget != null && GilesTrinity.CurrentTarget.IsBoss)
                {
                    ResetCheckGold();
                    return false;
                }

                lastCheckBag = DateTime.Now;
                int currentcoin = ZetaDia.Me.Inventory.Coinage;

                if (currentcoin != lastKnowCoin && currentcoin != 0)
                {
                    lastRefreshCoin = DateTime.Now;
                    lastKnowCoin = currentcoin;
                }
                int notpickupgoldsec = Convert.ToInt32(DateTime.Now.Subtract(lastRefreshCoin).TotalSeconds);
                if (notpickupgoldsec >= GilesTrinity.Settings.Advanced.GoldInactivityTimer)
                {
                    DbHelper.Log(TrinityLogLevel.Normal, LogCategory.UserInformation, "Gold inactivity after {0}s. Sending abort.", notpickupgoldsec);
                    lastRefreshCoin = DateTime.Now;
                    lastKnowCoin = currentcoin;
                    notpickupgoldsec = 0;
                    return true;
                }
                else if (notpickupgoldsec > 0)
                {
                    DbHelper.Log(TrinityLogLevel.Normal, LogCategory.Movement, "Gold unchanged for {0}s", notpickupgoldsec);
                }
            }
            catch (Exception e)
            {
                DbHelper.Log(TrinityLogLevel.Normal, LogCategory.Movement, e.Message);
            }
            return false;
        }

        /// <summary>
        /// Check if we are stuck or not by simply checking for position changing max once every 3 seconds
        /// </summary>
        /// <param name="vMyCurrentPosition"></param>
        /// <param name="checkDuration"></param>
        /// <returns></returns>
        public static bool UnstuckChecker(Vector3 vMyCurrentPosition, int checkDuration = 3000)
        {
            // Keep checking distance changes every 3 seconds
            if (DateTime.Now.Subtract(TimeLastRecordedPosition).TotalMilliseconds >= checkDuration)
            {
                // We're not stuck if we're vendoring!
                if (GilesTrinity.ForceVendorRunASAP || Zeta.CommonBot.Logic.BrainBehavior.IsVendoring)
                {
                    UnStuckCheckerLastResult = false;
                    return UnStuckCheckerLastResult;
                }

                if (checkDuration >= 3000)
                {
                    TimeLastRecordedPosition = DateTime.Now;
                }

                Composite c = null;

                try
                {
                    if (ProfileManager.CurrentProfileBehavior != null)
                        c = ProfileManager.CurrentProfileBehavior.Behavior;
                }
                catch { }

                if (c != null && c.GetType() == typeof(WaitTimerTag))
                {
                    vOldPosition = Vector3.Zero;
                    ResetCheckGold();
                    UnStuckCheckerLastResult = false;
                    return UnStuckCheckerLastResult;
                }

                Zeta.Internals.UIElement vendorWindow = Zeta.Internals.UIElements.VendorWindow;

                // We're not stuck if we're doing stuff!
                if (ZetaDia.Me.IsInConversation || ZetaDia.IsPlayingCutscene || ZetaDia.IsLoadingWorld || (vendorWindow.IsValid && vendorWindow.IsVisible))
                {
                    vOldPosition = Vector3.Zero;
                    ResetCheckGold();
                    UnStuckCheckerLastResult = false;
                    return UnStuckCheckerLastResult;
                }

                AnimationState aState = ZetaDia.Me.CommonData.AnimationState;
                // We're not stuck if we're doing stuff!
                if (aState == AnimationState.Attacking ||
                    aState == AnimationState.Casting ||
                    aState == AnimationState.Channeling)
                {
                    vOldPosition = Vector3.Zero;
                    ResetCheckGold();
                    UnStuckCheckerLastResult = false;
                    return UnStuckCheckerLastResult;
                }

                if (vOldPosition != Vector3.Zero && vOldPosition.Distance(vMyCurrentPosition) <= 4f)
                {
                    UnStuckCheckerLastResult = true;
                    return UnStuckCheckerLastResult;
                }

                if (checkDuration >= 3000)
                {
                    vOldPosition = vMyCurrentPosition;
                }
            }

            // Return last result if within the specified timeframe
            return false;
        }
        public static bool UnstuckChecker()
        {
            return UnstuckChecker(vMyCurrentPosition);
        }
        public static Vector3 UnstuckHandler()
        {
            return UnstuckHandler(vMyCurrentPosition, vOldMoveToTarget);
        }
        // Actually deal with a stuck - find an unstuck point etc.
        public static Vector3 UnstuckHandler(Vector3 vMyCurrentPosition, Vector3 vOriginalDestination)
        {
            PathStack.Clear();

            if (GoldInactive())
            {
                GoldInactiveLeaveGame();
                return vOriginalDestination;
            }

            // Update the last time we generated a path
            timeStartedUnstuckMeasure = DateTime.Now;
            // If we got stuck on a 2nd/3rd/4th "chained" anti-stuck route, then return the old move to target to keep movement of some kind going
            if (iTimesReachedStuckPoint > 0)
            {
                vSafeMovementLocation = Vector3.Zero;
                iTimesReachedStuckPoint++;
                // Reset the path and allow a whole "New" unstuck generation next cycle
                iTimesReachedStuckPoint = 0;
                // And cancel unstucking for 9 seconds so DB can try to navigate
                iCancelUnstuckerForSeconds = (9 * iTotalAntiStuckAttempts);
                if (iCancelUnstuckerForSeconds < 20)
                    iCancelUnstuckerForSeconds = 20;
                timeCancelledUnstuckerFor = DateTime.Now;
                Navigator.Clear();
                DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.Movement, "Clearing old route and trying new path find to: " + vOldMoveToTarget.ToString());
                Navigator.MoveTo(vOldMoveToTarget, "original destination", false);
                return vSafeMovementLocation;
            }
            // Only try an unstuck 10 times maximum in XXX period of time
            if (Vector3.Distance(vOriginalDestination, vMyCurrentPosition) >= 700f)
            {
                DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.Movement, "You are " + Vector3.Distance(vOriginalDestination, vMyCurrentPosition).ToString() + " distance away from your destination.");
                DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.Movement, "This is too far for the unstucker, and is likely a sign of ending up in the wrong map zone.");
                iTotalAntiStuckAttempts = 20;
            }
            // intell
            if (iTotalAntiStuckAttempts <= 15)
            {
                DbHelper.Log(TrinityLogLevel.Normal, LogCategory.Movement, "Your bot got stuck! Trying to unstuck (attempt #{0} of 15 attempts) {1} {2} {3} {4}",
                    iTotalAntiStuckAttempts.ToString(),
                    "Act=\"" + ZetaDia.CurrentAct + "\"",
                    "questId=\"" + ZetaDia.CurrentQuest.QuestSNO + "\"",
                    "stepId=\"" + ZetaDia.CurrentQuest.StepId + "\"",
                    "worldId=\"" + ZetaDia.CurrentWorldId + "\""
                );

                DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.Movement, "(destination=" + vOriginalDestination.ToString() + ", which is " + Vector3.Distance(vOriginalDestination, vMyCurrentPosition).ToString() + " distance away)");
                //GilesTrinity.PlayerStatus.CurrentPosition = vMyCurrentPosition;
                vSafeMovementLocation = GilesTrinity.FindSafeZone(true, iTotalAntiStuckAttempts, vMyCurrentPosition);
                // Temporarily log stuff
                if (iTotalAntiStuckAttempts == 1 && GilesTrinity.Settings.Advanced.LogStuckLocation)
                {
                    FileStream LogStream = File.Open(Path.Combine(FileManager.LoggingPath, "Stucks - " + GilesTrinity.PlayerStatus.ActorClass.ToString() + ".log"), FileMode.Append, FileAccess.Write, FileShare.Read);
                    using (StreamWriter LogWriter = new StreamWriter(LogStream))
                    {
                        LogWriter.WriteLine(DateTime.Now.ToString() + ": Original Destination=" + vOldMoveToTarget.ToString() + ". Current player position when stuck=" + vMyCurrentPosition.ToString());
                        LogWriter.WriteLine("Profile Name=" + ProfileManager.CurrentProfile.Name);
                    }
                    LogStream.Close();
                }
                // Now count up our stuck attempt generations
                iTotalAntiStuckAttempts++;
                return vSafeMovementLocation;
            }
            iTimesReachedMaxUnstucks++;
            iTotalAntiStuckAttempts = 1;
            vSafeMovementLocation = Vector3.Zero;
            vOldPosition = Vector3.Zero;
            iTimesReachedStuckPoint = 0;
            TimeLastRecordedPosition = DateTime.Today;
            timeStartedUnstuckMeasure = DateTime.Today;
            // int iSafetyLoops = 0;
            if (iTimesReachedMaxUnstucks == 1)
            {
                Navigator.Clear();
                DbHelper.Log(TrinityLogLevel.Normal, LogCategory.Movement, "Anti-stuck measures now attempting to kickstart DB's path-finder into action.");
                Navigator.MoveTo(vOriginalDestination, "original destination", false);
                iCancelUnstuckerForSeconds = 40;
                timeCancelledUnstuckerFor = DateTime.Now;
                return vSafeMovementLocation;
            }
            if (iTimesReachedMaxUnstucks == 2)
            {
                DbHelper.Log(TrinityLogLevel.Normal, LogCategory.Movement, "Anti-stuck measures failed. Now attempting to reload current profile.");

                Navigator.Clear();

                ProfileManager.Load(Zeta.CommonBot.ProfileManager.CurrentProfile.Path);
                DbHelper.Log(TrinityLogLevel.Normal, LogCategory.UserInformation, "Anti-stuck successfully reloaded current profile, DemonBuddy now navigating again.");
                return vSafeMovementLocation;

                // Didn't make it to town, so skip instantly to the exit game system
                //iTimesReachedMaxUnstucks = 3;
            }
            // Exit the game and reload the profile
            if (GilesTrinity.Settings.Advanced.AllowRestartGame && DateTime.Now.Subtract(timeLastRestartedGame).TotalMinutes >= 15)
            {
                timeLastRestartedGame = DateTime.Now;
                string sUseProfile = GilesTrinity.sFirstProfileSeen;
                DbHelper.Log(TrinityLogLevel.Normal, LogCategory.UserInformation, "Anti-stuck measures exiting current game.");
                // Load the first profile seen last run
                ProfileManager.Load(!string.IsNullOrEmpty(sUseProfile)
                                        ? sUseProfile
                                        : Zeta.CommonBot.ProfileManager.CurrentProfile.Path);
                Thread.Sleep(1000);
                GilesTrinity.GilesResetEverythingNewGame();
                ZetaDia.Service.Games.LeaveGame();
                // Wait for 10 second log out timer if not in town
                if (!ZetaDia.Me.IsInTown)
                {
                    Thread.Sleep(10000);
                }
            }
            else
            {
                DbHelper.Log(TrinityLogLevel.Normal, LogCategory.UserInformation, "Unstucking measures failed. Now stopping Trinity unstucker for 12 minutes to inactivity timers to kick in or DB to auto-fix.");
                iCancelUnstuckerForSeconds = 720;
                timeCancelledUnstuckerFor = DateTime.Now;
                return vSafeMovementLocation;
            }
            return vSafeMovementLocation;
        }
        // Handle moveto requests from the current routine/profile
        // This replaces DemonBuddy's own built-in "Basic movement handler" with a custom one
        private static Vector3 vLastMoveTo = Vector3.Zero;
        private static bool bLastWaypointWasTown = false;
        private static HashSet<Vector3> hashDoneThisVector = new HashSet<Vector3>();
        private static Vector3 vShiftedPosition = Vector3.Zero;
        private static DateTime lastShiftedPosition = DateTime.Today;
        private static int iShiftPositionFor = 0;

        private static Vector3 lastMovementPosition = Vector3.Zero;
        private static DateTime lastRecordedPosition = DateTime.Now;
        internal static double MovementSpeed { get; private set; }

        private static List<SpeedSensor> SpeedSensors = new List<SpeedSensor>();
        private static int MaxSpeedSensors = 3;

        public static double GetMovementSpeed()
        {
            // If we just used a spell, we "moved"
            if (DateTime.Now.Subtract(GilesTrinity.lastGlobalCooldownUse).TotalMilliseconds <= 1000)
                return 1d;

            // Minimum of 2 records to calculate speed
            if (!SpeedSensors.Any() || SpeedSensors.Count <= 1)
                return 0d;

            // Check if we have enough recorded positions, remove one if so
            while (SpeedSensors.Count > MaxSpeedSensors - 1)
            {
                // first sensors
                SpeedSensors.Remove(SpeedSensors.OrderBy(s => s.Timestamp).FirstOrDefault());
            }
            // Clean up the stack
            if (SpeedSensors.Any(s => s.WorldID != GilesTrinity.iCurrentWorldID))
            {
                SpeedSensors.Clear();
                return 0d;
            }

            double AverageRecordingTime = SpeedSensors.Average(s => s.TimeSinceLastMove.TotalHours); ;
            double averageMovementSpeed = SpeedSensors.Average(s => Vector3.Distance(s.Location, vMyCurrentPosition) * 1000000);

            return averageMovementSpeed / AverageRecordingTime;
        }

        public void MoveTowards(Vector3 vMoveToTarget)
        {
            if (!ZetaDia.IsInGame || !ZetaDia.Me.IsValid || ZetaDia.Me.IsDead || ZetaDia.IsLoadingWorld)
            {
                return;
            }

            vMyCurrentPosition = ZetaDia.Me.Position;

            // record speed once per second
            if (DateTime.Now.Subtract(lastRecordedPosition).TotalMilliseconds >= 500)
            {
                // Record our current location and time
                if (!SpeedSensors.Any())
                {
                    SpeedSensors.Add(new SpeedSensor()
                    {
                        Location = vMyCurrentPosition,
                        TimeSinceLastMove = new TimeSpan(0),
                        Distance = 0f,
                        WorldID = GilesTrinity.iCurrentWorldID
                    });
                }
                else
                {
                    SpeedSensor lastSensor = SpeedSensors.OrderByDescending(s => s.Timestamp).FirstOrDefault();
                    SpeedSensors.Add(new SpeedSensor()
                     {
                         Location = vMyCurrentPosition,
                         TimeSinceLastMove = new TimeSpan(DateTime.Now.Subtract(lastSensor.TimeSinceLastMove).Ticks),
                         Distance = Vector3.Distance(vMyCurrentPosition, lastSensor.Location),
                         WorldID = GilesTrinity.iCurrentWorldID
                     });
                }

                lastRecordedPosition = DateTime.Now;
                // Set the public variable
                MovementSpeed = GetMovementSpeed();
            }

            // rrrix-note: This really shouldn't be here... 
            // Recording of all the XML's in use this run
            RecordLastProfile();


            vMoveToTarget = WarnAndLogLongPath(vMoveToTarget);

            // Make sure GilesTrinity doesn't want us to avoid routine-movement
            //if (GilesTrinity.bDontMoveMeIAmDoingShit)
            //    return;
            // Store player current position

            // Store distance to current moveto target
            float DestinationDistance;
            DestinationDistance = vMyCurrentPosition.Distance2D(vMoveToTarget);
            vOldMoveToTarget = vMoveToTarget;

            // Do unstuckery things
            if (GilesTrinity.Settings.Advanced.UnstuckerEnabled)
            {
                if (GoldInactive())
                {
                    GoldInactiveLeaveGame();
                    return;
                }
                // Store the "real" (not anti-stuck) destination
                // See if we can reset the 10-limit unstuck counter, if >120 seconds since we last generated an unstuck location
                if (iTotalAntiStuckAttempts > 1 && DateTime.Now.Subtract(timeStartedUnstuckMeasure).TotalSeconds >= 120)
                {
                    iTotalAntiStuckAttempts = 1;
                    iTimesReachedStuckPoint = 0;
                    vSafeMovementLocation = Vector3.Zero;
                    GilesTrinity.UsedStuckSpots = new List<GilesTrinity.GridPoint>();
                }
                // See if we need to, and can, generate unstuck actions
                if (DateTime.Now.Subtract(timeCancelledUnstuckerFor).TotalSeconds > iCancelUnstuckerForSeconds && UnstuckChecker(vMyCurrentPosition))
                {
                    // Record the time we last apparently couldn't move for a brief period of time
                    timeLastReportedAnyStuck = DateTime.Now;
                    // See if there's any stuck position to try and navigate to generated by random mover
                    vSafeMovementLocation = UnstuckHandler(vMyCurrentPosition, vOldMoveToTarget);
                    if (vSafeMovementLocation == Vector3.Zero)
                        return;
                }
                // See if we can clear the total unstuckattempts if we haven't been stuck in over 6 minutes.
                if (DateTime.Now.Subtract(timeLastReportedAnyStuck).TotalSeconds >= 360)
                {
                    iTimesReachedMaxUnstucks = 0;
                }
                // Did we have a safe point already generated (eg from last loop through), if so use it as our current location instead
                if (vSafeMovementLocation != Vector3.Zero)
                {
                    // Set our current movement target to the safe point we generated last cycle
                    vMoveToTarget = vSafeMovementLocation;
                }
                // Get distance to current destination
                // Remove the stuck position if it's been reached, this bit of code also creates multiple stuck-patterns in an ever increasing amount
                if (vSafeMovementLocation != Vector3.Zero && DestinationDistance <= 3f)
                {
                    vSafeMovementLocation = Vector3.Zero;
                    iTimesReachedStuckPoint++;
                    // Do we want to immediately generate a 2nd waypoint to "chain" anti-stucks in an ever-increasing path-length?
                    if (iTimesReachedStuckPoint <= iTotalAntiStuckAttempts)
                    {
                        //GilesTrinity.PlayerStatus.CurrentPosition = vMyCurrentPosition;
                        vSafeMovementLocation = GilesTrinity.FindSafeZone(true, iTotalAntiStuckAttempts, vMyCurrentPosition);
                        vMoveToTarget = vSafeMovementLocation;
                    }
                    else
                    {
                        if (GilesTrinity.Settings.Advanced.LogCategories.HasFlag(LogCategory.Movement))
                            DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.Movement, "Clearing old route and trying new path find to: " + vOldMoveToTarget.ToString());
                        // Reset the path and allow a whole "New" unstuck generation next cycle
                        iTimesReachedStuckPoint = 0;
                        // And cancel unstucking for 9 seconds so DB can try to navigate
                        iCancelUnstuckerForSeconds = (9 * iTotalAntiStuckAttempts);
                        if (iCancelUnstuckerForSeconds < 20)
                            iCancelUnstuckerForSeconds = 20;
                        timeCancelledUnstuckerFor = DateTime.Now;

                        Navigator.Clear();
                        Navigator.MoveTo(vOldMoveToTarget, "original destination", false);

                        return;
                    }
                }
            }
            // Is the built-in unstucker enabled or not?
            // if (GilesTrinity.Settings.Advanced.DebugInStatusBar)
            // {
            //    Logging.WriteDiagnostic("[Trinity] Moving toward <{0:0},{1:0},{2:0}> distance: {3:0}", vMoveToTarget.X, vMoveToTarget.Y, vMoveToTarget.Z, fDistanceFromTarget);
            // }
            // See if there's an obstacle in our way, if so try to navigate around it
            Vector3 point = vMoveToTarget;
            foreach (GilesObstacle obstacle in GilesTrinity.hashNavigationObstacleCache.Where(o =>
                            GilesTrinity.GilesIntersectsPath(o.Location, o.Radius, vMyCurrentPosition, point)))
            {
                if (vShiftedPosition == Vector3.Zero)
                {
                    // Make sure we only shift max once every 1 second
                    if (DateTime.Now.Subtract(lastShiftedPosition).TotalSeconds >= 1)
                    {
                        GetShiftedPosition(ref vMoveToTarget, ref point, obstacle.Radius + 5f);
                    }
                }
                else
                {
                    if (DateTime.Now.Subtract(lastShiftedPosition).TotalMilliseconds <= iShiftPositionFor)
                    {
                        vMoveToTarget = vShiftedPosition;
                    }
                    else
                    {
                        vShiftedPosition = Vector3.Zero;
                    }
                }
            }

            // See if we can use abilities like leap etc. for movement out of combat, but not in town
            if (GilesTrinity.Settings.Combat.Misc.AllowOOCMovement && !ZetaDia.Me.IsInTown && !GilesTrinity.bDontMoveMeIAmDoingShit)
            {
                bool bTooMuchZChange = (Math.Abs(vMyCurrentPosition.Z - vMoveToTarget.Z) >= 4f);

                // Leap movement for a barb
                if (GilesTrinity.Hotbar.Contains(SNOPower.Barbarian_Leap) &&
                    DateTime.Now.Subtract(GilesTrinity.dictAbilityLastUse[SNOPower.Barbarian_Leap]).TotalMilliseconds >= GilesTrinity.dictAbilityRepeatDelay[SNOPower.Barbarian_Leap] &&
                    DestinationDistance >= 20f &&
                    PowerManager.CanCast(SNOPower.Barbarian_Leap) && !ShrinesInArea(vMoveToTarget))
                {
                    Vector3 vThisTarget = vMoveToTarget;
                    if (DestinationDistance > 35f)
                        vThisTarget = MathEx.CalculatePointFrom(vMoveToTarget, vMyCurrentPosition, 35f);
                    ZetaDia.Me.UsePower(SNOPower.Barbarian_Leap, vThisTarget, GilesTrinity.iCurrentWorldID, -1);
                    GilesTrinity.dictAbilityLastUse[SNOPower.Barbarian_Leap] = DateTime.Now;
                    if (GilesTrinity.Settings.Advanced.LogCategories.HasFlag(LogCategory.Movement))
                        DbHelper.Log(TrinityLogLevel.Debug, LogCategory.Movement, "Using Leap for OOC movement, distance={0}", DestinationDistance);
                    return;
                }
                // Furious Charge movement for a barb
                if (GilesTrinity.Hotbar.Contains(SNOPower.Barbarian_FuriousCharge) && !bTooMuchZChange &&
                    DateTime.Now.Subtract(GilesTrinity.dictAbilityLastUse[SNOPower.Barbarian_FuriousCharge]).TotalMilliseconds >= GilesTrinity.dictAbilityRepeatDelay[SNOPower.Barbarian_FuriousCharge] &&
                    DestinationDistance >= 20f &&
                    PowerManager.CanCast(SNOPower.Barbarian_FuriousCharge) && !ShrinesInArea(vMoveToTarget))
                {
                    Vector3 vThisTarget = vMoveToTarget;
                    if (DestinationDistance > 35f)
                        vThisTarget = MathEx.CalculatePointFrom(vMoveToTarget, vMyCurrentPosition, 35f);
                    ZetaDia.Me.UsePower(SNOPower.Barbarian_FuriousCharge, vThisTarget, GilesTrinity.iCurrentWorldID, -1);
                    GilesTrinity.dictAbilityLastUse[SNOPower.Barbarian_FuriousCharge] = DateTime.Now;
                    if (GilesTrinity.Settings.Advanced.LogCategories.HasFlag(LogCategory.Movement))
                        DbHelper.Log(TrinityLogLevel.Debug, LogCategory.Movement, "Using Furious Charge for OOC movement, distance={0}", DestinationDistance);
                    return;
                }
                // Vault for a DH - maximum set by user-defined setting
                if (GilesTrinity.Hotbar.Contains(SNOPower.DemonHunter_Vault) && !bTooMuchZChange &&
                    DateTime.Now.Subtract(GilesTrinity.dictAbilityLastUse[SNOPower.DemonHunter_Vault]).TotalMilliseconds >= GilesTrinity.Settings.Combat.DemonHunter.VaultMovementDelay &&
                    DestinationDistance >= 18f &&
                    PowerManager.CanCast(SNOPower.DemonHunter_Vault) && !ShrinesInArea(vMoveToTarget) &&
                    // Don't Vault into avoidance/monsters if we're kiting
                    (GilesTrinity.PlayerKiteDistance <= 0 || (GilesTrinity.PlayerKiteDistance > 0 &&
                     (!GilesTrinity.hashAvoidanceObstacleCache.Any(a => a.Location.Distance(vMoveToTarget) <= GilesTrinity.PlayerKiteDistance) ||
                     (!GilesTrinity.hashAvoidanceObstacleCache.Any(a => MathEx.IntersectsPath(a.Location, a.Radius, GilesTrinity.PlayerStatus.CurrentPosition, vMoveToTarget))) ||
                     !GilesTrinity.hashMonsterObstacleCache.Any(a => a.Location.Distance(vMoveToTarget) <= GilesTrinity.PlayerKiteDistance))))
                    )
                {

                    Vector3 vThisTarget = vMoveToTarget;
                    if (DestinationDistance > 35f)
                        vThisTarget = MathEx.CalculatePointFrom(vMoveToTarget, vMyCurrentPosition, 35f);
                    ZetaDia.Me.UsePower(SNOPower.DemonHunter_Vault, vThisTarget, GilesTrinity.iCurrentWorldID, -1);
                    GilesTrinity.dictAbilityLastUse[SNOPower.DemonHunter_Vault] = DateTime.Now;
                    if (GilesTrinity.Settings.Advanced.LogCategories.HasFlag(LogCategory.Movement))
                        DbHelper.Log(TrinityLogLevel.Debug, LogCategory.Movement, "Using Vault for OOC movement, distance={0}", DestinationDistance);
                    return;
                }
                // Tempest rush for a monk
                if (GilesTrinity.Hotbar.Contains(SNOPower.Monk_TempestRush) &&
                    (GilesTrinity.Settings.Combat.Monk.TROption == TempestRushOption.MovementOnly || GilesTrinity.Settings.Combat.Monk.TROption == TempestRushOption.Always))
                {
                    float aimPointDistance = 10f;
                    Vector3 vTargetAimPoint = vMoveToTarget;
                    if (DestinationDistance > aimPointDistance)
                    {
                        vTargetAimPoint = MathEx.CalculatePointFrom(vMoveToTarget, vMyCurrentPosition, aimPointDistance);
                    }
                    bool canRayCastTarget = GilesTrinity.GilesCanRayCast(vMyCurrentPosition, vTargetAimPoint, NavCellFlags.AllowWalk);

                    if (!CanChannelTempestRush &&
                        GilesTrinity.PlayerStatus.PrimaryResource >= GilesTrinity.Settings.Combat.Monk.TR_MinSpirit &&
                        DestinationDistance >= GilesTrinity.Settings.Combat.Monk.TR_MinDist &&
                        canRayCastTarget)
                    {
                        CanChannelTempestRush = true;
                    }
                    else if (CanChannelTempestRush && (GilesTrinity.PlayerStatus.PrimaryResource < 10f))
                    {
                        CanChannelTempestRush = false;
                    }
                    if (CanChannelTempestRush)
                    {
                        if (GilesTrinity.GilesUseTimer(SNOPower.Monk_TempestRush))
                        {
                            ZetaDia.Me.UsePower(SNOPower.Monk_TempestRush, vTargetAimPoint, GilesTrinity.iCurrentWorldID, -1);

                            // simulate movement speed of 30
                            SpeedSensor lastSensor = SpeedSensors.OrderByDescending(s => s.Timestamp).FirstOrDefault();
                            SpeedSensors.Add(new SpeedSensor()
                            {
                                Location = vMyCurrentPosition,
                                TimeSinceLastMove = new TimeSpan(0, 0, 0, 0, 500),
                                Distance = 30f,
                                WorldID = GilesTrinity.iCurrentWorldID
                            });

                            GilesTrinity.MaintainTempestRush = true;
                            GilesTrinity.dictAbilityLastUse[SNOPower.Monk_TempestRush] = DateTime.Now;
                            GilesTrinity.LastPowerUsed = SNOPower.Monk_TempestRush;

                            if (GilesTrinity.Settings.Advanced.LogCategories.HasFlag(LogCategory.Movement))
                                DbHelper.Log(TrinityLogLevel.Debug, LogCategory.Movement, "Using Tempest Rush for OOC movement, distance={0:0}",
                                    DestinationDistance);
                        }
                        return;
                    }
                    else
                    {
                        if (GilesTrinity.Settings.Advanced.LogCategories.HasFlag(LogCategory.Movement))
                            DbHelper.Log(TrinityLogLevel.Debug, LogCategory.Movement,
                            "Tempest rush failed!: {0:00.0} / {1} distance: {2:00.0} / {3} Raycast: {4} MS: {5:0.0}",
                            GilesTrinity.PlayerStatus.PrimaryResource,
                            GilesTrinity.Settings.Combat.Monk.TR_MinSpirit,
                            DestinationDistance,
                            GilesTrinity.Settings.Combat.Monk.TR_MinDist,
                            canRayCastTarget, 
                            GetMovementSpeed());

                        GilesTrinity.MaintainTempestRush = false;
                    }
                }
                // Teleport for a wizard (need to be able to check skill rune in DB for a 3-4 teleport spam in a row)
                if (GilesTrinity.Hotbar.Contains(SNOPower.Wizard_Teleport) &&
                    DateTime.Now.Subtract(GilesTrinity.dictAbilityLastUse[SNOPower.Wizard_Teleport]).TotalMilliseconds >= GilesTrinity.dictAbilityRepeatDelay[SNOPower.Wizard_Teleport] &&
                    DestinationDistance >= 20f &&
                    PowerManager.CanCast(SNOPower.Wizard_Teleport) && !ShrinesInArea(vMoveToTarget))
                {
                    Vector3 vThisTarget = vMoveToTarget;
                    if (DestinationDistance > 35f)
                        vThisTarget = MathEx.CalculatePointFrom(vMoveToTarget, vMyCurrentPosition, 35f);
                    ZetaDia.Me.UsePower(SNOPower.Wizard_Teleport, vThisTarget, GilesTrinity.iCurrentWorldID, -1);
                    GilesTrinity.dictAbilityLastUse[SNOPower.Wizard_Teleport] = DateTime.Now;
                    if (GilesTrinity.Settings.Advanced.LogCategories.HasFlag(LogCategory.Movement))
                        DbHelper.Log(TrinityLogLevel.Debug, LogCategory.Movement, "Using Teleport for OOC movement, distance={0}", DestinationDistance);
                    return;
                }
                // Archon Teleport for a wizard 
                if (GilesTrinity.Hotbar.Contains(SNOPower.Wizard_Archon_Teleport) &&
                    DateTime.Now.Subtract(GilesTrinity.dictAbilityLastUse[SNOPower.Wizard_Archon_Teleport]).TotalMilliseconds >= GilesTrinity.dictAbilityRepeatDelay[SNOPower.Wizard_Archon_Teleport] &&
                    DestinationDistance >= 20f &&
                    PowerManager.CanCast(SNOPower.Wizard_Archon_Teleport) && !ShrinesInArea(vMoveToTarget))
                {
                    Vector3 vThisTarget = vMoveToTarget;
                    if (DestinationDistance > 35f)
                        vThisTarget = MathEx.CalculatePointFrom(vMoveToTarget, vMyCurrentPosition, 35f);
                    ZetaDia.Me.UsePower(SNOPower.Wizard_Archon_Teleport, vThisTarget, GilesTrinity.iCurrentWorldID, -1);
                    GilesTrinity.dictAbilityLastUse[SNOPower.Wizard_Archon_Teleport] = DateTime.Now;
                    if (GilesTrinity.Settings.Advanced.LogCategories.HasFlag(LogCategory.Movement))
                        DbHelper.Log(TrinityLogLevel.Debug, LogCategory.Movement, "Using Archon Teleport for OOC movement, distance={0}", DestinationDistance);
                    return;
                }
            }

            if (vMyCurrentPosition.Distance2D(vMoveToTarget) > 1f)
            {
                // Default movement
                ZetaDia.Me.UsePower(SNOPower.Walk, vMoveToTarget, GilesTrinity.iCurrentWorldID, -1);

                if (GilesTrinity.Settings.Advanced.LogCategories.HasFlag(LogCategory.Movement))
                    DbHelper.Log(TrinityLogLevel.Debug, LogCategory.Movement, "Moved to:{0} dir: {1} Speed:{2:0.00} Dist:{3:0} ZDiff:{4:0} Nav:{5} LoS:{6}",
                        vMoveToTarget, GilesTrinity.GetHeadingToPoint(vMoveToTarget), MovementSpeed, vMyCurrentPosition.Distance2D(vMoveToTarget),
                        Math.Abs(vMyCurrentPosition.Z - vMoveToTarget.Z),
                        GilesTrinity.pf.IsNavigable(GilesTrinity.gp.WorldToGrid(vMoveToTarget.ToVector2())),
                        ZetaDia.Physics.Raycast(vMyCurrentPosition, vMoveToTarget, Zeta.Internals.SNO.NavCellFlags.AllowWalk));

            }
            else
            {
                if (GilesTrinity.Settings.Advanced.LogCategories.HasFlag(LogCategory.Movement))
                    DbHelper.Log(TrinityLogLevel.Debug, LogCategory.Movement, "Reached MoveTowards Destination {0} Current Speed: {0}", vMoveToTarget, MovementSpeed);
            }


        }

        private static void GoldInactiveLeaveGame()
        {
            // Exit the game and reload the profile
            timeLastRestartedGame = DateTime.Now;
            string sUseProfile = GilesTrinity.sFirstProfileSeen;
            DbHelper.Log(TrinityLogLevel.Normal, LogCategory.UserInformation, "Gold Inactivity timer tripped - Anti-stuck measures exiting current game.");
            // Load the first profile seen last run
            ProfileManager.Load(!string.IsNullOrEmpty(sUseProfile)
                                    ? sUseProfile
                                    : Zeta.CommonBot.ProfileManager.CurrentProfile.Path);
            Thread.Sleep(1000);
            GilesTrinity.GilesResetEverythingNewGame();
            ZetaDia.Service.Games.LeaveGame();
            // Wait for 10 second log out timer if not in town
            if (!ZetaDia.Me.IsInTown)
            {
                Thread.Sleep(12000);
            }
            return;
        }

        internal static int GetObstacleNavigationSize(GilesObstacle obstacle)
        {
            if (GilesTrinity.dictSNONavigationSize.ContainsKey(obstacle.ActorSNO))
                return GilesTrinity.dictSNONavigationSize[obstacle.ActorSNO];
            else
                return (int)Math.Ceiling(obstacle.Radius);
        }

        private static void GetShiftedPosition(ref Vector3 vMoveToTarget, ref Vector3 point, float radius = 15f)
        {
            try
            {
                if (ProfileManager.CurrentProfileBehavior.GetType() == typeof(TrinityExploreDungeon))
                {
                    return;
                }
            }
            catch { }

            float fDirectionToTarget = GilesTrinity.FindDirectionDegree(vMyCurrentPosition, vMoveToTarget);
            vMoveToTarget = MathEx.GetPointAt(vMyCurrentPosition, radius, MathEx.ToRadians(fDirectionToTarget - 65));
            if (!GilesTrinity.GilesCanRayCast(vMyCurrentPosition, vMoveToTarget, NavCellFlags.AllowWalk))
            {
                vMoveToTarget = MathEx.GetPointAt(vMyCurrentPosition, radius, MathEx.ToRadians(fDirectionToTarget + 65));
                if (!GilesTrinity.GilesCanRayCast(vMyCurrentPosition, vMoveToTarget, NavCellFlags.AllowWalk))
                {
                    vMoveToTarget = point;
                }
            }
            if (vMoveToTarget != point)
            {
                vShiftedPosition = vMoveToTarget;
                iShiftPositionFor = 900;
                lastShiftedPosition = DateTime.Now;
                DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.Movement, "Navigation handler position shift to: " + vMoveToTarget.ToString() + " (was " + point.ToString() + ")");
            }
        }

        private static Vector3 WarnAndLogLongPath(Vector3 vMoveToTarget)
        {
            // The below code is to help profile/routine makers avoid waypoints with a long distance between them.
            // Long-distances between waypoints is bad - it increases stucks, and forces the DB nav-server to be called.
            if (GilesTrinity.Settings.Advanced.LogStuckLocation)
            {
                if (vLastMoveTo == Vector3.Zero)
                    vLastMoveTo = vMoveToTarget;
                if (vMoveToTarget != vLastMoveTo)
                {
                    float fDistance = Vector3.Distance(vMoveToTarget, vLastMoveTo);
                    // Log if not in town, last waypoint wasn't FROM town, and the distance is >200 but <2000 (cos 2000+ probably means we changed map zones!)
                    if (!ZetaDia.Me.IsInTown && !bLastWaypointWasTown && fDistance >= 200 & fDistance <= 2000)
                    {
                        if (!hashDoneThisVector.Contains(vMoveToTarget))
                        {
                            // Log it
                            FileStream LogStream = File.Open(Path.Combine(FileManager.LoggingPath, "LongPaths - " + ZetaDia.Actors.Me.ActorClass.ToString() + ".log"), FileMode.Append, FileAccess.Write, FileShare.Read);
                            using (StreamWriter LogWriter = new StreamWriter(LogStream))
                            {
                                LogWriter.WriteLine(DateTime.Now.ToString() + ":");
                                LogWriter.WriteLine("Profile Name=" + ProfileManager.CurrentProfile.Name);
                                LogWriter.WriteLine("'From' Waypoint=" + vLastMoveTo.ToString() + ". 'To' Waypoint=" + vMoveToTarget.ToString() + ". Distance=" + fDistance.ToString());
                            }
                            LogStream.Close();
                            hashDoneThisVector.Add(vMoveToTarget);
                        }
                    }
                    vLastMoveTo = vMoveToTarget;
                    bLastWaypointWasTown = false;
                    if (ZetaDia.Me.IsInTown)
                        bLastWaypointWasTown = true;
                }
            }
            return vMoveToTarget;
        }

        private static void RecordLastProfile()
        {
            string currentProfileFileName = Path.GetFileName(ProfileManager.CurrentProfile.Path);
            if (!TrinityLoadOnce.UsedProfiles.Contains(currentProfileFileName))
            {
                TrinityLoadOnce.UsedProfiles.Add(currentProfileFileName);
            }


            string sThisProfile = Zeta.CommonBot.Settings.GlobalSettings.Instance.LastProfile;
            if (sThisProfile != GilesTrinity.sLastProfileSeen)
            {
                // See if we appear to have started a new game
                if (GilesTrinity.sFirstProfileSeen != "" && sThisProfile == GilesTrinity.sFirstProfileSeen)
                {
                    GilesTrinity.TotalProfileRecycles++;
                    if (GilesTrinity.TotalProfileRecycles > GilesTrinity.iTotalJoinGames && GilesTrinity.TotalProfileRecycles > GilesTrinity.TotalLeaveGames)
                    {
                        GilesTrinity.GilesResetEverythingNewGame();
                    }
                }
                GilesTrinity.listProfilesLoaded.Add(sThisProfile);
                GilesTrinity.sLastProfileSeen = sThisProfile;

                if (ProfileManager.CurrentProfile != null && ProfileManager.CurrentProfile.Name != null)
                {
                    GilesTrinity.SetWindowTitle(ProfileManager.CurrentProfile.Name);
                }
                if (GilesTrinity.sFirstProfileSeen == "")
                    GilesTrinity.sFirstProfileSeen = sThisProfile;
            }
        }

        internal static bool CanFullyPathTo(Vector3 point, float withinDistance = 10f)
        {
            if (ZetaDia.WorldInfo.IsGenerated)
            {
                IndexedList<Vector3> PathStack = GeneratePath(GilesTrinity.PlayerStatus.CurrentPosition, point);

                if (!PathStack.Any())
                    return false;

                for (int i = PathStack.Count - 1; i >= 0; i--)
                {
                    if (PathStack[i].Distance2D(point) <= withinDistance)
                    {
                        return true;
                    }
                }

                return false;
            }
            else
            {
                var nav = new Zeta.Navigation.DefaultNavigationProvider();
                return nav.CanPathWithinDistance(point, withinDistance);
            }
        }

        internal static IndexedList<Vector3> GeneratePath(Vector3 start, Vector3 destination)
        {
            Stack<Vector3> pathStack = new Stack<Vector3>();
            GilesTrinity.UpdateSearchGridProvider();

            PathFindResult pfr = GilesTrinity.pf.FindPath(
                GilesTrinity.gp.WorldToGrid(start.ToVector2()),
                GilesTrinity.gp.WorldToGrid(destination.ToVector2()),
                true, 50, true
                );

            DbHelper.Log(TrinityLogLevel.Normal, LogCategory.Movement, "Generated path with {0} points", pfr.PointsReversed.Count());

            if (pfr.Error)
            {
                DbHelper.Log(TrinityLogLevel.Normal, LogCategory.Movement, "Error in generating path: {0}", pfr.ErrorMessage);
                return new IndexedList<Vector3>(pathStack, false);
            }

            if (pfr.IsPartialPath)
            {
                DbHelper.Log(TrinityLogLevel.Normal, LogCategory.Movement, "Partial Path Generated!", true);
            }

            pathStack.Clear();

            foreach (Point p in pfr.PointsReversed)
            {
                Vector3 v3 = GilesTrinity.gp.GridToWorld(p).ToVector3();
                pathStack.Push(v3);
            }
            return new IndexedList<Vector3>(pathStack, false);
        }

        private static IndexedList<Vector3> PathStack = new IndexedList<Vector3>();

        private static DateTime lastGeneratedPath = DateTime.MinValue;
        internal static MoveResult NavigateTo(Vector3 moveTarget, string destinationName = "")
        {
            bool newPath = false;

            if (!PathStack.Any() || DateTime.Now.Subtract(lastGeneratedPath).TotalMilliseconds > 5000)
            {
                PathStack = PlayerMover.GeneratePath(ZetaDia.Me.Position, moveTarget);

                if (!PathStack.Any())
                {
                    Navigator.PlayerMover.MoveTowards(moveTarget);
                    return MoveResult.PathGenerationFailed;
                }
                lastGeneratedPath = DateTime.Now;
                newPath = true;
            }

            if (PathStack.Any() && PathStack.Count <= 2 && moveTarget.Distance2D(PathStack[PathStack.Count - 1]) > 5f && newPath)
            {
                Navigator.PlayerMover.MoveTowards(moveTarget);
                return MoveResult.PathGenerationFailed;
            }

            if (PathStack.Any())
            {

                if (PathStack.Current.Distance2D(ZetaDia.Me.Position) <= 5f)
                {
                    PathStack.Next();
                    PathStack.RemoveAt(0);
                }
            }

            if (PathStack.Any())
            {
                Navigator.PlayerMover.MoveTowards(PathStack.Current);
                return MoveResult.Moved;
            }

            return MoveResult.ReachedDestination;
        }

        private static DateTime lastRecordedSkipAheadCache = DateTime.MinValue;
        internal static void RecordSkipAheadCachePoint()
        {
            if (DateTime.Now.Subtract(lastRecordedSkipAheadCache).TotalMilliseconds < 100)
                return;

            lastRecordedSkipAheadCache = DateTime.Now;

            if (!GilesTrinity.hashSkipAheadAreaCache.Any(p => p.Location.Distance2D(ZetaDia.Me.Position) <= 5f))
            {
                GilesTrinity.hashSkipAheadAreaCache.Add(new GilesObstacle() { Location = ZetaDia.Me.Position, Radius = 20f });
            }
        }


    }
}
