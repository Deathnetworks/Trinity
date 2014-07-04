using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
using Trinity;
using Trinity.Combat;
using Trinity.Combat.Abilities;
using Trinity.Helpers;
using Trinity.Reference;
using Zeta.Game;
using Zeta.Game.Internals;
using Zeta.Game.Internals.Actors;

namespace Trinity.Objects
{
    /// <summary>
    /// Contains information about a Skill
    /// </summary>
    public class Skill
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
        public bool IsPrimary { get; set; }

        /// <summary>
        /// How much this spell costs to cast; uses rune value when applicable.
        /// </summary>
        public int Cost
        {
            get { return CurrentRune.ModifiedCost.HasValue ? CurrentRune.ModifiedCost.Value : _cost; }
            set { _cost = value; }
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
        /// Base cooldown; uses rune value when applicable (does not take into account item cooldown reduction).
        /// </summary>
        public TimeSpan Cooldown
        {
            get { return CurrentRune.ModifiedCooldown.HasValue ? CurrentRune.ModifiedCooldown.Value : _cooldown; }
            set { _cooldown = value;  }
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
            get { return Skills.ActiveIds.Contains(SNOPower); }
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
                if (ZetaDia.IsInGame && ZetaDia.Me.IsValid && ZetaDia.CPlayer.IsValid && IsActive)
                {
                    return ZetaDia.CPlayer.GetActiveSkillBySlot(HotbarSkills.BySNOPower(SNOPower).Slot).HasRuneEquipped;
                }
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
            return SpellTracker.IsUnitTracked(unit, SNOPower);
        }

        /// <summary>
        /// Record this skill as being on the specified unit; for the specified time.
        /// </summary>
        public void TrackOnUnit(TrinityCacheObject unit, float duration = 0f)
        {
            SpellTracker.TrackSpellOnUnit(unit.ACDGuid, SNOPower, duration);
        }

        /// <summary>
        /// Gets the time in Millseconds since we've used the specified power
        /// </summary>
        public double TimeSinceUse()
        {
            return CombatBase.TimeSincePowerUse(SNOPower);
        }

        /// <summary>
        /// Unique Identifier so that dictionarys can compare Skill objects.
        /// </summary>        
        public override int GetHashCode()
        {
            return Index.GetHashCode() ^ Name.GetHashCode();
        }

    }


}
