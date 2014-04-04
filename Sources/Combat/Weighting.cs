using System;
using System.Linq;
using Trinity.Combat.Abilities;
using Trinity.Config.Combat;
using Trinity.DbProvider;
using Trinity.Technicals;
using Trinity.XmlTags;
using Zeta.Bot;
using Zeta.Bot.Profile.Common;
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
        private static double GetLastHadUnitsInSights()
        {
            return Math.Max(DateTime.UtcNow.Subtract(lastHadUnitInSights).TotalMilliseconds, DateTime.UtcNow.Subtract(lastHadEliteUnitInSights).TotalMilliseconds);
        }

        private static void RefreshDiaGetWeights()
        {
            using (new PerformanceLogger("RefreshDiaObjectCache.Weighting"))
            {
                double MovementSpeed = PlayerMover.GetMovementSpeed();

                bool noGoblinsPresent = (!AnyTreasureGoblinsPresent && Settings.Combat.Misc.GoblinPriority >= GoblinPriority.Prioritize) || Settings.Combat.Misc.GoblinPriority < GoblinPriority.Prioritize;


                bool prioritizeCloseRangeUnits = (ForceCloseRangeTarget || Player.IsRooted || DateTime.UtcNow.Subtract(PlayerMover.LastRecordedAnyStuck).TotalMilliseconds < 1000 &&
                    ObjectCache.Count(u => u.IsUnit && u.RadiusDistance < 10f) >= 3);

                bool hasWrathOfTheBerserker = Player.ActorClass == ActorClass.Barbarian && GetHasBuff(SNOPower.Barbarian_WrathOfTheBerserker);

                int TrashMobCount = ObjectCache.Count(u => u.IsUnit && u.IsTrashMob);
                int EliteCount = CombatBase.IgnoringElites ? 0 : ObjectCache.Count(u => u.IsUnit && u.IsBossOrEliteRareUnique);
                int AvoidanceCount = Settings.Combat.Misc.AvoidAOE ? 0 : ObjectCache.Count(o => o.Type == GObjectType.Avoidance && o.CentreDistance <= 50f);

                bool profileTagCheck = false;

                string behaviorName = "";
                if (ProfileManager.CurrentProfileBehavior != null)
                {
                    Type behaviorType = ProfileManager.CurrentProfileBehavior.GetType();
                    behaviorName = behaviorType.Name;
                    if (!Settings.Combat.Misc.ProfileTagOverride && CombatBase.IsQuestingMode || behaviorType == typeof(WaitTimerTag) || behaviorType == typeof(UseTownPortalTag) || behaviorType == typeof(XmlTags.TrinityTownRun) || behaviorType == typeof(XmlTags.TrinityTownPortal))
                    {
                        profileTagCheck = true;
                    }
                }

                bool ShouldIgnoreElites =
                     !CombatBase.IsQuestingMode &&
                     !DataDictionary.QuestLevelAreaIds.Contains(Player.LevelAreaId) &&
                     !profileTagCheck &&
                     !XmlTags.TrinityTownPortal.ForceClearArea &&
                     !TownRun.IsTryingToTownPortal() &&
                     CombatBase.IgnoringElites;

                Logger.Log(TrinityLogLevel.Debug, LogCategory.Weight,
                    "Starting weights: packSize={0} packRadius={1} MovementSpeed={2:0.0} Elites={3} AoEs={4} disableIgnoreTag={5} ({6}) closeRangePriority={7} townRun={8} forceClear={9} questingArea={10} level={11} isQuestingMode={12}",
                    Settings.Combat.Misc.TrashPackSize, Settings.Combat.Misc.TrashPackClusterRadius, MovementSpeed, EliteCount, AvoidanceCount, profileTagCheck, behaviorName,
                    prioritizeCloseRangeUnits, TownRun.IsTryingToTownPortal(), TrinityTownPortal.ForceClearArea, DataDictionary.QuestLevelAreaIds.Contains(Player.LevelAreaId), Player.Level, CombatBase.IsQuestingMode);

                if (TrinityCombatIgnore.IgnoreList.Any())
                    Logger.LogDebug(LogCategory.Weight, " CombatIgnoreList={0}", Logger.ListToString(TrinityCombatIgnore.IgnoreList.ToList<object>()));

                bool inQuestArea = DataDictionary.QuestLevelAreaIds.Contains(Player.LevelAreaId);
                bool usingTownPortal = TownRun.IsTryingToTownPortal();
                foreach (TrinityCacheObject cacheObject in ObjectCache.OrderBy(c => c.CentreDistance))
                {
                    bool elitesInRangeOfUnit = !CombatBase.IgnoringElites &&
                        ObjectCache.Any(u => u.ACDGuid != cacheObject.ACDGuid && u.IsEliteRareUnique && u.Position.Distance2D(cacheObject.Position) <= 25f);

                    bool shouldIgnoreTrashMobs =
                        !CombatBase.IsQuestingMode &&
                        !inQuestArea &&
                        !XmlTags.TrinityTownPortal.ForceClearArea &&
                        !usingTownPortal &&
                        !profileTagCheck &&
                        MovementSpeed > 1 &&
                        Settings.Combat.Misc.TrashPackSize > 1 &&
                        !elitesInRangeOfUnit &&
                        Player.Level >= 15 &&
                        Player.CurrentHealthPct > 0.10 &&
                        DateTime.UtcNow.Subtract(PlayerMover.LastRecordedAnyStuck).TotalMilliseconds > 500
                        ;

                    string objWeightInfo = "";

                    // Just to make sure each one starts at 0 weight...
                    cacheObject.Weight = 0d;

                    bool navBlocking = CacheData.NavigationObstacles.Any(ob => MathUtil.IntersectsPath(ob.Position, ob.Radius, Trinity.Player.Position, cacheObject.Position));

                    // Now do different calculations based on the object type
                    switch (cacheObject.Type)
                    {
                        // Weight Units
                        case GObjectType.Unit:
                            {
                                if (cacheObject.IsNPC && cacheObject.NPCIsOperable)
                                {
                                    cacheObject.Weight = 300;
                                    break;
                                }

                                int nearbyMonsterCount = ObjectCache.Count(u => u.ACDGuid != cacheObject.ACDGuid && u.IsTrashMob && u.HitPoints > 0 &&
                                    cacheObject.Position.Distance2D(u.Position) <= Settings.Combat.Misc.TrashPackClusterRadius);

                                bool isInHotSpot = GroupHotSpots.CacheObjectIsInHotSpot(cacheObject);

                                bool ignoring = false;

                                if (cacheObject.IsTrashMob)
                                {
                                    // Ignore trash mobs < 15% health or 50% health with a DoT
                                    if (cacheObject.IsTrashMob && shouldIgnoreTrashMobs &&
                                        (cacheObject.HitPointsPct < Settings.Combat.Misc.IgnoreTrashBelowHealth ||
                                         cacheObject.HitPointsPct < Settings.Combat.Misc.IgnoreTrashBelowHealthDoT && cacheObject.HasDotDPS) && !cacheObject.IsQuestMonster && !cacheObject.IsMinimapActive)
                                    {
                                        objWeightInfo = "Ignoring Health/DoT ";
                                        ignoring = true;
                                    }

                                    bool ignoreSummoner = true;
                                    if (cacheObject.IsSummoner && Settings.Combat.Misc.ForceKillSummoners && !navBlocking)
                                        ignoreSummoner = false;

                                    // Ignore Solitary Trash mobs (no elites present)
                                    // Except if has been primary target or if already low on health (<= 20%)
                                    if (shouldIgnoreTrashMobs && !isInHotSpot &&
                                        !(nearbyMonsterCount >= Settings.Combat.Misc.TrashPackSize) && ignoreSummoner && !cacheObject.IsQuestMonster && !cacheObject.IsMinimapActive)
                                    {
                                        objWeightInfo = "Ignoring ";
                                        ignoring = true;
                                    }
                                    else
                                    {
                                        objWeightInfo = "Adding ";
                                    }
                                    objWeightInfo += String.Format("nearbyCount={0} radiusDistance={1:0} hotspot={2} ShouldIgnore={3} elitesInRange={4} hitPointsPc={5:0.0} summoner={6} quest:{7} minimap={8}",
                                        nearbyMonsterCount, cacheObject.RadiusDistance, isInHotSpot, shouldIgnoreTrashMobs, elitesInRangeOfUnit, cacheObject.HitPointsPct, ignoreSummoner, cacheObject.IsQuestMonster, cacheObject.IsMinimapActive);

                                    if (ignoring)
                                        break;
                                }

                                if (cacheObject.IsEliteRareUnique)
                                {
                                    // Ignore elite option, except if trying to town portal
                                    if (!cacheObject.IsBoss && ShouldIgnoreElites && cacheObject.IsEliteRareUnique && !isInHotSpot &&
                                        !(cacheObject.HitPointsPct <= (Settings.Combat.Misc.ForceKillElitesHealth / 100)))
                                    {
                                        objWeightInfo = "Ignoring ";
                                        ignoring = true;
                                    }
                                    else if (cacheObject.IsEliteRareUnique && !ShouldIgnoreElites)
                                    {
                                        objWeightInfo = "Adding ";
                                    }
                                    objWeightInfo += String.Format("shouldIgnore={0} hitPointsPct={1}", ShouldIgnoreElites, cacheObject.HitPointsPct);

                                    if (ignoring)
                                        break;
                                }

                                // Monster on Combat Ignore list
                                if (!usingTownPortal && !profileTagCheck && TrinityCombatIgnore.IgnoreList.Any(u => u.ActorSNO == cacheObject.ActorSNO && !u.ShouldAttack(cacheObject)))
                                {
                                    objWeightInfo += " CombatIgnore";
                                }

                                // Monster is in cache but not within kill range
                                if (!cacheObject.IsBoss && cacheObject.RadiusDistance > cacheObject.KillRange)
                                {
                                    if (cacheObject.Weight <= 0)
                                        break;
                                }

                                if (cacheObject.HitPoints <= 0)
                                {
                                    break;
                                }

                                if (cacheObject.RadiusDistance <= 25f && !bAnyNonWWIgnoreMobsInRange && !DataDictionary.WhirlwindIgnoreSNOIds.Contains(cacheObject.ActorSNO))
                                {
                                    bAnyNonWWIgnoreMobsInRange = true;
                                }

                                // Force a close range target because we seem to be stuck *OR* if not ranged and currently rooted
                                if (prioritizeCloseRangeUnits)
                                {
                                    cacheObject.Weight = Math.Max((50 - cacheObject.RadiusDistance) / 50 * 2000d, 2d);

                                    // Goblin priority KAMIKAZEEEEEEEE
                                    if (cacheObject.IsTreasureGoblin && Settings.Combat.Misc.GoblinPriority == GoblinPriority.Kamikaze)
                                        cacheObject.Weight += 25000;
                                }
                                else
                                {

                                    // Not attackable, could be shielded, make super low priority
                                    if (cacheObject.HasAffixShielded && cacheObject.Unit.IsInvulnerable)
                                    {
                                        // Only 100 weight helps prevent it being prioritized over an unshielded
                                        cacheObject.Weight = 100;
                                    }
                                    // Not forcing close-ranged targets from being stuck, so let's calculate a weight!
                                    else
                                    {
                                        // Elites/Bosses that are killed should have weight erased so we don't keep attacking
                                        if ((cacheObject.IsEliteRareUnique || cacheObject.IsBoss) && cacheObject.HitPointsPct <= 0)
                                        {
                                            cacheObject.Weight = 0;
                                            break;
                                        }

                                        // Starting weight of 500
                                        if (cacheObject.IsTrashMob)
                                            cacheObject.Weight = Math.Max((CurrentBotKillRange - cacheObject.RadiusDistance) / CurrentBotKillRange * 500d, 2d);

                                        // Starting weight of 1000 for elites
                                        if (cacheObject.IsBossOrEliteRareUnique)
                                            cacheObject.Weight = Math.Max((90f - cacheObject.RadiusDistance) / 90f * 2000d, 20d);

                                        // Elites with Archon get super weight
                                        if (!CombatBase.IgnoringElites && Player.ActorClass == ActorClass.Wizard && GetHasBuff(SNOPower.Wizard_Archon) && cacheObject.IsBossOrEliteRareUnique)
                                        {
                                            cacheObject.Weight += 10000d;
                                        }


                                        // Monsters near players given higher weight
                                        if (cacheObject.Weight > 0)
                                        {
                                            foreach (var player in ObjectCache.Where(p => p.Type == GObjectType.Player && p.ACDGuid != Player.ACDGuid))
                                            {
                                                cacheObject.Weight += Math.Max(((55f - cacheObject.Position.Distance2D(player.Position)) / 55f * 500d), 2d);
                                            }
                                        }

                                        // Is standing in HotSpot - focus fire!
                                        if (isInHotSpot)
                                        {
                                            cacheObject.Weight += 10000d;
                                        }

                                        // Give extra weight to ranged enemies
                                        if ((Player.ActorClass == ActorClass.Barbarian || Player.ActorClass == ActorClass.Monk) &&
                                            (cacheObject.MonsterSize == MonsterSize.Ranged || DataDictionary.RangedMonsterIds.Contains(c_ActorSNO)))
                                        {
                                            cacheObject.Weight += 1100d;
                                            cacheObject.ForceLeapAgainst = true;
                                        }

                                        // Lower health gives higher weight - health is worth up to 1000ish extra weight
                                        if (cacheObject.IsTrashMob && cacheObject.HitPointsPct < 0.20)
                                            cacheObject.Weight += Math.Max((1 - cacheObject.HitPointsPct) / 100 * 1000d, 100);

                                        // Elites on low health get extra priority - up to 2500ish
                                        if (cacheObject.IsBossOrEliteRareUnique && cacheObject.HitPointsPct < 0.20)
                                            cacheObject.Weight += Math.Max((1 - cacheObject.HitPointsPct) / 100 * 2500d, 100);

                                        // Goblins on low health get extra priority - up to 2000ish
                                        if (Settings.Combat.Misc.GoblinPriority >= GoblinPriority.Prioritize && cacheObject.IsTreasureGoblin && cacheObject.HitPointsPct <= 0.98)
                                            cacheObject.Weight += Math.Max((1 - cacheObject.HitPointsPct) / 100 * 2000d, 100);

                                        // Bonuses to priority type monsters from the dictionary/hashlist set at the top of the code
                                        int extraPriority;
                                        if (DataDictionary.MonsterCustomWeights.TryGetValue(cacheObject.ActorSNO, out extraPriority))
                                        {
                                            // adding a constant multiple of 3 to all weights here (e.g. 999 becomes 1998)
                                            cacheObject.Weight += extraPriority * 2d;
                                        }

                                        // Extra weight for summoners
                                        if (cacheObject.IsSummoner)
                                        {
                                            cacheObject.Weight += 2500;
                                        }

                                        // Close range get higher weights the more of them there are, to prevent body-blocking
                                        if (!cacheObject.IsBoss && cacheObject.RadiusDistance <= 10f)
                                        {
                                            cacheObject.Weight += (3000d * cacheObject.Radius);
                                        }

                                        // Special additional weight for corrupt growths in act 4 ONLY if they are at close range (not a standard priority thing)
                                        if ((cacheObject.ActorSNO == 210120 || cacheObject.ActorSNO == 210268) && cacheObject.CentreDistance <= 25f)
                                            cacheObject.Weight += 2000d;

                                        // Was already a target and is still viable, give it some free extra weight, to help stop flip-flopping between two targets
                                        if (cacheObject.RActorGuid == LastTargetRactorGUID && cacheObject.CentreDistance <= 25f)
                                            cacheObject.Weight += 1000d;

                                        //if (ObjectCache.Any(u => MathEx.IntersectsPath(u.Position, u.Radius, Trinity.Player.Position,
                                        //                cacheObject.Position)))
                                        //    cacheObject.Weight *= 0.10d;

                                        // Prevent going less than 300 yet to prevent annoyances (should only lose this much weight from priority reductions in priority list?)
                                        if (cacheObject.Weight < 300)
                                            cacheObject.Weight = 300d;

                                        // If standing Molten, Arcane, or Poison Tree near unit, reduce weight
                                        if (PlayerKiteDistance <= 0 &&
                                            CacheData.TimeBoundAvoidance.Any(aoe =>
                                            (aoe.AvoidanceType == AvoidanceType.Arcane ||
                                            aoe.AvoidanceType == AvoidanceType.MoltenCore ||
                                                //aoe.AvoidanceType == AvoidanceType.MoltenTrail ||
                                            aoe.AvoidanceType == AvoidanceType.PoisonTree) &&
                                            cacheObject.Position.Distance2D(aoe.Position) <= aoe.Radius))
                                            cacheObject.Weight = 1;

                                        // If any AoE between us and target, reduce weight, for melee only
                                        if (!Settings.Combat.Misc.KillMonstersInAoE &&
                                            PlayerKiteDistance <= 0 && cacheObject.RadiusDistance > 3f &&
                                            CacheData.TimeBoundAvoidance.Any(aoe => aoe.AvoidanceType != AvoidanceType.PlagueCloud &&
                                                MathUtil.IntersectsPath(aoe.Position, aoe.Radius, Player.Position, cacheObject.Position)))
                                            cacheObject.Weight = 1;

                                        // See if there's any AOE avoidance in that spot, if so reduce the weight to 1, for melee only
                                        if (!Settings.Combat.Misc.KillMonstersInAoE &&
                                            PlayerKiteDistance <= 0 && cacheObject.RadiusDistance > 3f &&
                                            CacheData.TimeBoundAvoidance.Any(aoe => aoe.AvoidanceType != AvoidanceType.PlagueCloud &&
                                                cacheObject.Position.Distance2D(aoe.Position) <= aoe.Radius))
                                            cacheObject.Weight = 1d;

                                        if (PlayerKiteDistance > 0)
                                        {
                                            if (ObjectCache.Any(m => m.IsUnit &&
                                                MathUtil.IntersectsPath(cacheObject.Position, cacheObject.Radius, Player.Position, m.Position) &&
                                                m.RActorGuid != cacheObject.RActorGuid))
                                            {
                                                cacheObject.Weight = 1d;
                                            }
                                        }

                                        // Deal with treasure goblins - note, of priority is set to "0", then the is-a-goblin flag isn't even set for use here - the monster is ignored
                                        if (cacheObject.IsTreasureGoblin && !ObjectCache.Any(u => (u.Type == GObjectType.Door || u.Type == GObjectType.Barricade) && u.RadiusDistance <= 40f))
                                        {
                                            // Logging goblin sightings
                                            if (lastGoblinTime == DateTime.MinValue)
                                            {
                                                iTotalNumberGoblins++;
                                                lastGoblinTime = DateTime.UtcNow;
                                                Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "Goblin #{0} in sight. Distance={1:0}", iTotalNumberGoblins, cacheObject.CentreDistance);
                                            }
                                            else
                                            {
                                                if (DateTime.UtcNow.Subtract(lastGoblinTime).TotalMilliseconds > 30000)
                                                    lastGoblinTime = DateTime.MinValue;
                                            }

                                            if (CacheData.TimeBoundAvoidance.Any(aoe => cacheObject.Position.Distance2D(aoe.Position) <= aoe.Radius) && Settings.Combat.Misc.GoblinPriority != GoblinPriority.Kamikaze)
                                            {
                                                cacheObject.Weight = 1;
                                                break;
                                            }

                                            // Original Trinity stuff for priority handling now
                                            switch (Settings.Combat.Misc.GoblinPriority)
                                            {
                                                case GoblinPriority.Normal:
                                                    // Treating goblins as "normal monsters". Ok so I lied a little in the config, they get a little extra weight really! ;)
                                                    cacheObject.Weight += 751;
                                                    break;
                                                case GoblinPriority.Prioritize:
                                                    // Super-high priority option below... 
                                                    cacheObject.Weight += 5000;
                                                    break;
                                                case GoblinPriority.Kamikaze:
                                                    // KAMIKAZE SUICIDAL TREASURE GOBLIN RAPE AHOY!
                                                    cacheObject.Weight += 20000;
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
                                if (cacheObject.CentreDistance <= V.F("Cache.HotSpot.MinDistance"))
                                {
                                    break;
                                }
                                else if (!CacheData.TimeBoundAvoidance.Any(aoe => aoe.Position.Distance2D(cacheObject.Position) <= aoe.Radius))
                                {
                                    float maxDist = V.F("Cache.HotSpot.MaxDistance");
                                    cacheObject.Weight = (maxDist - cacheObject.CentreDistance) / maxDist * 50000d;
                                }
                                break;
                            }
                        case GObjectType.Item:
                        case GObjectType.Gold:
                            {
                                if (navBlocking)
                                {
                                    objWeightInfo += " NavBlocking";
                                    cacheObject.Weight = 0;
                                    break;
                                }

                                // Weight Items

                                // We'll weight them based on distance, giving gold less weight and close objects more
                                //if (cacheObject.GoldAmount > 0)
                                //    cacheObject.Weight = 5000d - (Math.Floor(cacheObject.CentreDistance) * 2000d);
                                //else
                                //    cacheObject.Weight = 8000d - (Math.Floor(cacheObject.CentreDistance) * 1900d);

                                // ignore non-legendaries and gold near elites if we're ignoring elites
                                // not sure how we should safely determine this distance
                                if (cacheObject.ItemQuality < ItemQuality.Legendary && ((CombatBase.IgnoringElites &&
                                    ObjectCache.Any(u => u.IsUnit && u.IsEliteRareUnique &&
                                        u.Position.Distance2D(cacheObject.Position) <= V.F("Weight.Items.IgnoreNonLegendaryNearEliteDistance"))) ||
                                    CacheData.TimeBoundAvoidance.Any(aoe => cacheObject.Position.Distance2D(aoe.Position) <= aoe.Radius)))
                                {
                                    cacheObject.Weight = 0;
                                    break;
                                }

                                // Ignore Legendaries in AoE
                                if (Settings.Loot.Pickup.IgnoreLegendaryInAoE && cacheObject.Type == GObjectType.Item && cacheObject.ItemQuality >= ItemQuality.Legendary &&
                                    CacheData.TimeBoundAvoidance.Any(aoe => cacheObject.Position.Distance2D(aoe.Position) <= aoe.Radius))
                                {
                                    cacheObject.Weight = 0;
                                    break;
                                }

                                // Ignore Non-Legendaries in AoE
                                if (Settings.Loot.Pickup.IgnoreNonLegendaryInAoE && cacheObject.Type == GObjectType.Item && cacheObject.ItemQuality < ItemQuality.Legendary &&
                                    CacheData.TimeBoundAvoidance.Any(aoe => cacheObject.Position.Distance2D(aoe.Position) <= aoe.Radius))
                                {
                                    cacheObject.Weight = 0;
                                    break;
                                }

                                // Ignore Legendaries near Elites
                                if (Settings.Loot.Pickup.IgnoreLegendaryNearElites && cacheObject.Type == GObjectType.Item && cacheObject.ItemQuality >= ItemQuality.Legendary &&
                                    ObjectCache.Any(u => u.IsEliteRareUnique && u.Position.Distance2D(cacheObject.Position) <= V.F("Weight.Items.IgnoreLegendaryNearEliteDistance")))
                                {
                                    cacheObject.Weight = 0;
                                    break;
                                }
                                // Ignore Non-Legendaries near Elites
                                if (Settings.Loot.Pickup.IgnoreNonLegendaryNearElites && cacheObject.Type == GObjectType.Item && cacheObject.ItemQuality < ItemQuality.Legendary &&
                                    ObjectCache.Any(u => u.IsEliteRareUnique && u.Position.Distance2D(cacheObject.Position) <= V.F("Weight.Items.IgnoreNonLegendaryNearEliteDistance")))
                                {
                                    cacheObject.Weight = 0;
                                    break;
                                }

                                // Ignore gold near Elites
                                if (Settings.Loot.Pickup.IgnoreGoldNearElites && cacheObject.Type == GObjectType.Gold &&
                                    ObjectCache.Any(u => u.IsEliteRareUnique && u.Position.Distance2D(cacheObject.Position) <= V.F("Weight.Items.IgnoreGoldNearEliteDistance")))
                                {
                                    cacheObject.Weight = 0;
                                    break;
                                }

                                // Ignore gold in AoE
                                if (Settings.Loot.Pickup.IgnoreGoldInAoE && cacheObject.Type == GObjectType.Gold &&
                                    CacheData.TimeBoundAvoidance.Any(aoe => cacheObject.Position.Distance2D(aoe.Position) <= aoe.Radius))
                                {
                                    cacheObject.Weight = 0;
                                    break;
                                }

                                if (cacheObject.GoldAmount > 0)
                                    cacheObject.Weight = (300 - cacheObject.CentreDistance) / 300 * 9000d;
                                else
                                    cacheObject.Weight = (300 - cacheObject.CentreDistance) / 300 * 9000d;


                                // Point-blank items get a weight increase 
                                if (cacheObject.GoldAmount <= 0 && cacheObject.CentreDistance <= 12f)
                                    cacheObject.Weight += 1000d;

                                // Was already a target and is still viable, give it some free extra weight, to help stop flip-flopping between two targets
                                if (cacheObject.RActorGuid == LastTargetRactorGUID)
                                    cacheObject.Weight += 800;

                                // Give yellows more weight
                                if (cacheObject.GoldAmount <= 0 && cacheObject.ItemQuality >= ItemQuality.Rare4)
                                    cacheObject.Weight += 4000d;

                                // Give legendaries more weight
                                if (cacheObject.GoldAmount <= 0 && cacheObject.ItemQuality >= ItemQuality.Legendary)
                                    cacheObject.Weight += 15000d;

                                // Are we prioritizing close-range stuff atm? If so limit it at a value 3k lower than monster close-range priority
                                //if (PrioritizeCloseRangeUnits)
                                //    cacheObject.Weight = (200f - cacheObject.CentreDistance) / 200f * 18000d;

                                if (Player.ActorClass == ActorClass.Monk && TimeSinceUse(SNOPower.Monk_TempestRush) < 1000 && cacheObject.ItemQuality < ItemQuality.Legendary)
                                {
                                    cacheObject.Weight = 500;
                                }

                                // If there's a monster in the path-line to the item, reduce the weight to 1, except legendaries
                                if (CacheData.MonsterObstacles.Any(cp => MathUtil.IntersectsPath(cp.Position, cp.Radius * 1.2f, Player.Position, cacheObject.Position)))
                                    cacheObject.Weight = 1;

                                // ignore any items/gold if there is mobs in kill radius and we aren't combat looting
                                if (CurrentTarget != null && AnyMobsInRange && cacheObject.ItemQuality < ItemQuality.Legendary)
                                    cacheObject.Weight = 1;

                                // See if there's any AOE avoidance in that spot or inbetween us, if so reduce the weight to 1
                                if (CacheData.TimeBoundAvoidance.Any(aoe => cacheObject.Position.Distance2D(aoe.Position) <= aoe.Radius))
                                    cacheObject.Weight = 1;

                                break;
                            }
                        case GObjectType.PowerGlobe:
                            {
                                if (!TownRun.IsTryingToTownPortal())
                                {
                                    cacheObject.Weight = (90f - cacheObject.RadiusDistance) / 90f * 5000d;
                                }

                                // Point-blank items get a weight increase 
                                if (cacheObject.GoldAmount <= 0 && cacheObject.CentreDistance <= 12f)
                                    cacheObject.Weight += 1000d;

                                // If there's a monster in the path-line to the item, reduce the weight to 1
                                if (CacheData.MonsterObstacles.Any(cp => MathUtil.IntersectsPath(cp.Position, cp.Radius * 1.2f, Player.Position, cacheObject.Position)))
                                    cacheObject.Weight = 1;

                                // See if there's any AOE avoidance in that spot or inbetween us, if so reduce the weight to 1
                                if (CacheData.TimeBoundAvoidance.Any(aoe => cacheObject.Position.Distance2D(aoe.Position) <= aoe.Radius))
                                    cacheObject.Weight = 1;

                                if (navBlocking)
                                {
                                    objWeightInfo += " NavBlocking";
                                    cacheObject.Weight = 0;
                                    break;
                                }

                                break;
                            }
                        case GObjectType.HealthGlobe:
                            {
                                // Calculate a spot reaching a little bit further out from the globe, to help globe-movements
                                if (cacheObject.Weight > 0)
                                    cacheObject.Position = MathEx.CalculatePointFrom(cacheObject.Position, Player.Position, cacheObject.CentreDistance + 3f);

                                // Weight Health Globes

                                bool witchDoctorManaLow =
                                    Player.ActorClass == ActorClass.Witchdoctor &&
                                    Player.PrimaryResourcePct <= 0.15 &&
                                    ZetaDia.CPlayer.PassiveSkills.Contains(SNOPower.Witchdoctor_Passive_GruesomeFeast);

                                if ((Player.CurrentHealthPct >= 1 || !Settings.Combat.Misc.CollectHealthGlobe))
                                {
                                    cacheObject.Weight = 0;
                                }
                                // Give all globes super low weight if we don't urgently need them, but are not 100% health
                                else if (!witchDoctorManaLow && (Player.CurrentHealthPct > PlayerEmergencyHealthGlobeLimit))
                                {
                                    double myHealth = Player.CurrentHealthPct;

                                    double minPartyHealth = 1d;
                                    if (ObjectCache.Any(p => p.Type == GObjectType.Player && p.RActorGuid != Player.RActorGuid))
                                        minPartyHealth = ObjectCache.Where(p => p.Type == GObjectType.Player && p.RActorGuid != Player.RActorGuid).Min(p => p.HitPointsPct);

                                    if (myHealth > 0d && myHealth < V.D("Weight.Globe.MinPlayerHealthPct"))
                                        cacheObject.Weight = (1d - myHealth) * 1000d;

                                    // Added weight for lowest health of party member
                                    if (minPartyHealth > 0d && minPartyHealth < V.D("Weight.Globe.MinPartyHealthPct"))
                                        cacheObject.Weight = (1d - minPartyHealth) * 2500d;
                                }
                                else
                                {
                                    // Ok we have globes enabled, and our health is low
                                    cacheObject.Weight = (90f - cacheObject.RadiusDistance) / 90f * 17000d;

                                    if (witchDoctorManaLow)
                                        cacheObject.Weight += 10000d; // 10k for WD's!

                                    // Point-blank items get a weight increase
                                    if (cacheObject.CentreDistance <= 15f)
                                        cacheObject.Weight += 3000d;

                                    // Close items get a weight increase
                                    if (cacheObject.CentreDistance <= 60f)
                                        cacheObject.Weight += 1500d;

                                    // Was already a target and is still viable, give it some free extra weight, to help stop flip-flopping between two targets
                                    if (cacheObject.RActorGuid == LastTargetRactorGUID && cacheObject.CentreDistance <= 25f)
                                        cacheObject.Weight += 800;
                                }

                                // If there's a monster in the path-line to the item, reduce the weight by 15% for each
                                Vector3 point = cacheObject.Position;
                                foreach (CacheObstacleObject tempobstacle in CacheData.MonsterObstacles.Where(cp =>
                                    MathUtil.IntersectsPath(cp.Position, cp.Radius, Player.Position, point)))
                                {
                                    cacheObject.Weight *= 0.85;
                                }

                                if (cacheObject.CentreDistance > 10f)
                                {
                                    // See if there's any AOE avoidance in that spot, if so reduce the weight by 10%
                                    if (CacheData.TimeBoundAvoidance.Any(cp => MathUtil.IntersectsPath(cp.Position, cp.Radius, Player.Position, cacheObject.Position)))
                                        cacheObject.Weight *= 0.9;

                                }

                                // do not collect health globes if we are kiting and health globe is too close to monster or avoidance
                                if (PlayerKiteDistance > 0)
                                {
                                    if (CacheData.MonsterObstacles.Any(m => m.Position.Distance(cacheObject.Position) < PlayerKiteDistance))
                                        cacheObject.Weight = 0;
                                    if (CacheData.TimeBoundAvoidance.Any(m => m.Position.Distance(cacheObject.Position) < PlayerKiteDistance))
                                        cacheObject.Weight = 0;
                                }

                                if (navBlocking)
                                {
                                    objWeightInfo += " NavBlocking";
                                    cacheObject.Weight = 0;
                                    break;
                                }

                                break;
                            }
                        case GObjectType.HealthWell:
                            {
                                if (!Settings.WorldObject.UseShrine)
                                    break;

                                if (CacheData.MonsterObstacles.Any(unit => MathUtil.IntersectsPath(unit.Position, unit.Radius, Player.Position, cacheObject.Position)))
                                    break;

                                if (CacheData.TimeBoundAvoidance.Any(aoe => MathUtil.IntersectsPath(aoe.Position, aoe.Radius, Player.Position, cacheObject.Position)))
                                    break;

                                if (prioritizeCloseRangeUnits)
                                    break;

                                if (MovementSpeed < 1)
                                    break;

                                // Current Health Percentage is higher than setting
                                if (Player.CurrentHealthPct * 100 > Settings.WorldObject.HealthWellMinHealth)
                                    break;

                                if (navBlocking)
                                {
                                    objWeightInfo += " NavBlocking";
                                    cacheObject.Weight = 0;
                                    break;
                                }


                                // As a percentage of health with typical maximum weight
                                cacheObject.Weight = 50000d * (1 - Trinity.Player.CurrentHealthPct);

                                break;
                            }
                        case GObjectType.Shrine:
                            {
                                // Weight Shrines
                                cacheObject.Weight = Math.Max(((75f - cacheObject.RadiusDistance) / 75f * 14500f), 100d);

                                // Very close shrines get a weight increase
                                if (cacheObject.CentreDistance <= 30f)
                                    cacheObject.Weight += 10000d;

                                if (cacheObject.Weight > 0)
                                {
                                    // Was already a target and is still viable, give it some free extra weight, to help stop flip-flopping between two targets
                                    if (cacheObject.RActorGuid == LastTargetRactorGUID && cacheObject.CentreDistance <= 25f)
                                        cacheObject.Weight += 400;

                                    // If there's a monster in the path-line to the item
                                    if (CacheData.MonsterObstacles.Any(cp => MathUtil.IntersectsPath(cp.Position, cp.Radius, Player.Position, cacheObject.Position)))
                                        cacheObject.Weight = 1;

                                    // See if there's any AOE avoidance in that spot, if so reduce the weight to 1
                                    if (CacheData.TimeBoundAvoidance.Any(cp => MathUtil.IntersectsPath(cp.Position, cp.Radius, Player.Position, cacheObject.Position)))
                                        cacheObject.Weight = 1;

                                    // if there's any monsters nearby
                                    if (TargetUtil.AnyMobsInRange(15f))
                                        cacheObject.Weight = 1;

                                    if (prioritizeCloseRangeUnits)
                                        cacheObject.Weight = 1;
                                }
                                break;
                            }
                        case GObjectType.Door:
                            {
                                // Ignore doors where units are blocking our LoS
                                if (ObjectCache.Any(u => u.IsUnit && u.HitPointsPct > 0 &&
                                        MathUtil.IntersectsPath(u.Position, u.Radius, Player.Position, cacheObject.Position)))
                                {
                                    cacheObject.Weight = 0;
                                    objWeightInfo += " Unitblocking";
                                    break;
                                }

                                // Prioritize doors where units are behind them
                                //if (!ObjectCache.Any(u => u.IsUnit && u.HitPointsPct > 0 &&
                                //    MathUtil.IntersectsPath(cacheObject.Position, cacheObject.Radius, Player.Position, u.Position)) && cacheObject.RadiusDistance <= 5f)
                                //{
                                //        cacheObject.Weight += 15000d;
                                //        objWeightInfo += " BlockingUnit";
                                //}

                                // Was already a target and is still viable, give it some free extra weight, to help stop flip-flopping between two targets
                                if (cacheObject.RActorGuid == LastTargetRactorGUID && cacheObject.CentreDistance <= 25f)
                                {
                                    cacheObject.Weight += 1000;
                                    objWeightInfo += " RePick";

                                }
                                // We're standing on the damn thing... open it!!
                                if (cacheObject.RadiusDistance <= 12f)
                                {
                                    cacheObject.Weight += 30000d;
                                    objWeightInfo += " <12f";
                                }
                                break;
                            }
                        case GObjectType.Barricade:
                            {
                                // rrrix added this as a single "weight" source based on the DestructableRange.
                                // Calculate the weight based on distance, where a distance = 1 is 5000, 90 = 0
                                cacheObject.Weight = (90f - cacheObject.RadiusDistance) / 90f * 5000f;

                                // Was already a target and is still viable, give it some free extra weight, to help stop flip-flopping between two targets
                                if (cacheObject.RActorGuid == LastTargetRactorGUID && cacheObject.CentreDistance <= 25f)
                                    cacheObject.Weight += 1000;

                                // We're standing on the damn thing... open it!!
                                if (cacheObject.RadiusDistance <= 12f)
                                    cacheObject.Weight += 30000d;
                            }
                            break;

                        case GObjectType.Destructible:
                            {

                                // Not Stuck, skip!
                                if (Settings.WorldObject.DestructibleOption == DestructibleIgnoreOption.OnlyIfStuck &&
                                    MovementSpeed > 1)
                                {
                                    break;
                                }

                                // Not stuck, skip
                                if (Settings.WorldObject.DestructibleOption == DestructibleIgnoreOption.OnlyIfStuck &&
                                    DateTime.UtcNow.Subtract(PlayerMover.LastRecordedAnyStuck).TotalMilliseconds > 500)
                                {
                                    break;
                                }

                                if (cacheObject.RadiusDistance > Settings.WorldObject.DestructibleRange &&
                                    (DateTime.UtcNow.Subtract(PlayerMover.LastRecordedAnyStuck).TotalMilliseconds > 500 || MovementSpeed > 1))
                                {
                                    break;
                                }

                                // rrrix added this as a single "weight" source based on the DestructableRange.
                                // Calculate the weight based on distance, where a distance = 1 is 5000, 90 = 0
                                cacheObject.Weight = (90f - cacheObject.RadiusDistance) / 90f * 5000f;

                                // Was already a target and is still viable, give it some free extra weight, to help stop flip-flopping between two targets
                                if (cacheObject.RActorGuid == LastTargetRactorGUID && cacheObject.CentreDistance <= 25f)
                                    cacheObject.Weight += 400;

                                //// Close destructibles get a weight increase
                                //if (cacheObject.CentreDistance <= 16f)
                                //    cacheObject.Weight += 1500d;

                                // If there's a monster in the path-line to the item, reduce the weight by 50%
                                if (CacheData.MonsterObstacles.Any(cp => MathUtil.IntersectsPath(cp.Position, cp.Radius, Player.Position, cacheObject.Position)))
                                    cacheObject.Weight *= 0.5;

                                // See if there's any AOE avoidance in that spot, if so reduce the weight to 1
                                if (CacheData.TimeBoundAvoidance.Any(cp => MathUtil.IntersectsPath(cp.Position, cp.Radius, Player.Position, cacheObject.Position)))
                                    cacheObject.Weight = 1;

                                // Are we prioritizing close-range stuff atm? If so limit it at a value 3k lower than monster close-range priority
                                if (prioritizeCloseRangeUnits)
                                    cacheObject.Weight = (200d - cacheObject.CentreDistance) / 200d * 19200d;

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
                                // Need to Prioritize, forget it!
                                if (prioritizeCloseRangeUnits)
                                    break;

                                // nearby monsters attacking us - don't try to use headtone
                                if (cacheObject.Object is DiaGizmo && cacheObject.Gizmo.CommonData.ActorInfo.GizmoType == GizmoType.Headstone && 
                                    ObjectCache.Any(u => u.IsUnit && u.RadiusDistance < 25f && u.IsFacingPlayer))
                                {
                                    cacheObject.Weight = 0;
                                    break;
                                }

                                // Weight Interactable Specials
                                cacheObject.Weight = (90d - cacheObject.CentreDistance) / 90d * 15000d;

                                // Very close interactables get a weight increase
                                if (cacheObject.CentreDistance <= 12f)
                                    cacheObject.Weight += 1000d;

                                // Was already a target and is still viable, give it some free extra weight, to help stop flip-flopping between two targets
                                if (cacheObject.RActorGuid == LastTargetRactorGUID && cacheObject.CentreDistance <= 25f)
                                    cacheObject.Weight += 400;

                                // If there's a monster in the path-line to the item, reduce the weight by 50%
                                if (CacheData.MonsterObstacles.Any(cp => MathUtil.IntersectsPath(cp.Position, cp.Radius, Player.Position, cacheObject.Position)))
                                    cacheObject.Weight *= 0.5;

                                // See if there's any AOE avoidance in that spot, if so reduce the weight to 1
                                if (CacheData.TimeBoundAvoidance.Any(cp => MathUtil.IntersectsPath(cp.Position, cp.Radius, Player.Position, cacheObject.Position)))
                                    cacheObject.Weight = 1;

                                //if (bAnyMobsInCloseRange || (CurrentTarget != null && CurrentTarget.IsBossOrEliteRareUnique))
                                //    cacheObject.Weight = 1;

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

                                if (navBlocking)
                                {
                                    objWeightInfo += " NavBlocking";
                                    cacheObject.Weight = 0;
                                    break;
                                }

                                // Weight Containers
                                cacheObject.Weight = (maxRange - cacheObject.CentreDistance) / maxRange * 1000d;

                                // Very close containers get a weight increase
                                if (cacheObject.CentreDistance <= 8f)
                                    cacheObject.Weight += 600d;

                                // Was already a target and is still viable, give it some free extra weight, to help stop flip-flopping between two targets
                                if (cacheObject.RActorGuid == LastTargetRactorGUID && cacheObject.CentreDistance <= 25f)
                                    cacheObject.Weight += 400;

                                // If there's a monster in the path-line to the item, reduce the weight by 50%
                                if (CacheData.MonsterObstacles.Any(cp => MathUtil.IntersectsPath(cp.Position, cp.Radius, Player.Position, cacheObject.Position)))
                                    cacheObject.Weight *= 0.5;

                                // See if there's any AOE avoidance in that spot, if so reduce the weight to 1
                                if (CacheData.TimeBoundAvoidance.Any(cp => MathUtil.IntersectsPath(cp.Position, cp.Radius, Player.Position, cacheObject.Position)))
                                    cacheObject.Weight = 1;
                                break;
                            }

                    }

                    // Force the character to stay where it is if there is nothing available that is out of avoidance stuff and we aren't already in avoidance stuff
                    if (cacheObject.Weight == 1 && !StandingInAvoidance && ObjectCache.Any(o => o.Type == GObjectType.Avoidance))
                    {
                        cacheObject.Weight = 0;
                        ShouldStayPutDuringAvoidance = true;
                    }


                    // Prevent current target dynamic ranged weighting flip-flop 
                    if (LastTargetRactorGUID == cacheObject.RActorGuid && cacheObject.Weight <= 1 && !navBlocking)
                    {
                        cacheObject.Weight = 100;
                    }

                    objWeightInfo += cacheObject.IsNPC ? " IsNPC" : "";
                    objWeightInfo += cacheObject.NPCIsOperable ? " IsOperable" : "";

                    Logger.Log(TrinityLogLevel.Debug, LogCategory.Weight,
                        "Weight={0:0} name={1} sno={2} type={3} R-Dist={4:0} IsElite={5} RAGuid={6} {7}",
                            cacheObject.Weight, cacheObject.InternalName, cacheObject.ActorSNO, cacheObject.Type, cacheObject.RadiusDistance, cacheObject.IsEliteRareUnique,
                            cacheObject.RActorGuid, objWeightInfo);

                    // Is the weight of this one higher than the current-highest weight? Then make this the new primary target!
                    if (cacheObject.Weight > w_HighestWeightFound && cacheObject.Weight > 0)
                    {
                        // Clone the current CacheObject
                        CurrentTarget = cacheObject.Copy();
                        w_HighestWeightFound = cacheObject.Weight;

                        // See if we can try attempting kiting later
                        NeedToKite = false;
                        KiteAvoidDestination = Vector3.Zero;

                        // Kiting and Avoidance
                        if (CurrentTarget.IsUnit)
                        {
                            var AvoidanceList = CacheData.TimeBoundAvoidance.Where(o =>
                                // Distance from avoidance to target is less than avoidance radius
                                o.Position.Distance(CurrentTarget.Position) <= (GetAvoidanceRadius(o.ActorSNO) * 1.2) &&
                                    // Distance from obstacle to me is <= cacheObject.RadiusDistance
                                o.Position.Distance(Player.Position) <= (cacheObject.RadiusDistance - 4f)
                                );

                            // if there's any obstacle within a specified distance of the avoidance radius *1.2 
                            if (AvoidanceList.Any())
                            {
                                foreach (CacheObstacleObject o in AvoidanceList)
                                {
                                    Logger.Log(TrinityLogLevel.Debug, LogCategory.Targetting, "Avoidance: Id={0} Weight={1} Loc={2} Radius={3} Name={4}", o.ActorSNO, o.Weight, o.Position, o.Radius, o.Name);
                                }

                                KiteAvoidDestination = CurrentTarget.Position;
                                NeedToKite = true;
                            }
                        }
                    }
                }

                // Loop through all the objects and give them a weight
                if (CurrentTarget != null && CurrentTarget.InternalName != null && CurrentTarget.ActorSNO > 0 && CurrentTarget.RActorGuid != LastTargetRactorGUID)
                {
                    RecordTargetHistory();

                    Logger.Log(TrinityLogLevel.Verbose,
                                    LogCategory.Targetting,
                                    "Target changed to name={2} sno={0} type={1} raGuid={3}",
                                    CurrentTarget.InternalName,
                                    CurrentTarget.ActorSNO,
                                    CurrentTarget.Type,
                                    CurrentTarget.RActorGuid);
                }
            }
        }

        private static void RecordTargetHistory()
        {
            string targetMd5Hash = HashGenerator.GenerateObjecthash(CurrentTarget);

            // clean up past targets
            if (!GenericCache.ContainsKey(targetMd5Hash))
            {
                CurrentTarget.HasBeenPrimaryTarget = true;
                CurrentTarget.TimesBeenPrimaryTarget = 1;
                CurrentTarget.FirstTargetAssignmentTime = DateTime.UtcNow;
                GenericCache.AddToCache(new GenericCacheObject(targetMd5Hash, CurrentTarget, new TimeSpan(0, 10, 0)));
            }
            else if (GenericCache.ContainsKey(targetMd5Hash))
            {
                TrinityCacheObject cTarget = (TrinityCacheObject)GenericCache.GetObject(targetMd5Hash).Value;
                bool isEliteLowHealth = cTarget.HitPointsPct <= 0.75 && cTarget.IsBossOrEliteRareUnique;
                bool isLegendaryItem = cTarget.Type == GObjectType.Item && cTarget.ItemQuality >= ItemQuality.Legendary;
                if (!cTarget.IsBoss && cTarget.TimesBeenPrimaryTarget > 100 && !isEliteLowHealth && !isLegendaryItem)
                {
                    Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "Blacklisting target {0} ActorSNO={1} RActorGUID={2} due to possible stuck/flipflop!",
                        CurrentTarget.InternalName, CurrentTarget.ActorSNO, CurrentTarget.RActorGuid);

                    hashRGUIDBlacklist60.Add(CurrentTarget.RActorGuid);

                    // Add to generic blacklist for safety, as the RActorGUID on items and gold can change as we move away and get closer to the items (while walking around corners)
                    // So we can't use any ID's but rather have to use some data which never changes (actorSNO, position, type, worldID)
                    GenericBlacklist.AddToBlacklist(new GenericCacheObject()
                    {
                        Key = CurrentTarget.ObjectHash,
                        Value = null,
                        Expires = DateTime.UtcNow.AddSeconds(60)
                    });
                }
                else
                {
                    cTarget.TimesBeenPrimaryTarget++;
                    GenericCache.UpdateObject(new GenericCacheObject(targetMd5Hash, cTarget, new TimeSpan(0, 10, 0)));
                }

            }
        }
    }
}
