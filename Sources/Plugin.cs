using GilesTrinity.DbProvider;
using GilesTrinity.Technicals;
using System;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using Zeta;
using Zeta.Common.Plugins;
using Zeta.CommonBot;
using Zeta.Navigation;
using Zeta.Pathfinding;

namespace GilesTrinity
{
    /// <summary>
    /// Trinity DemonBuddy Plugin 
    /// </summary>
    public partial class GilesTrinity : IPlugin
    {
        /// <summary>
        /// Gets the version of plugin.
        /// </summary>
        /// <remarks>
        /// This is used by DemonBuddy on plugin tab 
        /// </remarks>
        /// <value>
        /// The version of plugin.
        /// </value>
        public Version Version
        {
            get
            {
                return new Version(1, 7, 1, 19);
            }
        }

        /// <summary>
        /// Gets the author of plugin.
        /// </summary>
        /// <remarks>
        /// This is used by DemonBuddy on plugin tab 
        /// </remarks>
        /// <value>
        /// The author of plugin.
        /// </value>
        public string Author
        {
            get
            {
                return "GilesSmith + Demonbuddy Community Devs";
            }
        }

        /// <summary>
        /// Gets the description of plugin.
        /// </summary>
        /// <remarks>
        /// This is used by DemonBuddy on plugin tab 
        /// </remarks>
        /// <value>
        /// The description of plugin.
        /// </value>
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
            if (!ZetaDia.IsInGame || !ZetaDia.Me.IsValid || ZetaDia.IsLoadingWorld || !ZetaDia.CPlayer.IsValid)
                return;


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
                DbHelper.Log(TrinityLogLevel.Normal, LogCategory.UserInformation, "Fatal Error - cannot enable plugin. Invalid path: {0}", FileManager.PluginPath);
                DbHelper.Log(TrinityLogLevel.Normal, LogCategory.UserInformation, "Please check you have installed the plugin to the correct location, and then restart DemonBuddy and re-enable the plugin.");
                DbHelper.Log(TrinityLogLevel.Normal, LogCategory.UserInformation, @"Plugin should be installed to \<DemonBuddyFolder>\Plugins\Trinity\");
            }
            else
            {
                HasMappedPlayerAbilities = false;
                IsPluginEnabled = true;

                // Settings are available after this... 
                LoadConfiguration();

                Navigator.PlayerMover = new PlayerMover();
                SetUnstuckProvider();
                GameEvents.OnPlayerDied += TrinityOnDeath;
                GameEvents.OnGameJoined += TrinityOnJoinGame;
                GameEvents.OnGameLeft += TrinityOnLeaveGame;
                CombatTargeting.Instance.Provider = new BlankCombatProvider();
                LootTargeting.Instance.Provider = new BlankLootProvider();
                ObstacleTargeting.Instance.Provider = new BlankObstacleProvider();
                ItemManager.Current = new TrinityItemManager();

                UpdateSearchGridProvider();

                // Safety check incase DB "OnStart" event didn't fire properly
                if (BotMain.IsRunning)
                {
                    TrinityBotStart(null);
                    if (ZetaDia.IsInGame)
                        TrinityOnJoinGame(null, null);
                }

                SetBotTPS();

                TrinityPowerManager.LoadLegacyDelays();

                DbHelper.Log(TrinityLogLevel.Normal, LogCategory.UserInformation, "ENABLED: {0} now in action!", Description); ;
            }

            // reseting stash rules
            StashRule.reset();
        }

        internal static void SetBotTPS()
        {
            // Carguy's ticks-per-second feature
            if (Settings.Advanced.TPSEnabled)
            {
                BotMain.TicksPerSecond = (int)Settings.Advanced.TPSLimit;
                DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.UserInformation, "Bot TPS set to {0}", (int)Settings.Advanced.TPSLimit);
            }
            else
            {
                BotMain.TicksPerSecond = 10;
                DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.UserInformation, "Reset bot TPS to default", (int)Settings.Advanced.TPSLimit);
            }
        }

        internal static void SetUnstuckProvider()
        {
            if (Settings.Advanced.UnstuckerEnabled)
            {
                Navigator.StuckHandler = new StuckHandler();
                DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.UserInformation, "Using Trinity Unstucker", true);
            }
            else
            {
                Navigator.StuckHandler = new Zeta.Navigation.DefaultStuckHandler();
                DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.UserInformation, "Using Default Demonbuddy Unstucker", true);
            }
        }

        internal static void SetWindowTitle(string profileName = "")
        {
            Application.Current.Dispatcher.Invoke(new Action( () => {
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

        /// <summary>
        /// Adds the Pause and Town Run buttons to Demonbuddy's main window. Sets Window Title.
        /// </summary>
        /// <param name="battleTagName"></param>
        /// <returns></returns>
        [Obsolete("This has been removed in the latest DemonbuddyBETA")]
        private static Action PaintMainWindowButtons()
        {
            return new System.Action(
                        () =>
                        {
                            Window mainWindow = Application.Current.MainWindow;
                            var tab = mainWindow.FindName("tabControlMain") as TabControl;
                            if (tab == null) return;
                            var infoDumpTab = tab.Items[0] as TabItem;
                            if (infoDumpTab == null) return;
                            btnPauseBot = new Button
                            {
                                Width = 100,
                                HorizontalAlignment = HorizontalAlignment.Left,
                                VerticalAlignment = VerticalAlignment.Top,
                                Margin = new Thickness(232, 6, 0, 0),
                                Content = "Pause Bot"
                            };
                            btnPauseBot.Click += buttonPause_Click;
                            btnTownRun = new Button
                            {
                                Width = 100,
                                HorizontalAlignment = HorizontalAlignment.Left,
                                VerticalAlignment = VerticalAlignment.Top,
                                Margin = new Thickness(232, 32, 0, 0),
                                Content = "Force Town Run"
                            };
                            btnTownRun.Click += buttonTownRun_Click;
                            var grid = infoDumpTab.Content as Grid;
                            if (grid == null) return;
                            grid.Children.Add(btnPauseBot);
                            grid.Children.Add(btnTownRun);
                        });
        }

        /// <summary>
        /// Called when user disable the plugin.
        /// </summary>
        public void OnDisabled()
        {
            IsPluginEnabled = false;
            Navigator.PlayerMover = new DefaultPlayerMover();
            Navigator.StuckHandler = new DefaultStuckHandler();
            CombatTargeting.Instance.Provider = new DefaultCombatTargetingProvider();
            LootTargeting.Instance.Provider = new DefaultLootTargetingProvider();
            ObstacleTargeting.Instance.Provider = new DefaultObstacleTargetingProvider();

            GameEvents.OnPlayerDied -= TrinityOnDeath;
            BotMain.OnStop -= TrinityBotStop;
            BotMain.OnStop -= PluginCheck;
            GameEvents.OnGameJoined -= TrinityOnJoinGame;
            GameEvents.OnGameLeft -= TrinityOnLeaveGame;
            DbHelper.Log(TrinityLogLevel.Normal, LogCategory.UserInformation, "");
            DbHelper.Log(TrinityLogLevel.Normal, LogCategory.UserInformation, "DISABLED: Trinity is now shut down...");
            DbHelper.Log(TrinityLogLevel.Normal, LogCategory.UserInformation, "");
        }

        /// <summary>
        /// Called when DemonBuddy shut down.
        /// </summary>
        public void OnShutdown()
        {
        }

        /// <summary>
        /// Called when DemonBuddy initialize the plugin.
        /// </summary>
        public void OnInitialize()
        {
            Zeta.CommonBot.BotMain.OnStart += PluginCheck;
        }

        void PluginCheck(IBot bot)
        {
            if (!IsPluginEnabled && bot != null)
            {
                DbHelper.Log(TrinityLogLevel.Error, LogCategory.UserInformation, "\tWARNING: Trinity Plugin is NOT YET ENABLED. Bot start detected");
                DbHelper.Log(TrinityLogLevel.Error, LogCategory.UserInformation, "\tIgnore this message if you are not currently using Trinity.");
                return;
            }
        }



        /// <summary>
        /// Gets the displayed name of plugin.
        /// </summary>
        /// <remarks>
        /// This is used by DemonBuddy on plugin tab 
        /// </remarks>
        /// <value>The name.</value>
        public string Name
        {
            get
            {
                return "Trinity";
            }
        }

        /// <summary>
        /// Check if this instance of plugin is equals to the specified other.
        /// </summary>
        /// <param name="other">The other plugin to compare.</param>
        /// <returns>
        /// <c>true</c> if this instance is equals to the specified other; otherwise <c>false</c>
        /// </returns>
        public bool Equals(IPlugin other)
        {
            return (other.Name == Name) && (other.Version == Version);
        }


        public GilesTrinity()
        {

        }
    }
}
