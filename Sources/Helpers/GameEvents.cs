using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Trinity.DbProvider;
using Trinity.ItemRules;
using Trinity.Technicals;
using Trinity.XmlTags;
using Zeta;
using Zeta.Common;
using Zeta.CommonBot;
using Zeta.CommonBot.Settings;
using Zeta.Internals;
using Zeta.Internals.Actors;
using Zeta.Navigation;

namespace Trinity
{
    public partial class Trinity
    {
        // How many total leave games, for stat-tracking?
        public static int TotalGamesJoined = 0;
        // How many total leave games, for stat-tracking?
        public static int TotalLeaveGames = 0;
        public static int TotalProfileRecycles = 0;

        /// <summary>
        /// This is wired up by Plugin.OnEnabled, and called when the bot is started
        /// </summary>
        /// <param name="bot"></param>
        private static void TrinityBotStart(IBot bot)
        {
            Logger.Log("Bot Starting");
            DateTime timeBotStart = DateTime.Now;

            V.ValidateLoad();

            // Recording of all the XML's in use this run
            try
            {
                string sThisProfile = GlobalSettings.Instance.LastProfile;
                if (sThisProfile != CurrentProfile)
                {
                    ProfileHistory.Add(sThisProfile);
                    CurrentProfile = sThisProfile;
                    if (FirstProfile == "")
                        FirstProfile = sThisProfile;
                }
            }
            catch
            {
            }

            HasMappedPlayerAbilities = false;
            if (!bMaintainStatTracking)
            {
                ItemStatsWhenStartedBot = DateTime.Now;
                ItemStatsLastPostedReport = DateTime.Now;
                bMaintainStatTracking = true;
            }
            else
            {
                Logger.Log(TrinityLogLevel.Normal, LogCategory.UserInformation,
                    "Note: Maintaining item stats from previous run. To reset stats fully, please restart DB.");
            }

            BeginInvoke(new Action(() => UsedProfileManager.RefreshProfileBlacklists()));

            ReplaceTreeHooks();

            PlayerMover.TimeLastRecordedPosition = DateTime.Now;
            PlayerMover.timeLastRestartedGame = DateTime.Now;
            GoldInactivity.ResetCheckGold();

            if (CharacterSettings.Instance.KillRadius < 20)
            {
                Logger.Log(TrinityLogLevel.Normal, LogCategory.UserInformation,
                    "WARNING: Low Kill Radius detected, currently set to: {0} (you can change this through Demonbuddy bot settings)",
                    CharacterSettings.Instance.KillRadius);
            }

            if (CharacterSettings.Instance.LootRadius < 50)
            {
                Logger.Log(TrinityLogLevel.Normal, LogCategory.UserInformation,
                    "WARNING: Low Gold Loot Radius detected, currently set to: {0} (you can change this through Demonbuddy bot settings)",
                    CharacterSettings.Instance.LootRadius);
            }

            BeginInvoke(new Action(() =>
                {
                    if (StashRule == null)
                        StashRule = new Interpreter();

                    StashRule.readConfiguration();
                }
            ));

            Navigator.SearchGridProvider.Update();

            Logger.LogDebug("Trinity BotStart took {0}ms", DateTime.Now.Subtract(timeBotStart).TotalMilliseconds);
        }

        private void GameEvents_OnGameChanged(object sender, EventArgs e)
        {
            ClearCachesOnGameChange(sender, e);

            // reload the profile juuuuuuuuuuuust in case Demonbuddy missed it... which it is known to do on disconnects
            string currentProfilePath = ProfileManager.CurrentProfile.Path;
            ProfileManager.Load(currentProfilePath);
            Navigator.SearchGridProvider.Update();
            ResetEverythingNewGame();
        }

        // When the bot stops, output a final item-stats report so it is as up-to-date as can be
        private void TrinityBotStop(IBot bot)
        {
            // Issue final reports
            OutputReport();
            vBacktrackList = new SortedList<int, Vector3>();
            iTotalBacktracks = 0;
            PlayerMover.TotalAntiStuckAttempts = 1;
            PlayerMover.vSafeMovementLocation = Vector3.Zero;
            PlayerMover.vOldPosition = Vector3.Zero;
            PlayerMover.iTimesReachedStuckPoint = 0;
            PlayerMover.TimeLastRecordedPosition = DateTime.Today;
            PlayerMover.LastGeneratedStuckPosition = DateTime.Today;
            TrinityUseOnce.UseOnceIDs = new HashSet<int>();
            TrinityUseOnce.UseOnceCounter = new Dictionary<int, int>();
            dictRandomID = new Dictionary<int, int>();
            iMaxDeathsAllowed = 0;
            iDeathsThisRun = 0;
        }

        private void TrinityOnDeath(object src, EventArgs mea)
        {
            if (DateTime.Now.Subtract(LastDeathTime).TotalSeconds > 10)
            {
                LastDeathTime = DateTime.Now;
                iTotalDeaths++;
                iDeathsThisRun++;
                PlayerInfoCache.RefreshHotbar();
                AbilityLastUsedCache = new Dictionary<SNOPower, DateTime>(DataDictionary.LastUseAbilityTimeDefaults);
                vBacktrackList = new SortedList<int, Vector3>();
                iTotalBacktracks = 0;
                PlayerMover.TotalAntiStuckAttempts = 1;
                PlayerMover.vSafeMovementLocation = Vector3.Zero;

                // Reset pre-townrun position if we die
                TownRun.PreTownRunPosition = Vector3.Zero;
                TownRun.PreTownRunWorldId = -1;

                // Does Trinity need to handle deaths?
                if (iMaxDeathsAllowed > 0)
                {
                    if (iDeathsThisRun >= iMaxDeathsAllowed)
                    {
                        Logger.Log(TrinityLogLevel.Normal, LogCategory.UserInformation,
                            "You have died too many times. Now restarting the game.");
                        string sUseProfile = FirstProfile;
                        ProfileManager.Load(!string.IsNullOrEmpty(sUseProfile)
                            ? sUseProfile
                            : GlobalSettings.Instance.LastProfile);
                        Thread.Sleep(1000);
                        ResetEverythingNewGame();
                        ZetaDia.Service.Party.LeaveGame();
                        Thread.Sleep(10000);
                    }
                    else
                    {
                        Logger.Log(TrinityLogLevel.Normal, LogCategory.UserInformation,
                            "I'm sorry, but I seem to have let you die :( Now restarting the current profile.");
                        ProfileManager.Load(ProfileManager.CurrentProfile.Path);
                        Thread.Sleep(2000);
                    }
                }
            }
        }


        // Each time we join & leave a game, might as well clear the hashset of looked-at dropped items - just to keep it smaller
        private static void TrinityOnJoinGame(object src, EventArgs mea)
        {
            TotalGamesJoined++;
            ResetEverythingNewGame();
        }

        // Each time we join & leave a game, might as well clear the hashset of looked-at dropped items - just to keep it smaller
        private static void TrinityOnLeaveGame(object src, EventArgs mea)
        {
            TotalLeaveGames++;
            ResetEverythingNewGame();
        }

        public static void ResetEverythingNewGame()
        {
            // In-thread stuff
            PlayerInfoCache.RefreshHotbar();
            V.ValidateLoad();

            // Out of thread Async stuff
            BeginInvoke(new Action(() =>
                {
                    
                    Logger.Log("New Game - resetting everything");

                    Trinity.IsReadyToTownRun = false;
                    Trinity.ForceVendorRunASAP = false;
                    TownRun.TownRunCheckTimer.Reset();
                    TownRun.SendEmailNotification();
                    TownRun.PreTownRunPosition = Vector3.Zero;
                    TownRun.PreTownRunWorldId = -1;
                    TownRun.WasVendoring = false;

                    AbilityLastUsedCache.Clear();

                    TrinityUseOnce.UseOnceIDs = new HashSet<int>();
                    TrinityUseOnce.UseOnceCounter = new Dictionary<int, int>();
                    iMaxDeathsAllowed = 0;
                    iDeathsThisRun = 0;
                    LastDeathTime = DateTime.Now;
                    _hashsetItemStatsLookedAt = new HashSet<string>();
                    _hashsetItemPicksLookedAt = new HashSet<string>();
                    _hashsetItemFollowersIgnored = new HashSet<string>();

                    hashRGUIDBlacklist60 = new HashSet<int>();
                    hashRGUIDBlacklist90 = new HashSet<int>();
                    hashRGUIDBlacklist15 = new HashSet<int>();
                    vBacktrackList = new SortedList<int, Vector3>();
                    iTotalBacktracks = 0;
                    HasMappedPlayerAbilities = false;
                    PlayerMover.TotalAntiStuckAttempts = 1;
                    PlayerMover.vSafeMovementLocation = Vector3.Zero;
                    PlayerMover.vOldPosition = Vector3.Zero;
                    PlayerMover.iTimesReachedStuckPoint = 0;
                    PlayerMover.TimeLastRecordedPosition = DateTime.Today;
                    PlayerMover.LastGeneratedStuckPosition = DateTime.Today;
                    PlayerMover.iTimesReachedMaxUnstucks = 0;
                    PlayerMover.iCancelUnstuckerForSeconds = 0;
                    PlayerMover._lastCancelledUnstucker = DateTime.Today;
                    NavHelper.UsedStuckSpots = new List<GridPoint>();

                    // Reset all the caches
                    objectTypeCache = new Dictionary<int, GObjectType>();
                    unitMonsterAffixCache = new Dictionary<int, MonsterAffixes>();
                    unitMaxHealthCache = new Dictionary<int, double>();
                    currentHealthCache = new Dictionary<int, double>();
                    currentHealthCheckTimeCache = new Dictionary<int, int>();
                    unitBurrowedCache = new Dictionary<int, bool>();
                    actorSNOCache = new Dictionary<int, int>();
                    ACDGUIDCache = new Dictionary<int, int>();
                    nameCache = new Dictionary<int, string>();
                    gameBalanceIDCache = new Dictionary<int, int>();
                    dynamicIDCache = new Dictionary<int, int>();
                    positionCache = new Dictionary<int, Vector3>();
                    goldAmountCache = new Dictionary<int, int>();
                    itemQualityCache = new Dictionary<int, ItemQuality>();
                    pickupItemCache = new Dictionary<int, bool>();
                    summonedByIdCache = new Dictionary<int, int>();
                    interactAttemptsCache = new Dictionary<int, int>();
                    ProfileHistory = new List<string>();
                    CurrentProfile = "";
                    FirstProfile = "";

                    AbilityLastUsedCache = new Dictionary<SNOPower, DateTime>(DataDictionary.LastUseAbilityTimeDefaults);

                    GoldInactivity.ResetCheckGold();

                    TrinityLoadOnce.UsedProfiles = new List<string>();

                    GenericCache.ClearCache();
                    GenericBlacklist.ClearBlacklist();
                }));
        }
    }
}