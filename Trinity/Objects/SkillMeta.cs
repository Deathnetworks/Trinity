using System;
using System.Collections.Generic;
using System.Linq;
using Trinity.Combat.Abilities;
using Zeta.Common;
using Logger = Trinity.Technicals.Logger;

namespace Trinity.Objects
{
    /// <summary>
    /// Additional combat behavior related metadata for a skill
    /// </summary>
    public class SkillMeta : ICloneable
    {
        #region Constructors

        public SkillMeta(Skill skill = null)
        {
            Skill = skill;
        }

        #endregion

        #region Properties

        /// <summary>
        /// The skill this constraint is intended for.
        /// </summary>
        public Skill Skill { get; set; }

        /// <summary>
        /// Should only be used while in combat. 
        /// </summary>
        public bool IsCombatOnly
        {
            get { return _isCombatOnly.GetValueOrDefault(); }
            set { _isCombatOnly = value; }
        }

        /// <summary>
        /// Should only be used for elites. 
        /// </summary> 
        public bool IsEliteOnly
        {
            get { return _isEliteOnly.GetValueOrDefault(); }
            set { _isEliteOnly = value; }
        }

        /// <summary>
        /// Should only be used for elites
        /// </summary> 
        public bool IsCastOnSelf
        {
            get { return _isCastOnSelf.GetValueOrDefault(); }
            set { _isCastOnSelf = value; }
        }

        /// <summary>
        /// Can be used to avoid stuff
        /// </summary>        
        public bool IsAvoidanceSkill
        {
            get { return _isAvoidanceSkill.GetValueOrDefault(); }
            set { _isAvoidanceSkill = value; }
        }

        /// <summary>
        /// Can be used to move places faster   
        /// </summary>    
        public bool IsMovementSkill
        {
            get { return _isMovementSkill.GetValueOrDefault(); }
            set { _isMovementSkill = value; }
        }

        /// <summary>
        /// Summon minions or pets when used
        /// </summary>    
        public bool IsSummoningSkill
        {
            get { return _isSummoningSkill.GetValueOrDefault(); }
            set { _isSummoningSkill = value; }
        }

        /// <summary>
        /// Summon minions or pets when used
        /// </summary>    
        public bool IsKnockbackSkill
        {
            get { return _isKnockbackSkill.GetValueOrDefault(); }
            set { _isKnockbackSkill = value; }
        }

        /// <summary>
        /// Applies positive effects to player or allies when used
        /// </summary>        
        public bool IsBuffingSkill
        {
            get { return _isBuffingSkill.GetValueOrDefault(); }
            set { _isBuffingSkill = value; }
        }

        /// <summary>
        /// Applies negative effects to enemies when used
        /// </summary>        
        public bool IsDebuffingSkill
        {
            get { return _isDebuffingSkill.GetValueOrDefault(); }
            set { _isDebuffingSkill = value; }
        }

        /// <summary>
        /// Applies negative effects to enemies when used
        /// </summary>        
        public bool IsAreaEffectSkill
        {
            get { return _isAreaEffectSkill.GetValueOrDefault(); }
            set { _isAreaEffectSkill = value; }
        }

        /// <summary>
        /// Can be used to attack things
        /// </summary>    
        public bool IsOffensiveSkill
        {
            get { return _isOffensiveSkill.GetValueOrDefault(); }
            set { _isOffensiveSkill = value; }
        }

        /// <summary>
        /// Can be used to avoid dying
        /// </summary>    
        public bool IsDefensiveSkill
        {
            get { return _isDefensiveSkill.GetValueOrDefault(); }
            set { _isDefensiveSkill = value; }
        }

        /// <summary>
        /// Can be used to destroy destructables
        /// </summary>
        public bool IsDestructableSkill
        {
            get { return _isDestructableSkill.GetValueOrDefault(); }
            set { _isDestructableSkill = value; }
        }

        /// <summary>
        /// How long wait (in Milliseconds) after using, before using again.
        /// </summary>
        public float AfterUseDelay
        {
            get { return _afterUseDelay.GetValueOrDefault(); }
            set { _afterUseDelay = value; }
        }

        /// <summary>
        /// The maximum distance an ememy can be from the player 
        /// and still receive the effect of the skill
        /// </summary>
        public float MaxTargetDistance
        {
            get { return _maxTargetDistance.GetValueOrDefault(); }
            set { _maxTargetDistance = value; }
        }

        /// <summary>
        /// How close the player must be to the target before casting spell
        /// </summary>
        public float CastRange
        {
            get { return _maxCastDistance.GetValueOrDefault(); }
            set { _maxCastDistance = value; }
        }

        /// <summary>
        /// The number of units required to be in target area
        /// </summary>
        public float RequiredCluster
        {
            get { return _requiredCluster.GetValueOrDefault(); }
            set { _requiredCluster = value; }
        }

        /// <summary>
        /// How much resource is required before use. Defaults to Spell cost.
        /// </summary>
        public float RequiredResource
        {
            get { return _requiredResource.GetValueOrDefault(); }
            set { _requiredResource = value; }
        }

        /// <summary>
        /// The effects this skill causes on target
        /// </summary>
        public EffectTypeFlags TargetEffectFlags
        {
            get { return _targetEffectFlags.GetValueOrDefault(); }
            set { _targetEffectFlags = value; }
        }

        /// <summary>
        /// Shape of area damage
        /// </summary>
        public AreaEffectShape AreaEffectType
        {
            get { return _areaEffectType.GetValueOrDefault(); }
            set { _areaEffectType = value; }
        }

        /// <summary>
        /// Shape of area damage
        /// </summary>
        public CombatBase.CanCastFlags CastFlags
        {
            get { return _castFlags.GetValueOrDefault(); }
            set { _castFlags = value; }
        }

        public string DebugType
        {
            get
            {
                return
                    IsBuffingSkill ? "Buff" :
                    IsOffensiveSkill ? "Offensive" :
                    IsMovementSkill ? "Movement" :
                    IsAvoidanceSkill ? "Avoidance" :
                    IsDefensiveSkill ? "Defensive" : 
                    "Other";                
            }
        }

        public string DebugResourceEffect
        {
            get
            {
                return
                    Skill.IsGeneratorOrPrimary ? "Generator" :
                    Skill.IsAttackSpender ? "Spender" : 
                    "Other";                
            }
        }

        /// <summary>
        /// Special Condition to be evaluated for Attacking
        /// </summary>
        public Func<SkillMeta, bool> CastCondition;

        /// <summary>
        /// Special Condition to be evaluated for Attacking
        /// </summary>
        public Func<SkillMeta, Vector3> TargetSelector;

        /// <summary>
        /// Special action to apply rune based overrides
        /// </summary>
        public Action<Skill, SkillMeta> Overrides;

        #endregion

        #region Fields

        private CombatBase.CanCastFlags? _castFlags;
        private AreaEffectShape? _areaEffectType;
        private float? _requiredResource;
        private float? _requiredCluster;
        private float? _maxCastDistance;
        private float? _maxTargetDistance;
        private float? _afterUseDelay;
        private bool? _isDestructableSkill;
        private bool? _isDefensiveSkill;
        private bool? _isOffensiveSkill;
        private bool? _isDebuffingSkill;
        private bool? _isBuffingSkill;
        private bool? _isKnockbackSkill;
        private bool? _isSummoningSkill;
        private bool? _isMovementSkill;
        private bool? _isAvoidanceSkill;
        private bool? _isCastOnSelf;
        private bool? _isEliteOnly;
        private bool? _isCombatOnly;
        private EffectTypeFlags? _targetEffectFlags;
        private bool? _isAreaEffectSkill;

        #endregion

        #region Methods

        /// <summary>
        /// Change this instance using the set values of other instances
        /// </summary>
        public void Apply(IEnumerable<SkillMeta> others)
        {
            Apply(others.ToArray());
        }

        /// <summary>
        /// Change this instance using the set values of other instances
        /// </summary>
        public SkillMeta Apply(params SkillMeta[] others)
        {
            if (others == null)
                return this;

            foreach (var other in others)
            {
                Skill = other.Skill ?? Skill;
                IsCombatOnly = other._isCombatOnly ?? IsCombatOnly;
                IsEliteOnly = other._isEliteOnly ?? IsEliteOnly;
                IsCastOnSelf = other._isCastOnSelf ?? IsCastOnSelf;
                IsAvoidanceSkill = other._isAvoidanceSkill ?? IsAvoidanceSkill;
                IsMovementSkill = other._isMovementSkill ?? IsMovementSkill;
                IsSummoningSkill = other._isSummoningSkill ?? IsSummoningSkill;
                IsDestructableSkill = other._isDestructableSkill ?? IsDestructableSkill;
                IsOffensiveSkill = other._isOffensiveSkill ?? IsOffensiveSkill;
                IsDefensiveSkill = other._isDefensiveSkill ?? IsDefensiveSkill;
                IsBuffingSkill = other._isBuffingSkill ?? IsBuffingSkill;
                IsDebuffingSkill = other._isDebuffingSkill ?? IsDebuffingSkill;
                IsAreaEffectSkill = other._isAreaEffectSkill ?? IsAreaEffectSkill;
                AreaEffectType = other._areaEffectType ?? AreaEffectType;
                TargetEffectFlags = other._targetEffectFlags ?? TargetEffectFlags;
                MaxTargetDistance = other._maxTargetDistance ?? MaxTargetDistance;
                CastRange = other._maxCastDistance ?? CastRange;
                AfterUseDelay = other._afterUseDelay ?? AfterUseDelay;
                RequiredCluster = other._requiredCluster ?? RequiredCluster;
                RequiredResource = other._requiredResource ?? RequiredResource;
                CastCondition = other.CastCondition ?? CastCondition;
                CastCondition = other.CastCondition ?? CastCondition;
            }

            return this;
        }

        /// <summary>
        /// Copy the set values of this instance to another instance
        /// </summary>
        /// <param name="other"></param>
        public void ApplyTo(SkillMeta other)
        {
            other.Skill = Skill ?? other.Skill;
            other.IsCombatOnly = _isCombatOnly ?? other.IsCombatOnly;
            other.IsEliteOnly = _isEliteOnly ?? other.IsEliteOnly;
            other.IsCastOnSelf = _isCastOnSelf ?? other.IsCastOnSelf;
            other.IsAvoidanceSkill = _isAvoidanceSkill ?? other.IsAvoidanceSkill;
            other.IsMovementSkill = _isMovementSkill ?? other.IsMovementSkill;
            other.IsSummoningSkill = _isSummoningSkill ?? other.IsSummoningSkill;
            other.IsDestructableSkill = _isDestructableSkill ?? other.IsDestructableSkill;
            other.IsOffensiveSkill = _isOffensiveSkill ?? other.IsOffensiveSkill;
            other.IsDefensiveSkill = _isDefensiveSkill ?? other.IsDefensiveSkill;
            other.IsBuffingSkill = _isBuffingSkill ?? other.IsBuffingSkill;
            other.IsDebuffingSkill = _isDebuffingSkill ?? other.IsDebuffingSkill;
            other.IsAreaEffectSkill = _isAreaEffectSkill ?? other.IsAreaEffectSkill;
            other.AreaEffectType = _areaEffectType ?? other.AreaEffectType;
            other.TargetEffectFlags = _targetEffectFlags ?? other.TargetEffectFlags;
            other.MaxTargetDistance = _maxTargetDistance ?? other.MaxTargetDistance;
            other.CastRange = _maxCastDistance ?? other.CastRange;
            other.AfterUseDelay = _afterUseDelay ?? other.AfterUseDelay;
            other.RequiredCluster = _requiredCluster ?? other.RequiredCluster;
            other.RequiredResource = _requiredResource ?? other.RequiredResource;
            other.CastCondition = CastCondition ?? other.CastCondition;
        }

        public object Clone()
        {
            var clone = new SkillMeta(Skill);
            clone.Apply(this);
            return clone;
        }       

        #endregion

    };
}
