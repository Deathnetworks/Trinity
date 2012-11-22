using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Web;
using System.Reflection;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.IO;
using System.Windows.Markup;
using Zeta;
using Zeta.Common;
using Zeta.Common.Plugins;
using Zeta.CommonBot;
using Zeta.CommonBot.Profile;
using Zeta.CommonBot.Profile.Composites;
using Zeta.Internals;
using Zeta.Internals.Actors;
using Zeta.Internals.Actors.Gizmos;
using Zeta.Internals.SNO;
using Zeta.Navigation;
using Zeta.TreeSharp;
using Zeta.XmlEngine;
namespace GilesTrinity
{
    public partial class GilesTrinity : IPlugin
    {
        public Version Version { get { return new Version(1, 6, 3, 4); } }
        public string Author { get { return "GilesSmith + Demonbuddy Community Devs"; } }
        public string Description { get { return "GilesTrinity version " + Version + " (v0.43) + x7"; } }

        public void OnPulse()
        {
        }
        public void OnEnabled()
        {
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
            Logging.WriteDiagnostic("Trinity Initialization, settings location=" + sTrinityConfigFile);
            BotMain.OnStart += GilesTrinityStart;
            // Force logging to disabled
            if (bDisableFileLogging)
                Zeta.Common.Logging.LogFileLevel = Zeta.Common.LogLevel.None;
            // Set up the pause button
            Application.Current.Dispatcher.Invoke(
            new System.Action(
            () =>
            {
                Window mainWindow = Application.Current.MainWindow;
                try
                {
                    if (bDebugLogSpecial)
                        mainWindow.Title = "DB - " + battleTagName + " - PID:" + System.Diagnostics.Process.GetCurrentProcess().Id.ToString();
                    else
                        mainWindow.Title = "DB - " + battleTagName;


                }
                catch
                {
                }
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
                if (settings.bEnableTPS)
                {
                    BotMain.TicksPerSecond = (int)settings.iTPSAmount;
                }
                Log("");
                Log("ENABLED: " + Description + " now in action!"); ;
                Log("");
            }
        }
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
        public void OnShutdown()
        {
        }
        public void OnInitialize()
        {
        }
        public string Name { get { return "GilesTrinity"; } }
        public bool Equals(IPlugin other) { return (other.Name == Name) && (other.Version == Version); }
    }
}
