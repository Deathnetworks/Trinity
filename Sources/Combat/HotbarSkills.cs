using System.Collections.Generic;
using Trinity.Technicals;
using Zeta.Game;
using Zeta.Game.Internals;
using Zeta.Game.Internals.Actors;
namespace Trinity
{
    public class HotbarSkills
    {
        private static HashSet<HotbarSkills> _assignedSkills = new HashSet<HotbarSkills>();
        private static SNOPower[] _hotbarSlots = new SNOPower[6];

        public static SNOPower[] HotbarSlots
        {
            get { return HotbarSkills._hotbarSlots; }
            set { HotbarSkills._hotbarSlots = value; }
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
                    Update();
                }
                return _assignedSkills;
            }
            set
            {
                _assignedSkills = value;
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
        public static void Update()
        {
            _assignedSkills.Clear();
            UpdateHotbarSlotPowers();
            foreach (SNOPower p in Trinity.Hotbar)
            {
                _assignedSkills.Add(new HotbarSkills()
                {
                    Power = p,
                    Slot = HotbarSkills.GetHotbarSlotFromPower(p),
                    RuneIndex = HotbarSkills.GetRuneIndexFromPower(p)
                });
            }

            string skillList = "";
            foreach (HotbarSkills skill in HotbarSkills.AssignedSkills)
            {
                skillList += " " + skill.Power.ToString() + "/" + skill.RuneIndex + "/" + skill.Slot;
            }
            Logger.Log(TrinityLogLevel.Debug, LogCategory.CacheManagement, " Hotbar Skills (Skill/RuneIndex/Slot): " + skillList);
        }


        /// <summary>
        /// Returns the slot index (0-5) for the given SNOPower, returns HotbarSlot.Invalid if not in hotbar
        /// </summary>
        /// <param name="power"></param>
        /// <returns></returns>
        private static HotbarSlot GetHotbarSlotFromPower(SNOPower power)
        {
            if (Trinity.Hotbar.Contains(power))
            {
                for (int i = 0; i < 6; i++)
                {
                    //if (cPlayer.GetPowerForSlot((HotbarSlot)i) == power)
                    if (_hotbarSlots[i] == power)
                    {
                        return (HotbarSlot)i;
                    }
                }
            }
            return HotbarSlot.Invalid;
        }

        /// <summary>
        /// Returns the rune index for the given SNOPower. If rune can't be found returns -999
        /// </summary>
        /// <param name="power"></param>
        /// <returns></returns>
        private static int GetRuneIndexFromPower(SNOPower power)
        {
            int runeIndex = -999;
            HotbarSlot slot = GetHotbarSlotFromPower(power);

            if (slot != HotbarSlot.Invalid)
            {
                return cPlayer.GetRuneIndexForSlot(slot);
            }

            return runeIndex;
        }

        private static void UpdateHotbarSlotPowers()
        {
            for (int i = 0; i < 6; i++)
            {
                _hotbarSlots[i] = cPlayer.GetPowerForSlot((HotbarSlot)i);
            }
        }

        public static SNOPower GetPowerForSlot(HotbarSlot slot)
        {
            if (slot == HotbarSlot.Invalid)
                return SNOPower.None;

            return _hotbarSlots[(int)slot];
        }

        private static CPlayer cPlayer { get { return ZetaDia.CPlayer; } }

    }

}
