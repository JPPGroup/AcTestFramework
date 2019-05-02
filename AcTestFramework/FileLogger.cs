using System;
using System.IO;
using NLog;

namespace Jpp.AcTestFramework
{
    public class FileLogger : IDisposable
    {
        public enum LogType { TestFixture, Client, Server }

        private const string FILE_NAME = "AcTests_[type].log";
        private readonly Logger _logger;
        private readonly bool _shouldLog;

        public FileLogger(string directory, LogType type, bool shouldLog = false)
        {
            _shouldLog = shouldLog;
            var fileName = FILE_NAME.Replace("[type]", type.ToString());
            var filePath = Path.Combine(directory ?? throw new InvalidOperationException(), fileName);

            var config = new NLog.Config.LoggingConfiguration();
            var logfile = new NLog.Targets.FileTarget(type.ToString()) { FileName = filePath, KeepFileOpen = false, ArchiveAboveSize = 1000000, MaxArchiveFiles = 10 };
            config.AddRule(LogLevel.Trace, LogLevel.Fatal, logfile);
            LogManager.Configuration = config;
            _logger = LogManager.GetCurrentClassLogger();
        }

        public void Entry(string message)
        {
            if (_shouldLog) _logger.Info($"{message}");
        }
        
        public void Exception(Exception exception)
        {
            if (_shouldLog) _logger.Error(exception);
        }

        public void Dispose()
        {
            LogManager.Flush();
        }

        ~FileLogger()
        {
            LogManager.Flush();
        }
    }
}
