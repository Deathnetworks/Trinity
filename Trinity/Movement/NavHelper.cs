using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using Trinity.Combat.Abilities;
using Trinity.Config.Combat;
using Trinity.Reference;
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
        internal static HashSet<GridNode> SafeGrid = new HashSet<GridNode>();
        private static List<TrinityCacheObject> ObjectCache
        {
            get
            {
                return Trinity.ObjectCache;
            }
        }
        private static CacheData.PlayerCache Player
        {
            get
            {
                return CacheData.Player;
            }
        }
        private static bool AnyTreasureGoblinsPresent
        {
            get
            {
                if (ObjectCache != null)
                    return ObjectCache.Any(u => u.IsTreasureGoblin);
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
                return CacheData.Hotbar.ActivePowers;
            }
        }
        private static MainGridProvider MainGridProvider
        {
            get
            {
                return Trinity.MainGridProvider;
            }
        }
        #endregion

        #region CanRayCast Fields
        internal static bool CanRayCast(Vector3 destination, bool offSet = false)
        {
            return CanRayCast(Player.Position, destination, offSet);
        }
        #endregion
        /// <summary>
        /// Checks the Navigator to see if the destination is in LoS (walkable)
        /// </summary>
        internal static bool CanRayCast(Vector3 vStartLocation, Vector3 vDestination, bool offSet = false)
        {
            // Navigator.Raycast is REVERSE Of ZetaDia.Physics.Raycast
            // Navigator.Raycast returns True if it "hits" an edge
            // ZetaDia.Physics.Raycast returns False if it "hits" an edge
            // So ZetaDia.Physics.Raycast() == !Navigator.Raycast()
            // We're using Navigator.Raycast now because it's "faster" (per Nesox)

            using (new MemorySpy("NavHelper.CanRayCast()"))
            {
                if (offSet)
                {
                    vStartLocation.Z += 2.5f;
                    vDestination.Z += 2.5f;
                }

                foreach (var cacheObstacle in CacheData.NavRayCastObstacles.OrderBy(o => o.Key.Distance2D(vStartLocation)))
                {
                    if (cacheObstacle.Value <= 3f)
                        continue;

                    if (cacheObstacle.Key.Distance2D(vStartLocation) > 60f)
                        continue;

                    if (MathEx.IntersectsPath(cacheObstacle.Key, cacheObstacle.Value, vStartLocation, vDestination))
                        return false;
                }

                if (Navigator.Raycast(vStartLocation, vDestination))
                    return false;

                return true;
            }
        }

        internal static string PrettyPrintVector3(Vector3 pos)
        {
            return string.Format("x=\"{0:0}\" y=\"{1:0}\" z=\"{2:0}\"", pos.X, pos.Y, pos.Z);
        }

        /// <summary>
        /// This will find a safe place to stand in both Kiting and Avoidance situations
        /// </summary>
        /// <param name="isStuck"></param>
        /// <param name="stuckAttempts"></param>
        /// <param name="dangerPoint"></param>
        /// <param name="shouldKite"></param>
        /// <returns></returns>
        internal static Vector3 FindSafeZone(bool isStuck, int stuckAttempts, Vector3 dangerPoint, float minimumRange = 5f, float maximumRange = 100f)
        {
            // Handle The Butcher's Lair
            var butcherFloorPanels = CacheData.AvoidanceObstacles.Where(aoe => DataDictionary.ButcherFloorPanels.Contains(aoe.ActorSNO)).ToList();
            if (butcherFloorPanels.Any())
            {
                foreach (var bestSafeGridPointoint in DataDictionary.ButcherPanelPositions.OrderBy(p => p.Value.Distance2DSqr(Player.Position)))
                {
                    // Floor panel with fire animation was added to cache
                    if (butcherFloorPanels.Any(p => p.ActorSNO == bestSafeGridPointoint.Key && p.Position.Distance2DSqr(bestSafeGridPointoint.Value) <= 15f * 15f))
                    {
                        continue;
                    }

                    // floor panel position is in Butcher animation avoidance (charging, chain hook)
                    if (CacheData.AvoidanceObstacles.Any(aoe => aoe.Position.Distance2D(bestSafeGridPointoint.Value) < aoe.Radius))
                        continue;

                    // no avoidance object in cache, this point is safe
                    return bestSafeGridPointoint.Value;
                }

                // Don't fall back to regular avoidance
                return Vector3.Zero;
            }

            if (!isStuck)
            {
                hasEmergencyTeleportUp = (!Player.IsIncapacitated && (
                    // Leap is available
                    (CombatBase.CanCast(SNOPower.Barbarian_Leap)) ||
                    // Whirlwind is available
                    (CombatBase.CanCast(SNOPower.Barbarian_Whirlwind) &&
                        ((Player.PrimaryResource >= 10 && !CombatBase.IsWaitingForSpecial) || Player.PrimaryResource >= Trinity.MinEnergyReserve)) ||
                    // Tempest rush is available
                    (CombatBase.CanCast(SNOPower.Monk_TempestRush) &&
                        ((Player.PrimaryResource >= 20 && !CombatBase.IsWaitingForSpecial) || Player.PrimaryResource >= Trinity.MinEnergyReserve)) ||
                    // Teleport is available
                    (CombatBase.CanCast(SNOPower.Wizard_Teleport) && Player.PrimaryResource >= 15) ||
                    // Archon Teleport is available
                    (CombatBase.CanCast(SNOPower.Wizard_Archon_Teleport))
                    ));
                // Wizards can look for bee stings in range and try a wave of force to dispel them
                if (Player.ActorClass == ActorClass.Wizard && CombatBase.CanCast(SNOPower.Wizard_WaveOfForce) &&
                    !Player.IsIncapacitated && CacheData.AvoidanceObstacles.Count(u => u.ActorSNO == 5212 && u.Position.Distance(Player.Position) <= 15f) >= 2 &&
                    (
                    //HotbarSkills.PassiveSkills.Contains(SNOPower.Wizard_Passive_CriticalMass) || 
                    PowerManager.CanCast(SNOPower.Wizard_WaveOfForce)))
                {
                    ZetaDia.Me.UsePower(SNOPower.Wizard_WaveOfForce, Vector3.Zero, Player.WorldDynamicID, -1);
                }
            }

            return MainFindSafeZone(Player.Position, true, false, null, minimumRange);
        }

        // thanks to Main for the super fast can-stand-at code
        internal static Vector3 MainFindSafeZone(Vector3 origin, bool shouldKite = false, bool isStuck = false, IEnumerable<TrinityCacheObject> monsterList = null, float minimumRange = 5f)
        {
            using (new PerformanceLogger("MainFindSafeZoneLoop"))
            {
                try
                {
                    origin = Player.Position;

                    if (!Player.StandingInAvoidance && !isStuck && DateTime.UtcNow.Subtract(lastFoundSafeSpot).TotalMilliseconds <= 500 ||
                        (Player.StandingInAvoidance && DateTime.UtcNow.Subtract(lastFoundSafeSpot).TotalMilliseconds <= 250))
                    {
                        if (lastSafeZonePosition != Vector3.Zero && lastSafeZonePosition.Distance(Player.Position) >= minimumRange)
                        {
                            return lastSafeZonePosition;
                        }
                        else if (SafeGrid.Any(p => p.Position.Distance2D(origin) >= minimumRange))
                        {
                            return SafeGrid.Where(p => p.Position.Distance2D(origin) >= minimumRange).OrderByDescending(p => p.DynamicWeight).FirstOrDefault().Position;
                        }
                    }

                    Stopwatch[] timers = Enumerable.Range(0, 21).Select(i => new Stopwatch()).ToArray();
                    timers[0].Start();

                    SafeGrid.Clear();
                    #region Fields
                    const float gridSquareSize = 2.1f;
                    const float maxDistance = 85f;

                    double gridSquareRadius = Math.Sqrt((Math.Pow(gridSquareSize / 2, 2) + Math.Pow(gridSquareSize / 2, 2)));

                    GridNode bestPoint = new GridNode(Vector3.Zero);
                    HashSet<GridNode> simpleGrid = new HashSet<GridNode>();

                    int pointsFound = 0;
                    int totalNodes = 0;
                    int nodesCantStand = 0;
                    int nodesAvoidance = 0;
                    int nodesMonsters = 0;
                    List<int> nodesDirection = new List<int> 
                    {
                        0, // S
                        0, // SE
                        0, // E
                        0, // NE
                        0, // N
                        0, // NW
                        0, // W
                        0 // SW
                    };

                    int worldId = Player.WorldID;

                    Vector2 minWorld;
                    minWorld.X = origin.X - maxDistance;
                    minWorld.Y = origin.Y - maxDistance;

                    Point minPoint = MainGridProvider.WorldToGrid(minWorld);
                    minPoint.X = Math.Max(minPoint.X, 0);
                    minPoint.Y = Math.Max(minPoint.Y, 0);

                    Vector2 maxWorld;
                    maxWorld.X = origin.X + maxDistance;
                    maxWorld.Y = origin.Y + maxDistance;

                    Point maxPoint = MainGridProvider.WorldToGrid(maxWorld);
                    maxPoint.X = Math.Min(maxPoint.X, MainGridProvider.Width - 1);
                    maxPoint.Y = Math.Min(maxPoint.Y, MainGridProvider.Height - 1);

                    Point originPos = MainGridProvider.WorldToGrid(origin.ToVector2());

                    if (!monsterList.Any())
                    {
                        monsterList = (from m in ObjectCache where m.IsUnit select m).ToList();
                    }

                    List<TrinityCacheObject> healthGlobeCache =
                        (from u in ObjectCache
                         where u.Type == GObjectType.HealthGlobe &&
                         u.RadiusDistance <= maxDistance
                         select u).ToList();

                    var directions = new string[] { "s", "se", "e", "ne", "n", "nw", "w", "sw", "s" };

                    int nearbyMonsterCount = monsterList.Count(u => u.RadiusDistance < 15f);

                    if (Player.StandingInAvoidance && minimumRange <= 5f)
                        minimumRange = 2f;

                    #endregion

                    HashSet<GridLine> gridMap = new HashSet<GridLine>();
                    int nodeLine = 0;

                    for (int y = minPoint.Y; y <= maxPoint.Y; y = y + (int)gridSquareSize)
                    {
                        GridLine grid = new GridLine(new HashSet<GridNode>(), nodeLine);
                        nodeLine++;

                        int searchAreaBasis = y * MainGridProvider.Width;
                        for (int x = minPoint.X; x <= maxPoint.X; x = x + (int)gridSquareSize)
                        {
                            totalNodes++;

                            timers[1].Start();
                            #region Oor & CanStandAt
                            int dx = originPos.X - x;
                            int dy = originPos.Y - y;

                            // Ignore out of range
                            if (dx * dx + dy * dy > (maxDistance / 2.5f) * (maxDistance / 2.5f))
                            {
                                grid.Line.Add(new GridNode(Vector3.Zero));

                                timers[1].Stop();
                                continue;
                            }
                            // extremely efficient CanStandAt
                            if (!MainGridProvider.SearchArea[searchAreaBasis + x])
                            {
                                nodesCantStand++;
                                grid.Line.Add(new GridNode(Vector3.Zero));

                                timers[1].Stop();
                                continue;
                            }

                            Vector2 xy = MainGridProvider.GridToWorld(new Point(x, y));
                            Vector3 xyz = Vector3.Zero;

                            if (Trinity.Settings.Combat.Misc.UseNavMeshTargeting)
                            {
                                xyz = new Vector3(xy.X, xy.Y, MainGridProvider.GetHeight(xy));
                            }
                            else
                            {
                                xyz = new Vector3(xy.X, xy.Y, origin.Z + 4);
                            }

                            GridNode gridPoint = new GridNode(xyz);
                            #endregion
                            timers[1].Stop();

                            timers[2].Start();
                            #region Origin range
                            if (gridPoint.Position == origin || gridPoint.Distance <= minimumRange)
                            {
                                grid.Line.Add(new GridNode(gridPoint.Position));

                                timers[2].Stop();
                                continue;
                            }
                            #endregion
                            timers[2].Stop();

                            timers[3].Start();
                            #region Boss areas
                            if (UnSafeZone.UnsafeKiteAreas.Any(a => a.WorldId == Player.WorldID && a.Position.Distance2DSqr(gridPoint.Position) <= (a.Radius * a.Radius)))
                            {
                                grid.Line.Add(new GridNode(gridPoint.Position));

                                timers[3].Stop();
                                continue;
                            }
                            #endregion
                            timers[3].Stop();

                            simpleGrid.Add(gridPoint);

                            timers[4].Start();
                            #region Monster check
                            bool ignoreNode = false;
                            foreach (TrinityCacheObject m in monsterList)
                            {
                                if (Player.Position.Distance2D(m.Position) > maxDistance + 10f)
                                {
                                    continue;
                                }

                                float distFromPointToMonster = gridPoint.Position.Distance2D(m.Position);

                                double monsterRadius = (m.Radius * 1.05);
                                if (nearbyMonsterCount > 8 && monsterRadius > 10f)
                                {
                                    monsterRadius = 10f;
                                }

                                // Monster radius not relay safe
                                if (distFromPointToMonster < monsterRadius)
                                {
                                    nodesMonsters++;
                                    grid.Line.Add(new GridNode(gridPoint.Position));

                                    ignoreNode = true;
                                    break;
                                }
                            }

                            if (ignoreNode)
                            {
                                timers[4].Stop();
                                continue;
                            }
                            #endregion
                            timers[4].Stop();

                            timers[5].Start();
                            #region Avoidance check
                            foreach (CacheObstacleObject a in CacheData.AvoidanceObstacles)
                            {
                                if (Player.Position.Distance2D(a.Position) > maxDistance + 10f)
                                {
                                    continue;
                                }

                                float distFromPointToAvoidance = gridPoint.Position.Distance2D(a.Position);

                                // Inside avoidance
                                if (distFromPointToAvoidance < a.Radius)
                                {
                                    nodesAvoidance++;
                                    grid.Line.Add(new GridNode(gridPoint.Position));

                                    ignoreNode = true;
                                    break;
                                }
                            }

                            if (ignoreNode)
                            {
                                timers[5].Stop();
                                continue;
                            }
                            #endregion
                            timers[5].Stop();

                            timers[6].Start();
                            #region Stuck
                            if (isStuck)
                            {
                                if (UsedStuckSpots.Any(p => Vector3.Distance(p.Position, gridPoint.Position) <= gridSquareRadius))
                                {
                                    grid.Line.Add(new GridNode(gridPoint.Position));

                                    timers[6].Stop();
                                    continue;
                                }
                            }
                            #endregion
                            timers[6].Stop();

                            #region Direction++
                            string direction = MathUtil.GetHeadingToPoint(gridPoint.Position);
                            switch (direction)
                            {
                                case "S":
                                    nodesDirection[0]++;
                                    break;
                                case "SE":
                                    nodesDirection[1]++;
                                    break;
                                case "E":
                                    nodesDirection[2]++;
                                    break;
                                case "NE":
                                    nodesDirection[3]++;
                                    break;
                                case "N":
                                    nodesDirection[4]++;
                                    break;
                                case "NW":
                                    nodesDirection[5]++;
                                    break;
                                case "W":
                                    nodesDirection[6]++;
                                    break;
                                case "SW":
                                    nodesDirection[7]++;
                                    break;
                            }
                            #endregion

                            pointsFound++;
                            SafeGrid.Add(gridPoint);

                            grid.Line.Add(new GridNode(gridPoint.Position));
                        }

                        gridMap.Add(grid);
                    }

                    string bestSafeDirection = directions[nodesDirection.IndexOf(nodesDirection.Max())].ToUpper();
                    bool playerIsInBadWay = Player.AvoidDeath ||
                        (nearbyMonsterCount > 8 && Player.CurrentHealthPct <= 0.6) || Player.CurrentHealthPct <= 0.5 ||
                        Player.StandingInAvoidance || minimumRange >= 35f;

                    bestPoint = SetGridWeight(SafeGrid, bestSafeDirection, playerIsInBadWay, minimumRange);

                    if (bestPoint.Position == Vector3.Zero &&
                        SafeGrid.Any(p => p.Distance >= minimumRange))
                    {
                        Logger.Log(TrinityLogLevel.Verbose, LogCategory.Avoidance, "SafeGrid Attempt");
                        bestPoint = SafeGrid
                            .Where(p => p.Distance >= minimumRange)
                            .OrderBy(p => p.Distance)
                            .FirstOrDefault();
                    }
                    if (bestPoint.Position == Vector3.Zero && simpleGrid.Any())
                    {
                        Logger.Log(TrinityLogLevel.Verbose, LogCategory.Avoidance, "SimpleGrid Attempt");

                        // Grid without monster or avoidance, set weight
                        bestPoint = SetGridWeight(simpleGrid, bestSafeDirection, playerIsInBadWay, minimumRange);
                    }
                    if (bestPoint.Position == Vector3.Zero &&
                        PositionCache.Cache.Any(p => p.Position.Distance2D(origin) >= minimumRange))
                    {
                        Logger.Log(TrinityLogLevel.Verbose, LogCategory.Avoidance, "PositionCache Attempt");
                        bestPoint.Position = PositionCache.Cache
                            .Where(p => p.Position.Distance2D(origin) >= minimumRange)
                            .OrderByDescending(p => p.Position.Distance2D(origin))
                            .FirstOrDefault()
                            .Position;
                    }
                    if (bestPoint.Position == Vector3.Zero &&
                        ObjectCache.Any(p => !p.IsUnit && p.Distance >= minimumRange))
                    {
                        Logger.Log(TrinityLogLevel.Verbose, LogCategory.Avoidance, "ObjectCache Attempt");
                        bestPoint.Position = ObjectCache
                            .Where(p => !p.IsUnit && p.Distance >= minimumRange)
                            .OrderByDescending(c => c.Distance)
                            .FirstOrDefault()
                            .Position;
                    }

                    if (isStuck && bestPoint.Position != Vector3.Zero)
                    {
                        UsedStuckSpots.Add(bestPoint);
                    }

                    timers[0].Stop();

                    #region LogDebug
                    /*string line = "";
                    foreach (GridLine grid in gridMap)
                    {
                        foreach (GridPoint gridPoint in SafeGrid)
                        {
                            if (grid.Line.Any(n => n.Equals(gridPoint)))
                            {
                                if (gridPoint.Equals(bestPoint))
                                {
                                    grid.Line.Where(n => n.Equals(gridPoint)).FirstOrDefault().WeightInfos = "(**)";
                                }
                                else
                                {
                                    grid.Line.Where(n => n.Equals(gridPoint)).FirstOrDefault().WeightInfos = "|" + ((gridPoint.Weight * 100) / bestPoint.Weight).ToString("000");
                                }
                            }
                        }

                        foreach (GridPoint gridPoint in grid.Line)
                        {
                            line += gridPoint.WeightInfos;
                        }

                        Logger.Log(TrinityLogLevel.Debug, LogCategory.Avoidance, line);
                        line = "";
                    }*/

                    string debugTimers = "";
                    for (int t = 0; t < timers.Length; t++)
                    {
                        if (timers[t].IsRunning) timers[t].Stop();
                        debugTimers += string.Format("{0}/{1:00} ", t, timers[t].ElapsedMilliseconds);
                    }

                    int totalTimesMs = (int)timers[0].ElapsedMilliseconds;

                    Logger.Log(TrinityLogLevel.Info, LogCategory.Avoidance, "SafePoint generated in {0}ms - {1} Dist={2:1} Weight={3} PointsFound={6}/{7} Power={9} PowerRange={10} KiteDist={11}",
                        totalTimesMs,
                        bestPoint.Position,
                        bestPoint.Position.Distance2D(origin).ToString("F0"),
                        bestPoint.DynamicWeight.ToString("F0"),
                        MathUtil.GetHeadingToPoint(bestPoint.Position),
                        bestSafeDirection,
                        SafeGrid.Count(),
                        pointsFound,
                        bestPoint.DynamicWeightInfos,
                        CombatBase.CurrentPower.SNOPower,
                        CombatBase.CurrentPower.MinimumRange,
                        CombatBase.KiteDistance
                    );

                    Logger.Log(TrinityLogLevel.Verbose, LogCategory.Avoidance, "Safe grid stats Total={0} CantStand={1} Avoidance={2} Monsters={3} bestSafeDirection={9} PointsFound={4} // shouldKite={6} isStuck={5} avoidDeath={7} // timers={8} Status={10}",
                        totalNodes, //0
                        nodesCantStand, //1
                        nodesAvoidance, //2
                        nodesMonsters, //3
                        pointsFound, //4
                        shouldKite, //5
                        isStuck, //6
                        Player.AvoidDeath, //7
                        debugTimers, //8
                        bestSafeDirection, //9
                        bestPoint.DynamicWeightInfos // 10
                    );
                    #endregion

                    lastFoundSafeSpot = DateTime.UtcNow;
                    return bestPoint.Position;
                }
                catch
                {
                    GridNode bestPoint = new GridNode(Vector3.Zero, 0);

                    if (bestPoint.Position == Vector3.Zero &&
                        PositionCache.Cache.Any(p => p.Position.Distance2D(origin) >= minimumRange))
                    {
                        Logger.Log(TrinityLogLevel.Verbose, LogCategory.Avoidance, "Catch PositionCache Attempt");
                        bestPoint.Position = PositionCache.Cache
                            .Where(p => p.Position.Distance2D(origin) >= minimumRange)
                            .OrderBy(p => p.Position.Distance2D(origin))
                            .FirstOrDefault()
                            .Position;
                    }
                    if (bestPoint.Position == Vector3.Zero &&
                        ObjectCache.Any(p => !p.IsUnit && p.Distance >= minimumRange))
                    {
                        Logger.Log(TrinityLogLevel.Verbose, LogCategory.Avoidance, "Catch ObjectCache Attempt");
                        bestPoint.Position = ObjectCache
                            .Where(p => !p.IsUnit && p.Distance >= minimumRange)
                            .OrderByDescending(c => c.Distance)
                            .FirstOrDefault()
                            .Position;
                    }

                    lastFoundSafeSpot = DateTime.UtcNow;
                    return bestPoint.Position;
                }
            }
        }

        internal static GridNode SetGridWeight(HashSet<GridNode> grid, string bestSafeDirection, bool playerIsInBadWay = false, float minimumRange = 5f, float maxDistance = 110f)
        {
            #region SetGridWeight fields

            Stopwatch[] timers = Enumerable.Range(0, 28).Select(i => new Stopwatch()).ToArray();
            timers[0].Start();

            GridNode bestPoint = new GridNode(Vector3.Zero, -1000);

            float maxWeight = 250f;

            float currentPowerMinimumRange = Trinity.Settings.Combat.Misc.NonEliteRange;
            if (CombatBase.CurrentPower.MinimumRange > 0)
            {
                currentPowerMinimumRange = CombatBase.CurrentPower.MinimumRange;
            }
            else if (CombatBase.LastPowerRange > 0)
            {
                currentPowerMinimumRange = CombatBase.LastPowerRange;
            }

            bool playerShouldKite = true;
            if (CombatBase.CurrentPower.SNOPower != SNOPower.None &&
                currentPowerMinimumRange > 2f &&
                currentPowerMinimumRange < CombatBase.KiteDistance)
            {
                playerShouldKite = false;
            }

            bool collectHealthGlobe = false;
            if (Trinity.Settings.Combat.Misc.HiPriorityHG ||
                (Player.CurrentHealthPct < CombatBase.EmergencyHealthGlobeLimit ||
                (Player.PrimaryResourcePct < CombatBase.HealthGlobeResource &&
                (Legendary.ReapersWraps.IsEquipped ||
                (Player.ActorClass == ActorClass.Witchdoctor && CacheData.Hotbar.PassiveSkills.Contains(SNOPower.Witchdoctor_Passive_GruesomeFeast)) ||
                (Player.ActorClass == ActorClass.DemonHunter && CacheData.Hotbar.PassiveSkills.Contains(SNOPower.DemonHunter_Passive_Vengeance))))))
            {
                collectHealthGlobe = true;
            }

            float healthGlobeWeightPct = (float)(1f - Player.CurrentHealthPct) * 10f;
            if (Player.CurrentHealthPct > 1 && Player.PrimaryResourcePct > 1)
            {
                healthGlobeWeightPct += (float)(1f - Player.PrimaryResourcePct) * 10f;
                healthGlobeWeightPct = (float)healthGlobeWeightPct * 0.5f;
            }

            #endregion
            foreach (GridNode gridPoint in grid.OrderByDescending(p => p.Distance))
            {
                #region gridPoint fields
                timers[1].Start();
                gridPoint.OperateWeight(WeightType.Dynamic, "BaseDistanceWeight", (maxDistance - gridPoint.Distance) * 10f);

                float dstFromPlayerToPoint = gridPoint.Position.Distance2D(Player.Position);
                timers[1].Stop();
                #endregion
                foreach (CacheObstacleObject cacheObject in CacheData.AvoidanceObstacles)
                {
                    #region cacheObject fields

                    timers[2].Start();
                    int dstFromObjectToPoint = (int)gridPoint.Position.Distance2D(cacheObject.Position);

                    float dstFromObjectToPlayer = Player.Position.Distance2D(cacheObject.Position);
                    if (dstFromObjectToPlayer > maxDistance)
                    {
                        continue;
                    }
                    timers[2].Stop();

                    #endregion
                    {
                        timers[4].Start();
                        if (dstFromObjectToPoint <= cacheObject.Radius * 1.3)
                        {
                            gridPoint.OperateWeight(WeightType.Dynamic, "CloseToAoe", (maxWeight - dstFromObjectToPoint) * -10f);
                        }
                        timers[4].Stop();
                        timers[5].Start();
                        if (MathUtil.IntersectsPath(cacheObject.Position, cacheObject.Radius, Player.Position, gridPoint.Position, true, true))
                        {
                            gridPoint.OperateWeight(WeightType.Dynamic, "IntersectsPathAoE", (maxWeight - dstFromObjectToPoint + cacheObject.Radius) * -8f);
                        }
                        timers[5].Stop();
                        break;
                    }
                }

                foreach (TrinityCacheObject cacheObject in ObjectCache.OrderBy(c => c.Distance))
                {
                    #region cacheObject fields

                    timers[3].Start();

                    if (cacheObject.Distance > maxDistance)
                    {
                        break;
                    }

                    int dstFromObjectToPoint = (int)(gridPoint.Position.Distance2D(cacheObject.Position) - cacheObject.Radius);

                    timers[3].Stop();

                    #endregion
                    switch (cacheObject.Type)
                    {
                        case GObjectType.Unit:
                            #region Unit
                            {
                                timers[6].Start();
                                if (playerIsInBadWay)
                                {
                                    gridPoint.OperateWeight(WeightType.Dynamic, "AvoidThisUnit", (maxWeight + dstFromObjectToPoint) * 6f);
                                    break;
                                }
                                timers[6].Stop();
                                timers[7].Start();
                                if (currentPowerMinimumRange > 2f && dstFromObjectToPoint <= currentPowerMinimumRange &&
                                    CurrentTarget != null && CurrentTarget.Type == GObjectType.Unit && CurrentTarget.RActorGuid == cacheObject.RActorGuid)
                                {
                                    if (cacheObject.IsInLineOfSightOfPoint(gridPoint.Position))
                                    {
                                        gridPoint.OperateWeight(WeightType.Dynamic, "CanRayCastTarget", (maxWeight + dstFromObjectToPoint) * 10f);

                                        if (playerShouldKite && dstFromObjectToPoint > CombatBase.KiteDistance)
                                        {
                                            gridPoint.OperateWeight(WeightType.Dynamic, "TargetSafePowerRange", (maxWeight + dstFromObjectToPoint) * 8f);
                                        }
                                        else
                                        {
                                            gridPoint.OperateWeight(WeightType.Dynamic, "TargetPowerRange", (maxWeight + dstFromObjectToPoint) * 4f);
                                        }
                                    }
                                }
                                timers[7].Stop();
                                timers[8].Start();
                                if (playerShouldKite && !cacheObject.IsTreasureGoblin && dstFromObjectToPoint < CombatBase.KiteDistance)
                                {
                                    if (cacheObject.IsBoss && CombatBase.KiteMode != KiteMode.Never)
                                    {
                                        gridPoint.OperateWeight(WeightType.Dynamic, "InBossKiteRange", (maxWeight - dstFromObjectToPoint + CombatBase.KiteDistance) * -11f);
                                    }
                                    else if (cacheObject.IsBossOrEliteRareUnique && (CombatBase.KiteMode == KiteMode.Elites || CombatBase.KiteMode == KiteMode.Always))
                                    {
                                        gridPoint.OperateWeight(WeightType.Dynamic, "InEliteKiteRange", (maxWeight - dstFromObjectToPoint + CombatBase.KiteDistance) * -9f);
                                    }
                                    else if (CombatBase.KiteMode == KiteMode.Always)
                                    {
                                        gridPoint.OperateWeight(WeightType.Dynamic, "InMobKiteRange", (maxWeight - dstFromObjectToPoint + CombatBase.KiteDistance) * -7f);
                                    }
                                }
                                timers[8].Stop();
                                timers[9].Start();
                                if (MathUtil.IntersectsPath(cacheObject.Position, cacheObject.Radius, Player.Position, gridPoint.Position, true, true))
                                {
                                    gridPoint.OperateWeight(WeightType.Dynamic, "IntersectsPathUnit", (maxWeight - dstFromObjectToPoint + cacheObject.Radius) * -6f);
                                }
                                timers[9].Stop();

                                break;
                            }
                            #endregion
                        case GObjectType.HealthWell:
                            #region HealthWell
                            {
                                timers[10].Start();
                                if (dstFromObjectToPoint <= 5f && Player.CurrentHealthPct < 0.3)
                                {
                                    if (!cacheObject.IsInLineOfSight)
                                    {
                                        break;
                                    }
                                    gridPoint.OperateWeight(WeightType.Dynamic, "CloseToHealthGlobe", (maxWeight - dstFromPlayerToPoint) * 4f);
                                }
                                timers[10].Stop();
                                break;
                            }
                            #endregion
                        case GObjectType.HealthGlobe:
                            #region HealthGlobe
                            {
                                timers[11].Start();
                                if (dstFromObjectToPoint <= Player.GoldPickupRadius + 2f && collectHealthGlobe)
                                {
                                    if (Trinity.Settings.Combat.Misc.HiPriorityHG)
                                    {
                                        gridPoint.OperateWeight(WeightType.Dynamic, "CloseToHealthGlobeHighPririty", (float)((maxWeight - dstFromPlayerToPoint) * healthGlobeWeightPct * 10f));
                                    }
                                    else if (playerIsInBadWay)
                                    {
                                        gridPoint.OperateWeight(WeightType.Dynamic, "CloseToHealthGlobe&LowHealth", (float)((maxWeight - dstFromPlayerToPoint) * healthGlobeWeightPct * 9f));
                                    }
                                    else
                                    {
                                        gridPoint.OperateWeight(WeightType.Dynamic, "CloseToHealthGlobe", (float)((maxWeight - dstFromPlayerToPoint) * healthGlobeWeightPct * 8f));
                                    }
                                }
                                timers[11].Stop();
                                timers[12].Start();
                                if (MathUtil.IntersectsPath(cacheObject.Position, Player.GoldPickupRadius + 2f, Player.Position, gridPoint.Position))
                                {
                                    if (Trinity.Settings.Combat.Misc.HiPriorityHG)
                                    {
                                        gridPoint.OperateWeight(WeightType.Dynamic, "IntersectsPathHealthGlobe&HiPriority", (float)((maxWeight - dstFromPlayerToPoint) * healthGlobeWeightPct * 10f));
                                    }
                                    else if (playerIsInBadWay)
                                    {
                                        gridPoint.OperateWeight(WeightType.Dynamic, "IntersectsPathHealthGlobe&LowHealth", (float)((maxWeight - dstFromPlayerToPoint) * healthGlobeWeightPct * 9f));
                                    }
                                    else
                                    {
                                        gridPoint.OperateWeight(WeightType.Dynamic, "IntersectsPathHealthGlobe", (float)((maxWeight - dstFromPlayerToPoint) * healthGlobeWeightPct * 8f));
                                    }
                                }
                                timers[12].Stop();
                                break;
                            }
                            #endregion
                        case GObjectType.ProgressionGlobe:
                            #region ProgressionGlobe
                            {
                                timers[13].Start();
                                if (dstFromObjectToPoint <= Player.GoldPickupRadius + 2f)
                                {
                                    gridPoint.OperateWeight(WeightType.Dynamic, "CloseToProgressionGlobe", (maxWeight - dstFromPlayerToPoint) * 3f);
                                }
                                timers[13].Stop();
                                timers[14].Start();
                                if (MathUtil.IntersectsPath(cacheObject.Position, Player.GoldPickupRadius + 2f, Player.Position, gridPoint.Position))
                                {
                                    gridPoint.OperateWeight(WeightType.Dynamic, "IntersectsPathProgressionGlobe", (maxWeight - dstFromPlayerToPoint) * 3f);
                                }
                                timers[14].Stop();
                                break;
                            }
                            #endregion
                        case GObjectType.PowerGlobe:
                            #region PowerGlobe
                            {
                                timers[15].Start();
                                if (dstFromObjectToPoint <= Player.GoldPickupRadius + 2f)
                                {
                                    gridPoint.OperateWeight(WeightType.Dynamic, "CloseToPowerGlobe", (maxWeight - dstFromPlayerToPoint) * 2f);
                                }
                                timers[15].Stop();
                                timers[16].Start();
                                if (MathUtil.IntersectsPath(cacheObject.Position, Player.GoldPickupRadius + 2f, Player.Position, gridPoint.Position))
                                {
                                    gridPoint.OperateWeight(WeightType.Dynamic, "IntersectsPathPowerGlobe", (maxWeight - dstFromPlayerToPoint) * 2f);
                                }
                                timers[16].Stop();
                                break;
                            }
                            #endregion
                        case GObjectType.Gold:
                            #region Gold
                            {
                                timers[17].Start();
                                if (dstFromObjectToPoint <= Player.GoldPickupRadius + 2f)
                                {
                                    if (Legendary.Goldwrap.IsEquipped)
                                    {
                                        gridPoint.OperateWeight(WeightType.Dynamic, "CloseToGold&GoldWrapActive", (maxWeight - dstFromPlayerToPoint) * 10f);
                                    }
                                    else if (Legendary.KymbosGold.IsEquipped && Player.CurrentHealthPct < 0.8)
                                    {
                                        gridPoint.OperateWeight(WeightType.Dynamic, "CloseToGold&GoldWrapActive", (maxWeight - dstFromPlayerToPoint) * 5f);
                                    }
                                    else
                                    {
                                        gridPoint.OperateWeight(WeightType.Dynamic, "CloseToGold", (maxWeight - dstFromPlayerToPoint));
                                    }
                                }
                                timers[17].Stop();
                                timers[18].Start();
                                if (MathUtil.IntersectsPath(cacheObject.Position, Player.GoldPickupRadius + 2f, Player.Position, gridPoint.Position))
                                {
                                    if (Legendary.Goldwrap.IsEquipped)
                                    {
                                        gridPoint.OperateWeight(WeightType.Dynamic, "IntersectsPathGold&GoldWrapActive", (maxWeight - dstFromPlayerToPoint) * 10f);
                                    }
                                    else if (Legendary.KymbosGold.IsEquipped && Player.CurrentHealthPct < 0.8)
                                    {
                                        gridPoint.OperateWeight(WeightType.Dynamic, "IntersectsPathGold&GoldWrapActive", (maxWeight - dstFromPlayerToPoint) * 5f);
                                    }
                                    else
                                    {
                                        gridPoint.OperateWeight(WeightType.Dynamic, "IntersectsPathGold", (maxWeight - dstFromPlayerToPoint) * 1.5f);
                                    }
                                }
                                timers[18].Stop();
                                break;
                            }
                            #endregion
                        case GObjectType.Shrine:
                            #region Shrine
                            {
                                timers[19].Start();
                                if (dstFromObjectToPoint <= 5f)
                                {
                                    if (Trinity.Settings.WorldObject.HiPriorityShrines)
                                    {
                                        gridPoint.OperateWeight(WeightType.Dynamic, "CloseToShrineHiPriority", (maxWeight - dstFromPlayerToPoint) * 6f);
                                    }
                                    else
                                    {
                                        gridPoint.OperateWeight(WeightType.Dynamic, "CloseToShrine", (maxWeight - dstFromPlayerToPoint) * 3f);
                                    }
                                }
                                timers[19].Start();
                                timers[20].Stop();
                                if (MathUtil.IntersectsPath(cacheObject.Position, Player.GoldPickupRadius + 2f, Player.Position, gridPoint.Position))
                                {
                                    if (Trinity.Settings.WorldObject.HiPriorityShrines)
                                    {
                                        gridPoint.OperateWeight(WeightType.Dynamic, "IntersectsPathShrineHiPriority", (maxWeight - dstFromPlayerToPoint) * 6f);
                                    }
                                    else
                                    {
                                        gridPoint.OperateWeight(WeightType.Dynamic, "IntersectsPathShrine", (maxWeight - dstFromPlayerToPoint) * 3f);
                                    }
                                }
                                timers[20].Stop();
                                break;
                            }
                            #endregion
                        case GObjectType.Door:
                        case GObjectType.Barricade:
                            #region NavBlocking
                            {
                                timers[21].Start();
                                if (MathUtil.IntersectsPath(cacheObject.Position, cacheObject.Radius, Player.Position, gridPoint.Position, true, true))
                                {
                                    gridPoint.OperateWeight(WeightType.Dynamic, "IntersectsPathObstacles", (maxWeight - dstFromObjectToPoint + cacheObject.Radius) * -4f);
                                }
                                if (CurrentTarget != null && CurrentTarget.IsUnit && MathUtil.IntersectsPath(cacheObject.Position, cacheObject.Radius, Player.Position, CurrentTarget.Position))
                                {
                                    bestPoint = new GridNode(cacheObject.Position, 500000);
                                    break;
                                }
                                timers[21].Stop();
                                break;
                            }
                            #endregion
                        case GObjectType.Destructible:
                        case GObjectType.Interactable:
                        case GObjectType.Container:
                            #region Container
                            {
                                timers[22].Start();
                                if (dstFromObjectToPoint <= cacheObject.Radius + 5f && !playerIsInBadWay && cacheObject.IsInLineOfSight &&
                                    (Trinity.Settings.WorldObject.HiPriorityContainers ||
                                    ((Legendary.HarringtonWaistguard.IsEquipped && !Legendary.HarringtonWaistguard.IsBuffActive))))
                                {
                                    gridPoint.OperateWeight(WeightType.Dynamic, "IntersectsPathObstacles", (maxWeight - dstFromPlayerToPoint) * 10f);
                                }
                                timers[22].Stop();
                                timers[23].Start();
                                if (MathUtil.IntersectsPath(cacheObject.Position, cacheObject.Radius, Player.Position, gridPoint.Position, true, true))
                                {
                                    gridPoint.OperateWeight(WeightType.Dynamic, "IntersectsPathObstacles", (maxWeight - dstFromObjectToPoint + cacheObject.Radius) * -2f);
                                }
                                timers[23].Stop();
                                break;
                            }
                            #endregion
                        default:
                            break;
                    }
                }

                #region ZoneWeight
                float visitedZoneWeight = 0f;
                timers[24].Start();
                foreach (PositionCache positionCache in PositionCache.Cache.OrderBy(p => p.Position.Distance2D(Player.Position)))
                {
                    #region positionCache fields

                    float dstFromPositionCacheToPoint = gridPoint.Position.Distance2D(positionCache.Position);

                    #endregion
                    if (dstFromPositionCacheToPoint > 15f)
                    {
                        break;
                    }

                    visitedZoneWeight++;
                }
                timers[24].Stop();

                timers[25].Start();
                if (visitedZoneWeight > 0f)
                {
                    gridPoint.OperateWeight(WeightType.Dynamic, "InVisitedZone", (maxWeight - gridPoint.Distance) * Math.Min(visitedZoneWeight, 4f));

                    if (MathUtil.GetHeadingToPoint(gridPoint.Position).Equals(bestSafeDirection))
                    {
                        gridPoint.OperateWeight(WeightType.Dynamic, "InBestSafeDirection", (maxWeight - gridPoint.Distance) * 2f);
                    }
                }
                timers[25].Stop();
                #endregion

                // Best point = best weight & smallest distance
                if (gridPoint.DynamicWeight > bestPoint.DynamicWeight && gridPoint.Distance > 1f ||
                    (gridPoint.DynamicWeight == bestPoint.DynamicWeight && gridPoint.Distance < bestPoint.Distance))
                {
                    bestPoint = gridPoint;
                }
            }

            #region AdditionalWeight
            if (bestPoint.Position == Vector3.Zero &&
                grid.Any(p => p.Distance >= minimumRange))
            {
                Logger.Log(TrinityLogLevel.Verbose, LogCategory.Avoidance, "SafeGrid Attempt 2");
                bestPoint = grid
                    .Where(p => p.Distance >= minimumRange)
                    .OrderByDescending(p => p.DynamicWeight)
                    .FirstOrDefault();
            }
            else
            {
                // Remove bad points
                HashSet<GridNode> gridList = grid;
                gridList.RemoveWhere(gridPoint => gridPoint.DynamicWeight < (bestPoint.DynamicWeight * 0.5));

                if (gridList.Any())
                {
                    timers[26].Start();
                    foreach (GridNode gridPoint in gridList)
                    {
                        gridPoint.OperateWeight(WeightType.Dynamic, "CloseToOTherPoints", (maxWeight - gridPoint.Distance) * Math.Min(gridList.Where(p => p.Position.Distance2D(gridPoint.Position) < 15f).Count(), 8f));

                        if (CanRayCast(gridPoint.Position))
                        {
                            if (playerIsInBadWay)
                            {
                                gridPoint.OperateWeight(WeightType.Dynamic, "PlayerCanRayCastPointHiP", (maxWeight - gridPoint.Distance) * 8f);
                            }
                            else
                            {
                                gridPoint.OperateWeight(WeightType.Dynamic, "PlayerCanRayCastPoint", (maxWeight - gridPoint.Distance) * 5f);
                            }
                        }

                        // Best point = best weight & smallest distance
                        if (gridPoint.DynamicWeight > bestPoint.DynamicWeight && gridPoint.Distance > 1f ||
                            (gridPoint.DynamicWeight == bestPoint.DynamicWeight && gridPoint.Distance < bestPoint.Distance))
                        {
                            bestPoint = gridPoint;
                        }
                    }
                    timers[26].Stop();
                }
            }
            #endregion

            timers[0].Stop();

            string debugTimers = "";
            for (int t = 0; t < timers.Length; t++)
            {
                if (timers[t].IsRunning) timers[t].Stop();
                debugTimers += string.Format("{0}/{1:00} ", t, timers[t].ElapsedMilliseconds);
            }

            Logger.Log(TrinityLogLevel.Verbose, LogCategory.Avoidance, "Safe weight timers =({0})", debugTimers);


            return bestPoint;
        }

        internal static Vector3 SimpleUnstucker()
        {
            var myPos = Player.Position;
            float rotation = Player.Rotation;

            const double totalPoints = 2 * Math.PI;
            const double start = 0;
            const double step = Math.PI / 4;

            const float minDistance = 10f;
            const float maxDistance = 25f;
            const float stepDistance = 5f;

            HashSet<GridNode> gridPoints = new HashSet<GridNode>();

            int navigationObstacleFail = 0;

            for (double r = start; r <= totalPoints; r += step)
            {
                for (float d = minDistance; d <= maxDistance; d += stepDistance)
                {
                    float newDirection = (float)(rotation + r);
                    Vector3 newPos = MathEx.GetPointAt(myPos, d, newDirection);

                    if (!MainGridProvider.CanStandAt(MainGridProvider.WorldToGrid(newPos.ToVector2())))
                    {
                        continue;
                    }

                    // If this hits a known navigation obstacle, skip it
                    if (CacheData.NavigationObstacles.Any(o => MathUtil.IntersectsPath(o.Position, o.Radius, myPos, newPos)))
                    {
                        navigationObstacleFail++;
                        continue;
                    }

                    // use distance as weight
                    gridPoints.Add(new GridNode(newPos, (int)d));
                }
            }

            if (!gridPoints.Any())
            {
                Logger.LogDebug(LogCategory.UserInformation, "Unable to generate new unstucker position! navObsticle={0} - trying RANDOM point!", navigationObstacleFail);

                Random random = new Random();
                int distance = random.Next(5, 30);
                float direction = (float)random.NextDouble();

                return MathEx.GetPointAt(myPos, distance, direction);
            }
            Navigator.Clear();

            var bestPoint = gridPoints.OrderByDescending(p => p.DynamicWeight).FirstOrDefault();
            Logger.LogDebug(LogCategory.UserInformation, "Generated Unstuck position {0} distance={1:0.0} navObsticle={2}",
                NavHelper.PrettyPrintVector3(bestPoint.Position), bestPoint.Distance, navigationObstacleFail);
            return bestPoint.Position;
        }

        internal static List<GridNode> UsedStuckSpots = new List<GridNode>();
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
        internal static HashSet<UnSafeZone> UnsafeKiteAreas = new HashSet<UnSafeZone>
        {
            new UnSafeZone
            {
                WorldId = 182976, 
                Position = (new Vector3(281.0147f,361.5885f,20.86533f)),
                Name = "Chamber of Queen Araneae",
                Radius = 90f
            },
            new UnSafeZone
            {
                WorldId = 78839,
                Position = (new Vector3(54.07843f, 55.02061f, 0.100002f)),
                Name = "Chamber of Suffering (Butcher)",
                Radius = 120f
            },
            new UnSafeZone
            {
                WorldId = 109143,
                Position = (new Vector3(355.8749f,424.0184f,-14.9f)),
                Name = "Izual",
                Radius = 120f
            },
            new UnSafeZone
            {
                WorldId = 121214,
                Position = new Vector3(579, 582, 21),
                Name = "Azmodan",
                Radius = 120f
            },
            new UnSafeZone
            {
                WorldId = 308446, 
                Position = new Vector3(469.9994f, 355.01f, -15.85094f),
                Name = "Urzael",
                Radius = (new Vector3(375.144f, 359.9929f, 0.1f)).Distance2D(new Vector3(469.9994f, 355.01f, -15.85094f)),
            }
        };
    }

    internal class GridLine : IEquatable<GridLine>
    {
        public HashSet<GridNode> Line { get; set; }
        public int LineId { get; set; }

        public GridLine(HashSet<GridNode> line, int lineId)
        {
            Line = line;
            LineId = lineId;
        }

        public bool Equals(GridLine other)
        {
            return Equals(Line, other.Line);
        }
    }
}
