using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using Zeta.Common;

namespace GilesTrinity.DbProvider
{
    /// <summary>
    /// Utilities help developer interact with DemonBuddy
    /// </summary>
    internal static class DbHelper
    {
        /// <summary>Logs the specified level.</summary>
        /// <param name="level">The logging level.</param>
        /// <param name="formatMessage">The format message.</param>
        /// <param name="args">The parameters used when format message.</param>
        public static void Log(TrinityLogLevel level, string formatMessage, params object[] args)
        {
            string msg = "[Trinity]: " + formatMessage;
            if (level == TrinityLogLevel.Critical)
            {
                Logging.Write(ConvertToLogLevel(level), Colors.Red, msg, args);
            }
            else
            {
                Logging.Write(ConvertToLogLevel(level), msg, args);
            }
        }

        /// <summary>Logs the specified level.</summary>
        /// <param name="formatMessage">The format message.</param>
        /// <param name="args">The parameters used when format message.</param>
        public static void Log(string formatMessage, params object[] args)
        {
            Log(TrinityLogLevel.Debug, formatMessage, args);
        }

        /// <summary>Converts <see cref="TrinityLogLevel"/> to <see cref="LogLevel"/>.</summary>
        /// <param name="level">The trinity logging level.</param>
        /// <returns>DemonBuddy logging level.</returns>
        private static LogLevel ConvertToLogLevel(TrinityLogLevel level)
        {
            LogLevel logLevel = LogLevel.Diagnostic;
            switch (level)
            {
                case TrinityLogLevel.Critical:
                    logLevel = LogLevel.None;
                    break;
                case TrinityLogLevel.Error:
                    logLevel = LogLevel.Quiet;
                    break;
                case TrinityLogLevel.Normal:
                    logLevel = LogLevel.Normal;
                    break;
                case TrinityLogLevel.LootRules:
                case TrinityLogLevel.Verbose:
                    logLevel = LogLevel.Verbose;
                    break;
                case TrinityLogLevel.Debug:
                    logLevel = LogLevel.Diagnostic;
                    break;
            }
            return logLevel;
        }
    }
}
