using System;
using System.Linq;
using Trinity.Cache;
using Trinity.Helpers;
using Trinity.Reference;
using Trinity.Technicals;
using Zeta.Bot;
using Zeta.Common;
using Zeta.Game;
using Zeta.Game.Internals.Actors;
using Logger = Trinity.Technicals.Logger;

namespace Trinity
{
    public partial class CacheData
    {
        /// <summary>
        /// Fast Player Cache, Self-Updating, use instead of ZetaDia.Me / ZetaDia.Cplayer
        /// </summary>
        public class PlayerCache
        {
            static PlayerCache()
            {
                Pulsator.OnPulse += (sender, args) => Instance.UpdatePlayerCache();            
            }

            public PlayerCache()
            {
                // Make sure data is immediately available 
                // while bot is not running or before pulse starts
                UpdatePlayerCache();                
            }

            private static PlayerCache _instance = null;
            public static PlayerCache Instance
            {
                get { return _instance ?? (_instance = new PlayerCache()); }
                set { _instance = value; }
            }

            public int ActorSNO { get; private set; }
			public int ACDGuid { get; private set; }
            public int RActorGuid { get; private set; }
            public ACD CommonData { get; private set; }
            public DateTime LastUpdated { get; private set; }
            public bool IsIncapacitated { get; private set; }
            public bool IsRooted { get; private set; }
            public bool IsInRift { get; private set; }
            public double CurrentHealthPct { get; private set; }
            public double PrimaryResource { get; private set; }
            public double PrimaryResourcePct { get; private set; }
            public double PrimaryResourceMax { get; private set; }
            public double PrimaryResourceMissing { get; private set; }
            public double SecondaryResource { get; private set; }
            public double SecondaryResourcePct { get; private set; }
            public double SecondaryResourceMax { get; private set; }
			public double SecondaryResourceMissing { get; private set; }
			public float CooldownReductionPct { get; private set; }
			public float ResourceCostReductionPct { get; private set; }
			public Vector3 Position { get; private set; }
			public int MyDynamicID { get; private set; }
			public int Level { get; private set; }
			public ActorClass ActorClass { get; private set; }
			public string BattleTag { get; private set; }
			public int SceneId { get; private set; }
			public int LevelAreaId { get; private set; }
			public double PlayerDamagePerSecond { get; private set; }
			public SceneInfo Scene { get; private set; }
			public int WorldDynamicID { get; private set; }
			public int WorldID { get; private set; }
			public bool IsInGame { get; private set; }
			public bool IsDead { get; private set; }
			public bool IsLoadingWorld { get; private set; }
			public int Coinage { get; private set; }
			public float GoldPickupRadius { get; private set; }
			public bool IsHidden { get; private set; }
			public int CurrentExperience { get; private set; }
			public int ExperienceNextLevel { get; private set; }
			public int ParagonLevel { get; private set; }
			public int ParagonCurrentExperience { get; private set; }
			public long ParagonExperienceNextLevel { get; private set; }
			public float Rotation { get; private set; }
			public Vector2 DirectionVector { get; private set; }
			public float MovementSpeed { get; private set; }
			public bool IsMoving { get; private set; }
			public bool IsGhosted { get; private set; }
			public bool IsInPandemoniumFortress { get; private set; }
			public GameDifficulty GameDifficulty { get; private set; }
			public TrinityBountyInfo ActiveBounty { get; private set; }
			public bool InActiveEvent { get; private set; }
			public bool HasEventInspectionTask { get; private set; }
			public bool ParticipatingInTieredLootRun { get; private set; }
			public bool IsInTown { get; private set; }
			public bool IsInCombat { get; private set; }
            public int BloodShards { get; private set; }
            public bool IsRanged { get; private set; }
            public bool IsValid { get; private set; }
            public int TieredLootRunlevel { get; private set; }
            public int CurrentQuestSNO { get; private set; }
            public int CurrentQuestStep { get; private set; }
            public Act WorldType { get; private set; }
            public bool StandingInAvoidance { get; set; }
            public bool TryToAvoidProjectile { get; set; }
            public bool AvoidDeath { get; set; }
            public bool NeedToKite { get; set; }

            public bool HasDebuff(SNOPower debuffSNO)
            {
                try
                {
                    if (CommonData.GetAttribute<int>(((int)debuffSNO << 12) + ((int)ActorAttributeType.PowerBuff0VisualEffect & 0xFFF)) == 1)
                        return true;
                    if (CommonData.GetAttribute<int>(((int)debuffSNO << 12) + ((int)ActorAttributeType.PowerBuff0VisualEffectA & 0xFFF)) == 1)
                        return true;
                    if (CommonData.GetAttribute<int>(((int)debuffSNO << 12) + ((int)ActorAttributeType.PowerBuff0VisualEffectB & 0xFFF)) == 1)
                        return true;
                    if (CommonData.GetAttribute<int>(((int)debuffSNO << 12) + ((int)ActorAttributeType.PowerBuff0VisualEffectC & 0xFFF)) == 1)
                        return true;
                    if (CommonData.GetAttribute<int>(((int)debuffSNO << 12) + ((int)ActorAttributeType.PowerBuff0VisualEffectD & 0xFFF)) == 1)
                        return true;
                    if (CommonData.GetAttribute<int>(((int)debuffSNO << 12) + ((int)ActorAttributeType.PowerBuff0VisualEffectE & 0xFFF)) == 1)
                        return true;

                }
                catch (Exception) { }
                return false;
            }

            public class SceneInfo
            {
                public DateTime LastUpdate { get; set; }
                public int SceneId { get; set; }
            }

            internal static DateTime LastSlowUpdate = DateTime.MinValue;
            internal static DateTime LastVerySlowUpdate = DateTime.MinValue;
			internal static DiaActivePlayer _me;

			internal void UpdatePlayerCache()
			{               
				using (new PerformanceLogger("UpdateCachedPlayerData"))
				{
					if (DateTime.UtcNow.Subtract(LastUpdated).TotalMilliseconds <= 100)
						return;

					if (!ZetaDia.IsInGame)
					{
                        IsInGame = false;
                        IsValid = false;
						return;
					}

					if (ZetaDia.IsLoadingWorld)
					{
                        IsLoadingWorld = true;
                        IsValid = false;
						return;
					}
                    
					_me = ZetaDia.Me;
					if (_me == null || !_me.IsFullyValid())
					{
                        IsValid = false;
						return;
					}

					try
					{
                        IsValid = true;
                        IsInGame = true;
                        IsLoadingWorld = false;

                        LevelAreaId = ZetaDia.CurrentLevelAreaId;
                        WorldDynamicID = ZetaDia.CurrentWorldDynamicId;
                        WorldID = ZetaDia.CurrentWorldId;

                        Trinity.CurrentWorldDynamicId = WorldDynamicID;
                        Trinity.CurrentWorldId = WorldID;

						if (DateTime.UtcNow.Subtract(LastVerySlowUpdate).TotalMilliseconds > 5000)
							UpdateVerySlowChangingData();

					    if (DateTime.UtcNow.Subtract(LastSlowUpdate).TotalMilliseconds > 1000)					   
                            UpdateSlowChangingData();								                  

                        UpdateFastChangingData();

					}
					catch (Exception ex)
					{
						Logger.Log(TrinityLogLevel.Debug, LogCategory.CacheManagement, "Safely handled exception for grabbing player data.{0}{1}", Environment.NewLine, ex);
					}
				}
			}

			internal void UpdateFastChangingData()
			{
                ActorSNO = _me.ActorSNO;
                ACDGuid = _me.ACDGuid;
                RActorGuid = _me.RActorGuid;
                CommonData = _me.CommonData;
                LastUpdated = DateTime.UtcNow;
                IsInTown = DataDictionary.TownLevelAreaIds.Contains(LevelAreaId);
                IsInRift = DataDictionary.RiftWorldIds.Contains(WorldID);
                IsDead = _me.IsDead;
                IsIncapacitated = (_me.IsFeared || _me.IsStunned || _me.IsFrozen || _me.IsBlind);
                IsRooted = _me.IsRooted;
                CurrentHealthPct = _me.HitpointsCurrentPct;
                PrimaryResource = _me.CurrentPrimaryResource;
                PrimaryResourcePct = PrimaryResource / PrimaryResourceMax;
                PrimaryResourceMissing = PrimaryResourceMax - PrimaryResource;
                SecondaryResource = _me.CurrentSecondaryResource;
                SecondaryResourcePct = SecondaryResource / SecondaryResourceMax;
                SecondaryResourceMissing = SecondaryResourceMax - SecondaryResource;
                Position = _me.Position;
                Rotation = _me.Movement.Rotation;
                DirectionVector = _me.Movement.DirectionVector;
                MovementSpeed = _me.Movement.SpeedXY;
                IsMoving = _me.Movement.IsMoving;
                IsInCombat = _me.IsInCombat;       

                // For WD Angry Chicken
                IsHidden = _me.IsHidden;
			}

			internal void UpdateSlowChangingData()
			{
                BloodShards = ZetaDia.CPlayer.BloodshardCount;
                MyDynamicID = _me.CommonData.DynamicId;
                ParticipatingInTieredLootRun = _me.IsParticipatingInTieredLootRun;
                TieredLootRunlevel = _me.InTieredLootRunLevel;

                Coinage = _me.Inventory.Coinage;
                CurrentExperience = ZetaDia.Me.CurrentExperience;

                IsInPandemoniumFortress = DataDictionary.PandemoniumFortressWorlds.Contains(WorldID) ||
                        DataDictionary.PandemoniumFortressLevelAreaIds.Contains(LevelAreaId);

                if (CurrentHealthPct > 0)
                    IsGhosted = _me.CommonData.GetAttribute<int>(ActorAttributeType.Ghosted) > 0;

                if (Trinity.Settings.Combat.Misc.UseNavMeshTargeting)
                    SceneId = _me.SceneId;

				// Step 13 is used when the player needs to go "Inspect the cursed shrine"
				// Step 1 is event in progress, kill stuff
				// Step 2 is event completed
				// Step -1 is not started
                InActiveEvent = ZetaDia.ActInfo.ActiveQuests.Any(q => DataDictionary.EventQuests.Contains(q.QuestSNO) && q.QuestStep != 13);
                HasEventInspectionTask = ZetaDia.ActInfo.ActiveQuests.Any(q => DataDictionary.EventQuests.Contains(q.QuestSNO) && q.QuestStep == 13);

                WorldType = ZetaDia.WorldType;
                if (WorldType != Act.OpenWorld)
                {
                    // Update these only with campaign
                    CurrentQuestSNO = ZetaDia.CurrentQuest.QuestSNO;
                    CurrentQuestStep = ZetaDia.CurrentQuest.StepId;
                }

				LastSlowUpdate = DateTime.UtcNow;            
			}

			internal void UpdateVerySlowChangingData()
			{
                Level = _me.Level;
                ActorClass = _me.ActorClass;
                BattleTag = FileManager.BattleTagName;
                CooldownReductionPct = ZetaDia.Me.CommonData.GetAttribute<float>(ActorAttributeType.PowerCooldownReductionPercentAll);
                ResourceCostReductionPct = ZetaDia.Me.CommonData.GetAttribute<float>(ActorAttributeType.ResourceCostReductionPercentAll);
                GoldPickupRadius = _me.GoldPickupRadius;
                ExperienceNextLevel = ZetaDia.Me.ExperienceNextLevel;
                ParagonLevel = ZetaDia.Me.ParagonLevel;
                ParagonCurrentExperience = ZetaDia.Me.ParagonCurrentExperience;
                ParagonExperienceNextLevel = ZetaDia.Me.ParagonExperienceNextLevel;
                GameDifficulty = ZetaDia.Service.Hero.CurrentDifficulty;
                SecondaryResourceMax = _me.MaxSecondaryResource;
                PrimaryResourceMax = _me.MaxPrimaryResource;
				LastVerySlowUpdate = DateTime.UtcNow;			    
                switch (ActorClass)
                {
                    case ActorClass.Witchdoctor:
                        IsRanged = !Sets.RaimentOfTheJadeHarvester.IsMaxBonusActive;
                        break;
                    case ActorClass.DemonHunter:
                    case ActorClass.Wizard:
                        IsRanged = true;
                        break;
                    case ActorClass.Monk:
                        IsRanged = Legendary.TzoKrinsGaze.IsEquipped;
                        break;
                    default:
                        IsRanged = false;
                        break;
                }
			}

			public void Clear()
			{
                LastUpdated = DateTime.MinValue;
                LastSlowUpdate = DateTime.MinValue;
                LastVerySlowUpdate = DateTime.MinValue;
                IsIncapacitated = false;
                IsRooted = false;
                IsInTown = false;
                CurrentHealthPct = 0;
                PrimaryResource = 0;
                PrimaryResourcePct = 0;
                SecondaryResource = 0;
                SecondaryResourcePct = 0;
                Position = Vector3.Zero;
                MyDynamicID = -1;
                Level = -1;
                ActorClass = ActorClass.Invalid;
                BattleTag = String.Empty;
                SceneId = -1;
                LevelAreaId = -1;
				Scene = new SceneInfo()
				{
					SceneId = -1,
					LastUpdate = DateTime.UtcNow
				};
			}

            public void ForceUpdates()
            {
                LastUpdated = DateTime.MinValue;
                LastSlowUpdate = DateTime.MinValue;
                LastVerySlowUpdate = DateTime.MinValue;
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
				return false;
			}

		}
    }
}
