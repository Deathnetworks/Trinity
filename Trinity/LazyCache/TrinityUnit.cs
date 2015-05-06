using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Trinity.DbProvider;
using Trinity.Technicals;
using Zeta.Bot.Navigation;
using Zeta.Common;
using Zeta.Game;
using Zeta.Game.Internals.Actors;
using Zeta.Game.Internals.SNO;
using Logger = Trinity.Technicals.Logger;

namespace Trinity.LazyCache
{
    /// <summary>
    /// Trinity base object
    /// </summary>
    public class TrinityUnit : TrinityObject
    {
        public TrinityUnit(ACD acd) : base(acd) { }

        #region Fields

        private readonly CacheField<float> _hitpointsCurrent = new CacheField<float>(UpdateSpeed.Fast);
        private readonly CacheField<float> _hitpointsCurrentPct = new CacheField<float>(UpdateSpeed.Fast);
        private readonly CacheField<float> _hitpointsMax = new CacheField<float>(UpdateSpeed.Fast);
        private readonly CacheField<bool> _isPetOwner = new CacheField<bool>(UpdateSpeed.Normal);
        private readonly CacheField<SummonType> _petType = new CacheField<SummonType>();
        private readonly CacheField<int> _petCreatorACDId = new CacheField<int>();
        private readonly CacheField<int> _appearanceSNO = new CacheField<int>();
        private readonly CacheField<int> _physMeshSNO = new CacheField<int>();
        private readonly CacheField<int> _physicsSNO = new CacheField<int>();
        private readonly CacheField<int> _summonedByACDId = new CacheField<int>();
        private readonly CacheField<bool> _isSummonedByPlayer = new CacheField<bool>();
        private readonly CacheField<MonsterQuality> _monsterQuality = new CacheField<MonsterQuality>();
        private readonly CacheField<bool> _isElite = new CacheField<bool>();
        private readonly CacheField<bool> _isRare = new CacheField<bool>();
        private readonly CacheField<bool> _isUnique = new CacheField<bool>();
        private readonly CacheField<bool> _isBoss = new CacheField<bool>();
        private readonly CacheField<bool> _isMinion = new CacheField<bool>();
        private readonly CacheField<bool> _isFacingPlayer = new CacheField<bool>();
        private readonly CacheField<MonsterAffixes> _monsterAffixes = new CacheField<MonsterAffixes>();
        private readonly CacheField<List<MonsterAffixEntry>> _monsterAffixEntries = new CacheField<List<MonsterAffixEntry>>();
        private readonly CacheField<List<MonsterAffix>> _affixes = new CacheField<List<MonsterAffix>>();
        private readonly CacheField<bool> _isBleeding = new CacheField<bool>(UpdateSpeed.Normal);
        private readonly CacheField<bool> _isBurrowed = new CacheField<bool>(UpdateSpeed.Normal);
        private readonly CacheField<bool> _isEnraged = new CacheField<bool>();
        private readonly CacheField<bool> _isQuestMonster = new CacheField<bool>(UpdateSpeed.Normal);
        private readonly CacheField<bool> _isStealthed = new CacheField<bool>(UpdateSpeed.Normal);
        private readonly CacheField<bool> _isUninterruptible = new CacheField<bool>();
        private readonly CacheField<MonsterRace> _monsterRace = new CacheField<MonsterRace>();
        private readonly CacheField<MonsterSize> _monsterSize = new CacheField<MonsterSize>();
        private readonly CacheField<bool> _isWebbed = new CacheField<bool>();
        private readonly CacheField<bool> _isCharging = new CacheField<bool>(UpdateSpeed.Ultra);
        private readonly CacheField<bool> _hasDotDps = new CacheField<bool>(UpdateSpeed.Fast);
        private readonly CacheField<bool> _hasShieldingAffix = new CacheField<bool>();
        private readonly CacheField<bool> _isTreasureGoblin = new CacheField<bool>();
        private readonly CacheField<bool> _isEliteRareUnique = new CacheField<bool>();
        private readonly CacheField<bool> _isBossOrEliteRareUnique = new CacheField<bool>();
        private readonly CacheField<bool> _isTrash = new CacheField<bool>();
        private readonly CacheField<int> _summonedBySNO = new CacheField<int>();
        private readonly CacheField<int> _summonerId = new CacheField<int>();
        private readonly CacheField<int> _summonerACDGuid = new CacheField<int>();
        private readonly CacheField<float> _movementScalar = new CacheField<float>(UpdateSpeed.Fast);
        private readonly CacheField<float> _movementScalarCap = new CacheField<float>();
        private readonly CacheField<float> _movementScalarCappedTotal = new CacheField<float>();
        private readonly CacheField<float> _movementScalarSubtotal = new CacheField<float>();
        private readonly CacheField<float> _movementScalarTotal = new CacheField<float>(UpdateSpeed.Fast);
        private readonly CacheField<int> _rootTargetACD = new CacheField<int>();
        private readonly CacheField<float> _runningRate = new CacheField<float>(UpdateSpeed.Fast);
        private readonly CacheField<float> _runningRateTotal = new CacheField<float>(UpdateSpeed.Fast);
        private readonly CacheField<bool> _isAlive = new CacheField<bool>(UpdateSpeed.Ultra);
        private readonly CacheField<bool> _isDead = new CacheField<bool>(UpdateSpeed.Ultra);
        private readonly CacheField<bool> _isAttackable = new CacheField<bool>(UpdateSpeed.Slow);
        private readonly CacheField<bool> _isFriendly = new CacheField<bool>(UpdateSpeed.Slow);
        private readonly CacheField<bool> _isHelper = new CacheField<bool>();
        private readonly CacheField<bool> _isHidden = new CacheField<bool>(UpdateSpeed.Fast);
        private readonly CacheField<bool> _isRooted = new CacheField<bool>(UpdateSpeed.Fast);
        private readonly CacheField<bool> _isBlind = new CacheField<bool>(UpdateSpeed.Fast);
        private readonly CacheField<bool> _isFeared = new CacheField<bool>(UpdateSpeed.Fast);
        private readonly CacheField<bool> _isChilled = new CacheField<bool>(UpdateSpeed.Fast);
        private readonly CacheField<bool> _isFrozen = new CacheField<bool>(UpdateSpeed.Fast);
        private readonly CacheField<bool> _isStunned = new CacheField<bool>(UpdateSpeed.Fast);
        private readonly CacheField<bool> _isSlowed = new CacheField<bool>(UpdateSpeed.Fast);
        private readonly CacheField<bool> _isHostile = new CacheField<bool>(UpdateSpeed.Slow);
        private readonly CacheField<bool> _isNPC = new CacheField<bool>();
        private readonly CacheField<bool> _isNPCOperable = new CacheField<bool>(UpdateSpeed.Slow);
        private readonly CacheField<bool> _isQuestGiver = new CacheField<bool>(UpdateSpeed.Slow);
        private readonly CacheField<bool> _isSalvageShortcut = new CacheField<bool>();
        private readonly CacheField<bool> _isTownVendor = new CacheField<bool>();
        private readonly CacheField<HirelingType> _hirelingType = new CacheField<HirelingType>();
        private readonly CacheField<float> _hitpointsRegenPerSecond = new CacheField<float>();
        private readonly CacheField<float> _hitpointsRegenPerSecondTotal = new CacheField<float>();
        private readonly CacheField<bool> _isStandingInAvoidance = new CacheField<bool>(UpdateSpeed.Ultra);
        private readonly CacheField<int> _unitsBehind = new CacheField<int>(UpdateSpeed.Fast);
        private readonly CacheField<int> _unitsInFront = new CacheField<int>(UpdateSpeed.Fast);
        private readonly CacheField<int> _unitsNearby = new CacheField<int>(UpdateSpeed.Fast);
        private readonly CacheField<ActorMovement> _movement = new CacheField<ActorMovement>(UpdateSpeed.Fast);
        private readonly CacheField<Vector2> _directionVector = new CacheField<Vector2>(UpdateSpeed.Fast);
        private readonly CacheField<float> _movementSpeed = new CacheField<float>(UpdateSpeed.Fast);
        private readonly CacheField<bool> _isMoving = new CacheField<bool>(UpdateSpeed.Ultra);
        private readonly CacheField<float> _rotation = new CacheField<float>(UpdateSpeed.Fast);
        private readonly CacheField<float> _rotationDegrees = new CacheField<float>(UpdateSpeed.Fast);
        private readonly CacheField<float> _currentHealthPct = new CacheField<float>(UpdateSpeed.Fast);
        private readonly CacheField<double> _killRange = new CacheField<double>(UpdateSpeed.Fast);
        private readonly CacheField<bool> _isSummoner = new CacheField<bool>(UpdateSpeed.Slow);
        private readonly CacheField<bool> _isChargeTarget = new CacheField<bool>();
        

        #endregion

        #region Properties

        /// <summary>
        /// Current Hitpoints
        /// </summary>
        public float HitpointsCurrent
        {
            get { return _hitpointsCurrent.IsCacheValid ? _hitpointsCurrent.CachedValue : (_hitpointsCurrent.CachedValue = GetUnitProperty(x => x.HitpointsCurrent)); }
            set { _hitpointsCurrent.SetValueOverride(value); }
        }

        /// <summary>
        /// Current Hitpoint Percentage (0-100)
        /// </summary>
        public float HitpointsCurrentPct
        {
            get { return _hitpointsCurrentPct.IsCacheValid ? _hitpointsCurrentPct.CachedValue : (_hitpointsCurrentPct.CachedValue = GetUnitProperty(x => x.HitpointsCurrentPct)); }
            set { _hitpointsCurrentPct.SetValueOverride(value); }
        }

        /// <summary>
        /// Maximum possible hitpoints
        /// </summary>
        public float HitpointsMax
        {
            get { return _hitpointsMax.IsCacheValid ? _hitpointsMax.CachedValue : (_hitpointsMax.CachedValue = GetUnitProperty(x => x.HitpointsMax)); }
            set { _hitpointsMax.SetValueOverride(value); }
        }

        /// <summary>
        /// If unit is a pet owner
        /// </summary>
        public bool IsPetOwner
        {
            get { return _isPetOwner.IsCacheValid ? _isPetOwner.CachedValue : (_isPetOwner.CachedValue = GetUnitProperty(x => x.PetOwner) > 0); }
            set { _isPetOwner.SetValueOverride(value); }
        }

        /// <summary>
        /// Type of pets owned
        /// </summary>
        public SummonType PetType
        {
            get { return _petType.IsCacheValid ? _petType.CachedValue : (_petType.CachedValue = GetSummonType(this)); }
            set { _petType.SetValueOverride(value); }
        }

        /// <summary>
        /// ACDId of unit who created this unit
        /// </summary>
        public int PetCreatorACDId
        {
            get { return _petCreatorACDId.IsCacheValid ? _petCreatorACDId.CachedValue : (_petCreatorACDId.CachedValue = (GetUnitProperty(x => x.PetCreator) != null) ? Unit.PetCreator.ACDGuid : 0); }
            set { _petCreatorACDId.SetValueOverride(value); }
        }

        /// <summary>
        /// Id for the monster's appearance
        /// </summary>
        public int AppearanceSNO
        {
            get { return _appearanceSNO.IsCacheValid ? _appearanceSNO.CachedValue : (_appearanceSNO.CachedValue = GetObjectProperty(x => x.AppearanceSNO)); }
            set { _appearanceSNO.SetValueOverride(value); }
        }

        /// <summary>
        /// Id for the monster's mesh
        /// </summary>
        public int PhysMeshSNO
        {
            get { return _physMeshSNO.IsCacheValid ? _physMeshSNO.CachedValue : (_physMeshSNO.CachedValue = GetObjectProperty(x => x.PhysMeshSNO)); }
            set { _physMeshSNO.SetValueOverride(value); }
        }

        /// <summary>
        /// Id for the monster's physics
        /// </summary>
        public int PhysicsSNO
        {
            get { return _physicsSNO.IsCacheValid ? _physicsSNO.CachedValue : (_physicsSNO.CachedValue = GetObjectProperty(x => x.PhysicsSNO)); }
            set { _physicsSNO.SetValueOverride(value); }
        }

        /// <summary>
        /// ACDId of the actor that summoned this unit
        /// </summary>
        public int SummonedByACDId
        {
            get { return _summonedByACDId.IsCacheValid ? _summonedByACDId.CachedValue : (_summonedByACDId.CachedValue = GetUnitProperty(x => x.SummonedByACDId)); }
            set { _summonedByACDId.SetValueOverride(value); }
        }

        /// <summary>
        /// ACDId of the actor that summoned this unit
        /// </summary>
        public bool IsSummonedByPlayer
        {
            get { return _isSummonedByPlayer.IsCacheValid ? _isSummonedByPlayer.CachedValue : (_isSummonedByPlayer.CachedValue = SummonedByACDId == CacheManager.Me.DynamicId || PetCreatorACDId == ZetaDia.Me.ACDGuid); }
            set { _isSummonedByPlayer.SetValueOverride(value); }
        }

        /// <summary>
        /// Rank of monster Trash, Elite, Boss etc.
        /// </summary>
        public MonsterQuality MonsterQualityLevel
        {
            get { return _monsterQuality.IsCacheValid ? _monsterQuality.CachedValue : (_monsterQuality.CachedValue = (MonsterQuality)Source.ReadMemoryOrDefault<int>(0xB8)); }
            set { _monsterQuality.SetValueOverride(value); }
        }

        /// <summary>
        /// If elite pack champion
        /// </summary>
        public bool IsElite
        {
            get { return _isElite.IsCacheValid ? _isElite.CachedValue : (_isElite.CachedValue = MonsterQualityLevel == MonsterQuality.Champion); }
            set { _isElite.SetValueOverride(value); }
        }

        /// <summary>
        /// If rare unit
        /// </summary>
        public bool IsRare
        {
            get { return _isRare.IsCacheValid ? _isRare.CachedValue : (_isRare.CachedValue = MonsterQualityLevel == MonsterQuality.Rare); }
            set { _isRare.SetValueOverride(value); }
        }

        /// <summary>
        /// If is Elite pack minion
        /// </summary>
        public bool IsMinion
        {
            get { return _isMinion.IsCacheValid ? _isMinion.CachedValue : (_isMinion.CachedValue = MonsterQualityLevel == MonsterQuality.Minion); }
            set { _isMinion.SetValueOverride(value); }
        }

        /// <summary>
        /// If Unique unit
        /// </summary>
        public bool IsUnique
        {
            get { return _isUnique.IsCacheValid ? _isUnique.CachedValue : (_isUnique.CachedValue = MonsterQualityLevel == MonsterQuality.Unique); }
            set { _isUnique.SetValueOverride(value); }
        }

        /// <summary>
        /// If Boss unit
        /// </summary>
        public bool IsBoss
        {
            get { return _isBoss.IsCacheValid ? _isBoss.CachedValue : (_isBoss.CachedValue = MonsterQualityLevel == MonsterQuality.Boss); }
            set { _isBoss.SetValueOverride(value); }
        }

        /// <summary>
        /// Is currently facing the player
        /// </summary>
        public bool IsFacingPlayer
        {
            get { return _isFacingPlayer.IsCacheValid ? _isFacingPlayer.CachedValue : (_isFacingPlayer.CachedValue = GetObjectProperty(x => x.IsFacingPlayer)); }
            set { _isFacingPlayer.SetValueOverride(value); }
        }

        /// <summary>
        /// Monster Affixes
        /// </summary>
        public MonsterAffixes MonsterAffixes
        {
            get { return _monsterAffixes.IsCacheValid ? _monsterAffixes.CachedValue : (_monsterAffixes.CachedValue = GetSourceProperty(x => x.MonsterAffixes)); }
            set { _monsterAffixes.SetValueOverride(value); }
        }

        /// <summary>
        /// Monster Affix Entries
        /// </summary>
        public List<MonsterAffixEntry> MonsterAffixEntries
        {
            get { return _monsterAffixEntries.IsCacheValid ? _monsterAffixEntries.CachedValue : (_monsterAffixEntries.CachedValue = GetSourceProperty(x => x.MonsterAffixEntries).ToList()); }
            set { _monsterAffixEntries.SetValueOverride(value); }
        }

        /// <summary>
        /// Affixes
        /// </summary>
        public List<MonsterAffix> Affixes
        {
            get { return _affixes.IsCacheValid ? _affixes.CachedValue : (_affixes.CachedValue = GetSourceProperty(x => x.Affixes).Select(x => (MonsterAffix)x).ToList()); }
            set { _affixes.SetValueOverride(value); }
        }

        /// <summary>
        /// Is currently nleeding
        /// </summary>
        public bool IsBleeding
        {
            get { return _isBleeding.IsCacheValid ? _isBleeding.CachedValue : (_isBleeding.CachedValue = GetUnitProperty(x => x.IsBleeding)); }
            set { _isBleeding.SetValueOverride(value); }
        }

        /// <summary>
        /// Is currently burrowed
        /// </summary>
        public bool IsBurrowed
        {
            get { return _isBurrowed.IsCacheValid ? _isBurrowed.CachedValue : (_isBurrowed.CachedValue = GetUnitProperty(x => x.IsBurrowed)); }
            set { _isBurrowed.SetValueOverride(value); }
        }

        /// <summary>
        /// Is currently Enraged
        /// </summary>
        public bool IsEnraged
        {
            get { return _isEnraged.IsCacheValid ? _isEnraged.CachedValue : (_isEnraged.CachedValue = GetUnitProperty(x => x.IsEnraged)); }
            set { _isEnraged.SetValueOverride(value); }
        }

        /// <summary>
        /// 
        /// </summary>
        public bool IsQuestMonster
        {
            get { return _isQuestMonster.IsCacheValid ? _isQuestMonster.CachedValue : (_isQuestMonster.CachedValue = GetUnitProperty(x => x.IsQuestMonster)); }
            set { _isQuestMonster.SetValueOverride(value); }
        }

        /// <summary>
        /// If unit is invisible
        /// </summary>
        public bool IsStealthed
        {
            get { return _isStealthed.IsCacheValid ? _isStealthed.CachedValue : (_isStealthed.CachedValue = GetUnitProperty(x => x.IsStealthed)); }
            set { _isStealthed.SetValueOverride(value); }
        }

        /// <summary>
        /// If unit can't be interrupted
        /// </summary>
        public bool IsUninterruptible
        {
            get { return _isUninterruptible.IsCacheValid ? _isUninterruptible.CachedValue : (_isUninterruptible.CachedValue = GetUnitProperty(x => x.IsUninterruptible)); }
            set { _isUninterruptible.SetValueOverride(value); }
        }

        /// <summary>
        /// Monster race e.g. Animal, Fallen, Spider.
        /// </summary>
        public MonsterRace MonsterRace
        {
            get { return _monsterRace.IsCacheValid ? _monsterRace.CachedValue : (_monsterRace.CachedValue = GetUnitProperty(x => x.MonsterInfo.MonsterRace)); }
            set { _monsterRace.SetValueOverride(value); }
        }

        /// <summary>
        /// Monster Size e.g. Big, Boss, Ranged, Swarm
        /// </summary>
        public MonsterSize MonsterSize
        {
            get { return _monsterSize.IsCacheValid ? _monsterSize.CachedValue : (_monsterSize.CachedValue = GetUnitProperty(x => x.MonsterInfo.MonsterSize)); }
            set { _monsterSize.SetValueOverride(value); }
        }

        /// <summary>
        /// If unit is webbed
        /// </summary>
        public bool IsWebbed
        {
            get { return _isWebbed.IsCacheValid ? _isWebbed.CachedValue : (_isWebbed.CachedValue = GetUnitProperty(x => x.IsWebbed)); }
            set { _isWebbed.SetValueOverride(value); }
        }

        /// <summary>
        /// If unit is currently charging
        /// </summary>
        public bool IsCharging
        {
            get { return _isCharging.IsCacheValid ? _isCharging.CachedValue : (_isCharging.CachedValue = GetUnitProperty(x => x.IsFacingPlayer) && DataDictionary.ActorChargeAnimations.Any(a => a == CurrentAnimation)); }
            set { _isCharging.SetValueOverride(value); }
        }

        /// <summary>
        /// If unit has a dot damaging it currently
        /// </summary>
        public bool HasDotDps
        {
            get { return _hasDotDps.IsCacheValid ? _hasDotDps.CachedValue : (_hasDotDps.CachedValue = Source.GetAttributeOrDefault<int>(ActorAttributeType.DOTDPS) != 0); }
            set { _hasDotDps.SetValueOverride(value); }
        }

        /// <summary>
        /// If has shielding affix
        /// </summary>
        public bool HasShieldingAffix
        {
            get { return _hasShieldingAffix.IsCacheValid ? _hasShieldingAffix.CachedValue : (_hasShieldingAffix.CachedValue = MonsterAffixes.HasFlag(MonsterAffixes.Shielding)); }
            set { _hasShieldingAffix.SetValueOverride(value); }
        }

        /// <summary>
        /// Is a treasure goblin
        /// </summary>
        public bool IsTreasureGoblin
        {
            get { return _isTreasureGoblin.IsCacheValid ? _isTreasureGoblin.CachedValue : (_isTreasureGoblin.CachedValue = DataDictionary.GoblinIds.Contains(ActorSNO) || InternalName.ToLower().StartsWith("treasureGoblin")); }
            set { _isTreasureGoblin.SetValueOverride(value); }
        }

        /// <summary>
        /// Is elite, minion, unique or rare.
        /// </summary>
        public bool IsEliteRareUnique
        {
            get { return _isEliteRareUnique.IsCacheValid ? _isEliteRareUnique.CachedValue : (_isEliteRareUnique.CachedValue = IsElite || IsRare || IsUnique || IsMinion); }
            set { _isEliteRareUnique.SetValueOverride(value); }
        }

        /// <summary>
        /// Is Boss, elite, minion, unique or rare.
        /// </summary>
        public bool IsBossOrEliteRareUnique
        {
            get { return _isBossOrEliteRareUnique.IsCacheValid ? _isBossOrEliteRareUnique.CachedValue : (_isBossOrEliteRareUnique.CachedValue = IsEliteRareUnique || IsBoss); }
            set { _isBossOrEliteRareUnique.SetValueOverride(value); }
        }

        /// <summary>
        /// Is just a shitty mob
        /// </summary>
        public bool IsTrash
        {
            get { return _isTrash.IsCacheValid ? _isTrash.CachedValue : (_isTrash.CachedValue = !(IsBossOrEliteRareUnique || IsTreasureGoblin)); }
            set { _isTrash.SetValueOverride(value); }
        }

        /// <summary>
        /// SNO of the actor who summoned this
        /// </summary>
        public int SummonedBySNO
        {
            get { return _summonedBySNO.IsCacheValid ? _summonedBySNO.CachedValue : (_summonedBySNO.CachedValue = GetUnitProperty(x => x.SummonedBySNO)); }
            set { _summonedBySNO.SetValueOverride(value); }
        }

        /// <summary>
        /// Id (DynamicId?) of the actor who summoned this
        /// </summary>
        public int SummonerId
        {
            get { return _summonerId.IsCacheValid ? _summonerId.CachedValue : (_summonerId.CachedValue = GetUnitProperty(x => x.SummonerId)); }
            set { _summonerId.SetValueOverride(value); }
        }

        /// <summary>
        /// ACDGuid of the actor who summoned this
        /// </summary>
        public int SummonerACDGuid
        {
            get { return _summonerACDGuid.IsCacheValid ? _summonerACDGuid.CachedValue : (_summonerACDGuid.CachedValue = GetUnitProperty(x => x.ACDGuid)); }
            set { _summonerACDGuid.SetValueOverride(value); }
        }

        /// <summary>
        ///  A quantity that can multiply movement vectors
        /// </summary>
        public float MovementScalar
        {
            get { return _movementScalar.IsCacheValid ? _movementScalar.CachedValue : (_movementScalar.CachedValue = GetUnitProperty(x => x.MovementScalar)); }
            set { _movementScalar.SetValueOverride(value); }
        }

        /// <summary>
        ///  Maximum movement scalar 
        /// </summary>
        public float MovementScalarCap
        {
            get { return _movementScalarCap.IsCacheValid ? _movementScalarCap.CachedValue : (_movementScalarCap.CachedValue = GetUnitProperty(x => x.MovementScalar)); }
            set { _movementScalarCap.SetValueOverride(value); }
        }

        /// <summary>
        ///  
        /// </summary>
        public float MovementScalarCappedTotal
        {
            get { return _movementScalarCappedTotal.IsCacheValid ? _movementScalarCappedTotal.CachedValue : (_movementScalarCappedTotal.CachedValue = GetUnitProperty(x => x.MovementScalarCappedTotal)); }
            set { _movementScalarCappedTotal.SetValueOverride(value); }
        }

        /// <summary>
        ///  
        /// </summary>
        public float MovementScalarSubtotal
        {
            get { return _movementScalarSubtotal.IsCacheValid ? _movementScalarSubtotal.CachedValue : (_movementScalarSubtotal.CachedValue = GetUnitProperty(x => x.MovementScalarSubtotal)); }
            set { _movementScalarSubtotal.SetValueOverride(value); }
        }

        /// <summary>
        ///  
        /// </summary>
        public float MovementScalarTotal
        {
            get { return _movementScalarTotal.IsCacheValid ? _movementScalarTotal.CachedValue : (_movementScalarTotal.CachedValue = GetUnitProperty(x => x.MovementScalarTotal)); }
            set { _movementScalarTotal.SetValueOverride(value); }
        }

        /// <summary>
        /// 
        /// </summary>
        public int RootTargetACD
        {
            get { return _rootTargetACD.IsCacheValid ? _rootTargetACD.CachedValue : (_rootTargetACD.CachedValue = GetUnitProperty(x => x.RootTargetACD)); }
            set { _rootTargetACD.SetValueOverride(value); }
        }

        /// <summary>
        /// 
        /// </summary>
        public float RunningRate
        {
            get { return _runningRate.IsCacheValid ? _runningRate.CachedValue : (_runningRate.CachedValue = GetUnitProperty(x => x.RunningRate)); }
            set { _runningRate.SetValueOverride(value); }
        }

        /// <summary>
        /// 
        /// </summary>
        public float RunningRateTotal
        {
            get { return _runningRateTotal.IsCacheValid ? _runningRateTotal.CachedValue : (_runningRateTotal.CachedValue = GetUnitProperty(x => x.RunningRateTotal)); }
            set { _runningRateTotal.SetValueOverride(value); }
        }

        /// <summary>
        /// If unit is alive 
        /// </summary>
        public bool IsAlive
        {
            get { return _isAlive.IsCacheValid ? _isAlive.CachedValue : (_isAlive.CachedValue = !IsDead); }
            set { _isAlive.SetValueOverride(value); }
        }

        /// <summary>
        /// If unit is dead 
        /// </summary>
        public bool IsDead
        {
            get { return _isDead.IsCacheValid ? _isDead.CachedValue : (_isDead.CachedValue = GetUnitProperty(x => x.HitpointsCurrent <= 0)); }
            set { _isDead.SetValueOverride(value); }
        }

        /// <summary>
        /// If unit is attackable
        /// </summary>
        public bool IsAttackable
        {
            get { return _isAttackable.IsCacheValid ? _isAttackable.CachedValue : (_isAttackable.CachedValue = GetIsAttackable(this)); }
            set { _isAttackable.SetValueOverride(value); }
        }

        /// <summary>
        /// If unit is friendly
        /// </summary>
        public bool IsFriendly
        {
            get { return _isFriendly.IsCacheValid ? _isFriendly.CachedValue : (_isFriendly.CachedValue = GetUnitProperty(x => x.IsFriendly)); }
            set { _isFriendly.SetValueOverride(value); }
        }

        /// <summary>
        /// If unit is Helper
        /// </summary>
        public bool IsHelper
        {
            get { return _isHelper.IsCacheValid ? _isHelper.CachedValue : (_isHelper.CachedValue = GetUnitProperty(x => x.IsHelper)); }
            set { _isHelper.SetValueOverride(value); }
        }

        /// <summary>
        /// If unit is Invisible
        /// </summary>
        public bool IsHidden
        {
            get { return _isHidden.IsCacheValid ? _isHidden.CachedValue : (_isHidden.CachedValue = GetUnitProperty(x => x.IsHidden)); }
            set { _isHidden.SetValueOverride(value); }
        }

        /// <summary>
        /// If unit is Rooted
        /// </summary>
        public bool IsRooted
        {
            get { return _isRooted.IsCacheValid ? _isRooted.CachedValue : (_isRooted.CachedValue = GetUnitProperty(x => x.IsRooted)); }
            set { _isRooted.SetValueOverride(value); }
        }

        /// <summary>
        /// If unit is Blind
        /// </summary>
        public bool IsBlind
        {
            get { return _isBlind.IsCacheValid ? _isBlind.CachedValue : (_isBlind.CachedValue = GetUnitProperty(x => x.IsFriendly)); }
            set { _isBlind.SetValueOverride(value); }
        }

        /// <summary>
        /// If unit is Feared
        /// </summary>
        public bool IsFeared
        {
            get { return _isFeared.IsCacheValid ? _isFeared.CachedValue : (_isFeared.CachedValue = GetUnitProperty(x => x.IsFeared)); }
            set { _isFeared.SetValueOverride(value); }
        }

        /// <summary>
        /// If unit is Chilled
        /// </summary>
        public bool IsChilled
        {
            get { return _isChilled.IsCacheValid ? _isChilled.CachedValue : (_isChilled.CachedValue = GetUnitProperty(x => x.IsChilled)); }
            set { _isChilled.SetValueOverride(value); }
        }

        /// <summary>
        /// If unit is Frozen
        /// </summary>
        public bool IsFrozen
        {
            get { return _isFrozen.IsCacheValid ? _isFrozen.CachedValue : (_isFrozen.CachedValue = GetUnitProperty(x => x.IsFrozen)); }
            set { _isFrozen.SetValueOverride(value); }
        }

        /// <summary>
        /// If unit is Stunned
        /// </summary>
        public bool IsStunned
        {
            get { return _isStunned.IsCacheValid ? _isBlind.CachedValue : (_isStunned.CachedValue = GetUnitProperty(x => x.IsStunned)); }
            set { _isStunned.SetValueOverride(value); }
        }

        /// <summary>
        /// If unit is Slowed
        /// </summary>
        public bool IsSlowed
        {
            get { return _isSlowed.IsCacheValid ? _isSlowed.CachedValue : (_isSlowed.CachedValue = GetUnitProperty(x => x.IsSlow)); }
            set { _isSlowed.SetValueOverride(value); }
        }

        /// <summary>
        /// If unit is Hostile
        /// </summary>
        public bool IsHostile
        {
            get { return _isHostile.IsCacheValid ? _isHostile.CachedValue : (_isHostile.CachedValue = GetUnitProperty(x => x.IsHostile)); }
            set { _isHostile.SetValueOverride(value); }
        }

        /// <summary>
        /// If unit is an NPC
        /// </summary>
        public bool IsNPC
        {
            get { return _isNPC.IsCacheValid ? _isNPC.CachedValue : (_isNPC.CachedValue = GetUnitProperty(x => x.IsNPC)); }
            set { _isNPC.SetValueOverride(value); }
        }

        /// <summary>
        /// If NPC Unit and operable
        /// </summary>
        public bool IsNPCOperable
        {
            get { return _isNPCOperable.IsCacheValid ? _isNPCOperable.CachedValue : (_isNPCOperable.CachedValue = IsNPC && Source.GetAttributeOrDefault<int>(ActorAttributeType.NPCIsOperatable) > 0); }
            set { _isNPCOperable.SetValueOverride(value); }
        }


        /// <summary>
        /// Unit has a quest to give
        /// </summary>
        public bool IsQuestGiver
        {
            get { return _isQuestGiver.IsCacheValid ? _isQuestGiver.CachedValue : (_isQuestGiver.CachedValue = GetUnitProperty(x => x.IsQuestGiver)); }
            set { _isQuestGiver.SetValueOverride(value); }
        }

        /// <summary>
        /// Is a salvage vendor tab shortcut
        /// </summary>
        public bool IsSalvageShortcut
        {
            get { return _isSalvageShortcut.IsCacheValid ? _isSalvageShortcut.CachedValue : (_isSalvageShortcut.CachedValue = GetUnitProperty(x => x.IsSalvageShortcut)); }
            set { _isSalvageShortcut.SetValueOverride(value); }
        }

        /// <summary>
        /// If unit is a vendor in town
        /// </summary>
        public bool IsTownVendor
        {
            get { return _isTownVendor.IsCacheValid ? _isTownVendor.CachedValue : (_isTownVendor.CachedValue = GetUnitProperty(x => x.IsTownVendor)); }
            set { _isTownVendor.SetValueOverride(value); }
        }

        /// <summary>
        /// Type of hireling unit is
        /// </summary>
        public HirelingType HirelingType
        {
            get { return _hirelingType.IsCacheValid ? _hirelingType.CachedValue : (_hirelingType.CachedValue = GetUnitProperty(x => x.HirelingType)); }
            set { _hirelingType.SetValueOverride(value); }
        }

        /// <summary>
        /// Amount of hitpoints regen per second from skills and items
        /// </summary>
        public float HitpointsRegenPerSecond
        {
            get { return _hitpointsRegenPerSecond.IsCacheValid ? _hitpointsRegenPerSecond.CachedValue : (_hitpointsRegenPerSecond.CachedValue = GetUnitProperty(x => x.HitpointsRegenPerSecond)); }
            set { _hitpointsRegenPerSecond.SetValueOverride(value); }
        }

        /// <summary>
        /// Total hitpoints regenerated per second
        /// </summary>
        public float HitpointsRegenPerSecondTotal
        {
            get { return _hitpointsRegenPerSecondTotal.IsCacheValid ? _hitpointsRegenPerSecondTotal.CachedValue : (_hitpointsRegenPerSecondTotal.CachedValue = GetUnitProperty(x => x.HitpointsRegenPerSecondTotal)); }
            set { _hitpointsRegenPerSecondTotal.SetValueOverride(value); }
        }

        /// <summary>
        /// If unit is currently on top of an avoidance.
        /// </summary>
        public bool IsStandingInAvoidance
        {
            get { return _isStandingInAvoidance.IsCacheValid ? _isStandingInAvoidance.CachedValue : (_isStandingInAvoidance.CachedValue = GetIsStandingInAvoidance(this)); }
            set { _isStandingInAvoidance.SetValueOverride(value); }
        }

        /// <summary>
        /// Count of units directly behind this unit via ray from player
        /// </summary>
        public int UnitsBehind
        {
            get { return _unitsBehind.IsCacheValid ? _unitsBehind.CachedValue : (_unitsBehind.CachedValue = GetUnitsBehind(this)); }
            set { _unitsBehind.SetValueOverride(value); }
        }

        /// <summary>
        /// Count of units between player and this unit
        /// </summary>
        public int UnitsInFront
        {
            get { return _unitsInFront.IsCacheValid ? _unitsInFront.CachedValue : (_unitsInFront.CachedValue = GetUnitsInFront(this)); }
            set { _unitsInFront.SetValueOverride(value); }
        }

        /// <summary>
        /// Count of units within cluster radius of this unit
        /// </summary>
        public int UnitsNearby
        {
            get { return _unitsNearby.IsCacheValid ? _unitsNearby.CachedValue : (_unitsNearby.CachedValue = GetUnitsNearby(this)); }
            set { _unitsNearby.SetValueOverride(value); }
        }

        /// <summary>
        /// Source for movement related information (accessing properties reads directly from Diablo memory)
        /// </summary>
        public ActorMovement Movement
        {
            get { return _movement.IsCacheValid ? _movement.CachedValue : (_movement.CachedValue = GetUnitProperty(x => x.Movement)); }
            set { _movement.SetValueOverride(value); }
        }

        /// <summary>
        /// Count of units within cluster radius of this unit
        /// </summary>
        public Vector2 DirectionVector
        {
            get { return _directionVector.IsCacheValid ? _directionVector.CachedValue : (_directionVector.CachedValue = GetProperty(Movement, x => x.DirectionVector)); }
            set { _directionVector.SetValueOverride(value); }
        }

        /// <summary>
        /// How fast unit is currentlyl moving
        /// </summary>
        public float MovementSpeed
        {
            get { return _movementSpeed.IsCacheValid ? _movementSpeed.CachedValue : (_movementSpeed.CachedValue = GetProperty(Movement, x => x.SpeedXY)); }
            set { _movementSpeed.SetValueOverride(value); }
        }

        /// <summary>
        /// If unit is moving
        /// </summary>
        public bool IsMoving
        {
            get { return _isMoving.IsCacheValid ? _isMoving.CachedValue : (_isMoving.CachedValue = GetProperty(Movement, x => x.IsMoving)); }
            set { _isMoving.SetValueOverride(value); }
        }

        /// <summary>
        /// Rotation of the actor
        /// </summary>
        public float Rotation
        {
            get { return _rotation.IsCacheValid ? _rotation.CachedValue : (_rotation.CachedValue = GetProperty(Movement, x => x.Rotation)); }
            set { _rotation.SetValueOverride(value); }
        }

        /// <summary>
        /// Rotation of the actor in Degrees
        /// </summary>
        public float RotationDegrees
        {
            get { return _rotationDegrees.IsCacheValid ? _rotationDegrees.CachedValue : (_rotationDegrees.CachedValue = GetProperty(Movement, x => x.RotationDegrees)); }
            set { _rotationDegrees.SetValueOverride(value); }
        }

        /// <summary>
        /// Current hitpoints as a percentage of maximum.
        /// </summary>
        public float CurrentHealthPct
        {
            get { return _currentHealthPct.IsCacheValid ? _currentHealthPct.CachedValue : (_currentHealthPct.CachedValue = GetUnitProperty(x => x.HitpointsCurrentPct)); }
            set { _currentHealthPct.SetValueOverride(value); }
        }

        /// <summary>
        /// If unit is this amount or closer to the player, kill it.
        /// </summary>
        public double KillRange
        {
            get { return _killRange.IsCacheValid ? _killRange.CachedValue : (_killRange.CachedValue = GetUnitProperty(x => GetKillRange(this))); }
            set { _killRange.SetValueOverride(value); }
        }

        /// <summary>
        /// If this unit summons stuff
        /// </summary>
        public bool IsSummoner
        {
            get { return _isSummoner.IsCacheValid ? _isSummoner.CachedValue : (_isSummoner.CachedValue = SummonerId > 0); }
            set { _isSummoner.SetValueOverride(value); }
        }

        /// <summary>
        /// If this unit is a good candidate for leap/charge abilities
        /// </summary>
        public bool IsChargeTarget
        {
            get { return _isChargeTarget.IsCacheValid ? _isChargeTarget.CachedValue : (_isChargeTarget.CachedValue = MonsterSize == MonsterSize.Ranged || DataDictionary.RangedMonsterIds.Contains(ActorSNO)); }
            set { _isChargeTarget.SetValueOverride(value); }
        }


            
        

        #endregion

        #region Methods

        public bool IsPlayerFacing(float arc)
        {
            return CacheManager.Me.IsFacing(Position, arc);
        }

        public bool Interact()
        {
            return Object.Interact();
        }

        public bool IsFacing(Vector3 targetPosition, float arcDegrees = 70f)
        {
            return Object.IsFacing(targetPosition, arcDegrees);
        }

        /// <summary>
        /// The number of actors directly behind this actor
        /// </summary>
        internal static int GetUnitsBehind(TrinityUnit o)
        {
            return (from u in CacheManager.Units
                    where u.ACDGuid != o.ACDGuid && o.IsAlive && MathUtil.IntersectsPath(o.Position, o.Radius, CacheManager.Me.Position, u.Position)
                select u).Count();
        }

        /// <summary>
        /// The number of actors between you and this actor
        /// </summary>
        internal static int GetUnitsInFront(TrinityUnit o)
        {
            return (from u in CacheManager.Units
                    where u.ACDGuid != o.ACDGuid && o.IsAlive && MathUtil.IntersectsPath(u.Position, u.Radius, CacheManager.Me.Position, o.Position)
                select u).Count();
        }

        /// <summary>
        /// Get attack distance specific to a unit
        /// </summary>
        private static double GetKillRange(TrinityUnit o)
        {
            var killRange = (double)Math.Max(Trinity.Settings.Combat.Misc.EliteRange, Trinity.Settings.Combat.Misc.NonEliteRange);

            // Always within kill range if in the NoCheckKillRange list!
            if (DataDictionary.NoCheckKillRange.Contains(o.ActorSNO))
                return o.RadiusDistance + 100f;

            // Bosses, always kill
            if (o.IsBoss)
                return o.RadiusDistance + 100f;

            // Elitey type mobs and things
            if (o.IsEliteRareUnique)
                killRange = Trinity.Settings.Combat.Misc.EliteRange;

            if (!TownRun.IsTryingToTownPortal())
                return killRange;

            // Safety for TownRuns
            if (killRange <= V.F("Cache.TownPortal.KillRange")) 
                killRange = V.F("Cache.TownPortal.KillRange");

            return killRange;
        }

        /// <summary>
        /// The number of actors clustered around this unit (TrashPackClusterRadius)
        /// </summary>
        internal static int GetUnitsNearby(TrinityUnit o)
        {
            return (from u in CacheManager.Units
                where u.ACDGuid != o.ACDGuid && o.IsAlive && MathUtil.PositionIsInCircle(u.Position, o.Position, Trinity.Settings.Combat.Misc.TrashPackClusterRadius)
                select u).Count();
        }

        /// <summary>
        /// If the unit has a specific debuff
        /// </summary>
        public bool HasDebuff(SNOPower debuffSNO)
        {
            try
            {
                if (Source.GetAttribute<int>(((int)debuffSNO << 12) + ((int)ActorAttributeType.PowerBuff0VisualEffect & 0xFFF)) == 1)
                    return true;
                if (Source.GetAttribute<int>(((int)debuffSNO << 12) + ((int)ActorAttributeType.PowerBuff0VisualEffectA & 0xFFF)) == 1)
                    return true;
                if (Source.GetAttribute<int>(((int)debuffSNO << 12) + ((int)ActorAttributeType.PowerBuff0VisualEffectB & 0xFFF)) == 1)
                    return true;
                if (Source.GetAttribute<int>(((int)debuffSNO << 12) + ((int)ActorAttributeType.PowerBuff0VisualEffectC & 0xFFF)) == 1)
                    return true;
                if (Source.GetAttribute<int>(((int)debuffSNO << 12) + ((int)ActorAttributeType.PowerBuff0VisualEffectD & 0xFFF)) == 1)
                    return true;
                if (Source.GetAttribute<int>(((int)debuffSNO << 12) + ((int)ActorAttributeType.PowerBuff0VisualEffectE & 0xFFF)) == 1)
                    return true;
            }
            catch (Exception ex)
            {
                Logger.Log("Exception in HasDebuff for {0} on {1}. {2}", debuffSNO, Name, ex);
            }
            return false;
        }

        /// <summary>
        /// If unit is facing a specific position within degrees of tolerance (is this faster than .IsFacing property?)
        /// </summary>
        public bool IsFacingPosition(Vector3 targetPosition, float arcDegrees = 70f)
        {
            if (DirectionVector != Vector2.Zero)
            {
                var u = targetPosition - this.Position;
                u.Z = 0f;
                var v = new Vector3(DirectionVector.X, DirectionVector.Y, 0f);
                var result = ((MathEx.ToDegrees(Vector3.AngleBetween(u, v)) <= arcDegrees) ? 1 : 0) != 0;
                return result;
            }

            return false;
        }

        /// <summary>
        /// If unit is attackable factoring overrides
        /// </summary>
        internal static bool GetIsAttackable(TrinityUnit o)
        {
            return o.IsHostile && (o.GetUnitProperty(x => x.IsAttackable) || o.InternalName.StartsWith("Diablo_shadowClone"));
        }

        /// <summary>
        /// Gets the TrinityObject of this unit's summoner
        /// </summary>
        /// <returns></returns>
        internal TrinityUnit GetSummoner()
        {
            var unit = GetUnitProperty(x => x.ACDGuid);
            return unit > 0 ? CacheManager.GetActorByACDGuid<TrinityUnit>(GetUnitProperty(x => x.ACDGuid)) : null;
        }

        /// <summary>
        /// If unit player is standing in avoidance
        /// </summary>
        internal static bool GetIsStandingInAvoidance(TrinityUnit o)
        {
            return CacheManager.Avoidances.Any(a => a.Position.Distance2D(o.Position) <= a.Radius) || CacheData.TimeBoundAvoidance.Any(a => a.Position.Distance2D(o.Position) <= a.Radius);
        }

        public override string ToString()
        {
            return String.Format("{0}, Type={1} Dist={2} IsBossOrEliteRareUnique={3} IsAttackable={4}", Name, TrinityType, RadiusDistance, IsBossOrEliteRareUnique, IsAttackable);
        }

        #endregion

        #region Operators

        public static implicit operator TrinityUnit(ACD x)
        {
            return CacheFactory.CreateObject<TrinityUnit>(x);
        }

        public static explicit operator DiaUnit(TrinityUnit x)
        {
            return x.Unit;
        }

        #endregion
    }
}
