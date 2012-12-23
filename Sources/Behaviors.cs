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

                    new Action(ctx => GilesHandleTarget(ctx))
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
        private static RunStatus GilesHandleTarget(object ret)
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
                    GilesPlayerMover.iTimesReachedStuckPoint = 0;
                    GilesPlayerMover.vSafeMovementLocation = Vector3.Zero;
                    GilesPlayerMover.timeLastRecordedPosition = DateTime.Now;

                    // Whether we should refresh the target list or not
                    // See if we should update hotbar abilities
                    if (bRefreshHotbarAbilities)
                    {
                        GilesRefreshHotbar(GilesHasBuff(SNOPower.Wizard_Archon));
                    }
                    if (currentPower == null)
                        currentPower = GilesAbilitySelector();
                    // Special pausing *AFTER* using certain powers
                    if (bWaitingAfterPower && currentPower.iForceWaitLoopsAfter >= 1)
                    {
                        if (currentPower.iForceWaitLoopsAfter >= 1)
                            currentPower.iForceWaitLoopsAfter--;
                        if (currentPower.iForceWaitLoopsAfter <= 0)
                            bWaitingAfterPower = false;
                        return RunStatus.Running;
                    }

                    // Check for death / player being dead
                    if (playerStatus.CurrentHealthPct <= 0)
                    {
                        runStatus = HandlerRunStatus.TreeSuccess;
                    }
                    //check if we are returning to the tree
                    if (runStatus != HandlerRunStatus.NotFinished)
                        return GetTreeSharpRunStatus(runStatus);

                    // See if we have been "newly rooted", to force target updates
                    if (playerStatus.IsRooted && !wasRootedLastTick)
                    {
                        wasRootedLastTick = true;
                        bForceTargetUpdate = true;
                    }
                    if (!playerStatus.IsRooted)
                        wasRootedLastTick = false;
                    if (CurrentTarget == null)
                    {
                        DbHelper.Log(TrinityLogLevel.Debug, LogCategory.Behavior, "CurrentTarget was passed as null!");
                    }
                    CheckStaleCache();
                    // So, after all that, do we actually want a new target list?
                    if (!bWholeNewTarget && !bWaitingForPower && !bWaitingForPotion)
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
                                bAlreadyMoving = false;
                                lastMovementCommand = DateTime.Today;
                                bPickNewAbilities = true;
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

                    if (CurrentTarget == null)
                    {
                        DbHelper.Log(TrinityLogLevel.Normal, LogCategory.Behavior, "CurrentTarget set as null in refresh");
                        runStatus = HandlerRunStatus.TreeSuccess;
                    }

                    //check if we are returning to the tree
                    if (runStatus != HandlerRunStatus.NotFinished)
                        return GetTreeSharpRunStatus(runStatus);

                    // Blacklist the current target
                    runStatus = HandleTargetTimeout(runStatus);

                    //check if we are returning to the tree
                    if (runStatus != HandlerRunStatus.NotFinished)
                        return GetTreeSharpRunStatus(runStatus);

                    // This variable just prevents an instant 2-target update after coming here from the main decorator function above
                    bWholeNewTarget = false;
                    AssignMonsterTargetPower();

                    // Pop a potion when necessary
                    // Note that we force a single-loop pause first, to help potion popping "go off"
                    if (playerStatus.CurrentHealthPct <= PlayerEmergencyHealthPotionLimit && !bWaitingForPower && !bWaitingForPotion && !playerStatus.IsIncapacitated && GilesUseTimer(SNOPower.DrinkHealthPotion))
                    {
                        bWaitingForPotion = true;
                        runStatus = HandlerRunStatus.TreeRunning;
                    }

                    //check if we are returning to the tree
                    if (runStatus != HandlerRunStatus.NotFinished)
                        return GetTreeSharpRunStatus(runStatus);

                    // If we just looped waiting for a potion, use it
                    UseHealthPotionIfNeeded();

                    // See if we can use any special buffs etc. while in avoidance
                    if (CurrentTarget.Type == GObjectType.Avoidance)
                    {
                        powerBuff = GilesAbilitySelector(true, false, false);
                        if (powerBuff.SNOPower != SNOPower.None)
                        {
                            ZetaDia.Me.UsePower(powerBuff.SNOPower, powerBuff.vTargetLocation, powerBuff.iTargetWorldID, powerBuff.iTargetGUID);
                            powerLastSnoPowerUsed = powerBuff.SNOPower;
                            dictAbilityLastUse[powerBuff.SNOPower] = DateTime.Now;
                        }
                    }

                    // Pick the destination point and range of target
                    /*
                     * Set the range required for attacking/interacting/using
                     */

                    SetRangeRequiredForTarget();


                    // Maintain an area list of all zones we pass through/near while moving, for our custom navigation handler
                    if (DateTime.Now.Subtract(lastAddedLocationCache).TotalMilliseconds >= 100)
                    {
                        lastAddedLocationCache = DateTime.Now;
                        if (Vector3.Distance(playerStatus.CurrentPosition, vLastRecordedLocationCache) >= 5f)
                        {
                            hashSkipAheadAreaCache.Add(new GilesObstacle(playerStatus.CurrentPosition, 20f, 0));
                            vLastRecordedLocationCache = playerStatus.CurrentPosition;
                        }
                    }
                    // Maintain a backtrack list only while fighting monsters
                    if (CurrentTarget.Type == GObjectType.Unit && Settings.Combat.Misc.AllowBacktracking &&
                        (iTotalBacktracks == 0 || Vector3.Distance(playerStatus.CurrentPosition, vBacktrackList[iTotalBacktracks]) >= 10f))
                    {
                        bool bAddThisBacktrack = true;
                        // Check we aren't within 12 feet of 2 backtracks again (eg darting back & forth)
                        if (iTotalBacktracks >= 2)
                        {
                            if (Vector3.Distance(playerStatus.CurrentPosition, vBacktrackList[iTotalBacktracks - 1]) < 12f)
                                bAddThisBacktrack = false;
                        }
                        if (bAddThisBacktrack)
                        {
                            iTotalBacktracks++;
                            vBacktrackList.Add(iTotalBacktracks, playerStatus.CurrentPosition);
                        }
                    }

                    // Calculate the player's current distance from destination
                    float fDistanceFromTarget = Vector3.Distance(playerStatus.CurrentPosition, vCurrentDestination) - fDistanceReduction;
                    if (fDistanceFromTarget < 0f)
                        fDistanceFromTarget = 0f;

                    bool currentTargetIsInLoS = (GilesCanRayCast(playerStatus.CurrentPosition, vCurrentDestination, NavCellFlags.AllowWalk) || LineOfSightWhitelist.Contains(CurrentTarget.ActorSNO));

					// Item Swap + Blinding flash cast
					if (playerStatus.ActorClass == ActorClass.Monk)
					{
						if (weaponSwap.DpsGearOn() && Settings.Combat.Monk.SweepingWindWeaponSwap && 
							hashCachedPowerHotbarAbilities.Contains(SNOPower.Monk_SweepingWind))
						{
							if (PowerManager.CanCast(SNOPower.Monk_BlindingFlash) && DateTime.Now.Subtract(WeaponSwapTime).TotalMilliseconds >= 200 && !GilesHasBuff(SNOPower.Monk_SweepingWind) 
								&& (playerStatus.CurrentEnergy >= 85 || (Settings.Combat.Monk.HasInnaSet && playerStatus.CurrentEnergy >= 15)))
								{
									ZetaDia.Me.UsePower(SNOPower.Monk_BlindingFlash, vCurrentDestination, iCurrentWorldID, -1);
									return RunStatus.Running;
								}
							else if (DateTime.Now.Subtract(WeaponSwapTime).TotalMilliseconds >= 1500 || DateTime.Now.Subtract(WeaponSwapTime).TotalMilliseconds >= 800 
									&& GilesHasBuff(SNOPower.Monk_SweepingWind))
							{
								weaponSwap.SwapGear();
							}
						}
						// Spam sweeping winds
						if (hashCachedPowerHotbarAbilities.Contains(SNOPower.Monk_SweepingWind) && (playerStatus.CurrentEnergy >= 75 || (Settings.Combat.Monk.HasInnaSet && playerStatus.CurrentEnergy >= 5))
							&& (GilesHasBuff(SNOPower.Monk_SweepingWind) && DateTime.Now.Subtract(SweepWindSpam).TotalMilliseconds >= 3700 && DateTime.Now.Subtract(SweepWindSpam).TotalMilliseconds < 5100
							|| !GilesHasBuff(SNOPower.Monk_SweepingWind) && weaponSwap.DpsGearOn() && Settings.Combat.Monk.SweepingWindWeaponSwap && 
							DateTime.Now.Subtract(WeaponSwapTime).TotalMilliseconds >= 400))
						{
							ZetaDia.Me.UsePower(SNOPower.Monk_SweepingWind, vCurrentDestination, iCurrentWorldID, -1);
							SweepWindSpam = DateTime.Now;
							return RunStatus.Running;
						}
					}
					
                    // Interact/use power on target if already in range
                    if (fRangeRequired <= 0f || fDistanceFromTarget <= fRangeRequired && currentTargetIsInLoS)
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
                                    if (currentPower.SNOPower != SNOPower.None)
                                    {
                                        // Force waiting for global cooldown timer or long-animation abilities
                                        if (currentPower.iForceWaitLoopsBefore >= 1 || (currentPower.bWaitWhileAnimating != SIGNATURE_SPAM && DateTime.Now.Subtract(lastGlobalCooldownUse).TotalMilliseconds <= 50))
                                        {
                                            bWaitingForPower = true;
                                            if (currentPower.iForceWaitLoopsBefore >= 1)
                                                currentPower.iForceWaitLoopsBefore--;
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
                                    if (currentPower.SNOPower != SNOPower.None)
                                    {
                                        if (CurrentTarget.Type == GObjectType.Barricade)
                                        {
                                            DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.Behavior,
                                                "Barricade: Name={0}. SNO={1}, Range={2}. Needed range={3}. Radius={4}. Type={5}. Using power={6}",
                                                CurrentTarget.InternalName,     // 0
                                                CurrentTarget.ActorSNO,         // 1
                                                CurrentTarget.CentreDistance,   // 2
                                                fRangeRequired,                 // 3
                                                CurrentTarget.Radius,           // 4
                                                CurrentTarget.Type,             // 5
                                                currentPower.SNOPower           // 6
                                                );
                                        }
                                        else
                                        {
                                            DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.Behavior,
                                                "Destructible: Name={0}. SNO={1}, Range={2}. Needed range={3}. Radius={4}. Type={5}. Using power={6}",
                                                CurrentTarget.InternalName,     // 0
                                                CurrentTarget.ActorSNO,         // 1
                                                CurrentTarget.CentreDistance,   // 2
                                                fRangeRequired,                 // 3 
                                                CurrentTarget.Radius,           // 4
                                                CurrentTarget.Type,             // 5
                                                currentPower.SNOPower           // 6
                                                );
                                        }

                                        WaitWhileAnimating(12, true);

                                        if (CurrentTarget.RActorGuid == IgnoreRactorGUID || hashDestructableLocationTarget.Contains(CurrentTarget.ActorSNO))
                                        {
                                            // Location attack - attack the Vector3/map-area (equivalent of holding shift and left-clicking the object in-game to "force-attack")
                                            Vector3 vAttackPoint;
                                            if (CurrentTarget.CentreDistance >= 6f)
                                                vAttackPoint = MathEx.CalculatePointFrom(CurrentTarget.Position, playerStatus.CurrentPosition, 6f);
                                            else
                                                vAttackPoint = CurrentTarget.Position;

                                            vAttackPoint.Z += 1.5f;
                                            DbHelper.Log(TrinityLogLevel.Debug, LogCategory.Behavior, "(NB: Attacking location of destructable)");
                                            ZetaDia.Me.UsePower(currentPower.SNOPower, vAttackPoint, iCurrentWorldID, -1);
                                        }
                                        else
                                        {
                                            // Standard attack - attack the ACDGUID (equivalent of left-clicking the object in-game)
                                            ZetaDia.Me.UsePower(currentPower.SNOPower, vNullLocation, -1, CurrentTarget.ACDGuid);
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
                                        if (iInteractAttempts > 3)
                                        {
                                            hashRGUIDBlacklist90.Add(CurrentTarget.RActorGuid);
                                            //dateSinceBlacklist90Clear = DateTime.Now;
                                        }
                                        dictAbilityLastUse[currentPower.SNOPower] = DateTime.Now;
                                        currentPower.SNOPower = SNOPower.None;
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
                    // Out-of-range, so move towards the target
                    UpdateStatusTextTarget(false);
                    // Are we currently incapacitated? If so then wait...
                    if (playerStatus.IsIncapacitated || playerStatus.IsRooted)
                    {
                        return RunStatus.Running;
                    }
                    // Some stuff to avoid spamming usepower EVERY loop, and also to detect stucks/staying in one place for too long
                    // Count how long we have failed to move - body block stuff etc.
                    if (fDistanceFromTarget == fLastDistanceFromTarget)
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
                                fAvoidBlacklistDirection = FindDirectionDegree(playerStatus.CurrentPosition, CurrentTarget.Position);
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
                            return RunStatus.Running;
                        }
                        // Been 250 milliseconds of non-movement?
                    }
                    else
                    {
                        // Movement has been made, so count the time last moved!
                        lastMovedDuringCombat = DateTime.Now;
                    }
                    // Update the last distance stored
                    fLastDistanceFromTarget = fDistanceFromTarget;
                    // See if there's an obstacle in our way, if so try to navigate around it
                    if (CurrentTarget.Type != GObjectType.Avoidance)
                    {
                        Vector3 point = vCurrentDestination;
                        foreach (GilesObstacle tempobstacle in GilesTrinity.hashNavigationObstacleCache.Where(cp =>
                                        GilesTrinity.GilesIntersectsPath(cp.Location, cp.Radius, playerStatus.CurrentPosition, point) &&
                                        cp.Location.Distance(playerStatus.CurrentPosition) > GilesPlayerMover.GetObstacleNavigationSize(cp)))
                        {
                            if (vShiftedPosition == Vector3.Zero)
                            {
                                if (DateTime.Now.Subtract(lastShiftedPosition).TotalSeconds >= 10)
                                {
                                    float fDirectionToTarget = GilesTrinity.FindDirectionDegree(playerStatus.CurrentPosition, vCurrentDestination);
                                    vCurrentDestination = MathEx.GetPointAt(playerStatus.CurrentPosition, 15f, MathEx.ToRadians(fDirectionToTarget - 50));
                                    if (!GilesCanRayCast(playerStatus.CurrentPosition, vCurrentDestination, NavCellFlags.AllowWalk))
                                    {
                                        vCurrentDestination = MathEx.GetPointAt(playerStatus.CurrentPosition, 15f, MathEx.ToRadians(fDirectionToTarget + 50));
                                        if (!GilesCanRayCast(playerStatus.CurrentPosition, vCurrentDestination, NavCellFlags.AllowWalk))
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




                    // Only position-shift when not avoiding
                    // See if we want to ACTUALLY move, or are just waiting for the last move command...
                    if (!bForceNewMovement && bAlreadyMoving && vCurrentDestination == vLastMoveToTarget && DateTime.Now.Subtract(lastMovementCommand).TotalMilliseconds <= 100)
                    {
                        return RunStatus.Running;
                    }
                    // If we're doing avoidance, globes or backtracking, try to use special abilities to move quicker
                    if ((CurrentTarget.Type == GObjectType.Avoidance ||
                        CurrentTarget.Type == GObjectType.Globe ||
                        (CurrentTarget.Type == GObjectType.Backtrack && Settings.Combat.Misc.AllowOOCMovement))
                        && GilesCanRayCast(playerStatus.CurrentPosition, vCurrentDestination, NavCellFlags.AllowWalk)
                        )
                    {
                        bool bFoundSpecialMovement = UsedSpecialMovement();

                        if (CurrentTarget.Type != GObjectType.Backtrack)
                        {
                            // Whirlwind for a barb
                            //intell
                            if (!bWaitingForSpecial && currentPower.SNOPower != SNOPower.Barbarian_WrathOfTheBerserker && !bFoundSpecialMovement && hashPowerHotbarAbilities.Contains(SNOPower.Barbarian_Whirlwind) && playerStatus.CurrentEnergy >= 10)
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
                            if (!bFoundSpecialMovement && hashPowerHotbarAbilities.Contains(SNOPower.Monk_TempestRush) && playerStatus.CurrentEnergy >= 20)
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
                            if (!bFoundSpecialMovement && hashPowerHotbarAbilities.Contains(SNOPower.DemonHunter_Strafe) && playerStatus.CurrentEnergy >= 15)
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
                    // Whirlwind against everything within range (except backtrack points)
                    //intell
                    if (hashPowerHotbarAbilities.Contains(SNOPower.Barbarian_Whirlwind) && playerStatus.CurrentEnergy >= 10 && iAnythingWithinRange[RANGE_20] >= 1 && !bWaitingForSpecial && currentPower.SNOPower != SNOPower.Barbarian_WrathOfTheBerserker && fDistanceFromTarget <= 12f && CurrentTarget.Type != GObjectType.Container && CurrentTarget.Type != GObjectType.Backtrack &&
                        (!hashPowerHotbarAbilities.Contains(SNOPower.Barbarian_Sprint) || GilesHasBuff(SNOPower.Barbarian_Sprint)) &&
                        CurrentTarget.Type != GObjectType.Backtrack &&
                        (CurrentTarget.Type != GObjectType.Item && CurrentTarget.Type != GObjectType.Gold && fDistanceFromTarget >= 6f) &&
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

        private static Stack<KeyValuePair<int, DateTime>> BlackListStack = new Stack<KeyValuePair<int, DateTime>>(20);

        /// <summary>
        /// Handles target blacklist assignment if necessary, used for all targets (units/gold/items/interactables)
        /// </summary>
        /// <param name="runStatus"></param>
        /// <returns></returns>
        private static HandlerRunStatus HandleTargetTimeout(HandlerRunStatus runStatus)
        {
            // Been trying to handle the same target for more than 30 seconds without damaging/reaching it? Blacklist it!
            // Note: The time since target picked updates every time the current target loses health, if it's a monster-target
            // Don't blacklist stuff if we're playing a cutscene

            if (!ZetaDia.IsPlayingCutscene && CurrentTargetIsNotAvoidance() && (
                        (CurrentTargetIsNonUnit() && GetSecondsSinceTargetUpdate() > 6) ||
                        (CurrentTargetIsUnit() && GetSecondsSinceTargetUpdate() > 15)))
            {
                // NOTE: This only blacklists if it's remained the PRIMARY TARGET that we are trying to actually directly attack!
                // So it won't blacklist a monster "on the edge of the screen" who isn't even being targetted
                // Don't blacklist monsters on <= 50% health though, as they can't be in a stuck location... can they!? Maybe give them some extra time!

                bool isNavigable = pf.IsNavigable(gp.WorldToGrid(CurrentTarget.Position.ToVector2()));
                bool bBlacklistThis = true;

                // PREVENT blacklisting a monster on less than 90% health unless we haven't damaged it for more than 2 minutes
                if (CurrentTarget.Type == GObjectType.Unit && isNavigable)
                {
                    if (CurrentTarget.IsTreasureGoblin && Settings.Combat.Misc.GoblinPriority >= GoblinPriority.Kamikaze)
                        bBlacklistThis = false;
                    //if (CurrentTarget.iHitPoints <= 0.90 && DateTime.Now.Subtract(dateSincePickedTarget).TotalSeconds <= 30)
                    //    bBlacklistThis = false;
                    //if (CurrentTarget.bIsBoss)
                    //    bBlacklistThis = false;
                }
                if (bBlacklistThis)
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

        /// <summary>
        /// Checks to see if we need a new monster power and will assign it to <see cref="currentPower"/>, distinguishes destructables/barricades from units
        /// </summary>
        private static void AssignMonsterTargetPower()
        {
            // Find a valid ability if the target is a monster
            if (bPickNewAbilities && !bWaitingForPower && !bWaitingForPotion)
            {
                bPickNewAbilities = false;
                if (CurrentTarget.Type == GObjectType.Unit)
                {
                    // Pick a suitable ability
                    currentPower = GilesAbilitySelector(false, false, false);
                    if (currentPower.SNOPower == SNOPower.None && !playerStatus.IsIncapacitated)
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
                    currentPower = GilesAbilitySelector(false, false, true);
            }
        }

        /// <summary>
        /// Will check <see cref=" bWaitingForPotion"/> and Use a Potion if needed
        /// </summary>
        private static void UseHealthPotionIfNeeded()
        {
            if (bWaitingForPotion)
            {
                bWaitingForPotion = false;
                if (!playerStatus.IsIncapacitated && GilesUseTimer(SNOPower.DrinkHealthPotion))
                {
                    ACDItem thisBestPotion = ZetaDia.Me.Inventory.Backpack.Where(i => i.IsPotion).OrderByDescending(p => p.HitpointsGranted).FirstOrDefault();
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

        /// <summary>
        /// Determines if we need a cache refresh, or just a current target health check
        /// </summary>
        private static void CheckStaleCache()
        {
            // Let's calculate whether or not we want a new target list...
            if (!bWholeNewTarget && !bWaitingForPower && !bWaitingForPotion)
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

        /// <summary>
        /// If we can use special class movement abilities, this will use it and return true
        /// </summary>
        /// <returns></returns>
        private static bool UsedSpecialMovement()
        {
            // Log whether we used a  (for avoidance really)
            bool bFoundSpecialMovement = false;
            // Leap movement for a barb
            if (!bFoundSpecialMovement && hashPowerHotbarAbilities.Contains(SNOPower.Barbarian_Leap) &&
                DateTime.Now.Subtract(dictAbilityLastUse[SNOPower.Barbarian_Leap]).TotalMilliseconds >= dictAbilityRepeatDelay[SNOPower.Barbarian_Leap] &&
                PowerManager.CanCast(SNOPower.Barbarian_Leap))
            {
                WaitWhileAnimating(3, true);
                ZetaDia.Me.UsePower(SNOPower.Barbarian_Leap, vCurrentDestination, iCurrentWorldID, -1);
                dictAbilityLastUse[SNOPower.Barbarian_Leap] = DateTime.Now;
                bFoundSpecialMovement = true;
            }
            // Furious Charge movement for a barb
            if (!bFoundSpecialMovement && hashPowerHotbarAbilities.Contains(SNOPower.Barbarian_FuriousCharge) &&
                DateTime.Now.Subtract(dictAbilityLastUse[SNOPower.Barbarian_FuriousCharge]).TotalMilliseconds >= dictAbilityRepeatDelay[SNOPower.Barbarian_FuriousCharge] &&
                PowerManager.CanCast(SNOPower.Barbarian_FuriousCharge))
            {
                WaitWhileAnimating(3, true);
                ZetaDia.Me.UsePower(SNOPower.Barbarian_FuriousCharge, vCurrentDestination, iCurrentWorldID, -1);
                dictAbilityLastUse[SNOPower.Barbarian_FuriousCharge] = DateTime.Now;
                bFoundSpecialMovement = true;
            }
            // Vault for a Demon Hunter
            if (!bFoundSpecialMovement && hashPowerHotbarAbilities.Contains(SNOPower.DemonHunter_Vault) &&
                //DateTime.Now.Subtract(dictAbilityLastUse[SNOPower.DemonHunter_Vault]).TotalMilliseconds >= dictAbilityRepeatDelay[SNOPower.DemonHunter_Vault] &&
                DateTime.Now.Subtract(GilesTrinity.dictAbilityLastUse[SNOPower.DemonHunter_Vault]).TotalMilliseconds >= GilesTrinity.Settings.Combat.DemonHunter.VaultMovementDelay &&
                PowerManager.CanCast(SNOPower.DemonHunter_Vault) &&
                (PlayerKiteDistance <= 0 || (!hashMonsterObstacleCache.Any(a => a.Location.Distance(vCurrentDestination) <= PlayerKiteDistance) &&
                !hashAvoidanceObstacleCache.Any(a => a.Location.Distance(vCurrentDestination) <= PlayerKiteDistance))) &&
                (!GilesTrinity.hashAvoidanceObstacleCache.Any(a => MathEx.IntersectsPath(a.Location, a.Radius, GilesTrinity.playerStatus.CurrentPosition, vCurrentDestination)))
                )
            {
                WaitWhileAnimating(3, true);
                ZetaDia.Me.UsePower(SNOPower.DemonHunter_Vault, vCurrentDestination, iCurrentWorldID, -1);
                dictAbilityLastUse[SNOPower.DemonHunter_Vault] = DateTime.Now;
                bFoundSpecialMovement = true;
            }
            // Teleport for a wizard (need to be able to check skill rune in DB for a 3-4 teleport spam in a row)
            if (!bFoundSpecialMovement && hashPowerHotbarAbilities.Contains(SNOPower.Wizard_Teleport) &&
                DateTime.Now.Subtract(dictAbilityLastUse[SNOPower.Wizard_Teleport]).TotalMilliseconds >= dictAbilityRepeatDelay[SNOPower.Wizard_Teleport] &&
                playerStatus.CurrentEnergy >= 15 &&
                PowerManager.CanCast(SNOPower.Wizard_Teleport))
            {
                WaitWhileAnimating(3, true);
                ZetaDia.Me.UsePower(SNOPower.Wizard_Teleport, vCurrentDestination, iCurrentWorldID, -1);
                dictAbilityLastUse[SNOPower.Wizard_Teleport] = DateTime.Now;
                bFoundSpecialMovement = true;
            }
            // Archon Teleport for a wizard (need to be able to check skill rune in DB for a 3-4 teleport spam in a row)
            if (!bFoundSpecialMovement && hashPowerHotbarAbilities.Contains(SNOPower.Wizard_Archon_Teleport) &&
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
                            if (!GilesCanRayCast(playerStatus.CurrentPosition, CurrentTarget.Position, NavCellFlags.AllowWalk))
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
            statusText.Append(CurrentTarget.CentreDistance.ToString("0"));
            statusText.Append(". R-Dist=");
            statusText.Append(Math.Round(CurrentTarget.RadiusDistance, 2));
            statusText.Append(". RangeReq'd: ");
            statusText.Append(fRangeRequired.ToString("0"));
            statusText.Append(". ");
            if (CurrentTarget.Type == GObjectType.Unit && currentPower.SNOPower != SNOPower.None)
            {
                statusText.Append("Power=");
                statusText.Append(currentPower.SNOPower);
                statusText.Append(" (range ");
                statusText.Append(fRangeRequired);
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
            // Now for the actual movement request stuff
            bAlreadyMoving = true;
            lastMovementCommand = DateTime.Now;
            if (DateTime.Now.Subtract(lastSentMovePower).TotalMilliseconds >= 250 || Vector3.Distance(vLastMoveToTarget, vCurrentDestination) >= 2f || bForceNewMovement)
            {
                //ZetaDia.Me.UsePower(SNOPower.Walk, vCurrentDestination, iCurrentWorldID, -1);
                //Navigator.MoveTo(vCurrentDestination, null, true);
                //Navigator.PlayerMover.MoveTowards(vCurrentDestination);

                //ZetaDia.Me.Movement.MoveActor(vCurrentDestination);
                Zeta.Navigation.Navigator.MoveTo(vCurrentDestination, CurrentTarget.InternalName, true);
                lastSentMovePower = DateTime.Now;

                // Store the current destination for comparison incase of changes next loop
                vLastMoveToTarget = vCurrentDestination;
                // Reset total body-block count, since we should have moved
                if (DateTime.Now.Subtract(lastForcedKeepCloseRange).TotalMilliseconds >= 2000)
                    TimesBlockedMoving = 0;
            }
        }

        private static void SetRangeRequiredForTarget()
        {
            fRangeRequired = 1f;
            fDistanceReduction = 0f;
            // Set current destination to our current target's destination
            vCurrentDestination = CurrentTarget.Position;
            float fDistanceToDestination = playerStatus.CurrentPosition.Distance(vCurrentDestination);
            switch (CurrentTarget.Type)
            {
                // * Unit, we need to pick an ability to use and get within range
                case GObjectType.Unit:
                    {
                        // Treat the distance as closer based on the radius of monsters
                        fDistanceReduction = CurrentTarget.Radius;
                        if (ForceCloseRangeTarget)
                            fDistanceReduction -= 3f;
                        if (fDistanceReduction <= 0f)
                            fDistanceReduction = 0f;
                        // Pick a range to try to reach
                        fRangeRequired = currentPower.SNOPower == SNOPower.None ? 9f : currentPower.iMinimumRange;
                        break;
                    }
                // * Item - need to get within 6 feet and then interact with it
                case GObjectType.Item:
                    {
                        fRangeRequired = 6f;

                        break;
                    }
                // * Gold - need to get within pickup radius only
                case GObjectType.Gold:
                    {
                        fRangeRequired = (float)ZetaDia.Me.GoldPickUpRadius;
                        if (fRangeRequired < 2f)
                            fRangeRequired = 2f;
                        break;
                    }
                // * Globes - need to get within pickup radius only
                case GObjectType.Globe:
                    {
                        fRangeRequired = (float)ZetaDia.Me.GoldPickUpRadius;
                        if (fRangeRequired < 2f)
                            fRangeRequired = 2f;
                        if (fRangeRequired > 5f)
                            fRangeRequired = 5f;
                        break;
                    }
                // * Shrine & Container - need to get within 8 feet and interact
                case GObjectType.HealthWell:
                    {
                        fRangeRequired = CurrentTarget.Radius + 5f;
                        fRangeRequired = 5f;
                        int _range;
                        if (dictInteractableRange.TryGetValue(CurrentTarget.ActorSNO, out _range))
                        {
                            fRangeRequired = (float)_range;
                        }
                        break;
                    }
                case GObjectType.Shrine:
                case GObjectType.Container:
                    {
                        // Treat the distance as closer based on the radius of the object
                        fDistanceReduction = CurrentTarget.Radius;
                        fRangeRequired = 8f;
                        if (ForceCloseRangeTarget)
                            fRangeRequired -= 2f;
                        // Treat the distance as closer if the X & Y distance are almost point-blank, for objects
                        if (fDistanceToDestination <= 1.5f)
                            fDistanceReduction += 1f;
                        int iTempRange;
                        if (dictInteractableRange.TryGetValue(CurrentTarget.ActorSNO, out iTempRange))
                        {
                            fRangeRequired = (float)iTempRange;
                        }
                        break;
                    }
                case GObjectType.Interactable:
                    {
                        // Treat the distance as closer based on the radius of the object
                        fDistanceReduction = CurrentTarget.Radius;
                        fRangeRequired = 12f;
                        if (ForceCloseRangeTarget)
                            fRangeRequired -= 2f;
                        // Check if it's in our interactable range dictionary or not
                        int iTempRange;
                        if (dictInteractableRange.TryGetValue(CurrentTarget.ActorSNO, out iTempRange))
                        {
                            fRangeRequired = (float)iTempRange;
                        }
                        // Treat the distance as closer if the X & Y distance are almost point-blank, for objects
                        if (fDistanceToDestination <= 1.5f)
                            fDistanceReduction += 1f;
                        break;
                    }
                // * Destructible - need to pick an ability and attack it
                case GObjectType.Destructible:
                case GObjectType.Barricade:
                    {
                        // Pick a range to try to reach + (tmp_fThisRadius * 0.70);
                        fRangeRequired = currentPower.SNOPower == SNOPower.None ? 9f : currentPower.iMinimumRange;
                        fDistanceReduction = CurrentTarget.Radius;
                        if (ForceCloseRangeTarget)
                            fDistanceReduction -= 3f;
                        if (fDistanceReduction <= 0f)
                            fDistanceReduction = 0f;
                        // Treat the distance as closer if the X & Y distance are almost point-blank, for destructibles
                        if (fDistanceToDestination <= 1.5f)
                            fDistanceReduction += 1f;
                        break;
                    }
                // * Avoidance - need to pick an avoid location and move there
                case GObjectType.Avoidance:
                    {
                        fRangeRequired = 2f;
                        // Treat the distance as closer if the X & Y distance are almost point-blank, for avoidance spots
                        if (fDistanceToDestination <= 1.5f)
                            fDistanceReduction += 2f;
                        break;
                    }
                // * Backtrack Destination
                case GObjectType.Backtrack:
                    {
                        fRangeRequired = 5f;
                        if (ForceCloseRangeTarget)
                            fRangeRequired -= 2f;
                        break;
                    }
                case GObjectType.Door:
                    fRangeRequired = CurrentTarget.Radius + 2f;
                    break;
                default:
                    fRangeRequired = CurrentTarget.Radius;
                    break;
            }
        }

        private static void HandleUnitInRange()
        {
            bWaitingForPower = false;
            // Wait while animating before an attack
            if (currentPower.bWaitWhileAnimating)
                WaitWhileAnimating(5, false);
            // Use the power
            bool bUsePowerSuccess = false;
            // Note that whirlwinds use an off-on-off-on to avoid spam
            if (currentPower.SNOPower != SNOPower.Barbarian_Whirlwind && currentPower.SNOPower != SNOPower.DemonHunter_Strafe)
            {
                ZetaDia.Me.UsePower(currentPower.SNOPower, currentPower.vTargetLocation, currentPower.iTargetWorldID, currentPower.iTargetGUID);
                bUsePowerSuccess = true;
                lastChangedZigZag = DateTime.Today;
                vPositionLastZigZagCheck = Vector3.Zero;
            }
            else
            {
                // Special code to prevent whirlwind double-spam, this helps save fury
                bool bUseThisLoop = currentPower.SNOPower != powerLastSnoPowerUsed;
                if (!bUseThisLoop)
                {
                    //powerLastSnoPowerUsed = SNOPower.None;
                    if (DateTime.Now.Subtract(dictAbilityLastUse[currentPower.SNOPower]).TotalMilliseconds >= 200)
                        bUseThisLoop = true;
                }
                if (bUseThisLoop)
                {
                    ZetaDia.Me.UsePower(currentPower.SNOPower, currentPower.vTargetLocation, currentPower.iTargetWorldID, currentPower.iTargetGUID);
                    bUsePowerSuccess = true;
                }
            }
            if (bUsePowerSuccess)
            {
                dictAbilityLastUse[currentPower.SNOPower] = DateTime.Now;
                lastGlobalCooldownUse = DateTime.Now;
                powerLastSnoPowerUsed = currentPower.SNOPower;
                currentPower.SNOPower = SNOPower.None;
                // Wait for animating AFTER the attack
                if (currentPower.bWaitWhileAnimating)
                    WaitWhileAnimating(3, false);
            }
            // Wait for animating AFTER the attack
            if (currentPower.bWaitWhileAnimating)
                WaitWhileAnimating(3, false);
            bPickNewAbilities = true;
            // Keep looking for monsters at "normal kill range" a few moments after we successfully attack a monster incase we can pull them into range
            iKeepKillRadiusExtendedFor = 8;
            timeKeepKillRadiusExtendedUntil = DateTime.Now.AddSeconds(iKeepKillRadiusExtendedFor);
            iKeepLootRadiusExtendedFor = 8;
            // if at full or nearly full health, see if we can raycast to it, if not, ignore it for 2000 ms
            if (CurrentTarget.HitPoints >= 0.9d && iAnythingWithinRange[RANGE_50] > 3)
            {
                if (!GilesCanRayCast(playerStatus.CurrentPosition, CurrentTarget.Position, NavCellFlags.AllowWalk))
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
            bWaitingAfterPower = false;
            if (currentPower.iForceWaitLoopsAfter >= 1)
            {
                bWaitingAfterPower = true;
            }
        }

        private static int HandleItemInRange()
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
            if (!_hashsetItemPicksLookedAt.Contains(CurrentTarget.RActorGuid))
            {
                _hashsetItemPicksLookedAt.Add(CurrentTarget.RActorGuid);
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
                    if ((CurrentTarget.Level < 0) || (CurrentTarget.Level >= 64))
                    {
                        DbHelper.Log(TrinityLogLevel.Normal, LogCategory.UserInformation, "ERROR: Item level (" + CurrentTarget.Level + ") out of range");
                    }
                    ItemsPickedStats.TotalPerQuality[iQuality]++;
                    ItemsPickedStats.TotalPerLevel[CurrentTarget.Level]++;
                    ItemsPickedStats.TotalPerQPerL[iQuality, CurrentTarget.Level]++;
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
                    if ((CurrentTarget.Level < 0) || (CurrentTarget.Level > 63))
                    {
                        DbHelper.Log(TrinityLogLevel.Normal, LogCategory.UserInformation, "ERROR: Gem level ({0}) out of range", CurrentTarget.Level);
                    }

                    ItemsPickedStats.GemsPerType[iGemType]++;
                    ItemsPickedStats.GemsPerLevel[CurrentTarget.Level]++;
                    ItemsPickedStats.GemsPerTPerL[iGemType, CurrentTarget.Level]++;
                }
                else if (thisgilesitemtype == GItemType.HealthPotion)
                {
                    ItemsPickedStats.TotalPotions++;
                    if ((CurrentTarget.Level < 0) || (CurrentTarget.Level > 63))
                    {
                        DbHelper.Log(TrinityLogLevel.Normal, LogCategory.UserInformation, "ERROR: Potion level ({0}) out of range", CurrentTarget.Level);
                    }
                    ItemsPickedStats.PotionsPerLevel[CurrentTarget.Level]++;
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
