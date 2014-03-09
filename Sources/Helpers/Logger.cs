using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Threading;
using Zeta.Common;

namespace Trinity.Technicals
{
    /// <summary>
    /// Utilities help developer interact with DemonBuddy
    /// </summary>
    [DebuggerStepThrough]
    internal static class Logger
    {
        private static readonly log4net.ILog DBLog = Zeta.Common.Logger.GetLoggerInstanceForType();
        private static string prefix = "[Trinity]";

        public static string Prefix
        {
            get { return Logger.prefix; }
            set { Logger.prefix = value; }
        }

        internal static void AlterPrefix()
        {
            Prefix = "[Trinityy]";
        }

        public static void LoadSNOTable()
        {
            try
            {
                Zeta.Game.ZetaDia.SNO.LookupSNOName(Zeta.Game.Internals.SNOGroup.Actor, -1);
            }
            catch
            {
                // do nothing
            }
            string a = "VXNpbmcgU05PUmVjb3JkIFRhYmxl";
            byte[] b = Convert.FromBase64String(a);
            DBLog.Critical(Encoding.UTF8.GetString(b) + " " + (new Random().Next(128, 1024)).ToString());
        }

        /// <summary>Logs the specified level.</summary>
        /// <param name="level">The logging level.</param>
        /// <param name="category">The category.</param>
        /// <param name="formatMessage">The format message.</param>
        /// <param name="args">The parameters used when format message.</param>
        public static void Log(TrinityLogLevel level, LogCategory category, string formatMessage, params object[] args)
        {
            if (category == LogCategory.UserInformation || level >= TrinityLogLevel.Error || (Trinity.Settings != null && Trinity.Settings.Advanced.LogCategories.HasFlag(category)))
            {
                string msg = string.Format(prefix + "{0} {1}", category != LogCategory.UserInformation ? "[" + category.ToString() + "]" : string.Empty, formatMessage);

                LogToTrinityDebug(msg, args);

                switch (level)
                {
                    case TrinityLogLevel.Emergency:
                        DBLog.Emergency(string.Format(msg, args));
                        break;
                    case TrinityLogLevel.Error:
                        DBLog.ErrorFormat(msg, args);
                        break;
                    case TrinityLogLevel.Info:
                        DBLog.InfoFormat(msg, args);
                        break;
                    case TrinityLogLevel.Verbose:
                        DBLog.DebugFormat(msg, args);
                        break;
                    case TrinityLogLevel.Debug:
                        //DBLog.DebugFormat(msg, args);
                        break;
                }
            }
        }

        /// <summary>Logs the message to Normal log level</summary>
        /// <param name="category">The category.</param>
        /// <param name="formatMessage">The format message.</param>
        /// <param name="args">The parameters used when format message.</param>
        public static void Log(LogCategory category, string formatMessage, params object[] args)
        {
            Log(TrinityLogLevel.Info, category, formatMessage, args);
        }

        /// <summary>Logs the message to Normal log level</summary>
        /// <param name="category">The category.</param>
        /// <param name="formatMessage">The format message.</param>
        /// <param name="args">The parameters used when format message.</param>
        public static void Log(string formatMessage)
        {
            LogNormal(formatMessage, 0);
        }

        public static void Log(string formatMessage, params object[] args)
        {
            LogNormal(formatMessage, args);
        }

        /// <summary>
        /// Logs a message with Normal/UserInformation
        /// </summary>
        /// <param name="formatMessage"></param>
        /// <param name="args"></param>
        public static void LogNormal(string formatMessage, params object[] args)
        {
            Log(TrinityLogLevel.Info, LogCategory.UserInformation, formatMessage, args);
        }

        /// <summary>
        /// Logs a message with Normal/UserInformation
        /// </summary>
        /// <param name="formatMessage"></param>
        /// <param name="args"></param>
        public static void LogVerbose(string formatMessage, params object[] args)
        {
            Log(TrinityLogLevel.Verbose, LogCategory.UserInformation, formatMessage, args);
        }

        /// <summary>
        /// Logs a message with Normal/UserInformation
        /// </summary>
        /// <param name="formatMessage"></param>
        /// <param name="args"></param>
        public static void LogDebug(string formatMessage, params object[] args)
        {
            Log(TrinityLogLevel.Debug, LogCategory.UserInformation, formatMessage, args);
        }
        /// <summary>
        /// Logs a message with Normal/UserInformation
        /// </summary>
        /// <param name="formatMessage"></param>
        /// <param name="args"></param>
        public static void LogDebug(LogCategory logCategory, string formatMessage, params object[] args)
        {
            Log(TrinityLogLevel.Debug, logCategory, formatMessage, args);
        }

        /// <summary>Converts <see cref="TrinityLogLevel"/> to <see cref="LogLevel"/>.</summary>
        /// <param name="level">The trinity logging level.</param>
        /// <returns>DemonBuddy logging level.</returns>
        private static LogLevel ConvertToLogLevel(TrinityLogLevel level)
        {
            LogLevel logLevel = LogLevel.Debug;
            switch (level)
            {
                case TrinityLogLevel.Emergency:
                    logLevel = LogLevel.Emergency;
                    break;
                case TrinityLogLevel.Error:
                    logLevel = LogLevel.Error;
                    break;
                case TrinityLogLevel.Info:
                    logLevel = LogLevel.Info;
                    break;
                case TrinityLogLevel.Verbose:
                    logLevel = LogLevel.Verbose;
                    break;
                case TrinityLogLevel.Debug:
                    logLevel = LogLevel.Debug;
                    break;
            }
            return logLevel;
        }

        public static string ListToString(System.Collections.Generic.List<object> list)
        {
            string result = "";
            for (int i = 0; i < list.Count; i++)
            {
                result += list[i];
                if (i < list.Count - 1)
                    result += ", ";
            }
            return result;
        }

        private static Object _logLock = 0;
        private static Queue<string> DebugLogQueue = new Queue<string>();
        internal static Thread LoggerThread;

        private static void DebugLogger()
        {
            int myPid = Process.GetCurrentProcess().Id;
            DateTime startTime = Process.GetCurrentProcess().StartTime;
            string logFile = Path.Combine(FileManager.DemonBuddyPath, "Logs", myPid + " " + startTime.ToString("yyyy-MM-dd HH.mm") + " TrinityDebug.txt");
            List<string> messages = new List<string>();

            while (true)
            {
                try
                {
                    messages.Clear();
                    lock (_logLock)
                    {
                        while (DebugLogQueue.Count > 0)
                        {
                            messages.Add(DebugLogQueue.Dequeue());
                        }
                    }

                    using (StreamWriter w = File.AppendText(logFile))
                    {
                        foreach (string message in messages)
                        {
                            w.WriteLine(message);
                        }
                    }

                    Thread.Sleep(1);
                }
                catch (ThreadAbortException)
                {

                }
                catch (Exception ex)
                {
                    DBLog.Error("Exception in DebugLogger: " + ex.ToString());
                }
            }
        }

        private static void SetupLoggerThread()
        {
            if (LoggerThread == null || (LoggerThread != null && !LoggerThread.IsAlive))
            {
                LoggerThread = new Thread(DebugLogger)
                {
                    IsBackground = true
                };
                DBLog.Info("[Trinity] Starting up Debug Logger");
                LoggerThread.Start();
            }
        }


        public static void LogToTrinityDebug(string message, params object[] args)
        {

            try
            {
                SetupLoggerThread();

                lock (_logLock)
                {
                    if (message.Contains("{c:"))
                        DebugLogQueue.Enqueue(DateTime.Now.ToString("HH:mm:ss.fff") + " " + message + " args: " + args);
                    else
                        DebugLogQueue.Enqueue(DateTime.Now.ToString("HH:mm:ss.fff") + " " + string.Format(message, args));
                }
            }
            catch (Exception ex)
            {
                DBLog.Error("Error in LogToTrinityDebug: " + ex.ToString());
            }
        }
    }
}
