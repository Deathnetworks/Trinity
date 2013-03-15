using GilesTrinity.DbProvider;
using GilesTrinity.Settings.Combat;
using GilesTrinity.Technicals;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Zeta;
using Zeta.Common;
using Zeta.Common.Plugins;
using Zeta.CommonBot;
using Zeta.Internals.Actors;
using Zeta.Internals.SNO;
using Zeta.TreeSharp;
using Action = Zeta.TreeSharp.Action;
using Decorator = Zeta.TreeSharp.Decorator;
using System.Diagnostics;
using Zeta.Navigation;
using GilesTrinity.XmlTags;
namespace GilesTrinity
{
    public partial class GilesTrinity : IPlugin
    {

        /// <summary>
        /// Returns the current DiaPlayer
        /// </summary>
        public static DiaActivePlayer Me
        {
            get { return ZetaDia.Me; }
        }

        /// <summary>
        /// Decorator for main Action delegate, also handles bot pausing
        /// </summary>
        /// <returns></returns>
        private static Composite HandleTargetAction()
        {
            return new PrioritySelector(
                new Decorator(ret => ZetaDia.IsInGame && !ZetaDia.IsLoadingWorld && !bMainBotPaused,
                    new Action(ctx => HandleTarget(ctx))
                ),
                new Decorator(ret => ZetaDia.IsInGame && !ZetaDia.IsLoadingWorld && bMainBotPaused,
                    new Action(ret => PausedAction(ret))
                )
            );
        }

        private static RunStatus PausedAction(object ret)
        {
            return bMainBotPaused ? RunStatus.Running : RunStatus.Success;
        }

        /// <summary>
        /// Help determine runstatus in the main action
        /// </summary>
        private enum HandlerRunStatus
        {
            NotFinished,
            TreeRunning,
            TreeSuccess,
            TreeFailure
        }

        private static bool StaleCache = false;

        /// <summary>
        /// Returns a RunStatus, if appropriate. Throws an exception if not.
        /// </summary>
        /// <param name="rs"></param>
        /// <returns></returns>
        private static RunStatus GetTreeSharpRunStatus(HandlerRunStatus rs)
        {
            Monk_MaintainTempestRush();

            RunStatus treeRunStatus;

            switch (rs)
            {
                case HandlerRunStatus.TreeFailure:
                    treeRunStatus = RunStatus.Failure; break;
                case HandlerRunStatus.TreeRunning:
                    treeRunStatus = RunStatus.Running; break;
                case HandlerRunStatus.TreeSuccess:
                    treeRunStatus = RunStatus.Success; break;
                case HandlerRunStatus.NotFinished:
                default:
                    throw new ApplicationException("Unable to return Non-TreeSharp RunStatus");
            }
            string extras = "";
            if (IsWaitingForPower)
                extras += " IsWaitingForPower";
            if (IsWaitingAfterPower)
                extras += " IsWaitingAfterPower";
            if (IsWaitingForPotion)
                extras += " IsWaitingForPotion";
            if (TownRun.IsTryingToTownPortal())
                extras += " IsTryingToTownPortal";
            if (TownRun.TownRunTimerRunning())
                extras += " TownRunTimerRunning";
            if (TownRun.TownRunTimerFinished())
                extras += " TownRunTimerFinished";
            if (ForceTargetUpdate)
                extras += " ForceTargetUpdate";
            if (CurrentTarget == null)
                extras += " CurrentTargetIsNull";
            if (CurrentPower != null && CurrentPower.ShouldWaitBeforeUse)
                extras += " CPowerShouldWaitBefore=" + (CurrentPower.WaitBeforeUseDelay - CurrentPower.TimeSinceAssigned);
            if (CurrentPower != null && CurrentPower.ShouldWaitAfterUse)
                extras += " CPowerShouldWaitAfter=" + (CurrentPower.WaitAfterUseDelay - CurrentPower.TimeSinceUse);
            if (CurrentPower != null && (CurrentPower.ShouldWaitBeforeUse || CurrentPower.ShouldWaitAfterUse))
                extras += " " + CurrentPower.ToString();

            DbHelper.Log(TrinityLogLevel.Debug, LogCategory.Behavior, "Handle Target returning {0} to tree" + extras, treeRunStatus);
            return treeRunStatus;

        }

        /// <summary>
        /// Handles all aspects of moving to and attacking the current target
        /// </summary>
        /// <param name="ret"></param>
        /// <returns></returns>
        private static RunStatus HandleTarget(object ret)
        {
            HandlerRunStatus runStatus = HandlerRunStatus.NotFinished;
            using (new PerformanceLogger("HandleTarget"))
            {
                try
                {
                    if (!ZetaDia.IsInGame || !ZetaDia.Me.IsValid || ZetaDia.IsLoadingWorld)
                    {
                        DbHelper.Log(TrinityLogLevel.Error, LogCategory.UserInformation, "No longer in game world", true);
                        runStatus = HandlerRunStatus.TreeFailure;
                        return GetTreeSharpRunStatus(runStatus);
                    }
                    if (ZetaDia.Me.IsDead)
                    {
                        DbHelper.Log(TrinityLogLevel.Error, LogCategory.UserInformation, "Player is dead", true);
                        runStatus = HandlerRunStatus.TreeFailure;
                        return GetTreeSharpRunStatus(runStatus);
                    }
                    else if (GoldInactivity.GoldInactive())
                    {
                        BotMain.PauseWhile(GoldInactivity.GoldInactiveLeaveGame);
                        DbHelper.Log(TrinityLogLevel.Error, LogCategory.UserInformation, "Gold Inactivity Tripped", true);
                        runStatus = HandlerRunStatus.TreeFailure;
                        return GetTreeSharpRunStatus(runStatus);
                    }

                    // Make sure we reset unstucker stuff here
                    PlayerMover.iTimesReachedStuckPoint = 0;
                    PlayerMover.vSafeMovementLocation = Vector3.Zero;
                    PlayerMover.TimeLastRecordedPosition = DateTime.Now;

                    // Whether we should refresh the target list or not
                    // See if we should update hotbar abilities
                    if (ShouldRefreshHotbarAbilities)
                    {
                        GilesPlayerCache.RefreshHotbar();
                    }
                    if (!IsWaitingForPower && CurrentPower == null && CurrentTarget != null)
                        CurrentPower = AbilitySelector();

                    // Time based wait delay for certain powers with animations
                    if (IsWaitingAfterPower && CurrentPower.ShouldWaitAfterUse)
                    {
                        runStatus = HandlerRunStatus.TreeRunning;
                    }
                    else
                    {
                        IsWaitingAfterPower = false;
                    }

                    //check if we are returning to the tree
                    if (runStatus != HandlerRunStatus.NotFinished)
                        return GetTreeSharpRunStatus(runStatus);

                    // See if we have been "newly rooted", to force target updates
                    if (PlayerStatus.IsRooted && !wasRootedLastTick)
                    {
                        wasRootedLastTick = true;
                        ForceTargetUpdate = true;
                    }
                    if (!PlayerStatus.IsRooted)
                        wasRootedLastTick = false;
                    if (CurrentTarget == null)
                    {
                        DbHelper.Log(TrinityLogLevel.Debug, LogCategory.Behavior, "CurrentTarget was passed as null! Continuing...");
                    }

                    Monk_MaintainTempestRush();

                    CheckStaleCache();
                    using (new PerformanceLogger("HandleTarget.CheckForNewTarget"))
                    {
                        // So, after all that, do we actually want a new target list?
                        if (!IsWholeNewTarget && !IsWaitingForPower && !IsWaitingForPotion)
                        {
                            // If we *DO* want a new target list, do this... 
                            if (StaleCache)
                            {
                                // Now call the function that refreshes targets
                                RefreshDiaObjectCache();

                                // No target, return success
                                if (CurrentTarget == null)
                                {
                                    runStatus = HandlerRunStatus.TreeSuccess;
                                }
                                else
                                {
                                    // Make sure we start trying to move again should we need to!
                                    IsAlreadyMoving = false;
                                    lastMovementCommand = DateTime.Today;
                                    ShouldPickNewAbilities = true;
                                }
                            }
                            // Always Update the current target 
                            UpdateCurrentTarget();
                        }
                    }

                    if (CurrentTarget == null && (ForceVendorRunASAP || IsReadyToTownRun) && !Zeta.CommonBot.Logic.BrainBehavior.IsVendoring && TownRun.TownRunTimerRunning())
                    {
                        DbHelper.Log(TrinityLogLevel.Normal, LogCategory.Behavior, "CurrentTarget is null but we are ready to to Town Run, waiting... ");
                        runStatus = HandlerRunStatus.TreeRunning;
                    }

                    //check if we are returning to the tree
                    if (runStatus != HandlerRunStatus.NotFinished)
                        return GetTreeSharpRunStatus(runStatus);

                    if (CurrentTarget == null && TownRun.IsTryingToTownPortal() && TownRun.TownRunTimerRunning())
                    {
                        DbHelper.Log(TrinityLogLevel.Normal, LogCategory.Behavior, "Waiting for town run... ");
                        runStatus = HandlerRunStatus.TreeRunning;
                    }
                    else if (CurrentTarget == null && TownRun.IsTryingToTownPortal() && TownRun.TownRunTimerFinished())
                    {
                        DbHelper.Log(TrinityLogLevel.Normal, LogCategory.Behavior, "Town Run Ready!");
                        runStatus = HandlerRunStatus.TreeSuccess;
                    }
                    //check if we are returning to the tree
                    if (runStatus != HandlerRunStatus.NotFinished)
                        return GetTreeSharpRunStatus(runStatus);

                    if (CurrentTarget == null)
                    {
                        DbHelper.Log(TrinityLogLevel.Normal, LogCategory.Behavior, "CurrentTarget set as null in refresh! Error 2");
                        runStatus = HandlerRunStatus.TreeFailure;
                    }


                    //check if we are returning to the tree
                    if (runStatus != HandlerRunStatus.NotFinished)
                        return GetTreeSharpRunStatus(runStatus);

                    // Handle Target stuck / timeout
                    runStatus = HandleTargetTimeout(runStatus);

                    //check if we are returning to the tree
                    if (runStatus != HandlerRunStatus.NotFinished)
                        return GetTreeSharpRunStatus(runStatus);

                    // This variable just prevents an instant 2-target update after coming here from the main decorator function above
                    IsWholeNewTarget = false;
                    AssignMonsterTargetPower();

                    // Pop a potion when necessary
                    // Note that we force a single-loop pause first, to help potion popping "go off"
                    if (PlayerStatus.CurrentHealthPct <= PlayerEmergencyHealthPotionLimit && !IsWaitingForPower && !IsWaitingForPotion
                        && !PlayerStatus.IsIncapacitated && GilesUseTimer(SNOPower.DrinkHealthPotion))
                    {
                        IsWaitingForPotion = true;
                        runStatus = HandlerRunStatus.TreeRunning;
                    }

                    //check if we are returning to the tree
                    if (runStatus != HandlerRunStatus.NotFinished)
                        return GetTreeSharpRunStatus(runStatus);

                    // If we just looped waiting for a potion, use it
                    UseHealthPotionIfNeeded();

                    using (new PerformanceLogger("HandleTarget.CheckAvoidanceBuffs"))
                    {
                        // See if we can use any special buffs etc. while in avoidance
                        if (CurrentTarget.Type == GObjectType.Avoidance)
                        {
                            powerBuff = AbilitySelector(true, false, false);
                            if (powerBuff.SNOPower != SNOPower.None)
                            {
                                ZetaDia.Me.UsePower(powerBuff.SNOPower, powerBuff.TargetPosition, powerBuff.TargetDynamicWorldId, powerBuff.TargetRActorGUID);
                                LastPowerUsed = powerBuff.SNOPower;
                                dictAbilityLastUse[powerBuff.SNOPower] = DateTime.Now;
                            }
                        }
                    }

                    // Pick the destination point and range of target
                    /*
                     * Set the range required for attacking/interacting/using
                     */

                    SetRangeRequiredForTarget();


                    using (new PerformanceLogger("HandleTarget.SpecialNavigation"))
                    {
                        PositionCache.AddPosition();

                        // Maintain an area list of all zones we pass through/near while moving, for our custom navigation handler
                        if (DateTime.Now.Subtract(lastAddedLocationCache).TotalMilliseconds >= 100)
                        {
                            lastAddedLocationCache = DateTime.Now;
                            if (Vector3.Distance(PlayerStatus.CurrentPosition, vLastRecordedLocationCache) >= 5f)
                            {
                                hashSkipAheadAreaCache.Add(new GilesObstacle(PlayerStatus.CurrentPosition, 20f, 0));
                                vLastRecordedLocationCache = PlayerStatus.CurrentPosition;

                                // Mark Dungeon Explorer nodes as Visited if combat pulls us into it
                                if (ProfileManager.CurrentProfileBehavior != null)
                                {
                                    Type profileBehaviorType = ProfileManager.CurrentProfileBehavior.GetType();
                                    if (profileBehaviorType == typeof(TrinityExploreDungeon))
                                    {
                                        TrinityExploreDungeon trinityExploreDungeonTag = (TrinityExploreDungeon)ProfileManager.CurrentProfileBehavior;
                                        trinityExploreDungeonTag.MarkNearbyNodesVisited();
                                    }
                                }
                            }
                        }
                        // Maintain a backtrack list only while fighting monsters
                        if (CurrentTarget.Type == GObjectType.Unit && Settings.Combat.Misc.AllowBacktracking &&
                            (iTotalBacktracks == 0 || Vector3.Distance(PlayerStatus.CurrentPosition, vBacktrackList[iTotalBacktracks]) >= 10f))
                        {
                            bool bAddThisBacktrack = true;
                            // Check we aren't within 12 feet of 2 backtracks again (eg darting back & forth)
                            if (iTotalBacktracks >= 2)
                            {
                                if (Vector3.Distance(PlayerStatus.CurrentPosition, vBacktrackList[iTotalBacktracks - 1]) < 12f)
                                    bAddThisBacktrack = false;
                            }
                            if (bAddThisBacktrack)
                            {
                                iTotalBacktracks++;
                                vBacktrackList.Add(iTotalBacktracks, PlayerStatus.CurrentPosition);
                            }
                        }
                    }


                    using (new PerformanceLogger("HandleTarget.LoSCheck"))
                    {
                        TargetCurrentDistance = PlayerStatus.CurrentPosition.Distance2D(vCurrentDestination) - TargetDistanceReduction;
                        if (TargetCurrentDistance < 0f)
                            TargetCurrentDistance = 0f;

                        if (TargetCurrentDistance <= 20f)
                        {
                            CurrentTargetIsInLoS = true;
                        }
                        else if (Settings.Combat.Misc.UseNavMeshTargeting && CurrentTarget.Type != GObjectType.Barricade && CurrentTarget.Type != GObjectType.Destructible)
                        {
                            CurrentTargetIsInLoS = (NavHelper.CanRayCast(PlayerStatus.CurrentPosition, vCurrentDestination) || LineOfSightWhitelist.Contains(CurrentTarget.ActorSNO));
                        }
                        else
                        {
                            CurrentTargetIsInLoS = true;
                        }
                    }

                    using (new PerformanceLogger("HandleTarget.InRange"))
                    {
                        // Interact/use power on target if already in range
                        if (TargetRangeRequired <= 0f || TargetCurrentDistance <= TargetRangeRequired && CurrentTargetIsInLoS)
                        {
                            // If avoidance, instantly skip
                            if (CurrentTarget.Type == GObjectType.Avoidance)
                            {
                                //vlastSafeSpot = vNullLocation;
                                ForceTargetUpdate = true;
                                //bAvoidDirectionBlacklisting = false;
                                runStatus = HandlerRunStatus.TreeRunning;
                            }
                            //check if we are returning to the tree
                            if (runStatus != HandlerRunStatus.NotFinished)
                                return GetTreeSharpRunStatus(runStatus);

                            UpdateStatusTextTarget(true);

                            // An integer to log total interact attempts on a particular object or item
                            int iInteractAttempts;
                            switch (CurrentTarget.Type)
                            {
                                // Unit, use our primary power to attack
                                case GObjectType.Unit:
                                    {
                                        if (CurrentPower.SNOPower != SNOPower.None)
                                        {
                                            if (IsWaitingForPower && CurrentPower.ShouldWaitBeforeUse)
                                            {
                                                runStatus = HandlerRunStatus.TreeRunning;
                                            }
                                            else if (IsWaitingForPower && !CurrentPower.ShouldWaitBeforeUse)
                                            {
                                                runStatus = HandlerRunStatus.TreeRunning;
                                                IsWaitingForPower = false;
                                            }
                                            else
                                            {
                                                IsWaitingForPower = false;
                                                HandleUnitInRange();
                                                runStatus = HandlerRunStatus.TreeRunning;
                                            }
                                        }
                                        //check if we are returning to the tree
                                        if (runStatus != HandlerRunStatus.NotFinished)
                                            return GetTreeSharpRunStatus(runStatus);
                                        break;
                                    }
                                // Item, interact with it and log item stats
                                case GObjectType.Item:
                                    {
                                        // Check if we actually have room for this item first
                                        Vector2 ValidLocation = FindValidBackpackLocation(true);
                                        if (ValidLocation.X < 0 || ValidLocation.Y < 0)
                                        {
                                            DbHelper.Log(TrinityLogLevel.Normal, LogCategory.UserInformation, "No more space to pickup a 2-slot item, town-run requested at next free moment.");
                                            ForceVendorRunASAP = true;
                                            //runStatus = HandlerRunStatus.TreeRunning;
                                            runStatus = HandlerRunStatus.TreeSuccess;
                                        }
                                        else
                                        {
                                            iInteractAttempts = HandleItemInRange();
                                            runStatus = HandlerRunStatus.TreeRunning;
                                        }
                                        //check if we are returning to the tree
                                        if (runStatus != HandlerRunStatus.NotFinished)
                                            return GetTreeSharpRunStatus(runStatus);
                                        break;
                                    }
                                // * Gold & Globe - need to get within pickup radius only
                                case GObjectType.Gold:
                                case GObjectType.Globe:
                                    {
                                        // Count how many times we've tried interacting
                                        if (!dictTotalInteractionAttempts.TryGetValue(CurrentTarget.RActorGuid, out iInteractAttempts))
                                        {
                                            dictTotalInteractionAttempts.Add(CurrentTarget.RActorGuid, 1);
                                        }
                                        else
                                        {
                                            dictTotalInteractionAttempts[CurrentTarget.RActorGuid]++;
                                        }
                                        // If we've tried interacting too many times, blacklist this for a while
                                        if (iInteractAttempts > 3)
                                        {
                                            hashRGUIDBlacklist90.Add(CurrentTarget.RActorGuid);
                                            //dateSinceBlacklist90Clear = DateTime.Now;
                                            hashRGUIDBlacklist60.Add(CurrentTarget.RActorGuid);
                                        }
                                        IgnoreRactorGUID = CurrentTarget.RActorGuid;
                                        IgnoreTargetForLoops = 3;

                                        // Now tell Trinity to get a new target!
                                        ForceTargetUpdate = true;
                                        runStatus = HandlerRunStatus.TreeRunning;

                                        //check if we are returning to the tree
                                        if (runStatus != HandlerRunStatus.NotFinished)
                                            return GetTreeSharpRunStatus(runStatus);
                                        break;
                                    }
                                // * Shrine & Container - need to get within 8 feet and interact
                                case GObjectType.Door:
                                case GObjectType.HealthWell:
                                case GObjectType.Shrine:
                                case GObjectType.Container:
                                case GObjectType.Interactable:
                                    {
                                        WaitWhileAnimating(5, true);
                                        ZetaDia.Me.UsePower(SNOPower.Axe_Operate_Gizmo, Vector3.Zero, 0, CurrentTarget.ACDGuid);
                                        //iIgnoreThisRactorGUID = CurrentTarget.iRActorGuid;
                                        //iIgnoreThisForLoops = 2;
                                        // Interactables can have a long channeling time...
                                        if (CurrentTarget.Type == GObjectType.Interactable)
                                            WaitWhileAnimating(1500, true);
                                        else
                                            WaitWhileAnimating(12, true);
                                        if (CurrentTarget.Type == GObjectType.Interactable)
                                        {
                                            IgnoreTargetForLoops = 30;
                                            hashRGUIDDestructible3SecBlacklist.Add(CurrentTarget.RActorGuid);
                                        }
                                        // Count how many times we've tried interacting
                                        if (!dictTotalInteractionAttempts.TryGetValue(CurrentTarget.RActorGuid, out iInteractAttempts))
                                        {
                                            dictTotalInteractionAttempts.Add(CurrentTarget.RActorGuid, 1);
                                        }
                                        else
                                        {
                                            dictTotalInteractionAttempts[CurrentTarget.RActorGuid]++;
                                        }
                                        // If we've tried interacting too many times, blacklist this for a while
                                        if ((iInteractAttempts > 5 || (CurrentTarget.Type == GObjectType.Interactable && iInteractAttempts > 3)) &&
                                            !(CurrentTarget.Type != GObjectType.HealthWell))
                                        {
                                            hashRGUIDBlacklist15.Add(CurrentTarget.RActorGuid);
                                            //dateSinceBlacklist90Clear = DateTime.Now;
                                        }

                                        // Now tell Trinity to get a new target!
                                        ForceTargetUpdate = true;

                                        runStatus = HandlerRunStatus.TreeRunning;

                                        //check if we are returning to the tree
                                        if (runStatus != HandlerRunStatus.NotFinished)
                                            return GetTreeSharpRunStatus(runStatus);
                                        break;
                                    }
                                // * Destructible - need to pick an ability and attack it
                                case GObjectType.Destructible:
                                case GObjectType.Barricade:
                                    {
                                        if (CurrentPower.SNOPower != SNOPower.None)
                                        {
                                            if (CurrentTarget.Type == GObjectType.Barricade)
                                            {
                                                DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.Behavior,
                                                    "Barricade: Name={0}. SNO={1}, Range={2}. Needed range={3}. Radius={4}. Type={5}. Using power={6}",
                                                    CurrentTarget.InternalName,     // 0
                                                    CurrentTarget.ActorSNO,         // 1
                                                    CurrentTarget.CentreDistance,   // 2
                                                    TargetRangeRequired,                 // 3
                                                    CurrentTarget.Radius,           // 4
                                                    CurrentTarget.Type,             // 5
                                                    CurrentPower.SNOPower           // 6
                                                    );
                                            }
                                            else
                                            {
                                                DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.Behavior,
                                                    "Destructible: Name={0}. SNO={1}, Range={2}. Needed range={3}. Radius={4}. Type={5}. Using power={6}",
                                                    CurrentTarget.InternalName,     // 0
                                                    CurrentTarget.ActorSNO,         // 1
                                                    CurrentTarget.CentreDistance,   // 2
                                                    TargetRangeRequired,                 // 3 
                                                    CurrentTarget.Radius,           // 4
                                                    CurrentTarget.Type,             // 5
                                                    CurrentPower.SNOPower           // 6
                                                    );
                                            }

                                            WaitWhileAnimating(12, true);

                                            if (CurrentTarget.RActorGuid == IgnoreRactorGUID || hashDestructableLocationTarget.Contains(CurrentTarget.ActorSNO))
                                            {
                                                // Location attack - attack the Vector3/map-area (equivalent of holding shift and left-clicking the object in-game to "force-attack")
                                                Vector3 vAttackPoint;
                                                if (CurrentTarget.CentreDistance >= 6f)
                                                    vAttackPoint = MathEx.CalculatePointFrom(CurrentTarget.Position, PlayerStatus.CurrentPosition, 6f);
                                                else
                                                    vAttackPoint = CurrentTarget.Position;

                                                vAttackPoint.Z += 1.5f;
                                                DbHelper.Log(TrinityLogLevel.Debug, LogCategory.Behavior, "(NB: Attacking location of destructable)");
                                                ZetaDia.Me.UsePower(CurrentPower.SNOPower, vAttackPoint, CurrentWorldDynamicId, -1);
                                                if (CurrentPower.SNOPower == SNOPower.Monk_TempestRush)
                                                    LastTempestRushLocation = vAttackPoint;
                                            }
                                            else
                                            {
                                                // Standard attack - attack the ACDGUID (equivalent of left-clicking the object in-game)
                                                ZetaDia.Me.UsePower(CurrentPower.SNOPower, vNullLocation, -1, CurrentTarget.ACDGuid);
                                                if (CurrentPower.SNOPower == SNOPower.Monk_TempestRush)
                                                    LastTempestRushLocation = CurrentTarget.Position;
                                            }
                                            // Count how many times we've tried interacting
                                            if (!dictTotalInteractionAttempts.TryGetValue(CurrentTarget.RActorGuid, out iInteractAttempts))
                                            {
                                                dictTotalInteractionAttempts.Add(CurrentTarget.RActorGuid, 1);
                                            }
                                            else
                                            {
                                                dictTotalInteractionAttempts[CurrentTarget.RActorGuid]++;
                                            }

                                            dictAbilityLastUse[CurrentPower.SNOPower] = DateTime.Now;
                                            //CurrentPower.SNOPower = SNOPower.None;
                                            WaitWhileAnimating(6, true);
                                            // Prevent this EXACT object being targetted again for a short while, just incase
                                            IgnoreRactorGUID = CurrentTarget.RActorGuid;
                                            IgnoreTargetForLoops = 3;
                                            // Add this destructible/barricade to our very short-term ignore list
                                            hashRGUIDDestructible3SecBlacklist.Add(CurrentTarget.RActorGuid);
                                            DbHelper.Log(TrinityLogLevel.Debug, LogCategory.Behavior, "Blacklisting {0} {1} {2} for 3 seconds for Destrucable attack", CurrentTarget.Type, CurrentTarget.InternalName, CurrentTarget.ActorSNO);
                                            lastDestroyedDestructible = DateTime.Now;
                                            bNeedClearDestructibles = true;
                                        }
                                        // Now tell Trinity to get a new target!
                                        ForceTargetUpdate = true;
                                    }
                                    runStatus = HandlerRunStatus.TreeRunning;
                                    //check if we are returning to the tree
                                    if (runStatus != HandlerRunStatus.NotFinished)
                                        return GetTreeSharpRunStatus(runStatus);
                                    break;
                                // * Backtrack - clear this waypoint
                                case GObjectType.Backtrack:
                                    // Remove the current backtrack location now we reached it
                                    vBacktrackList.Remove(iTotalBacktracks);
                                    iTotalBacktracks--;
                                    // Never bother with the very first backtrack location
                                    if (iTotalBacktracks <= 1)
                                    {
                                        iTotalBacktracks = 0;
                                        vBacktrackList = new SortedList<int, Vector3>();
                                    }
                                    ForceTargetUpdate = true;
                                    runStatus = HandlerRunStatus.TreeRunning;
                                    //check if we are returning to the tree
                                    if (runStatus != HandlerRunStatus.NotFinished)
                                        return GetTreeSharpRunStatus(runStatus);
                                    break;
                            }
                            runStatus = HandlerRunStatus.TreeRunning;
                            //check if we are returning to the tree
                            if (runStatus != HandlerRunStatus.NotFinished)
                                return GetTreeSharpRunStatus(runStatus);
                        }
                    }
                    using (new PerformanceLogger("HandleTarget.UpdateStatusText"))
                    {
                        // Out-of-range, so move towards the target
                        UpdateStatusTextTarget(false);
                    }

                    // Are we currently incapacitated? If so then wait...
                    if (PlayerStatus.IsIncapacitated || PlayerStatus.IsRooted)
                    {
                        runStatus = HandlerRunStatus.TreeFailure;
                        DbHelper.Log(LogCategory.Behavior, "Player is rooted or incapacitated!");
                        return GetTreeSharpRunStatus(runStatus);
                    }

                    // Check to see if we're stuck in moving to the target
                    runStatus = HandleTargetDistanceCheck(runStatus);

                    //check if we are returning to the tree
                    if (runStatus != HandlerRunStatus.NotFinished)
                        return GetTreeSharpRunStatus(runStatus);


                    // Update the last distance stored
                    fLastDistanceFromTarget = TargetCurrentDistance;
                    using (new PerformanceLogger("HandleTarget.PositionShift"))
                    {

                        // See if there's an obstacle in our way, if so try to navigate around it
                        if (CurrentTarget.Type != GObjectType.Avoidance)
                        {
                            Vector3 point = vCurrentDestination;
                            foreach (GilesObstacle tempobstacle in GilesTrinity.hashNavigationObstacleCache.Where(cp =>
                                            MathUtil.IntersectsPath(cp.Location, cp.Radius, PlayerStatus.CurrentPosition, point) &&
                                            cp.Location.Distance2D(PlayerStatus.CurrentPosition) > PlayerMover.GetObstacleNavigationSize(cp)))
                            {
                                if (vShiftedPosition == Vector3.Zero)
                                {
                                    if (DateTime.Now.Subtract(lastShiftedPosition).TotalSeconds >= 10)
                                    {
                                        float fDirectionToTarget = MathUtil.FindDirectionDegree(PlayerStatus.CurrentPosition, vCurrentDestination);
                                        vCurrentDestination = MathEx.GetPointAt(PlayerStatus.CurrentPosition, 15f, MathEx.ToRadians(fDirectionToTarget - 50));
                                        if (!NavHelper.CanRayCast(PlayerStatus.CurrentPosition, vCurrentDestination))
                                        {
                                            vCurrentDestination = MathEx.GetPointAt(PlayerStatus.CurrentPosition, 15f, MathEx.ToRadians(fDirectionToTarget + 50));
                                            if (!NavHelper.CanRayCast(PlayerStatus.CurrentPosition, vCurrentDestination))
                                            {
                                                vCurrentDestination = point;
                                            }
                                        }
                                        if (vCurrentDestination != point)
                                        {
                                            vShiftedPosition = vCurrentDestination;
                                            iShiftPositionFor = 1000;
                                            lastShiftedPosition = DateTime.Now;
                                            DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.Behavior, "Mid-Target-Handle position shift location to: {0} (was {1})", vCurrentDestination, point);
                                        }
                                    }
                                    // Make sure we only shift max once every 10 seconds
                                }
                                else
                                {
                                    if (DateTime.Now.Subtract(lastShiftedPosition).TotalMilliseconds <= iShiftPositionFor)
                                    {
                                        vCurrentDestination = vShiftedPosition;
                                    }
                                    else
                                    {
                                        vShiftedPosition = Vector3.Zero;
                                    }
                                }
                            }
                            // Position shifting code
                        }
                    }

                    if (TimeSinceUse(SNOPower.Monk_TempestRush) < 250)
                    {
                        bForceNewMovement = true;
                    }

                    // Only position-shift when not avoiding
                    // See if we want to ACTUALLY move, or are just waiting for the last move command...
                    if (!bForceNewMovement && IsAlreadyMoving && vCurrentDestination == vLastMoveToTarget && DateTime.Now.Subtract(lastMovementCommand).TotalMilliseconds <= 100)
                    {
                        runStatus = HandlerRunStatus.TreeRunning;
                        //check if we are returning to the tree
                        if (runStatus != HandlerRunStatus.NotFinished)
                            return GetTreeSharpRunStatus(runStatus);
                    }
                    using (new PerformanceLogger("HandleTarget.SpecialMovement"))
                    {

                        bool Monk_SpecialMovement = ((CurrentTarget.Type == GObjectType.Gold ||
                            CurrentTarget.Type == GObjectType.Unit ||
                            CurrentTarget.Type == GObjectType.Barricade ||
                            CurrentTarget.Type == GObjectType.Destructible) && (Monk_TempestRushReady()));

                        bool Barbarian_SpecialMovement = ((CurrentTarget.Type == GObjectType.Avoidance &&
                            GilesObjectCache.Any(u => (u.Type == GObjectType.Unit || u.Type == GObjectType.Destructible || u.Type == GObjectType.Barricade) &&
                                MathUtil.IntersectsPath(u.Position, u.Radius, PlayerStatus.CurrentPosition, CurrentTarget.Position))) ||
                                CurrentTarget.Type == GObjectType.Globe);

                        // If we're doing avoidance, globes or backtracking, try to use special abilities to move quicker
                        if ((CurrentTarget.Type == GObjectType.Avoidance ||
                            CurrentTarget.Type == GObjectType.Globe ||
                            Monk_SpecialMovement ||
                            (CurrentTarget.Type == GObjectType.Backtrack && Settings.Combat.Misc.AllowOOCMovement))
                            && NavHelper.CanRayCast(PlayerStatus.CurrentPosition, vCurrentDestination)
                            )
                        {
                            bool bFoundSpecialMovement = UsedSpecialMovement();

                            if (CurrentTarget.Type != GObjectType.Backtrack)
                            {
                                // Whirlwind for a barb

                                if (Barbarian_SpecialMovement && !IsWaitingForSpecial && CurrentPower.SNOPower != SNOPower.Barbarian_WrathOfTheBerserker && !bFoundSpecialMovement && Hotbar.Contains(SNOPower.Barbarian_Whirlwind) && PlayerStatus.PrimaryResource >= 10)
                                {
                                    ZetaDia.Me.UsePower(SNOPower.Barbarian_Whirlwind, vCurrentDestination, CurrentWorldDynamicId, -1);
                                    // Store the current destination for comparison incase of changes next loop
                                    vLastMoveToTarget = vCurrentDestination;
                                    // Reset total body-block count, since we should have moved
                                    if (DateTime.Now.Subtract(lastForcedKeepCloseRange).TotalMilliseconds >= 2000)
                                        TimesBlockedMoving = 0;
                                    runStatus = HandlerRunStatus.TreeRunning;
                                    //check if we are returning to the tree
                                    if (runStatus != HandlerRunStatus.NotFinished)
                                        return GetTreeSharpRunStatus(runStatus);
                                }
                                // Tempest rush for a monk
                                if (!bFoundSpecialMovement && Hotbar.Contains(SNOPower.Monk_TempestRush) && PlayerStatus.PrimaryResource >= Settings.Combat.Monk.TR_MinSpirit &&
                                    ((CurrentTarget.Type == GObjectType.Item && CurrentTarget.CentreDistance > 20f) || CurrentTarget.Type != GObjectType.Item) &&
                                    Settings.Combat.Monk.TROption != TempestRushOption.MovementOnly &&
                                    Monk_TempestRushReady())
                                {
                                    ZetaDia.Me.UsePower(SNOPower.Monk_TempestRush, vCurrentDestination, CurrentWorldDynamicId, -1);
                                    dictAbilityLastUse[SNOPower.Monk_TempestRush] = DateTime.Now;
                                    LastPowerUsed = SNOPower.Monk_TempestRush;
                                    LastTempestRushLocation = vCurrentDestination;
                                    // Store the current destination for comparison incase of changes next loop
                                    vLastMoveToTarget = vCurrentDestination;
                                    // Reset total body-block count, since we should have moved
                                    if (DateTime.Now.Subtract(lastForcedKeepCloseRange).TotalMilliseconds >= 2000)
                                        TimesBlockedMoving = 0;
                                    runStatus = HandlerRunStatus.TreeRunning;
                                    //check if we are returning to the tree
                                    if (runStatus != HandlerRunStatus.NotFinished)
                                        return GetTreeSharpRunStatus(runStatus);
                                }
                                // Strafe for a Demon Hunter
                                if (!bFoundSpecialMovement && Hotbar.Contains(SNOPower.DemonHunter_Strafe) && PlayerStatus.PrimaryResource >= 15)
                                {
                                    ZetaDia.Me.UsePower(SNOPower.DemonHunter_Strafe, vCurrentDestination, CurrentWorldDynamicId, -1);
                                    // Store the current destination for comparison incase of changes next loop
                                    vLastMoveToTarget = vCurrentDestination;
                                    // Reset total body-block count, since we should have moved
                                    if (DateTime.Now.Subtract(lastForcedKeepCloseRange).TotalMilliseconds >= 2000)
                                        TimesBlockedMoving = 0;
                                    runStatus = HandlerRunStatus.TreeRunning;
                                    //check if we are returning to the tree
                                    if (runStatus != HandlerRunStatus.NotFinished)
                                        return GetTreeSharpRunStatus(runStatus);
                                }
                            }
                            if (bFoundSpecialMovement)
                            {
                                WaitWhileAnimating(6, true);
                                // Store the current destination for comparison incase of changes next loop
                                vLastMoveToTarget = vCurrentDestination;
                                // Reset total body-block count, since we should have moved
                                if (DateTime.Now.Subtract(lastForcedKeepCloseRange).TotalMilliseconds >= 2000)
                                    TimesBlockedMoving = 0;
                                runStatus = HandlerRunStatus.TreeRunning;
                                //check if we are returning to the tree
                                if (runStatus != HandlerRunStatus.NotFinished)
                                    return GetTreeSharpRunStatus(runStatus);
                            }
                        }
                    }

                    // Whirlwind against everything within range (except backtrack points)

                    if (Hotbar.Contains(SNOPower.Barbarian_Whirlwind) && PlayerStatus.PrimaryResource >= 10 && AnythingWithinRange[RANGE_20] >= 1 && !IsWaitingForSpecial && CurrentPower.SNOPower != SNOPower.Barbarian_WrathOfTheBerserker && TargetCurrentDistance <= 12f && CurrentTarget.Type != GObjectType.Container && CurrentTarget.Type != GObjectType.Backtrack &&
                        (!Hotbar.Contains(SNOPower.Barbarian_Sprint) || GetHasBuff(SNOPower.Barbarian_Sprint)) &&
                        CurrentTarget.Type != GObjectType.Backtrack &&
                        (CurrentTarget.Type != GObjectType.Item && CurrentTarget.Type != GObjectType.Gold && TargetCurrentDistance >= 6f) &&
                        (CurrentTarget.Type != GObjectType.Unit ||
                        (CurrentTarget.Type == GObjectType.Unit && !CurrentTarget.IsTreasureGoblin &&
                            (!Settings.Combat.Barbarian.SelectiveWhirlwind || bAnyNonWWIgnoreMobsInRange || !hashActorSNOWhirlwindIgnore.Contains(CurrentTarget.ActorSNO)))))
                    {
                        // Special code to prevent whirlwind double-spam, this helps save fury
                        bool bUseThisLoop = SNOPower.Barbarian_Whirlwind != LastPowerUsed;
                        if (!bUseThisLoop)
                        {
                            LastPowerUsed = SNOPower.None;
                            if (DateTime.Now.Subtract(dictAbilityLastUse[SNOPower.Barbarian_Whirlwind]).TotalMilliseconds >= 200)
                                bUseThisLoop = true;
                        }
                        if (bUseThisLoop)
                        {
                            ZetaDia.Me.UsePower(SNOPower.Barbarian_Whirlwind, vCurrentDestination, CurrentWorldDynamicId, -1);
                            LastPowerUsed = SNOPower.Barbarian_Whirlwind;
                            dictAbilityLastUse[SNOPower.Barbarian_Whirlwind] = DateTime.Now;
                        }
                        // Store the current destination for comparison incase of changes next loop
                        vLastMoveToTarget = vCurrentDestination;
                        // Reset total body-block count
                        if ((!ForceCloseRangeTarget || DateTime.Now.Subtract(lastForcedKeepCloseRange).TotalMilliseconds > ForceCloseRangeForMilliseconds) &&
                            DateTime.Now.Subtract(lastForcedKeepCloseRange).TotalMilliseconds >= 2000)
                            TimesBlockedMoving = 0;
                        runStatus = HandlerRunStatus.TreeRunning;
                        //check if we are returning to the tree
                        if (runStatus != HandlerRunStatus.NotFinished)
                            return GetTreeSharpRunStatus(runStatus);
                    }
                }
                catch (Exception ex)
                {
                    DbHelper.Log(TrinityLogLevel.Debug, LogCategory.Behavior, "{0}", ex);
                    runStatus = HandlerRunStatus.TreeFailure;
                    return GetTreeSharpRunStatus(runStatus);
                }

                DbHelper.Log(LogCategory.Behavior, "Using Navigator to reach target");
                HandleTargetBasicMovement(bForceNewMovement);

                runStatus = HandlerRunStatus.TreeRunning;
                return GetTreeSharpRunStatus(runStatus);
            }
        }

        private static HandlerRunStatus HandleTargetDistanceCheck(HandlerRunStatus runStatus)
        {
            using (new PerformanceLogger("HandleTarget.DistanceEqualCheck"))
            {
                // Count how long we have failed to move - body block stuff etc.
                if (Math.Abs(TargetCurrentDistance - fLastDistanceFromTarget) < 5f && PlayerMover.GetMovementSpeed() < 1)
                {
                    bForceNewMovement = true;
                    if (DateTime.Now.Subtract(lastMovedDuringCombat).TotalMilliseconds >= 500)
                    {
                        lastMovedDuringCombat = DateTime.Now;
                        // We've been stuck at least 250 ms, let's go and pick new targets etc.
                        TimesBlockedMoving++;
                        ForceCloseRangeTarget = true;
                        lastForcedKeepCloseRange = DateTime.Now;
                        // And tell Trinity to get a new target
                        ForceTargetUpdate = true;

                        // Handle body blocking by blacklisting
                        //GilesHandleBodyBlocking();

                        // If we were backtracking and failed, remove the current backtrack and try and move to the next
                        if (CurrentTarget.Type == GObjectType.Backtrack && TimesBlockedMoving >= 2)
                        {
                            vBacktrackList.Remove(iTotalBacktracks);
                            iTotalBacktracks--;
                            if (iTotalBacktracks <= 1)
                            {
                                iTotalBacktracks = 0;
                                vBacktrackList = new SortedList<int, Vector3>();
                            }
                        }
                        // Reset the emergency loop counter and return success
                        runStatus = HandlerRunStatus.TreeRunning;
                    }
                    // Been 250 milliseconds of non-movement?
                }
                else
                {
                    // Movement has been made, so count the time last moved!
                    lastMovedDuringCombat = DateTime.Now;
                }
            }
            return runStatus;
        }

        private static Stack<KeyValuePair<int, DateTime>> BlackListStack = new Stack<KeyValuePair<int, DateTime>>(20);

        /// <summary>
        /// Handles target blacklist assignment if necessary, used for all targets (units/gold/items/interactables)
        /// </summary>
        /// <param name="runStatus"></param>
        /// <returns></returns>
        private static HandlerRunStatus HandleTargetTimeout(HandlerRunStatus runStatus)
        {
            using (new PerformanceLogger("HandleTarget.TargetTimeout"))
            {
                // Been trying to handle the same target for more than 30 seconds without damaging/reaching it? Blacklist it!
                // Note: The time since target picked updates every time the current target loses health, if it's a monster-target
                // Don't blacklist stuff if we're playing a cutscene

                bool shouldTryBlacklist = false;

                // don't timeout on avoidance
                if (CurrentTarget.Type == GObjectType.Avoidance)
                    return HandlerRunStatus.NotFinished;

                // don't timeout on legendary items
                if (CurrentTarget.Type == GObjectType.Item && CurrentTarget.ItemQuality >= ItemQuality.Legendary)
                    return HandlerRunStatus.NotFinished;

                // don't timeout if we're actively moving
                if (PlayerMover.GetMovementSpeed() >= 1)
                    return HandlerRunStatus.NotFinished;

                if (CurrentTargetIsNonUnit() && GetSecondsSinceTargetUpdate() > 6)
                    shouldTryBlacklist = true;

                if ((CurrentTargetIsUnit() && CurrentTarget.IsBoss && GetSecondsSinceTargetUpdate() > 45))
                    shouldTryBlacklist = true;

                // special raycast check for current target after 10 sec
                if ((CurrentTargetIsUnit() && !CurrentTarget.IsBoss && GetSecondsSinceTargetUpdate() > 10))
                    shouldTryBlacklist = true;

                if (shouldTryBlacklist)
                {
                    // NOTE: This only blacklists if it's remained the PRIMARY TARGET that we are trying to actually directly attack!
                    // So it won't blacklist a monster "on the edge of the screen" who isn't even being targetted
                    // Don't blacklist monsters on <= 50% health though, as they can't be in a stuck location... can they!? Maybe give them some extra time!

                    bool isNavigable = NavHelper.CanRayCast(PlayerStatus.CurrentPosition, vCurrentDestination);

                    bool addTargetToBlacklist = true;

                    // PREVENT blacklisting a monster on less than 90% health unless we haven't damaged it for more than 2 minutes
                    if (CurrentTarget.Type == GObjectType.Unit && isNavigable && CurrentTarget.IsTreasureGoblin && Settings.Combat.Misc.GoblinPriority >= GoblinPriority.Kamikaze)
                    {
                        addTargetToBlacklist = false;
                    }

                    // Check if we can Raycast to a trash mob
                    //if (CurrentTarget.IsTrashMob &&
                    //    GetSecondsSinceTargetUpdate() > 4 &&
                    //    CurrentTarget.HitPoints > 0.90)
                    //{
                    //    if (GilesNavHelper.CanRayCast(PlayerStatus.CurrentPosition, CurrentTarget.Position))
                    //    {
                    //        addTargetToBlacklist = false;
                    //    }
                    //}

                    if (addTargetToBlacklist)
                    {
                        if (CurrentTarget.Type == GObjectType.Unit)
                        {
                            DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.Behavior,
                                "Blacklisting a monster because of possible stuck issues. Monster={0} [{1}] Range={2:0} health %={3:0} RActorGUID={4}",
                                CurrentTarget.InternalName,         // 0
                                CurrentTarget.ActorSNO,             // 1
                                CurrentTarget.CentreDistance,       // 2
                                CurrentTarget.HitPointsPct,            // 3
                                CurrentTarget.RActorGuid            // 4
                                );
                        }
                        else
                        {
                            DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.Behavior,
                                "Blacklisting an object because of possible stuck issues. Object={0} [{1}]. Range={2:0} RActorGUID={3}",
                                CurrentTarget.InternalName,         // 0
                                CurrentTarget.ActorSNO,             // 1 
                                CurrentTarget.CentreDistance,       // 2
                                CurrentTarget.RActorGuid            // 3
                                );
                        }

                        if (CurrentTarget.IsBoss)
                        {
                            hashRGUIDBlacklist15.Add(CurrentTarget.RActorGuid);
                            dateSinceBlacklist15Clear = DateTime.Now;
                            CurrentTarget = null;
                            runStatus = HandlerRunStatus.TreeSuccess;
                        }
                        else if (CurrentTarget.Type == GObjectType.Item && CurrentTarget.ItemQuality >= ItemQuality.Legendary)
                        {
                            // Don't blacklist legendaries!!
                            runStatus = HandlerRunStatus.TreeSuccess;
                        }
                        else
                        {
                            hashRGUIDBlacklist90.Add(CurrentTarget.RActorGuid);
                            dateSinceBlacklist90Clear = DateTime.Now;
                            CurrentTarget = null;
                            runStatus = HandlerRunStatus.TreeSuccess;
                        }
                    }
                }
                return runStatus;
            }
        }

        /// <summary>
        /// Checks to see if we need a new monster power and will assign it to <see cref="CurrentPower"/>, distinguishes destructables/barricades from units
        /// </summary>
        private static void AssignMonsterTargetPower()
        {
            using (new PerformanceLogger("HandleTarget.AssignMonsterTargetPower"))
            {
                // Find a valid ability if the target is a monster
                if (ShouldPickNewAbilities && !IsWaitingForPower && !IsWaitingForPotion)
                {
                    ShouldPickNewAbilities = false;
                    if (CurrentTarget.Type == GObjectType.Unit)
                    {
                        // Pick a suitable ability
                        CurrentPower = AbilitySelector(false, false, false);
                        if (CurrentPower.SNOPower == SNOPower.None && !PlayerStatus.IsIncapacitated)
                        {
                            iNoAbilitiesAvailableInARow++;
                            if (DateTime.Now.Subtract(lastRemindedAboutAbilities).TotalSeconds > 60 && iNoAbilitiesAvailableInARow >= 4)
                            {
                                lastRemindedAboutAbilities = DateTime.Now;
                                DbHelper.Log(TrinityLogLevel.Error, LogCategory.Behavior, "Fatal Error: Couldn't find a valid attack ability. Not enough resource for any abilities or all on cooldown");
                                DbHelper.Log(TrinityLogLevel.Error, LogCategory.Behavior, "If you get this message frequently, you should consider changing your build");
                                DbHelper.Log(TrinityLogLevel.Error, LogCategory.Behavior, "Perhaps you don't have enough critical hit chance % for your current build, or just have a bad skill setup?");
                            }
                        }
                        else
                        {
                            iNoAbilitiesAvailableInARow = 0;
                        }
                    }
                    // Select an ability for destroying a destructible with in advance
                    if (CurrentTarget.Type == GObjectType.Destructible || CurrentTarget.Type == GObjectType.Barricade)
                        CurrentPower = AbilitySelector(false, false, true);
                }
            }
        }

        /// <summary>
        /// Will check <see cref=" IsWaitingForPotion"/> and Use a Potion if needed
        /// </summary>
        private static void UseHealthPotionIfNeeded()
        {
            using (new PerformanceLogger("HandleTarget.UseHealthPotionIfNeeded"))
            {
                if (IsWaitingForPotion)
                {
                    IsWaitingForPotion = false;
                    if (!PlayerStatus.IsIncapacitated && GilesUseTimer(SNOPower.DrinkHealthPotion))
                    {
                        ACDItem thisBestPotion = ZetaDia.Me.Inventory.Backpack.Where(i => i.IsPotion).OrderByDescending(p => p.HitpointsGranted).ThenBy(p => p.ItemStackQuantity).FirstOrDefault();
                        if (thisBestPotion != null)
                        {
                            WaitWhileAnimating(3, true);
                            ZetaDia.Me.Inventory.UseItem((thisBestPotion.DynamicId));
                        }
                        dictAbilityLastUse[SNOPower.DrinkHealthPotion] = DateTime.Now;
                        WaitWhileAnimating(2, true);
                    }
                }
            }
        }

        /// <summary>
        /// Determines if we need a cache refresh, or just a current target health check
        /// </summary>
        private static void CheckStaleCache()
        {
            using (new PerformanceLogger("HandleTarget.CheckStaleCache"))
            {
                // Let's calculate whether or not we want a new target list...
                if (!IsWholeNewTarget && !IsWaitingForPower && !IsWaitingForPotion)
                {
                    // Update targets at least once every 80 milliseconds
                    if (ForceTargetUpdate || IsAvoidingProjectiles || DateTime.Now.Subtract(LastRefreshedCache).TotalMilliseconds > Settings.Advanced.CacheRefreshRate)
                    {
                        StaleCache = true;
                    }
                    // If we AREN'T getting new targets - find out if we SHOULD because the current unit has died etc.
                    if (!StaleCache && CurrentTarget.Type == GObjectType.Unit)
                    {
                        if (CurrentTarget.Unit == null || CurrentTarget.Unit.BaseAddress == IntPtr.Zero)
                        {
                            StaleCache = true;
                        }
                        else
                        {
                            // health calculations
                            double dThisMaxHealth;
                            // Get the max health of this unit, a cached version if available, if not cache it
                            if (!dictGilesMaxHealthCache.TryGetValue(c_RActorGuid, out dThisMaxHealth))
                            {
                                try
                                {

                                    dThisMaxHealth = CurrentTarget.Unit.HitpointsMax;
                                    dictGilesMaxHealthCache.Add(c_RActorGuid, CurrentTarget.Unit.HitpointsMax);
                                }
                                catch
                                {
                                    DbHelper.Log(TrinityLogLevel.Debug, LogCategory.Behavior, "Safely handled exception getting attribute max health #2 for unit {0} [{1}]", c_InternalName, c_ActorSNO);
                                    StaleCache = true;
                                }
                            }
                            // Ok check we didn't fail getting the maximum health, now try to get live current health...
                            if (!StaleCache)
                            {
                                try
                                {
                                    double dTempHitpoints = CurrentTarget.Unit.HitpointsCurrent / dThisMaxHealth;
                                    if (dTempHitpoints <= 0d)
                                    {
                                        StaleCache = true;
                                    }
                                    else
                                    {
                                        CurrentTarget.HitPointsPct = dTempHitpoints;
                                        CurrentTarget.Position = CurrentTarget.Unit.Position;
                                    }
                                }
                                catch
                                {
                                    StaleCache = true;
                                }
                            }

                            // force update of cached player data
                            GilesPlayerCache.UpdateCachedPlayerData();
                        }
                    }
                }
            }
        }

        /// <summary>
        /// If we can use special class movement abilities, this will use it and return true
        /// </summary>
        /// <returns></returns>
        private static bool UsedSpecialMovement()
        {
            using (new PerformanceLogger("HandleTarget.UsedSpecialMovement"))
            {
                // Log whether we used a  (for avoidance really)
                bool bFoundSpecialMovement = false;
                // Leap movement for a barb
                if (!bFoundSpecialMovement && Hotbar.Contains(SNOPower.Barbarian_Leap) &&
                    DateTime.Now.Subtract(dictAbilityLastUse[SNOPower.Barbarian_Leap]).TotalMilliseconds >= dictAbilityRepeatDelay[SNOPower.Barbarian_Leap] &&
                    PowerManager.CanCast(SNOPower.Barbarian_Leap))
                {
                    WaitWhileAnimating(3, true);
                    ZetaDia.Me.UsePower(SNOPower.Barbarian_Leap, vCurrentDestination, CurrentWorldDynamicId, -1);
                    dictAbilityLastUse[SNOPower.Barbarian_Leap] = DateTime.Now;
                    bFoundSpecialMovement = true;
                }
                // Furious Charge movement for a barb
                if (!bFoundSpecialMovement && Hotbar.Contains(SNOPower.Barbarian_FuriousCharge) &&
                    DateTime.Now.Subtract(dictAbilityLastUse[SNOPower.Barbarian_FuriousCharge]).TotalMilliseconds >= dictAbilityRepeatDelay[SNOPower.Barbarian_FuriousCharge] &&
                    PowerManager.CanCast(SNOPower.Barbarian_FuriousCharge))
                {
                    WaitWhileAnimating(3, true);
                    ZetaDia.Me.UsePower(SNOPower.Barbarian_FuriousCharge, vCurrentDestination, CurrentWorldDynamicId, -1);
                    dictAbilityLastUse[SNOPower.Barbarian_FuriousCharge] = DateTime.Now;
                    bFoundSpecialMovement = true;
                }
                // Vault for a Demon Hunter
                if (!bFoundSpecialMovement && Hotbar.Contains(SNOPower.DemonHunter_Vault) &&
                    //DateTime.Now.Subtract(dictAbilityLastUse[SNOPower.DemonHunter_Vault]).TotalMilliseconds >= dictAbilityRepeatDelay[SNOPower.DemonHunter_Vault] &&
                    DateTime.Now.Subtract(GilesTrinity.dictAbilityLastUse[SNOPower.DemonHunter_Vault]).TotalMilliseconds >= GilesTrinity.Settings.Combat.DemonHunter.VaultMovementDelay &&
                    PowerManager.CanCast(SNOPower.DemonHunter_Vault) &&
                    (PlayerKiteDistance <= 0 || (!hashMonsterObstacleCache.Any(a => a.Location.Distance(vCurrentDestination) <= PlayerKiteDistance) &&
                    !hashAvoidanceObstacleCache.Any(a => a.Location.Distance(vCurrentDestination) <= PlayerKiteDistance))) &&
                    (!GilesTrinity.hashAvoidanceObstacleCache.Any(a => MathEx.IntersectsPath(a.Location, a.Radius, GilesTrinity.PlayerStatus.CurrentPosition, vCurrentDestination)))
                    )
                {
                    WaitWhileAnimating(3, true);
                    ZetaDia.Me.UsePower(SNOPower.DemonHunter_Vault, vCurrentDestination, CurrentWorldDynamicId, -1);
                    dictAbilityLastUse[SNOPower.DemonHunter_Vault] = DateTime.Now;
                    bFoundSpecialMovement = true;
                }
                // Teleport for a wizard (need to be able to check skill rune in DB for a 3-4 teleport spam in a row)
                if (!bFoundSpecialMovement && Hotbar.Contains(SNOPower.Wizard_Teleport) &&
                    DateTime.Now.Subtract(dictAbilityLastUse[SNOPower.Wizard_Teleport]).TotalMilliseconds >= dictAbilityRepeatDelay[SNOPower.Wizard_Teleport] &&
                    PlayerStatus.PrimaryResource >= 15 &&
                    PowerManager.CanCast(SNOPower.Wizard_Teleport))
                {
                    WaitWhileAnimating(3, true);
                    ZetaDia.Me.UsePower(SNOPower.Wizard_Teleport, vCurrentDestination, CurrentWorldDynamicId, -1);
                    dictAbilityLastUse[SNOPower.Wizard_Teleport] = DateTime.Now;
                    bFoundSpecialMovement = true;
                }
                // Archon Teleport for a wizard (need to be able to check skill rune in DB for a 3-4 teleport spam in a row)
                if (!bFoundSpecialMovement && Hotbar.Contains(SNOPower.Wizard_Archon_Teleport) &&
                    DateTime.Now.Subtract(dictAbilityLastUse[SNOPower.Wizard_Archon_Teleport]).TotalMilliseconds >= dictAbilityRepeatDelay[SNOPower.Wizard_Archon_Teleport] &&
                    PowerManager.CanCast(SNOPower.Wizard_Archon_Teleport))
                {
                    WaitWhileAnimating(3, true);
                    ZetaDia.Me.UsePower(SNOPower.Wizard_Archon_Teleport, vCurrentDestination, CurrentWorldDynamicId, -1);
                    dictAbilityLastUse[SNOPower.Wizard_Archon_Teleport] = DateTime.Now;
                    bFoundSpecialMovement = true;
                }
                return bFoundSpecialMovement;
            }
        }

        private static bool CurrentTargetIsNotAvoidance()
        {
            return CurrentTarget.Type != GObjectType.Avoidance;
        }

        private static bool CurrentTargetIsNonUnit()
        {
            return CurrentTarget.Type != GObjectType.Unit;
        }

        private static bool CurrentTargetIsUnit()
        {
            return CurrentTarget.Type == GObjectType.Unit;
        }

        /// <summary>
        /// Returns the number of seconds since our current target was updated
        /// </summary>
        /// <returns></returns>
        private static double GetSecondsSinceTargetUpdate()
        {
            return DateTime.Now.Subtract(dateSincePickedTarget).TotalSeconds;
        }

        /// <summary>
        /// Will identify if we have been body blocked from moving during any movement, and add avoidance if needed. Can cancel movement altogether to clear nearby mobs.
        /// </summary>
        private static void GilesHandleBodyBlocking()
        {
            using (new PerformanceLogger("HandleTarget.HandleBodyBlocking"))
            {
                // Tell target finder to prioritize close-combat targets incase we were bodyblocked
                switch (TimesBlockedMoving)
                {
                    case 0:
                    case 1:
                        ForceCloseRangeForMilliseconds = 850;
                        break;
                    case 2:
                        ForceCloseRangeForMilliseconds = 1300;
                        // Cancel avoidance attempts for 500ms
                        cancelledEmergencyMoveForMilliseconds = 1500;
                        DbHelper.Log(LogCategory.Movement, "Canceling emergency movement for {0} ms", cancelledEmergencyMoveForMilliseconds);
                        timeCancelledEmergencyMove = DateTime.Now;

                        // Check for raycastability against objects
                        switch (CurrentTarget.Type)
                        {
                            case GObjectType.Container:
                            case GObjectType.Shrine:
                            case GObjectType.Globe:
                            case GObjectType.Gold:
                            case GObjectType.Item:
                                // No raycast available, try and force-ignore this for a little while, and blacklist for a few seconds
                                if (!NavHelper.CanRayCast(PlayerStatus.CurrentPosition, CurrentTarget.Position) && CurrentTarget.Unit.InLineOfSight)
                                {
                                    IgnoreRactorGUID = CurrentTarget.RActorGuid;
                                    IgnoreTargetForLoops = 6;
                                    DbHelper.Log(TrinityLogLevel.Debug, LogCategory.Behavior, "Blacklisting {0} {1} {2} for 60 seconds due to BodyBlocking", CurrentTarget.Type, CurrentTarget.InternalName, CurrentTarget.ActorSNO);
                                    if (!(CurrentTarget.Type == GObjectType.Item && CurrentTarget.ItemQuality >= ItemQuality.Legendary)) // don't blacklist legendaries
                                        hashRGUIDBlacklist60.Add(CurrentTarget.RActorGuid);
                                    //dateSinceBlacklist90Clear = DateTime.Now;
                                }
                                break;
                        }
                        break;
                    case 3:
                        ForceCloseRangeForMilliseconds = 2000;
                        // Cancel avoidance attempts for 1.5 seconds
                        cancelledEmergencyMoveForMilliseconds = 2000;
                        DbHelper.Log(LogCategory.Movement, "Canceling emergency movement for {0} ms", cancelledEmergencyMoveForMilliseconds);
                        timeCancelledEmergencyMove = DateTime.Now;

                        // Blacklist the current avoidance target area for the next avoidance-spot find
                        if (CurrentTarget.Type == GObjectType.Avoidance)
                            hashAvoidanceBlackspot.Add(new GilesObstacle(CurrentTarget.Position, 12f, -1, 0));
                        break;
                    default:
                        ForceCloseRangeForMilliseconds = 4000;
                        // Cancel avoidance attempts for 3.5 seconds
                        cancelledEmergencyMoveForMilliseconds = 4000;
                        DbHelper.Log(LogCategory.Movement, "Canceling emergency movement for {0} ms", cancelledEmergencyMoveForMilliseconds);
                        timeCancelledEmergencyMove = DateTime.Now;

                        // Blacklist the current avoidance target area for the next avoidance-spot find
                        if (TimesBlockedMoving == 4 && CurrentTarget.Type == GObjectType.Avoidance)
                            hashAvoidanceBlackspot.Add(new GilesObstacle(CurrentTarget.Position, 16f, -1, 0));
                        break;
                }
            }
        }

        /// <summary>
        /// Updates bot status text with appropriate information if we are moving into range of our <see cref="CurrentTarget"/>
        /// </summary>
        private static void UpdateStatusTextTarget(bool targetIsInRange)
        {
            StringBuilder statusText = new StringBuilder();
            switch (CurrentTarget.Type)
            {
                case GObjectType.Avoidance:
                    statusText.Append("Avoid ");
                    break;
                case GObjectType.Unit:
                    statusText.Append("Attack ");
                    break;
                case GObjectType.Item:
                case GObjectType.Gold:
                case GObjectType.Globe:
                    statusText.Append("Pickup ");
                    break;
                case GObjectType.Backtrack:
                    statusText.Append("Backtrack ");
                    break;
                case GObjectType.Interactable:
                    statusText.Append("Interact ");
                    break;
                case GObjectType.Door:
                case GObjectType.Container:
                    statusText.Append("Open ");
                    break;
                case GObjectType.Destructible:
                case GObjectType.Barricade:
                    statusText.Append("Destroy ");
                    break;
                case GObjectType.Shrine:
                    statusText.Append("Click ");
                    break;
            }
            statusText.Append("Target=");
            statusText.Append(CurrentTarget.InternalName);
            statusText.Append(" {");
            statusText.Append(CurrentTarget.ActorSNO);
            statusText.Append("} ");
            statusText.Append("Type=");
            statusText.Append(CurrentTarget.Type);
            statusText.Append(" C-Dist=");
            statusText.Append(CurrentTarget.CentreDistance.ToString("0.0"));
            statusText.Append(" R-Dist=");
            statusText.Append(CurrentTarget.RadiusDistance.ToString("0.0"));
            statusText.Append(" RangeReq'd=");
            statusText.Append(TargetRangeRequired.ToString("0.0"));
            statusText.Append(" DistfromTrgt=");
            statusText.Append(" tHP=");
            statusText.Append(CurrentTarget.HitPointsPct.ToString("0.00"));
            statusText.Append(TargetCurrentDistance.ToString("0.0"));
            statusText.Append(" MyHP=");
            statusText.Append((PlayerStatus.CurrentHealthPct * 100).ToString("0"));
            statusText.Append(" MyMana=");
            statusText.Append((PlayerStatus.PrimaryResource).ToString("0"));
            statusText.Append(" InLoS=");
            statusText.Append(CurrentTargetIsInLoS);
            statusText.Append(" ");
            if (CurrentTarget.Type == GObjectType.Unit && CurrentPower.SNOPower != SNOPower.None)
            {
                statusText.Append("Power=");
                statusText.Append(CurrentPower.SNOPower);
                statusText.Append(" (range ");
                statusText.Append(TargetRangeRequired);
                statusText.Append(") ");
            }
            statusText.Append("Weight=");
            statusText.Append(CurrentTarget.Weight.ToString("0"));
            statusText.Append(" RAGuid=");
            statusText.Append(CurrentTarget.RActorGuid);

            statusText.Append(String.Format(" Duration={0:0.0}", DateTime.Now.Subtract(dateSincePickedTarget).TotalSeconds));

            if (!targetIsInRange)
                statusText.Append(" MOVING INTO RANGE");
            if (Settings.Advanced.DebugInStatusBar)
            {
                sStatusText = "[Trinity] " + statusText.ToString();
                BotMain.StatusText = sStatusText;
            }
            DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.Targetting, "{0}", statusText.ToString());
            bResetStatusText = true;
        }

        /// <summary>
        /// Moves our player if no special ability is available
        /// </summary>
        /// <param name="bForceNewMovement"></param>
        private static void HandleTargetBasicMovement(bool bForceNewMovement)
        {
            using (new PerformanceLogger("HandleTarget.HandleBasicMovement"))
            {
                // Now for the actual movement request stuff
                IsAlreadyMoving = true;
                lastMovementCommand = DateTime.Now;

                if (DateTime.Now.Subtract(lastSentMovePower).TotalMilliseconds >= 250 || Vector3.Distance(vLastMoveToTarget, vCurrentDestination) >= 2f || bForceNewMovement)
                {
                    string destname = String.Format("Name={0} Dist={1} IsElite={2} HasBeenInLoS={3} HitPointsPct={4}",
                        CurrentTarget.InternalName,
                        CurrentTarget.RadiusDistance,
                        CurrentTarget.IsBossOrEliteRareUnique,
                        CurrentTarget.HasBeenInLoS,
                        CurrentTarget.HitPointsPct);

                    MoveResult lastMoveResult = PlayerMover.NavigateTo(vCurrentDestination, destname);
                    lastSentMovePower = DateTime.Now;

                    //if (lastMoveResult == MoveResult.ReachedDestination && vCurrentDestination.Distance2D(PlayerStatus.CurrentPosition) > 40f)
                    //{
                    //    hashRGUIDBlacklist60.Add(CurrentTarget.RActorGuid);
                    //    DbHelper.Log(TrinityLogLevel.Normal, LogCategory.Behavior, "Blacklisting {0} {1} {2} dist={3} " + (CurrentTarget.IsElite ? " IsElite" : "") + (CurrentTarget.ItemQuality >= ItemQuality.Legendary ? "IsLegendaryItem" : ""),
                    //        CurrentTarget.InternalName, CurrentTarget.ActorSNO, CurrentTarget.RActorGuid, CurrentTarget.CentreDistance);
                    //}

                    // Store the current destination for comparison incase of changes next loop
                    vLastMoveToTarget = vCurrentDestination;
                    // Reset total body-block count, since we should have moved
                    if (DateTime.Now.Subtract(lastForcedKeepCloseRange).TotalMilliseconds >= 2000)
                        TimesBlockedMoving = 0;
                }
            }
        }

        private static void SetRangeRequiredForTarget()
        {
            using (new PerformanceLogger("HandleTarget.SetRequiredRange"))
            {
                TargetRangeRequired = 1f;
                TargetCurrentDistance = 0;
                CurrentTargetIsInLoS = false;
                TargetDistanceReduction = 0f;
                // Set current destination to our current target's destination
                vCurrentDestination = CurrentTarget.Position;
                float fDistanceToDestination = PlayerStatus.CurrentPosition.Distance(vCurrentDestination);
                switch (CurrentTarget.Type)
                {
                    // * Unit, we need to pick an ability to use and get within range
                    case GObjectType.Unit:
                        {
                            // Treat the distance as closer based on the radius of monsters
                            TargetDistanceReduction = CurrentTarget.Radius;
                            if (ForceCloseRangeTarget)
                                TargetDistanceReduction -= 3f;
                            if (TargetDistanceReduction <= 0f)
                                TargetDistanceReduction = 0f;
                            // Pick a range to try to reach
                            TargetRangeRequired = CurrentPower.SNOPower == SNOPower.None ? 9f : CurrentPower.MinimumRange;
                            break;
                        }
                    // * Item - need to get within 6 feet and then interact with it
                    case GObjectType.Item:
                        {
                            TargetRangeRequired = 6f;

                            break;
                        }
                    // * Gold - need to get within pickup radius only
                    case GObjectType.Gold:
                        {
                            TargetRangeRequired = PlayerStatus.GoldPickupRadius - 2f;
                            if (TargetRangeRequired < 2f)
                                TargetRangeRequired = 2f;
                            break;
                        }
                    // * Globes - need to get within pickup radius only
                    case GObjectType.Globe:
                        {
                            TargetRangeRequired = PlayerStatus.GoldPickupRadius;
                            if (TargetRangeRequired < 2f)
                                TargetRangeRequired = 2f;
                            if (TargetRangeRequired > 5f)
                                TargetRangeRequired = 5f;
                            break;
                        }
                    // * Shrine & Container - need to get within 8 feet and interact
                    case GObjectType.HealthWell:
                        {
                            TargetRangeRequired = CurrentTarget.Radius + 5f;
                            //TargetRangeRequired = 5f;
                            int _range;
                            if (dictInteractableRange.TryGetValue(CurrentTarget.ActorSNO, out _range))
                            {
                                TargetRangeRequired = (float)_range;
                            }
                            break;
                        }
                    case GObjectType.Shrine:
                    case GObjectType.Container:
                        {
                            // Treat the distance as closer based on the radius of the object
                            TargetDistanceReduction = CurrentTarget.Radius;
                            TargetRangeRequired = 8f;
                            if (ForceCloseRangeTarget)
                                TargetRangeRequired -= 2f;
                            // Treat the distance as closer if the X & Y distance are almost point-blank, for objects
                            if (fDistanceToDestination <= 1.5f)
                                TargetDistanceReduction += 1f;
                            int iTempRange;
                            if (dictInteractableRange.TryGetValue(CurrentTarget.ActorSNO, out iTempRange))
                            {
                                TargetRangeRequired = (float)iTempRange;
                            }
                            break;
                        }
                    case GObjectType.Interactable:
                        {
                            // Treat the distance as closer based on the radius of the object
                            TargetDistanceReduction = CurrentTarget.Radius;
                            TargetRangeRequired = 12f;
                            if (ForceCloseRangeTarget)
                                TargetRangeRequired -= 2f;
                            // Check if it's in our interactable range dictionary or not
                            int iTempRange;
                            if (dictInteractableRange.TryGetValue(CurrentTarget.ActorSNO, out iTempRange))
                            {
                                TargetRangeRequired = (float)iTempRange;
                            }
                            // Treat the distance as closer if the X & Y distance are almost point-blank, for objects
                            if (fDistanceToDestination <= 1.5f)
                                TargetDistanceReduction += 1f;
                            break;
                        }
                    // * Destructible - need to pick an ability and attack it
                    case GObjectType.Destructible:
                        {
                            // Pick a range to try to reach + (tmp_fThisRadius * 0.70);
                            TargetRangeRequired = CurrentPower.SNOPower == SNOPower.None ? 9f : CurrentPower.MinimumRange;
                            TargetDistanceReduction = CurrentTarget.Radius;

                            if (ForceCloseRangeTarget)
                                TargetDistanceReduction += TimesBlockedMoving * 2.5f;

                            if (TargetDistanceReduction <= 0f)
                                TargetDistanceReduction = 0f;
                            // Treat the distance as closer if the X & Y distance are almost point-blank, for destructibles
                            if (fDistanceToDestination <= 1.5f)
                                TargetDistanceReduction += 1f;
                            break;
                        }
                    case GObjectType.Barricade:
                        {
                            // Pick a range to try to reach + (tmp_fThisRadius * 0.70);
                            TargetRangeRequired = CurrentPower.SNOPower == SNOPower.None ? 9f : CurrentPower.MinimumRange;
                            TargetDistanceReduction = CurrentTarget.Radius;

                            if (ForceCloseRangeTarget)
                                TargetDistanceReduction += TimesBlockedMoving * 3f;

                            if (TargetDistanceReduction <= 0f)
                                TargetDistanceReduction = 0f;

                            break;
                        }
                    // * Avoidance - need to pick an avoid location and move there
                    case GObjectType.Avoidance:
                        {
                            TargetRangeRequired = 2f;
                            // Treat the distance as closer if the X & Y distance are almost point-blank, for avoidance spots
                            if (fDistanceToDestination <= 1.5f)
                                TargetDistanceReduction += 2f;
                            break;
                        }
                    // * Backtrack Destination
                    case GObjectType.Backtrack:
                        {
                            TargetRangeRequired = 5f;
                            if (ForceCloseRangeTarget)
                                TargetRangeRequired -= 2f;
                            break;
                        }
                    case GObjectType.Door:
                        TargetRangeRequired = CurrentTarget.Radius + 2f;
                        break;
                    default:
                        TargetRangeRequired = CurrentTarget.Radius;
                        break;
                }
            }
        }

        private static void HandleUnitInRange()
        {
            using (new PerformanceLogger("HandleTarget.HandleUnitInRange"))
            {
                // Wait while animating before an attack
                if (CurrentPower.WaitForAnimationFinished)
                    WaitWhileAnimating(5, false);

                // try WW every tick if we want - we should use other methods to avoid this garbage code... 
                float dist = 0;
                if (CurrentPower.TargetPosition != Vector3.Zero)
                    dist = CurrentPower.TargetPosition.Distance2D(PlayerStatus.CurrentPosition);
                else if (CurrentTarget != null)
                    dist = CurrentTarget.Position.Distance2D(PlayerStatus.CurrentPosition);

                DbHelper.Log(TrinityLogLevel.Debug, LogCategory.Behavior, "Used Power {0} at {1} on {2} dist={3}", CurrentPower.SNOPower, CurrentPower.TargetPosition, CurrentPower.TargetRActorGUID, dist);
                var usePowerResult = ZetaDia.Me.UsePower(CurrentPower.SNOPower, CurrentPower.TargetPosition, CurrentPower.TargetDynamicWorldId, CurrentPower.TargetRActorGUID);

                if (usePowerResult)
                {
                    if (CurrentPower.SNOPower == SNOPower.Monk_TempestRush)
                        LastTempestRushLocation = CurrentPower.TargetPosition;

                    Monk_MaintainTempestRush();

                    dictAbilityLastUse[CurrentPower.SNOPower] = DateTime.Now;
                    lastGlobalCooldownUse = DateTime.Now;
                    LastPowerUsed = CurrentPower.SNOPower;
                    //CurrentPower.SNOPower = SNOPower.None;
                    // Wait for animating AFTER the attack
                    if (CurrentPower.WaitForAnimationFinished)
                        WaitWhileAnimating(3, false);
                    // See if we should force a long wait AFTERWARDS, too
                    // Force waiting AFTER power use for certain abilities
                    IsWaitingAfterPower = false;
                    if (CurrentPower.ShouldWaitAfterUse)
                    {
                        IsWaitingAfterPower = true;
                    }
                }

                ShouldPickNewAbilities = true;

                // Keep looking for monsters at "normal kill range" a few moments after we successfully attack a monster incase we can pull them into range
                iKeepKillRadiusExtendedFor = 8;
                timeKeepKillRadiusExtendedUntil = DateTime.Now.AddSeconds(iKeepKillRadiusExtendedFor);
                iKeepLootRadiusExtendedFor = 8;
                // if at full or nearly full health, see if we can raycast to it, if not, ignore it for 2000 ms
                if (CurrentTarget.HitPointsPct >= 0.9d)
                {
                    if (!NavHelper.CanRayCast(PlayerStatus.CurrentPosition, CurrentTarget.Position))
                    {
                        IgnoreRactorGUID = CurrentTarget.RActorGuid;
                        IgnoreTargetForLoops = 6;
                        // Add this monster to our very short-term ignore list
                        if (!CurrentTarget.IsBoss)
                        {
                            hashRGUIDBlacklist3.Add(CurrentTarget.RActorGuid);
                            DbHelper.Log(TrinityLogLevel.Debug, LogCategory.Behavior, "Blacklisting {0} {1} {2} for 3 seconds due to Raycast failure", CurrentTarget.Type, CurrentTarget.InternalName, CurrentTarget.ActorSNO);
                            dateSinceBlacklist3Clear = DateTime.Now;
                            NeedToClearBlacklist3 = true;
                        }
                    }
                }

            }
        }

        private static int HandleItemInRange()
        {
            using (new PerformanceLogger("HandleTarget.HandleItemInRange"))
            {
                int iInteractAttempts;
                // Pick the item up the usepower way, and "blacklist" for a couple of loops
                WaitWhileAnimating(12, true);
                ZetaDia.Me.UsePower(SNOPower.Axe_Operate_Gizmo, Vector3.Zero, 0, CurrentTarget.ACDGuid);
                IgnoreRactorGUID = CurrentTarget.RActorGuid;
                IgnoreTargetForLoops = 3;
                // Store item pickup stats

                string itemSha1Hash = HashGenerator.GenerateItemHash(CurrentTarget.Position, CurrentTarget.ActorSNO, CurrentTarget.InternalName, CurrentWorldDynamicId, CurrentTarget.ItemQuality, CurrentTarget.ItemLevel);
                if (!_hashsetItemPicksLookedAt.Contains(itemSha1Hash))
                {
                    _hashsetItemPicksLookedAt.Add(itemSha1Hash);
                    GItemType thisgilesitemtype = DetermineItemType(CurrentTarget.InternalName, CurrentTarget.DBItemType, CurrentTarget.FollowerType);
                    GItemBaseType thisgilesbasetype = DetermineBaseType(thisgilesitemtype);
                    if (thisgilesbasetype == GItemBaseType.Armor || thisgilesbasetype == GItemBaseType.WeaponOneHand || thisgilesbasetype == GItemBaseType.WeaponTwoHand ||
                        thisgilesbasetype == GItemBaseType.WeaponRange || thisgilesbasetype == GItemBaseType.Jewelry || thisgilesbasetype == GItemBaseType.FollowerItem ||
                        thisgilesbasetype == GItemBaseType.Offhand)
                    {
                        int iQuality;
                        ItemsPickedStats.Total++;
                        if (CurrentTarget.ItemQuality >= ItemQuality.Legendary)
                            iQuality = QUALITYORANGE;
                        else if (CurrentTarget.ItemQuality >= ItemQuality.Rare4)
                            iQuality = QUALITYYELLOW;
                        else if (CurrentTarget.ItemQuality >= ItemQuality.Magic1)
                            iQuality = QUALITYBLUE;
                        else
                            iQuality = QUALITYWHITE;
                        //asserts	
                        if (iQuality > QUALITYORANGE)
                        {
                            DbHelper.Log(TrinityLogLevel.Normal, LogCategory.UserInformation, "ERROR: Item type (" + iQuality + ") out of range");
                        }
                        if ((CurrentTarget.ItemLevel < 0) || (CurrentTarget.ItemLevel >= 64))
                        {
                            DbHelper.Log(TrinityLogLevel.Normal, LogCategory.UserInformation, "ERROR: Item level (" + CurrentTarget.ItemLevel + ") out of range");
                        }
                        ItemsPickedStats.TotalPerQuality[iQuality]++;
                        ItemsPickedStats.TotalPerLevel[CurrentTarget.ItemLevel]++;
                        ItemsPickedStats.TotalPerQPerL[iQuality, CurrentTarget.ItemLevel]++;
                    }
                    else if (thisgilesbasetype == GItemBaseType.Gem)
                    {
                        int iGemType = 0;
                        ItemsPickedStats.TotalGems++;
                        if (thisgilesitemtype == GItemType.Topaz)
                            iGemType = GEMTOPAZ;
                        if (thisgilesitemtype == GItemType.Ruby)
                            iGemType = GEMRUBY;
                        if (thisgilesitemtype == GItemType.Emerald)
                            iGemType = GEMEMERALD;
                        if (thisgilesitemtype == GItemType.Amethyst)
                            iGemType = GEMAMETHYST;
                        // !sp - asserts	
                        if (iGemType > GEMEMERALD)
                        {
                            DbHelper.Log(TrinityLogLevel.Normal, LogCategory.UserInformation, "ERROR: Gem type ({0}) out of range", iGemType);
                        }
                        if ((CurrentTarget.ItemLevel < 0) || (CurrentTarget.ItemLevel > 63))
                        {
                            DbHelper.Log(TrinityLogLevel.Normal, LogCategory.UserInformation, "ERROR: Gem level ({0}) out of range", CurrentTarget.ItemLevel);
                        }

                        ItemsPickedStats.GemsPerType[iGemType]++;
                        ItemsPickedStats.GemsPerLevel[CurrentTarget.ItemLevel]++;
                        ItemsPickedStats.GemsPerTPerL[iGemType, CurrentTarget.ItemLevel]++;
                    }
                    else if (thisgilesitemtype == GItemType.HealthPotion)
                    {
                        ItemsPickedStats.TotalPotions++;
                        if ((CurrentTarget.ItemLevel < 0) || (CurrentTarget.ItemLevel > 63))
                        {
                            DbHelper.Log(TrinityLogLevel.Normal, LogCategory.UserInformation, "ERROR: Potion level ({0}) out of range", CurrentTarget.ItemLevel);
                        }
                        ItemsPickedStats.PotionsPerLevel[CurrentTarget.ItemLevel]++;
                    }
                    else if (c_item_GItemType == GItemType.InfernalKey)
                    {
                        ItemsPickedStats.TotalInfernalKeys++;
                    }
                    // See if we should update the stats file
                    if (DateTime.Now.Subtract(ItemStatsLastPostedReport).TotalSeconds > 10)
                    {
                        ItemStatsLastPostedReport = DateTime.Now;
                        OutputReport();
                    }
                }
                WaitWhileAnimating(5, true);
                // Count how many times we've tried interacting
                if (!dictTotalInteractionAttempts.TryGetValue(CurrentTarget.RActorGuid, out iInteractAttempts))
                {
                    dictTotalInteractionAttempts.Add(CurrentTarget.RActorGuid, 1);
                }
                else
                {
                    dictTotalInteractionAttempts[CurrentTarget.RActorGuid]++;
                }
                // If we've tried interacting too many times, blacklist this for a while
                if (iInteractAttempts > 20 && CurrentTarget.ItemQuality < ItemQuality.Legendary)
                {
                    hashRGUIDBlacklist90.Add(CurrentTarget.RActorGuid);
                }
                // Now tell Trinity to get a new target!
                ForceTargetUpdate = true;
                return iInteractAttempts;
            }
        }
    }
}
