using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Trinity.Combat.Abilities;
using Trinity.Config.Combat;
using Trinity.Technicals;
using Zeta.Bot;
using Zeta.Bot.Navigation;
using Zeta.Bot.Profile;
using Zeta.Bot.Profile.Common;
using Zeta.Common;
using Zeta.Game;
using Zeta.Game.Internals;
using Zeta.Game.Internals.Actors;
using Logger = Trinity.Technicals.Logger;

namespace Trinity.DbProvider
{
    // Player Mover Class
    public class PlayerMover : IPlayerMover
    {
        private static readonly HashSet<int> BasicMovementOnlyIDs = new HashSet<int> { 138989, 176074, 176076, 176077, 176536, 260330 };
        // 138989 = health pool, 176074 = protection, 176076 = fortune, 176077 = frenzied, 176536 = portal in leorics, 260330 = cooldown shrine
        // Exp shrines = ???? Other shrines ????

        private static bool ShrinesInArea(Vector3 targetpos)
        {
            return Trinity.ObjectCache.Any(o => BasicMovementOnlyIDs.Contains(o.ActorSNO) && Vector3.Distance(o.Position, targetpos) <= 10f);
        }

        public void MoveStop()
        {
            ZetaDia.Me.UsePower(SNOPower.Walk, Trinity.Player.Position, Trinity.CurrentWorldDynamicId, -1);
        }

        // Anti-stuck variables
        internal static Vector3 LastMoveToTarget = Vector3.Zero;
        internal static int TimesReachedStuckPoint = 0;
        internal static int TotalAntiStuckAttempts = 1;
        internal static Vector3 vSafeMovementLocation = Vector3.Zero;
        internal static DateTime TimeLastRecordedPosition = DateTime.MinValue;
        internal static Vector3 LastPosition = Vector3.Zero;
        internal static DateTime LastGeneratedStuckPosition = DateTime.MinValue;
        internal static int TimesReachedMaxUnstucks = 0;
        internal static DateTime LastCancelledUnstucker = DateTime.MinValue;
        internal static DateTime LastRecordedAnyStuck = DateTime.MinValue;
        internal static int CancelUnstuckerForSeconds = 60;
        internal static DateTime LastRestartedGame = DateTime.MinValue;
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
        public static bool UnstuckChecker(Vector3 vMyCurrentPosition, int checkDuration = 3000)
        {
            // set checkDuration to 30 sec while in town or vendoring, just to avoid annoyances
            if (ZetaDia.IsInTown || Trinity.ForceVendorRunASAP || Zeta.Bot.Logic.BrainBehavior.IsVendoring)
            {
                checkDuration = 15000;
            }

            // Keep checking distance changes every 3 seconds
            if (DateTime.UtcNow.Subtract(TimeLastRecordedPosition).TotalMilliseconds >= checkDuration)
            {
                if (ZetaDia.IsInTown && (UIElements.VendorWindow.IsVisible || UIElements.SalvageWindow.IsVisible))
                {
                    UnStuckCheckerLastResult = false;
                    return UnStuckCheckerLastResult;
                }

                if (checkDuration >= 3000)
                {
                    TimeLastRecordedPosition = DateTime.UtcNow;
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
                    LastPosition = Vector3.Zero;
                    GoldInactivity.ResetCheckGold();
                    UnStuckCheckerLastResult = false;
                    return UnStuckCheckerLastResult;
                }

                Zeta.Game.Internals.UIElement vendorWindow = Zeta.Game.Internals.UIElements.VendorWindow;

                // We're not stuck if we're doing stuff!
                if (ZetaDia.Me.IsInConversation || ZetaDia.IsPlayingCutscene || ZetaDia.IsLoadingWorld || (vendorWindow.IsValid && vendorWindow.IsVisible))
                {
                    LastPosition = Vector3.Zero;
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
                    LastPosition = Vector3.Zero;
                    GoldInactivity.ResetCheckGold();
                    UnStuckCheckerLastResult = false;
                    return UnStuckCheckerLastResult;
                }

                if (LastPosition != Vector3.Zero && LastPosition.Distance(vMyCurrentPosition) <= 4f)
                {
                    UnStuckCheckerLastResult = true;
                    return UnStuckCheckerLastResult;
                }

                if (checkDuration >= 3000)
                {
                    LastPosition = vMyCurrentPosition;
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
            LastGeneratedStuckPosition = DateTime.UtcNow;
            Navigator.Clear();

            // If we got stuck on a 2nd/3rd/4th "chained" anti-stuck route, then return the old move to target to keep movement of some kind going
            if (TimesReachedStuckPoint > 0)
            {
                vSafeMovementLocation = Vector3.Zero;

                // Reset the path and allow a whole "New" unstuck generation next cycle
                TimesReachedStuckPoint = 0;
                // And cancel unstucking for 9 seconds so DB can try to navigate
                CancelUnstuckerForSeconds = (9 * TotalAntiStuckAttempts);
                if (CancelUnstuckerForSeconds < 20)
                    CancelUnstuckerForSeconds = 20;
                LastCancelledUnstucker = DateTime.UtcNow;
                Logger.Log(TrinityLogLevel.Verbose, LogCategory.UserInformation, "Clearing old route and trying new path find to: " + LastMoveToTarget.ToString());
                Navigator.MoveTo(LastMoveToTarget, "original destination", false);
                return vSafeMovementLocation;
            }
            // Only try an unstuck 10 times maximum in XXX period of time
            if (Vector3.Distance(vOriginalDestination, vMyCurrentPosition) >= V.F("Unstucker.MaxDistance"))
            {
                Logger.Log(TrinityLogLevel.Verbose, LogCategory.UserInformation, "You are " + Vector3.Distance(vOriginalDestination, vMyCurrentPosition).ToString() + " distance away from your destination.");
                Logger.Log(TrinityLogLevel.Verbose, LogCategory.UserInformation, "This is too far for the unstucker, and is likely a sign of ending up in the wrong map zone.");
                TotalAntiStuckAttempts = 20;
            }
            // intell
            if (TotalAntiStuckAttempts <= 10)
            {
                Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "Your bot got stuck! Trying to unstuck (attempt #{0} of 10 attempts) {1} {2} {3} {4}",
                    TotalAntiStuckAttempts.ToString(),
                    "Act=\"" + ZetaDia.CurrentAct + "\"",
                    "questId=\"" + ZetaDia.CurrentQuest.QuestSNO + "\"",
                    "stepId=\"" + ZetaDia.CurrentQuest.StepId + "\"",
                    "worldId=\"" + ZetaDia.CurrentWorldId + "\""
                );

                // check failed minimap markers
                MiniMapMarker.UpdateFailedMarkers();

                Logger.Log(TrinityLogLevel.Verbose, LogCategory.UserInformation, "(destination=" + vOriginalDestination.ToString() + ", which is " + Vector3.Distance(vOriginalDestination, vMyCurrentPosition).ToString() + " distance away)");

                vSafeMovementLocation = NavHelper.FindSafeZone(true, TotalAntiStuckAttempts, vMyCurrentPosition);

                // Temporarily log stuff
                if (TotalAntiStuckAttempts == 1 && Trinity.Settings.Advanced.LogStuckLocation)
                {
                    FileStream LogStream = File.Open(Path.Combine(FileManager.LoggingPath, "Stucks - " + Trinity.Player.ActorClass.ToString() + ".log"), FileMode.Append, FileAccess.Write, FileShare.Read);
                    using (StreamWriter LogWriter = new StreamWriter(LogStream))
                    {
                        LogWriter.WriteLine(DateTime.UtcNow.ToString() + ": Original Destination=" + LastMoveToTarget.ToString() + ". Current player position when stuck=" + vMyCurrentPosition.ToString());
                        LogWriter.WriteLine("Profile Name=" + ProfileManager.CurrentProfile.Name);
                    }
                    LogStream.Close();
                }
                // Now count up our stuck attempt generations
                TotalAntiStuckAttempts++;
                return vSafeMovementLocation;
            }

            TimesReachedMaxUnstucks++;
            TotalAntiStuckAttempts = 1;
            vSafeMovementLocation = Vector3.Zero;
            LastPosition = Vector3.Zero;
            TimesReachedStuckPoint = 0;
            TimeLastRecordedPosition = DateTime.MinValue;
            LastGeneratedStuckPosition = DateTime.MinValue;
            // int iSafetyLoops = 0;
            if (TimesReachedMaxUnstucks == 1)
            {
                Navigator.Clear();
                Logger.Log(TrinityLogLevel.Info, LogCategory.Movement, "Anti-stuck measures now attempting to kickstart DB's path-finder into action.");
                Navigator.MoveTo(vOriginalDestination, "original destination", false);
                CancelUnstuckerForSeconds = 40;
                LastCancelledUnstucker = DateTime.UtcNow;
                return vSafeMovementLocation;
            }
            if (TimesReachedMaxUnstucks == 2)
            {
                Logger.Log(TrinityLogLevel.Info, LogCategory.Movement, "Anti-stuck measures failed. Now attempting to reload current profile.");

                Navigator.Clear();

                ProfileManager.Load(Zeta.Bot.ProfileManager.CurrentProfile.Path);
                Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "Anti-stuck successfully reloaded current profile, DemonBuddy now navigating again.");
                return vSafeMovementLocation;

                // Didn't make it to town, so skip instantly to the exit game system
                //iTimesReachedMaxUnstucks = 3;
            }
            // Exit the game and reload the profile
            if (Trinity.Settings.Advanced.AllowRestartGame && DateTime.UtcNow.Subtract(LastRestartedGame).TotalMinutes >= 5)
            {
                LastRestartedGame = DateTime.UtcNow;
                string sUseProfile = Trinity.FirstProfile;
                Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "Anti-stuck measures exiting current game.");
                // Load the first profile seen last run
                ProfileManager.Load(!string.IsNullOrEmpty(sUseProfile)
                                        ? sUseProfile
                                        : Zeta.Bot.ProfileManager.CurrentProfile.Path);
                Thread.Sleep(1000);
                Trinity.ResetEverythingNewGame();
                ZetaDia.Service.Party.LeaveGame();
                // Wait for 10 second log out timer if not in town
                if (!ZetaDia.IsInTown)
                {
                    Thread.Sleep(15000);
                }
            }
            else
            {
                Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "Unstucking measures failed. Now stopping Trinity unstucker for 12 minutes to inactivity timers to kick in or DB to auto-fix.");
                CancelUnstuckerForSeconds = 720;
                LastCancelledUnstucker = DateTime.UtcNow;
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
        private static DateTime lastShiftedPosition = DateTime.MinValue;
        private static int iShiftPositionFor = 0;

        private static Vector3 lastMovementPosition = Vector3.Zero;
        private static DateTime lastRecordedPosition = DateTime.UtcNow;
        internal static double MovementSpeed { get; private set; }

        private static List<SpeedSensor> SpeedSensors = new List<SpeedSensor>();
        private static int MaxSpeedSensors = 5;

        public static double GetMovementSpeed()
        {
            // record speed once per second
            if (DateTime.UtcNow.Subtract(lastRecordedPosition).TotalMilliseconds >= 1000)
            {
                // Record our current location and time
                if (!SpeedSensors.Any())
                {
                    SpeedSensors.Add(new SpeedSensor()
                    {
                        Location = vMyCurrentPosition,
                        TimeSinceLastMove = new TimeSpan(0),
                        Distance = 0f,
                        WorldID = Trinity.CurrentWorldDynamicId
                    });
                }
                else
                {
                    SpeedSensor lastSensor = SpeedSensors.OrderByDescending(s => s.Timestamp).FirstOrDefault();
                    SpeedSensors.Add(new SpeedSensor()
                    {
                        Location = vMyCurrentPosition,
                        TimeSinceLastMove = new TimeSpan(DateTime.UtcNow.Subtract(lastSensor.TimeSinceLastMove).Ticks),
                        Distance = Vector3.Distance(vMyCurrentPosition, lastSensor.Location),
                        WorldID = Trinity.CurrentWorldDynamicId
                    });
                }

                lastRecordedPosition = DateTime.UtcNow;
            }

            // If we just used a spell, we "moved"
            if (DateTime.UtcNow.Subtract(Trinity.lastGlobalCooldownUse).TotalMilliseconds <= 1000)
                return 1d;

            if (DateTime.UtcNow.Subtract(Trinity.lastHadUnitInSights).TotalMilliseconds <= 1000)
                return 1d;

            if (DateTime.UtcNow.Subtract(Trinity.lastHadEliteUnitInSights).TotalMilliseconds <= 1000)
                return 1d;

            // Minimum of 2 records to calculate speed
            if (!SpeedSensors.Any() || SpeedSensors.Count <= 1)
                return 0d;

            // If we haven't "moved" in over a second, then we're standing still
            if (DateTime.UtcNow.Subtract(TimeLastUsedPlayerMover).TotalMilliseconds > 1000)
                return 0d;

            // Check if we have enough recorded positions, remove one if so
            while (SpeedSensors.Count > MaxSpeedSensors - 1)
            {
                // first sensors
                SpeedSensors.Remove(SpeedSensors.OrderBy(s => s.Timestamp).FirstOrDefault());
            }
            // Clean up the stack
            if (SpeedSensors.Any(s => s.WorldID != Trinity.CurrentWorldDynamicId))
            {
                SpeedSensors.Clear();
                return 1d;
            }

            double AverageRecordingTime = SpeedSensors.Average(s => s.TimeSinceLastMove.TotalHours); ;
            double averageMovementSpeed = SpeedSensors.Average(s => Vector3.Distance(s.Location, vMyCurrentPosition) * 1000000);

            return averageMovementSpeed / AverageRecordingTime;
        }

        /// <summary>
        /// Returns true if there's a blocking UIElement that we should NOT be moving!
        /// </summary>
        /// <returns></returns>
        public bool UISafetyCheck()
        {
            if (ElementIsVisible(UIElements.ConfirmationDialog))
                return true;
            if (ElementIsVisible(UIElements.ConfirmationDialogCancelButton))
                return true;
            if (ElementIsVisible(UIElements.ConfirmationDialogOkButton))
                return true;
            if (ElementIsVisible(UIElements.ReviveAtLastCheckpointButton))
                return true;

            return false;
        }

        private bool ElementIsVisible(UIElement element)
        {
            if (element == null)
                return false;
            if (!UIElement.IsValidElement(element.Hash))
                return false;
            if (!element.IsValid)
                return false;
            if (!element.IsVisible)
                return false;

            return true;
        }

        public void MoveTowards(Vector3 vMoveToTarget)
        {
            if (!ZetaDia.IsInGame || !ZetaDia.Me.IsValid || ZetaDia.Me.IsDead || ZetaDia.IsLoadingWorld)
            {
                return;
            }

            if (UISafetyCheck())
            {
                return;
            }

            TimeLastUsedPlayerMover = DateTime.UtcNow;
            vMyCurrentPosition = Trinity.Player.Position;
            LastMoveToTarget = vMoveToTarget;

            // Set the public variable
            MovementSpeed = GetMovementSpeed();

            vMoveToTarget = WarnAndLogLongPath(vMoveToTarget);

            // Store player current position

            // Store distance to current moveto target
            float DestinationDistance;
            DestinationDistance = vMyCurrentPosition.Distance2D(vMoveToTarget);

            // Do unstuckery things
            if (Trinity.Settings.Advanced.UnstuckerEnabled)
            {
                // See if we can reset the 10-limit unstuck counter, if >120 seconds since we last generated an unstuck location
                // this is used if we're NOT stuck...
                if (TotalAntiStuckAttempts > 1 && DateTime.UtcNow.Subtract(LastGeneratedStuckPosition).TotalSeconds >= 120)
                {
                    TotalAntiStuckAttempts = 1;
                    TimesReachedStuckPoint = 0;
                    vSafeMovementLocation = Vector3.Zero;
                    NavHelper.UsedStuckSpots = new List<GridPoint>();
                    Logger.Log(TrinityLogLevel.Info, LogCategory.Movement, "Resetting unstuck timers", true);
                }

                // See if we need to, and can, generate unstuck actions
                // check if we're stuck
                bool isStuck = UnstuckChecker(vMyCurrentPosition);
                if (DateTime.UtcNow.Subtract(LastCancelledUnstucker).TotalSeconds > CancelUnstuckerForSeconds && isStuck)
                {
                    // Record the time we last apparently couldn't move for a brief period of time
                    LastRecordedAnyStuck = DateTime.UtcNow;
                    // See if there's any stuck position to try and navigate to generated by random mover
                    vSafeMovementLocation = UnstuckHandler(vMyCurrentPosition, LastMoveToTarget);
                    Logger.Log(TrinityLogLevel.Info, LogCategory.Movement, "SafeMovement Location set to {0}", vSafeMovementLocation);
                    if (vSafeMovementLocation == Vector3.Zero)
                        return;
                }
                // See if we can clear the total unstuckattempts if we haven't been stuck in over 6 minutes.
                if (DateTime.UtcNow.Subtract(LastRecordedAnyStuck).TotalSeconds >= 360)
                {
                    TimesReachedMaxUnstucks = 0;
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
                    TimesReachedStuckPoint++;
                    // Do we want to immediately generate a 2nd waypoint to "chain" anti-stucks in an ever-increasing path-length?
                    if (TimesReachedStuckPoint <= TotalAntiStuckAttempts)
                    {
                        vSafeMovementLocation = NavHelper.FindSafeZone(true, TotalAntiStuckAttempts, vMyCurrentPosition);
                        vMoveToTarget = vSafeMovementLocation;
                    }
                    else
                    {
                        if (Trinity.Settings.Advanced.LogCategories.HasFlag(LogCategory.Movement))
                            Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "Clearing old route and trying new path find to: " + LastMoveToTarget.ToString());
                        // Reset the path and allow a whole "New" unstuck generation next cycle
                        TimesReachedStuckPoint = 0;
                        // And cancel unstucking for 9 seconds so DB can try to navigate
                        CancelUnstuckerForSeconds = (9 * TotalAntiStuckAttempts);
                        if (CancelUnstuckerForSeconds < 20)
                            CancelUnstuckerForSeconds = 20;
                        LastCancelledUnstucker = DateTime.UtcNow;

                        Navigator.Clear();
                        Navigator.MoveTo(LastMoveToTarget, "original destination", false);

                        return;
                    }
                }
            }

            // See if there's an obstacle in our way, if so try to navigate around it
            Vector3 point = vMoveToTarget;
            foreach (CacheObstacleObject obstacle in Trinity.NavigationObstacleCache.Where(o => vMoveToTarget.Distance2D(o.Location) <= o.Radius))
            {
                if (vShiftedPosition == Vector3.Zero)
                {
                    // Make sure we only shift max once every 6 seconds
                    if (DateTime.UtcNow.Subtract(lastShiftedPosition).TotalMilliseconds >= 6000)
                    {
                        Logger.Log(TrinityLogLevel.Debug, LogCategory.Movement, "Shifting position for Navigation Obstacle {0} {1} at {2}", obstacle.ActorSNO, obstacle.Name, obstacle.Location);
                        GetShiftedPosition(ref vMoveToTarget, ref point, obstacle.Radius + 5f);
                    }
                }
                else
                {
                    if (DateTime.UtcNow.Subtract(lastShiftedPosition).TotalMilliseconds <= iShiftPositionFor)
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
            bool cancelSpecialMovementAfterStuck = DateTime.UtcNow.Subtract(LastGeneratedStuckPosition).TotalMilliseconds > 10000;

            // See if we can use abilities like leap etc. for movement out of combat, but not in town
            if (Trinity.Settings.Combat.Misc.AllowOOCMovement && !Trinity.Player.IsInTown && !Trinity.bDontMoveMeIAmDoingShit && cancelSpecialMovementAfterStuck)
            {
                bool bTooMuchZChange = (Math.Abs(vMyCurrentPosition.Z - vMoveToTarget.Z) >= 4f);

                // Whirlwind for a barb, special context only
                if (Trinity.Settings.Combat.Barbarian.SprintMode != BarbarianSprintMode.CombatOnly &&
                    Trinity.Hotbar.Contains(SNOPower.Barbarian_Whirlwind) && Trinity.ObjectCache.Any(u => u.Type == GObjectType.Unit && 
                    MathUtil.IntersectsPath(u.Position, u.Radius + 5f, Trinity.Player.Position, vMoveToTarget)) &&
                    Trinity.Player.PrimaryResource >= V.F("Barbarian.Whirlwind.MinFury") && !Trinity.IsWaitingForSpecial && V.B("Barbarian.Whirlwind.UseForMovement"))
                {
                    ZetaDia.Me.UsePower(SNOPower.Barbarian_Whirlwind, vMoveToTarget, Trinity.CurrentWorldDynamicId, -1);
                    if (Trinity.Settings.Advanced.LogCategories.HasFlag(LogCategory.Movement))
                        Logger.Log(TrinityLogLevel.Debug, LogCategory.Movement, "Using Whirlwind for OOC movement, distance={0}", DestinationDistance);
                    return;
                }

                // Leap movement for a barb
                if (Trinity.Settings.Combat.Barbarian.UseLeapOOC && Trinity.Hotbar.Contains(SNOPower.Barbarian_Leap) &&
                    DateTime.UtcNow.Subtract(CacheData.AbilityLastUsedCache[SNOPower.Barbarian_Leap]).TotalMilliseconds >= CombatBase.GetSNOPowerUseDelay(SNOPower.Barbarian_Leap) &&
                    DestinationDistance >= 20f &&
                    PowerManager.CanCast(SNOPower.Barbarian_Leap) && !ShrinesInArea(vMoveToTarget))
                {
                    Vector3 vThisTarget = vMoveToTarget;
                    if (DestinationDistance > 35f)
                        vThisTarget = MathEx.CalculatePointFrom(vMoveToTarget, vMyCurrentPosition, 35f);
                    ZetaDia.Me.UsePower(SNOPower.Barbarian_Leap, vThisTarget, Trinity.CurrentWorldDynamicId, -1);
                    CacheData.AbilityLastUsedCache[SNOPower.Barbarian_Leap] = DateTime.UtcNow;
                    if (Trinity.Settings.Advanced.LogCategories.HasFlag(LogCategory.Movement))
                        Logger.Log(TrinityLogLevel.Debug, LogCategory.Movement, "Using Leap for OOC movement, distance={0}", DestinationDistance);
                    return;
                }
                // Furious Charge movement for a barb
                if (Trinity.Settings.Combat.Barbarian.UseChargeOOC && Trinity.Hotbar.Contains(SNOPower.Barbarian_FuriousCharge) && !bTooMuchZChange &&
                    DateTime.UtcNow.Subtract(CacheData.AbilityLastUsedCache[SNOPower.Barbarian_FuriousCharge]).TotalMilliseconds >= CombatBase.GetSNOPowerUseDelay(SNOPower.Barbarian_FuriousCharge) &&
                    DestinationDistance >= 20f &&
                    PowerManager.CanCast(SNOPower.Barbarian_FuriousCharge) && !ShrinesInArea(vMoveToTarget))
                {
                    Vector3 vThisTarget = vMoveToTarget;
                    if (DestinationDistance > 35f)
                        vThisTarget = MathEx.CalculatePointFrom(vMoveToTarget, vMyCurrentPosition, 35f);
                    ZetaDia.Me.UsePower(SNOPower.Barbarian_FuriousCharge, vThisTarget, Trinity.CurrentWorldDynamicId, -1);
                    CacheData.AbilityLastUsedCache[SNOPower.Barbarian_FuriousCharge] = DateTime.UtcNow;
                    if (Trinity.Settings.Advanced.LogCategories.HasFlag(LogCategory.Movement))
                        Logger.Log(TrinityLogLevel.Debug, LogCategory.Movement, "Using Furious Charge for OOC movement, distance={0}", DestinationDistance);
                    return;
                }
                // Vault for a DH - maximum set by user-defined setting
                if (Trinity.Hotbar.Contains(SNOPower.DemonHunter_Vault) && !bTooMuchZChange &&
                    DateTime.UtcNow.Subtract(CacheData.AbilityLastUsedCache[SNOPower.DemonHunter_Vault]).TotalMilliseconds >= Trinity.Settings.Combat.DemonHunter.VaultMovementDelay &&
                    DestinationDistance >= 18f &&
                    PowerManager.CanCast(SNOPower.DemonHunter_Vault) && !ShrinesInArea(vMoveToTarget) &&
                    // Don't Vault into avoidance/monsters if we're kiting
                    (Trinity.PlayerKiteDistance <= 0 || (Trinity.PlayerKiteDistance > 0 &&
                     (!Trinity.AvoidanceObstacleCache.Any(a => a.Location.Distance(vMoveToTarget) <= Trinity.PlayerKiteDistance) ||
                     (!Trinity.AvoidanceObstacleCache.Any(a => MathEx.IntersectsPath(a.Location, a.Radius, Trinity.Player.Position, vMoveToTarget))) ||
                     !Trinity.MonsterObstacleCache.Any(a => a.Location.Distance(vMoveToTarget) <= Trinity.PlayerKiteDistance))))
                    )
                {

                    Vector3 vThisTarget = vMoveToTarget;
                    if (DestinationDistance > 35f)
                        vThisTarget = MathEx.CalculatePointFrom(vMoveToTarget, vMyCurrentPosition, 35f);
                    ZetaDia.Me.UsePower(SNOPower.DemonHunter_Vault, vThisTarget, Trinity.CurrentWorldDynamicId, -1);
                    CacheData.AbilityLastUsedCache[SNOPower.DemonHunter_Vault] = DateTime.UtcNow;
                    if (Trinity.Settings.Advanced.LogCategories.HasFlag(LogCategory.Movement))
                        Logger.Log(TrinityLogLevel.Debug, LogCategory.Movement, "Using Vault for OOC movement, distance={0}", DestinationDistance);
                    return;
                }
                // Tempest rush for a monk
                if (Trinity.Hotbar.Contains(SNOPower.Monk_TempestRush) &&
                    (Trinity.Settings.Combat.Monk.TROption == TempestRushOption.MovementOnly || Trinity.Settings.Combat.Monk.TROption == TempestRushOption.Always ||
                    (Trinity.Settings.Combat.Monk.TROption == TempestRushOption.TrashOnly && !TargetUtil.AnyElitesInRange(40f))))
                {
                    Vector3 vTargetAimPoint = vMoveToTarget;

                    bool canRayCastTarget = true;

                    vTargetAimPoint = TargetUtil.FindTempestRushTarget();

                    if (!CanChannelTempestRush &&
                        ((Trinity.Player.PrimaryResource >= Trinity.Settings.Combat.Monk.TR_MinSpirit &&
                        DestinationDistance >= Trinity.Settings.Combat.Monk.TR_MinDist) ||
                         DateTime.UtcNow.Subtract(CacheData.AbilityLastUsedCache[SNOPower.Monk_TempestRush]).TotalMilliseconds <= 150) &&
                        canRayCastTarget && PowerManager.CanCast(SNOPower.Monk_TempestRush))
                    {
                        CanChannelTempestRush = true;
                    }
                    else if ((CanChannelTempestRush && (Trinity.Player.PrimaryResource < 10f)) || !canRayCastTarget)
                    {
                        CanChannelTempestRush = false;
                    }

                    double lastUse = DateTime.UtcNow.Subtract(CacheData.AbilityLastUsedCache[SNOPower.Monk_TempestRush]).TotalMilliseconds;

                    if (CanChannelTempestRush)
                    {
                        if (Trinity.SNOPowerUseTimer(SNOPower.Monk_TempestRush))
                        {
                            LastTempestRushPosition = vTargetAimPoint;

                            ZetaDia.Me.UsePower(SNOPower.Monk_TempestRush, vTargetAimPoint, Trinity.CurrentWorldDynamicId, -1);
                            CacheData.AbilityLastUsedCache[SNOPower.Monk_TempestRush] = DateTime.UtcNow;
                            Trinity.LastPowerUsed = SNOPower.Monk_TempestRush;

                            // simulate movement speed of 30
                            SpeedSensor lastSensor = SpeedSensors.OrderByDescending(s => s.Timestamp).FirstOrDefault();
                            SpeedSensors.Add(new SpeedSensor()
                            {
                                Location = vMyCurrentPosition,
                                TimeSinceLastMove = new TimeSpan(0, 0, 0, 0, 1000),
                                Distance = 5f,
                                WorldID = Trinity.CurrentWorldDynamicId
                            });

                            if (Trinity.Settings.Advanced.LogCategories.HasFlag(LogCategory.Movement))
                                Logger.Log(TrinityLogLevel.Debug, LogCategory.Movement, "Using Tempest Rush for OOC movement, distance={0:0} spirit={1:0} cd={2} lastUse={3:0} V3={4} vAim={5}",
                                    DestinationDistance, Trinity.Player.PrimaryResource, PowerManager.CanCast(SNOPower.Monk_TempestRush), lastUse, vMoveToTarget, vTargetAimPoint);
                            return;
                        }
                        else
                            return;
                    }
                    else
                    {
                        if (Trinity.Settings.Advanced.LogCategories.HasFlag(LogCategory.Movement))
                            Logger.Log(TrinityLogLevel.Debug, LogCategory.Movement,
                            "Tempest rush failed!: {0:00.0} / {1} distance: {2:00.0} / {3} Raycast: {4} MS: {5:0.0} lastUse={6:0}",
                            Trinity.Player.PrimaryResource,
                            Trinity.Settings.Combat.Monk.TR_MinSpirit,
                            DestinationDistance,
                            Trinity.Settings.Combat.Monk.TR_MinDist,
                            canRayCastTarget,
                            GetMovementSpeed(),
                            lastUse);

                        Trinity.MaintainTempestRush = false;
                    }

                    // Always set this from PlayerMover
                    Trinity.LastTempestRushLocation = vTargetAimPoint;

                }

                // Dashing Strike OOC
                if (CombatBase.CanCast(SNOPower.X1_Monk_DashingStrike) && Trinity.Settings.Combat.Monk.UseDashingStrikeOOC && DestinationDistance > 10f)
                {
                    ZetaDia.Me.UsePower(SNOPower.X1_Monk_DashingStrike, vMoveToTarget, Trinity.CurrentWorldDynamicId, -1);
                    CacheData.AbilityLastUsedCache[SNOPower.X1_Monk_DashingStrike] = DateTime.UtcNow;
                    if (Trinity.Settings.Advanced.LogCategories.HasFlag(LogCategory.Movement))
                        Logger.Log(TrinityLogLevel.Debug, LogCategory.Movement, "Using Dashing Strike for OOC movement, distance={0}", DestinationDistance);
                }


                bool hasWormHole = HotbarSkills.AssignedSkills.Any(s => s.Power == SNOPower.Wizard_Teleport && s.RuneIndex == 4);

                // Teleport for a wizard (need to be able to check skill rune in DB for a 3-4 teleport spam in a row)
                if (Trinity.Hotbar.Contains(SNOPower.Wizard_Teleport) &&
                    ((PowerManager.CanCast(SNOPower.Wizard_Teleport) &&
                    CombatBase.SNOPowerUseTimer(SNOPower.Wizard_Teleport)) ||
                    (hasWormHole && WizardTeleportCount < 3 && DateTime.UtcNow.Subtract(CacheData.AbilityLastUsedCache[SNOPower.Wizard_Teleport]).TotalMilliseconds >= 250)) &&
                    DestinationDistance >= 10f && !ShrinesInArea(vMoveToTarget))
                {
                    // Reset teleport count if we've already hit the max
                    if (WizardTeleportCount >= 3)
                        WizardTeleportCount = 0;

                    // increment the teleport count for wormhole rune
                    WizardTeleportCount++;

                    var maxTeleportRange = 75f;

                    Vector3 vThisTarget = vMoveToTarget;
                    if (DestinationDistance > maxTeleportRange)
                        vThisTarget = MathEx.CalculatePointFrom(vMoveToTarget, vMyCurrentPosition, maxTeleportRange);
                    ZetaDia.Me.UsePower(SNOPower.Wizard_Teleport, vThisTarget, Trinity.CurrentWorldDynamicId, -1);
                    CacheData.AbilityLastUsedCache[SNOPower.Wizard_Teleport] = DateTime.UtcNow;
                    if (Trinity.Settings.Advanced.LogCategories.HasFlag(LogCategory.Movement))
                        Logger.Log(TrinityLogLevel.Debug, LogCategory.Movement, "Using Teleport for OOC movement, distance={0}", DestinationDistance);
                    return;
                }
                // Archon Teleport for a wizard 
                if (Trinity.Hotbar.Contains(SNOPower.Wizard_Archon_Teleport) &&
                    DateTime.UtcNow.Subtract(CacheData.AbilityLastUsedCache[SNOPower.Wizard_Archon_Teleport]).TotalMilliseconds >= CombatBase.GetSNOPowerUseDelay(SNOPower.Wizard_Archon_Teleport) &&
                    DestinationDistance >= 20f &&
                    PowerManager.CanCast(SNOPower.Wizard_Archon_Teleport) && !ShrinesInArea(vMoveToTarget))
                {
                    Vector3 vThisTarget = vMoveToTarget;
                    if (DestinationDistance > 35f)
                        vThisTarget = MathEx.CalculatePointFrom(vMoveToTarget, vMyCurrentPosition, 35f);
                    ZetaDia.Me.UsePower(SNOPower.Wizard_Archon_Teleport, vThisTarget, Trinity.CurrentWorldDynamicId, -1);
                    CacheData.AbilityLastUsedCache[SNOPower.Wizard_Archon_Teleport] = DateTime.UtcNow;
                    if (Trinity.Settings.Advanced.LogCategories.HasFlag(LogCategory.Movement))
                        Logger.Log(TrinityLogLevel.Debug, LogCategory.Movement, "Using Archon Teleport for OOC movement, distance={0}", DestinationDistance);
                    return;
                }
            }

            if (vMyCurrentPosition.Distance2D(vMoveToTarget) > 1f)
            {
                // Default movement
                ZetaDia.Me.UsePower(SNOPower.Walk, vMoveToTarget, Trinity.CurrentWorldDynamicId, -1);

                if (Trinity.Settings.Advanced.LogCategories.HasFlag(LogCategory.Movement))
                    Logger.Log(TrinityLogLevel.Debug, LogCategory.Movement, "Moved to:{0} dir: {1} Speed:{2:0.00} Dist:{3:0} ZDiff:{4:0} Nav:{5} LoS:{6}",
                        vMoveToTarget, MathUtil.GetHeadingToPoint(vMoveToTarget), MovementSpeed, vMyCurrentPosition.Distance2D(vMoveToTarget),
                        Math.Abs(vMyCurrentPosition.Z - vMoveToTarget.Z),
                        Trinity.MainGridProvider.CanStandAt(Trinity.MainGridProvider.WorldToGrid(vMoveToTarget.ToVector2())),
                        !Navigator.Raycast(vMyCurrentPosition, vMoveToTarget)
                        );

            }
            else
            {
                if (Trinity.Settings.Advanced.LogCategories.HasFlag(LogCategory.Movement))
                    Logger.Log(TrinityLogLevel.Debug, LogCategory.Movement, "Reached MoveTowards Destination {0} Current Speed: {1:0.0}", vMoveToTarget, MovementSpeed);
            }


        }


        internal static int GetObstacleNavigationSize(CacheObstacleObject obstacle)
        {
            if (DataDictionary.ObstacleCustomRadius.ContainsKey(obstacle.ActorSNO))
                return DataDictionary.ObstacleCustomRadius[obstacle.ActorSNO];
            else
                return (int)Math.Ceiling(obstacle.Radius);
        }

        private static void GetShiftedPosition(ref Vector3 vMoveToTarget, ref Vector3 point, float radius = 15f)
        {
            double moveDirection = MathUtil.FindDirectionRadian(vMyCurrentPosition, vMoveToTarget);

            vMoveToTarget = MathEx.GetPointAt(vMyCurrentPosition, radius + 30f, (float)moveDirection);

            if (vMoveToTarget != point)
            {
                vShiftedPosition = vMoveToTarget;
                iShiftPositionFor = 3000;
                lastShiftedPosition = DateTime.UtcNow;
                Logger.Log(TrinityLogLevel.Verbose, LogCategory.Movement, "Navigation handler position shift to: " + vMoveToTarget.ToString() + " (was " + point.ToString() + ")");
            }
        }

        private static Vector3 WarnAndLogLongPath(Vector3 vMoveToTarget)
        {
            // The below code is to help profile/routine makers avoid waypoints with a long distance between them.
            // Long-distances between waypoints is bad - it increases stucks, and forces the DB nav-server to be called.
            if (Trinity.Settings.Advanced.LogStuckLocation)
            {
                if (vLastMoveTo == Vector3.Zero)
                    vLastMoveTo = vMoveToTarget;
                if (vMoveToTarget != vLastMoveTo)
                {
                    float fDistance = Vector3.Distance(vMoveToTarget, vLastMoveTo);
                    // Log if not in town, last waypoint wasn't FROM town, and the distance is >200 but <2000 (cos 2000+ probably means we changed map zones!)
                    if (!Trinity.Player.IsInTown && !bLastWaypointWasTown && fDistance >= 200 & fDistance <= 2000)
                    {
                        if (!hashDoneThisVector.Contains(vMoveToTarget))
                        {
                            // Log it
                            FileStream LogStream = File.Open(Path.Combine(FileManager.LoggingPath, "LongPaths - " + ZetaDia.Me.ActorClass.ToString() + ".log"), FileMode.Append, FileAccess.Write, FileShare.Read);
                            using (StreamWriter LogWriter = new StreamWriter(LogStream))
                            {
                                LogWriter.WriteLine(DateTime.UtcNow.ToString() + ":");
                                LogWriter.WriteLine("Profile Name=" + ProfileManager.CurrentProfile.Name);
                                LogWriter.WriteLine("'From' Waypoint=" + vLastMoveTo.ToString() + ". 'To' Waypoint=" + vMoveToTarget.ToString() + ". Distance=" + fDistance.ToString());
                            }
                            LogStream.Close();
                            hashDoneThisVector.Add(vMoveToTarget);
                        }
                    }
                    vLastMoveTo = vMoveToTarget;
                    bLastWaypointWasTown = false;
                    if (Trinity.Player.IsInTown)
                        bLastWaypointWasTown = true;
                }
            }
            return vMoveToTarget;
        }


        private static TrinityCacheObject CurrentTarget { get { return Trinity.CurrentTarget; } }

        internal static MoveResult NavigateTo(Vector3 moveTarget, string destinationName = "")
        {
            using (new PerformanceLogger("NavigateTo"))
            {
                Vector3 MyPos = Trinity.Player.Position;

                PositionCache.AddPosition();
                MoveResult result;

                try
                {
                    result = Navigator.MoveTo(moveTarget, destinationName, true);
                    Logger.Log(LogCategory.Movement, "Navigator Result={0} dest={1} ({2})", result, moveTarget, destinationName);
                }
                catch (Exception ex)
                {
                    Logger.Log("{0}", ex);
                    return MoveResult.Failed;
                }
                return result;
            }
        }

        private static DateTime lastRecordedSkipAheadCache = DateTime.MinValue;
        internal static void RecordSkipAheadCachePoint()
        {
            if (DateTime.UtcNow.Subtract(lastRecordedSkipAheadCache).TotalMilliseconds < 100)
                return;

            lastRecordedSkipAheadCache = DateTime.UtcNow;

            if (!Trinity.SkipAheadAreaCache.Any(p => p.Location.Distance2D(Trinity.Player.Position) <= 5f))
            {
                Trinity.SkipAheadAreaCache.Add(new CacheObstacleObject() { Location = Trinity.Player.Position, Radius = 20f });
            }
        }


    }
}
