using System;

namespace Cofftea.IO
{
    public sealed class Logger : CoffeeString
    {
        static object printLock = new object();
        public void LogInfo(string message)
        {
            Log(ConsoleColor.Green, "info", ConsoleColor.White, message);
        }
        public void LogWarning(string message)
        {
            Log(ConsoleColor.Yellow, "warn", ConsoleColor.White, message);
        }
        public void LogError(string message)
        {
            Log(ConsoleColor.DarkRed, "error", ConsoleColor.Red, message);
        }
        void Log(ConsoleColor prefixColor, string prefix, ConsoleColor color, string message)
        {
            lock (printLock) {
                Write(DateTime.Now.ToLongTimeString(), ConsoleColor.DarkGray);
                Write(" [", ConsoleColor.Gray);
                Write(prefix, prefixColor);
                Write("]> ", ConsoleColor.Gray);
                WriteLine(message, color);
            }
        }
    }
}
