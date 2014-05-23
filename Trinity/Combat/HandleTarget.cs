using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using Trinity.Combat.Abilities;
using Trinity.Config.Combat;
using Trinity.DbProvider;
using Trinity.Helpers;
using Trinity.Items;
using Trinity.Technicals;
using Zeta.Bot;
using Zeta.Bot.Navigation;
using Zeta.Common;
using Zeta.Game;
using Zeta.Game.Internals.Actors;
using Zeta.Game.Internals.SNO;
using Zeta.TreeSharp;
using Action = Zeta.TreeSharp.Action;
using Decorator = Zeta.TreeSharp.Decorator;
using Logger = Trinity.Technicals.Logger;

namespace Trinity
{
    public partial class Trinity
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
        internal static Composite HandleTargetAction()
        {
            return new PrioritySelector(
                new Decorator(ret => ZetaDia.IsInGame && !ZetaDia.IsLoadingWorld && !MainBotPaused,
                    new Action(ctx => HandleTarget())
                ),
                new Decorator(ret => ZetaDia.IsInGame && !ZetaDia.IsLoadingWorld && MainBotPaused,
                    new Action(ret => PausedAction())
                )
            );
        }

        private static RunStatus PausedAction()
        {
            return MainBotPaused ? RunStatus.Running : RunStatus.Success;
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

        private static bool _staleCache;

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
                default:
                    throw new ApplicationException("Unable to return Non-TreeSharp RunStatus");
            }
            string extras = "";
            if (_isWaitingForPower)
                extras += " IsWaitingForPower";
            if (_isWaitingAfterPower)
                extras += " IsWaitingAfterPower";
            if (_isWaitingForPotion)
                extras += " IsWaitingForPotion";
            if (TownRun.IsTryingToTownPortal())
                extras += " IsTryingToTownPortal";
            if (TownRun.TownRunTimerRunning())
                extras += " TownRunTimerRunning";
            if (TownRun.TownRunTimerFinished())
                extras += " TownRunTimerFinished";
            if (_forceTargetUpdate)
                extras += " ForceTargetUpdate";
            if (CurrentTarget == null)
                extras += " CurrentTargetIsNull";
            if (CombatBase.CurrentPower != null && CombatBase.CurrentPower.ShouldWaitBeforeUse)
                extras += " CPowerShouldWaitBefore=" + (CombatBase.CurrentPower.WaitBeforeUseDelay - CombatBase.CurrentPower.TimeSinceAssigned);
            if (CombatBase.CurrentPower != null && CombatBase.CurrentPower.ShouldWaitAfterUse)
                extras += " CPowerShouldWaitAfter=" + (CombatBase.CurrentPower.WaitAfterUseDelay - CombatBase.CurrentPower.TimeSinceUse);
            if (CombatBase.CurrentPower != null && (CombatBase.CurrentPower.ShouldWaitBeforeUse || CombatBase.CurrentPower.ShouldWaitAfterUse))
                extras += " " + CombatBase.CurrentPower;

            Logger.Log(TrinityLogLevel.Debug, LogCategory.Behavior, "Handle Target returning {0} to tree" + extras, treeRunStatus);
            return treeRunStatus;

        }

        /// <summary>
        /// Handles all aspects of moving to and attacking the current target
        /// </summary>
        /// <returns></returns>
        private static RunStatus HandleTarget()
        {
            HandlerRunStatus runStatus = HandlerRunStatus.NotFinished;
            using (new PerformanceLogger("HandleTarget"))
            {
                try
                {
                    if (!ZetaDia.IsInGame || !ZetaDia.Me.IsValid || ZetaDia.IsLoadingWorld)
                    {
                        Logger.Log(TrinityLogLevel.Error, LogCategory.UserInformation, "No longer in game world", true);
                        runStatus = HandlerRunStatus.TreeFailure;
                        return GetTreeSharpRunStatus(runStatus);
                    }
                    if (ZetaDia.Me.IsDead)
                    {
                        Logger.Log(TrinityLogLevel.Error, LogCategory.UserInformation, "Player is dead", true);
                        runStatus = HandlerRunStatus.TreeFailure;
                        return GetTreeSharpRunStatus(runStatus);
                    }

                    // Make sure we reset unstucker stuff here
                    PlayerMover.TimesReachedStuckPoint = 0;
                    PlayerMover.vSafeMovementLocation = Vector3.Zero;
                    PlayerMover.TimeLastRecordedPosition = DateTime.UtcNow;

                    // Whether we should refresh the target list or not
                    // See if we should update hotbar abilities
                    if (ShouldRefreshHotbarAbilities || HotbarRefreshTimer.ElapsedMilliseconds > 5000 ||
                         (CombatBase.TimeSincePowerUse(SNOPower.Wizard_Archon) < 20000))
                    {
                        PlayerInfoCache.RefreshHotbar();
                    }
                    if (!_isWaitingForPower && CombatBase.CurrentPower == null && CurrentTarget != null)
                        CombatBase.CurrentPower = AbilitySelector();

                    // Time based wait delay for certain powers with animations
                    if (_isWaitingAfterPower && CombatBase.CurrentPower.ShouldWaitAfterUse)
                    {
                        runStatus = HandlerRunStatus.TreeRunning;
                    }
                    else
                    {
                        _isWaitingAfterPower = false;
                    }

                    //check if we are returning to the tree
                    if (runStatus != HandlerRunStatus.NotFinished)
                        return GetTreeSharpRunStatus(runStatus);

                    // See if we have been "newly rooted", to force target updates
                    if (Player.IsRooted && !wasRootedLastTick)
                    {
                        wasRootedLastTick = true;
                        _forceTargetUpdate = true;
                    }
                    if (!Player.IsRooted)
                        wasRootedLastTick = false;
                    if (CurrentTarget == null)
                    {
                        Logger.Log(TrinityLogLevel.Debug, LogCategory.Behavior, "CurrentTarget was passed as null! Continuing...");
                    }

                    Monk_MaintainTempestRush();

                    CheckStaleCache();
                    using (new PerformanceLogger("HandleTarget.CheckForNewTarget"))
                    {
                        // So, after all that, do we actually want a new target list?
                        if (!_isWholeNewTarget && !_isWaitingForPower && !_isWaitingForPotion)
                        {
                            // If we *DO* want a new target list, do this... 
                            if (_staleCache)
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
                                    lastMovementCommand = DateTime.MinValue;
                                    _shouldPickNewAbilities = true;
                                }
                            }

                            UpdateCurrentTarget();
                        }
                    }

                    if (CurrentTarget == null && (ForceVendorRunASAP || IsReadyToTownRun) && !Zeta.Bot.Logic.BrainBehavior.IsVendoring && TownRun.TownRunTimerRunning())
                    {
                        Logger.Log(TrinityLogLevel.Info, LogCategory.Behavior, "CurrentTarget is null but we are ready to to Town Run, waiting... ");
                        runStatus = HandlerRunStatus.TreeRunning;
                    }

                    //check if we are returning to the tree
                    if (runStatus != HandlerRunStatus.NotFinished)
                        return GetTreeSharpRunStatus(runStatus);

                    if (CurrentTarget == null && TownRun.IsTryingToTownPortal() && TownRun.TownRunTimerRunning())
                    {
                        Logger.Log(TrinityLogLevel.Info, LogCategory.Behavior, "Waiting for town run... ");
                        runStatus = HandlerRunStatus.TreeRunning;
                    }
                    else if (CurrentTarget == null && TownRun.IsTryingToTownPortal() && TownRun.TownRunTimerFinished())
                    {
                        Logger.Log(TrinityLogLevel.Info, LogCategory.Behavior, "Town Run Ready!");
                        runStatus = HandlerRunStatus.TreeSuccess;
                    }
                    //check if we are returning to the tree
                    if (runStatus != HandlerRunStatus.NotFinished)
                        return GetTreeSharpRunStatus(runStatus);

                    if (CurrentTarget == null)
                    {
                        Logger.Log(TrinityLogLevel.Info, LogCategory.Behavior, "CurrentTarget set as null in refresh! Error 2");
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
                    _isWholeNewTarget = false;
                    if (CurrentTarget != null)
                        AssignMonsterTargetPower();

                    // Pop a potion when necessary

                    UsePotionIfNeeded();

                    //check if we are returning to the tree
                    if (runStatus != HandlerRunStatus.NotFinished)
                        return GetTreeSharpRunStatus(runStatus);

                    // If we just looped waiting for a potion, use it

                    using (new PerformanceLogger("HandleTarget.CheckAvoidanceBuffs"))
                    {
                        // See if we can use any special buffs etc. while in avoidance
                        if (CurrentTarget.Type == GObjectType.Avoidance)
                        {
                            powerBuff = AbilitySelector(true, false, false);
                            if (powerBuff.SNOPower != SNOPower.None)
                            {
                                ZetaDia.Me.UsePower(powerBuff.SNOPower, powerBuff.TargetPosition, powerBuff.TargetDynamicWorldId, powerBuff.TargetACDGUID);
                                LastPowerUsed = powerBuff.SNOPower;
                                CacheData.AbilityLastUsed[powerBuff.SNOPower] = DateTime.UtcNow;
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
                        if (DateTime.UtcNow.Subtract(LastAddedLocationCache).TotalMilliseconds >= 100)
                        {
                            LastAddedLocationCache = DateTime.UtcNow;
                            if (Vector3.Distance(Player.Position, LastRecordedPosition) >= 5f)
                            {
                                SkipAheadAreaCache.Add(new CacheObstacleObject(Player.Position, 20f, 0));
                                LastRecordedPosition = Player.Position;

                            }
                        }
                        // Maintain a backtrack list only while fighting monsters
                        if (CurrentTarget.IsUnit && Settings.Combat.Misc.AllowBacktracking &&
                            (TotalBacktracks == 0 || Vector3.Distance(Player.Position, BacktrackList[TotalBacktracks]) >= 10f))
                        {
                            bool bAddThisBacktrack = true;
                            // Check we aren't within 12 feet of 2 backtracks again (eg darting back & forth)
                            if (TotalBacktracks >= 2)
                            {
                                if (Vector3.Distance(Player.Position, BacktrackList[TotalBacktracks - 1]) < 12f)
                                    bAddThisBacktrack = false;
                            }
                            if (bAddThisBacktrack)
                            {
                                TotalBacktracks++;
                                BacktrackList.Add(TotalBacktracks, Player.Position);
                            }
                        }
                    }


                    using (new PerformanceLogger("HandleTarget.LoSCheck"))
                    {
                        TargetCurrentDistance = CurrentTarget.RadiusDistance;
                        if (DataDictionary.AlwaysRaycastWorlds.Contains(Player.WorldID) && CurrentTarget.Distance > CurrentTarget.Radius + 2f)
                        {
                            CurrentTargetIsInLoS = NavHelper.CanRayCast(Player.Position, CurrentDestination);
                        }
                        else if (TargetCurrentDistance <= 20f)
                        {
                            CurrentTargetIsInLoS = true;
                        }
                        else if (Settings.Combat.Misc.UseNavMeshTargeting && CurrentTarget.Type != GObjectType.Barricade && CurrentTarget.Type != GObjectType.Destructible)
                        {
                            CurrentTargetIsInLoS = (NavHelper.CanRayCast(Player.Position, CurrentDestination) || DataDictionary.LineOfSightWhitelist.Contains(CurrentTarget.ActorSNO));
                        }
                        else
                        {
                            CurrentTargetIsInLoS = true;
                        }
                    }

                    using (new PerformanceLogger("HandleTarget.InRange"))
                    {
                        bool stuckOnTarget =
                            ((CurrentTarget.Type == GObjectType.Barricade ||
                             CurrentTarget.Type == GObjectType.Interactable ||
                             CurrentTarget.Type == GObjectType.CursedChest ||
                             CurrentTarget.Type == GObjectType.CursedShrine ||
                             CurrentTarget.Type == GObjectType.Destructible) &&
                             !ZetaDia.Me.Movement.IsMoving && DateTime.UtcNow.Subtract(PlayerMover.TimeLastUsedPlayerMover).TotalMilliseconds < 250);

                        bool npcInRange = CurrentTarget.IsQuestGiver && CurrentTarget.RadiusDistance <= 3f;

                        bool noRangeRequired = TargetRangeRequired <= 1f;
                        switch (CurrentTarget.Type)
                        {
                            // These always have TargetRangeRequired=1f, but, we need to run directly to their center until we stop moving, then destroy them
                            case GObjectType.Door:
                            case GObjectType.Barricade:
                            case GObjectType.Destructible:
                                noRangeRequired = false;
                                break;
                        }

                        // Interact/use power on target if already in range
                        if (noRangeRequired || (TargetCurrentDistance <= TargetRangeRequired && CurrentTargetIsInLoS) || stuckOnTarget || npcInRange)
                        {
                            Logger.LogDebug(LogCategory.Behavior, "Object in Range: noRangeRequired={0} Target In Range={1} stuckOnTarget={2} npcInRange={3}",
                                noRangeRequired, (TargetCurrentDistance <= TargetRangeRequired && CurrentTargetIsInLoS), stuckOnTarget, npcInRange);

                            UpdateStatusTextTarget(true);

                            HandleObjectInRange();
                            runStatus = HandlerRunStatus.TreeRunning;
                        }
                        //check if we are returning to the tree
                        if (runStatus != HandlerRunStatus.NotFinished)
                            return GetTreeSharpRunStatus(runStatus);
                    }


                    using (new PerformanceLogger("HandleTarget.UpdateStatusText"))
                    {
                        // Out-of-range, so move towards the target
                        UpdateStatusTextTarget(false);
                    }

                    // Are we currently incapacitated? If so then wait...
                    if (Player.IsIncapacitated || Player.IsRooted)
                    {
                        runStatus = HandlerRunStatus.TreeRunning;
                        Logger.Log(LogCategory.Behavior, "Player is rooted or incapacitated!");
                        return GetTreeSharpRunStatus(runStatus);
                    }

                    // Check to see if we're stuck in moving to the target
                    runStatus = HandleTargetDistanceCheck(runStatus);

                    //check if we are returning to the tree
                    if (runStatus != HandlerRunStatus.NotFinished)
                        return GetTreeSharpRunStatus(runStatus);


                    // Update the last distance stored
                    LastDistanceFromTarget = TargetCurrentDistance;

                    if (TimeSinceUse(SNOPower.Monk_TempestRush) < 250)
                    {
                        ForceNewMovement = true;
                    }

                    // Only position-shift when not avoiding
                    // See if we want to ACTUALLY move, or are just waiting for the last move command...
                    if (!ForceNewMovement && IsAlreadyMoving && CurrentDestination == LastMoveToTarget && DateTime.UtcNow.Subtract(lastMovementCommand).TotalMilliseconds <= 100)
                    {
                        runStatus = HandlerRunStatus.TreeRunning;
                        //check if we are returning to the tree
                        if (runStatus != HandlerRunStatus.NotFinished)
                            return GetTreeSharpRunStatus(runStatus);
                    }
                    using (new PerformanceLogger("HandleTarget.SpecialMovement"))
                    {

                        bool Monk_SpecialMovement = ((CurrentTarget.Type == GObjectType.Gold ||
                            CurrentTarget.IsUnit ||
                            CurrentTarget.Type == GObjectType.Barricade ||
                            CurrentTarget.Type == GObjectType.Destructible) && (Monk_TempestRushReady()));

                        bool Attackable_SpecialMovement = ((CurrentTarget.Type == GObjectType.Avoidance &&
                            ObjectCache.Any(u => (u.IsUnit || u.Type == GObjectType.Destructible || u.Type == GObjectType.Barricade) &&
                                MathUtil.IntersectsPath(u.Position, u.Radius, Player.Position, CurrentTarget.Position))));

                        // If we're doing avoidance, globes or backtracking, try to use special abilities to move quicker
                        if ((CurrentTarget.Type == GObjectType.Avoidance ||
                            CurrentTarget.Type == GObjectType.HealthGlobe ||
                            CurrentTarget.Type == GObjectType.PowerGlobe ||
                            Monk_SpecialMovement ||
                            (CurrentTarget.Type == GObjectType.Backtrack && Settings.Combat.Misc.AllowOOCMovement))
                            && NavHelper.CanRayCast(Player.Position, CurrentDestination)
                            )
                        {
                            bool bFoundSpecialMovement = UsedSpecialMovement();

                            if (CurrentTarget.Type != GObjectType.Backtrack)
                            {
                                // Whirlwind for a barb

                                if (Attackable_SpecialMovement && !IsWaitingForSpecial && CombatBase.CurrentPower.SNOPower != SNOPower.Barbarian_WrathOfTheBerserker && !bFoundSpecialMovement && Hotbar.Contains(SNOPower.Barbarian_Whirlwind) && Player.PrimaryResource >= 10)
                                {
                                    ZetaDia.Me.UsePower(SNOPower.Barbarian_Whirlwind, CurrentDestination, CurrentWorldDynamicId, -1);
                                    // Store the current destination for comparison incase of changes next loop
                                    LastMoveToTarget = CurrentDestination;
                                    // Reset total body-block count, since we should have moved
                                    if (DateTime.UtcNow.Subtract(_lastForcedKeepCloseRange).TotalMilliseconds >= 2000)
                                        _timesBlockedMoving = 0;
                                    runStatus = HandlerRunStatus.TreeRunning;
                                    //check if we are returning to the tree
                                    if (runStatus != HandlerRunStatus.NotFinished)
                                        return GetTreeSharpRunStatus(runStatus);
                                }
                                // Tempest rush for a monk
                                if (!bFoundSpecialMovement && Hotbar.Contains(SNOPower.Monk_TempestRush) && Player.PrimaryResource >= Settings.Combat.Monk.TR_MinSpirit &&
                                    ((CurrentTarget.Type == GObjectType.Item && CurrentTarget.Distance > 20f) || CurrentTarget.Type != GObjectType.Item) &&
                                    Settings.Combat.Monk.TROption != TempestRushOption.MovementOnly &&
                                    Monk_TempestRushReady())
                                {
                                    ZetaDia.Me.UsePower(SNOPower.Monk_TempestRush, CurrentDestination, CurrentWorldDynamicId, -1);
                                    CacheData.AbilityLastUsed[SNOPower.Monk_TempestRush] = DateTime.UtcNow;
                                    LastPowerUsed = SNOPower.Monk_TempestRush;
                                    LastTempestRushLocation = CurrentDestination;
                                    // Store the current destination for comparison incase of changes next loop
                                    LastMoveToTarget = CurrentDestination;
                                    // Reset total body-block count, since we should have moved
                                    if (DateTime.UtcNow.Subtract(_lastForcedKeepCloseRange).TotalMilliseconds >= 2000)
                                        _timesBlockedMoving = 0;
                                    runStatus = HandlerRunStatus.TreeRunning;
                                    //check if we are returning to the tree
                                    if (runStatus != HandlerRunStatus.NotFinished)
                                        return GetTreeSharpRunStatus(runStatus);
                                }
                                // Strafe for a Demon Hunter
                                if (Attackable_SpecialMovement && !bFoundSpecialMovement && Hotbar.Contains(SNOPower.DemonHunter_Strafe) && Player.PrimaryResource >= 15)
                                {
                                    ZetaDia.Me.UsePower(SNOPower.DemonHunter_Strafe, CurrentDestination, CurrentWorldDynamicId, -1);
                                    // Store the current destination for comparison incase of changes next loop
                                    LastMoveToTarget = CurrentDestination;
                                    // Reset total body-block count, since we should have moved
                                    if (DateTime.UtcNow.Subtract(_lastForcedKeepCloseRange).TotalMilliseconds >= 2000)
                                        _timesBlockedMoving = 0;
                                    runStatus = HandlerRunStatus.TreeRunning;
                                    //check if we are returning to the tree
                                    if (runStatus != HandlerRunStatus.NotFinished)
                                        return GetTreeSharpRunStatus(runStatus);
                                }
                            }
                            if (bFoundSpecialMovement)
                            {
                                // Store the current destination for comparison incase of changes next loop
                                LastMoveToTarget = CurrentDestination;
                                // Reset total body-block count, since we should have moved
                                if (DateTime.UtcNow.Subtract(_lastForcedKeepCloseRange).TotalMilliseconds >= 2000)
                                    _timesBlockedMoving = 0;
                                runStatus = HandlerRunStatus.TreeRunning;
                                //check if we are returning to the tree
                                if (runStatus != HandlerRunStatus.NotFinished)
                                    return GetTreeSharpRunStatus(runStatus);
                            }
                        }
                    }

                    // Whirlwind against everything within range (except backtrack points)

                    if (Hotbar.Contains(SNOPower.Barbarian_Whirlwind) && Player.PrimaryResource >= 10 && TargetUtil.AnyMobsInRange(20) && !IsWaitingForSpecial && CombatBase.CurrentPower.SNOPower != SNOPower.Barbarian_WrathOfTheBerserker && TargetCurrentDistance <= 12f && CurrentTarget.Type != GObjectType.Container && CurrentTarget.Type != GObjectType.Backtrack &&
                        (!Hotbar.Contains(SNOPower.Barbarian_Sprint) || GetHasBuff(SNOPower.Barbarian_Sprint)) &&
                        CurrentTarget.Type != GObjectType.Backtrack &&
                        (CurrentTarget.Type != GObjectType.Item && CurrentTarget.Type != GObjectType.Gold && TargetCurrentDistance >= 6f) &&
                        (CurrentTarget.Type != GObjectType.Unit ||
                        (CurrentTarget.IsUnit && !CurrentTarget.IsTreasureGoblin)))
                    {
                        // Special code to prevent whirlwind double-spam, this helps save fury
                        bool bUseThisLoop = SNOPower.Barbarian_Whirlwind != LastPowerUsed;
                        if (!bUseThisLoop)
                        {
                            LastPowerUsed = SNOPower.None;
                            if (TimeSinceUse(SNOPower.Barbarian_Whirlwind) >= 200)
                                bUseThisLoop = true;
                        }
                        if (bUseThisLoop)
                        {
                            ZetaDia.Me.UsePower(SNOPower.Barbarian_Whirlwind, CurrentDestination, CurrentWorldDynamicId, -1);
                            LastPowerUsed = SNOPower.Barbarian_Whirlwind;
                            CacheData.AbilityLastUsed[SNOPower.Barbarian_Whirlwind] = DateTime.UtcNow;
                        }
                        // Store the current destination for comparison incase of changes next loop
                        LastMoveToTarget = CurrentDestination;
                        // Reset total body-block count
                        if ((!_forceCloseRangeTarget || DateTime.UtcNow.Subtract(_lastForcedKeepCloseRange).TotalMilliseconds > ForceCloseRangeForMilliseconds) &&
                            DateTime.UtcNow.Subtract(_lastForcedKeepCloseRange).TotalMilliseconds >= 2000)
                            _timesBlockedMoving = 0;
                        runStatus = HandlerRunStatus.TreeRunning;
                        //check if we are returning to the tree
                        if (runStatus != HandlerRunStatus.NotFinished)
                            return GetTreeSharpRunStatus(runStatus);
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError("Error in HandleTarget: {0}", ex.Message);
                    runStatus = HandlerRunStatus.TreeFailure;
                    return GetTreeSharpRunStatus(runStatus);
                }

                HandleTargetBasicMovement(ForceNewMovement);

                runStatus = HandlerRunStatus.TreeRunning;
                return GetTreeSharpRunStatus(runStatus);
            }
        }

        private static void HandleObjectInRange()
        {
            switch (CurrentTarget.Type)
            {
                case GObjectType.Avoidance:
                    _forceTargetUpdate = true;
                    break;
                case GObjectType.Player:
                    break;

                // Unit, use our primary power to attack
                case GObjectType.Unit:
                    {
                        if (CombatBase.CurrentPower.SNOPower != SNOPower.None)
                        {
                            if (_isWaitingForPower && CombatBase.CurrentPower.ShouldWaitBeforeUse)
                            {
                            }
                            else if (_isWaitingForPower && !CombatBase.CurrentPower.ShouldWaitBeforeUse)
                            {
                                _isWaitingForPower = false;
                            }
                            else
                            {
                                _isWaitingForPower = false;
                                HandleUnitInRange();
                            }
                        }
                        break;
                    }
                // Item, interact with it and log item stats
                case GObjectType.Item:
                    {
                        // Check if we actually have room for this item first

                        bool isTwoSlot = true;
                        if (CurrentTarget.Item != null && CurrentTarget.Item.CommonData != null)
                        {
                            isTwoSlot = CurrentTarget.Item.CommonData.IsTwoSquareItem;
                        }

                        Vector2 validLocation = TrinityItemManager.FindValidBackpackLocation(isTwoSlot);
                        if (validLocation.X < 0 || validLocation.Y < 0)
                        {
                            Logger.Log("No more space to pickup item, town-run requested at next free moment. (HandleTarget)");
                            ForceVendorRunASAP = true;

                            // Record the first position when we run out of bag space, so we can return later
                            TownRun.SetPreTownRunPosition();
                        }
                        else
                        {
                            HandleItemInRange();
                        }
                        break;
                    }
                // * Gold & Globe - need to get within pickup radius only
                case GObjectType.Gold:
                case GObjectType.HealthGlobe:
                case GObjectType.PowerGlobe:
                    {
                        int interactAttempts;
                        // Count how many times we've tried interacting
                        if (!CacheData.InteractAttempts.TryGetValue(CurrentTarget.RActorGuid, out interactAttempts))
                        {
                            CacheData.InteractAttempts.Add(CurrentTarget.RActorGuid, 1);
                        }
                        else
                        {
                            CacheData.InteractAttempts[CurrentTarget.RActorGuid]++;
                        }
                        // If we've tried interacting too many times, blacklist this for a while
                        if (interactAttempts > 3)
                        {
                            Blacklist90Seconds.Add(CurrentTarget.RActorGuid);
                            //dateSinceBlacklist90Clear = DateTime.UtcNow;
                            Blacklist60Seconds.Add(CurrentTarget.RActorGuid);
                        }
                        _ignoreRactorGuid = CurrentTarget.RActorGuid;
                        _ignoreTargetForLoops = 3;

                        // Now tell Trinity to get a new target!
                        _forceTargetUpdate = true;
                        break;
                    }

                case GObjectType.Door:
                case GObjectType.HealthWell:
                case GObjectType.Shrine:
                case GObjectType.Container:
                case GObjectType.Interactable:
                case GObjectType.CursedChest:
                case GObjectType.CursedShrine:
                    {
                        _forceTargetUpdate = true;

                        if (ZetaDia.Me.Movement.SpeedXY > 0.5)
                        {
                            Logger.LogVerbose(LogCategory.Behavior, "Trying to stop, Speeds:{0:0.00}/{1:0.00}", ZetaDia.Me.Movement.SpeedXY, PlayerMover.GetMovementSpeed());
                            Navigator.PlayerMover.MoveStop();

                        }
                        else
                        {
                            if (SpellHistory.TimeSinceUse(SNOPower.Axe_Operate_Gizmo) < TimeSpan.FromMilliseconds(150))
                            {
                                break;
                            }

                            int attemptCount;
                            CacheData.InteractAttempts.TryGetValue(CurrentTarget.RActorGuid, out attemptCount);

                            Logger.LogVerbose(LogCategory.Behavior, "Interacting with {1} Distance {2:0} Radius {3:0.0} Attempt {4}",
                                     SNOPower.Axe_Operate_Gizmo, CurrentTarget.InternalName, CurrentTarget.Distance, CurrentTarget.Radius, attemptCount);

                            if (CurrentTarget.ActorType == ActorType.Monster)
                                ZetaDia.Me.UsePower(SNOPower.Axe_Operate_NPC, Vector3.Zero, CurrentWorldDynamicId, CurrentTarget.ACDGuid);
                            else
                                ZetaDia.Me.UsePower(SNOPower.Axe_Operate_Gizmo, Vector3.Zero, 0, CurrentTarget.ACDGuid);

                            SpellHistory.RecordSpell(new TrinityPower()
                            {
                                SNOPower = SNOPower.Axe_Operate_Gizmo,
                                TargetACDGUID = CurrentTarget.ACDGuid,
                                MinimumRange = TargetRangeRequired,
                                TargetPosition = CurrentTarget.Position,
                            });

                            // Count how many times we've tried interacting
                            if (!CacheData.InteractAttempts.TryGetValue(CurrentTarget.RActorGuid, out attemptCount))
                            {
                                CacheData.InteractAttempts.Add(CurrentTarget.RActorGuid, 1);
                            }
                            else
                            {
                                CacheData.InteractAttempts[CurrentTarget.RActorGuid] += 1;
                            }

                            // If we've tried interacting too many times, blacklist this for a while
                            if (CacheData.InteractAttempts[CurrentTarget.RActorGuid] > 15 && CurrentTarget.Type != GObjectType.HealthWell)
                            {
                                Logger.LogVerbose("Blacklisting {0} ({1}) for 15 seconds after {2} interactions",
                                    CurrentTarget.InternalName, CurrentTarget.ActorSNO, attemptCount);
                                Blacklist15Seconds.Add(CurrentTarget.RActorGuid);
                            }
                        }
                        break;
                    }
                // * Destructible - need to pick an ability and attack it
                case GObjectType.Destructible:
                case GObjectType.Barricade:
                    {
                        if (CombatBase.CurrentPower.SNOPower != SNOPower.None)
                        {
                            if (CurrentTarget.Type == GObjectType.Barricade)
                            {
                                Logger.Log(TrinityLogLevel.Verbose, LogCategory.Behavior,
                                    "Barricade: Name={0}. SNO={1}, Range={2}. Needed range={3}. Radius={4}. Type={5}. Using power={6}",
                                    CurrentTarget.InternalName,     // 0
                                    CurrentTarget.ActorSNO,         // 1
                                    CurrentTarget.Distance,         // 2
                                    TargetRangeRequired,            // 3
                                    CurrentTarget.Radius,           // 4
                                    CurrentTarget.Type,             // 5
                                    CombatBase.CurrentPower.SNOPower// 6 
                                    );
                            }
                            else
                            {
                                Logger.Log(TrinityLogLevel.Verbose, LogCategory.Behavior,
                                    "Destructible: Name={0}. SNO={1}, Range={2}. Needed range={3}. Radius={4}. Type={5}. Using power={6}",
                                    CurrentTarget.InternalName,       // 0
                                    CurrentTarget.ActorSNO,           // 1
                                    TargetCurrentDistance,            // 2
                                    TargetRangeRequired,              // 3 
                                    CurrentTarget.Radius,             // 4
                                    CurrentTarget.Type,               // 5
                                    CombatBase.CurrentPower.SNOPower  // 6
                                    );
                            }

                            if (CurrentTarget.RActorGuid == _ignoreRactorGuid || DataDictionary.DestroyAtLocationIds.Contains(CurrentTarget.ActorSNO))
                            {
                                // Location attack - attack the Vector3/map-area (equivalent of holding shift and left-clicking the object in-game to "force-attack")
                                Vector3 vAttackPoint;
                                if (CurrentTarget.Distance >= 6f)
                                    vAttackPoint = MathEx.CalculatePointFrom(CurrentTarget.Position, Player.Position, 6f);
                                else
                                    vAttackPoint = CurrentTarget.Position;

                                vAttackPoint.Z += 1.5f;
                                Logger.Log(TrinityLogLevel.Debug, LogCategory.Behavior, "(NB: Attacking location of destructable)");
                                ZetaDia.Me.UsePower(CombatBase.CurrentPower.SNOPower, vAttackPoint, CurrentWorldDynamicId, -1);
                                if (CombatBase.CurrentPower.SNOPower == SNOPower.Monk_TempestRush)
                                    LastTempestRushLocation = vAttackPoint;
                            }
                            else
                            {
                                // Standard attack - attack the ACDGUID (equivalent of left-clicking the object in-game)
                                ZetaDia.Me.UsePower(CombatBase.CurrentPower.SNOPower, Vector3.Zero, -1, CurrentTarget.ACDGuid);
                                if (CombatBase.CurrentPower.SNOPower == SNOPower.Monk_TempestRush)
                                    LastTempestRushLocation = CurrentTarget.Position;
                            }

                            int interactAttempts;
                            // Count how many times we've tried interacting
                            if (!CacheData.InteractAttempts.TryGetValue(CurrentTarget.RActorGuid, out interactAttempts))
                            {
                                CacheData.InteractAttempts.Add(CurrentTarget.RActorGuid, 1);
                            }
                            else
                            {
                                CacheData.InteractAttempts[CurrentTarget.RActorGuid]++;
                            }

                            CacheData.AbilityLastUsed[CombatBase.CurrentPower.SNOPower] = DateTime.UtcNow;

                            // Prevent this EXACT object being targetted again for a short while, just incase
                            _ignoreRactorGuid = CurrentTarget.RActorGuid;
                            _ignoreTargetForLoops = 3;
                            // Add this destructible/barricade to our very short-term ignore list
                            //Destructible3SecBlacklist.Add(CurrentTarget.RActorGuid);
                            Logger.Log(TrinityLogLevel.Debug, LogCategory.Behavior, "Blacklisting {0} {1} {2} for 3 seconds for Destrucable attack", CurrentTarget.Type, CurrentTarget.InternalName, CurrentTarget.ActorSNO);
                            _lastDestroyedDestructible = DateTime.UtcNow;
                            _needClearDestructibles = true;
                        }
                        // Now tell Trinity to get a new target!
                        _forceTargetUpdate = true;
                    }
                    break;
                // * Backtrack - clear this waypoint
                case GObjectType.Backtrack:
                    // Remove the current backtrack location now we reached it
                    BacktrackList.Remove(TotalBacktracks);
                    TotalBacktracks--;
                    // Never bother with the very first backtrack location
                    if (TotalBacktracks <= 1)
                    {
                        TotalBacktracks = 0;
                        BacktrackList = new SortedList<int, Vector3>();
                    }
                    break;
                default:
                    {
                        _forceTargetUpdate = true;
                        Logger.LogError("Default handle target in range encountered for {0} Type: {1}", CurrentTarget.InternalName, CurrentTarget.Type);
                        break;
                    }
            }
        }

        private static HandlerRunStatus HandleTargetDistanceCheck(HandlerRunStatus runStatus)
        {
            using (new PerformanceLogger("HandleTarget.DistanceEqualCheck"))
            {
                // Count how long we have failed to move - body block stuff etc.
                if (Math.Abs(TargetCurrentDistance - LastDistanceFromTarget) < 5f && PlayerMover.GetMovementSpeed() < 1)
                {
                    ForceNewMovement = true;
                    if (DateTime.UtcNow.Subtract(_lastMovedDuringCombat).TotalMilliseconds >= 500)
                    {
                        _lastMovedDuringCombat = DateTime.UtcNow;
                        // We've been stuck at least 250 ms, let's go and pick new targets etc.
                        _timesBlockedMoving++;
                        _forceCloseRangeTarget = true;
                        _lastForcedKeepCloseRange = DateTime.UtcNow;
                        // And tell Trinity to get a new target
                        _forceTargetUpdate = true;

                        // If we were backtracking and failed, remove the current backtrack and try and move to the next
                        if (CurrentTarget.Type == GObjectType.Backtrack && _timesBlockedMoving >= 2)
                        {
                            BacktrackList.Remove(TotalBacktracks);
                            TotalBacktracks--;
                            if (TotalBacktracks <= 1)
                            {
                                TotalBacktracks = 0;
                                BacktrackList = new SortedList<int, Vector3>();
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
                    _lastMovedDuringCombat = DateTime.UtcNow;
                }
            }
            return runStatus;
        }

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
                if (PlayerMover.GetMovementSpeed() > 1)
                    return HandlerRunStatus.NotFinished;

                if (CurrentTargetIsNonUnit() && GetSecondsSinceTargetUpdate() > 6)
                    shouldTryBlacklist = true;

                if ((CurrentTargetIsUnit() && CurrentTarget.IsBoss && GetSecondsSinceTargetUpdate() > 45))
                    shouldTryBlacklist = true;

                // special raycast check for current target after 10 sec
                if ((CurrentTargetIsUnit() && !CurrentTarget.IsBoss && GetSecondsSinceTargetUpdate() > 10))
                    shouldTryBlacklist = true;

                if (CurrentTarget.Type == GObjectType.HotSpot)
                    shouldTryBlacklist = false;

                if (shouldTryBlacklist)
                {
                    // NOTE: This only blacklists if it's remained the PRIMARY TARGET that we are trying to actually directly attack!
                    // So it won't blacklist a monster "on the edge of the screen" who isn't even being targetted
                    // Don't blacklist monsters on <= 50% health though, as they can't be in a stuck location... can they!? Maybe give them some extra time!

                    bool isNavigable = NavHelper.CanRayCast(Player.Position, CurrentDestination);

                    bool addTargetToBlacklist = true;

                    // PREVENT blacklisting a monster on less than 90% health unless we haven't damaged it for more than 2 minutes
                    if (CurrentTarget.IsUnit && isNavigable && CurrentTarget.IsTreasureGoblin && Settings.Combat.Misc.GoblinPriority >= GoblinPriority.Kamikaze)
                    {
                        addTargetToBlacklist = false;
                    }

                    int interactAttempts;
                    CacheData.InteractAttempts.TryGetValue(CurrentTarget.RActorGuid, out interactAttempts);

                    if ((CurrentTarget.Type == GObjectType.Door || CurrentTarget.Type == GObjectType.Interactable || CurrentTarget.Type == GObjectType.Container) &&
                        interactAttempts < 45 && DateTime.UtcNow.Subtract(PlayerMover.LastRecordedAnyStuck).TotalSeconds > 15)
                    {
                        addTargetToBlacklist = false;
                    }

                    if (addTargetToBlacklist)
                    {
                        if (CurrentTarget.IsUnit)
                        {
                            Logger.Log(TrinityLogLevel.Verbose, LogCategory.Behavior,
                                "Blacklisting a monster because of possible stuck issues. Monster={0} [{1}] Range={2:0} health %={3:0} RActorGUID={4}",
                                CurrentTarget.InternalName,         // 0
                                CurrentTarget.ActorSNO,             // 1
                                CurrentTarget.Distance,       // 2
                                CurrentTarget.HitPointsPct,            // 3
                                CurrentTarget.RActorGuid            // 4
                                );
                        }
                        else
                        {
                            Logger.Log(TrinityLogLevel.Verbose, LogCategory.Behavior,
                                "Blacklisting an object because of possible stuck issues. Object={0} [{1}]. Range={2:0} RActorGUID={3}",
                                CurrentTarget.InternalName,         // 0
                                CurrentTarget.ActorSNO,             // 1 
                                CurrentTarget.Distance,       // 2
                                CurrentTarget.RActorGuid            // 3
                                );
                        }

                        if (CurrentTarget.IsBoss)
                        {
                            Blacklist15Seconds.Add(CurrentTarget.RActorGuid);
                            Blacklist15LastClear = DateTime.UtcNow;
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
                            Blacklist90Seconds.Add(CurrentTarget.RActorGuid);
                            Blacklist90LastClear = DateTime.UtcNow;
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
                if (_shouldPickNewAbilities && !_isWaitingForPower && !_isWaitingForPotion)
                {
                    _shouldPickNewAbilities = false;
                    if (CurrentTarget.IsUnit)
                    {
                        // Pick a suitable ability
                        if (CurrentTarget != null)
                            CombatBase.CurrentPower = AbilitySelector(false, false, false);
                        if (CombatBase.CurrentPower.SNOPower == SNOPower.None && !Player.IsIncapacitated)
                        {
                            NoAbilitiesAvailableInARow++;
                            if (DateTime.UtcNow.Subtract(lastRemindedAboutAbilities).TotalSeconds > 60 && NoAbilitiesAvailableInARow >= 4)
                            {
                                ShouldRefreshHotbarAbilities = true;
                                lastRemindedAboutAbilities = DateTime.UtcNow;
                                Logger.Log(TrinityLogLevel.Error, LogCategory.Behavior, "Fatal Error: Couldn't find a valid attack ability. Not enough resource for any abilities or all on cooldown");
                                Logger.Log(TrinityLogLevel.Error, LogCategory.Behavior, "If you get this message frequently, you should consider changing your build");
                                Logger.Log(TrinityLogLevel.Error, LogCategory.Behavior, "Perhaps you don't have enough critical hit chance % for your current build, or just have a bad skill setup?");
                            }
                        }
                        else
                        {
                            NoAbilitiesAvailableInARow = 0;
                        }
                    }
                    // Select an ability for destroying a destructible with in advance
                    if (CurrentTarget.Type == GObjectType.Destructible || CurrentTarget.Type == GObjectType.Barricade)
                        CombatBase.CurrentPower = AbilitySelector(false, false, true);
                }
            }
        }

        /// <summary>
        /// Will check <see cref=" _isWaitingForPotion"/> and Use a Potion if needed
        /// </summary>
        private static void UsePotionIfNeeded()
        {
            using (new PerformanceLogger("HandleTarget.UseHealthPotionIfNeeded"))
            {
                if (!Player.IsIncapacitated && Player.CurrentHealthPct > 0 && SpellHistory.TimeSinceUse(SNOPower.DrinkHealthPotion) > TimeSpan.FromSeconds(30) &&
                    Player.CurrentHealthPct <= PlayerEmergencyHealthPotionLimit)
                {
                    var legendaryPotions = ZetaDia.Me.Inventory.Backpack.Where(i => i.InternalName.ToLower()
                        .Contains("healthpotion_legendary_"));

                    var regularPotions = ZetaDia.Me.Inventory.Backpack.Where(i => i.InternalName.ToLower()
                        .Contains("healthpotion_") && !i.InternalName.ToLower().Contains("legendary"));


                    int dynamicId = 0;
                    if (legendaryPotions.Any())
                    {
                        Logger.Log(TrinityLogLevel.Verbose, LogCategory.UserInformation, "Using Legendary Potion", 0);
                        dynamicId = legendaryPotions.FirstOrDefault().DynamicId;
                        ZetaDia.Me.Inventory.UseItem(dynamicId);
                        SpellHistory.RecordSpell(new TrinityPower(SNOPower.DrinkHealthPotion));
                    }
                    else if (regularPotions.Any())
                    {
                        Logger.Log(TrinityLogLevel.Verbose, LogCategory.UserInformation, "Using Potion", 0);
                        dynamicId = regularPotions.FirstOrDefault().DynamicId;
                        ZetaDia.Me.Inventory.UseItem(dynamicId);
                        SpellHistory.RecordSpell(new TrinityPower(SNOPower.DrinkHealthPotion));
                    }
                    else
                    {
                        Logger.Log(TrinityLogLevel.Verbose, LogCategory.UserInformation, "No Available potions!", 0);
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
                if (!_isWholeNewTarget && !_isWaitingForPower && !_isWaitingForPotion)
                {
                    // Update targets at least once every 80 milliseconds
                    if (_forceTargetUpdate || _isAvoidingProjectiles || DateTime.UtcNow.Subtract(LastRefreshedCache).TotalMilliseconds > Settings.Advanced.CacheRefreshRate)
                    {
                        _staleCache = true;
                    }
                    // If we AREN'T getting new targets - find out if we SHOULD because the current unit has died etc.
                    if (!_staleCache && CurrentTarget != null && CurrentTarget.IsUnit)
                    {
                        if (CurrentTarget.Unit == null || CurrentTarget.Unit.BaseAddress == IntPtr.Zero)
                        {
                            _staleCache = true;
                        }
                        else
                        {
                            // health calculations
                            double dThisMaxHealth;
                            // Get the max health of this unit, a cached version if available, if not cache it
                            if (!CacheData.UnitMaxHealth.TryGetValue(CurrentCacheObject.RActorGuid, out dThisMaxHealth))
                            {
                                try
                                {

                                    dThisMaxHealth = CurrentTarget.Unit.HitpointsMax;
                                    CacheData.UnitMaxHealth.Add(CurrentCacheObject.RActorGuid, CurrentTarget.Unit.HitpointsMax);
                                }
                                catch
                                {
                                    Logger.Log(TrinityLogLevel.Debug, LogCategory.Behavior, "Safely handled exception getting attribute max health #2 for unit {0} [{1}]", CurrentCacheObject.InternalName, CurrentCacheObject.ActorSNO);
                                    _staleCache = true;
                                }
                            }
                            // Ok check we didn't fail getting the maximum health, now try to get live current health...
                            if (!_staleCache)
                            {
                                try
                                {
                                    double dTempHitpoints = CurrentTarget.Unit.HitpointsCurrent / dThisMaxHealth;
                                    if (dTempHitpoints <= 0d)
                                    {
                                        _staleCache = true;
                                    }
                                    else
                                    {
                                        CurrentTarget.HitPointsPct = dTempHitpoints;
                                        CurrentTarget.Position = CurrentTarget.Unit.Position;
                                    }
                                }
                                catch
                                {
                                    _staleCache = true;
                                }
                            }

                            // force update of cached player data
                            PlayerInfoCache.UpdateCachedPlayerData();
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
                    TimeSinceUse(SNOPower.Barbarian_Leap) >= CombatBase.GetSNOPowerUseDelay(SNOPower.Barbarian_Leap) &&
                    PowerManager.CanCast(SNOPower.Barbarian_Leap))
                {
                    ZetaDia.Me.UsePower(SNOPower.Barbarian_Leap, CurrentDestination, CurrentWorldDynamicId, -1);
                    SpellHistory.RecordSpell(SNOPower.Barbarian_Leap);
                    bFoundSpecialMovement = true;
                }

                // Furious Charge movement for a barb
                if (!bFoundSpecialMovement && Hotbar.Contains(SNOPower.Barbarian_FuriousCharge) &&
                    TimeSinceUse(SNOPower.Barbarian_FuriousCharge) >= CombatBase.GetSNOPowerUseDelay(SNOPower.Barbarian_FuriousCharge) &&
                    PowerManager.CanCast(SNOPower.Barbarian_FuriousCharge))
                {
                    ZetaDia.Me.UsePower(SNOPower.Barbarian_FuriousCharge, CurrentDestination, CurrentWorldDynamicId, -1);
                    SpellHistory.RecordSpell(SNOPower.Barbarian_FuriousCharge);
                    bFoundSpecialMovement = true;
                }

                // Vault for a Demon Hunter
                if (!bFoundSpecialMovement && CombatBase.CanCast(SNOPower.DemonHunter_Vault) && Settings.Combat.DemonHunter.VaultMode != DemonHunterVaultMode.MovementOnly &&
                    CombatBase.TimeSincePowerUse(SNOPower.DemonHunter_Vault) >= Trinity.Settings.Combat.DemonHunter.VaultMovementDelay &&
                    (CombatBase.PlayerKiteDistance <= 0 || (!CacheData.MonsterObstacles.Any(a => a.Position.Distance(CurrentDestination) <= CombatBase.PlayerKiteDistance) &&
                    !CacheData.TimeBoundAvoidance.Any(a => a.Position.Distance(CurrentDestination) <= CombatBase.PlayerKiteDistance))) &&
                    (!CacheData.TimeBoundAvoidance.Any(a => MathEx.IntersectsPath(a.Position, a.Radius, Trinity.Player.Position, CurrentDestination))))
                {
                    ZetaDia.Me.UsePower(SNOPower.DemonHunter_Vault, CurrentDestination, CurrentWorldDynamicId, -1);
                    SpellHistory.RecordSpell(SNOPower.DemonHunter_Vault);
                    bFoundSpecialMovement = true;
                }

                // Teleport for a wizard (need to be able to check skill rune in DB for a 3-4 teleport spam in a row)
                if (!bFoundSpecialMovement && Hotbar.Contains(SNOPower.Wizard_Teleport) &&
                    TimeSinceUse(SNOPower.Wizard_Teleport) >= CombatBase.GetSNOPowerUseDelay(SNOPower.Wizard_Teleport) &&
                    Player.PrimaryResource >= 15 &&
                    PowerManager.CanCast(SNOPower.Wizard_Teleport))
                {
                    ZetaDia.Me.UsePower(SNOPower.Wizard_Teleport, CurrentDestination, CurrentWorldDynamicId, -1);
                    SpellHistory.RecordSpell(SNOPower.Wizard_Teleport);
                    bFoundSpecialMovement = true;
                }

                // Archon Teleport for a wizard (need to be able to check skill rune in DB for a 3-4 teleport spam in a row)
                if (!bFoundSpecialMovement && Hotbar.Contains(SNOPower.Wizard_Archon_Teleport) &&
                    TimeSinceUse(SNOPower.Wizard_Archon_Teleport) >= CombatBase.GetSNOPowerUseDelay(SNOPower.Wizard_Archon_Teleport) &&
                    PowerManager.CanCast(SNOPower.Wizard_Archon_Teleport))
                {
                    ZetaDia.Me.UsePower(SNOPower.Wizard_Archon_Teleport, CurrentDestination, CurrentWorldDynamicId, -1);
                    SpellHistory.RecordSpell(SNOPower.Wizard_Archon_Teleport);
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
            return CurrentTarget.IsUnit;
        }

        /// <summary>
        /// Returns the number of seconds since our current target was updated
        /// </summary>
        /// <returns></returns>
        private static double GetSecondsSinceTargetUpdate()
        {
            return DateTime.UtcNow.Subtract(_lastPickedTargetTime).TotalSeconds;
        }

        private static string lastStatusText = "";

        /// <summary>
        /// Updates bot status text with appropriate information if we are moving into range of our <see cref="CurrentTarget"/>
        /// </summary>
        private static void UpdateStatusTextTarget(bool targetIsInRange)
        {
            string action = "";

            StringBuilder statusText = new StringBuilder();
            if (!targetIsInRange)
                action = "Moveto ";
            else
                switch (CurrentTarget.Type)
                {
                    case GObjectType.Avoidance:
                        action = "Avoid ";
                        break;
                    case GObjectType.Unit:
                        action = "Attack ";
                        break;
                    case GObjectType.Item:
                    case GObjectType.Gold:
                    case GObjectType.PowerGlobe:
                    case GObjectType.HealthGlobe:
                        action = "Pickup ";
                        break;
                    case GObjectType.Backtrack:
                        action = "Backtrack ";
                        break;
                    case GObjectType.Interactable:
                        action = "Interact ";
                        break;
                    case GObjectType.Door:
                    case GObjectType.Container:
                        action = "Open ";
                        break;
                    case GObjectType.Destructible:
                    case GObjectType.Barricade:
                        action = "Destroy ";
                        break;
                    case GObjectType.Shrine:
                        action = "Click ";
                        break;
                }
            statusText.Append(action);

            statusText.Append("Target=");
            statusText.Append(CurrentTarget.InternalName);
            if (CurrentTarget.IsUnit && CombatBase.CurrentPower.SNOPower != SNOPower.None)
            {
                statusText.Append(" Power=");
                statusText.Append(CombatBase.CurrentPower.SNOPower);
            }
            statusText.Append(" Speed=");
            statusText.Append(ZetaDia.Me.Movement.SpeedXY.ToString("0"));
            statusText.Append(" SNO=");
            statusText.Append(CurrentTarget.ActorSNO.ToString(CultureInfo.InvariantCulture));
            statusText.Append(" Elite=");
            statusText.Append(CurrentTarget.IsBossOrEliteRareUnique.ToString());
            statusText.Append(" Weight=");
            statusText.Append(CurrentTarget.Weight.ToString("0"));
            statusText.Append(" Type=");
            statusText.Append(CurrentTarget.Type.ToString());
            statusText.Append(" C-Dist=");
            statusText.Append(CurrentTarget.Distance.ToString("0.0"));
            statusText.Append(" R-Dist=");
            statusText.Append(CurrentTarget.RadiusDistance.ToString("0.0"));
            statusText.Append(" RangeReq'd=");
            statusText.Append(TargetRangeRequired.ToString("0.0"));
            statusText.Append(" DistfromTrgt=");
            statusText.Append(TargetCurrentDistance.ToString("0"));
            statusText.Append(" tHP=");
            statusText.Append((CurrentTarget.HitPointsPct * 100).ToString("0"));
            statusText.Append(" MyHP=");
            statusText.Append((Player.CurrentHealthPct * 100).ToString("0"));
            statusText.Append(" MyMana=");
            statusText.Append((Player.PrimaryResource).ToString("0"));
            statusText.Append(" InLoS=");
            statusText.Append(CurrentTargetIsInLoS.ToString());

            statusText.Append(String.Format(" Duration={0:0}", DateTime.UtcNow.Subtract(_lastPickedTargetTime).TotalSeconds));

            if (Settings.Advanced.DebugInStatusBar)
            {
                _statusText = statusText.ToString();
                BotMain.StatusText = _statusText;
            }
            if (lastStatusText != statusText.ToString())
            {
                // prevent spam
                lastStatusText = statusText.ToString();
                Logger.Log(TrinityLogLevel.Debug, LogCategory.Targetting, "{0}", statusText.ToString());
                _resetStatusText = true;
            }
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
                lastMovementCommand = DateTime.UtcNow;

                if (DateTime.UtcNow.Subtract(lastSentMovePower).TotalMilliseconds >= 250 || Vector3.Distance(LastMoveToTarget, CurrentDestination) >= 2f || bForceNewMovement)
                {
                    bool straightLinePathing = DataDictionary.StraightLinePathingLevelAreaIds.Contains(Player.LevelAreaId);

                    string destname = String.Format("Name={0} Dist={1:0} IsElite={2} LoS={3} HP={4:0.00} Dir={5}",
                        CurrentTarget.InternalName,
                        CurrentTarget.RadiusDistance,
                        CurrentTarget.IsBossOrEliteRareUnique,
                        CurrentTarget.HasBeenInLoS,
                        CurrentTarget.HitPointsPct,
                        MathUtil.GetHeadingToPoint(CurrentTarget.Position));

                    MoveResult lastMoveResult;
                    if (straightLinePathing || CurrentDestination.Distance2DSqr(Player.Position) <= 10f * 10f)
                    {
                        lastMoveResult = MoveResult.Moved;
                        // just "Click" 
                        Navigator.PlayerMover.MoveTowards(CurrentDestination);
                        Logger.Log(TrinityLogLevel.Info, LogCategory.Movement, "Straight line pathing to {0}", destname);
                    }
                    else
                    {
                        lastMoveResult = PlayerMover.NavigateTo(CurrentDestination, destname);
                    }

                    lastSentMovePower = DateTime.UtcNow;

                    bool inRange = TargetCurrentDistance <= TargetRangeRequired || CurrentTarget.Distance < 10f;
                    if (lastMoveResult == MoveResult.ReachedDestination && !inRange &&
                        CurrentTarget.Type != GObjectType.Item &&
                        CurrentTarget.Type != GObjectType.Destructible &&
                        CurrentTarget.Type != GObjectType.Barricade)
                    {
                        bool pathFindresult = ((DefaultNavigationProvider)Navigator.NavigationProvider).CanPathWithinDistance(CurrentTarget.Position, CurrentTarget.Radius);
                        if (!pathFindresult)
                        {
                            Blacklist60Seconds.Add(CurrentTarget.RActorGuid);
                            Logger.Log("Unable to navigate to target! Blacklisting {0} SNO={1} RAGuid={2} dist={3:0} "
                                + (CurrentTarget.IsElite ? " IsElite " : "")
                                + (CurrentTarget.ItemQuality >= ItemQuality.Legendary ? "IsLegendaryItem " : ""),
                                CurrentTarget.InternalName, CurrentTarget.ActorSNO, CurrentTarget.RActorGuid, CurrentTarget.Distance);
                        }
                    }

                    // Store the current destination for comparison incase of changes next loop
                    LastMoveToTarget = CurrentDestination;
                    // Reset total body-block count, since we should have moved
                    if (DateTime.UtcNow.Subtract(_lastForcedKeepCloseRange).TotalMilliseconds >= 2000)
                        _timesBlockedMoving = 0;
                }
            }
        }

        private static void SetRangeRequiredForTarget()
        {
            using (new PerformanceLogger("HandleTarget.SetRequiredRange"))
            {
                TargetRangeRequired = 2f;
                TargetCurrentDistance = CurrentTarget.RadiusDistance;
                CurrentTargetIsInLoS = false;
                // Set current destination to our current target's destination
                CurrentDestination = CurrentTarget.Position;

                switch (CurrentTarget.Type)
                {
                    // * Unit, we need to pick an ability to use and get within range
                    case GObjectType.Unit:
                        {
                            // Pick a range to try to reach
                            TargetRangeRequired = CombatBase.CurrentPower.MinimumRange;
                            break;
                        }
                    // * Item - need to get within 6 feet and then interact with it
                    case GObjectType.Item:
                        {
                            TargetRangeRequired = 2f;
                            TargetCurrentDistance = CurrentTarget.Distance;
                            break;
                        }
                    // * Gold - need to get within pickup radius only
                    case GObjectType.Gold:
                        {
                            TargetRangeRequired = 2f;
                            TargetCurrentDistance = CurrentTarget.Distance;
                            CurrentDestination = MathEx.CalculatePointFrom(Player.Position, CurrentTarget.Position, -2f);
                            break;
                        }
                    // * Globes - need to get within pickup radius only
                    case GObjectType.PowerGlobe:
                    case GObjectType.HealthGlobe:
                        {
                            TargetRangeRequired = 2f;
                            TargetCurrentDistance = CurrentTarget.Distance;
                            break;
                        }
                    // * Shrine & Container - need to get within 8 feet and interact
                    case GObjectType.HealthWell:
                        {
                            TargetRangeRequired = 4f;

                            float range;
                            if (DataDictionary.CustomObjectRadius.TryGetValue(CurrentTarget.ActorSNO, out range))
                            {
                                TargetRangeRequired = range;
                            }
                            break;
                        }
                    case GObjectType.Shrine:
                    case GObjectType.Container:
                        {
                            TargetRangeRequired = 6f;

                            float range;
                            if (DataDictionary.CustomObjectRadius.TryGetValue(CurrentTarget.ActorSNO, out range))
                            {
                                TargetRangeRequired = range;
                            }
                            break;
                        }
                    case GObjectType.Interactable:
                        {
                            if (CurrentTarget.IsQuestGiver)
                            {
                                CurrentDestination = MathEx.CalculatePointFrom(CurrentTarget.Position, Player.Position, CurrentTarget.Radius + 2f);
                                TargetRangeRequired = 5f;
                            }
                            else
                            {
                                TargetRangeRequired = 5f;
                            }
                            // Check if it's in our interactable range dictionary or not
                            float range;

                            if (DataDictionary.CustomObjectRadius.TryGetValue(CurrentTarget.ActorSNO, out range))
                            {
                                TargetRangeRequired = range;
                            }
                            if (TargetRangeRequired <= 0)
                                TargetRangeRequired = CurrentTarget.Radius;

                            break;
                        }
                    // * Destructible - need to pick an ability and attack it
                    case GObjectType.Destructible:
                        {
                            // Pick a range to try to reach + (tmp_fThisRadius * 0.70);
                            //TargetRangeRequired = CombatBase.CurrentPower.SNOPower == SNOPower.None ? 9f : CombatBase.CurrentPower.MinimumRange;
                            TargetRangeRequired = CombatBase.CurrentPower.MinimumRange;
                            CurrentTarget.Radius = 1f;
                            TargetCurrentDistance = CurrentTarget.Distance;
                            break;
                        }
                    case GObjectType.Barricade:
                        {
                            // Pick a range to try to reach + (tmp_fThisRadius * 0.70);
                            TargetRangeRequired = CombatBase.CurrentPower.MinimumRange;
                            CurrentTarget.Radius = 1f;
                            TargetCurrentDistance = CurrentTarget.Distance;
                            break;
                        }
                    // * Avoidance - need to pick an avoid location and move there
                    case GObjectType.Avoidance:
                        {
                            TargetRangeRequired = 2f;
                            break;
                        }
                    // * Backtrack Destination
                    case GObjectType.Backtrack:
                        {
                            TargetRangeRequired = 5f;
                            break;
                        }
                    case GObjectType.Door:
                        TargetRangeRequired = 2f;
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
                float dist = 0;
                if (CombatBase.CurrentPower.TargetPosition != Vector3.Zero)
                    dist = CombatBase.CurrentPower.TargetPosition.Distance2D(Player.Position);
                else if (CurrentTarget != null)
                    dist = CurrentTarget.Position.Distance2D(Player.Position);

                var usePowerResult = ZetaDia.Me.UsePower(CombatBase.CurrentPower.SNOPower, CombatBase.CurrentPower.TargetPosition, CombatBase.CurrentPower.TargetDynamicWorldId, CombatBase.CurrentPower.TargetACDGUID);

                if (usePowerResult)
                {
                    Logger.Log(TrinityLogLevel.Debug, LogCategory.Behavior, "UsePower SUCCESS {0} at {1} on {2} dist={3}", CombatBase.CurrentPower.SNOPower, CombatBase.CurrentPower.TargetPosition, CombatBase.CurrentPower.TargetACDGUID, dist);
                    if (CombatBase.CurrentPower.SNOPower == SNOPower.Monk_TempestRush)
                        LastTempestRushLocation = CombatBase.CurrentPower.TargetPosition;

                    Monk_MaintainTempestRush();
                    SpellTracker.TrackSpellOnUnit(CombatBase.CurrentPower.TargetACDGUID, CombatBase.CurrentPower.SNOPower);
                    SpellHistory.RecordSpell(CombatBase.CurrentPower);

                    CacheData.AbilityLastUsed[CombatBase.CurrentPower.SNOPower] = DateTime.UtcNow;
                    lastGlobalCooldownUse = DateTime.UtcNow;
                    LastPowerUsed = CombatBase.CurrentPower.SNOPower;

                    // See if we should force a long wait AFTERWARDS, too
                    // Force waiting AFTER power use for certain abilities
                    _isWaitingAfterPower = false;
                    if (CombatBase.CurrentPower.ShouldWaitAfterUse)
                    {
                        _isWaitingAfterPower = true;
                    }
                }
                //else
                //{
                //    PowerManager.CanCastFlags failFlags;
                //    PowerManager.CanCast(CombatBase.CurrentPower.SNOPower, out failFlags);
                //    Logger.Log(TrinityLogLevel.Debug, LogCategory.Behavior, "UsePower FAILED {0} ({1}) at {2} on {3} dist={4}", CombatBase.CurrentPower.SNOPower, failFlags, CombatBase.CurrentPower.TargetPosition, CombatBase.CurrentPower.TargetACDGUID, dist);
                //}

                _shouldPickNewAbilities = true;

                // Keep looking for monsters at "normal kill range" a few moments after we successfully attack a monster incase we can pull them into range
                _keepKillRadiusExtendedForSeconds = 8;
                _timeKeepKillRadiusExtendedUntil = DateTime.UtcNow.AddSeconds(_keepKillRadiusExtendedForSeconds);
                _keepLootRadiusExtendedForSeconds = 8;
                // if at full or nearly full health, see if we can raycast to it, if not, ignore it for 2000 ms
                if (CurrentTarget.HitPointsPct >= 0.9d &&
                    !NavHelper.CanRayCast(Player.Position, CurrentTarget.Position) &&
                    !CurrentTarget.IsBoss &&
                    !(DataDictionary.StraightLinePathingLevelAreaIds.Contains(Player.LevelAreaId) || DataDictionary.LineOfSightWhitelist.Contains(CurrentTarget.ActorSNO)))
                {
                    _ignoreRactorGuid = CurrentTarget.RActorGuid;
                    _ignoreTargetForLoops = 6;
                    // Add this monster to our very short-term ignore list
                    Blacklist3Seconds.Add(CurrentTarget.RActorGuid);
                    Logger.Log(TrinityLogLevel.Verbose, LogCategory.Behavior, "Blacklisting {0} {1} {2} for 3 seconds due to Raycast failure", CurrentTarget.Type, CurrentTarget.InternalName, CurrentTarget.ActorSNO);
                    Blacklist3LastClear = DateTime.UtcNow;
                    NeedToClearBlacklist3 = true;
                }

            }
        }

        private static int HandleItemInRange()
        {
            using (new PerformanceLogger("HandleTarget.HandleItemInRange"))
            {
                int iInteractAttempts;
                // Pick the item up the usepower way, and "blacklist" for a couple of loops
                ZetaDia.Me.UsePower(SNOPower.Axe_Operate_Gizmo, Vector3.Zero, 0, CurrentTarget.ACDGuid);
                _ignoreRactorGuid = CurrentTarget.RActorGuid;
                _ignoreTargetForLoops = 3;
                // Store item pickup stats

                string itemSha1Hash = HashGenerator.GenerateItemHash(CurrentTarget.Position, CurrentTarget.ActorSNO, CurrentTarget.InternalName, CurrentWorldDynamicId, CurrentTarget.ItemQuality, CurrentTarget.ItemLevel);
                if (!_hashsetItemPicksLookedAt.Contains(itemSha1Hash))
                {
                    _hashsetItemPicksLookedAt.Add(itemSha1Hash);
                    GItemType itemType = DetermineItemType(CurrentTarget.InternalName, CurrentTarget.DBItemType, CurrentTarget.FollowerType);
                    GItemBaseType itemBaseType = DetermineBaseType(itemType);
                    if (itemBaseType == GItemBaseType.Armor || itemBaseType == GItemBaseType.WeaponOneHand || itemBaseType == GItemBaseType.WeaponTwoHand ||
                        itemBaseType == GItemBaseType.WeaponRange || itemBaseType == GItemBaseType.Jewelry || itemBaseType == GItemBaseType.FollowerItem ||
                        itemBaseType == GItemBaseType.Offhand)
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
                            Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "ERROR: Item type (" + iQuality + ") out of range");
                        }
                        if ((CurrentTarget.ItemLevel < 0) || (CurrentTarget.ItemLevel >= 74))
                        {
                            Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "ERROR: Item level (" + CurrentTarget.ItemLevel + ") out of range");
                        }
                        ItemsPickedStats.TotalPerQuality[iQuality]++;
                        ItemsPickedStats.TotalPerLevel[CurrentTarget.ItemLevel]++;
                        ItemsPickedStats.TotalPerQPerL[iQuality, CurrentTarget.ItemLevel]++;
                    }
                    else if (itemBaseType == GItemBaseType.Gem)
                    {
                        int iGemType = 0;
                        ItemsPickedStats.TotalGems++;
                        if (itemType == GItemType.Topaz)
                            iGemType = GEMTOPAZ;
                        if (itemType == GItemType.Ruby)
                            iGemType = GEMRUBY;
                        if (itemType == GItemType.Emerald)
                            iGemType = GEMEMERALD;
                        if (itemType == GItemType.Amethyst)
                            iGemType = GEMAMETHYST;
                        if (itemType == GItemType.Diamond)
                            iGemType = GEMDIAMOND;

                        ItemsPickedStats.GemsPerType[iGemType]++;
                        ItemsPickedStats.GemsPerLevel[CurrentTarget.ItemLevel]++;
                        ItemsPickedStats.GemsPerTPerL[iGemType, CurrentTarget.ItemLevel]++;
                    }
                    else if (itemType == GItemType.HealthPotion)
                    {
                        ItemsPickedStats.TotalPotions++;
                        if ((CurrentTarget.ItemLevel < 0) || (CurrentTarget.ItemLevel > 63))
                        {
                            Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "ERROR: Potion level ({0}) out of range", CurrentTarget.ItemLevel);
                        }
                        ItemsPickedStats.PotionsPerLevel[CurrentTarget.ItemLevel]++;
                    }
                    else if (c_item_GItemType == GItemType.InfernalKey)
                    {
                        ItemsPickedStats.TotalInfernalKeys++;
                    }
                }

                // Count how many times we've tried interacting
                if (!CacheData.InteractAttempts.TryGetValue(CurrentTarget.RActorGuid, out iInteractAttempts))
                {
                    CacheData.InteractAttempts.Add(CurrentTarget.RActorGuid, 1);

                    // Fire item looted for Demonbuddy Item stats
                    GameEvents.FireItemLooted(CurrentTarget.ACDGuid);
                }
                else
                {
                    CacheData.InteractAttempts[CurrentTarget.RActorGuid]++;
                }
                // If we've tried interacting too many times, blacklist this for a while
                if (iInteractAttempts > 20 && CurrentTarget.ItemQuality < ItemQuality.Legendary)
                {
                    Blacklist90Seconds.Add(CurrentTarget.RActorGuid);
                }
                // Now tell Trinity to get a new target!
                _forceTargetUpdate = true;
                return iInteractAttempts;
            }
        }
    }
}
