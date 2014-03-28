using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Trinity.Combat.Abilities;
using Trinity.Config.Combat;
using Trinity.Technicals;
using Zeta.Bot;
using Zeta.Common;
using Zeta.Common.Plugins;
using Zeta.Game;
using Zeta.Game.Internals.Actors;
using Zeta.Game.Internals.SNO;
using Logger = Trinity.Technicals.Logger;

namespace Trinity
{
    public partial class Trinity : IPlugin
    {
        /// <summary>
        /// For backwards compatability
        /// </summary>
        public static void RefreshDiaObjects()
        {
            // Framelock should happen in the MainLoop, where we read all the actual ACD's
            RefreshDiaObjectCache();
        }

        /// <summary>
        /// This method will add and update necessary information about all available actors. Determines ObjectType, sets ranges, updates blacklists, determines avoidance, kiting, target weighting
        /// and the result is we will have a new target for the Target Handler. Returns true if the cache was refreshed.
        /// </summary>
        /// <returns>True if the cache was updated</returns>
        public static bool RefreshDiaObjectCache(bool forceUpdate = false)
        {
            using (new PerformanceLogger("RefreshDiaObjectCache"))
            {
                if (DateTime.UtcNow.Subtract(LastRefreshedCache).TotalMilliseconds < Settings.Advanced.CacheRefreshRate && !forceUpdate)
                {
                    if (!UpdateCurrentTarget())
                        return false;
                }
                LastRefreshedCache = DateTime.UtcNow;

                using (new PerformanceLogger("RefreshDiaObjectCache.UpdateBlock"))
                {
                    GenericCache.MaintainCache();
                    GenericBlacklist.MaintainBlacklist();

                    // Update player-data cache, including buffs
                    PlayerInfoCache.UpdateCachedPlayerData();

                    if (Player.CurrentHealthPct <= 0)
                    {
                        return false;
                    }

                    RefreshCacheInit();

                    // Now pull up all the data and store anything we want to handle in the super special cache list
                    // Also use many cache dictionaries to minimize DB<->D3 memory hits, and speed everything up a lot
                    RefreshCacheMainLoop();

                }

                // Add Team HotSpots to the cache
                ObjectCache.AddRange(GroupHotSpots.GetCacheObjectHotSpots());

                    /* Fire Chains Experimental Avoidance */
                if (Settings.Combat.Misc.UseExperimentalFireChainsAvoidance)
                {
                    const float fireChainSize = 5f;
                    foreach (var unit1 in ObjectCache.Where(u => u.MonsterAffixes.HasFlag(MonsterAffixes.FireChains)))
                    {
                        foreach (var unit2 in ObjectCache.Where(u => u.MonsterAffixes.HasFlag(MonsterAffixes.FireChains)).Where(unit2 => unit1.RActorGuid != unit2.RActorGuid))
                        {
                            for (float i = 0; i <= unit1.Position.Distance2D(unit2.Position); i += (fireChainSize / 4))
                            {
                                Vector3 fireChainSpot = MathEx.CalculatePointFrom(unit1.Position, unit2.Position, i);

                                if (Trinity.Player.Position.Distance2D(fireChainSpot) <= fireChainSize)
                                {
                                    Logger.Log(TrinityLogLevel.Debug, LogCategory.CacheManagement, "Avoiding Fire Chains!");
                                    StandingInAvoidance = true;
                                }
                                CacheData.TimeBoundAvoidance.Add(new CacheObstacleObject(fireChainSpot, fireChainSize, -2, "FireChains"));
                            }
                        }
                        if (CacheData.TimeBoundAvoidance.Any(aoe => aoe.ActorSNO == -2))
                            Logger.Log(TrinityLogLevel.Debug, LogCategory.CacheManagement, "Generated {0} avoidance points for FireChains, minDistance={1} maxDistance={2}",
                                CacheData.TimeBoundAvoidance.Count(aoe => aoe.ActorSNO == -2),
                                CacheData.TimeBoundAvoidance.Where(aoe => aoe.ActorSNO == -2)
                                    .Min(aoe => aoe.Position.Distance2D(Trinity.Player.Position)),
                                CacheData.TimeBoundAvoidance.Where(aoe => aoe.ActorSNO == -2)
                                    .Max(aoe => aoe.Position.Distance2D(Trinity.Player.Position)));
                    }
                }

                /* Beast Charge Experimental Avoidance */
                if (Settings.Combat.Misc.UseExperimentalSavageBeastAvoidance)
                {
                    const float beastChargePathWidth = 10f;
                    const int beastChargerSNO = 3337;
                    foreach (var unit1 in ObjectCache.Where(u => u.IsFacingPlayer && u.Animation == SNOAnim.Beast_start_charge_02 ||
                                    u.Animation == SNOAnim.Beast_charge_02 || u.Animation == SNOAnim.Beast_charge_04))
                    {

                        Vector3 endPoint = MathEx.GetPointAt(unit1.Position, 90f, unit1.Unit.Movement.Rotation);

                        for (float i = 0; i <= unit1.Position.Distance2D(endPoint); i += (beastChargePathWidth / 4))
                        {
                            Vector3 pathSpot = MathEx.CalculatePointFrom(unit1.Position, endPoint, i);

                            Logger.Log(TrinityLogLevel.Debug, LogCategory.CacheManagement,
                                "Generating BeastCharge Avoidance: {0} dist: {1}",
                                pathSpot, pathSpot.Distance2D(unit1.Position));

                            if (Trinity.Player.Position.Distance2D(pathSpot) <= beastChargePathWidth)
                            {
                                Logger.Log(TrinityLogLevel.Debug, LogCategory.CacheManagement, "Avoiding Beast Charger!");
                                StandingInAvoidance = true;
                            }
                            CacheData.TimeBoundAvoidance.Add(new CacheObstacleObject(pathSpot, beastChargePathWidth, beastChargerSNO,
                                "BeastCharge"));
                        }
                        if (CacheData.TimeBoundAvoidance.Any(aoe => aoe.ActorSNO == beastChargerSNO))
                            Logger.Log(TrinityLogLevel.Debug, LogCategory.CacheManagement,
                                "Generated {0} avoidance points for BeastCharge, minDistance={1} maxDistance={2}",
                                CacheData.TimeBoundAvoidance.Count(aoe => aoe.ActorSNO == beastChargerSNO),
                                CacheData.TimeBoundAvoidance.Where(aoe => aoe.ActorSNO == beastChargerSNO)
                                    .Min(aoe => aoe.Position.Distance2D(Trinity.Player.Position)),
                                CacheData.TimeBoundAvoidance.Where(aoe => aoe.ActorSNO == beastChargerSNO)
                                    .Max(aoe => aoe.Position.Distance2D(Trinity.Player.Position)));
                    }
                }

                /* Poison Experimental Avoidance */




                // Reduce ignore-for-loops counter
                if (IgnoreTargetForLoops > 0)
                    IgnoreTargetForLoops--;
                // If we have an avoidance under our feet, then create a new object which contains a safety point to move to
                // But only if we aren't force-cancelling avoidance for XX time
                bool hasFoundSafePoint = false;

                using (new PerformanceLogger("RefreshDiaObjectCache.AvoidanceCheck"))
                {
                    // Note that if treasure goblin level is set to kamikaze, even avoidance moves are disabled to reach the goblin!
                    if (StandingInAvoidance && (!AnyTreasureGoblinsPresent || Settings.Combat.Misc.GoblinPriority <= GoblinPriority.Prioritize) &&
                        DateTime.UtcNow.Subtract(timeCancelledEmergencyMove).TotalMilliseconds >= cancelledEmergencyMoveForMilliseconds)
                    {
                        Vector3 vAnySafePoint = NavHelper.FindSafeZone(false, 1, Player.Position, true, null, true);
                        // Ignore avoidance stuff if we're incapacitated or didn't find a safe spot we could reach
                        if (vAnySafePoint != Vector3.Zero)
                        {
                            if (Settings.Advanced.LogCategories.HasFlag(LogCategory.Movement))
                            {
                                Logger.Log(TrinityLogLevel.Verbose, LogCategory.Movement, "Kiting Avoidance: {0} Distance: {1:0} Direction: {2:0}, Health%={3:0.00}, KiteDistance: {4:0}",
                                    vAnySafePoint, vAnySafePoint.Distance(Me.Position), MathUtil.GetHeading(MathUtil.FindDirectionDegree(Me.Position, vAnySafePoint)),
                                    Player.CurrentHealthPct, PlayerKiteDistance);
                            }

                            hasFoundSafePoint = true;
                            CurrentTarget = new TrinityCacheObject()
                                {
                                    Position = vAnySafePoint,
                                    Type = GObjectType.Avoidance,
                                    Weight = 20000,
                                    CentreDistance = Vector3.Distance(Player.Position, vAnySafePoint),
                                    RadiusDistance = Vector3.Distance(Player.Position, vAnySafePoint),
                                    InternalName = "SafePoint"
                                }; ;
                        }
                    }
                }
                /*
                 * Give weights to objects
                 */
                // Special flag for special whirlwind circumstances
                bAnyNonWWIgnoreMobsInRange = false;
                // Now give each object a weight *IF* we aren't skipping direcly to a safe-spot
                if (!hasFoundSafePoint)
                {
                    RefreshDiaGetWeights();
                    RefreshSetKiting(ref KiteAvoidDestination, NeedToKite, ref TryToKite);
                }
                // Not heading straight for a safe-spot?
                // No valid targets but we were told to stay put?
                if (CurrentTarget == null && ShouldStayPutDuringAvoidance && !StandingInAvoidance)
                {
                    CurrentTarget = new TrinityCacheObject()
                                        {
                                            Position = Player.Position,
                                            Type = GObjectType.Avoidance,
                                            Weight = 20000,
                                            CentreDistance = 2f,
                                            RadiusDistance = 2f,
                                            InternalName = "StayPutPoint"
                                        };
                    Logger.Log(TrinityLogLevel.Debug, LogCategory.CacheManagement, "Staying Put During Avoidance");
                }

                // Pre-townrun is too far away
                if (!Player.IsInTown && TownRun.PreTownRunPosition != Vector3.Zero && TownRun.PreTownRunWorldId == Player.WorldID && !ForceVendorRunASAP
                    && TownRun.PreTownRunPosition.Distance2D(Player.Position) <= V.F("Cache.PretownRun.MaxDistance"))
                {
                    Logger.Log(TrinityLogLevel.Debug, LogCategory.UserInformation, "Pre-TownRun position is more than {0} yards away, canceling", V.I("Cache.PretownRun.MaxDistance"));
                    TownRun.PreTownRunPosition = Vector3.Zero;
                    TownRun.PreTownRunWorldId = -1;
                }

                // Reached pre-townrun position
                if (!Player.IsInTown && TownRun.PreTownRunPosition != Vector3.Zero && TownRun.PreTownRunWorldId == Player.WorldID && !ForceVendorRunASAP
                    && TownRun.PreTownRunPosition.Distance2D(Player.Position) <= 15f)
                {
                    Logger.Log(TrinityLogLevel.Debug, LogCategory.UserInformation, "Successfully returned to Pre-TownRun Position");
                    TownRun.PreTownRunPosition = Vector3.Zero;
                    TownRun.PreTownRunWorldId = -1;
                }

                // After a townrun, make sure to return to original TownRun Location
                if (!Player.IsInTown && CurrentTarget == null && TownRun.PreTownRunPosition != Vector3.Zero && TownRun.PreTownRunWorldId == Player.WorldID && !ForceVendorRunASAP)
                {
                    if (TownRun.PreTownRunPosition.Distance2D(Player.Position) > 10f && TownRun.PreTownRunPosition.Distance2D(Player.Position) <= V.F("Cache.PretownRun.MaxDistance"))
                    {
                        CurrentTarget = new TrinityCacheObject()
                        {
                            Position = TownRun.PreTownRunPosition,
                            Type = GObjectType.Avoidance,
                            Weight = 20000,
                            CentreDistance = 2f,
                            RadiusDistance = 2f,
                            InternalName = "PreTownRunPosition"
                        };
                        Logger.Log(TrinityLogLevel.Debug, LogCategory.UserInformation, "Returning to Pre-TownRun Position");
                    }
                }

                using (new PerformanceLogger("RefreshDiaObjectCache.FinalChecks"))
                {
                    // force to stay put if we want to town run and there's no target
                    if (CurrentTarget == null && ForceVendorRunASAP)
                    {
                        bDontMoveMeIAmDoingShit = true;
                    }

                    // Still no target, let's see if we should backtrack or wait for wrath to come off cooldown...
                    if (CurrentTarget == null)
                    {
                        RefreshDoBackTrack();
                    }
                    // Still no target, let's end it all!
                    if (CurrentTarget == null)
                    {
                        return true;
                    }
                    // Ok record the time we last saw any unit at all
                    if (CurrentTarget.IsUnit)
                    {
                        lastHadUnitInSights = DateTime.UtcNow;
                        // And record when we last saw any form of elite
                        if (CurrentTarget.IsBoss || CurrentTarget.IsEliteRareUnique || CurrentTarget.IsTreasureGoblin)
                            lastHadEliteUnitInSights = DateTime.UtcNow;
                    }
                    if (CurrentTarget.Type == GObjectType.Container)
                    {
                        lastHadContainerInSights = DateTime.UtcNow;
                    }
                    // Record the last time our target changed
                    if (LastTargetRactorGUID != CurrentTarget.RActorGuid)
                    {
                        RecordTargetHistory();

                        Logger.Log(TrinityLogLevel.Verbose, LogCategory.Weight, 
                            "Found New Target {0} dist={1:0} IsElite={2} Radius={3:0.0} Weight={4:0} ActorSNO={5} " +
                            "Anim={6} Target++={7} Type={8} ",
                            CurrentTarget.InternalName, 
                            CurrentTarget.CentreDistance, 
                            CurrentTarget.IsEliteRareUnique, 
                            CurrentTarget.Radius,
                            CurrentTarget.Weight,
                            CurrentTarget.ActorSNO,
                            CurrentTarget.Animation,
                            CurrentTarget.TimesBeenPrimaryTarget,
                            CurrentTarget.Type
                            );

                        dateSincePickedTarget = DateTime.UtcNow;
                        iTargetLastHealth = 0f;
                    }
                    else
                    {
                        // We're sticking to the same target, so update the target's health cache to check for stucks
                        if (CurrentTarget.IsUnit)
                        {
                            // Check if the health has changed, if so update the target-pick time before we blacklist them again
                            if (CurrentTarget.HitPointsPct != iTargetLastHealth)
                            {
                                Logger.Log(TrinityLogLevel.Debug, LogCategory.Weight, "Keeping Target {0} - CurrentTarget.HitPoints: {1:0.00} TargetLastHealth: {2:0.00} ",
                                                CurrentTarget.RActorGuid, CurrentTarget.HitPointsPct, iTargetLastHealth);
                                dateSincePickedTarget = DateTime.UtcNow;
                            }
                            // Now store the target's last-known health
                            iTargetLastHealth = CurrentTarget.HitPointsPct;
                        }
                    }
                }
                // We have a target and the cached was refreshed
                return true;
            }
        }

        private static void RefreshCacheMainLoop()
        {
            using (new PerformanceLogger("CacheManagement.RefreshCacheMainLoop"))
            {
                IEnumerable<DiaObject> refreshSource;

                if (Settings.Advanced.LogCategories.HasFlag(LogCategory.CacheManagement))
                {
                    refreshSource = ReadDebugActorsFromMemory();
                }
                else
                {
                    refreshSource = ReadActorsFromMemory();
                }
                Stopwatch t1 = new Stopwatch();

                foreach (DiaObject currentObject in refreshSource)
                {
                    try
                    {
                        bool AddToCache = false;

                        if (!Settings.Advanced.LogCategories.HasFlag(LogCategory.CacheManagement))
                        {
                            /*
                             *  Main Cache Function
                             */
                            AddToCache = CacheDiaObject(currentObject);
                        }
                        else
                        {
                            // We're debugging, slightly slower, calculate performance metrics and dump debugging to log 
                            t1.Reset();
                            t1.Start();

                            /*
                             *  Main Cache Function
                             */
                            AddToCache = CacheDiaObject(currentObject);

                            if (t1.IsRunning)
                                t1.Stop();

                            double duration = t1.Elapsed.TotalMilliseconds;

                            // don't log stuff we never care about
                            if (duration <= 1 && c_IgnoreSubStep == "IgnoreNames")
                                continue;

                            if ((Settings.Advanced.LogCategories.HasFlag(LogCategory.Performance) && duration > 1 || !Settings.Advanced.LogCategories.HasFlag(LogCategory.Performance)))
                            {
                                string extraData = "";

                                switch (c_ObjectType)
                                {
                                    case GObjectType.Unit:
                                        {
                                            if (c_IsEliteRareUnique)
                                                extraData += " IsElite " + c_MonsterAffixes.ToString();

                                            if (c_unit_HasShieldAffix)
                                                extraData += " HasAffixShielded";

                                            if (c_HasDotDPS)
                                                extraData += " HasDotDPS";

                                            if (c_HasBeenInLoS)
                                                extraData += " HasBeenInLoS";

                                            extraData += " HP=" + c_HitPoints.ToString("0") + " (" + c_HitPointsPct.ToString("0.00") + ")";
                                        } break;
                                    case GObjectType.Avoidance:
                                        {
                                            extraData += " Ro:" + c_Rotation.ToString("0.00");
                                            break;
                                        }
                                }

                                if (c_IgnoreReason != "InternalName")
                                    Logger.Log(TrinityLogLevel.Debug, LogCategory.CacheManagement,
                                        "[{0:0000.00}ms] {1} {2} Type: {3} ({4}/{5}) Name={6} ({7}) {8} {9} Dist2Mid={10:0} Dist2Rad={11:0} ZDiff={12:0} Radius={13:0} RAGuid={14} {15}",
                                        duration,
                                        (AddToCache ? "Added " : "Ignored"),
                                        (!AddToCache ? ("By: " + (c_IgnoreReason != "None" ? c_IgnoreReason + "." : "") + c_IgnoreSubStep) : ""),
                                        c_diaObject.ActorType,
                                        c_diaObject is DiaGizmo ? ((DiaGizmo)c_diaObject).ActorInfo.GizmoType.ToString() : "",
                                        c_ObjectType,
                                        c_InternalName,
                                        c_ActorSNO,
                                        (c_unit_IsBoss ? " IsBoss" : ""),
                                        (c_CurrentAnimation != SNOAnim.Invalid ? " Anim: " + c_CurrentAnimation : ""),
                                        c_CentreDistance,
                                        c_RadiusDistance,
                                        c_ZDiff,
                                        c_Radius,
                                        c_RActorGuid,
                                        extraData);
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        string gizmoType = "";
                        if (currentObject is DiaGizmo)
                        {
                            gizmoType = "GizmoType: " + ((DiaGizmo)currentObject).CommonData.ActorInfo.GizmoType.ToString();
                        }
                        Logger.Log(TrinityLogLevel.Error, LogCategory.UserInformation, "Error while refreshing DiaObject ActorSNO: {0} Name: {1} Type: {2} Distance: {3:0} {4}",
                                currentObject.ActorSNO, currentObject.Name, currentObject.ActorType, currentObject.Distance, gizmoType);
                        Logger.Log(TrinityLogLevel.Error, LogCategory.UserInformation, "{0}", ex);

                        if (c_ACDGUID != -1 && CacheData.ObjectType.ContainsKey(c_RActorGuid))
                        {
                            CacheData.ObjectType.Remove(c_RActorGuid);
                        }

                    }
                }

            }
        }

        private static bool UpdateCurrentTarget()
        {
            // Return true if we need to refresh objects and get a new target
            bool forceUpdate = false;
            try
            {
                Player.Position = ZetaDia.Me.Position;
                Player.CurrentHealthPct = ZetaDia.Me.HitpointsCurrentPct;

                if (CurrentTarget != null && CurrentTarget.IsUnit && CurrentTarget.Unit != null && CurrentTarget.Unit.IsValid)
                {
                    try
                    {
                        DiaUnit unit = CurrentTarget.Unit;
                        //if (unit.HitpointsCurrent <= 0d)
                        //{
                        //    Logger.Log(TrinityLogLevel.Debug, LogCategory.Behavior, "CurrentTarget is dead, setting null");
                        //    CurrentTarget = null;
                        //    forceUpdate = true;
                        //}
                        //else
                        //{
                        CurrentTarget.Position = unit.Position;
                        CurrentTarget.HitPointsPct = unit.HitpointsCurrentPct;
                        CurrentTarget.HitPoints = unit.HitpointsCurrent;
                        Logger.Log(TrinityLogLevel.Debug, LogCategory.Behavior, "Updated CurrentTarget HitPoints={0:0.00} & Position={1}", CurrentTarget.HitPointsPct, CurrentTarget.Position);
                        //}
                    }
                    catch (Exception)
                    {
                        Logger.Log(TrinityLogLevel.Debug, LogCategory.Behavior, "Error updating current target information");
                        CurrentTarget = null;
                        forceUpdate = true;
                    }
                }
                else if (CurrentTarget != null && CurrentTarget.IsUnit)
                {
                    Logger.Log(TrinityLogLevel.Debug, LogCategory.Behavior, "CurrentTarget is invalid, setting null");
                    CurrentTarget = null;
                    forceUpdate = true;
                }
            }
            catch
            {
                Logger.Log(TrinityLogLevel.Debug, LogCategory.Behavior, "Error updating current target information");
                CurrentTarget = null;
                forceUpdate = true;
            }
            return forceUpdate;
        }
        // Refresh object list from Diablo 3 memory RefreshDiaObjects()
        private static void RefreshCacheInit()
        {
            using (new PerformanceLogger("RefreshDiaObjectCache.CacheInit"))
            {

                // Update when we last refreshed with current time
                LastRefreshedCache = DateTime.UtcNow;

                // Blank current/last/next targets
                LastPrimaryTargetPosition = CurrentTarget != null ? CurrentTarget.Position : Vector3.Zero;
                KiteAvoidDestination = Vector3.Zero;
                // store last target GUID
                LastTargetRactorGUID = CurrentTarget != null ? CurrentTarget.RActorGuid : -1;
                LastTargetACDGuid = CurrentTarget != null ? CurrentTarget.ACDGuid : -1;
                //reset current target
                CurrentTarget = null;
                // Reset all variables for target-weight finding
                AnyTreasureGoblinsPresent = false;
                CurrentBotKillRange = Settings.Combat.Misc.NonEliteRange;

                // Max kill range if we're questing
                if (DataDictionary.QuestLevelAreaIds.Contains(Player.LevelAreaId))
                    CurrentBotKillRange = 300f;

                CurrentBotLootRange = Zeta.Bot.Settings.CharacterSettings.Instance.LootRadius;
                ShouldStayPutDuringAvoidance = false;

                // Always have a minimum kill radius, so we're never getting whacked without retaliating
                if (CurrentBotKillRange < 10)
                    CurrentBotKillRange = 10;

                // Not allowed to kill monsters due to profile/routine/combat targeting settings - just set the kill range to a third
                if (!ProfileManager.CurrentProfile.KillMonsters || !CombatTargeting.Instance.AllowedToKillMonsters)
                {
                    CurrentBotKillRange = 0;
                }

                // Not allowed to loots due to profile/routine/loot targeting settings - just set range to a quarter
                if (!ProfileManager.CurrentProfile.PickupLoot || !LootTargeting.Instance.AllowedToLoot)
                {
                    CurrentBotLootRange = 0;
                }

                if (Player.ActorClass == ActorClass.Barbarian && Hotbar.Contains(SNOPower.Barbarian_WrathOfTheBerserker) && GetHasBuff(SNOPower.Barbarian_WrathOfTheBerserker))
                { //!sp - keep looking for kills while WOTB is up
                    iKeepKillRadiusExtendedFor = Math.Max(3, iKeepKillRadiusExtendedFor);
                    timeKeepKillRadiusExtendedUntil = DateTime.UtcNow.AddSeconds(iKeepKillRadiusExtendedFor);
                }
                // Counter for how many cycles we extend or reduce our attack/kill radius, and our loot radius, after a last kill
                if (iKeepKillRadiusExtendedFor > 0)
                {
                    TimeSpan diffResult = DateTime.UtcNow.Subtract(timeKeepKillRadiusExtendedUntil);
                    iKeepKillRadiusExtendedFor = (int)diffResult.Seconds;
                    //DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.Moving, "Kill Radius remaining " + diffResult.Seconds + "s");
                    if (timeKeepKillRadiusExtendedUntil <= DateTime.UtcNow)
                    {
                        iKeepKillRadiusExtendedFor = 0;
                    }
                }
                if (iKeepLootRadiusExtendedFor > 0)
                    iKeepLootRadiusExtendedFor--;

                // Clear forcing close-range priority on mobs after XX period of time
                if (ForceCloseRangeTarget && DateTime.UtcNow.Subtract(lastForcedKeepCloseRange).TotalMilliseconds > ForceCloseRangeForMilliseconds)
                {
                    ForceCloseRangeTarget = false;
                }
                // Bunch of variables used throughout
                CacheData.MonsterObstacles = new HashSet<CacheObstacleObject>();
                CacheData.TimeBoundAvoidance = new HashSet<CacheObstacleObject>();
                CacheData.NavigationObstacles = new HashSet<CacheObstacleObject>();
                //AnyElitesPresent = false;
                AnyMobsInRange = false;

                IsAvoidingProjectiles = false;
                // Every 15 seconds, clear the "blackspots" where avoidance failed, so we can re-check them
                if (DateTime.UtcNow.Subtract(lastClearedAvoidanceBlackspots).TotalSeconds > 15)
                {
                    lastClearedAvoidanceBlackspots = DateTime.UtcNow;
                    hashAvoidanceBlackspot = new HashSet<CacheObstacleObject>();
                }
                // Clear our very short-term destructible blacklist within 3 seconds of last attacking a destructible
                if (bNeedClearDestructibles && DateTime.UtcNow.Subtract(lastDestroyedDestructible).TotalMilliseconds > 2500)
                {
                    bNeedClearDestructibles = false;
                    hashRGUIDDestructible3SecBlacklist = new HashSet<int>();
                }
                // Clear our very short-term ignore-monster blacklist (from not being able to raycast on them or already dead units)
                if (NeedToClearBlacklist3 && DateTime.UtcNow.Subtract(dateSinceBlacklist3Clear).TotalMilliseconds > 3000)
                {
                    NeedToClearBlacklist3 = false;
                    hashRGUIDBlacklist3 = new HashSet<int>();
                }

                // Reset the counters for player-owned things
                iPlayerOwnedMysticAlly = 0;
                iPlayerOwnedGargantuan = 0;
                PlayerOwnedZombieDog = 0;
                iPlayerOwnedDHPets = 0;

                // Flag for if we should search for an avoidance spot or not
                StandingInAvoidance = false;
                // Highest weight found as we progress through, so we can pick the best target at the end (the one with the highest weight)
                w_HighestWeightFound = 0;
                // Here's the list we'll use to store each object
                ObjectCache = new List<TrinityCacheObject>();
                hashDoneThisRactor = new HashSet<int>();
            }
        }

        private static void ClearCachesOnGameChange(object sender, EventArgs e)
        {
            CacheData.Position = new Dictionary<int, Vector3>();
            CacheData.ObjectType = new Dictionary<int, GObjectType>();
            CacheData.ActorSNO = new Dictionary<int, int>();
            CacheData.AcdGuid = new Dictionary<int, int>();
            CacheData.CurrentUnitHealth = new Dictionary<int, double>();
            CacheData.LastCheckedUnitHealth = new Dictionary<int, int>();
            CacheData.UnitMonsterAffix = new Dictionary<int, MonsterAffixes>();
            CacheData.UnitMaxHealth = new Dictionary<int, double>();
            CacheData.MonsterTypes = new Dictionary<int, MonsterType>();
            CacheData.MonsterSizes = new Dictionary<int, MonsterSize>();
            CacheData.UnitIsBurrowed = new Dictionary<int, bool>();
            CacheData.SummonedByACDId = new Dictionary<int, int>();
            CacheData.Name = new Dictionary<int, string>();
            CacheData.GoldStack = new Dictionary<int, int>();
            CacheData.GameBalanceID = new Dictionary<int, int>();
            CacheData.DynamicID = new Dictionary<int, int>();
            CacheData.ItemQuality = new Dictionary<int, ItemQuality>();
            CacheData.PickupItem = new Dictionary<int, bool>();
            CacheData.HasBeenRayCasted = new Dictionary<int, bool>();
            CacheData.HasBeenNavigable = new Dictionary<int, bool>();
            CacheData.HasBeenInLoS = new Dictionary<int, bool>();
        }

        private static HashSet<string> ignoreNames = new HashSet<string>
        {
            "MarkerLocation", "Generic_Proxy", "Hireling", "Start_Location", "SphereTrigger", "Checkpoint", "ConductorProxyMaster", "BoxTrigger", "SavePoint", "TriggerSphere", 
            "minimapicon", 
        };

        
        private static IOrderedEnumerable<DiaObject> ReadDebugActorsFromMemory()
        {
            return from o in ZetaDia.Actors.GetActorsOfType<DiaObject>(true, false)
                   orderby o.Distance
                   select o;
        }

        private static IEnumerable<DiaObject> ReadActorsFromMemory()
        {
            return from o in ZetaDia.Actors.GetActorsOfType<DiaObject>(true, false)
                   select o;
        }

        private static void RefreshDoBackTrack()
        {
            // See if we should wait for [playersetting] milliseconds for possible loot drops before continuing run
            if (DateTime.UtcNow.Subtract(lastHadUnitInSights).TotalMilliseconds <= Settings.Combat.Misc.DelayAfterKill || 
                DateTime.UtcNow.Subtract(lastHadEliteUnitInSights).TotalMilliseconds <= Settings.Combat.Misc.DelayAfterKill ||
                DateTime.UtcNow.Subtract(lastHadContainerInSights).TotalMilliseconds <= 1000)
            {
                CurrentTarget = new TrinityCacheObject()
                                    {
                                        Position = Player.Position,
                                        Type = GObjectType.Avoidance,
                                        Weight = 20000,
                                        CentreDistance = 2f,
                                        RadiusDistance = 2f,
                                        InternalName = "WaitForLootDrops"
                                    };
                Logger.Log(TrinityLogLevel.Debug, LogCategory.Behavior, "Waiting for loot to drop, delay: {0}ms", Settings.Combat.Misc.DelayAfterKill);
            }
            // Now see if we need to do any backtracking
            if (CurrentTarget == null && iTotalBacktracks >= 2 && Settings.Combat.Misc.AllowBacktracking && !Player.IsInTown)
            // Never bother with the 1st backtrack position nor if we are in town
            {
                // See if we're already within 18 feet of our start position first
                if (Vector3.Distance(Player.Position, vBacktrackList[1]) <= 18f)
                {
                    vBacktrackList = new SortedList<int, Vector3>();
                    iTotalBacktracks = 0;
                }
                // See if we can raytrace to the final location and it's within 25 feet
                if (iTotalBacktracks >= 2 && Vector3.Distance(Player.Position, vBacktrackList[1]) <= 25f &&
                    NavHelper.CanRayCast(Player.Position, vBacktrackList[1]))
                {
                    vBacktrackList = new SortedList<int, Vector3>();
                    iTotalBacktracks = 0;
                }
                if (iTotalBacktracks >= 2)
                {
                    // See if we can skip to the next backtracker location first
                    if (iTotalBacktracks >= 3)
                    {
                        if (Vector3.Distance(Player.Position, vBacktrackList[iTotalBacktracks - 1]) <= 10f)
                        {
                            vBacktrackList.Remove(iTotalBacktracks);
                            iTotalBacktracks--;
                        }
                    }
                    CurrentTarget = new TrinityCacheObject()
                                        {
                                            Position = vBacktrackList[iTotalBacktracks],
                                            Type = GObjectType.Backtrack,
                                            Weight = 20000,
                                            CentreDistance = Vector3.Distance(Player.Position, vBacktrackList[iTotalBacktracks]),
                                            RadiusDistance = Vector3.Distance(Player.Position, vBacktrackList[iTotalBacktracks]),
                                            InternalName = "Backtrack"
                                        };
                }
            }
            else
            {
                vBacktrackList = new SortedList<int, Vector3>();
                iTotalBacktracks = 0;
            }
            // End of backtracking check
            //TODO : If this code is obselete remove it (Check that) 
            // Finally, a special check for waiting for wrath of the berserker cooldown before engaging Azmodan
            if (CurrentTarget == null && Hotbar.Contains(SNOPower.Barbarian_WrathOfTheBerserker) && Settings.Combat.Barbarian.WaitWOTB && !SNOPowerUseTimer(SNOPower.Barbarian_WrathOfTheBerserker) &&
                ZetaDia.CurrentWorldId == 121214 &&
                (Vector3.Distance(Player.Position, new Vector3(711.25f, 716.25f, 80.13903f)) <= 40f || Vector3.Distance(Player.Position, new Vector3(546.8467f, 551.7733f, 1.576313f)) <= 40f))
            {
                DisableOutofCombatSprint = true;
                BarbarianCombat.AllowSprintOOC = false;
                Logger.Log("[Trinity] Waiting for Wrath Of The Berserker cooldown before continuing to Azmodan.");
                CurrentTarget = new TrinityCacheObject()
                                    {
                                        Position = Player.Position,
                                        Type = GObjectType.Avoidance,
                                        Weight = 20000,
                                        CentreDistance = 2f,
                                        RadiusDistance = 2f,
                                        InternalName = "WaitForWrath"
                                    };
            }
            // And a special check for wizard archon
            if (CurrentTarget == null && Hotbar.Contains(SNOPower.Wizard_Archon) && !SNOPowerUseTimer(SNOPower.Wizard_Archon) && Settings.Combat.Wizard.WaitArchon && ZetaDia.CurrentWorldId == 121214 &&
                (Vector3.Distance(Player.Position, new Vector3(711.25f, 716.25f, 80.13903f)) <= 40f || Vector3.Distance(Player.Position, new Vector3(546.8467f, 551.7733f, 1.576313f)) <= 40f))
            {
                Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "Waiting for Wizard Archon cooldown before continuing to Azmodan.");
                CurrentTarget = new TrinityCacheObject()
                                    {
                                        Position = Player.Position,
                                        Type = GObjectType.Avoidance,
                                        Weight = 20000,
                                        CentreDistance = 2f,
                                        RadiusDistance = 2f,
                                        InternalName = "WaitForArchon"
                                    };
            }
            // And a very sexy special check for WD BigBadVoodoo
            if (CurrentTarget == null && Hotbar.Contains(SNOPower.Witchdoctor_BigBadVoodoo) && !PowerManager.CanCast(SNOPower.Witchdoctor_BigBadVoodoo) && ZetaDia.CurrentWorldId == 121214 &&
                (Vector3.Distance(Player.Position, new Vector3(711.25f, 716.25f, 80.13903f)) <= 40f || Vector3.Distance(Player.Position, new Vector3(546.8467f, 551.7733f, 1.576313f)) <= 40f))
            {
                Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "Waiting for WD BigBadVoodoo cooldown before continuing to Azmodan.");
                CurrentTarget = new TrinityCacheObject()
                                    {
                                        Position = Player.Position,
                                        Type = GObjectType.Avoidance,
                                        Weight = 20000,
                                        CentreDistance = 2f,
                                        RadiusDistance = 2f,
                                        InternalName = "WaitForVoodooo"
                                    };
            }
        }


    }
}
