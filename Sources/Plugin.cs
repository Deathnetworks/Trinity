using System;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using Trinity.DbProvider;
using Trinity.Technicals;
using Zeta;
using Zeta.Common.Plugins;
using Zeta.CommonBot;
using Zeta.Navigation;

namespace Trinity
{
    /// <summary>
    /// Trinity DemonBuddy Plugin 
    /// </summary>
    public partial class Trinity : IPlugin
    {
        public Version Version
        {
            get
            {
                return new Version(1, 7, 2, 14);
            }
        }

        public string Author
        {
            get
            {
                return "GilesSmith + rrrix + Community Devs";
            }
        }

        public string Description
        {
            get
            {
                return string.Format("Trinity v{0}", Version);
            }
        }

        /// <summary>
        /// Receive Pulse event from DemonBuddy.
        /// </summary>
        public void OnPulse()
        {
            try
            {
                if (ZetaDia.Me == null)
                    return;

                if (!ZetaDia.IsInGame || !ZetaDia.Me.IsValid || ZetaDia.IsLoadingWorld)
                    return;

                // hax for sending notifications after a town run
                if (!Zeta.CommonBot.Logic.BrainBehavior.IsVendoring && !PlayerStatus.IsInTown)
                {
                    TownRun.SendEmailNotification();
                    TownRun.SendMobileNotifications();
                }

                // See if we should update the stats file
                if (DateTime.Now.Subtract(ItemStatsLastPostedReport).TotalSeconds > 10)
                {
                    ItemStatsLastPostedReport = DateTime.Now;
                    OutputReport();
                }

                // Recording of all the XML's in use this run
                UsedProfileManager.RecordProfile();

                Monk_MaintainTempestRush();
            }
            catch (Exception ex)
            {
                Logger.Log(LogCategory.UserInformation, "Exception in Pulse: {0}", ex.ToString());
            }
        }

        /// <summary>
        /// Called when user Enable the plugin.
        /// </summary>
        public void OnEnabled()
        {
            BotMain.OnStart += TrinityBotStart;
            BotMain.OnStop += TrinityBotStop;

            // Set up the pause button

            // rrrix: removing for next DB beta... 
            //Application.Current.Dispatcher.Invoke(PaintMainWindowButtons());

            SetWindowTitle();

            if (!Directory.Exists(FileManager.PluginPath))
            {
                Logger.Log(TrinityLogLevel.Normal, LogCategory.UserInformation, "Fatal Error - cannot enable plugin. Invalid path: {0}", FileManager.PluginPath);
                Logger.Log(TrinityLogLevel.Normal, LogCategory.UserInformation, "Please check you have installed the plugin to the correct location, and then restart DemonBuddy and re-enable the plugin.");
                Logger.Log(TrinityLogLevel.Normal, LogCategory.UserInformation, @"Plugin should be installed to \<DemonBuddyFolder>\Plugins\Trinity\");
            }
            else
            {
                Helpers.PluginCheck.Start();

                HasMappedPlayerAbilities = false;
                isPluginEnabled = true;

                // Settings are available after this... 
                LoadConfiguration();

                Navigator.PlayerMover = new PlayerMover();
                SetUnstuckProvider();
                GameEvents.OnPlayerDied += TrinityOnDeath;
                GameEvents.OnGameJoined += TrinityOnJoinGame;
                GameEvents.OnGameLeft += TrinityOnLeaveGame;

                GameEvents.OnItemSold += ItemEvents.TrinityOnItemSold;
                GameEvents.OnItemSalvaged += ItemEvents.TrinityOnItemSalvaged;
                GameEvents.OnItemStashed += ItemEvents.TrinityOnItemStashed;

                GameEvents.OnGameChanged += GameEvents_OnGameChanged;

                CombatTargeting.Instance.Provider = new BlankCombatProvider();
                LootTargeting.Instance.Provider = new BlankLootProvider();
                ObstacleTargeting.Instance.Provider = new BlankObstacleProvider();

                if (Settings.Loot.ItemFilterMode != global::Trinity.Settings.Loot.ItemFilterMode.DemonBuddy)
                {
                    ItemManager.Current = new TrinityItemManager();
                }

                // Safety check incase DB "OnStart" event didn't fire properly
                if (BotMain.IsRunning)
                {
                    TrinityBotStart(null);
                    if (ZetaDia.IsInGame)
                        TrinityOnJoinGame(null, null);
                }

                SetBotTPS();

                TrinityPowerManager.LoadLegacyDelays();

                Logger.Log(TrinityLogLevel.Normal, LogCategory.UserInformation, "ENABLED: {0} now in action!", Description); ;
            }

            if (StashRule != null)
            {
                // reseting stash rules
                StashRule.reset();
            }
        }
        
        /// <summary>
        /// Called when user disable the plugin.
        /// </summary>
        public void OnDisabled()
        {
            isPluginEnabled = false;
            Navigator.PlayerMover = new DefaultPlayerMover();
            Navigator.StuckHandler = new DefaultStuckHandler();
            CombatTargeting.Instance.Provider = new DefaultCombatTargetingProvider();
            LootTargeting.Instance.Provider = new DefaultLootTargetingProvider();
            ObstacleTargeting.Instance.Provider = new DefaultObstacleTargetingProvider();

            GameEvents.OnPlayerDied -= TrinityOnDeath;
            BotMain.OnStop -= TrinityBotStop;

            GameEvents.OnGameJoined -= TrinityOnJoinGame;
            GameEvents.OnGameLeft -= TrinityOnLeaveGame;
            Logger.Log(TrinityLogLevel.Normal, LogCategory.UserInformation, "");
            Logger.Log(TrinityLogLevel.Normal, LogCategory.UserInformation, "DISABLED: Trinity is now shut down...");
            Logger.Log(TrinityLogLevel.Normal, LogCategory.UserInformation, "");
            GenericCache.Shutdown();
            GenericBlacklist.Shutdown();
        }

        /// <summary>
        /// Called when DemonBuddy shut down.
        /// </summary>
        public void OnShutdown()
        {
            GenericCache.Shutdown();
            GenericBlacklist.Shutdown();
            Helpers.PluginCheck.Shutdown();
        }

        /// <summary>
        /// Called when DemonBuddy initialize the plugin.
        /// </summary>
        public void OnInitialize()
        {

        }

        public string Name
        {
            get
            {
                return "Trinity";
            }
        }

        public bool Equals(IPlugin other)
        {
            return (other.Name == Name) && (other.Version == Version);
        }

        private static Trinity _instance;
        public static Trinity Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new Trinity();
                }
                return _instance;
            }
        }

        public Trinity()
        {
            _instance = this;
        }


        internal static void SetWindowTitle(string profileName = "")
        {
            Application.Current.Dispatcher.Invoke(new Action(() =>
            {
                string battleTagName = "";
                try
                {
                    battleTagName = ZetaDia.Service.CurrentHero.BattleTagName;
                }
                catch { }
                Window mainWindow = Application.Current.MainWindow;

                string windowTitle = "DB - " + battleTagName + " - PID:" + System.Diagnostics.Process.GetCurrentProcess().Id.ToString();

                if (profileName.Trim() != String.Empty)
                {
                    windowTitle += " - " + profileName;
                }

                mainWindow.Title = windowTitle;
            }));
        }

    }
}
