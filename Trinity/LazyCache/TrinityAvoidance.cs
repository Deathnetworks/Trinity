using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Zeta.Game.Internals;
using Zeta.Game.Internals.Actors;
using Zeta.Game.Internals.SNO;
using Logger = Trinity.Technicals.Logger;

namespace Trinity.LazyCache
{
    /// <summary>
    /// Trinity Avoidance Object
    /// </summary>
    public class TrinityAvoidance : TrinityObject
    {
        #region Constructors

        public TrinityAvoidance()
        {
            AvoidanceStartTime = CacheManager.LastUpdated;
        }

        public TrinityAvoidance(DiaObject rActor) : base(rActor)
        {
            AvoidanceStartTime = CacheManager.LastUpdated;
        }

        #endregion

        #region Fields

        private readonly CacheField<float> _avoidanceHealth = new CacheField<float>(UpdateSpeed.Ultra);
        private readonly CacheField<float> _avoidanceRadius = new CacheField<float>(UpdateSpeed.Ultra);
        private readonly CacheField<bool> _isAvoidAtPlayerPosition = new CacheField<bool>();
        private readonly CacheField<bool> _isSpawnerAvoidance = new CacheField<bool>(UpdateSpeed.Ultra);
        private readonly CacheField<TimeSpan> _avoidanceSpawnerDuration = new CacheField<TimeSpan>();
        private readonly CacheField<bool> _isPlayerImmune = new CacheField<bool>();
        private readonly CacheField<bool> _isProjectile = new CacheField<bool>(UpdateSpeed.Ultra);
        private readonly CacheField<bool> _shouldAvoid = new CacheField<bool>(UpdateSpeed.Ultra);

        #endregion

        #region Properties

        /// <summary>
        /// Amount of player health when this avoidance should be avoided.
        /// </summary>
        public float AvoidanceHealth
        {
            get { return _avoidanceHealth.IsCacheValid ? _avoidanceHealth.CachedValue : (_avoidanceHealth.CachedValue = AvoidanceManager.GetAvoidanceHealthBySNO(ActorSNO, 100)); }
            set { _avoidanceHealth.SetValueOverride(value); }
        }

        /// <summary>
        /// Distance from the center of the avoidance to the edge
        /// </summary>
        public float AvoidanceRadius
        {
            get { return _avoidanceRadius.IsCacheValid ? _avoidanceRadius.CachedValue : (_avoidanceRadius.CachedValue = AvoidanceManager.GetAvoidanceRadius(this)); }
            set { _avoidanceRadius.SetValueOverride(value); }
        }

        /// <summary>
        /// Distance from the center of the avoidance to the edge
        /// </summary>
        public bool IsAvoidAtPlayerPosition
        {
            get { return _isAvoidAtPlayerPosition.IsCacheValid ? _isAvoidAtPlayerPosition.CachedValue : (_isAvoidAtPlayerPosition.CachedValue = DataDictionary.AvoidAnimationAtPlayer.Contains((int)CurrentAnimation)); }
            set { _isAvoidAtPlayerPosition.SetValueOverride(value); }
        }

        /// <summary>
        /// If object creates spawns avoidance objects (which may not be detectable as an avoidance)
        /// </summary>
        public bool IsSpawnerAvoidance
        {
            get { return _isSpawnerAvoidance.IsCacheValid ? _isSpawnerAvoidance.CachedValue : (_isSpawnerAvoidance.CachedValue = DataDictionary.AvoidanceSpawners.Contains(ActorSNO)); }
            set { _isSpawnerAvoidance.SetValueOverride(value); }
        }

        /// <summary>
        /// Time that spawner should be avoided for
        /// </summary>
        public TimeSpan AvoidanceSpawnerDuration
        {
            get { return _avoidanceSpawnerDuration.IsCacheValid ? _avoidanceSpawnerDuration.CachedValue : (_avoidanceSpawnerDuration.CachedValue = GetSpawnerDuration()); }
            set { _avoidanceSpawnerDuration.SetValueOverride(value); }
        }

        /// <summary>
        /// When this avoidance first appeared.
        /// </summary>
        public DateTime AvoidanceStartTime { get; set; }

        /// <summary>
        /// Time that spawner should be avoided for
        /// </summary>
        public bool IsPlayerImmune
        {
            get { return _isPlayerImmune.IsCacheValid ? _isPlayerImmune.CachedValue : (_isPlayerImmune.CachedValue = AvoidanceManager.IsPlayerImmune(AvoidanceType)); }
            set { _isPlayerImmune.SetValueOverride(value); }
        }

        /// <summary>
        /// If the avoidance is a projectile (DataDictionary based)
        /// </summary>
        public bool IsProjectile
        {
            get { return _isProjectile.IsCacheValid ? _isProjectile.CachedValue : (_isProjectile.CachedValue = DataDictionary.AvoidanceProjectiles.Contains(ActorSNO)); }
            set { _isProjectile.SetValueOverride(value); }
        }

        /// <summary>
        /// If the avoidance is a projectile (DataDictionary based)
        /// </summary>
        public bool ShouldAvoid
        {
            get { return _shouldAvoid.IsCacheValid ? _shouldAvoid.CachedValue : (_shouldAvoid.CachedValue = CacheManager.Me.CurrentHealthPct <= AvoidanceHealth && Distance <= AvoidanceRadius); }
            set { _shouldAvoid.SetValueOverride(value); }
        }

        #endregion


        #region Methods

        public override string ToString()
        {
            return String.Format("{0}, Type={1} AvoidanceType={2} Health={3} Radius={4} Dist={5}", Name, TrinityType, AvoidanceType, AvoidanceHealth, AvoidanceRadius, RadiusDistance);
        }

        private TimeSpan GetSpawnerDuration()
        {
            TimeSpan duration;
            return DataDictionary.AvoidanceSpawnerDuration.TryGetValue(ActorSNO, out duration) ? duration : TimeSpan.Zero;
        }

        #endregion

    }
}
