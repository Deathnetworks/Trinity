using System;
using System.Collections.Generic;
using System.Linq;
using Trinity.Cache;
using Trinity.Combat;
using Trinity.Combat.Abilities;
using Trinity.Technicals;
using Zeta.Bot;
using Zeta.Common;
using Zeta.Game;
using Zeta.Game.Internals.Actors;
using Logger = Trinity.Technicals.Logger;

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
        public bool IsInRift { get; set; }
        public double CurrentHealthPct { get; set; }
        public double PrimaryResource { get; set; }
        public double PrimaryResourcePct { get; set; }
        public double PrimaryResourceMax { get; set; }
        public double PrimaryResourceMissing { get; set; }

        public double SecondaryResource { get; set; }
        public double SecondaryResourcePct { get; set; }
        public double SecondaryResourceMax { get; set; }
        public double SecondaryResourceMissing { get; set; }

        public float CooldownReductionPct { get; set; }
        public float ResourceCostReductionPct { get; set; }

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
        public int CurrentExperience { get; set; }
        public int ExperienceNextLevel { get; set; }
        public int ParagonLevel { get; set; }
        public int ParagonCurrentExperience { get; set; }
        public long ParagonExperienceNextLevel { get; set; }
        public float Rotation { get; set; }
        public Vector2 DirectionVector { get; set; }
        public float MovementSpeed { get; set; }
        public bool IsMoving { get; set; }
        public bool IsGhosted { get; set; }
        public GameDifficulty GameDifficulty { get; set; }

        public TrinityBountyInfo ActiveBounty { get; set; }

        public bool InActiveEvent { get; set; }
        public bool HasEventInspectionTask { get; set; }
        public EquippedItemCache EquippedItemCache { get { return EquippedItemCache.Instance; } }

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
            ActorClass = Zeta.Game.ActorClass.Invalid;
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
                LastUpdate = DateTime.UtcNow
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
                if (DateTime.UtcNow.Subtract(Player.LastUpdated).TotalMilliseconds <= 100)
                    return;
                // If we aren't in the game of a world is loading, don't do anything yet
                if (!ZetaDia.IsInGame || ZetaDia.IsLoadingWorld)
                    return;
                var me = ZetaDia.Me;
                if (me == null)
                    return;

                try
                {
                    // chicken / archon
                    //me.SkillOverrideActive

                    Player.ACDGuid = me.ACDGuid;
                    Player.RActorGuid = me.RActorGuid;
                    Player.LastUpdated = DateTime.UtcNow;
                    Player.IsInTown = ZetaDia.IsInTown;
                    Trinity.CurrentWorldDynamicId = ZetaDia.CurrentWorldDynamicId;
                    Trinity.CurrentWorldId = ZetaDia.CurrentWorldId;
                    Player.WorldDynamicID = ZetaDia.CurrentWorldDynamicId;
                    Player.WorldID = ZetaDia.CurrentWorldId;
                    Player.IsInRift = DataDictionary.RiftWorldIds.Contains(Player.WorldID);
                    Player.IsDead = me.IsDead;
                    Player.IsInGame = ZetaDia.IsInGame;
                    Player.IsLoadingWorld = ZetaDia.IsLoadingWorld;

                    Player.IsIncapacitated = (me.IsFeared || me.IsStunned || me.IsFrozen || me.IsBlind);
                    Player.IsRooted = me.IsRooted;

                    Player.CurrentHealthPct = me.HitpointsCurrentPct;
                    Player.PrimaryResource = me.CurrentPrimaryResource;
                    Player.PrimaryResourcePct = Player.PrimaryResource / me.MaxPrimaryResource;
                    Player.PrimaryResourceMax = me.MaxPrimaryResource;
                    Player.PrimaryResourceMissing = Player.PrimaryResourceMax - Player.PrimaryResource;

                    Player.SecondaryResource = me.CurrentSecondaryResource;
                    Player.SecondaryResourcePct = Player.SecondaryResource / me.MaxSecondaryResource;
                    Player.SecondaryResourceMax = me.MaxSecondaryResource;
                    Player.SecondaryResourceMissing = Player.SecondaryResourceMax - Player.SecondaryResource;

                    Player.Position = me.Position;
                    Player.Rotation = me.Movement.Rotation;
                    Player.DirectionVector = me.Movement.DirectionVector;
                    Player.MovementSpeed = me.Movement.SpeedXY;
                    Player.IsMoving = me.Movement.IsMoving;

                    Player.GoldPickupRadius = me.GoldPickupRadius;
                    Player.Coinage = me.Inventory.Coinage;

                    if (Player.PrimaryResource >= Trinity.MinEnergyReserve)
                        Player.WaitingForReserveEnergy = false;
                    if (Player.PrimaryResource < 20)
                        Player.WaitingForReserveEnergy = true;

                    Player.MyDynamicID = me.CommonData.DynamicId;
                    Player.Level = me.Level;
                    Player.ActorClass = me.ActorClass;
                    Player.BattleTag = FileManager.BattleTagName;
                    Player.LevelAreaId = ZetaDia.CurrentLevelAreaId;

                    Player.CooldownReductionPct = ZetaDia.Me.CommonData.GetAttribute<float>(ActorAttributeType.PowerCooldownReductionPercentAll);
                    Player.ResourceCostReductionPct = ZetaDia.Me.CommonData.GetAttribute<float>(ActorAttributeType.ResourceCostReductionPercentAll);                        

                    Player.CurrentExperience = ZetaDia.Me.CurrentExperience;
                    Player.ExperienceNextLevel = ZetaDia.Me.ExperienceNextLevel;
                    Player.ParagonLevel = ZetaDia.Me.ParagonLevel;
                    Player.ParagonCurrentExperience = ZetaDia.Me.ParagonCurrentExperience;
                    Player.ParagonExperienceNextLevel = ZetaDia.Me.ParagonExperienceNextLevel;

                    Player.IsHidden = me.IsHidden;
                    Player.GameDifficulty = ZetaDia.Service.Hero.CurrentDifficulty;

                    Player.EquippedItemCache.Update();

                    if (Player.CurrentHealthPct > 0)
                        Player.IsGhosted = ZetaDia.Me.CommonData.GetAttribute<int>(ActorAttributeType.Ghosted) > 0;

                    if (DateTime.UtcNow.Subtract(Player.Scene.LastUpdate).TotalMilliseconds > 1000 && Trinity.Settings.Combat.Misc.UseNavMeshTargeting)
                    {
                        int CurrentSceneSNO = -1;
                        CurrentSceneSNO = (int)ZetaDia.Me.SceneId;
                        if (Player.SceneId != CurrentSceneSNO)
                        {
                            Player.SceneId = CurrentSceneSNO;
                        }
                    }


                    using (new PerformanceLogger("BountyInfo"))
                    {

                        // Step 13 is used when the player needs to go "Inspect the cursed shrine"
                        // Step 1 is event in progress, kill stuff
                        // Step 2 is event completed
                        // Step -1 is not started
                        Player.InActiveEvent = ZetaDia.ActInfo.ActiveQuests.Any(q => DataDictionary.EventQuests.Contains(q.QuestSNO) && q.QuestStep != 13);
                        Player.HasEventInspectionTask = ZetaDia.ActInfo.ActiveQuests.Any(q => DataDictionary.EventQuests.Contains(q.QuestSNO) && q.QuestStep == 13);

                    }

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
                var oldBuffs = Trinity.PlayerBuffs.Keys.Cast<SNOPower>().ToList();

                Trinity.PlayerBuffs = new Dictionary<int, int>();
                Trinity.listCachedBuffs = ZetaDia.Me.GetAllBuffs().ToList();

                // This contains only NEW buffs, e.g. buffs which we didn't have the last tick
                var newBuffs = Trinity.listCachedBuffs.Select(b => b.SNOId).Cast<SNOPower>().ToList().Except(oldBuffs);

                if (newBuffs.Contains(SNOPower.Pages_Buff_Infinite_Casting))
                {
                    foreach (var power in CacheData.AbilityLastUsed.Keys.ToList())
                    {
                        CacheData.AbilityLastUsed[power] = DateTime.MinValue;
                    }
                }

                // Special flag for detecting the activation and de-activation of archon
                bool archonBuff = false;
                int stackCount;
                string buffList = "";
                Trinity.GotFrenzyShrine = false;
                Trinity.GotBlessedShrine = false;
                // Store how many stacks of each buff we have
                foreach (Buff buff in Trinity.listCachedBuffs)
                {
                    buffList += " " + buff.InternalName + " (SNO: " + buff.SNOId + " stack: " + buff.StackCount + ")";

                    // Store the stack count of this buff
                    if (!Trinity.PlayerBuffs.TryGetValue(buff.SNOId, out stackCount))
                        Trinity.PlayerBuffs.Add(buff.SNOId, buff.StackCount);
                    // Check for archon stuff
                    if (buff.SNOId == (int)SNOPower.Wizard_Archon)
                        archonBuff = true;
                    if (buff.SNOId == 30476) //Blessed (+25% defence)
                        Trinity.GotBlessedShrine = true;
                    if (buff.SNOId == 30479) //Frenzy  (+25% atk speed)
                        Trinity.GotFrenzyShrine = true;
                }
                Logger.Log(TrinityLogLevel.Debug, LogCategory.CacheManagement, "Refreshed buffs: " + buffList);
                // Archon stuff
                if (archonBuff)
                {
                    if (!Trinity.HasHadArchonbuff)
                        Trinity.ShouldRefreshHotbarAbilities = true;
                    Trinity.HasHadArchonbuff = true;
                }
                else
                {
                    if (Trinity.HasHadArchonbuff)
                    {
                        Trinity.Hotbar = new List<SNOPower>(Trinity.hashCachedPowerHotbarAbilities);
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
                // Update Hotbar Skills first
                HotbarSkills.Update();

                SpellTracker.RefreshCachedSpells();

                if (!Trinity.GetHasBuff(SNOPower.Wizard_Archon) && !Player.IsHidden)
                    Trinity.hashCachedPowerHotbarAbilities = new List<SNOPower>(Trinity.Hotbar);
            }

            // Monk Seven Sided Strike: Sustained Attack
            if (Player.ActorClass == ActorClass.Monk && HotbarSkills.AssignedSkills.Any(s => s.Power == SNOPower.Monk_SevenSidedStrike && s.RuneIndex == 3))
                CombatBase.SetSNOPowerUseDelay(SNOPower.Monk_SevenSidedStrike, 17000);

            if (Player.ActorClass == ActorClass.Witchdoctor && HotbarSkills.PassiveSkills.Contains(SNOPower.Witchdoctor_Passive_GraveInjustice))
            {
                CombatBase.SetSNOPowerUseDelay(SNOPower.Witchdoctor_SoulHarvest, 1000);
                CombatBase.SetSNOPowerUseDelay(SNOPower.Witchdoctor_SpiritWalk, 1000);
                CombatBase.SetSNOPowerUseDelay(SNOPower.Witchdoctor_Horrify, 1000);
                CombatBase.SetSNOPowerUseDelay(SNOPower.Witchdoctor_Gargantuan, 20000);
                CombatBase.SetSNOPowerUseDelay(SNOPower.Witchdoctor_SummonZombieDog, 20000);
                CombatBase.SetSNOPowerUseDelay(SNOPower.Witchdoctor_GraspOfTheDead, 500);
                CombatBase.SetSNOPowerUseDelay(SNOPower.Witchdoctor_SpiritBarrage, 2000);
                CombatBase.SetSNOPowerUseDelay(SNOPower.Witchdoctor_Locust_Swarm, 2000);
                CombatBase.SetSNOPowerUseDelay(SNOPower.Witchdoctor_Haunt, 2000);
                CombatBase.SetSNOPowerUseDelay(SNOPower.Witchdoctor_Hex, 3000);
                CombatBase.SetSNOPowerUseDelay(SNOPower.Witchdoctor_MassConfusion, 15000);
                CombatBase.SetSNOPowerUseDelay(SNOPower.Witchdoctor_FetishArmy, 20000);
                CombatBase.SetSNOPowerUseDelay(SNOPower.Witchdoctor_BigBadVoodoo, 20000);
            }
            if (Player.ActorClass == ActorClass.Barbarian && HotbarSkills.PassiveSkills.Contains(SNOPower.Barbarian_Passive_BoonOfBulKathos))
            {
                CombatBase.SetSNOPowerUseDelay(SNOPower.Barbarian_Earthquake, 90500);
                CombatBase.SetSNOPowerUseDelay(SNOPower.Barbarian_CallOfTheAncients, 90500);
                CombatBase.SetSNOPowerUseDelay(SNOPower.Barbarian_WrathOfTheBerserker, 90500);
            }
        }

        public static void DumpPlayerSkills()
        {
            if (BotMain.IsRunning)
                BotMain.Stop();

            using (var helper = new Helpers.ZetaCacheHelper())
            {
                HotbarSkills.Update(TrinityLogLevel.Info, LogCategory.UserInformation);

                foreach (var skill in HotbarSkills.PassiveSkills.ToList())
                {
                    Logger.Log("Passive: {0}", skill);
                }
            }
        }

        public bool IsFacing(Vector3 targetPosition, float arcDegrees = 70f)
        {
            if (DirectionVector != Vector2.Zero)
            {
                Vector3 u = targetPosition - this.Position;
                u.Z = 0f;
                Vector3 v = new Vector3(DirectionVector.X, DirectionVector.Y, 0f);
                bool result = ((MathEx.ToDegrees(Vector3.AngleBetween(u, v)) <= arcDegrees) ? 1 : 0) != 0;
                return result;
            }
            else
                return false;
        }

    }
}
