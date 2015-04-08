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
            _Stopwatch = new Stopwatch();
            _Stopwatch.Start();
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (!_IsDisposed)
            {
                _IsDisposed = true;
                _Stopwatch.Stop();
                if (_Stopwatch.Elapsed.TotalMilliseconds > 5)
                {
                    if (_Stopwatch.Elapsed.TotalMilliseconds > 1000 ||
                        (Trinity.Settings.Advanced.LogCategories.HasFlag(LogCategory.Performance) && _Stopwatch.Elapsed.TotalMilliseconds > 25))
                    {
                        Logging.InfoFormat("[Trinity][Performance] Execution of {0} took {1:00.00}ms.", _BlockName,
                                            _Stopwatch.Elapsed.TotalMilliseconds);
                    }
                    else if (Trinity.Settings.Advanced.LogCategories.HasFlag(LogCategory.Performance))
                    {
                        Logging.DebugFormat("[Trinity][Performance] Execution of {0} took {1:00.00}ms.", _BlockName,
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
