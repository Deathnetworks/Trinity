using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Trinity;
using Trinity.Combat.Abilities;
using Trinity.Config.Combat;
using Trinity.DbProvider;
using Trinity.LazyCache;
using Trinity.Reference;
using Trinity.Technicals;
using Zeta.Bot.Profile.Common;
using Zeta.Bot;
using Trinity.Settings;
using Zeta.Game;

namespace Trinity.Combat
{
    /// <summary>
    /// Situational factors that influence weighting, combat and targetting decisions.
    /// These are generally not related to a single object
    /// </summary>
    public static class CombatContext
    {
        private static CacheField<bool> _isHealthGlobeEmergency = new CacheField<bool>(UpdateSpeed.Fast);
        private static CacheField<bool> _shouldPrioritizeShrines = new CacheField<bool>(UpdateSpeed.Fast);
        private static CacheField<bool> _shouldPrioritizeContainers = new CacheField<bool>(UpdateSpeed.Fast);
        private static CacheField<bool> _isProfileTagNonCombat = new CacheField<bool>(UpdateSpeed.Fast);
        private static CacheField<bool> _isKillBounty = new CacheField<bool>(UpdateSpeed.Fast);
        private static CacheField<bool> _shouldIgnoreElites = new CacheField<bool>(UpdateSpeed.Fast);
        private static CacheField<bool> _inQuestArea = new CacheField<bool>(UpdateSpeed.Fast);
        private static CacheField<bool> _shouldIgnoreTrashMobs = new CacheField<bool>(UpdateSpeed.Fast);
        private static CacheField<bool> _shouldIgnoreBosses = new CacheField<bool>(UpdateSpeed.Fast);
        private static CacheField<int> _eliteCount = new CacheField<int>(UpdateSpeed.Fast);
        private static CacheField<int> _validAvoidanceCount = new CacheField<int>(UpdateSpeed.Fast);
        private static CacheField<bool> _avoidanceNearby = new CacheField<bool>(UpdateSpeed.Fast);
        private static CacheField<bool> _prioritizeCloseRangeUnits = new CacheField<bool>(UpdateSpeed.Fast);
        private static CacheField<bool> _inActiveEvent = new CacheField<bool>(UpdateSpeed.Fast);
        private static CacheField<bool> _hasEventInspectionTask = new CacheField<bool>(UpdateSpeed.Fast);
        private static CacheField<bool> _shouldKamakaziGoblins = new CacheField<bool>(UpdateSpeed.Fast);
        private static CacheField<float> _killRadius = new CacheField<float>(UpdateSpeed.Fast);

        /// <summary>
        /// If our health is low enough that we should prioritize picking up health globes
        /// </summary>
        public static bool IsHealthGlobeEmergency
        {
            get
            {
                if (_isHealthGlobeEmergency.IsCacheValid) 
                    return _isHealthGlobeEmergency.CachedValue;

                var isHealthGlobeEmergency = (CacheManager.Me.CurrentHealthPct <= CombatBase.EmergencyHealthGlobeLimit || 
                                              CacheManager.Me.PrimaryResourcePct <= CombatBase.HealthGlobeResource) &&
                                              CacheManager.Globes.Any() && Trinity.Settings.Combat.Misc.HiPriorityHG;

                return _isHealthGlobeEmergency.CachedValue = isHealthGlobeEmergency;
            }
        }
       
        /// <summary>
        /// If bot should risk death to kill a nearby goblin
        /// </summary>
        public static bool ShouldKamakaziGoblins
        {
            get
            {
                if (_shouldKamakaziGoblins.IsCacheValid)
                    return _shouldKamakaziGoblins.CachedValue;

                return _shouldKamakaziGoblins.CachedValue = CacheManager.Goblins.Any() && Trinity.Settings.Combat.Misc.GoblinPriority == GoblinPriority.Kamikaze;
            }
        }

        /// <summary>
        /// Current quest step
        /// </summary>
        public static float KillRadius
        {
            get
            {
                if (_killRadius.IsCacheValid) return _killRadius.CachedValue;

                var eliteRadius = 0f;
                var trashRadius = 0f;

                if (CacheManager.EliteRareUniqueBoss.Any())
                    eliteRadius = Trinity.Settings.Combat.Misc.EliteRange;

                if (CacheManager.Trash.Any())
                    trashRadius = Trinity.Settings.Combat.Misc.NonEliteRange;
                
                return _killRadius.CachedValue = Math.Min(trashRadius,eliteRadius);
            }
        }

        /// <summary>
        /// If its a priority to click on nearby shrines
        /// </summary>
        public static bool ShouldPrioritizeShrines
        {
            get
            {
                if (_shouldPrioritizeShrines.IsCacheValid)
                    return _shouldPrioritizeShrines.CachedValue;

                return _shouldPrioritizeShrines.CachedValue = Trinity.Settings.WorldObject.HiPriorityShrines && CacheManager.Globes.Any();
            }
        }

        /// <summary>
        /// If its a priority to open containers
        /// </summary>
        public static bool ShouldPrioritizeContainers
        {
            get
            {
                if (_shouldPrioritizeContainers.IsCacheValid)
                    return _shouldPrioritizeContainers.CachedValue;

                var result = Trinity.Settings.WorldObject.HiPriorityContainers && CacheManager.Containers.Any() &&
                                      !(Legendary.HarringtonWaistguard.IsEquipped && Legendary.HarringtonWaistguard.IsBuffActive);

                return _shouldPrioritizeContainers.CachedValue = result;
            }
        }

        /// <summary>
        /// If current profile tag should not be interrupted
        /// </summary>
        public static bool IsProfileTagNonCombat
        {
            get
            {
                if (_isProfileTagNonCombat.IsCacheValid)
                    return _isProfileTagNonCombat.CachedValue;

                var result = false;
                if (ProfileManager.CurrentProfileBehavior != null)
                {
                    var behaviorType = ProfileManager.CurrentProfileBehavior.GetType();
                    var behaviorName = behaviorType.Name;
                    if (!Trinity.Settings.Combat.Misc.ProfileTagOverride && CombatBase.IsQuestingMode ||
                        behaviorType == typeof(WaitTimerTag) ||
                        behaviorType == typeof(UseTownPortalTag) ||
                        behaviorName.ToLower().Contains("townrun") ||
                        behaviorName.ToLower().Contains("townportal"))
                    {
                        result = true;
                    }
                }

                return _isProfileTagNonCombat.CachedValue = result;            
            }
        }

        /// <summary>
        /// If we're currently on a bounty that requires a mosnter to be killed
        /// </summary>
        public static bool IsKillBounty
        {
            get
            {
                if (_isKillBounty.IsCacheValid)
                    return _isKillBounty.CachedValue;

                var result = !CacheManager.Me.InTieredLootRun &&
                             CacheManager.Me.ActiveBounty != null &&
                             CacheManager.Me.ActiveBounty.Info.KillCount > 0;

                return _isKillBounty.CachedValue = result;
            }
        }

        public static bool InActiveEvent
        {
            get
            {
                if (_inActiveEvent.IsCacheValid)
                    return _inActiveEvent.CachedValue;

                return _inActiveEvent.CachedValue = ZetaDia.ActInfo.ActiveQuests.Any(q => DataDictionary.EventQuests.Contains(q.QuestSNO) && q.QuestStep != 13);

            }
        }

        public static bool HasEventInspectionTask
        {
            get
            {
                if (_hasEventInspectionTask.IsCacheValid)
                    return _hasEventInspectionTask.CachedValue;

                return _hasEventInspectionTask.CachedValue = ZetaDia.ActInfo.ActiveQuests.Any(q => DataDictionary.EventQuests.Contains(q.QuestSNO) && q.QuestStep == 13);

            }
        }

        public static bool ShouldIgnoreElites
        {
            get
            {
                if (_shouldIgnoreElites.IsCacheValid)
                    return _shouldIgnoreElites.CachedValue;

                var result = (!(IsKillBounty || InActiveEvent) &&
                                   !CombatBase.IsQuestingMode &&
                                   !DataDictionary.RiftWorldIds.Contains(CacheManager.Me.WorldId) &&
                                   !DataDictionary.QuestLevelAreaIds.Contains(CacheManager.Me.LevelAreaId) &&
                                   !IsProfileTagNonCombat &&
                                   !TownRun.IsTryingToTownPortal() &&
                                   CombatBase.IgnoringElites);

                return _shouldIgnoreElites.CachedValue = result;

            }
        }

        public static bool InQuestArea
        {
            get
            {
                if (_inQuestArea.IsCacheValid)
                    return _inQuestArea.CachedValue;

                return _inQuestArea.CachedValue = DataDictionary.QuestLevelAreaIds.Contains(CacheManager.Me.LevelAreaId);

            }
        }

        public static bool ShouldIgnoreTrashMobs
        {
            get
            {
                if (_shouldIgnoreTrashMobs.IsCacheValid)
                    return _shouldIgnoreTrashMobs.CachedValue;

                var result = (!(IsKillBounty || InActiveEvent) &&
                              !CombatBase.IsQuestingMode &&
                              !InQuestArea &&
                              CacheManager.Me.TieredLootRunLevel != 0 && // Rift Trials
                              !TownRun.IsTryingToTownPortal() &&
                              !IsProfileTagNonCombat &&
                              PlayerMover.MovementSpeed >= 1 &&
                              Trinity.Settings.Combat.Misc.TrashPackSize > 1 &&
                              CacheManager.Me.Level >= 15 &&
                              CacheManager.Me.CurrentHealthPct > 0.10 &&
                              DateTime.UtcNow.Subtract(PlayerMover.LastRecordedAnyStuck).TotalMilliseconds > 500);

                return _shouldIgnoreTrashMobs.CachedValue = result;
            }
        }

        public static bool ShouldIgnoreBosses
        {
            get
            {
                if (_shouldIgnoreBosses.IsCacheValid)
                    return _shouldIgnoreBosses.CachedValue;

                return _shouldIgnoreBosses.CachedValue = IsHealthGlobeEmergency || ShouldPrioritizeShrines || ShouldPrioritizeContainers;                
            }
        }

        /// <summary>
        /// 'Elite' in the broader sense, includes settings check.
        /// </summary>
        public static int ValidEliteCount
        {
            get
            {
                if (_eliteCount.IsCacheValid)
                    return _eliteCount.CachedValue;

                return _eliteCount.CachedValue = CombatBase.IgnoringElites ? 0 : CacheManager.Units.Count(u => u.IsBossOrEliteRareUnique);                
            }
        }

        /// <summary>
        /// Includes settings check and avoidances within 50f of player;
        /// </summary>
        public static int ValidAvoidanceCount
        {
            get
            {
                if (_validAvoidanceCount.IsCacheValid)
                    return _validAvoidanceCount.CachedValue;

                return _validAvoidanceCount.CachedValue = Trinity.Settings.Combat.Misc.AvoidAOE ? 0 : CacheManager.Avoidances.Count(o => o.ShouldAvoid);
            }
        }

        /// <summary>
        /// Avoidances within 15f of player
        /// </summary>
        public static bool AvoidanceNearby
        {
            get
            {               
                if (_avoidanceNearby.IsCacheValid)
                    return _avoidanceNearby.CachedValue;

                return _avoidanceNearby.CachedValue = Trinity.Settings.Combat.Misc.AvoidAOE && CacheManager.Avoidances.Any(o => o.Distance <= 15f);
            }
        }

        public static bool PrioritizeCloseRangeUnits
        {
            get
            {
                if (_prioritizeCloseRangeUnits.IsCacheValid)
                    return _prioritizeCloseRangeUnits.CachedValue;

                //todo: apply timer logic to forcing close range units for a period after its turned on.
                var forceCloseRangeTargets = false;

                var result = (AvoidanceNearby || forceCloseRangeTargets || CacheManager.Me.IsRooted || 
                             DateTime.UtcNow.Subtract(PlayerMover.LastRecordedAnyStuck).TotalMilliseconds < 1000 &&
                             CacheManager.Units.Count(u => u.RadiusDistance < 10f) >= 3);

                return _prioritizeCloseRangeUnits.CachedValue = result;
            }
        }

    }
}
