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
            RefreshDiaObjectCache();
        }

        /// <summary>
        /// This method will add and update necessary information about all available actors. Determines GilesObjectType, sets ranges, updates blacklists, determines avoidance, kiting, target weighting
        /// and the result is we will have a new target for the Target Handler
        /// </summary>
        public static void RefreshDiaObjectCache()
        {
            //RefreshInit(out vSafePointNear, out vKitePointAvoid, out iCurrentTargetRactorGUID, out iUnitsSurrounding, out iHighestWeightFound, out listGilesObjectCache, out hashDoneThisRactor);
            RefreshCacheInit();
            // Now pull up all the data and store anything we want to handle in the super special cache list
            // Also use many cache dictionaries to minimize DB<->D3 memory hits, and speed everything up a lot
            RefreshCacheMainLoop();
            // Reduce ignore-for-loops counter
            if (iIgnoreThisForLoops > 0)
                iIgnoreThisForLoops--;
            // If we have an avoidance under our feet, then create a new object which contains a safety point to move to
            // But only if we aren't force-cancelling avoidance for XX time
            bool bFoundSafeSpot = false;
            // Note that if treasure goblin level is set to kamikaze, even avoidance moves are disabled to reach the goblin!
            if (bRequireAvoidance && (!bAnyTreasureGoblinsPresent || settings.iTreasureGoblinPriority <= 2) &&
                DateTime.Now.Subtract(timeCancelledEmergencyMove).TotalMilliseconds >= iMillisecondsCancelledEmergencyMoveFor)
            {
                Vector3 vAnySafePoint = FindSafeZone(false, 1, vSafePointNear);
                // Ignore avoidance stuff if we're incapacitated or didn't find a safe spot we could reach
                if (vAnySafePoint != vNullLocation)
                {
                    bFoundSafeSpot = true;
                    CurrentTarget = new GilesObject(vAnySafePoint, GilesObjectType.Avoidance, 20000, Vector3.Distance(playerStatus.vCurrentPosition, vAnySafePoint), Vector3.Distance(playerStatus.vCurrentPosition, vAnySafePoint), "GilesSafePoint");
                }
                else
                {
                    // Didn't find any safe spot we could reach, so don't look for any more safe spots for at least 2.8 seconds
                    iMillisecondsCancelledEmergencyMoveFor = 2800;
                    timeCancelledEmergencyMove = DateTime.Now;
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
                RefreshSetKiting(ref vKitePointAvoid, bNeedToKite, ref bShouldTryKiting);
            }
            // Not heading straight for a safe-spot?
            // No valid targets but we were told to stay put?
            if (CurrentTarget == null && bStayPutDuringAvoidance && !bRequireAvoidance)
            {
                CurrentTarget = new GilesObject(playerStatus.vCurrentPosition, GilesObjectType.Avoidance, 20000, 2f, 2f, "GilesStayPutPoint");
                Logging.WriteDiagnostic("Staying Put During Avoidance");
            }
            // Still no target, let's see if we should backtrack or wait for wrath to come off cooldown...
            if (CurrentTarget == null)
            {
                RefreshDoBackTrack();
            }
            // Still no target, let's end it all!
            if (CurrentTarget == null)
            {
                return;
            }
            // Ok record the time we last saw any unit at all
            if (CurrentTarget.GilesObjectType == GilesObjectType.Unit)
            {
                lastHadUnitInSights = DateTime.Now;
                // And record when we last saw any form of elite
                if (CurrentTarget.IsBoss || CurrentTarget.IsEliteRareUnique || CurrentTarget.bIsTreasureGoblin)
                    lastHadEliteUnitInSights = DateTime.Now;
            }
            // Record the last time our target changed etc.
            if (iCurrentTargetRactorGUID != CurrentTarget.iRActorGuid)
            {
                if (bDebugLogSpecial && settings.bDebugInfo)
                {
                    Logging.WriteDiagnostic("[Trinity] Setting dateSincePicked to {0} iCurrentTargetRactorGUID: {1} CurrentTarget.iRActorGuid: {2}",
                        DateTime.Now, iCurrentTargetRactorGUID, CurrentTarget.iRActorGuid);
                }
                dateSincePickedTarget = DateTime.Now;
                iTargetLastHealth = 0f;
            }
            else
            {
                // We're sticking to the same target, so update the target's health cache to check for stucks
                if (CurrentTarget.GilesObjectType == GilesObjectType.Unit)
                {
                    // Check if the health has changed, if so update the target-pick time before we blacklist them again
                    if (CurrentTarget.iHitPoints != iTargetLastHealth)
                    {
                        if (bDebugLogSpecial && settings.bDebugInfo)
                        {
                            Logging.WriteDiagnostic("[Trinity] Setting dateSincePicked to {0} CurrentTarget.iHitPoints: {1}  iTargetLastHealth: {2} ",
                                DateTime.Now, CurrentTarget.iHitPoints, iTargetLastHealth);
                        }
                        dateSincePickedTarget = DateTime.Now;
                    }
                    // Now store the target's last-known health
                    iTargetLastHealth = CurrentTarget.iHitPoints;
                }
            }
        }
        // Refresh object list from Diablo 3 memory RefreshDiaObjects()
        //private static void RefreshInit(out Vector3 vSafePointNear, out Vector3 vKitePointAvoid, out int iCurrentTargetRactorGUID, out int iUnitsSurrounding, out double iHighestWeightFound, out List<GilesObject> listGilesObjectCache, out HashSet<int> hashDoneThisRactor)
        private static void RefreshCacheInit()
        {
            // Update when we last refreshed with current time
            lastRefreshedObjects = DateTime.Now;

            // Blank current/last/next targets
            vSafePointNear = CurrentTarget != null ? CurrentTarget.vPosition : vNullLocation;
            vKitePointAvoid = vNullLocation;
            // store current target GUID
            iCurrentTargetRactorGUID = CurrentTarget != null ? CurrentTarget.iRActorGuid : -1;
            //reset current target
            CurrentTarget = null;
            // Reset all variables for target-weight finding
            bAnyTreasureGoblinsPresent = false;
            iCurrentMaxKillRadius = (float)(settings.iMonsterKillRange);
            //intell
            iCurrentMaxLootRadius = Zeta.CommonBot.Settings.CharacterSettings.Instance.LootRadius;
            bStayPutDuringAvoidance = false;
            // Set up the fake object for the target handler
            thisFakeObject = null;
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
            // Counter for how many cycles we extend or reduce our attack/kill radius, and our loot radius, after a last kill
            if (iKeepKillRadiusExtendedFor > 0)
                iKeepKillRadiusExtendedFor--;
            if (iKeepLootRadiusExtendedFor > 0)
                iKeepLootRadiusExtendedFor--;
            // Refresh buffs (so we can check for wrath being up to ignore ice balls and anything else like that)
            GilesRefreshBuffs();
            // Clear forcing close-range priority on mobs after XX period of time
            if (bForceCloseRangeTarget && DateTime.Now.Subtract(lastForcedKeepCloseRange).TotalMilliseconds > iMillisecondsForceCloseRange)
            {
                bForceCloseRangeTarget = false;
            }
            // Bunch of variables used throughout
            iUnitsSurrounding = 0;
            hashMonsterObstacleCache = new HashSet<GilesObstacle>();
            hashAvoidanceObstacleCache = new HashSet<GilesObstacle>();
            hashNavigationObstacleCache = new HashSet<GilesObstacle>();
            bAnyChampionsPresent = false;
            bAnyMobsInCloseRange = false;
            iLastDistance = 0f;
            bTravellingAvoidance = false;
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
            if (bNeedClearTemporaryBlacklist && DateTime.Now.Subtract(lastTemporaryBlacklist).TotalMilliseconds > 3000)
            {
                bNeedClearTemporaryBlacklist = false;
                hashRGuid3SecBlacklist = new HashSet<int>();
            }
            // Clear certain cache dictionaries sequentially, spaced out over time, to force data updates
            if (DateTime.Now.Subtract(lastClearedCacheDictionary).TotalMilliseconds >= 4000)
            {
                lastClearedCacheDictionary = DateTime.Now;
                iLastClearedCacheDictionary++;
                if (iLastClearedCacheDictionary > 5)
                    iLastClearedCacheDictionary = 1;
                switch (iLastClearedCacheDictionary)
                {
                    case 1:
                        dictGilesVectorCache = new Dictionary<int, Vector3>();
                        dictGilesObjectTypeCache = new Dictionary<int, GilesObjectType>();
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
            bRequireAvoidance = false;
            // Highest weight found as we progress through, so we can pick the best target at the end (the one with the highest weight)
            iHighestWeightFound = 0;
            // Here's the list we'll use to store each object
            listGilesObjectCache = new List<GilesObject>();
            hashDoneThisRactor = new HashSet<int>();
        }
        private static Stopwatch swTimeSinceLastDebug = new Stopwatch();

        private static HashSet<string> ignoreNames = new HashSet<string>
        {
            "MarkerLocation", "Generic_Proxy", "Hireling", "Barbarian","Barbarian", "Start_Location", "SphereTrigger", "Checkpoint", "ConductorProxyMaster", "BoxTrigger", "SavePoint",
        };

        private static void RefreshCacheMainLoop()
        {
            if (!swTimeSinceLastDebug.IsRunning)
                swTimeSinceLastDebug.Start();
            var refreshSource =
                from o in ZetaDia.Actors.GetActorsOfType<DiaObject>(true, false)
                where o.IsValid
                orderby o.ActorType, o.Distance
                select o;
            foreach (DiaObject currentObject in refreshSource)
            {
                try
                {
                    bool bWantThis = CacheDiaObject(currentObject);
                    //if (bDebugLogRefreshDiaObject && tmp_ThisGilesObjectType != GilesObjectType.Unknown && swTimeSinceLastDebug.ElapsedMilliseconds > 1000)
                    if (bDebugLogRefreshDiaObject && settings.bDebugInfo)
                    {
                        bool ignore = (from n in ignoreNames
                                       where c_sName.StartsWith(n)
                                       select true).FirstOrDefault();

                        if (!ignore)
                        {
                            Logging.WriteDiagnostic("[Trinity] Cache:"
                                + (bWantThis ? "Added  " : "Ignored")
                                + (!bWantThis ? (" By: " + (c_IgnoreReason != "None" ? c_IgnoreReason + "." : "") + c_IgnoreSubStep) : "")
                                + " Type: " + c_ObjectType
                                + " (" + c_diaObject.ActorType + ")"
                                + " Name: " + c_sName
                                + " (" + c_iActorSNO + ")"
                                + (c_unit_bIsBoss ? " isBossSNO" : "")
                                + " Dist2Mid: " + c_fCentreDistance.ToString("0")
                                + " Dist2Rad: " + c_fRadiusDistance.ToString("0")
                                + " ZDiff: " + c_fZDiff.ToString("0")
                                + " Radius: " + c_fRadius
                               );
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (bDebugLogSpecial && settings.bDebugInfo)
                    {
                        Logging.WriteDiagnostic("[Trinity] error while refreshing DiaObject ActorSNO: {0} Name: {1} Type: {2} Distance: {3:0}",
                            currentObject.ActorSNO, currentObject.Name, currentObject.ActorType, currentObject.Distance);
                        Logging.WriteDiagnostic(ex.Message);
                        Logging.WriteDiagnostic(ex.StackTrace);
                    }
                }
            }
            if (swTimeSinceLastDebug.ElapsedMilliseconds > 1000)
            {
                swTimeSinceLastDebug.Restart();
            }
        }

        private static void RefreshItemStats(GilesBaseItemType tempbasetype)
        {
            if (!_hashsetItemStatsLookedAt.Contains(c_iRActorGuid))
            {
                _hashsetItemStatsLookedAt.Add(c_iRActorGuid);
                if (tempbasetype == GilesBaseItemType.Armor || tempbasetype == GilesBaseItemType.WeaponOneHand || tempbasetype == GilesBaseItemType.WeaponTwoHand ||
                    tempbasetype == GilesBaseItemType.WeaponRange || tempbasetype == GilesBaseItemType.Jewelry || tempbasetype == GilesBaseItemType.FollowerItem ||
                    tempbasetype == GilesBaseItemType.Offhand)
                {
                    int iThisQuality;
                    ItemsDroppedStats.iTotal++;
                    if (c_item_tQuality >= ItemQuality.Legendary)
                        iThisQuality = QUALITYORANGE;
                    else if (c_item_tQuality >= ItemQuality.Rare4)
                        iThisQuality = QUALITYYELLOW;
                    else if (c_item_tQuality >= ItemQuality.Magic1)
                        iThisQuality = QUALITYBLUE;
                    else
                        iThisQuality = QUALITYWHITE;
                    ItemsDroppedStats.iTotalPerQuality[iThisQuality]++;
                    ItemsDroppedStats.iTotalPerLevel[c_item_iLevel]++;
                    ItemsDroppedStats.iTotalPerQPerL[iThisQuality, c_item_iLevel]++;
                }
                else if (tempbasetype == GilesBaseItemType.Gem)
                {
                    int iThisGemType = 0;
                    ItemsDroppedStats.iTotalGems++;
                    if (c_item_GilesItemType == GilesItemType.Topaz)
                        iThisGemType = GEMTOPAZ;
                    if (c_item_GilesItemType == GilesItemType.Ruby)
                        iThisGemType = GEMRUBY;
                    if (c_item_GilesItemType == GilesItemType.Emerald)
                        iThisGemType = GEMEMERALD;
                    if (c_item_GilesItemType == GilesItemType.Amethyst)
                        iThisGemType = GEMAMETHYST;
                    ItemsDroppedStats.iGemsPerType[iThisGemType]++;
                    ItemsDroppedStats.iGemsPerLevel[c_item_iLevel]++;
                    ItemsDroppedStats.iGemsPerTPerL[iThisGemType, c_item_iLevel]++;
                }
                else if (c_item_GilesItemType == GilesItemType.HealthPotion)
                {
                    ItemsDroppedStats.iTotalPotions++;
                    ItemsDroppedStats.iPotionsPerLevel[c_item_iLevel]++;
                }
                else if (c_item_GilesItemType == GilesItemType.InfernalKey)
                {
                    ItemsDroppedStats.iTotalInfernalKeys++;
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
            if (c_unit_bIsBoss)
            {
                if (c_iActorSNO != 80509)
                    // Kulle Exception
                    dUseKillRadius *= 1.5;
                // And even more if they're already injured
                if (c_unit_dHitPoints <= 0.98)
                    dUseKillRadius *= 4;
                // And make sure we have a MINIMUM range for bosses - incase they are at screen edge etc.
                if (dUseKillRadius <= 200)
                    if (c_iActorSNO != 80509)
                        // Kulle Exception
                        dUseKillRadius = 200;
            }
            // Special short-range list to ignore weakling mobs
            if (hashActorSNOShortRangeOnly.Contains(c_iActorSNO))
                dUseKillRadius = 12;
            // Prevent long-range mobs beign ignored while they may be pounding on us
            if (dUseKillRadius <= 30 && hashActorSNORanged.Contains(c_iActorSNO))
                dUseKillRadius = 30;
            //intell
            //GoatMutant_Ranged_A_Unique_Uber-10955 ActorSNO:	255996 	(act 1)
            //DuneDervish_B_Unique_Uber-14252 ActorSNO: 		256000	(act 2)
            //morluSpellcaster_A_Unique_Uber-17451 ActorSNO:	256015	(act 3)
            if (c_iActorSNO == 256015 || c_iActorSNO == 256000 || c_iActorSNO == 255996)
                dUseKillRadius = 80;
            // Injured treasure goblins get a huge extra radius - since they don't stay on the map long if injured, anyway!
            if (c_unit_bIsTreasureGoblin && (c_fCentreDistance <= 60 || c_unit_dHitPoints <= 0.99))
            {
                c_bForceLeapAgainst = true;
                if (settings.iTreasureGoblinPriority <= 2)
                    dUseKillRadius *= 2.5;
                else
                    dUseKillRadius *= 4;
                // Minimum distance of 60
                if (dUseKillRadius <= 60) dUseKillRadius = 60;
            }
            // Elitey type mobs and things
            else if ((c_unit_bIsElite || c_unit_bIsRare || c_unit_bIsUnique || c_unit_bIsMinion))
            {
                c_bForceLeapAgainst = true;
                if (c_unit_dHitPoints <= 0.99)
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
            else if (bGilesForcedVendoring)
            {
                if (dUseKillRadius <= 60) dUseKillRadius = 60;
                //intell
            }
            return dUseKillRadius;
        }
        private static MonsterAffixes RefreshAffixes(ACD tempCommonData)
        {
            MonsterAffixes theseaffixes;
            if (!dictGilesMonsterAffixCache.TryGetValue(c_iRActorGuid, out theseaffixes))
            {
                try
                {
                    theseaffixes = tempCommonData.MonsterAffixes;
                    dictGilesMonsterAffixCache.Add(c_iRActorGuid, theseaffixes);
                }
                catch
                {
                    theseaffixes = MonsterAffixes.None;
                }
            }
            c_unit_bIsElite = theseaffixes.HasFlag(MonsterAffixes.Elite);
            c_unit_bIsRare = theseaffixes.HasFlag(MonsterAffixes.Rare);
            c_unit_bIsUnique = theseaffixes.HasFlag(MonsterAffixes.Unique);
            c_unit_bIsMinion = theseaffixes.HasFlag(MonsterAffixes.Minion);
            return theseaffixes;
        }
        private static void RefreshCachedHealth(int iLastCheckedHealth, double dThisCurrentHealth, bool bHasCachedHealth)
        {
            if (!bHasCachedHealth)
            {
                dictGilesLastHealthCache.Add(c_iRActorGuid, dThisCurrentHealth);
                dictGilesLastHealthChecked.Add(c_iRActorGuid, iLastCheckedHealth);
            }
            else
            {
                dictGilesLastHealthCache[c_iRActorGuid] = dThisCurrentHealth;
                dictGilesLastHealthChecked[c_iRActorGuid] = iLastCheckedHealth;
            }
        }
        private static MonsterType RefreshMonsterType(ACD tempCommonData, MonsterType monsterType, bool bAddToDictionary)
        {
            SNORecordMonster monsterInfo = tempCommonData.MonsterInfo;
            if (monsterInfo != null)
            {
                // Force Jondar as an undead, since Diablo 3 sticks him as a permanent ally
                if (c_iActorSNO == 86624)
                {
                    monsterType = MonsterType.Undead;
                }
                else
                {
                    monsterType = monsterInfo.MonsterType;
                }
                // Is this going to be a new dictionary entry, or updating one already existing?
                if (bAddToDictionary)
                    dictionaryStoredMonsterTypes.Add(c_iActorSNO, monsterType);
                else
                    dictionaryStoredMonsterTypes[c_iActorSNO] = monsterType;
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
            if (DateTime.Now.Subtract(lastHadUnitInSights).TotalMilliseconds <= settings.iKillLootDelay && DateTime.Now.Subtract(lastHadEliteUnitInSights).TotalMilliseconds <= 10000)
            {
                CurrentTarget = new GilesObject(playerStatus.vCurrentPosition, GilesObjectType.Avoidance, 20000, 2f, 2f, "GilesWaitForLootDrops");
            }
            // Now see if we need to do any backtracking
            if (CurrentTarget == null && iTotalBacktracks >= 2 && settings.bEnableBacktracking && !playerStatus.bIsInTown)
            // Never bother with the 1st backtrack position nor if we are in town
            {
                // See if we're already within 18 feet of our start position first
                if (Vector3.Distance(playerStatus.vCurrentPosition, vBacktrackList[1]) <= 18f)
                {
                    vBacktrackList = new SortedList<int, Vector3>();
                    iTotalBacktracks = 0;
                }
                // See if we can raytrace to the final location and it's within 25 feet
                if (iTotalBacktracks >= 2 && Vector3.Distance(playerStatus.vCurrentPosition, vBacktrackList[1]) <= 25f &&
                    GilesCanRayCast(playerStatus.vCurrentPosition, vBacktrackList[1], NavCellFlags.AllowWalk))
                {
                    vBacktrackList = new SortedList<int, Vector3>();
                    iTotalBacktracks = 0;
                }
                if (iTotalBacktracks >= 2)
                {
                    // See if we can skip to the next backtracker location first
                    if (iTotalBacktracks >= 3)
                    {
                        if (Vector3.Distance(playerStatus.vCurrentPosition, vBacktrackList[iTotalBacktracks - 1]) <= 10f)
                        {
                            vBacktrackList.Remove(iTotalBacktracks);
                            iTotalBacktracks--;
                        }
                    }
                    CurrentTarget = new GilesObject(vBacktrackList[iTotalBacktracks], GilesObjectType.Backtrack, 20000, Vector3.Distance(playerStatus.vCurrentPosition, vBacktrackList[iTotalBacktracks]), Vector3.Distance(playerStatus.vCurrentPosition, vBacktrackList[iTotalBacktracks]), "GilesBacktrack");
                }
            }
            else
            {
                vBacktrackList = new SortedList<int, Vector3>();
                iTotalBacktracks = 0;
            }
            // End of backtracking check
            // Finally, a special check for waiting for wrath of the berserker cooldown before engaging Azmodan
            if (CurrentTarget == null && hashPowerHotbarAbilities.Contains(SNOPower.Barbarian_WrathOfTheBerserker) && settings.bWaitForWrath && !GilesUseTimer(SNOPower.Barbarian_WrathOfTheBerserker) &&
                ZetaDia.CurrentWorldId == 121214 &&
                (Vector3.Distance(playerStatus.vCurrentPosition, new Vector3(711.25f, 716.25f, 80.13903f)) <= 40f || Vector3.Distance(playerStatus.vCurrentPosition, new Vector3(546.8467f, 551.7733f, 1.576313f)) <= 40f))
            {
                bDontSpamOutofCombat = true;
                Logging.Write("[Trinity] Waiting for Wrath Of The Berserker cooldown before continuing to Azmodan.");
                CurrentTarget = new GilesObject(playerStatus.vCurrentPosition, GilesObjectType.Avoidance, 20000, 2f, 2f, "GilesWaitForWrath");
            }
            // And a special check for wizard archon
            if (CurrentTarget == null && hashPowerHotbarAbilities.Contains(SNOPower.Wizard_Archon) && !GilesUseTimer(SNOPower.Wizard_Archon) && settings.bWaitForArchon && ZetaDia.CurrentWorldId == 121214 &&
                (Vector3.Distance(playerStatus.vCurrentPosition, new Vector3(711.25f, 716.25f, 80.13903f)) <= 40f || Vector3.Distance(playerStatus.vCurrentPosition, new Vector3(546.8467f, 551.7733f, 1.576313f)) <= 40f))
            {
                Logging.Write("[Trinity] Waiting for Wizard Archon cooldown before continuing to Azmodan.");
                CurrentTarget = new GilesObject(playerStatus.vCurrentPosition, GilesObjectType.Avoidance, 20000, 2f, 2f, "GilesWaitForArchon");
            }
            // And a very sexy special check for WD BigBadVoodoo
            if (CurrentTarget == null && hashPowerHotbarAbilities.Contains(SNOPower.Witchdoctor_BigBadVoodoo) && !PowerManager.CanCast(SNOPower.Witchdoctor_BigBadVoodoo) && ZetaDia.CurrentWorldId == 121214 &&
                (Vector3.Distance(playerStatus.vCurrentPosition, new Vector3(711.25f, 716.25f, 80.13903f)) <= 40f || Vector3.Distance(playerStatus.vCurrentPosition, new Vector3(546.8467f, 551.7733f, 1.576313f)) <= 40f))
            {
                Logging.Write("[Trinity] Waiting for WD BigBadVoodoo cooldown before continuing to Azmodan.");
                CurrentTarget = new GilesObject(playerStatus.vCurrentPosition, GilesObjectType.Avoidance, 20000, 2f, 2f, "GilesWaitForVoodooo");
            }
        }
        private static void RefreshSetKiting(ref Vector3 vKitePointAvoid, bool bNeedToKite, ref bool bShouldTryKiting)
        {
            bShouldTryKiting = false;
            if (
                (((CurrentTarget != null && CurrentTarget.GilesObjectType == GilesObjectType.Unit && iKiteDistance > 0 && CurrentTarget.fRadiusDistance <= iKiteDistance) ||
                hashMonsterObstacleCache.Any(m => m.vThisLocation.Distance(playerStatus.vCurrentPosition) <= iKiteDistance)) &&
                (iMyCachedActorClass != ActorClass.Wizard || IsWizardShouldKite())) || playerStatus.dCurrentHealthPct <= 0.15
                )
            {
                bShouldTryKiting = true;

                // lets try this... 
                vKitePointAvoid = playerStatus.vCurrentPosition;
            }
            // Note that if treasure goblin level is set to kamikaze, even avoidance moves are disabled to reach the goblin!
            if ((bShouldTryKiting || bNeedToKite) && (!bAnyTreasureGoblinsPresent || settings.iTreasureGoblinPriority <= 2) &&
                DateTime.Now.Subtract(timeCancelledEmergencyMove).TotalMilliseconds >= iMillisecondsCancelledEmergencyMoveFor &&
                (DateTime.Now.Subtract(timeCancelledKiteMove).TotalMilliseconds >= iMillisecondsCancelledKiteMoveFor ||
                (DateTime.Now.Subtract(timeCancelledKiteMove).TotalMilliseconds >= 2500 && bNeedToKite)))
            {
                Vector3 vAnySafePoint = FindSafeZone(false, 1, vKitePointAvoid, true);
                // Ignore avoidance stuff if we're incapacitated or didn't find a safe spot we could reach
                if (vAnySafePoint != vNullLocation)
                {
                    if (bDebugLogSpecial)
                    {
                        Logging.Write("[Trinity] Kiting to: {0} Distance: {1:0} Direction: {2:0} ", vAnySafePoint, vAnySafePoint.Distance(Me.Position), FindDirectionDegree(Me.Position, vAnySafePoint));
                    }
                    CurrentTarget = new GilesObject(vAnySafePoint, GilesObjectType.Avoidance, 20000, Vector3.Distance(playerStatus.vCurrentPosition, vAnySafePoint), Vector3.Distance(playerStatus.vCurrentPosition, vAnySafePoint), "GilesKiting");
                    timeCancelledKiteMove = DateTime.Today;
                    iMillisecondsCancelledKiteMoveFor = 5000;

                    // Try forcing a target update with each kiting
                    //bForceTargetUpdate = true;
                }
                else
                {
                    // Didn't find any kiting we could reach, so don't look for any more kite spots for at least 1.5 seconds
                    timeCancelledKiteMove = DateTime.Today;
                    iMillisecondsCancelledKiteMoveFor = 2500;
                }
            }
        }
        private static bool IsWizardShouldKite()
        {
            return (iMyCachedActorClass == ActorClass.Wizard && (!settings.bKiteOnlyArchon || GilesHasBuff(SNOPower.Wizard_Archon)));
        }
    }
}
