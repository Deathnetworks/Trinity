using Zeta.Common.Plugins;
namespace GilesTrinity
{
    // Current plugin settings
    public class GilesSettings
    {
        public bool bUseGilesFilters { get; set; }
        public int iMinimumGoldStack { get; set; }
        public double iNeedPointsToKeepJewelry { get; set; }
        public double iNeedPointsToKeepArmor { get; set; }
        public double iNeedPointsToKeepWeapon { get; set; }
        public int iFilterPotions { get; set; }
        public int iFilterLegendary { get; set; }
        public int iFilterBlueWeapons { get; set; }
        public int iFilterYellowWeapons { get; set; }
        public int iFilterBlueArmor { get; set; }
        public int iFilterYellowArmor { get; set; }
        public int iFilterBlueJewelry { get; set; }
        public int iFilterYellowJewelry { get; set; }
        public int iFilterGems { get; set; }
        public int iFilterMisc { get; set; }
        public int iFilterPotionLevel { get; set; }
        public bool bGemsEmerald { get; set; }
        public bool bGemsAmethyst { get; set; }
        public bool bGemsTopaz { get; set; }
        public bool bGemsRuby { get; set; }
        public bool bSalvageJunk { get; set; }
        public bool bPickupCraftTomes { get; set; }
        public bool bPickupPlans { get; set; }
        public bool bPickupFollower { get; set; }
        // Combat replacer config settings
        public bool bEnableBacktracking { get; set; }
        public bool bEnableAvoidance { get; set; }
        public bool bEnableGlobes { get; set; }
        public bool bEnableCriticalMass { get; set; }
        public int iTreasureGoblinPriority { get; set; }
        public double iMonsterKillRange { get; set; }
        public bool bOutOfCombatMovementPowers { get; set; }
        public bool bExtendedKillRange { get; set; }
        public bool bSelectiveWhirlwind { get; set; }
        public bool bWaitForWrath { get; set; }
        public bool bGoblinWrath { get; set; }
        public bool bFuryDumpWrath { get; set; }
        public bool bFuryDumpAlways { get; set; }
        public int iKillLootDelay { get; set; }
        public int iDHVaultMovementDelay { get; set; }
        public bool bMonkInnaSet { get; set; }
        public bool bWaitForArchon { get; set; }
        public bool bKiteOnlyArchon { get; set; }
        public bool bWrath90Seconds { get; set; }
        // World object handler config settings
        public bool bIgnoreAllShrines { get; set; }
        public bool bIgnoreCorpses { get; set; }
        public double iContainerOpenRange { get; set; }
        public double iDestructibleAttackRange { get; set; }
        // Performance stuff
        public bool bEnableTPS { get; set; }
        public double iTPSAmount { get; set; }
        public bool bDebugInfo { get; set; }
        // Enable Prowl
        public bool bEnableProwl { get; set; }
        public bool bEnableAndroid { get; set; }
        public bool bEnableLegendaryNotifyScore { get; set; }
        public double iNeedPointsToNotifyJewelry { get; set; }
        public double iNeedPointsToNotifyArmor { get; set; }
        public double iNeedPointsToNotifyWeapon { get; set; }
        // Enable Email
        public bool bEnableEmail { get; set; }
        // Log stuck points
        public bool bLogStucks { get; set; }
        // Enable unstucker
        public bool bEnableUnstucker { get; set; }
        public bool bEnableProfileReloading { get; set; }
        // Pot & Globe usage
        public double dEmergencyHealthPotionBarb { get; set; }
        public double dEmergencyHealthGlobeBarb { get; set; }
        public double dEmergencyHealthPotionMonk { get; set; }
        public double dEmergencyHealthGlobeMonk { get; set; }
        public double dEmergencyHealthPotionWiz { get; set; }
        public double dEmergencyHealthGlobeWiz { get; set; }
        public double dEmergencyHealthPotionWitch { get; set; }
        public double dEmergencyHealthGlobeWitch { get; set; }
        public double dEmergencyHealthPotionDemon { get; set; }
        public double dEmergencyHealthGlobeDemon { get; set; }
        public int iKiteDistanceBarb { get; set; }
        public int iKiteDistanceWiz { get; set; }
        public int iKiteDistanceWitch { get; set; }
        public int iKiteDistanceDemon { get; set; }
        // Defaults For All Settings can be set here
        public GilesSettings(
            bool usegilesfilters = true,
            int mingoldstack = 1,
            double pointsj = 15000,
            double pointsa = 16000,
            double pointsw = 70000,
            int filterpot = 3,
            int filterbluew = 1,
            int filteryelloww = 1,
            int filterbluea = 1,
            int filteryellowa = 1,
            int filterbluej = 1,
            int filteryellowj = 1,
            int filtergems = 1,
            int filtermisc = 1,
            int filterpotlevel = 1,
            bool emerald = true,
            bool amethyst = true,
            bool topaz = true,
            bool ruby = true,
            bool salvage = false,
            bool pickupcrafttomes = true,
            bool pickupplans = true,
            bool pickupfollower = true,
            bool enablebacktracking = false,
            bool enableavoidance = true,
            bool enableglobes = true,
            bool enablecriticalmass = false,
            int treasuregoblins = 2,
            double monsterkillrange = 40,
            bool outofcombatmovement = true,
            bool ignoreshrines = false,
            bool ignorecorpses = true,
            double containeropen = 15,
            double destructibleattack = 12,
            bool enabletps = false,
            double tpsamount = 10,
            bool logstucks = false,
            bool enableunstucker = true,
            bool extendedrange = true,
            bool selectiveww = false,
            bool debuginfo = false,
            double healthpot0 = 0.42,
            double healthpot1 = 0.46,
            double healthpot2 = 0.7,
            double healthpot3 = 0.7,
            double healthpot4 = 0.7,
            double healthglobe0 = 0.55,
            double healthglobe1 = 0.6,
            double healthglobe2 = 0.8,
            double healthglobe3 = 0.8,
            double healthglobe4 = 0.8,
            bool enableprowl = false,
            bool enableandroid = false,
            double pointsnotifyj = 28000,
            double pointsnotifya = 30000,
            double pointsnotifyw = 100000,
            int killlootdelay = 800,
            int vaultdelay = 400,
            bool waitforwrath = true,
            bool goblinwrath = true,
            bool furydumpwrath = true,
            bool furydumpalways = false,
            int filterlegendary = 1,
            bool profilereload = true,
            bool monkinna = false,
            int kitebarb = 0,
            int kitewiz = 10,
            int kitewitch = 10,
            int kitedemon = 10,
            bool waitarchon = true,
            bool kitearchon = false,
            bool wrath90 = false,
            bool enableEmail = false,
            bool bEnableLegendaryNotifyScore = false)
        {
            bUseGilesFilters = usegilesfilters;
            iMinimumGoldStack = mingoldstack;
            iNeedPointsToKeepJewelry = pointsj;
            iNeedPointsToKeepArmor = pointsa;
            iNeedPointsToKeepWeapon = pointsw;
            iFilterPotions = filterpot;
            iFilterLegendary = filterlegendary;
            iFilterBlueWeapons = filterbluew;
            iFilterYellowWeapons = filteryelloww;
            iFilterBlueArmor = filterbluea;
            iFilterYellowArmor = filteryellowa;
            iFilterBlueJewelry = filterbluej;
            iFilterYellowJewelry = filteryellowj;
            iFilterGems = filtergems;
            iFilterMisc = filtermisc;
            iFilterPotionLevel = filterpotlevel;
            bGemsEmerald = emerald;
            bGemsAmethyst = amethyst;
            bGemsTopaz = topaz;
            bGemsRuby = ruby;
            bSalvageJunk = salvage;
            bPickupCraftTomes = pickupcrafttomes;
            bPickupPlans = pickupplans;
            bPickupFollower = pickupfollower;
            // Combat replacer config settings
            bEnableBacktracking = enablebacktracking;
            bEnableAvoidance = enableavoidance;
            bEnableGlobes = enableglobes;
            bEnableCriticalMass = enablecriticalmass;
            iTreasureGoblinPriority = treasuregoblins;
            iMonsterKillRange = monsterkillrange;
            bOutOfCombatMovementPowers = outofcombatmovement;
            bExtendedKillRange = extendedrange;
            bSelectiveWhirlwind = selectiveww;
            bWaitForWrath = waitforwrath;
            bGoblinWrath = goblinwrath;
            bFuryDumpWrath = furydumpwrath;
            bFuryDumpAlways = furydumpalways;
            iKillLootDelay = killlootdelay;
            iDHVaultMovementDelay = vaultdelay;
            bMonkInnaSet = monkinna;
            bWrath90Seconds = wrath90;
            // World object handler config settings
            bIgnoreAllShrines = ignoreshrines;
            bIgnoreCorpses = ignorecorpses;
            iContainerOpenRange = containeropen;
            iDestructibleAttackRange = destructibleattack;
            // Advanced stuff
            bEnableTPS = enabletps;
            iTPSAmount = tpsamount;
            bLogStucks = logstucks;
            bEnableUnstucker = enableunstucker;
            bEnableProfileReloading = profilereload;
            bDebugInfo = debuginfo;
            // Enable prowl
            bEnableProwl = enableprowl;
            bEnableAndroid = enableandroid;
            this.bEnableLegendaryNotifyScore = bEnableLegendaryNotifyScore;
            iNeedPointsToNotifyJewelry = pointsnotifyj;
            iNeedPointsToNotifyArmor = pointsnotifya;
            iNeedPointsToNotifyWeapon = pointsnotifyw;
            // Enable email
            this.bEnableEmail = enableEmail;
            // Globes and pots
            dEmergencyHealthPotionBarb = healthpot0;
            dEmergencyHealthPotionMonk = healthpot1;
            dEmergencyHealthPotionWiz = healthpot2;
            dEmergencyHealthPotionWitch = healthpot3;
            dEmergencyHealthPotionDemon = healthpot4;
            dEmergencyHealthGlobeBarb = healthglobe0;
            dEmergencyHealthGlobeMonk = healthglobe1;
            dEmergencyHealthGlobeWiz = healthglobe2;
            dEmergencyHealthGlobeWitch = healthglobe3;
            dEmergencyHealthGlobeDemon = healthglobe4;
            iKiteDistanceBarb = kitebarb;
            iKiteDistanceWiz = kitewiz;
            iKiteDistanceWitch = kitewitch;
            iKiteDistanceDemon = kitedemon;
            bWaitForArchon = waitarchon;
            bKiteOnlyArchon = kitearchon;
        }
    }
}
