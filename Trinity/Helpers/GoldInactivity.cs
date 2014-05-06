using System;
using System.Diagnostics;
using System.Threading;
using Trinity.DbProvider;
using Trinity.Technicals;
using Zeta.Bot;
using Zeta.Bot.Profile;
using Zeta.Bot.Profile.Common;
using Zeta.Game;

namespace Trinity.Helpers
{
    public class GoldInactivity : IDisposable
    {
        private int _lastKnowCoin;
        private DateTime _lastCheckBag = DateTime.MinValue;
        private DateTime _lastRefreshCoin = DateTime.MinValue;

        private static GoldInactivity _instance;
        public static GoldInactivity Instance { get { return _instance ?? (_instance = new GoldInactivity()); } }

        private Thread _watcherThread;

        public GoldInactivity()
        {
            _watcherThread = new Thread(GoldInactivityWorker)
            {
                Name = "GoldInactivityWorker",
                IsBackground = true,
                Priority = ThreadPriority.Lowest
            };
            _watcherThread.Start();
        }

        public void Dispose()
        {
            try
            {
                if (_watcherThread != null)
                    _watcherThread.Abort();
                _watcherThread = null;
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
                        long pauseTicks = DateTime.UtcNow.Subtract(_lastRefreshCoin).Ticks;
                        _lastRefreshCoin = _lastRefreshCoin.AddTicks(pauseTicks);
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
            _lastCheckBag = DateTime.UtcNow;
            _lastRefreshCoin = DateTime.UtcNow;
            _lastKnowCoin = 0;
        }

        private const int CheckGoldSeconds = 5;

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

                if ((DateTime.UtcNow.Subtract(_lastCheckBag).TotalSeconds < CheckGoldSeconds))
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

                //if (TownRun.IsTryingToTownPortal())
                //{
                //    Logger.Log(TrinityLogLevel.Info, LogCategory.GlobalHandler, "Trying to town portal or WaitTimer tag, gold inactivity reset", 0);
                //    ResetCheckGold();
                //    return false;
                //}

                // Don't go inactive on WaitTimer tags
                //try
                //{
                //    if (ProfileManager.CurrentProfileBehavior != null)
                //        ;
                //}
                //catch { }
                
                //if (c != null && c.GetType() == typeof(WaitTimerTag))
                //{
                //    Logger.Log(TrinityLogLevel.Info, LogCategory.GlobalHandler, "Wait timer tag, gold inactivity reset", 0);
                //    ResetCheckGold();
                //    return false;
                //}

                _lastCheckBag = DateTime.UtcNow;
                int currentcoin = Trinity.Player.Coinage;

                if (currentcoin != _lastKnowCoin && currentcoin != 0)
                {
                    _lastRefreshCoin = DateTime.UtcNow;
                    _lastKnowCoin = currentcoin;
                }
                int notpickupgoldsec = Convert.ToInt32(DateTime.UtcNow.Subtract(_lastRefreshCoin).TotalSeconds);
                if (notpickupgoldsec >= Trinity.Settings.Advanced.GoldInactivityTimer)
                {
                    Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "Gold inactivity after {0}s. Sending abort.", notpickupgoldsec);
                    _lastRefreshCoin = DateTime.UtcNow;
                    _lastKnowCoin = currentcoin;
                    return true;
                }
                if (notpickupgoldsec > 0)
                {
                    Logger.Log(TrinityLogLevel.Info, LogCategory.GlobalHandler, "Gold unchanged for {0}s", notpickupgoldsec);
                }
            }
            catch (Exception e)
            {
                Logger.Log(TrinityLogLevel.Info, LogCategory.GlobalHandler, e.Message);
            }
            //Logger.Log(TrinityLogLevel.Info, LogCategory.GlobalHandler, "Gold inactivity error - no result", 0);
            return false;
        }

        private bool _isLeavingGame;
        private bool _leaveGameInitiated;

        private readonly Stopwatch _leaveGameTimer = new Stopwatch();

        /// <summary>
        /// Leaves the game if gold inactivity timer is tripped
        /// </summary>
        internal bool GoldInactiveLeaveGame()
        {
            if (_leaveGameTimer.IsRunning && _leaveGameTimer.ElapsedMilliseconds < 12000)
            {
                return true;
            }

            // Fixes a race condition crash. Zomg!
            Thread.Sleep(5000);

            if (!ZetaDia.IsInGame || !ZetaDia.Me.IsValid || ZetaDia.IsLoadingWorld)
            {
                _isLeavingGame = false;
                _leaveGameInitiated = false;
                Logger.Log(TrinityLogLevel.Info, LogCategory.GlobalHandler, "GoldInactiveLeaveGame called but not in game!");
                return false;
            }

            if (!BotMain.IsRunning)
            {
                return false;
            }

            if (!_isLeavingGame && !_leaveGameInitiated)
            {
                // Exit the game and reload the profile
                PlayerMover.LastRestartedGame = DateTime.UtcNow;
                Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "Gold Inactivity timer tripped - Anti-stuck measures exiting current game.");
                // Reload this profile
                ProfileManager.Load(ProfileManager.CurrentProfile.Path);
                Trinity.ResetEverythingNewGame();
                _isLeavingGame = true;
                return true;
            }

            if (!_leaveGameInitiated && _isLeavingGame)
            {
                _leaveGameTimer.Start();
                ZetaDia.Service.Party.LeaveGame(true);
                Logger.Log(TrinityLogLevel.Info, LogCategory.GlobalHandler, "GoldInactiveLeaveGame initiated LeaveGame");
                return true;
            }

            if (DateTime.UtcNow.Subtract(PlayerMover.LastRestartedGame).TotalSeconds <= 12)
            {
                Logger.Log(TrinityLogLevel.Info, LogCategory.GlobalHandler, "GoldInactiveLeaveGame waiting for LeaveGame");
                return true;
            }

            _isLeavingGame = false;
            _leaveGameInitiated = false;
            Logger.Log(TrinityLogLevel.Info, LogCategory.GlobalHandler, "GoldInactiveLeaveGame finished");

            return false;
        }

    }
}
