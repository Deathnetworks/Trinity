using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Trinity.Cache;
using Trinity.Combat.Abilities;
using Trinity.Config.Combat;
using Trinity.Configuration;
using Trinity.Technicals;
using Zeta.Bot;
using Zeta.Common;
using Zeta.Common.Plugins;
using Zeta.Game;
using Zeta.Game.Internals;
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

                /*
                 *  Refresh the Cache
                 */
                using (new PerformanceLogger("RefreshDiaObjectCache.UpdateBlock"))
                {
                    CacheData.Clear();
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

                /*
                 * Add Legendary & Set Minimap Markers to ObjectCache
                 */
                RefreshCacheMarkers();

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

                                if (Player.Position.Distance2D(fireChainSpot) <= fireChainSize)
                                {
                                    Logger.Log(TrinityLogLevel.Debug, LogCategory.CacheManagement, "Avoiding Fire Chains!");
                                    _standingInAvoidance = true;
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
                                _standingInAvoidance = true;
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



                // Reduce ignore-for-loops counter
                if (_ignoreTargetForLoops > 0)
                    _ignoreTargetForLoops--;
                // If we have an avoidance under our feet, then create a new object which contains a safety point to move to
                // But only if we aren't force-cancelling avoidance for XX time
                bool hasFoundSafePoint = false;

                using (new PerformanceLogger("RefreshDiaObjectCache.AvoidanceCheck"))
                {
                    if (Player.IsGhosted)
                        _standingInAvoidance = true;

                    // Note that if treasure goblin level is set to kamikaze, even avoidance moves are disabled to reach the goblin!
                    if (_standingInAvoidance && (!AnyTreasureGoblinsPresent || Settings.Combat.Misc.GoblinPriority <= GoblinPriority.Prioritize) &&
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
                                    Player.CurrentHealthPct, CombatBase.PlayerKiteDistance);
                            }

                            hasFoundSafePoint = true;
                            CurrentTarget = new TrinityCacheObject()
                                {
                                    Position = vAnySafePoint,
                                    Type = GObjectType.Avoidance,
                                    Weight = 20000,
                                    Distance = Vector3.Distance(Player.Position, vAnySafePoint),
                                    Radius = 2f,
                                    InternalName = "SafePoint"
                                }; ;
                        }
                    }
                }

                /*
                 *  Set Weights, assign CurrentTarget
                 */

                if (!hasFoundSafePoint)
                {
                    RefreshDiaGetWeights();

                    RefreshSetKiting(ref KiteAvoidDestination, NeedToKite);
                }
                // Not heading straight for a safe-spot?
                // No valid targets but we were told to stay put?
                if (CurrentTarget == null && _shouldStayPutDuringAvoidance && !_standingInAvoidance && Settings.Combat.Misc.AvoidAoEOutOfCombat)
                {
                    CurrentTarget = new TrinityCacheObject()
                                        {
                                            Position = Player.Position,
                                            Type = GObjectType.Avoidance,
                                            Weight = 20000,
                                            Distance = 2f,
                                            Radius = 2f,
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
                            Distance = 2f,
                            Radius = 2f,
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
                        DontMoveMeIAmDoingShit = true;
                    }

                    if (Settings.WorldObject.EnableBountyEvents)
                    {
                        bool eventObjectNear = ObjectCache.Any(o => o.Type == GObjectType.CursedChest || o.Type == GObjectType.CursedShrine);

                        if (!Player.InActiveEvent)
                        {
                            EventStartPosition = Vector3.Zero;
                            EventStartTime = DateTime.MinValue;
                        }

                        // Reset Event time while we have targts
                        if (CurrentTarget != null && Player.InActiveEvent && eventObjectNear)
                        {
                            EventStartTime = DateTime.UtcNow;
                        }

                        if (eventObjectNear)
                        {
                            EventStartPosition = ObjectCache.FirstOrDefault(o => o.Type == GObjectType.CursedChest || o.Type == GObjectType.CursedShrine).Position;
                        }

                        var activeEvent = ZetaDia.ActInfo.ActiveQuests.FirstOrDefault(q => DataDictionary.EventQuests.Contains(q.QuestSNO));

                        const int waitTimeoutSeconds = 90;
                        if (DateTime.UtcNow.Subtract(EventStartTime).TotalSeconds > waitTimeoutSeconds && activeEvent != null)
                        {
                            CacheData.BlacklistedEvents.Add(activeEvent.QuestSNO);
                        }

                        if (CurrentTarget == null && Player.InActiveEvent && EventStartPosition != Vector3.Zero &&
                            DateTime.UtcNow.Subtract(EventStartTime).TotalSeconds < waitTimeoutSeconds &&
                            activeEvent != null && !CacheData.BlacklistedEvents.Contains(activeEvent.QuestSNO))
                        {
                            CurrentTarget = new TrinityCacheObject()
                            {
                                Position = EventStartPosition,
                                Type = GObjectType.Avoidance,
                                Weight = 20000,
                                Distance = 2f,
                                Radius = 2f,
                                InternalName = "WaitForEvent"
                            };
                            Logger.Log("Waiting for Event {0} - Time Remaining: {1:0} seconds",
                                ZetaDia.ActInfo.ActiveQuests.FirstOrDefault(q => DataDictionary.EventQuests.Contains(q.QuestSNO)).Quest,
                                waitTimeoutSeconds - DateTime.UtcNow.Subtract(EventStartTime).TotalSeconds);
                        }
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


                    if (CurrentTarget.IsUnit)
                        lastHadUnitInSights = DateTime.UtcNow;

                    if (CurrentTarget.IsBossOrEliteRareUnique)
                        lastHadEliteUnitInSights = DateTime.UtcNow;

                    if (CurrentTarget.IsBoss || CurrentTarget.IsBountyObjective)
                        lastHadBossUnitInSights = DateTime.UtcNow;


                    if (CurrentTarget.Type == GObjectType.Container)
                        lastHadContainerInSights = DateTime.UtcNow;

                    // Record the last time our target changed
                    if (LastTargetRactorGUID != CurrentTarget.RActorGuid)
                    {
                        RecordTargetHistory();

                        Logger.Log(TrinityLogLevel.Verbose, LogCategory.Weight,
                            "Found New Target {0} dist={1:0} IsElite={2} Radius={3:0.0} Weight={4:0} ActorSNO={5} " +
                            "Anim={6} TargetedCount={7} Type={8} ",
                            CurrentTarget.InternalName,
                            CurrentTarget.Distance,
                            CurrentTarget.IsEliteRareUnique,
                            CurrentTarget.Radius,
                            CurrentTarget.Weight,
                            CurrentTarget.ActorSNO,
                            CurrentTarget.Animation,
                            CurrentTarget.TimesBeenPrimaryTarget,
                            CurrentTarget.Type
                            );

                        _lastPickedTargetTime = DateTime.UtcNow;
                        _targetLastHealth = 0f;
                    }
                    else
                    {
                        // We're sticking to the same target, so update the target's health cache to check for stucks
                        if (CurrentTarget.IsUnit)
                        {
                            // Check if the health has changed, if so update the target-pick time before we blacklist them again
                            if (CurrentTarget.HitPointsPct != _targetLastHealth)
                            {
                                Logger.Log(TrinityLogLevel.Debug, LogCategory.Weight, "Keeping Target {0} - CurrentTarget.HitPoints: {1:0.00} TargetLastHealth: {2:0.00} ",
                                                CurrentTarget.RActorGuid, CurrentTarget.HitPointsPct, _targetLastHealth);
                                _lastPickedTargetTime = DateTime.UtcNow;
                            }
                            // Now store the target's last-known health
                            _targetLastHealth = CurrentTarget.HitPointsPct;
                        }
                    }
                }

                // We have a target and the cached was refreshed
                Events.OnCacheUpdatedHandler.Invoke();
                return true;
            }
        }

        /// <summary>
        /// Adds Legendary & Set Minimap Markers to ObjectCache
        /// </summary>
        private static void RefreshCacheMarkers()
        {
            const int setItemMarkerTexture = 404424;
            const int legendaryItemMarkerTexture = 275968;

            foreach (var marker in ZetaDia.Minimap.Markers.CurrentWorldMarkers.Where(m => m.IsValid && (m.MinimapTexture == setItemMarkerTexture || m.MinimapTexture == legendaryItemMarkerTexture)))
            {
                ObjectCache.Add(new TrinityCacheObject()
                {
                    Position = marker.Position,
                    InternalName = (marker.MinimapTexture == setItemMarkerTexture ? "Set Item" : "Legendary Item") + " Minimap Marker",
                    Distance = marker.Position.Distance(Player.Position),
                    ActorType = ActorType.Item,
                    Type = GObjectType.Item,
                    Radius = 2f,
                    Weight = 20000
                });
            }

            // Add Rift Guardian POI's or Markers to ObjectCache
            const int riftGuardianMarkerTexture = 81058;
            bool isRiftGuardianQuestStep = ZetaDia.CurrentQuest.QuestSNO == 337492 && ZetaDia.CurrentQuest.StepId == 16;
            Func<MinimapMarker, bool> riftGuardianMarkerFunc = m => m.IsValid && ((m.IsPointOfInterest && isRiftGuardianQuestStep) || m.MinimapTexture == riftGuardianMarkerTexture);

            foreach (var marker in ZetaDia.Minimap.Markers.CurrentWorldMarkers.Where(riftGuardianMarkerFunc))
            {
                ObjectCache.Add(new TrinityCacheObject()
                {
                    Position = marker.Position,
                    InternalName = "Rift Guardian",
                    Distance = marker.Position.Distance(Player.Position),
                    ActorType = ActorType.Monster,
                    Type = GObjectType.Unit,
                    Radius = 10f,
                    Weight = 5000,
                });
            }

            if (ZetaDia.CurrentQuest.QuestSNO == 337492 && ZetaDia.CurrentQuest.StepId == 16) // X1_LR_DungeonFinder
            {
                foreach (var marker in ZetaDia.Minimap.Markers.CurrentWorldMarkers.Where(m => m.IsPointOfInterest))
                {
                    ObjectCache.Add(new TrinityCacheObject()
                    {
                        Position = marker.Position,
                        InternalName = "Rift Guardian",
                        Distance = marker.Position.Distance(Player.Position),
                        ActorType = ActorType.Monster,
                        Type = GObjectType.Unit,
                        Radius = 10f,
                        Weight = 5000,
                    });
                }
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
                        bool addToCache;

                        if (!Settings.Advanced.LogCategories.HasFlag(LogCategory.CacheManagement))
                        {
                            /*
                             *  Main Cache Function
                             */
                            CacheDiaObject(currentObject);
                        }
                        else
                        {
                            // We're debugging, slightly slower, calculate performance metrics and dump debugging to log 
                            t1.Reset();
                            t1.Start();

                            /*
                             *  Main Cache Function
                             */
                            addToCache = CacheDiaObject(currentObject);

                            if (t1.IsRunning)
                                t1.Stop();

                            double duration = t1.Elapsed.TotalMilliseconds;

                            // don't log stuff we never care about
                            if (duration <= 1 && c_IgnoreSubStep == "IgnoreNames")
                                continue;

                            string extraData = "";

                            switch (CurrentCacheObject.Type)
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
                                        extraData += _standingInAvoidance ? "InAoE " : "";
                                        break;
                                    }
                            }

                            if (c_IgnoreReason != "InternalName")
                                Logger.Log(TrinityLogLevel.Debug, LogCategory.CacheManagement,
                                    "[{0:0000.00}ms] {1} {2} Type: {3} ({4}/{5}) Name={6} ({7}) {8} {9} Dist2Mid={10:0} Dist2Rad={11:0} ZDiff={12:0} Radius={13:0} RAGuid={14} {15}",
                                    duration,
                                    (addToCache ? "Added " : "Ignored"),
                                    (!addToCache ? ("By: " + (c_IgnoreReason != "None" ? c_IgnoreReason + "." : "") + c_IgnoreSubStep) : ""),
                                    CurrentCacheObject.ActorType,
                                    CurrentCacheObject.GizmoType != GizmoType.None ? CurrentCacheObject.GizmoType.ToString() : "",
                                    CurrentCacheObject.Type,
                                    CurrentCacheObject.InternalName,
                                    CurrentCacheObject.ActorSNO,
                                    (CurrentCacheObject.IsBoss ? " IsBoss" : ""),
                                    (c_CurrentAnimation != SNOAnim.Invalid ? " Anim: " + c_CurrentAnimation : ""),
                                    CurrentCacheObject.Distance,
                                    CurrentCacheObject.RadiusDistance,
                                    c_ZDiff,
                                    CurrentCacheObject.Radius,
                                    CurrentCacheObject.RActorGuid,
                                    extraData);
                        }
                    }
                    catch (Exception ex)
                    {
                        TrinityLogLevel ll = TrinityLogLevel.Debug;
                        LogCategory lc = LogCategory.CacheManagement;

                        if (ex is NullReferenceException)
                        {
                            ll = TrinityLogLevel.Error;
                            lc = LogCategory.UserInformation;
                        }

                        string gizmoType = "";
                        var giz = currentObject as DiaGizmo;
                        if (giz != null)
                        {
                            gizmoType = "GizmoType: " + giz.CommonData.ActorInfo.GizmoType.ToString();
                        }
                        Logger.Log(ll, lc, "Error while refreshing DiaObject ActorSNO: {0} Name: {1} Type: {2} Distance: {3:0} {4} {5}",
                                currentObject.ActorSNO, currentObject.Name, currentObject.ActorType, currentObject.Distance, gizmoType, ex.Message);

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
                _shouldStayPutDuringAvoidance = false;

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
                    _keepKillRadiusExtendedForSeconds = Math.Max(3, _keepKillRadiusExtendedForSeconds);
                    _timeKeepKillRadiusExtendedUntil = DateTime.UtcNow.AddSeconds(_keepKillRadiusExtendedForSeconds);
                }
                // Counter for how many cycles we extend or reduce our attack/kill radius, and our loot radius, after a last kill
                if (_keepKillRadiusExtendedForSeconds > 0)
                {
                    TimeSpan diffResult = DateTime.UtcNow.Subtract(_timeKeepKillRadiusExtendedUntil);
                    _keepKillRadiusExtendedForSeconds = (int)diffResult.Seconds;
                    //DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.Moving, "Kill Radius remaining " + diffResult.Seconds + "s");
                    if (_timeKeepKillRadiusExtendedUntil <= DateTime.UtcNow)
                    {
                        _keepKillRadiusExtendedForSeconds = 0;
                    }
                }
                if (_keepLootRadiusExtendedForSeconds > 0)
                    _keepLootRadiusExtendedForSeconds--;

                // Clear forcing close-range priority on mobs after XX period of time
                if (_forceCloseRangeTarget && DateTime.UtcNow.Subtract(_lastForcedKeepCloseRange).TotalMilliseconds > ForceCloseRangeForMilliseconds)
                {
                    _forceCloseRangeTarget = false;
                }

                //AnyElitesPresent = false;
                AnyMobsInRange = false;

                _isAvoidingProjectiles = false;

                // Clear our very short-term destructible blacklist within 3 seconds of last attacking a destructible
                if (_needClearDestructibles && DateTime.UtcNow.Subtract(_lastDestroyedDestructible).TotalMilliseconds > 2500)
                {
                    _needClearDestructibles = false;
                    _destructible3SecBlacklist = new HashSet<int>();
                }
                // Clear our very short-term ignore-monster blacklist (from not being able to raycast on them or already dead units)
                if (NeedToClearBlacklist3 && DateTime.UtcNow.Subtract(Blacklist3LastClear).TotalMilliseconds > 3000)
                {
                    NeedToClearBlacklist3 = false;
                    Blacklist3Seconds = new HashSet<int>();
                }

                // Reset the counters for player-owned things
                PlayerOwnedMysticAllyCount = 0;
                PlayerOwnedGargantuanCount = 0;
                PlayerOwnedZombieDogCount = 0;
                PlayerOwnedDHPetsCount = 0;
                PlayerOwnedDHSentryCount = 0;

                // Flag for if we should search for an avoidance spot or not
                _standingInAvoidance = false;

                // Highest weight found as we progress through, so we can pick the best target at the end (the one with the highest weight)
                HighestWeightFound = 0;

                // Here's the list we'll use to store each object
                ObjectCache = new List<TrinityCacheObject>();
            }
        }

        private static void ClearCachesOnGameChange(object sender, EventArgs e)
        {
            CacheData.FullClear();
        }

        private static HashSet<string> ignoreNames = new HashSet<string>
        {
            "MarkerLocation", "Generic_Proxy", "Hireling", "Start_Location", "SphereTrigger", "Checkpoint", "ConductorProxyMaster", "BoxTrigger", "SavePoint", "TriggerSphere", 
            "minimapicon", 
        };


        private static List<DiaObject> ReadDebugActorsFromMemory()
        {
            return (from o in ZetaDia.Actors.GetActorsOfType<DiaObject>(true, false)
                    where o.IsValid && o.CommonData != null && o.CommonData.IsValid
                    orderby o.Distance
                    select o).ToList();
        }

        private static IEnumerable<DiaObject> ReadActorsFromMemory()
        {
            return from o in ZetaDia.Actors.GetActorsOfType<DiaObject>(true, false)
                   where o.IsValid && o.CommonData != null && o.CommonData.IsValid
                   select o;
        }

        private static void RefreshDoBackTrack()
        {

            // See if we should wait for [playersetting] milliseconds for possible loot drops before continuing run
            if (CurrentTarget == null &&
                (DateTime.UtcNow.Subtract(lastHadUnitInSights).TotalMilliseconds <= Settings.Combat.Misc.DelayAfterKill ||
                DateTime.UtcNow.Subtract(lastHadEliteUnitInSights).TotalMilliseconds <= Settings.Combat.Misc.DelayAfterKill ||
                DateTime.UtcNow.Subtract(lastHadBossUnitInSights).TotalMilliseconds <= 3000 ||
                DateTime.UtcNow.Subtract(Helpers.Composites.LastFoundHoradricCache).TotalMilliseconds <= 5000) ||
                DateTime.UtcNow.Subtract(lastHadContainerInSights).TotalMilliseconds <= Settings.WorldObject.OpenContainerDelay)
            {
                CurrentTarget = new TrinityCacheObject()
                                    {
                                        Position = Player.Position,
                                        Type = GObjectType.Avoidance,
                                        Weight = 20000,
                                        Distance = 2f,
                                        Radius = 2f,
                                        InternalName = "WaitForLootDrops"
                                    };
                Logger.Log(TrinityLogLevel.Debug, LogCategory.Behavior, "Waiting for loot to drop, delay: {0}ms", Settings.Combat.Misc.DelayAfterKill);
            }

            // End of backtracking check
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
                                        Distance = 2f,
                                        Radius = 2f,
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
                                        Distance = 2f,
                                        Radius = 2f,
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
                                        Distance = 2f,
                                        Radius = 2f,
                                        InternalName = "WaitForVoodooo"
                                    };
            }
        }


    }
}
