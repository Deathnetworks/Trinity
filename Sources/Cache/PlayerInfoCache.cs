using System;
using System.Linq;
using System.Collections.Generic;
using Trinity.Technicals;
using Zeta;
using Zeta.Common;
using Zeta.Internals.Actors;

namespace Trinity
{
    // Current cached player data
    // Just stores the data on YOU, well, your character's current status - for readability of those variables more than anything, but also caching
    public class PlayerInfoCache
    {
        public int ACDGuid { get; set; }
        public int RActorGuid { get; set; }
        public DateTime LastUpdated { get; set; }
        public bool IsIncapacitated { get; set; }
        public bool IsRooted { get; set; }
        public bool IsInTown { get; set; }
        public double CurrentHealthPct { get; set; }
        public double PrimaryResource { get; set; }
        public double PrimaryResourcePct { get; set; }
        public double SecondaryResource { get; set; }
        public double SecondaryResourcePct { get; set; }
        public Vector3 Position { get; set; }
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

        public PlayerInfoCache()
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
            Position = Vector3.Zero;
            WaitingForReserveEnergy = false;
            MyDynamicID = -1;
            Level = -1;
            ActorClass = Zeta.Internals.Actors.ActorClass.Invalid;
            BattleTag = String.Empty;
            SceneId = -1;
            LevelAreaId = -1;
        }

        public PlayerInfoCache(
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
            Position = currentPosition;
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
                if (DateTime.Now.Subtract(Player.LastUpdated).TotalMilliseconds <= 100)
                    return;
                // If we aren't in the game of a world is loading, don't do anything yet
                if (!ZetaDia.IsInGame || ZetaDia.IsLoadingWorld)
                    return;
                var me = ZetaDia.Me;
                if (me == null)
                    return;

                try
                {
                    Player.ACDGuid = me.ACDGuid;
                    Player.RActorGuid = me.RActorGuid;
                    Player.LastUpdated = DateTime.Now;
                    Player.IsInTown = me.IsInTown;
                    Player.IsDead = me.IsDead;
                    Player.IsInGame = ZetaDia.IsInGame;
                    Player.IsLoadingWorld = ZetaDia.IsLoadingWorld;

                    Player.IsIncapacitated = (me.IsFeared || me.IsStunned || me.IsFrozen || me.IsBlind);
                    Player.IsRooted = me.IsRooted;

                    Player.CurrentHealthPct = me.HitpointsCurrentPct;
                    Player.PrimaryResource = me.CurrentPrimaryResource;
                    Player.PrimaryResourcePct = Player.PrimaryResource / me.MaxPrimaryResource;
                    Player.SecondaryResource = me.CurrentSecondaryResource;
                    Player.SecondaryResourcePct = Player.SecondaryResource / me.MaxSecondaryResource;
                    Player.Position = me.Position;

                    Player.GoldPickupRadius = me.GoldPickupRadius;
                    Player.Coinage = me.Inventory.Coinage;

                    if (Player.PrimaryResource >= Trinity.MinEnergyReserve)
                        Player.WaitingForReserveEnergy = false;
                    if (Player.PrimaryResource < 20)
                        Player.WaitingForReserveEnergy = true;


                    Player.MyDynamicID = me.CommonData.DynamicId;
                    Player.Level = me.Level;
                    Player.ActorClass = me.ActorClass;
                    Player.BattleTag = ZetaDia.Service.CurrentHero.BattleTagName;
                    Player.LevelAreaId = ZetaDia.CurrentLevelAreaId;

                    if (Player.ActorClass == ActorClass.WitchDoctor && HotbarSkills.AssignedSkills.Any(s => s.Power == SNOPower.Witchdoctor_Hex && s.RuneIndex == 1))
                        Player.IsHidden = me.IsHidden;
                    else
                        Player.IsHidden = false;

                    if (DateTime.Now.Subtract(Player.Scene.LastUpdate).TotalMilliseconds > 1000 && Trinity.Settings.Combat.Misc.UseNavMeshTargeting)
                    {
                        int CurrentSceneSNO = -1;
                        CurrentSceneSNO = (int)ZetaDia.Me.SceneId;
                        if (Player.SceneId != CurrentSceneSNO)
                        {
                            Player.SceneId = CurrentSceneSNO;
                        }
                    }

                    // World ID safety caching incase it's ever unavailable
                    Trinity.CurrentWorldDynamicId = ZetaDia.CurrentWorldDynamicId;
                    Player.WorldDynamicID = ZetaDia.CurrentWorldDynamicId;
                    Player.WorldID = ZetaDia.CurrentWorldId;
                    Trinity.cachedStaticWorldId = ZetaDia.CurrentWorldId;
                    // Game difficulty, used really for vault on DH's
                    Trinity.iCurrentGameDifficulty = ZetaDia.Service.CurrentHero.CurrentDifficulty;

                    // Refresh player buffs (to check for archon)
                    RefreshBuffs();
                }
                catch (Exception ex)
                {
                    Logger.Log(TrinityLogLevel.Debug, LogCategory.CacheManagement, "Safely handled exception for grabbing player data.{0}{1}", Environment.NewLine, ex);
                }
            }
        }

        /// <summary>
        /// Refreshes all player buffs
        /// </summary>
        internal static void RefreshBuffs()
        {
            using (new PerformanceLogger("RefreshBuffs"))
            {

                Trinity.dictCachedBuffs = new Dictionary<int, int>();
                Trinity.listCachedBuffs = ZetaDia.Me.GetAllBuffs().ToList();
                // Special flag for detecting the activation and de-activation of archon
                bool bThisArchonBuff = false;
                int iTempStackCount;
                // Store how many stacks of each buff we have
                foreach (Buff thisbuff in Trinity.listCachedBuffs)
                {
                    // Store the stack count of this buff
                    if (!Trinity.dictCachedBuffs.TryGetValue(thisbuff.SNOId, out iTempStackCount))
                        Trinity.dictCachedBuffs.Add(thisbuff.SNOId, thisbuff.StackCount);
                    // Check for archon stuff
                    if (thisbuff.SNOId == (int)SNOPower.Wizard_Archon)
                        bThisArchonBuff = true;
                }
                // Archon stuff
                if (bThisArchonBuff)
                {
                    if (!Trinity.HasHadArchonbuff)
                        Trinity.ShouldRefreshHotbarAbilities = true;
                    Trinity.HasHadArchonbuff = true;
                }
                else
                {
                    if (Trinity.HasHadArchonbuff)
                    {
                        Trinity.Hotbar = new HashSet<SNOPower>(Trinity.hashCachedPowerHotbarAbilities);
                    }
                    Trinity.HasHadArchonbuff = false;
                }
            }
        }


        private static PlayerInfoCache Player
        {
            get
            {
                return Trinity.Player;
            }
            set
            {
                Trinity.Player = value;
            }
        }


        /// <summary>
        /// Re-reads the active assigned skills and runes from thoe hotbar
        /// </summary>
        internal static void RefreshHotbar()
        {
            using (new PerformanceLogger("RefreshHotbar"))
            {
                Trinity.Hotbar = new HashSet<SNOPower>();
                for (int i = 0; i <= 5; i++)
                {
                    SNOPower power = ZetaDia.CPlayer.GetPowerForSlot((HotbarSlot)i);
                    Trinity.Hotbar.Add(power);
                    if (!DataDictionary.LastUseAbilityTimeDefaults.ContainsKey(power))
                    {
                        DataDictionary.LastUseAbilityTimeDefaults.Add(power, DateTime.Now);
                    }
                    if (!Trinity.AbilityLastUsedCache.ContainsKey(power))
                    {
                        Trinity.AbilityLastUsedCache.Add(power, DateTime.MinValue);
                    }
                }
                Trinity.HasMappedPlayerAbilities = true;
                Trinity.ShouldRefreshHotbarAbilities = false;
                Trinity.HotbarRefreshTimer.Restart();

                HotbarSkills.Update();

                SpellTracker.RefreshCachedSpells();

                if (!Trinity.GetHasBuff(SNOPower.Wizard_Archon) && !Player.IsHidden)
                    Trinity.hashCachedPowerHotbarAbilities = new HashSet<SNOPower>(Trinity.Hotbar);
            }
        }
    }
}
