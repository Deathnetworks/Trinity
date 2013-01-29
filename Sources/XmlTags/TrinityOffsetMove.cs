using GilesTrinity.DbProvider;
using GilesTrinity.Technicals;
using Zeta;
using Zeta.Common;
using Zeta.CommonBot.Profile;
using Zeta.Navigation;
using Zeta.Pathfinding;
using Zeta.TreeSharp;
using Zeta.XmlEngine;
using Action = Zeta.TreeSharp.Action;

namespace GilesTrinity.XmlTags
{
    /// <summary>
    /// This profile tag will move the player a a direction given by the offsets x, y. Examples:
    ///       <TrinityOffsetMove questId="101758" stepId="1" offsetX="-1000" offsetY="1000" />
    ///       <TrinityOffsetMove questId="101758" stepId="1" offsetX="1000" offsetY="-1000" />
    ///       <TrinityOffsetMove questId="101758" stepId="1" offsetX="-1000" offsetY="-1000" />
    ///       <TrinityOffsetMove questId="101758" stepId="1" offsetX="1000" offsetY="1000" />
    /// </summary>
    [XmlElement("TrinityOffsetMove")]
    public class TrinityOffsetMove : ProfileBehavior
    {
        private bool isDone;
        public override bool IsDone
        {
            get { return !IsActiveQuestStep || isDone; }
        }

        /// <summary>
        /// The distance on the X axis to move
        /// </summary>
        [XmlAttribute("x")]
        [XmlAttribute("offsetX")]
        [XmlAttribute("offsetx")]
        public float OffsetX { get; set; }

        /// <summary>
        /// The distance on the Y axis to move
        /// </summary>
        [XmlAttribute("y")]
        [XmlAttribute("offsetY")]
        [XmlAttribute("offsety")]
        public float OffsetY { get; set; }

        /// <summary>
        /// The distance before we've "reached" the destination
        /// </summary>
        [XmlAttribute("pathPrecision")]
        public float PathPrecision { get; set; }

        public Vector3 Position { get; set; }

        protected override Composite CreateBehavior()
        {
            return
            new PrioritySelector(
                new Decorator(ret => Position.Distance2D(MyPos) <= PathPrecision,
                    new Sequence(
                        new Action(ret => DbHelper.Log(TrinityLogLevel.Normal, LogCategory.UserInformation, "Finished Offset Move x={0} y={1} position={3}", 
                            OffsetX, OffsetY, Position.Distance2D(MyPos), Position)),
                        new Action(ret => isDone = true)
                    )
                ),
                new Action(ret => MoveToPostion())
            );
        }

        private void MoveToPostion()
        {
            DbHelper.Log(TrinityLogLevel.Normal, LogCategory.UserInformation, "Moving to offset x={0} y={1} distance={2:0} position={3}",
                        OffsetX, OffsetY, Position.Distance2D(MyPos), Position);

            MoveResult mr = PlayerMover.NavigateTo(Position);

            if (mr == MoveResult.PathGenerationFailed)
            {
                DbHelper.Log(TrinityLogLevel.Normal, LogCategory.UserInformation, "Error moving to offset x={0} y={1} distance={2:0} position={3}",
                           OffsetX, OffsetY, Position.Distance2D(MyPos), Position);
                isDone = true;
            }
        }


        public Vector3 MyPos { get { return ZetaDia.Me.Position; } }
        private ISearchAreaProvider gp { get { return GilesTrinity.gp; } }
        //private PathFinder pf { get { return GilesTrinity.pf; } }

        public override void OnStart()
        {
            float x = MyPos.X + OffsetX;
            float y = MyPos.Y + OffsetY;

            Position = new Vector3(x, y, gp.GetHeight(new Vector2(x, y)));

            if (PathPrecision == 0)
                PathPrecision = 10f;
            DbHelper.Log(TrinityLogLevel.Normal, LogCategory.ProfileTag, "OffsetMove Initialized offset x={0} y={1} distance={2:0} position={3}",
                       OffsetX, OffsetY, Position.Distance2D(MyPos), Position);

        }
        public override void OnDone()
        {

        }
    }
}
