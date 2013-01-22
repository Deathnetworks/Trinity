using System;
using Zeta;
using Zeta.Common;
using Zeta.Internals.Actors;

namespace GilesTrinity
{
    // Current cached player data
    // Just stores the data on YOU, well, your character's current status - for readability of those variables more than anything, but also caching
    public class GilesPlayerCache
    {
        public DateTime LastUpdated { get; set; }
        public bool IsIncapacitated { get; set; }
        public bool IsRooted { get; set; }
        public bool IsInTown { get; set; }
        public double CurrentHealthPct { get { return ZetaDia.Me.HitpointsCurrentPct; } }
        public double PrimaryResource
        {
            get
            {
                return ZetaDia.Me.CurrentPrimaryResource;
            }
        }
        public double PrimaryResourcePct
        {
            get
            {
                return ZetaDia.Me.CurrentPrimaryResource / ZetaDia.Me.MaxPrimaryResource;
            }
        }
        public double SecondaryResource { get { return ZetaDia.Me.CurrentSecondaryResource; } }
        public double SecondaryResourcePct { get { return ZetaDia.Me.CurrentSecondaryResource / ZetaDia.Me.MaxSecondaryResource; } }
        public Vector3 CurrentPosition { get { return ZetaDia.Me.Position; } }
        public bool WaitingForReserveEnergy { get; set; }
        public int MyDynamicID { get; set; }
        public int Level { get; set; }
        public ActorClass ActorClass { get; set; }
        public string BattleTag { get; set; }
        public int SceneId { get; set; }
        public int LevelAreaId { get; set; }
        public double PlayerDamagePerSecond { get; set; }
        public SceneInfo Scene { get; set; }

        public GilesPlayerCache()
        {
            LastUpdated = DateTime.MinValue;
            IsIncapacitated = false;
            IsRooted = false;
            IsInTown = false;
            //CurrentHealthPct = 0;
            //PrimaryResource = 0;
            //PrimaryResourcePct = 0;
            //SecondaryResource = 0;
            //SecondaryResourcePct = 0;
            //CurrentPosition = Vector3.Zero;
            WaitingForReserveEnergy = false;
            MyDynamicID = -1;
            Level = -1;
            ActorClass = Zeta.Internals.Actors.ActorClass.Invalid;
            BattleTag = String.Empty;
            SceneId = -1;
            LevelAreaId = -1;
        }

        public GilesPlayerCache(
            DateTime lastUpdated, bool incapacitated, bool isRooted, bool isInTown, double currentHealth, double currentEnergy, double currentEnergyPct,
            double discipline, double disciplinePct, Vector3 currentPosition, bool waitingReserve, int dynamicId, int level, ActorClass actorClass, string battleTag)
        {
            LastUpdated = lastUpdated;
            IsIncapacitated = incapacitated;
            IsRooted = isRooted;
            IsInTown = isInTown;
            //CurrentHealthPct = currentHealth;
            //PrimaryResource = currentEnergy;
            //PrimaryResourcePct = currentEnergyPct;
            //SecondaryResource = discipline;
            //SecondaryResourcePct = disciplinePct;
            //CurrentPosition = currentPosition;
            WaitingForReserveEnergy = waitingReserve;
            MyDynamicID = dynamicId;
            Level = level;
            ActorClass = actorClass;
            BattleTag = battleTag;
            SceneId = -1;
            LevelAreaId = -1;
            Scene = new SceneInfo()
            {
                SceneId = -1,
                LastUpdate = DateTime.Now
            };
        }

        public class SceneInfo
        {
            public DateTime LastUpdate { get; set; }
            public int SceneId { get; set; }
        }
    }
}
