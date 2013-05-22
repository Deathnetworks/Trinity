using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Trinity.Technicals;
using Zeta.CommonBot;

namespace Trinity.Helpers
{
    public class PluginCheck
    {
        private static Thread PluginCheckWatcher;

        /// <summary>
        /// Starts the watcher thread
        /// </summary>
        public static void Start()
        {
            Shutdown();

            if (PassedAllChecks)
                return;

            if (PluginCheckWatcher == null)
            {
                PluginCheckWatcher = new Thread(PluginChecker);
                PluginCheckWatcher.IsBackground = true;
                PluginCheckWatcher.Start();
                Logger.LogDebug("Plugin Check Watcher thread started");
            }
        }

        /// <summary>
        /// Stops the watcher thread if its running
        /// </summary>
        public static void Shutdown()
        {
            if (PluginCheckWatcher != null)
            {
                if (PluginCheckWatcher.IsAlive)
                    PluginCheckWatcher.Abort();
                PluginCheckWatcher = null;
            }
        }

        private static bool passedAllChecks = false;

        public static bool PassedAllChecks
        {
            get { return PluginCheck.passedAllChecks; }
            private set { PluginCheck.passedAllChecks = value; }
        }

        internal static void PluginChecker()
        {
            while (!PassedAllChecks)
            {
                while (!BotMain.IsRunning)
                {
                    Thread.Sleep(250);
                    PassedAllChecks = false;
                }

                if (!Trinity.IsPluginEnabled)
                {
                    Logger.Log(TrinityLogLevel.Error, LogCategory.UserInformation, "#################################################################");
                    Logger.Log(TrinityLogLevel.Error, LogCategory.UserInformation, "WARNING: Trinity Plugin is NOT YET ENABLED. Bot start detected");
                    Logger.Log(TrinityLogLevel.Error, LogCategory.UserInformation, "Ignore this message if you are not currently using Trinity.");
                    Logger.Log(TrinityLogLevel.Error, LogCategory.UserInformation, "#################################################################");
                    return;
                }

                bool trinityRoutineSelected = Zeta.CommonBot.RoutineManager.Current.Name.ToLower().Contains("trinity");

                if (!trinityRoutineSelected)
                {
                    BotMain.Stop();
                    Logger.Log(TrinityLogLevel.Error, LogCategory.UserInformation, "WARNING: Trinity Plugin is enabled, incorrect routine found. Bot start detected");
                    Logger.Log(TrinityLogLevel.Error, LogCategory.UserInformation, "#################################################################");
                    Logger.Log(TrinityLogLevel.Error, LogCategory.UserInformation, "");
                    Logger.Log(TrinityLogLevel.Error, LogCategory.UserInformation, "Found Routine: {0}", Zeta.CommonBot.RoutineManager.Current.Name);
                    Logger.Log(TrinityLogLevel.Error, LogCategory.UserInformation, "");
                    Logger.Log(TrinityLogLevel.Error, LogCategory.UserInformation, "#################################################################");
                    Logger.Log(TrinityLogLevel.Error, LogCategory.UserInformation, "ERROR: You are not using the Trinity Combat Routine!");
                    Logger.Log(TrinityLogLevel.Error, LogCategory.UserInformation, "You MUST download and select the Trinity Combat Routine");
                    Logger.Log(TrinityLogLevel.Error, LogCategory.UserInformation, "http://www.thebuddyforum.com/demonbuddy-forum/plugins/trinity/93720-trinity-download-here.html");
                    Logger.Log(TrinityLogLevel.Error, LogCategory.UserInformation, "");
                    Logger.Log(TrinityLogLevel.Error, LogCategory.UserInformation, "Trinity will NOT work with any other combat routine");
                    Logger.Log(TrinityLogLevel.Error, LogCategory.UserInformation, "#################################################################");
                }

                if (Trinity.IsPluginEnabled && trinityRoutineSelected && BotMain.IsRunning)
                {
                    PassedAllChecks = true;
                }

                Thread.Sleep(250);
            }

            Logger.LogDebug("Plugin and Routine checks passed!");
        }


    }
}
