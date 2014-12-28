using System;
using System.Collections.Generic;
using System.Linq;
using Trinity.Combat;
using Trinity.Objects;
using Zeta.Game;
using Zeta.Game.Internals.Actors;

namespace Trinity.Reference
{
    public class SkillUtils
    {       /// <summary>
        /// Fast lookup for a Skill by SNOPower
        /// </summary>
        public static Skill ById(SNOPower power)
        {
            if (!_allSkillBySnoPower.Any())
                _allSkillBySnoPower = _all.ToDictionary(s => s.SNOPower, s => s);
            Skill skill;
            var result = _allSkillBySnoPower.TryGetValue(power, out skill);
            return result ? skill : new Skill();
        }

        private static Dictionary<SNOPower, Skill> _allSkillBySnoPower = new Dictionary<SNOPower, Skill>();

        /// <summary>
        /// All SNOPowers
        /// </summary>        
        public static HashSet<SNOPower> AllIds
        {
            get { return _allSNOPowers ?? (_allSNOPowers = new HashSet<SNOPower>(All.Select(s => s.SNOPower))); }
        }

        private static HashSet<SNOPower> _allSNOPowers;

        /// <summary>
        /// All skills that are currently active
        /// </summary>
        public static List<Skill> Active
        {
            get
            {
                if (!_active.Any() || ShouldUpdateActiveSkills)
                    UpdateActiveSkills();

                return _active;
            }
        }

        private static List<Skill> _active = new List<Skill>();

        /// <summary>
        /// All skills that are currently active, as SNOPower
        /// </summary>
        public static HashSet<SNOPower> ActiveIds
        {
            get
            {
                if (!_activeIds.Any() || ShouldUpdateActiveSkills)
                    UpdateActiveSkills();

                return _activeIds;
            }
        }

        private static HashSet<SNOPower> _activeIds = new HashSet<SNOPower>();

        /// <summary>
        /// Refresh active skills collections with the latest data
        /// </summary>
        private static void UpdateActiveSkills()
        {
            _lastUpdatedActiveSkills = DateTime.UtcNow;
            _active = CurrentClass.Where(s => HotbarSkills.AssignedSNOPowers.Contains(s.SNOPower)).ToList();
            _activeIds = HotbarSkills.AssignedSNOPowers;
        }

        private static DateTime _lastUpdatedActiveSkills = DateTime.MinValue;

        /// <summary>
        /// Check time since last update of active skills
        /// </summary>
        private static bool ShouldUpdateActiveSkills
        {
            get { return DateTime.UtcNow.Subtract(_lastUpdatedActiveSkills) > TimeSpan.FromSeconds(3); }
        }

        /// <summary>
        /// All possible skills, as SNOPower
        /// </summary>        
        public static HashSet<SNOPower> CurrentClassIds
        {
            get { return new HashSet<SNOPower>(CurrentClass.Select(s => s.SNOPower)); }
        }

        /// <summary>
        /// All skills
        /// </summary>        
        public static List<Skill> All
        {
            get
            {
                if (!_all.Any())
                {
                    _all.AddRange(Skills.Barbarian.ToList());
                    _all.AddRange(Skills.WitchDoctor.ToList());
                    _all.AddRange(Skills.DemonHunter.ToList());
                    _all.AddRange(Skills.Wizard.ToList());
                    _all.AddRange(Skills.Crusader.ToList());
                    _all.AddRange(Skills.Monk.ToList());
                }
                return _all;
            }
        }
        private static List<Skill> _all = new List<Skill>();

        /// <summary>
        /// All skills for the specified class
        /// </summary>
        public static List<Skill> ByActorClass(ActorClass Class)
        {
            if (ZetaDia.Me.IsValid)
            {
                switch (ZetaDia.Me.ActorClass)
                {
                    case ActorClass.Barbarian:
                        return Skills.Barbarian.ToList();
                    case ActorClass.Crusader:
                        return Skills.Crusader.ToList();
                    case ActorClass.DemonHunter:
                        return Skills.DemonHunter.ToList();
                    case ActorClass.Monk:
                        return Skills.Monk.ToList();
                    case ActorClass.Witchdoctor:
                        return Skills.WitchDoctor.ToList();
                    case ActorClass.Wizard:
                        return Skills.Wizard.ToList();
                }
            }
            return new List<Skill>();
        }

        /// <summary>
        /// Skills for the current class
        /// </summary>
        public static IEnumerable<Skill> CurrentClass
        {
            get { return ZetaDia.Me.IsValid ? ByActorClass(ZetaDia.Me.ActorClass) : new List<Skill>(); }
        }

    }
}
