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

        public enum TrinityExploreEndType
        {
            ObjectFound,
            FullyExplored,
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

        private DungeonExplorer DE;
        private MoveResult lastMoveResult = MoveResult.PathGenerating;
        private bool InitDone = false;
        private DungeonNode NextNode;
        private DateTime lastUpdatedPOS = DateTime.MinValue;

        private class ExploreBox
        {
            public float Min_X { get; set; }
            public float Max_X { get; set; }
            public float Min_Y { get; set; }
            public float Max_Y { get; set; }
            public Vector3 NavigableCenter { get; set; }
            public float BoxSize { get; set; }

            public ExploreBox(Vector3 center, float boxSize)
            {
                this.NavigableCenter = center;
                this.BoxSize = boxSize;
                float halfBoxSize = (boxSize / 2);
                Min_X = center.X - halfBoxSize;
                Max_X = center.X + halfBoxSize;
                Min_Y = center.Y - halfBoxSize;
                Max_Y = center.Y + halfBoxSize;
            }

            public bool PointIsInBox(Vector3 position)
            {
                if (position.X > Min_X &&
                    position.X < Max_X &&
                    position.Y > Min_Y &&
                    position.Y < Max_Y)
                    return true;
                else
                    return false;
            }
        }

        private HashSet<ExploreBox> VisistedBoxes = new HashSet<ExploreBox>();

        private bool HasVisistedNode(Vector3 center)
        {
            return VisistedBoxes.Any(b => b.PointIsInBox(center));
        }

        private void Init()
        {
            DbHelper.Log(TrinityLogLevel.Normal, LogCategory.Moving,
                "Initializing TrinityExploreDungeon: boxSize={0} boxTolerance={1:0.00} endType={2} timeoutType={3} timeoutValue={4} pathPrecision={5:0} sceneId={6}",
                BoxSize, BoxTolerance, EndType, ExploreTimeoutType, TimeoutValue, PathPrecision, SceneId);

            GridSegmentation.BoxSize = (BoxSize > 0 ? BoxSize : 18);
            GridSegmentation.BoxTolerance = (BoxTolerance > 0 && BoxTolerance <= 1 ? BoxTolerance : 0.18f);
            GridSegmentation.Update();
            BrainBehavior.DungeonExplorer.Reset();
            BrainBehavior.DungeonExplorer.Update();

            DE = BrainBehavior.DungeonExplorer;

            GetNodeCounts();

            if (PathPrecision == 0)
                PathPrecision = 20f;

            GilesTrinity.hashSkipAheadAreaCache = new HashSet<GilesObstacle>();

            InitDone = true;
        }

        private void GetNodeCounts()
        {
            Logging.Write("DungeonExplorer CurrentRoute node count: " + DE.CurrentRoute.Count());
            Logging.Write("DungeonExplorer BestRoute node count: " + DE.GetBestRoute().Count());
        }

        private PrioritySelector CheckIsFinished()
        {
            return new PrioritySelector(
                new Decorator(ret => EndType == TrinityExploreEndType.FullyExplored && DE.CurrentRoute.Count == 0,
                    new Sequence(
                        new Action(ret => DbHelper.Log(TrinityLogLevel.Normal, LogCategory.Moving, "Current DungeonExplorer route is empty, ExploreArea finished")),
                        new Action(ret => isDone = true)
                    )
                ),
                new Decorator(ret => EndType == TrinityExploreEndType.ExitFound && ExitNameHash != 0 && ZetaDia.Minimap.Markers.CurrentWorldMarkers.Any(m => m.NameHash == ExitNameHash),
                    new Sequence(
                        new Action(ret => DbHelper.Log(TrinityLogLevel.Normal, LogCategory.Moving, "Found exitNameHash {0}!", ExitNameHash)),
                        new Action(ret => isDone = true)
                    )
                ),
                new Decorator(ret => EndType == TrinityExploreEndType.ObjectFound && ActorId != 0 && ZetaDia.Actors.GetActorsOfType<DiaObject>()
                    .Any(a => a.ActorSNO == ActorId && a.Distance <= ObjectDistance),
                    new Sequence(
                        new Action(ret => DbHelper.Log(TrinityLogLevel.Normal, LogCategory.Moving, "Found Object {0}!", ActorId)),
                        new Action(ret => isDone = true)
                    )
                ),
                new Decorator(ret => EndType == TrinityExploreEndType.SceneFound && ZetaDia.Me.SceneId == SceneId,
                    new Sequence(
                        new Action(ret => DbHelper.Log(TrinityLogLevel.Normal, LogCategory.Moving, "Found SceneId {0}!", SceneId)),
                        new Action(ret => isDone = true)
                    )
                )
            );
        }

        private Vector3 CurrentNavTarget { get { return DE.CurrentNode.NavigableCenter; } }

        protected override Composite CreateBehavior()
        {
            return

            new Sequence(
                new DecoratorContinue(ret => !InitDone,
                    new Action(ret => Init())
                ),
                new PrioritySelector(
                    //new Decorator(ret => DE.CurrentRoute.Count == 0,
                    //    //new Action(ret => DE.Update())
                    //    new Sequence(
                    //        new Action(ret => DbHelper.Log(TrinityLogLevel.Normal, LogCategory.Moving, "Current DungeonExplorer route is empty, ExploreArea finished")),
                    //        new Action(ret => isDone = true)
                    //    )
                    //),
                    CheckIsFinished(),
                    new Decorator(ret => lastMoveResult != MoveResult.Moved && (lastMoveResult == MoveResult.Failed || lastMoveResult == MoveResult.PathGenerationFailed),
                        new Sequence(
                            new Action(ret => UpdateRoute()),
                            new Action(ret => VisistedBoxes.Clear()),
                            new Action(ret => GilesTrinity.hashSkipAheadAreaCache.Clear())
                        )
                    ),
                    new Decorator(ret => DE.CurrentNode.NavigableCenter.Distance(GilesTrinity.PlayerStatus.CurrentPosition) <= PathPrecision || lastMoveResult == MoveResult.ReachedDestination,
                        new Sequence(
                            new Action(ret => GilesTrinity.hashSkipAheadAreaCache.Add(new GilesObstacle() { Location = CurrentNavTarget, Radius = PathPrecision })),
                            new Action(ret => VisistedBoxes.Add(new ExploreBox(CurrentNavTarget, BoxSize))),
                            new Action(ret => DE.CurrentRoute.Dequeue())
                        )
                    ),
                //new Decorator(ret => HasVisistedNode(CurrentNavTarget),
                //    new Sequence(
                //        new Action(ret => DbHelper.Log(TrinityLogLevel.Normal, LogCategory.Moving, "Dequeing previously visited node {0}", CurrentNavTarget)),
                //        new Action(ret => DE.CurrentRoute.Dequeue())
                //    )
                //),
                //new Decorator(ret => GilesTrinity.hashSkipAheadAreaCache.Any(p => Vector3.Distance(p.Location, CurrentNavTarget) <= BoxSize),
                //    new Sequence(
                //        new Action(ret => DbHelper.Log(TrinityLogLevel.Normal, LogCategory.Moving, "Dequeing previously visited area {0}", CurrentNavTarget)),
                //        new Action(ret => DE.CurrentRoute.Dequeue())
                //    )
                //),
                    new Action(ret => MoveToNextNode())
                )
            );
        }

        private void UpdateRoute()
        {
            DE.Update();
            lastMoveResult = MoveResult.PathGenerated;
        }

        private void MoveToNextNode()
        {
            NextNode = DE.CurrentRoute.Peek();

            lastMoveResult = Navigator.MoveTo(NextNode.NavigableCenter);

            switch (lastMoveResult)
            {

                case MoveResult.ReachedDestination:
                    break;
                case MoveResult.Moved:
                    //Logging.Write("Movement return is successful.");
                    break;
                case MoveResult.PathGenerated:
                case MoveResult.PathGenerating:
                    DbHelper.Log(TrinityLogLevel.Normal, LogCategory.Moving, "Movement pending", true);
                    break;
                case MoveResult.Failed:
                case MoveResult.PathGenerationFailed:
                    DbHelper.Log(TrinityLogLevel.Normal, LogCategory.Moving, "Movement Failure", true);
                    break;
            }

        }
    }
}