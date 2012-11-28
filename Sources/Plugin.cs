using System;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using GilesTrinity.DbProvider;
using Zeta;
using Zeta.Common;
using Zeta.Common.Plugins;
using Zeta.CommonBot;
using Zeta.Navigation;

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
                return new Version(1, 7, 0, 13);
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
        { }

        /// <summary>
        /// Called when user Enable the plugin.
        /// </summary>
        public void OnEnabled()
        {
#if DEBUG
            Settings.Advanced.DebugItemValuation = true;
#endif

            string battleTagName = "";
            try
            {
                battleTagName = ZetaDia.Service.CurrentHero.BattleTagName;
            }
            catch { }

            string sDemonBuddyPath = Assembly.GetEntryAssembly().Location;
            sTrinityPluginPath = Path.GetDirectoryName(sDemonBuddyPath) + @"\Plugins\GilesTrinity\";
            sTrinityConfigFile = Path.GetDirectoryName(sDemonBuddyPath) + @"\Settings\GilesTrinity.cfg";

            sTrinityEmailConfigFile = Path.GetDirectoryName(sDemonBuddyPath) + @"\Settings\" + battleTagName + @"-Email.cfg";

            //Logging.WriteDiagnostic("Trinity Initialization, settings location=" + sTrinityConfigFile);

            BotMain.OnStart += GilesTrinityStart;
            // Force logging to disabled
            //if (bDisableFileLogging)
            //    Zeta.Common.Logging.LogFileLevel = Zeta.Common.LogLevel.None;

            // Set up the pause button
            Application.Current.Dispatcher.Invoke(
            new System.Action(
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
            }));
            if (!Directory.Exists(sTrinityPluginPath))
            {
                Log("Fatal Error - cannot enable plugin. Invalid path: " + sTrinityPluginPath);
                Log("Please check you have installed the plugin to the correct location, and then restart DemonBuddy and re-enable the plugin.");
                Log(@"Plugin should be installed to \<DemonBuddyFolder>\Plugins\GilesTrinity\");
            }
            else
            {
                bMappedPlayerAbilities = false;
                bPluginEnabled = true;
                LoadConfiguration();
                Navigator.PlayerMover = new GilesPlayerMover();
                Navigator.StuckHandler = new GilesStuckHandler();
                GameEvents.OnPlayerDied += GilesTrinityOnDeath;
                BotMain.OnStop += GilesTrinityHandleBotStop;
                GameEvents.OnGameJoined += GilesTrinityOnJoinGame;
                GameEvents.OnGameLeft += GilesTrinityOnLeaveGame;
                ITargetingProvider newCombatTargetingProvider = new GilesCombatTargetingReplacer();
                CombatTargeting.Instance.Provider = newCombatTargetingProvider;
                ITargetingProvider newLootTargetingProvider = new GilesLootTargetingProvider();
                LootTargeting.Instance.Provider = newLootTargetingProvider;
                ITargetingProvider newObstacleTargetingProvider = new GilesObstacleTargetingProvider();
                ObstacleTargeting.Instance.Provider = newObstacleTargetingProvider;
                // Safety check incase DB "OnStart" event didn't fire properly
                if (BotMain.IsRunning)
                {
                    GilesTrinityStart(null);
                    if (ZetaDia.IsInGame)
                        GilesTrinityOnJoinGame(null, null);
                }
                // Carguy's ticks-per-second feature
                if (Settings.Advanced.TPSEnabled)
                {
                    BotMain.TicksPerSecond = (int)Settings.Advanced.TPSLimit;
                }
                Log("");
                Log("ENABLED: " + Description + " now in action!"); ;
                Log("");
            }
            try
            {
                StashRule.init();
            }
            catch { }
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
            BotMain.OnStop -= GilesTrinityHandleBotStop;
            GameEvents.OnGameJoined -= GilesTrinityOnJoinGame;
            GameEvents.OnGameLeft -= GilesTrinityOnLeaveGame;
            Log("DISABLED: Giles Trinity is now shut down...");
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
           // Instance = this;
        }

        //public static GilesTrinity Instance { get; private set; }

    }
}
