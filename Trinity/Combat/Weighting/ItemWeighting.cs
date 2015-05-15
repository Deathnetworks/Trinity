using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trinity.Combat.Abilities;
using Trinity.Items;
using Trinity.LazyCache;
using Zeta.Common;
using Zeta.Game;
using Zeta.Game.Internals.Actors;

namespace Trinity.Combat.Weighting
{
    public class ItemWeighting : WeightingBase
    {
        /// <summary>
        /// Weighting for Gold
        /// </summary>
        public static IEnumerable<Weight> GetGoldWeight(TrinityObject cacheObject)
        {
            var weightFactors = new List<Weight>();
            var item = cacheObject as TrinityItem;
            if (item == null)
            {
                weightFactors.Add(new Weight(0, WeightMethod.Set, WeightReason.TypeMismatch));
                return weightFactors;
            }

            if (weightFactors.TryAddWeight(cacheObject, WeightReason.DisableForQuest))
                return weightFactors;

            if (CacheManager.Units.Any(m => m.IsBossOrEliteRareUnique))
            {
                weightFactors.Add(new Weight(0, WeightMethod.Set, WeightReason.BossOrEliteNearby));
                return weightFactors;
            }

            weightFactors.Add(new Weight(Math.Max((175 - item.Distance) / 175 * WeightManager.MaxWeight, 100d), WeightMethod.Set, WeightReason.Start));
            weightFactors.TryAddWeight(cacheObject, WeightReason.PreviousTarget, 800);

            // Ignore gold in AoE
            if (Trinity.Settings.Loot.Pickup.IgnoreGoldInAoE && CacheManager.Avoidances.Any(aoe => item.Position.Distance2D(aoe.Position) <= aoe.Radius))
            {
                weightFactors.Add(new Weight(0, WeightMethod.Set, WeightReason.AvoidanceNearby));
            }

            if (item.ItemQuality < ItemQuality.Legendary)
                weightFactors.TryAddWeight(cacheObject, WeightReason.NoCombatLooting);

            weightFactors.TryAddWeight(cacheObject, WeightReason.MonsterInLoS);
            weightFactors.TryAddWeight(cacheObject, WeightReason.AvoidanceInLoS);

            return weightFactors;
        }

        /// <summary>
        /// Weighting for Items
        /// </summary>
        public static IEnumerable<Weight> GetItemWeight(TrinityObject cacheObject)
        {
            var weightFactors = new List<Weight>();
            var item = cacheObject as TrinityItem;
            if (item == null)
            {
                weightFactors.Add(new Weight(0, WeightMethod.Set, WeightReason.TypeMismatch));
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
            weightFactors.Add(new Weight(Math.Max((175 - item.Distance) / 175 * WeightManager.MaxWeight, 100d), WeightMethod.Set, WeightReason.Start));

            // Don't pickup items if we're doing a TownRun
            if (TrinityItemManager.FindValidBackpackLocation(item.IsTwoSquareItem) == new Vector2(-1, -1))
            {
                weightFactors.Add(new Weight(0, WeightMethod.Set, WeightReason.TownRun));
                return weightFactors;
            }

            // Give legendaries max weight, always
            if (item.ItemQuality >= ItemQuality.Legendary)
            {
                weightFactors.Add(new Weight(WeightManager.MaxWeight, WeightMethod.Set, WeightReason.IsLegendary));
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
    }
}
