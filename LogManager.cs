using System;
using System.IO;

namespace StardewModdingAPI
{
    public static class LogManager
    {
        private static string logFilePath;

        public static void SetupLog(string path)
        {
            logFilePath = path;
            Console.SetOut(new DualWriter(logFilePath));
        }

        public static void LogError(string message, Exception ex)
        {
            Console.WriteLine($"Error: {message}");
            Console.WriteLine($"Exception: {ex.Message}");
            Console.WriteLine($"Stack Trace: {ex.StackTrace}");
        }
    }
}
