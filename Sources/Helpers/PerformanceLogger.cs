using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Zeta.Common;

namespace Trinity.Technicals
{
    public class PerformanceLogger : IDisposable
    {
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
                    if (_Stopwatch.Elapsed.TotalMilliseconds > 1)
                    {
                        Logging.WriteVerbose("[Trinity][Performance] Execution of the block {0} took {1:00.00}ms.", _BlockName,
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
