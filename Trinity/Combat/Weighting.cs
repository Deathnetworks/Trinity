using System;
using System.Linq;
using Trinity.Cache;
using Trinity.Combat.Abilities;
using Trinity.Config.Combat;
using Trinity.Configuration;
using Trinity.DbProvider;
using Trinity.Helpers;
using Trinity.Items;
using Trinity.Reference;
using Trinity.Technicals;
using Zeta.Bot;
using Zeta.Bot.Navigation;
using Zeta.Bot.Profile.Common;
using Zeta.Bot.Settings;
using Zeta.Common;
using Zeta.Game;
using Zeta.Game.Internals.Actors;
using Zeta.Game.Internals.SNO;
using Logger = Trinity.Technicals.Logger;

namespace Trinity
{
    public partial class Trinity
    {
        private static void RefreshDiaGetWeights()
        {
            #region RefreshDiaObjectCache.AvoidanceCheck
            using (new PerformanceLogger("RefreshDiaObjectCache.AvoidanceCheck"))
            {
                if (Settings.Combat.Misc.FleeInGhostMode && Player.IsGhosted)
                    Trinity.Player.StandingInAvoidance = true;

                // Note that if treasure goblin level is set to kamikaze, even avoidance moves are disabled to reach the goblin!
                if (Trinity.Player.StandingInAvoidance || Player.AvoidDeath && (!AnyTreasureGoblinsPresent || Settings.Combat.Misc.GoblinPriority <= GoblinPriority.Prioritize) &&
                    !Combat.QueuedMovementManager.Stuck.IsStuck())
                {
                    var _safeNode = GridMap.GetBestMoveNode();
                    if (_safeNode != null)
                    {
                        PlayerMover.UsedSpecialMovement(_safeNode.Position);

                        Logger.Log(TrinityLogLevel.Info, LogCategory.Avoidance, "Kiting Avoidance: {0} Distance: {1:0} Direction: {2:0}, Health%={3:0.00}, KiteDistance: {4:0}",
                            _safeNode.Position, _safeNode.Position.Distance(Player.Position), MathUtil.GetHeading(MathUtil.FindDirectionDegree(Player.Position, _safeNode.Position)),
                            Player.CurrentHealthPct, CombatBase.KiteDistance);

                        CurrentTarget = new TrinityCacheObject()
                        {
                            Position = _safeNode.Position,
                            Type = GObjectType.Avoidance,
                            Weight = _safeNode.Weight,
                            Radius = 2f,
                            InternalName = "AvoidancePoint"
                        };
                    }
                }
            }
            #endregion

            if (CurrentTarget != null && CurrentTarget.IsAvoidance && Trinity.Player.StandingInAvoidance)
                return;

            #region RefreshDiaObjectCache.Weighting
            using (new PerformanceLogger("RefreshDiaObjectCache.Weighting"))
            {
                double movementSpeed = PlayerMover.GetMovementSpeed();

                int eliteCount = CombatBase.IgnoringElites ? 0 : ObjectCache.Count(u => u.IsUnit && u.IsBossOrEliteRareUnique);
                int avoidanceCount = Settings.Combat.Misc.AvoidAOE ? 0 : ObjectCache.Count(o => o.Type == GObjectType.Avoidance && o.Distance <= 65f);

                bool avoidanceNearby = Settings.Combat.Misc.AvoidAOE && ObjectCache.Any(o => o.Type == GObjectType.Avoidance && o.Distance <= 20f);

                bool prioritizeCloseRangeUnits = (avoidanceNearby || _forceCloseRangeTarget || Player.IsRooted || DateTime.UtcNow.Subtract(PlayerMover.LastRecordedAnyStuck).TotalMilliseconds < 1000 &&
                                                  ObjectCache.Count(u => u.IsUnit && u.RadiusDistance < 10f) >= 3);

                bool hiPriorityHealthGlobes = Settings.Combat.Misc.HiPriorityHG;

                bool healthGlobeEmergency = (Player.CurrentHealthPct <= CombatBase.EmergencyHealthGlobeLimit || Player.PrimaryResourcePct <= CombatBase.HealthGlobeResource) &&
                                            ObjectCache.Any(g => g.Type == GObjectType.HealthGlobe) && hiPriorityHealthGlobes;

                bool collectHealthGlobeForRessource =
                    CombatBase.IsCombatAllowed &&
                    Player.PrimaryResourcePct < CombatBase.HealthGlobeResource &&
                    (Legendary.ReapersWraps.IsEquipped ||
                    (Player.ActorClass == ActorClass.Witchdoctor && CacheData.Hotbar.PassiveSkills.Contains(SNOPower.Witchdoctor_Passive_GruesomeFeast)) ||
                    (Player.ActorClass == ActorClass.DemonHunter && CacheData.Hotbar.PassiveSkills.Contains(SNOPower.DemonHunter_Passive_Vengeance)));

                bool hiPriorityShrine = Settings.WorldObject.HiPriorityShrines;

                bool getHiPriorityShrine = ObjectCache.Any(s => s.Type == GObjectType.Shrine) && hiPriorityShrine;

                bool getHiPriorityContainer = Settings.WorldObject.HiPriorityContainers && ObjectCache.Any(c => c.Type == GObjectType.Container) &&
                                              !(Legendary.HarringtonWaistguard.IsEquipped && Legendary.HarringtonWaistguard.IsBuffActive);

                bool profileTagCheck = false;

                string behaviorName = "";
                if (ProfileManager.CurrentProfileBehavior != null)
                {
                    Type behaviorType = ProfileManager.CurrentProfileBehavior.GetType();
                    behaviorName = behaviorType.Name;
                    if (!Settings.Combat.Misc.ProfileTagOverride && CombatBase.IsQuestingMode ||
                        behaviorType == typeof(WaitTimerTag) ||
                        behaviorType == typeof(UseTownPortalTag) ||
                        behaviorName.ToLower().Contains("townrun") ||
                        behaviorName.ToLower().Contains("townportal"))
                    {
                        profileTagCheck = true;
                    }
                }

                bool isKillBounty =
                    !Player.ParticipatingInTieredLootRun &&
                    Player.ActiveBounty != null &&
                    Player.ActiveBounty.Info.KillCount > 0;

                bool shouldIgnoreElites =
                    (!(isKillBounty || Player.InActiveEvent) &&
                     !CombatBase.IsQuestingMode &&
                     !DataDictionary.RiftWorldIds.Contains(Player.WorldID) &&
                     !DataDictionary.QuestLevelAreaIds.Contains(Player.LevelAreaId) &&
                     !profileTagCheck &&
                     !TownRun.IsTryingToTownPortal() &&
                     CombatBase.IgnoringElites);

                Logger.Log(TrinityLogLevel.Debug, LogCategory.Weight,
                    "Starting weights: packSize={0} packRadius={1} MovementSpeed={2:0.0} Elites={3} AoEs={4} disableIgnoreTag={5} ({6}) closeRangePriority={7} townRun={8} questingArea={9} level={10} isQuestingMode={11} healthGlobeEmerg={12} hiPriHG={13} hiPriShrine={14}",
                    Settings.Combat.Misc.TrashPackSize, Settings.Combat.Misc.TrashPackClusterRadius, movementSpeed, eliteCount, avoidanceCount, profileTagCheck, behaviorName,
                    prioritizeCloseRangeUnits, TownRun.IsTryingToTownPortal(), DataDictionary.QuestLevelAreaIds.Contains(Player.LevelAreaId), Player.Level, CombatBase.IsQuestingMode, healthGlobeEmergency, hiPriorityHealthGlobes, hiPriorityShrine);

                bool inQuestArea = DataDictionary.QuestLevelAreaIds.Contains(Player.LevelAreaId);
                bool usingTownPortal = TownRun.IsTryingToTownPortal();

                bool shouldIgnoreTrashMobs =
                    (!(isKillBounty || Player.InActiveEvent) &&
                     !CombatBase.IsQuestingMode &&
                     !inQuestArea &&
                     Player.TieredLootRunlevel != 0 && // Rift Trials
                     !usingTownPortal &&
                     !profileTagCheck &&
                     movementSpeed >= 1 &&
                     Settings.Combat.Misc.TrashPackSize > 1 &&
                     Player.Level >= 15 &&
                     Player.CurrentHealthPct > 0.10 &&
                     DateTime.UtcNow.Subtract(PlayerMover.LastRecordedAnyStuck).TotalMilliseconds > 500);

                bool shouldIgnoreBosses = healthGlobeEmergency || getHiPriorityShrine || getHiPriorityContainer;

                // Highest weight found as we progress through, so we can pick the best target at the end (the one with the highest weight)
                HighestWeightFound = 0;

                foreach (TrinityCacheObject cacheObject in ObjectCache.OrderBy(c => c.Distance))
                {
                    string objWeightInfo = "";
                    cacheObject.Weight = 0d;

                    // Now do different calculations based on the object type
                    switch (cacheObject.Type)
                    {
                        // Weight Units
                        case GObjectType.Unit:
                            {
                                bool goblinKamikaze = cacheObject.IsTreasureGoblin && Settings.Combat.Misc.GoblinPriority == GoblinPriority.Kamikaze;

                                bool isInHotSpot = GroupHotSpots.CacheObjectIsInHotSpot(cacheObject);

                                bool ignoring = false;

                                if (cacheObject.IsTrashMob)
                                {
                                    bool elitesInRangeOfUnit = !CombatBase.IgnoringElites &&
                                        ObjectCache.Any(u => u.ACDGuid != cacheObject.ACDGuid && u.IsEliteRareUnique && u.Position.Distance2D(cacheObject.Position) <= 15f);

                                    int nearbyMonsterCount = cacheObject.NearbyUnits;

                                    bool shouldIgnoreTrashMob = shouldIgnoreTrashMobs && nearbyMonsterCount < Settings.Combat.Misc.TrashPackSize && !elitesInRangeOfUnit;

                                    // Ignore trash mobs < 15% health or 50% health with a DoT
                                    if (cacheObject.IsTrashMob && shouldIgnoreTrashMob &&
                                        (cacheObject.HitPointsPct < Settings.Combat.Misc.IgnoreTrashBelowHealth ||
                                         cacheObject.HitPointsPct < Settings.Combat.Misc.IgnoreTrashBelowHealthDoT && cacheObject.HasDotDPS) && !cacheObject.IsQuestMonster && !cacheObject.IsMinimapActive)
                                    {
                                        objWeightInfo += "Ignoring Health/DoT ";
                                        ignoring = true;
                                    }

                                    bool ignoreSummoner = cacheObject.IsSummoner && !Settings.Combat.Misc.ForceKillSummoners || !cacheObject.IsNavigable;

                                    // Ignore Solitary Trash mobs (no elites present)
                                    // Except if has been primary target or if already low on health (<= 20%)
                                    if ((shouldIgnoreTrashMob && !isInHotSpot &&
                                        !cacheObject.IsQuestMonster && !cacheObject.IsMinimapActive && !ignoreSummoner &&
                                        !cacheObject.IsBountyObjective) || healthGlobeEmergency || getHiPriorityContainer || getHiPriorityShrine || goblinKamikaze)
                                    {
                                        objWeightInfo += "Ignoring ";
                                        ignoring = true;
                                    }
                                    else if (Player.ActorClass == ActorClass.Barbarian && Sets.TheLegacyOfRaekor.IsMaxBonusActive &&
                                        !TownRun.IsTryingToTownPortal() &&
                                        Skills.Barbarian.FuriousCharge.IsActive && !(cacheObject.IsPlayerFacing(20f) && cacheObject.Distance < cacheObject.Radius) &&
                                        (TargetUtil.NumMobsInRangeOfPosition(cacheObject.Position, 40f) <= 1 || cacheObject.CountFCObjectsInFront() < 3 || !cacheObject.IsInLineOfSight))
                                    {
                                        objWeightInfo += "BarbIgnoring ";
                                        ignoring = true;
                                    }
                                    else
                                    {
                                        objWeightInfo += "Adding ";
                                    }
                                    objWeightInfo += String.Format("ShouldIgnore={3} nearbyCount={0} radiusDistance={1:0} hotspot={2} elitesInRange={4} hitPointsPc={5:0.0} summoner={6} quest={7} minimap={8} bounty={9} ",
                                        nearbyMonsterCount, cacheObject.RadiusDistance, isInHotSpot, shouldIgnoreTrashMob, elitesInRangeOfUnit, cacheObject.HitPointsPct, ignoreSummoner, cacheObject.IsQuestMonster, cacheObject.IsMinimapActive, cacheObject.IsBountyObjective);

                                    if (ignoring)
                                        break;
                                }

                                if (cacheObject.IsEliteRareUnique)
                                {
                                    // Ignore elite option, except if trying to town portal
                                    if ((!cacheObject.IsBoss || shouldIgnoreBosses) && !cacheObject.IsBountyObjective &&
                                        shouldIgnoreElites && cacheObject.IsEliteRareUnique && !isInHotSpot &&
                                        !(cacheObject.HitPointsPct <= (Settings.Combat.Misc.ForceKillElitesHealth / 100))
                                        || healthGlobeEmergency || getHiPriorityShrine || getHiPriorityContainer || goblinKamikaze)
                                    {
                                        objWeightInfo += "Ignoring ";
                                        ignoring = true;
                                    }
                                    else if (cacheObject.IsEliteRareUnique && !shouldIgnoreElites)
                                    {
                                        objWeightInfo += "Adding ";
                                    }
                                    objWeightInfo += String.Format("shouldIgnore={0} hitPointsPct={1:0} ", shouldIgnoreElites, cacheObject.HitPointsPct * 100);

                                    if (ignoring)
                                        break;
                                }

                                if (Player.InActiveEvent && ObjectCache.Any(o => o.IsEventObject))
                                {
                                    Vector3 eventObjectPosition = ObjectCache.FirstOrDefault(o => o.IsEventObject).Position;

                                    if (!cacheObject.IsQuestMonster && cacheObject.Position.Distance2DSqr(eventObjectPosition) > 75 * 75)
                                    {
                                        objWeightInfo += "TooFarFromEvent ";
                                        cacheObject.Weight = 0;
                                        break;
                                    }
                                }

                                // Monster is in cache but not within kill range
                                if (!Player.IsRanged && !cacheObject.IsBoss && !cacheObject.IsTreasureGoblin && LastTargetRactorGUID != cacheObject.RActorGuid &&
                                    cacheObject.RadiusDistance > cacheObject.KillRange &&
                                    !cacheObject.IsQuestMonster &&
                                    !cacheObject.IsBountyObjective)
                                {
                                    objWeightInfo += "KillRange ";
                                    break;
                                }

                                if (cacheObject.HitPointsPct <= 0)
                                {
                                    objWeightInfo += "Dead ";
                                    cacheObject.Weight = 0;
                                    break;
                                }

                                if (Player.IsRanged && !cacheObject.IsInLineOfSight)
                                {
                                    objWeightInfo += "NotInLoS ";
                                    cacheObject.Weight = 0;
                                    break;
                                }

                                // Goblin priority KAMIKAZE
                                if (cacheObject.IsTreasureGoblin && Settings.Combat.Misc.GoblinPriority == GoblinPriority.Kamikaze)
                                {
                                    objWeightInfo += "GoblinKamikaze ";
                                    cacheObject.Weight = ((90f - cacheObject.RadiusDistance) / 90f) * MaxWeight * 2;

                                    break;
                                }

                                // Force a close range target kite needed
                                else if (CombatBase.KiteDistance > 0)
                                {
                                    cacheObject.Weight = ((90f - cacheObject.RadiusDistance) / 90f) * MaxWeight * 1.5;
                                    objWeightInfo += "CloseRangeForKite ";

                                    if (cacheObject.RadiusDistance < (CombatBase.KiteDistance * 0.5))
                                        prioritizeCloseRangeUnits = true;

                                    break;
                                }

                                // Low health
                                if (cacheObject.HitPointsPct > 0.001 && cacheObject.HitPointsPct <= 0.05)
                                {
                                    cacheObject.Weight += ((90f - cacheObject.RadiusDistance) / 90f) * 1000d;
                                    objWeightInfo += "LowHealth ";
                                }

                                // Force a close range target because we seem to be stuck *OR* if not ranged and currently rooted
                                if (prioritizeCloseRangeUnits)
                                {
                                    double rangePercent = (20d - cacheObject.RadiusDistance) / 20d;

                                    cacheObject.Weight = ((90f - cacheObject.RadiusDistance) / 90f) * MaxWeight * 1.5;
                                    objWeightInfo += "CloseRange ";

                                    break;
                                }
                                else
                                {
                                    // Not attackable, could be shielded, make super low priority
                                    if (cacheObject.HasAffixShielded && cacheObject.Unit != default(DiaUnit) && cacheObject.Unit.IsInvulnerable)
                                    {
                                        // Only 100 weight helps prevent it being prioritized over an unshielded
                                        cacheObject.Weight = 0;
                                    }
                                    // Not forcing close-ranged targets from being stuck, so let's calculate a weight!
                                    else
                                    {
                                        // Elites/Bosses that are killed should have weight erased so we don't keep attacking
                                        if ((cacheObject.IsEliteRareUnique || cacheObject.IsBoss) && cacheObject.HitPointsPct <= 0)
                                        {
                                            objWeightInfo += "EliteHitPoints0";
                                            cacheObject.Weight = 0;
                                            break;
                                        }

                                        // Starting weight of 500
                                        if (cacheObject.IsTrashMob)
                                        {
                                            objWeightInfo += "IsTrash ";
                                            cacheObject.Weight = Math.Max((cacheObject.KillRange - cacheObject.RadiusDistance) / cacheObject.KillRange * 100d, 2d);
                                        }

                                        // Elite Weight based on kill range and max possible weight
                                        if (cacheObject.IsBossOrEliteRareUnique)
                                        {
                                            objWeightInfo += "IsBossOrEliteRareUnique ";
                                            cacheObject.Weight = Math.Max((cacheObject.KillRange - cacheObject.RadiusDistance) / cacheObject.KillRange * MaxWeight, 2000d);
                                        }

                                        // Bounty Objectives goooo
                                        if (cacheObject.IsBountyObjective && !!cacheObject.IsNavigable)
                                        {
                                            objWeightInfo += "BountyObjective ";
                                            cacheObject.Weight += 15000d;
                                        }

                                        // set a minimum 100 just to make sure it's not 0
                                        if ((isKillBounty || Player.InActiveEvent))
                                        {
                                            objWeightInfo += "InActiveEvent ";
                                            cacheObject.Weight += 100;
                                        }
                                        // Elites with Archon get super weight
                                        if (!CombatBase.IgnoringElites && Player.ActorClass == ActorClass.Wizard && GetHasBuff(SNOPower.Wizard_Archon) && cacheObject.IsBossOrEliteRareUnique)
                                        {
                                            objWeightInfo += "ArchonElite ";
                                            cacheObject.Weight += 10000d;
                                        }

                                        // Monsters near players given higher weight
                                        if (cacheObject.Weight > 0)
                                        {
                                            var group = 0.0;
                                            foreach (var player in ObjectCache.Where(p => p.Type == GObjectType.Player && p.ACDGuid != Player.ACDGuid))
                                            {
                                                group += Math.Max(((55f - cacheObject.Position.Distance2D(player.Position)) / 55f * 500d), 2d);
                                            }
                                            if (group > 100.0)
                                            {
                                                objWeightInfo += string.Format("group{0:0} ", group);
                                            }
                                            cacheObject.Weight += group;
                                        }

                                        // Is standing in HotSpot - focus fire!
                                        if (isInHotSpot)
                                        {
                                            objWeightInfo += "HotSpot ";
                                            cacheObject.Weight += 10000d;
                                        }

                                        // Below actions only when not prioritizing close range units
                                        if (!prioritizeCloseRangeUnits)
                                        {
                                            // Give extra weight to ranged enemies
                                            if ((Player.ActorClass == ActorClass.Barbarian || Player.ActorClass == ActorClass.Monk) &&
                                                (cacheObject.MonsterSize == MonsterSize.Ranged || DataDictionary.RangedMonsterIds.Contains(c_CacheObject.ActorSNO)))
                                            {
                                                objWeightInfo += "Ranged ";
                                                cacheObject.Weight += 1100d;
                                                cacheObject.ForceLeapAgainst = true;
                                            }

                                            // Lower health gives higher weight - health is worth up to 1000ish extra weight
                                            if (cacheObject.IsTrashMob && cacheObject.HitPointsPct < 0.20 && cacheObject.HitPointsPct > 0.01)
                                            {
                                                objWeightInfo += "LowHPTrash ";
                                                cacheObject.Weight += Math.Max((1 - cacheObject.HitPointsPct) / 100 * 1000d, 100d);
                                            }

                                            // Elites on low health get extra priority - up to 2500ish
                                            if (cacheObject.IsEliteRareUnique && cacheObject.HitPointsPct < 0.25 && cacheObject.HitPointsPct > 0.01)
                                            {
                                                objWeightInfo += "LowHPElite ";
                                                cacheObject.Weight += Math.Max((1 - cacheObject.HitPointsPct) / 100 * 2500d, 100d);
                                            }

                                            // Bonuses to priority type monsters from the dictionary/hashlist set at the top of the code
                                            int extraPriority;
                                            if (DataDictionary.MonsterCustomWeights.TryGetValue(cacheObject.ActorSNO, out extraPriority))
                                            {
                                                // adding a constant multiple of 3 to all weights here (e.g. 999 becomes 1998)
                                                objWeightInfo += "XtraPriority ";
                                                cacheObject.Weight += extraPriority * 2d;
                                            }

                                            // Extra weight for summoners
                                            if (!cacheObject.IsBoss && cacheObject.IsSummoner)
                                            {
                                                objWeightInfo += "Summoner ";
                                                cacheObject.Weight += 2500;
                                            }

                                            // Was already a target and is still viable, give it some free extra weight, to help stop flip-flopping between two targets
                                            if (cacheObject.RActorGuid == LastTargetRactorGUID && cacheObject.Distance <= 25f)
                                            {
                                                objWeightInfo += "LastTarget ";
                                                cacheObject.Weight += 1000d;
                                            }
                                        }

                                        // Close range get higher weights the more of them there are, to prevent body-blocking
                                        if (!cacheObject.IsBoss && cacheObject.RadiusDistance <= 10f)
                                        {
                                            objWeightInfo += "CloseRange10f ";
                                            cacheObject.Weight += (3000d * cacheObject.Radius);
                                        }

                                        // Special additional weight for corrupt growths in act 4 ONLY if they are at close range (not a standard priority thing)
                                        if ((cacheObject.ActorSNO == 210120 || cacheObject.ActorSNO == 210268) && cacheObject.Distance <= 25f)
                                        {
                                            objWeightInfo += "CorruptGrowth ";
                                            cacheObject.Weight += 5000d;
                                        }

                                        // Prevent going less than 300 yet to prevent annoyances (should only lose this much weight from priority reductions in priority list?)
                                        if (cacheObject.Weight < 100)
                                        {
                                            objWeightInfo += "MinWeight ";
                                            cacheObject.Weight = 100d;
                                        }

                                        // If standing Molten, Arcane, or Poison Tree near unit, reduce weight
                                        if (CombatBase.KiteDistance <= 0 &&
                                            CacheData.AvoidanceObstacles.Any(aoe =>
                                            (aoe.AvoidanceType == AvoidanceType.Arcane ||
                                            aoe.AvoidanceType == AvoidanceType.MoltenCore ||
                                                //aoe.AvoidanceType == AvoidanceType.MoltenTrail ||
                                            aoe.AvoidanceType == AvoidanceType.PoisonTree) &&
                                            cacheObject.Position.Distance2D(aoe.Position) <= aoe.Radius))
                                        {
                                            objWeightInfo += "InSpecialAoE ";
                                            cacheObject.Weight *= 0.2;
                                        }

                                        // If any AoE between us and target, reduce weight, for melee only
                                        if (!KillMonstersInAoE &&
                                            CombatBase.KiteDistance <= 0 && cacheObject.RadiusDistance > 3f &&
                                            CacheData.AvoidanceObstacles.Any(aoe => aoe.AvoidanceType != AvoidanceType.PlagueCloud &&
                                                !aoe.IsAvoidanceAnimations && 
                                                MathUtil.IntersectsPath(aoe.Position, aoe.Radius, Player.Position, cacheObject.Position)))
                                        {
                                            objWeightInfo += "AoEPathLine ";
                                            cacheObject.Weight = 1;
                                        }
                                        // See if there's any AOE avoidance in that spot, if so reduce the weight to 1, for melee only
                                        if (!KillMonstersInAoE &&
                                            CombatBase.KiteDistance <= 0 && cacheObject.RadiusDistance > 3f &&
                                            CacheData.AvoidanceObstacles.Any(aoe => aoe.AvoidanceType != AvoidanceType.PlagueCloud &&
                                                !aoe.IsAvoidanceAnimations && 
                                                cacheObject.Position.Distance2D(aoe.Position) <= aoe.Radius))
                                        {
                                            objWeightInfo += "InAoE ";
                                            cacheObject.Weight = 1d;
                                        }

                                        // If any AoE between us and target, reduce weight, for melee only
                                        if (KillMonstersInAoE &&
                                            CombatBase.KiteDistance <= 0 && cacheObject.RadiusDistance > 3f &&
                                            CacheData.AvoidanceObstacles.Any(aoe => aoe.AvoidanceType != AvoidanceType.PlagueCloud &&
                                                !aoe.IsAvoidanceAnimations &&
                                                MathUtil.IntersectsPath(aoe.Position, aoe.Radius, Player.Position, cacheObject.Position)))
                                        {
                                            objWeightInfo += "AoEPathLine ";
                                            cacheObject.Weight *= 0.5;
                                        }
                                        // See if there's any AOE avoidance in that spot, if so reduce the weight to 1, for melee only
                                        if (KillMonstersInAoE &&
                                            CombatBase.KiteDistance <= 0 && cacheObject.RadiusDistance > 3f &&
                                            CacheData.AvoidanceObstacles.Any(aoe => aoe.AvoidanceType != AvoidanceType.PlagueCloud &&
                                                !aoe.IsAvoidanceAnimations &&
                                                cacheObject.Position.Distance2D(aoe.Position) <= aoe.Radius))
                                        {
                                            objWeightInfo += "InAoE ";
                                            cacheObject.Weight *= 0.5;
                                        }

                                        // Deal with treasure goblins - note, of priority is set to "0", then the is-a-goblin flag isn't even set for use here - the monster is ignored
                                        // Goblins on low health get extra priority - up to 2000ish
                                        if (Settings.Combat.Misc.GoblinPriority >= GoblinPriority.Prioritize && cacheObject.IsTreasureGoblin && cacheObject.HitPointsPct <= 0.98)
                                        {
                                            objWeightInfo += "LowHPGoblin ";
                                            cacheObject.Weight += Math.Max(((1 - cacheObject.HitPointsPct) / 100) * 2000d, 100d);
                                        }
                                        if (cacheObject.IsTreasureGoblin && !ObjectCache.Any(obj => (obj.Type == GObjectType.Door || obj.Type == GObjectType.Barricade) &&
                                            !MathUtil.IntersectsPath(obj.Position, obj.Radius, Player.Position, cacheObject.Position)))
                                        {
                                            // Logging goblin sightings
                                            if (lastGoblinTime == DateTime.MinValue)
                                            {
                                                TotalNumberGoblins++;
                                                lastGoblinTime = DateTime.UtcNow;
                                                Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "Goblin #{0} in sight. Distance={1:0}", TotalNumberGoblins, cacheObject.Distance);
                                            }
                                            else
                                            {
                                                if (DateTime.UtcNow.Subtract(lastGoblinTime).TotalMilliseconds > 30000)
                                                    lastGoblinTime = DateTime.MinValue;
                                            }

                                            if (CacheData.AvoidanceObstacles.Any(aoe => cacheObject.Position.Distance2D(aoe.Position) <= aoe.Radius) && Settings.Combat.Misc.GoblinPriority != GoblinPriority.Kamikaze)
                                            {
                                                objWeightInfo += "GoblinInAoE ";
                                                cacheObject.Weight = 1;
                                                break;
                                            }

                                            // Original Trinity stuff for priority handling now
                                            switch (Settings.Combat.Misc.GoblinPriority)
                                            {
                                                case GoblinPriority.Normal:
                                                    // Treating goblins as "normal monsters". Ok so I lied a little in the config, they get a little extra weight really! ;)
                                                    objWeightInfo += "GoblinNormal ";
                                                    cacheObject.Weight += 751;
                                                    break;
                                                case GoblinPriority.Prioritize:
                                                    // Super-high priority option below... 
                                                    objWeightInfo += "GoblinPrioritize ";
                                                    cacheObject.Weight = Math.Max((cacheObject.KillRange - cacheObject.RadiusDistance) / cacheObject.KillRange * MaxWeight, 1000d);
                                                    break;
                                                case GoblinPriority.Kamikaze:
                                                    // KAMIKAZE SUICIDAL TREASURE GOBLIN RAPE AHOY!
                                                    objWeightInfo += "GoblinKamikaze ";
                                                    cacheObject.Weight = MaxWeight;
                                                    break;

                                            }
                                        }
                                    }
                                }
                                break;
                            }
                        case GObjectType.HotSpot:
                            {
                                // If there's monsters in our face, ignore
                                if (prioritizeCloseRangeUnits)
                                    break;

                                // if we started cache refresh with a target already
                                if (LastTargetRactorGUID != -1)
                                    break;

                                // If it's very close, ignore
                                if (cacheObject.Distance <= V.F("Cache.HotSpot.MinDistance"))
                                {
                                    break;
                                }
                                else if (!CacheData.AvoidanceObstacles.Any(aoe => aoe.Position.Distance2D(cacheObject.Position) <= aoe.Radius))
                                {
                                    float maxDist = V.F("Cache.HotSpot.MaxDistance");
                                    cacheObject.Weight = (maxDist - cacheObject.Distance) / maxDist * 50000d;
                                }
                                break;
                            }
                        case GObjectType.Item:
                            {
                                // Default Weight
                                cacheObject.Weight = Math.Max((175 - cacheObject.Distance) / 175 * MaxWeight, 100d);

                                bool isTwoSquare = true;

                                var item = cacheObject.Item;
                                if (item != null)
                                {
                                    var commonData = item.CommonData;
                                    if (commonData != null && commonData.IsValid)
                                        isTwoSquare = commonData.IsTwoSquareItem;
                                }

                                // Don't pickup items if we're doing a TownRun
                                if (TrinityItemManager.FindValidBackpackLocation(isTwoSquare) == new Vector2(-1, -1))
                                {
                                    objWeightInfo += "TownRun";
                                    cacheObject.Weight = 0;
                                    break;
                                }

                                // Give legendaries max weight, always
                                if (cacheObject.ItemQuality >= ItemQuality.Legendary)
                                {
                                    cacheObject.Weight = MaxWeight * 1.2;
                                    objWeightInfo += " IsLegendary";
                                }

                                if (cacheObject.GoldAmount > 0 && cacheObject.Distance < 25f)
                                {
                                    cacheObject.Weight += MaxWeight * 0.25;
                                }

                                // ignore non-legendaries and gold near elites if we're ignoring elites
                                // not sure how we should safely determine this distance
                                if (cacheObject.ItemQuality < ItemQuality.Legendary && CombatBase.IgnoringElites &&
                                    ObjectCache.Any(u => u.IsUnit && u.IsEliteRareUnique &&
                                        u.Position.Distance2D(cacheObject.Position) <= V.F("Weight.Items.IgnoreNonLegendaryNearEliteDistance")))
                                {
                                    objWeightInfo += " IgnoringElites";
                                    cacheObject.Weight = 0;
                                    break;
                                }

                                // Ignore Legendaries in AoE
                                if (Settings.Loot.Pickup.IgnoreLegendaryInAoE && cacheObject.ItemQuality >= ItemQuality.Legendary)
                                {
                                    if (CacheData.AvoidanceObstacles.Any(a => cacheObject.Position.Distance2D(a.Position) <= a.Radius + 2f))
                                    {
                                        objWeightInfo += " InAvoidance";
                                        cacheObject.Weight = 1;
                                        break;
                                    }
                                    if (CacheData.AvoidanceObstacles.Any(a => MathUtil.IntersectsPath(a.Position, a.Radius + 2f, Player.Position, cacheObject.Position)))
                                    {
                                        objWeightInfo += " InterAvoidance";
                                        cacheObject.Weight = 1;
                                        break;
                                    }
                                }

                                // Ignore Non-Legendaries in AoE
                                if (Settings.Loot.Pickup.IgnoreNonLegendaryInAoE && cacheObject.ItemQuality < ItemQuality.Legendary)
                                {
                                    if (CacheData.AvoidanceObstacles.Any(a => cacheObject.Position.Distance2D(a.Position) <= a.Radius + 2f))
                                    {
                                        objWeightInfo += " InAvoidance";
                                        cacheObject.Weight = 1;
                                        break;
                                    }
                                    if (CacheData.AvoidanceObstacles.Any(a => MathUtil.IntersectsPath(a.Position, a.Radius + 2f, Player.Position, cacheObject.Position)))
                                    {
                                        objWeightInfo += " InterAvoidance";
                                        cacheObject.Weight = 1;
                                        break;
                                    }
                                }

                                // Ignore Legendaries near Elites
                                if (Settings.Loot.Pickup.IgnoreLegendaryNearElites && cacheObject.ItemQuality >= ItemQuality.Legendary &&
                                    ObjectCache.Any(u => u.IsEliteRareUnique && u.Position.Distance2D(cacheObject.Position) <= V.F("Weight.Items.IgnoreLegendaryNearEliteDistance")))
                                {
                                    cacheObject.Weight = 0;
                                    break;
                                }
                                // Ignore Non-Legendaries near Elites
                                if (Settings.Loot.Pickup.IgnoreNonLegendaryNearElites && cacheObject.ItemQuality < ItemQuality.Legendary &&
                                    ObjectCache.Any(u => u.IsEliteRareUnique && u.Position.Distance2D(cacheObject.Position) <= V.F("Weight.Items.IgnoreNonLegendaryNearEliteDistance")))
                                {
                                    cacheObject.Weight = 0;
                                    break;
                                }

                                // Point-blank items get a weight increase 
                                if (cacheObject.Distance <= 9f)
                                {
                                    cacheObject.Weight += 1000d;
                                    objWeightInfo += " IsPointBlank";
                                }

                                // Was already a target and is still viable, give it some free extra weight, to help stop flip-flopping between two targets
                                if (cacheObject.RActorGuid == LastTargetRactorGUID)
                                {
                                    cacheObject.Weight += 800;
                                    objWeightInfo += " PreviousTarget";
                                }

                                if (Player.ActorClass == ActorClass.Monk && Hotbar.Contains(SNOPower.Monk_TempestRush) && TimeSinceUse(SNOPower.Monk_TempestRush) < 1000 && cacheObject.ItemQuality < ItemQuality.Legendary)
                                {
                                    cacheObject.Weight = 500;
                                    objWeightInfo += " MonkTR Weight";
                                }

                                // If there's a monster in the path-line to the item, reduce the weight to 1, except legendaries
                                if (cacheObject.ItemQuality < ItemQuality.Legendary &&
                                    CacheData.MonsterObstacles.Any(cp => MathUtil.IntersectsPath(cp.Position, cp.Radius * 1.2f, Player.Position, cacheObject.Position)))
                                {
                                    objWeightInfo += " MonsterObstacles";
                                    cacheObject.Weight = 0;
                                }

                                // ignore any items/gold if there is mobs in kill radius and we aren't combat looting
                                if (CharacterSettings.Instance.CombatLooting && CurrentTarget != null && AnyMobsInRange && cacheObject.ItemQuality < ItemQuality.Legendary)
                                {
                                    objWeightInfo += " NoCombatLooting";
                                    cacheObject.Weight = 1;
                                }

                                break;
                            }
                        case GObjectType.Gold:
                            {
                                // Campaign A5 Quest "Lost Treasure of the Nephalem" - have to interact with nephalem switches first... 
                                // Quest: x1_Adria, Id: 257120, Step: 108 - disable all looting, pickup, and objects
                                if (Player.WorldType != Act.OpenWorld && Player.CurrentQuestSNO == 257120 && Player.CurrentQuestStep == 108)
                                {
                                    cacheObject.Weight = 0;
                                    objWeightInfo += " DisableForQuest";
                                    break;
                                }

                                // Default Weight
                                cacheObject.Weight = Math.Max((175 - cacheObject.Distance) / 175 * MaxWeight, 100d);

                                // Ignore gold near Elites
                                if (Settings.Loot.Pickup.IgnoreGoldNearElites && ObjectCache.Any(u => u.IsEliteRareUnique && u.Position.Distance2D(cacheObject.Position) <= V.F("Weight.Items.IgnoreGoldNearEliteDistance")))
                                {
                                    cacheObject.Weight = 0;
                                    break;
                                }

                                // Ignore gold in AoE
                                if (Settings.Loot.Pickup.IgnoreGoldInAoE && CacheData.AvoidanceObstacles.Any(aoe => cacheObject.Position.Distance2D(aoe.Position) <= aoe.Radius))
                                {
                                    cacheObject.Weight = 1;
                                    break;
                                }

                                // Was already a target and is still viable, give it some free extra weight, to help stop flip-flopping between two targets
                                if (cacheObject.RActorGuid == LastTargetRactorGUID)
                                {
                                    cacheObject.Weight += 800;
                                    objWeightInfo += " PreviousTarget";
                                }

                                // If there's a monster in the path-line to the item, reduce the weight to 1, except legendaries
                                if (cacheObject.ItemQuality < ItemQuality.Legendary &&
                                    CacheData.MonsterObstacles.Any(cp => MathUtil.IntersectsPath(cp.Position, cp.Radius * 1.2f, Player.Position, cacheObject.Position)))
                                {
                                    objWeightInfo += " MonsterObstacles";
                                    cacheObject.Weight = 0;
                                    break;
                                }

                                // ignore any items/gold if there is mobs in kill radius and we aren't combat looting
                                if (CharacterSettings.Instance.CombatLooting && CurrentTarget != null && AnyMobsInRange && cacheObject.ItemQuality < ItemQuality.Legendary)
                                {
                                    objWeightInfo += " NoCombatLooting";
                                    cacheObject.Weight = 0;
                                    break;
                                }

                                if (!cacheObject.IsNavigable)
                                {
                                    objWeightInfo += " NavBlocking";
                                    cacheObject.Weight = 0;
                                    break;
                                }

                                // See if there's any AOE avoidance in that spot or inbetween us, if so reduce the weight to 1
                                if (CacheData.AvoidanceObstacles.Any(a => cacheObject.Position.Distance2D(a.Position) <= a.Radius + 2f))
                                {
                                    objWeightInfo += " InAvoidance";
                                    cacheObject.Weight = 1;
                                    break;
                                }
                                if (CacheData.AvoidanceObstacles.Any(a => MathUtil.IntersectsPath(a.Position, a.Radius + 2f, Player.Position, cacheObject.Position)))
                                {
                                    objWeightInfo += " InterAvoidance";
                                    cacheObject.Weight = 1;
                                    break;
                                }

                                break;
                            }
                        case GObjectType.PowerGlobe:
                            {
                                if (!TownRun.IsTryingToTownPortal())
                                {
                                    cacheObject.Weight = (90f - cacheObject.RadiusDistance) / 90f * 5000d;
                                }

                                // Ignore globe near Elites
                                if (ObjectCache.Any(u => u.IsEliteRareUnique && u.Position.Distance2D(cacheObject.Position) <= 15f))
                                {
                                    cacheObject.Weight = 0;
                                    break;
                                }

                                // Point-blank items get a weight increase 
                                if (cacheObject.GoldAmount <= 0 && cacheObject.Distance <= 12f)
                                    cacheObject.Weight += 1000d;

                                // If there's a monster in the path-line to the item, reduce the weight to 1
                                if (CacheData.MonsterObstacles.Any(cp => MathUtil.IntersectsPath(cp.Position, cp.Radius * 1.2f, Player.Position, cacheObject.Position)))
                                    cacheObject.Weight = 0;

                                if (!cacheObject.IsNavigable)
                                {
                                    objWeightInfo += " NavBlocking";
                                    cacheObject.Weight = 0;
                                }

                                // See if there's any AOE avoidance in that spot or inbetween us, if so reduce the weight to 1
                                if (CacheData.AvoidanceObstacles.Any(a => cacheObject.Position.Distance2D(a.Position) <= a.Radius + 2f))
                                {
                                    objWeightInfo += " InAvoidance";
                                    cacheObject.Weight = 1;
                                    break;
                                }
                                if (CacheData.AvoidanceObstacles.Any(a => MathUtil.IntersectsPath(a.Position, a.Radius + 2f, Player.Position, cacheObject.Position)))
                                {
                                    objWeightInfo += " InterAvoidance";
                                    cacheObject.Weight = 1;
                                    break;
                                }

                                break;
                            }
                        case GObjectType.ProgressionGlobe:
                            {
                                if (!TownRun.IsTryingToTownPortal())
                                {
                                    cacheObject.Weight = (150f - cacheObject.RadiusDistance) / 150f * MaxWeight;
                                }

                                // Ignore globe near Elites
                                if (ObjectCache.Any(u => u.IsEliteRareUnique && u.HitPointsPct >= 0.1 && u.Position.Distance2D(cacheObject.Position) <= 15f))
                                {
                                    cacheObject.Weight = 0;
                                    break;
                                }

                                // Point-blank items get a weight increase 
                                if (cacheObject.GoldAmount <= 0 && cacheObject.Distance <= 12f)
                                    cacheObject.Weight += 1000d;

                                // If there's a monster in the path-line to the item, reduce the weight to 1
                                if (CacheData.MonsterObstacles.Any(cp => MathUtil.IntersectsPath(cp.Position, cp.Radius * 1.2f, Player.Position, cacheObject.Position)))
                                    cacheObject.Weight = 0;

                                if (Player.ActorClass == ActorClass.Monk && Skills.Monk.DashingStrike.CanCast(CombatBase.CanCastFlags.NoTimer))
                                    cacheObject.Weight = MaxWeight;

                                // See if there's any AOE avoidance in that spot or inbetween us, if so reduce the weight to 1
                                if (CacheData.AvoidanceObstacles.Any(a => cacheObject.Position.Distance2D(a.Position) <= a.Radius + 2f))
                                {
                                    objWeightInfo += " InAvoidance";
                                    cacheObject.Weight = 1;
                                    break;
                                }
                                if (CacheData.AvoidanceObstacles.Any(a => MathUtil.IntersectsPath(a.Position, a.Radius + 2f, Player.Position, cacheObject.Position)))
                                {
                                    objWeightInfo += " InterAvoidance";
                                    cacheObject.Weight = 1;
                                    break;
                                }

                                break;
                            }
                        case GObjectType.HealthGlobe:
                            {
                                // Ignore globe near Elites
                                if (ObjectCache.Any(u => u.IsEliteRareUnique && u.Position.Distance2D(cacheObject.Position) <= 15f))
                                {
                                    cacheObject.Weight = 0;
                                    break;
                                }

                                // Weight Health Globes
                                if (!cacheObject.IsNavigable || TownRun.IsTryingToTownPortal())
                                {
                                    objWeightInfo += " NavBlocking";
                                    cacheObject.Weight = 0;
                                    break;
                                }


                                double weightPct = (1d - Player.CurrentHealthPct);
                                if (Player.CurrentHealthPct > 1 && Player.PrimaryResourcePct > 1)
                                {
                                    weightPct = ((2d - Player.CurrentHealthPct - Player.PrimaryResourcePct)) * 0.5;
                                }

                                if (!Settings.Combat.Misc.CollectHealthGlobe)
                                {
                                    cacheObject.Weight = 0;
                                    break;
                                }
                                else if (cacheObject.Distance < 6f && Player.CurrentHealthPct >= CombatBase.EmergencyHealthGlobeLimit)
                                {
                                    if (hiPriorityHealthGlobes)
                                    {
                                        cacheObject.Weight = MaxWeight * weightPct;
                                    }
                                    else
                                    {
                                        cacheObject.Weight = (90f - cacheObject.RadiusDistance) / 90f * weightPct * 30000d;
                                    }

                                    if (ObjectCache.Any(p => p.Type == GObjectType.Player && p.RActorGuid != Player.RActorGuid))
                                    {
                                        double minPartyHealth = 1d;
                                        minPartyHealth = ObjectCache.Where(p => p.Type == GObjectType.Player && p.RActorGuid != Player.RActorGuid).Min(p => p.HitPointsPct);

                                        // Added weight for lowest health of party member
                                        if (minPartyHealth > 0d && minPartyHealth < V.D("Weight.Globe.MinPartyHealthPct"))
                                        {
                                            cacheObject.Weight = (1d - minPartyHealth) * 5000d;
                                        }
                                    }

                                }
                                else if (Player.CurrentHealthPct < CombatBase.EmergencyHealthGlobeLimit || collectHealthGlobeForRessource)
                                {
                                    // Ok we have globes enabled, and our health is low
                                    if (hiPriorityHealthGlobes)
                                    {
                                        cacheObject.Weight = MaxWeight * weightPct;
                                    }
                                    else
                                    {
                                        cacheObject.Weight = (90f - cacheObject.RadiusDistance) / 90f * weightPct * 17000d;
                                    }

                                    // Point-blank items get a weight increase
                                    if (cacheObject.Distance <= 15f)
                                        cacheObject.Weight += 3000d;

                                    // Close items get a weight increase
                                    if (cacheObject.Distance <= 60f)
                                        cacheObject.Weight += 1500d;

                                    // Primary resource is low and we're wearing Reapers Wraps
                                    if (Player.IsInCombat && Player.PrimaryResourcePct < 0.3 && Legendary.ReapersWraps.IsEquipped && (TargetUtil.AnyMobsInRange(40, 5) || TargetUtil.AnyElitesInRange(40)))
                                        cacheObject.Weight += 3000d;

                                    // Was already a target and is still viable, give it some free extra weight, to help stop flip-flopping between two targets
                                    if (cacheObject.RActorGuid == LastTargetRactorGUID && cacheObject.Distance <= 25f)
                                        cacheObject.Weight += 800;
                                }

                                // If there's a monster in the path-line to the item, reduce the weight by 15% for each
                                Vector3 point = cacheObject.Position;
                                foreach (CacheObstacleObject tempobstacle in CacheData.MonsterObstacles.Where(cp =>
                                    MathUtil.IntersectsPath(cp.Position, cp.Radius, Player.Position, point)))
                                {
                                    if (hiPriorityHealthGlobes)
                                        cacheObject.Weight *= 0.85;
                                    else
                                        cacheObject.Weight *= 0.7;
                                }

                                if (cacheObject.Distance > 10f)
                                {
                                    // See if there's any AOE avoidance in that spot, if so reduce the weight by 10%
                                    if (hiPriorityHealthGlobes)
                                    {
                                        if (CacheData.AvoidanceObstacles.Any(a => MathUtil.IntersectsPath(a.Position, a.Radius + 2f, Player.Position, cacheObject.Position)))
                                            cacheObject.Weight *= 0.9;
                                    }
                                    else
                                    {
                                        if (CacheData.AvoidanceObstacles.Any(a => MathUtil.IntersectsPath(a.Position, a.Radius + 2f, Player.Position, cacheObject.Position)))
                                            cacheObject.Weight *= .8;
                                    }

                                }

                                // do not collect health globes if we are kiting and health globe is too close to monster or avoidance
                                if (CombatBase.KiteDistance > 0 && CacheData.MonsterObstacles.Any(m => m.Position.Distance(cacheObject.Position) < (CombatBase.KiteDistance * 0.7)))
                                    cacheObject.Weight = 0;

                                // See if there's any AOE avoidance in that spot or inbetween us, if so reduce the weight to 1
                                if (CacheData.AvoidanceObstacles.Any(a => cacheObject.Position.Distance2D(a.Position) <= a.Radius + 2f))
                                {
                                    objWeightInfo += " InAvoidance";
                                    cacheObject.Weight = 1;
                                    break;
                                }
                                if (CacheData.AvoidanceObstacles.Any(a => MathUtil.IntersectsPath(a.Position, a.Radius + 2f, Player.Position, cacheObject.Position)))
                                {
                                    objWeightInfo += " InterAvoidance";
                                    cacheObject.Weight = 1;
                                    break;
                                }

                                // Calculate a spot reaching a little bit further out from the globe, to help globe-movements
                                if (cacheObject.Weight > 1)
                                    cacheObject.Position = MathEx.CalculatePointFrom(cacheObject.Position, Player.Position, cacheObject.Distance + 3f);

                                break;
                            }
                        case GObjectType.HealthWell:
                            {
                                if (!ObjectCache.Any(o => o.IsBoss))
                                {
                                    if (!Settings.WorldObject.UseShrine)
                                    {
                                        objWeightInfo += "UseShrineDisabled";
                                        break;
                                    }

                                    if (CacheData.MonsterObstacles.Any(unit => MathUtil.IntersectsPath(unit.Position, unit.Radius, Player.Position, cacheObject.Position)))
                                    {
                                        objWeightInfo += "MonsterObstacles";
                                        cacheObject.Weight = 0;
                                        break;
                                    }

                                    if (CacheData.AvoidanceObstacles.Any(aoe => MathUtil.IntersectsPath(aoe.Position, aoe.Radius, Player.Position, cacheObject.Position)))
                                    {
                                        objWeightInfo += "TimeBoundAvoidance";
                                        cacheObject.Weight = 1;
                                        break;
                                    }
                                    if (!cacheObject.IsNavigable)
                                    {
                                        objWeightInfo += " NavBlocking";
                                        cacheObject.Weight = 0;
                                        break;
                                    }
                                }

                                if (prioritizeCloseRangeUnits)
                                {
                                    objWeightInfo += "prioritizeCloseRangeUnits";
                                    break;
                                }

                                // Current Health Percentage is higher than setting
                                if (Player.CurrentHealthPct * 100 > Settings.WorldObject.HealthWellMinHealth)
                                {
                                    objWeightInfo += "HealthWellMinHealth";
                                    break;
                                }

                                // As a percentage of health with typical maximum weight
                                cacheObject.Weight = MaxWeight * (1 - Player.CurrentHealthPct);

                                // See if there's any AOE avoidance in that spot or inbetween us, if so reduce the weight to 1
                                if (CacheData.AvoidanceObstacles.Any(a => cacheObject.Position.Distance2D(a.Position) <= a.Radius + 2f))
                                {
                                    objWeightInfo += " InAvoidance";
                                    cacheObject.Weight = 1;
                                    break;
                                }
                                if (CacheData.AvoidanceObstacles.Any(a => MathUtil.IntersectsPath(a.Position, a.Radius + 2f, Player.Position, cacheObject.Position)))
                                {
                                    objWeightInfo += " InterAvoidance";
                                    cacheObject.Weight = 1;
                                    break;
                                }

                                break;
                            }
                        case GObjectType.CursedShrine:
                            {
                                cacheObject.Weight += 5000d;

                                // See if there's any AOE avoidance in that spot or inbetween us, if so reduce the weight to 1
                                if (CacheData.AvoidanceObstacles.Any(a => cacheObject.Position.Distance2D(a.Position) <= a.Radius + 2f))
                                {
                                    objWeightInfo += " InAvoidance";
                                    cacheObject.Weight = 1;
                                    break;
                                }
                                if (CacheData.AvoidanceObstacles.Any(a => MathUtil.IntersectsPath(a.Position, a.Radius + 2f, Player.Position, cacheObject.Position)))
                                {
                                    objWeightInfo += " InterAvoidance";
                                    cacheObject.Weight = 1;
                                    break;
                                }

                                break;
                            }
                        case GObjectType.Shrine:
                            {
                                float maxRange = Player.IsInRift ? 90f : 75f;
                                double maxWeight = Player.IsInRift ? MaxWeight * 0.75d : 100d;

                                // Weight Shrines
                                if (Settings.WorldObject.HiPriorityShrines)
                                {
                                    cacheObject.Weight = MaxWeight * 0.75;
                                }
                                else
                                    cacheObject.Weight = Math.Max(((maxRange - cacheObject.RadiusDistance) / maxRange * 30000d), 100d);

                                // Very close shrines get a weight increase
                                if (cacheObject.Distance <= 30f)
                                    cacheObject.Weight += 10000d;

                                // Disable safety checks for Rift Pylons
                                if (cacheObject.Weight > 0)
                                {
                                    if (cacheObject.Distance <= 5f)
                                    {
                                        cacheObject.Weight += MaxWeight;
                                        break;
                                    }

                                    // Was already a target and is still viable, give it some free extra weight, to help stop flip-flopping between two targets
                                    if (cacheObject.RActorGuid == LastTargetRactorGUID && cacheObject.Distance <= 25f)
                                        cacheObject.Weight += 400;

                                    // If there's a monster in the path-line to the item
                                    if (CacheData.MonsterObstacles.Any(cp => MathUtil.IntersectsPath(cp.Position, cp.Radius, Player.Position, cacheObject.Position)))
                                        cacheObject.Weight = 0;

                                    // if there's any monsters nearby
                                    if (TargetUtil.AnyMobsInRange(15f) && !Player.IsInRift)
                                        cacheObject.Weight = 0;

                                    if (prioritizeCloseRangeUnits)
                                        cacheObject.Weight = 0;

                                    // See if there's any AOE avoidance in that spot or inbetween us, if so reduce the weight to 1
                                    if (CacheData.AvoidanceObstacles.Any(a => cacheObject.Position.Distance2D(a.Position) <= a.Radius + 2f))
                                    {
                                        objWeightInfo += " InAvoidance";
                                        cacheObject.Weight = 1;
                                        break;
                                    }
                                    if (CacheData.AvoidanceObstacles.Any(a => MathUtil.IntersectsPath(a.Position, a.Radius + 2f, Player.Position, cacheObject.Position)))
                                    {
                                        objWeightInfo += " InterAvoidance";
                                        cacheObject.Weight = 1;
                                        break;
                                    }
                                }
                                break;
                            }
                        case GObjectType.Door:
                            {
                                // Ignore doors where units are blocking our LoS
                                if (ObjectCache.Any(u => u.IsUnit && u.HitPointsPct > 0 && u.Distance < cacheObject.Distance &&
                                        MathUtil.IntersectsPath(u.Position, u.Radius, Player.Position, cacheObject.Position)))
                                {
                                    cacheObject.Weight = 0;
                                    objWeightInfo += " Unitblocking";
                                    break;
                                }

                                // Prioritize doors where units are behind them
                                if (ObjectCache.Any(u => u.IsUnit && u.HitPointsPct > 0 && u.Distance > cacheObject.Distance &&
                                    MathUtil.IntersectsPath(cacheObject.Position, cacheObject.Radius, Player.Position, u.Position)))
                                {
                                    cacheObject.Weight += 5000;
                                    objWeightInfo += " BlockingUnit";
                                }

                                // Was already a target and is still viable, give it some free extra weight, to help stop flip-flopping between two targets
                                if (cacheObject.RActorGuid == LastTargetRactorGUID && cacheObject.Distance <= 25f)
                                {
                                    cacheObject.Weight += 1000;
                                    objWeightInfo += " RePick";

                                }
                                // We're standing on the damn thing... open it!!
                                if (cacheObject.RadiusDistance <= 15f)
                                {
                                    cacheObject.Weight = MaxWeight;
                                    objWeightInfo += " <12f";
                                }

                                // See if there's any AOE avoidance in that spot or inbetween us, if so reduce the weight to 1
                                if (CacheData.AvoidanceObstacles.Any(a => cacheObject.Position.Distance2D(a.Position) <= a.Radius + 2f))
                                {
                                    objWeightInfo += " InAvoidance";
                                    cacheObject.Weight = 1;
                                    break;
                                }
                                if (CacheData.AvoidanceObstacles.Any(a => MathUtil.IntersectsPath(a.Position, a.Radius + 2f, Player.Position, cacheObject.Position)))
                                {
                                    objWeightInfo += " InterAvoidance";
                                    cacheObject.Weight = 1;
                                    break;
                                }

                                break;
                            }
                        case GObjectType.Barricade:
                            {
                                // rrrix added this as a single "weight" source based on the DestructableRange.
                                // Calculate the weight based on distance, where a distance = 1 is 5000, 90 = 0
                                cacheObject.Weight = (90f - cacheObject.RadiusDistance) / 90f * 5000f;

                                // Was already a target and is still viable, give it some free extra weight, to help stop flip-flopping between two targets
                                if (cacheObject.RActorGuid == LastTargetRactorGUID && cacheObject.Distance <= 25f)
                                    cacheObject.Weight += 1000;

                                // We're standing on the damn thing... open it!!
                                if (cacheObject.RadiusDistance <= 12f)
                                    cacheObject.Weight += 30000d;

                            }
                            break;

                        case GObjectType.Destructible:
                            {
                                if (DataDictionary.ForceDestructibles.Contains(cacheObject.ActorSNO))
                                {
                                    objWeightInfo += " ForceDestructibles";
                                    cacheObject.Weight = 100;
                                    break;
                                }

                                // Not Stuck, skip!
                                if (Settings.WorldObject.DestructibleOption == DestructibleIgnoreOption.OnlyIfStuck &&
                                    cacheObject.RadiusDistance > 0 &&
                                    (DateTime.UtcNow.Subtract(PlayerMover.LastGeneratedStuckPosition).TotalSeconds > 3))
                                {
                                    objWeightInfo += " NotStuck";
                                    break;
                                }

                                // rrrix added this as a single "weight" source based on the DestructableRange.
                                // Calculate the weight based on distance, where a distance = 1 is 5000, 90 = 0
                                cacheObject.Weight = (90f - cacheObject.RadiusDistance) / 90f * 1000f;

                                // Was already a target and is still viable, give it some free extra weight, to help stop flip-flopping between two targets
                                if (cacheObject.RActorGuid == LastTargetRactorGUID && cacheObject.Distance <= 25f)
                                    cacheObject.Weight += 400;

                                //// Close destructibles get a weight increase
                                //if (cacheObject.CentreDistance <= 16f)
                                //    cacheObject.Weight += 1500d;

                                // If there's a monster in the path-line to the item, reduce the weight by 50%
                                if (CacheData.MonsterObstacles.Any(cp => MathUtil.IntersectsPath(cp.Position, cp.Radius, Player.Position, cacheObject.Position)))
                                    cacheObject.Weight *= 0.5;

                                // See if there's any AOE avoidance in that spot or inbetween us, if so reduce the weight to 1
                                if (CacheData.AvoidanceObstacles.Any(a => cacheObject.Position.Distance2D(a.Position) <= a.Radius + 2f))
                                {
                                    objWeightInfo += " InAvoidance";
                                    cacheObject.Weight = 1;
                                    break;
                                }
                                if (CacheData.AvoidanceObstacles.Any(a => MathUtil.IntersectsPath(a.Position, a.Radius + 2f, Player.Position, cacheObject.Position)))
                                {
                                    objWeightInfo += " InterAvoidance";
                                    cacheObject.Weight = 1;
                                    break;
                                }

                                // Are we prioritizing close-range stuff atm? If so limit it at a value 3k lower than monster close-range priority
                                if (prioritizeCloseRangeUnits)
                                    cacheObject.Weight = (15f - cacheObject.Distance) / 15f * 19200d;

                                //// We're standing on the damn thing... break it
                                if (cacheObject.RadiusDistance <= 5f)
                                    cacheObject.Weight += 40000d;

                                //// Fix for WhimsyShire Pinata
                                if (DataDictionary.ResplendentChestIds.Contains(cacheObject.ActorSNO))
                                    cacheObject.Weight = 100 + cacheObject.RadiusDistance;
                                break;
                            }
                        case GObjectType.Interactable:
                            {
                                // Campaign A5 Quest "Lost Treasure of the Nephalem" - have to interact with nephalem switches first... 
                                // Quest: x1_Adria, Id: 257120, Step: 108 - disable all looting, pickup, and objects
                                if (Player.WorldType != Act.OpenWorld && Player.CurrentQuestSNO == 257120 && Player.CurrentQuestStep == 108)
                                {
                                    cacheObject.Weight = MaxWeight / 3;
                                    objWeightInfo += " PrioritizeForQuest";
                                    break;
                                }

                                // Need to Prioritize, forget it!
                                if (prioritizeCloseRangeUnits)
                                    cacheObject.Weight = (15f - cacheObject.Distance) / 15f * 19100d;

                                // nearby monsters attacking us - don't try to use headtone
                                if (cacheObject.Object is DiaGizmo && cacheObject.Gizmo.CommonData.ActorInfo.GizmoType == GizmoType.Headstone &&
                                    ObjectCache.Any(u => u.IsUnit && u.RadiusDistance < 25f && u.IsFacingPlayer))
                                {
                                    cacheObject.Weight = 0;
                                    break;
                                }

                                // Weight Interactable Specials
                                cacheObject.Weight = (300d - cacheObject.Distance) / 300d * 1000d;

                                if (DataDictionary.HighPriorityInteractables.Contains(cacheObject.ActorSNO) && cacheObject.RadiusDistance <= 30f)
                                {
                                    cacheObject.Weight = MaxWeight;
                                    break;
                                }

                                // Very close interactables get a weight increase
                                if (cacheObject.Distance <= 8f)
                                    cacheObject.Weight += 1000d;

                                if (cacheObject.IsQuestMonster)
                                {
                                    cacheObject.Weight += 3000d;
                                }

                                // Was already a target and is still viable, give it some free extra weight, to help stop flip-flopping between two targets
                                if (cacheObject.RActorGuid == LastTargetRactorGUID && cacheObject.Distance <= 25f)
                                    cacheObject.Weight += 400;

                                // If there's a monster in the path-line to the item, if so reduce the weight to 1
                                if (CacheData.MonsterObstacles.Any(cp => MathUtil.IntersectsPath(cp.Position, cp.Radius, Player.Position, cacheObject.Position)))
                                {
                                    cacheObject.Weight = 0;
                                    break;
                                }

                                // See if there's any AOE avoidance in that spot or inbetween us, if so reduce the weight to 1
                                if (CacheData.AvoidanceObstacles.Any(a => cacheObject.Position.Distance2D(a.Position) <= a.Radius + 2f))
                                {
                                    objWeightInfo += " InAvoidance";
                                    cacheObject.Weight = 1;
                                    break;
                                }
                                if (CacheData.AvoidanceObstacles.Any(a => MathUtil.IntersectsPath(a.Position, a.Radius + 2f, Player.Position, cacheObject.Position)))
                                {
                                    objWeightInfo += " InterAvoidance";
                                    cacheObject.Weight = 1;
                                    break;
                                }


                                break;
                            }
                        case GObjectType.Container:
                            {
                                // Need to Prioritize, forget it!
                                if (prioritizeCloseRangeUnits)
                                {
                                    objWeightInfo += " prioritizeCloseRangeUnits";
                                    cacheObject.Weight = 0;
                                    break;
                                }

                                float maxRange = Settings.WorldObject.ContainerOpenRange;
                                if (cacheObject.InternalName.ToLower().Contains("chest_rare"))
                                    maxRange = 250f;

                                if (!cacheObject.IsNavigable)
                                {
                                    objWeightInfo += " NavBlocking";
                                    cacheObject.Weight = 0;
                                    break;
                                }

                                if (Legendary.HarringtonWaistguard.IsBuffActive)
                                {
                                    objWeightInfo += " HarringtonBuffIsUp";
                                    cacheObject.Weight = 0;
                                    break;
                                }

                                // Weight Containers
                                cacheObject.Weight = (maxRange - cacheObject.Distance) / maxRange * 100d;

                                // Very close containers get a weight increase
                                if (cacheObject.Distance <= 8f)
                                    cacheObject.Weight += 600d;

                                // Open container for the damage buff
                                if (Legendary.HarringtonWaistguard.IsEquipped && !Legendary.HarringtonWaistguard.IsBuffActive &&
                                    Player.IsInCombat && cacheObject.Distance < 80f || getHiPriorityContainer)
                                    cacheObject.Weight += 20000d;

                                // Was already a target and is still viable, give it some free extra weight, to help stop flip-flopping between two targets
                                if (cacheObject.RActorGuid == LastTargetRactorGUID && cacheObject.Distance <= 25f)
                                    cacheObject.Weight += 400;

                                // If there's a monster in the path-line to the item, reduce the weight by 50%
                                if (CacheData.MonsterObstacles.Any(cp => MathUtil.IntersectsPath(cp.Position, cp.Radius, Player.Position, cacheObject.Position)))
                                    cacheObject.Weight *= 0.5;

                                // See if there's any AOE avoidance in that spot or inbetween us, if so reduce the weight to 1
                                if (CacheData.AvoidanceObstacles.Any(a => cacheObject.Position.Distance2D(a.Position) <= a.Radius + 2f))
                                {
                                    objWeightInfo += " InAvoidance";
                                    cacheObject.Weight = 1;
                                    break;
                                }
                                if (CacheData.AvoidanceObstacles.Any(a => MathUtil.IntersectsPath(a.Position, a.Radius + 2f, Player.Position, cacheObject.Position)))
                                {
                                    objWeightInfo += " InterAvoidance";
                                    cacheObject.Weight = 1;
                                    break;
                                }
                                break;
                            }

                    }

                    if (cacheObject.Weight > 0 &&
                        !cacheObject.IsUnit &&
                        cacheObject.Type != GObjectType.Door &&
                        cacheObject.Type != GObjectType.Interactable &&
                        cacheObject.Type != GObjectType.Destructible &&
                        CombatBase.KiteDistance > 0 && CacheData.MonsterObstacles.Any(m => m.Position.Distance(cacheObject.Position) < (CombatBase.KiteDistance * 0.5)))
                    {
                        objWeightInfo += " InKiteZone ";
                        cacheObject.Weight = 0;
                    }

                    if (cacheObject.Weight > MaxWeight && !Double.IsNaN(cacheObject.Weight))
                    {
                        objWeightInfo += " MaxWeight ";
                        cacheObject.Weight = Math.Min(cacheObject.Weight, MaxWeight);
                    }

                    // Prevent current target dynamic ranged weighting flip-flop 
                    if (LastTargetRactorGUID == cacheObject.RActorGuid && cacheObject.Weight < 1 && !!cacheObject.IsNavigable && !cacheObject.IsInLineOfSight)
                    {
                        cacheObject.Weight = 100;
                    }

                    if (LastTargetRactorGUID == cacheObject.RActorGuid && cacheObject.Weight > 1)
                    {
                        cacheObject.Weight += 250;
                    }

                    if (cacheObject.IsUnit && !CombatBase.IsNull(CombatBase.CurrentPower) && cacheObject.Position.Distance2D(CombatBase.CurrentPower.MovePosition) <= 10f)
                    {
                        cacheObject.Weight += 1000;
                    }

                    objWeightInfo += cacheObject.IsNPC ? " IsNPC" : "";
                    objWeightInfo += cacheObject.NPCIsOperable ? " IsOperable" : "";

                    Logger.Log(TrinityLogLevel.Verbose, LogCategory.Weight,
                        "Weight={0:0} name={1} sno={2} type={3} R-Dist={4:0} IsElite={5} RAGuid={6} {7}",
                            cacheObject.Weight, cacheObject.InternalName, cacheObject.ActorSNO, cacheObject.Type, cacheObject.RadiusDistance, cacheObject.IsEliteRareUnique,
                            cacheObject.RActorGuid, objWeightInfo);
                    cacheObject.WeightInfo = objWeightInfo;

                    // Use the highest weight, and if at max weight, the closest
                    bool pickNewTarget = (cacheObject.Weight > 0 &&
                        (cacheObject.Weight > HighestWeightFound ||
                        (cacheObject.Weight == HighestWeightFound && cacheObject.Distance < CurrentTarget.Distance)));

                    // Is the weight of this one higher than the current-highest weight? Then make this the new primary target!
                    if (pickNewTarget)
                    {
                        // Force the character to stay where it is if there is nothing available that is out of avoidance stuff and we aren't already in avoidance stuff
                        if (cacheObject.Weight == 1 && !cacheObject.IsUnit)
                        {
                            _shouldStayPutDuringAvoidance = true;
                            cacheObject.Weight = 0;
                            cacheObject.WeightInfo += " StayPutAoE ";
                        }
                        else
                        {
                            _shouldStayPutDuringAvoidance = false;
                        }

                        /*
                         *  Assign CurrentTarget
                         */
                        if (cacheObject.Weight > 0)
                        {
                            CurrentTarget = cacheObject;
                            HighestWeightFound = cacheObject.Weight;
                        }
                    }
                }

                if (CurrentTarget != null && !CurrentTarget.IsUnit && !CurrentTarget.IsAvoidance &&
                    CurrentTarget.Weight == 1 &&
                    CacheData.AvoidanceObstacles.Any(a => MathUtil.IntersectsPath(a.Position, a.Radius + 2f, Player.Position, CurrentTarget.Position) ||
                    CacheData.AvoidanceObstacles.Any(aoe => CurrentTarget.Position.Distance2D(aoe.Position) <= aoe.Radius)))
                {
                    _shouldStayPutDuringAvoidance = true;
                }

                // Loop through all the objects and give them a weight
                if (CurrentTarget != null && CurrentTarget.InternalName != null && CurrentTarget.ActorSNO > 0 && CurrentTarget.RActorGuid != LastTargetRactorGUID || CurrentTarget != null && CurrentTarget.IsMarker)
                {
                    RecordTargetHistory();
                    Logger.Log(TrinityLogLevel.Verbose, LogCategory.Weight, "Target changed to {0} // {1} ({2}) {3}", CurrentTarget.ActorSNO, CurrentTarget.InternalName, CurrentTarget.Type, CurrentTarget.WeightInfo);
                }
            }
            #endregion

            #region StayingPutDuringAvoidance
            bool shouldStayPutDuringAvoidance =
                CurrentTarget != null && CombatBase.IsCombatAllowed &&
                Settings.Combat.Misc.AvoidAoEOutOfCombat && _shouldStayPutDuringAvoidance &&
                !(Player.StandingInAvoidance || Player.AvoidDeath || Player.NeedToKite || CombatBase.PlayerIsSurrounded);

            if (shouldStayPutDuringAvoidance)
            {
                var _pathIntersect = (
                    from p in MainGrid.Map
                    where
                        p.Position.Distance2D(CurrentTarget.Position) < CurrentTarget.Distance &&
                        p.Weight > 0 &&
                        !p.HasAvoidanceRelated &&
                        !CacheData.AvoidanceObstacles.Any(a => MathUtil.IntersectsPath(a.Position, a.Radius + 2f, Player.Position, p.Position))
                    orderby
                        p.Position.Distance2D(CurrentTarget.Position)
                    select p).ToList().FirstOrDefault();

                if (_pathIntersect != null)
                {
                    Logger.Log(TrinityLogLevel.Info, LogCategory.Avoidance, "Generating new path to CurrentTarget (Avoidance OOC)");
                    CurrentTarget.Position = _pathIntersect.Position;
                }
                else
                {
                    Logger.Log(TrinityLogLevel.Info, LogCategory.Avoidance, "Staying Put During Avoidance (Avoidance OOC)");
                    CurrentTarget = new TrinityCacheObject()
                    {
                        Position = Player.Position,
                        Type = GObjectType.OocAvoidance,
                        Weight = 20000,
                        Distance = 2f,
                        Radius = 2f,
                        InternalName = "StayPutPoint"
                    };
                }
            }
            #endregion

            // Kite
            if (CurrentTarget != null && (CurrentTarget.IsUnit || CurrentTarget.Type == GObjectType.OocAvoidance))
            {
                if (CurrentTarget.IsUnit && CurrentTarget.Weight == 1)
                    Trinity.Player.NeedToKite = true;

                KiteAvoidDestination = Player.Position;
                RefreshSetKiting(ref KiteAvoidDestination, Trinity.Player.NeedToKite);
            }
        }

        internal static void RecordTargetHistory()
        {
            int timesBeenPrimaryTarget;

            string objectKey = CurrentTarget.Type.ToString() + CurrentTarget.Position + CurrentTarget.InternalName + CurrentTarget.ItemLevel + CurrentTarget.ItemQuality + CurrentTarget.HitPoints;

            if (CacheData.PrimaryTargetCount.TryGetValue(objectKey, out timesBeenPrimaryTarget))
            {
                timesBeenPrimaryTarget++;
                CacheData.PrimaryTargetCount[objectKey] = timesBeenPrimaryTarget;
                CurrentTarget.TimesBeenPrimaryTarget = timesBeenPrimaryTarget;
                CurrentTarget.HasBeenPrimaryTarget = true;

                bool isEliteLowHealth = CurrentTarget.HitPointsPct <= 0.75 && CurrentTarget.IsBossOrEliteRareUnique;
                bool isLegendaryItem = CurrentTarget.Type == GObjectType.Item && CurrentTarget.ItemQuality >= ItemQuality.Legendary;

                bool isHoradricRelic = (CurrentTarget.InternalName.ToLower().StartsWith("horadricrelic") && CurrentTarget.TimesBeenPrimaryTarget > 5);

                if ((CurrentTarget.Type != GObjectType.ProgressionGlobe && !CurrentTarget.IsBoss && CurrentTarget.TimesBeenPrimaryTarget > 50 && !isEliteLowHealth && !isLegendaryItem) || isHoradricRelic ||
                    (CurrentTarget.TimesBeenPrimaryTarget > 200 && isLegendaryItem))
                {
                    Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "Blacklisting target {0} ActorSNO={1} RActorGUID={2} due to possible stuck/flipflop!",
                        CurrentTarget.InternalName, CurrentTarget.ActorSNO, CurrentTarget.RActorGuid);

                    var expires = CurrentTarget.IsMarker ? DateTime.UtcNow.AddSeconds(60) : DateTime.UtcNow.AddSeconds(30);

                    // Add to generic blacklist for safety, as the RActorGUID on items and gold can change as we move away and get closer to the items (while walking around corners)
                    // So we can't use any ID's but rather have to use some data which never changes (actorSNO, position, type, worldID)
                    GenericBlacklist.AddToBlacklist(new GenericCacheObject
                    {
                        Key = CurrentTarget.ObjectHash,
                        Value = null,
                        Expires = expires
                    });
                }
            }
            else
            {
                // Add to Primary Target Cache Count
                CacheData.PrimaryTargetCount.Add(objectKey, 1);
            }
        }

        private static double GetLastHadUnitsInSights()
        {
            return Math.Max(DateTime.UtcNow.Subtract(lastHadUnitInSights).TotalMilliseconds, DateTime.UtcNow.Subtract(lastHadEliteUnitInSights).TotalMilliseconds);
        }

        const double MaxWeight = 50000d;
    }
}
