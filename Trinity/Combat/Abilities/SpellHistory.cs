using System;
using System.Collections.Generic;
using System.Linq;
using Zeta.Common;
using Zeta.Game.Internals.Actors;

namespace Trinity.Combat.Abilities
{
    public static class SpellHistory
    {
        private const int SpellHistorySize = 1000;
        private static List<SpellHistoryItem> _historyQueue = new List<SpellHistoryItem>(SpellHistorySize * 2);

        internal static List<SpellHistoryItem> HistoryQueue
        {
            get { return _historyQueue; }
            set { _historyQueue = value; }
        }

        public static void RecordSpell(TrinityPower power)
        {
            if (_historyQueue.Count >= SpellHistorySize)
                _historyQueue.RemoveAt(_historyQueue.Count() - 1);
            _historyQueue.Add(new SpellHistoryItem
            {
                Power = power,
                UseTime = DateTime.UtcNow,
                MyPosition = Trinity.Player.Position,
                TargetPosition = power.TargetPosition
            });
            //Logger.Log("Recorded {0}", power);

            CacheData.AbilityLastUsed[power.SNOPower] = DateTime.UtcNow;
            Trinity.LastPowerUsed = power.SNOPower;
        }

        public static void RecordSpell(SNOPower power)
        {
            RecordSpell(new TrinityPower(power));
        }

        public static TrinityPower GetLastTrinityPower()
        {
            if (HistoryQueue.Any())
                return _historyQueue.OrderByDescending(i => i.UseTime).FirstOrDefault().Power;
            return new TrinityPower();
        }

        public static SNOPower GetLastSNOPower()
        {
            if (HistoryQueue.Any())
                return _historyQueue.OrderByDescending(i => i.UseTime).FirstOrDefault().Power.SNOPower;
            return SNOPower.None;
        }

        public static DateTime GetSpellLastused(SNOPower power)
        {
            DateTime lastUsed = DateTime.MinValue;
            CacheData.AbilityLastUsed.TryGetValue(power, out lastUsed);
            return lastUsed;
        }

        public static TimeSpan TimeSinceUse(SNOPower power)
        {
            DateTime lastUsed = GetSpellLastused(power);
            return DateTime.UtcNow.Subtract(lastUsed);
        }

        public static int SpellUseCountInTime(SNOPower power, TimeSpan time)
        {
            if (_historyQueue.Any(i => i.Power.SNOPower == power))
            {
                var spellCount = _historyQueue.Count(i => i.Power.SNOPower == power && i.TimeSinceUse <= time);
                //Logger.Log("Found {0}/{1} spells in {2} time for {3} power", spellCount, _historyQueue.Count(i => i.Power.SNOPower == power), time, power);
                return spellCount;
            }
            return 0;
        }

        public static bool HasUsedSpell(SNOPower power)
        {
            if (_historyQueue.Any() && _historyQueue.Any(i => i.Power.SNOPower == power))
                return true;
            return false;
        }

        public static Vector3 GetSpellLastTargetPosition(SNOPower power)
        {
            Vector3 lastUsed = Vector3.Zero;
            if (_historyQueue.Any(i => i.Power.SNOPower == power))
                lastUsed = _historyQueue.FirstOrDefault(i => i.Power.SNOPower == power).TargetPosition;
            return lastUsed;
        }

        public static Vector3 GetSpellLastMyPosition(SNOPower power)
        {
            Vector3 lastUsed = Vector3.Zero;
            if (_historyQueue.Any(i => i.Power.SNOPower == power))
                lastUsed = _historyQueue.FirstOrDefault(i => i.Power.SNOPower == power).MyPosition;
            return lastUsed;
        }

        public static float DistanceFromLastTarget(SNOPower power)
        {
            var lastUsed = GetSpellLastTargetPosition(power);
            return Trinity.Player.Position.Distance2D(lastUsed);
        }

        public static float DistanceFromLastUsePosition(SNOPower power)
        {
            var lastUsed = GetSpellLastMyPosition(power);
            return Trinity.Player.Position.Distance2D(lastUsed);
        }

    }
}
