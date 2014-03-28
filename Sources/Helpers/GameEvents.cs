using System;
using System.Collections.Generic;
using System.Threading;
using Trinity.Combat.Abilities;
using Trinity.DbProvider;
using Trinity.ItemRules;
using Trinity.Technicals;
using Trinity.XmlTags;
using Zeta.Bot;
using Zeta.Bot.Navigation;
using Zeta.Bot.Settings;
using Zeta.Common;
using Zeta.Game;
using Zeta.Game.Internals.Actors;
using Logger = Trinity.Technicals.Logger;

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
            DateTime timeBotStart = DateTime.UtcNow;

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
                ItemStatsWhenStartedBot = DateTime.UtcNow;
                ItemStatsLastPostedReport = DateTime.UtcNow;
                bMaintainStatTracking = true;
            }
            else
            {
                Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation,
                    "Note: Maintaining item stats from previous run. To reset stats fully, please restart DB.");
            }

            BeginInvoke(new Action(() => UsedProfileManager.RefreshProfileBlacklists()));

            ReplaceTreeHooks();

            PlayerMover.TimeLastRecordedPosition = DateTime.UtcNow;
            PlayerMover.LastRestartedGame = DateTime.UtcNow;
            GoldInactivity.ResetCheckGold();

            if (CharacterSettings.Instance.KillRadius < 20)
            {
                Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation,
                    "WARNING: Low Kill Radius detected, currently set to: {0} (you can change this through Demonbuddy bot settings)",
                    CharacterSettings.Instance.KillRadius);
            }

            if (CharacterSettings.Instance.LootRadius < 50)
            {
                Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation,
                    "WARNING: Low Gold Loot Radius detected, currently set to: {0} (you can change this through Demonbuddy bot settings)",
                    CharacterSettings.Instance.LootRadius);
            }

            if (Settings.Loot.ItemFilterMode == Config.Loot.ItemFilterMode.TrinityWithItemRules)
            {
                BeginInvoke(new Action(() =>
                    {
                        try
                        {
                            if (StashRule == null)
                                StashRule = new Interpreter();
                        }
                        catch (Exception ex)
                        {
                            Logger.LogError("Error configuring ItemRules Interpreter: " + ex.ToString());
                        }
                    }
                ));
            }

            Logger.LogDebug("Trinity BotStart took {0:0}ms", DateTime.UtcNow.Subtract(timeBotStart).TotalMilliseconds);
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
            PlayerMover.LastPosition = Vector3.Zero;
            PlayerMover.TimesReachedStuckPoint = 0;
            PlayerMover.TimeLastRecordedPosition = DateTime.MinValue;
            PlayerMover.LastGeneratedStuckPosition = DateTime.MinValue;
            TrinityUseOnce.UseOnceIDs = new HashSet<int>();
            TrinityUseOnce.UseOnceCounter = new Dictionary<int, int>();
            dictRandomID = new Dictionary<int, int>();
            iMaxDeathsAllowed = 0;
            iDeathsThisRun = 0;
        }

        private void TrinityOnDeath(object src, EventArgs mea)
        {
            if (DateTime.UtcNow.Subtract(LastDeathTime).TotalSeconds > 10)
            {
                LastDeathTime = DateTime.UtcNow;
                iTotalDeaths++;
                iDeathsThisRun++;
                CacheData.AbilityLastUsed = new Dictionary<SNOPower, DateTime>(DataDictionary.LastUseAbilityTimeDefaults);
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
                        Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation,
                            "You have died too many times. Now restarting the game.");
                        string sUseProfile = FirstProfile;
                        ProfileManager.Load(!string.IsNullOrEmpty(sUseProfile)
                            ? sUseProfile
                            : GlobalSettings.Instance.LastProfile);
                        Thread.Sleep(1000);
                        ResetEverythingNewGame();
                        ZetaDia.Service.Party.LeaveGame(true);
                        Thread.Sleep(10000);
                    }
                    else
                    {
                        Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation,
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

                    CacheData.AbilityLastUsed.Clear();

                    TrinityUseOnce.UseOnceIDs = new HashSet<int>();
                    TrinityUseOnce.UseOnceCounter = new Dictionary<int, int>();
                    iMaxDeathsAllowed = 0;
                    iDeathsThisRun = 0;
                    LastDeathTime = DateTime.UtcNow;
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
                    PlayerMover.LastPosition = Vector3.Zero;
                    PlayerMover.TimesReachedStuckPoint = 0;
                    PlayerMover.TimeLastRecordedPosition = DateTime.MinValue;
                    PlayerMover.LastGeneratedStuckPosition = DateTime.MinValue;
                    PlayerMover.TimesReachedMaxUnstucks = 0;
                    PlayerMover.CancelUnstuckerForSeconds = 0;
                    PlayerMover.LastCancelledUnstucker = DateTime.MinValue;
                    NavHelper.UsedStuckSpots = new List<GridPoint>();

                    // Reset all the caches
                    CacheData.ObjectType = new Dictionary<int, GObjectType>();
                    CacheData.UnitMonsterAffix = new Dictionary<int, MonsterAffixes>();
                    CacheData.UnitMaxHealth = new Dictionary<int, double>();
                    CacheData.CurrentUnitHealth = new Dictionary<int, double>();
                    CacheData.LastCheckedUnitHealth = new Dictionary<int, int>();
                    CacheData.UnitIsBurrowed = new Dictionary<int, bool>();
                    CacheData.ActorSNO = new Dictionary<int, int>();
                    CacheData.AcdGuid = new Dictionary<int, int>();
                    CacheData.Name = new Dictionary<int, string>();
                    CacheData.GameBalanceID = new Dictionary<int, int>();
                    CacheData.DynamicID = new Dictionary<int, int>();
                    CacheData.Position = new Dictionary<int, Vector3>();
                    CacheData.GoldStack = new Dictionary<int, int>();
                    CacheData.ItemQuality = new Dictionary<int, ItemQuality>();
                    CacheData.PickupItem = new Dictionary<int, bool>();
                    CacheData.SummonedByACDId = new Dictionary<int, int>();
                    CacheData.InteractAttempts = new Dictionary<int, int>();
                    CacheData.ItemLinkQuality = new Dictionary<int, ItemQuality>();
                    CacheData.IsSummoner = new Dictionary<int, bool>();
                    ProfileHistory = new List<string>();
                    CurrentProfile = "";
                    FirstProfile = "";

                    CacheData.AbilityLastUsed = new Dictionary<SNOPower, DateTime>(DataDictionary.LastUseAbilityTimeDefaults);

                    GoldInactivity.ResetCheckGold();

                    TrinityLoadOnce.UsedProfiles = new List<string>();
                    CombatBase.IsQuestingMode = false;

                    GenericCache.ClearCache();
                    GenericBlacklist.ClearBlacklist();
                }));
        }
    }
}