using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using GilesTrinity.DbProvider;
using GilesTrinity.Technicals;
using Zeta;
using Zeta.Common;
using Zeta.Common.Plugins;
using Zeta.CommonBot;
using Zeta.Internals.Actors;
using Zeta.Internals.SNO;
using Zeta.Navigation;
using Zeta.Pathfinding;
namespace GilesTrinity
{
    public partial class GilesTrinity : IPlugin
    {
        // Special Zig-Zag movement for whirlwind/tempest
        public static Vector3 FindZigZagTargetLocation(Vector3 vTargetLocation, float fDistanceOutreach, bool bRandomizeDistance = false, bool bRandomizeStart = false, bool bCheckGround = false)
        {


            Vector3 vThisZigZag = vTargetLocation;
            using (new PerformanceLogger("FindZigZagTargetLocation"))
            {
                using (new PerformanceLogger("FindZigZagTargetLocation.CheckObjectCache"))
                {
                    bool useTargetBasedZigZag = false;
                    float maxDistance = 30f;
                    int minTargets = 2;

                    if (PlayerStatus.ActorClass == ActorClass.Monk)
                    {
                        maxDistance = 20f;
                        minTargets = 3;
                        useTargetBasedZigZag = (Settings.Combat.Monk.TargetBasedZigZag);
                    }
                    if (PlayerStatus.ActorClass == ActorClass.Barbarian)
                    {
                        useTargetBasedZigZag = (Settings.Combat.Barbarian.TargetBasedZigZag);
                    }

                    int eliteCount = GilesObjectCache.Count(u => u.Type == GObjectType.Unit && u.IsBossOrEliteRareUnique);
                    bool shouldZigZagElites = ((CurrentTarget.IsBossOrEliteRareUnique && eliteCount != 1) || eliteCount == 0);

                    if (useTargetBasedZigZag && shouldZigZagElites && !bAnyTreasureGoblinsPresent && GilesObjectCache.Where(o => o.Type == GObjectType.Unit).Count() >= minTargets)
                    {

                        return TargetUtil.GetBestClusterPoint(fDistanceOutreach, fDistanceOutreach, false);

                        //IEnumerable<GilesObject> zigZagTargets =
                        //    from u in GilesObjectCache
                        //    where u.Type == GObjectType.Unit && u.RadiusDistance < maxDistance &&
                        //    !hashAvoidanceObstacleCache.Any(a => Vector3.Distance(u.Position, a.Location) < GetAvoidanceRadius(a.ActorSNO) && PlayerStatus.CurrentHealthPct <= GetAvoidanceHealth(a.ActorSNO))
                        //    select u;
                        //if (zigZagTargets.Count() >= minTargets)
                        //{
                        //    vThisZigZag = zigZagTargets.OrderByDescending(u => u.CentreDistance).FirstOrDefault().Position;
                        //    if (CanRayCast(vThisZigZag))
                        //        return vThisZigZag;
                        //}
                    }
                }

                Random rndNum = new Random(int.Parse(Guid.NewGuid().ToString().Substring(0, 8), NumberStyles.HexNumber));

                // Simple single target, go straight across!
                if (CurrentTarget != null && CurrentTarget.Type == GObjectType.Unit &&
                    GilesObjectCache.Count(u => u.Type == GObjectType.Unit && u.Weight > 0 && u.RadiusDistance <= fDistanceOutreach) == 1)
                {
                    double directionRandom = rndNum.Next(5, 125) * Math.PI * 0.001d; // up to 45 degree randomization
                    if (rndNum.Next(0, 1) == 1)
                        directionRandom *= -1;
                    double targetDirection = NormalizeRadian((float)(FindDirectionRadian(PlayerStatus.CurrentPosition, vTargetLocation) + directionRandom));

                    Vector3 newDestination = MathEx.GetPointAt(vTargetLocation, fDistanceOutreach, (float)targetDirection);
                    if (newDestination != Vector3.Zero && CanRayCast(newDestination))
                        return newDestination;
                }



                using (new PerformanceLogger("FindZigZagTargetLocation.RandomZigZagPoint"))
                {
                    bool bCanRayCast;
                    float iFakeStart = 0;
                    //K: bRandomizeStart is for boss and elite, they usually jump around, make obstacles, let you Incapacitated. 
                    //   you usually have to move back and forth to hit them
                    if (bRandomizeStart)
                        iFakeStart = rndNum.Next(18) * 5;
                    if (bRandomizeDistance)
                        fDistanceOutreach += rndNum.Next(18);
                    float fDirectionToTarget = FindDirectionDegree(PlayerStatus.CurrentPosition, vTargetLocation);

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

                            vThisZigZag = MathEx.GetPointAt(PlayerStatus.CurrentPosition, fRunDistance, MathEx.ToRadians(iPosition));

                            if (fPointToTarget <= 30f || fPointToTarget >= 330f)
                            {
                                vThisZigZag.Z = vTargetLocation.Z;
                            }
                            else if (fPointToTarget <= 60f || fPointToTarget >= 300f)
                            {
                                //K: we are trying to find position that we can circle around the target
                                //   but we shouldn't run too far away from target
                                vThisZigZag.Z = (vTargetLocation.Z + PlayerStatus.CurrentPosition.Z) / 2;
                                fRunDistance = fDistanceOutreach - 5f;
                            }
                            else
                            {
                                //K: don't move too far if we are not point to target, we just try to move
                                //   this can help a lot when we are near stairs
                                fRunDistance = 8f;
                            }

                            if (Settings.Combat.Misc.UseNavMeshTargeting)
                            {
                                if (bCheckGround)
                                {
                                    vThisZigZag.Z = gp.GetHeight(vThisZigZag.ToVector2());
                                    bCanRayCast = !Navigator.Raycast(PlayerStatus.CurrentPosition, vThisZigZag);
                                }
                                else
                                    bCanRayCast = gp.CanStandAt(gp.WorldToGrid(vThisZigZag.ToVector2()));
                            }
                            else
                            {
                                bCanRayCast = !Navigator.Raycast(PlayerStatus.CurrentPosition, vThisZigZag);
                            }

                            // Give weight to each zigzag point, so we can find the best one to aim for
                            if (bCanRayCast)
                            {
                                bool bAnyAvoidance = false;

                                // Starting weight is 1000f
                                float fThisWeight = 1000f;
                                if (iMultiplier == 2)
                                    fThisWeight -= 80f;

                                if (PlayerKiteDistance > 0)
                                {
                                    if (hashMonsterObstacleCache.Any(m => m.Location.Distance(vThisZigZag) <= PlayerKiteDistance))
                                        continue;
                                }

                                // Remove weight for each avoidance *IN* that location
                                if (hashAvoidanceObstacleCache.Any(m => m.Location.Distance(vThisZigZag) <= m.Radius && PlayerStatus.CurrentHealthPct <= GetAvoidanceHealth(m.ActorSNO)))
                                    continue;

                                foreach (GilesObstacle tempobstacle in hashAvoidanceObstacleCache.Where(cp => GilesIntersectsPath(cp.Location, cp.Radius * 1.2f, PlayerStatus.CurrentPosition, vThisZigZag)))
                                {
                                    bAnyAvoidance = true;
                                    //fThisWeight -= (float)tempobstacle.Weight;
                                    fThisWeight = 0;
                                }
                                if (bAnyAvoidance && fThisWeight <= 0)
                                    continue;

                                // Give extra weight to areas we've been inside before
                                //bool bExtraSafetyWeight = hashSkipAheadAreaCache.Any(cp => cp.Location.Distance(vThisZigZag) <= cp.Radius);
                                //if (bExtraSafetyWeight)
                                //    fThisWeight += 100f;

                                float distanceToPoint = vThisZigZag.Distance2D(PlayerStatus.CurrentPosition);
                                float distanceToTarget = vTargetLocation.Distance2D(PlayerStatus.CurrentPosition);
                                float distanceFromTargetToPoint = vThisZigZag.Distance2D(vTargetLocation);

                                fThisWeight += (distanceToPoint * 10f);

                                // Use this one if it's more weight, or we haven't even found one yet, or if same weight as another with a random chance
                                if (fThisWeight > fHighestWeight)
                                {
                                    fHighestWeight = fThisWeight;

                                    if (Settings.Combat.Misc.UseNavMeshTargeting)
                                    {
                                        vBestLocation = new Vector3(vThisZigZag.X, vThisZigZag.Y, gp.GetHeight(vThisZigZag.ToVector2()));
                                    }
                                    else
                                    {
                                        vBestLocation = new Vector3(vThisZigZag.X, vThisZigZag.Y, vThisZigZag.Z + 4);
                                    }
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
            }
        }

        public static bool CanRayCast(Vector3 destination)
        {
            return CanRayCast(PlayerStatus.CurrentPosition, destination);
        }

        // Quick Easy Raycast Function for quick changes
        public static bool CanRayCast(Vector3 vStartLocation, Vector3 vDestination, float ZDiff = 4f)
        {
            // Navigator.Raycast is REVERSE Of ZetaDia.Physics.Raycast
            // Navigator.Raycast returns True if it "hits" an edge
            // ZetaDia.Physics.Raycast returns False if it "hits" an edge
            // So ZetaDia.Physics.Raycast() == !Navigator.Raycast()
            // We're using Navigator.Raycast now because it's "faster" (per Nesox)

            bool rc = Navigator.Raycast(new Vector3(vStartLocation.X, vStartLocation.Y, vStartLocation.Z + ZDiff), new Vector3(vDestination.X, vDestination.Y, vDestination.Z + ZDiff));

            if (!rc)
            {
                if (hashNavigationObstacleCache.Any(o => MathEx.IntersectsPath(o.Location, o.Radius, vStartLocation, vDestination)))
                    return false;
                else
                    return true;
            }
            return false;
        }
        /// <summary>
        /// Returns the Degree angle of a target location
        /// </summary>
        /// <param name="vStartLocation"></param>
        /// <param name="vTargetLocation"></param>
        /// <returns></returns>
        public static float FindDirectionDegree(Vector3 vStartLocation, Vector3 vTargetLocation)
        {
            return (float)RadianToDegree(NormalizeRadian((float)Math.Atan2(vTargetLocation.Y - vStartLocation.Y, vTargetLocation.X - vStartLocation.X)));
        }
        public static double FindDirectionRadian(Vector3 start, Vector3 end)
        {
            double radian = Math.Atan2(end.Y - start.Y, end.X - start.X);

            if (radian < 0)
            {
                double mod = -radian;
                mod %= Math.PI * 2d;
                mod = -mod + Math.PI * 2d;
                return mod;
            }
            return (radian % (Math.PI * 2d));
        }
        // Find A Safe Movement Location
        //private static bool bAvoidDirectionBlacklisting = false;
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
                if (shouldKite && DateTime.Now.Subtract(lastFoundSafeSpot).TotalMilliseconds <= 1500 && vlastSafeSpot != Vector3.Zero)
                {
                    return vlastSafeSpot;
                }
                else if (DateTime.Now.Subtract(lastFoundSafeSpot).TotalMilliseconds <= 800 && vlastSafeSpot != Vector3.Zero)
                {
                    return vlastSafeSpot;
                }
                hasEmergencyTeleportUp = (
                    // Leap is available
                    (!PlayerStatus.IsIncapacitated && Hotbar.Contains(SNOPower.Barbarian_Leap) &&
                        DateTime.Now.Subtract(dictAbilityLastUse[SNOPower.Barbarian_Leap]).TotalMilliseconds >= dictAbilityRepeatDelay[SNOPower.Barbarian_Leap]) ||
                    // Whirlwind is available
                    (!PlayerStatus.IsIncapacitated && Hotbar.Contains(SNOPower.Barbarian_Whirlwind) &&
                        ((PlayerStatus.PrimaryResource >= 10 && !PlayerStatus.WaitingForReserveEnergy) || PlayerStatus.PrimaryResource >= MinEnergyReserve)) ||
                    // Tempest rush is available
                    (!PlayerStatus.IsIncapacitated && Hotbar.Contains(SNOPower.Monk_TempestRush) &&
                        ((PlayerStatus.PrimaryResource >= 20 && !PlayerStatus.WaitingForReserveEnergy) || PlayerStatus.PrimaryResource >= MinEnergyReserve)) ||
                    // Teleport is available
                    (!PlayerStatus.IsIncapacitated && Hotbar.Contains(SNOPower.Wizard_Teleport) &&
                        PlayerStatus.PrimaryResource >= 15 &&
                        PowerManager.CanCast(SNOPower.Wizard_Teleport)) ||
                    // Archon Teleport is available
                    (!PlayerStatus.IsIncapacitated && Hotbar.Contains(SNOPower.Wizard_Archon_Teleport) &&
                        PowerManager.CanCast(SNOPower.Wizard_Archon_Teleport))
                    );
                // Wizards can look for bee stings in range and try a wave of force to dispel them
                if (!shouldKite && PlayerStatus.ActorClass == ActorClass.Wizard && Hotbar.Contains(SNOPower.Wizard_WaveOfForce) && PlayerStatus.PrimaryResource >= 25 &&
                    DateTime.Now.Subtract(dictAbilityLastUse[SNOPower.Wizard_WaveOfForce]).TotalMilliseconds >= dictAbilityRepeatDelay[SNOPower.Wizard_WaveOfForce] &&
                    !PlayerStatus.IsIncapacitated && hashAvoidanceObstacleCache.Count(u => u.ActorSNO == 5212 && u.Location.Distance(PlayerStatus.CurrentPosition) <= 15f) >= 2 &&
                    (ZetaDia.CPlayer.PassiveSkills.Contains(SNOPower.Wizard_Passive_CriticalMass) || PowerManager.CanCast(SNOPower.Wizard_WaveOfForce)))
                {
                    ZetaDia.Me.UsePower(SNOPower.Wizard_WaveOfForce, vNullLocation, CurrentWorldDynamicId, -1);
                }
            }

            float fHighestWeight = 0f;
            Vector3 vBestLocation = vNullLocation;

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

        internal class UnSafeZone
        {
            public int WorldId { get; set; }
            public Vector3 Position { get; set; }
            public float Radius { get; set; }
            public string Name { get; set; }
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
            int maxDistance = 200;
            int maxWeight = 100;
            int maxZDiff = 14;

            // special settings for Azmodan
            //if (ZetaDia.CurrentWorldId == 121214)
            //{
            //    List<Vector3> AzmoKitePositions = new List<Vector3>()
            //    {
            //        new Vector3(364f, 550f, 0f), // right
            //        new Vector3(533f, 536f, 0f), // bottom
            //        new Vector3(540f, 353f, 0f), // left
            //        new Vector3(368f, 369f, 0f), // top
            //    };

            //    return AzmoKitePositions.OrderByDescending(p => p.Distance2D(origin)).FirstOrDefault();
            //}


            int gridTotalSize = (int)(maxDistance / gridSquareSize) * 2;

            /* If maxDistance is the radius of a circle from the origin, then we want to get the hypotenuse of the radius (x) and tangent (y) as our search grid corner
             * anything outside of the circle will not be considered
             */
            Vector2 topleft = new Vector2(origin.X - maxDistance, origin.Y - maxDistance);




            //Make a circle on the corners of the square
            double gridSquareRadius = Math.Sqrt((Math.Pow(gridSquareSize / 2, 2) + Math.Pow(gridSquareSize / 2, 2)));

            GridPoint bestPoint = new GridPoint(Vector3.Zero, 0, 0);

            int nodesNotNavigable = 0;
            int nodesZDiff = 0;
            int nodesGT45Raycast = 0;
            int nodesAvoidance = 0;
            int nodesMonsters = 0;
            int pathFailures = 0;

            for (int x = 0; x < gridTotalSize; x++)
            {
                for (int y = 0; y < gridTotalSize; y++)
                {
                    Vector2 xy = new Vector2(topleft.X + (x * gridSquareSize), topleft.Y + (y * gridSquareSize));
                    Vector3 xyz = Vector3.Zero;
                    Point p_xy = Point.Empty;

                    if (Settings.Combat.Misc.UseNavMeshTargeting)
                    {
                        xyz = new Vector3(xy.X, xy.Y, gp.GetHeight(xy));
                    }
                    else
                    {
                        xyz = new Vector3(xy.X, xy.Y, origin.Z + 4);
                    }

                    GridPoint gridPoint = new GridPoint(xyz, 0, origin.Distance(xyz));

                    //if (gridPoint.Distance > maxDistance + gridSquareRadius)
                    //    continue;
                    if (Settings.Combat.Misc.UseNavMeshTargeting)
                    {
                        p_xy = gp.WorldToGrid(xy);
                        if (!gp.CanStandAt(p_xy))
                        {
                            nodesNotNavigable++;
                            continue;
                        }
                    }
                    else
                    {
                        // If ZDiff is way too different (up a cliff or wall)
                        if (Math.Abs(gridPoint.Position.Z - origin.Z) > maxZDiff)
                        {
                            nodesZDiff++;
                            continue;
                        }
                    }
                    if (gridPoint.Distance > 45 && Navigator.Raycast(origin, xyz))
                    {
                        nodesGT45Raycast++;
                        continue;
                    }

                    if (isStuck && gridPoint.Distance > (PlayerMover.iTotalAntiStuckAttempts + 2) * 5)
                    {
                        continue;
                    }

                    /*
                     * Check if a square is occupied already
                     */
                    // Avoidance
                    if (hashAvoidanceObstacleCache.Any(a => Vector3.Distance(xyz, a.Location) - a.Radius <= gridSquareRadius))
                    {
                        nodesAvoidance++;
                        continue;
                    }
                    // Obstacles
                    if (hashNavigationObstacleCache.Any(a => Vector3.Distance(xyz, a.Location) - a.Radius <= gridSquareRadius))
                    {
                        nodesMonsters++;
                        continue;
                    }

                    // Monsters
                    if (shouldKite)
                    {
                        // Any monster standing in this GridPoint
                        if (hashMonsterObstacleCache.Any(a => Vector3.Distance(xyz, a.Location) - a.Radius <= (shouldKite ? gridSquareRadius : gridSquareSize + PlayerKiteDistance)))
                        {
                            nodesMonsters++;
                            continue;
                        }

                        if (!hasEmergencyTeleportUp)
                        {
                            // Any monsters blocking in a straight line between origin and this GridPoint
                            foreach (GilesObstacle monster in hashMonsterObstacleCache.Where(m =>
                                MathEx.IntersectsPath(new Vector3(m.Location.X, m.Location.Y, 0), m.Radius, new Vector3(origin.X, origin.Y, 0), new Vector3(gridPoint.Position.X, gridPoint.Position.Y, 0))
                                ))
                            {

                                nodesMonsters++;
                                continue;
                            }
                        }

                        int nearbyMonsters = (monsterList != null ? monsterList.Count() : 0);
                    }

                    if (isStuck && UsedStuckSpots.Any(p => Vector3.Distance(p.Position, gridPoint.Position) <= gridSquareRadius))
                    {
                        continue;
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

                    // Boss Areas
                    if (UnsafeKiteAreas.Any(a => a.WorldId == ZetaDia.CurrentWorldId && Vector3.Distance(a.Position, gridPoint.Position) <= a.Radius))
                    {
                        continue;
                    }

                    if (shouldKite)
                    {
                        // make sure we can raycast to our target
                        if (!CanRayCast(gridPoint.Position, LastPrimaryTargetPosition))
                            continue;

                        /*
                        * We want to down-weight any grid points where monsters are closer to it than we are
                        */
                        foreach (GilesObstacle monster in hashMonsterObstacleCache)
                        {
                            float distFromMonster = gridPoint.Position.Distance2D(monster.Location);
                            float distFromOrigin = gridPoint.Position.Distance2D(origin);
                            float distFromOriginToAvoidance = origin.Distance2D(monster.Location);
                            if (distFromOriginToAvoidance < distFromOrigin)
                                continue;

                            if (distFromMonster < distFromOrigin)
                            {
                                gridPoint.Weight -= distFromOrigin;
                            }
                            else if (distFromMonster > distFromOrigin)
                            {
                                gridPoint.Weight += distFromMonster;
                            }
                        }
                        foreach (GilesObstacle avoidance in hashAvoidanceObstacleCache)
                        {
                            float distFromAvoidance = gridPoint.Position.Distance2D(avoidance.Location);
                            float distFromOrigin = gridPoint.Position.Distance2D(origin);
                            float distFromOriginToAvoidance = origin.Distance2D(avoidance.Location);

                            float health = AvoidanceManager.GetAvoidanceHealthBySNO(avoidance.ActorSNO, 1f);
                            float radius = AvoidanceManager.GetAvoidanceRadiusBySNO(avoidance.ActorSNO, 1f);

                            // position is inside avoidance
                            if (PlayerStatus.CurrentHealthPct < health && distFromAvoidance < radius)
                                continue;

                            // closer to avoidance than it is to player
                            if (distFromOriginToAvoidance < distFromOrigin)
                                continue;

                            if (distFromAvoidance < distFromOrigin)
                            {
                                gridPoint.Weight -= distFromOrigin;
                            }
                            else if (distFromAvoidance > distFromOrigin)
                            {
                                gridPoint.Weight += distFromAvoidance;
                            }
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

            if (isStuck)
            {
                UsedStuckSpots.Add(bestPoint);
            }

            DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.Movement, "Kiting grid found {0}, distance: {1:0}, weight: {2:0}", bestPoint.Position, bestPoint.Distance, bestPoint.Weight);
            DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.Movement, "Kiting grid stats NotNavigable {0} ZDiff {1} GT45Raycast {2} Avoidance {3} Monsters {4} pathFailures {5}",
                nodesNotNavigable,
                nodesZDiff,
                nodesGT45Raycast,
                nodesAvoidance,
                nodesMonsters,
                pathFailures);
            return bestPoint.Position;

        }

        internal static List<GridPoint> UsedStuckSpots = new List<GridPoint>();

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

        internal static void UpdateSearchGridProvider(bool force = false)
        {
            if (!ZetaDia.IsInGame || ZetaDia.IsLoadingWorld)
                return;

            if (Settings.Combat.Misc.UseNavMeshTargeting || force)
            {
                if (ZetaDia.IsInGame)
                {
                    DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.CacheManagement, "Updating Grid Provider", true);
                    gp.Update();
                }
            }

        }
        public static string GetHeadingToPoint(Vector3 TargetPoint)
        {
            return GetHeading(FindDirectionDegree(PlayerStatus.CurrentPosition, TargetPoint));
        }
        public static string GetHeading(float heading)
        {
            var directions = new string[] {
              //"n", "ne", "e", "se", "s", "sw", "w", "nw", "n"
                "s", "se", "e", "ne", "n", "nw", "w", "sw", "s"
            };

            var index = (((int)heading) + 23) / 45;
            return directions[index].ToUpper();
        }
    }
}
