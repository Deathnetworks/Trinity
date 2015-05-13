using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Trinity.Combat.Abilities;
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

namespace Trinity.Combat.Weighting
{
    /// <summary>
    /// Calculation for each object used for prioritising targets
    /// </summary>
    public static class WeightManager
    {
        public const double MaxWeight = 50000d;

        /// <summary>
        /// Calculate a weight for a TrinityObject
        /// </summary>
        public static double CalculateWeight(TrinityObject trinityObject, out List<Weight> outFactors)
        {
            //var timer = Stopwatch.StartNew();

            var weightFactors = new List<Weight>();

            //switch (trinityObject.TrinityType)
            //{
            //    case TrinityObjectType.Unit:
            //        weightFactors.AddRange(UnitWeighting.GetWeight(trinityObject));
            //        break;

            //    case TrinityObjectType.HotSpot:
            //        weightFactors.AddRange(HotSpotWeighting.GetWeight(trinityObject));
            //        break;

            //    case TrinityObjectType.Item:
            //        weightFactors.AddRange(ItemWeighting.GetItemWeight(trinityObject));
            //        break;

            //    case TrinityObjectType.Gold:
            //        weightFactors.AddRange(ItemWeighting.GetGoldWeight(trinityObject));
            //        break;

            //    case TrinityObjectType.PowerGlobe:
            //        weightFactors.AddRange(GlobeWeighting.GetPowerGlobeWeight(trinityObject));
            //        break;

            //    case TrinityObjectType.HealthGlobe:
            //        weightFactors.AddRange(GlobeWeighting.GetHealthGlobeWeight(trinityObject));
            //        break;

            //    case TrinityObjectType.ProgressionGlobe:
            //        weightFactors.AddRange(GlobeWeighting.GetProgressionGlobeWeight(trinityObject));
            //        break;

            //    case TrinityObjectType.HealthWell:
            //        weightFactors.AddRange(HealthWellWeighting.GetWeight(trinityObject));
            //        break;

            //    case TrinityObjectType.CursedShrine:
            //    case TrinityObjectType.Shrine:
            //        weightFactors.AddRange(ShrineWeighting.GetWeight(trinityObject));
            //        break;

            //    case TrinityObjectType.Door:
            //        weightFactors.AddRange(DoorWeighting.GetWeight(trinityObject));
            //        break;

            //    case TrinityObjectType.Barricade:
            //        weightFactors.AddRange(ObstacleWeighting.GetBarricadeWeight(trinityObject));
            //        break;

            //    case TrinityObjectType.Destructible:
            //        weightFactors.AddRange(ObstacleWeighting.GetDestructibleWeight(trinityObject));
            //        break;

            //    case TrinityObjectType.Interactable:
            //        weightFactors.AddRange(InteractableWeighting.GetWeight(trinityObject));
            //        break;

            //    case TrinityObjectType.Container:
            //        weightFactors.AddRange(ContainerWeighting.GetWeight(trinityObject));
            //        break;
            //}

            var finalWeight = CombineWeights(weightFactors);

            // Prevent current target dynamic ranged weighting flip-flop 
            if (trinityObject.IsLastTarget && finalWeight <= 1 && !trinityObject.IsBlocking)
                finalWeight = 100;

            // Spurt the history of our work for debugging etc
            outFactors = weightFactors;

            //CacheUtilities.LogTime(timer);

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
            }
            return (float)Math.Min(finalWeight, MaxWeight);
        }


    }


}
