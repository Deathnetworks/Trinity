using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Zeta;
using Zeta.Internals;
using Zeta.Internals.Actors;

namespace Trinity
{
    public class SpellTracker : IEquatable<SpellTracker>
    {
        public int ACDGuid { get; set; }
        public SNOPower Power { get; set; }
        public DateTime Expiration { get; set; }

        internal static HashSet<SpellTracker> TrackedUnits { get; set; }
        private static Thread MaintenanceThread;

        internal static void TrackSpellOnUnit(SpellTracker trackedUnit)
        {
            if (!TrackedUnits.Any(t => t.Equals(trackedUnit)))
            {
                TrackedUnits.Add(trackedUnit);
            }
        }

        internal static void TrackSpellOnUnit(int acdGuid, SNOPower power)
        {
            float duration = 0;

            if (CachedTrackedSpells.ContainsKey(power))
            {
                HotbarSkills skill = HotbarSkills.AssignedSkills.FirstOrDefault(p => p.Power == power);
                TrackedSpell spell = TrackedSpells.FirstOrDefault(s => s.Equals(new TrackedSpell(power, skill.RuneIndex)));
                if (spell != null)
                    duration = spell.Duration;
            }

            if (duration > 0)
            {
                //Technicals.Logger.LogNormal("Tracking unit {0} with power {1} for duration {2:0.00}", acdGuid, power, duration);
                TrackSpellOnUnit(new SpellTracker()
                   {
                       ACDGuid = acdGuid,
                       Power = power,
                       Expiration = DateTime.Now.AddMilliseconds(duration)
                   });
            }
        }

        /// <summary>
        /// Checks if a unit is currently being tracked with a given SNOPower. When the spell is properly configured, this can be used to set a "timer" on a DoT re-cast, for example.
        /// </summary>
        /// <param name="acdGuid"></param>
        /// <param name="power"></param>
        /// <returns></returns>
        public static bool IsUnitTracked(int acdGuid, SNOPower power)
        {
            return TrackedUnits.Any(t => t.ACDGuid == acdGuid && t.Power == power);
        }

        public static bool IsUnitTracked(TrinityCacheObject unit, SNOPower power)
        {
            if (unit.Type != GObjectType.Unit)
                return false;
            bool result = TrackedUnits.Any(t => t.ACDGuid == unit.ACDGuid && t.Power == power);
            //if (result)
            //    Technicals.Logger.LogNormal("Unit {0} is tracked with power {1}", unit.ACDGuid, power);
            //else
            //    Technicals.Logger.LogNormal("Unit {0} is NOT tracked with power {1}", unit.ACDGuid, power);
            return result;
        }

        #region Static Constructor
        static SpellTracker()
        {
            TrackedUnits = new HashSet<SpellTracker>();
            PopulateTrackedSpells();

            MaintenanceThread = new Thread(RunMaintenance);
            MaintenanceThread.IsBackground = true;
            MaintenanceThread.Priority = ThreadPriority.Lowest;
            MaintenanceThread.Start();

        }
        #endregion

        #region Background Maintenance Thread
        private static void RunMaintenance()
        {
            while (true)
            {
                if (TrackedUnits.Any())
                {
                    lock (TrackedUnits)
                    {
                        //TrackedUnits.RemoveWhere(t => t.Expiration < DateTime.Now);
                        var units = TrackedUnits.Where(t => t.Expiration < DateTime.Now);
                        foreach (var unit in units.ToList())
                        {
                            //Technicals.Logger.LogNormal("Removing unit {0} from TrackedUnits ({1}, {2})", unit.ACDGuid, unit.Expiration.Ticks, DateTime.Now.Ticks);
                            TrackedUnits.Remove(unit);
                        }
                    }
                }
                Thread.Sleep(100);
            }
        }
        #endregion

        #region IEquatable Implimentation
        public bool Equals(SpellTracker other)
        {
            return this.ACDGuid == other.ACDGuid && this.Power == other.Power;
        }
        #endregion

        private static Dictionary<SNOPower, int> CachedTrackedSpells = new Dictionary<SNOPower, int>();

        /// <summary>
        /// Updates the cached tracked spells. Call only after updating HotbarSkills.
        /// </summary>
        internal static void RefreshCachedSpells()
        {
            CachedTrackedSpells.Clear();
            foreach (HotbarSkills skill in HotbarSkills.AssignedSkills)
            {
                if (TrackedSpells.Any(s => s.Power == skill.Power && (s.RuneIndex == skill.RuneIndex || s.RuneIndex == -999)))
                {
                    CachedTrackedSpells.Add(skill.Power, skill.RuneIndex);
                }
            }
        }

        private static HashSet<TrackedSpell> TrackedSpells = new HashSet<TrackedSpell>();

        /// <summary>
        /// Populates the static TrackedSpells Hashset. Called once from Static Constructor. Do not call anywhere else...
        /// </summary>
        private static void PopulateTrackedSpells()
        {
            // new TrackedSpell(SNOPower, RuneIndex, MillsecondsDuration)
            // Use RuneIndex = -999 for "default" or any rune index

            TrackedSpells.Clear();

            // Barbarian 
            // TBD, maybe Rend is a good candidate?

            // Monk
            // TBD

            // Wizard
            // TBD

            // Witch Doctor
            TrackedSpells.Add(new TrackedSpell(SNOPower.Witchdoctor_Haunt, 0, 6000f));
            TrackedSpells.Add(new TrackedSpell(SNOPower.Witchdoctor_Haunt, 1, 6000f));
            TrackedSpells.Add(new TrackedSpell(SNOPower.Witchdoctor_Haunt, 2, 6000f));
            TrackedSpells.Add(new TrackedSpell(SNOPower.Witchdoctor_Haunt, 3, 6000f));
            TrackedSpells.Add(new TrackedSpell(SNOPower.Witchdoctor_Haunt, 4, 2000f)); // WD, Resentful Spirit

            TrackedSpells.Add(new TrackedSpell(SNOPower.Witchdoctor_Locust_Swarm, -999, 8000f));

            // Demon Hunter
            // TBD
        }


        public class TrackedSpell : IEquatable<TrackedSpell>
        {
            /// <summary>
            /// The SNO Power
            /// </summary>
            public SNOPower Power { get; set; }
            /// <summary>
            /// The rune index
            /// </summary>
            public int RuneIndex { get; set; }
            /// <summary>
            /// Duration of Tracked Spell in Millseconds
            /// </summary>
            public float Duration { get; set; }

            public TrackedSpell(SNOPower power, int runeIndex, float duration)
            {
                this.Power = power;
                this.RuneIndex = runeIndex;
                this.Duration = duration;
            }

            public TrackedSpell(SNOPower power, int runeIndex)
            {
                this.Power = power;
                this.RuneIndex = runeIndex;
            }

            public bool Equals(TrackedSpell other)
            {
                return this.Power == other.Power && this.RuneIndex == other.RuneIndex;
            }
        }
    }

}
