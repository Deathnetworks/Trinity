using System;
using System.Globalization;
using System.Linq;
using System.Text;
using Trinity.Combat;
using Trinity.Combat.Abilities;
using Trinity.Config.Combat;
using Trinity.DbProvider;
using Trinity.Items;
using Trinity.Reference;
using Trinity.Technicals;
using Zeta.Bot;
using Zeta.Bot.Navigation;
using Zeta.Common;
using Zeta.Game;
using Zeta.Game.Internals.Actors;
using Zeta.Game.Internals.SNO;
using Zeta.TreeSharp;
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
        /// Returns a RunStatus, if appropriate. Throws an exception if not.
        /// </summary>
        /// <param name="rs"></param>
        /// <returns></returns>
        private static RunStatus GetRunStatus(RunStatus status)
        {
            MonkCombat.RunOngoingPowers();

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

            Logger.Log(TrinityLogLevel.Debug, LogCategory.Behavior, "HandleTarget returning {0} to tree" + extras, status);

            return status;

        }

        /// <summary>
        /// Handles all aspects of moving to and attacking the current target
        /// </summary>
        /// <returns></returns>
        internal static RunStatus HandleTarget()
        {
            using (new PerformanceLogger("HandleTarget"))
            {
                try
                {
                    if (!Player.IsValid)
                    {
                        Logger.Log(TrinityLogLevel.Error, LogCategory.UserInformation, "No longer in game world", true);
                        return GetRunStatus(RunStatus.Failure);
                    }

                    if (Player.IsDead)
                    {
                        Logger.Log(TrinityLogLevel.Error, LogCategory.UserInformation, "Player is dead", true);
                        return GetRunStatus(RunStatus.Failure);
                    }

                    // Make sure we reset unstucker stuff here
                    PlayerMover.TimesReachedStuckPoint = 0;
                    PlayerMover.vSafeMovementLocation = Vector3.Zero;
                    PlayerMover.TimeLastRecordedPosition = DateTime.UtcNow;

                    if (!IsWaitingForPower && CombatBase.CurrentPower == null && CurrentTarget != null)
                        CombatBase.CurrentPower = AbilitySelector();

                    // Time based wait delay for certain powers with animations
                    if (IsWaitingAfterPower && CombatBase.CurrentPower.ShouldWaitAfterUse)
                    {
                        return GetRunStatus(RunStatus.Running);
                    }

                    IsWaitingAfterPower = false;

                    // See if we have been "newly rooted", to force target updates
                    if (Player.IsRooted && !WasRootedLastTick)
                    {
                        WasRootedLastTick = true;
                        _forceTargetUpdate = true;
                    }
                    if (!Player.IsRooted)
                        WasRootedLastTick = false;
                    if (CurrentTarget == null)
                    {
                        Logger.Log(TrinityLogLevel.Debug, LogCategory.Behavior, "CurrentTarget was passed as null! Continuing...");
                    }

                    MonkCombat.RunOngoingPowers();

                    // Refresh the object Cache every time
                    RefreshDiaObjectCache();

                    if (CombatBase.CombatMovement.IsQueuedMovement & CombatBase.IsCombatAllowed)
                    {
                        CombatBase.CombatMovement.Execute();
                        return GetRunStatus(RunStatus.Running);
                    }

                    while (CurrentTarget == null && (ForceVendorRunASAP || WantToTownRun) && !Zeta.Bot.Logic.BrainBehavior.IsVendoring && TownRun.TownRunTimerRunning())
                    {
                        Logger.Log(TrinityLogLevel.Info, LogCategory.Behavior, "CurrentTarget is null but we are ready to to Town Run, waiting... ");
                        return GetRunStatus(RunStatus.Running);
                    }

                    while (CurrentTarget == null && TownRun.IsTryingToTownPortal() && TownRun.TownRunTimerRunning())
                    {
                        Logger.Log(TrinityLogLevel.Info, LogCategory.Behavior, "Waiting for town run... ");
                        return GetRunStatus(RunStatus.Running);
                    }

                    if (CurrentTarget == null && TownRun.IsTryingToTownPortal() && TownRun.TownRunTimerFinished())
                    {
                        Logger.Log(TrinityLogLevel.Info, LogCategory.Behavior, "Town Run Ready!");
                        return GetRunStatus(RunStatus.Success);
                    }


                    if (CurrentTarget == null)
                    {
                        Logger.Log(TrinityLogLevel.Info, LogCategory.Behavior, "CurrentTarget set as null in refresh! Error 2, Returning Failure");
                        return GetRunStatus(RunStatus.Failure);
                    }

                    // Handle Target stuck / timeout
                    if (HandleTargetTimeoutTask())
                    {
                        Logger.Log(TrinityLogLevel.Info, LogCategory.Behavior, "Blacklisted Target, Returning Failure");
                        return GetRunStatus(RunStatus.Running);
                    }

                    if (CurrentTarget != null)
                        AssignPower();

                    // Pop a potion when necessary

                    if (UsePotionIfNeededTask())
                    {
                        return GetRunStatus(RunStatus.Running);
                    }

                    using (new PerformanceLogger("HandleTarget.CheckAvoidanceBuffs"))
                    {
                        // See if we can use any special buffs etc. while in avoidance
                        if (CurrentTarget.Type == TrinityObjectType.Avoidance)
                        {
                            PowerBuff = AbilitySelector(true);
                            if (PowerBuff.SNOPower != SNOPower.None)
                            {
                                ZetaDia.Me.UsePower(PowerBuff.SNOPower, PowerBuff.TargetPosition, PowerBuff.TargetDynamicWorldId, PowerBuff.TargetACDGUID);
                                LastPowerUsed = PowerBuff.SNOPower;
                                CacheData.AbilityLastUsed[PowerBuff.SNOPower] = DateTime.UtcNow;
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
                        else if (Settings.Combat.Misc.UseNavMeshTargeting && CurrentTarget.Type != TrinityObjectType.Barricade && CurrentTarget.Type != TrinityObjectType.Destructible)
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
                            ((CurrentTarget.Type == TrinityObjectType.Barricade ||
                             CurrentTarget.Type == TrinityObjectType.Interactable ||
                             CurrentTarget.Type == TrinityObjectType.CursedChest ||
                             CurrentTarget.Type == TrinityObjectType.CursedShrine ||
                             CurrentTarget.Type == TrinityObjectType.Destructible) &&
                             !ZetaDia.Me.Movement.IsMoving && DateTime.UtcNow.Subtract(PlayerMover.TimeLastUsedPlayerMover).TotalMilliseconds < 250);

                        bool npcInRange = CurrentTarget.IsQuestGiver && CurrentTarget.RadiusDistance <= 3f;

                        bool noRangeRequired = TargetRangeRequired <= 1f;
                        switch (CurrentTarget.Type)
                        {
                            // These always have TargetRangeRequired=1f, but, we need to run directly to their center until we stop moving, then destroy them
                            case TrinityObjectType.Door:
                            case TrinityObjectType.Barricade:
                            case TrinityObjectType.Destructible:
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
                            return GetRunStatus(RunStatus.Running);
                        }

                    }


                    using (new PerformanceLogger("HandleTarget.UpdateStatusText"))
                    {
                        // Out-of-range, so move towards the target
                        UpdateStatusTextTarget(false);
                    }

                    // Are we currently incapacitated? If so then wait...
                    if (Player.IsIncapacitated || Player.IsRooted)
                    {
                        Logger.Log(LogCategory.Behavior, "Player is rooted or incapacitated!");
                        return GetRunStatus(RunStatus.Running);
                    }

                    // Check to see if we're stuck in moving to the target
                    if (HandleTargetDistanceCheck())
                    {
                        return GetRunStatus(RunStatus.Running);
                    }
                    // Update the last distance stored
                    LastDistanceFromTarget = TargetCurrentDistance;

                    if (TimeSinceUse(SNOPower.Monk_TempestRush) < 250)
                    {
                        ForceNewMovement = true;
                    }

                    // Only position-shift when not avoiding
                    // See if we want to ACTUALLY move, or are just waiting for the last move command...
                    if (!ForceNewMovement && IsAlreadyMoving && CurrentDestination == LastMoveToTarget && DateTime.UtcNow.Subtract(LastMovementCommand).TotalMilliseconds <= 100)
                    {
                        // return GetTaskResult(true);
                    }
                    using (new PerformanceLogger("HandleTarget.SpecialMovement"))
                    {

                        bool Monk_SpecialMovement = ((CurrentTarget.Type == TrinityObjectType.Gold ||
                            CurrentTarget.IsUnit || CurrentTarget.IsDestroyable) && MonkCombat.IsTempestRushReady());

                        // If we're doing avoidance, globes or backtracking, try to use special abilities to move quicker
                        if ((CurrentTarget.Type == TrinityObjectType.Avoidance ||
                            CurrentTarget.Type == TrinityObjectType.HealthGlobe ||
                            CurrentTarget.Type == TrinityObjectType.PowerGlobe ||
                            CurrentTarget.Type == TrinityObjectType.ProgressionGlobe ||
                            CurrentTarget.Type == TrinityObjectType.Shrine ||
                            Monk_SpecialMovement)
                            && NavHelper.CanRayCast(Player.Position, CurrentDestination)
                            )
                        {
                            bool usedSpecialMovement = UsedSpecialMovement();

                            if (usedSpecialMovement)
                            {
                                // Store the current destination for comparison incase of changes next loop
                                LastMoveToTarget = CurrentDestination;
                                // Reset total body-block count, since we should have moved
                                if (DateTime.UtcNow.Subtract(LastForcedKeepCloseRange).TotalMilliseconds >= 2000)
                                    TimesBlockedMoving = 0;

                                return GetRunStatus(RunStatus.Running);
                            }
                        }
                    }

                    // DemonHunter Strafe
                    if (Skills.DemonHunter.Strafe.IsActive && Player.PrimaryResource > 12 && TargetUtil.AnyMobsInRange(30f, false))
                    {
                        Skills.DemonHunter.Strafe.Cast(CurrentDestination);
                        return GetRunStatus(RunStatus.Running);
                    }

                    if (Player.ActorClass == ActorClass.Monk && CombatBase.CanCast(SNOPower.X1_Monk_DashingStrike) && ((Skills.Monk.DashingStrike.Charges > 1 && ZetaDia.Me.CurrentPrimaryResource > 75) || CacheData.Buffs.HasCastingShrine))
					{
						Logger.Log("Dash towards: {0}, charges={1}", GetTargetName(), Skills.Monk.DashingStrike.Charges);
                        Skills.Monk.DashingStrike.Cast(CurrentDestination);
						
						return GetRunStatus(RunStatus.Running);
					}

                    if (Player.ActorClass == ActorClass.Barbarian)
                    {
                        bool wwToItem = (CurrentTarget.Type != TrinityObjectType.Item || (CurrentTarget.Type == TrinityObjectType.Item && CurrentTarget.Distance > 10f && NavHelper.CanRayCast(CurrentTarget.Position)));
                        // Whirlwind against everything within range
                        if (Player.PrimaryResource >= 10 && CombatBase.CanCast(SNOPower.Barbarian_Whirlwind) && wwToItem &&
                            (TargetUtil.AnyMobsInRange(20, false) || Sets.BulKathossOath.IsFullyEquipped) && !IsWaitingForSpecial)
                        {
                            Skills.Barbarian.Whirlwind.Cast(CurrentDestination);
                            LastMoveToTarget = CurrentDestination;
                            return GetRunStatus(RunStatus.Running);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.LogError("Error in HandleTarget: {0}", ex);
                    return GetRunStatus(RunStatus.Failure);
                }

                HandleTargetBasicMovement(ForceNewMovement);
                Logger.LogDebug(LogCategory.Behavior, "End of HandleTarget");
                return GetRunStatus(RunStatus.Running);
            }
        }

        private static void HandleObjectInRange()
        {
            switch (CurrentTarget.Type)
            {
                case TrinityObjectType.Avoidance:
                    _forceTargetUpdate = true;
                    break;
                case TrinityObjectType.Player:
                    break;

                // Unit, use our primary power to attack
                case TrinityObjectType.Unit:
                    {
                        if (CombatBase.CurrentPower.SNOPower != SNOPower.None)
                        {
                            if (IsWaitingForPower && CombatBase.CurrentPower.ShouldWaitBeforeUse)
                            {
                            }
                            else if (IsWaitingForPower && !CombatBase.CurrentPower.ShouldWaitBeforeUse)
                            {
                                IsWaitingForPower = false;
                            }
                            else
                            {
                                IsWaitingForPower = false;
                                HandleUnitInRange();
                            }
                        }
                        break;
                    }
                // Item, interact with it and log item stats
                case TrinityObjectType.Item:
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
                case TrinityObjectType.Gold:
                case TrinityObjectType.HealthGlobe:
                case TrinityObjectType.PowerGlobe:
                case TrinityObjectType.ProgressionGlobe:
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
                        IgnoreRactorGuid = CurrentTarget.RActorGuid;
                        IgnoreTargetForLoops = 3;

                        // Now tell Trinity to get a new target!
                        _forceTargetUpdate = true;
                        break;
                    }

                case TrinityObjectType.Door:
                case TrinityObjectType.HealthWell:
                case TrinityObjectType.Shrine:
                case TrinityObjectType.Container:
                case TrinityObjectType.Interactable:
                case TrinityObjectType.CursedChest:
                case TrinityObjectType.CursedShrine:
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

                            Logger.LogDebug(LogCategory.UserInformation, "Interacting with {1} Distance {2:0} Radius {3:0.0} Attempt {4}",
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
                            if (CacheData.InteractAttempts[CurrentTarget.RActorGuid] > 15 && CurrentTarget.Type != TrinityObjectType.HealthWell)
                            {
                                Logger.LogVerbose("Blacklisting {0} ({1}) for 15 seconds after {2} interactions",
                                    CurrentTarget.InternalName, CurrentTarget.ActorSNO, attemptCount);
                                Blacklist15Seconds.Add(CurrentTarget.RActorGuid);
                            }
                        }
                        break;
                    }
                // * Destructible - need to pick an ability and attack it
                case TrinityObjectType.Destructible:
                case TrinityObjectType.Barricade:
                    {
                        if (CombatBase.CurrentPower.SNOPower != SNOPower.None)
                        {
                            if (CurrentTarget.Type == TrinityObjectType.Barricade)
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

                            if (CurrentTarget.RActorGuid == IgnoreRactorGuid || DataDictionary.DestroyAtLocationIds.Contains(CurrentTarget.ActorSNO))
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
                                    MonkCombat.LastTempestRushLocation = vAttackPoint;
                            }
                            else
                            {
                                // Standard attack - attack the ACDGUID (equivalent of left-clicking the object in-game)
                                ZetaDia.Me.UsePower(CombatBase.CurrentPower.SNOPower, Vector3.Zero, -1, CurrentTarget.ACDGuid);
                                if (CombatBase.CurrentPower.SNOPower == SNOPower.Monk_TempestRush)
                                    MonkCombat.LastTempestRushLocation = CurrentTarget.Position;
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
                            IgnoreRactorGuid = CurrentTarget.RActorGuid;
                            IgnoreTargetForLoops = 3;
                            // Add this destructible/barricade to our very short-term ignore list
                            //Destructible3SecBlacklist.Add(CurrentTarget.RActorGuid);
                            Logger.Log(TrinityLogLevel.Debug, LogCategory.Behavior, "Blacklisting {0} {1} {2} for 3 seconds for Destrucable attack", CurrentTarget.Type, CurrentTarget.InternalName, CurrentTarget.ActorSNO);
                            LastDestroyedDestructible = DateTime.UtcNow;
                            NeedClearDestructibles = true;
                        }
                        // Now tell Trinity to get a new target!
                        _forceTargetUpdate = true;
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

        private static bool HandleTargetDistanceCheck()
        {
            using (new PerformanceLogger("HandleTarget.DistanceEqualCheck"))
            {
                // Count how long we have failed to move - body block stuff etc.
                if (Math.Abs(TargetCurrentDistance - LastDistanceFromTarget) < 5f && PlayerMover.GetMovementSpeed() < 1)
                {
                    ForceNewMovement = true;
                    if (DateTime.UtcNow.Subtract(LastMovedDuringCombat).TotalMilliseconds >= 250)
                    {
                        LastMovedDuringCombat = DateTime.UtcNow;
                        // We've been stuck at least 250 ms, let's go and pick new targets etc.
                        TimesBlockedMoving++;
                        ForceCloseRangeTarget = true;
                        LastForcedKeepCloseRange = DateTime.UtcNow;
                        // And tell Trinity to get a new target
                        _forceTargetUpdate = true;

                        // Reset the emergency loop counter and return success
                        return true;
                    }
                }
                else
                {
                    // Movement has been made, so count the time last moved!
                    LastMovedDuringCombat = DateTime.UtcNow;
                }
            }
            return false;
        }

        /// <summary>
        /// Handles target blacklist assignment if necessary, used for all targets (units/gold/items/interactables)
        /// </summary>
        /// <param name="runStatus"></param>
        /// <returns></returns>
        private static bool HandleTargetTimeoutTask()
        {
            using (new PerformanceLogger("HandleTarget.TargetTimeout"))
            {
                // Been trying to handle the same target for more than 30 seconds without damaging/reaching it? Blacklist it!
                // Note: The time since target picked updates every time the current target loses health, if it's a monster-target
                // Don't blacklist stuff if we're playing a cutscene

                bool shouldTryBlacklist = false;

                // don't timeout on avoidance
                if (CurrentTarget.Type == TrinityObjectType.Avoidance)
                    return false;

                // don't timeout on legendary items
                if (CurrentTarget.Type == TrinityObjectType.Item && CurrentTarget.ItemQuality >= ItemQuality.Legendary)
                    return false;

                // don't timeout if we're actively moving
                if (PlayerMover.GetMovementSpeed() > 1)
                    return false;

                if (CurrentTargetIsNonUnit() && GetSecondsSinceTargetUpdate() > 6)
                    shouldTryBlacklist = true;

                if ((CurrentTargetIsUnit() && CurrentTarget.IsBoss && GetSecondsSinceTargetUpdate() > 45))
                    shouldTryBlacklist = true;

                // special raycast check for current target after 10 sec
                if ((CurrentTargetIsUnit() && !CurrentTarget.IsBoss && GetSecondsSinceTargetUpdate() > 10))
                    shouldTryBlacklist = true;

                if (CurrentTarget.Type == TrinityObjectType.HotSpot)
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

                    if ((CurrentTarget.Type == TrinityObjectType.Door || CurrentTarget.Type == TrinityObjectType.Interactable || CurrentTarget.Type == TrinityObjectType.Container) &&
                        interactAttempts < 45 && DateTime.UtcNow.Subtract(PlayerMover.LastRecordedAnyStuck).TotalSeconds > 15)
                    {
                        addTargetToBlacklist = false;
                    }

                    if (addTargetToBlacklist)
                    {
                        if (CurrentTarget.IsBoss)
                        {
                            Blacklist15Seconds.Add(CurrentTarget.RActorGuid);
                            Blacklist15LastClear = DateTime.UtcNow;
                            CurrentTarget = null;
                            return true;
                        }
                        if (CurrentTarget.Type == TrinityObjectType.Item && CurrentTarget.ItemQuality >= ItemQuality.Legendary)
                        {
                            return false;
                        }

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

                        Blacklist90Seconds.Add(CurrentTarget.RActorGuid);
                        Blacklist90LastClear = DateTime.UtcNow;
                        CurrentTarget = null;
                        return true;
                    }
                }
                return false;
            }
        }

        /// <summary>
        /// Checks to see if we need a new monster power and will assign it to <see cref="CurrentPower"/>, distinguishes destructables/barricades from units
        /// </summary>
        private static void AssignPower()
        {
            using (new PerformanceLogger("HandleTarget.AssignMonsterTargetPower"))
            {
                // Find a valid ability if the target is a monster
                if (ShouldPickNewAbilities && !IsWaitingForPower && !IsWaitingForPotion)
                {
                    ShouldPickNewAbilities = false;
                    if (CurrentTarget.IsUnit)
                    {
                        // Pick a suitable ability
                        CombatBase.CurrentPower = AbilitySelector();

                        if (Player.IsInCombat && CombatBase.CurrentPower.SNOPower == SNOPower.None && !Player.IsIncapacitated)
                        {
                            NoAbilitiesAvailableInARow++;
                            if (DateTime.UtcNow.Subtract(LastRemindedAboutAbilities).TotalSeconds > 60 && NoAbilitiesAvailableInARow >= 4)
                            {
                                LastRemindedAboutAbilities = DateTime.UtcNow;
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
                    if (CurrentTarget.Type == TrinityObjectType.Destructible || CurrentTarget.Type == TrinityObjectType.Barricade)
                        CombatBase.CurrentPower = AbilitySelector(UseDestructiblePower: true);

                    // Return since we should have assigned a power
                    return;
                }
                if (!IsWaitingForPower && CombatBase.CurrentPower == null)
                {
                    CombatBase.CurrentPower = AbilitySelector(UseOOCBuff: true);
                }
            }
        }

        /// <summary>
        /// Will check <see cref=" IsWaitingForPotion"/> and Use a Potion if needed
        /// </summary>
        public static bool UsePotionIfNeededTask()
        {
            using (new PerformanceLogger("HandleTarget.UseHealthPotionIfNeeded"))
            {
                if (!Player.IsIncapacitated && Player.CurrentHealthPct > 0 && SpellHistory.TimeSinceUse(SNOPower.DrinkHealthPotion) > TimeSpan.FromSeconds(30) &&
                    Player.CurrentHealthPct <= CombatBase.EmergencyHealthPotionLimit)
                {
                    var legendaryPotions = CacheData.Inventory.Backpack.Where(i => i.InternalName.ToLower()
                        .Contains("healthpotion_legendary_")).ToList();

                    if (legendaryPotions.Any())
                    {
                        Logger.Log(TrinityLogLevel.Verbose, LogCategory.UserInformation, "Using Legendary Potion", 0);
                        int dynamicId = legendaryPotions.FirstOrDefault().DynamicId;
                        ZetaDia.Me.Inventory.UseItem(dynamicId);
                        SpellHistory.RecordSpell(new TrinityPower(SNOPower.DrinkHealthPotion));
                        return true;
                    }
                    var potion = ZetaDia.Me.Inventory.BaseHealthPotion;
                    if (potion != null)
                    {
                        Logger.Log(TrinityLogLevel.Verbose, LogCategory.UserInformation, "Using Potion", 0);
                        ZetaDia.Me.Inventory.UseItem(potion.DynamicId);
                        SpellHistory.RecordSpell(new TrinityPower(SNOPower.DrinkHealthPotion));
                        return true;
                    }

                    Logger.Log(TrinityLogLevel.Verbose, LogCategory.UserInformation, "No Available potions!", 0);
                }
                return false;
            }
        }

        /// <summary>
        /// If we can use special class movement abilities, this will use it and return true
        /// </summary>
        /// <returns></returns>
        private static bool UsedSpecialMovement()
        {
            bool attackableSpecialMovement = ((CurrentTarget.Type == TrinityObjectType.Avoidance &&
            ObjectCache.Any(u => (u.IsUnit || u.Type == TrinityObjectType.Destructible || u.Type == TrinityObjectType.Barricade) &&
                MathUtil.IntersectsPath(u.Position, u.Radius, Player.Position, CurrentTarget.Position))));

            using (new PerformanceLogger("HandleTarget.UsedSpecialMovement"))
            {
                // Leap movement for a barb
                if (CombatBase.CanCast(SNOPower.Barbarian_Leap))
                {
                    ZetaDia.Me.UsePower(SNOPower.Barbarian_Leap, CurrentDestination, CurrentWorldDynamicId);
                    SpellHistory.RecordSpell(SNOPower.Barbarian_Leap);
                    return true;
                }

                // Furious Charge movement for a barb
                if (CombatBase.CanCast(SNOPower.Barbarian_FuriousCharge) && Settings.Combat.Barbarian.UseChargeOOC)
                {
                    ZetaDia.Me.UsePower(SNOPower.Barbarian_FuriousCharge, CurrentDestination, CurrentWorldDynamicId);
                    SpellHistory.RecordSpell(SNOPower.Barbarian_FuriousCharge);
                    return true;
                }

                // Whirlwind for a barb
                if (attackableSpecialMovement && !IsWaitingForSpecial && CombatBase.CurrentPower.SNOPower != SNOPower.Barbarian_WrathOfTheBerserker
                    && Hotbar.Contains(SNOPower.Barbarian_Whirlwind) && Player.PrimaryResource >= 10)
                {
                    ZetaDia.Me.UsePower(SNOPower.Barbarian_Whirlwind, CurrentDestination, CurrentWorldDynamicId);
                    // Store the current destination for comparison incase of changes next loop
                    LastMoveToTarget = CurrentDestination;
                    // Reset total body-block count, since we should have moved
                    if (DateTime.UtcNow.Subtract(LastForcedKeepCloseRange).TotalMilliseconds >= 2000)
                        TimesBlockedMoving = 0;
                    return true;
                }

                // Vault for a Demon Hunter
                if (CombatBase.CanCast(SNOPower.DemonHunter_Vault) && Settings.Combat.DemonHunter.VaultMode != DemonHunterVaultMode.MovementOnly &&
                    (CombatBase.KiteDistance <= 0 || (!CacheData.MonsterObstacles.Any(a => a.Position.Distance(CurrentDestination) <= CombatBase.KiteDistance) &&
                    !CacheData.TimeBoundAvoidance.Any(a => a.Position.Distance(CurrentDestination) <= CombatBase.KiteDistance))) &&
                    (!CacheData.TimeBoundAvoidance.Any(a => MathEx.IntersectsPath(a.Position, a.Radius, Player.Position, CurrentDestination))))
                {
                    ZetaDia.Me.UsePower(SNOPower.DemonHunter_Vault, CurrentDestination, CurrentWorldDynamicId);
                    SpellHistory.RecordSpell(SNOPower.DemonHunter_Vault);
                    return true;
                }

                // Teleport for a wizard (need to be able to check skill rune in DB for a 3-4 teleport spam in a row)
                if (CombatBase.CanCast(SNOPower.Wizard_Teleport))
                {
                    ZetaDia.Me.UsePower(SNOPower.Wizard_Teleport, CurrentDestination, CurrentWorldDynamicId);
                    SpellHistory.RecordSpell(SNOPower.Wizard_Teleport);
                    return true;
                }

                // Archon Teleport for a wizard (need to be able to check skill rune in DB for a 3-4 teleport spam in a row)
                if (CombatBase.CanCast(SNOPower.Wizard_Archon_Teleport))
                {
                    ZetaDia.Me.UsePower(SNOPower.Wizard_Archon_Teleport, CurrentDestination, CurrentWorldDynamicId, -1);
                    SpellHistory.RecordSpell(SNOPower.Wizard_Archon_Teleport);
                    return true;
                }

                // Tempest rush for a monk
                if (CombatBase.CanCast(SNOPower.Monk_TempestRush) && Player.PrimaryResource >= Settings.Combat.Monk.TR_MinSpirit &&
                    ((CurrentTarget.Type == TrinityObjectType.Item && CurrentTarget.Distance > 20f) || CurrentTarget.Type != TrinityObjectType.Item) &&
                    Settings.Combat.Monk.TROption != TempestRushOption.MovementOnly &&
                    MonkCombat.IsTempestRushReady())
                {
                    ZetaDia.Me.UsePower(SNOPower.Monk_TempestRush, CurrentDestination, CurrentWorldDynamicId);
                    CacheData.AbilityLastUsed[SNOPower.Monk_TempestRush] = DateTime.UtcNow;
                    LastPowerUsed = SNOPower.Monk_TempestRush;
                    MonkCombat.LastTempestRushLocation = CurrentDestination;
                    // Store the current destination for comparison incase of changes next loop
                    LastMoveToTarget = CurrentDestination;
                    // Reset total body-block count, since we should have moved
                    if (DateTime.UtcNow.Subtract(LastForcedKeepCloseRange).TotalMilliseconds >= 2000)
                        TimesBlockedMoving = 0;
                    return true;
                }

                // Strafe for a Demon Hunter
                if (attackableSpecialMovement && CombatBase.CanCast(SNOPower.DemonHunter_Strafe))
                {
                    ZetaDia.Me.UsePower(SNOPower.DemonHunter_Strafe, CurrentDestination, CurrentWorldDynamicId);
                    // Store the current destination for comparison incase of changes next loop
                    LastMoveToTarget = CurrentDestination;
                    // Reset total body-block count, since we should have moved
                    if (DateTime.UtcNow.Subtract(LastForcedKeepCloseRange).TotalMilliseconds >= 2000)
                        TimesBlockedMoving = 0;
                    return true;
                }

                return false;
            }
        }

        private static bool CurrentTargetIsNotAvoidance()
        {
            return CurrentTarget.Type != TrinityObjectType.Avoidance;
        }

        private static bool CurrentTargetIsNonUnit()
        {
            return CurrentTarget.Type != TrinityObjectType.Unit;
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
            return DateTime.UtcNow.Subtract(LastPickedTargetTime).TotalSeconds;
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
                    case TrinityObjectType.Avoidance:
                        action = "Avoid ";
                        break;
                    case TrinityObjectType.Unit:
                        action = "Attack ";
                        break;
                    case TrinityObjectType.Item:
                    case TrinityObjectType.Gold:
                    case TrinityObjectType.PowerGlobe:
                    case TrinityObjectType.HealthGlobe:
                    case TrinityObjectType.ProgressionGlobe:
                        action = "Pickup ";
                        break;
                    case TrinityObjectType.Interactable:
                        action = "Interact ";
                        break;
                    case TrinityObjectType.Door:
                    case TrinityObjectType.Container:
                        action = "Open ";
                        break;
                    case TrinityObjectType.Destructible:
                    case TrinityObjectType.Barricade:
                        action = "Destroy ";
                        break;
                    case TrinityObjectType.Shrine:
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
            statusText.Append(ZetaDia.Me.Movement.SpeedXY.ToString("0.00"));
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

            statusText.Append(String.Format(" Duration={0:0}", DateTime.UtcNow.Subtract(LastPickedTargetTime).TotalSeconds));

            if (Settings.Advanced.DebugInStatusBar)
            {
                StatusText = statusText.ToString();
                BotMain.StatusText = StatusText;
            }
            if (lastStatusText != statusText.ToString())
            {
                // prevent spam
                lastStatusText = statusText.ToString();
                Logger.Log(TrinityLogLevel.Debug, LogCategory.Targetting, "{0}", statusText.ToString());
                ResetStatusText = true;
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
                LastMovementCommand = DateTime.UtcNow;

                if (DateTime.UtcNow.Subtract(LastSentMovePower).TotalMilliseconds >= 250 || Vector3.Distance(LastMoveToTarget, CurrentDestination) >= 2f || bForceNewMovement)
                {
                    bool straightLinePathing = DataDictionary.StraightLinePathingLevelAreaIds.Contains(Player.LevelAreaId);

                    string destname = String.Format("{0} {1:0} yds Elite={2} LoS={3} HP={4:0.00} Dir={5}",
                        CurrentTarget.InternalName,
                        CurrentTarget.Distance,
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

                    LastSentMovePower = DateTime.UtcNow;

                    bool inRange = TargetCurrentDistance <= TargetRangeRequired || CurrentTarget.Distance < 10f;
                    if (lastMoveResult == MoveResult.ReachedDestination && !inRange &&
                        CurrentTarget.Type != TrinityObjectType.Item &&
                        CurrentTarget.Type != TrinityObjectType.Destructible &&
                        CurrentTarget.Type != TrinityObjectType.Barricade)
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
                    if (DateTime.UtcNow.Subtract(LastForcedKeepCloseRange).TotalMilliseconds >= 2000)
                        TimesBlockedMoving = 0;
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
                    case TrinityObjectType.Unit:
                        {
                            // Pick a range to try to reach
                            TargetRangeRequired = CombatBase.CurrentPower.MinimumRange;
                            break;
                        }
                    // * Item - need to get within 6 feet and then interact with it
                    case TrinityObjectType.Item:
                        {
                            TargetRangeRequired = 2f;
                            TargetCurrentDistance = CurrentTarget.Distance;
                            break;
                        }
                    // * Gold - need to get within pickup radius only
                    case TrinityObjectType.Gold:
                        {
                            TargetRangeRequired = 2f;
                            TargetCurrentDistance = CurrentTarget.Distance;
                            CurrentDestination = MathEx.CalculatePointFrom(Player.Position, CurrentTarget.Position, -2f);
                            break;
                        }
                    // * Globes - need to get within pickup radius only
                    case TrinityObjectType.PowerGlobe:
                    case TrinityObjectType.HealthGlobe:
                    case TrinityObjectType.ProgressionGlobe:
                        {
                            TargetRangeRequired = 2f;
                            TargetCurrentDistance = CurrentTarget.Distance;
                            break;
                        }
                    // * Shrine & Container - need to get within 8 feet and interact
                    case TrinityObjectType.HealthWell:
                        {
                            TargetRangeRequired = 4f;

                            float range;
                            if (DataDictionary.CustomObjectRadius.TryGetValue(CurrentTarget.ActorSNO, out range))
                            {
                                TargetRangeRequired = range;
                            }
                            break;
                        }
                    case TrinityObjectType.Shrine:
                    case TrinityObjectType.Container:
                        {
                            TargetRangeRequired = 6f;

                            float range;
                            if (DataDictionary.CustomObjectRadius.TryGetValue(CurrentTarget.ActorSNO, out range))
                            {
                                TargetRangeRequired = range;
                            }
                            break;
                        }
                    case TrinityObjectType.Interactable:
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
                    case TrinityObjectType.Destructible:
                        {
                            // Pick a range to try to reach + (tmp_fThisRadius * 0.70);
                            //TargetRangeRequired = CombatBase.CurrentPower.SNOPower == SNOPower.None ? 9f : CombatBase.CurrentPower.MinimumRange;
                            TargetRangeRequired = CombatBase.CurrentPower.MinimumRange;
                            CurrentTarget.Radius = 1f;
                            TargetCurrentDistance = CurrentTarget.Distance;
                            break;
                        }
                    case TrinityObjectType.Barricade:
                        {
                            // Pick a range to try to reach + (tmp_fThisRadius * 0.70);
                            TargetRangeRequired = CombatBase.CurrentPower.MinimumRange;
                            CurrentTarget.Radius = 1f;
                            TargetCurrentDistance = CurrentTarget.Distance;
                            break;
                        }
                    // * Avoidance - need to pick an avoid location and move there
                    case TrinityObjectType.Avoidance:
                        {
                            TargetRangeRequired = 2f;
                            break;
                        }
                    case TrinityObjectType.Door:
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
                bool usePowerResult;

                // For "no-attack" logic
                if (CombatBase.CurrentPower.SNOPower == SNOPower.Walk && CombatBase.CurrentPower.TargetPosition == Vector3.Zero)
                {
                    Navigator.PlayerMover.MoveStop();
                    usePowerResult = true;
                }
                else
                {
                    usePowerResult = ZetaDia.Me.UsePower(CombatBase.CurrentPower.SNOPower, CombatBase.CurrentPower.TargetPosition, CombatBase.CurrentPower.TargetDynamicWorldId, CombatBase.CurrentPower.TargetACDGUID);
                }

                var skill = SkillUtils.ById(CombatBase.CurrentPower.SNOPower);
                string target = GetTargetName();

                if (usePowerResult)
                {
                    // Monk Stuffs get special attention
                    {
                        if (CombatBase.CurrentPower.SNOPower == SNOPower.Monk_TempestRush)
                            MonkCombat.LastTempestRushLocation = CombatBase.CurrentPower.TargetPosition;
                        if (CombatBase.CurrentPower.SNOPower == SNOPower.Monk_SweepingWind)
                            MonkCombat.LastSweepingWindRefresh = DateTime.UtcNow;

                        MonkCombat.RunOngoingPowers();
                    }


                    if (skill != null && skill.Meta != null)
                    {
                        Logger.LogVerbose("Used Power {0} ({1}) {2} Range={3} ({4} {5}) Delay={6}/{7} TargetDist={8} CurrentTarget={9} charges={10}",
                            skill.Name,
                            (int)skill.SNOPower,
                            target,
                            skill.Meta.CastRange,
                            skill.Meta.DebugResourceEffect,
                            skill.Meta.DebugType,
                            skill.Meta.BeforeUseDelay,
                            skill.Meta.AfterUseDelay,
                            Player.Position.Distance(CombatBase.CurrentPower.TargetPosition),
                            CurrentTarget != null ? CurrentTarget.InternalName : "Null",
                            skill.Charges
                            );

                    }
                    else
                    {
                        Logger.LogVerbose("Used Power {0} " + target, CombatBase.CurrentPower.SNOPower);

                    }

                    SpellTracker.TrackSpellOnUnit(CombatBase.CurrentPower.TargetACDGUID, CombatBase.CurrentPower.SNOPower);
                    SpellHistory.RecordSpell(CombatBase.CurrentPower);

                    CacheData.AbilityLastUsed[CombatBase.CurrentPower.SNOPower] = DateTime.UtcNow;
                    LastGlobalCooldownUse = DateTime.UtcNow;
                    LastPowerUsed = CombatBase.CurrentPower.SNOPower;

                    // See if we should force a long wait AFTERWARDS, too
                    // Force waiting AFTER power use for certain abilities
                    IsWaitingAfterPower = CombatBase.CurrentPower.ShouldWaitAfterUse;

                }
                else
                {
                    if (skill != null && skill.Meta != null)
                    {
                        Logger.LogVerbose(LogCategory.Behavior, "Failed to use Power {0} ({1}) {2} Range={3} ({4} {5}) Delay={6}/{11} TargetDist={7} CurrentTarget={10}",
                                       skill.Name,
                                       (int)skill.SNOPower,
                                       target,
                                       skill.Meta.CastRange,
                                       skill.Meta.DebugResourceEffect,
                                       skill.Meta.DebugType,
                                       skill.Meta.BeforeUseDelay,
                                       Player.Position.Distance(CombatBase.CurrentPower.TargetPosition),
                                       Player.IsFacing(CombatBase.CurrentPower.TargetPosition),
                                       CurrentTarget != null && Player.IsFacing(CurrentTarget.Position),
                                       CurrentTarget != null ? CurrentTarget.InternalName : "Null",
                                       skill.Meta.AfterUseDelay
                                       );
                    }
                    else
                    {
                        Logger.LogVerbose(LogCategory.Behavior, "Failed to use power {0} " + target, CombatBase.CurrentPower.SNOPower);

                    }
                }

                ShouldPickNewAbilities = true;

                // Keep looking for monsters at "normal kill range" a few moments after we successfully attack a monster incase we can pull them into range
                KeepKillRadiusExtendedForSeconds = 8;
                TimeKeepKillRadiusExtendedUntil = DateTime.UtcNow.AddSeconds(KeepKillRadiusExtendedForSeconds);
                KeepLootRadiusExtendedForSeconds = 8;
                // if at full or nearly full health, see if we can raycast to it, if not, ignore it for 2000 ms
                if (CurrentTarget.HitPointsPct >= 0.9d &&
                    !NavHelper.CanRayCast(Player.Position, CurrentTarget.Position) &&
                    !CurrentTarget.IsBoss &&
                    !(DataDictionary.StraightLinePathingLevelAreaIds.Contains(Player.LevelAreaId) || DataDictionary.LineOfSightWhitelist.Contains(CurrentTarget.ActorSNO)))
                {
                    IgnoreRactorGuid = CurrentTarget.RActorGuid;
                    IgnoreTargetForLoops = 6;
                    // Add this monster to our very short-term ignore list
                    Blacklist3Seconds.Add(CurrentTarget.RActorGuid);
                    Logger.Log(TrinityLogLevel.Verbose, LogCategory.Behavior, "Blacklisting {0} {1} {2} for 3 seconds due to Raycast failure", CurrentTarget.Type, CurrentTarget.InternalName, CurrentTarget.ActorSNO);
                    Blacklist3LastClear = DateTime.UtcNow;
                    NeedToClearBlacklist3 = true;
                }

            }
        }

        private static string GetTargetName()
        {
            float dist = 0;
            if (CombatBase.CurrentPower.TargetPosition != Vector3.Zero)
                dist = CombatBase.CurrentPower.TargetPosition.Distance2D(Player.Position);
            else if (CurrentTarget != null)
                dist = CurrentTarget.Position.Distance2D(Player.Position);

            string target = CombatBase.CurrentPower.TargetPosition != Vector3.Zero ? "at " + NavHelper.PrettyPrintVector3(CombatBase.CurrentPower.TargetPosition) + " dist=" + (int)dist : "";
            target += CombatBase.CurrentPower.TargetACDGUID != -1 ? " on " + CombatBase.CurrentPower.TargetACDGUID : "";
            return target;
        }

        private static int HandleItemInRange()
        {
            using (new PerformanceLogger("HandleTarget.HandleItemInRange"))
            {
                int iInteractAttempts;
                // Pick the item up the usepower way, and "blacklist" for a couple of loops
                ZetaDia.Me.UsePower(SNOPower.Axe_Operate_Gizmo, Vector3.Zero, 0, CurrentTarget.ACDGuid);
                IgnoreRactorGuid = CurrentTarget.RActorGuid;
                IgnoreTargetForLoops = 3;
                // Store item pickup stats

                string itemSha1Hash = HashGenerator.GenerateItemHash(CurrentTarget.Position, CurrentTarget.ActorSNO, CurrentTarget.InternalName, CurrentWorldDynamicId, CurrentTarget.ItemQuality, CurrentTarget.ItemLevel);
                if (!ItemDropStats._hashsetItemPicksLookedAt.Contains(itemSha1Hash))
                {
                    ItemDropStats._hashsetItemPicksLookedAt.Add(itemSha1Hash);
                    TrinityItemType itemType = TrinityItemManager.DetermineItemType(CurrentTarget.InternalName, CurrentTarget.DBItemType, CurrentTarget.FollowerType);
                    TrinityItemBaseType itemBaseType = TrinityItemManager.DetermineBaseType(itemType);
                    if (itemBaseType == TrinityItemBaseType.Armor || itemBaseType == TrinityItemBaseType.WeaponOneHand || itemBaseType == TrinityItemBaseType.WeaponTwoHand ||
                        itemBaseType == TrinityItemBaseType.WeaponRange || itemBaseType == TrinityItemBaseType.Jewelry || itemBaseType == TrinityItemBaseType.FollowerItem ||
                        itemBaseType == TrinityItemBaseType.Offhand)
                    {
                        int iQuality;
                        ItemDropStats.ItemsPickedStats.Total++;
                        if (CurrentTarget.ItemQuality >= ItemQuality.Legendary)
                            iQuality = ItemDropStats.QUALITYORANGE;
                        else if (CurrentTarget.ItemQuality >= ItemQuality.Rare4)
                            iQuality = ItemDropStats.QUALITYYELLOW;
                        else if (CurrentTarget.ItemQuality >= ItemQuality.Magic1)
                            iQuality = ItemDropStats.QUALITYBLUE;
                        else
                            iQuality = ItemDropStats.QUALITYWHITE;
                        //asserts	
                        if (iQuality > ItemDropStats.QUALITYORANGE)
                        {
                            Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "ERROR: Item type (" + iQuality + ") out of range");
                        }
                        if ((CurrentTarget.ItemLevel < 0) || (CurrentTarget.ItemLevel >= 74))
                        {
                            Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "ERROR: Item level (" + CurrentTarget.ItemLevel + ") out of range");
                        }
                        ItemDropStats.ItemsPickedStats.TotalPerQuality[iQuality]++;
                        ItemDropStats.ItemsPickedStats.TotalPerLevel[CurrentTarget.ItemLevel]++;
                        ItemDropStats.ItemsPickedStats.TotalPerQPerL[iQuality, CurrentTarget.ItemLevel]++;
                    }
                    else if (itemBaseType == TrinityItemBaseType.Gem)
                    {
                        int iGemType = 0;
                        ItemDropStats.ItemsPickedStats.TotalGems++;
                        if (itemType == TrinityItemType.Topaz)
                            iGemType = ItemDropStats.GEMTOPAZ;
                        if (itemType == TrinityItemType.Ruby)
                            iGemType = ItemDropStats.GEMRUBY;
                        if (itemType == TrinityItemType.Emerald)
                            iGemType = ItemDropStats.GEMEMERALD;
                        if (itemType == TrinityItemType.Amethyst)
                            iGemType = ItemDropStats.GEMAMETHYST;
                        if (itemType == TrinityItemType.Diamond)
                            iGemType = ItemDropStats.GEMDIAMOND;

                        ItemDropStats.ItemsPickedStats.GemsPerType[iGemType]++;
                        ItemDropStats.ItemsPickedStats.GemsPerLevel[CurrentTarget.ItemLevel]++;
                        ItemDropStats.ItemsPickedStats.GemsPerTPerL[iGemType, CurrentTarget.ItemLevel]++;
                    }
                    else if (itemType == TrinityItemType.HealthPotion)
                    {
                        ItemDropStats.ItemsPickedStats.TotalPotions++;
                        if ((CurrentTarget.ItemLevel < 0) || (CurrentTarget.ItemLevel > 63))
                        {
                            Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "ERROR: Potion level ({0}) out of range", CurrentTarget.ItemLevel);
                        }
                        ItemDropStats.ItemsPickedStats.PotionsPerLevel[CurrentTarget.ItemLevel]++;
                    }
                    else if (_cItemTinityItemType == TrinityItemType.InfernalKey)
                    {
                        ItemDropStats.ItemsPickedStats.TotalInfernalKeys++;
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
