using System;
using System.Collections.Generic;
using System.Linq;
using Trinity.Combat;
using Trinity.Combat.Abilities;
using Trinity.Reference;
using Zeta.Common;
using Zeta.Game;
using Zeta.Game.Internals.Actors;

namespace Trinity.Objects
{
    /// <summary>
    /// Contains information about a Skill
    /// </summary>
    public class Skill : IUnique
    {
        private int _cost;
        private TimeSpan _duration;
        private TimeSpan _cooldown;
        private Element _element;

        public Skill()
        {
            Runes = new List<Rune>();
            Index = 0;
            SNOPower = SNOPower.None;
            Name = string.Empty;
            Category = SpellCategory.Unknown;
            Description = string.Empty;
            RequiredLevel = 0;
            Tooltip = string.Empty;
            Slug = string.Empty;
            Class = ActorClass.Invalid;
        }

        public string Name { get; set; }
        public string Description { get; set; }
        public string Slug { get; set; }

        /// <summary>
        /// The runes that may be selected for this skill
        /// </summary>
        public List<Rune> Runes { get; set; }

        /// <summary>
        /// DBs internal enum value for this skill
        /// </summary>
        public SNOPower SNOPower { get; set; }

        /// <summary>
        /// Name of the group of skills this belongs to as listed in d3 skill selection menu
        /// Ie. Barbarian Primary, Might, Tactics or Rage skill groups.
        /// </summary>
        public SpellCategory Category { get; set; }

        /// <summary>
        /// The level required before this skill may be selected in diablo3
        /// </summary>
        public int RequiredLevel { get; set; }

        /// <summary>
        /// Zero-based index for this skill within the list of skills for this class
        /// Maps to element position in Skills.[class name] 
        /// </summary>
        public int Index { get; set; }

        /// <summary>
        /// Code for mapping this skill to a tooltip using http://us.battle.net/d3/en/tooltip/         
        /// </summary>
        public string Tooltip { get; set; }

        /// <summary>
        /// Class this skill is used by (barbarian/wizard etc).
        /// </summary>
        public ActorClass Class { get; set; }

        /// <summary>
        /// Resource type used by this skill - mana, hatred, discipline etc.
        /// </summary>
        public Resource Resource { get; set; }

        /// <summary>
        /// If this skill is a special primary skill (free to cast or generators)
        /// </summary>
        public bool IsPrimary
        {
            get { return Category == SpellCategory.Primary; }
            set { } // todo: find out why non-primary are being set as primary in JS Collector
        }

        /// <summary>
        /// How much this spell costs to cast; uses rune value when applicable.
        /// </summary>
        public int Cost
        {
            get { return CurrentRune.ModifiedCost.HasValue ? CurrentRune.ModifiedCost.Value : _cost; }
            set { _cost = value; }
        }

        /// <summary>
        /// Check if skill spend primary ressource
        /// </summary>
        public bool IsSpender
        {
            get
            {
                if (SNOPower == 0 || Resource == Resource.Discipline || Resource == Resource.Unknown)
                    return false;

                if (this == Skills.DemonHunter.Chakram && Legendary.SpinesOfSeethingHatred.IsEquipped)
                    return false;

                if (this == Skills.DemonHunter.ElementalArrow && Legendary.Kridershot.IsEquipped)
                    return false;

                return Cost > 0;
            }
        }

        /// <summary>
        /// Check if skill generates resource and can hit
        /// </summary>
        public bool IsAttackGenerator
        {
            get
            {
                if (this == Skills.DemonHunter.Chakram && Legendary.SpinesOfSeethingHatred.IsEquipped)
                    return true;

                if (this == Skills.DemonHunter.ElementalArrow && Legendary.Kridershot.IsEquipped)
                    return true;

                return Category == SpellCategory.Primary;
            }
        }

        /// <summary>
        /// Signature spells are free to cast (not classified generators)
        /// </summary>
        public bool IsSignature
        {
            get { return Category == SpellCategory.Primary && (Class == ActorClass.Witchdoctor || Class == ActorClass.Wizard); }           
        }

        /// <summary>
        /// How long this spell or its effect will last; uses rune value when applicable.
        /// </summary>
        public TimeSpan Duration
        {
            get { return CurrentRune.ModifiedDuration.HasValue ? CurrentRune.ModifiedDuration.Value : _duration; }
            set { _duration = value; }
        }

        /// <summary>
        /// Cooldown; uses rune value when applicable and cooldown reduction from items.
        /// </summary>
        public TimeSpan Cooldown
        {
            get
            {
                var baseCooldown = CurrentRune.ModifiedCooldown.HasValue ? CurrentRune.ModifiedCooldown.Value : _cooldown;
                var newCooldownMilliseconds = baseCooldown.TotalMilliseconds * (1 - Trinity.Player.CooldownReductionPct);
                var finalCooldown = Trinity.Player.CooldownReductionPct > 0 ? TimeSpan.FromMilliseconds(newCooldownMilliseconds) : baseCooldown;
                return finalCooldown;
            }
            set { _cooldown = value;  }
        }

        /// <summary>
        /// Milliseconds until spell is off cooldown
        /// </summary>
        public int CooldownRemaining
        {
            get
            {
                if (TimeSinceUse > 9999999) return 0;
                var castTime = DateTime.UtcNow.Subtract(TimeSpan.FromMilliseconds(TimeSinceUse));
                var endTime = castTime.Add(Cooldown);                
                var remainingMilliseconds = DateTime.UtcNow.Subtract(endTime).TotalMilliseconds;
                return remainingMilliseconds < 0 ? (int)remainingMilliseconds * -1 : 0;;
            }
        }


        /// <summary>
        /// Element for this skill (lightning/fire etc); uses rune value when applicable.
        /// </summary>
        public Element Element
        {
            get { return CurrentRune.ModifiedElement.HasValue ? CurrentRune.ModifiedElement.Value : _element; }
            set { _element = value; }
        }

        /// <summary>
        /// If this passive is currently selected in the Diablo3 skills menu (skill is on the hotbar).
        /// </summary>
        public bool IsActive
        {
            get { return SkillUtils.ActiveIds.Contains(SNOPower); }
        }

        /// <summary>
        /// If the skill's associated buff is currently active, ie, archon, warcry etc.
        /// </summary>
        public bool IsBuffActive
        {
            get { return CombatBase.GetHasBuff(SNOPower); }
        }

        /// <summary>
        /// Gets the current buff stack count
        /// </summary>
        public int BuffStacks
        {
            get { return CombatBase.GetBuffStacks(SNOPower); }
        }

        /// <summary>
        /// Gets the current skill charge count
        /// </summary>
        public int Charges
        {
            get { return CombatBase.GetSkillCharges(SNOPower); }
        }

        /// <summary>
        /// The currently selected rune for this skill.        
        /// </summary>
        public Rune CurrentRune
        {
            get
            {
                var rune =  Runes.FirstOrDefault(r => r.IsActive);
                return rune ?? new Rune();
            }
        }

        /// <summary>
        /// If all runes for this skill are currently enabled
        /// </summary>
        public bool IsAllRuneBonusActive
        {
            get
            {
                Set set;
                return DataDictionary.AllRuneSetsBySkill.TryGetValue(this, out set) && set.IsMaxBonusActive;
            }
        }

        /// <summary>
        /// If this skill has a rune equipped
        /// </summary>
        public bool HasRuneEquipped
        {
            get
            {
                if (ZetaDia.IsInGame && ZetaDia.Me.IsValid && IsActive)
                    return CacheData.Hotbar.GetSkill(SNOPower).Rune != null;

                return false;
            }
        }        

        /// <summary>
        /// Performs basic checks to see if we have and can cast a power (hotbar, power manager). Checks use timer for Wiz, DH, Monk
        /// </summary>
        public bool CanCast (CombatBase.CanCastFlags flags = CombatBase.CanCastFlags.All)
        {
            return CombatBase.CanCast(SNOPower, flags);
        }

        /// <summary>
        /// Checks if a unit is currently being tracked with a given SNOPower. When the spell is properly configured, this can be used to set a "timer" on a DoT re-cast, for example.
        /// </summary>
        public bool IsTrackedOnUnit(TrinityCacheObject unit)
        {
            return unit.HasDebuff(SNOPower);
        }

        /// <summary>
        /// Time since last used
        /// </summary>
        public double TimeSinceUse
        {
            get {  return DateTime.UtcNow.Subtract(LastUsed).TotalMilliseconds; }
        }

        /// <summary>
        /// When this spell was last used
        /// </summary>
        public DateTime LastUsed
        {
            get
            {
                DateTime dt;
                return CacheData.AbilityLastUsed.TryGetValue(SNOPower, out dt) ? dt : DateTime.MinValue;
            }
        }

        /// <summary>
        /// This skill as TrinityPower
        /// </summary>
        public TrinityPower ToPower(float minimumRange, Vector3 targetPosition)
        {
            return new TrinityPower(SNOPower, minimumRange, targetPosition);
        }

        /// <summary>
        /// This skill as TrinityPower
        /// </summary>
        public TrinityPower ToPower(float minimumRange)
        {
            return new TrinityPower(SNOPower, minimumRange);
        }

        /// <summary>
        /// This skill as TrinityPower
        /// </summary>
        public TrinityPower ToPower()
        {
            return new TrinityPower(SNOPower);
        }

        /// <summary>
        /// Cast this skill at the current position
        /// </summary>
        public bool Cast()
        {
            return Cast(Trinity.Player.Position, -1);
        }

        /// <summary>
        /// Cast this skill at the specified position
        /// </summary>
        public bool Cast(Vector3 position)
        {
            return Cast(position, -1);
        }

        /// <summary>
        /// Cast this skill at the specified target
        /// </summary>
        public bool Cast (TrinityCacheObject target)
        {
            return Cast(target.Position, target.ACDGuid);
        }

        /// <summary>
        /// Cast this speed using TrinityPower
        /// </summary>
        public bool Cast(TrinityPower power)
        {
            return Cast(power.TargetPosition, power.TargetACDGUID);
        }

        /// <summary>
        /// Cast this skill
        /// </summary>
        public bool Cast(Vector3 clickPosition, int targetAcdGuid)
        {
            if (targetAcdGuid != -1)
                return CombatBase.Cast(new TrinityPower(SNOPower, 0f, targetAcdGuid));

            return CombatBase.Cast(new TrinityPower(SNOPower, 0f, clickPosition));
        }

        private bool GameIsReady
        {
            get { return ZetaDia.IsInGame && ZetaDia.Me.IsValid && !ZetaDia.IsLoadingWorld && !ZetaDia.IsInTown && !ZetaDia.IsPlayingCutscene; }
        }

        /// <summary>
        /// Unique Identifier so that dictionarys can compare Skill objects.
        /// </summary>        
        public override int GetHashCode()
        {
            return Index.GetHashCode() ^ Name.GetHashCode();
        }

        public int Id
        {
            get { return (int)SNOPower; }
        }
    }
}


