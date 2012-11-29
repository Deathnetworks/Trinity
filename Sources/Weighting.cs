using GilesTrinity.Settings.Combat;
using System;
using System.Linq;
using Zeta.Common;
using Zeta.Common.Plugins;
using Zeta.Internals.Actors;
using Zeta.Internals.SNO;
namespace GilesTrinity
{
    public partial class GilesTrinity : IPlugin
    {
        private static void RefreshDiaGetWeights()
        {

            // Store if we are ignoring all units this cycle or not
            bool bIgnoreAllUnits = !bAnyChampionsPresent &&
                                    !bAnyMobsInCloseRange &&
                                    (
                                        (
                                            !bAnyTreasureGoblinsPresent &&
                                            Settings.Combat.Misc.GoblinPriority >= GoblinPriority.Prioritize
                                        ) ||
                                        Settings.Combat.Misc.GoblinPriority < GoblinPriority.Prioritize
                                    ) &&
                                    playerStatus.CurrentHealthPct >= 0.85d;

            bool PrioritizeCloseRangeUnits = (ForceCloseRangeTarget || playerStatus.IsRooted);

            bool bIsBerserked = GilesHasBuff(SNOPower.Barbarian_WrathOfTheBerserker);
            foreach (GilesObject cacheObject in GilesObjectCache)
            {

                // Just to make sure each one starts at 0 weight...
                cacheObject.Weight = 0d;

                // Now do different calculations based on the object type
                switch (cacheObject.Type)
                {
                    case GObjectType.Unit:
                        {

                            // Weight Units

                            // No champions, no mobs nearby, no treasure goblins to prioritize, and not injured, so skip mobs
                            if (bIgnoreAllUnits)
                            {
                                break;
                            }

                            // Total up monsters at various ranges
                            if (cacheObject.RadiusDistance <= 50f)
                            {
                                bool bCountAsElite = (cacheObject.IsEliteRareUnique || cacheObject.IsBoss);
                                //intell -- removed thisgilesobject.bThisTreasureGoblin

                                // Flag up any bosses in range
                                if (cacheObject.IsBoss)
                                    bAnyBossesInRange = true;
                                if (cacheObject.RadiusDistance <= 6f)
                                {
                                    iAnythingWithinRange[RANGE_6]++;
                                    if (bCountAsElite)
                                        iElitesWithinRange[RANGE_6]++;
                                }
                                if (cacheObject.RadiusDistance <= 12f)
                                {
                                    iAnythingWithinRange[RANGE_12]++;
                                    if (bCountAsElite)
                                        iElitesWithinRange[RANGE_12]++;
                                }
                                if (cacheObject.RadiusDistance <= 15f)
                                {
                                    iAnythingWithinRange[RANGE_15]++;
                                    if (bCountAsElite)
                                        iElitesWithinRange[RANGE_15]++;
                                }
                                if (cacheObject.RadiusDistance <= 20f)
                                {
                                    iAnythingWithinRange[RANGE_20]++;
                                    if (bCountAsElite)
                                        iElitesWithinRange[RANGE_20]++;
                                }
                                if (cacheObject.RadiusDistance <= 25f)
                                {
                                    if (!bAnyNonWWIgnoreMobsInRange && !hashActorSNOWhirlwindIgnore.Contains(cacheObject.ActorSNO))
                                        bAnyNonWWIgnoreMobsInRange = true;
                                    iAnythingWithinRange[RANGE_25]++;
                                    if (bCountAsElite)
                                        iElitesWithinRange[RANGE_25]++;
                                }
                                if (cacheObject.RadiusDistance <= 30f)
                                {
                                    iAnythingWithinRange[RANGE_30]++;
                                    if (bCountAsElite)
                                        iElitesWithinRange[RANGE_30]++;
                                }
                                if (cacheObject.RadiusDistance <= 40f)
                                {
                                    iAnythingWithinRange[RANGE_40]++;
                                    if (bCountAsElite)
                                        iElitesWithinRange[RANGE_40]++;
                                }
                                if (cacheObject.RadiusDistance <= 50f)
                                {
                                    iAnythingWithinRange[RANGE_50]++;
                                    if (bCountAsElite)
                                        iElitesWithinRange[RANGE_50]++;
                                }
                            }

                            // Force a close range target because we seem to be stuck *OR* if not ranged and currently rooted
                            if (PrioritizeCloseRangeUnits)
                            {
                                cacheObject.Weight = 20000 - (Math.Floor(cacheObject.CentreDistance) * 200);

                                // Goblin priority KAMIKAZEEEEEEEE
                                if (cacheObject.IsTreasureGoblin && Settings.Combat.Misc.GoblinPriority == GoblinPriority.Kamikaze)
                                    cacheObject.Weight += 25000;
                            }
                            else
                            {

                                // Not attackable, could be shielded, make super low priority
                                if (!cacheObject.IsAttackable)
                                {

                                    // Only 500 weight helps prevent it being prioritized over an unshielded
                                    cacheObject.Weight = 500;
                                }
                                // Not forcing close-ranged targets from being stuck, so let's calculate a weight!
                                else
                                {

                                    // Starting weight of 5000 to beat a lot of crap weight stuff
                                    cacheObject.Weight = 5000;

                                    // Distance as a percentage of max radius gives a value up to 1000 (1000 would be point-blank range)
                                    if (cacheObject.RadiusDistance < iCurrentMaxKillRadius)
                                        cacheObject.Weight += (1200 * (1 - (cacheObject.RadiusDistance / iCurrentMaxKillRadius)));

                                    // Give extra weight to ranged enemies
                                    if ((iMyCachedActorClass == ActorClass.Barbarian || iMyCachedActorClass == ActorClass.Monk) &&
                                        (cacheObject.MonsterStyle == MonsterSize.Ranged || hashActorSNORanged.Contains(c_ActorSNO)))
                                    {
                                        cacheObject.Weight += 1100;
                                        cacheObject.ForceLeapAgainst = true;
                                    }

                                    // Give more weight to elites and minions
                                    //intell -- no weight for uber elites (key wardens), they already got 200 radius kill
                                    if ((cacheObject.IsEliteRareUnique || cacheObject.IsMinion) && c_ActorSNO != 256015 && c_ActorSNO != 256000 && c_ActorSNO != 255996)
                                        cacheObject.Weight += 2000;

                                    // Give more weight to bosses
                                    if (cacheObject.IsBoss)
                                        cacheObject.Weight += 4000;

                                    // Barbarians with wrath of the berserker up should prioritize elites more
                                    if (bIsBerserked && (cacheObject.IsEliteRareUnique || cacheObject.IsTreasureGoblin || cacheObject.IsBoss))
                                        cacheObject.Weight += 2000;

                                    // Swarmers/boss-likes get more weight
                                    if (cacheObject.MonsterStyle == MonsterSize.Swarm || cacheObject.MonsterStyle == MonsterSize.Boss)
                                        cacheObject.Weight += 900;

                                    // Standard/big get a small bonus incase of "unknown" monster types being present
                                    if (cacheObject.MonsterStyle == MonsterSize.Standard || cacheObject.MonsterStyle == MonsterSize.Big)
                                        cacheObject.Weight += 150;

                                    // Lower health gives higher weight - health is worth up to 300 extra weight
                                    if (cacheObject.HitPoints < 0.20)
                                        cacheObject.Weight += (300 * (1 - (cacheObject.HitPoints / 0.5)));

                                    // Elites on low health get extra priority - up to 1500
                                    if ((cacheObject.IsEliteRareUnique || cacheObject.IsTreasureGoblin) && cacheObject.HitPoints < 0.20)
                                        cacheObject.Weight += (1500 * (1 - (cacheObject.HitPoints / 0.45)));

                                    // Goblins on low health get extra priority - up to 2500
                                    if (Settings.Combat.Misc.GoblinPriority >= GoblinPriority.Prioritize && cacheObject.IsTreasureGoblin && cacheObject.HitPoints <= 0.98)
                                        cacheObject.Weight += (3000 * (1 - (cacheObject.HitPoints / 0.85)));

                                    // Bonuses to priority type monsters from the dictionary/hashlist set at the top of the code
                                    int iExtraPriority;
                                    if (dictActorSNOPriority.TryGetValue(cacheObject.ActorSNO, out iExtraPriority))
                                    {
                                        cacheObject.Weight += iExtraPriority;
                                    }

                                    // Close range get higher weights the more of them there are, to prevent body-blocking

                                    // Plus a free bonus to anything close anyway
                                    if (cacheObject.RadiusDistance <= 11f)
                                    {

                                        // Extra bonus for point-blank range
                                        iUnitsSurrounding++;

                                        // Give special "surrounded" weight to each unit
                                        cacheObject.Weight += (200 * iUnitsSurrounding);
                                    }

                                    // Special additional weight for corrupt growths in act 4 ONLY if they are at close range (not a standard priority thing)
                                    if ((cacheObject.ActorSNO == 210120 || cacheObject.ActorSNO == 210268) && cacheObject.CentreDistance <= 25f)
                                        cacheObject.Weight += 2000;

                                    // Was already a target and is still viable, give it some free extra weight, to help stop flip-flopping between two targets
                                    if (cacheObject.RActorGuid == CurrentTargetRactorGUID && cacheObject.CentreDistance <= 25f)
                                        cacheObject.Weight += 800;

                                    // Lower the priority for EACH AOE *BETWEEN* us and the target, NOT counting the one directly under-foot, up to a maximum of 1500 reduction
                                    Vector3 point = cacheObject.Position;
                                    float fWeightRemoval = 0;
                                    foreach (GilesObstacle tempobstacle in hashAvoidanceObstacleCache.Where(cp =>
                                        GilesIntersectsPath(cp.Location, cp.Radius, playerStatus.CurrentPosition, point) &&
                                        cp.Location.Distance(point) > GetAvoidanceRadius(cp.ActorSNO)))
                                    {
                                        fWeightRemoval += (float)tempobstacle.Weight * 8;
                                    }
                                    if (fWeightRemoval > 1500)
                                        fWeightRemoval = 1500;
                                    cacheObject.Weight -= fWeightRemoval;

                                    // Lower the priority if there is AOE *UNDER* the target, by the HIGHEST weight there only
                                    fWeightRemoval = 0;
                                    foreach (GilesObstacle tempobstacle in hashAvoidanceObstacleCache.Where(cp => cp.Location.Distance(point) <= GetAvoidanceRadius(cp.ActorSNO) &&
                                        cp.Location.Distance(playerStatus.CurrentPosition) <= (cacheObject.RadiusDistance - 4f)))
                                    {

                                        // Up to 200 weight for a high-priority AOE - maximum 3400 weight reduction
                                        if (tempobstacle.Weight > fWeightRemoval)
                                            fWeightRemoval = (float)tempobstacle.Weight * 30;
                                    }
                                    cacheObject.Weight -= fWeightRemoval;

                                    // Prevent going less than 300 yet to prevent annoyances (should only lose this much weight from priority reductions in priority list?)
                                    if (cacheObject.Weight < 300)
                                        cacheObject.Weight = 300;

                                    // Deal with treasure goblins - note, of priority is set to "0", then the is-a-goblin flag isn't even set for use here - the monster is ignored
                                    if (cacheObject.IsTreasureGoblin)
                                    {

                                        // Logging goblin sightings
                                        if (lastGoblinTime == DateTime.Today)
                                        {
                                            iTotalNumberGoblins++;
                                            lastGoblinTime = DateTime.Now;
                                            Logging.Write("[Trinity] Goblin #" + iTotalNumberGoblins.ToString() + " in sight. Distance=" + cacheObject.CentreDistance);
                                        }
                                        else
                                        {
                                            if (DateTime.Now.Subtract(lastGoblinTime).TotalMilliseconds > 30000)
                                                lastGoblinTime = DateTime.Today;
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
                                                cacheObject.Weight += 10101;
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
                            if (cacheObject.GoldAmount > 0)
                                cacheObject.Weight = 11000d - (Math.Floor(cacheObject.CentreDistance) * 200d);
                            else
                                cacheObject.Weight = 13000d - (Math.Floor(cacheObject.CentreDistance) * 190d);

                            // Point-blank items get a weight increase 
                            if (cacheObject.GoldAmount <= 0 && cacheObject.CentreDistance <= 12f)
                                cacheObject.Weight += 600d;

                            // Was already a target and is still viable, give it some free extra weight, to help stop flip-flopping between two targets
                            if (cacheObject.RActorGuid == CurrentTargetRactorGUID && cacheObject.CentreDistance <= 25f)
                                cacheObject.Weight += 800;

                            // Give yellows more weight
                            if (cacheObject.GoldAmount <= 0 && cacheObject.ItemQuality >= ItemQuality.Rare4)
                                cacheObject.Weight += 6000d;

                            // Give legendaries more weight
                            if (cacheObject.GoldAmount <= 0 && cacheObject.ItemQuality >= ItemQuality.Legendary)
                                cacheObject.Weight += 10000d;

                            // Are we prioritizing close-range stuff atm? If so limit it at a value 3k lower than monster close-range priority
                            if (PrioritizeCloseRangeUnits)
                                cacheObject.Weight = 18000 - (Math.Floor(cacheObject.CentreDistance) * 200);

                            // If there's a monster in the path-line to the item, reduce the weight to 1
                            if (hashMonsterObstacleCache.Any(cp => GilesIntersectsPath(cp.Location, cp.Radius*1.2f, playerStatus.CurrentPosition, cacheObject.Position)))
                                cacheObject.Weight = 1;

                            // See if there's any AOE avoidance in that spot or inbetween us, if so reduce the weight to 1
                            if (hashAvoidanceObstacleCache.Any(cp => GilesIntersectsPath(cp.Location, cp.Radius, playerStatus.CurrentPosition, cacheObject.Position)))
                                cacheObject.Weight = 1;

                            // ignore any items/gold if there is mobs in kill radius and we aren't combat looting
                            if (bAnyMobsInCloseRange && !Zeta.CommonBot.Settings.CharacterSettings.Instance.CombatLooting)
                                cacheObject.Weight = 1;

                            // Calculate a spot reaching a little bit further out from the item, to help pickup-movements
                            /*if (thisgilesobject.dThisWeight > 0)
                            {
                                if (thisgilesobject.iThisGoldAmount > 0)
                                    thisgilesobject.vThisPosition = MathEx.CalculatePointFrom(thisgilesobject.vThisPosition, playerStatus.vCurrentPosition, thisgilesobject.fCentreDistance + 2f);
                                else
                                    thisgilesobject.vThisPosition = MathEx.CalculatePointFrom(thisgilesobject.vThisPosition, playerStatus.vCurrentPosition, thisgilesobject.fCentreDistance + 1f);
                            }*/
                            break;
                        }
                    case GObjectType.Globe:
                        {
                            // Weight Health Globes

                            // Give all globes 0 weight (so never gone-to), unless we have low health, then go for them
                            if (playerStatus.CurrentHealthPct > PlayerEmergencyHealthGlobeLimit || !Settings.Combat.Misc.CollectHealthGlobe)
                            {
                                cacheObject.Weight = 0;
                            }
                            else
                            {

                                // Ok we have globes enabled, and our health is low...!
                                cacheObject.Weight = 17000d - (Math.Floor(cacheObject.CentreDistance) * 90d);

                                // Point-blank items get a weight increase
                                if (cacheObject.CentreDistance <= 15f)
                                    cacheObject.Weight += 3000d;

                                // Close items get a weight increase
                                if (cacheObject.CentreDistance <= 60f)
                                    cacheObject.Weight += 1500d;

                                // Was already a target and is still viable, give it some free extra weight, to help stop flip-flopping between two targets
                                if (cacheObject.RActorGuid == CurrentTargetRactorGUID && cacheObject.CentreDistance <= 25f)
                                    cacheObject.Weight += 800;

                                // Are we prioritizing close-range stuff atm? If so limit it at a value 3k lower than monster close-range priority
                                //if (bPrioritizeCloseRange)

                                //    thisgilesobject.dThisWeight = 22000 - (Math.Floor(thisgilesobject.fCentreDistance) * 200);

                                // If there's a monster in the path-line to the item, reduce the weight by 15% for each
                                Vector3 point = cacheObject.Position;
                                foreach (GilesObstacle tempobstacle in hashMonsterObstacleCache.Where(cp =>
                                    GilesIntersectsPath(cp.Location, cp.Radius, playerStatus.CurrentPosition, point)))
                                {
                                    cacheObject.Weight *= 0.85;
                                }

                                // See if there's any AOE avoidance in that spot, if so reduce the weight by 10%
                                if (hashAvoidanceObstacleCache.Any(cp => GilesIntersectsPath(cp.Location, cp.Radius, playerStatus.CurrentPosition, cacheObject.Position)))
                                    cacheObject.Weight *= 0.9;

                                // Calculate a spot reaching a little bit further out from the globe, to help globe-movements
                                if (cacheObject.Weight > 0)
                                    cacheObject.Position = MathEx.CalculatePointFrom(cacheObject.Position, playerStatus.CurrentPosition, cacheObject.CentreDistance + 3f);
                            }
                            break;
                        }
                    case GObjectType.HealthWell:
                        {

                            // Healths Wells get handled correctly ... 
                            if (cacheObject.Type == GObjectType.HealthWell && playerStatus.CurrentHealthPct <= .75)
                            {
                                cacheObject.Weight += 7500;
                            }
                            if (cacheObject.Type == GObjectType.HealthWell && playerStatus.CurrentHealthPct <= .25)
                            {
                                cacheObject.Weight += 20000d;
                            }
                            break;
                        }
                    case GObjectType.Shrine:
                        {

                            // Weight Shrines
                            cacheObject.Weight = 14500d - (Math.Floor(cacheObject.CentreDistance) * 170d);

                            // Very close shrines get a weight increase
                            if (cacheObject.CentreDistance <= 20f)
                                cacheObject.Weight += 1000d;
                            switch (cacheObject.ActorSNO)
                            {
                                case 176074:

                                    // protection shrine
                                    break;
                                case 176076:

                                    // fortune shrine
                                    break;
                                case 176077:

                                    // frenzied shrine
                                    break;
                            }
                            if (cacheObject.Weight > 0)
                            {

                                // Was already a target and is still viable, give it some free extra weight, to help stop flip-flopping between two targets
                                if (cacheObject.RActorGuid == CurrentTargetRactorGUID && cacheObject.CentreDistance <= 25f)
                                    cacheObject.Weight += 400;

                                // Are we prioritizing close-range stuff atm? If so limit it at a value 3k lower than monster close-range priority
                                if (PrioritizeCloseRangeUnits)
                                    cacheObject.Weight = 18500d - (Math.Floor(cacheObject.CentreDistance) * 200);

                                // If there's a monster in the path-line to the item, reduce the weight by 25%
                                if (hashMonsterObstacleCache.Any(cp => GilesIntersectsPath(cp.Location, cp.Radius, playerStatus.CurrentPosition, cacheObject.Position)))
                                    cacheObject.Weight *= 0.75;

                                // See if there's any AOE avoidance in that spot, if so reduce the weight to 1
                                if (hashAvoidanceObstacleCache.Any(cp => GilesIntersectsPath(cp.Location, cp.Radius, playerStatus.CurrentPosition, cacheObject.Position)))
                                    cacheObject.Weight = 1;
                            }
                            break;
                        }
                    case GObjectType.Door:
                        {
                            if (cacheObject.RadiusDistance <= 20f)
                                cacheObject.Weight += 15000d;

                            // We're standing on the damn thing... open it!!
                            if (cacheObject.RadiusDistance <= 12f)
                                cacheObject.Weight += 250000d;
                            break;
                        }
                    case GObjectType.Destructible:
                    case GObjectType.Barricade:
                        {

                            // rrrix added this as a single "weight" source based on the DestructableRange.
                            // Calculate the weight based on distance, where a distance = 1 is 5000
                            cacheObject.Weight = 5000 * (1 - (cacheObject.RadiusDistance / Settings.WorldObject.DestructibleRange));

                            // Was already a target and is still viable, give it some free extra weight, to help stop flip-flopping between two targets
                            if (cacheObject.RActorGuid == CurrentTargetRactorGUID && cacheObject.CentreDistance <= 25f)
                                cacheObject.Weight += 400;

                            //// Close destructibles get a weight increase
                            //if (cacheObject.CentreDistance <= 16f)
                            //    cacheObject.Weight += 1500d;

                            // If there's a monster in the path-line to the item, reduce the weight by 50%
                            if (hashMonsterObstacleCache.Any(cp => GilesIntersectsPath(cp.Location, cp.Radius, playerStatus.CurrentPosition, cacheObject.Position)))
                                cacheObject.Weight *= 0.5;

                            // See if there's any AOE avoidance in that spot, if so reduce the weight to 1
                            if (hashAvoidanceObstacleCache.Any(cp => GilesIntersectsPath(cp.Location, cp.Radius, playerStatus.CurrentPosition, cacheObject.Position)))
                                cacheObject.Weight = 1;

                            // Are we prioritizing close-range stuff atm? If so limit it at a value 3k lower than monster close-range priority
                            if (PrioritizeCloseRangeUnits)
                                cacheObject.Weight = 19200d - (Math.Floor(cacheObject.CentreDistance) * 200d);

                            //// Very close destructibles get a final weight increase
                            //if (cacheObject.CentreDistance <= 12f)
                            //    cacheObject.Weight += 10000d;

                            //// We're standing on the damn thing... break it
                            //if (cacheObject.RadiusDistance <= 5f)
                            //    cacheObject.Weight += 20000d;

                            //// Fix for WhimsyShire Pinata
                            if (hashSNOContainerResplendant.Contains(cacheObject.ActorSNO))
                                cacheObject.Weight = 100 + cacheObject.RadiusDistance;
                            break;
                        }
                    case GObjectType.Interactable:
                        {

                            // Weight Interactable Specials

                            // Very close interactables get a weight increase
                            cacheObject.Weight = 15000d - (Math.Floor(cacheObject.CentreDistance) * 170d);
                            if (cacheObject.CentreDistance <= 12f)
                                cacheObject.Weight += 800d;

                            // Was already a target and is still viable, give it some free extra weight, to help stop flip-flopping between two targets
                            if (cacheObject.RActorGuid == CurrentTargetRactorGUID && cacheObject.CentreDistance <= 25f)
                                cacheObject.Weight += 400;

                            // If there's a monster in the path-line to the item, reduce the weight by 50%
                            if (hashMonsterObstacleCache.Any(cp => GilesIntersectsPath(cp.Location, cp.Radius, playerStatus.CurrentPosition, cacheObject.Position)))
                                cacheObject.Weight *= 0.5;

                            // See if there's any AOE avoidance in that spot, if so reduce the weight to 1
                            if (hashAvoidanceObstacleCache.Any(cp => GilesIntersectsPath(cp.Location, cp.Radius, playerStatus.CurrentPosition, cacheObject.Position)))
                                cacheObject.Weight = 1;
                            break;
                        }
                    case GObjectType.Container:
                        {

                            // Weight Containers

                            // Very close containers get a weight increase
                            cacheObject.Weight = 11000d - (Math.Floor(cacheObject.CentreDistance) * 190d);
                            if (cacheObject.CentreDistance <= 12f)
                                cacheObject.Weight += 600d;

                            // Was already a target and is still viable, give it some free extra weight, to help stop flip-flopping between two targets
                            if (cacheObject.RActorGuid == CurrentTargetRactorGUID && cacheObject.CentreDistance <= 25f)
                                cacheObject.Weight += 400;

                            // If there's a monster in the path-line to the item, reduce the weight by 50%
                            if (hashMonsterObstacleCache.Any(cp => GilesIntersectsPath(cp.Location, cp.Radius, playerStatus.CurrentPosition, cacheObject.Position)))
                                cacheObject.Weight *= 0.5;

                            // See if there's any AOE avoidance in that spot, if so reduce the weight to 1
                            if (hashAvoidanceObstacleCache.Any(cp => GilesIntersectsPath(cp.Location, cp.Radius, playerStatus.CurrentPosition, cacheObject.Position)))
                                cacheObject.Weight = 1;
                            break;
                        }
                }

                // Switch on object type

                // Force the character to stay where it is if there is nothing available that is out of avoidance stuff and we aren't already in avoidance stuff
                //if (thisgilesobject.dWeight == 1 && !bRequireAvoidance)
                //{

                //    thisgilesobject.dWeight = 0;

                //    bStayPutDuringAvoidance = true;
                //}
                if (Settings.Advanced.DebugWeights)
                {
                    Logging.WriteDiagnostic("[Trinity] Weighting of {0} ({1}) found to be: {2} type: {3} mobsInCloseRange: {4} requireAvoidance: {5}",
                        cacheObject.InternalName, cacheObject.ActorSNO, cacheObject.Weight, cacheObject.Type, bAnyMobsInCloseRange, StandingInAvoidance);
                }

                // Is the weight of this one higher than the current-highest weight? Then make this the new primary target!
                if (cacheObject.Weight > w_HighestWeightFound && cacheObject.Weight > 0)
                {

                    // Clone the current Giles-cache object
                    CurrentTarget = cacheObject.Clone();
                    w_HighestWeightFound = cacheObject.Weight;

                    // See if we can try attempting kiting later
                    NeedToKite = false;
                    vKitePointAvoid = vNullLocation;

                    // Kiting and Avoidance
                    if (CurrentTarget.Type == GObjectType.Unit)
                    {
                        var AvoidanceList = hashAvoidanceObstacleCache.Where(o =>
                            // Distance from avoidance to target is less than avoidance radius
                            o.Location.Distance(CurrentTarget.Position) <= (GetAvoidanceRadius(o.ActorSNO) * 1.2) &&
                                // Distance from obstacle to me is <= cacheObject.RadiusDistance
                            o.Location.Distance(playerStatus.CurrentPosition) <= (cacheObject.RadiusDistance - 4f)
                            );

                        // if there's any obstacle within a specified distance of the avoidance radius *1.2 
                        if (AvoidanceList.Any())
                        {
                            if (Settings.Advanced.DebugTargetting)
                            {
                                foreach (GilesObstacle o in AvoidanceList)
                                {
                                    Logging.WriteDiagnostic("[Trinity] Avoidance: Id={0} Weight={1} Loc={2} Radius={3} Name={4}", o.ActorSNO, o.Weight, o.Location, o.Radius, o.Name);
                                }
                            }

                            vKitePointAvoid = CurrentTarget.Position;
                            NeedToKite = true;
                        }
                    }
                }
            }

            // Loop through all the objects and give them a weight
            if (Settings.Advanced.DebugTargetting && CurrentTarget != null && CurrentTarget.InternalName != null && CurrentTarget.ActorSNO > 0)
            {
                Logging.WriteVerbose("[Trinity] Target changed to {2} {0} ({1})",
                                CurrentTarget.InternalName, CurrentTarget.ActorSNO, CurrentTarget.Type);
            }
        }
    }
}
