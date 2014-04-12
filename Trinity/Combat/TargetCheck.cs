using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using Trinity.Combat.Abilities;
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
                    return false;
                }

                // We keep dying because we're spawning in AoE and next to 50 elites and we need to just leave the game
                if (DateTime.UtcNow.Subtract(Trinity.LastDeathTime).TotalSeconds < 30 && ZetaDia.Me.Inventory.Equipped.Average(i => i.DurabilityPercent) < 0.05 && !ZetaDia.IsInTown)
                {
                    Logger.Log("Durability is zero, emergency leave game");
                    ZetaDia.Service.Party.LeaveGame(true);
                    Thread.Sleep(11000);
                    return false;
                }

                if (ZetaDia.Me.IsDead)
                {
                    GoldInactivity.ResetCheckGold();
                    return false;
                }
                else if (GoldInactivity.GoldInactive())
                {
                    BotMain.PauseWhile(GoldInactivity.GoldInactiveLeaveGame);
                    return false;
                }

                if (!HotbarRefreshTimer.IsRunning)
                    HotbarRefreshTimer.Start();

                if (!HasMappedPlayerAbilities || HotbarRefreshTimer.ElapsedMilliseconds > 1000 || ShouldRefreshHotbarAbilities)
                {
                    PlayerInfoCache.RefreshHotbar();
                    // Pick an appropriate health set etc. based on class
                    switch (Player.ActorClass)
                    {
                        case ActorClass.Barbarian:
                            // What health % should we use a potion, or look for a globe
                            PlayerEmergencyHealthPotionLimit = Settings.Combat.Barbarian.PotionLevel;
                            PlayerEmergencyHealthGlobeLimit = Settings.Combat.Barbarian.HealthGlobeLevel;
                            PlayerKiteDistance = Settings.Combat.Barbarian.KiteLimit;
                            break;
                        case ActorClass.Crusader:
                            // What health % should we use a potion, or look for a globe
                            PlayerEmergencyHealthPotionLimit = Settings.Combat.Crusader.PotionLevel;
                            PlayerEmergencyHealthGlobeLimit = Settings.Combat.Crusader.HealthGlobeLevel;
                            PlayerKiteDistance = 0;
                            break;
                        case ActorClass.Monk:
                            // What health % should we use a potion, or look for a globe
                            PlayerEmergencyHealthPotionLimit = Settings.Combat.Monk.PotionLevel;
                            PlayerEmergencyHealthGlobeLimit = Settings.Combat.Monk.HealthGlobeLevel;
                            // Monks never kite :)
                            PlayerKiteDistance = 0;
                            break;
                        case ActorClass.Wizard:
                            // What health % should we use a potion, or look for a globe
                            PlayerEmergencyHealthPotionLimit = Settings.Combat.Wizard.PotionLevel;
                            PlayerEmergencyHealthGlobeLimit = Settings.Combat.Wizard.HealthGlobeLevel;
                            PlayerKiteDistance = Settings.Combat.Wizard.KiteLimit;
                            break;
                        case ActorClass.Witchdoctor:
                            // What health % should we use a potion, or look for a globe
                            PlayerEmergencyHealthPotionLimit = Settings.Combat.WitchDoctor.PotionLevel;
                            PlayerEmergencyHealthGlobeLimit = Settings.Combat.WitchDoctor.HealthGlobeLevel;
                            PlayerKiteDistance = Settings.Combat.WitchDoctor.KiteLimit;
                            break;
                        case ActorClass.DemonHunter:
                            // What health % should we use a potion, or look for a globe
                            PlayerEmergencyHealthPotionLimit = Settings.Combat.DemonHunter.PotionLevel;
                            PlayerEmergencyHealthGlobeLimit = Settings.Combat.DemonHunter.HealthGlobeLevel;
                            PlayerKiteDistance = Settings.Combat.DemonHunter.KiteLimit;
                            break;
                    }
                }
                // Clear target current and reset key variables used during the target-handling function

                //CurrentTarget = null;
                bDontMoveMeIAmDoingShit = false;
                TimesBlockedMoving = 0;
                IsAlreadyMoving = false;
                lastMovementCommand = DateTime.MinValue;
                IsWaitingForPower = false;
                IsWaitingAfterPower = false;
                IsWaitingForPotion = false;
                wasRootedLastTick = false;

                ClearBlacklists();

                using (new PerformanceLogger("TargetCheck.RefreshCache"))
                {
                    // Refresh Cache if needed
                    bool CacheWasRefreshed = RefreshDiaObjectCache();
                }

                // We have a target, start the target handler!
                if (CurrentTarget != null)
                {
                    IsWholeNewTarget = true;
                    bDontMoveMeIAmDoingShit = true;
                    ShouldPickNewAbilities = true;
                    return true;
                }

                //Monk_MaintainTempestRush();


                using (new PerformanceLogger("TargetCheck.OOCPotion"))
                {
                    // Pop a potion when necessary
                    if (Player.CurrentHealthPct <= PlayerEmergencyHealthPotionLimit)
                    {
                        if (!Player.IsIncapacitated && SNOPowerUseTimer(SNOPower.DrinkHealthPotion))
                        {
                            IsWaitingForPotion = false;
                            bool hasPotion = ZetaDia.Me.Inventory.Backpack.Any(p => p.GameBalanceId == -2142362846);
                            if (hasPotion)
                            {
                                Logger.Log(TrinityLogLevel.Debug, LogCategory.Targetting, "Using Potion", 0);
                                WaitWhileAnimating(3, true);
                                //UIManager.UsePotion();
                                GameUI.SafeClickElement(GameUI.PotionButton, "Use Potion", false);

                                CacheData.AbilityLastUsed[SNOPower.DrinkHealthPotion] = DateTime.UtcNow;
                                WaitWhileAnimating(2, true);
                            }
                        }
                    }
                }
                sStatusText = "[Trinity] No more targets - DemonBuddy/profile management is now in control";

                if (Settings.Advanced.DebugInStatusBar && bResetStatusText)
                {
                    bResetStatusText = false;
                    BotMain.StatusText = sStatusText;
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
                                WaitWhileAnimating(4, true);
                                Logger.Log(TrinityLogLevel.Verbose, LogCategory.Behavior, "Using OOC Buff: {0}", powerBuff.SNOPower.ToString());
                                if (powerBuff.WaitTicksBeforeUse > 0)
                                    BotMain.PauseFor(new TimeSpan(0, 0, 0, 0, (int)powerBuff.WaitBeforeUseDelay));
                                ZetaDia.Me.UsePower(powerBuff.SNOPower, powerBuff.TargetPosition, powerBuff.TargetDynamicWorldId, powerBuff.TargetACDGUID);
                                LastPowerUsed = powerBuff.SNOPower;
                                CacheData.AbilityLastUsed[powerBuff.SNOPower] = DateTime.UtcNow;
                                if (powerBuff.WaitTicksAfterUse > 0)
                                    BotMain.PauseFor(new TimeSpan(0, 0, 0, 0, (int)powerBuff.WaitAfterUseDelay));
                                WaitWhileAnimating(3, true);
                            }
                        }
                        else if (isLoopingAnimation)
                        {
                            iKeepKillRadiusExtendedFor = 20;
                            timeKeepKillRadiusExtendedUntil = DateTime.UtcNow.AddSeconds(iKeepKillRadiusExtendedFor);
                        }
                    }
                }
                CurrentTarget = null;

                if ((Trinity.ForceVendorRunASAP || Trinity.IsReadyToTownRun) && TownRun.TownRunTimerRunning())
                {
                    Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "Waiting for town run timer (Target Check)", true);
                    return true;
                }

                // Ok let DemonBuddy do stuff this loop, since we're done for the moment
                //DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.GlobalHandler, sStatusText);

                return false;
            }
        }
        private static DateTime lastMaintenanceCheck = DateTime.UtcNow;

        private static void ClearBlacklists()
        {
            // Clear the temporary blacklist every 90 seconds (default was 90)
            if (DateTime.UtcNow.Subtract(dateSinceBlacklist90Clear).TotalSeconds > 90)
            {
                dateSinceBlacklist90Clear = DateTime.UtcNow;
                hashRGUIDBlacklist90 = new HashSet<int>();

                // Refresh profile blacklists now, just in case
                UsedProfileManager.RefreshProfileBlacklists();
            }
            // Clear the full blacklist every 60 seconds (default was 60)
            if (DateTime.UtcNow.Subtract(dateSinceBlacklist60Clear).TotalSeconds > 60)
            {
                dateSinceBlacklist60Clear = DateTime.UtcNow;
                hashRGUIDBlacklist60 = new HashSet<int>();
            }
            // Clear the temporary blacklist every 15 seconds (default was 15)
            if (DateTime.UtcNow.Subtract(dateSinceBlacklist15Clear).TotalSeconds > 15)
            {
                dateSinceBlacklist15Clear = DateTime.UtcNow;
                hashRGUIDBlacklist15 = new HashSet<int>();
            }
            // Clear our very short-term ignore-monster blacklist (from not being able to raycast on them or already dead units)
            if (DateTime.UtcNow.Subtract(dateSinceBlacklist3Clear).TotalMilliseconds > 3000)
            {
                dateSinceBlacklist3Clear = DateTime.UtcNow;
                NeedToClearBlacklist3 = false;
                hashRGUIDBlacklist3 = new HashSet<int>();
            }

        }
    }
}
