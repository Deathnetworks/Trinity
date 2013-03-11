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
using Zeta.CommonBot.Profile;

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
            ZetaDia.Me.UsePower(SNOPower.Walk, GilesTrinity.PlayerStatus.CurrentPosition, GilesTrinity.CurrentWorldDynamicId, -1);
        }

        // Anti-stuck variables
        internal static Vector3 LastMoveToTarget = Vector3.Zero;
        internal static int iTimesReachedStuckPoint = 0;
        internal static int iTotalAntiStuckAttempts = 1;
        internal static Vector3 vSafeMovementLocation = Vector3.Zero;
        internal static DateTime TimeLastRecordedPosition = DateTime.Today;
        internal static Vector3 vOldPosition = Vector3.Zero;
        internal static DateTime LastGeneratedStuckPosition = DateTime.Today;
        internal static int iTimesReachedMaxUnstucks = 0;
        internal static DateTime _lastCancelledUnstucker = DateTime.Today;
        internal static DateTime _lastRecordedAnyStuck = DateTime.Today;
        internal static int iCancelUnstuckerForSeconds = 60;
        internal static DateTime timeLastRestartedGame = DateTime.Today;
        internal static bool UnStuckCheckerLastResult = false;
        internal static DateTime TimeLastUsedPlayerMover = DateTime.MinValue;

        internal static Vector3 LastTempestRushPosition = Vector3.Zero;

        private static int WizardTeleportCount = 0;

        // Store player current position
        public static Vector3 vMyCurrentPosition = Vector3.Zero;

        //For Tempest Rush Monks
        private static bool CanChannelTempestRush = false;

        /// <summary>
        /// Check if we are stuck or not by simply checking for position changing max once every 3 seconds
        /// </summary>
        /// <param name="vMyCurrentPosition"></param>
        /// <param name="checkDuration"></param>
        /// <returns></returns>
        public static bool UnstuckChecker(Vector3 vMyCurrentPosition, int checkDuration = 5000)
        {
            // set checkDuration to 30 sec while in town or vendoring, just to avoid annoyances
            if (ZetaDia.Me.IsInTown || GilesTrinity.ForceVendorRunASAP || Zeta.CommonBot.Logic.BrainBehavior.IsVendoring)
            {
                checkDuration = 15000;
            }

            // Keep checking distance changes every 3 seconds
            if (DateTime.Now.Subtract(TimeLastRecordedPosition).TotalMilliseconds >= checkDuration)
            {
                if (ZetaDia.Me.IsInTown && (UIElements.VendorWindow.IsVisible || UIElements.SalvageWindow.IsVisible))
                {
                    UnStuckCheckerLastResult = false;
                    return UnStuckCheckerLastResult;
                }

                if (checkDuration >= 5000)
                {
                    TimeLastRecordedPosition = DateTime.Now;
                }

                ProfileBehavior c = null;

                try
                {
                    if (ProfileManager.CurrentProfileBehavior != null)
                        c = ProfileManager.CurrentProfileBehavior;
                }
                catch { }

                if (c != null && c.GetType() == typeof(WaitTimerTag))
                {
                    vOldPosition = Vector3.Zero;
                    GoldInactivity.ResetCheckGold();
                    UnStuckCheckerLastResult = false;
                    return UnStuckCheckerLastResult;
                }

                Zeta.Internals.UIElement vendorWindow = Zeta.Internals.UIElements.VendorWindow;

                // We're not stuck if we're doing stuff!
                if (ZetaDia.Me.IsInConversation || ZetaDia.IsPlayingCutscene || ZetaDia.IsLoadingWorld || (vendorWindow.IsValid && vendorWindow.IsVisible))
                {
                    vOldPosition = Vector3.Zero;
                    GoldInactivity.ResetCheckGold();
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
                    GoldInactivity.ResetCheckGold();
                    UnStuckCheckerLastResult = false;
                    return UnStuckCheckerLastResult;
                }

                if (vOldPosition != Vector3.Zero && vOldPosition.Distance(vMyCurrentPosition) <= 4f)
                {
                    UnStuckCheckerLastResult = true;
                    return UnStuckCheckerLastResult;
                }

                if (checkDuration >= 5000)
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
            return UnstuckHandler(vMyCurrentPosition, LastMoveToTarget);
        }
        // Actually deal with a stuck - find an unstuck point etc.
        public static Vector3 UnstuckHandler(Vector3 vMyCurrentPosition, Vector3 vOriginalDestination)
        {
            // Update the last time we generated a path
            LastGeneratedStuckPosition = DateTime.Now;
            Navigator.Clear();

            // If we got stuck on a 2nd/3rd/4th "chained" anti-stuck route, then return the old move to target to keep movement of some kind going
            if (iTimesReachedStuckPoint > 0)
            {
                vSafeMovementLocation = Vector3.Zero;

                // Reset the path and allow a whole "New" unstuck generation next cycle
                iTimesReachedStuckPoint = 0;
                // And cancel unstucking for 9 seconds so DB can try to navigate
                iCancelUnstuckerForSeconds = (9 * iTotalAntiStuckAttempts);
                if (iCancelUnstuckerForSeconds < 20)
                    iCancelUnstuckerForSeconds = 20;
                _lastCancelledUnstucker = DateTime.Now;
                DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.UserInformation, "Clearing old route and trying new path find to: " + LastMoveToTarget.ToString());
                Navigator.MoveTo(LastMoveToTarget, "original destination", false);
                return vSafeMovementLocation;
            }
            // Only try an unstuck 10 times maximum in XXX period of time
            if (Vector3.Distance(vOriginalDestination, vMyCurrentPosition) >= 700f)
            {
                DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.UserInformation, "You are " + Vector3.Distance(vOriginalDestination, vMyCurrentPosition).ToString() + " distance away from your destination.");
                DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.UserInformation, "This is too far for the unstucker, and is likely a sign of ending up in the wrong map zone.");
                iTotalAntiStuckAttempts = 20;
            }
            // intell
            if (iTotalAntiStuckAttempts <= 15)
            {
                DbHelper.Log(TrinityLogLevel.Normal, LogCategory.UserInformation, "Your bot got stuck! Trying to unstuck (attempt #{0} of 15 attempts) {1} {2} {3} {4}",
                    iTotalAntiStuckAttempts.ToString(),
                    "Act=\"" + ZetaDia.CurrentAct + "\"",
                    "questId=\"" + ZetaDia.CurrentQuest.QuestSNO + "\"",
                    "stepId=\"" + ZetaDia.CurrentQuest.StepId + "\"",
                    "worldId=\"" + ZetaDia.CurrentWorldId + "\""
                );

                // check failed minimap markers
                MiniMapMarker.UpdateFailedMarkers();

                DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.UserInformation, "(destination=" + vOriginalDestination.ToString() + ", which is " + Vector3.Distance(vOriginalDestination, vMyCurrentPosition).ToString() + " distance away)");

                vSafeMovementLocation = NavHelper.FindSafeZone(true, iTotalAntiStuckAttempts, vMyCurrentPosition);

                // Temporarily log stuff
                if (iTotalAntiStuckAttempts == 1 && GilesTrinity.Settings.Advanced.LogStuckLocation)
                {
                    FileStream LogStream = File.Open(Path.Combine(FileManager.LoggingPath, "Stucks - " + GilesTrinity.PlayerStatus.ActorClass.ToString() + ".log"), FileMode.Append, FileAccess.Write, FileShare.Read);
                    using (StreamWriter LogWriter = new StreamWriter(LogStream))
                    {
                        LogWriter.WriteLine(DateTime.Now.ToString() + ": Original Destination=" + LastMoveToTarget.ToString() + ". Current player position when stuck=" + vMyCurrentPosition.ToString());
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
            LastGeneratedStuckPosition = DateTime.Today;
            // int iSafetyLoops = 0;
            if (iTimesReachedMaxUnstucks == 1)
            {
                Navigator.Clear();
                DbHelper.Log(TrinityLogLevel.Normal, LogCategory.Movement, "Anti-stuck measures now attempting to kickstart DB's path-finder into action.");
                Navigator.MoveTo(vOriginalDestination, "original destination", false);
                iCancelUnstuckerForSeconds = 40;
                _lastCancelledUnstucker = DateTime.Now;
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
                ZetaDia.Service.Party.LeaveGame();
                // Wait for 10 second log out timer if not in town
                if (!ZetaDia.Me.IsInTown)
                {
                    Thread.Sleep(15000);
                }
            }
            else
            {
                DbHelper.Log(TrinityLogLevel.Normal, LogCategory.UserInformation, "Unstucking measures failed. Now stopping Trinity unstucker for 12 minutes to inactivity timers to kick in or DB to auto-fix.");
                iCancelUnstuckerForSeconds = 720;
                _lastCancelledUnstucker = DateTime.Now;
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
        private static int MaxSpeedSensors = 5;

        public static double GetMovementSpeed()
        {
            // If we just used a spell, we "moved"
            if (DateTime.Now.Subtract(GilesTrinity.lastGlobalCooldownUse).TotalMilliseconds <= 1000)
                return 1d;

            if (DateTime.Now.Subtract(GilesTrinity.lastHadUnitInSights).TotalMilliseconds <= 1000)
                return 1d;

            if (DateTime.Now.Subtract(GilesTrinity.lastHadEliteUnitInSights).TotalMilliseconds <= 1000)
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
            if (SpeedSensors.Any(s => s.WorldID != GilesTrinity.CurrentWorldDynamicId))
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

            TimeLastUsedPlayerMover = DateTime.Now;
            vMyCurrentPosition = GilesTrinity.PlayerStatus.CurrentPosition;
            LastMoveToTarget = vMoveToTarget;

            // record speed once per second
            if (DateTime.Now.Subtract(lastRecordedPosition).TotalMilliseconds >= 1000)
            {
                // Record our current location and time
                if (!SpeedSensors.Any())
                {
                    SpeedSensors.Add(new SpeedSensor()
                    {
                        Location = vMyCurrentPosition,
                        TimeSinceLastMove = new TimeSpan(0),
                        Distance = 0f,
                        WorldID = GilesTrinity.CurrentWorldDynamicId
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
                         WorldID = GilesTrinity.CurrentWorldDynamicId
                     });
                }

                lastRecordedPosition = DateTime.Now;
            }
            // Set the public variable
            MovementSpeed = GetMovementSpeed();

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

            // Do unstuckery things
            if (GilesTrinity.Settings.Advanced.UnstuckerEnabled)
            {

                // See if we can reset the 10-limit unstuck counter, if >120 seconds since we last generated an unstuck location
                // this is used if we're NOT stuck...
                if (iTotalAntiStuckAttempts > 1 && DateTime.Now.Subtract(LastGeneratedStuckPosition).TotalSeconds >= 120)
                {
                    iTotalAntiStuckAttempts = 1;
                    iTimesReachedStuckPoint = 0;
                    vSafeMovementLocation = Vector3.Zero;
                    NavHelper.UsedStuckSpots = new List<GridPoint>();
                    DbHelper.Log(TrinityLogLevel.Normal, LogCategory.Movement, "Resetting unstuck timers", true);
                }

                // See if we need to, and can, generate unstuck actions
                // check if we're stuck
                bool isStuck = UnstuckChecker(vMyCurrentPosition);
                if (DateTime.Now.Subtract(_lastCancelledUnstucker).TotalSeconds > iCancelUnstuckerForSeconds && isStuck)
                {
                    // Record the time we last apparently couldn't move for a brief period of time
                    _lastRecordedAnyStuck = DateTime.Now;
                    // See if there's any stuck position to try and navigate to generated by random mover
                    vSafeMovementLocation = UnstuckHandler(vMyCurrentPosition, LastMoveToTarget);
                    DbHelper.Log(TrinityLogLevel.Normal, LogCategory.Movement, "SafeMovement Location set to {0}", vSafeMovementLocation);
                    if (vSafeMovementLocation == Vector3.Zero)
                        return;
                }
                // See if we can clear the total unstuckattempts if we haven't been stuck in over 6 minutes.
                if (DateTime.Now.Subtract(_lastRecordedAnyStuck).TotalSeconds >= 360)
                {
                    iTimesReachedMaxUnstucks = 0;
                }
                // Did we have a safe point already generated (eg from last loop through), if so use it as our current location instead
                if (vSafeMovementLocation != Vector3.Zero)
                {
                    // Set our current movement target to the safe point we generated last cycle
                    vMoveToTarget = vSafeMovementLocation;
                    DestinationDistance = vMyCurrentPosition.Distance2D(vMoveToTarget);
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
                        vSafeMovementLocation = NavHelper.FindSafeZone(true, iTotalAntiStuckAttempts, vMyCurrentPosition);
                        vMoveToTarget = vSafeMovementLocation;
                    }
                    else
                    {
                        if (GilesTrinity.Settings.Advanced.LogCategories.HasFlag(LogCategory.Movement))
                            DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.Movement, "Clearing old route and trying new path find to: " + LastMoveToTarget.ToString());
                        // Reset the path and allow a whole "New" unstuck generation next cycle
                        iTimesReachedStuckPoint = 0;
                        // And cancel unstucking for 9 seconds so DB can try to navigate
                        iCancelUnstuckerForSeconds = (9 * iTotalAntiStuckAttempts);
                        if (iCancelUnstuckerForSeconds < 20)
                            iCancelUnstuckerForSeconds = 20;
                        _lastCancelledUnstucker = DateTime.Now;

                        Navigator.Clear();
                        Navigator.MoveTo(LastMoveToTarget, "original destination", false);

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
            foreach (GilesObstacle obstacle in GilesTrinity.hashNavigationObstacleCache.Where(o => vMoveToTarget.Distance2D(o.Location) <= o.Radius))
            {
                if (vShiftedPosition == Vector3.Zero)
                {
                    // Make sure we only shift max once every 6 seconds
                    if (DateTime.Now.Subtract(lastShiftedPosition).TotalMilliseconds >= 6000)
                    {
                        DbHelper.Log(TrinityLogLevel.Debug, LogCategory.Movement, "Shifting position for Navigation Obstacle {0} {1} at {2}", obstacle.ActorSNO, obstacle.Name, obstacle.Location);
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

            

            // don't use special movement within 10 seconds of being stuck
            bool cancelSpecialMovementAfterStuck = DateTime.Now.Subtract(LastGeneratedStuckPosition).TotalMilliseconds > 10000;

            // See if we can use abilities like leap etc. for movement out of combat, but not in town
            if (GilesTrinity.Settings.Combat.Misc.AllowOOCMovement && !GilesTrinity.PlayerStatus.IsInTown && !GilesTrinity.bDontMoveMeIAmDoingShit && cancelSpecialMovementAfterStuck)
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
                    ZetaDia.Me.UsePower(SNOPower.Barbarian_Leap, vThisTarget, GilesTrinity.CurrentWorldDynamicId, -1);
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
                    ZetaDia.Me.UsePower(SNOPower.Barbarian_FuriousCharge, vThisTarget, GilesTrinity.CurrentWorldDynamicId, -1);
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
                    ZetaDia.Me.UsePower(SNOPower.DemonHunter_Vault, vThisTarget, GilesTrinity.CurrentWorldDynamicId, -1);
                    GilesTrinity.dictAbilityLastUse[SNOPower.DemonHunter_Vault] = DateTime.Now;
                    if (GilesTrinity.Settings.Advanced.LogCategories.HasFlag(LogCategory.Movement))
                        DbHelper.Log(TrinityLogLevel.Debug, LogCategory.Movement, "Using Vault for OOC movement, distance={0}", DestinationDistance);
                    return;
                }
                // Tempest rush for a monk
                if (GilesTrinity.Hotbar.Contains(SNOPower.Monk_TempestRush) &&
                    (GilesTrinity.Settings.Combat.Monk.TROption == TempestRushOption.MovementOnly || GilesTrinity.Settings.Combat.Monk.TROption == TempestRushOption.Always ||
                    (GilesTrinity.Settings.Combat.Monk.TROption == TempestRushOption.TrashOnly && !TargetUtil.AnyElitesInRange(40f))))
                {
                    Vector3 vTargetAimPoint = vMoveToTarget;
                    //vTargetAimPoint = MathEx.CalculatePointFrom(vMoveToTarget, vMyCurrentPosition, aimPointDistance);
                    //vTargetAimPoint = MathEx.CalculatePointFrom(vMyCurrentPosition, vMoveToTarget, aimPointDistance);

                    //bool canRayCastTarget = GilesTrinity.NavHelper.CanRayCast(vMyCurrentPosition, vTargetAimPoint);
                    bool canRayCastTarget = true;

                    vTargetAimPoint = TargetUtil.FindTempestRushTarget();

                    if (!CanChannelTempestRush &&
                        ((GilesTrinity.PlayerStatus.PrimaryResource >= GilesTrinity.Settings.Combat.Monk.TR_MinSpirit &&
                        DestinationDistance >= GilesTrinity.Settings.Combat.Monk.TR_MinDist) ||
                         DateTime.Now.Subtract(GilesTrinity.dictAbilityLastUse[SNOPower.Monk_TempestRush]).TotalMilliseconds <= 150) &&
                        canRayCastTarget && PowerManager.CanCast(SNOPower.Monk_TempestRush))
                    {
                        CanChannelTempestRush = true;
                    }
                    else if ((CanChannelTempestRush && (GilesTrinity.PlayerStatus.PrimaryResource < 10f)) || !canRayCastTarget)
                    {
                        CanChannelTempestRush = false;
                    }

                    double lastUse = DateTime.Now.Subtract(GilesTrinity.dictAbilityLastUse[SNOPower.Monk_TempestRush]).TotalMilliseconds;

                    if (CanChannelTempestRush)
                    {
                        if (GilesTrinity.GilesUseTimer(SNOPower.Monk_TempestRush))
                        {
                            LastTempestRushPosition = vTargetAimPoint;

                            ZetaDia.Me.UsePower(SNOPower.Monk_TempestRush, vTargetAimPoint, GilesTrinity.CurrentWorldDynamicId, -1);
                            GilesTrinity.dictAbilityLastUse[SNOPower.Monk_TempestRush] = DateTime.Now;
                            GilesTrinity.LastPowerUsed = SNOPower.Monk_TempestRush;

                            // simulate movement speed of 30
                            SpeedSensor lastSensor = SpeedSensors.OrderByDescending(s => s.Timestamp).FirstOrDefault();
                            SpeedSensors.Add(new SpeedSensor()
                            {
                                Location = vMyCurrentPosition,
                                TimeSinceLastMove = new TimeSpan(0, 0, 0, 0, 1000),
                                Distance = 5f,
                                WorldID = GilesTrinity.CurrentWorldDynamicId
                            });

                            if (GilesTrinity.Settings.Advanced.LogCategories.HasFlag(LogCategory.Movement))
                                DbHelper.Log(TrinityLogLevel.Debug, LogCategory.Movement, "Using Tempest Rush for OOC movement, distance={0:0} spirit={1:0} cd={2} lastUse={3:0} V3={4} vAim={5}",
                                    DestinationDistance, GilesTrinity.PlayerStatus.PrimaryResource, PowerManager.CanCast(SNOPower.Monk_TempestRush), lastUse, vMoveToTarget, vTargetAimPoint);
                            return;
                        }
                        else
                            return;
                    }
                    else
                    {
                        if (GilesTrinity.Settings.Advanced.LogCategories.HasFlag(LogCategory.Movement))
                            DbHelper.Log(TrinityLogLevel.Debug, LogCategory.Movement,
                            "Tempest rush failed!: {0:00.0} / {1} distance: {2:00.0} / {3} Raycast: {4} MS: {5:0.0} lastUse={6:0}",
                            GilesTrinity.PlayerStatus.PrimaryResource,
                            GilesTrinity.Settings.Combat.Monk.TR_MinSpirit,
                            DestinationDistance,
                            GilesTrinity.Settings.Combat.Monk.TR_MinDist,
                            canRayCastTarget,
                            GetMovementSpeed(),
                            lastUse);

                        GilesTrinity.MaintainTempestRush = false;
                    }

                    // Always set this from PlayerMover
                    GilesTrinity.LastTempestRushLocation = vTargetAimPoint;

                }

                bool hasWormHole = HotbarSkills.AssignedSkills.Any(s => s.Power == SNOPower.Wizard_Teleport && s.RuneIndex == 4);

                // Teleport for a wizard (need to be able to check skill rune in DB for a 3-4 teleport spam in a row)
                if (GilesTrinity.Hotbar.Contains(SNOPower.Wizard_Teleport) &&
                    ((PowerManager.CanCast(SNOPower.Wizard_Teleport) && DateTime.Now.Subtract(GilesTrinity.dictAbilityLastUse[SNOPower.Wizard_Teleport]).TotalMilliseconds >= GilesTrinity.dictAbilityRepeatDelay[SNOPower.Wizard_Teleport]) ||
                    (hasWormHole && WizardTeleportCount < 3 && DateTime.Now.Subtract(GilesTrinity.dictAbilityLastUse[SNOPower.Wizard_Teleport]).TotalMilliseconds >= 250)) &&
                    DestinationDistance >= 10f && !ShrinesInArea(vMoveToTarget))
                {
                    // Reset teleport count if we've already hit the max
                    if (WizardTeleportCount >= 3)
                        WizardTeleportCount = 0;

                    // increment the teleport count for wormhole rune
                    WizardTeleportCount++;

                    Vector3 vThisTarget = vMoveToTarget;
                    if (DestinationDistance > 35f)
                        vThisTarget = MathEx.CalculatePointFrom(vMoveToTarget, vMyCurrentPosition, 35f);
                    ZetaDia.Me.UsePower(SNOPower.Wizard_Teleport, vThisTarget, GilesTrinity.CurrentWorldDynamicId, -1);
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
                    ZetaDia.Me.UsePower(SNOPower.Wizard_Archon_Teleport, vThisTarget, GilesTrinity.CurrentWorldDynamicId, -1);
                    GilesTrinity.dictAbilityLastUse[SNOPower.Wizard_Archon_Teleport] = DateTime.Now;
                    if (GilesTrinity.Settings.Advanced.LogCategories.HasFlag(LogCategory.Movement))
                        DbHelper.Log(TrinityLogLevel.Debug, LogCategory.Movement, "Using Archon Teleport for OOC movement, distance={0}", DestinationDistance);
                    return;
                }
            }

            if (vMyCurrentPosition.Distance2D(vMoveToTarget) > 1f)
            {
                // Default movement
                ZetaDia.Me.UsePower(SNOPower.Walk, vMoveToTarget, GilesTrinity.CurrentWorldDynamicId, -1);

                if (GilesTrinity.Settings.Advanced.LogCategories.HasFlag(LogCategory.Movement))
                    DbHelper.Log(TrinityLogLevel.Debug, LogCategory.Movement, "Moved to:{0} dir: {1} Speed:{2:0.00} Dist:{3:0} ZDiff:{4:0} Nav:{5} LoS:{6}",
                        vMoveToTarget, MathUtil.GetHeadingToPoint(vMoveToTarget), MovementSpeed, vMyCurrentPosition.Distance2D(vMoveToTarget),
                        Math.Abs(vMyCurrentPosition.Z - vMoveToTarget.Z),
                        GilesTrinity.gp.CanStandAt(GilesTrinity.gp.WorldToGrid(vMoveToTarget.ToVector2())),
                        !Navigator.Raycast(vMyCurrentPosition, vMoveToTarget)
                        );

            }
            else
            {
                if (GilesTrinity.Settings.Advanced.LogCategories.HasFlag(LogCategory.Movement))
                    DbHelper.Log(TrinityLogLevel.Debug, LogCategory.Movement, "Reached MoveTowards Destination {0} Current Speed: {1:0.0}", vMoveToTarget, MovementSpeed);
            }


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
            double moveDirection = MathUtil.FindDirectionRadian(vMyCurrentPosition, vMoveToTarget);
            
            vMoveToTarget = MathEx.GetPointAt(vMyCurrentPosition, radius + 30f, (float)moveDirection);
            //if (!GilesTrinity.NavHelper.CanRayCast(vMyCurrentPosition, vMoveToTarget))
            //{
            //    vMoveToTarget = MathEx.GetPointAt(vMyCurrentPosition, radius, MathEx.ToRadians(fDirectionToTarget + 65));
            //    if (!GilesTrinity.NavHelper.CanRayCast(vMyCurrentPosition, vMoveToTarget))
            //    {
            //        vMoveToTarget = point;
            //    }
            //}
            if (vMoveToTarget != point)
            {
                vShiftedPosition = vMoveToTarget;
                iShiftPositionFor = 3000;
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
                    if (!GilesTrinity.PlayerStatus.IsInTown && !bLastWaypointWasTown && fDistance >= 200 & fDistance <= 2000)
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
                    if (GilesTrinity.PlayerStatus.IsInTown)
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

        private static GilesObject CurrentTarget { get { return GilesTrinity.CurrentTarget; } }

        internal static MoveResult NavigateTo(Vector3 moveTarget, string destinationName = "")
        {
            using (new PerformanceLogger("NavigateTo"))
            {
                Vector3 MyPos = GilesTrinity.PlayerStatus.CurrentPosition;

                PositionCache.AddPosition();

                float distanceToTarget = moveTarget.Distance2D(GilesTrinity.PlayerStatus.CurrentPosition);

                bool MoveTargetIsInLoS = distanceToTarget <= 90f && !Navigator.Raycast(MyPos, moveTarget);

                if (distanceToTarget <= 5f || MoveTargetIsInLoS)
                {
                    if (distanceToTarget <= 5f)
                        DbHelper.Log(TrinityLogLevel.Debug, LogCategory.Navigator, "Destination within 5f, using MoveTowards", true);
                    else
                        DbHelper.Log(TrinityLogLevel.Debug, LogCategory.Navigator, "Destination within LoS, using MoveTowards", true);
                    Navigator.PlayerMover.MoveTowards(moveTarget);
                    return MoveResult.Moved;
                }

                return Navigator.MoveTo(moveTarget, destinationName, true);
            }
        }

        private static DateTime lastRecordedSkipAheadCache = DateTime.MinValue;
        internal static void RecordSkipAheadCachePoint()
        {
            if (DateTime.Now.Subtract(lastRecordedSkipAheadCache).TotalMilliseconds < 100)
                return;

            lastRecordedSkipAheadCache = DateTime.Now;

            if (!GilesTrinity.hashSkipAheadAreaCache.Any(p => p.Location.Distance2D(GilesTrinity.PlayerStatus.CurrentPosition) <= 5f))
            {
                GilesTrinity.hashSkipAheadAreaCache.Add(new GilesObstacle() { Location = GilesTrinity.PlayerStatus.CurrentPosition, Radius = 20f });
            }
        }


    }
}
