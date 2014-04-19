using System;
using System.Collections.Generic;
using Trinity.Cache;
using Zeta.Common;
using Zeta.Game.Internals.Actors;
using Zeta.Game.Internals.SNO;

namespace Trinity
{
    public class CacheData
    {
        /* 
         * This set of dictionaries are used for performance increases throughout, and a minimization of DB mis-read/null exception errors
         * Uses a little more ram - but for a massive CPU gain. And ram is cheap, CPU is not!
         */

        /// <summary>
        /// Contains the time we last used a spell
        /// </summary>
        public static Dictionary<SNOPower, DateTime> AbilityLastUsed { get { return abilityLastUsedCache; } internal set { abilityLastUsedCache = value; } }
        private static Dictionary<SNOPower, DateTime> abilityLastUsedCache = new Dictionary<SNOPower, DateTime>();

        /// <summary>
        /// Special cache for monster types {ActorSNO, MonsterType}
        /// </summary>
        internal static Dictionary<int, MonsterType> MonsterTypes = new Dictionary<int, MonsterType>();

        /// <summary>
        /// Special cache for Monster sizes {ActorSNO, MonsterSize}
        /// </summary>
        internal static Dictionary<int, MonsterSize> MonsterSizes = new Dictionary<int, MonsterSize>();

        /// <summary>
        /// Caches monster affixes for the monster ID, as this value can be a pain to get and slow (RactorGUID based)
        /// </summary>
        internal static Dictionary<int, MonsterAffixes> UnitMonsterAffix = new Dictionary<int, MonsterAffixes>();

        /// <summary>
        /// Caches each monster's max-health, since this never changes (RactorGUID based)
        /// </summary>
        internal static Dictionary<int, double> UnitMaxHealth = new Dictionary<int, double>();

        /// <summary>
        /// Caches each monster's current health for brief periods  (RactorGUID based)
        /// </summary>
        internal static Dictionary<int, double> CurrentUnitHealth = new Dictionary<int, double>();

        /// <summary>
        /// Stores when we last checked a units health (we don't check too fast, to avoid hitting game memory)
        /// </summary>
        internal static Dictionary<int, int> LastCheckedUnitHealth = new Dictionary<int, int>();

        /// <summary>
        /// Store a "not-burrowed" value for monsters that we have already checked a burrowed-status of and found false (RactorGUID based)
        /// </summary>
        internal static Dictionary<int, bool> UnitIsBurrowed = new Dictionary<int, bool>();

        /// <summary>
        /// Store Collision-sphere radius for each object (SNO based)
        /// </summary>
        internal static Dictionary<int, float> CollisionSphere = new Dictionary<int, float>();

        /// <summary>
        /// Caches the position for each object (only used for non-units, as this data is static so can be cached) (RactorGUID based)
        /// </summary>
        internal static Dictionary<int, Vector3> Position = new Dictionary<int, Vector3>();

        /// <summary>
        /// Same as above but for gold-amount of pile (RactorGUID based)
        /// </summary>
        internal static Dictionary<int, int> GoldStack = new Dictionary<int, int>();
        ///// <summary>
        ///// Same as above but for quality of item, we check this twice to make bloody sure we don't miss a legendary from a mis-read though (RactorGUID based)
        ///// </summary>
        internal static Dictionary<int, ItemQuality> ItemQuality = new Dictionary<int, ItemQuality>();

        /// <summary>
        /// Same as above but for whether we want to pick it up or not (RactorGUID based)
        /// </summary>
        internal static Dictionary<int, bool> PickupItem = new Dictionary<int, bool>();

        /// <summary>
        /// How many times the player tried to interact with this object in total
        /// </summary>
        internal static Dictionary<int, int> InteractAttempts = new Dictionary<int, int>();
        /// <summary>
        /// Summoned-by ID for units (RactorGUID based)
        /// </summary>
        internal static Dictionary<int, int> SummonedByACDId = new Dictionary<int, int>();

        /// <summary>
        /// If a unit, item, or other object has been navigable/visible before, this will contain true value and will be considered for targetting, otherwise we will continue to check
        /// </summary>
        internal static Dictionary<int, bool> HasBeenNavigable = new Dictionary<int, bool>();

        /// <summary>
        /// If a unit, item, or other object has been raycastable before, this will contain true value and will be considered for targetting, otherwise we will continue to check
        /// </summary>
        internal static Dictionary<int, bool> HasBeenRayCasted = new Dictionary<int, bool>();

        /// <summary>
        /// If a unit, item, or other object has been in LoS before, this will contain true value and will be considered for targetting, otherwise we will continue to check
        /// </summary>
        internal static Dictionary<int, bool> HasBeenInLoS = new Dictionary<int, bool>();

        /// <summary>
        /// Stores the computed ItemQuality from an ACDItem.ItemLink (ACDGuid based)
        /// </summary>
        internal static Dictionary<int, ItemQuality> ItemLinkQuality = new Dictionary<int, ItemQuality>();

        /// <summary>
        /// Stores if a unit/monster is a Summoner (spawns other units) (ACDGuid based)
        /// </summary>
        internal static Dictionary<int, bool> IsSummoner = new Dictionary<int, bool>();

        /// <summary>
        /// Obstacle cache, things we can't or shouldn't move through
        /// </summary>
        internal static HashSet<CacheObstacleObject> NavigationObstacles = new HashSet<CacheObstacleObject>();

        /// <summary>
        /// A list of all monsters and their positions, so we don't try to walk through them during avoidance
        /// </summary>
        internal static HashSet<CacheObstacleObject> MonsterObstacles = new HashSet<CacheObstacleObject>();

        /// <summary>
        /// A list of all current obstacles, to help avoid running through them when picking targets
        /// </summary>
        internal static HashSet<CacheObstacleObject> AvoidanceObstacles = new HashSet<CacheObstacleObject>();

        /// <summary>
        /// A set of Avoidances that appear then disappear from the object manager, but can still hurt our player. We need to expire these based on a Timespan from the obstacle object.
        /// </summary>
        internal static HashSet<CacheObstacleObject> TimeBoundAvoidance = new HashSet<CacheObstacleObject>();

        /// <summary>
        /// Stores the last use of same world portals, like in Pandemonium fortress 
        /// </summary>
        internal static HashSet<SameWorldPortal> SameWorldPortals = new HashSet<SameWorldPortal>();

        /// <summary>
        /// Contains an RActorGUID and count of the number of times we've switched to this target
        /// </summary>
        internal static Dictionary<string, int> PrimaryTargetCount = new Dictionary<string, int>();

        /// <summary>
        /// Events that have expired without being completed
        /// </summary>
        internal static HashSet<int> BlacklistedEvents = new HashSet<int>();

        /// <summary>
        /// Called every cache-refresh
        /// </summary>
        internal static void Clear()
        {
            CacheData.CollisionSphere.Clear();
            CacheData.CurrentUnitHealth.Clear();
            CacheData.LastCheckedUnitHealth.Clear();
            CacheData.MonsterObstacles.Clear();
            CacheData.MonsterSizes.Clear();
            CacheData.MonsterTypes.Clear();
            CacheData.Position.Clear();
            CacheData.SummonedByACDId.Clear();
            CacheData.TimeBoundAvoidance.RemoveWhere(aoe => aoe.Expires < DateTime.UtcNow);
            CacheData.UnitIsBurrowed.Clear();
            CacheData.UnitMaxHealth.Clear();
            CacheData.UnitMonsterAffix.Clear();
        }

        /// <summary>
        /// Called on bot stop, new game, join game, etc
        /// </summary>
        internal static void FullClear()
        {
            Clear();

            CacheData.AbilityLastUsed = new Dictionary<SNOPower, DateTime>(DataDictionary.LastUseAbilityTimeDefaults);
            CacheData.AvoidanceObstacles.Clear();
            CacheData.GoldStack.Clear();
            CacheData.HasBeenInLoS.Clear();
            CacheData.HasBeenNavigable.Clear();
            CacheData.HasBeenRayCasted.Clear();
            CacheData.InteractAttempts.Clear();
            CacheData.IsSummoner.Clear();
            CacheData.ItemLinkQuality.Clear();
            CacheData.ItemLinkQuality.Clear();
            CacheData.NavigationObstacles.Clear();
            CacheData.PickupItem.Clear();
            CacheData.PrimaryTargetCount.Clear();
            CacheData.SameWorldPortals.Clear();
            CacheData.TimeBoundAvoidance.Clear();
            CacheData.BlacklistedEvents.Clear();
        }

    }
}
