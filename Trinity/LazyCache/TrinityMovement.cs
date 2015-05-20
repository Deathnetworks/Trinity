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
using Zeta.Common;
using Zeta.Game;
using Zeta.Game.Internals;
using Zeta.Game.Internals.Actors;
using Zeta.Game.Internals.SNO;
using Logger = Trinity.Technicals.Logger;

namespace Trinity.Helpers
{
    /// <summary>
    /// DiaUnit.Movement object is throwing all sorts of weird exceptions on Player.
    /// So adapting our own version so we don't have to call it.
    /// </summary>
    public class TrinityMovement
    {
        private readonly List<SpeedSensor> _speedSensors = new List<SpeedSensor>();
        private const int MaxSpeedSensors = 5;
        private DateTime _lastRecordedPositionTime = DateTime.MinValue;
        
        private float _lastHeadingRadians;
        private float _lastHeadingDegrees;
        private string _lastHeading = string.Empty;
        private double _lastMovementSpeed;

        public bool HasntMoved { get; set; }

        private float _startHeadingRadians;

        /// <summary>
        /// This is separated from GetMovementSpeed() to prevent parrallel tasks failing on changed collection.
        /// </summary>
        /// <param name="o"></param>
        public void RecordMovement(TrinityObject o)
        {
            if (o.ActorType != ActorType.Monster && o.ActorType != ActorType.Projectile && o.ActorType != ActorType.Player)
                return;

            if (DateTime.UtcNow.Subtract(_lastRecordedPositionTime).TotalMilliseconds >= 250)
            {
                if (!_speedSensors.Any())
                {
                    _startHeadingRadians = o.DiaUnit.Movement.Rotation;

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
                        HasntMoved = false;
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

        public float GetHeadingDegrees()
        {
            if (_speedSensors.Count >= 2)
            {
                if (_lastMovementSpeed == 0)
                    return _lastHeadingDegrees;

                var startPosition = _speedSensors.ElementAt(_speedSensors.Count - 2).Location;
                var endPosition = _speedSensors.ElementAt(_speedSensors.Count - 1).Location;
                var headingDegrees = MathUtil.FindDirectionDegree(startPosition, endPosition);                
                _lastHeadingDegrees = headingDegrees;
                return headingDegrees;
            }
            return (float)MathUtil.RadianToDegree(_startHeadingRadians);
        }

        public float GetHeadingRadians()
        {
            if (_speedSensors.Count >= 2)
            {
                if (_lastMovementSpeed == 0)
                    return _lastHeadingRadians;

                var startPosition = _speedSensors.ElementAt(_speedSensors.Count - 2).Location;
                var endPosition = _speedSensors.ElementAt(_speedSensors.Count - 1).Location;
                var headingRadians = (float)MathUtil.FindDirectionRadian(startPosition, endPosition);
                _lastHeadingRadians = headingRadians;
                return headingRadians;
            }

            return _startHeadingRadians;
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
            double movementSpeed;

            if (_speedSensors.Any(s => s != null && s.WorldID != CacheManager.WorldDynamicId))
            {
                _speedSensors.Clear();
                movementSpeed = 1d;
            }

            else if (o.IsMe && DateTime.UtcNow.Subtract(SpellHistory.GetSpellLastused()).TotalMilliseconds <= 1000)
            {
                movementSpeed = 1d;                
            }

            // Minimum of 2 records to calculate speed
            else if (!_speedSensors.Any() || _speedSensors.Count <= 1)
                movementSpeed = 0d;                

            // If we haven't "moved" in over a second, then we're standing still
            else if (DateTime.UtcNow.Subtract(_lastRecordedPositionTime).TotalMilliseconds > 1000)
                movementSpeed = 0d;

            else
            {
                double averageRecordingTime = _speedSensors.Average(s => s.TimeSinceLastMove.TotalHours);        
                double averageMovementSpeed = _speedSensors.Average(s => Vector3.Distance(s.Location, o.Position) * 1000000);
                movementSpeed =  averageMovementSpeed / averageRecordingTime;
            }

            _lastMovementSpeed = movementSpeed;
            return movementSpeed;
        }
    }
}
