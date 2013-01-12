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
        private static Zeta.TreeSharp.RunStatus GetTreeSharpRunStatus(HandlerRunStatus rs)
        {
            switch (rs)
            {
                case HandlerRunStatus.TreeFailure:
                    return RunStatus.Failure;
                case HandlerRunStatus.TreeRunning:
                    return RunStatus.Running;
                case HandlerRunStatus.TreeSuccess:
                    return RunStatus.Success;
                case HandlerRunStatus.NotFinished:
                default:
                    throw new ApplicationException("Unable to return Non-TreeSharp RunStatus");
            }

        }

        /// <summary>
        /// Handles all aspects of moving to and attacking the current target
        /// </summary>
        /// <param name="ret"></param>
        /// <returns></returns>
        private static RunStatus HandleTarget(object ret)
        {
            using (new PerformanceLogger("GilesTrinity.GilesHandleTarget"))
            {
                try
                {
                    if (!ZetaDia.IsInGame || ZetaDia.IsLoadingWorld)
                    {
                        DbHelper.Log(TrinityLogLevel.Error, LogCategory.UserInformation, "No longer in game world", true);
                        return RunStatus.Failure;
                    }
                    HandlerRunStatus runStatus = HandlerRunStatus.NotFinished;

                    // Make sure we reset unstucker stuff here
                    PlayerMover.iTimesReachedStuckPoint = 0;
                    PlayerMover.vSafeMovementLocation = Vector3.Zero;
                    PlayerMover.TimeLastRecordedPosition = DateTime.Now;

                    // Whether we should refresh the target list or not
                    // See if we should update hotbar abilities
                    if (bRefreshHotbarAbilities)
                    {
                        RefreshHotbar(GetHasBuff(SNOPower.Wizard_Archon));
                    }
                    if (CurrentPower == null)
                        CurrentPower = AbilitySelector();
                    // Special pausing *AFTER* using certain powers
                    if (IsWaitingAfterPower && CurrentPower.ForceWaitLoopsAfter >= 1)
                    {
                        if (CurrentPower.ForceWaitLoopsAfter >= 1)
                            CurrentPower.ForceWaitLoopsAfter--;
                        if (CurrentPower.ForceWaitLoopsAfter <= 0)
                            IsWaitingAfterPower = false;
                        return RunStatus.Running;
                    }

                    // Check for death / player being dead
                    if (PlayerStatus.CurrentHealthPct <= 0)
                    {
                        runStatus = HandlerRunStatus.TreeSuccess;
                    }
                    //check if we are returning to the tree
                    if (runStatus != HandlerRunStatus.NotFinished)
                        return GetTreeSharpRunStatus(runStatus);

                    // See if we have been "newly rooted", to force target updates
                    if (PlayerStatus.IsRooted && !wasRootedLastTick)
                    {
                        wasRootedLastTick = true;
                        bForceTargetUpdate = true;
                    }
                    if (!PlayerStatus.IsRooted)
                        wasRootedLastTick = false;
                    if (CurrentTarget == null)
                    {
                        DbHelper.Log(TrinityLogLevel.Debug, LogCategory.Behavior, "CurrentTarget was passed as null!");
                    }
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
                            // Ok we didn't want a new target list, should we at least update the position of the current target, if it's a monster?
                            else if (CurrentTarget.Type == GObjectType.Unit && CurrentTarget.Unit != null && CurrentTarget.Unit.BaseAddress != IntPtr.Zero)
                            {
                                try
                                {
                                    CurrentTarget.Position = CurrentTarget.Unit.Position;
                                }
                                catch
                                {
                                    // Keep going anyway if we failed to get a new position from DemonBuddy
                                    DbHelper.Log(TrinityLogLevel.Debug, LogCategory.Behavior, "GSDEBUG: Caught position read failure in main target handler loop.");
                                }
                            }
                        }
                    }

                    if (CurrentTarget == null)
                    {
                        DbHelper.Log(TrinityLogLevel.Normal, LogCategory.Behavior, "CurrentTarget set as null in refresh");
                        runStatus = HandlerRunStatus.TreeSuccess;
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
                    if (PlayerStatus.CurrentHealthPct <= PlayerEmergencyHealthPotionLimit && !IsWaitingForPower && !IsWaitingForPotion && !PlayerStatus.IsIncapacitated && GilesUseTimer(SNOPower.DrinkHealthPotion))
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
                                ZetaDia.Me.UsePower(powerBuff.SNOPower, powerBuff.vTargetLocation, powerBuff.iTargetWorldID, powerBuff.iTargetGUID);
                                powerLastSnoPowerUsed = powerBuff.SNOPower;
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
                        // Maintain an area list of all zones we pass through/near while moving, for our custom navigation handler
                        if (DateTime.Now.Subtract(lastAddedLocationCache).TotalMilliseconds >= 100)
                        {
                            lastAddedLocationCache = DateTime.Now;
                            if (Vector3.Distance(PlayerStatus.CurrentPosition, vLastRecordedLocationCache) >= 5f)
                            {
                                hashSkipAheadAreaCache.Add(new GilesObstacle(PlayerStatus.CurrentPosition, 20f, 0));
                                vLastRecordedLocationCache = PlayerStatus.CurrentPosition;
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
                        TargetCurrentDistance = Vector3.Distance(PlayerStatus.CurrentPosition, vCurrentDestination) - TargetDistanceReduction;
                        if (TargetCurrentDistance < 0f)
                            TargetCurrentDistance = 0f;

                        if (Settings.Combat.Misc.UseNavMeshTargeting && CurrentTarget.Type != GObjectType.Barricade && CurrentTarget.Type != GObjectType.Destructible)
                        {
                            CurrentTargetIsInLoS = (GilesCanRayCast(PlayerStatus.CurrentPosition, vCurrentDestination, NavCellFlags.AllowWalk) || LineOfSightWhitelist.Contains(CurrentTarget.ActorSNO));
                        }
                        else
                        {
                            CurrentTargetIsInLoS = true;
                        }
                    }

                    using (new PerformanceLogger("HandleTarget.MonkWeaponSwap"))
                    {
                        // Item Swap + Blinding flash cast
                        if (PlayerStatus.ActorClass == ActorClass.Monk)
                        {
                            if (weaponSwap.DpsGearOn() && Settings.Combat.Monk.SweepingWindWeaponSwap &&
                                hashCachedPowerHotbarAbilities.Contains(SNOPower.Monk_SweepingWind))
                            {
                                if (PowerManager.CanCast(SNOPower.Monk_BlindingFlash) && DateTime.Now.Subtract(WeaponSwapTime).TotalMilliseconds >= 200 && !GetHasBuff(SNOPower.Monk_SweepingWind)
                                    && (PlayerStatus.CurrentEnergy >= 85 || (Settings.Combat.Monk.HasInnaSet && PlayerStatus.CurrentEnergy >= 15)))
                                {
                                    ZetaDia.Me.UsePower(SNOPower.Monk_BlindingFlash, vCurrentDestination, iCurrentWorldID, -1);
                                    return RunStatus.Running;
                                }
                                else if (DateTime.Now.Subtract(WeaponSwapTime).TotalMilliseconds >= 1500 || DateTime.Now.Subtract(WeaponSwapTime).TotalMilliseconds >= 800
                                        && GetHasBuff(SNOPower.Monk_SweepingWind))
                                {
                                    weaponSwap.SwapGear();
                                }
                            }
                            // Spam sweeping winds
                            if (hashCachedPowerHotbarAbilities.Contains(SNOPower.Monk_SweepingWind) && (PlayerStatus.CurrentEnergy >= 75 || (Settings.Combat.Monk.HasInnaSet && PlayerStatus.CurrentEnergy >= 5))
                                && (GetHasBuff(SNOPower.Monk_SweepingWind) && DateTime.Now.Subtract(SweepWindSpam).TotalMilliseconds >= 3700 && DateTime.Now.Subtract(SweepWindSpam).TotalMilliseconds < 5100
                                || !GetHasBuff(SNOPower.Monk_SweepingWind) && weaponSwap.DpsGearOn() && Settings.Combat.Monk.SweepingWindWeaponSwap &&
                                DateTime.Now.Subtract(WeaponSwapTime).TotalMilliseconds >= 400))
                            {
                                ZetaDia.Me.UsePower(SNOPower.Monk_SweepingWind, vCurrentDestination, iCurrentWorldID, -1);
                                SweepWindSpam = DateTime.Now;
                                return RunStatus.Running;
                            }
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
                                bForceTargetUpdate = true;
                                bAvoidDirectionBlacklisting = false;
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
                                            // Force waiting for global cooldown timer or long-animation abilities
                                            if (CurrentPower.iForceWaitLoopsBefore >= 1 || (CurrentPower.bWaitWhileAnimating != SIGNATURE_SPAM && DateTime.Now.Subtract(lastGlobalCooldownUse).TotalMilliseconds <= 50))
                                            {
                                                IsWaitingForPower = true;
                                                if (CurrentPower.iForceWaitLoopsBefore >= 1)
                                                    CurrentPower.iForceWaitLoopsBefore--;
                                                runStatus = HandlerRunStatus.TreeRunning;
                                            }
                                            else
                                            {
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
                                            runStatus = HandlerRunStatus.TreeSuccess;
                                        }
                                        else
                                        {
                                            iInteractAttempts = HandleItemInRange();
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
                                        lastChangedZigZag = DateTime.Today;
                                        vPositionLastZigZagCheck = Vector3.Zero;
                                        bForceTargetUpdate = true;
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
                                            hashRGUIDBlacklist90.Add(CurrentTarget.RActorGuid);
                                            //dateSinceBlacklist90Clear = DateTime.Now;
                                        }
                                        // Now tell Trinity to get a new target!
                                        lastChangedZigZag = DateTime.Today;
                                        vPositionLastZigZagCheck = Vector3.Zero;
                                        bForceTargetUpdate = true;

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
                                                ZetaDia.Me.UsePower(CurrentPower.SNOPower, vAttackPoint, iCurrentWorldID, -1);
                                            }
                                            else
                                            {
                                                // Standard attack - attack the ACDGUID (equivalent of left-clicking the object in-game)
                                                ZetaDia.Me.UsePower(CurrentPower.SNOPower, vNullLocation, -1, CurrentTarget.ACDGuid);
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
                                            CurrentPower.SNOPower = SNOPower.None;
                                            WaitWhileAnimating(6, true);
                                            // Prevent this EXACT object being targetted again for a short while, just incase
                                            IgnoreRactorGUID = CurrentTarget.RActorGuid;
                                            IgnoreTargetForLoops = 3;
                                            // Add this destructible/barricade to our very short-term ignore list
                                            hashRGUIDDestructible3SecBlacklist.Add(CurrentTarget.RActorGuid);
                                            lastDestroyedDestructible = DateTime.Now;
                                            bNeedClearDestructibles = true;
                                        }
                                        // Now tell Trinity to get a new target!
                                        bForceTargetUpdate = true;
                                        lastChangedZigZag = DateTime.Today;
                                        vPositionLastZigZagCheck = Vector3.Zero;
                                    }
                                    return RunStatus.Running;
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
                                    bForceTargetUpdate = true;
                                    return RunStatus.Running;
                            }
                            return RunStatus.Running;
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
                        return RunStatus.Running;
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
                                            GilesTrinity.GilesIntersectsPath(cp.Location, cp.Radius, PlayerStatus.CurrentPosition, point) &&
                                            cp.Location.Distance(PlayerStatus.CurrentPosition) > PlayerMover.GetObstacleNavigationSize(cp)))
                            {
                                if (vShiftedPosition == Vector3.Zero)
                                {
                                    if (DateTime.Now.Subtract(lastShiftedPosition).TotalSeconds >= 10)
                                    {
                                        float fDirectionToTarget = GilesTrinity.FindDirectionDegree(PlayerStatus.CurrentPosition, vCurrentDestination);
                                        vCurrentDestination = MathEx.GetPointAt(PlayerStatus.CurrentPosition, 15f, MathEx.ToRadians(fDirectionToTarget - 50));
                                        if (!GilesCanRayCast(PlayerStatus.CurrentPosition, vCurrentDestination, NavCellFlags.AllowWalk))
                                        {
                                            vCurrentDestination = MathEx.GetPointAt(PlayerStatus.CurrentPosition, 15f, MathEx.ToRadians(fDirectionToTarget + 50));
                                            if (!GilesCanRayCast(PlayerStatus.CurrentPosition, vCurrentDestination, NavCellFlags.AllowWalk))
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



                    // Only position-shift when not avoiding
                    // See if we want to ACTUALLY move, or are just waiting for the last move command...
                    if (!bForceNewMovement && IsAlreadyMoving && vCurrentDestination == vLastMoveToTarget && DateTime.Now.Subtract(lastMovementCommand).TotalMilliseconds <= 100)
                    {
                        return RunStatus.Running;
                    }
                    using (new PerformanceLogger("HandleTarget.SpecialMovement"))
                    {

                        // If we're doing avoidance, globes or backtracking, try to use special abilities to move quicker
                        if ((CurrentTarget.Type == GObjectType.Avoidance ||
                            CurrentTarget.Type == GObjectType.Globe ||
                            (CurrentTarget.Type == GObjectType.Backtrack && Settings.Combat.Misc.AllowOOCMovement))
                            && GilesCanRayCast(PlayerStatus.CurrentPosition, vCurrentDestination, NavCellFlags.AllowWalk)
                            )
                        {
                            bool bFoundSpecialMovement = UsedSpecialMovement();

                            if (CurrentTarget.Type != GObjectType.Backtrack)
                            {
                                // Whirlwind for a barb
                                //intell
                                if (!IsWaitingForSpecial && CurrentPower.SNOPower != SNOPower.Barbarian_WrathOfTheBerserker && !bFoundSpecialMovement && Hotbar.Contains(SNOPower.Barbarian_Whirlwind) && PlayerStatus.CurrentEnergy >= 10)
                                {
                                    ZetaDia.Me.UsePower(SNOPower.Barbarian_Whirlwind, vCurrentDestination, iCurrentWorldID, -1);
                                    // Store the current destination for comparison incase of changes next loop
                                    vLastMoveToTarget = vCurrentDestination;
                                    // Reset total body-block count, since we should have moved
                                    if (DateTime.Now.Subtract(lastForcedKeepCloseRange).TotalMilliseconds >= 2000)
                                        TimesBlockedMoving = 0;
                                    return RunStatus.Running;
                                }
                                // Tempest rush for a monk
                                if (!bFoundSpecialMovement && Hotbar.Contains(SNOPower.Monk_TempestRush) && PlayerStatus.CurrentEnergy >= 20 &&
                                    ((CurrentTarget.Type == GObjectType.Item && CurrentTarget.CentreDistance > 20f) || CurrentTarget.Type != GObjectType.Item))
                                {
                                    ZetaDia.Me.UsePower(SNOPower.Monk_TempestRush, vCurrentDestination, iCurrentWorldID, -1);
                                    // Store the current destination for comparison incase of changes next loop
                                    vLastMoveToTarget = vCurrentDestination;
                                    // Reset total body-block count, since we should have moved
                                    if (DateTime.Now.Subtract(lastForcedKeepCloseRange).TotalMilliseconds >= 2000)
                                        TimesBlockedMoving = 0;
                                    return RunStatus.Running;
                                }
                                // Strafe for a Demon Hunter
                                if (!bFoundSpecialMovement && Hotbar.Contains(SNOPower.DemonHunter_Strafe) && PlayerStatus.CurrentEnergy >= 15)
                                {
                                    ZetaDia.Me.UsePower(SNOPower.DemonHunter_Strafe, vCurrentDestination, iCurrentWorldID, -1);
                                    // Store the current destination for comparison incase of changes next loop
                                    vLastMoveToTarget = vCurrentDestination;
                                    // Reset total body-block count, since we should have moved
                                    if (DateTime.Now.Subtract(lastForcedKeepCloseRange).TotalMilliseconds >= 2000)
                                        TimesBlockedMoving = 0;
                                    return RunStatus.Running;
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
                                return RunStatus.Running;
                            }
                        }
                    }

                    // Whirlwind against everything within range (except backtrack points)
                    //intell
                    if (Hotbar.Contains(SNOPower.Barbarian_Whirlwind) && PlayerStatus.CurrentEnergy >= 10 && AnythingWithinRange[RANGE_20] >= 1 && !IsWaitingForSpecial && CurrentPower.SNOPower != SNOPower.Barbarian_WrathOfTheBerserker && TargetCurrentDistance <= 12f && CurrentTarget.Type != GObjectType.Container && CurrentTarget.Type != GObjectType.Backtrack &&
                        (!Hotbar.Contains(SNOPower.Barbarian_Sprint) || GetHasBuff(SNOPower.Barbarian_Sprint)) &&
                        CurrentTarget.Type != GObjectType.Backtrack &&
                        (CurrentTarget.Type != GObjectType.Item && CurrentTarget.Type != GObjectType.Gold && TargetCurrentDistance >= 6f) &&
                        (CurrentTarget.Type != GObjectType.Unit ||
                        (CurrentTarget.Type == GObjectType.Unit && !CurrentTarget.IsTreasureGoblin &&
                            (!Settings.Combat.Barbarian.SelectiveWhirlwind || bAnyNonWWIgnoreMobsInRange || !hashActorSNOWhirlwindIgnore.Contains(CurrentTarget.ActorSNO)))))
                    {
                        // Special code to prevent whirlwind double-spam, this helps save fury
                        bool bUseThisLoop = SNOPower.Barbarian_Whirlwind != powerLastSnoPowerUsed;
                        if (!bUseThisLoop)
                        {
                            powerLastSnoPowerUsed = SNOPower.None;
                            if (DateTime.Now.Subtract(dictAbilityLastUse[SNOPower.Barbarian_Whirlwind]).TotalMilliseconds >= 200)
                                bUseThisLoop = true;
                        }
                        if (bUseThisLoop)
                        {
                            ZetaDia.Me.UsePower(SNOPower.Barbarian_Whirlwind, vCurrentDestination, iCurrentWorldID, -1);
                            powerLastSnoPowerUsed = SNOPower.Barbarian_Whirlwind;
                            dictAbilityLastUse[SNOPower.Barbarian_Whirlwind] = DateTime.Now;
                        }
                        // Store the current destination for comparison incase of changes next loop
                        vLastMoveToTarget = vCurrentDestination;
                        // Reset total body-block count
                        if ((!ForceCloseRangeTarget || DateTime.Now.Subtract(lastForcedKeepCloseRange).TotalMilliseconds > ForceCloseRangeForMilliseconds) &&
                            DateTime.Now.Subtract(lastForcedKeepCloseRange).TotalMilliseconds >= 2000)
                            TimesBlockedMoving = 0;
                        return RunStatus.Running;
                    }
                }
                catch (Exception ex)
                {
                    DbHelper.Log(TrinityLogLevel.Debug, LogCategory.Behavior, "{0}", ex);
                    return RunStatus.Failure;
                }

                GilesHandleTargetBasicMovement(bForceNewMovement);

                return RunStatus.Running;
            }
        }

        private static HandlerRunStatus HandleTargetDistanceCheck(HandlerRunStatus runStatus)
        {
            using (new PerformanceLogger("HandleTarget.DistanceEqualCheck"))
            {
                // Count how long we have failed to move - body block stuff etc.
                if (Math.Abs(TargetCurrentDistance - fLastDistanceFromTarget) < 2f)
                {
                    bForceNewMovement = true;
                    if (DateTime.Now.Subtract(lastMovedDuringCombat).TotalMilliseconds >= 250)
                    {
                        lastMovedDuringCombat = DateTime.Now;
                        // We've been stuck at least 250 ms, let's go and pick new targets etc.
                        TimesBlockedMoving++;
                        ForceCloseRangeTarget = true;
                        lastForcedKeepCloseRange = DateTime.Now;
                        // And tell Trinity to get a new target
                        bForceTargetUpdate = true;
                        // Blacklist an 80 degree direction for avoidance
                        if (CurrentTarget.Type == GObjectType.Avoidance)
                        {
                            bAvoidDirectionBlacklisting = true;
                            fAvoidBlacklistDirection = FindDirectionDegree(PlayerStatus.CurrentPosition, CurrentTarget.Position);
                        }
                        // Handle body blocking by blacklisting
                        GilesHandleBodyBlocking();
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

                if (!CurrentTargetIsNotAvoidance())
                    return HandlerRunStatus.NotFinished;

                if (CurrentTargetIsNonUnit() && GetSecondsSinceTargetUpdate() > 6)
                    shouldTryBlacklist = true;

                if ((CurrentTargetIsUnit() && GetSecondsSinceTargetUpdate() > 15))
                    shouldTryBlacklist = true;

                // special raycast check for current target after 5 sec
                if ((CurrentTargetIsUnit() && GetSecondsSinceTargetUpdate() > 5))
                    shouldTryBlacklist = true;

                if (shouldTryBlacklist)
                {
                    // NOTE: This only blacklists if it's remained the PRIMARY TARGET that we are trying to actually directly attack!
                    // So it won't blacklist a monster "on the edge of the screen" who isn't even being targetted
                    // Don't blacklist monsters on <= 50% health though, as they can't be in a stuck location... can they!? Maybe give them some extra time!

                    bool isNavigable;

                    if (Settings.Combat.Misc.UseNavMeshTargeting)
                        isNavigable = pf.IsNavigable(gp.WorldToGrid(CurrentTarget.Position.ToVector2()));
                    else
                        isNavigable = true;

                    bool addTargetToBlacklist = true;


                    // PREVENT blacklisting a monster on less than 90% health unless we haven't damaged it for more than 2 minutes
                    if (CurrentTarget.Type == GObjectType.Unit && isNavigable)
                    {
                        if (CurrentTarget.IsTreasureGoblin && Settings.Combat.Misc.GoblinPriority >= GoblinPriority.Kamikaze)
                            addTargetToBlacklist = false;
                    }

                    // Check if we can Raycast to a trash mob
                    if (CurrentTarget.IsTrashMob && 
                        GetSecondsSinceTargetUpdate() > 4 && 
                        CurrentTarget.HitPoints > 0.90)
                    {
                        if (GilesCanRayCast(PlayerStatus.CurrentPosition, CurrentTarget.Position, NavCellFlags.AllowWalk))
                        {
                            addTargetToBlacklist = false;
                        }
                    }

                    if (addTargetToBlacklist)
                    {
                        if (CurrentTarget.Type == GObjectType.Unit)
                        {
                            DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.Behavior,
                                "Blacklisting a monster because of possible stuck issues. Monster={0} [{1}] Range={2:0} health %={3:0} RActorGUID={4}",
                                CurrentTarget.InternalName,         // 0
                                CurrentTarget.ActorSNO,             // 1
                                CurrentTarget.CentreDistance,       // 2
                                CurrentTarget.HitPoints,            // 3
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
                    if (bForceTargetUpdate || IsAvoidingProjectiles)
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
                                    dThisMaxHealth = CurrentTarget.Unit.CommonData.GetAttribute<float>(ActorAttributeType.HitpointsMax);
                                    dictGilesMaxHealthCache.Add(c_RActorGuid, dThisMaxHealth);
                                }
                                catch
                                {
                                    DbHelper.Log(TrinityLogLevel.Debug, LogCategory.Behavior, "Safely handled exception getting attribute max health #2 for unit {0} [{1}]", c_Name, c_ActorSNO);
                                    StaleCache = true;
                                }
                            }
                            // Ok check we didn't fail getting the maximum health, now try to get live current health...
                            if (!StaleCache)
                            {
                                try
                                {
                                    double dTempHitpoints = (CurrentTarget.Unit.CommonData.GetAttribute<float>(ActorAttributeType.HitpointsCur) / dThisMaxHealth);
                                    if (dTempHitpoints <= 0d)
                                    {
                                        StaleCache = true;
                                    }
                                    else
                                    {
                                        CurrentTarget.HitPoints = dTempHitpoints;
                                        CurrentTarget.Position = CurrentTarget.Unit.Position;
                                    }
                                }
                                catch
                                {
                                    StaleCache = true;
                                }
                            }
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
                    ZetaDia.Me.UsePower(SNOPower.Barbarian_Leap, vCurrentDestination, iCurrentWorldID, -1);
                    dictAbilityLastUse[SNOPower.Barbarian_Leap] = DateTime.Now;
                    bFoundSpecialMovement = true;
                }
                // Furious Charge movement for a barb
                if (!bFoundSpecialMovement && Hotbar.Contains(SNOPower.Barbarian_FuriousCharge) &&
                    DateTime.Now.Subtract(dictAbilityLastUse[SNOPower.Barbarian_FuriousCharge]).TotalMilliseconds >= dictAbilityRepeatDelay[SNOPower.Barbarian_FuriousCharge] &&
                    PowerManager.CanCast(SNOPower.Barbarian_FuriousCharge))
                {
                    WaitWhileAnimating(3, true);
                    ZetaDia.Me.UsePower(SNOPower.Barbarian_FuriousCharge, vCurrentDestination, iCurrentWorldID, -1);
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
                    ZetaDia.Me.UsePower(SNOPower.DemonHunter_Vault, vCurrentDestination, iCurrentWorldID, -1);
                    dictAbilityLastUse[SNOPower.DemonHunter_Vault] = DateTime.Now;
                    bFoundSpecialMovement = true;
                }
                // Teleport for a wizard (need to be able to check skill rune in DB for a 3-4 teleport spam in a row)
                if (!bFoundSpecialMovement && Hotbar.Contains(SNOPower.Wizard_Teleport) &&
                    DateTime.Now.Subtract(dictAbilityLastUse[SNOPower.Wizard_Teleport]).TotalMilliseconds >= dictAbilityRepeatDelay[SNOPower.Wizard_Teleport] &&
                    PlayerStatus.CurrentEnergy >= 15 &&
                    PowerManager.CanCast(SNOPower.Wizard_Teleport))
                {
                    WaitWhileAnimating(3, true);
                    ZetaDia.Me.UsePower(SNOPower.Wizard_Teleport, vCurrentDestination, iCurrentWorldID, -1);
                    dictAbilityLastUse[SNOPower.Wizard_Teleport] = DateTime.Now;
                    bFoundSpecialMovement = true;
                }
                // Archon Teleport for a wizard (need to be able to check skill rune in DB for a 3-4 teleport spam in a row)
                if (!bFoundSpecialMovement && Hotbar.Contains(SNOPower.Wizard_Archon_Teleport) &&
                    DateTime.Now.Subtract(dictAbilityLastUse[SNOPower.Wizard_Archon_Teleport]).TotalMilliseconds >= dictAbilityRepeatDelay[SNOPower.Wizard_Archon_Teleport] &&
                    PowerManager.CanCast(SNOPower.Wizard_Archon_Teleport))
                {
                    WaitWhileAnimating(3, true);
                    ZetaDia.Me.UsePower(SNOPower.Wizard_Archon_Teleport, vCurrentDestination, iCurrentWorldID, -1);
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
                    case 1:
                        ForceCloseRangeForMilliseconds = 850;
                        break;
                    case 2:
                        ForceCloseRangeForMilliseconds = 1300;
                        // Cancel avoidance attempts for 500ms
                        cancelledEmergencyMoveForMilliseconds = 1500;
                        timeCancelledEmergencyMove = DateTime.Now;
                        vlastSafeSpot = vNullLocation;
                        // Check for raycastability against objects
                        switch (CurrentTarget.Type)
                        {
                            case GObjectType.Container:
                            case GObjectType.Shrine:
                            case GObjectType.Globe:
                            case GObjectType.Gold:
                            case GObjectType.Item:
                                // No raycast available, try and force-ignore this for a little while, and blacklist for a few seconds
                                if (!GilesCanRayCast(PlayerStatus.CurrentPosition, CurrentTarget.Position, NavCellFlags.AllowWalk))
                                {
                                    IgnoreRactorGUID = CurrentTarget.RActorGuid;
                                    IgnoreTargetForLoops = 6;
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
                        timeCancelledEmergencyMove = DateTime.Now;
                        vlastSafeSpot = vNullLocation;
                        // Blacklist the current avoidance target area for the next avoidance-spot find
                        if (CurrentTarget.Type == GObjectType.Avoidance)
                            hashAvoidanceBlackspot.Add(new GilesObstacle(CurrentTarget.Position, 12f, -1, 0));
                        break;
                    default:
                        ForceCloseRangeForMilliseconds = 4000;
                        // Cancel avoidance attempts for 3.5 seconds
                        cancelledEmergencyMoveForMilliseconds = 4000;
                        timeCancelledEmergencyMove = DateTime.Now;
                        vlastSafeSpot = vNullLocation;
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
            statusText.Append("}. ");
            statusText.Append("Type=");
            statusText.Append(CurrentTarget.Type);
            statusText.Append(" C-Dist=");
            statusText.Append(CurrentTarget.CentreDistance.ToString("0.0"));
            statusText.Append(". R-Dist=");
            statusText.Append(CurrentTarget.RadiusDistance.ToString("0.0"));
            statusText.Append(". RangeReq'd: ");
            statusText.Append(TargetRangeRequired.ToString("0.0"));
            statusText.Append(". DistfromTrgt: ");
            statusText.Append(TargetCurrentDistance.ToString("0.0"));
            statusText.Append(". InLoS: ");
            statusText.Append(CurrentTargetIsInLoS);
            statusText.Append(". ");
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
        private static void GilesHandleTargetBasicMovement(bool bForceNewMovement)
        {
            using (new PerformanceLogger("HandleTarget.HandleBasicMovement"))
            {
                // Now for the actual movement request stuff
                IsAlreadyMoving = true;
                lastMovementCommand = DateTime.Now;
                if (DateTime.Now.Subtract(lastSentMovePower).TotalMilliseconds >= 250 || Vector3.Distance(vLastMoveToTarget, vCurrentDestination) >= 2f || bForceNewMovement)
                {
                    //ZetaDia.Me.UsePower(SNOPower.Walk, vCurrentDestination, iCurrentWorldID, -1);

                    //Navigator.PlayerMover.MoveTowards(vCurrentDestination);
                    //ZetaDia.Me.Movement.MoveActor(vCurrentDestination);
                    Navigator.MoveTo(vCurrentDestination, CurrentTarget.InternalName, true);
                    lastSentMovePower = DateTime.Now;

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
                            TargetRangeRequired = CurrentPower.SNOPower == SNOPower.None ? 9f : CurrentPower.iMinimumRange;
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
                            TargetRangeRequired = (float)ZetaDia.Me.GoldPickUpRadius;
                            if (TargetRangeRequired < 2f)
                                TargetRangeRequired = 2f;
                            break;
                        }
                    // * Globes - need to get within pickup radius only
                    case GObjectType.Globe:
                        {
                            TargetRangeRequired = (float)ZetaDia.Me.GoldPickUpRadius;
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
                            TargetRangeRequired = 5f;
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
                            TargetRangeRequired = CurrentPower.SNOPower == SNOPower.None ? 9f : CurrentPower.iMinimumRange;
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
                            TargetRangeRequired = CurrentPower.SNOPower == SNOPower.None ? 9f : CurrentPower.iMinimumRange;
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
                IsWaitingForPower = false;
                // Wait while animating before an attack
                if (CurrentPower.bWaitWhileAnimating)
                    WaitWhileAnimating(5, false);
                // Use the power
                bool bUsePowerSuccess = false;
                // Note that whirlwinds use an off-on-off-on to avoid spam
                if (CurrentPower.SNOPower != SNOPower.Barbarian_Whirlwind && CurrentPower.SNOPower != SNOPower.DemonHunter_Strafe)
                {
                    ZetaDia.Me.UsePower(CurrentPower.SNOPower, CurrentPower.vTargetLocation, CurrentPower.iTargetWorldID, CurrentPower.iTargetGUID);
                    bUsePowerSuccess = true;
                    lastChangedZigZag = DateTime.Today;
                    vPositionLastZigZagCheck = Vector3.Zero;
                }
                else
                {
                    // Special code to prevent whirlwind double-spam, this helps save fury
                    bool bUseThisLoop = CurrentPower.SNOPower != powerLastSnoPowerUsed;
                    if (!bUseThisLoop)
                    {
                        //powerLastSnoPowerUsed = SNOPower.None;
                        if (DateTime.Now.Subtract(dictAbilityLastUse[CurrentPower.SNOPower]).TotalMilliseconds >= 200)
                            bUseThisLoop = true;
                    }
                    if (bUseThisLoop)
                    {
                        ZetaDia.Me.UsePower(CurrentPower.SNOPower, CurrentPower.vTargetLocation, CurrentPower.iTargetWorldID, CurrentPower.iTargetGUID);
                        bUsePowerSuccess = true;
                    }
                }
                if (bUsePowerSuccess)
                {
                    dictAbilityLastUse[CurrentPower.SNOPower] = DateTime.Now;
                    lastGlobalCooldownUse = DateTime.Now;
                    powerLastSnoPowerUsed = CurrentPower.SNOPower;
                    CurrentPower.SNOPower = SNOPower.None;
                    // Wait for animating AFTER the attack
                    if (CurrentPower.bWaitWhileAnimating)
                        WaitWhileAnimating(3, false);
                }
                // Wait for animating AFTER the attack
                if (CurrentPower.bWaitWhileAnimating)
                    WaitWhileAnimating(3, false);
                ShouldPickNewAbilities = true;
                // Keep looking for monsters at "normal kill range" a few moments after we successfully attack a monster incase we can pull them into range
                iKeepKillRadiusExtendedFor = 8;
                timeKeepKillRadiusExtendedUntil = DateTime.Now.AddSeconds(iKeepKillRadiusExtendedFor);
                iKeepLootRadiusExtendedFor = 8;
                // if at full or nearly full health, see if we can raycast to it, if not, ignore it for 2000 ms
                if (CurrentTarget.HitPoints >= 0.9d && AnythingWithinRange[RANGE_50] > 3)
                {
                    if (!GilesCanRayCast(PlayerStatus.CurrentPosition, CurrentTarget.Position, NavCellFlags.AllowWalk))
                    {
                        IgnoreRactorGUID = CurrentTarget.RActorGuid;
                        IgnoreTargetForLoops = 6;
                        // Add this monster to our very short-term ignore list
                        if (!CurrentTarget.IsBoss)
                        {
                            hashRGUIDBlacklist3.Add(CurrentTarget.RActorGuid);
                            dateSinceBlacklist3Clear = DateTime.Now;
                            NeedToClearBlacklist3 = true;
                        }
                    }
                }
                // See if we should force a long wait AFTERWARDS, too
                // Force waiting AFTER power use for certain abilities
                IsWaitingAfterPower = false;
                if (CurrentPower.ForceWaitLoopsAfter >= 1)
                {
                    IsWaitingAfterPower = true;
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
                lastChangedZigZag = DateTime.Today;
                vPositionLastZigZagCheck = Vector3.Zero;
                IgnoreRactorGUID = CurrentTarget.RActorGuid;
                IgnoreTargetForLoops = 3;
                // Store item pickup stats

                string itemSha1Hash = ItemHash.GenerateItemHash(CurrentTarget.Position, CurrentTarget.ActorSNO, CurrentTarget.InternalName, iCurrentWorldID, CurrentTarget.ItemQuality, CurrentTarget.ItemLevel);
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
                if (iInteractAttempts > 20)
                {
                    hashRGUIDBlacklist90.Add(CurrentTarget.RActorGuid);
                    //dateSinceBlacklist90Clear = DateTime.Now;
                }
                // Now tell Trinity to get a new target!
                bForceTargetUpdate = true;
                return iInteractAttempts;
            }
        }
    }
}
