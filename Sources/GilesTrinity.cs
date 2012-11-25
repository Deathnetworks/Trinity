using GilesTrinity.DbProvider;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Windows;
using Zeta;
using Zeta.Common;
using Zeta.Common.Plugins;
using Zeta.CommonBot;
using Zeta.Internals.Actors;
namespace GilesTrinity
{
    public partial class GilesTrinity : IPlugin
    {
        /// <summary>
        /// Update the cached data on the player status - health, location etc.
        /// </summary>
        private static void UpdateCachedPlayerData()
        {
            if (DateTime.Now.Subtract(playerStatus.LastUpdated).TotalMilliseconds <= 50)
                return;
            // If we aren't in the game of a world is loading, don't do anything yet
            if (!ZetaDia.IsInGame || ZetaDia.IsLoadingWorld)
                return;
            var me = ZetaDia.Me;
            if (me == null)
                return;
            try
            {
                playerStatus.LastUpdated = DateTime.Now;
                playerStatus.IsInTown = me.IsInTown;
                playerStatus.IsIncapacitated = (me.IsFeared || me.IsStunned || me.IsFrozen || me.IsBlind);
                playerStatus.IsRooted = me.IsRooted;
                playerStatus.CurrentHealthPct = me.HitpointsCurrentPct;
                playerStatus.CurrentEnergy = me.CurrentPrimaryResource;
                playerStatus.CurrentEnergyPct = playerStatus.CurrentEnergy / me.MaxPrimaryResource;
                playerStatus.Discipline = me.CurrentSecondaryResource;
                playerStatus.DisciplinePct = playerStatus.Discipline / me.MaxSecondaryResource;
                playerStatus.CurrentPosition = me.Position;
                if (playerStatus.CurrentEnergy >= iWaitingReservedAmount)
                    playerStatus.WaitingForReserveEnergy = false;
                if (playerStatus.CurrentEnergy < 20)
                    playerStatus.WaitingForReserveEnergy = true;
                playerStatus.MyDynamicID = me.CommonData.DynamicId;
                playerStatus.Level = me.Level;
            }
            catch
            {
                Logging.WriteDiagnostic("[Trinity] Safely handled exception for grabbing player data.");
            }
        }

        /// <summary>
        /// Quick and Dirty routine just to force a wait until the character is "free"
        /// </summary>
        /// <param name="maxSafetyLoops">The max safety loops.</param>
        /// <param name="waitForAttacking">if set to <c>true</c> wait for attacking.</param>
        public static void WaitWhileAnimating(int maxSafetyLoops = 10, bool waitForAttacking = false)
        {
            bool bKeepLooping = true;
            int iSafetyLoops = 0;
            while (bKeepLooping)
            {
                iSafetyLoops++;
                if (iSafetyLoops > maxSafetyLoops)
                    bKeepLooping = false;
                bool bIsAnimating = false;
                try
                {
                    ACDAnimationInfo myAnimationState = ZetaDia.Me.CommonData.AnimationInfo;
                    if (myAnimationState == null || myAnimationState.State == AnimationState.Casting || myAnimationState.State == AnimationState.Channeling)
                        bIsAnimating = true;
                    if (waitForAttacking && (myAnimationState == null || myAnimationState.State == AnimationState.Attacking))
                        bIsAnimating = true;
                }
                catch
                {
                    bIsAnimating = true;
                }
                if (!bIsAnimating)
                    bKeepLooping = false;
            }
        }

        /// <summary>
        /// Check re-use timers on skills
        /// </summary>
        /// <param name="power">The power.</param>
        /// <param name="recheck">if set to <c>true</c> check again.</param>
        /// <returns>
        /// Returns whether or not we can use a skill, or if it's on our own internal Trinity cooldown timer
        /// </returns>
        private static bool GilesUseTimer(SNOPower power, bool recheck = false)
        {
            if (DateTime.Now.Subtract(dictAbilityLastUse[power]).TotalMilliseconds >= dictAbilityRepeatDelay[power])
                return true;
            if (recheck && DateTime.Now.Subtract(dictAbilityLastUse[power]).TotalMilliseconds >= 150 && DateTime.Now.Subtract(dictAbilityLastUse[power]).TotalMilliseconds <= 600)
                return true;
            return false;
        }
        // This function checks when the spell last failed (according to D3 memory, which isn't always reliable)
        // To prevent Trinity getting stuck re-trying the same spell over and over and doing nothing else
        // No longer used but keeping this here incase I re-use it
        private static bool GilesCanRecastAfterFailure(SNOPower power, int maxRecheckTime = 250)
        {
            if (DateTime.Now.Subtract(dictAbilityLastFailed[power]).TotalMilliseconds <= maxRecheckTime)
                return false;
            return true;
        }

        // When last hit the power-manager for this - not currently used, saved here incase I use it again in the future!
        // This is a safety function to prevent spam of the CPU and time-intensive "PowerManager.CanCast" function in DB
        // No longer used but keeping this here incase I re-use it
        private static bool GilesPowerManager(SNOPower power, int maxRecheckTime)
        {
            if (DateTime.Now.Subtract(dictAbilityLastPowerChecked[power]).TotalMilliseconds <= maxRecheckTime)
                return false;
            dictAbilityLastPowerChecked[power] = DateTime.Now;
            if (PowerManager.CanCast(power))
                return true;
            return false;
        }

        // Checking for buffs and caching the buff list
        // Cache all current buffs on character
        public static void GilesRefreshBuffs()
        {
            listCachedBuffs = new List<Buff>();
            dictCachedBuffs = new Dictionary<int, int>();
            listCachedBuffs = ZetaDia.Me.GetAllBuffs().ToList();
            // Special flag for detecting the activation and de-activation of archon
            bool bThisArchonBuff = false;
            int iTempStackCount;
            // Store how many stacks of each buff we have
            foreach (Buff thisbuff in listCachedBuffs)
            {
                // Store the stack count of this buff
                if (!dictCachedBuffs.TryGetValue(thisbuff.SNOId, out iTempStackCount))
                    dictCachedBuffs.Add(thisbuff.SNOId, thisbuff.StackCount);
                // Check for archon stuff
                if (thisbuff.SNOId == (int)SNOPower.Wizard_Archon)
                    bThisArchonBuff = true;
            }
            // Archon stuff
            if (bThisArchonBuff)
            {
                if (!bHasHadArchonbuff)
                    bRefreshHotbarAbilities = true;
                bHasHadArchonbuff = true;
            }
            else
            {
                if (bHasHadArchonbuff)
                {
                    hashPowerHotbarAbilities = new HashSet<SNOPower>(hashCachedPowerHotbarAbilities);
                }
                bHasHadArchonbuff = false;
            }
            //"g_killElitePack : 1, snoid=230745" <- Noting this here incase I ever want to monitor NV stacks, this is the SNO ID code for it!
        }

        // Check if a particular buff is present
        public static bool GilesHasBuff(SNOPower power)
        {
            int id = (int)power;
            return listCachedBuffs.Any(u => u.SNOId == id);
        }

        // Returns how many stacks of a particular buff there are
        public static int GilesBuffStacks(SNOPower power)
        {
            int stacks;
            if (dictCachedBuffs.TryGetValue((int)power, out stacks))
            {
                return stacks;
            }
            return 0;
        }

        // Refresh the skills in our hotbar
        // Also caches the values after - but ONLY if we aren't in archon mode (or if this function is told NOT to cache this)
        public static void GilesRefreshHotbar(bool dontCacheThis = false)
        {
            bMappedPlayerAbilities = true;
            hashPowerHotbarAbilities = new HashSet<SNOPower>();
            for (int i = 0; i <= 5; i++)
                hashPowerHotbarAbilities.Add(ZetaDia.Me.GetHotbarPowerId((HotbarSlot)i));
            bRefreshHotbarAbilities = false;
            if (!dontCacheThis)
                hashCachedPowerHotbarAbilities = new HashSet<SNOPower>(hashPowerHotbarAbilities);
        }

        // Now Find the best ability to use
        // Special check to force re-buffing before castign archon
        private static bool CanCastArchon = false;

        /// <summary>
        /// Writes a log message (default is LogLevel=Normal)
        /// </summary>
        /// <param name="message">The message</param>
        /// <param name="isDiagnostic">If true, will be loglevel=Diagnostic, else, loglevel=Normal</param>
        internal static void Log(string message, bool isDiagnostic = false)
        {
            string totalMessage = String.Format("[Trinity] {0}", message);
            if (!isDiagnostic)
                Logging.Write(totalMessage);
            else
                Logging.WriteDiagnostic(totalMessage);
        }

        private static bool BotIsPaused()
        {
            return bMainBotPaused;
        }

        // Force town-run button
        private static void buttonTownRun_Click(object sender, RoutedEventArgs e)
        {
            if (!BotMain.IsRunning || !ZetaDia.IsInGame || ZetaDia.IsLoadingWorld)
            {
                Logging.Write("[Trinity] You can only force a town run while DemonBuddy is started and running!");
                return;
            }
            bGilesForcedVendoring = true;
            Logging.Write("[Trinity] Town-run request received, will town-run at next possible moment.");
        }
        // Pause Button
        private static void buttonPause_Click(object sender, RoutedEventArgs e)
        {
            if (bMainBotPaused)
            {
                btnPauseBot.Content = "Pause Bot";
                bMainBotPaused = false;
                bMappedPlayerAbilities = false;
                lastChangedZigZag = DateTime.Today;
                bAlreadyMoving = false;
                lastMovementCommand = DateTime.Today;
            }
            else
            {
                BotMain.PauseWhile(BotIsPaused);
                btnPauseBot.Content = "Unpause Bot";
                bMainBotPaused = true;
            }
        }

        private void GilesTrinityOnDeath(object src, EventArgs mea)
        {
            if (DateTime.Now.Subtract(lastDied).TotalSeconds > 10)
            {
                lastDied = DateTime.Now;
                iTotalDeaths++;
                iDeathsThisRun++;
                dictAbilityLastUse = new Dictionary<SNOPower, DateTime>(dictAbilityLastUseDefaults);
                vBacktrackList = new SortedList<int, Vector3>();
                iTotalBacktracks = 0;
                GilesPlayerMover.iTotalAntiStuckAttempts = 1;
                GilesPlayerMover.vSafeMovementLocation = Vector3.Zero;
                // Does Trinity need to handle deaths?
                if (iMaxDeathsAllowed > 0)
                {
                    if (iDeathsThisRun >= iMaxDeathsAllowed)
                    {
                        Logging.Write("[Trinity] You have died too many times. Now restarting the game.");
                        string sUseProfile = GilesTrinity.sFirstProfileSeen;
                        ProfileManager.Load(!string.IsNullOrEmpty(sUseProfile)
                                                ? sUseProfile
                                                : Zeta.CommonBot.Settings.GlobalSettings.Instance.LastProfile);
                        Thread.Sleep(1000);
                        GilesResetEverythingNewGame();
                        ZetaDia.Service.Games.LeaveGame();
                        Thread.Sleep(10000);
                    }
                    else
                    {
                        Logging.Write("[Trinity] I'm sorry, but I seem to have let you die :( Now restarting the current profile.");
                        ProfileManager.Load(Zeta.CommonBot.Settings.GlobalSettings.Instance.LastProfile);
                        Thread.Sleep(2000);
                    }
                }
            }
        }

        // When the bot stops, output a final item-stats report so it is as up-to-date as can be
        private void GilesTrinityHandleBotStop(IBot bot)
        {
            // Issue final reports
            OutputReport();
            vBacktrackList = new SortedList<int, Vector3>();
            iTotalBacktracks = 0;
            GilesPlayerMover.iTotalAntiStuckAttempts = 1;
            GilesPlayerMover.vSafeMovementLocation = Vector3.Zero;
            GilesPlayerMover.vOldPosition = Vector3.Zero;
            GilesPlayerMover.iTimesReachedStuckPoint = 0;
            GilesPlayerMover.timeLastRecordedPosition = DateTime.Today;
            GilesPlayerMover.timeStartedUnstuckMeasure = DateTime.Today;
            hashUseOnceID = new HashSet<int>();
            dictUseOnceID = new Dictionary<int, int>();
            dictRandomID = new Dictionary<int, int>();
            iMaxDeathsAllowed = 0;
            iDeathsThisRun = 0;
        }

        // How many total leave games, for stat-tracking?
        public static int iTotalJoinGames = 0;

        // Each time we join & leave a game, might as well clear the hashset of looked-at dropped items - just to keep it smaller
        private static void GilesTrinityOnJoinGame(object src, EventArgs mea)
        {
            iTotalJoinGames++;
            GilesResetEverythingNewGame();
        }

        // How many total leave games, for stat-tracking?
        public static int TotalLeaveGames = 0;

        // Each time we join & leave a game, might as well clear the hashset of looked-at dropped items - just to keep it smaller
        private static void GilesTrinityOnLeaveGame(object src, EventArgs mea)
        {
            TotalLeaveGames++;
            GilesResetEverythingNewGame();
        }

        public static int TotalProfileRecycles = 0;

        public static void GilesResetEverythingNewGame()
        {
            hashUseOnceID = new HashSet<int>();
            dictUseOnceID = new Dictionary<int, int>();
            iMaxDeathsAllowed = 0;
            iDeathsThisRun = 0;
            _hashsetItemStatsLookedAt = new HashSet<int>();
            _hashsetItemPicksLookedAt = new HashSet<int>();
            _hashsetItemFollowersIgnored = new HashSet<int>();
            _dictItemStashAttempted = new Dictionary<int, int>();
            hashRGUIDIgnoreBlacklist60 = new HashSet<int>();
            hashRGUIDIgnoreBlacklist90 = new HashSet<int>();
            hashRGUIDIgnoreBlacklist15 = new HashSet<int>();
            vBacktrackList = new SortedList<int, Vector3>();
            iTotalBacktracks = 0;
            bMappedPlayerAbilities = false;
            GilesPlayerMover.iTotalAntiStuckAttempts = 1;
            GilesPlayerMover.vSafeMovementLocation = Vector3.Zero;
            GilesPlayerMover.vOldPosition = Vector3.Zero;
            GilesPlayerMover.iTimesReachedStuckPoint = 0;
            GilesPlayerMover.timeLastRecordedPosition = DateTime.Today;
            GilesPlayerMover.timeStartedUnstuckMeasure = DateTime.Today;
            GilesPlayerMover.iTimesReachedMaxUnstucks = 0;
            GilesPlayerMover.iCancelUnstuckerForSeconds = 0;
            GilesPlayerMover.timeCancelledUnstuckerFor = DateTime.Today;
            // Reset all the caches
            dictGilesObjectTypeCache = new Dictionary<int, GObjectType>();
            dictGilesMonsterAffixCache = new Dictionary<int, MonsterAffixes>();
            dictGilesMaxHealthCache = new Dictionary<int, double>();
            dictGilesLastHealthCache = new Dictionary<int, double>();
            dictGilesLastHealthChecked = new Dictionary<int, int>();
            dictGilesBurrowedCache = new Dictionary<int, bool>();
            dictGilesActorSNOCache = new Dictionary<int, int>();
            dictGilesACDGUIDCache = new Dictionary<int, int>();
            dictGilesInternalNameCache = new Dictionary<int, string>();
            dictGilesGameBalanceIDCache = new Dictionary<int, int>();
            dictGilesDynamicIDCache = new Dictionary<int, int>();
            dictGilesVectorCache = new Dictionary<int, Vector3>();
            dictGilesGoldAmountCache = new Dictionary<int, int>();
            dictGilesQualityCache = new Dictionary<int, ItemQuality>();
            dictGilesQualityRechecked = new Dictionary<int, bool>();
            dictGilesPickupItem = new Dictionary<int, bool>();
            dictSummonedByID = new Dictionary<int, int>();
            dictTotalInteractionAttempts = new Dictionary<int, int>();
            listProfilesLoaded = new List<string>();
            sLastProfileSeen = "";
            sFirstProfileSeen = "";
        }
    }
    // End of main routines
}
