using System;
using System.Linq;
using System.Collections.Generic;
using GilesTrinity.Technicals;
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
        public double CurrentHealthPct { get; set; }
        public double PrimaryResource { get; set; }
        public double PrimaryResourcePct { get; set; }
        public double SecondaryResource { get; set; }
        public double SecondaryResourcePct { get; set; }
        public Vector3 CurrentPosition { get; set; }
        public bool WaitingForReserveEnergy { get; set; }
        public int MyDynamicID { get; set; }
        public int Level { get; set; }
        public ActorClass ActorClass { get; set; }
        public string BattleTag { get; set; }
        public int SceneId { get; set; }
        public int LevelAreaId { get; set; }
        public double PlayerDamagePerSecond { get; set; }
        public SceneInfo Scene { get; set; }
        public int WorldDynamicID { get; set; }
        public int WorldID { get; set; }
        public bool IsInGame { get; set; }
        public bool IsDead { get; set; }
        public bool IsLoadingWorld { get; set; }
        public int Coinage { get; set; }
        public float GoldPickupRadius { get; set; }
        public bool IsHidden { get; set; }

        public GilesPlayerCache()
        {
            LastUpdated = DateTime.MinValue;
            IsIncapacitated = false;
            IsRooted = false;
            IsInTown = false;
            CurrentHealthPct = 0;
            PrimaryResource = 0;
            PrimaryResourcePct = 0;
            SecondaryResource = 0;
            SecondaryResourcePct = 0;
            CurrentPosition = Vector3.Zero;
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
            CurrentHealthPct = currentHealth;
            PrimaryResource = currentEnergy;
            PrimaryResourcePct = currentEnergyPct;
            SecondaryResource = discipline;
            SecondaryResourcePct = disciplinePct;
            CurrentPosition = currentPosition;
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

        /// <summary>
        /// Update the cached data on the player information, including buffs if needed
        /// </summary>
        internal static void UpdateCachedPlayerData()
        {
            using (new PerformanceLogger("UpdateCachedPlayerData"))
            {
                if (DateTime.Now.Subtract(PlayerStatus.LastUpdated).TotalMilliseconds <= 100)
                    return;
                // If we aren't in the game of a world is loading, don't do anything yet
                if (!ZetaDia.IsInGame || ZetaDia.IsLoadingWorld)
                    return;
                var me = ZetaDia.Me;
                if (me == null)
                    return;

                try
                {
                    PlayerStatus.LastUpdated = DateTime.Now;
                    PlayerStatus.IsInTown = me.IsInTown;
                    PlayerStatus.IsDead = me.IsDead;
                    PlayerStatus.IsInGame = ZetaDia.IsInGame;
                    PlayerStatus.IsLoadingWorld = ZetaDia.IsLoadingWorld;

                    PlayerStatus.IsIncapacitated = (me.IsFeared || me.IsStunned || me.IsFrozen || me.IsBlind);
                    PlayerStatus.IsRooted = me.IsRooted;

                    PlayerStatus.CurrentHealthPct = me.HitpointsCurrentPct;
                    PlayerStatus.PrimaryResource = me.CurrentPrimaryResource;
                    PlayerStatus.PrimaryResourcePct = PlayerStatus.PrimaryResource / me.MaxPrimaryResource;
                    PlayerStatus.SecondaryResource = me.CurrentSecondaryResource;
                    PlayerStatus.SecondaryResourcePct = PlayerStatus.SecondaryResource / me.MaxSecondaryResource;
                    PlayerStatus.CurrentPosition = me.Position;

                    PlayerStatus.GoldPickupRadius = me.GoldPickupRadius;
                    PlayerStatus.Coinage = me.Inventory.Coinage;

                    if (PlayerStatus.PrimaryResource >= GilesTrinity.MinEnergyReserve)
                        PlayerStatus.WaitingForReserveEnergy = false;
                    if (PlayerStatus.PrimaryResource < 20)
                        PlayerStatus.WaitingForReserveEnergy = true;


                    PlayerStatus.MyDynamicID = me.CommonData.DynamicId;
                    PlayerStatus.Level = me.Level;
                    PlayerStatus.ActorClass = me.ActorClass;
                    PlayerStatus.BattleTag = ZetaDia.Service.CurrentHero.BattleTagName;
                    PlayerStatus.LevelAreaId = ZetaDia.CurrentLevelAreaId;

                    if (PlayerStatus.ActorClass == ActorClass.WitchDoctor && HotbarSkills.AssignedSkills.Any(s => s.Power == SNOPower.Witchdoctor_Hex && s.RuneIndex == 1))
                        PlayerStatus.IsHidden = me.IsHidden;
                    else
                        PlayerStatus.IsHidden = false;

                    if (DateTime.Now.Subtract(PlayerStatus.Scene.LastUpdate).TotalMilliseconds > 1000 && GilesTrinity.Settings.Combat.Misc.UseNavMeshTargeting)
                    {
                        int CurrentSceneSNO = -1;
                        CurrentSceneSNO = (int)ZetaDia.Me.SceneId;
                        if (PlayerStatus.SceneId != CurrentSceneSNO)
                        {
                            PlayerStatus.SceneId = CurrentSceneSNO;
                            DbHelper.Log(TrinityLogLevel.Debug, LogCategory.CacheManagement, "Updating Grid Provider", true);
                            NavHelper.UpdateSearchGridProvider();
                        }
                    }

                    // World ID safety caching incase it's ever unavailable
                    GilesTrinity.CurrentWorldDynamicId = ZetaDia.CurrentWorldDynamicId;
                    PlayerStatus.WorldDynamicID = ZetaDia.CurrentWorldDynamicId;
                    PlayerStatus.WorldID = ZetaDia.CurrentWorldId;
                    GilesTrinity.cachedStaticWorldId = ZetaDia.CurrentWorldId;
                    // Game difficulty, used really for vault on DH's
                    GilesTrinity.iCurrentGameDifficulty = ZetaDia.Service.CurrentHero.CurrentDifficulty;

                    // Refresh player buffs (to check for archon)
                    RefreshBuffs();
                }
                catch (Exception ex)
                {
                    DbHelper.Log(TrinityLogLevel.Debug, LogCategory.CacheManagement, "Safely handled exception for grabbing player data.{0}{1}", Environment.NewLine, ex);
                }
            }
        }

        /// <summary>
        /// Refreshes all player buffs
        /// </summary>
        internal static void RefreshBuffs()
        {
            using (new PerformanceLogger("GilesRefreshBuffs"))
            {

                GilesTrinity.dictCachedBuffs = new Dictionary<int, int>();
                GilesTrinity.listCachedBuffs = ZetaDia.Me.GetAllBuffs().ToList();
                // Special flag for detecting the activation and de-activation of archon
                bool bThisArchonBuff = false;
                int iTempStackCount;
                // Store how many stacks of each buff we have
                foreach (Buff thisbuff in GilesTrinity.listCachedBuffs)
                {
                    // Store the stack count of this buff
                    if (!GilesTrinity.dictCachedBuffs.TryGetValue(thisbuff.SNOId, out iTempStackCount))
                        GilesTrinity.dictCachedBuffs.Add(thisbuff.SNOId, thisbuff.StackCount);
                    // Check for archon stuff
                    if (thisbuff.SNOId == (int)SNOPower.Wizard_Archon)
                        bThisArchonBuff = true;
                }
                // Archon stuff
                if (bThisArchonBuff)
                {
                    if (!GilesTrinity.HasHadArchonbuff)
                        GilesTrinity.ShouldRefreshHotbarAbilities = true;
                    GilesTrinity.HasHadArchonbuff = true;
                }
                else
                {
                    if (GilesTrinity.HasHadArchonbuff)
                    {
                        GilesTrinity.Hotbar = new HashSet<SNOPower>(GilesTrinity.hashCachedPowerHotbarAbilities);
                    }
                    GilesTrinity.HasHadArchonbuff = false;
                }
            }
        }


        private static GilesPlayerCache PlayerStatus
        {
            get
            {
                return GilesTrinity.PlayerStatus;
            }
            set
            {
                GilesTrinity.PlayerStatus = value;
            }
        }


        /// <summary>
        /// Re-reads the active assigned skills and runes from thoe hotbar
        /// </summary>
        internal static void RefreshHotbar()
        {
            using (new PerformanceLogger("RefreshHotbar"))
            {
                GilesTrinity.Hotbar = new HashSet<SNOPower>();
                for (int i = 0; i <= 5; i++)
                {
                    GilesTrinity.Hotbar.Add(ZetaDia.CPlayer.GetPowerForSlot((HotbarSlot)i));
                }
                GilesTrinity.HasMappedPlayerAbilities = true;
                GilesTrinity.ShouldRefreshHotbarAbilities = false;
                GilesTrinity.HotbarRefreshTimer.Restart();

                HotbarSkills.Update();

                if (!GilesTrinity.GetHasBuff(SNOPower.Wizard_Archon) && !PlayerStatus.IsHidden)
                    GilesTrinity.hashCachedPowerHotbarAbilities = new HashSet<SNOPower>(GilesTrinity.Hotbar);
            }
        }
    }
}
