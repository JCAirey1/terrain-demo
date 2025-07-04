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
        private static string logFilePath;

        public TerrainLogger()
        {
            SetLogPath();
        }

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

        private static void Write(string level, string message)
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            string formatted = $"[{timestamp}] [{level}] {message}";

            DebugOutput(level, formatted);
            File.AppendAllText(logFilePath, formatted + Environment.NewLine);
        }

        private static void SetLogPath()
        {
            string logsDir = Path.Combine(Application.dataPath, "../Logs");
            Directory.CreateDirectory(logsDir);

            string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
            logFilePath = Path.Combine(logsDir, $"log_{timestamp}.txt");

            Log("Logger initialized.");
        }

        private static void DebugOutput(string level, string message)
        {
            if(string.IsNullOrEmpty(logFilePath))
            {
                SetLogPath();
            }

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
