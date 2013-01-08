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
    public class TrinityExploreDungeon : ExploreAreaTag
    {
        private bool isDone = false;
        public override bool IsDone
        {
            get { return !IsActiveQuestStep || isDone; }
        }

        public override void ResetCachedDone()
        {
            isDone = false;
            InitDone = false;
            base.ResetCachedDone();
        }

        public enum TrinityExploreEndType
        {
            FullyExplored = 0,
            ObjectFound,
            ExitFound,
            SceneFound
        }

        [XmlAttribute("endType", true)]
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

        private void Init()
        {
            if (gp == null)
            {
                GilesTrinity.UpdateSearchGridProvider();
            }

            BrainBehavior.DungeonExplorer.Update();

            if (BrainBehavior.DungeonExplorer.CurrentRoute == null)
            {
                throw new ApplicationException("DungeonExplorer CurrentRoute is null");
            }

            GetNodeCounts();

            if (PathPrecision == 0)
                PathPrecision = 20f;

            GilesTrinity.hashSkipAheadAreaCache = new HashSet<GilesObstacle>();

            DbHelper.Log(TrinityLogLevel.Normal, LogCategory.XmlTag,
                "Initialized TrinityExploreDungeon: boxSize={0} boxTolerance={1:0.00} endType={2} timeoutType={3} timeoutValue={4} pathPrecision={5:0} sceneId={6}",
                GridSegmentation.BoxSize, GridSegmentation.BoxTolerance, EndType, ExploreTimeoutType, TimeoutValue, PathPrecision, SceneId);

            InitDone = true;
        }

        private void GetNodeCounts()
        {
            DbHelper.Log(TrinityLogLevel.Normal, LogCategory.XmlTag, "GridSegmentation Stats: Visisted nodes: {0} Unvisited nodes: {1} BoxSize: {2} BoxTolerance: {3}",
                BrainBehavior.DungeonExplorer.CurrentRoute.Count(n => n.Visited), BrainBehavior.DungeonExplorer.CurrentRoute.Count(n => !n.Visited), 
                GridSegmentation.BoxSize, GridSegmentation.BoxTolerance);
        }

        private PrioritySelector CheckIsFinished()
        {
            return new PrioritySelector(
                new Decorator(ret => EndType == TrinityExploreEndType.FullyExplored && (GridSegmentation.Nodes.Count(n => !n.Visited) == 0 && GridSegmentation.Nodes.Count(n => n.Visited) > 0),
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

        private Vector3 CurrentNavTarget { get { return BrainBehavior.DungeonExplorer.CurrentNode.NavigableCenter; } }

        private int GetCurrentRouteNodeCount()
        {
            if (BrainBehavior.DungeonExplorer.CurrentRoute != null)
                return BrainBehavior.DungeonExplorer.CurrentRoute.Count();
            else
                return 0;
        }

        protected override Composite CreateBehavior()
        {
            return
            new Sequence(
                new DecoratorContinue(ret => !InitDone,
                    new Action(ret => Init())
                ),
                new DecoratorContinue(ret => !BrainBehavior.DungeonExplorer.CurrentRoute.Any() || BrainBehavior.DungeonExplorer.CurrentNode == null,
                    new Action(ret => BrainBehavior.DungeonExplorer.Update())
                ),
                new Action(ret => GetNodeCounts()),
                new PrioritySelector(
                    CheckIsFinished(),
                    new Decorator(ret => BrainBehavior.DungeonExplorer.CurrentNode != null,
                        new Sequence(
                            new Decorator(ret => BrainBehavior.DungeonExplorer.CurrentNode.NavigableCenter.Distance(GilesTrinity.PlayerStatus.CurrentPosition) <= PathPrecision || lastMoveResult == MoveResult.ReachedDestination,
                                new Sequence(
                                    new Action(ret => SetNodeVisited()),
                                    new Action(ret => UpdateRoute())
                                )
                            ),
                            new Action(ret => MoveToNextNode())
                        )
                    )
                )
            );
        }

        private void UpdateRoute()
        {
            BrainBehavior.DungeonExplorer.Update();
            lastMoveResult = MoveResult.PathGenerated;
        }

        private void SetNodeVisited()
        {
            BrainBehavior.DungeonExplorer.CurrentNode.Visited = true;
            BrainBehavior.DungeonExplorer.CurrentRoute.Dequeue();
        }

        private void MoveToNextNode()
        {
            NextNode = BrainBehavior.DungeonExplorer.CurrentRoute.Peek();

            string nodeName = String.Format("{0} Distance: {1:0} Direction: {2}",
                NextNode.NavigableCenter, NextNode.NavigableCenter.Distance(GilesTrinity.PlayerStatus.CurrentPosition), GilesTrinity.GetHeadingToPoint(NextNode.NavigableCenter));

            Vector3 moveTarget = NextNode.NavigableCenter;

            if (PathStack.Any())
            {
                DbHelper.Log(TrinityLogLevel.Normal, LogCategory.XmlTag, "Using alternative path", true);
                moveTarget = PathStack.Peek();
            }
            if (PathStack.Any() && Vector3.Distance(moveTarget, GilesTrinity.PlayerStatus.CurrentPosition) <= PathPrecision)
            {
                PathStack.Pop();
                moveTarget = PathStack.Peek();
            }

            if (!GilesTrinity.hashSkipAheadAreaCache.Any(p => Vector3.Distance(p.Location, ZetaDia.Me.Position) <= PathPrecision))
            {
                GilesTrinity.hashSkipAheadAreaCache.Add(new GilesObstacle() { Location = ZetaDia.Me.Position, Radius = PathPrecision });
            }

            lastMoveResult = Navigator.MoveTo(moveTarget, nodeName, true);

            switch (lastMoveResult)
            {

                case MoveResult.ReachedDestination:
                    break;
                case MoveResult.Moved:
                    //Logging.Write("Movement return is successful.");
                    break;
                case MoveResult.PathGenerated:
                case MoveResult.PathGenerating:
                    DbHelper.Log(TrinityLogLevel.Normal, LogCategory.XmlTag, "Movement pending", true);
                    break;
                case MoveResult.Failed:
                case MoveResult.PathGenerationFailed:
                    {
                        GeneratePathTo(NextNode.NavigableCenter);
                        break;
                    }
            }

        }

        private static ISearchAreaProvider gp { get { return GilesTrinity.gp; } }
        private static PathFinder pf { get { return GilesTrinity.pf; } }

        private Stack<Vector3> PathStack = new Stack<Vector3>();

        private void GeneratePathTo(Vector3 destination)
        {
            GilesTrinity.UpdateSearchGridProvider();

            PathFindResult pfr = pf.FindPath(
                gp.WorldToGrid(GilesTrinity.PlayerStatus.CurrentPosition.ToVector2()),
                gp.WorldToGrid(destination.ToVector2()),
                true, 50, true
                );

            DbHelper.Log(TrinityLogLevel.Normal, LogCategory.XmlTag, "Movement Failure, generating path manually with {0} points", pfr.PointsReversed.Count());

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
                Logging.Write("Pushing path point to stack {0}, order {1}, distance {2}", v3, PathStack.Count(), v3.Distance(myPos));
            }
        }

        private Vector3 myPos { get { return GilesTrinity.PlayerStatus.CurrentPosition; } }
    }
}