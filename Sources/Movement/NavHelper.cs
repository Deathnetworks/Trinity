using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Trinity.Combat.Abilities;
using Trinity.DbProvider;
using Trinity.Technicals;
using Zeta.Bot;
using Zeta.Bot.Navigation;
using Zeta.Common;
using Zeta.Game;
using Zeta.Game.Internals.Actors;
using Logger = Trinity.Technicals.Logger;

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
        private static Zeta.Bot.Navigation.MainGridProvider MainGridProvider
        {
            get
            {
                return Trinity.MainGridProvider;
            }
        }
        #endregion

        internal static string PrettyPrintVector3(Vector3 pos)
        {
            return string.Format("x=\"{0:0}\" y=\"{0:1}\" z=\"{0:2}\"", pos.X, pos.Y, pos.Z);
        }

        internal static bool CanRayCast(Vector3 destination)
        {
            return CanRayCast(PlayerStatus.Position, destination);
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

            if (rc)
                return false;

            return !CacheData.NavigationObstacles.Any(o => MathEx.IntersectsPath(o.Position, o.Radius, vStartLocation, vDestination));
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
        internal static Vector3 FindSafeZone(bool isStuck, int stuckAttempts, Vector3 dangerPoint, bool shouldKite = false, IEnumerable<TrinityCacheObject> monsterList = null, bool avoidDeath = false)
        {
            if (!isStuck)
            {
                if (shouldKite && DateTime.UtcNow.Subtract(lastFoundSafeSpot).TotalMilliseconds <= 1500 && lastSafeZonePosition != Vector3.Zero)
                {
                    return lastSafeZonePosition;
                }
                else if (DateTime.UtcNow.Subtract(lastFoundSafeSpot).TotalMilliseconds <= 800 && lastSafeZonePosition != Vector3.Zero)
                {
                    return lastSafeZonePosition;
                }
                hasEmergencyTeleportUp = (
                    // Leap is available
                    (!PlayerStatus.IsIncapacitated && CombatBase.CanCast(SNOPower.Barbarian_Leap)) ||
                    // Whirlwind is available
                    (!PlayerStatus.IsIncapacitated && CombatBase.CanCast(SNOPower.Barbarian_Whirlwind) &&
                        ((PlayerStatus.PrimaryResource >= 10 && !PlayerStatus.WaitingForReserveEnergy) || PlayerStatus.PrimaryResource >= Trinity.MinEnergyReserve)) ||
                    // Tempest rush is available
                    (!PlayerStatus.IsIncapacitated && CombatBase.CanCast(SNOPower.Monk_TempestRush) &&
                        ((PlayerStatus.PrimaryResource >= 20 && !PlayerStatus.WaitingForReserveEnergy) || PlayerStatus.PrimaryResource >= Trinity.MinEnergyReserve)) ||
                    // Teleport is available
                    (!PlayerStatus.IsIncapacitated && CombatBase.CanCast(SNOPower.Wizard_Teleport) && PlayerStatus.PrimaryResource >= 15) ||
                    // Archon Teleport is available
                    (!PlayerStatus.IsIncapacitated && CombatBase.CanCast(SNOPower.Wizard_Archon_Teleport))
                    );
                // Wizards can look for bee stings in range and try a wave of force to dispel them
                if (!shouldKite && PlayerStatus.ActorClass == ActorClass.Wizard && Hotbar.Contains(SNOPower.Wizard_WaveOfForce) && PlayerStatus.PrimaryResource >= 25 &&
                    DateTime.UtcNow.Subtract(CacheData.AbilityLastUsed[SNOPower.Wizard_WaveOfForce]).TotalMilliseconds >= CombatBase.GetSNOPowerUseDelay(SNOPower.Wizard_WaveOfForce) &&
                    !PlayerStatus.IsIncapacitated && CacheData.TimeBoundAvoidance.Count(u => u.ActorSNO == 5212 && u.Position.Distance(PlayerStatus.Position) <= 15f) >= 2 &&
                    (
                    //ZetaDia.CPlayer.PassiveSkills.Contains(SNOPower.Wizard_Passive_CriticalMass) || 
                    PowerManager.CanCast(SNOPower.Wizard_WaveOfForce)))
                {
                    ZetaDia.Me.UsePower(SNOPower.Wizard_WaveOfForce, Vector3.Zero, PlayerStatus.WorldDynamicID, -1);
                }
            }

            float highestWeight = 0f;

            if (monsterList == null)
                monsterList = new List<TrinityCacheObject>();

            Vector3 vBestLocation = FindSafeZone(dangerPoint, shouldKite, isStuck, monsterList, avoidDeath);
            highestWeight = 1;

            // Loop through distance-range steps
            if (highestWeight <= 0)
                return vBestLocation;

            lastFoundSafeSpot = DateTime.UtcNow;
            lastSafeZonePosition = vBestLocation;
            return vBestLocation;
        }

        internal static Vector3 FindSafeZone(Vector3 origin, bool shouldKite = false, bool isStuck = false, IEnumerable<TrinityCacheObject> monsterList = null, bool avoidDeath = false)
        {
            MainGridProvider.Update();
            Navigator.Clear();

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

            const float gridSquareSize = 2.5f;
            const int maxDistance = 150;
            const int maxWeight = 100;
            const int maxZDiff = 14;

            const int gridTotalSize = (int)(maxDistance / gridSquareSize) * 2;

            /* If maxDistance is the radius of a circle from the origin, then we want to get the hypotenuse of the radius (x) and tangent (y) as our search grid corner
             * anything outside of the circle will not be considered
             */
            Vector2 topleft = new Vector2(origin.X - maxDistance, origin.Y - maxDistance);


            //Make a circle on the corners of the square
            double gridSquareRadius = Math.Sqrt((Math.Pow(gridSquareSize / 2, 2) + Math.Pow(gridSquareSize / 2, 2)));

            GridPoint bestPoint = new GridPoint(Vector3.Zero, 0, 0);

            int nodesCantStand = 0;
            int nodesZDiff = 0;
            int nodesGT45Raycast = 0;
            int nodesAvoidance = 0;
            int nodesMonsters = 0;
            int pathFailures = 0;
            int navRaycast = 0;
            int pointsFound = 0;

            // Not sure if I need this...
            MainGridProvider.Update();

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

                    if (Trinity.Settings.Combat.Misc.UseNavMeshTargeting)
                    {
                        p_xy = MainGridProvider.WorldToGrid(xy);

                        if (!MainGridProvider.CanStandAt(p_xy))
                        {
                            nodesCantStand++;
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
                    //if (!DataDictionary.StraightLinePathingLevelAreaIds.Contains(Trinity.Player.LevelAreaId) &&
                    //    gridPoint.Distance > 45 && !Navigator.Raycast(origin, xyz))
                    if (!DataDictionary.StraightLinePathingLevelAreaIds.Contains(Trinity.Player.LevelAreaId) &&
                        Navigator.Raycast(origin, xyz))
                    {
                        nodesGT45Raycast++;
                        continue;
                    }

                    if (isStuck && gridPoint.Distance > (PlayerMover.TotalAntiStuckAttempts + 2) * 5)
                    {
                        continue;
                    }

                    /*
                     * Check if a square is occupied already
                     */
                    // Avoidance
                    if (CacheData.TimeBoundAvoidance.Any(a => Vector3.Distance(xyz, a.Position) - a.Radius <= gridSquareRadius))
                    {
                        nodesAvoidance++;
                        continue;
                    }

                    // Obstacles
                    if (CacheData.NavigationObstacles.Any(a => Vector3.Distance(xyz, a.Position) - a.Radius <= gridSquareRadius))
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
                        if (CacheData.MonsterObstacles.Any(a => Vector3.Distance(xyz, a.Position) + a.Radius <= checkRadius))
                        {
                            nodesMonsters++;
                            continue;
                        }

                        if (!hasEmergencyTeleportUp)
                        {
                            // Any monsters blocking in a straight line between origin and this GridPoint
                            foreach (CacheObstacleObject monster in CacheData.MonsterObstacles.Where(m =>
                                MathEx.IntersectsPath(new Vector3(m.Position.X, m.Position.Y, 0), m.Radius, new Vector3(origin.X, origin.Y, 0), new Vector3(gridPoint.Position.X, gridPoint.Position.Y, 0))
                                ))
                            {

                                nodesMonsters++;
                                continue;
                            }
                        }

                    }

                    if (isStuck && UsedStuckSpots.Any(p => Vector3.Distance(p.Position, gridPoint.Position) <= gridSquareRadius))
                    {
                        continue;
                    }

                    // set base weight
                    if (!isStuck && !avoidDeath)
                    {
                        // e.g. ((100 - 15) / 100) * 100) = 85
                        // e.g. ((100 - 35) / 100) * 100) = 65
                        // e.g. ((100 - 75) / 100) * 100) = 25
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
                        if (!DataDictionary.StraightLinePathingLevelAreaIds.Contains(Trinity.Player.LevelAreaId) &&
                            !NavHelper.CanRayCast(gridPoint.Position, Trinity.LastPrimaryTargetPosition))
                        {
                            navRaycast++;
                            continue;
                        }

                        /*
                        * We want to down-weight any grid points where monsters are closer to it than we are
                        */
                        foreach (CacheObstacleObject monster in CacheData.MonsterObstacles)
                        {
                            float distFromMonster = gridPoint.Position.Distance2D(monster.Position);
                            float distFromOrigin = gridPoint.Position.Distance2D(origin);
                            float distFromOriginToAvoidance = origin.Distance2D(monster.Position);
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
                        foreach (CacheObstacleObject avoidance in CacheData.TimeBoundAvoidance)
                        {
                            float distFromAvoidance = gridPoint.Position.Distance2D(avoidance.Position);
                            float distFromOrigin = gridPoint.Position.Distance2D(origin);
                            float distFromOriginToAvoidance = origin.Distance2D(avoidance.Position);

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
                    else if (!shouldKite && !isStuck && !avoidDeath) // melee avoidance use only
                    {
                        var monsterCount = Trinity.ObjectCache.Count(u => u.IsUnit && u.Position.Distance2D(gridPoint.Position) <= gridSquareRadius);
                        if (monsterCount > 0)
                            gridPoint.Weight *= monsterCount;
                    }

                    pointsFound++;

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
            Logger.Log(TrinityLogLevel.Verbose, LogCategory.Movement, "Kiting grid stats CantStand {0} ZDiff {1} GT45Raycast {2} Avoidance {3} Monsters {4} pathFailures {5} navRaycast {6} pointsFound {7}",
                nodesCantStand,
                nodesZDiff,
                nodesGT45Raycast,
                nodesAvoidance,
                nodesMonsters,
                pathFailures,
                navRaycast,
                pointsFound);
            return bestPoint.Position;

        }

        internal static Vector3 SimpleUnstucker()
        {
            // Clear caches just in case
            

            var myPos = Trinity.Player.Position;
            var navigationPos = PlayerMover.LastMoveToTarget;
            float rotation = (float)Trinity.Player.Rotation;

            const double totalPoints = 3 * Math.PI / 2;
            const double start = Math.PI / 2;
            const double step = Math.PI / 4;

            const float minDistance = 10f;
            const float maxDistance = 45f;
            const float stepDistance = 5f;

            HashSet<GridPoint> gridPoints = new HashSet<GridPoint>();

            int raycastFail = 0;
            int navigationObstacleFail = 0;

            for (double r = start; r <= totalPoints; r += step)
            {
                for (float d = minDistance; d <= maxDistance; d += stepDistance)
                {
                    float newDirection = (float)(rotation + r);
                    Vector3 newPos = MathEx.GetPointAt(myPos, d, newDirection);

                    // If this hits a navigation wall, skip it
                    if (Navigator.Raycast(myPos, newPos))
                    {
                        raycastFail++;
                        continue;
                    }
                    // If this hits a known navigation obstacle, skip it
                    if (CacheData.NavigationObstacles.Any(o => MathEx.IntersectsPath(o.Position, o.Radius, myPos, newPos)))
                    {
                        navigationObstacleFail++;
                        continue;
                    }
                    // use distance as weight
                    GridPoint gridPoint = new GridPoint(newPos, (int)d, d);
                }
            }

            if (!gridPoints.Any())
            {
                Logger.LogDebug(LogCategory.Navigator, "Unable to generage new unstucker position! rayCast={0} navObsticle={1}", raycastFail, navigationObstacleFail);
                return navigationPos;
            }
            else
            {
                Navigator.Clear();
                
                var bestPoint = gridPoints.OrderByDescending(p => p.Weight).FirstOrDefault();
                Logger.LogDebug(LogCategory.Navigator, "Using unstucker position {0} distance={1:0.0} rayCast={2} navObsticle={3}",
                    NavHelper.PrettyPrintVector3(bestPoint.Position), bestPoint.Distance, raycastFail, navigationObstacleFail);
                return bestPoint.Position;
            }
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
