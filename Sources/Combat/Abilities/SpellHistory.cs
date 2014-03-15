using System;
using System.Collections.Generic;
using System.Linq;
using Zeta.Game.Internals.Actors;
namespace Trinity.Combat.Abilities
{
    public static class SpellHistory
    {
        private const int m_SpellHistorySize = 1000;
        private static Queue<SpellHistoryItem> historyQueue = new Queue<SpellHistoryItem>(m_SpellHistorySize * 2);

        internal static Queue<SpellHistoryItem> HistoryQueue
        {
            get { return historyQueue; }
            set { historyQueue = value; }
        }

        public static void RecordSpell(TrinityPower power)
        {
            if (historyQueue.Count >= m_SpellHistorySize)
                historyQueue.Dequeue();
            historyQueue.Enqueue(new SpellHistoryItem()
            {
                Power = power,
                UseTime = DateTime.UtcNow
            });
        }

        public static TrinityPower GetLastTrinityPower()
        {
            if (HistoryQueue.Any())
                return historyQueue.OrderByDescending(i => i.UseTime).FirstOrDefault().Power;
            else
                return new TrinityPower();
        }

        public static SNOPower GetLastSNOPower()
        {
            if (HistoryQueue.Any())
                return historyQueue.OrderByDescending(i => i.UseTime).FirstOrDefault().Power.SNOPower;
            else
                return SNOPower.None;
        }

        public static DateTime GetSpellLastused(SNOPower power)
        {
            DateTime lastUsed = DateTime.MinValue;
            if (historyQueue.Any(i => i.Power.SNOPower == power))
                lastUsed = historyQueue.OrderByDescending(i => i.UseTime).FirstOrDefault().UseTime;
            return lastUsed;
        }

        public static TimeSpan TimeSinceUse(SNOPower power)
        {
            DateTime lastUsed = GetSpellLastused(power);
            return DateTime.UtcNow.Subtract(lastUsed);
        }

        public static int SpellUseCountInTime(SNOPower power, TimeSpan time)
        {
            if (historyQueue.Any(i => i.Power.SNOPower == power))
            {
                DateTime lookBack = DateTime.UtcNow.Subtract(time);

                var spellCount = historyQueue.Count(i => i.Power.SNOPower == power && i.UseTime >= lookBack);

                return spellCount;
            }
            return 0;
        }

        public static bool HasUsedSpell(SNOPower power)
        {
            if (historyQueue.Any() && historyQueue.Any(i => i.Power.SNOPower == power))
                return true;
            return false;
        }

    }
}
