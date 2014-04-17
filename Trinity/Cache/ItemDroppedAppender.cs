using System;
using System.Collections.Concurrent;
using System.IO;
using System.Text;
using System.Threading;
using Trinity.Technicals;

namespace Trinity.Cache
{
    public class ItemDroppedAppender : IDisposable
    {
        public ItemDroppedAppender()
        {
            logItemQueue = new ConcurrentQueue<string>();

            droppedItemLogPath = Path.Combine(FileManager.TrinityLogsPath, "ItemsDropped.csv");

            CheckHeader();

            QueueThread = new Thread(QueueWorker)
            {
                IsBackground = true,
                Priority = ThreadPriority.Lowest
            };
            QueueThread.Start();

        }

        private void CheckHeader()
        {
            bool writeHeader = !File.Exists(droppedItemLogPath);

            if (writeHeader)
            {
                logItemQueue.Enqueue("ActorSNO,GameBalanceID,Name,InternalName,DBBaseType,DBItemType,TBaseType,TItemType,Quality,Level,Pickup\n");
            }
        }
        public void Dispose()
        {
            try
            {
                
                if (_logWriter != null)
                    _logWriter.Dispose();

                if (_fileStream != null)
                    _fileStream.Dispose(); 

            }
            catch { }
            _logWriter = null;

            try
            {
                if (QueueThread != null)
                    QueueThread.Abort();
            }
            catch { }
            QueueThread = null;
        }

        ~ItemDroppedAppender()
        {
            Dispose();
        }

        private Mutex mutex = new Mutex(false, "ItemDroppedMutex");

        private static ItemDroppedAppender _Instance;
        public static ItemDroppedAppender Instance { get { return _Instance ?? (_Instance = new ItemDroppedAppender()); } }

        private StreamWriter _logWriter;
        private FileStream _fileStream;

        private ConcurrentQueue<string> logItemQueue;

        private Thread QueueThread;
        private string droppedItemLogPath;

        internal void AppendDroppedItem(PickupItem item)
        {
            bool pickupItem;
            CacheData.PickupItem.TryGetValue(item.RActorGUID, out pickupItem);

            StringBuilder sb = new StringBuilder();

            sb.Append(FormatCSVField(item.ActorSNO));
            sb.Append(FormatCSVField(item.BalanceID));
            sb.Append(FormatCSVField(item.Name));
            sb.Append(FormatCSVField(item.InternalName));
            sb.Append(FormatCSVField(item.DBBaseType.ToString()));
            sb.Append(FormatCSVField(item.DBItemType.ToString()));
            sb.Append(FormatCSVField(item.TBaseType.ToString()));
            sb.Append(FormatCSVField(item.TType.ToString()));
            sb.Append(FormatCSVField(item.Quality.ToString()));
            sb.Append(FormatCSVField(item.Level));
            sb.Append(FormatCSVField(pickupItem));
            sb.Append("\n");

            logItemQueue.Enqueue(sb.ToString());

        }

        private void QueueWorker()
        {
            const int bufferSize = 65536;
            const int maxAttempts = 50;

            while (true)
            {
                try
                {
                    CheckHeader(); 
                    
                    if (_fileStream == null)
                        _fileStream = File.Open(droppedItemLogPath, FileMode.OpenOrCreate, FileAccess.Write, FileShare.ReadWrite);

                    if (_logWriter == null)
                        _logWriter = new StreamWriter(_fileStream, System.Text.Encoding.UTF8, bufferSize);

                    string queueItem = "";
                    while (logItemQueue.TryDequeue(out queueItem))
                    {
                        bool success = false;
                        int attempts = 0;
                        while (!string.IsNullOrWhiteSpace(queueItem) && !success && attempts <= maxAttempts)
                        {
                            try
                            {
                                mutex.WaitOne();

                                attempts++;
                                _logWriter.Write(queueItem);
                                _logWriter.Flush();
                                _fileStream.Flush();
                                success = true;
                                queueItem = "";

                            }
                            catch (Exception ex)
                            {
                                Logger.LogError("Error in LogDroppedItems QueueWorker: " + ex.Message);
                            }
                            finally
                            {
                                mutex.ReleaseMutex();
                            }
                        }
                    }
                }
                catch (ThreadAbortException)
                {
                    // ssh...
                }
                catch (Exception ex)
                {
                    Logger.LogError("Error in LogDroppedItems QueueWorker: " + ex.Message);
                }

                Thread.Sleep(10);
            }
        }

        private static string FormatCSVField(DateTime time)
        {
            return String.Format("\"{0:yyyy-MM-ddTHH:mm:ss.ffff}\",", time.ToLocalTime());
        }

        private static string FormatCSVField(string text)
        {
            return String.Format("\"{0}\",", text);
        }

        private static string FormatCSVField(int number)
        {
            return String.Format("\"{0}\",", number);
        }

        private static string FormatCSVField(double number)
        {
            return String.Format("\"{0:0}\",", number);
        }

        private static string FormatCSVField(bool value)
        {
            return String.Format("\"{0}\",", value);
        }
    }
}
