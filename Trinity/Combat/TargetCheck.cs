using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Trinity.Combat.Abilities;
using Trinity.DbProvider;
using Trinity.Helpers;
using Trinity.Technicals;
using Zeta.Bot;
using Zeta.Common.Plugins;
using Zeta.Game;
using Zeta.Game.Internals.Actors;

namespace Trinity
{
    public partial class Trinity : IPlugin
    {
        internal static int lastSceneId = -1;

        internal static Stopwatch HotbarRefreshTimer = new Stopwatch();


        private static bool TargetCheckResult(bool result, string source)
        {
            Logger.LogDebug(LogCategory.GlobalHandler, "TargetCheck returning {0}, {1}", result, source);
            return result;
        }

        /// <summary>
        /// Find fresh targets, start main BehaviorTree if needed, cast any buffs needed etc.
        /// </summary>
        /// <param name="ret"></param>
        /// <returns></returns>
        internal static bool TargetCheck(object ret)
        {
            using (new PerformanceLogger("TargetCheck"))
            {
                // If we aren't in the game or a world is loading, don't do anything yet
                if (!ZetaDia.IsInGame || !ZetaDia.Me.IsValid || ZetaDia.IsLoadingWorld)
                {
                    return TargetCheckResult(false, "Not in Game, Invalid, Or Loading World");
                }

                // We keep dying because we're spawning in AoE and next to 50 elites and we need to just leave the game
                if (DateTime.UtcNow.Subtract(LastDeathTime).TotalSeconds < 30 &&
                    ZetaDia.Me.Inventory.Equipped.Any() &&
                    ZetaDia.Me.Inventory.Equipped.Where(i => i.IsValid).Average(i => i.DurabilityPercent) < 0.05 && !ZetaDia.IsInTown)
                {
                    Logger.Log("Durability is zero, emergency leave game");
                    ZetaDia.Service.Party.LeaveGame(true);
                    Thread.Sleep(11000);
                    return TargetCheckResult(false, "Recently Died, zero durability");
                }

                if (ZetaDia.Me.IsDead)
                {
                    return TargetCheckResult(false, "Is Dead");
                }

                if (!HotbarRefreshTimer.IsRunning)
                    HotbarRefreshTimer.Start();

                if (HotbarRefreshTimer.ElapsedMilliseconds > 1000 || ShouldRefreshHotbarAbilities)
                {
                    PlayerInfoCache.RefreshHotbar();
                    // Pick an appropriate health set etc. based on class
                    switch (Player.ActorClass)
                    {
                        case ActorClass.Barbarian:
                            PlayerEmergencyHealthPotionLimit = Settings.Combat.Barbarian.PotionLevel;
                            _playerEmergencyHealthGlobeLimit = Settings.Combat.Barbarian.HealthGlobeLevel;
                            _playerHealthGlobeResource = Settings.Combat.Barbarian.HealthGlobeLevelResource;
                            CombatBase.PlayerKiteDistance = Settings.Combat.Barbarian.KiteLimit;
                            CombatBase.PlayerKiteMode = Config.Combat.KiteMode.Never;
                            break;
                        case ActorClass.Crusader:
                            PlayerEmergencyHealthPotionLimit = Settings.Combat.Crusader.PotionLevel;
                            _playerEmergencyHealthGlobeLimit = Settings.Combat.Crusader.HealthGlobeLevel;
                            _playerHealthGlobeResource = Settings.Combat.Barbarian.HealthGlobeLevelResource;
                            CombatBase.PlayerKiteDistance = 0;
                            CombatBase.PlayerKiteMode = Config.Combat.KiteMode.Never;
                            break;
                        case ActorClass.Monk:
                            PlayerEmergencyHealthPotionLimit = Settings.Combat.Monk.PotionLevel;
                            _playerEmergencyHealthGlobeLimit = Settings.Combat.Monk.HealthGlobeLevel;
                            _playerHealthGlobeResource = Settings.Combat.Barbarian.HealthGlobeLevelResource;
                            // Monks never kite :)
                            CombatBase.PlayerKiteDistance = 0;
                            CombatBase.PlayerKiteMode = Config.Combat.KiteMode.Never;
                            break;
                        case ActorClass.Wizard:
                            PlayerEmergencyHealthPotionLimit = Settings.Combat.Wizard.PotionLevel;
                            _playerEmergencyHealthGlobeLimit = Settings.Combat.Wizard.HealthGlobeLevel;
                            _playerHealthGlobeResource = Settings.Combat.Barbarian.HealthGlobeLevelResource;
                            CombatBase.PlayerKiteDistance = Settings.Combat.Wizard.KiteLimit;
                            CombatBase.PlayerKiteMode = Config.Combat.KiteMode.Always;
                            break;
                        case ActorClass.Witchdoctor:
                            PlayerEmergencyHealthPotionLimit = Settings.Combat.WitchDoctor.PotionLevel;
                            _playerEmergencyHealthGlobeLimit = Settings.Combat.WitchDoctor.HealthGlobeLevel;
                            _playerHealthGlobeResource = Settings.Combat.Barbarian.HealthGlobeLevelResource;
                            CombatBase.PlayerKiteDistance = Settings.Combat.WitchDoctor.KiteLimit;
                            CombatBase.PlayerKiteMode = Config.Combat.KiteMode.Always;
                            break;
                        case ActorClass.DemonHunter:
                            PlayerEmergencyHealthPotionLimit = Settings.Combat.DemonHunter.PotionLevel;
                            _playerEmergencyHealthGlobeLimit = Settings.Combat.DemonHunter.HealthGlobeLevel;
                            _playerHealthGlobeResource = Settings.Combat.Barbarian.HealthGlobeLevelResource;
                            CombatBase.PlayerKiteDistance = Settings.Combat.DemonHunter.KiteLimit;
                            CombatBase.PlayerKiteMode = Settings.Combat.DemonHunter.KiteMode;
                            break;
                    }
                }
                // Clear target current and reset key variables used during the target-handling function

                //CurrentTarget = null;
                DontMoveMeIAmDoingShit = false;
                _timesBlockedMoving = 0;
                IsAlreadyMoving = false;
                lastMovementCommand = DateTime.MinValue;
                _isWaitingForPower = false;
                _isWaitingAfterPower = false;
                _isWaitingForPotion = false;
                wasRootedLastTick = false;

                ClearBlacklists();

                using (new PerformanceLogger("TargetCheck.RefreshCache"))
                {
                    // Refresh Cache if needed
                    RefreshDiaObjectCache();
                }

                // We have a target, start the target handler!
                if (CurrentTarget != null)
                {
                    _isWholeNewTarget = true;
                    DontMoveMeIAmDoingShit = true;
                    _shouldPickNewAbilities = true;
                    return TargetCheckResult(true, "Current Target is not null");
                }

                // if we just opened a horadric cache, wait around to open it
                if (DateTime.UtcNow.Subtract(Composites.LastFoundHoradricCache).TotalSeconds < 5)
                    return TargetCheckResult(true, "Recently opened Horadric Cache");

                using (new PerformanceLogger("TargetCheck.OOCPotion"))
                {
                    // Pop a potion when necessary
                    if (Player.CurrentHealthPct <= PlayerEmergencyHealthPotionLimit)
                    {
                        Trinity.UsePotionIfNeededTask();
                    }
                }
                _statusText = "[Trinity] No more targets - DemonBuddy/profile management is now in control";

                if (Settings.Advanced.DebugInStatusBar && _resetStatusText)
                {
                    _resetStatusText = false;
                    BotMain.StatusText = _statusText;
                }

                // Nothing to do... do we have some maintenance we can do instead, like out of combat buffing?

                if (DateTime.UtcNow.Subtract(lastMaintenanceCheck).TotalMilliseconds > 150)
                {
                    using (new PerformanceLogger("TargetCheck.OOCBuff"))
                    {
                        lastMaintenanceCheck = DateTime.UtcNow;

                        bool isLoopingAnimation = ZetaDia.Me.LoopingAnimationEndTime > 0;

                        if (!isLoopingAnimation && !IsReadyToTownRun && !ForceVendorRunASAP)
                        {
                            BarbarianCombat.AllowSprintOOC = true;
                            DisableOutofCombatSprint = false;

                            powerBuff = AbilitySelector(false, true, false);

                            if (powerBuff.SNOPower != SNOPower.None)
                            {

                                Logger.Log(TrinityLogLevel.Verbose, LogCategory.Behavior, "Using OOC Buff: {0}", powerBuff.SNOPower.ToString());
                                if (powerBuff.WaitTicksBeforeUse > 0)
                                    BotMain.PauseFor(new TimeSpan(0, 0, 0, 0, (int)powerBuff.WaitBeforeUseDelay));
                                ZetaDia.Me.UsePower(powerBuff.SNOPower, powerBuff.TargetPosition, powerBuff.TargetDynamicWorldId, powerBuff.TargetACDGUID);
                                LastPowerUsed = powerBuff.SNOPower;
                                CacheData.AbilityLastUsed[powerBuff.SNOPower] = DateTime.UtcNow;
                                if (powerBuff.WaitTicksAfterUse > 0)
                                    BotMain.PauseFor(new TimeSpan(0, 0, 0, 0, (int)powerBuff.WaitAfterUseDelay));

                            }
                        }
                        else if (isLoopingAnimation)
                        {
                            _keepKillRadiusExtendedForSeconds = 20;
                            _timeKeepKillRadiusExtendedUntil = DateTime.UtcNow.AddSeconds(_keepKillRadiusExtendedForSeconds);
                        }
                    }
                }
                CurrentTarget = null;

                if ((Trinity.ForceVendorRunASAP || Trinity.IsReadyToTownRun) && TownRun.TownRunTimerRunning())
                {
                    Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "Waiting for town run timer (Target Check)", true);
                    return TargetCheckResult(true, "Waiting for TownRunTimer");
                }

                return TargetCheckResult(false, "End of TargetCheck");
            }
        }
        private static DateTime lastMaintenanceCheck = DateTime.UtcNow;

        private static void ClearBlacklists()
        {
            // Clear the temporary blacklist every 90 seconds (default was 90)
            if (DateTime.UtcNow.Subtract(Blacklist90LastClear).TotalSeconds > 90)
            {
                Blacklist90LastClear = DateTime.UtcNow;
                Blacklist90Seconds = new HashSet<int>();

                // Refresh profile blacklists now, just in case
                UsedProfileManager.RefreshProfileBlacklists();
            }
            // Clear the full blacklist every 60 seconds (default was 60)
            if (DateTime.UtcNow.Subtract(Blacklist60LastClear).TotalSeconds > 60)
            {
                Blacklist60LastClear = DateTime.UtcNow;
                Blacklist60Seconds = new HashSet<int>();
            }
            // Clear the temporary blacklist every 15 seconds (default was 15)
            if (DateTime.UtcNow.Subtract(Blacklist15LastClear).TotalSeconds > 15)
            {
                Blacklist15LastClear = DateTime.UtcNow;
                Blacklist15Seconds = new HashSet<int>();
            }
            // Clear our very short-term ignore-monster blacklist (from not being able to raycast on them or already dead units)
            if (DateTime.UtcNow.Subtract(Blacklist3LastClear).TotalMilliseconds > 3000)
            {
                Blacklist3LastClear = DateTime.UtcNow;
                NeedToClearBlacklist3 = false;
                Blacklist3Seconds = new HashSet<int>();
            }

        }
    }
}
