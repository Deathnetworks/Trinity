using System;
using Zeta;
using Zeta.Common;
using Zeta.Internals.Actors;
using Zeta.Internals.SNO;
using System.Text.RegularExpressions;


namespace GilesTrinity.Cache
{
    internal class CacheUnit : CacheObject
    {
        public CacheUnit(ACD acd)
            : base(acd)
        {
            ActorSNO = acd.ActorSNO;
            InternalUnit = (DiaUnit)acd.AsRActor;
            RActorGUID = InternalUnit.RActorGuid;
            DynamicID = acd.DynamicId;
            GameBalanceID = acd.GameBalanceId;
            Name = acd.Name;


            HitpointsCurrent = InternalUnit.HitpointsCurrent;
            HitpointsMax = InternalUnit.HitpointsMax;
            HitpointsMaxTotal = InternalUnit.HitpointsMaxTotal;
            HitpointsCurrentPct = HitpointsCurrent / HitpointsMaxTotal;

            CurrentAnimation = acd.CurrentAnimation;
            MonsterSize = acd.MonsterInfo.MonsterSize;

            IsElite = acd.IsElite;
            IsRare = acd.IsRare;
            IsUnique = acd.IsUnique;
            IsMinion = InternalUnit.SummonedByACDId > 0;
            IsEliteRareUnique = (IsElite || IsRare || IsUnique);
            IsBoss = CacheUtils.IsBossSNO(ActorSNO);
        }

        /// <summary>
        /// Gets a DiaUnit
        /// </summary>
        public DiaUnit InternalUnit { get; set; }

        /// <summary>
        /// Gets the HitPoints of the Unit
        /// </summary>
        public double HitpointsCurrent { get; set; }
        public double HitpointsCurrentPct { get; set; }
        public double HitpointsMax { get; set; }
        public double HitpointsMaxTotal { get; set; }

        /// <summary>
        /// The Current Animation as SNOAnim
        /// </summary>
        public SNOAnim CurrentAnimation { get; set; }
        /// <summary>
        /// MonsterSize 
        /// </summary>
        public MonsterSize MonsterSize { get; set; }

        public bool IsElite { get; set; }
        public bool IsRare { get; set; }
        public bool IsUnique { get; set; }
        /// <summary>
        /// A minion of an Elite, Rare, or Unique Unit
        /// </summary>
        public bool IsMinion { get; set; }
        public bool IsEliteRareUnique { get; set; }
        public bool IsBoss { get; set; }

        public bool IsTreasureGoblin { get; set; }

        public bool IsAttackable { get; set; }
        public bool IsHireling { get; set; }
        public bool IsBurrowed { get; set; }
        public bool IsDead { get; set; }
        public bool IsNPC { get; set; }
        public bool IsRooted { get; set; }
        public bool IsTownVendor { get; set; }
        public bool IsUninterruptible { get; set; }
        public bool IsUntargetable { get; set; }
        public int SummonedByACDId { get; set; }
        public int SummonedBySNO { get; set; }
        public int SummonerId { get; set; }

        /// <summary>
        /// intellq special sauce
        /// </summary>
        public bool ForceLeapAgainst { get; set; }

        public override CacheObject Clone()
        {
            return (CacheObject)this.MemberwiseClone();
        }

    }

}
