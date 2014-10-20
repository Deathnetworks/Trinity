using System;
using System.Collections.Generic;
using System.Linq;
using Trinity.Technicals;
using Zeta.Game;
using Zeta.Game.Internals;
using Zeta.Game.Internals.Actors;

namespace Trinity.Combat
{
    public class HotbarSkills
    {
        private static HashSet<HotbarSkills> _assignedSkills = new HashSet<HotbarSkills>();
        private static HashSet<SNOPower> _passiveSkills = new HashSet<SNOPower>();
        private static Dictionary<SNOPower, HotbarSkills> _skillBySNOPower = new Dictionary<SNOPower, HotbarSkills>();
        private static Dictionary<HotbarSlot, HotbarSkills> _skillBySlot = new Dictionary<HotbarSlot, HotbarSkills>();
        private static HashSet<SNOPower> _assignedSNOPowers = new HashSet<SNOPower>();

        private static bool ShouldUpdate
        {
            get { return Trinity.HotbarRefreshTimer.Elapsed > TimeSpan.FromSeconds(10); }
        }

        /// <summary>
        /// Assigned SNOPowers as HashSet
        /// </summary>
        internal static HashSet<SNOPower> AssignedSNOPowers
        {
            get
            {
                if (!_assignedSNOPowers.Any() || ShouldUpdate)
                    Update();

                return _assignedSNOPowers;
            }
        }

        /// <summary>
        ///  Assigned skill by power
        /// </summary>
        internal static HotbarSkills BySNOPower(SNOPower power)
        {
            if (!_skillBySNOPower.Any() || ShouldUpdate)
                Update();

            HotbarSkills hbs;
            var result = _skillBySNOPower.TryGetValue(power, out hbs);
            return result ? hbs : new HotbarSkills() { RuneIndex = -999 };
        }

        /// <summary>
        ///  Assigned skill by slot
        /// </summary>
        internal static HotbarSkills BySlot(HotbarSlot slot)
        {
            if (!_skillBySlot.Any() || ShouldUpdate)
                Update();

            HotbarSkills hbs;
            var result = _skillBySlot.TryGetValue(slot, out hbs);
            return result ? hbs : new HotbarSkills() { RuneIndex = -999 };
        }

        /// <summary>
        /// The currently assigned hotbar skills with runes and slots
        /// </summary>
        internal static HashSet<HotbarSkills> AssignedSkills
        {
            get
            {
                if (_assignedSkills == null)
                {
                    _assignedSkills = new HashSet<HotbarSkills>();
                }
                if (!_assignedSkills.Any())
                {
                    Update();
                }
                return _assignedSkills;
            }
            set
            {
                _assignedSkills = value;
            }
        }


        internal static HashSet<SNOPower> PassiveSkills
        {
            get
            {
                if (!_passiveSkills.Any() || ShouldUpdate)
                    Update();

                return _passiveSkills;
            }
            set
            {
                _passiveSkills = value;
            }
        }

        public HotbarSlot Slot { get; set; }
        public SNOPower Power { get; set; }
        public int RuneIndex { get; set; }

        public HotbarSkills()
        {

        }

        /// <summary>
        /// Updates AssignedSkills
        /// </summary>
        internal static void Update(TrinityLogLevel logLevel = TrinityLogLevel.Debug, LogCategory logCategory = LogCategory.CacheManagement)
        {
            //Logger.Log("Refreshing Hotbar {0} ms", Trinity.HotbarRefreshTimer.ElapsedMilliseconds);

            Trinity.Hotbar = new List<SNOPower>();

            CPlayer cPlayer = CPlayer;

            for (int i = 0; i <= 5; i++)
            {
                SNOPower power = cPlayer.GetPowerForSlot((HotbarSlot)i);
                Trinity.Hotbar.Add(power);

                if (!DataDictionary.LastUseAbilityTimeDefaults.ContainsKey(power))
                {
                    DataDictionary.LastUseAbilityTimeDefaults.Add(power, DateTime.MinValue);
                }
                if (!CacheData.AbilityLastUsed.ContainsKey(power))
                {
                    CacheData.AbilityLastUsed.Add(power, DateTime.MinValue);
                }
            }
            Trinity.ShouldRefreshHotbarAbilities = false;
            Trinity.HotbarRefreshTimer.Restart();

            var oldSkills = new HashSet<HotbarSkills>();
            foreach (var skill in _assignedSkills)
            {
                oldSkills.Add(skill);
            }

            // Get current Skills and Runes 
            _assignedSkills.Clear();

            foreach (SNOPower p in Trinity.Hotbar)
            {
                _assignedSkills.Add(new HotbarSkills
                {
                    Power = p,
                    Slot = GetHotbarSlotFromPower(p),
                    RuneIndex = GetRuneIndexFromPower(p, cPlayer)
                });
            }


            string skillList = "";

            _passiveSkills = new HashSet<SNOPower>(cPlayer.PassiveSkills);
            _assignedSNOPowers = new HashSet<SNOPower>(_assignedSkills.Select(v => v.Power));

            foreach (var skill in _assignedSkills)
            {
                skillList += " " + skill.Power + "/" + skill.RuneIndex + "/" + skill.Slot;
                
                if (!_skillBySNOPower.ContainsKey(skill.Power))
                    _skillBySNOPower.Add(skill.Power, skill);

                if (!_skillBySlot.ContainsKey(skill.Slot))
                {
                    _skillBySlot.Add(skill.Slot, skill);
                }
            }
            Logger.Log(logLevel, logCategory, " Hotbar Skills (Skill/RuneIndex/Slot): " + skillList);

        }


        /// <summary>
        /// Returns the slot index (0-5) for the given SNOPower, returns HotbarSlot.Invalid if not in hotbar
        /// </summary>
        /// <param name="power"></param>
        /// <returns></returns>
        private static HotbarSlot GetHotbarSlotFromPower(SNOPower power)
        {
            int powerIndex = Trinity.Hotbar.IndexOf(power);
            if (powerIndex != -1)
                return (HotbarSlot)powerIndex;

            return HotbarSlot.Invalid;
        }

        /// <summary>
        /// Returns the rune index for the given SNOPower. If rune can't be found returns -999
        /// </summary>
        /// <param name="power"></param>
        /// <returns></returns>
        private static int GetRuneIndexFromPower(SNOPower power, CPlayer cPlayer)
        {
            const int runeIndex = -999;
            HotbarSlot slot = GetHotbarSlotFromPower(power);

            if (slot != HotbarSlot.Invalid)
            {
                return cPlayer.GetRuneIndexForSlot(slot);
            }

            return runeIndex;
        }

        public static SNOPower GetPowerForSlot(HotbarSlot slot)
        {
            if (slot == HotbarSlot.Invalid)
                return SNOPower.None;

            return Trinity.Hotbar[(int)slot];
        }

        private static CPlayer CPlayer { get { return ZetaDia.CPlayer; } }

    }

}
