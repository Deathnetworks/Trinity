using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Buddy.Coroutines;
using Trinity.Combat.Abilities;
using Trinity.DbProvider;
using Trinity.Objects;
using Trinity.Reference;
using Zeta.Bot.Navigation;
using Zeta.Common;
using Zeta.Game;
using Zeta.Game.Internals.Actors;
using Zeta.TreeSharp;
using Logger = Trinity.Technicals.Logger;

namespace Trinity.Combat
{

    public delegate bool CombatMovementCondition(CombatMovement movement);
    public delegate void CombatMovementUpdateDelegate(CombatMovement movement);

    public class CombatMovement
    {
        /// <summary>
        /// Executed by HandleTarget when added to CombatMovement.Queue();
        /// </summary>
        public CombatMovement()
        {
            AcceptableDistance = 5;
        }

        /// <summary>
        /// A friendly name to identify this SpecialMovement
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Destination where this SpecialMovement will move to
        /// </summary>
        public Vector3 Destination { get; set; }

        /// <summary>
        /// How close it should get to the destination before considering the destination reached
        /// </summary>
        public float AcceptableDistance { get; set; }

        /// <summary>
        /// Executed directly after moving, will stop SpecialMovement if true is returned
        /// </summary>
        public CombatMovementCondition StopCondition { get; set; }

        /// <summary>
        /// Executed directly before moving, may be used to update destination
        /// </summary>
        public CombatMovementUpdateDelegate OnUpdate { get; set; }

        /// <summary>
        /// Executed after success or failure of specialmovement
        /// </summary>
        public CombatMovementUpdateDelegate OnFinished { get; set; }

        public Vector3 StartPosition { get; set; }
        public CombatMovementStatus Status { get; set; }
        public DateTime LastUsed { get; set; }
        public bool Verbose { get; set; }

        public override int GetHashCode()
        {
            return Name.GetHashCode() ^ Destination.GetHashCode();
        }
    }

    public class CombatMovementStatus
    {
        public MoveResult LastStatus { get; set; }
        public Vector3 LastPosition { get; set; }
        public double DistanceToObjective { get; set; }
        public double ChangeInDistance { get; set; }
    }

    public class CombatMovementManager
    {
        /// <summary>
        /// Change in distance since last move tick
        /// </summary>
        public float DistanceChangeLimit { get; set; }

        /// <summary>
        /// Time below the DistanceChangeLimit before considered blocked
        /// </summary>
        public double TimeToConsiderBlocked { get; set; }

        /// <summary>
        /// Duration movements are blacklisted from re-queue after Blocked or failed MoveResult
        /// </summary>
        public int FailureBlacklistSeconds { get; set; }

        private Queue<CombatMovement> InternalQueue { get; set; }
        private CombatMovement CurrentMovement { get; set; }
        private List<CombatMovement> Blacklist { get; set; }
        private CombatMovementStatus Status { get; set; }
        private bool _isBlocked;

        public CombatMovementManager()
        {
            FailureBlacklistSeconds = 5;
            DistanceChangeLimit = 1.5f;
            TimeToConsiderBlocked = 750;
            Blacklist = new List<CombatMovement>();
            InternalQueue = new Queue<CombatMovement>();
            Status = new CombatMovementStatus();
        }

        public async Task<bool> Execute()
        {
            if (!IsQueuedMovement) return false;

            if (CurrentMovement == null)
                CurrentMovement = InternalQueue.Dequeue();

            if (CurrentMovement.OnUpdate != null)
                CurrentMovement.OnUpdate.Invoke(CurrentMovement);

            Status.LastStatus = PlayerMover.NavigateTo(CurrentMovement.Destination, CurrentMovement.Name);
            Status.DistanceToObjective = ZetaDia.Me.Position.Distance(CurrentMovement.Destination);
            Status.ChangeInDistance = Status.LastPosition.Distance(CurrentMovement.Destination) - Status.DistanceToObjective;
            Status.LastPosition = ZetaDia.Me.Position;
            CurrentMovement.Status = Status;

            if (CurrentMovement.StopCondition != null &&
                CurrentMovement.StopCondition.Invoke(CurrentMovement))
            {
                FailedHandler("StopWhen");
                return false;
            }

            if (IsBlocked)
            {
                FailedHandler("Blocked");
                return false;
            }

            if (Status.DistanceToObjective < CurrentMovement.AcceptableDistance)
            {
                SuccessHandler("AcceptableDistance");
                return true;
            }

            if (HasRecentlyFailed(CurrentMovement))
            {
                FailedHandler("RecentlyFailed");
                return false;
            }

            if (Status.DistanceToObjective > 100)
            {
                FailedHandler("MaxDistance");
                return false;
            }

            switch (Status.LastStatus)
            {
                case MoveResult.ReachedDestination:
                    SuccessHandler();
                    return true;
                case MoveResult.PathGenerationFailed:
                case MoveResult.Moved:
                    MovedHandler();
                    await Coroutine.Yield();
                    return true;
                case MoveResult.Failed:
                    FailedHandler("Navigation");
                    return false;
                default:
                    await Coroutine.Yield();
                    return true;
            }

        }

        private bool IsBlocked
        {
            get
            {
                if (Status.ChangeInDistance < DistanceChangeLimit && !_isBlocked)
                {
                    _isBlocked = true;
                    BlockedSince = DateTime.UtcNow;
                }

                if (Status.ChangeInDistance >= DistanceChangeLimit && _isBlocked)
                    _isBlocked = false;

                if (_isBlocked && TimeSinceBlocked >= TimeToConsiderBlocked)
                {
                    _isBlocked = false;
                    return true;
                }

                return false;
            }

        }

        public void SuccessHandler(string reason = "")
        {
            if (CurrentMovement.Verbose)
            {
                var location = (!string.IsNullOrEmpty(reason) ? "(" + reason + ")" : reason);
                LogLocation("Arrived at " + location, CurrentMovement);
            }
            FinishedHandler();
        }

        public void MovedHandler()
        {
            if (CurrentMovement.Verbose)
                LogLocation("Moving to", CurrentMovement);
        }

        public void FinishedHandler()
        {
            if (CurrentMovement.OnFinished != null)
                CurrentMovement.OnFinished.Invoke(CurrentMovement);

            CurrentMovement = null;
        }

        public void FailedHandler(string reason = "")
        {
            if (!Blacklist.Contains(CurrentMovement))
                Blacklist.Add(CurrentMovement);

            if (CurrentMovement.Verbose)
            {
                var location = (!string.IsNullOrEmpty(reason) ? "(" + reason + ") " : reason);
                LogLocation("Failed " + location + "moving to ", CurrentMovement);
            }
            FinishedHandler();
        }

        public void LogLocation(string pre, CombatMovement movement)
        {
            Logger.LogNormal(pre + " {0} Distance={4:0.##} ({1:0.##},{2:0.##},{3:0.##})",
                movement.Name,
                movement.Destination.X,
                movement.Destination.Y,
                movement.Destination.Z,
                ZetaDia.Me.Position.Distance(movement.Destination));
        }

        public void Queue(CombatMovement movement)
        {
            if (HasRecentlyFailed(movement))
            {
                Logger.LogNormal("Discarding Queue (recently failed)");
            }
            else
            {
                movement.StartPosition = ZetaDia.Me.Position;
                movement.LastUsed = DateTime.UtcNow;
                LogLocation("Queueing", movement);
                InternalQueue.Enqueue(movement);
            }
        }

        public bool IsQueuedMovement
        {
            get
            {
                return InternalQueue.Count > 0 || CurrentMovement != null;
            }
        }

        public bool HasRecentlyFailed(CombatMovement movement)
        {
            return RecentlyFailed.Any(m => m.Name == movement.Name);
        }

        public List<CombatMovement> RecentlyFailed
        {
            get
            {
                Blacklist.RemoveAll(m => DateTime.UtcNow.Subtract(m.LastUsed).TotalSeconds > FailureBlacklistSeconds);
                return Blacklist;
            }
        }

        public DateTime BlockedSince = DateTime.MinValue;
        public double TimeSinceBlocked { get { return DateTime.UtcNow.Subtract(BlockedSince).TotalMilliseconds; } }


    }

}
