using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

        #region Properties

        public float HitpointsCurrent
        {
            get { return CacheManager.GetCacheValue(this, o => o.Unit.HitpointsCurrent, 100); }
        }

        public float HitpointsCurrentPct
        {
            get { return CacheManager.GetCacheValue(this, o => o.Unit.HitpointsCurrentPct, 100); }
        }

        public float HitpointsMax
        {
            get { return CacheManager.GetCacheValue(this, o => o.Unit.HitpointsMax); }
        }

        public bool IsPlayerOwned
        {
            get { return CacheManager.GetCacheValue(this, o => PetOwner == ZetaDia.Me.ACDGuid , 2000); }
        }

        public int PetOwner
        {
            get { return CacheManager.GetCacheValue(this, o => o.Unit.PetOwner, 2000); }
        }

        public int PetType
        {
            get { return CacheManager.GetCacheValue(this, o => o.Unit.PetType); }
        }

        public TrinityUnit PetCreator
        {
            get { return CacheManager.GetCacheValue(this, o => new TrinityUnit(o.Unit.PetCreator.CommonData)); }
        }

        public int AppearanceSNO
        {
            get { return CacheManager.GetCacheValue(this, o => o.Object.AppearanceSNO); }
        }

        public int PhysMeshSNO
        {
            get { return CacheManager.GetCacheValue(this, o => o.Object.PhysMeshSNO); }
        }

        public int PhysicsSNO
        {
            get { return CacheManager.GetCacheValue(this, o => o.Object.PhysicsSNO); }
        }

        public int RActorGuid
        {
            get { return CacheManager.GetCacheValue(this, o => o.Object.RActorGuid); }
        }

        public int SummonedByACDId
        {
            get { return CacheManager.GetCacheValue(this, o => o.Unit.SummonedByACDId); }
        }

        public bool IsSummonedByPlayer
        {
            get { return CacheManager.GetCacheValue(this, o => SummonedByACDId == CacheManager.Me.DynamicId); }
        }

        public MonsterQuality MonsterQualityLevel
        {
            get { return CacheManager.GetCacheValue(this, o => (MonsterQuality)o.Source.ReadMemoryOrDefault<int>(0xB8)); }
        }

        public bool IsElite
        {
            get { return CacheManager.GetCacheValue(this, o => MonsterQualityLevel == MonsterQuality.Champion); }
        }

        public bool IsRare
        {
            get { return CacheManager.GetCacheValue(this, o => MonsterQualityLevel == MonsterQuality.Rare); }
        }

        public bool IsUnique
        {
            get { return CacheManager.GetCacheValue(this, o => MonsterQualityLevel == MonsterQuality.Unique); }
        }

        public bool IsBoss
        {
            get { return CacheManager.GetCacheValue(this, o => MonsterQualityLevel == MonsterQuality.Boss); }
        }

        public bool IsTrash
        {
            get { return CacheManager.GetCacheValue(this, o => MonsterQualityLevel == MonsterQuality.Normal); }
        }

        public bool IsMinion
        {
            get { return CacheManager.GetCacheValue(this, o => MonsterQualityLevel == MonsterQuality.Minion); }
        }

        public bool IsFacingPlayer
        {
            get { return CacheManager.GetCacheValue(this, o => o.Object.IsFacingPlayer, 200); }
        }

        public MonsterAffixes MonsterAffixes
        {
            get { return CacheManager.GetCacheValue(this, o => o.Source.MonsterAffixes); }
        }

        public List<MonsterAffixEntry> MonsterAffixEntries
        {
            get { return CacheManager.GetCacheValue(this, o => o.Source.MonsterAffixEntries); }
        }

        public bool IsTreasureGoblin
        {
            get { return CacheManager.GetCacheValue(this, o => DataDictionary.GoblinIds.Contains(o.ActorId) || o.InternalName.ToLower().StartsWith("treasureGoblin")); }                       
        }

        public bool IsEliteRareUnique
        {
            get { return CacheManager.GetCacheValue(this, o => IsElite || IsRare || IsUnique || IsMinion); }
        }

        public bool IsBossOrEliteRareUnique
        {
            get { return CacheManager.GetCacheValue(this, o => IsEliteRareUnique || IsBoss); }
        }

        public bool IsTrashMob
        {
            get { return CacheManager.GetCacheValue(this, o => !(IsEliteRareUnique || IsBoss || IsTreasureGoblin)); }
        }

        public int SummonedBySNO
        {
            get { return CacheManager.GetCacheValue(this, o => o.Unit.SummonedBySNO); }
        }

        public int SummonerId
        {
            get { return CacheManager.GetCacheValue(this, o => o.Unit.SummonerId); }
        }
        public TrinityUnit Summoner
        {
            get { return CacheManager.GetCacheValue(this, o => new TrinityUnit(o.Unit.Summoner)); }
        }

        public float MovementScalar
        {
            get { return CacheManager.GetCacheValue(this, o => o.Unit.MovementScalar, 1000); }
        }

        public float MovementScalarCap
        {
            get { return CacheManager.GetCacheValue(this, o => o.Unit.MovementScalarCap); }
        }

        public float MovementScalarCappedTotal
        {
            get { return CacheManager.GetCacheValue(this, o => o.Unit.MovementScalarCappedTotal); }
        }

        public float MovementScalarSubtotal
        {
            get { return CacheManager.GetCacheValue(this, o => o.Unit.MovementScalarSubtotal); }
        }

        public float MovementScalarTotal
        {
            get { return CacheManager.GetCacheValue(this, o => o.Unit.MovementScalarTotal); }
        }

        public int RootTargetACD
        {
            get { return CacheManager.GetCacheValue(this, o => o.Unit.RootTargetACD); }
        }

        public float RunningRate
        {
            get { return CacheManager.GetCacheValue(this, o => o.Unit.RunningRate, 1000); }
        }

        public bool IsAlive
        {
            get { return CacheManager.GetCacheValue(this, o => o.Unit.IsAlive, 250); }
        }

        public bool IsDead
        {
            get { return CacheManager.GetCacheValue(this, o => o.Unit.IsDead, 250); }
        }

        public bool IsAttackable
        {
            get { return CacheManager.GetCacheValue(this, o => o.Unit.IsAttackable, 500); }
        }

        public bool IsFriendly
        {
            get { return CacheManager.GetCacheValue(this, o => o.Unit.IsFriendly); }
        }

        public bool IsHelper
        {
            get { return CacheManager.GetCacheValue(this, o => o.Unit.IsHelper); }
        }

        public bool IsHidden
        {
            get { return CacheManager.GetCacheValue(this, o => o.Unit.IsHidden, 250); }
        }

        public bool IsRooted
        {
            get { return CacheManager.GetCacheValue(this, o => o.Unit.IsRooted, 250); }
        }

        public bool IsBlind
        {
            get { return CacheManager.GetCacheValue(this, o => o.Unit.IsBlind, 250); }
        }

        public bool IsFeared
        {
            get { return CacheManager.GetCacheValue(this, o => o.Unit.IsFeared, 250); }
        }

        public bool IsChilled
        {
            get { return CacheManager.GetCacheValue(this, o => o.Unit.IsChilled, 250); }
        }

        public bool IsFrozen
        {
            get { return CacheManager.GetCacheValue(this, o => o.Unit.IsFrozen, 250); }
        }

        public bool IsStunned
        {
            get { return CacheManager.GetCacheValue(this, o => o.Unit.IsStunned, 250); }
        }

        public bool IsSlowed
        {
            get { return CacheManager.GetCacheValue(this, o => o.Unit.IsSlow, 250); }
        }

        public bool IsHostile
        {
            get { return CacheManager.GetCacheValue(this, o => o.Unit.IsHostile, 500); }
        }

        public bool IsInvulnerable
        {
            get { return CacheManager.GetCacheValue(this, o => o.Unit.IsInvulnerable, 500); }
        }

        public bool IsNPC
        {
            get { return CacheManager.GetCacheValue(this, o => o.Unit.IsNPC); }
        }

        public bool IsQuestGiver
        {
            get { return CacheManager.GetCacheValue(this, o => o.Unit.IsQuestGiver, 2000); }
        }

        public bool IsSalvageShortcut
        {
            get { return CacheManager.GetCacheValue(this, o => o.Unit.IsSalvageShortcut); }
        }

        public bool IsTownVendor
        {
            get { return CacheManager.GetCacheValue(this, o => o.Unit.IsTownVendor); }
        }

        public HirelingType HirelingType
        {
            get { return CacheManager.GetCacheValue(this, o => o.Unit.HirelingType); }
        }

        public float HitpointsRegenPerSecond
        {
            get { return CacheManager.GetCacheValue(this, o => o.Unit.HitpointsRegenPerSecond); }
        }

        public bool IsStandingInAvoidance
        {
            get { return CacheManager.GetCacheValue(this, o => CacheData.TimeBoundAvoidance.Any(a => a.Position.Distance2D(Position) <= a.Radius),250); }
        }

        public float UnitsBehind
        {
            get 
            { 
                return CacheManager.GetCacheValue(this, o =>
                {
                    return
                        (from u in CacheManager.Monsters
                         where u.ACDGuid != ACDGuid && MathUtil.IntersectsPath(Position, Radius, CacheManager.Me.Position, u.Position)
                         select u).Count();

                }, 500); 
            }
        }

        public float UnitsInFront
        {
            get
            {
                return CacheManager.GetCacheValue(this, o =>
                {
                    return
                        (from u in CacheManager.Monsters
                         where u.ACDGuid != ACDGuid && MathUtil.IntersectsPath(u.Position, u.Radius, CacheManager.Me.Position, Position)
                         select u).Count();

                }, 500);
            }
        }

        public Vector2 DirectionVector
        {
            get { return CacheManager.GetCacheValue(this, o => ZetaDia.Me.Movement.DirectionVector, 100); }
        }

        public float MovementSpeed
        {
            get { return CacheManager.GetCacheValue(this, o => ZetaDia.Me.Movement.SpeedXY, 50); }
        }

        public bool IsMoving
        {
            get { return CacheManager.GetCacheValue(this, o => ZetaDia.Me.Movement.IsMoving, 50); }
        }

        public float Rotation
        {
            get { return CacheManager.GetCacheValue(this, o => ZetaDia.Me.Movement.Rotation, 50); }
        }

        public float CurrentHealthPct
        {
            get { return CacheManager.GetCacheValue(this, o => o.Unit.HitpointsCurrentPct, 50); }
        }       


        public bool IsMarker { get; set; }

        #endregion

        #region Methods

        public bool IsPlayerFacing(float arc)
        {
            return CacheManager.Me.IsFacing(this.Position, arc);
        }

        public int NearbyUnitsWithinDistance(float range = 5f)
        {
            using (new Technicals.PerformanceLogger("CacheObject.UnitsNear"))
            {
                return Trinity.ObjectCache
                    .Count(u => u.RActorGuid != this.RActorGuid && u.IsUnit && u.Position.Distance2D(this.Position) <= range && u.HasBeenInLoS);
            }
        }

        public bool Interact()
        {
            return Object.Interact();
        }

        public bool IsFacing(Vector3 targetPosition, float arcDegrees = 70f)
        {
            return Object.IsFacing(targetPosition, arcDegrees);
        }

        public bool HasDebuff(SNOPower debuffSNO)
        {
            try
            {
                if (Source.GetAttribute<int>(((int) debuffSNO << 12) + ((int) ActorAttributeType.PowerBuff0VisualEffect & 0xFFF)) == 1)
                    return true;
                if (Source.GetAttribute<int>(((int) debuffSNO << 12) + ((int) ActorAttributeType.PowerBuff0VisualEffectA & 0xFFF)) == 1)
                    return true;
                if (Source.GetAttribute<int>(((int) debuffSNO << 12) + ((int) ActorAttributeType.PowerBuff0VisualEffectB & 0xFFF)) == 1)
                    return true;
                if (Source.GetAttribute<int>(((int) debuffSNO << 12) + ((int) ActorAttributeType.PowerBuff0VisualEffectC & 0xFFF)) == 1)
                    return true;
                if (Source.GetAttribute<int>(((int) debuffSNO << 12) + ((int) ActorAttributeType.PowerBuff0VisualEffectD & 0xFFF)) == 1)
                    return true;
                if (Source.GetAttribute<int>(((int) debuffSNO << 12) + ((int) ActorAttributeType.PowerBuff0VisualEffectE & 0xFFF)) == 1)
                    return true;
            }
            catch (Exception ex)
            {
                Logger.Log("Exception in HasDebuff for {0} on {1}. {2}", debuffSNO, Name, ex);
            }
            return false;
        }

        //public bool IsFacing(Vector3 targetPosition, float arcDegrees = 70f)
        //{
        //    if (DirectionVector != Vector2.Zero)
        //    {
        //        Vector3 u = targetPosition - this.Position;
        //        u.Z = 0f;
        //        Vector3 v = new Vector3(DirectionVector.X, DirectionVector.Y, 0f);
        //        bool result = ((MathEx.ToDegrees(Vector3.AngleBetween(u, v)) <= arcDegrees) ? 1 : 0) != 0;
        //        return result;
        //    }
        //    else
        //        return false;
        //}

        public override string ToString()
        {
            return string.Format("{0}, Type={1} Dist={2} IsBossOrEliteRareUnique={3} IsAttackable={4}", InternalName, Type, RadiusDistance, IsBossOrEliteRareUnique, IsAttackable);
        }

        #endregion

        public static implicit operator TrinityUnit(ACD x)
        {
            return CacheFactory.CreateObject<TrinityUnit>(x);
        }

    }
}
