using GilesTrinity.DbProvider;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Zeta;
using Zeta.Common;
using Zeta.Common.Plugins;
using Zeta.CommonBot;
using Zeta.Internals.Actors;
using Zeta.Internals.SNO;
using Zeta.TreeSharp;
using Action = Zeta.TreeSharp.Action;
using Decorator = Zeta.TreeSharp.Decorator;
namespace GilesTrinity
{
    public partial class GilesTrinity : IPlugin
    {
        /*
         * Blank decorator and action to wipe some of DB's behavior trees
         */
        private static bool GilesBlankDecorator(object ret)
        {
            return false;
        }
        private static RunStatus GilesBlankAction(object ret)
        {
            return RunStatus.Success;
        }

        public static DiaActivePlayer Me
        {
            get { return ZetaDia.Me; }
        }

        /*
         * Start the process of moving to target object and dealing with it 
         */

        private static Composite HandleTargetAction()
        {
            return new PrioritySelector(
                new Decorator(ret => ZetaDia.IsInGame && !ZetaDia.IsLoadingWorld && !bMainBotPaused,

                    new Action(ctx => GilesHandleTarget(ctx))
                )
            );
        }

        private static RunStatus GilesHandleTarget(object ret)
        {
            try
            {
                // Make sure we reset unstucker stuff here
                GilesPlayerMover.iTimesReachedStuckPoint = 0;
                GilesPlayerMover.vSafeMovementLocation = Vector3.Zero;
                GilesPlayerMover.timeLastRecordedPosition = DateTime.Now;

                // Whether we should refresh the target list or not
                bool bShouldRefreshDiaObjects = false;
                // See if we should update hotbar abilities
                if (bRefreshHotbarAbilities)
                {
                    GilesRefreshHotbar(GilesHasBuff(SNOPower.Wizard_Archon));
                }
                if (powerPrime == null)
                    powerPrime = GilesAbilitySelector();
                // Special pausing *AFTER* using certain powers
                if (bWaitingAfterPower && powerPrime.iForceWaitLoopsAfter >= 1)
                {
                    if (powerPrime.iForceWaitLoopsAfter >= 1)
                        powerPrime.iForceWaitLoopsAfter--;
                    if (powerPrime.iForceWaitLoopsAfter <= 0)
                        bWaitingAfterPower = false;
                    return RunStatus.Running;
                }
                // Update player-data cache
                UpdateCachedPlayerData();
                // Check for death / player being dead
                if (playerStatus.CurrentHealthPct <= 0)
                {
                    return RunStatus.Success;
                }
                // See if we have been "newly rooted", to force target updates
                if (playerStatus.IsRooted && !bWasRootedLastTick)
                {
                    bWasRootedLastTick = true;
                    bForceTargetUpdate = true;
                }
                if (!playerStatus.IsRooted)
                    bWasRootedLastTick = false;
                if (CurrentTarget == null)
                {
                    Logging.WriteDiagnostic("[Trinity] targetCurrent was passed as null!");
                }
                // Let's calculate whether or not we want a new target list...
                if (!bWholeNewTarget && !bWaitingForPower && !bWaitingForPotion)
                {
                    // Update targets at least once every 80 milliseconds
                    if (bForceTargetUpdate || bTravellingAvoidance || DateTime.Now.Subtract(lastRefreshedObjects).TotalMilliseconds >= 80)
                    {
                        bShouldRefreshDiaObjects = true;
                    }
                    // If we AREN'T getting new targets - find out if we SHOULD because the current unit has died etc.
                    if (!bShouldRefreshDiaObjects && CurrentTarget.Type == GilesObjectType.Unit)
                    {
                        if (CurrentTarget.Unit == null || CurrentTarget.Unit.BaseAddress == IntPtr.Zero)
                        {
                            bShouldRefreshDiaObjects = true;
                        }
                        else
                        {
                            // health calculations
                            double dThisMaxHealth;
                            // Get the max health of this unit, a cached version if available, if not cache it
                            if (!dictGilesMaxHealthCache.TryGetValue(c_iRActorGuid, out dThisMaxHealth))
                            {
                                try
                                {
                                    dThisMaxHealth = CurrentTarget.Unit.CommonData.GetAttribute<float>(ActorAttributeType.HitpointsMax);
                                    dictGilesMaxHealthCache.Add(c_iRActorGuid, dThisMaxHealth);
                                }
                                catch
                                {
                                    Logging.WriteDiagnostic("[Trinity] Safely handled exception getting attribute max health #2 for unit " + c_sName + " [" + c_iActorSNO.ToString() + "]");
                                    bShouldRefreshDiaObjects = true;
                                }
                            }
                            // Ok check we didn't fail getting the maximum health, now try to get live current health...
                            if (!bShouldRefreshDiaObjects)
                            {
                                try
                                {
                                    double dTempHitpoints = (CurrentTarget.Unit.CommonData.GetAttribute<float>(ActorAttributeType.HitpointsCur) / dThisMaxHealth);
                                    if (dTempHitpoints <= 0d)
                                    {
                                        bShouldRefreshDiaObjects = true;
                                    }
                                    else
                                    {
                                        CurrentTarget.HitPoints = dTempHitpoints;
                                        CurrentTarget.Position = CurrentTarget.Unit.Position;
                                    }
                                }
                                catch
                                {
                                    bShouldRefreshDiaObjects = true;
                                }
                            }
                        }
                    }
                }
                // So, after all that, do we actually want a new target list?
                if (!bWholeNewTarget && !bWaitingForPower && !bWaitingForPotion)
                {
                    // If we *DO* want a new target list, do this... 
                    if (bShouldRefreshDiaObjects)
                    {
                        // Now call the function that refreshes targets
                        RefreshDiaObjectCache();
                        // Update when we last refreshed with current time
                        lastRefreshedObjects = DateTime.Now;
                        // No target, return success
                        if (CurrentTarget == null)
                        {
                            return RunStatus.Success;
                        }
                        // Make sure we start trying to move again should we need to!
                        bAlreadyMoving = false;
                        lastMovementCommand = DateTime.Today;
                        bPickNewAbilities = true;
                    }
                    // Ok we didn't want a new target list, should we at least update the position of the current target, if it's a monster?
                    else if (CurrentTarget.Type == GilesObjectType.Unit && CurrentTarget.Unit != null && CurrentTarget.Unit.BaseAddress != IntPtr.Zero)
                    {
                        try
                        {
                            CurrentTarget.Position = CurrentTarget.Unit.Position;
                        }
                        catch
                        {
                            // Keep going anyway if we failed to get a new position from DemonBuddy
                            Logging.WriteDiagnostic("GSDEBUG: Caught position read failure in main target handler loop.");
                        }
                    }
                }
                // Been trying to handle the same target for more than 30 seconds without damaging/reaching it? Blacklist it!
                // Note: The time since target picked updates every time the current target loses health, if it's a monster-target
                if (CurrentTargetIsNotAvoidance() && (
                            (CurrentTargetIsNonUnit() && GetSecondsSinceTargetAssigned() > 6) ||
                            (CurrentTargetIsUnit() && GetSecondsSinceTargetAssigned() > 15)))
                {
                    // NOTE: This only blacklists if it's remained the PRIMARY TARGET that we are trying to actually directly attack!
                    // So it won't blacklist a monster "on the edge of the screen" who isn't even being targetted
                    // Don't blacklist monsters on <= 50% health though, as they can't be in a stuck location... can they!? Maybe give them some extra time!
                    bool isNavigable = pf.IsNavigable(gp.WorldToGrid(CurrentTarget.Position.ToVector2()));
                    bool bBlacklistThis = true;
                    // PREVENT blacklisting a monster on less than 90% health unless we haven't damaged it for more than 2 minutes
                    if (CurrentTarget.Type == GilesObjectType.Unit && isNavigable)
                    {
                        if (CurrentTarget.IsTreasureGoblin && settings.iTreasureGoblinPriority >= 3)
                            bBlacklistThis = false;
                        //if (CurrentTarget.iHitPoints <= 0.90 && DateTime.Now.Subtract(dateSincePickedTarget).TotalSeconds <= 30)
                        //    bBlacklistThis = false;
                        //if (CurrentTarget.bIsBoss)
                        //    bBlacklistThis = false;
                    }
                    if (bBlacklistThis)
                    {
                        if (CurrentTarget.Type == GilesObjectType.Unit)
                        {
                            Logging.Write("[Trinity] Blacklisting a monster because of possible stuck issues. Monster=" + CurrentTarget.InternalName + " {" +
                                CurrentTarget.ActorSNO.ToString() + "}. Range=" + CurrentTarget.CentreDistance.ToString("0") + ", health %=" + CurrentTarget.HitPoints.ToString("0")
                                );
                        }
                        else
                        {
                            Logging.Write("[Trinity] Blacklisting an object because of possible stuck issues. Object=" + CurrentTarget.InternalName + " {" +
                                CurrentTarget.ActorSNO + "}. Range=" + CurrentTarget.CentreDistance.ToString("0"));
                        }

                        if (CurrentTarget.IsBoss)
                        {
                            hashRGUIDIgnoreBlacklist15.Add(CurrentTarget.RActorGuid);
                            dateSinceBlacklist15Clear = DateTime.Now;
                        }
                        else
                        {
                            hashRGUIDIgnoreBlacklist90.Add(CurrentTarget.RActorGuid);
                            //dateSinceBlacklist90Clear = DateTime.Now;
                            CurrentTarget = null;
                            return RunStatus.Success;
                        }
                    }
                }
                // This variable just prevents an instant 2-target update after coming here from the main decorator function above
                bWholeNewTarget = false;
                // Find a valid ability if the target is a monster
                if (bPickNewAbilities && !bWaitingForPower && !bWaitingForPotion)
                {
                    bPickNewAbilities = false;
                    if (CurrentTarget.Type == GilesObjectType.Unit)
                    {
                        // Pick a suitable ability
                        powerPrime = GilesAbilitySelector(false, false, false);
                        if (powerPrime.SNOPower == SNOPower.None && !playerStatus.IsIncapacitated)
                        {
                            iNoAbilitiesAvailableInARow++;
                            if (DateTime.Now.Subtract(lastRemindedAboutAbilities).TotalSeconds > 60 && iNoAbilitiesAvailableInARow >= 4)
                            {
                                lastRemindedAboutAbilities = DateTime.Now;
                                Logging.Write("Fatal Error: Couldn't find a valid attack ability. Not enough resource for any abilities or all on cooldown");
                                Logging.Write("If you get this message frequently, you should consider changing your build");
                                Logging.Write("Perhaps you don't have enough critical hit chance % for your current build, or just have a bad skill setup?");
                            }
                        }
                        else
                        {
                            iNoAbilitiesAvailableInARow = 0;
                        }
                    }
                    // Select an ability for destroying a destructible with in advance
                    if (CurrentTarget.Type == GilesObjectType.Destructible || CurrentTarget.Type == GilesObjectType.Barricade)
                        powerPrime = GilesAbilitySelector(false, false, true);
                }
                // Pop a potion when necessary
                // Note that we force a single-loop pause first, to help potion popping "go off"
                if (playerStatus.CurrentHealthPct <= iEmergencyHealthPotionLimit && !bWaitingForPower && !bWaitingForPotion && !playerStatus.IsIncapacitated && GilesUseTimer(SNOPower.DrinkHealthPotion))
                {
                    bWaitingForPotion = true;
                    return RunStatus.Running;
                }
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
                // See if we can use any special buffs etc. while in avoidance
                if (CurrentTarget.Type == GilesObjectType.Avoidance)
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
                GilesHandleStepSetRangeRequired();
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
                if (CurrentTarget.Type == GilesObjectType.Unit && settings.bEnableBacktracking &&
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
                // Interact/use power on target if already in range
                if (fRangeRequired <= 0f || fDistanceFromTarget <= fRangeRequired)
                {
                    // If avoidance, instantly skip
                    if (CurrentTarget.Type == GilesObjectType.Avoidance)
                    {
                        //vlastSafeSpot = vNullLocation;
                        bForceTargetUpdate = true;
                        bAvoidDirectionBlacklisting = false;
                        return RunStatus.Running;
                    }
                    GilesHandleStepLogInteraction();
                    // An integer to log total interact attempts on a particular object or item
                    int iInteractAttempts;
                    switch (CurrentTarget.Type)
                    {
                        // Unit, use our primary power to attack
                        case GilesObjectType.Unit:
                            if (powerPrime.SNOPower != SNOPower.None)
                            {
                                // Force waiting for global cooldown timer or long-animation abilities
                                if (powerPrime.iForceWaitLoopsBefore >= 1 || (powerPrime.bWaitWhileAnimating != SIGNATURE_SPAM && DateTime.Now.Subtract(lastGlobalCooldownUse).TotalMilliseconds <= 50))
                                {
                                    //Logging.WriteDiagnostic("Debug: Force waiting BEFORE ability " + powerPrime.powerThis.ToString() + "...");
                                    bWaitingForPower = true;
                                    if (powerPrime.iForceWaitLoopsBefore >= 1)
                                        powerPrime.iForceWaitLoopsBefore--;
                                    return RunStatus.Running;
                                }
                                GilesHandleStepUnit();
                                return RunStatus.Running;
                            }
                            return RunStatus.Running;
                        // Item, interact with it and log item stats
                        case GilesObjectType.Item:
                            {
                                // Check if we actually have room for this item first
                                Vector2 ValidLocation = FindValidBackpackLocation(true);
                                if (ValidLocation.X < 0 || ValidLocation.Y < 0)
                                {
                                    Logging.Write("No more space to pickup a 2-slot item, town-run requested at next free moment.");
                                    bGilesForcedVendoring = true;
                                    return RunStatus.Success;
                                }
                                iInteractAttempts = GilesHandleStepItem();
                            }
                            return RunStatus.Running;
                        // * Gold & Globe - need to get within pickup radius only
                        case GilesObjectType.Gold:
                        case GilesObjectType.Globe:
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
                                hashRGUIDIgnoreBlacklist90.Add(CurrentTarget.RActorGuid);
                                //dateSinceBlacklist90Clear = DateTime.Now;
                                hashRGUIDIgnoreBlacklist60.Add(CurrentTarget.RActorGuid);
                            }
                            iIgnoreThisRactorGUID = CurrentTarget.RActorGuid;
                            iIgnoreThisForLoops = 3;
                            // Now tell Trinity to get a new target!
                            lastChangedZigZag = DateTime.Today;
                            vPositionLastZigZagCheck = Vector3.Zero;
                            bForceTargetUpdate = true;
                            return RunStatus.Running;
                        // * Shrine & Container - need to get within 8 feet and interact
                        case GilesObjectType.Door:
                        case GilesObjectType.HealthWell:
                        case GilesObjectType.Shrine:
                        case GilesObjectType.Container:
                        case GilesObjectType.Interactable:
                            WaitWhileAnimating(5, true);
                            ZetaDia.Me.UsePower(SNOPower.Axe_Operate_Gizmo, Vector3.Zero, 0, CurrentTarget.ACDGuid);
                            //iIgnoreThisRactorGUID = CurrentTarget.iRActorGuid;
                            //iIgnoreThisForLoops = 2;
                            // Interactables can have a long channeling time...
                            if (CurrentTarget.Type == GilesObjectType.Interactable)
                                WaitWhileAnimating(1500, true);
                            else
                                WaitWhileAnimating(12, true);
                            if (CurrentTarget.Type == GilesObjectType.Interactable)
                            {
                                iIgnoreThisForLoops = 30;
                                hashRGUIDIgnoreBlacklist60.Add(CurrentTarget.RActorGuid);
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
                            if ((iInteractAttempts > 5 || (CurrentTarget.Type == GilesObjectType.Interactable && iInteractAttempts > 3)) &&
                                !(CurrentTarget.Type != GilesObjectType.HealthWell))
                            {
                                hashRGUIDIgnoreBlacklist90.Add(CurrentTarget.RActorGuid);
                                //dateSinceBlacklist90Clear = DateTime.Now;
                            }
                            // Now tell Trinity to get a new target!
                            lastChangedZigZag = DateTime.Today;
                            vPositionLastZigZagCheck = Vector3.Zero;
                            bForceTargetUpdate = true;
                            return RunStatus.Running;
                        // * Destructible - need to pick an ability and attack it
                        case GilesObjectType.Destructible:
                        case GilesObjectType.Barricade:
                            {
                                if (powerPrime.SNOPower != SNOPower.None)
                                {
                                    if (CurrentTarget.Type == GilesObjectType.Barricade)
                                        Logging.WriteDiagnostic("[Trinity] Barricade: Name=" + CurrentTarget.InternalName + ". SNO=" + CurrentTarget.ActorSNO.ToString() +
                                            ", Range=" + CurrentTarget.CentreDistance.ToString() + ". Needed range=" + fRangeRequired.ToString() + ". Radius=" +
                                            CurrentTarget.Radius.ToString() + ". Type=" + CurrentTarget.Type.ToString() + ". Using power=" + powerPrime.SNOPower.ToString());
                                    else
                                        Logging.WriteDiagnostic("[Trinity] Destructible: Name=" + CurrentTarget.InternalName + ". SNO=" + CurrentTarget.ActorSNO.ToString() +
                                            ", Range=" + CurrentTarget.CentreDistance.ToString() + ". Needed range=" + fRangeRequired.ToString() + ". Radius=" +
                                            CurrentTarget.Radius.ToString() + ". Type=" + CurrentTarget.Type.ToString() + ". Using power=" + powerPrime.SNOPower.ToString());
                                    WaitWhileAnimating(12, true);
                                    if (CurrentTarget.RActorGuid == iIgnoreThisRactorGUID || hashDestructableLocationTarget.Contains(CurrentTarget.ActorSNO))
                                    {
                                        // Location attack - attack the Vector3/map-area (equivalent of holding shift and left-clicking the object in-game to "force-attack")
                                        Vector3 vAttackPoint;
                                        if (CurrentTarget.CentreDistance >= 6f)
                                            vAttackPoint = MathEx.CalculatePointFrom(CurrentTarget.Position, playerStatus.CurrentPosition, 6f);
                                        else
                                            vAttackPoint = CurrentTarget.Position;
                                        vAttackPoint.Z += 1.5f;
                                        Logging.WriteDiagnostic("[Trinity] (NB: Attacking location of destructable)");
                                        ZetaDia.Me.UsePower(powerPrime.SNOPower, vAttackPoint, iCurrentWorldID, -1);
                                    }
                                    else
                                    {
                                        // Standard attack - attack the ACDGUID (equivalent of left-clicking the object in-game)
                                        ZetaDia.Me.UsePower(powerPrime.SNOPower, vNullLocation, -1, CurrentTarget.ACDGuid);
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
                                        hashRGUIDIgnoreBlacklist90.Add(CurrentTarget.RActorGuid);
                                        //dateSinceBlacklist90Clear = DateTime.Now;
                                    }
                                    dictAbilityLastUse[powerPrime.SNOPower] = DateTime.Now;
                                    powerPrime.SNOPower = SNOPower.None;
                                    WaitWhileAnimating(6, true);
                                    // Prevent this EXACT object being targetted again for a short while, just incase
                                    iIgnoreThisRactorGUID = CurrentTarget.RActorGuid;
                                    iIgnoreThisForLoops = 3;
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
                        case GilesObjectType.Backtrack:
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
                GilesHandleStepMoveIntoRange();
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
                        iTimesBlockedMoving++;
                        bForceCloseRangeTarget = true;
                        lastForcedKeepCloseRange = DateTime.Now;
                        // And tell Trinity to get a new target
                        bForceTargetUpdate = true;
                        // Blacklist an 80 degree direction for avoidance
                        if (CurrentTarget.Type == GilesObjectType.Avoidance)
                        {
                            bAvoidDirectionBlacklisting = true;
                            fAvoidBlacklistDirection = FindDirectionDegree(playerStatus.CurrentPosition, CurrentTarget.Position);
                        }
                        // Handle body blocking by blacklisting
                        GilesHandleBodyBlocking();
                        // If we were backtracking and failed, remove the current backtrack and try and move to the next
                        if (CurrentTarget.Type == GilesObjectType.Backtrack && iTimesBlockedMoving >= 2)
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
                if (CurrentTarget.Type != GilesObjectType.Avoidance)
                {
                    Vector3 point = vCurrentDestination;
                    foreach (GilesObstacle tempobstacle in GilesTrinity.hashNavigationObstacleCache.Where(cp =>
                                    GilesTrinity.GilesIntersectsPath(cp.Location, cp.Radius, playerStatus.CurrentPosition, point) &&
                                    cp.Location.Distance(playerStatus.CurrentPosition) > GilesTrinity.dictSNONavigationSize[cp.SNOID]))
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
                                    Logging.WriteDiagnostic("[Trinity] Mid-Target-Handle position shift location to: " + vCurrentDestination.ToString() + " (was " + point.ToString() + ")");
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
                if ((CurrentTarget.Type == GilesObjectType.Avoidance ||
                    CurrentTarget.Type == GilesObjectType.Globe ||
                    (CurrentTarget.Type == GilesObjectType.Backtrack && settings.bOutOfCombatMovementPowers))
                    && GilesCanRayCast(playerStatus.CurrentPosition, vCurrentDestination, NavCellFlags.AllowWalk)
                    )
                {
                    bool bFoundSpecialMovement = CanUseSpecialMovement();
                    if (CurrentTarget.Type != GilesObjectType.Backtrack)
                    {
                        // Whirlwind for a barb
                        //intell
                        if (!bWaitingForSpecial && powerPrime.SNOPower != SNOPower.Barbarian_WrathOfTheBerserker && !bFoundSpecialMovement && hashPowerHotbarAbilities.Contains(SNOPower.Barbarian_Whirlwind) && playerStatus.CurrentEnergy >= 10)
                        {
                            ZetaDia.Me.UsePower(SNOPower.Barbarian_Whirlwind, vCurrentDestination, iCurrentWorldID, -1);
                            // Store the current destination for comparison incase of changes next loop
                            vLastMoveToTarget = vCurrentDestination;
                            // Reset total body-block count, since we should have moved
                            if (DateTime.Now.Subtract(lastForcedKeepCloseRange).TotalMilliseconds >= 2000)
                                iTimesBlockedMoving = 0;
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
                                iTimesBlockedMoving = 0;
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
                                iTimesBlockedMoving = 0;
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
                            iTimesBlockedMoving = 0;
                        return RunStatus.Running;
                    }
                }
                // Whirlwind against everything within range (except backtrack points)
                //intell
                if (hashPowerHotbarAbilities.Contains(SNOPower.Barbarian_Whirlwind) && playerStatus.CurrentEnergy >= 10 && iAnythingWithinRange[RANGE_20] >= 1 && !bWaitingForSpecial && powerPrime.SNOPower != SNOPower.Barbarian_WrathOfTheBerserker && fDistanceFromTarget <= 12f && CurrentTarget.Type != GilesObjectType.Container && CurrentTarget.Type != GilesObjectType.Backtrack &&
                    (!hashPowerHotbarAbilities.Contains(SNOPower.Barbarian_Sprint) || GilesHasBuff(SNOPower.Barbarian_Sprint)) &&
                    CurrentTarget.Type != GilesObjectType.Backtrack &&
                    (CurrentTarget.Type != GilesObjectType.Item && CurrentTarget.Type != GilesObjectType.Gold && fDistanceFromTarget >= 6f) &&
                    (CurrentTarget.Type != GilesObjectType.Unit ||
                    (CurrentTarget.Type == GilesObjectType.Unit && !CurrentTarget.IsTreasureGoblin &&
                        (!settings.bSelectiveWhirlwind || bAnyNonWWIgnoreMobsInRange || !hashActorSNOWhirlwindIgnore.Contains(CurrentTarget.ActorSNO)))))
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
                    if ((!bForceCloseRangeTarget || DateTime.Now.Subtract(lastForcedKeepCloseRange).TotalMilliseconds > iMillisecondsForceCloseRange) &&
                        DateTime.Now.Subtract(lastForcedKeepCloseRange).TotalMilliseconds >= 2000)
                        iTimesBlockedMoving = 0;
                    return RunStatus.Running;
                }
            }
            catch (Exception ex)
            {
                Logging.WriteDiagnostic(ex.ToString());
                Logging.WriteException(ex);
                return RunStatus.Failure;
            }

            GilesHandleTargetBasicMovement(bForceNewMovement);

            return RunStatus.Running;

        }
        // GilesMoveToTarget()
        private static bool CanUseSpecialMovement()
        {
            // Log whether we used a special movement (for avoidance really)
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
                DateTime.Now.Subtract(dictAbilityLastUse[SNOPower.DemonHunter_Vault]).TotalMilliseconds >= dictAbilityRepeatDelay[SNOPower.DemonHunter_Vault] &&
                PowerManager.CanCast(SNOPower.DemonHunter_Vault))
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
            return CurrentTarget.Type != GilesObjectType.Avoidance;
        }
        private static bool CurrentTargetIsNonUnit()
        {
            return CurrentTarget.Type != GilesObjectType.Unit;
        }
        private static bool CurrentTargetIsUnit()
        {
            return CurrentTarget.Type == GilesObjectType.Unit;
        }
        private static double GetSecondsSinceTargetAssigned()
        {
            return DateTime.Now.Subtract(dateSincePickedTarget).TotalSeconds;
        }
        private static void GilesHandleBodyBlocking()
        {
            // Tell target finder to prioritize close-combat targets incase we were bodyblocked
            switch (iTimesBlockedMoving)
            {
                case 1:
                    iMillisecondsForceCloseRange = 850;
                    break;
                case 2:
                    iMillisecondsForceCloseRange = 1300;
                    // Cancel avoidance attempts for 500ms
                    iMillisecondsCancelledEmergencyMoveFor = 1500;
                    timeCancelledEmergencyMove = DateTime.Now;
                    vlastSafeSpot = vNullLocation;
                    // Check for raycastability against objects
                    switch (CurrentTarget.Type)
                    {
                        case GilesObjectType.Container:
                        case GilesObjectType.Shrine:
                        case GilesObjectType.Globe:
                        case GilesObjectType.Gold:
                        case GilesObjectType.Item:
                            // No raycast available, try and force-ignore this for a little while, and blacklist for a few seconds
                            if (!GilesCanRayCast(playerStatus.CurrentPosition, CurrentTarget.Position, NavCellFlags.AllowWalk))
                            {
                                iIgnoreThisRactorGUID = CurrentTarget.RActorGuid;
                                iIgnoreThisForLoops = 6;
                                hashRGUIDIgnoreBlacklist60.Add(CurrentTarget.RActorGuid);
                                //dateSinceBlacklist90Clear = DateTime.Now;
                            }
                            break;
                    }
                    break;
                case 3:
                    iMillisecondsForceCloseRange = 2000;
                    // Cancel avoidance attempts for 1.5 seconds
                    iMillisecondsCancelledEmergencyMoveFor = 2000;
                    timeCancelledEmergencyMove = DateTime.Now;
                    vlastSafeSpot = vNullLocation;
                    // Blacklist the current avoidance target area for the next avoidance-spot find
                    if (CurrentTarget.Type == GilesObjectType.Avoidance)
                        hashAvoidanceBlackspot.Add(new GilesObstacle(CurrentTarget.Position, 12f, -1, 0));
                    break;
                default:
                    iMillisecondsForceCloseRange = 4000;
                    // Cancel avoidance attempts for 3.5 seconds
                    iMillisecondsCancelledEmergencyMoveFor = 4000;
                    timeCancelledEmergencyMove = DateTime.Now;
                    vlastSafeSpot = vNullLocation;
                    // Blacklist the current avoidance target area for the next avoidance-spot find
                    if (iTimesBlockedMoving == 4 && CurrentTarget.Type == GilesObjectType.Avoidance)
                        hashAvoidanceBlackspot.Add(new GilesObstacle(CurrentTarget.Position, 16f, -1, 0));
                    break;
            }
        }
        private static void GilesHandleStepMoveIntoRange()
        {
            sStatusText = "[Trinity] ";
            switch (CurrentTarget.Type)
            {
                case GilesObjectType.Avoidance:
                    sStatusText += "Avoid ";
                    break;
                case GilesObjectType.Unit:
                    sStatusText += "Attack ";
                    break;
                case GilesObjectType.Item:
                case GilesObjectType.Gold:
                case GilesObjectType.Globe:
                    sStatusText += "Pickup ";
                    break;
                case GilesObjectType.Backtrack:
                    sStatusText += "Backtrack ";
                    break;
                case GilesObjectType.Interactable:
                    sStatusText += "Interact ";
                    break;
                case GilesObjectType.Door:
                case GilesObjectType.Container:
                    sStatusText += "Open ";
                    break;
                case GilesObjectType.Destructible:
                case GilesObjectType.Barricade:
                    sStatusText += "Destroy ";
                    break;
                case GilesObjectType.Shrine:
                    sStatusText += "Click ";
                    break;
            }
            sStatusText += "Target=" + CurrentTarget.InternalName + " {" + CurrentTarget.ActorSNO + "}. ";
            sStatusText += "Type=" + CurrentTarget.Type + " C-Dist=" + CurrentTarget.CentreDistance.ToString("0") + ". ";
            sStatusText += "R-Dist=" + Math.Round(CurrentTarget.RadiusDistance, 2).ToString() + ". ";
            sStatusText += "RangeReq'd: " + fRangeRequired.ToString("0") + ". ";
            if (CurrentTarget.Type == GilesObjectType.Unit && powerPrime.SNOPower != SNOPower.None)
                sStatusText += "Power=" + powerPrime.SNOPower.ToString() + " (range " + fRangeRequired.ToString() + ") ";
            sStatusText += "Weight=" + CurrentTarget.Weight.ToString() + " MOVING INTO RANGE";
            if (settings.bDebugInfo)
            {
                BotMain.StatusText = sStatusText;
                Logging.WriteDiagnostic(sStatusText);
            }
            bResetStatusText = true;
        }
        private static void GilesHandleStepLogInteraction()
        {
            sStatusText = "[Trinity] ";
            switch (CurrentTarget.Type)
            {
                case GilesObjectType.Avoidance:
                    sStatusText += "Avoid ";
                    break;
                case GilesObjectType.Unit:
                    sStatusText += "Attack ";
                    break;
                case GilesObjectType.Item:
                case GilesObjectType.Gold:
                case GilesObjectType.Globe:
                    sStatusText += "Pickup ";
                    break;
                case GilesObjectType.Backtrack:
                    sStatusText += "Backtrack ";
                    break;
                case GilesObjectType.Interactable:
                    sStatusText += "Interact ";
                    break;
                case GilesObjectType.Door:
                case GilesObjectType.Container:
                    sStatusText += "Open ";
                    break;
                case GilesObjectType.Destructible:
                case GilesObjectType.Barricade:
                    sStatusText += "Destroy ";
                    break;
                case GilesObjectType.HealthWell:
                case GilesObjectType.Shrine:
                    sStatusText += "Click ";
                    break;
            }
            sStatusText += "Target=" + CurrentTarget.InternalName + " {" + CurrentTarget.ActorSNO + "}. C-Dist=" + Math.Round(CurrentTarget.CentreDistance, 2).ToString() + ". " +
                "R-Dist=" + Math.Round(CurrentTarget.RadiusDistance, 2).ToString() + ". ";
            if (CurrentTarget.Type == GilesObjectType.Unit && powerPrime.SNOPower != SNOPower.None)
                sStatusText += "Power=" + powerPrime.SNOPower.ToString() + " (range " + fRangeRequired.ToString() + ") ";
            sStatusText += "Weight=" + Math.Round(CurrentTarget.Weight, 2).ToString() + " IN RANGE, NOW INTERACTING";
            if (settings.bDebugInfo)
            {
                BotMain.StatusText = sStatusText;
                Logging.WriteDiagnostic(sStatusText);
            }
            bResetStatusText = true;
        }
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

                ZetaDia.Me.Movement.MoveActor(vCurrentDestination);
                lastSentMovePower = DateTime.Now;

                // Store the current destination for comparison incase of changes next loop
                vLastMoveToTarget = vCurrentDestination;
                // Reset total body-block count, since we should have moved
                if (DateTime.Now.Subtract(lastForcedKeepCloseRange).TotalMilliseconds >= 2000)
                    iTimesBlockedMoving = 0;
            }
        }
        private static void GilesHandleStepSetRangeRequired()
        {
            fRangeRequired = 1f;
            fDistanceReduction = 0f;
            // Set current destination to our current target's destination
            vCurrentDestination = CurrentTarget.Position;
            float fDistanceToDestination = playerStatus.CurrentPosition.Distance(vCurrentDestination);
            switch (CurrentTarget.Type)
            {
                // * Unit, we need to pick an ability to use and get within range
                case GilesObjectType.Unit:
                    {
                        // Treat the distance as closer based on the radius of monsters
                        fDistanceReduction = CurrentTarget.Radius;
                        if (bForceCloseRangeTarget)
                            fDistanceReduction -= 3f;
                        if (fDistanceReduction <= 0f)
                            fDistanceReduction = 0f;
                        // Pick a range to try to reach
                        fRangeRequired = powerPrime.SNOPower == SNOPower.None ? 9f : powerPrime.iMinimumRange;
                        break;
                    }
                // * Item - need to get within 6 feet and then interact with it
                case GilesObjectType.Item:
                    {
                        fRangeRequired = 5f;
                        // If we're having stuck issues, try forcing us to get closer to this item
                        if (bForceCloseRangeTarget)
                            fRangeRequired -= 1f;
                        // Try and randomize the distances required if we have problems looting this
                        if (iIgnoreThisRactorGUID == CurrentTarget.RActorGuid)
                        {
                            Random rndNum = new Random(int.Parse(Guid.NewGuid().ToString().Substring(0, 8), NumberStyles.HexNumber));
                            fRangeRequired = (rndNum.Next(5)) + 2f;
                        }
                        // Treat the distance as closer if the X & Y distance are almost point-blank, for items
                        if (fDistanceToDestination <= 1.5f)
                            fDistanceReduction += 1f;
                        break;
                    }
                // * Gold - need to get within pickup radius only
                case GilesObjectType.Gold:
                    {
                        fRangeRequired = (float)ZetaDia.Me.GoldPickUpRadius;
                        if (fRangeRequired < 2f)
                            fRangeRequired = 2f;
                        break;
                    }
                // * Globes - need to get within pickup radius only
                case GilesObjectType.Globe:
                    {
                        fRangeRequired = (float)ZetaDia.Me.GoldPickUpRadius;
                        if (fRangeRequired < 2f)
                            fRangeRequired = 2f;
                        if (fRangeRequired > 5f)
                            fRangeRequired = 5f;
                        break;
                    }
                // * Shrine & Container - need to get within 8 feet and interact
                case GilesObjectType.HealthWell:
                    {
                        fRangeRequired = CurrentTarget.Radius + 5f;
                        fRangeRequired = 5f;
                        int iTempRange;
                        if (dictInteractableRange.TryGetValue(CurrentTarget.ActorSNO, out iTempRange))
                        {
                            fRangeRequired = (float)iTempRange;
                        }
                        break;
                    }
                case GilesObjectType.Shrine:
                case GilesObjectType.Container:
                    {
                        // Treat the distance as closer based on the radius of the object
                        fDistanceReduction = CurrentTarget.Radius;
                        fRangeRequired = 8f;
                        if (bForceCloseRangeTarget)
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
                case GilesObjectType.Interactable:
                    {
                        // Treat the distance as closer based on the radius of the object
                        fDistanceReduction = CurrentTarget.Radius;
                        fRangeRequired = 12f;
                        if (bForceCloseRangeTarget)
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
                case GilesObjectType.Destructible:
                case GilesObjectType.Barricade:
                    {
                        // Pick a range to try to reach + (tmp_fThisRadius * 0.70);
                        fRangeRequired = powerPrime.SNOPower == SNOPower.None ? 9f : powerPrime.iMinimumRange;
                        fDistanceReduction = CurrentTarget.Radius;
                        if (bForceCloseRangeTarget)
                            fDistanceReduction -= 3f;
                        if (fDistanceReduction <= 0f)
                            fDistanceReduction = 0f;
                        // Treat the distance as closer if the X & Y distance are almost point-blank, for destructibles
                        if (fDistanceToDestination <= 1.5f)
                            fDistanceReduction += 1f;
                        break;
                    }
                // * Avoidance - need to pick an avoid location and move there
                case GilesObjectType.Avoidance:
                    {
                        fRangeRequired = 2f;
                        // Treat the distance as closer if the X & Y distance are almost point-blank, for avoidance spots
                        if (fDistanceToDestination <= 1.5f)
                            fDistanceReduction += 2f;
                        break;
                    }
                // * Backtrack Destination
                case GilesObjectType.Backtrack:
                    {
                        fRangeRequired = 5f;
                        if (bForceCloseRangeTarget)
                            fRangeRequired -= 2f;
                        break;
                    }
                case GilesObjectType.Door:
                    fRangeRequired = CurrentTarget.Radius + 2f;
                    break;
                default:
                    fRangeRequired = CurrentTarget.Radius;
                    break;
            }
        }
        private static void GilesHandleStepUnit()
        {
            bWaitingForPower = false;
            // Wait while animating before an attack
            if (powerPrime.bWaitWhileAnimating)
                WaitWhileAnimating(5, false);
            // Use the power
            bool bUsePowerSuccess = false;
            // Note that whirlwinds use an off-on-off-on to avoid spam
            if (powerPrime.SNOPower != SNOPower.Barbarian_Whirlwind && powerPrime.SNOPower != SNOPower.DemonHunter_Strafe)
            {
                ZetaDia.Me.UsePower(powerPrime.SNOPower, powerPrime.vTargetLocation, powerPrime.iTargetWorldID, powerPrime.iTargetGUID);
                bUsePowerSuccess = true;
                lastChangedZigZag = DateTime.Today;
                vPositionLastZigZagCheck = Vector3.Zero;
            }
            else
            {
                // Special code to prevent whirlwind double-spam, this helps save fury
                bool bUseThisLoop = powerPrime.SNOPower != powerLastSnoPowerUsed;
                if (!bUseThisLoop)
                {
                    //powerLastSnoPowerUsed = SNOPower.None;
                    if (DateTime.Now.Subtract(dictAbilityLastUse[powerPrime.SNOPower]).TotalMilliseconds >= 200)
                        bUseThisLoop = true;
                }
                if (bUseThisLoop)
                {
                    ZetaDia.Me.UsePower(powerPrime.SNOPower, powerPrime.vTargetLocation, powerPrime.iTargetWorldID, powerPrime.iTargetGUID);
                    bUsePowerSuccess = true;
                }
            }
            if (bUsePowerSuccess)
            {
                //Logging.WriteDiagnostic("Used power {0} at {1} on target {2} successfully", powerPrime.SNOPower, powerPrime.vTargetLocation, powerPrime.iTargetGUID);
                dictAbilityLastUse[powerPrime.SNOPower] = DateTime.Now;
                lastGlobalCooldownUse = DateTime.Now;
                powerLastSnoPowerUsed = powerPrime.SNOPower;
                powerPrime.SNOPower = SNOPower.None;
                // Wait for animating AFTER the attack
                if (powerPrime.bWaitWhileAnimating)
                    WaitWhileAnimating(3, false);
            }
            else
            {
                //Logging.Write(powerPrime.powerThis.ToString() + " reported failure");
                //dictAbilityLastFailed[powerPrime.powerThis] = DateTime.Now;
                //*Logging.WriteDiagnostic("GSDebug: Skill use apparently failed=" + powerPrime.powerThis.ToString() + ", against enemy: " + targetCurrent.sThisInternalName +
                //    " (skill use range=" + powerPrime.iMinimumRange.ToString() + ", enemy centre range=" + targetCurrent.fCentreDistance.ToString() + ", radius range=" +
                //    targetCurrent.fRadiusDistance.ToString() + " (radius=" + targetCurrent.fThisRadius.ToString() + ")");*/
            }
            // Wait for animating AFTER the attack
            if (powerPrime.bWaitWhileAnimating)
                WaitWhileAnimating(3, false);
            bPickNewAbilities = true;
            // Keep looking for monsters at "normal kill range" a few moments after we successfully attack a monster incase we can pull them into range
            iKeepKillRadiusExtendedFor = 8;
            iKeepLootRadiusExtendedFor = 8;
            // if at full or nearly full health, see if we can raycast to it, if not, ignore it for 2000 ms
            if (CurrentTarget.HitPoints >= 0.9d && iAnythingWithinRange[RANGE_50] > 3)
            {
                if (!GilesCanRayCast(playerStatus.CurrentPosition, CurrentTarget.Position, NavCellFlags.AllowWalk))
                {
                    iIgnoreThisRactorGUID = CurrentTarget.RActorGuid;
                    iIgnoreThisForLoops = 6;
                    // Add this monster to our very short-term ignore list
                    if (!CurrentTarget.IsBoss)
                    {
                        hashRGuid3SecBlacklist.Add(CurrentTarget.RActorGuid);
                        lastTemporaryBlacklist = DateTime.Now;
                        bNeedClearTemporaryBlacklist = true;
                    }
                }
            }
            // See if we should force a long wait AFTERWARDS, too
            // Force waiting AFTER power use for certain abilities
            bWaitingAfterPower = false;
            if (powerPrime.iForceWaitLoopsAfter >= 1)
            {
                //Logging.WriteDiagnostic("Force waiting AFTER ability " + powerPrime.powerThis.ToString() + "...");
                bWaitingAfterPower = true;
            }
        }
        private static int GilesHandleStepItem()
        {
            int iInteractAttempts;
            // Pick the item up the usepower way, and "blacklist" for a couple of loops
            WaitWhileAnimating(12, true);
            ZetaDia.Me.UsePower(SNOPower.Axe_Operate_Gizmo, Vector3.Zero, 0, CurrentTarget.ACDGuid);
            lastChangedZigZag = DateTime.Today;
            vPositionLastZigZagCheck = Vector3.Zero;
            iIgnoreThisRactorGUID = CurrentTarget.RActorGuid;
            iIgnoreThisForLoops = 3;
            // Store item pickup stats
            if (!_hashsetItemPicksLookedAt.Contains(CurrentTarget.RActorGuid))
            {
                _hashsetItemPicksLookedAt.Add(CurrentTarget.RActorGuid);
                GilesItemType thisgilesitemtype = DetermineItemType(CurrentTarget.InternalName, CurrentTarget.DBItemType, CurrentTarget.FollowerType);
                GilesBaseItemType thisgilesbasetype = DetermineBaseType(thisgilesitemtype);
                if (thisgilesbasetype == GilesBaseItemType.Armor || thisgilesbasetype == GilesBaseItemType.WeaponOneHand || thisgilesbasetype == GilesBaseItemType.WeaponTwoHand ||
                    thisgilesbasetype == GilesBaseItemType.WeaponRange || thisgilesbasetype == GilesBaseItemType.Jewelry || thisgilesbasetype == GilesBaseItemType.FollowerItem ||
                    thisgilesbasetype == GilesBaseItemType.Offhand)
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
                    ItemsPickedStats.TotalPerQuality[iQuality]++;
                    ItemsPickedStats.TotalPerLevel[CurrentTarget.Level]++;
                    ItemsPickedStats.TotalPerQPerL[iQuality, CurrentTarget.Level]++;
                }
                else if (thisgilesbasetype == GilesBaseItemType.Gem)
                {
                    int iGemType = 0;
                    ItemsPickedStats.TotalGems++;
                    if (thisgilesitemtype == GilesItemType.Topaz)
                        iGemType = GEMTOPAZ;
                    if (thisgilesitemtype == GilesItemType.Ruby)
                        iGemType = GEMRUBY;
                    if (thisgilesitemtype == GilesItemType.Emerald)
                        iGemType = GEMEMERALD;
                    if (thisgilesitemtype == GilesItemType.Amethyst)
                        iGemType = GEMAMETHYST;
                    ItemsPickedStats.GemsPerType[iGemType]++;
                    ItemsPickedStats.GemsPerLevel[CurrentTarget.Level]++;
                    ItemsPickedStats.GemsPerTPerL[iGemType, CurrentTarget.Level]++;
                }
                else if (thisgilesitemtype == GilesItemType.HealthPotion)
                {
                    ItemsPickedStats.TotalPotions++;
                    ItemsPickedStats.PotionsPerLevel[CurrentTarget.Level]++;
                }
                else if (c_item_GilesItemType == GilesItemType.InfernalKey)
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
                hashRGUIDIgnoreBlacklist90.Add(CurrentTarget.RActorGuid);
                //dateSinceBlacklist90Clear = DateTime.Now;
            }
            // Now tell Trinity to get a new target!
            bForceTargetUpdate = true;
            return iInteractAttempts;
        }
    }
}
