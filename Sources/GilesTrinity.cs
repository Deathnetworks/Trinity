using GilesTrinity.Cache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using GilesTrinity.DbProvider;
using GilesTrinity.Technicals;
using Zeta;
using Zeta.Common;
using Zeta.Common.Plugins;
using Zeta.CommonBot;
using Zeta.Internals.Actors;
using Zeta.Navigation;
using Zeta.Pathfinding;
namespace GilesTrinity
{
    public partial class GilesTrinity : IPlugin
    {
        // How many total leave games, for stat-tracking?
        public static int iTotalJoinGames = 0;
        // How many total leave games, for stat-tracking?
        public static int TotalLeaveGames = 0;
        public static int TotalProfileRecycles = 0;

        // Force town-run button
        private static void buttonTownRun_Click(object sender, RoutedEventArgs e)
        {
            if (!BotMain.IsRunning || !ZetaDia.IsInGame || ZetaDia.IsLoadingWorld)
            {
                DbHelper.Log(TrinityLogLevel.Normal, LogCategory.UserInformation, "You can only force a town run while DemonBuddy is started and running!");
                return;
            }
            ForceVendorRunASAP = true;
            DbHelper.Log(TrinityLogLevel.Normal, LogCategory.UserInformation, "Town-run request received, will town-run at next possible moment.");
        }
        // Pause Button
        private static void buttonPause_Click(object sender, RoutedEventArgs e)
        {
            if (bMainBotPaused)
            {
                btnPauseBot.Content = "Pause Bot";
                bMainBotPaused = false;
                HasMappedPlayerAbilities = false;
                IsAlreadyMoving = false;
                lastMovementCommand = DateTime.Today;
            }
            else
            {
                BotMain.PauseWhile(BotIsPaused);
                btnPauseBot.Content = "Unpause Bot";
                bMainBotPaused = true;
            }

            GoldInactivity.ResetCheckGold();

        }

        private static bool BotIsPaused()
        {
            return bMainBotPaused;
        }
        private void TrinityOnDeath(object src, EventArgs mea)
        {
            if (DateTime.Now.Subtract(lastDied).TotalSeconds > 10)
            {
                lastDied = DateTime.Now;
                iTotalDeaths++;
                iDeathsThisRun++;
                dictAbilityLastUse = new Dictionary<SNOPower, DateTime>(dictAbilityLastUseDefaults);
                vBacktrackList = new SortedList<int, Vector3>();
                iTotalBacktracks = 0;
                PlayerMover.iTotalAntiStuckAttempts = 1;
                PlayerMover.vSafeMovementLocation = Vector3.Zero;
                // Does Trinity need to handle deaths?
                if (iMaxDeathsAllowed > 0)
                {
                    if (iDeathsThisRun >= iMaxDeathsAllowed)
                    {
                        DbHelper.Log(TrinityLogLevel.Normal, LogCategory.UserInformation, "You have died too many times. Now restarting the game.");
                        string sUseProfile = GilesTrinity.sFirstProfileSeen;
                        ProfileManager.Load(!string.IsNullOrEmpty(sUseProfile)
                                                ? sUseProfile
                                                : Zeta.CommonBot.Settings.GlobalSettings.Instance.LastProfile);
                        Thread.Sleep(1000);
                        ResetEverythingNewGame();
                        ZetaDia.Service.Party.LeaveGame();
                        Thread.Sleep(10000);
                    }
                    else
                    {
                        DbHelper.Log(TrinityLogLevel.Normal, LogCategory.UserInformation, "I'm sorry, but I seem to have let you die :( Now restarting the current profile.");
                        ProfileManager.Load(Zeta.CommonBot.Settings.GlobalSettings.Instance.LastProfile);
                        Thread.Sleep(2000);
                    }
                }
            }
        }


        // Each time we join & leave a game, might as well clear the hashset of looked-at dropped items - just to keep it smaller
        private static void TrinityOnJoinGame(object src, EventArgs mea)
        {
            iTotalJoinGames++;
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
            hashUseOnceID = new HashSet<int>();
            dictUseOnceID = new Dictionary<int, int>();
            iMaxDeathsAllowed = 0;
            iDeathsThisRun = 0;
            _hashsetItemStatsLookedAt = new HashSet<string>();
            _hashsetItemPicksLookedAt = new HashSet<string>();
            _hashsetItemFollowersIgnored = new HashSet<string>();
            TownRun._dictItemStashAttempted = new Dictionary<int, int>();
            hashRGUIDBlacklist60 = new HashSet<int>();
            hashRGUIDBlacklist90 = new HashSet<int>();
            hashRGUIDBlacklist15 = new HashSet<int>();
            vBacktrackList = new SortedList<int, Vector3>();
            iTotalBacktracks = 0;
            HasMappedPlayerAbilities = false;
            PlayerMover.iTotalAntiStuckAttempts = 1;
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
            dictGilesObjectTypeCache = new Dictionary<int, GObjectType>();
            dictGilesMonsterAffixCache = new Dictionary<int, MonsterAffixes>();
            dictGilesMaxHealthCache = new Dictionary<int, double>();
            dictGilesLastHealthCache = new Dictionary<int, double>();
            dictGilesLastHealthChecked = new Dictionary<int, int>();
            dictGilesBurrowedCache = new Dictionary<int, bool>();
            dictGilesActorSNOCache = new Dictionary<int, int>();
            dictGilesACDGUIDCache = new Dictionary<int, int>();
            dictGilesInternalNameCache = new Dictionary<int, string>();
            dictGilesGameBalanceIDCache = new Dictionary<int, int>();
            dictGilesDynamicIDCache = new Dictionary<int, int>();
            dictGilesVectorCache = new Dictionary<int, Vector3>();
            dictGilesGoldAmountCache = new Dictionary<int, int>();
            dictGilesQualityCache = new Dictionary<int, ItemQuality>();
            dictGilesPickupItem = new Dictionary<int, bool>();
            dictSummonedByID = new Dictionary<int, int>();
            dictTotalInteractionAttempts = new Dictionary<int, int>();
            listProfilesLoaded = new List<string>();
            sLastProfileSeen = "";
            sFirstProfileSeen = "";


            NavHelper.UpdateSearchGridProvider();
            GoldInactivity.ResetCheckGold();

            global::GilesTrinity.XmlTags.TrinityLoadOnce.UsedProfiles = new List<string>();

            GenericCache.ClearCache();
            GenericBlacklist.ClearBlacklist();

        }
    }
}
