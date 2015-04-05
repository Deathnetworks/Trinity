using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Trinity.DbProvider;
using Trinity.Technicals;
using Zeta.Bot;
using Zeta.Bot.Dungeons;
using Zeta.Bot.Navigation;
using Zeta.Common;
using Zeta.Game;
using Zeta.TreeSharp;
using Logger = Trinity.Technicals.Logger;

namespace Trinity.Combat
{
    public delegate bool QueuedMovementCondition(QueuedMovement movement);
    public delegate void QueuedMovementUpdateDelegate(QueuedMovement movement);

    /// <summary>
    /// Executed by HandleTarget when added to QueuedMovement.Queue();
    /// </summary>
    public class QueuedMovement
    {
        /// <summary>
        /// A friendly name to identify this SpecialMovement
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Some infos
        /// </summary>
        public string Infos { get; set; }

        /// <summary>
        /// Destination where this SpecialMovement will move to
        /// </summary>
        public Vector3 Destination { get; set; }

        /*/// <summary>
        /// Straight line pathing on a definite route
        /// </summary>
        public HashSet<GridPoint> Route { get; set; }*/

        /// <summary>
        /// Executed directly after moving, will stop SpecialMovement if true is returned
        /// </summary>
        public QueuedMovementCondition StopCondition { get; set; }

        /// <summary>
        /// Executed directly before moving, may be used to update destination
        /// </summary>
        public QueuedMovementUpdateDelegate OnUpdate { get; set; }

        /// <summary>
        /// Executed after success or failure of specialmovement
        /// </summary>
        public QueuedMovementUpdateDelegate OnFinished { get; set; }

        /// <summary>
        /// Executed before execute movement
        /// </summary>
        public QueuedMovementUpdateDelegate OnInitialize { get; set; }

        /// <summary>
        /// Optional Options to further customize the movement
        /// </summary>
        public QueuedMovementOptions Options = new QueuedMovementOptions();

        /// <summary>
        /// The position of player when movement started
        /// </summary>
        public Vector3 StartPosition { get; set; }

        /// <summary>
        /// Status is updated every tick and passed to OnUpdate() and OnFinished() events
        /// </summary>
        public QueuedMovementStatus Status { get; set; }

        public DateTime LastFinishedTime { get; set; }
        public DateTime LastStartedTime { get; set; }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        internal static void Queue(QueuedMovement movement)
        {
            throw new NotImplementedException();
        }
    }

    public class QueuedMovementStatus
    {
        public MoveResult LastStatus { get; set; }
        public Vector3 LastPosition { get; set; }
        public double DistanceToObjective { get; set; }
        public double ChangeInDistance { get; set; }
    }

    public class QueuedMovementOptions
    {
        public QueuedMovementOptions()
        {
            FailureBlacklistSeconds = 0.5;
            SuccessBlacklistSeconds = 0;
            ChangeInDistanceLimit = 2f;
            TimeBeforeBlocked = 1500;
            Logging = LogLevel.Info;
            AcceptableDistance = 8f;
            MaxDistance = 150f;
            Type = MoveType.TargetAttempt;
        }

        /// <summary>
        /// Change in distance since last move tick
        /// </summary>
        public float ChangeInDistanceLimit { get; set; }

        /// <summary>
        /// Time in Milliseconds below the ChangeInDistance setting to be 'blocked'
        /// </summary>
        public double TimeBeforeBlocked { get; set; }

        /// <summary>
        /// Duration movements are blacklisted from re-queue after Blocked or failed MoveResult
        /// </summary>
        public double FailureBlacklistSeconds { get; set; }
        public int SuccessBlacklistSeconds { get; set; }

        /// <summary>
        /// How detailed the logging will be
        /// </summary>
        public LogLevel Logging { get; set; }

        /// <summary>
        /// How close it should get to the destination before considering the destination reached
        /// </summary>
        public float AcceptableDistance { get; set; }

        /// <summary>
        /// How far away the destination is allowed to be
        /// </summary>
        public double MaxDistance { get; set; }

        /// <summary>
        /// To see on queue if moveType priority is higher than last movement
        /// </summary>
        public MoveType Type { get; set; }
    }

    public class QueuedMovementManager
    {
        public QueuedMovement CurrentMovement { get; set; }
        public QueuedMovementOptions Options
        {
            get
            {
                if (CurrentMovement != null)
                    return CurrentMovement.Options;

                return new QueuedMovementOptions();
            }
        }
        private QueuedMovementOptions _options = new QueuedMovementOptions();
        private Queue<QueuedMovement> _internalQueue = new Queue<QueuedMovement>();
        private QueuedMovementStatus _status = new QueuedMovementStatus();
        private readonly List<QueuedMovement> _blacklist = new List<QueuedMovement>();

        public void Queue(QueuedMovement movement)
        {
            if (IsQueuedMovement &&
                CurrentMovement.StopCondition != null &&
                CurrentMovement.StopCondition.Invoke(CurrentMovement))
            {
                SuccessHandler("StopWhen");
            }

            if (movement != null && movement.Destination != Vector3.Zero && IsHigherMoveTypePriority(movement))
            {
                if (CurrentMovement != null)
                    FinishedHandler();

                QueueingHandler(movement);
                LastExecutedMovement = DateTime.UtcNow;
            }
        }

        public RunStatus Execute()
        {
            if (!IsQueuedMovement)
            {
                FailedHandler("Dequeue");
                return RunStatus.Failure;
            }

            if (CurrentMovement == null)
            {
                CurrentMovement = _internalQueue.Dequeue();
                CurrentMovement.StartPosition = ZetaDia.Me.Position;
                CurrentMovement.LastStartedTime = DateTime.UtcNow;
                Stuck.Reset();
            }

            if (DateTime.UtcNow.Subtract(LastExecutedMovement).TotalMilliseconds < 50)
                return RunStatus.Running;

            LastExecutedMovement = DateTime.UtcNow;

            _options = Options;

            if (CurrentMovement.StopCondition != null &&
                CurrentMovement.StopCondition.Invoke(CurrentMovement))
            {
                SuccessHandler("StopWhen");
                return RunStatus.Success;
            }

            if (Stuck.IsStuck(_options.ChangeInDistanceLimit, _options.TimeBeforeBlocked))
            {
                FailedHandler("Blocked " + Stuck.LastLogMessage);
                return RunStatus.Failure;
            }

            if (IsBlacklisted(CurrentMovement))
            {
                FailedHandler("RecentlyFailed");
                return RunStatus.Failure;
            }

            if (CurrentMovement.OnUpdate != null)
                CurrentMovement.OnUpdate.Invoke(CurrentMovement);

            Vector3 _destination = CurrentMovement.Destination;

            _status.DistanceToObjective = ZetaDia.Me.Position.Distance(_destination);

            if (_status.DistanceToObjective < _options.AcceptableDistance)
            {
                SuccessHandler(string.Format("AcceptableDistance: {0}", _options.AcceptableDistance));
                return RunStatus.Success;
            }

            if (_status.DistanceToObjective > _options.MaxDistance)
            {
                FailedHandler(string.Format("MaxDistance: {0}", _options.MaxDistance));
                return RunStatus.Success;
            }

            _status.ChangeInDistance = _status.LastPosition.Distance(_destination) - _status.DistanceToObjective;
            _status.LastPosition = ZetaDia.Me.Position;

            if (DataDictionary.StraightLinePathingLevelAreaIds.Contains(Trinity.Player.LevelAreaId) ||
                _destination.Distance2DSqr(Trinity.Player.Position) <= 10f * 10f)
            {
                Navigator.PlayerMover.MoveTowards(_destination);
                _status.LastStatus = MoveResult.Moved;
            }
            else
            {
                _status.LastStatus = PlayerMover.NavigateTo(_destination, CurrentMovement.Name);
            }

            CurrentMovement.Status = _status;

            switch (_status.LastStatus)
            {
                case MoveResult.ReachedDestination:
                case MoveResult.PathGenerationFailed:
                case MoveResult.PathGenerating:
                case MoveResult.PathGenerated:
                case MoveResult.Moved:
                    MovedHandler();
                    return RunStatus.Running;
                case MoveResult.UnstuckAttempt:
                case MoveResult.Failed:
                    FailedHandler("Navigation");
                    return RunStatus.Failure;
                default:
                    SuccessHandler("MoveResult.Default");
                    return RunStatus.Success;
            }
        }

        /// <summary>
        /// QueuedMovement is finished successfully - arrived at destination.
        /// </summary>
        /// <param name="reason"></param>
        public void SuccessHandler(string reason = "")
        {
            var location = (!string.IsNullOrEmpty(reason) ? "(" + reason + ")" : reason);
            LogLocation("Arrived at " + location, CurrentMovement, Stuck.LastLogMessage, TrinityLogLevel.Verbose);

            if (_options.SuccessBlacklistSeconds > 0 && !_blacklist.Contains(CurrentMovement))
                _blacklist.Add(CurrentMovement);

            FinishedHandler();
        }

        /// <summary>
        /// QueuedMovement is in progress
        /// </summary>
        public void MovedHandler()
        {
            LogLocation("Moving to", CurrentMovement, Stuck.LastLogMessage, TrinityLogLevel.Verbose);
        }

        /// <summary>
        /// QueuedMovement was a dismal failure.
        /// </summary>
        public void FailedHandler(string reason = "")
        {
            if (!_blacklist.Contains(CurrentMovement))
                _blacklist.Add(CurrentMovement);

            PlayerMover.UnstuckHandler();

            var location = (!string.IsNullOrEmpty(reason) ? "(" + reason + ") " : reason);
            LogLocation("Failed " + location + "moving to ", CurrentMovement, Stuck.LastLogMessage, TrinityLogLevel.Verbose);

            FinishedHandler();
        }

        /// <summary>
        /// Common tidy-up after finishing
        /// </summary>
        public void FinishedHandler()
        {
            CurrentMovement.LastFinishedTime = DateTime.UtcNow;

            if (CurrentMovement.OnFinished != null)
                CurrentMovement.OnFinished.Invoke(CurrentMovement);

            CurrentMovement.Options = new QueuedMovementOptions();
            _internalQueue = new Queue<QueuedMovement>();
            _status = new QueuedMovementStatus();
            CurrentMovement = null;
        }

        /// <summary>
        /// Common tidy-up on Queueing
        /// </summary>
        public void QueueingHandler(QueuedMovement movement)
        {
            _internalQueue.Enqueue(movement);

            if (movement.OnInitialize != null)
                movement.OnInitialize.Invoke(CurrentMovement);

            if (movement.Options.Logging >= LogLevel.Info)
                LogLocation("Queueing", movement);

            CurrentMovement = _internalQueue.Dequeue();
            CurrentMovement.StartPosition = ZetaDia.Me.Position;
            CurrentMovement.LastStartedTime = DateTime.UtcNow;
            Stuck.Reset();
        }

        public void LogLocation(string pre, QueuedMovement movement, string post = "", TrinityLogLevel level = TrinityLogLevel.Info)
        {
            Logger.Log(level, LogCategory.Movement, pre + " {0} Distance={4:0.#} (x={1:0.#},y={2:0.#},z={3:0.#}) {5}",
                movement.Infos,
                movement.Destination.X,
                movement.Destination.Y,
                movement.Destination.Z,
                ZetaDia.Me.Position.Distance(movement.Destination),
                post);
        }

        public bool IsQueuedMovement
        {
            get { return _internalQueue.Count > 0 || CurrentMovement != null; }
        }

        public bool IsBlacklisted(QueuedMovement movement)
        {
            _blacklist.RemoveAll(m => m != null && DateTime.UtcNow.Subtract(m.LastFinishedTime).TotalSeconds >= m.Options.FailureBlacklistSeconds);
            return _blacklist.Any(m => m != null && m.Name == movement.Name);
        }

        public bool IsHigherMoveTypePriority(QueuedMovement movement)
        {
            if (!IsQueuedMovement)
                return true;

            if ((int)Options.Type == 0 && (int)movement.Options.Type == 0)
                return true;

            return (int)movement.Options.Type > (int)Options.Type;
        }

        public static class Stuck
        {
            static Stuck()
            {
                Pulsator.OnPulse += (sender, args) => Pulse();
            }

            private static bool _isMoving;
            private static Vector3 _lastPosition = Vector3.Zero;
            public static float ChangeInDistance { get; set; }
            private const int MaxPossibleDistanceTravelled = 1;
            static readonly Stopwatch StuckTime = new Stopwatch();
            private static string _log;

            private static void Pulse()
            {
                try
                {
                    ChangeInDistance = _lastPosition.Distance(ZetaDia.Me.Position);
                    _lastPosition = ZetaDia.Me.Position;
                    IsStuck();
                }
                catch { }
            }

            public static double StuckElapsedMilliseconds
            {
                get { return StuckTime.ElapsedMilliseconds; }
            }

            public static string LastLogMessage
            {
                get { return _log; }
            }

            public static bool IsStuck(float changeInDistanceLimit = 2.5f, double stuckTimeLimit = 500)
            {
                try
                {
                    if (ChangeInDistance < MaxPossibleDistanceTravelled && ChangeInDistance < changeInDistanceLimit * ZetaDia.Me.MovementScalar)
                    {
                        if (_isMoving)
                        {
                            Reset();
                            StuckTime.Start();
                        }
                        _isMoving = false;
                    }
                    else
                    {
                        Reset();
                    }
                }
                catch { }

                _log = string.Format("Speed={0:0.#}/{1:0.#} StuckTime={2:0.#}/{3:0.#}", ChangeInDistance, changeInDistanceLimit * ZetaDia.Me.MovementScalar, StuckTime.ElapsedMilliseconds, stuckTimeLimit);

                var stuck = !_isMoving && StuckTime.ElapsedMilliseconds >= stuckTimeLimit;

                return stuck;
            }

            public static void Reset()
            {
                StuckTime.Stop();
                StuckTime.Reset();
                _isMoving = true;
            }
        }

        public static DateTime LastExecutedMovement = DateTime.UtcNow;
    }



}
