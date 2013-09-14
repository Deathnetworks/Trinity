using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Trinity.Technicals;
using Zeta;
using Zeta.Common;
using Zeta.Internals.Actors;

namespace Trinity
{
    /// <summary>
    /// Manage SNOPower delays and usages.
    /// </summary>
    internal static class TrinityPowerManager
    {
        private static readonly IDictionary<SNOPower, DateTime> _PowerLastUsage = new Dictionary<SNOPower, DateTime>();
        private static readonly IDictionary<SNOPower, int> _PowerDelay = new Dictionary<SNOPower, int>();
        private static readonly IList<ScheduledPower> _ScheduledPowers = new List<ScheduledPower>();
        private static Thread _ScheduleThread;


        public static bool HasEmpowered
        {
            get;
            set;
        }

        /// <summary>
        /// Loads delays from Trinity.AbilityRepeatDelay dictionary
        /// </summary>
        public static void LoadLegacyDelays()
        {
            foreach (TVar v in V.Data.Where(v => v.Key.StartsWith("SpellPower.")).Select(v => v.Value))
            {
                SNOPower p = (SNOPower)Enum.Parse(typeof(SNOPower), v.Name);

                DefineDelay(p, Convert.ToInt32(v.Value));
            }
        }

        /// <summary>Defines the delay of power.</summary>
        /// <param name="power">The power.</param>
        /// <param name="delay">The delay.</param>
        /// <exception cref="System.InvalidOperationException">Delay can't be negative</exception>
        public static void DefineDelay(SNOPower power, int delay)
        {
            if (delay < 0)
            {
                throw new InvalidOperationException("Delay can't be negative");
            }
            if (_PowerDelay.ContainsKey(power))
            {
                _PowerDelay[power] = delay;
            }
            else
            {
                _PowerDelay.Add(power, delay);
            }
        }

        /// <summary>
        /// Determines if Power can be used.
        /// </summary>
        /// <param name="power">The power to check.</param>
        /// <returns><c>true</c> if Power can be used; otherwise, <c>false</c>.</returns>
        public static bool CanUse(SNOPower power)
        {
            if (_PowerLastUsage.ContainsKey(power))
            {
                if (_PowerDelay.ContainsKey(power))
                {
                    return DateTime.UtcNow.Subtract(_PowerLastUsage[power]).TotalMilliseconds >= _PowerDelay[power] * (HasEmpowered?0.75:1) && Zeta.CommonBot.PowerManager.CanCast(power);
                }
                else
                {
                    Logger.Log(TrinityLogLevel.Debug, LogCategory.Behavior, "{0} don't have usage delay.", power);
                }
            }
            return Zeta.CommonBot.PowerManager.CanCast(power);
        }

        /// <summary>
        /// Uses the specified power.
        /// </summary>
        /// <param name="power">The power to use.</param>
        /// <param name="clickPos">The position to click.</param>
        /// <param name="worldDynamicId">The world dynamic id.</param>
        /// <param name="targetACDGuid">The target ACDGuid.</param>
        public static void Use(SNOPower power, Vector3 clickPos, int worldDynamicId = -1, int targetACDGuid = -1)
        {
            if (_PowerLastUsage.ContainsKey(power))
            {
                _PowerLastUsage[power] = DateTime.UtcNow;
            }
            else
            {
                _PowerLastUsage.Add(power, DateTime.UtcNow);
            }

            ZetaDia.Me.UsePower(power, clickPos, worldDynamicId, targetACDGuid);
        }

        /// <summary>
        /// Schedules the power.
        /// </summary>
        /// <param name="power">The power to use.</param>
        /// <param name="delay">The delay between 2 usages.</param>
        /// <param name="condition">The condition call before use power.</param>
        public static void Schedule(SNOPower power, int delay, Func<SNOPower, bool> condition)
        {
            lock (_ScheduledPowers)
            {
                if (_ScheduledPowers.Count < 1)
                {
                    _ScheduleThread = new Thread(MonitorPower);
                    _ScheduleThread.Priority = ThreadPriority.BelowNormal;
                    _ScheduleThread.Start();
                }
                else if (_ScheduledPowers.Any(sp => sp.Power == power))
                {
                    Logger.Log(TrinityLogLevel.Normal, LogCategory.Behavior, "You try to schedule 2 times same Power.");
                    return;
                }
                _ScheduledPowers.Add(new ScheduledPower()
                                        {
                                            Power = power,
                                            Delay = delay,
                                            LastUsage = DateTime.MinValue,
                                            Condition = condition
                                        });
            }
        }

        /// <summary>
        /// Unschedules the specified power.
        /// </summary>
        /// <param name="power">The power must be remove from schedule table.</param>
        public static void Unschedule(SNOPower power)
        {
            lock (_ScheduledPowers)
            {
                _ScheduledPowers.Remove(new ScheduledPower() { Power = power });

                if (_ScheduledPowers.Count < 1)
                {
                    _ScheduleThread.Abort();
                    _ScheduleThread = null;
                }
            }
        }

        private static void MonitorPower(object obj)
        {
            while (true)
            {
                lock (_ScheduledPowers)
                {
                    foreach (ScheduledPower power in _ScheduledPowers.Where(sp => DateTime.UtcNow.Subtract(sp.LastUsage).TotalMilliseconds >= sp.Delay && CanUse(sp.Power)))
                    {
                        if (power.Condition(power.Power))
                        {
                            power.LastUsage = DateTime.UtcNow;
                            Use(power.Power, Vector3.Zero);
                        }
                    }
                }
                Thread.Sleep(250);
            }
        }
    }
}
