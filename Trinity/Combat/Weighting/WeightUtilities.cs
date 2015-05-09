using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Trinity.Combat.Abilities;
using Trinity.LazyCache;
using Zeta.Bot.Settings;
using Zeta.Game;

namespace Trinity.Combat.Weighting
{
    public static class WeightUtilities
    {
        /// <summary>
        /// Utility for setting a weight and returning the collection
        /// </summary>
        public static List<Weight> Return(this List<Weight> weightFactors, WeightReason reason, double value = 0)        
        {
            weightFactors.Add(new Weight(0, WeightMethod.Set, reason));
            return weightFactors;
        }


        /// <summary>
        /// Utility for common weight factors
        /// </summary>
        public static bool TryAddWeight(this List<Weight> weightFactors, TrinityObject cacheObject, WeightReason reason, double value = -1, WeightMethod method = WeightMethod.None)
        {
            var isMethod = method != WeightMethod.None;
            var isValue = value > 0;

            //switch (reason)
            //{
            //    case WeightReason.PreviousTarget:

            //        if (cacheObject.RActorGuid == Trinity.LastTargetRactorGUID && cacheObject.Distance <= 25f)
            //            weightFactors.Add(new Weight(isValue ? value : 400, isMethod ? method : WeightMethod.Add, WeightReason.PreviousTarget));

            //        return true;

            //    case WeightReason.AvoidanceInLoS:

            //        if (CacheManager.Avoidances.Any(a => MathUtil.IntersectsPath(a.Position, a.Radius + 5f, CacheManager.Me.Position, cacheObject.Position)))
            //            weightFactors.Add(new Weight(isValue ? value : 1, isMethod ? method : WeightMethod.Set, WeightReason.AvoidanceInLoS));

            //        return true;

            //    case WeightReason.MonsterInLoS:

            //        if (CacheManager.Units.Any(cp => MathUtil.IntersectsPath(cp.Position, cp.Radius * 1.2f, CacheManager.Me.Position, cacheObject.Position)))
            //            weightFactors.Add(new Weight(isValue ? value : 1, isMethod ? method : WeightMethod.Set, WeightReason.MonsterInLoS));

            //        return true;

            //    case WeightReason.DangerClose:

            //        if (CacheManager.Monsters.Any(u => u.Distance <= 15f) && !CacheManager.Me.IsInRift || CombatContext.PrioritizeCloseRangeUnits)
            //            weightFactors.Add(new Weight(isValue ? value : 1, isMethod ? method : WeightMethod.Set, WeightReason.DangerClose));

            //        return true;

            //    case WeightReason.TouchProximity:

            //        if (cacheObject.RadiusDistance <= 4f)
            //            weightFactors.Add(new Weight(isValue ? value : 5000, isMethod ? method : WeightMethod.Add, WeightReason.TouchProximity));

            //        return true;

            //    case WeightReason.CloseProximity:

            //        if (cacheObject.RadiusDistance <= 12f)
            //            weightFactors.Add(new Weight(isValue ? value : 1000, isMethod ? method : WeightMethod.Add, WeightReason.CloseProximity));

            //        return true;

            //    case WeightReason.UnitsBehind:

            //        if (CacheManager.Units.Any(u => u.IsUnit && u.HitpointsCurrentPct > 0 && u.Distance > cacheObject.Distance && MathUtil.IntersectsPath(cacheObject.Position, cacheObject.Radius, CacheManager.Me.Position, u.Position)))
            //            weightFactors.Add(new Weight(isValue ? value : 250, isMethod ? method : WeightMethod.Add, WeightReason.UnitsBehind));

            //        return true;

            //    case WeightReason.DisableForQuest:

            //        // Campaign A5 Quest "Lost Treasure of the Nephalem" - have to interact with nephalem switches first... 
            //        // Quest: x1_Adria, Id: 257120, Step: 108 - disable all looting, pickup, and objects
            //        if (CacheManager.Me.WorldType != Act.OpenWorld && CacheManager.Me.CurrentQuestSNO == 257120 && CacheManager.Me.CurrentQuestStep == 108)
            //            weightFactors.Add(new Weight(isValue ? value : 0, isMethod ? method : WeightMethod.Set, WeightReason.DisableForQuest));

            //        return true;

            //    case WeightReason.CloseRangePriority:

            //        if (CombatContext.PrioritizeCloseRangeUnits)
            //            weightFactors.Add(new Weight(isValue ? value : 0, isMethod ? method : WeightMethod.Set, WeightReason.CloseRangePriority));

            //        return true;

            //    case WeightReason.NavBlocking:

            //        if (cacheObject.IsBlocking)
            //            weightFactors.Add(new Weight(isValue ? value : 0, isMethod ? method : WeightMethod.Set, WeightReason.NavBlocking));

            //        return true;

            //    case WeightReason.BossOrEliteNearby:

            //        if (CacheManager.Units.Any(m => m.IsBossOrEliteRareUnique))
            //            weightFactors.Add(new Weight(isValue ? value : 0, isMethod ? method : WeightMethod.Set, WeightReason.BossOrEliteNearby));

            //        return true;


            //    case WeightReason.NoCombatLooting:

            //        if (CharacterSettings.Instance.CombatLooting && CombatBase.IsInCombat && CacheManager.Monsters.Any(u => u.Distance <= CombatContext.KillRadius))
            //            weightFactors.Add(new Weight(isValue ? value : 1, isMethod ? method : WeightMethod.Set, WeightReason.NoCombatLooting));

            //        return true;

            //}

            //if (isValue && isMethod)
            //{
            //    weightFactors.Add(new Weight(value, method, reason));
            //    return true;
            //}

            return false;
        }

    }
}
