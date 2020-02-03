using System;
using System.IO;
using NLog;

namespace Jpp.AcTestFramework
{
    public class FileLogger : IDisposable
    {
        public enum LogType { TestFixture, Server, Runner }

        private const string FILE_NAME = "AcTests.[TYPE].log";
        private readonly Logger _logger;
        private readonly bool _shouldLog;

        public FileLogger(string directory, LogType type, bool shouldLog = false)
        {
            _shouldLog = shouldLog;
            var file = FILE_NAME.Replace("[TYPE]", type.ToString());
            var filePath = Path.Combine(directory ?? throw new InvalidOperationException(), file);

            var config = LogManager.Configuration ?? new NLog.Config.LoggingConfiguration();

            var target = new NLog.Targets.FileTarget
            {
                Name = type.ToString(),
                FileName = filePath, 
                KeepFileOpen = false, 
                ArchiveAboveSize = 1000000, 
                MaxArchiveFiles = 10
            };

            config.AddRule(LogLevel.Trace, LogLevel.Fatal, target, type.ToString());
            LogManager.Configuration = config;
            _logger = LogManager.GetLogger(type.ToString());
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
