using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Web;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using System.Windows.Markup;
using Zeta;
using Zeta.Common;
using Zeta.Common.Plugins;
using Zeta.CommonBot;
using Zeta.CommonBot.Profile;
using Zeta.CommonBot.Profile.Composites;
using Zeta.Internals;
using Zeta.Internals.Actors;
using Zeta.Internals.Actors.Gizmos;
using Zeta.Internals.SNO;
using Zeta.Navigation;
using Zeta.TreeSharp;
using Zeta.XmlEngine;
namespace GilesTrinity
{
    public partial class GilesTrinity : IPlugin
    {
        private static void RefreshDiaGetWeights()
        {

            // Store if we are ignoring all units this cycle or not
            bool bIgnoreAllUnits = !bAnyChampionsPresent && !bAnyMobsInCloseRange && ((!bAnyTreasureGoblinsPresent && settings.iTreasureGoblinPriority >= 2) || settings.iTreasureGoblinPriority < 2) &&
                            playerStatus.dCurrentHealthPct >= 0.85d;
            bool bPrioritizeCloseRange = (bForceCloseRangeTarget || playerStatus.bIsRooted);
            bool bIsBerserked = GilesHasBuff(SNOPower.Barbarian_WrathOfTheBerserker);
            foreach (GilesObject thisgilesobject in listGilesObjectCache)
            {

                // Just to make sure each one starts at 0 weight...
                thisgilesobject.dWeight = 0d;

                // Now do different calculations based on the object type
                switch (thisgilesobject.GilesObjectType)
                {
                    case GilesObjectType.Unit:
                        {

                            // Weight Units

                            // No champions, no mobs nearby, no treasure goblins to prioritize, and not injured, so skip mobs
                            if (bIgnoreAllUnits)
                            {
                                break;
                            }

                            // Total up monsters at various ranges
                            if (thisgilesobject.fRadiusDistance <= 50f)
                            {
                                bool bCountAsElite = (thisgilesobject.IsEliteRareUnique || thisgilesobject.IsBoss);
                                //intell -- removed thisgilesobject.bThisTreasureGoblin

                                // Flag up any bosses in range
                                if (thisgilesobject.IsBoss)
                                    bAnyBossesInRange = true;
                                if (thisgilesobject.fRadiusDistance <= 6f)
                                {
                                    iAnythingWithinRange[RANGE_6]++;
                                    if (bCountAsElite)
                                        iElitesWithinRange[RANGE_6]++;
                                }
                                if (thisgilesobject.fRadiusDistance <= 12f)
                                {
                                    iAnythingWithinRange[RANGE_12]++;
                                    if (bCountAsElite)
                                        iElitesWithinRange[RANGE_12]++;
                                }
                                if (thisgilesobject.fRadiusDistance <= 15f)
                                {
                                    iAnythingWithinRange[RANGE_15]++;
                                    if (bCountAsElite)
                                        iElitesWithinRange[RANGE_15]++;
                                }
                                if (thisgilesobject.fRadiusDistance <= 20f)
                                {
                                    iAnythingWithinRange[RANGE_20]++;
                                    if (bCountAsElite)
                                        iElitesWithinRange[RANGE_20]++;
                                }
                                if (thisgilesobject.fRadiusDistance <= 25f)
                                {
                                    if (!bAnyNonWWIgnoreMobsInRange && !hashActorSNOWhirlwindIgnore.Contains(thisgilesobject.iActorSNO))
                                        bAnyNonWWIgnoreMobsInRange = true;
                                    iAnythingWithinRange[RANGE_25]++;
                                    if (bCountAsElite)
                                        iElitesWithinRange[RANGE_25]++;
                                }
                                if (thisgilesobject.fRadiusDistance <= 30f)
                                {
                                    iAnythingWithinRange[RANGE_30]++;
                                    if (bCountAsElite)
                                        iElitesWithinRange[RANGE_30]++;
                                }
                                if (thisgilesobject.fRadiusDistance <= 40f)
                                {
                                    iAnythingWithinRange[RANGE_40]++;
                                    if (bCountAsElite)
                                        iElitesWithinRange[RANGE_40]++;
                                }
                                if (thisgilesobject.fRadiusDistance <= 50f)
                                {
                                    iAnythingWithinRange[RANGE_50]++;
                                    if (bCountAsElite)
                                        iElitesWithinRange[RANGE_50]++;
                                }
                            }

                            // Force a close range target because we seem to be stuck *OR* if not ranged and currently rooted
                            if (bPrioritizeCloseRange)
                            {
                                thisgilesobject.dWeight = 20000 - (Math.Floor(thisgilesobject.fCentreDist) * 200);

                                // Goblin priority KAMIKAZEEEEEEEE
                                if (thisgilesobject.bIsTreasureGoblin && settings.iTreasureGoblinPriority == 3)
                                    thisgilesobject.dWeight += 25000;
                            }
                            else
                            {

                                // Not attackable, could be shielded, make super low priority
                                if (!thisgilesobject.bIsAttackable)
                                {

                                    // Only 500 weight helps prevent it being prioritized over an unshielded
                                    thisgilesobject.dWeight = 500;
                                }
// Not forcing close-ranged targets from being stuck, so let's calculate a weight!
                                else
                                {

                                    // Starting weight of 5000 to beat a lot of crap weight stuff
                                    thisgilesobject.dWeight = 5000;

                                    // Distance as a percentage of max radius gives a value up to 1000 (1000 would be point-blank range)
                                    if (thisgilesobject.fRadiusDistance < iCurrentMaxKillRadius)
                                        thisgilesobject.dWeight += (1200 * (1 - (thisgilesobject.fRadiusDistance / iCurrentMaxKillRadius)));

                                    // Give extra weight to ranged enemies
                                    if ((iMyCachedActorClass == ActorClass.Barbarian || iMyCachedActorClass == ActorClass.Monk) &&
                                        (thisgilesobject.eMonsterStyle == MonsterSize.Ranged || hashActorSNORanged.Contains(c_iActorSNO)))
                                    {
                                        thisgilesobject.dWeight += 1100;
                                        thisgilesobject.bForceLeapAgainst = true;
                                    }

                                    // Give more weight to elites and minions
                                    //intell -- no weight for uber elites (key wardens), they already got 200 radius kill
                                    if ((thisgilesobject.IsEliteRareUnique || thisgilesobject.bIsMinion) && c_iActorSNO != 256015 && c_iActorSNO != 256000 && c_iActorSNO != 255996)
                                        thisgilesobject.dWeight += 2000;

                                    // Give more weight to bosses
                                    if (thisgilesobject.IsBoss)
                                        thisgilesobject.dWeight += 4000;

                                    // Barbarians with wrath of the berserker up should prioritize elites more
                                    if (bIsBerserked && (thisgilesobject.IsEliteRareUnique || thisgilesobject.bIsTreasureGoblin || thisgilesobject.IsBoss))
                                        thisgilesobject.dWeight += 2000;

                                    // Swarmers/boss-likes get more weight
                                    if (thisgilesobject.eMonsterStyle == MonsterSize.Swarm || thisgilesobject.eMonsterStyle == MonsterSize.Boss)
                                        thisgilesobject.dWeight += 900;

                                    // Standard/big get a small bonus incase of "unknown" monster types being present
                                    if (thisgilesobject.eMonsterStyle == MonsterSize.Standard || thisgilesobject.eMonsterStyle == MonsterSize.Big)
                                        thisgilesobject.dWeight += 150;

                                    // Lower health gives higher weight - health is worth up to 300 extra weight
                                    if (thisgilesobject.iHitPoints < 0.20)
                                        thisgilesobject.dWeight += (300 * (1 - (thisgilesobject.iHitPoints / 0.5)));

                                    // Elites on low health get extra priority - up to 1500
                                    if ((thisgilesobject.IsEliteRareUnique || thisgilesobject.bIsTreasureGoblin) && thisgilesobject.iHitPoints < 0.20)
                                        thisgilesobject.dWeight += (1500 * (1 - (thisgilesobject.iHitPoints / 0.45)));

                                    // Goblins on low health get extra priority - up to 2500
                                    if (settings.iTreasureGoblinPriority >= 2 && thisgilesobject.bIsTreasureGoblin && thisgilesobject.iHitPoints <= 0.98)
                                        thisgilesobject.dWeight += (3000 * (1 - (thisgilesobject.iHitPoints / 0.85)));

                                    // Bonuses to priority type monsters from the dictionary/hashlist set at the top of the code
                                    int iExtraPriority;
                                    if (dictActorSNOPriority.TryGetValue(thisgilesobject.iActorSNO, out iExtraPriority))
                                    {
                                        thisgilesobject.dWeight += iExtraPriority;
                                    }

                                    // Close range get higher weights the more of them there are, to prevent body-blocking

                                    // Plus a free bonus to anything close anyway
                                    if (thisgilesobject.fRadiusDistance <= 11f)
                                    {

                                        // Extra bonus for point-blank range
                                        iUnitsSurrounding++;

                                        // Give special "surrounded" weight to each unit
                                        thisgilesobject.dWeight += (200 * iUnitsSurrounding);
                                    }

                                    // Special additional weight for corrupt growths in act 4 ONLY if they are at close range (not a standard priority thing)
                                    if ((thisgilesobject.iActorSNO == 210120 || thisgilesobject.iActorSNO == 210268) && thisgilesobject.fCentreDist <= 25f)
                                        thisgilesobject.dWeight += 2000;

                                    // Was already a target and is still viable, give it some free extra weight, to help stop flip-flopping between two targets
                                    if (thisgilesobject.iRActorGuid == iCurrentTargetRactorGUID && thisgilesobject.fCentreDist <= 25f)
                                        thisgilesobject.dWeight += 800;

                                    // Lower the priority for EACH AOE *BETWEEN* us and the target, NOT counting the one directly under-foot, up to a maximum of 1500 reduction
                                    Vector3 point = thisgilesobject.vPosition;
                                    float fWeightRemoval = 0;
                                    foreach (GilesObstacle tempobstacle in hashAvoidanceObstacleCache.Where(cp =>
                                        GilesIntersectsPath(cp.vThisLocation, cp.fThisRadius, playerStatus.vCurrentPosition, point) &&
                                        cp.vThisLocation.Distance(point) > GetAvoidanceRadius(cp.iThisSNOID)))
                                    {
                                        fWeightRemoval += (float)tempobstacle.dThisWeight * 8;
                                    }
                                    if (fWeightRemoval > 1500)
                                        fWeightRemoval = 1500;
                                    thisgilesobject.dWeight -= fWeightRemoval;

                                    // Lower the priority if there is AOE *UNDER* the target, by the HIGHEST weight there only
                                    fWeightRemoval = 0;
                                    foreach (GilesObstacle tempobstacle in hashAvoidanceObstacleCache.Where(cp => cp.vThisLocation.Distance(point) <= GetAvoidanceRadius(cp.iThisSNOID) &&
                                        cp.vThisLocation.Distance(playerStatus.vCurrentPosition) <= (thisgilesobject.fRadiusDistance - 4f)))
                                    {

                                        // Up to 200 weight for a high-priority AOE - maximum 3400 weight reduction
                                        if (tempobstacle.dThisWeight > fWeightRemoval)
                                            fWeightRemoval = (float)tempobstacle.dThisWeight * 30;
                                    }
                                    thisgilesobject.dWeight -= fWeightRemoval;

                                    // Prevent going less than 300 yet to prevent annoyances (should only lose this much weight from priority reductions in priority list?)
                                    if (thisgilesobject.dWeight < 300)
                                        thisgilesobject.dWeight = 300;

                                    // Deal with treasure goblins - note, of priority is set to "0", then the is-a-goblin flag isn't even set for use here - the monster is ignored
                                    if (thisgilesobject.bIsTreasureGoblin)
                                    {

                                        // Logging goblin sightings
                                        if (lastGoblinTime == DateTime.Today)
                                        {
                                            iTotalNumberGoblins++;
                                            lastGoblinTime = DateTime.Now;
                                            Logging.Write("[Trinity] Goblin #" + iTotalNumberGoblins.ToString() + " in sight. Distance=" + thisgilesobject.fCentreDist);
                                        }
                                        else
                                        {
                                            if (DateTime.Now.Subtract(lastGoblinTime).TotalMilliseconds > 30000)
                                                lastGoblinTime = DateTime.Today;
                                        }

                                        // Original Trinity stuff for priority handling now
                                        switch (settings.iTreasureGoblinPriority)
                                        {
                                            case 1:

                                                // Treating goblins as "normal monsters". Ok so I lied a little in the config, they get a little extra weight really! ;)
                                                thisgilesobject.dWeight += 751;
                                                break;
                                            case 2:

                                                // Super-high priority option below... 
                                                thisgilesobject.dWeight += 10101;
                                                break;
                                            case 3:

                                                // KAMIKAZE SUICIDAL TREASURE GOBLIN RAPE AHOY!
                                                thisgilesobject.dWeight += 40000;
                                                break;

                                            // PS: 58008 is an awesome number on any calculator.
                                        }
                                    }
                                }

                                // Forcing close range target or not?
                            }

                            // This is an attackable unit
                            break;
                        }
                    case GilesObjectType.Item:
                    case GilesObjectType.Gold:
                        {

                            // 

                            // Weight Items

                            // We'll weight them based on distance, giving gold less weight and close objects more
                            if (thisgilesobject.iGoldAmount > 0)
                                thisgilesobject.dWeight = 11000d - (Math.Floor(thisgilesobject.fCentreDist) * 200d);
                            else
                                thisgilesobject.dWeight = 13000d - (Math.Floor(thisgilesobject.fCentreDist) * 190d);

                            // Point-blank items get a weight increase 
                            if (thisgilesobject.iGoldAmount <= 0 && thisgilesobject.fCentreDist <= 12f)
                                thisgilesobject.dWeight += 600d;

                            // Was already a target and is still viable, give it some free extra weight, to help stop flip-flopping between two targets
                            if (thisgilesobject.iRActorGuid == iCurrentTargetRactorGUID && thisgilesobject.fCentreDist <= 25f)
                                thisgilesobject.dWeight += 800;

                            // Give yellows more weight
                            if (thisgilesobject.iGoldAmount <= 0 && thisgilesobject.eItemQuality >= ItemQuality.Rare4)
                                thisgilesobject.dWeight += 6000d;

                            // Give legendaries more weight
                            if (thisgilesobject.iGoldAmount <= 0 && thisgilesobject.eItemQuality >= ItemQuality.Legendary)
                                thisgilesobject.dWeight += 10000d;

                            // Are we prioritizing close-range stuff atm? If so limit it at a value 3k lower than monster close-range priority
                            if (bPrioritizeCloseRange)
                                thisgilesobject.dWeight = 18000 - (Math.Floor(thisgilesobject.fCentreDist) * 200);

                            // If there's a monster in the path-line to the item, reduce the weight by 25%
                            if (hashMonsterObstacleCache.Any(cp => GilesIntersectsPath(cp.vThisLocation, cp.fThisRadius, playerStatus.vCurrentPosition, thisgilesobject.vPosition)))
                                thisgilesobject.dWeight *= 0.75;

                            // See if there's any AOE avoidance in that spot or inbetween us, if so reduce the weight to 1
                            if (hashAvoidanceObstacleCache.Any(cp => GilesIntersectsPath(cp.vThisLocation, cp.fThisRadius, playerStatus.vCurrentPosition, thisgilesobject.vPosition)))
                                thisgilesobject.dWeight = 1;

                            // ignore any items/gold if there is mobs in kill radius
                            if (bAnyMobsInCloseRange)
                                thisgilesobject.dWeight = 1;

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
                    case GilesObjectType.Globe:
                        {

                            // 

                            // Weight Health Globes

                            // Give all globes 0 weight (so never gone-to), unless we have low health, then go for them
                            if (playerStatus.dCurrentHealthPct > iEmergencyHealthGlobeLimit || !settings.bEnableGlobes)
                            {
                                thisgilesobject.dWeight = 0;
                            }
                            else
                            {

                                // Ok we have globes enabled, and our health is low...!
                                thisgilesobject.dWeight = 17000d - (Math.Floor(thisgilesobject.fCentreDist) * 90d);

                                // Point-blank items get a weight increase
                                if (thisgilesobject.fCentreDist <= 15f)
                                    thisgilesobject.dWeight += 3000d;

                                // Close items get a weight increase
                                if (thisgilesobject.fCentreDist <= 60f)
                                    thisgilesobject.dWeight += 1500d;

                                // Was already a target and is still viable, give it some free extra weight, to help stop flip-flopping between two targets
                                if (thisgilesobject.iRActorGuid == iCurrentTargetRactorGUID && thisgilesobject.fCentreDist <= 25f)
                                    thisgilesobject.dWeight += 800;

                                // Are we prioritizing close-range stuff atm? If so limit it at a value 3k lower than monster close-range priority
                                //if (bPrioritizeCloseRange)

                                //    thisgilesobject.dThisWeight = 22000 - (Math.Floor(thisgilesobject.fCentreDistance) * 200);

                                // If there's a monster in the path-line to the item, reduce the weight by 15% for each
                                Vector3 point = thisgilesobject.vPosition;
                                foreach (GilesObstacle tempobstacle in hashMonsterObstacleCache.Where(cp =>
                                    GilesIntersectsPath(cp.vThisLocation, cp.fThisRadius, playerStatus.vCurrentPosition, point)))
                                {
                                    thisgilesobject.dWeight *= 0.85;
                                }

                                // See if there's any AOE avoidance in that spot, if so reduce the weight by 10%
                                if (hashAvoidanceObstacleCache.Any(cp => GilesIntersectsPath(cp.vThisLocation, cp.fThisRadius, playerStatus.vCurrentPosition, thisgilesobject.vPosition)))
                                    thisgilesobject.dWeight *= 0.9;

                                // Calculate a spot reaching a little bit further out from the globe, to help globe-movements
                                if (thisgilesobject.dWeight > 0)
                                    thisgilesobject.vPosition = MathEx.CalculatePointFrom(thisgilesobject.vPosition, playerStatus.vCurrentPosition, thisgilesobject.fCentreDist + 3f);
                            }
                            break;
                        }
                    case GilesObjectType.HealthWell:
                        {

                            // Healths Wells get handled correctly ... 
                            if (thisgilesobject.GilesObjectType == GilesObjectType.HealthWell && playerStatus.dCurrentHealthPct <= .75)
                            {
                                thisgilesobject.dWeight += 7500;
                            }
                            if (thisgilesobject.GilesObjectType == GilesObjectType.HealthWell && playerStatus.dCurrentHealthPct <= .25)
                            {
                                thisgilesobject.dWeight += 20000d;
                            }
                            break;
                        }
                    case GilesObjectType.Shrine:
                        {

                            // Weight Shrines
                            thisgilesobject.dWeight = 14500d - (Math.Floor(thisgilesobject.fCentreDist) * 170d);

                            // Very close shrines get a weight increase
                            if (thisgilesobject.fCentreDist <= 20f)
                                thisgilesobject.dWeight += 1000d;
                            switch (thisgilesobject.iActorSNO)
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
                            if (thisgilesobject.dWeight > 0)
                            {

                                // Was already a target and is still viable, give it some free extra weight, to help stop flip-flopping between two targets
                                if (thisgilesobject.iRActorGuid == iCurrentTargetRactorGUID && thisgilesobject.fCentreDist <= 25f)
                                    thisgilesobject.dWeight += 400;

                                // Are we prioritizing close-range stuff atm? If so limit it at a value 3k lower than monster close-range priority
                                if (bPrioritizeCloseRange)
                                    thisgilesobject.dWeight = 18500d - (Math.Floor(thisgilesobject.fCentreDist) * 200);

                                // If there's a monster in the path-line to the item, reduce the weight by 25%
                                if (hashMonsterObstacleCache.Any(cp => GilesIntersectsPath(cp.vThisLocation, cp.fThisRadius, playerStatus.vCurrentPosition, thisgilesobject.vPosition)))
                                    thisgilesobject.dWeight *= 0.75;

                                // See if there's any AOE avoidance in that spot, if so reduce the weight to 1
                                if (hashAvoidanceObstacleCache.Any(cp => GilesIntersectsPath(cp.vThisLocation, cp.fThisRadius, playerStatus.vCurrentPosition, thisgilesobject.vPosition)))
                                    thisgilesobject.dWeight = 1;
                            }
                            break;
                        }
                    case GilesObjectType.Door:
                        {
                            if (thisgilesobject.fRadiusDistance <= 20f)
                                thisgilesobject.dWeight += 15000d;

                            // We're standing on the damn thing... open it!!
                            if (thisgilesobject.fRadiusDistance <= 12f)
                                thisgilesobject.dWeight += 250000d;
                            break;
                        }
                    case GilesObjectType.Destructible:
                    case GilesObjectType.Barricade:
                        {

                            // Weight Destructibles
                            thisgilesobject.dWeight = 1750d - (Math.Floor(thisgilesobject.fCentreDist) * 175d);
                            //intell

                            // Was already a target and is still viable, give it some free extra weight, to help stop flip-flopping between two targets
                            if (thisgilesobject.iRActorGuid == iCurrentTargetRactorGUID && thisgilesobject.fCentreDist <= 25f)
                                thisgilesobject.dWeight += 400;

                            // Close destructibles get a weight increase
                            if (thisgilesobject.fCentreDist <= 16f)
                                thisgilesobject.dWeight += 1500d;

                            // If there's a monster in the path-line to the item, reduce the weight by 50%
                            if (hashMonsterObstacleCache.Any(cp => GilesIntersectsPath(cp.vThisLocation, cp.fThisRadius, playerStatus.vCurrentPosition, thisgilesobject.vPosition)))
                                thisgilesobject.dWeight *= 0.5;

                            // See if there's any AOE avoidance in that spot, if so reduce the weight to 1
                            if (hashAvoidanceObstacleCache.Any(cp => GilesIntersectsPath(cp.vThisLocation, cp.fThisRadius, playerStatus.vCurrentPosition, thisgilesobject.vPosition)))
                                thisgilesobject.dWeight = 1;

                            // Are we prioritizing close-range stuff atm? If so limit it at a value 3k lower than monster close-range priority
                            if (bPrioritizeCloseRange)
                                thisgilesobject.dWeight = 19200d - (Math.Floor(thisgilesobject.fCentreDist) * 200d);

                            // Very close destructibles get a final weight increase
                            if (thisgilesobject.fCentreDist <= 12f)
                                thisgilesobject.dWeight += 20000d;

                            // We're standing on the damn thing... break it!!
                            if (thisgilesobject.fRadiusDistance <= 5f)
                                thisgilesobject.dWeight += 200000d;
                            break;
                        }
                    case GilesObjectType.Interactable:
                        {

                            // Weight Interactable Specials

                            // Very close interactables get a weight increase
                            thisgilesobject.dWeight = 15000d - (Math.Floor(thisgilesobject.fCentreDist) * 170d);
                            if (thisgilesobject.fCentreDist <= 12f)
                                thisgilesobject.dWeight += 800d;

                            // Was already a target and is still viable, give it some free extra weight, to help stop flip-flopping between two targets
                            if (thisgilesobject.iRActorGuid == iCurrentTargetRactorGUID && thisgilesobject.fCentreDist <= 25f)
                                thisgilesobject.dWeight += 400;

                            // If there's a monster in the path-line to the item, reduce the weight by 50%
                            if (hashMonsterObstacleCache.Any(cp => GilesIntersectsPath(cp.vThisLocation, cp.fThisRadius, playerStatus.vCurrentPosition, thisgilesobject.vPosition)))
                                thisgilesobject.dWeight *= 0.5;

                            // See if there's any AOE avoidance in that spot, if so reduce the weight to 1
                            if (hashAvoidanceObstacleCache.Any(cp => GilesIntersectsPath(cp.vThisLocation, cp.fThisRadius, playerStatus.vCurrentPosition, thisgilesobject.vPosition)))
                                thisgilesobject.dWeight = 1;
                            break;
                        }
                    case GilesObjectType.Container:
                        {

                            // Weight Containers

                            // Very close containers get a weight increase
                            thisgilesobject.dWeight = 11000d - (Math.Floor(thisgilesobject.fCentreDist) * 190d);
                            if (thisgilesobject.fCentreDist <= 12f)
                                thisgilesobject.dWeight += 600d;

                            // Was already a target and is still viable, give it some free extra weight, to help stop flip-flopping between two targets
                            if (thisgilesobject.iRActorGuid == iCurrentTargetRactorGUID && thisgilesobject.fCentreDist <= 25f)
                                thisgilesobject.dWeight += 400;

                            // If there's a monster in the path-line to the item, reduce the weight by 50%
                            if (hashMonsterObstacleCache.Any(cp => GilesIntersectsPath(cp.vThisLocation, cp.fThisRadius, playerStatus.vCurrentPosition, thisgilesobject.vPosition)))
                                thisgilesobject.dWeight *= 0.5;

                            // See if there's any AOE avoidance in that spot, if so reduce the weight to 1
                            if (hashAvoidanceObstacleCache.Any(cp => GilesIntersectsPath(cp.vThisLocation, cp.fThisRadius, playerStatus.vCurrentPosition, thisgilesobject.vPosition)))
                                thisgilesobject.dWeight = 1;
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
                if (bDebugLogWeights && settings.bDebugInfo)
                {
                    Logging.WriteDiagnostic("[Trinity] Weighting of {0} ({1}) found to be: {2} type: {3} mobsInCloseRange: {4} requireAvoidance: {5}",
                        thisgilesobject.sInternalName, thisgilesobject.iActorSNO, thisgilesobject.dWeight, thisgilesobject.GilesObjectType, bAnyMobsInCloseRange, bRequireAvoidance);
                }

                // Is the weight of this one higher than the current-highest weight? Then make this the new primary target!
                if (thisgilesobject.dWeight > iHighestWeightFound && thisgilesobject.dWeight > 0)
                {

                    // Clone the current Giles-cache object
                    CurrentTarget = thisgilesobject.Clone();
                    iHighestWeightFound = thisgilesobject.dWeight;

                    // See if we can try attempting kiting later
                    bNeedToKite = false;
                    vKitePointAvoid = vNullLocation;

                    // Kiting
                    if (CurrentTarget.GilesObjectType == GilesObjectType.Unit)
                    {
                        Vector3 point = CurrentTarget.vPosition;

                        // if there's any obstacle within a specified distance of the avoidance radius *1.2 
                        if (hashAvoidanceObstacleCache.Any(cp => cp.vThisLocation.Distance(point) <= (GetAvoidanceRadius(cp.iThisSNOID) * 1.2) &&
                            cp.vThisLocation.Distance(playerStatus.vCurrentPosition) <= (thisgilesobject.fRadiusDistance - 4f)))
                        {
                            vKitePointAvoid = CurrentTarget.vPosition;
                            bNeedToKite = true;
                        }
                    }
                }
            }

            // Loop through all the objects and give them a weight
            if (bDebugLogSpecial && !settings.bDebugInfo && CurrentTarget != null && CurrentTarget.sInternalName != null && CurrentTarget.iActorSNO != null && CurrentTarget.GilesObjectType != null)
            {
                Logging.WriteVerbose("[Trinity] Target changed to {2} {0} ({1})",
                                CurrentTarget.sInternalName, CurrentTarget.iActorSNO, CurrentTarget.GilesObjectType);
            }
        }
    }
}
