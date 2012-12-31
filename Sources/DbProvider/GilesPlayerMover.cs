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

namespace GilesTrinity.DbProvider
{
    // Player Mover Class
    public class GilesPlayerMover : IPlayerMover
    {
        private static readonly HashSet<int> hashAvoidLeapingToSNO = new HashSet<int> { 138989, 176074, 176076, 176077, 176536, 260330 };
        // 138989 = health pool, 176074 = protection, 176076 = fortune, 176077 = frenzied, 176536 = portal in leorics, 260330 = cooldown shrine
        // Exp shrines = ???? Other shrines ????

        private static bool ShrinesInArea(Vector3 targetpos)
        {
            return GilesTrinity.GilesObjectCache.Any(o => hashAvoidLeapingToSNO.Contains(o.ActorSNO) && Vector3.Distance(o.Position, targetpos) <= 10f);
            //return ZetaDia.Actors.GetActorsOfType<DiaObject>(true).Any(u => hashAvoidLeapingToSNO.Contains(u.ActorSNO) && Vector3.Distance(u.Position, targetpos) <= 10f);
        }

        public void MoveStop()
        {
            ZetaDia.Me.UsePower(SNOPower.Walk, ZetaDia.Me.Position, GilesTrinity.iCurrentWorldID, -1);
        }

        // Anti-stuck variables
        public static Vector3 vOldMoveToTarget = Vector3.Zero;
        public static int iTimesReachedStuckPoint = 0;
        public static int iTotalAntiStuckAttempts = 1;
        public static Vector3 vSafeMovementLocation = Vector3.Zero;
        public static DateTime timeLastRecordedPosition = DateTime.Today;
        public static Vector3 vOldPosition = Vector3.Zero;
        public static DateTime timeStartedUnstuckMeasure = DateTime.Today;
        public static int iTimesReachedMaxUnstucks = 0;
        public static DateTime timeCancelledUnstuckerFor = DateTime.Today;
        public static DateTime timeLastReportedAnyStuck = DateTime.Today;
        public static int iCancelUnstuckerForSeconds = 60;
        public static DateTime timeLastRestartedGame = DateTime.Today;

        // Store player current position
        public static Vector3 vMyCurrentPosition = Vector3.Zero;

        private static int lastKnowCoin;
        private static DateTime lastCheckBag;
        private static DateTime lastRefreshCoin;

        //For Tempest Rush Monks
        private static bool bCanChannelTR = false;

        internal static void ResetCheckGold()
        {
            lastCheckBag = DateTime.Now;
            lastRefreshCoin = DateTime.Now;
            lastKnowCoin = 0;
        }

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
                if (GilesTrinity.CurrentTarget.IsBoss)
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
                    DbHelper.Log(TrinityLogLevel.Normal, LogCategory.Moving, "Gold inactivity after {0}s. Sending abort.", notpickupgoldsec);
                    lastRefreshCoin = DateTime.Now;
                    lastKnowCoin = currentcoin;
                    notpickupgoldsec = 0;
                    return true;
                }
                else if (notpickupgoldsec > 0)
                {
                    DbHelper.Log(TrinityLogLevel.Normal, LogCategory.Moving, "Gold unchanged for {0}s", notpickupgoldsec);
                }
            }
            catch (Exception e)
            {
                DbHelper.Log(TrinityLogLevel.Normal, LogCategory.Moving, e.Message);
            }
            return false;
        }




        // Check if we are stuck or not
        // Simply checks for position changing max once every 3 seconds, to decide on stuck
        public static bool UnstuckChecker(Vector3 vMyCurrentPosition)
        {
            // Keep checking distance changes every 3 seconds
            if (DateTime.Now.Subtract(timeLastRecordedPosition).TotalMilliseconds >= 3000)
            {
                // We're not stuck if we're vendoring!
                if (GilesTrinity.ForceVendorRunASAP || Zeta.CommonBot.Logic.BrainBehavior.IsVendoring)
                {
                    return false;
                }


                timeLastRecordedPosition = DateTime.Now;
                Composite c = null;
                try
                {
                    if (Zeta.CommonBot.ProfileManager.CurrentProfileBehavior != null)
                        c = Zeta.CommonBot.ProfileManager.CurrentProfileBehavior.Behavior;
                }
                catch { }
                Zeta.Internals.UIElement vendorWindow = Zeta.Internals.UIElements.VendorWindow;
                AnimationState aState = ZetaDia.Me.CommonData.AnimationState;
                if (c != null && c.GetType() == typeof(WaitTimerTag))
                {
                    vOldPosition = Vector3.Zero;
                    ResetCheckGold();
                    return false;
                }
                // We're not stuck if we're doing stuff!
                if (ZetaDia.Me.IsInConversation || ZetaDia.IsPlayingCutscene || ZetaDia.IsLoadingWorld || (vendorWindow.IsValid && vendorWindow.IsVisible))
                {
                    vOldPosition = Vector3.Zero;
                    ResetCheckGold();
                    return false;
                }

                // We're not stuck if we're doing stuff!
                if (aState == AnimationState.Attacking ||
                    aState == AnimationState.Casting ||
                    aState == AnimationState.Channeling)
                {
                    vOldPosition = Vector3.Zero;
                    ResetCheckGold();
                    return false;
                }

                if (vOldPosition != Vector3.Zero && vOldPosition.Distance(vMyCurrentPosition) <= 4f)
                {
                    return true;
                }

                vOldPosition = vMyCurrentPosition;
            }
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
                DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.Moving, "Clearing old route and trying new path find to: " + vOldMoveToTarget.ToString());
                Navigator.MoveTo(vOldMoveToTarget, "original destination", false);
                return vSafeMovementLocation;
            }
            // Only try an unstuck 10 times maximum in XXX period of time
            if (Vector3.Distance(vOriginalDestination, vMyCurrentPosition) >= 700f)
            {
                DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.Moving, "You are " + Vector3.Distance(vOriginalDestination, vMyCurrentPosition).ToString() + " distance away from your destination.");
                DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.Moving, "This is too far for the unstucker, and is likely a sign of ending up in the wrong map zone.");
                iTotalAntiStuckAttempts = 20;
            }
            // intell
            if (iTotalAntiStuckAttempts <= 15)
            {
                DbHelper.Log(TrinityLogLevel.Normal, LogCategory.Moving, "Your bot got stuck! Trying to unstuck (attempt #{0} of 15 attempts) {1} {2} {3} {4}",
                    iTotalAntiStuckAttempts.ToString(),
                    "Act=\"" + ZetaDia.CurrentAct + "\"",
                    "questId=\"" + ZetaDia.CurrentQuest.QuestSNO + "\"",
                    "stepId=\"" + ZetaDia.CurrentQuest.StepId + "\"",
                    "worldId=\"" + ZetaDia.CurrentWorldId + "\""
                );

                DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.Moving, "(destination=" + vOriginalDestination.ToString() + ", which is " + Vector3.Distance(vOriginalDestination, vMyCurrentPosition).ToString() + " distance away)");
                GilesTrinity.playerStatus.CurrentPosition = vMyCurrentPosition;
                vSafeMovementLocation = GilesTrinity.FindSafeZone(true, iTotalAntiStuckAttempts, Vector3.Zero);
                // Temporarily log stuff
                if (iTotalAntiStuckAttempts == 1 && GilesTrinity.Settings.Advanced.LogStuckLocation)
                {
                    FileStream LogStream = File.Open(Path.Combine(FileManager.LoggingPath, "Stucks - " + GilesTrinity.playerStatus.ActorClass.ToString() + ".log"), FileMode.Append, FileAccess.Write, FileShare.Read);
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
            timeLastRecordedPosition = DateTime.Today;
            timeStartedUnstuckMeasure = DateTime.Today;
            // int iSafetyLoops = 0;
            if (iTimesReachedMaxUnstucks == 1)
            {
                Navigator.Clear();
                DbHelper.Log(TrinityLogLevel.Normal, LogCategory.Moving, "Anti-stuck measures now attempting to kickstart DB's path-finder into action.");
                Navigator.MoveTo(vOriginalDestination, "original destination", false);
                iCancelUnstuckerForSeconds = 40;
                timeCancelledUnstuckerFor = DateTime.Now;
                return vSafeMovementLocation;
            }
            if (iTimesReachedMaxUnstucks == 2)
            {
                DbHelper.Log(TrinityLogLevel.Normal, LogCategory.Moving, "Anti-stuck measures failed. Now attempting to reload current profile.");

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


        public void MoveTowards(Vector3 vMoveToTarget)
        {
            // rrrix-note: This really shouldn't be here... 
            // Recording of all the XML's in use this run
            string sThisProfile = Zeta.CommonBot.Settings.GlobalSettings.Instance.LastProfile;
            RecordLastProfile(sThisProfile);


            vMoveToTarget = WarnAndLogLongPath(vMoveToTarget);

            // Make sure GilesTrinity doesn't want us to avoid routine-movement
            //if (GilesTrinity.bDontMoveMeIAmDoingShit)
            //    return;
            // Store player current position

            vMyCurrentPosition = ZetaDia.Me.Position;

            // Store distance to current moveto target
            float fDistanceFromTarget;

            // Do unstuckery things
            if (GilesTrinity.Settings.Advanced.UnstuckerEnabled)
            {
                if (GoldInactive())
                {
                    GoldInactiveLeaveGame();
                    return;
                }
                // Store the "real" (not anti-stuck) destination
                vOldMoveToTarget = vMoveToTarget;
                // See if we can reset the 10-limit unstuck counter, if >120 seconds since we last generated an unstuck location
                if (iTotalAntiStuckAttempts > 1 && DateTime.Now.Subtract(timeStartedUnstuckMeasure).TotalSeconds >= 120)
                {
                    iTotalAntiStuckAttempts = 1;
                    iTimesReachedStuckPoint = 0;
                    vSafeMovementLocation = Vector3.Zero;
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
                fDistanceFromTarget = Vector3.Distance(vMyCurrentPosition, vMoveToTarget);
                // Remove the stuck position if it's been reached, this bit of code also creates multiple stuck-patterns in an ever increasing amount
                if (vSafeMovementLocation != Vector3.Zero && fDistanceFromTarget <= 3f)
                {
                    vSafeMovementLocation = Vector3.Zero;
                    iTimesReachedStuckPoint++;
                    // Do we want to immediately generate a 2nd waypoint to "chain" anti-stucks in an ever-increasing path-length?
                    if (iTimesReachedStuckPoint <= iTotalAntiStuckAttempts)
                    {
                        GilesTrinity.playerStatus.CurrentPosition = vMyCurrentPosition;
                        vSafeMovementLocation = GilesTrinity.FindSafeZone(true, iTotalAntiStuckAttempts, Vector3.Zero);
                        vMoveToTarget = vSafeMovementLocation;
                    }
                    else
                    {
                        DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.Moving, "Clearing old route and trying new path find to: " + vOldMoveToTarget.ToString());
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
            else
            {
                // Get distance to current destination
                fDistanceFromTarget = Vector3.Distance(vMyCurrentPosition, vMoveToTarget);
            }
            // Is the built-in unstucker enabled or not?
            // if (GilesTrinity.Settings.Advanced.DebugInStatusBar)
            // {
            //    Logging.WriteDiagnostic("[Trinity] Moving toward <{0:0},{1:0},{2:0}> distance: {3:0}", vMoveToTarget.X, vMoveToTarget.Y, vMoveToTarget.Z, fDistanceFromTarget);
            // }
            // See if there's an obstacle in our way, if so try to navigate around it
            Vector3 point = vMoveToTarget;
            foreach (GilesObstacle tempobstacle in GilesTrinity.hashNavigationObstacleCache.Where(cp =>
                            GilesTrinity.GilesIntersectsPath(cp.Location, cp.Radius, vMyCurrentPosition, point) &&
                            cp.Location.Distance(vMyCurrentPosition) > GetObstacleNavigationSize(cp)))
            {
                if (vShiftedPosition == Vector3.Zero)
                {
                    // Make sure we only shift max once every 10 seconds
                    if (DateTime.Now.Subtract(lastShiftedPosition).TotalSeconds >= 10)
                    {
                        GetShiftedPosition(ref vMoveToTarget, ref point);
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
            if (GilesTrinity.Settings.Combat.Misc.AllowOOCMovement && !ZetaDia.Me.IsInTown)
            {
                bool bTooMuchZChange = (Math.Abs(vMyCurrentPosition.Z - vMoveToTarget.Z) >= 4f);

                // Leap movement for a barb
                if (GilesTrinity.hashPowerHotbarAbilities.Contains(SNOPower.Barbarian_Leap) &&
                    DateTime.Now.Subtract(GilesTrinity.dictAbilityLastUse[SNOPower.Barbarian_Leap]).TotalMilliseconds >= GilesTrinity.dictAbilityRepeatDelay[SNOPower.Barbarian_Leap] &&
                    fDistanceFromTarget >= 20f &&
                    PowerManager.CanCast(SNOPower.Barbarian_Leap) && !ShrinesInArea(vMoveToTarget))
                {
                    Vector3 vThisTarget = vMoveToTarget;
                    if (fDistanceFromTarget > 35f)
                        vThisTarget = MathEx.CalculatePointFrom(vMoveToTarget, vMyCurrentPosition, 35f);
                    ZetaDia.Me.UsePower(SNOPower.Barbarian_Leap, vThisTarget, GilesTrinity.iCurrentWorldID, -1);
                    GilesTrinity.dictAbilityLastUse[SNOPower.Barbarian_Leap] = DateTime.Now;
                    DbHelper.Log(TrinityLogLevel.Debug, LogCategory.Moving, "Using Leap for OOC movement, distance={0}", fDistanceFromTarget);
                    return;
                }
                // Furious Charge movement for a barb
                if (GilesTrinity.hashPowerHotbarAbilities.Contains(SNOPower.Barbarian_FuriousCharge) && !bTooMuchZChange &&
                    DateTime.Now.Subtract(GilesTrinity.dictAbilityLastUse[SNOPower.Barbarian_FuriousCharge]).TotalMilliseconds >= GilesTrinity.dictAbilityRepeatDelay[SNOPower.Barbarian_FuriousCharge] &&
                    fDistanceFromTarget >= 20f &&
                    PowerManager.CanCast(SNOPower.Barbarian_FuriousCharge) && !ShrinesInArea(vMoveToTarget))
                {
                    Vector3 vThisTarget = vMoveToTarget;
                    if (fDistanceFromTarget > 35f)
                        vThisTarget = MathEx.CalculatePointFrom(vMoveToTarget, vMyCurrentPosition, 35f);
                    ZetaDia.Me.UsePower(SNOPower.Barbarian_FuriousCharge, vThisTarget, GilesTrinity.iCurrentWorldID, -1);
                    GilesTrinity.dictAbilityLastUse[SNOPower.Barbarian_FuriousCharge] = DateTime.Now;
                    DbHelper.Log(TrinityLogLevel.Debug, LogCategory.Moving, "Using Furious Charge for OOC movement, distance={0}", fDistanceFromTarget);
                    return;
                }
                // Vault for a DH - maximum set by user-defined setting
                if (GilesTrinity.hashPowerHotbarAbilities.Contains(SNOPower.DemonHunter_Vault) && !bTooMuchZChange &&
                    DateTime.Now.Subtract(GilesTrinity.dictAbilityLastUse[SNOPower.DemonHunter_Vault]).TotalMilliseconds >= GilesTrinity.Settings.Combat.DemonHunter.VaultMovementDelay &&
                    fDistanceFromTarget >= 18f &&
                    PowerManager.CanCast(SNOPower.DemonHunter_Vault) && !ShrinesInArea(vMoveToTarget) &&
                    // Don't Vault into avoidance/monsters if we're kiting
                    (GilesTrinity.PlayerKiteDistance <= 0 || (GilesTrinity.PlayerKiteDistance > 0 &&
                     (!GilesTrinity.hashAvoidanceObstacleCache.Any(a => a.Location.Distance(vMoveToTarget) <= GilesTrinity.PlayerKiteDistance) ||
                     (!GilesTrinity.hashAvoidanceObstacleCache.Any(a => MathEx.IntersectsPath(a.Location, a.Radius, GilesTrinity.playerStatus.CurrentPosition, vMoveToTarget))) ||
                     !GilesTrinity.hashMonsterObstacleCache.Any(a => a.Location.Distance(vMoveToTarget) <= GilesTrinity.PlayerKiteDistance))))
                    )
                {

                    Vector3 vThisTarget = vMoveToTarget;
                    if (fDistanceFromTarget > 35f)
                        vThisTarget = MathEx.CalculatePointFrom(vMoveToTarget, vMyCurrentPosition, 35f);
                    ZetaDia.Me.UsePower(SNOPower.DemonHunter_Vault, vThisTarget, GilesTrinity.iCurrentWorldID, -1);
                    GilesTrinity.dictAbilityLastUse[SNOPower.DemonHunter_Vault] = DateTime.Now;
                    DbHelper.Log(TrinityLogLevel.Debug, LogCategory.Moving, "Using Vault for OOC movement, distance={0}", fDistanceFromTarget);
                    return;
                }
                // Tempest rush for a monk
                if (GilesTrinity.hashPowerHotbarAbilities.Contains(SNOPower.Monk_TempestRush) && !bTooMuchZChange)
                {
                    if (!bCanChannelTR && GilesTrinity.playerStatus.CurrentEnergy >= GilesTrinity.Settings.Combat.Monk.TR_MinSpirit && fDistanceFromTarget > GilesTrinity.Settings.Combat.Monk.TR_MinDist)
                        bCanChannelTR = true;
                    else if (bCanChannelTR && (GilesTrinity.playerStatus.CurrentEnergy <= 20 || fDistanceFromTarget < 10f))
                        bCanChannelTR = false;

                    if (bCanChannelTR)
                    {
                        float aimPointDistance = 20f;
                        Vector3 vTargetAimPoint = vMoveToTarget;
                        if (fDistanceFromTarget > aimPointDistance)
                        {
                            vTargetAimPoint = MathEx.CalculatePointFrom(vMoveToTarget, vMyCurrentPosition, aimPointDistance);
                        }

                        ZetaDia.Me.UsePower(SNOPower.Monk_TempestRush, vTargetAimPoint, GilesTrinity.iCurrentWorldID, -1);

                        //ZetaDia.Me.UsePower(SNOPower.Monk_TempestRush, vMoveToTarget);

                        GilesTrinity.dictAbilityLastUse[SNOPower.Monk_TempestRush] = DateTime.Now;
                        DbHelper.Log(TrinityLogLevel.Debug, LogCategory.Moving, "Using Tempest Rush for OOC movement, distance={0:0}", fDistanceFromTarget);
                        return;
                    }
                }
                // Teleport for a wizard (need to be able to check skill rune in DB for a 3-4 teleport spam in a row)
                if (GilesTrinity.hashPowerHotbarAbilities.Contains(SNOPower.Wizard_Teleport) &&
                    DateTime.Now.Subtract(GilesTrinity.dictAbilityLastUse[SNOPower.Wizard_Teleport]).TotalMilliseconds >= GilesTrinity.dictAbilityRepeatDelay[SNOPower.Wizard_Teleport] &&
                    fDistanceFromTarget >= 20f &&
                    PowerManager.CanCast(SNOPower.Wizard_Teleport) && !ShrinesInArea(vMoveToTarget))
                {
                    Vector3 vThisTarget = vMoveToTarget;
                    if (fDistanceFromTarget > 35f)
                        vThisTarget = MathEx.CalculatePointFrom(vMoveToTarget, vMyCurrentPosition, 35f);
                    ZetaDia.Me.UsePower(SNOPower.Wizard_Teleport, vThisTarget, GilesTrinity.iCurrentWorldID, -1);
                    GilesTrinity.dictAbilityLastUse[SNOPower.Wizard_Teleport] = DateTime.Now;
                    DbHelper.Log(TrinityLogLevel.Debug, LogCategory.Moving, "Using Teleport for OOC movement, distance={0}", fDistanceFromTarget);
                    return;
                }
                // Archon Teleport for a wizard 
                if (GilesTrinity.hashPowerHotbarAbilities.Contains(SNOPower.Wizard_Archon_Teleport) &&
                    DateTime.Now.Subtract(GilesTrinity.dictAbilityLastUse[SNOPower.Wizard_Archon_Teleport]).TotalMilliseconds >= GilesTrinity.dictAbilityRepeatDelay[SNOPower.Wizard_Archon_Teleport] &&
                    fDistanceFromTarget >= 20f &&
                    PowerManager.CanCast(SNOPower.Wizard_Archon_Teleport) && !ShrinesInArea(vMoveToTarget))
                {
                    Vector3 vThisTarget = vMoveToTarget;
                    if (fDistanceFromTarget > 35f)
                        vThisTarget = MathEx.CalculatePointFrom(vMoveToTarget, vMyCurrentPosition, 35f);
                    ZetaDia.Me.UsePower(SNOPower.Wizard_Archon_Teleport, vThisTarget, GilesTrinity.iCurrentWorldID, -1);
                    GilesTrinity.dictAbilityLastUse[SNOPower.Wizard_Archon_Teleport] = DateTime.Now;
                    DbHelper.Log(TrinityLogLevel.Debug, LogCategory.Moving, "Using Archon Teleport for OOC movement, distance={0}", fDistanceFromTarget);
                    return;
                }
            }

            if (fDistanceFromTarget > 1f)
            {
                // Default movement
                //int moveResult = ZetaDia.Me.Movement.MoveActor(vMoveToTarget);
                ZetaDia.Me.UsePower(SNOPower.Walk, vMoveToTarget, GilesTrinity.iCurrentWorldID, -1);
                //ZetaDia.Me.UsePower(SNOPower.Walk, vMoveToTarget);

                DbHelper.Log(TrinityLogLevel.Debug, LogCategory.Moving, "Used basic movement to {0}", vMoveToTarget);
            }
            else
            {
                DbHelper.Log(TrinityLogLevel.Debug, LogCategory.Moving, "Reached MoveTowards Destination {0}", vMoveToTarget);
            }


        }

        private static void GoldInactiveLeaveGame()
        {
            // Exit the game and reload the profile
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
            return;
        }

        internal static int GetObstacleNavigationSize(GilesObstacle obstacle)
        {
            if (GilesTrinity.dictSNONavigationSize.ContainsKey(obstacle.ActorSNO))
                return GilesTrinity.dictSNONavigationSize[obstacle.ActorSNO];
            else
                return (int)Math.Ceiling(obstacle.Radius);
        }

        private static void GetShiftedPosition(ref Vector3 vMoveToTarget, ref Vector3 point)
        {
            float fDirectionToTarget = GilesTrinity.FindDirectionDegree(vMyCurrentPosition, vMoveToTarget);
            vMoveToTarget = MathEx.GetPointAt(vMyCurrentPosition, 15f, MathEx.ToRadians(fDirectionToTarget - 50));
            if (!GilesTrinity.GilesCanRayCast(vMyCurrentPosition, vMoveToTarget, NavCellFlags.AllowWalk))
            {
                vMoveToTarget = MathEx.GetPointAt(vMyCurrentPosition, 15f, MathEx.ToRadians(fDirectionToTarget + 50));
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
                DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.Moving, "Navigation handler position shift to: " + vMoveToTarget.ToString() + " (was " + point.ToString() + ")");
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

        private static void RecordLastProfile(string sThisProfile)
        {
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
                if (GilesTrinity.sFirstProfileSeen == "")
                    GilesTrinity.sFirstProfileSeen = sThisProfile;
            }
        }
    }
}
