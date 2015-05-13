using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Windows.Media.Imaging;
using Trinity.Combat.Abilities;
using Trinity.Objects;
using Zeta.Bot.Logic;
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
        public TrinityPlayer(ACD acd, int acdGuid) : base(acd, acdGuid) {}

        #region Fields

        private readonly CacheField<float> _corpseResurrectionCharges = new CacheField<float>(UpdateSpeed.VerySlow);
        private readonly CacheField<float> _currentPrimaryResource = new CacheField<float>(UpdateSpeed.RealTime);
        private readonly CacheField<float> _currentSecondaryResource = new CacheField<float>(UpdateSpeed.RealTime);
        private readonly CacheField<float> _secondaryResourcePct = new CacheField<float>(UpdateSpeed.Ultra);
        private readonly CacheField<float> _primaryResourcePct = new CacheField<float>(UpdateSpeed.Ultra);
        private readonly CacheField<float> _maxPrimaryResource = new CacheField<float>(UpdateSpeed.Ultra);
        private readonly CacheField<float> _maxSecondaryResource = new CacheField<float>(UpdateSpeed.Ultra);
        private readonly CacheField<int> _deathCount = new CacheField<int>(UpdateSpeed.VerySlow);
        private readonly CacheField<int> _level = new CacheField<int>(UpdateSpeed.VerySlow);
        private readonly CacheField<int> _tieredLootRunLevel = new CacheField<int>(UpdateSpeed.Normal);
        private readonly CacheField<bool> _inTieredLootRunLevel = new CacheField<bool>(UpdateSpeed.Normal);
        private readonly CacheField<int> _levelCap = new CacheField<int>();
        private readonly CacheField<int> _sharedStashSlots = new CacheField<int>();
        private readonly CacheField<ActorClass> _actorClass = new CacheField<ActorClass>(UpdateSpeed.VerySlow);
        private readonly CacheField<int> _armor = new CacheField<int>();
        private readonly CacheField<double> _attacksPerSecond = new CacheField<double>();
        private readonly CacheField<double> _criticalDamagePercent = new CacheField<double>();
        private readonly CacheField<double> _damageReduction = new CacheField<double>();
        private readonly CacheField<double> _dexterity = new CacheField<double>();
        private readonly CacheField<double> _strength = new CacheField<double>();
        private readonly CacheField<double> _intelligence = new CacheField<double>();
        private readonly CacheField<double> _vitality = new CacheField<double>();
        private readonly CacheField<bool> _isAfk = new CacheField<bool>(UpdateSpeed.VerySlow);
        private readonly CacheField<bool> _isBusy = new CacheField<bool>(UpdateSpeed.VerySlow);
        private readonly CacheField<bool> _isInCombat = new CacheField<bool>(UpdateSpeed.Ultra);
        private readonly CacheField<bool> _isInConversation = new CacheField<bool>(UpdateSpeed.Ultra);
        private readonly CacheField<bool> _isSkillOverrideActive = new CacheField<bool>(UpdateSpeed.Ultra);
        private readonly CacheField<bool> _isMelee = new CacheField<bool>();
        private readonly CacheField<double> _goldPickupRadius = new CacheField<double>();
        private readonly CacheField<double> _meleeDamageeduction = new CacheField<double>();
        private readonly CacheField<int> _currentExperience = new CacheField<int>();
        private readonly CacheField<int> _jewelUpgradesLeft = new CacheField<int>();
        private readonly CacheField<int> _maxBloodShards = new CacheField<int>();
        private readonly CacheField<int> _jewelUpgradesMax = new CacheField<int>();
        private readonly CacheField<int> _jewelUpgradesUsed = new CacheField<int>();
        private readonly CacheField<double> _loopingAnimationEndTime = new CacheField<double>();
        private readonly CacheField<double> _loopingAnimationStartTime = new CacheField<double>();
        private readonly CacheField<int> _backpackSlots = new CacheField<int>();
        private readonly CacheField<int> _freeBackpackSlots = new CacheField<int>(UpdateSpeed.RealTime);
        private readonly CacheField<int> _stashSlots = new CacheField<int>();
        private readonly CacheField<int> _freeStashSlots = new CacheField<int>(UpdateSpeed.RealTime); 
        private readonly CacheField<DiaActivePlayer.InventoryManager> _inventory = new CacheField<DiaActivePlayer.InventoryManager>();
        private readonly CacheField<int> _paragonCurrentExperience = new CacheField<int>();
        private readonly CacheField<int> _paragonLevel = new CacheField<int>();
        private readonly CacheField<long> _restExperience = new CacheField<long>();
        private readonly CacheField<bool> _canUseTownPortal = new CacheField<bool>(UpdateSpeed.Normal);
        private readonly CacheField<float> _secondaryResourceMissing = new CacheField<float>(UpdateSpeed.Fast);
        private readonly CacheField<float> _primaryResourceMissing = new CacheField<float>(UpdateSpeed.Fast);
        private readonly CacheField<bool> _isIncapacitated = new CacheField<bool>(UpdateSpeed.Fast);
        private readonly CacheField<bool> _isInRift = new CacheField<bool>(UpdateSpeed.Normal);
        private readonly CacheField<int> _worldId = new CacheField<int>(UpdateSpeed.Fast);
        private readonly CacheField<int> _blopdshards = new CacheField<int>(UpdateSpeed.Fast);
        private readonly CacheField<long> _coinage = new CacheField<long>(UpdateSpeed.Fast);
        private readonly CacheField<int> _currentLevelAreaId = new CacheField<int>(UpdateSpeed.Fast);
        private readonly CacheField<ActInfo> _actInfo = new CacheField<ActInfo>();
        private readonly CacheField<BountyInfo> _activeBounty = new CacheField<BountyInfo>(UpdateSpeed.Normal);
        private readonly CacheField<List<BountyInfo>> _bounties = new CacheField<List<BountyInfo>>(UpdateSpeed.Normal);
        private readonly CacheField<List<QuestInfo>> _activeQuests = new CacheField<List<QuestInfo>>(UpdateSpeed.Normal);
        private readonly CacheField<bool> _isLoadingWorld = new CacheField<bool>(UpdateSpeed.Fast);
        private readonly CacheField<bool> _isInTown = new CacheField<bool>(UpdateSpeed.Fast);
        private readonly CacheField<bool> _isInGame = new CacheField<bool>(UpdateSpeed.Fast);
        private readonly CacheField<bool> _isMe = new CacheField<bool>();
        private readonly CacheField<int> _riftRewardKey = new CacheField<int>(UpdateSpeed.Normal);
        private readonly CacheField<int> _riftDeathCount = new CacheField<int>(UpdateSpeed.Normal);
        private readonly CacheField<double> _riftResurrectionTime = new CacheField<double>(UpdateSpeed.Fast);
        private readonly CacheField<int> _stashTabsPurchased = new CacheField<int>();
        private readonly CacheField<double> _lastClickZAxis = new CacheField<double>(UpdateSpeed.Fast);
        private readonly CacheField<bool> _isInHotspot = new CacheField<bool>(UpdateSpeed.Normal);
        private readonly CacheField<Act> _worldType = new CacheField<Act>(UpdateSpeed.Normal);
        private readonly CacheField<int> _currentQuestSNO = new CacheField<int>(UpdateSpeed.Normal);
        private readonly CacheField<int> _currentQuestStep = new CacheField<int>(UpdateSpeed.Normal);

        #endregion

        #region Properties

        /// <summary>
        /// Number of times that you can ressurect at your current death location
        /// </summary>
        public float CorpseResurrectionCharges
        {
            get { return _corpseResurrectionCharges.IsCacheValid ? _corpseResurrectionCharges.CachedValue : (_corpseResurrectionCharges.CachedValue = ZetaDia.Me.CorpseResurrectionCharges); }
            set { _corpseResurrectionCharges.SetValueOverride(value); }
        }

        /// <summary>
        /// Current amount of primary resource (mana, hatred etc)
        /// </summary>
        public float CurrentPrimaryResource
        {
            get { return _currentPrimaryResource.IsCacheValid ? _currentPrimaryResource.CachedValue : (_currentPrimaryResource.CachedValue = ZetaDia.Me.CurrentPrimaryResource); }
            set { _currentPrimaryResource.SetValueOverride(value); }
        }

        /// <summary>
        /// Current amount of secondary resource (DH Discipline)
        /// </summary>
        public float CurrentSecondaryResource
        {
            get { return _currentSecondaryResource.IsCacheValid ? _currentSecondaryResource.CachedValue : (_currentSecondaryResource.CachedValue = ZetaDia.Me.CurrentSecondaryResource); }
            set { _currentSecondaryResource.SetValueOverride(value); }
        }

        /// <summary>
        /// Secondary resource as a percentage of maximum (0-100)
        /// </summary>
        public float SecondaryResourcePct
        {
            get { return _secondaryResourcePct.IsCacheValid ? _secondaryResourcePct.CachedValue : (_secondaryResourcePct.CachedValue = MaxSecondaryResource > 0 ? CurrentSecondaryResource / MaxSecondaryResource : 0); }
            set { _secondaryResourcePct.SetValueOverride(value); }
        }

        /// <summary>
        /// Primary resource as a percentage of maximum (0-100)
        /// </summary>
        public float PrimaryResourcePct
        {
            get { return _primaryResourcePct.IsCacheValid ? _primaryResourcePct.CachedValue : (_primaryResourcePct.CachedValue = MaxPrimaryResource > 0 ? CurrentPrimaryResource / MaxPrimaryResource : 0); }
            set { _primaryResourcePct.SetValueOverride(value); }
        }

        /// <summary>
        /// Amount of Primary resource when full
        /// </summary>
        public float MaxPrimaryResource
        {
            get { return _maxPrimaryResource.IsCacheValid ? _maxPrimaryResource.CachedValue : (_maxPrimaryResource.CachedValue = ZetaDia.Me.MaxPrimaryResource); }
            set { _maxPrimaryResource.SetValueOverride(value); }
        }

        /// <summary>
        /// Amount of Secondary resource when full
        /// </summary>
        public float MaxSecondaryResource
        {
            get { return _maxSecondaryResource.IsCacheValid ? _maxSecondaryResource.CachedValue : (_maxSecondaryResource.CachedValue = ZetaDia.Me.MaxPrimaryResource); }
            set { _maxSecondaryResource.SetValueOverride(value); }
        }

        /// <summary>
        /// The amount of times you have died
        /// </summary>
        public int DeathCount
        {
            get { return _deathCount.IsCacheValid ? _deathCount.CachedValue : (_deathCount.CachedValue = ZetaDia.Me.DeathCount); }
            set { _deathCount.SetValueOverride(value); }
        }

        /// <summary>
        /// Current level
        /// </summary>
        public int Level
        {
            get { return _level.IsCacheValid ? _level.CachedValue : (_level.CachedValue = ZetaDia.Me.Level); }
            set { _level.SetValueOverride(value); }
        }

        /// <summary>
        /// Current greater rift level
        /// </summary>
        public int TieredLootRunLevel
        {
            get { return _tieredLootRunLevel.IsCacheValid ? _tieredLootRunLevel.CachedValue : (_tieredLootRunLevel.CachedValue = ZetaDia.Me.Level); }
            set { _tieredLootRunLevel.SetValueOverride(value); }
        }

        /// <summary>
        /// If currently in greater rift
        /// </summary>
        public bool InTieredLootRun
        {
            get { return _inTieredLootRunLevel.IsCacheValid ? _inTieredLootRunLevel.CachedValue : (_inTieredLootRunLevel.CachedValue = TieredLootRunLevel > 0); }
            set { _inTieredLootRunLevel.SetValueOverride(value); }
        }

        /// <summary>
        /// Maximum level that player is allowed to reach
        /// </summary>
        public int LevelCap
        {
            get { return _levelCap.IsCacheValid ? _levelCap.CachedValue : (_levelCap.CachedValue = ZetaDia.Me.LevelCap); }
            set { _levelCap.SetValueOverride(value); }
        }

        /// <summary>
        /// Number of stash slots unlocked
        /// </summary>
        public int SharedStashSlots
        {
            get { return _sharedStashSlots.IsCacheValid ? _sharedStashSlots.CachedValue : (_sharedStashSlots.CachedValue = ZetaDia.Me.NumSharedStashSlots); }
            set { _sharedStashSlots.SetValueOverride(value); }
        }

        /// <summary>
        /// Class of this player
        /// </summary>
        public ActorClass ActorClass
        {
            get { return _actorClass.IsCacheValid ? _actorClass.CachedValue : (_actorClass.CachedValue = ZetaDia.Me.ActorClass); }
            set { _actorClass.SetValueOverride(value); }
        }

        /// <summary>
        /// Number of stash slots unlocked
        /// </summary>
        public int Armor
        {
            get { return _armor.IsCacheValid ? _armor.CachedValue : (_armor.CachedValue = (int)ZetaDia.Me.Armor); }
            set { _armor.SetValueOverride(value); }
        }

        /// <summary>
        /// Number of attacks per second
        /// </summary>
        public double AttacksPerSecond
        {
            get { return _attacksPerSecond.IsCacheValid ? _attacksPerSecond.CachedValue : (_attacksPerSecond.CachedValue = ZetaDia.Me.AttacksPerSecond); }
            set { _attacksPerSecond.SetValueOverride(value); }
        }

        /// <summary>
        /// Multiplier on critical attacks.
        /// </summary>
        public double CritDamagePct
        {
            get { return _criticalDamagePercent.IsCacheValid ? _criticalDamagePercent.CachedValue : (_criticalDamagePercent.CachedValue = ZetaDia.Me.CritDamagePercent); }
            set { _criticalDamagePercent.SetValueOverride(value); }
        }

        /// <summary>
        /// Damage reduction percent (from items and skills)
        /// </summary>
        public double DamageReductionPct
        {
            get { return _damageReduction.IsCacheValid ? _damageReduction.CachedValue : (_damageReduction.CachedValue = ZetaDia.Me.DamageReduction); }
            set { _damageReduction.SetValueOverride(value); }
        }

        /// <summary>
        /// Amount of Dexterity stat
        /// </summary>
        public double Dexterity
        {
            get { return _dexterity.IsCacheValid ? _dexterity.CachedValue : (_dexterity.CachedValue = ZetaDia.Me.Dexterity); }
            set { _dexterity.SetValueOverride(value); }
        }

        /// <summary>
        /// Amount of Strength stat
        /// </summary>
        public double Strength
        {
            get { return _strength.IsCacheValid ? _strength.CachedValue : (_strength.CachedValue = ZetaDia.Me.Strength); }
            set { _strength.SetValueOverride(value); }
        }

        /// <summary>
        /// Amount of Dexterity stat
        /// </summary>
        public double Intelligence
        {
            get { return _intelligence.IsCacheValid ? _intelligence.CachedValue : (_intelligence.CachedValue = ZetaDia.Me.Intelligence); }
            set { _intelligence.SetValueOverride(value); }
        }

        /// <summary>
        /// Amount of Vitality stat
        /// </summary>
        public double Vitality
        {
            get { return _vitality.IsCacheValid ? _vitality.CachedValue : (_vitality.CachedValue = ZetaDia.Me.Vitality); }
            set { _vitality.SetValueOverride(value); }
        }

        /// <summary>
        /// If currently afk
        /// </summary>
        public bool IsAfk
        {
            get { return _isAfk.IsCacheValid ? _isAfk.CachedValue : (_isAfk.CachedValue = ZetaDia.Me.IsAfk); }
            set { _isAfk.SetValueOverride(value); }
        }

        /// <summary>
        /// Is currently in busy state
        /// </summary>
        public bool IsBusy
        {
            get { return _isAfk.IsCacheValid ? _isBusy.CachedValue : (_isBusy.CachedValue = ZetaDia.Me.IsBusy); }
            set { _isBusy.SetValueOverride(value); }
        }

        /// <summary>
        /// Whether player is in combat (calls CombatBase.IsInCombat)
        /// </summary>
        public bool IsInCombat
        {
            get { return _isInCombat.IsCacheValid ? _isInCombat.CachedValue : (_isInCombat.CachedValue = CombatBase.IsInCombat); }
            set { _isInCombat.SetValueOverride(value); }
        }

        /// <summary>
        /// If player is a melee class
        /// </summary>
        public bool IsMelee
        {
            get
            {
                if (_isMelee.IsCacheValid) return _isMelee.CachedValue;
                return _isMelee.CachedValue = ActorClass == ActorClass.Barbarian || ActorClass == ActorClass.Monk || ActorClass == ActorClass.Crusader;
            }
            set { _isMelee.SetValueOverride(value); }
        }

        /// <summary>
        /// If talking to another actor
        /// </summary>
        public bool IsInConversation
        {
            get { return _isInConversation.IsCacheValid ? _isInConversation.CachedValue : (_isInConversation.CachedValue = CombatBase.IsInCombat); }
            set { _isInConversation.SetValueOverride(value); }
        }

        /// <summary>
        /// Indicates whether special skills are in use - Wizard Archon etc.
        /// </summary>
        public bool IsSkillOverrideActive
        {
            get { return _isSkillOverrideActive.IsCacheValid ? _isSkillOverrideActive.CachedValue : (_isSkillOverrideActive.CachedValue = ZetaDia.Me.SkillOverrideActive); }
            set { _isSkillOverrideActive.SetValueOverride(value); }
        }

        /// <summary>
        /// Distance from player that auto-pickup items (globes, gold etc) can be collected
        /// </summary>
        public double GoldPickUpRadius
        {
            get { return _goldPickupRadius.IsCacheValid ? _goldPickupRadius.CachedValue : (_goldPickupRadius.CachedValue = ZetaDia.Me.GoldPickUpRadius); }
            set { _goldPickupRadius.SetValueOverride(value); }
        }

        /// <summary>
        /// Amount of damage reduction for melee attacks from skills and items.
        /// </summary>
        public double MeleeDamageeduction
        {
            get { return _meleeDamageeduction.IsCacheValid ? _meleeDamageeduction.CachedValue : (_meleeDamageeduction.CachedValue = ZetaDia.Me.MeleeDamageeduction); }
            set { _meleeDamageeduction.SetValueOverride(value); }
        }

        /// <summary>
        /// Current amount of experience
        /// </summary>
        public int CurrentExperience
        {
            get { return _currentExperience.IsCacheValid ? _currentExperience.CachedValue : (_currentExperience.CachedValue = ZetaDia.Me.CurrentExperience); }
            set { _currentExperience.SetValueOverride(value); }
        }


        /// <summary>
        ///Maximum blood shards
        /// </summary>
        public int MaxBloodShards
        {
            get { return _maxBloodShards.IsCacheValid ? _maxBloodShards.CachedValue : (_maxBloodShards.CachedValue = 500 + ZetaDia.Me.CommonData.GetAttributeOrDefault<int>(ActorAttributeType.HighestSoloRiftLevel) * 10); }
            set { _maxBloodShards.SetValueOverride(value); }
        }

        /// <summary>
        /// Jewel upgrades left to use
        /// </summary>
        public int JewelUpgradesLeft
        {
            get { return _jewelUpgradesLeft.IsCacheValid ? _jewelUpgradesLeft.CachedValue : (_jewelUpgradesLeft.CachedValue = ZetaDia.Me.JewelUpgradesLeft); }
            set { _jewelUpgradesLeft.SetValueOverride(value); }
        }

        /// <summary>
        /// Jewel upgrades max
        /// </summary>
        public int JewelUpgradesMax
        {
            get { return _jewelUpgradesMax.IsCacheValid ? _jewelUpgradesMax.CachedValue : (_jewelUpgradesMax.CachedValue = ZetaDia.Me.JewelUpgradesMax); }
            set { _jewelUpgradesMax.SetValueOverride(value); }
        }

        /// <summary>
        /// Jewel upgrades already used
        /// </summary>
        public int JewelUpgradesUsed
        {
            get { return _jewelUpgradesUsed.IsCacheValid ? _jewelUpgradesUsed.CachedValue : (_jewelUpgradesUsed.CachedValue = ZetaDia.Me.JewelUpgradesUsed); }
            set { _jewelUpgradesUsed.SetValueOverride(value); }
        }

        /// <summary>
        /// End time of looping animation
        /// </summary>
        public double LoopingAnimationEndTime
        {
            get { return _loopingAnimationEndTime.IsCacheValid ? _loopingAnimationEndTime.CachedValue : (_loopingAnimationEndTime.CachedValue = ZetaDia.Me.LoopingAnimationEndTime); }
            set { _loopingAnimationEndTime.SetValueOverride(value); }
        }

        /// <summary>
        /// Start time of looping animation
        /// </summary>
        public double LoopingAnimationStartTime
        {
            get { return _loopingAnimationStartTime.IsCacheValid ? _loopingAnimationStartTime.CachedValue : (_loopingAnimationStartTime.CachedValue = ZetaDia.Me.LoopingAnimationStartTime); }
            set { _loopingAnimationStartTime.SetValueOverride(value); }
        }

        /// <summary>
        /// Total number of backpack slots
        /// </summary>
        public int BackpackSlots
        {
            get { return _backpackSlots.IsCacheValid ? _backpackSlots.CachedValue : (_backpackSlots.CachedValue = ZetaDia.Me.NumBackpackSlots); }
            set { _backpackSlots.SetValueOverride(value); }
        }

        /// <summary>
        /// Number of free/available backpack slots
        /// </summary>
        public int FreeBackpackSlots
        {
            get { return _freeBackpackSlots.IsCacheValid ? _freeBackpackSlots.CachedValue : (_freeBackpackSlots.CachedValue = Inventory.NumFreeBackpackSlots); }
            set { _freeBackpackSlots.SetValueOverride(value); }
        }

        /// <summary>
        /// Total number of stash slots
        /// </summary>
        public int StashSlots
        {
            get { return _stashSlots.IsCacheValid ? _stashSlots.CachedValue : (_stashSlots.CachedValue = ZetaDia.Me.NumBackpackSlots); }
            set { _stashSlots.SetValueOverride(value); }
        }

        /// <summary>
        /// Number of free/available stash slots
        /// </summary>
        public int FreeStashSlots
        {
            get { return _freeStashSlots.IsCacheValid ? _freeStashSlots.CachedValue : (_freeStashSlots.CachedValue = Inventory.NumFreeSharedStashSlots); }
            set { _freeStashSlots.SetValueOverride(value); }
        }

        /// <summary>
        /// DBs Inventory Manager
        /// </summary>
        public DiaActivePlayer.InventoryManager Inventory
        {
            get { return _inventory.IsCacheValid ? _inventory.CachedValue : (_inventory.CachedValue = ZetaDia.Me.Inventory); }
            set { _inventory.SetValueOverride(value); }
        }

        /// <summary>
        /// Current paragon experience
        /// </summary>
        public int ParagonCurrentExperience
        {
            get { return _paragonCurrentExperience.IsCacheValid ? _paragonCurrentExperience.CachedValue : (_paragonCurrentExperience.CachedValue = ZetaDia.Me.ParagonCurrentExperience); }
            set { _paragonCurrentExperience.SetValueOverride(value); }
        }

        /// <summary>
        /// Current paragon level
        /// </summary>
        public int ParagonLevel
        {
            get { return _paragonLevel.IsCacheValid ? _paragonLevel.CachedValue : (_paragonLevel.CachedValue = ZetaDia.Me.ParagonLevel); }
            set { _paragonLevel.SetValueOverride(value); }
        }

        /// <summary>
        /// About of rested experience
        /// </summary>
        public long RestExperience
        {
            get { return _restExperience.IsCacheValid ? _restExperience.CachedValue : (_restExperience.CachedValue = ZetaDia.Me.RestExperience); }
            set { _restExperience.SetValueOverride(value); }
        }

        /// <summary>
        /// About of rested experience
        /// </summary>
        public bool CanUseTownPortal
        {
            get { return _canUseTownPortal.IsCacheValid ? _canUseTownPortal.CachedValue : (_canUseTownPortal.CachedValue = ZetaDia.Me.CanUseTownPortal()); }
            set { _canUseTownPortal.SetValueOverride(value); }
        }

        /// <summary>
        /// Amount of secondary resource that has been used
        /// </summary>
        public float SecondaryResourceMissing
        {
            get { return _secondaryResourceMissing.IsCacheValid ? _secondaryResourceMissing.CachedValue : (_secondaryResourceMissing.CachedValue = MaxSecondaryResource - CurrentSecondaryResource); }
            set { _secondaryResourceMissing.SetValueOverride(value); }
        }

        /// <summary>
        /// Amount of secondary resource that has been used
        /// </summary>
        public float PrimaryResourceMissing
        {
            get { return _primaryResourceMissing.IsCacheValid ? _primaryResourceMissing.CachedValue : (_primaryResourceMissing.CachedValue = MaxPrimaryResource - CurrentPrimaryResource); }
            set { _primaryResourceMissing.SetValueOverride(value); }
        }

        /// <summary>
        /// Whether completely unable to do anything
        /// </summary>
        public bool IsIncapacitated
        {
            get { return _isIncapacitated.IsCacheValid ? _isIncapacitated.CachedValue : (_isIncapacitated.CachedValue = IsFeared || IsFrozen || IsStunned); }
            set { _isIncapacitated.SetValueOverride(value); }
        }

        /// <summary>
        /// Whether completely unable to do anything
        /// </summary>
        public bool IsInRift
        {
            get { return _isInRift.IsCacheValid ? _isInRift.CachedValue : (_isInRift.CachedValue = DataDictionary.RiftWorldIds.Contains(WorldId)); }
            set { _isInRift.SetValueOverride(value); }
        }

        /// <summary>
        /// Unique id of the current world
        /// </summary>
        public int WorldId
        {
            get { return _worldId.IsCacheValid ? _worldId.CachedValue : (_worldId.CachedValue = ZetaDia.CurrentWorldId); }
            set { _worldId.SetValueOverride(value); }
        }

        /// <summary>
        /// Current amount of bloodshards
        /// </summary>
        public int Bloodshards
        {
            get { return _blopdshards.IsCacheValid ? _blopdshards.CachedValue : (_blopdshards.CachedValue = ZetaDia.CPlayer.BloodshardCount); }
            set { _blopdshards.SetValueOverride(value); }
        }

        /// <summary>
        /// Current amount of Gold
        /// </summary>
        public long Coinage
        {
            get { return _coinage.IsCacheValid ? _coinage.CachedValue : (_coinage.CachedValue = ZetaDia.CPlayer.Coinage); }
            set { _coinage.SetValueOverride(value); }
        }

        /// <summary>
        /// Unique Id for the current level area
        /// </summary>
        public int LevelAreaId
        {
            get { return _currentLevelAreaId.IsCacheValid ? _currentLevelAreaId.CachedValue : (_currentLevelAreaId.CachedValue = ZetaDia.CurrentLevelAreaId); }
            set { _currentLevelAreaId.SetValueOverride(value); }
        }

        /// <summary>
        /// Source for information on quests and bounties
        /// </summary>
        public ActInfo ActInfo
        {
            get { return _actInfo.IsCacheValid ? _actInfo.CachedValue : (_actInfo.CachedValue = ZetaDia.ActInfo); }
            set { _actInfo.SetValueOverride(value); }
        }

        /// <summary>
        /// The current bounty quest
        /// </summary>
        public BountyInfo ActiveBounty
        {
            get { return _activeBounty.IsCacheValid ? _activeBounty.CachedValue : (_activeBounty.CachedValue = ActInfo.ActiveBounty); }
            set { _activeBounty.SetValueOverride(value); }
        }

        /// <summary>
        /// The current bounty quest
        /// </summary>
        public List<BountyInfo> Bounties
        {
            get { return _bounties.IsCacheValid ? _bounties.CachedValue : (_bounties.CachedValue = ActInfo.Bounties.ToList()); }
            set { _bounties.SetValueOverride(value); }
        }

        /// <summary>
        /// The current bounty quest
        /// </summary>
        public List<QuestInfo> ActiveQuests
        {
            get { return _activeQuests.IsCacheValid ? _activeQuests.CachedValue : (_activeQuests.CachedValue = ActInfo.ActiveQuests.ToList()); }
            set { _activeQuests.SetValueOverride(value); }
        }

        /// <summary>
        /// If transitioning between worlds
        /// </summary>
        public bool IsLoadingWorld
        {
            get { return _isLoadingWorld.IsCacheValid ? _isLoadingWorld.CachedValue : (_isLoadingWorld.CachedValue = ZetaDia.IsLoadingWorld); }
            set { _isLoadingWorld.SetValueOverride(value); }
        }

        /// <summary>
        /// If in a town area
        /// </summary>
        public bool IsInTown
        {
            get { return _isInTown.IsCacheValid ? _isInTown.CachedValue : (_isInTown.CachedValue = ZetaDia.IsInTown); }
            set { _isInTown.SetValueOverride(value); }
        }

        /// <summary>
        /// If a game is loaded (as opposed to the game lobby or character selection screen)
        /// </summary>
        public bool IsInGame
        {
            get { return _isInGame.IsCacheValid ? _isInGame.CachedValue : (_isInGame.CachedValue = ZetaDia.IsInGame); }
            set { _isInGame.SetValueOverride(value); }
        }

        /// <summary>
        /// If this TrinityPlayer object is for the current player
        /// </summary>
        public bool IsMe
        {
            get { return _isMe.IsCacheValid ? _isMe.CachedValue : (_isMe.CachedValue = ACDGuid == ZetaDia.ActivePlayerACDGuid); }
            set { _isMe.SetValueOverride(value); }
        }

        /// <summary>
        /// Number of times you've died so far in the current rift (effects respawn delay)
        /// </summary>
        public int RiftDeathCount
        {
            get { return _riftDeathCount.IsCacheValid ? _riftDeathCount.CachedValue : (_riftDeathCount.CachedValue = Source.GetAttributeOrDefault<int>(ActorAttributeType.TieredLootRunDeathCount)); }
            set { _riftDeathCount.SetValueOverride(value); }
        }

        /// <summary>
        /// Level of the key awarded for finishing the current rift
        /// </summary>
        public int RiftRewardKey
        {
            get { return _riftRewardKey.IsCacheValid ? _riftRewardKey.CachedValue : (_riftRewardKey.CachedValue = Source.GetAttributeOrDefault<int>(ActorAttributeType.TieredLootRunRewardKeyValue)); }
            set { _riftRewardKey.SetValueOverride(value); }
        }

        /// <summary>
        /// Time until allowed to ressurect (current rift)
        /// </summary>
        public double RiftResurrectionTime
        {
            get { return _riftResurrectionTime.IsCacheValid ? _riftResurrectionTime.CachedValue : (_riftResurrectionTime.CachedValue = Source.GetAttributeOrDefault<int>(ActorAttributeType.TieredLootRunCorpseResurrectionAllowedGameTime)); }
            set { _riftResurrectionTime.SetValueOverride(value); }
        }

        /// <summary>
        /// The number of stash tabs that have been purchased
        /// </summary>
        public int StashTabsPurchased
        {
            get { return _stashTabsPurchased.IsCacheValid ? _stashTabsPurchased.CachedValue : (_stashTabsPurchased.CachedValue = Source.GetAttributeOrDefault<int>(ActorAttributeType.StashTabsPurchasedWithGold)); }
            set { _stashTabsPurchased.SetValueOverride(value); }
        }

        /// <summary>
        /// The last click's Z-Axis value
        /// </summary>
        public double LastClickZAxis
        {
            get { return _lastClickZAxis.IsCacheValid ? _lastClickZAxis.CachedValue : (_lastClickZAxis.CachedValue = ZetaDia.Me.Movement.LastClickZAxis); }
            set { _lastClickZAxis.SetValueOverride(value); }
        }

        /// <summary>
        /// If player is currently standing in a hotspot
        /// </summary>
        public bool IsInHotspot
        {
            get
            {
                if (_isInHotspot.IsCacheValid)
                    return _isInHotspot.CachedValue;

                return _isInHotspot.CachedValue = GroupHotSpots.CacheObjectIsInHotSpot(Position);
            }
        }

        /// <summary>
        /// World type
        /// </summary>
        public Act WorldType
        {
            get { return _worldType.IsCacheValid ? _worldType.CachedValue : (_worldType.CachedValue = ZetaDia.WorldType); }
            set { _worldType.SetValueOverride(value); }
        }

        /// <summary>
        /// Current quest sno
        /// </summary>
        public int CurrentQuestSNO
        {
            get { return _currentQuestSNO.IsCacheValid ? _currentQuestSNO.CachedValue : (_currentQuestSNO.CachedValue = ZetaDia.CurrentQuest.QuestSNO); }
            set { _currentQuestSNO.SetValueOverride(value); }
        }

        /// <summary>
        /// Current quest step
        /// </summary>
        public int CurrentQuestStep
        {
            get { return _currentQuestStep.IsCacheValid ? _currentQuestStep.CachedValue : (_currentQuestStep.CachedValue = ZetaDia.CurrentQuest.StepId); }
            set { _currentQuestStep.SetValueOverride(value); }
        }

        //public List<Buff> AllDebuffs
        //{
        //    get { return CacheManager.GetCacheValue(this, o => ZetaDia.Me.GetAllDebuffs().ToList(), 250); }
        //}

        //public List<Buff> AllBuffs
        //{
        //    get { return CacheManager.GetCacheValue(this, o => ZetaDia.Me.GetAllBuffs().ToList(), 250); }
        //}

        ////public List<ActiveSkillEntry> KnownSkillEntrys
        ////{
        ////    get { return CacheManager.GetCacheValue(this, o => ZetaDia.Me.KnownSkills, 5000); }
        ////}

        ////public List<Skill> Skills
        ////{
        ////    get { return CacheManager.GetCacheValue(this, o => ZetaDia.Me.KnownSkills.Select(s => (Skill)s).ToList(), 5000); }
        ////}

        ////public List<TraitEntry> KnownTraitEntrys
        ////{
        ////    get { return CacheManager.GetCacheValue(this, o => ZetaDia.Me.KnownTraits, 5000); }
        ////}

        ////public List<Passive> Passives
        ////{
        ////    get { return CacheManager.GetCacheValue(this, o => ZetaDia.Me.KnownTraits.Select(s => (Passive)s).ToList(), 5000); }
        ////} 

        #endregion

        #region Methods

        public int CountSummonsOfType(TrinityPetType type)
        {
            return CacheManager.Pets.Count(p => p.PetType == type);
        }

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

        public new void HasDebuff(SNOPower power)
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