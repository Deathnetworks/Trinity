using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Ink;
using Buddy.Coroutines;
using Org.BouncyCastle.Bcpg;
using Trinity.DbProvider;
using Trinity.Technicals;
using Zeta.Bot;
using Zeta.Bot.Navigation;
using Zeta.Bot.Profile;
using Zeta.Common;
using Zeta.Game;
using Zeta.Game.Internals;
using Zeta.Game.Internals.SNO;
using Zeta.TreeSharp;
using Action = Zeta.TreeSharp.Action;
using Logger = Trinity.Technicals.Logger;

namespace Trinity
{
    /// <summary>
    /// Blank Stuck Handler - to disable DB stuck handler
    /// </summary>
    public class StuckHandler : IStuckHandler
    {
        public bool IsStuck 
        { 
            get 
            {
                if (PlayerMover.UnstuckChecker())
                {
                    if (SmoothUnstucker.Attempts < 10)
                    {
                        SmoothUnstucker.Start();
                        return false;
                    }
                    // Let DB/Trinity try and fix it
                    return true;
                }
                return false;
            } 
        }

        public Vector3 GetUnstuckPos() 
        { 
            return PlayerMover.UnstuckHandler(); 
        }

        /// <summary>
        /// This is based on the RandomMoveTag from QuestTools, rewired to run off add/remove hook rather instead of a tag behavior.
        /// The important aspect is movement without DB pathfinding/navigation, which is the primary issue in getting in and out of stuckness.
        /// It attempts to move to each of the points given/generated and will discard a point if the bot isnt moving towards it.
        /// </summary>
        public class SmoothUnstucker
        {
            public static int Timeout { get; set; }
            public static bool IsRunning { get; private set; }
            public static int Attempts { get; private set; }
            
            private static List<Vector3> _points = new List<Vector3>();
            private static DateTime _startTime = DateTime.MaxValue;
            private static DateTime _lastMoving = DateTime.MinValue;
            private static Composite _hook;
            private static bool _hookInserted;
            private static int _stuckAttempt;
            private static bool _isDone;

            private static bool IsDone
            {
                get
                {
                    if(DateTime.UtcNow.Subtract(_startTime).TotalSeconds > Timeout)
                        Stop();

                    return _isDone;
                }
                set
                {
                    if(value)
                        Stop();

                    _isDone = value;
                }
            }           

            public static void Start(List<Vector3> points = null)
            {
                if (IsRunning)
                    return;

                UpdateStuckAttempts();
                Logger.Log("Bot is Stuck! Attempt #{0} to fix it", Attempts);
                    
                Timeout = Timeout <= 0 ? 15 : Timeout;

                _startTime = DateTime.UtcNow;

                _points = points ?? RandomShuffle(GetCirclePoints(20, 30, ZetaDia.Me.Position));

                RandomShuffle(_points);

                _isDone = false;

                InsertHook();
            }

            private static void UpdateStuckAttempts()
            {
                if (DateTime.UtcNow.Subtract(_startTime).TotalSeconds < 30)
                    Attempts++;
                else
                    Attempts = 1;
            }

            public static void Stop()
            {
                _isDone = true;                
                RemoveHook();
            }

            private static void InsertHook()
            {
                _hook = CreateUnstuckBehavior();
                IsRunning = true;
                TreeHooks.Instance.InsertHook("BotBehavior",0, _hook);                
            }

            private static void RemoveHook()
            {
                TreeHooks.Instance.RemoveHook("BotBehavior", _hook);
                IsRunning = false;
            }

            private static List<T> RandomShuffle<T>(List<T> list)
            {
                var rng = new Random();
                var n = list.Count;
                while (n > 1)
                {
                    n--;
                    var k = rng.Next(n + 1);
                    var value = list[k];
                    list[k] = list[n];
                    list[n] = value;
                }
                return list;
            }

            private static List<Vector3> GetCirclePoints(int points, double radius, Vector3 center)
            {
                var result = new List<Vector3>();
                var slice = 2 * Math.PI / points;
                for (var i = 0; i < points; i++)
                {
                    var angle = slice * i;
                    var newX = (int)(center.X + radius * Math.Cos(angle));
                    var newY = (int)(center.Y + radius * Math.Sin(angle));

                    var newpoint = new Vector3(newX, newY, center.Z);
                    result.Add(newpoint);

                    Logger.Log(TrinityLogLevel.Debug, LogCategory.Movement, "Calculated point {0}: {1}", i, newpoint.ToString());
                }
                return result;
            }

            private static bool IsWithinRange(Vector3 position, float range = 12f)
            {
                return position != Vector3.Zero && !(position.Distance2D(ZetaDia.Me.Position) > range);
            }

            protected static Composite CreateUnstuckBehavior()
            {
                return new Decorator(ret => !IsDone,

                    new Action(ret =>
                    {
                        if (!_points.Any() || IsWithinRange(_points.First()))
                        {
                            Logger.Log(TrinityLogLevel.Debug, LogCategory.Movement, "Arrived at Destination {0} Distance={1}", _points.First().ToString(), _points.First());

                            IsDone = true;
                            return RunStatus.Failure;
                        }

                        if (ZetaDia.Me.Movement.IsMoving)
                            _lastMoving = DateTime.UtcNow;

                        var rayResult = ZetaDia.Physics.Raycast(ZetaDia.Me.Position, _points.First(), NavCellFlags.AllowWalk);
                        var stuckResult = DateTime.UtcNow.Subtract(_lastMoving).TotalMilliseconds > 250;

                        if (stuckResult || !rayResult)
                        {
                            Logger.Log(TrinityLogLevel.Debug, LogCategory.Movement, "Discarded Location {0} Distance={1} RaycastResult={2} StuckResult={3} LocationsRemaining={4}", _points.First().ToString(), _points.First().Distance(ZetaDia.Me.Position), rayResult, stuckResult, _points.Count - 1);
                            _points.RemoveAt(0);
                        }

                        if (_points.Any() && _points.First() != Vector3.Zero && ZetaDia.Me.Movement.MoveActor(_points.First()) == 1)
                            return RunStatus.Success;

                        IsDone = true;
                        return RunStatus.Failure;
                    })
                );
            }

        }

    }
}
