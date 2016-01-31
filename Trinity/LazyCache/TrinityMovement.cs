using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trinity.Combat;
using Trinity.Combat.Abilities;
using Trinity.LazyCache;
using Zeta.Bot;
using Zeta.Bot.Dungeons;
using Zeta.Bot.Navigation;
using Zeta.Bot.Pathfinding;
using Zeta.Common;
using Zeta.Game;
using Zeta.Game.Internals;
using Zeta.Game.Internals.Actors;
using Zeta.Game.Internals.SNO;
using Logger = Trinity.Technicals.Logger;

namespace Trinity.Helpers
{
    public class TrinityMovement
    {
        private readonly List<SpeedSensor> _speedSensors = new List<SpeedSensor>();
        private const int MaxSpeedSensors = 5;
        private DateTime _lastRecordedPositionTime = DateTime.MinValue;
        
        private float _lastHeadingRadians;
        private double _movementSpeed;

        public bool HasMoved { get; set; }

        private float _startHeadingRadians;
        private double _averageMovementSpeed;
        private double _averageRecordingTimeHours;
        private float _averageDistance;

        /// <summary>
        /// This is separated from GetMovementSpeed() to prevent parrallel tasks failing on changed collection.
        /// </summary>
        /// <param name="o"></param>
        public void RecordMovement(TrinityObject o)
        {
            if (o.ActorType != ActorType.Monster && o.ActorType != ActorType.Projectile && o.ActorType != ActorType.Player)
            {
                if (o.AvoidanceType != AvoidanceType.Arcane)
                    return;
            }

            if (!_speedSensors.Any() || o.AvoidanceType == AvoidanceType.Arcane)
            {
                // For things that don't move, like units standing in town we can't calculate which way they're facing.
                _startHeadingRadians = o.Object.Movement.Rotation;                
            }

            if (DateTime.UtcNow.Subtract(_lastRecordedPositionTime).TotalMilliseconds >= (int)UpdateSpeed.Ultra)
            {
                if (!_speedSensors.Any())
                {                    
                    _speedSensors.Add(new SpeedSensor
                    {
                        Location = o.Position,
                        TimeSinceLastMove = new TimeSpan(0),
                        Distance = 0f,
                        WorldID = CacheManager.WorldDynamicId
                    });
                    _lastRecordedPositionTime = DateTime.UtcNow;
                }
                else
                {
                    var lastSensor = _speedSensors.Last();
                    var distanceTravelled = Vector3.Distance(o.Position, lastSensor.Location);
                    if (distanceTravelled > 1f)
                    {
                        _speedSensors.Add(new SpeedSensor
                        {
                            Location = o.Position,
                            TimeSinceLastMove = new TimeSpan(DateTime.UtcNow.Subtract(lastSensor.TimeSinceLastMove).Ticks),
                            Distance = distanceTravelled,
                            WorldID = CacheManager.WorldDynamicId
                        });

                        _lastRecordedPositionTime = DateTime.UtcNow;
                        HasMoved = true;
                    }
                }
            }

            // Check if we have enough recorded positions, remove one if so
            while (_speedSensors.Count > MaxSpeedSensors - 1)
            {
                _speedSensors.RemoveAt(0);
            }            
        }

        public Vector2 GetDirectionVector()
        {
            if (_speedSensors.Count >= 2)
            {
                var startPosition = _speedSensors.ElementAt(_speedSensors.Count - 2).Location;
                var endPosition = _speedSensors.ElementAt(_speedSensors.Count - 1).Location;
                return MathUtil.GetDirectionVector(startPosition, endPosition);
            }
            return Vector2.Zero;
        }

        public float GetHeadingRadians()
        {
            if (_speedSensors.Count >= 2)
            {
                if (_movementSpeed == 0 && !HasMoved)
                    return _lastHeadingRadians;

                var startPosition = _speedSensors.ElementAt(_speedSensors.Count - 2).Location;
                var endPosition = _speedSensors.ElementAt(_speedSensors.Count - 1).Location;
                var headingRadians = (float)MathUtil.FindDirectionRadian(startPosition, endPosition);
                _lastHeadingRadians = headingRadians;
                return headingRadians;
            }

            return _startHeadingRadians;
        }

        public float GetHeadingDegrees()
        {
            return (float)MathUtil.RadianToDegree(GetHeadingRadians());
        }

        public string GetHeading()
        {
            return MathUtil.GetHeading(GetHeadingDegrees());
        }

        public static void LogComparison(TrinityUnit u)
        {
            Logger.Log("{8} Speed={0}/{1} Rotation={2}/{3} RotationDegrees={4}/{5} Heading={6}/{7}",
                u.Movement.GetMovementSpeed(u), u.DiaUnit.Movement.SpeedXY,
                u.Movement.GetHeadingRadians(), u.DiaUnit.Movement.Rotation,
                u.Movement.GetHeadingDegrees(), u.DiaUnit.Movement.RotationDegrees,
                u.Movement.GetHeading(), MathUtil.GetHeading(u.DiaUnit.Movement.RotationDegrees),
                u.Name);            
        }

        public double GetMovementSpeed(TrinityObject o)
        {
            if (_speedSensors.Any(s => s != null && s.WorldID != CacheManager.WorldDynamicId))
            {
                _speedSensors.Clear();
                _movementSpeed = 1d;
            }

            else if (o.IsMe && DateTime.UtcNow.Subtract(SpellHistory.GetSpellLastused()).TotalMilliseconds <= 1000)
            {
                _movementSpeed = 1d;                
            }

            // Minimum of 2 records to calculate speed
            else if (!_speedSensors.Any() || _speedSensors.Count <= 1)
                _movementSpeed = 0d;                

            // If we haven't "moved" in over a second, then we're standing still
            else if (DateTime.UtcNow.Subtract(_lastRecordedPositionTime).TotalMilliseconds > 1000)
                _movementSpeed = 0d;

            else
            {
                _averageRecordingTimeHours = _speedSensors.Average(s => s.TimeSinceLastMove.TotalHours);
                _averageDistance = _speedSensors.Average(s => Vector3.Distance(s.Location, o.Position));
                _averageMovementSpeed =  _averageDistance * 1000000;
                _movementSpeed =  _averageMovementSpeed / _averageRecordingTimeHours;
            }

            return _movementSpeed;
        }

        public bool TryPredictPosition(DateTime time, out Vector3 position)
        {
            var last = _speedSensors.LastOrDefault();

            //var averageRecordingTimeMs = _speedSensors.Average(s => s.TimeSinceLastMove.TotalMilliseconds);
            //var distanceTravelled = _speedSensors.Average(s => Vector3.Distance(s.Location, CacheManager.Me.Position));
            //var distancePerMs = distanceTravelled/averageRecordingTimeMs;
            //var timeDifferenceMs = DateTime.UtcNow.Subtract(time).TotalMilliseconds;

            if (last == null)
            {
                position = Vector3.Zero;
                return false;
            }
                

            var averageRecordingTimeMs = DateTime.UtcNow.Subtract(last.Timestamp).TotalMilliseconds;
            var distanceTravelled = last.Location.Distance(CacheManager.Me.Position);
            var distancePerMs = distanceTravelled/averageRecordingTimeMs;
            var timeDifferenceMs = time.Subtract(DateTime.UtcNow).TotalMilliseconds;
            var distanceToTravel = timeDifferenceMs * distancePerMs;

            Logger.Log("averageRecordingTimeMs={0} distanceTravelled={1} distancePerMs={2} timeDifferenceMs={3} distToTravel={4}",
                averageRecordingTimeMs,
                distanceTravelled,
                distancePerMs,
                timeDifferenceMs,
                distanceToTravel);



            if (last != null)
            {
                


                position = MathEx.GetPointAt(last.Location, (float)distanceToTravel, GetHeadingRadians());
                //position = MathUtil.GetEstimatedPosition(
                //                    last.Location,
                //                    GetHeadingRadians(),
                //                    timeDifferenceMs,
                //                    distancePerMs);  

                return true;                             
            }
            position = Vector3.Zero;
            return false;
        }
    }
}
