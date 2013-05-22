using Trinity.Settings.Combat;
using Trinity.Technicals;
using System;
using System.Linq;
using Zeta.Common;
using Zeta.Common.Plugins;
using Zeta.Internals.Actors;
using Zeta.Internals.SNO;
using Zeta.TreeSharp;
using Zeta.CommonBot;
using Zeta.CommonBot.Profile.Common;
using Zeta;
using Trinity.DbProvider;
namespace Trinity
{
    public partial class Trinity : IPlugin
    {
        private static double GetLastHadUnitsInSights()
        {
            return Math.Max(DateTime.Now.Subtract(lastHadUnitInSights).TotalMilliseconds, DateTime.Now.Subtract(lastHadEliteUnitInSights).TotalMilliseconds);
        }

        private static void RefreshDiaGetWeights()
        {
            using (new PerformanceLogger("RefreshDiaObjectCache.Weighting"))
            {
                double MovementSpeed = PlayerMover.GetMovementSpeed();

                // Store if we are ignoring all units this cycle or not
                bool bIgnoreAllUnits = !AnyElitesPresent &&
                                        !AnyMobsInRange &&
                                        (
                                            (
                                                !AnyTreasureGoblinsPresent &&
                                                Settings.Combat.Misc.GoblinPriority >= GoblinPriority.Prioritize
                                            ) ||
                                            Settings.Combat.Misc.GoblinPriority < GoblinPriority.Prioritize
                                        ) &&
                                        PlayerStatus.CurrentHealthPct >= 0.85d;

                bool PrioritizeCloseRangeUnits = (ForceCloseRangeTarget || PlayerStatus.IsRooted || MovementSpeed < 1 || ObjectCache.Count(u => u.Type == GObjectType.Unit && u.RadiusDistance < 5f) >= 3);

                bool hasWrathOfTheBerserker = PlayerStatus.ActorClass == ActorClass.Barbarian && GetHasBuff(SNOPower.Barbarian_WrathOfTheBerserker);

                int TrashMobCount = ObjectCache.Count(u => u.Type == GObjectType.Unit && u.IsTrashMob);
                int EliteCount = Settings.Combat.Misc.IgnoreElites ? 0 : ObjectCache.Count(u => u.Type == GObjectType.Unit && u.IsBossOrEliteRareUnique);
                int AvoidanceCount = Settings.Combat.Misc.AvoidAOE ? 0 : ObjectCache.Count(o => o.Type == GObjectType.Avoidance && o.CentreDistance <= 50f);

                bool profileTagCheck = false;
                if (ProfileManager.CurrentProfileBehavior != null)
                {
                    Type behaviorType = ProfileManager.CurrentProfileBehavior.GetType();
                    if (behaviorType == typeof(WaitTimerTag) || behaviorType == typeof(UseTownPortalTag) || behaviorType == typeof(XmlTags.TrinityTownRun))
                    {
                        profileTagCheck = true;
                    }
                }

                bool ShouldIgnoreTrashMobs =
                    (!TownRun.IsTryingToTownPortal() &&
                    !profileTagCheck &&
                    !PrioritizeCloseRangeUnits &&
                    Settings.Combat.Misc.TrashPackSize > 1 &&
                    EliteCount == 0 &&
                    AvoidanceCount == 0 &&
                    PlayerStatus.Level >= 15 &&
                    MovementSpeed >= 1
                    );

                string unitWeightInfo = "";

                foreach (TrinityCacheObject cacheObject in ObjectCache.OrderBy(c => c.CentreDistance))
                {
                    unitWeightInfo = "";

                    // Just to make sure each one starts at 0 weight...
                    cacheObject.Weight = 0d;

                    // Now do different calculations based on the object type
                    switch (cacheObject.Type)
                    {
                        // Weight Units
                        case GObjectType.Unit:
                            {
                                int nearbyMonsterCount = ObjectCache.Count(u => u.IsTrashMob && cacheObject.Position.Distance2D(u.Position) <= Settings.Combat.Misc.TrashPackClusterRadius);

                                // Ignore Solitary Trash mobs (no elites present)
                                // Except if has been primary target or if already low on health (<= 20%)
                                if (ShouldIgnoreTrashMobs && cacheObject.IsTrashMob && !cacheObject.HasBeenPrimaryTarget && cacheObject.RadiusDistance >= 2f &&
                                    !(nearbyMonsterCount >= Settings.Combat.Misc.TrashPackSize))
                                {
                                    unitWeightInfo = String.Format("Ignoring trash mob {0} {1} nearbyCount={2} packSize={3} packRadius={4:0} radiusDistance={5:0} ShouldIgnore={6} ms={7:0.00} Elites={8} Avoid={9} profileTagCheck={10} level={11} prioritize={12}",
                                        cacheObject.InternalName, cacheObject.RActorGuid, nearbyMonsterCount, Settings.Combat.Misc.TrashPackSize, Settings.Combat.Misc.TrashPackClusterRadius,
                                        cacheObject.RadiusDistance, ShouldIgnoreTrashMobs, MovementSpeed, EliteCount, AvoidanceCount, profileTagCheck, PlayerStatus.Level, PrioritizeCloseRangeUnits);
                                    break;
                                }
                                else
                                {
                                    unitWeightInfo = String.Format("Adding trash mob {0} {1} nearbyCount={2} packSize={3} packRadius={4:0} radiusDistance={5:0} ShouldIgnore={6} ms={7:0.00} Elites={8} Avoid={9} profileTagCheck={10} level={11} prioritize={12}",
                                        cacheObject.InternalName, cacheObject.RActorGuid, nearbyMonsterCount, Settings.Combat.Misc.TrashPackSize, Settings.Combat.Misc.TrashPackClusterRadius,
                                        cacheObject.RadiusDistance, ShouldIgnoreTrashMobs, MovementSpeed, EliteCount, AvoidanceCount, profileTagCheck, PlayerStatus.Level, PrioritizeCloseRangeUnits);
                                }

                                // Ignore elite option, except if trying to town portal
                                if (Settings.Combat.Misc.IgnoreElites && (cacheObject.IsEliteRareUnique) && !TownRun.IsTryingToTownPortal())
                                {
                                    break;
                                }


                                // No champions, no mobs nearby, no treasure goblins to prioritize, and not injured, so skip mobs
                                if (bIgnoreAllUnits)
                                {
                                    break;
                                }

                                // Monster is in cache but not within kill range
                                if (cacheObject.RadiusDistance > cacheObject.KillRange)
                                {
                                    break;
                                }

                                if (cacheObject.HitPoints <= 0)
                                {
                                    break;
                                }

                                // Total up monsters at various ranges
                                if (cacheObject.RadiusDistance <= 50f)
                                {
                                    bool isElite = (cacheObject.IsEliteRareUnique || cacheObject.IsBoss);

                                    bool isRended = cacheObject.HasDotDPS;

                                    // Flag up any bosses in range
                                    if (cacheObject.IsBoss)
                                        anyBossesInRange = true;
                                    if (cacheObject.RadiusDistance <= 6f)
                                    {
                                        AnythingWithinRange[RANGE_6]++;
                                        if (isElite)
                                            ElitesWithinRange[RANGE_6]++;
                                    }
                                    if (cacheObject.RadiusDistance <= 9f && !isRended)
                                    {
                                        NonRendedTargets_9++;
                                    }
                                    if (cacheObject.RadiusDistance <= 12f)
                                    {
                                        AnythingWithinRange[RANGE_12]++;
                                        if (isElite)
                                            ElitesWithinRange[RANGE_12]++;
                                    }
                                    if (cacheObject.RadiusDistance <= 15f)
                                    {
                                        AnythingWithinRange[RANGE_15]++;
                                        if (isElite)
                                            ElitesWithinRange[RANGE_15]++;
                                    }
                                    if (cacheObject.RadiusDistance <= 20f)
                                    {
                                        AnythingWithinRange[RANGE_20]++;
                                        if (isElite)
                                            ElitesWithinRange[RANGE_20]++;
                                    }
                                    if (cacheObject.RadiusDistance <= 25f)
                                    {
                                        if (!bAnyNonWWIgnoreMobsInRange && !DataDictionary.WhirlwindIgnoreSNOIds.Contains(cacheObject.ActorSNO))
                                            bAnyNonWWIgnoreMobsInRange = true;
                                        AnythingWithinRange[RANGE_25]++;
                                        if (isElite)
                                            ElitesWithinRange[RANGE_25]++;
                                    }
                                    if (cacheObject.RadiusDistance <= 30f)
                                    {
                                        AnythingWithinRange[RANGE_30]++;
                                        if (isElite)
                                            ElitesWithinRange[RANGE_30]++;
                                    }
                                    if (cacheObject.RadiusDistance <= 40f)
                                    {
                                        AnythingWithinRange[RANGE_40]++;
                                        if (isElite)
                                            ElitesWithinRange[RANGE_40]++;
                                    }
                                    if (cacheObject.RadiusDistance <= 50f)
                                    {
                                        AnythingWithinRange[RANGE_50]++;
                                        if (isElite)
                                            ElitesWithinRange[RANGE_50]++;
                                    }
                                }

                                // Force a close range target because we seem to be stuck *OR* if not ranged and currently rooted
                                if (PrioritizeCloseRangeUnits)
                                {
                                    cacheObject.Weight = (50 - cacheObject.RadiusDistance) / 50 * 20000d;

                                    // Goblin priority KAMIKAZEEEEEEEE
                                    if (cacheObject.IsTreasureGoblin && Settings.Combat.Misc.GoblinPriority == GoblinPriority.Kamikaze)
                                        cacheObject.Weight += 25000;
                                }
                                else
                                {

                                    // Not attackable, could be shielded, make super low priority
                                    if (cacheObject.IsShielded)
                                    {
                                        // Only 500 weight helps prevent it being prioritized over an unshielded
                                        cacheObject.Weight = 500;
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


                                        // Starting weight of 5000
                                        if (cacheObject.IsTrashMob)
                                            cacheObject.Weight = (CurrentBotKillRange - cacheObject.RadiusDistance) / CurrentBotKillRange * 5000;

                                        // Starting weight of 8000 for elites
                                        if (cacheObject.IsBossOrEliteRareUnique)
                                            cacheObject.Weight = (90f - cacheObject.RadiusDistance) / 90f * 8000;

                                        // Give extra weight to ranged enemies
                                        if ((PlayerStatus.ActorClass == ActorClass.Barbarian || PlayerStatus.ActorClass == ActorClass.Monk) &&
                                            (cacheObject.MonsterStyle == MonsterSize.Ranged || DataDictionary.RangedMonsterIds.Contains(c_ActorSNO)))
                                        {
                                            cacheObject.Weight += 1100;
                                            cacheObject.ForceLeapAgainst = true;
                                        }

                                        // Lower health gives higher weight - health is worth up to 1000ish extra weight
                                        if (cacheObject.IsTrashMob && cacheObject.HitPointsPct < 0.20)
                                            cacheObject.Weight += (100 - cacheObject.HitPointsPct) / 100 * 1000;

                                        // Elites on low health get extra priority - up to 2500ish
                                        if (cacheObject.IsBossOrEliteRareUnique && cacheObject.HitPointsPct < 0.20)
                                            cacheObject.Weight += (100 - cacheObject.HitPointsPct) / 100 * 2500;

                                        // Goblins on low health get extra priority - up to 4000ish
                                        if (Settings.Combat.Misc.GoblinPriority >= GoblinPriority.Prioritize && cacheObject.IsTreasureGoblin && cacheObject.HitPointsPct <= 0.98)
                                            cacheObject.Weight += (100 - cacheObject.HitPointsPct) / 100 * 4000;

                                        // Bonuses to priority type monsters from the dictionary/hashlist set at the top of the code
                                        int iExtraPriority;
                                        if (DataDictionary.MonsterCustomWeights.TryGetValue(cacheObject.ActorSNO, out iExtraPriority))
                                        {
                                            cacheObject.Weight += iExtraPriority;
                                        }

                                        // Close range get higher weights the more of them there are, to prevent body-blocking
                                        if (cacheObject.RadiusDistance <= 5f)
                                        {
                                            cacheObject.Weight += (2000 * cacheObject.Radius);
                                        }

                                        // Special additional weight for corrupt growths in act 4 ONLY if they are at close range (not a standard priority thing)
                                        if ((cacheObject.ActorSNO == 210120 || cacheObject.ActorSNO == 210268) && cacheObject.CentreDistance <= 25f)
                                            cacheObject.Weight += 2000;

                                        // Was already a target and is still viable, give it some free extra weight, to help stop flip-flopping between two targets
                                        if (cacheObject.RActorGuid == CurrentTargetRactorGUID && cacheObject.CentreDistance <= 25f)
                                            cacheObject.Weight += 1000;

                                        // Prevent going less than 300 yet to prevent annoyances (should only lose this much weight from priority reductions in priority list?)
                                        if (cacheObject.Weight < 300)
                                            cacheObject.Weight = 300;

                                        // If any AoE between us and target, do not attack, for non-ranged attacks only
                                        if (!Settings.Combat.Misc.KillMonstersInAoE &&
                                            PlayerKiteDistance <= 0 &&
                                            cacheObject.AvoidanceType != AvoidanceType.PlagueCloud &&
                                            hashAvoidanceObstacleCache.Any(o => MathUtil.IntersectsPath(o.Location, o.Radius, PlayerStatus.CurrentPosition, cacheObject.Position)))
                                            cacheObject.Weight = 1;

                                        // See if there's any AOE avoidance in that spot, if so reduce the weight to 1, for non-ranged attacks only
                                        if (!Settings.Combat.Misc.KillMonstersInAoE &&
                                            PlayerKiteDistance <= 0 &&
                                            cacheObject.AvoidanceType != AvoidanceType.PlagueCloud &&
                                            hashAvoidanceObstacleCache.Any(aoe => cacheObject.Position.Distance2D(aoe.Location) <= aoe.Radius))
                                            cacheObject.Weight = 1;

                                        if (PlayerKiteDistance > 0)
                                        {
                                            if (ObjectCache.Any(m => m.Type == GObjectType.Unit &&
                                                MathUtil.IntersectsPath(cacheObject.Position, cacheObject.Radius, PlayerStatus.CurrentPosition, m.Position) &&
                                                m.RActorGuid != cacheObject.RActorGuid))
                                            {
                                                cacheObject.Weight = 0;
                                            }
                                        }

                                        // Deal with treasure goblins - note, of priority is set to "0", then the is-a-goblin flag isn't even set for use here - the monster is ignored
                                        if (cacheObject.IsTreasureGoblin && !ObjectCache.Any(u => (u.Type == GObjectType.Door || u.Type == GObjectType.Barricade) && u.RadiusDistance <= 40f))
                                        {

                                            // Logging goblin sightings
                                            if (lastGoblinTime == DateTime.Today)
                                            {
                                                iTotalNumberGoblins++;
                                                lastGoblinTime = DateTime.Now;
                                                Logger.Log(TrinityLogLevel.Normal, LogCategory.UserInformation, "Goblin #{0} in sight. Distance={1:0}", iTotalNumberGoblins, cacheObject.CentreDistance);
                                            }
                                            else
                                            {
                                                if (DateTime.Now.Subtract(lastGoblinTime).TotalMilliseconds > 30000)
                                                    lastGoblinTime = DateTime.Today;
                                            }

                                            if (hashAvoidanceObstacleCache.Any(aoe => cacheObject.Position.Distance2D(aoe.Location) <= aoe.Radius) && Settings.Combat.Misc.GoblinPriority != GoblinPriority.Kamikaze)
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
                                                    cacheObject.Weight += 20000;
                                                    break;
                                                case GoblinPriority.Kamikaze:
                                                    // KAMIKAZE SUICIDAL TREASURE GOBLIN RAPE AHOY!
                                                    cacheObject.Weight += 40000;
                                                    break;

                                            }
                                        }
                                    }

                                    // Forcing close range target or not?
                                }

                                // This is an attackable unit
                                break;
                            }
                        case GObjectType.Item:
                        case GObjectType.Gold:
                            {
                                // Weight Items

                                // We'll weight them based on distance, giving gold less weight and close objects more
                                //if (cacheObject.GoldAmount > 0)
                                //    cacheObject.Weight = 5000d - (Math.Floor(cacheObject.CentreDistance) * 2000d);
                                //else
                                //    cacheObject.Weight = 8000d - (Math.Floor(cacheObject.CentreDistance) * 1900d);

                                if (cacheObject.GoldAmount > 0)
                                    cacheObject.Weight = (300 - cacheObject.CentreDistance) / 300 * 9000d;
                                else
                                    cacheObject.Weight = (300 - cacheObject.CentreDistance) / 300 * 9000d;


                                // Point-blank items get a weight increase 
                                if (cacheObject.GoldAmount <= 0 && cacheObject.CentreDistance <= 12f)
                                    cacheObject.Weight += 1000d;

                                // Was already a target and is still viable, give it some free extra weight, to help stop flip-flopping between two targets
                                if (cacheObject.RActorGuid == CurrentTargetRactorGUID)
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

                                if (PlayerStatus.ActorClass == ActorClass.Monk && TimeSinceUse(SNOPower.Monk_TempestRush) < 1000 && cacheObject.ItemQuality < ItemQuality.Legendary)
                                {
                                    cacheObject.Weight = 500;
                                }

                                // If there's a monster in the path-line to the item, reduce the weight to 1, except legendaries
                                if (cacheObject.ItemQuality < ItemQuality.Legendary && hashMonsterObstacleCache.Any(cp => MathUtil.IntersectsPath(cp.Location, cp.Radius * 1.2f, PlayerStatus.CurrentPosition, cacheObject.Position)))
                                    cacheObject.Weight = 1;

                                // ignore any items/gold if there is mobs in kill radius and we aren't combat looting
                                if (CurrentTarget != null && AnyMobsInRange && !Zeta.CommonBot.Settings.CharacterSettings.Instance.CombatLooting && cacheObject.ItemQuality < ItemQuality.Legendary)
                                    cacheObject.Weight = 1;

                                // See if there's any AOE avoidance in that spot or inbetween us, if so reduce the weight to 1
                                if (hashAvoidanceObstacleCache.Any(aoe => cacheObject.Position.Distance2D(aoe.Location) <= aoe.Radius))
                                    cacheObject.Weight = 1;

                                // ignore non-legendaries and gold near elites if we're ignoring elites
                                // not sure how we should safely determine this distance
                                if (Settings.Combat.Misc.IgnoreElites && cacheObject.ItemQuality < ItemQuality.Legendary &&
                                    ObjectCache.Any(u => u.Type == GObjectType.Unit && u.IsEliteRareUnique && u.Position.Distance2D(cacheObject.Position) <= 40f))
                                {
                                    cacheObject.Weight = 0;
                                }

                                break;
                            }
                        case GObjectType.Globe:
                            {
                                // Weight Health Globes

                                // Give all globes 0 weight (so never gone-to), unless we have low health, then go for them
                                if (PlayerStatus.CurrentHealthPct > PlayerEmergencyHealthGlobeLimit || !Settings.Combat.Misc.CollectHealthGlobe)
                                {
                                    cacheObject.Weight = 0;
                                }
                                else
                                {

                                    // Ok we have globes enabled, and our health is low...!
                                    cacheObject.Weight = (300f - cacheObject.RadiusDistance) / 300f * 17000d;

                                    // Point-blank items get a weight increase
                                    if (cacheObject.CentreDistance <= 15f)
                                        cacheObject.Weight += 3000d;

                                    // Close items get a weight increase
                                    if (cacheObject.CentreDistance <= 60f)
                                        cacheObject.Weight += 1500d;

                                    // Was already a target and is still viable, give it some free extra weight, to help stop flip-flopping between two targets
                                    if (cacheObject.RActorGuid == CurrentTargetRactorGUID && cacheObject.CentreDistance <= 25f)
                                        cacheObject.Weight += 800;

                                    // If there's a monster in the path-line to the item, reduce the weight by 15% for each
                                    Vector3 point = cacheObject.Position;
                                    foreach (CacheObstacleObject tempobstacle in hashMonsterObstacleCache.Where(cp =>
                                        MathUtil.IntersectsPath(cp.Location, cp.Radius, PlayerStatus.CurrentPosition, point)))
                                    {
                                        cacheObject.Weight *= 0.85;
                                    }

                                    // See if there's any AOE avoidance in that spot, if so reduce the weight by 10%
                                    if (hashAvoidanceObstacleCache.Any(cp => MathUtil.IntersectsPath(cp.Location, cp.Radius, PlayerStatus.CurrentPosition, cacheObject.Position)))
                                        cacheObject.Weight *= 0.9;

                                    // Calculate a spot reaching a little bit further out from the globe, to help globe-movements
                                    if (cacheObject.Weight > 0)
                                        cacheObject.Position = MathEx.CalculatePointFrom(cacheObject.Position, PlayerStatus.CurrentPosition, cacheObject.CentreDistance + 3f);

                                    // do not collect health globes if we are kiting and health globe is too close to monster or avoidance
                                    if (PlayerKiteDistance > 0)
                                    {
                                        if (hashMonsterObstacleCache.Any(m => m.Location.Distance(cacheObject.Position) < PlayerKiteDistance))
                                            cacheObject.Weight = 0;
                                        if (hashAvoidanceObstacleCache.Any(m => m.Location.Distance(cacheObject.Position) < PlayerKiteDistance))
                                            cacheObject.Weight = 0;
                                    }

                                }
                                break;
                            }
                        case GObjectType.HealthWell:
                            {

                                // Healths Wells get handled correctly ... 
                                if (cacheObject.Type == GObjectType.HealthWell && PlayerStatus.CurrentHealthPct <= .75)
                                {
                                    cacheObject.Weight += 7500;
                                }
                                if (cacheObject.Type == GObjectType.HealthWell && PlayerStatus.CurrentHealthPct <= .25)
                                {
                                    cacheObject.Weight += 20000d;
                                }
                                break;
                            }
                        case GObjectType.Shrine:
                            {
                                // Weight Shrines
                                cacheObject.Weight = (75f - cacheObject.RadiusDistance) / 75f * 14500f;

                                // Very close shrines get a weight increase
                                if (cacheObject.CentreDistance <= 30f)
                                    cacheObject.Weight += 10000d;

                                if (cacheObject.Weight > 0)
                                {
                                    // Was already a target and is still viable, give it some free extra weight, to help stop flip-flopping between two targets
                                    if (cacheObject.RActorGuid == CurrentTargetRactorGUID && cacheObject.CentreDistance <= 25f)
                                        cacheObject.Weight += 400;

                                    // If there's a monster in the path-line to the item
                                    if (hashMonsterObstacleCache.Any(cp => MathUtil.IntersectsPath(cp.Location, cp.Radius, PlayerStatus.CurrentPosition, cacheObject.Position)))
                                        cacheObject.Weight = 1;

                                    // See if there's any AOE avoidance in that spot, if so reduce the weight to 1
                                    if (hashAvoidanceObstacleCache.Any(cp => MathUtil.IntersectsPath(cp.Location, cp.Radius, PlayerStatus.CurrentPosition, cacheObject.Position)))
                                        cacheObject.Weight = 1;

                                    // if there's any monsters nearby
                                    if (TargetUtil.AnyMobsInRange(15f))
                                        cacheObject.Weight = 1;

                                    if (PrioritizeCloseRangeUnits)
                                        cacheObject.Weight = 1;
                                }
                                break;
                            }
                        case GObjectType.Door:
                            {
                                if (!ObjectCache.Any(u => u.Type == GObjectType.Unit && u.HitPointsPct > 0 &&
                                    MathUtil.IntersectsPath(u.Position, u.Radius, PlayerStatus.CurrentPosition, cacheObject.Position)))
                                {
                                    if (cacheObject.RadiusDistance <= 20f)
                                        cacheObject.Weight += 15000d;

                                    // We're standing on the damn thing... open it!!
                                    if (cacheObject.RadiusDistance <= 12f)
                                        cacheObject.Weight += 30000d;
                                }
                                break;
                            }
                        case GObjectType.Destructible:
                        case GObjectType.Barricade:
                            {

                                // rrrix added this as a single "weight" source based on the DestructableRange.
                                // Calculate the weight based on distance, where a distance = 1 is 5000, 90 = 0
                                cacheObject.Weight = (90f - cacheObject.RadiusDistance) / 90f * 5000f;

                                // Was already a target and is still viable, give it some free extra weight, to help stop flip-flopping between two targets
                                if (cacheObject.RActorGuid == CurrentTargetRactorGUID && cacheObject.CentreDistance <= 25f)
                                    cacheObject.Weight += 400;

                                //// Close destructibles get a weight increase
                                //if (cacheObject.CentreDistance <= 16f)
                                //    cacheObject.Weight += 1500d;

                                // If there's a monster in the path-line to the item, reduce the weight by 50%
                                if (hashMonsterObstacleCache.Any(cp => MathUtil.IntersectsPath(cp.Location, cp.Radius, PlayerStatus.CurrentPosition, cacheObject.Position)))
                                    cacheObject.Weight *= 0.5;

                                // See if there's any AOE avoidance in that spot, if so reduce the weight to 1
                                if (hashAvoidanceObstacleCache.Any(cp => MathUtil.IntersectsPath(cp.Location, cp.Radius, PlayerStatus.CurrentPosition, cacheObject.Position)))
                                    cacheObject.Weight = 1;

                                // Are we prioritizing close-range stuff atm? If so limit it at a value 3k lower than monster close-range priority
                                if (PrioritizeCloseRangeUnits)
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
                                // Weight Interactable Specials

                                // Very close interactables get a weight increase
                                cacheObject.Weight = (90d - cacheObject.CentreDistance) / 90d * 15000d;
                                if (cacheObject.CentreDistance <= 12f)
                                    cacheObject.Weight += 1000d;

                                // Was already a target and is still viable, give it some free extra weight, to help stop flip-flopping between two targets
                                if (cacheObject.RActorGuid == CurrentTargetRactorGUID && cacheObject.CentreDistance <= 25f)
                                    cacheObject.Weight += 400;

                                // If there's a monster in the path-line to the item, reduce the weight by 50%
                                if (hashMonsterObstacleCache.Any(cp => MathUtil.IntersectsPath(cp.Location, cp.Radius, PlayerStatus.CurrentPosition, cacheObject.Position)))
                                    cacheObject.Weight *= 0.5;

                                // See if there's any AOE avoidance in that spot, if so reduce the weight to 1
                                if (hashAvoidanceObstacleCache.Any(cp => MathUtil.IntersectsPath(cp.Location, cp.Radius, PlayerStatus.CurrentPosition, cacheObject.Position)))
                                    cacheObject.Weight = 1;

                                //if (bAnyMobsInCloseRange || (CurrentTarget != null && CurrentTarget.IsBossOrEliteRareUnique))
                                //    cacheObject.Weight = 1;

                                break;
                            }
                        case GObjectType.Container:
                            {

                                // Weight Containers

                                // Very close containers get a weight increase
                                cacheObject.Weight = (190d - cacheObject.CentreDistance) / 190d * 11000d;
                                if (cacheObject.CentreDistance <= 12f)
                                    cacheObject.Weight += 600d;

                                // Was already a target and is still viable, give it some free extra weight, to help stop flip-flopping between two targets
                                if (cacheObject.RActorGuid == CurrentTargetRactorGUID && cacheObject.CentreDistance <= 25f)
                                    cacheObject.Weight += 400;

                                // If there's a monster in the path-line to the item, reduce the weight by 50%
                                if (hashMonsterObstacleCache.Any(cp => MathUtil.IntersectsPath(cp.Location, cp.Radius, PlayerStatus.CurrentPosition, cacheObject.Position)))
                                    cacheObject.Weight *= 0.5;

                                // See if there's any AOE avoidance in that spot, if so reduce the weight to 1
                                if (hashAvoidanceObstacleCache.Any(cp => MathUtil.IntersectsPath(cp.Location, cp.Radius, PlayerStatus.CurrentPosition, cacheObject.Position)))
                                    cacheObject.Weight = 1;
                                break;
                            }
                    }

                    // Switch on object type

                    // Force the character to stay where it is if there is nothing available that is out of avoidance stuff and we aren't already in avoidance stuff
                    if (cacheObject.Weight == 1 && !StandingInAvoidance && ObjectCache.Any(o => o.Type == GObjectType.Avoidance))
                    {
                        cacheObject.Weight = 0;
                        ShouldStayPutDuringAvoidance = true;
                    }
                    Logger.Log(TrinityLogLevel.Debug, LogCategory.Weight,
                        "Weight={2:0} target= {0} ({1}) type={3} R-Dist={4:0} IsElite={5} RAGuid={6} {7}",
                            cacheObject.InternalName, cacheObject.ActorSNO, cacheObject.Weight, cacheObject.Type, cacheObject.RadiusDistance, cacheObject.IsElite, cacheObject.RActorGuid, unitWeightInfo);

                    // Prevent current target dynamic ranged weighting flip-flop 
                    if (CurrentTargetRactorGUID == cacheObject.RActorGuid && cacheObject.Weight <= 1)
                    {
                        cacheObject.Weight = 100;
                    }

                    // Is the weight of this one higher than the current-highest weight? Then make this the new primary target!
                    if (cacheObject.Weight > w_HighestWeightFound && cacheObject.Weight > 0)
                    {
                        // Clone the current CacheObject
                        CurrentTarget = cacheObject.Clone();
                        w_HighestWeightFound = cacheObject.Weight;

                        // See if we can try attempting kiting later
                        NeedToKite = false;
                        vKitePointAvoid = Vector3.Zero;

                        // Kiting and Avoidance
                        if (CurrentTarget.Type == GObjectType.Unit)
                        {
                            var AvoidanceList = hashAvoidanceObstacleCache.Where(o =>
                                // Distance from avoidance to target is less than avoidance radius
                                o.Location.Distance(CurrentTarget.Position) <= (GetAvoidanceRadius(o.ActorSNO) * 1.2) &&
                                    // Distance from obstacle to me is <= cacheObject.RadiusDistance
                                o.Location.Distance(PlayerStatus.CurrentPosition) <= (cacheObject.RadiusDistance - 4f)
                                );

                            // if there's any obstacle within a specified distance of the avoidance radius *1.2 
                            if (AvoidanceList.Any())
                            {
                                foreach (CacheObstacleObject o in AvoidanceList)
                                {
                                    Logger.Log(TrinityLogLevel.Debug, LogCategory.Targetting, "Avoidance: Id={0} Weight={1} Loc={2} Radius={3} Name={4}", o.ActorSNO, o.Weight, o.Location, o.Radius, o.Name);
                                }

                                vKitePointAvoid = CurrentTarget.Position;
                                NeedToKite = true;
                            }
                        }
                    }
                }

                // Loop through all the objects and give them a weight
                if (CurrentTarget != null && CurrentTarget.InternalName != null && CurrentTarget.ActorSNO > 0 && CurrentTarget.RActorGuid != CurrentTargetRactorGUID)
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
                CurrentTarget.FirstTargetAssignmentTime = DateTime.Now;
                GenericCache.AddToCache(new GenericCacheObject(targetMd5Hash, CurrentTarget, new TimeSpan(0, 10, 0)));
            }
            else if (GenericCache.ContainsKey(targetMd5Hash))
            {
                TrinityCacheObject cTarget = (TrinityCacheObject)GenericCache.GetObject(targetMd5Hash).Value;
                if (!cTarget.IsBoss && cTarget.TimesBeenPrimaryTarget > 15 && !(cTarget.Type == GObjectType.Item && cTarget.ItemQuality >= ItemQuality.Legendary))
                {
                    Logger.Log(TrinityLogLevel.Normal, LogCategory.UserInformation, "Blacklisting target {0} ActorSNO={1} RActorGUID={2} due to possible stuck/flipflop!",
                        CurrentTarget.InternalName, CurrentTarget.ActorSNO, CurrentTarget.RActorGuid);

                    hashRGUIDBlacklist60.Add(CurrentTarget.RActorGuid);

                    // Add to generic blacklist for safety, as the RActorGUID on items and gold can change as we move away and get closer to the items (while walking around corners)
                    // So we can't use any ID's but rather have to use some data which never changes (actorSNO, position, type, worldID)
                    GenericBlacklist.AddToBlacklist(new GenericCacheObject()
                    {
                        Key = CurrentTarget.ObjectHash,
                        Value = null,
                        Expires = DateTime.Now.AddSeconds(60)
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
