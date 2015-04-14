using System;
using System.Globalization;
using System.Linq;
using System.Text;
using Trinity.Combat;
using Trinity.Combat.Abilities;
using Trinity.Config.Combat;
using Trinity.DbProvider;
using Trinity.Items;
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
        /// Handles all aspects of moving to and attacking the current target
        /// </summary>
        /// <returns></returns>
        internal static RunStatus HandleTarget()
        {
            using (new MemorySpy("HandleTarget()"))
            {
                try
                {
                    if (!Player.IsInGame)
                        return GetRunStatus("NotInGame", RunStatus.Failure);

                    if (Player.IsLoadingWorld)
                        return GetRunStatus("LoadingWorld", RunStatus.Failure);

                    if (!Player.IsValid)
                        return GetRunStatus("MeInvalid", RunStatus.Failure);

                    if (!Player.CommonData.IsValid)
                        return GetRunStatus("CDInvalid", RunStatus.Failure);

                    if (Player.IsDead)
                        return GetRunStatus("HeroIsDead", RunStatus.Failure);

                    if (DemonHunterCombat.CurrentlyUseVault) // Wait a little
                        return GetRunStatus("HeroVault", RunStatus.Running);

                    if (MonkCombat.CurrentlyUseDashingStrike) // Wait a little
                        return GetRunStatus("HeroDash", RunStatus.Running);

                    if (BarbarianCombat.CurrentlyUseFuriousCharge) // Wait a little
                        return GetRunStatus("HeroCharge", RunStatus.Running);

                    if (UsePotionIfNeededTask()) // Pop a potion when necessary
                        return GetRunStatus("UsePotion", RunStatus.Running);

                    using (new MemorySpy("HandleTarget().CheckAvoidDeath"))
                    {
                        if (Settings.Combat.Misc.AvoidDeath &&
                            Player.CurrentHealthPct <= CombatBase.EmergencyHealthPotionLimit &&
                            !SNOPowerUseTimer(SNOPower.DrinkHealthPotion) &&
                            TargetUtil.AnyMobsInRange(90f, false))
                        {
                            Logger.LogNormal("Attempting to avoid death!");
                            Trinity.Player.AvoidDeath = true;
                        }
                        else
                        {
                            Trinity.Player.AvoidDeath = false;
                        }
                    }

                    RefreshDiaObjects(); // Refresh cache

                    using (new MemorySpy("HandleTarget().HandleAssignAbilityTask"))
                    { HandleAssignAbilityTask(); }

                    using (new MemorySpy("HandleTarget().SetTargetFields"))
                    { SetTargetFields(); }

                    using (new MemorySpy("HandleTarget().SetQueuedSpecialMovement"))
                    { SetQueuedSpecialMovement(); }

                    if (HandlePowerWaitTask()) // Waiting for/after power            
                        return GetRunStatus("WaitForPower", RunStatus.Running);

                    if (HandleTownRunWaitTask()) // Waiting for town run              
                        return GetRunStatus("WaitForTownRun", RunStatus.Running);

                    if (HandleTownRunReadyTask()) // Waiting for town run               
                        return GetRunStatus("TownRunReady", RunStatus.Success);

                    if (CurrentTarget == null) // CurrentTarget is null
                        return GetRunStatus("TargetNull", RunStatus.Failure);

                    if (HandleTargetTimeoutTask()) // Handle Target stuck / timeout
                        return GetRunStatus("TargetTimeOut", RunStatus.Failure);

                    // Save route
                    PositionCache.AddPosition();
                    PlayerMover.RecordSkipAheadCachePoint();

                    // Cast power/pick up item/buffs ...
                    if (HandleObjectInRange())
                        return GetRunStatus("HandleObject", RunStatus.Running);

                    // Target is not in range, update status
                    UpdateStatusTextTarget(false);

                    // Check if incapacited or rooted
                    if (Player.IsIncapacitated || Player.IsRooted)
                        return GetRunStatus("HeroRooted", RunStatus.Running);

                    // Check to see if we're stuck in moving to the target
                    if (HandleTargetDistanceCheck())
                        return GetRunStatus("TargetStuck", RunStatus.Running);

                    // Check if can use special movement 
                    if (UsedSpecialMovement())
                        return GetRunStatus("SpecialMovement", RunStatus.Running);

                }
                catch (Exception ex)
                {
                    Logger.LogError("Error in HandleTarget: {0}", ex);
                    return GetRunStatus("Error", RunStatus.Running);
                }

                if (TimeSinceUse(SNOPower.Monk_TempestRush) < 250)
                    ForceNewMovement = true;

                using (new MemorySpy("HandleTarget().SetQueuedBasicMovement"))
                { SetQueuedBasicMovement(ForceNewMovement); }

                if (CombatBase.QueuedMovement.IsQueuedMovement)
                    CombatBase.QueuedMovement.Execute();

                Logger.LogDebug(LogCategory.Behavior, "End of HandleTarget");
                return GetRunStatus("EndLoop", RunStatus.Running);
            }
        }

        /// <summary>
        /// Returns a RunStatus, if appropriate. Throws an exception if not.
        /// </summary>
        /// <param name="rs"></param>
        /// <returns></returns>
        private static RunStatus GetRunStatus(string post, RunStatus status)
        {
            Logger.Log(TrinityLogLevel.Debug, LogCategory.Targetting, "| HandleTarget returning {0} reason:{1}", status, post);

            if (Trinity.Settings.Advanced.LogCategories.HasFlag(LogCategory.Targetting))
            {
                if (CurrentTarget != null)
                {
                    Logger.Log(TrinityLogLevel.Debug, LogCategory.Targetting,
                        "| =>   CurrentTarget  : {0}",
                        CurrentTarget.Infos);
                }

                if (!CombatBase.IsNull(CombatBase.CurrentPower))
                {
                    Logger.Log(TrinityLogLevel.Debug, LogCategory.Targetting,
                        "| =>   CurrentPower   : {0}", CombatBase.CurrentPower.SNOPower + " InRange:" + CurrentTargetIsInRange + " Dist:" + TargetCurrentDistance);
                    Logger.Log(TrinityLogLevel.Debug, LogCategory.Targetting,
                        "| =>   MCD:{0} TCD{1} TCDi:{2} CPTP:{3} CPMP:{4} ACD:{5}",
                        CurrentMoveDestination, TargetCurrentDestination, TargetCurrentDistance, CombatBase.CurrentPower.TargetPosition, CombatBase.CurrentPower.MovePosition, CombatBase.CurrentPower.TargetACDGUID);
                }

                if (CombatBase.QueuedMovement.IsQueuedMovement)
                {
                    Logger.Log(TrinityLogLevel.Debug, LogCategory.Targetting,
                        "| =>   QueuedMovement : {0}",
                        "Type: " + CombatBase.QueuedMovement.CurrentMovement.Options.Type + " Infos: " + CombatBase.QueuedMovement.CurrentMovement.Infos);
                }

                if (ObjectCache != null)
                {
                    Logger.Log(TrinityLogLevel.Debug, LogCategory.Targetting,
                        "| =>   ObjectCache    : {0}",
                        ObjectCache.Count().ToString() + " objects");
                }

                if (MainGrid.Map.Any() && GridMap.GetBestClusterNode() != null)
                {
                    Logger.Log(TrinityLogLevel.Debug, LogCategory.Targetting,
                        "| =>   ClusterNode    : {0}",
                        GridMap.GetBestClusterNode().ClusterWeightInfos + " Dist:" + GridMap.GetBestClusterNode().Distance);
                }

                if (MainGrid.Map.Any() && GridMap.GetBestMoveNode() != null)
                {
                    Logger.Log(TrinityLogLevel.Debug, LogCategory.Targetting,
                        "| =>   MoveNode       : {0}",
                        GridMap.GetBestMoveNode().WeightInfos + " Dist:" + GridMap.GetBestMoveNode().Distance);
                }
            }

            return status;
        }

        private static bool HandlePowerWaitTask()
        {
            // Obsolet ?
            /*if (!_isWaitingForPower && CombatBase.CurrentPower == null && CurrentTarget != null)
                CombatBase.CurrentPower = AbilitySelector();

            // Time based wait delay for certain powers with animations
            if (!(IsWaitingAfterPower && CombatBase.CurrentPower.ShouldWaitAfterUse))
            {
                IsWaitingAfterPower = false;
            }
            else
            {
                //return true;
            }

            IsWaitingAfterPower = false;*/
            return false;
        }

        private static bool HandleTownRunWaitTask()
        {
            while (CurrentTarget == null && (ForceVendorRunASAP || WantToTownRun) && !Zeta.Bot.Logic.BrainBehavior.IsVendoring && TownRun.TownRunTimerRunning())
            {
                Logger.Log(TrinityLogLevel.Info, LogCategory.Behavior, "CurrentTarget is null but we are ready to to Town Run, waiting... ");
                return true;
            }

            while (CurrentTarget == null && TownRun.IsTryingToTownPortal() && TownRun.TownRunTimerRunning())
            {
                Logger.Log(TrinityLogLevel.Info, LogCategory.Behavior, "Waiting for town run... ");
                return true;
            }

            return false;
        }

        private static bool HandleTownRunReadyTask()
        {
            if (CurrentTarget == null && TownRun.IsTryingToTownPortal() && TownRun.TownRunTimerFinished())
            {
                Logger.Log(TrinityLogLevel.Info, LogCategory.Behavior, "Town Run Ready!");
                return true;
            }
            ;
            return false;
        }



        private static bool HandleObjectInRange()
        {
            if (CurrentTarget == null)
                return false;

            if (CurrentTarget.IsAvoidance)
            {
                TrinityPower power = AbilitySelector();
                if (!CombatBase.IsNull(power))
                    CombatBase.Cast(power);
            }

            bool stuckOnTarget =
                ((CurrentTarget.Type == GObjectType.Barricade ||
                CurrentTarget.Type == GObjectType.Interactable ||
                CurrentTarget.Type == GObjectType.CursedChest ||
                CurrentTarget.Type == GObjectType.CursedShrine ||
                CurrentTarget.Type == GObjectType.Destructible) &&
                !ZetaDia.Me.Movement.IsMoving && DateTime.UtcNow.Subtract(PlayerMover.TimeLastUsedPlayerMover).TotalMilliseconds < 250);

            bool npcInRange = CurrentTarget.IsQuestGiver && CurrentTarget.RadiusDistance <= 3f;

            bool noRangeRequired = CurrentTarget.RequiredRange <= 1f;
            switch (CurrentTarget.Type)
            {
                case GObjectType.Door:
                case GObjectType.Barricade:
                case GObjectType.Destructible:
                    noRangeRequired = false;
                    break;
            }

            // Interact/use power on target if already in range
            if (noRangeRequired || CurrentTargetIsInRange || stuckOnTarget || npcInRange)
            {
                Logger.LogDebug(LogCategory.Behavior, "Object in Range: noRangeRequired={0} Target In Range={1} stuckOnTarget={2} npcInRange={3}",
                    noRangeRequired, CurrentTargetIsInRange, stuckOnTarget, npcInRange);

                UpdateStatusTextTarget(true);

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
                            if (!CombatBase.IsNull(CombatBase.CurrentPower))
                            {
                                return HandleUnitInRange();
                            }
                            return false;
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
                                return HandleItemInRange() > 0;
                            }

                            return false;
                        }
                    // * Gold & Globe - need to get within pickup radius only
                    case GObjectType.Gold:
                    case GObjectType.HealthGlobe:
                    case GObjectType.PowerGlobe:
                    case GObjectType.ProgressionGlobe:
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
                            if (interactAttempts > 5)
                            {
                                Blacklist3Seconds.Add(CurrentTarget.RActorGuid);
                            }

                            return true;
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
                                    MinimumRange = CurrentTarget.RequiredRange,
                                    TargetPosition = CurrentTarget.Position,
                                });

                                // Count how many times we've tried interacting
                                if (!CacheData.InteractAttempts.TryGetValue(CurrentTarget.RActorGuid, out attemptCount))
                                {
                                    CacheData.InteractAttempts.Add(CurrentTarget.RActorGuid, 1);
                                }
                                else
                                {
                                    CacheData.InteractAttempts[CurrentTarget.RActorGuid]++;
                                }

                                // If we've tried interacting too many times, blacklist this for a while
                                if (CacheData.InteractAttempts[CurrentTarget.RActorGuid] > 15 && CurrentTarget.Type != GObjectType.HealthWell)
                                {
                                    Logger.Log(TrinityLogLevel.Verbose, LogCategory.Targetting, "Blacklisting {0} ({1}) for 15 seconds after {2} interactions",
                                        CurrentTarget.InternalName, CurrentTarget.ActorSNO, attemptCount);
                                    Blacklist15Seconds.Add(CurrentTarget.RActorGuid);
                                }

                                return true;
                            }
                            return false;
                        }
                    // * Destructible - need to pick an ability and attack it
                    case GObjectType.Destructible:
                    case GObjectType.Barricade:
                        {
                            if (CombatBase.CurrentPower.SNOPower != SNOPower.None)
                            {
                                // obsolet ?
                                if (CurrentTarget.Type == GObjectType.Barricade)
                                {
                                    Logger.Log(TrinityLogLevel.Debug, LogCategory.Targetting,
                                        "Barricade: Name={0}. SNO={1}, Range={2}. Needed range={3}. Radius={4}. Type={5}. Using power={6}",
                                        CurrentTarget.InternalName,     // 0
                                        CurrentTarget.ActorSNO,         // 1
                                        CurrentTarget.Distance,         // 2
                                        CurrentTarget.RequiredRange,            // 3
                                        CurrentTarget.Radius,           // 4
                                        CurrentTarget.Type,             // 5
                                        CombatBase.CurrentPower.SNOPower// 6 
                                        );
                                }
                                else
                                {
                                    Logger.Log(TrinityLogLevel.Debug, LogCategory.Targetting,
                                        "Destructible: Name={0}. SNO={1}, Range={2}. Needed range={3}. Radius={4}. Type={5}. Using power={6}",
                                        CurrentTarget.InternalName,       // 0
                                        CurrentTarget.ActorSNO,           // 1
                                        TargetCurrentDistance,            // 2
                                        CurrentTarget.RequiredRange,              // 3 
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
                                    CombatBase.CurrentPower.TargetPosition = vAttackPoint;

                                    if (CombatBase.Cast(CombatBase.CurrentPower))
                                    {
                                        Logger.Log(TrinityLogLevel.Debug, LogCategory.Behavior, "(NB: Attacking location of destructable)");
                                    }
                                }
                                else if (CombatBase.Cast(CombatBase.CurrentPower))
                                {
                                    Logger.Log(TrinityLogLevel.Debug, LogCategory.Behavior, "(NB: Standard attack on ACDGuid)");
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
                                if (CacheData.InteractAttempts[CurrentTarget.RActorGuid] > 3)
                                {
                                    _ignoreRactorGuid = CurrentTarget.RActorGuid;
                                    _ignoreTargetForLoops = 3;
                                    Logger.Log(TrinityLogLevel.Verbose, LogCategory.Targetting, "Blacklisting {0} {1} {2} for 3 seconds for Destrucable attack", CurrentTarget.Type, CurrentTarget.InternalName, CurrentTarget.ActorSNO);
                                    Blacklist3Seconds.Add(CurrentTarget.RActorGuid);
                                    _lastDestroyedDestructible = DateTime.UtcNow;
                                    _needClearDestructibles = true;
                                }


                                return true;
                            }
                            // Now tell Trinity to get a new target!
                            _forceTargetUpdate = true;

                            return false;
                        }
                    default:
                        {
                            _forceTargetUpdate = true;
                            Logger.LogError("Default handle target in range encountered for {0} Type: {1}", CurrentTarget.InternalName, CurrentTarget.Type);

                            return false;
                        }
                }
            }

            return false;
        }

        private static bool HandleTargetDistanceCheck()
        {
            using (new PerformanceLogger("HandleTarget.DistanceEqualCheck"))
            {
                // Count how long we have failed to move - body block stuff etc.
                if (Math.Abs(TargetCurrentDistance - LastDistanceFromTarget) < 5f && PlayerMover.GetMovementSpeed() < 1)
                {
                    ForceNewMovement = true;
                    if (DateTime.UtcNow.Subtract(_lastMovedDuringCombat).TotalMilliseconds >= 250)
                    {
                        _lastMovedDuringCombat = DateTime.UtcNow;
                        // We've been stuck at least 250 ms, let's go and pick new targets etc.
                        _timesBlockedMoving++;
                        _forceCloseRangeTarget = true;
                        _lastForcedKeepCloseRange = DateTime.UtcNow;
                        // And tell Trinity to get a new target
                        _forceTargetUpdate = true;

                        // Reset the emergency loop counter and return success
                        return true;
                    }
                }
                else
                {
                    // Movement has been made, so count the time last moved!
                    _lastMovedDuringCombat = DateTime.UtcNow;
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
                if (CurrentTarget.Type == GObjectType.Avoidance)



                    // don't timeout on legendary items
                    if (CurrentTarget.Type == GObjectType.Item && CurrentTarget.ItemQuality >= ItemQuality.Legendary)
                    {
                        return false;
                    }

                // don't timeout if we're actively moving
                if (PlayerMover.GetMovementSpeed() > 1)
                {
                    return false;
                }

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

                    bool isNavigable = NavHelper.CanRayCast(CurrentMoveDestination);

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
                        if (CurrentTarget.IsBoss)
                        {
                            Logger.Log(TrinityLogLevel.Info, LogCategory.Behavior, "Blacklisted Boss, Returning Failure");

                            Blacklist15Seconds.Add(CurrentTarget.RActorGuid);
                            CurrentTarget = null;
                            return true;
                        }
                        if (CurrentTarget.Type == GObjectType.Item && CurrentTarget.ItemQuality >= ItemQuality.Legendary)
                        {
                            return false;
                        }

                        if (CurrentTarget.IsUnit)
                        {
                            Logger.Log(TrinityLogLevel.Error, LogCategory.UserInformation,
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
                            Logger.Log(TrinityLogLevel.Error, LogCategory.UserInformation,
                                "Blacklisting an object because of possible stuck issues. Object={0} [{1}]. Range={2:0} RActorGUID={3}",
                                CurrentTarget.InternalName,         // 0
                                CurrentTarget.ActorSNO,             // 1 
                                CurrentTarget.Distance,       // 2
                                CurrentTarget.RActorGuid            // 3
                                );
                        }

                        Logger.Log(TrinityLogLevel.Error, LogCategory.UserInformation, "Blacklisted Target, Returning Failure");

                        Blacklist60Seconds.Add(CurrentTarget.RActorGuid);
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
        private static void HandleAssignAbilityTask()
        {
            using (new PerformanceLogger("HandleTarget.AssignMonsterTargetPower"))
            {
                if (CurrentTarget == null)
                    return;

                // Just in case
                if (CombatBase.IsNull(CombatBase.CurrentPower))
                    _shouldPickNewAbilities = true;

                // force to execute special combat routine like wd soulharverster
                if (DateTime.UtcNow.Subtract(lastPickNewAbilitiesForced).TotalMilliseconds >= 100)
                {
                    _shouldPickNewAbilities = true;
                    lastPickNewAbilitiesForced = DateTime.UtcNow;
                }

                // Find a valid ability if the target is a monster
                if (_shouldPickNewAbilities && !_isWaitingForPower && !_isWaitingForPotion)
                {
                    _shouldPickNewAbilities = false;
                    if (CurrentTarget.IsUnit)
                    {
                        // Pick a suitable ability
                        CombatBase.CurrentPower = AbilitySelector();

                        if (Player.IsInCombat && CombatBase.CurrentPower.SNOPower == SNOPower.None && !Player.IsIncapacitated)
                        {
                            NoAbilitiesAvailableInARow++;
                            if (DateTime.UtcNow.Subtract(lastRemindedAboutAbilities).TotalSeconds > 60 && NoAbilitiesAvailableInARow >= 4)
                            {
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
                        CombatBase.CurrentPower = AbilitySelector();

                    // Return since we should have assigned a power
                    return;
                }
                if (!_isWaitingForPower && CombatBase.CurrentPower == null)
                {
                    CombatBase.CurrentPower = AbilitySelector();
                }
            }
        }
        /// <summary>
        /// Will check <see cref=" _isWaitingForPotion"/> and Use a Potion if needed
        /// </summary>
        private static bool UsePotionIfNeededTask()
        {
            using (new PerformanceLogger("HandleTarget.UseHealthPotionIfNeeded"))
            {
                if (!Player.IsIncapacitated && Player.CurrentHealthPct > 0 && SpellHistory.TimeSinceUse(SNOPower.DrinkHealthPotion) > TimeSpan.FromSeconds(30) &&
                    Player.CurrentHealthPct <= CombatBase.EmergencyHealthPotionLimit)
                {
                    var legendaryPotions = CacheData.Inventory.Backpack.Where(i => i.InternalName.ToLower()
                        .Contains("healthpotion_legendary_")).ToList();


                    int dynamicId;
                    if (legendaryPotions.Any())
                    {
                        Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "Using Legendary Potion", 0);
                        dynamicId = legendaryPotions.FirstOrDefault().DynamicId;
                        ZetaDia.Me.Inventory.UseItem(dynamicId);
                        SpellHistory.RecordSpell(new TrinityPower(SNOPower.DrinkHealthPotion));
                        return true;
                    }

                    ACDItem potion = ZetaDia.Me.Inventory.BaseHealthPotion;
                    if (potion != null)
                    {
                        int id = potion.DynamicId;
                        Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "Using Regular Potion", 0);
                        ZetaDia.Me.Inventory.UseItem(id);

                        SpellHistory.RecordSpell(new TrinityPower(SNOPower.DrinkHealthPotion));
                        return true;
                    }

                    Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "No Available potions!", 0);
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
            // Whirlwind against everything within range
            if (Hotbar.Contains(SNOPower.Barbarian_Whirlwind) && Player.PrimaryResource >= 10 && TargetUtil.AnyMobsInRange(20) &&
                !IsWaitingForSpecial && CombatBase.CurrentPower.SNOPower != SNOPower.Barbarian_WrathOfTheBerserker && TargetCurrentDistance <= 12f
                && CurrentTarget.Type != GObjectType.Container &&
                (!Hotbar.Contains(SNOPower.Barbarian_Sprint) || GetHasBuff(SNOPower.Barbarian_Sprint)) &&
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
                    CombatBase.Cast(new TrinityPower(SNOPower.Barbarian_Whirlwind, 0f, CurrentMoveDestination));
                }
                // Store the current destination for comparison incase of changes next loop
                LastMoveToTarget = CurrentMoveDestination;
                // Reset total body-block count
                if ((!_forceCloseRangeTarget || DateTime.UtcNow.Subtract(_lastForcedKeepCloseRange).TotalMilliseconds > ForceCloseRangeForMilliseconds) &&
                    DateTime.UtcNow.Subtract(_lastForcedKeepCloseRange).TotalMilliseconds >= 2000)
                    _timesBlockedMoving = 0;

                return true;
            }

            bool Monk_SpecialMovement = ((CurrentTarget.Type == GObjectType.Gold ||
                CurrentTarget.IsUnit ||
                CurrentTarget.Type == GObjectType.Barricade ||
                CurrentTarget.Type == GObjectType.Destructible) && MonkCombat.IsTempestRushReady());

            // If we're doing avoidance, globes or backtracking, try to use special abilities to move quicker
            if ((CurrentTarget.Type == GObjectType.Avoidance ||
                CurrentTarget.Type == GObjectType.HealthGlobe ||
                CurrentTarget.Type == GObjectType.PowerGlobe ||
                CurrentTarget.Type == GObjectType.ProgressionGlobe ||
                Monk_SpecialMovement)
                && NavHelper.CanRayCast(Player.Position, CurrentMoveDestination)
                )
            {
                bool attackableSpecialMovement = ((CurrentTarget.Type == GObjectType.Avoidance &&
                ObjectCache.Any(u => (u.IsUnit || u.Type == GObjectType.Destructible || u.Type == GObjectType.Barricade) &&
                    MathUtil.IntersectsPath(u.Position, u.Radius, Player.Position, CurrentTarget.Position))));

                // Leap movement for a barb
                if (CombatBase.CanCast(SNOPower.Barbarian_Leap))
                {
                    return CombatBase.Cast(new TrinityPower(SNOPower.Barbarian_Leap, 0f, CurrentMoveDestination));
                }

                // Furious Charge movement for a barb
                if (CombatBase.CanCast(SNOPower.Barbarian_FuriousCharge) && Settings.Combat.Barbarian.UseChargeOOC)
                {
                    return CombatBase.Cast(new TrinityPower(SNOPower.Barbarian_FuriousCharge, 0f, CurrentMoveDestination));
                }

                // Teleport for a wizard (need to be able to check skill rune in DB for a 3-4 teleport spam in a row)
                if (CombatBase.CanCast(SNOPower.Wizard_Teleport))
                {
                    return CombatBase.Cast(new TrinityPower(SNOPower.Wizard_Teleport, 0f, CurrentMoveDestination));
                }

                // Archon Teleport for a wizard (need to be able to check skill rune in DB for a 3-4 teleport spam in a row)
                if (CombatBase.CanCast(SNOPower.Wizard_Archon_Teleport))
                {
                    return CombatBase.Cast(new TrinityPower(SNOPower.Wizard_Archon_Teleport, 0f, CurrentMoveDestination));
                }
                // Whirlwind for a barb

                if (attackableSpecialMovement && !IsWaitingForSpecial && CombatBase.CurrentPower.SNOPower != SNOPower.Barbarian_WrathOfTheBerserker
                    && Hotbar.Contains(SNOPower.Barbarian_Whirlwind) && Player.PrimaryResource >= 10)
                {
                    if (CombatBase.Cast(new TrinityPower(SNOPower.Barbarian_Whirlwind, 0f, CurrentMoveDestination)))
                    {
                        LastMoveToTarget = CurrentMoveDestination;
                        // Reset total body-block count, since we should have moved
                        if (DateTime.UtcNow.Subtract(_lastForcedKeepCloseRange).TotalMilliseconds >= 2000)
                            _timesBlockedMoving = 0;

                        return true;
                    }
                }

                // Tempest rush for a monk
                if (CombatBase.CanCast(SNOPower.Monk_TempestRush) && Player.PrimaryResource >= Settings.Combat.Monk.TR_MinSpirit &&
                    ((CurrentTarget.Type == GObjectType.Item && CurrentTarget.Distance > 20f) || CurrentTarget.Type != GObjectType.Item) &&
                    Settings.Combat.Monk.TROption != TempestRushOption.MovementOnly &&
                    MonkCombat.IsTempestRushReady())
                {
                    if (CombatBase.Cast(new TrinityPower(SNOPower.Monk_TempestRush, 0f, CurrentMoveDestination)))
                    {
                        LastMoveToTarget = CurrentMoveDestination;
                        // Reset total body-block count, since we should have moved
                        if (DateTime.UtcNow.Subtract(_lastForcedKeepCloseRange).TotalMilliseconds >= 2000)
                            _timesBlockedMoving = 0;

                        return true;
                    }
                }

                // Strafe for a Demon Hunter
                if (attackableSpecialMovement && CombatBase.CanCast(SNOPower.DemonHunter_Strafe))
                {
                    if (CombatBase.Cast(new TrinityPower(SNOPower.DemonHunter_Strafe, 0f, CurrentMoveDestination)))
                    {
                        LastMoveToTarget = CurrentMoveDestination;
                        // Reset total body-block count, since we should have moved
                        if (DateTime.UtcNow.Subtract(_lastForcedKeepCloseRange).TotalMilliseconds >= 2000)
                            _timesBlockedMoving = 0;

                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Updates bot status text with appropriate information if we are moving into range of our <see cref="CurrentTarget"/>
        /// </summary>
        private static string lastStatusText = "";
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
                    case GObjectType.ProgressionGlobe:
                        action = "Pickup ";
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
            statusText.Append(CurrentTarget.RequiredRange.ToString("0.0"));
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
        private static void SetQueuedSpecialMovement()
        {
            if (CurrentTarget == null)
                return;

            if (CurrentTarget.Type == GObjectType.Player ||
                CurrentTarget.Type == GObjectType.OocAvoidance)
            {
                Navigator.PlayerMover.MoveStop();
                return;
            }

            try
            {
                // Moving to kite point
                if (CurrentTarget.IsKite)
                {
                    PlayerMover.SetKiteMovement(CurrentTarget.InternalName);
                    CombatBase.QueuedMovement.CurrentMovement.StopCondition += m =>
                        CurrentTarget == null || !CurrentTarget.IsKite;
                }

                // Moving to Avoidance point
                if (CurrentTarget.IsAvoidance)
                {
                    PlayerMover.SetKiteMovement(CurrentTarget.InternalName);
                    CombatBase.QueuedMovement.CurrentMovement.StopCondition += m =>
                        CurrentTarget == null || !CurrentTarget.IsAvoidance;
                }

                // Keep kiting option
                if (Settings.Combat.Misc.KeepMovingInCombat && Player.IsRanged && CurrentTarget.IsUnit &&
                    CurrentTargetIsInRange && !CombatBase.IsNull(CombatBase.CurrentPower) &&
                    MainGrid.Map.Any() &&
                    (Player.IsRanged || TargetUtil.ClusterExists(Settings.Combat.Misc.TrashPackClusterRadius, TargetCurrentDistance + 5f, Settings.Combat.Misc.TrashPackSize)))
                {
                    CombatBase.QueuedMovement.Queue(new QueuedMovement
                    {
                        Id = CurrentTarget.RActorGuid,
                        Name = CurrentTarget.InternalName,
                        Infos = "(Keep moving) " + CurrentTarget.Infos,
                        Destination = GridMap.GetBestMoveNode().Position,
                        OnUpdate = m =>
                        {
                            var _safeNode = GridMap.GetBestMoveNode();
                            if (_safeNode != null)
                                m.Destination = _safeNode.Position;
                        },
                        StopCondition = m =>
                            m.Destination == Vector3.Zero ||
                            m.Destination.Distance2D(Trinity.Player.Position) <= 1f ||
                            CurrentTarget == null ||
                            CombatBase.IsNull(CombatBase.CurrentPower) || CurrentTarget.IsAvoidance ||
                            !CurrentTarget.IsUnit || (CurrentTarget.IsUnit && !CurrentTargetIsInRange)
                        ,
                        Options = new QueuedMovementOptions
                        {
                            Logging = LogLevel.Info,
                            AcceptableDistance = 1f,
                            Type = MoveType.KeepMoving,
                        }
                    });
                }

                if (CurrentTarget.IsUnit && !CombatBase.IsNull(CombatBase.CurrentPower) && CombatBase.CurrentPower.SNOPower == SNOPower.Walk && CombatBase.CurrentPower.TargetPosition != Vector3.Zero)
                {
                    CombatBase.QueuedMovement.Queue(new QueuedMovement
                    {
                        Id = CurrentTarget.RActorGuid,
                        Name = CurrentTarget.InternalName,
                        Infos = "(Walk) " + CurrentTarget.Infos,
                        Destination = CombatBase.CurrentPower.TargetPosition,
                        StopCondition = m =>
                            m.Destination == Vector3.Zero ||
                            m.Destination.Distance2D(Trinity.Player.Position) <= 1f ||
                            CurrentTarget == null ||
                            CombatBase.IsNull(CombatBase.CurrentPower) || CurrentTarget == null || CurrentTarget.IsAvoidance ||
                            !CurrentTarget.IsUnit || CombatBase.CurrentPower.SNOPower != SNOPower.Walk
                        ,
                        Options = new QueuedMovementOptions
                        {
                            Logging = LogLevel.Info,
                            AcceptableDistance = 3f,
                            Type = MoveType.BasicCombat,
                        }
                    });
                }

                if (CombatBase.QueuedMovement.IsQueuedMovement)
                {
                    lastSentMovePower = DateTime.UtcNow;
                    LastMoveToTarget = CombatBase.QueuedMovement.CurrentMovement.Destination;
                }
            }
            catch {/* not a big deal */}
        }

        /// <summary>
        /// Moves our player if no special ability is available
        /// </summary>
        /// <param name="bForceNewMovement"></param>
        private static void SetQueuedBasicMovement(bool bForceNewMovement)
        {
            using (new PerformanceLogger("HandleTarget.HandleBasicMovement"))
            {
                // Now for the actual movement request stuff
                IsAlreadyMoving = true;
                lastMovementCommand = DateTime.UtcNow;

                if (CurrentTarget.Type == GObjectType.Player ||
                    CurrentTarget.Type == GObjectType.OocAvoidance)
                {
                    Navigator.PlayerMover.MoveStop();
                    return;
                }

                if (DateTime.UtcNow.Subtract(lastSentMovePower).TotalMilliseconds >= 250 || Vector3.Distance(LastMoveToTarget, CurrentMoveDestination) >= 2f || bForceNewMovement)
                {
                    if (!CurrentTarget.IsAvoidance && !(CurrentTarget.IsUnit && CurrentTargetIsInRange))
                    {
                        int rActorGuid = CurrentTarget.RActorGuid;
                        CombatBase.QueuedMovement.Queue(new QueuedMovement
                        {
                            Id = CurrentTarget.RActorGuid,
                            Name = CurrentTarget.InternalName,
                            Infos = CurrentMoveDestination != CurrentTarget.Position ? "(Required destination) " + CurrentTarget.Infos : CurrentTarget.Infos,
                            Destination = CurrentMoveDestination,
                            OnUpdate = m =>
                            {
                                if (CurrentTarget != null && CurrentTarget.Type == GObjectType.Player || CurrentTarget.Type == GObjectType.OocAvoidance)
                                {
                                    Navigator.PlayerMover.MoveStop();
                                }
                                else if (CurrentTarget != null && !CurrentTarget.IsAvoidance && !CurrentTargetIsInRange)
                                {
                                    SetTargetFields();
                                    m.Name = CurrentTarget.InternalName;
                                    m.Infos = CurrentMoveDestination != CurrentTarget.Position ? "(Required destination) " + CurrentTarget.Infos : CurrentTarget.Infos;
                                    m.Destination = CurrentMoveDestination;
                                    m.Options.Type = CurrentTarget.IsUnit ? MoveType.BasicCombat : MoveType.TargetAttempt;
                                }
                            },
                            StopCondition = m =>
                                CurrentTarget == null || CurrentTarget.IsAvoidance || CurrentTarget.Type == GObjectType.Player || CurrentTarget.Type == GObjectType.OocAvoidance || CurrentTargetIsInRange
                            ,
                            Options = new QueuedMovementOptions
                            {
                                Logging = LogLevel.Info,
                                AcceptableDistance = 2f,
                                Type = CurrentTarget.IsUnit ? MoveType.BasicCombat : MoveType.TargetAttempt
                            }
                        });
                    }

                    if (CombatBase.QueuedMovement.IsQueuedMovement)
                    {
                        lastSentMovePower = DateTime.UtcNow;
                        LastMoveToTarget = CurrentMoveDestination;
                    }
                }
            }
        }

        private static void SetTargetFields()
        {
            using (new PerformanceLogger("HandleTarget.SetRequiredRange"))
            {
                if (CurrentTarget == null)
                    return;

                CurrentTarget.RequiredRange = 2f;

                TargetCurrentDistance = CurrentTarget.RadiusDistance;
                CurrentTargetIsInLoS = CurrentTarget.IsInLineOfSight;

                TargetCurrentDestination = CurrentTarget.Position;
                CurrentMoveDestination = CurrentTarget.Position;

                #region switch (CurrentTarget.Type)
                switch (CurrentTarget.Type)
                {
                    // * Unit, we need to pick an ability to use and get within range
                    case GObjectType.Unit:
                        {
                            CurrentTarget.RequiredRange = CombatBase.CurrentPower.MinimumRange;

                            if (CombatBase.CurrentPower.TargetACDGUID != -1)
                            {
                                var target = ObjectCache.Where(u => u.ACDGuid == CombatBase.CurrentPower.TargetACDGUID).FirstOrDefault();
                                if (target != null)
                                {
                                    // This move position is for an aoe movement
                                    // the required range is not checked with this position but with the Acd position 
                                    if (CombatBase.CurrentPower.MovePosition != Vector3.Zero)
                                    {
                                        TargetCurrentDestination = CombatBase.CurrentPower.MovePosition;
                                    }
                                    else
                                    {
                                        TargetCurrentDestination = target.Position;
                                    }

                                    // Distance to ACD
                                    TargetCurrentDistance = target.Position.Distance2D(Player.Position) - target.Radius;
                                    CombatBase.CurrentPower.TargetPosition = target.Position;
                                }
                            }
                            else if (CombatBase.CurrentPower.TargetPosition != Vector3.Zero)
                            {
                                // This move position is for a required position before cast at the target position
                                if (CombatBase.CurrentPower.MovePosition != Vector3.Zero)
                                {
                                    TargetCurrentDestination = CombatBase.CurrentPower.MovePosition;
                                }
                                else
                                {
                                    TargetCurrentDestination = CombatBase.CurrentPower.TargetPosition;
                                }

                                // Distance to destination
                                TargetCurrentDistance = TargetCurrentDestination.Distance2D(Player.Position) - CurrentTarget.Radius;
                            }

                            break;
                        }
                    // * Item - need to get within 6 feet and then interact with it
                    case GObjectType.Item:
                        {
                            CurrentTarget.RequiredRange = 6f;
                            TargetCurrentDistance = CurrentTarget.Distance;
                            break;
                        }
                    // * Gold - need to get within pickup radius only
                    case GObjectType.Gold:
                        {
                            CurrentTarget.RequiredRange = Math.Max(Player.GoldPickupRadius - 5f, 5f);
                            TargetCurrentDistance = CurrentTarget.Distance;
                            CurrentMoveDestination = MathEx.CalculatePointFrom(Player.Position, CurrentTarget.Position, -2f);
                            break;
                        }
                    // * Globes - need to get within pickup radius only
                    case GObjectType.PowerGlobe:
                    case GObjectType.HealthGlobe:
                    case GObjectType.ProgressionGlobe:
                        {
                            CurrentTarget.RequiredRange = Math.Max(Player.GoldPickupRadius - 5f, 5f);
                            TargetCurrentDistance = CurrentTarget.Distance;
                            break;
                        }
                    // * Shrine & Container - need to get within 8 feet and interact
                    case GObjectType.HealthWell:
                        {
                            CurrentTarget.RequiredRange = 4f;

                            float range;
                            if (DataDictionary.CustomObjectRadius.TryGetValue(CurrentTarget.ActorSNO, out range))
                            {
                                CurrentTarget.RequiredRange = range;
                            }
                            break;
                        }
                    case GObjectType.Shrine:
                    case GObjectType.Container:
                        {
                            CurrentTarget.RequiredRange = 6f;

                            float range;
                            if (DataDictionary.CustomObjectRadius.TryGetValue(CurrentTarget.ActorSNO, out range))
                            {
                                CurrentTarget.RequiredRange = range;
                            }
                            if (CurrentTarget.RequiredRange <= 1)
                                CurrentTarget.RequiredRange = 2f;
                            break;
                        }
                    case GObjectType.Interactable:
                        {
                            if (CurrentTarget.IsQuestGiver)
                            {
                                CurrentMoveDestination = MathEx.CalculatePointFrom(CurrentTarget.Position, Player.Position, CurrentTarget.Radius + 2f);
                                CurrentTarget.RequiredRange = 5f;
                            }
                            else
                            {
                                CurrentTarget.RequiredRange = 5f;
                            }
                            // Check if it's in our interactable range dictionary or not
                            float range;

                            if (DataDictionary.CustomObjectRadius.TryGetValue(CurrentTarget.ActorSNO, out range))
                            {
                                CurrentTarget.RequiredRange = range;
                            }
                            if (CurrentTarget.RequiredRange <= 0)
                                CurrentTarget.RequiredRange = CurrentTarget.Radius;

                            break;
                        }
                    // * Destructible - need to pick an ability and attack it
                    case GObjectType.Destructible:
                        {
                            CurrentTarget.RequiredRange = CombatBase.CurrentPower.MinimumRange;
                            CurrentTarget.Radius = 1f;
                            TargetCurrentDistance = CurrentTarget.Distance;
                            break;
                        }
                    case GObjectType.Barricade:
                        {
                            // Pick a range to try to reach + (tmp_fThisRadius * 0.70);
                            CurrentTarget.RequiredRange = CombatBase.CurrentPower.MinimumRange;
                            CurrentTarget.Radius = 1f;
                            TargetCurrentDistance = CurrentTarget.Distance;
                            break;
                        }
                    // * Avoidance - need to pick an avoid location and move there
                    case GObjectType.Avoidance:
                        {
                            CurrentTarget.RequiredRange = 2f;
                            break;
                        }
                    case GObjectType.Door:
                        CurrentTarget.RequiredRange = 2f;
                        break;
                    default:
                        CurrentTarget.RequiredRange = CurrentTarget.Radius;
                        break;
                }
                #endregion

                LastDistanceFromTarget = TargetCurrentDistance;
                CurrentTargetIsInRange = CurrentTarget.RequiredRange <= 1f || (TargetCurrentDistance <= CurrentTarget.RequiredRange && CurrentTargetIsInLoS);
            }
        }

        private static bool HandleUnitInRange()
        {
            using (new PerformanceLogger("HandleTarget.HandleUnitInRange"))
            {
                bool usePowerResult = CombatBase.Cast(CombatBase.CurrentPower);
                if (usePowerResult)
                    _shouldPickNewAbilities = true;

                // Keep looking for monsters at "normal kill range" a few moments after we successfully attack a monster incase we can pull them into range
                _keepKillRadiusExtendedForSeconds = 8;
                _timeKeepKillRadiusExtendedUntil = DateTime.UtcNow.AddSeconds(_keepKillRadiusExtendedForSeconds);
                _keepLootRadiusExtendedForSeconds = 8;
                // if at full or nearly full health, see if we can raycast to it, if not, ignore it for 2000 ms
                /*if (CurrentTarget.HitPointsPct >= 0.9d &&
                    !NavHelper.CanRayCast(Player.Position, CurrentTarget.Position) &&
                    !CurrentTarget.IsBoss &&
                    !(DataDictionary.StraightLinePathingLevelAreaIds.Contains(Player.LevelAreaId) || DataDictionary.LineOfSightWhitelist.Contains(CurrentTarget.ActorSNO)))
                {
                    _ignoreRactorGuid = CurrentTarget.RActorGuid;
                    _ignoreTargetForLoops = 6;
                    // Add this monster to our very short-term ignore list
                    //Blacklist3Seconds.Add(CurrentTarget.RActorGuid);
                    //Logger.Log(TrinityLogLevel.Error, LogCategory.UserInformation, "Blacklisting {0} {1} {2} for 3 seconds due to Raycast failure", CurrentTarget.Type, CurrentTarget.InternalName, CurrentTarget.ActorSNO);
                    Blacklist3LastClear = DateTime.UtcNow;
                    NeedToClearBlacklist3 = true;
                }*/

                return usePowerResult;
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
                if (!ItemDropStats._hashsetItemPicksLookedAt.Contains(itemSha1Hash))
                {
                    ItemDropStats._hashsetItemPicksLookedAt.Add(itemSha1Hash);
                    GItemType itemType = TrinityItemManager.DetermineItemType(CurrentTarget.InternalName, CurrentTarget.DBItemType, CurrentTarget.FollowerType);
                    GItemBaseType itemBaseType = TrinityItemManager.DetermineBaseType(itemType);
                    if (itemBaseType == GItemBaseType.Armor || itemBaseType == GItemBaseType.WeaponOneHand || itemBaseType == GItemBaseType.WeaponTwoHand ||
                        itemBaseType == GItemBaseType.WeaponRange || itemBaseType == GItemBaseType.Jewelry || itemBaseType == GItemBaseType.FollowerItem ||
                        itemBaseType == GItemBaseType.Offhand)
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
                            Logger.Log(TrinityLogLevel.Error, LogCategory.UserInformation, "ERROR: Item type (" + iQuality + ") out of range");
                        }
                        if ((CurrentTarget.ItemLevel < 0) || (CurrentTarget.ItemLevel >= 74))
                        {
                            Logger.Log(TrinityLogLevel.Error, LogCategory.UserInformation, "ERROR: Item level (" + CurrentTarget.ItemLevel + ") out of range");
                        }
                        ItemDropStats.ItemsPickedStats.TotalPerQuality[iQuality]++;
                        ItemDropStats.ItemsPickedStats.TotalPerLevel[CurrentTarget.ItemLevel]++;
                        ItemDropStats.ItemsPickedStats.TotalPerQPerL[iQuality, CurrentTarget.ItemLevel]++;
                    }
                    else if (itemBaseType == GItemBaseType.Gem)
                    {
                        int iGemType = 0;
                        ItemDropStats.ItemsPickedStats.TotalGems++;
                        if (itemType == GItemType.Topaz)
                            iGemType = ItemDropStats.GEMTOPAZ;
                        if (itemType == GItemType.Ruby)
                            iGemType = ItemDropStats.GEMRUBY;
                        if (itemType == GItemType.Emerald)
                            iGemType = ItemDropStats.GEMEMERALD;
                        if (itemType == GItemType.Amethyst)
                            iGemType = ItemDropStats.GEMAMETHYST;
                        if (itemType == GItemType.Diamond)
                            iGemType = ItemDropStats.GEMDIAMOND;

                        ItemDropStats.ItemsPickedStats.GemsPerType[iGemType]++;
                        ItemDropStats.ItemsPickedStats.GemsPerLevel[CurrentTarget.ItemLevel]++;
                        ItemDropStats.ItemsPickedStats.GemsPerTPerL[iGemType, CurrentTarget.ItemLevel]++;
                    }
                    else if (itemType == GItemType.HealthPotion)
                    {
                        ItemDropStats.ItemsPickedStats.TotalPotions++;
                        if ((CurrentTarget.ItemLevel < 0) || (CurrentTarget.ItemLevel > 63))
                        {
                            Logger.Log(TrinityLogLevel.Error, LogCategory.UserInformation, "ERROR: Potion level ({0}) out of range", CurrentTarget.ItemLevel);
                        }
                        ItemDropStats.ItemsPickedStats.PotionsPerLevel[CurrentTarget.ItemLevel]++;
                    }
                    else if (c_item_GItemType == GItemType.InfernalKey)
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
                if (iInteractAttempts > 35 && CurrentTarget.ItemQuality < ItemQuality.Legendary)
                {
                    Blacklist60Seconds.Add(CurrentTarget.RActorGuid);
                }
                // Now tell Trinity to get a new target!
                _forceTargetUpdate = true;
                return iInteractAttempts;
            }
        }

        private static bool CurrentTargetIsNotAvoidance()
        {
            return !CurrentTarget.IsAvoidance;
        }

        private static bool CurrentTargetIsNonUnit()
        {
            return !CurrentTarget.IsUnit;
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
    }
}
