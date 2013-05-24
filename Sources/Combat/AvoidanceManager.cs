using Trinity.Config.Combat;
using Trinity.Technicals;
using System.Collections.Generic;
using System.IO;
using Zeta.Internals.Actors;

namespace Trinity
{
    public static class AvoidanceManager
    {
        /// <summary>
        /// Initializes the <see cref="AvoidanceManager" /> class.
        /// </summary>
        static AvoidanceManager()
        {
            LoadAvoidanceDictionary();
        }

        private static void LoadAvoidanceDictionary(bool force = false)
        {
            if (SNOAvoidanceType == null || force)
            {
                SNOAvoidanceType = FileManager.Load<int, AvoidanceType>("AvoidanceType", "SNO", "Type");
            }
        }

        private static IDictionary<int, AvoidanceType> SNOAvoidanceType
        {
            get;
            set;
        }

        public static AvoidanceType GetAvoidanceType(int actorSno)
        {
            LoadAvoidanceDictionary(false);
            if (SNOAvoidanceType.ContainsKey(actorSno))
                return SNOAvoidanceType[actorSno];
            else
                return AvoidanceType.None;
        }

        public static float GetAvoidanceRadius(AvoidanceType type, float defaultValue)
        {
            LoadAvoidanceDictionary(false);
            //TODO : Make mapping between Type and Config
            switch (type)
            {
                case AvoidanceType.Arcane:
                    return Trinity.Settings.Combat.AvoidanceRadius.Arcane;
                case AvoidanceType.AzmodanBody:
                    return Trinity.Settings.Combat.AvoidanceRadius.AzmoBodies;
                case AvoidanceType.AzmoFireball:
                    return Trinity.Settings.Combat.AvoidanceRadius.AzmoFireBall;
                case AvoidanceType.AzmodanPool:
                    return Trinity.Settings.Combat.AvoidanceRadius.AzmoPools;
                case AvoidanceType.BeeWasp:
                    return Trinity.Settings.Combat.AvoidanceRadius.BeesWasps;
                case AvoidanceType.Belial:
                    return Trinity.Settings.Combat.AvoidanceRadius.Belial;
                case AvoidanceType.ButcherFloorPanel:
                    return Trinity.Settings.Combat.AvoidanceRadius.ButcherFloorPanel;
                case AvoidanceType.Desecrator:
                    return Trinity.Settings.Combat.AvoidanceRadius.Desecrator;
                case AvoidanceType.DiabloMeteor:
                    return Trinity.Settings.Combat.AvoidanceRadius.DiabloMeteor;
                case AvoidanceType.DiabloPrison:
                    return Trinity.Settings.Combat.AvoidanceRadius.DiabloPrison;
                case AvoidanceType.DiabloRingOfFire:
                    return Trinity.Settings.Combat.AvoidanceRadius.DiabloRingOfFire;
                case AvoidanceType.GhomGas:
                    return Trinity.Settings.Combat.AvoidanceRadius.GhomGas;
                case AvoidanceType.IceBall:
                    return Trinity.Settings.Combat.AvoidanceRadius.IceBalls;
                case AvoidanceType.IceTrail:
                    return Trinity.Settings.Combat.AvoidanceRadius.IceTrail;
                case AvoidanceType.MageFire:
                    return Trinity.Settings.Combat.AvoidanceRadius.MageFire;
                case AvoidanceType.MaghdaProjectille:
                    return Trinity.Settings.Combat.AvoidanceRadius.MaghdaProjectille;
                case AvoidanceType.MoltenCore:
                    return Trinity.Settings.Combat.AvoidanceRadius.MoltenCore;
                case AvoidanceType.MoltenTrail:
                    return Trinity.Settings.Combat.AvoidanceRadius.MoltenTrail;
                case AvoidanceType.MoltenBall:
                    return Trinity.Settings.Combat.AvoidanceRadius.MoltenBall;
                case AvoidanceType.PlagueCloud:
                    return Trinity.Settings.Combat.AvoidanceRadius.PlagueCloud;
                case AvoidanceType.PlagueHand:
                    return Trinity.Settings.Combat.AvoidanceRadius.PlagueHands;
                case AvoidanceType.PoisonTree:
                    return Trinity.Settings.Combat.AvoidanceRadius.PoisonTree;
                case AvoidanceType.SuccubusStar:
                    return Trinity.Settings.Combat.AvoidanceRadius.SuccubusStar;
                case AvoidanceType.ShamanFire:
                    return Trinity.Settings.Combat.AvoidanceRadius.ShamanFire;
                case AvoidanceType.ZoltBubble:
                    return Trinity.Settings.Combat.AvoidanceRadius.ZoltBubble;
                case AvoidanceType.ZoltTwister:
                    return Trinity.Settings.Combat.AvoidanceRadius.ZoltTwister;
                default:
                    {
                        Logger.Log(TrinityLogLevel.Error, LogCategory.Avoidance, "Unknown Avoidance type in Radius Switch! {0}", type.ToString());
                        return defaultValue;
                    }
            }
        }

        public static float GetAvoidanceRadiusBySNO(int snoId, float defaultValue)
        {
            if (SNOAvoidanceType.ContainsKey(snoId))
            {
                float radius = GetAvoidanceRadius(SNOAvoidanceType[snoId], defaultValue);
                //DbHelper.Log(TrinityLogLevel.Debug, LogCategory.Avoidance, "Found avoidance Radius of={0} for snoId={1} (default={2})", radius, snoId, defaultValue);
                return radius;
            }
            else
            {
                Logger.Log(TrinityLogLevel.Debug, LogCategory.Avoidance, "Unkown Avoidance type for Radius! {0}", snoId);
            }
            return defaultValue;
        }

        public static float GetAvoidanceHealth(AvoidanceType type, float defaultValue)
        {
            //TODO : Make mapping between Type and Config
            LoadAvoidanceDictionary(false);
            IAvoidanceHealth avoidanceHealth = null;
            switch (Trinity.PlayerStatus.ActorClass)
            {
                case ActorClass.Barbarian:
                    avoidanceHealth = Trinity.Settings.Combat.Barbarian;
                    break;
                case ActorClass.Monk:
                    avoidanceHealth = Trinity.Settings.Combat.Monk;
                    break;
                case ActorClass.Wizard:
                    avoidanceHealth = Trinity.Settings.Combat.Wizard;
                    break;
                case ActorClass.WitchDoctor:
                    avoidanceHealth = Trinity.Settings.Combat.WitchDoctor;
                    break;
                case ActorClass.DemonHunter:
                    avoidanceHealth = Trinity.Settings.Combat.DemonHunter;
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
                    case AvoidanceType.AzmoFireball:
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
                    case AvoidanceType.SuccubusStar:
                        return avoidanceHealth.AvoidSuccubusStarHealth;
                    case AvoidanceType.ShamanFire:
                        return avoidanceHealth.AvoidShamanFireHealth;
                    case AvoidanceType.ZoltBubble:
                        return avoidanceHealth.AvoidZoltBubbleHealth;
                    case AvoidanceType.ZoltTwister:
                        return avoidanceHealth.AvoidZoltTwisterHealth;
                    default:
                        {
                            Logger.Log(TrinityLogLevel.Error, LogCategory.Avoidance, "Unknown Avoidance type in Health Switch! {0}", type.ToString());
                            return defaultValue;
                        }
                }
            }
            return defaultValue;
        }

        public static float GetAvoidanceHealthBySNO(int snoId, float defaultValue)
        {
            if (SNOAvoidanceType.ContainsKey(snoId))
            {
                float health = GetAvoidanceHealth(SNOAvoidanceType[snoId], defaultValue);
                //DbHelper.Log(TrinityLogLevel.Debug, LogCategory.Avoidance, "Found avoidance Health of={0} for snoId={1} (default={2})", health, snoId, defaultValue);
                return health;
            }
            else
            {
                Logger.Log(TrinityLogLevel.Debug, LogCategory.Avoidance, "Unkown Avoidance type for Health! {0}", snoId);
            }
            return defaultValue;
        }
    }
}
