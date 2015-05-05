using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Trinity.Combat.Abilities;
using Trinity.Combat.Targetting;
using Trinity.Config.Combat;
using Trinity.DbProvider;
using Trinity.Items;
using Trinity.LazyCache;
using Trinity.Reference;
using Trinity.Technicals;
using Zeta.Bot.Settings;
using Zeta.Common;
using Zeta.Game;
using Zeta.Game.Internals.Actors;
using Zeta.Game.Internals.Actors.Gizmos;
using Zeta.Game.Internals.SNO;
using Logger = Trinity.Technicals.Logger;

namespace Trinity.Combat
{
    /// <summary>
    /// Calculation for each object used for prioritising targets
    /// </summary>
    public static class Weighting
    {
        const double MaxWeight = 50000d;

        /// <summary>
        /// Calculate a weight for a TrinityObject
        /// </summary>
        public static double CalculateWeight(TrinityObject trinityObject, out List<Weight> outFactors)
        {
            var weightFactors = new List<Weight>();

            switch (trinityObject.TrinityType)
            {
                 case TrinityObjectType.Unit:
                    weightFactors.AddRange(UnitWeighting(trinityObject));                   
                    break;

                 case TrinityObjectType.HotSpot:
                    weightFactors.AddRange(HotSpotWeighting(trinityObject));
                    break;

                 case TrinityObjectType.Item:
                    weightFactors.AddRange(ItemWeighting(trinityObject));
                    break;

                 case TrinityObjectType.Gold:
                    weightFactors.AddRange(GoldWeighting(trinityObject));
                    break;

                 case TrinityObjectType.PowerGlobe:
                    weightFactors.AddRange(PowerGlobeWeighting(trinityObject));
                    break;

                 case TrinityObjectType.HealthGlobe:
                    weightFactors.AddRange(HealthGlobeWeighting(trinityObject));
                    break;

                 case TrinityObjectType.ProgressionGlobe:
                    weightFactors.AddRange(ProgressionGlobeWeighting(trinityObject));
                    break;

                 case TrinityObjectType.HealthWell:
                    weightFactors.AddRange(HealthWellWeighting(trinityObject));
                    break;

                 case TrinityObjectType.CursedShrine:
                    weightFactors.Add(new Weight(5000, WeightMethod.Add, WeightReason.Event));
                    break;

                 case TrinityObjectType.Shrine:
                    weightFactors.AddRange(ShrineWeighting(trinityObject));
                    break;

                 case TrinityObjectType.Door:
                    weightFactors.AddRange(DoorWeighting(trinityObject));
                    break;

                 case TrinityObjectType.Barricade:
                    weightFactors.AddRange(BarricadeWeighting(trinityObject));
                    break;

                 case TrinityObjectType.Destructible:
                    weightFactors.AddRange(DestructibleWeighting(trinityObject));
                    break;

                 case TrinityObjectType.Interactable:
                    weightFactors.AddRange(InteractableWeighting(trinityObject));
                    break;

                 case TrinityObjectType.Container:
                    weightFactors.AddRange(ContainerWeighting(trinityObject));
                    break;  
            }

            var finalWeight = CombineWeights(weightFactors);

            // Prevent current target dynamic ranged weighting flip-flop 
            if (trinityObject.IsLastTarget && finalWeight <= 1 && !trinityObject.IsBlocking)
            {
                weightFactors.Add(new Weight(100, WeightMethod.Set, WeightReason.AntiFlipFlop));
                finalWeight = CombineWeights(weightFactors);
            }
                
            // Spurt the history of our work for debugging etc
            outFactors = weightFactors;

            return finalWeight;
        }

        /// <summary>
        /// Combine a collection of weights to arrive at a final weight
        /// </summary>
        public static double CombineWeights(List<Weight> weightFactors)
        {
            double finalWeight = 0f;

            // Remove any weights created with empty constructor
            weightFactors.RemoveAll(wf => wf.Method == WeightMethod.None);

            foreach (var w in weightFactors)
            {
                switch (w.Method)
                {
                    case WeightMethod.Add:
                        finalWeight = finalWeight + w.Amount;
                        break;

                    case WeightMethod.Subtract:
                        finalWeight = finalWeight - w.Amount;
                        break;

                    case WeightMethod.Multiply:
                        finalWeight = finalWeight*w.Amount;
                        break;

                    case WeightMethod.Set:
                        finalWeight = w.Amount;
                        break;
                }

                if (w.IsFinal)
                    break;
            }
            return (float)Math.Min(finalWeight, MaxWeight);
        }

        /// <summary>
        /// Weighting for Health Globes
        /// </summary>
        private static IEnumerable<Weight> HealthGlobeWeighting(TrinityObject cacheObject)
        {
            var weightFactors = new List<Weight>();
            var item = cacheObject as TrinityItem;
            if (item == null)
            {
                weightFactors.Add(new Weight(0, WeightMethod.Set, WeightReason.InvalidType));
                return weightFactors;
            }

            var highPrioritySetting = Trinity.Settings.Combat.Misc.HiPriorityHG;

            // Can't be reached.
            if (cacheObject.IsBlocking)
            {
                weightFactors.Add(new Weight(0, WeightMethod.Set, WeightReason.NavBlocking));
                return weightFactors;
            }

            if (!Trinity.Settings.Combat.Misc.CollectHealthGlobe)
            {
                weightFactors.Add(new Weight(0, WeightMethod.Set, WeightReason.DisabledInSettings));
            }

            // WD's logic with Gruesome Feast passive,
            // mostly for intelligence stacks, 10% per globe
            // 1200 by default
            bool wdGruesomeFeast =
                CacheManager.Me.ActorClass == ActorClass.Witchdoctor &&
                CacheManager.Me.CurrentPrimaryResource <= V.F("WitchDoctor.ManaForHealthGlobes") &&
                CacheData.Hotbar.PassiveSkills.Contains(SNOPower.Witchdoctor_Passive_GruesomeFeast);

            if (wdGruesomeFeast)
                weightFactors.Add(new Weight(10000, WeightMethod.Add, WeightReason.GruesomeFeast));


            // DH's logic with Blood Vengeance passive
            // gain amount - 30 hatred per globe
            // 100 by default
            bool dhBloodVengeance =
                CacheManager.Me.ActorClass == ActorClass.DemonHunter &&
                CacheManager.Me.CurrentPrimaryResource <= V.F("DemonHunter.HatredForHealthGlobes") &&
                CacheData.Hotbar.PassiveSkills.Contains(SNOPower.DemonHunter_Passive_Vengeance);

            if (dhBloodVengeance)
                weightFactors.Add(new Weight(10000, WeightMethod.Add, WeightReason.BloodVengeance));

            // Health is High - Non-Emergency
            if (!dhBloodVengeance && !wdGruesomeFeast && (CacheManager.Me.CurrentHealthPct > CombatBase.EmergencyHealthGlobeLimit))
            {                
                // If we're giving high priority to health globes, give it higher weight and check for resource level
                if (Trinity.Settings.Combat.Misc.HiPriorityHG)
                {
                    weightFactors.Add(new Weight(0.9 * MaxWeight, WeightMethod.Set, WeightReason.HighPrioritySetting));
                }

                // Added weight for lowest health of party member
                var minPartyHealth = (CacheManager.Players.Any(p => !p.IsMe)) ? CacheManager.Players.Where(p => !p.IsMe).Min(p => p.HitpointsCurrentPct) : 1d;                              
                if (minPartyHealth > 0d && minPartyHealth < V.D("Weight.Globe.MinPartyHealthPct"))
                    weightFactors.Add(new Weight((1d - minPartyHealth) * 5000d, WeightMethod.Add, WeightReason.LowHealthPartyMember));
            }
            else
            {
                // Ok we have globes enabled, and our health is low
                if (highPrioritySetting)
                {
                    weightFactors.Add(new Weight(MaxWeight, WeightMethod.Set, WeightReason.HighPrioritySetting));
                }
                else
                {
                    weightFactors.Add(new Weight((90f - cacheObject.RadiusDistance) / 90f * 17000d, WeightMethod.Set, WeightReason.HealthEmergency));
                }

                // Point-blank items get a weight increase
                weightFactors.TryAddWeight(cacheObject, WeightReason.CloseProximity, 3000);

                // Close items get a weight increase
                if (cacheObject.Distance <= 60f)
                    weightFactors.Add(new Weight(1500, WeightMethod.Add, WeightReason.MediumProximity));

                // Primary resource is low and we're wearing Reapers Wraps
                if (CacheManager.Me.IsInCombat && CacheManager.Me.PrimaryResourcePct < 0.3 && Legendary.ReapersWraps.IsEquipped && (TargetUtil.AnyMobsInRange(40, 5) || TargetUtil.AnyElitesInRange(40)))
                    weightFactors.Add(new Weight(3000, WeightMethod.Add, WeightReason.ReapersWraps));

                // Was already a target and is still viable, give it some free extra weight, to help stop flip-flopping between two targets
                if (cacheObject.RActorGuid == Trinity.LastTargetRactorGUID && cacheObject.Distance <= 25f)
                    weightFactors.Add(new Weight(800, WeightMethod.Add, WeightReason.PreviousTarget));
            }

            if (!highPrioritySetting)
            {
                // If there's a monster in the path-line to the item, reduce the weight by 15% for each
                foreach (var obstacle in CacheManager.Units.Where(cp => MathUtil.IntersectsPath(cp.Position, cp.Radius, CacheManager.Me.Position, cacheObject.Position)))
                {
                    if (Trinity.Settings.Combat.Misc.HiPriorityHG)
                        weightFactors.Add(new Weight(0.85, WeightMethod.Multiply, WeightReason.MonsterInLoS));
                }         
       
                // See if there's any AOE avoidance in that spot, if so reduce the weight by 10%
                if (cacheObject.Distance > 10f)
                    weightFactors.Add(new Weight(0.90, WeightMethod.Multiply, WeightReason.AvoidanceAtPosition));
            }

            // Do not collect health globes if we are kiting and health globe is too close to monster or avoidance
            if (CombatBase.KiteDistance > 0)
            {
                weightFactors.TryAddWeight(cacheObject, WeightReason.MonsterInLoS);
                weightFactors.TryAddWeight(cacheObject, WeightReason.AvoidanceInLoS);
            }

            return weightFactors;
        }

        /// <summary>
        /// Weighting for Gold
        /// </summary>
        private static IEnumerable<Weight> GoldWeighting(TrinityObject cacheObject)
        {
            var weightFactors = new List<Weight>();
            var item = cacheObject as TrinityItem;
            if (item == null)
            {
                weightFactors.Add(new Weight(0, WeightMethod.Set, WeightReason.InvalidType));
                return weightFactors;
            }

            if (weightFactors.TryAddWeight(cacheObject, WeightReason.DisableForQuest))
                return weightFactors;

            if (CacheManager.Units.Any(m => m.IsBossOrEliteRareUnique))
            {
                weightFactors.Add(new Weight(0, WeightMethod.Set, WeightReason.BossOrEliteNearby));
                return weightFactors;
            }

            weightFactors.Add(new Weight(Math.Max((175 - item.Distance) / 175 * MaxWeight, 100d), WeightMethod.Set, WeightReason.StartingWeight));
            weightFactors.TryAddWeight(cacheObject, WeightReason.PreviousTarget, 800);

            // Ignore gold in AoE
            if (Trinity.Settings.Loot.Pickup.IgnoreGoldInAoE && CacheManager.Avoidances.Any(aoe => item.Position.Distance2D(aoe.Position) <= aoe.Radius))
            {
                weightFactors.Add(new Weight(0, WeightMethod.Set, WeightReason.AvoidanceNearby));
            }
         
            if(item.ItemQuality < ItemQuality.Legendary)
                weightFactors.TryAddWeight(cacheObject, WeightReason.NoCombatLooting);

            weightFactors.TryAddWeight(cacheObject, WeightReason.MonsterInLoS);
            weightFactors.TryAddWeight(cacheObject, WeightReason.AvoidanceInLoS);

            return weightFactors;
        }

        /// <summary>
        /// Weighting for Items
        /// </summary>
        private static IEnumerable<Weight> ItemWeighting(TrinityObject cacheObject)
        {
            var weightFactors = new List<Weight>();
            var item = cacheObject as TrinityItem;
            if (item == null)
            {
                weightFactors.Add(new Weight(0, WeightMethod.Set, WeightReason.InvalidType));
                return weightFactors;
            }

            if (weightFactors.TryAddWeight(item, WeightReason.DisableForQuest))
                return weightFactors;

            if (CacheManager.Me.InTieredLootRun && CacheManager.Units.Any(m => m.IsBoss))
            {
                weightFactors.Add(new Weight(0, WeightMethod.Set, WeightReason.BossNearby));
                return weightFactors;
            }
                      
            // Default Weight
            weightFactors.Add(new Weight(Math.Max((175 - item.Distance) / 175 * MaxWeight, 100d), WeightMethod.Set, WeightReason.StartingWeight));

            // Don't pickup items if we're doing a TownRun
            if (TrinityItemManager.FindValidBackpackLocation(item.IsTwoSquareItem) == new Vector2(-1, -1))
            {
                weightFactors.Add(new Weight(0, WeightMethod.Set, WeightReason.TownRun));
                return weightFactors;
            }

            // Give legendaries max weight, always
            if (item.ItemQuality >= ItemQuality.Legendary)
            {
                weightFactors.Add(new Weight(MaxWeight, WeightMethod.Set, WeightReason.IsLegendary));
                return weightFactors;
            }

            // ignore non-legendaries and gold near elites if we're ignoring elites; not sure how we should safely determine this distance
            if (item.ItemQuality < ItemQuality.Legendary && CombatBase.IgnoringElites && CacheManager.Units.Any(u => u.IsUnit && u.IsEliteRareUnique &&
                    u.Position.Distance2D(item.Position) <= V.F("Weight.Items.IgnoreNonLegendaryNearEliteDistance")))
            {
                weightFactors.Add(new Weight(0, WeightMethod.Set, WeightReason.IgnoreElites));
                return weightFactors;
            }

            // Ignore Legendaries in AoE
            if (Trinity.Settings.Loot.Pickup.IgnoreLegendaryInAoE && item.ItemQuality >= ItemQuality.Legendary &&
                CacheManager.Avoidances.Any(aoe => cacheObject.Position.Distance2D(aoe.Position) <= aoe.Radius))
            {
                weightFactors.Add(new Weight(0, WeightMethod.Set, WeightReason.AvoidanceNearby));
                return weightFactors;
            }

            // Ignore Non-Legendaries in AoE
            if (Trinity.Settings.Loot.Pickup.IgnoreNonLegendaryInAoE && item.ItemQuality < ItemQuality.Legendary &&
                CacheManager.Avoidances.Any(aoe => cacheObject.Position.Distance2D(aoe.Position) <= aoe.Radius))
            {
                weightFactors.Add(new Weight(0, WeightMethod.Set, WeightReason.AvoidanceNearby));
                return weightFactors;
            }

            // Ignore Legendaries near Elites
            if (Trinity.Settings.Loot.Pickup.IgnoreLegendaryNearElites && item.ItemQuality >= ItemQuality.Legendary &&
                CacheManager.Units.Any(u => u.IsEliteRareUnique && u.Position.Distance2D(cacheObject.Position) <= V.F("Weight.Items.IgnoreLegendaryNearEliteDistance")))
            {
                weightFactors.Add(new Weight(0, WeightMethod.Set, WeightReason.IgnoreNearElites));
                return weightFactors;
            }
            // Ignore Non-Legendaries near Elites
            if (Trinity.Settings.Loot.Pickup.IgnoreNonLegendaryNearElites && item.ItemQuality < ItemQuality.Legendary &&
                CacheManager.Units.Any(u => u.IsEliteRareUnique && u.Position.Distance2D(cacheObject.Position) <= V.F("Weight.Items.IgnoreNonLegendaryNearEliteDistance")))
            {
                weightFactors.Add(new Weight(0, WeightMethod.Set, WeightReason.IgnoreNearElites));
                return weightFactors;
            }

            weightFactors.TryAddWeight(item, WeightReason.CloseProximity);
            weightFactors.TryAddWeight(item, WeightReason.PreviousTarget, 800);

            if (CacheManager.Me.ActorClass == ActorClass.Monk && Trinity.Hotbar.Contains(SNOPower.Monk_TempestRush) && Trinity.TimeSinceUse(SNOPower.Monk_TempestRush) < 1000 && item.ItemQuality < ItemQuality.Legendary)
            {
                weightFactors.Add(new Weight(500, WeightMethod.Set, WeightReason.MonkTR));
            }

            if (item.ItemQuality < ItemQuality.Legendary)
            {
                weightFactors.TryAddWeight(item, WeightReason.AvoidanceInLoS);
                weightFactors.TryAddWeight(item, WeightReason.MonsterInLoS);
                weightFactors.TryAddWeight(item, WeightReason.NoCombatLooting);
            }

            return weightFactors;
        }

        /// <summary>
        /// Weighting for Hotspots
        /// </summary>
        private static IEnumerable<Weight> HotSpotWeighting(TrinityObject cacheObject)
        {
            var weightFactors = new List<Weight>();

            // If there's monsters in our face, ignore
            if (CombatContext.PrioritizeCloseRangeUnits)
            {
                weightFactors.Add(new Weight(0, WeightMethod.Set, WeightReason.MonstersNearPlayer));
                return weightFactors;
            }

            // if we started cache refresh with a target already
            if (Trinity.LastTargetRactorGUID != -1)
            {
                weightFactors.Add(new Weight(0, WeightMethod.Set, WeightReason.HasTarget));
                return weightFactors;                
            }

            // If it's very close, ignore
            if (cacheObject.Distance <= V.F("Cache.HotSpot.MinDistance"))
            {
                weightFactors.Add(new Weight(0, WeightMethod.Set, WeightReason.AlreadyCloseEnough));
                return weightFactors;       
            }
            
            // Avoidance near position
            if (!CacheData.TimeBoundAvoidance.Any(aoe => aoe.Position.Distance2D(cacheObject.Position) <= aoe.Radius))
            {
                float maxDist = V.F("Cache.HotSpot.MaxDistance");
                weightFactors.Add(new Weight((maxDist - cacheObject.Distance) / maxDist * 50000d, WeightMethod.Set, WeightReason.AvoidanceAtPosition));
            }

            return weightFactors;
        }

        /// <summary>
        /// Weighting for Units
        /// </summary>
        private static IEnumerable<Weight> UnitWeighting(TrinityObject cacheObject)
        {
            var weightFactors = new List<Weight>();
            var unit = cacheObject as TrinityUnit;
            if (unit == null)
            {
                weightFactors.Add(new Weight(0, WeightMethod.Set, WeightReason.InvalidType));
                return weightFactors;
            }

            bool isInHotSpot = GroupHotSpots.CacheObjectIsInHotSpot(unit.Position);
            
            // Ignore Trash Situations
            if (unit.IsTrash)
            {
                int nearbyMonsterCount = unit.UnitsNearby;
                bool elitesInRangeOfUnit = !CombatBase.IgnoringElites && TargetUtil.AnyElitesInRangeOfPosition(unit.Position, 15f);
                bool shouldIgnoreTrashMob = CombatContext.ShouldIgnoreTrashMobs && nearbyMonsterCount < Trinity.Settings.Combat.Misc.TrashPackSize && !elitesInRangeOfUnit;
                bool ignoreSummoner = unit.IsSummoner && !Trinity.Settings.Combat.Misc.ForceKillSummoners || unit.IsBlocking;
                
                // Ignore trash mobs < 15% health or 50% health with a DoT
                if (unit.IsTrash && shouldIgnoreTrashMob &&
                    (unit.HitpointsCurrentPct < Trinity.Settings.Combat.Misc.IgnoreTrashBelowHealth ||
                        unit.HitpointsCurrentPct < Trinity.Settings.Combat.Misc.IgnoreTrashBelowHealthDoT && unit.HasDotDps) &&
                        !unit.IsQuestMonster && !unit.IsMinimapActive)
                {
                    weightFactors.Add(new Weight(0, WeightMethod.Set, WeightReason.IgnoreHealthDot));
                    return weightFactors;
                }

                // Ignore Solitary Trash mobs (no elites present)
                // Except if has been primary target or if already low on health (<= 20%)
                if ((shouldIgnoreTrashMob && !isInHotSpot &&
                        !unit.IsQuestMonster && !unit.IsMinimapActive && !ignoreSummoner &&
                        !unit.IsBountyObjective) || CombatContext.IsHealthGlobeEmergency || CombatContext.ShouldPrioritizeContainers
                        || CombatContext.ShouldPrioritizeShrines || CombatContext.ShouldKamakaziGoblins)
                {
                    weightFactors.Add(new Weight(0, WeightMethod.Set, WeightReason.IgnoreOrphanTrash));
                    return weightFactors;
                }
            }

            // Ignore Elite Situations
            if (unit.IsEliteRareUnique)
            {
                // Ignore elite option, except if trying to town portal
                if ((!unit.IsBoss || CombatContext.ShouldIgnoreBosses) && !unit.IsBountyObjective &&
                    CombatContext.ShouldIgnoreElites && unit.IsEliteRareUnique && !isInHotSpot &&
                    !(unit.HitpointsCurrentPct <= ((float)Trinity.Settings.Combat.Misc.ForceKillElitesHealth / 100))
                    || CombatContext.IsHealthGlobeEmergency || CombatContext.ShouldPrioritizeShrines || CombatContext.ShouldPrioritizeContainers || CombatContext.ShouldKamakaziGoblins)
                {
                    weightFactors.Add(new Weight(0, WeightMethod.Set, WeightReason.IgnoreElites));
                    return weightFactors;
                }
            }

            // Monster is not within kill range
            if (!unit.IsBoss && !unit.IsTreasureGoblin && Trinity.LastTargetRactorGUID != unit.RActorGuid &&
                unit.RadiusDistance > unit.KillRange && !unit.IsQuestMonster && !unit.IsBountyObjective)
            {
                weightFactors.Add(new Weight(0, WeightMethod.Set, WeightReason.NotInKillRange));
                return weightFactors;
            }

            // Ignore event monsters that are too far from event
            if (CombatContext.InActiveEvent)
            {
                var eventObject = CacheManager.Objects.FirstOrDefault(o => o.IsEventObject);
                if (eventObject != null)
                {
                    var eventObjectPosition = eventObject.Position;
                    if (!unit.IsQuestMonster && unit.Position.Distance2DSqr(eventObjectPosition) > 75 * 75)
                    {
                        weightFactors.Add(new Weight(0, WeightMethod.Set, WeightReason.TooFarFromEvent));
                        return weightFactors;
                    }                    
                }
            }

            // Ignore Dead monsters :)
            if (unit.IsDead)
            {
                weightFactors.Add(new Weight(0, WeightMethod.Set, WeightReason.DeadUnit));
                return weightFactors;
            }

            // Force a close range target because we seem to be stuck *OR* if not ranged and currently rooted
            if (CombatContext.PrioritizeCloseRangeUnits)
            {
                double rangePercent = (20d - unit.RadiusDistance) / 20d;
                weightFactors.Add(new Weight(Math.Max(rangePercent * MaxWeight, 200d), WeightMethod.Set, WeightReason.GoblinKamikaze));

                // Goblin priority KAMIKAZEEEEEEEE
                if (unit.IsTreasureGoblin && Trinity.Settings.Combat.Misc.GoblinPriority == GoblinPriority.Kamikaze)
                    weightFactors.Add(new Weight(0, WeightMethod.Set, WeightReason.GoblinKamikaze));

                return weightFactors;
            }

            // Not attackable, could be shielded, make super low priority
            if (!unit.IsAttackable || unit.HasShieldingAffix && unit.IsInvulnerable)
            {
                // Only 100 weight helps prevent it being prioritized over an unshielded
                weightFactors.Add(new Weight(100, WeightMethod.Set, WeightReason.NotAttackable));
                return weightFactors;
            }

            // Starting weight of 500 for Trash
            if (unit.IsTrash)
            {
                var startingWeight = Math.Max((unit.KillRange - unit.RadiusDistance) / unit.KillRange * 100d, 2d);
                weightFactors.Add(new Weight(startingWeight, WeightMethod.Set, WeightReason.StartingWeight));
            }

            // Elite Weight based on kill range and max possible weight
            if (unit.IsBossOrEliteRareUnique)
            {
                var startingWeight = unit.Weight = Math.Max((unit.KillRange - unit.RadiusDistance) / unit.KillRange * MaxWeight, 2000d);
                weightFactors.Add(new Weight(startingWeight, WeightMethod.Set, WeightReason.IsBossEliteRareUnique));
            }

            // Bounty Objectives goooo
            if (unit.IsBountyObjective && !unit.IsBlocking)
            {
                weightFactors.Add(new Weight(15000d, WeightMethod.Add, WeightReason.BountyObjective));
            }

            // set a minimum 100 just to make sure it's not 0
            if (CombatContext.IsKillBounty || CombatContext.InActiveEvent)
                 weightFactors.Add(new Weight(100, WeightMethod.Add, WeightReason.InActiveEvent));

            // Elites with Archon get super weight
            if (!CombatBase.IgnoringElites && CacheManager.Me.ActorClass == ActorClass.Wizard && Trinity.GetHasBuff(SNOPower.Wizard_Archon) && unit.IsBossOrEliteRareUnique)
                weightFactors.Add(new Weight(10000d, WeightMethod.Add, WeightReason.ArchonElite));

            // Monsters near other players given higher weight
            if (CacheManager.Players.Count > 1)
            {
                var group = 0d;
                foreach (var player in CacheManager.Players.Where(p => !p.IsMe))
                {
                    group += Math.Max(((55f - unit.Position.Distance2D(player.Position)) / 55f * 500d), 2d);
                }
                weightFactors.Add(new Weight(group, WeightMethod.Add, WeightReason.ElitesNearPlayers));
            }    

            // Is standing in HotSpot - focus fire!
            if (isInHotSpot)
                weightFactors.Add(new Weight(10000, WeightMethod.Add, WeightReason.InHotSpot));  

            // Give extra weight to ranged enemies
            if ((CacheManager.Me.ActorClass == ActorClass.Barbarian || CacheManager.Me.ActorClass == ActorClass.Monk) &&
                (unit.MonsterSize == MonsterSize.Ranged || DataDictionary.RangedMonsterIds.Contains(unit.ActorSNO)))
            {
                weightFactors.Add(new Weight(1100d, WeightMethod.Add, WeightReason.RangedUnit));
            }

            // Lower health gives higher weight - health is worth up to 1000ish extra weight
            if (unit.IsTrash && unit.HitpointsCurrentPct < 0.20 && unit.IsAlive)
                weightFactors.Add(new Weight(Math.Max((1 - unit.HitpointsCurrentPct) / 100 * 1000d, 100d), WeightMethod.Add, WeightReason.LowHPTrash));

            // Elites on low health get extra priority - up to 2500ish
            if (unit.IsEliteRareUnique && unit.HitpointsCurrentPct < 0.25 && unit.HitpointsCurrentPct > 0.01)
                weightFactors.Add(new Weight(Math.Max((1 - unit.HitpointsCurrentPct) / 100 * 2500d, 100d), WeightMethod.Add, WeightReason.LowHPElite));
        
            // Bonuses to priority type monsters from the dictionary/hashlist set at the top of the code
            int extraPriority;
            if (DataDictionary.MonsterCustomWeights.TryGetValue(unit.ActorSNO, out extraPriority))
            {
                // adding a constant multiple of 3 to all weights here (e.g. 999 becomes 1998)
                weightFactors.Add(new Weight(extraPriority * 2d, WeightMethod.Add, WeightReason.XtraPriority));
            }

            // Extra weight for summoners
            if (!unit.IsBoss && unit.IsSummoner)
                weightFactors.Add(new Weight(2500, WeightMethod.Add, WeightReason.Summoner));
 
            // Was already a target and is still viable, give it some free extra weight, to help stop flip-flopping between two targets
            if (unit.IsLastTarget && unit.Distance <= 25f)
                weightFactors.Add(new Weight(1000d, WeightMethod.Add, WeightReason.PreviousTarget));
          
            // Close range get higher weights the more of them there are, to prevent body-blocking
            if (!unit.IsBoss && unit.RadiusDistance <= 10f)
                weightFactors.Add(new Weight(3000d * unit.Radius, WeightMethod.Add, WeightReason.CloseProximity));

            // Special additional weight for corrupt growths in act 4 ONLY if they are at close range (not a standard priority thing)
            if ((unit.ActorSNO == 210120 || unit.ActorSNO == 210268) && unit.Distance <= 25f)
                weightFactors.Add(new Weight(5000d * unit.Radius, WeightMethod.Add, WeightReason.CorruptGrowth));

            // If standing Molten, Arcane, or Poison Tree near unit, reduce weight
            if (CombatBase.KiteDistance <= 0 && CacheManager.Avoidances.Any(aoe =>
                    (aoe.AvoidanceType == AvoidanceType.Arcane ||
                     aoe.AvoidanceType == AvoidanceType.MoltenCore ||
                     //aoe.AvoidanceType == AvoidanceType.MoltenTrail ||
                     aoe.AvoidanceType == AvoidanceType.PoisonTree) &&
                    unit.Position.Distance2D(aoe.Position) <= aoe.Radius))
            {
                weightFactors.Add(new Weight(1, WeightMethod.Set, WeightReason.InSpecialAoE));
            }

            // If any AoE between us and target, reduce weight, for melee only
            if (!Trinity.Settings.Combat.Misc.KillMonstersInAoE &&
                CombatBase.KiteDistance <= 0 && unit.RadiusDistance > 3f &&
                CacheManager.Avoidances.Any(aoe => aoe.AvoidanceType != AvoidanceType.PlagueCloud &&
                MathUtil.IntersectsPath(aoe.Position, aoe.Radius, CacheManager.Me.Position, unit.Position)))
            {
                weightFactors.Add(new Weight(1, WeightMethod.Set, WeightReason.AoEPathLine));
            }
            // See if there's any AOE avoidance in that spot, if so reduce the weight to 1, for melee only
            if (!Trinity.Settings.Combat.Misc.KillMonstersInAoE &&
                CombatBase.KiteDistance <= 0 && unit.RadiusDistance > 3f &&
                CacheData.TimeBoundAvoidance.Any(aoe => aoe.AvoidanceType != AvoidanceType.PlagueCloud &&
                unit.Position.Distance2D(aoe.Position) <= aoe.Radius))
            {
                weightFactors.Add(new Weight(1, WeightMethod.Set, WeightReason.InAoE));
            }

            // Deal with treasure goblins - note, of priority is set to "0", then the is-a-goblin flag isn't even set for use here - the monster is ignored
            // Goblins on low health get extra priority - up to 2000ish
            if (Trinity.Settings.Combat.Misc.GoblinPriority >= GoblinPriority.Prioritize && unit.IsTreasureGoblin && unit.HitpointsCurrentPct <= 0.98)
                weightFactors.Add(new Weight(Math.Max(((1 - unit.HitpointsCurrentPct) / 100) * 2000d, 100d), WeightMethod.Add, WeightReason.LowHPGoblin));

            // If this is a goblin and there's no doors or barricades blocking our path to it.
            if (unit.IsTreasureGoblin && !CacheManager.Objects.Any(obj => (obj.TrinityType == TrinityObjectType.Door || obj.TrinityType == TrinityObjectType.Barricade) && !MathUtil.IntersectsPath(obj.Position, obj.Radius, CacheManager.Me.Position, unit.Position)))
            {
                // Logging goblin sightings
                if (Trinity.LastGoblinTime == DateTime.MinValue)
                {
                    Trinity.TotalNumberGoblins++;
                    Trinity.LastGoblinTime = DateTime.UtcNow;
                    Logger.Log(TrinityLogLevel.Info, LogCategory.UserInformation, "Goblin #{0} in sight. Distance={1:0}", Trinity.TotalNumberGoblins, unit.Distance);
                }
                else
                {
                    if (DateTime.UtcNow.Subtract(Trinity.LastGoblinTime).TotalMilliseconds > 30000)
                        Trinity.LastGoblinTime = DateTime.MinValue;
                }

                // Ignore goblins in AOE
                if (CacheManager.Avoidances.Any(aoe => unit.Position.Distance2D(aoe.Position) <= aoe.Radius) && Trinity.Settings.Combat.Misc.GoblinPriority != GoblinPriority.Kamikaze)
                {
                    weightFactors.Add(new Weight(1, WeightMethod.Set, WeightReason.InAoE));
                    return weightFactors;
                }

                // Original Trinity stuff for priority handling now
                switch (Trinity.Settings.Combat.Misc.GoblinPriority)
                {
                    case GoblinPriority.Normal:
                        weightFactors.Add(new Weight(751, WeightMethod.Add, WeightReason.GoblinNormal));
                        break;

                    case GoblinPriority.Prioritize:
                        // Super-high priority option below... 
                        var gobWeight = Math.Max((unit.KillRange - unit.RadiusDistance) / unit.KillRange * MaxWeight, 1000d);
                        weightFactors.Add(new Weight(gobWeight, WeightMethod.Add, WeightReason.GoblinPriority));
                        break;
                    case GoblinPriority.Kamikaze:
                        // KAMIKAZE SUICIDAL TREASURE GOBLIN RAPE AHOY!
                        weightFactors.Add(new Weight(MaxWeight, WeightMethod.Add, WeightReason.GoblinKamikaze));
                        break;
                }
            }

            return weightFactors;
        }

        /// <summary>
        /// Weighting for Power Globes
        /// </summary>
        private static IEnumerable<Weight> PowerGlobeWeighting(TrinityObject cacheObject)
        {
            var weightFactors = new List<Weight>();
            var item = cacheObject as TrinityItem;
            if (item == null)
            {
                weightFactors.Add(new Weight(0, WeightMethod.Set, WeightReason.InvalidType));
                return weightFactors;
            }

            if (!TownRun.IsTryingToTownPortal())
            {
                weightFactors.Add(new Weight((90f - item.RadiusDistance) / 90f * 5000d, WeightMethod.Set, WeightReason.StartingWeight));
            }

            weightFactors.TryAddWeight(cacheObject, WeightReason.CloseProximity);
            weightFactors.TryAddWeight(cacheObject, WeightReason.MonsterInLoS);
            weightFactors.TryAddWeight(cacheObject, WeightReason.AvoidanceInLoS);

            return weightFactors;
        }

        /// <summary>
        /// Weighting for Progression Globes (grift thingies)
        /// </summary>
        private static IEnumerable<Weight> ProgressionGlobeWeighting(TrinityObject cacheObject)
        {
            var weightFactors = new List<Weight>();
            var item = cacheObject as TrinityItem;
            if (item == null)
            {
                weightFactors.Add(new Weight(0, WeightMethod.Set, WeightReason.InvalidType));
                return weightFactors;
            }

            if (!TownRun.IsTryingToTownPortal())
            {
                weightFactors.Add(new Weight((90f - cacheObject.RadiusDistance) / 90f * MaxWeight, WeightMethod.Set, WeightReason.StartingWeight));
            }

            weightFactors.TryAddWeight(cacheObject, WeightReason.CloseProximity);
            weightFactors.TryAddWeight(cacheObject, WeightReason.MonsterInLoS);
            weightFactors.TryAddWeight(cacheObject, WeightReason.AvoidanceInLoS);
           
            return weightFactors;
        }

        /// <summary>
        /// Weighting for Health Wells
        /// </summary>
        private static IEnumerable<Weight> HealthWellWeighting(TrinityObject cacheObject)
        {
            var weightFactors = new List<Weight>();
            var gizmo = cacheObject as TrinityGizmo;
            if (gizmo == null)
            {
                weightFactors.Add(new Weight(0, WeightMethod.Set, WeightReason.InvalidType));
                return weightFactors;
            }

            if (!CacheManager.EliteRareUniqueBoss.Any())
            {
                if (!Trinity.Settings.WorldObject.UseShrine)
                {
                    weightFactors.Add(new Weight(1, WeightMethod.Set, WeightReason.DisabledInSettings));
                    return weightFactors;
                }

                weightFactors.TryAddWeight(cacheObject, WeightReason.MonsterInLoS);
                weightFactors.TryAddWeight(cacheObject, WeightReason.AvoidanceInLoS);

                if (cacheObject.IsBlocking)
                {
                    weightFactors.Add(new Weight(0, WeightMethod.Set, WeightReason.NavBlocking));
                    return weightFactors;
                }
            }

            // As a percentage of health with typical maximum weight
            cacheObject.Weight = MaxWeight * (1 - CacheManager.Me.CurrentHealthPct);

            return weightFactors;
        }

        /// <summary>
        /// Weighting for Shrines
        /// </summary>
        private static IEnumerable<Weight> ShrineWeighting(TrinityObject cacheObject)
        {
            var weightFactors = new List<Weight>();
            var gizmo = cacheObject as TrinityGizmo;
            if (gizmo == null)
            {
                weightFactors.Add(new Weight(0, WeightMethod.Set, WeightReason.InvalidType));
                return weightFactors;
            }

            if(weightFactors.TryAddWeight(cacheObject, WeightReason.DisableForQuest))
                return weightFactors;

            var maxRange = CacheManager.Me.IsInRift ? 300f : 75f;
            var maxWeight = CacheManager.Me.IsInRift ? MaxWeight * 0.75d : 100d;
            var priorityShrines = Trinity.Settings.WorldObject.HiPriorityShrines;
            var startingWeight = priorityShrines ? MaxWeight*0.75 : Math.Max(((maxRange - cacheObject.RadiusDistance)/maxRange*15000d), 100d);

            weightFactors.Add(new Weight(startingWeight, WeightMethod.Set, WeightReason.StartingWeight));

            // Very close shrines get a weight increase
            if (cacheObject.Distance <= 30f)
                weightFactors.Add(new Weight(10000, WeightMethod.Add, WeightReason.CloseProximity));

            // Disable safety checks for Rift Pylons
            if (!CacheManager.Me.IsInRift && cacheObject.Weight > 0)
            {
                weightFactors.TryAddWeight(cacheObject, WeightReason.PreviousTarget);
                weightFactors.TryAddWeight(cacheObject, WeightReason.MonsterInLoS);
                weightFactors.TryAddWeight(cacheObject, WeightReason.AvoidanceInLoS);
                weightFactors.TryAddWeight(cacheObject, WeightReason.DangerClose);
            }

            return weightFactors;
        }

        /// <summary>
        /// Weighting for Doors
        /// </summary>
        private static IEnumerable<Weight> DoorWeighting(TrinityObject cacheObject)
        {
            var weightFactors = new List<Weight>();
            var gizmo = cacheObject as TrinityGizmo;
            if (gizmo == null)
            {
                weightFactors.Add(new Weight(0, WeightMethod.Set, WeightReason.InvalidType));
                return weightFactors;
            }

            if(weightFactors.TryAddWeight(cacheObject, WeightReason.MonsterInLoS))
                return weightFactors;

            weightFactors.TryAddWeight(cacheObject, WeightReason.UnitsBehind, 15000);
            weightFactors.TryAddWeight(cacheObject, WeightReason.PreviousTarget);
            weightFactors.TryAddWeight(cacheObject, WeightReason.CloseProximity, MaxWeight);
  
            return weightFactors;
        }

        /// <summary>
        /// Weighting for Barricades
        /// </summary>
        private static IEnumerable<Weight> BarricadeWeighting(TrinityObject cacheObject)
        {
            var weightFactors = new List<Weight>();

            // rrrix added this as a single "weight" source based on the DestructableRange.
            // Calculate the weight based on distance, where a distance = 1 is 5000, 90 = 0
            weightFactors.Add(new Weight((90f - cacheObject.RadiusDistance) / 90f * 5000f, WeightMethod.Set, WeightReason.StartingWeight));

            weightFactors.TryAddWeight(cacheObject, WeightReason.PreviousTarget);
            weightFactors.TryAddWeight(cacheObject, WeightReason.CloseProximity, 30000);
  
            return weightFactors;
        }

        /// <summary>
        /// Weighting for Destructibles
        /// </summary>
        private static IEnumerable<Weight> DestructibleWeighting(TrinityObject cacheObject)
        {
            var weightFactors = new List<Weight>();

            if (DataDictionary.ForceDestructibles.Contains(cacheObject.ActorSNO))
            {
                weightFactors.Add(new Weight(100, WeightMethod.Add, WeightReason.SettingForceDestructibles));
                return weightFactors;
            }

            // Not Stuck, skip!
            if (Trinity.Settings.WorldObject.DestructibleOption == DestructibleIgnoreOption.OnlyIfStuck && cacheObject.RadiusDistance > 0 &&
                (DateTime.UtcNow.Subtract(PlayerMover.LastGeneratedStuckPosition).TotalSeconds > 3))
            {
                return weightFactors;
            }

            // rrrix added this as a single "weight" source based on the DestructableRange.
            // Calculate the weight based on distance, where a distance = 1 is 5000, 90 = 0
            weightFactors.Add(new Weight((90f - cacheObject.RadiusDistance) / 90f * 1000f, WeightMethod.Set, WeightReason.StartingWeight));

            weightFactors.TryAddWeight(cacheObject, WeightReason.PreviousTarget);
            weightFactors.TryAddWeight(cacheObject, WeightReason.CloseProximity, 1500);
            weightFactors.TryAddWeight(cacheObject, WeightReason.MonsterInLoS, 0.5, WeightMethod.Multiply);
            weightFactors.TryAddWeight(cacheObject, WeightReason.AvoidanceInLoS);

            // Are we prioritizing close-range stuff atm? If so limit it at a value 3k lower than monster close-range priority
            if (CombatContext.PrioritizeCloseRangeUnits)
                weightFactors.Add(new Weight((15f - cacheObject.Distance) / 15f * 19200d, WeightMethod.Set, WeightReason.CloseRangePriority));

            weightFactors.TryAddWeight(cacheObject, WeightReason.TouchProximity, 40000);

            // Fix for WhimsyShire Pinata
            if (DataDictionary.ResplendentChestIds.Contains(cacheObject.ActorSNO))
                weightFactors.Add(new Weight(100 + cacheObject.RadiusDistance, WeightMethod.Set, WeightReason.DestructableChest));

            return weightFactors;
        }


        /// <summary>
        /// Weighting for Interactables
        /// </summary>
        private static IEnumerable<Weight> InteractableWeighting(TrinityObject cacheObject)
        {
            var weightFactors = new List<Weight>();

            // Campaign A5 Quest "Lost Treasure of the Nephalem" - have to interact with nephalem switches first... 
            // Quest: x1_Adria, Id: 257120, Step: 108 - disable all looting, pickup, and objects
            if (CacheManager.Me.WorldType != Act.OpenWorld && CacheManager.Me.CurrentQuestSNO == 257120 && CacheManager.Me.CurrentQuestStep == 108)
            {
                weightFactors.Add(new Weight(MaxWeight / 3, WeightMethod.Set, WeightReason.PrioritizeForQuest));
                return weightFactors;
            }

            // Need to Prioritize, forget it!
            if (CombatContext.PrioritizeCloseRangeUnits)
                return weightFactors;

            // nearby monsters attacking us - don't try to use headtone
            if (cacheObject.Object is DiaGizmo && cacheObject.Gizmo.CommonData.ActorInfo.GizmoType == GizmoType.Headstone &&
                CacheManager.Units.Any(u => u.RadiusDistance < 25f && u.IsFacingPlayer))
            {
                weightFactors.Add(new Weight(0, WeightMethod.Set, WeightReason.MonstersNearPlayer));
                return weightFactors;
            }
            
            if (DataDictionary.HighPriorityInteractables.Contains(cacheObject.ActorSNO) && cacheObject.RadiusDistance <= 30f)
            {
                weightFactors.Add(new Weight(MaxWeight, WeightMethod.Set, WeightReason.HighPriorityInteractable));
                return weightFactors;
            }

            // Weight Interactable Specials
            weightFactors.Add(new Weight((300d - cacheObject.Distance) / 300d * 1000d, WeightMethod.Set, WeightReason.StartingWeight));

            weightFactors.TryAddWeight(cacheObject, WeightReason.CloseProximity);

            var unit = cacheObject as TrinityUnit;
            if (unit != null && unit.IsQuestMonster)
                weightFactors.Add(new Weight(3000, WeightMethod.Add, WeightReason.QuestObjective));

            weightFactors.TryAddWeight(cacheObject, WeightReason.PreviousTarget);
            weightFactors.TryAddWeight(cacheObject, WeightReason.MonsterInLoS);
            weightFactors.TryAddWeight(cacheObject, WeightReason.AvoidanceInLoS);

            return weightFactors;
        }


        /// <summary>
        /// Weighting for Containers
        /// </summary>
        private static IEnumerable<Weight> ContainerWeighting(TrinityObject cacheObject)
        {
            var weightFactors = new List<Weight>();

            if (weightFactors.TryAddWeight(cacheObject, WeightReason.CloseRangePriority))
                return weightFactors;

            if (weightFactors.TryAddWeight(cacheObject, WeightReason.NavBlocking))
                return weightFactors;

            // Dont open any containers while damage buff is active
            if (Legendary.HarringtonWaistguard.IsBuffActive)
            {
                weightFactors.Add(new Weight(0, WeightMethod.Set, WeightReason.HarringtonBuff));
                return weightFactors;
            }

            // Weight Containers
            float maxOpenRange = cacheObject.InternalName.ToLower().Contains("chest_rare") ? 250 : Trinity.Settings.WorldObject.ContainerOpenRange;
            weightFactors.Add(new Weight((maxOpenRange - cacheObject.Distance) / maxOpenRange * 100d, WeightMethod.Set, WeightReason.StartingWeight));

            weightFactors.TryAddWeight(cacheObject, WeightReason.CloseProximity, 600);

            // Open container for the damage buff
            if (Legendary.HarringtonWaistguard.IsEquipped && !Legendary.HarringtonWaistguard.IsBuffActive &&
                ZetaDia.Me.IsInCombat && cacheObject.Distance < 80f || CombatContext.ShouldPrioritizeContainers)
                weightFactors.Add(new Weight(20000, WeightMethod.Add, WeightReason.StartingWeight));

            weightFactors.TryAddWeight(cacheObject, WeightReason.PreviousTarget);
            weightFactors.TryAddWeight(cacheObject, WeightReason.MonsterInLoS, 0.5, WeightMethod.Multiply);
            weightFactors.TryAddWeight(cacheObject, WeightReason.AvoidanceInLoS);

            return weightFactors;
        }


        /// <summary>
        /// Utility for common weight factors
        /// </summary>
        public static bool TryAddWeight(this List<Weight> weightFactors, TrinityObject cacheObject, WeightReason reason, double value = -1, WeightMethod method = WeightMethod.None)
        {
            var isMethod = method != WeightMethod.None;
            var isValue = value > 0;

            switch (reason)
            {
                case WeightReason.PreviousTarget:

                    if (cacheObject.RActorGuid == Trinity.LastTargetRactorGUID && cacheObject.Distance <= 25f)
                        weightFactors.Add(new Weight(isValue ? value : 400, isMethod ? method : WeightMethod.Add, WeightReason.PreviousTarget));

                    return true;

                case WeightReason.AvoidanceInLoS:

                    if (CacheManager.Avoidances.Any(a => MathUtil.IntersectsPath(a.Position, a.Radius + 5f, CacheManager.Me.Position, cacheObject.Position)))
                        weightFactors.Add(new Weight(isValue ? value : 1, isMethod ? method : WeightMethod.Set, WeightReason.AvoidanceInLoS));

                    return true;

                case WeightReason.MonsterInLoS:

                    if (CacheManager.Units.Any(cp => MathUtil.IntersectsPath(cp.Position, cp.Radius * 1.2f, CacheManager.Me.Position, cacheObject.Position)))
                        weightFactors.Add(new Weight(isValue ? value : 1, isMethod ? method : WeightMethod.Set, WeightReason.MonsterInLoS));

                    return true;

                case WeightReason.DangerClose:

                    if (TargetUtil.AnyMobsInRange(15f) && !CacheManager.Me.IsInRift || CombatContext.PrioritizeCloseRangeUnits)
                        weightFactors.Add(new Weight(isValue ? value : 1, isMethod ? method : WeightMethod.Set, WeightReason.DangerClose));

                    return true;

                case WeightReason.TouchProximity:

                    if (cacheObject.RadiusDistance <= 4f)
                        weightFactors.Add(new Weight(isValue ? value : 5000, isMethod ? method : WeightMethod.Add, WeightReason.TouchProximity));

                    return true;

                case WeightReason.CloseProximity:

                    if (cacheObject.RadiusDistance <= 12f)
                        weightFactors.Add(new Weight(isValue ? value : 1000, isMethod ? method : WeightMethod.Add, WeightReason.CloseProximity));

                    return true;

                case WeightReason.UnitsBehind:

                    if (CacheManager.Units.Any(u => u.IsUnit && u.HitpointsCurrentPct > 0 && u.Distance > cacheObject.Distance && MathUtil.IntersectsPath(cacheObject.Position, cacheObject.Radius, CacheManager.Me.Position, u.Position)))
                        weightFactors.Add(new Weight(isValue ? value : 250, isMethod ? method : WeightMethod.Add, WeightReason.UnitsBehind));

                    return true;

                case WeightReason.DisableForQuest:

                    // Campaign A5 Quest "Lost Treasure of the Nephalem" - have to interact with nephalem switches first... 
                    // Quest: x1_Adria, Id: 257120, Step: 108 - disable all looting, pickup, and objects
                    if (CacheManager.Me.WorldType != Act.OpenWorld && CacheManager.Me.CurrentQuestSNO == 257120 && CacheManager.Me.CurrentQuestStep == 108)
                        weightFactors.Add(new Weight(isValue ? value : 0, isMethod ? method : WeightMethod.Set, WeightReason.DisableForQuest));

                    return true;

                case WeightReason.CloseRangePriority:

                    if (CombatContext.PrioritizeCloseRangeUnits)
                        weightFactors.Add(new Weight(isValue ? value : 0, isMethod ? method : WeightMethod.Set, WeightReason.CloseRangePriority));

                    return true;

                case WeightReason.NavBlocking:

                    if (cacheObject.IsBlocking)
                        weightFactors.Add(new Weight(isValue ? value : 0, isMethod ? method : WeightMethod.Set, WeightReason.NavBlocking));

                    return true;

                case WeightReason.BossOrEliteNearby:

                    if (CacheManager.Units.Any(m => m.IsBossOrEliteRareUnique))
                        weightFactors.Add(new Weight(isValue ? value : 0, isMethod ? method : WeightMethod.Set, WeightReason.BossOrEliteNearby));

                    return true;


                case WeightReason.NoCombatLooting:

                    if (CharacterSettings.Instance.CombatLooting && CombatBase.IsInCombat && TargetUtil.AnyMobsInRange(CombatContext.KillRadius))
                        weightFactors.Add(new Weight(isValue ? value : 1, isMethod ? method : WeightMethod.Set, WeightReason.NoCombatLooting));

                    return true;


                 
            }

            if (isValue && isMethod)
            {
                weightFactors.Add(new Weight(value, method, reason));
                return true;
            }
                
            return false;
        }


        #region Supporting Classes, Enums

        /// <summary>
        /// A single weight calculation
        /// </summary>
        public struct Weight
        {
            public Weight(double amount, WeightMethod method, WeightReason reason, bool isFinal = false)
            {
                Amount = amount;
                Reason = reason;
                Method = method;
                IsFinal = isFinal;
            }

            public bool IsFinal;
            public WeightReason Reason;
            public WeightMethod Method;
            public double Amount;
        }

        /// <summary>
        /// Way of combining a weight with others
        /// </summary>
        public enum WeightMethod
        {
            None = 0,
            Multiply,
            Set,
            Add,
            Subtract
        }

        /// <summary>
        /// Why the weight is being adjusted 
        /// </summary>
        public enum WeightReason
        {
            None = 0,
            StartingWeight,
            AvoidanceNearby,

            /// <summary>
            /// Set, 1, If there are monsters close to player
            /// </summary>
            DangerClose,
            IgnoreElites,
            HighPriorityContainer,
            HighPriorityShrine,
            HealthEmergency,

            /// <summary>
            /// Set, 0, If blocking movement
            /// </summary>
            NavBlocking,
            DisabledInSettings,
            GruesomeFeast,
            BloodVengeance,
            HighPrioritySetting,
            LowHealthPartyMember,

            /// <summary>
            /// Add, 1000, If very close to player (12f)
            /// </summary>
            CloseProximity,
            MediumProximity,
            ReapersWraps,

            /// <summary>
            /// Add, 400, If this was the target last tick
            /// </summary>
            PreviousTarget,
            AvoidanceAtPosition,

            /// <summary>
            /// Set, 1, If monsters are between us and the object
            /// </summary>
            MonsterInLoS,
            CloseToMonster,
            CloseToAvoidance,
            MonstersNearPlayer,
            HasTarget,
            AlreadyCloseEnough,

            /// <summary>
            /// Set, 0, If we're supposed to be prioritizing quest things
            /// </summary>
            DisableForQuest,

            /// <summary>
            /// Set, 0, If boss or elite nearby
            /// </summary>
            BossOrEliteNearby,

            InvalidType,

            /// <summary>
            /// Set, 1, If combatlooting is disabled and we're in combat
            /// </summary>
            NoCombatLooting,

            /// <summary>
            /// Set, 1, If there is avoidance between us and the object
            /// </summary>
            AvoidanceInLoS,
            Event,

            /// <summary>
            /// Add, 250, If units are behind
            /// </summary>
            UnitsBehind,
            SettingForceDestructibles,

            /// <summary>
            /// Add, 5000, If very close to player (5f)
            /// </summary>
            TouchProximity,

            /// <summary>
            /// Set, 0, If supposed to be prioritizing close range monsters
            /// </summary>
            CloseRangePriority,
            PrioritizeForQuest,
            DestructableChest,
            HighPriorityInteractable,
            QuestObjective,
            HarringtonBuff,
            BossNearby,
            TownRun,
            IsLegendary,
            IgnoreNearElites,
            MonkTR,
            IgnoreOrphanTrash,
            IgnoreHealthDot,
            NotInKillRange,
            TooFarFromEvent,
            DeadUnit,
            GoblinKamikaze,
            NotAttackable,
            IsBossEliteRareUnique,
            BountyObjective,
            InActiveEvent,
            ArchonElite,
            ElitesNearPlayers,
            InHotSpot,
            RangedUnit,
            LowHPTrash,
            LowHPElite,
            XtraPriority,
            Summoner,
            CorruptGrowth,
            MinWeight,
            InSpecialAoE,
            InAoE,
            AoEPathLine,
            LowHPGoblin,
            GoblinNormal,
            GoblinPriority,
            AntiFlipFlop
        }

        #endregion
    }
}
