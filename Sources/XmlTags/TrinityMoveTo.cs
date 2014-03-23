using System;
using System.Collections.Generic;
using System.Linq;
using Trinity.Technicals;
using Zeta.Bot.Navigation;
using Zeta.Bot.Profile;
using Zeta.Common;
using Zeta.Game;
using Zeta.TreeSharp;
using Zeta.XmlEngine;
using Action = Zeta.TreeSharp.Action;
using Logger = Trinity.Technicals.Logger;

namespace Trinity.XmlTags
{
    // * TrinityMoveTo moves in a straight line without any navigation hits, and allows tag-skips
    [XmlElement("TrinityMoveTo")]
    public class TrinityMoveTo : ProfileBehavior
    {
        private bool isDone;
        private float posX;
        private float posY;
        private float posZ;
        private float pathPrecision;
        private float unsafeRandomDistance;
        private string noSkip;
        private string useNavigator;
        private Vector3? mainVector;
        private MoveResult lastMoveResult = MoveResult.Moved;

        protected override Composite CreateBehavior()
        {
            return
            new PrioritySelector(
                new Decorator(ret => lastMoveResult == MoveResult.ReachedDestination,
                    new Sequence(
                        new Action(ret => SetTagDone("Reached Destination"))
                    )
                ),
                new Decorator(ret => lastMoveResult != MoveResult.Moved,
                    new Sequence(
                        new Action(ret => SetTagDone("Movement failed"))
                    )
                ),
                new Decorator(ret => IsDistanceWithinPathPrecision,
                    new Action(ret => SetTagDone("Within path precision"))
                ),
                new Action(ret => MoveTo())
            );
        }

        public override void OnStart()
        {
            lastMoveResult = MoveResult.Moved;

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
                if (Trinity.SkipAheadAreaCache.Any())
                {

                    // Loop through all the skip ahead zones and see if one of them is within radius of our intended destination to skip ahead
                    foreach (CacheObstacleObject thisObject in Trinity.SkipAheadAreaCache)
                    {
                        if (thisObject.Position.Distance(Position) <= thisObject.Radius)
                        {
                            Logger.Log(TrinityLogLevel.Verbose, LogCategory.ProfileTag, "Skipping ahead from moveto {0} to next moveto.", Position);
                            Trinity.bSkipAheadAGo = true;
                            return RunStatus.Success;
                        }
                    }
                    Trinity.SkipAheadAreaCache = new HashSet<CacheObstacleObject>();
                }
            }
            else
            {
                Trinity.SkipAheadAreaCache = new HashSet<CacheObstacleObject>();
            }

            // Now use Trinity movement to try a direct movement towards that location
            Vector3 NavTarget = Position;
            Vector3 MyPos = Trinity.Player.Position;
            // DB 300+ always uses local nav! Yay :)
            //if (!ZetaDia.WorldInfo.IsGenerated && Vector3.Distance(MyPos, NavTarget) > 250)
            //{
            //    NavTarget = MathEx.CalculatePointFrom(MyPos, NavTarget, Vector3.Distance(MyPos, NavTarget) - 250);
            //}

            if (useNavigator != null && useNavigator.ToLower() == "false")
            {
                Navigator.PlayerMover.MoveTowards(NavTarget);
                lastMoveResult = MoveResult.Moved;
            }
            else
            {
                var positionName = this.getPosition() + " (" + this.Name + ")";
                lastMoveResult = Navigator.MoveTo(NavTarget, positionName, true);
            }

            return RunStatus.Success;
        }

        private bool IsDistanceWithinPathPrecision
        {
            get
            {
                // First see if we should skip ahead one move because we were already at that location
                if (Trinity.bSkipAheadAGo)
                {
                    Trinity.bSkipAheadAGo = false;
                    return true;
                }

                // Ok not skipping, now see if we are already within pathprecision range of that location
                return (Trinity.Player.Position.Distance(Position) <= Math.Max(PathPrecision, Navigator.PathPrecision));
            }
        }

        private void SetTagDone(string reason = "")
        {
            isDone = true;

            if (reason != string.Empty)
            {
                Logger.LogNormal("[TrinityMoveTo] tag finished: {0} {1}", reason, getPosition());
            }

        }

        public override void ResetCachedDone()
        {
            isDone = false;
            base.ResetCachedDone();
        }

        public override bool IsDone
        {
            get
            {
                if (IsActiveQuestStep)
                {
                    return isDone;
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
                return noSkip;
            }
            set
            {
                noSkip = value;
            }
        }

        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlAttribute("pathPrecision")]
        public float PathPrecision
        {
            get
            {
                return pathPrecision;
            }
            set
            {
                pathPrecision = value;
            }
        }

        public Vector3 Position
        {
            get
            {
                if (!mainVector.HasValue)
                {
                    if (UnsafeRandomDistance == 0f)
                    {
                        mainVector = new Vector3(X, Y, Z);
                    }
                    else
                    {
                        float degrees = new Random().Next(0, 360);
                        mainVector = new Vector3?(MathEx.GetPointAt(new Vector3(X, Y, Z), (float)(new Random().NextDouble() * UnsafeRandomDistance), MathEx.ToRadians(degrees)));
                    }
                }
                return mainVector.Value;
            }
        }

        [XmlAttribute("unsafeRandomDistance")]
        public float UnsafeRandomDistance
        {
            get
            {
                return unsafeRandomDistance;
            }
            set
            {
                unsafeRandomDistance = value;
            }
        }

        [XmlAttribute("x")]
        public float X
        {
            get
            {
                return posX;
            }
            set
            {
                posX = value;
            }
        }

        [XmlAttribute("y")]
        public float Y
        {
            get
            {
                return posY;
            }
            set
            {
                posY = value;
            }
        }

        [XmlAttribute("z")]
        public float Z
        {
            get
            {
                return posZ;
            }
            set
            {
                posZ = value;
            }
        }


    }
}
