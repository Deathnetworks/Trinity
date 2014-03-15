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
    public class GoldInactivity
    {
        private static int lastKnowCoin = 0;
        private static DateTime lastCheckBag = DateTime.MinValue;
        private static DateTime lastRefreshCoin = DateTime.MinValue;

        /// <summary>
        /// Resets the gold inactivity timer
        /// </summary>
        internal static void ResetCheckGold()
        {
            lastCheckBag = DateTime.UtcNow;
            lastRefreshCoin = DateTime.UtcNow;
            lastKnowCoin = 0;
        }

        /// <summary>
        /// Determines whether or not to leave the game based on the gold inactivity timer
        /// </summary>
        /// <returns></returns>
        internal static bool GoldInactive()
        {
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
                if ((DateTime.UtcNow.Subtract(lastCheckBag).TotalSeconds < 5))
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



                lastCheckBag = DateTime.UtcNow;
                int currentcoin = Trinity.Player.Coinage;

                if (currentcoin != lastKnowCoin && currentcoin != 0)
                {
                    lastRefreshCoin = DateTime.UtcNow;
                    lastKnowCoin = currentcoin;
                }
                int notpickupgoldsec = Convert.ToInt32(DateTime.UtcNow.Subtract(lastRefreshCoin).TotalSeconds);
                if (notpickupgoldsec >= Trinity.Settings.Advanced.GoldInactivityTimer)
                {
                    Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "Gold inactivity after {0}s. Sending abort.", notpickupgoldsec);
                    lastRefreshCoin = DateTime.UtcNow;
                    lastKnowCoin = currentcoin;
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

        private static bool isLeavingGame = false;
        private static bool leaveGameInitiated = false;

        private static Stopwatch leaveGameTimer = new Stopwatch();

        /// <summary>
        /// Leaves the game if gold inactivity timer is tripped
        /// </summary>
        internal static bool GoldInactiveLeaveGame()
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
                ZetaDia.Service.Party.LeaveGame();
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
