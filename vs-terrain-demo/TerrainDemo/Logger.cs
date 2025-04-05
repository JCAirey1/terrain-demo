using System;
using log4net;
using log4net.Appender;
using log4net.Core;
using log4net.Layout;
using log4net.Repository.Hierarchy;

namespace TerrainDemo
{

    public enum LogType
    {
        Debug,
        Error
    }

    public class Logger
    {
        private static readonly ILog log = LogManager.GetLogger(typeof(Logger));

        public Logger()
        {
            RollingFileAppender rollingFileAppender = new RollingFileAppender
            {
                File = "logz.txt",                        // Log file path
                AppendToFile = true,                      // Append or overwrite log file
                MaximumFileSize = "5MB",                  // Max file size before rolling over
                MaxSizeRollBackups = 10,                  // Max number of backup files to keep
                Layout = new PatternLayout("%date [%thread] %-5level %logger - %message%newline") // Log format
            };

            // Step 2: Create the root logger and set its level
            Hierarchy hierarchy = (Hierarchy)LogManager.GetRepository();
            hierarchy.Root.Level = Level.Info; // Set the minimum level to INFO
            hierarchy.Root.AddAppender(rollingFileAppender); // Add the appender to the root logger

            // Step 3: Initialize log4net (activating the configuration)
            log4net.Config.BasicConfigurator.Configure(hierarchy);
        }

        public void LogInfo(string message)
        {
            log.Info(message);
        }

        public void LogDebug(string message)
        {
            log.Debug(message);
        }

        public void LogError(string message)
        {
            log.Error(message);
        }

        public void LogError(Exception ex)
        {
            log.Error(ex.ToString());
        }
    }
}
