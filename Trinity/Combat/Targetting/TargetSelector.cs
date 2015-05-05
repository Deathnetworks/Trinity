using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trinity.Combat.Abilities;
using Trinity.DbProvider;
using Trinity.Helpers;
using Trinity.LazyCache;
using Trinity.Technicals;
using Zeta.Bot;
using Zeta.Game;
using Zeta.Game.Internals.Actors;

namespace Trinity.Combat.Targetting
{
    internal class TargetSelector
    {
        internal static int LastSceneId = -1;

        internal static Stopwatch HotbarRefreshTimer = new Stopwatch();

        private static bool TargetCheckResult(bool result, string source)
        {
            Logger.LogDebug(LogCategory.GlobalHandler, "TargetCheck returning {0}, {1}", result, source);
            return result;
        }

        internal static TrinityCacheObject CurrentTarget { get { return Trinity.CurrentTarget; } }

        /// <summary>
        /// Determine if a valid target is available and should be engaged/interacted with/moved to etc.    
        /// </summary>
        internal static bool IsValidTarget(object ret)
        {
            using (new PerformanceLogger("TargetCheck"))
            {
                if (CacheManager.Me.IsDead)
                {
                    return TargetCheckResult(false, "Is Dead");
                }

                Trinity.TimesBlockedMoving = 0;
                Trinity.IsAlreadyMoving = false;
                Trinity.LastMovementCommand = DateTime.MinValue;
                Trinity.IsWaitingForPower = false;
                Trinity.IsWaitingAfterPower = false;
                Trinity.IsWaitingForPotion = false;
                Trinity.WasRootedLastTick = false;

                ClearBlacklists();

                //using (new PerformanceLogger("TargetCheck.RefreshCache"))
                //{
                //    // Refresh Cache if needed
                //    RefreshDiaObjectCache();
                //}

                // We have a target, start the target handler!
                if (Trinity.CurrentTarget != null)
                {
                    Trinity.ShouldPickNewAbilities = true;
                    return TargetCheckResult(true, "Current Target is not null");
                }

                MonkCombat.RunOngoingPowers();

                // if we just opened a horadric cache, wait around to open it
                if (DateTime.UtcNow.Subtract(Composites.LastFoundHoradricCache).TotalSeconds < 5)
                    return TargetCheckResult(true, "Recently opened Horadric Cache");

                using (new PerformanceLogger("TargetCheck.OOCPotion"))
                {
                    // Pop a potion when necessary
                    if (CacheManager.Me.CurrentHealthPct <= CombatBase.EmergencyHealthPotionLimit)
                    {
                        Trinity.UsePotionIfNeededTask();
                    }
                }

                Trinity.StatusText = "[Trinity] No more targets - DemonBuddy/profile management is now in control";

                if (Trinity.Settings.Advanced.DebugInStatusBar && Trinity.ResetStatusText)
                {
                    Trinity.ResetStatusText = false;
                    BotMain.StatusText = Trinity.StatusText;
                }

                // Nothing to do... do we have some maintenance we can do instead, like out of combat buffing?

                if (DateTime.UtcNow.Subtract(_lastMaintenanceCheck).TotalMilliseconds > 150)
                {
                    using (new PerformanceLogger("TargetCheck.OOCBuff"))
                    {
                        _lastMaintenanceCheck = DateTime.UtcNow;

                        bool isLoopingAnimation = ZetaDia.Me.LoopingAnimationEndTime > 0;

                        if (!isLoopingAnimation && !Trinity.WantToTownRun && !Trinity.ForceVendorRunASAP)
                        {
                            BarbarianCombat.AllowSprintOOC = true;
                            Trinity.DisableOutofCombatSprint = false;

                            var powerBuff = Trinity.AbilitySelector(UseOOCBuff: true);

                            if (powerBuff.SNOPower != SNOPower.None)
                            {

                                Logger.Log(TrinityLogLevel.Verbose, LogCategory.Behavior, "Using OOC Buff: {0}", powerBuff.SNOPower.ToString());
                                ZetaDia.Me.UsePower(powerBuff.SNOPower, powerBuff.TargetPosition, powerBuff.TargetDynamicWorldId, powerBuff.TargetACDGUID);
                                Trinity.LastPowerUsed = powerBuff.SNOPower;
                                CacheData.AbilityLastUsed[powerBuff.SNOPower] = DateTime.UtcNow;

                                // Monk Stuffs get special attention
                                {
                                    if (powerBuff.SNOPower == SNOPower.Monk_TempestRush)
                                        MonkCombat.LastTempestRushLocation = CombatBase.CurrentPower.TargetPosition;
                                    if (powerBuff.SNOPower == SNOPower.Monk_SweepingWind)
                                        MonkCombat.LastSweepingWindRefresh = DateTime.UtcNow;
                                }

                            }
                        }
                        else if (isLoopingAnimation)
                        {
                            Trinity.KeepKillRadiusExtendedForSeconds = 20;
                            Trinity.TimeKeepKillRadiusExtendedUntil = DateTime.UtcNow.AddSeconds(Trinity.KeepKillRadiusExtendedForSeconds);
                        }
                    }
                }

                Trinity.CurrentTarget = null;

                if ((Trinity.ForceVendorRunASAP || Trinity.WantToTownRun) && TownRun.TownRunTimerRunning())
                {
                    Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "Waiting for town run timer (Target Check)", true);
                    return TargetCheckResult(true, "Waiting for TownRunTimer");
                }

                return TargetCheckResult(false, "End of TargetCheck");
            }
        }
        private static DateTime _lastMaintenanceCheck = DateTime.UtcNow;

        private static void ClearBlacklists()
        {
            // Clear the temporary blacklist every 90 seconds (default was 90)
            if (DateTime.UtcNow.Subtract(Trinity.Blacklist90LastClear).TotalSeconds > 90)
            {
                Trinity.Blacklist90LastClear = DateTime.UtcNow;
                Trinity.Blacklist90Seconds = new HashSet<int>();

                // Refresh profile blacklists now, just in case
                UsedProfileManager.RefreshProfileBlacklists();
            }
            // Clear the full blacklist every 60 seconds (default was 60)
            if (DateTime.UtcNow.Subtract(Trinity.Blacklist60LastClear).TotalSeconds > 60)
            {
                Trinity.Blacklist60LastClear = DateTime.UtcNow;
                Trinity.Blacklist60Seconds = new HashSet<int>();
            }
            // Clear the temporary blacklist every 15 seconds (default was 15)
            if (DateTime.UtcNow.Subtract(Trinity.Blacklist15LastClear).TotalSeconds > 15)
            {
                Trinity.Blacklist15LastClear = DateTime.UtcNow;
                Trinity.Blacklist15Seconds = new HashSet<int>();
            }
            // Clear our very short-term ignore-monster blacklist (from not being able to raycast on them or already dead units)
            if (DateTime.UtcNow.Subtract(Trinity.Blacklist3LastClear).TotalMilliseconds > 3000)
            {
                Trinity.Blacklist3LastClear = DateTime.UtcNow;
                Trinity.NeedToClearBlacklist3 = false;
                Trinity.Blacklist3Seconds = new HashSet<int>();
            }

        }
    }

}
