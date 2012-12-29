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
    /// Giles Trinity DemonBuddy Plugin 
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
                return new Version(1, 7, 1, 10);
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
                return string.Format("GilesTrinity Community Edition (version {0})", Version);
            }
        }

        /// <summary>
        /// Receive Pulse event from DemonBuddy.
        /// </summary>
        public void OnPulse()
        {
            //RefreshDiaObjectCache();
        }

        /// <summary>
        /// Called when user Enable the plugin.
        /// </summary>
        public void OnEnabled()
        {
            string battleTagName = "";
            try
            {
                battleTagName = ZetaDia.Service.CurrentHero.BattleTagName;
            }
            catch { }

            BotMain.OnStart += TrinityBotStart;
            BotMain.OnStop += TrinityBotStop;

            // Set up the pause button
            Application.Current.Dispatcher.Invoke(PaintMainWindowButtons(battleTagName));

            if (!Directory.Exists(FileManager.PluginPath))
            {
                DbHelper.Log(TrinityLogLevel.Normal, LogCategory.UserInformation, "Fatal Error - cannot enable plugin. Invalid path: {0}", FileManager.PluginPath);
                DbHelper.Log(TrinityLogLevel.Normal, LogCategory.UserInformation, "Please check you have installed the plugin to the correct location, and then restart DemonBuddy and re-enable the plugin.");
                DbHelper.Log(TrinityLogLevel.Normal, LogCategory.UserInformation, @"Plugin should be installed to \<DemonBuddyFolder>\Plugins\GilesTrinity\");
            }
            else
            {
                bMappedPlayerAbilities = false;
                bPluginEnabled = true;

                // Settings are available after this... 
                LoadConfiguration();

                Navigator.PlayerMover = new GilesPlayerMover();
                SetUnstuckProvider();
                GameEvents.OnPlayerDied += GilesTrinityOnDeath;
                GameEvents.OnGameJoined += GilesTrinityOnJoinGame;
                GameEvents.OnGameLeft += GilesTrinityOnLeaveGame;
                CombatTargeting.Instance.Provider = new GilesCombatTargetingReplacer();
                LootTargeting.Instance.Provider = new GilesLootTargetingProvider();
                ObstacleTargeting.Instance.Provider = new GilesObstacleTargetingProvider();

                if (Settings.Combat.Misc.UseNavMeshTargeting)
                {
                    if (gp == null)
                        gp = Navigator.SearchGridProvider;
                    if (pf == null)
                        pf = new PathFinder(gp);
                }

                // Safety check incase DB "OnStart" event didn't fire properly
                if (BotMain.IsRunning)
                {
                    TrinityBotStart(null);
                    if (ZetaDia.IsInGame)
                        GilesTrinityOnJoinGame(null, null);
                }

                SetBotTPS();

                TrinityPowerManager.LoadLegacyDelays();

                DbHelper.Log(TrinityLogLevel.Normal, LogCategory.UserInformation, "");
                DbHelper.Log(TrinityLogLevel.Normal, LogCategory.UserInformation, "ENABLED: {0} now in action!", Description); ;
                DbHelper.Log(TrinityLogLevel.Normal, LogCategory.UserInformation, "");
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
                Navigator.StuckHandler = new GilesStuckHandler();
                DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.UserInformation, "Using Trinity Unstucker", true);
            }
            else
            {
                Navigator.StuckHandler = new Zeta.Navigation.DefaultStuckHandler();
                DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.UserInformation, "Using Default Demonbuddy Unstucker", true);
            }
        }

        /// <summary>
        /// Adds the Pause and Town Run buttons to Demonbuddy's main window. Sets Window Title.
        /// </summary>
        /// <param name="battleTagName"></param>
        /// <returns></returns>
        private static Action PaintMainWindowButtons(string battleTagName)
        {
            return new System.Action(
                        () =>
                        {
                            Window mainWindow = Application.Current.MainWindow;
                            try
                            {
                                mainWindow.Title = "DB - " + battleTagName + " - PID:" + System.Diagnostics.Process.GetCurrentProcess().Id.ToString();
                            }
                            catch { }
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
            bPluginEnabled = false;
            Navigator.PlayerMover = new DefaultPlayerMover();
            Navigator.StuckHandler = new DefaultStuckHandler();
            CombatTargeting.Instance.Provider = new DefaultCombatTargetingProvider();
            LootTargeting.Instance.Provider = new DefaultLootTargetingProvider();
            ObstacleTargeting.Instance.Provider = new DefaultObstacleTargetingProvider();
            GameEvents.OnPlayerDied -= GilesTrinityOnDeath;
            BotMain.OnStop -= TrinityBotStop;
            GameEvents.OnGameJoined -= GilesTrinityOnJoinGame;
            GameEvents.OnGameLeft -= GilesTrinityOnLeaveGame;
            DbHelper.Log(TrinityLogLevel.Normal, LogCategory.UserInformation, "");
            DbHelper.Log(TrinityLogLevel.Normal, LogCategory.UserInformation, "DISABLED: Giles Trinity is now shut down...");
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
                return "GilesTrinity";
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
