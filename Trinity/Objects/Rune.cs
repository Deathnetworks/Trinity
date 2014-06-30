using System;
using System.Linq;
using System.Windows.Forms.VisualStyles;
using Trinity.Combat;
using Trinity.Reference;
using Zeta.Common;
using Zeta.Game;

namespace Trinity.Objects
{
    /// <summary>
    /// Contains information about a Rune
    /// </summary>
    public class Rune
    {
        public string Name { get; set; }
        public int Index { get; set; }
        public string Description { get; set; }
        public int RequiredLevel { get; set; }
        public string TypeId { get; set; }
        public string Tooltip { get; set; }
        public ActorClass Class { get; set; }
        public int RuneIndex { get; set; }

        /// <summary>
        /// Zero-Based index to the Skills object for this class
        /// </summary>
        public int SkillIndex { get; set; }

        public Rune()
        {
            Name = "None";
            Index = -1;
            Description = string.Empty;
            RequiredLevel = 0;
            TypeId = string.Empty;
            Tooltip = string.Empty;
            Class = ActorClass.Invalid;
            SkillIndex = -1;
        }

        public bool IsActive
        {
            get
            {
                if (ZetaDia.IsInGame && ZetaDia.Me.IsValid && Class == ZetaDia.Me.ActorClass && Skill != null && RuneIndex >= 0)
                {
                    return HotbarSkills.BySNOPower(Skill.SNOPower).RuneIndex == RuneIndex || Skill.IsAllRuneBonusActive;
                }
                return false;
            }
        }

        public override int GetHashCode()
        {
            return Index.GetHashCode() ^ Name.GetHashCode();
        }

        public Skill Skill
        {
            get { return Skills.ByActorClass(Class).ElementAtOrDefault(SkillIndex); } 
        }

    }
}
