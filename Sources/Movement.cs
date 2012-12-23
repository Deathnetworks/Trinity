using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using GilesTrinity.Technicals;
using Zeta;
using Zeta.Common;
using Zeta.Common.Plugins;
using Zeta.CommonBot;
using Zeta.Internals.Actors;
using Zeta.Internals.SNO;
using Zeta.Pathfinding;
namespace GilesTrinity
{
    public partial class GilesTrinity : IPlugin
    {
        // Special Zig-Zag movement for whirlwind/tempest
        public static Vector3 FindZigZagTargetLocation(Vector3 vTargetLocation, float fDistanceOutreach, bool bRandomizeDistance = false, bool bRandomizeStart = false, bool bCheckGround = false)
        {
            Vector3 vThisZigZag = vNullLocation;
            Random rndNum = new Random(int.Parse(Guid.NewGuid().ToString().Substring(0, 8), NumberStyles.HexNumber));
            bool bCanRayCast;
            float iFakeStart = 0;
            //K: bRandomizeStart is for boss and elite, they usually jump around, make obstacles, let you Incapacitated. 
            //   you usually have to move back and forth to hit them
            if (bRandomizeStart)
                iFakeStart = rndNum.Next(18) * 5;
            if (bRandomizeDistance)
                fDistanceOutreach += rndNum.Next(18);
            float fDirectionToTarget = FindDirectionDegree(playerStatus.CurrentPosition, vTargetLocation);

            float fPointToTarget;

            float fHighestWeight = float.NegativeInfinity;
            Vector3 vBestLocation = vNullLocation;

            bool bFoundSafeSpotsFirstLoop = false;
            float fAdditionalRange = 0f;
            //K: Direction is more important than distance
            for (int iMultiplier = 1; iMultiplier <= 2; iMultiplier++)
            {
                if (iMultiplier == 2)
                {
                    if (bFoundSafeSpotsFirstLoop)
                        break;
                    fAdditionalRange = 150f;
                    if (bRandomizeStart)
                        iFakeStart = 30f + (rndNum.Next(16) * 5);
                    else
                        iFakeStart = (rndNum.Next(17) * 5);
                }
                float fRunDistance = fDistanceOutreach;
                for (float iDegreeChange = iFakeStart; iDegreeChange <= 30f + fAdditionalRange; iDegreeChange += 5)
                {
                    float iPosition = iDegreeChange;
                    //point to target is better, otherwise we have to avoid obstacle first 
                    if (iPosition > 105f)
                        iPosition = 90f - iPosition;
                    else if (iPosition > 30f)
                        iPosition -= 15f;
                    else
                        iPosition = 15f - iPosition;
                    fPointToTarget = iPosition;

                    iPosition += fDirectionToTarget;
                    if (iPosition < 0)
                        iPosition = 360f + iPosition;
                    if (iPosition >= 360f)
                        iPosition = iPosition - 360f;
                    vThisZigZag = MathEx.GetPointAt(playerStatus.CurrentPosition, fRunDistance, MathEx.ToRadians(iPosition));
                    if (fPointToTarget <= 30f || fPointToTarget >= 330f)
                    {
                        vThisZigZag.Z = vTargetLocation.Z;
                    }
                    else if (fPointToTarget <= 60f || fPointToTarget >= 300f)
                    {
                        //K: we are trying to find position that we can circle around the target
                        //   but we shouldn't run too far away from target
                        vThisZigZag.Z = (vTargetLocation.Z + playerStatus.CurrentPosition.Z) / 2;
                        fRunDistance = fDistanceOutreach - 5f;
                    }
                    else
                    {
                        //K: don't move too far if we are not point to target, we just try to move
                        //   this can help a lot when we are near stairs
                        fRunDistance = 8f;
                    }

                    if (bCheckGround)
                    {
                        vThisZigZag.Z = gp.GetHeight(vThisZigZag.ToVector2());
                        bCanRayCast = ZetaDia.Physics.Raycast(playerStatus.CurrentPosition, vThisZigZag, NavCellFlags.AllowWalk);
                    }
                    else
                        bCanRayCast = pf.IsNavigable(gp.WorldToGrid(vThisZigZag.ToVector2()));
                    // Give weight to each zigzag point, so we can find the best one to aim for
                    if (bCanRayCast)
                    {
                        bool bAnyAvoidance = false;
                        float fThisWeight = 1000f;
                        if (iMultiplier == 2)
                            fThisWeight -= 80f;

                        if (PlayerKiteDistance > 0)
                        {
                            if (hashMonsterObstacleCache.Any(m => m.Location.Distance(vThisZigZag) <= PlayerKiteDistance))
                                continue;
                        }

                        // Remove weight for each avoidance *IN* that location
                        if (hashAvoidanceObstacleCache.Any(m => m.Location.Distance(vThisZigZag) <= m.Radius))
                            continue;

                        foreach (GilesObstacle tempobstacle in hashAvoidanceObstacleCache.Where(cp => GilesIntersectsPath(cp.Location, cp.Radius * 1.2f, playerStatus.CurrentPosition, vThisZigZag)))
                        {
                            bAnyAvoidance = true;
                            //fThisWeight -= (float)tempobstacle.Weight;
                            fThisWeight = 0;
                        }
                        // Give extra weight to areas we've been inside before
                        bool bExtraSafetyWeight = hashSkipAheadAreaCache.Any(cp => cp.Location.Distance(vThisZigZag) <= cp.Radius);
                        if (bExtraSafetyWeight)
                            fThisWeight += 100f;
                        // Use this one if it's more weight, or we haven't even found one yet, or if same weight as another with a random chance
                        if (fThisWeight > fHighestWeight)
                        {
                            fHighestWeight = fThisWeight;
                            vBestLocation = new Vector3(vThisZigZag.X, vThisZigZag.Y, gp.GetHeight(vThisZigZag.ToVector2()));
                            if (!bAnyAvoidance)
                                bFoundSafeSpotsFirstLoop = true;
                        }
                    }
                    // Can we raycast to the point at minimum?
                }
                // Loop through degrees
            }
            // Loop through multiplier
            return vBestLocation;
        }
        // Quick Easy Raycast Function for quick changes
        public static bool GilesCanRayCast(Vector3 vStartLocation, Vector3 vDestination, NavCellFlags NavType = NavCellFlags.AllowWalk)
        {
            if (ZetaDia.Physics.Raycast(new Vector3(vStartLocation.X, vStartLocation.Y, vStartLocation.Z + 3f), new Vector3(vDestination.X, vDestination.Y, vDestination.Z + 3f), NavType))
            {
                if (hashNavigationObstacleCache.Any(o => MathEx.IntersectsPath(o.Location, o.Radius, vStartLocation, vDestination)))
                    return false;
                else
                    return true;
            }
            return false;
        }
        // Calculate direction of A to B
        // Quickly calculate the direction a vector is from ourselves, and return it as a float
        public static float FindDirectionDegree(Vector3 vStartLocation, Vector3 vTargetLocation)
        {
            return (float)RadianToDegree(NormalizeRadian((float)Math.Atan2(vTargetLocation.Y - vStartLocation.Y, vTargetLocation.X - vStartLocation.X)));
        }
        // Find A Safe Movement Location
        private static bool bAvoidDirectionBlacklisting = false;
        private static float fAvoidBlacklistDirection = 0f;

        /// <summary>
        /// This will find a safe place to stand in both Kiting and Avoidance situations
        /// </summary>
        /// <param name="isStuck"></param>
        /// <param name="stuckAttempts"></param>
        /// <param name="dangerPoint"></param>
        /// <param name="shouldKite"></param>
        /// <param name="avoidDeath"></param>
        /// <returns></returns>
        public static Vector3 FindSafeZone(bool isStuck, int stuckAttempts, Vector3 dangerPoint, bool shouldKite = false, IEnumerable<GilesObject> monsterList = null)
        {
            if (!isStuck)
            {
                if (shouldKite && DateTime.Now.Subtract(lastFoundSafeSpot).TotalMilliseconds <= 1500 && vlastSafeSpot != vNullLocation)
                {
                    return vlastSafeSpot;
                }
                else if (!IsAvoidingProjectiles && DateTime.Now.Subtract(lastFoundSafeSpot).TotalMilliseconds <= 800 && vlastSafeSpot != vNullLocation)
                {
                    return vlastSafeSpot;
                }
                hasEmergencyTeleportUp = (
                    // Leap is available
                    (!playerStatus.IsIncapacitated && hashPowerHotbarAbilities.Contains(SNOPower.Barbarian_Leap) &&
                        DateTime.Now.Subtract(dictAbilityLastUse[SNOPower.Barbarian_Leap]).TotalMilliseconds >= dictAbilityRepeatDelay[SNOPower.Barbarian_Leap]) ||
                    // Whirlwind is available
                    (!playerStatus.IsIncapacitated && hashPowerHotbarAbilities.Contains(SNOPower.Barbarian_Whirlwind) &&
                        ((playerStatus.CurrentEnergy >= 10 && !playerStatus.WaitingForReserveEnergy) || playerStatus.CurrentEnergy >= iWaitingReservedAmount)) ||
                    // Tempest rush is available
                    (!playerStatus.IsIncapacitated && hashPowerHotbarAbilities.Contains(SNOPower.Monk_TempestRush) &&
                        ((playerStatus.CurrentEnergy >= 20 && !playerStatus.WaitingForReserveEnergy) || playerStatus.CurrentEnergy >= iWaitingReservedAmount)) ||
                    // Teleport is available
                    (!playerStatus.IsIncapacitated && hashPowerHotbarAbilities.Contains(SNOPower.Wizard_Teleport) &&
                        playerStatus.CurrentEnergy >= 15 &&
                        PowerManager.CanCast(SNOPower.Wizard_Teleport)) ||
                    // Archon Teleport is available
                    (!playerStatus.IsIncapacitated && hashPowerHotbarAbilities.Contains(SNOPower.Wizard_Archon_Teleport) &&
                        PowerManager.CanCast(SNOPower.Wizard_Archon_Teleport))
                    );
                // Wizards can look for bee stings in range and try a wave of force to dispel them
                if (!shouldKite && playerStatus.ActorClass == ActorClass.Wizard && hashPowerHotbarAbilities.Contains(SNOPower.Wizard_WaveOfForce) && playerStatus.CurrentEnergy >= 25 &&
                    DateTime.Now.Subtract(dictAbilityLastUse[SNOPower.Wizard_WaveOfForce]).TotalMilliseconds >= dictAbilityRepeatDelay[SNOPower.Wizard_WaveOfForce] &&
                    !playerStatus.IsIncapacitated && hashAvoidanceObstacleCache.Count(u => u.ActorSNO == 5212 && u.Location.Distance(playerStatus.CurrentPosition) <= 15f) >= 2 &&
                    (Settings.Combat.Wizard.CriticalMass || PowerManager.CanCast(SNOPower.Wizard_WaveOfForce)))
                {
                    ZetaDia.Me.UsePower(SNOPower.Wizard_WaveOfForce, vNullLocation, iCurrentWorldID, -1);
                }
            }
            // Only looking for an anti-stuck location?
            // Now find a randomized safe point we can actually move to

            // We randomize the order so we don't spam walk by accident back and forth
            //Random rndNum = new Random(int.Parse(Guid.NewGuid().ToString().Substring(0, 8), NumberStyles.HexNumber));
            //int DirectionStartDegree = (rndNum.Next(36)) * 10;

            //DirectionStartDegree = (int)(FindDirectionDegree(playerStatus.CurrentPosition, dangerPoint) - 180);

            float fHighestWeight = 0f;
            Vector3 vBestLocation = vNullLocation;

            #region oldjunk
            //// Start off checking every 12 degrees (which is 30 loops for a full circle)
            //const int iMaxRadiusChecks = 30;

            //// 
            //const int iRadiusMultiplier = 12;

            //// Multiply ring size by this
            //const int KiteRingMultiplier = 3;

            //for (int concentricRingStep = 0; concentricRingStep <= 11; concentricRingStep++)
            //{
            //    // Distance of 10 for each step loop at first
            //    int concentricRingDistance = 10;

            //    if (shouldKite)
            //    {
            //        /* 10+(6*1), 10+(6*2), 10+(6*n)... 
            //         * Should be between 10 and 90-ish yards
            //         */
            //        concentricRingDistance = PlayerKiteDistance + (KiteRingMultiplier * concentricRingStep);

            //    }
            //    else
            //    {
            //        // Avoidance
            //        switch (concentricRingStep)
            //        {
            //            case 1: concentricRingDistance = 10; break;
            //            case 2: concentricRingDistance = 18; break;
            //            case 3: concentricRingDistance = 26; break;
            //            case 4: concentricRingDistance = 34; break;
            //            case 5: concentricRingDistance = 42; break;
            //            case 6: concentricRingDistance = 50; break;
            //            case 7: concentricRingDistance = 58; break;
            //            case 8: concentricRingDistance = 66; break;
            //            default:
            //                concentricRingDistance = concentricRingDistance + (concentricRingStep * 8);
            //                break;
            //        }
            //    }

            //    int iRandomUse = 3 + Math.Max(((concentricRingStep - 1) * 4), 1);

            //    // Try to return "early", or as soon as possible, beyond step 4, except when unstucking, when the max steps is based on the unstuck attempt
            //    if (fHighestWeight > 0 &&
            //        ((!isStuck && concentricRingStep > 5) || (isStuck && concentricRingStep > stuckAttempts))
            //        )
            //    {
            //        lastFoundSafeSpot = DateTime.Now;
            //        vlastSafeSpot = vBestLocation;
            //        break;
            //    }
            //    // Loop through all possible radii
            //    for (int iThisRadius = 0; iThisRadius < iMaxRadiusChecks; iThisRadius++)
            //    {
            //        int iPosition = DirectionStartDegree + (iThisRadius * iRadiusMultiplier);

            //        if (iPosition >= 360)
            //            iPosition -= 360;

            //        float fBonusAmount = 0f;
            //        // See if we've blacklisted a 70 degree arc around this direction
            //        if (bAvoidDirectionBlacklisting)
            //        {
            //            if (Math.Abs(fAvoidBlacklistDirection - iPosition) <= 35 || Math.Abs(fAvoidBlacklistDirection - iPosition) >= 325)
            //                continue;
            //            if (Math.Abs(fAvoidBlacklistDirection - iPosition) >= 145 || Math.Abs(fAvoidBlacklistDirection - iPosition) <= 215)
            //                fBonusAmount = 200f;
            //        }
            //        Vector3 vTestPoint = MathEx.GetPointAt(playerStatus.CurrentPosition, concentricRingDistance, MathEx.ToRadians(iPosition));
            //        // First check no avoidance obstacles in this spot
            //        if (!hashAvoidanceObstacleCache.Any(u => u.Location.Distance(vTestPoint) <= GetAvoidanceRadius(u.ActorSNO)))
            //        {
            //            bool bAvoidBlackspot = hashAvoidanceBlackspot.Any(cp => Vector3.Distance(cp.Location, vTestPoint) <= cp.Radius);
            //            bool bCanRaycast = false;

            //            // Now see if the client can navigate there, and we haven't temporarily blacklisted this spot
            //            //if (!bAvoidBlackspot)
            //            //{
            //            //    bCanRaycast = GilesCanRayCast(playerStatus.vCurrentPosition, vTestPoint, NavCellFlags.AllowWalk);
            //            //}

            //            if (!bAvoidBlackspot)
            //            {
            //                bCanRaycast = pf.IsNavigable(gp.WorldToGrid(vTestPoint.ToVector2()));
            //            }
            //            if (!bAvoidBlackspot && bCanRaycast)
            //            {
            //                // Now calculate a weight to pick the "best" avoidance safety spot at the moment
            //                float pointWeight = 1000f + fBonusAmount;
            //                if (!isStuck)
            //                {
            //                    pointWeight -= ((concentricRingStep - 1) * 150);
            //                }
            //                // is it near a point we'd prefer to be close to?
            //                if (dangerPoint != vNullLocation)
            //                {
            //                    float fDistanceToNearby = Vector3.Distance(vTestPoint, dangerPoint);
            //                    if (fDistanceToNearby <= 25f)
            //                    {
            //                        if (!shouldKite)
            //                            pointWeight += (160 * (1 - (fDistanceToNearby / 25)));
            //                        else
            //                            pointWeight -= (300 * (1 - (fDistanceToNearby / 25)));
            //                    }
            //                }
            //                // Give extra weight to areas we've been inside before
            //                bool bExtraSafetyWeight = hashSkipAheadAreaCache.Any(cp => cp.Location.Distance(vTestPoint) <= cp.Radius);
            //                if (bExtraSafetyWeight)
            //                {
            //                    if (shouldKite)
            //                    {
            //                        pointWeight += 350f;
            //                    }
            //                    else if (isStuck)
            //                    {
            //                        pointWeight += 300f;
            //                    }
            //                    else
            //                    {
            //                        pointWeight += 100f;
            //                    }
            //                }
            //                // See if we should check for avoidance spots and monsters in the pathing
            //                if (!isStuck)
            //                {
            //                    Vector3 point = vTestPoint;
            //                    int iMonsterCount = hashMonsterObstacleCache.Count(cp => GilesIntersectsPath(cp.Location, cp.Radius, playerStatus.CurrentPosition, point));
            //                    pointWeight -= (iMonsterCount * 30);
            //                    foreach (GilesObstacle tempobstacle in hashAvoidanceObstacleCache.Where(cp => GilesIntersectsPath(cp.Location, cp.Radius * 2f, playerStatus.CurrentPosition, point)))
            //                    {
            //                        // We don't want to kite through avoidance... 
            //                        if (shouldKite)
            //                            pointWeight = 0;
            //                        else
            //                            pointWeight -= (float)(tempobstacle.Weight * 0.6);
            //                    }
            //                    foreach (GilesObstacle tempobstacle in hashMonsterObstacleCache.Where(cp => GilesIntersectsPath(cp.Location, cp.Radius * 2f, playerStatus.CurrentPosition, point)))
            //                    {
            //                        // We don't want to kite through monsters... 
            //                        if (shouldKite)
            //                            pointWeight = 0;
            //                        else
            //                            pointWeight -= (float)(tempobstacle.Weight * 0.6);
            //                    }
            //                    if (shouldKite)
            //                    {
            //                        foreach (GilesObstacle tempobstacle in hashNavigationObstacleCache.Where(cp => GilesIntersectsPath(cp.Location, cp.Radius * 2f, playerStatus.CurrentPosition, point)))
            //                        {
            //                            // We don't want to kite through obstacles...
            //                            pointWeight = 0;
            //                        }
            //                    }
            //                    foreach (GilesObstacle tempobstacle in hashMonsterObstacleCache)
            //                    {
            //                        float fDistFromMonster = tempobstacle.Location.Distance(dangerPoint);
            //                        float fDistFromMe = vTestPoint.Distance(dangerPoint);
            //                        if (fDistFromMonster < fDistFromMe)
            //                        {
            //                            // if the vTestPoint is closer to any monster than it is to me, give it less weight
            //                            //fThisWeight -= fDistFromMe * 15;
            //                            pointWeight = 0;
            //                        }
            //                        else
            //                        {
            //                            // otherwise, give it more weight, the further it is from the monster
            //                            pointWeight += fDistFromMe * 15;
            //                        }
            //                    }
            //                }
            //                if (shouldKite)
            //                {
            //                    // Kiting spots don't like to end up near other monsters
            //                    foreach (GilesObstacle tempobstacle in hashMonsterObstacleCache.Where(cp => Vector3.Distance(cp.Location, vTestPoint) <= (cp.Radius + PlayerKiteDistance)))
            //                    {
            //                        pointWeight = 0;
            //                    }
            //                }
            //                if (pointWeight <= 1)
            //                    pointWeight = 1;
            //                // Use this one if it's more weight, or we haven't even found one yet, or if same weight as another with a random chance
            //                //if (fThisWeight > fHighestWeight || fHighestWeight == 0f || (fThisWeight == fHighestWeight && rndNum.Next(iRandomUse) == 1))                            
            //                if (pointWeight > fHighestWeight || fHighestWeight == 0f)
            //                {
            //                    fHighestWeight = pointWeight;
            //                    vBestLocation = vTestPoint;
            //                    // Found a very good spot so just use this one!
            //                    //if (iAOECount == 0 && fThisWeight > 400)
            //                    //    break;
            //                }
            //            }
            //        }
            //    }
            //    // Loop through the circle
            //}
            #endregion


            if (monsterList == null)
                monsterList = new List<GilesObject>();

            vBestLocation = newFindSafeZone(dangerPoint, shouldKite, isStuck);
            fHighestWeight = 1;

            // Loop through distance-range steps
            if (fHighestWeight > 0)
            {
                lastFoundSafeSpot = DateTime.Now;
                vlastSafeSpot = vBestLocation;
            }
            return vBestLocation;
        }

        internal static Vector3 newFindSafeZone(Vector3 origin, bool shouldKite = false, bool isStuck = false, IEnumerable<GilesObject> monsterList = null)
        {
            /*
            generate 50x50 grid of 5x5 squares within max 100 distance from origin to edge of grid
            
            all squares start with 0 weight

            check if Center IsNavigable
            check Z
            check if avoidance is present
            check if monsters are present

            final distance tile weight = (Max Dist - Dist)/Max Dist*Max Weight
              
            end result should be that only navigable squares where no avoidance, monsters, or obstacles are present
            */

            float gridSquareSize = 5f;
            int maxDistance = 100;
            int gridTotalSize = (int)(maxDistance / gridSquareSize) * 2;
            int maxWeight = 100;
            int maxZDiff = 14;

            /* If maxDistance is the radius of a circle from the origin, then we want to get the hypotenuse of the radius (x) and tangent (y) as our search grid corner
             * anything outside of the circle will not be considered
             */
            Vector2 topleft = new Vector2(origin.X - maxDistance, origin.Y - maxDistance);

            //Make a circle on the corners of the square
            double gridSquareRadius = Math.Sqrt((Math.Pow(gridSquareSize / 2, 2) + Math.Pow(gridSquareSize / 2, 2)));

            GridPoint bestPoint = new GridPoint(Vector3.Zero, 0, 0);

            for (int x = 0; x < gridTotalSize; x++)
            {
                for (int y = 0; y < gridTotalSize; y++)
                {
                    Vector2 xy = new Vector2(topleft.X + (x * gridSquareSize), topleft.Y + (y * gridSquareSize));
                    Vector3 xyz = new Vector3(xy.X, xy.Y, gp.GetHeight(xy));
                    Point p_xy = gp.WorldToGrid(xy);

                    GridPoint gridPoint = new GridPoint(xyz, 0, origin.Distance(xyz));

                    if (gridPoint.Distance > maxDistance + gridSquareRadius)
                        continue;

                    // If the point is navigable but ZDiff is way too different (up a cliff or wall)
                    //if (Math.Abs(gridPoint.Position.Z - origin.Z) > maxZDiff)
                    //{
                    //    continue;
                    //}

                    if (!pf.IsNavigable(p_xy))
                    {
                        continue;
                    }

                    if (gridPoint.Distance > 45 && !ZetaDia.Physics.Raycast(origin, xyz, NavCellFlags.AllowWalk))
                    {
                        continue;
                    }

                    /*
                     * Check if a square is occupied already
                     */
                    // Avoidance
                    if (hashAvoidanceObstacleCache.Any(a => a.Location.Distance(xyz) <= gridSquareRadius + a.Radius))
                    {
                        continue;
                    }
                    // Obstacles
                    if (hashNavigationObstacleCache.Any(a => a.Location.Distance(xyz) <= gridSquareRadius + a.Radius))
                    {
                        continue;
                    }

                    // Monsters
                    if (shouldKite)
                    {
                        // Any monster standing in this GridPoint
                        if (hashMonsterObstacleCache.Any(a => a.Location.Distance(xyz) <= (shouldKite ? gridSquareRadius + a.Radius : gridSquareSize + a.Radius + PlayerKiteDistance)))
                        {
                            continue;
                        }

                        // Any monsters blocking in a straight line between origin and this GridPoint
                        foreach (GilesObstacle monster in hashMonsterObstacleCache.Where(m =>
                            MathEx.IntersectsPath(new Vector3(m.Location.X, m.Location.Y, 0), m.Radius, new Vector3(origin.X, origin.Y, 0), new Vector3(gridPoint.Position.X, gridPoint.Position.Y, 0))
                            ))
                        {

                            continue;
                        }

                        int nearbyMonsters = (monsterList != null ? monsterList.Count() : 0);

                        /*
                         * This little bit is insanely CPU intensive and causes lots of small game freezes, maybe needs GUI option..
                         */
                        if (!hasEmergencyTeleportUp && nearbyMonsters > 3 && gridPoint.Distance <= 75)
                        {
                            PathFindResult pfr = pf.FindPath(gp.WorldToGrid(origin.ToVector2()), p_xy, true, 25, true);

                            bool pathFailure = false;
                            Point lastNode = gp.WorldToGrid(origin.ToVector2());

                            // analyze pathing to a safe point
                            foreach (Point node in pfr.PointsReversed)
                            {
                                Vector2 node_xy = gp.GridToWorld(node);
                                Vector3 node_xyz = new Vector3(node_xy.X, node_xy.Y, gp.GetHeight(node_xy));

                                // never skip first-step nodes
                                if (node_xyz.Distance(origin) < 10)
                                    continue;

                                // ignore any path points where monsters are standing in it
                                if (hashMonsterObstacleCache.Any(m => m.Location.Distance(node_xyz) <= m.Radius))
                                {
                                    pathFailure = true;
                                    break;
                                }
                                // Any monsters blocking previous and this node
                                foreach (GilesObstacle monster in hashMonsterObstacleCache.Where(m =>
                                    MathEx.IntersectsPath(
                                        new Vector3(m.Location.X, m.Location.Y, 0),
                                        m.Radius,
                                        new Vector3(lastNode.X, lastNode.Y, 0),
                                        new Vector3(gridPoint.Position.X, gridPoint.Position.Y, 0))
                                    ))
                                {
                                    pathFailure = true;
                                    break;
                                }
                            }

                            if (pathFailure)
                                continue;
                        }

                    }

                    if (!isStuck)
                    {
                        gridPoint.Weight = ((maxDistance - gridPoint.Distance) / maxDistance) * maxWeight;

                        // Low weight for close range grid points
                        if (shouldKite && gridPoint.Distance < PlayerKiteDistance)
                        {
                            gridPoint.Weight = (int)gridPoint.Distance;
                        }
                    }
                    else
                    {
                        gridPoint.Weight = gridPoint.Distance;
                    }

                    if (shouldKite)
                    {
                        /*
                        * We want to down-weight any grid points where monsters are closer to it than we are
                        */
                        foreach (GilesObstacle monster in hashMonsterObstacleCache)
                        {
                            float distFromMonster = gridPoint.Position.Distance(monster.Location);
                            float distFromOrigin = gridPoint.Position.Distance(origin);
                            if (distFromMonster < distFromOrigin)
                            {
                                gridPoint.Weight -= distFromOrigin;
                            }
                            else if (distFromMonster > distFromOrigin)
                            {
                                gridPoint.Weight += distFromMonster;
                            }
                        }
                    }
                    foreach (GilesObstacle avoidance in hashAvoidanceObstacleCache)
                    {
                        float distFromAvoidance = gridPoint.Position.Distance(avoidance.Location);
                        float distFromOrigin = gridPoint.Position.Distance(origin);
                        if (distFromAvoidance < distFromOrigin)
                        {
                            gridPoint.Weight -= distFromOrigin;
                        }
                        else if (distFromAvoidance > distFromOrigin)
                        {
                            gridPoint.Weight += distFromAvoidance;
                        }
                    }

                    if (gridPoint.Weight > bestPoint.Weight && gridPoint.Distance > 1)
                    {
                        bestPoint = gridPoint;
                    }

                    //if (gridPoint.Weight > 0)
                    //    DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.Moving, "Kiting grid point {0}, distance: {1:0}, weight: {2:0}", gridPoint.Position, gridPoint.Distance, gridPoint.Weight);
                }
            }

            DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.Moving, "Kiting grid found {0}, distance: {1:0}, weight: {2:0}", bestPoint.Position, bestPoint.Distance, bestPoint.Weight);
            return bestPoint.Position;

        }

        internal class GridPoint : IEquatable<GridPoint>
        {
            public Vector3 Position { get; set; }
            public double Weight { get; set; }
            public float Distance { get; set; }

            /// <summary>
            /// Creates a new gridpoint
            /// </summary>
            /// <param name="position">Vector3 Position of the GridPoint</param>
            /// <param name="weight">Weight of the GridPoint</param>
            /// <param name="distance">Distance to the Position</param>
            public GridPoint(Vector3 position, int weight, float distance)
            {
                this.Position = position;
                this.Weight = weight;
                this.Distance = distance;
            }

            public bool Equals(GridPoint other)
            {
                return Vector3.Equals(Position, other.Position);
            }
        }
        public static double GetRelativeAngularVariance(Vector3 origin, Vector3 destA, Vector3 destB)
        {
            float fDirectionToTarget = NormalizeRadian((float)Math.Atan2(destA.Y - origin.Y, destA.X - origin.X));
            float fDirectionToObstacle = NormalizeRadian((float)Math.Atan2(destB.Y - origin.Y, destB.X - origin.X));
            return AbsAngularDiffernce(RadianToDegree(fDirectionToTarget), RadianToDegree(fDirectionToObstacle));
        }
        public static double AbsAngularDiffernce(double angleA, double angleB)
        {
            return 180d - Math.Abs(180d - Math.Abs(angleA - angleB));
        }
        // Check if an obstacle is blocking our path
        /// <summary>
        /// Checks if <see cref="obstacle"/> with <see cref="radius"/> is blocking the ray between <see cref="start"/> and <see cref="destination"/>
        /// </summary>
        /// <param name="obstacle"></param>
        /// <param name="radius"></param>
        /// <param name="start"></param>
        /// <param name="destination"></param>
        /// <returns></returns>
        public static bool GilesIntersectsPath(Vector3 obstacle, float radius, Vector3 start, Vector3 destination)
        {
            return MathEx.IntersectsPath(obstacle, radius, start, destination);

            //// should fix height differences (basically makes this a 2D check)
            //start.Z = obstacle.Z;
            //destination.Z = obstacle.Z;

            //double dAngle = GetRelativeAngularVariance(start, obstacle, destination);
            ////if (dAngle > 30)
            ////{
            ////    return false;
            ////}
            ////if (radius <= 1f) radius = 1f;
            ////if (radius >= 15f) radius = 15f;

            //Ray ray = new Ray(start, Vector3.NormalizedDirection(start, destination));

            //Sphere sphere = new Sphere(obstacle, radius);
            //float? nullable = ray.Intersects(sphere);
            //bool result = (nullable.HasValue && (nullable.Value < start.Distance(destination)));
            //return result;
        }
        public static double Normalize180(double angleA, double angleB)
        {
            //Returns an angle in the range -180 to 180
            double diffangle = (angleA - angleB) + 180d;
            diffangle = (diffangle / 360.0);
            diffangle = ((diffangle - Math.Floor(diffangle)) * 360.0d) - 180d;
            return diffangle;
        }
        public static float NormalizeRadian(float radian)
        {
            if (radian < 0)
            {
                double mod = -radian;
                mod %= Math.PI * 2d;
                mod = -mod + Math.PI * 2d;
                return (float)mod;
            }
            return (float)(radian % (Math.PI * 2d));
        }
        public static double RadianToDegree(double angle)
        {
            return angle * (180.0 / Math.PI);
        }
    }
}
