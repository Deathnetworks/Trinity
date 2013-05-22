using System;
using System.Collections.Generic;
using Zeta.Common;
using Zeta.Common.Plugins;
using Zeta.Internals.Actors;
using Zeta.Internals.SNO;

namespace Trinity
{
    public partial class Trinity : IPlugin
    {
        /* 
         * This set of dictionaries are used for performance increases throughout, and a minimization of DB mis-read/null exception errors
         * Uses a little more ram - but for a massive CPU gain. And ram is cheap, CPU is not!
         */

        /// <summary>
        /// Contains the time we last used a spell
        /// </summary>
        public static Dictionary<SNOPower, DateTime> AbilityLastUsedCache { get { return abilityLastUsedCache; } internal set { abilityLastUsedCache = value; } }
        private static Dictionary<SNOPower, DateTime> abilityLastUsedCache = new Dictionary<SNOPower, DateTime>();

        /// <summary>
        /// Special cache for monster types {ActorSNO, MonsterType}
        /// </summary>
        private static Dictionary<int, MonsterType> dictionaryStoredMonsterTypes = new Dictionary<int, MonsterType>();

        /// <summary>
        /// Special cache for Monster sizes {ActorSNO, MonsterSize}
        /// </summary>
        private static Dictionary<int, MonsterSize> dictionaryStoredMonsterSizes = new Dictionary<int, MonsterSize>();
      
        /// <summary>
        /// Caches the ObjectType of each object as we find it (RactorGUID based)
        /// </summary>
        private static Dictionary<int, GObjectType> objectTypeCache = new Dictionary<int, GObjectType>();
        /// <summary>
        /// Caches monster affixes for the monster ID, as this value can be a pain to get and slow (RactorGUID based)
        /// </summary>
        private static Dictionary<int, MonsterAffixes> unitMonsterAffixCache = new Dictionary<int, MonsterAffixes>();
        /// <summary>
        /// Caches each monster's max-health, since this never changes (RactorGUID based)
        /// </summary>
        private static Dictionary<int, double> unitMaxHealthCache = new Dictionary<int, double>();
        /// <summary>
        /// Caches each monster's current health for brief periods  (RactorGUID based)
        /// </summary>
        private static Dictionary<int, double> currentHealthCache = new Dictionary<int, double>();
        private static Dictionary<int, int> currentHealthCheckTimeCache = new Dictionary<int, int>();
        /// <summary>
        /// Store a "not-burrowed" value for monsters that we have already checked a burrowed-status of and found false (RactorGUID based)
        /// </summary>
        private static Dictionary<int, bool> unitBurrowedCache = new Dictionary<int, bool>();
        /// <summary>
        /// Store Actor SNO for each object (RactorGUID based)
        /// </summary>
        private static Dictionary<int, int> actorSNOCache = new Dictionary<int, int>();
        /// <summary>
        /// Store ACDGUID for each object (RactorGUID based)
        /// </summary>
        private static Dictionary<int, int> ACDGUIDCache = new Dictionary<int, int>();
        /// <summary>
        /// Store internal name for each object (RactorGUID based)
        /// </summary>
        private static Dictionary<int, string> nameCache = new Dictionary<int, string>();
        /// <summary>
        /// Store Collision-sphere radius for each object (SNO based)
        /// </summary>
        private static Dictionary<int, float> collisionSphereCache = new Dictionary<int, float>();
        /// <summary>
        /// Caches the game balance ID for each object, which can then be used to pull up the appropriate entry from within GameBalanceDataCache (RactorGUID based)
        /// </summary>
        private static Dictionary<int, int> gameBalanceIDCache = new Dictionary<int, int>();
        /// <summary>
        /// Caches the Dynamic ID for each object (only used for non-units) (RactorGUID based)
        /// </summary>
        private static Dictionary<int, int> dynamicIDCache = new Dictionary<int, int>();
        /// <summary>
        /// Caches the position for each object (only used for non-units, as this data is static so can be cached) (RactorGUID based)
        /// </summary>
        private static Dictionary<int, Vector3> positionCache = new Dictionary<int, Vector3>();
        /// <summary>
        /// Same as above but for gold-amount of pile (RactorGUID based)
        /// </summary>
        private static Dictionary<int, int> goldAmountCache = new Dictionary<int, int>();
        ///// <summary>
        ///// Same as above but for quality of item, we check this twice to make bloody sure we don't miss a legendary from a mis-read though (RactorGUID based)
        ///// </summary>
        private static Dictionary<int, ItemQuality> itemQualityCache = new Dictionary<int, ItemQuality>();
        /// <summary>
        /// Same as above but for whether we want to pick it up or not (RactorGUID based)
        /// </summary>
        private static Dictionary<int, bool> pickupItemCache = new Dictionary<int, bool>();
        /// <summary>
        /// How many times the player tried to interact with this object in total
        /// </summary>
        private static Dictionary<int, int> interactAttemptsCache = new Dictionary<int, int>();
        /// <summary>
        /// Physics SNO for certain objects (SNO based)
        /// </summary>
        private static Dictionary<int, int> physicsSNOCache = new Dictionary<int, int>();
        /// <summary>
        /// Summoned-by ID for units (RactorGUID based)
        /// </summary>
        private static Dictionary<int, int> summonedByIdCache = new Dictionary<int, int>();

        /// <summary>
        /// If a unit, item, or other object has been navigable/visible before, this will contain true value and will be considered for targetting, otherwise we will continue to check
        /// </summary>
        private static Dictionary<int, bool> hasBeenNavigableCache = new Dictionary<int, bool>();

        /// <summary>
        /// If a unit, item, or other object has been raycastable before, this will contain true value and will be considered for targetting, otherwise we will continue to check
        /// </summary>
        private static Dictionary<int, bool> hasBeenRayCastedCache = new Dictionary<int, bool>();

        /// <summary>
        /// If a unit, item, or other object has been in LoS before, this will contain true value and will be considered for targetting, otherwise we will continue to check
        /// </summary>
        private static Dictionary<int, bool> hasBeenInLoSCache = new Dictionary<int, bool>();

    }
}
