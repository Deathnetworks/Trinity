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

        private MoveResult lastMoveResult = MoveResult.PathGenerating;
        private bool InitDone = false;
        private DungeonNode NextNode;
        private Vector3 CurrentNavTarget { get { return BrainBehavior.DungeonExplorer.CurrentNode.NavigableCenter; } }
        private Vector3 myPos { get { return ZetaDia.Me.Position; } }
        private static ISearchAreaProvider gp { get { return GilesTrinity.gp; } }
        private static PathFinder pf { get { return GilesTrinity.pf; } }
        private Stack<Vector3> PathStack = new Stack<Vector3>();
        private int mySceneId = -1;
        private Vector3 GPUpdatePosition = Vector3.Zero;
        private List<MiniMapMarker> MiniMapMarkers = new List<MiniMapMarker>();

        private class MiniMapMarker
        {
            public int MarkerNameHash { get; set; }
            public Vector3 Location { get; set; }
            public bool Visited { get; set; }
            public MiniMapMarker() { }
        }

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

        private void CheckResetDungeonExplorer()
        {
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

        private void AddMarkersToList()
        {
            foreach (Zeta.Internals.MinimapMarker marker in ZetaDia.Minimap.Markers.CurrentWorldMarkers)
            {
                MiniMapMarkers.Add(new MiniMapMarker()
                {
                    MarkerNameHash = marker.NameHash,
                    Location = marker.Position,
                    Visited = false
                });
            }
        }

        protected override Composite CreateBehavior()
        {
            MiniMapMarker targetmarker = null;

            return
            new Sequence(
                new DecoratorContinue(ret => ZetaDia.Minimap.Markers.CurrentWorldMarkers.Any(m => m.NameHash == 0 && !MiniMapMarkers.Any(m2 => m2.MarkerNameHash != m.NameHash)),
                    new Sequence(
                        new Action(ret => AddMarkersToList())
                    )
                ),
                new DecoratorContinue(ret => mySceneId != ZetaDia.Me.SceneId || Vector3.Distance(myPos, GPUpdatePosition) > 150,
                    new Sequence(
                        new Action(ret => mySceneId = ZetaDia.Me.SceneId),
                        new Action(ret => GPUpdatePosition = myPos),
                        new Action(ret => GilesTrinity.UpdateSearchGridProvider())
                    )
                ),
                new Action(ret => PrintNodeCounts("MainBehavior")),
                new PrioritySelector(
                    new Decorator(ret => MiniMapMarkers.Any(m => !m.Visited),
                        new Sequence(
                            new WhileLoop(ret => MiniMapMarkers.Any(m => Vector3.Distance(myPos, m.Location) <= PathPrecision),
                                new Action(ret => MiniMapMarkers.FirstOrDefault(m => Vector3.Distance(myPos, m.Location) <= PathPrecision).Visited = true)
                            ),
                            new Action(ret => targetmarker = MiniMapMarkers.OrderBy(m => Vector3.Distance(myPos, m.Location)).FirstOrDefault(m => m.Visited)),
                            new Decorator(ret => targetmarker != null,
                                new Sequence(
                                    new Action(ret => DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.XmlTag, "Moving to inspect nameHash 0 at {0} distance {1:0}",
                                        targetmarker.Location, Vector3.Distance(myPos, targetmarker.Location))),
                                    new Action(ret => Navigator.MoveTo(targetmarker.Location))
                                )
                            )
                        )
                    ),
                    new PrioritySelector(
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
                        )
                    )
                )
            );
        }

        private PrioritySelector CheckIsFinished()
        {
            return
            new PrioritySelector(
                new Decorator(ret => EndType == TrinityExploreEndType.FullyExplored && GetRouteUnvisitedNodeCount() == 0, // When fully Explored, just finish!
                    new Sequence(
                        new Action(ret => DbHelper.Log(TrinityLogLevel.Normal, LogCategory.XmlTag, "Visited all nodes, TrinityExploreDungeon finished")),
                        new Action(ret => isDone = true)
                    )
                ),
                new Decorator(ret => EndType == TrinityExploreEndType.ExitFound && ExitNameHash != 0 && ZetaDia.Minimap.Markers.CurrentWorldMarkers.Any(m => m.NameHash == ExitNameHash),
                    new Sequence(
                        new Action(ret => DbHelper.Log(TrinityLogLevel.Normal, LogCategory.XmlTag, "Found exitNameHash {0}!", ExitNameHash)),
                        new Action(ret => isDone = true)
                    )
                ),
                new Decorator(ret => EndType == TrinityExploreEndType.ObjectFound && ActorId != 0 && ZetaDia.Actors.GetActorsOfType<DiaObject>()
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
                )
            );
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
                new Decorator(ret => CurrentNavTarget.Distance(ZetaDia.Me.Position) <= PathPrecision,
                    new Sequence(
                        new Action(ret => SetNodeVisited(String.Format("Node {0} is within PathPrecision ({1:0}/{2:0})", BrainBehavior.DungeonExplorer.CurrentNode.NavigableCenter,
                            BrainBehavior.DungeonExplorer.CurrentNode.NavigableCenter.Distance(ZetaDia.Me.Position), PathPrecision))),
                        new Action(ret => UpdateRoute())
                    )
                ),
                new Decorator(ret => CurrentNavTarget.Distance(ZetaDia.Me.Position) <= 90f,
                    new Decorator(ret => !pf.IsNavigable(gp.WorldToGrid(CurrentNavTarget.ToVector2())),
                        new Sequence(
                            new Action(ret => SetNodeVisited("Center Not Navigable")),
                            new Action(ret => UpdateRoute())
                        )
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

            lastMoveResult = MoveResult.PathGenerated;
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

            if (PathPrecision < 40f)
                PathPrecision = 40f;

            if (ObjectDistance == 0)
                ObjectDistance = 50f;

            GilesTrinity.hashSkipAheadAreaCache = new HashSet<GilesObstacle>();

            DbHelper.Log(TrinityLogLevel.Normal, LogCategory.XmlTag,
                "Initialized TrinityExploreDungeon: boxSize={0} boxTolerance={1:0.00} endType={2} timeoutType={3} timeoutValue={4} pathPrecision={5:0} sceneId={6}",
                GridSegmentation.BoxSize, GridSegmentation.BoxTolerance, EndType, ExploreTimeoutType, TimeoutValue, PathPrecision, SceneId);

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
            string nodeDistance = String.Empty;
            if (GetRouteUnvisitedNodeCount() > 0)
            {
                try
                {
                    float distance = BrainBehavior.DungeonExplorer.CurrentNode.NavigableCenter.Distance(myPos);

                    if (distance > 0)
                        nodeDistance = String.Format(" Distance: {0:0} ", Math.Round(distance / 10f, 2) * 10f);
                }
                catch { }
            }


            DbHelper.Log(TrinityLogLevel.Normal, LogCategory.XmlTag, "Nodes [ Route-Unvisited: {1} Grid-Visited: {2} Grid-Unvisited: {3} ] Box: {4}/{5} Step: {6} {7} ",
                GetRouteVisistedNodeCount(), GetRouteUnvisitedNodeCount(), GetGridSegmentationVisistedNodeCount(), GetGridSegmentationUnvisitedNodeCount(),
                GridSegmentation.BoxSize, GridSegmentation.BoxTolerance, step, nodeDistance);
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
            if (!GilesTrinity.hashSkipAheadAreaCache.Any(p => Vector3.Distance(p.Location, ZetaDia.Me.Position) <= PathPrecision))
            {
                GilesTrinity.hashSkipAheadAreaCache.Add(new GilesObstacle() { Location = myPos, Radius = PathPrecision });
            }


            NextNode = BrainBehavior.DungeonExplorer.CurrentRoute.Peek();
            Vector3 moveTarget = NextNode.NavigableCenter;

            string nodeName = String.Format("{0} Distance: {1:0} Direction: {2}",
                NextNode.NavigableCenter, NextNode.NavigableCenter.Distance(GilesTrinity.PlayerStatus.CurrentPosition), GilesTrinity.GetHeadingToPoint(NextNode.NavigableCenter));

            //lastMoveResult = Navigator.MoveTo(moveTarget, nodeName, true);

            if (!PathStack.Any())
            {
                // Generate nodes for the PathStack
                GeneratePathTo(NextNode.NavigableCenter);
            }

            if (PathStack.Any())
            {
                moveTarget = PathStack.Peek();
            }

            if (Vector3.Distance(moveTarget, GilesTrinity.PlayerStatus.CurrentPosition) <= PathPrecision || lastMoveResult == MoveResult.ReachedDestination)
            {
                PathStack.Pop();
                if (PathStack.Any())
                    moveTarget = PathStack.Peek();
                else
                {
                    SetNodeVisited("Current movement stack is empty!");
                    UpdateRoute();
                }
            }


            lastMoveResult = Navigator.MoveTo(moveTarget, nodeName, true);

            //switch (lastMoveResult)
            //{

            //    case MoveResult.ReachedDestination:
            //        break;
            //    case MoveResult.Moved:
            //        //Logging.Write("Movement return is successful.");
            //        break;
            //    case MoveResult.PathGenerated:
            //    case MoveResult.PathGenerating:
            //        DbHelper.Log(TrinityLogLevel.Normal, LogCategory.XmlTag, "Movement pending", true);
            //        break;
            //    case MoveResult.Failed:
            //    case MoveResult.PathGenerationFailed:
            //        {
            //            GeneratePathTo(NextNode.NavigableCenter);
            //            break;
            //        }
            //}

        }

        private void GeneratePathTo(Vector3 destination)
        {
            GilesTrinity.UpdateSearchGridProvider();

            PathFindResult pfr = pf.FindPath(
                gp.WorldToGrid(myPos.ToVector2()),
                gp.WorldToGrid(destination.ToVector2()),
                true, 50, true
                );

            DbHelper.Log(TrinityLogLevel.Normal, LogCategory.XmlTag, "Generated path with {0} points", pfr.PointsReversed.Count());

            if (pfr.Error)
            {
                DbHelper.Log(TrinityLogLevel.Normal, LogCategory.XmlTag, "Error in generating path: {0}", pfr.ErrorMessage);
                return;
            }

            if (pfr.IsPartialPath)
            {
                DbHelper.Log(TrinityLogLevel.Normal, LogCategory.XmlTag, "Partial Path Generated!", true);
            }

            PathStack.Clear();


            foreach (Point p in pfr.PointsReversed)
            {
                Vector3 v3 = gp.GridToWorld(p).ToVector3();
                PathStack.Push(v3);
                //Logging.Write("Pushing path point to stack {0}, order {1}, distance {2}", v3, PathStack.Count(), v3.Distance(myPos));
            }
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
            MiniMapMarkers = new List<MiniMapMarker>();
        }
    }
}

/*
 * Never need to call GridSegmentation.Update()
 * GridSegmentation.Reset() is automatically called on world change
 * DungeonExplorer.Reset() will reset the current route and revisit nodes
 * DungeonExplorer.Update() will update the current route to include new scenes
 */
