using GilesTrinity.DbProvider;
using Zeta;
using Zeta.Common;
using Zeta.CommonBot.Profile;
using Zeta.Pathfinding;
using Zeta.TreeSharp;
using Zeta.XmlEngine;
using Action = Zeta.TreeSharp.Action;

namespace GilesTrinity.XmlTags
{
    /// <summary>
    /// This profile tag will move the player a a direction given by the offsets x, y
    /// </summary>
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
        [XmlAttribute("offsetX")]
        public float OffsetX { get; set; }

        /// <summary>
        /// The distance on the Y axis to move
        /// </summary>
        [XmlAttribute("offsetY")]
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
                    new Action(ret => isDone = true)
                ),
                new Action(ret => PlayerMover.NavigateTo(Position))
            );
        }

        public Vector3 MyPos { get { return ZetaDia.Me.Position; } }
        private ISearchAreaProvider gp { get { return GilesTrinity.gp; } }
        private PathFinder pf { get { return GilesTrinity.pf; } }

        public override void OnStart()
        {
            float x = MyPos.X + OffsetX;
            float y = MyPos.Y + OffsetY;

            Position = new Vector3(x, y, gp.GetHeight(new Vector2(x, y)));

            if (PathPrecision == 0)
                PathPrecision = 10f;
        }
        public override void OnDone()
        {

        }
    }
}
