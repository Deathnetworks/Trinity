using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace Trinity.Technicals
{
    [DebuggerStepThrough]
    public class MemorySpy : IDisposable
    {
        private readonly int _IndexOf;
        private readonly string _BlockName;
        private bool _IsDisposed;

        public MemorySpy(string blockName)
        {
            if (Trinity.LogHasFlagPerformance)
            {
                _BlockName = blockName;

                if (!Memory.IndexOf.TryGetValue(_BlockName, out _IndexOf))
                {
                    _IndexOf = Memory.LastIndex;
                    Memory.LastIndex++;

                    Memory.TaskTimers[_IndexOf] = new TaskTimer(_BlockName);
                    try { Memory.IndexOf.Add(_BlockName, _IndexOf); }
                    catch { }
                }

                Memory.TaskTimers[_IndexOf].Timer.Start();
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (!_IsDisposed)
            {
                if (Trinity.LogHasFlagPerformance)
                {
                    _IsDisposed = true;
                    Memory.TaskTimers[_IndexOf].Timer.Stop();
                    GC.SuppressFinalize(this); 
                }
            }
        }

        #endregion

        ~MemorySpy()
        {
            Dispose();
        }
    }

    public class Memory
    {
        public static int LastIndex = 0;
        public static Dictionary<string, int> IndexOf = new Dictionary<string, int>();
        public static TaskTimer[] TaskTimers = new TaskTimer[500];

        public static void ClearSpy()
        {
            LastIndex = 0;
            IndexOf.Clear();
            TaskTimers = new TaskTimer[500];
        }

        public static void LogSpy()
        {
            if (Trinity.LogHasFlagPerformance)
            {
                string _log = string.Empty;
                HashSet<TaskTimer> _list = new HashSet<TaskTimer>();

                for (int t = 1; t < TaskTimers.Length; t++)
                {
                    if (TaskTimers[t] != null && TaskTimers[t].Timer.Elapsed.TotalMilliseconds > 1)
                        _list.Add(TaskTimers[t]);
                }

                if (_list.Any())
                {
                    foreach (var _t in _list.OrderByDescending(t => t.Timer.Elapsed.TotalMilliseconds))
                    {
                        if (string.IsNullOrEmpty(_log) && _t.Timer.Elapsed.TotalMilliseconds < 5)
                        {
                            ClearSpy();
                            return;
                        }

                        _log += string.Format("{0}={1:00.00}ms ", _t.BlockName, _t.Timer.Elapsed.TotalMilliseconds);
                    }

                    if (!string.IsNullOrEmpty(_log))
                    {
                        Logger.Log(TrinityLogLevel.Info, LogCategory.Performance, "Tasks timed :");
                        Logger.Log(TrinityLogLevel.Info, LogCategory.Performance, _log);
                    }
                }
            }

            ClearSpy();
        }
    }

    public class TaskTimer : IEquatable<TaskTimer>
    {
        public string BlockName { get; set; }
        public Stopwatch Timer { get; set; }

        public TaskTimer(string _blockName)
        {
            BlockName = _blockName;
            Timer = new Stopwatch();
        }

        public bool Equals(TaskTimer other)
        {
            return BlockName == other.BlockName;
        }
    }
}

/* [Trinity][UnknownObjects] 
 * RefreshDiaObjects().Loop=33,20ms 
 * CacheDiaObject().CheckName=02,55ms 
 * CacheDiaObject().GetInfosInit=02,94ms 
 * CacheDiaObject().StepObjectType=01,03ms 
 * CacheDiaObject().StepPlayerSummons=06,92ms 
 * CacheDiaObject().StepCheckBlacklists=00,91ms 
 * CacheDiaObject().GetComplex=04,27ms 
 * CacheDiaObject().StepMainObjectType=06,70ms 
 * StepMainObjectType().Unit=05,53ms 
 * RefreshDiaObjects().Markers=02,85ms 
 * RefreshDiaObjects().Weight=02,62ms 
 * RefreshDiaObjects().Grid=25,15ms 
 * RefreshUnit().CheckAffix=03,68ms 
 * RefreshUnit().RefreshAffixes().Check=03,68ms 
 * CacheDiaObject().LoSCheck=01,47ms 
 * HandleTarget()=29,93ms 
 * RefreshDiaObjects()=13,56ms 
 */