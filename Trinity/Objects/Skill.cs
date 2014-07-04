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
        public List<Rune> Runes { get; set; }
        public SNOPower SNOPower { get; set; }
        public string Name { get; set; }
        public SpellCategory Category { get; set; }
        public string Description { get; set; }
        public int RequiredLevel { get; set; }
        public int Index { get; set; }
        public string Tooltip { get; set; }
        public string Slug { get; set; }
        public ActorClass Class { get; set; }
        public Resource Resource { get; set; }
        public bool IsPrimary { get; set; }

        private int _cost;
        public int Cost
        {
            get { return CurrentRune.ModifiedCost.HasValue ? CurrentRune.ModifiedCost.Value : _cost; }
            set { _cost = value; }
        }

        private TimeSpan _duration;
        public TimeSpan Duration
        {
            get { return CurrentRune.ModifiedDuration.HasValue ? CurrentRune.ModifiedDuration.Value : _duration; }
            set { _duration = value; }
        }

        private TimeSpan _cooldown;
        public TimeSpan Cooldown
        {
            get { return CurrentRune.ModifiedCooldown.HasValue ? CurrentRune.ModifiedCooldown.Value : _cooldown; }
            set { _cooldown = value;  }
        }

        private Element _element;
        public Element Element
        {
            get { return CurrentRune.ModifiedElement.HasValue ? CurrentRune.ModifiedElement.Value : _element; }
            set { _element = value; }
        }

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

        public bool IsActive
        {
            get { return Skills.ActiveIds.Contains(SNOPower); }
        }

        public Rune CurrentRune
        {
            get
            {
                var rune =  Runes.FirstOrDefault(r => r.IsActive);
                return rune ?? new Rune();
            }
        }

        public bool IsAllRuneBonusActive
        {
            get
            {
                Set set;
                return DataDictionary.AllRuneSetsBySkill.TryGetValue(this, out set) && set.IsMaxBonusActive;
            }
        }

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

        public void TrackOnUnit(TrinityCacheObject unit)
        {
            SpellTracker.TrackSpellOnUnit(unit.ACDGuid, SNOPower);
        }

        /// <summary>
        /// Gets the time in Millseconds since we've used the specified power
        /// </summary>
        public double TimeSinceUse()
        {
            return CombatBase.TimeSincePowerUse(SNOPower);
        }

        public override int GetHashCode()
        {
            return Index.GetHashCode() ^ Name.GetHashCode();
        }

    }


}
