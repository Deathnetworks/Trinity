using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Zeta.Common;

namespace GilesTrinity.Technicals
{
    public class PerformanceLogger : IDisposable
    {
        private readonly string _blockName;
        private readonly bool _isEnabled;
        private readonly Stopwatch _stopwatch;
        private bool _isDisposed;

        public PerformanceLogger(string blockName)
        {
            _blockName = blockName;
            if (GilesTrinity.Settings.Advanced.LogCategories.HasFlag(LogCategory.Performance))
            {
                _stopwatch = new Stopwatch();
                _stopwatch.Start();
            }
        }

        #region IDisposable Members

        public void Dispose()
        {
            if (!_isDisposed)
            {
                _isDisposed = true;
                if (GilesTrinity.Settings.Advanced.LogCategories.HasFlag(LogCategory.Performance))
                {
                    _stopwatch.Stop();
                    Logging.WriteVerbose("[Trinity][Performance] Execution of the block {0} took {1}ms.", _blockName,
                                        _stopwatch.ElapsedMilliseconds);
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
