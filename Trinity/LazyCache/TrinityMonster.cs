using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Zeta.Common;
using Zeta.Game;
using Zeta.Game.Internals.Actors;
using Zeta.Game.Internals.SNO;
using Logger = Trinity.Technicals.Logger;

namespace Trinity.LazyCache
{
    /// <summary>
    /// Trinity Monster
    /// </summary>
    public class TrinityMonster : TrinityUnit
    {
        public TrinityMonster(ACD acd) : base(acd) { }

        #region Properties

        public bool IsElite
        {
            get { return CacheManager.GetCacheValue(this, o => o.Source.IsElite); }
        }

        public bool IsRare
        {
            get { return CacheManager.GetCacheValue(this, o => o.Source.IsRare); }
        }

        public bool IsUnique
        {
            get { return CacheManager.GetCacheValue(this, o => o.Source.IsUnique); }
        }

        public List<int> Affixes
        {
            get { return CacheManager.GetCacheValue(this, o => o.Source.Affixes.ToList()); }
        }

        public SNORecordMonster MonsterInfo
        {
            get { return CacheManager.GetCacheValue(this, o => o.Source.MonsterInfo); }
        }

        public List<MonsterAffixEntry> MonsterAffixEntries
        {
            get { return CacheManager.GetCacheValue(this, o => o.Source.MonsterAffixEntries); }
        }

        public bool IsBleeding
        {
            get { return CacheManager.GetCacheValue(this, o => o.Unit.IsBleeding, 250); }
        }

        public bool IsBurrowed
        {
            get { return CacheManager.GetCacheValue(this, o => o.Unit.IsBurrowed, 250); }
        }

        public bool IsEnraged
        {
            get { return CacheManager.GetCacheValue(this, o => o.Unit.IsEnraged, 250); }
        }

        public bool IsAttackSpeedReductionImmune
        {
            get { return CacheManager.GetCacheValue(this, o => o.Unit.IsAttackSpeedReductionImmune, 3000); }
        }

        public bool IsFearImmune
        {
            get { return CacheManager.GetCacheValue(this, o => o.Unit.IsFearImmune, 1000); }
        }

        public bool IsFreezeImmune
        {
            get { return CacheManager.GetCacheValue(this, o => o.Unit.IsFreezeImmune, 1000); }
        }

        public bool IsGethitImmune
        {
            get { return CacheManager.GetCacheValue(this, o => o.Unit.IsGethitImmune, 1000); }
        }

        public bool IsQuestMonster
        {
            get { return CacheManager.GetCacheValue(this, o => o.Unit.IsQuestMonster, 1000); }
        }

        public bool IsRootImmune
        {
            get { return CacheManager.GetCacheValue(this, o => o.Unit.IsRootImmune, 1000); }
        }

        public bool IsSlowImmune
        {
            get { return CacheManager.GetCacheValue(this, o => o.Unit.IsSlowdownImmune, 1000); }
        }

        public bool IsStealthed
        {
            get { return CacheManager.GetCacheValue(this, o => o.Unit.IsStealthed, 250); }
        }

        public bool IsStunImmune
        {
            get { return CacheManager.GetCacheValue(this, o => o.Unit.IsStunImmune, 1000); }
        }

        public bool IsUninterruptible
        {
            get { return CacheManager.GetCacheValue(this, o => o.Unit.IsUninterruptible, 1000); }
        }

        public bool IsUntargetable
        {
            get { return CacheManager.GetCacheValue(this, o => o.Unit.IsUntargetable, 1000); }
        }

        public MonsterType MonsterType
        {
            get { return CacheManager.GetCacheValue(this, o => MonsterInfo.MonsterType); }
        }

        public MonsterRace MonsterRace
        {
            get { return CacheManager.GetCacheValue(this, o => MonsterInfo.MonsterRace); }
        }

        public MonsterSize MonsterSize
        {
            get { return CacheManager.GetCacheValue(this, o => MonsterInfo.MonsterSize); }
        }

        public bool IsWebbed
        {
            get { return CacheManager.GetCacheValue(this, o => o.Unit.IsWebbed, 250); }
        }

        public bool IgnoresCriticalHits
        {
            get { return CacheManager.GetCacheValue(this, o => o.Unit.IgnoresCriticalHits); }
        }

        public bool IsCharging
        {
            get { return CacheManager.GetCacheValue(this, o => o.Unit.IsFacingPlayer && DataDictionary.ActorChargeAnimations.Any(a => a == o.CurrentAnimation), 50); }
        }

        public bool HasDOTDps
        {
            get { return CacheManager.GetCacheValue(this, o => o.Source.GetAttributeOrDefault<int>(ActorAttributeType.DOTDPS) != 0); }
            
        }

        public bool ImmuneToKnockback
        {
            get { return CacheManager.GetCacheValue(this, o => o.Unit.ImmuneToKnockback); }
        }

        public bool HasShieldingAffix
        {
            get { return CacheManager.GetCacheValue(this, o => MonsterAffixes.HasFlag(MonsterAffixes.Shielding)); }            
        }

        #endregion

        #region Methods



        #endregion

        public static implicit operator TrinityMonster(ACD x)
        {
            return CacheFactory.CreateObject<TrinityMonster>(x);
        }

    }
}
