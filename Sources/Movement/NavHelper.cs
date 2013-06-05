using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using Trinity.DbProvider;
using Trinity.Technicals;
using Zeta;
using Zeta.Common;
using Zeta.CommonBot;
using Zeta.Internals.Actors;
using Zeta.Navigation;
using Zeta.Pathfinding;

namespace Trinity
{
    class NavHelper
    {
        #region Fields
        private static DateTime lastFoundSafeSpot = DateTime.MinValue;
        private static Vector3 lastSafeZonePosition = Vector3.Zero;
        private static bool hasEmergencyTeleportUp = false;
        #endregion

        #region Helper fields
        private static List<TrinityCacheObject> ObjectCache
        {
            get
            {
                return Trinity.ObjectCache;
            }
        }
        private static PlayerInfoCache PlayerStatus
        {
            get
            {
                return Trinity.Player;
            }
        }
        private static bool AnyTreasureGoblinsPresent
        {
            get
            {
                if (ObjectCache != null)
                    return ObjectCache.Any(u => u.IsTreasureGoblin);
                else
                    return false;
            }
        }
        private static TrinityCacheObject CurrentTarget
        {
            get
            {
                return Trinity.CurrentTarget;
            }
        }
        private static HashSet<SNOPower> Hotbar
        {
            get
            {
                return Trinity.Hotbar;
            }
        }
        private static ISearchAreaProvider MainGridProvider
        {
            get
            {
                return Trinity.MainGridProvider;
            }
        }
        #endregion

        internal static bool CanRayCast(Vector3 destination)
        {
            return CanRayCast(PlayerStatus.CurrentPosition, destination);
        }

        /// <summary>
        /// Checks the Navigator to see if the destination is in LoS (walkable) and also checks for any navigation obstacles
        /// </summary>
        /// <param name="vStartLocation"></param>
        /// <param name="vDestination"></param>
        /// <param name="ZDiff"></param>
        /// <returns></returns>
        internal static bool CanRayCast(Vector3 vStartLocation, Vector3 vDestination, float ZDiff = 4f)
        {
            // Navigator.Raycast is REVERSE Of ZetaDia.Physics.Raycast
            // Navigator.Raycast returns True if it "hits" an edge
            // ZetaDia.Physics.Raycast returns False if it "hits" an edge
            // So ZetaDia.Physics.Raycast() == !Navigator.Raycast()
            // We're using Navigator.Raycast now because it's "faster" (per Nesox)

            bool rc = Navigator.Raycast(new Vector3(vStartLocation.X, vStartLocation.Y, vStartLocation.Z + ZDiff), new Vector3(vDestination.X, vDestination.Y, vDestination.Z + ZDiff));

            if (!rc)
            {
                if (Trinity.hashNavigationObstacleCache.Any(o => MathEx.IntersectsPath(o.Location, o.Radius, vStartLocation, vDestination)))
                    return false;
                else
                    return true;
            }
            return false;
        }

        /// <summary>
        /// This will find a safe place to stand in both Kiting and Avoidance situations
        /// </summary>
        /// <param name="isStuck"></param>
        /// <param name="stuckAttempts"></param>
        /// <param name="dangerPoint"></param>
        /// <param name="shouldKite"></param>
        /// <param name="avoidDeath"></param>
        /// <returns></returns>
        internal static Vector3 FindSafeZone(bool isStuck, int stuckAttempts, Vector3 dangerPoint, bool shouldKite = false, IEnumerable<TrinityCacheObject> monsterList = null)
        {
            if (!isStuck)
            {
                if (shouldKite && DateTime.Now.Subtract(lastFoundSafeSpot).TotalMilliseconds <= 1500 && lastSafeZonePosition != Vector3.Zero)
                {
                    return lastSafeZonePosition;
                }
                else if (DateTime.Now.Subtract(lastFoundSafeSpot).TotalMilliseconds <= 800 && lastSafeZonePosition != Vector3.Zero)
                {
                    return lastSafeZonePosition;
                }
                hasEmergencyTeleportUp = (
                    // Leap is available
                    (!PlayerStatus.IsIncapacitated && Hotbar.Contains(SNOPower.Barbarian_Leap) &&
                        DateTime.Now.Subtract(Trinity.AbilityLastUsedCache[SNOPower.Barbarian_Leap]).TotalMilliseconds >= DataDictionary.AbilityRepeatDelays[SNOPower.Barbarian_Leap]) ||
                    // Whirlwind is available
                    (!PlayerStatus.IsIncapacitated && Hotbar.Contains(SNOPower.Barbarian_Whirlwind) &&
                        ((PlayerStatus.PrimaryResource >= 10 && !PlayerStatus.WaitingForReserveEnergy) || PlayerStatus.PrimaryResource >= Trinity.MinEnergyReserve)) ||
                    // Tempest rush is available
                    (!PlayerStatus.IsIncapacitated && Hotbar.Contains(SNOPower.Monk_TempestRush) &&
                        ((PlayerStatus.PrimaryResource >= 20 && !PlayerStatus.WaitingForReserveEnergy) || PlayerStatus.PrimaryResource >= Trinity.MinEnergyReserve)) ||
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
                    DateTime.Now.Subtract(Trinity.AbilityLastUsedCache[SNOPower.Wizard_WaveOfForce]).TotalMilliseconds >= DataDictionary.AbilityRepeatDelays[SNOPower.Wizard_WaveOfForce] &&
                    !PlayerStatus.IsIncapacitated && Trinity.hashAvoidanceObstacleCache.Count(u => u.ActorSNO == 5212 && u.Location.Distance(PlayerStatus.CurrentPosition) <= 15f) >= 2 &&
                    (ZetaDia.CPlayer.PassiveSkills.Contains(SNOPower.Wizard_Passive_CriticalMass) || PowerManager.CanCast(SNOPower.Wizard_WaveOfForce)))
                {
                    ZetaDia.Me.UsePower(SNOPower.Wizard_WaveOfForce, Vector3.Zero, PlayerStatus.WorldDynamicID, -1);
                }
            }

            float fHighestWeight = 0f;
            Vector3 vBestLocation = Vector3.Zero;

            if (monsterList == null)
                monsterList = new List<TrinityCacheObject>();

            vBestLocation = FindSafeZone(dangerPoint, shouldKite, isStuck);
            fHighestWeight = 1;

            // Loop through distance-range steps
            if (fHighestWeight > 0)
            {
                lastFoundSafeSpot = DateTime.Now;
                lastSafeZonePosition = vBestLocation;
            }
            return vBestLocation;
        }

        internal static Vector3 FindSafeZone(Vector3 origin, bool shouldKite = false, bool isStuck = false, IEnumerable<TrinityCacheObject> monsterList = null)
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

                    if (Trinity.Settings.Combat.Misc.UseNavMeshTargeting)
                    {
                        xyz = new Vector3(xy.X, xy.Y, MainGridProvider.GetHeight(xy));
                    }
                    else
                    {
                        xyz = new Vector3(xy.X, xy.Y, origin.Z + 4);
                    }

                    GridPoint gridPoint = new GridPoint(xyz, 0, origin.Distance(xyz));

                    //if (gridPoint.Distance > maxDistance + gridSquareRadius)
                    //    continue;
                    if (Trinity.Settings.Combat.Misc.UseNavMeshTargeting)
                    {
                        p_xy = MainGridProvider.WorldToGrid(xy);
                        if (!MainGridProvider.CanStandAt(p_xy))
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
                    if (Trinity.hashAvoidanceObstacleCache.Any(a => Vector3.Distance(xyz, a.Location) - a.Radius <= gridSquareRadius))
                    {
                        nodesAvoidance++;
                        continue;
                    }
                    // Obstacles
                    if (Trinity.hashNavigationObstacleCache.Any(a => Vector3.Distance(xyz, a.Location) - a.Radius <= gridSquareRadius))
                    {
                        nodesMonsters++;
                        continue;
                    }

                    // Monsters
                    if (shouldKite)
                    {
                        double checkRadius = gridSquareRadius;

                        if (Trinity.PlayerKiteDistance > 0)
                        {
                            checkRadius = gridSquareSize + Trinity.PlayerKiteDistance;
                        }

                        // Any monster standing in this GridPoint
                        if (Trinity.hashMonsterObstacleCache.Any(a => Vector3.Distance(xyz, a.Location) + a.Radius <= checkRadius))
                        {
                            nodesMonsters++;
                            continue;
                        }

                        if (!hasEmergencyTeleportUp)
                        {
                            // Any monsters blocking in a straight line between origin and this GridPoint
                            foreach (CacheObstacleObject monster in Trinity.hashMonsterObstacleCache.Where(m =>
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
                        if (shouldKite && gridPoint.Distance < Trinity.PlayerKiteDistance)
                        {
                            gridPoint.Weight = (int)gridPoint.Distance;
                        }
                    }
                    else
                    {
                        gridPoint.Weight = gridPoint.Distance;
                    }

                    // Boss Areas
                    if (UnSafeZone.UnsafeKiteAreas.Any(a => a.WorldId == ZetaDia.CurrentWorldId && Vector3.Distance(a.Position, gridPoint.Position) <= a.Radius))
                    {
                        continue;
                    }

                    if (shouldKite)
                    {
                        // make sure we can raycast to our target
                        if (!NavHelper.CanRayCast(gridPoint.Position, Trinity.LastPrimaryTargetPosition))
                            continue;

                        /*
                        * We want to down-weight any grid points where monsters are closer to it than we are
                        */
                        foreach (CacheObstacleObject monster in Trinity.hashMonsterObstacleCache)
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
                        foreach (CacheObstacleObject avoidance in Trinity.hashAvoidanceObstacleCache)
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
                    else if (isStuck)
                    {
                        // give weight to points nearer to our destination
                        gridPoint.Weight *= (maxDistance - PlayerMover.LastMoveToTarget.Distance2D(gridPoint.Position)) / maxDistance * maxWeight;
                    }
                    else if (!shouldKite && !isStuck) // melee avoidance use only
                    {
                        var monsterCount = Trinity.ObjectCache.Count(u => u.Type == GObjectType.Unit && u.Position.Distance2D(gridPoint.Position) <= gridSquareRadius);
                        if (monsterCount > 0)
                            gridPoint.Weight *= monsterCount;
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

            Logger.Log(TrinityLogLevel.Verbose, LogCategory.Movement, "Kiting grid found {0}, distance: {1:0}, weight: {2:0}", bestPoint.Position, bestPoint.Distance, bestPoint.Weight);
            Logger.Log(TrinityLogLevel.Verbose, LogCategory.Movement, "Kiting grid stats NotNavigable {0} ZDiff {1} GT45Raycast {2} Avoidance {3} Monsters {4} pathFailures {5}",
                nodesNotNavigable,
                nodesZDiff,
                nodesGT45Raycast,
                nodesAvoidance,
                nodesMonsters,
                pathFailures);
            return bestPoint.Position;

        }

        internal static List<GridPoint> UsedStuckSpots = new List<GridPoint>();


    }
    internal class UnSafeZone
    {
        public int WorldId { get; set; }
        public Vector3 Position { get; set; }
        public float Radius { get; set; }
        public string Name { get; set; }

        /// <summary>
        /// Spots where we should not kite to (used during boss fights)
        /// </summary>
        internal static HashSet<UnSafeZone> UnsafeKiteAreas = new HashSet<UnSafeZone>()
        {
            { 
                new UnSafeZone() {
                    WorldId = 182976, 
                    Position = (new Vector3(281.0147f,361.5885f,20.86533f)),
                    Name = "Chamber of Queen Araneae",
                    Radius = 90f
                }
            },
            {
                new UnSafeZone()
                {
                    WorldId = 78839,
                    Position = (new Vector3(59.50927f,60.12386f,0.100002f)),
                    Name = "Chamber of Suffering (Butcher)",
                    Radius = 120f
                }
            },
            {
                new UnSafeZone()
                {
                    WorldId = 109143,
                    Position = (new Vector3(355.8749f,424.0184f,-14.9f)),
                    Name = "Izual",
                    Radius = 120f
                }
            },
            {
                new UnSafeZone()
                {
                    WorldId = 121214,
                    Position = new Vector3(579, 582, 21),
                    Name = "Azmodan",
                    Radius = 120f
                }
            }
        };
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

}
