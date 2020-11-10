

/// <summary>
/// Author : https://github.com/SwarajKetan/
/// </summary>
namespace AsyncLogger
{
    using System;
    using System.Diagnostics;
    using System.Runtime.CompilerServices;
    /// <summary>
    /// Usage:
    /// using(var tl = new TimeLogger()
    /// </summary>
    public sealed class TimeLogger : IDisposable
    {
        private Stopwatch Stopwatch { get; }
        private string ExecutionBlockName { get; }
        private string CallerMemberName { get; }
        private string CallerFilePath { get; }
        private int CallerLineNo { get; }
        private ILogger Logger { get; }


        public TimeLogger(string executionBlockName, ILogger logger,
            [CallerMemberName] string callerMemberName = "", [CallerFilePath] string callerFilePath = "", [CallerLineNumber] int callerLineNo = 0)
        {
            this.Logger = logger;
            this.ExecutionBlockName = executionBlockName;
            this.Stopwatch = new Stopwatch();
            this.CallerMemberName = callerMemberName;
            this.CallerFilePath = callerFilePath;
            this.CallerLineNo = callerLineNo;
            this.Stopwatch.Restart();
        }

        #region IDisposable
        private bool disposed;
        private void Dispose(bool disposing)
        {
            if (!disposing) return;

            if (!disposed)
            {
                this.Stopwatch.Stop();
                this.Logger.Time($"{ this.ExecutionBlockName}", this.Stopwatch.ElapsedMilliseconds, this.CallerMemberName, this.CallerFilePath, this.CallerLineNo);
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
