using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Remoting.Messaging;
using System.Text;
using System.Threading.Tasks;
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

        public bool IsPrimary
        {
            get { return Category == SpellCategory.Primary; }
        }

        public string Tooltip { get; set; }
        public string Slug { get; set; }
        public ActorClass Class { get; set; }

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
            get { return Skills.ActiveSkillsSNOPowers.Contains(SNOPower); }
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
        /// Gets the time in Millseconds since we've used the specified power
        /// </summary>
        public double TimeSincePowerUse()
        {
            return CombatBase.TimeSincePowerUse(SNOPower);
        }

        public override int GetHashCode()
        {
            return Index.GetHashCode() ^ Name.GetHashCode();
        }

    }


}
