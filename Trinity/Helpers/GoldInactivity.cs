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
    public class GoldInactivity
    {
        private int _lastGoldAmount;
        private DateTime _lastCheckBag = DateTime.MinValue;
        private DateTime _lastFoundGold = DateTime.MinValue;

        private static GoldInactivity _instance;
        public static GoldInactivity Instance { get { return _instance ?? (_instance = new GoldInactivity()); } }

        /// <summary>
        /// Resets the gold inactivity timer
        /// </summary>
        internal void ResetCheckGold()
        {
            Logger.LogDebug(LogCategory.GlobalHandler, "Resetting Gold Timer, Last gold changed from {0} to {1}", _lastGoldAmount, Trinity.Player.Coinage);
            
            _lastCheckBag = DateTime.UtcNow;
            _lastFoundGold = DateTime.UtcNow;
            _lastGoldAmount = 0;
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
                    Logger.Log("Not in game, gold inactivity reset", 0);
                    ResetCheckGold(); //If not in game, reset the timer
                    return false;
                }
                if (ZetaDia.IsLoadingWorld)
                {
                    Logger.Log("Loading world, gold inactivity reset");
                    return false;
                }

                if ((DateTime.UtcNow.Subtract(_lastCheckBag).TotalSeconds < CheckGoldSeconds))
                {
                    return false;
                }
                _lastCheckBag = DateTime.UtcNow;

                // sometimes bosses take a LONG time
                if (Trinity.CurrentTarget != null && Trinity.CurrentTarget.IsBoss)
                {
                    Logger.Log("Current target is boss, gold inactivity reset");
                    ResetCheckGold();
                    return false;
                }

                if (Trinity.Player.Coinage != _lastGoldAmount && Trinity.Player.Coinage != 0)
                {
                    Logger.LogVerbose(LogCategory.GlobalHandler, "Gold Changed from {0} to {1}", _lastGoldAmount, Trinity.Player.Coinage);
                    _lastFoundGold = DateTime.UtcNow;
                    _lastGoldAmount = Trinity.Player.Coinage;
                }

                int goldUnchangedSeconds = Convert.ToInt32(DateTime.UtcNow.Subtract(_lastFoundGold).TotalSeconds);
                if (goldUnchangedSeconds >= Trinity.Settings.Advanced.GoldInactivityTimer)
                {
                    Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "Gold inactivity after {0}s. Sending abort.", goldUnchangedSeconds);
                    _lastFoundGold = DateTime.UtcNow;
                    _lastGoldAmount = Trinity.Player.Coinage;
                    return true;
                }
                if (goldUnchangedSeconds > 0)
                {
                    Logger.Log(LogCategory.GlobalHandler, "Gold unchanged for {0}s", goldUnchangedSeconds);
                }
            }
            catch (Exception e)
            {
                Logger.Log(LogCategory.GlobalHandler, "Error in GoldInactivity: " + e.Message);
            }

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
