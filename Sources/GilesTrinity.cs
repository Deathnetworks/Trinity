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
            using (new PerformanceLogger("UpdateCachedPlayerData"))
            {
                if (DateTime.Now.Subtract(PlayerStatus.LastUpdated).TotalMilliseconds <= 100)
                    return;
                // If we aren't in the game of a world is loading, don't do anything yet
                if (!ZetaDia.IsInGame || ZetaDia.IsLoadingWorld)
                    return;
                var me = ZetaDia.Me;
                if (me == null)
                    return;

                try
                {
                    using (new PerformanceLogger("UpdateCachedPlayerData.1"))
                    {

                        PlayerStatus.LastUpdated = DateTime.Now;
                        PlayerStatus.IsInTown = me.IsInTown;
                        PlayerStatus.IsIncapacitated = (me.IsFeared || me.IsStunned || me.IsFrozen || me.IsBlind);
                        PlayerStatus.IsRooted = me.IsRooted;

                    }
                    using (new PerformanceLogger("UpdateCachedPlayerData.2"))
                    {

                        //PlayerStatus.CurrentHealthPct = me.HitpointsCurrentPct;
                        //PlayerStatus.PrimaryResource = me.CurrentPrimaryResource;
                        //PlayerStatus.PrimaryResourcePct = PlayerStatus.PrimaryResource / me.MaxPrimaryResource;
                        //PlayerStatus.SecondaryResource = me.CurrentSecondaryResource;
                        //PlayerStatus.SecondaryResourcePct = PlayerStatus.SecondaryResource / me.MaxSecondaryResource;
                        //PlayerStatus.CurrentPosition = me.Position;
                    }
                    using (new PerformanceLogger("UpdateCachedPlayerData.3"))
                    {

                        if (PlayerStatus.PrimaryResource >= MinEnergyReserve)
                            PlayerStatus.WaitingForReserveEnergy = false;
                        if (PlayerStatus.PrimaryResource < 20)
                            PlayerStatus.WaitingForReserveEnergy = true;
                        PlayerStatus.MyDynamicID = me.CommonData.DynamicId;
                        PlayerStatus.Level = me.Level;
                        PlayerStatus.ActorClass = me.ActorClass;
                        PlayerStatus.BattleTag = ZetaDia.Service.CurrentHero.BattleTagName;
                        PlayerStatus.LevelAreaId = ZetaDia.CurrentLevelAreaId;
                    }
                    using (new PerformanceLogger("UpdateCachedPlayerData.4"))
                    {
                        if (DateTime.Now.Subtract(PlayerStatus.Scene.LastUpdate).TotalMilliseconds > 1000 && Settings.Combat.Misc.UseNavMeshTargeting)
                        {
                            int CurrentSceneSNO = -1;
                            using (new PerformanceLogger("UpdateCachedPlayerData.4.1"))
                            {
                                CurrentSceneSNO = (int)ZetaDia.Me.SceneId;
                            }
                            using (new PerformanceLogger("UpdateCachedPlayerData.4.2"))
                            {
                                if (PlayerStatus.SceneId != CurrentSceneSNO)
                                {
                                    PlayerStatus.SceneId = CurrentSceneSNO;
                                    DbHelper.Log(TrinityLogLevel.Debug, LogCategory.CacheManagement, "Updating Grid Provider", true);
                                    UpdateSearchGridProvider();
                                }
                            }
                        }
                    }
                    using (new PerformanceLogger("UpdateCachedPlayerData.5"))
                    {

                        // World ID safety caching incase it's ever unavailable
                        if (ZetaDia.CurrentWorldDynamicId != -1)
                            CurrentWorldDynamicId = ZetaDia.CurrentWorldDynamicId;
                        if (ZetaDia.CurrentWorldId != -1)
                            cachedStaticWorldId = ZetaDia.CurrentWorldId;
                        // Game difficulty, used really for vault on DH's
                        if (ZetaDia.Service.CurrentHero.CurrentDifficulty != GameDifficulty.Invalid)
                            iCurrentGameDifficulty = ZetaDia.Service.CurrentHero.CurrentDifficulty;
                    }


                    // Refresh player buffs (to check for archon)
                    GilesRefreshBuffs();
                }
                catch (Exception ex)
                {
                    DbHelper.Log(TrinityLogLevel.Debug, LogCategory.CacheManagement, "Safely handled exception for grabbing player data.{0}{1}", Environment.NewLine, ex);
                }
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
                HasMappedPlayerAbilities = false;
                lastChangedZigZag = DateTime.Today;
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
        private static void TrinityOnJoinGame(object src, EventArgs mea)
        {
            iTotalJoinGames++;
            GilesResetEverythingNewGame();
        }
        // Each time we join & leave a game, might as well clear the hashset of looked-at dropped items - just to keep it smaller
        private static void TrinityOnLeaveGame(object src, EventArgs mea)
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
            PlayerMover.timeCancelledUnstuckerFor = DateTime.Today;
            GilesTrinity.UsedStuckSpots = new List<GilesTrinity.GridPoint>();
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
            //dictGilesQualityCache = new Dictionary<int, ItemQuality>();
            //dictGilesQualityRechecked = new Dictionary<int, bool>();
            dictGilesPickupItem = new Dictionary<int, bool>();
            dictSummonedByID = new Dictionary<int, int>();
            dictTotalInteractionAttempts = new Dictionary<int, int>();
            listProfilesLoaded = new List<string>();
            sLastProfileSeen = "";
            sFirstProfileSeen = "";


            UpdateSearchGridProvider();
            GoldInactivity.ResetCheckGold();

            global::GilesTrinity.XmlTags.TrinityLoadOnce.UsedProfiles = new List<string>();

            GenericCache.ClearCache();

        }
    }
}
