using System;
using System.Linq;
using Zeta;
using Zeta.Common;
using Zeta.CommonBot.Dungeons;
using Zeta.CommonBot.Profile;
using Zeta.CommonBot.Logic;
using Zeta.CommonBot.Profile.Common;
using Zeta.Navigation;
using Zeta.TreeSharp;
using Zeta.XmlEngine;
using Zeta.Pathfinding;
using Action = Zeta.TreeSharp.Action;
using System.Collections.Generic;
using GilesTrinity.Technicals;
using Zeta.Internals.Actors;
using System.Drawing;
using Zeta.CommonBot;
using GilesTrinity.DbProvider;

namespace GilesTrinity.XmlTags
{
    [XmlElement("TrinityExploreDungeon")]
    public class TrinityExploreDungeon : ProfileBehavior
    {
        [XmlAttribute("actorId", true)]
        public int ActorId { get; set; }

        [XmlAttribute("boxSize", true)]
        public int BoxSize { get; set; }

        [XmlAttribute("boxTolerance", true)]
        public float BoxTolerance { get; set; }

        [XmlAttribute("exitNameHash", true)]
        public int ExitNameHash { get; set; }

        [XmlAttribute("ignoreGridReset", true)]
        public bool IgnoreGridReset { get; set; }

        [XmlAttribute("leaveWhenFinished", true)]
        public bool LeaveWhenExplored { get; set; }

        [XmlAttribute("objectDistance", true)]
        public float ObjectDistance { get; set; }

        public enum TrinityExploreEndType
        {
            FullyExplored = 0,
            ObjectFound,
            ExitFound,
            SceneFound
        }

        [XmlAttribute("endType", true)]
        [XmlAttribute("until", true)]
        public TrinityExploreEndType EndType { get; set; }

        [XmlAttribute("ignoreScenes")]
        public string IgnoreScenes { get; set; }

        [XmlAttribute("sceneId")]
        public int SceneId { get; set; }

        [XmlAttribute("pathPrecision")]
        public float PathPrecision { get; set; }

        [XmlAttribute("markerDistance")]
        public float MarkerDistance { get; set; }

        public enum TimeoutType
        {
            None,
            Timer,
            GoldInactivity
        }

        [XmlAttribute("timeoutType")]
        public TimeoutType ExploreTimeoutType { get; set; }

        [XmlAttribute("timeoutValue")]
        public int TimeoutValue { get; set; }

        private bool InitDone = false;
        private DungeonNode NextNode;
        /// <summary>
        /// The Position of the CurrentNode NavigableCenter
        /// </summary>
        private Vector3 CurrentNavTarget
        {
            get
            {
                if (GetRouteUnvisitedNodeCount() > 0)
                {
                    return BrainBehavior.DungeonExplorer.CurrentNode.NavigableCenter;
                }
                else
                {
                    return Vector3.Zero;
                }
            }
        }

        /// <summary>
        /// The current player position
        /// </summary>
        private Vector3 myPos { get { return ZetaDia.Me.Position; } }
        private static ISearchAreaProvider gp { get { return GilesTrinity.gp; } }
        private static PathFinder pf { get { return GilesTrinity.pf; } }
        /// <summary>
        /// Contains the current navigation path
        /// </summary>
        private Stack<Vector3> PathStack = new Stack<Vector3>();
        private int mySceneId = -1;
        private Vector3 GPUpdatePosition = Vector3.Zero;
        /// <summary>
        /// Contains known MiniMapMarkers
        /// </summary>

        /// <summary>
        /// Called when the profile behavior starts
        /// </summary>
        public override void OnStart()
        {
            DbHelper.Log(TrinityLogLevel.Normal, LogCategory.XmlTag, "TrinityExploreDungeon OnStart() called");

            CheckResetDungeonExplorer();

            if (!InitDone)
            {
                Init();
            }

            PrintNodeCounts("PostInit");
        }

        /// <summary>
        /// Re-sets the DungeonExplorer, BoxSize, BoxTolerance, and Updates the current route
        /// </summary>
        private void CheckResetDungeonExplorer()
        {
            // I added this because GridSegmentation may (rarely) reset itself without us doing it to 15/.55.
            if ((BoxSize != 0 && BoxTolerance != 0) && (GridSegmentation.BoxSize != BoxSize || GridSegmentation.BoxTolerance != BoxTolerance) || (GetGridSegmentationNodeCount() == 0))
            {
                DbHelper.Log(TrinityLogLevel.Normal, LogCategory.XmlTag, "Box Size or Tolerance has been changed! {0}/{1}", GridSegmentation.BoxSize, GridSegmentation.BoxTolerance);

                BrainBehavior.DungeonExplorer.Reset();
                PrintNodeCounts("BrainBehavior.DungeonExplorer.Reset");

                GridSegmentation.BoxSize = BoxSize;
                GridSegmentation.BoxTolerance = BoxTolerance;
                PrintNodeCounts("SetBoxSize+Tolerance");

                BrainBehavior.DungeonExplorer.Update();
                PrintNodeCounts("BrainBehavior.DungeonExplorer.Update");
            }
        }

        /// <summary>
        /// Adds any visibile 
        /// </summary>

        protected override Composite CreateBehavior()
        {
            return
            new Sequence(
                new Action(ret => PrintNodeCounts("MainBehavior")),
                MiniMapMarker.DetectMiniMapMarkers(),
                UpdateSearchGridProvider(),
                new PrioritySelector(
                    MiniMapMarker.VisitMiniMapMarkers(myPos, MarkerDistance),
                    new Sequence(
                        new DecoratorContinue(ret => !BrainBehavior.DungeonExplorer.CurrentRoute.Any(),
                            new Action(ret => UpdateRoute())
                        ),
                        CheckIsFinished()
                    ),
                    new DecoratorContinue(ret => BrainBehavior.DungeonExplorer.CurrentRoute.Any(),
                        new PrioritySelector(
                            CheckNodeFinished(),
                            new Action(ret => MoveToNextNode())
                        )
                    ),
                    new Action(ret => DbHelper.Log(TrinityLogLevel.Normal, LogCategory.XmlTag, "Error 1: Unknown error occured!"))
                )
            );
        }


        private DecoratorContinue UpdateSearchGridProvider()
        {
            return
            new DecoratorContinue(ret => mySceneId != ZetaDia.Me.SceneId || Vector3.Distance(myPos, GPUpdatePosition) > 150,
                new Sequence(
                    new Action(ret => mySceneId = ZetaDia.Me.SceneId),
                    new Action(ret => GPUpdatePosition = myPos),
                    new Action(ret => GilesTrinity.UpdateSearchGridProvider(true))
                )
            );
        }

        private PrioritySelector CheckIsFinished()
        {
            return
            new PrioritySelector(
                new Decorator(ret => EndType == TrinityExploreEndType.ExitFound && ExitNameHash != 0 && IsExitNameHashVisible(),
                    new Sequence(
                        new Action(ret => DbHelper.Log(TrinityLogLevel.Normal, LogCategory.XmlTag, "Found exitNameHash {0}!", ExitNameHash)),
                        new Action(ret => isDone = true)
                    )
                ),
                new Decorator(ret => EndType == TrinityExploreEndType.ObjectFound && ActorId != 0 && ZetaDia.Actors.GetActorsOfType<DiaObject>(true, false)
                    .Any(a => a.ActorSNO == ActorId && a.Distance <= ObjectDistance),
                    new Sequence(
                        new Action(ret => DbHelper.Log(TrinityLogLevel.Normal, LogCategory.XmlTag, "Found Object {0}!", ActorId)),
                        new Action(ret => isDone = true)
                    )
                ),
                new Decorator(ret => EndType == TrinityExploreEndType.SceneFound && ZetaDia.Me.SceneId == SceneId,
                    new Sequence(
                        new Action(ret => DbHelper.Log(TrinityLogLevel.Normal, LogCategory.XmlTag, "Found SceneId {0}!", SceneId)),
                        new Action(ret => isDone = true)
                    )
                ),
                 new Decorator(ret => GetRouteUnvisitedNodeCount() == 0, // When fully Explored, just finish!
                    new Sequence(
                        new Action(ret => DbHelper.Log(TrinityLogLevel.Normal, LogCategory.XmlTag, "Visited all nodes, TrinityExploreDungeon finished")),
                        new Action(ret => isDone = true)
                    )
                )
           );
        }

        private bool IsExitNameHashVisible()
        {
            return ZetaDia.Minimap.Markers.CurrentWorldMarkers.Any(m => m.NameHash == ExitNameHash && Vector3.Distance(m.Position, myPos) <= MarkerDistance);
        }

        private PrioritySelector CheckNodeFinished()
        {
            return
            new PrioritySelector(
                new Decorator(ret => GetRouteUnvisitedNodeCount() == 0 || !BrainBehavior.DungeonExplorer.CurrentRoute.Any(),
                    new Sequence(
                        new Action(ret => DbHelper.Log(TrinityLogLevel.Normal, LogCategory.XmlTag, "Error - CheckIsNodeFinished() called while Route is empty!")),
                        new Action(ret => UpdateRoute())
                    )
                ),
                new Decorator(ret => CurrentNavTarget.Distance2D(myPos) <= PathPrecision,
                    new Sequence(
                        new Action(ret => SetNodeVisited(String.Format("Node {0} is within PathPrecision ({1:0}/{2:0})", BrainBehavior.DungeonExplorer.CurrentNode.NavigableCenter,
                            BrainBehavior.DungeonExplorer.CurrentNode.NavigableCenter.Distance(ZetaDia.Me.Position), PathPrecision))),
                        new Action(ret => UpdateRoute())
                    )
                ),
                new Decorator(ret => CurrentNavTarget.Distance2D(myPos) <= 90f && !pf.IsNavigable(gp.WorldToGrid(CurrentNavTarget.ToVector2())),
                    new Sequence(
                        new Action(ret => SetNodeVisited("Center Not Navigable")),
                        new Action(ret => UpdateRoute())
                    )
                ),
                new Decorator(ret => GilesTrinity.hashNavigationObstacleCache.Any(o => o.Location.Distance2D(CurrentNavTarget) <= o.Radius * 2),
                    new Sequence(
                        new Action(ret => SetNodeVisited("Navigation obstacle detected at node point")),
                        new Action(ret => UpdateRoute())
                    )
                ),
                new Decorator(ret => PlayerMover.GetMovementSpeed() < 1 && myPos.Distance2D(CurrentNavTarget) <= 50f && ZetaDia.Physics.Raycast(myPos, CurrentNavTarget, Zeta.Internals.SNO.NavCellFlags.AllowWalk),
                    new Sequence(
                        new Action(ret => SetNodeVisited("Stuck moving to node point, marking done (in LoS and nearby!)")),
                        new Action(ret => UpdateRoute())
                    )
                )
            );
        }

        private void UpdateRoute()
        {
            GilesTrinity.UpdateSearchGridProvider();

            CheckResetDungeonExplorer();

            BrainBehavior.DungeonExplorer.Update();
            PrintNodeCounts("BrainBehavior.DungeonExplorer.Update");

            // Throw an exception if this shiz don't work
            ValidateCurrentRoute();
        }

        private void SetNodeVisited(string reason = "")
        {
            DbHelper.Log(TrinityLogLevel.Normal, LogCategory.XmlTag, "Dequeueing current node {0} - {1}", BrainBehavior.DungeonExplorer.CurrentNode.NavigableCenter, reason);
            BrainBehavior.DungeonExplorer.CurrentNode.Visited = true;
            BrainBehavior.DungeonExplorer.CurrentRoute.Dequeue();
            PrintNodeCounts("SetNodeVisited");
        }

        private void Init()
        {
            if (gp == null)
            {
                GilesTrinity.UpdateSearchGridProvider();
            }

            if (BoxSize == 0)
                BoxSize = 15;

            if (BoxTolerance == 0)
                BoxTolerance = 0.55f;

            if (PathPrecision == 0)
                PathPrecision = BoxSize / 2f;

            float minPathPrecision = 10f;

            if (PathPrecision < minPathPrecision)
                PathPrecision = minPathPrecision;

            if (ObjectDistance == 0)
                ObjectDistance = 75f;

            if (MarkerDistance == 0)
                MarkerDistance = 50f;

            GilesTrinity.hashSkipAheadAreaCache = new HashSet<GilesObstacle>();

            DbHelper.Log(TrinityLogLevel.Normal, LogCategory.XmlTag,
                "Initialized TrinityExploreDungeon: boxSize={0} boxTolerance={1:0.00} endType={2} timeoutType={3} timeoutValue={4} pathPrecision={5:0} sceneId={6} actorId={7} objectDistance={8} markerDistance={9}",
                GridSegmentation.BoxSize, GridSegmentation.BoxTolerance, EndType, ExploreTimeoutType, TimeoutValue, PathPrecision, SceneId, ActorId, ObjectDistance, MarkerDistance);

            InitDone = true;
        }

        private static void ValidateCurrentRoute()
        {
            if (BrainBehavior.DungeonExplorer.CurrentRoute == null)
            {
                throw new ApplicationException("DungeonExplorer CurrentRoute is null");
            }
        }

        private void PrintNodeCounts(string step = "")
        {
            if (GilesTrinity.Settings.Advanced.LogCategories.HasFlag(LogCategory.XmlTag))
            {
                string nodeDistance = String.Empty;
                if (GetRouteUnvisitedNodeCount() > 0)
                {
                    try
                    {
                        float distance = BrainBehavior.DungeonExplorer.CurrentNode.NavigableCenter.Distance(myPos);

                        if (distance > 0)
                            nodeDistance = String.Format("Dist:{0:0}", Math.Round(distance / 10f, 2) * 10f);
                    }
                    catch { }
                }

                var log = String.Format("Nodes [Unvisited: Route:{1} Grid:{3} | Grid-Visited: {2}] Box:{4}/{5} Step:{6} {7} Nav:{8} RayCast:{9} PP:{10:0} Dir: {11} ZDiff:{12:0} PathSize:{13}",
                    GetRouteVisistedNodeCount(),                                 // 0
                    GetRouteUnvisitedNodeCount(),                                // 1
                    GetGridSegmentationVisistedNodeCount(),                      // 2
                    GetGridSegmentationUnvisitedNodeCount(),                     // 3
                    GridSegmentation.BoxSize,                                    // 4
                    GridSegmentation.BoxTolerance,                               // 5
                    step,                                                        // 6
                    nodeDistance,                                                // 7
                    pf.IsNavigable(gp.WorldToGrid(CurrentNavTarget.ToVector2())), // 8
                    ZetaDia.Physics.Raycast(myPos, CurrentNavTarget, Zeta.Internals.SNO.NavCellFlags.AllowWalk),
                    PathPrecision,
                    GilesTrinity.GetHeadingToPoint(CurrentNavTarget),
                    Math.Abs(myPos.Z - CurrentNavTarget.Z),
                    PathStack.Count
                    );

                DbHelper.Log(TrinityLogLevel.Normal, LogCategory.XmlTag, log);
            }
        }

        /*
         * Dungeon Explorer Nodes
         */
        private int GetRouteUnvisitedNodeCount()
        {
            if (GetCurrentRouteNodeCount() > 0)
                return BrainBehavior.DungeonExplorer.CurrentRoute.Count(n => !n.Visited);
            else
                return 0;
        }

        private int GetRouteVisistedNodeCount()
        {
            if (GetCurrentRouteNodeCount() > 0)
                return BrainBehavior.DungeonExplorer.CurrentRoute.Count(n => n.Visited);
            else
                return 0;
        }

        private int GetCurrentRouteNodeCount()
        {
            if (BrainBehavior.DungeonExplorer.CurrentRoute != null)
                return BrainBehavior.DungeonExplorer.CurrentRoute.Count();
            else
                return 0;
        }
        /*
         *  Grid Segmentation Nodes
         */
        private int GetGridSegmentationUnvisitedNodeCount()
        {
            if (GetGridSegmentationNodeCount() > 0)
                return GridSegmentation.Nodes.Count(n => !n.Visited);
            else
                return 0;
        }

        private int GetGridSegmentationVisistedNodeCount()
        {
            if (GetCurrentRouteNodeCount() > 0)
                return GridSegmentation.Nodes.Count(n => n.Visited);
            else
                return 0;
        }

        private int GetGridSegmentationNodeCount()
        {
            if (GridSegmentation.Nodes != null)
                return GridSegmentation.Nodes.Count();
            else
                return 0;
        }

        private void MoveToNextNode()
        {
            if (!GilesTrinity.hashSkipAheadAreaCache.Any(p => p.Location.Distance2D(ZetaDia.Me.Position) <= PathPrecision))
            {
                GilesTrinity.hashSkipAheadAreaCache.Add(new GilesObstacle() { Location = myPos, Radius = PathPrecision });
            }


            NextNode = BrainBehavior.DungeonExplorer.CurrentNode;
            Vector3 moveTarget = NextNode.NavigableCenter;

            string nodeName = String.Format("{0} Distance: {1:0} Direction: {2} PathStack: {3}",
                NextNode.NavigableCenter, NextNode.NavigableCenter.Distance(GilesTrinity.PlayerStatus.CurrentPosition), GilesTrinity.GetHeadingToPoint(NextNode.NavigableCenter),
                PathStack.Count);

            if (!PathStack.Any())
            {
                // Generate nodes for the PathStack
                PathStack = PlayerMover.GeneratePath(myPos, NextNode.NavigableCenter);
            }

            if (PathStack.Any())
            {
                moveTarget = PathStack.Peek();
            }

            var playerPosition = ZetaDia.Me.Position;
            var distToTarget = Vector3.Distance2D(ref moveTarget, ref playerPosition);
            if (distToTarget <= PathPrecision || (PlayerMover.GetMovementSpeed() < 1 && distToTarget <= 50f && ZetaDia.Physics.Raycast(moveTarget, myPos, Zeta.Internals.SNO.NavCellFlags.AllowWalk)))
            {
                Vector3 lastStep = PathStack.Peek();
                PathStack.Pop();
                if (PathStack.Any())
                {
                    moveTarget = PathStack.Peek();
                    DbHelper.Log(LogCategory.Moving, "[Path] removed:{0} next:{1} dist:{2} dir:{3}",
                        lastStep, moveTarget, Vector3.Distance(lastStep, moveTarget), GilesTrinity.GetHeadingToPoint(moveTarget));
                }
                else
                {
                    SetNodeVisited("Current movement stack is empty!");
                    UpdateRoute();
                }
            }

            Navigator.PlayerMover.MoveTowards(moveTarget);
        }

        private bool isDone = false;
        public override bool IsDone
        {
            get { return !IsActiveQuestStep || isDone; }
        }

        public override void ResetCachedDone()
        {
            isDone = false;
            InitDone = false;
            BrainBehavior.DungeonExplorer.Reset();
            MiniMapMarker.KnownMarkers.Clear();
        }
    }
}

/*
 * Never need to call GridSegmentation.Update()
 * GridSegmentation.Reset() is automatically called on world change
 * DungeonExplorer.Reset() will reset the current route and revisit nodes
 * DungeonExplorer.Update() will update the current route to include new scenes
 */
