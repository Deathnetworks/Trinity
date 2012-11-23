using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using Zeta;
using Zeta.Common;
using Zeta.Common.Plugins;
using Zeta.Internals.Actors;
using Zeta.Internals.SNO;
using Zeta.Pathfinding;

namespace GilesTrinity
{
    public partial class GilesTrinity : IPlugin
    {

        /// <summary>
        /// Settings of the plugin
        /// </summary>
        public static GilesSettings settings = new GilesSettings();

        /// <summary>
        /// Special debugging
        /// </summary>
        private static bool bDebugLogSpecial = false;

        /// <summary>
        /// Dumps info every tick about object caching... this will dump hundreds of MB of data in an hour, be careful!
        /// </summary>
        private static bool bDebugLogRefreshDiaObject = false;

        /// <summary>
        /// Dumps info every tick about object/target weighting... this will dump hundreds of MB of data in an hour, be careful!
        /// </summary>
        private static bool bDebugLogWeights = false;

        /// <summary>
        /// Used for letting noobs know they started the bot without Trinity enabled in the plugins tab.
        /// </summary>
        private static bool bPluginEnabled = false;

        /* A few special variables, mainly for Giles use, just at the top for easy access
         * Set the following to true, to disable file-logging for performance increase
         * WARNING: IF YOU GET CRASHES, ISSUES, OR PROBLEMS AND HAVE LOG-FILES DISABLED...
         * NOBODY CAN HELP YOU. Re-enable logging, wait for the issue/crash/problem, then report it with a log.
         * DO NOT DISABLE LOGGING AND THEN POST BLANK LOGS EXPECTING HELP!
         */
        private const bool bDisableFileLogging = false;

        /// <summary>
        /// This will log item stat balancing data to special log files
        /// </summary>
        private const bool bLogBalanceDataForGiles = false;

        /// <summary>
        /// A flag to say whether any NON-hashActorSNOWhirlwindIgnore things are around
        /// </summary>
        private static bool bAnyNonWWIgnoreMobsInRange = false;

        /* I create so many variables that it's a pain in the arse to categorize them
         * So I just throw them all here for quick searching, reference etc.
         * I've tried to make most variable names be pretty damned obvious what they are for!
         * I've also commented a lot of variables/sections of variables to explain what they are for, incase you are trying to work them all out!
         */
        /// <summary>
        /// A null location, may shave off the tiniest fraction of CPU time, but probably not. Still, I like using this variable! :D
        /// </summary>
        private static readonly Vector3 vNullLocation = Vector3.Zero;

        /// <summary>
        /// Used for a global bot-pause
        /// </summary>
        private static bool bMainBotPaused = false;

        /// <summary>
        /// Used to force-refresh dia objects at least once every XX milliseconds
        /// </summary>
        public static DateTime lastRefreshedObjects = DateTime.Today;

        /// <summary>
        /// This object is used for the main handling - the "current target" etc. as selected by the target-selecter, whether it be a unit, an item, a shrine, anything. 
        /// It's cached data using my own class, so I never need to hit D3 memory to "re-check" the data or to call an interact request or anything
        /// </summary>
        internal static GilesObject CurrentTarget = null;

        /// <summary>
        /// A flag to indicate whether we have a new target from the overlord (decorator) or not, in which case don't refresh targets again this first loop
        /// </summary>
        private static bool bWholeNewTarget = false;

        /// <summary>
        /// A flag to indicate if we should pick a new power/ability to use or not
        /// </summary>
        private static bool bPickNewAbilities = false;

        /// <summary>
        /// Flag used to indicate if we are simply waiting for a power to go off - so don't do any new target checking or anything
        /// </summary>
        private static bool bWaitingForPower = false;

        /// <summary>
        /// A special post power use pause
        /// </summary>
        private static bool bWaitingAfterPower = false;

        /// <summary>
        /// If we are waiting before popping a potion
        /// </summary>
        private static bool bWaitingForPotion = false;

        /// <summary>
        /// Status text for DB main window status
        /// </summary>
        private static string sStatusText = "";

        /// <summary>
        /// A flag to indicate if we just entered or just left archon form (and so to force-update the hotbar)
        /// </summary>
        private static bool bHasHadArchonbuff = false;

        /// <summary>
        /// A flag to see if we need to refresh hot bar abilities
        /// </summary>
        private static bool bRefreshHotbarAbilities = false;

        /// <summary>
        /// A "fake" object to send to target provider for stuck handlers etc.
        /// </summary>
        public static DiaObject thisFakeObject;

        /// <summary>
        /// Timestamp of when our position was last measured as changed
        /// </summary>
        private static DateTime lastMovedDuringCombat = DateTime.Today;

        // The following three dictionaries are special caches - they start empty but fill up with data that gets re-used while the bot runs, saving on D3 memory hits

        /// <summary>
        /// Special cache for monster types {ActorSNO, MonsterType}
        /// </summary>
        private static Dictionary<int, MonsterType> dictionaryStoredMonsterTypes = new Dictionary<int, MonsterType>();

        /// <summary>
        /// Special cache for Monster sizes {ActorSNO, MonsterSize}
        /// </summary>
        private static Dictionary<int, MonsterSize> dictionaryStoredMonsterSizes = new Dictionary<int, MonsterSize>();

        /// <summary>
        /// Used to ignore a specific RActor for <see cref="iIgnoreThisForLoops"/> ticks
        /// </summary>
        private static int iIgnoreThisRactorGUID = 0;

        /// <summary>
        /// Ignore <see cref=" iIgnoreThisRactorGUID"/> for this many ticks
        /// </summary>
        private static int iIgnoreThisForLoops = 0;

        /// <summary>
        /// Holds all of the player's current info handily cached, updated once per loop with a minimum timer on updates to save D3 memory hits
        /// </summary>
        public static GilesPlayerCache playerStatus = new GilesPlayerCache(DateTime.Today, false, false, false, 0d, 0d, 0d, 0d, 0d, vNullLocation, false, 0, 1);

        /// <summary>
        /// Obstacle cache, things we can't or shouldn't move through
        /// </summary>
        internal static HashSet<GilesObstacle> hashNavigationObstacleCache = new HashSet<GilesObstacle>();

        // Related to the profile reloaded when restarting games, to pick the FIRST profile.

        // Also storing a list of all profiles, for experimental reasons/incase I want to use them down the line
        public static List<string> listProfilesLoaded = new List<string>();
        public static string sLastProfileSeen = "";
        public static string sFirstProfileSeen = "";

        // A list of small areas covering zones we move through while fighting to help our custom move-handler skip ahead waypoints
        internal static HashSet<GilesObstacle> hashSkipAheadAreaCache = new HashSet<GilesObstacle>();
        public static DateTime lastAddedLocationCache = DateTime.Today;
        public static Vector3 vLastRecordedLocationCache = Vector3.Zero;
        public static bool bSkipAheadAGo = false;

        /// <summary>
        /// A list of all monsters and their positions, so we don't try to walk through them during avoidance
        /// </summary>
        private static HashSet<GilesObstacle> hashMonsterObstacleCache = new HashSet<GilesObstacle>();

        /// <summary>
        /// A list of all current obstacles, to help avoid running through them when picking targets
        /// </summary>
        private static HashSet<GilesObstacle> hashAvoidanceObstacleCache = new HashSet<GilesObstacle>();

        /// <summary>
        /// Blacklist avoidance spots we failed to reach in time, for a period of time
        /// </summary>
        private static HashSet<GilesObstacle> hashAvoidanceBlackspot = new HashSet<GilesObstacle>();
        private static DateTime lastClearedAvoidanceBlackspots = DateTime.Today;

        // A count for player mystic ally, gargantuans, and zombie dogs
        private static int iPlayerOwnedMysticAlly = 0;
        private static int iPlayerOwnedGargantuan = 0;
        private static int iPlayerOwnedZombieDog = 0;
        private static int iPlayerOwnedDHPets = 0;

        // These are a bunch of safety counters for how many times in a row we register having *NO* ability to select when we need one (eg all off cooldown)

        // After so many, give the player a friendly warning to check their skill/build setup
        private static int iNoAbilitiesAvailableInARow = 0;
        private static DateTime lastRemindedAboutAbilities = DateTime.Today;

        // Prowl API Key
        public static string sProwlAPIKey = "";

        // Android API Key
        public static string sAndroidAPIKey = "";

        // Last had any mob in range, for loot-waiting
        private static DateTime lastHadUnitInSights = DateTime.Today;

        // When we last saw a boss/elite etc.
        private static DateTime lastHadEliteUnitInSights = DateTime.Today;

        // Do we need to reset the debug bar after combat handling?
        private static bool bResetStatusText = false;

        // A list of "useonceonly" tags that have been triggered this xml profile
        public static HashSet<int> hashUseOnceID = new HashSet<int>();
        public static Dictionary<int, int> dictUseOnceID = new Dictionary<int, int>();

        // For the random ID tag
        public static Dictionary<int, int> dictRandomID = new Dictionary<int, int>();

        // Death counts
        public static int iMaxDeathsAllowed = 0;
        public static int iDeathsThisRun = 0;

        // Force a target update after certain interactions
        private static bool bForceTargetUpdate = false;

        // This holds whether or not we want to prioritize a close-target, used when we might be body-blocked by monsters
        private static bool bForceCloseRangeTarget = false;

        // How many times a movement fails because of being "blocked"
        private static int iTimesBlockedMoving = 0;

        // how long to force close-range targets for
        private static int iMillisecondsForceCloseRange = 0;

        // Date time we were last told to stick to close range targets
        private static DateTime lastForcedKeepCloseRange = DateTime.Today;

        // The distance last loop, so we can compare to current distance to work out if we moved
        private static float iLastDistance = 0f;

        // Caching of the current primary target's health, to detect if we AREN'T damaging it for a period of time
        private static double iTargetLastHealth = 0f;

        // This is used so we don't use certain skills until we "top up" our primary resource by enough
        private static double iWaitingReservedAmount = 0d;

        // When did we last clear the temporary blacklist?
        private static DateTime dateSinceBlacklist90Clear = DateTime.Today;

        // And the full blacklist?
        private static DateTime dateSinceBlacklist60Clear = DateTime.Today;

        // And the 15 sec 
        private static DateTime dateSinceBlacklist15Clear = DateTime.Today;

        // Store the date-time when we *FIRST* picked this target, so we can blacklist after X period of time targeting
        private static DateTime dateSincePickedTarget = DateTime.Today;

        // Total main loops so we can update things every XX loops
        private static int iCombatLoops = 0;

        // These values below are set on a per-class basis later on, so don't bother changing them here! These are the old default values
        private static double iEmergencyHealthPotionLimit = 0.46;
        private static double iEmergencyHealthGlobeLimit = 0.6;
        private static int iKiteDistance = 0;
        /// <summary>
        /// Use RActorGUID to blacklist an object/monster for 90 seconds
        /// </summary>
        private static HashSet<int> hashRGUIDIgnoreBlacklist90 = new HashSet<int>();
        /// <summary>
        /// Use RActorGUID to blacklist an object/monster for 60 seconds
        /// </summary>
        private static HashSet<int> hashRGUIDIgnoreBlacklist60 = new HashSet<int>();
        /// <summary>
        /// Use RActorGUID to blacklist an object/monster for 15 seconds
        /// </summary>
        private static HashSet<int> hashRGUIDIgnoreBlacklist15 = new HashSet<int>();

        // This is a blacklist that is cleared within 3 seconds of last attacking a destructible
        private static HashSet<int> hashRGUIDDestructible3SecBlacklist = new HashSet<int>();
        private static DateTime lastDestroyedDestructible = DateTime.Today;
        private static bool bNeedClearDestructibles = false;

        // This is a blacklist that is cleared within 3 seconds of last attacking a destructible
        private static HashSet<int> hashRGuid3SecBlacklist = new HashSet<int>();
        private static DateTime lastTemporaryBlacklist = DateTime.Today;
        private static bool bNeedClearTemporaryBlacklist = false;

        // An ordered list of all of the backtrack locations to navigate through once we finish our current activities
        public static SortedList<int, Vector3> vBacktrackList = new SortedList<int, Vector3>();
        public static int iTotalBacktracks = 0;

        // The number of loops to extend kill range for after a fight to try to maximize kill bonus exp etc.
        private static int iKeepKillRadiusExtendedFor = 0;

        // The number of loops to extend loot range for after a fight to try to stop missing loot
        private static int iKeepLootRadiusExtendedFor = 0;

        // Some avoidance related variables

        // Whether or not we need avoidance this target-search-loop
        private static bool bRequireAvoidance = false;

        // Whether or not there are projectiles to avoid
        private static bool bTravellingAvoidance = false;

        // When we last FOUND a safe spot
        private static DateTime lastFoundSafeSpot = DateTime.Today;
        private static Vector3 vlastSafeSpot = Vector3.Zero;

        // This lets us know if there is a target but it's in avoidance so we can just "stay put" until avoidance goes
        private static bool bStayPutDuringAvoidance = false;

        /// <summary>
        /// This force-prevents avoidance for XX loops incase we get stuck trying to avoid stuff
        /// </summary>
        private static DateTime timeCancelledEmergencyMove = DateTime.Today;
        private static int iMillisecondsCancelledEmergencyMoveFor = 0;

        /// <summary>
        /// Prevent spam-kiting too much - allow fighting between each kite movement
        /// </summary>
        private static DateTime timeCancelledKiteMove = DateTime.Today;
        private static int iMillisecondsCancelledKiteMoveFor = 0;

        // For if we have emergency teleport abilities available right now or not
        private static bool bHasEmergencyTeleportUp = false;

        // How many follower items were ignored, purely for item stat tracking
        private static int iTotalFollowerItemsIgnored = 0;

        // Variable to let us force new target creations immediately after a root
        private static bool bWasRootedLastTick = false;

        // Random variables used during item handling and town-runs
        private static int iItemDelayLoopLimit = 0;
        private static int iCurrentItemLoops = 0;
        private static bool bLoggedAnythingThisStash = false;
        private static bool bUpdatedStashMap = false;
        private static bool bLoggedJunkThisStash = false;
        private static string sValueItemStatString = "";
        private static string sJunkItemStatString = "";
        private static bool bTestingBackpack = false;

        // Stash mapper - it's an array representing every slot in your stash, true or false dictating if the slot is free or not
        private static bool[,] GilesStashSlotBlocked = new bool[7, 30];
        private static bool bOutputItemScores = false;

        // Full Analysis SPAMS like hell. But useful for seeing how the score-calculator is adding/removing points

        // Really this is only for Giles to debug and improve the formula, users likely won't find this useful
        private const bool bFullAnalysis = false;

        // Teehee I typed "Anal" LOLOLOL

        // Variables used to actually hold powers the power-selector has picked to use, for buffing and main power use
        private static GilesPower powerBuff;
        private static GilesPower powerPrime;
        private static SNOPower powerLastSnoPowerUsed = SNOPower.None;

        // Two variables to stop DB from attempting any navigator movement mid-combat/mid-backtrack
        public static bool bDontMoveMeIAmDoingShit = false;
        public static bool bDontSpamOutofCombat = false;
        public static bool bOnlyTarget = false;

        // Target provider and core routine variables
        public static string sTrinityPluginPath = "";
        private static string sTrinityConfigFile = "";
        private static string sTrinityEmailConfigFile = "";
        private static bool bSavingConfig = false;
        public static ActorClass iMyCachedActorClass = ActorClass.Invalid;
        private static bool bAnyChampionsPresent = false;
        private static bool bAnyTreasureGoblinsPresent = false;
        private static bool bAnyMobsInCloseRange = false;
        private static float iCurrentMaxKillRadius = 0f;
        private static float iCurrentMaxLootRadius = 0f;

        // Goblinney things
        private static bool bUseBerserker = false;
        private static bool bWaitingForSpecial = false;
        private static int iTotalNumberGoblins = 0;
        private static DateTime lastGoblinTime = DateTime.Today;

        private static DateTime SweepWindSpam = DateTime.Today; //intell -- inna

        // Variables relating to quick-reference of monsters within sepcific ranges (if anyone has suggestion for similar functionality with reduced CPU use, lemme know, but this is fast atm!)
        private static int[] iElitesWithinRange = new int[] { 0, 0, 0, 0, 0, 0, 0 };
        private static int[] iAnythingWithinRange = new int[] { 0, 0, 0, 0, 0, 0, 0 };
        private static bool bAnyBossesInRange = false;
        private const int RANGE_50 = 0;
        private const int RANGE_40 = 1;
        private const int RANGE_30 = 2;
        private const int RANGE_25 = 3;
        private const int RANGE_20 = 4;
        private const int RANGE_15 = 5;
        private const int RANGE_12 = 6;
        private const int RANGE_6 = 7;
        private static int iWithinRangeLastRend = 0;

        // Unique ID of mob last targetting when using rend
        private static int iACDGUIDLastRend = 0;

        // Unique ID of mob last targetting when using whirlwind
        private static int iACDGUIDLastWhirlwind = 0;
        private static bool bAlreadyMoving = false;
        private static Vector3 vLastMoveToTarget;
        private static float fLastDistanceFromTarget;
        private static DateTime lastMovementCommand = DateTime.Today;

        // Actual combat function variables
        private static bool bMappedPlayerAbilities = false;

        // Contains our apparent *CURRENT* hotbar abilities, cached in a fast hash
        public static HashSet<SNOPower> hashPowerHotbarAbilities = new HashSet<SNOPower>();

        // Contains a hash of our LAST hotbar abilities before we transformed into archon (for quick and safe hotbar restoration)
        public static HashSet<SNOPower> hashCachedPowerHotbarAbilities = new HashSet<SNOPower>();

        // A list and a dictionary for quick buff checking and buff references
        private static List<Buff> listCachedBuffs = new List<Buff>();
        private static Dictionary<int, int> dictCachedBuffs = new Dictionary<int, int>();

        // For "position-shifting" to navigate around obstacle SNO's
        private static Vector3 vShiftedPosition = Vector3.Zero;
        private static DateTime lastShiftedPosition = DateTime.Today;
        private static int iShiftPositionFor = 0;
        private static Vector3 vCurrentDestination;
        private static Vector3 vSideToSideTarget;
        private static DateTime lastChangedZigZag = DateTime.Today;
        private static Vector3 vPositionLastZigZagCheck = Vector3.Zero;
        public static int iCurrentWorldID = -1;
        public static GameDifficulty iCurrentGameDifficulty = GameDifficulty.Invalid;
        private const bool USE_COMBAT_ONLY = false;
        private const bool USE_ANY_TIME = true;
        private const bool SIGNATURE_SPAM = false;
        private const bool USE_SLOWLY = true;

        // Constants and variables used by the item-stats stuff
        private const int QUALITYWHITE = 0;
        private const int QUALITYBLUE = 1;
        private const int QUALITYYELLOW = 2;
        private const int QUALITYORANGE = 3;
        private static readonly string[] sQualityString = new string[4] { "White", "Magic", "Rare", "Legendary" };
        private const int GEMRUBY = 0;
        private const int GEMTOPAZ = 1;
        private const int GEMAMETHYST = 2;
        private const int GEMEMERALD = 3;
        private static readonly string[] sGemString = new string[4] { "Ruby", "Topaz", "Amethyst", "Emerald" };
        private static DateTime ItemStatsLastPostedReport = DateTime.Now;
        private static DateTime ItemStatsWhenStartedBot = DateTime.Now;
        private static bool bMaintainStatTracking = false;

        // Store items already logged by item-stats, to make sure no stats get doubled up by accident
        private static HashSet<int> _hashsetItemStatsLookedAt = new HashSet<int>();
        private static HashSet<int> _hashsetItemPicksLookedAt = new HashSet<int>();
        private static HashSet<int> _hashsetItemFollowersIgnored = new HashSet<int>();

        // This dictionary stores attempted stash counts on items, to help detect any stash stucks on the same item etc.
        private static Dictionary<int, int> _dictItemStashAttempted = new Dictionary<int, int>();

        // These objects are instances of my stats class above, holding identical types of data for two different things - one holds item DROP stats, one holds item PICKUP stats
        private static GilesItemStats ItemsDroppedStats = new GilesItemStats(0, new double[4], new double[64], new double[4, 64], 0, new double[64], 0, new double[4], new double[64], new double[4, 64], 0);
        private static GilesItemStats ItemsPickedStats = new GilesItemStats(0, new double[4], new double[64], new double[4, 64], 0, new double[64], 0, new double[4], new double[64], new double[4, 64], 0);
        private static HashSet<GilesCachedACDItem> hashGilesCachedKeepItems = new HashSet<GilesCachedACDItem>();
        private static HashSet<GilesCachedACDItem> hashGilesCachedSalvageItems = new HashSet<GilesCachedACDItem>();
        private static HashSet<GilesCachedACDItem> hashGilesCachedSellItems = new HashSet<GilesCachedACDItem>();

        // Whether to try forcing a vendor-run for custom reasons
        public static bool bGilesForcedVendoring = false;
        public static bool bWantToTownRun = false;
        private static bool bLastTownRunCheckResult = false;

        // Whether salvage/sell run should go to a middle-waypoint first to help prevent stucks
        private static bool bGoToSafetyPointFirst = false;
        private static bool bGoToSafetyPointSecond = false;
        private static bool bReachedSafety = false;

        // DateTime check to prevent inventory-check spam when looking for repairs being needed
        private static DateTime timeLastAttemptedTownRun = DateTime.Now;
        private static bool bCurrentlyMoving = false;
        private static bool bReachedDestination = false;
        private static bool bNeedsEquipmentRepairs = false;
        private static float iLowestDurabilityFound = -1;

        /*
         * From RefreshDiaObject
         *
         * Bunch of temporary variables that get used when creating the current object/target list - this was just a nicer way for me to handle it code wise at first
         * Even if it looks a bit messy and probably should have just used it's own object instance of the cache-class instead! :D
         * c_ variables are all used in the caching mechanism
         */
        private static Vector3 c_vPosition = Vector3.Zero;
        private static GilesObjectType c_ObjectType = GilesObjectType.Unknown;
        private static double c_dWeight = 0d;
        private static double c_unit_dHitPoints = 0d;
        private static float c_fCentreDistance = 0f;
        private static float c_fRadiusDistance = 0f;
        private static float c_fRadius = 0f;
        private static float c_fZDiff = 0f;
        private static string c_sName = "";
        private static string c_IgnoreReason = "";
        private static string c_IgnoreSubStep = "";
        private static int c_iACDGUID = 0;
        private static int c_iRActorGuid = 0;
        private static int c_iDynamicID = 0;
        private static int c_iBalanceID = 0;
        private static int c_iActorSNO = 0;
        private static int c_item_iLevel = 0;
        private static int c_item_iGoldStackSize = 0;
        private static bool c_item_bOneHanded = false;
        private static ItemQuality c_item_tQuality = ItemQuality.Invalid;
        private static ItemType c_item_tDBItemType = ItemType.Unknown;
        private static FollowerType c_item_tFollowerType = FollowerType.None;
        private static GilesItemType c_item_GilesItemType = GilesItemType.Unknown;
        private static MonsterSize c_unit_MonsterSize = MonsterSize.Unknown;
        private static DiaObject c_diaObject = null;
        private static ACD c_CommonData = null;
        private static bool c_unit_bIsElite = false;
        private static bool c_unit_bIsRare = false;
        private static bool c_unit_bIsUnique = false;
        private static bool c_unit_bIsMinion = false;
        private static bool c_unit_bIsTreasureGoblin = false;
        private static bool c_bIsEliteRareUnique = false;
        private static bool c_unit_bIsBoss = false;
        private static bool c_unit_bIsAttackable = false;
        private static bool c_bForceLeapAgainst = false;
        private static bool c_bIsObstacle = false;

        /// <summary>
        /// Used for trimming off numbers from object names in RefreshDiaObject
        /// </summary>
        private static Regex nameNumberTrimRegex = new Regex(@"-\d+$");

        /* Special Sauce Dictionaries for SPEEEEEEED
         * This set of dictionaries are used for huge performance increases throughout, and a minimization of DB mis-read/null exception errors
         * Uses a little more ram - but for a massive CPU gain. And ram is cheap, CPU is not!
         */

        // The following 2 variables are used to clear the dictionaries out - clearing one dictionary out per maximum every 2 seconds, working through in sequential order
        private static DateTime lastClearedCacheDictionary = DateTime.Today;
        private static int iLastClearedCacheDictionary = 0;

        // Extra height added on for location-based-attacks on targets - may be needed for beam-spells etc. for wizards? (eg add 2 foot height from their feet)
        private const float iExtraHeight = 2f;
        private static Button btnPauseBot, btnTownRun;

        // On death, clear the timers for all abilities
        private static DateTime lastDied = DateTime.Today;
        private static int iTotalDeaths = 0;

        // When did we last send a move-power command?
        private static DateTime lastSentMovePower = DateTime.Today;

        // From RefreshDiaObjects()
        private static Vector3 vSafePointNear;
        private static Vector3 vKitePointAvoid;
        private static int iCurrentTargetRactorGUID;
        private static int iUnitsSurrounding;
        private static double iHighestWeightFound;
        private static List<GilesObject> listGilesObjectCache;
        private static HashSet<int> hashDoneThisRactor;
        private static bool bNeedToKite = false;
        private static bool bShouldTryKiting = false;

        // Email stuff
        public static string SmtpServer = "smtp.gmail.com";
        public static StringBuilder EmailMessage = new StringBuilder();

        // Email Settings
        public static string sEmailAddress = "";
        public static string sEmailPassword = "";
        public static string sBotName = ZetaDia.Service.CurrentHero.BattleTagName;

        // HandleTarget
        private static bool bForceNewMovement = false;

        // Store player current position
        Vector3 vMyCurrentPosition = Vector3.Zero;

        // For path finding
        private static ISearchAreaProvider gp;
        private static PathFinder pf;

        // Behaviors: How close we need to get to the target before we consider it "reached"
        private static float fRangeRequired = 1f;
        private static float fDistanceReduction = 0f;
    }
}
