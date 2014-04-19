using System;
using System.Diagnostics;
using System.Threading;
using Trinity.DbProvider;
using Trinity.Technicals;
using Zeta.Bot;
using Zeta.Bot.Profile;
using Zeta.Bot.Profile.Common;
using Zeta.Game;

namespace Trinity
{
    public class GoldInactivity : IDisposable
    {
        private int _LastKnowCoin = 0;
        private DateTime _LastCheckBag = DateTime.MinValue;
        private DateTime _LastRefreshCoin = DateTime.MinValue;

        private static GoldInactivity _Instance;
        public static GoldInactivity Instance { get { return _Instance ?? (_Instance = new GoldInactivity()); } }

        private Thread _WatcherThread;

        public GoldInactivity()
        {
            _WatcherThread = new Thread(GoldInactivityWorker)
            {
                Name = "GoldInactivityWorker",
                IsBackground = true,
                Priority = ThreadPriority.Lowest
            };
            _WatcherThread.Start();
        }

        public void Dispose()
        {
            try
            {
                if (_WatcherThread != null)
                    _WatcherThread.Abort();
                _WatcherThread = null;
            }
            catch { }
        }

        private void GoldInactivityWorker()
        {
            while (true)
            {
                try
                {
                    if (BotMain.IsPaused)
                    {
                        ResetCheckGold();
                    }

                }
                catch (ThreadAbortException)
                {
                    // ssh
                }
                catch (Exception ex)
                {
                    Logger.LogError("Error in GoldInactivityWatcher: {0}", ex.Message);
                }
                Thread.Sleep(1000);
            }
        }

        ~GoldInactivity()
        {
            Dispose();
        }

        /// <summary>
        /// Resets the gold inactivity timer
        /// </summary>
        internal void ResetCheckGold()
        {
            _LastCheckBag = DateTime.UtcNow;
            _LastRefreshCoin = DateTime.UtcNow;
            _LastKnowCoin = 0;
        }

        /// <summary>
        /// Determines whether or not to leave the game based on the gold inactivity timer
        /// </summary>
        /// <returns></returns>
        internal bool GoldInactive()
        {
            if (Trinity.Settings.Advanced.DisableAllMovement)
                return false;

            if (!Trinity.Settings.Advanced.GoldInactivityEnabled)
            {
                // timer isn't enabled so move along!
                ResetCheckGold();
                return false;
            }
            try
            {
                if (!ZetaDia.IsInGame)
                {
                    ResetCheckGold(); //If not in game, reset the timer
                    Logger.Log(TrinityLogLevel.Info, LogCategory.GlobalHandler, "Not in game, gold inactivity reset", 0);
                    return false;
                }
                if (ZetaDia.IsLoadingWorld)
                {
                    Logger.Log(TrinityLogLevel.Info, LogCategory.GlobalHandler, "Loading world, gold inactivity reset", 0);
                    return false;
                }
                if ((DateTime.UtcNow.Subtract(_LastCheckBag).TotalSeconds < 5))
                {
                    return false;
                }

                // sometimes bosses take a LONG time
                if (Trinity.CurrentTarget != null && Trinity.CurrentTarget.IsBoss)
                {
                    Logger.Log(TrinityLogLevel.Info, LogCategory.GlobalHandler, "Current target is boss, gold inactivity reset", 0);
                    ResetCheckGold();
                    return false;
                }

                if (TownRun.IsTryingToTownPortal())
                {
                    Logger.Log(TrinityLogLevel.Info, LogCategory.GlobalHandler, "Trying to town portal or WaitTimer tag, gold inactivity reset", 0);
                    ResetCheckGold();
                    return false;
                }
                // Don't go inactive on WaitTimer tags
                ProfileBehavior c = null;
                try
                {
                    if (ProfileManager.CurrentProfileBehavior != null)
                        c = ProfileManager.CurrentProfileBehavior;
                }
                catch { }
                if (c != null && c.GetType() == typeof(WaitTimerTag))
                {
                    Logger.Log(TrinityLogLevel.Info, LogCategory.GlobalHandler, "Wait timer tag, gold inactivity reset", 0);
                    ResetCheckGold();
                    return false;
                }



                _LastCheckBag = DateTime.UtcNow;
                int currentcoin = Trinity.Player.Coinage;

                if (currentcoin != _LastKnowCoin && currentcoin != 0)
                {
                    _LastRefreshCoin = DateTime.UtcNow;
                    _LastKnowCoin = currentcoin;
                }
                int notpickupgoldsec = Convert.ToInt32(DateTime.UtcNow.Subtract(_LastRefreshCoin).TotalSeconds);
                if (notpickupgoldsec >= Trinity.Settings.Advanced.GoldInactivityTimer)
                {
                    Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "Gold inactivity after {0}s. Sending abort.", notpickupgoldsec);
                    _LastRefreshCoin = DateTime.UtcNow;
                    _LastKnowCoin = currentcoin;
                    notpickupgoldsec = 0;
                    return true;
                }
                else if (notpickupgoldsec > 0)
                {
                    Logger.Log(TrinityLogLevel.Info, LogCategory.GlobalHandler, "Gold unchanged for {0}s", notpickupgoldsec);
                }
            }
            catch (Exception e)
            {
                Logger.Log(TrinityLogLevel.Info, LogCategory.GlobalHandler, e.Message);
            }
            Logger.Log(TrinityLogLevel.Info, LogCategory.GlobalHandler, "Gold inactivity error - no result", 0);
            return false;
        }

        private bool isLeavingGame = false;
        private bool leaveGameInitiated = false;

        private Stopwatch leaveGameTimer = new Stopwatch();

        /// <summary>
        /// Leaves the game if gold inactivity timer is tripped
        /// </summary>
        internal bool GoldInactiveLeaveGame()
        {
            if (leaveGameTimer.IsRunning && leaveGameTimer.ElapsedMilliseconds < 12000)
            {
                return true;
            }

            // Fixes a race condition crash. Zomg!
            Thread.Sleep(5000);

            if (!ZetaDia.IsInGame || !ZetaDia.Me.IsValid || ZetaDia.IsLoadingWorld)
            {
                isLeavingGame = false;
                leaveGameInitiated = false;
                Logger.Log(TrinityLogLevel.Info, LogCategory.GlobalHandler, "GoldInactiveLeaveGame called but not in game!");
                return false;
            }

            if (!BotMain.IsRunning)
            {
                return false;
            }

            if (!isLeavingGame && !leaveGameInitiated)
            {
                // Exit the game and reload the profile
                PlayerMover.LastRestartedGame = DateTime.UtcNow;
                Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "Gold Inactivity timer tripped - Anti-stuck measures exiting current game.");
                // Reload this profile
                ProfileManager.Load(Zeta.Bot.ProfileManager.CurrentProfile.Path);
                Trinity.ResetEverythingNewGame();
                isLeavingGame = true;
                return true;
            }

            if (!leaveGameInitiated && isLeavingGame)
            {
                leaveGameTimer.Start();
                ZetaDia.Service.Party.LeaveGame(true);
                Logger.Log(TrinityLogLevel.Info, LogCategory.GlobalHandler, "GoldInactiveLeaveGame initiated LeaveGame");
                return true;
            }

            if (DateTime.UtcNow.Subtract(PlayerMover.LastRestartedGame).TotalSeconds <= 12)
            {
                Logger.Log(TrinityLogLevel.Info, LogCategory.GlobalHandler, "GoldInactiveLeaveGame waiting for LeaveGame");
                return true;
            }

            isLeavingGame = false;
            leaveGameInitiated = false;
            Logger.Log(TrinityLogLevel.Info, LogCategory.GlobalHandler, "GoldInactiveLeaveGame finished");

            return false;
        }

    }
}
