using GilesTrinity.Technicals;
using System;
using System.Collections.Generic;
using System.Linq;
using Zeta;
using Zeta.Common;
using Zeta.Common.Plugins;
using Zeta.CommonBot;
using Zeta.CommonBot.Profile;
using Zeta.Internals.Actors;
using Zeta.Navigation;
using Zeta.Pathfinding;
using GilesTrinity.Cache;
using Zeta.Internals;

namespace GilesTrinity
{
    public partial class GilesTrinity : IPlugin
    {
        internal static int lastSceneId = -1;

        /// <summary>
        /// Find fresh targets, start main BehaviorTree if needed, cast any buffs needed etc.
        /// </summary>
        /// <param name="ret"></param>
        /// <returns></returns>
        internal static bool CheckHasTarget(object ret)
        {
            using (new PerformanceLogger("GilesTrinity.GilesGlobalOverlord"))
            {
                using (new PerformanceLogger("GilesGlobalOverlord.Validation"))
                {

                    // If we aren't in the game or a world is loading, don't do anything yet
                    if (!ZetaDia.IsInGame || !ZetaDia.Me.IsValid || !ZetaDia.CPlayer.IsValid || ZetaDia.IsLoadingWorld)
                    {
                        lastChangedZigZag = DateTime.Today;
                        vPositionLastZigZagCheck = Vector3.Zero;
                        return false;
                    }
                    // Big main-bot pause button
                    if (bMainBotPaused)
                    {
                        return true;
                    }

                }
                using (new PerformanceLogger("GilesGlobalOverlord.RefreshHotBar"))
                {

                    // Store all of the player's abilities every now and then, to keep it cached and handy, also check for critical-mass timer changes etc.
                    iCombatLoops++;
                    if (!HasMappedPlayerAbilities || iCombatLoops >= 5 || bRefreshHotbarAbilities)
                    {
                        // Update the cached player's cache
                        ActorClass tempClass = ActorClass.Invalid;
                        try
                        {
                            tempClass = PlayerStatus.ActorClass;
                        }
                        catch
                        {
                            DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.GlobalHandler, "Safely handled exception trying to get character class.");
                        }

                        iCombatLoops = 0;
                        RefreshHotbar();
                        dictAbilityRepeatDelay = new Dictionary<SNOPower, int>(dictAbilityRepeatDefaults);
                        if (Settings.Combat.Wizard.CriticalMass && PlayerStatus.ActorClass == ActorClass.Wizard)
                        {
                            dictAbilityRepeatDelay[SNOPower.Wizard_FrostNova] = 25;
                            dictAbilityRepeatDelay[SNOPower.Wizard_ExplosiveBlast] = 25;
                            dictAbilityRepeatDelay[SNOPower.Wizard_DiamondSkin] = 100;
                            dictAbilityRepeatDelay[SNOPower.Wizard_SlowTime] = 6000;
                            dictAbilityRepeatDelay[SNOPower.Wizard_WaveOfForce] = 1500;
                            dictAbilityRepeatDelay[SNOPower.Wizard_MirrorImage] = 1500;
                            dictAbilityRepeatDelay[SNOPower.Wizard_Archon_ArcaneBlast] = 1500;
                            dictAbilityRepeatDelay[SNOPower.Wizard_Teleport] = 2700;
                            dictAbilityRepeatDelay[SNOPower.Wizard_Archon_SlowTime] = 1500;
                            dictAbilityRepeatDelay[SNOPower.Wizard_Archon_Teleport] = 2700;
                        }
                        if (Settings.Combat.WitchDoctor.GraveInjustice && PlayerStatus.ActorClass == ActorClass.WitchDoctor)
                        {
                            dictAbilityRepeatDelay[SNOPower.Witchdoctor_SoulHarvest] = 1000;
                            dictAbilityRepeatDelay[SNOPower.Witchdoctor_SpiritWalk] = 1000;
                            dictAbilityRepeatDelay[SNOPower.Witchdoctor_Horrify] = 1000;
                            dictAbilityRepeatDelay[SNOPower.Witchdoctor_Gargantuan] = 20000;
                            dictAbilityRepeatDelay[SNOPower.Witchdoctor_SummonZombieDog] = 20000;
                            dictAbilityRepeatDelay[SNOPower.Witchdoctor_GraspOfTheDead] = 500;
                            dictAbilityRepeatDelay[SNOPower.Witchdoctor_SpiritBarrage] = 2000;
                            dictAbilityRepeatDelay[SNOPower.Witchdoctor_Locust_Swarm] = 2000;
                            dictAbilityRepeatDelay[SNOPower.Witchdoctor_Haunt] = 2000;
                            dictAbilityRepeatDelay[SNOPower.Witchdoctor_Hex] = 3000;
                            dictAbilityRepeatDelay[SNOPower.Witchdoctor_MassConfusion] = 15000;
                            dictAbilityRepeatDelay[SNOPower.Witchdoctor_FetishArmy] = 20000;
                            dictAbilityRepeatDelay[SNOPower.Witchdoctor_BigBadVoodoo] = 20000;
                        }
                        if (Settings.Combat.Barbarian.BoonBulKathosPassive && PlayerStatus.ActorClass == ActorClass.Barbarian)
                        {
                            dictAbilityRepeatDelay[SNOPower.Barbarian_Earthquake] = 90500;
                            dictAbilityRepeatDelay[SNOPower.Barbarian_CallOfTheAncients] = 90500;
                            dictAbilityRepeatDelay[SNOPower.Barbarian_WrathOfTheBerserker] = 90500;
                        }
                        // Pick an appropriate health set etc. based on class
                        switch (PlayerStatus.ActorClass)
                        {
                            case ActorClass.Barbarian:
                                // What health % should we use a potion, or look for a globe
                                PlayerEmergencyHealthPotionLimit = Settings.Combat.Barbarian.PotionLevel;
                                PlayerEmergencyHealthGlobeLimit = Settings.Combat.Barbarian.HealthGlobeLevel;
                                PlayerKiteDistance = Settings.Combat.Barbarian.KiteLimit;
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
                            case ActorClass.WitchDoctor:
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
                }
                // Clear target current and reset key variables used during the target-handling function

                using (new PerformanceLogger("GilesGlobalOverlord.ClearBlacklist"))
                {
                    //CurrentTarget = null;
                    bDontMoveMeIAmDoingShit = false;
                    TimesBlockedMoving = 0;
                    IsAlreadyMoving = false;
                    lastMovementCommand = DateTime.Today;
                    //bAvoidDirectionBlacklisting = false;
                    IsWaitingForPower = false;
                    IsWaitingAfterPower = false;
                    IsWaitingForPotion = false;
                    wasRootedLastTick = false;

                    ClearBlacklists();
                }
                using (new PerformanceLogger("GilesGlobalOverlord.RefreshCache"))
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

                using (new PerformanceLogger("GilesGlobalOverlord.UsePotion"))
                {

                    // Pop a potion when necessary
                    if (PlayerStatus.CurrentHealthPct <= PlayerEmergencyHealthPotionLimit)
                    {
                        if (!PlayerStatus.IsIncapacitated && GilesUseTimer(SNOPower.DrinkHealthPotion))
                        {
                            ACDItem thisBestPotion = ZetaDia.Me.Inventory.Backpack.Where(i => i.IsPotion).OrderByDescending(p => p.HitpointsGranted).ThenBy(p => p.ItemStackQuantity).FirstOrDefault();
                            if (thisBestPotion != null)
                            {
                                WaitWhileAnimating(4, true);
                                ZetaDia.Me.Inventory.UseItem((thisBestPotion.DynamicId));
                            }
                            dictAbilityLastUse[SNOPower.DrinkHealthPotion] = DateTime.Now;
                            WaitWhileAnimating(3, true);
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

                if (DateTime.Now.Subtract(lastMaintenanceCheck).TotalMilliseconds > 150)
                {
                    lastMaintenanceCheck = DateTime.Now;

                    using (new PerformanceLogger("GilesGlobalOverlord.Maintenence"))
                    {

                        lastChangedZigZag = DateTime.Today;
                        vPositionLastZigZagCheck = Vector3.Zero;

                        // Out of combat buffing etc. but only if we don't want to return to town etc.
                        ACDAnimationInfo myAnimationState = ZetaDia.Me.CommonData.AnimationInfo;

                        if (!PlayerStatus.IsInTown && !IsReadyToTownRun && !ForceVendorRunASAP && myAnimationState != null
                            && myAnimationState.State != AnimationState.Attacking
                            && myAnimationState.State != AnimationState.Casting
                            && myAnimationState.State != AnimationState.Channeling)
                        {

                            bDontSpamOutofCombat = false;

                            powerBuff = AbilitySelector(false, true, false);

                            if (powerBuff.SNOPower != SNOPower.None && powerBuff.SNOPower != SNOPower.Weapon_Melee_Instant)
                            {
                                WaitWhileAnimating(4, true);
                                DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.Behavior, "Using OOC Buff: {0}", powerBuff.SNOPower.ToString());
                                ZetaDia.Me.UsePower(powerBuff.SNOPower, powerBuff.vTargetLocation, powerBuff.iTargetWorldID, powerBuff.iTargetGUID);
                                LastPowerUsed = powerBuff.SNOPower;
                                dictAbilityLastUse[powerBuff.SNOPower] = DateTime.Now;
                                WaitWhileAnimating(3, true);
                            }
                        }
                        else if (myAnimationState != null)
                        {
                            // Check if we are portalling to town, if so increase our kill radius temporarily
                            switch (myAnimationState.Current)
                            {
                                case SNOAnim.barbarian_male_HTH_Recall_Channel_01:
                                case SNOAnim.Barbarian_Female_HTH_Recall_Channel_01:
                                case SNOAnim.Monk_Male_recall_channel:
                                case SNOAnim.Monk_Female_recall_channel:
                                case SNOAnim.WitchDoctor_Male_recall_channel:
                                case SNOAnim.WitchDoctor_Female_recall_channel:
                                case SNOAnim.Wizard_Male_HTH_recall_channel:
                                case SNOAnim.Wizard_Female_HTH_recall_channel:
                                case SNOAnim.Demonhunter_Male_HTH_recall_channel:
                                case SNOAnim.Demonhunter_Female_HTH_recall_channel:
                                    iKeepKillRadiusExtendedFor = 20;
                                    timeKeepKillRadiusExtendedUntil = DateTime.Now.AddSeconds(iKeepKillRadiusExtendedFor);
                                    break;
                            }

                        }
                    }
                }
                CurrentTarget = null;

                if ((GilesTrinity.ForceVendorRunASAP || GilesTrinity.IsReadyToTownRun) && TownRun.TownRunTimerRunning())
                {
                    DbHelper.Log(TrinityLogLevel.Normal, LogCategory.UserInformation, "Waiting for town run timer", true);
                    return true;
                }

                // Ok let DemonBuddy do stuff this loop, since we're done for the moment
                //DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.GlobalHandler, sStatusText);

                return false;
            }
        }
        private static DateTime lastMaintenanceCheck = DateTime.Now;

        private static void ClearBlacklists()
        {
            // Clear the temporary blacklist every 90 seconds (default was 90)
            if (DateTime.Now.Subtract(dateSinceBlacklist90Clear).TotalSeconds > 90)
            {
                dateSinceBlacklist90Clear = DateTime.Now;
                hashRGUIDBlacklist90 = new HashSet<int>();
            }
            // Clear the full blacklist every 60 seconds (default was 60)
            if (DateTime.Now.Subtract(dateSinceBlacklist60Clear).TotalSeconds > 60)
            {
                dateSinceBlacklist60Clear = DateTime.Now;
                hashRGUIDBlacklist60 = new HashSet<int>();
                RefreshProfileBlacklists();
            }
            // Clear the temporary blacklist every 15 seconds (default was 15)
            if (DateTime.Now.Subtract(dateSinceBlacklist15Clear).TotalSeconds > 15)
            {
                dateSinceBlacklist15Clear = DateTime.Now;
                hashRGUIDBlacklist15 = new HashSet<int>();
            }
            // Clear our very short-term ignore-monster blacklist (from not being able to raycast on them or already dead units)
            if (DateTime.Now.Subtract(dateSinceBlacklist3Clear).TotalMilliseconds > 3000)
            {
                dateSinceBlacklist3Clear = DateTime.Now;
                NeedToClearBlacklist3 = false;
                hashRGUIDBlacklist3 = new HashSet<int>();
            }

        }
        /// <summary>
        /// Adds profile blacklist entries to the Giles Blacklist
        /// </summary>
        private static void RefreshProfileBlacklists()
        {
            foreach (TargetBlacklist b in Zeta.CommonBot.ProfileManager.CurrentProfile.TargetBlacklists)
            {
                if (!hashSNOIgnoreBlacklist.Contains(b.ActorId))
                {
                    DbHelper.Log(TrinityLogLevel.Verbose, LogCategory.GlobalHandler, "Adding Profile TargetBlacklist {0} to Giles Blacklists", b.ActorId);
                    hashSNOIgnoreBlacklist.Add(b.ActorId);
                }
                if (!hashActorSNOIgnoreBlacklist.Contains(b.ActorId))
                {
                    hashActorSNOIgnoreBlacklist.Add(b.ActorId);
                }
            }
        }
    }
}
