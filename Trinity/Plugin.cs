using System;
using System.IO;
using System.Windows;
using Trinity.DbProvider;
using Trinity.Technicals;
using Zeta.Bot;
using Zeta.Bot.Navigation;
using Zeta.Common.Plugins;
using Zeta.Game;

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
                return new Version(1, 8, 22);
            }
        }

        public string Author
        {
            get
            {
                return "rrrix + darkfriend77 + GilesSmith + Community Devs";
            }
        }

        public string Description
        {
            get
            {
                return string.Format("Trinity v{0}", Version);
            }
        }

        private bool MouseLeft()
        {
            return (System.Windows.Forms.Control.MouseButtons & System.Windows.Forms.MouseButtons.Left) == System.Windows.Forms.MouseButtons.Left;
        }

        /// <summary>
        /// Receive Pulse event from DemonBuddy.
        /// </summary>
        public void OnPulse()
        {
            using (new PerformanceLogger("OnPulse"))
            {
                try
                {
                    if (ZetaDia.Me == null)
                        return;

                    if (!ZetaDia.IsInGame || !ZetaDia.Me.IsValid || ZetaDia.IsLoadingWorld)
                        return;

                    GameUI.SafeClickUIButtons();

                    if (ZetaDia.Me.IsDead)
                        return;

                    using (new PerformanceLogger("LazyRaiderClickToPause"))
                    {

                        if (Settings.Advanced.LazyRaiderClickToPause && !BotMain.IsPaused)
                        {
                            BotMain.PauseWhile(MouseLeft);
                        }
                    }

                    UsedProfileManager.SetProfileInWindowTitle();

                    // See if we should update the stats file
                    if (DateTime.UtcNow.Subtract(ItemStatsLastPostedReport).TotalSeconds > 10)
                    {
                        ItemStatsLastPostedReport = DateTime.UtcNow;
                        OutputReport();
                    }

                    // Recording of all the XML's in use this run
                    UsedProfileManager.RecordProfile();

                    Monk_MaintainTempestRush();
                }
                catch (System.AccessViolationException)
                {
                    // woof! 
                }
                catch (Exception ex)
                {
                    Logger.Log(LogCategory.UserInformation, "Exception in Pulse: {0}", ex.ToString());
                }
            }
        }

        /// <summary>
        /// Called when user Enable the plugin.
        /// </summary>
        public void OnEnabled()
        {
            try
            {
                Logger.Log("OnEnable start");
                DateTime dateOnEnabledStart = DateTime.UtcNow;

                BotMain.OnStart += TrinityBotStart;
                BotMain.OnStop += TrinityBotStop;

                SetWindowTitle();

                if (!Directory.Exists(FileManager.PluginPath))
                {
                    Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "Fatal Error - cannot enable plugin. Invalid path: {0}", FileManager.PluginPath);
                    Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "Please check you have installed the plugin to the correct location, and then restart DemonBuddy and re-enable the plugin.");
                    Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, @"Plugin should be installed to \<DemonBuddyFolder>\Plugins\Trinity\");
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
                    GameEvents.OnItemIdentificationRequest += ItemEvents.TrinityOnOnItemIdentificationRequest;

                    GameEvents.OnGameChanged += GameEvents_OnGameChanged;

                    CombatTargeting.Instance.Provider = new BlankCombatProvider();
                    LootTargeting.Instance.Provider = new BlankLootProvider();
                    ObstacleTargeting.Instance.Provider = new BlankObstacleProvider();

                    if (Settings.Loot.ItemFilterMode != global::Trinity.Config.Loot.ItemFilterMode.DemonBuddy)
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

                    BeginInvoke(new Action(() => SetBotTPS()));

                    UI.UILoader.PreLoadWindowContent();

                    Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "ENABLED: {0} now in action!", Description); ;
                }

                if (StashRule != null)
                {
                    // reseting stash rules
                    BeginInvoke(new Action(() => StashRule.reset()));
                }

                Logger.LogDebug("OnEnable took {0}ms", DateTime.UtcNow.Subtract(dateOnEnabledStart).TotalMilliseconds);
            }
            catch (Exception ex)
            {
                Logger.LogError("Error in OnEnable: " + ex.ToString());
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
            Navigator.SearchGridProvider = new Zeta.Bot.Navigation.MainGridProvider();

            GameEvents.OnPlayerDied -= TrinityOnDeath;
            BotMain.OnStop -= TrinityBotStop;

            GameEvents.OnGameJoined -= TrinityOnJoinGame;
            GameEvents.OnGameLeft -= TrinityOnLeaveGame;

            Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "");
            Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "DISABLED: Trinity is now shut down...");
            Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "");
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
            Helpers.PluginCheck.CheckAndInstallTrinityRoutine();
            Logger.Log("Initialized v{0}", this.Version);
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
            Helpers.PluginCheck.CheckAndInstallTrinityRoutine();
        }


        private static DateTime _lastWindowTitleTick = DateTime.MinValue;
        private static Window mainWindow;
        private static string mainWindowTitle;
        internal static void SetWindowTitle(string profileName = "")
        {
            if (DateTime.UtcNow.Subtract(_lastWindowTitleTick).TotalMilliseconds < 5000)
                return;

            if (mainWindow == null)
            {
                Application.Current.Dispatcher.Invoke(new Action(() => mainWindow = Application.Current.MainWindow));
            }
            if (mainWindow != null)
            {
                Application.Current.Dispatcher.Invoke(new Action(() => mainWindowTitle = mainWindow.Title));
            }

            if (mainWindowTitle.Contains("Demonbuddy") && ZetaDia.Service.IsValid && ZetaDia.Service.Platform.IsValid && ZetaDia.Service.Platform.IsConnected)
            {

                string battleTagName = "";
                if (Settings.Advanced.ShowBattleTag)
                {
                    try
                    {
                        battleTagName = "- " + FileManager.BattleTagName + " ";
                    }
                    catch { }
                }

                string windowTitle = "DB " + battleTagName + "- PID:" + System.Diagnostics.Process.GetCurrentProcess().Id.ToString();

                if (profileName.Trim() != String.Empty)
                {
                    windowTitle += " - " + profileName;
                }

                BeginInvoke(new Action(() =>
                {
                    try
                    {
                        if (mainWindow != null && windowTitle != null)
                            mainWindow.Title = windowTitle;
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError("Unable to set MainWindow Title {0}", ex.ToString());
                    }
                }));
            }
        }

        internal static void BeginInvoke(Action action)
        {
            Application.Current.Dispatcher.BeginInvoke(action);
        }

    }
}
