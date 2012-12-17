using GilesTrinity.Settings.Combat;
using GilesTrinity.Technicals;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Zeta;
using Zeta.Common;
using Zeta.Common.Plugins;
using Zeta.CommonBot;
using Zeta.Internals.Actors;
using Zeta.Internals.SNO;
namespace GilesTrinity
{
    public partial class GilesTrinity : IPlugin
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
        /// This method will add and update necessary information about all available actors. Determines GilesObjectType, sets ranges, updates blacklists, determines avoidance, kiting, target weighting
        /// and the result is we will have a new target for the Target Handler. Returns true if the cache was refreshed.
        /// </summary>
        /// <returns>True if the cache was updated</returns>
        public static bool RefreshDiaObjectCache()
        {
            if (DateTime.Now.Subtract(LastRefreshedCache).TotalMilliseconds <= 100)
                return false;

            LastRefreshedCache = DateTime.Now;

            using (ZetaDia.Memory.AcquireFrame())
            {
                // Update player-data cache, including buffs
                UpdateCachedPlayerData();

                if (playerStatus.CurrentHealthPct <= 0)
                {
                    return false;
                }

                //RefreshInit(out vSafePointNear, out vKitePointAvoid, out iCurrentTargetRactorGUID, out iUnitsSurrounding, out iHighestWeightFound, out listGilesObjectCache, out hashDoneThisRactor);
                RefreshCacheInit();
                // Now pull up all the data and store anything we want to handle in the super special cache list
                // Also use many cache dictionaries to minimize DB<->D3 memory hits, and speed everything up a lot
                RefreshCacheMainLoop();
            }
            // Reduce ignore-for-loops counter
            if (IgnoreTargetForLoops > 0)
                IgnoreTargetForLoops--;
            // If we have an avoidance under our feet, then create a new object which contains a safety point to move to
            // But only if we aren't force-cancelling avoidance for XX time
            bool bFoundSafeSpot = false;

            // Note that if treasure goblin level is set to kamikaze, even avoidance moves are disabled to reach the goblin!
            if (StandingInAvoidance && (!bAnyTreasureGoblinsPresent || Settings.Combat.Misc.GoblinPriority <= GoblinPriority.Prioritize) &&
                DateTime.Now.Subtract(timeCancelledEmergencyMove).TotalMilliseconds >= cancelledEmergencyMoveForMilliseconds)
            {
                Vector3 vAnySafePoint = FindSafeZone(false, 1, vSafePointNear);
                // Ignore avoidance stuff if we're incapacitated or didn't find a safe spot we could reach
                if (vAnySafePoint != vNullLocation)
                {
                    if (Settings.Advanced.LogCategories.HasFlag(LogCategory.Moving))
                    {
                        DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.Moving, "Kiting Avoidance: {0} Distance: {1:0} Direction: {2:0}, Health%={3:0.00}, KiteDistance: {4:0}",
                            vAnySafePoint, vAnySafePoint.Distance(Me.Position), GetHeading(FindDirectionDegree(Me.Position, vAnySafePoint)),
                            playerStatus.CurrentHealthPct, PlayerKiteDistance);
                    }

                    bFoundSafeSpot = true;
                    CurrentTarget = new GilesObject()
                        {
                            Position = vAnySafePoint,
                            Type = GObjectType.Avoidance,
                            Weight = 20000,
                            CentreDistance = Vector3.Distance(playerStatus.CurrentPosition, vAnySafePoint),
                            RadiusDistance = Vector3.Distance(playerStatus.CurrentPosition, vAnySafePoint),
                            InternalName = "GilesSafePoint"
                        }; ;
                }
                else
                {
                    // Didn't find any safe spot we could reach, so don't look for any more safe spots for at least 2.8 seconds
                    cancelledEmergencyMoveForMilliseconds = 2800;
                    timeCancelledEmergencyMove = DateTime.Now;
                    DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.Moving, "Unable to find kite location, canceling kite movement for {0}ms", cancelledKiteMoveForMilliseconds);
                }
            }
            /*
             * Give weights to objects
             */
            // Special flag for special whirlwind circumstances
            bAnyNonWWIgnoreMobsInRange = false;
            // Now give each object a weight *IF* we aren't skipping direcly to a safe-spot
            if (!bFoundSafeSpot)
            {
                RefreshDiaGetWeights();
                RefreshSetKiting(ref vKitePointAvoid, NeedToKite, ref TryToKite);
            }
            // Not heading straight for a safe-spot?
            // No valid targets but we were told to stay put?
            if (CurrentTarget == null && bStayPutDuringAvoidance && !StandingInAvoidance)
            {
                CurrentTarget = new GilesObject()
                                    {
                                        Position = playerStatus.CurrentPosition,
                                        Type = GObjectType.Avoidance,
                                        Weight = 20000,
                                        CentreDistance = 2f,
                                        RadiusDistance = 2f,
                                        InternalName = "GilesStayPutPoint"
                                    };
                DbHelper.Log(TrinityLogLevel.Debug, LogCategory.CacheManagement, "Staying Put During Avoidance");
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
            if (CurrentTarget.Type == GObjectType.Unit)
            {
                lastHadUnitInSights = DateTime.Now;
                // And record when we last saw any form of elite
                if (CurrentTarget.IsBoss || CurrentTarget.IsEliteRareUnique || CurrentTarget.IsTreasureGoblin)
                    lastHadEliteUnitInSights = DateTime.Now;
            }
            // Record the last time our target changed
            if (CurrentTargetRactorGUID != CurrentTarget.RActorGuid)
            {
                DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.Targetting, "Setting dateSincePicked to {0} CurrentTargetRactorGUID: {1} CurrentTarget.RActorGuid: {2}",
                                DateTime.Now, CurrentTargetRactorGUID, CurrentTarget.RActorGuid);
                dateSincePickedTarget = DateTime.Now;
                iTargetLastHealth = 0f;
            }
            else
            {
                // We're sticking to the same target, so update the target's health cache to check for stucks
                if (CurrentTarget.Type == GObjectType.Unit)
                {
                    // Check if the health has changed, if so update the target-pick time before we blacklist them again
                    if (CurrentTarget.HitPoints != iTargetLastHealth)
                    {
                        DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.Targetting, "Setting dateSincePicked to {0} CurrentTarget.iHitPoints: {1}  iTargetLastHealth: {2} ",
                                        DateTime.Now, CurrentTarget.HitPoints, iTargetLastHealth);
                        dateSincePickedTarget = DateTime.Now;
                    }
                    // Now store the target's last-known health
                    iTargetLastHealth = CurrentTarget.HitPoints;
                }
            }
            // We have a target and the cached was refreshed
            return true;
        }
        // Refresh object list from Diablo 3 memory RefreshDiaObjects()
        //private static void RefreshInit(out Vector3 vSafePointNear, out Vector3 vKitePointAvoid, out int iCurrentTargetRactorGUID, out int iUnitsSurrounding, out double iHighestWeightFound, out List<GilesObject> listGilesObjectCache, out HashSet<int> hashDoneThisRactor)
        private static void RefreshCacheInit()
        {
            // Update when we last refreshed with current time
            LastRefreshedCache = DateTime.Now;

            // Blank current/last/next targets
            vSafePointNear = CurrentTarget != null ? CurrentTarget.Position : vNullLocation;
            vKitePointAvoid = vNullLocation;
            // store current target GUID
            CurrentTargetRactorGUID = CurrentTarget != null ? CurrentTarget.RActorGuid : -1;
            //reset current target
            CurrentTarget = null;
            // Reset all variables for target-weight finding
            bAnyTreasureGoblinsPresent = false;
            iCurrentMaxKillRadius = (float)(Settings.Combat.Misc.NonEliteRange);
            //intell
            iCurrentMaxLootRadius = Zeta.CommonBot.Settings.CharacterSettings.Instance.LootRadius;
            bStayPutDuringAvoidance = false;
            // Set up the fake object for the target handler
            FakeObject = null;
            // Not allowed to kill monsters due to profile/routine/combat targeting settings - just set the kill range to a third
            if (!ProfileManager.CurrentProfile.KillMonsters || !CombatTargeting.Instance.AllowedToKillMonsters)
            {
                iCurrentMaxKillRadius /= 3;
            }
            // Always have a minimum kill radius, so we're never getting whacked without retaliating
            if (iCurrentMaxKillRadius < 10)
                iCurrentMaxKillRadius = 10;
            // Not allowed to loots due to profile/routine/loot targeting settings - just set range to a quarter
            if (!ProfileManager.CurrentProfile.PickupLoot || !LootTargeting.Instance.AllowedToLoot)
            {
                iCurrentMaxLootRadius /= 4;
            }
            if (playerStatus.ActorClass == ActorClass.Barbarian && hashPowerHotbarAbilities.Contains(SNOPower.Barbarian_WrathOfTheBerserker) && GilesHasBuff(SNOPower.Barbarian_WrathOfTheBerserker))
            { //!sp - keep looking for kills while WOTB is up
                iKeepKillRadiusExtendedFor = Math.Max(3, iKeepKillRadiusExtendedFor);
                timeKeepKillRadiusExtendedUntil = DateTime.Now.AddSeconds(iKeepKillRadiusExtendedFor);
            }
            // Counter for how many cycles we extend or reduce our attack/kill radius, and our loot radius, after a last kill
            if (iKeepKillRadiusExtendedFor > 0)
            {
                TimeSpan diffResult = DateTime.Now.Subtract(timeKeepKillRadiusExtendedUntil);
                iKeepKillRadiusExtendedFor = (int)diffResult.Seconds;
                //DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.Moving, "Kill Radius remaining " + diffResult.Seconds + "s");
                if (timeKeepKillRadiusExtendedUntil <= DateTime.Now)
                {
                    iKeepKillRadiusExtendedFor = 0;
                }
            }
            if (iKeepLootRadiusExtendedFor > 0)
                iKeepLootRadiusExtendedFor--;

            // Clear forcing close-range priority on mobs after XX period of time
            if (ForceCloseRangeTarget && DateTime.Now.Subtract(lastForcedKeepCloseRange).TotalMilliseconds > ForceCloseRangeForMilliseconds)
            {
                ForceCloseRangeTarget = false;
            }
            // Bunch of variables used throughout
            iUnitsSurrounding = 0;
            hashMonsterObstacleCache = new HashSet<GilesObstacle>();
            hashAvoidanceObstacleCache = new HashSet<GilesObstacle>();
            hashNavigationObstacleCache = new HashSet<GilesObstacle>();
            bAnyChampionsPresent = false;
            bAnyMobsInCloseRange = false;
            TownRun.lastDistance = 0f;
            IsAvoidingProjectiles = false;
            // Every 15 seconds, clear the "blackspots" where avoidance failed, so we can re-check them
            if (DateTime.Now.Subtract(lastClearedAvoidanceBlackspots).TotalSeconds > 15)
            {
                lastClearedAvoidanceBlackspots = DateTime.Now;
                hashAvoidanceBlackspot = new HashSet<GilesObstacle>();
            }
            // Clear our very short-term destructible blacklist within 3 seconds of last attacking a destructible
            if (bNeedClearDestructibles && DateTime.Now.Subtract(lastDestroyedDestructible).TotalMilliseconds > 2500)
            {
                bNeedClearDestructibles = false;
                hashRGUIDDestructible3SecBlacklist = new HashSet<int>();
            }
            // Clear our very short-term ignore-monster blacklist (from not being able to raycast on them or already dead units)
            if (NeedToClearBlacklist3 && DateTime.Now.Subtract(dateSinceBlacklist3Clear).TotalMilliseconds > 3000)
            {
                NeedToClearBlacklist3 = false;
                hashRGUIDBlacklist3 = new HashSet<int>();
            }
            // Clear certain cache dictionaries sequentially, spaced out over time, to force data updates
            if (DateTime.Now.Subtract(lastClearedCacheDictionary).TotalMilliseconds >= 30000)
            {
                lastClearedCacheDictionary = DateTime.Now;
                iLastClearedCacheDictionary++;
                if (iLastClearedCacheDictionary > 5)
                    iLastClearedCacheDictionary = 1;
                switch (iLastClearedCacheDictionary)
                {
                    case 1:
                        dictGilesVectorCache = new Dictionary<int, Vector3>();
                        dictGilesObjectTypeCache = new Dictionary<int, GObjectType>();
                        dictGilesActorSNOCache = new Dictionary<int, int>();
                        dictGilesACDGUIDCache = new Dictionary<int, int>();
                        dictGilesLastHealthCache = new Dictionary<int, double>();
                        dictGilesLastHealthChecked = new Dictionary<int, int>();
                        break;
                    case 2:
                        dictGilesMonsterAffixCache = new Dictionary<int, MonsterAffixes>();
                        dictGilesMaxHealthCache = new Dictionary<int, double>();
                        dictionaryStoredMonsterTypes = new Dictionary<int, MonsterType>();
                        dictionaryStoredMonsterSizes = new Dictionary<int, MonsterSize>();
                        dictGilesBurrowedCache = new Dictionary<int, bool>();
                        dictSummonedByID = new Dictionary<int, int>();
                        break;
                    case 3:
                        dictGilesInternalNameCache = new Dictionary<int, string>();
                        dictGilesGoldAmountCache = new Dictionary<int, int>();
                        break;
                    case 4:
                    case 5:
                        dictGilesGameBalanceIDCache = new Dictionary<int, int>();
                        dictGilesDynamicIDCache = new Dictionary<int, int>();
                        dictGilesQualityCache = new Dictionary<int, ItemQuality>();
                        dictGilesQualityRechecked = new Dictionary<int, bool>();
                        dictGilesPickupItem = new Dictionary<int, bool>();
                        break;
                }
            }
            // Reset the counters for player-owned things
            iPlayerOwnedMysticAlly = 0;
            iPlayerOwnedGargantuan = 0;
            iPlayerOwnedZombieDog = 0;
            iPlayerOwnedDHPets = 0;
            // Reset the counters for monsters at various ranges
            iElitesWithinRange = new int[] { 0, 0, 0, 0, 0, 0, 0, 0 };
            iAnythingWithinRange = new int[] { 0, 0, 0, 0, 0, 0, 0, 0 };
            bAnyBossesInRange = false;
            // Flag for if we should search for an avoidance spot or not
            StandingInAvoidance = false;
            // Highest weight found as we progress through, so we can pick the best target at the end (the one with the highest weight)
            w_HighestWeightFound = 0;
            // Here's the list we'll use to store each object
            GilesObjectCache = new List<GilesObject>();
            hashDoneThisRactor = new HashSet<int>();
        }

        private static HashSet<string> ignoreNames = new HashSet<string>
        {
            "MarkerLocation", "Generic_Proxy", "Hireling", "Start_Location", "SphereTrigger", "Checkpoint", "ConductorProxyMaster", "BoxTrigger", "SavePoint",
        };

        private static void RefreshCacheMainLoop()
        {
            using (new PerformanceLogger("CacheManagement.RefreshCacheMainLoop"))
            {
                ZetaDia.Actors.Update();

                var refreshSource =
                from o in ZetaDia.Actors.GetActorsOfType<DiaObject>(true, false)
                //where o.IsValid
                //orderby o.ActorType, o.Distance
                select o;

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

                            // Disabled, was missing some things on output... ServerProps maybe?
                            // bool ignore = (from n in ignoreNames
                            //               where c_Name.StartsWith(n)
                            //               select true).FirstOrDefault();
                            // if (!ignore)
                            // {

                            double duration = t1.Elapsed.TotalMilliseconds;

                            DbHelper.Log(TrinityLogLevel.Debug, LogCategory.CacheManagement,
                                "Cache: [{0:0000.0000}ms] {1} {2} Type: {3} ({4}) Name: {5} ({6}) {7} {8} Dist2Mid: {9:0} Dist2Rad: {10:0} ZDiff: {11:0} Radius: {12}",
                                duration,
                                (AddToCache ? "Added  " : " Ignored"),
                                (!AddToCache ? (" By: " + (c_IgnoreReason != "None" ? c_IgnoreReason + "." : "") + c_IgnoreSubStep) : ""),
                                c_diaObject.ActorType,
                                c_ObjectType,
                                c_Name,
                                c_ActorSNO,
                                (c_unit_IsBoss ? " IsBoss" : ""),
                                (c_CurrentAnimation != SNOAnim.Invalid ? " Anim: " + c_CurrentAnimation : ""),
                                c_CentreDistance,
                                c_RadiusDistance,
                                c_ZDiff,
                                c_Radius);
                            // }
                        }
                    }
                    catch (Exception ex)
                    {
                        DbHelper.Log(TrinityLogLevel.Debug, LogCategory.CacheManagement, "Error while refreshing DiaObject ActorSNO: {0} Name: {1} Type: {2} Distance: {3:0}",
                                currentObject.ActorSNO, currentObject.Name, currentObject.ActorType, currentObject.Distance);
                        DbHelper.Log(TrinityLogLevel.Debug, LogCategory.CacheManagement, "{0}", ex);

                    }
                }

            }
        }

        private static void RefreshItemStats(GItemBaseType tempbasetype)
        {
            if (!_hashsetItemStatsLookedAt.Contains(c_RActorGuid))
            {
                _hashsetItemStatsLookedAt.Add(c_RActorGuid);
                if (tempbasetype == GItemBaseType.Armor || tempbasetype == GItemBaseType.WeaponOneHand || tempbasetype == GItemBaseType.WeaponTwoHand ||
                    tempbasetype == GItemBaseType.WeaponRange || tempbasetype == GItemBaseType.Jewelry || tempbasetype == GItemBaseType.FollowerItem ||
                    tempbasetype == GItemBaseType.Offhand)
                {
                    int iThisQuality;
                    ItemsDroppedStats.Total++;
                    if (c_ItemQuality >= ItemQuality.Legendary)
                        iThisQuality = QUALITYORANGE;
                    else if (c_ItemQuality >= ItemQuality.Rare4)
                        iThisQuality = QUALITYYELLOW;
                    else if (c_ItemQuality >= ItemQuality.Magic1)
                        iThisQuality = QUALITYBLUE;
                    else
                        iThisQuality = QUALITYWHITE;
                    ItemsDroppedStats.TotalPerQuality[iThisQuality]++;
                    ItemsDroppedStats.TotalPerLevel[c_ItemLevel]++;
                    ItemsDroppedStats.TotalPerQPerL[iThisQuality, c_ItemLevel]++;
                }
                else if (tempbasetype == GItemBaseType.Gem)
                {
                    int iThisGemType = 0;
                    ItemsDroppedStats.TotalGems++;
                    if (c_item_GItemType == GItemType.Topaz)
                        iThisGemType = GEMTOPAZ;
                    if (c_item_GItemType == GItemType.Ruby)
                        iThisGemType = GEMRUBY;
                    if (c_item_GItemType == GItemType.Emerald)
                        iThisGemType = GEMEMERALD;
                    if (c_item_GItemType == GItemType.Amethyst)
                        iThisGemType = GEMAMETHYST;
                    ItemsDroppedStats.GemsPerType[iThisGemType]++;
                    ItemsDroppedStats.GemsPerLevel[c_ItemLevel]++;
                    ItemsDroppedStats.GemsPerTPerL[iThisGemType, c_ItemLevel]++;
                }
                else if (c_item_GItemType == GItemType.HealthPotion)
                {
                    ItemsDroppedStats.TotalPotions++;
                    ItemsDroppedStats.PotionsPerLevel[c_ItemLevel]++;
                }
                else if (c_item_GItemType == GItemType.InfernalKey)
                {
                    ItemsDroppedStats.TotalInfernalKeys++;
                }
                // See if we should update the stats file
                if (DateTime.Now.Subtract(ItemStatsLastPostedReport).TotalSeconds > 10)
                {
                    ItemStatsLastPostedReport = DateTime.Now;
                    OutputReport();
                }
            }
        }
        private static double RefreshKillRadius()
        {
            // Cancel altogether if it's not even in range, unless it's a boss or an injured treasure goblin
            double dUseKillRadius = iCurrentMaxKillRadius;
            // Bosses get extra radius
            if (c_unit_IsBoss)
            {
                if (c_ActorSNO != 80509)
                    // Kulle Exception
                    dUseKillRadius *= 1.5;
                // And even more if they're already injured
                if (c_HitPoints <= 0.98)
                    dUseKillRadius *= 4;
                // And make sure we have a MINIMUM range for bosses - incase they are at screen edge etc.
                if (dUseKillRadius <= 200)
                    if (c_ActorSNO != 80509)
                        // Kulle Exception
                        dUseKillRadius = 200;
            }
            // Special short-range list to ignore weakling mobs
            if (PlayerKiteDistance <= 0)
            {
                if (hashActorSNOShortRangeOnly.Contains(c_ActorSNO))
                    dUseKillRadius = 12;
            }
            // Prevent long-range mobs beign ignored while they may be pounding on us
            if (dUseKillRadius <= 30 && hashActorSNORanged.Contains(c_ActorSNO))
                dUseKillRadius = 80;
            //intell
            //GoatMutant_Ranged_A_Unique_Uber-10955 ActorSNO:	255996 	(act 1)
            //DuneDervish_B_Unique_Uber-14252 ActorSNO: 		256000	(act 2)
            //morluSpellcaster_A_Unique_Uber-17451 ActorSNO:	256015	(act 3)
            if (c_ActorSNO == 256015 || c_ActorSNO == 256000 || c_ActorSNO == 255996)
                dUseKillRadius = 80;
            // Injured treasure goblins get a huge extra radius - since they don't stay on the map long if injured, anyway!
            if (c_unit_IsTreasureGoblin && (c_CentreDistance <= 60 || c_HitPoints <= 0.99))
            {
                c_ForceLeapAgainst = true;
                if (Settings.Combat.Misc.GoblinPriority <= GoblinPriority.Prioritize)
                    dUseKillRadius *= 2.5;
                else
                    dUseKillRadius *= 4;
                // Minimum distance of 60
                if (dUseKillRadius <= 60) dUseKillRadius = 60;
            }
            // Elitey type mobs and things
            else if ((c_unit_IsElite || c_unit_IsRare || c_unit_IsUnique || c_unit_IsMinion))
            {
                c_ForceLeapAgainst = true;
                if (c_HitPoints <= 0.99)
                {
                    dUseKillRadius *= 2;
                    if (dUseKillRadius <= 90) dUseKillRadius = 90;
                }
                else
                {
                    if (dUseKillRadius <= 60) dUseKillRadius = 60;
                }
            }
            // Safety for Giles own portal-back-to-town for full-backpack
            if (ForceVendorRunASAP)
            {
                if (dUseKillRadius <= 90) dUseKillRadius = 90;
            }
            return dUseKillRadius;
        }
        private static MonsterAffixes RefreshAffixes(ACD tempCommonData)
        {
            MonsterAffixes theseaffixes;
            if (!dictGilesMonsterAffixCache.TryGetValue(c_RActorGuid, out theseaffixes))
            {
                try
                {
                    theseaffixes = tempCommonData.MonsterAffixes;
                    dictGilesMonsterAffixCache.Add(c_RActorGuid, theseaffixes);
                }
                catch
                {
                    theseaffixes = MonsterAffixes.None;
                }
            }
            c_unit_IsElite = theseaffixes.HasFlag(MonsterAffixes.Elite);
            c_unit_IsRare = theseaffixes.HasFlag(MonsterAffixes.Rare);
            c_unit_IsUnique = theseaffixes.HasFlag(MonsterAffixes.Unique);
            c_unit_IsMinion = theseaffixes.HasFlag(MonsterAffixes.Minion);
            return theseaffixes;
        }
        private static MonsterType RefreshMonsterType(ACD tempCommonData, MonsterType monsterType, bool bAddToDictionary)
        {
            SNORecordMonster monsterInfo = tempCommonData.MonsterInfo;
            if (monsterInfo != null)
            {
                // Force Jondar as an undead, since Diablo 3 sticks him as a permanent ally
                if (c_ActorSNO == 86624)
                {
                    monsterType = MonsterType.Undead;
                }
                else
                {
                    monsterType = monsterInfo.MonsterType;
                }
                // Is this going to be a new dictionary entry, or updating one already existing?
                if (bAddToDictionary)
                    dictionaryStoredMonsterTypes.Add(c_ActorSNO, monsterType);
                else
                    dictionaryStoredMonsterTypes[c_ActorSNO] = monsterType;
            }
            else
            {
                monsterType = MonsterType.Undead;
            }
            return monsterType;
        }
        private static void RefreshDoBackTrack()
        {
            // See if we should wait for [playersetting] milliseconds for possible loot drops before continuing run
            if (DateTime.Now.Subtract(lastHadUnitInSights).TotalMilliseconds <= Settings.Combat.Misc.DelayAfterKill || DateTime.Now.Subtract(lastHadEliteUnitInSights).TotalMilliseconds <= Settings.Combat.Misc.DelayAfterKill)
            {
                CurrentTarget = new GilesObject()
                                    {
                                        Position = playerStatus.CurrentPosition,
                                        Type = GObjectType.Avoidance,
                                        Weight = 20000,
                                        CentreDistance = 2f,
                                        RadiusDistance = 2f,
                                        InternalName = "GilesWaitForLootDrops"
                                    };
                DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.Behavior, "Waiting for loot to drop, delay: {0}ms", Settings.Combat.Misc.DelayAfterKill);
            }
            // Now see if we need to do any backtracking
            if (CurrentTarget == null && iTotalBacktracks >= 2 && Settings.Combat.Misc.AllowBacktracking && !playerStatus.IsInTown)
            // Never bother with the 1st backtrack position nor if we are in town
            {
                // See if we're already within 18 feet of our start position first
                if (Vector3.Distance(playerStatus.CurrentPosition, vBacktrackList[1]) <= 18f)
                {
                    vBacktrackList = new SortedList<int, Vector3>();
                    iTotalBacktracks = 0;
                }
                // See if we can raytrace to the final location and it's within 25 feet
                if (iTotalBacktracks >= 2 && Vector3.Distance(playerStatus.CurrentPosition, vBacktrackList[1]) <= 25f &&
                    GilesCanRayCast(playerStatus.CurrentPosition, vBacktrackList[1], NavCellFlags.AllowWalk))
                {
                    vBacktrackList = new SortedList<int, Vector3>();
                    iTotalBacktracks = 0;
                }
                if (iTotalBacktracks >= 2)
                {
                    // See if we can skip to the next backtracker location first
                    if (iTotalBacktracks >= 3)
                    {
                        if (Vector3.Distance(playerStatus.CurrentPosition, vBacktrackList[iTotalBacktracks - 1]) <= 10f)
                        {
                            vBacktrackList.Remove(iTotalBacktracks);
                            iTotalBacktracks--;
                        }
                    }
                    CurrentTarget = new GilesObject()
                                        {
                                            Position = vBacktrackList[iTotalBacktracks],
                                            Type = GObjectType.Backtrack,
                                            Weight = 20000,
                                            CentreDistance = Vector3.Distance(playerStatus.CurrentPosition, vBacktrackList[iTotalBacktracks]),
                                            RadiusDistance = Vector3.Distance(playerStatus.CurrentPosition, vBacktrackList[iTotalBacktracks]),
                                            InternalName = "GilesBacktrack"
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
            //if (CurrentTarget == null && hashPowerHotbarAbilities.Contains(SNOPower.Barbarian_WrathOfTheBerserker) && Settings.Combat.Barbarian.WaitWOTB && !GilesUseTimer(SNOPower.Barbarian_WrathOfTheBerserker) &&
            //    ZetaDia.CurrentWorldId == 121214 &&
            //    (Vector3.Distance(playerStatus.CurrentPosition, new Vector3(711.25f, 716.25f, 80.13903f)) <= 40f || Vector3.Distance(playerStatus.CurrentPosition, new Vector3(546.8467f, 551.7733f, 1.576313f)) <= 40f))
            //{
            //    bDontSpamOutofCombat = true;
            //    Logging.Write("[Trinity] Waiting for Wrath Of The Berserker cooldown before continuing to Azmodan.");
            //    CurrentTarget = new GilesObject()
            //                        {
            //                            Position = playerStatus.CurrentPosition,
            //                            Type = GObjectType.Avoidance,
            //                            Weight = 20000,
            //                            CentreDistance = 2f,
            //                            RadiusDistance = 2f,
            //                            InternalName = "GilesWaitForWrath"
            //                        };
            //}
            // And a special check for wizard archon
            if (CurrentTarget == null && hashPowerHotbarAbilities.Contains(SNOPower.Wizard_Archon) && !GilesUseTimer(SNOPower.Wizard_Archon) && Settings.Combat.Wizard.WaitArchon && ZetaDia.CurrentWorldId == 121214 &&
                (Vector3.Distance(playerStatus.CurrentPosition, new Vector3(711.25f, 716.25f, 80.13903f)) <= 40f || Vector3.Distance(playerStatus.CurrentPosition, new Vector3(546.8467f, 551.7733f, 1.576313f)) <= 40f))
            {
                DbHelper.Log(TrinityLogLevel.Normal, LogCategory.UserInformation, "Waiting for Wizard Archon cooldown before continuing to Azmodan.");
                CurrentTarget = new GilesObject()
                                    {
                                        Position = playerStatus.CurrentPosition,
                                        Type = GObjectType.Avoidance,
                                        Weight = 20000,
                                        CentreDistance = 2f,
                                        RadiusDistance = 2f,
                                        InternalName = "GilesWaitForArchon"
                                    };
            }
            // And a very sexy special check for WD BigBadVoodoo
            if (CurrentTarget == null && hashPowerHotbarAbilities.Contains(SNOPower.Witchdoctor_BigBadVoodoo) && !PowerManager.CanCast(SNOPower.Witchdoctor_BigBadVoodoo) && ZetaDia.CurrentWorldId == 121214 &&
                (Vector3.Distance(playerStatus.CurrentPosition, new Vector3(711.25f, 716.25f, 80.13903f)) <= 40f || Vector3.Distance(playerStatus.CurrentPosition, new Vector3(546.8467f, 551.7733f, 1.576313f)) <= 40f))
            {
                DbHelper.Log(TrinityLogLevel.Normal, LogCategory.UserInformation, "Waiting for WD BigBadVoodoo cooldown before continuing to Azmodan.");
                CurrentTarget = new GilesObject()
                                    {
                                        Position = playerStatus.CurrentPosition,
                                        Type = GObjectType.Avoidance,
                                        Weight = 20000,
                                        CentreDistance = 2f,
                                        RadiusDistance = 2f,
                                        InternalName = "GilesWaitForVoodooo"
                                    };
            }
        }
        private static void RefreshSetKiting(ref Vector3 vKitePointAvoid, bool NeedToKite, ref bool TryToKite)
        {
            TryToKite = false;

            var monsterList = from m in GilesObjectCache
                              where m.Type == GObjectType.Unit && 
                              m.RadiusDistance <= PlayerKiteDistance &&
                              (m.IsBossOrEliteRareUnique ||
                               ((m.HitPoints >= .15 || m.MonsterStyle != MonsterSize.Swarm) && !m.IsBossOrEliteRareUnique)
                               )
                              select m;

            if (CurrentTarget != null && CurrentTarget.Type == GObjectType.Unit && PlayerKiteDistance > 0 && CurrentTarget.RadiusDistance <= PlayerKiteDistance)
            {
                TryToKite = true;
                vKitePointAvoid = playerStatus.CurrentPosition;
            }

            if (monsterList.Count() > 0 && (playerStatus.ActorClass != ActorClass.Wizard || IsWizardShouldKite()))
            {
                TryToKite = true;
                vKitePointAvoid = playerStatus.CurrentPosition;
            }

            // Note that if treasure goblin level is set to kamikaze, even avoidance moves are disabled to reach the goblin!
            bool shouldKamikazeTreasureGoblins = (!bAnyTreasureGoblinsPresent || Settings.Combat.Misc.GoblinPriority <= GoblinPriority.Prioritize);

            double msCancelledEmergency = DateTime.Now.Subtract(timeCancelledEmergencyMove).TotalMilliseconds;
            bool shouldEmergencyMove = msCancelledEmergency >= cancelledEmergencyMoveForMilliseconds && NeedToKite;

            double msCancelledKite = DateTime.Now.Subtract(timeCancelledKiteMove).TotalMilliseconds;
            bool shouldKite = msCancelledKite >= cancelledKiteMoveForMilliseconds && TryToKite;

            if (shouldKamikazeTreasureGoblins && (shouldEmergencyMove || shouldKite))
            {
                Vector3 vAnySafePoint = FindSafeZone(false, 1, vKitePointAvoid, true, monsterList);

                // Ignore avoidance stuff if we're incapacitated or didn't find a safe spot we could reach
                if (vAnySafePoint != Vector3.Zero && vAnySafePoint.Distance(playerStatus.CurrentPosition) >= 1)
                {
                    if (Settings.Advanced.LogCategories.HasFlag(LogCategory.Moving))
                    {
                        DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.Moving, "Kiting to: {0} Distance: {1:0} Direction: {2:0}, Health%={3:0.00}, KiteDistance: {4:0}, Nearby Monsters: {5:0} NeedToKite: {6} TryToKite: {7}",
                            vAnySafePoint, vAnySafePoint.Distance(Me.Position), GetHeading(FindDirectionDegree(Me.Position, vAnySafePoint)),
                            playerStatus.CurrentHealthPct, PlayerKiteDistance, monsterList.Count(),
                            NeedToKite, TryToKite);
                    }
                    CurrentTarget = new GilesObject()
                                        {
                                            Position = vAnySafePoint,
                                            Type = GObjectType.Avoidance,
                                            Weight = 20000,
                                            CentreDistance = Vector3.Distance(playerStatus.CurrentPosition, vAnySafePoint),
                                            RadiusDistance = Vector3.Distance(playerStatus.CurrentPosition, vAnySafePoint),
                                            InternalName = "GilesKiting"
                                        };

                    timeCancelledKiteMove = DateTime.Now;
                    cancelledKiteMoveForMilliseconds = 750;

                    // Try forcing a target update with each kiting
                    //bForceTargetUpdate = true;
                }
                else
                {
                    // Didn't find any kiting we could reach, so don't look for any more kite spots for at least 1.5 seconds
                    timeCancelledKiteMove = DateTime.Now;
                    cancelledKiteMoveForMilliseconds = 500;
                }
            }
            else if (!shouldEmergencyMove && NeedToKite)
            {
                DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.Moving, "Emergency movement cancelled for {0:0}ms", DateTime.Now.Subtract(timeCancelledEmergencyMove).TotalMilliseconds);
            }
            else if (!shouldKite && TryToKite)
            {
                DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.Moving, "Kite movement cancelled for {0:0}ms", DateTime.Now.Subtract(timeCancelledKiteMove).TotalMilliseconds);
            }

        }
        public static string GetHeading(float heading)
        {
            var directions = new string[] {
                "n", "ne", "e", "se", "s", "sw", "w", "nw", "n"
            };

            var index = (((int)heading) + 23) / 45;
            return directions[index];
        }
        private static bool IsWizardShouldKite()
        {
            return (playerStatus.ActorClass == ActorClass.Wizard && (!Settings.Combat.Wizard.OnlyKiteInArchon || GilesHasBuff(SNOPower.Wizard_Archon)));
        }
    }
}
