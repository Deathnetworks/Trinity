using System;
using System.Diagnostics;

namespace Trinity.Technicals
{
    [DebuggerStepThrough]
    public class PerformanceLogger : IDisposable
    {
        private static readonly log4net.ILog Logging = Zeta.Common.Logger.GetLoggerInstanceForType();
        private readonly string _BlockName;
        private readonly Stopwatch _Stopwatch;
        private bool _IsDisposed;

        public PerformanceLogger(string blockName)
        {
            _BlockName = blockName;
            if (Trinity.Settings.Advanced.LogCategories.HasFlag(LogCategory.Performance))
            {
                _Stopwatch = new Stopwatch();
                _Stopwatch.Start();
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (!_IsDisposed)
            {
                _IsDisposed = true;
                if (Trinity.Settings.Advanced.LogCategories.HasFlag(LogCategory.Performance))
                {
                    _Stopwatch.Stop();
                    if (_Stopwatch.Elapsed.TotalMilliseconds > 100)
                    {
                        Logging.DebugFormat("[Trinity][Performance] Execution of the block {0} took {1:00.00}ms.", _BlockName,
                                            _Stopwatch.Elapsed.TotalMilliseconds);
                    }
                }
                GC.SuppressFinalize(this);
            }
        }

        #endregion

        ~PerformanceLogger()
        {
            Dispose();
        }
    }
}
