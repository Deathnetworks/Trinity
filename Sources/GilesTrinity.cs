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
        /// <summary>
        /// Update the cached data on the player information, including buffs if needed
        /// </summary>
        private static void UpdateCachedPlayerData()
        {
            if (DateTime.Now.Subtract(playerStatus.LastUpdated).TotalMilliseconds <= 100)
                return;
            // If we aren't in the game of a world is loading, don't do anything yet
            if (!ZetaDia.IsInGame || ZetaDia.IsLoadingWorld)
                return;
            var me = ZetaDia.Me;
            if (me == null)
                return;

            try
            {
                playerStatus.LastUpdated = DateTime.Now;
                playerStatus.IsInTown = me.IsInTown;
                playerStatus.IsIncapacitated = (me.IsFeared || me.IsStunned || me.IsFrozen || me.IsBlind);
                playerStatus.IsRooted = me.IsRooted;
                playerStatus.CurrentHealthPct = me.HitpointsCurrentPct;
                playerStatus.CurrentEnergy = me.CurrentPrimaryResource;
                playerStatus.CurrentEnergyPct = playerStatus.CurrentEnergy / me.MaxPrimaryResource;
                playerStatus.Discipline = me.CurrentSecondaryResource;
                playerStatus.DisciplinePct = playerStatus.Discipline / me.MaxSecondaryResource;
                playerStatus.CurrentPosition = me.Position;
                if (playerStatus.CurrentEnergy >= iWaitingReservedAmount)
                    playerStatus.WaitingForReserveEnergy = false;
                if (playerStatus.CurrentEnergy < 20)
                    playerStatus.WaitingForReserveEnergy = true;
                playerStatus.MyDynamicID = me.CommonData.DynamicId;
                playerStatus.Level = me.Level;
                playerStatus.ActorClass = me.ActorClass;
                playerStatus.BattleTag = ZetaDia.Service.CurrentHero.BattleTagName;
                playerStatus.SceneId = ZetaDia.Me.CurrentScene.SceneInfo.SNOId;

                // World ID safety caching incase it's ever unavailable
                if (ZetaDia.CurrentWorldDynamicId != -1)
                    iCurrentWorldID = ZetaDia.CurrentWorldDynamicId;
                // Game difficulty, used really for vault on DH's
                if (ZetaDia.Service.CurrentHero.CurrentDifficulty != GameDifficulty.Invalid)
                    iCurrentGameDifficulty = ZetaDia.Service.CurrentHero.CurrentDifficulty;

                // Refresh player buffs (to check for archon)
                GilesRefreshBuffs();
            }
            catch (Exception ex)
            {
                DbHelper.Log(TrinityLogLevel.Debug, LogCategory.CacheManagement, "Safely handled exception for grabbing player data.{0}{1}", Environment.NewLine, ex);
            }
        }


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
                bMappedPlayerAbilities = false;
                lastChangedZigZag = DateTime.Today;
                bAlreadyMoving = false;
                lastMovementCommand = DateTime.Today;
            }
            else
            {
                BotMain.PauseWhile(BotIsPaused);
                btnPauseBot.Content = "Unpause Bot";
                bMainBotPaused = true;
            }
        }

        private static bool BotIsPaused()
        {
            return bMainBotPaused;
        }
        private void GilesTrinityOnDeath(object src, EventArgs mea)
        {
            if (DateTime.Now.Subtract(lastDied).TotalSeconds > 10)
            {
                lastDied = DateTime.Now;
                iTotalDeaths++;
                iDeathsThisRun++;
                dictAbilityLastUse = new Dictionary<SNOPower, DateTime>(dictAbilityLastUseDefaults);
                vBacktrackList = new SortedList<int, Vector3>();
                iTotalBacktracks = 0;
                GilesPlayerMover.iTotalAntiStuckAttempts = 1;
                GilesPlayerMover.vSafeMovementLocation = Vector3.Zero;
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
                        GilesResetEverythingNewGame();
                        ZetaDia.Service.Games.LeaveGame();
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
        private static void GilesTrinityOnJoinGame(object src, EventArgs mea)
        {
            iTotalJoinGames++;
            GilesResetEverythingNewGame();
        }
        // Each time we join & leave a game, might as well clear the hashset of looked-at dropped items - just to keep it smaller
        private static void GilesTrinityOnLeaveGame(object src, EventArgs mea)
        {
            TotalLeaveGames++;
            GilesResetEverythingNewGame();
        }
        public static void GilesResetEverythingNewGame()
        {
            hashUseOnceID = new HashSet<int>();
            dictUseOnceID = new Dictionary<int, int>();
            iMaxDeathsAllowed = 0;
            iDeathsThisRun = 0;
            _hashsetItemStatsLookedAt = new HashSet<int>();
            _hashsetItemPicksLookedAt = new HashSet<int>();
            _hashsetItemFollowersIgnored = new HashSet<int>();
            TownRun._dictItemStashAttempted = new Dictionary<int, int>();
            hashRGUIDBlacklist60 = new HashSet<int>();
            hashRGUIDBlacklist90 = new HashSet<int>();
            hashRGUIDBlacklist15 = new HashSet<int>();
            vBacktrackList = new SortedList<int, Vector3>();
            iTotalBacktracks = 0;
            bMappedPlayerAbilities = false;
            GilesPlayerMover.iTotalAntiStuckAttempts = 1;
            GilesPlayerMover.vSafeMovementLocation = Vector3.Zero;
            GilesPlayerMover.vOldPosition = Vector3.Zero;
            GilesPlayerMover.iTimesReachedStuckPoint = 0;
            GilesPlayerMover.timeLastRecordedPosition = DateTime.Today;
            GilesPlayerMover.timeStartedUnstuckMeasure = DateTime.Today;
            GilesPlayerMover.iTimesReachedMaxUnstucks = 0;
            GilesPlayerMover.iCancelUnstuckerForSeconds = 0;
            GilesPlayerMover.timeCancelledUnstuckerFor = DateTime.Today;
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
            dictGilesQualityRechecked = new Dictionary<int, bool>();
            dictGilesPickupItem = new Dictionary<int, bool>();
            dictSummonedByID = new Dictionary<int, int>();
            dictTotalInteractionAttempts = new Dictionary<int, int>();
            listProfilesLoaded = new List<string>();
            sLastProfileSeen = "";
            sFirstProfileSeen = "";

            if (gp == null)
                gp = Navigator.SearchGridProvider;
            if (pf == null)
                pf = new PathFinder(gp);

        }
    }
}
