using System;
using System.IO;
using UnityEngine;

namespace TerrainDemo
{
    public enum LogType
    {
        Debug,
        Error
    }

    public class TerrainLogger
    {
        private static string _logFilePath;

        public TerrainLogger() { }

        public static void Log(string message)
        {
            Write("INFO", message);
        }

        public static void Warn(string message)
        {
            Write("WARN", message);
        }

        public static void Error(string message)
        {
            Write("ERROR", message);
        }

        private static string LogFilePath
        {
            get
            {
                if(string.IsNullOrEmpty(_logFilePath))
                {
                    SetLogPath();
                }

                return _logFilePath;
            }
        }

        private static void Write(string level, string message)
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            string formatted = $"[{timestamp}] [{level}] {message}";

            DebugOutput(level, formatted);

            try
            {
                File.AppendAllText(LogFilePath, formatted + Environment.NewLine);
            } catch { }
        }

        private static void SetLogPath()
        {
            string logsDir = Path.Combine(Application.dataPath, "../Logs");
            Directory.CreateDirectory(logsDir);

            string daystamp = DateTime.Now.ToString("yyyy-MM-dd");
            _logFilePath = Path.Combine(logsDir, $"log_{daystamp}.txt");
        }

        private static void DebugOutput(string level, string message)
        {
            switch (level)
            {
                case "ERROR":
                    Debug.LogError(message);
                    break;
                case "WARN":
                    Debug.LogWarning(message);
                    break;
                default:
                    Debug.Log(message);
                    break;
            }
        }
    }
}
