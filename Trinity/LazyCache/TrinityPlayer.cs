using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;
using Trinity.Objects;
using Zeta.Common;
using Zeta.Game;
using Zeta.Game.Internals;
using Zeta.Game.Internals.Actors;
using Zeta.Game.Internals.SNO;
using Logger = Trinity.Technicals.Logger;

namespace Trinity.LazyCache
{
    /// <summary>
    /// Trinity Item
    /// </summary>
    public class TrinityPlayer : TrinityUnit
    {
        public TrinityPlayer(ACD acd) : base(acd) { }

        #region Properties

        public float CorpseResurrectionCharges
        {
            get { return CacheManager.GetCacheValue(this, o => ZetaDia.Me.CorpseResurrectionCharges, 5000); }
        }

        public float CurrentPrimaryResource
        {
            get { return CacheManager.GetCacheValue(this, o => ZetaDia.Me.CurrentPrimaryResource, 0); }
        }

        public float CurrentSecondaryResource
        {
            get { return CacheManager.GetCacheValue(this, o => ZetaDia.Me.CurrentSecondaryResource, 0); }
        }

        public float SecondaryResourcePct
        {
            get { return CacheManager.GetCacheValue(this, o => MaxPrimaryResource > 0 ? CurrentPrimaryResource / MaxPrimaryResource : 0, 50); }
        }

        public float PrimaryResourcePct
        {
            get { return CacheManager.GetCacheValue(this, o => MaxSecondaryResource > 0 ? CurrentSecondaryResource / MaxSecondaryResource : 0, 50); }
        }

        public int DeathCount
        {
            get { return CacheManager.GetCacheValue(this, o => ZetaDia.Me.DeathCount, 5000); }
        }

        public int Level
        {
            get { return CacheManager.GetCacheValue(this, o => ZetaDia.Me.Level, 5000); }
        }

        public int InTieredLootRunLevel
        {
            get { return CacheManager.GetCacheValue(this, o => ZetaDia.Me.InTieredLootRunLevel, 500); }
        }

        public int LevelCap
        {
            get { return CacheManager.GetCacheValue(this, o => ZetaDia.Me.LevelCap); }
        }

        public float MaxPrimaryResource
        {
            get { return CacheManager.GetCacheValue(this, o => ZetaDia.Me.MaxPrimaryResource); }
        }

        public float MaxSecondaryResource
        {
            get { return CacheManager.GetCacheValue(this, o => ZetaDia.Me.MaxSecondaryResource); }
        }

        public int SharedStashSlots
        {
            get { return CacheManager.GetCacheValue(this, o => ZetaDia.Me.SharedStashSlots); }
        }

        public int SkillKitId
        {
            get { return CacheManager.GetCacheValue(this, o => ZetaDia.Me.SkillKitId); }
        }

        public ActorClass ActorClass
        {
            get { return CacheManager.GetCacheValue(this, o => ZetaDia.Me.ActorClass); }
        }

        public double Armor
        {
            get { return CacheManager.GetCacheValue(this, o => ZetaDia.Me.Armor); }
        }

        public double AttacksPerSecond
        {
            get { return CacheManager.GetCacheValue(this, o => ZetaDia.Me.AttacksPerSecond); }
        }

        public double CritDamagePercent
        {
            get { return CacheManager.GetCacheValue(this, o => ZetaDia.Me.CritDamagePercent); }
        }

        public double DamageReduction
        {
            get { return CacheManager.GetCacheValue(this, o => ZetaDia.Me.DamageReduction); }
        }

        public double Dexterity
        {
            get { return CacheManager.GetCacheValue(this, o => ZetaDia.Me.Dexterity, 10000); }
        }

        public double Intelligence
        {
            get { return CacheManager.GetCacheValue(this, o => ZetaDia.Me.Intelligence, 10000); }
        }

        public double Strength
        {
            get { return CacheManager.GetCacheValue(this, o => ZetaDia.Me.Strength, 10000); }
        }

        public double Vitality
        {
            get { return CacheManager.GetCacheValue(this, o => ZetaDia.Me.Vitality, 10000); }
        }

        public bool IsAfk
        {
            get { return CacheManager.GetCacheValue(this, o => ZetaDia.Me.IsAfk, 5000); }
        }

        public bool IsBusy
        {
            get { return CacheManager.GetCacheValue(this, o => ZetaDia.Me.IsBusy); }
        }

        public bool IsInCombat
        {
            get { return CacheManager.GetCacheValue(this, o => ZetaDia.Me.IsInCombat, 0); }
        }

        public bool IsInConversation
        {
            get { return CacheManager.GetCacheValue(this, o => ZetaDia.Me.IsInConversation, 100); }
        }

        public bool IsParticipatingInTieredLootRun
        {
            get { return CacheManager.GetCacheValue(this, o => ZetaDia.Me.IsParticipatingInTieredLootRun, 250); }
        }

        public bool SkillOverrideActive
        {
            get { return CacheManager.GetCacheValue(this, o => ZetaDia.Me.SkillOverrideActive); }
        }

        public double GoldPickUpRadius
        {
            get { return CacheManager.GetCacheValue(this, o => ZetaDia.Me.GoldPickUpRadius); }
        }

        public double MeleeDamageeduction
        {
            get { return CacheManager.GetCacheValue(this, o => ZetaDia.Me.MeleeDamageeduction); }
        }

        public double CurrentExperience
        {
            get { return CacheManager.GetCacheValue(this, o => ZetaDia.Me.CurrentExperience); }
        }

        public int JewelUpgradesLeft
        {
            get { return CacheManager.GetCacheValue(this, o => ZetaDia.Me.JewelUpgradesLeft, 0); }
        }

        public int JewelUpgradesMax
        {
            get { return CacheManager.GetCacheValue(this, o => ZetaDia.Me.JewelUpgradesMax, 0); }
        }

        public int JewelUpgradesUsed
        {
            get { return CacheManager.GetCacheValue(this, o => ZetaDia.Me.JewelUpgradesUsed, 0); }
        }

        public double LoopingAnimationEndTime
        {
            get { return CacheManager.GetCacheValue(this, o => ZetaDia.Me.LoopingAnimationEndTime, 50); }
        }

        public double LoopingAnimationStartTime
        {
            get { return CacheManager.GetCacheValue(this, o => ZetaDia.Me.LoopingAnimationStartTime, 50); }
        }

        public int NumBackpackSlots
        {
            get { return CacheManager.GetCacheValue(this, o => ZetaDia.Me.NumBackpackSlots, 0); }
        }

        public int ParagonCurrentExperience
        {
            get { return CacheManager.GetCacheValue(this, o => ZetaDia.Me.ParagonCurrentExperience); }
        }

        public int ParagonLevel
        {
            get { return CacheManager.GetCacheValue(this, o => ZetaDia.Me.ParagonLevel); }
        }

        public int RestExperience
        {
            get { return CacheManager.GetCacheValue(this, o => ZetaDia.Me.RestExperience); }
        }

        public List<Buff> AllDebuffs
        {
            get { return CacheManager.GetCacheValue(this, o => ZetaDia.Me.GetAllDebuffs().ToList(), 250); }
        }

        public List<Buff> AllBuffs
        {
            get { return CacheManager.GetCacheValue(this, o => ZetaDia.Me.GetAllBuffs().ToList(), 250); }
        }

        public DiaActivePlayer.InventoryManager Inventory
        {
            get { return CacheManager.GetCacheValue(this, o => ZetaDia.Me.Inventory, 0); }
        }

        public bool CanUseTownPortal
        {
            get { return CacheManager.GetCacheValue(this, o => ZetaDia.Me.CanUseTownPortal(), 250); }
        }

        //public List<ActiveSkillEntry> KnownSkillEntrys
        //{
        //    get { return CacheManager.GetCacheValue(this, o => ZetaDia.Me.KnownSkills, 5000); }
        //}

        //public List<Skill> Skills
        //{
        //    get { return CacheManager.GetCacheValue(this, o => ZetaDia.Me.KnownSkills.Select(s => (Skill)s).ToList(), 5000); }
        //}

        //public List<TraitEntry> KnownTraitEntrys
        //{
        //    get { return CacheManager.GetCacheValue(this, o => ZetaDia.Me.KnownTraits, 5000); }
        //}

        //public List<Passive> Passives
        //{
        //    get { return CacheManager.GetCacheValue(this, o => ZetaDia.Me.KnownTraits.Select(s => (Passive)s).ToList(), 5000); }
        //}

        public float SecondaryResourceMissing
        {
            get { return CacheManager.GetCacheValue(this, o => MaxSecondaryResource - CurrentSecondaryResource, 250); }
        }

        public float PrimaryResourceMissing
        {
            get { return CacheManager.GetCacheValue(this, o => MaxPrimaryResource - CurrentPrimaryResource, 250); }
        }

        public bool IsIncapacitated
        {
            get { return CacheManager.GetCacheValue(this, o => IsBlind || IsFeared || IsFrozen || IsStunned, 250); }
        }

        public bool IsInRift
        {
            get { return CacheManager.GetCacheValue(this, o => DataDictionary.RiftWorldIds.Contains(WorldId), 50); }
        }

        public int WorldId
        {
            get { return CacheManager.GetCacheValue(this, o => ZetaDia.CurrentWorldId, 500); }         
        }

        public int Bloodshards
        {
            get { return CacheManager.GetCacheValue(this, o => ZetaDia.CPlayer.BloodshardCount, 500); }
        }

        public long Coinage
        {
            get { return CacheManager.GetCacheValue(this, o => ZetaDia.CPlayer.Coinage, 500); }
        }

        public int LevelAreaId
        {
            get { return CacheManager.GetCacheValue(this, o => ZetaDia.CurrentLevelAreaId, 500); }
        }

        public int CurrentWorldId
        {
            get { return CacheManager.GetCacheValue(this, o => ZetaDia.CurrentWorldId, 500); }
        }

        public ActInfo ActInfo
        {
            get { return CacheManager.GetCacheValue(this, o => ZetaDia.ActInfo, 500); }
        }

        public bool IsLoadingWorld
        {
            get { return CacheManager.GetCacheValue(this, o => ZetaDia.IsLoadingWorld, 100); }
        }

        public bool IsInTown
        {
            get { return CacheManager.GetCacheValue(this, o => ZetaDia.IsInTown, 250); }
        }

        public bool IsInGame
        {
            get { return CacheManager.GetCacheValue(this, o => ZetaDia.IsInGame, 250); }
        }

        public bool IsMe
        {
            get { return CacheManager.GetCacheValue(this, o => ACDGuid == ZetaDia.ActivePlayerACDGuid); }
        }

        public int RiftDeathCount
        {
            get { return CacheManager.GetCacheValue(this, o => o.Source.GetAttributeOrDefault<int>(ActorAttributeType.TieredLootRunDeathCount)); }
        }

        public int RiftRewardKey
        {
            get { return CacheManager.GetCacheValue(this, o => o.Source.GetAttributeOrDefault<int>(ActorAttributeType.TieredLootRunRewardKeyValue)); }
        }

        public double RiftResurrectionTime
        {
            get { return CacheManager.GetCacheValue(this, o => o.Source.GetAttributeOrDefault<double>(ActorAttributeType.TieredLootRunCorpseResurrectionAllowedGameTime)); }
        }

        public double StashTabsPurchased
        {
            get { return CacheManager.GetCacheValue(this, o => o.Source.GetAttributeOrDefault<double>(ActorAttributeType.StashTabsPurchasedWithGold)); }
        }

        #endregion

        #region Methods

        public void AdvanceConversation()
        {
            ZetaDia.Me.AdvanceConversation();
        }

        public void AttemptUpgradeKeystone()
        {
            ZetaDia.Me.AttemptUpgradeKeystone();
        }

        public void GetBuff(SNOPower power)
        {
            ZetaDia.Me.GetBuff(power);
        }

        public void GetDebuff(SNOPower power)
        {
            ZetaDia.Me.GetDebuff(power);
        }

        public void HasBuff(SNOPower power)
        {
            ZetaDia.Me.HasBuff(power);
        }

        public void HasDebuff(SNOPower power)
        {
            ZetaDia.Me.HasDebuff(power);
        }

        public void OpenRift(ACDItem keystone)
        {
            ZetaDia.Me.OpenRift(keystone);
        }

        public void SetActiveSkill(SNOPower power, int runeId, HotbarSlot slot)
        {
            ZetaDia.Me.SetActiveSkill(power, runeId, slot);
        }

        public void SetTraits(SNOPower t1 = SNOPower.None, SNOPower t2 = SNOPower.None, SNOPower t3 = SNOPower.None, SNOPower t4 = SNOPower.None)
        {
            ZetaDia.Me.SetTraits(t1, t2, t2, t4);
        }

        public void SkipConversation()
        {
            ZetaDia.Me.SkipConversation();
        }

        public void SkipCutscene()
        {
            ZetaDia.Me.SkipCutscene();
        }

        public void TeleportToPlayerByIndex(int playerIndex)
        {
            ZetaDia.Me.TeleportToPlayerByIndex(playerIndex);
        }

        public void UseWaypoint(int waypointIndex)
        {
            ZetaDia.Me.UseWaypoint(waypointIndex);
        }

        #endregion

        public static implicit operator TrinityPlayer(ACD x)
        {
            return CacheFactory.CreateObject<TrinityPlayer>(x);
        }

    }
}