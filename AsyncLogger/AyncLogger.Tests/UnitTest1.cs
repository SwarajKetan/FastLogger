using AsyncLogger;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace AyncLogger.Tests
{
    public class UnitTest1
    {
        [Fact]
        public void ConsoleLoggerTest()
        {

            List<string> storage = new List<string>();

            ILogger logger = new Logger(LogLevel.Verbose, (s) =>
            {
                storage.Add(s);
            }, "StoragteLogger");

            logger.Info("some info");
            logger.Error("some error");
            logger.Warning("some warning");
            logger.Info("some info");
            logger.Error("some error");
            logger.Warning("some warning");
            logger.Info("some info");
            logger.Error("some error");
            logger.Warning("some warning");
            logger.Info("some info");
            logger.Error("some error");
            logger.Warning("some warning");
            logger.Info("some info");
            logger.Error("some error");
            logger.Warning("some warning");
            Task.Run(() => logger.Info("some info")).ConfigureAwait(false);
            logger.Error("some error");
            logger.Warning("some warning");
            logger.Info("some info");
            logger.Error("some error");
            logger.Warning("some warning");
            logger.Error(new Exception("dummy exp"));
            logger.Warning("some | warning");


            using (var tl = new TimeLogger("TestBlock", logger))
            {
                Thread.Sleep(2000);
            }
            logger.Dispose();

#if DEBUG
            var sb = new StringBuilder();
            storage.ForEach(x => sb.AppendLine(x));
            File.WriteAllText(Environment.CurrentDirectory + "\\log.txt", sb.ToString());
#endif


        }
    }
}
