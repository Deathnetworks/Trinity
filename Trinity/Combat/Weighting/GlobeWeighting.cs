using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trinity.Combat.Abilities;
using Trinity.DbProvider;
using Trinity.LazyCache;
using Trinity.Reference;
using Zeta.Game;
using Zeta.Game.Internals.Actors;

namespace Trinity.Combat.Weighting
{
    public class GlobeWeighting : WeightingBase
    {
        /// <summary>
        /// Weighting for Health Globes
        /// </summary>
        public static IEnumerable<Weight> GetHealthGlobeWeight(TrinityObject cacheObject)
        {
            var weightFactors = new List<Weight>();
            //var item = cacheObject as TrinityItem;
            //if (item == null)
            //{
            //    weightFactors.Add(new Weight(0, WeightMethod.Set, WeightReason.InvalidType));
            //    return weightFactors;
            //}

            //var highPrioritySetting = Trinity.Settings.Combat.Misc.HiPriorityHG;

            //// Can't be reached.
            //if (cacheObject.IsBlocking)
            //{
            //    weightFactors.Add(new Weight(0, WeightMethod.Set, WeightReason.NavBlocking));
            //    return weightFactors;
            //}

            //if (!Trinity.Settings.Combat.Misc.CollectHealthGlobe)
            //{
            //    weightFactors.Add(new Weight(0, WeightMethod.Set, WeightReason.DisabledInSettings));
            //}

            //// WD's logic with Gruesome Feast passive,
            //// mostly for intelligence stacks, 10% per globe
            //// 1200 by default
            //bool wdGruesomeFeast =
            //    CacheManager.Me.ActorClass == ActorClass.Witchdoctor &&
            //    CacheManager.Me.CurrentPrimaryResource <= V.F("WitchDoctor.ManaForHealthGlobes") &&
            //    CacheData.Hotbar.PassiveSkills.Contains(SNOPower.Witchdoctor_Passive_GruesomeFeast);

            //if (wdGruesomeFeast)
            //    weightFactors.Add(new Weight(10000, WeightMethod.Add, WeightReason.GruesomeFeast));


            //// DH's logic with Blood Vengeance passive
            //// gain amount - 30 hatred per globe
            //// 100 by default
            //bool dhBloodVengeance =
            //    CacheManager.Me.ActorClass == ActorClass.DemonHunter &&
            //    CacheManager.Me.CurrentPrimaryResource <= V.F("DemonHunter.HatredForHealthGlobes") &&
            //    CacheData.Hotbar.PassiveSkills.Contains(SNOPower.DemonHunter_Passive_Vengeance);

            //if (dhBloodVengeance)
            //    weightFactors.Add(new Weight(10000, WeightMethod.Add, WeightReason.BloodVengeance));

            //// Health is High - Non-Emergency
            //if (!dhBloodVengeance && !wdGruesomeFeast && (CacheManager.Me.CurrentHealthPct > CombatBase.EmergencyHealthGlobeLimit))
            //{
            //    // If we're giving high priority to health globes, give it higher weight and check for resource level
            //    if (Trinity.Settings.Combat.Misc.HiPriorityHG)
            //    {
            //        weightFactors.Add(new Weight(0.9 * WeightManager.MaxWeight, WeightMethod.Set, WeightReason.HighPrioritySetting));
            //    }

            //    // Added weight for lowest health of party member
            //    var minPartyHealth = (CacheManager.Players.Any(p => !p.IsMe)) ? CacheManager.Players.Where(p => !p.IsMe).Min(p => p.HitpointsCurrentPct) : 1d;
            //    if (minPartyHealth > 0d && minPartyHealth < V.D("Weight.Globe.MinPartyHealthPct"))
            //        weightFactors.Add(new Weight((1d - minPartyHealth) * 5000d, WeightMethod.Add, WeightReason.LowHealthPartyMember));
            //}
            //else
            //{
            //    // Ok we have globes enabled, and our health is low
            //    if (highPrioritySetting)
            //    {
            //        weightFactors.Add(new Weight(WeightManager.MaxWeight, WeightMethod.Set, WeightReason.HighPrioritySetting));
            //    }
            //    else
            //    {
            //        weightFactors.Add(new Weight((90f - cacheObject.RadiusDistance) / 90f * 17000d, WeightMethod.Set, WeightReason.HealthEmergency));
            //    }

            //    // Point-blank items get a weight increase
            //    weightFactors.TryAddWeight(cacheObject, WeightReason.CloseProximity, 3000);

            //    // Close items get a weight increase
            //    if (cacheObject.Distance <= 60f)
            //        weightFactors.Add(new Weight(1500, WeightMethod.Add, WeightReason.MediumProximity));

            //    // Primary resource is low and we're wearing Reapers Wraps
            //    if (CacheManager.Me.IsInCombat && CacheManager.Me.PrimaryResourcePct < 0.3 && Legendary.ReapersWraps.IsEquipped && (CacheManager.Monsters.Count(u => u.Distance <= 40f) >= 5 || CacheManager.Monsters.Any(u => u.IsElite && u.Distance <= 40f)))
            //        weightFactors.Add(new Weight(3000, WeightMethod.Add, WeightReason.ReapersWraps));

            //    // Was already a target and is still viable, give it some free extra weight, to help stop flip-flopping between two targets
            //    if (cacheObject.RActorGuid == Trinity.LastTargetRactorGUID && cacheObject.Distance <= 25f)
            //        weightFactors.Add(new Weight(800, WeightMethod.Add, WeightReason.PreviousTarget));
            //}

            //if (!highPrioritySetting)
            //{
            //    // If there's a monster in the path-line to the item, reduce the weight by 15% for each
            //    foreach (var obstacle in CacheManager.Units.Where(cp => MathUtil.IntersectsPath(cp.Position, cp.Radius, CacheManager.Me.Position, cacheObject.Position)))
            //    {
            //        if (Trinity.Settings.Combat.Misc.HiPriorityHG)
            //            weightFactors.Add(new Weight(0.85, WeightMethod.Multiply, WeightReason.MonsterInLoS));
            //    }

            //    // See if there's any AOE avoidance in that spot, if so reduce the weight by 10%
            //    if (cacheObject.Distance > 10f)
            //        weightFactors.Add(new Weight(0.90, WeightMethod.Multiply, WeightReason.AvoidanceAtPosition));
            //}

            //// Do not collect health globes if we are kiting and health globe is too close to monster or avoidance
            //if (CombatBase.KiteDistance > 0)
            //{
            //    weightFactors.TryAddWeight(cacheObject, WeightReason.MonsterInLoS);
            //    weightFactors.TryAddWeight(cacheObject, WeightReason.AvoidanceInLoS);
            //}

            return weightFactors;
        }

        /// <summary>
        /// Weighting for Power Globes
        /// </summary>
        public static IEnumerable<Weight> GetPowerGlobeWeight(TrinityObject cacheObject)
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
                weightFactors.Add(new Weight((90f - item.RadiusDistance) / 90f * 5000d, WeightMethod.Set, WeightReason.Start));
            }

            weightFactors.TryAddWeight(cacheObject, WeightReason.CloseProximity);
            weightFactors.TryAddWeight(cacheObject, WeightReason.MonsterInLoS);
            weightFactors.TryAddWeight(cacheObject, WeightReason.AvoidanceInLoS);

            return weightFactors;
        }

        /// <summary>
        /// Weighting for Progression Globes (grift thingies)
        /// </summary>
        public static IEnumerable<Weight> GetProgressionGlobeWeight(TrinityObject cacheObject)
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
                weightFactors.Add(new Weight((90f - cacheObject.RadiusDistance) / 90f * WeightManager.MaxWeight, WeightMethod.Set, WeightReason.Start));
            }

            weightFactors.TryAddWeight(cacheObject, WeightReason.CloseProximity);
            weightFactors.TryAddWeight(cacheObject, WeightReason.MonsterInLoS);
            weightFactors.TryAddWeight(cacheObject, WeightReason.AvoidanceInLoS);

            return weightFactors;
        }
    }
}
