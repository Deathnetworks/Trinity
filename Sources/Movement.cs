using System;
using System.Globalization;
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
                    
                    if (bCheckGround)   {
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
                        // Remove weight for each avoidance *IN* that location
                        foreach (GilesObstacle tempobstacle in hashAvoidanceObstacleCache.Where(cp => GilesIntersectsPath(cp.Location, cp.Radius * 1.2f, playerStatus.CurrentPosition, vThisZigZag)))
                        {
                            bAnyAvoidance = true;
                            fThisWeight -= (float)tempobstacle.Weight;
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
                return true;
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
        public static Vector3 FindSafeZone(bool bFindAntiStuckSpot, int iAntiStuckAttempts, Vector3 vNearbyPoint, bool bKitingSpot = false)
        {
            if (!bFindAntiStuckSpot)
            {
                // Already searched & found a safe spot in last 800 milliseconds, stick to it
                if (!bTravellingAvoidance && DateTime.Now.Subtract(lastFoundSafeSpot).TotalMilliseconds <= 800 && vlastSafeSpot != vNullLocation)
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
                if (!bKitingSpot && iMyCachedActorClass == ActorClass.Wizard && hashPowerHotbarAbilities.Contains(SNOPower.Wizard_WaveOfForce) && playerStatus.CurrentEnergy >= 25 &&
                    DateTime.Now.Subtract(dictAbilityLastUse[SNOPower.Wizard_WaveOfForce]).TotalMilliseconds >= dictAbilityRepeatDelay[SNOPower.Wizard_WaveOfForce] &&
                    !playerStatus.IsIncapacitated && hashAvoidanceObstacleCache.Count(u => u.SNOID == 5212 && u.Location.Distance(playerStatus.CurrentPosition) <= 15f) >= 2 &&
                    (Settings.Combat.Wizard.CriticalMass || PowerManager.CanCast(SNOPower.Wizard_WaveOfForce)))
                {
                    ZetaDia.Me.UsePower(SNOPower.Wizard_WaveOfForce, vNullLocation, iCurrentWorldID, -1);
                }
            }
            // Only looking for an anti-stuck location?
            // Now find a randomized safe point we can actually move to

            // We randomize the order so we don't spam walk by accident back and forth
            Random rndNum = new Random(int.Parse(Guid.NewGuid().ToString().Substring(0, 8), NumberStyles.HexNumber));
            int iFakeStart = (rndNum.Next(36)) * 10;

            iFakeStart = (int)(FindDirectionDegree(playerStatus.CurrentPosition, vNearbyPoint) - 180);

            float fHighestWeight = 0f;
            Vector3 vBestLocation = vNullLocation;
            // Start off checking every 12 degrees (which is 30 loops for a full circle)
            const int iMaxRadiusChecks = 30;
            const int iRadiusMultiplier = 12;
            for (int iStepDistance = 1; iStepDistance <= 12; iStepDistance++)
            {
                // Distance of 10 for each step loop at first
                int iDistanceOut = 10;

                int iKiteStepSize = 6;
                if (bKitingSpot)
                {
                    iDistanceOut = iKiteDistance + (iKiteStepSize * iStepDistance);

                    //switch (iStepDistance)
                    //{

                    //    case 8: iDistanceOut = 10; break;
                    //    case 7: iDistanceOut = 15; break;
                    //    case 6: iDistanceOut = 20; break;
                    //    case 5: iDistanceOut = 27; break;
                    //    case 4: iDistanceOut = 33; break;
                    //    case 3: iDistanceOut = 44; break;
                    //    case 2: iDistanceOut = 55; break;
                    //    case 1: iDistanceOut = 66; break;
                    //}
                }
                else
                {
                    switch (iStepDistance)
                    {
                        case 1: iDistanceOut = 10; break;
                        case 2: iDistanceOut = 18; break;
                        case 3: iDistanceOut = 26; break;
                        case 4: iDistanceOut = 34; break;
                        case 5: iDistanceOut = 42; break;
                        case 6: iDistanceOut = 50; break;
                        case 7: iDistanceOut = 58; break;
                        case 8: iDistanceOut = 66; break;
                        default:
                            iDistanceOut = iDistanceOut + (iStepDistance * 8);
                            break;
                    }
                }
                int iRandomUse = 3 + ((iStepDistance - 1) * 4);
                // Try to return "early", or as soon as possible, beyond step 4, except when unstucking, when the max steps is based on the unstuck attempt
                //if (fHighestWeight > 0 &&
                //    ((!bFindAntiStuckSpot && iStepDistance > 5) || (bFindAntiStuckSpot && iStepDistance > iAntiStuckAttempts))
                //    )
                //{
                //    lastFoundSafeSpot = DateTime.Now;
                //    vlastSafeSpot = vBestLocation;
                //    break;
                //}
                // Loop through all possible radii
                for (int iThisRadius = 0; iThisRadius < iMaxRadiusChecks; iThisRadius++)
                {
                    int iPosition = iFakeStart + (iThisRadius * iRadiusMultiplier);
                    if (iPosition >= 360)
                        iPosition -= 360;
                    float fBonusAmount = 0f;
                    // See if we've blacklisted a 70 degree arc around this direction
                    if (bAvoidDirectionBlacklisting)
                    {
                        if (Math.Abs(fAvoidBlacklistDirection - iPosition) <= 35 || Math.Abs(fAvoidBlacklistDirection - iPosition) >= 325)
                            continue;
                        if (Math.Abs(fAvoidBlacklistDirection - iPosition) >= 145 || Math.Abs(fAvoidBlacklistDirection - iPosition) <= 215)
                            fBonusAmount = 200f;
                    }
                    Vector3 vTestPoint = MathEx.GetPointAt(playerStatus.CurrentPosition, iDistanceOut, MathEx.ToRadians(iPosition));
                    // First check no avoidance obstacles in this spot
                    if (!hashAvoidanceObstacleCache.Any(u => u.Location.Distance(vTestPoint) <= GetAvoidanceRadius(u.SNOID)))
                    {
                        bool bAvoidBlackspot = hashAvoidanceBlackspot.Any(cp => Vector3.Distance(cp.Location, vTestPoint) <= cp.Radius);
                        bool bCanRaycast = false;
                        // Now see if the client can navigate there, and we haven't temporarily blacklisted this spot
                        //if (!bAvoidBlackspot)
                        //{
                        //    bCanRaycast = GilesCanRayCast(playerStatus.vCurrentPosition, vTestPoint, NavCellFlags.AllowWalk);
                        //}
                        if (!bAvoidBlackspot)
                        {
                            bCanRaycast = pf.IsNavigable(gp.WorldToGrid(vTestPoint.ToVector2()));
                        }
                        if (!bAvoidBlackspot && bCanRaycast)
                        {
                            // Now calculate a weight to pick the "best" avoidance safety spot at the moment
                            float fThisWeight = 1000f + fBonusAmount;
                            if (!bFindAntiStuckSpot)
                            {
                                fThisWeight -= ((iStepDistance - 1) * 150);
                            }
                            // is it near a point we'd prefer to be close to?
                            if (vNearbyPoint != vNullLocation)
                            {
                                float fDistanceToNearby = Vector3.Distance(vTestPoint, vNearbyPoint);
                                if (fDistanceToNearby <= 25f)
                                {
                                    if (!bKitingSpot)
                                        fThisWeight += (160 * (1 - (fDistanceToNearby / 25)));
                                    else
                                        fThisWeight -= (300 * (1 - (fDistanceToNearby / 25)));
                                }
                            }
                            // Give extra weight to areas we've been inside before
                            bool bExtraSafetyWeight = hashSkipAheadAreaCache.Any(cp => cp.Location.Distance(vTestPoint) <= cp.Radius);
                            if (bExtraSafetyWeight)
                            {
                                if (bKitingSpot)
                                {
                                    fThisWeight += 350f;
                                }
                                else if (bFindAntiStuckSpot)
                                {
                                    fThisWeight += 300f;
                                }
                                else
                                {
                                    fThisWeight += 100f;
                                }
                            }
                            // See if we should check for avoidance spots and monsters in the pathing
                            if (!bFindAntiStuckSpot)
                            {
                                Vector3 point = vTestPoint;
                                int iMonsterCount = hashMonsterObstacleCache.Count(cp => GilesIntersectsPath(cp.Location, cp.Radius, playerStatus.CurrentPosition, point));
                                fThisWeight -= (iMonsterCount * 30);
                                foreach (GilesObstacle tempobstacle in hashAvoidanceObstacleCache.Where(cp => GilesIntersectsPath(cp.Location, cp.Radius * 2f, playerStatus.CurrentPosition, point)))
                                {
                                    // We don't want to kite through avoidance... 
                                    if (bKitingSpot)
                                        fThisWeight = 0;
                                    else
                                        fThisWeight -= (float)(tempobstacle.Weight * 0.6);
                                }
                                foreach (GilesObstacle tempobstacle in hashMonsterObstacleCache.Where(cp => GilesIntersectsPath(cp.Location, cp.Radius * 2f, playerStatus.CurrentPosition, point)))
                                {
                                    // We don't want to kite through monsters... 
                                    if (bKitingSpot)
                                        fThisWeight = 0;
                                    else
                                        fThisWeight -= (float)(tempobstacle.Weight * 0.6);
                                }
                                if (bKitingSpot)
                                {
                                    foreach (GilesObstacle tempobstacle in hashNavigationObstacleCache.Where(cp => GilesIntersectsPath(cp.Location, cp.Radius * 2f, playerStatus.CurrentPosition, point)))
                                    {
                                        // We don't want to kite through obstacles...
                                        fThisWeight = 0;
                                    }
                                }
                                foreach (GilesObstacle tempobstacle in hashMonsterObstacleCache)
                                {
                                    float fDistFromMonster = tempobstacle.Location.Distance(vNearbyPoint);
                                    float fDistFromMe = vTestPoint.Distance(vNearbyPoint);
                                    if (fDistFromMonster < fDistFromMe)
                                    {
                                        // if the vTestPoint is closer to any monster than it is to me, give it less weight
                                        //fThisWeight -= fDistFromMe * 15;
                                        fThisWeight = 0;
                                    }
                                    else
                                    {
                                        // otherwise, give it more weight, the further it is from the monster
                                        fThisWeight += fDistFromMe * 15;
                                    }
                                }
                            }
                            if (bKitingSpot)
                            {
                                // Kiting spots don't like to end up near other monsters
                                foreach (GilesObstacle tempobstacle in hashMonsterObstacleCache.Where(cp => Vector3.Distance(cp.Location, vTestPoint) <= (cp.Radius + iKiteDistance)))
                                {
                                    fThisWeight = 0;
                                }
                            }
                            if (fThisWeight <= 1)
                                fThisWeight = 1;
                            // Use this one if it's more weight, or we haven't even found one yet, or if same weight as another with a random chance
                            if (fThisWeight > fHighestWeight || fHighestWeight == 0f || (fThisWeight == fHighestWeight && rndNum.Next(iRandomUse) == 1))
                            {
                                fHighestWeight = fThisWeight;
                                vBestLocation = vTestPoint;
                                // Found a very good spot so just use this one!
                                //if (iAOECount == 0 && fThisWeight > 400)
                                //    break;
                            }
                        }
                    }
                }
                // Loop through the circle
            }
            // Loop through distance-range steps
            if (fHighestWeight > 0)
            {
                lastFoundSafeSpot = DateTime.Now;
                vlastSafeSpot = vBestLocation;
            }
            return vBestLocation;
        }
        // Check if an obstacle is blocking our path
        public static bool GilesIntersectsPath(Vector3 obstacle, float radius, Vector3 start, Vector3 destination)
        {
            float fDirectionToTarget = NormalizeRadian((float)Math.Atan2(destination.Y - start.Y, destination.X - start.X));
            float fDirectionToObstacle = NormalizeRadian((float)Math.Atan2(obstacle.Y - start.Y, obstacle.X - start.X));
            if (Math.Abs(RadianToDegree(fDirectionToTarget) - RadianToDegree(fDirectionToObstacle)) > 30)
            {
                return false;
            }
            if (radius <= 1f) radius = 1f;
            if (radius >= 15f) radius = 15f;
            Ray ray = new Ray(start, Vector3.NormalizedDirection(start, destination));
            Sphere sphere = new Sphere(obstacle, radius);
            float? nullable = ray.Intersects(sphere);
            bool result = (nullable.HasValue && (nullable.Value < start.Distance(destination)));
            return result;
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
