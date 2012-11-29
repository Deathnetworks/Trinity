using GilesTrinity.Settings.Combat;
using System.Collections.Generic;
using Zeta.Internals.Actors;

namespace GilesTrinity
{
    public static class AvoidanceManager
    {
        static AvoidanceManager()
        {
            SNOAvoidanceType = new Dictionary<int, AvoidanceType>
                                    {
                                        {219702, AvoidanceType.Arcane},
                                        {221225, AvoidanceType.Arcane},
                                        {84608, AvoidanceType.Desecrator},
                                        {5482, AvoidanceType.PoisonTree},
                                        {6578, AvoidanceType.PoisonTree},
                                        {4803, AvoidanceType.MoltenCore},
                                        {4804, AvoidanceType.MoltenCore},
                                        {224225, AvoidanceType.MoltenCore},
                                        {247987, AvoidanceType.MoltenCore},
                                        {95868, AvoidanceType.MoltenTrail},
                                        {108869, AvoidanceType.PlagueCloud},
                                        {402, AvoidanceType.IceBall},
                                        {223675, AvoidanceType.IceBall},
                                        {4103, AvoidanceType.ShamanFire},
                                        {5212, AvoidanceType.BeeWasp},
                                        {3865, AvoidanceType.PlagueHand},
                                        {123124, AvoidanceType.AzmodanPool},
                                        {93837, AvoidanceType.GhomGas},                   
                                        {123839, AvoidanceType.AzmodanBody},                    // BARB     MONK    W*2     DH
                                        {161822, AvoidanceType.Belial},                         // 100%     100%    100%    100%
                                        {161833, AvoidanceType.Belial},                         // 100%     100%    100%    100%
                                        {123842, AvoidanceType.AzmodanFireball},
                                        {432, AvoidanceType.MageFire},                          // 30%      30%     10%     20%
                                        {168031, AvoidanceType.DiabloPrison},                   // 100%     100%    100%    100%
                                        {214845, AvoidanceType.DiabloMeteor},                   // 50%      50%     80%     100%
                                        {260377, AvoidanceType.IceTrail},                       // 50%      50%     90%     90%
                                        {160154, AvoidanceType.MoltenBall},                     // 0%       0%      10%     20%
                                        {199997, AvoidanceType.WallOfFire},                     // 50%      50%     50%     50%
                                        {199998, AvoidanceType.WallOfFire},                     // 50%      50%     50%     50%
                                        {185924, AvoidanceType.ZoltBubble},                     // 100%     100%    100%    100%
                                        {139741, AvoidanceType.ZoltTwister},                    // 70%      70%     70%     70%
                                        {166686, AvoidanceType.MaghdaProjectille},              // 70%      70%     70%     70%
                                        {201454, AvoidanceType.ButcherFloorPanel},              // 100%     100%    100%    100%    
                                        {201464, AvoidanceType.ButcherFloorPanel},              // 100%     100%    100%    100%
                                        {201426, AvoidanceType.ButcherFloorPanel},              // 100%     100%    100%    100%
                                        {201438, AvoidanceType.ButcherFloorPanel},              // 100%     100%    100%    100%
                                        {200969, AvoidanceType.ButcherFloorPanel},              // 100%     100%    100%    100%
                                        {201423, AvoidanceType.ButcherFloorPanel},              // 100%     100%    100%    100%
                                        {201242, AvoidanceType.ButcherFloorPanel},              // 100%     100%    100%    100%
                                        {226350, AvoidanceType.DiabloRingOfFire},               // 100%     100%    100%    100%
                                        {226525, AvoidanceType.DiabloRingOfFire},               // 100%     100%    100%    100%
                                    };
        }


        public static IDictionary<int, AvoidanceType> SNOAvoidanceType
        {
            get;
            private set;
        }

        public static float GetAvoidanceRadius(AvoidanceType type, float defaultValue)
        {
            //TODO : Make mapping between Type and Config
            switch (type)
            {
                case AvoidanceType.Arcane:
                    return GilesTrinity.Settings.Combat.AvoidanceRadius.Arcane;
                case AvoidanceType.AzmodanBody:
                    return GilesTrinity.Settings.Combat.AvoidanceRadius.AzmoBodies;
                case AvoidanceType.AzmodanFireball:
                    return GilesTrinity.Settings.Combat.AvoidanceRadius.AzmoFireBall;
                case AvoidanceType.AzmodanPool:
                    return GilesTrinity.Settings.Combat.AvoidanceRadius.AzmoPools;
                case AvoidanceType.BeeWasp:
                    return GilesTrinity.Settings.Combat.AvoidanceRadius.BeesWasps;
                case AvoidanceType.Belial:
                    return GilesTrinity.Settings.Combat.AvoidanceRadius.Belial;
                case AvoidanceType.ButcherFloorPanel:
                    return GilesTrinity.Settings.Combat.AvoidanceRadius.ButcherFloorPanel;
                case AvoidanceType.Desecrator:
                    return GilesTrinity.Settings.Combat.AvoidanceRadius.Desecrator;
                case AvoidanceType.DiabloMeteor:
                    return GilesTrinity.Settings.Combat.AvoidanceRadius.DiabloMeteor;
                case AvoidanceType.DiabloPrison:
                    return GilesTrinity.Settings.Combat.AvoidanceRadius.DiabloPrison;
                case AvoidanceType.DiabloRingOfFire:
                    return GilesTrinity.Settings.Combat.AvoidanceRadius.DiabloRingOfFire;
                case AvoidanceType.GhomGas:
                    return GilesTrinity.Settings.Combat.AvoidanceRadius.GhomGas;
                case AvoidanceType.IceBall:
                    return GilesTrinity.Settings.Combat.AvoidanceRadius.IceBalls;
                case AvoidanceType.IceTrail:
                    return GilesTrinity.Settings.Combat.AvoidanceRadius.IceTrail;
                case AvoidanceType.MageFire:
                    return GilesTrinity.Settings.Combat.AvoidanceRadius.MageFire;
                case AvoidanceType.MaghdaProjectille:
                    return GilesTrinity.Settings.Combat.AvoidanceRadius.MaghdaProjectille;
                case AvoidanceType.MoltenBall:
                    return GilesTrinity.Settings.Combat.AvoidanceRadius.MoltenBall;
                case AvoidanceType.MoltenCore:
                    return GilesTrinity.Settings.Combat.AvoidanceRadius.MoltenCore;
                case AvoidanceType.MoltenTrail:
                    return GilesTrinity.Settings.Combat.AvoidanceRadius.MoltenTrail;
                case AvoidanceType.PlagueCloud:
                    return GilesTrinity.Settings.Combat.AvoidanceRadius.PlagueCloud;
                case AvoidanceType.PlagueHand:
                    return GilesTrinity.Settings.Combat.AvoidanceRadius.PlagueHands;
                case AvoidanceType.PoisonTree:
                    return GilesTrinity.Settings.Combat.AvoidanceRadius.PoisonTree;
                case AvoidanceType.ShamanFire:
                    return GilesTrinity.Settings.Combat.AvoidanceRadius.ShamanFire;
                case AvoidanceType.WallOfFire:
                    return GilesTrinity.Settings.Combat.AvoidanceRadius.WallOfFire;
                case AvoidanceType.ZoltBubble:
                    return GilesTrinity.Settings.Combat.AvoidanceRadius.ZoltBubble;
                case AvoidanceType.ZoltTwister:
                    return GilesTrinity.Settings.Combat.AvoidanceRadius.ZoltTwister;
            }
            return defaultValue;
        }

        public static float GetAvoidanceRadiusBySNO(int snoId, float defaultValue)
        {
            if (SNOAvoidanceType.ContainsKey(snoId))
            {
                return GetAvoidanceRadius(SNOAvoidanceType[snoId], defaultValue);
            }
            return defaultValue;
        }

        public static float GetAvoidanceHealth(AvoidanceType type, float defaultValue)
        {
            //TODO : Make mapping between Type and Config
            IAvoidanceHealth avoidanceHealth = null;
            switch (GilesTrinity.iMyCachedActorClass)
            {
                case ActorClass.Barbarian:
                    avoidanceHealth = GilesTrinity.Settings.Combat.Barbarian;
                    break;
                case ActorClass.Monk:
                    avoidanceHealth = GilesTrinity.Settings.Combat.Monk;
                    break;
                case ActorClass.Wizard:
                    avoidanceHealth = GilesTrinity.Settings.Combat.Wizard;
                    break;
                case ActorClass.WitchDoctor:
                    avoidanceHealth = GilesTrinity.Settings.Combat.WitchDoctor;
                    break;
                case ActorClass.DemonHunter:
                    avoidanceHealth = GilesTrinity.Settings.Combat.DemonHunter;
                    break;
            }
            if (avoidanceHealth != null)
            {
                switch (type)
                {
                    case AvoidanceType.Arcane:
                        return avoidanceHealth.AvoidArcaneHealth;
                    case AvoidanceType.AzmodanBody:
                        return avoidanceHealth.AvoidAzmoBodiesHealth;
                    case AvoidanceType.AzmodanFireball:
                        return avoidanceHealth.AvoidAzmoFireBallHealth;
                    case AvoidanceType.AzmodanPool:
                        return avoidanceHealth.AvoidAzmoPoolsHealth;
                    case AvoidanceType.BeeWasp:
                        return avoidanceHealth.AvoidBeesWaspsHealth;
                    case AvoidanceType.Belial:
                        return avoidanceHealth.AvoidBelialHealth;
                    case AvoidanceType.ButcherFloorPanel:
                        return avoidanceHealth.AvoidButcherFloorPanelHealth;
                    case AvoidanceType.Desecrator:
                        return avoidanceHealth.AvoidDesecratorHealth;
                    case AvoidanceType.DiabloMeteor:
                        return avoidanceHealth.AvoidDiabloMeteorHealth;
                    case AvoidanceType.DiabloPrison:
                        return avoidanceHealth.AvoidDiabloPrisonHealth;
                    case AvoidanceType.DiabloRingOfFire:
                        return avoidanceHealth.AvoidDiabloRingOfFireHealth;
                    case AvoidanceType.GhomGas:
                        return avoidanceHealth.AvoidGhomGasHealth;
                    case AvoidanceType.IceBall:
                        return avoidanceHealth.AvoidIceBallsHealth;
                    case AvoidanceType.IceTrail:
                        return avoidanceHealth.AvoidIceTrailHealth;
                    case AvoidanceType.MageFire:
                        return avoidanceHealth.AvoidMageFireHealth;
                    case AvoidanceType.MaghdaProjectille:
                        return avoidanceHealth.AvoidMaghdaProjectilleHealth;
                    case AvoidanceType.MoltenBall:
                        return avoidanceHealth.AvoidMoltenBallHealth;
                    case AvoidanceType.MoltenCore:
                        return avoidanceHealth.AvoidMoltenCoreHealth;
                    case AvoidanceType.MoltenTrail:
                        return avoidanceHealth.AvoidMoltenTrailHealth;
                    case AvoidanceType.PlagueCloud:
                        return avoidanceHealth.AvoidPlagueCloudHealth;
                    case AvoidanceType.PlagueHand:
                        return avoidanceHealth.AvoidPlagueHandsHealth;
                    case AvoidanceType.PoisonTree:
                        return avoidanceHealth.AvoidPoisonTreeHealth;
                    case AvoidanceType.ShamanFire:
                        return avoidanceHealth.AvoidShamanFireHealth;
                    case AvoidanceType.WallOfFire:
                        return avoidanceHealth.AvoidWallOfFireHealth;
                    case AvoidanceType.ZoltBubble:
                        return avoidanceHealth.AvoidZoltBubbleHealth;
                    case AvoidanceType.ZoltTwister:
                        return avoidanceHealth.AvoidZoltTwisterHealth;
                }
            }
            return defaultValue;
        }

        public static float GetAvoidanceHealthBySNO(int snoId, float defaultValue)
        {
            if (SNOAvoidanceType.ContainsKey(snoId))
            {
                return GetAvoidanceHealth(SNOAvoidanceType[snoId], defaultValue);
            }
            return defaultValue;
        }
    }
}
