using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Trinity.Cache;
using Trinity.Combat.Abilities;
using Trinity.Configuration;
using Trinity.DbProvider;
using Trinity.Helpers;
using Trinity.Items;
using Trinity.Technicals;
using Trinity.UI.UIComponents;
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
        private static int Tick = 0;
        /// <summary>
        /// For backwards compatability
        /// </summary>
        public static void RefreshDiaObjects()
        {
            // Framelock should happen in the MainLoop, where we read all the actual ACD's
            using (new MemorySpy("RefreshDiaObjects()"))
            {
                RefreshDiaObjectCache();
            }
        }

        /// <summary>
        /// This method will add and update necessary information about all available actors. Determines ObjectType, 
        /// sets ranges, updates blacklists, determines avoidance, kiting, target weighting
        /// and the result is we will have a new target for the Target Handler. Returns true if the cache was refreshed.
        /// </summary>
        /// <returns>True if the cache was updated</returns>
        public static bool RefreshDiaObjectCache()
        {
            if (Tick > 10) Tick = 0;
            Tick++;

            if (!Player.IsInGame)
                return false;

            if (Player.IsLoadingWorld)
                return false;

            if (!Player.IsValid)
                return false;

            if (!Player.CommonData.IsValid)
                return false;

            if (Player.CurrentHealthPct <= 0)
                return false;

            if (Player.IsDead)
                return false;

            MonkCombat.RunOngoingPowers();

            using (new PerformanceLogger("RefreshDiaObjectCache"))
            {
                /* Reduce ignore-for-loops counter */
                if (_ignoreTargetForLoops > 0)
                    _ignoreTargetForLoops--;

                // Make sure we reset unstucker stuff here
                PlayerMover.TimesReachedStuckPoint = 0;
                PlayerMover.vSafeMovementLocation = Vector3.Zero;
                PlayerMover.TimeLastRecordedPosition = DateTime.UtcNow;

                /* Refresh the Cache */
                using (new PerformanceLogger("RefreshDiaObjectCache.UpdateBlock"))
                {
                    /* Refresh at Tick pair */
                    if ((Tick & 1) != 1)
                    {
                        using (new MemorySpy("RefreshDiaObjects().Maintain"))
                        {
                            GenericCache.MaintainCache();
                            GenericBlacklist.MaintainBlacklist();
                            CacheData.Clear();
                        }

                        /* Now pull up all the data and store anything we want to handle in the super special cache list
                        Also use many cache dictionaries to minimize DB<->D3 memory hits, and speed everything up a lot */
                        using (new MemorySpy("RefreshDiaObjects().Init"))
                        {
                            RefreshCacheInit();
                        }
                        using (new MemorySpy("RefreshDiaObjects().Loop"))
                        {
                            RefreshCacheMainLoop();
                        }

                        /* Add Legendary & Set Minimap Markers to ObjectCache */
                        using (new MemorySpy("RefreshDiaObjects().Markers"))
                        {
                            RefreshCacheMarkers();
                        }

                        /* Add Team HotSpots to the cache */
                        using (new MemorySpy("RefreshDiaObjects().HotSpots"))
                        {
                            foreach (var ghs in GroupHotSpots.GetCacheObjectHotSpots())
                            {
                                ObjectCache.Add(ghs);
                            }
                        }

                        RefreshKiteValue();

                        /* Fire Chains Experimental Avoidance */
                        using (new MemorySpy("RefreshDiaObjects().FireChains"))
                        {
                            #region FireChains
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
                                                Trinity.Player.StandingInAvoidance = true;
                                            }
                                            CacheData.AvoidanceObstacles.Add(new CacheObstacleObject(fireChainSpot, fireChainSize, -2, "FireChains"));
                                        }
                                    }
                                    if (CacheData.AvoidanceObstacles.Any(aoe => aoe.ActorSNO == -2))
                                        Logger.Log(TrinityLogLevel.Debug, LogCategory.CacheManagement, "Generated {0} avoidance points for FireChains, minDistance={1} maxDistance={2}",
                                            CacheData.AvoidanceObstacles.Count(aoe => aoe.ActorSNO == -2),
                                            CacheData.AvoidanceObstacles.Where(aoe => aoe.ActorSNO == -2)
                                                .Min(aoe => aoe.Position.Distance2D(Trinity.Player.Position)),
                                            CacheData.AvoidanceObstacles.Where(aoe => aoe.ActorSNO == -2)
                                                .Max(aoe => aoe.Position.Distance2D(Trinity.Player.Position)));
                                }
                            }
                            #endregion
                        }

                        /* Clear animation at player obsolete */
                        using (new MemorySpy("RefreshDiaObjects().Obsolete"))
                        {
                            CacheData.ClearObsolete();
                        }

                        /* Add avoidances and usafe zones to dictionary (faster check / HashSet) */
                        using (new MemorySpy("RefreshDiaObjects().SetDictionary"))
                        {
                            CacheData.SetDictionary();
                        }

                        /* Combat helper values */
                        using (new MemorySpy("RefreshDiaObjects().RefreshCombatValues"))
                        {
                            CombatBase.RefreshValues();
                        }

                        /* Set Weights, assign CurrentTarget */
                        using (new MemorySpy("RefreshDiaObjects().Weight"))
                        {
                            RefreshDiaGetWeights();
                        }

                        /* Invoke all methode called in cache update */
                        using (new MemorySpy("RefreshDiaObjects().InvokeEvents"))
                        {
                            Events.OnCacheUpdatedHandler.Invoke();
                        }
                        CacheUI.DataModel.SourceCacheObjects.Clear();
                        foreach (var co in ObjectCache)
                        {
                            CacheUI.DataModel.SourceCacheObjects.Add(co.Copy());
                        }

                    }
                    /* Refresh at Tick impair */
                    else
                    {
                        RefreshKiteValue();

                        /* Refresh grid map fields */
                        using (new MemorySpy("RefreshDiaObjects().Grid"))
                        {
                            GridMap.RefreshGridMainLoop();
                        }
                    }
                }

                using (new MemorySpy("RefreshDiaObjects().FinalCheck"))
                {
                    if (ObjectCache != null)
                    {
                        // Pre-townrun is too far away
                        if (!Player.IsInTown && TownRun.PreTownRunPosition != Vector3.Zero && TownRun.PreTownRunWorldId == Player.WorldID && !ForceVendorRunASAP
                            && TownRun.PreTownRunPosition.Distance2D(Player.Position) <= V.F("Cache.PretownRun.MaxDistance"))
                        {
                            Logger.Log(TrinityLogLevel.Debug, LogCategory.UserInformation,
                                "Pre-TownRun position is more than {0} yards away, canceling", V.I("Cache.PretownRun.MaxDistance"));
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
                                    Type = GObjectType.Player,
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
                                        Type = GObjectType.Player,
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
                                RefreshWaitTimers();
                            }

                            // Still has target
                            if (CurrentTarget != null)
                            {
                                CombatBase.SwitchToTarget(CurrentTarget);
                            }

                            // Store less important objects.
                            if (ObjectCache.Count > 1)
                            {
                                var setting = Settings.Advanced.CacheWeightThresholdPct;
                                var threshold = setting > 0 && CurrentTarget != null ? CurrentTarget.Weight * ((double)setting / 100) : 0;

                                var lowPriorityObjects = ObjectCache.DistinctBy(c => c.RActorGuid).Where(c =>
                                    c.Type != GObjectType.Avoidance && c.Type != GObjectType.Unit ||
                                    c.Weight < threshold && c.Distance > 12f && !c.IsElite
                                    ).ToDictionary(x => x.RActorGuid, x => x);

                                Logger.Log(TrinityLogLevel.Debug, LogCategory.CacheManagement, "Cached {0}/{1} ({2:0}%) WeightThreshold={3}",
                                    lowPriorityObjects.Count,
                                    ObjectCache.Count,
                                    lowPriorityObjects.Count > 0 ? ((double)lowPriorityObjects.Count / ObjectCache.Count) * 100 : 0,
                                    threshold);

                                CacheData.LowPriorityObjectCache = lowPriorityObjects;
                            }
                        }
                    }
                }

                // We have a target and the cached was refreshed
                return CurrentTarget != null;
            }
        }

        /// <summary>
        /// Adds Legendary & Set Minimap Markers to ObjectCache
        /// </summary>
        private static void RefreshCacheMarkers()
        {
            const int setItemMarkerTexture = 404424;
            const int legendaryItemMarkerTexture = 275968;

            if (!WantToTownRun && !ForceVendorRunASAP)
            {
                var legendaryItemMarkers = ZetaDia.Minimap.Markers.CurrentWorldMarkers.Where(m => m.IsValid &&
                                    m.Position.Distance2D(Player.Position) >= 45f &&
                                    (m.MinimapTexture == setItemMarkerTexture || m.MinimapTexture == legendaryItemMarkerTexture) && !Blacklist60Seconds.Contains(m.NameHash)).ToList();

                foreach (var marker in legendaryItemMarkers)
                {
                    var name = (marker.MinimapTexture == setItemMarkerTexture ? "Set Item" : "Legendary Item") + " Minimap Marker";
                    var hash = marker.NameHash + marker.Position.ToString();

                    if (GenericBlacklist.ContainsKey(hash))
                    {
                        Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "Ignoring Marker because it's blacklisted {0} {1} at {2} distance {3}",
                            name, marker.NameHash, marker.Position, marker.Position.Distance(Player.Position));
                        continue;
                    }

                    Logger.LogDebug(LogCategory.CacheManagement, "Adding {0} {1} at {2} distance {3}", name, marker.NameHash, marker.Position, marker.Position.Distance(Player.Position));
                    ObjectCache.Add(new TrinityCacheObject()
                    {
                        Position = marker.Position,
                        InternalName = name,
                        RActorGuid = marker.NameHash,
                        ActorType = ActorType.Item,
                        Type = GObjectType.Item,
                        ItemQuality = ItemQuality.Legendary,
                        ObjectHash = hash,
                        Radius = 2f,
                        Weight = 50,
                        IsMarker = true
                    });
                }

                if (legendaryItemMarkers.Any() && TrinityItemManager.FindValidBackpackLocation(true) != new Vector2(-1, -1))
                {
                    var legendaryItems = ZetaDia.Actors.GetActorsOfType<DiaItem>().Where(i => i.IsValid && i.IsACDBased && i.Position.Distance2D(ZetaDia.Me.Position) < 5f &&
                        legendaryItemMarkers.Any(im => i.Position.Distance2D(i.Position) < 2f));

                    foreach (var diaItem in legendaryItems)
                    {
                        Logger.LogDebug(LogCategory.CacheManagement, "Adding Legendary Item from Marker {0} dist={1} ActorSNO={2} ACD={3} RActor={4}",
                            diaItem.Name, diaItem.Distance, diaItem.ActorSNO, diaItem.ACDGuid, diaItem.RActorGuid);

                        ObjectCache.Add(new TrinityCacheObject()
                        {
                            Position = diaItem.Position,
                            InternalName = diaItem.Name,
                            RActorGuid = diaItem.RActorGuid,
                            ActorSNO = diaItem.ActorSNO,
                            ACDGuid = diaItem.ACDGuid,
                            IsNavigable = true,
                            IsInLineOfSight = true,
                            Distance = diaItem.Distance,
                            ActorType = ActorType.Item,
                            Type = GObjectType.Item,
                            Radius = 2f,
                            Weight = 50,
                            ItemQuality = ItemQuality.Legendary,
                            RequiredRange = 1f,
                        });
                    }
                }
            }

            bool isRiftGuardianQuestStep = ZetaDia.CurrentQuest.QuestSNO == 337492 && ZetaDia.CurrentQuest.StepId == 16;

            if (isRiftGuardianQuestStep)
            {
                // Add Rift Guardian POI's or Markers to ObjectCache
                const int riftGuardianMarkerTexture = 81058;
                Func<MinimapMarker, bool> riftGuardianMarkerFunc = m => m.IsValid && (m.IsPointOfInterest || m.MinimapTexture == riftGuardianMarkerTexture) &&
                    !Blacklist60Seconds.Contains(m.NameHash);

                foreach (var marker in ZetaDia.Minimap.Markers.CurrentWorldMarkers.Where(riftGuardianMarkerFunc))
                {
                    Logger.LogDebug(LogCategory.CacheManagement, "Adding Rift Guardian POI, distance {0}", marker.Position.Distance2D(Player.Position));
                    ObjectCache.Add(new TrinityCacheObject()
                    {
                        Position = marker.Position,
                        InternalName = "Rift Guardian",
                        RActorGuid = marker.NameHash,
                        ActorType = ActorType.Monster,
                        Type = GObjectType.Unit,
                        Radius = 35f,
                        Weight = 5000,
                        IsBoss = true,
                    });
                }
            }

            if (isRiftGuardianQuestStep || Player.ParticipatingInTieredLootRun) // X1_LR_DungeonFinder
            {
                foreach (var marker in ZetaDia.Minimap.Markers.CurrentWorldMarkers.Where(m => m.IsPointOfInterest && !Blacklist60Seconds.Contains(m.NameHash)))
                {
                    ObjectCache.Add(new TrinityCacheObject()
                    {
                        Position = marker.Position,
                        InternalName = "Rift Guardian",
                        RActorGuid = marker.NameHash,
                        ActorType = ActorType.Monster,
                        Type = GObjectType.Unit,
                        Radius = 10f,
                        Weight = 5000,
                        IsBoss = true,
                    });
                }
            }

            // Bounty POI
            // ZetaDia.ActInfo.ActiveBounty is insanely slow. It iterates over all Quests for the current active LevelAreaId. Need to find a faster way to do this.
            //if (ZetaDia.ActInfo.ActiveBounty != null && ZetaDia.ActInfo.ActiveBounty.LevelAreas.Contains((SNOLevelArea)Player.LevelAreaId) && 
            //    ZetaDia.Minimap.Markers.CurrentWorldMarkers.Any(m => m.IsPointOfInterest))
            //{
            //    foreach (var marker in ZetaDia.Minimap.Markers.CurrentWorldMarkers.Where(m => m.IsPointOfInterest && !Blacklist60Seconds.Contains(m.NameHash)))
            //    {
            //        ObjectCache.Add(new TrinityCacheObject()
            //        {
            //            Position = marker.Position,
            //            InternalName = "Bounty Objective",
            //            RActorGuid = marker.NameHash,
            //            ActorType = ActorType.Monster,
            //            Type = GObjectType.Unit,
            //            Radius = 10f,
            //            Weight = 5000,
            //            IsBoss = true,
            //        });
            //    }
            //}
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
                        // Objects deemed of low importance are stored from the last refresh
                        TrinityCacheObject cachedObject;
                        if (CacheData.LowPriorityObjectCache.TryGetValue(currentObject.RActorGuid, out cachedObject))
                        {
                            cachedObject.Distance = currentObject.Distance;
                            var timeSinceRefresh = DateTime.UtcNow.Subtract(cachedObject.LastSeenTime).TotalMilliseconds;

                            // Determine if we should use the stored object or not
                            if (timeSinceRefresh < Settings.Advanced.CacheLowPriorityRefreshRate && cachedObject.Distance > 12f)
                            {
                                cachedObject.Position = currentObject.Position;
                                ObjectCache.Add(cachedObject);
                                continue;
                            }
                        }

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
                            bool addToCache = CacheDiaObject(currentObject);

                            if (t1.IsRunning)
                                t1.Stop();

                            double duration = t1.Elapsed.TotalMilliseconds;

                            // don't log stuff we never care about
                            if (duration <= 1 && (c_IgnoreReason == "IgnoreName" || c_IgnoreReason == "InvalidName"))
                                continue;

                            string extraData = "";

                            switch (c_CacheObject.Type)
                            {
                                case GObjectType.Unit:
                                    {
                                        if (c_IsEliteRareUnique)
                                            extraData += " IsElite " + c_MonsterAffixes.ToString();

                                        if (c_unit_HasShieldAffix)
                                            extraData += " HasAffixShielded";

                                        if (c_HasDotDPS)
                                            extraData += " HasDotDPS";

                                        if (c_CacheObject.IsInLineOfSight)
                                            extraData += " HasBeenInLoS";

                                        extraData += " HP=" + c_HitPoints.ToString("0") + " (" + c_HitPointsPct.ToString("0.00") + ")";
                                    } break;
                                case GObjectType.Avoidance:
                                    {
                                        extraData += Trinity.Player.StandingInAvoidance ? "InAoE " : "";
                                        break;
                                    }
                            }

                            if (c_IgnoreReason != "InternalName")
                                Logger.Log(TrinityLogLevel.Debug, LogCategory.CacheManagement,
                                    "[{0:0000.00}ms] {1} {2} Type: {3} ({4}/{5}) Name={6} ({7}) {8} {9} Dist2Mid={10:0} Dist2Rad={11:0} ZDiff={12:0} Radius={13:0} RAGuid={14} {15}",
                                    duration,
                                    (addToCache ? "Added " : "Ignored"),
                                    (!addToCache ? ("By: " + (c_IgnoreReason != "None" ? c_IgnoreReason + "." : "") + c_InfosSubStep) : ""),
                                    c_CacheObject.ActorType,
                                    c_CacheObject.GizmoType != GizmoType.None ? c_CacheObject.GizmoType.ToString() : "",
                                    c_CacheObject.Type,
                                    c_CacheObject.InternalName,
                                    c_CacheObject.ActorSNO,
                                    (c_CacheObject.IsBoss ? " IsBoss" : ""),
                                    (c_CurrentAnimation != SNOAnim.Invalid ? " Anim: " + c_CurrentAnimation : ""),
                                    c_CacheObject.Distance,
                                    c_CacheObject.RadiusDistance,
                                    c_ZDiff,
                                    c_CacheObject.Radius,
                                    c_CacheObject.RActorGuid,
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
                        if (giz != null && giz.CommonData.IsValid)
                        {
                            gizmoType = "GizmoType: " + giz.CommonData.ActorInfo.GizmoType.ToString();
                        }
                        Logger.Log(ll, lc, "Error while refreshing DiaObject ActorSNO: {0} Name: {1} Type: {2} Distance: {3:0} {4} {5}",
                                currentObject.ActorSNO, currentObject.Name, currentObject.ActorType, currentObject.Distance, gizmoType, ex.Message);

                    }
                }

            }
        }

        // Refresh object list from Diablo 3 memory RefreshDiaObjects()
        private static void RefreshCacheInit()
        {
            using (new PerformanceLogger("RefreshDiaObjectCache.CacheInit"))
            {
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
                CurrentBotKillRange = Settings.Combat.Misc.NonEliteRange;

                //if (AnyTreasureGoblinsPresent && Settings.Combat.Misc.GoblinPriority == GoblinPriority.Kamikaze && CurrentBotKillRange < 60f)
                //    CurrentBotKillRange = 60f;

                AnyTreasureGoblinsPresent = false;

                // Max kill range if we're questing
                if (DataDictionary.QuestLevelAreaIds.Contains(Player.LevelAreaId) && CombatBase.IsQuestingMode)
                    CurrentBotKillRange = 300f;

                CurrentBotLootRange = Zeta.Bot.Settings.CharacterSettings.Instance.LootRadius;
                _shouldStayPutDuringAvoidance = false;

                // Not allowed to kill monsters due to profile/routine/combat targeting settings
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

                ClearBlacklists();

                // Reset the counters for player-owned things
                PlayerOwnedMysticAllyCount = 0;
                PlayerOwnedGargantuanCount = 0;
                PlayerOwnedZombieDogCount = 0;
                PlayerOwnedFetishCount = 0;
                PlayerOwnedDHPetsCount = 0;
                PlayerOwnedDHSentryCount = 0;

                // Flag for if we should search for an avoidance spot or not
                Trinity.Player.StandingInAvoidance = false;
                Trinity.Player.TryToAvoidProjectile = false;
                Trinity.Player.NeedToKite = false;

                IsAlreadyMoving = false;

                // Here's the list we'll use to store each object
                ObjectCache.Clear();
            }
        }

        private static void ClearCachesOnGameChange(object sender, EventArgs e)
        {
            CacheData.FullClear();
        }

        private static HashSet<string> IgnoreNames = new HashSet<string>
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

        private static void RefreshWaitTimers()
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
                    Type = GObjectType.Player,
                    Weight = 20000,
                    Distance = 2f,
                    Radius = 2f,
                    InternalName = "WaitForLootDrops"
                };
                Logger.Log(TrinityLogLevel.Debug, LogCategory.Behavior, "Waiting for loot to drop, delay: {0}ms", Settings.Combat.Misc.DelayAfterKill);
            }

            // End of backtracking check
            // Finally, a special check for waiting for wrath of the berserker cooldown before engaging Azmodan
            if (CurrentTarget == null && Hotbar.Contains(SNOPower.Barbarian_WrathOfTheBerserker)
                && Settings.Combat.Barbarian.WaitWOTB && !SNOPowerUseTimer(SNOPower.Barbarian_WrathOfTheBerserker) &&
                Player.WorldID == 121214 &&
                (Vector3.Distance(Player.Position, new Vector3(711.25f, 716.25f, 80.13903f)) <= 40f || Vector3.Distance(Player.Position, new Vector3(546.8467f, 551.7733f, 1.576313f)) <= 40f))
            {
                DisableOutofCombatSprint = true;
                BarbarianCombat.AllowSprintOOC = false;
                Logger.Log("[Trinity] Waiting for Wrath Of The Berserker cooldown before continuing to Azmodan.");
                CurrentTarget = new TrinityCacheObject()
                {
                    Position = Player.Position,
                    Type = GObjectType.Player,
                    Weight = 20000,
                    Distance = 2f,
                    Radius = 2f,
                    InternalName = "WaitForWrath"
                };
            }
            // And a special check for wizard archon
            if (CurrentTarget == null && Hotbar.Contains(SNOPower.Wizard_Archon) && !SNOPowerUseTimer(SNOPower.Wizard_Archon) && Settings.Combat.Wizard.WaitArchon && Player.WorldID == 121214 &&
                (Vector3.Distance(Player.Position, new Vector3(711.25f, 716.25f, 80.13903f)) <= 40f || Vector3.Distance(Player.Position, new Vector3(546.8467f, 551.7733f, 1.576313f)) <= 40f))
            {
                Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "Waiting for Wizard Archon cooldown before continuing to Azmodan.");
                CurrentTarget = new TrinityCacheObject()
                {
                    Position = Player.Position,
                    Type = GObjectType.Player,
                    Weight = 20000,
                    Distance = 2f,
                    Radius = 2f,
                    InternalName = "WaitForArchon"
                };
            }
            // And a very sexy special check for WD BigBadVoodoo
            if (CurrentTarget == null && Hotbar.Contains(SNOPower.Witchdoctor_BigBadVoodoo) && !PowerManager.CanCast(SNOPower.Witchdoctor_BigBadVoodoo) && Player.WorldID == 121214 &&
                (Vector3.Distance(Player.Position, new Vector3(711.25f, 716.25f, 80.13903f)) <= 40f || Vector3.Distance(Player.Position, new Vector3(546.8467f, 551.7733f, 1.576313f)) <= 40f))
            {
                Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "Waiting for WD BigBadVoodoo cooldown before continuing to Azmodan.");
                CurrentTarget = new TrinityCacheObject()
                {
                    Position = Player.Position,
                    Type = GObjectType.Player,
                    Weight = 20000,
                    Distance = 2f,
                    Radius = 2f,
                    InternalName = "WaitForVoodooo"
                };
            }
        }


    }
}
