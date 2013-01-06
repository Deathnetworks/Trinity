using GilesTrinity.Technicals;
using System;
using System.Collections.Generic;
using System.Linq;
using Zeta;
using Zeta.Common;
using Zeta.CommonBot.Profile;
using Zeta.Navigation;
using Zeta.TreeSharp;
using Zeta.XmlEngine;

namespace GilesTrinity.XmlTags
{
    // * TrinityMoveTo moves in a straight line without any navigation hits, and allows tag-skips
    [XmlElement("TrinityMoveTo")]
    public class TrinityMoveTo : ProfileBehavior
    {
        private bool m_IsDone;
        private float fPosX;
        private float fPosY;
        private float fPosZ;
        private float fPathPrecision;
        private float fRandomizedDistance;
        private string sDestinationName;
        private string sNoSkip;
        private string UseNavigator;
        private Vector3? vMainVector;

        protected override Composite CreateBehavior()
        {
            Composite[] children = new Composite[2];
            Composite[] compositeArray = new Composite[2];
            compositeArray[0] = new Zeta.TreeSharp.Action(new ActionSucceedDelegate(FlagTagAsCompleted));
            children[0] = new Zeta.TreeSharp.Decorator(new CanRunDecoratorDelegate(CheckDistanceWithinPathPrecision), new Sequence(compositeArray));
            ActionDelegate actionDelegateMove = new ActionDelegate(GilesMoveToLocation);
            Sequence sequenceblank = new Sequence(
                new Zeta.TreeSharp.Action(actionDelegateMove)
                );
            children[1] = sequenceblank;
            return new PrioritySelector(children);
        }

        private RunStatus GilesMoveToLocation(object ret)
        {

            // First check if we can skip ahead because we recently moved here
            if (!GilesTrinity.Settings.Combat.Misc.AllowBacktracking && (NoSkip == null || NoSkip.ToLower() != "true"))
            {
                if (GilesTrinity.hashSkipAheadAreaCache.Any())
                {

                    // Loop through all the skip ahead zones and see if one of them is within radius of our intended destination to skip ahead
                    foreach (GilesObstacle thisObject in GilesTrinity.hashSkipAheadAreaCache)
                    {
                        if (thisObject.Location.Distance(Position) <= thisObject.Radius)
                        {
                            DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.XmlTag, "Skipping ahead from moveto {0} to next moveto.", Position);
                            GilesTrinity.bSkipAheadAGo = true;
                            return RunStatus.Success;
                        }
                    }
                    GilesTrinity.hashSkipAheadAreaCache = new HashSet<GilesObstacle>();
                }
            }
            else
            {
                GilesTrinity.hashSkipAheadAreaCache = new HashSet<GilesObstacle>();
            }

            // Now use Trinity movement to try a direct movement towards that location

            Vector3 NavTarget = Position;
            Vector3 MyPos = GilesTrinity.PlayerStatus.CurrentPosition;
            if (Vector3.Distance(MyPos, NavTarget) > 250)
            {
                NavTarget = MathEx.CalculatePointFrom(MyPos, NavTarget, Vector3.Distance(MyPos, NavTarget) - 250);
            }

            if (UseNavigator.ToLower() != "true")
                Navigator.PlayerMover.MoveTowards(NavTarget);
            else
                Navigator.MoveTo(NavTarget);

            return RunStatus.Success;
        }

        private bool CheckDistanceWithinPathPrecision(object object_0)
        {

            // First see if we should skip ahead one move because we were already at that location
            if (GilesTrinity.bSkipAheadAGo)
            {
                GilesTrinity.bSkipAheadAGo = false;
                return true;
            }

            // Ok not skipping, now see if we are already within pathprecision range of that location
            return (ZetaDia.Me.Position.Distance(Position) <= Math.Max(PathPrecision, Navigator.PathPrecision));
        }

        private void FlagTagAsCompleted(object object_0)
        {
            m_IsDone = true;
        }

        public override void ResetCachedDone()
        {
            m_IsDone = false;
            base.ResetCachedDone();
        }

        public override bool IsDone
        {
            get
            {
                if (IsActiveQuestStep)
                {
                    return m_IsDone;
                }
                return true;
            }
        }

        [XmlAttribute("navigation")]
        public string Navigation
        {
            get
            {
                return UseNavigator;
            }
            set
            {
                UseNavigator = value;
            }
        }

        [XmlAttribute("noskip")]
        public string NoSkip
        {
            get
            {
                return sNoSkip;
            }
            set
            {
                sNoSkip = value;
            }
        }

        [XmlAttribute("name")]
        public string Name
        {
            get
            {
                return sDestinationName;
            }
            set
            {
                sDestinationName = value;
            }
        }

        [XmlAttribute("pathPrecision")]
        public float PathPrecision
        {
            get
            {
                return fPathPrecision;
            }
            set
            {
                fPathPrecision = value;
            }
        }

        public Vector3 Position
        {
            get
            {
                if (!vMainVector.HasValue)
                {
                    if (UnsafeRandomDistance == 0f)
                    {
                        vMainVector = new Vector3(X, Y, Z);
                    }
                    else
                    {
                        float degrees = new Random().Next(0, 360);
                        vMainVector = new Vector3?(MathEx.GetPointAt(new Vector3(X, Y, Z), (float)(new Random().NextDouble() * UnsafeRandomDistance), MathEx.ToRadians(degrees)));
                    }
                }
                return vMainVector.Value;
            }
        }

        [XmlAttribute("unsafeRandomDistance")]
        public float UnsafeRandomDistance
        {
            get
            {
                return fRandomizedDistance;
            }
            set
            {
                fRandomizedDistance = value;
            }
        }

        [XmlAttribute("x")]
        public float X
        {
            get
            {
                return fPosX;
            }
            set
            {
                fPosX = value;
            }
        }

        [XmlAttribute("y")]
        public float Y
        {
            get
            {
                return fPosY;
            }
            set
            {
                fPosY = value;
            }
        }

        [XmlAttribute("z")]
        public float Z
        {
            get
            {
                return fPosZ;
            }
            set
            {
                fPosZ = value;
            }
        }


    }
}
