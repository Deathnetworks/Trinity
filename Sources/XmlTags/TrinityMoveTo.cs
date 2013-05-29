using System;
using System.Collections.Generic;
using System.Linq;
using Trinity.Technicals;
using Zeta;
using Zeta.Common;
using Zeta.CommonBot.Profile;
using Zeta.Navigation;
using Zeta.TreeSharp;
using Zeta.XmlEngine;
using Action = Zeta.TreeSharp.Action;

namespace Trinity.XmlTags
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
        private string sNoSkip;
        private string useNavigator;
        private Vector3? vMainVector;

        protected override Composite CreateBehavior()
        {
            return
            new PrioritySelector(
                new Decorator(ret => CheckDistanceWithinPathPrecision(),
                    new Action(ret => FlagTagAsCompleted())
                ),
                new Action(ret => MoveTo())
            );
        }

        public override void OnStart()
        {
            Logger.Log(LogCategory.UserInformation, "[TrinityMoveTo] Started Tag; {0} name=\"{1}\" questId=\"{2}\" stepId=\"{3}\" worldId=\"{4}\" levelAreaId=\"{5}\"",
                getPosition(), this.Name, this.QuestId, this.StepId, ZetaDia.CurrentWorldId, ZetaDia.CurrentLevelAreaId);
        }

        private string getPosition()
        {
            return String.Format("x=\"{0}\" y=\"{1}\" z=\"{2}\" ", this.X, this.Y, this.Z);
        }

        private RunStatus MoveTo()
        {

            // First check if we can skip ahead because we recently moved here
            if (!Trinity.Settings.Combat.Misc.AllowBacktracking && (NoSkip == null || NoSkip.ToLower() != "true"))
            {
                if (Trinity.hashSkipAheadAreaCache.Any())
                {

                    // Loop through all the skip ahead zones and see if one of them is within radius of our intended destination to skip ahead
                    foreach (CacheObstacleObject thisObject in Trinity.hashSkipAheadAreaCache)
                    {
                        if (thisObject.Location.Distance(Position) <= thisObject.Radius)
                        {
                            Logger.Log(TrinityLogLevel.Verbose, LogCategory.ProfileTag, "Skipping ahead from moveto {0} to next moveto.", Position);
                            Trinity.bSkipAheadAGo = true;
                            return RunStatus.Success;
                        }
                    }
                    Trinity.hashSkipAheadAreaCache = new HashSet<CacheObstacleObject>();
                }
            }
            else
            {
                Trinity.hashSkipAheadAreaCache = new HashSet<CacheObstacleObject>();
            }

            // Now use Trinity movement to try a direct movement towards that location
            Vector3 NavTarget = Position;
            Vector3 MyPos = Trinity.Player.CurrentPosition;
            if (!ZetaDia.WorldInfo.IsGenerated && Vector3.Distance(MyPos, NavTarget) > 250)
            {
                NavTarget = MathEx.CalculatePointFrom(MyPos, NavTarget, Vector3.Distance(MyPos, NavTarget) - 250);
            }

            if (useNavigator != null && useNavigator.ToLower() == "false")
            {
                Navigator.PlayerMover.MoveTowards(NavTarget);
            }
            else
            {
                var positionName = this.getPosition() + " (" + this.Name + ")";
                Navigator.MoveTo(NavTarget, positionName, true);
            }

            return RunStatus.Success;
        }

        private bool CheckDistanceWithinPathPrecision()
        {

            // First see if we should skip ahead one move because we were already at that location
            if (Trinity.bSkipAheadAGo)
            {
                Trinity.bSkipAheadAGo = false;
                return true;
            }

            // Ok not skipping, now see if we are already within pathprecision range of that location
            return (Trinity.Player.CurrentPosition.Distance(Position) <= Math.Max(PathPrecision, Navigator.PathPrecision));
        }

        private void FlagTagAsCompleted()
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
        public string UseNavigator
        {
            get
            {
                return useNavigator;
            }
            set
            {
                useNavigator = value;
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
        public string Name { get; set; }

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
