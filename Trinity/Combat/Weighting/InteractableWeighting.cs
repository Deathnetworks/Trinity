using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trinity.LazyCache;
using Zeta.Game;
using Zeta.Game.Internals.Actors;
using Zeta.Game.Internals.SNO;

namespace Trinity.Combat.Weighting
{
    public class InteractableWeighting : WeightingBase
    {
        /// <summary>
        /// Weighting for Interactables
        /// </summary>
        public static IEnumerable<Weight> GetWeight(TrinityObject cacheObject)
        {
            var weightFactors = new List<Weight>();

            // Campaign A5 Quest "Lost Treasure of the Nephalem" - have to interact with nephalem switches first... 
            // Quest: x1_Adria, Id: 257120, Step: 108 - disable all looting, pickup, and objects
            if (CacheManager.Me.WorldType != Act.OpenWorld && CacheManager.Me.CurrentQuestSNO == 257120 && CacheManager.Me.CurrentQuestStep == 108)
            {
                weightFactors.Add(new Weight(WeightManager.MaxWeight / 3, WeightMethod.Set, WeightReason.PrioritizeForQuest));
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
                weightFactors.Add(new Weight(WeightManager.MaxWeight, WeightMethod.Set, WeightReason.HighPriorityInteractable));
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
    }
}
