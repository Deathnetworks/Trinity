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
                if (_passiveSkills == null)
                {
                    _passiveSkills = new HashSet<SNOPower>();
                    Update();
                }
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
            Trinity.Hotbar = new List<SNOPower>();
            
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
           
            HashSet<HotbarSkills> oldSkills = new HashSet<HotbarSkills>();
            foreach (var skill in _assignedSkills)
            {
                oldSkills.Add(skill);
            }

            if (Trinity.Hotbar.Any(hb => oldSkills.All(old => old.Power != hb)))
            {
                _assignedSkills.Clear();

                foreach (SNOPower p in Trinity.Hotbar)
                {
                    _assignedSkills.Add(new HotbarSkills
                    {
                        Power = p,
                        Slot = GetHotbarSlotFromPower(p),
                        RuneIndex = GetRuneIndexFromPower(p)
                    });
                }
            }

            string skillList = "";
            foreach (HotbarSkills skill in AssignedSkills)
            {
                skillList += " " + skill.Power + "/" + skill.RuneIndex + "/" + skill.Slot;
            }
            Logger.Log(logLevel, logCategory, " Hotbar Skills (Skill/RuneIndex/Slot): " + skillList);

            PassiveSkills = new HashSet<SNOPower>(cPlayer.PassiveSkills);
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
        private static int GetRuneIndexFromPower(SNOPower power)
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

        private static CPlayer cPlayer { get { return ZetaDia.CPlayer; } }

    }

}
