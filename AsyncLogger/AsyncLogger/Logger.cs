
/// <summary>
/// Author : https://github.com/SwarajKetan/
/// </summary>
namespace AsyncLogger
{
    using System;
    using System.Collections.Concurrent;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;

    public interface ILogger : IDisposable
    { 
        LogLevel LogLevel { get; }

        void Info(string message,
            [CallerMemberName]string callerMemberName = "", [CallerFilePath]string callerFilePath = "", [CallerLineNumber]int callerLineNo = 0);

        void Debug(string message,
            [CallerMemberName]string callerMemberName = "", [CallerFilePath]string callerFilePath = "", [CallerLineNumber]int callerLineNo = 0);

        void Warning(string message,
            [CallerMemberName]string callerMemberName = "", [CallerFilePath]string callerFilePath = "", [CallerLineNumber]int callerLineNo = 0);

        void Error(string message,
            [CallerMemberName]string callerMemberName = "", [CallerFilePath]string callerFilePath = "", [CallerLineNumber]int callerLineNo = 0);

        void Error(Exception ex, string message = "",
            [CallerMemberName]string callerMemberName = "", [CallerFilePath]string callerFilePath = "", [CallerLineNumber]int callerLineNo = 0);

        void Time(string blockName, long elapsedMiliSecods, string callerMemberName, string callerFilePath, int callerLineNo);
    }
    public class Logger : ILogger
    { 
        public LogLevel LogLevel { get; } 

        public volatile int LogId  = 0;

        private BlockingCollection<string> MessageQueue { get; set; } = new BlockingCollection<string>();

        private Action<string> WriterHook { get; set; }
        private Task WriterTask { get; set; }

        private string LogName { get; }

        /// <summary>
        /// Inject using DI, Container controlled Singleton object
        /// </summary>
        /// <param name="logLevel">Logger is dependent upon trace level</param>
        /// <param name="writerHook">Hook to write log into file or DB</param>
        public Logger(LogLevel logLevel, Action<string> writerHook, string logName)
        {
            this.LogLevel = logLevel;
            this.WriterHook = writerHook;
            this.LogName = logName;
            this.WriterTask = FireupWriterTask();
            this.Info($"Started logging.");
            this.Info("TimeStamp|ProcessId|ThreadId|LogName|LogType|Message|CallerMemberName|CallerFilePath|CallerLineNo");
        }

        #region ILogger
        public void Info(string message, [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNo = 0)
        {
            if (this.LogLevel < LogLevel.Information) return;
            this.Log(Process.GetCurrentProcess().Id, Thread.CurrentThread.ManagedThreadId, DateTime.UtcNow,
                "Info", message,
                callerMemberName, callerFilePath, callerLineNo);
        }

        public void Debug(string message, [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNo = 0)
        {
            if (this.LogLevel < LogLevel.Verbose) return;

            this.Log(Process.GetCurrentProcess().Id, Thread.CurrentThread.ManagedThreadId, DateTime.UtcNow,
                "Debug", message,
               callerMemberName, callerFilePath, callerLineNo);
        }

        public void Warning(string message, [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNo = 0)
        {
            if (this.LogLevel < LogLevel.Warning) return;

            this.Log(Process.GetCurrentProcess().Id, Thread.CurrentThread.ManagedThreadId, DateTime.UtcNow,
                "Warning", message,
               callerMemberName, callerFilePath, callerLineNo);
        }

        public void Error(string message, [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNo = 0)
        {
            if (this.LogLevel < LogLevel.Critical) return;

            this.Log(Process.GetCurrentProcess().Id, Thread.CurrentThread.ManagedThreadId, DateTime.UtcNow,
                "Error", message,
               callerMemberName, callerFilePath, callerLineNo);
        }

        public void Error(Exception ex, string message = "", [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNo = 0)
        {
            if (this.LogLevel < LogLevel.Critical) return;

            this.Log(Process.GetCurrentProcess().Id, Thread.CurrentThread.ManagedThreadId, DateTime.UtcNow,
                "Error", FormatException(message, ex),
               callerMemberName, callerFilePath, callerLineNo);

            static string FormatException(string message, Exception ex)
            {
                var sb = new StringBuilder();
                sb.Append(message);
                sb.Append($"Message : {ex?.Message} ");
                sb.Append($"StackTrace : {ex?.StackTrace} ");
                sb.Append($"InnerException : {ex?.InnerException?.ToString()}");
                return sb.ToString();
            }
        }

        public void Time(string blockName, long elapsedMiliSecods, string callerMemberName, string callerFilePath, int callerLineNo)
        {
            if (this.LogLevel < LogLevel.Verbose) return;
            this.Log( Process.GetCurrentProcess().Id, Thread.CurrentThread.ManagedThreadId, DateTime.UtcNow,
                "Debug", $"{blockName} took {elapsedMiliSecods} ms",
               callerMemberName, callerFilePath, callerLineNo);
        }

        #endregion
        internal void Log(int processId, int threadId, DateTime timeStamp, string type, string message, string callerMemberName, string callerFilePath, int callerLineNo)
        {
            MessageQueue.TryAdd(Format(timeStamp, processId, threadId, message?.Replace(spacer, "[pipe]"), type, callerMemberName, callerFilePath, callerLineNo));
        }

        public const string spacer = "|";
        private string Format(DateTime timeStamp, int processId, int threadId, string message, string type, string callerMemberName, string callerFilePath, int callerLineNo)
        {
            return $"{timeStamp}{spacer}{processId}{spacer}{threadId}{spacer}{this.LogName}{spacer}{type}{spacer}{message}{spacer}{callerMemberName}{spacer}{callerFilePath}{spacer}{callerLineNo}";
        }
        private async Task FireupWriterTask()
        {
            await Task.Run(() =>
            {
                while (!this.MessageQueue.IsAddingCompleted || MessageQueue.Count > 0)
                {
                    if (MessageQueue.TryTake(out string msg, Timeout.Infinite))
                    {
                        try
                        {
                            WriterHook(msg);
                        }
                        catch (Exception) { /* no throw*/ }
                    }
                }
            });
        }

        #region IDisposable
        bool disposed;
        public void Dispose(bool disposing)
        {
            if (!disposing) return;
            if (!disposed)
            {
                this.Info($"Stopped logging.");
                this.MessageQueue?.CompleteAdding();
                disposed = true;
            }
        }
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        } 
        #endregion
    }
}
